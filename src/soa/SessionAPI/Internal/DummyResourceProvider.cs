//------------------------------------------------------------------------------
// <copyright file="DummyResourceProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Resource provider that does nothing
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Resource provider that does nothing
    /// </summary>
    public class DummyResourceProvider : IResourceProvider
    {
        /// <summary>
        /// Stores a value indicating whether it is a durable session
        /// </summary>
        private bool durable;

        /// <summary>
        /// Initializes a new instance of the DummyResourceProvider
        /// </summary>
        /// <param name="durable">indicating whether the session is a durable session</param>
        public DummyResourceProvider(bool durable)
        {
            this.durable = durable;
        }

        /// <summary>
        /// Allocate resource for service job and provide broker epr
        /// </summary>
        /// <param name="startInfo">indicating session start information</param>
        /// <param name="durable">indicating whether it is a durable session</param>
        /// <param name="timeout">indicating the timeout</param>
        /// <param name="eprs">output string array of available broker epr list</param>
        /// <param name="sessionInfo">output the session info</param>
        /// <returns>returns unique session id</returns>
        public Task<SessionAllocateInfoContract> AllocateResource(SessionStartInfo startInfo, bool durable, TimeSpan timeout)
        {
            SessionAllocateInfoContract sessionAllocateInfo = new SessionAllocateInfoContract();
            sessionAllocateInfo.BrokerLauncherEpr = new string[0];
            sessionAllocateInfo.SessionInfo = null;
            sessionAllocateInfo.Id = InprocessSessions.GetInstance().AllocateSessionId(durable);
#if net40
            return TaskEx.FromResult(sessionAllocateInfo);
#else
            return Task.FromResult(sessionAllocateInfo);
#endif
        }

        /// <summary>
        /// Get resource information
        /// </summary>
        /// <param name="attachInfo">indicating session attach information</param>
        /// <param name="timeout">indicating the timeout</param>
        /// <returns>returns session information</returns>
        public Task<SessionInfo> GetResourceInfo(SessionAttachInfo attachInfo, TimeSpan timeout)
        {
            try
            {
#if net40
                return TaskEx.FromResult(InprocessSessions.GetInstance().FetchSessionInfo(attachInfo.SessionId));
#else
                return Task.FromResult(InprocessSessions.GetInstance().FetchSessionInfo(attachInfo.SessionId));
#endif
            }
            catch (SessionException e)
            {
                if (this.durable && attachInfo.SessionId == InprocessSessions.DebugModeSessionId && e.ErrorCode == SOAFaultCode.Session_ValidateJobFailed_AlreadyFinished)
                {
                    // Return an empty instance of SessionInfo as it is not used in debug mode when trying to raise up a broker
#if net40
                    return TaskEx.FromResult(new SessionInfo());
#else
                    return Task.FromResult(new SessionInfo());
#endif
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Free resource
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        public Task FreeResource(SessionStartInfo startInfo, string sessionId)
        {
            // Do nothing
#if net40
            return TaskEx.FromResult(1);
#else
            return Task.FromResult(1);
#endif
        }
    }
}
