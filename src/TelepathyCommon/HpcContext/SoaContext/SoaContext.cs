using System.Threading;
using TelepathyCommon.Registry;

namespace TelepathyCommon.HpcContext.SoaContext
{
    // TODO: Remove me
    public class SoaContext : ITelepathyContext
    {
        public static readonly ITelepathyContext Default = new SoaContext();

        public SoaContext()
        {
            this.FabricContext = SoaFabricContext.Default;
        }

        public SoaContext(string connectionString)
        {
            this.FabricContext = new SoaFabricContext(connectionString);
        }

        public SoaContext(EndpointsConnectionString connectionString)
        {
            this.FabricContext = new SoaFabricContext(connectionString);
        }

        public CancellationToken CancellationToken { get; }

        public IFabricContext FabricContext { get; }

        public IRegistry Registry { get; } = new NonHARegistry();
    }
}
