using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the state of the task.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const Configuring = 1
    /// const Submitted = 2
    /// const Validating = 4
    /// const Queued = 8
    /// const Dispatching = 16
    /// const Running = 32
    /// const Finishing = 64
    /// const Finished = 128
    /// const Failed = 256
    /// const Canceled = 512
    /// const Canceling = 1024
    /// const All = 2047</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.State" />
    [Flags]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidTaskState)]
    public enum TaskState
    {
        /// <summary>
        ///   <para>The state is not set. This enumeration member represents a value of 0.</para>
        /// </summary>
        NA = 0x0,
        /// <summary>
        ///   <para>The task is being configured. The application called the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTask" /> method to create the task but has not called the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> method to add the task to the job. This enumeration member represents a value of 1.</para> 
        /// </summary>
        Configuring = 0x1,
        /// <summary>
        ///   <para>The task was added to the scheduling queue. This enumeration member represents a value of 2.</para>
        /// </summary>
        Submitted = 0x2,
        /// <summary>
        ///   <para>The scheduler is determining if the task is correctly configured and can run. This enumeration member represents a value of 4.</para>
        /// </summary>
        Validating = 0x4,
        /// <summary>
        ///   <para>The task was added to the scheduling queue. This enumeration member represents a value of 8.</para>
        /// </summary>
        Queued = 0x8,
        /// <summary>
        ///   <para>The scheduler is in the process of sending the task to the node to run. This enumeration member represents a value of 16.</para>
        /// </summary>
        Dispatching = 0x10,
        /// <summary>
        ///   <para>The task is running. This enumeration member represents a value of 32.</para>
        /// </summary>
        Running = 0x20,
        /// <summary>
        ///   <para>The node is cleaning up the resources that were allocated to the task. This enumeration member represents a value of 64.</para>
        /// </summary>
        Finishing = 0x40,
        /// <summary>
        ///   <para>The task successfully finished. This enumeration member represents a value of 128.</para>
        /// </summary>
        Finished = 0x80,
        /// <summary>
        ///   <para>The task failed, the job was canceled, or a system error occurred on the compute node. To get a description of the error, access the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ErrorMessage" /> property. This enumeration member represents a value of 256.</para>
        /// </summary>
        Failed = 0x100,
        /// <summary>
        ///   <para>The task was canceled (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CancelTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />). If the caller provided the reason for canceling the task, then the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ErrorMessage" /> property will contain the reason. This enumeration member represents a value of 512.</para> 
        /// </summary>
        Canceled = 0x200,
        /// <summary>
        ///   <para>The task is in the process of being canceled. This enumeration member represents a value of 1024.</para>
        /// </summary>
        Canceling = 0x400,
        /// <summary>
        ///   <para>A mask used to indicate all states. This enumeration member represents a value of 2047.</para>
        /// </summary>
        All = Configuring | Submitted | Validating | Queued | Dispatching | Running | Finishing | Finished | Failed | Canceled | Canceling,
    }

    /// <summary>
    ///   <para>Defines how to run the command for a task.</para>
    /// </summary>
    /// <remarks>
    ///   <para>The following properties do not apply to tasks that you start on 
    /// a per-resource basis, and thus you cannot set these properties if you set the  
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Type" /> property to 
    /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskType.NodePrep" />, 
    /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskType.NodeRelease" />, or 
    /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskType.Service" />:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfNodes" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfNodes" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfCores" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfCores" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfSockets" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfSockets" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequiredNodes" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsExclusive" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsRerunnable" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.DependsOn" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EndValue" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IncrementValue" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StartValue" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    ///   <para>You cannot add a basic task or a parametric sweep task to a job that contains a service task.</para>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const Basic = 0
    /// const NodePrep = 2
    /// const NodeRelease = 3
    /// const ParametricSweep = 1
    /// const Service = 4</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Type" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidTaskType)]
    public enum TaskType
    {
        /// <summary>
        ///   <para>Runs a single instance of a serial application or a Message Passing Interface (MPI) application. An MPI application typically runs 
        /// concurrently on multiple cores and can span multiple nodes. This task type is the default. This enumeration member represents a value of 0.</para>
        /// </summary>
        Basic = 0,
        /// <summary>
        ///   <para>Runs a command a specified number of times as indicated by the start, 
        /// end, and increment values, generally across indexed input and output files. The steps of the  
        /// sweep may or may not run in parallel, depending on the resources that are available 
        /// on the HPC cluster when the task is running. This enumeration member represents a value of 1.</para> 
        /// </summary>
        ParametricSweep = 1,
        /// <summary>
        ///   <para>Runs a command or script on each compute node as it is allocated to the job. The Node Prep task runs on a node before any other task 
        /// in the job. If the Node Prep task fails to run on a node, then 
        /// that node is not added to the job. This enumeration member represents a value of 2.</para> 
        /// </summary>
        NodePrep = 2,
        /// <summary>
        ///   <para>Runs a command or script on compute each node as it is released from the job. Node Release tasks run when the job is canceled by 
        /// the user or by graceful preemption. Node Release tasks do not run when the 
        /// job is canceled by immediate preemption.  This enumeration member represents a value of 3.</para> 
        /// </summary>
        NodeRelease = 3,
        /// <summary>
        ///   <para>Runs a command or service on all resources that are assigned to the job. New instances of the command are started when new resources are added to the job, or 
        /// if a previously running instance exits and the resource that the previously running instance was running on is still allocated to the job. A service task continues to start new instances until the  
        /// task is canceled, the maximum run time expires, or the maximum number of instances is reached. A service task can create up to 1,000,000 sub-tasks. Tasks that you submit through a Service Oriented 
        /// Architecture (SOA) client run as service tasks. You cannot add a basic task or a parametric sweep task to a job that contains a service task. This enumeration member represents a value of 4.</para> 
        /// </summary>
        Service = 4,
    }

    /// <summary>
    ///   <para>Defines identifiers that uniquely identify the properties of a task.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Use these identifiers when creating filters, specifying sort 
    /// orders, and using rowsets to retrieve specific properties from the database.</para>
    /// </remarks>
    /// <example>
    ///   <para>The following example shows how to use the property identifiers with a rowset enumerator to retrieve all 
    /// the properties for a specific task in a job. For an alternative way of accessing the property value, see  
    /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.Error" />.</para>
    ///   <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using System.Linq;
    /// using System.Text;
    /// using Microsoft.Hpc.Scheduler;
    /// using Microsoft.Hpc.Scheduler.Properties;
    /// 
    /// namespace AccessTaskPropertyIds
    /// {
    ///     class Program
    ///     {
    ///         static void Main(string[] args)
    ///         {
    /// IScheduler scheduler = new Scheduler();
    /// ISchedulerJob job = null;
    /// ITaskId taskId = null;
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
    /// // for your HPC cluster that is consistent with the filters 
    /// // specified later in the code (in other words, a job that has 
    /// // a parametric sweep task with at least three subtasks as the
    /// // first task in the job).
    /// int jobId = 124;
    /// 
    /// scheduler.Connect(headNodeName);
    /// 
    /// properties.Add(TaskPropertyIds.AllocatedCores);
    /// properties.Add(TaskPropertyIds.AllocatedNodes);
    /// properties.Add(TaskPropertyIds.AllocatedSockets);
    /// properties.Add(TaskPropertyIds.AutoRequeueCount);
    /// properties.Add(TaskPropertyIds.ChangeTime);
    /// properties.Add(TaskPropertyIds.Closed);
    /// properties.Add(TaskPropertyIds.CommandLine);
    /// properties.Add(TaskPropertyIds.CreateTime);
    /// properties.Add(TaskPropertyIds.CurrentCoreCount);
    /// properties.Add(TaskPropertyIds.CurrentNodeCount);
    /// properties.Add(TaskPropertyIds.CurrentSocketCount);
    /// properties.Add(TaskPropertyIds.DependsOn);
    /// properties.Add(TaskPropertyIds.EndTime);
    /// properties.Add(TaskPropertyIds.EndValue);
    /// properties.Add(TaskPropertyIds.ErrorCode);
    /// properties.Add(TaskPropertyIds.ErrorMessage);
    /// properties.Add(TaskPropertyIds.ErrorParams);
    /// properties.Add(TaskPropertyIds.ExitCode);
    /// properties.Add(TaskPropertyIds.FailureReason);
    /// properties.Add(TaskPropertyIds.GroupId);
    /// properties.Add(TaskPropertyIds.HasCustomProperties);
    /// properties.Add(TaskPropertyIds.HasRuntime);
    /// properties.Add(TaskPropertyIds.Id);
    /// properties.Add(TaskPropertyIds.IncrementValue);
    /// properties.Add(TaskPropertyIds.InstanceId);
    /// properties.Add(TaskPropertyIds.InstanceValue);
    /// properties.Add(TaskPropertyIds.IsExclusive);
    /// properties.Add(TaskPropertyIds.IsParametric);
    /// properties.Add(TaskPropertyIds.IsRerunnable);
    /// properties.Add(TaskPropertyIds.MaxCores);
    /// properties.Add(TaskPropertyIds.MaxSockets);
    /// properties.Add(TaskPropertyIds.MemoryUsed);
    /// properties.Add(TaskPropertyIds.MinCores);
    /// properties.Add(TaskPropertyIds.MinNodes);
    /// properties.Add(TaskPropertyIds.MinSockets);
    /// properties.Add(TaskPropertyIds.Name);
    /// properties.Add(TaskPropertyIds.JobTaskId);
    /// properties.Add(TaskPropertyIds.Output);
    /// properties.Add(TaskPropertyIds.ParentJobId);
    /// properties.Add(TaskPropertyIds.ParentJobState);
    /// properties.Add(TaskPropertyIds.PendingReason);
    /// properties.Add(TaskPropertyIds.PreviousState);
    /// properties.Add(TaskPropertyIds.ProcessIds);
    /// properties.Add(TaskPropertyIds.RequestCancel);
    /// properties.Add(TaskPropertyIds.RequeueCount);
    /// properties.Add(TaskPropertyIds.RequiredNodes);
    /// properties.Add(TaskPropertyIds.RuntimeSeconds);
    /// properties.Add(TaskPropertyIds.StartTime);
    /// properties.Add(TaskPropertyIds.StartValue);
    /// properties.Add(TaskPropertyIds.State);
    /// properties.Add(TaskPropertyIds.StdErrFilePath);
    /// properties.Add(TaskPropertyIds.StdInFilePath);
    /// properties.Add(TaskPropertyIds.StdOutFilePath);
    /// properties.Add(TaskPropertyIds.SubmitTime);
    /// properties.Add(TaskPropertyIds.TaskId);
    /// properties.Add(TaskPropertyIds.TaskOwner);
    /// properties.Add(TaskPropertyIds.Timestamp);
    /// properties.Add(TaskPropertyIds.TotalCpuTime);
    /// properties.Add(TaskPropertyIds.TotalKernelTime);
    /// properties.Add(TaskPropertyIds.TotalNodeCount);
    /// properties.Add(TaskPropertyIds.TotalCoreCount);
    /// properties.Add(TaskPropertyIds.TotalSocketCount);
    /// properties.Add(TaskPropertyIds.TotalUserTime);
    /// properties.Add(TaskPropertyIds.UnitType);
    /// properties.Add(TaskPropertyIds.UserBlob);
    /// properties.Add(TaskPropertyIds.WorkDirectory);
    /// 
    /// // The following lines of code are specific to 
    /// // Windows HPC Server 2008 R2. To use this example with 
    /// // Windows HPC Server 2008, remove the lines between here and 
    /// // the next comment, or place the lines within comments.
    /// properties.Add(TaskPropertyIds.AllocatedCoreIds);
    /// properties.Add(TaskPropertyIds.FailedNodeId);
    /// properties.Add(TaskPropertyIds.IsServiceConcluded);
    /// properties.Add(TaskPropertyIds.TotalSubTaskNumber);
    /// properties.Add(TaskPropertyIds.Type);
    /// // End of code specific to Windows HPC Server 2008 R2.
    /// 
    /// // If you do not specify a filter, you will receive all tasks
    /// // in the job. This example retrieves the third instance of
    /// // the first task from the job (the task is a parametric task).
    /// // You cannot use TaskId to filter for a specific task. 
    /// filters.Add(FilterOperator.Equal, TaskPropertyIds.JobTaskId, 1);
    /// filters.Add(FilterOperator.Equal, TaskPropertyIds.InstanceId, 3);
    /// 
    /// job = scheduler.OpenJob(jobId);
    /// 
    /// using (rows = job.OpenTaskEnumerator(properties, filters, null, true))
    /// {
    ///     PropertyRow row = rows.GetRows(1).Rows[0];
    /// 
    ///     taskId = (ITaskId)row[TaskPropertyIds.TaskId].Value;
    ///     Console.WriteLine("TaskId: id({0}), instance({1}), job({2})",
    ///         taskId.JobTaskId, taskId.InstanceId, taskId.ParentJobId);
    /// 
    ///     // Key is the node name; value is the number of cores on the node allocated to the task.
    ///     Console.WriteLine("AllocatedCores: ");
    ///     foreach (KeyValuePair&lt;string, int&gt; node in (List&lt;KeyValuePair&lt;string, int&gt;&gt;)row[TaskPropertyIds.AllocatedCores].Value)
    /// Console.Write("{0}({1})", node.Key, node.Value);
    ///     Console.WriteLine();
    /// 
    ///     // Key is the node name; value is always 1.
    ///     Console.WriteLine("AllocatedNodes: ");
    ///     foreach (KeyValuePair&lt;string, int&gt; node in (List&lt;KeyValuePair&lt;string, int&gt;&gt;)row[TaskPropertyIds.AllocatedNodes].Value)
    ///         Console.Write(node.Key + " ");
    ///     Console.WriteLine();
    /// 
    ///     // Key is the node name; value is the number of sockets on the node allocated to the task.
    ///     Console.WriteLine("AllocatedSockets: ");
    ///     foreach (KeyValuePair&lt;string, int&gt; node in (List&lt;KeyValuePair&lt;string, int&gt;&gt;)row[TaskPropertyIds.AllocatedSockets].Value)
    ///         Console.Write("{0}({1})", node.Key, node.Value);
    ///     Console.WriteLine();
    /// 
    ///     property = row[TaskPropertyIds.AutoRequeueCount];
    ///     Console.WriteLine("AutoRequeueCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.ChangeTime];
    ///     Console.WriteLine("ChangeTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.Closed];
    ///     Console.WriteLine("Closed: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.CommandLine];
    ///     Console.WriteLine("CommandLine: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.CreateTime];
    ///     Console.WriteLine("CreateTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.CurrentCoreCount];
    ///     Console.WriteLine("CurrentCoreCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.CurrentNodeCount];
    ///     Console.WriteLine("CurrentNodeCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.CurrentSocketCount];
    ///     Console.WriteLine("CurrentSocketCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.DependsOn];
    ///     Console.WriteLine("DependsOn: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.EndTime];
    ///     Console.WriteLine("EndTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.EndValue];
    ///     Console.WriteLine("EndValue: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.ErrorCode];
    ///     Console.WriteLine("ErrorCode: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.ErrorMessage];
    ///     Console.WriteLine("ErrorMessage: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.ErrorParams];
    ///     Console.WriteLine("ErrorParams: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.ExitCode];
    ///     Console.WriteLine("ExitCode: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.FailureReason];
    ///     Console.WriteLine("FailureReason: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.GroupId];
    ///     Console.WriteLine("GroupId: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.HasCustomProperties];
    ///     Console.WriteLine("HasCustomProperties: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.HasRuntime];
    ///     Console.WriteLine("HasRuntime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.Id];
    ///     Console.WriteLine("Id: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.IncrementValue];
    ///     Console.WriteLine("IncrementValue: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.InstanceId];
    ///     Console.WriteLine("InstanceId: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.InstanceValue];
    ///     Console.WriteLine("InstanceValue: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.IsExclusive];
    ///     Console.WriteLine("IsExclusive: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.IsParametric];
    ///     Console.WriteLine("IsParametric: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.IsRerunnable];
    ///     Console.WriteLine("IsRerunnable: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.MaxCores];
    ///     Console.WriteLine("MaxCores: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.MaxNodes];
    ///     Console.WriteLine("MaxNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.MaxSockets];
    ///     Console.WriteLine("MaxSockets: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.MemoryUsed];
    ///     Console.WriteLine("MemoryUsed: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.MinCores];
    ///     Console.WriteLine("MinCores: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.MinNodes];
    ///     Console.WriteLine("MinNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.MinSockets];
    ///     Console.WriteLine("MinSockets: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.Name];
    ///     Console.WriteLine("Name: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.JobTaskId];
    ///     Console.WriteLine("JobTaskId: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.Output];
    ///     Console.WriteLine("Output: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.ParentJobId];
    ///     Console.WriteLine("ParentJobId: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.ParentJobState];
    ///     Console.WriteLine("ParentJobState: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.PendingReason];
    ///     Console.WriteLine("PendingReason: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.PreviousState];
    ///     Console.WriteLine("PreviousState: {0}", (null != property) ? property.Value : "");
    /// 
    ///     Console.WriteLine("ProcessIds: ");
    ///     foreach (KeyValuePair&lt;string, string&gt; id in (Dictionary&lt;string, string&gt;)row[TaskPropertyIds.ProcessIds].Value)
    ///         Console.Write("{0}({1})", id.Key, id.Value);
    ///     Console.WriteLine();
    /// 
    ///     property = row[TaskPropertyIds.RequestCancel];
    ///     Console.WriteLine("RequestCancel: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.RequeueCount];
    ///     Console.WriteLine("RequeueCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.RequiredNodes];
    ///     Console.WriteLine("RequiredNodes: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.RuntimeSeconds];
    ///     Console.WriteLine("RuntimeSeconds: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.StartTime];
    ///     Console.WriteLine("StartTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.StartValue];
    ///     Console.WriteLine("StartValue: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.State];
    ///     Console.WriteLine("State: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.StdErrFilePath];
    ///     Console.WriteLine("StdErrFilePath: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.StdInFilePath];
    ///     Console.WriteLine("StdInFilePath: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.StdOutFilePath];
    ///     Console.WriteLine("StdOutFilePath: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.SubmitTime];
    ///     Console.WriteLine("SubmitTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.TaskOwner];
    ///     Console.WriteLine("TaskOwner " + (string)row[TaskPropertyIds.TaskOwner].Value);
    /// 
    ///     property = row[TaskPropertyIds.TotalCpuTime];
    ///     Console.WriteLine("TotalCpuTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.TotalKernelTime];
    ///     Console.WriteLine("TotalKernelTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.TotalNodeCount];
    ///     Console.WriteLine("TotalNodeCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.TotalCoreCount];
    ///     Console.WriteLine("TotalCoreCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.TotalSocketCount];
    ///     Console.WriteLine("TotalSocketCount: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.TotalUserTime];
    ///     Console.WriteLine("TotalUserTime: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.UnitType];
    ///     Console.WriteLine("UnitType: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.UserBlob];
    ///     Console.WriteLine("UserBlob: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.WorkDirectory];
    ///     Console.WriteLine("WorkDirectory: {0}", (null != property) ? property.Value : "");
    /// 
    ///     // The following lines of code are specific to 
    ///     // Windows HPC Server 2008 R2. To use this example with 
    ///     // Windows HPC Server 2008, remove the lines between here and 
    ///     // the comment that ends this section, or place the lines 
    ///     // within comments.
    /// 
    ///     // Key is the node name; Value is the identifier to the core 
    ///     // on the node that is allocated to the task.
    ///     Console.WriteLine("AllocatedCoreIds: ");
    ///     foreach (KeyValuePair&lt;string, string&gt; node in (Dictionary&lt;string, string&gt;)row[TaskPropertyIds.AllocatedCoreIds].Value)
    /// Console.Write("{0}({1})", node.Key, node.Value);
    ///     Console.WriteLine();
    /// 
    ///     property = row[TaskPropertyIds.FailedNodeId];
    ///     Console.WriteLine("FailedNodeId: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.IsServiceConcluded];
    ///     Console.WriteLine("IsServiceConcluded: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.TotalSubTaskNumber];
    ///     Console.WriteLine("TotalSubTaskNumber: {0}", (null != property) ? property.Value : "");
    /// 
    ///     property = row[TaskPropertyIds.Type];
    ///     Console.WriteLine("Type: {0}", (null != property) ? property.Value : "");
    /// 
    ///     // End of code specific to Windows HPC Server 2008 R2.
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
    public class TaskPropertyIds
    {
        // Make it so that no one can construct one of these.
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" /> class.</para>
        /// </summary>
        protected TaskPropertyIds()
        {
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the task in the store.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para>The cores that were allocated to the task the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the task is running, it lists the cores that have been allocated since the task began running. 
        /// The list of cores does not shrink even if the server shrinks the cores that it allocates to your task. </para>
        ///   <para>The property is a collection of 
        /// <see cref="System.Collections.Generic.KeyValuePair`2" /> objects. The key is the node name and the value is the number of cores.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId AllocatedCores
        {
            get { return StorePropertyIds.AllocatedCores; }
        }

        /// <summary>
        ///   <para>The sockets that were allocated to the task the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the task is running, it lists the sockets that have been allocated since the task began running. 
        /// The list of sockets does not shrink even if the server shrinks the sockets that it allocates to your task. </para>
        ///   <para>The property is a collection of 
        /// <see cref="System.Collections.Generic.KeyValuePair`2" /> objects. The key is the node name and the value is the number of sockets.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId AllocatedSockets
        {
            get { return StorePropertyIds.AllocatedSockets; }
        }

        /// <summary>
        ///   <para>The nodes that were allocated to the task the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the task is running, it lists the nodes that have been allocated since the task began running. 
        /// The list of nodes does not shrink even if the server shrinks the nodes that it allocates to your task. </para>
        ///   <para>The property is a collection of 
        /// <see cref="System.Collections.Generic.KeyValuePair`2" /> objects. The key is the node name and the value is the number of nodes (always 1).</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId AllocatedNodes
        {
            get { return StorePropertyIds.AllocatedNodes; }
        }

        /// <summary>
        ///   <para>The identifiers of the processor cores that have been allocated to run the task or that have run the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is a 
        /// <see cref="System.Collections.Generic.Dictionary`2" /> of 
        /// <see cref="System.Collections.Generic.KeyValuePair`2" /> objects. The key is the node name and the value is the identifier of the core.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId AllocatedCoreIds
        {
            get { return StorePropertyIds.AllocatedCoreIds; }
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
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyError" /> value. </para>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyError" />.</para>
        /// </remarks>
        /// <example>
        ///   <para>The following example shows how you use this property. You must use a 
        /// zero-based index to index the property instead of using the property identifier to index the property.</para>
        ///   <code>
        ///     // Assumes that the zero-based index for the 
        ///     // Output property for this query is four. The index is 
        ///     // based on the position in which you insert the property 
        ///     // identifier into your IPropertyIdCollection collection.
        ///     property = row[JobPropertyIds.Output];
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
        ///   <para>The output generated by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Output" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId Output
        {
            get { return _Output; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the parent job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ParentJobId" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId ParentJobId
        {
            get { return _ParentJobId; }
        }

        /// <summary>
        ///   <para>The previous state of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details of this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.PreviousState" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId PreviousState
        {
            get { return _PreviousState; }
        }

        /// <summary>
        ///   <para>The nodes that the task requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequiredNodes" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId RequiredNodes
        {
            get { return _RequiredNodes; }
        }

        /// <summary>
        ///   <para>The run-time limit for the task, in seconds.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Runtime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId RuntimeSeconds
        {
            get { return StorePropertyIds.RuntimeSeconds; }
        }

        /// <summary>
        ///   <para>The state of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.State" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId State
        {
            get { return _State; }
        }

        /// <summary>
        ///   <para>The total CPU time used by the task (includes the time spent in user-mode and kernel-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int64" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId TotalCpuTime
        {
            get { return StorePropertyIds.TotalCpuTime; }
        }

        /// <summary>
        ///   <para>The time that the task spent in user-mode.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int64" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId TotalUserTime
        {
            get { return StorePropertyIds.TotalUserTime; }
        }

        /// <summary>
        ///   <para>The time that the task spent in kernel-mode.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int64" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId TotalKernelTime
        {
            get { return StorePropertyIds.TotalKernelTime; }
        }

        /// <summary>
        ///   <para>The total number of sockets used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId TotalSocketCount
        {
            get { return StorePropertyIds.TotalSocketCount; }
        }

        /// <summary>
        ///   <para>The total number of nodes used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId TotalNodeCount
        {
            get { return StorePropertyIds.TotalNodeCount; }
        }

        /// <summary>
        ///   <para>The total number of cores used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId TotalCoreCount
        {
            get { return StorePropertyIds.TotalCoreCount; }
        }

        /// <summary>
        ///   <para>The internal task object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId TaskObject
        {
            get { return StorePropertyIds.TaskObject; }
        }

        /// <summary>
        ///   <para>Determines whether cores, nodes, or sockets are used to allocate resources for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId UnitType
        {
            get { return StorePropertyIds.UnitType; }
        }

        /// <summary>
        ///   <para>The working directory used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.WorkDirectory" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId WorkDirectory
        {
            get { return _WorkDirectory; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the task in a job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId.JobTaskId" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId JobTaskId
        {
            get { return _NiceId; }
        }

        /// <summary>
        ///   <para>The command line that the task executes.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.CommandLine" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId CommandLine
        {
            get { return _CommandLine; }
        }

        /// <summary>
        ///   <para>A comma-delimited list of tasks that must complete before this task can run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.DependsOn" />.</para>
        ///   <para>The property type is <see cref="System.String" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId DependsOn
        {
            get { return _DependsOn; }
        }

        /// <summary>
        ///   <para>Determines whether the task can be run again.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsRerunnable" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId IsRerunnable
        {
            get { return _IsRerunnable; }
        }

        /// <summary>
        ///   <para>The path to which the server redirects standard output.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdOutFilePath" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId StdOutFilePath
        {
            get { return _StdOutFilePath; }
        }

        /// <summary>
        ///   <para>The path from which the server redirects standard input.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdInFilePath" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId StdInFilePath
        {
            get { return _StdInFilePath; }
        }

        /// <summary>
        ///   <para>The path to which the server redirects standard error.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdErrFilePath" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId StdErrFilePath
        {
            get { return _StdErrFilePath; }
        }

        /// <summary>
        ///   <para>The task contains user-defined properties.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Boolean" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId HasCustomProperties
        {
            get { return _HasCustomProps; }
        }

        /// <summary>
        ///   <para>The exit code that the task set when it returned.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ExitCode" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId ExitCode
        {
            get { return _ExitCode; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId TaskValidExitCodes
        {
            get { return _TaskValidExitCodes; }
        }

        /// <summary>
        ///   <para>Indicates whether the user has requested that the task be canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Boolean" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId RequestCancel
        {
            get { return _RequestCancel; }
        }

        /// <summary>
        ///   <para>Indicates whether the task was closed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Boolean" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId Closed
        {
            get { return _Closed; }
        }

        /// <summary>
        ///   <para>The number of cores being used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is set only if the task is running.</para>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId CurrentCoreCount
        {
            get { return TotalCoreCount; }
        }

        /// <summary>
        ///   <para>The number of sockets being used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is set only if the task is running.</para>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId CurrentSocketCount
        {
            get { return TotalSocketCount; }
        }

        /// <summary>
        ///   <para>The number of nodes being used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>This property is set only if the task is running.</para>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId CurrentNodeCount
        {
            get { return TotalNodeCount; }
        }

        /// <summary>
        ///   <para>The number of times that the task has been queued again.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details of this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequeueCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId RequeueCount
        {
            get { return _RequeueCount; }
        }

        /// <summary>
        ///   <para>The reason that the task failed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.FailureReason" /> enumeration.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId FailureReason
        {
            get { return _FailureReason; }
        }

        /// <summary>
        ///   <para>The reason that the task is pending.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.PendingReason.ReasonCode" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId PendingReason
        {
            get { return _PendingReason; }
        }

        /// <summary>
        ///   <para>The identifier of a node on which the task faieled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId FailedNodeId
        {
            get { return _FailedNodeId; }
        }

        /// <summary>
        ///   <para>Determines whether the task is a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        [Obsolete("Please use 'TaskPropertyIds.Type' instead")]
        static public PropertyId IsParametric
        {
            get { return _IsParametric; }
        }

        /// <summary>
        ///   <para>The start value for the first instance of a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StartValue" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId StartValue
        {
            get { return _StartValue; }
        }

        /// <summary>
        ///   <para>The last value to use for an instance of a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EndValue" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId EndValue
        {
            get { return _EndValue; }
        }

        /// <summary>
        ///   <para>The number by which to increment the instance value for each instance of a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IncrementValue" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId IncrementValue
        {
            get { return _IncrementValue; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the instance of a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId.InstanceId" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId InstanceId
        {
            get { return _InstanceId; }
        }

        /// <summary>
        ///   <para>The value used for the parametric instance.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is <see cref="System.Int32" /> object.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId InstanceValue
        {
            get { return _InstanceValue; }
        }

        /// <summary>
        ///   <para>The task group to which the task belongs.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        static public PropertyId GroupId
        {
            get { return _GroupId; }
        }

        /// <summary>
        ///   <para>The date and time that the task was submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.SubmitTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId SubmitTime
        {
            get { return StorePropertyIds.SubmitTime; }
        }

        /// <summary>
        ///   <para>The date and time that the task was created.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.CreateTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId CreateTime
        {
            get { return StorePropertyIds.CreateTime; }
        }

        /// <summary>
        ///   <para>The date and time that the task started.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.DateTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId StartTime
        {
            get { return StorePropertyIds.StartTime; }
        }

        /// <summary>
        ///   <para>The date and time that the task ended.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EndTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId EndTime
        {
            get { return StorePropertyIds.EndTime; }
        }

        /// <summary>
        ///   <para>The last time the user or server changed one of the property values of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ChangeTime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
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
        ///   <para>The display name of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Name" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId Name
        {
            get { return StorePropertyIds.Name; }
        }

        /// <summary>
        ///   <para>Determines whether the task has exclusive access to the node resource.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsExclusive" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId IsExclusive
        {
            get { return StorePropertyIds.IsExclusive; }
        }

        /// <summary>
        ///   <para>The reason why the task failed or was canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ErrorMessage" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId ErrorMessage
        {
            get { return StorePropertyIds.ErrorMessage; }
        }

        /// <summary>
        ///   <para>An error code that identifies the error that occurred while running or trying to run the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.ErrorCode" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId ErrorCode
        {
            get { return StorePropertyIds.ErrorCode; }
        }

        /// <summary>
        ///   <para>A delimited list of insertion strings that are inserted into the message string.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is <see cref="System.String" /> object. The delimiter is three vertical bars (|||).</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId ErrorParams
        {
            get { return StorePropertyIds.ErrorParams; }
        }

        /// <summary>
        ///   <para>The minimum number of cores that the task requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfCores" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId MinCores
        {
            get { return StorePropertyIds.MinCores; }
        }

        /// <summary>
        ///   <para>The maximum number of cores that the scheduler can allocate for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfCores" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId MaxCores
        {
            get { return StorePropertyIds.MaxCores; }
        }

        /// <summary>
        ///   <para>The minimum number of sockets that the task requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfSockets" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId MinSockets
        {
            get { return StorePropertyIds.MinSockets; }
        }

        /// <summary>
        ///   <para>The maximum number of sockets that the scheduler can allocate for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfSockets" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId MaxSockets
        {
            get { return StorePropertyIds.MaxSockets; }
        }

        /// <summary>
        ///   <para>The minimum number of nodes that the task requires.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfNodes" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId MinNodes
        {
            get { return StorePropertyIds.MinNodes; }
        }

        /// <summary>
        ///   <para>The maximum number of nodes that the scheduler can allocate for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfNodes" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId MaxNodes
        {
            get { return StorePropertyIds.MaxNodes; }
        }

        /// <summary>
        ///   <para>Indicates whether the run-time limit for the task is set.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.HasRuntime" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId HasRuntime
        {
            get { return StorePropertyIds.HasRuntime; }
        }

        /// <summary>
        ///   <para>The state of the job that contains this task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details of this property, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId ParentJobState
        {
            get { return _ParentJobState; }
        }

        /// <summary>
        ///   <para>The amount of memory used by the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId MemoryUsed
        {
            get { return StorePropertyIds.MemoryUsed; }
        }

        /// <summary>
        ///   <para>The identifiers that uniquely identify the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.TaskId" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId TaskId
        {
            get { return _TaskId; }
        }

        /// <summary>
        ///   <para>The number of times that the system reran the task when a system error occurred. </para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is 
        /// <see cref="System.Int32" />. For details on this property, see TaskRetryCount in the Remarks section of 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId AutoRequeueCount
        {
            get { return _AutoRequeueCount; }
        }

        /// <summary>
        ///   <para>The user that owns the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The task owner is the same as the job owner.</para>
        ///   <para>The property type is <see cref="System.String" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId TaskOwner
        {
            get { return _TaskOwner; }
        }

        /// <summary>
        ///   <para>The encrypted user blob.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId EncryptedUserBlob
        {
            get { return _EncryptedUserBlob; }
        }

        /// <summary>
        ///   <para>The user-defined data associated with the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>For details on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.UserBlob" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId UserBlob
        {
            get { return _UserBlob; }
        }

        /// <summary>
        ///   <para>A comma-delimited list of the process identifiers associated with the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ProcessIds
        {
            get { return StorePropertyIds.ProcessIds; }
        }

        /// <summary>
        ///   <para>A task type that defines how to run the command for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is a value from the <see cref="Microsoft.Hpc.Scheduler.Properties.StorePropertyType.TaskType" /> enumeration.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId Type
        {
            get { return _Type; }
        }

        /// <summary>
        ///   <para>The total number of subtasks in the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId TotalSubTaskNumber
        {
            get { return _TotalSubTaskNumber; }
        }

        /// <summary>
        ///   <para>Whether the HPC Job Scheduler Service has concluded starting subtasks for a service task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is a 
        /// 
        /// <see cref="System.Boolean" /> indicates whether the HPC Job Scheduler Service has concluded starting subtasks for a service task. True indicates that the HPC Job Scheduler Service has concluded starting subtasks for a service task. False indicates that the HPC Job Scheduler Service has not concluded starting subtasks for a service task, or that the task is not a service task.</para> 
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        public static PropertyId IsServiceConcluded
        {
            get { return _IsServiceConcluded; }
        }

        /// <summary>
        ///   <para>Whether the task is critical for the job. If a task is critical for the job, 
        /// the job and its tasks stop running and the job is immediately marked as failed if the task fails.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Boolean" />.</para>
        ///   <para>For more information on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailure" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailure" />
        public static PropertyId FailJobOnFailure
        {
            get { return _FailJobOnFailure; }
        }

        /// <summary>
        ///   <para>The number of subtasks of a critical parametric sweep or service task that must fail before 
        /// the job and its tasks and subtask should stop running and the task and job should be marked as failed.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For more information on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailureCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailureCount" />
        public static PropertyId FailJobOnFailureCount
        {
            get { return _FailJobOnFailureCount; }
        }

        /// <summary>
        ///   <para>Whether the task is running on nodes that are being preempted by other jobs. This is used during graceful preemption to mark running tasks in this job that are running on nodes which can be used by jobs wanting to preempt this job.</para>
        /// </summary>                                                                           
        /// <value>                                                                              
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Boolean" />.</para>
        ///   <para>For more information on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ExitIfPossible" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ExitIfPossible" />
        public static PropertyId ExitIfPossible
        {
            get { return _ExitIfPossible; }
        }

        /// <summary>
        ///   <para>The retried execution count of the task after execution failure.</para>
        /// </summary>
        /// <value>                                                                              
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.Int32" />.</para>
        ///   <para>For more information on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ExecutionFailureRetryCount" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ExecutionFailureRetryCount" />
        public static PropertyId ExecutionFailureRetryCount
        {
            get { return _ExecutionFailureRetryCount; }
        }

        /// <summary>
        /// <para>The requested node group of the task.</para>
        /// </summary>
        /// <value>                                                                              
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property type is <see cref="System.String" />.</para>
        ///   <para>For more information on this property, see <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequestedNodeGroup" />.</para>
        ///   <para>For an example that uses this property identifier, see <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequestedNodeGroup" />
        public static PropertyId RequestedNodeGroup
        {
            get { return _RequestedNodeGroup; }
        }


        //
        // Privates follow
        //

        static PropertyId _State = new PropertyId(StorePropertyType.TaskState, "State", PropertyIdConstants.TaskPropertyIdStart + 1);
        static PropertyId _PreviousState = new PropertyId(StorePropertyType.TaskState, "PreviousState", PropertyIdConstants.TaskPropertyIdStart + 2);

        static PropertyId _ParentJobId = new PropertyId(StorePropertyType.Int32, "ParentJobId", PropertyIdConstants.TaskPropertyIdStart + 3);
        static PropertyId _NiceId = new PropertyId(StorePropertyType.Int32, "NiceId", PropertyIdConstants.TaskPropertyIdStart + 4);

        static PropertyId _TaskOwner = new PropertyId(StorePropertyType.String, "TaskOwner", PropertyIdConstants.TaskPropertyIdStart + 5);

        static PropertyId _CommandLine = new PropertyId(StorePropertyType.String, "CommandLine", PropertyIdConstants.TaskPropertyIdStart + 10);
        static PropertyId _WorkDirectory = new PropertyId(StorePropertyType.String, "WorkDirectory", PropertyIdConstants.TaskPropertyIdStart + 11);

        static PropertyId _RequiredNodes = new PropertyId(StorePropertyType.String, "RequiredNodes", PropertyIdConstants.TaskPropertyIdStart + 12);

        static PropertyId _DependsOn = new PropertyId(StorePropertyType.String, "DependsOn", PropertyIdConstants.TaskPropertyIdStart + 13);

        static PropertyId _IsExclusive = new PropertyId(StorePropertyType.Boolean, "IsExclusive", PropertyIdConstants.TaskPropertyIdStart + 14);
        static PropertyId _IsRerunnable = new PropertyId(StorePropertyType.Boolean, "IsRerunnable", PropertyIdConstants.TaskPropertyIdStart + 15);

        static PropertyId _StdOutFilePath = new PropertyId(StorePropertyType.String, "StdOutFilePath", PropertyIdConstants.TaskPropertyIdStart + 23);
        static PropertyId _StdInFilePath = new PropertyId(StorePropertyType.String, "StdInFilePath", PropertyIdConstants.TaskPropertyIdStart + 24);
        static PropertyId _StdErrFilePath = new PropertyId(StorePropertyType.String, "StdErrFilePath", PropertyIdConstants.TaskPropertyIdStart + 25);

        static PropertyId _HasCustomProps = new PropertyId(StorePropertyType.Boolean, "HasCustomProps", PropertyIdConstants.TaskPropertyIdStart + 26);

        static PropertyId _ExitCode = new PropertyId(StorePropertyType.Int32, "ExitCode", PropertyIdConstants.TaskPropertyIdStart + 33);
        static PropertyId _TaskValidExitCodes = new PropertyId(StorePropertyType.String, "TaskValidExitCodes", PropertyIdConstants.TaskPropertyIdStart + 34);

        static PropertyId _RequestCancel = new PropertyId(StorePropertyType.CancelRequest, "RequestCancel", PropertyIdConstants.TaskPropertyIdStart + 40);

        static PropertyId _Closed = new PropertyId(StorePropertyType.Boolean, "Closed", PropertyIdConstants.TaskPropertyIdStart + 41);

        static PropertyId _RequeueCount = new PropertyId(StorePropertyType.Int32, "RequeueCount", PropertyIdConstants.TaskPropertyIdStart + 44);
        static PropertyId _AutoRequeueCount = new PropertyId(StorePropertyType.Int32, "AutoRequeueCount", PropertyIdConstants.TaskPropertyIdStart + 45);
        static PropertyId _FailureReason = new PropertyId(StorePropertyType.FailureReason, "FailureReason", PropertyIdConstants.TaskPropertyIdStart + 46);
        static PropertyId _PendingReason = new PropertyId(StorePropertyType.PendingReason, "PendingReason", PropertyIdConstants.TaskPropertyIdStart + 47);
        static PropertyId _FailedNodeId = new PropertyId(StorePropertyType.Int32, "FailedNodeID", PropertyIdConstants.TaskPropertyIdStart + 48);

        static PropertyId _IsParametric = new PropertyId(StorePropertyType.Boolean, "IsParametric", PropertyIdConstants.TaskPropertyIdStart + 50, PropFlags.Obsolete);

        static PropertyId _StartValue = new PropertyId(StorePropertyType.Int32, "StartValue", PropertyIdConstants.TaskPropertyIdStart + 51);
        static PropertyId _EndValue = new PropertyId(StorePropertyType.Int32, "EndValue", PropertyIdConstants.TaskPropertyIdStart + 52);
        static PropertyId _IncrementValue = new PropertyId(StorePropertyType.Int32, "IncrementValue", PropertyIdConstants.TaskPropertyIdStart + 53);
        static PropertyId _InstanceId = new PropertyId(StorePropertyType.Int32, "InstanceId", PropertyIdConstants.TaskPropertyIdStart + 54);
        static PropertyId _InstanceValue = new PropertyId(StorePropertyType.Int32, "InstanceValue", PropertyIdConstants.TaskPropertyIdStart + 55);

        static PropertyId _GroupId = new PropertyId(StorePropertyType.Int32, "GroupId", PropertyIdConstants.TaskPropertyIdStart + 61);

        static PropertyId _Output = new PropertyId(StorePropertyType.String, "Output", PropertyIdConstants.TaskPropertyIdStart + 70);

        static PropertyId _ParentJobState = new PropertyId(StorePropertyType.JobState, "ParentJobState", PropertyIdConstants.TaskPropertyIdStart + 71);

        static PropertyId _TaskId = new PropertyId(StorePropertyType.TaskId, "TaskId", PropertyIdConstants.TaskPropertyIdStart + 72);

        static PropertyId _EncryptedUserBlob = new PropertyId(StorePropertyType.Binary, "EncryptedUserBlob", PropertyIdConstants.TaskPropertyIdStart + 73);
        static PropertyId _UserBlob = new PropertyId(StorePropertyType.String, "UserBlob", PropertyIdConstants.TaskPropertyIdStart + 74);

        static PropertyId _Type = new PropertyId(StorePropertyType.TaskType, "Type", PropertyIdConstants.TaskPropertyIdStart + 80);

        static PropertyId _TotalSubTaskNumber = new PropertyId(StorePropertyType.Int32, "TotalSubTaskNumber", PropertyIdConstants.TaskPropertyIdStart + 90, PropFlags.Calculated);

        static PropertyId _IsServiceConcluded = new PropertyId(StorePropertyType.Boolean, "IsServiceConcluded", PropertyIdConstants.TaskPropertyIdStart + 91);

        static PropertyId _FailJobOnFailure = new PropertyId(StorePropertyType.Boolean, "FailJobOnFailure", PropertyIdConstants.TaskPropertyIdStart + 92);
        static PropertyId _FailJobOnFailureCount = new PropertyId(StorePropertyType.Int32, "FailJobOnFailureCount", PropertyIdConstants.TaskPropertyIdStart + 93);

        static PropertyId _ExitIfPossible = new PropertyId(StorePropertyType.Boolean, "ExitIfPossible", PropertyIdConstants.TaskPropertyIdStart + 94);
        static PropertyId _ExecutionFailureRetryCount = new PropertyId(StorePropertyType.Int32, "ExecutionFailureRetryCount", PropertyIdConstants.TaskPropertyIdStart + 95);
        static PropertyId _RequestedNodeGroup = new PropertyId(StorePropertyType.String, "RequestedNodeGroup", PropertyIdConstants.TaskPropertyIdStart + 96);

    }
}
