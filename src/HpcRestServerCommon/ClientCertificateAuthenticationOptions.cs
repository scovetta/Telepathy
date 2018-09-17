namespace Microsoft.Hpc
{
    using Microsoft.Owin.Security;

    public class ClientCertificateAuthenticationOptions : AuthenticationOptions
    {
        public ClientCertificateAuthenticationOptions() : base("X.509")
        {
        }
    }
}