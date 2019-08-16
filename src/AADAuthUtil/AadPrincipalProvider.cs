namespace Microsoft.Hpc.AADAuthUtil
{
    using System.Security.Principal;
    using System.Threading;

    using TelepathyCommon.Service;

    public static class AadPrincipalProvider
    {
        public static IPrincipal DefaultAadPrincipalProvider()
        {
            return ServiceAsClientProvider.DefaultAadPrincipalProvider(Thread.CurrentPrincipal);
        }
    }
}
