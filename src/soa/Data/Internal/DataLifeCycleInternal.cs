//------------------------------------------------------------------------------
// <copyright file="DataLifeCycleInternal.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data life cycle internal
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    /// <summary>
    /// Internal data life cycle class
    /// </summary>
    public class DataLifeCycleInternal
    {
        private DataLifeCycleContext lifeCycleContext;

        /// <summary>
        /// Create a DataLifeCycleInternal object that aligns with lifetime of a session. Data assocaited with
        /// this DataLifeCycleInternal object will be automatically removed on session close.
        /// </summary>
        /// <param name="sessionId">session id</param>
        public DataLifeCycleInternal(int sessionId)
        {
            lifeCycleContext = new SessionBasedDataLifeCycleContext(sessionId);
        }

        /// <summary>
        /// Returns data lifecycle type.
        /// </summary>
        public DataLifeCycleType Type
        {
            get { return this.lifeCycleContext.Type; }
        }

        /// <summary>
        /// Returns data lifecycle context
        /// </summary>
        public DataLifeCycleContext Context
        {
            get { return this.lifeCycleContext; }
        }
    }
}
