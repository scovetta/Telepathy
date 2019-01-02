namespace Microsoft.Hpc.Scheduler.Session.SchedulerPort
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Xml;

    using Microsoft.Hpc.Scheduler.Session.Data;

    // TODO: Trim the interface down to miminal
    public interface ISchedulerJob
    {
        #region V2 methods. Don't change

        /// <summary>
        ///   <para>Creates a task.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="ISchedulerTask" /> interface that you use to define the task.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The initial state of the task is Configuring (see 
        /// 
        /// <see cref="TaskState" />). You can add tasks to the job when the job is in the Configuring state. If the job is running, you can call the  
        /// 
        /// <see cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" /> method to add the task to the running job.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853426(v=vs.85).aspx">Creating and Submitting a Job</see>.</para>
        /// </example>
        /// <seealso cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" />
        /// <seealso cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" />
        ISchedulerTask CreateTask();

        /// <summary>
        ///   <para>Adds the task to the job.</para>
        /// </summary>
        /// <param name="task">
        ///   <para>An 
        /// <see cref="ISchedulerTask" /> interface of the task to add to the job. To create the task, call the 
        /// <see cref="ISchedulerJob{ITaskId}.CreateTask" /> method.</para>
        /// </param>
        /// <remarks>
        ///   <para>Typically, you call this method while the job is in the Configuring state. After you submit the job (see 
        /// 
        /// <see cref="ISchedISchedulerJob{ITaskId}r.ISchedulerJob,System.String,System.String)" />),  you must call the  
        /// <see cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method to add a task to the job if you want the task to run (if you call the  
        /// 
        /// <see cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" /> method after the job is submitted, the task will not run until you submit the task). If you call the  
        /// <see cref="ISchedulerJob{ITaskId}.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method, you must first call the 
        /// 
        /// <see cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" /> method to get a task identifier assigned to the task.</para> 
        ///   <para>You cannot submit a task after the job finishes.</para>
        ///   <para>Unless you specify dependencies for the tasks, the tasks you add to a job do not 
        /// necessarily run in any particular order, except that node preparation and node release tasks are the first and last  
        /// tasks to run on a node, respectively. The HPC Job Scheduler Service tries to run as many tasks 
        /// in parallel as it can, which may preclude the tasks from running in the order in which you added them.</para> 
        ///   <para>For better performance in Windows HPC Server 2008 R2, call the 
        /// 
        /// <see cref="ISchedulerJob{ITaISchedulerTask.ISchedulerTask[])" /> method to add several tasks to a job at once, instead of calling  
        /// <see cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" /> multiple times inside a loop.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853426(v=vs.85).aspx">Creating and Submitting a Job</see>.</para>
        /// </example>
        /// <seealso cref="ISchedulerJob{ITaISchedulerTask.ISchedulerTask[])" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CreateTask" />
        /// <seealso cref="ISchedulerJob{ITaskId}.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CancelTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        /// <seealso cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" />
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
        /// <see cref="ISchedulerJob{ITaskId}.Commit" /> method does not return a value, you should check the values of the properties that you changed before you called  
        /// <see cref="ISchedulerJob{ITaskId}.Commit" /> to check whether 
        /// <see cref="ISchedulerJob{ITaskId}.Commit" /> succeeded or failed.</para>
        ///   <para>If you want to modify a job in the queued state, you may need to call the 
        /// 
        /// <see cref="IScheduler.ConfigureJob(System.Int32)" /> method to move the job back to the configuring state before you make the changes. You can modify a larger set of properties for jobs in the configuring state than you can for jobs in the queued state.</para> 
        ///   <para>If the job is running, you can modify only the 
        /// <see cref="ISchedulerJob{ITaskId}.Runtime" />, 
        /// <see cref="ISchedulerJob{ITaskId}.RunUntilCanceled" />, and 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.Project" /> job properties. The changes take effect immediately. If the job is a backfill job, you cannot modify the  
        /// <see cref="ISchedulerJob{ITaskId}.Runtime" /> property.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITaskId}.Refresh" />
        void Commit();

        /// <summary>
        /// Opens an existing task on the server.
        /// </summary>
        /// <param name="taskId">ID of the task to open on the server.</param>
        /// <returns></returns>
        ISchedulerTask OpenTask(string taskId);

        /// <summary>
        /// Cancels the task.
        /// </summary>
        /// <param name="taskId">ID of the task to Cancel</param>
        void CancelTask(string taskId);

        /// <summary>
        /// Requeues a failed or canceled Task.
        /// </summary>
        /// <param name="taskId">ID of the task to Requeue</param>
        void RequeueTask(string taskId);

        /// <summary>
        /// Submits a task to a running job, where the Task has already been added to the cluster.
        /// </summary>
        /// <param name="taskId"></param>
        void SubmitTaskById(string taskId);

        /// <summary>
        ///   <para>Submits a task to the job using the specified task.</para>
        /// </summary>
        /// <param name="task">
        ///   <para>An <see cref="ISchedulerTask" /> interface of the task to add to the job.</para>
        /// </param>
        /// <remarks>
        ///   <para>You can call this method any time after the job is added to the scheduler (see 
        /// 
        /// <see cref="IScISchedulerJob{ITaskId}r.ISchedulerJob)" />). Before the job is added to the scheduler, you must call the  
        /// 
        /// <see cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" /> method to add tasks to the job. After the job is submitted (see  
        /// 
        /// <see cref="ISchedISchedulerJob{ITaskId}r.ISchedulerJob,System.String,System.String)" />), you must call the  
        /// <see cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method to add a task to the job if you want the task to run (if you call the  
        /// 
        /// <see cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" /> method after the job is submitted, the task will not run until you submit the task). You cannot submit a task after the job finishes.</para> 
        ///   <para>Unless you specify dependencies for the tasks, the tasks you add to a job do not 
        /// necessarily run in any particular order, except that node preparation and node release tasks are the first and last  
        /// tasks to run on a node, respectively. The HPC Job Scheduler Service tries to run as many tasks 
        /// in parallel as it can, which may preclude the tasks from running in the order in which you added them.</para> 
        ///   <para>For better performance in Windows HPC Server 2008 R2, call the 
        /// 
        /// <see cref="ISchedulerJob{ITaskIISchedulerTask.ISchedulerTask[])" /> method to add several tasks to a job at once, instead of calling the  
        /// 
        /// <see cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" /> method multiple times inside a loop.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CreateTask" />
        /// <seealso cref="ISchedulerJob{ITaskId}.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        void SubmitTask(ISchedulerTask task);

        /// <summary>
        ///   <para>Sets the job template to use for the job.</para>
        /// </summary>
        /// <param name="templateName">
        ///   <para>The name of the template to use for this job.</para>
        /// </param>
        /// <remarks>
        ///   <para>To get the template specified for the job, access the 
        /// <see cref="ISchedulerJob{ITaskId}.JobTemplate" /> property. To get a list of available template, access the 
        /// <see cref="IScheduler.GetJobTemplateList" /> property.</para>
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
        /// <see cref="ISchedulerJob{ITaskId}.SetJobTemplate(System.String)" /> method.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="IScheduler.GetJobTemplateList" />
        string JobTemplate { get; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.NodeGroups" />
        /// <seealso cref="ISchedulerJob{ITaskId}.RequestedNodes" />
        /// <seealso cref="ISchedulerTask.RequiredNodes" />
        /// <seealso cref="ISchedulerTask.AllocatedNodes" />
        ICollection<string> AllocatedNodes { get; }

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
        ICollection<string> EndpointAddresses { get; }

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
        ///   <para>Call the <see cref="ISchedulerJob{ITaskId}.Commit" /> method to commit the changes to the job.</para>
        /// </remarks>
        /// <example />
        void RestoreFromXml(string url);

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
        /// <see cref="ISchedulerJob{ITaskId}.GetCustomProperties" /> method.</para>
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
        /// <see cref="ISchedulerJob{ITaskId}.SetCustomProperty(System.String,System.String)" /> method.</para>
        /// </remarks>
        /// <example />
        IDictionary<string, string> GetCustomProperties();

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
        /// <see cref="IScISchedulerJob{ITaskId}r.ISchedulerJob)" /> method to add the job to the scheduler. The 
        /// 
        /// <see cref="ISchedISchedulerJob{ITaskId}r.ISchedulerJob,System.String,System.String)" /> method will also set the identifier if the job has not been added to the scheduler.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853429(v=vs.85).aspx">Creating a Parametric Sweep Task</see>.</para>
        /// </example>
        int Id { get; }

        /// <summary>
        ///   <para>Retrieves or sets the display name of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The display name. The name is limited to 80 characters.</para>
        /// </value>
        string Name { get; set; }

        /// <summary>
        ///   <para>Retrieves the name of the user who created, submitted, or queued the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The user name.</para>
        /// </value>
        string Owner { get; }

        /// <summary>
        ///   <para>Retrieves or sets the RunAs user for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The user name in the form, domain\username.</para>
        /// </value>
        /// <remarks>
        ///   <para>You do not need to call this method. The initial value is set to the user that submitted the job to the scheduling queue (see 
        /// <see cref="ISchedISchedulerJob{ITaskId}r.ISchedulerJob,System.String,System.String)" />).</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITaskId}.Owner" />
        string UserName { get; set; }

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
        string Project { get; set; }

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
        /// <see cref="ISchedulerTask.Runtime" /> values for the tasks in the job should be less than the run-time value for the job.</para> 
        /// </remarks>
        /// <example />
        int Runtime { get; set; }

        /// <summary>
        ///   <para>Retrieves the time that the job was submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>The job submit time. The value is in Coordinated Universal Time. The value is 
        /// <see cref="System.DateTime.MinValue" /> if the job has not been submitted (see 
        /// <see cref="ISchedISchedulerJob{ITaskId}r.ISchedulerJob,System.String,System.String)" />).</para>
        /// </value>
        DateTime SubmitTime { get; }

        /// <summary>
        ///   <para>Retrieves the date and time that the job was created.</para>
        /// </summary>
        /// <value>
        ///   <para>The job creation time. The value is in Coordinated Universal Time.</para>
        /// </value>
        DateTime CreateTime { get; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.ChangeTime" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CreateTime" />
        /// <seealso cref="ISchedulerJob{ITaskId}.SubmitTime" />
        /// <seealso cref="ISchedulerJob{ITaskId}.StartTime" />
        DateTime EndTime { get; }

        /// <summary>
        ///   <para>Retrieves the date and time that the job started running.</para>
        /// </summary>
        /// <value>
        ///   <para>The job start time. The value is in Coordinated Universal Time.</para>
        /// </value>
        DateTime StartTime { get; }

        /// <summary>
        ///   <para>Retrieves the last time that the user or server changed a property of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The date and time that the job was last touched. The value is in Coordinated Universal Time.</para>
        /// </value>
        DateTime ChangeTime { get; }

        /// <summary>
        ///   <para>Retrieves the state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the job. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> enumeration.</para>
        /// </value>
        JobState State { get; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of cores that the job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of cores.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> job property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMin" /> to 
        /// False; otherwise, the minimum number of cores that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at least n cores 
        /// to run (the scheduler will not allocate less than this number of cores for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="ISchedulerJob{ITaskId}.CanGrow" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The value cannot exceed the value of the <see cref="ISchedulerJob{ITaskId}.MaximumNumberOfCores" /> property.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.MinCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfNodes" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfSockets" />
        int MinimumNumberOfCores { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of cores that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of cores.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> job property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMax" /> to 
        /// False; otherwise, the maximum number of cores that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at most n cores 
        /// to run (the scheduler will not allocate more than this number of cores for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="ISchedulerJob{ITaskId}.CanGrow" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The property value cannot:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Exceed the number of cores in the cluster or the number of cores on the nodes that you requested (with the 
        /// <see cref="ISchedulerJob{ITaskId}.RequestedNodes" /> property).</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="ISchedulerJob{ITaskId}.MinimumNumberOfCores" /> property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfNodes" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfSockets" />
        int MaximumNumberOfCores { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of nodes that the job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of nodes.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> job property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Node" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMin" /> to 
        /// False; otherwise, the minimum number of nodes that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at least n nodes 
        /// to run (the scheduler will not allocate less than this number of nodes for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="ISchedulerJob{ITaskId}.CanGrow" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The value cannot exceed the value of the <see cref="ISchedulerJob{ITaskId}.MaximumNumberOfNodes" /> property.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfCores" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfSockets" />
        int MinimumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of nodes that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of nodes.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the <see cref="ISchedulerJob{ITaskId}.UnitType" /> job property is Node.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMax" /> to 
        /// False; otherwise, the maximum number of nodes that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at most n nodes 
        /// to run (the scheduler will not allocate more than this number of nodes for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="ISchedulerJob{ITaskId}.CanGrow" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The value cannot:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Exceed the number of nodes in the cluster or the number of nodes that you requested (with the 
        /// <see cref="ISchedulerJob{ITaskId}.RequestedNodes" /> property).</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="ISchedulerJob{ITaskId}.MinimumNumberOfNodes" /> job property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfCores" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfSockets" />
        int MaximumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of sockets that the job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of sockets.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> job property is 
        /// <see cref="Socket" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMin" /> to 
        /// False; otherwise, the minimum number of sockets that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at least n sockets 
        /// to run (the scheduler will not allocate less than this number of sockets for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="ISchedulerJob{ITaskId}.CanGrow" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The value cannot exceed the value of the <see cref="ISchedulerJob{ITaskId}.MaximumNumberOfSockets" /> property.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfCores" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfNodes" />
        int MinimumNumberOfSockets { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of sockets that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of sockets.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> job property is 
        /// <see cref="Socket" />.</para>
        ///   <para>If you set this property, you must set 
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMax" /> to 
        /// False; otherwise, the maximum number of sockets that you specified will be ignored.</para>
        ///   <para>The Default job template sets the default value to 1.</para>
        ///   <para>This property tells the scheduler that the job requires at most n sockets 
        /// to run (the scheduler will not allocate more than this number of sockets for the job).</para>
        ///   <para>The job can run when its minimum resource requirements are met. The scheduler may allocate up to the 
        /// maximum specified resource limit for the job. The scheduler will allocate more resources to the job or release resources if the  
        /// <see cref="ISchedulerJob{ITaskId}.CanGrow" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.CanShrink" /> properties are set to true; otherwise, the job uses the initial allocation for its lifetime.</para> 
        ///   <para>The property value cannot:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Exceed the number of sockets in the cluster or the number of sockets on the nodes that you requested (with the 
        /// <see cref="ISchedulerJob{ITaskId}.RequestedNodes" /> property).</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="ISchedulerJob{ITaskId}.MinimumNumberOfSockets" /> job property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfCores" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfNodes" />
        int MaximumNumberOfSockets { get; set; }

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
        /// <see cref="ISchedulerJob{ITaskId}.NodeGroups" /> property, the job can be run on the intersection of the two lists.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITaskId}.AllocatedNodes" />
        /// <seealso cref="ISchedulerTask.RequiredNodes" />
        ICollection<string> RequestedNodes { get; set; }

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
        /// <seealso cref="ISchedulerTask.IsExclusive" />
        bool IsExclusive { get; set; }

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
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMax" /> and 
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMin" /> are also set to 
        /// True.</para>
        /// </remarks>
        /// <example />
        bool RunUntilCanceled { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the names of the node groups that specify the nodes on which the job can run.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of node group names.</para>
        /// </value>
        /// <remarks>
        ///   <para>A node group is typically created to identify a group of nodes with a similar characteristic. For example, nodes that contain 
        /// a specific software license. You can then use this property to identify the nodes instead of having to specify each node using the  
        /// <see cref="ISchedulerJob{ITaskId}.RequestedNodes" /> property.</para>
        ///   <para>If you specify multiple node groups, the resulting node list is based on the value of the 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.NodeGroupOp" /> property. For example if group A contains nodes 1, 2, 3, and 4 and group B contains nodes 3, 4, 5, and 6, the resulting list would be.</para> 
        ///   <para>
        ///     <see cref="ISchedulerJob{ITaskId}.NodeGroupOp" /> Property Value</para>
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
        /// <see cref="ISchedulerJob{ITaskId}.RequestedNodes" /> property, the list of nodes on which the job may run is the intersection of the requested node list and the resulting node group list.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="IScheduler.GetNodeGroupList" />
        /// <seealso cref="IScheduler.GetNodesInNodeGroup(System.String)" />
        ICollection<string> NodeGroups { get; set; }

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
        bool FailOnTaskFailure { get; set; }

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
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> property determines which maximum resource value the scheduler calculates (for example,  
        /// <see cref="ISchedulerJob{ITaskId}.MaximumNumberOfNodes" /> or 
        /// <see cref="ISchedulerJob{ITaskId}.MaximumNumberOfCores" />).</para>
        ///   <para>If you set one of the maximum resource properties, you must set 
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMax" /> to 
        /// False; otherwise, the maximum resource value that you specified will be ignored.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITaskId}.AutoCalculateMin" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CanGrow" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CanShrink" />
        bool AutoCalculateMax { get; set; }

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
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> property determines which minimum resource value the scheduler calculates (for example,  
        /// <see cref="ISchedulerJob{ITaskId}.MinimumNumberOfNodes" /> or 
        /// <see cref="ISchedulerJob{ITaskId}.MinimumNumberOfCores" />).</para>
        ///   <para>If you set one of the minimum resource properties, you must set 
        /// <see cref="ISchedulerJob{ITaskId}.AutoCalculateMin" /> to 
        /// False; otherwise, the maximum resource value that you specified will be ignored.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerJob{ITaskId}.AutoCalculateMax" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CanGrow" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CanShrink" />
        bool AutoCalculateMin { get; set; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.AutoCalculateMax" />
        /// <seealso cref="ISchedulerJob{ITaskId}.AutoCalculateMin" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CanShrink" />
        bool CanGrow { get; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.AutoCalculateMax" />
        /// <seealso cref="ISchedulerJob{ITaskId}.AutoCalculateMin" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CanGrow" />
        bool CanShrink { get; }

        /// <summary>
        ///   <para>Determines whether another job can preempt this job.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if another job can preempt this job; otherwise, False.</para>
        /// </value>
        /// <remarks>
        ///   <para>The Default job template sets the default value to True.</para>
        ///   <para>For details on job preemption, see the PreemptionType configuration parameter in Remarks section of 
        /// <see cref="IScheduler.SetClusterParameter(System.String,System.String)" />.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.Priority" />
        bool CanPreempt { get; set; }

        /// <summary>
        ///   <para>Retrieves the job-related error message or job cancellation message.</para>
        /// </summary>
        /// <value>
        ///   <para>The message.</para>
        /// </value>
        /// <remarks>
        ///   <para>The message contains the last message that was set for the job. The message can be a run-time error message or the message passed to the 
        /// <see cref="IScheduler.CancelJob(System.Int32,System.String)" /> method.</para>
        ///   <para>Check the message if the job state is Failed or Canceled.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="ISchedulerTask.ErrorMessage" />
        string ErrorMessage { get; }

        /// <summary>
        ///   <para>Determines whether the <see cref="ISchedulerJob{ITaskId}.Runtime" /> job property is set.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the run-time limit is set; otherwise, False.</para>
        /// </value>
        bool HasRuntime { get; }

        /// <summary>
        ///   <para>Retrieves the number of times that the job has been queued again.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of times that the job has been queued again.</para>
        /// </value>
        /// <remarks>
        ///   <para>The count is incremented each time you call the 
        /// <see cref="IScheduler.ConfigureJob(System.Int32)" /> method.</para>
        /// </remarks>
        /// <example />
        int RequeueCount { get; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfCores" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfSockets" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfNodes" />
        int MinMemory { get; set; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfNodes" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfCores" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfSockets" />
        int MaxMemory { get; set; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfNodes" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfCores" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MinimumNumberOfSockets" />
        int MinCoresPerNode { get; set; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.MinCoresPerNode" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfNodes" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaxMemory" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfCores" />
        /// <seealso cref="ISchedulerJob{ITaskId}.MaximumNumberOfSockets" />
        int MaxCoresPerNode { get; set; }

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
        ICollection<string> SoftwareLicense { get; set; }

        /// <summary>
        ///   <para>Retrieves the name of the process that created the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the process that created the job.</para>
        /// </value>
        string ClientSource { get; set; }

        #endregion

        #region V3 methods Don't change

        /// <summary>
        ///   <para>Sets the job to the finished state and does not run any additional tasks except node release tasks.</para>
        /// </summary>
        /// <remarks>
        ///   <para>You can use this method to finish jobs that have the 
        /// <see cref="ISchedulerJob{ITaskId}.RunUntilCanceled" /> property set to 
        /// True when you have an alternate way of determining that the job is finished.</para>
        ///   <para>You can call this method only for jobs that are in the queued or running states.</para>
        ///   <para>The 
        /// <see cref="ISchedulerJob{ITaskId}.Finish" /> method is essentially equivalent to calling the 
        /// <see cref="IScheduler.CancelJob(System.Int32,System.String)" /> method, except that the 
        /// <see cref="ISchedulerJob{ITaskId}.Finish" /> method sets the state of the job to finished instead of canceled.</para>
        /// </remarks>
        /// <seealso cref="ISISchedulerJob{ITaskId}duler.ISchedulerJob,System.String,System.String)" />
        /// <seealso cref="IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />
        /// <seealso cref="IScheduler.CancelJob(System.Int32,System.String)" />
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
        /// <see cref="ISchedulerTask.SetEnvironmentVariable(System.String,System.String)" /> method.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerTask.SetEnvironmentVariable(System.String,System.String)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.EnvironmentVariables" />
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
        /// <see cref="ISchedulerJob{ITaskId}.SetEnvironmentVariable(System.String,System.String)" /> method to set or unset environment variables for a job.</para> 
        ///   <para>You cannot add environment variables to the job by adding 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValue" /> items to the 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> that the 
        /// <see cref="ISchedulerJob{ITaskId}.EnvironmentVariables" /> property contains. Use the 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.SetEnvironmentVariable(System.String,System.String)" /> method to add environment variables instead.</para> 
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.SetEnvironmentVariable(System.String,System.String)" />
        /// <seealso cref="ISchedulerTask.EnvironmentVariables" />
        IDictionary<string, string> EnvironmentVariables { get; }

        /// <summary>
        ///   <para>Add the specified tasks to the job.</para>
        /// </summary>
        /// <param name="taskList">
        ///   <para>An array of 
        /// <see cref="ISchedulerTask" /> interfaces for the tasks that job want to add to the job. To create the tasks, call the 
        /// <see cref="ISchedulerJob{ITaskId}.CreateTask" /> method once for each task that you want to create.</para>
        /// </param>
        /// <remarks>
        ///   <para>Typically, you call this method while the job is in the Configuring state. After you submit the job by calling the 
        /// 
        /// <see cref="ISchedISchedulerJob{ITaskId}r.ISchedulerJob,System.String,System.String)" /> method, you must call the  
        /// 
        /// <see cref="ISchedulerJob{ITaskIISchedulerTask.ISchedulerTask[])" /> method to add tasks to the job if you want the tasks to run. If you call the  
        /// 
        /// <see cref="ISchedulerJob{ITaISchedulerTask.ISchedulerTask[])" /> method after the job is submitted, the tasks will not run until you submit the tasks.</para> 
        ///   <para>You cannot submit tasks after the job finishes.</para>
        ///   <para>Unless you specify dependencies for the tasks, the tasks you add to a job do not 
        /// necessarily run in any particular order, except that node preparation and node release tasks are the first and last  
        /// tasks to run on a node, respectively. The HPC Job Scheduler Service tries to run as many tasks 
        /// in parallel as it can, which may preclude the tasks from running in the order in which you added them.</para> 
        ///   <para>For better performance, call the 
        /// 
        /// <see cref="ISchedulerJob{ITaISchedulerTask.ISchedulerTask[])" /> method to add several tasks to a job at once, instead of calling  
        /// <see cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" /> multiple times inside a loop.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CreateTask" />
        /// <seealso cref="ISchedulerJob{ITaskIISchedulerTask.ISchedulerTask[])" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CancelTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        void AddTasks(ISchedulerTask[] taskList);

        /// <summary>
        ///   <para>Submits the specified tasks to the job.</para>
        /// </summary>
        /// <param name="taskList">
        ///   <para>An array of <see cref="ISchedulerTask" /> interfaces to the tasks that you want to submit.</para>
        /// </param>
        /// <remarks>
        ///   <para>You can call this method any time after you add to the job to the HPC Job Scheduler Service. Before you add the job, you must call the 
        /// <see cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaISchedulerTask.ISchedulerTask[])" /> method to add tasks to the job. After you submit the job by calling the  
        /// <see cref="ISchedISchedulerJob{ITaskId}r.ISchedulerJob,System.String,System.String)" /> or 
        /// <see cref="IScheduler.SubmitJobById(System.Int32,System.String,System.String)" /> method, you must call the 
        /// <see cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" />, 
        /// <see cref="ISchedulerJob{ITaskId}.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />, or 
        /// 
        /// <see cref="ISchedulerJob{ITaskIISchedulerTask.ISchedulerTask[])" /> method to add tasks to the job if you want the tasks to run. If you call the  
        /// <see cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaISchedulerTask.ISchedulerTask[])" /> method after you submit the job, the tasks do not run until you submit the tasks. You cannot submit tasks after the job finishes.</para> 
        ///   <para>Unless you specify dependencies for the tasks, the tasks you add to a job do not 
        /// necessarily run in any particular order, except that node preparation and node release tasks are the first and last  
        /// tasks to run on a node, respectively. The HPC Job Scheduler Service tries to run as many tasks 
        /// in parallel as it can, which may preclude the tasks from running in the order in which you added them.</para> 
        ///   <para>To submit a single task to the job, use the 
        /// <see cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" /> or 
        /// <see cref="ISchedulerJob{ITaskId}.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method.</para>
        ///   <para>For better performance, call the 
        /// 
        /// <see cref="ISchedulerJob{ITaskIISchedulerTask.ISchedulerTask[])" /> method to add several tasks to a job at once, instead of calling the  
        /// <see cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" /> or 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method multiple times inside a loop.</para> 
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITISchedulerTask.ISchedulerTask)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.CreateTask" />
        /// <seealso cref="ISchedulerJob{ITaskId}.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />
        /// <seealso cref="ISchedulerJob{ITaISchedulerTask.ISchedulerTask[])" />
        /// <seealso cref="ISchedulerJob{ITaskISchedulerTask.ISchedulerTask)" />
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
        /// <see cref="ISchedulerJob{ITaskId}.ClearHold" /> method.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.ClearHold" />
        /// <seealso cref="ISchedulerJob{ITaskId}.HoldUntil" />
        void SetHoldUntil(DateTime holdUntil);

        /// <summary>
        ///   <para>Removes the hold on the job by clearing the date and 
        /// time that the HPC Job Scheduler Service should wait until before running the job.</para>
        /// </summary>
        /// <remarks>
        ///   <para>If you call this method for a job that is not currently on hold, the method has no effect.</para>
        ///   <para>The 
        /// <see cref="ISchedulerJob{ITaskId}.ClearHold" /> method does not return a value. To confirm that the 
        /// <see cref="ISchedulerJob{ITaskId}.ClearHold" /> method succeeded, check the value of the 
        /// <see cref="ISchedulerJob{ITaskId}.HoldUntil" /> property.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.SetHoldUntil(System.DateTime)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.HoldUntil" />
        void ClearHold();

        /// <summary>
        /// Cancels the task.
        /// </summary>
        /// <param name="taskId">ID of the task to Cancel</param>
        /// <param name="message">Message to be saved on the task object once the task has been canceled.  Can be null</param>
        void CancelTask(string taskId, string message);

        /// <summary>
        /// Cancels the task.
        /// </summary>
        /// <param name="taskId">ID of the task to Cancel</param>
        /// <param name="message">Message to be saved on the task object once the task has been canceled.  Can be null</param>
        /// <param name="isForce">Should the task be force cancelled ?</param>
        void CancelTask(string taskId, string message, bool isForce);

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
        /// <see cref="ISchedulerTask.IsRerunnable" /> property for the task is true.</para>
        ///   <para>If a node is specified in the 
        /// 
        /// <see cref="ISchedulerTask.RequiredNodes" /> property for any of the tasks in the job, an exception occurs if you also specify that node when you call the  
        /// <see cref="ISchedulerJob{ITaskId}.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method.</para>
        ///   <para>If you call 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> and specify a set of nodes that would cause the set of available resources to become smaller than the minimum number of resources that the job requires, an exception occurs. For example, if you have an HPC cluster than consists of three nodes and you include two of them in the <paramref name="nodeNames" /> parameter when you call  
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />, that action makes only one node available, and an exception occurs if the job requires a minimum of two nodes.</para> 
        ///   <para>If you specify the name of a node that does not currently belong to the HPC cluster, the method generates an exception.</para>
        ///   <para>If you add the same node twice to the list of nodes that should 
        /// not be used for the job, the second time that you add the node has no effect.</para>
        ///   <para>This method succeeds if and only if it does not generate an exception. 
        /// This method does not return a value. To verify that the method succeeded, use the  
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.ExcludedNodes" /> property to get the full list of nodes that should not be used for the job.</para> 
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.ClearExcludedNodes" />
        /// <seealso cref="ISchedulerJob{ITaskId}.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.ExcludedNodes" />
        void AddExcludedNodes(ICollection<string> nodeNames);

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
        /// <see cref="ISchedulerJob{ITaskId}.ClearExcludedNodes" /> method.</para>
        ///   <para>If you specify a node that does not currently belong to the HPC cluster, an exception occurs. If you specify a node that 
        /// is part of the HPC cluster but is not part of the current list of nodes that should not be used for the job, the  
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method has no effect and no error occurs.</para> 
        ///   <para>The 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method does not return a value. To verify that the method succeeded, check the value of the  
        /// <see cref="ISchedulerJob{ITaskId}.ExcludedNodes" /> property.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.ClearExcludedNodes" />
        /// <seealso cref="ISchedulerJob{ITaskId}.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.ExcludedNodes" />
        void RemoveExcludedNodes(ICollection<string> nodeNames);

        /// <summary>
        ///   <para>Removes all of the nodes in the list of nodes that should not be used for the job from that list.</para>
        /// </summary>
        /// <remarks>
        ///   <para>To remove specific nodes from the list of nodes that should not be used for the job, use the 
        /// <see cref="ISchedulerJob{ITaskId}.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method.</para>
        ///   <para>The 
        /// <see cref="ISchedulerJob{ITaskId}.ClearExcludedNodes" /> method does not return a value. To confirm that the 
        /// <see cref="ISchedulerJob{ITaskId}.ClearExcludedNodes" /> method succeeded, check the value of the 
        /// <see cref="ISchedulerJob{ITaskId}.ExcludedNodes" /> property.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.ExcludedNodes" />
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
        /// <seealso cref="ISchedulerJob{ITaskId}.ProgressMessage" />
        int Progress { get; set; }

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
        /// <seealso cref="Progress{T}" />
        string ProgressMessage { get; set; }

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
        /// <see cref="ISchedulerJob{ITaskId}.TargetResourceCount" /> property applies to depends on the value of the 
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> property. For example, if 
        /// <see cref="ISchedulerJob{ITaskId}.TargetResourceCount" /> is 3 and 
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Node" />, then the job currently can use a maximum of three nodes. If 
        /// <see cref="ISchedulerJob{ITaskId}.TargetResourceCount" /> is 3 and 
        /// <see cref="ISchedulerJob{ITaskId}.UnitType" /> is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />, then the job currently can use a maximum of three cores.</para>
        ///   <para>This property is updated periodically based on the RebalancingInterval cluster parameter. 
        /// During an adjustment, the broker updates job properties and evaluates the value of the  
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.TargetResourceCount" /> property for a session, based on the load sampling metrics that are gathered within the adjustment interval. The HPC Job Scheduler Service uses the  
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.TargetResourceCount" /> property to help determine how many service hosts to allocate to the session. </para> 
        ///   <para>If a SOA client sets the value of the property for a service job, the broker resets it if allocation adjustment is enabled.</para>
        /// </remarks>
        int TargetResourceCount { get; set; }

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
        /// <see cref="ISchedulerJob{ITaskId}.ExpandedPriority" /> and 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.Priority" /> properties, the property that you set last determines the priority of the job. When you set the value of one of these properties, the value of the other property is automatically updated to the equivalent value.</para> 
        ///   <para>The Default job template sets the default value to <see cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.Normal" />.</para>
        ///   <para>The job template for the job determines that values the a user can set for 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.ExpandedPriority" /> without administrative privileges. A cluster administrator cannot submit a job with an  
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.ExpandedPriority" /> that the job template does not allow, but when the job is in the queued or running state, the cluster administrator can set  
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.ExpandedPriority" /> to any value between 0 and 4000, even if the job template does not allow that expanded priority.</para> 
        ///   <para>Server resources are allocated to jobs based on job priority, except for backfill jobs. Jobs can be preempted if the 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.CanPreempt" /> property is true; otherwise, jobs run until they finish, fail, or are canceled.</para> 
        ///   <para>Within a job, tasks receive resources based on the order in which 
        /// they were added to the job. If a core is available, the task will run.</para>
        ///   <para>See also Microsoft.Hpc.Scheduler.Session.SessionStartInfo.SessionPriority</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.BelowNormal" />
        /// <seealso cref="ISchedulerJob{ITaskId}.Priority" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.Highest" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPriority" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.JobPriorityToExpandedPriority(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.Lowest" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.AboveNormal" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.Normal" />
        int ExpandedPriority { get; set; }

        /// <summary>
        ///   <para>Gets or sets the name of the SOA service that the service tasks in the job use, if the job contains service tasks.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.String" /> that specifies the name of the SOA service that the service tasks in the job use, if the job contains service tasks.</para> 
        /// </value>
        string ServiceName { get; set; }

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
        /// <see cref="ISchedulerJob{ITaskId}.SetHoldUntil(System.DateTime)" /> method. To remove that value that is set for this property, call the  
        /// <see cref="ISchedulerJob{ITaskId}.ClearHold" /> method.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.SetHoldUntil(System.DateTime)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.ClearHold" />
        DateTime HoldUntil { get; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.NotifyOnCompletion" />
        bool NotifyOnStart { get; set; }

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
        /// <seealso cref="ISchedulerJob{ITaskId}.NotifyOnStart" />
        bool NotifyOnCompletion { get; set; }

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
        /// <see cref="ISchedulerJob{ITaskId}.ExcludedNodes" /> property contains. To add nodes to the set, use the 
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method. To remove individual nodes from the set, use the  
        /// 
        /// <see cref="ISchedulerJob{ITaskId}.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method. To remove all of the nodes from the set, use the  
        /// <see cref="ISchedulerJob{ITaskId}.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" /> method.</para>
        ///   <para>The limit to the number of nodes which can be excluded is set by 
        /// the ExcludedNodesLimit parameter of the <see href="http://technet.microsoft.com/library/ff950175.aspx">Set-HpcClusterProperty</see> property. 
        /// This limit can be modified by the cluster administrator.</para> 
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        /// <seealso cref="ISchedulerJob{ITaskId}.ClearExcludedNodes" />
        /// <seealso cref="ISchedulerJob{ITaskId}.RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection)" />
        ICollection<string> ExcludedNodes { get; }

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
        /// <see cref="ISchedulerJob{ITaskId}.NotifyOnStart" /> and/or 
        /// <see cref="ISchedulerJob{ITaskId}.NotifyOnCompletion" /> property to 
        /// True to specify when you want to receive notifications about the job.</para>
        /// </remarks>
        /// <seealso cref="ISchedulerJob{ITaskId}.NotifyOnStart" />
        /// <seealso cref="ISchedulerJob{ITaskId}.NotifyOnCompletion" />
        string EmailAddress { get; set; }

        #endregion

        #region V3 SP2 Properties Don't change

        /// <summary>
        ///   <para>Returns the name of the job’s pool.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> object that contains the name of the job’s pool.</para>
        /// </value>
        string Pool { get; }

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
        string ValidExitCodes { get; set; }

        /// <summary>
        ///   <para>Gets or sets a value that indicates whether all resources such as cores or sockets should be allocated on one node.</para>
        /// </summary>
        /// <value>
        ///   <para>
        ///     True if all resources should be allocated on one node; otherwise, False. The default is false.</para>
        /// </value>
        bool SingleNode { get; set; }

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
        /// <see cref="ISchedulerJob{ITaskId}.EstimatedProcessMemory" /> property is used by the job scheduler to assign jobs to nodes with sufficient memory to execute the job.</para> 
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
        void RestoreFromXmlEx(XmlReader reader, bool includeTaskGroup);

        #endregion

        #region V4SP3 methods, Don't change

        /// <summary>
        /// <para>Finish a task directly. So the task will be never requeued.</para>
        /// </summary>
        /// <param name="taskId">the ID of the task to finish</param>
        /// <param name="message">the message to finish the task.</param>
        void FinishTask(string taskId, string message);

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

        #region V5SP2 methods / properties

        bool GetBalanceRequest(out IList<SoaBalanceRequest> request);

        #endregion
    }
}