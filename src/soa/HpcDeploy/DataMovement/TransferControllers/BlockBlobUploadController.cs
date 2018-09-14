//------------------------------------------------------------------------------
// <copyright file="BlockBlobUploadController.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Block Blob uploading code.
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
    using System.Net;
    using System.Security.Cryptography;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers;

    /// <summary>
    /// Block Blob uploading code.
    /// </summary>
    internal class BlockBlobUploadController : TransferControllerBase, IDisposable
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
        /// Keeps track of the block id used for each block.
        /// Blocks might be uploaded in any order, but at the end of the upload
        /// process we'll need to be able to figure out in which sequence these
        /// blocks should be pieced together. This dictionary is used to keep
        /// track of this sequence.
        /// </summary>
        private string[] blockIdSequence;

        /// <summary>
        /// Keeps tracks of blocks that are as yet unprocessed. Whenever we
        /// start processing a new block it's retrieved from this queue.
        /// </summary>
        private ConcurrentQueue<int> unprocessedBlocks;

        /// <summary>
        /// UploadedBlockIds is used to keep track of which block Ids
        /// have already been uploaded to storage. If we are overwriting an
        /// existing blob this dictionary is prepopulated with the existing
        /// block ids already present. Before uploading any data to storage
        /// we will first check this dictionary to see if a block with the
        /// same id has already been uploaded.
        /// If a blockId is not present in this collection it has not been
        /// uploaded yet. If a blockId is present, and the value is 'false'
        /// the block is currently in progress of being uploaded. And finally
        /// if a blockId is present, and the value is 'true' the block has
        /// finished uploading.
        /// </summary>
        private ConcurrentDictionary<string, bool> uploadedBlockIds =
            new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Countdown event to track number of blocks that still need to be
        /// uploaded/are in progress of being uploaded. Used to detect when
        /// all blocks have finished uploading and change state to Commit 
        /// state.
        /// </summary>
        private CountdownEvent toUploadBlocksCountdownEvent;

        /// <summary>
        /// Target blob object.
        /// </summary>
        private CloudBlockBlob blob;

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
        /// Running md5 hash of the blob being uploaded.
        /// </summary>        
        private MD5CryptoServiceProvider md5hash = new MD5CryptoServiceProvider();
        
        /// <summary>
        /// In restartable mode, to indicate whether to retransfer the whole file 
        /// from the very beginning.
        /// </summary>
        private bool transferFromBeginning;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockBlobUploadController"/> class.
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
        internal BlockBlobUploadController(
            BlobTransferManager manager,
            BlobTransferFileTransferEntry transferEntry,
            CloudBlockBlob blob,
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

            this.transferFromBeginning = this.transferEntry.Status == BlobTransferEntryStatus.NotStarted;

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
        /// Finalizes an instance of the <see cref="BlockBlobUploadController" /> class.
        /// </summary>
        ~BlockBlobUploadController()
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
            DownloadBlockList,
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
                case State.DownloadBlockList:
                    return this.GetDownloadBlockListAction();
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
                if (null != this.toUploadBlocksCountdownEvent)
                {
                    this.toUploadBlocksCountdownEvent.Dispose();
                    this.toUploadBlocksCountdownEvent = null;
                }

                this.CloseOwnedInputStream();

                if (null != this.md5hash)
                {
                    this.md5hash.Clear();
                    this.md5hash = null;
                }
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

        /// <summary>
        /// Generates a new block ID to be used for PutBlock.
        /// </summary>
        /// <param name="blockIdPrefix">Prefix for block id.</param>
        /// <param name="count">The count of blocks before current block.</param>
        /// <returns>Base64 encoded block ID.</returns>
        private static string GetCurrentBlockId(string blockIdPrefix, int count)
        {
            string blockIdSuffix = count.ToString("D6");
            byte[] blockIdInBytes = System.Text.Encoding.UTF8.GetBytes(blockIdPrefix + blockIdSuffix);
            return Convert.ToBase64String(blockIdInBytes);
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
                                this.transferFromBeginning = true;
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

                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                if (this.inputStreamLength > BlobTransferConstants.MaxBlockBlobFileSize)
                {
                    string exceptionMessage = string.Format(
                                CultureInfo.InvariantCulture,
                                Resources.BlobFileSizeTooLargeException,
                                Utils.BytesToHumanReadableSize(this.inputStreamLength),
                                Resources.BlockBlob,
                                Utils.BytesToHumanReadableSize(BlobTransferConstants.MaxBlockBlobFileSize));

                    this.SetErrorState(
                        new BlobTransferException(
                            BlobTransferErrorCode.UploadBlobSourceFileSizeTooLarge,
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
                                Resources.CannotOverwritePageBlobWithBlockBlobException,
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

            if (string.IsNullOrEmpty(this.transferEntry.BlockIdPrefix))
            {
                this.transferEntry.BlockIdPrefix = new Random().Next().ToString("X8") + "-";
            }

            if (0 == this.inputStreamLength)
            {
                // Create empty sequence array.
                this.blockIdSequence = new string[0];

                this.state = State.Commit;
            }
            else if (existingBlob)
            {
                if (this.blob.Properties.BlobType == BlobType.Unspecified)
                {
                    this.SetErrorState(
                        new InvalidOperationException(
                            Resources.FailedToGetBlobTypeException),
                        callbackState);
                    return;
                }

                if (this.blob.Properties.BlobType == BlobType.PageBlob)
                {
                    this.SetErrorState(
                        new InvalidOperationException(
                            Resources.CannotOverwritePageBlobWithBlockBlobException),
                        callbackState);
                    return;
                }

                Debug.Assert(
                    this.blob.Properties.BlobType == BlobType.BlockBlob,
                    "BlobType should be BlockBlob if we reach here.");

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

                if (!this.transferFromBeginning)
                {
                    this.state = State.DownloadBlockList;
                }
                else
                {
                    this.state = State.Upload;
                }
            }
            else
            {
                if (!this.transferFromBeginning)
                {
                    this.state = State.DownloadBlockList;
                }
                else
                {
                    this.state = State.Upload;
                }
            }

            this.HasWork = true;
            callbackState.CallFinish(this, this.PostWork());
        }

        private Action<Action<ITransferController, bool>> GetDownloadBlockListAction()
        {
            Debug.Assert(
                this.state == State.DownloadBlockList,
                "GetDownloadBlockListAction called, but state isn't DownloadBlockList");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.DownloadBlockList);
                AccessCondition accessCondition = AccessCondition.GenerateIfMatchCondition(this.blob.Properties.ETag);
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
                            return this.blob.BeginDownloadBlockList(
                                BlockListingFilter.Uncommitted,
                                accessCondition,
                                requestOptions,
                                operationContext,
                                this.DownloadBlockListCallback,
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

        private void DownloadBlockListCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.DownloadBlockList,
                "DownloadBlockListCallback called, but state isn't DownloadBlockList");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            IEnumerable<ListBlockItem> blockList = null;

            try
            {
                blockList = this.blob.EndDownloadBlockList(asyncResult);
            }
            catch (Exception)
            {
            }

            if (null != blockList)
            {
                foreach (ListBlockItem listBlockItem in blockList)
                {
                    // We only register the block id if the size of the block
                    // is equal to our desired block size. There could be potential
                    // collisions with blocks of a different size from the original
                    // blob. 
                    // The name length check is to ensure the existing blocks
                    // have the same length as the blocks we will be generating.
                    if (listBlockItem.Length == this.Manager.TransferOptions.BlockSize &&
                        listBlockItem.Name.Length == 20)
                    {
                        this.uploadedBlockIds.TryAdd(listBlockItem.Name, true);
                    }
                }
            }

            this.state = State.Upload;
            this.HasWork = true;
            callbackState.CallFinish(this, this.PostWork());
        }

        private Action<Action<ITransferController, bool>> GetUploadAction()
        {
            Debug.Assert(
                this.state == State.Upload,
                "GetUploadAction called, but state isn't Upload");

            this.HasWork = false;

            if (null == this.blockIdSequence || null == this.unprocessedBlocks || null == this.toUploadBlocksCountdownEvent)
            {
                Debug.Assert(
                    null == this.blockIdSequence,
                    "blockIdSequence expected to be null");
                Debug.Assert(
                    null == this.unprocessedBlocks,
                    "unprocessedBlocks expected to be null");
                Debug.Assert(
                    null == this.toUploadBlocksCountdownEvent,
                    "toUploadBlocksCountdownEvent expected to be null");

                // Calculate number of blocks.
                int numBlocks = (int)Math.Ceiling(
                    this.inputStreamLength / (double)this.Manager.TransferOptions.BlockSize);

                this.toUploadBlocksCountdownEvent = new CountdownEvent(numBlocks);

                // Create sequence array.
                this.blockIdSequence = new string[numBlocks];

                // Create unprocessed blocks queue.
                this.unprocessedBlocks = new ConcurrentQueue<int>();

                // Fill unprocessed blocks queue with the full list of block 
                // sequence numbers.
                for (
                    int blockSequenceNum = 0;
                    blockSequenceNum < numBlocks;
                    ++blockSequenceNum)
                {
                    this.unprocessedBlocks.Enqueue(blockSequenceNum);
                }
            }

            Debug.Assert(
                null != this.blockIdSequence,
                "blockIdSequence not expected to be null");
            Debug.Assert(
                null != this.unprocessedBlocks,
                "unprocessedBlocks not expected to be null");
            Debug.Assert(
                null != this.toUploadBlocksCountdownEvent,
                "toUploadBlocksCountdownEvent not expected to be null");

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                byte[] memoryBuffer = this.Manager.MemoryManager.RequireBuffer();
                if (null != memoryBuffer)
                {
                    int blockSequenceNumber;

                    if (this.unprocessedBlocks.TryDequeue(out blockSequenceNumber))
                    {
                        long blockOffset = blockSequenceNumber * this.Manager.TransferOptions.BlockSize;

                        CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };

                        ReadDataState asyncState = new ReadDataState
                        {
                            SequenceNumber = blockSequenceNumber,
                            MemoryBuffer = memoryBuffer,
                            BytesRead = 0,
                            StartOffset = blockOffset,
                            Length = (int)(Math.Min((long)(blockSequenceNumber + 1) * this.Manager.TransferOptions.BlockSize, this.inputStreamLength) - blockOffset),
                            CallbackState = callbackState,
                            MemoryManager = this.Manager.MemoryManager,
                        };

                        Debug.Assert(
                            asyncState.Length <= this.Manager.TransferOptions.BlockSize,
                            "state.Length should be within options.BlockSize");

                        this.BeginUploadBlock(asyncState);
                    }
                    else
                    {
                        Debug.Fail("Couldn't find a block to dequeue.");

                        this.SetErrorState(
                            new InvalidOperationException(),
                            new CallbackState { FinishDelegate = finishDelegate });
                        return;
                    }
                }
                else
                {
                    this.HasWork = true;
                    finishDelegate(this, this.PostWork());
                }
            };
        }

        private void BeginUploadBlock(ReadDataState asyncState)
        {
            Debug.Assert(null != asyncState, "asyncState object expected");
            Debug.Assert(
                this.state == State.Upload || this.state == State.Error,
                "BeginUploadBlock called, but state isn't Upload or Error");

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

                this.inputStream.BeginRead(
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
                readBytes = this.inputStream.EndRead(asyncResult);
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
                this.BeginUploadBlock(asyncState);
            }
            else
            {
                try
                {
                    this.CancellationHandler.CheckCancellation();

                    this.md5hash.TransformBlock(asyncState.MemoryBuffer, 0, asyncState.Length, null, 0);
                }
                catch (Exception e)
                {
                    this.SetErrorState(e, asyncState.CallbackState);
                    asyncState.Dispose();
                    return;
                }

                // As long as we have queued up blocks, we've got work
                // to do for the calling controller.
                this.HasWork = !this.unprocessedBlocks.IsEmpty;

                string blockId = GetCurrentBlockId(this.transferEntry.BlockIdPrefix, asyncState.SequenceNumber);

                this.blockIdSequence[asyncState.SequenceNumber] = blockId;

                if (this.uploadedBlockIds.TryAdd(blockId, false))
                {
                    try
                    {
                        BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.PutBlock);
                        OperationContext operationContext = new OperationContext()
                        {
                            ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                        };

                        // We're to upload this block.
                        asyncState.MemoryStream =
                            new MemoryStream(
                                asyncState.MemoryBuffer,
                                0,
                                asyncState.Length);

                        string blockHash = Convert.ToBase64String((new MD5CryptoServiceProvider()).ComputeHash(asyncState.MemoryBuffer, 0, asyncState.Length));

                        this.CancellationHandler.RegisterCancellableAsyncOper(
                            delegate
                            {
                                return this.blob.BeginPutBlock(
                                    blockId,
                                    asyncState.MemoryStream,
                                    blockHash,
                                    null,
                                    requestOptions,
                                    operationContext,
                                    this.PutBlockCallback,
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
                    // Block already uploaded or in progress.
                    this.FinishBlock(asyncState);
                }
            }
        }

        private void FinishBlock(ReadDataState asyncState)
        {
            Debug.Assert(null != asyncState, "asyncState object expected");
            Debug.Assert(
                this.state == State.Upload || this.state == State.Error,
                "FinishBlock called, but state isn't Upload or Error");
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

            try
            {
                this.transferStatusTracker.AddBytesTransferred(asyncState.Length);
            }
            catch (BlobTransferCallbackException e)
            {
                this.SetErrorState(e, asyncState);
                return;
            }

            if (this.toUploadBlocksCountdownEvent.Signal())
            {
                this.state = State.Commit;
                this.HasWork = true;
            }

            asyncState.CallbackState.CallFinish(this, this.PostWork());
            asyncState.Dispose();
        }

        private void PutBlockCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.Upload || this.state == State.Error,
                "PutBlockCallback called, but state isn't Upload or Error");

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
                this.blob.EndPutBlock(asyncResult);
                this.FinishBlock(asyncState);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState.CallbackState);
                asyncState.Dispose();
                return;
            }
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

                this.md5hash.TransformFinalBlock(new byte[0], 0, 0);
                this.blob.Properties.ContentMD5 = Convert.ToBase64String(this.md5hash.Hash);

                BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.PutBlockList);
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
                            return this.blob.BeginPutBlockList(
                                this.blockIdSequence,
                                null,
                                requestOptions,
                                operationContext,
                                this.CommitActionCallback,
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

        private void CommitActionCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.Commit,
                "CommitActionCallback called, but state isn't Commit");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            CallbackState callbackState = asyncResult.AsyncState as CallbackState;

            Debug.Assert(
                null != callbackState,
                "CallbackState expected in AsyncState");

            try
            {
                this.blob.EndPutBlockList(asyncResult);
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
                        CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };
                        this.SetErrorState(e, callbackState);
                        return;
                    }
                }

                bool finished = this.ChangeStatus();

                finishDelegate(this, finished);
            };
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