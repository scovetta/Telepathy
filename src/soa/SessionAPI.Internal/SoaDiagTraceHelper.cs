// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker
{
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using System.Collections.Concurrent;
    /// <summary>
    /// Utility class providing diag trace enabled/disabled flag.
    /// </summary>
    public static class SoaDiagTraceHelper
    {
        /// <summary>
        /// Stores the diag trace enabled/disabled flag for soa sessions.
        /// key: session id
        /// value: enable/disable diag trace
        /// </summary>
        private static ConcurrentDictionary<string, bool> dic = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Stores the value indicating whether it is running on Azure
        /// </summary>
        private static bool isOnAzure = SoaHelper.IsSchedulerOnAzure();

        /// <summary>
        /// Sets the IsDiagTraceEnabledDelegate to get the real value
        /// </summary>
        public static IsDiagTraceEnabledDelegate IsDiagTraceEnabledInternal
        {
            private get;
            set;
        }

        /// <summary>
        /// Check if the diag trace is enabled for the specified session.
        /// </summary>
        /// <param name="jobId">job id of the session</param>
        /// <returns>is trace enabled</returns>
        public static bool IsDiagTraceEnabled(string jobId)
        {
            bool enable = false;

            if (dic.TryGetValue(jobId, out enable))
            {
                return enable;
            }
            else if (isOnAzure)
            {
                // If on azure, always enable diag trace
                return true;
            }
            else if (IsDiagTraceEnabledInternal != null)
            {
                enable = IsDiagTraceEnabledInternal(jobId);
                SetDiagTraceEnabledFlag(jobId, enable);
                return enable;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Add the diag trace enabled/disabled flag.
        /// </summary>
        /// <param name="jobId">job id of the session</param>
        /// <param name="enabled">is trace enabled</param>
        public static void SetDiagTraceEnabledFlag(string jobId, bool enabled)
        {
            dic.AddOrUpdate(jobId, enabled, (key, value) => enabled);
        }

        /// <summary>
        /// Delete the diag trace enabled/disabled flag.
        /// </summary>
        /// <param name="jobId">job id of the session</param>
        public static void RemoveDiagTraceEnabledFlag(string jobId)
        {
            bool enable = false;
            dic.TryRemove(jobId, out enable);
        }
    }
}
