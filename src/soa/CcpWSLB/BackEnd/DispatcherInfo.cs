//-----------------------------------------------------------------------
// <copyright file="DispatcherInfo.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Stores the info of a dispatcher</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Properties;

    /// <summary>
    /// Stores the info of a dispatcher
    /// </summary>
    internal abstract class DispatcherInfo
    {
        /// <summary>
        /// Stores the unique id
        /// </summary>
        private int uniqueId;

        /// <summary>
        /// Stores the capacity
        /// </summary>
        private int capacity;

        /// <summary>
        /// Stores the machine name
        /// </summary>
        private string machineName;

        /// <summary>
        /// Stores the machine virtual name for Azure cluster
        /// </summary>
        private string machineVirtualName;

        /// <summary>
        /// Allocated node location, OnPremise or Azure.
        /// </summary>
        private Scheduler.Session.Data.NodeLocation nodeLocation;

        /// <summary>
        /// Stores the time when the task is started.
        /// </summary>
        private DateTime taskStartTime;

        /// <summary>
        /// Stores the time when this dispatcher is blocked
        /// </summary>
        private DateTime blockTime;

        /// <summary>
        /// Stores how many times this task has been blocked consequtively
        /// </summary>
        private int blockRetryCount;

        /// <summary>
        /// Initializes a new instance of the DispatcherInfo class
        /// </summary>
        /// <param name="uniqueId">indicating the unique id of this dispatcher</param>
        /// <param name="capacity">indicating the capacity of this dispatcher</param>
        /// <param name="machineName">indicating the machine name</param>
        protected DispatcherInfo(int uniqueId, int capacity, string machineName, string machineVirtualName, Scheduler.Session.Data.NodeLocation location)
        {
            this.uniqueId = uniqueId;
            this.capacity = capacity;
            this.CoreCount = capacity;
            this.machineName = machineName;
            this.machineVirtualName = machineVirtualName;
            this.nodeLocation = location;
            this.taskStartTime = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the cores count for this dispatcher.
        /// </summary>
        public int CoreCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the unique id
        /// </summary>
        public int UniqueId
        {
            get { return this.uniqueId; }
        }

        /// <summary>
        /// Gets the capacity
        /// </summary>
        public int Capacity
        {
            get { return this.capacity; }
        }

        /// <summary>
        /// Gets the time when corresponding task is started
        /// </summary>
        public DateTime TaskStartTime
        {
            get { return this.taskStartTime; }
        }

        /// <summary>
        /// Gets or sets the time when this dispatcher is blocked
        /// </summary>
        public DateTime BlockTime
        {
            get { return this.blockTime; }
            set { this.blockTime = value; }
        }

        /// <summary>
        /// Gets the machine name
        /// </summary>
        public string MachineName
        {
            get { return this.machineName; }
        }

        /// <summary>
        /// Gets the machine virtual name
        /// </summary>
        public string MachineVirtualName
        {
            get { return this.machineVirtualName; }
        }

        /// <summary>
        /// Location of the allocated node, on-premise or Azure.
        /// </summary>
        public Microsoft.Hpc.Scheduler.Session.Data.NodeLocation AllocatedNodeLocation
        {
            get { return this.nodeLocation; }
            set { this.nodeLocation = value; }
        }

        /// <summary>
        /// Gets or sets how many times this dispatcher is blocked and retried consequtively
        /// </summary>
        public int BlockRetryCount
        {
            get { return this.blockRetryCount; }
            set { this.blockRetryCount = value; }
        }

        /// <summary>
        /// Gets the service host address indicating by this dispatcher info
        /// </summary>
        public EndpointAddress ServiceHostAddress
        {
            get { return this.GetEndpointAddress(false); }
        }

        /// <summary>
        /// Gets the service host controller address indicating by this dispatcher info
        /// </summary>
        public EndpointAddress ServiceHostControllerAddress
        {
            get { return this.GetEndpointAddress(true); }
        }

        /// <summary>
        /// Apply the default capacity
        /// </summary>
        /// <param name="defaultCapacity">indicating the default capacity</param>
        public void ApplyDefaultCapacity(int defaultCapacity)
        {
            if (defaultCapacity != 0)
            {
                this.capacity = defaultCapacity;
            }
        }

        /// <summary>
        /// Gets the enpoint address for a service host
        /// </summary>
        /// <param name="isController">indicating whether controller address is required</param>
        /// <returns>endpoint address</returns>
        protected abstract EndpointAddress GetEndpointAddress(bool isController);
    }
}
