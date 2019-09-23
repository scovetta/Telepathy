// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using System;

    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Common;

    /// <summary>
    /// the broker nodes manager class.
    /// </summary>
    internal class BrokerNodesManager
#if HPCPACK
        : IDisposable
#endif
    {

        /// <summary>
        /// the endpoint prefix for https binding.
        /// </summary>
        internal const string HttpsPrefix = "https://";

        /// <summary>
        /// the endpoint prefix for net.tcp binding.
        /// </summary>
        internal const string NettcpPrefix = "net.tcp://";

        /// <summary>
        /// the broker service EPR templete for nettcp.
        /// </summary>
        private const string BrokerEndpointPath = "BrokerLauncher";

        /// <summary>
        /// How often to check BN status
        /// </summary>
        private const int CheckNodesPerClusterInterval = 10000;
#if HPCPACK
        /// <summary>
        /// the broker nodes filter used to pull the broker nodes from the scheduler.
        /// </summary>
        private volatile FilterCollection brokerNodesFilterField = null;

        /// <summary>
        /// the scheduler.
        /// </summary>
        private IScheduler schedulerField;

        /// <summary>
        /// Synchronizes access to availableNonHABrokerNodes, availableHABrokerNodes
        /// </summary>
        private object brokerEPRsLock = new object();

        /// <summary>
        /// The avariable NonHA Brokers' EPR list
        /// </summary>
        private volatile List<NodeInfo> availableNonHABrokerNodes = new List<NodeInfo>();

        /// <summary>
        /// The avariable HA Brokers' EPR list
        /// </summary>
        private volatile List<NodeInfo> availableHABrokerNodes = new List<NodeInfo>();

        /// <summary>
        /// The available Azure broker node IP address list
        /// </summary>
        private List<string> availableAzureBrokers = new List<string>();

        /// <summary>
        /// Index of node name in available BN lists
        /// </summary>
        internal const int NodeNameIndex = 0;

        /// <summary>
        /// Index of node SSDL in available BN lists
        /// </summary>
        internal const int NodeSSDLIndex = 1;

        /// <summary>
        /// the seed to roundrobing the HA brokers
        /// No need to use volatile as it is only used in InterlockedIncrement
        /// </summary>
        private int seedHA;

        /// <summary>
        /// the seed to roundrobing the NonHA brokers
        /// No need to use volatile as it is only used in InterlockedIncrement
        /// </summary>
        private int seedNonHA;

        /// <summary>
        /// timer to update broker node information
        /// </summary>
        private Timer brokerNodeTimer;

        /// <summary>
        /// Cached connections to BN failover clusters
        /// </summary>
        private Dictionary<string, IntPtr> failoverClusterConnections = new Dictionary<string, IntPtr>();

        /// <summary>
        /// Cached all the Broker Node names
        /// </summary>
        private List<string> availableBrokers = new List<string>();

        /// <summary>
        /// The last list of BNs communicated to admin. Maintained to minimize unnecessary info events
        /// </summary>
        private string lastBrokerListInfoMessage = String.Empty;

        /// <summary>
        /// Perf counter for total number of failover broker nodes
        /// </summary>
        private PerformanceCounter totalFailoverBrokerNodeCount = null;

        /// <summary>
        /// Perf counter for total number of failover clusters within broker node tier
        /// </summary>
        private PerformanceCounter totalFailoverClusterCount = null;

        /// <summary>
        /// Perf counter for total number of active broker nodes
        /// </summary>
        private PerformanceCounter activeBrokerNodeCount = null;

        /// <summary>
        /// Perf counter for total number of active broker resource groups
        /// </summary>
        private PerformanceCounter activeBrokerResourceGroupCount = null;

        /// <summary>
        /// Initializes a new instance of the BrokerNodesManager class with the specified head node. 
        /// </summary>
        /// <param name="headNode">the head node name.</param>
        public BrokerNodesManager()
        {
            this.schedulerField = CommonSchedulerHelper.GetScheduler(TelepathyContext.Get().CancellationToken).GetAwaiter().GetResult();
            
            // Initialize performance counters
            this.totalFailoverBrokerNodeCount = SessionPerformanceCounterHelper.GetPerfCounter(SessionPerformanceCounterKey.TotalFailoverBrokerNodeCount);
            this.totalFailoverClusterCount = SessionPerformanceCounterHelper.GetPerfCounter(SessionPerformanceCounterKey.TotalFailoverClusterCount);
            this.activeBrokerNodeCount = SessionPerformanceCounterHelper.GetPerfCounter(SessionPerformanceCounterKey.ActiveBrokerNodeCount);
            this.activeBrokerResourceGroupCount = SessionPerformanceCounterHelper.GetPerfCounter(SessionPerformanceCounterKey.ActiveBrokerResourceGroupCount);

            if (SoaHelper.IsOnAzure())
            {
                this.brokerNodeTimer = new Timer(this.UpdateAvailableAzureBroker, null, 0, CheckNodesPerClusterInterval);
            }
            else
            {
                // poll the broker nodes immediately once connect to the scheduler.
                this.BrokerNodesTimerCallback(null);

                // Start the timer that will poll broker nodes
                // TODO: Consider a timer per cluster
                this.brokerNodeTimer = new Timer(this.BrokerNodesTimerCallback, null, CheckNodesPerClusterInterval, CheckNodesPerClusterInterval);
            }
        }
       
        /// <summary>
        /// Broker node poll timer callback 
        /// </summary>
        /// <param name="state"></param>
        internal void BrokerNodesTimerCallback(object state)
        {
            // Only allow one refresh at a time. If a tick occurs while another refresh is in progress, skip the new tick
            if (this.failoverClusterConnections == null)
            {
                return;
            }

            if (Monitor.TryEnter(this.failoverClusterConnections))
            {
                try
                {
                    List<string> brokerNodesWithFailover = new List<string>();
                    Dictionary<string, ResourceGroupInfo> resourceGroups = new Dictionary<string, ResourceGroupInfo>();

                    // Get the BN failover clusters, if any
                    string[] failoverClusterNames = GetFailoverClusterNames();

                    // If there are BN failover clusters
                    if (failoverClusterNames != null)
                    {
                        // TODO: Consider moving querying MSCS to a broker launcher operation to lower chattiness
                        //  and speed up each poll

                        // Connect to the failover clusters
                        ConnectToBNFailoverClusters(failoverClusterNames, this.failoverClusterConnections);

                        // If there are connections
                        if (this.failoverClusterConnections.Count != 0)
                        {
                            // Get which broker nodes are HA and get resource groups
                            GetFailoverClusterInfo(this.failoverClusterConnections, brokerNodesWithFailover, resourceGroups);
                        }
                        else
                        {
                            TraceHelper.TraceEvent(TraceEventType.Error, "Cannot connect to any failover clusters");
                        }

                        // Update HpcSession performance counter - TotalFailoverClusterCount
                        if (this.totalFailoverClusterCount != null)
                        {
                            this.totalFailoverClusterCount.RawValue = failoverClusterNames.Length;
                        }

                        // Update HpcSession performance counter - TotalFailoverBrokerNodeCount
                        if (this.totalFailoverBrokerNodeCount != null)
                        {
                            this.totalFailoverBrokerNodeCount.RawValue = brokerNodesWithFailover.Count;
                        }

                        // Update HpcSession performance counter - ActiveBrokerResourceGroupCount
                        if (this.activeBrokerResourceGroupCount != null)
                        {
                            this.activeBrokerResourceGroupCount.RawValue = resourceGroups.Count;
                        }
                    }

                    // Get available broker nodes in the cluster.
                    List<BrokerNodeItem> brokerNodes = GetBrokerNodesFromScheduler();

                    // Update the available broker node EPR lists (HA and non HA)
                    UpdateAvaliableBrokers(brokerNodes, brokerNodesWithFailover, resourceGroups);
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "Get broker nodes from the scheduler raised exception, {0}", e);
                }
                finally
                {
                    Monitor.Exit(this.failoverClusterConnections);
                }
            }
        }

        /// <summary>
        /// Get available broker node name list.
        /// </summary>
        /// <returns>name list</returns>
        internal List<string> GetAvailableBrokerNodeName()
        {
            lock (this.availableBrokers)
            {
                return new List<string>(this.availableBrokers.ToArray());
            }
        }

        /// <summary>
        /// Returns whether failover clustering is enabled for the BNs
        /// </summary>
        /// <returns></returns>
        private static int GetFailoverClusterCount()
        {
            string[] failoverClusters = GetFailoverClusterNames();
            return (failoverClusters != null) ? failoverClusters.Length : 0;
        }

        /// <summary>
        /// Returns the names of the failover cluster
        /// </summary>
        /// <returns></returns>
        private static string[] GetFailoverClusterNames()
        {
            string failoverClusterNames = ConfigurationManager.AppSettings["failoverClusterName"];

            if (String.IsNullOrEmpty(failoverClusterNames))
                return null;

            // Make all machine names upper case so we can easily compare\lookup
            failoverClusterNames = failoverClusterNames.ToUpper();

            return failoverClusterNames.Split(';');
        }

        /// <summary>
        /// Get the Azure broker nodes' IP addresses and update the address list.
        /// </summary>
        private void UpdateAvailableAzureBroker(object state)
        {
            // throw away the previous list and update the list reference
            this.availableAzureBrokers = AzureRoleHelper.GetAllBrokerAddress();

            TraceHelper.TraceEvent(
                TraceEventType.Verbose,
                "[BrokerNodesManager] .UpdateAvailableAzureBroker: Get {0} brokers", this.availableAzureBrokers.Count);
        }

        /// <summary>
        /// Get broker nodes inside Azure cluster.
        /// </summary>
        /// <param name="nodesInfo">node info list</param>
        /// <param name="incrementIndex">true to increment the round robin index.</param>
        /// <returns>The returned array sorts the addresses by round robin fashion.</returns>
        public string[] GetAvailableAzureBrokerEPRs(out List<NodeInfo> nodesInfo, bool incrementIndex = true)
        {
            List<string> eprs = new List<string>();

            nodesInfo = new List<NodeInfo>();

            // this.availableAzureBrokers is being updated by the timer,
            // so create a tmp list reference here refering to the previous list.
            List<string> tmp = this.availableAzureBrokers;

            int count = tmp.Count;
            if (count >= 1)
            {
                int step = 0;
                if (incrementIndex)
                {
                    step = Interlocked.Increment(ref this.seedNonHA);
                    TraceHelper.TraceInfo(0, "[BrokerNodesMananger] .GetAvailableAzureBrokerEPRs: Incremented SeedNonHA = {0}", step);
                }

                for (int i = 0; i < count; i++)
                {
                    string broker = tmp[(i + step) % count];
                    eprs.Add(SoaHelper.GetBrokerLauncherAddress(broker));

                    // no need to use the FQDN for Azure broker.
                    nodesInfo.Add(GenerateNodeInfo(broker));
                }
            }

            TraceHelper.TraceEvent(
                TraceEventType.Verbose,
                "[BrokerNodesManager] .GetAvailableAzureBrokerEPRs: {0} azure brokers are available.", eprs.Count);

            return eprs.ToArray();
        }

        /// <summary>
        /// Gets the available broker eprs.
        /// </summary>
        /// <param name="durable">true if the session is durable.</param>
        /// <param name="endpointPrefix">the prefix of the endpoint</param>
        /// <param name="needFqdn">true if need FQDN</param>
        /// <param name="nodesOnly">the node info</param>
        /// <param name="incrementIndex">true to increment the round robin index.</param>
        /// <returns>the EPRs of the brokers.</returns>
        public string[] GetAvailableBrokerEPRs(bool durable, string endpointPrefix, bool needFqdn, ChannelTypes type, TransportScheme scheme, out List<NodeInfo> nodesOnly, bool incrementIndex = true)
        {
            if (string.IsNullOrEmpty(endpointPrefix))
            {
                endpointPrefix = BrokerNodesManager.NettcpPrefix;
            }

            List<NodeInfo> availableNonHABrokerNodes = null;
            List<NodeInfo> availableHABrokerNodes = null;

            // Get a reference to the current lists atomically
            // This is safe because:
            // 1. Read-only access of List<T> is thread-safe. 
            // 2. We don't write to those List<T>, but only create new List<T> and assign them to available*BrokerNode variables.
            lock (this.brokerEPRsLock)
            {
                availableNonHABrokerNodes = this.availableNonHABrokerNodes;
                availableHABrokerNodes = this.availableHABrokerNodes;
            }

            int step = 0;
            List<string> availableBrokerEprs = new List<string>();

            nodesOnly = new List<NodeInfo>();

            // If caller requires a nondurable session or failover BNs arent enabled, add the non HA BNs
            if (!durable || (0 == GetFailoverClusterCount()))
            {
                if (incrementIndex)
                {
                    step = Interlocked.Increment(ref this.seedNonHA);
                    TraceHelper.TraceInfo(0, "[BrokerNodesMananger] .GetAvailableAzureBrokerEPRs: Incremented SeedNonHA = {0}", step);
                }

                for (int i = 0; i < availableNonHABrokerNodes.Count; i++)
                {
                    NodeInfo nodeInfo = availableNonHABrokerNodes[(i + step) % availableNonHABrokerNodes.Count];

                    // availableBrokerEprs.Add( isInternal ?
                    //     GenerateBrokerLauncherInternalEpr(endpointPrefix, needFqdn ? nodeInfo.FQDN : nodeInfo.Name):
                    //     GenerateBrokerLauncherEpr(endpointPrefix, needFqdn ? nodeInfo.FQDN : nodeInfo.Name, scheme));

                    switch (type)
                    {
                        case ChannelTypes.AzureAD:
                            availableBrokerEprs.Add(GenerateBrokerLauncherAadEpr(endpointPrefix, needFqdn ? nodeInfo.FQDN : nodeInfo.Name));
                            break;
                        case ChannelTypes.LocalAD:
                            availableBrokerEprs.Add(GenerateBrokerLauncherEpr(endpointPrefix, needFqdn ? nodeInfo.FQDN : nodeInfo.Name, scheme));
                            break;
                        case ChannelTypes.Certificate:
                            availableBrokerEprs.Add(GenerateBrokerLauncherInternalEpr(endpointPrefix, needFqdn ? nodeInfo.FQDN : nodeInfo.Name));
                            break;
                        default:
                            throw new NotSupportedException($"Type {type} is not a supported broker channel type.");
                    }

                    nodesOnly.Add(nodeInfo);
                }
            }

            // If caller wants durable or non-durable sessions and failover BNs are enabled, add HA BNs. If
            // the caller asked for non-durable, add durable EPRs to the end of the list as a fallback in
            // case the non-HA BNs are busy
            if (0 != GetFailoverClusterCount())
            {
                // If the caller wants nondurable add HA BNs next. If call wants durable, return HA BNs only
                if (incrementIndex)
                {
                    step = Interlocked.Increment(ref this.seedHA);
                    TraceHelper.TraceInfo(0, "[BrokerNodesMananger] .GetAvailableAzureBrokerEPRs: Incremented SeedHA = {0}", step);
                }

                for (int i = 0; i < availableHABrokerNodes.Count; i++)
                {
                    NodeInfo nodeInfo = availableHABrokerNodes[(i + step) % availableHABrokerNodes.Count];

                    switch (type)
                    {
                        case ChannelTypes.AzureAD:
                            availableBrokerEprs.Add(GenerateBrokerLauncherAadEpr(endpointPrefix, needFqdn ? nodeInfo.FQDN : nodeInfo.Name));
                            break;
                        case ChannelTypes.LocalAD:
                            availableBrokerEprs.Add(GenerateBrokerLauncherEpr(endpointPrefix, needFqdn ? nodeInfo.FQDN : nodeInfo.Name, scheme));
                            break;
                        case ChannelTypes.Certificate:
                            availableBrokerEprs.Add(GenerateBrokerLauncherInternalEpr(endpointPrefix, needFqdn ? nodeInfo.FQDN : nodeInfo.Name));
                            break;
                        default:
                            throw new NotSupportedException($"Type {type} is not a supported broker channel type.");
                    }

                    nodesOnly.Add(nodeInfo);
                }
            }

            return availableBrokerEprs.ToArray();
        }

        /// <summary>
        /// release resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        public bool IsBrokerNode(string name)
        {
            lock (this.availableBrokers)
            {
                return this.availableBrokers.BinarySearch(name, StringComparer.InvariantCultureIgnoreCase) >= 0;
            }
        }

        /// <summary>
        /// Generate broker launcher internal epr
        /// </summary>
        /// <param name="machineName">machine name</param>
        /// <returns>broker launcher epr</returns>
        internal static string GenerateBrokerLauncherInternalEpr(string endpointPrefix, string machineName)
        {
            if (endpointPrefix.Equals(HttpsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                // this is https
                return endpointPrefix + machineName + "/" + BrokerEndpointPath;
            }

            if (endpointPrefix.Equals(NettcpPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return SoaHelper.GetBrokerLauncherInternalAddress(machineName);
            }

            throw new ArgumentException();
        }

        internal static string GenerateBrokerLauncherAadEpr(string endpointPrefix, string machineName)
        {
            if (endpointPrefix.Equals(NettcpPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return SoaHelper.GetBrokerLauncherAadAddress(machineName);
            }

            throw new ArgumentException("AAD only support net.tcp");
        }

        /// <summary>
        /// release resources.
        /// </summary>
        /// <param name="dispose">a value indicating whether release the resources.</param>
        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                if (this.brokerNodeTimer != null)
                {
                    this.brokerNodeTimer.Dispose();
                    this.brokerNodeTimer = null;
                }

                if (this.schedulerField != null)
                {
                    this.schedulerField.Close();
                    this.schedulerField.Dispose();
                    this.schedulerField = null;
                }

                if (this.failoverClusterConnections != null)
                {
                    lock (this.failoverClusterConnections)
                    {
                        foreach (KeyValuePair<string, IntPtr> connection in this.failoverClusterConnections)
                        {
                            try
                            {
                                Win32API.CloseCluster(connection.Value);
                            }
                            catch (Exception ex)
                            {
                                TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerNodesManager].Dispose: Exception {0}", ex);
                            }   // Shutting down so just swallow exceptions
                        }
                    }

                    this.failoverClusterConnections = null;
                }
            }
        }

        /// <summary>
        /// get the broker nodes from the scheduler.
        /// </summary>
        private List<BrokerNodeItem> GetBrokerNodesFromScheduler()
        {
            List<BrokerNodeItem> brokerNodesInCluster = new List<BrokerNodeItem>();

            // brokerNodesFilterField is immutable so accessing brokerNodesFilterField is thread safe
            if (brokerNodesFilterField == null)
            {
                FilterCollection brokerNodesFilters = new FilterCollection();
                brokerNodesFilters.Add(new FilterProperty(FilterOperator.HasBitSet, new StoreProperty(NodePropertyIds.JobType, JobType.Broker)));

                // TODO: Why arent these used?
                // brokerNodesFilters.Add(new FilterProperty(FilterOperator.Equal, new StoreProperty(NodePropertyIds.Reachable, true)));
                // brokerNodesFilters.Add(new FilterProperty(FilterOperator.Equal, new StoreProperty(NodePropertyIds.State, NodeState.Online)));
                brokerNodesFilterField = brokerNodesFilters;
            }

            ISchedulerCollection nodeList = null;
            try
            {
                nodeList = this.schedulerField.GetNodeList(brokerNodesFilterField, new SortCollection());
            }
            catch (SchedulerException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error,
                    "Failed to get the node list from the scheduler, Exception:{0}, {1}",
                    e.Message, e);
                throw;
            }

            TraceHelper.TraceEvent(
                TraceEventType.Information,
                "Get {0} broker nodes from the scheduler.",
                nodeList.Count);

            foreach (ISchedulerNode brokerNode in nodeList)
            {
                if (brokerNode != null && ((brokerNode.JobType & JobType.Broker) != 0))
                {
                    brokerNodesInCluster.Add(
                        new BrokerNodeItem(brokerNode.Name, brokerNode.DnsSuffix, brokerNode.State, brokerNode.Reachable));

                    TraceHelper.TraceEvent(
                        TraceEventType.Information,
                        "Add broker node {0} to the list, domain name is {1}.",
                        brokerNode.Name,
                        brokerNode.DnsSuffix);
                }
            }

            return brokerNodesInCluster;
        }

        /// <summary>
        /// update the available broker EPRs.
        /// </summary>
        private void UpdateAvaliableBrokers(
            List<BrokerNodeItem> brokerNodesInCluster,
            List<string> brokerNodesWithFailover,
            Dictionary<string, ResourceGroupInfo> resourceGroups)
        {
            List<NodeInfo> onlineNonHABrokers = new List<NodeInfo>();
            List<NodeInfo> onlineHABrokers = new List<NodeInfo>();
            List<NodeInfo> offlineBrokers = new List<NodeInfo>();
            String brokerListInfoMessage = String.Empty;

#if HPCPACK
            // Loop through the broker nodes
            foreach (BrokerNodeItem brokerNode in brokerNodesInCluster)
            {
                // Add those that are reachable, online and not failover BNs to the NonHA BN list
                if (brokerNode != null && brokerNode.Reachable)
                {
                    if (!brokerNodesWithFailover.Contains(brokerNode.Name))
                    {
                        if (brokerNode.State == NodeState.Online)
                        {
                            onlineNonHABrokers.Add(GenerateNodeInfo(brokerNode.Name, brokerNode.DomainName));
                        }
                        else
                        {
                            // Note: We need to authenticate offline broker node also to
                            // enable SoaDiagSvc on offline broker nodes.
                            offlineBrokers.Add(GenerateNodeInfo(brokerNode.Name, brokerNode.DomainName));
                        }
                    }
                }
            }
#endif

            // Loop through the broker launcher resource groups hosted on failover BNs
            foreach (ResourceGroupInfo resourceGroupInfo in resourceGroups.Values)
            {
                BrokerNodeItem hostBrokerNode = GetBrokerNode(brokerNodesInCluster, resourceGroupInfo.HostName);

                // It is possible for a resource group's BN to be removed from the compute cluster but not yet
                // removed from its failover cluster. If this occurs, move on to next resourceGroup
                if (hostBrokerNode == null)
                    continue;

                // If the resource group is available and its host is available and online, add its network name to the HA BN EPRs
                if (resourceGroupInfo.Available && hostBrokerNode.Reachable)
                {
                    // Note: Resource group's NetworkName is not FQDN. The broker node is
                    // in the same domain as the HA broker resource group.
                    // http://technet.microsoft.com/en-us/library/cc771404.aspx#BKMK_Account_Infrastructure
                    // All servers in the cluster must be in the same Active Directory domain.
                    if (hostBrokerNode.State == NodeState.Online)
                    {
                        onlineHABrokers.Add(GenerateNodeInfo(resourceGroupInfo.NetworkName, hostBrokerNode.DomainName));
                    }
                    else
                    {
                        // Note: We need to authenticate offline broker node also to
                        // enable SoaDiagSvc on offline broker nodes.
                        offlineBrokers.Add(GenerateNodeInfo(resourceGroupInfo.NetworkName, hostBrokerNode.DomainName));
                    }
                }
            }

            StringBuilder sbHA = new StringBuilder();
            StringBuilder sbNonHA = new StringBuilder();
            StringBuilder sbOffline = new StringBuilder();

            lock (this.availableBrokers)
            {
                this.availableBrokers.Clear();

                for (int i = 0; i < onlineHABrokers.Count; i++)
                {
                    sbHA.AppendLine(onlineHABrokers[i].Name);
                    this.availableBrokers.Add(onlineHABrokers[i].Name);
                }

                for (int i = 0; i < onlineNonHABrokers.Count; i++)
                {
                    sbNonHA.AppendLine(onlineNonHABrokers[i].Name);
                    this.availableBrokers.Add(onlineNonHABrokers[i].Name);
                }

                foreach (NodeInfo info in offlineBrokers)
                {
                    sbOffline.AppendLine(info.Name);
                    this.availableBrokers.Add(info.Name);
                }

                this.availableBrokers.Sort(StringComparer.InvariantCultureIgnoreCase);

                // Update HpcSession performance counter - ActiveBrokerNodeCount
                if (this.activeBrokerNodeCount != null)
                {
                    this.activeBrokerNodeCount.RawValue = this.availableBrokers.Count - offlineBrokers.Count;
                }
            }

            brokerListInfoMessage = String.Format(
                "Available broker node eprs: HA - {0}, NonHA - {1}, OffLine - {2}",
                sbHA.ToString(),
                sbNonHA.ToString(),
                sbOffline.ToString());

            // Inform admin of broker node list if it changed
            if (0 != String.Compare(this.lastBrokerListInfoMessage, brokerListInfoMessage, StringComparison.InvariantCultureIgnoreCase))
            {
                TraceHelper.TraceEvent(TraceEventType.Verbose, brokerListInfoMessage);
                this.lastBrokerListInfoMessage = brokerListInfoMessage;
            }

            // Save the updated EPR lists
            lock (this.brokerEPRsLock)
            {
                this.availableHABrokerNodes = onlineHABrokers;
                this.availableNonHABrokerNodes = onlineNonHABrokers;
            }
        }

        /// <summary>
        /// Generate a node info instance
        /// </summary>
        /// <param name="nodeName">indicating the node name</param>
        /// <returns>returns the node info instance</returns>
        internal static NodeInfo GenerateNodeInfo(string nodeName)
        {
            return GenerateNodeInfo(nodeName, null);
        }

        /// <summary>
        /// Generate a node info instance
        /// </summary>
        /// <param name="nodeName">indicating the node name</param>
        /// <param name="domainName">indicating the FQDN</param>
        /// <returns>returns the node info instance</returns>
        internal static NodeInfo GenerateNodeInfo(string nodeName, string domainName)
        {
            Exception ex = null;
            string ssdl = string.Empty;

            // Can't get the SID of the Azure node or Non-Domain joined
            if (!SoaHelper.IsOnAzure() && DomainUtil.IsInDomain())
            {
                ssdl = BrokerNodesManager.GetSDDI(nodeName, out ex);
            }

            return new NodeInfo(nodeName, domainName, ssdl, ex);
        }

        /// <summary>
        /// Retrieves BN's SSDL
        /// </summary>
        /// <param name="nodeName">indicating the node name</param>
        /// <param name="exception">output the exception if available</param>
        /// <returns>returns SSDL string if succeeded, returns null if failed</returns>
        internal static string GetSDDI(string nodeName, out Exception exception)
        {
            string sddi = null;
            exception = null;
            try
            {
                using (WindowsIdentity wi = new WindowsIdentity(nodeName))
                {
                    sddi = wi.User.ToString();
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "Cannot get BN {0} SSDL. Exception = {1}",
                            nodeName, e);
                exception = e;
            }

            return sddi;
        }

        /// <summary>
        /// Get the info of specified broker node
        /// </summary>
        /// <param name="brokerNodes">List of broker nodes</param>
        /// <param name="brokerNodeName">Name of the broker node</param>
        /// <returns>Broker node info</returns>
        private static BrokerNodeItem GetBrokerNode(List<BrokerNodeItem> brokerNodes, string brokerNodeName)
        {
            BrokerNodeItem result = null;

            foreach (BrokerNodeItem brokerNodeItem in brokerNodes)
            {
                if (0 == String.Compare(brokerNodeItem.Name, brokerNodeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = brokerNodeItem;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Connects to any BN failover clusters, marks which BNs have failover enabled
        /// and enumerates the brokerlauncher resource groups
        /// </summary>
        /// <remarks>
        /// Called under lock of failoverClusterConnections.
        /// </remarks>
        /// <param name="clusterNames"></param>
        private static void ConnectToBNFailoverClusters(string[] clusterNames, Dictionary<string, IntPtr> failoverClusterConnections)
        {
            IntPtr hCluster = IntPtr.Zero;

            // Enumerate cluster names
            foreach (string clusterName in clusterNames)
            {
                // If we do not have a handle to the cluster
                if (!failoverClusterConnections.ContainsKey(clusterName))
                {
                    // Open connection to the cluster
                    hCluster = Win32API.OpenCluster(clusterName);

                    if (hCluster != IntPtr.Zero)
                    {
                        failoverClusterConnections.Add(clusterName, hCluster);
                    }
                    else
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "Cannot connect to {0}. Error = {1}",
                                    clusterName, Marshal.GetLastWin32Error());
                    }
                }
            }
        }

        private delegate void GetFailoverClusterInfoDelegate(IntPtr failoverClusterConnection, List<string> brokerNodesWithFailover,
                        Dictionary<string, ResourceGroupInfo> resourceGroups);

        /// <summary>
        /// Returns the failover cluster info for multiple failover clusters
        /// </summary>
        /// <remarks>
        /// Needs to lock failoverClusterConnections in the caller side.
        /// </remarks>
        /// <param name="failoverClusterConnections">HANDLEs to multiple clusters</param>
        /// <param name="brokerNodesWithFailover">List to return cluster nodes</param>
        /// <param name="resourceGroups">List to return brokerlauncher resource groups</param>
        private void GetFailoverClusterInfo(Dictionary<string, IntPtr> failoverClusterConnections,
                        List<string> brokerNodesWithFailover, Dictionary<string, ResourceGroupInfo> resourceGroups)
        {
            List<string> failedClusterConnections = new List<string>();
            List<IAsyncResult> asyncResults = new List<IAsyncResult>();
            GetFailoverClusterInfoDelegate a = GetFailoverClusterInfo;

            // Loop through each cluster and asynchronously request its info. This is done async because a BN tier
            // could have multiple failover clusters (~up to 12 for 1000 node compute cluster)
            foreach (KeyValuePair<string, IntPtr> failoverClusterConnection in failoverClusterConnections)
            {
                IAsyncResult asyncResult = a.BeginInvoke(failoverClusterConnection.Value, brokerNodesWithFailover,
                                        resourceGroups, null, failoverClusterConnection.Key);
                asyncResults.Add(asyncResult);
            }

            // Loop through the results, wait for actions to complete and log any errors
            foreach (IAsyncResult asyncResult in asyncResults)
            {
                try
                {
                    a.EndInvoke(asyncResult);
                }

                catch (Exception e)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, e.ToString());

                    // Remove the connections to the clusters that could not be accessed successfully
                    // TODO: Consider removing failover cluster names that repeatedly cannot be connected to
                    failoverClusterConnections.Remove((string)asyncResult.AsyncState);
                }
            }
        }

        /// <summary>
        /// Returns the failover cluster info for a single failover cluster
        /// </summary>
        /// <param name="failoverClusterConnection">HANDLE to cluster</param>
        /// <param name="brokerNodesWithFailover">List to return cluster nodes</param>
        /// <param name="resourceGroups">List to return brokerlauncher resource groups</param>
        private void GetFailoverClusterInfo(IntPtr failoverClusterConnection, List<string> brokerNodesWithFailover,
                        Dictionary<string, ResourceGroupInfo> resourceGroups)
        {
            Debug.Assert(failoverClusterConnection != IntPtr.Zero);
            Debug.Assert(brokerNodesWithFailover != null);
            Debug.Assert(resourceGroups != null);

            IntPtr hClusterEnum = IntPtr.Zero;
            int enumResult = (int)CLUSTER_ENUM_RESULT.ERROR_SUCCESS;
            int index = 0;
            int nameLen = Win32API.MAX_HOST_NAME_LEN;
            uint type = 0;
            StringBuilder name = null;
            bool exit = false;

            try
            {
                // Enumerate nodes and resource groups
                hClusterEnum = Win32API.ClusterOpenEnum(failoverClusterConnection,
                            (uint)(CLUSTER_ENUM_TYPE.CLUSTER_ENUM_NODE | CLUSTER_ENUM_TYPE.CLUSTER_ENUM_GROUP));
                if (hClusterEnum == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), SR.BrokerNodesManager_CannotEnumerateInformation);

                while (!exit)
                {
                    name = new StringBuilder(nameLen);

                    enumResult = Win32API.ClusterEnum(hClusterEnum, index, out type, name, ref nameLen);

                    if (enumResult == (int)CLUSTER_ENUM_RESULT.ERROR_SUCCESS)
                    {
                        if (type == (uint)CLUSTER_ENUM_TYPE.CLUSTER_ENUM_GROUP)
                        {
                            AddFailoverResourceGroup(resourceGroups, failoverClusterConnection, name.ToString());
                        }
                        else if (type == (uint)CLUSTER_ENUM_TYPE.CLUSTER_ENUM_NODE)
                        {
                            AddFailoverNode(brokerNodesWithFailover, name.ToString());
                        }
                        else
                        {
                            TraceHelper.TraceEvent(TraceEventType.Warning,
                                    String.Format("Unexpected cluster object type {0} from cluster enum.", type));
                        }

                        nameLen = Win32API.MAX_HOST_NAME_LEN;

                        index++;
                    }
                    else if (enumResult == (int)CLUSTER_ENUM_RESULT.ERROR_NO_MORE_ITEMS)
                    {
                        exit = true;
                    }
                    else if (enumResult == (int)CLUSTER_ENUM_RESULT.ERROR_MORE_DATA)
                    {
                        // try same item again with returned nameLen
                        continue;
                    }
                    else
                    {
                        throw new Exception(String.Format(SR.UnexpectedReturnValue, enumResult));
                    }
                }
            }

            finally
            {
                if (hClusterEnum != IntPtr.Zero)
                    Win32API.ClusterCloseEnum(hClusterEnum);
            }
        }

        /// <summary>
        /// Adds a broker node with failover enabled to list
        /// </summary>
        /// <param name="brokerNodesWithFailover"></param>
        /// <param name="name"></param>
        private static void AddFailoverNode(List<string> brokerNodesWithFailover, string name)
        {
            name = name.ToUpper();

            lock (brokerNodesWithFailover)
            {
                if (!brokerNodesWithFailover.Contains(name))
                {
                    brokerNodesWithFailover.Add(name);
                }
            }
        }

        /// <summary>
        /// Adds resource group hosted on BN to list
        /// </summary>
        /// <param name="resourceGroups"></param>
        /// <param name="hCluster"></param>
        /// <param name="name"></param>
        private static void AddFailoverResourceGroup(Dictionary<string, ResourceGroupInfo> resourceGroups, IntPtr hCluster, string name)
        {
            name = name.ToUpper();

            lock (resourceGroups)
            {
                if (!resourceGroups.ContainsKey(name))
                {
                    ResourceGroupInfo resourceGroupInfo = ResourceGroupInfo.Get(hCluster, name);

                    if (resourceGroupInfo != null)
                    {
                        resourceGroups.Add(name, resourceGroupInfo);
                    }
                }
            }
        }
#endif
        /// <summary>
        /// Generate broker launcher epr
        /// </summary>
        /// <param name="machineName">machine name</param>
        /// <returns>broker launcher epr</returns>
        internal static string GenerateBrokerLauncherEpr(string endpointPrefix, string machineName, TransportScheme scheme)
        {
            if (endpointPrefix.Equals(HttpsPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return SoaHelper.GetBrokerLauncherAddress(machineName, scheme);
            }
            else if (endpointPrefix.Equals(NettcpPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                return SoaHelper.GetBrokerLauncherAddress(machineName);
            }

            throw new ArgumentException();
        }
    }
}