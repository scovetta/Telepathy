// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using TelepathyCommon.HpcContext;
using TelepathyCommon.HpcContext.Extensions;
using TelepathyCommon.HpcContext.Extensions.RegistryExtension;

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;
    using System.Net.Http;
    using SoaAmbientConfig;

    using SR = Microsoft.Hpc.SvcBroker.SR;

    /// <summary>
    /// Manage dispatchers
    /// </summary>
    internal sealed class DispatcherManager : IDispatcherManager, IDisposable
    {
        /// <summary>
        /// Stores the uri informations for REST service host
        /// </summary>
        private string prefix = "http://";

        private int port = 80;

        private string serverName = "SvcHost";

        private string endPoint = "svchostserver";

        /// <summary>
        /// Stores the push message period
        /// </summary>
        private const int PushMessagePeriod = 5000;

        /// <summary>
        /// Stores the block time span
        /// </summary>
        private readonly TimeSpan blockTimeSpan;

        /// <summary>
        /// Stores the default binding
        /// </summary>
        private Binding defaultBinding;

        /// <summary>
        /// Store the dictionary of active dispatchers, keyed by task id
        /// </summary>
        private Dictionary<string, Dispatcher> dispatcherDic;

        /// <summary>
        /// Stores dispatchers that are failed due to Service_InitializeFailed, or EndpointNotFoundException
        /// </summary>
        private List<string> failedDispatcherList;

        /// <summary>
        /// Lock object for failedDispatcherList
        /// </summary>
        private object lockFailedDispatcherList = new object();

        /// <summary>
        /// Stores disptacher info of temporarily blocked dispatchers
        /// </summary>
        private Dictionary<string, DispatcherInfo> blockedDispatcherDic;

        /// <summary>
        /// Stores dispatcher info of temporarily blocked dispatchers, in order of their blocktime
        /// </summary>
        private Queue<DispatcherInfo> blockedDispatcherQueue;

        /// <summary>
        /// Number of blocked disaptchers whose BlockRetryCount == 0
        /// </summary>
        private int youngBlockedDispatcherCount;

        /// <summary>
        /// Stores the default capacity
        /// </summary>
        private int defaultCapacity;

        /// <summary>
        /// Stores the lock object
        /// </summary>
        private object lockThis = new object();

        /// <summary>
        /// Stores the timer to unblock blocked dispatchers
        /// </summary>
        private Timer unblockTimer;

        /// <summary>
        /// Stores shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// Stores the broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Stores the broker queue factory
        /// </summary>
        private BrokerQueueFactory queueFactory;

        /// <summary>
        /// Stores the service job monitor
        /// </summary>
        private ServiceJobMonitorBase monitor;

        /// <summary>
        /// Stores a value indicating whether backend binding is http
        /// </summary>
        private bool isHttp;

        /// <summary>
        /// Instance of the AzureQueueManager.
        /// </summary>
        private AzureQueueManager azureQueueManager;

        /// <summary>
        /// Stores a value indicating whether connection string is valid.
        /// </summary>
        private bool? connectionStringValid;

        /// <summary>
        /// Stores a value indicating whether "Message Details" is available for this session
        /// </summary>
        private bool supportsMessageDetails;

        /// <summary>
        /// It indicates if use Azure storage based burst.
        /// </summary>
        private bool httpsBurst;

        /// <summary>
        /// Stores the fabric cluster context;
        /// </summary>
        private ITelepathyContext context;

        /// <summary>
        /// Initializes a new instance of the DispatcherManager class
        /// </summary>
        /// <param name="bindings">indicating the bindings section</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="queueFactory">indicating the queue factory</param>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="frontendResult">indicating the frontend result</param>
        public DispatcherManager(BindingsSection bindings, SharedData sharedData, BrokerObserver observer, ServiceJobMonitorBase monitor, BrokerQueueFactory queueFactory, ITelepathyContext context)
        {
            this.dispatcherDic = new Dictionary<string, Dispatcher>();
            this.failedDispatcherList = new List<string>();
            this.blockedDispatcherDic = new Dictionary<string, DispatcherInfo>();
            this.blockedDispatcherQueue = new Queue<DispatcherInfo>();

            this.sharedData = sharedData;
            this.observer = observer;
            this.monitor = monitor;
            this.queueFactory = queueFactory;
            this.context = context;
            this.defaultCapacity = this.sharedData.ServiceConfig.MaxConcurrentCalls;
            this.blockTimeSpan = TimeSpan.FromMilliseconds(this.sharedData.Config.LoadBalancing.EndpointNotFoundRetryPeriod);
            this.unblockTimer = new Timer(new ThreadHelper<object>(new TimerCallback(this.CallbackToQueryBlockedDispatcherList)).CallbackRoot, null, -1, -1);

            this.defaultBinding = BindingHelper.GetBackEndBinding(bindings);
            this.isHttp = this.defaultBinding.CreateBindingElements().Find<HttpTransportBindingElement>() != null;

            // Update binding's maxMessageSize settings with global maxMessageSize if its enabled (> 0)
            int maxMessageSize = sharedData.ServiceConfig.MaxMessageSize;
            if (maxMessageSize > 0)
            {
                BindingHelper.ApplyMaxMessageSize(this.defaultBinding, maxMessageSize);
            }

            // get soa burst protocol info
            this.httpsBurst = sharedData.BrokerInfo.HttpsBurst;
            if (this.httpsBurst)
            {
                this.azureQueueManager = new AzureQueueManager(sharedData.BrokerInfo.SessionId, this.sharedData.BrokerInfo.ClusterName, this.sharedData.BrokerInfo.ClusterId);
            }

            this.supportsMessageDetails = this.defaultBinding.MessageVersion.Addressing != AddressingVersion.None || this.defaultBinding is BasicHttpBinding;
            BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[DispatcherManager] Init backend binding: OperatiomTimeout = {0}, DefaultCapacity = {1}", this.sharedData.Config.LoadBalancing.ServiceOperationTimeout, this.defaultCapacity);
            BrokerTracing.EtwTrace.LogBackendBindingLoaded(
                sharedData.BrokerInfo.SessionId,
                "Backend",
                maxMessageSize,
                this.defaultBinding.ReceiveTimeout.Ticks,
                this.defaultBinding.SendTimeout.Ticks,
                this.defaultBinding.MessageVersion.ToString(),
                this.defaultBinding.Scheme);
        }

        /// <summary>
        /// Gets a value indicating whether backend is using http binding
        /// </summary>
        public bool BackEndIsHttp
        {
            get { return this.isHttp; }
        }

        /// <summary>
        /// Gets a value indicating whether "Message Details" is available for the backend of this session
        /// </summary>
        public bool BackendSupportsMessageDetails
        {
            get
            {
                return this.supportsMessageDetails;
            }
        }

        /// <summary>
        /// Gets the current active dispatcher count.
        /// </summary>
        public int ActiveDispatcherCount
        {
            get
            {
                lock (this.lockThis)
                {
                    return this.sharedData.DispatcherCount + this.youngBlockedDispatcherCount;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether all dispatchers are idle
        /// </summary>
        public bool AreAllIdle
        {
            get
            {
                lock (this.lockThis)
                {
                    return this.dispatcherDic.Values.All(d => d.ProcessingRequests == 0);
                }
            }
        }

        /// <summary>
        /// Finalizes an instance of the DispatcherManager class
        /// </summary>
        ~DispatcherManager()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Determine whether the dispatcher exists
        /// </summary>
        /// <param name="taskId">indicating the task id</param>
        /// <returns>returns a boolean value</returns>
        public bool ContainDispather(string taskId)
        {
            lock (this.lockThis)
            {
                if (this.dispatcherDic.ContainsKey(taskId) || this.blockedDispatcherDic.ContainsKey(taskId))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Dispose the DispatcherManager class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// stop the dispatchers prefetching.
        /// </summary>
        /// <param name="taskIds">the task ids identifying the dispatchers to stop</param>
        /// <param name="availableToShutdownTaskIds">true if resume the remaining dispatchers.</param>
        public void StopDispatchers(IEnumerable<string> availableToShutdownTaskIds, IEnumerable<string> tasksInInterest, bool resumeRemaining = true)
        {
            lock (this.lockThis)
            {
                Debug.Assert(availableToShutdownTaskIds != null);
                Debug.Assert(tasksInInterest == null || !availableToShutdownTaskIds.Except(tasksInInterest).Any(), "availableToShutdownTaskIds is not subset of tasksInInterest");

                // Get dispatchers available to stop.
                foreach (string taskId in availableToShutdownTaskIds)
                {
                    Dispatcher dispatcher;
                    if (this.dispatcherDic.TryGetValue(taskId, out dispatcher))
                    {
                        BrokerTracing.TraceInfo(
                            "[DispatcherManager].StopDispatchers Stop Dispatcher ID = {0}, CoreCount = {1}",
                            dispatcher.TaskId,
                            dispatcher.Info.CoreCount);

                        dispatcher.Stop();
                    }
                    else
                    {
                        BrokerTracing.TraceInfo(
                            "[DispatcherManager].StopDispatchers ID = {0}. Not Found.",
                            taskId);
                    }
                }

                if (resumeRemaining)
                {
                    HashSet<string> tasksInInterestSet = null;
                    if (tasksInInterest != null)
                    {
                        tasksInInterestSet = new HashSet<string>(tasksInInterest);
                    }

                    HashSet<string> shouldNotResumeDispatcherIds = new HashSet<string>(availableToShutdownTaskIds);

                    // resume the remaining dispatchers
                    foreach (var dispatcher in this.dispatcherDic.Values.Where(d => TaskInInterestUtil.IsTaskInInterest(tasksInInterestSet, d.TaskId) && !shouldNotResumeDispatcherIds.Contains(d.TaskId)))
                    {
                        if (!shouldNotResumeDispatcherIds.Contains(dispatcher.TaskId))
                        {
                            dispatcher.Resume();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// stop all the dispatchers.
        /// </summary>
        public void StopAllDispatchers()
        {
            lock (this.lockThis)
            {
                Parallel.ForEach(
                    this.dispatcherDic.Values,
                    d =>
                    {
                        BrokerTracing.TraceInfo(
                            "[DispatcherManager].StopAllDispatchers stop ID = {0}.",
                            d.TaskId);

                        d.Stop();
                    });
            }
        }

        /// <summary>
        /// resume the dispatchers to prefetch requests.
        /// </summary>
        public void ResumeDispatchers()
        {
            lock (this.lockThis)
            {
                foreach (var dispatcher in this.dispatcherDic.Values)
                {
                    dispatcher.Resume();
                }
            }
        }

        /// <summary>
        /// Remove a dispatcher
        /// </summary>
        /// <param name="taskId">task id link to this dispatcher</param>
        /// <param name="exitServiceHost">flag indicating if exit related service host</param>
        /// <param name="preemption">flag idicating if the service is preempted</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "May need in the future")]
        public bool RemoveDispatcher(string taskId, bool exitServiceHost, bool preemption)
        {
            Dispatcher dispatcher = null;
            DispatcherInfo dispatcherInfo = null;
            lock (this.lockThis)
            {
                dispatcher = this.RemoveActiveDispatcher(taskId);
                if (dispatcher != null)
                {
                    dispatcherInfo = dispatcher.Info;
                }
                else
                {
                    dispatcherInfo = this.RemoveBlockedDispatcher(taskId);
                }
            }

            if (dispatcherInfo == null)
            {
                BrokerTracing.TraceWarning(
                    "[DispatcherManager].RemoveDispatcher: Remove dispatcher failed, the dispatcher info of task {0} does not exist",
                    taskId);

                return false;
            }
            else
            {
                BrokerTracing.TraceVerbose(
                    "[DispatcherManager].RemoveDispatcher: Attempt to remove dispatcher {0} for the node {1}",
                    taskId,
                    SoaHelper.IsOnAzure() ? dispatcherInfo.MachineVirtualName : dispatcherInfo.MachineName);
            }

            // stop dispatching to prevent too many communication exceptions when exiting serivce host
            if (dispatcher != null)
            {
                dispatcher.Stop();
            }

            if (!preemption)
            {
                // exit service host before closing dispatcher
                if (exitServiceHost)
                {
                    this.ExitServiceHost(dispatcherInfo);
                }

                if (dispatcher != null)
                {
                    dispatcher.Close();
                }
            }
            else
            {
                // The dispatcher waits for all the responses with a timeout before closing.
                // When preemption happens, the HpcServiceHost directly replies a fault message
                // without invoking the hosted service, so the responses come back quickly.
                if (dispatcher != null)
                {
                    dispatcher.Close();
                }

                if (exitServiceHost)
                {
                    this.ExitServiceHost(dispatcherInfo);
                }
            }

            // Also remember to remove it from failed dispatcher list
            lock (this.lockFailedDispatcherList)
            {
                this.failedDispatcherList.Remove(taskId);
            }

            return true;
        }

        /// <summary>
        /// Exit service host
        /// </summary>
        /// <param name="dispatcherInfo"></param>
        private void ExitServiceHost(DispatcherInfo dispatcherInfo)
        {
            var controller = Dispatcher.CreateController(dispatcherInfo, this.defaultBinding, this.httpsBurst);

            BrokerTracing.TraceInfo("[DispatcherManager] .ExitServiceHost: Calling BeginExit for task id {0}, {1}", dispatcherInfo.UniqueId, dispatcherInfo.ServiceHostControllerAddress);
            controller.BeginExit(() =>
            {
                try
                {
                    this.monitor.FinishTask(dispatcherInfo).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[ServiceHostController] .BeginExit: onFailed callback exception for task id {0} : {1}", dispatcherInfo.UniqueId, e.ToString());
                }
            });
        }

        /// <summary>
        /// Block a dispatcher temporarily
        /// </summary>
        /// <param name="dispatcherInfo">dispatcher info</param>
        public void BlockDispatcher(DispatcherInfo dispatcherInfo)
        {
            Dispatcher dispatcher = null;
            int activeDispatcherCount = 0;
            lock (this.lockThis)
            {
                dispatcher = this.RemoveActiveDispatcher(dispatcherInfo.UniqueId);
                if (dispatcher == null)
                {
                    BrokerTracing.TraceVerbose("[DispatcherManager] Block dispatcher failed: {0} is not active", dispatcherInfo.UniqueId);
                    return;
                }

                this.AddBlockedDispatcher(dispatcherInfo);

                // Note: dispatchers that are blocked but not retried are also considered as 'active'
                activeDispatcherCount = this.sharedData.DispatcherCount + this.youngBlockedDispatcherCount;
            }

            dispatcher.Close();

            // check job healthy to see if number of active dispatchers is smaller than (job.minResourceUnit)
            this.monitor.CheckJobHealthy(activeDispatcherCount);
        }

        /// <summary>
        /// Count of dispather number
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "May need in the future")]
        public int Count
        {
            get
            {
                lock (this.lockThis)
                {
                    return this.dispatcherDic.Count;
                }
            }
        }

        /// <summary>
        /// Number of blocked dispatcher
        /// </summary>
        public int BlockedDispatcherCount
        {
            get
            {
                lock (this.lockThis)
                {
                    return this.blockedDispatcherDic.Count;
                }
            }
        }

        /// <summary>
        /// Create a instance of Dispatcher
        /// </summary>
        /// <param name="dispatcherInfo">the dispatcher info</param>
        public async Task NewDispatcherAsync(DispatcherInfo dispatcherInfo)
        {
            dispatcherInfo.ApplyDefaultCapacity(this.defaultCapacity);

            Dispatcher dispatcher = null;

            // This lock is to sync operations to the dispatcherDic
            lock (this.lockThis)
            {
                if (this.dispatcherDic.ContainsKey(dispatcherInfo.UniqueId) || this.blockedDispatcherDic.ContainsKey(dispatcherInfo.UniqueId))
                {
                    BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[DispatcherManager] Task race condition detected, taskid = {0}", dispatcherInfo.UniqueId);
                    return;
                }
            }

            try
            {
                BrokerTracing.TraceInfo("[DispatcherManager] Create new dispatcher: {0}", dispatcherInfo.AllocatedNodeLocation);
                if (dispatcherInfo.AllocatedNodeLocation == Microsoft.Hpc.Scheduler.Session.Data.NodeLocation.OnPremise
                    || dispatcherInfo.AllocatedNodeLocation == Scheduler.Session.Data.NodeLocation.Linux
                    || dispatcherInfo.AllocatedNodeLocation == Scheduler.Session.Data.NodeLocation.AzureEmbedded
                    || dispatcherInfo.AllocatedNodeLocation == Scheduler.Session.Data.NodeLocation.AzureEmbeddedVM
                    || dispatcherInfo.AllocatedNodeLocation == Scheduler.Session.Data.NodeLocation.NonDomainJoined)
                {
                    // check if using backend-security (for java soa only)
                    if (dispatcherInfo is WssDispatcherInfo)
                    {
                        // use security mode
                        dispatcher = new WssDispatcher(
                            dispatcherInfo,
                            this.defaultBinding,
                            this.sharedData,
                            this.observer,
                            this.queueFactory,
                            this.monitor.SchedulerAdapterFactory,
                            this.monitor.NeedAdjustAllocation);
                    }
                    else
                    {
                        // normal mode
                        dispatcher = new Dispatcher(
                            dispatcherInfo,
                            this.defaultBinding,
                            this.sharedData,
                            this.observer,
                            this.queueFactory,
                            this.monitor.SchedulerAdapterFactory,
                            this.monitor.NeedAdjustAllocation);
                    }
                }
#if HPCPACK
                else if (dispatcherInfo.AllocatedNodeLocation == Scheduler.Session.Data.NodeLocation.AzureVM
                    || dispatcherInfo.AllocatedNodeLocation == Scheduler.Session.Data.NodeLocation.Azure)
                {
                    // NodeLocation.Azure, NodeLocation.AzureVM
                    if (this.httpsBurst)
                    {
                        // for https connection
                        if (this.connectionStringValid == null)
                        {
                            this.ValidateConnectionString().GetAwaiter().GetResult();
                        }

                        if (this.connectionStringValid.Value)
                        {
                            dispatcher = new AzureHttpsDispatcher(
                                this.azureQueueManager,
                                dispatcherInfo,
                                this.defaultBinding,
                                this.sharedData,
                                this.observer,
                                this.queueFactory,
                                this.monitor.SchedulerAdapterFactory,
                                this.monitor.NeedAdjustAllocation);
                        }
                        else
                        {
                            // ValidateConnectionString method already writes trace
                            // for this case.
                            return;
                        }
                    }
                    else
                    {
                        // for nettcp connection
                        dispatcher = new AzureDispatcher(
                            dispatcherInfo,
                            this.defaultBinding,
                            this.sharedData,
                            this.observer,
                            this.queueFactory,
                            this.monitor.SchedulerAdapterFactory,
                            this.monitor.NeedAdjustAllocation);
                    }
                }
#endif
                else
                {
                    BrokerTracing.TraceError("Not supported NodeLocation {0} for dispatcher", dispatcherInfo.AllocatedNodeLocation);
                }

                dispatcher.Failed += new EventHandler(this.DispatcherFailed);
                dispatcher.Connected += new EventHandler(this.DispatcherConnected);
                dispatcher.OnServiceInstanceFailedEvent += new EventHandler<ServiceInstanceFailedEventArgs>(this.ServiceInstanceFailed);
            }
            catch (InvalidOperationException e)
            {
                BrokerTracing.TraceError("[DispatcherManager] Create dispatcher failed: {0}", e);
                return;
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[DispatcherManager] Create dispatcher failed.  Exception: {0}", e);
                return;
            }

            bool exist = false;
            lock (this.lockThis)
            {
                if (this.dispatcherDic.ContainsKey(dispatcherInfo.UniqueId) || this.blockedDispatcherDic.ContainsKey(dispatcherInfo.UniqueId))
                {
                    BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[DispatcherManager] Task race condition detected, taskid = {0}", dispatcherInfo.UniqueId);
                    exist = true;
                }
                else
                {
                    this.dispatcherDic.Add(dispatcherInfo.UniqueId, dispatcher);
                    this.sharedData.DispatcherCount = this.dispatcherDic.Count;
                }
            }

            if (exist)
            {
                try
                {
                    dispatcher.Close();
                }
                catch (Exception ex)
                {
                    // Abandon the dispatcher, don't care about the exception.
                    BrokerTracing.TraceWarning("[DispatcherManager].Dispose: Exception {0}", ex);
                }
            }
            else
            {
                try
                {
                    // Notice: it is expensive to start the azure dispatcher because the client.open is time consuming for azure.
                    BrokerTracing.TraceVerbose("[DispatcherManager.NewDispatcher] Begin: Start dispatcher.");
                    await dispatcher.StartAsync().ConfigureAwait(false);
                    BrokerTracing.TraceVerbose("[DispatcherManager.NewDispatcher] End: Start dispatcher.");
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError("[DispatcherManager] Create dispatcher failed.  Exception: {0}", e);
                }
            }
        }

#if HPCPACK
        /// <summary>
        /// Check if the Azure storage connection string is valid.
        /// </summary>
        private async Task ValidateConnectionString()
        {
            int sessionId = this.sharedData.BrokerInfo.SessionId;

            string sessionNode = await this.context.ResolveSessionLauncherNodeAsync();

            string certThrumbprint = await this.context.GetSSLThumbprint();

            string storageConnectionString = this.sharedData.BrokerInfo.AzureStorageConnectionString; // CommonClusterManagerHelper.GetAzureStorageConnectionString(scheduler);

            if (string.IsNullOrEmpty(storageConnectionString))
            {
                this.connectionStringValid = false;

                BrokerTracing.TraceError(
                    "[DispatcherManager].ValidateConnectionString: Azure storage connection string is missed.");

                // set job's progress message if Azure connection string is missed
                using (HpcSchedulerAdapterInternalClient client = new HpcSchedulerAdapterInternalClient(sessionNode, certThrumbprint))
                {
                    await client.SetJobProgressMessage(sessionId, SR.MissAzureStorageConnectionString);
                }
            }
            else
            {
                try
                {
                    this.azureQueueManager.StorageConnectionString = storageConnectionString;

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                    CloudQueueClient client = storageAccount.CreateCloudQueueClient();

                    client.ListQueues().Any<CloudQueue>();

                    this.connectionStringValid = true;
                }
                catch (StorageException e)
                {
                    string errorCode = BurstUtility.GetStorageErrorCode(e);

                    if (errorCode == StorageErrorCodeStrings.AuthenticationFailed)
                    {
                        this.connectionStringValid = false;

                        BrokerTracing.TraceError(
                            "[DispatcherManager].ValidateConnectionString: Access key in Azure storage connection string is invalid.");

                        // set job's progress message if Azure connection string is invalid
                        using (HpcSchedulerAdapterInternalClient client = new HpcSchedulerAdapterInternalClient(sessionNode, certThrumbprint))
                        {
                            await client.SetJobProgressMessage(sessionId, SR.InvalidAzureStorageConnectionString);
                        }
                    }
                    else if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.BadGateway)
                    {
                        this.connectionStringValid = false;

                        BrokerTracing.TraceError(
                            "[DispatcherManager].ValidateConnectionString: Account name in Azure storage connection string is invalid.");

                        // set job's progress message if Azure connection string is invalid
                        using (HpcSchedulerAdapterInternalClient client = new HpcSchedulerAdapterInternalClient(sessionNode, certThrumbprint))
                        {
                            await client.SetJobProgressMessage(sessionId, SR.InvalidAzureStorageConnectionString);
                        }
                    }
                    else
                    {
                        this.connectionStringValid = true;

                        BrokerTracing.TraceWarning(
                            "[DispatcherManager].ValidateConnectionString: Error occurs when check storage connection string, {0}", e);
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Remove a dispatcher from active dispatcher list
        /// </summary>
        /// <param name="taskId">dispatcher's task id</param>
        /// <returns>removed dispatcher</returns>
        private Dispatcher RemoveActiveDispatcher(string taskId)
        {
            if (!this.dispatcherDic.ContainsKey(taskId))
            {
                return null;
            }

            Dispatcher dispatcher = this.dispatcherDic[taskId];
            dispatcher.Failed -= this.DispatcherFailed;
            dispatcher.Connected -= this.DispatcherConnected;
            dispatcher.OnServiceInstanceFailedEvent -= this.ServiceInstanceFailed;

            BrokerTracing.TraceInfo("[DispatcherManager] Remove dispatcher {0} from the active dispatcher list.", taskId);
            this.dispatcherDic.Remove(taskId);

            this.sharedData.DispatcherCount = this.dispatcherDic.Count;
            return dispatcher;
        }

        /// <summary>
        /// Remove a dispatcher (info) from blocked dispatcher list
        /// </summary>
        /// <param name="taskId">task id related to the dispatcher</param>
        /// <returns>true on success, false on failure</returns>
        private DispatcherInfo RemoveBlockedDispatcher(string taskId)
        {
            DispatcherInfo dispatcherInfo = null;
            if (this.blockedDispatcherDic.TryGetValue(taskId, out dispatcherInfo))
            {
                this.blockedDispatcherDic.Remove(taskId);
                BrokerTracing.TraceInfo("[DispatcherManager] Dispatcher {0} removed from the blocked dispatcher list.", taskId);

                if (dispatcherInfo.BlockRetryCount <= 0)
                {
                    BrokerTracing.TraceVerbose("[DispatcherManager] Decrement youngBlockedDispatcherCount, task id={0}", dispatcherInfo.UniqueId);
                    this.youngBlockedDispatcherCount--;
                }
            }

            return dispatcherInfo;
        }

        /// <summary>
        /// Add a dispatcher to blocked dispatcher list
        /// </summary>
        /// <param name="dispatcherInfo">dispatcher info</param>
        private void AddBlockedDispatcher(DispatcherInfo dispatcherInfo)
        {
            try
            {
                dispatcherInfo.BlockTime = DateTime.Now;
                this.blockedDispatcherDic.Add(dispatcherInfo.UniqueId, dispatcherInfo);

                // if a dispatcher has never been blocked, take it as "young" blocked disaptcher.
                if (dispatcherInfo.BlockRetryCount <= 0)
                {
                    BrokerTracing.TraceVerbose("[DispatcherManager] Increment youngBlockedDispatcherCount, task id={0}, BlockRetryCount={1}", dispatcherInfo.UniqueId, dispatcherInfo.BlockRetryCount);
                    this.youngBlockedDispatcherCount++;
                }

                this.blockedDispatcherQueue.Enqueue(dispatcherInfo);
                if (this.blockedDispatcherQueue.Count == 1)
                {
                    BrokerTracing.TraceVerbose("[DispatcherManager] Block dispatcher: change unblock timer, task id = {0}", dispatcherInfo.UniqueId);
                    this.unblockTimer.Change(this.blockTimeSpan, TimeSpan.FromMilliseconds(-1));
                }
                BrokerTracing.TraceInfo("[DispatcherManager] Add dispatcher {0} into the blocked dispatcher list.", dispatcherInfo.UniqueId);
            }
            catch (ArgumentException)
            {
                BrokerTracing.TraceError("[DispatcherManager] Dispatcher {0} already exist in the blocked dispatcher list.", dispatcherInfo.UniqueId);
            }
        }

        /// <summary>
        /// Remove a list of dispatchers
        /// </summary>
        /// <param name="removeIdList">indicating remove id list</param>
        public void BatchRemoveDispatcher(IEnumerable<string> removeIdList)
        {
            List<Dispatcher> closeInstanceList = new List<Dispatcher>();
            lock (this.lockThis)
            {
                foreach (string taskId in removeIdList)
                {
                    Dispatcher dispatcher = this.RemoveActiveDispatcher(taskId);
                    if (dispatcher != null)
                    {
                        closeInstanceList.Add(dispatcher);
                        continue;
                    }

                    this.RemoveBlockedDispatcher(taskId);
                }

                this.sharedData.DispatcherCount = this.dispatcherDic.Count;
            }

            Dispatcher.BatchCloseDispatcher(closeInstanceList);
        }

        /// <summary>
        /// Dispatcher failed
        /// </summary>
        /// <param name="sender">indicating the failed dispatcher</param>
        /// <param name="e">indicating the event args (task id)</param>
        private void DispatcherFailed(object sender, EventArgs e)
        {
            Dispatcher dispatcher = sender as Dispatcher;
            Debug.Assert(dispatcher != null, "[DispatcherManager] sender of DispatcherFailed event must be a Dispatcher instance");
            this.BlockDispatcher(dispatcher.Info);
        }

        /// <summary>
        /// Dispatcher connected
        /// </summary>
        /// <param name="sender">indicating the disaptcher</param>
        /// <param name="e">indicating the event args</param>
        private void DispatcherConnected(object sender, EventArgs e)
        {
            Dispatcher dispatcher = sender as Dispatcher;
            Debug.Assert(dispatcher != null, "[DispatcherManager] sender of DispatcherConnected event must be a Dispatcher instance");

            // reset BlockRetryCount
            BrokerTracing.TraceVerbose("[DispatcherManager] Reset BlockRetryCount, task id={0}", dispatcher.Info.UniqueId);
            if (dispatcher.Info.BlockRetryCount != 0)
            {
                dispatcher.Info.BlockRetryCount = 0;
            }
        }

        /// <summary>
        /// Service instance is failed 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServiceInstanceFailed(object sender, ServiceInstanceFailedEventArgs e)
        {
            Dispatcher dispatcher = sender as Dispatcher;
            DispatcherInfo dispatcherInfo = dispatcher.Info;

            // Filter out duplicate failures
            lock (this.lockFailedDispatcherList)
            {
                if (this.failedDispatcherList.Contains(dispatcher.TaskId))
                {
                    return;
                }

                this.failedDispatcherList.Add(dispatcher.TaskId);
            }

            BrokerTracing.TraceError("[DispatcherManager] Service instance failed! Task id = {0}, node name = {1}", dispatcherInfo.UniqueId, dispatcherInfo.MachineName);

            // If the service is unavailable, just block the dispatcher. We cannot blacklist it because it may have just been preempted and the node is still good.
            // Furthermore the network may be temporarily out but the CN and its app install is fine
            if (e.Fault.Code == (int)SOAFaultCode.Service_Unreachable)
            {
                this.BlockDispatcher(dispatcherInfo);
            }

            // If the service cannot be initialized, remove and blacklist the dispatcher. We know the service cannot be loaded 
            else if (e.Fault.Code == (int)SOAFaultCode.Service_InitializeFailed)
            {
                if (this.RemoveDispatcher(dispatcherInfo.UniqueId, /*exitServiceHost =*/false, false))
                {
                    // Should use the machine virtual name for scheduler API to exclude a node.
                    this.monitor.BlacklistNode(SoaHelper.IsOnAzure() ? dispatcherInfo.MachineVirtualName : dispatcherInfo.MachineName).GetAwaiter().GetResult();
                }
            }

            // If the service host is preempted, remove the dispatcher.
            else if (e.Fault.Code == SOAFaultCode.Service_Preempted)
            {
                this.RemoveDispatcher(dispatcherInfo.UniqueId, true, true);
            }

            // There should be no other possible failure codes
            else
            {
                Debug.Assert(false, String.Format("Invalid fault code sent to ServiceInstanceFailed - {0}", e.Fault.Code));
            }
        }

        /// <summary>
        /// Callback to query the blocked dispatcher list, put expired item back to the active list
        /// </summary>
        /// <param name="state">null object</param>
        private void CallbackToQueryBlockedDispatcherList(object state)
        {
            List<DispatcherInfo> unblockList = new List<DispatcherInfo>();
            lock (this.lockThis)
            {
                BrokerTracing.TraceInfo("[DispatcherManager] Callback to query blocked dispatcher list.");
                Debug.Assert(this.blockedDispatcherQueue.Count > 0, "no blocked dispatcher");
                while (this.blockedDispatcherQueue.Count > 0)
                {
                    DispatcherInfo info = this.blockedDispatcherQueue.Peek();
                    TimeSpan elapsedTime = DateTime.Now.Subtract(info.BlockTime);
                    if (elapsedTime >= this.blockTimeSpan)
                    {
                        this.blockedDispatcherQueue.Dequeue();

                        if (null == this.RemoveBlockedDispatcher(info.UniqueId))
                        {
                            // if the task id doesn't have a match in blockedDispatcherDic, ignore it
                            continue;
                        }

                        // remove the task id from failedDispatcherList.
                        lock (this.lockFailedDispatcherList)
                        {
                            this.failedDispatcherList.Remove(info.UniqueId);
                        }

                        info.BlockRetryCount++;
                        BrokerTracing.TraceVerbose("[DispatcherManager] Increment BlockRetryCount: task id={0}, BlockRetryCount={1}", info.UniqueId, info.BlockRetryCount);
                        unblockList.Add(info);
                    }
                    else
                    {
                        BrokerTracing.TraceVerbose("[DispatcherManager] Unblock dispatcher: change unblock timer, task id = {0}", info.UniqueId);
                        this.unblockTimer.Change(this.blockTimeSpan - elapsedTime, TimeSpan.FromMilliseconds(-1));
                        break;
                    }
                }
            }

            foreach (DispatcherInfo info in unblockList)
            {
                BrokerTracing.TraceInfo("[DispatcherManager] Move dispatcher {0} from blocked list back to active.", info.UniqueId);
                this.NewDispatcherAsync(info).GetAwaiter().GetResult(); // TODO: change this to async
            }
        }

        /// <summary>
        /// Dispose the DispatcherManager class
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.unblockTimer.Dispose();
            }

            if (SoaCommonConfig.WithoutSessionLayer)
            {
                this.DeleteFromHosts().GetAwaiter().GetResult();
            }

            lock (this.lockThis)
            {
                BrokerTracing.TraceVerbose("[DispatcherManager].Dispose: Closing dispatchers...");

                Dispatcher.BatchCloseDispatcher(this.dispatcherDic.Values.ToList());

                BrokerTracing.TraceVerbose("[DispatcherManager].Dispose: Close all dispatchers succeeded.");
            }

            if (disposing)
            {
                if (this.azureQueueManager != null)
                {
                    try
                    {
                        this.azureQueueManager.Close();
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError("[DispatcherManager].Dispose: Closing AzureQueueManager failed, {0}", e);
                    }

                    this.azureQueueManager = null;
                }
            }
        }

        /// <summary>
        /// send Delete request to REST server for closing service host.
        /// </summary>
        private async Task DeleteFromHosts()
        {
            List<Dispatcher> dpcList = this.dispatcherDic.Values.ToList();
            Task[] tasks = new Task[dpcList.Count];
            int count = 0;
            foreach (Dispatcher dispatcher in dpcList)
            {
                string epr = dispatcher.MachineName;
                //clean appdomain from servicehost
                tasks[count++]=DeleteFromHostAsnyc(epr);
            }

            await Task.WhenAll(tasks);
        }

        private async Task DeleteFromHostAsnyc(string epr)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(prefix + epr + ":" + port + "/" + serverName + "/api/");
                //HTTP DELETE
                var result = await client.DeleteAsync(endPoint);
                BrokerTracing.TraceVerbose("[CloseSvcHost].result{0}:", result);
            }
        }

        /// <summary>
        /// Gets the idle dispatcher info list
        /// (1) Get all the idle unit.
        /// (2) Sort the units in with the minimum occupied node in front.
        /// </summary>
        /// <returns>an id list indicates all the idle dispatchers</returns>
        public List<DispatcherInfo> GetIdleDispatcherInfoInNodeOccupationOrder(out int totalActiveCapacityInCores, out int totalActiveCapacityInResourceUnits)
        {
            totalActiveCapacityInCores = 0;
            totalActiveCapacityInResourceUnits = 0;

            // key is the machine name in lower case
            Dictionary<string, MachineResourceInfo> machineInfoDic = new Dictionary<string, MachineResourceInfo>();

            string machineName;
            MachineResourceInfo info;
            lock (this.lockThis)
            {
                // calculate occupied unit count and idle unit count of each node
                foreach (Dispatcher dispatcher in this.dispatcherDic.Values)
                {
                    machineName = dispatcher.Info.MachineName.ToLowerInvariant();

                    // accumulate the occupied unit count
                    if (!machineInfoDic.TryGetValue(machineName, out info))
                    {
                        info = new MachineResourceInfo();
                        machineInfoDic[machineName] = info;
                    }

                    info.OccupiedUnitCount++;

                    // accumulate the idle unit count
                    if (dispatcher.ProcessingRequests == 0)
                    {
                        info.IdleDispatcherInfos.Add(dispatcher.Info);
                    }
                    else
                    {
                        totalActiveCapacityInCores += dispatcher.Capacity;
                        totalActiveCapacityInResourceUnits++;
                    }
                }

                // blocked dispatcher can't send message to the hpcservicehost
                // shrink temporarily blocked dispatchers, count them in the idle unit count
                // Notice: blockedDispatcherDic has no intersection with dispatcherDic
                foreach (DispatcherInfo dispatcherInfo in this.blockedDispatcherDic.Values)
                {
                    machineName = dispatcherInfo.MachineName.ToLowerInvariant();

                    // accumulate the occupied unit count
                    if (!machineInfoDic.TryGetValue(machineName, out info))
                    {
                        info = new MachineResourceInfo();
                        machineInfoDic[machineName] = info;
                    }

                    info.OccupiedUnitCount++;

                    // accumulate the idle unit count
                    info.IdleDispatcherInfos.Add(dispatcherInfo);
                }
            }

            // find out the candidate nodes, which contain idle resources
            List<MachineResourceInfo> candidates = new List<MachineResourceInfo>();
            foreach (KeyValuePair<string, MachineResourceInfo> pair in machineInfoDic)
            {
                info = pair.Value;
                if (info.IdleUnitCount > 0)
                {
                    candidates.Add(info);
                }
            }

            candidates.Sort();

            List<DispatcherInfo> result = new List<DispatcherInfo>();
            foreach (MachineResourceInfo machineInfo in candidates)
            {
                result.AddRange(machineInfo.IdleDispatcherInfos);
            }

            return result;
        }

        /// <summary>
        /// Get the current resource usage state of the dispatchers.
        /// </summary>
        /// <param name="averageCoresPerDispatcher">average cores per dispatcher</param>
        /// <param name="totalCores">total used cores count</param>
        /// <remarks>Consider calc when dispatchers are added\removed</remarks>
        public void GetCoreResourceUsageInformation(
            IEnumerable<string> tasksInInterest,
            out int averageCoresPerDispatcher,
            out int totalCores)
        {
            totalCores = 0;

            HashSet<string> tasksInInterestSet = null;
            if (tasksInInterest != null)
            {
                tasksInInterestSet = new HashSet<string>(tasksInInterest);
            }

            lock (this.lockThis)
            {
                var dispatcherInInterest = this.dispatcherDic.Values.Where(d => TaskInInterestUtil.IsTaskInInterest(tasksInInterestSet, d.TaskId)).ToList();
                var blockedDispatcherInInterest = this.blockedDispatcherDic.Values.Where(i => TaskInInterestUtil.IsTaskInInterest(tasksInInterestSet, i.UniqueId)).ToList();

                int dispatcherCount = dispatcherInInterest.Count + blockedDispatcherInInterest.Count;
                if (dispatcherCount == 0)
                {
                    averageCoresPerDispatcher = 0;
                    totalCores = 0;
                    return;
                }

                foreach (Dispatcher dispatcher in dispatcherInInterest)
                {
                    totalCores += dispatcher.Info.CoreCount;
                }

                foreach (DispatcherInfo dispatcherInfo in blockedDispatcherInInterest)
                {
                    totalCores += dispatcherInfo.CoreCount;
                }

                averageCoresPerDispatcher = (int)(totalCores / dispatcherCount);
            }
        }

        /// <summary>
        /// Retrieve the possible runaway tasks
        /// </summary>
        /// <param name="totalTaskIds">total task Ids in ascending order</param>
        /// <returns></returns>
        public IEnumerable<string> GetRunawayTasks(IEnumerable<string> totalTaskIds)
        {
            return totalTaskIds.Except(this.dispatcherDic.Keys).Except(this.blockedDispatcherDic.Keys).OrderBy(i => i);
        }
    }
}
