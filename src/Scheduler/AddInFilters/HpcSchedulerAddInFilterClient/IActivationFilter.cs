using System.IO;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcClient
{
    /// <summary>
    ///   <para>This enumeration defines the responses that a custom job activation filter can return to the 
    /// HPC Job Scheduler Service. These responses tell the HPC Job Scheduler Service how to continue processing the job.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Activation filters run when candidate resources are allocated to a queued or running job (candidate resources for a job are based on the job and 
    /// task properties and on the scheduling policies). The return value from an activation filter determines how the HPC Job Scheduler Service will treat the job and its resources.  
    /// Because activation filters run every time that resources are allocated to a job, the activation filter might run multiple times for the same job. For example, the 
    /// activation filter can run when the job is about to be started, and then run again as new resources are about to be added to the job (dynamic growth).</para> 
    ///   <para>The following code snippet illustrates how to use the filter response enumeration to tell the HPC Job Scheduler 
    /// Service what action to take on the job. In this case, the filter is enforcing a policy that allows all  
    /// jobs to start during non-business hours. If the off hours condition is met, the filter response is set to <c>StartJob</c>. 
    /// The snippet is from the AFHoldUntil sample DLL filter in the Microsoft HPC Pack 2008 R2 <see href="http://go.microsoft.com/fwlink/p/?LinkId=223350">SP2 SDK</see> code samples.</para> 
    ///   <code language="c#">   if (DuringOffHours()) {
    ///         logFile.WriteLine("AF: During Off Peak Hours, job starting");
    ///         return ActivationFilterResponse.StartJob;
    ///     }</code>
    ///   <para>
    /// Distinguishing between Queued and Running jobs
    ///           </para>
    ///   <para>Activation filters evaluate the allocation of resources to Queued and Running jobs. The following table 
    /// summarizes which response values to use for each state. A check mark indicates that the response is valid.</para>
    ///   <para>Response</para>
    ///   <para>Queued</para>
    ///   <para>Running</para>
    ///   <para>The following code snippet illustrates how to get the job state information from the 
    /// job XML. The snippet is from the JobFlexLM sample DLL filter in the SP2 SDK code samples.</para>
    ///   <code language="c#">// Get the job state to determine if this is an initial allocation or growing.
    /// jobidnode = job.SelectSingleNode(@"@State", nsmgr);
    /// if (jobidnode != null)
    /// {
    ///     string JobStateStr = jobidnode.InnerXml;
    ///     if (String.IsNullOrEmpty(JobStateStr))
    ///     {
    ///         LogEvent(@"Unable to extract job state from job file");
    ///         return;
    ///     }
    ///     if (string.Compare(JobStateStr, "Running", StringComparison.InvariantCultureIgnoreCase) == 0)
    ///     {
    ///         _growing = true;
    ///     }
    /// }
    /// else
    /// {
    ///     LogEvent(@"Unable to extract job state from job file");
    ///     return;
    /// }</code>
    ///   <para>
    /// Saving resources for a job
    ///           </para>
    ///   <para>When the filter returns <c>DoNotRunKeepResourcesAllowOtherJobstoSchedule</c>, the number of resources that 
    /// are reserved for the job depend on the Scheduling Mode: In Queued,  
    /// up to the job’s maximum resources are reserved; in Balanced, the minimum 
    /// resources are reserved. For more information, see <see href="http://technet.microsoft.com/library/ff919422(WS.10).aspx">HPC Job Scheduler policy configuration</see>.</para> 
    ///   <para>
    /// Holding a job
    ///           </para>
    ///   <para>When the filter returns <c>HoldJobReleaseResourcesAllowOtherJobsToSchedule</c>, the job is put 
    /// on hold until the date and time specified by the Hold Until  
    /// job property (which can be set by the filter, see code snippet). After the hold period, the job is reevaluated by the filter program.</para>
    ///   <para>If the filter returns this response and no Hold Until value is specified for that job, the job is held for the amount 
    /// of time specified by the Default Hold Duration cluster setting. The cluster 
    /// administrator can change the setting, but the default value is 900 seconds (15 minutes).</para> 
    ///   <para>The following code snippet illustrates how to set the <c>HoldUntil</c> job property. In this case, the 
    /// filter is specifying that certain types of jobs should not be started until after business hours. To ensure that  
    /// a job will not be re-evaluated for activation until after-hours, the filter program sets the <c>HoldUntil</c> job property. The 
    /// snippet is from the AFHoldUntil sample DLL filter in the Microsoft HPC Pack 2008 R2 <see href="http://go.microsoft.com/fwlink/p/?LinkId=223350">SP2 SDK</see> code samples.</para> 
    ///   <code language="c#">
    ///     // If the job is not already set to delay until off peak hours, set it
    ///     // This property should be null, but could be non-null if some other
    ///     // thread has set it after scheduling called the activation filter
    ///     if ((job.HoldUntil == null) || (job.HoldUntil &lt; peakEnd)) {
    ///         job.SetHoldUntil(peakEnd);
    ///         job.Commit();
    ///         logFile.WriteLine("Delay job {0} until off peak hours", jobId);
    ///     } else {
    ///         logFile.WriteLine("Job {0} already set to {1}", jobId, job.HoldUntil);
    ///     }</code>
    /// </remarks>
    public enum ActivationFilterResponse
    {
        /// <summary>
        ///   <para>Start the job on the candidate resources. This enumeration member represents a value of 0.</para>
        /// </summary>
        StartJob = 0,
        /// <summary>
        ///   <para>Do not start the queued job, and do not start any other jobs of equal or 
        /// lower priority until the job passes the activation filter or is canceled. The HPC Job Scheduler Service reruns  
        /// the filter periodically until the job passes or is canceled. The scheduler behavior for this response value on 
        /// a Running job is undefined. Use only when evaluating Queued jobs. This enumeration member represents a value of 1.</para> 
        /// </summary>
        DoNotRunHoldQueue = 1,
        /// <summary>
        ///   <para>Do not start the queued job, but reserve the candidate resources for the job 
        /// and continue scheduling jobs on other resources. The HPC Job Scheduler Service reruns the filter periodically  
        /// until the job passes or is canceled. The scheduler behavior for this response value on a 
        /// Running job is undefined. Use only when evaluating Queued jobs. This enumeration member represents a value of 2.</para> 
        /// </summary>
        DoNotRunKeepResourcesAllowOtherJobsToSchedule = 2,
        /// <summary>
        ///   <para>Hold the job, release the candidate resources, and continue 
        /// scheduling other jobs. See remarks for more information. The scheduler behavior for  
        /// this response value on a Running job is undefined. Use only when evaluating Queued jobs. This enumeration member represents a value of 3. </para>
        /// </summary>
        HoldJobReleaseResourcesAllowOtherJobsToSchedule = 3,
        /// <summary>
        ///   <para>Mark the job as Failed with an error message that the job was failed by the activation filter. The scheduler behavior 
        /// for this response value on a Running job is undefined. Use 
        /// only when evaluating Queued jobs. This enumeration member represents a value of 4.</para> 
        /// </summary>
        FailJob = 4,
        /// <summary>
        ///   <para>Allow the candidate resources to be added to the running job. This enumeration member represents a value of 0.</para>
        /// </summary>
        AddResourcesToRunningJob = 0,
        /// <summary>
        ///   <para />
        /// </summary>
        RejectAdditionOfResources = 1
    };

    /// <summary>
    ///   <para>This interface defines the methods for implementing a custom job activation filter. The HPC Job Scheduler 
    /// Service can call activation filters to provide additional checks and controls when a job is about to start or  
    /// about to grow (get more resources while running). This type of filter is defined as a DLL (its methods 
    /// are called directly by the HPC Job Scheduler Service), and is configured by the administrator at the job template level.</para> 
    /// </summary>
    /// <remarks>
    ///   <para>The HPC Job Scheduler Service can run an activation filter when candidate resources are about to be allocated to a queued or 
    /// running job. The activation filter might run multiple times for the same job. For example, the activation filter can run when the job is  
    /// about to be started, and then run again as new resources are about to be added to the job (dynamic growth).The job activation filter 
    /// can check the job for factors that you want to control, such as unavailability of licenses or exceeded usage time for the submitting user. </para> 
    ///   <para>The filter must implement the <c>FilterActivation</c> and the <c>RevertActivation</c> methods. To evaluate a job, 
    /// the HPC Job Scheduler Service calls the <c>FilterActivation</c> method. The return value from this method determines how  
    /// the HPC Job Scheduler Service will treat the job and its resources. If the call to <c>FilterActivation</c> 
    /// returns <c>JobFailed</c>, then the HPC Job Scheduler Service calls <c>RevertActivation</c> for all previously run filters in the chain.</para> 
    ///   <para>The Microsoft HPC Pack 2008 R2  code 
    /// samples include example activation filters 
    /// (in the  folder).Scheduler/Filters/DLL<see href="http://go.microsoft.com/fwlink/p/?LinkId=223350">SP2 SDK</see></para> 
    ///   <para>The behavior of the HPC Job Scheduler Service is undefined when filters return directly by throwing an exception. 
    /// To avoid this, use try/catch constructions and set a valid return response within the catch area if an exception is thrown.</para>
    ///   <para>All calls to filter methods are time bounded and are aborted if they 
    /// exceed the allowed time. The time allowed for <c>FilterActivation</c> and <c>RevertActivation</c> is determined by the  
    /// Activation Filter Timeout setting that the cluster administrator can configure in Job Scheduler options (the 
    /// timeout for cluster-wide filters also applies to the job template filters).The default timeout is 15 seconds.</para> 
    ///   <para>DLLs that contain more than one implementation of this interface are rejected by the HPC Job Scheduler Service.</para>
    ///   <para>Do not use the scheduler APIs to cancel a job from within a filter. This can cause the HPC Job Scheduler Service to stop responding. To 
    /// cancel a job, the filter should return the <c>FailJob</c> filter response. This response value 
    /// causes the HPC Job Scheduler Service to cancel the job and mark it as Failed.</para> 
    ///   <para>DLL filters are only called if they are specified in the job template that is used 
    /// for the job. For more information, see <see href="http://technet.microsoft.com/library/hh405436(WS.10).aspx">How 
    /// to Add or Remove Job Template Level Submission or Activation Filters</see>.</para> 
    ///   <para>To read about or share best practices, see the TechNet Wiki article 
    /// <see href="http://social.technet.microsoft.com/wiki/contents/articles/4707.best-practices-for-writing-job-template-level-filters-for-the-hpc-job-scheduler-service.aspx">Best practices for writing job template 
    /// level filters for the HPC Job Scheduler Service</see>.</para> 
    /// </remarks>
    public interface IActivationFilter
    {
        // Filters the activation of the specified job.
        // Subsequent action is specified by the return value (ActivationFilterResponse).
        // Calls that exceed ActivationFilterTimeout are terminated.
        /// <summary>
        ///   <para>The HPC Job Scheduler Service calls this method of the activation filter when a queued job is about to 
        /// be started or when a running job is about to be given additional resources. This method should implement the business logic that  
        /// you want the filter to enforce, such as checking for license availability or other factors that you want to check for 
        /// before a job is started. The HPC Job Scheduler Service continues processing the job according to the return value supplied by this method.</para> 
        /// </summary>
        /// <param name="jobXml">
        ///   <para>An XML stream that defines the job properties and tasks. You can parse this stream to 
        /// get the job property values that the filter will check for. In Windows HPC Server 2008 R2, the  
        /// job XML schema is defined by the <c>http://schemas.microsoft.com/HPCS2008R2/scheduler/</c> namespace. Note: Any changes that are made to the job 
        /// through the scheduler APIs while a job is being evaluated by the filters are not reflected in the XML stream.</para> 
        /// </param>
        /// <param name="schedulerPass">
        ///   <para>An integer that represents the number of scheduling passes the HPC Job Scheduler Service has made since the service was started.</para>
        /// </param>
        /// <param name="jobIndex">
        ///   <para>An integer that represents the job’s position in the queue during 
        /// the current scheduling pass. A job might have a different position in the  
        /// queue during different scheduling passes, depending on its priority level and submit time 
        /// compared to other jobs that have been added to or removed from the queue.</para> 
        /// </param>
        /// <param name="backfill">
        ///   <para>A boolean that indicates whether or not the HPC Job Scheduler is attempting to start this job as a backfill job. Backfilling is when a job farther back 
        /// in the queue runs ahead of a job waiting at the front of the queue, as long as the job at the front is not delayed as a result. The  
        /// HPC Job Scheduler Service evaluates backfill feasibility in terms of resource allocation (core/node/socket count and runtime). If a filter is checking for license availability, then consider that a backfill job 
        /// can delay a higher priority job if it is using too many shared licenses. For example, you might want to only allow backfill jobs to start if they do not require licenses.</para> 
        /// </param>
        /// <param name="resourceCount">
        ///   <para>An integer that indicates the number of candidate resources that the HPC Job Scheduler Service can allocate to the 
        /// job during the current scheduling pass. This value will always be between the minimum and maximum resource counts set by the job owner.</para>
        /// </param>
        /// <returns>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.AddInFilter.HpcClient.ActivationFilterResponse" />.</para>
        /// </returns>
        /// <remarks>
        ///   <para>As an input parameter, the filter accepts an XML stream that specifies the properties of the job and its tasks. 
        /// The filter can check the job properties and other data sources to determine if the job should start. Additional parameters are passed  
        /// to the filter to provide information such as the number of candidate resources available during the current scheduling pass, the job’s position 
        /// in the queue, and whether or not backfilling is enabled on the cluster. You can use these parameters to help fine tune filter behavior.</para> 
        ///   <para>The return value from this method determines how the HPC Job Scheduler Service 
        /// will treat the job and its resources. For more information about the return values, see  
        /// <see cref="Microsoft.Hpc.Scheduler.AddInFilter.HpcClient.ActivationFilterResponse" />.</para>
        /// </remarks>
        ActivationFilterResponse FilterActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount);

        /// <summary>
        ///   <para>The HPC Job Scheduler Service calls this method of the activation filter if 
        /// the cluster administrator specifies a chain of activation filters for a job, and one of the  
        /// filters later in the chain fails the job. This method should implement any actions that 
        /// should occur if a job that already passed through the filter is rejected by a later filter.</para> 
        /// </summary>
        /// <param name="jobXml">
        ///   <para>An XML stream that defines the job properties and tasks. You can parse this stream to 
        /// get the job property values that the filter will check for. In Windows HPC Server 2008 R2, the  
        /// job XML schema is defined by the <c>http://schemas.microsoft.com/HPCS2008R2/scheduler/</c> namespace. Note: Any changes that are made to the job 
        /// through the scheduler APIs while a job is being evaluated by the filters are not reflected in the XML stream.</para> 
        /// </param>
        /// <param name="schedulerPass">
        ///   <para>An integer that represents the number of scheduling passes the HPC Job Scheduler Service has made since the service was started.</para>
        /// </param>
        /// <param name="jobIndex">
        ///   <para>An integer that represents the job’s position in the queue during 
        /// the current scheduling pass. A job might have a different position in the  
        /// queue during different scheduling passes, depending on its priority level and submit time 
        /// compared to other jobs that have been added to or removed from the queue.</para> 
        /// </param>
        /// <param name="backfill">
        ///   <para>A boolean that indicates whether or not the HPC Job Scheduler is attempting to start this job as a backfill job. Backfilling is when a job farther back 
        /// in the queue runs ahead of a job waiting at the front of the queue, as long as the job at the front is not delayed as a result. The  
        /// HPC Job Scheduler Service evaluates backfill feasibility in terms of resource allocation (core/node/socket count and runtime). If a filter is checking for license availability, then consider that a backfill job 
        /// can delay a higher priority job if it is using too many shared licenses. For example, you might want to only allow backfill jobs to start if they do not require licenses.</para> 
        /// </param>
        /// <param name="resourceCount">
        ///   <para>An integer that indicates the number of candidate resources that the HPC Job Scheduler Service can allocate to the 
        /// job during the current scheduling pass. This value will always be between the minimum and maximum resource counts set by the job owner.</para>
        /// </param>
        /// <remarks>
        ///   <para>If the cluster administrator specifies a chain of activation filters, a job is evaluated by each filter in the order listed as long as 
        /// it passes each filter with a response value of <c>StartJob</c> or <c>AddResourcesToRunningJob</c>. If a filter in the chain returns a response of <c>FailJob</c>, that value is  
        /// passed to the HPC Job Scheduler and any activation filters that already ran on the job are called again in reverse order to allow the filters 
        /// to revert actions, if necessary. For example, an activation filter that checks for available licenses might include code to release the licenses if the revert function is called.</para> 
        ///   <para>The parameter values that are passed in during the RevertActivation call 
        /// are exactly the same as those that were passed in during the matching FilterActivation call.</para>
        /// </remarks>
        void RevertActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount);
    }
}
