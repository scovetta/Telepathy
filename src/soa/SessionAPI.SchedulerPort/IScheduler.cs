namespace Microsoft.Hpc.Scheduler.Session.SchedulerPort
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IScheduler
    {
        #region V2 scheduler methods/props. Don't modify this

        /// <summary>
        ///   <para>Connects you to the specified cluster.</para>
        /// </summary>
        /// <param name="cluster">
        ///   <para>The computer name of the cluster's head node (the head node is the node on which Microsoft HPC 
        /// Service is installed). If your application is running on the head node, you can specify the node's name or use “localhost”.</para>
        /// </param>
        /// <remarks>
        ///   <para>To connect to the cluster, the user needs to have an account on the head node or be a member of the Users or Administrators group.</para>
        ///   <para>You can use the HPC SDK to schedule jobs on Microsoft HPC Server 2008 and 
        /// later HPC servers; you cannot use this SDK to schedule jobs on Microsoft Compute Cluster Server 2003 (CCS).</para>
        ///   <para>You can use the following filter to look up the name of the head nodes in a domain in Active Directory:</para>
        ///   <code>(&amp;amp;(objectClass=ServiceConnectionPoint)(serviceClassName=MicrosoftComputeCluster)(keywords=*Version2*))</code>
        ///   <para>To close the connection, use the <see cref="Microsoft.Hpc.Scheduler.IScheduler.Close" /> method.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853425(v=vs.85).aspx">Connecting to a Cluster</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.Close" />
        void Connect(string cluster);

        /// <summary>
        ///   <para>Specifies whether the calling application is a console or Windows application.</para>
        /// </summary>
        /// <param name="isConsole">
        ///   <para>Set to True if the calling application is a console application: otherwise, False.</para>
        /// </param>
        /// <param name="hwnd">
        ///   <para>The window handle to the parent window if the application is 
        /// a Windows application. The handle is ignored if <paramref name="isConsole" /> is True.</para>
        /// </param>
        /// <remarks>
        ///   <para>The information is used to determine how to prompt the user for credentials if 
        /// they are not specified in the job. If you do not call this method, the console is used.</para>
        ///   <para>For Windows HPC Server 2008 R2, you can indicate that the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> method should generate an exception if cached credentials cannot be used for the job. To indicate that  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> should generate an exception in that case, specify a value of false for the <paramref name="isConsole" /> parameter and an  
        /// <see cref="System.IntPtr" /> object for a value of -1 as the <paramref name="hwnd" /> parameter when you call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetInterfaceMode(System.Boolean,System.IntPtr)" /> method. For example, <c>SetInterfaceMode(false, new IntPtr(-1));</c>.</para> 
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />
        void SetInterfaceMode(bool isConsole, IntPtr hwnd);

        /// <summary>
        ///   <para>Creates a job.</para>
        /// </summary>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface that defines the newly created job. The job uses the default job template to specify the job’s default property values and constraints.</para> 
        /// </returns>
        /// <remarks>
        ///   <para>To specify a specific template to use, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetJobTemplate(System.String)" /> method. </para>
        ///   <para>After defining the job (setting property values and adding tasks), call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> method to add the job to the scheduler and scheduling queue. If the job is not ready to be added to the scheduling queue, you can call the  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" /> method to add the job to the scheduler.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853426(v=vs.85).aspx">Creating and Submitting a Job</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.CloneJob(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />
        ISchedulerJob CreateJob();

        /// <summary>
        ///   <para>Retrieves the specified job from the scheduler.</para>
        /// </summary>
        /// <param name="id">
        ///   <para>The job to retrieve. </para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface that you can use to access the properties of the job.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The scheduler assigns the identifier when you call the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> method to add the job to the scheduler. To get the identifier, access the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Id" /> property.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/en-us/library/cc853435(v=vs.85).aspx">Getting a List of Jobs</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobIdList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJobRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJobEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" 
        /// /> 
        ISchedulerJob OpenJob(int id);

        /// <summary>
        ///   <para>Clones the specified job.</para>
        /// </summary>
        /// <param name="jobId">
        ///   <para>The identifier of the job to clone.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface that defines the newly cloned job.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Only the owner of the job can clone the job. The job can be in any state when cloned. </para>
        ///   <para>The state of the new job is 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobState.Configuring" />. After modifying the job (if necessary), call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> method to schedule the new job to run. If the job is not ready to run, call the  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" /> method to save the job in the scheduler.</para>
        ///   <para>The method copies all the tasks (and instances for a parametric task) and the following subset of job and task properties.</para>
        ///   <para>Cloned job properties</para>
        ///   <para>Cloned task properties</para>
        ///   <list type="table">
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.AutoCalculateMax" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.CommandLine" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.AutoCalculateMin" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.DependsOn" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.EncryptedPassword" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.EndValue" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.FailOnTaskFailure" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.Id" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.IsExclusive" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.IncrementValue" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.JobTemplate" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.IsExclusive" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.JobType" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.IsParametric" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MaxCores" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.IsRerunnable" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MaxCoresPerNode" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.MaxCores" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MaxMemory" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.MaxNodes" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MaxNodes" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.MaxSockets" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MaxSockets" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.MinCores" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MinCores" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.MinNodes" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MinCoresPerNode" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.MinSockets" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MinMemory" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.Name" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MinNodes" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.RequiredNodes" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.MinSockets" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.RuntimeSeconds" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.Name" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.StartValue" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.NextJobTaskId" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.StdErrFilePath" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.NodeGroups" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.StdInFilePath" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.OrderBy" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.StdOutFilePath" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.Owner" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.WorkDirectory" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.Priority" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para />
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.Project" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para />
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.RequestedNodes" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para />
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.RuntimeSeconds" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para />
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.RunUntilCanceled" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para />
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.SoftwareLicense" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para />
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.UnitType" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para />
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.UserName" />
        ///         </para>
        ///       </term>
        ///       <description>
        ///         <para />
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853421(v=vs.85).aspx">Cloning a Job</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.CreateJob" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RestoreFromXml(System.String)" />
        ISchedulerJob CloneJob(int jobId);

        /// <summary>
        ///   <para>Adds the specified job to the scheduler.</para>
        /// </summary>
        /// <param name="job">
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface of the job to add to the scheduler.</para>
        /// </param>
        /// <remarks>
        ///   <para>Use this method when you want to save the job in the scheduler but are not ready to add the job to 
        /// the scheduling queue. Adding the job to the scheduling queue also adds the job to the scheduler if it does not already exist (see  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />).</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.CreateJob" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />
        void AddJob(ISchedulerJob job);

        /// <summary>
        ///   <para>Adds a job to the scheduling queue using the job interface to identify the job.</para>
        /// </summary>
        /// <param name="job">
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface that identifies the job to add to the scheduling queue.</para>
        /// </param>
        /// <param name="username">
        ///   <para>The name of the RunAs user, in the form 
        /// domain\username. The user name is limited to 80 characters.</para>
        ///   <para>If this parameter is NULL, the method uses the name in 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UserName" />, if set; otherwise, the method uses the owner of the job.</para>
        ///   <para>If this parameter is an empty string, the service searches the credentials cache for the credentials to use. If the cache contains 
        /// the credentials for a single user, those credentials are used. However, if 
        /// multiple credentials exist in the cache, the user is prompted for the credentials.</para> 
        /// </param>
        /// <param name="password">
        ///   <para>The password for the RunAs user. The password is limited to 127 characters.</para>
        ///   <para>If this parameter is null or empty, the method uses 
        /// the cached password if cached; otherwise, the user is prompted for the password.</para>
        /// </param>
        /// <remarks>
        ///   <para>If the specified job does not exist in the scheduler, the 
        /// method adds the job to the scheduler before adding the job to the scheduling queue.</para>
        ///   <para>If the submit succeeds, the state of the job is Submitted (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" />). After the job is validated, the job moves to the Queued state. The job moves to the Running state when all required resources are available and the scheduler starts the job.</para> 
        ///   <para>Tasks are started in the order in which they were added to the job, except if there is a dependency (see the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.DependsOn" /> property).</para>
        ///   <para>You can call this method on a job that does not contain tasks to reserve resources for the job. If the job's 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" /> property is 
        /// True, the job is scheduled and runs indefinitely or until it exceeds the run-time limit set in the job's 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" /> property (then the job is canceled). If the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" /> property is 
        /// False, the job moves to the Finished state.</para>
        ///   <para>For Windows HPC Server 2008 R2, you can indicate that the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> method should generate an exception if cached credentials cannot be used for the job. To indicate that  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" /> should generate an exception in that case, specify a value of false for the isConsole parameter and an  
        /// <see cref="System.IntPtr" /> object for a value of -1 as the hwnd parameter when you call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetInterfaceMode(System.Boolean,System.IntPtr)" /> method. For example, <c>SetInterfaceMode(false, new IntPtr(-1));</c>.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853426(v=vs.85).aspx">Creating and Submitting a Job</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SetInterfaceMode(System.Boolean,System.IntPtr)" />
        void SubmitJob(ISchedulerJob job, string username, string password);

        /// <summary>
        ///   <para>Adds the job to the scheduling queue using the job identifier to identify the job.</para>
        /// </summary>
        /// <param name="jobId">
        ///   <para>An identifier that uniquely identifies the job in the cluster to add to the scheduling queue.</para>
        /// </param>
        /// <param name="username">
        ///   <para>The name of the RunAs user, in the form 
        /// domain\username. The user name is limited to 80 characters.</para>
        ///   <para>If this parameter is NULL, the method uses the name in 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UserName" />, if set; otherwise, the method uses the owner of the job.</para>
        ///   <para>If this parameter is an empty string, the service searches the credentials cache for the credentials to use. If the cache contains 
        /// the credentials for a single user, those credentials are used. However, if 
        /// multiple credentials exist in the cache, the user is prompted for the credentials.</para> 
        /// </param>
        /// <param name="password">
        ///   <para>The password for the RunAs user. The password is limited to 127 characters.</para>
        ///   <para>If this parameter is null or empty, the method uses 
        /// the cached password if cached; otherwise, the user is prompted for the password.</para>
        /// </param>
        /// <remarks>
        ///   <para>If the specified job does not exist in the scheduler, the 
        /// method adds the job to the scheduler before adding the job to the scheduling queue.</para>
        ///   <para>If the submit succeeds, the state of the job is Submitted (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" />). After the job is validated, the job moves to the Queued state. The job moves to the Running state when all required resources are available and the scheduler starts the job.</para> 
        ///   <para>Tasks are started in the order in which they were added to the job, except if there is a dependency (see the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.DependsOn" /> property).</para>
        ///   <para>You can call this method on a job that does not contain tasks to reserve resources for the job. If the job's 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" /> property is true, the job is scheduled and runs indefinitely or until it exceeds the run-time limit set in the job's  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" /> property (then the job is canceled). If the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" /> property is false, the job moves to the Finished state.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853421(v=vs.85).aspx">Cloning a Job</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />
        void SubmitJobById(int jobId, string username, string password);

        /// <summary>
        ///   <para>Cancels the specified job and provides a message to the user that explains why you canceled the job.</para>
        /// </summary>
        /// <param name="jobId">
        ///   <para>The job to cancel.</para>
        /// </param>
        /// <param name="message">
        ///   <para>A message that describes the reason why the job was canceled. The message is limited to 320 characters. Can be empty.</para>
        ///   <para>The message is stored with the job. To get the message, access the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ErrorMessage" /> property.</para>
        /// </param>
        /// <remarks>
        ///   <para>The job is removed from the queue but remains in 
        /// the scheduler. The TTLCompletedJobs cluster parameter (for details, see the Remarks section of  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" />) determines when the job is removed from the scheduler after it has been canceled.</para> 
        ///   <para>To cancel a job, the state of the job must be configuring, submitted, validating, queued, or running. If the job is running tasks when 
        /// you cancel the job, the tasks are stopped and marked as failed. If you queue the job again, the tasks in the job are treated as follows:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Tasks that were finished when you canceled the job stay finished.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Tasks that were explicitly canceled before they ran and that are currently in the canceled state stay canceled.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>All other tasks are queued, including those that failed.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        ///   <para>To cancel a command job, the user must be running as an administrator.</para>
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String)" /> method honors all cancelation grace periods for currently running tasks, and runs the node release task if one exists for the job on all nodes currently allocated to the job.</para> 
        ///   <para>To cancel a job the specified job in Windows HPC Server 2008 R2 so that it stops immediately without using the 
        /// grace period for canceling the tasks in the job or running the node release task for the job, call the  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String,System.Boolean)" /> method. Calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String)" /> method is equivalent to calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String,System.Boolean)" /> with the isForced parameter set to 
        /// False.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.CreateJob" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />
        void CancelJob(int jobId, string message);

        /// <summary>
        ///   <para>Moves the job to the Configuring state.</para>
        /// </summary>
        /// <param name="jobId">
        ///   <para>Identifies the job to move to the Configuring state.</para>
        /// </param>
        /// <remarks>
        ///   <para>You can move the job to the Configuring state from the following states (see 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" />):</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobState.Canceled" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobState.Failed" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobState.Queued" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>
        ///           <see cref="Microsoft.Hpc.Scheduler.Properties.JobState.Submitted" />
        ///         </para>
        ///       </description>
        ///     </item>
        ///   </list>
        ///   <para>Typically, you would move a job back to the Configuring state if a task failed so that you can fix the task and run the job 
        /// again. When the job is run again, only tasks which failed or tasks which 
        /// did not run will be run. Tasks which finished successfully will not be run again.</para> 
        ///   <para>You should also call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.ConfigureJob(System.Int32)" /> method for a job that is in the queued state before you modify job properties such as  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NodeGroups" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfCores" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfNodes" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MaximumNumberOfSockets" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfCores" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfNodes" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.MinimumNumberOfSockets" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SoftwareLicense" />, and 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" />.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />
        void ConfigureJob(int jobId);

        /// <summary>
        ///   <para>Creates a task identifier that identifies a task.</para>
        /// </summary>
        /// <param name="jobTaskId">
        ///   <para>A sequential, numeric identifier that uniquely identifies the task within the job. Task identifiers begin with 1.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" /> interface that identifies a task in a job.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Use this method to create an identifier that identifies a task or parametric 
        /// task. To create a task identifier that identifies an instance of a parametric task, call the  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateParametricTaskId(System.Int32,System.Int32)" /> method.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853421(v=vs.85).aspx">Cloning a Job</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.CreateParametricTaskId(System.Int32,System.Int32)" />
        string CreateTaskId(Int32 jobTaskId);

        /// <summary>
        ///   <para>Creates a task identifier that identifies an instance of parametric task.</para>
        /// </summary>
        /// <param name="jobTaskId">
        ///   <para>A sequential, numeric identifier that uniquely identifies the parametric task within the job. Task identifiers begin with 1.</para>
        /// </param>
        /// <param name="instanceId">
        ///   <para>A sequential, numeric identifier that uniquely identifies the instance of a parametric task.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" /> interface that identifies the instance of a parametric task.</para>
        /// </returns>
        /// <remarks>
        ///   <para>To create a task identifier object that identifies a task or parametric task, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateTaskId(System.Int32)" /> method.</para>
        /// </remarks>
        string CreateParametricTaskId(Int32 jobTaskId, Int32 instanceId);

        /// <summary>
        ///   <para>Sets a cluster-wide environment variable.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The name of the variable.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The value of the variable.</para>
        /// </param>
        /// <remarks>
        ///   <para>The sum of all environment variables is limited to 2,048 characters.</para>
        ///   <para>You can use this method to add, delete, or update an environment variable. You cannot delete or 
        /// update an HPC-defined variable. If you try to delete or 
        /// update an HPC-defined variable (for example, CCP_CLUSTER_NAME), the operation is ignored.</para> 
        ///   <para>The method uses a case-insensitive comparison to find the environment 
        /// variable. If the variable is not found, the method adds the variable.  
        /// If the variable is found, the method updates its value unless the value is empty or null, in which case the method deletes the variable.</para>
        ///   <para>To set environment variables for a task, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.SetEnvironmentVariable(System.String,System.String)" /> method.</para>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.EnvironmentVariables" />
        void SetEnvironmentVariable(string name, string value);

        /// <summary>
        ///   <para>Retrieves the cluster-wide environment variables.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains a collection of environment variables defined for the cluster.</para> 
        /// </value>
        /// <remarks>
        ///   <para>The variables are available to all tasks on all nodes. To set a cluster-wide environment variable, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetEnvironmentVariable(System.String,System.String)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EnvironmentVariables" />
        IDictionary<string, string> EnvironmentVariables { get; }

        /// <summary>
        ///   <para>Sets a configuration parameter for the cluster.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The name of the parameter. The name is case-insensitive.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The value of the parameter.</para>
        /// </param>
        /// <remarks>
        ///   <para>Only a user with administrator privileges can set the cluster's configuration parameters.</para>
        ///   <para>You can only update HPC configuration parameters; you cannot delete them. The method fails if the HPC parameter value is not valid.</para>
        ///   <para>To retrieve the current configuration values, access the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.ClusterParameters" /> property.</para>
        ///   <para>The following are the supported configuration parameters.</para>
        ///   <para>Parameter</para>
        ///   <para>Description</para>
        ///   <list type="table">
        ///     <item>
        ///       <term>
        ///         <para>ActivationFilterProgram</para>
        ///       </term>
        ///       <description>
        ///         <para>The absolute path to an application that determines whether a job should be run. This application 
        /// is run for each job before the job is started. This application returns 0 if the job should be started and a nonzero value otherwise.  
        /// It must accept a single command-line argument, which is an absolute path to an XML file that specifies the terms of the job. The job 
        /// terms are an attribute of the Job element. The ExtendedTerm element contains the name/value pairs for application-defined extended job terms. For more information, see Job Schema.</para> 
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>ActivationFilterTimeout</para>
        ///       </term>
        ///       <description>
        ///         <para>Time-out value for the activation filter, in seconds. By default, the filter must complete in 15 seconds.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>AutomaticGrowthEnabled</para>
        ///       </term>
        ///       <description>
        ///         <para>Determines whether the server can use more resources 
        /// for the job as they become available. If enabled (True), the server will allocate more resources  
        /// to job if it determines that the job will benefit from the increase and there are 
        /// more resources available; the server will not allocate more resources beyond the maximum requested. The default is True.</para> 
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>AutomaticShrinkEnabled</para>
        ///       </term>
        ///       <description>
        ///         <para>Determines whether the server can release extra resources when they are no longer needed by the job. If enabled (True), the server 
        /// will remove resources from a job if it determines that the job no longer needs 
        /// the resources; the server will not shrink the resources beyond the minimum requested. The default is True.</para> 
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>BackfillLookahead</para>
        ///       </term>
        ///       <description>
        ///         <para>Number of jobs that the scheduler searches to find jobs that 
        /// can backfill the jobs at the top of the queue. The default is 100. The following lists the possible values:</para>
        ///         <list type="bullet">
        ///           <item>
        /// <description>
        ///   <para>If less than zero, search the entire job queue.</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <description>
        ///   <para>If zero, do not backfill jobs.</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <description>
        ///   <para>If greater than zero, the value is the number of jobs to search.</para>
        /// </description>
        ///           </item>
        ///         </list>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>EventLogLevel</para>
        ///       </term>
        ///       <description>
        ///         <para>Sets the error log level. The log level can be one of the following values:</para>
        ///         <list type="bullet">
        ///           <item>
        /// <description>
        ///   <para>Off</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <description>
        ///   <para>Critical</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <description>
        ///   <para>Error</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <description>
        ///   <para>Warning</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <description>
        ///   <para>Information</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <description>
        ///   <para>Verbose</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <description>
        ///   <para>ActivityTracing</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <description>
        ///   <para>All</para>
        /// </description>
        ///           </item>
        ///         </list>
        ///         <para>The default is Warning. For a description of these values, see the 
        /// <see cref="System.Diagnostics.SourceLevels" /> enumeration.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>HeartbeatInterval</para>
        ///       </term>
        ///       <description>
        ///         <para>Interval for the scheduler to attempt to contact the node. The default interval is 30 seconds.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>InactivityCount</para>
        ///       </term>
        ///       <description>
        ///         <para>Number of times the scheduler must attempt 
        /// to contact a node before it can declare the node unreachable. The default number is 3.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>JobRetryCount</para>
        ///       </term>
        ///       <description>
        ///         <para>Maximum number of times the system will rerun a job when a system error 
        /// occurs (not when a job error occurs). The default number is 3. The job's status is set to Failed when the count is reached.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>JobRuntime</para>
        ///       </term>
        ///       <description>
        ///         <para>Default run time for any job if not specified on the job. If the run time of the 
        /// job exceeds this limit, the job is terminated and its status is set 
        /// to failed. Specify the run time as a string integer. The default is no limit.</para> 
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>PreemptionType</para>
        ///       </term>
        ///       <description>
        ///         <para>Determines whether a higher priority job can preempt a lower priority job. The possible values are:</para>
        ///         <para>Value</para>
        ///         <para>Description</para>
        ///         <list type="table">
        ///           <item>
        /// <term>
        ///   <para>Graceful</para>
        /// </term>
        /// <description>
        ///   <para>A running job can be preempted only after the tasks that are 
        /// currently running complete. Any remaining tasks in the job will not run. This is the default.</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <term>
        ///   <para>Immediate</para>
        /// </term>
        /// <description>
        ///   <para>A running job can be preempted immediately.</para>
        /// </description>
        ///           </item>
        ///           <item>
        /// <term>
        ///   <para>None</para>
        /// </term>
        /// <description>
        ///   <para>Running jobs cannot be preempted by higher priority jobs.</para>
        /// </description>
        ///           </item>
        ///         </list>
        ///         <para>To set the preemption policy for a job, see the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanPreempt" /> property.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>SpoolDir</para>
        ///       </term>
        ///       <description>
        ///         <para>A UNC path to the folder that will spool the output from a remote command. The default is \\HeadNodeName\CcpSpoolDir.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>SubmissionFilterProgram</para>
        ///       </term>
        ///       <description>
        ///         <para>The absolute path to an application that determines whether a job should be submitted to the queue. This 
        /// application is run for each job before the job is added to the scheduling queue. This application returns 0 if the job should be submitted and a  
        /// nonzero value otherwise. It must accept a single command-line argument, which is an absolute path to an XML file that specifies the terms of the job. 
        /// The job terms are an attribute of the Job element. The ExtendedTerms element contains the name/value pairs for application-defined extended job terms. For more information, see Job Schema.</para> 
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>SubmissionFilterTimeout</para>
        ///       </term>
        ///       <description>
        ///         <para>Time-out value for the submission filter, in seconds. By default, the filter must complete in 15 seconds.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>TaskRetryCount</para>
        ///       </term>
        ///       <description>
        ///         <para>Maximum number of times the system will rerun a task when a system error 
        /// occurs (not when a task error occurs). The default number is 3. The task's status is set to Failed when the count is reached.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <term>
        ///         <para>TTLCompletedJobs</para>
        ///       </term>
        ///       <description>
        ///         <para>The minimum number of days that a completed job will be kept. A 
        /// completed job is a job whose status is finished, canceled, or failed. The default interval is five days; this is the recommended minimum.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.ClusterParameters" />
        void SetClusterParameter(string name, string value);

        /// <summary>
        ///   <para>Retrieves the cluster's configuration parameters.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains a collection of configuration parameters for the cluster.</para> 
        /// </value>
        /// <remarks>
        ///   <para>To set the configuration parameters, call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" /> method. For a list of parameters, see the Remarks section of  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" />.</para>
        /// </remarks>
        IDictionary<string, string> ClusterParameters { get; }

        /// <summary>
        ///   <para>Retrieves a list of job template names defined in the cluster.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of job template names.</para>
        /// </returns>
        /// <remarks>
        ///   <para>To get information about a job template, such as the times that the job template was created and most recently changed, use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateInfo(System.String)" /> method. To get a reader that provides access to the XML data that represents the job template, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateXml(System.String)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.JobTemplate" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetJobTemplate(System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateXml(System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateInfo(System.String)" />
        ICollection<string> GetJobTemplateList();

        /// <summary>
        ///   <para>Retrieves a list of node group names defined in the cluster.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of node group names.</para>
        /// </returns>
        ICollection<string> GetNodeGroupList();

        /// <summary>
        ///   <para>Retrieves the list of nodes in the specified node group.</para>
        /// </summary>
        /// <param name="nodeGroup">
        ///   <para>A node group name.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains the collection of node names in the group.</para>
        /// </returns>
        /// <remarks>
        ///   <para>To retrieve the available node group names, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetNodeGroupList" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetNodeGroupList" />
        ICollection<string> GetNodesInNodeGroup(string nodeGroup);

        /// <summary>
        ///   <para>Creates an empty collection to which you can add name/value pairs.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface to which you can add name/value pairs.</para>
        /// </returns>
        IDictionary<string, string> CreateNameValueCollection();

        /// <summary>
        ///   <para>Creates an empty collection to which you add string values.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface to which you add string values.</para>
        /// </returns>
        ICollection<string> CreateStringCollection();

        /// <summary>
        ///   <para>Deletes the credentials that were cached for the specified user.</para>
        /// </summary>
        /// <param name="userName">
        ///   <para>The name of the RunAs user, in the form domain\user. The user name is limited 
        /// to 80 characters. If this parameter is NULL or empty, the method deletes all credentials that have been cached by the calling user.</para>
        /// </param>
        /// <remarks>
        ///   <para>Only the user that cached the credentials can delete the credentials.</para>
        ///   <para>Note that the name that you provide must match the name used to submit the job or command. For example, if 
        /// the user specified domain\username, you must specify domain\username when calling this method. 
        /// If the user specified username, you must specify username when calling this method.</para> 
        ///   <para>The method succeeds whether or not it finds the specified user in the cache.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SetCachedCredentials(System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.StartWithCredentials(System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />
        void DeleteCachedCredentials(string userName);

        /// <summary>
        ///   <para>Sets the credentials for the specified user in the credential 
        /// cache, so that the job scheduler can use the credentials for submitting jobs.</para>
        /// </summary>
        /// <param name="userName">
        ///   <para>A 
        /// 
        /// <see cref="System.String" /> object that specifies the name of the user for which you want the set the cached credentials, in the form domain\user. The user name is limited to 80 characters.</para> 
        /// </param>
        /// <param name="password">
        ///   <para>A <see cref="System.String" /> object that specifies the password for the user for which you want the set the cached credentials.</para>
        /// </param>
        void SetCachedCredentials(string userName, string password);

        /// <summary>
        ///   <para>Closes the connection between the application and the HPC Job Scheduler Service.</para>
        /// </summary>
        void Close();
        
        /// <summary>
        ///   <para>Cancels the specified job and provides a message to the user that 
        /// explains why you canceled the job, and optionally forces the job to stop immediately. When  
        /// you force the job to stop immediately, this method ignores the grace period for canceling 
        /// the tasks in the job, and does not run the node release task for the job.</para> 
        /// </summary>
        /// <param name="jobId">
        ///   <para>An integer that specifies the identifier of the job that you want to cancel.</para>
        /// </param>
        /// <param name="message">
        ///   <para>A string that specifies a message that describes the reason why you canceled the job. The message is 
        /// limited to 320 characters. Can be empty. The message is stored with the job. To get the message, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ErrorMessage" /> property.</para>
        /// </param>
        /// <param name="isForced">
        ///   <para>A 
        /// 
        /// <see cref="System.Boolean" /> that specifies whether to stop the job immediately without using the grace period for canceling the tasks in the job and without running the node release task, if the job contains one.  
        /// True indicates that the method should stop the job immediately without using the grace 
        /// period for canceling the tasks in the job and without running the node release task.  
        /// False indicates that the method should not stop the job immediately and should use 
        /// the grace period for canceling the tasks in the job and run the node release task.</para> 
        /// </param>
        /// <remarks>
        ///   <para>This method removes the job from the queue, but the job remains in the scheduler. The TTLCompletedJobs cluster parameter determines 
        /// when the job is removed from the scheduler after you have canceled the job. For information about the TTLCompletedJobs cluster parameter, see  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" />.</para>
        ///   <para>To cancel a job, the state of the job must be configuring, submitted, validating, queued, or running. If the job is running tasks when 
        /// you cancel the job, the tasks are stopped and marked as failed. If you queue the job again, the tasks in the job are treated as follows:</para>
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         <para>Tasks that were finished when you canceled the job stay finished.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>Tasks that were explicitly canceled before they ran and that are currently in the canceled state stay canceled.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>All other tasks are queued, including those that failed.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        ///   <para>To cancel a command job, the user must be an administrator.</para>
        ///   <para>Calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String)" /> method is equivalent to calling the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String,System.Boolean)" /> with the <paramref name="isForced" /> parameter set to  
        /// False.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.CancelJob(System.Int32,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.CreateJob" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJobById(System.Int32,System.String,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />
        void CancelJob(int jobId, string message, bool isForced);
      
        /// <summary>
        ///   <para>Deletes a pool on a cluster based on the supplied name. An exception is thrown if the pool doesn’t exist.</para>
        /// </summary>
        /// <param name="poolName">
        ///   <para>The name of the pool on the cluster to be deleted.</para>
        /// </param>
        /// <remarks>
        ///   <para>The pool will not be deleted until all running and queued jobs are finished.</para>
        ///   <para>If there is a job template using the pool, an exception is thrown. 
        /// If there are running or queued jobs, those jobs will be moved to the default pool.</para>
        /// </remarks>
        void DeletePool(string poolName);

        /// <summary>
        ///   <para>Deletes a pool on a cluster based on the supplied name. An exception is thrown if the pool doesn’t exist.</para>
        /// </summary>
        /// <param name="poolName">
        ///   <para>The name of the pool on the cluster that will be deleted.</para>
        /// </param>
        /// <param name="force">
        ///   <para>If true, all job templates that reference the pool are moved to the default pool 
        /// in addition to any jobs that use that template. After the job templates and jobs are moved  
        /// to the default pool, the pool is deleted. If false, the pool is only deleted if no 
        /// job template uses that pool. If a job template is currently using the pool, an exception is thrown.</para> 
        /// </param>
        void DeletePool(string poolName, bool force);
      
        /// <summary>
        ///   <para>Uploads a certificate in the client’s certificate store to the scheduler for running jobs as this user.</para>
        /// </summary>
        /// <param name="userName">
        ///   <para>The name of the RunAs user, in the form 
        /// domain\user. The user name is limited to 80 characters.</para>
        /// </param>
        /// <param name="thumbprint">
        ///   <para>The thumbprint of the certificate to upload.</para>
        /// </param>
        /// <remarks>
        ///   <para>If <paramref name="userName" /> is null or empty, the owner is treated as the user. Only SYSTEM on the 
        /// cluster headnode is allowed to submit credentialsas another user. To set the user as the owner, set username as either null or empty.</para>
        ///   <para>If <paramref name="thumbprint" /> is null, the certificate store will be searched for relevant certificates. 
        /// If there are multiple certificates, the user will be prompted to choose a password depending on the interface mode.</para>
        /// </remarks>
        void SetCertificateCredentials(string userName, string thumbprint);

        /// <summary>
        ///   <para>Uploads a certificate encoded with a password to the scheduler to use for running jobs as this user.</para>
        /// </summary>
        /// <param name="userName">
        ///   <para>The name of the RunAs user, in the form 
        /// domain\user. The user name is limited to 80 characters.</para>
        /// </param>
        /// <param name="pfxPassword">
        ///   <para>The password for the RunAs user. The password is limited to 127 characters.</para>
        /// </param>
        /// <param name="certBytes">
        ///   <para>A <see cref="System.Byte" /> array that contains the certificate.</para>
        /// </param>
        void SetCertificateCredentialsPfx(string userName, string pfxPassword, byte[] certBytes);

        /// <summary>
        ///   <para>Enrolls the user in a certificate based on the supplied template.</para>
        /// </summary>
        /// <param name="templateName">
        ///   <para>The template name. If the scheduler specifies an hpcsoftcard template, this parameter is ignored.</para>
        /// </param>
        /// <returns>
        ///   <para>Returns a <see cref="System.String" /> object that contains the certificate thumbprint.</para>
        /// </returns>
        string EnrollCertificate(string templateName);
        #endregion

        #region V3 SP4 scheduler methods /props

        /// <summary>
        ///   <para>Returns the name of the active head node.</para>
        /// </summary>
        /// <returns>
        ///   <para>Returns the name of the head node as a <see cref="System.String" />.</para>
        /// </returns>
        /// <remarks>
        ///   <para>This method was introduced in Windows HPC Server 2008 R2 Service Pack 4 (SP4) and is not available in earlier versions.</para>
        /// </remarks>
        string GetActiveHeadNode();

        #endregion

        #region V4 scheduler methods /props

        /// <summary>
        ///   <para>Sets the email credentials by using the specified username and password.</para>
        /// </summary>
        /// <param name="userName">
        ///   <para>The username.</para>
        /// </param>
        /// <param name="password">
        ///   <para>The password.</para>
        /// </param>
        /// <remarks>
        ///   <para>Email credentials use the username and password of the email 
        /// account as credentials for submitting jobs. To delete the email credentials, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.DeleteEmailCredentials" /> method.</para>
        ///   <para>This method was introduced in Microsoft HPC Pack 2012 and is not available in earlier versions.</para>
        /// </remarks>
        void SetEmailCredentials(string userName, string password);

        /// <summary>
        ///   <para>Removes the email credentials for running jobs.</para>
        /// </summary>
        /// <remarks>
        ///   <para>Email credentials use the username and password of the email 
        /// account as credentials for submitting jobs. To set the email credentials, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetEmailCredentials(System.String,System.String)" /> method.</para>
        ///   <para>This method was introduced in Windows HPC Server 2008 R2 Service Pack 4 (SP4) and is not available in earlier versions.</para>
        ///   <para />
        /// </remarks>
        void DeleteEmailCredentials();

        #endregion

        #region V4SP1 scheduler methods / properties
        /// <summary>
        /// <para>Requeue job</para>
        /// </summary>
        /// <param name="jobId"></param>
        void RequeueJob(int jobId);

        #endregion

        #region V4SP3 scheduler methods / properties

        /// <summary>
        /// <para>Cancel a job with the specified id</para>
        /// </summary>
        /// <param name="jobId">the id of the job.</param>
        /// <param name="message">the cancel message.</param>
        /// <param name="isForced">true to cancel the job without running the release tasks or allowing the tasks any grace period</param>
        /// <param name="isGraceful">true to cancel the job and wait for running tasks end.</param>
        void CancelJob(int jobId, string message, bool isForced, bool isGraceful);

        /// <summary>
        /// <para>Finish a job with the specified id</para>
        /// </summary>
        /// <param name="jobId">the id of the job.</param>
        /// <param name="message">the finish message.</param>
        /// <param name="isForced">true to finish the job without running the release tasks or allowing the tasks any grace period</param>
        /// <param name="isGraceful">true to finish the job and wait for running tasks end.</param>
        void FinishJob(int jobId, string message, bool isForced, bool isGraceful);

        #endregion

        #region V5SP2 Scheduler methods / properties

        /// <summary>
        /// <para>This method is used to delete a job.</para>
        /// </summary>
        /// <param name="jobId">
        /// <para>The id of the job.</para>
        /// </param>
        void DeleteJob(int jobId);

        #endregion
    }
}