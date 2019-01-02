//------------------------------------------------------------------------------
// <copyright file="InprocBrokerAdapter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Broker launcher adapter for inproc broker
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Hpc.Rest;
    using Microsoft.Hpc.Scheduler.Session.ServiceContainer;

    /// <summary>
    /// Broker launcher adapter for inproc broker
    /// </summary>
    /// <remarks>
    /// This class is not thread safe, need to lock the instance when access.
    /// </remarks>
    internal class InprocBrokerAdapter : IBrokerLauncher
    {
        /// <summary>
        /// Stores the ClusterModel assembly full name
        /// </summary>
        // private const string ClusterModelFullName = "Microsoft.Ccp.ClusterModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Stores BrokerBase assembly full name
        /// </summary>
        private const string BrokerBaseFullName = "BrokerBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Stores MSMQInterop assembly full name
        /// </summary>
        // private const string MSMQInteropFullName = "MSMQInterop, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Stores broker core service lib full name
        /// </summary>
        private const string BrokerCoreServiceLibFullName = "Microsoft.Hpc.SvcBroker, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Stores soa common utility lib full name
        /// </summary>
        private const string SoaCommonUtilityLibFullName = "Microsoft.Hpc.Scheduler.Session.Utility, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Stores the full class name of BrokerEntry class
        /// </summary>
        private const string BrokerEntryClassFullName = "Microsoft.Hpc.ServiceBroker.BrokerEntry";

        /// <summary>
        /// Stores the full class name of the TraceHelper class, including assembly name
        /// </summary>
        private const string TraceHelperClassFullName = "Microsoft.Hpc.RuntimeTrace.TraceHelper, BrokerBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Stores RESTServiceModel assembly full name
        /// </summary>
        private const string RESTServiceModelLibFullName = "Microsoft.Hpc.SvcHostRestServer, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Stores StrategyConfig assembly full name
        /// </summary>
        private const string SoaAmbientConfigLibFullName = "SoaAmbientConfig, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null";

        /// <summary>
        /// Stores the ClusterModel assembly full path
        /// </summary>
        private string clusterModelFullPath;

        /// <summary>
        /// Stores the broker base full path
        /// </summary>
        private string brokerBaseFullPath;

        /// <summary>
        /// Stores the broker base full path
        /// </summary>
        private string MSMQInteropFullPath;

        /// <summary>
        /// Stores the broker core service lib path
        /// </summary>
        private string brokerCoreServiceLibPath;

        /// <summary>
        /// Stores the soa common utility lib path
        /// </summary>
        private string soaCommonUtilityLibPath;

        /// <summary>
        /// Stores the broker entry instance
        /// </summary>
        private IBrokerEntry brokerEntry;

        /// <summary>
        /// store the REST service model lib path
        /// </summary>
        private string RESTServiceModelLibPath;

        /// <summary>
        /// store the strategy configuration lib path
        /// </summary>
        private string SoaAmbientConfigLibPath;

        /// <summary>
        /// Stores the start info
        /// </summary>
        private SessionStartInfo startInfo;

        /// <summary>
        /// Stores a value indicating whether it is attaching
        /// </summary>
        private bool attached;

        /// <summary>
        /// Stores the initialization result
        /// </summary>
        private BrokerInitializationResult result;

        /// <summary>
        /// Stores the broker start information
        /// </summary>
        private BrokerStartInfo brokerInfo;

        /// <summary>
        /// Stores the session start information
        /// </summary>
        private SessionStartInfoContract startInfoContract;

        /// <summary>
        /// Stores a value indicating whether debug mode had been enabled
        /// </summary>
        private bool isDebugModeEnabled;

        /// <summary>
        /// Stores a value indicating whether has session Id
        /// </summary>
        private bool isNoSession;

        /// <summary>
        /// Stores the list of all frontend adapters
        /// </summary>
        private List<InprocessBrokerFrontendAdapter> frontendAdapters = new List<InprocessBrokerFrontendAdapter>();

        /// <summary>
        /// Binding
        /// </summary>
        private Binding binding;

        /// <summary>
        /// Initializes a new instance of the InprocBrokerAdapter class
        /// </summary>
        /// <param name="headnode">indicating the head node</param>
        /// <param name="attached">indicating whether it is attaching</param>
        /// <param name="isDebugModeEnabled">indicating whether debug mode had been enabled</param>
        /// <param name="binding">indicating the binding</param>
        public InprocBrokerAdapter(SessionStartInfo startInfo, bool attached, bool isDebugModeEnabled, Binding binding)
        {
            this.startInfo = startInfo;
            this.attached = attached;
            this.isDebugModeEnabled = isDebugModeEnabled;
            this.binding = binding;
            this.isNoSession = startInfo.IsNoSession;
            string homePath = Environment.GetEnvironmentVariable(Constant.HomePathEnvVar);
            string currentPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string binPath = null;

            if (!string.IsNullOrEmpty(homePath))
            {
                binPath = Path.Combine(homePath, "Bin");
            }

            // this.clusterModelFullPath = BuildPath(binPath, currentPath, "Microsoft.Ccp.ClusterModel.dll");
            this.brokerBaseFullPath = BuildPath(binPath, currentPath, "BrokerBase.dll");
            // this.MSMQInteropFullPath = BuildPath(binPath, currentPath, "MSMQInterop.dll");
            this.brokerCoreServiceLibPath = BuildPath(binPath, currentPath, "Microsoft.Hpc.SvcBroker.dll");
            this.RESTServiceModelLibPath = BuildPath(binPath, currentPath, "Microsoft.Hpc.SvcHostRestServer.dll");
            this.soaCommonUtilityLibPath = BuildPath(binPath, currentPath, "Microsoft.Hpc.Scheduler.Session.Utility.dll");
            this.SoaAmbientConfigLibPath = BuildPath(binPath, currentPath, "SoaAmbientConfig.dll");
        }

        private static string BuildPath(string binPath, string currentPath, string libName)
        {
            if (File.Exists(Path.Combine(currentPath, libName)))
            {
                return Path.Combine(currentPath, libName);
            }
            else if (!string.IsNullOrEmpty(binPath))
            {
                return Path.Combine(binPath, libName);
            }
            else
            {
                throw new InvalidOperationException($"No path valid for {binPath}, {currentPath}, {libName}.");
            }
        }

        /// <summary>
        /// Create a new broker
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionid">the session id which is also service job id</param>
        /// <returns>The session Info</returns>
        public BrokerInitializationResult Create(SessionStartInfoContract info, int sessionId)
        {
            // TODO: SF: change to async
            this.ThrowIfInitialized();
            this.startInfoContract = info;
            if (info.IsNoSession)
            {
                this.BuildStandaloneBrokerStartInfo(info.ServiceName, info.ServiceVersion, sessionId, false, this.attached);
            }
            else
            {
                this.BuildBrokerStartInfo(info.ServiceName, info.ServiceVersion, sessionId, false, this.attached).GetAwaiter().GetResult();
            }

            this.CreateInternal(sessionId).GetAwaiter().GetResult();
            return this.result;
        }

        /// <summary>
        /// Create a new broker which working under Durable manner.
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionid">the session id which is also service job id</param>
        /// <returns>The created session info</returns>
        public BrokerInitializationResult CreateDurable(SessionStartInfoContract info, int sessionId)
        {
            this.ThrowIfInitialized();
            this.startInfoContract = info;
            this.BuildBrokerStartInfo(info.ServiceName, info.ServiceVersion, sessionId, true, this.attached).GetAwaiter().GetResult();
            this.CreateInternal(sessionId).GetAwaiter().GetResult();
            return this.result;
        }

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="sessionId">The session Identity</param>
        /// <returns>the Broker Launcher EPR</returns>
        public BrokerInitializationResult Attach(int sessionId)
        {
            if (this.brokerEntry == null)
            {
                // TODO: Support attach
                throw new NotImplementedException();
            }

            this.brokerEntry.Attach();
            return this.result;
        }

        /// <summary>
        /// Gets broker frontend instance
        /// </summary>
        /// <param name="callbackInstance">indicating callback instance</param>
        /// <returns>returns broker frontend instance</returns>
        public IBrokerFrontend GetBrokerFrontend(IResponseServiceCallback callbackInstance)
        {
            this.ThrowIfClosed();
            InprocessBrokerFrontendAdapter frontendAdapter = new InprocessBrokerFrontendAdapter(this.brokerEntry, callbackInstance);
            lock (this.frontendAdapters)
            {
                this.frontendAdapters.Add(frontendAdapter);
            }

            return frontendAdapter;
        }

        /// <summary>
        /// Clean up all the resource related to this session.
        /// Finish the session Job
        /// </summary>
        /// <param name="sessionId">The session id</param>
        public void Close(int sessionId)
        {
            if (this.brokerEntry != null)
            {
                this.brokerEntry.Close(true);
            }
        }

        #region NotSupported
        /// <summary>
        /// Async Create a new broker
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionid">the session id which is also service job id</param>
        /// <returns>The session Info</returns>
        IAsyncResult IBrokerLauncher.BeginCreate(SessionStartInfoContract info, int sessionId, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// End the async creating
        /// </summary>
        /// <param name="ar">The async result</param>
        /// <returns>the sessioninfo</returns>
        BrokerInitializationResult IBrokerLauncher.EndCreate(IAsyncResult ar)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pings specified broker
        /// </summary>
        /// <param name="sessionID">sessionID of broker to ping</param>
        bool IBrokerLauncher.PingBroker(int sessionID)
        {
            // Inproc broker does not support ping broker
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pings specified broker
        /// </summary>
        /// <param name="sessionID">sessionID of broker to ping</param>
        String IBrokerLauncher.PingBroker2(int sessionID)
        {
            // Inproc broker does not support ping broker
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pings specified broker
        /// </summary>
        /// <param name="sessionID">sessionID of broker to ping</param>
        IAsyncResult IBrokerLauncher.BeginPingBroker(int sessionID, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pings specified broker
        /// </summary>
        /// <param name="sessionID">sessionID of broker to ping</param>
        IAsyncResult IBrokerLauncher.BeginPingBroker2(int sessionID, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pings specified broker
        /// </summary>
        /// <param name="sessionID">sessionID of broker to ping</param>
        bool IBrokerLauncher.EndPingBroker(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pings specified broker
        /// </summary>
        /// <param name="sessionID">sessionID of broker to ping</param>
        String IBrokerLauncher.EndPingBroker2(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Create a new broker which working under Durable manner.
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionid">the session id which is also service job id</param>
        /// <param name="callback">The async callback</param>
        /// <param name="state">the async state</param>
        /// <returns>Async result</returns>
        IAsyncResult IBrokerLauncher.BeginCreateDurable(SessionStartInfoContract info, int sessionId, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// End the async operation of CreateDurable
        /// </summary>
        /// <param name="ar">the async result</param>
        /// <returns>The session Info</returns>
        BrokerInitializationResult IBrokerLauncher.EndCreateDurable(IAsyncResult ar)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="sessionId">The session Identity</param>
        /// <returns>IAsyncResult instance</returns>
        IAsyncResult IBrokerLauncher.BeginAttach(int sessionId, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="result">The IAsyncResult instance</param>
        /// <returns>IAsyncResult instance</returns>
        BrokerInitializationResult IBrokerLauncher.EndAttach(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Clean up all the resource related to this session.
        /// Finish the session Job
        /// </summary>
        /// <param name="sessionId">The session id</param>
        IAsyncResult IBrokerLauncher.BeginClose(int sessionId, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Clean up all the resource related to this session.
        /// Finish the session Job
        /// </summary>
        void IBrokerLauncher.EndClose(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the active broker id list
        /// </summary>
        /// <returns>the list of active broker's session id</returns>
        int[] IBrokerLauncher.GetActiveBrokerIdList()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the active broker id list
        /// </summary>
        /// <returns>the list of active broker's session id</returns>
        IAsyncResult IBrokerLauncher.BeginGetActiveBrokerIdList(AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the active broker id list
        /// </summary>
        /// <returns>the list of active broker's session id</returns>
        int[] IBrokerLauncher.EndGetActiveBrokerIdList(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        #endregion

        /// <summary>
        /// Sets the TraceHelper.IsDiagTraceEnabled property for inprocess broker
        /// </summary>
        private void SetIsDiagTraceEnabledProperty()
        {
            Type traceHelper = Type.GetType(TraceHelperClassFullName, true);
            PropertyInfo isDiagTraceEnabled = traceHelper.GetProperty("IsDiagTraceEnabled", BindingFlags.Public | BindingFlags.Static);
            Delegate d = Delegate.CreateDelegate(isDiagTraceEnabled.PropertyType, this, "IsDiagTraceEnabled", false, true);
            isDiagTraceEnabled.SetValue(null, d, null);
        }

        /// <summary>
        /// Throw heartbeat exception if the broker is closed
        /// </summary>
        private void ThrowIfClosed()
        {
            if (this.brokerEntry == null)
            {
                throw SessionBase.GetHeartbeatException(false);
            }
        }

        /// <summary>
        /// Build broker start information
        /// </summary>
        /// <param name="serviceName">indicating service name</param>
        /// <param name="serviceVersion">indicating service version</param>
        /// <param name="sessionId">indicating session id</param>
        /// <param name="durable">indicating whether the session is durable</param>
        /// <param name="attached">indicating whether the session is raised up by attaching</param>
        private async Task BuildBrokerStartInfo(string serviceName, Version serviceVersion, int sessionId, bool durable, bool attached)
        {
            this.brokerInfo = new BrokerStartInfo();
            this.brokerInfo.Headnode = this.startInfo.Headnode;

            // Secure will be set to false and the following two settings are ignored
            this.brokerInfo.JobOwnerSID = null;
            this.brokerInfo.JobTemplateACL = null;
            this.brokerInfo.PersistVersion = BrokerVersion.DefaultPersistVersion;
            this.brokerInfo.SessionId = sessionId;
            this.brokerInfo.Attached = attached;
            this.brokerInfo.Durable = durable;
            this.brokerInfo.NetworkPrefix = Constant.EnterpriseNetwork;
            this.brokerInfo.ConfigurationFile = await FetchServiceRegistrationPath(serviceName, serviceVersion).ConfigureAwait(false);

            // Bug 14892: Fetch AutoShrinkEnabled property from scheduler (via session launcher)
            if (!this.isDebugModeEnabled)
            {
                SessionLauncherClient client = new SessionLauncherClient(await Utility.GetSessionLauncherAsync(this.startInfo, this.binding).ConfigureAwait(false), this.binding, this.startInfo.IsAadOrLocalUser);

                try
                {
                    this.brokerInfo.AutomaticShrinkEnabled = await RetryHelper<bool>.InvokeOperationAsync(
                        async () => Convert.ToBoolean(await client.GetSOAConfigurationAsync(Constant.AutomaticShrinkEnabled).ConfigureAwait(false)),
                        async (e, count) =>
                        {
                            SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[InprocBrokerAdapter] Failed to get AutomaticShrinkEnabled property via session launcher service: {0}\nRetryCount = {1}", e, count);
                            Utility.SafeCloseCommunicateObject(client);
                            client = new SessionLauncherClient(await Utility.GetSessionLauncherAsync(this.startInfo, this.binding).ConfigureAwait(false), this.binding, this.startInfo.IsAadOrLocalUser);
                        },
                        SoaHelper.GetDefaultExponentialRetryManager()).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[InprocBrokerAdapter] Failed to get AutomaticShrinkEnabled property via session launcher service: {0}", e);
                }
                finally
                {
                    Utility.SafeCloseCommunicateObject(client);
                }

                this.brokerInfo.EnableDiagTrace = false; // TODO: retrieve this from session

                // SchedulerAdapterInternalClient schedulerAdapterClient = new SchedulerAdapterInternalClient(await this.startInfo.ResolveHeadnodeMachineAsync().ConfigureAwait(false));
                /*
                ISchedulerAdapter schedulerAdapterClient = SessionServiceContainer.SchedulerAdapterInstance;
                try
                {
                    this.brokerInfo.EnableDiagTrace = await RetryHelper<bool>.InvokeOperationAsync(
                        async () => await
#if net40
                        TaskEx.Run(
#else
                        Task.Run(
#endif
                            () => schedulerAdapterClient.IsDiagTraceEnabled(sessionId)).ConfigureAwait(false),
                        async (e, count) =>
                        {
                            SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[InprocBrokerAdapter] Failed to get IsDiagTraceEnabled property via session launcher service: {0}\nRetryCount = {1}", e, count);
                            var communicateObj = schedulerAdapterClient as ICommunicationObject;
                            if (communicateObj != null)
                            {
                                Utility.SafeCloseCommunicateObject(communicateObj);
                            }

                            //schedulerAdapterClient = new SchedulerAdapterInternalClient(await this.startInfo.ResolveHeadnodeMachineAsync().ConfigureAwait(false));
                            schedulerAdapterClient = SessionServiceContainer.SchedulerAdapterInstance;
                        },
                        SoaHelper.GetDefaultExponentialRetryManager()).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[InprocBrokerAdapter] Failed to get IsDiagTraceEnabled property via session launcher service: {0}", e);
                }
                finally
                {
                    var communicateObj = schedulerAdapterClient as ICommunicationObject;
                    if (communicateObj != null)
                    {
                        Utility.SafeCloseCommunicateObject(communicateObj);
                    }

                }
                */
            }
        }

        private void BuildStandaloneBrokerStartInfo(string serviceName, Version serviceVersion, int sessionId, bool durable, bool attached)
        {
            this.brokerInfo = new BrokerStartInfo();
            this.brokerInfo.Headnode = this.startInfo.Headnode;

            // Secure will be set to false and the following two settings are ignored
            this.brokerInfo.JobOwnerSID = null;
            this.brokerInfo.JobTemplateACL = null;
            this.brokerInfo.PersistVersion = BrokerVersion.DefaultPersistVersion;
            this.brokerInfo.SessionId = sessionId;
            this.brokerInfo.Attached = attached;
            this.brokerInfo.Durable = durable;
            this.brokerInfo.NetworkPrefix = Constant.EnterpriseNetwork;
            
            //rewrite the method building configuration file since regpath gotten from input.
            string regPath = this.startInfo.RegPath;
            ServiceRegistrationRepo serviceRegistration = new ServiceRegistrationRepo(regPath, null);
            this.brokerInfo.ConfigurationFile = serviceRegistration.GetServiceRegistrationPath(serviceName, serviceVersion);
        }



        /// <summary>
        /// Fetch service registration path
        /// </summary>
        /// <param name="serviceName">indicating the service name</param>
        /// <param name="serviceVersion">indicating the server version</param>
        /// <returns>returns tha path of the service registration file</returns>
        private async Task<string> FetchServiceRegistrationPath(string serviceName, Version serviceVersion)
        {
            string centralPath = null;

            if (this.isDebugModeEnabled)
            {
                // Fetch central path directly from environment if debug mode is enabled
                centralPath = Environment.GetEnvironmentVariable(Constant.RegistryPathEnv);
            }
            else
            {
                // Fetch central path from session launcher via GetSOAConfiguration on the other hand
                SessionLauncherClient client = new SessionLauncherClient(await Utility.GetSessionLauncherAsync(this.startInfo, this.binding).ConfigureAwait(false), this.binding, this.startInfo.IsAadOrLocalUser);
                try
                {
                    centralPath = await client.GetSOAConfigurationAsync(Constant.RegistryPathEnv).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[InprocBrokerAdapter] Failed to get service registration path via session launcher service: {0}", e);
                }
                finally
                {
                    Utility.SafeCloseCommunicateObject(client);
                }
            }

            if (centralPath == null)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.ServiceRegistrationPathEnvironmentMissing, SR.ServiceRegistrationPathEnvironmentMissing);
            }

            // setup the service registery helper
            ServiceRegistrationRepo serviceRegistration = new ServiceRegistrationRepo(centralPath, HpcContext.GetOrAdd(this.startInfo.Headnode, CancellationToken.None).GetServiceRegistrationRestClient());
            string serviceRegistrationPath = serviceRegistration.GetServiceRegistrationPath(serviceName, serviceVersion);
            if (serviceRegistrationPath == null)
            {
                throw new FileNotFoundException("Registration file is not found", serviceName);
            }
            return serviceRegistrationPath;
        }

        /// <summary>
        /// Determine whether to enable diag trace
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        /// <returns>returns a value indicating whether to enable diag trace</returns>
        private bool IsDiagTraceEnabled(int sessionId)
        {
            return this.brokerInfo.EnableDiagTrace;
        }

        /// <summary>
        /// Create broker
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        private async Task CreateInternal(int sessionId)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            SetIsDiagTraceEnabledProperty();
            Assembly brokerCoreAsm = Assembly.LoadFile(this.brokerCoreServiceLibPath);
            ConstructorInfo ci = brokerCoreAsm.GetType(BrokerEntryClassFullName).GetConstructor(new Type[1] { typeof(int) });
            this.brokerEntry = (IBrokerEntry)ci.Invoke(new object[1] { sessionId });
            this.brokerEntry.BrokerFinished += new EventHandler(this.Entry_BrokerFinished);
            this.result = this.brokerEntry.Run(this.startInfoContract, this.brokerInfo);

            //set isDebugModeEnabled True to jump UpdateBrokerInfo method.
            if (!this.isDebugModeEnabled && !this.isNoSession)
            {
                await UpdateBrokerInfo(this.brokerInfo).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Update the broker info
        /// </summary>
        /// <param name="info">broker info</param>
        private async Task UpdateBrokerInfo(BrokerStartInfo info)
        {
            int retry = 3;

            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add(BrokerSettingsConstants.BrokerNode, Constant.InprocessBrokerNode);
            properties.Add(BrokerSettingsConstants.Suspended, info.Durable);
            properties.Add(BrokerSettingsConstants.Durable, info.Durable);
            properties.Add(BrokerSettingsConstants.PersistVersion, info.PersistVersion);

            //SchedulerAdapterInternalClient client = new SchedulerAdapterInternalClient(await this.startInfo.ResolveHeadnodeMachineAsync().ConfigureAwait(false));
            ISchedulerAdapter client = SessionServiceContainer.SchedulerAdapterInstance;

            try
            {
                while (retry > 0)
                {
                    try
                    {
                        if (await client.UpdateBrokerInfoAsync(info.SessionId, properties).ConfigureAwait(false))
                        {
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[BrokerLauncher.SchedulerHelper] UpdateBrokerInfo failed: Exception = {0}", ex);
                    }

                    retry--;
                }
            }
            finally
            {
                var communicateObj = client as ICommunicationObject;
                if (communicateObj != null)
                {
                    Utility.SafeCloseCommunicateObject(communicateObj);
                }
            }

            throw new InvalidOperationException("Can not update the properties in the scheduler database for EPRs");
        }

        /// <summary>
        /// Resolve assembly load by broker core service lib
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="args">indicating the event args</param>
        /// <returns>returns loaded assembly</returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == BrokerBaseFullName)
            {
                return Assembly.LoadFile(this.brokerBaseFullPath);
            }
            /*
            else if (args.Name == ClusterModelFullName)
            {
                return Assembly.LoadFile(this.clusterModelFullPath);
            }
            else if (args.Name == MSMQInteropFullName)
            {
                return Assembly.LoadFile(this.MSMQInteropFullPath);
            }
            */
            else if (args.Name == BrokerCoreServiceLibFullName)
            {
                return Assembly.LoadFile(this.brokerCoreServiceLibPath);
            }
            else if (args.Name == SoaCommonUtilityLibFullName)
            {
                return Assembly.LoadFile(this.soaCommonUtilityLibPath);
            }
            else if (args.Name == RESTServiceModelLibFullName)
            {
                return Assembly.LoadFile(this.RESTServiceModelLibPath);
            }
            else if (args.Name == SoaAmbientConfigLibFullName)
            {
                return Assembly.LoadFile(this.SoaAmbientConfigLibPath);
            }

            return null;
        }

        /// <summary>
        /// Check if the broker has already initialized and throw exception if so
        /// </summary>
        private void ThrowIfInitialized()
        {
            if (this.brokerEntry != null)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_AlreadyInitialized, SR.Broker_AlreadyInitialized);
            }
        }

        /// <summary>
        /// Event triggered when broker finished
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event args</param>
        private void Entry_BrokerFinished(object sender, EventArgs e)
        {
            InprocessSessions.GetInstance().RemoveSessionInstance(this.brokerInfo.SessionId);
            this.brokerEntry = null;

            lock (this.frontendAdapters)
            {
                foreach (InprocessBrokerFrontendAdapter adapter in this.frontendAdapters)
                {
                    adapter.ShutdownConnection();
                }
            }
        }

        /// <summary>
        /// Broker frontend adapter for inprocess broker
        /// This adapter simulates connection shutdown by accepting a "ShutdownConnection" call.
        /// After this call returns, it will block all calls and returns heartbeat exception as broker is closed
        /// </summary>
        private class InprocessBrokerFrontendAdapter : IBrokerFrontend, IDisposable
        {
            /// <summary>
            /// Define a delegate that returns type T
            /// </summary>
            /// <typeparam name="T">indicating the return type</typeparam>
            /// <returns>returns a instance of type T</returns>
            private delegate T ReturnTDelegate<T>();

            /// <summary>
            /// Stores the broker entry reference
            /// </summary>
            private IBrokerEntry brokerEntry;

            /// <summary>
            /// Stores the callback instance
            /// </summary>
            private IResponseServiceCallback callbackInstance;

            /// <summary>
            /// Stores the IBrokerFrontend instance
            /// </summary>
            private volatile IBrokerFrontend frontend;

            /// <summary>
            /// Stores the close flag
            /// </summary>
            private bool closed;

            /// <summary>
            /// Initializes a new instance of the InprocessBrokerFrontendAdapter class
            /// </summary>
            /// <param name="brokerEntry">indicating the broker entry instance</param>
            /// <param name="callbackInstance">indicating the callback instance</param>
            public InprocessBrokerFrontendAdapter(IBrokerEntry brokerEntry, IResponseServiceCallback callbackInstance)
            {
                this.brokerEntry = brokerEntry;
                this.callbackInstance = callbackInstance;
                this.BuildFrontend();
            }

            /// <summary>
            /// Simluates that the connection is shutdown
            /// After this call, all call to the member of this instance will return heartbeat exception
            /// </summary>
            public void ShutdownConnection()
            {
                this.closed = true;
            }

            /// <summary>
            /// Dispose the object
            /// </summary>
            void IDisposable.Dispose()
            {
                ((IDisposable)this.frontend).Dispose();
            }

            #region IBrokerFrontend Members
            void IBrokerFrontend.SendRequest(System.ServiceModel.Channels.Message message)
            {
                this.ThrowIfClosed();
                this.RenewFrontendInstanceIfDisposed<object>(null, delegate ()
                {
                    this.frontend.SendRequest(message);
                    return null;
                });
            }

            void IController.Flush(int count, string clientId, int batchId, int timeoutThrottlingMs, int timeoutFlushMs)
            {
                this.ThrowIfClosed();
                this.RenewFrontendInstanceIfDisposed<object>(clientId, delegate ()
                {
                    this.frontend.Flush(count, clientId, batchId, timeoutThrottlingMs, timeoutFlushMs);
                    return null;
                });
            }

            void IController.EndRequests(int count, string clientId, int batchId, int timeoutThrottlingMs, int timeoutEOMMs)
            {
                this.ThrowIfClosed();
                this.RenewFrontendInstanceIfDisposed<object>(clientId, delegate ()
                {
                    this.frontend.EndRequests(count, clientId, batchId, timeoutThrottlingMs, timeoutEOMMs);
                    return null;
                });
            }

            void IController.Purge(string clientId)
            {
                this.ThrowIfClosed();
                this.RenewFrontendInstanceIfDisposed<object>(clientId, delegate ()
                {
                    this.frontend.Purge(clientId);
                    return null;
                });
            }

            void IController.Ping()
            {
                // ignore this
            }

            BrokerClientStatus IController.GetBrokerClientStatus(string clientId)
            {
                this.ThrowIfClosed();
                return this.RenewFrontendInstanceIfDisposed<BrokerClientStatus>(clientId, delegate ()
                {
                    return this.frontend.GetBrokerClientStatus(clientId);
                });
            }

            int IController.GetRequestsCount(string clientId)
            {
                this.ThrowIfClosed();
                return this.RenewFrontendInstanceIfDisposed<int>(clientId, delegate ()
                {
                    return this.frontend.GetRequestsCount(clientId);
                });
            }

            BrokerResponseMessages IController.PullResponses(string action, GetResponsePosition position, int count, string clientId)
            {
                this.ThrowIfClosed();
                return this.RenewFrontendInstanceIfDisposed<BrokerResponseMessages>(clientId, delegate ()
                {
                    return this.frontend.PullResponses(action, position, count, clientId);
                });
            }

            void IResponseService.GetResponses(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId)
            {
                this.ThrowIfClosed();
                this.RenewFrontendInstanceIfDisposed<object>(clientId, delegate ()
                {
                    this.frontend.GetResponses(action, clientData, resetToBegin, count, clientId);
                    return null;
                });
            }

            bool IBrokerFrontend.EndRequestReceived(string clientId)
            {
                return this.frontend.EndRequestReceived(clientId);
            }


            #endregion

            /// <summary>
            /// Throw heartbeat exception if closed flag is true
            /// </summary>
            private void ThrowIfClosed()
            {
                if (this.closed)
                {
                    throw SessionBase.GetHeartbeatException(false);
                }
            }

            /// <summary>
            /// Calls the operation and renew frontend instance if the original one has been disposed
            /// </summary>
            /// <param name="operation">indicating the operation</param>
            private T RenewFrontendInstanceIfDisposed<T>(string clientId, ReturnTDelegate<T> operation)
            {
                try
                {
                    return (T)operation.Invoke();
                }
                catch (FaultException<SessionFault> e)
                {
                    if (e.Detail.Code == SOAFaultCode.ClientTimeout)
                    {
                        // Bug 10445: Only retry if the client has not received EndRequests() call
                        if (clientId == null || !this.frontend.EndRequestReceived(clientId))
                        {
                            this.BuildFrontend();
                            return (T)operation.Invoke();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            /// <summary>
            /// Build frontend instance
            /// </summary>
            private void BuildFrontend()
            {
                this.frontend = this.brokerEntry.GetFrontendForInprocessBroker(this.callbackInstance);
            }

            // not implemented
            public void GetResponsesAQ(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId, int sessionHash, out string azureResponseQueueUri, out string azureResponseBlobUri)
            {
                throw new NotImplementedException();
            }
        }
    }
}
