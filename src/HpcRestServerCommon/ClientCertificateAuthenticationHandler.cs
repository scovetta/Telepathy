// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Infrastructure;

    public class ClientCertificateAuthenticationHandler : AuthenticationHandler<ClientCertificateAuthenticationOptions>
    {
        private readonly IClientCertificateValidator clientCertificateValidator;
        private readonly string owinClientCertKey = "ssl.ClientCertificate";

        public ClientCertificateAuthenticationHandler(IClientCertificateValidator clientCertificateValidator)
        {
            if (clientCertificateValidator == null)
            {
                throw new ArgumentNullException(nameof(clientCertificateValidator));
            }

            this.clientCertificateValidator = clientCertificateValidator;
        }

        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            ClientCertificateValidationResult validationResult = this.ValidateCertificate(this.Request.Environment);
            if (validationResult.CertificateValid)
            {
                AuthenticationProperties authProperties =
                    new AuthenticationProperties
                        {
                            IssuedUtc = DateTime.UtcNow,
                            ExpiresUtc = DateTime.UtcNow.AddDays(1),
                            AllowRefresh = true,
                            IsPersistent = true
                        };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(new List<Claim> { }, "X.509");
                AuthenticationTicket ticket = new AuthenticationTicket(claimsIdentity, authProperties);
                return Task.FromResult(ticket);
            }

            return Task.FromResult<AuthenticationTicket>(null);
        }

        private ClientCertificateValidationResult ValidateCertificate(IDictionary<string, object> owinEnvironment)
        {
            if (owinEnvironment.ContainsKey(this.owinClientCertKey))
            {
                X509Certificate2 clientCert = this.Context.Get<X509Certificate2>(this.owinClientCertKey);
                return this.clientCertificateValidator.Validate(clientCert);
            }

            ClientCertificateValidationResult invalid = new ClientCertificateValidationResult(false);
            invalid.AddValidationException("There's no client certificate attached to the request.");
            return invalid;
        }
    }
}
