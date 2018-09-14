using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Used to track resource allocations for a task.</para>
    /// </summary>
    /// <remarks>
    ///   <para>If the task identifier is zero, the allocation is for the job.</para>
    /// </remarks>
    public class AllocationProperties
    {
        private AllocationProperties()
        {
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the allocation in the system.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para>The UTC date and time for when the allocation started.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId StartTime
        {
            get { return StorePropertyIds.StartTime; }
        }

        /// <summary>
        ///   <para>The UTC date and time when the allocation ended.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId EndTime
        {
            get { return StorePropertyIds.EndTime; }
        }

        /// <summary>
        ///   <para>The last time that an allocation property was changed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ChangeTime
        {
            get { return StorePropertyIds.ChangeTime; }
        }

        /// <summary>
        ///   <para>The total amount of time that the task spent in user mode.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TotalUserTime
        {
            get { return StorePropertyIds.TotalUserTime; }
        }

        /// <summary>
        ///   <para>The total amount of time that the task spent in kernel mode.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TotalKernelTime
        {
            get { return StorePropertyIds.TotalKernelTime; }
        }

        /// <summary>
        ///   <para>Identifies the job associated with the allocation.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId JobId
        {
            get { return _JobId; }
        }

        /// <summary>
        ///   <para>Identifies the task associated with the allocation.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TaskId
        {
            get { return _TaskId; }
        }

        /// <summary>
        ///   <para>Identifies the resource that is allocated.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ResourceId
        {
            get { return _ResourceId; }
        }

        /// <summary>
        ///   <para>The amount of memory, in kilobytes, used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId MemoryUsed
        {
            get { return _MemoryUsed; }
        }

        /// <summary>
        ///   <para>The number of processes that are running for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId NumProcesses
        {
            get { return _NumProcesses; }
        }

        /// <summary>
        ///   <para>The processes that are running for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId PIDS
        {
            get { return _PIDS; }
        }

        /// <summary>
        ///   <para>The number of times that the job was queued again.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId JobRequeueCount
        {
            get { return _JobRequeueCount; }
        }

        /// <summary>
        ///   <para>The number of times the task has been queued again.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TaskRequeueCount
        {
            get { return _TaskRequeueCount; }
        }

        /// <summary>
        ///   <para>The name of the node associated with the allocation.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId NodeName
        {
            get { return _NodeName; }
        }

        /// <summary>
        ///   <para>The identifier of the core associated with the allocation.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CoreId
        {
            get { return _CoreId; }
        }

        /// <summary>
        ///   <para>Identifies the node associated with the allocation.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId NodeId
        {
            get { return _NodeId; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the task within the job that is associated with the allocation. </para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TaskJobId
        {
            get { return _TaskNiceId; }
        }

        /// <summary>
        ///   <para>The instance identifier of the parametric task that is associated with the allocation.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TaskInstanceId
        {
            get { return _TaskInstanceId; }
        }

        /// <summary>
        ///   <para>The internal allocation object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId AllocationObject
        {
            get { return StorePropertyIds.AllocationObject; }
        }

        static private PropertyId _JobId = new PropertyId(StorePropertyType.Int32, "JobId", PropertyIdConstants.AllocationPropertyIdStart + 1);
        static private PropertyId _TaskId = new PropertyId(StorePropertyType.Int32, "TaskId", PropertyIdConstants.AllocationPropertyIdStart + 2);
        static private PropertyId _ResourceId = new PropertyId(StorePropertyType.Int32, "ResourceId", PropertyIdConstants.AllocationPropertyIdStart + 3);
        static private PropertyId _MemoryUsed = new PropertyId(StorePropertyType.Int64, "MemoryUsed", PropertyIdConstants.AllocationPropertyIdStart + 5);
        static private PropertyId _NumProcesses = new PropertyId(StorePropertyType.String, "NumProcesses", PropertyIdConstants.AllocationPropertyIdStart + 6);
        static private PropertyId _PIDS = new PropertyId(StorePropertyType.String, "PIDS", PropertyIdConstants.AllocationPropertyIdStart + 7);
        static private PropertyId _JobRequeueCount = new PropertyId(StorePropertyType.String, "JobRequeueCount", PropertyIdConstants.AllocationPropertyIdStart + 8);
        static private PropertyId _TaskRequeueCount = new PropertyId(StorePropertyType.String, "TaskRequeuecount", PropertyIdConstants.AllocationPropertyIdStart + 9);

        static private PropertyId _NodeName = new PropertyId(StorePropertyType.String, "NodeName", PropertyIdConstants.AllocationPropertyIdStart + 10);
        static private PropertyId _CoreId = new PropertyId(StorePropertyType.Int32, "CoreId", PropertyIdConstants.AllocationPropertyIdStart + 11);
        static private PropertyId _NodeId = new PropertyId(StorePropertyType.Int32, "NodeId", PropertyIdConstants.AllocationPropertyIdStart + 12);
        static private PropertyId _TaskNiceId = new PropertyId(StorePropertyType.Int32, "TaskJobId", PropertyIdConstants.AllocationPropertyIdStart + 13);
        static private PropertyId _TaskInstanceId = new PropertyId(StorePropertyType.Int32, "TaskInstanceId", PropertyIdConstants.AllocationPropertyIdStart + 14);
    }
}
