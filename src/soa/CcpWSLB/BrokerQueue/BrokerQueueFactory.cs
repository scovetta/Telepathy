// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.Persistences;
    using Microsoft.Telepathy.ServiceBroker.Persistences.AzureQueuePersist;
    using Microsoft.Telepathy.Session.Common;

    /// <summary>
    /// a calllback delegate to judge whether the specified session is stale, the related job is purged by the scheduler.
    /// </summary>
    /// <param name="sessionId">the session id.</param>
    /// <returns>a value indicates wheter the session is stale.</returns>
    public delegate Task<bool> IsStaleSessionCallback(string sessionId);

    /// <summary>
    /// the factory class for the broker queue.this class willl parse the broker's local persistence config file.
    /// </summary>
    internal sealed class BrokerQueueFactory : IBrokerQueueFactory, IDisposable
    {
        #region private static fields
        /// <summary>the lock object for get the instance.</summary>
        private static object lockObj = new object();

        /// <summary>the default client id.</summary>
        private static string defaultClientId = string.Empty;
        #endregion

        #region private fields
        /// <summary>the session id for this broker queue factory.</summary>
        private string sessionId;

        /// <summary>the persist name for this broker queue factory.</summary>
        private string persistName;

        /// <summary>the lock object for thread safe.</summary>
        private object thisLockObj;

        /// <summary>the cache dic for the client broker queue.</summary>
        private Dictionary<string, BrokerQueue> clientBrokerQueueDic;

        /// <summary>the broker queue dispatcher for the session.</summary>
        private BrokerQueueDispatcher brokerQueueDispatcher;

        /// <summary>
        /// Stores the shared data
        /// </summary>
        private SharedData sharedData;
        #endregion

        /// <summary>
        /// Initializes a new instance of the BrokerQueueFactory class, 
        /// </summary>
        /// <param name="persistName">tye persist name.</param>
        /// <param name="sharedData">indicating the shared data</param>
        public BrokerQueueFactory(string persistName, SharedData sharedData)
        {
            if (persistName == null)
            {
                persistName = string.Empty;
            }

            ParamCheckUtility.ThrowIfOutofRange(!string.IsNullOrEmpty(persistName) && !string.Equals("AzureQueue", persistName, StringComparison.OrdinalIgnoreCase), "persistName");
            this.thisLockObj = new object();
            this.sessionId = sharedData.BrokerInfo.SessionId;
            this.persistName = persistName;
            this.sharedData = sharedData;
            this.clientBrokerQueueDic = new Dictionary<string, BrokerQueue>(StringComparer.OrdinalIgnoreCase);
            this.brokerQueueDispatcher = new BrokerQueueDispatcher(this.sessionId, !string.IsNullOrEmpty(persistName), sharedData);
#if MSMQ
            if (!string.IsNullOrEmpty(sharedData.BrokerInfo.AadUserSid) && !string.IsNullOrEmpty(sharedData.BrokerInfo.AadUserName))
            {
                BrokerTracing.TraceInfo($"[BrokerQueueFactory] .Ctor: Add AAD SID mapping from broker start info: {sharedData.BrokerInfo.AadUserSid}, {sharedData.BrokerInfo.AadUserName}.");
                MSMQ.MSMQPersist.SetAadSidEntry(sharedData.BrokerInfo.AadUserSid, sharedData.BrokerInfo.AadUserName);
            }
#endif
        }

        /// <summary>
        /// Gets all the client info in the session.
        /// </summary>
        public ClientInfo[] AllClientInfos
        {
            get
            {
                ClientInfo[] allClients = null;
                if (string.IsNullOrEmpty(this.persistName))
                {
                    List<ClientInfo> clientsList = new List<ClientInfo>();
                    lock (this.clientBrokerQueueDic)
                    {
                        foreach (KeyValuePair<string, BrokerQueue> pair in this.clientBrokerQueueDic)
                        {
                            clientsList.Add(new ClientInfo(pair.Key, (int)pair.Value.AllRequestsCount, (int)pair.Value.ProcessedRequestsCount, pair.Value.UserName)); 
                        }
                    }

                    allClients = clientsList.ToArray();
                }
                else
                {

                    // for the durable session, the broker maybe restart, need enumerate all the physical queues to get the client ids for the session.
                    //allClients = Microsoft.Hpc.ServiceBroker.BrokerStorage.MSMQ.MSMQPersist.GetSessionClients(this.sessionId, this.sharedData.StartInfo.UseAad);
                    allClients = AzureQueuePersist.GetSessionClients(this.sharedData.BrokerInfo.AzureStorageConnectionString, this.sessionId, false);
                }

                return allClients;
            }
        }

        /// <summary>
        /// Gets the broker queue dispatcher.
        /// </summary>
        public BrokerQueueDispatcher Dispatcher
        {
            get
            {
                return this.brokerQueueDispatcher;
            }
        }

        /// <summary>
        /// Put the response into the storage, and delete corresponding request from the storage.
        /// the async result will return void.byt GetResult will throw exception if the response is not persisted into the persistence.
        /// </summary>
        /// <param name="responseMsg">the response message</param>
        /// <param name="requestItem">corresponding request item</param>
        /// <remarks>
        /// This method here is only visible from the interface as it is included mainly
        /// for testability. All callers of this method should have an instance of IBrokerQueueFactory
        /// instead of any special implementation.
        /// </remarks>
        async Task IBrokerQueueFactory.PutResponseAsync(Message responseMsg, BrokerQueueItem requestItem)
        {
            await this.Dispatcher.PutResponseAsync(responseMsg, requestItem);
        }

        /// <summary>
        /// cleanup the stale persisted data for the sessions that related jobs are purged by the scheduler.
        /// </summary>
        /// <param name="persistName">the persist name.</param>
        /// <param name="isStaleSessionCallback">the calback function to judge whether a session is stale.</param>
        public static async Task CleanupStalePersistedData(string persistName, IsStaleSessionCallback isStaleSessionCallback, string connectString)
        {

            ParamCheckUtility.ThrowIfNull(isStaleSessionCallback, "isStaleSessionCallback");
            if (!string.IsNullOrEmpty(persistName))
            {
                if (persistName.Trim().Equals("AzureQueue", StringComparison.OrdinalIgnoreCase))
                {
                    BrokerTracing.TraceInfo(
                        "[BrokerQueueFactory] .CleanupStalePersistedData: cleaning up stale data in AzureQueue");
                    await AzureQueuePersist
                        .CleanupStaleMessageQueue(isStaleSessionCallback, connectString);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("persistName",
                        persistName + ", which persistence is not supported.");
                }
            }

        }

        /// <summary>
        /// get the correct persist broker queue by name 
        /// that passed in by session info when the client create the session.
        /// Note: not thread safe method.
        /// </summary>
        /// <param name="clientId">the client id for the broker queue.</param>
        /// <param name="userName">the user name.</param>
        /// <param name="isNewCreate">indicate whether the returned broker is a new create broker queue.</param>
        /// <returns>return the broker queue</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "acl"), SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "we need load assembly here to support different storage options.")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "we need return the broker queue and a value to indicate whether it is a new queue.")]
        public BrokerQueue GetPersistQueueByClient(string clientId, string userName, out bool isNewCreate)
        {
            BrokerTracing.TraceInfo($"[GetPersistQueueByClient] username:{userName}, sessionid:{this.sessionId}, client:{clientId}");
            isNewCreate = false;
            if (string.IsNullOrEmpty(clientId))
            {
                clientId = BrokerQueueFactory.defaultClientId;
            }

            BrokerQueue brokerQueue = null;

            lock (this.clientBrokerQueueDic)
            {
                this.clientBrokerQueueDic.TryGetValue(clientId, out brokerQueue);
            }

            if (brokerQueue == null)
            {
                lock (this.thisLockObj)
                {
                    lock (this.clientBrokerQueueDic)
                    {
                        this.clientBrokerQueueDic.TryGetValue(clientId, out brokerQueue);
                    }

                    if (brokerQueue == null)
                    {
                        ISessionPersist sessionPersist = null;
                        if (string.IsNullOrEmpty(this.persistName))
                        {
                            sessionPersist = new MemoryPersist(userName, this.sessionId, clientId);
                            brokerQueue = new BrokerPersistQueue(
                                this.brokerQueueDispatcher,
                                this.persistName,
                                this.sessionId,
                                clientId,
                                sessionPersist,
                                1,    // no request cache
                                // no request cache
                                1,    // no response cache
                                // no response cache
                                0,    // no timeout
                                // no timeout
                                false,
                                // no in-memory cache
                                this.sharedData,
                                this);
                            this.brokerQueueDispatcher.AddBrokerQueue(brokerQueue, null);
                        }
                        else
                        {
                            ParamCheckUtility.ThrowIfNull(this.sharedData.BrokerInfo.AzureStorageConnectionString, "StorageConnectString");

                            sessionPersist = new AzureQueuePersist(userName, this.sessionId, clientId, this.sharedData.BrokerInfo.AzureStorageConnectionString);
                            isNewCreate = sessionPersist.IsNewCreated;
                            brokerQueue = new BrokerPersistQueue(
                                this.brokerQueueDispatcher,
                                this.persistName,
                                this.sessionId,
                                clientId,
                                sessionPersist,
                                BrokerQueueConstants.DefaultThresholdForRequestPersist,
                                BrokerQueueConstants.DefaultThresholdForResponsePersist,
                                BrokerQueueConstants.DefaultMessagesInCacheTimeout,
                                isNewCreate,  // need in-memory quick cache for newly-created durable queue
                                // need in-memory quick cache for newly-created durable queue
                                this.sharedData,
                                this);

                            //
                            // For an existing durable queue, if EndOfMessage is not received, or there are requests waiting to be processed, 
                            // then schedule the broker queue for dispatching;
                            // for an newly created durable queue, it will be added to brokerQueueDispatcher when BrokerQueuePersist.Flush
                            // is called (with quick cache filled), so no need to call AddBrokerQueue here (bug #21453)
                            // 
                            if (!isNewCreate && (!brokerQueue.EOMReceived || sessionPersist.RequestsCount > 0))
                            {
                                this.brokerQueueDispatcher.AddBrokerQueue(brokerQueue, null);
                            }
                        }

                        lock (this.clientBrokerQueueDic)
                        {
                            this.clientBrokerQueueDic.Add(clientId, brokerQueue);
                        }
                    }
                }
            }

            // Bug 6313: Check the user name
            ThrowIfUserNameDoesNotMatch(brokerQueue, userName);
            return brokerQueue;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            List<BrokerQueue> queuelist = new List<BrokerQueue>();

            lock (this.clientBrokerQueueDic)
            {
                queuelist.AddRange(this.clientBrokerQueueDic.Values);
            }

            foreach (BrokerQueue brokerQueue in queuelist)
            {
                if (brokerQueue != null)
                {
                    brokerQueue.Dispose();
                }
            }

            lock (this.clientBrokerQueueDic)
            {
                this.clientBrokerQueueDic.Clear();
            }
        }

        /// <summary>
        /// remove the broker queue from the cached list.
        /// </summary>
        /// <param name="clientId">the client id.</param>
        internal void RemovePersistQueueByClient(string clientId)
        {
            ParamCheckUtility.ThrowIfNull(clientId, "clientId");
            BrokerTracing.TraceInfo("[BrokerQueueFactory] .RemovePersistQueueByClient: ClientId:{0}", clientId);
            lock (this.clientBrokerQueueDic)
            {
                if (this.clientBrokerQueueDic.ContainsKey(clientId))
                {
                    this.clientBrokerQueueDic.Remove(clientId);
                }
            }
        }

        /// <summary>
        /// Check the user name and throw exception if user name does not match
        /// </summary>
        /// <param name="brokerQueue">indicating the broker queue</param>
        /// <param name="userName">indicating the user name</param>
        private static void ThrowIfUserNameDoesNotMatch(BrokerQueue brokerQueue, string userName)
        {
            if (!string.Equals(userName, brokerQueue.UserName, StringComparison.OrdinalIgnoreCase))
            {
                throw new BrokerQueueException((int)BrokerQueueErrorCode.E_BQ_USER_NOT_MATCH, String.Format("the user name is not the user who create the queue. username: {0}, ownername={1}", userName, brokerQueue.UserName));
            }
        }
    }
}
