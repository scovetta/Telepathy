namespace TelepathyCommon.HpcContext
{
    using System.Threading;
    using System.Threading.Tasks;

    using TelepathyCommon.Registry;

    public interface IFabricContext
    {
        EndpointsConnectionString ConnectionString { get; }

        IRegistry Registry { get; }

        Task<string> ResolveSingletonServicePrimaryAsync(string serviceName, CancellationToken token);
    }
}