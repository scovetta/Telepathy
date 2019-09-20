// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Azure
{
    internal class BrokerProxyEndpointNames
    {
        public const string SoaBrokerProxy = "Microsoft.Hpc.Azure.Endpoint.BrokerProxy";

        public const string SoaProxyControl = "Microsoft.Hpc.Azure.Endpoint.ProxyControl";

        private const string SharedEndpointName = "Microsoft.Hpc.Azure.Endpoint.HpcComponents";

        public static string SoaBrokerProxyEndpointV4RTM
        {
            get
            {
                return SharedEndpointName;
            }
        }

        public static string SoaProxyControlEndpointV4RTM
        {
            get
            {
                return SharedEndpointName;
            }
        }
    }
}
