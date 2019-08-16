using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TelepathyCommon.HpcContext;

namespace TelepathyCommon.Rest
{
    class HpcAzureEndpointRestClient : HpcRestClient
    {
        public HpcAzureEndpointRestClient(ITelepathyContext context) : base(context)
        {
        }

        public HpcAzureEndpointRestClient(string restNode) : base(restNode)
        {
        }

        private static readonly HttpClient HttpClientCache = DefaultHttpClientFactory();

        protected override sealed HttpClient RestClient => HttpClientCache;

        protected override sealed string RestClientName => nameof(HpcAzureEndpointRestClient);

        protected override sealed string ApiRoot => "api/azure/";

        public async Task<string> GetAzureEnvironmentAsync(CancellationToken token = default(CancellationToken))
        {
            var ret = string.Empty;
            var resp = await this.GetHttpApiCallAsync(token, false, this.GetApiRoute("environmentname")).ConfigureAwait(false);
            if(resp.IsSuccessStatusCode)
            {
                ret = await resp.Content.ReadAsAsync<string>().ConfigureAwait(false);
            }
            else if (resp.StatusCode != System.Net.HttpStatusCode.NotFound && resp.StatusCode != System.Net.HttpStatusCode.NotImplemented)
            {
                resp.EnsureSuccessStatusCode();
            }

            return ret;
        } 

        public async Task<string> GetAzureEndpointStringAsync(string endpoint, CancellationToken token = default(CancellationToken))
        {
            var ret = string.Empty;
            var resp = await this.GetHttpApiCallAsync(token, false, this.GetApiRoute("endpoint/{0}", endpoint)).ConfigureAwait(false);
            if (resp.IsSuccessStatusCode)
            {
                ret = await resp.Content.ReadAsAsync<string>().ConfigureAwait(false);
            }
            else if (resp.StatusCode != System.Net.HttpStatusCode.NotFound && resp.StatusCode != System.Net.HttpStatusCode.NotImplemented)
            {
                resp.EnsureSuccessStatusCode();
            }

            return ret;
        }
    }
}
