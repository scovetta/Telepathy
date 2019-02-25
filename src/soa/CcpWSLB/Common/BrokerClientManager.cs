//------------------------------------------------------------------------------
// <copyright file="BrokerClientManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Broker client manager
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using System.Threading.Tasks;
    /// <summary>
    /// Broker client manager
    /// </summary>
    internal class BrokerClientManager : IDisposable
    {
        /// <summary>
        /// Stores the client dic, keyed by client id
        /// </summary>
        private Dictionary<string, BrokerClient> clientDic;

        /// <summary>
        /// Stores shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// Stores the broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Stores the broker state manager
        /// </summary>
        private BrokerStateManager stateManager;

        /// <summary>
        /// Stores the broker queue factory
        /// </summary>
        private BrokerQueueFactory queueFactory;

        /// <summary>
        /// Stores the service job monitor
        /// </summary>
        private ServiceJobMonitorBase monitor;

        /// <summary>
        /// Initializes a new instance of the BrokerClientManager class
        /// </summary>
        /// <param name="clientList">indicating the client info list</param>
        /// <param name="queueFactory">indicating the queue factory</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="stateManager">indicating the state manager</param>
        /// <param name="monitor">indicating the monitor</param>
        /// <param name="sharedData">indicating the shared data</param>
        public BrokerClientManager(ClientInfo[] clientList, BrokerQueueFactory queueFactory, BrokerObserver observer, BrokerStateManager stateManager, ServiceJobMonitorBase monitor, SharedData sharedData)
        {
            this.clientDic = new Dictionary<string, BrokerClient>(StringComparer.OrdinalIgnoreCase);
            this.queueFactory = queueFactory;
            this.observer = observer;
            this.stateManager = stateManager;
            this.monitor = monitor;
            this.sharedData = sharedData;

            foreach (ClientInfo client in clientList)
            {
                try
                {
                    // Bug 5193: Only raise client that has requests to process.
                    if (client.TotalRequestsCount != client.ProcessedRequestsCount)
                    {
                        this.AddNewClient(client.ClientId, client.UserName);
                    }
                }
                catch (Exception e)
                {
                    // Create client may fail because of broker queue failure, ignore the client in this situation and trys other client instead.
                    BrokerTracing.TraceError("[BrokerClientManager] Failed to create client {0}, Exception = {1}", client.ClientId, e);
                }
            }

            this.CheckIfAllRequestDone();
            this.CheckIfEOMCalled();
        }

        /// <summary>
        /// Finalizes an instance of the BrokerClientManager class
        /// </summary>
        ~BrokerClientManager()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets all active client ids
        /// </summary>
        /// <returns>list of client ids</returns>
        public List<string> GetAllActiveClientIds()
        {
            lock (this.clientDic)
            {
                return new List<string>(this.clientDic.Keys);
            }
        }

        /// <summary>
        /// Gets a broker client by client id
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="userName">indicating the user name</param>
        /// <returns>the broker client instance</returns>
        public BrokerClient GetClient(string clientId, string userName)
        {
            lock (this.clientDic)
            {
                BrokerClient client;
                if (this.clientDic.TryGetValue(clientId, out client))
                {
                    client.CheckAccess(userName);
                    return client;
                }

                return this.AddNewClient(clientId, userName);
            }
        }

        /// <summary>
        /// Purge the client
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="userName">indicating the user name</param>
        public async Task PurgeClient(string clientId, string userName)
        {
            BrokerClient client = this.GetClient(clientId, userName);
            SessionPersistCounter counter = this.RemoveClient(client, true);
            if (counter != null)
            {
                await this.monitor.ClientPurged(counter.FlushedRequestsCount,
                                          counter.FailedRequestsCountField,
                                          counter.ResponsesCountField);
            }
        }

        /// <summary>
        /// Dispose the BrokerClientManager
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Delete all queues
        /// </summary>
        public void DeleteAllQueues()
        {
            lock (this.clientDic)
            {
                foreach (BrokerClient client in this.clientDic.Values)
                {
                    client.DeleteQueue();
                }
            }
        }

        /// <summary>
        /// Check if EOM has called on any client, send event if so
        /// </summary>
        /// <remarks>only used during initialization, do not need lock</remarks>
        private void CheckIfEOMCalled()
        {
            foreach (BrokerClient client in this.clientDic.Values)
            {
                if (client.State == BrokerClientState.EndRequests)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Remove a client from the client manager
        /// Thread safe
        /// </summary>
        /// <param name="client">indicating the client</param>
        /// <param name="purge">indicating whether the client needs purge</param>
        private SessionPersistCounter RemoveClient(BrokerClient client, bool purge)
        {
            bool flag;
            SessionPersistCounter counter = null;

            lock (this.clientDic)
            {
                client.AllRequestDone -= new EventHandler(this.Client_AllRequestDone);
                client.ClientDisconnected -= new EventHandler(this.Client_ClientDisconnected);

                // Remove the client from client dic
                this.clientDic.Remove(client.ClientId);

                if (purge)
                {
                    counter = client.DeleteQueue();
                }

                client.Dispose();
                flag = this.clientDic.Count == 0;
            }

            if (flag)
            {
                this.stateManager.AllClientsDisconnected();
            }

            this.CheckIfAllRequestDone();
            return counter;
        }

        /// <summary>
        /// Check if all request done and send the event
        /// </summary>
        private void CheckIfAllRequestDone()
        {
            if (this.AllRequestDoneForAllClients())
            {
                this.monitor.AllClientsEnterAllRequestDoneState();
            }
        }

        /// <summary>
        /// Check if all clients are in the all request done state
        /// </summary>
        /// <returns>whether all clients are in the all request done state</returns>
        private bool AllRequestDoneForAllClients()
        {
            lock (this.clientDic)
            {
                foreach (BrokerClient client in this.clientDic.Values)
                {
                    if (client.State != BrokerClientState.AllRequestDone && client.State != BrokerClientState.GetResponse)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Dispose the BrokerClientManager
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.clientDic)
                {
                    foreach (BrokerClient client in this.clientDic.Values)
                    {
                        try
                        {
                            client.Dispose();
                        }
                        catch (Exception e)
                        {
                            BrokerTracing.TraceError("[BrokerClientManager] Dispose client {0} failed: {1}", client.ClientId, e);
                        }
                    }
                }

                this.clientDic = null;
            }
        }

        /// <summary>
        /// Add a new client
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <returns>return the new client</returns>
        private BrokerClient AddNewClient(string clientId, string userName)
        {
            BrokerClient client = new BrokerClient(clientId, userName, this.queueFactory, this.observer, this.stateManager, this.monitor, this.sharedData);
            client.AllRequestDone += new EventHandler(this.Client_AllRequestDone);
            client.ClientDisconnected += new EventHandler(this.Client_ClientDisconnected);
            this.clientDic.Add(clientId, client);
            BrokerTracing.TraceVerbose("[BrokerClientManager] Add new client {0}", clientId);
            this.stateManager.ClientConnected();
            this.monitor.ClientConnected();
            return client;
        }

        /// <summary>
        /// Triggered when a client is disconnected
        /// </summary>
        /// <param name="sender">indicating the client</param>
        /// <param name="e">indicating the event args</param>
        private void Client_ClientDisconnected(object sender, EventArgs e)
        {
            BrokerClient client = sender as BrokerClient;
            Debug.Assert(client != null, "[BrokerClientManager] ClientDisconnected Event: Sender should be the BrokerClient type.");
            BrokerTracing.TraceVerbose("[BrokerClientManager] Remove client {0}", client.ClientId);
            this.RemoveClient(client, false);
        }

        /// <summary>
        /// Triggered when a client reaches AllRequestDone state
        /// </summary>
        /// <param name="sender">indicating the client</param>
        /// <param name="e">indicating the event args</param>
        private void Client_AllRequestDone(object sender, EventArgs e)
        {
            this.CheckIfAllRequestDone();
        }
    }
}
