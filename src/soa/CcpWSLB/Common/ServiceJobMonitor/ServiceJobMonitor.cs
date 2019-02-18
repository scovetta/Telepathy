//-----------------------------------------------------------------------
// <copyright file="ServiceJobMonitor.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Monitor service job</summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.RESTServiceModel;
    using Microsoft.Hpc.ServiceBroker.BackEnd;
    using Microsoft.Hpc.ServiceBroker.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Monitor the service job
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class ServiceJobMonitor : ServiceJobMonitorBase
    {      
        /// <summary>
        /// Initializes a new instance of the ServiceJobMonitor class
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="stateManager">indicating the state manager</param>
        public ServiceJobMonitor(SharedData sharedData, BrokerStateManager stateManager, NodeMappingData nodeMappingData, IHpcContext context)
            :base(sharedData, stateManager, nodeMappingData, context)
        {       
        }

        /// <summary>
        /// override the Start Method in internal service job monitor
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="dispatcherManager"></param>
        /// <param name="observer"></param>
        /// <returns></returns>
        public override async Task Start(SessionStartInfoContract startInfo, DispatcherManager dispatcherManager, BrokerObserver observer)
        {
            await this.Start(dispatcherManager, observer);
        }

        /// <summary>
        /// Start the service job monitor
        /// </summary>
        /// <param name="dispatcherManager">indicating dispatcher manager</param>
        /// <param name="observer">indicating the observer</param>
        public async Task Start(DispatcherManager dispatcherManager, BrokerObserver observer)
        {
            BrokerTracing.TraceVerbose("[ServiceJobMonitor].Start: Enter");

            this.dispatcherManager = dispatcherManager;
            this.observer = observer;

            this.gracefulPreemptionHandler = new GracefulPreemptionHandler(
                this.dispatcherManager,
                this.sharedData.BrokerInfo.SessionId,
                taskId => this.FinishTask(taskId, isRunAwayTask: true));

            this.schedulerAdapterClientFactory = new SchedulerAdapterClientFactory(sharedData, this, dispatcherManager, this.context);

            try
            {
                // Bug 14035: Need to call GetSchedulerAdapterClient() to invoke RegisterJob() operation
                await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync();
            }
            catch (Exception e)
            {
                BrokerTracing.TraceVerbose("[ServiceJobMonitor].Start: Exception: {0}", e.ToString());
                throw;
            }

            this.trigger = new RepeatableCallbackTrigger();
            WaitCallback updateStatusWaitCallback = new ThreadHelper<object>(new WaitCallback(this.UpdateStatus)).CallbackRoot;
            WaitCallback loadSampleWaitCallback = new ThreadHelper<object>(new WaitCallback(this.LoadSample)).CallbackRoot;
            WaitCallback schedulerDelegationTimeoutCallback = new ThreadHelper<object>(new WaitCallback(this.SchedulerDelegationTimeout)).CallbackRoot;
            WaitCallback finishTaskCallback = new ThreadHelper<object>(new WaitCallback(this.FinishTasksThread)).CallbackRoot;

            BrokerTracing.TraceVerbose(
                "[ServiceJobMonitor].Start: Register updateStatusWaitCallback, interval={0}",
                this.sharedData.Config.Monitor.StatusUpdateInterval);
            this.trigger.RegisterCallback(TimeSpan.FromMilliseconds(this.sharedData.Config.Monitor.StatusUpdateInterval), TimeSpan.FromSeconds(FirstUpdateStatusSeconds), updateStatusWaitCallback, "TIMERCALLBACK");

            BrokerTracing.TraceVerbose(
                "[ServiceJobMonitor].Start: Register loadSampleWaitCallback, interval={0}",
                this.sharedData.Config.Monitor.LoadSamplingInterval);
            this.trigger.RegisterCallback(TimeSpan.FromMilliseconds(this.sharedData.Config.Monitor.LoadSamplingInterval), loadSampleWaitCallback, null);

            this.trigger.RegisterCallback(TimeSpan.FromMilliseconds(FinishTasksInterval), TimeSpan.FromMilliseconds(FinishTasksInterval), finishTaskCallback, null);

            // Get value of automaticShrinkEnabled cluster param
            this.automaticShrinkEnabled = this.sharedData.BrokerInfo.AutomaticShrinkEnabled;

            // If the allocationAdjustInterval setting is not infinite, start timer to adjust the allocation as appropriate
            // Bug 10492: Disable grow/shrink in debug mode
            if (this.sharedData.Config.Monitor.AllocationAdjustInterval != Timeout.Infinite && this.sharedData.StartInfo.EprList == null)
            {
                BrokerTracing.TraceVerbose(
                    "[ServiceJobMonitor].Start: start the allocation adjust thread, interval={0}",
                    this.sharedData.Config.Monitor.AllocationAdjustInterval);

                this.allocationAdjustThread.Start();
            }

            this.trigger.Start();

            this.schedulerNotifyTimeoutManager.RegisterTimeout(TimeoutPeriodFromSchedulerDelegationEvent, schedulerDelegationTimeoutCallback, null);

            // TODO: hack! rafactor this
            var startInfo = this.sharedData.StartInfo;
            if (startInfo != null)
            {
                List<TaskInfo> taskInfoList = new List<TaskInfo>();
                Task<TaskInfo>[] tl = new Task<TaskInfo>[startInfo.IpAddress.Length];

                for (int i = 0; i < startInfo.IpAddress.Length; i++)
                {
                    tl[i] = this.OpenSvcHostsAsync(i, startInfo.IpAddress[i], startInfo.RegPath, startInfo.ServiceName, startInfo.Environments, startInfo.DependFilesStorageInfo);
                }

                await Task.WhenAll(tl);
                for (int i = 0; i < tl.Length; i++)
                {
                    TaskInfo ti = tl[i].Result;
                    if (ti != null)
                    {
                        taskInfoList.Add(ti);
                    }
                }

                Debug.Assert(taskInfoList.Count != 0, "No available endpoint.");
                await ((ISchedulerNotify)this).TaskStateChanged(taskInfoList);
            }

            BrokerTracing.TraceVerbose("[ServiceJobMonitor].Start: Exit");
        }

        // TODO: hack. refactor this
        private const int taskIdStart = 9300;
        private const string prefix = "http://";
        private const int port = 80;
        private const string serverName = "SvcHost";
        private const string endPoint = "svchostserver";

        /// <summary>
        /// Asyn open service host.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="ipAddress"></param>
        /// <param name="regPath"></param>
        /// <param name="svcName"></param>
        /// <returns></returns>
        private async Task<TaskInfo> OpenSvcHostsAsync(int num, string ipAddress, string regPath, string svcName, Dictionary<string, string> environment, Dictionary<string, string> dependFilesInfo)
        {
            TaskInfo ti = new TaskInfo();
            ti.Id = taskIdStart + num;
            ti.Capacity = 1;
            ti.FirstCoreIndex = 3;
            ti.Location = Scheduler.Session.Data.NodeLocation.OnPremise;
            ti.MachineName = ipAddress;
            ti.State = Scheduler.Session.Data.TaskState.Dispatching;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(prefix + ipAddress + ":" + port + "/" + serverName + "/api/");
                //HTTP POST
                var serviceInfo = new ServiceInfo(this.sharedData.BrokerInfo.SessionId, ti.Id, ti.FirstCoreIndex, regPath + "\\", svcName + ".config", environment, dependFilesInfo);
                try
                {
                    var result = await client.PostAsJsonAsync<ServiceInfo>(endPoint, serviceInfo);
                    BrokerTracing.TraceVerbose("[OpenSvcHost].result:{0}", result);
                    if (result.IsSuccessStatusCode)
                        return ti;
                    else
                        return null;
                }
                catch (Exception e) //for the romote host closed 
                {
                    BrokerTracing.TraceVerbose("[OpenSvcHost].post: Exception: {0}", e.ToString());
                    return null;
                }
            }
        }

    }
}
