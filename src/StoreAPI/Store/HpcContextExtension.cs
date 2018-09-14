namespace Microsoft.Hpc.Scheduler.Store
{
    using System.Threading.Tasks;
    using Properties;

    public static class HpcContextExtension
    {
        public static Task<ISchedulerStore> GetSchedulerStoreAsync(this IHpcContext context)
        {
            return SchedulerStore.ConnectAsync(new StoreConnectionContext(context), context.CancellationToken);
        }

        public static Task<ISchedulerStore> GetSchedulerStoreAsync(this IHpcContext context, int port)
        {
            return SchedulerStore.ConnectAsync(new StoreConnectionContext(context) { Port = port }, context.CancellationToken);
        }

        public static Task<ISchedulerStore> GetSchedulerStoreAsync(this IHpcContext context, bool isOverHttp)
        {
            return SchedulerStore.ConnectAsync(new StoreConnectionContext(context) { IsHttp = isOverHttp }, context.CancellationToken);
        }

        public static Task<ISchedulerStore> GetSchedulerStoreAsync(this IHpcContext context, ServiceAsClientIdentityProvider identityProvider, ServiceAsClientPrincipalProvider principalProvider = null)
        {
            return SchedulerStore.ConnectAsync(new StoreConnectionContext(context) { ServiceAsClient = true, IdentityProvider = identityProvider, PrincipalProvider = principalProvider }, context.CancellationToken);
        }
    }
}
