namespace Microsoft.Hpc
{
    using global::Owin;

    public static class ClientCertificateAuthenticationExtensions
    {
        public static void UseClientCertificateAuthentication(this IAppBuilder appBuilder, IClientCertificateValidator clientCertificateValidator)
        {
            appBuilder.Use<ClientCertificateAuthMiddleware>(new ClientCertificateAuthenticationOptions(), clientCertificateValidator);
        }
    }
}
