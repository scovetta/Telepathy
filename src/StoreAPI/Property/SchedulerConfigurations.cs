using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines when a higher priority job can preempt a lower priority job. </para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const None = 0
    /// const Graceful = 1
    /// const Immediate = 2</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" />
    public enum PreemptionMode
    {
        /// <summary>
        ///   <para>Running jobs cannot be preempted by higher priority jobs. This enumeration member represents a value of 0.</para>
        /// </summary>
        None = 0,
        /// <summary>
        ///   <para>A running job can be preempted only after the tasks that are currently running 
        /// complete. Any remaining tasks in the job will not run. This enumeration member represents a value of 1.</para>
        /// </summary>
        Graceful = 1,
        /// <summary>
        ///   <para>A running job can be preempted immediately. This enumeration member represents a value of 2.</para>
        /// </summary>
        Immediate = 2,
    }

    /// <summary>
    ///   <para>Defines the processor affinity settings that control the association between tasks and cores.</para>
    /// </summary>
    /// <remarks>
    ///   <para>In Windows HPC Server 2008, processor affinity is set in the following ways:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>Through the HPC Node Manager Service.</para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>Through the mpiexec command, which provides an -affinity 
    /// option to set processor affinity on all of the ranks within a Message Passing Interface (MPI) application. </para>
    ///       </description>
    ///     </item>
    ///   </list>
    ///   <para>Processor affinity cannot be set by both of the above methods 
    /// simultaneously. To enable you to control how processor affinity is set, particularly for  
    /// MPI tasks and exclusive tasks, Windows HPC 2008 SP1 provides an AffinityType cluster-wide 
    /// parameter. The values of this enumeration correspond to the possible settings for that cluster-wide parameter.</para> 
    /// </remarks>
    public enum AffinityMode
    {
        /// <summary>
        ///   <para>Directs the HPC Node Manager Service to set the processor affinity for any task to which 
        /// an entire node is not allocated. This setting is the best choice for jobs such as parameter sweeps  
        /// and Service-Oriented Architecture (SOA) jobs, for which multiple instances of the application can run per node and for 
        /// which you want these instances to be isolated from each other. This setting corresponds to the affinity behavior in Windows HPC Server 2008.</para> 
        /// </summary>
        AllJobs = 0,            // V2RTM compatibility mode
                                /// <summary>
                                ///   <para>Directs the HPC Node Manager Service not to set affinity on jobs that are marked as exclusive. This setting is the ideal choice for jobs that contain only 
                                /// one task, because this setting enables that task to take advantage of all cores on the nodes to which the task is assigned. This setting provides new affinity-setting behavior for  
                                /// Windows HPC Server 2008 SP1 and is the default setting for Windows HPC Server 2008 SP1, Windows HPC Server 2008 SP2, and Windows HPC Server 2008 R2, because this setting provides the generally preferred behavior for MPI tasks, which are most likely to be sensitive to 
                                /// affinitization. When you use this setting, MPI tasks in exclusive jobs can take advantage of the –affinity option of the mpiexec command even if the task is not allocated an entire node.</para> 
                                /// </summary>
        NonExclusiveJobs = 1,    // V2 SP1 default
                                 /// <summary>
                                 ///   <para>Directs the HPC Node Manager Service never to set processor affinity on any task. This 
                                 /// setting is excellent choice if you are running MPI tasks and want to be sure that you  
                                 /// can use the –affinity option for the mpiexec command even when jobs share nodes. This setting is 
                                 /// also useful for applications that set their own processor affinity. This setting corresponds to the affinity behavior in Windows Compute Cluster Server 2003.</para> 
                                 /// </summary>
        NoJobs = 2,             // V1 compatibility mode
    }

    /// <summary>
    ///   <para />
    /// </summary>
    public enum SchedulingMode
    {
        /// <summary>
        ///   <para />
        /// </summary>
        Queued = 0,
        /// <summary>
        ///   <para />
        /// </summary>
        Balanced = 1,
        /// <summary>
        /// This is the new balanced mode which calculates the job's resource in one step.
        /// </summary>
        FastBalanced = 2,
    }

    /// <summary>
    ///   <para />
    /// </summary>
    public enum PriorityBias
    {
        /// <summary>
        ///   <para />
        /// </summary>
        NoBias = 0,
        /// <summary>
        ///   <para />
        /// </summary>
        MediumBias = 1,
        /// <summary>
        ///   <para />
        /// </summary>
        HighBias = 2
    }

    /// <summary>
    ///   <para>Enumeration that defines the softcard policy on a cluster. </para>
    /// </summary>
    public enum HpcSoftCardPolicy
    {
        /// <summary>
        ///   <para>Softcards are not permitted.</para>
        /// </summary>
        Disabled = 0,
        /// <summary>
        ///   <para>Softcards or passwords are allowed on the cluster.</para>
        /// </summary>
        Allowed = 1,
        /// <summary>
        ///   <para>Only softcards are permitted. No passwords are accepted for jobs, diagnostics, or service-oriented architecture (SOA).</para>
        /// </summary>
        Required = 2
    }
}
