// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.Rest.Server
{
    using Microsoft.Owin.Security;

    public class ClientCertificateAuthenticationOptions : AuthenticationOptions
    {
        public ClientCertificateAuthenticationOptions() : base("X.509")
        {
        }
    }
}