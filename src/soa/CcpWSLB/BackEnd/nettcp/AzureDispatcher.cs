// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.nettcp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    using Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.Common.SchedulerAdapter;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Dispatch messages to broker proxy in Windows Azure
    /// </summary>
    internal class AzureDispatcher : Dispatcher
    {
        #region private fields

        /// <summary>
        /// A string that used to mark an exception as indirect.
        /// Note: "indirect" exceptions are exceptions that carried in ProxyFault and passed back by broker proxy
        /// </summary>
        private const string IndirectExceptionMark = "IndirectServiceHostException";

        /// <summary>
        /// the time interval to wait between two consequtive proxy client check.
        /// </summary>
        private const int PingIntervalInSecond = 25;

        /// <summary>
        /// Minimum sleep time for backoff.
        /// </summary>
        private const int MinSleepTimeInSecond = 1;

        /// <summary>
        /// Maximum sleep time for backoff.
        /// </summary>
        private const int MaxSleepTimeInSecond = 60;

        /// <summary>
        /// There are a number of connections established between broker and each broker proxy, managed by ProxyClientPool
        /// Key: broker proxy uri string (different subscriptions have different uri)
        /// Value: pool of proxy service client
        /// </summary>
        private static Dictionary<EndpointAddress, ProxyClientPool> ProxyClientPoolDic = new Dictionary<EndpointAddress, ProxyClientPool>();

        /// <summary> lock object for ProxyClientPoolDic</summary>
        private static object LockProxyClientPoolDic = new object();

        /// <summary>
        /// the timer that checks proxy clients (and underlying connections) periodically
        /// Note: Azure load balancer terminates the idle connection after about 5 minutes.
        /// TODO: TCP KeepAlive may make this unnecessary, but it seems not work per testing.
        /// </summary>
        private static Timer CheckProxyClientTimer = new Timer(
            CheckProxyClients,
            null,
            TimeSpan.FromSeconds(PingIntervalInSecond),
            TimeSpan.FromSeconds(PingIntervalInSecond));

        /// <summary> ProxyClientPool this AzureDispatcher instance corresponds to. </summary>
        private ProxyClientPool proxyClientPool;

        /// <summary> Binding info for communicating with backend service hosts</summary>
        private BindingData backendBindingData;

        #endregion

        /// <summary>
        /// Initializes a new instance of the AzureDispatcher class
        /// </summary>
        /// <param name="info">indicating the dispatcher info</param>
        /// <param name="binding">binding information</param>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="queueFactory">indicating the queue factory</param>
        /// <param name="dispatcherIdle">set when the dispatcher enters idle status</param>
        public AzureDispatcher(DispatcherInfo info, Binding binding, SharedData sharedData, BrokerObserver observer, BrokerQueueFactory queueFactory, SchedulerAdapterClientFactory schedulerAdapterClientFactory, AutoResetEvent dispatcherIdle)
            : base(info, ProxyBinding.BrokerProxyBinding, sharedData, observer, queueFactory, schedulerAdapterClientFactory, dispatcherIdle)
        {
            // Initialize proxy client pool
            this.proxyClientPool = this.AttachProxyClientPool(this.Epr, sharedData.Config.LoadBalancing.MaxConnectionCountPerAzureProxy);

            // Update backend binding's maxMessageSize settings with global maxMessageSize if its enabled (> 0)
            int maxMessageSize = sharedData.ServiceConfig.MaxMessageSize;
            if (maxMessageSize > 0)
            {
                BindingHelper.ApplyMaxMessageSize(binding, maxMessageSize);
            }

            this.backendBindingData = new BindingData(binding);
        }

        /// <summary>
        /// Create AzureNettcpRequestSender.
        /// </summary>
        /// <returns>AzureNettcpRequestSender instance</returns>
        protected override RequestSender CreateRequestSender()
        {
            return new AzureNettcpRequestSender(
                this.Epr,
                this.BackendBinding,
                this.ServiceOperationTimeout,
                this.proxyClientPool,
                this.backendBindingData,
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
        protected override bool IsExceptionIndirect(Exception e)
        {
            BrokerTracing.TraceVerbose("[AzureDispatcher] .IsExceptionIndirect: Source is {0}, {1}", e.Source, e);
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
            BrokerTracing.TraceVerbose("[AzureDispatcher] .ShouldCheckTaskErrorCode: Source is {0}, {1}", e.Source, e);
            return (e is CommunicationException && e.Source == IndirectExceptionMark && this.SharedData.ServiceConfig.EnableMessageLevelPreemption);
        }

        /// <summary>
        /// Clear the client pool and close all the connections in it.
        /// </summary>
        /// <remarks>
        /// This method has high cost. We need to be pretty sure that the exception
        /// happens on the connection between broker and proxy, then call this method.
        /// </remarks>
        /// <returns>
        /// is the specified client in the pool
        /// </returns>
        public override bool CleanupClient(IService client)
        {
            AzureServiceClient serviceClient = client as AzureServiceClient;
            Debug.Assert(serviceClient != null);

            BrokerTracing.TraceWarning(
                BrokerTracing.GenerateTraceString(
                    "AzureDispatcher",
                    "CleanupClient",
                    this.TaskId,
                    -1,
                    serviceClient.ToString(),
                    string.Empty,
                    "Cleanup client."));

            AzureServiceClient[] clients = null;
            lock (this.proxyClientPool)
            {
                clients = this.proxyClientPool.Clear(serviceClient);
            }

            if (clients != null)
            {
                BrokerTracing.TraceWarning(
                    BrokerTracing.GenerateTraceString(
                        "AzureDispatcher",
                        "CleanupClient",
                        this.TaskId,
                        -1,
                        serviceClient.ToString(),
                        string.Empty,
                        string.Format("Close clients in the pool. count = {0}", clients.Length)));

                // don't close client in above lock (this.proxyClientPool) scope to avoid deadlock
                foreach (AzureServiceClient asc in clients)
                {
                    asc.AsyncClose();
                }
            }

            return (clients != null);
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            // detach proxy client pool
            this.DetachProxyClientPool(this.Epr);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Get ProxyClientPool according to broker proxy endpoint address.
        /// </summary>
        /// <param name="proxyEpr">broker proxy endpoint address</param>
        /// <param name="proxyClientPoolCapacity"> capacity of ProxyClient pool.  It is used when creating a new ProxyClientPool instance</param>
        /// <returns>ProxyClientPool instance</returns>
        private ProxyClientPool AttachProxyClientPool(EndpointAddress proxyEpr, int proxyClientPoolCapacity)
        {
            ProxyClientPool clientPool = null;
            bool createFlag = false;
            lock (LockProxyClientPoolDic)
            {
                // if there is already a connection pool to the target broker proxy, reuse it; otherwise, create a new one.
                if (!ProxyClientPoolDic.TryGetValue(proxyEpr, out clientPool))
                {
                    clientPool = new ProxyClientPool(proxyEpr, proxyClientPoolCapacity);
                    clientPool.RefCount = 1;
                    ProxyClientPoolDic.Add(proxyEpr, clientPool);
                    createFlag = true;
                }
                else
                {
                    clientPool.RefCount++;
                }
            }

            if (createFlag)
            {
                BrokerTracing.TraceVerbose("[AzureDispatcher] New ProxyClientPool instance is created. Proxy EPR address = {0}, capacity = {1}", proxyEpr, proxyClientPoolCapacity);
            }

            return clientPool;
        }

        /// <summary>
        /// Detach from the ProxyClientPool so that the ProxyClientPool can be reclaimed if nobody is referencing it
        /// </summary>
        /// <param name="proxyEpr">broker proxy endpoint address</param>
        private void DetachProxyClientPool(EndpointAddress proxyEpr)
        {
            ProxyClientPool clientPool = null;
            lock (LockProxyClientPoolDic)
            {
                if (ProxyClientPoolDic.TryGetValue(proxyEpr, out clientPool))
                {
                    clientPool.RefCount--;
                    if (clientPool.RefCount == 0)
                    {
                        ProxyClientPoolDic.Remove(proxyEpr);
                    }
                }
            }
        }

        /// <summary>
        /// Check connection status between the specified client and broker proxy by sending a ping message to broker proxy,
        /// and close the client if the connnection is broken.
        /// </summary>
        /// <param name="client">the service client instance to be </param>
        /// <returns>async result object</returns>
        private static IAsyncResult BeginPingProxyClient(AzureServiceClient client)
        {
            Debug.Assert(client != null);

            try
            {
                // no need to ping proxy if it is not ready
                if (!client.IsConnectionReady())
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceWarning("[AzureDispatcher].BeginPingProxyClient: Exception {0}", ex);

                // the client may be disposed, so its WaitHandleForOpen is already closed adn set null
                return null;
            }

            Message pingMessage = Message.CreateMessage(MessageVersion.Default, Constant.PingBrokerProxyAction);
            BrokerTracing.TraceVerbose("[AzureDispatcher] BeginPingProxyClient: ping client {0}", client.ServiceClient.Endpoint.Address);

            try
            {
                return client.BeginProcessMessage(
                    pingMessage,
                    new ThreadHelper<IAsyncResult>(new AsyncCallback(EndPingProxyClient)).CallbackRoot,
                    client);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[AzureDispatcher] BeginPingProxyClients: sending ping message via client {0} encounters excpetion: {1}", client.ServiceClient.Endpoint.Address, e);
                HandleClientFailure(client);
                return null;
            }
        }

        /// <summary>
        /// Callback function to check ping results for each service client
        /// </summary>
        /// <param name="ar">async result object</param>
        private static void EndPingProxyClient(IAsyncResult ar)
        {
            AzureServiceClient client = ar.AsyncState as AzureServiceClient;
            try
            {
                client.EndProcessMessage(ar);
                BrokerTracing.TraceVerbose("[AzureDispatcher] EndPingProxyClients: client {0} is ok", client.ServiceClient.Endpoint.Address);
            }
            catch (TimeoutException)
            {
                // TimeoutException may happen when the channel is sending a big request message.
                // It doesn't mean the channel is broken.
                BrokerTracing.TraceVerbose("[AzureDispatcher] EndPingProxyClients: client {0} timeout", client.ServiceClient.Endpoint.Address);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[AzureDispatcher] EndPingProxyClients: client {0} receives exception: {1}", client.ServiceClient.Endpoint.Address, e);
                HandleClientFailure(client);
            }
        }

        /// <summary>
        /// Handle client failure
        /// </summary>
        /// <param name="client">targeted client</param>
        private static void HandleClientFailure(AzureServiceClient client)
        {
            try
            {
                BrokerTracing.TraceWarning(
                    "[AzureDispatcher] HandleClientFailure: Handle invalid client, {0}, {1}",
                    client,
                    client.ServiceClient.Endpoint.Address);

                Dictionary<EndpointAddress, ProxyClientPool>.ValueCollection pools;

                lock (LockProxyClientPoolDic)
                {
                    pools = ProxyClientPoolDic.Values;
                }

                foreach (var pool in pools)
                {
                    bool existInPool = false;

                    lock (pool)
                    {
                        BrokerTracing.TraceVerbose(
                            "[AzureDispatcher] HandleClientFailure: Remove client {0} from pool.", client);

                        existInPool = pool.RemoveProxyClient(client);
                    }

                    if (existInPool)
                    {
                        BrokerTracing.TraceVerbose(
                            "[AzureDispatcher] HandleClientFailure: Close client {0}", client);

                        // Close the proxy client if any exception is encountered.
                        // As a result, async pending callback on clients will be
                        // invoked (with exception).
                        client.AsyncClose();

                        return;
                    }
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureDispatcher] HandleClientFailure: Error occurs, {0}", e);
            }
        }

        /// <summary>
        /// Callback thread that checks status of all proxy clients.
        /// </summary>
        /// <param name="state">state object</param>
        private static void CheckProxyClients(object state)
        {
            // get a list of all ProxyClientPool
            List<ProxyClientPool> allProxyClientPools = new List<ProxyClientPool>();

            lock (LockProxyClientPoolDic)
            {
                allProxyClientPools.AddRange(ProxyClientPoolDic.Values);
            }

            // get a list of all proxy clients
            List<AzureServiceClient> allProxyClients = new List<AzureServiceClient>();
            foreach (ProxyClientPool clientPool in allProxyClientPools)
            {
                lock (clientPool)
                {
                    allProxyClients.AddRange(clientPool.AllProxyClients);
                }
            }

            // ping proxy clients
            foreach (AzureServiceClient client in allProxyClients)
            {
                BeginPingProxyClient(client);
            }
        }
    }
}
