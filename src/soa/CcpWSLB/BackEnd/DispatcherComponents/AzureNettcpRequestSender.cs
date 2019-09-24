// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BackEnd.nettcp;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// This is an implementation of RequestSender for sending requests
    /// to Azure side service hosts via nettcp connection.
    /// </summary>
    internal class AzureNettcpRequestSender : RequestSender
    {
        /// <summary>
        /// Minimum sleep time for back-off.
        /// </summary>
        private const int MinSleepTimeInSecond = 1;

        /// <summary>
        /// Maximum sleep time for back-off.
        /// </summary>
        private const int MaxSleepTimeInSecond = 60;

        /// <summary>
        /// The sleep time for back-off.
        /// </summary>
        private int sleepTime = MinSleepTimeInSecond;

        /// <summary>
        /// It is a pool of clients connecting to the Azure broker proxy.
        /// </summary>
        private ProxyClientPool proxyClientPool;

        /// <summary>
        /// Binding info for communicating with backend service hosts
        /// </summary>
        private BindingData backendBindingData;

        /// <summary>
        /// Initializes a new instance of the AzureNettcpRequestSender class.
        /// </summary>
        /// <param name="epr">endpoint address of the target proxy or host</param>
        /// <param name="binding">backend binding</param>
        /// <param name="serviceOperationTimeout">service operation timeout of backend connection</param>
        /// <param name="proxyClientPool">connection pool</param>
        /// <param name="backendBindingData">binding data of the connection between proxy and host</param>
        /// <param name="dispatcher">dispatcher instance</param>
        public AzureNettcpRequestSender(EndpointAddress epr, Binding binding, int serviceOperationTimeout, ProxyClientPool proxyClientPool, BindingData backendBindingData, IDispatcher dispatcher)
            : base(epr, binding, serviceOperationTimeout, dispatcher)
        {
            this.proxyClientPool = proxyClientPool;

            this.backendBindingData = backendBindingData;
        }

        /// <summary>
        /// Gets or sets the AzureServiceClient.
        /// </summary>
        private AzureServiceClient Client
        {
            get
            {
                return this.IServiceClient as AzureServiceClient;
            }

            set
            {
                this.IServiceClient = value;
            }
        }

        /// <summary>
        /// Start the client.
        /// </summary>
        /// <param name="state">GetNextRequestState instance</param>
        public override void StartClient(GetNextRequestState state)
        {
            this.Client.AsyncStart(state, this.BackendServiceOperationTimeout);
        }

        /// <summary>
        /// Refresh the client.
        /// For burst, cleanup the connection pool if the exception occurs
        /// between broker and proxy.
        /// </summary>
        /// <param name="clientIndex">client index</param>
        /// <param name="exceptionIndirect">
        ///     is the exception from the connection between broker and proxy,
        ///     or between proxy and host
        /// </param>
        /// <param name="messageId">message Id</param>
        /// <returns>should increase the retry count or not</returns>
        public override async Task<bool> RefreshClientAsync(int clientIndex, bool exceptionIndirect, Guid messageId)
        {
            BrokerTracing.TraceWarning(
                BrokerTracing.GenerateTraceString(
                    "AzureNettcpRequestSender",
                    "RefreshClientAsync",
                    this.Dispatcher.TaskId,
                    clientIndex,
                    this.Client.ToString(),
                    messageId,
                    string.Format("exceptionIndirect = {0}", exceptionIndirect)));

            bool increaseRetryCount = false;

            if (exceptionIndirect)
            {
                this.Dispatcher.PassBindingFlags[clientIndex] = true;
                increaseRetryCount = true;
            }
            else
            {
                increaseRetryCount = this.Dispatcher.CleanupClient(this.Client);
                await this.CreateClientAsync(false, clientIndex).ConfigureAwait(false);
            }

            return increaseRetryCount;
        }

        /// <summary>
        /// Check if the client is ready for use.
        /// </summary>
        /// <remarks>
        /// If the client is not ready, create a client.
        /// The invoker of this method puts the request back to the queue and gets next request.
        /// </remarks>
        /// <param name="clientIndex">index of the client</param>
        /// <param name="messageId">message Id</param>
        /// <returns>
        /// Return true if the client is at opened state;
        /// Otherwise, return false.
        /// </returns>
        public override async Task<bool> ValidateClientAsync(int clientIndex, Guid messageId)
        {
            if (this.Client == null)
            {
                BrokerTracing.TraceWarning(
                    "[AzureNettcpRequestSender] .ValidateClient: Client is null, index = {0}, message id = {1}.",
                    clientIndex,
                    messageId);

                // the client is removed, so create a new one
                await this.CreateClientAsync(false, clientIndex).ConfigureAwait(false);
            }
            else
            {
                Debug.Assert(this.Client != null, "The client must be an AzureServiceClient.");

                CommunicationState state = this.Client.ServiceClient.State;

                BrokerTracing.TraceVerbose(
                    "[AzureNettcpRequestSender] .ValidateClient: Client is {0}, index = {1}, message id = {2}.",
                    this.Client,
                    clientIndex,
                    messageId);

                if (state == CommunicationState.Opened)
                {
                    // reset the sleep time
                    this.sleepTime = MinSleepTimeInSecond;

                    // the client is valid, so can process the request
                    return true;
                }
                else if (state == CommunicationState.Created || state == CommunicationState.Opening)
                {
                    // the client is in a good state but not ready
                }
                else
                {
                    // the client is in a bad state, so create a new one
                    await this.CreateClientAsync(false, clientIndex).ConfigureAwait(false);
                }
            }

            // #19916, it is an optimization here to wait 1 second instead of returning
            // false immediately. Otherwise, the request is put back to the queue and
            // is grabbed by another dispatcher, which very possibly has an invalid
            // client as well especially when the connection pool is rebuilding.
            // Previous logic is to wait 1 sec before re-sending request, but if
            // broker can't connect to Azure service for 10+ minutes, there are
            // hundreds of dispatch history traces. Change to exponential back-off
            // with upper limit MaxSleepTimeInSecond.
            BrokerTracing.TraceVerbose(
                "[AzureNettcpRequestSender] .ValidateClient: Wait {0} seconds for the client to get ready, message id = {1}",
                this.sleepTime,
                messageId);

            // TODO: Refactor this when we have DelayHandler.
            // (1) should not block thread.
            // (2) keep the exponential backoff as is.
            Thread.Sleep(TimeSpan.FromSeconds(this.sleepTime));

            this.sleepTime = Math.Min(this.sleepTime * 2, MaxSleepTimeInSecond);

            return false;
        }

        /// <summary>
        /// Get AzureServiceClient from client pool.
        /// </summary>
        protected override Task CreateClientAsync()
        {
            lock (this.proxyClientPool)
            {
                this.Client = this.proxyClientPool.GetProxyClient();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Remove the client from pool and close it.
        /// </summary>
        protected override void CloseClient()
        {
            if (this.Client == null)
            {
                BrokerTracing.TraceWarning("[AzureNettcpRequestSender] .CloseClient: Client is null.");
                return;
            }

            BrokerTracing.TraceVerbose(
                "[AzureNettcpRequestSender] .CloseClient: Close client {0}",
                this.Client.ToString());

            bool exist = false;

            lock (this.proxyClientPool)
            {
                exist = this.proxyClientPool.RemoveProxyClient(this.Client);
            }

            if (exist)
            {
                this.Client.AsyncClose();
            }
        }

        /// <summary>
        /// Remove the client from pool if no one refers it.
        /// </summary>
        public override void ReleaseClient()
        {
            if (this.Client == null)
            {
                BrokerTracing.TraceWarning("[AzureNettcpRequestSender] .ReleaseClient: Client is null.");
                return;
            }

            BrokerTracing.TraceVerbose(
                "[AzureNettcpRequestSender] .ReleaseClient: Release client {0}",
                this.Client.ToString());

            bool exist = false;

            lock (this.proxyClientPool)
            {
                exist = this.proxyClientPool.ReleaseProxyClient(this.Client);
            }

            if (exist)
            {
                this.Client.AsyncClose();
            }
        }

        /// <summary>
        /// Prepare the message for sending: add some information into the
        /// message headers. Broker proxy will generate epr of the Azure
        /// svchost based on these information.
        /// </summary>
        /// <param name="message">request message</param>
        /// <param name="dispatchId">indicating the dispatch id</param>
        /// <param name="needBinding">
        /// add binding data to the message header or not
        /// </param>
        protected override void PrepareMessage(Message message, Guid dispatchId, bool needBinding)
        {
            ServiceTaskDispatcherInfo serviceTaskDispatcherInfo = this.Dispatcher.Info as ServiceTaskDispatcherInfo;

            if (serviceTaskDispatcherInfo != null)
            {
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderMachineName, Constant.HpcHeaderNS, this.Dispatcher.Info.MachineName));
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderCoreId, Constant.HpcHeaderNS, serviceTaskDispatcherInfo.FirstCoreId));
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderJobId, Constant.HpcHeaderNS, serviceTaskDispatcherInfo.JobId));
                message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderTaskId, Constant.HpcHeaderNS, serviceTaskDispatcherInfo.TaskId));

                if (needBinding)
                {
                    // carry backend binding info over message header
                    message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderBinding, Constant.HpcHeaderNS, this.backendBindingData));

                    // also carry serviceOperationTimeout value over message header
                    message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderServiceOperationTimeout, Constant.HpcHeaderNS, this.BackendServiceOperationTimeout));
                }
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
