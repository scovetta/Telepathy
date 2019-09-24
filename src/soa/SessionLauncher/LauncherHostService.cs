// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.LauncherHostService
{
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceProcess;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Common.TelepathyContext;
    using Microsoft.Telepathy.Common.TelepathyContext.Extensions.RegistryExtension;
    using Microsoft.Telepathy.Internal.SessionLauncher;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SchedulerDelegations.AzureBatch;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SchedulerDelegations.Local;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers;
    using Microsoft.Telepathy.Internal.SessionLauncher.Utils;
#if HPCPACK
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.HpcPack;
#endif
    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Internal;

    using ISessionLauncher = Microsoft.Telepathy.Internal.SessionLauncher.ISessionLauncher;

    // TODO: Consider changing the if/switch branching for schedulers into sub-classes
    /// <summary>
    /// Launcher Host Service
    /// </summary>
    partial class LauncherHostService : ServiceBase
    {
        /// <summary>
        /// Store the broker launcher host
        /// </summary>
        private ServiceHost launcherHost;

        /// <summary>
        /// Store the broker launcher host
        /// </summary>
        private ServiceHost delegationHost;

        /// <summary>
        /// Store the data service host
        /// </summary>
        private ServiceHost dataServiceHost;

        /// <summary>
        /// Store the session launcher instance
        /// </summary>
        private SessionLauncher sessionLauncher;

        /// <summary>
        /// Store the broker node manager
        /// </summary>
        private BrokerNodesManager brokerNodesManager;

        /// <summary>
        /// Stores the scheduler delegation service instance
        /// </summary>
        private ISchedulerAdapter schedulerDelegation;

#if HPCPACK
        /// <summary>
        /// Store the data service instance
        /// </summary>
        private Microsoft.Hpc.Scheduler.Session.Data.Internal.DataService dataService;

        /// <summary>
        /// REST Server of Data Service
        /// </summary>
        private DataServiceRestServer dataServiceRestServer;
#endif

        /// <summary>
        /// Initializes a new instance of the LauncherHostService class
        /// </summary>
        public LauncherHostService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the LauncherHostService class
        /// This will be called when run in debug mode
        /// </summary>
        /// <param name="launcherChoose">launcher choose</param>
        //internal LauncherHostService(bool console)
        //{
        //    InitializeComponent();

        //    this.OpenService();
        //}

        public void StopService(bool isNtService = false)
        {
            try
            {
                // DataService instance is created by session launcher. To make sure data service exit gracefully, close DataService before closing session launcher.
                if (this.dataServiceHost != null)
                {
                    this.dataServiceHost.Faulted -= DataServiceHostFaultHandler;
                    this.dataServiceHost.Close();
                    this.dataServiceHost = null;

                    TraceHelper.TraceEvent(TraceEventType.Verbose, "SOA data service endpoint closed");
                }

                if (this.launcherHost != null)
                {
                    this.launcherHost.Close();
                }
                this.launcherHost = null;
                TraceHelper.TraceEvent(TraceEventType.Verbose, "Session launcher endpoint closed");

                this.delegationHost?.Close();
                this.delegationHost = null;
                TraceHelper.TraceEvent(TraceEventType.Verbose, "Scheduler delegation service closed");

#if HPCPACK
                // session launcher host has been closed, remember to close the data service instance
                if (this.dataService != null)
                {
                    this.dataService.Close();
                    this.dataService = null;
                    TraceHelper.TraceEvent(TraceEventType.Verbose, "Data service instance closed");
                }
#endif

                if (!isNtService)
                {
                    // only need to get cleaned in SF serivce
                    if (this.schedulerDelegation is IDisposable disposable)
                    {
                        disposable.Dispose();
                        TraceHelper.TraceEvent(TraceEventType.Verbose, "Scheduler delegation closed");
                    }
                    this.schedulerDelegation = null;

                    if (this.sessionLauncher != null)
                    {
                        this.sessionLauncher.Close();
                        TraceHelper.TraceEvent(TraceEventType.Verbose, "Session launcher closed");
                    }
                    this.sessionLauncher = null;

                    TraceHelper.IsDiagTraceEnabled = null;
                    SoaDiagTraceHelper.IsDiagTraceEnabledInternal = null;

                    if (this.brokerNodesManager is IDisposable d)
                    {
                       d.Dispose();
                    }
                    this.brokerNodesManager = null;
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "Failed to close the service host - {0}", e);
            }
        }

        /// <summary>
        /// Open the session launcher service
        /// </summary>
        public async Task OpenService()
        {
            try
            {
                // If running on Azure, monitor HAController service and terminate this service
                //  if HAController dies. Note that SCM service dependency monitoring does not provide
                //  this
                if (SoaHelper.IsOnAzure())
                {
                    ServiceControllerHelpers.MonitorHAControllerStopAsync(HpcServiceNames.HpcSession);
                    TraceHelper.TraceEvent(TraceEventType.Information, "Azure HAController service monitoring enabled");
                }

                if (SessionLauncherRuntimeConfiguration.SchedulerType == SchedulerType.HpcPack)
                {
#if HPCPACK
                    this.brokerNodesManager = new BrokerNodesManager();
                    this.sessionLauncher = SessionLauncherFactory.CreateHpcPackSessionLauncher(SoaHelper.GetSchedulerName(), false, this.brokerNodesManager);
                    this.schedulerDelegation = new HpcSchedulerDelegation(this.sessionLauncher, this.brokerNodesManager);
#endif
                }
                else if (SessionLauncherRuntimeConfiguration.SchedulerType == SchedulerType.AzureBatch)
                {
                    var instance = SessionLauncherFactory.CreateAzureBatchSessionLauncher();
                    this.sessionLauncher = instance;
                    this.schedulerDelegation = new AzureBatchSchedulerDelegation(instance);

                }
                else if (SessionLauncherRuntimeConfiguration.SchedulerType == SchedulerType.Local)
                {
                    var instance = SessionLauncherFactory.CreateLocalSessionLauncher();
                    this.sessionLauncher = instance;
                    this.schedulerDelegation = new LocalSchedulerDelegation(instance);
                }

                TraceHelper.IsDiagTraceEnabled = _ => true;
#if HPCPACK

                // Bug 18448: Need to enable traces only for those who have enabled trace
                if (this.schedulerDelegation is IHpcSchedulerAdapterInternal hpcAdapterInternal)
                {
                    SoaDiagTraceHelper.IsDiagTraceEnabledInternal = hpcAdapterInternal.IsDiagTraceEnabled;
                    TraceHelper.IsDiagTraceEnabled = SoaDiagTraceHelper.IsDiagTraceEnabled;
                }
#endif

                // start session launcher service
                this.StartSessionLauncherService();

                if (SessionLauncherRuntimeConfiguration.SchedulerType == SchedulerType.HpcPack 
                    || SessionLauncherRuntimeConfiguration.SchedulerType == SchedulerType.Local
                    || SessionLauncherRuntimeConfiguration.SchedulerType == SchedulerType.AzureBatch)
                {
                    // start scheduler delegation service
                    this.StartSchedulerDelegationService();
                }

#if HPCPACK
                // start data service
                if (!SoaHelper.IsOnAzure() && this.sessionLauncher is HpcPackSessionLauncher hpcSessionLauncher)
                {
                    this.dataService = hpcSessionLauncher.GetDataService();
                    this.StartDataWcfService();
                    this.StartDataRestService(this.dataService);
                }
#endif
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "Failed to open the service host - {0}", e);
                throw;
            }

            await Task.CompletedTask;
        }

#if HPCPACK

        private void StartDataRestService(DataService serviceInstance)
        {
            var server = new DataServiceRestServer(serviceInstance);
            server.Start();
            this.dataServiceRestServer = server;
        }
#endif

        /// <summary>
        /// Start session launcher service
        /// </summary>
        private void StartSessionLauncherService()
        {
            try
            {
                string sessionLauncherAddress = SoaHelper.GetSessionLauncherAddress("localhost");
                this.launcherHost = new ServiceHost(this.sessionLauncher, new Uri(sessionLauncherAddress));
                BindingHelper.ApplyDefaultThrottlingBehavior(this.launcherHost);

#if AZURE_STORAGE_BINDING
                if (SessionLauncherRuntimeConfiguration.OpenAzureStorageListener)
                {
                    this.launcherHost.AddServiceEndpoint(
                        typeof(ISessionLauncher),
                        new TableTransportBinding() { ConnectionString = SessionLauncherRuntimeConfiguration.SessionLauncherStorageConnectionString, TargetPartitionKey = "all" },
                        TelepathyConstants.SessionLauncherAzureTableBindingAddress);
                    TraceHelper.TraceEvent(TraceEventType.Information, "Add session launcher service endpoint {0}", TelepathyConstants.SessionLauncherAzureTableBindingAddress);
                }
#endif

                if (SessionLauncherRuntimeConfiguration.SchedulerType == SchedulerType.HpcPack)
                {
                    this.launcherHost.AddServiceEndpoint(typeof(ISessionLauncher), BindingHelper.HardCodedSessionLauncherNetTcpBinding, string.Empty);
                    this.launcherHost.AddServiceEndpoint(typeof(ISessionLauncher), BindingHelper.HardCodedNoAuthSessionLauncherNetTcpBinding, "AAD");
                    this.launcherHost.AddServiceEndpoint(typeof(ISessionLauncher), BindingHelper.HardCodedInternalSessionLauncherNetTcpBinding, "Internal");

                    TraceHelper.TraceEvent(TraceEventType.Information, "Open session launcher find cert {0}", TelepathyContext.Get().GetSSLThumbprint().GetAwaiter().GetResult());
                    this.launcherHost.Credentials.UseInternalAuthenticationAsync().GetAwaiter().GetResult();

                    TraceHelper.TraceEvent(TraceEventType.Information, "Add session launcher service endpoint {0}", sessionLauncherAddress);
                }
                else
                {
                    this.launcherHost.AddServiceEndpoint(typeof(ISessionLauncher), BindingHelper.HardCodedUnSecureNetTcpBinding, string.Empty);
                    this.launcherHost.AddServiceEndpoint(typeof(ISessionLauncher), BindingHelper.HardCodedUnSecureNetTcpBinding, "Internal");

                    TraceHelper.TraceEvent(TraceEventType.Information, "Add session launcher service endpoint {0}", sessionLauncherAddress);
                }

                this.launcherHost.Faulted += this.SessionLauncherHostFaultHandler;
                ServiceAuthorizationBehavior myServiceBehavior =
                    this.launcherHost.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
                myServiceBehavior.PrincipalPermissionMode = PrincipalPermissionMode.None;
                this.launcherHost.Open();
                TraceHelper.TraceEvent(TraceEventType.Information, "Open session launcher service");
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Faulted event handler for session launcher host
        /// </summary>
        private void SessionLauncherHostFaultHandler(object sender, EventArgs e)
        {
            TraceHelper.TraceEvent(TraceEventType.Error, "Session launcher service host goes into Faulted state.  Restart it.");

            // abort the Faulted service host
            try
            {
                this.launcherHost.Faulted -= this.SessionLauncherHostFaultHandler;
                this.launcherHost.Abort();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "Exception encountered while aborting session launcher service host: {0}", ex);
            }

            // and create/restart a new one
            try
            {
                this.StartSessionLauncherService();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "Exception encountered while restarting session launcher service: {0}", ex);
            }
        }

        /// <summary>
        /// Start scheduler delegation service
        /// </summary>
        private void StartSchedulerDelegationService()
        {
            string schedulerDelegationAddress = SoaHelper.GetSchedulerDelegationAddress("localhost");
            this.delegationHost = new ServiceHost(this.schedulerDelegation, new Uri(schedulerDelegationAddress));
            BindingHelper.ApplyDefaultThrottlingBehavior(this.delegationHost);
#if HPCPACK
            if (this.schedulerDelegation is IHpcSchedulerAdapterInternal)
            {
                this.delegationHost.AddServiceEndpoint(typeof(IHpcSchedulerAdapterInternal), BindingHelper.HardCodedInternalSchedulerDelegationBinding, "Internal");
                this.delegationHost.AddServiceEndpoint(typeof(IHpcSchedulerAdapter), BindingHelper.HardCodedInternalSchedulerDelegationBinding, string.Empty);
                this.delegationHost.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
                this.delegationHost.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
                this.delegationHost.Credentials.ServiceCertificate.SetCertificate(
                    StoreLocation.LocalMachine,
                    StoreName.My,
                    X509FindType.FindByThumbprint,
                    TelepathyContext.Get().GetSSLThumbprint().GetAwaiter().GetResult());
            }
            else
#endif
            {
                // Use insecure binding until unified authentication logic is implemented
                this.delegationHost.AddServiceEndpoint(typeof(ISchedulerAdapter), BindingHelper.HardCodedUnSecureNetTcpBinding, string.Empty);
                // if (SessionLauncherRuntimeConfiguration.OpenAzureStorageListener)
                // {
                //     this.delegationHost.AddServiceEndpoint(
                //         typeof(ISchedulerAdapter),
                //         new TableTransportBinding() { ConnectionString = SessionLauncherRuntimeConfiguration.SessionLauncherStorageConnectionString, TargetPartitionKey = "all" },
                //         TelepathyConstants.SessionSchedulerDelegationAzureTableBindingAddress);
                // }
            }

            this.delegationHost.Faulted += SchedulerDelegationHostFaultHandler;
            this.delegationHost.Open();
            TraceHelper.TraceEvent(TraceEventType.Information, "Open scheduler delegation service at {0}", schedulerDelegationAddress);
        }

        /// <summary>
        /// Faulted event handler for scheduler delegation host
        /// </summary>
        private void SchedulerDelegationHostFaultHandler(object sender, EventArgs e)
        {
            TraceHelper.TraceEvent(TraceEventType.Error, "Scheduler delegation service host goes into Faulted state.  Restart it.");

            // abort the Faulted service host
            try
            {
                this.delegationHost.Faulted -= SchedulerDelegationHostFaultHandler;
                this.delegationHost.Abort();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "Exception encountered while aborting scheduler delegation service host: {0}", ex);
            }

            // and create/restart a new one
            try
            {
                this.StartSchedulerDelegationService();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "Exception encountered while restarting scheduler delegation service: {0}", ex);
            }
        }

