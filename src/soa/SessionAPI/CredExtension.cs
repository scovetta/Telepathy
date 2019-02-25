//------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      WCF credential extensions
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Threading.Tasks;

    using Internal;

#if !net40
    using Microsoft.Hpc.AADAuthUtil;

#endif

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
            IRegistry registry = winService ? Registry : HpcContext.GetOrAdd(cancellationToken).Registry;
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
            IRegistry registry = winService ? Registry : HpcContext.GetOrAdd(cancellationToken).Registry;
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
        public static async Task<string> ResolveSessionLauncherNodeOnIaasAsync(this IHpcContext context, string headnode)
        {
            string sessionLauncher = await context.ResolveSessionLauncherNodeAsync().ConfigureAwait(false);

            if (SoaHelper.IsSchedulerOnIaaS(headnode))
            {
                string suffix = SoaHelper.GetSuffixFromHeadNodeEpr(headnode);
                sessionLauncher += suffix;
            }

            return sessionLauncher;
        }

#if !net40
        /// <summary>
        /// Add <see cref="AADClientEndpointBehavior"/> to behavior list
        /// </summary>
        /// <param name="behaviors"></param>
        /// <param name="headnode"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<KeyedByTypeCollection<IEndpointBehavior>> UseAadClientBehaviors(
            this KeyedByTypeCollection<IEndpointBehavior> behaviors,
            string headnode,
            string username = null,
            string password = null)
        {
            // If this headnode is an Azure IaaS cluster, we use AadIntegrationService on headnode instead of resolving it again
            var token = await GetSoaAadJwtToken(headnode, username, password).ConfigureAwait(false);
            behaviors.Add(new AADClientEndpointBehavior(token));
            return behaviors;
        }

        /// <summary>
        /// Add <see cref="AADClientEndpointBehavior"/> to behavior list
        /// </summary>
        /// <param name="behaviors"></param>
        /// <param name="sessionInitInfo"></param>
        /// <returns></returns>
        public static Task<KeyedByTypeCollection<IEndpointBehavior>> UseAadClientBehaviors(this KeyedByTypeCollection<IEndpointBehavior> behaviors, SessionInitInfoBase sessionInitInfo) =>
            behaviors.UseAadClientBehaviors(sessionInitInfo.Headnode, sessionInitInfo.Username, sessionInitInfo.InternalPassword);
        
        /// <summary>
        /// This method will query head node for cluster AAD info if cluster is an Azure IaaS cluster. 
        /// </summary>
        /// <param name="headnode">Headnode address of cluster headnode</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<string> GetSoaAadJwtToken(string headnode, string username, string password)
        {
            IHpcContext context = HpcContext.GetOrAdd(headnode, CancellationToken.None);

            string node = null;
            if (SoaHelper.IsSchedulerOnIaaS(headnode))
            {
                node = await context.ResolveSessionLauncherNodeOnIaasAsync(headnode).ConfigureAwait(false);
            }

            string token = await context.GetAADJwtTokenAsync(username, password, node).ConfigureAwait(false);
            return token;
        }
#endif
    }
}
