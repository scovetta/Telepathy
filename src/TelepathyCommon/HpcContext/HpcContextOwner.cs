namespace TelepathyCommon.HpcContext
{
    public enum HpcContextOwner
    {
        HpcServiceInSFCluster,
        HpcServiceOutOfSFCluster,
        Client
    }

    public static class HpcContextOwnerFabricContextExtension
    {
        public static bool IsHpcService(this IFabricContext fabricContext)
        {
            return fabricContext.Owner == HpcContextOwner.HpcServiceInSFCluster || fabricContext.Owner == HpcContextOwner.HpcServiceOutOfSFCluster;
        }

        public static bool UseInternalConnection(this IFabricContext fabricContext)
        {
            return fabricContext.IsHpcService() || !DomainUtil.IsInDomain();
        }
    }
}