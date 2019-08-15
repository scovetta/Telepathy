using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TelepathyCommon.HpcContext;

namespace TelepathyCommon.Rest
{
    public class HpcAadInfoRestClient : HpcRestClient
    {
        private const string ClientAppIdRoute = "client-appid";
        private const string ClientAppKeyRoute = "client-appkey";
        private const string ClientRedirectUrlRoute = "client-redirect-url";
        private const string TenantRoute = "tenant";
        private const string TenantIdRoute = "tenant-id";
        private const string AppNameRoute = "appname";
        private const string InstanceRoute = "aadinstance";

        private static readonly object InitLock = new object();
        private static readonly object RequestCacheUpdateLock = new object();
        private static readonly Dictionary<string, Dictionary<string, Lazy<Task<string>>>> RequestCacheMatrix = new Dictionary<string, Dictionary<string, Lazy<Task<string>>>>();
        private Dictionary<string, Lazy<Task<string>>> instanceRequestCacheTable;

        public HpcAadInfoRestClient(ITelepathyContext context) : base(context)
        {
            this.Init(context.FabricContext.ConnectionString.ConnectionString);
        }

        public HpcAadInfoRestClient(string restNode) : base(restNode)
        {
            this.Init(restNode);
        }

        private static readonly HttpClient HttpClientCache = DefaultHttpClientFactory();

        protected override sealed HttpClient RestClient => HttpClientCache;

        protected override sealed string RestClientName => nameof(HpcAadInfoRestClient);

        protected override sealed string ApiRoot => "api/aadinfo/";

        public async Task<string> GetAadClientAppIdAsync(CancellationToken token = default(CancellationToken)) => await this.GetResultFromCachedRequest(ClientAppIdRoute).ConfigureAwait(false);

        public async Task<string> GetAadClientAppKeyAsync(CancellationToken token = default(CancellationToken)) => await this.GetResultFromCachedRequest(ClientAppKeyRoute).ConfigureAwait(false);

        public async Task<string> GetAadClientRedirectUrlAsync(CancellationToken token = default(CancellationToken)) => await this.GetResultFromCachedRequest(ClientRedirectUrlRoute).ConfigureAwait(false);

        public async Task<string> GetAadTenantAsync(CancellationToken token = default(CancellationToken)) => await this.GetResultFromCachedRequest(TenantRoute).ConfigureAwait(false);

        public async Task<string> GetAadTenantIdAsync(CancellationToken token = default(CancellationToken)) => await this.GetResultFromCachedRequest(TenantIdRoute).ConfigureAwait(false);

        public async Task<string> GetAadAppNameAsync(CancellationToken token = default(CancellationToken)) => await this.GetResultFromCachedRequest(AppNameRoute).ConfigureAwait(false);

        public async Task<string> GetAadInstanceAsync(CancellationToken token = default(CancellationToken)) => await this.GetResultFromCachedRequest(InstanceRoute).ConfigureAwait(false);

        private void Init(string cacheKey)
        {
            if (!RequestCacheMatrix.ContainsKey(cacheKey))
            {
                lock (InitLock)
                {
                    Thread.MemoryBarrier();
                    if (!RequestCacheMatrix.ContainsKey(cacheKey))
                    {
                        // Create request cache table
                        var table = new Dictionary<string, Lazy<Task<string>>>();
                        table[ClientAppIdRoute] = new Lazy<Task<string>>(() => this.GetHttpApiCallAsync<string>(CancellationToken.None, ClientAppIdRoute));
                        table[ClientAppKeyRoute] = new Lazy<Task<string>>(() => this.GetHttpApiCallAsync<string>(CancellationToken.None, ClientAppKeyRoute));
                        table[ClientRedirectUrlRoute] = new Lazy<Task<string>>(() => this.GetHttpApiCallAsync<string>(CancellationToken.None, ClientRedirectUrlRoute));
                        table[TenantRoute] = new Lazy<Task<string>>(() => this.GetHttpApiCallAsync<string>(CancellationToken.None, TenantRoute));
                        table[TenantIdRoute] = new Lazy<Task<string>>(() => this.GetHttpApiCallAsync<string>(CancellationToken.None, TenantIdRoute));
                        table[AppNameRoute] = new Lazy<Task<string>>(() => this.GetHttpApiCallAsync<string>(CancellationToken.None, AppNameRoute));
                        table[InstanceRoute] = new Lazy<Task<string>>(() => this.GetHttpApiCallAsync<string>(CancellationToken.None, InstanceRoute));

                        RequestCacheMatrix[cacheKey] = table;
                    }
                }
            }

            this.instanceRequestCacheTable = RequestCacheMatrix[cacheKey];
        }

        private async Task<string> GetResultFromCachedRequest(string routeString)
        {
            Lazy<Task<string>> requestCache = null;
            try
            {
                requestCache = this.instanceRequestCacheTable[routeString];
                return await requestCache.Value.ConfigureAwait(false);
            }
            catch
            {
                Debug.Assert(requestCache != null, $"{nameof(this.instanceRequestCacheTable)} is not correctly initialized.");
                if (this.instanceRequestCacheTable[routeString] == requestCache)
                {
                    lock (RequestCacheUpdateLock)
                    {
                        Thread.MemoryBarrier();
                        if (this.instanceRequestCacheTable[routeString] == requestCache)
                        {
                            // By doing lazy value replacing, we will cache up to api number of HpcAadInfoRestClient instances, which should be ok as most 
                            // fields in this class is shared or cached.
                            this.instanceRequestCacheTable[routeString] = new Lazy<Task<string>>(() => this.GetHttpApiCallAsync<string>(CancellationToken.None, routeString));
                        }
                    }
                }

                throw;
            }
        }
    }
}
