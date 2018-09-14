using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the completed job states for which job history is captured.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get job history, call the 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJobHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> method.</para> 
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const None = 0
    /// const Finished = 1
    /// const Canceled = 2
    /// const Failed = 3
    /// const CancelRequestReceived = 4
    /// const PropChange = 5</code>
    /// </remarks>
    [Serializable]
    public enum JobEvent
    {
        /// <summary>
        ///   <para>The job has not completed. This enumeration member represents a value of 0.</para>
        /// </summary>
        None = 0,
        /// <summary>
        ///   <para>The job finished. This enumeration member represents a value of 1.</para>
        /// </summary>
        Finished = 1,
        /// <summary>
        ///   <para>The job was canceled. This enumeration member represents a value of 2.</para>
        /// </summary>
        Canceled = 2,
        /// <summary>
        ///   <para>The job failed. This enumeration member represents a value of 3.</para>
        /// </summary>
        Failed = 3,
        /// <summary>
        ///   <para>The HPC Job Scheduler Service received a request from a user to cancel the job. This enumeration 
        /// member represents a value of 4. This value was introduced in Windows HPC Server 2008 R2 and is not supported in previous versions.</para>
        /// </summary>
        CancelRequestReceived = 4,
        /// <summary>
        ///   <para>A property of the job was changed by a user. This enumeration member represents a value of 5. This value was introduced in Windows HPC Server 2012 R2 and is not supported in previous versions.</para>
        /// </summary>
        PropChange = 5
    }

    /// <summary>
    ///   <para>Defines the job history properties that you can retrieve when calling the 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJobHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> method.</para> 
    /// </summary>
    public class JobHistoryPropertyIds
    {
        /// <summary>
        ///   <para>The identifier that identifies the job history record.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Id" />.</para>
        /// </remarks>
        static public PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para>The job completion event.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.Properties.JobEvent" />.</para>
        /// </remarks>
        static public PropertyId JobEvent
        {
            get { return _JobEvent; }
        }

        /// <summary>
        ///   <para>The date and time that the job completion event occurred.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId EventTime
        {
            get { return _EventTime; }
        }

        /// <summary>
        ///   <para>The identifier that uniquely identifies the job in the cluster. </para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Id" />.</para>
        /// </remarks>
        static public PropertyId JobId
        {
            get { return _JobId; }
        }

        /// <summary>
        ///   <para>Use to uniquely identify the history records for a job when the job has been queued more than one time.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId RequeueId
        {
            get { return _RequeueId; }
        }

        /// <summary>
        ///   <para>The time that the user submitted the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTime" />.</para>
        /// </remarks>
        static public PropertyId SubmitTime
        {
            get { return _SubmitTime; }
        }

        /// <summary>
        ///   <para>The time that the scheduler started the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.StartTime" />.</para>
        /// </remarks>
        static public PropertyId StartTime
        {
            get { return _StartTime; }
        }

        /// <summary>
        ///   <para>The date and time that the job finished running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EndTime" />.</para>
        /// </remarks>
        static public PropertyId EndTime
        {
            get { return _EndTime; }
        }

        /// <summary>
        ///   <para>The project to which the job belongs.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Project" />.</para>
        /// </remarks>
        static public PropertyId Project
        {
            get { return _Project; }
        }

        /// <summary>
        ///   <para>The job template that the job uses.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.JobTemplate" />.</para>
        /// </remarks>
        static public PropertyId JobTemplate
        {
            get { return _JobTemplate; }
        }

        /// <summary>
        ///   <para>The total CPU time that the job consumed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CpuTime
        {
            get { return _CpuTime; }
        }

        /// <summary>
        ///   <para>The run-time limit for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" />.</para>
        /// </remarks>
        static public PropertyId Runtime
        {
            get { return _Runtime; }
        }

        /// <summary>
        ///   <para>The average amount of memory, in kilobytes, used by all tasks in the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId AverageMemoryUsed
        {
            get { return _AverageMemoryUsed; }
        }

        /// <summary>
        ///   <para>The average amount of memory, in kilobytes, used by all tasks in the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId MemoryUsed
        {
            get { return _AverageMemoryUsed; }
        }

        /// <summary>
        ///   <para>Indicates if the number of resources allocated to the job has grown since resources were last allocated.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" />.</para>
        /// </remarks>
        static public PropertyId HasGrown
        {
            get { return _HasGrown; }
        }

        /// <summary>
        ///   <para>Indicates if the number of resources allocated to the job has shrunk since resources were last allocated.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" />.</para>
        /// </remarks>
        static public PropertyId HasShrunk
        {
            get { return _HasShrunk; }
        }

        /// <summary>
        ///   <para>The name of the service that runs on the nodes of the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a string. This property applies to only web-service jobs. See 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.ServiceName" />.</para>
        /// </remarks>
        static public PropertyId ServiceName
        {
            get { return _ServiceName; }
        }

        /// <summary>
        ///   <para>The number of web-service calls made in the session.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 64-bit integer. This property applies to only web-service jobs.</para>
        /// </remarks>
        static public PropertyId NumberOfCalls
        {
            get { return _NumberOfCalls; }
        }

        /// <summary>
        ///   <para>The average duration of a web-service message call in the session.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is a 64-bit integer. The value is in milliseconds.</para>
        ///   <para>This property applies to only web-service jobs.</para>
        /// </remarks>
        static public PropertyId CallDuration
        {
            get { return _CallDuration; }
        }

        /// <summary>
        ///   <para>The number of web-service calls made in the session in the last second.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is a 64-bit integer.</para>
        ///   <para>This property applies to only web-service jobs.</para>
        /// </remarks>
        static public PropertyId CallsPerSecond
        {
            get { return _CallsPerSecond; }
        }

        /// <summary>
        ///   <para>The user that owns the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Owner" />.</para>
        /// </remarks>
        static public PropertyId JobOwner
        {
            get { return _JobOwner; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId Priority
        {
            get { return _Priority; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId JobType
        {
            get { return _JobType; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId Preemptable
        {
            get { return _Preemptable; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId SoftwareLicense
        {
            get { return _SoftwareLicense; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId RunUntilCanceled
        {
            get { return _RunUntilCanceled; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId IsExclusive
        {
            get { return _IsExclusive; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId FailOnTaskFailure
        {
            get { return _FailOnTaskFailure; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId NumberOfTask
        {
            get { return _NumberOfTask; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId CanceledTasksCount
        {
            get { return _CanceledTasksCount; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId FailedTasksCount
        {
            get { return _FailedTasksCount; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId FinishedTasksCount
        {
            get { return _FinishedTasksCount; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId UserCpu
        {
            get { return _UserCpu; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId KernelCpu
        {
            get { return _KernelCpu; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId JobName
        {
            get { return _JobName; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId ActorSid
        {
            get { return _ActorSid; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId CancelRequest
        {
            get { return _CancelRequest; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId Pool
        {
            get { return _Pool; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId ParentJobIds
        {
            get { return _ParentJobIds; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId ChildJobIds
        {
            get { return _ChildJobIds; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId Operator
        {
            get { return _Operator; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId PropChange
        {
            get { return _PropChange; }
        }

        static PropertyId _JobEvent = new PropertyId(StorePropertyType.JobEvent, "JobEvent", PropertyIdConstants.JobHistoryPropertyIdStart + 1);
        static PropertyId _EventTime = new PropertyId(StorePropertyType.Int32, "EventTime", PropertyIdConstants.JobHistoryPropertyIdStart + 2);
        static PropertyId _JobId = new PropertyId(StorePropertyType.Int32, "JobId", PropertyIdConstants.JobHistoryPropertyIdStart + 3);
        static PropertyId _RequeueId = new PropertyId(StorePropertyType.Int32, "RequeueId", PropertyIdConstants.JobHistoryPropertyIdStart + 4);
        static PropertyId _SubmitTime = new PropertyId(StorePropertyType.Int32, "SubmitTime", PropertyIdConstants.JobHistoryPropertyIdStart + 5);
        static PropertyId _StartTime = new PropertyId(StorePropertyType.Int32, "StartTime", PropertyIdConstants.JobHistoryPropertyIdStart + 6);
        static PropertyId _EndTime = new PropertyId(StorePropertyType.Int32, "EndTime", PropertyIdConstants.JobHistoryPropertyIdStart + 7);
        static PropertyId _Project = new PropertyId(StorePropertyType.String, "Project", PropertyIdConstants.JobHistoryPropertyIdStart + 8);
        static PropertyId _JobTemplate = new PropertyId(StorePropertyType.String, "JobTemplate", PropertyIdConstants.JobHistoryPropertyIdStart + 9);
        static PropertyId _CpuTime = new PropertyId(StorePropertyType.Int64, "CpuTime", PropertyIdConstants.JobHistoryPropertyIdStart + 10);
        static PropertyId _Runtime = new PropertyId(StorePropertyType.Int64, "Runtime", PropertyIdConstants.JobHistoryPropertyIdStart + 11);
        static PropertyId _AverageMemoryUsed = new PropertyId(StorePropertyType.Int64, "MemoryUsed", PropertyIdConstants.JobHistoryPropertyIdStart + 12);
        static PropertyId _HasGrown = new PropertyId(StorePropertyType.Boolean, "HasGrown", PropertyIdConstants.JobHistoryPropertyIdStart + 13);
        static PropertyId _HasShrunk = new PropertyId(StorePropertyType.Boolean, "HasShrunk", PropertyIdConstants.JobHistoryPropertyIdStart + 14);
        static PropertyId _ServiceName = new PropertyId(StorePropertyType.String, "ServiceName", PropertyIdConstants.JobHistoryPropertyIdStart + 15);
        static PropertyId _NumberOfCalls = new PropertyId(StorePropertyType.Int64, "NumberOfCalls", PropertyIdConstants.JobHistoryPropertyIdStart + 16);
        static PropertyId _CallDuration = new PropertyId(StorePropertyType.Int64, "CallDuration", PropertyIdConstants.JobHistoryPropertyIdStart + 17);
        static PropertyId _CallsPerSecond = new PropertyId(StorePropertyType.Int64, "CallsPerSecond", PropertyIdConstants.JobHistoryPropertyIdStart + 18);
        static PropertyId _JobOwner = new PropertyId(StorePropertyType.String, "JobOwner", PropertyIdConstants.JobHistoryPropertyIdStart + 19);
        static PropertyId _Priority = new PropertyId(StorePropertyType.JobPriority, "Priority", PropertyIdConstants.JobHistoryPropertyIdStart + 20);
        static PropertyId _JobType = new PropertyId(StorePropertyType.JobType, "JobType", PropertyIdConstants.JobHistoryPropertyIdStart + 21);
        static PropertyId _Preemptable = new PropertyId(StorePropertyType.Boolean, "Preemptable", PropertyIdConstants.JobHistoryPropertyIdStart + 22);
        static PropertyId _SoftwareLicense = new PropertyId(StorePropertyType.String, "SoftwareLicense", PropertyIdConstants.JobHistoryPropertyIdStart + 23);
        static PropertyId _RunUntilCanceled = new PropertyId(StorePropertyType.Boolean, "RunUntilCanceled", PropertyIdConstants.JobHistoryPropertyIdStart + 24);
        static PropertyId _IsExclusive = new PropertyId(StorePropertyType.Boolean, "IsExclusive", PropertyIdConstants.JobHistoryPropertyIdStart + 25);
        static PropertyId _FailOnTaskFailure = new PropertyId(StorePropertyType.Boolean, "FailOnTaskFailure", PropertyIdConstants.JobHistoryPropertyIdStart + 26);
        static PropertyId _NumberOfTask = new PropertyId(StorePropertyType.Int32, "NumberOfTask", PropertyIdConstants.JobHistoryPropertyIdStart + 27);
        static PropertyId _CanceledTasksCount = new PropertyId(StorePropertyType.Int32, "CanceledTasksCount", PropertyIdConstants.JobHistoryPropertyIdStart + 28);
        static PropertyId _FailedTasksCount = new PropertyId(StorePropertyType.Int32, "FailedTasksCount", PropertyIdConstants.JobHistoryPropertyIdStart + 29);
        static PropertyId _FinishedTasksCount = new PropertyId(StorePropertyType.Int32, "FinishedTasksCount", PropertyIdConstants.JobHistoryPropertyIdStart + 30);
        static PropertyId _UserCpu = new PropertyId(StorePropertyType.Int64, "UserCpu", PropertyIdConstants.JobHistoryPropertyIdStart + 31);
        static PropertyId _KernelCpu = new PropertyId(StorePropertyType.Int64, "KernelCpu", PropertyIdConstants.JobHistoryPropertyIdStart + 32);
        static PropertyId _JobName = new PropertyId(StorePropertyType.String, "JobName", PropertyIdConstants.JobHistoryPropertyIdStart + 33);

        static PropertyId _ActorSid = new PropertyId(StorePropertyType.String, "ActorSid", PropertyIdConstants.JobHistoryPropertyIdStart + 34);
        static PropertyId _CancelRequest = new PropertyId(StorePropertyType.Int32, "CancelRequest", PropertyIdConstants.JobHistoryPropertyIdStart + 35);
        static PropertyId _Pool = new PropertyId(StorePropertyType.String, "Pool", PropertyIdConstants.JobHistoryPropertyIdStart + 36);
        static PropertyId _ParentJobIds = new PropertyId(StorePropertyType.String, "ParentJobIds", PropertyIdConstants.JobHistoryPropertyIdStart + 37);
        static PropertyId _ChildJobIds = new PropertyId(StorePropertyType.String, "ChildJobIds", PropertyIdConstants.JobHistoryPropertyIdStart + 38);

        static PropertyId _Operator = new PropertyId(StorePropertyType.String, "Operator", PropertyIdConstants.JobHistoryPropertyIdStart + 39);
        static PropertyId _PropChange = new PropertyId(StorePropertyType.String, "PropChange", PropertyIdConstants.JobHistoryPropertyIdStart + 40);
    }
}
