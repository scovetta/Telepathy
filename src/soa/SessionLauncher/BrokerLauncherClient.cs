// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using System;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Telepathy.Common.TelepathyContext;
    using Microsoft.Telepathy.Common.TelepathyContext.Extensions.RegistryExtension;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Internal;

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
            string thumbpint = TelepathyContext.Get().GetSSLThumbprint().GetAwaiter().GetResult();
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
