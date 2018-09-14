#pragma warning disable 0420 // turn off 'a reference to a volatile field will not be treated as volatile' during CAS.
//-----------------------------------------------------------------------------
// <copyright file="BlobTransferManager.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
// BlobTransferManager class, used for simultanously uploading/downloading 
// multiple blobs to/from Windows Azure storage.
// </summary>
//-----------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.Hpc.Azure.DataMovement.TransferControllers;
    using Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers;

    /// <summary>
    /// BlobTransferManager class, used for simultanously uploading/downloading
    /// multiple blobs to/from Windows Azure storage.
    /// </summary>
    public sealed class BlobTransferManager : IDisposable
    {
        /// <summary>
        /// Main collection of transfer controllers.
        /// </summary>
        private BlockingCollection<ITransferController> controllerQueue;

        /// <summary>
        /// Collection of transfer monitors.
        /// </summary>
        private BlockingCollection<ITransferController> monitorQueue;

        /// <summary>
        /// Internal queue for the main controllers collection.
        /// </summary>
        private ConcurrentQueue<ITransferController> internalControllerQueue;

        /// <summary>
        /// Internal queue for the monitors collection.
        /// </summary>
        private ConcurrentQueue<ITransferController> internalMonitorQueue;

        /// <summary>
        /// Indicates the number of unfinished transfer controllers that have the
        /// ability to add controllers to the main controllers collection.
        /// </summary>
        private CountdownEvent controllerAddersCountdownEvent;

        /// <summary>
        /// Indicates the number of unfinished transfer controllers that have the
        /// ability to add monitors to the monitors collection.
        /// </summary>
        private CountdownEvent monitorAddersCountdownEvent;

        /// <summary>
        /// Indicates whether there are active controller adders.
        /// </summary>
        private int hasSignaledControllerAdders;

        /// <summary>
        /// Indicates whether there are active monitor adders.
        /// </summary>
        private int hasSignaledMonitorAdders;

        /// <summary>
        /// A buffer from which we select a transfer controller and add it into 
        /// active tasks when the bucket of active tasks is not full.
        /// </summary>
        private ConcurrentDictionary<ITransferController, object> activeControllerItems =
            new ConcurrentDictionary<ITransferController, object>();

        /// <summary>
        /// A buffer from which we select a transfer monitor and add it into active
        /// tasks when the bucket of active tasks is not full and no controllers is
        /// available.
        /// </summary>
        private ConcurrentDictionary<ITransferController, object> activeMonitorItems =
            new ConcurrentDictionary<ITransferController, object>();

        /// <summary>
        /// CancellationToken source.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource =
            new CancellationTokenSource();

        /// <summary>
        /// Transfer options that this manager will pass to transfer controllers.
        /// </summary>
        private BlobTransferOptions transferOptions;

        /// <summary>
        /// Number of active tasks.
        /// </summary>
        private volatile int activeTasks;

        /// <summary>
        /// Wait handle event for completion.
        /// </summary>
        private ManualResetEventSlim controllerResetEvent =
            new ManualResetEventSlim();

        /// <summary>
        /// A pool of memory buffer objects, used to limit total consumed memory.
        /// </summary>
        private MemoryManager memoryManager;

        /// <summary>
        /// Random object to generate random numbers.
        /// </summary>
        private Random randomGenerator;

        /// <summary>
        /// Object to check whether user cancel the work.
        /// </summary>
        private CancellationChecker cancellationChecker = new CancellationChecker();

        /// <summary>
        /// Represents a callback delegate that has been registered with a CancellationToken.
        /// </summary>
        private CancellationTokenRegistration cancellationTokenRegistration;

        /// <summary>
        /// Object to track download speed in whole BlobTransferManager.
        /// </summary>
        private TransferSpeedTracker globalDownloadSpeedTracker;

        /// <summary>
        /// Object to track upload speed in whole BlobTransferManager.
        /// </summary>
        private TransferSpeedTracker globalUploadSpeedTracker;

        /// <summary>
        /// Object to track copy speed in whole BlobTransferManager.
        /// </summary>
        private TransferSpeedTracker globalCopySpeedTracker;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="BlobTransferManager" /> class.
        /// </summary>        
        public BlobTransferManager()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="BlobTransferManager" /> class.
        /// </summary>
        /// <param name="options">BlobTransfer options.</param>
        public BlobTransferManager(BlobTransferOptions options)
        {
            // If no options specified create a default one.
            this.transferOptions = options ?? new BlobTransferOptions();

            this.globalDownloadSpeedTracker = new TransferSpeedTracker(
                this.OnGlobalDownloadSpeed,
                this.transferOptions.Concurrency);
            this.globalUploadSpeedTracker = new TransferSpeedTracker(
                this.OnGlobalUploadSpeed,
                this.transferOptions.Concurrency);
            this.globalCopySpeedTracker = new TransferSpeedTracker(
                this.OnGlobalCopySpeed,
                this.transferOptions.Concurrency);

            this.internalControllerQueue = new ConcurrentQueue<ITransferController>();
            this.internalMonitorQueue = new ConcurrentQueue<ITransferController>();

            this.controllerQueue = new BlockingCollection<ITransferController>(
                this.internalControllerQueue);
            this.monitorQueue = new BlockingCollection<ITransferController>(
                this.internalMonitorQueue);

            this.controllerAddersCountdownEvent = new CountdownEvent(1);
            this.monitorAddersCountdownEvent = new CountdownEvent(1);

            this.hasSignaledControllerAdders = 0;
            this.hasSignaledMonitorAdders = 0;

            this.activeTasks = 0;

            this.memoryManager = new MemoryManager(
                this.transferOptions.MaximumCacheSize,
                this.transferOptions.BlockSize);

            this.randomGenerator = new Random();

            this.cancellationTokenRegistration = this.cancellationTokenSource.Token.Register(this.cancellationChecker.Cancel);

            this.StartControllerThread();
        }

        /// <summary>
        /// Finalizes an instance of the 
        /// <see cref="BlobTransferManager" /> class.
        /// </summary>
        ~BlobTransferManager()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event triggered during transfer to notify about current 
        /// global download speed.
        /// After calling CancelWorkAndWaitForCompletion no events are 
        /// guaranteed to be triggered for any remaining transfers.
        /// </summary>
        public event EventHandler<BlobTransferManagerEventArgs> GlobalDownloadSpeedUpdated;

        /// <summary>
        /// Event triggered during transfer to notify about current 
        /// global upload speed.
        /// After calling CancelWorkAndWaitForCompletion no events are 
        /// guaranteed to be triggered for any remaining transfers.
        /// </summary>
        public event EventHandler<BlobTransferManagerEventArgs> GlobalUploadSpeedUpdated;

        /// <summary>
        /// Event triggered during transfer to notify about current 
        /// global copy speed.
        /// After calling CancelWorkAndWaitForCompletion no events are 
        /// guaranteed to be triggered for any remaining transfers.
        /// </summary>
        public event EventHandler<BlobTransferManagerEventArgs> GlobalCopySpeedUpdated;

        /// <summary>
        /// Gets the amount of items currently in the queue.
        /// Any items in the queue haven't started processing yet.
        /// </summary>
        /// <value>Amount of transfer operations queued.</value>
        public int QueuedItemsCount
        {
            get
            {
                return this.controllerQueue.Count + this.monitorQueue.Count;
            }
        }

        /// <summary>
        /// Gets the transfer options that this manager will pass to
        /// transfer controllers.
        /// </summary>
        internal BlobTransferOptions TransferOptions
        {
            get
            {
                return this.transferOptions;
            }
        }

        internal TransferSpeedTracker GlobalDownloadSpeedTracker
        {
            get
            {
                return this.globalDownloadSpeedTracker;
            }
        }

        internal TransferSpeedTracker GlobalUploadSpeedTracker
        {
            get
            {
                return this.globalUploadSpeedTracker;
            }
        }

        internal TransferSpeedTracker GlobalCopySpeedTracker
        {
            get
            {
                return this.globalCopySpeedTracker;
            }
        }

        internal CancellationTokenSource CancellationTokenSource
        {
            get
            {
                return this.cancellationTokenSource;
            }
        }

        internal MemoryManager MemoryManager
        {
            get
            {
                return this.memoryManager;
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
        /// Queues an upload operation.
        /// </summary>
        /// <param name="blob">Blob to upload.</param>
        /// <param name="fileName">Source file.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        public void QueueUpload(
            ICloudBlob blob,
            string fileName,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            this.QueueUpload(
                new BlobTransferFileTransferEntry(),
                blob,
                fileName,
                null,
                false,
                startCallback,
                progressCallback,
                finishCallback,
                userData);
        }

        /// <summary>
        /// Queues an upload operation.
        /// </summary>
        /// <param name="blob">Blob to upload.</param>
        /// <param name="inputStream">Source stream.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        public void QueueUpload(
            ICloudBlob blob,
            Stream inputStream,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            this.QueueUpload(
                new BlobTransferFileTransferEntry(),
                blob,
                null,
                inputStream,
                false,
                startCallback,
                progressCallback,
                finishCallback,
                userData);
        }

        /// <summary>
        /// Queues a download operation.
        /// </summary>
        /// <param name="blob">Blob to download.</param>
        /// <param name="fileName">Destination file.</param>
        /// <param name="checkMd5">Indicates whether to check MD5 hash after finishing transfer. 
        /// Only applicable when downloading blobs from Azure Storage to a local file.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        public void QueueDownload(
            ICloudBlob blob,
            string fileName,
            bool checkMd5,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            this.QueueDownload(
                new BlobTransferFileTransferEntry(),
                blob,
                fileName,
                null,
                checkMd5,
                false,
                false,
                startCallback,
                progressCallback,
                finishCallback,
                userData);
        }

        /// <summary>
        /// Queues a download operation.
        /// </summary>
        /// <param name="blob">Blob to download.</param>
        /// <param name="outputStream">Destination stream.</param>
        /// <param name="checkMd5">Indicates whether to check MD5 hash after finishing transfer. 
        /// Only applicable when downloading blobs from Azure Storage to a local file.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        public void QueueDownload(
            ICloudBlob blob,
            Stream outputStream,
            bool checkMd5,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            this.QueueDownload(
                new BlobTransferFileTransferEntry(),
                blob,
                null,
                outputStream,
                checkMd5,
                false,
                false,
                startCallback,
                progressCallback,
                finishCallback,
                userData);
        }

        /// <summary>
        /// Queues a blob StartCopy operation. The operation is finished when 
        /// StartCopy query is sent to server.
        /// Destination BlobType is determined by Azure Storage. For source blobs
        /// inside Azure Storage, a copy operation preserves the blob type; for 
        /// those outside, they will be copied to block blobs.
        /// </summary>
        /// <param name="sourceUri">Source uri to StartCopy from.</param>
        /// <param name="destinationContainer">Target container object.</param>
        /// <param name="destinationBlobName">Path under the container of target blob object.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        public void QueueBlobStartCopy(
            Uri sourceUri,
            CloudBlobContainer destinationContainer,
            string destinationBlobName,
            Action<object> startCallback,
            Action<object, string, Exception> finishCallback,
            object userData)
        {
            this.QueueBlobStartCopy(
                new BlobTransferFileTransferEntry(),
                sourceUri,
                null,
                destinationContainer,
                destinationBlobName,
                startCallback,
                null,
                finishCallback,
                userData);
        }

        /// <summary>
        /// Queues a blob StartCopy operation. The operation is finished when 
        /// StartCopy query is sent to server. This copy operation preserves 
        /// the blob type.
        /// </summary>
        /// <param name="sourceBlob">Source blob to StartCopy from.</param>
        /// <param name="destinationContainer">Target container object.</param>
        /// <param name="destinationBlobName">Path under the container of target blob object.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        public void QueueBlobStartCopy(
            ICloudBlob sourceBlob,
            CloudBlobContainer destinationContainer,
            string destinationBlobName,
            Action<object> startCallback,
            Action<object, string, Exception> finishCallback,
            object userData)
        {
            this.QueueBlobStartCopy(
                new BlobTransferFileTransferEntry(),
                null,
                sourceBlob,
                destinationContainer,
                destinationBlobName,
                startCallback,
                null,
                finishCallback,
                userData);
        }

        /// <summary>
        /// Queues a blobcopy operation.
        /// Destination BlobType is determined by Azure Storage. For source blobs
        /// inside Azure Storage, a copy operation preserves the blob type; for 
        /// those outside, they will be copied to block blobs.
        /// </summary>
        /// <param name="sourceUri">Source uri to StartCopy from.</param>
        /// <param name="destinationContainer">Target container object.</param>
        /// <param name="destinationBlobName">Path under the container of target blob object.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        public void QueueBlobCopy(
            Uri sourceUri,
            CloudBlobContainer destinationContainer,
            string destinationBlobName,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            this.QueueBlobCopy(
                new BlobTransferFileTransferEntry(),
                sourceUri,
                null,
                destinationContainer,
                destinationBlobName,
                false,
                startCallback,
                progressCallback,
                finishCallback,
                userData);
        }

        /// <summary>
        /// Queues a blobcopy operation. This copy operation preserves the blob type.
        /// </summary>
        /// <param name="sourceBlob">Source blob to StartCopy from.</param>
        /// <param name="destinationContainer">Target container object.</param>
        /// <param name="destinationBlobName">Path under the container of target blob object.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        public void QueueBlobCopy(
            ICloudBlob sourceBlob,
            CloudBlobContainer destinationContainer,
            string destinationBlobName,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            this.QueueBlobCopy(
                new BlobTransferFileTransferEntry(),
                null,
                sourceBlob,
                destinationContainer,
                destinationBlobName,
                false,
                startCallback,
                progressCallback,
                finishCallback,
                userData);
        }

        /// <summary>
        /// Queues a recursive transfer operation.
        /// </summary>
        /// <param name="sourceLocation">Source location to copy files from.
        /// </param>
        /// <param name="destinationLocation">Destination location to copy 
        /// files to.</param>
        /// <param name="options">Options object.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="startFileCallback">Indidual file start transfer callback.</param>
        /// <param name="progressFileCallback">Individual file progress transfer callback.</param>
        /// <param name="finishFileCallback">Individual file finish transfer callback.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        public void QueueRecursiveTransfer(
            string sourceLocation,
            string destinationLocation,
            BlobTransferRecursiveTransferOptions options,
            Action<object> startCallback,
            Action<object, Exception> finishCallback,
            Action<object, EntryData> startFileCallback,
            Action<object, EntryData, double, double> progressFileCallback,
            Action<object, EntryData, Exception> finishFileCallback,
            object userData)
        {
            this.cancellationChecker.CheckCancellation();

            this.AddToQueue(
                new BlobTransferRecursiveTransferItem(
                    this,
                    sourceLocation,
                    destinationLocation,
                    options,
                    startCallback,
                    finishCallback,
                    startFileCallback,
                    progressFileCallback,
                    finishFileCallback,
                    userData),
                this.cancellationTokenSource.Token,
                true);
        }

        /// <summary>
        /// Blocks untils the queue is empty and all transfers have been 
        /// completed.
        /// </summary>
        public void WaitForCompletion()
        {
            this.SignalQueueAdders(false, true, true);
            this.controllerResetEvent.Wait();
        }

        /// <summary>
        /// Cancels any remaining queued work.
        /// </summary>
        public void CancelWork()
        {
            this.cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Cancels any remaining queued work and block until transfers still 
        /// in progress have been completed.
        /// </summary>
        public void CancelWorkAndWaitForCompletion()
        {
            this.CancelWork();
            this.WaitForCompletion();
        }

        /// <summary>
        /// Queues a blobcopy operation.
        /// Destination BlobType is determined by Azure Storage. For source blobs
        /// inside Azure Storage, a copy operation preserves the blob type; for 
        /// those outside, they will be copied to block blobs.
        /// </summary>
        /// <param name="transferEntry">Transfer entry to store transfer information.</param>
        /// <param name="sourceUri">Source uri to StartCopy from. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="sourceBlob">Source blob to StartCopy from. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="destinationContainer">Target container object.</param>
        /// <param name="destinationBlobName">Path under the container of target blob object.</param>
        /// <param name="moveSource">Indicates whether to remove source file after finishing transfer.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        internal void QueueBlobCopy(
            BlobTransferFileTransferEntry transferEntry,
            Uri sourceUri,
            ICloudBlob sourceBlob,
            CloudBlobContainer destinationContainer,
            string destinationBlobName,
            bool moveSource,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            this.cancellationChecker.CheckCancellation();

            Action<object, string, Exception> startCopyFinishCallback = null;

            if (null != finishCallback)
            {
                startCopyFinishCallback = delegate(object startCopyUserData, string copyId, Exception ex)
                {
                    finishCallback(startCopyUserData, ex);
                };
            }

            this.AddToQueue(
                new BlobStartCopyController(
                    this,
                    transferEntry,
                    sourceUri,
                    sourceBlob,
                    destinationContainer,
                    destinationBlobName,
                    true,
                    moveSource,
                    startCallback,
                    progressCallback,
                    startCopyFinishCallback,
                    userData),
                this.cancellationTokenSource.Token,
                true);
        }

        /// <summary>
        /// Queues a blobcopy start operation.
        /// Destination BlobType is determined by Azure Storage. For source blobs
        /// inside Azure Storage, a copy operation preserves the blob type; for 
        /// those outside, they will be copied to block blobs.
        /// </summary>
        /// <param name="transferEntry">Transfer entry to store transfer information.</param>
        /// <param name="sourceUri">Source uri to StartCopy from. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="sourceBlob">Source blob to StartCopy from. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="destinationContainer">Target container object.</param>
        /// <param name="destinationBlobName">Path under the container of target blob object.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        internal void QueueBlobStartCopy(
            BlobTransferFileTransferEntry transferEntry,
            Uri sourceUri,
            ICloudBlob sourceBlob,
            CloudBlobContainer destinationContainer,
            string destinationBlobName,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, string, Exception> finishCallback,
            object userData)
        {
            this.cancellationChecker.CheckCancellation();

            this.AddToQueue(
                new BlobStartCopyController(
                    this,
                    transferEntry,
                    sourceUri,
                    sourceBlob,
                    destinationContainer,
                    destinationBlobName,
                    false,
                    false,
                    startCallback,
                    progressCallback,
                    finishCallback,
                    userData),
                this.cancellationTokenSource.Token,
                true);
        }

        /// <summary>
        /// Queues a blobcopy monitor.
        /// </summary>
        /// <param name="transferEntry">Transfer entry to store transfer information.</param>
        /// <param name="sourceUri">Source uri. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="sourceBlob">Source blob. Exactly one of sourceBlob and sourceUri should be non-null.</param>
        /// <param name="destinationBlob">Destination blob object.</param>
        /// <param name="moveSource">Indicates whether to remove source file after finishing transfer.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        internal void QueueBlobCopyMonitor(
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
            this.cancellationChecker.CheckCancellation();

            this.AddToQueue(
                new BlobCopyMonitor(
                    this,
                    transferEntry,
                    sourceUri,
                    sourceBlob,
                    destinationBlob,
                    moveSource,
                    startCallback,
                    progressCallback,
                    finishCallback,
                    userData),
                this.cancellationTokenSource.Token,
                false);
        }

        /// <summary>
        /// Queues an upload operation.
        /// </summary>
        /// <param name="transferEntry">Transfer entry to store transfer information.</param>
        /// <param name="blob">Blob to upload.</param>
        /// <param name="fileName">Source file. Exactly one of fileName and outputStream should be non-null.</param>
        /// <param name="inputStream">Source stream. Exactly one of fileName and outputStream should be non-null.</param>
        /// <param name="moveSource">Indicates whether to remove source file after finishing transfer.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        internal void QueueUpload(
            BlobTransferFileTransferEntry transferEntry,
            ICloudBlob blob,
            string fileName,
            Stream inputStream,
            bool moveSource,
            Action<object> startCallback,
            Action<object, double, double> progressCallback,
            Action<object, Exception> finishCallback,
            object userData)
        {
            this.cancellationChecker.CheckCancellation();

            if (BlobType.PageBlob == blob.BlobType)
            {
                this.AddToQueue(
                    new PageBlobUploadController(
                        this,
                        transferEntry,
                        blob as CloudPageBlob,
                        fileName,
                        inputStream,
                        moveSource,
                        startCallback,
                        progressCallback,
                        finishCallback,
                        userData),
                    this.cancellationTokenSource.Token,
                    true);
            }
            else if (BlobType.BlockBlob == blob.BlobType)
            {
                this.AddToQueue(
                    new BlockBlobUploadController(
                        this,
                        transferEntry,
                        blob as CloudBlockBlob,
                        fileName,
                        inputStream,
                        moveSource,
                        startCallback,
                        progressCallback,
                        finishCallback,
                        userData),
                    this.cancellationTokenSource.Token,
                    true);
            }
            else
            {
                throw new InvalidOperationException(Resources.OnlySupportTwoBlobTypesException);
            }
        }

        /// <summary>
        /// Queues a download operation.
        /// </summary>
        /// <param name="transferEntry">Transfer entry to store transfer information.</param>
        /// <param name="blob">Blob to download.</param>
        /// <param name="fileName">Destination file. Exactly one of fileName and outputStream should be non-null.</param>
        /// <param name="outputStream">Destination stream. Exactly one of fileName and outputStream should be non-null.</param>
        /// <param name="checkMd5">Indicates whether to check MD5 hash after finishing transfer. 
        /// Only applicable when downloading blobs from Azure Storage to a local file.</param>
        /// <param name="moveSource">Indicates whether to remove source file after finishing transfer.</param>
        /// <param name="keepLastWriteTime">Indicates whether to keep destination's 
        /// last write time to be the same with source blob's.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="progressCallback">Progress callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        internal void QueueDownload(
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
            this.cancellationChecker.CheckCancellation();

            this.AddToQueue(
                new BlobDownloadController(
                    this,
                    transferEntry,
                    blob,
                    fileName,
                    outputStream,
                    checkMd5,
                    moveSource,
                    keepLastWriteTime,
                    startCallback,
                    progressCallback,
                    finishCallback,
                    userData),
                this.cancellationTokenSource.Token,
                true);
        }

        private static void FillInQueue(
            ConcurrentDictionary<ITransferController, object> activeItems,
            BlockingCollection<ITransferController> collection,
            ConcurrentQueue<ITransferController> queueInCollection,
            CancellationToken token,
            int countUpperBound,
            SpinWait sw)
        {
            while (!token.IsCancellationRequested &&
                activeItems.Count < countUpperBound &&
                !queueInCollection.IsEmpty)
            {
                // Ensure we keep a decent number of transfer items active in 
                // parallel. The IsEmpty check here is safe, as we are the only
                // consumer of this queue, other threads are only producers,
                // thus after this check we are guaranteed there is at least
                // one item in the queue.
                sw.Reset();

                ITransferController transferItem = null;

                try
                {
                    transferItem = collection.Take(token);
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                activeItems.TryAdd(transferItem, null);
            }
        }

        /// <summary>
        /// Called to indicate the total download speed. 
        /// Used to trigger the GlobalDownloadSpeed event.
        /// </summary>
        /// <param name="globalSpeed">Global download speed.</param>
        private void OnGlobalDownloadSpeed(double globalSpeed)
        {
            EventHandler<BlobTransferManagerEventArgs> tempEventHandler =
                this.GlobalDownloadSpeedUpdated;

            if (null != tempEventHandler)
            {
                tempEventHandler(
                    this,
                    new BlobTransferManagerEventArgs(globalSpeed < 0 ? 0 : globalSpeed));
            }
        }

        /// <summary>
        /// Called to indicate the total upload speed. 
        /// Used to trigger the GlobalUploadSpeed event.
        /// </summary>
        /// <param name="globalSpeed">Global upload speed.</param>
        private void OnGlobalUploadSpeed(double globalSpeed)
        {
            EventHandler<BlobTransferManagerEventArgs> tempEventHandler =
                this.GlobalUploadSpeedUpdated;

            if (null != tempEventHandler)
            {
                tempEventHandler(
                    this,
                    new BlobTransferManagerEventArgs(globalSpeed < 0 ? 0 : globalSpeed));
            }
        }

        /// <summary>
        /// Called to indicate the total copy speed. 
        /// Used to trigger the GlobalCopySpeed event.
        /// </summary>
        /// <param name="globalSpeed">Global copy speed.</param>
        private void OnGlobalCopySpeed(double globalSpeed)
        {
            EventHandler<BlobTransferManagerEventArgs> tempEventHandler =
                this.GlobalCopySpeedUpdated;

            if (null != tempEventHandler)
            {
                tempEventHandler(
                    this,
                    new BlobTransferManagerEventArgs(globalSpeed < 0 ? 0 : globalSpeed));
            }
        }

        /// <summary>
        /// Private dispose method to release managed/unmanaged objects.
        /// If disposing = true clean up managed resources as well as 
        /// unmanaged resources.
        /// If disposing = false only clean up unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether or not to dispose 
        /// managed resources.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    this.cancellationTokenRegistration.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Object has been disposed before, just catch this exception, do nothing else.
                }

                if (null != this.controllerAddersCountdownEvent)
                {
                    this.controllerAddersCountdownEvent.Dispose();
                    this.controllerAddersCountdownEvent = null;
                }

                if (null != this.monitorAddersCountdownEvent)
                {
                    this.monitorAddersCountdownEvent.Dispose();
                    this.monitorAddersCountdownEvent = null;
                }

                if (null != this.controllerQueue)
                {
                    this.controllerQueue.Dispose();
                    this.controllerQueue = null;
                }

                if (null != this.monitorQueue)
                {
                    this.monitorQueue.Dispose();
                    this.monitorQueue = null;
                }

                if (null != this.cancellationTokenSource)
                {
                    this.cancellationTokenSource.Dispose();
                    this.cancellationTokenSource = null;
                }

                if (null != this.controllerResetEvent)
                {
                    this.controllerResetEvent.Dispose();
                    this.controllerResetEvent = null;
                }
            }
        }

        private void SignalQueueAdders(
            bool alwaysSignal,
            bool removeControllerAdder,
            bool removeMonitorAdder)
        {
            if (removeControllerAdder)
            {
                if (alwaysSignal ||
                    0 == Interlocked.CompareExchange(
                        ref this.hasSignaledControllerAdders, 1, 0))
                {
                    if (this.controllerAddersCountdownEvent.Signal())
                    {
                        this.controllerQueue.CompleteAdding();
                    }
                }
            }

            if (removeMonitorAdder)
            {
                if (alwaysSignal ||
                    0 == Interlocked.CompareExchange(
                        ref this.hasSignaledMonitorAdders, 1, 0))
                {
                    if (this.monitorAddersCountdownEvent.Signal())
                    {
                        this.monitorQueue.CompleteAdding();
                    }
                }
            }
        }

        private void AddToQueue(
            ITransferController item,
            CancellationToken cancellationToken,
            bool addController)
        {
            if (item.CanAddController)
            {
                this.controllerAddersCountdownEvent.AddCount();
            }

            if (item.CanAddMonitor)
            {
                this.monitorAddersCountdownEvent.AddCount();
            }

            if (addController)
            {
                this.controllerQueue.Add(item, cancellationToken);
            }
            else
            {
                this.monitorQueue.Add(item, cancellationToken);
            }
        }

        private void StartControllerThread()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
