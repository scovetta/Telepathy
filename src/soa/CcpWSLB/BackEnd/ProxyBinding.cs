// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Security;
    using System.ServiceModel;

    /// <summary>
    /// Binding info for broker and proxy communication
    /// </summary>
    internal static class ProxyBinding
    {
        /// <summary>
        /// Stores the maximum buffer size for broker proxy binding
        /// </summary>
        private const int MaxBufferSize = 512 * 1024 * 1024;

        /// <summary>
        /// Stores the maimum number of connections configuration for broker proxy binding
        /// </summary>
        private const int MaxConnections = 4096;

        /// <summary>
        /// Stores the receive timeout configuration for broker proxy binding.
        /// </summary>
        /// <remarks>
        /// The Azure load balancer kills connection after about 5 min idle time.
        /// We have heart beat message on each connection to avoid a long idle time.
        /// </remarks>
        private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// This binding is used for calculation messages on nettcp connection.
        /// </summary>
        private static readonly NetTcpBinding brokerProxyBinding;

        /// <summary>
        /// This binding is used for control messages on nettcp connection.
        /// </summary>
        private static readonly NetTcpBinding brokerProxyControllerBinding;

        /// <summary>
        /// This binding is used for control messages on http connection.
        /// </summary>
        private static readonly BasicHttpBinding brokerProxyControllerHttpBinding;

        /// <summary>
        /// Initializes static members of the ProxyBinding class
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Needs to do some logic when initializing the default bindings.")]
        static ProxyBinding()
        {
            brokerProxyBinding = new NetTcpBinding(SecurityMode.Transport, false);
            brokerProxyBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            brokerProxyBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;

            brokerProxyBinding.MaxBufferPoolSize = MaxBufferSize;
            brokerProxyBinding.MaxBufferSize = MaxBufferSize;
            brokerProxyBinding.MaxReceivedMessageSize = MaxBufferSize;
            brokerProxyBinding.MaxConnections = MaxConnections;

            brokerProxyBinding.ReceiveTimeout = ReceiveTimeout;

            brokerProxyBinding.ReaderQuotas.MaxStringContentLength = MaxBufferSize;
            brokerProxyBinding.ReaderQuotas.MaxArrayLength = MaxBufferSize;

            brokerProxyControllerBinding = new NetTcpBinding(SecurityMode.Transport, false);
            brokerProxyControllerBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            brokerProxyControllerBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;

            brokerProxyControllerHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            brokerProxyControllerHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
        }

        /// <summary>
        /// Gets the broker proxy binding
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding BrokerProxyBinding
        {
            get { return ProxyBinding.brokerProxyBinding; }
        }

        /// <summary>
        /// Gets the broker proxy controller nettcp binding
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding BrokerProxyControllerBinding
        {
            get { return ProxyBinding.brokerProxyControllerBinding; }
        }

        /// <summary>
        /// Gets the broker proxy controller http binding
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static BasicHttpBinding BrokerProxyControllerHttpBinding
        {
            get { return ProxyBinding.brokerProxyControllerHttpBinding; }
        }
    }
}
