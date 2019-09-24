// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Data;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Dispatcher info for service task that targets Azure nodes.
    /// </summary>
    internal class WssDispatcherInfo : ServiceTaskDispatcherInfo
    {
        /// <summary>
        /// Initializes a new instance of the WssDispatcherInfo class
        /// </summary>
        /// <param name="jobId">indicating the job id</param>
        /// <param name="taskId">indicating the task id</param>
        /// <param name="capacity">indicating the capacity</param>
        /// <param name="machineName">indicating the machine name</param>
        /// <param name="firstCoreId">indicating the first core id</param>
        /// <param name="networkPrefix">indicating the network prefix</param>
        /// <param name="location">indicating target resource location, OnPremise or Azure</param>
        public WssDispatcherInfo(string jobId, string taskId, int capacity, string machineName, string machineVirtualName, int firstCoreId, string networkPrefix, NodeLocation location)
            : base(jobId, taskId, capacity, machineName, machineVirtualName, firstCoreId, networkPrefix, location, true)
        {
        }

        /// <summary>
        /// Gets the enpoint address for a service host
        /// </summary>
        /// <param name="isController">indicating whether controller address is required</param>
        /// <returns>endpoint address</returns>
        protected override EndpointAddress GetEndpointAddress(bool isController)
        {
            string hostnameWithPrefix = this.MachineName;
            int port = PortHelper.ConvertToPort(this.firstCoreId, isController);

            if (!String.IsNullOrEmpty(this.networkPrefix))
            {
                hostnameWithPrefix = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", this.networkPrefix, hostnameWithPrefix);
            }

            string epr = BindingHelper.GenerateServiceHostEndpointAddress(hostnameWithPrefix, this.jobId, this.taskId, port, this.isHttp) + Constant.ServiceHostEndpointPath;
            if (isController)
            {
                epr += Constant.ServiceHostControllerEndpointPath;
                BrokerTracing.TraceVerbose("[WssDispatcherInfo]. Service host controller EPR = {0}.", epr);
            }
            else
            {
                BrokerTracing.TraceVerbose("[WssDispatcherInfo]. Service host EPR = {0}.", epr);
            }

            return new EndpointAddress(
                new Uri(epr),
                EndpointIdentity.CreateDnsIdentity(Constant.HpcWssServiceIdentity),
                new AddressHeaderCollection()
                );
        }

    }
}
