namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Internal;

    public class Session : IDisposable
    {
        /// <summary>
        /// v3 session instance
        /// </summary>
        protected V3Session v3session;

        public Session(V3Session v3session)
        {
            this.v3session = v3session;
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the session.</para>
        /// </summary>
        /// <value>
        ///   <para>An identifier that uniquely identifies the session.</para>
        /// </value>
        public string Id
        {
            get
            {
                return this.v3session.Id;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.v3session?.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   <para>Closes the session without finishing the job for the session or deleting response messages.</para>
        /// </summary>
        /// <remarks>
        ///   <para>This method is equivalent to the
        /// <see cref="Close()" /> method with the purge parameter set to
        /// False. To finish the job for the session if the job is still active and delete the response messages when you close the session, use the
        /// <see cref="Close()" /> or
        /// <see cref="Close()" /> method instead. </para>
        ///   <para>When you create a session, you will also start a new job. To close the job and the session, use
        /// <see cref="Close()" />(True). To close the job but keep the durable session active, use
        /// <see cref="Close()" />(False). If you use
        /// <see cref="Close()" />(False) on a durable session, you will still be able to attach to the session after the job completes by using the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Close()" />
        /// <seealso cref="Close()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close()" />
        public void Close()
        {
            this.v3session.Close();
        }

        /// <summary>
        ///   <para>Closes the session and optionally finishes the job for the session and deletes the response messages.</para>
        /// </summary>
        /// <param name="purge">
        ///   <para>A
        /// <see cref="System.Boolean" /> object that specifies whether to finish the job for the session and delete the response messages.
        /// True finishes the job for the session and deletes the response messages.
        /// False indicates that the method should not finish the job for the session and should not delete the response messages.</para>
        /// </param>
        /// <remarks>
        ///   <para>When you create a session, you will also start a new job. To close the job and the session, use
        /// <see cref="Close()" />(True). To close the job but keep the durable session active, use
        /// <see cref="Close()" />(False). If you use
        /// <see cref="Close()" />(False) on a durable session, you will still be able to attach to the session after the job completes by using the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method.</para>
        ///   <para>Calling this method with the <paramref name="purge" /> parameter set to
        /// False is equivalent to calling the
        /// <see cref="VisualStyleElement.ToolTip.Close" /> method.</para>
        ///   <para>The default timeout period for finishing the job and deleting the response
        /// messages is 60,000 milliseconds. To specify a specific length for the timeout period, use the
        /// <see cref="Close()" /> method instead.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.Close()" />
        /// <seealso cref="Close()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean,System.Int32)" />
        public void Close(bool purge)
        {
            this.v3session.Close(purge);
        }

        /// <summary>
        ///   <para>Closes the session, and optionally finishes the job for
        /// the session and deletes the response messages subject to the specified timeout period.</para>
        /// </summary>
        /// <param name="purge">
        ///   <para>A
        /// <see cref="System.Boolean" /> object that specifies whether to finish the job for the session and delete the response messages.
        /// True finishes the job for the session and deletes the response messages.
        /// False indicates that the method should not finish the job for the session and should not delete the response messages.</para>
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>Specifies the length of time in milliseconds that the method
        /// should wait for the job to finish and the response messages to be deleted.</para>
        /// </param>
        /// <exception cref="System.TimeoutException">
        ///   <para>Specifies that the job for the session did not finish or
        /// the response messages were not deleted before the end of the specified time period.</para>
        /// </exception>
        /// <remarks>
        ///   <para>To close the session subject to default timeout period for
        /// finishing the job and deleting the response messages of 60,000 milliseconds, use the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close()" /> or
        /// <see cref="Close()" /> method instead.</para>
        ///   <para>When you create a session, you will also start a new job. To close the job and the session, use
        /// <see cref="Close()" />(True). To close the job but keep the durable session active, use
        /// <see cref="Close()" />(False). If you use
        /// <see cref="Close()" />(False) on a durable session, you will still be able to attach to the session after the job completes by using the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.Close()" />
        /// <seealso cref="Close()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean,System.Int32)" />
        public void Close(bool purge, int timeoutMilliseconds)
        {
            this.v3session.Close(purge, timeoutMilliseconds);
        }

        public static Session CreateBrokerLayerSession(SessionStartInfo startInfo)
        {
            return CreateBrokerLayerSessionAsync(startInfo).GetAwaiter().GetResult();
        }

        public static async Task<Session> CreateBrokerLayerSessionAsync(SessionStartInfo startInfo)
        {
            IBrokerFactory brokerFactory = new V3BrokerFactory(false);
            DateTime targetTimeout = DateTime.Now.AddMilliseconds(Constant.DefaultCreateSessionTimeout);
            return new Session((V3Session)await brokerFactory.CreateBroker(startInfo, SessionStartInfo.StandaloneSessionId, targetTimeout, startInfo.BrokerLauncherEprs, null).ConfigureAwait(false));
        }


        /// <summary>
        /// create in process session
        /// </summary>
        /// <param name="startInfo"></param>
        /// <returns></returns>
        public static Session CreateCoreLayerSession(SessionStartInfo startInfo)
        {
            return CreateCoreLayerSessionAsync(startInfo).GetAwaiter().GetResult();
        }

        public static async Task<Session> CreateCoreLayerSessionAsync(SessionStartInfo startInfo)
        {
            InprocessBrokerFactory brokerFactory = new InprocessBrokerFactory(startInfo.Headnode, false);
            DateTime targetTimeout = DateTime.Now.AddMilliseconds(Constant.DefaultCreateSessionTimeout);
            return new Session((V3Session)await brokerFactory.CreateBroker(startInfo, SessionStartInfo.StandaloneSessionId, targetTimeout, null, null).ConfigureAwait(false));
        }

        /// <summary>
        /// Convert v3 session based object to SessionBase
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static implicit operator SessionBase(Session session)
        {
            return ToSessionBase(session);
        }

        public static SessionBase ToSessionBase(Session session)
        {
            return (SessionBase)session.v3session;
        }

        // TODO: design choice: completely synchronize api call
        /// <summary>
        ///   <para>Creates a session.</para>
        /// </summary>
        /// <param name="startInfo">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> class that contains information for starting the session.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> object that defines the session.</para>
        /// </returns>
        public static Session CreateSession(SessionStartInfo startInfo)
        {
            if (startInfo.IsNoSession)
            {
                if (startInfo.UseInprocessBroker)
                {
                    return CreateCoreLayerSession(startInfo);
                }
                else
                {
                    return CreateBrokerLayerSession(startInfo);
                }
            }
            else
            {
                return CreateSessionAsync(startInfo, null).GetAwaiter().GetResult();
            }
        }

        private static async Task<Session> CreateSessionAsync(SessionStartInfo startInfo, Binding binding)
        {
            Utility.ThrowIfNull(startInfo, "startInfo");

            return new Session(await V3Session.CreateSessionAsync(startInfo, binding).ConfigureAwait(false));
        }

        /// <summary>
        /// <para>
        /// Attaches an SOA client to an existing session by using the specified information about the session.
        /// </para>
        /// </summary>
        /// <param name="attachInfo">
        /// <para>
        /// A
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionAttachInfo"/> object that specifies information about the session to which you want to attach the SOA client, including the name of the head node for the cluster that hosts the session and the identifier of the session.
        /// </para>
        /// </param>
        /// <param name="binding">
        /// indicting the binding
        /// </param>
        /// <returns>
        /// <para>
        /// A <see cref="Microsoft.Hpc.Scheduler.Session.Session"/> that represents the session to which the client attached.
        /// </para>
        /// </returns>
        public static async Task<Session> AttachSessionAsync(SessionAttachInfo attachInfo, Binding binding)
        {
            Utility.ThrowIfNull(attachInfo, "attachInfo");

            return new Session(await V3Session.AttachSessionAsync(attachInfo, binding).ConfigureAwait(false));
        }

        public static async Task<Session> AttachSessionAsync(SessionAttachInfo attachInfo)
        {
            Utility.ThrowIfNull(attachInfo, "attachInfo");

            return new Session(await V3Session.AttachSessionAsync(attachInfo, null).ConfigureAwait(false));
        }

        public static Session AttachSession(SessionAttachInfo attachInfo, Binding binding)
        {
            return AttachSessionAsync(attachInfo, binding).GetAwaiter().GetResult();
        }

        public static Session AttachSession(SessionAttachInfo attachInfo)
        {
            return AttachSessionAsync(attachInfo).GetAwaiter().GetResult();
        }
    }
}