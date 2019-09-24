// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Helper class for performance counter
    /// </summary>
    public static class BrokerPerformanceCounterHelper
    {
        /// <summary>
        /// Stores the node category name
        /// </summary>
        private const string NodeCategoryName = "HPC WCF Broker Node";

        /// <summary>
        /// Stores the node category help
        /// </summary>
        private const string NodeCategoryHelp = "HPC WCF Broker Node";

        /// <summary>
        /// Stores the name for Requests/sec counter
        /// </summary>
        private const string RequestsPerSecCounterName = "Requests/sec";

        /// <summary>
        /// Stores the name for Responses/sec counter
        /// </summary>
        private const string ResponsesPerSecCounterName = "Responses/sec";

        /// <summary>
        /// Stores the name Calculations/sec counter
        /// </summary>
        private const string CalculationsPerSecCounterName = "Calculations/sec";

        /// <summary>
        /// Stores the name for Faults/sec counter
        /// </summary>
        private const string FaultsPerSecCounterName = "Faults/sec";


        /// <summary>
        /// Stores the name for Durable Requests Queue Length counter
        /// </summary>
        private const string DurableRequestsQueueLengthCounterName = "Durable Requests Queue Length";

        /// <summary>
        /// Stores the name for Durable Responses Queue Length counter
        /// </summary>
        private const string DurableResponsesQueueLengthCounterName = "Durable Responses Queue Length";

        /// <summary>
        /// Gets a node wide perf counter
        /// </summary>
        /// <param name="key">indicating the perf counter key</param>
        /// <returns>a correspinding perf counter</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static PerformanceCounter GetPerfCounter(BrokerPerformanceCounterKey key)
        {
            switch (key)
            {
                case BrokerPerformanceCounterKey.RequestMessages:
                    return new PerformanceCounter(NodeCategoryName, RequestsPerSecCounterName, false);
                case BrokerPerformanceCounterKey.ResponseMessages:
                    return new PerformanceCounter(NodeCategoryName, ResponsesPerSecCounterName, false);
                case BrokerPerformanceCounterKey.Calculations:
                    return new PerformanceCounter(NodeCategoryName, CalculationsPerSecCounterName, false);
                case BrokerPerformanceCounterKey.Faults:
                    return new PerformanceCounter(NodeCategoryName, FaultsPerSecCounterName, false);
                case BrokerPerformanceCounterKey.DurableRequestsQueueLength:
                    return new PerformanceCounter(NodeCategoryName, DurableRequestsQueueLengthCounterName, false);
                case BrokerPerformanceCounterKey.DurableResponsesQueueLength:
                    return new PerformanceCounter(NodeCategoryName, DurableResponsesQueueLengthCounterName, false);
                default:
                    throw new ArgumentException("Invalid performance counter key", "key");
            }
        }

        /// <summary>
        /// Gets an instance wide perf counter
        /// </summary>
        /// <param name="key">indicating the perf key</param>
        /// <param name="instanceName">indicating the instance name</param>
        /// <returns>returns the corresponding instance wide perf counter</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static PerformanceCounter GetPerfCounter(BrokerPerformanceCounterKey key, string instanceName)
        {
            switch (key)
            {
                default:
                    throw new ArgumentException("Invalid performance counter key", "key");
            }
        }
    }
}