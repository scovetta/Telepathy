//-----------------------------------------------------------------------
// <copyright file="ProxyClientPool.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Proxy client pool</summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using Microsoft.Hpc.BrokerProxy;

    /// <summary>
    /// A class that manages ServiceClients to one broker proxy.
    /// </summary>
    internal class ProxyClientPool
    {
        #region private fields
        /// <summary> Pool capacity </summary>
        private int capacity;

        /// <summary> Endpoint address of target broker proxy</summary>
        private EndpointAddress proxyEpr;

        /// <summary> A list of ServiceClients to broker proxy </summary>
        private List<AzureServiceClient> clientPool;

        /// <summary> Reference counts of ServiceCliens in clientPool </summary>
        private List<int> clientRefCounts;

        /// <summary> Index of ServiceClient to be returned to GetProxyClient in clientPool.</summary>
        private int currentIndex = -1;

        /// <summary> How many dispatchers are referencing this pool</summary>
        private int refCount;

        /// <summary> Sync object for this instance</summary>
        private object syncObj = new object();
        #endregion

        public ProxyClientPool(EndpointAddress epr, int capacity)
        {
            Debug.Assert(capacity > 0, "invalid capacity");
            this.proxyEpr = epr;
            this.capacity = capacity;
            this.clientPool = new List<AzureServiceClient>(this.capacity);
            this.clientRefCounts = new List<int>(this.capacity);
        }

        /// <summary> ProxyClientPool capacity </summary>
        public int Capacity
        {
            get { return this.capacity; }
        }

        /// <summary> ProxyClientPool size.  Returns number of ServiceClients in the pool.</summary>
        public int Size
        {
            get { return clientPool.Count; }
        }

        /// <summary> Returns how many dispatchers are referencing the pool</summary>
        public int RefCount
        {
            get { return this.refCount; }
            set { this.refCount = value; }
        }

        /// <summary> Retruns true if there is no ServiceClient in the pool</summary>
        public bool IsEmpty
        {
            get { return this.clientPool.Count == 0; }
        }

        /// <summary> Returns true if pool capacity is fully used</summary>
        public bool IsFull
        {
            get { return this.clientPool.Count == this.capacity; }
        }

        /// <summary> Returns a list of all service clients in the pool </summary>
        public List<AzureServiceClient> AllProxyClients
        {
            get { return this.clientPool; }
        }

        /// <summary>
        /// Get a service client from pool and make sure it is workable.
        /// </summary>
        /// <remarks>
        /// Notice: Call ReleaseProxyClient when you're done with ServiceClient.
        /// </remarks>
        /// <returns>service client</returns>
        public AzureServiceClient GetProxyClient()
        {
            while (true)
            {
                AzureServiceClient client = this.InternalGetProxyClient();

                if (client.ServiceClient.State == CommunicationState.Closed ||
                    client.ServiceClient.State == CommunicationState.Closing ||
                    client.ServiceClient.State == CommunicationState.Faulted)
                {
                    BrokerTracing.TraceVerbose(
                        "[ProxyClientPool].GetProxyClient: Client is not ready for use, remove it, {0}, {1}",
                        client,
                        client.ServiceClient.Endpoint.Address);

                    this.RemoveProxyClient(client);

                    if (client.ServiceClient.State == CommunicationState.Faulted)
                    {
                        client.AsyncClose();
                    }
                }
                else
                {
                    BrokerTracing.TraceVerbose(
                        "[ProxyClientPool].GetProxyClient: Get a client ready for use, {0}, {1}",
                        client,
                        client.ServiceClient.Endpoint.Address);

                    return client;
                }
            }
        }

        /// <summary>
        /// Returns one ServieClient in the pool.
        /// </summary>
        private AzureServiceClient InternalGetProxyClient()
        {
            AzureServiceClient client = null;

            if (!this.IsFull)
            {
                client = new AzureServiceClient(ProxyBinding.BrokerProxyBinding, this.proxyEpr);

                BrokerTracing.TraceInfo(
                    "[ProxyClientPool].InternalGetProxyClient: Create a new client, {0}, {1}",
                    client,
                    client.ServiceClient.Endpoint.Address);

                this.clientPool.Add(client);

                BrokerTracing.TraceInfo(
                    "[ProxyClientPool].InternalGetProxyClient: Pool size is {0}",
                    this.clientPool.Count);

                // add "1" to the RefCounts list since we will return this new client
                this.clientRefCounts.Add(1);

                return client;
            }
            else
            {
                int poolSize = this.clientPool.Count;

                if (poolSize > 0)
                {
                    if (this.currentIndex <= 0 || this.currentIndex >= poolSize)
                    {
                        this.currentIndex = 0;
                    }

                    client = this.clientPool[this.currentIndex];

                    this.clientRefCounts[this.currentIndex]++;

                    this.currentIndex++;
                }

                BrokerTracing.TraceInfo(
                    "[ProxyClientPool].InternalGetProxyClient: Get a client from pool, {0}, {1}",
                    client,
                    client.ServiceClient.Endpoint.Address);

                return client;
            }
        }

        /// <summary>
        /// Remove the specified client from pool and update the reference count.
        /// </summary>
        /// <param name="client">targeted client</param>
        /// <returns>removed from pool or not</returns>
        public bool ReleaseProxyClient(AzureServiceClient client)
        {
            for (int i = 0; i < this.clientPool.Count; i++)
            {
                if (ReferenceEquals(this.clientPool[i], client))
                {
                    this.clientRefCounts[i]--;
                    if (this.clientRefCounts[i] == 0)
                    {
                        this.clientPool.RemoveAt(i);
                        this.clientRefCounts.RemoveAt(i);
                        BrokerTracing.TraceInfo("[ProxyClientPool] .ReleaseProxyClient: close client {0} as its reference count reaches 0", client.ServiceClient.Endpoint.Address);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Remove the specified client from pool.
        /// </summary>
        /// <param name="client">targeted client</param>
        /// <returns>removed from pool or not</returns>
        public bool RemoveProxyClient(AzureServiceClient client)
        {
            for (int i = 0; i < this.clientPool.Count; i++)
            {
                if (ReferenceEquals(this.clientPool[i], client))
                {
                    this.clientPool.RemoveAt(i);
                    this.clientRefCounts.RemoveAt(i);
                    BrokerTracing.TraceInfo("ProxyClientPool. RemoveProxyClient: Close client {0}.", client.ServiceClient.Endpoint.Address);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If parameter serviceClient is in the pool, clear the pool and return the list items.
        /// The invoker of this method will async close returned clients outside a lock scope.
        /// </summary>
        /// <returns>the list items</returns>
        public AzureServiceClient[] Clear(AzureServiceClient serviceClient)
        {
            if (serviceClient != null && this.clientPool.Contains(serviceClient))
            {
                AzureServiceClient[] clients = this.clientPool.ToArray();
                this.clientPool.Clear();
                this.clientRefCounts.Clear();
                BrokerTracing.TraceWarning("[ProxyClientPool]. Clear: Client {0} is in the pool, so clear the pool {1}.", serviceClient, this.proxyEpr.ToString());
                return clients;
            }
            else
            {
                BrokerTracing.TraceWarning("[ProxyClientPool]. Clear: Client {0} is not in the pool {1}.", serviceClient, this.proxyEpr.ToString());
                return null;
            }
        }
    }
}
