using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TelepathyCommon.Registry;

namespace TelepathyCommon.HpcContext.SoaContext
{
    // TODO: remove me
    public class SoaFabricContext : IFabricContext
    {
        public static readonly IFabricContext Default = new SoaFabricContext();

        private const string LocalHost = "localhost";

        private static readonly string[] LocalHostArr = new[] { LocalHost };

        public SoaFabricContext(string connectionString) : this(EndpointsConnectionString.ParseConnectionString(connectionString))
        {
        }

        public SoaFabricContext()
        {
            this.ConnectionString = EndpointsConnectionString.ParseConnectionString(LocalHost);
        }

        public SoaFabricContext(EndpointsConnectionString connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public async Task<string> ResolveSingletonServicePrimaryAsync(string serviceName, CancellationToken token)
        {
            return this.ConnectionString.ConnectionString;
        }

        public async Task<IEnumerable<string>> ResolveStatelessServiceNodesAsync(string serviceName, CancellationToken token)
        {
            return new[] { this.ConnectionString.ConnectionString };
        }

        public async Task<IEnumerable<string>> GetNodesAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public IRegistry Registry => throw new NotImplementedException();

        public EndpointsConnectionString ConnectionString { get; }
    }
}