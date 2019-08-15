using System.Collections.Generic;

namespace TelepathyCommon.Azure
{
    public static class LegacyAzureEndpointName
    {
        public const string AzureManagementDomain = "AzureManagementDomain",
            AzureSqlManagementDomain = "AzureSqlManagementDomain",
            AzureBlobStorageDomain = "AzureBlobStorageDomain",
            AzureTableStorageDomain = "AzureTableStorageDomain",
            AzureQueueStorageDomain = "AzureQueueStorageDomain",
            AzureFileStorageDomain = "AzureFileStorageDomain",
            AzureServiceDomain = "AzureServiceDomain",
            AzureBatchResourceUri = "AzureBatchResourceUri",
            AzureADAuthority = "AzureADAuthority",
            AzureADResource = "AzureADResource";

        public static readonly List<string> NameCollection = new List<string>()
        {
            AzureManagementDomain,
            AzureSqlManagementDomain,
            AzureBlobStorageDomain,
            AzureTableStorageDomain,
            AzureQueueStorageDomain,
            AzureFileStorageDomain,
            AzureServiceDomain,
            AzureBatchResourceUri,
            AzureADAuthority,
            AzureADResource
        };
    }
}
