namespace TelepathyCommon.HpcContext
{
    internal enum HpcServerContextType
    {
        Undefined,
        ServiceFabric,
        NtService
    }

    public static class HpcServerContextFabricContextExtension
    {
        public static bool IsHpcHeadNodeService(this IFabricContext fabricContext) 
            => fabricContext.Owner == HpcContextOwner.HpcServiceInSFCluster
               || (fabricContext.Owner == HpcContextOwner.HpcServiceOutOfSFCluster && TelepathyContext.ServerContextType == HpcServerContextType.NtService);
    }
}
