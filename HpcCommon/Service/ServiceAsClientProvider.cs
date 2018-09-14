namespace Microsoft.Hpc
{
#if !net40
    using System.Security.Claims;
#endif
    using System.Security.Principal;

    public delegate string ServiceAsClientIdentityProvider();

    public delegate IPrincipal ServiceAsClientPrincipalProvider();

    public static class ServiceAsClientProvider
    {
        public static string DefaultIdentityProvider()
        {
            if (WindowsIdentity.GetCurrent() == null || WindowsIdentity.GetCurrent().IsSystem)
            {
                return null;
            }

            return WindowsIdentity.GetCurrent().Name;
        }

#if !net40
        public static IPrincipal DefaultAadPrincipalProvider(IPrincipal principal)
        {
            if (principal == null || !principal.IsHpcAadPrincipal()) return null;

            return (ClaimsPrincipal)principal;
        }
#endif
    }
}
