//------------------------------------------------------------------------------
// <copyright file="CommonClusterManagerHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Helper class for operation to cluster manager
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using Microsoft.ComputeCluster.Management;
    using Microsoft.ComputeCluster.Management.ClusterModel;
    using Microsoft.Hpc.RuntimeTrace;
    using Hpc;
    /// <summary>
    /// Helper class for operation to cluster
    /// </summary>
    internal static class CommonClusterManagerHelper
    {
        private const string PrivateNetworkPrefix = "private.";

        private static Dictionary<string, ClusterTopology> TopologyDic = new Dictionary<string, ClusterTopology>();

        private static object SyncObjForDic = new object();

        private static object SyncObjForConnetion = new object();

        /// <summary>
        /// Selects private network if it exists.
        /// </summary>
        /// <param name="headnode">scheduler name</param>
        /// <param name="internalAddress">expect internal address or not</param>
        /// <returns>epr address</returns>
        internal static string GetSchedulerDelegationAddressPerNetworkTopology(string headnode, bool internalAddress)
        {
            Debug.Assert(!string.IsNullOrEmpty(headnode));

            ClusterTopology topo = ClusterTopology.Public;

            bool success = false;
            lock (SyncObjForDic)
            {
                success = TopologyDic.TryGetValue(headnode, out topo);
            }

            if (!success)
            {
                UpdateClusterTopology(headnode);
                lock (SyncObjForDic)
                {
                    TopologyDic.TryGetValue(headnode, out topo);
                }
            }

            string machine = headnode; // ServiceFabricUtils.ResolveServiceNode(FabricConstants.SessionLauncherStatefulServiceUri, headnode);
            if (topo != ClusterTopology.Public)
            {
                string headNodeName = machine;
                int index = headNodeName.IndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                if (index > -1)
                {
                    headNodeName = headNodeName.Substring(0, index);
                }

                if (Dns.GetHostName().Equals(headNodeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    // use "<headnode>" without "private" in the epr when BN==HN
                    // machine = headnode;
                }
                else
                {
                    // use "private.<headnode>" in the epr when BN!=HN
                    machine = string.Concat(PrivateNetworkPrefix, machine);
                }
            }

            return internalAddress ? SoaHelper.GetSchedulerDelegationInternalAddress(machine) : SoaHelper.GetSchedulerDelegationAddress(machine);
        }

        /// <summary>
        /// Update the topology of the cluster network.
        /// </summary>
        /// <param name="headnode">scheduler name</param>
        /// <returns>topology</returns>
        internal static void UpdateClusterTopology(string headnode)
        {
            // there is no SDM for WAHS
            if (SoaHelper.IsSchedulerOnAzure())
            {
                return;
            }

            ClusterTopology topo = ClusterTopology.Public;

            try
            {
                // Always use ClusterTopology.Public in Azure cluster.
                if (!SoaHelper.IsOnAzure())
                {
                    // don't re-enter the ClusterManager
                    lock (SyncObjForConnetion)
                    {
                        using (ClusterManager mgr = HpcContext.GetOrAdd(headnode, System.Threading.CancellationToken.None).GetClusterManagerAsync().GetAwaiter().GetResult())
                        {
                            using (GlobalNetworkSettingsConfiguration network = new GlobalNetworkSettingsConfiguration(mgr))
                            {
                                topo = network.Topology.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Warning, "[CommonClusterManagerHelper].UpdateClusterTopology: Exception {0}", ex);
            }
            finally
            {
                lock (SyncObjForDic)
                {
                    TopologyDic[headnode] = topo;
                }
            }
        }


        /// <summary>
        /// Get the ClusterTopology of the specified cluster.
        /// </summary>
        /// <param name="headnode">head node name</param>
        /// <returns>ClusterTopology</returns>
        internal static ClusterTopology GetClusterTopology(string headnode)
        {
            Debug.Assert(!string.IsNullOrEmpty(headnode));

            ClusterTopology topo = ClusterTopology.Public;
            lock (SyncObjForDic)
            {
                TopologyDic.TryGetValue(headnode, out topo);
            }

            return topo;
        }


        /// <summary>
        /// Set the ClusterTopology of the specified cluster.
        /// </summary>
        /// <param name="headnode">head node name</param>
        /// <param name="topo">ClusterTopology</param>
        internal static void SetClusterTopology(string headnode, ClusterTopology topo)
        {
            Debug.Assert(!string.IsNullOrEmpty(headnode));

            lock (SyncObjForDic)
            {
                TopologyDic[headnode] = topo;
            }
        }

        /// <summary>
        /// Get the global cluster property "AzureStorageConnectionString".
        /// </summary>
        /// <param name="headnode">head node name</param>
        /// <returns>Azure storage connection string</returns>
        internal static string GetAzureStorageConnectionString(string headnode)
        {
            // there is no SDM for WAHS
            if (SoaHelper.IsSchedulerOnAzure())
            {
                return null;
            }

            using (ClusterManager mgr = HpcContext.GetOrAdd(headnode, System.Threading.CancellationToken.None).GetClusterManagerAsync().GetAwaiter().GetResult())
            {
                using (GlobalClusterConfiguration cfg = mgr.ConfigureCluster())
                {
                    return cfg.AzureStorageConnectionString;
                }
            }
        }

        /// <summary>
        /// Get the global cluster properties "ClusterName" and "ClusterId".
        /// </summary>
        /// <param name="headnode">head node name</param>
        /// <param name="clusterName">the cluster name</param>
        /// <param name="clusterId">the cluster Id</param>
        internal static void GetClusterUniqueId(string headnode, out string clusterName, out Guid clusterId)
        {
            // there is no SDM for WAHS
            if (SoaHelper.IsSchedulerOnAzure())
            {
                clusterName = null;
                clusterId = Guid.Empty;
                return;
            }

            using (ClusterManager mgr = HpcContext.GetOrAdd(headnode, System.Threading.CancellationToken.None).GetClusterManagerAsync().GetAwaiter().GetResult())
            {
                clusterId = mgr.ClusterId;

                using (GlobalClusterConfiguration cfg = mgr.ConfigureCluster())
                {
                    clusterName = cfg[GlobalClusterConfiguration.ClusterNameColum].Value as string;
                }
            }
        }
    }
}
