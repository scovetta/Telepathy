// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using System;

    /// <summary>
    /// Represents HpcSession's performance counter key
    /// </summary>
    [Serializable]
    internal enum SessionPerformanceCounterKey
    {
        /// <summary>
        /// Inidcate this should not be included in perf counter
        /// </summary>
        None = 0,

        /// <summary>
        /// Total number of failover broker nodes
        /// </summary>
        TotalFailoverBrokerNodeCount,

        /// <summary>
        /// Total number of failover clusters within broker node tier
        /// </summary>
        TotalFailoverClusterCount,

        /// <summary>
        /// Total number of active broker nodes (with and without failover)
        /// </summary>
        ActiveBrokerNodeCount,

        /// <summary>
        /// Total number of active broker resource groups
        /// </summary>
        ActiveBrokerResourceGroupCount
    }
}
