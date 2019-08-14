//------------------------------------------------------------------------------
// <copyright file="IResourceProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Interface for resource provider
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Threading.Tasks;
    /// <summary>
    /// Interface for resource provider
    /// </summary>
    /// <remarks>
    /// Resource provider is used when creating session. It provides methods to allocate resource for session or free them (when failed to create session).
    /// </remarks>
    public interface IResourceProvider
    {
        /// <summary>
        /// Allocate resource for service job and provide broker epr
        /// </summary>
        /// <param name="startInfo">indicating session start information</param>
        /// <param name="durable">indicating whether it is a durable session</param>
        /// <param name="timeout">indicating the timeout</param>
        /// <param name="eprs">output string array of available broker epr list</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>returns unique session id</returns>
        Task<SessionAllocateInfoContract> AllocateResource(SessionStartInfo startInfo, bool durable, TimeSpan timeout);

        /// <summary>
        /// Get resource information
        /// </summary>
        /// <param name="attachInfo">indicating session attach information</param>
        /// <param name="timeout">indicating the timeout</param>
        /// <returns>returns session information</returns>
        Task<SessionInfo> GetResourceInfo(SessionAttachInfo attachInfo, TimeSpan timeout);

        /// <summary>
        /// Free resource
        /// </summary>
        /// <param name="sessionId">indicating session id</param>
        Task FreeResource(SessionStartInfo startInfo, int sessionId);
    }
}
