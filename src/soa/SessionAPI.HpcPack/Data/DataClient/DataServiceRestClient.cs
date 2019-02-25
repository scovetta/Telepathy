namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Rest;
    using Microsoft.Hpc.Scheduler.Session.Data.DTO;

    internal class DataServiceRestClient : HpcRestClient
    {
        public DataServiceRestClient(IHpcContext context) : base(context)
        {
        }

        private static readonly HttpClient HttpClientCache = DefaultHttpClientFactory();
        
        protected override HttpClient RestClient => HttpClientCache;

        protected override string RestClientName => nameof(DataServiceRestClient);

        protected override string ApiRoot => "api/data-client";

        protected override string AppRoot => HpcConstants.HpcDataServiceAppRoot;

        protected override async Task<string> ResolveRestNodeAsync() => await this.Context.ResolveSessionLauncherNodeAsync().ConfigureAwait(false) + ":" + HpcConstants.HpcDataServicePort;

        public async Task<DataClientInfo> OpenDataClientBySecret(string dataClientId, int jobId, string jobSecret, CancellationToken token = default(CancellationToken)) 
            => await this.PostHttpApiCallAsync<DataClientInfo, OpenDataClientBySecretParams>(token, "open-by-secret", new OpenDataClientBySecretParams(dataClientId, jobId, jobSecret)).ConfigureAwait(false);

    }
}
