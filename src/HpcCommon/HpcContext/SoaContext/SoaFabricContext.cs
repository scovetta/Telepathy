namespace Microsoft.Hpc.SoaContext
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    // TODO: remove me
    public class SoaFabricContext : IFabricContext
    {
        public static IFabricContext Instance = new SoaFabricContext();

        private static readonly string LocalHost = "localhost";

        private static readonly string[] LocalHostArr = new[] { LocalHost };

        public async Task<string> ResolveSingletonServicePrimaryAsync(string serviceName, CancellationToken token)
        {
            return LocalHost;
        }

        public async Task<IEnumerable<string>> ResolveStatelessServiceNodesAsync(string serviceName, CancellationToken token)
        {
            return LocalHostArr;
        }

        public async Task<IEnumerable<string>> GetNodesAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IRegistry Registry => throw new NotImplementedException();

        public EndpointsConnectionString ConnectionString => EndpointsConnectionString.ParseConnectionString(LocalHost);

        public HpcContextOwner Owner => HpcContextOwner.HpcServiceOutOfSFCluster;
    }
}
