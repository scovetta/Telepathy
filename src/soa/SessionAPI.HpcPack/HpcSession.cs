//------------------------------------------------------------------------------
// <copyright file="Session.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The implementation of the Session Class
// </summary>
//------------------------------------------------------------------------------

using Microsoft.Hpc.Scheduler.Session.HpcPack.Internal;

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
    internal class HpcV3Session : V3Session
    {
        /// <summary>
        /// Initializes a new instance of the Session class
        /// </summary>
        /// <param name="info">session info</param>
        /// <param name="headnode">headnode name</param>
        /// <param name="sharedSession">if this is a shared session</param>
        /// <param name="binding">indicating the binding</param>
        internal HpcV3Session(SessionInfoBase info, string headnode, bool autoClose, Binding binding)
            : base(info, headnode, autoClose, binding)
        {
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
                return (V3Session)await HpcSessionFactory.BuildSessionFactory(startInfo).CreateSession(startInfo, false, Constant.DefaultCreateSessionTimeout, binding).ConfigureAwait(false);
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

                return (V3Session)await HpcSessionFactory.BuildSessionFactory(attachInfo).AttachSession(attachInfo, false, Timeout.Infinite, binding).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(SR.BackendNotSupported, "attachInfo");
            }
        }
    }
}
