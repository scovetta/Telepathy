using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TelepathyCommon.HpcContext;

namespace TelepathyCommon.Rest
{
    public class HpcBatchRestClient : HpcRestClient
    {
        public HpcBatchRestClient(ITelepathyContext context) : base(context)
        {
        }

        public HpcBatchRestClient(string restNode) : base(restNode)
        {
        }

        private static readonly HttpClient HttpClientCache = DefaultHttpClientFactory();

        protected override sealed HttpClient RestClient => HttpClientCache;

        protected override sealed string RestClientName => nameof(HpcBatchRestClient);

        protected override sealed string ApiRoot => "api/batch/";

        public async Task<string> GetAadInstanceAsync(CancellationToken token = default(CancellationToken)) => await this.GetHttpApiCallAsync<string>(token, "aadinstance").ConfigureAwait(false);

        public async Task<string> GetAadTenantIdAsync(CancellationToken token = default(CancellationToken)) => await this.GetHttpApiCallAsync<string>(token, "tenant-id").ConfigureAwait(false);

        public async Task<string> GetAadClientAppIdAsync(CancellationToken token = default(CancellationToken)) => await this.GetHttpApiCallAsync<string>(token, "client-appid").ConfigureAwait(false);

        public async Task<string> GetAadClientAppKeyAsync(CancellationToken token = default(CancellationToken)) => await this.GetHttpApiCallAsync<string>(token, "client-appkey").ConfigureAwait(false);

    }
}
