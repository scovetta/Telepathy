namespace Microsoft.Hpc.SoaContext
{
    using System.Threading;

    // TODO: Remove me
    public class SoaContext : IHpcContext
    {
        public static readonly IHpcContext Default = new SoaContext();

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
