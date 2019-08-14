namespace Microsoft.Hpc
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading.Tasks;
    using Rest;

    public static class AzureEndpointRegistryExtension
    {
        private static readonly MemoryCache CacheInstance = new MemoryCache("AzureEndpointRegistry");
        private static DateTimeOffset ExpireTimeForHn => DateTimeOffset.Now.AddSeconds(10);
        private static DateTimeOffset ExpireTimeForClient => DateTimeOffset.Now.AddSeconds(30);


        /// <summary>
        /// Gets the domain for the Azure management API.
        /// </summary>
        public static async Task<string> GetAzureManagementDomainAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureManagementDomain, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the domain for the Azure sql management API.
        /// </summary>
        public static async Task<string> GetAzureSQLManagementDomainAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureSqlManagementDomain, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the domain for the Azure Blob Storage.
        /// </summary>
        public static async Task<string> GetAzureBlobStorageDomainAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureBlobStorageDomain, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the domain for the Azure Table Storage.
        /// </summary>
        public static async Task<string> GetAzureTableStorageDomainAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureTableStorageDomain, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the domain for the Azure Queue Storage.
        /// </summary>
        public static async Task<string> GetAzureQueueStorageDomainAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureQueueStorageDomain, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the domain for the Azure Table Storage.
        /// </summary>
        public static async Task<string> GetAzureFileStorageDomainAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureFileStorageDomain, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the domain for the Azure Cloud Service.
        /// </summary>
        public static async Task<string> GetAzureServiceDomainAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureServiceDomain, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the batch resource uri
        /// </summary>
        public static async Task<string> GetAzureBatchResourceUri(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureBatchResourceUri, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the batch resource uri
        /// </summary>
        public static async Task<string> GetAzureADAuthorityUriAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureADAuthority, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the batch resource uri
        /// </summary>
        public static async Task<string> GetAzureADResourceUriAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(LegacyAzureEndpointName.AzureADResource, headNode).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the batch resource uri
        /// </summary>
        public static async Task<string> GetAzureStorageEndpointSuffixAsync(this IHpcContext context, string headNode = null)
        {
            return await context.GetAzureEndpointStringAsync(AzureEnvironment.Endpoint.StorageEndpointSuffix, headNode).ConfigureAwait(false);
        }

        public static async Task<string> GetAzureEnvironmentAsync(this IHpcContext context, string headNode = null)
        {
            var cacheValue = CacheInstance.Get(HpcConstants.AzureEnvironmentRegVal);
            if (cacheValue == null)
            {
                bool isheadNode = context.FabricContext.IsHpcHeadNodeService();
                string result = null;
                if (!isheadNode)
                {
                    var client = string.IsNullOrEmpty(headNode) ? new HpcAzureEndpointRestClient(context) : new HpcAzureEndpointRestClient(headNode);
                    result = await client.GetAzureEnvironmentAsync().ConfigureAwait(false);
                }

                if (string.IsNullOrEmpty(result))
                {
                    result = await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName,
                        HpcConstants.AzureEnvironmentRegVal,
                        context.CancellationToken,
                        AzureEnvironmentConstants.AzureCloud);
                }

                if (!string.IsNullOrEmpty(result))
                {
                    CacheInstance.Set(HpcConstants.AzureEnvironmentRegVal, result, isheadNode ? ExpireTimeForHn : ExpireTimeForClient);
                }

                return result;
            }
            else
            {
                return (string)cacheValue;
            }

        }

        public static async Task<string> GetAzureEndpointStringAsync(this IHpcContext context, string endpoint, string headNode = null)
        {
            var cacheValue = CacheInstance.Get(endpoint);
            if (cacheValue == null)
            {
                bool isheadNode = context.FabricContext.IsHpcHeadNodeService();
                string result = null;
                if (!isheadNode)
                {
                    var client = string.IsNullOrEmpty(headNode) ? new HpcAzureEndpointRestClient(context) : new HpcAzureEndpointRestClient(headNode);
                    result = await client.GetAzureEndpointStringAsync(endpoint).ConfigureAwait(false);
                }

                if (string.IsNullOrEmpty(result))
                {
                    if (LegacyAzureEndpointName.NameCollection.Contains(endpoint, StringComparer.OrdinalIgnoreCase))
                    {
                        result = await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, endpoint, context.CancellationToken, null).ConfigureAwait(false);
                    }

                    if (string.IsNullOrEmpty(result))
                    {
                        var azureEnv = await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.AzureEnvironmentRegVal, context.CancellationToken, AzureEnvironmentConstants.AzureCloud).ConfigureAwait(false);
                        AzureEnvironment.TryGetEndpointString(endpoint, azureEnv, out result);
                    }
                }

                if (!string.IsNullOrEmpty(result))
                {
                    CacheInstance.Set(endpoint, result, isheadNode ? ExpireTimeForHn : ExpireTimeForClient);
                }

                return result;
            }
            else
            {
                return (string)cacheValue;
            }
        }
    }
}
