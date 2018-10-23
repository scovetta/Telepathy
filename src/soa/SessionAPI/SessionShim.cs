//------------------------------------------------------------------------------
// <copyright file="SessionShim.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The implementation of the Session Shim
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;
    using System.Windows.Forms.VisualStyles;

    using SoaService.DataClient;

    /// <summary>
    ///   <para>Use to create an HPC session that binds the client application to
    /// a service that supports the service-oriented architecture (SOA) programming model based on Windows Communication Foundation (WCF).</para>
    /// </summary>
    /// <remarks>
    ///   <para>You must dispose of this object when you are done.</para>
    ///   <para>For Windows HPC Server 2008, the
    ///
    /// <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> class directly included the Dispose and Finalize methods, rather than inheriting the methods from the
    /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase" /> class as in Microsoft HPC Pack:</para>
    /// </remarks>
    /// <example>
    ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853427(v=vs.85).aspx">Creating a SOA Client</see>.</para>
    /// </example>
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" />
    public class Session : IDisposable
    {
        /// <summary>
        /// v3 session instance
        /// </summary>
        V3Session v3session;

        /// <summary>
        /// create the session shim based on a v3 session
        /// </summary>
        /// <param name="v2session"></param>
        internal Session(V3Session v3session)
        {
            this.v3session = v3session;
        }

        /// <summary>
        ///   <para>Releases all unmanaged resources that are used by the session.</para>
        /// </summary>
        public void Dispose()
        {
            if (v3session != null)
                v3session.Dispose();

            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   <para>Indicates whether the session closes automatically when Dispose is called.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the session closes automatically; otherwise, it is False.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default is True for non-shared sessions and False for shared sessions.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.CloseSession(System.String,System.Int32)" />
        public bool AutoClose
        {
            get
            {
                return v3session.AutoClose;
            }
            set
            {
                v3session.AutoClose = value;
            }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the session.</para>
        /// </summary>
        /// <value>
        ///   <para>An identifier that uniquely identifies the session.</para>
        /// </value>
        public int Id
        {
            get
            {
                return v3session.Id;
            }
        }

        /// <summary>
        ///   <para>Retrieves the unique network address that a client uses to communicate with a service endpoint.</para>
        /// </summary>
        /// <value>
        ///   <para>The network address.</para>
        /// </value>
        /// <remarks>
        ///   <para>Use the endpoint when you construct the client proxy.</para>
        ///   <para>If you specify the NetTcp and Http transport schemes, this property contains the endpoint reference for the NetTcp endpoint.</para>
        ///   <para>Instead of using this property, you can use the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.HttpEndpointReference" /> and
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.NetTcpEndpointReference" /> properties to access the endpoint references.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853427(v=vs.85).asp">Creating a SOA Client</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EndpointAddresses" />
        public EndpointAddress EndpointReference
        {
            get
            {
                return v3session.EndpointReference;
            }
        }

        /// <summary>
        ///   <para>Retrieves the unique network address that a client uses to communicate with a service endpoint that specifies the NetTcp transport.</para>
        /// </summary>
        /// <returns />
        /// <remarks>
        ///   <para>Use the endpoint when you construct the client proxy.</para>
        ///   <para>This property is valid if the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> property is set to NetTcp.</para>
        ///   <para>This is the same value that is in <see cref="Microsoft.Hpc.Scheduler.Session.Session.EndpointReference" />.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EndpointAddresses" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.HttpEndpointReference" />
        public EndpointAddress NetTcpEndpointReference
        {
            get
            {
                return v3session.NetTcpEndpointReference;
            }
        }

        /// <summary>
        ///   <para>Closes the session without finishing the job for the session or deleting response messages.</para>
        /// </summary>
        /// <remarks>
        ///   <para>This method is equivalent to the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" /> method with the purge parameter set to
        /// False. To finish the job for the session if the job is still active and delete the response messages when you close the session, use the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" /> or
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean,System.Int32)" /> method instead. </para>
        ///   <para>When you create a session, you will also start a new job. To close the job and the session, use
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />(True). To close the job but keep the durable session active, use
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />(False). If you use
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />(False) on a durable session, you will still be able to attach to the session after the job completes by using the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close()" />
        public void Close()
        {
            v3session.Close();
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
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />(True). To close the job but keep the durable session active, use
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />(False). If you use
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />(False) on a durable session, you will still be able to attach to the session after the job completes by using the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method.</para>
        ///   <para>Calling this method with the <paramref name="purge" /> parameter set to
        /// False is equivalent to calling the
        /// <see cref="VisualStyleElement.ToolTip.Close" /> method.</para>
        ///   <para>The default timeout period for finishing the job and deleting the response
        /// messages is 60,000 milliseconds. To specify a specific length for the timeout period, use the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean,System.Int32)" /> method instead.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.Close()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean,System.Int32)" />
        public void Close(bool purge)
        {
            v3session.Close(purge);
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
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" /> method instead.</para>
        ///   <para>When you create a session, you will also start a new job. To close the job and the session, use
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />(True). To close the job but keep the durable session active, use
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />(False). If you use
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />(False) on a durable session, you will still be able to attach to the session after the job completes by using the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.Close()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean,System.Int32)" />
        public void Close(bool purge, int timeoutMilliseconds)
        {
            v3session.Close(purge, timeoutMilliseconds);
        }

        /// <summary>
        ///   <para>Retrieves the unique network address that a client uses to communicate with a service endpoint that specifies the Http transport.</para>
        /// </summary>
        /// <returns />
        /// <remarks>
        ///   <para>Use the endpoint when you construct the client proxy.</para>
        ///   <para>This property is valid if the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> property is set to Http.</para>
        ///   <para>This is the same value that is in
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.EndpointReference" /> if Http is the only transport specified.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EndpointAddresses" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.NetTcpEndpointReference" />
        public EndpointAddress HttpEndpointReference
        {
            get
            {
                return v3session.HttpEndpointReference;
            }
        }

        /// <summary>
        ///   <para>Gets information about the version of the HPC Pack that is installed on the head node of the cluster that hosts the session.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Version" /> that contains the version information.</para>
        /// </value>
        /// <remarks>
        ///   <para>The
        /// <see cref="System.Version.Build" /> and
        /// <see cref="System.Version.Revision" /> portions of the version that the
        /// <see cref="System.Version" /> object represents are not defined for the HPC Pack.</para>
        ///   <para>HPC Pack 2008 is version 2.0. HPC Pack 2008 R2 is version 3.0.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.ClientVersion" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.ServerVersion" />
        /// <seealso cref="System.Version" />
        public Version ServerVersion
        {
            get
            {
                return v3session.ServerVersion;
            }
        }

        /// <summary>
        ///   <para>Gets information about the version of the HPC Pack that is
        /// installed on the node of the cluster that runs the SOA client application.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Version" /> that contains the version information.</para>
        /// </value>
        /// <remarks>
        ///   <para>The
        /// <see cref="System.Version.Build" /> and
        /// <see cref="System.Version.Revision" /> portions of the version that the
        /// <see cref="System.Version" /> object represents are not defined for the HPC Pack.</para>
        ///   <para>HPC Pack 2008 is version 2.0. HPC Pack 2008 R2 is version 3.0.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.ServerVersion" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.ClientVersion" />
        /// <seealso cref="System.Version" />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Backward compatibility")]
        public Version ClientVersion
        {
            get
            {
                return SessionBase.ClientVersionInternal;
            }
        }

        /// <summary>
        ///   <para>Specifies whether the client is a console or Windows application.</para>
        /// </summary>
        /// <param name="console">
        ///   <para>Set to True if the client is a console application; otherwise, it is False.</para>
        /// </param>
        /// <param name="wnd">
        ///   <para>The handle to the parent window if the client is a Windows application.</para>
        /// </param>
        /// <remarks>
        ///   <para>This information is used to determine how to prompt the user for credentials if
        /// they are not specified in the job. If you do not call this method, console is assumed.</para>
        /// </remarks>
        /// <example />
        public static void SetInterfaceMode(bool console, IntPtr wnd)
        {
            V3Session.SetInterfaceMode(console, wnd);
        }

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
            if (startInfo.IsNoSession )
            {
                if (startInfo.UseInprocessBroker)
                {
                    return CreateIPSession(startInfo);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                return CreateSession(startInfo, null);
            }
        }

        /// <summary>
        /// Synchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="info">The session start info for creating the service session</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static Session CreateSession(SessionStartInfo startInfo, Binding binding)
        {
            return CreateSessionAsync(startInfo, binding).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="info">The session start info for creating the service session</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static async Task<Session> CreateSessionAsync(SessionStartInfo startInfo)
        {
            return new Session(await V3Session.CreateSessionAsync(startInfo, null).ConfigureAwait(false));
        }

        /// <summary>
        /// Asynchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="info">The session start info for creating the service session</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static async Task<Session> CreateSessionAsync(SessionStartInfo startInfo, Binding binding)
        {
            Utility.ThrowIfNull(startInfo, "startInfo");

            return new Session(await V3Session.CreateSessionAsync(startInfo, binding).ConfigureAwait(false));
        }

        /// <summary>
        ///   <para>Creates a session by using the specified timeout value (the session must be created within the specified period or the call fails).</para>
        /// </summary>
        /// <param name="startInfo">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> class that contains information for starting the session.</para>
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>The amount of time, in milliseconds, in which the session must be created. If
        /// the time to create the session exceeds the timeout value, the call fails. The default is
        /// <see cref="System.Threading.Timeout.Infinite" />.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> object that defines the session.</para>
        /// </returns>
        [Obsolete("This CreateSession method overload is obsolete. The timeoutMilliseconds argument is no longer supported. Please use a version that does not take a timeoutMilliseconds argument.")] // Obsolete added for V4 RTM.
        public static Session CreateSession(SessionStartInfo startInfo, int timeoutMilliseconds)
        {
            return CreateSession(startInfo);
        }

        /// <summary>
        /// This method closes the session with the given ID
        /// </summary>
        /// <param name="headnode">Headnode name</param>
        /// <param name="sessionId">The ID of the session to be closed</param>
        public static void CloseSession(string headnode, int sessionId, bool isAadUser = false)
        {
            Utility.ThrowIfNull(headnode, "headnode");

            V3Session.CloseSession(headnode, sessionId, isAadUser);
        }

        /// <summary>
        /// This method closes the session with the given ID
        /// </summary>
        /// <param name="headnode">Headnode name</param>
        /// <param name="sessionId">The ID of the session to be closed</param>
        /// <param name="binding">indicating the binding</param>
        public static void CloseSession(string headnode, int sessionId, Binding binding, bool isAadUser = false)
        {
            Utility.ThrowIfNull(headnode, "headnode");

            V3Session.CloseSession(headnode, sessionId, binding, isAadUser);
        }

        /// <summary>
        ///   <para>Creates a session asynchronously.</para>
        /// </summary>
        /// <param name="startInfo">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> class that contains information for starting the session.</para>
        /// </param>
        /// <param name="callback">
        ///   <para>An
        /// <see cref="System.AsyncCallback" /> object that identifies a method to be called when the asynchronous operation completes. Can be
        /// null.</para>
        /// </param>
        /// <param name="state">
        ///   <para>User-defined data to pass to the callback. To get the user-defined data in the callback, access the
        /// <see cref="System.IAsyncResult.AsyncState" /> property that is passed to your callback. Can be
        /// null.</para>
        /// </param>
        /// <returns>
        ///   <para>An
        /// <see cref="System.IAsyncResult" /> interface that represents the status of an asynchronous operation. Use the interface when calling the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.EndCreateSession(System.IAsyncResult)" /> method.</para>
        /// </returns>
        public static IAsyncResult BeginCreateSession(
                                                      SessionStartInfo startInfo,
                                                      AsyncCallback callback,
                                                      object state)
        {
            Utility.ThrowIfNull(startInfo, "startInfo");

            return V3Session.BeginCreateSession(startInfo, callback, state);
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
                                                      Binding binding,
                                                      AsyncCallback callback,
                                                      object state)
        {
            Utility.ThrowIfNull(startInfo, "startInfo");

            return V3Session.BeginCreateSession(startInfo, binding, callback, state);
        }

        /// <summary>
        ///   <para>Cancels the attempt to create a session asynchronously.</para>
        /// </summary>
        /// <param name="result">
        ///   <para>An
        /// <see cref="System.IAsyncResult" /> interface that represents the status of an asynchronous operation. Specify the interface that the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.BeginCreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.AsyncCallback,System.Object)" /> method returns.</para>
        /// </param>
        public static void CancelCreateSession(IAsyncResult result)
        {
            Utility.ThrowIfNull(result, "result");

            V3Session.CancelCreateSession(result);
        }

        /// <summary>
        ///   <para>Blocks until the asynchronous process for creating the session completes.</para>
        /// </summary>
        /// <param name="result">
        ///   <para>An <see cref="System.IAsyncResult" /> interface that represents the status of an asynchronous operation. </para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> object that defines the session.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Typically, you call this method from the callback that you specify when calling the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.BeginCreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.AsyncCallback,System.Object)" /> method. If you use a callback, pass the result object that is passed to your callback. If you do not use a callback, pass the result object that the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.BeginCreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.AsyncCallback,System.Object)" /> method returns.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.BeginCreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.AsyncCallback,System.Object)"
        /// />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.CancelCreateSession(System.IAsyncResult)" />
        public static Session EndCreateSession(IAsyncResult result)
        {
            return new Session(V3Session.EndCreateSession(result));
        }

        /// <summary>
        ///   <para>Attaches an SOA client to an existing session by using the specified information about the session.</para>
        /// </summary>
        /// <param name="attachInfo">
        ///   <para>A
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionAttachInfo" /> object that specifies information about the session to which you want to attach the SOA client, including the name of the head node for the cluster that hosts the session and the identifier of the session.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> that represents the session to which the client attached.</para>
        /// </returns>
        public static Session AttachSession(SessionAttachInfo attachInfo)
        {
            Utility.ThrowIfNull(attachInfo, "attachInfo");

            return new Session(V3Session.AttachSession(attachInfo));
        }

        /// <summary>
        /// Attach to a existing session with the session id
        /// </summary>
        /// <param name="attachInfo">The attach info</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A persistant session</returns>
        public static Session AttachSession(SessionAttachInfo attachInfo, Binding binding)
        {
            Utility.ThrowIfNull(attachInfo, "attachInfo");

            return new Session(V3Session.AttachSession(attachInfo, binding));
        }

        /// <summary>
        ///   <para>Gets the value of a backend-specific property for a session.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>String that specifies the name of the property for which you want to get the value. The property names that you
        /// can specify are HPC_ServiceJobId, HPC_Headnode, and HPC_ServiceJobStatus, which is a string
        /// that indicates the current status of the service job for the session.</para>
        /// </param>
        /// <typeparam name="T">
        ///   <para>The data type of the property for which you want to get the value. For information about the
        /// properties for which you can get a value and their data types, see the description for the <paramref name="name" /> parameter.</para>
        /// </typeparam>
        /// <returns>
        ///   <para>An item with the data type that the T type parameter specifies and which contains the value of the property
        /// that the <paramref name="name" /> parameter specifies. The following table describes the
        /// return values and their types for the properties for which you can get values.</para>
        ///   <para>Property name</para>
        ///   <para>Type</para>
        ///   <para>Description</para>
        /// </returns>
        /// <remarks>
        ///   <para>This method does not support getting the values of custom properties.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobState" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Headnode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Id" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionAttachInfoBase.Headnode" />
        public T GetProperty<T>(string name)
        {
            return v3session.GetProperty<T>(name);
        }

        /// <summary>
        ///   <para>Gets the version of the service used to start this <see cref="Microsoft.Hpc.Scheduler.Session.Session" />.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns a
        /// <see cref="System.Version" /> object that represents the version of the service used to start the session represented by this
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> object.</para>
        /// </value>
        public Version ServiceVersion
        {
            get
            {
                return v3session.ServiceVersion;
            }
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


        // TODO: rename
        public static Session CreateBrkSession(SessionStartInfo startInfo)
        {
            return CreateBrkSessionAsync(startInfo).GetAwaiter().GetResult();
        }

        // TODO: rename
        public static async Task<Session> CreateBrkSessionAsync(SessionStartInfo startInfo)
        {
            IBrokerFactory brokerFactory = new V3BrokerFactory(false);
            DateTime targetTimeout = DateTime.Now.AddMilliseconds(Constant.DefaultCreateSessionTimeout);
            //in HPC sessionId cannot be negative (out of range)   
            return new Session((V3Session)await brokerFactory.CreateBroker(startInfo, startInfo.DummySessionId, targetTimeout, startInfo.BrokerLauncherEprs, null).ConfigureAwait(false));
        }


        /// <summary>
        /// create in process session
        /// </summary>
        /// <param name="startInfo"></param>
        /// <returns></returns>
        // TODO: rename
        public static Session CreateIPSession(SessionStartInfo startInfo)
        {
            return CreateIPSessionAsync(startInfo).GetAwaiter().GetResult();
        }

        // TODO: rename
        public static async Task<Session> CreateIPSessionAsync(SessionStartInfo startInfo)
        {
            InprocessBrokerFactory brokerFactory = new InprocessBrokerFactory(startInfo.Headnode, false);
            DateTime targetTimeout = DateTime.Now.AddMilliseconds(Constant.DefaultCreateSessionTimeout);
            return new Session((V3Session) await brokerFactory.CreateBroker(startInfo, startInfo.DummySessionId, targetTimeout, null, null).ConfigureAwait(false));
        }
    }
}
