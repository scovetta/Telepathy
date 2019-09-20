// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading;

    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// Implementation of broker launcher
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true,
        Name = "NodeMappingCache", Namespace = "http://hpc.microsoft.com/brokerlauncher/")]
    class NodeMappingCache : DisposableObject, INodeMappingCache
    {
        /// <summary>
        /// Node mapping data: logical name to IP address mapping
        /// </summary>
        private Dictionary<string, string> logicalName2IpMapping;

        /// <summary>
        /// Timestamp that records when last node mapping data update happened
        /// </summary>
        private DateTime lastUpdateTime = DateTime.MinValue;

        /// <summary>
        /// Lock object to allow at most one update operation at the same time
        /// </summary>
        private object lockUpdateNodeMapping = new object();

        /// <summary>
        /// Timer to update node mapping timely
        /// </summary>
        private Timer updateNodeMappingTimer;

        /// <summary>
        /// Refresh node mapping cache every 5 minutes
        /// </summary>
        private const int UpdateNodeMappingInterval = 5 * 60 * 1000;

        /// <summary>
        /// Node mapping data younder than 1 sec is considered as fresh, and will not be updated.
        /// </summary>
        private const int FreshPeriod = 1;

        public NodeMappingCache()
        {
            this.updateNodeMappingTimer = new Timer(this.UpdateNodeMappingProc, null, 0, UpdateNodeMappingInterval);
        }

        /// <summary>
        /// Get Azure node mapping data
        /// </summary>
        /// <param name="fromCache">if return node mapping data from cache or from node mapping table </param>
        /// <returns>A copy of Azure node mapping data</returns>
        public Dictionary<string, string> GetNodeMapping(bool fromCache)
        {
            if (this.logicalName2IpMapping == null || !fromCache)
            {
                this.UpdateNodeMapping();
            }
            
            return this.logicalName2IpMapping;
        }

        /// <summary>
        /// Dispose the updateNodeMappingTimer.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.updateNodeMappingTimer != null)
                {
                    this.updateNodeMappingTimer.Dispose();
                    this.updateNodeMappingTimer = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Get the node mapping for the Azure nodes.
        /// </summary>
        /// <param name="state">object used by the callback method.</param>
        private void UpdateNodeMappingProc(object state)
        {
            TraceHelper.TraceVerbose("0", "[NodeMappingCache] .UpdateNodeMappingProc: updating node mapping cache");
            this.UpdateNodeMapping();
        }

        /// <summary>
        /// Update node mapping data cache
        /// </summary>
        private void UpdateNodeMapping()
        {
            lock (this.lockUpdateNodeMapping)
            {
                if (DateTime.Now.CompareTo(this.lastUpdateTime.AddSeconds(FreshPeriod)) < 0)
                {
                    // if the node mapping data was updated in 1 second, skip
                    return;
                }


                this.logicalName2IpMapping = this.GetNodeMappingData();
                this.lastUpdateTime = DateTime.Now;
                this.updateNodeMappingTimer.Change(UpdateNodeMappingInterval, UpdateNodeMappingInterval);
            }
        }

        /// <summary>
        /// Query node mapping data from node mapping table
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetNodeMappingData()
        {  
            try
            {
                string dataConnectionString = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.DataConnectionString);
                string nodeMappingTableName = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.NodeMapping);
                NodeMapping nodeMapping = new NodeMapping(CloudStorageAccount.Parse(dataConnectionString), nodeMappingTableName);

                // Following GetLogicalNameIPMapping calls RefreshMapping, so we don't need to call it beforehand.
                return nodeMapping.GetLogicalNameIPMapping();
            }
            catch (Exception e)
            {
                TraceHelper.TraceError("0", "[NodeMappingCache] .GetNodeMappingData: Failed to get node mapping. {0}", e);
                return null;
            }
        }
    }
}
