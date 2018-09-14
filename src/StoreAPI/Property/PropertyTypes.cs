using System;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the possible data types for a property.</para>
    /// </summary>
    [ComVisible(true)]
    public enum StorePropertyType
    {
        /// <summary>
        ///   <para>The property does not contain a type.</para>
        /// </summary>
        None = 0x00000000,

        /// <summary>
        ///   <para>The property contains a 4-byte signed integer.</para>
        /// </summary>
        Int32 = 0x00010000,
        /// <summary>
        ///   <para>The property contains a 4-byte unsigned integer.</para>
        /// </summary>
        UInt32 = 0x00020000,
        /// <summary>
        ///   <para>The property contains an 8-byte signed integer.</para>
        /// </summary>
        Int64 = 0x00030000,
        /// <summary>
        ///   <para>The property contains a string.</para>
        /// </summary>
        String = 0x00040000,
        /// <summary>
        ///   <para>The property contains a date and time value.</para>
        /// </summary>
        DateTime = 0x00050000,
        /// <summary>
        ///   <para>The property contains a Boolean value (true or false).</para>
        /// </summary>
        Boolean = 0x00060000,
        /// <summary>
        ///   <para>The property contains a globally unique identifier.</para>
        /// </summary>
        Guid = 0x00070000,
        /// <summary>
        ///   <para>The property contains binary data.</para>
        /// </summary>
        Binary = 0x00080000,
        /// <summary>
        ///   <para>The property contains an object.</para>
        /// </summary>
        Object = 0x00090000,
        /// <summary>
        ///   <para>The property contains a list of strings.</para>
        /// </summary>
        StringList = 0x000a0000,

        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority" /> object.</para>
        /// </summary>
        JobPriority = 0x00110000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> object.</para>
        /// </summary>
        JobState = 0x00120000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType" /> object.</para>
        /// </summary>
        JobUnitType = 0x00130000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.CancelRequest" /> object.</para>
        /// </summary>
        CancelRequest = 0x00140000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.FailureReason" /> object.</para>
        /// </summary>
        FailureReason = 0x00150000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.JobType" /> object.</para>
        /// </summary>
        JobType = 0x00160000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object.</para>
        /// </summary>
        JobOrderby = 0x00170000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId" /> object.</para>
        /// </summary>
        TaskId = 0x00180000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.PendingReason" /> object.</para>
        /// </summary>
        PendingReason = 0x00190000,
        /// <summary>
        ///   <para>The property contains a SchedulingMode value. This value is only supported for Windows HPC Server 2008 R2.</para>
        /// </summary>
        JobSchedulingPolicy = 0x001a0000,
        /// <summary>
        ///   <para>The property contains a 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.StorePropertyType.TaskType" /> value. This value is only supported for Windows HPC Server 2008 R2.</para> 
        /// </summary>
        TaskType = 0x001b0000,
        /// <summary>
        ///   <para>The property contains a 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobRuntimeType" /> value. This property was introduced in HPC Pack 2012 and is not supported in previous versions.</para> 
        /// </summary>
        JobRuntimeType = 0x001c0000,
        /// <summary>
        ///   <para>The property contains a 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp" /> value. This property was introduced in HPC Pack 2012 and is not supported in previous versions.</para> 
        /// </summary>
        JobNodeGroupOp = 0x001d0000,
        /// <summary>
        ///   <para>The property contains a multiple string list value. This 
        /// property was introduced in HPC Pack 2012 and is not supported in previous versions.</para>
        /// </summary>
        MultipleStringLists = 0x001e0000,

        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState" /> object.</para>
        /// </summary>
        TaskState = 0x00220000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.ResourceState" /> object.</para>
        /// </summary>
        ResourceState = 0x00310000,
        /// <summary>
        ///   <para>The property contains a ResourceJobPhase value. This value is only supported for Windows HPC Server 2008 R2.</para>
        /// </summary>
        ResourceJobPhase = 0x00320000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.NodeState" /> object.</para>
        /// </summary>
        NodeState = 0x00410000,
        /// <summary>
        ///   <para />
        /// </summary>
        NodeAvailability = 0x00420000,

        /// <summary>
        ///   <para />
        /// </summary>
        NodeLocation = 0x00430000,

        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.NodeEvent" /> object.</para>
        /// </summary>
        NodeEvent = 0x00510000,
        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.JobEvent" /> object.</para>
        /// </summary>
        JobEvent = 0x00520000,

        /// <summary>
        ///   <para>The property contains collection of KeyValuePair objects.</para>
        /// </summary>
        AllocationList = 0x00610000,

        /// <summary>
        ///   <para>The property contains a JobMessageType value. This value is only supported for Windows HPC Server 2008 R2.</para>
        /// </summary>
        JobMessageType = 0x00620000,

        /// <summary>
        ///   <para>The property contains a <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyError" /> enumerated value.</para>
        /// </summary>
        Error = 0x11000000,
        /// <summary>
        ///   <para>A mask that you use to get a type value.</para>
        /// </summary>
        TypeMask = 0x1fff0000
    }

}
