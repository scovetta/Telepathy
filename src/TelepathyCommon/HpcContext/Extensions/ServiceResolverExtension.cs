using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelepathyCommon.HpcContext.Extensions
{
    /// <summary>
    /// This extends the QueryClient for service resolving.
    /// These extension methods need the QueryClient as a parameter, so avoids the implicit new FabricClient() call.
    /// It's not recommended to use new FabricClient().QueryManager.SomeMethodHere(), but try to pass the fabric client through parameters.
    /// </summary>
    public static class ServiceResolverExtension
    {
        #region Service Uris
        public static readonly string HpcApplicationName = "HpcApplication";
        public static readonly string UriTemplate = "fabric:/HpcApplication/{0}";

        public static readonly string SchedulerStatefulService = "SchedulerStatefulService";
        public static readonly string SessionLauncherStatefulService = "SessionLauncherStatefulService";
        public static readonly string BrokerLauncherStatefulService = "BrokerLauncherStatefulService";
        public static readonly string ManagementStatelessService = "ManagementStatelessService";
        public static readonly string ManagementStatefulService = "ManagementStatefulService";
        public static readonly string SdmStatefulService = "SdmStatefulService";
        public static readonly string MonitoringStatefulService = "MonitoringStatefulService";
        public static readonly string DiagnosticsStatefulService = "DiagnosticsStatefulService";
        public static readonly string FrontendStatelessService = "FrontendStatelessService";

        #endregion

        // We only return IpAddressOrFQDN for the service nodes which is the high frequency usage.
        // For additional properties of the Node, use the methods in Common region.
        #region Hpc Services Resolver methods

        public static async Task<string> ResolveSchedulerNodeAsync(this ITelepathyContext context)
        {
            return await context.FabricContext.ResolveSingletonServicePrimaryAsync(SchedulerStatefulService, context.CancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> ResolveSessionLauncherNodeAsync(this ITelepathyContext context)
        {
            return await context.FabricContext.ResolveSingletonServicePrimaryAsync(SessionLauncherStatefulService, context.CancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> ResolveBrokerLauncherNodeAsync(this ITelepathyContext context)
        {
            return await context.FabricContext.ResolveSingletonServicePrimaryAsync(BrokerLauncherStatefulService, context.CancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> ResolveManagementNodeAsync(this ITelepathyContext context)
        {
            return await context.ResolveStatelessServiceNodeAsync(ManagementStatelessService).ConfigureAwait(false);
        }

        public static async Task<string> ResolveFrontendNodeAsync(this ITelepathyContext context)
        {
            return await context.ResolveStatelessServiceNodeAsync(FrontendStatelessService).ConfigureAwait(false);
        }

        private static async Task<string> ResolveStatelessServiceNodeAsync(this ITelepathyContext context, string serviceName)
        {
            IEnumerable<string> nodes = await context.FabricContext.ResolveStatelessServiceNodesAsync(serviceName, context.CancellationToken).ConfigureAwait(false);
            if (nodes.Count() == 0)
            {
                throw new InvalidOperationException("HpcManagementStateless service is not found");
            }

            Random rand = new Random();
            return nodes.ElementAt(rand.Next(nodes.Count()));
        }

        public static async Task<string> ResolveManagementStatefulNodeAsync(this ITelepathyContext context)
        {
            return await context.FabricContext.ResolveSingletonServicePrimaryAsync(ManagementStatefulService, context.CancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> ResolveSdmNodeAsync(this ITelepathyContext context)
        {
            return await context.FabricContext.ResolveSingletonServicePrimaryAsync(SdmStatefulService, context.CancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> ResolveMonitoringNodeAsync(this ITelepathyContext context)
        {
            return await context.FabricContext.ResolveSingletonServicePrimaryAsync(MonitoringStatefulService, context.CancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> ResolveDiagnosticsNodeAsync(this ITelepathyContext context)
        {
            return await context.FabricContext.ResolveSingletonServicePrimaryAsync(DiagnosticsStatefulService, context.CancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}
