namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;

    public class AzureEnvironment
    {
        private static Dictionary<string, AzureEnvironment> BuiltInEnvironments { get; }

        static AzureEnvironment()
        {
            var azureCloud = new AzureEnvironment
            {
                Name = AzureEnvironmentConstants.AzureCloud,
                ServiceDnsSuffix = AzureEnvironmentConstants.AzureServiceDnsSuffix,
                ServiceManagementUrl = AzureEnvironmentConstants.AzureServiceEndpoint,
                ResourceManagerUrl = AzureEnvironmentConstants.AzureResourceManagerEndpoint,
                ManagementPortalUrl = AzureEnvironmentConstants.AzureManagementPortalUrl,
                ActiveDirectoryAuthority = AzureEnvironmentConstants.AzureActiveDirectoryEndpoint,
                ActiveDirectoryServiceEndpointResourceId = AzureEnvironmentConstants.AzureServiceEndpoint,
                StorageEndpointSuffix = AzureEnvironmentConstants.AzureStorageEndpointSuffix,
                GalleryUrl = AzureEnvironmentConstants.GalleryEndpoint,
                SqlDatabaseDnsSuffix = AzureEnvironmentConstants.AzureSqlDatabaseDnsSuffix,
                GraphUrl = AzureEnvironmentConstants.AzureGraphEndpoint,
                TrafficManagerDnsSuffix = AzureEnvironmentConstants.AzureTrafficManagerDnsSuffix,
                AzureKeyVaultDnsSuffix = AzureEnvironmentConstants.AzureKeyVaultDnsSuffix,
                AzureKeyVaultServiceEndpointResourceId = AzureEnvironmentConstants.AzureKeyVaultServiceEndpointResourceId,
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = AzureEnvironmentConstants.AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix,
                AzureDataLakeStoreFileSystemEndpointSuffix = AzureEnvironmentConstants.AzureDataLakeStoreFileSystemEndpointSuffix,
                GraphEndpointResourceId = AzureEnvironmentConstants.AzureGraphEndpoint,
                DataLakeEndpointResourceId = AzureEnvironmentConstants.AzureDataLakeServiceEndpointResourceId,
                BatchEndpointResourceId = AzureEnvironmentConstants.BatchEndpointResourceId,
                AdTenant = "Common"
            };
            var azureChina = new AzureEnvironment
            {
                Name = AzureEnvironmentConstants.AzureChinaCloud,
                ServiceDnsSuffix = AzureEnvironmentConstants.ChinaServiceDnsSuffix,
                ServiceManagementUrl = AzureEnvironmentConstants.ChinaServiceEndpoint,
                ResourceManagerUrl = AzureEnvironmentConstants.ChinaResourceManagerEndpoint,
                ManagementPortalUrl = AzureEnvironmentConstants.ChinaManagementPortalUrl,
                ActiveDirectoryAuthority = AzureEnvironmentConstants.ChinaActiveDirectoryEndpoint,
                ActiveDirectoryServiceEndpointResourceId = AzureEnvironmentConstants.ChinaServiceEndpoint,
                StorageEndpointSuffix = AzureEnvironmentConstants.ChinaStorageEndpointSuffix,
                GalleryUrl = AzureEnvironmentConstants.GalleryEndpoint,
                SqlDatabaseDnsSuffix = AzureEnvironmentConstants.ChinaSqlDatabaseDnsSuffix,
                GraphUrl = AzureEnvironmentConstants.ChinaGraphEndpoint,
                TrafficManagerDnsSuffix = AzureEnvironmentConstants.ChinaTrafficManagerDnsSuffix,
                AzureKeyVaultDnsSuffix = AzureEnvironmentConstants.ChinaKeyVaultDnsSuffix,
                AzureKeyVaultServiceEndpointResourceId = AzureEnvironmentConstants.ChinaKeyVaultServiceEndpointResourceId,
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = null,
                AzureDataLakeStoreFileSystemEndpointSuffix = null,
                DataLakeEndpointResourceId = null,
                GraphEndpointResourceId = AzureEnvironmentConstants.ChinaGraphEndpoint,
                BatchEndpointResourceId = AzureEnvironmentConstants.ChinaBatchEndpointResourceId,
                AdTenant = "Common"
            };
            var azureUSGovernment = new AzureEnvironment
            {
                Name = AzureEnvironmentConstants.AzureUSGovernment,
                ServiceDnsSuffix = AzureEnvironmentConstants.USGovernmentServiceDnsSuffix,
                ServiceManagementUrl = AzureEnvironmentConstants.USGovernmentServiceEndpoint,
                ResourceManagerUrl = AzureEnvironmentConstants.USGovernmentResourceManagerEndpoint,
                ManagementPortalUrl = AzureEnvironmentConstants.USGovernmentManagementPortalUrl,
                ActiveDirectoryAuthority = AzureEnvironmentConstants.USGovernmentActiveDirectoryEndpoint,
                ActiveDirectoryServiceEndpointResourceId = AzureEnvironmentConstants.USGovernmentServiceEndpoint,
                StorageEndpointSuffix = AzureEnvironmentConstants.USGovernmentStorageEndpointSuffix,
                GalleryUrl = AzureEnvironmentConstants.GalleryEndpoint,
                SqlDatabaseDnsSuffix = AzureEnvironmentConstants.USGovernmentSqlDatabaseDnsSuffix,
                GraphUrl = AzureEnvironmentConstants.USGovernmentGraphEndpoint,
                TrafficManagerDnsSuffix = AzureEnvironmentConstants.USGovernmentTrafficManagerDnsSuffix,
                AzureKeyVaultDnsSuffix = AzureEnvironmentConstants.USGovernmentKeyVaultDnsSuffix,
                AzureKeyVaultServiceEndpointResourceId = AzureEnvironmentConstants.USGovernmentKeyVaultServiceEndpointResourceId,
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = null,
                AzureDataLakeStoreFileSystemEndpointSuffix = null,
                DataLakeEndpointResourceId = null,
                GraphEndpointResourceId = AzureEnvironmentConstants.USGovernmentGraphEndpoint,
                BatchEndpointResourceId = AzureEnvironmentConstants.USGovernmentBatchEndpointResourceId,
                AdTenant = "Common"
            };
            var azureGermany = new AzureEnvironment
            {
                Name = AzureEnvironmentConstants.AzureGermanCloud,
                ServiceDnsSuffix = AzureEnvironmentConstants.GermanServiceDnsSuffix,
                ServiceManagementUrl = AzureEnvironmentConstants.GermanServiceEndpoint,
                ResourceManagerUrl = AzureEnvironmentConstants.GermanResourceManagerEndpoint,
                ManagementPortalUrl = AzureEnvironmentConstants.GermanManagementPortalUrl,
                ActiveDirectoryAuthority = AzureEnvironmentConstants.GermanActiveDirectoryEndpoint,
                ActiveDirectoryServiceEndpointResourceId = AzureEnvironmentConstants.GermanServiceEndpoint,
                StorageEndpointSuffix = AzureEnvironmentConstants.GermanStorageEndpointSuffix,
                GalleryUrl = AzureEnvironmentConstants.GalleryEndpoint,
                SqlDatabaseDnsSuffix = AzureEnvironmentConstants.GermanSqlDatabaseDnsSuffix,
                GraphUrl = AzureEnvironmentConstants.GermanGraphEndpoint,
                TrafficManagerDnsSuffix = AzureEnvironmentConstants.GermanTrafficManagerDnsSuffix,
                AzureKeyVaultDnsSuffix = AzureEnvironmentConstants.GermanKeyVaultDnsSuffix,
                AzureKeyVaultServiceEndpointResourceId = AzureEnvironmentConstants.GermanAzureKeyVaultServiceEndpointResourceId,
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = null,
                AzureDataLakeStoreFileSystemEndpointSuffix = null,
                DataLakeEndpointResourceId = null,
                GraphEndpointResourceId = AzureEnvironmentConstants.GermanGraphEndpoint,
                BatchEndpointResourceId = AzureEnvironmentConstants.GermanBatchEndpointResourceId,
                AdTenant = "Common"
            };

            BuiltInEnvironments = new Dictionary<string, AzureEnvironment>(StringComparer.InvariantCultureIgnoreCase)
            {
                { AzureEnvironmentConstants.AzureCloud, azureCloud },
                { AzureEnvironmentConstants.AzureChinaCloud, azureChina },
                { AzureEnvironmentConstants.AzureUSGovernment, azureUSGovernment },
                { AzureEnvironmentConstants.AzureGermanCloud, azureGermany }
            };
        }

        public static string GetEndpointString(string endpointName, string environmentName = AzureEnvironmentConstants.AzureCloud)
        {
            string propertyValue = null;
            if (TryGetEndpointString(endpointName, environmentName, out propertyValue))
            {
                return propertyValue;
            }

            throw new InvalidOperationException($"Endpoint name {endpointName} not found");
        }

        public static bool TryGetEndpointString(string endpointName, string environmentName, out string propertyValue)
        {
            if (string.IsNullOrEmpty(environmentName))
            {
                environmentName = AzureEnvironmentConstants.AzureCloud;
            }

            propertyValue = null;
            AzureEnvironment environment = null;
            if (!BuiltInEnvironments.TryGetValue(environmentName, out environment))
            {
                return false;
            }

            switch (endpointName)
            {
                case Endpoint.AdTenant:
                    propertyValue = environment.AdTenant;
                    break;
                case Endpoint.ActiveDirectoryServiceEndpointResourceId:
                    propertyValue = environment.ActiveDirectoryServiceEndpointResourceId;
                    break;
                case Endpoint.AzureKeyVaultDnsSuffix:
                    propertyValue = environment.AzureKeyVaultDnsSuffix;
                    break;
                case Endpoint.AzureKeyVaultServiceEndpointResourceId:
                    propertyValue = environment.AzureKeyVaultServiceEndpointResourceId;
                    break;
                case Endpoint.GraphEndpointResourceId:
                    propertyValue = environment.GraphEndpointResourceId;
                    break;
                case Endpoint.SqlDatabaseDnsSuffix:
                    propertyValue = environment.SqlDatabaseDnsSuffix;
                    break;
                case Endpoint.StorageEndpointSuffix:
                    propertyValue = environment.StorageEndpointSuffix;
                    break;
                case Endpoint.TrafficManagerDnsSuffix:
                    propertyValue = environment.TrafficManagerDnsSuffix;
                    break;
                case Endpoint.AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix:
                    propertyValue = environment.AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix;
                    break;
                case Endpoint.AzureDataLakeStoreFileSystemEndpointSuffix:
                    propertyValue = environment.AzureDataLakeStoreFileSystemEndpointSuffix;
                    break;
                case Endpoint.DataLakeEndpointResourceId:
                    propertyValue = environment.DataLakeEndpointResourceId;
                    break;
                case Endpoint.ActiveDirectory:
                case LegacyAzureEndpointName.AzureADAuthority:
                    propertyValue = environment.ActiveDirectoryAuthority;
                    break;
                case Endpoint.Gallery:
                    propertyValue = environment.GalleryUrl;
                    break;
                case Endpoint.Graph:
                    propertyValue = environment.GraphUrl;
                    break;
                case Endpoint.ManagementPortalUrl:
                    propertyValue = environment.ManagementPortalUrl;
                    break;
                case Endpoint.ResourceManager:
                case LegacyAzureEndpointName.AzureADResource:
                    propertyValue = environment.ResourceManagerUrl;
                    break;
                case Endpoint.ServiceManagement:
                    propertyValue = environment.ServiceManagementUrl;
                    break;
                case Endpoint.ServiceDnsSuffix:
                case LegacyAzureEndpointName.AzureServiceDomain:
                    propertyValue = environment.ServiceDnsSuffix;
                    break;
                case Endpoint.BatchEndpointResourceId:
                case LegacyAzureEndpointName.AzureBatchResourceUri:
                    propertyValue = environment.BatchEndpointResourceId;
                    break;
                case LegacyAzureEndpointName.AzureManagementDomain:
                    propertyValue = environment.ServiceManagementUrl;
                    if (propertyValue.Contains("//"))
                    {
                        propertyValue = propertyValue.Substring(propertyValue.IndexOf("//", StringComparison.Ordinal)).Trim('/');
                    }
                    break;
                case LegacyAzureEndpointName.AzureBlobStorageDomain:
                    propertyValue = $"blob.{environment.StorageEndpointSuffix}";
                    break;
                case LegacyAzureEndpointName.AzureQueueStorageDomain:
                    propertyValue = $"queue.{environment.StorageEndpointSuffix}";
                    break;
                case LegacyAzureEndpointName.AzureTableStorageDomain:
                    propertyValue = $"table.{environment.StorageEndpointSuffix}";
                    break;
                case LegacyAzureEndpointName.AzureFileStorageDomain:
                    propertyValue = $"file.{environment.StorageEndpointSuffix}";
                    break;
                case LegacyAzureEndpointName.AzureSqlManagementDomain:
                    propertyValue = $"management.{environment.SqlDatabaseDnsSuffix}";
                    break;
            }

            return propertyValue != null;
        }
        /// <summary>
        /// The name of the environment
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the environment
        /// </summary>
        public string ServiceDnsSuffix { get; set; }

        /// <summary>
        /// The name of the environment
        /// </summary>
        public string ServiceManagementDnsSuffix { get; set; }

        /// <summary>
        /// The RDFE endpoint
        /// </summary>
        string ServiceManagementUrl { get; set; }

        /// <summary>
        /// The Resource Manager endpoint
        /// </summary>
        string ResourceManagerUrl { get; set; }

        /// <summary>
        /// The location fot eh manageemnt portal
        /// </summary>
        string ManagementPortalUrl { get; set; }

        /// <summary>
        /// The Active Directory authentication endpoint
        /// </summary>
        string ActiveDirectoryAuthority { get; set; }

        /// <summary>
        /// The ARM template gallery endpoint.
        /// </summary>
        string GalleryUrl { get; set; }

        /// <summary>
        /// The Azure Active Directory Graph endpoint
        /// </summary>
        string GraphUrl { get; set; }

        /// <summary>
        /// The token audience required to access the RDFE or ARM endpoints
        /// </summary>
        string ActiveDirectoryServiceEndpointResourceId { get; set; }

        /// <summary>
        /// The domain name suffix for storage services
        /// </summary>
        string StorageEndpointSuffix { get; set; }

        /// <summary>
        /// The domain name suffix for Sql databases
        /// </summary>
        string SqlDatabaseDnsSuffix { get; set; }

        /// <summary>
        /// The domain anme suffix for traffic manager endpoints
        /// </summary>
        string TrafficManagerDnsSuffix { get; set; }

        /// <summary>
        /// The domain name suffix for Azure KeyVault valuts
        /// </summary>
        string AzureKeyVaultDnsSuffix { get; set; }

        /// <summary>
        /// The token audience required for authenticating with Azure KeyVault vaults
        /// </summary>
        string AzureKeyVaultServiceEndpointResourceId { get; set; }

        /// <summary>
        /// The token audience required to authenticate with the Azure Active Directory Graph services
        /// </summary>
        string GraphEndpointResourceId { get; set; }

        /// <summary>
        /// The token audience required to authenticate with the Azure Active Directory Data Lake services
        /// </summary>
        string DataLakeEndpointResourceId { get; set; }

        /// <summary>
        /// The token audience required to authenticate with the Azure Batch service
        /// </summary>
        string BatchEndpointResourceId { get; set; }

        /// <summary>
        ///  The domain name suffix for Azure DataLake Catalog and Job services
        /// </summary>
        string AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix { get; set; }

        /// <summary>
        /// The domain name suffix for Azure Data Lake FileSyattem services
        /// </summary>
        string AzureDataLakeStoreFileSystemEndpointSuffix { get; set; }

        /// <summary>
        /// The default Active Directory Tenant
        /// </summary>
        string AdTenant { get; set; }

        public static class Endpoint
        {
            public const string AdTenant = "AdTenant",
                ServiceDnsSuffix = "ServiceDnsSuffix",
                ActiveDirectoryServiceEndpointResourceId = "ActiveDirectoryServiceEndpointResourceId",
                GraphEndpointResourceId = "GraphEndpointResourceId",
                AzureKeyVaultServiceEndpointResourceId = "AzureKeyVaultServiceEndpointResourceId",
                AzureKeyVaultDnsSuffix = "AzureKeyVaultDnsSuffix",
                TrafficManagerDnsSuffix = "TrafficManagerDnsSuffix",
                SqlDatabaseDnsSuffix = "SqlDatabaseDnsSuffix",
                StorageEndpointSuffix = "StorageEndpointSuffix",
                Graph = "Graph",
                Gallery = "Gallery",
                ActiveDirectory = "ActiveDirectory",
                ServiceManagement = "ServiceManagement",
                ResourceManager = "ResourceManager",
                ManagementPortalUrl = "ManagementPortalUrl",
                AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix = "AzureDataLakeAnalyticsCatalogAndJobEndpointSuffix",
                AzureDataLakeStoreFileSystemEndpointSuffix = "AzureDataLakeStoreFileSystemEndpointSuffix",
                DataLakeEndpointResourceId = "DataLakeEndpointResourceId",
                BatchEndpointResourceId = "BatchEndpointResourceId";
        }
    }
}
