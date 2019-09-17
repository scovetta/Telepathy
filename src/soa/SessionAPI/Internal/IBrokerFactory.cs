//------------------------------------------------------------------------------
// <copyright file="IBrokerFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Interface for broker factory
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for broker factory
    /// </summary>
    public interface IBrokerFactory
    {
        /// <summary>
        /// Create broker
        /// </summary>
        /// <param name="startInfo">indicating the session start information</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="targetTimeout">indicating the target timeout</param>
        /// <param name="eprs">indicating the broker epr list</param>
        /// <param name="binding">indicting the binding</param>
        /// <returns>returns the session instance</returns>
        Task<SessionBase> CreateBroker(SessionStartInfo startInfo, string sessionId, DateTime targetTimeout, string[] eprs, Binding binding);

        /// <summary>
        /// Attach to a broker, returns session instance
        /// </summary>
        /// <param name="attachInfo">indicating the attach information</param>
        /// <param name="info">indicating the session info to be updated</param>
        /// <param name="timeout">indicating the timeout</param>
        /// <param name="binding">indicting the binding</param>
        /// <returns>returns the session instance</returns>
        Task<SessionBase> AttachBroker(SessionAttachInfo attachInfo, SessionInfo info, TimeSpan timeout, Binding binding);
    }
}
