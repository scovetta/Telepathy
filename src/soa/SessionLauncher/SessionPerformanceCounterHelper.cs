// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using System.Diagnostics;

    /// <summary>
    /// Helper class for performance counter
    /// </summary>
    internal static class SessionPerformanceCounterHelper
    {
        /// <summary>
        /// Stores the category name
        /// </summary>
        private const string SessionCategoryName = "HPC Session";

        /// <summary>
        /// Stores the name for Total Failover Broker Node Count
        /// </summary>
        private const string TotalFailoverBrokerNodeCountName = "Total Failover Broker Node Count";

        /// <summary>
        /// Stores the name for Total Failover Cluster Count
        /// </summary>
        private const string TotalFailoverClusterCountName = "Total Failover Cluster Count";

        /// <summary>
        /// Stores the name for Active Broker Node Count
        /// </summary>
        private const string ActiveBrokerNodeCountName = "Active Broker Node Count";

        /// <summary>
        /// Stores the name for Active Broker Resource Group Count
        /// </summary>
        private const string ActiveBrokerResourceGroupCountName = "Active Broker Resource Group Count";

        /// <summary>
        /// Gets HpcSession perf counter
        /// </summary>
        /// <param name="key">indicating the perf counter key</param>
        /// <returns>a correspinding perf counter</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static PerformanceCounter GetPerfCounter(SessionPerformanceCounterKey key)
        {
            try
            {
                switch (key)
                {
                    case SessionPerformanceCounterKey.TotalFailoverBrokerNodeCount:
                        return new PerformanceCounter(SessionCategoryName, TotalFailoverBrokerNodeCountName, false);
                    case SessionPerformanceCounterKey.TotalFailoverClusterCount:
                        return new PerformanceCounter(SessionCategoryName, TotalFailoverClusterCountName, false);
                    case SessionPerformanceCounterKey.ActiveBrokerNodeCount:
                        return new PerformanceCounter(SessionCategoryName, ActiveBrokerNodeCountName, false);
                    case SessionPerformanceCounterKey.ActiveBrokerResourceGroupCount:
                        return new PerformanceCounter(SessionCategoryName, ActiveBrokerResourceGroupCountName, false);
                }
            }

            catch { }

            return null;
        }
    }
}