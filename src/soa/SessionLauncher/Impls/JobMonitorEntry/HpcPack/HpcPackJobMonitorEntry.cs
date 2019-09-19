// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#if HPCPACK
namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.HpcPack
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.HpcPack.DataMapping;

    /// <summary>
    /// Entry class for job monitor
    /// </summary>
    internal class HpcPackJobMonitorEntry : IDisposable
    {
        /// <summary>
        /// Stores the retry limit
        /// </summary>
        private const int RetryLimit = 3;

        /// <summary>
        /// Stores the min time gap between two pull task operations
        /// </summary>
        private const int PullTaskMinGap = 1000;

        /// <summary>
        /// Stores the max time gap between two pull task operations (if required)
        /// </summary>
        private const int PullTaskMaxGap = 10000;

        /// <summary>
        /// Stores the pull task info period
        /// </summary>
        private static readonly TimeSpan PollTaskInfoPeriod = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Stores the timeout before the broker re-register itself
        /// </summary>
        private static readonly TimeSpan ReregisterTimeout = TimeSpan.FromMinutes(21);

        /// <summary>
        /// Stores the dispose flag
        /// </summary>
        private int disposeFlag;

        /// <summary>
        /// Stores the current pull task gap for two continuously request
        /// </summary>
        private int pullTaskGap = PullTaskMinGap;

        /// <summary>
        /// Stores the registered pull task flag
        /// </summary>
        private int registerdPullTask;

        /// <summary>
        /// The session id
        /// </summary>
        private readonly int sessionid;

        /// <summary>
        /// Stores the context
        /// </summary>
        private ISchedulerNotify context;

        /// <summary>
        /// Stores the previous state
        /// </summary>
        private volatile JobState previousState;

        /// <summary>
        /// Cluster scheduler
        /// </summary>
        private IScheduler scheduler;

        /// <summary>
        /// Stores the last change time
        /// </summary>
        private DateTime lastChangeTime;

        /// <summary>
        /// Stores the last response time
        /// </summary>
        private DateTime lastResponseTime;

        /// <summary>
        /// Stores the min units of resource
        /// </summary>
        private int minUnits;

        /// <summary>
        /// Stores the max units of resource
        /// </summary>
        private int maxUnits;

        /// <summary>
        /// Service job
        /// </summary>
        private volatile ISchedulerJob schedulerJob;

        /// <summary>
        /// Stores the previous counters
        /// </summary>
        private JobInfo previousJobInfo;

        /// <summary>
        /// A flag indicating if corresponding job is being requeued
        /// </summary>
        private int isRequeuingJob;

        /// <summary>
        /// Stores how many times corresponding job has been requeued because of broker request
        /// </summary>
        private int requeueCount;

        /// <summary>
        /// Stores the date time that Start() opertaion was last called
        /// </summary>
        private DateTime lastStartTime = DateTime.MinValue;

        /// <summary>
        /// Local cache for scheduler node
        /// Key: node name
        /// Value: ISchedulerNode.
        /// </summary>
        private static Dictionary<string, ISchedulerNode> NodeInfoCache = new Dictionary<string, ISchedulerNode>();

        /// <summary>
        /// Reader-writer lock that protects NodeNameInfoCache and NodeIdNameMap
        /// </summary>
        private static ReaderWriterLock LockNodeInfoCache = new ReaderWriterLock();

        /// <summary>
        /// Initializes a new instance of the JobMonitorEntry class
        /// </summary>
        /// <param name="sessionid">indicating the session id</param>
        /// <param name="store">indicating the store object</param>
        public HpcPackJobMonitorEntry(int sessionid, IScheduler scheduler)
        {
            this.sessionid = sessionid;
            this.scheduler = scheduler;
        }


        /// <summary>
        /// Finalizes an instance of the JobMonitorEntry class
        /// </summary>
        ~HpcPackJobMonitorEntry()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets the exit event
        /// </summary>
        public event EventHandler Exit;

        /// <summary>
        /// Gets the previous state
        /// </summary>
        public JobState PreviousState
        {
            get { return previousState; }
        }

        /// <summary>
        /// Gets the previous job info (counters)
        /// </summary>
        public JobInfo PreviousJobInfo
        {
            get { return this.previousJobInfo; }
        }

        /// <summary>
        /// Gets the session id
        /// </summary>
        public int SessionId
        {
            get { return sessionid; }
        }

        /// <summary>
        /// Gets the min units
        /// Calculated at startup
        /// </summary>
        public int MinUnits
        {
            get { return this.minUnits; }
        }

        /// <summary>
        /// Gets the max units
        /// Calculated at startup
        /// </summary>
        public int MaxUnits
        {
            get { return this.maxUnits; }
        }

        /// <summary>
        /// Gets/sets the number of times that corresponding job has been requeued by broker
        /// </summary>
        public int RequeueCount
        {
            get { return this.requeueCount; }
            set { this.requeueCount = value; }
        }

        /// <summary>
        /// Gets the service job
        /// </summary>
        internal ISchedulerJob SchedulerJob
        {
            get { return schedulerJob; }
        }

        /// <summary>
        /// Dispose the job monitor entry
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start the monitor
        /// </summary>
        public JobState Start(OperationContext context)
        {
            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] Start monitor.");

            this.lastStartTime = DateTime.UtcNow;
            this.lastResponseTime = DateTime.MinValue;
            this.ResetPreviousValues();
            this.context = context.GetCallbackChannel<ISchedulerNotify>();

            this.ResetSchedulerJob();

            if (this.schedulerJob.State == JobState.Canceled)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Session_ValidateJobFailed_JobCanceled, SR.SessionLauncher_ValidateJobFailed_JobCanceled, this.sessionid.ToString());
            }

            this.CalculateMinAndMax();

            ThreadPool.QueueUserWorkItem(this.CallbackToQueryTaskInfo, false);
            return this.schedulerJob.State;
        }

        /// <summary>
        /// Check if the corresponding broker is unavailable
        /// </summary>
        /// <returns>returns a value indicating if the broker is unavailable</returns>
        public bool CheckIfBrokerIsUnavailable()
        {
            // this.lastChangeTime recorded the last time when broker returned an ACK that
            // confirms it receives a task/job state change callback.
            // this.lastStartTime recorded the last time when broker called RegisterJob().
            // If a broker did not receive task/job state change in 15 min, it should re-register
            // itself by calling RegisterJob(). So if this.lastChangeTime > 21 min and
            // this.lastStartTime > 21 min, it means the broker is unavailable
            // We added the last response time to record the last time that the broker returned ACK.
            // It is because the last change time has another functionality to record the step to query tasks.
            TraceHelper.TraceInfo(this.sessionid, "Check if broker is unavailable, lastChangeTime = {0}, lastStartTime = {1}, lastResponseTime = {2}", this.lastChangeTime, this.lastStartTime, this.lastResponseTime);
            bool result = (DateTime.UtcNow.Subtract(this.lastResponseTime) > ReregisterTimeout
                && DateTime.UtcNow.Subtract(this.lastStartTime) > ReregisterTimeout);

            TraceHelper.TraceInfo(this.sessionid, "Check if broker is unavailable, result = {0}", result);

            return result;
        }

        /// <summary>
        /// Returns the number of tasks that failed for reasons other than preemption
        /// </summary>
        /// <param name="schedulerJob"></param>
        /// <returns></returns>
        private int GetNonPreemptedFailedTaskCount(IScheduler scheduler, ISchedulerJob schedulerJob)
        {
            int ret = 0;

            try
            {
                // Filter by failed tasks that failed due to preemption
                IFilterCollection fc = scheduler.CreateFilterCollection();
                fc.Add(FilterOperator.NotEqual, TaskPropertyIds.FailureReason, FailureReason.Preempted);
                fc.Add(FilterOperator.Equal, TaskPropertyIds.State, TaskState.Failed);

                // Only return the task Ids
                IPropertyIdCollection propIds = new PropertyIdCollection();
                propIds.AddPropertyId(TaskPropertyIds.TaskId);

                using (ISchedulerRowSet failedTasks = schedulerJob.OpenTaskRowSet(propIds, fc, null, true))
                {
                    ret = failedTasks.GetCount();
                }
            }

            catch (Exception ex)
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[JobMonitorEntry] Failed to get non-preempted failed task count : {0}", ex);
            }

            return ret;
        }

        /// <summary>
        /// Close the job monitor entry
        /// </summary>
        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Notify that the job will be requeued soon. 
        /// </summary>
        /// <returns>true if requeue job is allowed, false otherwise</returns>
        public bool PrepareForRequeueJob()
        {
            // set "isRequeuingJob" flag to indicate that it is in job requeue phase
            return 0 == Interlocked.CompareExchange(ref this.isRequeuingJob, 1, 0);
        }

        /// <summary>
        /// Reset previous values
        /// </summary>
        private void ResetPreviousValues()
        {
            this.previousJobInfo = new JobInfo(this.sessionid);
            this.lastChangeTime = SqlDateTime.MinValue.Value;
            this.previousState = JobState.Configuring;
        }

        /// <summary>
        /// Callback to query task info
        /// </summary>
        /// <param name="obj">null object</param>
        public void CallbackToQueryTaskInfo(object obj)
        {
            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Enters CallbackToQueryTaskInfo method, {0}", obj);

            bool reset = (bool)obj;
            if (reset)
            {
                try
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Refresh job properties.");
                    this.schedulerJob.Refresh();
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[JobMonitorEntry] Failed to refresh job properties: {0}", e);

                    // Try to reset the job instance
                    try
                    {
                        this.ResetSchedulerJob();
                    }
                    catch (Exception ex)
                    {
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[JobMonitorEntry] Failed to reset service job instance: {0}", ex);
                    }
                }

                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] ResetPreviousValues.");
                this.ResetPreviousValues();
            }

            // Query task info won't throw exceptions
            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Call QueryTaskInfo");
            this.QueryTaskInfo();
        }

        /// <summary>
        /// Reset scheduler job
        /// </summary>
        private void ResetSchedulerJob()
        {
            // Reopen scheduler job because job state might be wrong because of losting event
            ISchedulerJob newSchedulerJob = this.scheduler.OpenJob(sessionid) as ISchedulerJob;
            newSchedulerJob.OnJobState += this.SchedulerJob_OnJobState;
            newSchedulerJob.OnTaskState += this.SchedulerJob_OnTaskState;

            // Replace the scheduler job with the new one to avoid race condition issue, no need to lock here
            ISchedulerJob oldSchedulerJob = this.schedulerJob;
            this.schedulerJob = newSchedulerJob;

            // Clean up the old scheduler job
            if (oldSchedulerJob != null)
            {
                oldSchedulerJob.OnJobState -= this.SchedulerJob_OnJobState;
                oldSchedulerJob.OnTaskState -= this.SchedulerJob_OnTaskState;
            }
        }

        /// <summary>
        /// Query task info
        /// </summary>
        private void QueryTaskInfo()
        {
            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Enters QueryTaskInfo method.");
            if (Interlocked.Increment(ref this.registerdPullTask) != 1)
            {
                // register count doesn't change from 0 to 1 means somebody is pulling task, quit
                return;
            }

            bool shouldExit = false;
            List<TaskInfo> taskInfoList = null;
            this.pullTaskGap = PullTaskMinGap;

            while (true)
            {
                try
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Starting get job counters.");
                    ISchedulerJobCounters counters = this.schedulerJob.GetCounters();

                    JobInfo jobInfo = new JobInfo(this.sessionid, counters);
                    JobState state = this.schedulerJob.State;

                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] Starting query task info: JobState = {0}\nJobInfo: {1}", state, jobInfo);
                    if (state != this.previousState)
                    {
                        this.previousState = state;
                        if (this.context != null)
                        {
                            // Bug 7144: dispose JobMonitorEntry instance and unsubscribe events if job state changed to Canceled/Finished/Failed
                            shouldExit = (0 == isRequeuingJob) && (state == JobState.Canceled || state == JobState.Finished || state == JobState.Failed);

                            try
                            {
                                // ignore JobState change that happened during job requeue operation.
                                // Note: requeue job takes 3 steps: cancel job, configure job, submit job.  Job state transitions during job requeue, i.e.,
                                // (running) -> cancelling -> cancelled -> configuring -> submitted -> validating ->(queued), will all be ignored.
                                if (0 == this.isRequeuingJob)
                                {
                                    ISchedulerNotify proxy = this.context;
                                    proxy.JobStateChanged(JobStateConverter.FromHpcJobState(state)).ContinueWith(this.OnEndJobStateChanged);
                                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] Job state change event triggered, new state: {0}", state);
                                }
                            }
                            catch (CommunicationException e)
                            {
                                // Channel is aborted, set the context to null
                                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[JobMonitorEntry] Callback channel is aborted: {0}", e);
                                this.context = null;
                            }
                            catch (Exception e)
                            {
                                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[JobMonitorEntry] Failed to trigger job state change event: {0}", e);
                            }
                        }
                    }

                    if (this.context != null && (taskInfoList == null || !jobInfo.Equals(this.previousJobInfo)))
                    {
                        try
                        {
                            taskInfoList = this.GetTaskInfo();

                            if (taskInfoList != null)
                            {
                              ISchedulerNotify proxy = this.context;
                              proxy.TaskStateChanged(taskInfoList).ContinueWith(this.OnEndTaskStateChanged, jobInfo);
                              TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] Task state change event triggered.");
                            }
                        }
                        catch (CommunicationException e)
                        {
                            // Channel is aborted, set the context to null
                            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[JobMonitorEntry] Callback channel is aborted: {0}", e);
                            this.context = null;
                        }
                        catch (Exception e)
                        {
                            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[JobMonitorEntry] Failed to trigger task state change event: {0}", e);
                        }
                    }
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[JobMonitorEntry] Exception thrown when querying task info: {0}", e);
                }

                // pull task is not registered, quit
                if (Interlocked.Decrement(ref this.registerdPullTask) == 0)
                {
                    break;
                }

                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] Waiting {0} miliseconds and start another round of getting task info.", this.pullTaskGap);

                // Sleep and pull task again, clear the register pull task flag
                Thread.Sleep(this.pullTaskGap);
                if (this.pullTaskGap < PullTaskMaxGap)
                {
                    this.pullTaskGap *= 2;
                    if (this.pullTaskGap > PullTaskMaxGap)
                    {
                        this.pullTaskGap = PullTaskMaxGap;
                    }
                }

                this.registerdPullTask = 1;
            }

            if (shouldExit)
            {
                if (this.Exit != null)
                {
                    this.Exit(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Finish JobStateChanged operation
        /// </summary>
        /// <param name="result">The IAsyncResult instance</param>
        private void OnEndJobStateChanged(Task result)
        {
            if (result.Exception != null)
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[JobMonitorEntry] Exception thrown when finishing OnJobStateChanged operation: {0}", result.Exception);
            }
        }

        /// <summary>
        /// Finish TaskStateChanged operation
        /// </summary>
        /// <param name="result">The IAsyncResult instance</param>
        private void OnEndTaskStateChanged(Task result, object state)
        {
            if (result.Exception != null)
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[JobMonitorEntry] Exception thrown when finishing OnTaskStateChanged operation: {0}", result.Exception);
            }
            else
            {
                JobInfo jobInfo = (JobInfo)state;
                DateTime responseTime = DateTime.UtcNow;
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] changing the lastChangeTime to: {0}, previousJobInfo to: {1}", responseTime, jobInfo);
                this.lastResponseTime = responseTime;
                this.previousJobInfo = jobInfo;
            }
        }

        /// <summary>
        /// Calculate the min and max value for the service job
        /// </summary>
        private void CalculateMinAndMax()
        {
            PropertyId[] propIds;
            PropertyRow row;
            int userMax, userMin;
            switch (this.schedulerJob.UnitType)
            {
                case JobUnitType.Node:
                    userMax = this.schedulerJob.MaximumNumberOfNodes;
                    userMin = this.schedulerJob.MinimumNumberOfNodes;
                    propIds = new PropertyId[] { JobPropertyIds.ComputedMaxNodes, JobPropertyIds.ComputedMinNodes };
                    break;

                case JobUnitType.Socket:
                    userMax = this.schedulerJob.MaximumNumberOfSockets;
                    userMin = this.schedulerJob.MinimumNumberOfSockets;
                    propIds = new PropertyId[] { JobPropertyIds.ComputedMaxSockets, JobPropertyIds.ComputedMinSockets };
                    break;

                default:
                    userMax = this.schedulerJob.MaximumNumberOfCores;
                    userMin = this.schedulerJob.MinimumNumberOfCores;
                    propIds = new PropertyId[] { JobPropertyIds.ComputedMaxCores, JobPropertyIds.ComputedMinCores };
                    break;
            }

            IFilterCollection filter = new FilterCollection();
            filter.Add(FilterOperator.Equal, JobPropertyIds.Id, this.schedulerJob.Id);

            IPropertyIdCollection property = new PropertyIdCollection();
            foreach (PropertyId pid in propIds)
            {
                property.AddPropertyId(pid);
            }

            using (ISchedulerRowSet set = this.scheduler.OpenJobRowSet(property, filter, null))
            {
                PropertyRow[] rows = set.GetRows(0, set.GetCount() - 1).Rows;
                Debug.Assert(rows.Length > 0);
                row = rows[0];
            }

            string callerName = "[JobMonitorEntry.GetMinAndMax]";
            int computedMax = (int)JobHelper.GetStorePropertyValue(row.Props[0], propIds[0], 0, callerName);
            int computedMin = (int)JobHelper.GetStorePropertyValue(row.Props[1], propIds[1], 0, callerName);

            if (this.schedulerJob.CanShrink)
            {
                this.minUnits = this.schedulerJob.AutoCalculateMin ? computedMin : userMin;
            }
            else
            {
                this.minUnits = userMin;
            }

            if (this.schedulerJob.CanGrow)
            {
                this.maxUnits = this.schedulerJob.AutoCalculateMax ? computedMax : userMax;
            }
            else
            {
                this.maxUnits = userMax;
            }

            if (this.maxUnits < this.minUnits)
            {
                this.maxUnits = this.minUnits;
            }

            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] MaxUnits = {0}, MinUnits = {1}", this.maxUnits, this.minUnits);
        }

        /// <summary>
        /// Handler triggered when the job state changed
        /// </summary>
        private void SchedulerJob_OnJobState(object sender, JobStateEventArg e)
        {
            JobState state = e.NewState;
            if (previousState != state && this.context != null)
            {
                // reset isRequeuingJob flag if job state changed into JobState.Queued
                if (1 == this.isRequeuingJob)
                {
                    if (state == JobState.Queued)
                    {
                        Interlocked.Exchange(ref this.isRequeuingJob, 0);
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Job requeue completed");
                    }
                }

                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] Job state changed: {0} -> {1}", previousState, state);

                // Bug 10250: Job.State is not updated after triggered event, so need to refresh the job instance by setting the parameter to true
                ThreadPool.QueueUserWorkItem(this.CallbackToQueryTaskInfo, true /* ask to refresh the job instance */);
            }
        }

        /// <summary>
        /// Handler triggered when the task state changed
        /// </summary>
        private void SchedulerJob_OnTaskState(object sender, TaskStateEventArg e)
        {
            if (this.context != null)
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] Task {0}.{1} state changed: {2} -> {3}", e.TaskId.JobTaskId, e.TaskId.InstanceId, e.PreviousState, e.NewState);
                ThreadPool.QueueUserWorkItem(this.CallbackToQueryTaskInfo, false);
            }
        }

        /// <summary>
        /// Dispose the job monitor entry
        /// </summary>
        /// <param name="disposing">indicating the disposing flag</param>
        private void Dispose(bool disposing)
        {
            if (Interlocked.Increment(ref this.disposeFlag) == 1)
            {
                if (disposing)
                {
                    if (this.schedulerJob != null)
                    {
                        try
                        {
                            this.schedulerJob.OnJobState -= this.SchedulerJob_OnJobState;
                            this.schedulerJob.OnTaskState -= this.SchedulerJob_OnTaskState;
                        }
                        catch(Exception e)
                        {
                            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[JobMonitorEntry] Remove OnJobState/OnTaskState error: {0}", e);
                        }

                        this.schedulerJob = null;
                    }

                    this.scheduler = null;
                }
            }
        }

        /// <summary>
        /// Clear the static node cache
        /// </summary>
        public static void ClearNodeCache()
        {
            LockNodeInfoCache.AcquireWriterLock(Timeout.Infinite);
            try
            {
                foreach (var name in NodeInfoCache.Keys)
                {
                    // remove node info. step 1. deregister node state event
                    NodeInfoCache[name].OnNodeState -= Node_OnStateChange;
                }
                // remove node info. step 2. remove all node info from NodeInfoCache
                NodeInfoCache.Clear();
            }
            finally
            {
                LockNodeInfoCache.ReleaseLock();
            }
        }

        /// <summary>
        /// Gets the task info
        /// </summary>
        /// <returns>returns the task info as a dictionary, keyed by task id</returns>
        /// <remarks>
        /// This method returns a list of task info which ChangeTime property is in this rank: [this.lastChangeTime, DateTime.Now].
        /// This method does not change this.lastChangeTime to DateTime.Now after getting tasks because it may fail when sending those information to broker
        /// So changeTime is outputed and this.lastChangeTime should be modified to this time after suceeded sending back task info
        /// </remarks>
        private List<TaskInfo> GetTaskInfo()
        {
            DateTime changeTime = DateTime.UtcNow;
            try
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Query task info...");

                // Step 1: Query task allocation history to fetch node id and core id for tasks
                Dictionary<int, TaskAllocationHistoryItem> taskInfoDic = new Dictionary<int, TaskAllocationHistoryItem>();
                PropertyIdCollection allocationPropertyCollection = new PropertyIdCollection();
                allocationPropertyCollection.AddPropertyId(AllocationProperties.TaskId);
                allocationPropertyCollection.AddPropertyId(AllocationProperties.CoreId);
                allocationPropertyCollection.AddPropertyId(AllocationProperties.NodeName);
                using (ISchedulerRowEnumerator rows = this.schedulerJob.OpenTaskAllocationHistoryEnumerator(allocationPropertyCollection))
                {
                    foreach (PropertyRow row in rows)
                    {
                        // Note: Finished/Failed/Canceled task will also be enumerated here
                        // We are going to add them into the dic regaredless of the state
                        // because only running tasks will be queried in the following logic.
                        int objectId = (int)row[AllocationProperties.TaskId].Value;
                        TaskAllocationHistoryItem taskInfo;
                        if (taskInfoDic.TryGetValue(objectId, out taskInfo))
                        {
                            // For each task instance cache the assigned resource with the lowest coreId. This is needed when node or socket allocation is used
                            //   in order to generate the correct port to connect to the service host
                            int coreId = (int)row[AllocationProperties.CoreId].Value;
                            if (taskInfo.FirstCoreId > coreId)
                            {
                                taskInfo.FirstCoreId = coreId;
                            }

                            taskInfo.Capacity++;
                        }
                        else
                        {
                            taskInfo = new TaskAllocationHistoryItem();
                            taskInfo.Capacity = 1;
                            taskInfo.FirstCoreId = (int)row[AllocationProperties.CoreId].Value;
                            taskInfo.NodeName = (string)row[AllocationProperties.NodeName].Value;
                            taskInfoDic.Add(objectId, taskInfo);
                        }
                    }
                }

                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Query task info (got task allocation history).");

                // Step 2: Get task states from scheduler
                // Only task whose change time is between lastChangeTime and Now will be queried
                // Only task id and state are required, will get node name according to node id from allocation history got from step 1.
                IPropertyIdCollection collection = new PropertyIdCollection();
                collection.AddPropertyId(TaskPropertyIds.Id);
                collection.AddPropertyId(TaskPropertyIds.State);
                FilterCollection fc = new FilterCollection();
                fc.Add(FilterOperator.GreaterThan, TaskPropertyIds.ChangeTime, this.lastChangeTime);
                fc.Add(FilterOperator.LessThanOrEqual, TaskPropertyIds.ChangeTime, changeTime);

                // FIXME: There's performance impact on this query because we look for TaskPropertyIds.Type
                // which is requires a table join. Need to have a better way to do so.
                fc.Add(FilterOperator.Equal, TaskPropertyIds.Type, TaskType.Service);

                List<PropertyRow> taskRows = new List<PropertyRow>();

                // The ISchedulerRowSet object is a snapshot and is always a new object, so no lock is needed
                foreach (var taskRow in this.schedulerJob.OpenTaskEnumerator(collection, fc, null, true))
                {
                    taskRows.Add(taskRow);
                }

                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose,
                    "[JobMonitorEntry] GetTaskInfo, got {0} rows.", taskRows.Count);

                if (taskRows.Count == 0)
                {
                    // no service task dispathed yet.
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning,
                        "[JobMonitorEntry] Failed to get task property rows.");

                    return null;
                }

                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Query task info (got task info rows from scheduler).");

                this.schedulerJob.Refresh();
                int jobRequeueCount = this.schedulerJob.RequeueCount;
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Job requeue count is {0}", jobRequeueCount);

                List<TaskInfo> results = new List<TaskInfo>(taskRows.Count);
                foreach (PropertyRow row in taskRows)
                {
                    int objectId = (int)row[TaskPropertyIds.Id].Value;

                    TaskAllocationHistoryItem taskInfo;
                    if (!taskInfoDic.TryGetValue(objectId, out taskInfo))
                    {
                        continue;
                    }

                    TaskState state = (TaskState)row[TaskPropertyIds.State].Value;

                    if (state == TaskState.Running || state == TaskState.Dispatching)
                    {
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[JobMonitorEntry] Task {0} changed into Running", objectId);

                        string machineName = taskInfo.NodeName;
                        NodeLocation location = NodeLocation.OnPremise;
                        string azureServiceName = null;
                        string azureLoadBalancerAddress = null;

                        try
                        {
                            this.GetNodeInfo(machineName, out location, out azureServiceName, out azureLoadBalancerAddress);
                        }
                        catch (Exception e)
                        {
                            // if exception happens when querying node info, just skip this node temporarily.
                            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[JobMonitorEntry] -> Get node info for task {0} throws exception. Exception: {1}", objectId, e);
                            continue;
                        }

                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] ->Get machine name for task {0}: {1}", objectId, machineName);

                        int capacity = taskInfo.Capacity;
                        int coreindex = taskInfo.FirstCoreId;

                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] ->Get coreid for task {0}: {1}", objectId, coreindex);
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] ->Get AzureLoadBalancerAddress for task {0}: {1}", objectId, azureLoadBalancerAddress);

                        TaskInfo info = new TaskInfo();
                        info.Id = objectId;
                        info.Capacity = capacity;

                        if (SoaHelper.IsOnAzure())
                        {
                            info.MachineVirtualName = machineName;
                        }
                        else
                        {
                            info.MachineName = machineName;
                        }

                        info.Location = NodeLocationConverter.FromHpcNodeLocation(location);
                        info.ProxyServiceName = azureServiceName;
                        info.AzureLoadBalancerAddress = azureLoadBalancerAddress;
                        info.State = TaskStateConverter.FromHpcTaskState(state);
                        info.FirstCoreIndex = coreindex;
                        info.JobRequeueCount = jobRequeueCount;
                        results.Add(info);
                    }
                    else if (state == TaskState.Failed || state == TaskState.Canceled || state == TaskState.Canceling || state == TaskState.Finished || state == TaskState.Finishing)
                    {
                        TaskInfo info = new TaskInfo();
                        info.Id = objectId;
                        info.State = TaskStateConverter.FromHpcTaskState(state);
                        info.JobRequeueCount = jobRequeueCount;
                        results.Add(info);
                    }
                }

                this.lastChangeTime = changeTime;
                return results;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[JobMonitorEntry] Fail when get task info: {0}", ex);
                return null;
            }
            finally
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] Query task info finished.");
            }
        }

        /// <summary>
        /// Get node info, location and, for Azure node, azure service name and load balance address(dns name or internal load balance ip) for proxy node
        /// </summary>
        private void GetNodeInfo(string nodeName, out NodeLocation nodeLocation, out string azureServiceName, out string azureLoadBalancerAddress)
        {
            //
            // GetNodeInfo: 
            // step 1, get node info from cache. If cache is hit and the node info is not expired, done;  otherwise,
            // step 2, get node info from scheduler
            // 
            nodeLocation = NodeLocation.OnPremise;
            azureServiceName = null;
            azureLoadBalancerAddress = null;

            //step 1, get node info from cache.
            ISchedulerNode nodeInfo;
            bool hitCache = false;
            LockNodeInfoCache.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (NodeInfoCache.TryGetValue(nodeName, out nodeInfo))
                {
                    hitCache = true;
                }
            }
            finally
            {
                LockNodeInfoCache.ReleaseLock();
            }

            if (!hitCache)
            {
                // step 2, get node info from scheduler
                LockNodeInfoCache.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    // double check - if node info is found in the cache, done
                    if (NodeInfoCache.TryGetValue(nodeName, out nodeInfo))
                    {
                        hitCache = true;
                    }

                    if (!hitCache)
                    {
                        // Note: exception from OpenNodeByName will be thrown out.
                        nodeInfo = this.scheduler.OpenNodeByName(nodeName);
                        Debug.Assert(nodeInfo != null, "OpenNodeByName returns null");

                        //
                        // Register node state event so that if the node is taken offline, we can 
                        // update the node info cache immediately.
                        //
                        nodeInfo.OnNodeState += Node_OnStateChange;

                        // add node info to NodeNameInfoCache
                        NodeInfoCache.Add(nodeName, nodeInfo);
                    }
                }
                finally
                {
                    LockNodeInfoCache.ReleaseLock();
                }
            }

            nodeName = nodeInfo.Name;
            nodeLocation = nodeInfo.Location;
            azureServiceName = nodeInfo.AzureServiceName;
            azureLoadBalancerAddress = nodeInfo.AzureLoadBalancerAddress;
            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[JobMonitorEntry] GetNodeInfo: azureLoadBalancerAddress = {0}", azureLoadBalancerAddress);
        }

        /// <summary>
        /// Node state event handler.
        /// </summary>
        private static void Node_OnStateChange(object sender, NodeStateEventArg e)
        {
            if (e.NewState == NodeState.Offline)
            {
                // If node is offline, remove it from NodeInfoCache as we held the reference to ISchedulerNode instances in that cache.
                // If the node is online again, it will be added back into the cache.
                LockNodeInfoCache.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    ISchedulerNode nodeInfo = (ISchedulerNode)sender;
                    ISchedulerNode testNode;
                    if (!NodeInfoCache.TryGetValue(nodeInfo.Name, out testNode))
                    {
                        TraceHelper.TraceWarning(0, "[JobMonitorEntry] Node info cache inconsistent.");
                        return;
                    }

                    // remove node info. step 1. deregister node state event
                    nodeInfo.OnNodeState -= Node_OnStateChange;

                    // remove node info. step 2. remove node info from NodeInfoCache
                    NodeInfoCache.Remove(nodeInfo.Name);
                }
                finally
                {
                    LockNodeInfoCache.ReleaseLock();
                }
            }
        }

        /// <summary>
        /// Represents an item of task allocation history which contains node id and core id
        /// </summary>
        private class TaskAllocationHistoryItem
        {
            /// <summary>
            /// Stores the node name
            /// </summary>
            private string nodeName;

            /// <summary>
            /// Stores the core id
            /// </summary>
            private int firstCoreId;

            /// <summary>
            /// Stores the capacity
            /// </summary>
            private int capacity;

            /// <summary>
            /// Gets or sets the node name
            /// </summary>
            public string NodeName
            {
                get { return this.nodeName; }
                set { this.nodeName = value; }
            }

            /// <summary>
            /// Gets or sets the node id
            /// </summary>
            public int FirstCoreId
            {
                get { return this.firstCoreId; }
                set { this.firstCoreId = value; }
            }

            /// <summary>
            /// Gets or sets the capacity
            /// </summary>
            public int Capacity
            {
                get { return this.capacity; }
                set { this.capacity = value; }
            }
        }
    }
}
#endif