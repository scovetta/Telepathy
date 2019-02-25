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
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.HpcPack;

    // TODO: (Design) change this class to proper factory class

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
    public class HpcSession : Session
    {
        /// <summary>
        /// create the session shim based on a v3 session
        /// </summary>
        /// <param name="v2session"></param>
        internal HpcSession(V3Session v3session) : base(v3session)
        {
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
                return this.v3session.AutoClose;
            }
            set
            {
                this.v3session.AutoClose = value;
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
                return this.v3session.EndpointReference;
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
                return this.v3session.NetTcpEndpointReference;
            }
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
                return this.v3session.HttpEndpointReference;
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
            HpcSessionCredUtil.SetInterfaceMode(console, wnd);
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
        public static new Session CreateSession(SessionStartInfo startInfo)
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
                return CreateSession(startInfo, null);
            }
        }

        /// <summary>
        /// Synchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="info">The session start info for creating the service session</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static HpcSession CreateSession(SessionStartInfo startInfo, Binding binding)
        {
            return CreateSessionAsync(startInfo, binding).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="info">The session start info for creating the service session</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static async Task<HpcSession> CreateSessionAsync(SessionStartInfo startInfo)
        {
            return new HpcSession(await HpcV3Session.CreateSessionAsync(startInfo, null).ConfigureAwait(false));
        }

        /// <summary>
        /// Asynchronous mode of submitting a job and get a ServiceJobSession object.
        /// </summary>
        /// <param name="info">The session start info for creating the service session</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A service job session object, including the endpoint address and the two jobs related to this session</returns>
        public static async Task<HpcSession> CreateSessionAsync(SessionStartInfo startInfo, Binding binding)
        {
            Utility.ThrowIfNull(startInfo, "startInfo");

            return new HpcSession(await HpcV3Session.CreateSessionAsync(startInfo, binding).ConfigureAwait(false));
        }

        /// <summary>
        /// This method closes the session with the given ID
        /// </summary>
        /// <param name="headnode">Headnode name</param>
        /// <param name="sessionId">The ID of the session to be closed</param>
        public static void CloseSession(string headnode, int sessionId, bool isAadUser = false)
        {
            Utility.ThrowIfNull(headnode, "headnode");

            HpcV3Session.CloseSession(headnode, sessionId, isAadUser);
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

            HpcV3Session.CloseSession(headnode, sessionId, binding, isAadUser);
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
        public static HpcSession AttachSession(SessionAttachInfo attachInfo)
        {
            Utility.ThrowIfNull(attachInfo, "attachInfo");

            return new HpcSession(HpcV3Session.AttachSession(attachInfo));
        }

        /// <summary>
        /// Attach to a existing session with the session id
        /// </summary>
        /// <param name="attachInfo">The attach info</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>A persistant session</returns>
        public static HpcSession AttachSession(SessionAttachInfo attachInfo, Binding binding)
        {
            Utility.ThrowIfNull(attachInfo, "attachInfo");

            return new HpcSession(HpcV3Session.AttachSession(attachInfo, binding));
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
                return this.v3session.ServiceVersion;
            }
        }

       
    }
}