#if HPCPACK

        /// <summary>
        /// Start data service
        /// </summary>
        private void StartDataWcfService()
        {
            string dataServiceAddress = SoaHelper.GetDataServiceAddress();
            this.dataServiceHost = new ServiceHost(this.dataService, new Uri(dataServiceAddress));
            BindingHelper.ApplyDefaultThrottlingBehavior(this.dataServiceHost);

            Binding binding = BindingHelper.HardCodedDataServiceNetTcpBinding;
            binding.SendTimeout = TimeSpan.FromMinutes(Microsoft.Hpc.Scheduler.Session.Data.Internal.Constant.DataProxyOperationTimeoutInMinutes);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(Microsoft.Hpc.Scheduler.Session.Data.Internal.Constant.DataProxyOperationTimeoutInMinutes);
            this.dataServiceHost.AddServiceEndpoint(typeof(Microsoft.Hpc.Scheduler.Session.Data.Internal.IDataService), binding, string.Empty);

            // Bug 14900: for some unknown reason, service host may enter into Faulted state and
            // become unresponsive to client requests. Here register the Faulted event. If it 
            // occurs, restart the service host.
            this.dataServiceHost.Faulted += DataServiceHostFaultHandler;
            this.dataServiceHost.Open();

            TraceHelper.TraceEvent(TraceEventType.Information, "Open SOA data service at {0}", dataServiceAddress);
        }
#endif

        /// <summary>
        /// Faulted event handler for dataServiceHost
        /// </summary>
        private void DataServiceHostFaultHandler(object sender, EventArgs e)
        {
            TraceHelper.TraceEvent(TraceEventType.Error, "Data service host goes into Faulted state.  Restart it.");

            // abort the Faulted service host
            try
            {
                this.dataServiceHost.Faulted -= DataServiceHostFaultHandler;
                this.dataServiceHost.Abort();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "Exception encountered while aborting data service host: {0}", ex);
            }

#if HPCPACK
            // and create/restart a new one
            try
            {
                this.StartDataWcfService();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "Exception encountered while restarting data service: {0}", ex);
            }

#endif
        }

        protected override void OnStart(string[] args)
        {
            this.OpenService().Wait();
        }

        protected override void OnStop()
        {
            this.StopService(true);
        }
    }
}
