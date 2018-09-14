using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the priorities that you can specify for a job.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const Lowest = 0
    /// const BelowNormal = 1
    /// const Normal = 2
    /// const AboveNormal = 3
    /// const Highest = 4</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Priority" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidJobPriority)]
    public enum JobPriority
    {
        /// <summary>
        ///   <para>The job has the lowest priority. This enumeration member represents a value of 0.</para>
        /// </summary>
        Lowest = 0,
        /// <summary>
        ///   <para>The job has below-normal priority. This enumeration member represents a value of 1.</para>
        /// </summary>
        BelowNormal = 1,
        /// <summary>
        ///   <para>The job has normal priority. This enumeration member represents a value of 2.</para>
        /// </summary>
        Normal = 2,
        /// <summary>
        ///   <para>The job has above-normal priority. This enumeration member represents a value of 3.</para>
        /// </summary>
        AboveNormal = 3,
        /// <summary>
        ///   <para>The job has the highest priority. This enumeration member represents a value of 4.</para>
        /// </summary>
        Highest = 4
    }

    /// <summary>
    ///   <para>Defines the state of the job.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const Configuring = 1
    /// const Submitted = 2
    /// const Validating = 4
    /// const ExternalValidation = 8
    /// const Queued = 16
    /// const Running = 32
    /// const Finishing = 64
    /// const Finished = 128
    /// const Failed = 256
    /// const Canceled = 512
    /// const Canceling = 1024
    /// const All = 2047</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.PreviousState" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.State" />
    [Flags]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidJobState)]
    public enum JobState
    {
        /// <summary>
        ///   <para>The job is being configured. The application called the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateJob" /> method to create the job but has not called the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> method to add the job to the scheduler or submit the job to the scheduling queue. This enumeration member represents a value of 1.</para> 
        /// </summary>
        Configuring = 0x001,
        /// <summary>
        ///   <para>The job was submitted to the scheduling queue (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />). This enumeration member represents a value of 2.</para> 
        /// </summary>
        Submitted = 0x002,
        /// <summary>
        ///   <para>The server is determining if the job can run. This enumeration member represents a value of 4.</para>
        /// </summary>
        Validating = 0x004,
        /// <summary>
        ///   <para>A submission filter is determining if the job can run. 
        /// For details, see the SubmissionFilterProgram cluster parameter in the Remarks section of  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" />. This enumeration member represents a value of 8.</para> 
        /// </summary>
        ExternalValidation = 0x008,
        /// <summary>
        ///   <para>The job passed validation and was added to the scheduling queue. This enumeration member represents a value of 16.</para>
        /// </summary>
        Queued = 0x010,
        /// <summary>
        ///   <para>The job is running. This enumeration member represents a value of 32.</para>
        /// </summary>
        Running = 0x020,
        /// <summary>
        ///   <para>The server is cleaning up the resources that were allocated to the job. This enumeration member represents a value of 64.</para>
        /// </summary>
        Finishing = 0x040,
        /// <summary>
        ///   <para>The job successfully finished (all the tasks in the job finished successfully). This enumeration member represents a value of 128.</para>
        /// </summary>
        Finished = 0x080,
        /// <summary>
        ///   <para>One or more of the tasks in the job failed or a system error occurred on the compute node. To get a description of the error, access the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ErrorMessage" /> property. This enumeration member represents a value of 256.</para>
        /// </summary>
        Failed = 0x100,
        /// <summary>
        ///   <para>The job was canceled (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String)" />). If the caller provided the reason for canceling the job, then the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ErrorMessage" /> property will contain the reason. This enumeration member represents a value of 512.</para> 
        /// </summary>
        Canceled = 0x200,
        /// <summary>
        ///   <para>The job is being canceled. This enumeration member represents a value of 1024.</para>
        /// </summary>
        Canceling = 0x400,
        /// <summary>
        ///   <para>A mask used to indicate all states. This enumeration member represents a value of 2047.</para>
        /// </summary>
        All = Configuring | Submitted | Validating | ExternalValidation | Queued | Running | Finishing | Finished | Failed | Canceling | Canceled,
    }

    /// <summary>
    ///   <para>Defines the hardware resources used to determine on which nodes the job can run.</para>
    /// </summary>
    /// <remarks>
    ///   <para>The least granular resource unit is the node and the most granular resource unit is the processor. For example, a 
    /// node can have four processors on one socket and the node can contain multiple sockets. A job can specify that it needs a  
    /// minimum of four nodes to run, regardless of how many processors are on each node. The job could also specify that it needs 
    /// four processors to run, so it could run on one node that had four processors or on multiple nodes with one or two processors.</para> 
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code>const Core = 0
    /// const Socket = 1
    /// const Node = 2
    /// const Gpu = 3</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidJobUnitType)]
    public enum JobUnitType
    {
        /// <summary>
        ///   <para>Use cores to schedule the job. This enumeration member represents a value of 0.</para>
        /// </summary>
        Core = 0,
        /// <summary>
        ///   <para>Use sockets to scheduler the job. This enumeration member represents a value of 1.</para>
        /// </summary>
        Socket = 1,
        /// <summary>
        ///   <para>Use nodes to schedule the job. This enumeration member represents a value of 2.</para>
        /// </summary>
        Node = 2,
        /// <summary>
        ///   <para>Use GPUs to schedule the job. This enumeration member represents a value of 3.</para>
        /// </summary>
        Gpu = 3,
    }

    /// <summary>
    ///   <para>Defines how the job was canceled.</para>
    /// </summary>
    public enum CancelRequest
    {
        /// <summary>
        ///   <para>The job was not canceled.</para>
        /// </summary>
        None,
        /// <summary>
        ///   <para>The job was canceled by the user.</para>
        /// </summary>
        CancelByUser,
        /// <summary>
        ///   <para>The job was canceled because the session operation took too long to complete.</para>
        /// </summary>
        Timeout,
        /// <summary>
        ///   <para>The job was canceled as a result of a resource failure.</para>
        /// </summary>
        ResourceFailure,
        /// <summary>
        ///   <para>The job was canceled because it was preempted by a higher priority job.</para>
        /// </summary>
        Preemption,
        /// <summary>
        ///   <para>The cancel request finished.</para>
        /// </summary>
        Finish,
        /// <summary>
        ///   <para>The task was canceled when the user requeued it. 
        /// This value was introduced in Windows HPC Server 2008 R2 and is not supported in previous versions.</para>
        /// </summary>
        TaskCancelOnRequeue,
        /// <summary>
        ///   <para>The user canceled the job and specified that the job should 
        /// stop immediately. This value was introduced in Windows HPC Server 2008 R2 and is not supported in previous versions.</para>
        /// </summary>
        CancelForceByUser,
        /// <summary>
        ///   <para>The job was canceled because the job could not use the 
        /// node. This value was introduced in Windows HPC Server 2008 R2 and is not supported in previous versions.</para>
        /// </summary>
        NodeNotUsableByJob,
        /// <summary>
        ///   <para>The job was canceled because the parent job was canceled. 
        /// This value was introduced in Windows HPC Server 2008 R2 and is not supported in previous versions.</para>
        /// </summary>
        ParentJobsDeleted,
        /// <summary>
        ///   <para />
        /// </summary>
        FinishGraceful, // Finish the job, and wait for all its running tasks end.
                        /// <summary>
                        ///   <para />
                        /// </summary>
        CancelGraceful, // Cancel the job, and wait for all its running tasks end.
    }

    /// <summary>
    ///   <para>Defines the reasons for why a job can fail.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code>const None = 0
    /// const ExecutionFailure = 1
    /// const ResourceFailure = 2
    /// const Preempted = 3</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.FailureReason" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.FailureReason" />
    public enum FailureReason
    {
        /// <summary>
        ///   <para>The job did not fail. This enumeration member represents a value of 0.</para>
        /// </summary>
        None,
        /// <summary>
        ///   <para>A task in the job does not exist or could not start. This enumeration member represents a value of 1.</para>
        /// </summary>
        ExecutionFailure,
        /// <summary>
        ///   <para>The node failed. This enumeration member represents a value of 2.</para>
        /// </summary>
        ResourceFailure,
        /// <summary>
        ///   <para>The job was preempted by a higher priority job. This enumeration member represents a value of 3.</para>
        /// </summary>
        Preempted,
    }

    /// <summary>
    ///   <para />
    /// </summary>
    public enum ShrinkRequest
    {
        /// <summary>
        ///   <para />
        /// </summary>
        None,
        /// <summary>
        ///   <para />
        /// </summary>
        NodeGroupChange,
        /// <summary>
        ///   <para />
        /// </summary>
        Preemption,
    }


    /// <summary>
    ///   <para>Defines the types of jobs that can run in the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const Batch = 1
    /// const Admin = 2
    /// const Service = 4
    /// const Broker = 8</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.JobType" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.PropId" />
    [Flags]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidJobType)]
    public enum JobType
    {
        /// <summary>
        ///   <para>A normally scheduled job (see 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateJob" />). This enumeration member represents a value of 1.</para>
        /// </summary>
        Batch = 0x1,
        /// <summary>
        ///   <para>A job that contains commands that run immediately (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateCommand(System.String,Microsoft.Hpc.Scheduler.ICommandInfo,Microsoft.Hpc.Scheduler.IStringCollection)" />). This enumeration member represents a value of 2.</para> 
        /// </summary>
        Admin = 0x2,
        /// <summary>
        ///   <para>A service (session) job. This enumeration member represents a value of 4.</para>
        /// </summary>
        Service = 0x4,
        /// <summary>
        ///   <para>A broker job that routes requests from a WCF client to a service (session) job. This enumeration member represents a value of 8.</para>
        /// </summary>
        Broker = 0x8,
    }

    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(false)]
    [Flags]
    public enum JobRuntimeType
    {
        /// <summary>
        ///   <para />
        /// </summary>
        MPI = 0x1,
        /// <summary>
        ///   <para />
        /// </summary>
        LinqToHPC = 0x2,
        /// <summary>
        ///   <para />
        /// </summary>
        SOA = 0x4,
        /// <summary>
        ///   <para />
        /// </summary>
        Parametric = 0x8,
        /// <summary>
        ///   <para />
        /// </summary>
        All = MPI | LinqToHPC | SOA | Parametric,
    }

    /// <summary>
    ///   <para>Specifies the operator for the NodeGroups list.</para>
    /// </summary>
    [ComVisible(true)]
    [Serializable]
    [GuidAttribute(ComGuids.GuidJobNodeGroupOp)]
    public enum JobNodeGroupOp
    {
        /// <summary>
        ///   <para>Nodes belong to all of the specified node groups. This is the default.</para>
        /// </summary>
        Intersect = 0,
        /// <summary>
        ///   <para>Nodes belong to only one of the specified node groups.</para>
        /// </summary>
        Uniform = 1,
        /// <summary>
        ///   <para>Nodes belong to any of the specified node groups.</para>
        /// </summary>
        Union = 2,
        /// <summary>
        ///   <para>No NodeGroup Operation, also used to indicate there is Task Requested Node Group</para>
        /// </summary>
        NA = 3,
    }

    /// <summary>
    ///   <para>Defines the identifiers that uniquely identify the properties of a job.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Use these identifiers when creating filters, specifying sort 
    /// orders, and using rowsets to retrieve specific properties from the database.</para>
    /// </remarks>
    /// <example>
    ///   <para>The following example shows how to use the property identifiers with a rowset enumerator to 
    /// retrieve all the properties for a specific job. For an alternative way of accessing the property value, see  
    /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.Error" />.</para>
    ///   <code>using System;
    /// using System.Collections.Generic;
    /// using System.Linq;
    /// using System.Text;
    /// using Microsoft.Hpc.Scheduler;
    /// using Microsoft.Hpc.Scheduler.Properties;
    /// 
    /// namespace AccessJobPropertyIds
    /// {
    ///     class Program
    ///     {
    ///         static void Main(string[] args)
    ///         {
    /// IScheduler scheduler = new Scheduler();
    /// ISchedulerRowEnumerator rows = null;
    /// IPropertyIdCollection properties = new PropertyIdCollection();
    /// IFilterCollection filters = scheduler.CreateFilterCollection();
    /// StoreProperty property = null;
    /// 
    /// // Change the following line to specify the head node for your
    /// // HPC cluster.
    /// string headNodeName = "MyHeadNode";
    /// 
    /// // Change the following line to specify the identifier of a job 
    /// // that exists for your HPC cluster.
    /// int jobId = 127;
    /// 
    /// 
    /// scheduler.Connect(headNodeName);
    /// 
    /// properties.Add(JobPropertyIds.Id);
    /// properties.Add(JobPropertyIds.AllocatedCores);
    /// properties.Add(JobPropertyIds.AllocatedNodes);
    /// properties.Add(JobPropertyIds.AllocatedSockets);
    /// properties.Add(JobPropertyIds.AskedNodes);
    /// properties.Add(JobPropertyIds.AutoCalculateMax);
    /// properties.Add(JobPropertyIds.AutoCalculateMin);
    /// properties.Add(JobPropertyIds.AutoRequeueCount);
    /// properties.Add(JobPropertyIds.CallDuration);
    /// properties.Add(JobPropertyIds.CallsPerSecond);
    /// properties.Add(JobPropertyIds.CanceledTaskCount);
    /// properties.Add(JobPropertyIds.CancelingTaskCount);
    /// properties.Add(JobPropertyIds.CanGrow);
    /// properties.Add(JobPropertyIds.CanShrink);
    /// properties.Add(JobPropertyIds.ChangeTime);
    /// properties.Add(JobPropertyIds.ClientSource);
    /// properties.Add(JobPropertyIds.ClientSubSource);
    /// properties.Add(JobPropertyIds.ComputedMaxCores);
    /// properties.Add(JobPropertyIds.ComputedMaxNodes);
    /// properties.Add(JobPropertyIds.ComputedMaxSockets);
    /// properties.Add(JobPropertyIds.ComputedMinCores);
    /// properties.Add(JobPropertyIds.ComputedMinNodes);
    /// properties.Add(JobPropertyIds.ComputedMinNodes);
    /// properties.Add(JobPropertyIds.ComputedMinSockets);
    /// properties.Add(JobPropertyIds.ComputedNodeList);
    /// properties.Add(JobPropertyIds.ConfiguringTaskCount);
    /// properties.Add(JobPropertyIds.CreateTime);
    /// properties.Add(JobPropertyIds.CurrentCoreCount);
    /// properties.Add(JobPropertyIds.CurrentNodeCount);
    /// properties.Add(JobPropertyIds.CurrentSocketCount);
    /// properties.Add(JobPropertyIds.EndpointReference);
    /// properties.Add(JobPropertyIds.EndTime);
    /// properties.Add(JobPropertyIds.ErrorCode);
    /// properties.Add(JobPropertyIds.ErrorMessage);
    /// properties.Add(JobPropertyIds.ErrorParams);
    /// properties.Add(JobPropertyIds.FailedTaskCount);
    /// properties.Add(JobPropertyIds.FailedTaskCount);
    /// properties.Add(JobPropertyIds.FailOnTaskFailure);
    /// properties.Add(JobPropertyIds.FailureReason);
    /// properties.Add(JobPropertyIds.FinishedTaskCount);
    /// properties.Add(JobPropertyIds.HasGrown);
    /// properties.Add(JobPropertyIds.HasRuntime);
    /// properties.Add(JobPropertyIds.HasShrunk);
    /// properties.Add(JobPropertyIds.IsBackfill);
    /// properties.Add(JobPropertyIds.IsExclusive);
    /// properties.Add(JobPropertyIds.JobTemplate);
    /// properties.Add(JobPropertyIds.JobType);
    /// properties.Add(JobPropertyIds.MaxCores);
    /// properties.Add(JobPropertyIds.MaxMemory);
    /// properties.Add(JobPropertyIds.MaxNodes);
    /// properties.Add(JobPropertyIds.MaxCoresPerNode);
    /// properties.Add(JobPropertyIds.MaxSockets);
    /// properties.Add(JobPropertyIds.MinCores);
    /// properties.Add(JobPropertyIds.MinMaxUpdateTime);
    /// properties.Add(JobPropertyIds.MinMemory);
    /// properties.Add(JobPropertyIds.MinNodes);
    /// properties.Add(JobPropertyIds.MinCoresPerNode);
    /// properties.Add(JobPropertyIds.MinSockets);
    /// properties.Add(JobPropertyIds.Name);
    /// properties.Add(JobPropertyIds.NextJobTaskId);
    /// properties.Add(JobPropertyIds.NodeGroups);
    /// properties.Add(JobPropertyIds.NumberOfCalls);
    /// properties.Add(JobPropertyIds.NumberOfOutstandingCalls);
    /// properties.Add(JobPropertyIds.OrderBy);
    /// properties.Add(JobPropertyIds.Owner);
    /// properties.Add(JobPropertyIds.PendingReason);
    /// properties.Add(JobPropertyIds.Preemptable);
    /// properties.Add(JobPropertyIds.PreviousState);
    /// properties.Add(JobPropertyIds.Priority);
    /// properties.Add(JobPropertyIds.ProcessIds);
    /// properties.Add(JobPropertyIds.Project);
    /// properties.Add(JobPropertyIds.QueuedTaskCount);
    /// properties.Add(JobPropertyIds.RequestCancel);
    /// properties.Add(JobPropertyIds.RequestedNodes);
    /// properties.Add(JobPropertyIds.RequeueCount);
    /// properties.Add(JobPropertyIds.RequiredNodes);
    /// properties.Add(JobPropertyIds.RunningTaskCount);
    /// properties.Add(JobPropertyIds.RuntimeSeconds);
    /// properties.Add(JobPropertyIds.RunUntilCanceled);
    /// properties.Add(JobPropertyIds.ServiceName);
    /// properties.Add(JobPropertyIds.SoftwareLicense);
    /// properties.Add(JobPropertyIds.StartTime);
    /// properties.Add(JobPropertyIds.State);
    /// properties.Add(JobPropertyIds.SubmittedTaskCount);
    /// properties.Add(JobPropertyIds.SubmitTime);
    /// properties.Add(JobPropertyIds.TaskCount);
    /// properties.Add(JobPropertyIds.TotalCpuTime);
    /// properties.Add(JobPropertyIds.TotalKernelTime);
    /// properties.Add(JobPropertyIds.TotalUserTime);
    /// properties.Add(JobPropertyIds.UnitType);
    /// properties.Add(JobPropertyIds.UserName);
    /// properties.Add(JobPropertyIds.ValidatingTaskCount);
    /// properties.Add(JobPropertyIds.WaitTime);
    /// 
    /// // The following lines of code are specific to 
    /// // Windows HPC Server 2008 R2. To use this example with 
    /// // Windows HPC Server 2008, remove the lines between here and 
    /// // the next comment, or place the lines within comments.
    /// properties.Add(JobPropertyIds.DispatchingTaskCount);
    /// properties.Add(JobPropertyIds.ExcludedNodes);
    /// properties.Add(JobPropertyIds.ExpandedPriority);
    /// properties.Add(JobPropertyIds.FinishingTaskCount);
    /// properties.Add(JobPropertyIds.HoldUntil);
    /// properties.Add(JobPropertyIds.NotifyOnCompletion);
    /// properties.Add(JobPropertyIds.NotifyOnStart);
    /// properties.Add(JobPropertyIds.OwnerSID);
    /// properties.Add(JobPropertyIds.Progress);
    /// properties.Add(JobPropertyIds.ProgressMessage);
    /// properties.Add(JobPropertyIds.TargetResourceCount);
    /// properties.Add(JobPropertyIds.UserSID);
    /// // End of code specific to Windows HPC Server 2008 R2.
    /// 
    /// // The following line of code is specific to 
    /// // Windows HPC Server 2008 R2 with Service Pack 1 (SP1). To use 
    /// // this example with Windows HPC Server 2008 or 
    /// // Windows HPC Server 2008 R2, remove the line between here and 
    /// // the next comment, or place the line within comments.
    /// properties.Add(JobPropertyIds.UserSID);
    /// // End of code specific to Windows HPC Server 2008 R2 with SP1.
    /// 
    /// filters.Add(FilterOperator.Equal, JobPropertyIds.Id, jobId);
    /// 
    /// using (rows = scheduler.OpenJobEnumerator(properties, filters, null))
    /// {
    ///     PropertyRow row = rows.GetRows(1).Rows[0];
    /// 
    ///     Console.WriteLine("Job " + (int)row[JobPropertyIds.Id].Value);
    /// 
    ///     // Key is the node name; value is the number of cores on the node allocated to the job.
    ///     Console.WriteLine("AllocatedCores: ");
    ///     foreach (KeyValuePair&lt;string, int&gt; node in (List&lt;KeyValuePair&lt;string, int&gt;&gt;)row[JobPropertyIds.AllocatedCores].Value)
    ///         Console.Write("{0}({1})", node.Key, node.Value);
    ///     Console.WriteLine();
    /// 
    ///     // Key is the node name; value is always 1.
    ///     Console.WriteLine("AllocatedNodes: ");
    ///     foreach (KeyValuePair&lt;string, int&gt; node in (List&lt;KeyValuePair&lt;string, int&gt;&gt;)row[JobPropertyIds.AllocatedNodes].Value)
    ///         Console.Write(node.Key + " ");
    ///     Console.WriteLine();
    /// 
    ///     // Key is the node name; value is the number of sockets on the node allocated to the job.
    ///     Console.WriteLine("AllocatedSockets: ");
    ///     foreach (KeyValuePair&lt;string, int&gt; node in (List&lt;KeyValuePair&lt;string, int&gt;&gt;)row[JobPropertyIds.AllocatedSockets].Value)
    ///         Console.Write("{0}({1})", node.Key, node.Value);
    ///     Console.WriteLine();
    /// 
    ///     property = row[JobPropertyIds.AskedNodes];
    ///     Console.WriteLine("AskedNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.AutoCalculateMax];
    ///     Console.WriteLine("AutoCalculateMax: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.AutoCalculateMin];
    ///     Console.WriteLine("AutoCalculateMin: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.AutoRequeueCount];
    ///     Console.WriteLine("AutoRequeueCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.CallDuration];
    ///     Console.WriteLine("CallDuration: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.CallsPerSecond];
    ///     Console.WriteLine("CallsPerSecond: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.CanceledTaskCount];
    ///     Console.WriteLine("CanceledTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.CancelingTaskCount];
    ///     Console.WriteLine("CancelingTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.CanGrow];
    ///     Console.WriteLine("CanGrow: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.CanShrink];
    ///     Console.WriteLine("CanShrink: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ChangeTime];
    ///     Console.WriteLine("ChangeTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ClientSource];
    ///     Console.WriteLine("ClientSource: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ClientSubSource];
    ///     Console.WriteLine("ClientSubSource: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ComputedMaxCores];
    ///     Console.WriteLine("ComputedMaxCores: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ComputedMaxNodes];
    ///     Console.WriteLine("ComputedMaxNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ComputedMaxSockets];
    ///     Console.WriteLine("ComputedMaxSockets: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ComputedMinCores];
    ///     Console.WriteLine("ComputedMinCores: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ComputedMinNodes];
    ///     Console.WriteLine("ComputedMinNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ComputedMinSockets];
    ///     Console.WriteLine("ComputedMinSockets: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ComputedNodeList];
    ///     Console.WriteLine("ComputedNodeList: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ConfiguringTaskCount];
    ///     Console.WriteLine("ConfiguringTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.CreateTime];
    ///     Console.WriteLine("CreateTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.CurrentCoreCount];
    ///     Console.WriteLine("CurrentCoreCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.EndpointReference];
    ///     Console.WriteLine("EndpointReference: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.EndTime];
    ///     Console.WriteLine("EndTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ErrorCode];
    ///     Console.WriteLine("ErrorCode: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ErrorMessage];
    ///     Console.WriteLine("ErrorMessage: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ErrorParams];
    ///     Console.WriteLine("ErrorParams: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.FailedTaskCount];
    ///     Console.WriteLine("FailedTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.FailOnTaskFailure];
    ///     Console.WriteLine("FailOnTaskFailure: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.FailureReason];
    ///     Console.WriteLine("FailureReason: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.FinishedTaskCount];
    ///     Console.WriteLine("FinishedTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.HasGrown];
    ///     Console.WriteLine("HasGrown: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.HasRuntime];
    ///     Console.WriteLine("HasRuntime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.HasShrunk];
    ///     Console.WriteLine("HasShrunk: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.IsBackfill];
    ///     Console.WriteLine("IsBackfill: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.IsExclusive];
    ///     Console.WriteLine("IsExclusive: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.JobTemplate];
    ///     Console.WriteLine("JobTemplate: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.JobType];
    ///     Console.WriteLine("JobType: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MaxCores];
    ///     Console.WriteLine("MaxCores: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MaxMemory];
    ///     Console.WriteLine("MaxMemory: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MaxNodes];
    ///     Console.WriteLine("MaxNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MaxCoresPerNode];
    ///     Console.WriteLine("MaxCoresPerNode: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MaxSockets];
    ///     Console.WriteLine("MaxSockets: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MinCores];
    ///     Console.WriteLine("MinCores: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MinMaxUpdateTime];
    ///     Console.WriteLine("MinMaxUpdateTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MinMemory];
    ///     Console.WriteLine("MinMemory: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MinNodes];
    ///     Console.WriteLine("MinNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MinCoresPerNode];
    ///     Console.WriteLine("MinCoresPerNode: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.MinSockets];
    ///     Console.WriteLine("MinSockets: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.Name];
    ///     Console.WriteLine("Name: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.NextJobTaskId];
    ///     Console.WriteLine("NextJobTaskId: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.NodeGroups];
    ///     Console.WriteLine("NodeGroups: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.NumberOfCalls];
    ///     Console.WriteLine("NumberOfCalls: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.NumberOfOutstandingCalls];
    ///     Console.WriteLine("NumberOfOutstandingCalls: {0}", (null != property) ? property.Value : "");
    /// 
    ///     Console.WriteLine("OrderBy: ");
    ///     foreach (JobOrderBy orderBy in (JobOrderByList)row[JobPropertyIds.OrderBy].Value)
    ///     {
    ///         Console.WriteLine("count({0}), order({1}), property({2}) ", orderBy.Count, orderBy.Order, orderBy.Property);
    ///     }
    /// 
    ///     property = row[JobPropertyIds.Owner];
    ///     Console.WriteLine("Owner: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.PendingReason];
    ///     Console.WriteLine("PendingReason: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.Preemptable];
    ///     Console.WriteLine("Preemptable: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.PreviousState];
    ///     Console.WriteLine("PreviousState: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.Priority];
    ///     Console.WriteLine("Priority: {0}", (null != property) ? property.Value : "");
    /// 
    ///     Console.WriteLine("ProcessIds: ");
    ///     foreach (KeyValuePair&lt;string, string&gt; id in (Dictionary&lt;string, string&gt;)row[JobPropertyIds.ProcessIds].Value)
    ///         Console.Write("{0}({1})", id.Key, id.Value);
    ///     Console.WriteLine();
    /// 
    ///     property = row[JobPropertyIds.Project];
    ///     Console.WriteLine("Project: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.QueuedTaskCount];
    ///     Console.WriteLine("QueuedTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.RequestCancel];
    ///     Console.WriteLine("RequestCancel: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.RequestedNodes];
    ///     Console.WriteLine("RequestedNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.RequeueCount];
    ///     Console.WriteLine("RequeueCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.RequiredNodes];
    ///     Console.WriteLine("RequiredNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.RunningTaskCount];
    ///     Console.WriteLine("RunningTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.RuntimeSeconds];
    ///     Console.WriteLine("RuntimeSeconds: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.RunUntilCanceled];
    ///     Console.WriteLine("RunUntilCanceled: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ServiceName];
    ///     Console.WriteLine("ServiceName: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.SoftwareLicense];
    ///     Console.WriteLine("SoftwareLicense: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.StartTime];
    ///     Console.WriteLine("StartTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.State];
    ///     Console.WriteLine("State: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.SubmittedTaskCount];
    ///     Console.WriteLine("SubmittedTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.SubmitTime];
    ///     Console.WriteLine("SubmitTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.TaskCount];
    ///     Console.WriteLine("TaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    /// 
    ///     property = row[JobPropertyIds.TotalCpuTime];
    ///     Console.WriteLine("TotalCpuTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.TotalKernelTime];
    ///     Console.WriteLine("TotalKernelTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.TotalUserTime];
    ///     Console.WriteLine("TotalUserTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.UnitType];
    ///     Console.WriteLine("UnitType: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.UserName];
    ///     Console.WriteLine("UserName: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ValidatingTaskCount];
    ///     Console.WriteLine("ValidatingTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.WaitTime];
    ///     Console.WriteLine("WaitTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     // The following lines of code are specific to 
    ///     // Windows HPC Server 2008 R2. To use this example with 
    ///     // Windows HPC Server 2008, remove the lines between here and 
    ///     // the comment that ends this section, or place the lines 
    ///     // within comments.
    ///     property = row[JobPropertyIds.DispatchingTaskCount];
    ///     Console.WriteLine("DispatchingTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ExcludedNodes];
    ///     Console.WriteLine("ExcludedNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ExpandedPriority];
    ///     Console.WriteLine("ExpandedPriority: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.FinishingTaskCount];
    ///     Console.WriteLine("FinishingTaskCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.HoldUntil];
    ///     Console.WriteLine("HoldUntil: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.NotifyOnCompletion];
    ///     Console.WriteLine("NotifyOnCompletion: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.NotifyOnStart];
    ///     Console.WriteLine("NotifyOnStart: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.OwnerSID];
    ///     Console.WriteLine("OwnerSID: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.Progress];
    ///     Console.WriteLine("Progress: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.ProgressMessage];
    ///     Console.WriteLine("ProgressMessage: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.TargetResourceCount];
    ///     Console.WriteLine("TargetResourceCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[JobPropertyIds.UserSID];
    ///     Console.WriteLine("UserSID: {0}", (null != property) ? property.Value : "");
    ///     // End of code specific to Windows HPC Server 2008 R2.
    /// 
    ///     // The following lines of code are specific to 
    ///     // Windows HPC Server 2008 R2 with SP1. To use this example
    ///     // with Windows HPC Server 2008 or 
    ///     // Windows HPC Server 2008 R2, remove the lines between here 
    ///       // and the next comment, or place the lines within 
    ///       // comments.
    ///       property = row[JobPropertyIds.EmailAddress];
    ///       Console.WriteLine("EmailAddress: {0}", (null != property) ? property.Value : "");
    ///       // End of code specific to Windows HPC Server 2008 R2 with 
    ///       // SP1.
    /// 
    /// }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Microsoft.Hpc.Scheduler.IFilterCollection.Add(Microsoft.Hpc.Scheduler.Properties.FilterOperator,Microsoft.Hpc.Scheduler.Properties.PropertyId,System.Object)" 
    /// /> 
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISortCollection.Add(Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder,Microsoft.Hpc.Scheduler.Properties.PropertyId)" 
    /// /> 
    [Serializable]
    public class JobPropertyIds
    {
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" /> class.</para>
        /// </summary>
        protected JobPropertyIds()
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
            get { return StorePropertyIds.NA; }
        }

        /// <summary>
        ///   <para>The identifier that uniquely identifies the job in the scheduler.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Id" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
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
        /// <example>
        ///   <para>The following example shows how you use this property. You must use a 
        /// zero-based index to index the property instead of using the property identifier to index the property.</para>
        ///   <code>
        ///     // Assumes that the zero-based index for the 
        ///     // MaxMemory property for this query is four. The index is 
        ///     // based on the position in which you insert the property 
        ///     // identifier into your IPropertyIdCollection collection.
        ///     property = row[JobPropertyIds.MaxMemroy];
        ///     if (null == property)
        ///         Console.WriteLine("Output: " + row[4].Value);
        ///     else
        ///         Console.WriteLine("Output: " + property.Value);
        /// 
        ///     // If you do not need to know that the property was not 
        ///     // retrieved, you can simply use the index to access the 
        ///     // value.
        ///     Console.WriteLine("Output: " + row[4].Value);
        /// </code>
        /// </example>
        public static PropertyId Error
        {
            get { return StorePropertyIds.Error; }
        }

        /// <summary>
        ///   <para>The state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.State" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId State
        {
            get { return _State; }
        }

        /// <summary>
        ///   <para>The previous state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.PreviousState" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId PreviousState
        {
            get { return _PreviousState; }
        }

        /// <summary>
        ///   <para>The cores that were allocated to the job the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the job is running, it lists the cores that have been allocated since the job began running. 
        /// The list of cores does not shrink even if the server shrinks the cores that it allocates to your job. </para>
        ///   <para>The property is a collection of 
        /// <see cref="System.Collections.Generic.KeyValuePair`2" /> objects. The key is the node name and the value is the number of cores.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        static public PropertyId AllocatedCores
        {
            get { return StorePropertyIds.AllocatedCores; }
        }

        /// <summary>
        ///   <para>The sockets that were allocated to the job the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the job is running, it lists the sockets that have been allocated since the job began running. 
        /// The list of sockets does not shrink even if the server shrinks the sockets that it allocates to your job. </para>
        ///   <para>The property is a collection of 
        /// <see cref="System.Collections.Generic.KeyValuePair`2" /> objects. The key is the node name and the value is the number of sockets.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        static public PropertyId AllocatedSockets
        {
            get { return StorePropertyIds.AllocatedSockets; }
        }

        /// <summary>
        ///   <para>The nodes that were allocated to the job the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the job is running, it lists the nodes that have been allocated since the job began running. 
        /// The list of nodes does not shrink even if the server shrinks the nodes that it allocates to your job. </para>
        ///   <para>The property is a collection of 
        /// <see cref="System.Collections.Generic.KeyValuePair`2" /> objects. The key is the node name and the value is the number of nodes (always 1).</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        static public PropertyId AllocatedNodes
        {
            get { return StorePropertyIds.AllocatedNodes; }
        }

        /// <summary>
        ///   <para>The date and time that the job was submitted to the scheduling queue.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId SubmitTime
        {
            get { return StorePropertyIds.SubmitTime; }
        }

        /// <summary>
        ///   <para>The date and time that the job was created.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CreateTime
        {
            get { return StorePropertyIds.CreateTime; }
        }

        /// <summary>
        ///   <para>The date and time that the job started running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.StartTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId StartTime
        {
            get { return StorePropertyIds.StartTime; }
        }

        /// <summary>
        ///   <para>The date and time that the job finished, failed or was canceled. </para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EndTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId EndTime
        {
            get { return StorePropertyIds.EndTime; }
        }

        /// <summary>
        ///   <para>The date and time that the job was last touched.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ChangeTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ChangeTime
        {
            get { return StorePropertyIds.ChangeTime; }
        }

        /// <summary>
        ///   <para>The SQL time stamp that identifies when the row was last changed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Timestamp
        {
            get { return StorePropertyIds.Timestamp; }
        }

        /// <summary>
        ///   <para>The display name of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Name" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Name
        {
            get { return StorePropertyIds.Name; }
        }

        /// <summary>
        ///   <para>Determines whether nodes should be exclusively allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.IsExclusive" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId IsExclusive
        {
            get { return StorePropertyIds.IsExclusive; }
        }

        /// <summary>
        ///   <para>Determines whether the job runs until the user cancels it.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId RunUntilCanceled
        {
            get { return StorePropertyIds.RunUntilCanceled; }
        }

        /// <summary>
        ///   <para>Determines whether cores, nodes, or sockets are used to allocate resources for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId UnitType
        {
            get { return StorePropertyIds.UnitType; }
        }

        /// <summary>
        ///   <para>The run-time limit for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId RuntimeSeconds
        {
            get { return StorePropertyIds.RuntimeSeconds; }
        }

        /// <summary>
        ///   <para>The name of the user who created, submitted, or queued the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Owner" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Owner
        {
            get { return _Owner; }
        }

        /// <summary>
        ///   <para>The RunAs user for the job</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UserName" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId UserName
        {
            get { return _Username; }
        }

        /// <summary>
        ///   <para>The security identifier (SID) for the user who owns the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is a <see cref="System.String" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId OwnerSID
        {
            get { return _OwnerSID; }
        }

        /// <summary>
        ///   <para>The security identifier (SID) for the user under whose credentials the job should run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is a <see cref="System.String" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId UserSID
        {
            get { return _UserSID; }
        }

        /// <summary>
        ///   <para>The encrypted password of the RunAs user.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId EncryptedPassword
        {
            get { return _EncryptedPassword; }
        }

        /// <summary>
        ///   <para>The password of the RunAs user.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>You cannot retrieve this property.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Password
        {
            get { return _Password; }
        }

        /// <summary>
        ///   <para>The project name associated with the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Project" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Project
        {
            get { return _Project; }
        }

        /// <summary>
        ///   <para>The type of job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobType" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId JobType
        {
            get { return _JobType; }
        }

        /// <summary>
        ///   <para>The template that is applied to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.JobTemplate" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId JobTemplate
        {
            get { return _JobTemplate; }
        }

        /// <summary>
        ///   <para>The priority of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Priority" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Priority
        {
            get { return _Priority; }
        }

        /// <summary>
        ///   <para>The list of nodes on which the job can run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId RequestedNodes
        {
            get { return _RequestedNodes; }
        }

        /// <summary>
        ///   <para>The nodes that you requested for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId AskedNodes
        {
            get { return _RequestedNodes; }
        }

        /// <summary>
        ///   <para>The names of the node groups that defines the nodes on which the job can run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NodeGroups" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId NodeGroups
        {
            get { return _NodeGroups; }
        }

        /// <summary>
        ///   <para>The nodes on which the tasks in the job are required to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is a string that contains a comma-delimited list of node names. The 
        /// list is a union of all the required nodes defined in the tasks of the job (see  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequiredNodes" />).</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId RequiredNodes
        {
            get { return _RequiredNodes; }
        }

        /// <summary>
        ///   <para>Determines whether the job is running as a backfill job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>A backfill job is a lower priority job that runs before a higher priority job. This 
        /// ensures that a resource-intensive application will not delay other applications that are ready to run. The job scheduler will  
        /// schedule a lower priority job if a higher priority job is waiting for resources to become available and 
        /// the lower priority job can finish with the available resources without delaying the start time of the higher priority job.</para> 
        ///   <para>The property type is Boolean.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId IsBackfill
        {
            get { return _IsBackfill; }
        }

        /// <summary>
        ///   <para>The software licenses that must exist on the nodes for the job to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SoftwareLicense" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId SoftwareLicense
        {
            get { return _SoftwareLicense; }
        }

        /// <summary>
        ///   <para>A description of the error that caused the job to fail, or the reason the job was canceled. </para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ErrorMessage" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ErrorMessage
        {
            get { return StorePropertyIds.ErrorMessage; }
        }

        /// <summary>
        ///   <para>An error code that identifies an error message string.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.ErrorCode" /> enumeration.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ErrorCode
        {
            get { return StorePropertyIds.ErrorCode; }
        }

        /// <summary>
        ///   <para>The insert strings that are applied to the error message string.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is string. The format of the string is 
        /// a delimited list of insert strings. The delimiter is three vertical bars (string1|||string2).</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ErrorParams
        {
            get { return StorePropertyIds.ErrorParams; }
        }

        /// <summary>
        ///   <para>The minimum number of cores that the job requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MinCores
        {
            get { return StorePropertyIds.MinCores; }
        }

        /// <summary>
        ///   <para>The maximum number of cores that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MaxCores
        {
            get { return StorePropertyIds.MaxCores; }
        }

        /// <summary>
        ///   <para>The minimum number of sockets that the job requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MinSockets
        {
            get { return StorePropertyIds.MinSockets; }
        }

        /// <summary>
        ///   <para>The maximum number of sockets that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MaxSockets
        {
            get { return StorePropertyIds.MaxSockets; }
        }

        /// <summary>
        ///   <para>The minimum number of nodes that the job requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MinNodes
        {
            get { return StorePropertyIds.MinNodes; }
        }

        /// <summary>
        ///   <para>The maximum number of nodes that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MaxNodes
        {
            get { return StorePropertyIds.MaxNodes; }
        }

        /// <summary>
        ///   <para>The number of nodes allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is an integer. The property has meaning only if the job is running.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CurrentNodeCount
        {
            get { return StorePropertyIds.TotalNodeCount; }
        }

        /// <summary>
        ///   <para>The number of sockets allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is an integer. The property has meaning only if the job is running.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CurrentSocketCount
        {
            get { return StorePropertyIds.TotalSocketCount; }
        }

        /// <summary>
        ///   <para>The number of cores allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is an integer. The property has meaning only if the job is running.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CurrentCoreCount
        {
            get { return StorePropertyIds.TotalCoreCount; }
        }

        /// <summary>
        ///   <para>The sequential task identifier that will be given to the next task that is added to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The identifiers begin with one.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId NextJobTaskId
        {
            get { return _NextTaskNiceID; }
        }

        /// <summary>
        ///   <para>Indicates whether the number of resources allocated to the job has grown since resources were last allocated.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId HasGrown
        {
            get { return _HasGrown; }
        }

        /// <summary>
        ///   <para>Indicates whether the number of resources allocated to the job has shrunk since resources were last allocated.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId HasShrunk
        {
            get { return _HasShrunk; }
        }

        /// <summary>
        ///   <para>The number of tasks in the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.TaskCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId TaskCount
        {
            get { return StorePropertyIds.TotalTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks in the job that are being configured and have not yet been added to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.ConfiguringTaskCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ConfiguringTaskCount
        {
            get { return StorePropertyIds.ConfiguringTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that have been submitted to the scheduling queue.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.SubmittedTaskCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId SubmittedTaskCount
        {
            get { return StorePropertyIds.SubmittedTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks being validated.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property has meaning only if the job is running.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ValidatingTaskCount
        {
            get { return StorePropertyIds.ValidatingTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks in the job that are queued to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.QueuedTaskCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId QueuedTaskCount
        {
            get { return StorePropertyIds.QueuedTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks in the job that the HPC Job Scheduler Service is sending to a node to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId DispatchingTaskCount
        {
            get { return StorePropertyIds.DispatchingTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.RunningTaskCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId RunningTaskCount
        {
            get { return StorePropertyIds.RunningTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that finished running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.FinishedTaskCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId FinishedTaskCount
        {
            get { return StorePropertyIds.FinishedTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks in the job that failed the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the job is running, this is the number of tasks that have failed since the job began running. See 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.FailedTaskCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId FailedTaskCount
        {
            get { return StorePropertyIds.FailedTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that were canceled the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.CanceledTaskCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CanceledTaskCount
        {
            get { return StorePropertyIds.CanceledTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks that are being canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.CancelingTaskCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CancelingTaskCount
        {
            get { return StorePropertyIds.CancelingTaskCount; }
        }

        /// <summary>
        ///   <para>The number of tasks in the job for which the node is cleaning up the resources that were allocated to the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId FinishingTaskCount
        {
            get { return StorePropertyIds.FinishingTaskCount; }
        }

        /// <summary>
        ///   <para>The total CPU time that the job used.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.TotalCpuTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId TotalCpuTime
        {
            get { return StorePropertyIds.TotalCpuTime; }
        }

        /// <summary>
        ///   <para>The elapsed execution time for user-mode instructions (the total time that the job spent in user-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.TotalUserTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId TotalUserTime
        {
            get { return StorePropertyIds.TotalUserTime; }
        }

        /// <summary>
        ///   <para>The elapsed execution time for kernel-mode instructions (the total time that the job spent in kernel-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters.TotalKernelTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId TotalKernelTime
        {
            get { return StorePropertyIds.TotalKernelTime; }
        }

        /// <summary>
        ///   <para>Determines whether the job resources can grow.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CanGrow
        {
            get { return _CanGrow; }
        }

        /// <summary>
        ///   <para>Determines if the job resources can shrink.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CanShrink
        {
            get { return _CanShrink; }
        }

        /// <summary>
        ///   <para>The minimum number of cores that the server has currently allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>When you create the job, you specify the minimum and maximum 
        /// cores that the job requires. When the server schedules your job, it calculates,  
        /// within the specified bounds, the minimum number of cores that would best support 
        /// your job given the available resources. The number can change as the job runs.</para> 
        ///   <para>This property is an integer.</para>
        ///   <para>This property has meaning only if <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> is Core.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ComputedMinCores
        {
            get { return _ComputedMinCores; }
        }

        /// <summary>
        ///   <para>The maximum number of cores that the server has currently allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>When you create the job, you specify the minimum and maximum 
        /// cores that the job requires. When the server schedules your job, it calculates,  
        /// within the specified bounds, the maximum number of cores that would best support 
        /// your job given the available resources. The number can change as the job runs.</para> 
        ///   <para>This property is an integer.</para>
        ///   <para>This property has meaning only if <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> is Core.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ComputedMaxCores
        {
            get { return _ComputedMaxCores; }
        }

        /// <summary>
        ///   <para>The minimum number of sockets that the server has currently allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>When you create the job, you specify the minimum and maximum 
        /// sockets that the job requires. When the server schedules your job, it calculates,  
        /// within the specified bounds, the minimum number of sockets that would best support 
        /// your job given the available resources. The number can change as the job runs.</para> 
        ///   <para>This property is an integer.</para>
        ///   <para>This property has meaning only if <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> is Socket.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ComputedMinSockets
        {
            get { return _ComputedMinSockets; }
        }

        /// <summary>
        ///   <para>The maximum number of sockets that the server has currently allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>When you create the job, you specify the minimum and maximum 
        /// sockets that the job requires. When the server schedules your job, it calculates,  
        /// within the specified bounds, the maximum number of sockets that would best support 
        /// your job given the available resources. The number can change as the job runs.</para> 
        ///   <para>This property is an integer.</para>
        ///   <para>This property has meaning only if <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> is Socket.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ComputedMaxSockets
        {
            get { return _ComputedMaxSockets; }
        }

        /// <summary>
        ///   <para>The minimum number of nodes that the server has currently allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>When you create the job, you specify the minimum and maximum 
        /// nodes that the job requires. When the server schedules your job, it calculates,  
        /// within the specified bounds, the minimum number of nodes that would best support 
        /// your job given the available resources. The number can change as the job runs.</para> 
        ///   <para>This property is an integer.</para>
        ///   <para>This property has meaning only if <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> is Node.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ComputedMinNodes
        {
            get { return _ComputedMinNodes; }
        }

        /// <summary>
        ///   <para>The maximum number of nodes that the server has currently allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>When you create the job, you specify the minimum and maximum 
        /// nodes that the job requires. When the server schedules your job, it calculates,  
        /// within the specified bounds, the maximum number of nodes that would best support 
        /// your job given the available resources. The number can change as the job runs.</para> 
        ///   <para>This property is an integer.</para>
        ///   <para>This property has meaning only if <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> is Node.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ComputedMaxNodes
        {
            get { return _ComputedMaxNodes; }
        }

        /// <summary>
        ///   <para>How long the job has been or is waiting to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>Determined by subtracting the submit time from the current time, or subtracting the submit time from the start time.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId WaitTime
        {
            get { return _WaitTime; }
        }

        /// <summary>
        ///   <para>The preference given to criteria used to choose the nodes that are allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details of this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OrderBy" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId OrderBy
        {
            get { return _OrderBy; }
        }

        /// <summary>
        ///   <para>The last time that the Scheduler computed the maximum and minimum resource values.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId TaskLevelUpdateTime
        {
            get { return _TaskLevelUpdateTime; }
        }

        /// <summary>
        ///   <para>The last time that the server checked the computed minimum and maximum resource values for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MinMaxUpdateTime
        {
            get { return _MinMaxUpdateTime; }
        }

        /// <summary>
        ///   <para>Indicates that a cancel request is pending for the job. </para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is Boolean. The property has meaning only if the job is running.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId RequestCancel
        {
            get { return _RequestCancel; }
        }

        /// <summary>
        ///   <para>The number of times that the job has been queued again.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequeueCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        static public PropertyId RequeueCount
        {
            get { return _RequeueCount; }
        }

        /// <summary>
        ///   <para>The number of times that the system reran the job when a system error occurred.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId AutoRequeueCount
        {
            get { return _AutoRequeueCount; }
        }

        /// <summary>
        ///   <para>The reason the task failed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.FailureReason" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        static public PropertyId FailureReason
        {
            get { return _FailureReason; }
        }

        /// <summary>
        ///   <para>The endpoint references that a SOA-based client can connect to.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a string of one or more endpoint references. A semicolon delimits the endpoints.</para>
        ///   <para>This property applies to only broker jobs. See <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EndpointAddresses" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId EndpointReference
        {
            get { return _EndpointReference; }
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
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ServiceName
        {
            get { return _ServiceName; }
        }

        /// <summary>
        ///   <para>The reason the job has not yet run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.PendingReason.ReasonCode" /> enumeration.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId PendingReason
        {
            get { return _PendingReason; }
        }

        /// <summary>
        ///   <para>The list of nodes currently allocated to your running job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a string that contains a comma-delimited list of node names.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ComputedNodeList
        {
            get { return _ComputedNodeList; }
        }

        /// <summary>
        ///   <para>Determines whether the server automatically calculates the maximum resource value.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId AutoCalculateMax
        {
            get { return _AutoCalculateMax; }
        }

        /// <summary>
        ///   <para>Determines whether the server automatically calculates the minimum resource value.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId AutoCalculateMin
        {
            get { return _AutoCalculateMin; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the parent job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ParentJobId
        {
            get { return _ParentJobId; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the child job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId ChildJobId
        {
            get { return _ChildJobId; }
        }

        /// <summary>
        ///   <para>The minimum amount of memory that the job requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MinMemory" /> parameter is used by the job scheduler to select nodes to run the job. The job scheduler will select nodes that have amounts of memory that are equal or greater than the value of  
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MinMemory" />.</para>
        ///   <para>For more details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinMemory" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MinMemory
        {
            get { return _MinMemory; }
        }

        /// <summary>
        ///   <para>The maximum amount of memory that a node may have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxMemory" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId MaxMemory
        {
            get { return _MaxMemory; }
        }

        /// <summary>
        ///   <para>The minimum number of cores that must exist on the node for the job to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinCoresPerNode" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId MinCoresPerNode
        {
            get { return _MinProcsPerNode; }
        }

        /// <summary>
        ///   <para>The maximum number of cores that a node can have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxCoresPerNode" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId MaxCoresPerNode
        {
            get { return _MaxProcsPerNode; }
        }

        /// <summary>
        ///   <para>The internal job object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId JobObject
        {
            get { return StorePropertyIds.JobObject; }
        }

        /// <summary>
        ///   <para>The number of web-service calls made in the session.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 64-bit integer. This property applies to only web-service jobs.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId NumberOfCalls
        {
            get { return _NumberOfCalls; }
        }

        /// <summary>
        ///   <para>The number of web-service calls to which the broker as not yet replied to the client.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is a 64-bit integer. This property applies to only web-service jobs.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId NumberOfOutstandingCalls
        {
            get { return _NumberOfOutstandingCalls; }
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
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CallDuration
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
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId CallsPerSecond
        {
            get { return _CallsPerSecond; }
        }

        /// <summary>
        ///   <para>Indicates whether the run-time limit for the job is set.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.HasRuntime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId HasRuntime
        {
            get { return StorePropertyIds.HasRuntime; }
        }

        /// <summary>
        ///   <para>Determines whether the job fails when one of the tasks in the job fails.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.FailOnTaskFailure" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId FailOnTaskFailure
        {
            get { return _FailOnTaskFailure; }
        }

        /// <summary>
        ///   <para>Determines whether the job can be preempted by a higher priority job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanPreempt" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public static PropertyId Preemptable
        {
            get { return _Preemptable; }
        }

        /// <summary>
        ///   <para>The default task group for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId DefaultTaskGroupId
        {
            get { return _DefaultTaskGroupId; }
        }

        /// <summary>
        ///   <para>The name of the process that created the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId ClientSource
        {
            get { return _Source; }
        }

        /// <summary>
        ///   <para>Indicates whether the source was an XML file, executable, DLL, or a script.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId ClientSubSource
        {
            get { return _SubSource; }
        }

        /// <summary>
        ///   <para>The percentage of the job that is complete.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is a <see cref="System.Int32" /> between 0 and 100.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId Progress
        {
            get { return _Progress; }
        }

        /// <summary>
        ///   <para>A custom status message for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.String" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId ProgressMessage
        {
            get { return _ProgressMessage; }
        }

        /// <summary>
        ///   <para>A comma-delimited list of the process identifiers associated with the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ProcessIds
        {
            get { return StorePropertyIds.ProcessIds; }
        }

        /// <summary>
        ///   <para>The dynamically set maximum number of resources that a job can use, so 
        /// that the HPC Job Scheduler Service does not allocate more resources than the job can use.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is a <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId TargetResourceCount
        {
            get { return _targetResourceCount; }
        }

        /// <summary>
        ///   <para>The priority of the job, specified by using the expanded 
        /// range of priority values in Windows HPC Server 2008 R2 and is not supported in previous versions.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is 
        /// <see cref="System.Int32" />. For more information about this property, see 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" />
        public static PropertyId ExpandedPriority
        {
            get { return _ExpandedPriority; }
        }

        /// <summary>
        ///   <para>The date and time (in Coordinated Universal Time) until which 
        /// the HPC Job Scheduler Service should wait before trying to start the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.DateTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId HoldUntil
        {
            get { return _holdUntil; }
        }

        /// <summary>
        ///   <para>Whether the job owner wants to receive an email notification when then job starts.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is 
        /// 
        /// <see cref="System.Boolean" />. True indicates that the job owner wants to receive an email notification when then job starts. False indicates that the job owner does not want to receive an email notification when then job starts.</para> 
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId NotifyOnStart
        {
            get { return _NotifyOnStart; }
        }

        /// <summary>
        ///   <para>Whether the job owner wants to receive an email notification when then job ends.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is 
        /// 
        /// <see cref="System.Boolean" />. True indicates that the job owner wants to receive an email notification when then job ends. False indicates that the job owner does not want to receive an email notification when then job ends.</para> 
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId NotifyOnCompletion
        {
            get { return _NotifyOnCompletion; }
        }

        /// <summary>
        ///   <para>The list of nodes that should not be used for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is string that lists the nodes, separated by commas (,).</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId ExcludedNodes
        {
            get { return _ExcludedNodes; }
        }

        /// <summary>
        ///   <para>The email address to which the HPC Job Scheduler Service should send notifications when the job starts or finishes.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is a <see cref="System.String" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EmailAddress" />
        public static PropertyId EmailAddress
        {
            get { return _EmailAddress; }
        }

        /// <summary>
        ///   <para>The property for the name of the pool associated with the current job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Pool
        {
            get { return _Pool; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public static PropertyId RuntimeType
        {
            get { return _RuntimeType; }
        }

        /// <summary>
        ///   <para>Gets or sets the operator for the node group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is a 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp" /> which represents the operator for the node group.</para>
        /// </remarks>
        public static PropertyId NodeGroupOp
        {
            get { return _NodeGroupOp; }
        }

        /// <summary>
        ///   <para>Gets or sets the exit codes to be used for checking whether tasks in the job successfully exit.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is a 
        /// 
        /// <see cref="System.String" /> which contains the list of exit codes that the job can return upon a successful exit. These codes will only apply to tasks that do not specify their own success exit codes. You can specify discrete integers and integer ranges separated by commas.</para> 
        ///   <para>
        ///     min and max may be 
        /// used to specify a range of exit codes. For example, <c>0..max</c> represents nonnegative integers.</para>
        /// </remarks>
        public static PropertyId JobValidExitCodes
        {
            get { return _JobValidExitCodes; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.</para>
        /// </value>
        public static PropertyId SingleNode
        {
            get { return _SingleNode; }
        }

        /// <summary>
        ///   <para>Gets or sets the list of parent job IDs.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is an 
        /// <see cref="Microsoft.Hpc.Scheduler.IIntCollection" /> object which contains the collection of parent job IDs.</para>
        /// </remarks>
        public static PropertyId ParentJobIds
        {
            get { return _ParentJobIds; }
        }


        /// <summary>
        ///   <para>Gets or sets a value indicating whether child tasks should be marked as failed if the current task fails.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is a 
        /// <see cref="System.Boolean" /> which is 
        /// True if child tasks should be marked as failed if the current task fails; otherwise, 
        /// False.</para>
        /// </remarks>
        public static PropertyId FailDependentTasks
        {
            get { return _FailDependentTasks; }
        }

        /// <summary>
        ///   <para>Gets or sets the list of child job IDs.</para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        /// <remarks>
        ///   <para>The property type is a <see cref="Microsoft.Hpc.Scheduler.IIntCollection" /> object which is the collection of child job IDs.</para>
        ///   <para>Child jobs will not begin until all parent jobs have completed.</para>
        /// </remarks>
        public static PropertyId ChildJobIds
        {
            get { return _ChildJobIds; }
        }


        /// <summary>
        ///   <para>Gets or sets the estimate of the maximum amount of memory the job will consume.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is a 
        /// <see cref="System.Int32" /> which represents the estimated number of megabytes of memory the job will require.</para>
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EstimatedProcessMemory" /> property is used by the job scheduler to assign jobs to nodes with sufficient memory to execute the job.</para> 
        /// </remarks>
        public static PropertyId EstimatedProcessMemory
        {
            get { return _EstimatedProcessMemory; }
        }


        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public static PropertyId PlannedCoreCount
        {
            get { return _PlannedCoreCount; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public static PropertyId TaskExecutionFailureRetryLimit
        {
            get { return _TaskExecutionFailureRetryLimit; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public static PropertyId NodePrepareTask
        {
            get { return _NodePrepareTask; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public static PropertyId NodeReleaseTask
        {
            get { return _NodeReleaseTask; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public static readonly string JobGpuCustomPropertyName = "GPUJob";

        // 
        // Private members
        //

        static PropertyId _State = new PropertyId(StorePropertyType.JobState, "State", PropertyIdConstants.JobPropertyIdStart + 1, PropFlags.Visible | PropFlags.Indexed);
        static PropertyId _PreviousState = new PropertyId(StorePropertyType.JobState, "PreviousState", PropertyIdConstants.JobPropertyIdStart + 2, PropFlags.Visible | PropFlags.Indexed);

        static PropertyId _Owner = new PropertyId(StorePropertyType.String, "Owner", PropertyIdConstants.JobPropertyIdStart + 3, PropFlags.Visible | PropFlags.Indexed);
        static PropertyId _Username = new PropertyId(StorePropertyType.String, "UserName", PropertyIdConstants.JobPropertyIdStart + 4, PropFlags.Visible | PropFlags.Indexed);

        //password is encrypted string
        static PropertyId _EncryptedPassword = new PropertyId(StorePropertyType.Binary, "Password", PropertyIdConstants.JobPropertyIdStart + 5);
        static PropertyId _Project = new PropertyId(StorePropertyType.String, "Project", PropertyIdConstants.JobPropertyIdStart + 6, PropFlags.Visible | PropFlags.Indexed);

        static PropertyId _JobType = new PropertyId(StorePropertyType.JobType, "JobType", PropertyIdConstants.JobPropertyIdStart + 7);

        static PropertyId _UserSID = new PropertyId(StorePropertyType.String, "UserSID", PropertyIdConstants.JobPropertyIdStart + 8);

        static PropertyId _OwnerSID = new PropertyId(StorePropertyType.String, "OwnerSID", PropertyIdConstants.JobPropertyIdStart + 9);

        static PropertyId _JobTemplate = new PropertyId(StorePropertyType.String, "JobTemplate", PropertyIdConstants.JobPropertyIdStart + 16, PropFlags.Visible | PropFlags.Indexed);

        static PropertyId _Priority = new PropertyId(StorePropertyType.JobPriority, "Priority", PropertyIdConstants.JobPropertyIdStart + 17, PropFlags.Visible | PropFlags.Indexed);

        static PropertyId _RequestedNodes = new PropertyId(StorePropertyType.StringList, "RequestedNodes", PropertyIdConstants.JobPropertyIdStart + 19);
        static PropertyId _NodeGroups = new PropertyId(StorePropertyType.StringList, "NodeGroups", PropertyIdConstants.JobPropertyIdStart + 20);

        static PropertyId _RequiredNodes = new PropertyId(StorePropertyType.String, "RequiredNodes", PropertyIdConstants.JobPropertyIdStart + 22);

        static PropertyId _IsBackfill = new PropertyId(StorePropertyType.Boolean, "IsBackfill", PropertyIdConstants.JobPropertyIdStart + 23);

        static PropertyId _ErrorMsg = new PropertyId(StorePropertyType.String, "ErrorMsg", PropertyIdConstants.JobPropertyIdStart + 27);

        static PropertyId _SoftwareLicense = new PropertyId(StorePropertyType.StringList, "SoftwareLicense", PropertyIdConstants.JobPropertyIdStart + 28);

        static PropertyId _MaxNodesAllocated = new PropertyId(StorePropertyType.Int32, "MaxNodesAllocated", PropertyIdConstants.JobPropertyIdStart + 32);
        static PropertyId _MaxCoresAllocated = new PropertyId(StorePropertyType.Int32, "MaxCoresAllocated", PropertyIdConstants.JobPropertyIdStart + 33);
        static PropertyId _MaxSocketsAllocated = new PropertyId(StorePropertyType.Int32, "MaxSocketsAllocated", PropertyIdConstants.JobPropertyIdStart + 34);

        static PropertyId _NextTaskNiceID = new PropertyId(StorePropertyType.Int32, "NextTaskNiceID", PropertyIdConstants.JobPropertyIdStart + 38);

        static PropertyId _HasGrown = new PropertyId(StorePropertyType.Boolean, "HasGrown", PropertyIdConstants.JobPropertyIdStart + 46);
        static PropertyId _HasShrunk = new PropertyId(StorePropertyType.Boolean, "HasShrunk", PropertyIdConstants.JobPropertyIdStart + 47);

        static PropertyId _CanGrow = new PropertyId(StorePropertyType.Boolean, "CanGrow", PropertyIdConstants.JobPropertyIdStart + 59);
        static PropertyId _CanShrink = new PropertyId(StorePropertyType.Boolean, "CanShrink", PropertyIdConstants.JobPropertyIdStart + 60);

        static PropertyId _Password = new PropertyId(StorePropertyType.String, "PasswordClear", PropertyIdConstants.JobPropertyIdStart + 61, PropFlags.None);

        static PropertyId _ClientVersion = new PropertyId(StorePropertyType.String, "ClientVersion", PropertyIdConstants.JobPropertyIdStart + 63);

        static PropertyId _WaitTime = new PropertyId(StorePropertyType.Int32, "WaitTime", PropertyIdConstants.JobPropertyIdStart + 64);
        static PropertyId _OrderBy = new PropertyId(StorePropertyType.JobOrderby, "OrderBy", PropertyIdConstants.JobPropertyIdStart + 65);

        static PropertyId _TaskLevelUpdateTime = new PropertyId(StorePropertyType.DateTime, "TaskLevelUpdateTime", PropertyIdConstants.JobPropertyIdStart + 66, PropFlags.None);

        static PropertyId _MinMaxUpdateTime = new PropertyId(StorePropertyType.DateTime, "MinMaxUpdateTime", PropertyIdConstants.JobPropertyIdStart + 67, PropFlags.None);

        static PropertyId _ComputedMinCores = new PropertyId(StorePropertyType.Int32, "ComputedMinCores", PropertyIdConstants.JobPropertyIdStart + 68, PropFlags.None);
        static PropertyId _ComputedMaxCores = new PropertyId(StorePropertyType.Int32, "ComputedMaxCores", PropertyIdConstants.JobPropertyIdStart + 69, PropFlags.None);
        static PropertyId _ComputedMinSockets = new PropertyId(StorePropertyType.Int32, "ComputedMinSockets", PropertyIdConstants.JobPropertyIdStart + 70, PropFlags.None);
        static PropertyId _ComputedMaxSockets = new PropertyId(StorePropertyType.Int32, "ComputedMaxSockets", PropertyIdConstants.JobPropertyIdStart + 71, PropFlags.None);
        static PropertyId _ComputedMinNodes = new PropertyId(StorePropertyType.Int32, "ComputedMinNodes", PropertyIdConstants.JobPropertyIdStart + 72, PropFlags.None);
        static PropertyId _ComputedMaxNodes = new PropertyId(StorePropertyType.Int32, "ComputedMaxNodes", PropertyIdConstants.JobPropertyIdStart + 73, PropFlags.None);

        static PropertyId _RequestCancel = new PropertyId(StorePropertyType.CancelRequest, "RequestCancel", PropertyIdConstants.JobPropertyIdStart + 74);

        static PropertyId _RequeueCount = new PropertyId(StorePropertyType.Int32, "RequeueCount", PropertyIdConstants.JobPropertyIdStart + 75);
        static PropertyId _AutoRequeueCount = new PropertyId(StorePropertyType.Int32, "AutoRequeueCount", PropertyIdConstants.JobPropertyIdStart + 76);
        static PropertyId _FailureReason = new PropertyId(StorePropertyType.FailureReason, "FailureReason", PropertyIdConstants.JobPropertyIdStart + 77);

        static PropertyId _EndpointReference = new PropertyId(StorePropertyType.String, "EndpointReference", PropertyIdConstants.JobPropertyIdStart + 78, PropFlags.None);
        static PropertyId _ServiceName = new PropertyId(StorePropertyType.String, "ServiceName", PropertyIdConstants.JobPropertyIdStart + 79, PropFlags.None);

        static PropertyId _PendingReason = new PropertyId(StorePropertyType.PendingReason, "PendingReason", PropertyIdConstants.JobPropertyIdStart + 80);

        static PropertyId _ComputedNodeList = new PropertyId(StorePropertyType.String, "ComputedNodeList", PropertyIdConstants.JobPropertyIdStart + 82, PropFlags.Calculated);

        static PropertyId _AutoCalculateMax = new PropertyId(StorePropertyType.Boolean, "AutoCalculateMax", PropertyIdConstants.JobPropertyIdStart + 83);
        static PropertyId _AutoCalculateMin = new PropertyId(StorePropertyType.Boolean, "AutoCalculateMin", PropertyIdConstants.JobPropertyIdStart + 84);

        static PropertyId _ParentJobId = new PropertyId(StorePropertyType.Int32, "ParentJobId", PropertyIdConstants.JobPropertyIdStart + 85);
        static PropertyId _ChildJobId = new PropertyId(StorePropertyType.Int32, "ChildJobId", PropertyIdConstants.JobPropertyIdStart + 86);

        static PropertyId _MinMemory = new PropertyId(StorePropertyType.Int32, "MinMemory", PropertyIdConstants.JobPropertyIdStart + 87);
        static PropertyId _MaxMemory = new PropertyId(StorePropertyType.Int32, "MaxMemory", PropertyIdConstants.JobPropertyIdStart + 88);
        static PropertyId _MinProcsPerNode = new PropertyId(StorePropertyType.Int32, "MinCoresPerNode", PropertyIdConstants.JobPropertyIdStart + 89);

        static PropertyId _MaxProcsPerNode = new PropertyId(StorePropertyType.Int32, "MaxCoresPerNode", PropertyIdConstants.JobPropertyIdStart + 90);

        static PropertyId _NumberOfCalls = new PropertyId(StorePropertyType.Int64, "NumberOfCalls", PropertyIdConstants.JobPropertyIdStart + 91);
        static PropertyId _NumberOfOutstandingCalls = new PropertyId(StorePropertyType.Int64, "NumberOfOutstandingCalls", PropertyIdConstants.JobPropertyIdStart + 92);
        static PropertyId _CallDuration = new PropertyId(StorePropertyType.Int64, "CallDuration", PropertyIdConstants.JobPropertyIdStart + 93);
        static PropertyId _CallsPerSecond = new PropertyId(StorePropertyType.Int64, "CallsPerSecond", PropertyIdConstants.JobPropertyIdStart + 94);

        static PropertyId _FailOnTaskFailure = new PropertyId(StorePropertyType.Boolean, "FailOnTaskFailure", PropertyIdConstants.JobPropertyIdStart + 95);
        static PropertyId _Preemptable = new PropertyId(StorePropertyType.Boolean, "Preemptable", PropertyIdConstants.JobPropertyIdStart + 96);

        static PropertyId _DefaultTaskGroupId = new PropertyId(StorePropertyType.Int32, "DefaultTaskGroupId", PropertyIdConstants.JobPropertyIdStart + 97, PropFlags.ReadOnly);

        static PropertyId _Source = new PropertyId(StorePropertyType.String, "ClientSource", PropertyIdConstants.JobPropertyIdStart + 98);
        static PropertyId _SubSource = new PropertyId(StorePropertyType.String, "ClientSubSource", PropertyIdConstants.JobPropertyIdStart + 99);


        static PropertyId _Progress = new PropertyId(StorePropertyType.Int32, "Progress", PropertyIdConstants.JobPropertyIdStart + 100);
        static PropertyId _ProgressMessage = new PropertyId(StorePropertyType.String, "ProgressMessage", PropertyIdConstants.JobPropertyIdStart + 101);

        static PropertyId _targetResourceCount = new PropertyId(StorePropertyType.Int32, "TargetResourceCount", PropertyIdConstants.JobPropertyIdStart + 102);

        static PropertyId _ExpandedPriority = new PropertyId(StorePropertyType.Int32, "ExpandedPriority", PropertyIdConstants.JobPropertyIdStart + 103);

        static PropertyId _holdUntil = new PropertyId(StorePropertyType.DateTime, "HoldUntil", PropertyIdConstants.JobPropertyIdStart + 104);

        static PropertyId _NotifyOnStart = new PropertyId(StorePropertyType.Boolean, "NotifyOnStart", PropertyIdConstants.JobPropertyIdStart + 105);
        static PropertyId _NotifyOnCompletion = new PropertyId(StorePropertyType.Boolean, "NotifyOnCompletion", PropertyIdConstants.JobPropertyIdStart + 106);

        static PropertyId _ExcludedNodes = new PropertyId(StorePropertyType.StringList, "ExcludedNodes", PropertyIdConstants.JobPropertyIdStart + 107);
        static PropertyId _EmailAddress = new PropertyId(StorePropertyType.String, "EmailAddress", PropertyIdConstants.JobPropertyIdStart + 108);

        static PropertyId _Pool = new PropertyId(StorePropertyType.String, "Pool", PropertyIdConstants.JobPropertyIdStart + 109, PropFlags.Visible | PropFlags.Indexed);

        static PropertyId _RuntimeType = new PropertyId(StorePropertyType.JobRuntimeType, "RuntimeType", PropertyIdConstants.JobPropertyIdStart + 110);

        static PropertyId _NodeGroupOp = new PropertyId(StorePropertyType.JobNodeGroupOp, "NodeGroupOp", PropertyIdConstants.JobPropertyIdStart + 111);

        static PropertyId _SingleNode = new PropertyId(StorePropertyType.Boolean, "SingleNode", PropertyIdConstants.JobPropertyIdStart + 112);

        static PropertyId _JobValidExitCodes = new PropertyId(StorePropertyType.String, "JobValidExitCodes", PropertyIdConstants.JobPropertyIdStart + 113);

        static PropertyId _ParentJobIds = new PropertyId(StorePropertyType.String, "ParentJobIds", PropertyIdConstants.JobPropertyIdStart + 114);

        static PropertyId _FailDependentTasks = new PropertyId(StorePropertyType.Boolean, "FailDependentTasks", PropertyIdConstants.JobPropertyIdStart + 115);

        static PropertyId _ChildJobIds = new PropertyId(StorePropertyType.String, "ChildJobIds", PropertyIdConstants.JobPropertyIdStart + 116);

        static PropertyId _EstimatedProcessMemory = new PropertyId(StorePropertyType.Int32, "EstimatedProcessMemory", PropertyIdConstants.JobPropertyIdStart + 117);

        static PropertyId _PlannedCoreCount = new PropertyId(StorePropertyType.Int32, "PlannedCoreCount", PropertyIdConstants.JobPropertyIdStart + 118);

        static PropertyId _TaskExecutionFailureRetryLimit = new PropertyId(StorePropertyType.Int32, "TaskExecutionFailureRetryLimit", PropertyIdConstants.JobPropertyIdStart + 119);

        static PropertyId _NodePrepareTask = new PropertyId(StorePropertyType.String, "NodePrepareTask", PropertyIdConstants.JobPropertyIdStart + 120);

        static PropertyId _NodeReleaseTask = new PropertyId(StorePropertyType.String, "NodeReleaseTask", PropertyIdConstants.JobPropertyIdStart + 121);

    }
}
