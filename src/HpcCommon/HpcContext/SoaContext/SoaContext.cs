namespace Microsoft.Hpc.SoaContext
{
    using System.Threading;

    // TODO: Remove me
    public class SoaContext : IHpcContext
    {
        public static IHpcContext Instance = new SoaContext();

        public CancellationToken CancellationToken { get; }

        public IFabricContext FabricContext => SoaFabricContext.Instance;

        public IRegistry Registry { get; }
    }
}
