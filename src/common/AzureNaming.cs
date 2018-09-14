//--------------------------------------------------------------------------
// <copyright file="AzureNaming.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This is a common module for Azure-related naming
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.Win32;
    using System.Text.RegularExpressions;

    [Flags]
    internal enum SchedulerBootStrapModules
    {
        Scheduler = 0x1,
        Node = 0x2,
        RestService = 0x4,
        RemoteAccess = 0x8,
        RemoteForwarder = 0x10,
        Portal = 0x20,
        Session = 0x40,
        Broker = 0x80,
        SoaRest = 0x400
    }

    //For Auto-Upload of Log files for HPC Burst to Azure
    internal enum AzureLogsToBlobPolicyEnum
    {
        Disabled = 1, //no log files are transferred to blob. This is the default value at installation time
        Proxy = 2, //only log files from Proxy nodes (all) are transferred to blob
        Compute = 3, //only log files from Worker nodes (all) are transferred to blob
        All = 4 //proxy log files from both Proxy and Worker nodes (all) are transferred to blob
    }

    /// <summary>
    /// Common code for determining how we name things in Azure.
    /// </summary>
    internal class AzureNaming
    {
        private static string storageConnStrTemplate = "BlobEndpoint=https://{0}." + AzureBlobStorageDomain + ";QueueEndpoint=https://{0}." + AzureQueueStorageDomain + ";TableEndpoint=https://{0}." + AzureTableStorageDomain + ";AccountName={0};AccountKey={1}";
        private static string azureProxyFilePath = @"%CCP_HOME%Microsoft.Hpc.AzureProxyFile";
        public const string AzurePartitionKey = "NodeMapping";

        /// <summary>
        /// Gets a value indicating whether this instance is GA cluster.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is GA cluster; otherwise, <c>false</c>.
        /// </value>
        public static bool IsGACluster(string azureManagementDomain)
        {
            return string.Equals(AzureDnsSuffixes.ManagementPostfix, azureManagementDomain, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the domain for the Azure Blob Storage.
        /// </summary>
        public static string AzureBlobStorageDomain
        {
            get
            {
                return GetAzureDomain("AzureBlobStorageDomain", AzureDnsSuffixes.BlobPostfix);
            }
        }

        /// <summary>
        /// Gets the domain for the Azure Table Storage.
        /// </summary>
        public static string AzureTableStorageDomain
        {
            get
            {
                return GetAzureDomain("AzureTableStorageDomain", AzureDnsSuffixes.TablePostfix);
            }
        }

        /// <summary>
        /// Gets the domain for the Azure Queue Storage.
        /// </summary>
        public static string AzureQueueStorageDomain
        {
            get
            {
                return GetAzureDomain("AzureQueueStorageDomain", AzureDnsSuffixes.QueuePostfix);
            }
        }

        /// <summary>
        /// Gets the domain for the Azure Table Storage.
        /// </summary>
        public static string AzureFileStorageDomain
        {
            get
            {
                return GetAzureDomain("AzureFileStorageDomain", AzureDnsSuffixes.FilePostfix);
            }
        }

        /// <summary>
        /// Gets the domain for the Azure Cloud Service.
        /// </summary>
        public static string AzureServiceDomain
        {
            get
            {
                return GetAzureDomain("AzureServiceDomain", AzureDnsSuffixes.ServiceDomain);
            }
        }

        /// <summary>
        /// Gets path to the Azure proxy file
        /// </summary>
        public static string AzureProxyFileName
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(azureProxyFilePath);
            }
        }

        public static string AzureADAuthority
        {
            get { return GetAzureDomain("AzureADAuthority", AzureDnsSuffixes.AzureADAuthority); }
        }

        public static string AzureADResource
        {
            get { return GetAzureDomain("AzureADResource", AzureDnsSuffixes.AzureADResource); }
        }

        /// <summary>
        /// Retrieves a value from registry for an Azure domain.
        /// </summary>
        public static string GetAzureDomain(string name, string defaultValue)
        {
            string result = defaultValue;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\HPC"))
            {
                if (key != null)
                {
                    result = (string)key.GetValue(name, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Generates the name of an Azure Storage entity.
        /// This should be unique/deterministic based on the inputs, 63 character maximum.
        /// If you change logic here, please also update the same method in
        /// private\AzureSchedulerService\Samples\AzureSampleService\AppConfigure\AzureManagementHelper\AzureNaming.cs
        /// </summary>
        public static string GenerateAzureEntityName(
            string entityName,
            string clusterName,
            Guid subscriptionId,
            string serviceName)
        {
            // Concat the values; we add "/" inside here to resolve bug 20877
            string name = entityName + "/" + clusterName + "/" + serviceName + "/" + subscriptionId.ToString();
            return GenerateEntityName(name);
        }

        public static string GenerateAzureBatchPerfCounterTableName(
            string clusterName,
            string poolName)
        {
            return GenerateEntityName("Counter" + poolName + clusterName);
        }

        private static string GenerateEntityName(string longName)
        {
            var name = longName.ToLowerInvariant();
            // Strip non Alphanumeric chars
            string shortName = Regex.Replace(name, @"[^a-zA-Z0-9]", "");

            // Truncate to 55 characters
            if (shortName.Length > 55)
            {
                shortName = shortName.Substring(0, 55);
            }

            // Append a hash value of 8 characters
            return shortName + GenerateEightCharHashSuffix(name);
        }

        /// <summary>
        /// Generates logical name of an Azure node based on instance id.
        /// This is for bootstrapping only - to be removed later.
        /// </summary>
        public static string GenerateAzureLogicalNodeName(int instanceId)
        {
            Debug.Assert((instanceId >= 0) && (instanceId <= 9998));
            string name = string.Format("AzureCN-{0:D4}", instanceId + 1);
            return name;
        }

        /// <summary>
        /// Basically our goal for this function is to take a string and generate a "unique" 8 character string
        /// to help name things uniquely/determinstically. It should be:
        /// - Independent of CLR version (no reliance on GetHashCode()).
        /// - entirely comprised of safe-characters for Azure naming (we'll stick to numbers and lower-case letters).
        /// - Not too naive (collisons should be very improbable).
        /// </summary>
        private static string GenerateEightCharHashSuffix(string stringToHash)
        {
            // Calculate SHA-1 Hash of the string (this will be 20 bytes long)
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                byte[] str = sha1.ComputeHash(
                    System.Text.Encoding.UTF8.GetBytes(stringToHash));

                // A 4-byte hash is more useful to us for a short checksum
                byte[] hash = new byte[] { 0, 0, 0, 0 };

                // Use XOR to condense the hash
                for (int i = 0; i < str.Length / 4; i++)
                {
                    hash[0] ^= str[4 * i];
                    hash[1] ^= str[(4 * i) + 1];
                    hash[2] ^= str[(4 * i) + 2];
                    hash[3] ^= str[(4 * i) + 3];
                }

                // Hex-encode the 4-byte hash for a polite 8 character checksum
                string alphabet = "0123456789abcdef";
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(alphabet[hash[i] & 0xF]);
                    sb.Append(alphabet[(hash[i] >> 4) & 0xF]);
                }

                return sb.ToString();
            }
        }
    }

    internal partial class RoleNames
    {
        public const string Scheduler = "HpcAzureScheduler";
        public const string Node = "HpcNode";  // For general purpose

        public const string RestService = "HpcRestService";
        public const string PortalService = "HpcPortal";

        public const string Proxy = "HpcProxy";

        public const string SoaSession = "HpcSoaSession";
        public const string SoaBroker = "HpcSoaBroker";
        public const string SoaRest = "HpcSoaRest";

        public const string Role1Node = "HpcWorkerRole1";
        public const string Role2Node = "HpcWorkerRole2";
        public const string Role3Node = "HpcWorkerRole3";
        public const string Role4Node = "HpcWorkerRole4";
    }

    // For WAHS use
    internal partial class AzureRoleNames
    {
        // These variables are not used for node names, but just indicate the number of cores of a node
        public const string SmallEmbeddedNode = "HpcSmallEmbeddedWorker";
        public const string MediumEmbeddedNode = "HpcMediumEmbeddedWorker";
        public const string LargeEmbeddedNode = "HpcLargeEmbeddedWorker";
        public const string ExtraLargeEmbeddedNode = "HpcExtraLargeEmbeddedWorker";
        public const string A5EmbeddedNode = "HpcA5EmbeddedWorker";
        public const string A6EmbeddedNode = "HpcA6EmbeddedWorker";
        public const string A7EmbeddedNode = "HpcA7EmbeddedWorker";
        public const string A8EmbeddedNode = "HpcA8EmbeddedWorker";
        public const string A9EmbeddedNode = "HpcA9EmbeddedWorker";

        public static Dictionary<int, string> NumCoreToEmbeddedRoleNameMapping = null;
        public static Dictionary<string, int> EmbeddedRoleNameToNumCoreMapping = null;
        public static Dictionary<string, int> EmbeddedRoleNameToMemoryMapping = null;

        static AzureRoleNames()
        {
            // We still use hardcode here instead of parsing an XML file so as to minimize the change for WAHS
            EmbeddedRoleNameToNumCoreMapping = new Dictionary<string, int>();
            EmbeddedRoleNameToNumCoreMapping[SmallEmbeddedNode] = 1;
            EmbeddedRoleNameToNumCoreMapping[MediumEmbeddedNode] = 2;
            EmbeddedRoleNameToNumCoreMapping[LargeEmbeddedNode] = 4;
            EmbeddedRoleNameToNumCoreMapping[ExtraLargeEmbeddedNode] = 8;
            EmbeddedRoleNameToNumCoreMapping[A5EmbeddedNode] = 2;
            EmbeddedRoleNameToNumCoreMapping[A6EmbeddedNode] = 4;
            EmbeddedRoleNameToNumCoreMapping[A7EmbeddedNode] = 8;
            EmbeddedRoleNameToNumCoreMapping[A8EmbeddedNode] = 8;
            EmbeddedRoleNameToNumCoreMapping[A9EmbeddedNode] = 16;
        }
    }

    internal class SchedulerEndpointNames
    {
        public const string NodeManagerService = "Microsoft.Hpc.Azure.Endpoint.Manager";
        public const string ApplicationI = "Microsoft.Hpc.Azure.Endpoint.ApplicationI";
        public const string ApplicationII = "Microsoft.Hpc.Azure.Endpoint.ApplicationII";
        public const string SOADataService = "Microsoft.Hpc.Azure.Endpoint.ApplicationI";  // SOA reuses the application ports
        public const string SOAControlService = "Microsoft.Hpc.Azure.Endpoint.ApplicationII";
        public const string NodeMappingService = "Microsoft.Hpc.Azure.Endpoint.NodeMapping";

        ////public const string SchedulerListenerService = "SchedulerListener";
        // Temporarily all ports hack for Scheduler On Azure
        public const string SchedulerListenerService = "Microsoft.Hpc.Azure.Endpoint.AllPorts";

        public const int NumApplicationPorts = 8; // Change this value also have to change the gap between SOAData and SOAControl in the Module enum in NodeMapping.cs

        public const string HostsDistribution = "Microsoft.Hpc.Azure.Endpoint.HostsDistribution";
        public const string FileTransfer = "Microsoft.Hpc.Azure.Endpoint.FileTransfer";

        public const string HPCWebServiceHttps = "Microsoft.Hpc.Azure.Endpoint.HPCWebServiceHttps";

        public const string AllPorts = "Microsoft.Hpc.Azure.Endpoint.AllPorts";

        public const string ProxyService = "Microsoft.Hpc.Azure.Endpoint.JobManager";

        public const string FileStagingService = "Microsoft.Hpc.Azure.Endpoint.FileStaging";

        public static string ProxyServiceEndpoint = "Microsoft.Hpc.Azure.Endpoint.HpcComponents";

        public static string FileStagingServiceEndpoint = "Microsoft.Hpc.Azure.Endpoint.HpcComponents";
    }

    internal class DataProxyEndpointNames
    {
        public const string DataProxyEndpoint = "Microsoft.Hpc.Azure.Endpoint.DataProxy";
    }

    internal class TroubleShootingServiceTableNames
    {
        public const string RepositorySas = "RepositorySas";
    }

    internal class SchedulerQueueNames
    {
        public const string NodeMessageQueue = "NodeMessage";
        public const int MessageQueueCount = 4;
    }

    internal class SchedulerTableNames
    {
        public const string NodeMapping = "NodeMapping";
        public const string HeartBeats = "HeartBeats";
        public const string Counters = "Counters";
    }

    // Make sure it is the same with the template
    internal class SchedulerCertificateNames
    {
        public const string SSLCert = "Microsoft.Hpc.Azure.Certificate.SSLCert";
        public const string PasswordEncryptionCert = "Microsoft.Hpc.Azure.Certificate.PasswordEncryptionCert";
        public const string RDPPasswordEncryptionCert = "Microsoft.WindowsAzure.Plugins.RemoteAccess.PasswordEncryption";
        public const string HpcAzureServiceCertSubjectName = "CN=Microsoft HPC Azure Service";
        public const string HpcAzureClientCertSubjectName = "CN=Microsoft HPC Azure Client";
    }

    internal class AzureDnsSuffixes
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
        public const string AzureADAuthority = "https://login.windows.net/";
        public const string AzureADResource = "https://management.azure.com/";
    }

    internal class AzureConstants
    {
        public const int StorageClientTimeout = 180; //in seconds

        public const string LoadBalancerProbeName = "hpcilbp";

        public const string InternalLoadBalancerName = "hpcilb";
    }

    internal class AzurePortsSettings
    {
        public enum ReleaseOption : int
        {
            V3SP2 = 0,
            V3SP3 = 1,
        }

        public static ReleaseOption Setting = ReleaseOption.V3SP3;
    }

    internal class SchedulerPorts
    {
        public const string FileStagingPort = "7998";
        public const string FileStagingHeadNodePort = "7997";
        public const string NodeManagerPort = "1856";
        public const string SchedulerListenerPort = "5970";
        public const string SchedulerStorePort = "5802";
        public const string SchedulerStoreRemotingPort = "5800";
        public const string RestHttps = "443";
        public const string PortalHttps = "443";
        public const string NodeQueryPort = "6729";
        public const string HPCWebServiceHttps = "443";
        public const string SchedulerStoreInternalHttpsPort = "5801";
        public const string ProxyHttpsPort = "443";

        private const string _ProxyPortSettingV3SP2 = "7999";
        private const string _ProxyPortSettingV3SP3 = "443";

        private const string _FileStagingAzurePortSettingV3SP2 = "7998";
        private const string _FileStagingAzurePortSettingV3SP3 = "443";

        public static string ProxyPort = "443";

        public static string FileStagingAzurePort = "443";
    }

    internal class BrokerProxyPorts
    {
        private const string _ProxyPortSettingV3SP2 = "5901";
        private const string _ProxyPortSettingV3SP3 = "443";

        private const string _ManagementPortSettingV3SP2 = "5902";
        private const string _ManagementPortSettingV3SP3 = "443";

        public const string ProxyPortV4RTM = "443";
        public const string ManagementPortV4RTM = "443";
    }

    internal class DataProxyPorts
    {
        public static string ProxyPort = "8991";
    }

    internal class SchedulerConfigNames
    {
        public const string DataConnectionString = "Microsoft.Hpc.Azure.DataConnectionString";
        public const string StorageConnectionString = "Microsoft.Hpc.Azure.StorageConnectionString";
        
        public const string HeartBeatInterval = "Microsoft.Hpc.Azure.SchedulerHeartBeatInterval";
        public const string MaximumMissedHeartbeats = "Microsoft.Hpc.Azure.MaximumMissedHeartbeats";
        public const string ClusterId = "Microsoft.Hpc.Azure.ClusterId";
        public const string ClusterName = "Microsoft.Hpc.Azure.ClusterName";
        public const string SqlConnectionString = "Microsoft.Hpc.Azure.SqlConnectionString";
        public const string SchedulerIV = "Microsoft.Hpc.Azure.SchedulerIV";
        public const string SchedulerKey = "Microsoft.Hpc.Azure.SchedulerKey";
        public const string DeploymentId = "Microsoft.Hpc.Azure.DeploymentId";
        public const string AdminAccount = "Microsoft.Hpc.Azure.AdminAccount";
        public const string AdminEncryptedPassword = "Microsoft.Hpc.Azure.AdminEncryptedPassword";
        public const string RestorationTimeout = "Microsoft.Hpc.Azure.RestorationTimeout";
        public const string PasswordCertThumbprint = "Microsoft.Hpc.Azure.PasswordCertThumbprint";
        public const string SslCertThumbprint = "Microsoft.Hpc.Azure.SslCertThumbprint";
        public const string SchedulerRole = "Microsoft.Hpc.Azure.SchedulerRole";
        public const string NodeRoles = "Microsoft.Hpc.Azure.NodeRoles";
        public const string BrokerRoles = "Microsoft.Hpc.Azure.BrokerRoles";
        public const string SchedulerFailureActions = "Microsoft.Hpc.Azure.SchedulerFailureActions";
        public const string NodeFailureActions = "Microsoft.Hpc.Azure.NodeFailureActions";
        public const string NodeAutoOnline = "Microsoft.Hpc.Azure.NodeAutoOnline";
        public const string Counters = "Microsoft.Hpc.Azure.Counters";
        public const string CountersCollectionInterval = "Microsoft.Hpc.Azure.CollectionInterval";
        public const string ServiceDomain = "Microsoft.Hpc.Azure.ServiceDomain";
        public const string ServiceName = "Microsoft.Hpc.Azure.ServiceName";
        public const string NettcpOver443 = "Microsoft.Hpc.Azure.NettcpOver443";
        public const string NamingPattern = "Microsoft.Hpc.Azure.NamingPattern";

        public const string HostsServiceEnabled = "Microsoft.Hpc.Azure.HostsServiceEnabled";
        public const string HostsRefreshInterval = "Microsoft.Hpc.Azure.HostsRefreshInterval";
        public const string HostsRefreshShortInterval = "Microsoft.Hpc.Azure.HostsRefreshShortInterval";
        public const string HostsRefreshLongInterval = "Microsoft.Hpc.Azure.HostsRefreshLongInterval";
        public const string RestServiceFailureActions = "Microsoft.Hpc.Azure.RestServiceFailureActions";

        public const string NodeMapping = "Microsoft.Hpc.Azure.NodeMapping";
        public const string HeartBeats = "Microsoft.Hpc.Azure.HeartBeats";
        public const string NodeMessageQueue = "Microsoft.Hpc.Azure.NodeMessage";

        public const string ModulesEnabled = "Microsoft.Hpc.Azure.ModulesEnabled";

        public const string AzureSchedulerTracing = "Microsoft.Hpc.Azure.AzureSchedulerTracing";
        public const string AzureNodeManagerTracing = "Microsoft.Hpc.Azure.AzureNodeManagerTracing";
        public const string AzureFileStagingWorkerTracing = "Microsoft.Hpc.Azure.AzureFileStagingWorkerTracing";
        public const string AzureNodeCountersTracing = "Microsoft.Hpc.Azure.AzureNodeCountersTracing";
        public const string AzureRestServiceTracing = "Microsoft.Hpc.Azure.AzureRestServiceTracing";
        public const string AzurePortalTracing = "Microsoft.Hpc.Azure.AzurePortalTracing";
        public const string HostsFileDistributionTracing = "Microsoft.Hpc.Azure.HostsFileDistributionTracing";
        public const string FileTransferTracing = "Microsoft.Hpc.Azure.FileTransferTracing";
        public const string AzureSoaDiagMonTracing = "Microsoft.Hpc.Azure.AzureSoaDiagMonTracing";
        public const string SerializedNodeData = "Microsoft.Hpc.Azure.SerializedNodeData";
        public const string StartupScript = "Microsoft.Hpc.Azure.StartupScript";
        public const string DeployedBy = "Microsoft.Hpc.Azure.DeployedBy";

        public const string SchedulerHttpEnabled = "Microsoft.Hpc.Azure.SchedulerHttpEnabled";
        public const string InitDBOnline = "Microsoft.Hpc.Azure.InitDBOnline";

        public const string VhdDriveUrl = "Microsoft.Hpc.Azure.VhdDriveUrl";

        public const string AzureVNet = "Microsoft.Hpc.Azure.VNet";
        public const string AzureSubnets = "Microsoft.Hpc.Azure.Subnets";

        //Azure Log upload to Blob related configuration
        public const string AzureLogsToBlobPolicy = "Microsoft.Hpc.Azure.AzureLogsToBlobPolicy";
        public const string HpcProxyAzureLogsToBlobThrottling = "Microsoft.Hpc.Azure.HpcProxy.AzureLogsToBlobThrottling";
        public const string HpcWorkerRole1AzureLogsToBlobThrottling = "Microsoft.Hpc.Azure.HpcWorkerRole1.AzureLogsToBlobThrottling";
        public const string HpcWorkerRole2AzureLogsToBlobThrottling = "Microsoft.Hpc.Azure.HpcWorkerRole2.AzureLogsToBlobThrottling";
        public const string HpcWorkerRole3AzureLogsToBlobThrottling = "Microsoft.Hpc.Azure.HpcWorkerRole3.AzureLogsToBlobThrottling";
        public const string HpcWorkerRole4AzureLogsToBlobThrottling = "Microsoft.Hpc.Azure.HpcWorkerRole4.AzureLogsToBlobThrottling";
        public const string AzureLogsToBlobInterval = "Microsoft.Hpc.Azure.AzureLogsToBlobInterval";
        public const string AzureHpcSyncFailureEnable = "Microsoft.Hpc.Azure.HpcSyncFailureEnable";
        public const string AzureDeploymentTimeout = "Microsoft.Hpc.Azure.DeploymentOperationTimeoutInMinutes";
        public const string AzureStartupTaskFailureEnable = "Microsoft.Hpc.Azure.AzureStartupTaskFailureEnable";
        public const string AzureStartupTaskTimeoutSec = "Microsoft.Hpc.Azure.AzureStartupTaskTimeoutSec";

        public const string NameSpace = "Microsoft.Hpc.Azure";
    }

    internal class TroubleShootingServiceConfigNames
    {
        public const string RepositorySas = "Microsoft.Hpc.Azure.RepositorySas";
    }

    internal class AzurePluginsConfigNames
    {
        public const string RemoteForwarderEnabled = "Microsoft.WindowsAzure.Plugins.RemoteForwarder.Enabled";
        public const string RemoteForwarderNamespace = "Microsoft.WindowsAzure.Plugins.RemoteForwarder";

        public const string RemoteAccessEnabled = "Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled";
        public const string RemoteAccessAccountUsername = "Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountUsername";
        public const string RemoteAccessAccountExpiration = "Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountExpiration";
        public const string RemoteAccessAccountEncryptedPassword = "Microsoft.WindowsAzure.Plugins.RemoteAccess.AccountEncryptedPassword";
        public const string RemoteAccessNamespace = "Microsoft.WindowsAzure.Plugins.RemoteAccess";

        public const string DiagnosticsConnectionString = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";
    }

    internal class HPCWebSite
    {
        public const string SiteName = "HPC";
        public const string VirtualAppName = "HPCPortal";
    }

    internal class ProxyConfigName
    {
        public const string ProxyMultiplicity = "Microsoft.Hpc.Azure.ProxyMultiplicity";
        public const string EncodedServerCertificate = "Microsoft.Hpc.Azure.EncodedServerCertificate";
        public const string EncodedClientCertificate = "Microsoft.Hpc.Azure.EncodedClientCertificate";

        public const string AzureSchedulerProxyTracing = "Microsoft.Hpc.Azure.AzureSchedulerProxyTracing";
        public const string AzureBrokerProxyTracing = "Microsoft.Hpc.Azure.AzureBrokerProxyTracing";
        public const string AzureDataProxyTracing = "Microsoft.Hpc.Azure.AzureDataProxyTracing";
        public const string AzureFileStagingProxyTracing = "Microsoft.Hpc.Azure.AzureFileStagingProxyTracing";

        public const string AzureMetricsEnabled = "Microsoft.Hpc.Azure.AzureMetricsEnabled";
        public const string AzureMetricsData = "Microsoft.Hpc.Azure.AzureMetricsData";
        public const string AzureMetricsTable = "Microsoft.Hpc.Azure.AzureMetricsTable";
    }

    internal class SoaConfigName
    {
        public const string SessionServiceFailureActions = "Microsoft.Hpc.Azure.SessionServiceFailureActions";
        public const string BrokerServiceFailureActions = "Microsoft.Hpc.Azure.BrokerServiceFailureActions";
        public const string SoaRestServiceFailureActions = "Microsoft.Hpc.Azure.SoaRestServiceFailureActions";
    }

    internal class ExecutableNames
    {
        public const string HpcSync = "HpcSync.exe";
        public const string HpcBootStrapper = @"HPCPack\bin\HpcBootStrapper.exe";
    }

    internal class LocalStorageNames
    {
        public const string Application = "Microsoft.Hpc.Azure.LocalStorage.Application";
        public const string Output = "Microsoft.Hpc.Azure.LocalStorage.Output";
        public const string VhdCache = "Microsoft.Hpc.Azure.LocalStorage.VhdCache";
    }

    internal class TracingEventId
    {
        public const int General = 0;
        public const int AzureNodeManager = 1;
        public const int AzureSchedulerProxy = 2;
        public const int AzureFileStagingProxy = 3;
        public const int AzureFileStagingWorker = 4;
        public const int AzureBrokerProxy = 5;
        public const int AzureNodeCounters = 6;
        public const int AzureScheduler = 7;
        public const int AzureRestService = 8;
        public const int HostsFileDistribution = 9;
        public const int AzureDataProxy = 10;
        public const int AzureSoaDiagMon = 11;
        public const int FileTransfer = 12;
    }

    internal class AzureMetricsConstants
    {
        /// <summary>
        /// Service bus endpoint address registry override (from AzureMetricsTrackingRegistryOverride above)
        /// </summary>
        public const string ServiceBusEndpointAddressOverrideTag = "ServiceBusEndpointAddressOverride";

        /// <summary>
        /// Subscription Id (old UsageId1)
        /// </summary>
        public const string SubscriptionIdTag = "SubscriptionId";

        /// <summary>
        /// Datacenter location of cloud service for deployment
        /// </summary>
        public const string LocationTag = "Location";

        /// <summary>
        /// Deployment time
        /// </summary>
        public const string DeploymentTimeTag = "DeploymentTime";

        /// <summary>
        /// Node types for mapping generic worker role name to size sku
        /// </summary>
        public const string NodeTypesTag = "NodeTypes";

        /// <summary>
        /// Enabled Azure features flags
        /// </summary>
        public const string EnabledAzureFeaturesTag = "EnabledAzureFeatures";

        /// <summary>
        /// Proxy update concurrency control tag
        /// </summary>
        public const string ProxyUpdateControlTableKey = "ProxyUpdateControl";

        /// <summary>
        /// The azure metrics table name
        /// </summary>
        public const string AzureMetricsTableName = "TrackingData";

        /// <summary>
        /// The package upload timings tag, used in AzureMetricsData role envrionment
        /// </summary>
        public const string PackageUploadTimingsTag = "PU";

        /// <summary>
        /// The server certificate upload timings tag, used in AzureMetricsData role envrionment
        /// </summary>
        public const string ServerCertificateUploadTimingsTag = "SCU";

        /// <summary>
        /// The client certificate upload timings tag, used in AzureMetricsData role envrionment
        /// </summary>
        public const string ClientCertificateUploadTimingsTag = "CCU";

        /// <summary>
        /// The storage configuration timings tag, used in azure metrics table, as PK, RK and property name
        /// </summary>
        public const string StorageConfigurationTimingsTag = "SC";

        /// <summary>
        /// The stop deployment timing tag, used in azure metrics table, as PK, RK
        /// </summary>
        public const string StopDeploymentTimingTag = "SD";

        /// <summary>
        /// The timing detail property name
        /// </summary>
        public const string TimingDetailPropertyName = "TimingDetail";

        /// <summary>
        /// The node state health property name, used in azure metrics table
        /// </summary>
        public const string NodeStateHealthPropertyName = "NodeHealthState";

        /// <summary>
        /// The cluster node state property name, used in azure metrics table
        /// </summary>
        public const string ClusterNodeStatePropertyName = "ClusterNodeState";

        /// <summary>
        /// The role instance id property
        /// </summary>
        public const string RoleInstanceIdProperty = "RoleInstanceId";

        /// <summary>
        /// The azure node address property name
        /// </summary>
        public const string AzureNodeAddressPropertyName = "AzureNodeAddress";

        /// <summary>
        /// The active head node performance counter table key, both PK and RK are set to this value
        /// </summary>
        public const string ActiveHeadNodePerformanceCounterTableKey = "ActiveHeadNode";

        /// <summary>
        /// The networkthrought perf counter partition key
        /// </summary>
        public const string NetworkthroughtPerfCounterPartitionKey = "NetworkThroughput";

        /// <summary>
        /// The proxy performance counter table partition key
        /// </summary>
        public const string ProxyPerformanceCounterTablePartitionKey = "ProxyPerfCounter";

        /// <summary>
        /// The job statistics partition key
        /// </summary>
        public const string JobStatisticsPK = "AzureJobStatistics";

        /// <summary>
        /// The RDP failure partition key
        /// </summary>
        public const string RdpFailurePK = "RdpFailure";

        /// <summary>
        /// The RDP failure count property name
        /// </summary>
        public const string RdpFailureCountPropertyName = "RdpFailureCount";

        /// <summary>
        /// The azure node status update interval in minutes
        /// Note that it's not strictly guaranteed due to the ThreadPool implementation
        /// </summary>
        public const int AzureNodeStatusUpdateIntervalInMinutes = 3;

        /// <summary>
        /// The azure job statistics interval in minutes
        /// </summary>
        public const int AzureJobStatisticsIntervalInMinutes = 10;

        /// <summary>
        /// The head node metrics update interval in minutes
        /// </summary>
        public const int HeadNodeMetricsUpdateIntervalInMinutes = 5;

        /// <summary>
        /// The proxy metrics update interval in minutes
        /// </summary>
        public const int ProxyMetricsUpdateIntervalInMinutes = 5;

        /// <summary>
        /// The rdp failure update interval in minutes
        /// </summary>
        public const int RdpFailureUpdateIntervalInMinutes = 5;

        /// <summary>
        /// The default azure metrics job statistics delay minutes
        /// </summary>
        public const int DefaultAzureMetricsJobStatisticsDelayMinutes = 5;

        /// <summary>
        /// The azure metrics perf counter collection interval in seconds
        /// </summary>
        public const int AzureMetricsPerfCounterCollectionIntervalInSeconds = 30;

        /// <summary>
        /// The partition key property name
        /// </summary>
        public const string PartitionKeyPropertyName = "PartitionKey";
    }

    /// <summary>
    /// Job statistics targets enum
    /// </summary>
    internal enum JobStatisticsTargets
    {
        SOAFinished,
        SOACanceled,
        SOAFailed,
        MPIFinished,
        MPICanceled,
        MPIFailed,
        ParametricFinished,
        ParametricCanceled,
        ParametricFailed,
        OthersFinished,
        OthersCanceled,
        OthersFailed,
        MPIInfiniBand
    }
}
