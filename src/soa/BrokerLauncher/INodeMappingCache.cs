//------------------------------------------------------------------------------
// <copyright file="INodeMappingCache.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Interface for retrieving Azure node mapping information
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System.Collections.Generic;
    using System.ServiceModel;

    /// <summary>
    /// Interface for retrieving Azure node mapping data
    /// </summary>
    [ServiceContract(Name = "INodeMappingCache", Namespace = "http://hpc.microsoft.com/brokerlauncher/")]
    interface INodeMappingCache
    {
        /// <summary>
        /// Get Azure node mapping data
        /// </summary>
        /// <param name="fromCache">if return node mapping data from cache or not (querying node mapping table) </param>
        /// <returns>A copy of Azure node mapping data</returns>
        [OperationContract]
        Dictionary<string, string> GetNodeMapping(bool fromCache);
    }
}