// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session
{
    using System;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.Telepathy.Session.Internal.SessionFactory;

    /// <summary>
    ///   <para>Represents a durable session that binds a client application 
    /// to a service that supports the service-oriented architecture (SOA) programming model  
    /// that is based on the Windows Communication Foundation (WCF). A durable 
    /// session is a session that can recover from hardware or software failure.</para> 
    /// </summary>
    /// <remarks>
    ///   <para>You must dispose of this object when you finish using it. You can do this by calling the 
    /// <see cref="DurableSession.CreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo)" /> or 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.EndCreateSession(System.IAsyncResult)" /> method within a 
    /// <see href="http://go.microsoft.com/fwlink/?LinkID=177731">using Statement</see> (http://go.microsoft.com/fwlink/?LinkID=177731) in 
    /// C#, or by calling the  
    /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Dispose" /> method.</para>
    /// </remarks>
    public class DurableSession : SessionBase
    {
        /// <summary>
        /// Initializes a new instance of the PersistantSession class
        /// </summary>
        /// <param name="info">Session Info</param>
        /// <param name="headnode">the head node machine name.</param>
        /// <param name="binding">indicating the binding</param>
        public DurableSession(SessionInfoBase info, string headnode, Binding binding)
            : base(info, headnode, binding)
        {
        }

        /// <summary>
        ///   <para>Attaches a service-oriented architecture (SOA) client to an 
        /// existing durable session by using the specified information about the session.</para>
        /// </summary>
        /// <param name="attachInfo">
        ///   <para>A 
        /// <see cref="SessionAttachInfo" /> object that specifies information about the durable session to which you want to attach the SOA client, including the name of the head node for the cluster that hosts the session and the identifier of the session.</para> 
        /// </param>
        /// <returns>
        ///   <para>A <see cref="DurableSession" /> that represents the durable session to which the client attached.</para>
        /// </returns>
        public static DurableSession AttachSession(SessionAttachInfo attachInfo)
        {
            return AttachSession(attachInfo, null);
        }

        /// <summary>
        /// Attach to a existing session with the session id
        /// </summary>
        /// <param name="attachInfo">The attach info</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A persistent session</returns>
        public static DurableSession AttachSession(SessionAttachInfo attachInfo, Binding binding)
        {
            return AttachSessionAsync(attachInfo, binding).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Attach to a existing session async
        /// </summary>
        /// <param name="attachInfo">The attach info</param>
        /// <returns>A persistent session</returns>
        public static async Task<DurableSession> AttachSessionAsync(SessionAttachInfo attachInfo)
        {
            return await AttachSessionAsync(attachInfo, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Attach to a existing session async
        /// </summary>
        /// <param name="attachInfo">The attach info</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A persistent session</returns>
        public static async Task<DurableSession> AttachSessionAsync(SessionAttachInfo attachInfo, Binding binding)
        {
            Utility.ThrowIfNull(attachInfo, "attachInfo");

            Utility.ThrowIfEmpty(attachInfo.Headnode, "headNode");

            return (DurableSession)await new SessionFactory().AttachSession(attachInfo, true, Timeout.Infinite, binding)
                                       .ConfigureAwait(false);
        }

        public static DurableSession CreateSession(SessionStartInfo startInfo)
        {
            return CreateSession(startInfo, null);
        }

        /// <summary>
        /// Create a persistent session
        /// </summary>
        /// <typeparam name="TContract">The service contract type</typeparam>
        /// <param name="startInfo">The session start info</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A persistent session instance</returns>
        public static DurableSession CreateSession(SessionStartInfo startInfo, Binding binding)
        {
            return CreateSessionAsync(startInfo, binding).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Create a persistent session async
        /// </summary>
        /// <param name="startInfo">The session start info</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A persistent session instance</returns>
        public static async Task<DurableSession> CreateSessionAsync(SessionStartInfo startInfo, Binding binding)
        {
            Utility.ThrowIfNull(startInfo, "startInfo");

            if (Utility.IsHpcSessionType(startInfo.GetType()))
            {
                SessionStartInfo sessionStartInfo = startInfo;

                Utility.ThrowIfEmpty(sessionStartInfo.Headnode, "headNode");

                if (!sessionStartInfo.DebugModeEnabled && sessionStartInfo.UseInprocessBroker)
                {
                    throw new ArgumentException(SR.InprocessBroker_NotSupportDurableSession, "UseInprocessBroker");
                }

                return (DurableSession)await new SessionFactory()
                                           .CreateSession(
                                               startInfo,
                                               true,
                                               Constant.DefaultCreateSessionTimeout,
                                               binding).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(SR.BackendNotSupported, "startInfo");
            }
        }
    }
}