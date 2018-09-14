using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Manages the tasks and resources that are associated with a job.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To create a job, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateJob" /> method. </para>
    ///   <para>To clone a job, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.CloneJob(System.Int32)" /> method.</para>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIJobV5)]
    public interface ISchedulerJobV5
    {
        #region V2 methods. Don't change
        /// <summary>
        ///   <para>Creates a task.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interface that you use to define the task.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The initial state of the task is Configuring (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState" />). You can add tasks to the job when the job is in the Configuring state. If the job is running, you can call the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> method to add the task to the running job.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853426(v=vs.85).aspx">Creating and Submitting a Job</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" />
        ISchedulerTask CreateTask();

        /// <summary>
        ///   <para>Adds the task to the job.</para>
        /// </summary>
        /// <param name="task">
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interface of the task to add to the job. To create the task, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTask" /> method.</para>
        /// </param>
        /// <remarks>
        ///   <para>Typically, you call this method while the job is in the Configuring state. After you submit the job (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />),  you must call the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method to add a task to the job if you want the task to run (if you call the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> method after the job is submitted, the task will not run until you submit the task). If you call the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method, you must first call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> method to get a task identifier assigned to the task.</para> 
        ///   <para>You cannot submit a task after the job finishes.</para>
        ///   <para>Unless you specify dependencies for the tasks, the tasks you add to a job do not 
        /// necessarily run in any particular order, except that node preparation and node release tasks are the first and last  
        /// tasks to run on a node, respectively. The HPC Job Scheduler Service tries to run as many tasks 
        /// in parallel as it can, which may preclude the tasks from running in the order in which you added them.</para> 
        ///   <para>For better performance in Windows HPC Server 2008 R2, call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" /> method to add several tasks to a job at once, instead of calling  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> multiple times inside a loop.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853426(v=vs.85).aspx">Creating and Submitting a Job</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTask" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CancelTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" />
        void AddTask(ISchedulerTask task);


        /// <summary>
        ///   <para>Refreshes this copy of the job with the contents from the server.</para>
        /// </summary>
        void Refresh();

        /// <summary>
        ///   <para>Commits to the server any local changes to the job.</para>
        /// </summary>
        /// <remarks>
        ///   <para>All changes made to the job are made locally. Call this method to apply 
        /// your changes to the server. To commit updates, the job must have been previously added to the scheduler.</para>
        ///   <para>If the user changed a property value that the template marks as read-only, the property value is silently set to the template's default 
        /// value. If the user set a property value that is outside the range of the template's constraint on that property, the method fails. Because the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Commit" /> method does not return a value, you should check the values of the properties that you changed before you called  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Commit" /> to check whether 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Commit" /> succeeded or failed.</para>
        ///   <para>If you want to modify a job in the queued state, you may need to call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.ConfigureJob(System.Int32)" /> method to move the job back to the configuring state before you make the changes. You can modify a larger set of properties for jobs in the configuring state than you can for jobs in the queued state.</para> 
        ///   <para>If the job is running, you can modify only the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" />, and 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Project" /> job properties. The changes take effect immediately. If the job is a backfill job, you cannot modify the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" /> property.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Refresh" />
        void Commit();

        /// <summary>
        /// Opens an existing task on the server.
        /// </summary>
        /// <param name="taskId">ID of the task to open on the server.</param>
        /// <returns></returns>
        ISchedulerTask OpenTask(ITaskId taskId);

        /// <summary>
        /// Cancels the task.
        /// </summary>
        /// <param name="taskId">ID of the task to Cancel</param>
        void CancelTask(ITaskId taskId);

        /// <summary>
        /// Requeues a failed or canceled Task.
        /// </summary>
        /// <param name="taskId">ID of the task to Requeue</param>
        void RequeueTask(ITaskId taskId);

        /// <summary>
        /// Submits a task to a running job, where the Task has already been added to the cluster.
        /// </summary>
        /// <param name="taskId"></param>
        void SubmitTaskById(ITaskId taskId);

        /// <summary>
        ///   <para>Submits a task to the job using the specified task.</para>
        /// </summary>
        /// <param name="task">
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interface of the task to add to the job.</para>
        /// </param>
        /// <remarks>
        ///   <para>You can call this method any time after the job is added to the scheduler (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" />). Before the job is added to the scheduler, you must call the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> method to add tasks to the job. After the job is submitted (see  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />), you must call the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method to add a task to the job if you want the task to run (if you call the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> method after the job is submitted, the task will not run until you submit the task). You cannot submit a task after the job finishes.</para> 
        ///   <para>Unless you specify dependencies for the tasks, the tasks you add to a job do not 
        /// necessarily run in any particular order, except that node preparation and node release tasks are the first and last  
        /// tasks to run on a node, respectively. The HPC Job Scheduler Service tries to run as many tasks 
        /// in parallel as it can, which may preclude the tasks from running in the order in which you added them.</para> 
        ///   <para>For better performance in Windows HPC Server 2008 R2, call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" /> method to add several tasks to a job at once, instead of calling the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> method multiple times inside a loop.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTask" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        void SubmitTask(ISchedulerTask task);

        /// <summary>
        ///   <para>Retrieves a list of task objects based on the specified filters.</para>
        /// </summary>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains a collection of one or more filter properties used to filter the list of tasks. If null, the method returns all tasks.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An array of 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains a collection of one or more sort properties used to sort the list of tasks. If null, the list is not sorted.</para> 
        /// </param>
        /// <param name="expandParametric">
        ///   <para>Set to true to include parametric instances in the results; otherwise, false.</para>
        /// </param>
        /// <returns>
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCollection" /> interface that contains a collection of task identifiers (see 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" />) that meet the filter criteria.</para>
        /// </returns>
        /// <remarks>
        ///   <para>If you specify more than one filter, a logical AND is applied 
        /// to the filters (for example, return tasks that are running and have exclusive access to the nodes).</para>
        ///   <para>Only the job owner or administrator can list the tasks in 
        /// a job. The job must have been added to the scheduler before calling this method.</para>
        ///   <para>If you call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" /> method before you add or submit the job to the cluster, the filter properties that you specify in the <paramref name="filter" /> parameter do not get applied, and the method returns all of the tasks in the job.</para> 
        ///   <para>If you call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" /> method with the <paramref name="expandParametric" /> parameter set to  
        /// True before a task in job that has one of the parametric task types starts running, the 
        /// method does not show the instances of that task. The task must be running before a call to  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" /> with the <paramref name="expandParametric" /> parameter set to  
        /// True shows the instances of such a task. The parametric tasks types are 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskType.ParametricSweep" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskType.Service" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskType.NodePrep" />, and 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskType.NodeRelease" />.</para>
        ///   <para>If you use the <paramref name="filter" /> parameter to get tasks based on their 
        /// name, you can only get basic and parametric master tasks that meet the name criteria that the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> object specifies. The names of subtasks of parametric tasks are derived dynamically and are not stored in a database, so the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" /> method cannot apply a filter to them.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853421(v=vs.85).aspx">Cloning a Job</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskIdList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTaskEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        ISchedulerCollection GetTaskList(IFilterCollection filter, ISortCollection sort, bool expandParametric);

        /// <summary>
        ///   <para>Retrieves a list of task identifiers based on the specified filters.</para>
        /// </summary>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains a collection of one or more filter properties used to filter the list of tasks. If null, the method returns all tasks.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An array of 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains a collection of one or more sort properties used to sort the list of tasks. If null, the list is not sorted.</para> 
        /// </param>
        /// <param name="expandParametric">
        ///   <para>Set to true to include parametric instances in the results; otherwise, false.</para>
        /// </param>
        /// <returns>
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCollection" /> interface that contains a collection of task identifiers (see 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" />) that meet the filter criteria.</para>
        /// </returns>
        /// <remarks>
        ///   <para>If you specify more than one filter, a logical AND is applied 
        /// to the filters (for example, return tasks that are running and have exclusive access to the nodes).</para>
        ///   <para>Only the job owner or administrator can list the tasks in 
        /// a job. The job must have been added to the scheduler before calling this method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTaskEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        ISchedulerCollection GetTaskIdList(IFilterCollection filter, ISortCollection sort, bool expandParametric);

        /// <summary>
        ///   <para>Retrieves an enumerator that contains the tasks that match the filter criteria.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the task properties that you want to include for each task in the enumerator.</para> 
        /// </param>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of tasks. If null, the method returns all tasks.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of tasks. If null, the list is not sorted.</para> 
        /// </param>
        /// <param name="expandParametric">
        ///   <para>Set to true to include parametric instances in the results; otherwise, false.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator" /> interface that you can use to enumerate the results.</para>
        /// </returns>
        /// <remarks>
        ///   <para>A property can be null if the property has not been set. Check that the property object is valid before accessing its value.</para>
        ///   <para>If you specify more than one filter, a logical AND is applied to 
        /// the filters. For example, return tasks that are running and have exclusive access to the nodes.</para>
        ///   <para>Only the job owner or administrator can enumerate the tasks in 
        /// a job. The job must have been added to the scheduler before calling this method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskIdList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTaskRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenTaskEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort, bool expandParametric);

        /// <summary>
        ///   <para>Retrieves a rowset that contains the jobs that match the filter criteria.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the task properties that you want to include for each task in the rowset.</para> 
        /// </param>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of tasks. If null, the method returns all tasks.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of tasks. If null, the list is not sorted.</para> 
        /// </param>
        /// <param name="expandParametric">
        ///   <para>Set to true to include parametric instances in the results; otherwise, false.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowSet" /> interface that you use to access the results.</para>
        /// </returns>
        /// <remarks>
        ///   <para>A property can be null if the property has not been set. Check that the property object is valid before accessing its value.</para>
        ///   <para>If you specify more than one filter, a logical AND is applied to 
        /// the filters. For example, return tasks that are running and have exclusive access to the nodes.</para>
        ///   <para>Only the job owner or administrator can get the tasks in 
        /// a job. The job must have been added to the scheduler before calling this method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskIdList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTaskEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        [ComVisible(false)]
        ISchedulerRowSet OpenTaskRowSet(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort, bool expandParametric);

        /// <summary>
        ///   <para>Sets the job template to use for the job.</para>
        /// </summary>
        /// <param name="templateName">
        ///   <para>The name of the template to use for this job.</para>
        /// </param>
        /// <remarks>
        ///   <para>To get the template specified for the job, access the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.JobTemplate" /> property. To get a list of available template, access the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateList" /> property.</para>
        ///   <para>HPC maintains the following three property values for each property:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>The value set by the user</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>The template value if specified by the template</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>A default value</para>
        ///       </description>
        ///     </item>
        ///   </list>
        ///   <para>If the user sets the property value, the user-defined value is used. If not, the template 
        /// value is used. If the user and template do not specify a value, the default property value is used.</para>
        ///   <para>For those properties that are defined in the new and old template, the default values and template values defined in 
        /// the new template override the values for the same properties defined in the old template. All property values changed by the user  
        /// remain unchanged. The default values for any properties defined in the old template that are not also defined in the new template 
        /// are retained. The method does not validate the job's property values against the new template until the scheduler tries to run the job.</para> 
        ///   <para>Job templates are used to set default values, which can be adjusted by subsequent property changes. Therefore, when 
        /// creating a job, the job template should be applied before any property adjustments, including adding node groups. If property adjustments  
        /// are made prior to applying the job template, default values defined in the job template will not be applied, but 
        /// required values will still be checked. If the default value is required and not applied, a job template validation error can result.</para> 
        /// </remarks>
        /// <example />
        void SetJobTemplate(string templateName);

        /// <summary>
        ///   <para>Retrieves the name of the job template used to initialize the properties of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the job template.</para>
        /// </value>
        /// <remarks>
        ///   <para>To specify a new template for the job, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetJobTemplate(System.String)" /> method.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateList" />
        System.String JobTemplate { get; }

        /// <summary>
        ///   <para>Retrieves the names of the nodes that have been allocated to run the tasks in the job or have run the tasks.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains the names of the nodes that have been allocated to run the tasks in the job or have run the tasks.</para> 
        /// </value>
        /// <remarks>
        ///   <para>The list of allocated nodes are available after the job transitions to the Running state.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NodeGroups" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequiredNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.AllocatedNodes" />
        IStringCollection AllocatedNodes { get; }

        /// <summary>
        ///   <para>Retrieves the unique network addresses that a client uses to communicate with a service endpoint.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of endpoint addresses to which the session is connected.</para> 
        /// </value>
        /// <remarks>
        ///   <para>Used only by service jobs. See also Microsoft.Hpc.Scheduler.Session.Session.EndpointReference.</para>
        /// </remarks>
        IStringCollection EndpointAddresses { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        string OrderBy { get; set; }

        /// <summary>
        ///   <para>Overwrites the properties and tasks of the job using the XML at the specified URL.</para>
        /// </summary>
        /// <param name="url">
        ///   <para>The URL that identifies the XML to use to overwrite the contents of the job.</para>
        /// </param>
        /// <remarks>
        ///   <para>Call the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Commit" /> method to commit the changes to the job.</para>
        /// </remarks>
        /// <example />
        void RestoreFromXml(string url);

        /// <summary>
        ///   <para>Overwrites the properties and tasks of the job using the contents from the XML reader.</para>
        /// </summary>
        /// <param name="reader">
        ///   <para>An <see cref="System.Xml.XmlReader" /> that contains the XML used to overwrite the content of the job.</para>
        /// </param>
        /// <remarks>
        ///   <para>Call the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Commit" /> method to commit the changes to the job.</para>
        /// </remarks>
        /// <example />
        [ComVisible(false)]
        void RestoreFromXml(XmlReader reader);

        /// <summary>
        ///   <para>Sets an application-defined property on the job.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The property name. The name is limited to 80 characters.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The property value as a string. The value is limited to 128 characters.</para>
        /// </param>
        /// <remarks>
        ///   <para>Use this method to define your own properties that are passed to the 
        /// submission and activation filters. If the property exists, the <paramref name="value" /> of the property is updated.</para>
        ///   <para>To retrieve the application-defined properties, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetCustomProperties" /> method.</para>
        /// </remarks>
        /// <example />
        void SetCustomProperty(string name, string value);

        /// <summary>
        ///   <para>Retrieves the application-defined properties. </para>
        /// </summary>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains the collection of properties. Each item in the collection is an  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValue" /> interface that contains the property name and value. The collection is empty if no properties have been defined.</para> 
        /// </returns>
        /// <remarks>
        ///   <para>To add application-defined properties to the job, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetCustomProperty(System.String,System.String)" /> method.</para>
        /// </remarks>
        /// <example />
        INameValueCollection GetCustomProperties();

        /// <summary>
        ///   <para>Retrieves the counter data for the job.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters" /> interface that contains the counter data.</para>
        /// </returns>
        ISchedulerJobCounters GetCounters();

        /// <summary>
        ///   <para>An event that is raised when the state of the job changes.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.JobStateHandler" />.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853482(v=vs.85).aspx">Implementing the Event Handlers for Job Events in C#</see>.</para> 
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OnTaskState" />
        event EventHandler<JobStateEventArg> OnJobState;

        /// <summary>
        ///   <para>An event that is raised when the state of one of the tasks in the job changes.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.TaskStateHandler" />.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853482(v=vs.85).aspx">Implementing the Event Handlers for Job Events in C#</see>.</para> 
        ///   <para>If a task state changes very quickly, some intermediate states may not be detected, and will not raise 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OnTaskState" /> events.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OnJobState" />
        event EventHandler<TaskStateEventArg> OnTaskState;

        #endregion

        #region V2 properties. Don't change
        /// <summary>
        ///   <para>Retrieves the job identifier.</para>
        /// </summary>
        /// <value>
        ///   <para>The job identifier.</para>
        /// </value>
        /// <remarks>
        ///   <para>The identifier is set when you call the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" /> method to add the job to the scheduler. The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> method will also set the identifier if the job has not been added to the scheduler.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853429(v=vs.85).aspx">Creating a Parametric Sweep Task</see>.</para>
        /// </example>
        System.Int32 Id { get; }

        /// <summary>
        ///   <para>Retrieves or sets the display name of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The display name. The name is limited to 80 characters.</para>
        /// </value>
        System.String Name { get; set; }

        /// <summary>
        ///   <para>Retrieves the name of the user who created, submitted, or queued the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The user name.</para>
        /// </value>
        System.String Owner { get; }

        /// <summary>
        ///   <para>Retrieves or sets the RunAs user for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The user name in the form, domain\username.</para>
        /// </value>
        /// <remarks>
        ///   <para>You do not need to call this method. The initial value is set to the user that submitted the job to the scheduling queue (see 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />).</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Owner" />
        System.String UserName { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the job priority.</para>
        /// </summary>
        /// <value>
        ///   <para>The job priority. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority" /> enumeration.</para>
        /// </value>
        /// <remarks>
        ///   <para>The Default job template sets the default value to <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority.Normal" />.</para>
        ///   <para>Server resources are allocated to jobs based on job priority, except for backfill jobs. Jobs can be preempted if the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanPreempt" /> property is true; otherwise, jobs run until they finish, fail or are canceled.</para> 
        ///   <para>Within a job, tasks receive resources based on the order in which 
        /// they were added to the job. If a core is available, the task will run.</para>
        ///   <para>In Windows HPC Server 2008 R2, you can use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" /> property to set priority values over a scale of 4000 values instead of 5 values. If you set the values of both the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Priority" /> and 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" /> properties, the property that you set last determines the priority of the job. When you set the value of one of these properties, the value of the other property is automatically updated to the equivalent value.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanPreempt" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" />
        Microsoft.Hpc.Scheduler.Properties.JobPriority Priority { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the project name to associate with the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The project name. The name is limited to 80 characters.</para>
        /// </value>
        /// <remarks>
        ///   <para>The name can be used for accounting purposes.</para>
        /// </remarks>
        /// <example />
        System.String Project { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the run-time limit for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The run-time limit for the job, in seconds.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default value is 0.</para>
        ///   <para>The wall clock is used to determine the run time. The time is your best guess of how long the job will take; however, 
        /// it needs to be fairly accurate because it is used to allocate resources. 
        /// If the job exceeds this time, the job is terminated and its state becomes Canceled.</para> 
        ///   <para>The sum of all 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Runtime" /> values for the tasks in the job should be less than the run-time value for the job.</para> 
        /// </remarks>
        /// <example />
        System.Int32 Runtime { get; set; }

        /// <summary>
        ///   <para>Retrieves the time that the job was submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>The job submit time. The value is in Coordinated Universal Time. The value is 
        /// <see cref="System.DateTime.MinValue" /> if the job has not been submitted (see 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />).</para>
        /// </value>
        System.DateTime SubmitTime { get; }

        /// <summary>
        ///   <para>Retrieves the date and time that the job was created.</para>
        /// </summary>
        /// <value>
        ///   <para>The job creation time. The value is in Coordinated Universal Time.</para>
        /// </value>
        System.DateTime CreateTime { get; }

        /// <summary>
        ///   <para>Retrieves the date and time that job ended.</para>
        /// </summary>
        /// <value>
        ///   <para>The job end time. The value is in Coordinated Universal Time.</para>
        /// </value>
        /// <remarks>
        ///   <para>The end time indicates when the job finished, failed, or was canceled. The value is 
        /// 
        /// <see cref="System.DateTime.MinValue" /> if the job has not finished, failed, or been canceled. The time is reset if you queue a failed or canceled job again.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ChangeTime" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTime" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTime" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.StartTime" />
        System.DateTime EndTime { get; }

        /// <summary>
        ///   <para>Retrieves the date and time that the job started running.</para>
        /// </summary>
        /// <value>
        ///   <para>The job start time. The value is in Coordinated Universal Time.</para>
        /// </value>
        System.DateTime StartTime { get; }

        /// <summary>
        ///   <para>Retrieves the last time that the user or server changed a property of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The date and time that the job was last touched. The value is in Coordinated Universal Time.</para>
        /// </value>
        System.DateTime ChangeTime { get; }

        /// <summary>
        ///   <para>Retrieves the state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the job. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> enumeration.</para>
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.JobState State { get; }

        /// <summary>
        ///   <para>Retrieves the previous state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The previous state of the job. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> enumeration.</para>
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.JobState PreviousState { get; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of cores that the job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of cores.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> job property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" /> to 
        /// False; otherwise, the minimum number of cores that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at least n cores 
        /// to run (the scheduler will not allocate less than this number of cores for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The value cannot exceed the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" /> property.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" />
        System.Int32 MinimumNumberOfCores { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of cores that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of cores.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> job property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" /> to 
        /// False; otherwise, the maximum number of cores that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at most n cores 
        /// to run (the scheduler will not allocate more than this number of cores for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The property value cannot:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Exceed the number of cores in the cluster or the number of cores on the nodes that you requested (with the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" /> property).</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" /> property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" />
        System.Int32 MaximumNumberOfCores { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of nodes that the job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of nodes.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> job property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Node" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" /> to 
        /// False; otherwise, the minimum number of nodes that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at least n nodes 
        /// to run (the scheduler will not allocate less than this number of nodes for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The value cannot exceed the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" /> property.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" />
        System.Int32 MinimumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of nodes that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of nodes.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> job property is Node.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" /> to 
        /// False; otherwise, the maximum number of nodes that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at most n nodes 
        /// to run (the scheduler will not allocate more than this number of nodes for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The value cannot:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Exceed the number of nodes in the cluster or the number of nodes that you requested (with the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" /> property).</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" /> job property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" />
        System.Int32 MaximumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of sockets that the job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of sockets.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> job property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Socket" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" /> to 
        /// False; otherwise, the minimum number of sockets that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at least n sockets 
        /// to run (the scheduler will not allocate less than this number of sockets for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The value cannot exceed the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" /> property.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" />
        System.Int32 MinimumNumberOfSockets { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of sockets that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of sockets.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> job property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Socket" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" /> to 
        /// False; otherwise, the maximum number of sockets that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at most n sockets 
        /// to run (the scheduler will not allocate more than this number of sockets for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The property value cannot:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Exceed the number of sockets in the cluster or the number of sockets on the nodes that you requested (with the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" /> property).</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" /> job property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" />
        System.Int32 MaximumNumberOfSockets { get; set; }

        /// <summary>
        ///   <para>Determines whether cores, nodes, or sockets are used to allocate resources for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The unit type. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType" /> enumeration.</para>
        /// </value>
        /// <remarks>
        ///   <para>The Default job template sets the default value to <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />.</para>
        ///   <para>The resource units that you specify should be based on the threading model that the task uses. Specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" /> if the service is linked to non-thread safe libraries. Specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Node" /> if the task is multithreaded. Specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Socket" /> if the task is single-threaded  and memory-bus intensive.</para>
        ///   <para>If the unit type is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" /> properties to specify the required resources for the job.</para>
        ///   <para>If the unit type is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Socket" />, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" /> properties to specify the required resources for the job.</para>
        ///   <para>If the unit type is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Node" />, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" /> properties to specify the required resources for the job.</para>
        ///   <para>The maximum and minimum values are used unless the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" /> properties are set to 
        /// True, respectively.</para>
        /// </remarks>
        /// <example />
        Microsoft.Hpc.Scheduler.Properties.JobUnitType UnitType { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the list of nodes that are requested for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of node names. The nodes must exist in the cluster.</para> 
        /// </value>
        /// <remarks>
        ///   <para>This is the list of nodes on which your job is 
        /// capable of running. For example, the nodes contain the required software for your job.</para>
        ///   <para>If you also specify node group names in the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NodeGroups" /> property, the job can be run on the intersection of the two lists.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AllocatedNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequiredNodes" />
        IStringCollection RequestedNodes { get; set; }

        /// <summary>
        ///   <para>Determines whether nodes are exclusively allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the nodes are exclusively allocated to the job; otherwise, False. </para>
        /// </value>
        /// <remarks>
        ///   <para>The Default job template sets the default value to False.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsExclusive" />
        System.Boolean IsExclusive { get; set; }

        /// <summary>
        ///   <para>Determines whether the server reserves resources for the job until the job is canceled (even if the job has no active tasks).</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the server reserves resources 
        /// for the job; otherwise, False. The default is False.</para>
        /// </value>
        /// <remarks>
        ///   <para>The Default job template sets the default value to False.</para>
        ///   <para>You cannot set this property to 
        /// True if 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" /> are also set to 
        /// True.</para>
        /// </remarks>
        /// <example />
        System.Boolean RunUntilCanceled { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the names of the node groups that specify the nodes on which the job can run.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of node group names.</para>
        /// </value>
        /// <remarks>
        ///   <para>A node group is typically created to identify a group of nodes with a similar characteristic. For example, nodes that contain 
        /// a specific software license. You can then use this property to identify the nodes instead of having to specify each node using the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" /> property.</para>
        ///   <para>If you specify multiple node groups, the resulting node list is based on the value of the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NodeGroupOp" /> property. For example if group A contains nodes 1, 2, 3, and 4 and group B contains nodes 3, 4, 5, and 6, the resulting list would be.</para> 
        ///   <para>
        ///     <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NodeGroupOp" /> Property Value</para>
        ///   <para>Resulting List of Nodes</para>
        ///   <list type="table">
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp.Intersect" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>3, 4</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp.Union" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>1, 2, 3, 4, 5, 6</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp.Uniform" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>The scheduler checks if sufficient resources are in nodes 1, 2, 3, and 4, and if so, uses only those nodes. If 
        /// not, the scheduler checks if sufficient resources are in nodes 3, 4, 5, and 6, 
        /// and uses them if they are sufficient. If that also fails, then the job remains queued.</para> 
        ///       </description>
        ///     </item>
        ///   </list>
        ///   <para>If you also specify nodes in the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" /> property, the list of nodes on which the job may run is the intersection of the requested node list and the resulting node group list.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetNodeGroupList" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetNodesInNodeGroup(System.String)" />
        IStringCollection NodeGroups { get; set; }

        /// <summary>
        ///   <para>Determines whether the job fails when one of the tasks in the job fails.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the job fails when a task fails; otherwise, False. </para>
        /// </value>
        /// <remarks>
        ///   <para>The Default job template sets the default value to False.</para>
        ///   <para>By default, all tasks in the job will run. However, in some 
        /// cases, if a task fails, it is no longer necessary to run the entire job.</para>
        /// </remarks>
        /// <example />
        System.Boolean FailOnTaskFailure { get; set; }

        /// <summary>
        ///   <para>Determines whether the scheduler automatically calculates the maximum resource value.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the scheduler calculates the 
        /// maximum value; otherwise, False if the application specifies the value.</para>
        /// </value>
        /// <remarks>
        ///   <para>The value of the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property determines which maximum resource value the scheduler calculates (for example,  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" />).</para>
        ///   <para>If you set one of the maximum resource properties, you must set 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" /> to 
        /// False; otherwise, the maximum resource value that you specified will be ignored.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" />
        System.Boolean AutoCalculateMax { get; set; }

        /// <summary>
        ///   <para>Determines whether the scheduler automatically calculates the minimum resource value.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the scheduler calculates the 
        /// minimum value; otherwise, False if the application specifies the value.</para>
        /// </value>
        /// <remarks>
        ///   <para>The value of the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property determines which minimum resource value the scheduler calculates (for example,  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" />).</para>
        ///   <para>If you set one of the minimum resource properties, you must set 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" /> to 
        /// False; otherwise, the maximum resource value that you specified will be ignored.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" />
        System.Boolean AutoCalculateMin { get; set; }

        /// <summary>
        ///   <para>Determines whether the job resources can grow.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the job resources can grow as more resources become available; otherwise, False.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default is True if not specified by a job template.</para>
        ///   <para>The server will allocate more resources to job if it determines that the job will benefit from 
        /// the increase, and if there are more resources available; the server will not grow the resources beyond the maximum requested.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanShrink" />
        System.Boolean CanGrow { get; }

        /// <summary>
        ///   <para>Determines whether the job resources can shrink.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the server releases extra resources 
        /// when they are no longer needed by the job; otherwise, False.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default is True if not specified by a job template.</para>
        ///   <para>The server will remove resources from job if it determines that the job 
        /// no longer needs the resources; the server will not shrink the resources beyond the minimum requested.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanGrow" />
        System.Boolean CanShrink { get; }

        /// <summary>
        ///   <para>Determines whether another job can preempt this job.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if another job can preempt this job; otherwise, False.</para>
        /// </value>
        /// <remarks>
        ///   <para>The Default job template sets the default value to True.</para>
        ///   <para>For details on job preemption, see the PreemptionType configuration parameter in Remarks section of 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" />.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Priority" />
        System.Boolean CanPreempt { get; set; }

        /// <summary>
        ///   <para>Retrieves the job-related error message or job cancellation message.</para>
        /// </summary>
        /// <value>
        ///   <para>The message.</para>
        /// </value>
        /// <remarks>
        ///   <para>The message contains the last message that was set for the job. The message can be a run-time error message or the message passed to the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String)" /> method.</para>
        ///   <para>Check the message if the job state is Failed or Canceled.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ErrorMessage" />
        System.String ErrorMessage { get; }

        /// <summary>
        ///   <para>Determines whether the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" /> job property is set.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the run-time limit is set; otherwise, False.</para>
        /// </value>
        System.Boolean HasRuntime { get; }

        /// <summary>
        ///   <para>Retrieves the number of times that the job has been queued again.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of times that the job has been queued again.</para>
        /// </value>
        /// <remarks>
        ///   <para>The count is incremented each time you call the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.ConfigureJob(System.Int32)" /> method.</para>
        /// </remarks>
        /// <example />
        System.Int32 RequeueCount { get; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum amount of memory that a node must have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum amount of memory, in megabytes, that a node must have for the job to run on it.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default value is 1.</para>
        ///   <para>This property is not used in the scheduling process unless set by the user.</para>
        ///   <para>If you set this property to 1,000, the scheduler will not schedule the job on nodes that have less than 1 GB of RAM.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" />
        System.Int32 MinMemory { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum amount of memory that a node may have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum amount of memory, in megabytes, that a node may have for the job to run on it.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default value is Int.MaxValue.</para>
        ///   <para>This property is not used in the scheduling process unless set by the user.</para>
        ///   <para>If you set this property to 1,000, the scheduler will not schedule the job on nodes that have more than 1 GB of RAM.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" />
        System.Int32 MaxMemory { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of cores that a node must have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of cores on a node that the job requires.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default value is 1.</para>
        ///   <para>This property is not used in the scheduling process unless set by the user.</para>
        ///   <para>If you set this property to 2, the scheduler will not schedule the job on nodes that have less than two cores.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" />
        System.Int32 MinCoresPerNode { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of cores that a node can have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of cores.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default value is Int.MaxValue.</para>
        ///   <para>This property is not used in the scheduling process unless set by the user.</para>
        ///   <para>If you set this property to 2, the scheduler will not schedule the job on nodes that have more than two cores.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinCoresPerNode" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaxMemory" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" />
        System.Int32 MaxCoresPerNode { get; set; }



        /// <summary>
        ///   <para>Retrieves or sets the software licensing requirements for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The licenses that the job requires. The format is string:integer{,string: integer}, where each 
        /// string is the name of an application and each integer represents how many licenses are required.</para>
        /// </value>
        /// <remarks>
        ///   <para>The name of the application must match the license feature name.</para>
        /// </remarks>
        IStringCollection SoftwareLicense { get; set; }


        /// <summary>
        ///   <para>Retrieves the name of the process that created the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the process that created the job.</para>
        /// </value>
        System.String ClientSource { get; set; }

        #endregion

        #region V3 methods Don't change

        /// <summary>
        ///   <para>Sets the job to the finished state and does not run any additional tasks except node release tasks.</para>
        /// </summary>
        /// <remarks>
        ///   <para>You can use this method to finish jobs that have the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" /> property set to 
        /// True when you have an alternate way of determining that the job is finished.</para>
        ///   <para>You can call this method only for jobs that are in the queued or running states.</para>
        ///   <para>The 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Finish" /> method is essentially equivalent to calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String)" /> method, except that the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Finish" /> method sets the state of the job to finished instead of canceled.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String)" />
        void Finish();

        /// <summary>
        ///   <para>Sets the specified environment variable to the specified value in the context of the job.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>A string that specifies the name of the environment variable that you want to set in the context of the job.</para>
        /// </param>
        /// <param name="value">
        ///   <para>A string that specifies the value to which you want to set the 
        /// environment variable. To unset an environment variable in the context of a job, specify an empty string.</para>
        /// </param>
        /// <remarks>
        ///   <para>If you set or unset an environment variable for a job, that environment variable is also set or 
        /// unset for each task in the job unless you override that environment variable setting for the task by calling the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.SetEnvironmentVariable(System.String,System.String)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.SetEnvironmentVariable(System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EnvironmentVariables" />
        void SetEnvironmentVariable(string name, string value);

        /// <summary>
        ///   <para>Gets the environment variables that are set for the job and their values.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> object that represents a set of paired environment variable names and values.</para>
        /// </value>
        /// <remarks>
        ///   <para>Use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetEnvironmentVariable(System.String,System.String)" /> method to set or unset environment variables for a job.</para> 
        ///   <para>You cannot add environment variables to the job by adding 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValue" /> items to the 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> that the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EnvironmentVariables" /> property contains. Use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetEnvironmentVariable(System.String,System.String)" /> method to add environment variables instead.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetEnvironmentVariable(System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EnvironmentVariables" />
        INameValueCollection EnvironmentVariables { get; }

        /// <summary>
        ///   <para>Add the specified tasks to the job.</para>
        /// </summary>
        /// <param name="taskList">
        ///   <para>An array of 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interfaces for the tasks that job want to add to the job. To create the tasks, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTask" /> method once for each task that you want to create.</para>
        /// </param>
        /// <remarks>
        ///   <para>Typically, you call this method while the job is in the Configuring state. After you submit the job by calling the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> method, you must call the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" /> method to add tasks to the job if you want the tasks to run. If you call the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" /> method after the job is submitted, the tasks will not run until you submit the tasks.</para> 
        ///   <para>You cannot submit tasks after the job finishes.</para>
        ///   <para>Unless you specify dependencies for the tasks, the tasks you add to a job do not 
        /// necessarily run in any particular order, except that node preparation and node release tasks are the first and last  
        /// tasks to run on a node, respectively. The HPC Job Scheduler Service tries to run as many tasks 
        /// in parallel as it can, which may preclude the tasks from running in the order in which you added them.</para> 
        ///   <para>For better performance, call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" /> method to add several tasks to a job at once, instead of calling  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> multiple times inside a loop.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTask" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CancelTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        [ComVisible(false)]
        void AddTasks(ISchedulerTask[] taskList);

        /// <summary>
        ///   <para>Submits the specified tasks to the job.</para>
        /// </summary>
        /// <param name="taskList">
        ///   <para>An array of <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interfaces to the tasks that you want to submit.</para>
        /// </param>
        /// <remarks>
        ///   <para>You can call this method any time after you add to the job to the HPC Job Scheduler Service. Before you add the job, you must call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" /> method to add tasks to the job. After you submit the job by calling the  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" /> method, you must call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />, or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" /> method to add tasks to the job if you want the tasks to run. If you call the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" /> method after you submit the job, the tasks do not run until you submit the tasks. You cannot submit tasks after the job finishes.</para> 
        ///   <para>Unless you specify dependencies for the tasks, the tasks you add to a job do not 
        /// necessarily run in any particular order, except that node preparation and node release tasks are the first and last  
        /// tasks to run on a node, respectively. The HPC Job Scheduler Service tries to run as many tasks 
        /// in parallel as it can, which may preclude the tasks from running in the order in which you added them.</para> 
        ///   <para>To submit a single task to the job, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method.</para>
        ///   <para>For better performance, call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" /> method to add several tasks to a job at once, instead of calling the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method multiple times inside a loop.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTask" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[])" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" />
        [ComVisible(false)]
        void SubmitTasks(ISchedulerTask[] taskList);

        /// <summary>
        ///   <para>Sets the earliest date and time until which the HPC Job Scheduler Service should wait until before starting the job.</para>
        /// </summary>
        /// <param name="holdUntil">
        ///   <para>A 
        /// 
        /// <see cref="System.DateTime" /> object that specifies the earlier date and time in the local time zone until which the HPC Job Scheduler Service should wait before running the job. The date and time must be in the future.</para> 
        /// </param>
        /// <remarks>
        ///   <para>The HPC Job Scheduler Service only runs the job at the date 
        /// and time that you specify in the <paramref name="holdUntil" /> parameter if the resources needed  
        /// for the job are available. If the resources needed for the job are not 
        /// available at that date and time, the job remains queued until the necessary resources become available.</para> 
        ///   <para>You can only call this method once you submit the job and it is waiting 
        /// in the queue. While the job is on hold, HPC Cluster Manager displays the following message for the job:</para>
        ///   <para>
        /// The job is pending: This job is being held by the administrator.
        ///           </para>
        ///   <para>To unset the date and time that the HPC Job Scheduler Service should wait until 
        /// before running the job, which removes the hold on the job and allows it to run, call the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearHold" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearHold" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.HoldUntil" />

        void SetHoldUntil(DateTime holdUntil);

        /// <summary>
        ///   <para>Removes the hold on the job by clearing the date and 
        /// time that the HPC Job Scheduler Service should wait until before running the job.</para>
        /// </summary>
        /// <remarks>
        ///   <para>If you call this method for a job that is not currently on hold, the method has no effect.</para>
        ///   <para>The 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearHold" /> method does not return a value. To confirm that the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearHold" /> method succeeded, check the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.HoldUntil" /> property.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetHoldUntil(System.DateTime)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.HoldUntil" />

        void ClearHold();

        /// <summary>
        /// Cancels the task.
        /// </summary>
        /// <param name="taskId">ID of the task to Cancel</param>
        /// <param name="message">Message to be saved on the task object once the task has been canceled.  Can be null</param>
        [ComVisible(true)]
        void CancelTask(ITaskId taskId, string message);

        /// <summary>
        /// Cancels the task.
        /// </summary>
        /// <param name="taskId">ID of the task to Cancel</param>
        /// <param name="message">Message to be saved on the task object once the task has been canceled.  Can be null</param>
        /// <param name="isForce">Should the task be force cancelled ?</param>
        void CancelTask(ITaskId taskId, string message, bool isForce);

        /// <summary>
        ///   <para>Adds the specified nodes to the list of nodes that should not be used for the job.</para>
        /// </summary>
        /// <param name="nodeNames">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> object that specifies the names of the nodes that you want to add to the list of nodes on which the job should not run.</para> 
        /// </param>
        /// <remarks>
        ///   <para>If you add a node to the list of nodes that should not be used for the job while the job 
        /// is running on that node, the tasks in the job that are running on the node are canceled and then requeued if the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsRerunnable" /> property for the task is true.</para>
        ///   <para>If a node is specified in the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequiredNodes" /> property for any of the tasks in the job, an exception occurs if you also specify that node when you call the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method.</para>
        ///   <para>If you call 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> and specify a set of nodes that would cause the set of available resources to become smaller than the minimum number of resources that the job requires, an exception occurs. For example, if you have an HPC cluster than consists of three nodes and you include two of them in the <paramref name="nodeNames" /> parameter when you call  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />, that action makes only one node available, and an exception occurs if the job requires a minimum of two nodes.</para> 
        ///   <para>If you specify the name of a node that does not currently belong to the HPC cluster, the method generates an exception.</para>
        ///   <para>If you add the same node twice to the list of nodes that should 
        /// not be used for the job, the second time that you add the node has no effect.</para>
        ///   <para>This method succeeds if and only if it does not generate an exception. 
        /// This method does not return a value. To verify that the method succeeded, use the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExcludedNodes" /> property to get the full list of nodes that should not be used for the job.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearExcludedNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExcludedNodes" />
        void AddExcludedNodes(IStringCollection nodeNames);

        /// <summary>
        ///   <para>Removes the specified nodes from the list of nodes that should not be used for the job.</para>
        /// </summary>
        /// <param name="nodeNames">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> object that specifies the names of the nodes that you want to remove from the list of nodes on which the job should not run.</para> 
        /// </param>
        /// <remarks>
        ///   <para>To remove all of the nodes on the list of nodes that should not be used for the job from that list, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearExcludedNodes" /> method.</para>
        ///   <para>If you specify a node that does not currently belong to the HPC cluster, an exception occurs. If you specify a node that 
        /// is part of the HPC cluster but is not part of the current list of nodes that should not be used for the job, the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method has no effect and no error occurs.</para> 
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method does not return a value. To verify that the method succeeded, check the value of the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExcludedNodes" /> property.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearExcludedNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExcludedNodes" />
        void RemoveExcludedNodes(IStringCollection nodeNames);

        /// <summary>
        ///   <para>Removes all of the nodes in the list of nodes that should not be used for the job from that list.</para>
        /// </summary>
        /// <remarks>
        ///   <para>To remove specific nodes from the list of nodes that should not be used for the job, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method.</para>
        ///   <para>The 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearExcludedNodes" /> method does not return a value. To confirm that the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearExcludedNodes" /> method succeeded, check the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExcludedNodes" /> property.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExcludedNodes" />
        void ClearExcludedNodes();

        #endregion

        #region V3 properties Don't change

        /// <summary>
        ///   <para>Gets or sets the percentage of the job that is complete.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Int32" /> that specifies the percentage of the job that is complete. It is a value between 0 and 100.</para>
        /// </value>
        /// <remarks>
        ///   <para>If your application does not set the value of this property, the HPC 
        /// Job Scheduler Service calculates the progress based on the percentage of tasks that are complete for  
        /// the job. Once you application sets this property for a job, the HPC Job Scheduler 
        /// Service does not continue to update this property, and you application must continue to update the property.</para> 
        ///   <para>You can set the value of this property for jobs in any state.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example that uses this property, see <see href="http://go.microsoft.com/fwlink/?LinkID=177608">Setting Custom Job Progress 
        /// Information from Applications or Scripts that Run 
        /// on a Windows HPC Server 2008 R2 Cluster</see> (http://go.microsoft.com/fwlink/?LinkID=177608).</para> 
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ProgressMessage" />
        System.Int32 Progress { get; set; }

        /// <summary>
        ///   <para>Gets or sets a custom status message for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> object that contains the custom status message. The maximum length of the message is 80 characters.</para>
        /// </value>
        /// <remarks>
        ///   <para>You can set the value of this property for jobs in any state.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example that uses this property, see <see href="http://go.microsoft.com/fwlink/?LinkID=177608">Setting Custom Job Progress 
        /// Information from Applications or Scripts that Run 
        /// on a Windows HPC Server 2008 R2 Cluster</see> (http://go.microsoft.com/fwlink/?LinkID=177608).</para> 
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Progress" />
        System.String ProgressMessage { get; set; }

        /// <summary>
        ///   <para>Gets or sets the maximum number of resources that a job can use dynamically, 
        /// so that the HPC Job Scheduler Service does not allocate more resources than the job can use.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="System.Int32" /> that specifies the maximum number of resources that a job can use.</para>
        ///   <para>The possible values are from the minimum number of resources that are requested for the job through the maximum number 
        /// of resources that are requested for the job, and 0. Specify 0 to indicate that the resource count should not be adjusted dynamically. </para>
        ///   <para>If you specify a value that is outside the range of resources 
        /// that are requested for the job, and it is not 0, the following behaviors result:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>An exception occurs.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>The target maximum resource count for the job does not change.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>The job continues to run.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </value>
        /// <remarks>
        ///   <para>This property is used mostly by jobs that contain service tasks that run service-oriented architecture (SOA) services. For such 
        /// jobs, the broker node sets and maintains the value of this property based on the number of outstanding messages for the session.</para>
        ///   <para>The type of resource that the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.TargetResourceCount" /> property applies to depends on the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property. For example, if 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.TargetResourceCount" /> is 3 and 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Node" />, then the job currently can use a maximum of three nodes. If 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.TargetResourceCount" /> is 3 and 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />, then the job currently can use a maximum of three cores.</para>
        ///   <para>This property is updated periodically based on the RebalancingInterval cluster parameter. 
        /// During an adjustment, the broker updates job properties and evaluates the value of the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.TargetResourceCount" /> property for a session, based on the load sampling metrics that are gathered within the adjustment interval. The HPC Job Scheduler Service uses the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.TargetResourceCount" /> property to help determine how many service hosts to allocate to the session. </para> 
        ///   <para>If a SOA client sets the value of the property for a service job, the broker resets it if allocation adjustment is enabled.</para>
        /// </remarks>
        System.Int32 TargetResourceCount { get; set; }

        /// <summary>
        ///   <para>Gets or sets the priority of the job, using the expanded range of priority values in Windows HPC Server 2008 R2.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="System.Int32" /> between 0 and 4000 that indicates the priority for the job, where 0 is the lowest priority and 4000 is the highest.</para> 
        /// </value>
        /// <remarks>
        ///   <para>Use the fields of the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority" /> class to specify the numeric equivalents in Windows HPC Server 2008 R2 for named priority values such as BelowNormal and Highest. You can add or subtract values from these constants to specify an offset from the named value. For example,  
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.Normal" /> + 500.</para>
        ///   <para>If you set the value of 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority" /> to a value less than 0 or a value greater than 4000, an exception occurs.</para> 
        ///   <para>If you set the values of both the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" /> and 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Priority" /> properties, the property that you set last determines the priority of the job. When you set the value of one of these properties, the value of the other property is automatically updated to the equivalent value.</para> 
        ///   <para>The Default job template sets the default value to <see cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.Normal" />.</para>
        ///   <para>The job template for the job determines that values the a user can set for 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" /> without administrative privileges. A cluster administrator cannot submit a job with an  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" /> that the job template does not allow, but when the job is in the queued or running state, the cluster administrator can set  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" /> to any value between 0 and 4000, even if the job template does not allow that expanded priority.</para> 
        ///   <para>Server resources are allocated to jobs based on job priority, except for backfill jobs. Jobs can be preempted if the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanPreempt" /> property is true; otherwise, jobs run until they finish, fail, or are canceled.</para> 
        ///   <para>Within a job, tasks receive resources based on the order in which 
        /// they were added to the job. If a core is available, the task will run.</para>
        ///   <para>See also Microsoft.Hpc.Scheduler.Session.SessionStartInfo.SessionPriority</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.BelowNormal" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Priority" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.Highest" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPriority" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.JobPriorityToExpandedPriority(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.Lowest" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.AboveNormal" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.Normal" />
        System.Int32 ExpandedPriority { get; set; }

        /// <summary>
        ///   <para>Gets or sets the name of the SOA service that the service tasks in the job use, if the job contains service tasks.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.String" /> that specifies the name of the SOA service that the service tasks in the job use, if the job contains service tasks.</para> 
        /// </value>
        System.String ServiceName { get; set; }


        /// <summary>
        ///   <para>Gets the date and time in Coordinated Universal Time until 
        /// which the HPC Job Scheduler Service should wait before trying to start the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.DateTime" /> object that indicates the date and time until which the HPC Job Scheduler Service should wait before trying to start the job.</para> 
        /// </value>
        /// <remarks>
        ///   <para>The HPC Job Scheduler Service only runs the job at the date and time that this property specifies if the resources needed for the 
        /// job are available. If the resources needed for the job are not available 
        /// at that date and time, the job remains queued until the necessary resources become available.</para> 
        ///   <para>If the job is not on hold, the date and time that this property indicates is midnight on January 1, 1.</para>
        ///   <para>To set the value of this property, call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetHoldUntil(System.DateTime)" /> method. To remove that value that is set for this property, call the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearHold" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetHoldUntil(System.DateTime)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearHold" />
        System.DateTime HoldUntil { get; }

        /// <summary>
        ///   <para>Gets or sets whether or not you want to receive email notification when then job starts.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Boolean" /> the indicates whether or not you want to receive email notification when then job starts. 
        /// True indicates that you want to receive email notification when then job starts. 
        /// False indicates that you do not want to receive email notification when then job starts.</para>
        /// </value>
        /// <remarks>
        ///   <para>A cluster administrator must configure notification for the HPC cluster before you can receive notification about a job.</para>
        ///   <para>By default, you do not receive notification when a job starts.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NotifyOnCompletion" />
        System.Boolean NotifyOnStart { get; set; }

        /// <summary>
        ///   <para>Gets or sets whether or not you want to receive email notification when then job ends.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Boolean" /> the indicates whether or not you want to receive email notification when then job ends. 
        /// True indicates that you want to receive email notification when then job ends. 
        /// False indicates that you do not want to receive email notification when then job ends.</para>
        /// </value>
        /// <remarks>
        ///   <para>A job ends and notification is sent when the state of the job changes to finished, failed, or canceled.</para>
        ///   <para>A cluster administrator must configure notification for the HPC cluster before you can receive notification about a job.</para>
        ///   <para>By default, you do not receive notification when a job ends.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NotifyOnStart" />
        System.Boolean NotifyOnCompletion { get; set; }

        /// <summary>
        ///   <para>Gets the list of nodes that should not be used for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> that contains the names of the nodes on which the job should not run.</para>
        /// </value>
        /// <remarks>
        ///   <para>You cannot add nodes to or remove nodes from the set of nodes that should 
        /// not be used for the job by adding node names to or removing node names from the  
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> that the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExcludedNodes" /> property contains. To add nodes to the set, use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method. To remove individual nodes from the set, use the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method. To remove all of the nodes from the set, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method.</para>
        ///   <para>The limit to the number of nodes which can be excluded is set by 
        /// the ExcludedNodesLimit parameter of the <see href="http://technet.microsoft.com/library/ff950175.aspx">Set-HpcClusterProperty</see> property. 
        /// This limit can be modified by the cluster administrator.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ClearExcludedNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        IStringCollection ExcludedNodes { get; }

        #endregion

        #region V3 SP1 Methods Don't change

        /// <summary>
        ///   <para>Retrieves an enumerator that contains the allocation history for the job.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the allocation properties that you want to include for each job allocation in the enumerator.</para> 
        /// </param>
        /// <returns>
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator" /> interface that you can use to enumerate the allocations of nodes to the job.</para>
        /// </returns>
        /// <remarks>
        ///   <para>A property can be null if the property has 
        /// not been set. Check that the property object is valid before accessing its value.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenJobAllocationHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.AllocationProperties" />
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenJobAllocationHistoryEnumerator(IPropertyIdCollection properties);

        /// <summary>
        ///   <para>Retrieves an enumerator that contains the allocation history for all of the tasks in the job. For a task that includes 
        /// subtasks, the enumerator contains allocation history entries for all of the subtasks in the task in the place of an entry for the task.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the allocation properties that you want to include for each task or subtask allocation in the enumerator.</para> 
        /// </param>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator" /> interface that you can use to enumerate the allocations of nodes to the tasks and subtasks.</para> 
        /// </returns>
        /// <remarks>
        ///   <para>A property can be null if the property has not been set. Check that the property object is valid before accessing its value.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTaskAllocationHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.AllocationProperties" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTaskEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" 
        /// /> 
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenTaskAllocationHistoryEnumerator(IPropertyIdCollection properties);

        #endregion

        #region V3 SP1 Properties Don't change

        /// <summary>
        ///   <para>Gets or sets the email address to which the HPC Job Scheduler Service should send notifications when the job starts or finishes.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.String" /> that indicates the email address to which the HPC Job Scheduler Service should send notifications when the job starts or finishes.</para> 
        /// </value>
        /// <remarks>
        ///   <para>The maximum length for the email address that you specify is 256 characters. 
        /// The HPC Job Scheduler Service sends notifications to the user who created the job by default.</para>
        ///   <para>When you specify an email address for receiving notifications about a 
        /// job, the notifications are not turned on automatically. You still need to set the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NotifyOnStart" /> and/or 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NotifyOnCompletion" /> property to 
        /// True to specify when you want to receive notifications about the job.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NotifyOnStart" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NotifyOnCompletion" />
        System.String EmailAddress { get; set; }

        #endregion

        #region V3 SP2 Properties Don't change

        /// <summary>
        ///   <para>Returns the name of the job’s pool.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> object that contains the name of the job’s pool.</para>
        /// </value>
        System.String Pool { get; }

        #endregion

        #region V3 SP3 Properties

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [ComVisible(false)]
        Microsoft.Hpc.Scheduler.Properties.JobRuntimeType RuntimeType { get; set; }

        #endregion

        #region V4 properties. Don't change.

        /// <summary>
        ///   <para>Gets or sets the exit codes to be used for checking whether tasks in the job successfully exit.</para>
        /// </summary>
        /// <value>
        ///   <para>The exit codes that indicate whether tasks in the job successfully exited.</para>
        /// </value>
        /// <remarks>
        ///   <para>Specifies the exit codes to be used for checking whether tasks in the job successfully exit. These codes will only 
        /// apply to tasks that do not specify their own success exit codes. You can specify discrete integers and integer ranges separated by commas.</para>
        ///   <para>Integer ranges are denoted by integers separated by two 
        /// periods. For example, <c>10..20</c> represents an integer range from ten to twenty.</para>
        ///   <para>
        ///     min and max may be 
        /// used to specify minimum and maximum integer values. For example, <c>0..max</c> represents nonnegative integers.</para>
        /// </remarks>
        System.String ValidExitCodes { get; set; }

        /// <summary>
        ///   <para>Gets or sets a value that indicates whether all resources such as cores or sockets should be allocated on one node.</para>
        /// </summary>
        /// <value>
        ///   <para>
        ///     True if all resources should be allocated on one node; otherwise, False. The default is false.</para>
        /// </value>
        bool SingleNode { get; set; }

        /// <summary>
        ///   <para>Gets or sets the operator for the node group.</para>
        /// </summary>
        /// <value>
        ///   <para>The operator for the node group.</para>
        /// </value>
        /// <remarks>
        ///   <para>The 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NodeGroupOp" /> property stores the group operation, for this job which is a member of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp" /> enum. Valid values are 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp.Intersect" /> which signifies that nodes belong to all of the specified node groups, 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp.Uniform" /> which signifies that nodes belong to only one of the specified node groups, and  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp.Union" /> which signifies that nodes belong to any of the specified node groups.</para> 
        /// </remarks>
        JobNodeGroupOp NodeGroupOp { get; set; }

        /// <summary>
        ///   <para>Gets or sets the list of parent job IDs.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns an <see cref="Microsoft.Hpc.Scheduler.IIntCollection" /> object which contains the collection of parent job IDs.</para>
        /// </value>
        /// <remarks>
        ///   <para>Child jobs will not begin until all parent jobs have completed.</para>
        /// </remarks>
        IIntCollection ParentJobIds { get; set; }

        /// <summary>
        ///   <para>Gets or sets the list of child job IDs.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns an <see cref="Microsoft.Hpc.Scheduler.IIntCollection" /> object which is the collection of child job IDs.</para>
        /// </value>
        /// <remarks>
        ///   <para>Child jobs will not begin until all parent jobs have completed.</para>
        /// </remarks>
        IIntCollection ChildJobIds { get; }

        /// <summary>
        ///   <para>Gets or sets a value indicating whether child tasks should be marked as failed if the current task fails.</para>
        /// </summary>
        /// <value>
        ///   <para>
        ///     True if child tasks should be marked as failed if the current task fails; otherwise, False.</para>
        /// </value>
        bool FailDependentTasks { get; set; }

        /// <summary>
        ///   <para>Gets or sets the estimate of the maximum amount of memory the job will consume.</para>
        /// </summary>
        /// <value>
        ///   <para>The estimated number of megabytes of memory the job will require.</para>
        /// </value>
        /// <remarks>
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.EstimatedProcessMemory" /> property is used by the job scheduler to assign jobs to nodes with sufficient memory to execute the job.</para> 
        /// </remarks>
        int EstimatedProcessMemory { get; set; }

        #endregion

        #region V4SP1 properties, Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        int PlannedCoreCount { get; }

        #endregion

        #region V4SP1 methods, Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        void Requeue();

        #endregion

        #region V4SP2 methods, Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="url">
        ///   <para />
        /// </param>
        /// <param name="includeTaskGroup">
        ///   <para />
        /// </param>
        void RestoreFromXmlEx(string url, bool includeTaskGroup);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="reader">
        ///   <para />
        /// </param>
        /// <param name="includeTaskGroup">
        ///   <para />
        /// </param>
        [ComVisible(false)]
        void RestoreFromXmlEx(XmlReader reader, bool includeTaskGroup);

        #endregion

        #region V4SP3 methods, Don't change

        /// <summary>
        /// <para>Finish a task directly. So the task will be never requeued.</para>
        /// </summary>
        /// <param name="taskId">the ID of the task to finish</param>
        /// <param name="message">the message to finish the task.</param>
        void FinishTask(ITaskId taskId, string message);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        int TaskExecutionFailureRetryLimit { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="isForced">
        ///   <para />
        /// </param>
        /// <param name="isGraceful">
        ///   <para />
        /// </param>
        void Finish(bool isForced, bool isGraceful);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="isForced">
        ///   <para />
        /// </param>
        /// <param name="isGraceful">
        ///   <para />
        /// </param>
        void Cancel(bool isForced, bool isGraceful);

        #endregion

        #region V4SP5 methods / properties

        /// <summary>
        /// <para>Finish the task by system id.</para>
        /// </summary>
        /// <param name="taskSystemId">the system unique task id.</param>
        /// <param name="message">the message of cancelling.</param>
        /// <param name="isForced">true if forced to cancel the task</param>
        /// <param name="callback">the call back when the async completes.</param>
        /// <param name="state">the state object</param>
        IAsyncResult BeginFinishTask(int taskSystemId, string message, bool isForced, AsyncCallback callback, object state);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="ar">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        TaskState EndFinishTask(IAsyncResult ar);

        #endregion
    }
}
