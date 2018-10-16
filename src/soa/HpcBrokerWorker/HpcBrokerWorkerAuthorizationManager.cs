//------------------------------------------------------------------------------
// <copyright file="HpcBrokerWorkerAuthorizationManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Authroization manager for HpcBrokerWorker
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerShim
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ServiceModel;
    using Microsoft.Hpc.BrokerProxy;

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
