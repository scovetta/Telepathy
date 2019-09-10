//--------------------------------------------------------------------------
// <copyright file="BrokerProxyEndpointNames.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This is a common file for Azure broker proxy endpoint names.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.Common
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
