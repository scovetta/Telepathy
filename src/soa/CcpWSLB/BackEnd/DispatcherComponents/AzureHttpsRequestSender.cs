// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Hpc.BrokerProxy;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// This is an implementation of RequestSender for sending requests
    /// to Azure side service hosts via Azure storage.
    /// </summary>
    internal class AzureHttpsRequestSender : RequestSender
    {
        /// <summary>
        /// Instance of the AzureQueueManager.
        /// </summary>
        private IAzureQueueManager azureQueueManager;

        /// <summary>
        /// Azure service name.
        /// </summary>
        private string azureServiceName;

        /// <summary>
        /// Response storage name.
        /// </summary>
        private string responseStorageName;

        /// <summary>
        /// Binding info for communicating with backend service hosts
        /// </summary>
        private BindingData backendBindingData;

        public AzureHttpsRequestSender(EndpointAddress epr, Binding binding, int serviceOperationTimeout, BindingData backendBindingData, IAzureQueueManager azureQueueManager, string azureServiceName, string responseStorageName, IDispatcher dispatcher)
            : base(epr, binding, serviceOperationTimeout, dispatcher)
        {
            this.backendBindingData = backendBindingData;

            this.azureQueueManager = azureQueueManager;

            this.azureServiceName = azureServiceName;

            this.responseStorageName = responseStorageName;
        }

        private AzureHttpsServiceClient Client
        {
            get
            {
                return this.IServiceClient as AzureHttpsServiceClient;
            }

            set
            {
                this.IServiceClient = value;
            }
        }

        /// <summary>
        /// Create an AzureHttpsServiceClient instance.
        /// </summary>
        protected override Task CreateClientAsync()
        {
            this.Client = new AzureHttpsServiceClient(this.azureQueueManager, this.azureServiceName, this.responseStorageName);
            return Task.CompletedTask;
        }

        public override void StartClient(GetNextRequestState state)
        {
            if (state != null)
            {
                state.Invoke();
            }
        }

        /// <summary>
        /// Refresh the client.
        /// No need to refresh the AzureQueueClient. The client of CloudQueue
        /// doesn't hold connection, it is always valid.
        /// </summary>
        /// <param name="clientIndex">client index</param>
        /// <param name="exceptionIndirect">
        ///     true: exception is from the connection between proxy and host,
        ///     need to increase message retry count
        ///     false: exception is from the connection between broker and proxy.
        /// </param>
        /// <param name="messageId">message Id</param>
        /// <returns>should increase the retry count or not</returns>
        public override Task<bool> RefreshClientAsync(int clientIndex, bool exceptionIndirect, Guid messageId)
        {
            // It is not needed for Azure storage client.
            return Task.FromResult(true);
        }

        protected override void CloseClient()
        {
            if (this.Client == null)
            {
                BrokerTracing.TraceWarning("[AzureHttpsRequestSender] .CloseClient: Client is null.");
                return;
            }

            this.Client.Close();
        }

        public override void ReleaseClient()
        {
            this.CloseClient();
        }

        /// <summary>
        /// Prepare the message for sending: add some information into the
        /// message headers. Broker proxy will generate endpoint of the Azure
        /// service host based on these information.
        /// </summary>
        /// <param name="message">request message</param>
        /// <param name="dispatchId">indicating the dispatch id</param>
        /// <param name="needBinding">add binding data to the message header or not</param>
        protected override void PrepareMessage(Message message, Guid dispatchId, bool needBinding)
        {
            AzureDispatcherInfo azureDispatcherInfo = this.Dispatcher.Info as AzureDispatcherInfo;

            if (azureDispatcherInfo != null)
            {
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderMachineName, Constant.HpcHeaderNS, this.Dispatcher.Info.MachineName));
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderCoreId, Constant.HpcHeaderNS, azureDispatcherInfo.FirstCoreId));
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderJobId, Constant.HpcHeaderNS, azureDispatcherInfo.JobId));
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderRequeueCount, Constant.HpcHeaderNS, azureDispatcherInfo.RequeueCount));
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderTaskId, Constant.HpcHeaderNS, azureDispatcherInfo.TaskId));

                // Always pass binding info for the HttpsAzureDispatcher.
                // carry backend binding info over message header
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderBinding, Constant.HpcHeaderNS, this.backendBindingData));

                // also carry serviceOperationTimeout value over message header
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderServiceOperationTimeout, Constant.HpcHeaderNS, this.BackendServiceOperationTimeout));
            }
            else
            {
                EprDispatcherInfo eprDispatcherInfo = this.Dispatcher.Info as EprDispatcherInfo;
                if (eprDispatcherInfo != null)
                {
                    // TODO: support Azure service host in in-proc broker
                }
            }

            // following method must be called, otherwise the dispatcher Id is not set
            // to the message header and the SoaDiagUserTrace can not be recorded.
            base.PrepareMessage(message, dispatchId, needBinding);
        }
    }
}
