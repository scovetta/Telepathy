namespace TelepathyCommon
{
    using System;
    using System.Collections.Generic;

    using TelepathyCommon.Registry;

    /// <summary>
    /// Please only put the constants that need to be accessed by more than one services, setup or client here.
    /// Service specific constants should be moved into service local constants files.
    /// </summary>
    public static class HpcConstants
    {
        #region Service Fabric constants

        internal const int DefaultHttpsPort = 443;

        /// <summary>
        /// The default client connection port, in single node setup, this port should be incremented by 2 for logical node, 10100, 10101 ...
        /// </summary>
        public const int FabricClientConnectionPort = 10100;

        /// <summary>
        /// The default client connection port, in single node setup, this port should be incremented by 2 for logical node, 10101, 10103, 10105 ...
        /// </summary>
        public const int FabricClusterConnectionPort = 10101;

        /// <summary>
        /// The default lease driver port, in single node setup, this port should be incremented for logical node, 10200, 10201 ...
        /// </summary>
        public const int FabricLeaseDriverPort = 10200;

        /// <summary>
        /// The default service connection port, in single node setup, this port should be incremented for logical node, 10300, 10301 ...
        /// </summary>
        public const int FabricServiceConnectionPort = 10300;

        /// <summary>
        /// The number of ephemeral ports, not used in single node.
        /// </summary>
        public const int FabricNbrEphemeralPorts = 15000;

        /// <summary>
        /// The default gateway port, in single node setup, this port should be incremented for logical node, 10400, 10401 ...
        /// </summary>
        public const int FabricHttpGatewayPort = 10400;

        public static int HpcNamingServicePort => DefaultHttpsPort;

        public static int HpcFrontendServicePort => DefaultHttpsPort;

        public static int HpcWebStatelessServicePort => DefaultHttpsPort;

        public static int HpcDataServicePort => DefaultHttpsPort;

        public const string HpcApplicationTypeName = "HpcApplicationType";

        public const string HpcApplicationVersion = "1.0.0";

        public const string HpcApplicationUri = "fabric:/HpcApplication";

        #endregion

        #region Registry keys and names

        #region Cluster properties

        public static IList<string> ParentNames = new List<string>()
        {
            HpcFullKeyName,
            HpcSecurityRegKey,
            IaaSInfoKeyName
        };

#if !NETCORE
        public static IDictionary<string, ReliableProperty> ReliableProperties = new Dictionary<string, ReliableProperty>()
        {
            { ClusterNameRegVal,                        new ReliableProperty(ClusterNameRegVal,typeof(string), true) },
            { ClusterIdRegVal,                          new ReliableProperty(ClusterIdRegVal, typeof(Guid), true) },
            { CollectionIntervalRegVal,                 new ReliableProperty(CollectionIntervalRegVal, typeof(int)) },
            { LinuxHttpsRegVal,                         new ReliableProperty(LinuxHttpsRegVal, typeof(int)) },
            { SQMRegVal,                                new ReliableProperty(SQMRegVal, typeof(int)) },
            { RuntimeDataSharePropertyName,             new ReliableProperty(RuntimeDataSharePropertyName, typeof(string) )},
            { InstallSharePropertyName,                 new ReliableProperty(InstallSharePropertyName, typeof(string)) },
            { SpoolDirSharePropertyName,                new ReliableProperty(SpoolDirSharePropertyName, typeof(string)) },
            { DiagnosticsSharePropertyName,             new ReliableProperty(DiagnosticsSharePropertyName, typeof(string)) },
            { ServiceRegistrationSharePropertyName,     new ReliableProperty(ServiceRegistrationSharePropertyName, typeof(string)) },
            { AzureEnvironmentRegVal,                   new ReliableProperty(AzureEnvironmentRegVal, typeof(string)) },
            { AzureStorageConnectionString,             new ReliableProperty(AzureStorageConnectionString, typeof(string), HpcSecurityRegKey) },
            { ManagementTraceLevelRegVal,               new ReliableProperty(ManagementTraceLevelRegVal, typeof(int)) },
            { NetworkTopology,                          new ReliableProperty(NetworkTopology, typeof(string), true) },
            { ReportingDbStringRegVal,                  new ReliableProperty(ReportingDbStringRegVal, typeof(string), HpcSecurityRegKey) },
            { DiagnosticsDbStringRegVal,                new ReliableProperty(DiagnosticsDbStringRegVal, typeof(string),HpcSecurityRegKey) },
            { MonitoringDbStringRegVal,                 new ReliableProperty(MonitoringDbStringRegVal, typeof(string),HpcSecurityRegKey) },
            { SchedulerDbStringRegVal,                  new ReliableProperty(SchedulerDbStringRegVal, typeof(string),HpcSecurityRegKey) },
            { ManagementDbStringRegVal,                 new ReliableProperty(ManagementDbStringRegVal, typeof(string),HpcSecurityRegKey) },
            { ManagementDbServerRegVal,                 new ReliableProperty(ManagementDbServerRegVal, typeof(string)) },
            { SchedulerDbServerRegVal,                  new ReliableProperty(SchedulerDbServerRegVal, typeof(string)) },
            { ReportingDbServerRegVal,                  new ReliableProperty(ReportingDbServerRegVal, typeof(string)) },
            { DiagnosticsDbServerRegVal,                new ReliableProperty(DiagnosticsDbServerRegVal, typeof(string)) },
            { MonitoringDbServerRegVal,                 new ReliableProperty(MonitoringDbServerRegVal, typeof(string)) },
            { TrimLongMacName,                          new ReliableProperty(TrimLongMacName, typeof(int)) },
            { BiosIdName,                               new ReliableProperty(BiosIdName, typeof(int)) },
            { IaaSInfoDisableAutoAssignNodeTemplate,    new ReliableProperty(IaaSInfoDisableAutoAssignNodeTemplate, typeof(int), IaaSInfoKeyName) },
            { IaaSInfoSubscriptionId,                   new ReliableProperty(IaaSInfoSubscriptionId, typeof(Guid),IaaSInfoKeyName) },
            { IaaSInfoDeploymentId,                     new ReliableProperty(IaaSInfoDeploymentId, typeof(Guid),IaaSInfoKeyName) },
            { IaaSInfoLocation,                         new ReliableProperty(IaaSInfoLocation, typeof(string),IaaSInfoKeyName) },
            { IaaSInfoVNet,                             new ReliableProperty(IaaSInfoVNet, typeof(string),IaaSInfoKeyName) },
            { IaaSInfoSubnet,                           new ReliableProperty(IaaSInfoSubnet, typeof(string),IaaSInfoKeyName) },
            { IaaSInfoThumbPrint,                       new ReliableProperty(IaaSInfoThumbPrint, typeof(string),IaaSInfoKeyName) },
            { IaaSInfoResourceGroup,                    new ReliableProperty(IaaSInfoResourceGroup, typeof(string),IaaSInfoKeyName) },
            { IaaSInfoApplicationId,                    new ReliableProperty(IaaSInfoApplicationId, typeof(string),IaaSInfoKeyName) },
            { IaaSInfoTenantId,                         new ReliableProperty(IaaSInfoTenantId, typeof(string),IaaSInfoKeyName) },
            { AADTenant,                                new ReliableProperty(AADTenant, typeof(string)) },
            { AADTenantId,                              new ReliableProperty(AADTenantId, typeof(string)) },
            { AADInstance,                              new ReliableProperty(AADInstance, typeof(string)) },
            { AADAppName,                               new ReliableProperty(AADAppName, typeof(string)) },
            { AADClientAppId,                           new ReliableProperty(AADClientAppId, typeof(string)) },
            { AADClientAppKey,                          new ReliableProperty(AADClientAppKey, typeof(string)) },
            { AADClientAppRedirectUri,                  new ReliableProperty(AADClientAppRedirectUri, typeof(string)) },
            { BatchAADInstance,                         new ReliableProperty(BatchAADInstance, typeof(string)) },
            { BatchAADTenantId,                         new ReliableProperty(BatchAADTenantId, typeof(string)) },
            { BatchAADClientAppId,                      new ReliableProperty(BatchAADClientAppId, typeof(string)) },
            { BatchAADClientAppKey,                     new ReliableProperty(BatchAADClientAppKey, typeof(string)) },
            { SupportAad,                               new ReliableProperty(SupportAad, typeof(string)) },
            { SslThumbprint,                            new ReliableProperty(SslThumbprint, typeof(string)) },
        };
#endif

#endregion

#region Common registry keys

        public const string RegistryCollectionKey = "RegistryCollection";

        public const string HpcKeyName = @"SOFTWARE\Microsoft\HPC";
        public const string HpcFullKeyName = @"HKEY_LOCAL_MACHINE\" + HpcKeyName;
        public const string HpcSecurityRegKey = @"HKEY_LOCAL_MACHINE\Software\Microsoft\HPC\Security";

        public const string DisableSyncToLocalAdminGroup = "DisableSyncWithAdminGroup";

        // MISC
        public const string ClusterIdRegVal = @"ClusterId";
        public const string ClusterNameRegVal = @"ClusterName";
        public const string ClusterConnectionStringRegVal = @"ClusterConnectionString";
        
        public const string CollectionIntervalRegVal = @"CollectionInterval";
        public const string NodeIdRegVal = @"NodeId";
        public const string LinuxHttpsRegVal = "LinuxHttps";
        public const string SQMRegVal = "SQM";
        public const string AzureStorageConnectionString = "AzureStorageConnectionString";
        public const string NetworkTopology = "NetworkTopology";
        public const string AzureEnvironmentRegVal = "AzureEnvironment";

        // Per node information, still store in windows Registry on each node
        public const string ActiveRoleRegVal = @"ActiveRole";
        public const string InstalledRoleRegVal = "InstalledRole";
        public const string BinDirRegVal = @"BinDir";
        public const string DataDirRegVal = @"DataDir";
        public const string LogfileDirRegVal = "LogfileDir";

        // Client registry
        public const string CertificateValidationType = @"CertificateValidationType";

#endregion

#region Database related.

        public const string ReportingDbStringRegVal = "ReportingDbConnectionString";
        public const string DiagnosticsDbStringRegVal = "DiagnosticsDbConnectionString";
        public const string MonitoringDbStringRegVal = "MonitoringDbConnectionString";
        public const string SchedulerDbStringRegVal = @"SchedulerDbConnectionString";
        public const string ManagementDbStringRegVal = "ManagementDbConnectionString";

        public const string ManagementDbServerRegVal = "ManagementDbServerName";
        public const string SchedulerDbServerRegVal = "SchedulerDbServerName";
        public const string ReportingDbServerRegVal = "ReportingDbServerName";
        public const string DiagnosticsDbServerRegVal = "DiagnosticsDbServerName";
        public const string MonitoringDbServerRegVal = "MonitoringDbServerName";

#endregion

#region Scheduler specific registrys.

        public const string NodeManagerKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HpcNodeManager";

        public const string HeartbeatIntervalRegVal = @"HeartbeatInterval";

        public const string CreateConsoleEnabledRegVal = @"HpcConsoleSupport";

        /// <summary>
        /// The name of the registry value indicating whether node idleness detection policy is enabled.
        /// </summary>
        public const string HpcIdlenessPolicy = @"IdlenessPolicy";

        /// <summary>
        /// The name of the registry value indicating the time, in minutes, that must pass before a 
        /// "non-idle" (i.e. "occupied") compute node can become idle (i.e. "available").
        /// </summary>
        public const string HpcIdleTimeInterval = @"IdleTimeInterval";

#endregion

#region Management and SDM service specific registrys.

        public const string SdmSecurityPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\HpcSdm\Security";
        public const string SdmSecurityKey = @"ValueKey";
        public const string SdmSecurityInitVector = @"ValueIV";

        public const string ManagementTraceLevelRegVal = "ManagementTraceLevel";

#endregion

#region SOA specific registries

        public const string SoaRegKey = HpcFullKeyName + @"\SOA";
        public const string ServiceRegistrationRegKey = SoaRegKey + @"\ServiceRegistration";

#endregion

#region Monitoring specific

        public const string HpcMonitoringRegKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\HPC\Monitoring";

#endregion

#region Trouble Shooting Service specific registries

        // Reg Keys
        public const string TroubleShootingServiceRegKey = HpcFullKeyName + @"\TSS";
        public const string TssLogUploaderAgentServiceRegKey = TroubleShootingServiceRegKey + @"\LUA";

        // Reg Values in TssLogUploaderAgentServiceRegKey
        public const string TssLuaRepositorySasRegVal = @"RepositorySas";
        public const string TssLuaEnabledRegVal = @"Enabled";
        public const string TssLuaObserveLogsRegVal = @"ObserveLogs";
        public const string TssLuaConnectionStringRegVal = @"ConnectionString";
        public const string TssPaaSEtwTraceVerbose = @"PaaSEtwTraceVerbose";

#endregion

#endregion

#region HPC Cluster constants
        public const string CcpHome = "CCP_HOME";

        /// <summary>
        /// Name of the well known container.
        /// </summary>
        public const string PackageWellknownContainer = "hpcpackages-36a153ea-f6f6-4df2-924c-4262851cf440";

        public static string SingleHeadNodeToken => "SINGLE_HEAD_NODE_TOKEN";

        //TODO: consider to get real capacity
        public static string NodeCapacity => "1";

        //TODO: investigate why set the value later
        public static string FirstCoreIndex => "3";

#endregion

#region management part constants

        /// <summary>
        /// The trim long-macs property will automatically trim MAC addresses
        /// down to a 6-byte Maximum from the inside of the string -- outwards
        /// This special setting is used for cases where an 8-bit client identifier
        /// is provided in the Node XML for IB DHCP reservations
        /// </summary>
        public const string TrimLongMacName = "TrimLongMACAddresses";
        public const string BiosIdName = "DisableSMBIOS";


        /// <summary>
        /// IaaS info
        /// </summary>
        public const string IaaSInfoKeyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\HPC\IaaSInfo";
        public const string IaaSInfoDisableAutoAssignNodeTemplate = "DisableAutoAssignNodeTemplate";
        public const string IaaSInfoSubscriptionId = "SubscriptionId";
        public const string IaaSInfoDeploymentId = "DeploymentId";
        public const string IaaSInfoLocation = "Location";
        public const string IaaSInfoVNet = "VNet";
        public const string IaaSInfoSubnet = "Subnet";
        public const string IaaSInfoThumbPrint = "Thumbprint";
        public const string IaaSInfoResourceGroup = "ResourceGroup";
        public const string IaaSInfoApplicationId = "ApplicationId";
        public const string IaaSInfoTenantId = "TenantId";

        public const string TrackingServiceParentName = @"HKEY_LOCAL_MACHINE\Software\Microsoft\HPC\TrackingOverride";
        public const string TrackingServiceEndpointAddressOverrideKey = "epa";

        /// <summary>
        /// Grow shrink metric alias
        /// </summary>
        public static string GrowShrinkHpcCoresToGrowWithoutRequirementAlias => "HPCCoresToGrowWithoutRequirement";

        public static string GrowShrinkHpcSocketsToGrowWithoutRequirementAlias => "HPCSocketsToGrowWithoutRequirement";

        public static string GrowShrinkHpcNodesToGrowWithoutRequirementAlias => "HPCNodesToGrowWithoutRequirement";

#endregion

        #region integration with AAD constants
        /// <summary>
        /// Certificate for SSL, should be installed in local machine trust root store
        /// </summary>
        public const string SslThumbprint = "SSLThumbprint";

        /// <summary>
        /// flag to support AAD authentication
        /// </summary>
        public const string SupportAad = "SupportAAD";

        /// <summary>
        /// the base url for AAD, depend on Azure cloud
        /// for example, for global Azure cloud, it is https://login.microsoftonline.com/
        /// for other private Azure cloud, it could be other url
        /// </summary>
        public const string AADInstance = "AADInstance";

        /// <summary>
        /// The application name in AAD for HPC cluster
        /// </summary>
        public const string AADAppName = "AADAppName";

        /// <summary>
        /// Tenant name for AAD
        /// </summary>
        public const string AADTenant = "AADTenant";

        /// <summary>
        /// Tenant id (GUID) for AAD
        /// </summary>
        public const string AADTenantId = "AADTenantId";

        /// <summary>
        /// client app id(GUID) in AAD, used for client to authetication to HPC cluster
        /// </summary>
        public const string AADClientAppId = "AADClientAppId";

        /// <summary>
        /// client app key in AAD, used for servie principal authentication
        /// </summary>
        public const string AADClientAppKey = "AADClientAppKey";

        /// <summary>
        /// redirect uri of client app, used for client authentication to AAD
        /// </summary>
        public const string AADClientAppRedirectUri = "AADClientAppRedirectUri";

        public const string NonDomainRole = "NonDomainRole";
        #endregion

        #region Azure Batch ADD authentication

        /// <summary>
        /// the base url for AAD, depend on Azure cloud
        /// for example, for global Azure cloud, it is https://login.microsoftonline.com/
        /// for other private Azure cloud, it could be other url
        /// </summary>
        public const string BatchAADInstance = "BatchAADInstance";

        /// <summary>
        /// Tenant id (GUID) for AAD
        /// </summary>
        public const string BatchAADTenantId = "BatchAADTenantId";

        /// <summary>
        /// client app id(GUID) in AAD, used for client to authetication to HPC cluster
        /// </summary>
        public const string BatchAADClientAppId = "BatchAADClientAppId";

        /// <summary>
        /// client app key in AAD, used for servie principal authentication
        /// </summary>
        public const string BatchAADClientAppKey = "BatchAADClientAppKey";

        #endregion


        #region Monitoring service constants
        /// <summary>
        /// Port of monitoring service
        /// </summary>
        public const int MonitoringPort = 9894;

        /// <summary>
        /// Tracing source for monitoring interface
        /// </summary>
        public const string HpcMonitoringInterface = "HpcMonitoringInterface";

        /// <summary>
        /// monitoring client service name, also used as Tracing source
        /// </summary>
        public const string HpcMonitoringClient = "HpcMonitoringClient";

        /// <summary>
        /// Tracing source for monitoring server
        /// </summary>
        public const string HpcMonitoringServer = "HpcMonitoringServer";
#endregion

#region SOA constants

        /// <summary>
        /// Indicate data is stored in reliable registry
        /// </summary>
        public static string RegistrationStoreToken => "CCP_REGISTRATION_STORE";

#endregion

#region Scheduler constants

        public const string SchedulerEnvironmentVariableName = "CCP_SCHEDULER";
        public const string ConnectionStringEnvironmentVariableName = "CCP_CONNECTIONSTRING";

        public const string CcpAdminEnv = "CCP_ISADMIN";
        public const string CcpMapAdminUserEnv = "CCP_MAP_ADMIN_USER";

        public static string CcpRestart => "CCP_RESTART";

        #endregion

        #region Network share property names
        public const string RuntimeDataSharePropertyName = "RuntimeDataShare";
        public const string SpoolDirSharePropertyName = "SpoolDirShare";
        public const string ServiceRegistrationSharePropertyName = "ServiceRegistrationShare";
        public const string InstallSharePropertyName = "InstallShare";
        public const string DiagnosticsSharePropertyName = "DiagnosticsShare";
        #endregion

#region Azure DNS default suffix
        public const string DefaultBatchResourceUri = @"https://batch.core.windows.net/";
        public const string DefaultAzureADAuthorityUri = "https://login.windows.net/";
        public const string DefaultAzureADResourceUri = "https://management.azure.com/";
        public static class DefaultAzureDnsSuffixes
        {
            public const string ServiceDomain = "cloudapp.net";
            public const string QueuePostfix = "queue.core.windows.net";
            public const string TablePostfix = "table.core.windows.net";
            public const string FilePostfix = "file.core.windows.net";
            public const string BlobPostfix = "blob.core.windows.net";
            public const string ManagementPostfix = "management.core.windows.net";
            public const string SQLAzurePostfix = "database.windows.net";
            public const string SQLAzureManagementPostfix = "management.database.windows.net";
            public const string AzureIaaSDomains = "cloudapp.net|cloudapp.azure.com|chinacloudapp.cn";
        }
#endregion

#region EventId allocations

        // This part cannot be put into service specific constants because we need ensure they are unique
        public const int HpcCommonEventIdStart = 10000;
        public const int SchedulerServiceEventIdStart = 20000;
        public const int SessionServiceEventIdStart = 30000;
        public const int BrokerServiceEventIdStart = 40000;
        public const int ManagementServiceEventIdStart = 50000;
        public const int SdmServiceEventIdStart = 60000;
        public const int ComputeNodeServiceEventIdStart = 70000;
        public const int MonitoringServiceEventIdStart = 80000;
        public const int ReportingServiceEventIdStart = 90000;
        public const int DiagnosticsServiceEventIdStart = 100000;
        public const int WebServiceEventIdStart = 110000;
        public const int HpcNamingServiceEventIdStart = 120000;
        public const int AzureCommunicatorStatefulServiceEventIdStart = 130000;
        public const int AadIntegrationServiceEventIdStart = 140000;

#endregion

#region Front End constants
        public static string FrontEndAppRoot => "HpcFrontend";

        public static string HpcNamingAppRoot => "HpcNaming";

        public static string HpcManagerAppRoot => "HpcManager";

        public static string HpcMonitoringAppRoot => "HpcMonitoring";

        public static string HpcLinuxAppRoot => "HpcLinux";

        public static string HpcRestApiAppRoot => "Hpc";

        public static string HpcDataServiceAppRoot => "HpcData";

        #endregion

#region Deployment constants

        public const string HpcCertDefaultCommonName = "HPC Pack 2016 Communication";
        public const string CertificatesDirName = "Certificates";
        public const string HpcCnCertFileName = "HpcCnCommunication.pfx";
        public const string HpcHnPublicCertFileName = "HpcHnPublicCert.cer";

        #endregion
    }
}
