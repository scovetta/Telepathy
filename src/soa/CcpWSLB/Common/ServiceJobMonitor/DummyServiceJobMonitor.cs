// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common.ServiceJobMonitor
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Common.TelepathyContext;
    using Microsoft.Telepathy.ServiceBroker.BackEnd;
    using Microsoft.Telepathy.ServiceBroker.Common.SchedulerAdapter;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Interface;

    class DummyServiceJobMonitor : ServiceJobMonitorBase
    {
        /// <summary>
        /// Initializes a new instance of the DummyServiceJobMonitor class
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="stateManager">indicating the state manager</param>
        public DummyServiceJobMonitor(SharedData sharedData, BrokerStateManager stateManager, NodeMappingData nodeMappingData, ITelepathyContext context) : base(
            sharedData,
            stateManager,
            nodeMappingData,
            context)
        {
        }

        /// <summary>
        /// Override Start method and start dummy service job monitor
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="dispatcherManager"></param>
        /// <param name="observer"></param>
        /// <returns></returns>
        public override async Task Start(SessionStartInfoContract startInfo, DispatcherManager dispatcherManager, BrokerObserver observer)
        {
            BrokerTracing.TraceVerbose("[ServiceJobMonitor].Start: Enter");

            this.dispatcherManager = dispatcherManager;
            this.observer = observer;

            this.gracefulPreemptionHandler = new GracefulPreemptionHandler(this.dispatcherManager, this.sharedData.BrokerInfo.SessionId, taskId => this.FinishTask(taskId, isRunAwayTask: true));

            this.schedulerAdapterClientFactory = new SchedulerAdapterClientFactory(this.sharedData, this, dispatcherManager, this.context);

            if (this.sharedData.Config.Monitor.AllocationAdjustInterval != Timeout.Infinite && this.sharedData.StartInfo.EprList == null)
            {
                BrokerTracing.TraceVerbose("[ServiceJobMonitor].Start: start the allocation adjust thread, interval={0}", this.sharedData.Config.Monitor.AllocationAdjustInterval);

                this.allocationAdjustThread.Start();
            }

            await SvcHostRestModule.OpenSvcHostsAsync(this.sharedData.BrokerInfo.SessionId, startInfo, ((ISchedulerNotify)this).TaskStateChanged);
        }
    }
}