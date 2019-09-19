// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Runtime.Serialization;

    using Microsoft.Hpc.Scheduler.Session.Data;

    /// <summary>
    ///   <para />
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com")]
    public sealed class TaskInfo
    {
        /// <summary>
        /// Stores the task id
        /// </summary>
        private string id;

        /// <summary>
        /// Stores the number of cores that the task occupied
        /// </summary>
        private int capacity;

        /// <summary>
        /// Stores the machine name
        /// It is the real address of the node on Azure.
        /// </summary>
        private string machineName;

        /// <summary>
        /// Stores the machine virtual name for Azure cluster
        /// There is a node mapping between virtual name and its real address on Azure.
        /// </summary>
        private string machineVirtualName;

        /// <summary>
        /// Stores the machine location
        /// </summary>
        private NodeLocation location = NodeLocation.OnPremise;

        /// <summary>
        /// Stores the service name that used to deploy broker proxy service.
        /// Note: for on-premise node, this field is null.
        /// </summary>
        private string proxyServiceName;

        /// <summary>
        /// Stores the task state
        /// </summary>
        private TaskState state;

        /// <summary>
        /// the core index of the first core allocated to this task
        /// </summary>
        private int firstCoreIndex;

        /// <summary>
        /// the job requeue count
        /// </summary>
        private int jobRequeueCount;

        /// <summary>
        /// Gets or sets the Task ID
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets the capacity
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public int Capacity
        {
            get { return this.capacity; }
            set { this.capacity = value; }
        }

        /// <summary>
        /// Gets or sets the machine name
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string MachineName
        {
            get { return this.machineName; }
            set { this.machineName = value; }
        }

        /// <summary>
        /// Gets or sets the machine virtual name
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember(IsRequired = false)]
        public string MachineVirtualName
        {
            get { return this.machineVirtualName; }
            set { this.machineVirtualName = value; }
        }

        /// <summary>
        /// Gets or sets the node location
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public NodeLocation Location
        {
            get { return this.location; }
            set { this.location = value; }
        }

        /// <summary>
        /// Gets or sets the proxy service name
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string ProxyServiceName
        {
            get { return this.proxyServiceName; }
            set { this.proxyServiceName = value; }
        }

        /// <summary>
        /// Gets or sets the task state
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public TaskState State
        {
            get { return this.state; }
            set { this.state = value; }
        }

        /// <summary>
        /// Gets or set the first core index
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public int FirstCoreIndex
        {
            get { return this.firstCoreIndex; }
            set { this.firstCoreIndex = value; }
        }

        /// <summary>
        /// Gets or set the job re-queue count
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public int JobRequeueCount
        {
            get { return this.jobRequeueCount; }
            set { this.jobRequeueCount = value; }
        }

        /// <summary>
        /// Gets or set the Azure load balancer address
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string AzureLoadBalancerAddress { get; set; }
    }
}
