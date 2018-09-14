//--------------------------------------------------------------------------
// <copyright file="AzureFileStagingProxy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     Runs as part of the proxy worker role in Azure and redirects requests
//     to Azure worker nodes which do not have public endpoints.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using Microsoft.Hpc.Azure.Common;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.Hpc.Management.FileTransfer;
    using System.Collections.Generic;

    /// <summary>
    /// AzureFileStagingProxy runs as part of the proxy worker role in Azure.
    /// It redirects requests from SchedulerFileStaingProxy to Azure worker nodes.
    /// </summary>
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public class AzureFileStagingProxy : FileStagingProxy, IDisposable
    {
        /// <summary>
        /// Use the node mapping table to get endpoints for worker roles
        /// </summary>
        private NodeMapping nodeMapping = null;

        /// <summary>
        /// Refresh the nodemapping periodically
        /// </summary>
        private System.Timers.Timer nodemappingRefreshTimer = new System.Timers.Timer();

        /// <summary>
        /// Initializes a new instance of the AzureFileStagingProxy class
        /// </summary>
        public AzureFileStagingProxy()
        {
            // set up access to the node mapping table
            CloudStorageAccount acct = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.DataConnectionString));
            string tableName = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.NodeMapping);

            this.nodeMapping = new NodeMapping(acct, tableName);
            this.nodeMapping.RefreshMapping();

            this.nodemappingRefreshTimer.AutoReset = true;
            this.nodemappingRefreshTimer.Interval = 30 * 1000;
            this.nodemappingRefreshTimer.Elapsed += this.RefreshNodeMapping;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.nodemappingRefreshTimer.Dispose();
            }
        }

        /// <summary>
        /// Sets up the service so that it begins listening for clients
        /// </summary>
        public override void Start()
        {
            AzureProxyTraceHelper.TraceInformation("Starting File Staging Proxy.");
            serviceHost = new ServiceHost(this);

            // Use the HpcAzureProxy certificate to protect this endpoint
            serviceHost.Credentials.ServiceCertificate.SetCertificate(
                FileStagingCommon.HpcAzureProxyServerCertName,
                StoreLocation.LocalMachine,
                StoreName.Root);

            if (serviceHost.Credentials.ServiceCertificate != null && serviceHost.Credentials.ServiceCertificate.Certificate != null)
            {
                AzureProxyTraceHelper.WriteLine(
                    "Using certificate \"{0}\" with thumbprint {1}.",
                    serviceHost.Credentials.ServiceCertificate.Certificate.ToString(),
                    serviceHost.Credentials.ServiceCertificate.Certificate.Thumbprint);
            }
            else
            {
                AzureProxyTraceHelper.WriteLine("Not using certificate.");
            }

            // Check to see if the service host already has a ServiceThrottlingBehavior
            ServiceThrottlingBehavior throttlingBehavior = serviceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();

            // If not, add one
            if (throttlingBehavior == null)
            {
                throttlingBehavior = new ServiceThrottlingBehavior();
            }

            throttlingBehavior.MaxConcurrentCalls = FileStagingCommon.MaxConcurrentCalls;
            serviceHost.Description.Behaviors.Add(throttlingBehavior);

            // Define an external endpoint for client traffic
            RoleInstanceEndpoint externalEndPoint =
                RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[SchedulerEndpointNames.FileStagingServiceEndpoint];

            Uri listenUri = null;
            Uri endpointUri;
            if (AzureHelper.IsNettcpOver443)
            {
                listenUri = FileStagingCommon.GetFileStagingEndpoint(externalEndPoint.IPEndpoint.Address.ToString(), externalEndPoint.IPEndpoint.Port.ToString()).Uri;
            }
            else
            {
                listenUri = FileStagingCommon.GetHttpsFileStagingEndpoint(externalEndPoint.IPEndpoint.Address.ToString(), externalEndPoint.IPEndpoint.Port.ToString(), null).Uri;
            }

            if (!RoleEnvironment.IsAvailable)
            {
                // Build an endpoint URL that is valid when running in Azure test fabric
                endpointUri = FileStagingCommon.GetFileStagingEndpoint().Uri;
            }
            else
            {
                // Build an endpoint URL that is valid when running in Azure
                UriBuilder endpointUriBuilder = new UriBuilder(listenUri);
                endpointUriBuilder.Host = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.ServiceName) + "." + RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.ServiceDomain);
                endpointUriBuilder.Path = SchedulerEndpointNames.FileStagingService;
                endpointUriBuilder.Port = int.Parse(SchedulerPorts.FileStagingAzurePort);
                endpointUri = endpointUriBuilder.Uri;
            }

            try
            {
                Binding binding;
                if (AzureHelper.IsNettcpOver443)
                {
                    binding = FileStagingCommon.GetCertificateFileStagingBinding();

                    // If using Nettcp binding, require port sharing
                    (binding as NetTcpBinding).PortSharingEnabled = true;
                }
                else
                {
                    binding = FileStagingCommon.GetHttpsFileStagingBinding();
                }

                // Open the service
                serviceHost.AddServiceEndpoint(typeof(IFileStagingRouter), binding, endpointUri, listenUri);
                serviceHost.Open();

                AzureProxyTraceHelper.TraceInformation("Proxy opened on Uri {0}, listening on {1}.", endpointUri, listenUri);
            }
            catch (Exception ex)
            {
                AzureProxyTraceHelper.TraceError("Exception thrown while trying to create the node file staging service on {0}: {1}", listenUri, ex);

                // Rethrow the exception to notify the client that the service is not started.
                throw;
            }

            this.nodemappingRefreshTimer.Start();
            base.Start();
        }

        /// <summary>
        /// Overrides the implementation of the service so that the target node can be removed from the header. This causes the Azure node to process the
        /// message instead of passing it back (since it doesn't know its own name and will expect that the message is intended for another node).
        /// </summary>
        /// <param name="request">raw request message</param>
        /// <returns>processed request message</returns>
        public override Message ProcessMessage(Message request)
        {
            GenericFileStagingClient client = null;
            string logicalName;

            try
            {
                // Get the targeted node from the headers
                logicalName = request.Headers.GetHeader<string>(FileStagingCommon.WcfHeaderTargetNode, FileStagingCommon.WcfHeaderNamespace);
            }
            catch (Exception ex)
            {
                AzureProxyTraceHelper.TraceWarning("The node's logical name was missing from the headers. Exception: {0}", ex);
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(string.Format(Resources.Common_MissingHeaders, FileStagingCommon.WcfHeaderTargetNode), FileStagingErrorCode.CommunicationFailure));
            }

            try
            {
                // Remove the contents of the targeted node header so that the client will process the message instead of forwarding it
                MessageHeader<string> mh = new MessageHeader<string>(string.Empty);
                request.Headers.RemoveAt(request.Headers.FindHeader(FileStagingCommon.WcfHeaderTargetNode, FileStagingCommon.WcfHeaderNamespace));
                request.Headers.Add(mh.GetUntypedHeader(FileStagingCommon.WcfHeaderTargetNode, FileStagingCommon.WcfHeaderNamespace));

                client = this.GetChannel(logicalName);
                return client.ProcessMessage(request);
            }
            catch (FaultException<InternalFaultDetail> ex)
            {
                AzureProxyTraceHelper.TraceError("Internal fault exception while forwarding message: {0}", ex);
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

                AzureProxyTraceHelper.TraceError("Exception while forwarding message: {0}", ex);
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
                // FIXME: check always success only if user has the certificate.
                MessageHeaders incomingHeaders = System.ServiceModel.OperationContext.Current.IncomingMessageHeaders;
                string userName = incomingHeaders.GetHeader<string>(FileStagingCommon.WcfHeaderUserName, FileStagingCommon.WcfHeaderNamespace);

                return userName;
            }
            catch (Exception ex)
            {
                AzureProxyTraceHelper.TraceError("Could not read \"{0}\" from message headers. Exception: {1}", FileStagingCommon.WcfHeaderUserName, ex);
                throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(string.Format(Resources.Common_MissingHeaders, FileStagingCommon.WcfHeaderUserName), FileStagingErrorCode.CommunicationFailure));
            }
        }

        /// <summary>
        /// Gets a channel to the specified Azure node by using the node mapping table if necessary
        /// </summary>
        /// <param name="logicalName">logical name of target Azure node</param>
        /// <returns>a channel to the specified Azure node</returns>
        protected override GenericFileStagingClient GetChannel(string logicalName)
        {
            lock (channelLock)
            {
                GenericFileStagingClient channel = null;
                if (!channels.TryGetValue(logicalName, out channel)
                    || channel.State == CommunicationState.Faulted
                    || channel.State == CommunicationState.Closing
                    || channel.State == CommunicationState.Closed)
                {
                    if (channel != null)
                    {
                        // Means the previous channel failed, suspect the mapping isn't up to date
                        // Reload the mapping
                        AzureProxyTraceHelper.WriteLine("Refreshing node mapping table.");
                        this.nodeMapping.RefreshMapping();
                    }

                    channel = this.CreateChannel(logicalName);
                    channels[logicalName] = channel;
                }

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
                return CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.StorageConnectionString));
            }
            catch (Exception ex)
            {
                LocalProxyTraceHelper.TraceError("AzureFileStagingProxy: failed to get storage account information. Exception = {0}", ex);
                return null;
            }
        }

        /// <summary>
        /// Opens a channel to the specified Azure node
        /// </summary>
        /// <param name="logicalName">logical name of the target Azure node</param>
        /// <returns>a channel to the specified Azure node</returns>
        private GenericFileStagingClient CreateChannel(string logicalName)
        {
            string physicalEpr = null;
            if (!this.nodeMapping.TryGetNodeEndpoint(logicalName, Module.FileStaging, out physicalEpr))
            {
                AzureProxyTraceHelper.WriteLine("CreateChannel: Refreshing node mapping table.");
                this.nodeMapping.RefreshMapping();
                if (!this.nodeMapping.TryGetNodeEndpoint(logicalName, Module.FileStaging, out physicalEpr))
                {
                    AzureProxyTraceHelper.TraceWarning("CreateChannel: Could not find node {0} in node mapping table.", logicalName);
                    throw new FaultException<InternalFaultDetail>(new InternalFaultDetail(string.Format(CultureInfo.CurrentCulture, Resources.Azure_MissingNodeMapping, logicalName), FileStagingErrorCode.EndpointNotFound));
                }
            }

            string endpoint = string.Format(CultureInfo.CurrentCulture, "{0}/{1}", physicalEpr, SchedulerEndpointNames.FileStagingService);
            AzureProxyTraceHelper.TraceInformation(string.Format(CultureInfo.CurrentCulture, "Created a new file staging client toward {0}.", endpoint));
            return new GenericFileStagingClient(FileStagingCommon.GetFileStagingBinding(), new EndpointAddress(endpoint));
        }

        private Dictionary<string, string> serverAddress;

        /// <summary>
        /// Opens a channel to the specified server
        /// </summary>
        /// <param name="server"></param>
        private FileTransferChannel OpenClientToServer(string server)
        {
            Uri listenUri = new Uri(string.Format(CultureInfo.InvariantCulture, string.Format("net.tcp://{0}:{1}/FileTransfer", server, SchedulerPorts.FileStagingHeadNodePort)));

            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None, false);
            binding.MaxReceivedMessageSize = 2147483647;    // 2 GB
            binding.ReaderQuotas.MaxArrayLength = 2147483647;  // 2 GB
            binding.ReceiveTimeout = TimeSpan.FromHours(1);
            return new FileTransferChannel(binding, new EndpointAddress(listenUri));
        }

        private string GetServerAddress(string instanceName)
        {
            if (this.serverAddress == null || !this.serverAddress.ContainsKey(instanceName))
            {
                if (!RoleEnvironment.IsAvailable)
                {
                    // Without a role environment, there is no way to find servers
                    AzureProxyTraceHelper.TraceWarning("The role environment is not available and no server address is set.");
                }
                else
                {
                    if (this.serverAddress == null)
                    {
                        this.serverAddress = new Dictionary<string, string>();
                    }

                    foreach (Role role in RoleEnvironment.Roles.Values)
                    {
                        foreach (RoleInstance roleInstance in role.Instances)
                        {
                            if (!this.serverAddress.ContainsKey(roleInstance.Id.ToUpperInvariant()))
                            {
                                AzureProxyTraceHelper.WriteLine("Add ipaddress for instance {0}", roleInstance.Id);
                                serverAddress.Add(roleInstance.Id.ToUpperInvariant(), roleInstance.InstanceEndpoints[SchedulerEndpointNames.FileTransfer].IPEndpoint.Address.ToString());
                            }
                        }
                    }
                }
            }

            if (this.serverAddress != null && this.serverAddress.ContainsKey(instanceName))
            {
                AzureProxyTraceHelper.WriteLine("The server ip for instance {0} is {1}", instanceName, this.serverAddress[instanceName]);
                return this.serverAddress[instanceName];
            }

            AzureProxyTraceHelper.TraceWarning("Could not find a proxy ip for instance {0}", instanceName);

            return null;
        }

        /// <summary>
        /// Get the log file stream
        /// The load balancer will forward the function call to any proxy node, so we can't directly get log from the current machine
        /// We redirect the function to correct proxy node, and use HostFileDistributor service to do real work.
        /// </summary>
        /// <param name="instanceName">proxy instance name, such as HpcProxy_IN_0</param>
        /// <param name="fileName">log file name, such as hpcproxy_000000.bin</param>
        /// <returns></returns>
        public override byte[] GetAzureLocalLogFile(string instanceName, string fileName)
        {
            byte[] output = null;

            string ipAddress = GetServerAddress(instanceName.ToUpperInvariant());

            if (!string.IsNullOrEmpty(ipAddress))
            {
                using (FileTransferChannel client = OpenClientToServer(ipAddress))
                {
                    try
                    {
                        output = client.GetAzureLocalLogFile(fileName);
                    }
                    catch (EndpointNotFoundException)
                    {
                        // Abort the channel if server is not ready.
                        string message = "The server is not ready yet. Trying again after a short wait...";
                        AzureProxyTraceHelper.TraceWarning(message);
                        throw new FaultException<FileTransferFaultDetail>(new FileTransferFaultDetail(message, FileTransferFaultCode.NotInitialized));
                    }
                    catch (FaultException<FileTransferFaultDetail> fexp)
                    {
                        client.Abort();
                        //throw fexp; --> can't throw the original exception from FileTransfer service here as it has different namespace
                        //than exception directly thrown by this method, and it will not be correctly interpreted by client code.
                        throw new FaultException<FileTransferFaultDetail>(fexp.Detail);
                    }
                    catch (Exception exp)
                    {
                        client.Abort();
                        AzureProxyTraceHelper.TraceWarning(string.Format("Exception happened at GetAzureLocalLogFile {0}", exp));
                        throw new FaultException<FileTransferFaultDetail>(new FileTransferFaultDetail(exp.ToString(), FileTransferFaultCode.DownloadFile, exp));
                    }

                    try
                    {
                        // Attempt to close the channel.
                        client.Close();
                    }
                    catch
                    {
                        // Abort the channel if closing it failed.
                        client.Abort();
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Get the log file name list
        /// The load balancer will forward the function call to any proxy node, so we can't directly get log file list from the current machine
        /// We redirect the function to correct proxy node, and use HostFileDistributor service to do real work.
        /// </summary>
        /// <param name="instanceName">proxy instance name, such as HpcProxy_IN_0</param>
        /// <returns></returns>
        public override HpcFileInfo[] GetAzureLocalLogFileList(string instanceName, DateTime startTime, DateTime endTime)
        {
            HpcFileInfo[] output = null;

            string ipAddress = GetServerAddress(instanceName.ToUpperInvariant());

            if (!string.IsNullOrEmpty(ipAddress))
            {
                using (FileTransferChannel client = OpenClientToServer(ipAddress))
                {
                    try
                    {
                        output = client.GetAzureLocalLogFileList(startTime, endTime);
                    }
                    catch (EndpointNotFoundException)
                    {
                        // Abort the channel if server is not ready.
                        string message = "The server is not ready yet. Trying again after a short wait...";
                        AzureProxyTraceHelper.TraceWarning(message);
                        throw new FaultException<FileTransferFaultDetail>(new FileTransferFaultDetail(message, FileTransferFaultCode.NotInitialized));
                    }
                    catch (Exception exp)
                    {
                        client.Abort();
                        AzureProxyTraceHelper.TraceWarning(string.Format("Expcetion happened at GetAzureLocalLogFileList {0}", exp));
                        throw new FaultException<FileTransferFaultDetail>(new FileTransferFaultDetail(exp.ToString(), FileTransferFaultCode.RetrieveFileList));
                    }

                    try
                    {
                        // Attempt to close the channel.
                        client.Close();
                    }
                    catch
                    {
                        // Abort the channel if closing it failed.
                        client.Abort();
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Refresh node mapping table
        /// </summary>
        private void RefreshNodeMapping(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.nodeMapping.RefreshMapping();
            }
            catch (Exception ex)
            {
                AzureProxyTraceHelper.TraceWarning(string.Format("Refresh node mapping table failed. {0}", ex));
            }
        }
    }
}
