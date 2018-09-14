//------------------------------------------------------------------------------
// <copyright file="FileShareAndBlobDataProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      A data provider implementation based on file share and azure blob
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Data.DataProvider
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Security.Principal;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using TraceHelper = Microsoft.Hpc.Scheduler.Session.Data.Internal.DataServiceTraceHelper;

    /// <summary>
    /// A data provider implementation based on file share and azure blob
    /// </summary>
    internal class FileShareAndBlobDataProvider : IDataProvider
    {
        /// <summary>
        /// Default number of DataMoveWorkers
        /// </summary>
        private const int DefaultDataMoveWorkerNumber = 2;

        /// <summary>
        /// Default max concurrent connections allowed by a ServicePoint object
        /// </summary>
        private const int ServicePointManagerConnectionLimit = 64;

        /// <summary>
        /// Blob based data container name format
        /// </summary>
        private const string BlobBasedDataContainerNameFormat = "{0}-{1}";

        /// <summary>
        /// Data move task queue
        /// </summary>
        private static DataMoveTaskQueue dataMoveTaskQueue = new DataMoveTaskQueue();

        /// <summary>
        /// List of all DataMoveWorkers
        /// </summary>
        private static List<DataMoveWorker> dataMoveWorkers = new List<DataMoveWorker>();

        /// <summary>
        /// Number of DataMoveWorkers
        /// </summary>
        private static int dataMoveWorkerNumber = DefaultDataMoveWorkerNumber;

        /// <summary>
        /// Timer to track data move tasks
        /// </summary>
        private static Timer trackDataMoveTaskTimer;

        /// <summary>
        /// A flag indicating whether it is tracking data move tasks
        /// </summary>
        private static int isTrackingDataMoveTasks;

        /// <summary>
        /// File share based data provider
        /// </summary>
        private IDataProvider fileShareDataProvider;

        /// <summary>
        /// Azure blob based data provider
        /// </summary>
        private IDataProvider blobDataProvider;

        /// <summary>
        /// The data location
        /// </summary>
        private DataLocation location = DataLocation.FileShareAndAzureBlob;

        /// <summary>
        /// Initializes static members of the FileShareAndBlobDataProvider class
        /// </summary>
        static FileShareAndBlobDataProvider()
        {
            ServicePointManager.DefaultConnectionLimit = ServicePointManagerConnectionLimit;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            for (int i = 0; i < dataMoveWorkerNumber; i++)
            {
                dataMoveWorkers.Add(new DataMoveWorker(dataMoveTaskQueue));
            }

            trackDataMoveTaskTimer = new Timer(
                TrackDataMoveTaskTimerCallback,
                null,
                Constant.LastUpdateTimeUpdateIntervalInMilliseconds,
                Constant.LastUpdateTimeUpdateIntervalInMilliseconds);
        }

        /// <summary>
        /// Initializes a new instance of the FileShareAndBlobDataProvider class 
        /// </summary>
        /// <param name="info">data server information</param>
        public FileShareAndBlobDataProvider(DataServerInfo info, DataLocation location)
        {
            this.fileShareDataProvider = new FileShareDataProvider(info);
            this.blobDataProvider = new BlobDataProvider();
            this.location = location;
        }

        /// <summary>
        /// Create a new data container
        /// </summary>
        /// <param name="name">data container name</param>
        /// <returns>information for accessing the data container</returns>
        public DataClientInfo CreateDataContainer(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareAndBlobDataProvider].CreateDataContainer: name={0}", name);

            DataClientInfo info = new DataClientInfo();
            DataClientInfo fileShareInfo = this.fileShareDataProvider.CreateDataContainer(name);
            string uniqueBlobId = Guid.NewGuid().ToString();
            DataClientInfo blobInfo = null;
            try
            {
                blobInfo = this.blobDataProvider.CreateDataContainer(string.Format(BlobBasedDataContainerNameFormat, name, uniqueBlobId));
                Dictionary<string, string> containerAttributes = new Dictionary<string, string>();
                containerAttributes.Add(Constant.DataAttributeBlobId, uniqueBlobId);
                containerAttributes.Add(Constant.DataAttributeBlobUrl, blobInfo.PrimaryDataPath);
                if (this.location == DataLocation.AzureBlob)
                {
                    // For blob primary container, the file share data is not ready yet, we won't return it.
                    TraceHelper.TraceEvent(TraceEventType.Warning, "[FileShareAndBlobDataProvider].CreateDataContainer: name={0} , blobPrimay = true ", name);
                    containerAttributes.Add(Constant.DataAttributeBlobPrimary, bool.TrueString);
                    info.PrimaryDataPath = blobInfo.SecondaryDataPath;
                }
                else
                {
                    info.PrimaryDataPath = fileShareInfo.PrimaryDataPath;
                    info.SecondaryDataPath = blobInfo.SecondaryDataPath;
                }

                this.fileShareDataProvider.SetDataContainerAttributes(name, containerAttributes);
            }
            catch (Exception ex)
            {
                if (this.location == DataLocation.AzureBlob)
                {
                    // For a blob primary container, if the blob create failed, remove the file share container as well.
                    TraceHelper.TraceEvent(TraceEventType.Warning, "[FileShareAndBlobDataProvider].CreateDataContainer: name={0} failed. Exception={1} ", name, ex);
                    this.fileShareDataProvider.DeleteDataContainer(name);
                    throw;
                }
                else
                {
                    // Do not throw exception if failed to create BlobDataContainer. It returns DataClientInfo with SecondaryDataPath == null
                    TraceHelper.TraceEvent(TraceEventType.Warning, "[FileShareAndBlobDataProvider].CreateDataContainer: name={0} failed. Exception={1} ", name, ex);
                }
            }

            return info;
        }

        /// <summary>
        /// Open an existing data container
        /// </summary>
        /// <param name="name">name of the data container to be opened</param>
        /// <returns>information for accessing the data container</returns>
        public DataClientInfo OpenDataContainer(string name)
        {
            DataClientInfo info = this.OpenDataContainerInternal(name);
            if (this.location == DataLocation.AzureBlob)
            {
                // If the Data location is AzureBlob, we shall return the blob uri and SAS
                if (!BlobDataProvider.IsBlobDataContainerPath(info.PrimaryDataPath))
                {
                    info.PrimaryDataPath = info.SecondaryDataPath;
                }

                info.SecondaryDataPath = string.Empty;
            }

            return info;
        }
        /// <summary>
        /// Open an existing data container
        /// </summary>
        /// <param name="name">name of the data container to be opened</param>
        /// <returns>information for accessing the data container</returns>
        private DataClientInfo OpenDataContainerInternal(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareAndBlobDataProvider].OpenDataContainer: name={0}", name);
            DataClientInfo info = new DataClientInfo();

            bool blobDataPrimary = false;
            string strBlobPrimary = string.Empty;
            Dictionary<string, string> containerAttributes = this.GetDataContainerAttributes(name);
            if (containerAttributes.TryGetValue(Constant.DataAttributeBlobPrimary, out strBlobPrimary) &&
                    string.Equals(strBlobPrimary, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Verbose,
                    "[FileShareAndBlobDataProvider].OpenDataContainer: name={0} is marked as blob primary. ",
                    name);
                blobDataPrimary = true;
            }

            DataClientInfo blobInfo = null;
            DataClientInfo fileShareInfo = null;
            if (blobDataPrimary)
            {
                string uniqueBlobId;
                string blobUrl = string.Empty;
                bool blobIdExist = containerAttributes.TryGetValue(Constant.DataAttributeBlobId, out uniqueBlobId);
                bool urlExist = containerAttributes.TryGetValue(Constant.DataAttributeBlobUrl, out blobUrl);
                Debug.Assert(urlExist && blobIdExist, "blob id or blob url attribute not set");

                string blobName = string.Format(BlobBasedDataContainerNameFormat, name, uniqueBlobId);
                blobInfo = this.blobDataProvider.OpenDataContainer(blobName);
                if (!string.Equals(blobUrl, blobInfo.PrimaryDataPath, StringComparison.OrdinalIgnoreCase))
                {
                    // The storage account changed, the blob primary container is in wrong state
                    // TODO: shall we sync back from on-premise file share copy to blob of new storage account?
                    throw new DataException(DataErrorCode.DataInconsistent, string.Empty);
                }

                info.PrimaryDataPath = blobInfo.SecondaryDataPath;
                if (this.location == DataLocation.AzureBlob)
                {
                    return info;
                }

                string strWriteDone;
                if (!containerAttributes.TryGetValue(Constant.DataAttributeWriteDone, out strWriteDone) ||
                    !string.Equals(strWriteDone, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                {
                    // if the data container is not marked as "write done" yet, we won't return the file share container path.
                    TraceHelper.TraceEvent(
                        TraceEventType.Verbose,
                        "[FileShareAndBlobDataProvider].OpenDataContainer: name={0} is not marked as write done yet. skip data transfer.",
                        name);

                    info.SecondaryDataPath = string.Empty;
                    return info;
                }

                // Check if data already synced from Azure blob to file share
                string fileShareContainerPath = (this.fileShareDataProvider as FileShareDataProvider).GenerateContainerFilePath(name);
                string strSyncDone;
                if (containerAttributes.TryGetValue(Constant.DataAttributeSyncDone, out strSyncDone) &&
                    string.Equals(strSyncDone, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                {
                    // if the data container is marked as "sync done". blob is already in sync with data
                    // on file share. no need to move data
                    TraceHelper.TraceEvent(
                        TraceEventType.Verbose,
                        "[FileShareAndBlobDataProvider].OpenDataContainer: name={0} is marked as sync done. skip data transfer.",
                        name);

                    info.SecondaryDataPath = fileShareContainerPath;
                    return info;
                }

                // need move data
                StorageCredentials blobCredentials = (this.blobDataProvider as BlobDataProvider).BlobContainerCredentials;
                CloudBlockBlob srcBlob = new CloudBlockBlob(new Uri(blobInfo.PrimaryDataPath), blobCredentials);
                BlobDataProvider.GetBlobAttributes(srcBlob);
                if (IsBlobMarkedAsSynced(srcBlob))
                {
                    // a DataMoveTask with same source and dest alreay exists in the data move task queue. skip this one
                    TraceHelper.TraceEvent(
                        TraceEventType.Verbose,
                        "[FileShareAndBlobDataProvider].OpenDataContainer: name={0}, Data already synced from blob to file share.",
                        name);
                    Dictionary<string, string> newAttributes = new Dictionary<string, string>();
                    newAttributes.Add(Constant.DataAttributeSyncDone, bool.TrueString);
                    this.fileShareDataProvider.SetDataContainerAttributes(name, newAttributes);
                    info.SecondaryDataPath = fileShareContainerPath;
                    return info;
                }

                // Move data
                if (dataMoveTaskQueue.ContainsDataMoveTask(name, blobInfo.PrimaryDataPath, fileShareContainerPath))
                {
                    // a DataMoveTask with same source and dest alreay exists in the data move task queue. skip this one
                    TraceHelper.TraceEvent(
                        TraceEventType.Verbose,
                        "[FileShareAndBlobDataProvider].OpenDataContainer: name={0}, src blob={1}. Data move task with the same name already exists.",
                        name,
                        blobInfo.PrimaryDataPath);
                }
                else
                {
                    TraceHelper.TraceEvent(
                        TraceEventType.Verbose,
                        "[FileShareAndBlobDataProvider].OpenDataContainer: name={0}, move data from blob to file share. Src blob={1}.",
                        name,
                        blobInfo.PrimaryDataPath);
                    DataMoveTask task = new DataMoveTask(name, blobInfo.PrimaryDataPath, fileShareContainerPath, blobCredentials);
                    dataMoveTaskQueue.AddDataMoveTask(task);
                }

                info.SecondaryDataPath = string.Empty;
            }
            else
            {
                fileShareInfo = this.fileShareDataProvider.OpenDataContainer(name);
                info.PrimaryDataPath = fileShareInfo.PrimaryDataPath;

                // now begin to check if data on Azure blob is in sync with that on file share:
                bool createNewBlob = false;

                // fetch blob id and blob url from container attributes. if not exist, 
                // then data doesn't exist on Azure blob. Need create a new blob.
                string uniqueBlobId;
                string blobUrl = string.Empty;
                if (containerAttributes.TryGetValue(Constant.DataAttributeBlobId, out uniqueBlobId))
                {
                    bool urlExist = containerAttributes.TryGetValue(Constant.DataAttributeBlobUrl, out blobUrl);
                    Debug.Assert(urlExist, "blob url attribute not exist");
                }
                else
                {
                    uniqueBlobId = Guid.NewGuid().ToString();
                    createNewBlob = true;
                }

                // get blob url from BlobDataProvider. Check if it matches with that 
                // obtained from container attribute. if not, then storage account for
                // common data was changed.  Need create a new blob on the new storage account.
                string blobName = string.Format(BlobBasedDataContainerNameFormat, name, uniqueBlobId);
                blobInfo = this.blobDataProvider.OpenDataContainer(blobName);
                if (!createNewBlob && !string.Equals(blobUrl, blobInfo.PrimaryDataPath, StringComparison.OrdinalIgnoreCase))
                {
                    createNewBlob = true;
                }

                if (!createNewBlob)
                {
                    // if blob already exist, need if need to sync data from file share to Azure blob
                    string strSyncDone;
                    if (containerAttributes.TryGetValue(Constant.DataAttributeSyncDone, out strSyncDone) &&
                        string.Equals(strSyncDone, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                    {
                        // if the data container is marked as "sync done". blob is already in sync with data
                        // on file share. no need to move data
                        TraceHelper.TraceEvent(
                            TraceEventType.Verbose,
                            "[FileShareAndBlobDataProvider].OpenDataContainer: name={0} is marked as sync done. skip data transfer.",
                            name);

                        info.SecondaryDataPath = blobInfo.SecondaryDataPath;
                        return info;
                    }

                    string strWriteDone;
                    if (!containerAttributes.TryGetValue(Constant.DataAttributeWriteDone, out strWriteDone) ||
                        !string.Equals(strWriteDone, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                    {
                        // if the data container is not marked as "write done" yet, return "string.empty" as data path.
                        // At client side, a NullDataContainer will be created and user will read out no data from it.
                        TraceHelper.TraceEvent(
                            TraceEventType.Verbose,
                            "[FileShareAndBlobDataProvider].OpenDataContainer: name={0} is not marked as write done yet. skip data transfer.",
                            name);

                        info.SecondaryDataPath = string.Empty;
                        return info;
                    }
                }
                else
                {
                    Dictionary<string, string> newAttributes = new Dictionary<string, string>();
                    newAttributes.Add(Constant.DataAttributeBlobId, uniqueBlobId);
                    newAttributes.Add(Constant.DataAttributeBlobUrl, blobInfo.PrimaryDataPath);
                    this.fileShareDataProvider.SetDataContainerAttributes(name, newAttributes);
                }

                // move data only if the data container is already marked as "write done" but not "sync done"
                info.SecondaryDataPath = blobInfo.SecondaryDataPath;

                string sourceDataPath = info.PrimaryDataPath;
                string destDataPath = blobInfo.PrimaryDataPath;

                // before moving data:
                // a) check if a previous data move task is already in data move task queue
                if (dataMoveTaskQueue.ContainsDataMoveTask(name, sourceDataPath, destDataPath))
                {
                    // a DataMoveTask with same source and dest alreay exists in the data move task queue. skip this one
                    TraceHelper.TraceEvent(
                        TraceEventType.Verbose,
                        "[FileShareAndBlobDataProvider].OpenDataContainer: name={0}, dest blob={1}. Data move task with the same dest already exists.",
                        name,
                        destDataPath);

                    return info;
                }

                // b) check if dest blob is already marked as completed.
                bool needMoveData = true;
                StorageCredentials destBlobCredentials = (this.blobDataProvider as BlobDataProvider).BlobContainerCredentials;
                CloudBlockBlob destBlob = new CloudBlockBlob(new Uri(destDataPath), destBlobCredentials);
                try
                {
                    BlobDataProvider.GetBlobAttributes(destBlob);
                    if (IsBlobMarkedAsCompleted(destBlob))
                    {
                        Dictionary<string, string> newAttributes = new Dictionary<string, string>();
                        newAttributes.Add(Constant.DataAttributeSyncDone, bool.TrueString);
                        this.fileShareDataProvider.SetDataContainerAttributes(name, newAttributes);
                        needMoveData = false;
                    }
                    else
                    {
                        // cleanup the error code
                        destBlob.Metadata.Remove(Constant.MetadataKeyErrorCode);
                        destBlob.Metadata.Remove(Constant.MetadataKeyException);
                        BlobDataProvider.SetBlobAttributes(destBlob);
                    }
                }
                catch (DataException ex)
                {
                    if (ex.ErrorCode != DataErrorCode.DataClientNotFound)
                    {
                        throw;
                    }

                    try
                    {
                        // dest blob doesn't exist yet. create one
                        this.blobDataProvider.CreateDataContainer(blobName);
                    }
                    catch (DataException ex2)
                    {
                        // BlobDataProvider.CreateDataContainer may throw DataClientAlreadyExists
                        // because of concurrent open. ignore it.
                        if (ex2.ErrorCode != DataErrorCode.DataClientAlreadyExists)
                        {
                            throw;
                        }
                    }
                }

                if (needMoveData)
                {
                    // create a DataMoveTask which will help sync data from file share to Azure blob
                    DataMoveTask task = new DataMoveTask(name, sourceDataPath, destDataPath, destBlobCredentials);
                    dataMoveTaskQueue.AddDataMoveTask(task);
                }
            }

            return info;
        }

        /// <summary>
        /// Delete a data container
        /// </summary>
        /// <param name="name">name of the data container to be deleted</param>
        public void DeleteDataContainer(string name)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareAndBlobDataProvider].DeleteDataContainer: name={0}", name);

            // delete blob first, then delete file
            try
            {
                string uniqueBlobId;
                Dictionary<string, string> containerAttributes = this.fileShareDataProvider.GetDataContainerAttributes(name);
                if (containerAttributes != null && containerAttributes.TryGetValue(Constant.DataAttributeBlobId, out uniqueBlobId))
                {
                    dataMoveTaskQueue.CancelDataMoveTask(name);

                    this.blobDataProvider.DeleteDataContainer(string.Format(BlobBasedDataContainerNameFormat, name, uniqueBlobId));
                }
            }
            catch (DataException ex)
            {
                // for error code "DataServerForAzureBurstMisconfigured", ignore it and fall back to FileShareDataProvider
                if (ex.ErrorCode != DataErrorCode.DataServerForAzureBurstMisconfigured)
                {
                    throw;
                }
            }

            this.fileShareDataProvider.DeleteDataContainer(name);
        }

        /// <summary>
        /// Sets container attributes
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="attributes">attribute key and value pairs</param>
        /// <remarks>if attribute with the same key already exists, its value will be
        /// updated; otherwise, a new attribute is inserted. Valid characters for 
        /// attribute key and value are: 0~9, a~z</remarks>
        public void SetDataContainerAttributes(string name, Dictionary<string, string> attributes)
        {
            this.fileShareDataProvider.SetDataContainerAttributes(name, attributes);
        }

        /// <summary>
        /// Gets container attributes
        /// </summary>
        /// <param name="name"> data container name</param>
        /// <returns>data container attribute key and value pairs</returns>
        public Dictionary<string, string> GetDataContainerAttributes(string name)
        {
            return this.fileShareDataProvider.GetDataContainerAttributes(name);
        }

        /// <summary>
        /// List all data containers
        /// </summary>
        /// <returns>List of all data containers</returns>
        public IEnumerable<string> ListAllDataContainers()
        {
            return this.fileShareDataProvider.ListAllDataContainers();
        }

        /// <summary>
        /// Set data container permissions
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="userName">data container owner</param>
        /// <param name="allowedUsers">privileged users of the data container</param>
        public void SetDataContainerPermissions(string name, string userName, string[] allowedUsers)
        {
            this.fileShareDataProvider.SetDataContainerPermissions(name, userName, allowedUsers);
        }

        /// <summary>
        /// Check if a user has specified permissions to a data container
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="userIdentity">identity of the user to be checked</param>
        /// <param name="permissions">permissions to be checked</param>
        public void CheckDataContainerPermissions(string name, WindowsIdentity userIdentity, DataPermissions permissions)
        {
            this.fileShareDataProvider.CheckDataContainerPermissions(name, userIdentity, permissions);
        }

        /// <summary>
        /// Check if a blob is marked as completed
        /// </summary>
        /// <param name="blob">target blob to be checked</param>
        /// <returns>true if blob is marked as "completed" (error code=0), false otherwise</returns>
        private static bool IsBlobMarkedAsCompleted(CloudBlockBlob blob)
        {
            // Data blob has 3 attributes: ErrorCode, Exception, and LastUpdateTime.
            // Depends on blob status, some attributes may not been set.
            // 1. ErrorCode/Exception set, but ErrorCode != Success. Should sync data to blob
            // 2. ErrorCode/Exception set, errorCode = Success. No need to sync data.
            // 3. ErrorCode/Exception not set, LastUpdateTime not set. Blob is newly created and has no data yet. Should sync data to blob
            // 4. ErrorCode/Exception not set, LastUpdateTime set. Update to the blob is terminated unexpectedly.  Should sync data to blob.
            BlobDataProvider.GetBlobAttributes(blob);

            string strErrorCode;
            if (!blob.Metadata.TryGetValue(Constant.MetadataKeyErrorCode, out strErrorCode))
            {
                strErrorCode = string.Empty;
            }

            string strException;
            if (!blob.Metadata.TryGetValue(Constant.MetadataKeyException, out strException))
            {
                strException = string.Empty;
            }

            string strLastUpdateTime;
            if (!blob.Metadata.TryGetValue(Constant.MetadataKeyLastUpdateTime, out strLastUpdateTime))
            {
                strLastUpdateTime = string.Empty;
            }

            TraceHelper.TraceEvent(
                TraceEventType.Verbose,
                "[FileShareAndBlobDataProvider].IsBlobMarkedAsCompleted: blob={0} is marked with error code={1}, exception={2}. last update time={3}.",
                blob.Uri.AbsoluteUri,
                strErrorCode,
                strException,
                strLastUpdateTime);

            if (!string.IsNullOrEmpty(strErrorCode))
            {
                // if error code is set, check if it is "success"
                int errorCode;
                if (int.TryParse(strErrorCode, out errorCode) && errorCode == DataErrorCode.Success)
                {
                    // no need do transfer
                    TraceHelper.TraceEvent(
                        TraceEventType.Verbose,
                        "[FileShareAndBlobDataProvider].IsBlobMarkedAsCompleted: blob={0} is marked as completed.",
                        blob.Uri.AbsoluteUri);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a blob is marked as completed
        /// </summary>
        /// <param name="blob">target blob to be checked</param>
        /// <returns>true if blob is marked as "completed" (error code=0), false otherwise</returns>
        private static bool IsBlobMarkedAsSynced(CloudBlockBlob blob)
        {
            // Data blob has 3 attributes: ErrorCode, Exception, and LastUpdateTime.
            // Depends on blob status, some attributes may not been set.
            // 1. ErrorCode/Exception set, but ErrorCode != Success. Should sync data to blob
            // 2. ErrorCode/Exception set, errorCode = Success. No need to sync data.
            // 3. ErrorCode/Exception not set, LastUpdateTime not set. Blob is newly created and has no data yet. Should sync data to blob
            // 4. ErrorCode/Exception not set, LastUpdateTime set. Update to the blob is terminated unexpectedly.  Should sync data to blob.
            BlobDataProvider.GetBlobAttributes(blob);

            string strErrorCode;
            if (!blob.Metadata.TryGetValue(Constant.MetadataKeyErrorCode, out strErrorCode))
            {
                strErrorCode = string.Empty;
            }

            string strSynced;
            if (!blob.Metadata.TryGetValue(Constant.MetadataKeySynced, out strSynced))
            {
                strSynced = string.Empty;
            }

            string strException;
            if (!blob.Metadata.TryGetValue(Constant.MetadataKeyException, out strException))
            {
                strException = string.Empty;
            }

            TraceHelper.TraceEvent(
                TraceEventType.Verbose,
                "[FileShareAndBlobDataProvider].IsBlobMarkedAsSynced: blob={0}, synced={1}, error code={2}, exception={3}.",
                blob.Uri.AbsoluteUri,
                strSynced,
                strErrorCode,
                strException);

            if (!string.IsNullOrEmpty(strSynced) && string.Equals(strSynced, bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Timer callback to track data move tasks
        /// </summary>
        /// <param name="state">timer callback state</param>
        private static void TrackDataMoveTaskTimerCallback(object state)
        {
            if (1 == Interlocked.CompareExchange(ref isTrackingDataMoveTasks, 1, 0))
            {
                return;
            }

            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareAndBlobDataProvider].TrackDataMoveTaskCallback: enter.");

            List<DataMoveTask> tasks = dataMoveTaskQueue.GetAllDataMoveTasks();
            foreach (DataMoveTask task in tasks)
            {
                task.Keepalive();
            }

            TraceHelper.TraceEvent(TraceEventType.Verbose, "[FileShareAndBlobDataProvider].TrackDataMoveTaskCallback: exit.");
            isTrackingDataMoveTasks = 0;
        }
    }
}
