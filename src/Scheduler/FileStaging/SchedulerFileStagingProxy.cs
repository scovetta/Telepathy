//--------------------------------------------------------------------------
// <copyright file="SchedulerFileStagingProxy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This implementation of the file staging proxy runs on the head node
//     in the scheduler service. It listens on a different port so that it
//     doesn't interfere with the node manager's worker instance, and it
//     is responsible for taking requests from the client and forwarding
//     them to compute nodes or the Azure proxy.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Threading;
    using Microsoft.ComputeCluster.Management;
    using Microsoft.Hpc.Azure.Common;
    using Microsoft.Hpc.Management.FileTransfer;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Store;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// SchedulerFileStagingProxy runs on heade node.  It is responsible for
    /// taking requests from the client and forwarding them to compute nodes
    /// or the Azure proxy
    /// </summary>
    public class SchedulerFileStagingProxy : FileStagingProxy
    {
        /// <summary>
        /// SOA action of KeepAlive call
        /// </summary>
        private const string KeepAliveAction = @"http://hpc.microsoft.com/IFileStagingRouter/KeepAlive";

        /// <summary>
        /// Interval between two consequtive keep-alive operations: 25 seconds
        /// </summary>
        private const int KeepAliveIntervalInSeconds = 25;

        /// <summary>
        /// Config entry key for "NettcpOver443"
        /// </summary>
        private const string NettcpOver443Entry = @"NettcpOver443";

        /// <summary>
        /// Upper limit of idle time for cached GenericFileStagingClient.
        /// </summary>
        private static readonly TimeSpan TTL = TimeSpan.FromMinutes(30);

        /// <summary>
        /// It is time period for ttlTimer.
        /// </summary>
        private static readonly TimeSpan TtlTimerPeriod = TimeSpan.FromMinutes(10);

        /// <summary>
        /// List of channels that need to be keep-alived
        /// </summary>
        private Dictionary<GenericFileStagingClient, int> keepAlivedChannels
            = new Dictionary<GenericFileStagingClient, int>();

        /// <summary>
        /// Lock object for active channels
        /// </summary>
        private object lockKeepAlivedChannels = new object();

        /// <summary>
        /// The timer that keep alive channels to proxys in Azure
        /// </summary>
        private Timer channelKeepAliveTimer;

        /// <summary>
        /// The scheduler proxy needs a handle to the store to check for node information
        /// </summary>
        private ISchedulerStore store;

        /// <summary>
        /// It tracks last active timestamp for each GenericFileStagingClient.
        /// Key: logical node name
        /// Value: date time of last active time.
        /// </summary>
        private ConcurrentDictionary<string, DateTime> ttlCache
            = new ConcurrentDictionary<string, DateTime>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// This timer periodically checks the idle time of each cached
        /// GenericFileStagingClient, and removes it if timeout expires.
        /// </summary>
        private Timer ttlTimer;

        /// <summary>
        /// Initializes a new instance of the SchedulerFileStagingProxy class
        /// </summary>
        /// <param name="store">ISchedulerStore instance</param>
        /// <param name="useNettcpOver443">whetheer use nettcp binding over 443 or not</param>
        public SchedulerFileStagingProxy(ISchedulerStore store)
        {
            this.store = store;

            this.channelKeepAliveTimer =
                new Timer(KeepAliveChannels, null, TimeSpan.FromSeconds(KeepAliveIntervalInSeconds), TimeSpan.FromSeconds(KeepAliveIntervalInSeconds));

            this.ttlTimer =
                new Timer(this.TtlTimerCallback, null, (int)TtlTimerPeriod.TotalMilliseconds, Timeout.Infinite);
        }

        /// <summary>
        /// Update the due time of the timer to make it work next time.
        /// </summary>
        private void UpdateTimer()
        {
            try
            {
                this.ttlTimer.Change((int)TtlTimerPeriod.TotalMilliseconds, Timeout.Infinite);
            }
            catch (Exception e)
            {
                LocalProxyTraceHelper.TraceError(e, "UpdateTimer: Exception happens when update timer.");
            }
        }

        /// <summary>
        /// Implementations must stop the service with this method call
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            this.channelKeepAliveTimer?.Dispose();
            this.channelKeepAliveTimer = null;
            this.ttlTimer?.Dispose();
            this.ttlTimer = null;

            foreach(var client in this.keepAlivedChannels.Keys)
            {
                this.AsyncCloseICommunicationObject(client);
            }

            this.keepAlivedChannels.Clear();
        }

        /// <summary>
        /// Sets up the service so that it begins listening for clients
        /// </summary>
        public override void Start()
        {
            // listen uri for IFileStagingRounter interfaces
            Uri listenUri = FileStagingCommon.GetFileStagingEndpointOnHeadNode().Uri;

#if DEBUG
            // Construct service host with a base address that allows us to publish metadata
            serviceHost = new ServiceHost(this, new Uri("http://localhost:8080/FileStagingMetadata"));

            // Check to see if the service host already has a ServiceMetadataBehavior
            ServiceMetadataBehavior smb = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();

            // If not, add one
            if (smb == null)
            {
                smb = new ServiceMetadataBehavior();
            }

            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            serviceHost.Description.Behaviors.Add(smb);

            // Add MEX endpoint
            serviceHost.AddServiceEndpoint(
              ServiceMetadataBehavior.MexContractName,
              MetadataExchangeBindings.CreateMexHttpBinding(),
              "mex");
#else
            serviceHost = new ServiceHost(this);
#endif

            // Check to see if the service host already has a ServiceThrottlingBehavior
            ServiceThrottlingBehavior throttlingBehavior = serviceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();

            // If not, add one
            if (throttlingBehavior == null)
            {
                throttlingBehavior = new ServiceThrottlingBehavior();
            }

            throttlingBehavior.MaxConcurrentCalls = FileStagingCommon.MaxConcurrentCalls;
            serviceHost.Description.Behaviors.Add(throttlingBehavior);

            try
            {
                serviceHost.AddServiceEndpoint(typeof(IFileStagingRouter), FileStagingCommon.GetSecureFileStagingBinding(), listenUri);
                serviceHost.Open();

                LocalProxyTraceHelper.TraceInformation("Proxy is listening on to {0}.", listenUri.ToString());
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceCritical(ex, "Failed to create the proxy service on {0}.", listenUri.ToString());
                throw;
            }

            base.Start();
        }

        /// <summary>
        /// This service operation simply forwards a request to the targeted node
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        public override Message ProcessMessage(Message request)
        {
            GenericFileStagingClient client = null;
            string logicalName;

            // Get the information needed from the message headers (the target and the user ID).
            try
            {
                logicalName = request.Headers.GetHeader<string>(FileStagingCommon.WcfHeaderTargetNode, FileStagingCommon.WcfHeaderNamespace);
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceWarning(ex, "The node's logical name was missing from the headers.");
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(string.Format(Resources.Common_MissingHeaders, FileStagingCommon.WcfHeaderTargetNode), FileStagingErrorCode.AuthenticationFailed));
            }

            // Verify that the user is a cluster user
            string userName = this.CheckUserAccess();

            // Add a header that contains the user's SDDL
            string userSddl = ServiceSecurityContext.Current.WindowsIdentity.User.ToString();
            MessageHeader<string> messageHeaderSddl = new MessageHeader<string>(userSddl);
            MessageHeader untypedHeaderSddl = messageHeaderSddl.GetUntypedHeader(FileStagingCommon.WcfHeaderUserSddl, FileStagingCommon.WcfHeaderNamespace);
            request.Headers.Add(untypedHeaderSddl);

            // Add a header that contains the user name
            MessageHeader<string> messageHeaderUserName = new MessageHeader<string>(userName);
            MessageHeader untypedHeaderUserName = messageHeaderUserName.GetUntypedHeader(FileStagingCommon.WcfHeaderUserName, FileStagingCommon.WcfHeaderNamespace);
            request.Headers.Add(untypedHeaderUserName);

            // Add isAdmin header
            bool isAdmin = AuthenticationUtil.IsHpcAdmin(ServiceSecurityContext.Current.WindowsIdentity);
            MessageHeader<bool> messageHeaderIsAdmin = new MessageHeader<bool>(isAdmin);
            MessageHeader untypedHeaderIsAdmin = messageHeaderIsAdmin.GetUntypedHeader(FileStagingCommon.WcfHeaderIsAdmin, FileStagingCommon.WcfHeaderNamespace);
            request.Headers.Add((untypedHeaderIsAdmin));

            // Route the message
            try
            {
                // Send the message under the identity of the cluster rather than the 
                // identity of the head node.
                // TODO: Remove dependency on private APIs
                using (Hpc.HPCIdentity hpcIdentity = new Hpc.HPCIdentity())
                {
                    hpcIdentity.Impersonate();
                    client = this.GetChannel(logicalName);

                    // keep alive the channel when there is a call on it.
                    AddKeepAlive(client);

                    try
                    {
                        return client.ProcessMessage(request);
                    }
                    finally
                    {
                        RemoveKeepAlive(client);
                    }
                }
            }
            catch (FaultException<InternalFaultDetail> ex)
            {
                LocalProxyTraceHelper.TraceError(ex, "Internal fault exception while forwarding message.");
                throw;
            }
            catch (Exception ex)
            {
                if (client != null)
                {
                    try
                    {
                        client.Close();
                    }
                    catch
                    {
                        // If an exception was thrown, the client could not be closed. Abort instead.
                        client.Abort();
                    }
                }

                LocalProxyTraceHelper.TraceError(ex, "Exception while forwarding message.");
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(string.Format(CultureInfo.CurrentCulture, Resources.Common_CantForward, logicalName, ex.Message), FileStagingErrorCode.CommunicationFailure));
            }
        }

        /// <summary>
        /// Check that user is an authenticated user.
        /// </summary>
        /// <returns>user name of caller</returns>
        public override string CheckUserAccess()
        {
            try
            {
                // TODO: remove below traces
                LocalProxyTraceHelper.TraceInformation("begin check user access: {0}", DateTime.Now);

                // Verify that the user is a cluster admin, cluster user or local system account
                if (ServiceSecurityContext.Current.WindowsIdentity.IsSystem ||
                    AuthenticationUtil.IsHpcAdminOrUser(ServiceSecurityContext.Current.WindowsIdentity))
                {
                    LocalProxyTraceHelper.TraceInformation("end check user access: {0}", DateTime.Now);
                    return ServiceSecurityContext.Current.WindowsIdentity.Name;
                }
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceWarning(ex, "Exception thrown when looking up Admins group.");
            }

            LocalProxyTraceHelper.TraceWarning("Could not authenticate user {0} as an admin or cluster user.", ServiceSecurityContext.Current.WindowsIdentity.Name);
            throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(Resources.OnPremise_OnlyClusterUsersAllowed, FileStagingErrorCode.AuthenticationFailed));
        }

        /// <summary>
        /// Gets a channel to the specified compute node or to the Azure proxy for the specified Azure node
        /// </summary>
        /// <param name="logicalName">The logical name of a compute node or Azure node</param>
        /// <returns>a channel to the specified compute node or to the Azure proxy for the specified Azure node</returns>
        protected override GenericFileStagingClient GetChannel(string logicalName)
        {
            // Read value of "NettcpOver443" config.  Defaults to true.
            // This operation is supposed to be very fast because most of the time
            // it is served out of memory.
            bool useNettcpOver443 = true;
            Dictionary<string, string> configEntries = store.OpenStoreManager().GetConfigurationSettings();
            string configValue;
            if (configEntries.TryGetValue(NettcpOver443Entry, out configValue))
            {
                if (!Boolean.TryParse(configValue, out useNettcpOver443))
                {
                    useNettcpOver443 = true;
                    LocalProxyTraceHelper.TraceWarning("Cluster configuration entry for {0} set to non-Boolean Value", NettcpOver443Entry);
                }
            }

            lock (channelLock)
            {
                GenericFileStagingClient channel = null;
                if (!channels.TryGetValue(logicalName, out channel)
                    || !CheckChannelScheme(channel, useNettcpOver443)
                    || channel.State == CommunicationState.Faulted
                    || channel.State == CommunicationState.Closing
                    || channel.State == CommunicationState.Closed)
                {
                    if (channel != null)
                    {
                        this.AsyncCloseICommunicationObject(channel);

                        channel = null;
                    }

                    LocalProxyTraceHelper.TraceVerbose("Creating a new channel to {0}.", logicalName);
                    channel = this.CreateChannel(logicalName, useNettcpOver443);
                    channels[logicalName] = channel;
                }

                // update the timestamp in cache
                this.AddOrUpdateTtlTimestamp(logicalName);

                return channel;
            }
        }

        /// <summary>
        /// Gets the Azure storage account for accessing the Azure storage service
        /// </summary>
        /// <returns>Azure storage account for accessing the Azure storage service</returns>
        protected override CloudStorageAccount GetStorageAccount()
        {
            try
            {
                string storageConnstring = HpcContext.Get().GetAzureStorageConnectionStringAsync().GetAwaiter().GetResult();
                return CloudStorageAccount.Parse(storageConnstring);
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceError("SchedulerFileStagingProxy: failed to get storage account information. Exception = {0}", ex);
                return null;
            }
        }

        /// <summary>
        /// Timer callback to keep alive channels between SchedulerFileStaingProxy and
        /// AzureFileStaingProxys periodically.
        /// </summary>
        /// <param name="state">timer callback state</param>
        private void KeepAliveChannels(object state)
        {
            List<GenericFileStagingClient> channelList;
            lock (lockKeepAlivedChannels)
            {
                channelList = new List<GenericFileStagingClient>(keepAlivedChannels.Keys);
            }

            foreach (GenericFileStagingClient client in channelList)
            {
                if (client.State == CommunicationState.Created
                    || client.State == CommunicationState.Opened
                    || client.State == CommunicationState.Opening)
                {
                    // if client is already at invalid state, no need to keep it alive.
                    BeginKeepAliveChannel(client);
                }
            }
        }

        /// <summary>
        /// Keep alive the channel bewteen SchedulerFileStagingProxy and AzureFileStagingProxy
        /// by sending a KeepAlive message to AzureFileStaingProxy
        /// </summary>
        /// <param name="client">Channel to be keep-alived</param>
        private void BeginKeepAliveChannel(GenericFileStagingClient client)
        {
            Debug.Assert(client != null, "BeginKeepAliveChannel: client parameter should not be null.");

            string uri = string.Empty;

            Message keepAliveMessage = Message.CreateMessage(MessageVersion.Default, KeepAliveAction);

            try
            {
                uri = client.Endpoint.ListenUri.AbsoluteUri;

                LocalProxyTraceHelper.TraceInformation("BeginKeepAliveChannel, target endpoint is {0}.", uri);

                client.BeginProcessMessage(keepAliveMessage, EndKeepAliveChannel, client);
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceWarning(ex, "BeginKeepAliveChannel failed, target endpoint is {0}.", uri);
            }
        }

        /// <summary>
        /// Callback function to check KeepAlive result
        /// </summary>
        /// <param name="ar">async result object</param>
        private void EndKeepAliveChannel(IAsyncResult ar)
        {
            GenericFileStagingClient client = ar.AsyncState as GenericFileStagingClient;

            Debug.Assert(client != null, "EndKeepAliveChannel: ar.AsyncState should be GenericFileStagingClient type.");

            string uri = string.Empty;

            try
            {
                uri = client.Endpoint.ListenUri.AbsoluteUri;

                LocalProxyTraceHelper.TraceInformation("EndKeepAliveChannel, target endpoint is {0}.", uri);

                client.EndProcessMessage(ar);

                Debug.Assert(
                    !string.IsNullOrEmpty(client.LogicalNodeName),
                    "EndKeepAliveChannel: client.LogicalNodeName should not be null or empty.");

                // update the timestamp in cache after a successful heartbeat
                this.AddOrUpdateTtlTimestamp(client.LogicalNodeName);
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceWarning(ex, "EndKeepAliveChannel failed, target endpoint is {0}.", uri);
            }
        }

        /// <summary>
        /// Creeates a new channel to the specified compute node or to the Azure proxy for the specified Azure node
        /// </summary>
        /// <param name="logicalName">The logical name of a compute node or Azure node</param>
        /// <param name="useNettcpOver443">a value indicating whether the channel to the specified computer node should be over nettcp or not</param>
        /// <returns>a new channel to the specified compute node or to the Azure proxy for the specified Azure node</returns>
        private GenericFileStagingClient CreateChannel(string logicalName, bool useNettcpOver443)
        {
            // Check the store to see if the node is in Azure
            LocalProxyTraceHelper.TraceVerbose("Checking location of node {0}.", logicalName);
            using (IRowSet rowset = this.store.OpenNodeRowSet())
            {
                rowset.SetColumns(NodePropertyIds.AzureProxyAddress);
                rowset.SetFilter(
                    new FilterProperty(FilterOperator.Equal, NodePropertyIds.Name, logicalName),
                    new FilterProperty(FilterOperator.NotEqual, NodePropertyIds.Location, NodeLocation.OnPremise));

                if (rowset.GetCount() == 0)
                {
                    // It is a local endpoint. Open a channel to this endpoint, with impersonation enabled.
                    LocalProxyTraceHelper.TraceVerbose("Node {0} is a compute node.", logicalName);
                    return new GenericFileStagingClient(FileStagingCommon.GetSecureFileStagingBinding(), FileStagingCommon.GetFileStagingEndpoint(logicalName), logicalName);
                }
                else if (rowset.GetCount() == 1)
                {
                    // The node is in Azure. Get the right proxy for the node and point an endpoint string to it.
                    LocalProxyTraceHelper.TraceVerbose("Node {0} is in Azure.", logicalName);

                    string jobManagerProxyAddressString = rowset.GetRows(0, 1)[0][NodePropertyIds.AzureProxyAddress].Value as string;
                    EndpointAddress jobManagerProxyAddress = new EndpointAddress(jobManagerProxyAddressString);

                    EndpointAddress targetEndpoint = useNettcpOver443 ?
                        FileStagingCommon.GetFileStagingEndpoint(jobManagerProxyAddress.Uri.Host, SchedulerPorts.FileStagingAzurePort, EndpointIdentity.CreateDnsIdentity(FileStagingCommon.HpcAzureProxyServerIdentity)) :
                        FileStagingCommon.GetHttpsFileStagingEndpoint(jobManagerProxyAddress.Uri.Host, SchedulerPorts.FileStagingAzurePort, EndpointIdentity.CreateDnsIdentity(FileStagingCommon.HpcAzureProxyServerIdentity));

                    LocalProxyTraceHelper.TraceVerbose("Creating a client with endpoint {0}.", targetEndpoint.ToString());

                    Binding binding = useNettcpOver443 ? FileStagingCommon.GetCertificateFileStagingBinding() : FileStagingCommon.GetHttpsFileStagingBinding();
                    GenericFileStagingClient client = new GenericFileStagingClient(binding, targetEndpoint, logicalName);

                    client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                    // Use the HpcAzureProxy certificate with this client
                    client.ClientCredentials.ClientCertificate.SetCertificate(
                        StoreLocation.LocalMachine,
                        StoreName.My,
                        X509FindType.FindBySubjectDistinguishedName,
                        FileStagingCommon.HpcAzureProxyClientCertName);

                    client.ClientCredentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

                    return client;
                }
                else
                {
                    // I'm not sure how more than one match would be found, but this is definitely a problem
                    LocalProxyTraceHelper.TraceWarning("Multiple locations found for node {0}.", logicalName);
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(Resources.OnPremise_MultipleMatches, FileStagingErrorCode.EndpointNotFound));
                }
            }
        }

        /// <summary>
        /// Check if scheme of a channel is correct
        /// </summary>
        /// <param name="channel">channel to be checked</param>
        /// <param name="useNettcpOver443">whether the channel is a nettcp channel over 443, or https over 443</param>
        /// <returns>true if scheme of the specified channel is correct, false otherwise</returns>
        private static bool CheckChannelScheme(GenericFileStagingClient channel, bool useNettcpOver443)
        {
            string expectedScheme = useNettcpOver443 ? "net.tcp" : "https";
            return channel.Endpoint.ListenUri.Scheme.Equals(expectedScheme, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Keep alive a channel between SchedulerFileStagingProxy and AzureFileStagingProxy
        /// </summary>
        /// <param name="channel">channel to be keep-alived</param>
        private void AddKeepAlive(GenericFileStagingClient channel)
        {
            string uri = channel.Endpoint.ListenUri.AbsoluteUri;

            LocalProxyTraceHelper.TraceInformation("AddKeepAlive, target endpoint is {0}.", uri);

            lock (lockKeepAlivedChannels)
            {
                int count;

                if (keepAlivedChannels.TryGetValue(channel, out count))
                {
                    count++;
                }
                else
                {
                    count = 1;
                }

                keepAlivedChannels[channel] = count;
            }
        }

        /// <summary>
        /// Stop keeping alive a channel
        /// </summary>
        /// <param name="channel">channel not to be keep-alived any longer</param>
        private void RemoveKeepAlive(GenericFileStagingClient channel)
        {
            string uri = channel.Endpoint.ListenUri.AbsoluteUri;

            LocalProxyTraceHelper.TraceInformation("StopKeepAlive, target endpoint is {0}.", uri);

            bool removed = false;

            lock (lockKeepAlivedChannels)
            {
                int count;

                if (!keepAlivedChannels.TryGetValue(channel, out count))
                {
                    return;
                }

                count--;

                if (count <= 0)
                {
                    removed = keepAlivedChannels.Remove(channel);
                }
                else
                {
                    keepAlivedChannels[channel] = count;
                }
            }

            if (removed)
            {
                LocalProxyTraceHelper.TraceInformation("StopKeepAlive, remove a channel, target endpoint is {0}.", uri);
            }
        }

        /// <summary>
        /// Add or update the timestamp in cache for specified logical node name.
        /// </summary>
        /// <param name="logicalName">logical node name</param>
        private void AddOrUpdateTtlTimestamp(string logicalName)
        {
            var now = DateTime.UtcNow;
            this.ttlCache.AddOrUpdate(logicalName, now, (key, value) => now);
        }

        /// <summary>
        /// Remove the timestamp from cache for specified logical node name.
        /// </summary>
        /// <param name="logicalName">logical node name</param>
        private void RemoveTtlTimestamp(string logicalName)
        {
            DateTime timestamp;
            this.ttlCache.TryRemove(logicalName, out timestamp);
        }

        /// <summary>
        /// Callback routine of the TTL timer.
        /// </summary>
        private void TtlTimerCallback(object state)
        {
            try
            {
                LocalProxyTraceHelper.TraceVerbose("TtlTimerCallback is triggered.");

                var keys = this.ttlCache.Keys;

                DateTime timestamp;

                foreach (var key in keys)
                {
                    if (this.ttlCache.TryGetValue(key, out timestamp))
                    {
                        if (DateTime.UtcNow - timestamp >= TTL)
                        {
                            LocalProxyTraceHelper.TraceVerbose("TtlTimerCallback: Client for {0} exceeds ttl period, it will be removed.", key);

                            GenericFileStagingClient client;

                            lock (channelLock)
                            {
                                if (this.channels.TryGetValue(key, out client))
                                {
                                    if (!this.channels.Remove(key))
                                    {
                                        LocalProxyTraceHelper.TraceWarning("TtlTimerCallback: Failed to remove client for {0} from cache.", key);
                                    }

                                    this.AsyncCloseICommunicationObject(client);
                                }
                                else
                                {
                                    LocalProxyTraceHelper.TraceWarning("TtlTimerCallback: Failed to get client for {0} from cache.", key);
                                }

                                this.RemoveTtlTimestamp(key);
                            }
                        }
                        else
                        {
                            LocalProxyTraceHelper.TraceVerbose("TtlTimerCallback: Client for {0} is within ttl period.", key);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LocalProxyTraceHelper.TraceError(e, "TtlTimerCallback: Exception happens when cleanup ttl cache.");
            }
            finally
            {
                this.UpdateTimer();
            }
        }

        /// <summary>
        /// Helper class to async close ICommunicationObject
        /// </summary>
        /// <param name="obj">indicating the object</param>
        private void AsyncCloseICommunicationObject(ICommunicationObject obj)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }

                if (obj.State == CommunicationState.Faulted)
                {
                    obj.Abort();
                }
                else
                {
                    obj.BeginClose(CallbackToCloseChannel, obj);
                }
            }
            catch (Exception e)
            {
                LocalProxyTraceHelper.TraceWarning("AsyncCloseICommunicationObject: Error happens, ignore exception, {0}", e);
            }
        }

        /// <summary>
        /// Callback to close ICommunicationObject
        /// </summary>
        /// <param name="result">indicating the async result</param>
        private void CallbackToCloseChannel(IAsyncResult result)
        {
            ICommunicationObject obj = (ICommunicationObject)result.AsyncState;

            try
            {
                obj.EndClose(result);
            }
            catch (Exception e)
            {
                LocalProxyTraceHelper.TraceWarning("CallbackToCloseChannel: Error happens when close client, ignore exception, {0}", e);

                try
                {
                    obj.Abort();
                }
                catch (Exception ex)
                {
                    LocalProxyTraceHelper.TraceWarning("CallbackToCloseChannel: Error happens when abort client, ignore exception, {0}", ex);
                }
            }
        }

        public override byte[] GetAzureLocalLogFile(string instanceName, string fileName)
        {
            throw new NotSupportedException("GetAzureLocalLogFile");
        }

        public override HpcFileInfo[] GetAzureLocalLogFileList(string instanceName, DateTime startTime, DateTime endTime)
        {
            throw new NotSupportedException("GetAzureLocalLogFileList");
        }
    }
}