#if DEBUG
                // For debugging purposed change the name of the Thread to be
                // easily recognizable.
                Thread.CurrentThread.Name = "BlobTransferManager - Controller Thread";
#endif
                this.ControllerThread();
            });
        }

        private void ControllerThread()
        {
            SpinWait sw = new SpinWait();

            while (!this.cancellationTokenSource.Token.IsCancellationRequested &&
                (!this.controllerQueue.IsCompleted ||
                    !this.monitorQueue.IsCompleted ||
                    this.activeControllerItems.Any() ||
                    this.activeMonitorItems.Any() ||
                    this.activeTasks > 0))
            {
                FillInQueue(
                    this.activeControllerItems,
                    this.controllerQueue,
                    this.internalControllerQueue,
                    this.cancellationTokenSource.Token,
                    this.transferOptions.Concurrency,
                    sw);

                FillInQueue(
                    this.activeMonitorItems,
                    this.monitorQueue,
                    this.internalMonitorQueue,
                    this.cancellationTokenSource.Token,
                    this.transferOptions.Concurrency,
                    sw);

                if (!this.cancellationTokenSource.Token.IsCancellationRequested)
                {
                    // If we don't have the requested amount of active tasks
                    // running, get a task item from any active transfer item \
                    // that has work available.
                    if (this.activeTasks >= this.transferOptions.Concurrency ||
                        (!this.DoWorkFrom(this.activeControllerItems, sw) &&
                            !this.DoWorkFrom(this.activeMonitorItems, sw)))
                    {
                        sw.SpinOnce();
                    }
                }
            }

            if (this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                foreach (KeyValuePair<ITransferController, object> transferController in this.activeControllerItems)
                {
                    transferController.Key.CancelWork();
                }

                foreach (KeyValuePair<ITransferController, object> transferMonitor in this.activeMonitorItems)
                {
                    transferMonitor.Key.CancelWork();
                }
            }

            // there might be running "work" when the transfer is cancelled.
            // wait until all running "work" is done.
            sw.Reset();
            while (this.activeTasks != 0)
            {
                sw.SpinOnce();
            }

            // transfer is completed or canceled. If it is canceled, close all
            // active ITransferController items.
            foreach (ITransferController controller in
                this.activeControllerItems.Keys.Union(
                    this.activeMonitorItems.Keys))
            {
                IDisposable disposableController = controller as IDisposable;
                if (null != disposableController)
                {
                    disposableController.Dispose();
                }
            }

            this.controllerResetEvent.Set();
        }

        private void FinishedWorkItem(
            ITransferController transferController,
            bool finished)
        {
            if (finished)
            {
                object dummy;

                if (!this.activeControllerItems.TryRemove(transferController, out dummy))
                {
                    this.activeMonitorItems.TryRemove(transferController, out dummy);
                }

                if (transferController.CanAddController ||
                    transferController.CanAddMonitor)
                {
                    this.SignalQueueAdders(
                        true,
                        transferController.CanAddController,
                        transferController.CanAddMonitor);
                }

                // Our transfer controller can be disposable; check if it is 
                // and if so make sure to dispose it here.
                IDisposable disposableController = transferController as IDisposable;
                if (null != disposableController)
                {
                    disposableController.Dispose();
                }
            }

            Interlocked.Decrement(ref this.activeTasks);
        }

        private bool DoWorkFrom(
            ConcurrentDictionary<ITransferController, object> activeItems,
            SpinWait sw)
        {
            // Filter items with work only.
            List<KeyValuePair<ITransferController, object>> activeItemsWithWork =
                new List<KeyValuePair<ITransferController, object>>(
                    activeItems.Where(item => item.Key.HasWork && !item.Key.IsFinished));

            bool didWork = false;

            if (0 != activeItemsWithWork.Count)
            {
                sw.Reset();

                // Select random item and get work delegate.
                int idx = this.randomGenerator.Next(activeItemsWithWork.Count);
                Action<Action<ITransferController, bool>> work =
                    activeItemsWithWork[idx].Key.GetWork();

                if (null != work)
                {
                    didWork = true;

                    Interlocked.Increment(ref this.activeTasks);

                    // Start work delegate. Delegate is expected to start
                    // an async operation internally. 
                    try
                    {
                        work(this.FinishedWorkItem);
                    }
                    catch (Exception ex)
                    {
                        string strErrorMessage = string.Format("work() should never throw an exception: {0}", ex.StackTrace);
                        Debug.Fail(strErrorMessage);

                        this.FinishedWorkItem(
                            activeItemsWithWork[idx].Key,
                            true);
                    }
                }
            }

            return didWork;
        }
    }
}