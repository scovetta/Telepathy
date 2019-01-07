//-----------------------------------------------------------------------
// <copyright file="IServiceJobMonitor.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Internal Monitor service job</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Hpc.Scheduler;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.ServiceBroker.BackEnd;
    using Microsoft.Hpc.ServiceBroker.Common;
    using Microsoft.Hpc.Scheduler.Session.HpcPack.DataMapping;

    using SoaAmbientConfig;
    using Microsoft.Hpc.ServiceBroker.Common.ThreadHelper;

    using SR = Microsoft.Hpc.SvcBroker.SR;

    /// <summary>
    /// Internal Monitor the service job
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal abstract class InternalServiceJobMonitor : ReferenceObject, ISchedulerNotify
    {
        /// <summary>
        /// The first time the update status is called.
        /// </summary>
        protected const int FirstUpdateStatusSeconds = 3;

        /// <summary>
        /// Stores the max retry count
        /// </summary>
        protected const int MaxRetryCount = 3;

        /// <summary>
        /// Stores the timeout period of event from scheduler delegation
        /// </summary>
        protected const int TimeoutPeriodFromSchedulerDelegationEvent = 15 * 60 * 1000;

        /// <summary>
        /// Check tasks to finish every 10 seconds.
        /// </summary>
        protected const int FinishTasksInterval = 10 * 1000;

        /// <summary>
        /// The maximum retry count of finish task operation.
        /// </summary>
        protected const int MaxFinishTaskRetryCount = 3;

        /// <summary>
        /// Stores the max units
        /// </summary>
        protected int maxUnits;

        /// <summary>
        /// Stores the min units
        /// </summary>
        protected int minUnits;

        /// <summary>
        /// Stores a value indicating whether the service job is finished
        /// </summary>
        protected bool isFinished;

        /// <summary>
        /// Stores the dispatcher manager
        /// </summary>
        protected DispatcherManager dispatcherManager;

        /// <summary>
        /// Stores the broker observer
        /// </summary>
        protected BrokerObserver observer;

        /// <summary>
        /// Stores the trigger
        /// </summary>
        protected RepeatableCallbackTrigger trigger;

        /// <summary>
        /// Stores the state locker
        /// </summary>
        protected object lockState = new object();

        /// <summary>
        /// Stores the service job state
        /// </summary>
        protected ServiceJobState state;

        /// <summary>
        /// Stores the timeout manager
        /// </summary>
        protected TimeoutManager timeoutManager;

        /// <summary>
        /// Stores the scheduler notify timeout manager
        /// </summary>
        protected TimeoutManager schedulerNotifyTimeoutManager;

        /// <summary>
        /// Stores the shared data
        /// </summary>
        protected SharedData sharedData;

        /// <summary>
        /// Callback for timeout to finish service job
        /// </summary>
        protected WaitCallback timeoutToFinishServiceJobCallback;

        /// <summary>
        /// Number of  llocation adjustment intervals that have occured where the target resource count shrunk consecutively
        /// </summary>
        protected int consecutiveShrinkIntervals;

        /// <summary>
        /// Stores the callback to call ServiceFailed
        /// </summary>
        protected WaitCallback serviceFailedCallback;

        /// <summary>
        /// Stores the check job healthy callback
        /// </summary>
        protected WaitCallback checkJobHealthyCallback;

        /// <summary>
        /// Stores the state manager
        /// </summary>
        protected BrokerStateManager stateManager;

        /// <summary>
        /// Stores a copy of the job's blacklist.
        /// </summary>
        protected StringCollection remoteBlacklistCopy = new StringCollection();

        /// <summary>
        /// Stores lock object for remoteBlacklistCopy
        /// </summary>
        protected object lockRemoteBlacklistCopy = new object();

        /// <summary>
        /// Stores lock object to protect client proxy
        /// </summary>
        //protected object lockClient = new object(); // though client proxy is not thread safe, try it out

        /// <summary>
        /// Stores a value indicating whether it is requeueing job
        /// </summary>
        protected int isRequeuingJob;

        /// <summary>
        /// Whether cluster param AutomaticShrinkEnabled is false
        /// </summary>
        protected bool automaticShrinkEnabled = Constant.AutomaticShrinkEnabledDefault;

        /// <summary>
        /// Store the network prefix
        /// </summary>
        protected string networkPrefix;

        /// <summary>
        /// Stores the utc time when last called RequeueOrFailJob()
        /// </summary>
        protected DateTime lastRequeueTime = DateTime.MinValue;

        /// <summary>
        /// Stores the scheduler adapter client factory
        /// </summary>
        protected SchedulerAdapterClientFactory schedulerAdapterClientFactory;

        /// <summary>
        /// Stores the node mapping
        /// </summary>
        protected NodeMappingData nodeMappingData;

        /// <summary>
        /// The graceful preemption handler.
        /// </summary>
        protected GracefulPreemptionHandler gracefulPreemptionHandler;

        /// <summary>
        /// The last adjust time.
        /// </summary>
        protected DateTime lastFullAdjustTime;

        /// <summary>
        /// The allocation adjust thread.
        /// </summary>
        protected Thread allocationAdjustThread;

        /// <summary>
        /// The threads to new dispatcher semaphore.
        /// </summary>
        protected Semaphore newDispatcherThreadCount = new Semaphore(1000, 1000);

        /// <summary>
        /// The tasks to be cancelled;
        /// </summary>
        protected ConcurrentDictionary<int, int> tasksToBeCancelled = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Stores the fabric cluster context
        /// </summary>
        protected IHpcContext context;

        /// <summary>
        /// Initializes a new instance of the ServiceJobMonitor class
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="stateManager">indicating the state manager</param>
        public InternalServiceJobMonitor(SharedData sharedData, BrokerStateManager stateManager, NodeMappingData nodeMappingData, IHpcContext context)
        {
            this.context = context;
            this.sharedData = sharedData;
            this.stateManager = stateManager;
            this.nodeMappingData = nodeMappingData;
            this.NeedAdjustAllocation = new AutoResetEvent(false);
            this.lastFullAdjustTime = DateTime.Now;
            this.allocationAdjustThread = new Thread(this.AllocationAdjust);

            this.timeoutManager = new TimeoutManager("ServiceJobMonitor");
            this.schedulerNotifyTimeoutManager = new TimeoutManager("SchedulerNotifyTimeoutManager");
            this.timeoutToFinishServiceJobCallback = new BasicCallbackReferencedThreadHelper<object>(this.TimeoutToFinishServiceJob, this).CallbackRoot;
            this.serviceFailedCallback = new ThreadHelper<object>(new WaitCallback(this.ServiceFailed)).CallbackRoot;
            this.checkJobHealthyCallback = new ThreadHelper<object>(new WaitCallback(this.CheckJobHealthyThreadProc)).CallbackRoot;

            if (sharedData.StartInfo.UseInprocessBroker)
            {
                // inproc broker does not use network prefix for enterprise network
                if (string.Equals(this.networkPrefix, Constant.EnterpriseNetwork, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.networkPrefix = string.Empty;
                }
            }
            else
            {
                this.networkPrefix = this.sharedData.BrokerInfo.NetworkPrefix;
            }

            this.state = ServiceJobState.Started;
            this.timeoutManager.RegisterTimeout(sharedData.Config.Monitor.ClientConnectionTimeout, this.timeoutToFinishServiceJobCallback, null);
            BrokerTracing.TraceInfo("[ServiceJobMonitor] Service state: Started.");
        }

        /// <summary>
        /// Gets or sets the dispatcher idle event.
        /// </summary>
        public AutoResetEvent NeedAdjustAllocation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the scheduler adapter factory
        /// </summary>
        public SchedulerAdapterClientFactory SchedulerAdapterFactory
        {
            get
            {
                return this.schedulerAdapterClientFactory;
            }
        }

        /// <summary>
        /// Gets the service job state
        /// </summary>
        public ServiceJobState ServiceJobState
        {
            get
            {
                return this.state;
            }
        }

        /// <summary>
        /// Update suspended flag
        /// </summary>
        /// <param name="suspended">indicating the suspended flag</param>
        public async Task UpdateSuspended(bool suspended)
        {
            try
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add(BrokerSettingsConstants.Suspended, suspended);

                RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                await RetryHelper<object>.InvokeOperationAsync(
                        async () =>
                        {
                            await (await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync()).UpdateBrokerInfoAsync(this.sharedData.BrokerInfo.SessionId, props);
                            return null;
                        },
                        async (e, r) =>
                        {
                            await Task.FromResult<object>(new Func<object>(() =>
                            {
                                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while updating suspended flag: {0} with retry: {1}", e, r.RetryCount); return null;
                            }).Invoke());
                        }, retry);

            }
            catch (Exception ex)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while updating suspended flag: {0}", ex);
            }
        }

        /// <summary>
        /// Finish the service job
        /// </summary>
        /// <param name="reason">indicating the reason</param>
        public async Task FinishServiceJob(string reason)
        {
            if (!this.isFinished)
            {
                lock (this)
                {
                    if (!this.isFinished)
                    {
                        this.isFinished = true;
                        this.NeedAdjustAllocation.Set();
                    }
                    else
                    {
                        this.sharedData.WaitForJobFinish();
                        return;
                    }
                }

                // we should do others out of lock
                try
                {
                    // Update job properties to make sure it is right
                    this.UpdateStatus(null);

                    RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                    await RetryHelper<object>.InvokeOperationAsync(
                            async () =>
                            {
                                await (await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync()).FinishJobAsync(this.sharedData.BrokerInfo.SessionId, reason);
                                return null;
                            },
                            async (e, r) =>
                            {
                                await Task.FromResult<object>(new Func<object>(() => { BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while finishing the job: {0} with retry: {1}", e, r.RetryCount); return null; }).Invoke());
                            }, retry);

                    BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[ServiceJobMonitor] Successfully finished service job.");
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while finishing the job: {0}", e);
                }
                finally
                {
                    this.sharedData.JobFinished();
                }
            }
            else
            {
                this.sharedData.WaitForJobFinish();
            }
        }

        /// <summary>
        /// Fail the service job
        /// </summary>
        /// <param name="reason">indicating the reason</param>
        public async Task FailServiceJob(string reason)
        {
            try
            {
                RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                await RetryHelper<object>.InvokeOperationAsync(
                        async () =>
                        {
                            await (await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync()).FailJobAsync(this.sharedData.BrokerInfo.SessionId, reason);
                            return null;
                        },
                        async (e, r) =>
                        {
                            await Task.FromResult<object>(new Func<object>(() => { BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while failing the job: {0} with retry: {1}", e, r.RetryCount); return null; }).Invoke());
                        }, retry);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while failing the job: {0}", e);
            }
        }

        /// <summary>
        /// Requeue or fail the service job
        /// </summary>
        /// <param name="reason">indicating the reason</param>
        protected async Task RequeueOrFailServiceJob(string reason)
        {
            bool isRequeuing = (1 == Interlocked.CompareExchange(ref this.isRequeuingJob, 1, 0));
            if (isRequeuing)
            {
                return;
            }

            //lock (this.lockClient)
            //{
            try
            {
                if (DateTime.UtcNow.Subtract(this.lastRequeueTime) > TimeSpan.FromMilliseconds(this.sharedData.Config.LoadBalancing.EndpointNotFoundRetryPeriod))
                {
                    this.lastRequeueTime = DateTime.UtcNow;

                    RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                    await RetryHelper<object>.InvokeOperationAsync(
                            async () =>
                            {
                                await (await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync()).RequeueOrFailJobAsync(this.sharedData.BrokerInfo.SessionId, reason);
                                return null;
                            },
                            async (e, r) =>
                            {
                                await Task.FromResult<object>(new Func<object>(() => { BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while updating suspended flag: {0} with retry: {1}", e, r.RetryCount); return null; }).Invoke());
                            }, retry);

                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while requeuing the job: {0}", e);
            }
            //}

            Interlocked.Exchange(ref this.isRequeuingJob, 0);
        }

        /// <summary>
        /// Blacklist a node
        /// </summary>
        /// <param name="nodeName">name of the node to be blacklisted</param>
        /// <returns>true if blacklist success, false otherwise</returns>
        public async Task BlacklistNode(string nodeName)
        {
            lock (this.lockRemoteBlacklistCopy)
            {
                // if the node is already blacklisted, do nothing
                if (this.remoteBlacklistCopy.Contains(nodeName))
                {
                    return;
                }
                else
                {
                    this.remoteBlacklistCopy.Add(nodeName);
                }
            }

            bool excludeNodeSuccess = false;
            try
            {
                RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                excludeNodeSuccess = await RetryHelper<bool>.InvokeOperationAsync(
                        async () => await (await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync()).ExcludeNodeAsync(this.sharedData.BrokerInfo.SessionId, nodeName),
                        async (e, r) => await Task.FromResult<object>(new Func<object>(() => { BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Blacklist node={0} caught exception: {1} with retry: {2}", nodeName, e, r.RetryCount); return null; }).Invoke()),
                        retry);

                if (excludeNodeSuccess)
                {
                    BrokerTracing.TraceError("[ServiceJobMonitor] Node {0} blacklisted.", nodeName);
                }
                else
                {
                    BrokerTracing.TraceError("[ServiceJobMonitor] Failed to blacklist node {0}", nodeName);
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[ServiceJobMonitor] Blacklist node={0} caught exception : {1}.", nodeName, e);
            }

            if (!excludeNodeSuccess)
            {
                lock (this.lockRemoteBlacklistCopy)
                {
                    this.remoteBlacklistCopy.Remove(nodeName);
                }
            }
        }

        /// <summary>
        /// Check job healthy
        /// </summary>
        /// <param name="numOfActiveDispatcher">number of active dispatchers</param>
        public void CheckJobHealthy(int numOfActiveDispatcher)
        {
            ThreadPool.QueueUserWorkItem(this.checkJobHealthyCallback, numOfActiveDispatcher);
        }

        protected long totalCalls;
        protected long totalFaulted;
        protected long callDuration;
        protected long outstands;
        protected long incomingRate;
        protected long processed;
        protected long processing;
        protected long purgedProcessed;
        protected long reemitted;

        protected int targetResourceCountInResourceUnits = -1;
        protected int targetResourceCountInResourceUnitsCalculated = -1;

        /// <summary>
        /// Update the job status
        /// </summary>
        /// <param name="state">null object</param>
        protected void UpdateStatus(object state)
        {
            long totalCalls, totalFaulted, callDuration, outstands, incomingRate, processed, processing, purgedProcessed, reemitted;
            this.observer.GetCounters(out totalCalls, out totalFaulted, out callDuration, out outstands, out incomingRate, out processed, out processing, out purgedProcessed, out reemitted);

            if (this.totalCalls == totalCalls
                && this.totalFaulted == totalFaulted
                && this.callDuration == callDuration
                && this.outstands == outstands
                && this.incomingRate == incomingRate
                && this.processed == processed
                && this.processing == processing
                && this.purgedProcessed == purgedProcessed
                && this.reemitted == reemitted)
            {
                return;
            }
            else
            {
                this.totalCalls = totalCalls;
                this.totalFaulted = totalFaulted;
                this.callDuration = callDuration;
                this.outstands = outstands;
                this.incomingRate = incomingRate;
                this.processed = processed;
                this.processing = processing;
                this.purgedProcessed = purgedProcessed;
                this.reemitted = reemitted;
            }

            BrokerTracing.TraceVerbose(
                "[ServiceJobMonitor] Update Status: TotalCalls = {0}, TotalFaulted = {1}, CallDuration = {2}, Outstands = {3}, IncomingRate = {4}, Processed = {5}, Processing = {6}, PurgedProcessed = {7}, reemitted = {8}",
                totalCalls, totalFaulted, callDuration, outstands, incomingRate, processed, processing, purgedProcessed, reemitted);

            try
            {
                Dictionary<string, object> props = new Dictionary<string, object>();

                int progress = totalCalls == 0 ? 0 : (int)(processed * 100 / totalCalls);
                props.Add("NumberOfCalls", totalCalls);             // Total Calls
                props.Add("NumberOfOutstandingCalls", outstands);   // Outstanding (not used)
                if (callDuration != 0)
                {
                    props.Add("CallDuration", callDuration);        // Call Duration
                }

                props.Add("CallsPerSecond", incomingRate);          // Calls per second
                props.Add("Progress", progress);                    // Progress

                props.Add(BrokerSettingsConstants.Calculated, processed);               // Calculated
                props.Add(BrokerSettingsConstants.Calculating, processing);             // Calculating
                props.Add(BrokerSettingsConstants.Faulted, totalFaulted);               // Failed
                props.Add(BrokerSettingsConstants.PurgedProcessed, purgedProcessed);    // Purged Processed
                props.Add(BrokerSettingsConstants.Reemitted, reemitted);

                //lock (this.lockClient)
                //{
                RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                RetryHelper<object>.InvokeOperationAsync(
                        async () =>
                        {
                            if (!SoaAmbientConfig.StandAlone)
                                await (await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync()).UpdateBrokerInfoAsync(this.sharedData.BrokerInfo.SessionId, props);
                            return null;
                        },
                        async (e, r) =>
                        {
                            await Task.FromResult<object>(new Func<object>(() => { BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while updating status: {0} with retry: {1}", e, r.RetryCount); return null; }).Invoke());
                        }, retry).GetAwaiter().GetResult();
                //}
            }
            catch (Exception se)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[ServiceJobMonitor] Exception throwed while updating status: {0}", se);
            }
        }

        /// <summary>
        /// Update purged counter
        /// </summary>
        public async Task UpdatePurgedCounter()
        {
            try
            {
                Dictionary<string, object> props = new Dictionary<string, object>();
                props.Add(BrokerSettingsConstants.PurgedTotal, this.observer.PurgedTotal);
                props.Add(BrokerSettingsConstants.PurgedFaulted, this.observer.PurgedFailed);
                props.Add(BrokerSettingsConstants.PurgedProcessed, this.observer.PurgedProcessed);

                RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                await RetryHelper<object>.InvokeOperationAsync(
                        async () =>
                        {
                            await (await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync()).UpdateBrokerInfoAsync(this.sharedData.BrokerInfo.SessionId, props);
                            return null;
                        },
                        async (e, r) =>
                        {
                            await Task.FromResult<object>(new Func<object>(() => { BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while updating purged counters: {0} with retry: {1}", e, r.RetryCount); return null; }).Invoke());
                        }, retry);

            }
            catch (Exception se)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[ServiceJobMonitor] Exception throwed while updating purged counters: {0}", se);
            }
        }

        /// <summary>
        /// Allocation adjust
        /// </summary>
        /// <param name="totalCalls">indicating the total calls</param>
        /// <param name="pending">indicating the pending number</param>
        public void AllocationAdjust() // TODO: Change this to async
        {
            while (!this.isFinished)
            {
                try
                {
                    // Preemption decision data structure.
                    BalanceResultInfo balanceResultInfo = null;



                    bool fullAdjust = false;
                    bool balanceRequestRefreshed = false;

                    try
                    {
                        double timeRemaining = (this.lastFullAdjustTime + TimeSpan.FromMilliseconds(this.sharedData.Config.Monitor.AllocationAdjustInterval) - DateTime.Now).TotalMilliseconds;
                        BrokerTracing.TraceInfo(
                            "[ServiceJobMonitor].AllocationAdjust: timeRemaining = {0}, lastFullAdjustTime = {1}, AllocationAdjustInterval = {2}",
                            timeRemaining,
                            this.lastFullAdjustTime,
                            this.sharedData.Config.Monitor.AllocationAdjustInterval);

                        // Check for timeRemaining.
                        // if no time remaining, we need perform full sync,
                        // otherwise, wait for the event for time remaining as timeout value.
                        if (timeRemaining > 0.0 &&
                            this.NeedAdjustAllocation.WaitOne((int)Math.Ceiling(timeRemaining)))
                        {
                            if ((DateTime.Now - this.lastFullAdjustTime).Milliseconds >= this.sharedData.Config.Monitor.AllocationAdjustInterval)
                            {
                                fullAdjust = true;
                            }
                        }
                        else
                        {
                            // timeout or time up.
                            fullAdjust = true;
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // return when disposing.
                        BrokerTracing.TraceInfo("[ServiceJobMonitor].AllocationAdjust: return as the NeedAdjustAllocation is disposed");
                        return;
                    }
                    catch (NullReferenceException)
                    {
                        // return when disposing.
                        BrokerTracing.TraceInfo("[ServiceJobMonitor].AllocationAdjust: return as the NeedAdjustAllocation is null");
                        return;
                    }

                    if (this.trigger == null)
                    {
                        // return when disposing
                        BrokerTracing.TraceInfo("[ServiceJobMonitor].AllocationAdjust: return as the trigger is null, the ServiceJobMonitor is disposed");
                        return;
                    }

                    if (!fullAdjust)
                    {
                        BrokerTracing.TraceVerbose(
                            "[ServiceJobMonitor].AllocationAdjust: Adjust only to shutdown dispatchers for preemption",
                            this.isFinished);

                        // Signaled. Do quick check whether we need exit some dispatcher.
                        if (!this.isFinished)
                        {
                            int totalActiveCapacityInCores = 0;
                            int totalActiveCapacityInResourceUnits = 0;

                            // Get the list of idle dispatchers and total capacity of the active dispatchers (in cores and resource units). Total active capacity is how
                            // many requests the active dispatchers can process simutaneously
                            List<DispatcherInfo> shutdownDispatcherList = this.dispatcherManager.GetIdleDispatcherInfoInNodeOccupationOrder(out totalActiveCapacityInCores, out totalActiveCapacityInResourceUnits);

                            BrokerTracing.TraceVerbose(
                                "[ServiceJobMonitor].AllocationAdjust: Shutdown Dispatcher List Count {0}",
                                shutdownDispatcherList.Count);

                            // Get the preemption decision from the graceful preemption handler.
                            balanceResultInfo = this.gracefulPreemptionHandler.GetDispatchersToShutdown(
                                shutdownDispatcherList,

                                this.minUnits,
                                balanceRequestRefreshed);

                        }
                    }
                    else
                    {
                        // timeout. 
                        // Go with the original logic to check for shrink
                        // Sync with scheduler for the current preemption request.
                        this.lastFullAdjustTime = DateTime.Now;

                        long totalCalls = this.observer.TotalCalls;
                        long pending = this.observer.GetQueuedRequestsCount();

                        bool shutdownDispatcher = false;

                        BrokerTracing.TraceVerbose(
                            "[ServiceJobMonitor].AllocationAdjust: Enter Full Adjust, totalCalls={0}, pending={1}, this.isFinished={2}",
                            totalCalls,
                            pending,
                            this.isFinished);

                        if (!this.isFinished)
                        {
                            // Set targetResourceCount to the queuelength divided by the avg cores per resource
                            // Avg core count is used in case the cluster has CNs with different core counts 
                            // (which is probably the 20% case)
                            int totalCores;
                            int avgCorePerResource;
                            this.dispatcherManager.GetCoreResourceUsageInformation(null, out avgCorePerResource, out totalCores);

                            // refresh the handler for the preemption and on hold info
                            var client = this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync().GetAwaiter().GetResult();
                            balanceRequestRefreshed = this.gracefulPreemptionHandler.RefreshGracefulPreemptionInfo(client).GetAwaiter().GetResult();

                            BrokerTracing.TraceVerbose(
                                "[ServiceJobMonitor].AllocationAdjust: avgCorePerResource={0}",
                                avgCorePerResource);

                            // If there are no cores found yet, or job was on hold
                            if (avgCorePerResource == 0)
                            {
                                // check if the service job is on hold, if yes, set the target resource count to min, if not, set the target resource count to previous calculated value
                                if (!this.gracefulPreemptionHandler.BalanceInfo.OnHold)
                                {
                                    // Update targetResourceCount if it is not previous calculated value
                                    if (this.targetResourceCountInResourceUnitsCalculated != this.targetResourceCountInResourceUnits)
                                    {
                                        // Update the target resource count
                                        Dictionary<string, object> props = new Dictionary<string, object>();
                                        props.Add("TargetResourceCount", this.targetResourceCountInResourceUnitsCalculated);

                                        //lock (this.lockClient)
                                        //{
                                        BrokerTracing.TraceVerbose(
                                            "[ServiceJobMonitor].AllocationAdjust: Update the TargetResourceCount property of session {0} (no dispatcher, not on hold) to previous value, TargetResourceCount={1}",
                                            this.sharedData.BrokerInfo.SessionId,
                                            this.targetResourceCountInResourceUnitsCalculated);

                                        this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync().GetAwaiter().GetResult()?.UpdateBrokerInfoAsync(this.sharedData.BrokerInfo.SessionId, props);

                                        // If target resource count increases, reset consecutive shrink counter
                                        if (this.targetResourceCountInResourceUnitsCalculated > this.targetResourceCountInResourceUnits)
                                        {
                                            this.consecutiveShrinkIntervals = 0;
                                        }

                                        this.targetResourceCountInResourceUnits = this.targetResourceCountInResourceUnitsCalculated;
                                        //}
                                    }
                                }
                                else
                                {
                                    // Update targetResourceCount if it is not min
                                    if (this.minUnits != this.targetResourceCountInResourceUnits)
                                    {
                                        // Update the target resource count
                                        Dictionary<string, object> props = new Dictionary<string, object>();
                                        props.Add("TargetResourceCount", this.minUnits);

                                        //lock (this.lockClient)
                                        //{
                                        BrokerTracing.TraceVerbose(
                                            "[ServiceJobMonitor].AllocationAdjust: Update the TargetResourceCount property of session {0} (no dispatcher, on hold) to min, TargetResourceCount={1}",
                                            this.sharedData.BrokerInfo.SessionId,
                                            this.minUnits);

                                        this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync().GetAwaiter().GetResult()?.UpdateBrokerInfoAsync(this.sharedData.BrokerInfo.SessionId, props);

                                        this.targetResourceCountInResourceUnits = this.minUnits;
                                        //}
                                    }
                                }

                                BrokerTracing.TraceVerbose(
                                    "[ServiceJobMonitor].AllocationAdjust: continue the loop for the next cycle, avgCorePerResource={0}",
                                    avgCorePerResource);

                                continue;
                            }

                            int maxCapacityPerResource = 0;
                            if (this.sharedData.Config.LoadBalancing.DispatcherCapacityInGrowShrink == 0)
                            {
                                maxCapacityPerResource = avgCorePerResource * (1 + this.sharedData.Config.LoadBalancing.ServiceRequestPrefetchCount);
                            }
                            else
                            {
                                maxCapacityPerResource = this.sharedData.Config.LoadBalancing.DispatcherCapacityInGrowShrink * (1 + this.sharedData.Config.LoadBalancing.ServiceRequestPrefetchCount);
                            }

                            this.observer.GetCounters(out _, out _, out _, out var outstandsCounter, out _, out _, out var processingCounter, out _, out _);

                            // Calculate the number of resources needed for the current number of requests (outstands = queued + executing)
                            int targetResourceCountInResourceUnits = (int)(outstandsCounter / maxCapacityPerResource);

                            // If queue length isnt an even multiple of avg core, take one extra
                            if (outstandsCounter % maxCapacityPerResource != 0)
                                targetResourceCountInResourceUnits++;

                            // Try to ask more resources if there are blocked dispatchers
                            targetResourceCountInResourceUnits += this.dispatcherManager.BlockedDispatcherCount;

                            // Get the list of idle dispatchers and total capacity of the active dispatchers (in cores and resource units). Total active capacity is how
                            // many requests the active dispatchers can process simutaneously
                            int totalActiveCapacityInCores = 0;
                            int totalActiveCapacityInResourceUnits = 0;
                            List<DispatcherInfo> idleDispatcherList = this.dispatcherManager.GetIdleDispatcherInfoInNodeOccupationOrder(out totalActiveCapacityInCores, out totalActiveCapacityInResourceUnits);

                            // Adjust targetResourceCountInResourceUnits to reflect the decision of automatic shrink
                            int currentQueueLength = (int)(outstandsCounter - processingCounter);
                            // If processing is greater than totalActiveCapacityInCores, this is because perf counter updating lag. No automatic shrink should be performed.
                            bool shouldDoAutomaticShrink = this.automaticShrinkEnabled && outstandsCounter <= totalActiveCapacityInCores && totalActiveCapacityInCores >= currentQueueLength && idleDispatcherList.Count > currentQueueLength;

                            BrokerTracing.TraceVerbose(
                                "[ServiceJobMonitor].AllocationAdjust: automaticShrinkEnabled={0}, totalActiveCapacityInCores={1}, totalActiveCapacityInResourceUnits={2}, currentQueueLength={3}, idleDispatcherList.Count={4}, shouldDoAutomaticShrink={5}, outstands={6}, processing={7}",
                                this.automaticShrinkEnabled,
                                totalActiveCapacityInCores,
                                totalActiveCapacityInResourceUnits,
                                currentQueueLength,
                                idleDispatcherList.Count,
                                shouldDoAutomaticShrink,
                                outstandsCounter,
                                processingCounter);

                            if (shouldDoAutomaticShrink)
                            {
                                targetResourceCountInResourceUnits -= idleDispatcherList.Count;
                            }
                            // Bound by min
                            if (targetResourceCountInResourceUnits < this.minUnits)
                                targetResourceCountInResourceUnits = this.minUnits;

                            // Bound by max
                            if (targetResourceCountInResourceUnits > this.maxUnits)
                                targetResourceCountInResourceUnits = this.maxUnits;

                            // targetResourceCount is currently set to the number of resources needed to process all queued requests in parallel bounded by min and max.
                            // Ensure targetResourceCount isnt set lower than the number of active service calls (not considered in queuelength)
                            targetResourceCountInResourceUnits = Math.Max(totalActiveCapacityInResourceUnits, targetResourceCountInResourceUnits);
                            targetResourceCountInResourceUnitsCalculated = targetResourceCountInResourceUnits;

                            BrokerTracing.TraceVerbose(
                                "[ServiceJobMonitor].AllocationAdjust: new targetResourceCountInResourceUnits={0}, previous targetResourceCountInResourceUnits={1}",
                                targetResourceCountInResourceUnits,
                                this.targetResourceCountInResourceUnits);

                            // if the job is on hold, set the target resource to min units
                            if (this.gracefulPreemptionHandler.BalanceInfo.OnHold)
                            {
                                targetResourceCountInResourceUnits = this.minUnits;
                            }

                            // Update targetResourceCount if it changed since last interval
                            if (targetResourceCountInResourceUnits != this.targetResourceCountInResourceUnits)
                            {
                                // Update the target resource count
                                Dictionary<string, object> props = new Dictionary<string, object>();
                                props.Add("TargetResourceCount", targetResourceCountInResourceUnits);

                                //lock (this.lockClient)
                                //{
                                BrokerTracing.TraceVerbose(
                                    "[ServiceJobMonitor].AllocationAdjust: Update the TargetResourceCount property of session {0}, TargetResourceCount={1}",
                                    this.sharedData.BrokerInfo.SessionId,
                                    targetResourceCountInResourceUnits);

                                RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                                RetryHelper<object>.InvokeOperationAsync(
                                        async () =>
                                        {
                                            await (await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync()).UpdateBrokerInfoAsync(this.sharedData.BrokerInfo.SessionId, props);
                                            return null;
                                        },
                                        async (e, r) =>
                                        {
                                            await Task.FromResult<object>(new Func<object>(() => { BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] Exception throwed while updating SessionId: {0} with retry: {1}", e, r.RetryCount); return null; }).Invoke());
                                        }, retry).GetAwaiter().GetResult();

                                // If target resource count increases, reset consecutive shrink counter
                                if (targetResourceCountInResourceUnits > this.targetResourceCountInResourceUnits)
                                {
                                    this.consecutiveShrinkIntervals = 0;
                                }

                                this.targetResourceCountInResourceUnits = targetResourceCountInResourceUnits;
                                //}
                            }

                            int shutdownDispatcherCount = 0;

                            // If automaticShrinkEnabled cluster property is true and the capacity of the active dispatchers can handle more than the rest of the requests
                            // in the queue (if any) and there are idle services to shutdown
                            if (shouldDoAutomaticShrink)
                            {
                                BrokerTracing.TraceVerbose(
                                    "[ServiceJobMonitor].AllocationAdjust: Attempt to shutdown the dispatchers.");

                                // Init shutdown count to all idle dispatchers and then pare back
                                shutdownDispatcherCount = idleDispatcherList.Count;

                                // If there are less active services than min, leave some idle\zombie services to meet min
                                if (totalActiveCapacityInResourceUnits < this.minUnits)
                                {
                                    shutdownDispatcherCount -= (this.minUnits - totalActiveCapacityInResourceUnits);

                                    // If there arent any idle services once min is met, return
                                    if (shutdownDispatcherCount <= 0)
                                    {
                                        BrokerTracing.TraceVerbose(
                                           "[ServiceJobMonitor].AllocationAdjust: should shutdownDispatcherCount={0} for shrink",
                                           shutdownDispatcherCount);

                                        shutdownDispatcherCount = 0;
                                    }
                                }

                                if (shutdownDispatcherCount > 0)
                                {
                                    // Calculate how many services to shutdown. Shrink slowly and then faster on subsequent intervals from beginning of session 
                                    // or last interval that caused a grow. Ensure 2 ^ consecutiveShrinkIntervals doesnt overflow consecutiveShrinkIntervals
                                    this.consecutiveShrinkIntervals = Math.Min(this.consecutiveShrinkIntervals + 1, 30);

                                    shutdownDispatcherCount = Math.Min((int)Math.Pow((double)2, (double)this.consecutiveShrinkIntervals), shutdownDispatcherCount);

                                    // Shrink - Remove dispatcher from dispatcher manager, and close relatvie service host.
                                    for (int i = 0; i < shutdownDispatcherCount; i++)
                                    {
                                        this.dispatcherManager.RemoveDispatcher(idleDispatcherList[i].UniqueId, /*exitServiceHost =*/ true, false);
                                    }

                                    // shrink shutdown dispatchers
                                    shutdownDispatcher = true;

                                    BrokerTracing.TraceVerbose(
                                        "[ServiceJobMonitor].AllocationAdjust: Shrink removed dispatchers, shutdownDispatcherCount={0}",
                                        shutdownDispatcherCount);
                                }
                            }
                            else
                            {
                                BrokerTracing.TraceVerbose(
                                    "[ServiceJobMonitor].AllocationAdjust: Shrink didn't remove dispatchers.");
                            }

                            BrokerTracing.TraceInfo(
                                "[ServiceJobMonitor].AllocationAdjust: Calling graceful preemption handler with idleDispatcherListCount = {0}, startIndex = {1}",
                                idleDispatcherList.Count,
                                shutdownDispatcherCount);

                            //lock (this.lockClient)
                            //{
                            balanceResultInfo = this.gracefulPreemptionHandler.GetDispatchersToShutdown(
                                idleDispatcherList.Skip(shutdownDispatcherCount).ToList(),

                                this.minUnits,
                                balanceRequestRefreshed);

                            //}

                            // only check runaway tasks when no dispatcher is shutdown by shrink or preemption
                            if (!balanceResultInfo.UseFastBalance && !balanceResultInfo.GracefulPreemptionResults.SelectMany(r => r.DispatchersToShutdown).Any() && !shutdownDispatcher)
                            {
                                BrokerTracing.TraceVerbose("[ServiceJobMonitor].AllocationAdjust: no dispatacher shutdown, check runaway tasks.");

                                // check and shutdown the runaway tasks
                                IEnumerable<int> runawayTasks = this.gracefulPreemptionHandler.GetTasksToShutdown();
                                if (runawayTasks != null)
                                {
                                    foreach (var taskid in runawayTasks)
                                    {
                                        ThreadPool.QueueUserWorkItem(new WaitCallback((state) =>
                                        {
                                            int id = (int)state;
                                            BrokerTracing.TraceVerbose("[ServiceJobMonitor].AllocationAdjust: finish runaway task {0}", id);
                                            try
                                            {
                                                this.FinishTask(id, false).GetAwaiter().GetResult(); // We don't trace runaway task result in old balance mode as it has no tolerance before masking a task as runaway
                                            }
                                            catch (Exception e)
                                            {
                                                BrokerTracing.TraceVerbose("[ServiceJobMonitor].AllocationAdjust: finish runaway task {0} : exception {1}", id, e);
                                            }
                                        }), taskid);

                                        // only shutdown one possible runaway task at a time, for the tasks may still be exiting by the exit service host call
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (balanceResultInfo == null)
                    {
                        // No decision, maybe the serivce job is finished.
                        BrokerTracing.TraceInfo(
                            "[ServiceJobMonitor].AllocationAdjust: No decision for dispatchers to shutdown, the service job might be finished. IsFinished = {0}",
                            this.isFinished);
                    }
                    else
                    {
                        // execute the shutdown dicision.
                        BrokerTracing.TraceInfo(
                            "[ServiceJobMonitor].AllocationAdjust: Graceful preemption handler returned shouldCancelJob = {0}, result number = {1}, number of ResumeRemaining in results = {2}",
                            balanceResultInfo.ShouldCancelJob,
                            balanceResultInfo.GracefulPreemptionResults.Count,
                            balanceResultInfo.GracefulPreemptionResults.Count(r => r.ResumeRemaining));

                        if (balanceResultInfo.ShouldCancelJob)
                        {
                            // If this flag is true, the preemption algorithm has already stops all 
                            // dispatchers. We will try to cancel and requeue the job,
                            // but this should only happen gracefully.
                            // So we will wait until all dispatchers are idle.
                            if (this.dispatcherManager.AreAllIdle)
                            {
                                BrokerTracing.TraceInfo("[ServiceJobMonitor].AllocationAdjust: Requeue the job.");
                                this.RequeueOrFailServiceJob("Graceful preempted.").GetAwaiter().GetResult();
                            }
                            else
                            {
                                BrokerTracing.TraceInfo("[ServiceJobMonitor].AllocationAdjust: should requeue job but not all dispatchers are idle.");
                            }
                        }
                        else
                        {
                            BrokerTracing.TraceInfo(
                                "[ServiceJobMonitor].AllocationAdjust: Calling dispatcherManager to remove dispatchers");

                            Debug.Assert(balanceResultInfo.UseFastBalance || balanceResultInfo.GracefulPreemptionResults.First().TaskIdsInInterest == null);
                            foreach (var result in balanceResultInfo.GracefulPreemptionResults)
                            {
                                // Stop the dispatchers fast to minimum the probability of sending requests again to them.
                                // Note that it won't cause severe problems as the requests will be retried in case failed because of dispatcher shutdown.
                                this.dispatcherManager.StopDispatchers(result.DispatchersToShutdown.Select(info => info.UniqueId), result.TaskIdsInInterest, result.ResumeRemaining);

                                // Shrink - Remove dispatcher from dispatcher manager,
                                // and close relatvie service host.
                                foreach (var dispatcherInfo in result.DispatchersToShutdown)
                                {
                                    if (this.dispatcherManager.RemoveDispatcher(dispatcherInfo.UniqueId, exitServiceHost: true, preemption: false))
                                    {
                                        this.AddToRemovedDispatcherIds(dispatcherInfo.UniqueId);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning(
                        "[ServiceJobMonitor].AllocationAdjust: Exception throwed while allocation adjust: {0}",
                        e);
                }
            }

            BrokerTracing.TraceInfo("[ServiceJobMonitor].AllocationAdjust: Thread Exited since the isFinished = true.");
        }

        /// <summary>
        /// Virtual Start method for child class
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="dispatcherManager"></param>
        /// <param name="observer"></param>
        /// <returns></returns>
        public virtual async Task Start(SessionStartInfoContract startInfo, DispatcherManager dispatcherManager, BrokerObserver observer)
        {
        }

        /// <summary>
        /// Informs that there's attach call from user
        /// </summary>
        public void Attach()
        {
            this.timeoutManager.ResetTimeout();
        }

        /// <summary>
        /// Informs that a client enters EOM state
        /// </summary>
        public void ClientConnected()
        {
            BrokerTracing.TraceInfo("[ServiceJobMonitor] Client connected.");
            lock (this.lockState)
            {
                if (this.state == ServiceJobState.Idle || this.state == ServiceJobState.Started)
                {
                    BrokerTracing.TraceInfo("[ServiceJobMonitor] Service state changed from {0} to Busy.", this.state);
                    this.state = ServiceJobState.Busy;
                    this.timeoutManager.Stop();
                }
            }
        }

        /// <summary>
        /// Informs that all clients enter AllRequestDone state
        /// </summary>
        public void AllClientsEnterAllRequestDoneState()
        {
            BrokerTracing.TraceInfo("[ServiceJobMonitor] All clients enter AllRequestDone state.");

            lock (this.lockState)
            {
                if (this.state == ServiceJobState.Busy)
                {
                    if (this.sharedData.Initializing)
                    {
                        // Bug4451: If all clients enters AllRequestDoneState during the initializing state, 
                        // it means that all requests are processed and the broker is waken up only for getting responses, 
                        // goes to started state to wait for a client connection timeout in this case.
                        BrokerTracing.TraceInfo("[ServiceJobMonitor] Service state changed from Busy to Started.");
                        this.state = ServiceJobState.Started;
                        this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientConnectionTimeout, this.timeoutToFinishServiceJobCallback, null);
                    }
                    else
                    {
                        BrokerTracing.TraceInfo("[ServiceJobMonitor] Service state changed from Busy to Idle.");
                        this.state = ServiceJobState.Idle;
                        this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.SessionIdleTimeout, this.timeoutToFinishServiceJobCallback, null);
                    }
                }
            }
        }

        /// <summary>
        /// Informs that a client is purged
        /// </summary>
        /// <param name="total">indicating the total number</param>
        /// <param name="failed">indicating the failed number</param>
        /// <param name="processed">indicating the processed number</param>
        public async Task ClientPurged(long total, long failed, long processed)
        {
            this.observer.ClientPurged(total, failed, processed);
            await this.UpdatePurgedCounter();
        }

        /// <summary>
        /// Close the ServiceJobMonitor
        /// </summary>
        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Dispose the service job monitor
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.trigger != null)
                {
                    this.trigger.Dispose();
                    this.trigger = null;
                }

                if (this.timeoutManager != null)
                {
                    this.timeoutManager.Dispose();
                    this.timeoutManager = null;
                }

                if (this.schedulerNotifyTimeoutManager != null)
                {
                    this.schedulerNotifyTimeoutManager.Dispose();
                }

                if (this.schedulerAdapterClientFactory != null)
                {
                    this.schedulerAdapterClientFactory.Close();
                    this.schedulerAdapterClientFactory = null;
                }

                if (this.NeedAdjustAllocation != null)
                {
                    this.NeedAdjustAllocation.Set();
                    try
                    {
                        this.allocationAdjustThread.Join();
                    }
                    catch (ThreadStateException)
                    {
                    }

                    this.NeedAdjustAllocation.Dispose();
                    this.NeedAdjustAllocation = null;
                }
                if (this.newDispatcherThreadCount != null)
                {
                    this.newDispatcherThreadCount.Dispose();
                    this.newDispatcherThreadCount = null;
                }
            }

            if (this.dispatcherManager != null)
            {
                this.dispatcherManager.Dispose();
                this.dispatcherManager = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Triggered when scheduler delegation event timed out
        /// </summary>
        /// <param name="state">null object</param>
        protected void SchedulerDelegationTimeout(object state)
        {
            BrokerTracing.TraceError("[ServiceJobMonitor] Timeout to receive scheduler delegation event.");
            try
            {
                this.RegisterJob().GetAwaiter().GetResult();
            }
            catch (FaultException<SessionFault> e)
            {
                switch (e.Detail.Code)
                {
                    case SOAFaultCode.Session_ValidateJobFailed_AlreadyFinished:
                    case SOAFaultCode.Session_ValidateJobFailed_JobCanceled:
                        this.stateManager.ServiceFailed();
                        break;
                    default:
                        // Swallow exception when creating client
                        BrokerTracing.TraceError("[ServiceJobMonitor] Exception thrown when creating client: {0}", e);
                        break;
                }
            }
            catch (Exception e)
            {
                // Swallow exception when creating client
                BrokerTracing.TraceError("[ServiceJobMonitor] Exception thrown when creating client: {0}", e);
            }
        }

        /// <summary>
        /// Callback to load the sample
        /// </summary>
        /// <param name="state">null object</param>
        protected void LoadSample(object state)
        {
            if (!SoaHelper.IsOnAzure())
            {
                // Update performance counter
                this.observer.UpdatePerformanceCounter();
            }
        }

        /// <summary>
        /// Timeout to finish service job
        /// </summary>
        /// <param name="state">indicating the state</param>
        protected void TimeoutToFinishServiceJob(object state)
        {
            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[ServiceJobMonitor] Timeout to finish service job triggered.");
            bool timeout = false;
            lock (this.lockState)
            {
                if (this.state == ServiceJobState.Idle || this.state == ServiceJobState.Started)
                {
                    timeout = true;
                    this.state = ServiceJobState.Finished;
                    this.timeoutManager.Stop();
                }
            }

            if (timeout)
            {
                // Make sure the initialization is completed before broker unload itself
                this.sharedData.WaitForInitializationComplete();
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[ServiceJobMonitor] Finish service job because timeout.");
                this.FinishServiceJob("Timeout").GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Register job
        /// </summary>
        public async Task RegisterJob()
        {
            BrokerTracing.TraceVerbose("[ServiceJobMonitor] Begin: RegisterJob");

            int autoMax, autoMin;
            Microsoft.Hpc.Scheduler.Session.Data.JobState jobState;
            try
            {
                //lock (this.lockClient)
                //{
                RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                (jobState, autoMax, autoMin) = await RetryHelper< (Hpc.Scheduler.Session.Data.JobState, int, int)>.InvokeOperationAsync(
                        async () => await (await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync()).RegisterJobAsync(this.sharedData.BrokerInfo.SessionId),
                        async (e, r) => await Task.FromResult<object>(new Func<object>(() => { BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[ServiceJobMonitor] SessionFault throws when registering job: {0} with retry {1}", e, r.RetryCount); return null; }).Invoke()),
                        retry);
                
                //}
            }
            catch (FaultException<SessionFault> e)
            {
                BrokerTracing.TraceError("[ServiceJobMonitor] SessionFault throws when registoring job: {0}", e);
                throw;
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[ServiceJobMonitor] Failed connecting to Session Launcher: {0}", e);
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_RegisterJobFailed, SR.RegisterJobFailed, e.ToString());
                return;
            }

            if (this.sharedData.StartInfo.MaxUnits.HasValue)
            {
                this.maxUnits = this.sharedData.StartInfo.MaxUnits.Value;
            }
            else
            {
                this.maxUnits = autoMax;
            }

            if (this.sharedData.StartInfo.MinUnits.HasValue)
            {
                this.minUnits = this.sharedData.StartInfo.MinUnits.Value;
            }
            else
            {
                this.minUnits = autoMin;
            }

            BrokerTracing.TraceVerbose("[ServiceJobMonitor] Current job state is {0}", jobState);
            if (jobState == Hpc.Scheduler.Session.Data.JobState.Finished || jobState == Hpc.Scheduler.Session.Data.JobState.Finishing || jobState == Hpc.Scheduler.Session.Data.JobState.Failed)
            {
                // Bug 14543: If the job is already in the above state (suppose it should never go back to
                // Running without user iteraction), set ServiceJobState to Finished as it should not allow
                // user to send more requests.
                lock (this.lockState)
                {
                    this.state = ServiceJobState.Finished;
                }

                // Bug 18011: If the service job is already finished, set the event so that
                // it won't wait until timeout when exiting.
                this.sharedData.JobFinished();
                BrokerTracing.TraceInfo("[ServiceJobMonitor] Service job state changed to Finished because current job state is {0}", jobState);
            }

            BrokerTracing.TraceVerbose("[ServiceJobMonitor] Set max and min units number: {0}, {1}", this.maxUnits, this.minUnits);

            BrokerTracing.TraceVerbose("[ServiceJobMonitor] End: RegisterJob");
        }

        /// <summary>
        /// Callback to call service failed
        /// </summary>
        /// <param name="state">null object</param>
        protected void ServiceFailed(object state)
        {
            this.stateManager.ServiceFailed();
        }

        /// <summary>
        /// Thread pool callback for checking job healthy
        /// </summary>
        /// <param name="state"></param>
        protected async void CheckJobHealthyThreadProc(object state)
        {
            if (this.dispatcherManager != null)
            {
                int numOfActiveDispatcher = (int)state;
                BrokerTracing.TraceVerbose("[ServiceJobMonitor] CheckJobHealthyThreadProc: num of active dispatcher = {0}", numOfActiveDispatcher);

                if (numOfActiveDispatcher < this.minUnits)
                {
                    BrokerTracing.TraceWarning("[ServiceJobMonitor] Too many services unreachable, requeue or fail it. ");
                    await this.RequeueOrFailServiceJob("Too many unreachable services");
                }
            }
        }

        Task ISchedulerNotify.JobStateChanged(Microsoft.Hpc.Scheduler.Session.Data.JobState jobState)
        {
            return JobStateChangedInternal(JobStateConverter.FromSoaJobState(jobState));
        }


        /// <summary>
        /// The callback event
        /// </summary>
        /// <param name="state">indicating new job state</param>
        Task JobStateChangedInternal(JobState state)
        {
            return Task.Run(() =>
            {
                BrokerTracing.TraceInfo("[ServiceJobMonitor] Job state changed: {0}", state);
                this.schedulerNotifyTimeoutManager.ResetTimeout();
                if (!this.IncreaseCount())
                {
                    BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ServiceJobMonitor] Ref object is 0, return immediately.");
                    return;
                }

                try
                {
                    if (!this.isFinished && (state == JobState.Failed || state == JobState.Canceled))
                    {
                        BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Critical, 0, "[ServiceJobMonitor] Service job failed.");

                        // Set the job finish wait handle
                        this.sharedData.JobFinished();
                        ThreadPool.QueueUserWorkItem(this.serviceFailedCallback);
                    }
                }
                finally
                {
                    // Decrease the ref count in the finally block
                    // The decrease method may call the dispose method and catch and log the execptions here
                    try
                    {
                        this.DecreaseCount();
                    }
                    catch (ThreadAbortException)
                    {
                    }
                    catch (AppDomainUnloadedException)
                    {
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[ServiceJobMonitor] Exception catched while disposing the object: {0}", e);
                    }
                }
            });
        }

        Task ISchedulerNotify.TaskStateChanged(List<TaskInfo> taskInfoList)
        {
            if (taskInfoList == null)
            {
                BrokerTracing.TraceInfo("[ServiceJobMonitor] Task event changed. the task info list is null. It means the session service encountered an error when query scheduler.");
                return Task.CompletedTask;
            }

            BrokerTracing.TraceInfo("[ServiceJobMonitor] Task event changed. Raw count = {0}.", taskInfoList.Count);
            if (this.gracefulPreemptionHandler == null)
            {
                Debug.Assert(false, "[ServiceJobMonitor] Task event changed before GracefulPreemptionHandler is created.");
                BrokerTracing.TraceWarning("[ServiceJobMonitor] Task event changed before GracefulPreemptionHandler is created.");
            }
            else
            {
                this.gracefulPreemptionHandler.AddToRecognizedTaskIds(taskInfoList.Select(i => i.Id));
            }

            this.schedulerNotifyTimeoutManager.ResetTimeout();
            if (!this.IncreaseCount())
            {
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ServiceJobMonitor] Ref object is 0, return immediately.");
                return Task.CompletedTask;
            }

            try
            {
                List<ServiceTaskDispatcherInfo> validDispatcherInfoList = new List<ServiceTaskDispatcherInfo>();

                List<int> removeIdList = new List<int>(taskInfoList.Count);

                foreach (TaskInfo info in taskInfoList)
                {
                    if (info.State == Microsoft.Hpc.Scheduler.Session.Data.TaskState.Running || info.State == Microsoft.Hpc.Scheduler.Session.Data.TaskState.Dispatching)
                    {
                        if (this.IsRemovedDispatcher(info.Id))
                        {
                            BrokerTracing.TraceInfo("[ServiceJobMonitor] Dispatcher {0} is removed once and won't be created again. Task state {1}.", info.Id, info.State);
                        }
                        else if (!this.dispatcherManager.ContainDispather(info.Id))
                        {
                            ServiceTaskDispatcherInfo serviceTaskDispatcherInfo = null;

                            if (info.Location == Microsoft.Hpc.Scheduler.Session.Data.NodeLocation.OnPremise || info.Location == Microsoft.Hpc.Scheduler.Session.Data.NodeLocation.Linux || info.Location == Microsoft.Hpc.Scheduler.Session.Data.NodeLocation.NonDomainJoined)
                            {
                                //
                                // the node is on-premise or Linux node
                                //

                                // if this node is in blacklist, remove it
                                lock (this.lockRemoteBlacklistCopy)
                                {
                                    remoteBlacklistCopy.Remove(info.MachineName);
                                }

                                BrokerTracing.TraceInfo("[ServiceJobMonitor] Create new dispatcher for on-premise node {0} because task state is changed into {1}.", info.Id, info.State);

                                // check if using backend-security (for java soa only)
                                // check if env ENABLE_BACKEND_SECURITY == true
                                bool isEnableBackendSecurity = false;
                                var envEnableBackendSecurity = this.sharedData.ServiceConfig.EnvironmentVariables[Constant.IsEnableBackendSecurityEnvVar];
                                if (envEnableBackendSecurity != null
                                    && String.IsNullOrEmpty(envEnableBackendSecurity.Value) == false
                                    && Boolean.TryParse(envEnableBackendSecurity.Value, out isEnableBackendSecurity) == true
                                    && isEnableBackendSecurity == true
                                    // has to be using http binding
                                    && this.dispatcherManager.BackEndIsHttp == true)
                                {
                                    BrokerTracing.TraceInfo("[ServiceJobMonitor] Create new dispatcher info for Java Wss securint mode");

                                    // use securint mode
                                    serviceTaskDispatcherInfo = new WssDispatcherInfo(
                                        this.sharedData.BrokerInfo.SessionId,
                                        info.Id,
                                        info.Capacity,
                                        info.MachineName,
                                        info.MachineVirtualName,
                                        info.FirstCoreIndex,
                                        this.networkPrefix,
                                        info.Location);
                                }
                                else
                                {
                                    BrokerTracing.TraceInfo("[ServiceJobMonitor] Create new dispatcher info for normal mode");

                                    // normal mode
                                    serviceTaskDispatcherInfo = new ServiceTaskDispatcherInfo(
                                        this.sharedData.BrokerInfo.SessionId,
                                        info.Id,
                                        info.Capacity,
                                        info.MachineName,
                                        info.MachineVirtualName,
                                        info.FirstCoreIndex,
                                        this.networkPrefix,
                                        info.Location,
                                        this.dispatcherManager.BackEndIsHttp);
                                }
                            }
                            else if (info.Location == Microsoft.Hpc.Scheduler.Session.Data.NodeLocation.AzureEmbedded || info.Location == Microsoft.Hpc.Scheduler.Session.Data.NodeLocation.AzureEmbeddedVM)
                            {
                                //
                                // the cluster is on Azure
                                //

                                if (info.State == Microsoft.Hpc.Scheduler.Session.Data.TaskState.Running)
                                {
                                    lock (this.lockRemoteBlacklistCopy)
                                    {
                                        remoteBlacklistCopy.Remove(info.MachineVirtualName);
                                    }

                                    if (this.nodeMappingData != null)
                                    {
                                        this.nodeMappingData.Wait();

                                        string ipaddress;

                                        //TODO: on azure, update the mapping it if necessary
                                        if (this.nodeMappingData.Dictionary != null && this.nodeMappingData.Dictionary.TryGetValue(info.MachineVirtualName, out ipaddress))
                                        {
                                            info.MachineName = ipaddress;
                                        }
                                    }

                                    BrokerTracing.TraceInfo("[ServiceJobMonitor] Create new dispatcher for Azure cluster node {0} because task state is changed into {1}.", info.Id, info.State);

                                    serviceTaskDispatcherInfo = new ServiceTaskDispatcherInfo(
                                        this.sharedData.BrokerInfo.SessionId,
                                        info.Id,
                                        info.Capacity,
                                        info.MachineName,
                                        info.MachineVirtualName,
                                        info.FirstCoreIndex,
                                        this.networkPrefix,
                                        this.dispatcherManager.BackEndIsHttp);
                                }
                                else
                                {
                                    BrokerTracing.TraceInfo(
                                        "[ServiceJobMonitor] Skip to create new dispatcher for Azure cluster node {0} because task state is changed into {1}.",
                                        info.Id,
                                        info.State);
                                }
                            }
                            else if (info.Location == Microsoft.Hpc.Scheduler.Session.Data.NodeLocation.Azure || info.Location == Microsoft.Hpc.Scheduler.Session.Data.NodeLocation.AzureVM)
                            {
                                //
                                // burst mode (the node is on Azure)
                                //

                                if (info.State == Microsoft.Hpc.Scheduler.Session.Data.TaskState.Running)
                                {
                                    lock (this.lockRemoteBlacklistCopy)
                                    {
                                        remoteBlacklistCopy.Remove(info.MachineName);
                                    }

                                    BrokerTracing.TraceInfo(
                                        "[ServiceJobMonitor] Create new dispatcher for Azure burst node {0} because task state is changed into {1}, job requeue count {2}, azureLoadBalancerAddress {3}",
                                        info.Id,
                                        info.State,
                                        info.JobRequeueCount,
                                        info.AzureLoadBalancerAddress);

                                    serviceTaskDispatcherInfo = new AzureDispatcherInfo(
                                        this.sharedData.BrokerInfo.SessionId,
                                        info.JobRequeueCount,
                                        info.Id,
                                        info.Capacity,
                                        info.MachineName,
                                        info.FirstCoreIndex,
                                        this.networkPrefix,
                                        info.ProxyServiceName,
                                        !this.sharedData.BrokerInfo.HttpsBurst,
                                        info.AzureLoadBalancerAddress);
                                }
                                else
                                {
                                    BrokerTracing.TraceInfo(
                                        "[ServiceJobMonitor] Skip to create new dispatcher for Azure burst node {0} because task state is changed into {1}, job requeue count {2}",
                                        info.Id,
                                        info.State,
                                        info.JobRequeueCount);
                                }
                            }
                            else
                            {
                                BrokerTracing.TraceInfo("[ServiceJobMonitor] Un supported NodeLocation {0}", info.Location);
                            }

                            if (serviceTaskDispatcherInfo != null)
                            {
                                validDispatcherInfoList.Add(serviceTaskDispatcherInfo);
                            }
                        }
                        else
                        {
                            BrokerTracing.TraceInfo("[ServiceJobMonitor] Dispatcher {0} is already created. Task state {1}.", info.Id, info.State);
                        }
                    }
                    else if (info.State == Microsoft.Hpc.Scheduler.Session.Data.TaskState.Canceled || info.State == Microsoft.Hpc.Scheduler.Session.Data.TaskState.Failed || info.State == Microsoft.Hpc.Scheduler.Session.Data.TaskState.Finished || info.State == Microsoft.Hpc.Scheduler.Session.Data.TaskState.Finishing)
                    {
                        // remove (info.State == TaskState.Canceling) from the condition above.
                        // when preemption happens to the task, it is in cancelling state. Should not remove the dispather,
                        // because we need to wait for the responses of the processing requests. (graceful preemption at message level)

                        if (this.dispatcherManager.ContainDispather(info.Id))
                        {
                            BrokerTracing.TraceInfo("[ServiceJobMonitor] Remove dispatcher {0} because task state is changed into {1}.", info.Id, info.State);
                            removeIdList.Add(info.Id);
                        }
                    }
                }

                if (validDispatcherInfoList.Count != 0)
                {
                    if (BrokerIdentity.IsHAMode)
                    {
                        // Use threadpool threads to call NewDispatcher method avoid blocking
                        // the DecreaseCount method in the finally block below.

                        ParallelOptions options = new ParallelOptions();

                        options.MaxDegreeOfParallelism = 32;

                        Parallel.ForEach<ServiceTaskDispatcherInfo>(
                            validDispatcherInfoList,
                            options,
                            info =>

                                {
                                    try
                                    {
                                        if (this.newDispatcherThreadCount.WaitOne())
                                        {
                                            // WCF BUG:
                                            // We need a sync call Open on ClientBase<> to get the correct impersonation context
                                            // this is a WCF bug, so we create the client on a dedicated thread.
                                            // If we call it in thread pool thread, it will block the thread pool thread for a while
                                            // and drops the performance.
                                            Thread t = new Thread(
                                                () =>
                                                    {
                                                        try
                                                        {
                                                            this.dispatcherManager.NewDispatcherAsync(info).GetAwaiter().GetResult();
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            BrokerTracing.TraceError("[ServiceJobMonitor] Exception happened when create a dispatcher for task {0}, {1}", info.TaskId, e);
                                                        }
                                                    });

                                            t.Start();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        BrokerTracing.TraceError("[ServiceJobMonitor] Exception happened when create a thread to create a new dispatcher for task {0}, {1}", info.TaskId, e);
                                    }
                                    finally
                                    {
                                        try
                                        {
                                            this.newDispatcherThreadCount.Release();
                                        }
                                        catch (Exception e)
                                        {
                                            BrokerTracing.TraceError("[ServiceJobMonitor] Exception happened when release the semaphore for task {0}, {1}", info.TaskId, e);
                                        }
                                    }
                                });
                    }
                    else
                    {
                        foreach (var info in validDispatcherInfoList)
                        {
                            this.dispatcherManager.NewDispatcherAsync(info)
                                .ContinueWith(
                                    t => t.Exception.Handle(
                                        ex =>
                                        {
                                            BrokerTracing.TraceError("[ServiceJobMonitor] Exception happened when create a dispatcher for task {0}, {1}", info.TaskId, ex);
                                            return true;
                                        }),
                                    TaskContinuationOptions.OnlyOnFaulted);
                        }
                    }
                }

                if (removeIdList.Count != 0)
                {
                    // Use a threadpool thread to call BatchRemoveDispatcher method avoid blocking
                    // the DecreaseCount method in the finally block below.
                    // BatchRemoveDispatcher may be blocked when it disposes a dispatcher because
                    // of the non-zero ref count.
                    ThreadPool.QueueUserWorkItem(new ThreadHelper<object>(new WaitCallback(delegate (object state) { this.dispatcherManager.BatchRemoveDispatcher(removeIdList); })).CallbackRoot);
                }
            }
            finally
            {
                // Decrease the ref count in the finally block
                // The decrease method may call the dispose method and catch and log the execptions here
                try
                {
                    this.DecreaseCount();
                }
                catch (ThreadAbortException)
                {
                }
                catch (AppDomainUnloadedException)
                {
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[ServiceJobMonitor] Exception caught while disposing the object: {0}", e);
                }
            }

            return Task.CompletedTask;
        }

        private void AddToRemovedDispatcherIds(int taskId)
        {
            if (this.gracefulPreemptionHandler == null)
            {
                Debug.Assert(false, "[ServiceJobMonitor] Dispatcher removed before GracefulPreemptionHandler is created.");
                BrokerTracing.TraceWarning("[ServiceJobMonitor] Dispatcher removed before GracefulPreemptionHandler is created.");
            }
            else
            {
                this.gracefulPreemptionHandler.AddToRemovedDispatcherIds(taskId);
            }
        }

        private bool IsRemovedDispatcher(int taskId)
        {
            if (this.gracefulPreemptionHandler == null)
            {
                Debug.Assert(false, "[ServiceJobMonitor] Query removed dispatcher before GracefulPreemptionHandler is created.");
                BrokerTracing.TraceWarning("[ServiceJobMonitor] Query removed dispatcher before GracefulPreemptionHandler is created.");
                return false;
            }
            else
            {
                return this.gracefulPreemptionHandler.IsRemovedDispatcher(taskId);
            }
        }

        public async Task FinishTask(int taskId, bool isRunAwayTask)
        {
            if (!this.IsRemovedDispatcher(taskId))
            {
                if (isRunAwayTask)
                {
                    BrokerTracing.TraceError("Runaway task {0} is not previously stopped by broker worker.", taskId);
                }
                this.AddToRemovedDispatcherIds(taskId);
            }
            var client = await this.SchedulerAdapterFactory.GetSchedulerAdapterClientAsync();
            bool finishTaskSucceed = false;
            try
            {
                finishTaskSucceed = await client.FinishTask(this.sharedData.BrokerInfo.SessionId, taskId);

            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("Fail to finish task {0} of job {1} : {2}", taskId, this.sharedData.BrokerInfo.SessionId, e);
            }

            if (!finishTaskSucceed)
            {
                this.tasksToBeCancelled.AddOrUpdate(taskId, 1, (k, v) => v + 1);
            }
        }

        public async Task FinishTask(DispatcherInfo dispatcherInfo)
        {
            await this.FinishTask(dispatcherInfo.UniqueId, false);
        }

        protected void FinishTasksThread(object state)
        {
            var client = this.SchedulerAdapterFactory.GetSchedulerAdapterClientAsync().GetAwaiter().GetResult();

            var finishResults = this.tasksToBeCancelled.Select(kvp =>
            {
                bool finishTaskSucceed = false;
                try
                {
                    finishTaskSucceed = client.FinishTask(this.sharedData.BrokerInfo.SessionId, kvp.Key).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("Fail to finish task {0} of job {1} : {2}", kvp.Key, this.sharedData.BrokerInfo.SessionId, e);
                }

                return new
                {
                    TaskUniqueId = kvp.Key,
                    Success = finishTaskSucceed,
                };
            }).ToList();

            foreach (var r in finishResults)
            {
                if (r.Success)
                {
                    int retryCount;
                    this.tasksToBeCancelled.TryRemove(r.TaskUniqueId, out retryCount);
                }
                else
                {
                    int currentRetryCount = this.tasksToBeCancelled.AddOrUpdate(r.TaskUniqueId, 0, (k, v) => v + 1);
                    if (currentRetryCount > MaxFinishTaskRetryCount)
                    {
                        BrokerTracing.TraceError("Retried to finish the task {0} for {1} times, abort it", r.TaskUniqueId, currentRetryCount);
                        this.tasksToBeCancelled.TryRemove(r.TaskUniqueId, out currentRetryCount);
                    }
                }
            }
        }
    }
}
