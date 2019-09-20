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

    /// <summary>
    /// This is an implementation of RequestSender for sending requests
    /// to on-premise service hosts.
    /// </summary>
    internal class OnPremiseRequestSender : RequestSender
    {
        public OnPremiseRequestSender(EndpointAddress epr, Binding binding, int serviceOperationTimeout, IDispatcher dispatcher,
            int serviceInitializationTimeout, int initEndpointNotFoundWaitPeriod)
            : base(epr, binding, serviceOperationTimeout, dispatcher)
        {
            this.ServiceInitializationTimeout = serviceInitializationTimeout;
            this.InitEndpointNotFoundWaitPeriod = initEndpointNotFoundWaitPeriod;
        }

        protected ServiceClient Client
        {
            get
            {
                return this.IServiceClient as ServiceClient;
            }

            set
            {
                this.IServiceClient = value;
            }
        }

        protected int ServiceInitializationTimeout
        {
            get;
            set;
        }

        protected int InitEndpointNotFoundWaitPeriod
        {
            get;
            set;
        }

        /// <summary>
        /// Create a new ServiceClient instance
        /// </summary>
        protected override async Task CreateClientAsync()
        {
            DateTime start = DateTime.Now;

            // WCF BUG:
            // We need a sync call Open on ClientBase<> to get the correct impersonation context
            // this is a WCF bug, so we create the client on a dedicated thread.
            // If we call it in thread pool thread, it will block the thread pool thread for a while
            // and drops the performance.
            // The code in Service Job Monitor.TaskStateChanged guaranteed the
            // constructor is called in a dedicated thread.

            do
            {
                try
                {
                    this.Client = new ServiceClient(
                        this.BackendBinding,
                        this.EndpointReferenceAddress);
                    await this.Client.InitAsync().ConfigureAwait(false);

                    return;
                }
                catch (EndpointNotFoundException)
                {
                    int timeRemaining = this.ServiceInitializationTimeout - (int)(DateTime.Now - start).TotalMilliseconds;
                    // retry timeout, log error.
                    BrokerTracing.TraceEvent(
                         TraceEventType.Error,
                         0,
                         "[OnPremiseRequestSender].CreateClientAsync, TaskID = {0}, EndpointNotFoundException, timeRemaining {1}",
                         this.TaskId,
                         timeRemaining);

                    if (timeRemaining <= 0)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(Math.Min(this.InitEndpointNotFoundWaitPeriod, timeRemaining));
                    }
                }
            }
            while (true);
        }

        public override void StartClient(GetNextRequestState state)
        {
            this.Client.Start(state, this.BackendServiceOperationTimeout);
        }

        protected override void CloseClient()
        {
            if (this.Client == null)
            {
                BrokerTracing.TraceWarning("[OnPremiseRequestSender] .CloseClient: Client is null.");
                return;
            }

            this.Client.AsyncClose();
        }

        public override void ReleaseClient()
        {
            this.CloseClient();
        }

        /// <summary>
        /// Refresh the client.
        /// For on-premise cluster, just create a new client.
        /// </summary>
        /// <param name="clientIndex">client index</param>
        /// <param name="exceptionIndirect">
        ///     it is false for the on-premise cluster
        /// </param>
        /// <param name="messageId">message Id</param>
        /// <returns>should increase the retry count or not</returns>
        public override async Task<bool> RefreshClientAsync(int clientIndex, bool exceptionIndirect, Guid messageId)
        {
            BrokerTracing.TraceError(
                BrokerTracing.GenerateTraceString(
                    "OnPremiseRequestSender",
                    "RefreshClientAsync",
                    this.TaskId,
                    clientIndex,
                    this.Client.ToString(),
                    messageId,
                    string.Format("exceptionIndirect = {0}", exceptionIndirect)));

            await this.CreateClientAsync(false, clientIndex).ConfigureAwait(false);

            return true;
        }
    }
}
