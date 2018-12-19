//------------------------------------------------------------------------------
// <copyright file="Session.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The implementation of the Session Class
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The jobs associated with this object will be automatically closed on disposing this object
    /// </summary>
    internal class V3Session : SessionBase
    {
        /// <summary>
        /// Whether the jobs will be closed on disposing this session
        /// </summary>
        private bool autoCloseJob;

        /// <summary>
        /// Initializes a new instance of the Session class
        /// </summary>
        /// <param name="info">session info</param>
        /// <param name="headnode">headnode name</param>
        /// <param name="sharedSession">if this is a shared session</param>
        /// <param name="binding">indicating the binding</param>
        internal V3Session(SessionInfoBase info, string headnode, bool autoClose, Binding binding)
            : base(info, headnode, binding)
        {
            this.autoCloseJob = !autoClose;

            SessionBase.TraceSource.TraceInformation("[Session:{0}] Interactive session created.", info.Id);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the job will be automatically closed on disposal of this session
        /// </summary>
        public bool AutoClose
        {
            get { return this.autoCloseJob; }
            set
            {
                // we don't protect here since this is simple type assignment.
                this.autoCloseJob = value;
            }
        }

        /// <summary>
        /// Gets the Net.Tcp endpoint reference for load balancer of this session
        /// </summary>
        public EndpointAddress NetTcpEndpointReference
        {
            get
            {
                if ((this.Info.TransportScheme & TransportScheme.NetTcp) == 0 || this.Info.UseInprocessBroker || !(this.Info is SessionInfo))
                {
                    return null;
                }
                else
                {
                    return GenerateEndpointAddress(((SessionInfo)this.Info).BrokerEpr, TransportScheme.NetTcp);
                }
            }
        }

        /// <summary>
        /// Gets the Http endpoint reference for load balancer of this session
        /// </summary>
        public EndpointAddress HttpEndpointReference
        {
            get
            {
                if ((this.Info.TransportScheme & (TransportScheme.Http | TransportScheme.NetHttp)) == 0 || this.Info.UseInprocessBroker || !(this.Info is SessionInfo))
                {
                    return null;
                }
                else
                {
                    return GenerateEndpointAddress(((SessionInfo)this.Info).BrokerEpr, TransportScheme.Http);
                }
            }
        }

        /// <summary>
        /// Asynchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="startInfo">The session start info for creating the service session</param>
        /// <param name="callback">A callback to be invoked after an endpoint address is got</param>
        /// <param name="state">The parameter of the callback</param>
        /// <returns>Async result</returns>
        public static IAsyncResult BeginCreateSession(
                                                      SessionStartInfo startInfo,
                                                      AsyncCallback callback,
                                                      object state)
        {
            return BeginCreateSession(startInfo, null, callback, state);
        }

        /// <summary>
        /// Asynchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="startInfo">The session start info for creating the service session</param>
        /// <param name="binding">indicating the binding</param>
        /// <param name="callback">A callback to be invoked after an endpoint address is got</param>
        /// <param name="state">The parameter of the callback</param>
        /// <returns>Async result</returns>
        public static IAsyncResult BeginCreateSession(
                                                      SessionStartInfo startInfo,
                                                      Binding binding,
                                                      AsyncCallback callback,
                                                      object state)
        {
            Utility.ThrowIfNull(startInfo, "startInfo");

            if (Utility.IsHpcSessionType(startInfo.GetType()))
            {
                SessionStartInfo sessionStartInfo = (SessionStartInfo)startInfo;
                SessionBase.CheckSessionStartInfo(sessionStartInfo);

                if (sessionStartInfo.TransportScheme == TransportScheme.WebAPI)
                {
                    // return new SessionAsyncResultForWebAPI(sessionStartInfo, callback, state);
                    throw new NotSupportedException();
                }
                else
                {
                    return new SessionAsyncResult(sessionStartInfo, binding, callback, state);
                }
            }
            else
            {
                throw new ArgumentException(SR.BackendNotSupported, "startInfo");
            }
        }

        /// <summary>
        /// Asynchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="result">The IAsyncResult object returned from BeginSubmitJobAndGetSession method</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static V3Session EndCreateSession(IAsyncResult result)
        {
            Utility.ThrowIfNull(result, "result");

            SessionAsyncResultBase ar = result as SessionAsyncResultBase;
            if (ar == null || ar is DurableSessionAsyncResult)
            {
                throw new ArgumentException(SR.NotCorrectAsyncResultObj, "result");
            }

            if (ar.Canceled)
            {
                throw new SessionException(SOAFaultCode.CreateSessionCanceled, SR.CreateSessionCanceled);
            }

            if (ar.Disposed)
            {
                throw new ObjectDisposedException("result");
            }

            if (!result.IsCompleted)
            {
                result.AsyncWaitHandle.WaitOne();
            }

            Debug.Assert(result.IsCompleted, "Reached the completed state.");

            try
            {
                if (ar.ExceptionResult != null)
                {
                    Exception ex = ar.ExceptionResult;
                    throw ex;
                }

                Debug.Assert(ar.Session != null, "Session creation should either be canceled, or have an exception or session instance as the result.");
                return (V3Session)ar.Session;
            }
            finally
            {
                ar.Close();
            }
        }

        /// <summary>
        /// Cancel the session creating before it's finished.
        /// </summary>
        /// <param name="result">Async Result</param>
        public static void CancelCreateSession(IAsyncResult result)
        {
            Utility.ThrowIfNull(result, "result");

            SessionAsyncResultBase ar = result as SessionAsyncResultBase;
            if (ar == null)
            {
                throw new ArgumentException(SR.NotCorrectAsyncResultObj, "result");
            }

            if (ar.Disposed)
            {
                throw new ObjectDisposedException("result");
            }

            // TODO: Do we really care about the race condition
            if (ar.IsCompleted)
            {
                throw new InvalidOperationException();
            }

            ar.Cancel();

            // Instead of calling EndCreateSession(), we should directly call
            // the Close method to dispose related resources
            ar.Close();
        }

        /// <summary>
        /// Synchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="startInfo">The session start info for creating the service session</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static V3Session CreateSession(SessionStartInfo startInfo)
        {
            return CreateSessionAsync(startInfo).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="startInfo">The session start info for creating the service session</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static async Task<V3Session> CreateSessionAsync(SessionStartInfo startInfo)
        {
            return await CreateSessionAsync(startInfo, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="startInfo">The session start info for creating the service session</param>
        /// <param name="binding">indicting the binding</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static V3Session CreateSession(SessionStartInfo startInfo, Binding binding)
        {
            return CreateSessionAsync(startInfo, binding).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="startInfo">The session start info for creating the service session</param>
        /// <param name="binding">indicting the binding</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static async Task<V3Session> CreateSessionAsync(SessionStartInfo startInfo, Binding binding)
        {
            Utility.ThrowIfNull(startInfo, "startInfo");

            Utility.ThrowIfEmpty(startInfo.Headnode, "headNode");

            if (Utility.IsHpcSessionType(startInfo.GetType()))
            {
                return (V3Session)await SessionFactory.BuildSessionFactory(startInfo).CreateSession(startInfo, false, Constant.DefaultCreateSessionTimeout, binding).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(SR.BackendNotSupported, "startInfo");
            }
        }

        /// <summary>
        /// This method closes the session with the given ID
        /// </summary>
        /// <param name="headNode">Headnode name</param>
        /// <param name="sessionId">The ID of the session to be closed</param>
        /// <param name="isAadUser">If the session is belong to a AAD user</param>
        public static void CloseSession(string headNode, int sessionId, bool isAadUser)
        {
            CloseSession(headNode, sessionId, null, isAadUser);
        }

        /// <summary>
        /// This method closes the session with the given ID
        /// </summary>
        /// <param name="headNode">Headnode name</param>
        /// <param name="sessionId">The ID of the session to be closed</param>
        /// <param name="binding">indicting the binding</param>
        /// <param name="isAadUser">If the session is belong to a AAD user</param>
        public static void CloseSession(string headNode, int sessionId, Binding binding, bool isAadUser)
        {
            CloseSessionAsync(headNode, sessionId, binding, isAadUser, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// This method closes the session async with the given ID
        /// </summary>
        /// <param name="headNode">Headnode name</param>
        /// <param name="sessionId">The ID of the session to be closed</param>
        /// <param name="binding">indicting the binding</param>
        /// <param name="isAadUser">If the session is belong to a AAD user</param>
        /// <param name="token">The cancellation token.</param>
        public static async Task CloseSessionAsync(string headNode, int sessionId, Binding binding, bool isAadUser, CancellationToken token)
        {
            Utility.ThrowIfEmpty(headNode, "headNode");

            SessionLauncherClient client = null;
            BrokerLauncherClient broker = null;

            string headNodeMachine = await HpcContext.GetOrAdd(headNode, token).ResolveSessionLauncherNodeAsync().ConfigureAwait(false);

            try
            {
                client = new SessionLauncherClient(headNodeMachine, binding, isAadUser);
                client.InnerChannel.OperationTimeout = GetTimeout(DateTime.MaxValue);
                //TODO: need to change the endpoint prefix for https
                SessionInfo info = null;
                if (binding is NetTcpBinding)
                {
                    info = Utility.BuildSessionInfoFromDataContract(await client.GetInfoV5Async(SessionLauncherClient.EndpointPrefix, sessionId).ConfigureAwait(false));
                }
#if !net40
                else if (binding is BasicHttpBinding || binding is NetHttpBinding || binding is NetHttpsBinding)
                {
                    info = Utility.BuildSessionInfoFromDataContract(await client.GetInfoV5Async(SessionLauncherClient.HttpsEndpointPrefix, sessionId).ConfigureAwait(false));
                }
#endif
                broker = new BrokerLauncherClient(info, binding, new Uri(info.BrokerLauncherEpr));
                broker.InnerChannel.OperationTimeout = GetTimeout(DateTime.MaxValue);
                broker.Close(sessionId);
            }
            catch (FaultException<SessionFault> e)
            {
                throw Utility.TranslateFaultException(e);
            }
            finally
            {
                if (client != null)
                {
                    Utility.SafeCloseCommunicateObject(client);
                }

                if (broker != null)
                {
                    Utility.SafeCloseCommunicateObject(broker);
                }
            }
        }

        /// <summary>
        /// Attach to a existing session with the session attach info
        /// </summary>
        /// <param name="attachInfo">The attach info</param>
        /// <returns>A persistant session</returns>
        public static V3Session AttachSession(SessionAttachInfo attachInfo)
        {
            return AttachSession(attachInfo, null);
        }

        /// <summary>
        /// Attach to a existing session with the session attach info and binding
        /// </summary>
        /// <param name="attachInfo">The attach info</param>
        /// <param name="binding">indicting the binding</param>
        /// <returns>A persistant session</returns>
        public static V3Session AttachSession(SessionAttachInfo attachInfo, Binding binding)
        {
            return AttachSessionAsync(attachInfo, binding).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Attach to a existing session async with the session attach info
        /// </summary>
        /// <param name="attachInfo">The attach info</param>
        /// <returns>A persistant session</returns>
        public static async Task<V3Session> AttachSessionAsync(SessionAttachInfo attachInfo)
        {
            return await AttachSessionAsync(attachInfo, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Attach to a existing session async with the session attach info and binding
        /// </summary>
        /// <param name="attachInfo">The attach info</param>
        /// <param name="binding">indicting the binding</param>
        /// <returns>A persistant session</returns>
        public static async Task<V3Session> AttachSessionAsync(SessionAttachInfo attachInfo, Binding binding)
        {
            Utility.ThrowIfNull(attachInfo, "attachInfo");

            if (Utility.IsHpcSessionType(attachInfo.GetType()))
            {
                Utility.ThrowIfEmpty(attachInfo.Headnode, "headNode");

                return (V3Session)await SessionFactory.BuildSessionFactory(attachInfo).AttachSession(attachInfo, false, Timeout.Infinite, binding).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(SR.BackendNotSupported, "attachInfo");
            }
        }

        /// <summary>
        /// Dispose the service job if autoclose is true
        /// </summary>
        /// <param name="disposing">if we called from destructor</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Nothing to do here
            }

            // Auto close the jobs if the autoCloseJob is set to true
            if (autoCloseJob)
            {
                try
                {
                    IBrokerLauncher broker = this.BrokerLauncherClientFactory.GetBrokerLauncherClient(-1);
                    broker.Close(this.Id);
                }
                catch (Exception e)
                {
                    // Swallow the exception
                    SessionBase.TraceSource.TraceInformation(e.ToString());
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Generate endpoint address by scheme from epr list
        /// </summary>
        /// <param name="eprList">indicating the epr list</param>
        /// <param name="scheme">indicating the scheme</param>
        /// <returns>endpoint address</returns>
        private static EndpointAddress GenerateEndpointAddress(string[] eprList, TransportScheme scheme)
        {
            int index = (int)Math.Log((int)scheme, 2);
            return new EndpointAddress(eprList[index]);
        }
    }
}
