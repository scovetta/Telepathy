//------------------------------------------------------------------------------
// <copyright file="BlobTransferRecursiveTransferItem.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      BlobTransferItem class to recursive transfer file to/from azure storage.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferControllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Blob.Protocol;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers;
    using Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers;

    /// <summary>
    /// BlobTransferItem class to recursive transfer file to/from azure storage.
    /// </summary>
    internal class BlobTransferRecursiveTransferItem :
        TransferControllerBase
    {
        /// <summary>
        /// Keeps track of the internal state-machine state.
        /// </summary>
        private volatile State state;
        
        private ILocation sourceLocation;
        private ILocation destinationLocation;
        private BlobTransferRecursiveTransferOptions options;

        /// <summary>
        /// Callback delegate called when transfer starts.
        /// </summary>
        private Action<object> startCallback;

        /// <summary>
        /// Finish callback delegate called when transfer finished.
        /// </summary>
        private Action<object, Exception> finishCallback;

        /// <summary>
        /// Lock to protect finishCallback.
        /// </summary>
        private object finishCallbackLock = new object();

        /// <summary>
        /// Gets a start callback delegate called when a sub file transfer starts.
        /// </summary>
        private Func<EntryData, Action<object>> getStartFileCallback;

        /// <summary>
        /// Gets a progress callback delegate called by file transfer status tracker.
        /// </summary>
        private Func<EntryData, Action<object, double, double>> getProgressFileCallback;

        /// <summary>
        /// Gets a finish callback delegate called when sub file transfer finished.
        /// </summary>
        private Func<EntryData, BlobTransferFileTransferEntry, Action<object, Exception>> getFinishFileCallback;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferRecursiveTransferItem" /> class.
        /// </summary>
        /// <param name="manager">Manager object which creates this object.</param>
        /// <param name="sourceLocation">Source location to copy files from.</param>
        /// <param name="destinationLocation">Destination location to copy files to.</param>
        /// <param name="options">Options object.</param>
        /// <param name="startCallback">Start transfer callback method.</param>
        /// <param name="finishCallback">Finish transfer callback method.</param>
        /// <param name="startFileCallback">Indidual file start transfer callback.</param>
        /// <param name="progressFileCallback">Individual file progress transfer callback.</param>
        /// <param name="finishFileCallback">Individual file finish transfer callback.</param>
        /// <param name="userData">Opaque user data to pass to events.</param>
        internal BlobTransferRecursiveTransferItem(
            BlobTransferManager manager,
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
            if (manager == null || options == null)
            {
                throw new ArgumentNullException(null == manager ? "manager" : "options");
            }

            this.Manager = manager;

            try
            {
                this.sourceLocation = Location.CreateLocation(
                    sourceLocation, 
                    options.SourceKey, 
                    options.SourceSAS,
                    this.Manager.TransferOptions,
                    true);
            }
            catch (Exception ex)
            {
                throw new BlobTransferException(
                    BlobTransferErrorCode.InvalidSourceLocation,
                    ex.Message,
                    ex);
            }

            try
            {
                this.destinationLocation = Location.CreateLocation(
                    destinationLocation, 
                    options.DestinationKey, 
                    options.DestinationSAS,
                    this.Manager.TransferOptions,
                    false);
            }
            catch (Exception ex)
            {
                throw new BlobTransferException(
                    BlobTransferErrorCode.InvalidDestinationLocation,
                    ex.Message,
                    ex);
            }

            // We support Local->Azure, Azure->Local and Azure->Azure transfers currently. Local->Local is unsupported.
            if ((this.sourceLocation is FileSystemLocation) &&
                (this.destinationLocation is FileSystemLocation))
            {
                throw new BlobTransferException(
                    BlobTransferErrorCode.LocalToLocalTransfersUnsupported,
                    Resources.LocalToLocalTransferUnsupportedException);
            }

            if (Location.Equals(this.sourceLocation, this.destinationLocation))
            {
                throw new BlobTransferException(
                    BlobTransferErrorCode.SameSourceAndDestination,
                    Resources.SourceAndDestinationLocationCannotBeEqualException);
            }

            this.options = options;
            this.startCallback = startCallback;
            this.finishCallback = finishCallback;

            this.getStartFileCallback = delegate(EntryData entryData)
            {
                return delegate(object data)
                {
                    if (null != startFileCallback)
                    {
                        startFileCallback(
                            this.UserData,
                            entryData);
                    }
                };
            };

            this.getProgressFileCallback = delegate(EntryData entryData)
            {
                return delegate(object data, double speed, double progress)
                {
                    if (null != progressFileCallback)
                    {
                        progressFileCallback(
                            this.UserData,
                            entryData,
                            speed,
                            progress);
                    }
                };
            };

            this.getFinishFileCallback = delegate(EntryData entryData, BlobTransferFileTransferEntry transferEntry)
            {
                return delegate(object data, Exception exception)
                {
                    if (null == exception && null != transferEntry)
                    {
                        transferEntry.Status = BlobTransferEntryStatus.Finished;
                    }

                    if (null != finishFileCallback)
                    {
                        finishFileCallback(
                            this.UserData,
                            entryData,
                            exception);
                    }
                };
            };

            this.state = State.EnumerateFiles;

            this.HasWork = true;
            this.UserData = userData;
        }

        /// <summary>
        /// Internal state values.
        /// </summary>
        private enum State
        {
            EnumerateFiles,
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
                return true;
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
                case State.EnumerateFiles:
                    return this.GetEnumerateFilesAction();
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

            this.FinishCallbackHandler(ex);

            bool finished = this.SetFinishedAndPostWork();

            callbackState.CallFinish(this, finished);
        }

        private static void CreateTargetDirectory(string location)
        {
            string directoryName = Path.GetDirectoryName(location);

            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        private Action<Action<ITransferController, bool>> GetEnumerateFilesAction()
        {
            Debug.Assert(
                this.state == State.EnumerateFiles,
                "GetEnumerateFilesAction called, but state isn't EnumerateFiles");

            this.HasWork = false;

            return delegate(Action<ITransferController, bool> finishDelegate)
            {
                this.PreWork();
                Debug.Assert(null != finishDelegate, "Finish delegate expected");

                ThreadPool.QueueUserWorkItem(delegate
                {
                    this.PerformTransfer(
                        new CallbackState
                        {
                            FinishDelegate = finishDelegate,
                        });
                });
                return;
            };
        }
        
        private bool CheckFileAttributes(FileEntry entry)
        {
            string entrySource = (this.sourceLocation as FileSystemLocation).GetAbsolutePath(entry.RelativePath);
            FileAttributes entryAttrs = File.GetAttributes(entrySource);

            if (this.options.OnlyFilesWithArchiveBit)
            {
                // Check whether the file's attribute meets the /A option requirment
                if (FileAttributes.Archive != (entryAttrs & FileAttributes.Archive))
                {
                    return false;
                }
            }

            if (0 != this.options.IncludedAttributes)
            {
                // Check whether the file has any of attributes specified in /IA
                if (0 == (entryAttrs & this.options.IncludedAttributes))
                {
                    return false;
                }
            }

            if (0 != this.options.ExcludedAttributes)
            {
                // Check whether the file has any of attributes specified in /XA
                if (0 != (entryAttrs & this.options.ExcludedAttributes))
                {
                    return false;
                }
            }

            return true;
        }

        private void PerformTransfer(CallbackState callbackState)
        {
            try
            {
                bool sourceIsOnAzure = this.sourceLocation is AzureStorageLocation;
                bool destinationIsOnAzure = this.destinationLocation is AzureStorageLocation;
                bool getLastModifiedTime = this.options.ExcludeNewer || this.options.ExcludeOlder;

                FileEntryCache destinationLocationCache = new FileEntryCache(this.destinationLocation, getLastModifiedTime, this.Manager.CancellationTokenSource);

                getLastModifiedTime = getLastModifiedTime || (!sourceIsOnAzure && this.options.RestartableMode);

                // Quick quit if error occured.
                if (null != destinationLocationCache.Exception)
                {
                    // We will try to create folder or container later if not exist.
                    // If user provides a SAS to access a non-existent container, what 
                    // we received should be (403) Forbidden.
                    Exception ex = destinationLocationCache.Exception;
                    DirectoryNotFoundException directoryNotFoundEx = ex as DirectoryNotFoundException;

                    if (null == directoryNotFoundEx)
                    {
                        StorageException storageEx = ex as StorageException;

                        if (null == storageEx ||
                            null == storageEx.RequestInformation ||
                            null == storageEx.RequestInformation.ExtendedErrorInformation ||
                            BlobErrorCodeStrings.ContainerNotFound != storageEx.RequestInformation.ExtendedErrorInformation.ErrorCode)
                        {
                            throw new BlobTransferException(
                                BlobTransferErrorCode.InvalidDestinationLocation,
                                ex.Message,
                                ex);
                        }
                    }
                }

                // Transport snapshots only when user specified and this is downloading.
                bool ignoreSnapshot = !this.options.TransferSnapshots;

                // Check file attribute when source is file system and user specified to do filter.
                bool shallCheckAttribute = (this.sourceLocation is FileSystemLocation) && 
                    (this.options.OnlyFilesWithArchiveBit || 0 != this.options.IncludedAttributes || 0 != this.options.ExcludedAttributes);

                BlobTransferFileTransferEntries transferEntries = null;

                if (null != this.options.FileTransferStatus && this.options.FileTransferStatus.Initialized)
                {
                    transferEntries = this.options.FileTransferStatus.FileEntries;
                }
                else
                {
                    if (null != this.options.FileTransferStatus)
                    {
                        transferEntries = this.options.FileTransferStatus.FileEntries;
                    }
                    else
                    {
                        transferEntries = new BlobTransferFileTransferEntries();
                    }

                    IFileNameResolver nameResolver = FileNameResolver.GetFileNameResolver(
                                                        this.sourceLocation, 
                                                        this.destinationLocation,
                                                        this.options.Delimiter);

                    FileNameSnapshotAppender uniqueFileNameResolver = new FileNameSnapshotAppender();

                    // If source file is in local, get local file's modified time to determine whether it has been change after the last transferring
                    foreach (FileEntry entry in this.sourceLocation.EnumerateLocation(
                        this.options.FilePatterns, 
                        this.options.Recursive, 
                        getLastModifiedTime,
                        this.Manager.CancellationTokenSource))
                    {
                        this.CancellationHandler.CheckCancellation();

                        if (entry is ErrorFileEntry)
                        {
                            ErrorFileEntry errorEntry = entry as ErrorFileEntry;

                            throw new BlobTransferException(
                                BlobTransferErrorCode.InvalidSourceLocation,
                                errorEntry.Exception.Message,
                                errorEntry.Exception);
                        }

                        if (entry.SnapshotTime.HasValue && ignoreSnapshot)
                        {
                            continue;
                        }

                        if (shallCheckAttribute)
                        {
                            if (!this.CheckFileAttributes(entry))
                            {
                                continue;
                            }
                        }

                        string destinationRelativePath = nameResolver.ResolveFileName(entry);

                        FileEntry destEntry = destinationLocationCache.GetFileEntry(destinationRelativePath);

                        if (null != destEntry)
                        {
                            if (this.options.ExcludeNewer && entry.LastModified >= destEntry.LastModified)
                            {
                                continue;
                            }

                            if (this.options.ExcludeOlder && entry.LastModified <= destEntry.LastModified)
                            {
                                continue;
                            }
                        }

                        string uniqueFileName = uniqueFileNameResolver.ResolveFileName(entry);

                        AzureFileEntry azureFileEntry = entry as AzureFileEntry;

                        transferEntries[uniqueFileName] = new BlobTransferFileTransferEntry(
                            entry.RelativePath,
                            destinationRelativePath,
                            getLastModifiedTime ? entry.LastModified : null,
                            entry.SnapshotTime,
                            null == azureFileEntry ? BlobType.Unspecified : azureFileEntry.Blob.Properties.BlobType);
                    }

                    if (null != this.options.FileTransferStatus)
                    {
                        this.options.FileTransferStatus.Initialized = true;
                    }
                }

                if (sourceIsOnAzure && this.options.MoveFile)
                {
                    // Build up deletion blob sets to support MoveFile feature.
                    AzureStorageLocation azureSource = this.sourceLocation as AzureStorageLocation;

                    HashSet<string> blobAppeared = new HashSet<string>();
                    Dictionary<string, KeyValuePair<BlobType, int>> blobWithSnapshots =
                        new Dictionary<string, KeyValuePair<BlobType, int>>();

                    foreach (BlobTransferFileTransferEntry transferEntry in
                        transferEntries.Values.Where(x => !x.EntryTransferFinished))
                    {
                        this.CancellationHandler.CheckCancellation();

                        string fileName = transferEntry.SourceRelativePath;

                        bool appearOnce = blobAppeared.Contains(fileName);

                        if (transferEntry.SourceSnapshotTime.HasValue || appearOnce)
                        {
                            KeyValuePair<BlobType, int> blobCount;
                            if (blobWithSnapshots.TryGetValue(fileName, out blobCount))
                            {
                                blobWithSnapshots[fileName] =
                                    new KeyValuePair<BlobType, int>(
                                        blobCount.Key,
                                        blobCount.Value + 1);
                            }
                            else
                            {
                                blobWithSnapshots.Add(
                                    fileName,
                                    new KeyValuePair<BlobType, int>(
                                        transferEntry.SourceBlobType,
                                        appearOnce ? 2 : 1));
                            }
                        }

                        if (!appearOnce)
                        {
                            blobAppeared.Add(fileName);
                        }
                    }

                    Dictionary<string, BlobTransferFileTransferEntry.DeletionBlobSet> blobDeletionBlobSets =
                        new Dictionary<string, BlobTransferFileTransferEntry.DeletionBlobSet>();

                    foreach (KeyValuePair<string, KeyValuePair<BlobType, int>> kvp in blobWithSnapshots)
                    {
                        this.CancellationHandler.CheckCancellation();

                        blobDeletionBlobSets.Add(
                            kvp.Key,
                            new BlobTransferFileTransferEntry.DeletionBlobSet(
                                azureSource.GetBlobObject(
                                    kvp.Key,
                                    kvp.Value.Key),
                                kvp.Value.Value));
                    }

                    foreach (BlobTransferFileTransferEntry transferEntry in
                        transferEntries.Values.Where(x => !x.EntryTransferFinished && blobWithSnapshots.ContainsKey(x.SourceRelativePath)))
                    {
                        this.CancellationHandler.CheckCancellation();

                        transferEntry.BlobSet = blobDeletionBlobSets[transferEntry.SourceRelativePath];
                    }
                }

                this.StartCallbackHandler();

                bool destinationCreatedIfNotExist = false;

                foreach (KeyValuePair<string, BlobTransferFileTransferEntry> kvp in transferEntries.Where(x => !x.Value.EntryTransferFinished))
                {
                    this.CancellationHandler.CheckCancellation();

                    BlobTransferFileTransferEntry transferEntry = kvp.Value;

                    string uniqueFileName = kvp.Key;
                    string destinationRelativePath = transferEntry.DestinationRelativePath;

                    // Initialize data entry.
                    EntryData entryData = new EntryData
                    {
                        FileName = uniqueFileName,
                        TransferEntry = kvp.Value
                    };

                    if (sourceIsOnAzure)
                    {
                        entryData.SourceBlob = (this.sourceLocation as AzureStorageLocation).GetBlobObject(
                            transferEntry.SourceRelativePath,
                            transferEntry.SourceSnapshotTime,
                            transferEntry.SourceBlobType);
                    }

                    if (destinationIsOnAzure)
                    {
                        // For blob to blob copy, destination blob instance will be created before transferring.
                        entryData.DestinationBlob = sourceIsOnAzure ?
                            null :
                            (this.destinationLocation as AzureStorageLocation).GetBlobObject(destinationRelativePath, this.options.UploadBlobType);
                    }

                    if (this.options.FakeTransfer)
                    {
                        this.getStartFileCallback(entryData)(null);
                        continue;
                    }

                    if (!destinationIsOnAzure)
                    {
                        // SourceInOnAzure must be true since we have a check above.
                        ICloudBlob blobSource = entryData.SourceBlob;
                        string entryDestination = (this.destinationLocation as FileSystemLocation).GetAbsolutePath(destinationRelativePath);

                        // Callback before queueing the transfer. If necessary the caller can cancel the transfer for this particular entry.
                        if (!this.BeforeQueueCallbackHandler(blobSource.Uri.AbsoluteUri, entryDestination, entryData))
                        {
                            continue;
                        }

                        if (!destinationCreatedIfNotExist)
                        {
                            try
                            {
                                CreateTargetDirectory((this.destinationLocation as FileSystemLocation).FullPath);
                            }
                            catch (Exception ex)
                            {
                                throw new BlobTransferException(
                                    BlobTransferErrorCode.InvalidDestinationLocation,
                                    ex.Message,
                                    ex);
                            }

                            destinationCreatedIfNotExist = true;
                        }

                        try
                        {
                            CreateTargetDirectory(entryDestination);
                        }
                        catch (Exception ex)
                        {
                            this.OutputFailedFileEntry(entryData, ex);
                            continue;
                        }

                        this.Manager.QueueDownload(
                            transferEntry,
                            blobSource,
                            entryDestination,
                            null,
                            this.options.DownloadCheckMd5,
                            this.options.MoveFile,
                            this.options.KeepLastWriteTime,
                            this.getStartFileCallback(entryData),
                            this.getProgressFileCallback(entryData),
                            this.getFinishFileCallback(entryData, transferEntry),
                            null);
                    }
                    else
                    {
                        AzureStorageLocation azureDestination = this.destinationLocation as AzureStorageLocation;

                        Debug.Assert(null != azureDestination, "azureDestination must be AzureStorageLocation if we get here.");

                        if (azureDestination.ContainerName.Equals(BlobTransferConstants.DefaultContainerName))
                        {
                            // A blob in the $root container cannot include a forward slash (/) in its name.
                            if (-1 != destinationRelativePath.IndexOf('/'))
                            {
                                this.OutputFailedFileEntry(
                                    entryData,
                                    new BlobTransferException(
                                        BlobTransferErrorCode.InvalidDestinationLocation,
                                        Resources.SubfoldersNotAllowedUnderRootContainerException));

                                continue;
                            }
                        }

                        if (!destinationCreatedIfNotExist)
                        {
                            // If the credential to access the container is SAS, we cannot
                            // create a container. However, we are sure that the container
                            // must exists because we have checked the exception after
                            // building destinationLocationCache.
                            if (!azureDestination.StorageCredential.IsSAS)
                            {
                                try
                                {
                                    BlobTransferOptions transferOptions = this.Manager.TransferOptions;

                                    BlobRequestOptions requestOptions = transferOptions.GetBlobRequestOptions(BlobRequestOperation.CreateContainer);
                                    OperationContext operationContext = new OperationContext()
                                    {
                                        ClientRequestID = transferOptions.GetClientRequestId(),
                                    };

                                    azureDestination.BlobContainer.CreateIfNotExists(
                                        requestOptions,
                                        operationContext);
                                }
                                catch (StorageException ex)
                                {
                                    throw new BlobTransferException(
                                        BlobTransferErrorCode.InvalidDestinationLocation,
                                        ex.Message,
                                        ex);
                                }
                            }

                            destinationCreatedIfNotExist = true;
                        }

                        if (sourceIsOnAzure)
                        {
                            ICloudBlob blobSource = entryData.SourceBlob;

                            // Callback before queueing the transfer. If necessary the caller can cancel the transfer for this particular entry.
                            if (!this.BeforeQueueCallbackHandler(
                                    blobSource.Uri.AbsoluteUri,
                                    azureDestination.GetAbsoluteUri(destinationRelativePath),
                                    entryData))
                            {
                                continue;
                            }

                            this.Manager.QueueBlobCopy(
                                transferEntry,
                                null,
                                blobSource,
                                azureDestination.BlobContainer,
                                azureDestination.GetPathUnderContainer(destinationRelativePath),
                                this.options.MoveFile,
                                this.getStartFileCallback(entryData),
                                this.getProgressFileCallback(entryData),
                                this.getFinishFileCallback(entryData, transferEntry),
                                null);
                        }
                        else
                        {
                            string entrySource = (this.sourceLocation as FileSystemLocation).GetAbsolutePath(transferEntry.SourceRelativePath);
                            ICloudBlob blobDestination = entryData.DestinationBlob;

                            // Callback before queueing the transfer. If necessary the caller can cancel the transfer for this particular entry.
                            if (!this.BeforeQueueCallbackHandler(entrySource, blobDestination.Uri.AbsoluteUri, entryData))
                            {
                                continue;
                            }

                            try
                            {
                                this.Manager.QueueUpload(
                                    transferEntry,
                                    blobDestination,
                                    entrySource,
                                    null,
                                    this.options.MoveFile,
                                    this.getStartFileCallback(entryData),
                                    this.getProgressFileCallback(entryData),
                                    this.getFinishFileCallback(entryData, transferEntry),
                                    null);
                            }
                            catch (InvalidOperationException ex)
                            {
                                this.OutputFailedFileEntry(entryData, ex);
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.SetErrorState(e, callbackState);
                return;
            }

            this.HasWork = false;
            this.state = State.Finished;

            this.FinishCallbackHandler(null);

            bool finished = this.SetFinishedAndPostWork();

            callbackState.CallFinish(this, finished);
        }

        /// <summary>
        /// Output the status for entries supposed to fail.
        /// </summary>
        /// <param name="entryData">Entry to output.</param>
        /// <param name="ex">Exception to record.</param>
        private void OutputFailedFileEntry(EntryData entryData, Exception ex)
        {
            this.getStartFileCallback(entryData)(null);
            this.getFinishFileCallback(entryData, null)(null, ex);
        }

        private bool BeforeQueueCallbackHandler(string sourcePath, string destinationPath, EntryData entryData)
        {
            if (null != this.options.BeforeQueueCallback)
            {
                try
                {
                    return this.options.BeforeQueueCallback(sourcePath, destinationPath);
                }
                catch (Exception ex)
                {
                    this.OutputFailedFileEntry(
                        entryData,
                        new BlobTransferCallbackException(Resources.DataMovement_ExceptionFromCallback, ex));
                    return false;
                }
            }

            return true;
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

        private void FinishCallbackHandler(Exception ex)
        {
            if (null != this.finishCallback)
            {
                try
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
                catch (Exception e)
                {
                    // We have no way to report exception from finish call back, just ignore it here.
                    Debug.Fail(string.Format("An exception was thrown out from finishCallback: {0}", e.StackTrace));
                }
            }
        }
    }
}