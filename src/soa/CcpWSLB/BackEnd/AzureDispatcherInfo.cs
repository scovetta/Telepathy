// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.ServiceBroker.Azure;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Dispatcher info for service task that targets Azure nodes.
    /// </summary>
    internal class AzureDispatcherInfo : ServiceTaskDispatcherInfo
    {
        /// <summary>
        /// It indicates whether soa burst uses nettcp connection.
        /// </summary>
        private bool nettcp = true;

        /// <summary>
        /// Initializes a new instance of the AzureDispatcherInfo class
        /// </summary>
        /// <param name="jobId">indicating the job id</param>
        /// <param name="requeueCount">indicating the job requeue count</param>
        /// <param name="taskId">indicating the task id</param>
        /// <param name="capacity">indicating the capacity</param>
        /// <param name="machineName">indicating the machine name</param>
        /// <param name="firstCoreId">indicating the first core id</param>
        /// <param name="networkPrefix">indicating the network prefix</param>
        /// <param name="proxyServiceName">azure service name</param>
        /// <param name="nettcp">indicating the protocal</param>
        /// <param name="azureLoadBalancerAddress">the DNS name or internal IP for proxy node</param>
        public AzureDispatcherInfo(string jobId, int requeueCount, string taskId, int capacity, string machineName, int firstCoreId, string networkPrefix, string proxyServiceName, bool nettcp, string azureLoadBalancerAddress)
            : base(jobId, taskId, capacity, machineName, null, firstCoreId, networkPrefix, Hpc.Scheduler.Session.Data.NodeLocation.Azure, false)
        {
            this.AzureServiceName = proxyServiceName;
            this.AzureLoadBalancerAddress = azureLoadBalancerAddress;
            this.RequeueCount = requeueCount;
            this.nettcp = nettcp;
        }

        /// <summary>
        /// Gets the azure service name.
        /// </summary>
        public string AzureServiceName { get; private set; }

        /// <summary>
        /// the DNS name or internal IP for proxy node
        /// </summary>
        public string AzureLoadBalancerAddress { get; private set; }

        /// <summary>
        /// Gets the job requeue count.
        /// </summary>
        public int RequeueCount { get; private set; }

        /// <summary>
        /// Gets the enpoint address for a service host
        /// </summary>
        /// <param name="isController">indicating whether controller address is required</param>
        /// <returns>endpoint address</returns>
        protected override EndpointAddress GetEndpointAddress(bool isController)
        {
            if (!isController)
            {
                if (this.nettcp)
                {
                    return new EndpointAddress(
                        new Uri(string.Format(Constant.BrokerProxyEndpointFormat, this.AzureLoadBalancerAddress, BrokerProxyPorts.ProxyPortV4RTM, BrokerProxyEndpointNames.SoaBrokerProxy)),
                        EndpointIdentity.CreateDnsIdentity(Constant.HpcAzureProxyServerIdentity),
                        new AddressHeaderCollection());
                }
                else
                {
                    // Calculation messages go through Azure storage queue, so
                    // EndpointAddress here is not needed. But we already have
                    // much code writing trace, so create a dummy one here.
                    return new EndpointAddress(
                        new Uri(string.Format(Constant.BrokerProxyEndpointFormatHttps, this.AzureLoadBalancerAddress, BrokerProxyPorts.ProxyPortV4RTM, BrokerProxyEndpointNames.SoaBrokerProxy)),
                        EndpointIdentity.CreateDnsIdentity(Constant.HpcAzureProxyServerIdentity),
                        new AddressHeaderCollection());
                }
            }
            else
            {
                string format = null;

                if (this.nettcp)
                {
                    format = Constant.BrokerProxyManagementEndpointFormat;
                }
                else
                {
                    format = Constant.BrokerProxyManagementEndpointFormatHttps;
                }

                return new EndpointAddress(
                    new Uri(string.Format(format, this.AzureLoadBalancerAddress, BrokerProxyPorts.ManagementPortV4RTM, BrokerProxyEndpointNames.SoaProxyControl)),
                    EndpointIdentity.CreateDnsIdentity(Constant.HpcAzureProxyServerIdentity),
                    new AddressHeaderCollection());
            }
        }
    }

    internal class BrokerProxyPorts
    {
        private const string _ProxyPortSettingV3SP2 = "5901";
        private const string _ProxyPortSettingV3SP3 = "443";

        private const string _ManagementPortSettingV3SP2 = "5902";
        private const string _ManagementPortSettingV3SP3 = "443";

        public const string ProxyPortV4RTM = "443";
        public const string ManagementPortV4RTM = "443";
    }
}
