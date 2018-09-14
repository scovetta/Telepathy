namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Runtime.InteropServices;
    using Microsoft.Hpc.Scheduler.Store;
    using Microsoft.Hpc.Scheduler.Properties;


    /// <summary>
    ///   <para>Contains information about a compute node.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call one of the following methods:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNode(System.Int32)" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNodeByName(System.String)" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidINodeV4SP3)]
    public interface ISchedulerNode
    {
        #region V2 node methods Don't change

        /// <summary>
        ///   <para>Refreshes this copy of the node object with the contents from the server.</para>
        /// </summary>
        void Refresh();

        /// <summary>
        ///   <para>Retrieves the core state information for each core on the node.</para>
        /// </summary>
        /// <returns>
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCollection" /> interface that contains a collection of 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCore" /> interfaces.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The core information is a snapshot of the core at the time the call was made.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853435(v=vs.85).aspx">Getting a List of Nodes in the Cluster</see>.</para> 
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.NumberOfCores" />
        ISchedulerCollection GetCores();

        /// <summary>
        ///   <para>Retrieves the counter data for the node.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerNodeCounters" /> interface that contains the counter data.</para>
        /// </returns>
        ISchedulerNodeCounters GetCounters();

        #endregion

        #region V2 Node properties Don't Change

        /// <summary>
        ///   <para>Retrieves the identifier that uniquely identifies the node in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The sequential identifier given to the node when it was added to the cluster.</para>
        /// </value>
        System.Int32 Id { get; }

        /// <summary>
        ///   <para>Retrieves the computer name of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the node.</para>
        /// </value>
        System.String Name { get; }

        /// <summary>
        ///   <para>Retrieves the types of jobs that this node is configured to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The types of jobs that this node can run. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobType" /> enumeration.</para>
        /// </value>
        /// <remarks>
        ///   <para>A node can run more than one type of job. Treat the property value as an integer and the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobType" /> enumeration values as flags.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853436(v=vs.85).aspx">Getting a List of Nodes in the Cluster</see>.</para> 
        /// </example>
        Microsoft.Hpc.Scheduler.Properties.JobType JobType { get; }

        /// <summary>
        ///   <para>Retrieves the current state of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the node. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.NodeState" /> enumeration.</para>
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.NodeState State { get; }

        /// <summary>
        ///   <para>Determines whether the server thinks the node is reachable.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the node is reachable; otherwise, False.</para>
        /// </value>
        System.Boolean Reachable { get; }

        /// <summary>
        ///   <para>Retrieves the number of cores on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores.</para>
        /// </value>
        System.Int32 NumberOfCores { get; }

        /// <summary>
        ///   <para>Retrieves the number of sockets on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of sockets.</para>
        /// </value>
        System.Int32 NumberOfSockets { get; }

        /// <summary>
        ///   <para>Retrieves the date and time that the node last went offline.</para>
        /// </summary>
        /// <value>
        ///   <para>The last time that the node went offline. The value is in Coordinated Universal Time.</para>
        /// </value>
        System.DateTime OfflineTime { get; }

        /// <summary>
        ///   <para>Retrieves the date and time that the node last came online.</para>
        /// </summary>
        /// <value>
        ///   <para>The last time that the node came online. The value is in Coordinated Universal Time.</para>
        /// </value>
        System.DateTime OnlineTime { get; }

        /// <summary>
        ///   <para>Determines whether a user requested that the node go offline.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the user has requested that the node go offline; otherwise False.</para>
        /// </value>
        System.Boolean MoveToOffline { get; }

        /// <summary>
        ///   <para>Retrieves the globally unique identifier that uniquely identifies the node in the system.</para>
        /// </summary>
        /// <value>
        ///   <para>The node's system identifier.</para>
        /// </value>
        System.Guid Guid { get; }

        /// <summary>
        ///   <para>Retrieves the size of memory.</para>
        /// </summary>
        /// <value>
        ///   <para>The size of memory, in bytes.</para>
        /// </value>
        System.Int64 MemorySize { get; }

        /// <summary>
        ///   <para>Retrieves the processor speed of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The processor speed, in MHz.</para>
        /// </value>
        System.Int32 CpuSpeed { get; }

        /// <summary>
        ///   <para>Retrieves the node groups to which this node belongs.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of node group names.</para>
        /// </value>
        IStringCollection NodeGroups { get; }

        #endregion

        #region v3 methods Don't change

        /// <summary>
        ///   <para>Raised when the state of a node changes.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For information about the delegate that you implement to handle this event, see 
        /// <see cref="Microsoft.Hpc.Scheduler.NodeStateHandler" />. </para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.NodeStateHandler" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.INodeStateEventArg" />
        event EventHandler<NodeStateEventArg> OnNodeState;

        #endregion

        #region V3 SP1 Properties Don't Change

        /// <summary>
        ///   <para>Gets whether the node is available to run jobs, rather than unavailable because of user activity or the availability policy.</para>
        /// </summary>
        /// <value>
        ///   <para>A value from the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodeAvailability" /> enumeration that indicates whether the node is available to run jobs, rather than unavailable because of user activity or the availability policy.</para> 
        /// </value>
        /// <remarks>
        ///   <para>This property is primarily used to indicate if workstation nodes are available to run jobs, 
        /// or unavailable because a user is active on the workstation or because of settings in the availability policy. </para>
        ///   <para>If an availability policy is not configured for the node, the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Availability" /> property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodeAvailability.AlwaysOn" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.NodeAvailability" />
        NodeAvailability Availability { get; }

        /// <summary>
        ///   <para>Gets whether the node is hosted in Windows Azure or is located on premise.</para>
        /// </summary>
        /// <value>
        ///   <para>A value from the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodeLocation" /> enumeration that indicates whether the node is hosted in Windows Azure or is located on premise.</para> 
        /// </value>
        /// <remarks>
        ///   <para>This property does not provide the same type of information that is provided by the Location property of the HpcNode 
        /// object that the <see href="http://go.microsoft.com/fwlink/?LinkId=182819">Get-HpcNode</see> HPC cmdlet returns, or that is 
        /// displayed in the Location column in Node Management in HPC Cluster Manager.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.AzureServiceName" />
        NodeLocation Location { get; }

        /// <summary>
        ///   <para>Gets the public, globally unique name of the hosted service that is configured in 
        /// the subscription that you used to deploy the node, if the node is hosted in Windows Azure.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.String" /> that indicates the public, globally unique name of the hosted service that is configured in the subscription that you used to deploy the node, if the node is hosted in Windows Azure. If the node not hosted in Windows Azure, the property is  
        /// null.</para>
        /// </value>
        /// <remarks>
        ///   <para>The service name is the value of ServiceName that is configured in the public URL of the 
        /// service (http:// ServiceName.cloudapp.net). For example, if the public URL of 
        /// the service is http://contoso_service.cloudapp.net, then the service name is contoso_service. </para> 
        ///   <para>A cluster administrator specifies the service name for the 
        /// node when the administrator creates the node template for deploying the node.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Location" />
        string AzureServiceName { get; }

        /// <summary>
        ///   <para>Raised when the HPC Node Manager Service on a node becomes reachable or unreachable.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For information about the delegate that you implement to handle this event, see 
        /// <see cref="Microsoft.Hpc.Scheduler.NodeReachableHandler" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.NodeReachableHandler" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.INodeReachableEventArg" />
        event EventHandler<NodeReachableEventArg> OnNodeReachable;

        #endregion

        #region V3 SP2 Properties

        /// <summary>
        ///   <para>Retrieves the DNS suffix associated with this node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> object that contains the DNS suffix.</para>
        /// </value>
        string DnsSuffix { get; }

        #endregion

        #region V4 SP3 Properties

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        string AzureLoadBalancerAddress { get; }

        #endregion
    }

}
