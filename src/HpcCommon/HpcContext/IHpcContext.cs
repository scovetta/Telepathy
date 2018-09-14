namespace Microsoft.Hpc
{
    using System.Threading;

    public interface IHpcContext
    {
        CancellationToken CancellationToken { get; }

        IFabricContext FabricContext { get; }

        IRegistry Registry { get; }
    }
}
