// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System;
    using System.Globalization;
    using System.ServiceModel;

    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Dispatcher info for service task
    /// </summary>
    internal class ServiceTaskDispatcherInfo : DispatcherInfo
    {
        /// <summary>
        /// Stores the job id
        /// </summary>
        protected string jobId;

        /// <summary>
        /// Stores the task id
        /// </summary>
        protected string taskId;

        /// <summary>
        /// The first core id
        /// </summary>
        protected int firstCoreId;

        /// <summary>
        /// Stores the network prefix
        /// </summary>
        protected string networkPrefix;

        /// <summary>
        /// Stores a value indicating whether the service task is opening an HTTP service host
        /// </summary>
        protected bool isHttp;

        /// <summary>
        /// Initializes a new instance of the ServiceTaskDispatcherInfo class with ResoureLocation=OnPremise.
        /// </summary>
        /// <param name="jobId">indicating the job id</param>
        /// <param name="taskId">indicating the task id</param>
        /// <param name="capacity">indicating the capacity</param>
        /// <param name="machineName">indicating the machine name</param>
        /// <param name="firstCoreId">indicating the first core id</param>
        /// <param name="networkPrefix">indicating the network prefix</param>
        /// <param name="isHttp">indicating whether the service task is opening an HTTP service host</param>
        public ServiceTaskDispatcherInfo(string jobId, string taskId, int capacity, string machineName, int firstCoreId, string networkPrefix, bool isHttp)
            : this(jobId, taskId, capacity, machineName, null, firstCoreId, networkPrefix, Hpc.Scheduler.Session.Data.NodeLocation.OnPremise, isHttp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ServiceTaskDispatcherInfo class with ResoureLocation=OnPremise.
        /// </summary>
        /// <param name="jobId">indicating the job id</param>
        /// <param name="taskId">indicating the task id</param>
        /// <param name="capacity">indicating the capacity</param>
        /// <param name="machineName">indicating the machine name</param>
        /// <param name="machineVirtualName">indicating the machine virtual name for Azure cluster</param>
        /// <param name="firstCoreId">indicating the first core id</param>
        /// <param name="networkPrefix">indicating the network prefix</param>
        /// <param name="isHttp">indicating whether the service task is opening an HTTP service host</param>
        public ServiceTaskDispatcherInfo(string jobId, string taskId, int capacity, string machineName, string machineVirtualName, int firstCoreId, string networkPrefix, bool isHttp)
            : this(jobId, taskId, capacity, machineName, machineVirtualName, firstCoreId, networkPrefix, Hpc.Scheduler.Session.Data.NodeLocation.OnPremise, isHttp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ServiceTaskDispatcherInfo class
        /// </summary>
        /// <param name="jobId">indicating the job id</param>
        /// <param name="taskId">indicating the task id</param>
        /// <param name="capacity">indicating the capacity</param>
        /// <param name="machineName">indicating the machine name</param>
        /// <param name="machineVirtualName">indicating the machine virtual name for Azure cluster</param>
        /// <param name="firstCoreId">indicating the first core id</param>
        /// <param name="networkPrefix">indicating the network prefix</param>
        /// <param name="location">indicating target resource location, OnPremise or Azure</param>
        /// <param name="isHttp">indicating whether the service task is opening an HTTP service host</param>
        public ServiceTaskDispatcherInfo(string jobId, string taskId, int capacity, string machineName, string machineVirtualName, int firstCoreId, string networkPrefix, Hpc.Scheduler.Session.Data.NodeLocation location, bool isHttp)
            : base(taskId, capacity, machineName, machineVirtualName, location)
        {
            this.jobId = jobId;
            this.taskId = taskId;
            this.firstCoreId = firstCoreId;
            this.networkPrefix = networkPrefix;
            this.isHttp = isHttp;
        }

        /// <summary>
        /// Gets the job id
        /// </summary>
        public string JobId
        {
            get { return this.jobId; }
        }

        /// <summary>
        /// Gets the task id
        /// </summary>
        public string TaskId
        {
            get { return this.taskId; }
        }

        /// <summary>
        /// the first core id
        /// </summary>
        public int FirstCoreId
        {
            get { return this.firstCoreId; }
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

            // BUG FIX 2833 : Use localhost when communicating with local servicehost to get around security negotiation errors
            // For http backend, we must build the epr with real machine name
            if (!this.isHttp && IsThisBrokerNode(this.MachineName))
            {
                hostnameWithPrefix = "localhost";
            }
            else if (!String.IsNullOrEmpty(this.networkPrefix))
            {
                hostnameWithPrefix = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", this.networkPrefix, hostnameWithPrefix);
            }

            string epr = BindingHelper.GenerateServiceHostEndpointAddress(hostnameWithPrefix, this.jobId, this.taskId, port, this.isHttp) + Constant.ServiceHostEndpointPath;

            if (isController)
            {
                epr += Constant.ServiceHostControllerEndpointPath;
            }

            return new EndpointAddress(epr);
        }

        /// <summary>
        /// Returns whether specified machineName is this BN
        /// </summary>
        /// <param name="machineName">machine to check</param>
        /// <returns></returns>
        /// <remarks>This will not work if this is a failover BN but failover BN do not support BN == CN</remarks>
        protected static bool IsThisBrokerNode(string machineName)
        {
            return (0 == String.Compare(machineName, System.Net.Dns.GetHostName(), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
