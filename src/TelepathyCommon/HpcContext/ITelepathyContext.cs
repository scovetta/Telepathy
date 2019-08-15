using System.Threading;
using TelepathyCommon.Registry;

namespace TelepathyCommon.HpcContext
{
    public interface ITelepathyContext
    {
        CancellationToken CancellationToken { get; }

        IFabricContext FabricContext { get; }

        IRegistry Registry { get; }
    }
}
