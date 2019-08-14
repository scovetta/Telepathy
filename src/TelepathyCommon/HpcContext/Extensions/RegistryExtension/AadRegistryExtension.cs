namespace Microsoft.Hpc
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Rest;

    public static class AadRegistryExtension
    {
        public static async Task<bool> IsSupportAAD(this IHpcContext context)
        {
            bool supportAAD;
            if (bool.TryParse(await context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.SupportAad, context.CancellationToken).ConfigureAwait(false), out supportAAD))
            {
                return supportAAD;
            }

            return false;
        }

        public static async Task<string> GetAADTenantAsync(this IHpcContext context, string aadInfoNode = null)
            =>
                await
                    context.GetAADInfoAux(
                        () => context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.AADTenant, context.CancellationToken),
                        c => c.GetAadTenantAsync(context.CancellationToken),
                        aadInfoNode).ConfigureAwait(false);

        public static async Task<string> GetAADTenantIdAsync(this IHpcContext context, string aadInfoNode = null)
            =>
                await
                    context.GetAADInfoAux(
                        () => context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.AADTenantId, context.CancellationToken),
                        c => c.GetAadTenantIdAsync(context.CancellationToken),
                        aadInfoNode).ConfigureAwait(false);

        public static async Task<string> GetAADInstanceAsync(this IHpcContext context, string aadInfoNode = null)
            =>
                await
                    context.GetAADInfoAux(
                        () => context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.AADInstance, context.CancellationToken),
                        c => c.GetAadInstanceAsync(context.CancellationToken),
                        aadInfoNode).ConfigureAwait(false);

        public static async Task<string> GetAADAppNameAsync(this IHpcContext context, string aadInfoNode = null)
            =>
                await
                    context.GetAADInfoAux(
                        () => context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.AADAppName, context.CancellationToken),
                        c => c.GetAadAppNameAsync(context.CancellationToken),
                        aadInfoNode).ConfigureAwait(false);

        public static async Task<string> GetAADClientAppIdAsync(this IHpcContext context, string aadInfoNode = null)
            =>
                await
                    context.GetAADInfoAux(
                        () => context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.AADClientAppId, context.CancellationToken),
                        c => c.GetAadClientAppIdAsync(context.CancellationToken),
                        aadInfoNode).ConfigureAwait(false);

        public static async Task<string> GetAADClientAppKeyAsync(this IHpcContext context, string aadInfoNode = null)
            =>
                await
                    context.GetAADInfoAux(
                        () => context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.AADClientAppKey, context.CancellationToken),
                        c => c.GetAadClientAppKeyAsync(context.CancellationToken),
                        aadInfoNode).ConfigureAwait(false);

        public static async Task<string> GetAADClientAppRedirectUriAsync(this IHpcContext context, string aadInfoNode = null)
            =>
                await
                    context.GetAADInfoAux(
                        () => context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.AADClientAppRedirectUri, context.CancellationToken),
                        c => c.GetAadClientRedirectUrlAsync(context.CancellationToken),
                        aadInfoNode).ConfigureAwait(false);

        private static async Task<T> GetAADInfoAux<T>(this IHpcContext context, Func<Task<T>> funcRegistry, Func<HpcAadInfoRestClient, Task<T>> funcRest, string aadInfoNode)
        {
            if (context.FabricContext.IsHpcHeadNodeService())
            {
                return await funcRegistry().ConfigureAwait(false);
            }
            else
            {
                HpcAadInfoRestClient client;

                if (string.IsNullOrEmpty(aadInfoNode))
                {
                    client = new HpcAadInfoRestClient(context);
                }
                else
                {
                    client = new HpcAadInfoRestClient(aadInfoNode);
                }

                return await funcRest(client).ConfigureAwait(false);
            }
        }
    }
}