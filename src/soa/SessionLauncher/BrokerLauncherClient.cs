//------------------------------------------------------------------------------
// <copyright file="BrokerLauncherClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The Broker Launcher Client
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.ServiceBroker;
    using System.ServiceModel.Security;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// The Broker Launcher Client
    /// </summary>
    internal class BrokerLauncherClient : BrokerLauncherClientBase
    {

        /// <summary>
        /// Initializes a new instance of the BrokerLauncherClient class.
        /// </summary>
        /// <param name="uri">The broker launcher EPR</param>
        public BrokerLauncherClient(Uri uri, string certThrumbprint)
            : base(GetBinding(uri), GetEndpoint(uri, certThrumbprint))
        {
            string thumbpint = HpcContext.Get().GetSSLThumbprint().GetAwaiter().GetResult();
            this.ClientCredentials.UseInternalAuthentication(thumbpint);

            if (!SoaHelper.IsOnAzure())
            {
                this.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            }

        }

        /// <summary>
        /// Create a bind from the configuration, if not exists, use default one.
        /// </summary>
        /// <returns>The binding</returns>
        private static Binding GetBinding(Uri uri)
        {
            if (uri.Scheme.Equals(HttpsScheme, StringComparison.InvariantCultureIgnoreCase))
            {
                return BindingHelper.HardCodedBrokerLauncherHttpsBinding;
            }

            return BindingHelper.HardCodedInternalBrokerLauncherNetTcpBinding;
        }

        /// <summary>
        /// Create the endpoint of broker
        /// </summary>
        /// <param name="uri">Uri address</param>
        /// <returns>Endpoint Address</returns>
        private static EndpointAddress GetEndpoint(Uri uri, string certThrumbprint)
        {
            return SoaHelper.CreateInternalCertEndpointAddress(uri, certThrumbprint);
        }
    }
}
