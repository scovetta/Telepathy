// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using Microsoft.Hpc.BrokerProxy;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.ServiceBroker.Common;

    /// <summary>
    /// Dispatch messages to broker proxy in Windows Azure
    /// </summary>
    internal class AzureHttpsDispatcher : Dispatcher
    {
        /// <summary>
        /// A string that used to mark an exception as indirect.
        /// Note: "indirect" exceptions are exceptions that carried in ProxyFault and passed back by broker proxy
        /// </summary>
        private const string IndirectExceptionMark = "IndirectServiceHostException";

        /// <summary>
        /// Binding info for communicating with backend service hosts
        /// </summary>
        private BindingData backendBindingData;

        /// <summary>
        /// Instance of the AzureQueueManager.
        /// </summary>
        private AzureQueueManager azureQueueManager;

        /// <summary>
        /// Azure service name.
        /// </summary>
        private string azureServiceName;

        /// <summary>
        /// Response storage name.
        /// </summary>
        private string responseStorageName;

        /// <summary>
        /// Initializes a new instance of the AzureHttpsDispatcher class.
        /// </summary>
        /// <param name="azureQueueManager">AzureQueueManager instance</param>
        /// <param name="info">indicating the dispatcher info</param>
        /// <param name="binding">binding information</param>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="queueFactory">indicating the queue factory</param>
        /// <param name="schedulerAdapterClientFactory">SchedulerAdapterClientFactory instance</param>
        /// <param name="dispatcherIdle">set when the dispatcher enters idle status</param>
        public AzureHttpsDispatcher(AzureQueueManager azureQueueManager, DispatcherInfo info, Binding binding, SharedData sharedData, BrokerObserver observer, BrokerQueueFactory queueFactory, SchedulerAdapterClientFactory schedulerAdapterClientFactory, AutoResetEvent dispatcherIdle)
            : base(info, ProxyBinding.BrokerProxyBinding, sharedData, observer, queueFactory, schedulerAdapterClientFactory, dispatcherIdle)
        {
            AzureDispatcherInfo azureDispatcherInfo = info as AzureDispatcherInfo;

            this.azureServiceName = azureDispatcherInfo.AzureServiceName;

            this.azureQueueManager = azureQueueManager;

            this.azureQueueManager.CreateRequestStorage(this.azureServiceName);

            this.responseStorageName =
                this.azureQueueManager.Start(azureDispatcherInfo.JobId, azureDispatcherInfo.RequeueCount);

            // Update backend binding's maxMessageSize settings with global maxMessageSize if its enabled (> 0)
            int maxMessageSize = sharedData.ServiceConfig.MaxMessageSize;
            if (maxMessageSize > 0)
            {
                BindingHelper.ApplyMaxMessageSize(binding, maxMessageSize);
            }

            this.backendBindingData = new BindingData(binding);
        }

        /// <summary>
        /// Create AzureHttpsRequestSender.
        /// </summary>
        /// <returns>AzureHttpsRequestSender instance</returns>
        protected override RequestSender CreateRequestSender()
        {
            return new AzureHttpsRequestSender(
                this.Epr,
                this.BackendBinding,
                this.ServiceOperationTimeout,
                this.backendBindingData,
                this.azureQueueManager,
                this.azureServiceName,
                this.responseStorageName,
                this);
        }

        protected override ResponseReceiver CreateResponseReceiver()
        {
            return new AzureResponseReceiver(this);
        }

        /// <summary>
        /// Check if need recreate a service client on receiving an exception.
        /// If the exception is an "indirect" exception, we don't need to
        /// recreate the channel between dispatcher and broker proxy.
        /// </summary>
        /// <param name="e">target exception</param>
        /// <returns>
        /// Return true if the exception has IndirectExceptionMark.
        /// </returns>
        protected override bool IsExceptionIndirect(Exception e)
        {
            BrokerTracing.TraceVerbose("[AzureDispatcher].IsExceptionIndirect e: {0}, e.Source: {1}", e, e.Source);
            return e.Source == IndirectExceptionMark;
        }

        /// <summary>
        /// If the exception is CommunicationException or from the connection between the proxy
        /// and host, it might be caused by the job preemption or cancellation
        /// </summary>
        /// <param name="e">exception received by azure dispatcher</param>
        /// <returns>should check the error code or not</returns>
        protected override bool ShouldCheckTaskErrorCode(Exception e)
        {
            BrokerTracing.TraceVerbose("[AzureDispatcher].ShouldCheckTaskErrorCode e: {0}, e.Source: {1}", e, e.Source);
            return e is CommunicationException
                && e.Source == IndirectExceptionMark
                && this.SharedData.ServiceConfig.EnableMessageLevelPreemption;
        }
    }
}
