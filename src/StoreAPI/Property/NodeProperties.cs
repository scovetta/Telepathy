using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the state of the node.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const Offline = 1
    /// const Draining = 2
    /// const Online = 4
    /// const All = 7</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.State" />
    [Serializable]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidNodeState)]
    [Flags]
    public enum NodeState
    {
        /// <summary>
        ///   <para>The node is offline. This enumeration member represents a value of 1.</para>
        /// </summary>
        Offline = 0x1,
        /// <summary>
        ///   <para>The scheduler is in the process of removing jobs from the node. This enumeration member represents a value of 2.</para>
        /// </summary>
        Draining = 0x2,
        /// <summary>
        ///   <para>The node is online and ready to run jobs. This enumeration member represents a value of 4.</para>
        /// </summary>
        Online = 0x4,
        /// <summary>
        ///   <para>All states are set. This enumeration member represents a value of 7.</para>
        /// </summary>
        All = Offline | Draining | Online,
    }

    /// <summary>
    ///   <para>Defines values that indicate whether a node is available to run 
    /// jobs, or than unavailable to run jobs because of user activity or the availability policy.</para>
    /// </summary>
    /// <remarks>
    ///   <para>The values in this enumeration are primarily used to indicate if workstation nodes are available to 
    /// run jobs, or unavailable because a user is active on the workstation or because of settings in the availability policy.</para>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const AlwaysOn = 1
    /// const Available = 2
    /// const Occupied = 4
    /// const All = 7</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Availability" />
    [Serializable]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidNodeAvailability)]
    [Flags]
    public enum NodeAvailability
    {
        /// <summary>
        ///   <para>Indicates that the availability of the node to run jobs is not affected by user activity or settings 
        /// in an availability policy, because an availability policy is not configured for the node. This enumeration member represents a value of 1.</para>
        /// </summary>
        AlwaysOn = 0x1,
        /// <summary>
        ///   <para>Indicates that node is currently available to run jobs, but could become unavailable because of 
        /// user activity or settings in the availability policy for the node. This enumeration member represents a value of 2.</para>
        /// </summary>
        Available = 0x2,
        /// <summary>
        ///   <para>Indicates that node is currently not available to run jobs, because of user activity on 
        /// the node or settings in the availability policy for the node. This enumeration member represents a value of 4.</para>
        /// </summary>
        Occupied = 0x4,
        /// <summary>
        ///   <para>Represents the bitwise-OR combination of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodeAvailability.AlwaysOn" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodeAvailability.Available" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodeAvailability.Occupied" /> values. This enumeration member represents a value of 7.</para>
        /// </summary>
        All = AlwaysOn | Available | Occupied,
    }

    /// <summary>
    ///   <para>Describes the location, such as on-premises or Windows Azure, and type of compute node in a cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>The values in this enumeration do not provide the same type of information that is provided by the Location property of 
    /// the HpcNode object that the <see href="http://go.microsoft.com/fwlink/?LinkId=182819">Get-HpcNode</see> HPC cmdlet returns, or that 
    /// is displayed in the Location column in Node Management in HPC Cluster Manager.</para> 
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code>const OnPremise = 1
    /// const Azure = 2
    /// const AzureVM = 3
    /// const AzureEmbedded = 4
    /// const AzureEmbeddedVM = 5
    /// const UnmanagedResource = 6
    /// const Linux = 7
    /// const AzureBatch = 8
    /// const NonDomainJoined = 9</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Location" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.NodePropertyIds.Location" />
    [Serializable]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidNodeLocation)]
    public enum NodeLocation
    {
        /// <summary>
        ///   <para>The node is not hosted in Windows Azure. This enumeration member represents a value of 1.</para>
        /// </summary>
        OnPremise = 0x1,
        /// <summary>
        ///   <para>The node is Windows Azure worker node. This enumeration member represents a value of 2.</para>
        /// </summary>
        Azure = 0x2,
        /// <summary>
        ///   <para>The node is Windows Azure virtual machine worker node. This enumeration member represents a value of 3.</para>
        /// </summary>
        AzureVM = 0x3,
        /// <summary>
        ///   <para>The node is Windows Azure node manager that is in the 
        /// same deployment with the Windows Azure scheduler. This enumeration member represents a value of 4.</para>
        /// </summary>
        AzureEmbedded = 0x4,
        /// <summary>
        ///   <para>The node is Windows Azure virtual machine node manager that is in 
        /// the same deployment with the Windows Azure scheduler. This enumeration member represents a value of 5.</para>
        /// </summary>
        AzureEmbeddedVM = 0x5,
        /// <summary>
        ///   <para>The node is unmanaged worker node. This enumeration member represents a value of 6.</para>
        /// </summary>
        UnmanagedResource = 0x6,
        /// <summary>
        ///   <para>The node is Linux worker node. This enumeration member represents a value of 7.</para>
        /// </summary>
        Linux = 0x7,
        /// <summary>
        ///   <para>The node is Azure Batch worker node. This enumeration member represents a value of 8.</para>
        /// </summary>
        AzureBatch = 0x8,
        /// <summary>
        ///   <para>The node is not domain joined worker node. This enumeration member represents a value of 9.</para>
        /// </summary>
        NonDomainJoined = 0x9,
    }


    /// <summary>
    ///   <para>Defines the identifiers that uniquely identify the properties of a node.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Use these identifiers when creating filters, specifying sort 
    /// orders, and using rowsets to retrieve specific properties from the database.</para>
    /// </remarks>
    /// <example />
    [Serializable]
    public class NodePropertyIds
    {
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.NodePropertyIds" /> class.</para>
        /// </summary>
        protected NodePropertyIds()
        {
        }

        /// <summary>
        ///   <para>The identifier that uniquely identifies the node in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Id" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para>The reason why the property returned null.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>You do not retrieve this property. For each property that you retrieve, the store returns two values. The first is the property value and the second is the error value (which indicates why the call did not return 
        /// the property value). If you use the property identifier to index the value, you receive the property value. If you use the zero-based index to retrieve the property, you receive either the property value, if it was returned, or the  
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyError" /> value.</para>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyError" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Error
        {
            get { return StorePropertyIds.Error; }
        }

        /// <summary>
        ///   <para>The computer name of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Name" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Name
        {
            get { return StorePropertyIds.Name; }
        }

        /// <summary>
        ///   <para>The types of jobs that can run on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.JobType" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId JobType
        {
            get { return StorePropertyIds.JobType; }
        }

        /// <summary>
        ///   <para>The number of times the node has been offline.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId OfflineResourceCount
        {
            get { return StorePropertyIds.OfflineResourceCount; }
        }

        /// <summary>
        ///   <para>The number of times that the node has been idle.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId IdleResourceCount
        {
            get { return StorePropertyIds.IdleResourceCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that have been scheduled on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId JobScheduledResourceCount
        {
            get { return StorePropertyIds.JobScheduledResourceCount; }
        }

        /// <summary>
        ///   <para>The number of times that the node has been ready to run a task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ReadyForTaskResourceCount
        {
            get { return StorePropertyIds.ReadyForTaskResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that are scheduled to run on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId TaskScheduledResourceCount
        {
            get { return StorePropertyIds.TaskScheduledResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been scheduled on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId JobTaskScheduledResourceCount
        {
            get { return StorePropertyIds.JobTaskScheduledResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been sent to the node to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId TaskDispatchedResourceCount
        {
            get { return StorePropertyIds.TaskDispatchedResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks running on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId TaskRunningResourceCount
        {
            get { return StorePropertyIds.TaskRunningResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been closed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <example />
        public static PropertyId CloseTaskResourceCount
        {
            get { return StorePropertyIds.CloseTaskResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that the server has requested be closed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CloseTaskDispatchedResourceCount
        {
            get { return StorePropertyIds.CloseTaskDispatchedResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been closed on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId TaskClosedResourceCount
        {
            get { return StorePropertyIds.TaskClosedResourceCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that have been closed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 32-bit integer.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CloseJobResourceCount
        {
            get { return StorePropertyIds.CloseJobResourceCount; }
        }

        /// <summary>
        ///   <para>The state of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.State" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId State
        {
            get { return _State; }
        }

        /// <summary>
        ///   <para>The previous state of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For possible values, see <see cref="Microsoft.Hpc.Scheduler.Properties.NodeState" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId PreviousState
        {
            get { return _PreviousState; }
        }

        /// <summary>
        ///   <para>Indicates whether the server thinks that the node is reachable.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Reachable" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Reachable
        {
            get { return _reachable; }
        }

        /// <summary>
        ///   <para>The number of cores on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.NumberOfCores" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId NumCores
        {
            get { return _numCores; }
        }

        /// <summary>
        ///   <para>The number of sockets on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.NumberOfSockets" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId NumSockets
        {
            get { return _numSockets; }
        }

        /// <summary>
        ///   <para>The number of GPUs on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId NumGpus
        {
            get { return _numGpus; }
        }

        /// <summary>
        ///   <para>The last time that the node let the HPC server know that it was online.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId LastPingTime
        {
            get { return _lastPingTime; }
        }

        /// <summary>
        ///   <para>The date and time that the node last went offline.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OfflineTime" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId OfflineTime
        {
            get { return _offlineTime; }
        }

        /// <summary>
        ///   <para>The date and time that the node last came online.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OnlineTime" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId OnlineTime
        {
            get { return _onlineTime; }
        }

        /// <summary>
        ///   <para>The security identifier in Active Directory for the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Sid
        {
            get { return _sid; }
        }

        /// <summary>
        ///   <para>Indicates that a user requested that the node go offline.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.MoveToOffline" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MoveToOffline
        {
            get { return _moveToOffline; }
        }

        /// <summary>
        ///   <para>The identifier that uniquely identifies the node in the system.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Guid" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Guid
        {
            get { return _NodeID; }
        }

        /// <summary>
        ///   <para>The size of memory on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.MemorySize" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MemorySize
        {
            get { return _MemorySize; }
        }

        /// <summary>
        ///   <para>The CPU speed of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.CpuSpeed" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CpuSpeed
        {
            get { return _CpuSpeed; }
        }

        /// <summary>
        ///   <para>A comma-delimited list of the node groups to which the node belongs.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.NodeGroups" />.</para>
        /// </remarks>
        public static PropertyId NodeGroups
        {
            get { return _Tags; }
        }

        /// <summary>
        ///   <para>The internal node object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId NodeObject
        {
            get { return StorePropertyIds.NodeObject; }
        }

        /// <summary>
        ///   <para>Indicates whether the node was forced to go offline.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is a 
        /// 
        /// <see cref="System.Boolean" />. True indicates that the node was forced to go offline. False indicates that the node was not forced to go offline.</para> 
        /// </remarks>
        public static PropertyId IsForcedOffline
        {
            get { return _IsForcedOffline; }
        }

        /// <summary>
        ///   <para>Whether the node is available to run jobs, rather than unavailable because of user activity or the availability policy.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a value from the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodeAvailability" /> enumeration. For more information, see 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Availability" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.NodeAvailability" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Availability" />
        public static PropertyId Availability
        {
            get { return _Availability; }
        }

        // 
        // For Azure
        //

        /// <summary>
        ///   <para>Whether the node is hosted in Windows Azure or is located on premise.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a value from the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodeLocation" /> enumeration. For more information, see 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Location" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.NodeLocation" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Location" />
        public static PropertyId Location
        {
            get { return _Location; }
        }

        /// <summary>
        ///   <para>The address of the proxies created for each pair of Windows 
        /// Azure subscription and Windows Azure service, if the node is hosted in Windows Azure.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a <see cref="System.String" />. If the node is an on-premise node, this property is NULL.</para>
        /// </remarks>
        public static PropertyId AzureProxyAddress
        {
            get { return _AzureProxyAddress; }
        }

        /// <summary>
        ///   <para>The public, globally unique name of the hosted service that is configured in 
        /// the subscription that you used to deploy the node, if the node is hosted in Windows Azure.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 
        /// <see cref="System.String" />. If the node is an on-premise node, this property is 
        /// NULL. For more information see 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.AzureServiceName" />.</para>
        ///   <para>This value can differ from the current value in the node template, if the node template was edited after the node was deployed.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.AzureServiceName" />
        public static PropertyId AzureServiceName
        {
            get { return _AzureServiceName; }
        }

        /// <summary>
        ///   <para>The globally unique identifier (GUID) of the Windows Azure subscription used 
        /// for the current deployment of the node, if the node is hosted in Windows Azure.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 
        /// 
        /// <see cref="System.String" />. It has a format of XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX, where X is a hexadecimal digit. If the node is an on-premise node, this property is  
        /// NULL.</para>
        ///   <para>This value can differ from the current value in the node template, if the node template was edited after the node was deployed. </para>
        /// </remarks>
        public static PropertyId AzureSubscriptionId
        {
            get { return _AzureSubscriptionId; }
        }

        /// <summary>
        ///   <para>The string for connecting to Windows Azure storage services for 
        /// the current deployment of the node, if the node is hosted in Windows Azure.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 
        /// <see cref="System.String" />. If the node is an on-premise node, this property is 
        /// NULL. It has 
        /// the following format: BlobEndpoint=https://storage_account_name.blob.core.windows.net;QueueEndpoint=http://storage_account_name.queue.core.windows.net;TableEndpoint=https://storage_account_name.table.core.windows.net;AccountName=storage_account_name;AccountKey=account_key.</para> 
        /// </remarks>
        public static PropertyId AzureStorageConnectionString
        {
            get { return _AzureStorageConnectionString; }
        }

        /// <summary>
        ///   <para>The globally unique identifier (GUID) of the Windows Azure service deployment 
        /// with which the node is associated, if the node is hosted in Windows Azure.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 
        /// 
        /// <see cref="System.String" />. It has a format of XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX, where X is a hexadecimal digit. If the node is an on-premise node, this property is  
        /// NULL.</para>
        /// </remarks>
        public static PropertyId AzureDeploymentId
        {
            get { return _AzureDeploymentId; }
        }

        /// <summary>
        ///   <para>The number of proxies created for each pair of Windows 
        /// Azure subscription and Windows Azure service, if the node is hosted in Windows Azure.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is an 
        /// 
        /// <see cref="System.Int32" />. The possible values are from 1 through 50. The default value is 2. If the node is an on-premise node, this property is  
        /// NULL.</para>
        ///   <para>You cannot change this value in HPC Cluster Manager, but you can change 
        /// it by exporting the node template, editing the node template, and importing the updated node template.</para>
        /// </remarks>
        public static PropertyId AzureProxyMultiplicity
        {
            get { return _AzureProxyMultiplicity; }
        }

        /// <summary>
        ///   <para>The address of the node, if it is hosted in Windows Azure.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a <see cref="System.String" />. If the node is an on-premise node, this property is NULL.</para>
        /// </remarks>
        public static PropertyId AzureNodeAddress
        {
            get { return _AzureNodeAddress; }
        }

        /// <summary>
        ///   <para>The error message of the Azure node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId AzureNodeErrors
        {
            get { return _AzureNodeErrors; }
        }

        // Multi-domain for V3SP2

        /// <summary>
        ///   <para>A string that contains the DNS suffix associated with this node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId DnsSuffix
        {
            get { return _DnsSuffix; }
        }

        // Over and under subscription for V2SP2

        /// <summary>
        ///   <para>A nullable Boolean value that indicates whether or not the node 
        /// manager should assign affinity to tasks to cores when running task on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Affinity
        {
            get { return _Affinity; }
        }

        //Add Instance name to Azure nodes
        /// <summary>
        ///   <para>The instance name of the Azure node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a <see cref="System.String" /> which contains the Windows Azure node instance name.</para>
        /// </remarks>
        public static PropertyId AzureNodeInstanceName
        {
            get { return _AzureNodeInstanceName; }
        }

        //Role name for Azure Nodes
        /// <summary>
        ///   <para>The role name for the Azure node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId AzureNodeRoleName
        {
            get { return _AzureNodeRoleName; }
        }

        /// <summary>
        ///   <para>The Linux distribution name of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId OSDistrib
        {
            get { return _OSDistrib; }
        }

        /// <summary>
        ///   <para>The network infomation of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId NetworksInfo
        {
            get { return _NetworksInfo; }
        }

        /// <summary>
        ///   <para>The infomation of GPU on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId GpuInfo
        {
            get { return _GpuInfo; }
        }

        /// <summary>
        ///   <para>The address of Azure load balancer.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId AzureLoadBalancerAddress
        {
            get { return _AzureLoadBalancerAddress; }
        }

        /// <summary>
        ///   <para>The json format string of Azure Meta Data.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId AzureMetaData
        {
            get { return _AzureMetaData; }
        }

        // 
        // Private members
        //

        static PropertyId _State = new PropertyId(StorePropertyType.NodeState, "State", PropertyIdConstants.NodePropertyIdStart + 1);
        static PropertyId _PreviousState = new PropertyId(StorePropertyType.NodeState, "PreviousState", PropertyIdConstants.NodePropertyIdStart + 2);
        static PropertyId _reachable = new PropertyId(StorePropertyType.Boolean, "Reachable", PropertyIdConstants.NodePropertyIdStart + 4);
        static PropertyId _numCores = new PropertyId(StorePropertyType.Int32, "NumCores", PropertyIdConstants.NodePropertyIdStart + 5);
        static PropertyId _numSockets = new PropertyId(StorePropertyType.Int32, "NumSockets", PropertyIdConstants.NodePropertyIdStart + 6);
        static PropertyId _lastPingTime = new PropertyId(StorePropertyType.DateTime, "LastPingTime", PropertyIdConstants.NodePropertyIdStart + 7);
        static PropertyId _offlineTime = new PropertyId(StorePropertyType.DateTime, "OfflineTime", PropertyIdConstants.NodePropertyIdStart + 8);
        static PropertyId _onlineTime = new PropertyId(StorePropertyType.DateTime, "OnlineTime", PropertyIdConstants.NodePropertyIdStart + 9);
        static PropertyId _sid = new PropertyId(StorePropertyType.String, "SID", PropertyIdConstants.NodePropertyIdStart + 11);
        static PropertyId _moveToOffline = new PropertyId(StorePropertyType.Boolean, "MoveToOffline", PropertyIdConstants.NodePropertyIdStart + 12);
        static PropertyId _NodeID = new PropertyId(StorePropertyType.Guid, "Guid", PropertyIdConstants.NodePropertyIdStart + 15);
        static PropertyId _MemorySize = new PropertyId(StorePropertyType.Int64, "MemorySize", PropertyIdConstants.NodePropertyIdStart + 16);
        static PropertyId _CpuSpeed = new PropertyId(StorePropertyType.Int32, "CpuSpeed", PropertyIdConstants.NodePropertyIdStart + 17);
        static PropertyId _Tags = new PropertyId(StorePropertyType.String, "NodeGroups", PropertyIdConstants.NodePropertyIdStart + 18);
        static PropertyId _IsForcedOffline = new PropertyId(StorePropertyType.Boolean, "IsForcedOffline", PropertyIdConstants.NodePropertyIdStart + 19);
        static PropertyId _Availability = new PropertyId(StorePropertyType.NodeAvailability, "Availability", PropertyIdConstants.NodePropertyIdStart + 20);


        // For Azure

        static PropertyId _Location = new PropertyId(StorePropertyType.NodeLocation, "Location", PropertyIdConstants.NodePropertyIdStart + 21);
        static PropertyId _AzureProxyAddress = new PropertyId(StorePropertyType.String, "AzureProxyAddress", PropertyIdConstants.NodePropertyIdStart + 22);
        static PropertyId _AzureServiceName = new PropertyId(StorePropertyType.String, "AzureServiceName", PropertyIdConstants.NodePropertyIdStart + 23);
        static PropertyId _AzureSubscriptionId = new PropertyId(StorePropertyType.String, "AzureSubscriptionId", PropertyIdConstants.NodePropertyIdStart + 24);
        static PropertyId _AzureStorageConnectionString = new PropertyId(StorePropertyType.String, "AzureStorageConnectionString", PropertyIdConstants.NodePropertyIdStart + 25);
        static PropertyId _AzureDeploymentId = new PropertyId(StorePropertyType.String, "AzureDeploymentId", PropertyIdConstants.NodePropertyIdStart + 26);
        static PropertyId _AzureProxyMultiplicity = new PropertyId(StorePropertyType.Int32, "AzureProxyMultiplicity", PropertyIdConstants.NodePropertyIdStart + 27);
        static PropertyId _AzureNodeAddress = new PropertyId(StorePropertyType.String, "AzureNodeAddress", PropertyIdConstants.NodePropertyIdStart + 28);

        // Multi-domain for V3SP2
        static PropertyId _DnsSuffix = new PropertyId(StorePropertyType.String, "DnsSuffix", PropertyIdConstants.NodePropertyIdStart + 29);

        // Over and under subscription for V3SP2
        static PropertyId _Affinity = new PropertyId(StorePropertyType.Boolean, "Affinity", PropertyIdConstants.NodePropertyIdStart + 30);

        //Extension to Azure 
        static PropertyId _AzureNodeInstanceName = new PropertyId(StorePropertyType.String, "AzureNodeInstanceName", PropertyIdConstants.NodePropertyIdStart + 31);
        static PropertyId _AzureNodeRoleName = new PropertyId(StorePropertyType.String, "AzureNodeRoleName", PropertyIdConstants.NodePropertyIdStart + 32);

        static PropertyId _OSDistrib = new PropertyId(StorePropertyType.String, "OSDistrib", PropertyIdConstants.NodePropertyIdStart + 33);
        static PropertyId _NetworksInfo = new PropertyId(StorePropertyType.String, "NetworksInfo", PropertyIdConstants.NodePropertyIdStart + 34);

        static PropertyId _numGpus = new PropertyId(StorePropertyType.Int32, "NumGpus", PropertyIdConstants.NodePropertyIdStart + 35);
        static PropertyId _AzureNodeErrors = new PropertyId(StorePropertyType.String, "AzureNodeErrors", PropertyIdConstants.NodePropertyIdStart + 36);
        static PropertyId _AzureLoadBalancerAddress = new PropertyId(StorePropertyType.String, "AzureLoadBalancerAddress", PropertyIdConstants.NodePropertyIdStart + 37);
        static PropertyId _GpuInfo = new PropertyId(StorePropertyType.String, "GpuInfo", PropertyIdConstants.NodePropertyIdStart + 38);
        static PropertyId _AzureMetaData = new PropertyId(StorePropertyType.String, "AzureMetaData", PropertyIdConstants.NodePropertyIdStart + 39);
    }
}
