//------------------------------------------------------------------------------
// <copyright file="DataManagement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data management component
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using Microsoft.Hpc.Scheduler;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Data.DataProvider;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using TraceHelper = DataServiceTraceHelper;

    /// <summary>
    /// Data management class which is responsible for auto data cleanup.
    /// </summary>
    internal class DataManagement : IDisposable
    {
        /// <summary>
        /// Cluster head node name is "localhost" since data service runs on head node
        /// </summary>
        private const string HeadNode = "localhost";

        /// <summary>
        /// Directory name for soa common data server
        /// </summary>
        private const string SoaDataServerDirectory = "SOA";

        /// <summary>
        /// Directory name for root diretory user job data
        /// </summary>
        private const string SoaDataUserJobDirectory = "UserJobs";

        /// <summary>
        /// Wait period before next InitializeBlobDataProvider retry: 5 seconds
        /// </summary>
        private const int InitializeBlobDataProviderWaitPeriodInMilliseconds = 5000;

        /// <summary>
        /// Auto data cleanup frequency: 30 minutes a time
        /// </summary>
        private static TimeSpan autoDataCleanupPeriod = TimeSpan.FromMinutes(30);

        /// <summary>
        /// The scheduler
        /// </summary>
        private IScheduler scheduler;

        /// <summary>
        /// Data server information
        /// </summary>
        private DataServerInfo dataServerInfo;

        /// <summary>
        /// Timer for triggering automatic data cleanup periodically
        /// </summary>
        private Timer autoDataCleanupTimerProc;

        /// <summary>
        /// Auto data cleanup worker thread
        /// </summary>
        private Thread cleanupThread;

        /// <summary>
        /// Queue for managing expired (to-be-cleaned) DataLifeCycleInternal objects
        /// </summary>
        private Queue<DataLifeCycleInternal> cleanupItemQueue = new Queue<DataLifeCycleInternal>();

        /// <summary>
        /// Lock object for 'cleanupItemQueue'
        /// </summary>
        private object lockCleanupItemQueue = new object();

        /// <summary>
        /// Semaphore for syncing cleanup worker thread
        /// </summary>
        private Semaphore semCleanup = new Semaphore(0, int.MaxValue);

        /// <summary>
        /// Data lifecycle store
        /// </summary>
        private DataLifeCycleStore lifeCycleStore = new DataLifeCycleStore();
        
        /// <summary>
        /// Whether Dispose has been called.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// The cluster info
        /// </summary>
        private ClusterInfo clusterInfo;

        /// <summary>
        /// Initializes a new instance of the DataManagement class
        /// </summary>
        /// <param name="scheduler">the scheduler instance</param>
        public DataManagement(ClusterInfo clusterInfo, IScheduler scheduler)
        {
            this.clusterInfo = clusterInfo;
            this.scheduler = scheduler;
            this.autoDataCleanupTimerProc = new Timer(this.AutoDataCleanupThreadProc);

            this.LoadDataServerInfo();

            this.cleanupThread = new Thread(new ThreadStart(this.DataCleanupWorkerThread));
            this.cleanupThread.IsBackground = true;
            this.cleanupThread.Start();

            HpcSchedulerDelegation.OnJobFinished += this.OnSessionFinished;
            HpcSchedulerDelegation.OnJobFailedOrCanceled += this.OnSessionFailedOrCanceled;
        }

        /// <summary>
        /// Gets data server information
        /// </summary>
        internal DataServerInfo DataServer
        {
            get { return this.dataServerInfo; }
        }

        /// <summary>
        /// Gets the root directory of soa user job data
        /// </summary>
        public string UserJobDataRoot
        {
            get { return Path.Combine(this.DataServer.AddressInfo, SoaDataUserJobDirectory); }
        }

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region data server management

        /// <summary>
        /// Load data server information from cluster environment variable, and initialize data server.
        /// </summary>
        internal void LoadDataServerInfo()
        {
            // load file share data server info
            DataServerInfo dsInfo = GetDataServerInfo(this.scheduler);
            DataLifeCycleStore lifeCycleStore = new DataLifeCycleStore();
            if (dsInfo != null)
            {
                if (InitializeDataServer(dsInfo))
                {
                    // data server is initialized successfully, rebuild lifecycle management store
                    this.BuildDataLifeCycleStore(dsInfo, lifeCycleStore);
                }
            }
            else
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .LoadDataServerInfo: no data server is configured");
            }

            this.dataServerInfo = dsInfo;
            this.lifeCycleStore = lifeCycleStore;

            // reset blob storage info
            if (this.dataServerInfo != null)
            {
                InitializeBlobDataProvider();
            }
        }

        #region data lifecycle management
        /// <summary>
        /// Remove a data client from the dataclient lifecycle store
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        internal void RemoveDataClient(string dataClientId)
        {
            this.lifeCycleStore.RemoveDataClient(dataClientId);
        }

        /// <summary>
        /// Associate lifecycle of a DataClient with lifecycle of a session
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="sessionId">session id</param>
        internal void AssociateDataClientWithSession(string dataClientId, int sessionId)
        {
            this.lifeCycleStore.AssociateDataClientWithSession(dataClientId, sessionId);
        }

        #endregion

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="dispose">a flag indicating whether release resources</param>
        protected virtual void Dispose(bool dispose)
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;

                if (dispose)
                {
                    HpcSchedulerDelegation.OnJobFinished -= this.OnSessionFinished;
                    HpcSchedulerDelegation.OnJobFailedOrCanceled -= this.OnSessionFailedOrCanceled;

                    // notify auto cleanup thread to terminate
                    lock (this.lockCleanupItemQueue)
                    {
                        this.cleanupItemQueue.Clear();
                        this.cleanupItemQueue = null;
                        this.semCleanup.Release();
                        this.semCleanup.Close();
                    }

                    this.cleanupThread.Join();

                    if (this.autoDataCleanupTimerProc != null)
                    {
                        lock (this.autoDataCleanupTimerProc)
                        {
                            this.autoDataCleanupTimerProc.Dispose();
                        }
                    }

                    this.dataServerInfo = null;
                }
            }
        }

        /// <summary>
        /// Get data server info stored in specified cluster
        /// </summary>
        /// <param name="scheduler">the scheduler instance</param>
        /// <returns>data server info</returns>
        private static DataServerInfo GetDataServerInfo(IScheduler scheduler)
        {
            try
            {
                string strRuntimeSharePath = JobHelper.GetEnvironmentVariable(scheduler, Microsoft.Hpc.Scheduler.Session.Internal.Constant.RuntimeSharePathEnvVar);

                TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .GetDataServerInfo: HPC_RUNTIMESHARE = {0}", strRuntimeSharePath);

                if (string.IsNullOrEmpty(strRuntimeSharePath))
                {
                    var context = HpcContext.Get();
                    strRuntimeSharePath = context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.RuntimeDataSharePropertyName, context.CancellationToken).GetAwaiter().GetResult();
                    scheduler.SetEnvironmentVariable(Microsoft.Hpc.Scheduler.Session.Internal.Constant.RuntimeSharePathEnvVar, strRuntimeSharePath);
                }

                if (string.IsNullOrEmpty(strRuntimeSharePath))
                {
                    return null;
                }

                return new DataServerInfo(Path.Combine(strRuntimeSharePath, SoaDataServerDirectory));
            }
            catch (SchedulerException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .GetDataServerInfo: Failed to get data server info from the scheduler store, Exception:{0}", e);
                throw;
            }
        }

        /// <summary>
        /// Prepare the data server for use
        /// </summary>
        /// <param name="dataServerInfo">data server info</param>
        /// <returns>true if data server is initialized successfully, false otherwise</returns>
        private static bool InitializeDataServer(DataServerInfo dataServerInfo)
        {
            try
            {
                DataProviderHelper.InitializeDataServer(dataServerInfo);
                TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .InitializeDataServer: initialize data server: {0} successfully.", dataServerInfo.AddressInfo);
                return true;
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .InitializeDataServer: Failed to initialize data server: {0}. Exception:{1}", dataServerInfo.AddressInfo, e);
                return false;
            }
        }

        /// <summary>
        /// Timer callback to initialize BlobDataProvider
        /// </summary>
        private void InitializeBlobDataProvider()
        {
            try
            {
                TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .InitializeBlobDataProvider");
                string clusterName;
                Guid clusterId;
                clusterName = this.clusterInfo.Contract.ClusterName;
                if (!Guid.TryParse(this.clusterInfo.Contract.ClusterId, out clusterId))
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .InitializeBlobDataProvider: this.clusterInfo.Contract.ClusterId is not valid {0}. Wait and retry", this.clusterInfo.Contract.ClusterId);
                    return;
                }
                BlobDataProvider.UniqueClusterId = clusterId;

                CloudStorageAccount account;
                string storageConnectionString = this.clusterInfo.Contract.AzureStorageConnectionString;
                if (!CloudStorageAccount.TryParse(storageConnectionString, out account))
                {
                    TraceHelper.TraceEvent(TraceEventType.Warning, "[DataManagement] .InitializeBlobDataProvider: failed to parse storage connection string {0}.", storageConnectionString);
                    return;
                }

                BlobDataProvider.SetStorageAccount(account);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .InitializeBlobDataProvider: exception: {0}", e);
            }
        }

        #endregion

        /// <summary>
        /// Automatic data cleanup procedure
        /// </summary>
        /// <param name="state">callback state</param>
        private void AutoDataCleanupThreadProc(object state)
        {
            if (this.isDisposed)
            {
                return;
            }

            lock (this.autoDataCleanupTimerProc)
            {
                try
                {
                    // clean up data whose lifecycle aligns with that of obsolete sessions

                    // step 1, get a list of all session data lifecycle on data server
                    List<DataLifeCycleInternal> allSessionLifeCycles = this.GetAllSessionLifeCycles();
                    List<string> allSessionJobDataDirs = this.ListAllSoaJobDataDir();
                    if (allSessionLifeCycles.Count == 0 && allSessionJobDataDirs.Count == 0)
                    {
                        return;
                    }

                    // step 2, update active sssion id list
                    List<int> activeSessionIdList;
                    if (!this.GetActiveSessionIdList(out activeSessionIdList))
                    {
                        // if failed to update active session id list, skip this round
                        return;
                    }

                    // step 3, find out expired session lifecycle and clean data for it
                    foreach (DataLifeCycleInternal dataLifeCycle in allSessionLifeCycles)
                    {
                        SessionBasedDataLifeCycleContext context = dataLifeCycle.Context as SessionBasedDataLifeCycleContext;
                        Debug.Assert(context != null, "expect SessionBasedDataLifeCycleContext");

                        if (activeSessionIdList.BinarySearch(context.SessionId) < 0)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .AutoDataCleanupThreadProc: invalid session id found: {0}.", context.SessionId);
                            this.AddCleanupItem(dataLifeCycle);
                        }
                    }

                    // Step 4, find the expired soa job data dir
                    foreach (string jobdir in allSessionJobDataDirs)
                    {
                        string dirName = jobdir.Substring(jobdir.LastIndexOf('\\') + 1);
                        int jobid = -1;
                        if (int.TryParse(dirName, out jobid) && (activeSessionIdList.BinarySearch(jobid) >= 0))
                        {
                            continue;
                        }

                        try
                        {
                            Directory.Delete(jobdir, true);
                        }
                        catch (Exception e)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .AutoDataCleanupThreadProc: Failed to remove directory {0}: {1}.", jobdir, e);
                        }
                    }
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .AutoDataCleanupThreadProc: receives exception {0}.", e);
                }
                finally
                {
                    if (!this.isDisposed)
                    {
                        // wait for another AutoDataCleanupPeriod before starting next round of auto cleanup
                        this.autoDataCleanupTimerProc.Change(autoDataCleanupPeriod, TimeSpan.Zero);
                    }
                }
            }
        }

        /// <summary>
        /// Worker thread that does data cleanup work
        /// </summary>
        private void DataCleanupWorkerThread()
        {
            while (true)
            {
                DataLifeCycleInternal lifeCycle = this.GetNextCleanupItem();
                if (lifeCycle == null)
                {
                    TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .DataCleanupWorkerThread: exit");
                    break;
                }

                try
                {
                    DataServerInfo dsInfo = this.dataServerInfo;
                    if (dsInfo == null)
                    {
                        if (lifeCycle.Type == DataLifeCycleType.Session)
                        {
                            SessionBasedDataLifeCycleContext context = lifeCycle.Context as SessionBasedDataLifeCycleContext;
                            TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .DataCleanupWorkerThread: skip cleanup for session {0} because no data server is configured", context.SessionId);
                        }
                        else
                        {
                            TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .DataCleanupWorkerThread: skip cleanup for lifecycle object {0} because no data server is configured", lifeCycle.Context.Type);
                        }

                        continue;
                    }

                    IDataProvider provider = DataProviderHelper.GetDataProvider(DataLocation.FileShareAndAzureBlob, dsInfo);
                    if (lifeCycle.Type == DataLifeCycleType.Session)
                    {
                        SessionBasedDataLifeCycleContext context = lifeCycle.Context as SessionBasedDataLifeCycleContext;
                        TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .DataCleanupWorkerThread: cleanup DataClients associated with session {0}", context.SessionId);

                        foreach (string dataClientId in this.lifeCycleStore.ListDataClientsAssociatedWithSession(context.SessionId))
                        {
                            provider.DeleteDataContainer(dataClientId);
                            this.RemoveDataClient(dataClientId);
                        }
                    }
                    else
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .DataCleanupWorkerThread: unknown lifecycle object {0}", lifeCycle.Context.Type);
                    }
                }
                catch (DataException e)
                {
                    if (lifeCycle.Type == DataLifeCycleType.Session)
                    {
                        SessionBasedDataLifeCycleContext context = lifeCycle.Context as SessionBasedDataLifeCycleContext;
                        TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .DataCleanupWorkerThread: cleaning up data for session {0} receives exception: {1}", context.SessionId, e);
                    }
                    else
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .DataCleanupWorkerThread: cleaning up data for receives exception: {0}", e);
                    }
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .DataCleanupWorkerThread: unexpected exception: {0}", e);
                }
            }
        }

        /// <summary>
        /// Add a data cleanup work item into CleanupItemQueue.
        /// </summary>
        /// <param name="dataLifeCycle">data cleanup work item</param>
        private void AddCleanupItem(DataLifeCycleInternal dataLifeCycle)
        {
            bool success = true;
            lock (this.lockCleanupItemQueue)
            {
                if (this.cleanupItemQueue != null)
                {
                    this.cleanupItemQueue.Enqueue(dataLifeCycle);
                }
                else
                {
                    success = false;
                }
            }

            if (success)
            {
                this.semCleanup.Release();
            }
        }

        /// <summary>
        /// Get a data cleanup work item from CleanupItemQueue.
        /// </summary>
        /// <returns>next data cleanup workitem</returns>
        private DataLifeCycleInternal GetNextCleanupItem()
        {
            this.semCleanup.WaitOne();

            lock (this.lockCleanupItemQueue)
            {
                if (this.cleanupItemQueue != null)
                {
                    return this.cleanupItemQueue.Dequeue();
                }
            }

            return null;
        }

        /// <summary>
        /// Build data lifecycle management store
        /// </summary>
        /// <param name="dsInfo">Data server info</param>
        /// <param name="store">Data lifecycle management store</param>
        private void BuildDataLifeCycleStore(DataServerInfo dsInfo, DataLifeCycleStore store)
        {
            // Rebuild data lifecycle store might take a long time if there are a big number of DataClients
            // exists on the data server.  So we create a seperate thread to do this.
            Thread t = new Thread(new ParameterizedThreadStart(this.BuildDataLifeCycleStore));
            t.IsBackground = true;
            t.Start(new object[] { dsInfo, store });
        }

        /// <summary>
        /// Build data lifecycle management store
        /// </summary>
        /// <param name="obj">parameter object</param>
        private void BuildDataLifeCycleStore(object obj)
        {
            object[] objs = (object[])obj;
            DataServerInfo dsInfo = objs[0] as DataServerInfo;
            DataLifeCycleStore store = objs[1] as DataLifeCycleStore;
            TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .BuildDataLifeCycleStore: begin");

            IDataProvider provider = DataProviderHelper.GetDataProvider(DataLocation.FileShareAndAzureBlob, dsInfo);
            List<string> allContainers = null;
            try
            {
                allContainers = new List<string>(provider.ListAllDataContainers());
            }
            catch (DataException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "DataManagement .BuildDataLifeCycleStore: list all data contaienrs receives exception {0}", e);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "DataManagement .BuildDataLifeCycleStore: list all data containers receives exception {0}", e);
            }

            if (allContainers == null)
            {
                return;
            }

            foreach (string containerName in allContainers)
            {
                try
                {
                    string strSessionId;
                    Dictionary<string, string> containerAttributes = provider.GetDataContainerAttributes(containerName);
                    if (containerAttributes.TryGetValue(Constant.DataAttributeSessionId, out strSessionId))
                    {
                        TraceHelper.TraceEvent(
                            TraceEventType.Verbose,
                            "DataManagement .BuildDataLifeCycleStore: DataClient {0} is associated with session {1}",
                            containerName,
                            strSessionId);
                        store.AssociateDataClientWithSession(containerName, int.Parse(strSessionId));
                    }
                }
                catch (DataException e)
                {
                    if (e.ErrorCode != DataErrorCode.DataClientNotFound)
                    {
                        TraceHelper.TraceEvent(
                            TraceEventType.Error,
                            "DataManagement .BuildDataLifeCycleStore: check container {0} receives exception {1}",
                            containerName,
                            e);
                    }
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(
                        TraceEventType.Error,
                        "DataManagement .BuildDataLifeCycleStore: check container {0} receives exception {1}",
                        containerName,
                        e);
                }
            }

            TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .BuildDataLifeCycleStore: end");

            // data lifecycle management store is rebuilt, now start the first round of auto cleanup 
            this.autoDataCleanupTimerProc.Change(TimeSpan.Zero, TimeSpan.Zero);
        }

        /// <summary>
        /// Get a list of session ids that are associated with DataClient
        /// </summary>
        /// <returns>a list of session ids that are associated with DataClients</returns>
        private List<DataLifeCycleInternal> GetAllSessionLifeCycles()
        {
            List<DataLifeCycleInternal> allSessionLifeCycles = new List<DataLifeCycleInternal>();
            foreach (int sessionId in this.lifeCycleStore.ListSessionIds())
            {
                allSessionLifeCycles.Add(new DataLifeCycleInternal(sessionId));
            }

            return allSessionLifeCycles;
        }

        /// <summary>
        /// Get all active session ids
        /// </summary>
        /// <param name="activeSessionIdList">list of all active session ids</param>
        /// <returns>true if operation succeeds; false if it is failed due to some SchedulerException</returns>
        private bool GetActiveSessionIdList(out List<int> activeSessionIdList)
        {
            activeSessionIdList = new List<int>();
            try
            {
                IFilterCollection collection = new FilterCollection();
                collection.Add(new FilterProperty(FilterOperator.NotEqual, JobPropertyIds.ServiceName, string.Empty));
                collection.Add(new FilterProperty(FilterOperator.HasNoBitSet, JobPropertyIds.State, JobState.Finished));

                foreach (ISchedulerJob job in this.scheduler.GetJobList(collection, null))
                {
                    if (job.State != JobState.Canceled && job.State != JobState.Failed)
                    {
                        activeSessionIdList.Add(job.Id);
                    }
                    else
                    {
                        try
                        {
                            if (HpcSchedulerDelegation.IsDurableSessionJob(job))
                            {
                                // Failed/canceled job for durable session can be requeued, so keep its data for durable session
                                activeSessionIdList.Add(job.Id);
                            }
                        }
                        catch (SchedulerException e)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .GetActiveSessionIdList: checking if job is durable receives exception {0}", e);
                        }
                    }
                }

                activeSessionIdList.Sort();
                return true;
            }
            catch (SchedulerException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .GetActiveSessionIdList: receives exception {0}", e);
                return false;
            }
        }

        /// <summary>
        /// Method called when a session job is finished
        /// </summary>
        /// <param name="sender">the source of the event</param>
        /// <param name="args">event args that contains no event data</param>
        private void OnSessionFinished(object sender, EventArgs args)
        {
            ISchedulerJob schedulerJob = sender as ISchedulerJob;
            if (schedulerJob == null)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .OnSessionFinished: sender is not a scheduler job");
                return;
            }

            TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .OnSessionFinished: session job {0} is finished.  add it into data cleanup queue", schedulerJob.Id);
            this.AddCleanupItem(new DataLifeCycleInternal(schedulerJob.Id));
        }

        /// <summary>
        /// Method called when a session job is failed or canceled
        /// </summary>
        /// <param name="sender">the source of the event</param>
        /// <param name="args">event args that contains no event data</param>
        private void OnSessionFailedOrCanceled(object sender, EventArgs args)
        {
            ISchedulerJob schedulerJob = sender as ISchedulerJob;
            if (schedulerJob == null)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .OnSessionFailedOrCanceled: sender is not a scheduler job");
                return;
            }

            try
            {
                TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataManagement] .OnSessionFailedOrCanceled: session {0} is failed/canceled.", schedulerJob.Id);
                bool durable = HpcSchedulerDelegation.IsDurableSessionJob(schedulerJob);
                if (!durable)
                {
                    TraceHelper.TraceEvent(TraceEventType.Information, "[DataManagement] .OnSessionFailedOrCanceled: non-durable session {0} is failed/canceled.  add it into data cleanup queue", schedulerJob.Id);
                    this.AddCleanupItem(new DataLifeCycleInternal(schedulerJob.Id));
                }
            }
            catch (SchedulerException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataManagement] .OnSessionFailedOrCanceled: checking if session is durable receives exception {0}", e);
            }
        }

        private List<string> ListAllSoaJobDataDir()
        {
            List<string> list = new List<string>();
            if (Directory.Exists(this.UserJobDataRoot))
            {
                foreach (string userDir in Directory.GetDirectories(this.UserJobDataRoot))
                {
                    foreach (string jobDir in Directory.GetDirectories(userDir))
                    {
                        list.Add(jobDir);
                    }
                }
            }

            return list;
        }
    }
}
