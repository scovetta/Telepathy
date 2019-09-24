// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    /// <summary>
    /// Utility functions for the service host port.
    /// </summary>
    public static class PortHelper
    {
        /// <summary>
        /// Convert the offset to the valid port, and need to support oversubscription.
        /// Still use 9100~9199 for the service host; use 9200~9299 for the service controller.
        /// So we can support back-compact when HN and CN are not upgraded at the same time.
        /// For more than 100 cores (coreId >=100), in oversubscription scenario:
        /// service host port: 9300 + (coreId - 100) * 2 
        /// service controller port: 9301 + (coreId - 100) * 2
        /// </summary>
        /// <param name="coreId">port offset</param>
        /// <returns>value of port</returns>
        public static int ConvertToPort(int coreId, bool controller)
        {
            if (coreId >= Constant.ServiceHostPortDiff)
            {
                int basePort = controller ? Constant.ServiceHostControllerBasePort : Constant.ServiceHostBasePort;
                return basePort + (coreId - Constant.ServiceHostPortDiff) * 2;
            }
            else
            {
                int basePort = controller ? Constant.ServiceHostControllerPort : Constant.ServiceHostPort;
                return basePort + coreId;
            }
        }
    }
}
