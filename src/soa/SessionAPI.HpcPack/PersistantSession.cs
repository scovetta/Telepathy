//------------------------------------------------------------------------------
// <copyright file="PersistantSession.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The implementation of the Persistant Session Class
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///   <para>Represents a durable session that binds a client application 
    /// to a service that supports the service-oriented architecture (SOA) programming model  
    /// that is based on the Windows Communication Foundation (WCF). A durable 
    /// session is a session that can recover from hardware or software failure.</para> 
    /// </summary>
    /// <remarks>
    ///   <para>You must dispose of this object when you finish using it. You can do this by calling the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.CreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo)" /> or 
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
        internal DurableSession(SessionInfoBase info, string headnode, Binding binding)
            : base(info, headnode, binding)
        {
        }

        /// <summary>
        /// Create a persistant session
        /// </summary>
        /// <typeparam name="TContract">The service contract type</typeparam>
        /// <param name="startInfo">The session start info</param>
        /// <returns>A persistant session instance</returns>
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
        /// <returns>A persistent session instance</returns>
        public static async Task<DurableSession> CreateSessionAsync(SessionStartInfo startInfo)
        {
            return await CreateSessionAsync(startInfo, null).ConfigureAwait(false);
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

                return (DurableSession)await SessionFactory.BuildSessionFactory(startInfo).CreateSession(startInfo, true, Constant.DefaultCreateSessionTimeout, binding).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(SR.BackendNotSupported, "startInfo");
            }
        }

        /// <summary>
        ///   <para>Attaches a service-oriented architecture (SOA) client to an 
        /// existing durable session by using the specified information about the session.</para>
        /// </summary>
        /// <param name="attachInfo">
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionAttachInfo" /> object that specifies information about the durable session to which you want to attach the SOA client, including the name of the head node for the cluster that hosts the session and the identifier of the session.</para> 
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession" /> that represents the durable session to which the client attached.</para>
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

            if (Utility.IsHpcSessionType(attachInfo.GetType()))
            {
                Utility.ThrowIfEmpty(attachInfo.Headnode, "headNode");

                return (DurableSession)await SessionFactory.BuildSessionFactory(attachInfo).AttachSession(attachInfo, true, Timeout.Infinite, binding).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException(SR.BackendNotSupported, "attachInfo");
            }
        }

        /// <summary>
        ///   <para>Creates a new durable session asynchronously.</para>
        /// </summary>
        /// <param name="startInfo">
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> object that contains the information that you want to use to start the durable session, such as the head node of the HPC cluster on which you want to start the session and the name of the service </para> 
        /// </param>
        /// <param name="callback">
        ///   <para>An 
        /// <see cref="System.AsyncCallback" /> object that identifies a callback method that you implement to process the results of the asynchronous operation in a separate thread and is called when the asynchronous operation completes. Can be  
        /// null.</para>
        /// </param>
        /// <param name="asyncState">
        ///   <para>An object that includes user-defined data to pass to the callback method. To get the user-defined data in the callback method, use the 
        /// <see cref="System.IAsyncResult.AsyncState" /> property of the 
        /// <see cref="System.IAsyncResult" /> interface that is passed to your callback method. Can be 
        /// null.</para>
        /// </param>
        /// <returns>
        ///   <para>An 
        /// <see cref="System.IAsyncResult" /> interface that represents the status of the asynchronous operation. Use this interface when you call the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.EndCreateSession(System.IAsyncResult)" /> method.</para>
        /// </returns>
        public static IAsyncResult BeginCreateSession(SessionStartInfo startInfo, AsyncCallback callback, object asyncState)
        {
            return BeginCreateSession(startInfo, null, callback, asyncState);
        }

        /// <summary>
        /// Async create a persistent session
        /// </summary>
        /// <typeparam name="TContract">The service contract type</typeparam>
        /// <param name="startInfo">The session start info</param>
        /// <param name="binding">indicating the binding</param>
        /// <param name="callback">The async callback</param>
        /// <param name="asyncState">the async state</param>
        /// <returns>A async result</returns>
        public static IAsyncResult BeginCreateSession(SessionStartInfo startInfo, Binding binding, AsyncCallback callback, object asyncState)
        {
            Utility.ThrowIfNull(startInfo, "startInfo");

            if (Utility.IsHpcSessionType(startInfo.GetType()))
            {
                SessionStartInfo sessionStartInfo = (SessionStartInfo)startInfo;
                DurableSession.CheckSessionStartInfo(sessionStartInfo);

                return new DurableSessionAsyncResult(sessionStartInfo, binding, callback, asyncState);
            }
            else
            {
                throw new ArgumentException(SR.BackendNotSupported, "startInfo");
            }
        }

        /// <summary>
        ///   <para>Completes the asynchronous creation of a durable session.</para>
        /// </summary>
        /// <param name="result">
        ///   <para>An <see cref="System.IAsyncResult" /> interface that represents the status of the asynchronous operation. </para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession" /> object that represents the durable session.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Typically, you call this method from the callback method that you specify when you call the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.BeginCreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.AsyncCallback,System.Object)" /> method. If you use a callback method, pass the same  
        /// <see cref="System.IAsyncResult" /> object to the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.EndCreateSession(System.IAsyncResult)" /> method that is passed to your callback method. If you do not use a callback method, pass the  
        /// <see cref="System.IAsyncResult" /> object that the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.BeginCreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.AsyncCallback,System.Object)" /> method returns to the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.EndCreateSession(System.IAsyncResult)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.DurableSession.BeginCreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.AsyncCallback,System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.EndCreateSession(System.IAsyncResult)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.DurableSession.CreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo)" />
        public static DurableSession EndCreateSession(IAsyncResult result)
        {
            Utility.ThrowIfNull(result, "result");

            DurableSessionAsyncResult ar = result as DurableSessionAsyncResult;
            if (ar == null)
            {
                throw new ArgumentException(SR.NotCorrectAsyncResultObj, "result");
            }

            if (ar.Disposed)
            {
                throw new ObjectDisposedException("result");
            }

            if (!result.IsCompleted)
            {
                result.AsyncWaitHandle.WaitOne();
            }

            Debug.Assert(result.IsCompleted, "Why the operation is not finished after event waiting returns");

            try
            {
                if (ar.ExceptionResult != null)
                {
                    Exception ex = ar.ExceptionResult;
                    throw ex;
                }

                Debug.Assert(ar.Session != null, "Do not support cancel create session for DurableSession.BeginCreateSession. So if there's no exception, session instance must not be null.");
                return (DurableSession)ar.Session;
            }
            finally
            {
                ar.Close();
            }
        }

        /// <summary>
        /// the help function to check the sanity of SessionStartInfo before creating sesison.
        /// </summary>
        /// <param name="startInfo"></param>
        internal static new void CheckSessionStartInfo(SessionStartInfo startInfo)
        {
            SessionBase.CheckSessionStartInfo(startInfo);

            if (startInfo.TransportScheme == TransportScheme.Http)
            {
                throw new NotSupportedException(SR.TransportSchemeNotSupport);
            }

            if (startInfo.TransportScheme == TransportScheme.WebAPI)
            {
                throw new NotSupportedException(SR.TransportSchemeNotSupport);
            }
        }
    }
}
