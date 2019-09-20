// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common.ServiceJobMonitor
{
    using System;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Telepathy.ServiceBroker.BackEnd;
    using Microsoft.Telepathy.ServiceBroker.Common.SchedulerAdapter;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Interface;

    using TelepathyCommon.HpcContext;

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
        public ServiceJobMonitor(SharedData sharedData, BrokerStateManager stateManager, NodeMappingData nodeMappingData, ITelepathyContext context)
            : base(sharedData, stateManager, nodeMappingData, context)
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

            this.schedulerAdapterClientFactory = new SchedulerAdapterClientFactory(this.sharedData, this, dispatcherManager, this.context);

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

            if (this.sharedData.StartInfo.IsNoSession)
            {
                await this.OpenPreDefinedServiceHosts(); 
            }

            BrokerTracing.TraceVerbose("[ServiceJobMonitor].Start: Exit");
        }

        private async Task OpenPreDefinedServiceHosts()
        {
            // TODO: hack! rafactor this
            var startInfo = this.sharedData.StartInfo;
            if (startInfo != null)
            {
                await SvcHostRestModule.OpenSvcHostsAsync(this.sharedData.BrokerInfo.SessionId, startInfo, ((ISchedulerNotify)this).TaskStateChanged);
            }
        }
    }
}
