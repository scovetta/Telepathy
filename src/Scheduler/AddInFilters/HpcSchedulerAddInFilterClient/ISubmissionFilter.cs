using System.IO;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcClient
{
    /// <summary>
    ///   <para>This enumeration defines the responses that a custom job submission filter can return to the 
    /// HPC Job Scheduler Service. These responses tell the HPC Job Scheduler Service how to continue processing the job.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Based on the filter responses, the HPC Job Scheduler Service will mark the job as Failed 
    /// or proceed with job validation. If the job passes all validation, the job will be added to the job queue.</para>
    /// </remarks>
    public enum SubmissionFilterResponse
    {
        /// <summary>
        ///   <para>Mark the job as Failed with an error message that the 
        /// job was failed by the submission filter. This enumeration member represents a value of -1.</para>
        /// </summary>
        FailJob = -1,
        /// <summary>
        ///   <para>Continue validating the job. This response value indicates that filter approved 
        /// the job and made no changes. The HPC Job Scheduler will validate the  
        /// job against the job template (or to the next filter), and if it 
        /// passes, add the job to the queue. This enumeration member represents a value of 0.</para> 
        /// </summary>
        SuccessNoJobChange = 0,
        /// <summary>
        ///   <para>Continue validating the modified job. This response value indicates that the filter has 
        /// made modifications to the job properties and approved the job. The HPC Job Scheduler will  
        /// validate the modified job against the job template (or to the next filter), and if 
        /// it passes, add the job to the queue. This enumeration member represents a value of 1.</para> 
        /// </summary>
        SuccessJobChanged = 1
    };

    /// <summary>
    ///   <para>This interface defines the methods for implementing a custom job submission filter. The HPC 
    /// Job Scheduler Service can call submission filters to provide additional checks and controls when jobs are submitted  
    /// to the cluster. This type of filter is defined as a DLL (its methods are called 
    /// directly by the HPC Job Scheduler Service), and is configured by the administrator at the job template level.</para> 
    /// </summary>
    /// <remarks>
    ///   <para>The HPC Job Scheduler Service can run a submission filter when jobs are submitted to the cluster. The submission filter can 
    /// check job properties against information of your choosing. The filter can 
    /// also change job property values before jobs are added to the queue. </para> 
    ///   <para>The filter must implement the <c>FilterSubmission</c> and the <c>RevertSubmission</c> methods. To validate a job that has 
    /// just been submitted, the HPC Job Scheduler Service calls the <c>FilterSubmission</c> method. The return value from this method  
    /// determines if the job will be rejected or added to the queue (after further validation). If the call 
    /// to <c>FilterSubmission</c> returns <c>JobFailed</c>, then the HPC Job Scheduler Service calls <c>RevertSubmission</c> for all previously run filters in the chain.</para> 
    ///   <para>The Microsoft HPC Pack 2008 R2 <see href="http://go.microsoft.com/fwlink/p/?LinkId=223350">SP2 
    /// SDK</see> code samples include an example submission filter (in the Scheduler/Filters/DLL folder).</para>
    ///   <para>The behavior of the HPC Job Scheduler Service is undefined when filters return directly by throwing an exception. 
    /// To avoid this, use try/catch constructions and set a valid return response within the catch area if an exception is thrown.</para>
    ///   <para>All calls to filter methods are time bounded and are aborted if they 
    /// exceed the allowed time. The time allowed for <c>FilterSubmission</c> and <c>RevertSubmission</c> is determined by the  
    /// Submission Filter Timeout setting that the cluster administrator can configure in Job Scheduler options (the 
    /// timeout for cluster-wide filters also applies to the job template filters).The default timeout is 15 seconds.</para> 
    ///   <para>DLLs that contain more than one implementation of this interface are rejected by the HPC Job Scheduler Service.</para>
    ///   <para>Do not use the scheduler APIs to cancel a job from within a filter. This can cause the HPC Job Scheduler Service to stop responding. To 
    /// cancel a job, the filter should return the <c>FailJob</c> filter response. This response value 
    /// causes the HPC Job Scheduler Service to cancel the job and mark it as Failed.</para> 
    ///   <para>DLL filters are only called if they are specified in the job template that is used 
    /// for the job. For more information, see <see href="http://technet.microsoft.com/library/hh405436(WS.10).aspx">How 
    /// to Add or Remove Job Template Level Submission or Activation Filters</see>.</para> 
    /// </remarks>
    public interface ISubmissionFilter
    {
        // Filters the submission of the specified job.
        // Subsequent action is specified by the return value (SubmissionFilterResponse).
        // Calls that exceed SubmissionFilterTimeout are terminated.
        /// <summary>
        ///   <para>The HPC Job Scheduler Service calls this method of the submission filter when a job is first submitted to 
        /// the cluster. This method should implement the business logic that you want the filter to enforce, such as checking job properties  
        /// against information of your choosing. The filter can also change job property values before a job is added to the queue. 
        /// The HPC Job Scheduler Service rejects the job or continues validating the job according to the return value supplied by this method.</para> 
        /// </summary>
        /// <param name="jobXml">
        ///   <para>An XML stream that defines the job properties and tasks. You can parse this stream to get the job 
        /// property values that the filter will check. In Window HPC Server 
        /// 2008 R2, the job XML schema is defined by the <c>http://schemas.microsoft.com/HPCS2008R2/scheduler/</c> namespace.</para> 
        /// </param>
        /// <param name="jobXmlModified">
        ///   <para>If the filter modifies job properties, it creates a new XML stream and returns 
        /// it in this parameter. If the filter does not modify the job, this parameter should be set  
        /// to <c>null</c>. Any changes that are made to the job through the scheduler APIs (rather than through 
        /// the XML) while a job is being validated by the filters are not reflected in the XML stream.</para> 
        /// </param>
        /// <returns>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.AddInFilter.HpcClient.SubmissionFilterResponse" />.</para>
        /// </returns>
        /// <remarks>
        ///   <para>As an input parameter, the filter accepts an XML stream that specifies the properties 
        /// of the job and its tasks. The filter can check the job properties and other data  
        /// sources to determine if the job should be added to the queue. A submission filter can 
        /// also make changes to job property values by modifying the job XML. Task property values cannot be changed.</para> 
        ///   <para>If the filter modifies the job xml, the filter must set the response to <c>SuccessJobChanged</c>.</para>
        ///   <para>Submission filters run as soon as a job is submitted, before 
        /// the job is checked against the job template (submission filters can change job properties,  
        /// including the assigned job template). If the job passes the submission filter, the 
        /// user credentials are verified and then the job template defaults and value constraints are applied.</para> 
        /// </remarks>
        SubmissionFilterResponse FilterSubmission(Stream jobXml, out Stream jobXmlModified);

        /// <summary>
        ///   <para>The HPC Job Scheduler Service calls this method of the submission filter if 
        /// the cluster administrator specified a chain of submission filters for the job, and one of the  
        /// filters later in the chain fails the job. This method should implement any actions that 
        /// should occur if a job that already passed through the filter is rejected by a later filter.</para> 
        /// </summary>
        /// <param name="jobXml">
        ///   <para>An XML stream that defines the job properties and tasks. If the filter did not modify the job, this is the same stream (<c>jobxml</c>) 
        /// that was provided in the FilterSubmission call. If the filter modified the job, 
        /// it is the same stream (<c>jobxmlmodified</c>) that was returned by the <c>FilterSubmission</c> call. </para> 
        /// </param>
        /// <remarks>
        ///   <para>If the cluster administrator specifies a chain of submission filters, a job will run through each filter in 
        /// the order listed as long as it passes each filter with a response value of <c>SuccessNoJobChange</c> or <c>SuccessJobChanged</c>. If a filter  
        /// in the chain returns a response of <c>FailJob</c>, that value is passed to the HPC Job Scheduler, and any submission filters 
        /// that already ran on the job are called again in reverse order to allow the filters to revert actions, if necessary. </para> 
        ///   <para>To read about or share best practices, see the TechNet Wiki article 
        /// <see href="http://social.technet.microsoft.com/wiki/contents/articles/4707.best-practices-for-writing-job-template-level-filters-for-the-hpc-job-scheduler-service.aspx">Best practices for writing job template 
        /// level filters for the HPC Job Scheduler Service</see>.</para> 
        /// </remarks>
        void RevertSubmission(Stream jobXml);
    }
}
