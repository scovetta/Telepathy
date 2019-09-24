// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.Rest.Server
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
