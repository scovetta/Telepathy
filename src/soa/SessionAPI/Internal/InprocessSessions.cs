//------------------------------------------------------------------------------
// <copyright file="InprocessSessions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Singleton class to hold all inproc sessions instances
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Singleton class to hold all inproc sessions instances
    /// </summary>
    internal class InprocessSessions
    {
        /// <summary>
        /// Stores the session id for debug mode sessions
        /// </summary>
        public const string DebugModeSessionId = "-1";

        /// <summary>
        /// Stores the singleton instance of the InprocessSessions class
        /// </summary>
        private static InprocessSessions instance = new InprocessSessions();

        /// <summary>
        /// Stores the session instance for debug mode
        /// </summary>
        private volatile SessionBase debugModeSession;

        /// <summary>
        /// Stores the session start info for debug mode session
        /// </summary>
        private volatile SessionStartInfo startInfo;

        /// <summary>
        /// Stores the ids of sessions which are removed.
        /// RemoveSessionInstance method is called before AddSession method when
        /// SessionIdleTimeout is a pretty small value. If session is really added,
        /// no one will cleanup that and following inproc session can't be used.
        /// So need to record the id of session which is actually expected to be
        /// removed. (#22955)
        /// </summary>
        /// <remarks>
        /// We only add int type data to this list but never remove them.
        /// The data is session id, so memory consumption is not a concern here.
        /// </remarks>
        private List<string> removedSessionIds = new List<string>();

        /// <summary>
        /// Stores the lock to protect debugModeSession reference
        /// </summary>
        private object lockThis = new object();

        /// <summary>
        /// Private constructor for singleton class
        /// </summary>
        private InprocessSessions()
        {
        }

        /// <summary>
        /// Gets the instance of SessionStartInfo of the previous session
        /// If there's never a session added, returns null
        /// </summary>
        public SessionStartInfo PreviousSessionStartInfo
        {
            get { return this.startInfo; }
        }

        /// <summary>
        /// Gets the singleton instance of the InprocessSessions class
        /// </summary>
        /// <returns>returns the singleton instance</returns>
        public static InprocessSessions GetInstance()
        {
            return instance;
        }

        /// <summary>
        /// Allocates session id for a new session
        /// </summary>
        /// <returns>returns the new session id</returns>
        public string AllocateSessionId(bool durable)
        {
            lock (this.lockThis)
            {
                if (this.debugModeSession == null)
                {
                    return DebugModeSessionId;
                }
                else
                {
                    throw new SessionException(SOAFaultCode.DebugModeNotSupportConcurrentSession, SR.DebugModeNotSupportConcurrentSession);
                }
            }
        }

        /// <summary>
        /// Fetch session information
        /// </summary>
        /// <param name="sessionId">indicating session id</param>
        /// <returns>returns session info instance</returns>
        public SessionInfo FetchSessionInfo(string sessionId)
        {
            return (SessionInfo)this.FetchSessionInstance(sessionId).Info;
        }

        /// <summary>
        /// Fetch session instance
        /// </summary>
        /// <param name="sessionId">indicating session id</param>
        /// <returns>returns session instance</returns>
        public SessionBase FetchSessionInstance(string sessionId)
        {
            lock (this.lockThis)
            {
                // this.debugModeSession has not been assigned, this means no inprocess broker has been created
                if (this.debugModeSession == null)
                {
                    if (sessionId.Equals(DebugModeSessionId))
                    {
                        // In debug mode, simulate an job already finished exception here
                        throw new SessionException(SOAFaultCode.Session_ValidateJobFailed_AlreadyFinished, String.Format(SR.Session_ValidateJobFailed_AlreadyFninshed, sessionId));
                    }
                    else
                    {
                        // For inprocess broker, throw a specific exception (Bug 11525)
                        throw new SessionException(SOAFaultCode.InprocessBroker_InvalidSessionId, String.Format(SR.InprocessBroker_InvalidSessionId, sessionId));
                    }
                }
                else if (!this.debugModeSession.Id.Equals(sessionId))
                {
                    // Throw invalid session id exception if session id does not match
                    throw new SessionException(SOAFaultCode.InvalidSessionId, String.Format(SR.Broker_InvalidSessionId, sessionId));
                }
                else
                {
                    return this.debugModeSession;
                }
            }
        }

        /// <summary>
        /// Add session into inprocess sessions
        /// </summary>
        /// <param name="session">indicating the session instance</param>
        /// <param name="startInfo">indicating the session start information</param>
        public void AddSession(SessionBase session, SessionStartInfo startInfo)
        {
            lock (this.lockThis)
            {
                if (this.debugModeSession == null)
                {
                    if (!this.removedSessionIds.Contains(session.Id))
                    {
                        this.debugModeSession = session;
                        this.startInfo = startInfo;
                    }
                }
                else
                {
                    if (startInfo.DebugModeEnabled)
                    {
                        throw new SessionException(SOAFaultCode.DebugModeNotSupportConcurrentSession, SR.DebugModeNotSupportConcurrentSession);
                    }
                    else
                    {
                        throw new SessionException(SOAFaultCode.InprocessNotSupportConcurrentSession, SR.InprocessNotSupportConcurrentSession);
                    }
                }
            }
        }

        /// <summary>
        /// Removes session instance
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        public void RemoveSessionInstance(string sessionId)
        {
            lock (this.lockThis)
            {
                if (this.debugModeSession != null && this.debugModeSession.Id.Equals(sessionId))
                {
                    this.debugModeSession = null;
                }
                else
                {
                    this.removedSessionIds.Add(sessionId);
                }
            }
        }
    }
}
