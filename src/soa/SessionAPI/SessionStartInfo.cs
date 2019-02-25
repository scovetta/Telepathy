//------------------------------------------------------------------------------
// <copyright file="SessionStartInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       HPC class for information to create a session
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;

    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para>Defines a set of values used to create a session.</para>
    /// </summary>
    /// <remarks>
    ///   <para>In Windows HPC Server 2008, this class includes the HasRuntime property. This property was removed in Windows HPC Server 2008 R2.</para>
    /// </remarks>
    /// <example>
    ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853427(v=vs.85).aspx">Creating a SOA Client</see>.</para>
    /// </example>
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.BeginCreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.AsyncCallback,System.Object)" 
    /// /> 
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.CreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo)" />
    [Serializable]
    public class SessionStartInfo : SessionInitInfoBase, ISerializable
    {
        /// <summary>
        ///   <para>Defines settings that control the behavior of the broker for this session.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For options to provide a setting 
        /// for the broker, see <see href="https://msdn.microsoft.com/library/cc972968(v=vs.85).aspx">Specifying Broker Configuration Settings</see>.</para>
        ///   <para>In Windows HPC Server 2008, this class includes the BrokerSettingsInfo constructor. This 
        /// constructor has been removed in Windows HPC Server 2008 R2 and is not supported in previous versions.</para>
        ///   <para>From Windows HPC Server 2008 R2, the AllocationGrowLoadRatioThreshold and 
        /// AllocationShrinkLoadRatioThreshold properties are obsolete and no longer take effect.</para>
        /// </remarks>
        /// <example />
        [Serializable]
        public class BrokerSettingsInfo
        {
            private SessionStartInfoContract data;

            internal BrokerSettingsInfo(SessionStartInfoContract data)
            {
                this.data = data;
            }

            /// <summary>
            ///   <para>The amount of time that the client can go without sending requests to the service, in Windows HPC Server 2008.</para>
            ///   <para>The amount of time that a client application can go 
            /// without activity or pending requests before the broker closes the connection, in Windows HPC Server 2008 R2.</para>
            /// </summary>
            /// <value>
            ///   <para>The amount of time, in milliseconds, that the client can go without sending requests to the service, in Windows HPC Server 2008. </para>
            ///   <para>The amount of time, in milliseconds that a client application can 
            /// go without activity or pending requests before the broker closes the connection, From Windows HPC Server 2008 R2. </para>
            ///   <para>The default is 300,000 milliseconds.</para>
            /// </value>
            /// <remarks>
            ///   <para>If the idle timeout period is exceeded, the session closes.</para>
            ///   <para>From Windows HPC Server 2008 R2, the 
            /// 
            /// "Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T} method is a one-way call. If the client application times out, the client application does not see the timeout exception until the client application calls the  
            /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method.</para>
            ///   <para>You must cast the value to an integer. If the value is null (a null value means that the 
            /// value has not been set and the broker is using the default value set in the configuration file), the cast raises an exception.</para>
            /// </remarks>
            /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.SessionIdleTimeout" />
            public int? ClientIdleTimeout
            {
                get { return data.ClientIdleTimeout; }
                set { data.ClientIdleTimeout = value; }
            }

            /// <summary>
            ///   <para>The amount of time that the broker waits for a 
            /// client to bind to the service after all previous client sessions ended, in Windows HPC Server 2008.</para>
            ///   <para>The amount of time that the broker waits for client applications to connect to a session 
            /// after all previously connected client applications time out, in 
            /// Windows HPC Server 2008 R2. When this period elapses, the broker closes the session.</para> 
            /// </summary>
            /// <value>
            ///   <para>The amount of time, in milliseconds, that the broker waits for a client to connect, in Windows HPC Server 2008. </para>
            ///   <para>The amount of time, in milliseconds, that the broker waits for client 
            /// applications to connect to a session after all previously connected client applications time out, from Windows HPC Server 2008 R2.</para>
            ///   <para>The default is zero.</para>
            /// </value>
            /// <remarks>
            ///   <para>If the timeout period is exceeded, the broker closes.</para>
            ///   <para>For Windows HPC Server 2008 R2, if the session uses an HTTP binding, the period for the 
            /// 
            /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.SessionIdleTimeout" /> setting does not start until after the for the  
            /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.ClientIdleTimeout" /> setting elapses.</para>
            /// </remarks>
            /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.ClientIdleTimeout" />
            public int? SessionIdleTimeout
            {
                get
                {
                    return data.SessionIdleTimeout;
                }

                set
                {
                    data.ClientConnectionTimeout = value;
                    data.SessionIdleTimeout = value;
                }
            }

            /// <summary>
            /// Specify the dispatcher capacity used in grow and shrink. By default it is zero which means the auto calculated number of allocated cores.
            /// </summary>
            public int? DispatcherCapacityInGrowShrink
            {
                get
                {
                    return data.DispatcherCapacityInGrowShrink;
                }

                set
                {
                    data.DispatcherCapacityInGrowShrink = value;
                }
            }

            /// <summary>
            ///   <para>The upper threshold at which point the broker stops receiving messages from the clients.</para>
            /// </summary>
            /// <value>
            ///   <para>The upper threshold of queued messages. The default is 5,120 messages.</para>
            /// </value>
            /// <remarks>
            ///   <para>You must cast the value to an integer. If the value is null (a null value means that the 
            /// value has not been set and the broker is using the default value set in the configuration file), the cast raises an exception.</para>
            /// </remarks>
            /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.MessagesThrottleStopThreshold" />
            public int? MessagesThrottleStartThreshold
            {
                get { return data.MessagesThrottleStartThreshold; }
                set { data.MessagesThrottleStartThreshold = value; }
            }

            /// <summary>
            ///   <para>The lower threshold at which point the broker begins receiving messages from the clients.</para>
            /// </summary>
            /// <value>
            ///   <para>The lower threshold of queued messages. The default is 3,840 messages.</para>
            /// </value>
            /// <remarks>
            ///   <para>You must cast the value to an integer. If the value is null (a value of null means that the 
            /// value has not been set and the broker is using the default value set in the configuration file), the cast raises an exception.</para>
            /// </remarks>
            /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.MessagesThrottleStartThreshold" />
            public int? MessagesThrottleStopThreshold
            {
                get { return data.MessagesThrottleStopThreshold; }
                set { data.MessagesThrottleStopThreshold = value; }
            }

            /// <summary>
            ///   <para>Gets or sets the length of time in milliseconds that must elapse 
            /// between the client and broker heartbeats before the broker is considered unreachable by the session.</para>
            /// </summary>
            /// <value>
            ///   <para>A 
            /// <see cref="System.Nullable{T}" /> object with a type parameter of 
            /// int that indicates the length of time in milliseconds that must elapse between 
            /// the client and broker heartbeats before the broker is considered unreachable by the session.</para> 
            /// </value>
            /// <remarks>
            ///   <para>The default value for this property is 20,000 milliseconds. Application operation times can be long, 
            /// so this heartbeat interval is used to determine in a more timely fashion when the broker becomes unreachable.</para>
            /// </remarks>
            /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.ClientBrokerHeartbeatRetryCount" />
            public int? ClientBrokerHeartbeatInterval
            {
                get { return data.ClientBrokerHeartbeatInterval; }
                set { data.ClientBrokerHeartbeatInterval = value; }
            }

            /// <summary>
            ///   <para>Gets or sets the number of times that the amount of time that the 
            /// 
            /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.ClientBrokerHeartbeatInterval" /> property specifies must elapse between the client and broker heartbeats before the broker is considered unreachable by the session.</para> 
            /// </summary>
            /// <value>
            ///   <para>A 
            /// <see cref="System.Nullable{T}" /> object with a type parameter of 
            /// int that indicates the number of times that the amount of time that the 
            /// 
            /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.ClientBrokerHeartbeatInterval" /> property specifies must elapse between the client and broker heartbeats before the broker is considered unreachable by the session.</para> 
            /// </value>
            /// <remarks>
            ///   <para>The default value for this property is 3. When a broker is considered unreachable, all additional calls to methods of the 
            /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class generate a 
            /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionException" />, and the response handlers for the 
            /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object receive a 
            /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object that contains the same 
            /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionException" />.</para>
            /// </remarks>
            /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.ClientBrokerHeartbeatInterval" />
            public int? ClientBrokerHeartbeatRetryCount
            {
                get { return data.ClientBrokerHeartbeatRetryCount; }
                set { data.ClientBrokerHeartbeatRetryCount = value; }
            }

            /// <summary>
            ///   <para>Gets or sets the maximum size of a request or response message for the session.</para>
            /// </summary>
            /// <value>
            ///   <para>A 
            /// <see cref="System.Nullable{T}" /> object with a type parameter of 
            /// int that indicates the maximum size of a request or response message for the session in kilobytes (kb).</para>
            /// </value>
            /// <remarks>
            ///   <para>The default value of this property is 64.</para>
            ///   <para>The service-oriented architecture (SOA) runtime synchronized all of the related 
            /// settings for all of the Windows Communication Foundation (WCF) bindings to this size.</para>
            /// </remarks>
            public int? MaxMessageSize
            {
                get { return data.MaxMessageSize; }
                set { data.MaxMessageSize = value; }
            }

            /// <summary>
            ///   <para>Gets or sets the amount of time that the service should try to perform operations before timing out.</para>
            /// </summary>
            /// <value>
            ///   <para>A 
            /// <see cref="System.Nullable{T}" /> object with a type parameter of 
            /// int that indicates the amount of time in milliseconds that the service should try to perform operations before timing out..</para>
            /// </value>
            /// <remarks>
            ///   <para>The default value is 86,400,000 milliseconds, which is one day. </para>
            ///   <para>Applications can adjust this timeout for the maximum operation time of the service.</para>
            ///   <para>The service-oriented architecture (SOA) runtime synchronized all of the related settings for all 
            /// of the Windows Communication Foundation (WCF) bindings to this time-out value. The time-outs for the  
            /// "Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T} and 
            /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses" /> methods all synchronize to this value.</para>
            /// </remarks>
            /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T}(``0)" />
            /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses" />
            public int? ServiceOperationTimeout
            {
                get { return data.ServiceOperationTimeout; }
                set { data.ServiceOperationTimeout = value; }
            }
        }

        private SessionStartInfoContract data = new SessionStartInfoContract();
        //private string _headnode;
        //private string _headnodeMachine = null;
        private ICollection<string> _nodeGroups = new List<string>();
        private ICollection<string> _requestedNodes = new List<string>();
        private List<string> _requestedNodesList = new List<string>();
        private List<string> _nodeGroupsList = new List<string>();
        private IDictionary<string, string> _dependFiles = null;

        /// <summary>
        /// Stores a value indicating whether the session is inprocess
        /// </summary>
        private bool useInprocessBroker;

        /// <summary>
        /// Broker settings
        /// </summary> 
        private BrokerSettingsInfo _brokerSettings;

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> class for the specified service on the cluster with the specified head node.</para> 
        /// </summary>
        /// <param name="headnode">
        ///   <para>The name of the head node of the cluster to which you want to connect.</para>
        /// </param>
        /// <param name="serviceName">
        ///   <para>The name of the service to run on the nodes in the cluster. </para>
        /// </param>
        /// <remarks>
        ///   <para>The name that you specify for <paramref name="serviceName" /> must 
        /// be the same name that you specified for the service's configuration file (&lt;ServiceName&gt;.config).</para>
        /// </remarks>
        /// <example />
        public SessionStartInfo(string headnode, string serviceName)
            : this(headnode, serviceName, null)
        {
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> class for the specified service and service version on the cluster with the specified head node.</para> 
        /// </summary>
        /// <param name="headnode">
        ///   <para>String that specifies name of the head node of the HPC cluster to which you want to connect.</para>
        /// </param>
        /// <param name="serviceName">
        ///   <para>String that specifies the name of the service to run on the nodes in the cluster.</para>
        /// </param>
        /// <param name="serviceVersion">
        ///   <para>A <see cref="System.Version" /> that specifies the version of the service to which the session should connect.</para>
        /// </param>
        /// <remarks>
        ///   <para>The name and version of a service are specified by the file name of the configuration file for the service, which 
        /// has a format of service_name_major.minor.config. For example, MyService_1.0.config. The version must include 
        /// the major and minor portions of the version identifier and no further subversions.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.#ctor(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.#ctor(System.String,System.String)" />
        public SessionStartInfo(string headnode, string serviceName, Version serviceVersion)
            : base(headnode)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(SR.ServiceNameCantBeNull);
            }

            if (!IsServiceNameValid(serviceName))
            {
                throw new ArgumentException(SR.ServiceNameCantBeNull, "serviceName");
            }
            this.Init(headnode, serviceName, serviceVersion);
        }

        /// <summary>
        /// Initializes session start information
        /// </summary>
        /// <param name="headnode">indicating the head node</param>
        /// <param name="serviceName">indicating the service name</param>
        /// <param name="serviceVersion">indicating the service version</param>
        private void Init(string headnode, string serviceName, Version serviceVersion)
        {
            _brokerSettings = new BrokerSettingsInfo(data);
            //this._headnode = headnode;
            data.EprList = Utility.TryGetEprList();
            data.ServiceName = serviceName;
            data.RegPath = this.RegPath;
            data.IpAddress = this.IpAddress;
            if (serviceVersion != SessionBase.NoServiceVersion)
                data.ServiceVersion = serviceVersion;

            // check diagnostics
            string brokernode = System.Environment.GetEnvironmentVariable(Constant.DiagnosticBrokerNode);
            if (!String.IsNullOrEmpty(brokernode))
            {
                data.DiagnosticBrokerNode = brokernode;
            }

            // set client api version
            data.ClientVersion = SessionBase.FullClientVersionInternal;

            // if the head node is on Azure IaaS, set the default TransportScheme to Http
            if (SoaHelper.IsSchedulerOnIaaS(headnode))
            {
                data.TransportScheme = TransportScheme.Http;
            }

            data.LocalUser = base.LocalUser;
        }

        /// <summary>
        /// Check is service name is valid
        /// </summary>
        /// <param name="serviceName">indicating the service name</param>
        /// <returns>returns a value indicating whether service name is valid</returns>
        private static bool IsServiceNameValid(string serviceName)
        {
            if (serviceName.Length > 255)
                return false;

            if (Regex.Match(serviceName, @"[\w\-\.]+").Length != serviceName.Length)
                return false;

            return true;
        }

        /// <summary>
        /// Check if head node is an IaaS node
        /// </summary>
        /// <param name="headnodeName">
        /// Name of head node
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public static bool IsHeadNodeOnAzure(string headnodeName)
        {
            return SoaHelper.IsSchedulerOnIaaS(headnodeName);
        }

        /// <summary>
        /// Gets the epr list
        /// </summary>
        internal string[] EprList
        {
            get { return this.data.EprList; }
        }

        /// <summary>
        ///   <para>Retrieves or sets the RunAs user for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The RunAs user for the job, in the form domain\username.</para>
        /// </value>
        /// <remarks>
        ///   <para>The user name is limited to 80 characters. If this parameter is 
        /// NULL, empty, or not valid, HPC searches the credentials cache for the credentials to  
        /// use. If the cache contains the credentials for a single user, those credentials are 
        /// used. However, if multiple credentials exist in the cache, the user is prompted for the credentials.</para> 
        ///   <para>If the user under whose credentials the job runs differs from the job owner, the user under whose credentials the 
        /// job runs must be an administrator. If that user is not an administrator, an exception occurs because that user does not have  
        /// permission to read the job. The job owner is the user who runs the SOA client application. If you set the user 
        /// under whose credentials the job runs to be the same as the job owner, that user does not need to be an administrator.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Password" />
        public override string Username
        {
            get { return data.Username; }
            set { data.Username = value; }
        }

        /// <summary>
        ///   <para>Retrieves or sets the password for the RunAs user.</para>
        /// </summary>
        /// <value>
        ///   <para>The password for the RunAs user.</para>
        /// </value>
        /// <remarks>
        ///   <para>The password is limited to 127 characters. If this parameter is null or 
        /// empty, this method uses the cached password if cached; otherwise, the user is prompted for the password.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Username" />
        public string Password
        {
            set
            {
                data.Password = value;
            }
        }

        /// <summary>
        /// Only for internal use.
        /// </summary>
        public override string InternalPassword
        {
            get { return data.Password; }
            set { data.Password = value; }
        }

        /// <summary>
        /// Get or set a value indicates if to use local user as current principle
        /// </summary>
        public override bool LocalUser
        {
            get => base.LocalUser;
            set
            {
                base.LocalUser = value;
                this.data.LocalUser = this.LocalUser;
            }
        }

        /// <summary>
        /// If we need save password after we get encrypt password
        /// </summary>
        public bool SavePassword
        {
            get
            {
                return data.SavePassword.HasValue ? data.SavePassword.Value : false;
            }
            set
            {
                data.SavePassword = value;
            }
        }

        public byte[] Certificate
        {
            set { data.Certificate = value; }
        }

        public string PfxPassword
        {
            set { data.PfxPassword = value; }
        }

        /// <summary>
        ///   <para>Retrieves the name of the service to run on the nodes of the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the service that runs on the nodes of the cluster. Specify the name of the 
        /// registration file for the service. For example, if the name 
        /// of the registration file is EchoService.config, specify EchoService as the service name.</para> 
        /// </value>
        /// <remarks>
        ///   <para>The name is specified when you construct this object.</para>
        /// </remarks>
        /// <example />
        public string ServiceName
        {
            get { return data.ServiceName; }
        }

        /// <summary>
        ///   <para>Retrieves or sets the transport binding schemes used for the session.</para>
        /// </summary>
        /// <value>
        ///   <para>The transport binding schemes. You can specify one or more schemes. For possible values, see 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme" />. </para>
        /// </value>
        /// <remarks>
        ///   <para>If you specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.Http" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Secure" /> is 
        /// True, HTTPS is used; otherwise, if you specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.Http" /> and  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Secure" /> is 
        /// False, HTTP is used.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Secure" />
        public override TransportScheme TransportScheme
        {
            get { return data.TransportScheme; }
            set { data.TransportScheme = value; }
        }

        /// <summary>
        ///   <para>Gets or sets a <see cref="System.Boolean" /> value that indicates whether the session should use the in-process broker.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns a <see cref="System.Boolean" /> value that indicates whether the session should use the in-process broker..</para>
        /// </value>
        public bool UseInprocessBroker
        {
            get
            {
                if (this.DebugModeEnabled)
                {
                    // Always use inprocess broker if debug mode enabled
                    return true;
                }

                return this.useInprocessBroker;
            }

            set
            {
                this.useInprocessBroker = value;
            }
        }

        /// <summary>
        /// store a value indicating whether has session id from HPC
        /// </summary>
        public bool IsNoSession { get; set; }

        /// <summary>
        /// Storage connection string used to connect to broker launcher storage queue endpoint
        /// </summary>
        public string BrokerLauncherStorageConnectionString { get; set; }
        
        public static int StandaloneSessionId => TelepathyConstants.StandaloneSessionId;

        /// <summary>
        ///   <para>Determines if a secure connection is used between the client and the HPC broker.</para>
        /// </summary>
        /// <value>
        ///   <para>
        ///     True indicates that a secure front-end binding is used that includes both encryption and authentication. To 
        /// turn off encryption to improve performance, change the settings for this 
        /// binding in the service configuration file and in your client computer. </para> 
        ///   <para>
        ///     False indicates that a front-end binding with no security is used, so neither encryption nor 
        /// authentication is used. Setting this value to False is not recommended for production use, because False provides no security.</para>
        ///   <para>The default is True.</para>
        /// </value>
        /// <remarks>
        ///   <para>Uses SSL if 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> is 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.Http" />, or uses Kerberos if 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> is 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.NetTcp" />.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.ShareSession" />
        public bool Secure
        {
            get { return data.Secure; }
            set { data.Secure = value; }
        }

        /// <summary>
        ///   <para>Determines if more than one user can connect to the session.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if more than one user can connect 
        /// to the session; otherwise, it is False. The default is False.</para>
        /// </value>
        /// <remarks>
        ///   <para>If False, only the person who created the session can send requests to the broker.</para>
        ///   <para>If True, anyone who can submit jobs based on the job template can send requests to the broker.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Secure" />
        public bool ShareSession
        {
            get { return data.ShareSession; }
            set { data.ShareSession = value; }
        }

        /// <summary>
        ///   <para>Gets or sets a value that indicates whether the session can be preempted.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns 
        /// <see cref="System.Boolean" /> value that indicates whether the session can be preempted. 
        /// True if the session can be preempted. Otherwise, 
        /// False..</para>
        /// </value>
        public bool CanPreempt
        {
            get { return data.CanPreempt.Value; }
            set { data.CanPreempt = value; }
        }

        /// <summary>
        ///   <para>The broker settings that define the timeout periods that are used by the broker.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo" /> object that defines the timeout periods that are used by the broker.</para> 
        /// </value>
        public BrokerSettingsInfo BrokerSettings
        {
            get { return _brokerSettings; }
        }

        // The following four job properties are put a shortcut in the start info. They will be 
        // overriden if the same job property is added using AddJobProperty()

        /// <summary>
        ///   <para>Retrieves or sets the template to use for the service job.</para>
        /// </summary>
        /// <value>
        ///   <para>The template to use to set the default values and constraints for the service job.</para>
        /// </value>
        /// <remarks>
        ///   <para>If it is not set, the job uses the Default template.</para>
        ///   <para>Creating the session fails if the values that you specify for 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.MaximumUnits" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.MinimumUnits" />, and 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.SessionResourceUnitType" /> conflict with those that are specified in the template.</para> 
        /// </remarks>
        public string JobTemplate
        {
            get { return data.JobTemplate; }
            set { data.JobTemplate = value; }
        }

        /// <summary>
        ///   <para>Gets or sets whether cores, nodes, or sockets are used to allocate resources for the service job.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Nullable{T}" /> object with a type parameter of 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType" /> that indicates whether the HPC Job Scheduler Service should allocate resources for the service job in the form of cores, nodes, or sockets.</para> 
        /// </value>
        /// <remarks>
        ///   <para>The default value is Core.</para>
        ///   <para>The resource units that you specify should be based on the threading model that the service uses. Specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Core" /> if the service is linked to non-thread safe libraries. Specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Node" /> if the service is multithreaded. Specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Socket" /> if the service is single-threaded and memory-bus intensive.</para>
        ///   <para>By default, if you specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Core" />, the broker sends the service one message at a time. If you specify 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Node" />, the broker batches together the number of messages that is equal to the number of cores on the node, and then sends them to the service. If you specify  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Socket" />, the broker batches together the number of messages that is equal to the number of cores on the socket, and then sends them to the service.</para> 
        ///   <para>To override the default behavior in Windows HPC Server 2008, configure the ServiceThrottlingBehavior section of your service.dll.config file to specify the maximum concurrent 
        /// calls that the service can accept. For example, if you are using the Parallel Extension, you can specify the following service behavior  
        /// in the service.dll.config file to override the default behavior for the node resource unit type so that the service receives only one 
        /// request at a time. The following example shows how to set the maximum number of concurrent calls that the service can accept in Windows HPC Server 2008.</para> 
        ///   <code>&lt;serviceBehaviors&gt;
        ///     &lt;behavior  name="Throttled"&gt;
        ///         &lt;serviceThrottling maxConcurrentCalls="1" /&gt;
        ///     &lt;/behavior&gt;
        /// &lt;/serviceBehaviors&gt;
        /// </code>
        ///   <para>For Windows HPC Server 2008, the broker uses the value of maxConcurrentCalls 
        /// as the capacity of the service. This lets the administrator or software  
        /// developer use a standard WCF setting to fine tune the dispatching 
        /// algorithm of the broker node to fit the processing capacity of the service.</para> 
        ///   <para>For Windows HPC Server 2008 R2, you configure the maxConcurrentCalls setting for the Service element in the microsoft.Hpc.Session.ServiceRegistration section of the 
        /// servicename.config file, where servicename is the same as the value you used for the serviceName parameter when you called the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.#ctor(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" /> constructor. The value of the maxConcurrentCalls attribute specifies the maximum number of messages that a service host can actively process. A value of 0 indicates that the maximum value should be calculated automatically based on the service capacity of each service host. The service capacity of a service host is the number of cores for that host. The following example shows how to specify the maxConcurrentCalls setting in Windows HPC Server 2008 R2.</para> 
        ///   <code>  &lt;microsoft.Hpc.Session.ServiceRegistration&gt;
        ///     &lt;service assembly="%CCP_HOME%bin\EchoSvcLib.dll"
        ///  contract="EchoSvcLib.IEchoSvc"
        ///  type="EchoSvcLib.EchoSvc"
        ///  includeExceptionDetailInFaults="true"
        ///  maxConcurrentCalls="1"
        ///  maxMessageSize="65536"
        ///  serviceInitializationTimeout="60000" &gt;
        ///       &lt;!--The following lines add example environment variables to the service.   --&gt;
        ///       &lt;environmentVariables&gt;
        ///         &lt;add name="variable1" value="value1"/&gt;
        ///         &lt;add name="variable2" value="value2"/&gt;
        ///       &lt;/environmentVariables&gt;
        ///     &lt;/service&gt;
        /// </code>
        ///   <para>If you set the unit type, it must be the same as in the template, if one is specified.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.MinimumUnits" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.MaximumUnits" />
        public SessionUnitType? SessionResourceUnitType
        {
            get
            {
                if (data.ResourceUnitType.HasValue)
                {
                    return (SessionUnitType)data.ResourceUnitType.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    data.ResourceUnitType = (int)value.Value;
                }
                else
                {
                    data.ResourceUnitType = null;
                }
            }
        }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of resource units that the scheduler can allocate for the service job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of resource units.</para>
        /// </value>
        /// <remarks>
        ///   <para>The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.SessionResourceUnitType" /> property defines the resource units (for example, nodes or cores).</para> 
        ///   <para>The maximum units must be within the constraints of the template, if there are any.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.MinimumUnits" />
        public int? MaximumUnits
        {
            get { return data.MaxUnits; }
            set { data.MaxUnits = value; }
        }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of resource units that the service job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of resource units.</para>
        /// </value>
        /// <remarks>
        ///   <para>The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.SessionResourceUnitType" /> property defines the resource units (for example, nodes or cores).</para> 
        ///   <para>The minimum units must be within the constraints of the template, if there are any.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.MaximumUnits" />
        public int? MinimumUnits
        {
            get { return data.MinUnits; }
            set { data.MinUnits = value; }
        }

        /// <summary>
        ///   <para>The display name of the service job.</para>
        /// </summary>
        /// <value>
        ///   <para>The display name of the service job.</para>
        /// </value>
        public string ServiceJobName
        {
            get { return data.ServiceJobName; }
            set { data.ServiceJobName = value; }
        }

        /// <summary>
        ///   <para>Retrieves or sets the name of the project that is associated with the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The project name. The name is limited to 80 characters.</para>
        /// </value>
        /// <remarks>
        ///   <para>The name is used only for accounting purposes.</para>
        /// </remarks>
        /// <example />
        public string Project
        {
            get { return data.ServiceJobProject; }
            set { data.ServiceJobProject = value; }
        }

        /// <summary>
        ///   <para>Gets or sets a list of the node groups that define the nodes on which the service job for the session can run.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.Collections.Generic.List{T}" /> of strings that contain the names of the node groups that define the nodes on which the service job for the session can run.</para> 
        /// </value>
        /// <remarks>
        ///   <para>Use node groups that contain the nodes on which your job is 
        /// capable of running. For example, the nodes might contain the required software for your job.</para>
        ///   <para>If you specify multiple node groups, the resulting node list is the intersection of the groups. For example if group A 
        /// contains nodes 1, 2, 3, and 4 and group B contains nodes 3, 4, 5, and 6, the resulting list is 3 and 4.</para>
        ///   <para>If you also specify nodes in the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.RequestedNodesList" /> property, the job runs on the intersection of the requested node list and the resulting node group list.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.RequestedNodesList" />
        public List<string> NodeGroupList
        {
            get { return _nodeGroupsList; }
            set { _nodeGroupsList = value; }
        }

        /// <summary>
        ///   <para>Gets or sets a list of nodes that you request to run the service job for the session.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.Collections.Generic.List{T}" /> of strings that contain the names of the node on which you request to run the service job for the session. The nodes must exist in the HPC cluster with the head node that the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Headnode" /> property specifies.</para>
        /// </value>
        /// <remarks>
        ///   <para>Specify a list of the nodes on which your job is 
        /// capable of running. For example, the nodes might contain the required software for your job.</para>
        ///   <para>If you also specify a list of node group names in the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.NodeGroupList" /> property, the job runs on the intersection of the two lists.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.NodeGroupList" />
        public List<string> RequestedNodesList
        {
            get
            {
                return _requestedNodesList;
            }
            set
            {
                _requestedNodesList = value;
            }
        }

        /// <summary>
        ///   <para>Gets the environment variables of the service host.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Collections.Generic.Dictionary{T1,T2}" /> object that contains pairs of environment variable names and values.</para>
        /// </value>
        public Dictionary<string, string> Environments
        {
            get
            {
                if (this.data.Environments == null)
                {
                    this.data.Environments = new Dictionary<string, string>();
                }

                return this.data.Environments;
            }
        }

        /// <summary>
        ///   <para>Gets or sets the priority for the service job.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Int32" /> between 0 and 4000 that specifies the priority for the service job, where 0 is the lowest priority and 4000 is the highest.</para> 
        /// </value>
        /// <remarks>
        ///   <para>The default priority is 2000.</para>
        ///   <para>Server resources are allocated to jobs based on job priority, except for backfill jobs. </para>
        ///   <para>Note that jobs can be preempted. The default preemption mode is Graceful, which means that the 
        /// job is preempted only after its running tasks complete. In case another preemption mode is set, consider setting the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanPreempt" /> property for the service job to False so that the job runs until it finishes, fails, or is canceled.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.SchedulerJob.ExpandedPriority" />
        public int SessionPriority
        {
            get
            {
                return data.ExtendedPriority.HasValue ? data.ExtendedPriority.Value : 2000;
            }
            set
            {
                data.ExtendedPriority = value;
            }
        }

        /// <summary>
        ///   <para>Retrieves or sets the run-time limit for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The run-time limit for the job, in seconds.</para>
        /// </value>
        /// <remarks>
        ///   <para>The wall clock is used to determine the run time. The time is your best guess of how long the job will take. It 
        /// needs to be fairly accurate because it is used to allocate resources. If 
        /// the job exceeds this time, the job is terminated and its state becomes Canceled.</para> 
        /// </remarks>
        /// <example />
        public int Runtime
        {
            get { return data.Runtime; }
            set { data.Runtime = value; }
        }

        /// <summary>
        ///   <para>Gets the version of the service to which the session connected.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Version" /> that contains the version information. If this value is 
        /// null, the session uses the configuration file for the service that does not specify version information. </para>
        /// </value>
        /// <remarks>
        ///   <para>The 
        /// <see cref="System.Version.Build" /> and 
        /// <see cref="System.Version.Revision" /> portions of the version that the 
        /// <see cref="System.Version" /> object represents are not defined for the HPC Pack.</para>
        ///   <para>HPC Pack 2008 is version 2.0. HPC Pack 2008 R2 is version 3.0.</para>
        ///   <para>The version of a service is specified by the file name of the configuration file for the service, which has 
        /// a format of service_name_major.minor.config. For example, MyService_1.0.config. The version must include 
        /// the major and minor portions of the version identifier and no further subversions.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.ServiceVersion" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.GetServiceVersions(System.String,System.String)" />
        public Version ServiceVersion
        {
            get { return this.data.ServiceVersion; }
        }

        /// <summary>
        ///   <para>Gets or sets a 
        /// <see cref="System.Boolean" /> value that indicates whether this 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> uses the session pool.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns a 
        /// <see cref="System.Boolean" /> value that indicates whether this 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> uses the session pool..</para>
        /// </value>
        public bool UseSessionPool
        {
            get { return data.UseSessionPool; }
            set { data.UseSessionPool = value; }
        }

        /// <summary>
        /// <para>Gets or sets a value that indicates whether this 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> uses Azure Queue.</para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public bool? UseAzureQueue
        {
            get { return data.UseAzureQueue; }
            set { data.UseAzureQueue = value; }

        }

        /// <summary>
        /// Get or set whether the username and password windows client credential is used for the authentication
        /// </summary>
        public bool UseWindowsClientCredential
        {
            get { return data.UseWindowsClientCredential; }
            set { data.UseWindowsClientCredential = value; }
        }

        /// <summary>
        /// Get or set the client version
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public Version ClientVersion
        {
            get { return data.ClientVersion; }
            set { data.ClientVersion = value; }
        }

        /// <summary>
        /// Get or set the depend files
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public IDictionary<string, string> DependFiles
        {
            get
            {
                if (this._dependFiles == null)
                {
                    this._dependFiles = new Dictionary<string, string>();
                }
                return this._dependFiles;
            }
            set
            {
                this._dependFiles = new Dictionary<string, string>(value);
            }
        }

        /// <summary>
        /// Get or set the parent job IDs
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public List<int> ParentJobIds
        {
            get
            {
                if (this.data.ParentJobIds == null)
                {
                    this.data.ParentJobIds = new List<int>();
                }
                return this.data.ParentJobIds;
            }
            set
            {
                if (value != null)
                {
                    this.data.ParentJobIds = new List<int>(value);
                }
            }
        }

        /// <summary>
        /// Specify the service host idle timeout, in milliseconds.
        /// </summary>
        public int? ServiceHostIdleTimeout
        {
            get
            {
                return data.ServiceHostIdleTimeout;
            }

            set
            {
                data.ServiceHostIdleTimeout = value;
            }
        }

        /// <summary>
        /// Specify the service hang timeout, in milliseconds.
        /// </summary>
        public int? ServiceHangTimeout
        {
            get
            {
                return data.ServiceHangTimeout;
            }

            set
            {
                data.ServiceHangTimeout = value;
            }
        }

        /// <summary>
        /// the helper function to translate the string collection to a string.
        /// </summary>
        /// <param name="collection">the string collection.</param>
        /// <returns>the translated string from the string collection.</returns>
        private static string Collection2String(ICollection<string> collection)
        {
            if (collection == null)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            bool first = true;
            foreach (string str in collection)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(",");
                }

                builder.Append(str);
            }

            return builder.ToString();
        }

        public SessionStartInfoContract Data
        {
            get
            {
                // update the data
                data.RequestedNodesStr = SessionStartInfo.Collection2String(this._requestedNodes);
                data.NodeGroupsStr = SessionStartInfo.Collection2String(this._nodeGroups);
                data.UseInprocessBroker = this.UseInprocessBroker;
                data.IsNoSession = this.IsNoSession;

                if (this._requestedNodesList.Count != 0)
                {
                    data.RequestedNodesStr = List2String(this._requestedNodesList);
                }

                if (this._nodeGroupsList.Count != 0)
                {
                    data.NodeGroupsStr = List2String(this._nodeGroupsList);
                }

                this.data.UseAad = this.UseAad;

                return data;
            }
        }

        /// <summary>
        /// Gets a value indicating whether debug mode is enabled
        /// </summary>
        public bool DebugModeEnabled
        {
            get { return this.data.EprList != null; }
        }

        /// <summary>
        /// Remove the password and cert info.
        /// </summary>
        public void ClearCredential()
        {
            this.InternalPassword = null;
            this.SavePassword = false;
            this.Certificate = null;
            this.PfxPassword = null;
        }

        private string List2String(List<string> list)
        {
            if (list == null)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            bool first = true;
            foreach (string str in list)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(",");
                }

                builder.Append(str);
            }

            return builder.ToString();
        }

        /// <summary>
        /// it is only used by SOA diagnostic
        /// </summary>
        public bool AdminJobForHostInDiag
        {
            set
            {
                this.data.AdminJobForHostInDiag = value;
            }
        }

        public bool UseAzureStorage => this.data.UseAzureStorage;

        #region ISerializable Members
        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> class with the specified serialization information.</para>
        /// </summary>
        /// <param name="info">
        ///   <para>The <see cref="System.Runtime.Serialization.SerializationInfo" /> object that contains the serialization information.</para>
        /// </param>
        /// <param name="context">
        ///   <para>A <see cref="System.Runtime.Serialization.StreamingContext" /> structure that specifies the destination for this serialization.</para>
        /// </param>
        protected SessionStartInfo(
           SerializationInfo info,
           StreamingContext context)
            : base(info.GetString("Headnode"))
        {
            this.data = (SessionStartInfoContract)info.GetValue("Data", typeof(SessionStartInfoContract));

            _brokerSettings = new BrokerSettingsInfo(data);

            if (!String.IsNullOrEmpty(data.RequestedNodesStr))
                _requestedNodes = new List<string>(data.RequestedNodesStr.Split(','));

            if (!String.IsNullOrEmpty(data.NodeGroupsStr))
                _nodeGroups = new List<string>(data.NodeGroupsStr.Split(','));
        }

        /// <summary>
        /// Stores registartion path in session info and ip address list
        /// </summary>
        public string RegPath
        {
            get => this.data.RegPath;
            set => this.data.RegPath = value;
        }

        public string[] IpAddress
        {
            get => this.data.IpAddress;
            set => this.data.IpAddress = value;
        }

        public string[] BrokerLauncherEprs { get; set; }

        /// <summary>
        /// Initialize a new session start info for No Hpc work
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="regPath"></param>
        /// <param name="svcVersion"></param>
        /// <param name="ipaddress"></param>
        public SessionStartInfo(string serviceName, string regPath, Version svcVersion, params string[] ipaddress) :base()
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(SR.ServiceNameCantBeNull);
            }

            if (!IsServiceNameValid(serviceName))
            {
                throw new ArgumentException(SR.ServiceNameCantBeNull, "serviceName");
            }

            this.IpAddress = ipaddress;
            this.RegPath = regPath;
            this.Init(this.headnode, serviceName, svcVersion);
        }

        /// <summary>
        /// Initialize a new session start info for broker list work
        /// </summary>
        /// <param name="headNode"></param>
        /// <param name="serviceName"></param>
        /// <param name="regPath"></param>
        /// <param name="svcVersion"></param>
        /// <param name="brokerLauncherEprs"></param>
        public SessionStartInfo(string headNode, string serviceName, string regPath, Version svcVersion, params string[] brokerLauncherEprs) : base(headNode)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(SR.ServiceNameCantBeNull);
            }

            if (!IsServiceNameValid(serviceName))
            {
                throw new ArgumentException(SR.ServiceNameCantBeNull, "serviceName");
            }

            // TODO prefix code and affix code
            string prefix = "net.tcp://";
            int port = 9087;
            string affix = ":" + port + "/BrokerLauncher";
            List<string> strlist = new List<string>();
            if (brokerLauncherEprs != null)
            {
                foreach (string epr in brokerLauncherEprs)
                {
                    strlist.Add(prefix + epr + affix);
                }
            }

            this.BrokerLauncherEprs = strlist.ToArray();
            this.RegPath = regPath;
            this.Init(headnode, serviceName, svcVersion);
        }

        /// <summary>
        ///   <para>Populates a 
        /// <see cref="System.Runtime.Serialization.SerializationInfo" /> object with the data that is needed to serialize the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> object.</para>
        /// </summary>
        /// <param name="info">
        ///   <para>The <see cref="System.Runtime.Serialization.SerializationInfo" /> that you want to populate with the data.</para>
        /// </param>
        /// <param name="context">
        ///   <para>A <see cref="System.Runtime.Serialization.StreamingContext" /> structure that specifies the destination for this serialization.</para>
        /// </param>
        /// <remarks>
        ///   <para>Any objects that are included in the 
        /// <see cref="System.Runtime.Serialization.SerializationInfo" /> object are automatically tracked and serialized by the formatter.</para>
        ///   <para>Code that calls 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.#ctor(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" /> requires the  
        /// <see cref="System.Security.Permissions.SecurityPermission" /> object for providing serialization services.</para>
        /// </remarks>
        /// <seealso cref="System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" 
        /// /> 
        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Data", this.data);
            info.AddValue("Headnode", this.headnode);
        }

        public Dictionary<string, string> DependFilesStorageInfo
        {
            get
            {
                return this.data.DependFilesStorageInfo;
            }
            set
            {
                this.data.DependFilesStorageInfo = value;
            }
        }


        #endregion
    }
}
