// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;

    /// <summary>
    /// Factory to create inprocess broker
    /// </summary>
    public class InprocessBrokerFactory : IBrokerFactory
    {
        /// <summary>
        /// Stores the head node
        /// </summary>
        private string headNode;

        /// <summary>
        /// Stores a value indicating whether it is a durable session
        /// </summary>
        private bool durable;

        /// <summary>
        /// Initializes a new instance of the InprocessBrokerFactory class
        /// </summary>
        /// <param name="headNode">indicating the head node</param>
        /// <param name="durable">indicating whether it is a durable session</param>
        public InprocessBrokerFactory(string headNode, bool durable)
        {
            this.headNode = headNode;
            this.durable = durable;
        }

        /// <summary>
        /// Create broker
        /// </summary>
        /// <param name="startInfo">indicating the session start information</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="targetTimeout">indicating the target timeout</param>
        /// <param name="eprs">indicating the broker epr list</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>returns the broker initialization result</returns>
        public async Task<SessionBase> CreateBroker(SessionStartInfo startInfo, string sessionId, DateTime targetTimeout, string[] eprs, Binding binding)
        {
            return await this.CreateBrokerInternal(startInfo, sessionId, false, binding).ConfigureAwait(false);
        }

        /// <summary>
        /// Attach to a broker, returns session instance
        /// </summary>
        /// <param name="attachInfo">indicating the attach information</param>
        /// <param name="info">indicating the session info to be updated</param>
        /// <param name="timeout">indicating the timeout</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>returns the session instance</returns>
        public async Task<SessionBase> AttachBroker(SessionAttachInfo attachInfo, SessionInfo info, TimeSpan timeout, Binding binding)
        {
            SessionBase session;

            try
            {
                session = InprocessSessions.GetInstance().FetchSessionInstance(attachInfo.SessionId);
            }
            catch (SessionException e)
            {
                if (this.durable && attachInfo.SessionId == InprocessSessions.DebugModeSessionId && e.ErrorCode == SOAFaultCode.Session_ValidateJobFailed_AlreadyFinished)
                {
                    session = null;
                }
                else
                {
                    throw;
                }
            }


            if (session == null)
            {
                if (this.durable)
                {
                    if (attachInfo.SessionId != InprocessSessions.DebugModeSessionId)
                    {
                        throw new SessionException(SOAFaultCode.InvalidSessionId, String.Format(SR.Broker_InvalidSessionId, attachInfo.SessionId));
                    }

                    SessionStartInfo startInfo = InprocessSessions.GetInstance().PreviousSessionStartInfo;
                    if (startInfo != null)
                    {
                        DateTime targetTimeout;
                        if (timeout == TimeSpan.MaxValue)
                        {
                            targetTimeout = DateTime.MaxValue;
                        }
                        else
                        {
                            targetTimeout = DateTime.Now.Add(timeout);
                        }

                        return await this.CreateBrokerInternal(startInfo, attachInfo.SessionId, true, binding).ConfigureAwait(false);
                    }
                }

                throw new SessionException(SOAFaultCode.Session_ValidateJobFailed_AlreadyFinished, SR.Session_ValidateJobFailed_AlreadyFninshed);
            }
            else
            {
                if (this.durable)
                {
                    if (session is V3Session)
                    {
                        throw new SessionException(SOAFaultCode.InvalidAttachInteractiveSession, SR.InvalidAttachInteractiveSession);
                    }
                }
                else
                {
                    if (session is DurableSession)
                    {
                        throw new SessionException(SOAFaultCode.InvalidAttachDurableSession, SR.InvalidAttachDurableSession);
                    }
                }

                Debug.Assert(session.Info is SessionInfo, "[InprocessBrokerFactory] session.Info must be the type SessionInfo for inprocess broker.");
                SessionInfo sessionInfo = (SessionInfo)session.Info;
                Debug.Assert(sessionInfo.InprocessBrokerAdapter != null, "[InprocessBrokerFactory] session.Info.InprocessBrokerAdapter must be not null for inprocess broker.");
                sessionInfo.InprocessBrokerAdapter.Attach(session.Id);
            }

            return session;
        }

        /// <summary>
        /// Create broker
        /// </summary>
        /// <param name="startInfo">indicating the session start information</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="targetTimeout">indicating the target timeout</param>
        /// <param name="eprs">indicating the broker epr list</param>
        /// <param name="attached">indicating whether it is attaching</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>returns the broker initialization result</returns>
        private Task<SessionBase> CreateBrokerInternal(SessionStartInfo startInfo, string sessionId, bool attached, Binding binding)
        {
            InprocBrokerAdapter adapter = new InprocBrokerAdapter(startInfo, attached, startInfo.DebugModeEnabled, binding);
            BrokerInitializationResult result;
            if (this.durable)
            {
                result = adapter.CreateDurable(startInfo.Data, sessionId);
            }
            else
            {
                result = adapter.Create(startInfo.Data, sessionId);
            }

            SessionInfo info = SessionBase.BuildSessionInfo(result, this.durable, sessionId, String.Empty, startInfo.Data.ServiceVersion, startInfo);
            info.InprocessBrokerAdapter = adapter;

            SessionBase session;
            if (this.durable)
            {
                session = new DurableSession(info, startInfo.Headnode, null);
            }
            else
            {
                session = new V3Session(info, startInfo.Headnode, startInfo.ShareSession, null);
            }

            InprocessSessions.GetInstance().AddSession(session, startInfo);

#if net40
            return TaskEx.FromResult(session);
#else
            return Task.FromResult(session);
#endif
        }
    }
}
