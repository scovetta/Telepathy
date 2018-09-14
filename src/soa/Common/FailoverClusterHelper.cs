//-----------------------------------------------------------------------
// <copyright file="FailoverClusterHelper.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Helper class for failover cluster related functions</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System.Diagnostics;
    using Microsoft.Hpc.RuntimeTrace;

    /// <summary>
    /// Helper class for failover cluster related functions
    /// </summary>
    internal static class FailoverClusterHelper
    {
        /// <summary>
        /// Stores the value indicating whether the current node is within
        /// a failover cluster
        /// </summary>
        private static bool isInFailoverCluster;

        /// <summary>
        /// Static constructor
        /// </summary>
        static FailoverClusterHelper()
        {
            isInFailoverCluster = IsInFailoverClusterInternal();
        }

        /// <summary>
        /// Gets a value indicating whether the current node is within
        /// a failover cluster
        /// </summary>
        public static bool IsInFailoverCluster
        {
            get { return isInFailoverCluster; }
        }

        /// <summary>
        /// Gets a value indicating whether the current node is within
        /// a failover cluster
        /// </summary>
        /// <returns>
        /// returns a value indicating whether it is in a failover cluster
        /// </returns>
        private static bool IsInFailoverClusterInternal()
        {
            if (SoaHelper.IsOnAzure())
            {
                return false;
            }

            int clusterState = (int)ClusterState.ClusterStateNotInstalled;

            uint ret = Win32API.GetNodeClusterState(null, out clusterState);
            if (ret != 0)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[FailoverClusterHelper] Cannot access local failover cluster state. Error = {0}", ret);
                return false;
            }

            if (clusterState == (int)ClusterState.ClusterStateNotConfigured || clusterState == (int)ClusterState.ClusterStateNotInstalled)
            {
                TraceHelper.TraceEvent(TraceEventType.Information, "[FailoverClusterHelper] Cannot access local failover cluster state. Error = {0}", ret);
                return false;
            }

            return true;
        }
    }
}
