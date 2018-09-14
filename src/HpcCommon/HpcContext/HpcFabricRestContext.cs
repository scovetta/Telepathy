namespace Microsoft.Hpc
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    
    internal class HpcFabricRestContext : IFabricContext
    {
        internal HpcFabricRestContext(EndpointsConnectionString gatewayString, HpcContextOwner hpcContextOwner)
        {
            this.ConnectionString = gatewayString;
            this.Owner = hpcContextOwner;
        }

        private readonly HttpClient restClient = new HttpClient(new HttpClientHandler { Proxy = null, UseProxy = false });

        public IRegistry Registry => new NonHARegistry();

        public EndpointsConnectionString ConnectionString { get; }

        public HpcContextOwner Owner { get; }

        public async Task<string> ResolveSingletonServicePrimaryAsync(string serviceName, CancellationToken token) =>
            await this.GetFirstHttpApiCallResponseAsync<string>(token, "resolve/singleton", serviceName).ConfigureAwait(false);

        public async Task<IEnumerable<string>> ResolveStatelessServiceNodesAsync(string serviceName, CancellationToken token) =>
            await this.GetFirstHttpApiCallResponseAsync<IEnumerable<string>>(token, "resolve/stateless", serviceName).ConfigureAwait(false);

        public async Task<IEnumerable<string>> GetNodesAsync(CancellationToken token) =>
            await this.GetFirstHttpApiCallResponseAsync<IEnumerable<string>>(token, "nodes").ConfigureAwait(false);

        private string GetApiRoute(string routeString, string parameter = "") 
            => HpcConstants.HpcNamingAppRoot.TrimEnd('/') + '/' + "/api/fabric/" + routeString.TrimEnd('/') + '/' + parameter;

        private async Task<T> GetFirstHttpApiCallResponseAsync<T>(CancellationToken cancellationToken, string routeString, string parameter = "")
        {
            using (var childCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                Task<T> firstFinishedTask =
#if net40
                    await TaskEx.WhenAny(
#else
                    await Task.WhenAny(
#endif
                        this.ConnectionString.EndPoints.Select(
                            async endpoint =>
                                {
                                    var apiRoute = this.GetApiRoute(routeString, parameter);
                                    Trace.TraceInformation("[HpcFabricRestContext] Calling {0} on {1}.", apiRoute, endpoint);
                                    var res = await this.restClient.GetHttpApiCallAsync(endpoint, apiRoute, childCts.Token).ConfigureAwait(false);
#if net40
                                    var content = await res.Content.ReadAsAsync<T>().ConfigureAwait(false);
#else
                                    var content = await res.Content.ReadAsAsync<T>(cancellationToken).ConfigureAwait(false);
#endif
                                    Trace.TraceInformation("[HpcFabricRestContext] {0} on {1} returned {2}.", apiRoute, endpoint, content);
                                    cancellationToken.ThrowIfCancellationRequested(); // Throw OperationCanceledException if parentToken is canceled

                                    return content;
                                })).ConfigureAwait(false);
                childCts.Cancel();
                return await firstFinishedTask.ConfigureAwait(false);
            }
        }
    }
}
