// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace TelepathyCommon.HpcContext.Extensions
{
    using System.Threading.Tasks;

    /// <summary>
    ///     This extends the QueryClient for service resolving.
    ///     These extension methods need the QueryClient as a parameter, so avoids the implicit new FabricClient() call.
    ///     It's not recommended to use new FabricClient().QueryManager.SomeMethodHere(), but try to pass the fabric client
    ///     through parameters.
    /// </summary>
    public static class ServiceResolverExtension
    {
        private static readonly string SessionLauncherStatefulService = "SessionLauncherStatefulService";

        public static async Task<string> ResolveSessionLauncherNodeAsync(this ITelepathyContext context)
        {
            return await context.FabricContext.ResolveSingletonServicePrimaryAsync(SessionLauncherStatefulService, context.CancellationToken).ConfigureAwait(false);
        }

        // We only return IpAddressOrFQDN for the service nodes which is the high frequency usage.
        // For additional properties of the Node, use the methods in Common region.
    }
}