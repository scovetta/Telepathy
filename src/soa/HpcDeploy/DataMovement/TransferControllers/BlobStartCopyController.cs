//------------------------------------------------------------------------------
// <copyright file="BlobStartCopyController.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Blob start copying code.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferControllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Blob.Protocol;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.Hpc.Azure.DataMovement.Extensions;
    using Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers;

    /// <summary>
    /// Blob start copying code.
    /// </summary>
    internal class BlobStartCopyController : TransferControllerBase
    {
        /// <summary>
        /// Transfer entry to store transfer information.
        /// </summary>
        private BlobTransferFileTransferEntry transferEntry;

        /// <summary>
        /// Keeps track of the internal state-machine state.
        /// </summary>
        private volatile State state;

        /// <summary>
        /// Source uri object.
        /// </summary>
        private Uri sourceUri;

        /// <summary>
        /// Source blob object.
        /// </summary>
        private ICloudBlob sourceBlob;

        /// <summary>
        /// Destination blob object.
        /// </summary>
        private ICloudBlob destinationBlob;

        /// <summary>
        /// Target container object.
        /// </summary>
        private CloudBlobContainer destinationContainer;

        /// <summary>
        /// Path under the container of target blob object.
        /// </summary>
        private string destinationBlobName;

        /// <summary>
        /// Whether to monitor the blobcopy process after starting copying.
        /// </summary>
        private bool monitorProcess;

        /// <summary>
        /// Whether to remove source file after finishing transfer.
        /// </summary>
        private bool moveSource;

        /// <summary>
        /// Callback delegate called when transfer starts.
        /// </summary>
        private Action<object> startCallback;

        /// <summary>
        /// Progress callback delegate called by transfer status tracker.
        /// </summary>
        private Action<object, double, double> progressCallback;
        
        /// <summary>
        /// Finish callback delegate called when start copy request or transfer finished.
        /// If user doesn't requir to monitor the copy status, this will be called when 
        /// start copy request finished, otherwise, this will be called after the whole transfer
        /// finished.
        /// </summary>
        private Action<object, string, Exception> finishCallback;

        /// <summary>
        /// Lock to protect finishCallback.
        /// </summary>
        private object finishCallbackLock = new object();

        /// <summary>
        /// Indicates whether we got the destination blob object.
        /// </summary>
        private bool gotDestinationBlob;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStartCopyController"/> class.
        /// </summary>
        /// <param name="manager">Manager object which creates this object.</param>
        /// <param name="transferEntry">Transfer entry to store transfer information.</param>
        /// <param name="sourceUri">Source uri to StartCopy from. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="sourceBlob">Source blob to StartCopy from. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="destinationContainer">Target container object.</param>
        /// <param name="destinationBlobName">Path under the container of target blob object.</param>
        /// <param name="monitorProcess">Whether to monitor the blobcopy process.</param>
        /// <param name="moveSource">Indicates whether to remove source file after finishing transfer.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        internal BlobStartCopyController(
            BlobTransferManager manager,
            BlobTransferFileTransferEntry transferEntry,
            Uri sourceUri,
            ICloudBlob sourceBlob,
            CloudBlobContainer destinationContainer,
            string destinationBlobName,
            bool monitorProcess,
            bool moveSource,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, string, Exception> finishCallback,
            object userData)
        {
            if (null == manager || null == transferEntry)
            {
                throw new ArgumentNullException(null == manager ? "manager" : "transferEntry");
            }

            if (null == sourceUri && null == sourceBlob)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ProvideExactlyOneParameterBothNullException,
                    "sourceUri",
                    "sourceBlob"));
            }

            if (null != sourceUri && null != sourceBlob)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ProvideExactlyOneParameterBothProvidedException,
                    "sourceUri",
                    "sourceBlob"));
            }

            if (string.IsNullOrEmpty(destinationBlobName) || null == destinationContainer)
            {
                throw new ArgumentNullException(null == destinationBlobName ? "destinationBlobName" : "destinationContainer");
            }

            if (moveSource)
            {
                if (null == sourceBlob)
                {
                    throw new ArgumentNullException("sourceBlob", Resources.CannotMoveSourceIfSourceBlobIsNullException);
                }

                if (!monitorProcess)
                {
                    throw new ArgumentException(Resources.CannotMoveSourceIfMonitoringIsTurnedOffException, "moveSource");
                }
            }
            else
            {
                if (BlobTransferEntryStatus.RemoveSource == transferEntry.Status)
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.InvalidInitialEntryStatusWhenMoveSourceIsOffException,
                        transferEntry.Status));
                }
            }

            this.Manager = manager;
            this.transferEntry = transferEntry;

            this.sourceUri = sourceUri ?? sourceBlob.Uri;
            this.sourceBlob = sourceBlob;
            this.destinationBlobName = destinationBlobName;
            this.destinationContainer = destinationContainer;

            this.monitorProcess = monitorProcess;
            this.moveSource = moveSource;

            this.startCallback = startCallback;
            this.progressCallback = progressCallback;
            this.finishCallback = finishCallback;

            this.UserData = userData;

            this.SetInitialStatus();
        }
        
        /// <summary>
        /// Internal state values.
        /// </summary>
        private enum State
        {
            FetchSourceAttributes,
            GetDestinationBlob,
            StartCopy,
            QueueMonitor,
            Finished,
            Error,
        }

        /// <summary>
        /// Gets a value indicating whether this transfer item may leads
        /// additional controllers to be added to the queue.
        /// </summary>
        public override bool CanAddController
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this transfer item may leads
        /// additional monitors to be added to the queue.
        /// </summary>
        public override bool CanAddMonitor
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a work item delegate. Each work item represents a single 
        /// asynchronous operation or a single asynchronous continuation.
        /// If no work is currently available returns null.
        /// </summary>
        /// <returns>Work item delegate.</returns>
        public override Action<Action<ITransferController, bool>> GetWork()
        {
            if (!this.HasWork)
            {
                return null;
            }

            switch (this.state)
            {
                case State.FetchSourceAttributes:
                    return this.GetFetchSourceAttributesAction();
                case State.GetDestinationBlob:
                    return this.GetGetDestinationBlobAction();
                case State.StartCopy:
                    return this.GetStartCopyAction();
                case State.QueueMonitor:
                    return this.GetQueueMonitorAction();
                case State.Finished:
                case State.Error:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Sets the state of the controller to Error, while recording
        /// the last occured exception and setting the HasWork and 
        /// IsFinished fields.
        /// </summary>
        /// <param name="ex">Exception to record.</param>
        /// <param name="callbackState">Callback state to finish after 
        /// setting error state.</param>
        protected override void SetErrorState(Exception ex, CallbackState callbackState)
        {
            Debug.Assert(
                this.state != State.Finished,
                "SetErrorState called, while controller already in Finished state");
            Debug.Assert(
                null != callbackState,
                "CallbackState expected");

            this.state = State.Error;
            this.HasWork = false;

            this.FinishCallbackHandler(null, ex);

            bool finished = this.SetFinishedAndPostWork();
            callbackState.CallFinish(this, finished);
        }

        /// <summary>
        /// Taken from Microsoft.WindowsAzure.Storage.Core.Util.HttpUtility: Parse the http query string.
        /// </summary>
        /// <param name="query">Http query string.</param>
        /// <returns>A dictionary of query pairs.</returns>
        private static Dictionary<string, string> ParseQueryString(string query)
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            if (query == null || query.Length == 0)
            {
                return retVal;
            }

            // remove ? if present
            if (query.StartsWith("?"))
            {
                query = query.Substring(1);
            }

            string[] valuePairs = query.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string vp in valuePairs)
            {
                int equalDex = vp.IndexOf("=");
                if (equalDex < 0)
                {
                    retVal.Add(Uri.UnescapeDataString(vp), null);
                    continue;
                }

                string key = vp.Substring(0, equalDex);
                string value = vp.Substring(equalDex + 1);

                retVal.Add(Uri.UnescapeDataString(key), Uri.UnescapeDataString(value));
            }

            return retVal;
        }

        private void SetInitialStatus()
        {
            switch (this.transferEntry.Status)
            {
                case BlobTransferEntryStatus.NotStarted:
                    this.transferEntry.Status = BlobTransferEntryStatus.Transfer;
                    break;
                case BlobTransferEntryStatus.Transfer:
                    break;
                case BlobTransferEntryStatus.Monitor:
                    break;
                case BlobTransferEntryStatus.RemoveSource:
                    break;
                case BlobTransferEntryStatus.Finished:
                default:
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.InvalidInitialEntryStatusForControllerException,
                        this.transferEntry.Status,
                        this.GetType().Name));
            }

            this.SetHasWorkAfterStatusChanged(true);
        }

        private bool ChangeStatus()
        {
            Debug.Assert(
                this.transferEntry.Status != BlobTransferEntryStatus.Finished,
                "ChangeStatus called, while controller already in Finished state");

            if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
            {
                if (this.monitorProcess)
                {
                    this.transferEntry.Status = BlobTransferEntryStatus.Monitor;
                    this.SetHasWorkAfterStatusChanged(false);
                    return this.PostWork();
                }
                else
                {
                    this.transferEntry.Status = BlobTransferEntryStatus.Finished;
                    return this.SetFinished();
                }
            }
            else if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
            {
                return this.SetFinishedStartCopy();
            }
            else
            {
                Debug.Fail("We should never be here");
                return this.PostWork();
            }
        }

        private void SetHasWorkAfterStatusChanged(bool initSet)
        {
            if (initSet || (BlobTransferEntryStatus.Transfer == this.transferEntry.Status))
            {
                if (null == this.sourceBlob)
                {
                    this.state = State.GetDestinationBlob;
                }
                else
                {
                    this.state = State.FetchSourceAttributes;
                }
            }
            else if ((BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
                || (BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status))
            {
                this.state = State.QueueMonitor;
            }
            else
            {
                Debug.Fail("We should never be here");
            }

            this.HasWork = true;
        }

        private Action<Action<ITransferController, bool>> GetFetchSourceAttributesAction()
        {
            Debug.Assert(
                this.state == State.FetchSourceAttributes,
                "GetFetchSourceAttributesAction called, but state isn't FetchSourceAttributes");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                CallbackState callbackState = new CallbackState
                {
                    FinishDelegate = finishDelegate
                };

                if (!this.StartCallbackHandler(callbackState))
                {
                    return;
                }

                BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.FetchAttributes);
                OperationContext operationContext = new OperationContext()
                {
                    ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                };

                try
                {
                    this.CancellationHandler.RegisterCancellableAsyncOper(
                        delegate
                        { 
                            return this.sourceBlob.BeginFetchAttributes(
                                null,
                                requestOptions,
                                operationContext,
                                this.FetchSourceAttributesCallback,
                                callbackState);
                        });
                }
                catch (Exception e)
                {
                    this.HandleFetchSourceAttributesException(e, callbackState);
                    return;
                }
            };
        }

        private void FetchSourceAttributesCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.FetchSourceAttributes,
                "FetchSourceAttributesCallback called, but state isn't FetchSourceAttributes");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.sourceBlob.EndFetchAttributes(asyncResult);
            }
            catch (Exception e)
            {
                this.HandleFetchSourceAttributesException(e, callbackState);
                return;
            }

            if (string.IsNullOrEmpty(this.transferEntry.ETag))
            {
                this.transferEntry.ETag = this.sourceBlob.Properties.ETag;
            }
            else if (!this.transferEntry.ETag.Equals(this.sourceBlob.Properties.ETag, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!this.RetransferModifiedCallbackHandler(this.sourceBlob.Name, callbackState))
                {
                    return;
                }
                else
                {
                    this.transferEntry.ETag = this.sourceBlob.Properties.ETag;
                    this.transferEntry.CopyId = null;
                    this.transferEntry.Status = BlobTransferEntryStatus.Transfer;
                }
            }

            if (BlobType.Unspecified == this.sourceBlob.Properties.BlobType)
            {
                this.SetErrorState(
                    new InvalidOperationException(
                        Resources.FailedToGetBlobTypeException),
                        callbackState);

                return;
            }
            else if (BlobType.BlockBlob != this.sourceBlob.Properties.BlobType && 
                BlobType.PageBlob != this.sourceBlob.Properties.BlobType)
            {
                throw new InvalidOperationException(
                    Resources.OnlySupportTwoBlobTypesException);
            }

            if (!this.ProgressCallbackHandler(callbackState))
            {
                return;
            }

            this.state = State.GetDestinationBlob;

            this.HasWork = true;
            callbackState.CallFinish(this, this.PostWork());
        }

        private void HandleFetchSourceAttributesException(Exception e, CallbackState callbackState)
        {
            StorageException se = e as StorageException;

            if (null != se)
            {
                // Getting a storage exception is expected if the blob doesn't
                // exist. For those cases that indicate the blob doesn't exist
                // we will set a specific error state.
                if (null != se.RequestInformation &&
                    se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    this.SetErrorState(
                        new InvalidOperationException(
                            Resources.SourceBlobDoesNotExistException),
                            callbackState);

                    return;
                }
                else
                {
                    this.SetErrorState(se, callbackState);
                    return;
                }
            }
            else
            {
                this.SetErrorState(e, callbackState);
                return;
            }
        }

        private Action<Action<ITransferController, bool>> GetGetDestinationBlobAction()
        {
            Debug.Assert(
                this.state == State.GetDestinationBlob,
                "GetGetDestinationBlobAction called, but state isn't GetDestinationBlob");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };

                if (!this.StartCallbackHandler(callbackState))
                {
                    return;
                }

                BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.GetBlobReferenceFromServer);
                OperationContext operationContext = new OperationContext()
                {
                    ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                };

                try
                {
                    this.CancellationHandler.RegisterCancellableAsyncOper(
                        delegate
                        {
                            return this.destinationContainer.BeginGetBlobReferenceFromServer(
                                this.destinationBlobName,
                                null,
                                requestOptions,
                                operationContext,
                                this.GetDestinationBlobCallback,
                                callbackState);
                        });
                }
                catch (Exception e)
                {
                    this.HandleGetDestinationBlobResult(e, callbackState);
                    return;
                }
            };
        }

        private void GetDestinationBlobCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.GetDestinationBlob,
                "GetDestinationBlobCallback called, but state isn't GetDestinationBlob");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.destinationBlob = this.destinationContainer.EndGetBlobReferenceFromServer(asyncResult);
            }
            catch (Exception e)
            {
                this.HandleGetDestinationBlobResult(e, callbackState);
                return;
            }

            this.HandleGetDestinationBlobResult(null, callbackState);
        }

        private void HandleGetDestinationBlobResult(Exception e, CallbackState callbackState)
        {
            bool existingBlob = true;

            if (null != e)
            {
                StorageException se = e as StorageException;

                // Getting a storage exception is expected if the blob doesn't
                // exist. In this case we won't error out, but set the 
                // existingBlob flag to false to indicate we're uploading
                // a new blob instead of overwriting an existing blob.
                if (null != se &&
                    null != se.RequestInformation &&
                    se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                {
                    existingBlob = false;
                }
                else
                {
                    this.SetErrorState(e, callbackState);
                    return;
                }
            }

            if (existingBlob)
            {
                if (null != this.sourceBlob && this.sourceBlob.Properties.BlobType != this.destinationBlob.Properties.BlobType)
                {
                    this.SetErrorState(
                        new InvalidOperationException(
                            BlobType.PageBlob == this.sourceBlob.Properties.BlobType ?
                                Resources.CannotOverwriteBlockBlobWithPageBlobException :
                                Resources.CannotOverwritePageBlobWithBlockBlobException),
                        callbackState);
                    return;
                }
                
                if (BlobExtensions.Equals(this.sourceBlob, this.destinationBlob))
                {
                    this.SetErrorState(
                        new InvalidOperationException(
                            Resources.SourceAndDestinationLocationCannotBeEqualException),
                        callbackState);
                    return;
                }

                // If destination file exists, query user whether to overwrite it.
                if (null != this.Manager.TransferOptions.OverwritePromptCallback)
                {
                    if (!this.OverwritePromptCallbackHandler(
                        this.sourceUri.ToString(), 
                        this.destinationBlob.Uri.ToString(), 
                        callbackState))
                    {
                        return;
                    }
                }

                if ((BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
                    && string.IsNullOrEmpty(this.transferEntry.CopyId))
                {
                    this.SetErrorState(
                        new InvalidOperationException(
                            Resources.RestartableInfoCorruptedException),
                        callbackState);

                    return;
                }
            }
            else
            {
                if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
                {
                    this.transferEntry.Status = BlobTransferEntryStatus.Transfer;
                    this.transferEntry.CopyId = null;
                }

                // Destination BlobType is determined by Azure Storage. For source blobs inside Azure Storage, 
                // a copy operation preserves the blob type; for those outside, they will be copied to block blobs.
                // However, if the destinatin blob is non-existent, we can use either CloudBlockBlob or CloudPageBlob
                // reference to start the copy operation.
                if (null != this.sourceBlob && BlobType.PageBlob == this.sourceBlob.Properties.BlobType)
                {
                    this.destinationBlob = this.destinationContainer.GetPageBlobReference(this.destinationBlobName);
                }
                else
                {
                    this.destinationBlob = this.destinationContainer.GetBlockBlobReference(this.destinationBlobName);
                }
            }

            this.gotDestinationBlob = true;

            if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status ||
                BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
            {
                this.state = State.QueueMonitor;
            }
            else if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
            {
                this.state = State.StartCopy;
            }
            else
            {
                Debug.Fail("We should never be here");
            }

            if (!this.ProgressCallbackHandler(callbackState))
            {
                return;
            }

            this.HasWork = true;
            callbackState.CallFinish(this, this.PostWork());
        }

        private Action<Action<ITransferController, bool>> GetStartCopyAction()
        {
            Debug.Assert(
                this.state == State.StartCopy,
                "GetStartCopyAction called, but state isn't StartCopy");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.StartCopyFromBlob);
                OperationContext operationContext = new OperationContext()
                {
                    ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                };

                CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };

                try
                {
                    this.CancellationHandler.RegisterCancellableAsyncOper(
                        delegate
                        {
                            if (null == this.sourceBlob)
                            {
                                return ((CloudBlob)this.destinationBlob).BeginStartCopy(
                                    this.sourceUri,
                                    null,
                                    null,
                                    requestOptions,
                                    operationContext,
                                    this.StartCopyCallback,
                                    callbackState);
                            }
                            else
                            {
                                AccessCondition sourceAccessCondition = AccessCondition.GenerateIfMatchCondition(this.sourceBlob.Properties.ETag);

                                // Uri of source blob cannot be used directly since sourceBlob may be a snapshot.
                                if (BlobType.PageBlob == this.sourceBlob.Properties.BlobType)
                                {
                                    return (this.destinationBlob as CloudPageBlob).BeginStartCopy(
                                         this.sourceBlob.AppendSAS() as CloudPageBlob,
                                         sourceAccessCondition,
                                         null,
                                         requestOptions,
                                         operationContext,
                                         this.StartCopyCallback,
                                         callbackState);
                                }
                                else if (BlobType.BlockBlob == this.sourceBlob.Properties.BlobType)
                                {
                                    return (this.destinationBlob as CloudBlockBlob).BeginStartCopy(
                                        this.sourceBlob.AppendSAS() as CloudBlockBlob,
                                        sourceAccessCondition,
                                        null,
                                        requestOptions,
                                        operationContext,
                                        this.StartCopyCallback,
                                        callbackState);
                                }
                                else
                                {
                                    Debug.Fail("We should never get here.");
                                }
                            }

                            return null;
                        });
                }
                catch (Exception e)
                {
                    this.HandleStartCopyResult(e, callbackState);
                    return;
                }
            };
        }

        private void StartCopyCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.StartCopy,
                "StartCopyCallback called, but state isn't StartCopy");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.transferEntry.CopyId = ((CloudBlob)this.destinationBlob).EndStartCopy(asyncResult);
            }
            catch (Exception e)
            {
                this.HandleStartCopyResult(e, callbackState);
                return;
            }

            this.HandleStartCopyResult(null, callbackState);
        }

        private void HandleStartCopyResult(Exception e, CallbackState callbackState)
        {
            if (null != e)
            {
                StorageException se = e as StorageException;

                if (null != se &&
                    BlobErrorCodeStrings.PendingCopyOperation == se.RequestInformation.ExtendedErrorInformation.ErrorCode)
                {
                    BlobRequestOptions requestOptions = new BlobRequestOptions { RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.NoRetry() };
                    OperationContext operationContext = new OperationContext()
                    {
                        ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                    };

                    try
                    {
                        this.CancellationHandler.CheckCancellation();

                        this.destinationBlob.FetchAttributes(
                            null,
                            requestOptions,
                            operationContext);
                    }
                    catch (Exception)
                    {
                        // No more exception is allowed.
                        this.SetErrorState(e, callbackState);
                        return;
                    }

                    Uri sourceUri = this.destinationBlob.CopyState.Source;

                    string baseUriString = sourceUri.GetComponents(UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);

                    string ourBaseUriString = this.sourceUri.GetComponents(UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);

                    DateTimeOffset? baseSnapshot = null;

                    DateTimeOffset? ourSnapshot = null == this.sourceBlob ? null : this.sourceBlob.SnapshotTime;

                    string snapshotString;
                    if (ParseQueryString(sourceUri.Query).TryGetValue("snapshot", out snapshotString))
                    {
                        if (!string.IsNullOrEmpty(snapshotString))
                        {
                            DateTimeOffset snapshotTime;
                            if (DateTimeOffset.TryParse(
                                snapshotString,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AdjustToUniversal,
                                out snapshotTime))
                            {
                                baseSnapshot = snapshotTime;
                            }
                        }
                    }

                    if (!baseUriString.Equals(ourBaseUriString) ||
                        !baseSnapshot.Equals(ourSnapshot))
                    {
                        this.SetErrorState(e, callbackState);
                        return;
                    }

                    if (string.IsNullOrEmpty(this.transferEntry.CopyId))
                    {
                        this.transferEntry.CopyId = this.destinationBlob.CopyState.CopyId;
                    }
                }
                else
                {
                    this.SetErrorState(e, callbackState);
                    return;
                }
            }

            bool finished = this.ChangeStatus();

            if (!this.ProgressCallbackHandler(callbackState))
            {
                return;
            }

            callbackState.CallFinish(this, finished);
        }

        private Action<Action<ITransferController, bool>> GetQueueMonitorAction()
        {
            Debug.Assert(
                this.state == State.QueueMonitor,
                "GetQueueMonitorAction called, but state isn't QueueMonitor");

            this.HasWork = false;

            // Guarantee reference of destination blob is not null.
            if (!this.gotDestinationBlob)
            {
                this.state = State.GetDestinationBlob;
                return this.GetGetDestinationBlobAction();
            }

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                // if this.startCallback has been called in this class, it should be null;
                // this.progress should not be called in this class;
                Action<object, Exception> copyMonitorCallback = null;

                if (this.finishCallback != null)
                { 
                    copyMonitorCallback = delegate(object userData, Exception ex)
                    {
                        this.finishCallback(userData, null, ex);
                    };
                }

                try
                {
                    this.Manager.QueueBlobCopyMonitor(
                        this.transferEntry,
                        null == this.sourceBlob ? this.sourceUri : null,
                        this.sourceBlob,
                        this.destinationBlob,
                        this.moveSource,
                        this.startCallback,
                        this.progressCallback,
                        copyMonitorCallback,
                        this.UserData);
                }
                catch (OperationCanceledException ex)
                {
                    this.SetErrorState(
                        ex,
                        new CallbackState { FinishDelegate = finishDelegate });

                    return;
                }

                finishDelegate(this, this.ChangeStatus());
            };
        }

        private bool SetFinishedStartCopy()
        {
            this.state = State.Finished;
            this.HasWork = false;
            return this.SetFinishedAndPostWork();
        }

        private bool SetFinished()
        {
            this.state = State.Finished;
            this.HasWork = false;

            this.FinishCallbackHandler(this.transferEntry.CopyId, null);

            return this.SetFinishedAndPostWork();
        }

        private bool StartCallbackHandler(CallbackState callbackState)
        {
            return this.CallbackExceptionHandler(
                delegate
                {
                    if (null != this.startCallback)
                    {
                        this.startCallback(this.UserData);
                        this.startCallback = null;
                    }
                },
                callbackState);
        }

        private bool ProgressCallbackHandler(CallbackState callbackState)
        {
            return this.CallbackExceptionHandler(
                delegate
                {
                    if (null != this.progressCallback)
                    {
                        this.progressCallback(this.UserData, 0.0, 0.0);
                    }
                },
                callbackState);
        }

        private bool CallbackExceptionHandler(Action callbackAction, CallbackState callbackState)
        {
            try
            {
                callbackAction();
            }
            catch (Exception ex)
            {
                this.SetErrorState(
                    new BlobTransferCallbackException(Resources.DataMovement_ExceptionFromCallback, ex),
                    callbackState);
                return false;
            }

            return true;
        }

        private void FinishCallbackHandler(string copyId, Exception ex)
        {
            try
            {
                if (null != this.finishCallback)
                {
                    lock (this.finishCallbackLock)
                    {
                        if (null != this.finishCallback)
                        {
                            this.finishCallback(this.UserData, copyId, ex);

                            // Set finish callback to null, to ensure we only call
                            // it once.
                            this.finishCallback = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // We have no way to report exception from finish call back, just ignore it here.
                Debug.Fail(string.Format("An exception was thrown out from finishCallback: {0}", e.StackTrace));
            }
        }        
    }
}