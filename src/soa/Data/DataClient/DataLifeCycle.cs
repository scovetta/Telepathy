//------------------------------------------------------------------------------
// <copyright file="DataLifeCycle.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data life cycle
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    /// <summary>
    /// Data life cycle
    /// </summary>
    public class DataLifeCycle
    {
        private DataLifeCycleInternal lifeCycleInternal;

        /// <summary>
        /// Create a DataLifeCycle object that aligns with lifetime of a session. Data assocaited with
        /// this DataLifeCycle object will be automatically removed on session close.
        /// </summary>
        /// <param name="sessionId">session id</param>
        public DataLifeCycle(int sessionId)
        {
            Utility.ValidateSessionId(sessionId);

            lifeCycleInternal = new DataLifeCycleInternal(sessionId);
        }

#if false
        /// <summary>
        /// Create a DataLifeCycle object that aligns with lifetime of a  brokerclient. Data assocaited with
        /// this DataLifeCycle object will be automatically removed on close/timeout of the broker client instance.
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <param name="clientId">broker client id</param>
        public DataLifeCycle(int sessionId, string clientId)
        {
            lifeCycleInternal = new DataLifeCycleInternal(sessionId, clientId);
        }

        /// <summary>
        /// Create a TTL based DataLifeCycle object.   Data associated with this DataLifeCycle object
        /// will be automatically removed on data TTL expiration.
        /// </summary>
        /// <param name="ttl"></param>
        public DataLifeCycle(TimeSpan ttl)
        {
            lifeCycleInternal = new DataLifeCycleInternal(ttl);
        }
#endif

        /// <summary>
        /// Returns internal data lifecycle object
        /// </summary>
        internal DataLifeCycleInternal Internal
        {
            get { return this.lifeCycleInternal; }
        }
    }
}
