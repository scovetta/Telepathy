//------------------------------------------------------------------------------
// <copyright file="DataLifeCycleContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data lifecycle context
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    /// <summary>
    /// Abstract class for various data lifecycle context classes
    /// </summary>
    public abstract class DataLifeCycleContext
    {
        public abstract DataLifeCycleType Type
        {
            get;
        }
    }

    /// <summary>
    /// Context for session based data lifecycle
    /// </summary>
    public class SessionBasedDataLifeCycleContext : DataLifeCycleContext
    {
        int sessionId;

        /// <summary>
        /// Constructor of <see cref="SessionBasedDataLifeCycleContext"/>
        /// </summary>
        /// <param name="sessionId"></param>
        public SessionBasedDataLifeCycleContext(int sessionId)
        {
            this.sessionId = sessionId;
        }

        /// <summary>
        /// Get the <see cref="DataLifeCycleType"/>
        /// </summary>
        public override DataLifeCycleType Type
        {
            get { return DataLifeCycleType.Session; }
        }

        /// <summary>
        /// Get the session ID
        /// </summary>
        public int SessionId
        {
            get { return this.sessionId; }
        }
    }
}
