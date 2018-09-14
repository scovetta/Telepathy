using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{

    /// <summary>
    ///   <para>Defines when the node history event is raised.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get node history, call the 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNodeHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> method.</para> 
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const None = 0
    /// const Added = 1
    /// const Removed = 2
    /// const Online = 3
    /// const Offline = 4
    /// const Reachable = 5
    /// const Unreachable = 6
    /// const Draining = 7
    /// const Available = 8
    /// const Occupied = 9</code>
    /// </remarks>
    [Serializable]
    public enum NodeEvent
    {
        /// <summary>
        ///   <para>Reserved. This enumeration member represents a value of 0.</para>
        /// </summary>
        None = 0,
        /// <summary>
        ///   <para>When a user adds the node to the cluster. This enumeration member represents a value of 1.</para>
        /// </summary>
        Added = 1,
        /// <summary>
        ///   <para>When a user removes the node from the cluster. This enumeration member represents a value of 2.</para>
        /// </summary>
        Removed = 2,
        /// <summary>
        ///   <para>When the node goes online. This enumeration member represents a value of 3.</para>
        /// </summary>
        Online = 3,
        /// <summary>
        ///   <para>When the node goes offline. This enumeration member represents a value of 4.</para>
        /// </summary>
        Offline = 4,
        /// <summary>
        ///   <para>When the node becomes reachable. This enumeration member represents a value of 5.</para>
        /// </summary>
        Reachable = 5,
        /// <summary>
        ///   <para>When the node is becomes unreachable. This enumeration member represents a value of 6.</para>
        /// </summary>
        UnReachable = 6,
        /// <summary>
        ///   <para>When the scheduler is in the process of removing jobs from the node. This enumeration member represents a value of 7.</para>
        /// </summary>
        Draining = 7,
        /// <summary>
        ///   <para>When a node becomes available to run jobs, because of the availability policy settings or the end of 
        /// user activity. This enumeration member represents a value of 8. 
        /// This value is only supported for Windows HPC Server 2008 R2 with Service Pack 1 (SP1).</para> 
        /// </summary>
        Available = 8,
        /// <summary>
        ///   <para>When a node becomes unavailable to run jobs, because of the availability policy settings or user activity. 
        /// This enumeration member represents a value of 9. This value is only supported for Windows HPC Server 2008 R2 with Service Pack 1 (SP1).</para>
        /// </summary>
        Occupied = 9,
    }

    /// <summary>
    ///   <para>Defines the node history properties that you can retrieve when calling the 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNodeHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> method.</para> 
    /// </summary>
    [Serializable]
    public class NodeHistoryPropertyIds
    {

        /// <summary>
        ///   <para>An identifier that uniquely identifies the event in the store.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.NodeHistoryPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the node in the system.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Guid" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.NodeHistoryPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId NodeGuid
        {
            get { return _NodeGuid; }
        }

        /// <summary>
        ///   <para>The name of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Name" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.NodeHistoryPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId NodeName
        {
            get { return _NodeName; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the node in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Id" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.NodeHistoryPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId NodeId
        {
            get { return _NodeId; }
        }

        /// <summary>
        ///   <para>The node history event.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.NodeEvent" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.NodeHistoryPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId NodeEvent
        {
            get { return _NodeEvent; }
        }

        /// <summary>
        ///   <para>The date and time that the event occurred.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.DateTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.NodeHistoryPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId EventTime
        {
            get { return _EventTime; }
        }

        /// <summary>
        ///   <para>The number of cores on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.NumberOfCores" />.</para>
        /// </remarks>
        static public PropertyId NumCores
        {
            get { return _NumCores; }
        }

        /// <summary>
        ///   <para>The number of sockets on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.NumberOfSockets" />.</para>
        /// </remarks>
        static public PropertyId NumSockets
        {
            get { return _NumSockets; }
        }

        static PropertyId _NodeGuid = new PropertyId(StorePropertyType.Guid, "NodeGuid", PropertyIdConstants.NodeHistoryPropertyIdStart + 1);
        static PropertyId _NodeId = new PropertyId(StorePropertyType.Int32, "NodeId", PropertyIdConstants.NodeHistoryPropertyIdStart + 2);
        static PropertyId _NodeName = new PropertyId(StorePropertyType.String, "NodeName", PropertyIdConstants.NodeHistoryPropertyIdStart + 3);
        static PropertyId _NodeEvent = new PropertyId(StorePropertyType.NodeEvent, "NodeEvent", PropertyIdConstants.NodeHistoryPropertyIdStart + 4);
        static PropertyId _EventTime = new PropertyId(StorePropertyType.DateTime, "EventTime", PropertyIdConstants.NodeHistoryPropertyIdStart + 5);
        static PropertyId _NumCores = new PropertyId(StorePropertyType.Int32, "NumCores", PropertyIdConstants.NodeHistoryPropertyIdStart + 6);
        static PropertyId _NumSockets = new PropertyId(StorePropertyType.Int32, "NumSockets", PropertyIdConstants.NodeHistoryPropertyIdStart + 7);
    }
}
