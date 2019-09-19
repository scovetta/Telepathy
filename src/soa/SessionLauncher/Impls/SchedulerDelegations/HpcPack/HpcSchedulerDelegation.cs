// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#if HPCPACK

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Eventing.Reader;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Authentication;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.HpcPack;
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.SchedulerDelegations;
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.HpcPack;

    /// <summary>
    /// Scheduler adapter for both broker and broker launcher
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, MaxItemsInObjectGraph = int.MaxValue)]
    internal class HpcSchedulerDelegation : DisposableObject, IHpcSchedulerAdapter, IHpcSchedulerAdapterInternal, ISchedulerAdapter
    {
        /// <summary>
        /// Stores the default job template name
        /// </summary>
        internal const string DefaultJobTemplateName = "Default";

        /// <summary>
        /// Stores the runaway service job reason: broker launcher not found
        /// </summary>
        private const string FailReason_BrokerLauncherNotFound = "BrokerLauncher for the service job is not found";

        /// <summary>
        /// Stores the format of the target file name
        /// </summary>
        private const string TargetFileNameFormat = "EventDump-{0}";

        /// <summary>
        /// Stores the fail reason for runaway service job: no HPC_BROKER property found after 5 min.
        /// </summary>
        private const string FailReason_RunawayServiceJob = "Failed service job because corresponding broker failed to initialize.";

        /// <summary>
        /// Stores the service job fail reason: cannot raise up broker for a runaway service job.
        /// </summary>
        private const string FailReason_BrokerNotExist = "Broker for the service job doesn't exist and cannot be launched";

        /// <summary>
        /// Stores the service job fail reason: runaway service job retry limit exceeded
        /// </summary>
        private const string FailReason_RetryRunawayServiceJobFailed = "Fail runaway service job after retrying 3 times.";

        /// <summary>
        /// Stores the service job fail reason: interactive service job is runaway
        /// </summary>
        private const string FailReason_InteractiveServiceJobRunaway = "Service job for an interactive session is runaway.";

        /// <summary>
        /// Stores the service job fail reason: interactive service job cannot be recovered
        /// </summary>
        private const string FailReason_CannotRecoverInteractiveServiceJob = "Service job for an interactive session cannot be recovered";

        /// <summary>
        /// Sotres the service job fail reason: max requeue count exceeded
        /// </summary>
        private const string FailReason_MaxRequeueCountExceed = "Max requeue count exceeded";

        /// <summary>
        /// Stores the endpoint prefix for broker launcher
        /// </summary>
        private const string BrokerLauncherEndpointPrefix = "net.tcp://";

        /// <summary>
        /// Stores the interval time to finish service job between failures
        /// </summary>
        private const int FinishServiceJobPeriod = 5000;

        /// <summary>
        /// Stores the time period
        /// </summary>
        private readonly static TimeSpan MonitorJobStateTimePeriod = TimeSpan.FromMinutes(5);
       
        /// <summary>
        /// Scheduler
        /// </summary>
        private IScheduler scheduler;

        /// <summary>
        /// The dictionary to store the monitors: (JobId, JobMonitorEntry)
        /// </summary>
        private Dictionary<int, HpcPackJobMonitorEntry> JobMonitors = new Dictionary<int, HpcPackJobMonitorEntry>();

        /// <summary>
        /// Max number of times that a job can be requeued on error
        /// </summary>
        private int maxRequeueCount;

        /// <summary>
        /// Stores the timer to monitor job state
        /// </summary>
        private Timer jobStateMonitorTimer;

        /// <summary>
        /// Stores the monitor job state flag
        /// This flag is >0 when callback is triggering, and is set to 0 when finished.
        /// </summary>
        private int monitorJobStateFlag;

        /// <summary>
        /// Stores the list of finishing jobs for Bug 8361 so that those "finishing" jobs won't be raised up by failover logic
        /// </summary>
        /// <remarks>
        /// This list is protected by lock object JobMonitors.
        /// </remarks>
        private List<int> finishingJobs = new List<int>();

        /// <summary>
        /// Stores the SessionLauncher. 
        /// </summary>
        private SessionLauncher launcher;

        /// <summary>
        /// Internal class for retry timer
        /// </summary>
        internal class RetryTimer : System.Timers.Timer
        {
            public int JobId;
            public string Message;
            public int Count;
            public int TotalCount;
            public List<Action> ActionList;
            public int CurrentAction;
        }

        /// <summary>
        /// Stores the timers for requeue service job retries
        /// </summary>
        private ConcurrentDictionary<int, RetryTimer> requeueJobRetryTimers = new ConcurrentDictionary<int, RetryTimer>();


        /// <summary>
        /// Stores the broker node manager
        /// </summary>
        private BrokerNodesManager brokerNodesManager;

        /// <summary>
        /// Initializes a new instance of the HpcSchedulerDelegation class
        /// </summary>
        public HpcSchedulerDelegation(SessionLauncher launcher, BrokerNodesManager brokerNodesManager)
        {
            try
            {
                this.launcher = launcher;
                this.brokerNodesManager = brokerNodesManager;
                this.scheduler = CommonSchedulerHelper.GetScheduler(HpcContext.Get().CancellationToken).GetAwaiter().GetResult();
                TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] Successfully initialized scheduler adapter.");
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[HpcSchedulerDelegation] .InitializeSchedulerConnect: Failed to connect to the scheduler store, Exception:{0}", e);

                throw;
            }

            // Set maxRequeueCount from cluster parameter
            this.maxRequeueCount = 3;   // By default, max requeue count is 3.
            string strMaxRequeueCount = JobHelper.GetClusterParameterValue(this.scheduler, Constant.JobRetryCountParam, "");
            if (!string.IsNullOrEmpty(strMaxRequeueCount))
            {
                try
                {
                    this.maxRequeueCount = int.Parse(strMaxRequeueCount);
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(TraceEventType.Warning, "[HpcSchedulerDelegation] .Parsing JobRetryCount parameter {0} throws exception: {1}", strMaxRequeueCount, e);
                }
            }

            this.jobStateMonitorTimer = new Timer(this.JobStateMonitorCallback, null, MonitorJobStateTimePeriod, MonitorJobStateTimePeriod);
        }

