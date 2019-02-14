//------------------------------------------------------------------------------
// <copyright file="SessionBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The implementation of the SessionBase Class
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para>Serves as a base class to provide methods and properties that are common to classes that represent different kinds of sessions, such as 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession" /> and 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.Session" />.</para>
    /// </summary>
    public class SessionBase : IDisposable
    {
        /// <summary>
        /// the trace source name.
        /// </summary>
        internal const string TraceSourceName = "Microsoft.Hpc.Scheduler.Session";

        /// <summary>
        /// the event source.
        /// </summary>
        internal const string EventSource = "SOA Session API";

        /// <summary>
        /// name of the env variable for launching service host in an admin job.
        /// </summary>
        public static string EnvVarNameForAdminJob => "AdminJobForHostInDiag";

        /// <summary>
        /// max retry count when creating session
        /// </summary>
        public static int MaxRetryCount => 3;

        // the client version
        private static Version clientVersion;

        // the full client version
        private static Version fullclientVersion;

        // the v3sp1 version
        // the server side credential cache is supported in v3sp1 and later
        private static readonly Version V3Sp1Version = new Version(3, 1);

        // the server version
        protected Version serverVersion;

        // The endpoint address
        private readonly EndpointAddress _endpointReference;

        /// <summary>
        /// The lock object to protect the following two job related fields
        /// </summary>
        private object lockObj = new object();

        // This is for disposing it when disposing the session
        protected IDisposable _scheduler;


        // Maintains list of broker clients associated with this session
        // Client id is case insensitive
        private Dictionary<string, BrokerClientBase> _brokerClients = new Dictionary<string, BrokerClientBase>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// This is the trace source for the current session.
        /// </summary>
        private static readonly TraceSource _traceSource = new TraceSource(EventSource);

        /// <summary>
        /// The scheduler headnode name
        /// </summary>
        protected readonly string _headnode;

        private readonly SessionInfoBase _info;

        private readonly int serviceJobId;
        

        /// <summary>
        /// Whether session is shutting down (dispose was called)
        /// </summary>
        private bool shuttingDown;

        /// <summary>
        /// Signaled when broker is down based on heartbeat. This is needed to unbloack any I\O threads to proceed with shutdown
        /// Unsignaled when broker is back.
        /// </summary>
        private ManualResetEvent heartbeatSignaledEvent = new ManualResetEvent(false);

        /// <summary>
        /// Specifies whether the broker node is unavailable (vs just the broker)
        /// </summary>
        private bool isBrokerNodeUnavailable;

        /// <summary>
        /// Stores broker launcher client factory
        /// </summary>
        private BrokerLauncherClientFactory factory;

        /// <summary>
        /// Stores the heartbeat helper
        /// </summary>
        private BrokerHeartbeatHelper heartbeatHelper;

        /// <summary>
        /// The Azure storage proxy
        /// </summary>
        internal AzureQueueProxy AzureQueueProxy { get; set; }

        /// <summary>
        /// The session hash code
        /// </summary>
        private int sessionHash;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int SessionHash
        {
            get
            {
                return sessionHash;
            }
        }

        /// <summary>
        /// Default library directory path on on-premise installation and Azure VM role
        /// </summary>
        private const string HpcAssemblyDir = @"%CCP_HOME%bin";

        /// <summary>
        /// Default library directory path on Azure worker role
        /// </summary>
        private const string HpcAssemblyDir2 = @"%CCP_HOME%";

        /// <summary>
        /// Storage client library name
        /// </summary>
        private const string StorageClientAssemblyName = "Microsoft.WindowsAzure.Storage.dll";

        // protected Lazy<FabricClient> fabricClient;

        // protected CancellationTokenSource cts;

        // protected CancellationToken token;
        static SessionBase()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
        }

        // It can't be constructed outside
        internal protected SessionBase(SessionInfoBase info, string headnode, Binding binding)
        {
            // EndpointsConnectionString endpoints = null;
            // if (EndpointsConnectionString.TryParseConnectionString(headnode, out endpoints))
            // {
            // this.fabricClient = new Lazy<FabricClient>(() => new FabricClient(endpoints.EndPoints), LazyThreadSafetyMode.ExecutionAndPublication);
            // }
            // this.cts = new CancellationTokenSource();
            // this.token = cts.Token;
            this.serviceJobId = info.Id;
            this.factory = new BrokerLauncherClientFactory(info, binding);
            this._info = info;
            this._headnode = headnode;
            this.IsBrokerAvailable = true;

            if (this.serviceJobId == SessionStartInfo.StandaloneSessionId)
            {
                this.sessionHash = 0;
            }
            else
            {
                this.sessionHash = Guid.NewGuid().ToString().GetHashCode();
            }

            if (info is SessionInfo sessionInfo)
            {
                this.UserName = sessionInfo.Username;
                this.InternalPassword = sessionInfo.InternalPassword;

                foreach (string uri in this.SessionInfo.BrokerEpr)
                {
                    if (!string.IsNullOrEmpty(uri))
                    {
                        _endpointReference = new EndpointAddress(uri);

                        // this will automatically get the priority order
                        break;
                    }
                }

                if (this.SessionInfo.DebugModeEnabled)
                {
                    this.serverVersion = ClientVersionInternal;
                }

                // Start heartbeat to broker
                if (!this.SessionInfo.UseInprocessBroker && !this.SessionInfo.UseAzureStorage /* TODO: recover heartbeat via storage queue */)
                {
                    this.heartbeatHelper = new BrokerHeartbeatHelper(
                        this.Info.Id,
                        this.SessionInfo.ClientBrokerHeartbeatInterval,
                        this.SessionInfo.ClientBrokerHeartbeatRetryCount,
                        this.factory,
                        this.SessionInfo.BrokerUniqueId);
                    this.heartbeatHelper.HeartbeatLost += new EventHandler<BrokerHeartbeatEventArgs>(HeartbeatLost);
                }

                // build the proxy if using azure storage queue
                if (this.SessionInfo.UseAzureStorage)
                {
                    this.AzureQueueProxy = new AzureQueueProxy(
                        this.SessionInfo.Headnode,
                        this.SessionInfo.Id,
                        this.sessionHash,
                        this.SessionInfo.AzureRequestQueueUris,
                        this.SessionInfo.AzureRequestBlobUri);
                }
            }
        }

        /// <summary>
        /// Gets the broker launcher client factory
        /// </summary>
        public BrokerLauncherClientFactory BrokerLauncherClientFactory
        {
            get
            {
                return this.factory;
            }
        }

        internal IBrokerLauncher BrokerLauncherClient { get; set; }

        /// <summary>
        /// Gets the instance of SessionInfo
        /// </summary>
        public SessionInfoBase Info
        {
            get
            {
                return this._info;
            }
        }

        /// <summary>
        /// Gets the head node
        /// </summary>
        internal string HeadNode
        {
            get
            {
                return this._headnode;
            }
        }

        /// <summary>
        /// Gets and sets the user name
        /// </summary>
        internal string UserName { get; set; }

        /// <summary>
        /// Gets and sets the password
        /// </summary>
        internal string InternalPassword { get; set; }

        /// <summary>
        /// Gets the session info
        /// </summary>
        private SessionInfo SessionInfo
        {
            get
            {
                return (SessionInfo)this._info;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public static int BatchId = -1;

        /// <summary>
        ///   <para>Retrieves an identifier that uniquely identifies the session.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="System.Int32" /> that serves as the identifier that uniquely identifies the session.</para>
        /// </value>
        /// <remarks>
        ///   <para>In Microsoft HPC Pack, the session identifier is the job identifier of the service job for the session.</para>
        /// </remarks>
        public int Id
        {
            get
            {
                return serviceJobId;
            }
        }

        /// <summary>
        ///   <para>Retrieves the unique network address that a client uses to communicate with a service endpoint.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.ServiceModel.EndpointAddress" /> object that contains the network address..</para>
        /// </value>
        /// <remarks>
        ///   <para>Use the endpoint when you construct the client proxy for an application in Microsoft HPC Pack to communicate with the broker.</para>
        ///   <para>If you specify the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.Http" /> and the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.NetTcp" /> transport schemes, this property contains the endpoint reference for 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.NetTcp" />.</para>
        ///   <para>Instead of using this property, you can use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.HttpEndpointReference" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.NetTcpEndpointReference" /> properties to access the endpoint references.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{TTaskId}.EndpointAddresses" />
        public EndpointAddress EndpointReference
        {
            get
            {
                return _endpointReference;
            }
        }

        /// <summary>
        ///   <para>Gets information about the version of the HPC Pack that is 
        /// installed on the nodes of the cluster that runs the SOA client application.</para>
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
        ///   <para>SOA applications can use this version information when accessing cluster features in order to remain backward compatible.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.ServerVersion" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.ServiceVersion" />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Backward compatibility")]
        public Version ClientVersion
        {
            get
            {
                return ClientVersionInternal;
            }
        }

        /// <summary>
        /// client version, it is used internally.
        /// </summary>
        public static Version ClientVersionInternal
        {
            get
            {
                if (clientVersion == null)
                {
                    // get assembly of current type
                    Assembly assm = Assembly.GetAssembly(typeof(SessionBase));
                    Version version = new Version(FileVersionInfo.GetVersionInfo(assm.Location).FileVersion);

                    // only need the major and minor version components from the file version to build the client version
                    clientVersion = new Version(version.Major, version.Minor);
                }

                return clientVersion;
            }
        }

        /// <summary>
        /// full client version, it is used internally.
        /// </summary>
        internal static Version FullClientVersionInternal
        {
            get
            {
                if (fullclientVersion == null)
                {
                    // get assembly of current type
                    Assembly assm = Assembly.GetAssembly(typeof(SessionBase));
                    fullclientVersion = new Version(FileVersionInfo.GetVersionInfo(assm.Location).FileVersion);
                }

                return fullclientVersion;
            }
        }

        public static TraceSource TraceSource
        {
            get
            {
                return _traceSource;
            }
        }

        /// <summary>
        /// the help function to check the sanity of SessionStartInfo before creating sesison.
        /// </summary>
        /// <param name="startInfo"></param>
        public static void CheckSessionStartInfo(SessionStartInfo startInfo)
        {
            if (startInfo == null)
            {
                throw new ArgumentNullException("startInfo");
            }

            if (startInfo.TransportScheme == TransportScheme.None)
            {
                throw new ArgumentException(SR.MustIndicateTransportScheme, "TransportScheme");
            }

            if ((startInfo.TransportScheme & TransportScheme.WebAPI) == TransportScheme.WebAPI && startInfo.TransportScheme != TransportScheme.WebAPI)
            {
                throw new ArgumentException(SR.TransportSchemeWebAPIExclusive);
            }

            // Bug 11564, SharedSession and UseInprocessBroker cannot be both true.
            if (!startInfo.DebugModeEnabled && startInfo.UseInprocessBroker && startInfo.ShareSession)
            {
                throw new NotSupportedException(SR.InprocessBroker_NotSupportShareSession);
            }

            if (startInfo.BrokerSettings.SessionIdleTimeout != null && startInfo.BrokerSettings.SessionIdleTimeout < 0)
            {
                throw new ArgumentException(SR.SessionIdleTimeoutNotNegative, "SessionIdleTimeout");
            }

            if (startInfo.BrokerSettings.ClientIdleTimeout != null && startInfo.BrokerSettings.ClientIdleTimeout <= 0)
            {
                throw new ArgumentException(SR.ClientIdleTimeoutNotNegative, "ClientIdleTimeout");
            }

            if (startInfo.BrokerSettings.DispatcherCapacityInGrowShrink < 0)
            {
                throw new ArgumentException(SR.DispatcherCapacityInGrowShrinkNonNegative, "DispatcherCapacityInGrowShrink");
            }

            if (startInfo.BrokerSettings.MessagesThrottleStopThreshold != null && startInfo.BrokerSettings.MessagesThrottleStopThreshold < 0)
            {
                throw new ArgumentException(SR.MessageThrottleStopThresholdPositive, "MessagesThrottleStopThreshold");
            }

            if (startInfo.BrokerSettings.MessagesThrottleStopThreshold != null
                && (startInfo.BrokerSettings.MessagesThrottleStartThreshold == null
                    || startInfo.BrokerSettings.MessagesThrottleStartThreshold <= startInfo.BrokerSettings.MessagesThrottleStopThreshold))
            {
                throw new ArgumentException(SR.MessageThrottleStartGreaterStop, "MessagesThrottleStartThreshold");
            }

            if (startInfo.BrokerSettings.MessagesThrottleStartThreshold != null && startInfo.BrokerSettings.MessagesThrottleStartThreshold <= 0)
            {
                throw new ArgumentException(SR.MessageThrottleStartGreaterStop, "MessagesThrottleStartThreshold");
            }

            if (startInfo.ServiceVersion != null && !ParamCheckUtility.IsServiceVersionValid(startInfo.ServiceVersion))
            {
                throw new ArgumentException(SR.ServiceVersionNoZeroMajorAndMinor, "ServiceVersion");
            }

            // only shared session can use session pool
            if (startInfo.UseSessionPool && !startInfo.ShareSession)
            {
                throw new NotSupportedException(SR.UnsharedSession_NotSupportSessionPool);
            }

            // inproc broker needs unshared session, so can't use session pool
            if (startInfo.UseSessionPool && startInfo.UseInprocessBroker)
            {
                throw new NotSupportedException(SR.InprocessBroker_NotSupportSessionPool);
            }

            // set UseAzureQueue value if it is not set by user
            if (startInfo.UseAzureQueue == null)
            {
                if ((startInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http && SoaHelper.IsSchedulerOnIaaS(startInfo.Headnode))
                {
                    startInfo.UseAzureQueue = true;
                }
                else
                {
                    startInfo.UseAzureQueue = false;
                }
            }
        }

        /// <summary>
        /// Handle endpoint not found exception
        /// </summary>
        /// <param name="headnode">indicating headnode</param>
        public static void HandleEndpointNotFoundException(string headnode)
        {
            throw new SessionException(SOAFaultCode.SessionLauncherEndpointNotFound, string.Format(SR.SessionLauncherEndpointNotFound, headnode));
        }

        /// <summary>
        /// Build session info
        /// </summary>
        /// <param name="result">indicating the broker initialization result</param>
        /// <param name="durable">indicating whether it is durable</param>
        /// <param name="secure">indicating whether the session is secure</param>
        /// <param name="id">indicating the session id</param>
        /// <param name="brokerLauncherEpr">indicating the broker launcher epr</param>
        /// <param name="scheme">indicating the transport scheme</param>
        /// <returns>returns the session info</returns>
        internal static SessionInfo BuildSessionInfo(BrokerInitializationResult result, bool durable, int id, string brokerLauncherEpr, Version serviceVersion, SessionStartInfo startInfo)
        {
            SessionInfo info = new SessionInfo();

            if (SoaHelper.IsSchedulerOnIaaS(startInfo.Headnode))
            {
                string suffix = SoaHelper.GetSuffixFromHeadNodeEpr(startInfo.Headnode);
                SoaHelper.UpdateEprWithCloudServiceName(result.BrokerEpr, suffix);
                SoaHelper.UpdateEprWithCloudServiceName(result.ControllerEpr, suffix);
                SoaHelper.UpdateEprWithCloudServiceName(result.ResponseEpr, suffix);
            }

            info.Id = id;
            info.Durable = durable;
            info.BrokerEpr = result.BrokerEpr;
            info.BrokerLauncherEpr = brokerLauncherEpr;
            info.ControllerEpr = result.ControllerEpr;
            info.ResponseEpr = result.ResponseEpr;
            info.Secure = startInfo.Secure;

            // TODO: update the server version
            info.ServerVersion = new Version(3, 0);
            info.TransportScheme = startInfo.TransportScheme;
            info.ServiceOperationTimeout = result.ServiceOperationTimeout;
            info.ServiceVersion = serviceVersion;
            info.MaxMessageSize = result.MaxMessageSize;
            info.UseInprocessBroker = startInfo.UseInprocessBroker;
            info.DebugModeEnabled = startInfo.DebugModeEnabled;
            info.BrokerUniqueId = result.BrokerUniqueId;
            info.Username = startInfo.Username;
            info.InternalPassword = startInfo.InternalPassword;
            info.Headnode = startInfo.Headnode;
            info.UseWindowsClientCredential = startInfo.UseWindowsClientCredential;

            info.UseAzureQueue = startInfo.UseAzureQueue;
            info.AzureRequestQueueUris = result.AzureRequestQueueUris;
            info.AzureRequestBlobUri = result.AzureRequestBlobUri;
            info.UseAad = startInfo.UseAad;
            info.AzureControllerRequestQueueUri = result.AzureControllerRequestQueueUri;
            info.AzureControllerResponseQueueUri = result.AzureControllerResponseQueueUri;

            // If client supplies value use it else use service config values
            if (startInfo.BrokerSettings.ClientBrokerHeartbeatInterval.HasValue)
            {
                info.ClientBrokerHeartbeatInterval = startInfo.BrokerSettings.ClientBrokerHeartbeatInterval.Value;
            }
            else
            {
                info.ClientBrokerHeartbeatInterval = result.ClientBrokerHeartbeatInterval;
            }

            // If client supplies value use it else use service config values
            if (startInfo.BrokerSettings.ClientBrokerHeartbeatRetryCount.HasValue)
            {
                info.ClientBrokerHeartbeatRetryCount = startInfo.BrokerSettings.ClientBrokerHeartbeatRetryCount.Value;
            }
            else
            {
                info.ClientBrokerHeartbeatRetryCount = result.ClientBrokerHeartbeatRetryCount;
            }

            // If client supplies value use it else use service config values
            if (startInfo.BrokerSettings.MaxMessageSize.HasValue)
            {
                info.MaxMessageSize = startInfo.BrokerSettings.MaxMessageSize.Value;
            }
            else
            {
                info.MaxMessageSize = result.MaxMessageSize;
            }

            // If client supplies value use it else use service config values
            if (startInfo.BrokerSettings.ServiceOperationTimeout.HasValue)
            {
                info.ServiceOperationTimeout = startInfo.BrokerSettings.ServiceOperationTimeout.Value;
            }
            else
            {
                info.ServiceOperationTimeout = result.ServiceOperationTimeout;
            }

            return info;
        }

        /// <summary>
        ///   <para>Releases all managed or unmanaged resources that were used by the session, depending on where the method is being called.</para>
        /// </summary>
        /// <param name="disposing">
        ///   <para>
        ///     <see cref="System.Boolean" /> that specifies whether to release managed resources in addition to unmanaged resources. 
        /// True indicates that the managed and unmanaged resources should be released because the code is calling the method directly. 
        /// False indicates that only the unmanaged resources can be disposed of because the method is being called by the 
        /// <see cref="System.Object.Finalize" /> method.</para>
        /// </param>
        /// <remarks>
        ///   <para>The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Dispose" /> is equivalent to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Dispose(System.Boolean)" /> called with the <paramref name="disposing" /> parameter set to 
        /// False.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Dispose" />
        /// <seealso cref="System.Object.Finalize" />
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (this.heartbeatSignaledEvent != null)
                    {
                        try
                        {
                            this.heartbeatSignaledEvent.Dispose();
                        }
                        catch (Exception)
                        {
                        }

                        this.heartbeatSignaledEvent = null;
                    }

                    // Close method will block until all queued heartbeat callback are completed
                    if (this.heartbeatHelper != null)
                    {
                        try
                        {
                            this.heartbeatHelper.Close();
                        }
                        catch (Exception)
                        {
                        }

                        this.heartbeatHelper = null;
                    }

                    if (this._scheduler != null)
                    {
                        try
                        {
                            var disposable = this._scheduler;
                            disposable?.Dispose();
                        }
                        catch (Exception)
                        {
                        }

                        this._scheduler = null;
                    }

                    // Dispose all brokerClients associated with this session. Copy to a new collection
                    // first because BrokerClient.Dispose will remove items and update the collection
                    // CONSIDER : If this slows Close too much, change to Begin\End
                    List<BrokerClientBase> brokerClientsCopy = new List<BrokerClientBase>(this._brokerClients.Values);

                    foreach (BrokerClientBase brokerClient in brokerClientsCopy)
                    {
                        try
                        {
                            brokerClient.Dispose();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    if (this.factory != null)
                    {
                        try
                        {
                            this.factory.Dispose();
                        }
                        catch (Exception)
                        {
                        }

                        this.factory = null;
                    }
                }
                catch (Exception e)
                {
                    // Swallow the exception
                    TraceSource.TraceInformation(e.ToString());
                }

                this.shuttingDown = true;
            }
        }

        /// <summary>
        ///   <para>Releases all unmanaged resources that were used by the session.</para>
        /// </summary>
        /// <remarks>
        ///   <para>This method is equivalent to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Dispose(System.Boolean)" /> called with the disposing parameter set to 
        /// False.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Dispose(System.Boolean)" />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   <para>Closes the session without finishing the job for the session or deleting response messages.</para>
        /// </summary>
        /// <remarks>
        ///   <para>This method is equivalent to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean)" /> method with the purge parameter set to 
        /// False. To finish the job for the session if the job is still active and delete the response messages when you close the session, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean,System.Int32)" /> method instead. </para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean,System.Int32)" />
        public void Close()
        {
            this.Close(true, Constant.PurgeTimeoutMS);
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
        ///   <para>Calling this method with the <paramref name="purge" /> parameter set to 
        /// False is equivalent to calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close()" /> method.</para>
        ///   <para>The default timeout period for finishing the job and deleting the response 
        /// messages is 60,000 milliseconds. To specify a specific length for the timeout period, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean,System.Int32)" /> method instead.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Close(System.Boolean,System.Int32)" />
        public void Close(bool purge)
        {
            this.Close(purge, Constant.PurgeTimeoutMS);
        }

        /// <summary>
        ///   <para>Closes the session, and optionally finishes the job for 
        /// the session and deletes the response messages subject to the specified timeout period.</para>
        /// </summary>
        /// <param name="purge">
        ///   <para>A 
        /// <see cref="System.Boolean" /> object that specifies whether to finish the job for the session and delete the response messages. 
        /// True finishes the job for the session and deletes the response messages. 
        /// False indicates that the method should not finish the job for the session and should not deletes the response messages.</para>
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>Specifies the length of time in milliseconds that the method 
        /// should wait for the job to finish and the response messages to be deleted.</para>
        /// </param>
        /// <exception cref="System.TimeoutException">
        ///   <para>Specifies that the job for the session did not finish or 
        /// the response messages were not deleted before the end of the specified time period.</para>
        /// </exception>
        public void Close(bool purge, int timeoutMilliseconds)
        {
            Utility.ThrowIfInvalidTimeout(timeoutMilliseconds, "timeoutMilliseconds");
            try
            {
                if (purge)
                {
                    IBrokerLauncher broker = this.BrokerLauncherClient;
                    if (broker == null)
                    {
                        broker = this.factory?.GetBrokerLauncherClient(timeoutMilliseconds);
                    }

                    broker?.Close(this.Id);
                }
            }
            catch (Exception ex)
            {
                TraceSource.TraceInformation(ex.ToString());
                throw;
            }
            finally
            {
                this.Dispose();
            }
        }

        /// <summary>
        ///   <para>Gets the versions of the specified service that are installed on the HPC cluster with the specified head node.</para>
        /// </summary>
        /// <param name="headNode">
        ///   <para>String that specifies the head node of the HPC cluster for which you want to get the installed versions of the service.</para>
        /// </param>
        /// <param name="serviceName">
        ///   <para>String that specifies the name of the service for which you want to get the versions that are installed on the HPC cluster.</para>
        /// </param>
        /// <returns>
        ///   <para>An array of 
        /// <see cref="System.Version" /> object that indicate the versions of the specified service that are installed on the HPC cluster.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Client application that support multiple versions of a Windows Communication Foundation (WCF) service can use this method to 
        /// check the version of the service that are available on the HPC cluster before using version-specific features of the service. </para>
        ///   <para>The version of a service is specified by the file name of the configuration file for the service, which has 
        /// a format of service_name_major.minor.config. For example, MyService_1.0.config. The version must include 
        /// the major and minor portions of the version identifier and no further subversions.</para> 
        ///   <para>If the configuration file for the service does not include version information, this method gets a single 
        /// <see cref="System.Version" /> object with the 
        /// <see cref="System.Version.Major" /> and 
        /// <see cref="System.Version.Minor" /> properties set to 0.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.ServiceVersion" />
        public static Version[] GetServiceVersions(string headNode, string serviceName)
        {
            return GetServiceVersions(headNode, serviceName, null);
        }

        /// <summary>
        /// Returns the versions for a specific service
        /// </summary>
        /// <param name="headNode">headnode of cluster to connect to </param>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>Available service versions</returns>
        public static Version[] GetServiceVersions(string headNode, string serviceName, Binding binding)
        {
            return GetServiceVersionsAsync(headNode, serviceName, binding, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Returns the versions for a specific service async
        /// </summary>
        /// <param name="headNode">headnode of cluster to connect to </param>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <param name="binding">indicating the binding</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Available service versions</returns>
        public static Task<Version[]> GetServiceVersionsAsync(string headNode, string serviceName, Binding binding, CancellationToken token) =>
            GetServiceVersionsAsync(headNode, serviceName, binding, true, token);

        /// <summary>
        /// Returns the versions for a specific service async
        /// </summary>
        /// <param name="headNode">headnode of cluster to connect to </param>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <param name="binding">indicating the binding</param>
        /// <param name="useWindowsAuthentication">If we use Windows authentication in this channel</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Available service versions</returns>
        public static async Task<Version[]> GetServiceVersionsAsync(string headNode, string serviceName, Binding binding, bool useWindowsAuthentication, CancellationToken token)
        {
            string headNodeMachine = await HpcContext.GetOrAdd(headNode, token).ResolveSessionLauncherNodeAsync().ConfigureAwait(false);
            SessionLauncherClient client = new SessionLauncherClient(headNodeMachine, binding, !useWindowsAuthentication);
            try
            {
                return await client.GetServiceVersionsAsync(serviceName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                TraceSource.TraceInformation(ex.ToString());
                throw;
            }
            finally
            {
                Utility.SafeCloseCommunicateObject(client);
            }
        }

        /// <summary>
        ///   <para>Gets the versions of the specified service that are installed on 
        /// the HPC cluster with the specified head node by using the specified transport scheme.</para>
        /// </summary>
        /// <param name="headNode">
        ///   <para>String that specifies the head node of the HPC cluster for which you want to get the installed versions of the service.</para>
        /// </param>
        /// <param name="serviceName">
        ///   <para>String that specifies the name of the service for which you want to get the versions that are installed on the HPC cluster.</para>
        /// </param>
        /// <param name="scheme">
        ///   <para>Specifies the transport scheme to use for the request.</para>
        /// </param>
        /// <returns>
        ///   <para>An array of 
        /// <see cref="System.Version" /> object that indicate the versions of the specified service that are installed on the HPC cluster.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Client application that support multiple versions of a Windows Communication Foundation (WCF) service can use this method to 
        /// check the version of the service that are available on the HPC cluster before using version-specific features of the service. </para>
        ///   <para>The version of a service is specified by the file name of the configuration file for the service, which has 
        /// a format of service_name_major.minor.config. For example, MyService_1.0.config. The version must include 
        /// the major and minor portions of the version identifier and no further subversions.</para> 
        ///   <para>If the configuration file for the service does not include version information, this method gets a single 
        /// <see cref="System.Version" /> object with the 
        /// <see cref="System.Version.Major" /> and 
        /// <see cref="System.Version.Minor" /> properties set to 0.</para>
        /// </remarks>
        public static Version[] GetServiceVersions(string headNode, string serviceName, TransportScheme scheme)
        {
            return GetServiceVersions(headNode, serviceName, scheme, null, null);
        }

        /// <summary>
        ///   <para>Gets the versions of the specified service that are installed on 
        /// the HPC cluster with the specified head node by using the specified transport scheme.</para>
        /// </summary>
        /// <param name="headNode">
        ///   <para>String that specifies the head node of the HPC cluster for which you want to get the installed versions of the service.</para>
        /// </param>
        /// <param name="serviceName">
        ///   <para>String that specifies the name of the service for which you want to get the versions that are installed on the HPC cluster.</para>
        /// </param>
        /// <param name="scheme">
        ///   <para>Specifies the transport scheme to use for the request.</para>
        /// </param>
        /// <param name="username">
        ///   <para>String that specifies the username for the request.</para>
        /// </param>
        /// <param name="password">
        ///   <para>String that specifies the password for the request.</para>
        /// </param>
        /// <returns>
        ///   <para>An array of 
        /// <see cref="System.Version" /> object that indicate the versions of the specified service that are installed on the HPC cluster.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Client application that support multiple versions of a Windows Communication Foundation (WCF) service can use this method to 
        /// check the version of the service that are available on the HPC cluster before using version-specific features of the service. </para>
        ///   <para>The version of a service is specified by the file name of the configuration file for the service, which has 
        /// a format of service_name_major.minor.config. For example, MyService_1.0.config. The version must include 
        /// the major and minor portions of the version identifier and no further subversions.</para> 
        ///   <para>If the configuration file for the service does not include version information, this method gets a single 
        /// <see cref="System.Version" /> object with the 
        /// <see cref="System.Version.Major" /> and 
        /// <see cref="System.Version.Minor" /> properties set to 0.</para>
        /// </remarks>
        public static Version[] GetServiceVersions(string headNode, string serviceName, TransportScheme scheme, string username, string password)
        {
            return GetServiceVersions(headNode, serviceName);
        }

        /// <summary>
        ///   <para>Gets the version of the service to which the session connected.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Version" /> that contains the version information. The value of this property can be 
        /// null if the session uses a service with configuration file that does not specify a version.</para>
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
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.NoServiceVersion" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.ClientVersion" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.GetServiceVersions(System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.ServiceVersion" />
        public Version ServiceVersion
        {
            get
            {
                return this.Info.ServiceVersion;
            }
        }

        /// <summary>
        ///   <para>Gets a <see cref="System.Version" /> object that represents a service with no version information.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Version" /> object that represent a service with no version information.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property gets a 
        /// <see cref="System.Version" /> object with the 
        /// <see cref="System.Version.Major" /> and 
        /// <see cref="System.Version.Minor" /> properties set to 0.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionBase.ServiceVersion" />
        static public Version NoServiceVersion
        {
            get
            {
                return Constant.VersionlessServiceVersion;
            }
        }

       

        /// <summary>
        /// Remove the cached password from the local Windows Vault.
        /// </summary>
        /// <param name="info">the info specifies targeted headnode and username</param>
        public static void PurgeCredential(SessionStartInfo info)
        {
            PurgeCredential(info.Headnode, info.Username);
        }

        /// <summary>
        /// Remove the cached password from the local Windows Vault.
        /// </summary>
        /// <param name="info">
        /// attach info
        /// it specifies headnode and username
        /// </param>
        public static void PurgeCredential(SessionAttachInfo info)
        {
            PurgeCredential(info.Headnode, info.Username);
        }

        /// <summary>
        /// Remove the cached password from the local Windows Vault.
        /// </summary>
        ///<param name="headnode">head node name</param>
        ///<param name="username">user name</param>
        public static void PurgeCredential(string headnode, string username)
        {
            try
            {
                Debug.Assert(!string.IsNullOrEmpty(headnode), "The headnode can't be null or empty.");
                CredentialHelper.PurgePassword(headnode, username);
                SessionBase.TraceSource.TraceInformation("Cached credential is purged.");
            }
            catch (Win32Exception)
            {
                SessionBase.TraceSource.TraceInformation("Cached credential can't be purged.");
            }
        }

        /// <summary>
        /// Get the timspan from a targeted time
        /// </summary>
        /// <param name="targetTimeout">the target time to timeout</param>
        /// <returns>a timespan if timeout if not reached. otherwise a timeout exception will throw.</returns>
        public static TimeSpan GetTimeout(DateTime targetTimeout)
        {
            if (targetTimeout == DateTime.MaxValue)
            {
                return TimeSpan.MaxValue;
            }

            if (targetTimeout > DateTime.Now)
            {
                return targetTimeout - DateTime.Now;
            }
            else
            {
                throw new TimeoutException(SR.OperationTimeout);
            }
        }



        /// <summary>
        /// Associated a BrokerClient with this Session
        /// </summary>
        /// <param name="clientID">clientID</param>
        /// <param name="brokerClient">Broker Client</param>
        internal void AddBrokerClient(string clientID, BrokerClientBase brokerClient)
        {
            lock (this._brokerClients)
            {
                if (this._brokerClients.ContainsKey(clientID))
                    throw new ArgumentException(SR.DuplicateClientId);

                this._brokerClients.Add(clientID, brokerClient);
            }
        }

        /// <summary>
        /// Disassociates a BrokerClient with a session
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="brokerClient"></param>
        internal void RemoveBrokerClient(string clientID)
        {
            lock (this._brokerClients)
            {
                // Returns fails if not found (doesnt throw)
                this._brokerClients.Remove(clientID);
            }
        }

        /// <summary>
        /// Reset the heartbeat
        /// </summary>
        public void ResetHeartbeat()
        {
            if (this.heartbeatHelper != null)
            {
                // mark that broker is availabe again
                this.IsBrokerAvailable = true;

                // mark that broker node is up
                this.isBrokerNodeUnavailable = false;

                this.heartbeatSignaledEvent.Reset();
                this.heartbeatHelper.Reset();
            }
        }

        /// <summary>
        /// Triggered when heartbeat is lost
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="args">indicating the event args</param>
        private void HeartbeatLost(object sender, BrokerHeartbeatEventArgs args)
        {
            this.SendBrokerDownSignal(args.IsBrokerNodeDown);
        }

        /// <summary>
        /// Signals all brokerClient objects associated with the session that broker is down
        /// </summary>
        private void SendBrokerDownSignal(bool isBrokerNodeDown)
        {
            this.IsBrokerAvailable = false;

            // Save whether the broker or broker node is down
            this.isBrokerNodeUnavailable = isBrokerNodeDown;

            // Signal broker is down. 
            this.heartbeatSignaledEvent.Set();

            // BUG 5411 : Need to make a copy of BroekrCLient collection because the user may close the broker client
            // in a response handler which removes it from this._brokerClients while this function is enumerating the 
            // collection which isnt allowed
            Dictionary<string, BrokerClientBase> brokerClients = null;
            lock (this._brokerClients)
            {
                brokerClients = new Dictionary<string, BrokerClientBase>(this._brokerClients);
            }

            // CONSIDER : If this is too slow, change to Begin\End
            foreach (BrokerClientBase brokerClient in brokerClients.Values)
            {
                brokerClient.SendBrokerDownSignal(isBrokerNodeDown);
            }
        }

        /// <summary>
        /// Returns exception thrown when heartbeat expires
        /// </summary>
        static public Exception GetHeartbeatException(bool isBrokerNodeUnavailable)
        {
            return isBrokerNodeUnavailable
                       ? new SessionException(SOAFaultCode.Broker_BrokerNodeUnavailable, SR.BrokerNodeIsUnavailable)
                       : new SessionException(SOAFaultCode.Broker_BrokerUnavailable, SR.BrokerIsUnavailable);
        }

        /// <summary>
        /// Gets exception thrown when client is purged
        /// </summary>
        static internal Exception ClientPurgedException
        {
            get
            {
                return new SessionException(SOAFaultCode.ClientPurged, SR.ClientPurged);
            }
        }

        /// <summary>
        /// Gets the exception thrown when client timed out
        /// </summary>
        static internal Exception ClientTimeoutException
        {
            get
            {
                return new SessionException(SOAFaultCode.ClientTimeout, SR.ClientTimeout);
            }
        }

        /// <summary>
        /// Returns event signalled when broker is down
        /// </summary>
        public WaitHandle HeartbeatSignaledEvent
        {
            get
            {
                return this.heartbeatSignaledEvent;
            }
        }

        /// <summary>
        /// If corresponding broker is avaiable or not.
        /// </summary>
        public bool IsBrokerAvailable { get; private set; }

        /// <summary>
        /// Is broker node unavailable. This field is used by interactive session only
        /// </summary>
        public bool IsBrokerNodeUnavailable
        {
            get
            {
                return this.isBrokerNodeUnavailable;
            }
        }

        /// <summary>
        /// Load the assembly from some customized path, if it cannot be found automatically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A System.ResolveEventArgs that contains the event data.</param>
        /// <returns>targeted assembly</returns>
        private static Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
            {
                return null;
            }

            // Session API assembly may be installed in GAC, or %CCP_HOME%bin,
            // or "%CCP_HOME%"; while Microsoft.WindowsAzure.Storage.dll
            // may be installed in %CCP_HOME%bin, or "%CCP_HOME%".  If they are
            // located at different places, we need load it from target folder
            // explicitly
            AssemblyName targetAssemblyName = new AssemblyName(args.Name);
            if (targetAssemblyName.Name.Equals(Path.GetFileNameWithoutExtension(StorageClientAssemblyName), StringComparison.OrdinalIgnoreCase))
            {
                string assemblyPath = Path.Combine(Environment.ExpandEnvironmentVariables(HpcAssemblyDir), StorageClientAssemblyName);
                if (!File.Exists(assemblyPath))
                {
                    assemblyPath = Path.Combine(Environment.ExpandEnvironmentVariables(HpcAssemblyDir2), StorageClientAssemblyName);
                }

                if (!File.Exists(assemblyPath))
                {
                    return null;
                }

                try
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
                catch (Exception ex)
                {
                    TraceSource.TraceInformation("[SessionBase] .ResolveHandler: failed to load assembly {0}: {1}", assemblyPath, ex);
                    return null;
                }
            }

            return null;
        }
    }
}