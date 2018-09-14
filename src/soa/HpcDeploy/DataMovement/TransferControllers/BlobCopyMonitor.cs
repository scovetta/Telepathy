//------------------------------------------------------------------------------
// <copyright file="BlobCopyMonitor.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Blob copy monitor code.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferControllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.Hpc.Azure.DataMovement.Extensions;
    using Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers;
    
    /// <summary>
    /// Blob copy monitor code.
    /// </summary>
    internal class BlobCopyMonitor : TransferControllerBase, IDisposable
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
        /// Whether to remove source file after finishing transfer.
        /// </summary>
        private bool moveSource;

        /// <summary>
        /// Transfer status tracker object.
        /// </summary>
        private TransferStatusAndTotalSpeedTracker transferStatusTracker;

        /// <summary>
        /// Timer to signal refresh status.
        /// </summary>
        private Timer statusRefreshTimer;

        /// <summary>
        /// Lock to protect statusRefreshTimer.
        /// </summary>
        private object statusRefreshTimerLock = new object();

        /// <summary>
        /// Finish callback delegate called when transfer finished.
        /// </summary>
        private Action<object, Exception> finishCallback;

        /// <summary>
        /// Callback delegate called when transfer starts.
        /// </summary>
        private Action<object> startCallback;

        /// <summary>
        /// Progress callback delegate called by transfer status tracker.
        /// </summary>
        private Action<object, double, double> progressCallback;

        /// <summary>
        /// Lock to protect finishCallback.
        /// </summary>
        private object finishCallbackLock = new object();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCopyMonitor"/> class.
        /// </summary>
        /// <param name="manager">Manager object which creates this object.</param>
        /// <param name="transferEntry">Transfer entry to store transfer information.</param>
        /// <param name="sourceUri">Source uri. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="sourceBlob">Source blob. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="destinationBlob">Destination blob object.</param>
        /// <param name="moveSource">Indicates whether to remove source file after finishing transfer.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        internal BlobCopyMonitor(
            BlobTransferManager manager,
            BlobTransferFileTransferEntry transferEntry,
            Uri sourceUri,
            ICloudBlob sourceBlob,
            ICloudBlob destinationBlob,
            bool moveSource,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            if (null == manager || null == destinationBlob || null == transferEntry)
            {
                throw new ArgumentNullException(null == manager ? "manager" : null == destinationBlob ? "destinationBlob" : "transferEntry");
            }

            if (string.IsNullOrEmpty(transferEntry.CopyId))
            {
                throw new ArgumentException(Resources.TransferEntryCopyIdCannotBeNullOrEmptyException, "transferEntry");
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

            if (moveSource)
            {
                if (null == sourceBlob)
                {
                    throw new ArgumentNullException("sourceBlob", Resources.CannotMoveSourceIfSourceBlobIsNullException);
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
            this.destinationBlob = destinationBlob;

            this.moveSource = moveSource;

            this.startCallback = startCallback;
            this.progressCallback = progressCallback;
            this.finishCallback = finishCallback;

            this.UserData = userData;

            this.TotalBytes = -1L;
            this.BytesTransferred = -1L;

            this.SetInitialStatus();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BlobCopyMonitor" /> class.
        /// </summary>
        ~BlobCopyMonitor()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Internal state values.
        /// </summary>
        private enum State
        {
            FetchDestinationAttributes,
            DeleteSource,
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
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating the total bytes to be transferred. Default is -1.
        /// </summary>
        public long TotalBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating bytes transferred. Default is -1.
        /// </summary>
        public long BytesTransferred
        {
            get;
            private set;
        }

        /// <summary>
        /// Public dispose method to release all resources owned.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
                case State.FetchDestinationAttributes:
                    return this.GetFetchDestinationAttributesAction();
                case State.DeleteSource:
                    return this.GetDeleteSourceAction();
                case State.Finished:
                case State.Error:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Private dispose method to release managed/unmanaged objects.
        /// If disposing = true clean up managed resources as well as unmanaged resources.
        /// If disposing = false only clean up unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether or not to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeStatusRefreshTimer();
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

            if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
            {
                this.DisposeStatusRefreshTimer();
            }

            this.FinishCallbackHandler(ex);

            bool finished = this.SetFinishedAndPostWork();

            callbackState.CallFinish(this, finished);
        }

        private void DisposeStatusRefreshTimer()
        {
            if (null != this.statusRefreshTimer)
            {
                lock (this.statusRefreshTimerLock)
                {
                    if (null != this.statusRefreshTimer)
                    {
                        this.statusRefreshTimer.Dispose();
                        this.statusRefreshTimer = null;
                    }
                }
            }
        }

        private void SetInitialStatus()
        {
            switch (this.transferEntry.Status)
            {
                case BlobTransferEntryStatus.Monitor:
                    break;
                case BlobTransferEntryStatus.RemoveSource:
                    break;
                case BlobTransferEntryStatus.NotStarted:
                case BlobTransferEntryStatus.Transfer:
                case BlobTransferEntryStatus.Finished:
                default:
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.InvalidInitialEntryStatusForControllerException,
                        this.transferEntry.Status,
                        this.GetType().Name));
            }

            this.SetHasWorkAfterStatusChanged();
        }

        private bool ChangeStatus()
        {
            Debug.Assert(
                this.transferEntry.Status != BlobTransferEntryStatus.Finished,
                "ChangeStatus called, while controller already in Finished state");

            if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
            {
                if (this.moveSource)
                {
                    this.transferEntry.Status = BlobTransferEntryStatus.RemoveSource;
                }
                else
                {
                    this.transferEntry.Status = BlobTransferEntryStatus.Finished;
                }
            }
            else if (BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
            {
                this.transferEntry.Status = BlobTransferEntryStatus.Finished;
            }
            else
            {
                Debug.Fail("We should never be here");
            }

            if (BlobTransferEntryStatus.Finished == this.transferEntry.Status)
            {
                return this.SetFinished();
            }
            else
            {
                this.SetHasWorkAfterStatusChanged();
                return this.PostWork();
            }
        }

        private void SetHasWorkAfterStatusChanged()
        {
            if (BlobTransferEntryStatus.Monitor == this.transferEntry.Status)
            {
                this.statusRefreshTimer = new Timer(
                    new TimerCallback(
                        delegate(object timerState)
                        {
                            this.HasWork = true;
                        }));

                this.state = State.FetchDestinationAttributes;
            }
            else if (BlobTransferEntryStatus.RemoveSource == this.transferEntry.Status)
            {
                this.state = State.DeleteSource;
            }
            else
            {
                Debug.Fail("We should never be here");
            }

            this.HasWork = true;
        }

        private Action<Action<ITransferController, bool>> GetFetchDestinationAttributesAction()
        {
            Debug.Assert(
                this.state == State.FetchDestinationAttributes,
                "GetFetchDestinationAttributesAction called, but state isn't FetchDestinationAttributes");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                try
                {
                    this.StartCallbackHandler();
                }
                catch (Exception e)
                {
                    this.SetErrorState(e, new CallbackState { FinishDelegate = finishDelegate });
                    return;
                }

                BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.FetchAttributes);
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
                            return this.destinationBlob.BeginFetchAttributes(
                                null,
                                requestOptions,
                                operationContext,
                                this.FetchDestinationAttributesCallback,
                                callbackState);
                        });
                }
                catch (Exception e)
                {
                    this.HandleFetchDestinationAttributesResult(e, callbackState);
                    return;
                }
            };
        }

        private void FetchDestinationAttributesCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.FetchDestinationAttributes,
                "FetchDestinationAttributesCallback called, but state isn't FetchDestinationAttributes");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.destinationBlob.EndFetchAttributes(asyncResult);
            }
            catch (Exception e)
            {
                this.HandleFetchDestinationAttributesResult(e, callbackState);
                return;
            }

            this.HandleFetchDestinationAttributesResult(null, callbackState);
        }

        private void HandleFetchDestinationAttributesResult(Exception e, CallbackState callbackState)
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
                    // The reason of 404 (Not Found) may be that the destination blob has not been created yet.
                    existingBlob = false;
                }
                else
                {
                    this.SetErrorState(e, callbackState);
                    return;
                }
            }
            
            bool finished = false;

            if (existingBlob)
            {
                if (null == this.destinationBlob.CopyState)
                {
                    string exceptionMessage = string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.FailedToRetrieveCopyStateForBlobToMonitorException,
                                this.destinationBlob.Uri.ToString());

                    this.SetErrorState(
                        new BlobTransferException(
                            BlobTransferErrorCode.FailToRetrieveCopyStateForBlobToMonitor,
                            exceptionMessage),
                        callbackState);

                    return;
                }
                else
                {
                    // Verify we are monitoring the right blob copying process.
                    if (!this.transferEntry.CopyId.Equals(this.destinationBlob.CopyState.CopyId))
                    {
                        this.SetErrorState(
                            new BlobTransferException(
                                BlobTransferErrorCode.MismatchCopyId,
                                Resources.MismatchFoundBetweenLocalAndServerCopyIdsException),
                            callbackState);
                        return;
                    }

                    CopyStatus copyStatus = this.destinationBlob.CopyState.Status;
                    if (CopyStatus.Success == copyStatus)
                    {
                        this.DisposeStatusRefreshTimer();

                        finished = this.ChangeStatus();
                    }
                    else if (CopyStatus.Pending == copyStatus)
                    {
                        try
                        {
                            this.UpdateTransferProgress();
                        }
                        catch (BlobTransferCallbackException ex)
                        {
                            this.SetErrorState(ex, callbackState);
                            return;
                        }

                        // Wait a period to restart refresh the status.
                        this.statusRefreshTimer.Change(
                            TimeSpan.FromMilliseconds(BlobTransferConstants.BlobCopyStatusRefreshWaitTimeInMilliseconds),
                            new TimeSpan(-1));

                        finished = this.PostWork();
                    }
                    else
                    {
                        string exceptionMessage = string.Format(
                                    CultureInfo.InvariantCulture,
                                    Resources.FailedToCopyBlobException,
                                    this.sourceUri.ToString(),
                                    this.destinationBlob.Uri.ToString(),
                                    copyStatus.ToString());

                        // CopyStatus.Invalid | Failed | Aborted
                        this.SetErrorState(
                            new BlobTransferException(
                                BlobTransferErrorCode.CopyFromBlobToBlobFailed,
                                exceptionMessage),
                            callbackState);

                        return;
                    }
                }
            }

            callbackState.CallFinish(this, finished);
        }

        private Action<Action<ITransferController, bool>> GetDeleteSourceAction()
        {
            Debug.Assert(
                this.moveSource,
                "moveSource must be true if we get here");
            Debug.Assert(
                this.state == State.DeleteSource,
                "GetDeleteSourceAction called, but state isn't DeleteSource");

            this.HasWork = false;

            bool deleting = false;

            if (null != this.transferEntry.BlobSet)
            {
                // We will not remove the root blob until all snapshots are transferred.
                // However, a snapshot will be removed whenever it is transferred.
                if (this.transferEntry.BlobSet.CountDown.Signal())
                {
                    deleting = true;

                    ICloudBlob rootBlob = this.transferEntry.BlobSet.RootBlob;

                    if (!BlobExtensions.Equals(this.sourceBlob, rootBlob))
                    {
                        // TODO: do we need to fetch the ETag of source blob? If yes,
                        // when should we fetch it?
                        this.sourceBlob = rootBlob;
                    }
                }
                else
                {
                    if (this.sourceBlob.SnapshotTime.HasValue)
                    {
                        deleting = true;
                    }
                }
            }
            else
            {
                deleting = true;
            }

            if (deleting)
            {
                return delegate(Action<ITransferController, bool> finishDelegate)
                {
                    this.PreWork();
                    Debug.Assert(null != finishDelegate, "Finish delegate expected");

                    CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };

                    try
                    {
                        this.StartCallbackHandler();

                        // Display status as file copied.
                        this.ProgressCallbackHandler(0.0, 100.0);
                    }
                    catch (Exception e)
                    {
                        this.SetErrorState(e, callbackState);
                        return;
                    }

                    BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.Delete);
                    OperationContext operationContext = new OperationContext()
                    {
                        ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                    };
                    DeleteSnapshotsOption deleteSnapshotsOption = this.sourceBlob.SnapshotTime.HasValue ?
                        DeleteSnapshotsOption.None :
                        DeleteSnapshotsOption.IncludeSnapshots;

                    try
                    {
                        this.CancellationHandler.RegisterCancellableAsyncOper(
                            delegate
                            {
                                return this.sourceBlob.BeginDelete(
                                    deleteSnapshotsOption,
                                    null,
                                    requestOptions,
                                    operationContext,
                                    this.DeleteSourceCallback,
                                    callbackState);
                            });
                    }
                    catch (Exception e)
                    {
                        this.SetErrorState(e, callbackState);
                        return;
                    }
                };
            }
            else
            {
                return delegate(Action<ITransferController, bool> finishDelegate)
                {
                    this.PreWork();
                    Debug.Assert(null != finishDelegate, "Finish delegate expected");

                    bool finished = this.ChangeStatus();

                    finishDelegate(this, finished);
                };
            }
        }

        private void DeleteSourceCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.DeleteSource,
                "DeleteSourceCallback called, but state isn't DeleteSource");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.sourceBlob.EndDelete(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, callbackState);
                return;
            }

            bool finished = this.ChangeStatus();

            callbackState.CallFinish(this, finished);
        }

        private bool SetFinished()
        {
            this.state = State.Finished;
            this.HasWork = false;

            this.FinishCallbackHandler(null);
            return this.SetFinishedAndPostWork();
        }

        private void UpdateTransferProgress()
        {
            if (null != this.destinationBlob.CopyState &&
                this.destinationBlob.CopyState.TotalBytes.HasValue)
            {
                Debug.Assert(
                    this.destinationBlob.CopyState.BytesCopied.HasValue,
                    "BytesCopied cannot be null as TotalBytes is not null.");

                if (null == this.transferStatusTracker)
                {
                    this.TotalBytes = this.destinationBlob.CopyState.TotalBytes.Value;
                    this.transferStatusTracker = new TransferStatusAndTotalSpeedTracker(
                        this.TotalBytes,
                        this.Manager.TransferOptions.Concurrency,
                        this.ProgressCallbackHandler,
                        this.Manager.GlobalCopySpeedTracker);
                }

                this.BytesTransferred = this.destinationBlob.CopyState.BytesCopied.Value;
                this.transferStatusTracker.UpdateStatus(this.BytesTransferred);
            }
        }

        private void StartCallbackHandler()
        {
            if (null != this.startCallback)
            {
                this.CallbackExceptionHandler(
                    delegate
                    {
                        this.startCallback(this.UserData);
                        this.startCallback = null;
                    });
            }
        }

        private void ProgressCallbackHandler(double speed, double progress)
        {
            if (null != this.progressCallback)
            {
                this.CallbackExceptionHandler(
                    delegate
                    {
                        this.progressCallback(this.UserData, speed, progress);
                    });
            }
        }

        private void FinishCallbackHandler(Exception ex)
        {
            try
            {
                if (null != this.finishCallback)
                {
                    lock (this.finishCallbackLock)
                    {
                        if (null != this.finishCallback)
                        {
                            this.finishCallback(this.UserData, ex);

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