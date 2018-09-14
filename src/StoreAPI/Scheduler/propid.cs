// WARNING
// This file is auto generated.  Do not edit by hand!

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines identifiers that uniquely identify job, task, and node 
    /// properties. Use these property identifiers when specifying a filter or sort property.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const Job_Name = 2
    /// const Job_Owner = 1003
    /// const Job_UserName = 1004
    /// const Job_Priority = 1017
    /// const Job_Project = 1006
    /// const Job_RuntimeSeconds = 51
    /// const Job_SubmitTime = 10
    /// const Job_CreateTime = 11
    /// const Job_EndTime  = 14
    /// const Job_StartTime = 12
    /// const Job_ChangeTime = 16
    /// const Job_State = 1001
    /// const Job_PreviousState = 1002
    /// const Job_MinCores = 40
    /// const Job_MaxCores = 41
    /// const Job_MinNodes = 44
    /// const Job_MaxNodes = 45
    /// const Job_MinSockets = 42
    /// const Job_MaxSockets = 43
    /// const Job_UnitType = 39
    /// const Job_IsExclusive = 53
    /// const Job_RunUntilCanceled = 52
    /// const Job_AutoCalculateMax  = 1083
    /// const Job_AutoCalculateMin = 1084
    /// const Job_CanGrow = 1059
    /// const Job_CanShrink = 1060
    /// const Job_RequeueCount = 1075
    /// const Job_JobType = 1007
    /// const Task_Name = 2
    /// const Task_State = 2001
    /// const Task_PreviousState = 2002
    /// const Task_MinCores = 40
    /// const Task_MaxCores = 41
    /// const Task_MinNodes = 44
    /// const Task_MaxNodes = 45
    /// const Task_MinSockets = 42
    /// const Task_MaxSockets = 43
    /// const Task_RuntimeSeconds = 51
    /// const Task_SubmitTime = 10
    /// const Task_CreateTime = 11
    /// const Task_StartTime = 12
    /// const Task_EndTime = 14
    /// const Task_ChangeTime = 16
    /// const Task_ParentJobId = 2003
    /// const Task_IsExclusive = 53
    /// const Task_IsRerunnable = 2015
    /// const Task_ExitCode = 2033
    /// const Task_RequeueCount = 2044
    /// const Task_IsParametric = 2050
    /// const Task_Type = 2080
    /// const Node_Name = 2
    /// const Node_JobType = 400
    /// const Node_State = 8001
    /// const Node_Reachable = 8004
    /// const Node_NumCores = 8005
    /// const Node_NumSockets = 8006
    /// const Node_OfflineTime = 8008
    /// const Node_OnlineTime = 8009
    /// const Node_Guid = 8015
    /// const Node_MemorySize = 8016
    /// const Node_CpuSpeed = 8017</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.IFilterCollection.Add(Microsoft.Hpc.Scheduler.Properties.FilterOperator,Microsoft.Hpc.Scheduler.PropId,System.Object)" 
    /// /> 
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISortCollection.Add(Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder,Microsoft.Hpc.Scheduler.PropId)" 
    /// /> 
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidPropIdEnum)]
    public enum PropId
    {
        /// <summary>
        ///   <para>Identifies the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Name" /> property. This enumeration member represents a value of 2.</para>
        /// </summary>
        Job_Name = 2,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Owner" /> property. This enumeration member represents a value of 1003.</para>
        /// </summary>
        Job_Owner = 1003,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UserName" /> property. This enumeration member represents a value of 1004.</para>
        /// </summary>
        Job_UserName = 1004,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Priority" /> property. This enumeration member represents a value of 1017.</para>
        /// </summary>
        Job_Priority = 1017,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Project" /> property. This enumeration member represents a value of 1006.</para>
        /// </summary>
        Job_Project = 1006,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" /> property. This enumeration member represents a value of 51.</para>
        /// </summary>
        Job_RuntimeSeconds = 51,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTime" /> property. This enumeration member represents a value of 10.</para>
        /// </summary>
        Job_SubmitTime = 10,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTime" /> property. This enumeration member represents a value of 11.</para>
        /// </summary>
        Job_CreateTime = 11,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EndTime" /> property. This enumeration member represents a value of 14.</para>
        /// </summary>
        Job_EndTime = 14,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.StartTime" /> property. This enumeration member represents a value of 12.</para>
        /// </summary>
        Job_StartTime = 12,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ChangeTime" /> property. This enumeration member represents a value of 16.</para>
        /// </summary>
        Job_ChangeTime = 16,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.State" /> property. This enumeration member represents a value of 1001.</para>
        /// </summary>
        Job_State = 1001,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.PreviousState" /> property. This enumeration member represents a value of 1002. </para>
        /// </summary>
        Job_PreviousState = 1002,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" /> property. This enumeration member represents a value of 40.</para>
        /// </summary>
        Job_MinCores = 40,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" /> property. This enumeration member represents a value of 41.</para>
        /// </summary>
        Job_MaxCores = 41,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" /> property. This enumeration member represents a value of 44.</para>
        /// </summary>
        Job_MinNodes = 44,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" /> property. This enumeration member represents a value of 45.</para>
        /// </summary>
        Job_MaxNodes = 45,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" /> property. This enumeration member represents a value of 42.</para>
        /// </summary>
        Job_MinSockets = 42,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" /> property. This enumeration member represents a value of 43.</para>
        /// </summary>
        Job_MaxSockets = 43,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property. This enumeration member represents a value of 39.</para>
        /// </summary>
        Job_UnitType = 39,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.IsExclusive" /> property. This enumeration member represents a value of 53.</para>
        /// </summary>
        Job_IsExclusive = 53,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" /> property. This enumeration member represents a value of 52.</para>
        /// </summary>
        Job_RunUntilCanceled = 52,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" /> property. This enumeration member represents a value of 1083.</para>
        /// </summary>
        Job_AutoCalculateMax = 1083,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" /> property. This enumeration member represents a value of 1084.</para>
        /// </summary>
        Job_AutoCalculateMin = 1084,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" /> property. This enumeration member represents a value of 1059.</para>
        /// </summary>
        Job_CanGrow = 1059,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" /> property. This enumeration member represents a value of 1060.</para>
        /// </summary>
        Job_CanShrink = 1060,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequeueCount" /> property. This enumeration member represents a value of 1075.</para>
        /// </summary>
        Job_RequeueCount = 1075,
        /// <summary>
        ///   <para>Filter or sort the jobs based on the type of job (for example, a command or a scheduled batch job). For possible job type values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobType" /> enumeration. This enumeration member represents a value of 1007.</para>
        /// </summary>
        Job_JobType = 1007,

        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Name" /> property. This enumeration member represents a value of 2.</para>
        /// </summary>
        Task_Name = 2,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.State" /> property. This enumeration member represents a value of 2001.</para>
        /// </summary>
        Task_State = 2001,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.PreviousState" /> property. This enumeration member represents a value of 2002.</para>
        /// </summary>
        Task_PreviousState = 2002,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfCores" /> property. This enumeration member represents a value of 40.</para>
        /// </summary>
        Task_MinCores = 40,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfCores" /> property. This enumeration member represents a value of 41.</para>
        /// </summary>
        Task_MaxCores = 41,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfNodes" /> property. This enumeration member represents a value of 44.</para>
        /// </summary>
        Task_MinNodes = 44,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfNodes" /> property. This enumeration member represents a value of 45.</para>
        /// </summary>
        Task_MaxNodes = 45,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfSockets" /> property. This enumeration member represents a value of 42.</para>
        /// </summary>
        Task_MinSockets = 42,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfSockets" /> property. This enumeration member represents a value of 43.</para>
        /// </summary>
        Task_MaxSockets = 43,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Runtime" /> property. This enumeration member represents a value of 51.</para>
        /// </summary>
        Task_RuntimeSeconds = 51,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.SubmitTime" /> property. This enumeration member represents a value of 10.</para>
        /// </summary>
        Task_SubmitTime = 10,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.CreateTime" /> property. This enumeration member represents a value of 11.</para>
        /// </summary>
        Task_CreateTime = 11,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StartTime" /> property. This enumeration member represents a value of 12.</para>
        /// </summary>
        Task_StartTime = 12,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EndTime" /> property. This enumeration member represents a value of 14.</para>
        /// </summary>
        Task_EndTime = 14,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ChangeTime" /> property. This enumeration member represents a value of 16.</para>
        /// </summary>
        Task_ChangeTime = 16,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ParentJobId" /> property. This enumeration member represents a value of 2003.</para>
        /// </summary>
        Task_ParentJobId = 2003,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsExclusive" /> property. This enumeration member represents a value of 53.</para>
        /// </summary>
        Task_IsExclusive = 53,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsRerunnable" /> property. This enumeration member represents a value of 2015.</para>
        /// </summary>
        Task_IsRerunnable = 2015,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ExitCode" /> property. This enumeration member represents a value of 2033.</para>
        /// </summary>
        Task_ExitCode = 2033,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequeueCount" /> property. This enumeration member represents a value of 2044.</para>
        /// </summary>
        Task_RequeueCount = 2044,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property. This enumeration member represents a value of 2050.</para>
        /// </summary>
        Task_IsParametric = 2050,
        /// <summary>
        ///   <para>Identifies the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Type" /> property. This member was introduced in Windows HPC Server 2008 R2 and is not supported in previous versions. This enumeration member represents a value of 2080.</para> 
        /// </summary>
        Task_Type = 2080,

        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Name" /> property. This enumeration member represents a value of 2.</para>
        /// </summary>
        Node_Name = 2,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.JobType" /> property. This enumeration member represents a value of 400.</para>
        /// </summary>
        Node_JobType = 400,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.State" /> property. This enumeration member represents a value of 8001.</para>
        /// </summary>
        Node_State = 8001,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Reachable" /> property. This enumeration member represents a value of 8004.</para>
        /// </summary>
        Node_Reachable = 8004,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.NumberOfCores" /> property. This enumeration member represents a value of 8005.</para>
        /// </summary>
        Node_NumCores = 8005,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.NumberOfSockets" /> property. This enumeration member represents a value of 8006.</para>
        /// </summary>
        Node_NumSockets = 8006,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OfflineTime" /> property. This enumeration member represents a value of 8008.</para>
        /// </summary>
        Node_OfflineTime = 8008,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OnlineTime" /> property. This enumeration member represents a value of 8009.</para>
        /// </summary>
        Node_OnlineTime = 8009,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Guid" /> property. This enumeration member represents a value of 8015.</para>
        /// </summary>
        Node_Guid = 8015,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.MemorySize" /> property. This enumeration member represents a value of 8016.</para>
        /// </summary>
        Node_MemorySize = 8016,
        /// <summary>
        ///   <para>Identifies the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.CpuSpeed" /> property. This enumeration member represents a value of 8017.</para>
        /// </summary>
        Node_CpuSpeed = 8017,
    };
}

