// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerShim
{
    using System.ServiceModel;

    using Microsoft.Telepathy.Session.Common;

    /// <summary>
    /// Authroization manager for HpcBrokerWorker
    /// </summary>
    class HpcBrokerWorkerAuthorizationManager : ServiceAuthorizationManager
    {
        /// <summary>
        /// Check access for incoming call
        /// </summary>
        /// <param name="operationContext">indicating the operation context</param>
        /// <returns>returns if incoming user is system</returns>
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            if (SoaHelper.IsOnAzure())
            {
                return true;
            }

            // Bug 6152: Only allow system account to use this service
            return operationContext.ServiceSecurityContext.WindowsIdentity.IsSystem;
        }
    }
}
