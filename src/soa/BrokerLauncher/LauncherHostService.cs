//------------------------------------------------------------------------------
// <copyright file="LauncherHostService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Windows service for launcher host
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal.LauncherHostService
{
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher;
    using Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Hpc.ServiceBroker.Common;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceProcess;
    using System.Threading;

    using System.ServiceModel.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Description;

    using Microsoft.Hpc.AADAuthUtil;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter;

    /// <summary>
    /// Launcher Host Service
    /// </summary>
    partial class LauncherHostService : ServiceBase
    {
        /// <summary>
        /// Single process lock
        /// </summary>
        internal Mutex singleProcessLock;

        /// <summary>
        /// Set the offline mode to force
        /// </summary>
        private const int SetForceOfflineMode = 130;

        /// <summary>
        /// Set the offline mode to force
        /// </summary>
        private const int SetGracefulOfflineMode = 131;

        /// <summary>
        /// Cancel offline operation
        /// </summary>
        private const int CancelOffline = 132;

        /// <summary>
        /// enable port sharing service for all node
        /// </summary>
        internal const int SCMUserDefinedEnablePortSharingService = 249;

        /// <summary>
        /// Time interval requested when pause needs more time
        /// </summary>
        private const int pauseServiceWaitInterval = 3000;

        /// <summary>
        /// Time interval to check if exitForceMode is set when pasuing the service
        /// </summary>
        private const int exitModeWaitInterval = 1000;

        /// <summary>
        /// Maximum retry count to ensure that exitForceMode is set before pausing the service
        /// </summary>
        private const int exitModeRetryCount = 10;

        /// <summary>
        /// Retry period for SOA diag service and Azure storage cleanup service in ms
        /// </summary>
        private const int RetryPeriod = 60000;

        /// <summary>
        /// The max concurrent calls for diag svc.
        /// </summary>
        private const int SoaDiagSvcMaxConcurrentCalls = 32;

        /// <summary>
        /// Stores whether the service is running in debug mode
        /// </summary>
        private static bool isConsoleApplication = false;

        /// <summary>
        /// Store host name
        /// The static field variable initializers of a class correspond to a sequence of assignments
        /// that are executed in the textual order in which they appear in the class declaration.
        /// So this field's declaration needs to be prior to BrokerLauncherEpr and BrokerLauncherHttpsEpr below.
        /// </summary>
        private static readonly string HostName = Dns.GetHostName();

        /// <summary>
        /// Stores broker launcher epr for http binding
        /// </summary>
        private static readonly string BrokerLauncherHttpsEpr = string.Format("https://{0}:443/BrokerLauncher", HostName);

        /// <summary>
        /// Stores node mapping cache uri
        /// </summary>
        private static readonly string NodeMappingCacheEpr = @"net.pipe://localhost/NodeMapping";

        /// <summary>
        /// Store the broker launcher host
        /// </summary>
        private ServiceHost launcherHost;

        /// <summary>
        /// Server side cloud queue adapter
        /// </summary>
        private BrokerLauncherCloudQueueWatcher watcher;

        /// <summary>
        /// Stores the soa diag service host
        /// </summary>
        private ServiceHost diagServiceHost;

#if HPCPACK
        /// <summary>
        /// Stores the soa diag cleanup service.
        /// </summary>
        private DiagCleanupService cleanupService;
#endif

        /// <summary>
        /// Store the NodeMappingCache host
        /// </summary>
        private ServiceHost nodeMappingCacheHost;

        /// <summary>
        /// The stop mode
        /// </summary>
        private bool? exitForceMode;

        /// <summary>
        /// the service instance
        /// </summary>
        private BrokerLauncher launcherInstance;

        /// <summary>
        /// Stores the instance of the soa diag authenticator
        /// </summary>
        private SoaDiagAuthenticator soaDiagAuthenticator;

        /// <summary>
        /// flag to cancel a offline operation
        /// </summary>
        private bool bCancelOffline;

        /// <summary>
        /// Azure storage cleaner
        /// </summary>
        private AzureStorageCleaner azureStorageCleaner;

        /// <summary>
        /// Stores the service fabric context;
        /// </summary>
        private IHpcContext context;

        /// <summary>
        /// Initializes a new instance of the LauncherHostService class
        /// </summary>
        public LauncherHostService(IHpcContext context)
        {
            Trace.TraceInformation("LauncherHostService init.");
            this.context = context;
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the LauncherHostService class
        /// This will be called when run in debug mode
        /// </summary>
        /// <param name="launcherChoose">launcher choose</param>
        internal LauncherHostService(bool console, IHpcContext context)
        {
            this.context = context;
            isConsoleApplication = console;
            InitializeComponent();

            this.OpenService();
        }

        /// <summary>
        /// Start the service
        /// </summary>
        /// <param name="args">environment args</param>
        protected override void OnStart(string[] args)
        {
            this.OpenService();
        }

        /// <summary>
        /// Stop the service
        /// </summary>
        protected override void OnStop()
        {
            this.StopService();
        }

        public void StopService()
        {
            try
            {
                this.launcherInstance.Close();
                this.launcherHost.Close();

                this.CloseSoaDiagService();

                if (this.nodeMappingCacheHost != null)
                {
                    this.nodeMappingCacheHost.Close();
                    this.nodeMappingCacheHost = null;
                }

                if (this.azureStorageCleaner != null)
                {
                    this.azureStorageCleaner.Close();
                }

                this.azureStorageCleaner = null;

            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "Failed to close the service host - {0}", e);
            }

            if (isConsoleApplication)
            {
                try
                {
                    ReleaseSingleProcessLock();
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "Failed to release singleton service lock - {0}", e);
                }
            }
        }

        /// <summary>
        /// Move the serivce from offline to online state
        /// </summary>
        protected override void OnContinue()
        {
            // goes to online

            // Bring broker online
            this.launcherInstance.Online();

            base.OnContinue();

            TraceHelper.TraceEvent(TraceEventType.Information, "Broker is online");
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnPause()
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "Get Pause Request");

            // fix bug 16048: wait for (exitModeWaitInterval * exitModeRetryCount) period to ensure that exitForceMode is set before pausing the service
            int retryCount = 0;
            while (!this.exitForceMode.HasValue)
            {
                if (retryCount++ >= exitModeRetryCount)
                {
                    throw new InvalidOperationException();
                }
                RequestAdditionalTime(exitModeWaitInterval);
            }

            // SCM should have sent a command specified whether this is forced or not
            bool force = exitForceMode.Value;

            // wait for the broker entry finish
            this.exitForceMode = null;

            // initiate offline
            EventWaitHandle handle = this.launcherInstance.StartOffline(force);
            this.bCancelOffline = false;

            TraceHelper.TraceEvent(TraceEventType.Verbose, "Start draining");

            // TODO: Timeout?
            do
            {
                RequestAdditionalTime(pauseServiceWaitInterval);
            } while (!handle.WaitOne(pauseServiceWaitInterval, false) && !this.bCancelOffline);

            // If this wasnt a force offline and cancel is enabled
            if (!force && this.bCancelOffline)
            {
                TraceHelper.TraceEvent(TraceEventType.Verbose, "Offline is canceled");
                this.launcherInstance.Online();
                throw new OperationCanceledException();
            }
            else if (this.launcherInstance.IsOnline)
            {
                throw new Exception("Offline operation failed");
            }

            base.OnPause();

            TraceHelper.TraceEvent(TraceEventType.Information, "Broker is offline");
        }

        /// <summary>
        /// Handle the customized command to set the exit mode
        /// </summary>
        /// <param name="command">Exit Mode Command</param>
        protected override void OnCustomCommand(int command)
        {
            switch (command)
            {
                case SetForceOfflineMode:
                    this.exitForceMode = true;
                    break;
                case SetGracefulOfflineMode:
                    this.exitForceMode = false;
                    break;
                case CancelOffline:
                    this.bCancelOffline = true;
                    break;
                case SCMUserDefinedEnablePortSharingService:
                    // BN need to enable the portsharing service
                    NetTcpPortSharingService.SetRequiredConfigurationForApplicationIntegration();
                    break;
                default:
                    base.OnCustomCommand(command);
                    break;
            }
        }

        /// <summary>
        /// Open the broker launcher service
        /// </summary>
        public void OpenService()
        {
            Trace.TraceInformation("Open service.");
            //TODO: SF: remove the singleton implementation
            //SingletonRegistry.Initialize(SingletonRegistry.RegistryMode.WindowsNonHA);

            // for debug attach
            //Thread.Sleep(60 * 1000);
            bool isOnAzure = SoaHelper.IsOnAzure();

            if (isOnAzure)
            {
                this.StartNodeMappingCacheService();
            }

            try
            {
                // richci: if this is a console application we are running in MSCS in production. Make 
                // sure only one instance of the console app is running at a time.
                if (!isOnAzure && IsConsoleApplication && !AcquireSingleProcessLock())
                {
                    // If another instance already created the mutex, release this handle
                    ReleaseSingleProcessLock();
                    throw new InvalidOperationException("Only one instance of the process can be run a time");
                }

                if (false) //!isOnAzure && !IsConsoleApplication && Win32API.IsFailoverBrokerNode())
                {
                    //
                    // If this is a brokerlauncher service running as service on a failover BN, dont
                    // open WCF endpoints. In this configuration, the broker launcher windows service is
                    // for mgmt operations only. All application traffic will go through brokerlaunchers
                    // running as console apps in MSCS resource groups
                    //
                    // Otherwise this a HpcBroker windows service on FO BN handling mgmt operations only
                    //

                    this.launcherInstance = new BrokerLauncher(true, this.context);
                }
                else
                {
                    Trace.TraceInformation("Open broker launcher service host.");
                    this.launcherInstance = new BrokerLauncher(false, this.context);
                    this.launcherHost = new ServiceHost(this.launcherInstance, new Uri(SoaHelper.GetBrokerLauncherAddress(HostName)));
                    BindingHelper.ApplyDefaultThrottlingBehavior(this.launcherHost);
                    this.launcherHost.AddServiceEndpoint(typeof(IBrokerLauncher), BindingHelper.HardCodedUnSecureNetTcpBinding, string.Empty);
                    this.launcherHost.AddServiceEndpoint(typeof(IBrokerLauncher), BindingHelper.HardCodedUnSecureNetTcpBinding, "Internal");
                    this.launcherHost.AddServiceEndpoint(typeof(IBrokerLauncher), BindingHelper.HardCodedUnSecureNetTcpBinding, "AAD");
                    // this.launcherHost.Credentials.UseInternalAuthenticationAsync(true).GetAwaiter().GetResult();
                    string addFormat = SoaHelper.BrokerLauncherAadAddressFormat;
                    this.launcherHost.Authorization.ServiceAuthorizationManager = new AADServiceAuthorizationManager(addFormat.Substring(addFormat.IndexOf('/')), this.context);
                    ServiceAuthorizationBehavior myServiceBehavior = this.launcherHost.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
                    myServiceBehavior.PrincipalPermissionMode = PrincipalPermissionMode.None;
                    this.launcherHost.Open();

                    if (BrokerLauncherSettings.Default.EnableAzureStorageQueueEndpoint)
                    {
                        if (string.IsNullOrEmpty(BrokerLauncherSettings.Default.AzureStorageConnectionString))
                        {
                            Trace.TraceError("AzureStorageConnectionString is null or empty while EnableAzureStorageQueueEndpoint is set to true");
                        }
                        else
                        {
                            this.watcher = new BrokerLauncherCloudQueueWatcher(this.launcherInstance, BrokerLauncherSettings.Default.AzureStorageConnectionString);
                        }
                    }

                    Trace.TraceInformation("Open broker launcher service succeeded.");
                    TraceHelper.TraceEvent(TraceEventType.Information, "Open broker launcher service succeeded.");

                    if (SoaHelper.IsSchedulerOnAzure())
                    {
                        // Broker service is enabled on scheduler node for on-premise and scheduler on Azure cluster.
                        // SoaDiagSvc is not expected to run on the Azure cluster.
                        return;
                    }

                    ISchedulerHelper helper = SchedulerHelperFactory.GetSchedulerHelper(this.context);
#if HPCPACK
                    ThreadPool.QueueUserWorkItem(
                        (object state) =>
                            {
                                try
                                {
                                    RetryHelper<object>.InvokeOperation(
                                        () =>
                                            {
                                                this.soaDiagAuthenticator = new SoaDiagAuthenticator();
                                                SoaDiagService diagServiceInstance = new SoaDiagService(helper.GetClusterInfoAsync, this.soaDiagAuthenticator);
                                                this.diagServiceHost = new ServiceHost(
                                                    diagServiceInstance,
#if DEBUG
                                                    new Uri("http://localhost/SoaDiagService"),
#endif
                                                    new Uri(SoaHelper.GetDiagServiceAddress(HostName)));

                                                BindingHelper.ApplyDefaultThrottlingBehavior(this.diagServiceHost, SoaDiagSvcMaxConcurrentCalls);
                                                var endpoint = this.diagServiceHost.AddServiceEndpoint(typeof(ISoaDiagService), BindingHelper.HardCodedDiagServiceNetTcpBinding, string.Empty);
                                                endpoint.Behaviors.Add(new SoaDiagServiceErrorHandler());
#if DEBUG
                                                var httpEndpoint = this.diagServiceHost.AddServiceEndpoint(typeof(ISoaDiagService), new BasicHttpBinding(), string.Empty);
                                                httpEndpoint.Behaviors.Add(new SoaDiagServiceErrorHandler());
#endif
                                                this.diagServiceHost.Open();
                                                TraceHelper.TraceEvent(TraceEventType.Information, "Open soa diag service succeeded.");

                                                this.cleanupService = new DiagCleanupService(helper.GetClusterInfoAsync);
                                                this.cleanupService.Start();
                                                TraceHelper.TraceEvent(TraceEventType.Information, "Open soa diag cleanup service succeeded.");
                                                return null;
                                            },
                                        (ex, count) =>
                                            {
                                                TraceHelper.TraceEvent(TraceEventType.Error, "Failed to open soa diag service: {0}. Retry Count = {1}", ex, count);
                                                this.CloseSoaDiagService();
                                                Thread.Sleep(RetryPeriod);
                                            });
                                }
                                catch (Exception e)
                                {
                                    TraceHelper.TraceEvent(TraceEventType.Error, "Failed to open soa diag service after all retry: {0}", e);
                                }
                            });
#endif

                    ThreadPool.QueueUserWorkItem((object state) =>
                    {
                        try
                        {
                            RetryHelper<object>.InvokeOperation(
                                () =>
                                {
                                    this.azureStorageCleaner = new AzureStorageCleaner(helper);

                                    this.azureStorageCleaner.Start();

                                    TraceHelper.TraceEvent(TraceEventType.Information,
                                        "Open Azure storage cleanup service succeeded.");

                                    return null;
                                },
                                (ex, count) =>
                                {
                                    TraceHelper.TraceEvent(
                                        TraceEventType.Error,
                                        "Failed to open Azure storage cleanup service: {0}. Retry Count = {1}",
                                        ex,
                                        count);

                                    if (this.azureStorageCleaner != null)
                                    {
                                        this.azureStorageCleaner.Close();
                                    }

                                    Thread.Sleep(RetryPeriod);
                                });
                        }
                        catch (Exception e)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Error,
                                "Failed to open Azure storage cleanup service after all retry: {0}", e);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "Failed to open service: {0}", e);
                throw;
            }
        }

        /// <summary>
        /// Close soa diag service related components
        /// </summary>
        private void CloseSoaDiagService()
        {
            if (this.diagServiceHost != null)
            {
                this.diagServiceHost.Close();
                this.diagServiceHost = null;
            }

            if (this.soaDiagAuthenticator != null)
            {
                this.soaDiagAuthenticator.Close();
                this.soaDiagAuthenticator = null;
            }
#if HPCPACK
            if (this.cleanupService != null)
            {
                this.cleanupService.Close();
                this.cleanupService = null;
            }
#endif
        }

        /// <summary>
        /// Open a ServiceHost for NodeMappingCache service
        /// </summary>
        private void StartNodeMappingCacheService()
        {
            try
            {
                this.nodeMappingCacheHost = new ServiceHost(typeof(NodeMappingCache), new Uri(NodeMappingCacheEpr));
                BindingHelper.ApplyDefaultThrottlingBehavior(this.nodeMappingCacheHost);
                this.nodeMappingCacheHost.AddServiceEndpoint(typeof(INodeMappingCache), BindingHelper.HardCodedNamedPipeBinding, string.Empty);
                this.nodeMappingCacheHost.Open();
                TraceHelper.TraceEvent(TraceEventType.Information, "[LauncherHostService] .StartNodeMappingCacheService: open NodeMappingCache service succeeded");
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "[LauncherHostService] .StartNodeMappingCacheService: Failed to start node mapping cache service. Exception = {0}", e);
                throw;
            }
        }

        /// Acquires lock that ensures only one instance of console app is run at a time
        /// </summary>
        /// <returns></returns>
        private bool AcquireSingleProcessLock()
        {
            Debug.Assert(singleProcessLock == null);

            const string brokerLauncherSingletonLockName = "brokerLauncherSingletonLockName_{D2CB4004-744E-4021-A47A-B8B566CA4E1D}";
            bool createdNew = false;
            singleProcessLock = new Mutex(true, brokerLauncherSingletonLockName, out createdNew);
            return createdNew;
        }

        /// <summary>
        /// Releases lock that ensures only one instance of console app is run at a time
        /// </summary>
        private void ReleaseSingleProcessLock()
        {
            if (singleProcessLock != null)
            {
                singleProcessLock.Close();
                singleProcessLock = null;
            }
        }

        /// <summary>
        /// Return the service instance for internal use
        /// </summary>
        internal BrokerLauncher BrokerLauncher
        {
            get
            {
                return this.launcherInstance;
            }
        }

        /// <summary>
        /// Returns whether HpcBroker is running as a console application
        /// </summary>
        internal static bool IsConsoleApplication
        {
            get
            {
                return LauncherHostService.isConsoleApplication;
            }
        }

        /// <summary>
        /// Provides a standalone authenticator for soa diag service
        /// </summary>
        private class SoaDiagAuthenticator : DisposableObject, ISessionUserAuthenticator
        {
            /// <summary>
            /// Stores the head node
            /// </summary>
            private string headNode;

            /// <summary>
            /// Stores the scheduler helper
            /// </summary>
            //private SchedulerHelper schedulerHelper;

            /// <summary>
            /// Stores the disposed flag
            /// </summary>
            private volatile bool disposed;

            /// <summary>
            /// Stores the fabric cluster context;
            /// </summary>
            //private IHpcContext context;

            /// <summary>
            /// Initializes a new instance of the SoaDiagAuthenticator class
            /// </summary>
            public SoaDiagAuthenticator()
            {
                this.headNode = SoaHelper.GetSchedulerName(false);
                //this.schedulerHelper = new SchedulerHelper(fabricClient, token);
            }

            /// <summary>
            /// Authenticate the incoming user
            /// </summary>
            /// <param name="sessionId">indicating the session id</param>
            /// <param name="identity">indicating the call identity</param>
            /// <returns>
            /// returns a boolean value indicating whether the incoming user
            /// is authenticated to access the given session
            /// </returns>
            public bool AuthenticateUser(int sessionId, WindowsIdentity identity)
            {
                if (this.disposed)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerIsOffline, SR.BrokerIsOffline);
                }

                try
                {
                    //TODO: SF: this.headNode is cluster connection string
                    if (Utility.IsCallingFromHeadNode(identity, this.headNode))
                    {
                        return true;
                    }
                    else
                    {
                        return identity.IsAuthenticated;
                    }
                }
                catch (Exception e)
                {
                    // print trace here and rethrow the exception.
                    TraceHelper.TraceEvent(
                        sessionId,
                        TraceEventType.Error,
                        "[LauncherHostService] AuthenticateUser: Failed to authenticate user, {0}", e.ToString());
                    throw;
                }
            }

            /// <summary>
            /// Dispose the instance
            /// </summary>
            /// <param name="disposing">indicating the disposing flag</param>
            protected override void Dispose(bool disposing)
            {
                this.disposed = true;

                //if (this.schedulerHelper != null)
                //{
                //    this.schedulerHelper.Dispose();
                //    this.schedulerHelper = null;
                //}

                base.Dispose(disposing);
            }
        }
    }
}
