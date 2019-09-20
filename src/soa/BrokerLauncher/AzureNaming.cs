// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher

{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Common code for determining how we name things in Azure.
    /// </summary>
    internal class AzureNaming
    {
        private static string azureProxyFilePath = @"%CCP_HOME%Microsoft.Hpc.AzureProxyFile";

        public const string AzurePartitionKey = "NodeMapping";

        /// <summary>
        /// Generates the name of an Azure Storage entity.
        /// This should be unique/deterministic based on the inputs, 63 character maximum.
        /// If you change logic here, please also update the same method in
        /// private\AzureSchedulerService\Samples\AzureSampleService\AppConfigure\AzureManagementHelper\AzureNaming.cs
        /// </summary>
        public static string GenerateAzureEntityName(string entityName, string clusterName, Guid subscriptionId, string serviceName)
        {
            // Concat the values; we add "/" inside here to resolve bug 20877
            string name = entityName + "/" + clusterName + "/" + serviceName + "/" + subscriptionId.ToString();
            return GenerateEntityName(name);
        }

        private static string GenerateEntityName(string longName)
        {
            var name = longName.ToLowerInvariant();

            // Strip non Alphanumeric chars
            string shortName = Regex.Replace(name, @"[^a-zA-Z0-9]", string.Empty);

            // Truncate to 55 characters
            if (shortName.Length > 55)
            {
                shortName = shortName.Substring(0, 55);
            }

            // Append a hash value of 8 characters
            return shortName + GenerateEightCharHashSuffix(name);
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
                byte[] str = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToHash));

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

    internal class SchedulerTableNames
    {
        public const string NodeMapping = "NodeMapping";

        public const string HeartBeats = "HeartBeats";

        public const string Counters = "Counters";
    }

    // Make sure it is the same with the template
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

        // Azure Log upload to Blob related configuration
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
}