#region IHpcSchedulerAdapter and ISchedulerAdapterInternal operations
        /// <summary>
        /// Start to subscribe the job and task event
        /// </summary>
        /// <param name="jobid">indicating the job id</param>
        /// <param name="autoMax">indicating the auto max property of the job</param>
        /// <param name="autoMin">indicating the auto min property of the job</param>
        async Task<Tuple<JobState, int, int>> IHpcSchedulerAdapter.RegisterJob(int jobid)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobid <= 0, "jobid");
            TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] Begin: RegisterJob...");
            CheckBrokerAccess(jobid);

            int autoMax = 0, autoMin = 0;
            JobState state;
            try
            {
                HpcPackJobMonitorEntry data;
                lock (this.JobMonitors)
                {
                    if (!this.JobMonitors.TryGetValue(jobid, out data))
                    {
                        data = new HpcPackJobMonitorEntry(jobid, this.scheduler);
                        data.Exit += new EventHandler(JobMonitorEntry_Exit);
                    }
                }

                state = data.Start(OperationContext.Current);

                // Bug 18050: Only add/update the instance if it succeeded to
                // open the job.
                lock (this.JobMonitors)
                {
                    this.JobMonitors[jobid] = data;
                }

                autoMin = data.MinUnits;
                autoMax = data.MaxUnits;
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] Exception thrown while registering job: {0}", e);
                throw;
            }

            TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] End: RegisterJob. Current job state = {0}.", state);
            return await Task.FromResult(new Tuple<JobState, int, int>(state, autoMax, autoMin));
        }

        /// <summary>
        /// Update the job's properties
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="properties">the properties table</param>
        async Task<bool> IHpcSchedulerAdapterInternal.UpdateBrokerInfo(int jobid, Dictionary<string, object> properties)
        {
            return await ((IHpcSchedulerAdapter)this).UpdateBrokerInfo(jobid, properties);
        }

        /// <summary>
        /// Update the job's properties
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="properties">the properties table</param>
        async Task<bool> IHpcSchedulerAdapter.UpdateBrokerInfo(int jobid, Dictionary<string, object> properties)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobid <= 0, "jobid");
            ParamCheckUtility.ThrowIfNull(properties, "properties");

            TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] UpdateBrokerInfo...");
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (KeyValuePair<string, object> property in properties)
            {
                sb.AppendLine(String.Format("Property = {0}\tValue = {1}", property.Key, property.Value));
            }

            TraceHelper.TraceEvent(jobid, TraceEventType.Verbose, "[HpcSchedulerDelegation] Properties detail:\n{0}", sb.ToString());
            CheckBrokerAccess(jobid);

            try
            {
                ISchedulerJob schedulerJob = null;
                int minUnits = 0;
                int maxUnits = int.MaxValue;
                lock (this.JobMonitors)
                {
                    HpcPackJobMonitorEntry entry = null;
                    if (this.JobMonitors.TryGetValue(jobid, out entry))
                    {
                        schedulerJob = entry.SchedulerJob;
                        minUnits = entry.MinUnits;
                        maxUnits = entry.MaxUnits;
                    }
                }

                bool needLockJob = true;

                if (schedulerJob == null)
                {
                    schedulerJob = this.scheduler.OpenJob(jobid) as ISchedulerJob;
                    Debug.Assert(schedulerJob != null);

                    needLockJob = false;
                }

                // Only need to lock the job when it come from JobMonitor.
                // JobMonitorEntry calls job.Refresh, which impacts SetCustomProps and Commit in
                // UpdateSoaRelatedProperties method below. Bug5944(v3)
                if (needLockJob)
                {
                    lock (schedulerJob)
                    {
                        this.UpdateSoaRelatedProperties(schedulerJob, properties, minUnits, maxUnits);
                    }
                }
                else
                {
                    this.UpdateSoaRelatedProperties(schedulerJob, properties, minUnits, maxUnits);
                }

                // only happpens in debug build
                CheckSoaRelatedProperties(schedulerJob, properties, SchedulerDelegationCommon.CustomizedPropertyNames, SchedulerDelegationCommon.PropToEnvMapping);

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(jobid, TraceEventType.Warning, "Update broker fail: {0}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get all the queued and running durable jobs information back to the broker
        /// Cancel all non-durable service jobs which owned by the caller
        /// </summary>
        /// <returns>broker recover info array</returns>
        async Task<BrokerRecoverInfo[]> IHpcSchedulerAdapterInternal.GetRecoverInfoFromJobs(string machineName)
        {
            ParamCheckUtility.ThrowIfNullOrEmpty(machineName, "machineName");
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] Get recover info from jobs, machineName = {0}", machineName);
            CheckBrokerAccess(0);

            try
            {
                List<BrokerRecoverInfo> recoverInfoList = new List<BrokerRecoverInfo>();

                FilterCollection collection = new FilterCollection();
                collection.Add(new FilterProperty(FilterOperator.NotEqual, JobPropertyIds.ServiceName, string.Empty));
                collection.Add(new FilterProperty(FilterOperator.HasBitSet, JobPropertyIds.State, JobState.Running | JobState.Queued));

                foreach (ISchedulerJob job in this.scheduler.GetJobList(collection, null))
                {
                    int id = job.Id;
                    string brokerNode = null;
                    bool durable = false;

                    try
                    {
                        Dictionary<string, string> dic = JobHelper.GetCustomizedProperties(job, BrokerSettingsConstants.BrokerNode, BrokerSettingsConstants.Durable);
                        dic.TryGetValue(BrokerSettingsConstants.BrokerNode, out brokerNode);

                        string tmp;
                        if (dic.TryGetValue(BrokerSettingsConstants.Durable, out tmp))
                        {
                            durable = Convert.ToBoolean(tmp);
                        }
                        else
                        {
                            // Bug 5784: if HPC_DURABLE is not set in the job property, just ignore because the corresponding broker is still initializing
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        // Exception happens because convert the job property fails
                        // Ignore current property row
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[HpcSchedulerDelegation] Failed to load information from job: {0}, {1}", id, e);
                        continue;
                    }

                    if (!brokerNode.Equals(machineName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (!durable)
                    {
                        try
                        {
                            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] Fail non-durable runaway job.");
                            this.FailJob(job, FailReason_CannotRecoverInteractiveServiceJob);
                        }
                        catch (Exception ex)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Warning, "[HpcSchedulerDelegation] Fail non-durable runaway job failed: Id = {0}, Exception={1}", id, ex);
                        }

                        continue;
                    }

                    BrokerRecoverInfo info;
                    try
                    {
                        info = await ((IHpcSchedulerAdapterInternal)this).GetRecoverInfoFromJob(id);
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[HpcSchedulerDelegation] Failed to load start info from service job: {0}, {1}", id, e);
                        continue;
                    }

                    TraceHelper.TraceEvent(TraceEventType.Verbose, "[HpcSchedulerDelegation] Get broker info: Id = {0}", id);
                    recoverInfoList.Add(info);
                }

                return await Task.FromResult(recoverInfoList.ToArray());
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[HpcSchedulerDelegation] Failed to get recover info from jobs: {0}\nMachineName = {1}", e, machineName);
                throw;
            }
        }

        /// <summary>
        /// Create the sessionstartinfo from job properties.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>the sessionstart info</returns>
        async Task<BrokerRecoverInfo> IHpcSchedulerAdapterInternal.GetRecoverInfoFromJob(int jobid)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobid <= 0, "jobid");
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] Get recover info from job {0}...", jobid);
            CheckBrokerAccess(jobid);

            try
            {
                ISchedulerJob schedulerJob = this.scheduler.OpenJob(jobid) as ISchedulerJob;
                Debug.Assert(schedulerJob != null);

                if (schedulerJob.State == JobState.Canceled)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Session_ValidateJobFailed_JobCanceled, SR.SessionLauncher_ValidateJobFailed_JobCanceled, jobid.ToString());
                }

                string serviceName = JobHelper.GetServiceName(schedulerJob);
                if (string.IsNullOrEmpty(serviceName))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Session_ValidateJobFailed_NotServiceJob, SR.SessionLauncher_ValidateJobFailed_NotServiceJob, jobid.ToString());
                }

                Dictionary<string, string> dic = JobHelper.GetCustomizedProperties(schedulerJob, SchedulerDelegationCommon.CustomizedPropertyNames.ToArray());

                bool durable = Convert.ToBoolean(dic[BrokerSettingsConstants.Durable], CultureInfo.InvariantCulture);
                if (!durable)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Session_ValidateJobFailed_NotDurableSession, SR.SessionLauncher_ValidateJobFailed_NotDurableSession, jobid.ToString());
                }

                if (schedulerJob.State == JobState.Finished || schedulerJob.State == JobState.Failed || schedulerJob.State == JobState.Finishing)
                {
                    bool suspended = Convert.ToBoolean(dic[BrokerSettingsConstants.Suspended], CultureInfo.InvariantCulture);
                    if (!suspended)
                    {
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.Session_ValidateJobFailed_AlreadyFinished, SR.SessionLauncher_ValidateJobFailed_AlreadyFinished, jobid.ToString());
                    }
                }

                //FIXME:
                SessionStartInfoContract startInfo = new SessionStartInfoContract();
                BrokerRecoverInfo recoverInfo = new BrokerRecoverInfo();
                recoverInfo.Durable = true;
                recoverInfo.SessionId = jobid;
                recoverInfo.StartInfo = startInfo;

                startInfo.ServiceName = serviceName;

                string strValue;
                if (dic.TryGetValue(BrokerSettingsConstants.ServiceConfigMaxMessageSize, out strValue))
                {
                    startInfo.MaxMessageSize = Convert.ToInt32(strValue);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.ServiceConfigOperationTimeout, out strValue))
                {
                    startInfo.ServiceOperationTimeout = Convert.ToInt32(strValue);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.ShareSession, out strValue))
                {
                    startInfo.ShareSession = Convert.ToBoolean(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.UseAad, out strValue))
                {
                    startInfo.UseAad = Convert.ToBoolean(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.AadUserIdentity, out strValue))
                {
                    var vals = strValue.Split(';');
                    if (vals.Length == 2)
                    {
                        recoverInfo.AadUserSid = vals[0];
                        recoverInfo.AadUserName = vals[1];
                    }
                    else
                    {
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[HpcSchedulerDelegation].GetRecoverInfoFromJob: malformed AAD user identity information: {0}", strValue); 
                    }
                }

                if (dic.TryGetValue(BrokerSettingsConstants.Secure, out strValue))
                {
                    startInfo.Secure = Convert.ToBoolean(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.TransportScheme, out strValue))
                {
                    startInfo.TransportScheme = (TransportScheme)Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.AllocationGrowLoadRatioThreshold, out strValue))
                {
                    startInfo.AllocationGrowLoadRatioThreshold = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.AllocationShrinkLoadRatioThreshold, out strValue))
                {
                    startInfo.AllocationShrinkLoadRatioThreshold = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.ClientIdleTimeout, out strValue))
                {
                    startInfo.ClientIdleTimeout = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.SessionIdleTimeout, out strValue))
                {
                    startInfo.SessionIdleTimeout = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.DispatcherCapacityInGrowShrink, out strValue))
                {
                    startInfo.DispatcherCapacityInGrowShrink = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.MessagesThrottleStartThreshold, out strValue))
                {
                    startInfo.MessagesThrottleStartThreshold = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.MessagesThrottleStopThreshold, out strValue))
                {
                    startInfo.MessagesThrottleStopThreshold = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.ClientConnectionTimeout, out strValue))
                {
                    startInfo.ClientConnectionTimeout = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.ServiceVersion, out strValue))
                {
                    Version version = null;

                    // Move to Parse in .Net 4.0
                    try
                    {
                        version = new Version(strValue);
                    }
                    catch (Exception ex)
                    {
                        version = null;
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[HpcSchedulerDelegation].GetRecoverInfoFromJob: New Version Exception {0}", ex);
                    }

                    startInfo.ServiceVersion = version;
                }

                if (dic.TryGetValue(BrokerSettingsConstants.UseAzureQueue, out strValue))
                {
                    startInfo.UseAzureQueue = Convert.ToBoolean(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.LocalUser, out strValue))
                {
                    startInfo.LocalUser = Convert.ToBoolean(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.PurgedFaulted, out strValue))
                {
                    recoverInfo.PurgedFailed = Convert.ToInt64(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.PurgedProcessed, out strValue))
                {
                    recoverInfo.PurgedProcessed = Convert.ToInt64(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.PurgedTotal, out strValue))
                {
                    recoverInfo.PurgedTotal = Convert.ToInt64(strValue, CultureInfo.InvariantCulture);
                }

                if (dic.TryGetValue(BrokerSettingsConstants.PersistVersion, out strValue))
                {
                    recoverInfo.PersistVersion = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }

                return await Task.FromResult(recoverInfo);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[HpcSchedulerDelegation] Failed to get recover info from job {0}: {1}", jobid, e);
                throw;
            }
        }

        /// <summary>
        /// Get ACL string from a job template
        /// </summary>
        /// <param name="jobTemplate">the job template name</param>
        /// <returns>ACL string</returns>
        async Task<string> IHpcSchedulerAdapterInternal.GetJobOwnerSID(int jobid)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobid <= 0, "jobid");
            TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] GetUserSID...");
            CheckBrokerAccess(jobid);
            return await Task.FromResult(GetJobOwnerSIDInternal(jobid));
        }

        /// <summary>
        /// verify a job is there
        /// </summary>
        /// <param name="jobId">indicating the job id</param>
        async Task<bool> IHpcSchedulerAdapterInternal.IsValidJob(int jobId)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobId <= 0, "jobId");
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] Verify job {0} is valid.", jobId);
            CheckBrokerAccess(jobId);

            try
            {
                this.scheduler.OpenJob(jobId);
            }
            catch (SchedulerException e)
            {
                if (e.Code == ErrorCode.Operation_InvalidJobId)
                {
                    TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] IsValidJob: job(id={0}) doesnot exist, Exception:{1}", jobId, e);
                    return false;
                }
                else
                {
                    TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] IsValidJob: Open job(id={0}) raised exception:{1}", jobId, e);
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] IsValidJob: Open job(id={0}) raised exception:{1}", jobId, e);
            }

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Get the allocated node name of the specified task.
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <param name="taskId">task id</param>
        /// <returns>list of the node name and location flag (on premise or not)</returns>
        async Task<List<Tuple<string, bool>>> IHpcSchedulerAdapterInternal.GetTaskAllocatedNodeName(int jobId, int taskId)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobId <= 0, "jobId");
            ParamCheckUtility.ThrowIfOutofRange(taskId <= 0, "taskId");
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] GetTaskAllocatedNodeName of task {0} in job {1}.", taskId, jobId);

            CheckBrokerAccess(jobId);

            try
            {
                ISchedulerJob job = this.scheduler.OpenJob(jobId);

                PropertyIdCollection properties = new PropertyIdCollection();
                properties.Add(TaskPropertyIds.AllocatedNodes);

                FilterCollection filters = new FilterCollection();
                filters.Add(new FilterProperty(FilterOperator.Equal, TaskPropertyIds.Id, taskId));

                using (var taskRowSet = job.OpenTaskRowSet(properties, filters, null, true))
                {
                    if (taskRowSet.GetCount() == 0)
                    {
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.DiagService_InvalidTaskId, SR.DiagService_InvalidTaskId, taskId.ToString());
                    }

                    Debug.Assert(
                        taskRowSet.GetCount() == 1,
                        "[HpcSchedulerDelegation] As global task id was given, there should be only one element in the row set.");

                    PropertyRow[] rows = taskRowSet.GetRows(0, 0).Rows;
                    StoreProperty prop = rows[0][TaskPropertyIds.AllocatedNodes];
                    List<KeyValuePair<string, int>> nodes = (List<KeyValuePair<string, int>>)prop.Value;

                    Debug.Assert(
                        nodes.Count == 1,
                        "[HpcSchedulerDelegation] Should have only one node for a task.");

                    List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();

                    try
                    {
                        // consider caching the node location
                        ISchedulerNode node = this.scheduler.OpenNodeByName(nodes[0].Key);

                        Tuple<string, bool> tuple =
                            new Tuple<string, bool>(node.Name, node.Location == NodeLocation.OnPremise);

                        list.Add(tuple);
                    }
                    catch (SchedulerException se)
                    {
                        if (se.Code == ErrorCode.Operation_InvalidNodeId)
                        {
                            // the node does not exist in the cluster, so can not get its info
                            TraceHelper.TraceEvent(
                                TraceEventType.Warning,
                                "[HpcSchedulerDelegation] Can not OpenNodeByName, the node may not exist in the cluster, NodeName: {0}, {1}",
                                nodes[0].Key,
                                se);
                        }
                        else
                        {
                            throw;
                        }
                    }

                    return await Task.FromResult(list);
                }
            }
            catch (Exception e)
            {
                string messageFormat = "[HpcSchedulerDelegation] Failed to GetTaskAllocatedNodeName of task {0} in job {1}: {2}";

                TraceHelper.TraceEvent(TraceEventType.Error, messageFormat, taskId, jobId, e);

                ThrowHelper.ThrowSessionFault(
                    SOAFaultCode.UnknownError,
                    messageFormat,
                    taskId.ToString(),
                    jobId.ToString(),
                    e.ToString());

                return null;
            }
        }

        /// <summary>
        /// Get the allocated node name of the specified job.
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <returns>list of the node name and location flag (on premise or not)</returns>
        async Task<List<Tuple<string, bool>>> IHpcSchedulerAdapterInternal.GetJobAllocatedNodeName(int jobId)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobId <= 0, "jobId");

            TraceHelper.TraceEvent(
                jobId,
                TraceEventType.Information,
                "[HpcSchedulerDelegation] GetJobAllocatedNodeName: Get AllocatedNodeName of job {0}.",
                jobId);

            CheckBrokerAccess(jobId);

            try
            {
                // Notice: Do not use this.scheduler.OpenJob(jobId).AllocatedNodes,
                // it does not return shrink node names when job is running.

                List<string> nodes = new List<string>();

                PropertyIdCollection allocationPropertyCollection = new PropertyIdCollection();
                allocationPropertyCollection.AddPropertyId(AllocationProperties.NodeName);

                using (ISchedulerRowEnumerator rows = this.scheduler.OpenJob(jobId).OpenJobAllocationHistoryEnumerator(allocationPropertyCollection))
                {
                    foreach (PropertyRow row in rows)
                    {
                        if (row.Props[0].Id == AllocationProperties.NodeName)
                        {
                            string nodeName = row.Props[0].Value as string;

                            Debug.Assert(
                                !string.IsNullOrEmpty(nodeName),
                                "GetJobAllocatedNodeName: Node name should not be null or empty.");

                            if (!nodes.Contains(nodeName, StringComparer.InvariantCultureIgnoreCase))
                            {
                                nodes.Add(nodeName);

                                TraceHelper.TraceEvent(
                                    jobId,
                                    TraceEventType.Verbose,
                                    "[HpcSchedulerDelegation] GetJobAllocatedNodeName: Get allocated node {0}",
                                    nodeName);
                            }
                        }
                    }
                }

                TraceHelper.TraceEvent(
                    jobId,
                    TraceEventType.Verbose,
                    "[HpcSchedulerDelegation] GetJobAllocatedNodeName: Get {0} allocated nodes.",
                    nodes.Count);

                List<Tuple<string, bool>> list = new List<Tuple<string, bool>>();

                foreach (string nodeName in nodes)
                {
                    try
                    {
                        ISchedulerNode node = this.scheduler.OpenNodeByName(nodeName);

                        Tuple<string, bool> tuple =
                            new Tuple<string, bool>(node.Name, node.Location == NodeLocation.OnPremise);

                        list.Add(tuple);

                        TraceHelper.TraceEvent(
                            jobId,
                            TraceEventType.Verbose,
                            "[HpcSchedulerDelegation] GetJobAllocatedNodeName: Allocated node {0} is {1}.",
                            node.Name,
                            node.Location);
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceWarning(jobId, "[HpcSchedulerDelegation] GetJobAllocatedNodeName: Failed to open node by name {0}: {1}", nodeName, e);
                        continue;
                    }
                }

                return await Task.FromResult(list);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(
                    jobId,
                    TraceEventType.Error,
                    "[HpcSchedulerDelegation] GetJobAllocatedNodeName: Failed to get allocated nodes of job {0}: {1}",
                    jobId,
                    e);

                throw;
            }
        }

        /// <summary>
        /// Get all the exist session id list
        /// </summary>
        /// <returns>session id list</returns>
        async Task<List<int>> IHpcSchedulerAdapterInternal.GetAllSessionId()
        {
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] GetAllSessionId");

            this.CheckClusterAccess();

            try
            {
                IFilterCollection fc = new FilterCollection();
                fc.Add(FilterOperator.HasBitSet, JobPropertyIds.RuntimeType, JobRuntimeType.SOA);

                IIntCollection col = this.scheduler.GetJobIdList(fc, null);

                return await Task.FromResult(new List<int>(col));
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[HpcSchedulerDelegation] Failed to GetAllSessionId {0}", e);
                throw;
            }
        }

        /// <summary>
        /// Get the broker node name list.
        /// </summary>
        /// <returns>Get the broker node name list.</returns>
        async Task<List<string>> IHpcSchedulerAdapterInternal.GetBrokerNodeName()
        {
            this.CheckClusterAccess();
            return await Task.FromResult(this.brokerNodesManager.GetAvailableBrokerNodeName());
        }

        /// <summary>
        /// Get the broker name of session specified by the job Id.
        /// </summary>
        /// <param name="jobId">job Id of the session</param>
        /// <returns>broker node name</returns>
        async Task<string> IHpcSchedulerAdapterInternal.GetSessionBrokerNodeName(int jobId)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobId <= 0, "jobId");
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] GetSessionBrokerNodeName of job {0}.", jobId);
            CheckBrokerAccess(jobId);

            try
            {
                ISchedulerJob job = this.scheduler.OpenJob(jobId);

                Dictionary<string, string> dic = JobHelper.GetCustomizedProperties(job, BrokerSettingsConstants.BrokerNode);

                string brokerNode = null;
                if (!dic.TryGetValue(BrokerSettingsConstants.BrokerNode, out brokerNode))
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[HpcSchedulerDelegation] Can't get broker node name of session {0}", jobId);
                }

                TraceHelper.TraceEvent(TraceEventType.Verbose, "[HpcSchedulerDelegation] The broker node name of session {0} is {1}", jobId, brokerNode);
                return await Task.FromResult(brokerNode);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[HpcSchedulerDelegation] Failed to GetSessionBrokerNodeName of job {0}: {1}", jobId, e);
                throw;
            }
        }

        /// <summary>
        /// Check if the soa diag trace enabled for the specified session.
        /// </summary>
        /// <param name="jobId">job id of the session</param>
        /// <returns>soa diag trace is enabled or disabled </returns>
        bool IHpcSchedulerAdapterInternal.IsDiagTraceEnabled(int jobId)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobId <= 0, "jobId");
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] IsDiagTraceEnabled of job {0}.", jobId);
            if (OperationContext.Current != null) // could be null for service call
            {
                CheckBrokerAccess(jobId);
            }

            try
            {
                ISchedulerJob job = this.scheduler.OpenJob(jobId);

                var dic = JobHelper.GetCustomizedProperties(job, BrokerSettingsConstants.SoaDiagTraceLevel);

                SourceLevels level = SourceLevels.Off;
                if (Enum.TryParse<SourceLevels>(dic[BrokerSettingsConstants.SoaDiagTraceLevel], out level))
                {
                    return level != SourceLevels.Off;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "[HpcSchedulerDelegation] Failed to get DiagTraceEnabled property of job {0}: {1}",
                    jobId,
                    e);

                throw;
            }
        }

        async Task IHpcSchedulerAdapterInternal.SetJobProgressMessage(int jobid, string message)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobid <= 0, "jobid");
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] SetJobProgressMessage of job {0}.", jobid);
            CheckBrokerAccess(jobid);

            try
            {
                ISchedulerJob job = this.scheduler.OpenJob(jobid);
                job.ProgressMessage = message;
                job.Commit();
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "[HpcSchedulerDelegation] Failed to SetJobProgressMessage of job {0}: {1}",
                    jobid,
                    e);

                throw;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Get specified job's customized properties.
        /// </summary>
        /// <param name="propNames">customized property names</param>
        /// <returns>customized properties</returns>
        async Task<Dictionary<string, string>> IHpcSchedulerAdapterInternal.GetJobCustomizedProperties(int jobid, string[] propNames)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobid <= 0, "jobId");
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] GetJobCustomizedProperties of job {0}.", jobid);
            this.CheckClusterAccess();

            try
            {
                ISchedulerJob job = this.scheduler.OpenJob(jobid);
                return await Task.FromResult(JobHelper.GetCustomizedProperties(job, propNames));
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "[HpcSchedulerDelegation] Failed to get customized properties of job {0}: {1}",
                    jobid,
                    e);

                throw;
            }
        }

        /// <summary>
        /// Finish a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        async Task IHpcSchedulerAdapter.FinishJob(int jobid, string reason)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobid <= 0, "jobid");
            TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] Finish job: {0}", reason);
            CheckBrokerAccess(jobid);

            try
            {
                HpcPackJobMonitorEntry entry = null;

                lock (this.JobMonitors)
                {
                    // check if there is any data can be removed
                    if (this.JobMonitors.TryGetValue(jobid, out entry))
                    {
                        this.JobMonitors.Remove(jobid);
                    }

                    // this.finishingJobs is protected by lock object this.JobMonitors
                    // this lock contains two operations that needs to be atomic: 
                    // 1. Remove the job entry instance from JobMonitors
                    // 2. Add the job id into finishingJobs so that this job id would be ignored by failover logic
                    this.finishingJobs.Add(jobid);
                }

                ISchedulerJob job = null;
                if (entry != null)
                {
                    TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] Finish job: find and close the job monitor entry");
                    job = entry.SchedulerJob;
                    entry.Exit -= new EventHandler(JobMonitorEntry_Exit);
                    entry.Close();
                }

                // the job in the Monitor can be null, so open it via scheduler
                if (job == null)
                {
                    TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] Finish job: job is null, so open the job via the scheduler");
                    job = this.scheduler.OpenJob(jobid) as ISchedulerJob;
                    Debug.Assert(job != null);
                }

                ThreadPool.QueueUserWorkItem(this.CallbackToFinishServiceJob, job);

                await Task.CompletedTask;
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] Finish job failed: {0}", e);
                lock (this.JobMonitors)
                {
                    this.finishingJobs.Remove(jobid);
                }

                throw;
            }
        }

        /// <summary>
        /// Fail a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        async Task IHpcSchedulerAdapterInternal.FailJob(int jobid, string reason)
        {
            await ((IHpcSchedulerAdapter)this).FailJob(jobid, reason);
        }

        /// <summary>
        /// Fail a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        async Task IHpcSchedulerAdapter.FailJob(int jobid, string reason)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobid <= 0, "jobid");
            CheckBrokerAccess(jobid);

            try
            {
                HpcPackJobMonitorEntry entry = null;

                lock (this.JobMonitors)
                {
                    // check if there is any data can be removed
                    if (this.JobMonitors.TryGetValue(jobid, out entry))
                    {
                        this.JobMonitors.Remove(jobid);
                    }
                }

                ISchedulerJob job = null;
                if (entry != null)
                {
                    job = entry.SchedulerJob;
                    entry.Exit -= new EventHandler(JobMonitorEntry_Exit);
                    entry.Close();
                }

                // the job in the Monitor can be null, so open it via scheduler
                if (job == null)
                {
                    job = this.scheduler.OpenJob(jobid) as ISchedulerJob;
                }

                if (job == null)
                {
                    TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] FailJob: invalid job id");
                }
                else
                {
                    this.FailJob(job, reason);
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] FailJob failed: {0}", e);
                throw;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Requeue or fail a service job with reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">he reason string</param>
        async Task IHpcSchedulerAdapter.RequeueOrFailJob(int jobid, string reason)
        {
            TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] Requeue or fail job: reason = {0}", reason);
            CheckBrokerAccess(jobid);

            try
            {
                HpcPackJobMonitorEntry entry = null;

                lock (this.JobMonitors)
                {
                    if (!this.JobMonitors.TryGetValue(jobid, out entry))
                    {
                        TraceHelper.TraceEvent(jobid, TraceEventType.Warning, "[HpcSchedulerDelegation] Requeue or fail job: job not found");
                        return;
                    }
                }

                ISchedulerJob job = entry.SchedulerJob;
                job.Refresh();

                // If the job is not running, leave it as it is
                if (job.State != JobState.Running)
                {
                    TraceHelper.TraceInfo(jobid, "[HpcSchedulerDelegation] Do not requeue or fail service job because the job is not running. It might has already been requeued by scheduler.");
                    return;
                }

                // Requeue job for at most maxRequeueCount times, then fail it.
                int requeueCount = entry.RequeueCount;
                if (requeueCount >= this.maxRequeueCount)
                {
                    // Fail job
                    TraceHelper.TraceEvent(jobid, TraceEventType.Warning, "[HpcSchedulerDelegation] Fail job as requeue count reaches threshold");
                    bool removed = false;
                    lock (this.JobMonitors)
                    {
                        removed = this.JobMonitors.Remove(jobid);
                    }

                    // close entry
                    if (removed)
                    {
                        // Do not close entry here, entry will be closed when job failed event is triggered
                        this.FailJob(job, FailReason_MaxRequeueCountExceed);
                    }
                }
                else
                {
                    // requeue job
                    TraceHelper.TraceEvent(jobid, TraceEventType.Warning, "[HpcSchedulerDelegation] Requeue job. Requeue count = {0}", requeueCount);

                    // indicate job's JobMonitorEntry that it is entering a requeue phase
                    if (entry.PrepareForRequeueJob())
                    {
                        entry.RequeueCount++;
                        RetryTimer timer = new RetryTimer
                        {
                            JobId = jobid,
                            Message = reason,
                            TotalCount = 12,
                            ActionList = new List<Action> {
                                ()=>{ this.scheduler.CancelJob(jobid, string.Format("Cancel job for requeue.  Reason: {0}", reason)); },
                                ()=>{ this.scheduler.ConfigureJob(jobid); },
                                ()=>{
                                    this.scheduler.SubmitJobById(jobid, null, null);
                                    // Bug 19712: Sometimes job state event might be lost so that
                                    // the "isReqeueing" flag was not reset after successfully
                                    // requeued the service job. Need to manually reset this flag.
                                    // Note: After SubmitJobById finished succesfully, the job
                                    // must have at least entered "queued" state.
                                    try
                                    {
                                        HpcPackJobMonitorEntry ent = null;
                                        lock (this.JobMonitors)
                                        {
                                            this.JobMonitors.TryGetValue(jobid, out ent);
                                        }

                                        if (ent != null)
                                        {
                                            ent.CallbackToQueryTaskInfo(true);
                                        }
                                    }
                                    catch(Exception e)
                                    {
                                        TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] Failed to query task info: {0}", e);
                                    }
                                }
                            },
                            AutoReset = false,
                            Interval = 1
                        };

                        timer.Elapsed += RequeueServiceJobSteps;

                        if (!this.requeueJobRetryTimers.TryAdd(jobid, timer))
                        {
                            TraceHelper.TraceEvent(jobid, TraceEventType.Warning, "[HpcSchedulerDelegation] Failed to add the requeue job retry timer for job: {0}", jobid);
                            timer.Dispose();
                        }
                        else
                        {
                            timer.Start();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] Cancel job failed: {0}", e);
                throw;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Add a node to job's exclude node list
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="nodeName">name of the node to be excluded</param>
        /// <returns>true if the node is successfully excluded, or the job is failed. false otherwise</returns>
        async Task<bool> IHpcSchedulerAdapter.ExcludeNode(int jobid, string nodeName)
        {
            CheckBrokerAccess(jobid);

            try
            {
                HpcPackJobMonitorEntry entry = null;

                lock (this.JobMonitors)
                {
                    if (!this.JobMonitors.TryGetValue(jobid, out entry))
                    {
                        return false;
                    }
                }

                ISchedulerJob job = entry.SchedulerJob;
                StringCollection excludeNodes = new StringCollection();
                excludeNodes.Add(nodeName);
                job.AddExcludedNodes(excludeNodes);
                TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] Node = {0} excluded.", nodeName);
                return await Task.FromResult(true);
            }
            catch (SchedulerException e)
            {
                if (e.Code == ErrorCode.Operation_ExcludedRequiredNode)
                {
                    TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] Node = {0} is required node. Deltail: exception = {1}", nodeName, e);

                    // Requeue or fail the job if the node is a required node
                    // TODO: localize reason string
                    await ((IHpcSchedulerAdapter)this).RequeueOrFailJob(jobid, "Service on required node is failed");
                    return true;
                }
                else if (e.Code == ErrorCode.Operation_ExcludedTooManyNodes)
                {
                    TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] Job's min cannot be met when excluding node = {0}. Detail: exception = {1}", nodeName, e);

                    // Requeue or fail the job if job's min cannot be met
                    // TODO: localize reason string
                    await ((IHpcSchedulerAdapter)this).RequeueOrFailJob(jobid, "Job's min cannot be met due to service failure");
                    return true;
                }
                else if (e.Code == ErrorCode.Operation_ExcludedNodeListTooLong)
                {
                    TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] ExcludedNodes list is too long when excluding node ={0}. Detail: {1}", nodeName, e);

                    // Requeue or fail the job if job's ExcludedNodes list is too long
                    // TODO: localize reason string
                    await ((IHpcSchedulerAdapter)this).RequeueOrFailJob(jobid, "Job's ExcludedNodes list is too long");
                    return true;
                }
                else
                {
                    // for all other exception
                    TraceHelper.TraceEvent(jobid, TraceEventType.Warning, "[HpcSchedulerDelegation] Exclude node = {0} failed: {1}", nodeName, e);
                    return false;
                }

                // TODO: handle scheduler busy exception and retry!
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(jobid, TraceEventType.Error, "[HpcSchedulerDelegation] Exclude node = {0} failed: {1}", nodeName, e);
                return false;
            }
        }

        public async Task<(bool succeed, BalanceInfo balanceInfo, List<int> taskIds, List<int> runningTaskIds)> GetGracefulPreemptionInfoAsync(int sessionId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> FinishTaskAsync(int jobId, int taskUniqueId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExcludeNodeAsync(int jobid, string nodeName)
        {
            throw new NotImplementedException();
        }

        public async Task RequeueOrFailJobAsync(int sessionId, string reason)
        {
            throw new NotImplementedException();
        }

        public async Task FailJobAsync(int sessionId, string reason)
        {
            throw new NotImplementedException();
        }

        public async Task FinishJobAsync(int sessionId, string reason)
        {
            throw new NotImplementedException();
        }

        public async Task<(Data.JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(int jobid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancel a task.
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="taskUniqueId">the task unique id</param>
        async Task<bool> IHpcSchedulerAdapter.FinishTask(int jobid, int taskUniqueId)
        {
            CheckBrokerAccess(jobid);
            TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] .FinishTask: start to finish task {0} of job {1} by broker.", taskUniqueId, jobid);

            try
            {
                HpcPackJobMonitorEntry entry = null;

                lock (this.JobMonitors)
                {
                    if (!this.JobMonitors.TryGetValue(jobid, out entry))
                    {
                        TraceHelper.TraceEvent(jobid, TraceEventType.Warning, "[HpcSchedulerDelegation] .FinishTask: Failed to finish {0} because cannot find the job entry.", taskUniqueId);
                        return true;
                    }
                }

                ISchedulerJob job = entry.SchedulerJob;
                await Task.Factory.FromAsync(
                    job.BeginFinishTask,
                    ar =>
                    {
                        var state = job.EndFinishTask(ar);

                        // Here we don't check the final state, as when call back, the state should be in one of an end state.
                        TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] .FinishTask: task {0} finish by broker, final state {1}.", taskUniqueId, state);
                    },
                    taskUniqueId,
                    "Finished by broker",
                    true,
                    null);

                return await Task.FromResult(true);
            }
            catch (SchedulerException se)
            {
                if (se.Code == ErrorCode.Operation_InvalidTaskId)
                {
                    TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] .FinishTask: task {0} cannot be found.", taskUniqueId);
                    return true;
                }
                else if (se.Code == ErrorCode.Operation_InvalidCancelTaskState)
                {
                    TraceHelper.TraceEvent(jobid, TraceEventType.Information, "[HpcSchedulerDelegation] .FinishTask: task {0} is not in a state that can be cancelled.", taskUniqueId);
                    return true;
                }

                TraceHelper.TraceEvent(jobid, TraceEventType.Warning, "[HpcSchedulerDelegation] .FinishTask: Task Id {0}, Failed with exception {1}", taskUniqueId, se);
                return false;
            }
            catch (Exception e)
            {
                // for all other exception
                TraceHelper.TraceEvent(jobid, TraceEventType.Warning, "[HpcSchedulerDelegation] .FinishTask: Task Id {0}, Failed with exception {1}", taskUniqueId, e);
                return false;
                // TODO: handle scheduler busy exception and retry!
            }
        }

        /// <summary>
        /// Get ACL String from a job tempalte
        /// </summary>
        /// <param name="jobTemplate">indicating job template</param>
        /// <returns>user's sid</returns>
        async Task<string> IHpcSchedulerAdapterInternal.GetJobTemlpateACL(string jobTemplate)
        {
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] Get job template ACL, Template = {0}", jobTemplate);
            CheckBrokerAccess(0);

            if (string.IsNullOrEmpty(jobTemplate))
            {
                jobTemplate = DefaultJobTemplateName;
            }

            try
            {
                JobTemplateInfo info = this.scheduler.GetJobTemplateInfo(jobTemplate);
                return await Task.FromResult(info.SecurityDescriptor);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[HpcSchedulerDelegation] Failed to get job template ACL: {0}", e);
                throw;
            }
        }


        /// <summary>
        /// Get the error code property of the specified task.
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <param name="globalTaskId">unique task id</param>
        /// <returns>return error code value if it exists, otherwise return null</returns>
        async Task<int?> IHpcSchedulerAdapter.GetTaskErrorCode(int jobId, int globalTaskId)
        {
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] Get task error code for job {0}, TaskId = {1}", jobId, globalTaskId);
            CheckBrokerAccess(jobId);
            int? exitcode = null;

            try
            {
                ISchedulerJob job = this.scheduler.OpenJob(jobId);

                FilterCollection filters = new FilterCollection();
                filters.Add(FilterOperator.Equal, TaskPropertyIds.Id, globalTaskId);

                PropertyIdCollection propids = new PropertyIdCollection();
                propids.Add(TaskPropertyIds.ErrorCode);

                using (ISchedulerRowSet rowset = job.OpenTaskRowSet(propids, filters, null, true))
                {
                    Debug.Assert(rowset.GetCount() == 1);
                    PropertyRowSet propSet = rowset.GetRows(0, 0);
                    StoreProperty prop = propSet[0][TaskPropertyIds.ErrorCode];
                    if (prop != null)
                    {
                        exitcode = prop.Value as int?;
                    }
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error,
                    "[SessionLauncher] GetTaskErrorCode: Failed to the error code property of task {0}. Exception:{1}", globalTaskId, e);
            }

            return await Task.FromResult(exitcode);
        }

        /// <summary>
        /// Dump the event log onto a target file
        /// </summary>
        /// <param name="targetFolder">indicating the target folder to put the dumped file</param>
        /// <param name="logName">indicating the log name</param>
        /// <returns>returns the dumped file name</returns>
        async Task<string> IHpcSchedulerAdapterInternal.DumpEventLog(string targetFolder, string logName)
        {
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] Dump event logs: TargetFolder = {0}, LogName = {1}", targetFolder, logName);

            string targetFileName = Path.Combine(
                targetFolder,
                String.Format(TargetFileNameFormat, Guid.NewGuid()));
            try
            {
                CheckBrokerAccess(0);
                using (EventLogSession session = new EventLogSession())
                {
                    session.ExportLogAndMessages(logName, PathType.LogName, "*", targetFileName);
                }

                return await Task.FromResult(targetFileName);
            }
            catch (Exception e)
            {
                TraceHelper.TraceError(0, "[HpcSchedulerDelegation] Failed to dump event logs. TargetFile = {0}, LogName = {1}, Exception = {2}", targetFileName, logName, e);

                try
                {
                    if (File.Exists(targetFileName))
                    {
                        File.Delete(targetFileName);
                    }
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceError(0, "[HpcSchedulerDelegation] Failed to cleanup temp file {0}: {1}", targetFileName, ex);
                }

                throw;
            }
        }

        /// <summary>
        /// Get all the non terminated session id and requeue count
        /// </summary>
        /// <returns>
        /// dictionary
        /// key: session Id
        /// value: requeue count
        /// </returns>
        async Task<Dictionary<int, int>> IHpcSchedulerAdapterInternal.GetNonTerminatedSession()
        {
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] GetNonTerminatedSession");

            this.CheckBrokerAccess(0);

            Dictionary<int, int> dic = new Dictionary<int, int>();

            try
            {
                JobState nonTerminatedState =
                    JobState.Configuring |
                    JobState.Submitted |
                    JobState.Validating |
                    JobState.ExternalValidation |
                    JobState.Queued |
                    JobState.Running;

                IFilterCollection fc = new FilterCollection();
                fc.Add(FilterOperator.HasBitSet, JobPropertyIds.RuntimeType, JobRuntimeType.SOA);
                fc.Add(FilterOperator.HasBitSet, JobPropertyIds.State, nonTerminatedState);

                IPropertyIdCollection pc = new PropertyIdCollection();
                pc.Add(JobPropertyIds.Id);
                pc.Add(JobPropertyIds.RequeueCount);

                using (ISchedulerRowSet rowset = this.scheduler.OpenJobRowSet(pc, fc, null))
                {
                    int count = rowset.GetCount();

                    PropertyRow[] rows = rowset.GetRows(0, count - 1).Rows;

                    if (rows != null)
                    {
                        foreach (PropertyRow row in rows)
                        {
                            int sessionId = (int)row[JobPropertyIds.Id].Value;

                            int requeueCount = (int)row[JobPropertyIds.RequeueCount].Value;

                            dic[sessionId] = requeueCount;
                        }
                    }
                }

                return await Task.FromResult(dic);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[HpcSchedulerDelegation] Failed to GetNonTerminatedSession {0}", e);
                throw;
            }
        }

        /// <summary>
        /// Get specified job requeue count
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>requeue count</returns>
        async Task<int> IHpcSchedulerAdapterInternal.GetJobRequeueCount(int jobid)
        {
            ParamCheckUtility.ThrowIfOutofRange(jobid <= 0, "jobId");
            TraceHelper.TraceEvent(TraceEventType.Information, "[HpcSchedulerDelegation] GetJobRequeueCount of job {0}.", jobid);
            this.CheckBrokerAccess(jobid);

            try
            {
                return await Task.FromResult(this.scheduler.OpenJob(jobid).RequeueCount);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "[HpcSchedulerDelegation] Failed to get requeue count of job {0}: {1}",
                    jobid,
                    e);

                throw;
            }
        }

        public async Task<bool> UpdateBrokerInfoAsync(int sessionId, Dictionary<string, object> properties)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the graceful preemption information.
        /// </summary>
        /// <param name="jobId">the job id</param>
        /// <param name="plannedCoreCount">the maximum count of cores that the job can have.</param>
        /// <param name="taskIds">a list of task ids of which the resources can be preempted.</param>
        /// <returns>returns a value indicating whether the operation succeeded</returns>
        async Task<(bool succeed, BalanceInfo balanceInfo, List<int> taskIds, List<int> runningTaskIds)> IHpcSchedulerAdapter.GetGracefulPreemptionInfo(int jobId)
        {
            bool succeed = false;
            BalanceInfo balanceInfo;
            List<int> taskIds = new List<int>();
            List<int> runningTaskIds = new List<int>();

            try
            {
                var job = this.scheduler.OpenJob(jobId);
                TraceHelper.TraceInfo(jobId, "[HpcSchedulerDelegation].GetGracefulPreemptionInfo: Job opened {0}", job != null);

                bool jobOnHold = job.HoldUntil > DateTime.UtcNow;

                if (job.GetBalanceRequest(out var balanceRequests))
                {
                    TraceHelper.TraceInfo(jobId, "[HpcSchedulerDelegation].GetGracefulPreemptionInfo: Job is using fast balancing mode. hold until: {0}", jobOnHold);
                    balanceInfo = BalanceInfoHpcFactory.FromBalanceRequests(balanceRequests);
                    if (jobOnHold)
                    {
                        TraceHelper.TraceInfo(jobId, "[HpcSchedulerDelegation].GetGracefulPreemptionInfo: Job is on hold until {0}. Set allowed core count in all request to 0.", job.HoldUntil);
                        foreach (var request in balanceInfo.BalanceRequests)
                        {
                            request.AllowedCoreCount = 0;
                        }
                    }

                    return (true, balanceInfo, taskIds, runningTaskIds);
                }
                else
                {
                    if (jobOnHold)
                    {
                        TraceHelper.TraceInfo(jobId, "[HpcSchedulerDelegation].GetGracefulPreemptionInfo: Job is on hold until {0}", job.HoldUntil);
                        balanceInfo = new BalanceInfo(0);
                    }
                    else
                    {
                        balanceInfo = new BalanceInfo(job.PlannedCoreCount);
                    }

                    FilterCollection filterRunning = new FilterCollection();
                    filterRunning.Add(new FilterProperty(FilterOperator.Equal, TaskPropertyIds.State, TaskState.Running));

                    var taskPropertyIds = new PropertyIdCollection();

                    taskPropertyIds.AddPropertyId(TaskPropertyIds.Id);
                    taskPropertyIds.AddPropertyId(TaskPropertyIds.ExitIfPossible);
                    using (var rowSet = job.OpenTaskRowSet(taskPropertyIds, filterRunning, null, true))
                    {
                        TraceHelper.TraceInfo(jobId, "[HpcSchedulerDelegation].GetGracefulPreemptionInfo: rowset opened {0}", rowSet != null);

                        var rows = rowSet.GetRows(0, rowSet.GetCount() - 1);
                        TraceHelper.TraceInfo(jobId, "[HpcSchedulerDelegation].GetGracefulPreemptionInfo: rows opened {0}, rows.Rows not null {1}", rows != null, rows.Rows != null);

                        if (rows.Rows != null)
                        {
                            foreach (var row in rows.Rows)
                            {
                                object value = row[TaskPropertyIds.ExitIfPossible].Value;
                                int id = (int)row[TaskPropertyIds.Id].Value;
                                if (!jobOnHold)
                                {
                                    if (value != null)
                                    {
                                        if ((bool)value)
                                        {
                                            TraceHelper.TraceInfo(jobId, "[HpcSchedulerDelegation].GetGracefulPreemptionInfo: exit if possible is true for task {0}. Added to the list.", id);

                                            taskIds.Add(id);
                                        }
                                        else
                                        {
                                            runningTaskIds.Add(id);
                                        }
                                    }
                                    else
                                    {
                                        TraceHelper.TraceInfo(jobId, "[HpcSchedulerDelegation].GetGracefulPreemptionInfo: exit if possible is null for task {0}. Exiting with false return value.", id);
                                        return (false, balanceInfo, taskIds, runningTaskIds);
                                    }
                                }
                                else
                                {
                                    TraceHelper.TraceInfo(jobId, "[HpcSchedulerDelegation].GetGracefulPreemptionInfo: job on hold for task {0}. Added to the list.", id);
                                    taskIds.Add(id);
                                }
                            }
                        }
                    }

                    succeed = true;
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(jobId, "[SchedulerHelper] GetGracefulPreemptionInfo: {0}", ex);

                balanceInfo = new BalanceInfo(0);
                succeed = false;
            }

            return (succeed, balanceInfo, taskIds, runningTaskIds);
        }

#endregion

        /// <summary>
        /// Gets job owner sid
        /// </summary>
        /// <param name="jobid">indicating the job id</param>
        /// <returns>returns sid of job owner</returns>
        private string GetJobOwnerSIDInternal(int jobid)
        {
            try
            {
                IFilterCollection filter = new FilterCollection();
                filter.Add(FilterOperator.Equal, JobPropertyIds.Id, jobid);

                IPropertyIdCollection property = new PropertyIdCollection();
                property.AddPropertyId(JobPropertyIds.OwnerSID);

                using (ISchedulerRowSet set = this.scheduler.OpenJobRowSet(property, filter, null))
                {
                    PropertyRow[] rows = set.GetRows(0, set.GetCount() - 1).Rows;
                    Debug.Assert(rows != null && rows.Length > 0 && rows[0].Props.Length > 0);
                    return rows[0].Props[0].Value as string;
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SchedulerHelper] GetUserSID: {0}", ex);
                return null;
            }
        }

        /// <summary>
        /// Callback to finish service job
        /// </summary>
        /// <param name="obj">indicating the job object</param>
        private void CallbackToFinishServiceJob(object obj)
        {
            ISchedulerJob job = (ISchedulerJob)obj;
            Debug.Assert(job != null);
            int jobId = job.Id;
            TraceHelper.TraceEvent(jobId, TraceEventType.Information, "[HpcSchedulerDelegation] Callback to finish service job {0}.", jobId);

            int retry = 3;
            while (retry > 0)
            {
                try
                {
                    if (job.State == JobState.Canceled || job.State == JobState.Finished || job.State == JobState.Finishing || job.State == JobState.Failed)
                    {
                        TraceHelper.TraceEvent(jobId, TraceEventType.Warning, "[HpcSchedulerDelegation] Cannot finish service job because service job is already in {0} state.", job.State);
                        break;
                    }

                    //todo: (qingzhi) can't specify message
                    job.Finish();
                    NotifyJobFinished(job);
                    TraceHelper.TraceEvent(jobId, TraceEventType.Information, "[HpcSchedulerDelegation] Successfully finished service job.");
                    break;
                }
                catch (SchedulerException ex)
                {
                    TraceHelper.TraceEvent(jobId, TraceEventType.Error, "[HpcSchedulerDelegation] Failed to finish service job: {0}", ex);
                }
                finally
                {
                    retry--;
                }

                // Sleep for a period of time before retrying
                Thread.Sleep(FinishServiceJobPeriod);
            }

            lock (this.JobMonitors)
            {
                this.finishingJobs.Remove(jobId);
            }
        }

        /// <summary>
        /// Call back to requeue service job in steps
        /// </summary>
        /// <param name="sender">the RetryTimer</param>
        /// <param name="e">the ElapsedEventArgs</param>
        private void RequeueServiceJobSteps(object sender, System.Timers.ElapsedEventArgs e)
        {
            RetryTimer t = (RetryTimer)sender;
            t.Stop();
            int jobId = t.JobId;
            t.Count++;
            TraceHelper.TraceEvent(jobId, TraceEventType.Information, "[HpcSchedulerDelegation] Enter RequeueServiceJobSteps for job {0} step {1} try count {2} at {3}.", jobId, t.CurrentAction + 1, t.Count, e.SignalTime);
            try
            {
                t.ActionList[t.CurrentAction].Invoke();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(jobId, TraceEventType.Error, "[HpcSchedulerDelegation] Failed to requeue service job: {0}", ex);
                try
                {
                    //reset the timer to retry this step
                    if (t.Count < t.TotalCount)
                    {
                        t.Interval = Math.Pow(2, t.Count - 1) * 1000;
                        t.Start();
                    }
                    else
                    {
                        TraceHelper.TraceEvent(jobId, TraceEventType.Error, "[HpcSchedulerDelegation] Failed to requeue service job after retries.");
                    }
                }
                catch (Exception exc)
                {
                    TraceHelper.TraceEvent(jobId, TraceEventType.Error, "[HpcSchedulerDelegation] Failed to reset the timer: {0}", exc);
                }
                return;
            }

            try
            {
                t.CurrentAction++;
                if (t.CurrentAction < t.ActionList.Count)
                {
                    //reset the timer for the next step
                    t.Interval = 1;
                    t.Count = 0;
                    t.Start();
                }
                else
                {
                    //dispose and remove the timer
                    t.Dispose();
                    RetryTimer timer;
                    if (!requeueJobRetryTimers.TryRemove(jobId, out timer))
                    {
                        TraceHelper.TraceEvent(jobId, TraceEventType.Error, "[HpcSchedulerDelegation] Failed to remove the timer for job: {0}", jobId);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(jobId, TraceEventType.Error, "[HpcSchedulerDelegation] Failed to reset the timer for the next step or failed to stop and remove the timer: {0}", ex);
            }
        }

        /// Callback to fail service job
        /// </summary>
        /// <param name="obj">indicating the job object</param>
        private void CallbackToFailServiceJob(object state)
        {
            object[] objs = (object[])state;
            ISchedulerJob job = (ISchedulerJob)objs[0];
            string reason = (string)objs[1];

            FailJobInternal(this.scheduler, job, reason);
        }

        internal static void FailJob(IScheduler scheduler, ISchedulerJob job, string reason)
        {
            TraceHelper.TraceEvent(job.Id, TraceEventType.Information, "[HpcSchedulerDelegation] Fail job: reason = {0}", reason);
            FailJobInternal(scheduler, job, reason);
        }

        /// <summary>
        /// Fail a service job with the reason
        /// </summary>
        /// <param name="job">the service job</param>
        /// <param name="reason">the reason string</param>
        private void FailJob(ISchedulerJob job, string reason)
        {
            TraceHelper.TraceEvent(job.Id, TraceEventType.Information, "[HpcSchedulerDelegation] Fail job: reason = {0}", reason);
            ThreadPool.QueueUserWorkItem(this.CallbackToFailServiceJob, new object[] { job, reason });
        }

        /// <summary>
        /// Fail service job
        /// </summary>
        /// <param name="scheduler">the scheduler</param>
        /// <param name="job">the scheduler job</param>
        /// <param name="reason">fail reason</param>
        private static void FailJobInternal(IScheduler scheduler, ISchedulerJob job, string reason)
        {
            // To fail a running service job, cancel job's meta task. According to task state transition, 
            // cancelling a running task will cause the task to be in the failed state and for SOA sessions
            // the job should end up in the failed state.
            // Note 1: there is no FailJob interface available from scheduler.
            // Note 2: below logic is only for failing a running service job.  There is no way to fail a 
            // queued job - you can only cancel it.
            int jobId = job.Id;
            try
            {
                job.Refresh();

                // For running service job, cancel its service task;  For non-running service job, cancel job.
                // Note: when checking job state, if the job is changing to 'Running' state at the same time,
                // there is a race condition that will result in a running job being cancelled rather than being
                // failed.  We don't care much about it for now.
                // FIXME!
                switch (job.State)
                {
                    case JobState.Finished:
                    case JobState.Canceled:
                    case JobState.Failed:
                        TraceHelper.TraceEvent(jobId, TraceEventType.Error, "[HpcSchedulerDelegation] Cannot fail a service job in {0} state.", job.State);
                        break;
                    case JobState.Running:
                        // fail job
                        TaskId taskId = new TaskId();
                        taskId.JobTaskId = 1;
                        ISchedulerTask masterTask = job.OpenTask(taskId);
                        masterTask.ServiceConclude(/*cancelSubTasks =*/true);
                        break;
                    default:
                        // cancel job
                        TraceHelper.TraceEvent(jobId, TraceEventType.Warning, "[HpcSchedulerDelegation] Cannot fail a service job in {0} state. Cancel it.", job.State);
                        scheduler.CancelJob(jobId, string.Format("Failed.  Trying to fail a service job in {0} state with reason: {1}.", job.State, reason));
                        break;
                }

                NotifyJobFailedOrCanceled(job);
            }
            catch (SchedulerException e)
            {
                TraceHelper.TraceEvent(jobId, TraceEventType.Error, "[HpcSchedulerDelegation] Failed to fail service job: {0}", e);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(jobId, TraceEventType.Error, "[HpcSchedulerDelegation] Failed to fail service job: {0}", e);
            }
        }

        /// <summary>
        /// Update SOA related properties
        /// </summary>
        /// <param name="schedulerJob">indicating the service job</param>
        /// <param name="properties">indicating the properties</param>
        /// <param name="minUnits">indicating the min units</param>
        /// <param name="maxUnits">indicating the max units</param>
        private void UpdateSoaRelatedProperties(ISchedulerJob schedulerJob, Dictionary<string, object> properties, int minUnits, int maxUnits)
        {
            int retry = 3;
            int jobId = schedulerJob.Id;
            while (retry > 0)
            {
                foreach (KeyValuePair<string, object> pair in properties)
                {
                    TraceHelper.TraceEvent(jobId,
                        TraceEventType.Verbose,
                        "[HpcSchedulerDelegation] .UpdateSoaRelatedProperties: Job custom property {0}={1}",
                        pair.Key,
                        pair.Value);

                    if (SchedulerDelegationCommon.CustomizedPropertyNames.Contains(pair.Key))
                    {
                        if (pair.Value != null)
                        {
                            schedulerJob.SetCustomProperty(pair.Key, pair.Value.ToString());

                            TraceHelper.TraceEvent(jobId,
                                TraceEventType.Verbose,
                                "[HpcSchedulerDelegation] .UpdateSoaRelatedProperties: Call SetCustomProperty to set job custom property {0}={1}",
                                pair.Key,
                                pair.Value);
                        }
                    }
                    else
                    {
                        switch (pair.Key)
                        {
                            case "Progress":
                                schedulerJob.Progress = Convert.ToInt32(pair.Value);
                                break;

                            case "TargetResourceCount":
                                int targetResourceCount = Convert.ToInt32(pair.Value);
                                if (targetResourceCount < minUnits)
                                {
                                    targetResourceCount = minUnits;
                                }
                                else if (targetResourceCount > maxUnits)
                                {
                                    targetResourceCount = maxUnits;
                                }

                                schedulerJob.TargetResourceCount = targetResourceCount;
                                break;

                            default:
                                // update job environment variable
                                string envName;
                                if (SchedulerDelegationCommon.PropToEnvMapping.TryGetValue(pair.Key, out envName))
                                {
                                    if (pair.Value != null)
                                    {
                                        //schedulerJob.SetEnvironmentVariable(envName, pair.Value.ToString());
                                        schedulerJob.SetCustomProperty(envName, pair.Value.ToString());

                                        TraceHelper.TraceEvent(jobId,
                                            TraceEventType.Verbose,
                                            "[HpcSchedulerDelegation] .UpdateSoaRelatedProperties: Call SetCustomProperty to set job custom property {0}={1}",
                                            pair.Key,
                                            pair.Value);
                                    }

                                    break;
                                }
                                else
                                {
                                    throw new InvalidOperationException(SR.SchedulerDelegation_UnknowPropertyName);
                                }
                        }
                    }
                }

                try
                {
                    schedulerJob.Commit();
                    TraceHelper.TraceEvent(jobId, TraceEventType.Information, "[HpcSchedulerDelegation] .UpdateSoaRelatedProperties: Commit service job.");
                    break;
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(jobId, TraceEventType.Warning, "[HpcSchedulerDelegation] Failed to commit service job property change: {0}\nRetryCount = {1}", e, retry);
                }
                finally
                {
                    retry--;
                }
            }
        }


        /// <summary>
        /// Retrieve job's custom prop to see if they are set
        /// </summary>
        /// <param name="schedulerJob">targeted job</param>
        /// <param name="properties">expected prop</param>
        [Conditional("DEBUG")]
        private static void CheckSoaRelatedProperties(ISchedulerJob schedulerJob,
                                                      Dictionary<string, object> properties,
                                                      List<string> customizedPropertyNames,
                                                      Dictionary<string, string> propToEnvMapping)
        {
            int jobId = schedulerJob.Id;
            Debug.Assert(customizedPropertyNames != null);
            Debug.Assert(propToEnvMapping != null);

            INameValueCollection collection = schedulerJob.GetCustomProperties();

            foreach (KeyValuePair<string, object> pair in properties)
            {
                if (customizedPropertyNames.Contains(pair.Key))
                {
                    if (pair.Value != null)
                    {
                        if (!PropExist(collection, pair.Key))
                        {
                            TraceHelper.TraceEvent(jobId,
                                TraceEventType.Verbose,
                                "[HpcSchedulerDelegation] .CheckSoaRelatedProperties: Job custom property {0} is missed.",
                                pair.Key);

                            // don't break here, in order to log all the missed props
                        }
                        else
                        {
                            TraceHelper.TraceEvent(jobId,
                               TraceEventType.Verbose,
                               "[HpcSchedulerDelegation] .CheckSoaRelatedProperties: Job custom property {0} exists.",
                               pair.Key);
                        }
                    }
                }
                else
                {
                    switch (pair.Key)
                    {
                        case "Progress":
                        case "TargetResourceCount":
                            break;

                        default:
                            string envName;
                            if (propToEnvMapping.TryGetValue(pair.Key, out envName))
                            {
                                if (!PropExist(collection, pair.Key))
                                {
                                    TraceHelper.TraceEvent(jobId,
                                        TraceEventType.Verbose,
                                        "[HpcSchedulerDelegation] .CheckSoaRelatedProperties: Job custom property {0} is missed.",
                                        pair.Key);
                                }
                                else
                                {
                                    TraceHelper.TraceEvent(jobId,
                                       TraceEventType.Verbose,
                                       "[HpcSchedulerDelegation] .CheckSoaRelatedProperties: Job custom property {0} exists.",
                                       pair.Key);
                                }

                                break;
                            }
                            else
                            {
                                throw new InvalidOperationException(SR.SchedulerDelegation_UnknowPropertyName);
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Check if a job is a durable session job
        /// </summary>
        /// <param name="schedulerJob">target job</param>
        /// <returns>true if the job is a durable session job; false, otherwise</returns>
        internal static bool IsDurableSessionJob(ISchedulerJob schedulerJob)
        {
            Dictionary<string, string> dic = JobHelper.GetCustomizedProperties(schedulerJob, BrokerSettingsConstants.Durable);

            string strDurable;
            if (dic.TryGetValue(BrokerSettingsConstants.Durable, out strDurable))
            {
                bool durable;
                if (bool.TryParse(strDurable, out durable))
                {
                    return durable;
                }

                TraceHelper.TraceError(0, "[HpcSchedulerDelegation] .IsDurableSessionJob: failed to parse durable attribute, value = {0}", strDurable);
            }

            return false;
        }

        /// <summary>
        /// Job finished event handler
        /// </summary>
        public static event EventHandler OnJobFinished;

        /// <summary>
        /// Job failed/canceled event handler
        /// </summary>
        public static event EventHandler OnJobFailedOrCanceled;

        /// <summary>
        /// Notify that a session job is finished
        /// </summary>
        static private void NotifyJobFinished(ISchedulerJob schedulerJob)
        {
            if (OnJobFinished != null)
            {
                OnJobFinished(schedulerJob, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Notify that a session job is failed or canceled
        /// </summary>
        static private void NotifyJobFailedOrCanceled(ISchedulerJob schedulerJob)
        {
            if (OnJobFailedOrCanceled != null)
            {
                OnJobFailedOrCanceled(schedulerJob, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Check if the prop exists in the collection
        /// </summary>
        private static bool PropExist(INameValueCollection collection, string propName)
        {
            foreach (NameValue nv in collection)
            {
                if (nv.Name.Equals(propName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Authenticate incoming user, if the calling user is broker node's machine
        /// account, let it pass. Otherwise, check if the calling user is job owner.
        /// </summary>
        /// <param name="sessionId">
        /// indicating the session id of the crresponding job, speicify 0 to authenticate
        /// only broker node
        /// </param>
        private void CheckBrokerAccess(int sessionId)
        {
            if (OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.AuthenticationType.Equals("X509", StringComparison.OrdinalIgnoreCase))
            {
                IIdentity id = null;
                if (SoaHelper.CheckCertIdentity(OperationContext.Current, out id))
                {
                    return;
                }
                else
                {
                    throw new AuthenticationException(String.Format(CultureInfo.InvariantCulture, "Unauthorized certificate: {0}", id?.Name));
                }
            }

            WindowsIdentity identity = null;
            bool result = SoaHelper.CheckWindowsIdentity(OperationContext.Current, out identity);

            if (result && identity == null)
            {
                // this code path is for Azure cluster.
                return;
            }

            if (AuthenticationHelper.IsBrokerNode(identity, this.brokerNodesManager.IsBrokerNode))
            {
                return;
            }

            if (sessionId != 0)
            {
                if (identity != null && result)
                {
                    if (IsJobOwnerInternal(sessionId, identity))
                    {
                        return;
                    }
                }
            }

            string userName = identity == null ? "Anonymous" : identity.Name;
            TraceHelper.TraceError(0, "[HpcSchedulerDelegation] Unauthorized user: {0}", userName);
            throw new AuthenticationException(String.Format(CultureInfo.InvariantCulture, "Unauthorized user: {0}", userName));
        }

        /// <summary>
        /// Authenticate incoming user.
        /// If the calling user is cluster machine account, let it pass.
        /// </summary>
        private void CheckClusterAccess()
        {
            if (WcfChannelModule.IsX509Identity(ServiceSecurityContext.Current.PrimaryIdentity))
            {
                return;
            }

            WindowsIdentity identity = null;
            bool result = SoaHelper.CheckWindowsIdentity(OperationContext.Current, out identity);

            if (result && identity == null)
            {
                // this code path is for Azure cluster.
                return;
            }

            if (AuthenticationHelper.IsBrokerNode(identity, this.brokerNodesManager.IsBrokerNode))
            {
                return;
            }

            FilterCollection onPremiseNodeFilter = new FilterCollection();
            StoreProperty property = new StoreProperty(NodePropertyIds.Location, NodeLocation.OnPremise);
            onPremiseNodeFilter.Add(new FilterProperty(FilterOperator.Equal, property));

            ISchedulerCollection collection = this.scheduler.GetNodeList(onPremiseNodeFilter, null);
            if (AuthenticationHelper.IsClusterNode(identity, collection))
            {
                return;
            }

            string userName = identity == null ? "Anonymous" : identity.Name;
            TraceHelper.TraceError(0, "[HpcSchedulerDelegation] Unauthorized user: {0}", userName);
            throw new AuthenticationException(String.Format(CultureInfo.InvariantCulture, "Unauthorized user: {0}", userName));
        }

        /// <summary>
        /// Authenticate incoming user, if the calling user is broker node's machine
        /// account, or the HpcAdmin or the job owner.
        /// </summary>
        /// <param name="jobId">job Id</param>
        private void CheckBrokerOrAdminOrJobOwnerAccess(int jobId)
        {
            WindowsIdentity identity = null;
            bool result = SoaHelper.CheckWindowsIdentity(OperationContext.Current, out identity);

            if (result)
            {
                if (identity == null)
                {
                    // this code path is for Azure cluster.
                    // actually it won't happen, because SoaDiagSvc
                    // is not supported on Azure cluster.
                    return;
                }
                else
                {
                    if (AuthenticationHelper.IsBrokerNode(identity, this.brokerNodesManager.IsBrokerNode) ||
                        AuthenticationUtil.IsHpcAdmin(identity) ||
                        IsJobOwnerInternal(jobId, identity))
                    {
                        return;
                    }
                }
            }

            throw new AuthenticationException();
        }

        /// <summary>
        /// Check if the user is job owner.
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <param name="identity">user identity</param>
        /// <returns>is job owner or not</returns>
        private bool IsJobOwnerInternal(int jobId, WindowsIdentity identity)
        {
            Debug.Assert(jobId > 0, "IsJobOwnerInternal: The jobId parameter should be greater than 0.");
            Debug.Assert(identity != null, "IsJobOwnerInternal: The identity parameter should not be null.");

            string sid = this.GetJobOwnerSIDInternal(jobId);
            return new SecurityIdentifier(sid).Equals(identity.User);
        }

        /// <summary>
        /// Event triggered when job monitor entry's instance is exited
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event args</param>
        private void JobMonitorEntry_Exit(object sender, EventArgs e)
        {
            HpcPackJobMonitorEntry entry = (HpcPackJobMonitorEntry)sender;
            Debug.Assert(entry != null, "[HpcSchedulerDelegation] Sender should be an instance of JobMonitorEntry class.");

            // if a JobMonitorEntry is exited because of job state changed into Finished/Canceled/Failed
            if (entry.PreviousState == JobState.Finished)
            {
                NotifyJobFinished(entry.SchedulerJob);
            }
            else if (entry.PreviousState == JobState.Failed || entry.PreviousState == JobState.Canceled)
            {
                NotifyJobFailedOrCanceled(entry.SchedulerJob);
            }

            lock (this.JobMonitors)
            {
                this.JobMonitors.Remove(entry.SessionId);
            }

            entry.Exit -= new EventHandler(JobMonitorEntry_Exit);
            entry.Close();
        }

        /// <summary>
        /// Callback to monitor job state
        /// </summary>
        /// <param name="state">null object</param>
        private void JobStateMonitorCallback(object state)
        {
            if (Interlocked.Increment(ref this.monitorJobStateFlag) != 1)
            {
                return;
            }

            Dictionary<string, BrokerLauncherClient> brokerLauncherClientDic = GenerateBrokerLauncherClientDic();
            try
            {
                TraceHelper.TraceVerbose(0, "[HpcSchedulerDelegation] Callback to monitor job state.");

                List<JobInfo> jobInfoList = this.GetPreRunningServiceJobIntoList();
                List<JobInfo> runningServiceJobInfoList = this.GetRuningServiceJobIntoList();

                TraceRunningServiceJobInfoList(runningServiceJobInfoList);

                try
                {
                    // update the session pool maintained by session launcher
                    jobInfoList.AddRange(runningServiceJobInfoList);
                    this.launcher.UpdateSessionPool(jobInfoList);
                }
                catch (Exception e)
                {
                    TraceHelper.TraceError(0, "[HpcSchedulerDelegation] Exception thrown when updating session pool: {0}", e);
                }

                List<int> activeBrokerIdList = new List<int>();
                lock (this.JobMonitors)
                {
                    foreach (HpcPackJobMonitorEntry entry in this.JobMonitors.Values)
                    {
                        if (!entry.CheckIfBrokerIsUnavailable())
                        {
                            activeBrokerIdList.Add(entry.SessionId);
                        }
                    }
                }

                TraceActiveBrokerIdList(activeBrokerIdList);

                List<HpcPackJobMonitorEntry> activeJobMonitorEntryList;
                lock (this.JobMonitors)
                {
                    activeJobMonitorEntryList = new List<HpcPackJobMonitorEntry>(this.JobMonitors.Values);
                }

                TraceActiveJobMonitorEntryList(activeJobMonitorEntryList);

                // Find failed service job with registered JobMonitorEntry instance, ask the JobMonitorEntry instance to refresh the service job
                this.FindFailedServiceJobWithRegisteredJobMonitorEntry();

                // Find for sessions that has entry but no active broker
                // Remove the entry and cancel the job if needed
                foreach (HpcPackJobMonitorEntry activeEntry in activeJobMonitorEntryList)
                {
                    try
                    {
                        if (!activeBrokerIdList.Contains(activeEntry.SessionId))
                        {
                            TraceHelper.TraceInfo(activeEntry.SessionId, "[HpcSchedulerDelegation] Broker is missing.");
                            TraceHelper.TraceInfo(activeEntry.SessionId, "[HpcSchedulerDelegation] Remove job monitor entry because broker is missing.");
                            if (activeEntry.PreviousState != JobState.Canceled
                                && activeEntry.PreviousState != JobState.Canceling
                                && activeEntry.PreviousState != JobState.Failed
                                && activeEntry.PreviousState != JobState.Finished
                                && activeEntry.PreviousState != JobState.Finishing)
                            {
                                // Cancel service job and remove entry
                                this.FailJob(activeEntry.SchedulerJob, FailReason_RetryRunawayServiceJobFailed);
                            }

                            this.RemoveAndCloseJobEntry(activeEntry);

                            // Remove this session from runningServiceJobInfoList so that it won't be raised up as "requeued session"
                            runningServiceJobInfoList.RemoveAll(delegate (JobInfo info)
                            {
                                return info.Id == activeEntry.SessionId;
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceError(activeEntry.SessionId, "[HpcSchedulerDelegation] Exception thrown when trying to find runaway service job: {0}", e);
                    }
                }

                // Find for sessions that has running service job but no active broker
                foreach (JobInfo jobInfo in runningServiceJobInfoList)
                {
                    try
                    {
                        int sessionId = jobInfo.Id;
                        if (activeBrokerIdList.Contains(sessionId))
                        {
                            // Active broker, running service job, check if task state changed and GetTaskInfo if needed
                            HpcPackJobMonitorEntry entry;
                            bool flag = false;
                            lock (this.JobMonitors)
                            {
                                flag = this.JobMonitors.TryGetValue(sessionId, out entry);
                            }

                            if (flag)
                            {
                                if (!jobInfo.Equals(entry.PreviousJobInfo))
                                {
                                    // Task info changed, need to query task info and notify broker
                                    entry.CallbackToQueryTaskInfo(true);
                                }
                            }
                        }
                        else
                        {
                            lock (this.JobMonitors)
                            {
                                if (this.JobMonitors.ContainsKey(sessionId))
                                {
                                    continue;
                                }

                                // Bug 8361: Ignore finishing service jobs
                                if (this.finishingJobs.Contains(sessionId))
                                {
                                    continue;
                                }
                            }

                            TraceHelper.TraceInfo(sessionId, "[HpcSchedulerDelegation] Try to raise broker for runaway service job.");

                            ISchedulerJob job;
                            lock (this.scheduler)
                            {
                                job = this.scheduler.OpenJob(sessionId);
                            }

                            if (DateTime.UtcNow.Subtract(job.SubmitTime) <= MonitorJobStateTimePeriod)
                            {
                                // Do not try to do failover for job which was just submitted
                                // This is to avoid the race condition that when a service job is just
                                // submitted and HPC_BROKER property has not been updated before this
                                // logic failed it as "failed to initialize".
                                TraceHelper.TraceInfo(sessionId, "[HpcSchedulerDelegation] Skip raise broker as the service job is just submitted.");
                                continue;
                            }

                            try
                            {
                                string brokernode = null;
                                bool? isDurable = null;
                                foreach (NameValue pair in job.GetCustomProperties())
                                {
                                    if (pair.Name.Equals(BrokerSettingsConstants.BrokerNode, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        brokernode = pair.Value;
                                    }
                                    else if (pair.Name.Equals(BrokerSettingsConstants.Durable, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        isDurable = Convert.ToBoolean(pair.Value);
                                    }
                                }

                                if (brokernode == null)
                                {
                                    TraceHelper.TraceWarning(sessionId, "[HpcSchedulerDelegation] Cannot find broker node from service job.");
                                    this.FailJob(job, FailReason_RunawayServiceJob);
                                    continue;
                                }

                                TraceHelper.TraceInfo(sessionId, "[HpcSchedulerDelegation] Got Broker node: {0}", brokernode);
                                BrokerLauncherClient client;
                                if (brokerLauncherClientDic.TryGetValue(brokernode, out client))
                                {
                                    client.Attach(sessionId);
                                    TraceHelper.TraceInfo(sessionId, "[HpcSchedulerDelegation] Successfully raised up broker instance.");
                                }
                                else
                                {
                                    TraceHelper.TraceWarning(sessionId, "[HpcSchedulerDelegation] Failed to find broker launcher client for broker node: {0}.", brokernode);
                                    this.FailJob(job, FailReason_BrokerLauncherNotFound);
                                }
                            }
                            catch (Exception e)
                            {
                                TraceHelper.TraceWarning(sessionId, "[HpcSchedulerDelegation] Failed to raise up broker: {0}", e);
                                this.FailJob(job, FailReason_BrokerNotExist);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceError(jobInfo.Id, "[HpcSchedulerDelegation] Exception thrown when trying to find requeued service job: {0}", e);
                    }
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceError(0, "[HpcSchedulerDelegation] Exception thrown when querying runaway service jobs: {0}", e);
            }
            finally
            {
                if (brokerLauncherClientDic != null)
                {
                    foreach (BrokerLauncherClient client in brokerLauncherClientDic.Values)
                    {
                        if (client != null)
                        {
                            Microsoft.Hpc.ServiceBroker.Common.Utility.AsyncCloseICommunicationObject(client);
                        }
                    }
                }

                this.monitorJobStateFlag = 0;
            }
        }

        /// <summary>
        /// Print the content of activeJobMonitorEntryList into trace
        /// </summary>
        /// <param name="activeJobMonitorEntryList">indicating the instance of activeJobMonitorEntryList</param>
        [Conditional("TRACE")]
        private static void TraceActiveJobMonitorEntryList(List<HpcPackJobMonitorEntry> activeJobMonitorEntryList)
        {
            string[] stringArray = new string[activeJobMonitorEntryList.Count];
            for (int i = 0; i < activeJobMonitorEntryList.Count; i++)
            {
                stringArray[i] = activeJobMonitorEntryList[i].SessionId.ToString();
            }

            TraceHelper.TraceVerbose(0, "[HpcSchedulerDelegation] Active job monitor entries: {0}", String.Join(",", stringArray));
        }

        /// <summary>
        /// Print the content of activeBrokerIdList into trace
        /// </summary>
        /// <param name="activeBrokerIdList">indicating the instance of activeBrokerIdList</param>
        [Conditional("TRACE")]
        private static void TraceActiveBrokerIdList(List<int> activeBrokerIdList)
        {
            string[] stringArray = new string[activeBrokerIdList.Count];
            for (int i = 0; i < activeBrokerIdList.Count; i++)
            {
                stringArray[i] = activeBrokerIdList[i].ToString();
            }

            TraceHelper.TraceVerbose(0, "[HpcSchedulerDelegation] Active brokers: {0}", String.Join(",", stringArray));
        }

        /// <summary>
        /// Print the content of runningServiceJobInfoList into trace
        /// </summary>
        /// <param name="activeJobMonitorEntryList">indicating the instance of runningServiceJobInfoList</param>
        [Conditional("TRACE")]
        private static void TraceRunningServiceJobInfoList(List<JobInfo> runningServiceJobInfoList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (JobInfo info in runningServiceJobInfoList)
            {
                sb.AppendLine(info.ToString());
            }

            TraceHelper.TraceVerbose(0, "[HpcSchedulerDelegation] Running service job info list:\n{0}", sb.ToString());
        }

        /// <summary>
        /// Get the list of jobs in "PreRunning" state: JobState.Configuring, JobState.Submitted,
        /// JobState.Validating, JobState.ExternalValidation, JobState.Queued
        /// </summary>
        /// <returns>job info list</returns>
        private List<JobInfo> GetPreRunningServiceJobIntoList()
        {
            List<JobInfo> list;

            FilterCollection fc = new FilterCollection();
            fc.Add(FilterOperator.IsNotNull, JobPropertyIds.ServiceName, null);
            fc.Add(FilterOperator.HasBitSet,
                JobPropertyIds.State,
                JobState.Configuring | JobState.Submitted | JobState.Validating | JobState.ExternalValidation | JobState.Queued);

            PropertyIdCollection pc = new PropertyIdCollection();
            pc.Add(JobPropertyIds.Id);

            using (ISchedulerRowSet rowSet = this.scheduler.OpenJobRowSet(pc, fc, null))
            {
                int count = rowSet.GetCount();

                TraceHelper.TraceVerbose(0, "[HpcSchedulerDelegation] Get {0} pre-running service jobs.", count);

                list = new List<JobInfo>(count);

                PropertyRow[] rows = rowSet.GetRows(0, count - 1).Rows;
                if (rows != null)
                {
                    foreach (PropertyRow row in rows)
                    {
                        list.Add(new JobInfo((int)row[JobPropertyIds.Id].Value, 0, 0, 0, 0, 0));
                    }
                }

                return list;
            }
        }

        /// <summary>
        /// Find failed service job with registered JobMonitorEntry instance and ask them to refresh ISchedulerJob instance
        /// </summary>
        private void FindFailedServiceJobWithRegisteredJobMonitorEntry()
        {
            List<int> registeredSessionIdList;
            lock (this.JobMonitors)
            {
                registeredSessionIdList = new List<int>(this.JobMonitors.Keys);
            }

            object[] registeredSessionIdListForFilter = new object[registeredSessionIdList.Count];
            for (int i = 0; i < registeredSessionIdList.Count; i++)
            {
                registeredSessionIdListForFilter[i] = registeredSessionIdList[i];
            }

            FilterCollection fc = new FilterCollection();
            fc.Add(FilterOperator.HasBitSet, JobPropertyIds.State, JobState.Canceled | JobState.Failed);
            fc.Add(FilterOperator.IsNotNull, JobPropertyIds.ServiceName, null);
            fc.Add(FilterOperator.In, JobPropertyIds.Id, registeredSessionIdListForFilter);
            PropertyIdCollection pc = new PropertyIdCollection();
            pc.Add(JobPropertyIds.Id);
            using (ISchedulerRowSet rowSet = this.scheduler.OpenJobRowSet(pc, fc, null))
            {
                int count = rowSet.GetCount();
                TraceHelper.TraceVerbose(0, "[HpcSchedulerDelegation] Get {0} failed/canceled service jobs.", count);

                PropertyRow[] rows = rowSet.GetRows(0, count - 1).Rows;
                if (rows != null)
                {
                    foreach (PropertyRow row in rows)
                    {
                        int sessionId = (int)row[JobPropertyIds.Id].Value;
                        bool flag;
                        HpcPackJobMonitorEntry entry;
                        lock (this.JobMonitors)
                        {
                            flag = this.JobMonitors.TryGetValue(sessionId, out entry);
                        }

                        if (flag)
                        {
                            // Ask the entry to refresh the job and send state change event to broker
                            // The entry will close itself then
                            entry.CallbackToQueryTaskInfo(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate broker launcher client dic
        /// </summary>
        /// <returns>returns broker launcher client dic</returns>
        private Dictionary<string, BrokerLauncherClient> GenerateBrokerLauncherClientDic()
        {
            // Get broker eprs and broker nodes, indicating durable to false to get all broker nodes (including HA and non-HA broker nodes)
            Dictionary<string, BrokerLauncherClient> brokerLauncherClientDic = new Dictionary<string, BrokerLauncherClient>();
            List<NodeInfo> brokerNodes;
            List<string> brokerEprList = new List<string>(this.GetAvailableBrokerEPRs(false, BrokerLauncherEndpointPrefix, out brokerNodes));
            Debug.Assert(brokerNodes.Count == brokerEprList.Count, "[HpcSchedulerDelegation] Broker node list should have the same count as broker epr list.");
            // Get internal cert thumbprint
            string certThumbprint = HpcContext.Get().GetSSLThumbprint().GetAwaiter().GetResult();
            for (int i = 0; i < brokerEprList.Count; i++)
            {
                BrokerLauncherClient client = new BrokerLauncherClient(new Uri(brokerEprList[i]), certThumbprint);
                brokerLauncherClientDic.Add(brokerNodes[i].Name, client);
            }

            return brokerLauncherClientDic;
        }

        /// <summary>
        /// Gets running service job list
        /// </summary>
        /// <returns>returns running service job list</returns>
        private List<JobInfo> GetRuningServiceJobIntoList()
        {
            List<JobInfo> runningServiceJobInfoList;
            FilterCollection fc = new FilterCollection();
            fc.Add(FilterOperator.Equal, JobPropertyIds.State, JobState.Running);
            fc.Add(FilterOperator.IsNotNull, JobPropertyIds.ServiceName, null);
            PropertyIdCollection pc = new PropertyIdCollection();
            pc.Add(JobPropertyIds.Id);
            pc.Add(JobPropertyIds.RunningTaskCount);
            pc.Add(JobPropertyIds.FailedTaskCount);
            pc.Add(JobPropertyIds.CanceledTaskCount);
            pc.Add(JobPropertyIds.FinishedTaskCount);
            pc.Add(JobPropertyIds.TaskCount);
            using (ISchedulerRowSet rowSet = this.scheduler.OpenJobRowSet(pc, fc, null))
            {
                int count = rowSet.GetCount();
                TraceHelper.TraceVerbose(0, "[HpcSchedulerDelegation] Get {0} running service jobs.", count);
                runningServiceJobInfoList = new List<JobInfo>(count);

                PropertyRow[] rows = rowSet.GetRows(0, count - 1).Rows;
                if (rows != null)
                {
                    foreach (PropertyRow row in rows)
                    {
                        runningServiceJobInfoList.Add(
                            new JobInfo(
                                (int)row[JobPropertyIds.Id].Value,
                                (int)row[JobPropertyIds.RunningTaskCount].Value,
                                (int)row[JobPropertyIds.FailedTaskCount].Value,
                                (int)row[JobPropertyIds.CanceledTaskCount].Value,
                                (int)row[JobPropertyIds.FinishedTaskCount].Value,
                                (int)row[JobPropertyIds.TaskCount].Value));
                    }
                }

                return runningServiceJobInfoList;
            }
        }

        private void RemoveAndCloseJobEntry(HpcPackJobMonitorEntry activeEntry)
        {
            lock (this.JobMonitors)
            {
                this.JobMonitors.Remove(activeEntry.SessionId);
            }
            activeEntry.Exit -= new EventHandler(JobMonitorEntry_Exit);
            activeEntry.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.jobStateMonitorTimer != null)
                {
                    this.jobStateMonitorTimer.Dispose();
                    this.jobStateMonitorTimer = null;
                }

                if (this.requeueJobRetryTimers != null)
                {
                    foreach (var timer in this.requeueJobRetryTimers)
                    {
                        if (timer.Value != null)
                        {
                            timer.Value.Stop();
                            timer.Value.Dispose();
                        }
                    }
                    this.requeueJobRetryTimers.Clear();
                    this.requeueJobRetryTimers = null;
                }

                if (this.JobMonitors != null)
                {
                    List<HpcPackJobMonitorEntry> jobMonitorEntryList;
                    lock (this.JobMonitors)
                    {
                        jobMonitorEntryList = new List<HpcPackJobMonitorEntry>(this.JobMonitors.Values);
                        this.JobMonitors.Clear();
                    }

                    foreach (var monitor in jobMonitorEntryList)
                    {
                        monitor.Exit -= new EventHandler(JobMonitorEntry_Exit);
                        monitor.Close();
                    }

                    //clear up the static node info cache
                    HpcPackJobMonitorEntry.ClearNodeCache();
                }

                if (this.scheduler != null)
                {
                    this.scheduler.Close();
                    this.scheduler.Dispose();
                }
                this.scheduler = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the available broker eprs.
        /// </summary>
        private string[] GetAvailableBrokerEPRs(bool durable, string endpointPrefix, out List<NodeInfo> nodesOnly)
        {
            return this.brokerNodesManager.GetAvailableBrokerEPRs(durable, endpointPrefix, false, ChannelTypes.Certificate, TransportScheme.NetTcp, out nodesOnly, false);
        }
    }
}

#endif