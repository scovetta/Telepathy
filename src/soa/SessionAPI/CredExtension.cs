// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session
{
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.Common;

    using TelepathyCommon.HpcContext;
    using TelepathyCommon.HpcContext.Extensions;
    using TelepathyCommon.HpcContext.Extensions.RegistryExtension;
    using TelepathyCommon.Registry;

    /// <summary>
    /// WCF credential extensions
    /// </summary>
    public static class ClientCredExtension
    {
        /// <summary>
        /// For the ability to access native registry without getting HPC context 
        /// which require us knowing the headnode name
        /// </summary>
        private readonly static NonHARegistry Registry = new NonHARegistry();

        /// <summary>
        /// Set client credential to use internal authentication
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ClientBase<T>> UseInternalAuthenticationAsync<T>(this ClientBase<T> client, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            Debug.Assert(client != null);
            Debug.Assert(client.ClientCredentials != null);
            await client.ClientCredentials.UseInternalAuthenticationAsync(cancellationToken).ConfigureAwait(false);
            return client;
        }

        /// <summary>
        /// Set client credential to use internal authentication
        /// </summary>
        /// <param name="clientCredentials"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ClientCredentials> UseInternalAuthenticationAsync(this ClientCredentials clientCredentials, CancellationToken cancellationToken = default(CancellationToken))
        {
            string thumbprint = await Registry.GetSSLThumbprint(cancellationToken).ConfigureAwait(false);
            return clientCredentials.UseInternalAuthentication(thumbprint);
        }

        /// <summary>
        /// Set client credential to use internal authentication
        /// </summary>
        /// <param name="clientCredentials"></param>
        /// <param name="thumbprint"></param>
        /// <returns></returns>
        public static ClientCredentials UseInternalAuthentication(this ClientCredentials clientCredentials, string thumbprint)
        {
            clientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
            clientCredentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            clientCredentials.ClientCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, thumbprint);
            return clientCredentials;
        }

        /// <summary>
        /// Set service credential to use internal authentication
        /// </summary>
        /// <param name="serviceCredentials"></param>
        /// <param name="winService"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ServiceCredentials> UseInternalAuthenticationAsync(
            this ServiceCredentials serviceCredentials,
            bool winService = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IRegistry registry = winService ? Registry : TelepathyContext.GetOrAdd().Registry;
            serviceCredentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
            serviceCredentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            serviceCredentials.ServiceCertificate.SetCertificate(
                StoreLocation.LocalMachine,
                StoreName.My,
                X509FindType.FindByThumbprint,
                await registry.GetSSLThumbprint(cancellationToken).ConfigureAwait(false));
            return serviceCredentials;
        }

        /// <summary>
        /// Set service host to use specified <see cref="ServiceAuthorizationManager"/>
        /// </summary>
        /// <param name="serviceHost"></param>
        /// <param name="authorizationManager"></param>
        /// <param name="winService"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ServiceHost> UseServiceAuthorizationManagerAsync(
            this ServiceHost serviceHost,
            ServiceAuthorizationManager authorizationManager,
            bool winService = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IRegistry registry = winService ? Registry : TelepathyContext.GetOrAdd().Registry;
            serviceHost.Credentials.ServiceCertificate.SetCertificate(
                StoreLocation.LocalMachine,
                StoreName.My,
                X509FindType.FindByThumbprint,
                await registry.GetSSLThumbprint(cancellationToken).ConfigureAwait(false));
            serviceHost.Authorization.ServiceAuthorizationManager = authorizationManager;
            return serviceHost;
        }

        /// <summary>
        /// Check if client is authenticated by Kerberos or NTLM
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static bool AuthByKerberosOrNtlmOrBasic(this IIdentity identity) =>
            identity.AuthenticationType.Equals("Kerberos", StringComparison.OrdinalIgnoreCase)
            || identity.AuthenticationType.Equals("NTLM", StringComparison.OrdinalIgnoreCase)
            || identity.AuthenticationType.Equals("Basic", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Check if the <see cref="WindowsIdentity"/> can be impersonated
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static bool CanImpersonate(this WindowsIdentity identity)
        {
            if (identity == null)
            {
                return false;
            }

            return identity.ImpersonationLevel == TokenImpersonationLevel.Delegation || identity.ImpersonationLevel == TokenImpersonationLevel.Impersonation;
        }

        /// <summary>
        /// Resolve session launcher node on IaaS
        /// </summary>
        /// <param name="context"></param>
        /// <param name="headnode"></param>
        /// <returns></returns>
        public static async Task<string> ResolveSessionLauncherNodeOnIaasAsync(this ITelepathyContext context, string headnode)
        {
            string sessionLauncher = await context.ResolveSessionLauncherNodeAsync().ConfigureAwait(false);

            if (SoaHelper.IsSchedulerOnIaaS(headnode))
            {
                string suffix = SoaHelper.GetSuffixFromHeadNodeEpr(headnode);
                sessionLauncher += suffix;
            }

            return sessionLauncher;
        }
    }
}
