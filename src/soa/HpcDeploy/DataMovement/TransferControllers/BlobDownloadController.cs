//------------------------------------------------------------------------------
// <copyright file="BlobDownloadController.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Blob downloading code.
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
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.Hpc.Azure.DataMovement.Extensions;
    using Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;

    /// <summary>
    /// Blob downloading code.
    /// </summary>
    internal class BlobDownloadController : TransferControllerBase, IDisposable
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
        /// Blob source object.
        /// </summary>
        private ICloudBlob blob;

        /// <summary>
        /// Blockblob source object.
        /// </summary>
        private CloudBlockBlob blockBlob;

        /// <summary>
        /// Pageblob source object.
        /// </summary>
        private CloudPageBlob pageBlob;

        /// <summary>
        /// Destination file.
        /// </summary>
        private string fileName;

        /// <summary>
        /// Destination stream.
        /// </summary>
        private Stream outputStream;

        /// <summary>
        /// Indicates whether the output stream is owned by us or not.
        /// If owned by us we'll need to ensure to close it properly.
        /// </summary>
        private bool ownsStream;

        /// <summary>
        /// Dispose lock for output stream.
        /// </summary>
        private object outputStreamDisposeLock;

        /// <summary>
        /// Whether to remove source file after finishing transfer.
        /// </summary>
        private bool moveSource;

        /// <summary>
        /// Whether to keep destination's last write time to be the same with source's.
        /// </summary>
        private bool keepLastWriteTime;

        /// <summary>
        /// Countdown event to track number of blocks/page ranges that still 
        /// need to be downloaded/are in progress of being downloaded. Used 
        /// to detect when all blocks/page ranges have finished downloading 
        /// and change state to Finish state.
        /// </summary>
        private CountdownEvent toDownloadItemsCountdownEvent;
        
        /// <summary>
        /// Transfer status and total speed tracker object.
        /// </summary>
        private TransferStatusAndTotalSpeedTracker transferStatusTracker;

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
        /// Keeps track of the set of blocks that need to be downloaded.
        /// </summary>
        private List<BlockData> blocksToDownload;

        /// <summary>
        /// Indicates whether there is currently an active task writing data
        /// to the destination stream.
        /// </summary>
        private volatile bool hasActiveFileWriter;

        /// <summary>
        /// Cache for block blob data.
        /// </summary>
        private ConcurrentDictionary<string, BlockCacheEntry> availableBlockData;

        /// <summary>
        /// Cache for page blob data.
        /// </summary>
        private ConcurrentDictionary<long, byte[]> availablePageRangeData;

        /// <summary>
        /// Collection of currently downloading block blob blocks.
        /// Since blocks maybe be reused in multiple locations we keep track
        /// of currently downloading blocks to avoid downloading the same block
        /// data multiple times simultaniously.
        /// </summary>
        private ConcurrentDictionary<string, object> currentBlocksDownloading;

        /// <summary>
        /// Next download index in blocksToDownload or pageRangeList lists.
        /// </summary>
        private int nextDownloadIndex;

        /// <summary>
        /// Next write index in blocksToDownload or pageRangeList lists.
        /// </summary>
        private volatile int nextWriteIndex;

        /// <summary>
        /// Keeps track of the set of page ranges that need to be downloaded.
        /// </summary>
        private List<PageBlobRange> pageRangeList;

        /// <summary>
        /// Countdown event that tracks count of spans 
        /// to get page ranges.
        /// </summary>
        private CountdownEvent getPageRangesCountDownEvent;

        /// <summary>
        /// List of spans to get page ranges from from Azure storage.
        /// </summary>
        private HashSet<PageRangesSpan> pageRangesSpanSet;

        /// <summary>
        /// Lock used in get page ranges action to 
        /// make  sure accessing to pageRangesSpanList to be exclusive.
        /// </summary>
        private object getPageRangesLock;

        /// <summary>
        /// Index in pageRangesSpanList of span to get page ranges 
        /// from Azure Storage.
        /// </summary>
        private int getPageRangesSpanIndex;

        /// <summary>
        /// Total count of spans to get page ranges.
        /// </summary>
        private int getPageRangesSpanCount;

        /// <summary>
        /// Stream object to be thread safe accessed and to calculate MD5 hash.
        /// </summary>
        private MD5HashStream md5HashStream;

        /// <summary>
        /// Indicates whether need to check content MD5 or not.
        /// </summary>
        private bool checkMd5;

        /// <summary>
        /// Memory reserved for this controller.
        /// </summary>
        private long reservedMemory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobDownloadController"/> class.
        /// </summary>
        /// <param name="manager">Manager object which creates this object.</param>
        /// <param name="transferEntry">Transfer entry to store transfer information.</param>
        /// <param name="blob">Source blob to download.</param>
        /// <param name="fileName">File to write downloaded data to. Exactly one of fileName and outputStream should be non-null.</param>
        /// <param name="outputStream">Destination stream to write downloaded data to. Exactly one of fileName and outputStream should be non-null.</param>
        /// <param name="checkMd5">Indicates whether to check MD5 hash after finishing transfer. 
        /// Only applicable when downloading blobs from Azure Storage to a local file.</param>
        /// <param name="moveSource">Indicates whether to remove source file after finishing transfer.</param>
        /// <param name="keepLastWriteTime">Indicates whether to keep destination's last write time to be the same with source's.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        [SuppressMessage("Microsoft.Cryptographic.Standard", "CA5350:MD5CannotBeUsed", Justification = "MD5 used for data verification, not for encryption purposed.")]
        internal BlobDownloadController(
            BlobTransferManager manager,
            BlobTransferFileTransferEntry transferEntry,
            ICloudBlob blob,
            string fileName,
            Stream outputStream,
            bool checkMd5,
            bool moveSource,
            bool keepLastWriteTime,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            if (null == manager || null == transferEntry || null == blob)
            {
                throw new ArgumentNullException(null == manager ? "manager" : (null == transferEntry ? "transferEntry" : "blob"));
            }

            if (keepLastWriteTime && string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.NoFileToSetLastWriteTime));
            }

            if (string.IsNullOrEmpty(fileName) && null == outputStream)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ProvideExactlyOneParameterBothNullException,
                    "fileName",
                    "outputStream"));
            }

            if (!string.IsNullOrEmpty(fileName) && null != outputStream)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ProvideExactlyOneParameterBothProvidedException,
                    "fileName",
                    "outputStream"));
            }

            if (!moveSource && BlobTransferEntryStatus.RemoveSource == transferEntry.Status)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.InvalidInitialEntryStatusWhenMoveSourceIsOffException,
                    transferEntry.Status));
            }

            this.Manager = manager;
            this.transferEntry = transferEntry;

            if (null == outputStream)
            {
                this.ownsStream = true;
                this.fileName = fileName;
                this.outputStreamDisposeLock = new object();
            }
            else
            {
                this.ownsStream = false;
                this.outputStream = outputStream;
            }

            this.blob = blob;

            this.moveSource = moveSource;
            this.keepLastWriteTime = keepLastWriteTime;

            this.startCallback = startCallback;
            this.progressCallback = progressCallback;
            this.finishCallback = finishCallback;

            this.UserData = userData;

            this.checkMd5 = checkMd5;

            this.SetInitialStatus();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BlobDownloadController" /> class.
        /// </summary>
        ~BlobDownloadController()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Internal state values.
        /// <para>Possible state transitions:
        ///     FetchAttributes -> DownloadBlockList -> DownloadBlockBlob -> [DeleteSource -> [FetchAttributes ->]] Finished
        ///     FetchAttributes -> GetPageRanges -> DownloadPageBlob -> [DeleteSource -> [FetchAttributes ->]] Finished
        ///     &lt;any state&gt; -> Error</para>.
        /// </summary>
        private enum State
        {
            OpenOutputStream,
            FetchAttributes,
            DownloadBlockList,
            CalculateMD5,
            DownloadBlockBlob,
            GetPageRanges,
            DownloadPageBlob,
            SetLastWriteTime,
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
                case State.OpenOutputStream:
                    return this.GetOpenOutputStreamAction();
                case State.FetchAttributes:
                    return this.GetFetchAttributesAction();
                case State.DownloadBlockList:
                    return this.GetDownloadBlockListAction();
                case State.CalculateMD5:
                    return this.GetCalculateMD5Action();
                case State.DownloadBlockBlob:
                    return this.GetDownloadBlockBlobAction();
                case State.GetPageRanges:
                    return this.GetPageRangesAction();
                case State.DownloadPageBlob:
                    return this.GetDownloadPageBlobAction();
                case State.SetLastWriteTime:
                    return this.GetSetLastWriteTimeAction();
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
                if (null != this.toDownloadItemsCountdownEvent)
                {
                    this.toDownloadItemsCountdownEvent.Dispose();
                    this.toDownloadItemsCountdownEvent = null;
                }

                if (null != this.md5HashStream)
                {
                    this.md5HashStream.Dispose();
                    this.md5HashStream = null;
                }

                this.CloseOwnedOutputStream();

                this.getPageRangesCountDownEvent?.Dispose();
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
                this.CloseOwnedOutputStream();
            }

            this.FinishCallbackHandler(ex);

            bool finished = this.SetFinishedAndPostWork();
            callbackState.CallFinish(this, finished);
        }

        private void CloseOwnedOutputStream()
        {
            if (this.ownsStream)
            {
                if (null != this.outputStream)
                {
                    lock (this.outputStreamDisposeLock)
                    {
                        if (null != this.outputStream)
                        {
                            this.outputStream.Close();
                            this.outputStream = null;
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
                if (null == this.outputStream)
                {
                    // Open file first.
                    this.state = State.OpenOutputStream;
                }
                else
                {
                    if (!this.outputStream.CanWrite)
                    {
                        throw new NotSupportedException(string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.StreamMustSupportWriteException,
                            "outputStream"));
                    }

                    if (!this.outputStream.CanSeek)
                    {
                        throw new NotSupportedException(string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.StreamMustSupportSeekException,
                            "outputStream"));
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

        private Action<Action<ITransferController, bool>> GetOpenOutputStreamAction()
        {
            Debug.Assert(
                this.state == State.OpenOutputStream,
                "GetOpenOutputStreamAction called, but state isn't OpenOutputStream");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                CallbackState callbackState = new CallbackState { FinishDelegate = finishDelegate };

                try
                {
                    this.StartCallbackHandler();
                }
                catch (Exception e)
                {
                    this.SetErrorState(e, callbackState);
                    return;
                }

                // If destination file exists, query user whether to overwrite it.
                if (null != this.Manager.TransferOptions.OverwritePromptCallback)
                {
                    if (File.Exists(this.fileName))
                    {
                        if (!this.OverwritePromptCallbackHandler(
                                this.blob.Uri.ToString(),
                                this.fileName,
                                callbackState))
                        {
                            return;
                        }
                    }
                }

                try
                {
                    this.CancellationHandler.CheckCancellation();

                    // Attempt to open the file first so that we throw an exception before getting into the async work
                    this.outputStream = new FileStream(
                        this.fileName,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.Read);
                }
                catch (OperationCanceledException ex)
                {
                    this.SetErrorState(
                        ex,
                        callbackState);

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
                        callbackState);

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

                CallbackState callbackState = new CallbackState
                {
                    FinishDelegate = finishDelegate
                };

                try
                {
                    this.StartCallbackHandler();
                }
                catch (Exception e)
                {
                    this.SetErrorState(e, callbackState);
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
                    this.HandleFetchAttributesException(e, callbackState);
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
                this.HandleFetchAttributesException(e, callbackState);
                return;
            }

            if (this.keepLastWriteTime 
                && (!this.blob.Properties.LastModified.HasValue))
            {
                this.SetErrorState(
                    new BlobTransferException(
                        BlobTransferErrorCode.FailToGetSourceLastWriteTime,
                        Resources.FailedToGetSourceLastWriteTime),
                    callbackState);
                return;
            }

            if (this.blob.Properties.BlobType == BlobType.Unspecified)
            {
                this.SetErrorState(
                    new InvalidOperationException(
                        Resources.FailedToGetBlobTypeException),
                        callbackState);

                return;
            }

            if (string.IsNullOrEmpty(this.transferEntry.ETag))
            {
                if (0 != this.transferEntry.CheckPoint.EntryTransferOffset)
                {
                    this.SetErrorState(
                        new InvalidOperationException(
                            Resources.RestartableInfoCorruptedException),
                        callbackState);
                    return;
                }

                this.transferEntry.ETag = this.blob.Properties.ETag;
            }
            else if (!this.transferEntry.ETag.Equals(this.blob.Properties.ETag, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!this.RetransferModifiedCallbackHandler(this.blob.Name, callbackState))
                {
                    return;
                }
                else
                {
                    this.transferEntry.ETag = this.blob.Properties.ETag;
                    this.transferEntry.CheckPoint.Clear();
                }
            }
            else if ((this.transferEntry.CheckPoint.EntryTransferOffset > this.blob.Properties.Length)
                || (this.transferEntry.CheckPoint.EntryTransferOffset < 0))
            {
                this.SetErrorState(
                    new InvalidOperationException(
                        Resources.RestartableInfoCorruptedException),
                    callbackState);
                return;
            }

            this.md5HashStream = new MD5HashStream(
                this.outputStream,
                this.transferEntry.CheckPoint.EntryTransferOffset,
                this.checkMd5);

            // To trigger call for process call back and write transfer entry to restartable log
            this.ProgressCallbackHandler(0.0, 0.0);

            // Try to resize the output stream and check whether
            // the file can be put into destination.
            try
            {
                this.CancellationHandler.CheckCancellation();

                if (this.outputStream.Length != this.blob.Properties.Length)
                {
                    this.outputStream.SetLength(this.blob.Properties.Length);
                }
            }
            catch (Exception e)
            {
                this.SetErrorState(e, callbackState);
                return;
            }

            if (this.blob.Properties.BlobType == BlobType.PageBlob)
            {
                this.pageBlob = this.blob as CloudPageBlob;
                this.state = State.GetPageRanges;

                this.PrepareToGetPageRanges();

                if (0 == this.getPageRangesSpanCount)
                {
                    this.InitPageBlobDownloadInfo(callbackState);
                    return;
                }
            }
            else if (this.blob.Properties.BlobType == BlobType.BlockBlob)
            {
                this.blockBlob = this.blob as CloudBlockBlob;
                this.state = State.DownloadBlockList;
            }
            else
            {
                Debug.Fail("BlobType invalid.");
            }

            this.HasWork = true;

            bool isFinished = this.PostWork();
            callbackState.CallFinish(this, isFinished);
        }

        private void HandleFetchAttributesException(Exception e, CallbackState callbackState)
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

                CallbackState callbackState = new CallbackState 
                { 
                    FinishDelegate = finishDelegate 
                };

                try
                {
                    this.CancellationHandler.RegisterCancellableAsyncOper(
                        delegate
                        {
                            return this.blockBlob.BeginDownloadBlockList(
                                BlockListingFilter.Committed,
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

            IEnumerable<ListBlockItem> downloadedBlockList = null;

            try
            {
                downloadedBlockList = this.blockBlob.EndDownloadBlockList(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, callbackState);
                return;
            }

            Debug.Assert(
                null != downloadedBlockList,
                "downloadedBlockList not expected to be null");

            this.blocksToDownload = new List<BlockData>();
            this.availableBlockData = new ConcurrentDictionary<string, BlockCacheEntry>();
            this.currentBlocksDownloading = new ConcurrentDictionary<string, object>();

            this.nextDownloadIndex = 0;
            this.nextWriteIndex = 0;

            long blockOffset = 0;
            bool reachedTransferOffset = 0 == this.transferEntry.CheckPoint.EntryTransferOffset;
                        
            if (downloadedBlockList.Any())
            {
                // The block list we get back from the API call only gives us
                // the length of each block. We'll want to have the offset as well,
                // so we can download individual pieces in parallel.
                foreach (ListBlockItem listBlockItem in downloadedBlockList)
                {
                    Debug.Assert(
                        listBlockItem.Committed,
                        "listBlockItem expected to be Committed");

                    int blockLength = (int)listBlockItem.Length;

                    BlockData blockData = new BlockData
                    {
                        StartOffset = blockOffset,
                        Length = blockLength,
                        BlockName = listBlockItem.Name
                    };

                    this.blocksToDownload.Add(blockData);

                    blockOffset += blockLength;
                }
            }
            else
            {
                // If we get here we didn't receive any block list. This will occur
                // if the source blob was uploaded using the PutBlob API instead of
                // splitting the blob into individual blocks and committing using
                // PutBlockList. A blob uploaded in such manner can be at most 64MB
                // based on the current PutBlob REST API.
                // In this case we will split the file into pieces ourselves to allow
                // us to download the blob in parallel.
                long remainingBytes = this.blob.Properties.Length;

                while (remainingBytes > 0)
                {
                    int blockLength = (int)Math.Min(remainingBytes, this.Manager.TransferOptions.BlockSize);

                    BlockData blockData = new BlockData
                    {
                        StartOffset = blockOffset,
                        Length = blockLength,
                        BlockName = blockOffset.ToString(),
                    };

                    this.blocksToDownload.Add(blockData);

                    blockOffset += blockLength;
                    remainingBytes -= blockLength;
                }
            }

            if (!this.GetBlockNextDownloadIndex(callbackState))
            {
                return;
            }

            if (!this.md5HashStream.FinishedSeparateMd5Calculator)
            {
                this.state = State.CalculateMD5;
            }
            else
            {
                this.state = State.DownloadBlockBlob;
            }

            if (this.blocksToDownload.Count == this.nextDownloadIndex)
            {
                this.toDownloadItemsCountdownEvent = new CountdownEvent(1);

                if (!this.md5HashStream.FinishedSeparateMd5Calculator)
                {
                    this.HasWork = true;
                }

                this.SetTransferFinished(callbackState);
            }
            else
            {
                this.nextWriteIndex = this.nextDownloadIndex;
                this.toDownloadItemsCountdownEvent = new CountdownEvent(
                    this.blocksToDownload.Count - this.nextWriteIndex);

                this.transferStatusTracker = new TransferStatusAndTotalSpeedTracker(
                        this.blob.Properties.Length,
                        this.Manager.TransferOptions.Concurrency,
                        this.ProgressCallbackHandler,
                        this.Manager.GlobalDownloadSpeedTracker);
                
                this.transferStatusTracker.AddBytesTransferred(
                    this.transferEntry.CheckPoint.EntryTransferOffset);

                this.HasWork = true;

                bool isFinished = this.PostWork();
                callbackState.CallFinish(this, isFinished);
            }
        }

        private Action<Action<ITransferController, bool>> GetDownloadBlockBlobAction()
        {
            Debug.Assert(
                this.state == State.DownloadBlockBlob,
                "GetDownloadBlockBlobAction called, but state isn't DownloadBlockBlob");
            Debug.Assert(
                this.blocksToDownload != null,
                "blocksToDownload not expected to be null");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                if (!this.hasActiveFileWriter && this.nextWriteIndex < this.blocksToDownload.Count)
                {
                    string nextWriteBlockName = this.blocksToDownload[this.nextWriteIndex].BlockName;
                    BlockCacheEntry nextWriteBlockEntry;
                    if (this.availableBlockData.TryGetValue(nextWriteBlockName, out nextWriteBlockEntry))
                    {
                        if (null != nextWriteBlockEntry.MemoryBuffer)
                        {
                            DownloadBlockState writeBlockState = new DownloadBlockState
                            {
                                BlockData = this.blocksToDownload[this.nextWriteIndex],
                                CallbackState = new CallbackState { FinishDelegate = finishDelegate },
                                MemoryBuffer = nextWriteBlockEntry.MemoryBuffer,
                                BlockCacheEntry = nextWriteBlockEntry,
                            };

                            this.hasActiveFileWriter = true;
                            this.nextWriteIndex++;

                            this.SetBlockDownloadHasWork();
                            this.BeginWriteBlockData(writeBlockState);

                            return;
                        }
                    }
                }

                if (this.nextDownloadIndex < this.blocksToDownload.Count)
                {
                    BlockData blockData = this.blocksToDownload[this.nextDownloadIndex];

                    bool dataDownloaded = false;

                    // Update cache entry ref count by one if such a block already exists.
                    BlockCacheEntry nextDownloadBlockEntry;
                    if (this.availableBlockData.TryGetValue(blockData.BlockName, out nextDownloadBlockEntry))
                    {
                        if (nextDownloadBlockEntry.TryGetReference())
                        {
                            dataDownloaded = true;
                            this.nextDownloadIndex++;
                        }
                    }

                    if (!dataDownloaded)
                    {
                        // Attempt to reserve memory. If none available we'll
                        // retry some time later.
                        byte[] memoryBuffer = this.Manager.MemoryManager.RequireBuffer();
                        if (null != memoryBuffer)
                        {
                            nextDownloadBlockEntry = new BlockCacheEntry(this, blockData.BlockName);

                            if (this.availableBlockData.TryAdd(blockData.BlockName, nextDownloadBlockEntry))
                            {
                                Interlocked.Add(ref this.reservedMemory, blockData.Length);
                                this.nextDownloadIndex++;

                                // Setup download state.
                                DownloadBlockState downloadBlockState = new DownloadBlockState
                                {
                                    BlockData = blockData,
                                    CallbackState = new CallbackState { FinishDelegate = finishDelegate },
                                    MemoryBuffer = memoryBuffer,
                                    BlockCacheEntry = nextDownloadBlockEntry,
                                };

                                this.SetBlockDownloadHasWork();
                                this.BeginDownloadBlock(downloadBlockState);

                                return;
                            }
                            else
                            {
                                Debug.Fail("The previous block with the same name should has been removed.");
                            }
                        }
                    }
                }

                this.SetBlockDownloadHasWork();
                finishDelegate(this, this.PostWork());
            };
        }

        private void SetBlockDownloadHasWork()
        {
            if (this.HasWork)
            {
                return;
            }

            // Check if we have data available to write.
            int nextWriteIndexSnapshot = this.nextWriteIndex;
            if (!this.hasActiveFileWriter && nextWriteIndexSnapshot < this.blocksToDownload.Count)
            {
                string nextWriteBlockName = this.blocksToDownload[nextWriteIndexSnapshot].BlockName;
                BlockCacheEntry nextWriteBlockEntry;
                if (this.availableBlockData.TryGetValue(nextWriteBlockName, out nextWriteBlockEntry))
                {
                    this.HasWork = true;
                    return;
                }
            }

            // Check if we have blocks available to download.
            if (this.nextDownloadIndex < this.blocksToDownload.Count)
            {
                this.HasWork = true;
                return;
            }
        }

        private void BeginDownloadBlock(DownloadBlockState asyncState)
        {
            Debug.Assert(null != asyncState, "asyncState object expected");
            Debug.Assert(
                this.state == State.DownloadBlockBlob || this.state == State.Error,
                "BeginDownloadBlock called, but state isn't DownloadBlockBlob or Error");

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
                BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.OpenRead);

                AccessCondition accessCondition = AccessCondition.GenerateIfMatchCondition(this.blob.Properties.ETag);
                OperationContext operationContext = new OperationContext()
                {
                    ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                };

                asyncState.OperationContext = operationContext;

                // We're to download this block.
                asyncState.MemoryStream =
                    new MemoryStream(
                        asyncState.MemoryBuffer,
                        0,
                        asyncState.Length);

                this.CancellationHandler.RegisterCancellableAsyncOper(
                    delegate
                    {
                        return this.blob.BeginDownloadRangeToStream(
                            asyncState.MemoryStream,
                            asyncState.StartOffset,
                            asyncState.Length,
                            accessCondition,
                            requestOptions,
                            operationContext,
                            this.DownloadBlockBlobCallback,
                            asyncState);
                    });
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState);
                return;
            }
        }

        private void DownloadBlockBlobCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.DownloadBlockBlob || this.state == State.Error,
                "DownloadBlockBlobCallback called, but state isn't DownloadBlockBlob or Error");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            DownloadBlockState asyncState = asyncResult.AsyncState as DownloadBlockState;

            Debug.Assert(
                null != asyncState,
                "DownloadBlockState expected in AsyncState");

            bool needRetry = false;
            while (true)
            {
                if (needRetry)
                {
                    asyncState.MemoryStream.Seek(0, SeekOrigin.Begin);
                    this.CancellationHandler.RegisterCancellableAsyncOper(
                            delegate
                            {
                                return this.blob.BeginDownloadRangeToStream(
                                    asyncState.MemoryStream,
                                    asyncState.StartOffset,
                                    asyncState.Length,
                                    AccessCondition.GenerateIfMatchCondition(this.blob.Properties.ETag),
                                    this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.OpenRead),
                                    asyncState.OperationContext,
                                    this.DownloadBlockBlobCallback,
                                    asyncState);
                            });
                    needRetry = false;
                    return;
                }

                try
                {
                    this.blob.EndDownloadRangeToStream(asyncResult);
                    break;
                }
                catch (Exception e)
                {
                    if (e is Microsoft.WindowsAzure.Storage.StorageException && e.InnerException != null && e.InnerException is System.TimeoutException)
                    {
#if DEBUG
                        Console.WriteLine("retry on client timeout! {0}", e);
#endif
                        needRetry = true;
                    }
                    if (needRetry)
                    {
                        continue;
                    }

                    this.SetErrorState(e, asyncState);
                    return;
                }
            }

            // Update data cache and wait for writing to disk.
            asyncState.BlockCacheEntry.MemoryBuffer = asyncState.MemoryBuffer;

            // Set memory buffer to null. We don't want its dispose method to 
            // be called once our asyncState is disposed. The memory should 
            // not be reused yet, we still need to write it to disk.
            asyncState.MemoryBuffer = null;

            // Remove current block from active downloads.
            object dummy;
            this.currentBlocksDownloading.TryRemove(asyncState.BlockData.BlockName, out dummy);

            this.SetBlockDownloadHasWork();

            asyncState.CallbackState.CallFinish(this, this.PostWork());
            asyncState.Dispose();
        }

        private void BeginWriteBlockData(DownloadBlockState asyncState)
        {
            Debug.Assert(null != asyncState, "asyncState object expected");
            Debug.Assert(
                this.state == State.DownloadBlockBlob || this.state == State.Error,
                "BeginWriteBlockData called, but state isn't DownloadBlockBlob or Error");

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

                this.md5HashStream.BeginWrite(
                    asyncState.StartOffset,
                    asyncState.MemoryBuffer,
                    0,
                    asyncState.Length,
                    this.WriteBlockDataCallback,
                    asyncState);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState);
                return;
            }
        }

        private void WriteBlockDataCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.DownloadBlockBlob || this.state == State.Error,
                "WriteBlockDataCallback called, but state isn't DownloadBlockBlob or Error");

            DownloadBlockState asyncState = asyncResult.AsyncState as DownloadBlockState;

            Debug.Assert(
                null != asyncState,
                "DownloadBlockState expected in AsyncState");

            try
            {
                this.md5HashStream.EndWrite(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState);
                return;
            }

            lock (this.transferEntry.EntryLock)
            {
                this.transferEntry.CheckPoint.EntryTransferOffset += asyncState.Length;
            }

            if (!this.md5HashStream.MD5HashTransformBlock(
                asyncState.StartOffset,
                asyncState.MemoryBuffer,
                0,
                asyncState.Length,
                null,
                0))
            {
                return;
            }

            try
            {
                this.transferStatusTracker.AddBytesTransferred(asyncState.Length);
            }
            catch (BlobTransferCallbackException e)
            {
                // For we don't expect any exception from the tracker
                // just catch BlobTransferCallbackException here, 
                // if this cannot catch all of the exceptions, 
                // we need to add a new exception handler here.
                this.SetErrorState(e, asyncState);
                return;
            }

            asyncState.BlockCacheEntry.DecrementRefCount();

            this.hasActiveFileWriter = false;

            this.SetBlockDownloadHasWork();

            this.SetTransferFinished(asyncState);
        }

        private Action<Action<ITransferController, bool>> GetPageRangesAction()
        {
            Debug.Assert(
                (this.state == State.GetPageRanges) || (this.state == State.Error),
                "GetPageRangesAction called, but state isn't GetPageRanges or Error");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                int spanIndex = Interlocked.Increment(ref this.getPageRangesSpanIndex);
                
                this.HasWork = spanIndex < (this.getPageRangesSpanCount - 1);

                BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.GetPageRanges);
                AccessCondition accessCondition = AccessCondition.GenerateIfMatchCondition(this.pageBlob.Properties.ETag);
                OperationContext operationContext = new OperationContext()
                {
                    ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                };

                PageRangesSpan pageRangesSpan = new PageRangesSpan() 
                {
                    StartOffset = spanIndex * BlobTransferConstants.PageRangesSpanSize,
                    CallbackState = new CallbackState()
                    { 
                        FinishDelegate = finishDelegate 
                    },
                };

                pageRangesSpan.EndOffset = Math.Min(this.blob.Properties.Length, pageRangesSpan.StartOffset + BlobTransferConstants.PageRangesSpanSize) - 1;

                try
                {
                    this.CancellationHandler.RegisterCancellableAsyncOper(
                        delegate
                        {
                            return this.pageBlob.BeginGetPageRanges(
                                pageRangesSpan.StartOffset,
                                pageRangesSpan.EndOffset - pageRangesSpan.StartOffset + 1,
                                accessCondition,
                                requestOptions,
                                operationContext,
                                this.GetPageRangesCallback,
                                pageRangesSpan);
                        });
                }
                catch (Exception e)
                {
                    this.SetErrorState(e, pageRangesSpan.CallbackState);
                    return;
                }
            };
        }

        private void GetPageRangesCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                (this.state == State.GetPageRanges) || (this.state == State.Error),
                "GetPageRangesCallback called, but state isn't GetPageRanges or Error");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            PageRangesSpan pageRangesSpan = asyncResult.AsyncState as PageRangesSpan;

            Debug.Assert(
                null != pageRangesSpan,
                "PageRangesSpan expected in AsyncState");

            try
            {
                pageRangesSpan.PageRanges = new List<PageRange>(this.pageBlob.EndGetPageRanges(asyncResult));

                lock (this.getPageRangesLock)
                {
                    this.pageRangesSpanSet.Add(pageRangesSpan);
                }
            }
            catch (Exception e)
            {
                this.SetErrorState(e, pageRangesSpan.CallbackState);
                return;
            }

            if (this.getPageRangesCountDownEvent.Signal())
            {
                this.ArrangePageRanges();
                this.InitPageBlobDownloadInfo(pageRangesSpan.CallbackState);
            }
            else
            {
                pageRangesSpan.CallbackState.CallFinish(this, this.PostWork());
            }
        }

        private void InitPageBlobDownloadInfo(CallbackState callbackState)
        {
            this.ClearForGetPageRanges();
            if (!this.GetPageNextDownloadIndex(callbackState))
            {
                return;
            }

            if (!this.md5HashStream.FinishedSeparateMd5Calculator)
            {
                this.state = State.CalculateMD5;
            }
            else
            {
                this.state = State.DownloadPageBlob;
            }

            if (this.pageRangeList.Count == this.nextDownloadIndex)
            {
                this.toDownloadItemsCountdownEvent = new CountdownEvent(1);

                if (!this.md5HashStream.FinishedSeparateMd5Calculator)
                {
                    this.HasWork = true;
                }

                this.SetTransferFinished(callbackState);
            }
            else
            {
                this.nextWriteIndex = this.nextDownloadIndex;
                this.toDownloadItemsCountdownEvent = new CountdownEvent(
                    this.pageRangeList.Count - this.nextDownloadIndex);

                this.transferStatusTracker = new TransferStatusAndTotalSpeedTracker(
                        this.blob.Properties.Length,
                        this.Manager.TransferOptions.Concurrency,
                        this.ProgressCallbackHandler,
                        this.Manager.GlobalDownloadSpeedTracker);

                this.transferStatusTracker.AddBytesTransferred(
                    this.transferEntry.CheckPoint.EntryTransferOffset);

                this.HasWork = true;
                callbackState.CallFinish(this, this.PostWork());
            }
        }

        private void ClearForGetPageRanges()
        {
            this.pageRangesSpanSet = null;
            this.getPageRangesLock = null;
            this.getPageRangesCountDownEvent = null;
        }

        /// <summary>
        /// Turn raw page ranges get from Azure Storage in pageRangesSpanList
        /// into list of PageBlobRange.
        /// </summary>
        private void ArrangePageRanges()
        {
            long currentEndOffset = -1;

            IEnumerator<PageRangesSpan> enumerator = this.pageRangesSpanSet.OrderBy(pageRanges => pageRanges.StartOffset).GetEnumerator();
            bool hasValue = enumerator.MoveNext();

            PageRangesSpan current;
            PageRangesSpan next;

            if (hasValue)
            {
                current = enumerator.Current;

                while (hasValue)
                {
                    hasValue = enumerator.MoveNext();

                    if (!current.PageRanges.Any())
                    {
                        current = enumerator.Current;
                        continue;
                    }

                    if (hasValue)
                    {
                        next = enumerator.Current;

                        Debug.Assert(
                            (current.EndOffset + 1) == next.StartOffset,
                            "Something wrong with page ranges list.");

                        if (next.PageRanges.Any())
                        {
                            if ((current.PageRanges.Last().EndOffset + 1) == next.PageRanges.First().StartOffset)
                            {
                                PageRange mergedRange = new PageRange(
                                    current.PageRanges.Last().StartOffset,
                                    next.PageRanges.First().EndOffset);

                                current.PageRanges.RemoveAt(current.PageRanges.Count - 1);
                                next.PageRanges.RemoveAt(0);
                                current.PageRanges.Add(mergedRange);
                                current.EndOffset = mergedRange.EndOffset;
                                next.StartOffset = mergedRange.EndOffset + 1;

                                if (next.EndOffset == mergedRange.EndOffset)
                                {
                                    continue;
                                }
                            }
                        }
                    }

                    foreach (PageRange range in current.PageRanges)
                    {
                        // Check if we have a gap before the current range.
                        // If so we'll generate a range with HasData = false.
                        if (currentEndOffset != range.StartOffset - 1)
                        {
                            this.pageRangeList.AddRange(
                                new PageBlobRange
                                {
                                    StartOffset = currentEndOffset + 1,
                                    EndOffset = range.StartOffset - 1,
                                    HasData = false,
                                }.SplitRanges(this.Manager.TransferOptions.BlockSize));
                        }

                        this.pageRangeList.AddRange(
                            new PageBlobRange
                            {
                                StartOffset = range.StartOffset,
                                EndOffset = range.EndOffset,
                                HasData = true,
                            }.SplitRanges(this.Manager.TransferOptions.BlockSize));

                        currentEndOffset = range.EndOffset;
                    }

                    current = enumerator.Current;
                }
            }

            if (currentEndOffset < this.blob.Properties.Length - 1)
            {
                // Check if the last range reached the end of the blob.
                // If not we'll need to add one more empty range at the end.
                this.pageRangeList.AddRange(
                    new PageBlobRange
                    {
                        StartOffset = currentEndOffset + 1,
                        EndOffset = this.blob.Properties.Length - 1,
                        HasData = false,
                    }.SplitRanges(this.Manager.TransferOptions.BlockSize));
            }
        }

        private Action<Action<ITransferController, bool>> GetDownloadPageBlobAction()
        {
            Debug.Assert(
                this.state == State.DownloadPageBlob,
                "GetDownloadPageBlobAction called, but state isn't DownloadPageBlob");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                if (!this.hasActiveFileWriter && this.nextWriteIndex < this.pageRangeList.Count)
                {
                    long nextStartIndex = this.pageRangeList[this.nextWriteIndex].StartOffset;
                    byte[] nextWritePageBlobRangeEntry;
                    if (this.availablePageRangeData.TryGetValue(nextStartIndex, out nextWritePageBlobRangeEntry))
                    {
                        DownloadPageState writePageRangeState = new DownloadPageState
                        {
                            PageRange = this.pageRangeList[this.nextWriteIndex],
                            CallbackState = new CallbackState { FinishDelegate = finishDelegate },
                            MemoryBuffer = nextWritePageBlobRangeEntry,
                        };

                        this.hasActiveFileWriter = true;
                        this.nextWriteIndex++;

                        this.SetPageRangeDownloadHasWork();
                        this.BeginWritePageRangeData(writePageRangeState);

                        return;
                    }
                }

                if (this.nextDownloadIndex < this.pageRangeList.Count)
                {
                    PageBlobRange pageRangeData = this.pageRangeList[this.nextDownloadIndex];

                    // Attempt to reserve memory. If none available we'll
                    // retry some time later.
                    byte[] memoryBuffer = this.Manager.MemoryManager.RequireBuffer();
                    if (null != memoryBuffer)
                    {
                        Interlocked.Add(ref this.reservedMemory, memoryBuffer.Length);
                        this.nextDownloadIndex++;

                        DownloadPageState downloadPageRangeState = new DownloadPageState
                        {
                            PageRange = pageRangeData,
                            CallbackState = new CallbackState { FinishDelegate = finishDelegate },
                            MemoryBuffer = memoryBuffer,
                        };

                        this.SetPageRangeDownloadHasWork();
                        this.BeginDownloadPageRange(downloadPageRangeState);

                        return;
                    }
                }

                this.SetPageRangeDownloadHasWork();
                finishDelegate(this, this.PostWork());
            };
        }

        private void SetPageRangeDownloadHasWork()
        {
            if (this.HasWork)
            {
                return;
            }

            // Check if we have data available to write.
            int nextWriteIndexSnapshot = this.nextWriteIndex;
            if (!this.hasActiveFileWriter && nextWriteIndexSnapshot < this.pageRangeList.Count)
            {
                long nextStartIndex = this.pageRangeList[nextWriteIndexSnapshot].StartOffset;
                byte[] nextWritePageBlobRangeEntry;
                if (this.availablePageRangeData.TryGetValue(nextStartIndex, out nextWritePageBlobRangeEntry))
                {
                    this.HasWork = true;
                    return;
                }
            }

            // Check if we have page ranges available to download.
            if (this.nextDownloadIndex < this.pageRangeList.Count)
            {
                this.HasWork = true;
                return;
            }
        }

        private void BeginDownloadPageRange(DownloadPageState asyncState)
        {
            Debug.Assert(null != asyncState, "asyncState object expected");
            Debug.Assert(
                this.state == State.DownloadPageBlob || this.state == State.Error,
                "BeginDownloadPageRange called, but state isn't DownloadBlockBlob or Error");

            // If a parallel operation caused the controller to be placed in
            // error state exit early to avoid unnecessary I/O.
            if (this.state == State.Error)
            {
                asyncState.CallbackState.CallFinish(this, this.PostWork());
                asyncState.Dispose();

                return;
            }

            if (asyncState.PageRange.HasData)
            {
                try
                {
                    BlobRequestOptions requestOptions = this.Manager.TransferOptions.GetBlobRequestOptions(BlobRequestOperation.OpenRead);
                    AccessCondition accessCondition = AccessCondition.GenerateIfMatchCondition(this.blob.Properties.ETag);
                    OperationContext operationContext = new OperationContext()
                    {
                        ClientRequestID = this.Manager.TransferOptions.GetClientRequestId(),
                    };

                    asyncState.OperationContext = operationContext;

                    // We're to download this block.
                    asyncState.MemoryStream =
                        new MemoryStream(
                            asyncState.MemoryBuffer,
                            0,
                            asyncState.Length);

                    this.CancellationHandler.RegisterCancellableAsyncOper(
                        delegate
                        {
                            return this.blob.BeginDownloadRangeToStream(
                                asyncState.MemoryStream,
                                asyncState.StartOffset,
                                asyncState.Length,
                                accessCondition,
                                requestOptions,
                                operationContext,
                                this.DownloadPageRangeCallback,
                                asyncState);
                        });
                }
                catch (Exception e)
                {
                    this.SetErrorState(e, asyncState);
                    return;
                }
            }
            else
            {
                // Zero memory buffer.
                Array.Clear(asyncState.MemoryBuffer, 0, asyncState.MemoryBuffer.Length);

                this.availablePageRangeData.TryAdd(
                    asyncState.StartOffset,
                    asyncState.MemoryBuffer);

                // Set memory buffer to null. We don't want its dispose method to 
                // be called once our asyncState is disposed. The memory should 
                // not be reused yet, we still need to write it to disk.
                asyncState.MemoryBuffer = null;

                this.SetPageRangeDownloadHasWork();

                asyncState.CallbackState.CallFinish(this, this.PostWork());
                asyncState.Dispose();
            }
        }

        private void DownloadPageRangeCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.DownloadPageBlob || this.state == State.Error,
                "DownloadPageRangeCallback called, but state isn't DownloadPageBlob or Error");

            this.CancellationHandler.DeregisterCancellableAsyncOper(asyncResult as ICancellableAsyncResult);

            DownloadPageState asyncState = asyncResult.AsyncState as DownloadPageState;

            Debug.Assert(
                null != asyncState,
                "DownloadPageState expected in AsyncState");

            try
            {
                this.blob.EndDownloadRangeToStream(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState);
                return;
            }

            this.availablePageRangeData.TryAdd(
                asyncState.StartOffset,
                asyncState.MemoryBuffer);

            // Set memory buffer to null. We don't want its dispose method to 
            // be called once our asyncState is disposed. The memory should 
            // not be reused yet, we still need to write it to disk.
            asyncState.MemoryBuffer = null;

            this.SetPageRangeDownloadHasWork();

            asyncState.CallbackState.CallFinish(this, this.PostWork());
            asyncState.Dispose();
        }

        private void BeginWritePageRangeData(DownloadPageState asyncState)
        {
            Debug.Assert(null != asyncState, "asyncState object expected");
            Debug.Assert(
                this.state == State.DownloadPageBlob || this.state == State.Error,
                "BeginWritePageRangeData called, but state isn't DownloadPageBlob or Error");

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

                this.md5HashStream.BeginWrite(
                    asyncState.StartOffset,
                    asyncState.MemoryBuffer,
                    0,
                    asyncState.Length,
                    this.WritePageRangeDataCallback,
                    asyncState);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState);
                return;
            }
        }

        private void WritePageRangeDataCallback(IAsyncResult asyncResult)
        {
            Debug.Assert(null != asyncResult, "AsyncResult object expected");
            Debug.Assert(
                this.state == State.DownloadPageBlob || this.state == State.Error,
                "WritePageRangeDataCallback called, but state isn't DownloadPageBlob or Error");

            DownloadPageState asyncState = asyncResult.AsyncState as DownloadPageState;

            Debug.Assert(
                null != asyncState,
                "DownloadPageState expected in AsyncState");

            try
            {
                this.md5HashStream.EndWrite(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, asyncState);
                return;
            }

            lock (this.transferEntry.EntryLock)
            {
                this.transferEntry.CheckPoint.EntryTransferOffset += asyncState.Length;
            }

            if (!this.md5HashStream.MD5HashTransformBlock(
                asyncState.StartOffset,
                asyncState.MemoryBuffer,
                0,
                asyncState.Length,
                null,
                0))
            {
                // Error info has been set in Calculate MD5 action, just return
                asyncState.CallbackState.CallFinish(this, this.PostWork());
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

            byte[] dummy;
            this.availablePageRangeData.TryRemove(asyncState.StartOffset, out dummy);

            Interlocked.Add(ref this.reservedMemory, -asyncState.MemoryBuffer.Length);
            this.Manager.MemoryManager.ReleaseBuffer(asyncState.MemoryBuffer);

            this.hasActiveFileWriter = false;

            this.SetPageRangeDownloadHasWork();

            this.SetTransferFinished(asyncState);
        }

        private void SetTransferFinished(DownloadDataState downloadState)
        {
            this.SetTransferFinished(downloadState.CallbackState);

            downloadState.Dispose();
        }

        private void SetTransferFinished(CallbackState callbackState)
        {
            bool isFinished = false;
            if (this.toDownloadItemsCountdownEvent.Signal())
            {
                if (this.md5HashStream.CheckMd5Hash)
                {
                    this.md5HashStream.MD5HashTransformFinalBlock(new byte[0], 0, 0);

                    string calculatedMd5 = Convert.ToBase64String(this.md5HashStream.Hash);
                    string storedMd5 = this.blob.Properties.ContentMD5;

                    if (!calculatedMd5.Equals(storedMd5))
                    {
                        this.SetErrorState(
                            new InvalidOperationException(
                                string.Format(
                                    Resources.DownloadedMd5MismatchException,
                                    calculatedMd5,
                                    storedMd5)),
                            callbackState);

                        return;
                    }
                }

                this.CloseOwnedOutputStream();

                if (this.keepLastWriteTime)
                {
                    this.state = State.SetLastWriteTime;
                    this.HasWork = true;
                    isFinished = this.PostWork();
                }
                else
                {
                    isFinished = this.ChangeStatus();
                }
            }
            else
            {
                isFinished = this.PostWork();
            }

            callbackState.CallFinish(this, isFinished);
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

                if (this.blob.Properties.BlobType == BlobType.PageBlob)
                {
                    this.pageBlob = this.blob as CloudPageBlob;
                    this.state = State.DownloadPageBlob;
                }
                else if (this.blob.Properties.BlobType == BlobType.BlockBlob)
                {
                    this.blockBlob = this.blob as CloudBlockBlob;
                    this.state = State.DownloadBlockBlob;
                }

                this.HasWork = true;

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

                    bool isFinished = this.PostWork();
                    finishDelegate(this, isFinished);
                });
            };
        }

        private Action<Action<ITransferController, bool>> GetSetLastWriteTimeAction()
        {
            Debug.Assert(
                this.keepLastWriteTime,
                "keepLastModifiedTime must be true if we get here.");
            Debug.Assert(
                this.state == State.SetLastWriteTime,
                "GetSetLastModifiedTimeAction called, but state isn't SetLastModifiedTime.");
            Debug.Assert(
                !string.IsNullOrEmpty(this.fileName),
                "Destination is not a file, cannot set its last modified time.");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                ThreadPool.QueueUserWorkItem(
                    delegate
                    {
                        CallbackState callbackState = new CallbackState() { FinishDelegate = finishDelegate };

                        try
                        {
                            File.SetLastWriteTimeUtc(this.fileName, this.blob.Properties.LastModified.Value.DateTime);
                        }
                        catch (Exception ex)
                        {
                            this.SetErrorState(ex, callbackState);
                            return;
                        }

                        callbackState.CallFinish(this, this.ChangeStatus());
                    });
            };
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

                    if (!BlobExtensions.Equals(this.blob, rootBlob))
                    {
                        // TODO: do we need to fetch the ETag of source blob? If yes,
                        // when should we fetch it?
                        this.blob = rootBlob;
                    }
                }
                else
                {
                    if (this.blob.SnapshotTime.HasValue)
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

                    CallbackState callbackState = new CallbackState
                    {
                        FinishDelegate = finishDelegate
                    };

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
                    DeleteSnapshotsOption deleteSnapshotsOption = this.blob.SnapshotTime.HasValue ?
                        DeleteSnapshotsOption.None :
                        DeleteSnapshotsOption.IncludeSnapshots;

                    try
                    {
                        this.CancellationHandler.RegisterCancellableAsyncOper(
                            delegate
                            {
                                return this.blob.BeginDelete(
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

                    finishDelegate(this, this.ChangeStatus());
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
                this.blob.EndDelete(asyncResult);
            }
            catch (Exception e)
            {
                this.SetErrorState(e, callbackState);
                return;
            }

            callbackState.CallFinish(this, this.ChangeStatus());
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
        /// If the passed in downloadState holds any resources that need
        /// to be disposed they will be disposed at this time.
        /// The downloadState is expected to hold a CallbackState object,
        /// which will be used to indicate the download operation has finished.
        /// </summary>
        /// <param name="ex">Exception to record.</param>
        /// <param name="downloadState">DownloadState object.</param>
        private void SetErrorState(Exception ex, DownloadDataState downloadState)
        {
            Debug.Assert(
                this.state != State.Finished,
                "SetErrorState called, while controller already in Finished state");
            Debug.Assert(
                null != downloadState,
                "DownloadState expected");

            this.SetErrorState(ex, downloadState.CallbackState);

            downloadState.Dispose();
        }

        private bool GetNextDownloadIndex(Action binarySearch, CallbackState callbackState)
        {
            binarySearch();

            if (this.nextDownloadIndex < 0)
            {
                this.SetErrorState(
                    new InvalidOperationException(
                        Resources.RestartableInfoCorruptedException),
                    callbackState);

                return false;
            }

            return true;
        }

        private void PrepareToGetPageRanges()
        {
            // TODO: In restartalbe mode, we can just get page ranges behind the last transfer offset.
            this.getPageRangesSpanCount = (int)Math.Ceiling(this.blob.Properties.Length / (double)BlobTransferConstants.PageRangesSpanSize);

            this.getPageRangesCountDownEvent = new CountdownEvent(this.getPageRangesSpanCount);

            this.getPageRangesLock = new object();

            this.getPageRangesSpanIndex = -1;
            this.pageRangesSpanSet = new HashSet<PageRangesSpan>();

            this.pageRangeList = new List<PageBlobRange>();
            this.availablePageRangeData = new ConcurrentDictionary<long, byte[]>();

            this.nextDownloadIndex = 0;
            this.nextWriteIndex = 0;
        }

        /// <summary>
        /// Find next download index in blocksToDownload according to last EntryTransferOffset.
        /// </summary>
        /// <param name="callbackState">CallbackState object with reference to FinishDelegate in it.</param>
        /// <returns>
        /// If no StartOffset of BlockData in blocksToDownload or blob length is the same with EntryTransferOffset,
        /// returns false.
        /// Otherwise, we found the next BlockData index in blocksToDownload, returns true.
        /// </returns>
        private bool GetBlockNextDownloadIndex(CallbackState callbackState)
        {
            return this.GetNextDownloadIndex(
                delegate
                {
                    if (0 == this.transferEntry.CheckPoint.EntryTransferOffset)
                    {
                        this.nextDownloadIndex = 0;
                        return;
                    }

                    if (this.blob.Properties.Length == this.transferEntry.CheckPoint.EntryTransferOffset)
                    {
                        this.nextDownloadIndex = this.blocksToDownload.Count;
                        return;
                    }

                    this.nextDownloadIndex = this.blocksToDownload.BinarySearch(
                        new BlockData() { StartOffset = this.transferEntry.CheckPoint.EntryTransferOffset },
                        new BlockDataComparer());
                },
                callbackState);
        }

        /// <summary>
        /// Find next download index in pageRangeList according to last EntryTransferOffset.
        /// </summary>
        /// <param name="callbackState">CallbackState object with reference to FinishDelegate in it.</param>
        /// <returns>
        /// If no StartOffset of PageBlobRange in pageRangeList or blob length is the same with EntryTransferOffset,
        /// returns false.
        /// Otherwise, we found the next PageBlobRange index in pageRangeList, returns true.
        /// </returns>
        private bool GetPageNextDownloadIndex(CallbackState callbackState)
        {
            return this.GetNextDownloadIndex(
                delegate
                {
                    if (0 == this.transferEntry.CheckPoint.EntryTransferOffset)
                    {
                        this.nextDownloadIndex = 0;
                        return;
                    }

                    if (this.blob.Properties.Length == this.transferEntry.CheckPoint.EntryTransferOffset)
                    {
                        this.nextDownloadIndex = this.pageRangeList.Count;
                        return;
                    }

                    this.nextDownloadIndex = this.pageRangeList.BinarySearch(
                        new PageBlobRange() { StartOffset = this.transferEntry.CheckPoint.EntryTransferOffset },
                        new PageBlobRangeComparer());
                },
                callbackState);
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

        private class DownloadPageState : DownloadDataState
        {
            private PageBlobRange pageRange;

            public PageBlobRange PageRange
            {
                get
                {
                    return this.pageRange;
                }

                set
                {
                    this.pageRange = value;

                    this.StartOffset = value.StartOffset;
                    this.Length = (int)(value.EndOffset - value.StartOffset + 1);
                }
            }
        }

        private class DownloadBlockState : DownloadDataState
        {
            private BlockData blockData;

            public BlockData BlockData
            {
                get
                {
                    return this.blockData;
                }

                set
                {
                    this.blockData = value;

                    this.StartOffset = value.StartOffset;
                    this.Length = value.Length;
                }
            }

            public BlockCacheEntry BlockCacheEntry
            {
                get;
                set;
            }
        }

        private class PageBlobRange
        {
            public long StartOffset
            {
                get;
                set;
            }

            public long EndOffset
            {
                get;
                set;
            }

            public bool HasData
            {
                get;
                set;
            }

            /// <summary>
            /// Split a PageBlobRange into multiple PageBlobRange objects, each at most maxPageRangeSize long.
            /// </summary>
            /// <param name="maxPageRangeSize">Maximum length for each piece.</param>
            /// <returns>List of PageBlobRange objects.</returns>
            public IEnumerable<PageBlobRange> SplitRanges(long maxPageRangeSize)
            {
                long startOffset = this.StartOffset;
                long rangeSize = this.EndOffset - this.StartOffset + 1;

                do
                {
                    PageBlobRange subRange = new PageBlobRange
                    {
                        StartOffset = startOffset,
                        EndOffset = startOffset + Math.Min(rangeSize, maxPageRangeSize) - 1,
                        HasData = this.HasData,
                    };

                    startOffset += maxPageRangeSize;
                    rangeSize -= maxPageRangeSize;

                    yield return subRange;
                }
                while (rangeSize > 0);
            }
        }

        private class PageBlobRangeComparer : IComparer<PageBlobRange>
        {
            public int Compare(PageBlobRange x, PageBlobRange y)
            {
                return Math.Sign(x.StartOffset - y.StartOffset);
            }
        }

        private class PageRangesSpan
        {
            public long StartOffset
            {
                get;
                set;
            }

            public long EndOffset
            {
                get;
                set;
            }

            public List<PageRange> PageRanges
            {
                get;
                set;
            }

            public CallbackState CallbackState
            {
                get;
                set;
            }
        }

        private class BlockData
        {
            public long StartOffset
            {
                get;
                set;
            }

            public int Length
            {
                get;
                set;
            }

            public string BlockName
            {
                get;
                set;
            }
        }

        private class BlockDataComparer : IComparer<BlockData>
        {
            public int Compare(BlockData x, BlockData y)
            {
                return Math.Sign(x.StartOffset - y.StartOffset);
            }
        }

        private class BlockCacheEntry
        {
            private BlobDownloadController downloadController;
            private string blockName;
            private int refCount;

            private object removeLockObject = new object();

            public BlockCacheEntry(BlobDownloadController downloadController, string blockName)
            {
                this.downloadController = downloadController;
                this.blockName = blockName;
                this.refCount = 1;
            }

            public byte[] MemoryBuffer
            {
                get;
                set;
            }

            public bool TryGetReference()
            {
                Debug.Assert(this.refCount > 0, "BlockCacheEntry.IncrementRefCount called after buffer already released.");

                if (1 == Interlocked.Increment(ref this.refCount))
                {
                    lock (this.removeLockObject)
                    {
                        if (null == this.MemoryBuffer)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            public void DecrementRefCount()
            {
                Debug.Assert(this.refCount > 0, "BlockCacheEntry.DecrementRefCount called when ref count already zero.");

                if (0 == Interlocked.Decrement(ref this.refCount))
                {
                    lock (this.removeLockObject)
                    {
                        if (0 == this.refCount)
                        {
                            BlockCacheEntry dummy;
                            this.downloadController.availableBlockData.TryRemove(this.blockName, out dummy);

                            Interlocked.Add(ref this.downloadController.reservedMemory, -this.MemoryBuffer.Length);
                            this.downloadController.Manager.MemoryManager.ReleaseBuffer(this.MemoryBuffer);

                            this.MemoryBuffer = null;
                        }
                    }
                }
            }
        }
    }
}