namespace TelepathyCommon.HpcContext
{
    using System.Threading;

    using TelepathyCommon.Registry;

    public interface ITelepathyContext
    {
        CancellationToken CancellationToken { get; }

        IFabricContext FabricContext { get; }

        IRegistry Registry { get; }
    }
}