namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcClient
{
    /// <summary>
    ///   <para>This interface allows a filter to manage the lifespan of its in-memory state. The methods in this interface 
    /// allow for explicit creation and clean-up of a threading model and such things as opening files and connections to external services.</para>
    /// </summary>
    /// <remarks>
    ///   <para>This interface is optional. If <c>IFilterLifespan</c> is implemented, the HPC Job Scheduler 
    /// Service will call <c>OnFilterLoad</c> after the DLL is loaded, and <c>OnFilterUnload</c> before the DLL is unloaded.</para>
    ///   <para>DLL filters are loaded (instantiated) in the following cases:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>On HPC Job Scheduler Service start-up: When the 
    /// scheduler starts up, it enumerates all DLL filters that are listed in job templates and loads them.</para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>When the DLL filter is first added to a job template:  When a 
    /// cluster administrator specifies a DLL filter in a job template, the HPC Job Scheduler tries to load the DLL as part of validation.</para>
    ///       </description>
    ///     </item>
    ///   </list>
    ///   <para>DLL filters are unloaded in the following cases: </para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>On HPC Job Scheduler Service clean shutdown: During a clean shutdown, all DLL filters are unloaded.</para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>When the last reference to a DLL filter is removed from all job templates: Any time that job templates are 
    /// changed in the system (added, edited, or removed), the HPC Job Scheduler Service identifies 
    /// any orphaned filters (loaded filters that are not referenced by any template) and unloads them.</para> 
    ///       </description>
    ///     </item>
    ///   </list>
    ///   <para>All calls to filter methods are time bounded and are aborted if they exceed the allowed time. </para>
    ///   <para>DLLs that contain more than one implementation of this interface are rejected by the HPC Job Scheduler Service.</para>
    /// </remarks>
    public interface IFilterLifespan
    {
        /// <summary>
        ///   <para>The HPC Job Scheduler Service calls this method if it was able to successfully load the DLL. This method should 
        /// implement any actions that are required to initialize in-memory state. For 
        /// example, you can open connections to remote services, create threads, or open files.</para> 
        /// </summary>
        /// <remarks>
        ///   <para>If you create your own threading model, ensure that all exceptions are caught. Uncaught 
        /// exceptions or other issues in the filter DLL can cause the HPC Job Scheduler Service process to terminate.</para>
        ///   <para>The DLL filters are loaded in the same process as the 
        /// HPC Job Scheduler Service. Each filter is loaded in its own Application Domain. This  
        /// means that the filter is sharing system resources such as CPU, disk IO, 
        /// and memory with the scheduler. Issues such as memory leaks can adversely affect scheduler performance.</para> 
        /// </remarks>
        void OnFilterLoad();

        /// <summary>
        ///   <para>The HPC Job Scheduler Service calls this method before the DLL is unloaded. This method should implement any actions 
        /// required to finalize in-memory state. For example, you can close 
        /// connections to remote services, signal threads to exit cleanly, or close files.</para> 
        /// </summary>
        /// <remarks>
        ///   <para>The HPC Job Scheduler Service unloads a filter’s Application Domain immediately after the return from <c>OnFilterUnload</c>. 
        /// Ensure that all threads that the filter created (explicitly or implicitly) exit in a timely fashion. If the  
        /// filter’s Application Domain cannot be unloaded successfully, the HPC Job Scheduler Service will be terminated. For more information 
        /// about avoiding these issues, see <see href="http://social.technet.microsoft.com/wiki/contents/articles/best-practices-for-writing-job-template-level-filters-for-the-hpc-job-scheduler-service.aspx">Best practices for writing job template level filters for the HPC Job Scheduler Service</see>.</para> 
        /// </remarks>
        void OnFilterUnload();
    }
}
