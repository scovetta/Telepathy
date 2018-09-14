//------------------------------------------------------------------------------
// <copyright file="DataLifeCycleStore.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      In-memory data life cycle store
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// In-memory data life cycle store. Now it maintains the relationship between DataClient and SOA session.
    /// It tells which session a DataClient is associated with, and for a given session, which DataClients
    /// are associated with it.
    /// </summary>
    internal class DataLifeCycleStore
    {
        /// <summary>
        /// DataClient to session id mapping.  It is used to tell which session a DataClient is associated with.
        /// </summary>
        Dictionary<string, int> dataClientToSessionId = new Dictionary<string, int>();

        /// <summary>
        /// Session id to DataClient list mapping.  It tells which DataClients are associated with a session.
        /// </summary>
        Dictionary<int, LinkedList<string>> sessionIdToDataClients = new Dictionary<int, LinkedList<string>>();

        /// <summary>
        /// Remove a DataClient from the data lifecycle management store
        /// </summary>
        /// <param name="dataClientId">DataClient id</param>
        public void RemoveDataClient(string dataClientId)
        {
            lock (this.dataClientToSessionId)
            {
                int jobId;
                if (this.dataClientToSessionId.TryGetValue(dataClientId, out jobId))
                {
                    // remove dataClientId -> jobId from dataClientsToJobId dictionary
                    this.dataClientToSessionId.Remove(dataClientId);

                    // remove jobId -> dataClientId from jobIdToDataClients dictionary
                    this.sessionIdToDataClients[jobId].Remove(dataClientId);

                    // remove jobId entry if there is no dataclients associated with it
                    if (this.sessionIdToDataClients[jobId].Count == 0)
                    {
                        this.sessionIdToDataClients.Remove(jobId);
                    }
                }
            }
        }

        /// <summary>
        /// Add a DataClient lifecycle info item into the store
        /// </summary>
        /// <param name="dataClientId">DataClient id</param>
        /// <param name="sessionId">session id</param>
        public void AssociateDataClientWithSession(string dataClientId, int sessionId)
        {
            lock (this.dataClientToSessionId)
            {
                if (this.dataClientToSessionId.ContainsKey(dataClientId))
                {
                    this.dataClientToSessionId.Remove(dataClientId);
                }
                this.dataClientToSessionId.Add(dataClientId, sessionId);

                LinkedList<string> dataClients;
                if(!this.sessionIdToDataClients.TryGetValue(sessionId, out dataClients))
                {
                    dataClients = new LinkedList<string>();
                    this.sessionIdToDataClients.Add(sessionId, dataClients);
                }
                dataClients.AddLast(dataClientId);
            }
        }

        /// <summary>
        /// Get a list of all DataClients associated with the specified session
        /// </summary>
        /// <param name="sessionId">Session id</param>
        /// <returns>A list of all DataClients associated with the specified session</returns>
        public IEnumerable<string> ListDataClientsAssociatedWithSession(int sessionId)
        {
            List<string> retList = new List<string>();
            lock (this.dataClientToSessionId)
            {
                LinkedList<string> dataClients;
                if (this.sessionIdToDataClients.TryGetValue(sessionId, out dataClients))
                {
                    retList.AddRange(dataClients);
                }
            }
            return retList;
        }

        /// <summary>
        /// Get a list of all session ids that assocaite some DataClients
        /// </summary>
        public IEnumerable<int> ListSessionIds()
        {
            List<int> allSessionIds = new List<int>();
            lock (this.dataClientToSessionId)
            {
                foreach(int sessionId in this.sessionIdToDataClients.Keys)
                {
                    allSessionIds.Add(sessionId);
                }
            }
            return allSessionIds;
        }
    }
}
