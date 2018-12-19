namespace Microsoft.Hpc.Scheduler.Session.SchedulerPort
{
    using System;
    using System.Collections.Generic;

    // TODO: Trim this interface down to minimal
    public interface ISchedulerTask
    {
        #region V2 methods. Don't change

        /// <summary>
        ///   <para>Refreshes this copy of the task with the contents from the server.</para>
        /// </summary>
        void Refresh();

        /// <summary>
        ///   <para>Commits the local task changes to the server.</para>
        /// </summary>
        /// <remarks>
        ///   <para>All changes made to the task are made locally. Call this method to apply your changes to the server.</para>
        ///   <para>To make changes to a task, the task and job must be in 
        /// the Configuring state; you cannot make changes to the task after the job has been submitted.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853421(v=vs.85).aspx">Cloning a Job</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Refresh" />
        void Commit();

        /// <summary>
        ///   <para>Sets a task-specific environment variable.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The name of the variable.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The string value of the variable. If null or an empty string, the variable is deleted.</para>
        /// </param>
        /// <remarks>
        ///   <para>The length of all environment variables specified for the task is limited to 2,048 characters.</para>
        ///   <para>The environment variables that are made available to the task include variables set using this method, those set using the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetEnvironmentVariable(System.String,System.String)" /> method, and the HPC-defined environment variables.</para> 
        ///   <para>To retrieve an variable from inside your task, call the 
        /// <see cref="System.Environment.GetEnvironmentVariable(System.String)" /> method.</para>
        ///   <para>To set cluster-wide environment variables, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetEnvironmentVariable(System.String,System.String)" /> method.</para>
        /// </remarks>
        /// <example />
        void SetEnvironmentVariable(string name, string value);

        /// <summary>
        ///   <para>Retrieves the environment variables that were set for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains the collection of variables. Each item in the collection is an  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValue" /> interface that contains the variable name and value. The collection is empty if no variables have been set.</para> 
        /// </value>
        IDictionary<string, string> EnvironmentVariables { get; }

        /// <summary>
        ///   <para>Retrieves the names of the nodes that have been allocated to run the task or have run the task.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains the names of the nodes that have been allocated to run the task or have run the task.</para> 
        /// </value>
        ICollection<string> AllocatedNodes { get; }

        /// <summary>
        ///   <para>Sets an application-defined property on the task.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The property name. The name is limited to 80 characters.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The property value as a string. The name is limited to 1,024 characters.</para>
        /// </param>
        /// <remarks>
        ///   <para>Use this method to define your own properties that are passed to the submission and activation 
        /// filters. If the property exists, the value of the property is updated. If value is null, the property is deleted.</para>
        ///   <para>To retrieve the application-defined properties, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.GetCustomProperties" /> method.</para>
        /// </remarks>
        void SetCustomProperty(string name, string value);

        #endregion

        #region V2 properties. Don't change

        /// <summary>
        ///   <para>Retrieves or sets the display name of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The display name. The name is limited to 80 characters.</para>
        /// </value>
        string Name { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of cores that the task requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of cores. The default is 1.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />.</para>
        ///   <para>This property tells the scheduler that the task requires at least n cores 
        /// to run (the scheduler will not allocate less than this number of cores for the task).</para>
        ///   <para>The value must be less than or equal to:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>The value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfCores" /> property.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" /> property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfSockets" />
        int MinimumNumberOfCores { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of cores that the scheduler may allocate for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of cores. The default is 1.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core" />.</para>
        ///   <para>This property tells the scheduler that the task requires at most n cores 
        /// to run (the scheduler will not allocate more than this number of cores for the task).</para>
        ///   <para>The value cannot:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Exceed the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" /> property.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfCores" /> property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfSockets" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfCores" />
        int MaximumNumberOfCores { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of nodes that the task requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of nodes. The default is 1.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Node" />.</para>
        ///   <para>This property tells the scheduler that the task requires at least n nodes 
        /// to run (the scheduler will not allocate less than this number of nodes for the task).</para>
        ///   <para>The value must be less than or equal to:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>The value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfNodes" /> property.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" /> property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfSockets" />
        int MinimumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of nodes that the scheduler may allocate for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of nodes. The default is 1.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Node" />.</para>
        ///   <para>This property tells the scheduler that the task requires at most n nodes 
        /// to run (the scheduler will not allocate more than this number of nodes for the task).</para>
        ///   <para>The value cannot:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Exceed the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" /> property.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfNodes" /> property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfSockets" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfNodes" />
        int MaximumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of sockets that the task requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of sockets. The default is 1.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Socket" />.</para>
        ///   <para>This property tells the scheduler that the task requires at least n sockets 
        /// to run (the scheduler will not allocate less than this number of sockets for the task).</para>
        ///   <para>The value must be less than or equal to:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>The value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfSockets" /> property.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" /> property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfSockets" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfNodes" />
        int MinimumNumberOfSockets { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of sockets that the scheduler may allocate for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of sockets. The default is 1.</para>
        /// </value>
        /// <remarks>
        ///   <para>Set this property if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" /> property is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType.Socket" />.</para>
        ///   <para>This property tells the scheduler that the task requires at most n cores 
        /// to run (the scheduler will not allocate more than this number of cores for the task).</para>
        ///   <para>The value cannot:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Exceed the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" /> property.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Be less than the value of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfSockets" /> property.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfCores" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MaximumNumberOfNodes" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.MinimumNumberOfSockets" />
        int MaximumNumberOfSockets { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the run-time limit for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The run-time limit for the task, in seconds.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default value is 0.</para>
        ///   <para>The wall clock is used to determine the run time. The time is your best guess of how long the task will take; however, 
        /// it needs to be fairly accurate because it is used to allocate resources. 
        /// If the task exceeds this time, the task is terminated and its state becomes Failed.</para> 
        ///   <para>The sum of all run time values for the tasks in the job should be less than the run-time value for the job (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" />). If the task's run-time value is greater than the job's run-time value, then the task will run until it exceeds the job's run-time value. If this occurs, the task is terminated and its state becomes Failed.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" />
        int Runtime { get; set; }

        /// <summary>
        ///   <para>Retrieves the time that the task was submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>The time the task was submitted. The value is in Coordinated Universal Time.</para>
        /// </value>
        /// <remarks>
        ///   <para>A task is submitted when its parent job is submitted. You could also call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method to submit a task to a running job.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ChangeTime" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.CreateTime" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StartTime" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EndTime" />
        DateTime SubmitTime { get; }

        /// <summary>
        ///   <para>Retrieves the date and time when the task was created.</para>
        /// </summary>
        /// <value>
        ///   <para>The task creation time. The value is in Coordinated Universal Time.</para>
        /// </value>
        DateTime CreateTime { get; }

        /// <summary>
        ///   <para>Retrieves the date and time that the task ended.</para>
        /// </summary>
        /// <value>
        ///   <para>The time the task ended. The value is in Coordinated Universal Time.</para>
        /// </value>
        /// <remarks>
        ///   <para>The end time indicates when the task finished, failed, or was canceled. The value is 
        /// 
        /// <see cref="System.DateTime.MinValue" /> if the task has not finished, failed, or been canceled. The time is reset if you queue a failed or canceled task again.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ChangeTime" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.CreateTime" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.SubmitTime" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StartTime" />
        DateTime EndTime { get; }

        /// <summary>
        ///   <para>The last time that the user or server changed a property of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The date and time that the task was last touched. The value is in Coordinated Universal Time.</para>
        /// </value>
        DateTime ChangeTime { get; }

        /// <summary>
        ///   <para>Retrieves the date and time that the task started running.</para>
        /// </summary>
        /// <value>
        ///   <para>The task start time. The value is in Coordinated Universal Time.</para>
        /// </value>
        DateTime StartTime { get; }

        /// <summary>
        ///   <para>Retrieves the identifier of the parent job.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier of the parent job.</para>
        /// </value>
        int ParentJobId { get; }

        /// <summary>
        ///   <para>Retrieves or sets the command line for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The command to execute. The command is limited to 480 characters.</para>
        /// </value>
        /// <remarks>
        ///   <para>The command line is required for all tasks. The command line must include the name of the executable 
        /// program and any arguments. If the path to the executable program contains long file names, enclose the path in quotation marks.</para>
        ///   <para>The executable program must exist under the specified path on each node specified in the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequiredNodes" /> task property. If you do not specify a list of required nodes, the executable program must exist on each node in the cluster. The same is true if you add the task to a job and the job does not use the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" /> job property to limit the nodes on which the task can run.</para>
        ///   <para>For parametric tasks (see the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property), the Scheduler replaces all occurrences of an asterisk (*) found in the command line with the instance value.</para> 
        ///   <para>For more information about specifying the command line, see 
        /// the description for the command line 
        /// parameter of the Win32 <see href="https://msdn.microsoft.com/library/windows/desktop/ms682425(v=vs.85).aspx">CreateProcess</see> function.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853426(v=vs.85).aspx">Creating and Submitting a Job</see>.</para>
        /// </example>
        string CommandLine { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the directory in which to start the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The absolute path to the startup directory.</para>
        /// </value>
        string WorkDirectory { get; set; }

        /// <summary>
        ///   <para>Determines whether other tasks from the job can run on the node at the same time as this task.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if other tasks from the same job cannot run on the node; otherwise, False. </para>
        /// </value>
        /// <remarks>
        ///   <para>The default is False.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.IsExclusive" />
        bool IsExclusive { get; set; }

        /// <summary>
        ///   <para>Determines whether the task can run again after the task is preempted or fails because of 
        /// an issue with the HPC cluster, or after a node that is running the task is forced offline. </para>
        /// </summary>
        /// <value>
        ///   <para>
        ///     True indicates that the HPC Job Scheduler Service can attempt to rerun the task if the task is 
        /// preempted or fails because of an issue with the HPC cluster, such as a node becoming unreachable. It also indicates that the HPC  
        /// Job Scheduler Service can attempt to rerun the task after a node that is running the task is forced offline when the node comes 
        /// back online. The HPC Job Scheduler Service does not attempt to rerun tasks that run to completion and return a with a nonzero exit code.</para> 
        ///   <para>
        ///     False indicates that the task should fail after the attempt to run the tasks fails.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default is True.</para>
        ///   <para>If this value is 
        /// False, you cannot call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequeueTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method to run the task again.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853429(v=vs.85).aspx">Creating a Parametric Sweep Task</see>.</para>
        /// </example>
        bool IsRerunnable { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the path to which the server redirects standard output.</para>
        /// </summary>
        /// <value>
        ///   <para>The full path to the file to which standard output is redirected.</para>
        /// </value>
        /// <remarks>
        ///   <para>You must specify a file to capture output from stdout; otherwise, the output 
        /// is lost. If the file exists, it is overwritten. Specify a separate file for each  
        /// task. If you use the same file, the task could fail if the file is 
        /// currently locked by another task. The path must exist on each node on which the task runs.</para> 
        ///   <para>If you do not specify a file for stdout, you can access the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Output" /> property to view the output from written to stdout.</para>
        ///   <para>For parametric tasks (see the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property), the Scheduler replaces all occurrences of an asterisk (*) found in the file path with the instance value.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853429(v=vs.85).aspx">Creating a Parametric Sweep Task</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdErrFilePath" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdInFilePath" />
        string StdOutFilePath { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the path from which the server redirects standard input.</para>
        /// </summary>
        /// <value>
        ///   <para>The full path to the file from which standard input is redirected.</para>
        /// </value>
        /// <remarks>
        ///   <para>For parametric tasks (see the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property), the Scheduler replaces all occurrences of an asterisk (*) found in the file path with the instance value.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdErrFilePath" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdOutFilePath" />
        string StdInFilePath { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the path to which the server redirects standard error.</para>
        /// </summary>
        /// <value>
        ///   <para>The full path to the file to which standard error is redirected.</para>
        /// </value>
        /// <remarks>
        ///   <para>You must specify a file to capture output from stderr; otherwise, the output 
        /// is lost. If the file exists, it is overwritten. Specify a separate file for each  
        /// task. If you use the same file, the task could fail if the file is 
        /// currently locked by another task. The path must exist on each node on which the task runs.</para> 
        ///   <para>If you do not specify a file for stderr, you can access the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Output" /> property to view the output from written to stderr.</para>
        ///   <para>For parametric tasks (see the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property), the Scheduler replaces all occurrences of an asterisk (*) found in the file path with the instance value.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853429(v=vs.85).aspx">Creating a Parametric Sweep Task</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdInFilePath" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdOutFilePath" />
        string StdErrFilePath { get; set; }

        /// <summary>
        ///   <para>Retrieves the exit code that the task set. </para>
        /// </summary>
        /// <value>
        ///   <para>The exit code.</para>
        /// </value>
        int ExitCode { get; }

        /// <summary>
        ///   <para>Retrieves the number of times that the task has been queued again.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of times that the task has been queued again.</para>
        /// </value>
        /// <remarks>
        ///   <para>The count increments each time you call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequeueTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method.</para>
        /// </remarks>
        /// <example />
        int RequeueCount { get; }

        /// <summary>
        ///   <para>Determines whether the task is a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the task is a parametric task; otherwise, False. </para>
        /// </value>
        /// <remarks>
        ///   <para>The default is False.</para>
        ///   <para>For parametric tasks, the Scheduler replaces all occurrences of 
        /// an asterisk (*) found in the following task properties, with the instance value.</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.CommandLine" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Name" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdErrFilePath" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdInFilePath" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdOutFilePath" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///   </list>
        ///   <para>The total number of instances that a parametric task can generate is 100,000 instances.</para>
        ///   <para>When you add or submit the job that contains the parametric task to the Scheduler, the Scheduler adds all the instances of the 
        /// parametric task to the job. If you change any of the following properties on a parametric task, the Scheduler will rewrite the instances (if you change  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> to false, the scheduler will remove the instances).</para>
        ///   <list type="bullet">
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
        ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StartValue" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///   </list>
        ///   <para>To get the parametric task from the job, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateTaskId(System.Int32)" /> method and specify the parametric task identifier. Then call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method to get the task. To get an instance of the parametric task, call the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateParametricTaskId(System.Int32,System.Int32)" /> method and specify the instance identifier and parametric task identifier. Then call the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method to get the instance.</para>
        /// </remarks>
        /// <example />
        [System.ObsoleteAttribute("Please use the 'Type' property instead")]
        bool IsParametric { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the starting instance value for a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>The starting instance value. </para>
        /// </value>
        /// <remarks>
        ///   <para>The value must be greater than zero and must not exceed the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EndValue" /> property. The default value is 1.</para>
        ///   <para>The start value specifies the starting parametric instance value. The instance value is incremented by the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IncrementValue" /> property until the instance value exceeds the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EndValue" /> property.</para>
        ///   <para>This property has meaning only if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property is 
        /// True.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853429(v=vs.85).aspx">Creating a Parametric Sweep Task</see>.</para>
        /// </example>
        int StartValue { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the ending value for a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>The ending value.</para>
        /// </value>
        /// <remarks>
        ///   <para>The end value cannot be less than the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StartValue" /> property. The default value is 1.</para>
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StartValue" /> specifies the starting parametric instance value. The instance value is incremented by the value of the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IncrementValue" /> property until the instance value exceeds the specified ending value.</para>
        ///   <para>This property has meaning only if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property is 
        /// True.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853429(v=vs.85).aspx">Creating a Parametric Sweep Task</see>.</para>
        /// </example>
        int EndValue { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the number by which to increment the instance value for a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>The increment value used to calculate the next instance value.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default value is 1.</para>
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StartValue" /> specifies the starting parametric instance value. The instance value is incremented by the increment value until the instance value exceeds the value of the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EndValue" /> property.</para>
        ///   <para>This property has meaning only if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property is 
        /// True.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853429(v=vs.85).aspx">Creating a Parametric Sweep Task</see>.</para>
        /// </example>
        int IncrementValue { get; set; }

        /// <summary>
        ///   <para>Retrieves the task-related error message or task cancellation message.</para>
        /// </summary>
        /// <value>
        ///   <para>The message.</para>
        /// </value>
        /// <remarks>
        ///   <para>The message contains the last run-time error message that was set for 
        /// the task. The message can also contain the reason why the user canceled the task.</para>
        ///   <para>Check the message if the state of the task is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState.Failed" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState.Canceled" />.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ExitCode" />
        string ErrorMessage { get; }

        /// <summary>
        ///   <para>Retrieves the output generated by the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The output from the command.</para>
        /// </value>
        /// <remarks>
        ///   <para>The string includes the output from standard out and standard error. Only the first 4 
        /// kilobytes of the output are available. If you expect more than 4 kilobytes of output, set the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdErrFilePath" /> and 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.StdOutFilePath" /> properties. If you specify paths for both stderr and stdout, this property will be empty.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853482(v=vs.85).aspx">Implementing the Event Handlers for Job Events in C#</see>.</para> 
        /// </example>
        string Output { get; }

        /// <summary>
        ///   <para>Determines whether the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Runtime" /> task property is set.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the run-time limit is set; otherwise, False.</para>
        /// </value>
        /// <remarks>
        ///   <para>The default value is False.</para>
        /// </remarks>
        /// <example />
        bool HasRuntime { get; }

        /// <summary>
        ///   <para>The encrypted user blob.</para>
        /// </summary>
        /// <value>
        ///   <para>A byte array that contains the encrypted blob. </para>
        /// </value>
        /// <remarks>
        ///   <para>The blob is set using the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.UserBlob" /> property.</para>
        /// </remarks>
        byte[] EncryptedUserBlob { get; set; }

        /// <summary>
        ///   <para>Retrieves or sets the user data associated with the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The user data associated with the task. The data is limited to 4,000 bytes. </para>
        /// </value>
        /// <remarks>
        ///   <para>HPC encrypts the user data when the job is added or submitted to the scheduler. Only the user that added or submitted the job can get 
        /// the user data. Note that if the user specifies a different RunAs user when 
        /// the job is submitted, the RunAs user will not be able to access the user data.</para> 
        /// </remarks>
        string UserBlob { get; set; }

        #endregion

        #region V3 methods. Don't change

        /// <summary>
        ///   <para>Directs the HPC Job Scheduler Service to stop starting subtasks for a service task.</para>
        /// </summary>
        /// <param name="cancelSubTasks">
        ///   <para>
        ///     
        /// <see cref="System.Boolean" /> that specifies whether the HPC Job Scheduler Service should cancel subtasks of the service task that are already running.True specifies that the HPC Job Scheduler Service should cancel subtasks of the service task that are already running.  
        /// False specifies that the HPC Job Scheduler Service should not cancel subtasks of the service task that are already running.</para>
        /// </param>
        void ServiceConclude(bool cancelSubTasks);

        #endregion

        #region V3 properties. Don't change.

        /// <summary>
        ///   <para>Gets the identifiers of the processor cores that have been allocated to run the task or that have run the task. </para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.String" /> that indicates the cores that have been allocated to run the task or that have run the task. The format of the string is node1_name core1_identifier [node2_name core2_identifier ...].</para> 
        /// </value>
        /// <remarks>
        ///   <para>To get the names of the nodes that are running the task or have run the task, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.AllocatedNodes" /> property.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.AllocatedNodes" />
        string AllocatedCoreIds { get; }

        /// <summary>
        ///   <para>Gets whether the HPC Job Scheduler Service has concluded starting subtasks for a service task.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Boolean" /> that indicates whether the HPC Job Scheduler Service has concluded starting subtasks for a service task.</para>
        ///   <para>
        ///     True indicates that the HPC Job Scheduler Service has concluded starting subtasks for a service task. False 
        ///    indicates that the HPC Job Scheduler Service has not concluded 
        /// starting subtasks for a service task, or that the task is not a service task.</para> 
        /// </value>
        bool IsServiceConcluded { get; }

        #endregion

        #region V3 SP1 Properties

        /// <summary>
        ///   <para>Gets or sets whether the task is critical for the job. If a task is critical for 
        /// the job, the job and its tasks stop running and the job is immediately marked as failed if the task fails.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Boolean" /> that indicates whether the task is critical for the job. 
        /// True indicates that the task is critical for the job, and that the job and 
        /// its tasks stop running and the job is marked as failed if the task fails.  
        /// False indicates that the task is not critical for the job, so that job continues to run the remaining tasks 
        /// when the task that is not critical fails, and the job is marked as failed only when those remaining tasks finish.</para> 
        /// </value>
        /// <remarks>
        ///   <para>If you specify a value for the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailureCount" /> property, and do not specify a value for the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailure" /> property, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailure" /> is automatically set to 
        /// True. </para>
        ///   <para>To specify that a job and its tasks should stop running and that job 
        /// should be marked as failed when any of the tasks in the job fail, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.FailOnTaskFailure" /> property for the job.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailureCount" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.FailOnTaskFailure" />
        bool FailJobOnFailure { get; set; }

        /// <summary>
        ///   <para>Gets or sets the number of subtasks of a critical parametric sweep or service task that must fail 
        /// before the job and its tasks and subtask should stop running and the task and job should be marked as failed.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="System.Int32" /> that indicates the number of subtasks of a critical parametric sweep or service task that must fail before the job and its tasks and subtask should stop running and the task and job should be marked as failed.</para> 
        /// </value>
        /// <remarks>
        ///   <para>You can specify a value from 1 through 1,000,000. If you specify more subtasks than the 
        /// critical task contains, the job and its tasks and subtasks run to completion no matter how many subtasks fail.</para>
        ///   <para>If you specify a value greater than 1 for the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailureCount" /> property for a task that is not a parametric sweep or service task, an error occurs. </para> 
        ///   <para>If you specify a value for the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailureCount" /> property, but also specify 
        /// False for the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailure" /> property, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailureCount" /> is set to 0. If you specify a value for 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailureCount" />, and do not specify a value for 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailure" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailure" /> is automatically set to 
        /// True.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.FailJobOnFailure" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Type" />
        int FailJobOnFailureCount { get; set; }

        #endregion

        #region V4 properties. Don't change.

        /// <summary>
        ///   <para>Gets or sets the exit codes to be used for checking whether tasks in the job successfully exit.</para>
        /// </summary>
        /// <value>
        ///   <para>The exit codes that indicate whether task successfully exited.</para>
        /// </value>
        /// <remarks>
        ///   <para>Specifies the exit codes to be used for checking whether the 
        /// task successfully exited. You can specify discrete integers and integer ranges separated by commas.</para>
        ///   <para>Integer ranges are denoted by integers separated by two 
        /// periods. For example, <c>10..20</c> represents an integer range from ten to twenty.</para>
        ///   <para>
        ///     min and max may be 
        /// used to specify minimum and maximum integer values. For example, <c>0..max</c> represents nonnegative integers.</para>
        ///   <para>The 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ValidExitCodes" /> property overrides the job success exit codes specified by the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ValidExitCodes" /> property.</para>
        /// </remarks>
        string ValidExitCodes { get; set; }

        #endregion

        #region V4SP1 properties, Don't change

        /// <summary>
        ///   <para>Gets or sets whether the task is running on nodes that are being preempted by other jobs.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Boolean" /> that indicates whether the task is running on nodes that are being preempted by other jobs.</para>
        /// </value>
        /// <remarks>
        ///   <para>This is used during graceful preemption to mark running tasks in this job that are running on nodes which can be used by jobs wanting to preempt this job.</para>
        /// </remarks>
        bool ExitIfPossible { get; }

        #endregion

        #region V4SP3 properties, Don't change

        /// <summary>
        ///   <para>Gets or sets the retried execution count of the task after execution failure.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Int32" /> that indicates the retried execution count of the task after execution failure.</para>
        /// </value>
        int ExecutionFailureRetryCount { get; }

        #endregion

        #region V4SP5 properties, Don't change

        /// <summary>
        /// <para>The Requested Node Group Name for the task</para>
        /// </summary>
        string RequestedNodeGroup { get; set; }

        #endregion
    }
}