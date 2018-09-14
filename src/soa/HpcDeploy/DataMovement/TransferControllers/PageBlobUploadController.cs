//------------------------------------------------------------------------------
// <copyright file="PageBlobUploadController.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Page Blob uploading code.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferControllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers;

    /// <summary>
    /// Page Blob uploading code.
    /// </summary>
    internal class PageBlobUploadController : TransferControllerBase, IDisposable
    {
        /// <summary>
        /// Size of all files transferred to page blob must be exactly 
        /// divided by this constant.
        /// </summary>
        private const long PageBlobPageSize = (long)512;

        /// <summary>
        /// Transfer entry to store transfer information.
        /// </summary>
        private BlobTransferFileTransferEntry transferEntry;

        /// <summary>
        /// Started but didn't finish blocks in the last upload.
        /// </summary>
        private Queue<long> lastUploadWindow;

        /// <summary>
        /// Keeps track of the internal state-machine state.
        /// </summary>
        private volatile State state;

        /// <summary>
        /// Countdown event to track number of chunks that still need to be
        /// uploaded/are in progress of being uploaded. Used to detect when
        /// all blocks have finished uploading and change state to Commit 
        /// state.
        /// </summary>
        private CountdownEvent toUploadChunksCountdownEvent;

        /// <summary>
        /// Target blob object.
        /// </summary>
        private CloudPageBlob blob;

        /// <summary>
        /// Input filename.
        /// </summary>
        private string fileName;

        /// <summary>
        /// Input stream object.
        /// </summary>
        private Stream inputStream;

        /// <summary>
        /// Input stream length.
        /// </summary>
        private long inputStreamLength;

        /// <summary>
        /// Indicates whether the input stream is owned by us or not.
        /// If owned by us we'll need to ensure to close it properly.
        /// </summary>
        private bool ownsStream;

        /// <summary>
        /// Dispose lock for input stream.
        /// </summary>
        private object inputStreamDisposeLock;

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
        /// Finish callback delegate called when transfer finished.
        /// </summary>
        private Action<object, Exception> finishCallback;

        /// <summary>
        /// Lock to protect finishCallback.
        /// </summary>
        private object finishCallbackLock = new object();

        /// <summary>
        /// Transfer status and global speed tracker object.
        /// </summary>
        private TransferStatusAndTotalSpeedTracker transferStatusTracker;
        
        /// <summary>
        /// Boolean indicating whether or not we should send requests
        /// to azure storage to clear empty blocks.
        /// When creating a new blob we don't need to perform this action,
        /// as by default the empty blob is considered empty. However when
        /// overwriting we should send clear requests for any now empty pages.
        /// </summary>
        private bool needClearEmptyBlocks;

        /// <summary>
        /// Stream object to be thread safe accessed and to calculate MD5 hash.
        /// </summary>
        private MD5HashStream md5HashStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageBlobUploadController"/> class.
        /// </summary>
        /// <param name="manager">Manager object which creates this object.</param>
        /// <param name="transferEntry">Transfer entry to store transfer information.</param>
        /// <param name="blob">Target blob to upload to.</param>
        /// <param name="fileName">Source file to read data from. Exactly one of fileName and outputStream should be non-null.</param>
        /// <param name="inputStream">Source stream to read data to upload from. Exactly one of fileName and outputStream should be non-null.</param>
        /// <param name="moveSource">Indicates whether to remove source file after finishing transfer.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        [SuppressMessage("Microsoft.Cryptographic.Standard", "CA5350:MD5CannotBeUsed", Justification = "REST API dependency, only MD5 is available for calculating checksum.")]
        internal PageBlobUploadController(
            BlobTransferManager manager,
            BlobTransferFileTransferEntry transferEntry,
            CloudPageBlob blob,
            string fileName,
            Stream inputStream,
            bool moveSource,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            if (null == manager || null == transferEntry || null == blob)
            {
                throw new ArgumentNullException(null == manager ? "manager" : null == transferEntry ? "transferEntry" : "blob");
            }

            if (string.IsNullOrEmpty(fileName) && null == inputStream)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ProvideExactlyOneParameterBothNullException,
                    "fileName",
                    "inputStream"));
            }

            if (!string.IsNullOrEmpty(fileName) && null != inputStream)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ProvideExactlyOneParameterBothProvidedException,
                    "fileName",
                    "inputStream"));
            }

            if (moveSource)
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.CannotRemoveSourceWithoutSourceFileException));
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

            if (null == inputStream)
            {
                this.ownsStream = true;
                this.fileName = fileName;
                this.inputStreamDisposeLock = new object();
            }
            else
            {
                this.ownsStream = false;
                this.inputStream = inputStream;
                this.md5HashStream = new MD5HashStream(
                    this.inputStream, 
                    this.transferEntry.CheckPoint.EntryTransferOffset, 
                    true);
            }

            this.blob = blob;
            this.moveSource = moveSource;

            this.startCallback = startCallback;
            this.progressCallback = progressCallback;
            this.finishCallback = finishCallback;

            this.UserData = userData;

            this.SetInitialStatus();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PageBlobUploadController" /> class.
        /// </summary>
        ~PageBlobUploadController()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Internal state values.
        /// </summary>
        private enum State
        {
            OpenInputStream,
            FetchAttributes,
            Create,
            Resize,
            CalculateMD5,
            Upload,
            Commit,
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
                case State.OpenInputStream:
                    return this.GetOpenInputStreamAction();
                case State.FetchAttributes:
                    return this.GetFetchAttributesAction();
                case State.Create:
                    return this.GetCreateAction();
                case State.Resize:
                    return this.GetResizeAction();
                case State.CalculateMD5:
                    return this.GetCalculateMD5Action();
                case State.Upload:
                    return this.GetUploadAction();
                case State.Commit:
                    return this.GetCommitAction();
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
                if (null != this.toUploadChunksCountdownEvent)
                {
                    this.toUploadChunksCountdownEvent.Dispose();
                    this.toUploadChunksCountdownEvent = null;
                }

                if (null != this.md5HashStream)
                {
                    this.md5HashStream.Dispose();
                    this.md5HashStream = null;
                }

                this.CloseOwnedInputStream();
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

            if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
            {
                this.CloseOwnedInputStream();
            }

            this.FinishCallbackHandler(ex);

            bool finished = this.SetFinishedAndPostWork();
            callbackState.CallFinish(this, finished);
        }

        private void CloseOwnedInputStream()
        {
            if (this.ownsStream)
            {
                if (null != this.inputStream)
                {
                    lock (this.inputStreamDisposeLock)
                    {
                        if (null != this.inputStream)
                        {
                            this.inputStream.Close();
                            this.inputStream = null;
                        }
                    }
                }
            }
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
                case BlobTransferEntryStatus.RemoveSource:
                    break;
                case BlobTransferEntryStatus.Monitor:
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

            if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
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
            if (BlobTransferEntryStatus.Transfer == this.transferEntry.Status)
            {
                if (null == this.inputStream)
                {
                    // Open file first.
                    this.state = State.OpenInputStream;
                }
                else
                {
                    if (!this.inputStream.CanRead)
                    {
                        throw new NotSupportedException(string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.StreamMustSupportReadException,
                            "inputStream"));
                    }

                    if (!this.inputStream.CanSeek)
                    {
                        throw new NotSupportedException(string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.StreamMustSupportSeekException,
                            "inputStream"));
                    }

                    this.inputStreamLength = this.inputStream.Length;

                    if (0 != this.inputStream.Position)
                    {
                        this.inputStream.Seek(0, SeekOrigin.Begin);
                    }

                    this.state = State.FetchAttributes;
                }
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

        private Action<Action<ITransferController, bool>> GetOpenInputStreamAction()
        {
            Debug.Assert(
                this.state == State.OpenInputStream,
                "GetOpenInputStreamAction called, but state isn't OpenInputStream");

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

                if (null != this.transferEntry.LastModified)
                {
                    try
                    {
                        DateTimeOffset lastWriteTime = new DateTimeOffset(File.GetLastWriteTimeUtc(this.fileName));

                        if (lastWriteTime != this.transferEntry.LastModified)
                        {
                            if (!this.RetransferModifiedCallbackHandler(this.fileName, new CallbackState { FinishDelegate = finishDelegate }))
                            {
                                return;
                            }
                            else
                            {
                                this.transferEntry.LastModified = lastWriteTime;
                                this.transferEntry.CheckPoint.Clear();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.SetErrorState(
                            new BlobTransferException(
                                BlobTransferErrorCode.FailToGetSourceLastWriteTime,
                                Resources.FailedToGetSourceLastWriteTime,
                                ex),
                            new CallbackState { FinishDelegate = finishDelegate });

                        return;
                    }
                }

                if (null == this.transferEntry.CheckPoint.UploadWindow)
                {
                    this.transferEntry.CheckPoint.UploadWindow = new List<long>();
                }
                else if (this.transferEntry.CheckPoint.UploadWindow.Any())
                {
                    this.lastUploadWindow = new Queue<long>(this.transferEntry.CheckPoint.UploadWindow);
                }
                
                try
                {
                    this.CancellationHandler.CheckCancellation();

                    // Attempt to open the file first so that we throw an exception before getting into the async work
                    this.inputStream = new FileStream(
                        this.fileName,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite);

                    this.inputStreamLength = this.inputStream.Length;

                    this.md5HashStream = new MD5HashStream(
                        this.inputStream,
                        this.transferEntry.CheckPoint.EntryTransferOffset,
                        true);
                }
                catch (OperationCanceledException ex)
                {
                    this.SetErrorState(
                        ex,
                        new CallbackState { FinishDelegate = finishDelegate });

                    return;
                }
                catch (Exception ex)
                {
                    string exceptionMessage = string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.FailedToOpenFileException,
                                this.fileName);

                    this.SetErrorState(
                        new BlobTransferException(
                            BlobTransferErrorCode.OpenFileFailed,
                            exceptionMessage,
                            ex),
                        new CallbackState { FinishDelegate = finishDelegate });

                    return;
                }

                this.state = State.FetchAttributes;
                this.HasWork = true;
                finishDelegate(this, this.PostWork());
            };
        }

        private Action<Action<ITransferController, bool>> GetFetchAttributesAction()
        {
            Debug.Assert(
                this.state == State.FetchAttributes,
                "GetFetchAttributesAction called, but state isn't FetchAttributes");

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

                this.transferStatusTracker = new TransferStatusAndTotalSpeedTracker(
                    this.inputStreamLength,
                    this.Manager.TransferOptions.Concurrency,
                    this.ProgressCallbackHandler,
                    this.Manager.GlobalUploadSpeedTracker);

                if (this.inputStreamLength > BlobTransferConstants.MaxPageBlobFileSize)
                {
                    string exceptionMessage = string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.BlobFileSizeTooLargeException,
                                Utils.BytesToHumanReadableSize(this.inputStreamLength),
                                Resources.PageBlob,
                                Utils.BytesToHumanReadableSize(BlobTransferConstants.MaxPageBlobFileSize));

                    this.SetErrorState(
                        new BlobTransferException(
                            BlobTransferErrorCode.UploadBlobSourceFileSizeTooLarge,
                            exceptionMessage),
                        new CallbackState { FinishDelegate = finishDelegate });

                    return;
                }

                if (0 != this.inputStreamLength % PageBlobPageSize)
                {
                    string exceptionMessage = string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.BlobFileSizeInvalidException,
                                Utils.BytesToHumanReadableSize(this.inputStreamLength),
                                Resources.PageBlob,
                                Utils.BytesToHumanReadableSize(PageBlobPageSize));
                    
                    this.SetErrorState(
                        new BlobTransferException(
                            BlobTransferErrorCode.UploadBlobSourceFileSizeInvalid,
                            exceptionMessage),
                        new CallbackState { FinishDelegate = finishDelegate });

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
                            return this.blob.BeginFetchAttributes(
                                null,
                                requestOptions,
                                operationContext,
                                this.FetchAttributesCallback,
                                callbackState);
                        });
                }
                catch (Exception e)
                {
                    this.HandleFetchAttributesResult(e, callbackState);
                    return;
                }
            };
        }

        private void FetchAttributesCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.FetchAttributes,
                "FetchAttributesCallback called, but state isn't FetchAttributes");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.blob.EndFetchAttributes(asyncResult);
            }
            catch (Exception e)
            {
                this.HandleFetchAttributesResult(e, callbackState);
                return;
            }

            this.HandleFetchAttributesResult(null, callbackState);
        }

        private void HandleFetchAttributesResult(Exception e, CallbackState callbackState)
        {
            bool existingBlob = true;

            if (null != e)
            {
                StorageException se = e as StorageException;

                if (null != se)
                {
                    // Getting a storage exception is expected if the blob doesn't
                    // exist. In this case we won't error out, but set the 
                    // existingBlob flag to false to indicate we're uploading
                    // a new blob instead of overwriting an existing blob.
                    if (null != se.RequestInformation &&
                        se.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    {
                        existingBlob = false;
                    }
                    else
                    {
                        this.SetErrorState(se, callbackState);
                        return;
                    }
                }
                else
                {
                    InvalidOperationException ioe = e as InvalidOperationException;

                    if (null != ioe)
                    {
                        this.SetErrorState(
                            new InvalidOperationException(
                                Resources.CannotOverwriteBlockBlobWithPageBlobException,
                                e),
                            callbackState);
                        return;
                    }
                    else
                    {
                        this.SetErrorState(e, callbackState);
                        return;
                    }
                }
            }

            if (existingBlob)
            {
                if (this.blob.Properties.BlobType == BlobType.Unspecified)
                {
                    this.SetErrorState(
                        new InvalidOperationException(
                            Resources.FailedToGetBlobTypeException),
                        callbackState);
                    return;
                }

                if (this.blob.Properties.BlobType == BlobType.BlockBlob)
                {
                    this.SetErrorState(
                        new InvalidOperationException(
                            Resources.CannotOverwriteBlockBlobWithPageBlobException),
                        callbackState);
                    return;
                }

                Debug.Assert(
                    this.blob.Properties.BlobType == BlobType.PageBlob,
                    "BlobType should be PageBlob if we reach here.");

                // If destination file exists, query user whether to overwrite it.
                if (null != this.Manager.TransferOptions.OverwritePromptCallback)
                {
                    if (!this.OverwritePromptCallbackHandler(
                        this.fileName,
                        this.blob.Uri.ToString(), 
                        callbackState))
                    {
                        return;
                    }
                }

                this.state = State.Resize;
            }
            else
            {
                this.state = State.Create;
            }

            this.HasWork = true;
            callbackState.CallFinish(this, this.PostWork());
        }

        private Action<Action<ITransferController, bool>> GetCreateAction()
        {
            Debug.Assert(
                this.state == State.Create,
                "GetCreateAction called, but state isn't Create");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };

                try
                {
                    BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.CreatePageBlob);
                    OperationContext operationContext = new OperationContext()
                    {
                        ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                    };

                    this.CancellationHandler.RegisterCancellableAsyncOper(
                        delegate
                        {
                            return this.blob.BeginCreate(
                                this.inputStreamLength,
                                null,
                                requestOptions,
                                operationContext,
                                this.CreateCallback,
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

        private void CreateCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.Create,
                "CreateCallback called, but state isn't Create");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.blob.EndCreate(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, callbackState);
                return;
            }

            if (0 == this.inputStreamLength)
            {
                this.SetCommit();
            }
            else
            {
                this.InitUpload(callbackState);
                this.needClearEmptyBlocks = false;

                if (this.md5HashStream.FinishedSeparateMd5Calculator)
                {
                    this.state = State.Upload;
                }
                else
                {
                    this.state = State.CalculateMD5;
                }

                this.HasWork = true;
            }

            callbackState.CallFinish(this, this.PostWork());
        }

        private Action<Action<ITransferController, bool>> GetResizeAction()
        {
            Debug.Assert(
                this.state == State.Resize,
                "GetResizeAction called, but state isn't Resize");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.CreatePageBlob);
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
                            return this.blob.BeginResize(
                                this.inputStreamLength,
                                null,
                                requestOptions,
                                operationContext,
                                this.ResizeCallback,
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

        private void ResizeCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.Resize,
                "ResizeCallback called, but state isn't Resize");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.blob.EndResize(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, callbackState);
                return;
            }

            if (0 == this.inputStreamLength)
            {
                this.SetCommit();
            }
            else
            {
                this.InitUpload(callbackState);
                this.needClearEmptyBlocks = true;

                if (this.md5HashStream.FinishedSeparateMd5Calculator)
                {
                    this.state = State.Upload;
                }
                else
                {
                    this.state = State.CalculateMD5;
                }

                this.HasWork = true;
            }

            callbackState.CallFinish(this, this.PostWork());
        }

        private Action<Action<ITransferController, bool>> GetCalculateMD5Action()
        {
            Debug.Assert(
                this.state == State.CalculateMD5,
                "GetCalculateMD5Action called, but state isn't CalculateMD5");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                this.HasWork = true;
                this.state = State.Upload;
                
                CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };

                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        this.md5HashStream.CalculateMd5(this.Manager.MemoryManager, this.CancellationHandler.CancellationChecker);
                    }
                    catch (Exception e)
                    {
                        this.SetErrorState(e, callbackState);
                        return;
                    }

                    finishDelegate(this, this.PostWork());
                });
            };
        }

        private Action<Action<ITransferController, bool>> GetUploadAction()
        {
            Debug.Assert(
                this.state == State.Upload,
                "GetUploadAction called, but state isn't Upload");

            this.HasWork = false;

            Debug.Assert(
                null != this.toUploadChunksCountdownEvent,
                "toUploadChunksCountdownEvent not expected to be null");

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };

                byte[] memoryBuffer = this.Manager.MemoryManager.RequireBuffer();

                if (null != memoryBuffer)
                {
                    long startOffset = 0;

                    if (null != this.lastUploadWindow && this.lastUploadWindow.Any())
                    {
                        startOffset = this.lastUploadWindow.Dequeue();
                    }
                    else
                    {
                        bool canUpload = false;

                        lock (this.transferEntry.EntryLock)
                        {
                            if (this.transferEntry.CheckPoint.UploadWindow.Count < BlobTransferConstants.MaxUploadWindowSize)
                            {
                                startOffset = this.transferEntry.CheckPoint.EntryTransferOffset;
                                canUpload = true;

                                if (this.transferEntry.CheckPoint.EntryTransferOffset <= this.inputStreamLength)
                                {
                                    this.transferEntry.CheckPoint.UploadWindow.Add(startOffset);
                                    this.transferEntry.CheckPoint.EntryTransferOffset = Math.Min(
                                        this.transferEntry.CheckPoint.EntryTransferOffset + this.Manager.TransferOptions.BlockSize,
                                        this.inputStreamLength);
                                }
                            }
                        }

                        if (!canUpload)
                        {
                            this.HasWork = true;
                            finishDelegate(this, this.PostWork());
                            this.Manager.MemoryManager.ReleaseBuffer(memoryBuffer);
                            return;
                        }
                    }

                    if (startOffset > this.inputStreamLength)
                    {
                        this.SetErrorState(
                            new InvalidOperationException(Resources.SourceFileHasBeenChangedException),
                            callbackState);
                        this.Manager.MemoryManager.ReleaseBuffer(memoryBuffer);
                        return;
                    }

                    // Here calls to trigger the restart journal writing
                    this.transferStatusTracker.AddBytesTransferred(0);

                    ReadDataState asyncState = new ReadDataState
                    {
                        MemoryBuffer = memoryBuffer,
                        BytesRead = 0,
                        StartOffset = startOffset,
                        Length = (int)Math.Min(this.Manager.TransferOptions.BlockSize, this.inputStreamLength - startOffset),
                        CallbackState = callbackState,
                        MemoryManager = this.Manager.MemoryManager,
                    };

                    if (startOffset == this.inputStreamLength)
                    {
                        this.FinishChunk(asyncState);
                        return;
                    }

                    this.BeginUploadChunk(asyncState);
                }
                else
                {
                    this.HasWork = true;
                    finishDelegate(this, this.PostWork());
                }
            };
        }

        private void BeginUploadChunk(ReadDataState asyncState)
        {
            Debug.Assert(null != asyncState, "asyncState object expected");
            Debug.Assert(
                this.state == State.Upload || this.state == State.Error,
                "BeginUploadChunk called, but state isn't Upload or Error");

            Debug.Assert(
                null != asyncState.CallbackState,
                "CallbackState expected in AsyncState");

            // If a parallel operation caused the controller to be placed in
            // error state exit early to avoid unnecessary I/O.
            if (this.state == State.Error)
            {
                asyncState.CallbackState.CallFinish(this, this.PostWork());
                asyncState.Dispose();
                return;
            }

            try
            {
                this.CancellationHandler.CheckCancellation();

                this.md5HashStream.BeginRead(
                    asyncState.StartOffset + asyncState.BytesRead,
                    asyncState.MemoryBuffer,
                    asyncState.BytesRead,
                    asyncState.Length - asyncState.BytesRead,
                    this.UploadCallback,
                    asyncState);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState.CallbackState);
                asyncState.Dispose();
                return;
            }
        }

        private void UploadCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.Upload || this.state == State.Error,
                "UploadCallback called, but state isn't Upload or Error");

            ReadDataState asyncState = asyncResult.AsyncState as ReadDataState;

            Debug.Assert(
                null != asyncState,
                "Expected ReadDataState in AsyncState");
            Debug.Assert(
                null != asyncState.CallbackState,
                "CallbackState expected in AsyncState");

            int readBytes;

            try
            {
                readBytes = this.md5HashStream.EndRead(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState.CallbackState);
                asyncState.Dispose();
                return;
            }

            // If a parallel operation caused the controller to be placed in
            // error state exit early to avoid unnecessary I/O.
            // Note that this check needs to be after the EndRead operation
            // above to avoid leaking resources.
            if (this.state == State.Error)
            {
                asyncState.CallbackState.CallFinish(this, this.PostWork());
                asyncState.Dispose();
                return;
            }

            asyncState.BytesRead += readBytes;

            if (asyncState.BytesRead < asyncState.Length)
            {
                this.BeginUploadChunk(asyncState);
            }
            else
            {
                bool chunkAllZero = true;

                try
                {
                    this.CancellationHandler.CheckCancellation();

                    if (!this.md5HashStream.MD5HashTransformBlock(asyncState.StartOffset, asyncState.MemoryBuffer, 0, asyncState.Length, null, 0))
                    {
                        // Error info has been set in Calculate MD5 action, just return
                        asyncState.CallbackState.CallFinish(this, this.PostWork());
                        asyncState.Dispose();
                        return;
                    }
                }
                catch (Exception e)
                {
                    this.SetErrorState(e, asyncState.CallbackState);
                    asyncState.Dispose();
                    return;
                }

                lock (this.transferEntry.EntryLock)
                {
                    this.HasWork = ((this.transferEntry.CheckPoint.EntryTransferOffset < this.inputStreamLength) 
                        && (this.transferEntry.CheckPoint.UploadWindow.Count < BlobTransferFileTransferEntryCheckPoint.MaxUploadWindowLength))
                        || (this.transferEntry.CheckPoint.UploadWindow.Count > 0);
                }

                for (int i = 0; i < asyncState.Length; ++i)
                {
                    if (asyncState.MemoryBuffer[i] != 0)
                    {
                        chunkAllZero = false;
                        break;
                    }
                }

                if (!chunkAllZero)
                {
                    try
                    {
                        BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.WritePages);
                        OperationContext operationContext = new OperationContext()
                        {
                            ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                        };

                        asyncState.MemoryStream =
                            new MemoryStream(
                                asyncState.MemoryBuffer,
                                0,
                                asyncState.Length);

                        string pageHash = Convert.ToBase64String((new MD5CryptoServiceProvider()).ComputeHash(asyncState.MemoryBuffer, 0, asyncState.Length));

                        this.CancellationHandler.RegisterCancellableAsyncOper(
                            delegate
                            {
                                return this.blob.BeginWritePages(
                                    asyncState.MemoryStream,
                                    asyncState.StartOffset,
                                    pageHash,
                                    null,
                                    requestOptions,
                                    operationContext,
                                    this.WritePagesCallback,
                                    asyncState);
                            });
                    }
                    catch (Exception e)
                    {
                        this.SetErrorState(e, asyncState.CallbackState);
                        asyncState.Dispose();
                        return;
                    }
                }
                else
                {
                    if (this.needClearEmptyBlocks)
                    {
                        try
                        {
                            BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.ClearPages);
                            OperationContext operationContext = new OperationContext()
                            {
                                ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                            };

                            this.CancellationHandler.RegisterCancellableAsyncOper(
                                delegate
                                {
                                    return this.blob.BeginClearPages(
                                        asyncState.StartOffset,
                                        asyncState.Length,
                                        null,
                                        requestOptions,
                                        operationContext,
                                        this.ClearPagesCallback,
                                        asyncState);
                                });
                        }
                        catch (Exception e)
                        {
                            this.SetErrorState(e, asyncState.CallbackState);
                            asyncState.Dispose();
                            return;
                        }
                    }
                    else
                    {
                        this.FinishChunk(asyncState);
                    }
                }
            }
        }

        private void WritePagesCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.Upload || this.state == State.Error,
                "WritePagesCallback called, but state isn't Upload or Error");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            ReadDataState asyncState = asyncResult.AsyncState as ReadDataState;

            Debug.Assert(
                null != asyncState,
                "Expected ReadDataState in AsyncState");
            Debug.Assert(
                null != asyncState.CallbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.blob.EndWritePages(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState.CallbackState);
                asyncState.Dispose();
                return;
            }

            // If a parallel operation caused the controller to be placed in
            // error state exit early to avoid unnecessary I/O.
            // Note that this check needs to be after the EndRead operation
            // above to avoid leaking resources.
            if (this.state == State.Error)
            {
                asyncState.CallbackState.CallFinish(this, this.PostWork());
                asyncState.Dispose();
                return;
            }

            this.FinishChunk(asyncState);
        }

        private void ClearPagesCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.Upload || this.state == State.Error,
                "ClearPagesCallback called, but state isn't Upload or Error");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            ReadDataState asyncState = asyncResult.AsyncState as ReadDataState;

            Debug.Assert(
                null != asyncState,
                "Expected ReadDataState in AsyncState");
            Debug.Assert(
                null != asyncState.CallbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.blob.EndClearPages(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState.CallbackState);
                asyncState.Dispose();
                return;
            }

            // If a parallel operation caused the controller to be placed in
            // error state exit early to avoid unnecessary I/O.
            // Note that this check needs to be after the EndRead operation
            // above to avoid leaking resources.
            if (this.state == State.Error)
            {
                asyncState.CallbackState.CallFinish(this, this.PostWork());
                asyncState.Dispose();
                return;
            }

            this.FinishChunk(asyncState);
        }

        private void FinishChunk(ReadDataState asyncState)
        {
            Debug.Assert(null != asyncState, "asyncState object expected");
            Debug.Assert(
                this.state == State.Upload || this.state == State.Error,
                "FinishChunk called, but state isn't Upload or Error");
            Debug.Assert(
                null != asyncState.CallbackState,
                "CallbackState expected in AsyncState");

            // If a parallel operation caused the controller to be placed in
            // error state exit, make sure not to accidentally change it to
            // the Commit state.
            if (this.state == State.Error)
            {
                asyncState.CallbackState.CallFinish(this, this.PostWork());
                asyncState.Dispose();
                return;
            }

            lock (this.transferEntry.EntryLock)
            {
                this.transferEntry.CheckPoint.UploadWindow.Remove(asyncState.StartOffset);
            }

            try
            {
                this.transferStatusTracker.AddBytesTransferred(asyncState.Length);
            }
            catch (BlobTransferCallbackException e)
            {
                this.SetErrorState(e, asyncState);
                return;
            }

            if (this.toUploadChunksCountdownEvent.Signal())
            {
                if (!this.md5HashStream.SucceededSeparateMd5Calculator)
                {
                    asyncState.CallbackState.CallFinish(this, this.PostWork());
                    asyncState.Dispose();
                    return;
                }

                this.SetCommit();
            }

            asyncState.CallbackState.CallFinish(this, this.PostWork());
            asyncState.Dispose();
        }

        private void SetCommit()
        {
            this.state = State.Commit;
            this.HasWork = true;
        }

        private Action<Action<ITransferController, bool>> GetCommitAction()
        {
            Debug.Assert(
                this.state == State.Commit,
                "GetCommitAction called, but state isn't Commit");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };

                try
                {
                    BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.SetMetadata);
                    OperationContext operationContext = new OperationContext()
                    {
                        ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                    };

                    this.md5HashStream.MD5HashTransformFinalBlock(new byte[0], 0, 0);  

                    this.blob.Properties.ContentMD5 = Convert.ToBase64String(this.md5HashStream.Hash);

                    this.CancellationHandler.RegisterCancellableAsyncOper(
                        delegate
                        {
                            return this.blob.BeginSetProperties(
                                null,
                                requestOptions,
                                operationContext,
                                this.CommitCallback,
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

        private void CommitCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.Commit,
                "CommitCallback called, but state isn't Commit");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.blob.EndSetProperties(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, callbackState);
                return;
            }

            this.CloseOwnedInputStream();

            bool finished = this.ChangeStatus();

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

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                try
                {
                    this.StartCallbackHandler();

                    // Display status as file copied.
                    this.ProgressCallbackHandler(0.0, 100.0);
                }
                catch (Exception e)
                {
                    this.SetErrorState(e, new CallbackState { FinishDelegate = finishDelegate });
                    return;
                }

                if (this.moveSource)
                {
                    try
                    {
                        File.Delete(this.fileName);
                    }
                    catch (Exception e)
                    {
                        CallbackState callbackState = new CallbackState
                        {
                            FinishDelegate = finishDelegate
                        };
                        this.SetErrorState(e, callbackState);
                        return;
                    }
                }

                bool finished = this.ChangeStatus();

                finishDelegate(this, finished);
            };
        }

        private void InitUpload(CallbackState callbackState)
        {
            if (null == this.toUploadChunksCountdownEvent)
            {
                Debug.Assert(
                    null == this.toUploadChunksCountdownEvent,
                    "toUploadChunksCountdownEvent expected to be null");

                if ((this.transferEntry.CheckPoint.EntryTransferOffset != this.inputStreamLength)
                    && (0 != this.transferEntry.CheckPoint.EntryTransferOffset % this.Manager.TransferOptions.BlockSize))
                {
                    this.SetErrorState(
                        new FormatException(Resources.RestartableInfoCorruptedException), 
                        callbackState);
                    return;
                }

                // Calculate number of chunks.
                int numChunks = (int)Math.Ceiling(
                    (this.inputStreamLength - this.transferEntry.CheckPoint.EntryTransferOffset) / (double)this.Manager.TransferOptions.BlockSize)
                    + this.transferEntry.CheckPoint.UploadWindow.Count + 1;

                this.toUploadChunksCountdownEvent = new CountdownEvent(numChunks);

                long transferedBytes = this.transferEntry.CheckPoint.EntryTransferOffset - (this.transferEntry.CheckPoint.UploadWindow.Count * this.Manager.TransferOptions.BlockSize);

                this.transferStatusTracker.AddBytesTransferred(transferedBytes);
            }
        }

        private bool SetFinished()
        {
            this.state = State.Finished;
            this.HasWork = false;

            this.FinishCallbackHandler(null);
            return this.SetFinishedAndPostWork();
        }
        
        /// <summary>
        /// Sets the state of the controller to Error, while recording 
        /// the last occured exception and setting the HasWork and 
        /// IsFinished fields.
        /// If the passed in ReadDataState holds any resources that need
        /// to be disposed they will be disposed at this time.
        /// The ReadDataState is expected to hold a CallbackState object,
        /// which will be used to indicate the upload operation has finished.
        /// </summary>
        /// <param name="ex">Exception to record.</param>
        /// <param name="readState">ReadDataState object.</param>
        private void SetErrorState(Exception ex, ReadDataState readState)
        {
            Debug.Assert(
                this.state != State.Finished,
                "SetErrorState called, while controller already in Finished state");
            Debug.Assert(
                null != readState,
                "DownloadState expected");

            this.SetErrorState(ex, readState.CallbackState);

            readState.Dispose();
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