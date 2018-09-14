using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the identifiers that uniquely identify the each property in the store.</para>
    /// </summary>
    /// <remarks>
    ///   <para>You should not use these identifiers. Instead you should use the property identifiers defined in the following classes.</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobHistoryPropertyIds" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.NodeHistoryPropertyIds" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.NodePropertyIds" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    public class StorePropertyIds
    {
        // Make it so that no one can construct one of these.
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.StorePropertyIds" /> class.</para>
        /// </summary>
        protected StorePropertyIds()
        {
        }

        /// <summary>
        ///   <para>Used as an unknown property identifier.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId NA
        {
            get { return _NA; }
        }

        /// <summary>
        ///   <para>The reason why the property returned null.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Error
        {
            get { return _error; }
        }

        /// <summary>
        ///   <para>The identifier that uniquely identifies the record in the scheduler.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Id
        {
            get { return _ID; }
        }

        /// <summary>
        ///   <para>The display name of the job, task, or node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Name
        {
            get { return _Name; }
        }

        /// <summary>
        ///   <para>The file version of the HPC server assembly.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Version
        {
            get { return _Version; }
        }

        /// <summary>
        ///   <para>The date and time that the job or task was submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId SubmitTime
        {
            get { return _SubmitTime; }
        }

        /// <summary>
        ///   <para>The date and time that the record was created.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId CreateTime
        {
            get { return _CreateTime; }
        }

        /// <summary>
        ///   <para>The date and time that the job or task started running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId StartTime
        {
            get { return _StartTime; }
        }

        /// <summary>
        ///   <para>The date and time that the job or task finished, failed or was canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId EndTime
        {
            get { return _EndTime; }
        }

        /// <summary>
        ///   <para>The date and time that the record was last touched.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ChangeTime
        {
            get { return _ChangeTime; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public static PropertyId ModifyTime
        {
            get { return _ChangeTime; }
        }

        /// <summary>
        ///   <para>Determines whether cores, nodes, or sockets are used to allocate resources for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId UnitType
        {
            get { return _UnitType; }
        }

        /// <summary>
        ///   <para>The minimum number of cores that the job requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId MinCores
        {
            get { return _MinCores; }
        }

        /// <summary>
        ///   <para>The maximum number of cores that the scheduler can allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId MaxCores
        {
            get { return _MaxCores; }
        }

        /// <summary>
        ///   <para>The minimum number of sockets that the job requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId MinSockets
        {
            get { return _MinSockets; }
        }

        /// <summary>
        ///   <para>The maximum number of sockets that the scheduler can allocated for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId MaxSockets
        {
            get { return _MaxSockets; }
        }

        /// <summary>
        ///   <para>The minimum number of nodes that the job requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId MinNodes
        {
            get { return _MinNodes; }
        }

        /// <summary>
        ///   <para>The maximum number of nodes that the scheduler can allocated for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId MaxNodes
        {
            get { return _MaxNodes; }
        }

        /// <summary>
        ///   <para>A description of the error that caused the job to fail or the reason the job was canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ErrorMessage
        {
            get { return _ErrorMessage; }
        }

        /// <summary>
        ///   <para>An error code that identifies an error message string.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ErrorCode
        {
            get { return _ErrorCode; }
        }

        /// <summary>
        ///   <para>The insert strings that are applied to the error message string.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ErrorParams
        {
            get { return _ErrorParams; }
        }

        /// <summary>
        ///   <para>The number of seconds in which the job or task should run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId RuntimeSeconds
        {
            get { return _RuntimeSeconds; }
        }

        /// <summary>
        ///   <para>Indicates that the job will run until the user cancels it.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId RunUntilCanceled
        {
            get { return _RunUntilCancelled; }
        }

        /// <summary>
        ///   <para>Determines whether nodes should be exclusively allocated to the job or task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId IsExclusive
        {
            get { return _IsExclusive; }
        }

        /// <summary>
        ///   <para>The total CPU time used by the task (includes the time spent in user-mode and kernel-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId TotalCpuTime
        {
            get { return _TotalCpuTime; }
        }

        /// <summary>
        ///   <para>The time that the task spent in user-mode.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId TotalUserTime
        {
            get { return _TotalUserTime; }
        }

        /// <summary>
        ///   <para>The time that the task spent in kernel-mode.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId TotalKernelTime
        {
            get { return _TotalKernelTime; }
        }

        /// <summary>
        ///   <para>The internal job object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId JobObject
        {
            get { return _JobObject; }
        }

        /// <summary>
        ///   <para>The internal task object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId TaskObject
        {
            get { return _TaskObject; }
        }

        /// <summary>
        ///   <para>The internal resource object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ResourceObject
        {
            get { return _ResourceObject; }
        }

        /// <summary>
        ///   <para>Internal job template object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId TemplateObject
        {
            get { return _ProfileObject; }
        }

        /// <summary>
        ///   <para>The internal node object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId NodeObject
        {
            get { return _NodeObject; }
        }

        /// <summary>
        ///   <para>The internal allocation object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId AllocationObject
        {
            get { return _AllocationObject; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public static PropertyId JobMessageObject
        {
            get { return _JobMessageObject; }
        }

        /// <summary>
        ///   <para>Reserved.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Nothing
        {
            get { return _Nothing; }
        }

        /// <summary>
        ///   <para>The total number of jobs in the scheduler.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TotalJobCount
        {
            get { return _TotalJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that are being configured and have not yet been added to the scheduling queue.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ConfigJobCount
        {
            get { return _ConfigJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that have been submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId SubmittedJobCount
        {
            get { return _SubmittedJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs in the scheduler that are being validated.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ValidatingJobCount
        {
            get { return _ValidatingJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that have been queued.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId QueuedJobCount
        {
            get { return _QueuedJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId RunningJobCount
        {
            get { return _RunningJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that have finished running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId FinishedJobCount
        {
            get { return _FinishedJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that have failed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId FailedJobCount
        {
            get { return _FailedJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that have been canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CanceledJobCount
        {
            get { return _CanceledJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that are finishing.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId FinishingJobCount
        {
            get { return _FinishingJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that are being canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CancelingJobCount
        {
            get { return _CancelingJobCount; }
        }

        /// <summary>
        ///   <para>The number of jobs in the external validation state.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is an integer value.</para>
        /// </remarks>
        static public PropertyId ExternalValidationJobCount
        {
            get { return _ExternalJobCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that are being configured and have not yet been added to a job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ConfiguringTaskCount
        {
            get { return _ConfiguringTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>
        /// Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.
        /// </para>
        /// </value>
        static public PropertyId SubmittedTaskCount
        {
            get { return _SubmittedTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks in the cluster that are being validated.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ValidatingTaskCount
        {
            get { return _ValidatingTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been queued.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId QueuedTaskCount
        {
            get { return _QueuedTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks being dispatched to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId DispatchingTaskCount
        {
            get { return _DispatchingTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId RunningTaskCount
        {
            get { return _RunningTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that are finishing.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId FinishingTaskCount
        {
            get { return _FinishingTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have finished running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId FinishedTaskCount
        {
            get { return _FinishedTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have failed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId FailedTaskCount
        {
            get { return _FailedTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CanceledTaskCount
        {
            get { return _CanceledTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that are being canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CancelingTaskCount
        {
            get { return _CancelingTaskCount; }
        }

        /// <summary>
        ///   <para>The total number of tasks in the scheduler.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TotalTaskCount
        {
            get { return _TotalTaskCount; }
        }

        /// <summary>
        ///   <para>The total number of cores in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TotalCoreCount
        {
            get { return _TotalResourceCount; }
        }

        /// <summary>
        ///   <para>The number of cores that are offline.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId OfflineResourceCount
        {
            get { return _OfflineResourceCount; }
        }

        /// <summary>
        ///   <para>The number of times that the node has been idle.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId IdleResourceCount
        {
            get { return _IdleResourceCount; }
        }

        /// <summary>
        ///   <para>The number of cores that are reserved to run a job sometime in the future.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ScheduledReserveResourceCount
        {
            get { return _ScheduledReserveResourceCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that have been scheduled on a node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId JobScheduledResourceCount
        {
            get { return _JobScheduledResourceCount; }
        }

        /// <summary>
        ///   <para>The number of cores that are ready to run a task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ReadyForTaskResourceCount
        {
            get { return _ReadyForTaskResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been scheduled on a core to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TaskScheduledResourceCount
        {
            get { return _TaskScheduledResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been scheduled on a node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId JobTaskScheduledResourceCount
        {
            get { return _JobTaskScheduledResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been dispatched to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TaskDispatchedResourceCount
        {
            get { return _TaskDispatchedResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TaskRunningResourceCount
        {
            get { return _TaskRunningResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been closed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CloseTaskResourceCount
        {
            get { return _CloseTaskResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that the server has requested be closed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CloseTaskDispatchedResourceCount
        {
            get { return _CloseTaskDispatchedResourceCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been closed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TaskClosedResourceCount
        {
            get { return _TaskClosedResourceCount; }
        }

        /// <summary>
        ///   <para>The number of jobs that have been closed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CloseJobResourceCount
        {
            get { return _CloseJobResourceCount; }
        }

        /// <summary>
        ///   <para>The number of cores in the cluster that are unreachable.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId UnreachableResourceCount
        {
            get { return _UnreachableResourceCount; }
        }

        /// <summary>
        ///   <para>The total number of sockets in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TotalSocketCount
        {
            get { return _TotalSocketCount; }
        }

        /// <summary>
        ///   <para>The total number of nodes in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TotalNodeCount
        {
            get { return _TotalNodeCount; }
        }

        /// <summary>
        ///   <para>The number of nodes that are offline.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId OfflineNodeCount
        {
            get { return _OfflineNodeCount; }
        }

        /// <summary>
        ///   <para>The number of nodes that are marked to be taken offline after the current job finishes.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId DrainingNodeCount
        {
            get { return _DrainingNodeCount; }
        }

        /// <summary>
        ///   <para>The number of nodes that are ready to run a job or task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ReadyNodeCount
        {
            get { return _ReadyNodeCount; }
        }

        /// <summary>
        ///   <para>The number of nodes in the cluster that are unreachable.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId UnreachableNodeCount
        {
            get { return _UnreachableNodeCount; }
        }

        /// <summary>
        ///   <para>The SQL time stamp that identifies when the row was last changed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Timestamp
        {
            get { return _Timestamp; }
        }

        /// <summary>
        ///   <para>The nodes that were allocated to the job the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId AllocatedNodes
        {
            get { return _AllocatedNodes; }
        }

        /// <summary>
        ///   <para>The sockets that were allocated to the job the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId AllocatedSockets
        {
            get { return _AllocatedSockets; }
        }

        /// <summary>
        ///   <para>The cores that were allocated to the job the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId AllocatedCores
        {
            get { return _AllocatedCores; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId AllocatedCoreIds
        {
            get { return _AllocatedCoreIds; }
        }

        /// <summary>
        ///   <para>The types of jobs that can run on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>
        /// Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.
        /// </para>
        /// </value>
        static public PropertyId JobType
        {
            get { return _JobType; }
        }

        /// <summary>
        ///   <para>Indicates if the run-time limit for the job or task is set.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId HasRuntime
        {
            get { return _HasRuntime; }
        }

        /// <summary>
        ///   <para>The process identifiers that identify the processes used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ProcessIds
        {
            get { return _ProcessIds; }
        }

        /// <summary>
        ///   <para>The amount of memory, in kilobytes, used by the job or task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The type of the property is integer. </para>
        /// </remarks>
        public static PropertyId MemoryUsed
        {
            get { return _MemoryUsed; }
        }

        /// <summary>
        ///   <para>The UTC data and time that the row was returned.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId RowTime
        {
            get { return _RowTime; }
        }

        /// <summary>
        ///   <para>The internal node history object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId NodeHistoryObject
        {
            get { return _NodeHistoryObject; }
        }

        /// <summary>
        ///   <para>The custom properties set on a job and task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId FetchAllCustomProps
        {
            get { return _FetchAllCustomProps; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId PoolObject
        {
            get { return _PoolObject; }
        }

        //
        // Private static definitions follow.
        //

        static PropertyId _NA = new PropertyId(StorePropertyType.None, "NA", 0, PropFlags.None);
        static PropertyId _Nothing = new PropertyId(StorePropertyType.None, "Nothing", -1, PropFlags.None);

        static PropertyId _ID = new PropertyId(StorePropertyType.Int32, "Id", 1, PropFlags.Visible | PropFlags.Indexed | PropFlags.ReadOnly);
        static PropertyId _Name = new PropertyId(StorePropertyType.String, "Name", 2, PropFlags.Visible | PropFlags.Indexed);
        static PropertyId _Version = new PropertyId(StorePropertyType.String, "Version", 3, PropFlags.Visible | PropFlags.Calculated);

        static PropertyId _SubmitTime = new PropertyId(StorePropertyType.DateTime, "SubmitTime", 10, PropFlags.Visible | PropFlags.Indexed);
        static PropertyId _CreateTime = new PropertyId(StorePropertyType.DateTime, "CreateTime", 11, PropFlags.Visible | PropFlags.Indexed);
        static PropertyId _StartTime = new PropertyId(StorePropertyType.DateTime, "StartTime", 12, PropFlags.Visible | PropFlags.Indexed);
        static PropertyId _EndTime = new PropertyId(StorePropertyType.DateTime, "EndTime", 14, PropFlags.Visible | PropFlags.Indexed);
        static PropertyId _ChangeTime = new PropertyId(StorePropertyType.DateTime, "ChangeTime", 16, PropFlags.Visible | PropFlags.Indexed);


        static PropertyId _UnitType = new PropertyId(StorePropertyType.JobUnitType, "UnitType", 39);
        static PropertyId _MinCores = new PropertyId(StorePropertyType.Int32, "MinCores", 40);
        static PropertyId _MaxCores = new PropertyId(StorePropertyType.Int32, "MaxCores", 41);
        static PropertyId _MinSockets = new PropertyId(StorePropertyType.Int32, "MinSockets", 42);
        static PropertyId _MaxSockets = new PropertyId(StorePropertyType.Int32, "MaxSockets", 43);
        static PropertyId _MinNodes = new PropertyId(StorePropertyType.Int32, "MinNodes", 44);
        static PropertyId _MaxNodes = new PropertyId(StorePropertyType.Int32, "MaxNodes", 45);

        static PropertyId _ErrorMessage = new PropertyId(StorePropertyType.String, "ErrorMessage", 50);

        static PropertyId _RuntimeSeconds = new PropertyId(StorePropertyType.Int32, "RuntimeSeconds", 51);
        static PropertyId _RunUntilCancelled = new PropertyId(StorePropertyType.Boolean, "RunUntilCanceled", 52);

        static PropertyId _IsExclusive = new PropertyId(StorePropertyType.Boolean, "IsExclusive", 53);

        static PropertyId _ErrorCode = new PropertyId(StorePropertyType.Int32, "ErrorCode", 54);
        static PropertyId _ErrorParams = new PropertyId(StorePropertyType.String, "ErrorParams", 55);

        static PropertyId _HasRuntime = new PropertyId(StorePropertyType.Boolean, "HasRuntime", 56);

        static PropertyId _ProcessIds = new PropertyId(StorePropertyType.String, "ProcessIds", 57);

        static PropertyId _TotalJobCount = new PropertyId(StorePropertyType.Int32, "TotalJobCount", 101, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _ConfigJobCount = new PropertyId(StorePropertyType.Int32, "ConfigJobCount", 102, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _SubmittedJobCount = new PropertyId(StorePropertyType.Int32, "SubmittedJobCount", 103, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _ValidatingJobCount = new PropertyId(StorePropertyType.Int32, "ValidatingJobCount", 104, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _QueuedJobCount = new PropertyId(StorePropertyType.Int32, "QueuedJobCount", 105, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _RunningJobCount = new PropertyId(StorePropertyType.Int32, "RunningJobCount", 106, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _FinishedJobCount = new PropertyId(StorePropertyType.Int32, "FinishedJobCount", 107, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _FailedJobCount = new PropertyId(StorePropertyType.Int32, "FailedJobCount", 108, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _CanceledJobCount = new PropertyId(StorePropertyType.Int32, "CanceledJobCount", 109, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _FinishingJobCount = new PropertyId(StorePropertyType.Int32, "FinishingJobCount", 110, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _CancelingJobCount = new PropertyId(StorePropertyType.Int32, "CancelingJobCount", 111, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _ExternalJobCount = new PropertyId(StorePropertyType.Int32, "ExternalJobCount", 112, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);


        static PropertyId _TotalTaskCount = new PropertyId(StorePropertyType.Int32, "TaskCount", 120, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _ConfiguringTaskCount = new PropertyId(StorePropertyType.Int32, "ConfiguringTaskCount", 121, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _SubmittedTaskCount = new PropertyId(StorePropertyType.Int32, "SubmittedTaskCount", 122, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _ValidatingTaskCount = new PropertyId(StorePropertyType.Int32, "ValidatingTaskCount", 123, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _QueuedTaskCount = new PropertyId(StorePropertyType.Int32, "QueuedTaskCount", 124, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _DispatchingTaskCount = new PropertyId(StorePropertyType.Int32, "DispatchingTaskCount", 125, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _RunningTaskCount = new PropertyId(StorePropertyType.Int32, "RunningTaskCount", 126, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _FinishingTaskCount = new PropertyId(StorePropertyType.Int32, "FinishingTaskCount", 127, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _FinishedTaskCount = new PropertyId(StorePropertyType.Int32, "FinishedTaskCount", 128, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _FailedTaskCount = new PropertyId(StorePropertyType.Int32, "FailedTaskCount", 129, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _CanceledTaskCount = new PropertyId(StorePropertyType.Int32, "CanceledTaskCount", 130, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _CancelingTaskCount = new PropertyId(StorePropertyType.Int32, "CancelingTaskCount", 131, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);

        static PropertyId _TotalResourceCount = new PropertyId(StorePropertyType.Int32, "TotalResourceCount", 140, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _OfflineResourceCount = new PropertyId(StorePropertyType.Int32, "OfflineResourceCount", 141, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _IdleResourceCount = new PropertyId(StorePropertyType.Int32, "IdleResourceCount", 142, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _ScheduledReserveResourceCount = new PropertyId(StorePropertyType.Int32, "ReservedResourceCount", 143, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _JobScheduledResourceCount = new PropertyId(StorePropertyType.Int32, "JobScheduledResourceCount", 144, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _ReadyForTaskResourceCount = new PropertyId(StorePropertyType.Int32, "ReadyForTaskResourceCount", 145, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _TaskScheduledResourceCount = new PropertyId(StorePropertyType.Int32, "TaskScheduledResourceCount", 146, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _JobTaskScheduledResourceCount = new PropertyId(StorePropertyType.Int32, "JobTaskScheduledResourceCount", 147, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _TaskDispatchedResourceCount = new PropertyId(StorePropertyType.Int32, "TaskDispatchedResourceCount", 148, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _TaskRunningResourceCount = new PropertyId(StorePropertyType.Int32, "TaskRunningResourceCount", 149, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _CloseTaskResourceCount = new PropertyId(StorePropertyType.Int32, "CloseTaskResourceCount", 150, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _CloseTaskDispatchedResourceCount = new PropertyId(StorePropertyType.Int32, "CloseTaskDispatchedResourceCount", 151, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _TaskClosedResourceCount = new PropertyId(StorePropertyType.Int32, "TaskClosedResourceCount", 152, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _CloseJobResourceCount = new PropertyId(StorePropertyType.Int32, "CloseJobResourceCount", 153, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _UnreachableResourceCount = new PropertyId(StorePropertyType.Int32, "UnreachableResourceCount", 157, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _TotalSocketCount = new PropertyId(StorePropertyType.Int32, "TotalSocketCount", 158, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);

        static PropertyId _TotalNodeCount = new PropertyId(StorePropertyType.Int32, "TotalNodeCount", 160, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _OfflineNodeCount = new PropertyId(StorePropertyType.Int32, "OfflineNodeCount", 161, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _DrainingNodeCount = new PropertyId(StorePropertyType.Int32, "DraingingNodeCount", 162, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _ReadyNodeCount = new PropertyId(StorePropertyType.Int32, "ReadyNodeCount", 163, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _UnreachableNodeCount = new PropertyId(StorePropertyType.Int32, "UnreachableNodeCount", 164, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);

        static PropertyId _TotalCpuTime = new PropertyId(StorePropertyType.Int64, "TotalCpuTime", 170, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _TotalUserTime = new PropertyId(StorePropertyType.Int64, "TotalUserTime", 171, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _TotalKernelTime = new PropertyId(StorePropertyType.Int64, "TotalKernelTime", 172, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _MemoryUsed = new PropertyId(StorePropertyType.Int64, "MemoryUsed", 173, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);


        static PropertyId _Timestamp = new PropertyId(StorePropertyType.Binary, "Timestamp", 180, PropFlags.ReadOnly);

        static PropertyId _BackfillLimit = new PropertyId(StorePropertyType.Int32, "BackfillLimit", 200);

        static PropertyId _AllocatedNodes = new PropertyId(StorePropertyType.AllocationList, "AllocatedNodes", 300, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _AllocatedSockets = new PropertyId(StorePropertyType.AllocationList, "AllocatedSockets", 301, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _AllocatedCores = new PropertyId(StorePropertyType.AllocationList, "AllocatedCores", 302, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);

        static PropertyId _AllocatedCoreIds = new PropertyId(StorePropertyType.String, "AllocatedCoreIds", 310, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);

        static PropertyId _JobType = new PropertyId(StorePropertyType.JobType, "JobType", 400);

        static PropertyId _FetchAllCustomProps = new PropertyId(StorePropertyType.Boolean, "FetchAllCustomProps", 998, PropFlags.None);
        static PropertyId _error = new PropertyId(StorePropertyType.Error, "Error", 999, PropFlags.None);

        static PropertyId _JobObject = new PropertyId(StorePropertyType.Object, "JobObject", 91024, PropFlags.None);
        static PropertyId _TaskObject = new PropertyId(StorePropertyType.Object, "TaskObject", 91025, PropFlags.None);
        static PropertyId _ResourceObject = new PropertyId(StorePropertyType.Object, "ResourceObject", 91026, PropFlags.None);
        static PropertyId _ProfileObject = new PropertyId(StorePropertyType.Object, "ProfileObject", 91027, PropFlags.None);
        static PropertyId _NodeObject = new PropertyId(StorePropertyType.Object, "NodeObject", 91028, PropFlags.None);
        static PropertyId _AllocationObject = new PropertyId(StorePropertyType.Object, "AllocationObject", 91029, PropFlags.None);
        static PropertyId _NodeHistoryObject = new PropertyId(StorePropertyType.Object, "NodeHistoryObject", 91030, PropFlags.None);
        static PropertyId _JobMessageObject = new PropertyId(StorePropertyType.Object, "JobMessageObject", 91031, PropFlags.None);
        static PropertyId _PoolObject = new PropertyId(StorePropertyType.Object, "PoolObject", 91032, PropFlags.None);

        static PropertyId _RowTime = new PropertyId(StorePropertyType.DateTime, "RowTime", 91099, PropFlags.ReadOnly);
    }
}
