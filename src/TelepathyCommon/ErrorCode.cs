namespace TelepathyCommon
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    // TODO: this class is subject to change
    /// <summary>
    ///     <para>Defines the possible HPC-defined error codes that HPC can set.</para>
    /// </summary>
    /// <remarks>
    ///     <para />
    /// </remarks>
    public static class ErrorCode
    {
        /// <summary>
        ///     <para>
        ///         Separator used to separate the insertion strings for the error message text. See
        ///         <see cref="Microsoft.Hpc.Scheduler.Properties.SchedulerException.Params" />.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const string ArgumentSeparator = "|||";

        /// <summary>
        ///     <para>
        ///         Indicates that credential reuse is enabled and softcard-based
        ///         authentication is allowed, and the saved certificate is about to expire.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         This field provides additional information about the circumstances under which an
        ///         <see cref="Operation_AuthenticationFailure" /> error occurred. This field corresponds to a value of 3.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Operation_AuthenticationFailure" />
        public const int AuthFailureAllowSoftCardAboutToExpireSaved = 3; // used for the case where softcard is allowed, credential reuse is enabled and the saved certificate is about to expire

        /// <summary>
        ///     <para>Indicates that credential reuse is disabled and softcard based authentication is allowed.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         This field provides additional information about the circumstances under which an
        ///         <see cref="Operation_AuthenticationFailure" /> error occurred. This field corresponds to a value of 4.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Operation_AuthenticationFailure" />
        public const int AuthFailureAllowSoftCardDisableCredentialReuse = 4; // used for the case where credential reuse is disabled and softcard is allowed

        /// <summary>
        ///     <para>
        ///         Indicates that credential reuse is enabled and softcard-based authentication is allowed, but no credentials
        ///         are saved.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         This field provides additional information about the circumstances under which an
        ///         <see cref="Operation_AuthenticationFailure" /> error occurred. This field corresponds to a value of 2.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Operation_AuthenticationFailure" />
        public const int AuthFailureAllowSoftCardNoValidSaved = 2; // used for the case where softcard is allowed, credential reuse is enabled, but no creds are saved

        // Constants used by authentication failure to convery extra information through the string parameter of CallResult         
        /// <summary>
        ///     <para>Indicates that credential reuse is turned off and softcard-based authentication is turned off.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         This field provides additional information about the circumstances under which an
        ///         <see cref="Operation_AuthenticationFailure" /> error occurred. This field corresponds to a value of 1.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Operation_AuthenticationFailure" />
        public const int AuthFailureDisableCredentialReuse = 1; // used for the case where credential reuse is disabled and softcard is disabled

        /// <summary>
        ///     <para>
        ///         Indicates that softcard-based authentication is required, credential
        ///         reuse is enabled, and the saved certificate is about to expire.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         This field provides additional information about the circumstances under which an
        ///         <see cref="Operation_AuthenticationFailure" /> error occurred. This field corresponds to a value of 6.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Operation_AuthenticationFailure" />
        public const int AuthFailureRequireSoftCardAboutToExpireSaved = 6; // used for the case where softcard is required, credential reuse is enabled and the saved certificate is about to expire

        /// <summary>
        ///     <para>Indicates that softcard-based authentication is required and credential reuse is disabled.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         This field provides additional information about the circumstances under which an
        ///         <see cref="Operation_AuthenticationFailure" /> error occurred. This field corresponds to a value of 7.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Operation_AuthenticationFailure" />
        public const int AuthFailureRequireSoftCardDisableCredentialReuse = 7; // used for the case where credential reuse is disabled and softcard is required

        /// <summary>
        ///     <para>
        ///         Indicates that softcard-based authentication is required and credential reuse is enabled, but no credentials
        ///         are saved.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         This field provides additional information about the circumstances under which an
        ///         <see cref="Operation_AuthenticationFailure" /> error occurred. This field corresponds to a value of 5.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Operation_AuthenticationFailure" />
        public const int AuthFailureRequireSoftCardNoValidSaved = 5; // used for the case where softcard is required, credential reuse is enabled, but no creds are saved

        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int CustomErrorEnd = CustomErrorStart + 1000;

        // custom errors
        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int CustomErrorStart = ServiceBroker_ExitCode_End + 1;

        // Win32 standard exit code for ERROR_EXCEPTION_IN_SERVICE
        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Error_Exception_In_Service = 1064;

        /// <summary>
        ///     <para>The child job has finished running.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409D6.</para>
        /// </remarks>
        public const int Execution_ChildJobFinished = ExecutionErrorStart + 6;

        /// <summary>
        ///     <para>An activation filter marked the specified job as Failed.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409EA.</para>
        /// </remarks>
        public const int Execution_FailedByActivationFilter = ExecutionErrorStart + 26;

        /// <summary>
        ///     <para>
        ///         Unable to open or create the file to which the job should
        ///         redirect standard error output on the specified node. Verify that the file exists and that
        ///         the account under which the job runs has access to the file, or that
        ///         the account under which the job runs has permission to create the file, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409E3.</para>
        /// </remarks>
        public const int Execution_FailedToOpenStandardError = ExecutionErrorStart + 19;

        /// <summary>
        ///     <para>
        ///         Unable to open the file from which the job redirects standard input on the specified node. Verify
        ///         that the file exists and that the account under which the job runs has access to the file, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409E1.</para>
        /// </remarks>
        public const int Execution_FailedToOpenStandardInput = ExecutionErrorStart + 17;

        /// <summary>
        ///     <para>
        ///         Unable to open or create the file to which the job should
        ///         redirect standard output on the specified node. Verify that the file exists and that
        ///         the account under which the job runs has access to the file, or that
        ///         the account under which the job runs has permission to create the file, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409E2.</para>
        /// </remarks>
        public const int Execution_FailedToOpenStandardOutput = ExecutionErrorStart + 18;

        /// <summary>
        ///     <para>The user canceled the job while it was running.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409DA.</para>
        /// </remarks>
        public const int Execution_JobCanceled = ExecutionErrorStart + 10;

        /// <summary>
        ///     <para>
        ///         The job was preempted by a higher priority job. For details, see the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CanPreempt" /> and the PreemptionType cluster parameter in the
        ///         Remarks section of
        ///         <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" />.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409DE.</para>
        /// </remarks>
        public const int Execution_JobPreempted = ExecutionErrorStart + 14;

        /// <summary>
        ///     <para>
        ///         The job exceeded its run-time limit. See
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" /> and
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Runtime" />.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409D5.</para>
        /// </remarks>
        public const int Execution_JobRuntimeExpired = ExecutionErrorStart + 5;

        /// <summary>
        ///     <para>An error occurred on the node.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409D2.</para>
        /// </remarks>
        public const int Execution_NodeError = ExecutionErrorStart + 2;

        /// <summary>
        ///     <para>
        ///         The node preparation task failed on the specified node. For information about
        ///         the error and how to resolve it, see the error for the node preparation task.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409E8.</para>
        /// </remarks>
        public const int Execution_NodePrepTaskFailure = ExecutionErrorStart + 24;

        /// <summary>
        ///     <para>The job or task was scheduled to run on a node that is no longer reachable.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409D8.</para>
        /// </remarks>
        public const int Execution_NodeUnreachable = ExecutionErrorStart + 8;

        /// <summary>
        ///     <para>The node is unreachable because no IPV4 address could be found for it.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Execution_NoIPv4AddressForNode = ExecutionErrorStart + 27;

        /// <summary>
        ///     <para>The parent job of this child job has been canceled.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409D4.</para>
        /// </remarks>
        public const int Execution_ParentJobCanceled = ExecutionErrorStart + 4;

        /// <summary>
        ///     <para>The process has exited.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409D9.</para>
        /// </remarks>
        public const int Execution_ProcessDead = ExecutionErrorStart + 9;

        /// <summary>
        ///     <para>The job did not start on one or more nodes or the nodes were not reachable.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409DB.</para>
        /// </remarks>
        public const int Execution_ResourceFailure = ExecutionErrorStart + 11;

        /// <summary>
        ///     <para>The job did not start on node {0}.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409DF.</para>
        /// </remarks>
        public const int Execution_StartJobFailedOnNode = ExecutionErrorStart + 15;

        /// <summary>
        ///     <para>This subtask was canceled because the subtask did not have to run to complete the job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409E5.</para>
        /// </remarks>
        public const int Execution_TaskCanceledBeforeAssignment = ExecutionErrorStart + 21;

        /// <summary>
        ///     <para>
        ///         The task was canceled by the user while it was running. For more information, see the specified cancellation
        ///         message.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409E7.</para>
        /// </remarks>
        public const int Execution_TaskCanceledDuringExecution = ExecutionErrorStart + 23;

        /// <summary>
        ///     <para>
        ///         The subtask of a service task was canceled because the HPC Job Scheduler Service could not requeue the
        ///         subtask together with the rest of the job. The HPC Job Scheduler Service will create another subtask to replace
        ///         the canceled subtask.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>This error applies only to subtasks of service tasks.</para>
        ///     <para>For COM developers, the HRESULT value is 0x800409E6.</para>
        /// </remarks>
        public const int Execution_TaskCanceledOnJobRequeue = ExecutionErrorStart + 22;

        /// <summary>
        ///     <para>The scheduler was not able to execute the command specified in the task.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409DD.</para>
        /// </remarks>
        public const int Execution_TaskExecutionFailure = ExecutionErrorStart + 13;

        /// <summary>
        ///     <para>The task failed.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409DC.</para>
        /// </remarks>
        public const int Execution_TaskFailure = ExecutionErrorStart + 12;

        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Execution_TaskFinishedDuringExecution = ExecutionErrorStart + 28;

        /// <summary>
        ///     <para>
        ///         The task is running on a node that the job that contains the task can no longer use. The nodes that a task can
        ///         use
        ///         can change when the nodes that belong to node groups in the HPC
        ///         cluster change, or when nodes are added to the node exclusion list for the job.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409E9.</para>
        /// </remarks>
        public const int Execution_TaskNodeNotUsable = ExecutionErrorStart + 25;

        /// <summary>
        ///     <para>The task exceeded its run-time limit. See <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Runtime" />.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409D7.</para>
        /// </remarks>
        public const int Execution_TaskRuntimeExpired = ExecutionErrorStart + 7;

        /// <summary>
        ///     <para>The job to which the task belongs was canceled.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409D3.</para>
        /// </remarks>
        public const int Execution_TasksJobCanceled = ExecutionErrorStart + 3;

        /// <summary>
        ///     <para>
        ///         The task was canceled while it was running because a job with higher priority preempted the
        ///         task.  If the task can be rerun, the HPC Job Scheduler service will attempt to automatically queue it again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409E0.</para>
        /// </remarks>
        public const int Execution_TasksPreempted = ExecutionErrorStart + 16;

        /// <summary>
        ///     <para>An unknown error occurred.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Execution_UnknownError = ExecutionErrorStart + 1;

        /// <summary>
        ///     <para>
        ///         Unable to access the working directory for the job on the specified node. Check that the directory specified as
        ///         the
        ///         working directory for the job exists on the node, and that the account under which the job runs has access to
        ///         the directory.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800409E4.</para>
        /// </remarks>
        public const int Execution_WorkingDirectoryNotFound = ExecutionErrorStart + 20;

        /// <summary>
        ///     <para>The value is outside the allowable range of values.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040205.</para>
        /// </remarks>
        public const int Operation_ArgumentOutOfRange = OperationErrorStart + 5;

        /// <summary>
        ///     <para>Authenticate failure.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004020C.</para>
        /// </remarks>
        public const int Operation_AuthenticationFailure = OperationErrorStart + 12;

        /// <summary>
        ///     <para>
        ///         A deployment of Windows Azure with the specified identifier was not found. Check the identifier of the
        ///         deployment and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004032D.</para>
        /// </remarks>
        public const int Operation_AzureDeploymentNotFound = OperationErrorStart + 301;

        /// <summary>
        ///     <para>The user canceled the job or task.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040201.</para>
        /// </remarks>
        public const int Operation_CanceledByUser = OperationErrorStart + 1;

        /// <summary>
        ///     <para>
        ///         The cancellation message is longer than 128 characters, which is the maximum
        ///         length allowed. Change the cancellation message so that it contains no more than 128 characters.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402D3.</para>
        /// </remarks>
        public const int Operation_CancelMessageIsTooLong = OperationErrorStart + 211;

        /// <summary>
        ///     <para>
        ///         The application could not connect to the HPC Job Scheduler Service because the
        ///         user may not be authorized to connect to the HPC Job Scheduler Service or the HPC
        ///         Job Scheduler Service might not be running. Check that the user is authorized to connect to
        ///         the HPC Job Scheduler Service and that the HPC Job Scheduler Service is started, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402CF.</para>
        /// </remarks>
        public const int Operation_CannotConnectWithScheduler = OperationErrorStart + 207;

        /// <summary>
        ///     <para>You cannot remove item {0} from the default job template.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040226.</para>
        /// </remarks>
        public const int Operation_CannotRemoveProfileItemFromDefaultTemplate = OperationErrorStart + 38;

        /// <summary>
        ///     <para>You cannot change the name of the default job template.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004022C.</para>
        /// </remarks>
        public const int Operation_CannotResetDefaultProfileName = OperationErrorStart + 44;

        /// <summary>
        ///     <para>The certificate will expire before the soft card expiration warning period.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>This error can be resolved by uploading a new certificate.</para>
        ///     <para>For COM developers, the HRESULT value is 0x80040361.</para>
        /// </remarks>
        public const int Operation_CertificateAboutToExpire = OperationErrorStart + 353;

        /// <summary>
        ///     <para>Failed to enroll in a certificate.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004035F.</para>
        /// </remarks>
        public const int Operation_CertificateEnrollFailure = OperationErrorStart + 356;

        /// <summary>
        ///     <para>Failed to enroll in a certificate with a specific cause.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800404C5.</para>
        /// </remarks>
        public const int Operation_CertificateEnrollFailureWithCause = OperationErrorStart + 359;

        /// <summary>
        ///     <para>Failed to enroll in a certificate because the certificate request could not be created.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800404C7.</para>
        /// </remarks>
        public const int Operation_CertificateFailRequest = OperationErrorStart + 361;

        /// <summary>
        ///     <para>Failed to enroll in a certificate because its private keys could not be exported.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>Users should verify that the certificate template allows private keys to be exported.</para>
        ///     <para>For COM developers, the HRESULT value is 0x800404C6.</para>
        /// </remarks>
        public const int Operation_CertificateNoPrivateKeysExport = OperationErrorStart + 360;

        /// <summary>
        ///     <para>
        ///         This certificate cannot be used for user log on. A certificate with
        ///         a different template for client authentication needs to be used or contact your cluster administrator.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004036B.</para>
        /// </remarks>
        public const int Operation_CertificateNotFitForLogon = OperationErrorStart + 363;

        /// <summary>
        ///     <para>Indicates that the softcard certificate is not valid until a specific date in the future.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040360.</para>
        /// </remarks>
        public const int Operation_CertificateNotYetValid = OperationErrorStart + 352;

        /// <summary>
        ///     <para>You cannot submit child job, instead you must submit the parent job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004021A.</para>
        /// </remarks>
        public const int Operation_ChildJobCannotBeSubmittedAlone = OperationErrorStart + 26;

        /// <summary>
        ///     <para>Not able to register with the server.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004023C.</para>
        /// </remarks>
        public const int Operation_CouldNotRegisterWithServer = OperationErrorStart + 60;

        /// <summary>
        ///     <para>Cryptography error.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004020D.</para>
        /// </remarks>
        public const int Operation_CryptographyError = OperationErrorStart + 13;

        // length of custom property name and value
        /// <summary>
        ///     <para>The custom property name is too long.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_CustomPropertyNameTooLong = OperationErrorStart + 220;

        /// <summary>
        ///     <para>The custom property value is too long.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_CustomPropertyValueTooLong = OperationErrorStart + 221;

        /// <summary>
        ///     <para>A database exception occurred while processing the request.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040204.</para>
        /// </remarks>
        public const int Operation_DatabaseException = OperationErrorStart + 4;

        /// <summary>
        ///     <para>The scheduler was unable to create a new job because the database is full.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040242.</para>
        /// </remarks>
        public const int Operation_DatabaseIsFull = OperationErrorStart + 66;

        /// <summary>
        ///     <para>Duplicate application found in the software license item.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004022E.</para>
        /// </remarks>
        public const int Operation_DuplicateLicenseFeature = OperationErrorStart + 46;

        /// <summary>
        ///     <para>
        ///         The account that was specified for sending email notifications does not have administrator rights on the head
        ///         node of the HPC
        ///         cluster. Add the specified account to the Administrators group of the head
        ///         node of the HPC cluster, or specify another account that has administrator rights.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040268.</para>
        /// </remarks>
        public const int Operation_EmailCredentialMustBeAdmin = OperationErrorStart + 104;

        /// <summary>
        ///     <para>The environment variable name was too long. The size must be smaller than {0} characters.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004023F.</para>
        /// </remarks>
        public const int Operation_EnvironmentVarNameTooLong = OperationErrorStart + 63;

        /// <summary>
        ///     <para>The environment variable value was too long. The size must be smaller than {0} characters.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040240.</para>
        /// </remarks>
        public const int Operation_EnvironmentVarValueTooLong = OperationErrorStart + 64;

        /// <summary>
        ///     <para>
        ///         The specified node could not be added to the list of nodes on which the job should not run because the node
        ///         is not present in the HPC cluster. Check the name of the node and that the node is part of the HPC cluster,
        ///         then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004026B.</para>
        /// </remarks>
        public const int Operation_ExcludedNodeDoesNotExist = OperationErrorStart + 107;

        /// <summary>
        ///     <para>
        ///         The nodes were not added to the list of nodes on which the
        ///         job should not run because adding these nodes to the list would cause the length of
        ///         the list to exceed the specified cluster-wide limit. Check the current list of nodes on which
        ///         the job should not run, remove any nodes that the list should not include, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402D2.</para>
        /// </remarks>
        public const int Operation_ExcludedNodeListTooLong = OperationErrorStart + 210;

        /// <summary>
        ///     <para>
        ///         The specified node could not be added to the list of nodes on
        ///         which the job should not run because the node already is required by one of the
        ///         tasks in the job. Remove the node from the list of required nodes for the
        ///         tasks in the job or from the list of nodes on which the job should not run.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040269.</para>
        /// </remarks>
        public const int Operation_ExcludedRequiredNode = OperationErrorStart + 105;

        /// <summary>
        ///     <para>
        ///         The specified nodes could not be added to the list of nodes on which the job should
        ///         not run because adding the specified nodes to that list would cause the set of nodes on which the job
        ///         can run to have fewer resources than the minimum resource requirements for the job. Add fewer nodes to the list
        ///         of nodes on which the job should not run, or reduce the minimum resource requirements for the job, then try
        ///         again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004026A.</para>
        /// </remarks>
        public const int Operation_ExcludedTooManyNodes = OperationErrorStart + 106;

        /// <summary>
        ///     <para>The value of the expanded priority has to be an integer.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402CA.</para>
        /// </remarks>
        public const int Operation_ExpandedPriorityMustBeInteger = OperationErrorStart + 216;

        /// <summary>
        ///     <para>
        ///         The priority cannot be set to the specified value on the version of Windows HPC Server that you are
        ///         using. For closest available priority values, see the error message and change the priority to match one of
        ///         those values.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402C8.</para>
        /// </remarks>
        public const int Operation_ExpandedPriorityNotValidOnServer = OperationErrorStart + 200;

        /// <summary>
        ///     <para>Indicates that the value of the expanded priority is out of the valid range. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>The valid range for expanded priority is 0-4000.</para>
        ///     <para>For COM developers, the HRESULT value is 0x800402CB.</para>
        /// </remarks>
        public const int Operation_ExpandedPriorityOutOfRange = OperationErrorStart + 217;

        // Fail dependent tasks when any of their parent task fails or cancelled
        /// <summary>
        ///     <para>The dependent tasks failed due to failure of the parent task or tasks.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_FailDependentTasks = OperationErrorStart + 428;

        /// <summary>
        ///     <para>
        ///         The client called a function that is not supported on the
        ///         server, which is running the version of Windows HPC Server that the error message specifies. The
        ///         error message also specifies the version of Windows HPC Server that the client is using. Upgrade
        ///         the client so that it is using the same version of Windows HPC Server as the server.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040260.</para>
        /// </remarks>
        public const int Operation_FeatureDeprecated = OperationErrorStart + 96;

        /// <summary>
        ///     <para>
        ///         The client called a function that is not implemented on the
        ///         server, which is running the version of Windows HPC Server that the error message specifies. The
        ///         error message also specifies the version of Windows HPC Server that the client is using. Upgrade
        ///         the server so that it is using the same version of Windows HPC Server as the client.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040261.</para>
        /// </remarks>
        public const int Operation_FeatureUnimplemented = OperationErrorStart + 97;

        /// <summary>
        ///     <para>Indicates that an activation filter is not located in a specific location. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800403A6.</para>
        /// </remarks>
        public const int Operation_FilterNotActivation = OperationErrorStart + 422;

        /// <summary>
        ///     <para>Indicates that the specified filter could not be found. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800403A5.</para>
        /// </remarks>
        public const int Operation_FilterNotFound = OperationErrorStart + 421;

        /// <summary>
        ///     <para>Indicates that the submission filter is not located in a specific location. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800403A7.</para>
        /// </remarks>
        public const int Operation_FilterNotSubmission = OperationErrorStart + 423;

        // Filter
        /// <summary>
        ///     <para>The filter system experienced the specified error. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800403A4.</para>
        /// </remarks>
        public const int Operation_FilterSystemError = OperationErrorStart + 420;

        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_FinishedByUser = OperationErrorStart + 368;

        /// <summary>
        ///     <para>Owner name is invalid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_HpcCredOwnerNameInvalid = OperationErrorStart + 454;

        /// <summary>
        ///     <para>
        ///         The HPC Job Scheduler Service was unable to obtain a security identifier (SID) for the HPCUsers or
        ///         HPCAdminMirror security group.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402D0.</para>
        /// </remarks>
        public const int Operation_HPCSidUnavailable = OperationErrorStart + 208;

        /// <summary>
        ///     <para>The job template property {0} is not an allowed job template property.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040217.</para>
        /// </remarks>
        public const int Operation_IllegalProfileProperty = OperationErrorStart + 23;

        /// <summary>
        ///     <para>
        ///         The attempt to end a service task was unsuccessful because the attempt can only be made
        ///         for a master service task, and not the subtasks of that task. Try ending the master service task instead.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402D5.</para>
        /// </remarks>
        public const int Operation_IllegalServiceConcludeAttempt = OperationErrorStart + 213;

        /// <summary>
        ///     <para>
        ///         An attempt to end a service task in the specified state was made. Attempts to end service tasks can only be
        ///         made
        ///         if the tasks are in the Queued or Running state. Wait until the service task is in the Queued or Running state,
        ///         and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402D6.</para>
        /// </remarks>
        public const int Operation_IllegalStateForServiceConclude = OperationErrorStart + 214;

        /// <summary>
        ///     <para>
        ///         An attempt to add a new task to a job that contains a service task and is not in the Configuring state was
        ///         made, but a new
        ///         task can only be added to a job with a service task if the job
        ///         is in the Configuring state. Change the state of the job to Configuring and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040253.</para>
        /// </remarks>
        public const int Operation_IllegalTaskAddedToServiceJob = OperationErrorStart + 83;

        /// <summary>
        ///     <para>You cannot cancel the job in its current state.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040208.</para>
        /// </remarks>
        public const int Operation_InvalidCancelJobState = OperationErrorStart + 8;

        /// <summary>
        ///     <para>You cannot cancel the task in its current state.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040209.</para>
        /// </remarks>
        public const int Operation_InvalidCancelTaskState = OperationErrorStart + 9;

        /// <summary>
        ///     <para>
        ///         The specified softcard certificate does not match the softcard certificate template specified by the cluster
        ///         administrator.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004035F.</para>
        /// </remarks>
        public const int Operation_InvalidCertificateTemplate = OperationErrorStart + 351;

        // Invalid Parameter Value: CustomProperties
        /// <summary>
        ///     <para>
        ///         The value for CustomProperties should consist of name/value pairs separated by a semicolon, following a
        ///         regular expression.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_InvalidCustomProperties = OperationErrorStart + 426;

        // email
        /// <summary>
        ///     <para>The specified email address is not valid. Check the email address and try again.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004032E.</para>
        /// </remarks>
        public const int Operation_InvalidEmailAddress = OperationErrorStart + 302;

        /// <summary>
        ///     <para>The provided environment variable name is not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004023D.</para>
        /// </remarks>
        public const int Operation_InvalidEnvironmentVarName = OperationErrorStart + 61;

        /// <summary>
        ///     <para>The provided environment variable value is not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004023E.</para>
        /// </remarks>
        public const int Operation_InvalidEnvironmentVarValue = OperationErrorStart + 62;

        // Invalid Parameter Value: JobValidExitCodes, TaskValidExitCodes
        /// <summary>
        ///     <para>
        ///         The value for Valid Exit Codes, does not follow the correct format. The
        ///         value is a string containing integers or integer ranges separated by commas. For example
        ///         "-1..10,min..-100,0,100..max". "min"
        ///         and "max" can be used to represent minimum and maximum Int32 integers, but "min" or
        ///         "max" alone is not supported. ".." is not a valid range, it must be "min..max" or "max..min".
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_InvalidExitCodes = OperationErrorStart + 424;

        /// <summary>
        ///     <para>You cannot filter on property '{0}' for this kind of object.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040243.</para>
        /// </remarks>
        public const int Operation_InvalidFilterProperty = OperationErrorStart + 67;

        /// <summary>
        ///     <para>The specified job ended up in the wrong specified state after the job was cancelled.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040273.</para>
        /// </remarks>
        public const int Operation_InvalidJobCancelEndState = OperationErrorStart + 115;

        /// <summary>
        ///     <para>The specified job ended up in the wrong specified state after the job completed.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040274.</para>
        /// </remarks>
        public const int Operation_InvalidJobFinishEndState = OperationErrorStart + 116;

        /// <summary>
        ///     <para>The job identifier does not identify an existing job in the scheduler.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040206.</para>
        /// </remarks>
        public const int Operation_InvalidJobId = OperationErrorStart + 6;

        /// <summary>
        ///     <para>
        ///         The list of nodes on which the job should not run cannot be changed as long as the job is
        ///         in the specified state. Wait until the job is in the
        ///         Configuring, Queued, Running, Canceling, Finishing, Canceled, or Failed state, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004026C.</para>
        /// </remarks>
        public const int Operation_InvalidJobStateForNodeExclusion = OperationErrorStart + 108;

        /// <summary>
        ///     <para>Invalid specification for '{0}'.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040241.</para>
        /// </remarks>
        public const int Operation_InvalidJobTemplateItemXml = OperationErrorStart + 65;

        /// <summary>
        ///     <para>The node identifier does not identify and existing node in the cluster.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004020E.</para>
        /// </remarks>
        public const int Operation_InvalidNodeId = OperationErrorStart + 14;

        /// <summary>
        ///     <para>The requested operation is not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004020F.</para>
        /// </remarks>
        public const int Operation_InvalidOperation = OperationErrorStart + 15;

        // Invalid Parameter Value: ParentJobIds
        /// <summary>
        ///     <para>
        ///         The ParentJobIds value is invalid. The value for ParentJobIds should be a set
        ///         of one or more positive integers with a minimum value of one and separated by commas.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_InvalidParentJobIds = OperationErrorStart + 425;

        /// <summary>
        ///     <para>The job template identifier is not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004023B.</para>
        /// </remarks>
        public const int Operation_InvalidProfileId = OperationErrorStart + 59;

        /// <summary>
        ///     <para>
        ///         The specified property cannot be set for the specified type of task. Check
        ///         the property that you want to set or change the type of the task and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040254.</para>
        /// </remarks>
        public const int Operation_InvalidPropForTaskType = OperationErrorStart + 84;

        /// <summary>
        ///     <para>The row enumeration identifier is not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040239.</para>
        /// </remarks>
        public const int Operation_InvalidRowEnumId = OperationErrorStart + 57;

        /// <summary>
        ///     <para>The task identifier does not identify and existing task in the scheduler.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040207.</para>
        /// </remarks>
        public const int Operation_InvalidTaskId = OperationErrorStart + 7;

        /// <summary>
        ///     <para>The job already exists on the server.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040233.</para>
        /// </remarks>
        public const int Operation_JobAlreadyCreatedOnServer = OperationErrorStart + 51;

        /// <summary>
        ///     <para>
        ///         The job cannot be deleted because it was already submitted to the HPC Job
        ///         Scheduler Service. Cancel the job if it has not run and you no longer want it to run.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040250.</para>
        /// </remarks>
        public const int Operation_JobDeletionForbidden = OperationErrorStart + 80;

        /// <summary>
        ///     <para>
        ///         The job cannot be put on hold in the specified state. Wait
        ///         until the job is in the Queued state, and then try to set the HoldUntil property.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402CA.</para>
        /// </remarks>
        public const int Operation_JobHoldInvalidState = OperationErrorStart + 202;

        /// <summary>
        ///     <para>
        ///         The HoldUntil property for a job cannot be set for more than one year in the future. Set the value of the
        ///         HoldUntil property to
        ///         a date and time that is no more than one year in the future, and reset it later if you must hold the job for
        ///         more than one year.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402D1.</para>
        /// </remarks>
        public const int Operation_JobHoldUntilTooLong = OperationErrorStart + 209;

        /// <summary>
        ///     <para>
        ///         The HoldUntil property for a job cannot be set for more than one year in the future. Set the value of the
        ///         HoldUntil property to
        ///         a date and time that is no more than one year in the future, and reset it later if you must hold the job for
        ///         more than one year.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_JobHoldUntilTooLong2 = OperationErrorStart + 219;

        /// <summary>
        ///     <para>
        ///         The HoldUntil property for a job cannot be set to a date and time that is in the past. Specify a date and time
        ///         for the HoldUntil property that is in the future, or leave the HoldUntil
        ///         property unset if you no longer want to put running the job on hold.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402CB.</para>
        /// </remarks>
        public const int Operation_JobInvalidHoldUntil = OperationErrorStart + 203;

        /// <summary>
        ///     <para>The job modification failed.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040220.</para>
        /// </remarks>
        public const int Operation_JobModifyFailed = OperationErrorStart + 32;

        /// <summary>
        ///     <para>The job was not found on the server.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040232.</para>
        /// </remarks>
        public const int Operation_JobNotCreatedOnServer = OperationErrorStart + 50;

        // job progress
        /// <summary>
        ///     <para>
        ///         The value specified for the progress of the job was not an integer from 0
        ///         to 100. Specify a value for the progress of the job that is an integer from 0 to 100.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004024B.</para>
        /// </remarks>
        public const int Operation_JobProgressOutOfRange = OperationErrorStart + 75;

        /// <summary>
        ///     <para>Unable to revert the job changes.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040221.</para>
        /// </remarks>
        public const int Operation_JobRevertModifyFailed = OperationErrorStart + 33;

        /// <summary>
        ///     <para>
        ///         The value for the specified property of the job does not contain the specified value {1}, which the
        ///         job template for the job required. Update the value of the property to include the value that the job template
        ///         requires.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040272.</para>
        /// </remarks>
        public const int Operation_JobTemplateRequiredValueMissing = OperationErrorStart + 114;

        /// <summary>
        ///     <para>
        ///         The value for the specified property of the job does not match one of the values that the job
        ///         template allows for the property. Change the value of the property to match one of the values that the job
        ///         template allows.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004026F.</para>
        /// </remarks>
        public const int Operation_JobTemplateValueInvalid = OperationErrorStart + 111;

        /// <summary>
        ///     <para>
        ///         The value for the specified property of the job is larger than the maximum value that the job template allows
        ///         for the property. For the
        ///         maximum value that the job template allows for the property, see the error
        ///         message, then change the value of the property to no larger than that maximum value.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040271.</para>
        /// </remarks>
        public const int Operation_JobTemplateValueTooLarge = OperationErrorStart + 113;

        /// <summary>
        ///     <para>
        ///         The value for the specified property of the job is less than the minimum value that the job template allows for
        ///         the property. For the
        ///         minimum value that the job template allows for the property, see the error
        ///         message, then change the value of the property to no less than that minimum value.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040270.</para>
        /// </remarks>
        public const int Operation_JobTemplateValueTooSmall = OperationErrorStart + 112;

        /// <summary>
        ///     <para>
        ///         A function was called that cannot be applied to the selected job because the job is
        ///         running on the first version of Windows HPC Server that the error message specifies, and the function can only
        ///         be
        ///         applied to jobs running on the second version of Windows HPC Server that the error message specifies. To apply
        ///         the
        ///         function to the job, run the job on an HPC cluster that has the second version of Windows HPC Server installed.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040262.</para>
        /// </remarks>
        public const int Operation_JobVersionMismatch = OperationErrorStart + 98;

        /// <summary>
        ///     <para>You must specify a job template name.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004023A.</para>
        /// </remarks>
        public const int Operation_MustProvideProfileName = OperationErrorStart + 58;

        /// <summary>
        ///     <para>No certificates matching the softcard certificate template and thumbprint were found or selected by the user.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040362.</para>
        /// </remarks>
        public const int Operation_NoCertificateFoundOnClient = OperationErrorStart + 354;

        /// <summary>
        ///     <para>
        ///         The specified node could not be added to the HPC cluster because a node with that name already exists. Choose a
        ///         name
        ///         for the node that does not match the name of a node that the HPC cluster already contains, and try to add the
        ///         node again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004025E.</para>
        /// </remarks>
        public const int Operation_NodeAlreadyExists = OperationErrorStart + 94;

        // Azure

        /// <summary>
        ///     <para>The node is not deployed on Windows Azure.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004032C.</para>
        /// </remarks>
        public const int Operation_NodeIsNotOnAzure = OperationErrorStart + 300;

        /// <summary>
        ///     <para>The user does not have permission to use the job template.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040219.</para>
        /// </remarks>
        public const int Operation_NoPermissionToUseProfile = OperationErrorStart + 25;

        /// <summary>
        ///     <para>No storage is currently defined for the property.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040230.</para>
        /// </remarks>
        public const int Operation_NoTableDefinedForProperty = OperationErrorStart + 48;

        /// <summary>
        ///     <para>You do not have permission to change this job template.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040216.</para>
        /// </remarks>
        public const int Operation_NotAllowedToChangeProfiles = OperationErrorStart + 22;

        /// <summary>
        ///     <para>The user is not allowed to create job templates.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040214.</para>
        /// </remarks>
        public const int Operation_NotAllowedToCreateProfiles = OperationErrorStart + 20;

        /// <summary>
        ///     <para>The default job template cannot be deleted.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040218.</para>
        /// </remarks>
        public const int Operation_NotAllowedToDeleteDefaultProfile = OperationErrorStart + 24;

        /// <summary>
        ///     <para>The user is not allowed to delete job templates.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040215.</para>
        /// </remarks>
        public const int Operation_NotAllowedToDeleteProfiles = OperationErrorStart + 21;

        /// <summary>
        ///     <para>Cannot perform the operation because you are not connected to the server.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040238.</para>
        /// </remarks>
        public const int Operation_NotConnectedToServer = OperationErrorStart + 56;

        /// <summary>
        ///     <para>No softcard certificate template is specified by the cluster administrator or by the user.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040363.</para>
        /// </remarks>
        public const int Operation_NoTemplateName = OperationErrorStart + 355;

        /// <summary>
        ///     <para>The named softcard certificate template was not found.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004036A.</para>
        /// </remarks>
        public const int Operation_NoTemplateWithFriendlyName = OperationErrorStart + 362;

        /// <summary>
        ///     <para>
        ///         The operation could not finish because the HPC Job Scheduler Service
        ///         is already using an object that the operation tried to use. Try again later.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004026E.</para>
        /// </remarks>
        public const int Operation_ObjectInUse = OperationErrorStart + 110;

        /// <summary>
        ///     <para>The job must be in the configuration state in order to submit the job. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040202.</para>
        /// </remarks>
        public const int Operation_OnlyConfigJobsCanBeSubmitted = OperationErrorStart + 2;

        /// <summary>
        ///     <para>
        ///         You can submit the task only if the task is in the
        ///         <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState.Configuring" /> state.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040234.</para>
        /// </remarks>
        public const int Operation_OnlyConfiguringTasksCanBeSubmitted = OperationErrorStart + 52;

        /// <summary>
        ///     <para>You can reconfigure only canceled or failed jobs.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004020A.</para>
        /// </remarks>
        public const int Operation_OnlyFailedCancelledJobsCanBeConfigured = OperationErrorStart + 10;

        /// <summary>
        ///     <para>
        ///         The state of the task could not be changed to Configuring because the
        ///         current state is not a state that can be changed to Configuring. The state of a
        ///         task can be changed to Configuring only if the current state is Queued, Failed, or Canceled.
        ///         Wait for the state of the task to change to Queued, Failed, or Canceled, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004024C.</para>
        /// </remarks>
        public const int Operation_OnlyFailedCancelledTasksCanBeConfigured = OperationErrorStart + 76;

        /// <summary>
        ///     <para>Indicates that the specified parent jobs were deleted from the database.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_ParentJobsDeletedFromDB = OperationErrorStart + 429;

        /// <summary>
        ///     <para>The use of passwords is disabled on this cluster.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040460.</para>
        /// </remarks>
        public const int Operation_PasswordDisabledOnCluster = OperationErrorStart + 357;

        /// <summary>
        ///     <para>The user does not have permission to perform the operation.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004020B.</para>
        /// </remarks>
        public const int Operation_PermissionDenied = OperationErrorStart + 11;

        /// <summary>
        ///     <para>The specified default pool can’t be deleted.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040393.</para>
        /// </remarks>
        public const int Operation_PoolCantDeleteDefaultPool = OperationErrorStart + 403;

        /// <summary>
        ///     <para>
        ///         The specified pool cannot be deleted since it is still being used in the specified
        ///         template. You can force delete the pool and the templates will be reset to use the Default Pool.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040395.</para>
        /// </remarks>
        public const int Operation_PoolCantDeletePoolInTemplates = OperationErrorStart + 405;

        /// <summary>
        ///     <para>The weight of the pool has an integer value that is outside the specified range.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>The valid pool weight range is 0-100.</para>
        ///     <para>For COM developers, the HRESULT value is 0x80040391.</para>
        /// </remarks>
        public const int Operation_PoolInvalidWeight = OperationErrorStart + 401;

        /// <summary>
        ///     <para>The number of defined pools has exceeded the specified limit.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>The limit on the number of pools is 100.</para>
        ///     <para>For COM developers, the HRESULT value is 0x80040396.</para>
        /// </remarks>
        public const int Operation_PoolLimitReached = OperationErrorStart + 406;

        // Pool
        /// <summary>
        ///     <para>A pool with the specified name already exists on the cluster.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040390.</para>
        /// </remarks>
        public const int Operation_PoolNameBeenUsed = OperationErrorStart + 400;

        /// <summary>
        ///     <para>The pool name cannot contain the specified characters.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040398.</para>
        /// </remarks>
        public const int Operation_PoolNameHasInvalidCharacters = OperationErrorStart + 408;

        /// <summary>
        ///     <para>The pool name exceeds the specified number of characters.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>The character limit on pool names is 64.</para>
        ///     <para>For COM developers, the HRESULT value is 0x80040397.</para>
        /// </remarks>
        public const int Operation_PoolNameTooLong = OperationErrorStart + 407;

        /// <summary>
        ///     <para>The specified pool name does not exist on the cluster.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040392.</para>
        /// </remarks>
        public const int Operation_PoolNonExistent = OperationErrorStart + 402;

        /// <summary>
        ///     <para>Pool related operations cannot be performed when pools are disabled on the cluster.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040394.</para>
        /// </remarks>
        public const int Operation_PoolNotEnabled = OperationErrorStart + 404;

        /// <summary>
        ///     <para>You cannot change HPC-defined environment variables.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040236.</para>
        /// </remarks>
        public const int Operation_PreservedEnvironmentVariables = OperationErrorStart + 54;

        /// <summary>
        ///     <para>
        ///         The values of the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Priority" /> and
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" /> properties were set at the same time to
        ///         incompatible values. Set only the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Priority" /> value if you want to run your job on Windows HPC
        ///         Server 2008, and set only the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" /> value if you want to run your job on
        ///         Windows HPC Server 2008 R2 and is not supported in previous versions.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402C9.</para>
        /// </remarks>
        public const int Operation_PriorityAndExPriCannotBeSetTogether = OperationErrorStart + 201;

        /// <summary>
        ///     <para>The value of Priority is not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402CC.</para>
        /// </remarks>
        public const int Operation_PriorityNotValid = OperationErrorStart + 218;

        /// <summary>
        ///     <para>The job template item that you are trying to set already exists.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040211.</para>
        /// </remarks>
        public const int Operation_ProfileItemAlreadyExists = OperationErrorStart + 17;

        /// <summary>
        ///     <para>The default value for the job template item is not in the allowed value range for the item.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040228.</para>
        /// </remarks>
        public const int Operation_ProfileItemDefaultValueInvalid = OperationErrorStart + 40;

        /// <summary>
        ///     <para>
        ///         The default values specified for the job template item {0} must include all of the required values for
        ///         that item.  Ensure that the default values include all the values in the Required Values list, and then try
        ///         again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040227.</para>
        /// </remarks>
        public const int Operation_ProfileItemDefaultValueNotIncludeRequiredValue = OperationErrorStart + 39;

        /// <summary>
        ///     <para>The job template item that you are trying to set does not exist.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040210.</para>
        /// </remarks>
        public const int Operation_ProfileItemDoesNotExist = OperationErrorStart + 16;

        /// <summary>
        ///     <para>The minimum constraint for the job template item is larger than its maximum constraint.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004022A.</para>
        /// </remarks>
        public const int Operation_ProfileItemMinGreaterThanMax = OperationErrorStart + 42;

        /// <summary>
        ///     <para>You must provide a default value for the job template item.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040229.</para>
        /// </remarks>
        public const int Operation_ProfileItemMustProvideDefaultStringValue = OperationErrorStart + 41;

        /// <summary>
        ///     <para>
        ///         Invalid template: The template item {0}'s default value, {1}, is larger than the default
        ///         value of the related template item, {2} (default value: {3}).  Correct this inconsistency and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040224.</para>
        /// </remarks>
        public const int Operation_ProfileItemRangeInconsistent_LeftDefaultGreaterThanRightDefault = OperationErrorStart + 36;

        /// <summary>
        ///     <para>
        ///         Invalid template: The template item {0}'s maximum value, {1}, is larger than the maximum
        ///         value of the related template item, {2} (maximum value: {3}).  Correct this inconsistency and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040223.</para>
        /// </remarks>
        public const int Operation_ProfileItemRangeInconsistent_LeftMaxGreaterThanRightMax = OperationErrorStart + 35;

        /// <summary>
        ///     <para>
        ///         Invalid template: The template item {0}'s minimum value, {1}, is larger than the minimum
        ///         value of the related template item, {2} (minimum value: {3}).  Correct this inconsistency and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040222.</para>
        /// </remarks>
        public const int Operation_ProfileItemRangeInconsistent_LeftMinGreaterThanRightMin = OperationErrorStart + 34;

        /// <summary>
        ///     <para>The value of the job template item cannot be less than zero.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040225.</para>
        /// </remarks>
        public const int Operation_ProfileItemRangeInconsistent_ValueLessThanZero = OperationErrorStart + 37;

        /// <summary>
        ///     <para>The data type that is used to set or access the job template item does not match the data type of the item.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004022B.</para>
        /// </remarks>
        public const int Operation_ProfileItemTypeInconsistent = OperationErrorStart + 43;

        /// <summary>
        ///     <para>A job template with the specified name already exists. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004021C.</para>
        /// </remarks>
        public const int Operation_ProfileNameBeenUsed = OperationErrorStart + 28;

        /// <summary>
        ///     <para>The job template is not present in the scheduler.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004021B.</para>
        /// </remarks>
        public const int Operation_ProfileNotFound = OperationErrorStart + 27;

        /// <summary>
        ///     <para>The property is read-only and cannot be set.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040203.</para>
        /// </remarks>
        public const int Operation_PropertyIsReadOnly = OperationErrorStart + 3;

        /// <summary>
        ///     <para>The property name already exists.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040213.</para>
        /// </remarks>
        public const int Operation_PropertyNameAlreadyExists = OperationErrorStart + 19;

        /// <summary>
        ///     <para>The name of a custom property cannot be empty or NULL. Specify a name for the custom property and try again.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402CE.</para>
        /// </remarks>
        public const int Operation_PropertyNameCannotBeEmpty = OperationErrorStart + 206;

        /// <summary>
        ///     <para>
        ///         The specified property is not supported on the server, which is running
        ///         the first version of Windows HPC Server that is listed in the error message. The minimum server
        ///         version that supports the property is the second version of Windows HPC Server that is listed in
        ///         the error message. Leave the property unset, or upgrade your HPC cluster to the second version.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040255.</para>
        /// </remarks>
        public const int Operation_PropertyNotSupportedOnServerVersion = OperationErrorStart + 85;

        /// <summary>
        ///     <para>The value is too large.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040235.</para>
        /// </remarks>
        public const int Operation_PropertyValueTooLargeForDatabase = OperationErrorStart + 53;

        /// <summary>
        ///     <para>
        ///         The application could not connect with the HPC Job Scheduler Service in the specified number of seconds. Try
        ///         again later.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402D4.</para>
        /// </remarks>
        public const int Operation_ReconnectTimeout = OperationErrorStart + 212;

        /// <summary>
        ///     <para>
        ///         The job could not be submitted or modified because the HPC Job Scheduler Service
        ///         is performing maintenance on the database to create capacity for new jobs. Try again in several minutes.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402D7.</para>
        /// </remarks>
        public const int Operation_SchedulerInCleanupMode = OperationErrorStart + 215;

        /// <summary>
        ///     <para>The job could not be submitted because the HPC Job Scheduler Service is in recovery mode. Try again later.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402CC.</para>
        /// </remarks>
        public const int Operation_SchedulerInRecoverMode = OperationErrorStart + 204;

        /// <summary>
        ///     <para>
        ///         The scheduler database has reached the maximum number of instances of scheduler objects. Please contact your
        ///         cluster administrator.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_SchedulerInstanceLimitReached = OperationErrorStart + 222;

        // Scheduler on Azure
        /// <summary>
        ///     <para>This operation can only be called when the scheduler is running on Windows Azure.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004032F.</para>
        /// </remarks>
        public const int Operation_SchedulerOnAzureModeOnly = OperationErrorStart + 303;

        /// <summary>
        ///     <para>
        ///         A method that only the HPC Job Scheduler Service can call was called. Remove the method call from your
        ///         application.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800402CD.</para>
        /// </remarks>
        public const int Operation_SchedulerPrivilegeRequired = OperationErrorStart + 205;

        // Http transport related errors
        /// <summary>
        ///     <para>
        ///         The server certificate is not trusted. To correct this, the server certificate needs
        ///         either to have a valid chain of trust, or to be placed in the trusted root store.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_ServerCertNotTrusted = OperationErrorStart + 450;

        /// <summary>
        ///     <para>
        ///         The client request could not be handled because the server on which the HPC Job Scheduler Service runs
        ///         is busy. Try again later, or, for more information, check the event log for the HPC Job Scheduler Service on
        ///         head node.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004025F.</para>
        /// </remarks>
        public const int Operation_ServerIsBusy = OperationErrorStart + 95;

        /// <summary>
        ///     <para>
        ///         The user attempted to specify a certificate template when the cluster already has a specified certificate as
        ///         the server template.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040366.</para>
        /// </remarks>
        public const int Operation_ServerTemplatePresent = OperationErrorStart + 358;

        /// <summary>
        ///     <para>The specified user name is invalid in service as client mode.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_ServiceAsClientBadUserName = OperationErrorStart + 452;

        /// <summary>
        ///     <para>The service as client mode is not supported while connecting over Https.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_ServiceAsClientNotSupportedOverHttps = OperationErrorStart + 453;

        // User identity
        /// <summary>
        ///     <para>The client is not trusted to run in service as client mode.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_ServiceAsClientUserNotTrusted = OperationErrorStart + 451;

        /// <summary>
        ///     <para>You cannot change the property when the job is in the current state.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040231.</para>
        /// </remarks>
        public const int Operation_SetInvalidPropUnderCertainState = OperationErrorStart + 49;

        /// <summary>
        ///     <para>The certificate has expired or is about to expire.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004036D.</para>
        /// </remarks>
        public const int Operation_SoftCardAboutToExpireShort = OperationErrorStart + 365;

        // Soft card
        /// <summary>
        ///     <para>The use of softcards is disabled on this cluster.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004035E.</para>
        /// </remarks>
        public const int Operation_SoftCardDisabledOnCluster = OperationErrorStart + 350;

        /// <summary>
        ///     <para>The use of softcards is disabled on this cluster.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004036E.</para>
        /// </remarks>
        public const int Operation_SoftCardNotSupported = OperationErrorStart + 366;

        /// <summary>
        ///     <para>
        ///         The softcard related operation failed. Verify that the HPC key service provider (KSP) been installed on the
        ///         headnode.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004036F.</para>
        /// </remarks>
        public const int Operation_SoftCardNTEError = OperationErrorStart + 367;

        /// <summary>
        ///     <para>
        ///         The user of the job doesn’t match the job’s owner. When softcards are required, the user of a job has to be
        ///         the same as the owner.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004036C.</para>
        /// </remarks>
        public const int Operation_SoftCardRequiredNoUsername = OperationErrorStart + 364;

        /// <summary>
        ///     <para>
        ///         The task could not be deleted because it was submitted already or because the task is
        ///         a subtask for a parametric task. Cancel the task or subtask if you do not want it to run.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040251.</para>
        /// </remarks>
        public const int Operation_TaskDeletionForbidden = OperationErrorStart + 81;

        // Task modification
        /// <summary>
        ///     <para>Failed to modify the task.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040246.</para>
        /// </remarks>
        public const int Operation_TaskModifyFailed = OperationErrorStart + 70;

        /// <summary>
        ///     <para>The task name must not include a comma. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_TaskNameContainInvalidChars = OperationErrorStart + 427;

        // V3 bug 2658. Try to refresh/commit a task when the task is not on the server yet
        /// <summary>
        ///     <para>The task was not added yet to its parent job, or the parent job was not added yet to the server.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004024E.</para>
        /// </remarks>
        public const int Operation_TaskNotCreatedOnServer = OperationErrorStart + 78;

        /// <summary>
        ///     <para>
        ///         The task cannot be reconfigured or requeued after it was submitted because the task has the specified task
        ///         type.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004024D.</para>
        /// </remarks>
        public const int Operation_TaskOfTypeCannotBeReconfigured = OperationErrorStart + 77;

        /// <summary>
        ///     <para>
        ///         A task of the specified type was added to the job that is in a state other than Configuring, but tasks of that
        ///         type can only be
        ///         added while the job is in the Configuring state. Cancel the job and then modify
        ///         it to place the job in the Configuring state, then try to add the task again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040252.</para>
        /// </remarks>
        public const int Operation_TaskTypeAddedInWrongJobState = OperationErrorStart + 82;

        /// <summary>
        ///     <para>
        ///         The
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Type" /> property for the task cannot be set to the first
        ///         value specified in the error message when the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property is set to the second value in the
        ///         error message. Set
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Type" /> to
        ///         <see cref="Microsoft.Hpc.Scheduler.Properties.TaskType.ParametricSweep" /> and set
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> to True, or set
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.Type" /> to any value other than
        ///         <see cref="Microsoft.Hpc.Scheduler.Properties.TaskType.ParametricSweep" /> and set
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> to False, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040257.</para>
        /// </remarks>
        public const int Operation_TaskTypeAndIsParametricIncompatible = OperationErrorStart + 87;

        /// <summary>
        ///     <para>
        ///         The HPC cluster to which you are connected does not support a task of the specified type. Use another
        ///         task type, or connect to an HPC cluster that runs a later version of Windows HPC Server and create your job on
        ///         that cluster.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040244.</para>
        /// </remarks>
        public const int Operation_TaskTypeNotSupportedOnServer = OperationErrorStart + 68;

        /// <summary>
        ///     <para>
        ///         The job contains more than 100 parametric sweep tasks. Split the
        ///         job into two or more jobs that each contain 100 or fewer parameter sweep tasks.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040256</para>
        /// </remarks>
        public const int Operation_TooManyParametricSweepTasksPerJob = OperationErrorStart + 86;

        /// <summary>
        ///     <para>You cannot change the property when the node is in the current state.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004022F.</para>
        /// </remarks>
        public const int Operation_TryToChangePropertyInNodeState = OperationErrorStart + 47;

        /// <summary>
        ///     <para>You cannot modify the job property when the job is in its current state.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004021E.</para>
        /// </remarks>
        public const int Operation_TryToModifyInvalidProperty = OperationErrorStart + 30;

        /// <summary>
        ///     <para>You cannot modify the job property when the job is in its current state.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004021F.</para>
        /// </remarks>
        public const int Operation_TryToModifyInvalidStateJob = OperationErrorStart + 31;

        /// <summary>
        ///     <para>The task cannot be modified in its current state. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040247.</para>
        /// </remarks>
        public const int Operation_TryToModifyInvalidStateTask = OperationErrorStart + 71;

        /// <summary>
        ///     <para>The specified property cannot be set when the task is in the specified state.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Operation_TryToModifyInvalidStateTask2 = OperationErrorStart + 72;

        // Job modification 
        /// <summary>
        ///     <para>If the job is a backfill job, you cannot modify the job's runtime value when the job is running.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004021D.</para>
        /// </remarks>
        public const int Operation_TryToModifyRuntimeForRunningBackfillJob = OperationErrorStart + 29;

        /// <summary>
        ///     <para>The exception was not expected.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040264.</para>
        /// </remarks>
        public const int Operation_UnexpectedException = OperationErrorStart + 100;

        /// <summary>
        ///     <para>The specified node group does not exist.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040237.</para>
        /// </remarks>
        public const int Operation_UnknownNodeGroup = OperationErrorStart + 55;

        /// <summary>
        ///     <para>
        ///         The email notification properties could not be changed because the command to change the email notification
        ///         task inside Windows Task
        ///         Scheduler failed with the specified error code and message. For information about the error and how to resolve
        ///         it, see that error message.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040266.</para>
        /// </remarks>
        public const int Operation_WindowsTaskSchedulerExecFailure = OperationErrorStart + 102;

        /// <summary>
        ///     <para>
        ///         The application was unable to communicate with Windows Task Scheduler because of the
        ///         specified error. For information about the error and how to resolve it, see the specified error message.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040267.</para>
        /// </remarks>
        public const int Operation_WindowsTaskSchedulerNotStarted = OperationErrorStart + 103;

        /// <summary>
        ///     <para>
        ///         The command to modify the email notification task in
        ///         Windows Task Scheduler timed out. Try updating the email notification properties again later.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040265.</para>
        /// </remarks>
        public const int Operation_WindowsTaskSchedulerTimeout = OperationErrorStart + 101;

        // Software license validation
        /// <summary>
        ///     <para>
        ///         The software license format is not valid. The valid format is application:
        ///         numberoflicenses{,application:numberoflicenses.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004022D.</para>
        /// </remarks>
        public const int Operation_WrongLicenseFormat = OperationErrorStart + 45;

        /// <summary>
        ///     <para>The cluster does not contain a node that can run a broker job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800405E9.</para>
        /// </remarks>
        public const int ResourceAssignement_FailedToFindBrokerNode = ResourceAssignmentErrorStart + 1;

        /// <summary>
        ///     <para>Failed to run the activation filter specified in the ActivationFilterProgram cluster parameter.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800405EA.</para>
        /// </remarks>
        public const int ResourceAssignement_FailedToLaunchActivationFilter = ResourceAssignmentErrorStart + 2;

        /// <summary>
        ///     <para>There are no nodes available to run the task.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800405EB.</para>
        /// </remarks>
        public const int ResourceAssignement_NodeUnavailable = ResourceAssignmentErrorStart + 3;

        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int ServiceBroker_ExitCode_End = ServiceBroker_ExitCode_Start + 1000;

        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int ServiceBroker_ExitCode_Start = ServiceHost_ExitCode_End;

        /// <summary>
        ///     <para>
        ///         The BackendBinding value specified in the service broker's configuration
        ///         file is not valid. Correct the binding and start a new session.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004158E.</para>
        /// </remarks>
        public const int ServiceBroker_InvalidBackendBinding = ServiceBroker_ExitCode_Start + 6;

        /// <summary>
        ///     <para>The configuration file for the service broker was not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004158B.</para>
        /// </remarks>
        public const int ServiceBroker_InvalidConfig = ServiceBroker_ExitCode_Start + 3;

        /// <summary>
        ///     <para>The broker could not find the HPC environment information.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80041589.</para>
        /// </remarks>
        public const int ServiceBroker_MissingHpcEnvironmentInfomation = ServiceBroker_ExitCode_Start + 1;

        /// <summary>
        ///     <para>The service name or assembly name was not specified.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004158A.</para>
        /// </remarks>
        public const int ServiceBroker_MissingServiceInformation = ServiceBroker_ExitCode_Start + 2;

        /// <summary>
        ///     <para>The broker did not open the service broker web service.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004158D.</para>
        /// </remarks>
        public const int ServiceBroker_OpenBrokerServiceFailure = ServiceBroker_ExitCode_Start + 5;

        /// <summary>
        ///     <para>The broker did not open the service registration web service.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004158C.</para>
        /// </remarks>
        public const int ServiceBroker_OpenRegistrationServiceFailure = ServiceBroker_ExitCode_Start + 4;

        /// <summary>
        ///     <para>An unexpected exception occurred. For more information, see the broker execution log.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004159C.</para>
        /// </remarks>
        public const int ServiceBroker_UnexpectedError = ServiceBroker_ExitCode_Start + 20;

        /// <summary>
        ///     <para>
        ///         Not able to find the service assembly file name in the
        ///         service registration file. Make sure that the service registration file has the correct format.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411AC.</para>
        /// </remarks>
        public const int ServiceHost_AssemblyFileNameNullOrEmpty = ServiceHost_ExitCode_Start + 12;

        /// <summary>
        ///     <para>
        ///         Failed to find the service assembly file. Make sure that the assembly has been deployed and registered on the
        ///         node.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411A1.</para>
        /// </remarks>
        public const int ServiceHost_AssemblyFileNotFound = ServiceHost_ExitCode_Start + 1;

        /// <summary>
        ///     <para>
        ///         Failed to load the service assembly. Make sure that the security settings
        ///         for the assembly are correct and that
        ///         the service assembly information at HKEY_LOCAL_MACHINE\Microsoft\Hpc\ServiceRegistry\&lt;ServiceName&gt;
        ///         \AssemblyPath) is correct.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411A2.</para>
        /// </remarks>
        public const int ServiceHost_AssemblyLoadingError = ServiceHost_ExitCode_Start + 2;

        /// <summary>
        ///     <para>
        ///         The service registration file is not in the correct format. Use the service
        ///         registration XML schema to make sure that the service registration file is in the correct format.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411AD.</para>
        /// </remarks>
        public const int ServiceHost_BadServiceRegistrationFileFormat = ServiceHost_ExitCode_Start + 13;

        /// <summary>
        ///     <para>
        ///         Failed to find the service contract interface from the service assembly. Make sure
        ///         that the contract information provided in the
        ///         service registration is valid (see HKLM\Microsoft\Hpc\ServiceRegistry\&lt;ServiceName&gt;\AssemblyPath and
        ///         Contract type).
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411A3.</para>
        /// </remarks>
        public const int ServiceHost_ContractDiscoverError = ServiceHost_ExitCode_Start + 3;

        /// <summary>
        ///     <para>The service registry is corrupted.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411A9.</para>
        /// </remarks>
        public const int ServiceHost_CorruptServiceRegistry = ServiceHost_ExitCode_Start + 9;

        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int ServiceHost_ExitCode_End = ServiceHost_ExitCode_Start + 1000;

        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int ServiceHost_ExitCode_Start = ExceptionCodeEnd;

        /// <summary>
        ///     <para>Failed to register to the broker service. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411A8.</para>
        /// </remarks>
        public const int ServiceHost_FailedToRegisterLB = ServiceHost_ExitCode_Start + 8;

        /// <summary>
        ///     <para>
        ///         The command line for the HpcServiceHost.exe command contained an error. See the error message for more
        ///         information.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411AE.</para>
        /// </remarks>
        public const int ServiceHost_IncorrectCommandLineParameter = ServiceHost_ExitCode_Start + 14;

        /// <summary>
        ///     <para>Failed to find the contract implemented by the service type. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411A6.</para>
        /// </remarks>
        public const int ServiceHost_NoContractImplemented = ServiceHost_ExitCode_Start + 6;

        /// <summary>
        ///     <para>Command-line Help for the HpcServiceHost.exe command could not be displayed.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411B0.</para>
        /// </remarks>
        public const int ServiceHost_PrintCommandHelp = ServiceHost_ExitCode_Start + 16;

        /// <summary>
        ///     <para>The CCP_SERVICE_CONFIG_FILENAME environment variable is not set. Set the environment variable and try again.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411AF.</para>
        /// </remarks>
        public const int ServiceHost_ServiceConfigFileNameNotSpecified = ServiceHost_ExitCode_Start + 15;

        /// <summary>
        ///     <para>
        ///         Failed to open the service host. Make sure that the NetTcp port sharing service is running
        ///         and that the user has permission to use port sharing service, and that the firewall settings are correct.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411A7.</para>
        /// </remarks>
        public const int ServiceHost_ServiceHostFailedToOpen = ServiceHost_ExitCode_Start + 7;

        /// <summary>
        ///     <para>Failed to open the service host due to AddressAlreadyInUseException. </para>
        /// </summary>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411B5.</para>
        /// </remarks>
        public const int ServiceHost_ServiceHostFailedToOpen_AddressAlreadyInUse = ServiceHost_ExitCode_Start + 21;

        /// <summary>
        ///     <para>The service name is not specified.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411AA.</para>
        /// </remarks>
        public const int ServiceHost_ServiceNameNotSpecified = ServiceHost_ExitCode_Start + 10;

        /// <summary>
        ///     <para>
        ///         Cannot locate the service registration file in both %CCP_HOME%ServiceRegistration folder and user
        ///         specified central registration folder (if any). Make sure that the service has been successfully deployed.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411AB.</para>
        /// </remarks>
        public const int ServiceHost_ServiceRegistrationFileNotFound = ServiceHost_ExitCode_Start + 11;

        /// <summary>
        ///     <para>
        ///         Failed to find the service type from the service assembly. Make
        ///         sure that the type information in the
        ///         service registration is valid (see HKEY_LOCAL_MACHINE\Microsoft\Hpc\ServiceRegistry\&lt;ServiceName&gt;
        ///         \AssemblyPath and ServiceType).
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411A4.</para>
        /// </remarks>
        public const int ServiceHost_ServiceTypeDiscoverError = ServiceHost_ExitCode_Start + 4;

        /// <summary>
        ///     <para>Failed to load the service type from the service assembly. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411A5.</para>
        /// </remarks>
        public const int ServiceHost_ServiceTypeLoadingError = ServiceHost_ExitCode_Start + 5;

        /// <summary>
        ///     <para>An unexpected exception occurred. For more information, see the service host execution log.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800411B4.</para>
        /// </remarks>
        public const int ServiceHost_UnexpectedException = ServiceHost_ExitCode_Start + 20;

        /// <summary>
        ///     <para>The operation was a success.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0.</para>
        /// </remarks>
        public const int Success = 0;

        /// <summary>
        ///     <para>An unknown error occurred.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80041972.</para>
        /// </remarks>
        public const int UnknowError = UnknownError;

        /// <summary>
        ///     <para>An unknown error has occurred.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80041972.</para>
        /// </remarks>
        public const int UnknownError = CustomErrorStart + 1;

        /// <summary>
        ///     <para>
        ///         An administrative job with dependencies between tasks was submitted, but an
        ///         administrative job cannot have dependencies between tasks. Remove the dependencies between tasks and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040656.</para>
        /// </remarks>
        public const int Validation_AdminJobCannotHaveTaskDependency = ValidationErrorStart + 110;

        /// <summary>
        ///     <para>A batch job cannot be a child job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040640.</para>
        /// </remarks>
        public const int Validation_BatchJobMustNotBeChildJob = ValidationErrorStart + 88;

        /// <summary>
        ///     <para>Failed to compute the maximum resource requirement for the job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004062F.</para>
        /// </remarks>
        public const int Validation_CalcJobMaxError = ValidationErrorStart + 71;

        // compute min/max
        /// <summary>
        ///     <para>Failed to compute the minimum resource requirement for the job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004062E.</para>
        /// </remarks>
        public const int Validation_CalcJobMinError = ValidationErrorStart + 70;

        /// <summary>
        ///     <para>A child job cannot contain another child job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004063C.</para>
        /// </remarks>
        public const int Validation_ChildJobContainsChildJob = ValidationErrorStart + 84;

        /// <summary>
        ///     <para>The parent identifier included in the child job differs from its parent’s actual identifier.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004063B.</para>
        /// </remarks>
        public const int Validation_ChildJobIdNotPairWithParentJobId = ValidationErrorStart + 83;

        /// <summary>
        ///     <para>The child job must be a router job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004063A.</para>
        /// </remarks>
        public const int Validation_ChildJobMustBeRouterJob = ValidationErrorStart + 82;

        // child job
        /// <summary>
        ///     <para>The child job is not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040638.</para>
        /// </remarks>
        public const int Validation_ChildJobNotValid = ValidationErrorStart + 80;

        /// <summary>
        ///     <para>The cluster does not contain the required minimum number of resources.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004060C.</para>
        /// </remarks>
        public const int Validation_ClusterSizeLessThanMin = ValidationErrorStart + 36;

        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_CombineSelectException = ValidationErrorStart + 13;

        // validating credentials
        /// <summary>
        ///     <para>The credentials specified for this job are not able to log on to the cluster. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800405EB.</para>
        /// </remarks>
        public const int Validation_CredentialCheckFailed = ValidationErrorStart + 3;

        /// <summary>
        ///     <para>Failed to calculate task group level.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040626.</para>
        /// </remarks>
        public const int Validation_FailToCalculateTaskGroupLevel = ValidationErrorStart + 62;

        /// <summary>
        ///     <para>In Fast Balanced scheduling mode, job shouldn't be exclusive, or have single node setting.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_FastBalanced_JobExclusive = ValidationErrorStart + 153;

        /// <summary>
        ///     <para>In Fast Balanced scheduling mode, job's min should be auto or 1.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_FastBalanced_JobMinGreaterThanOne = ValidationErrorStart + 152;

        /// <summary>
        ///     <para>In Fast Balanced scheduling mode, only core type is allowed for jobs.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_FastBalanced_JobUnitType = ValidationErrorStart + 151;

        /// <summary>
        ///     <para>In Fast Balanced scheduling mode, job's node group operation shouldn't be in uniform.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_FastBalanced_NodeUniform = ValidationErrorStart + 154;

        /// <summary>
        ///     <para>In Fast Balanced scheduling mode, pool should be disabled.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_FastBalanced_PoolEnabled = ValidationErrorStart + 150;

        /// <summary>
        ///     <para>The contents of the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.DependsOn" /> property is not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040627.</para>
        /// </remarks>
        public const int Validation_InvalidDependsOn = ValidationErrorStart + 63;

        /// <summary>
        ///     <para>
        ///         The values for the node-related properties of the job create criteria that are not valid for selecting
        ///         nodes on which the job can run. Change the values for the node-related properties of the job, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040652.</para>
        /// </remarks>
        public const int Validation_InvalidNodeCriteriaForJob = ValidationErrorStart + 106;

        // Invalid settings for tasks of a given type
        /// <summary>
        ///     <para>
        ///         The specified property cannot be set for tasks of the specified type. Remove
        ///         the value set for the property or change the type of the task, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004064A.</para>
        /// </remarks>
        public const int Validation_InvalidPropForTaskType = ValidationErrorStart + 98;

        /// <summary>
        ///     <para>
        ///         Job submission failed because the job submission filter application is not valid. Your cluster administrator
        ///         should check that the submission filter is an executable binary file (".exe" file) or Windows command script
        ///         (".cmd" file).
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040644.</para>
        /// </remarks>
        public const int Validation_InvalidSubmissionFilter = ValidationErrorStart + 92;

        /// <summary>
        ///     <para>A node that the job requests does not support the job’s job type.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004060F.</para>
        /// </remarks>
        public const int Validation_JobAskedNodeMustContainJobType = ValidationErrorStart + 39;

        /// <summary>
        ///     <para>
        ///         The job failed because the tasks failed in the previous submission. Please check the failed task for more
        ///         details on the failure.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_JobFailureForTasksFailInLastSubmission = ValidationErrorStart + 144;

        // validate job dependency
        /// <summary>
        ///     <para>
        ///         This job depends on parent jobs which no longer exist. See the error message for the list of missing parent
        ///         jobs.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_JobHasMissingParentJobs = ValidationErrorStart + 16;

        /// <summary>
        ///     <para>
        ///         This job depends on parent jobs which have not been submitted. See the error message for the list of missing
        ///         parent jobs.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_JobHasUnsubmittedParentJobs = ValidationErrorStart + 17;

        /// <summary>
        ///     <para>The job is missing the user name or password.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800405EC.</para>
        /// </remarks>
        public const int Validation_JobMissUsernameOrPassword = ValidationErrorStart + 4;

        // allocation requirement

        /// <summary>
        ///     <para>The list of requested nodes does not contain a valid node. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040601.</para>
        /// </remarks>
        public const int Validation_JobRequestedNodesContainZeroValidNode = ValidationErrorStart + 25;

        /// <summary>
        ///     <para>The job requires a node that is not available.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004060E.</para>
        /// </remarks>
        public const int Validation_JobRequiredNodeNotAvailable = ValidationErrorStart + 38;

        /// <summary>
        ///     <para>The list of nodes that the task requires must be included in the list of the nodes that the job requested.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004060D.</para>
        /// </remarks>
        public const int Validation_JobRequiredNodeNotInJobAskedNodes = ValidationErrorStart + 37;

        /// <summary>
        ///     <para>The maximum number of resource units must be larger than zero.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040623.</para>
        /// </remarks>
        public const int Validation_MaxLessThanOne = ValidationErrorStart + 59;

        /// <summary>
        ///     <para>The maximum number of resource units cannot be less than zero.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040609.</para>
        /// </remarks>
        public const int Validation_MaxLessThanZero = ValidationErrorStart + 33;

        /// <summary>
        ///     <para>
        ///         You must specify the maximum number of resource units that the job requires if the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMax" /> property is false.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040607.</para>
        /// </remarks>
        public const int Validation_MaxNotSpecifiedWhenAutoCalcMaxIsFalse = ValidationErrorStart + 31;

        /// <summary>
        ///     <para>You must specify a maximum resource value for the task.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040615.</para>
        /// </remarks>
        public const int Validation_MaxUndefined = ValidationErrorStart + 45;

        /// <summary>
        ///     <para>The minimum value must be less than the maximum value. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004060A.</para>
        /// </remarks>
        public const int Validation_MinGreaterThanMax = ValidationErrorStart + 34;

        /// <summary>
        ///     <para>The minimum number of resource units must be larger than zero.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040622.</para>
        /// </remarks>
        public const int Validation_MinLessThanOne = ValidationErrorStart + 58;

        /// <summary>
        ///     <para>The minimum number of resource units cannot be less than zero.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040608.</para>
        /// </remarks>
        public const int Validation_MinLessThanZero = ValidationErrorStart + 32;

        /// <summary>
        ///     <para>
        ///         You must specify the minimum number of resource units that the job requires if the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AutoCalculateMin" /> property is false.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040606.</para>
        /// </remarks>
        public const int Validation_MinNotSpecifiedWhenAutoCalcMinIsFalse = ValidationErrorStart + 30;

        /// <summary>
        ///     <para>You must specify a minimum resource value for the task.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040614.</para>
        /// </remarks>
        public const int Validation_MinUndefined = ValidationErrorStart + 44;

        // validate task
        /// <summary>
        ///     <para>The task must specify a command to run.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040610.</para>
        /// </remarks>
        public const int Validation_MissCommandLine = ValidationErrorStart + 40;

        // Multiple prep and release tasks per job
        /// <summary>
        ///     <para>
        ///         The job contains more than one node preparation task, but a job can
        ///         contain only one such task. Remove all except one of the node preparation tasks, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040647.</para>
        /// </remarks>
        public const int Validation_MultipleNodePrepTasksPerJob = ValidationErrorStart + 95;

        /// <summary>
        ///     <para>
        ///         The job contains more than one node release task, but a job can
        ///         contain only one such task. Remove all except one of the node release tasks, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040648.</para>
        /// </remarks>
        public const int Validation_MultipleNodeReleaseTasksPerJob = ValidationErrorStart + 96;

        /// <summary>
        ///     <para>
        ///         A job with a node group operator did not specify any node groups. A job that uses a node group operator must
        ///         specify node groups.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_NodeGroupOpNoNodeGroup = ValidationErrorStart + 115;

        /// <summary>
        ///     <para>
        ///         Admin jobs cannot use the Uniform node group operator. Admin jobs should use the Union or Intersect
        ///         operators.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_NodeGroupOpUniform_AdminJob = ValidationErrorStart + 114;

        // Uniform Node Group validation errors
        /// <summary>
        ///     <para>
        ///         All the candidate node lists for a job with the Uniform node group
        ///         operator cannot be empty. The Job Scheduler service determines the candidate node lists using the following
        ///         job properties: NodeGroups, RequestedNodes, MinMemoryPerNode, MaxMemoryPerNode, MinCoresPerNode,
        ///         MaxCoresPerNode, and ExcludedNodes. Either reduce the number of
        ///         resources that the job requires, or redefine the relevant job properties, and then submit the job again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_NodeGroupOpUniform_NodeListEmpty = ValidationErrorStart + 111;

        /// <summary>
        ///     <para>
        ///         This job with the Uniform node group operator requires more resources than the Job Scheduler service returned
        ///         for
        ///         this job. See the error message for the specific resource that is insufficient to run this job. The Job
        ///         Scheduler
        ///         service determines the candidate node lists using the following job properties: NodeGroups, RequestedNodes,
        ///         MinMemoryPerNode, MaxMemoryPerNode, MinCoresPerNode, MaxCoresPerNode, and ExcludedNodes. Either
        ///         reduce the number of resources that the job requires, or redefine the relevant job properties, and then submit
        ///         the job again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_NodeGroupOpUniform_NodeListLessThanMin = ValidationErrorStart + 112;

        /// <summary>
        ///     <para>
        ///         This job with the Uniform node group operator has a required node that is not
        ///         part of all the lists of candidate nodes that the Job Scheduler service returned for this job.
        ///         See the error message for the specific node that is generated the error. The Job Scheduler service
        ///         determines the candidate node lists using the following job properties: NodeGroups, RequestedNodes,
        ///         MinMemoryPerNode, MaxMemoryPerNode, MinCoresPerNode, MaxCoresPerNode, and ExcludedNodes.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_NodeGroupOpUniform_NodeListMissingRequiredNode = ValidationErrorStart + 113;

        /// <summary>
        ///     <para>The computed resources for the job is less than the required number of resources.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004060B.</para>
        /// </remarks>
        public const int Validation_NodeListSizeLessThanMin = ValidationErrorStart + 35;

        /// <summary>
        ///     <para>The node is not present in the cluster.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040603.</para>
        /// </remarks>
        public const int Validation_NodeNotExist = ValidationErrorStart + 27;

        /// <summary>
        ///     <para>
        ///         The cluster does not contain a node that supports the specified resource requirements for
        ///         the job (for example, memory or core requirements). Please adjust your requirements and submit the job again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040602.</para>
        /// </remarks>
        public const int Validation_NoNodeFulfillsTheSelectionCriteria = ValidationErrorStart + 26;

        /// <summary>
        ///     <para>
        ///         Job submission failed because the job contains no runnable tasks other than
        ///         node preparation and node release tasks, and the job is not set to run until
        ///         it is canceled. Add a task other than a node preparation or node release task
        ///         to the job, or set the job to run until it is canceled, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040649.</para>
        /// </remarks>
        public const int Validation_OnlyNodePrepOrReleaseTasksInJob = ValidationErrorStart + 97;

        /// <summary>
        ///     <para>The parent job must be a service job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040639.</para>
        /// </remarks>
        public const int Validation_ParentJobMustBeServiceJob = ValidationErrorStart + 81;

        /// <summary>
        ///     <para>The parent job is not valid.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004063D.</para>
        /// </remarks>
        public const int Validation_ParentJobNotValid = ValidationErrorStart + 85;

        /// <summary>
        ///     <para>The job template does not exist.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800405F7.</para>
        /// </remarks>
        public const int Validation_ProfileNotExist = ValidationErrorStart + 15;

        /// <summary>
        ///     <para>The property value is not in the allowed range of values as specified by the job template.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800405F6.</para>
        /// </remarks>
        public const int Validation_PropertyOutOfRange = ValidationErrorStart + 14;

        /// <summary>
        ///     <para>
        ///         A job with the specified minimum resource requirements cannot have a greater
        ///         number of required nodes. See the specific error message to determine the resource and quantity required.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_RequiredNodesMoreThanMinUnits = ValidationErrorStart + 140;

        // validating profile
        /// <summary>
        ///     <para>A property that requires a value as specified by the job template has not been set.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800405F3.</para>
        /// </remarks>
        public const int Validation_RequiredPropertyNotSet = ValidationErrorStart + 11;

        /// <summary>
        ///     <para>A router job cannot be a child job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004063E.</para>
        /// </remarks>
        public const int Validation_RouterJobMustBeChildJob = ValidationErrorStart + 86;

        /// <summary>
        ///     <para>
        ///         To reserve resources for a job (when the  job does not contain tasks and has requested that it run until
        ///         canceled),
        ///         you must specify the maximum and minimum resource values for the job
        ///         – you cannot request that the scheduler compute the maximum and minimum resource values.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040605.</para>
        /// </remarks>
        public const int Validation_RunUntilCanceledButAutoMinMaxSet = ValidationErrorStart + 29;

        /// <summary>
        ///     <para>
        ///         To reserve resources for a job (when the  job does not contain tasks and
        ///         has requested that it run until canceled), you must specify the maximum and minimum resource values for the
        ///         job.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040604.</para>
        /// </remarks>
        public const int Validation_RunUntilCanceledButMinMaxNotSpecified = ValidationErrorStart + 28;

        /// <summary>
        ///     <para>A service job cannot be a child job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004063F.</para>
        /// </remarks>
        public const int Validation_ServiceJobMustNotBeChildJob = ValidationErrorStart + 87;

        /// <summary>
        ///     <para>
        ///         The job contains more than one service task or contains a service task that combines tasks other
        ///         than node preparation and node release tasks. A job can contain only one service task, and if the job
        ///         contains a service task, it can only contain additional tasks that are node preparation or node release tasks.
        ///         Remove
        ///         the extra service tasks of the tasks that are not service, node preparation, or node release tasks, then try
        ///         again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040651.</para>
        /// </remarks>
        public const int Validation_ServiceTaskIsNotAlone = ValidationErrorStart + 105;

        /// <summary>
        ///     <para>
        ///         The single node job requires a greater quantity of resources than any candidate node
        ///         is able to provide. See the error message to determine the resource and quantity required and available.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_SingleNode_AllNodesInAllListsLessThanMin = ValidationErrorStart + 132;

        /// <summary>
        ///     <para>
        ///         The single node job requires a greater quantity of a specific resources than any node usable
        ///         by the job is able to provide. See the error message to determine the resource and quantity required and
        ///         available.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_SingleNode_AllNodesInNodeListLessThanMin = ValidationErrorStart + 133;

        /// <summary>
        ///     <para>
        ///         A Single Node job cannot have a minimum requirement of more
        ///         than 1 node. Correct the minimum number of nodes required and submit the job again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_SingleNode_MoreThanOneNode = ValidationErrorStart + 131;

        // Single Node validation errors
        /// <summary>
        ///     <para>
        ///         A Single Node job cannot specify multiple required nodes. Correct the number of required nodes and submit the
        ///         job again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_SingleNode_MultipleRequiredNodes = ValidationErrorStart + 130;

        /// <summary>
        ///     <para>
        ///         Job submission failed because the job submission filter application
        ///         could not be found. Your cluster administrator should check that the submission
        ///         filter can be accessed from the head node of the cluster and
        ///         the path of the submission filter is a fully-qualified (not relative) path name.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040643.</para>
        /// </remarks>
        public const int Validation_SubmissionFilterDoesNotExist = ValidationErrorStart + 91;

        // submission filter
        /// <summary>
        ///     <para>The job did not pass the job submission filter specified in the SubmissionFilterProgram cluster parameter.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040642.</para>
        /// </remarks>
        public const int Validation_SubmissionFilterFailed = ValidationErrorStart + 90;

        /// <summary>
        ///     <para>
        ///         A submission filter created a job property that is not valid because of the specified
        ///         error. For information about the error and how to resolve it, see the error message for the specified error.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040655.</para>
        /// </remarks>
        public const int Validation_SubmissionFilterInvalidJobTerm = ValidationErrorStart + 109;

        /// <summary>
        ///     <para>
        ///         A submission filter timed out after the specified number of milliseconds elapsed. Use the
        ///         <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" /> method to
        ///         increase the value of the SubmissionFilterTimeout cluster parameter, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040654.</para>
        /// </remarks>
        public const int Validation_SubmissionFilterTimeout = ValidationErrorStart + 108;

        /// <summary>
        ///     <para>
        ///         The specified value of the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.TargetResourceCount" /> property for the job is lower than the
        ///         minimum of the expected range for this job. The value must be between the minimum and maximum values that the
        ///         error message specifies, or have a value of 0 to specify that the resource count should not be adjusted
        ///         dynamically. Change the value of the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.TargetResourceCount" /> property so that it is in the
        ///         specified range or is 0, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040650.</para>
        /// </remarks>
        public const int Validation_TargetResourceCountLessThanMin = ValidationErrorStart + 104;

        /// <summary>
        ///     <para>
        ///         The specified value of the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.TargetResourceCount" /> property for the job is larger than
        ///         the maximum of the expected range for this job. The value must be between the minimum and maximum values that
        ///         the error message specifies, or have a value of 0 to specify that the resource count should not be adjusted
        ///         dynamically. Change the value of the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.TargetResourceCount" /> property so that it is in the
        ///         specified range or is 0, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004064F.</para>
        /// </remarks>
        public const int Validation_TargetResourceCountMoreThanMax = ValidationErrorStart + 103;

        /// <summary>
        ///     <para>The task must specify the same resource unit as the job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040613.</para>
        /// </remarks>
        public const int Validation_TaskAllocUnitNotSameWithJob = ValidationErrorStart + 43;

        /// <summary>
        ///     <para>The dependency list for multiple tasks creates a circular dependency.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040625.</para>
        /// </remarks>
        public const int Validation_TaskDependenciesContainCycle = ValidationErrorStart + 61;

        /// <summary>
        ///     <para>
        ///         The task dependency tree exceeds the maximum depth. Adjust the job dependencies to reduce the depth of the
        ///         dependency tree.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_TaskDependencyTreeTooDeep = ValidationErrorStart + 65;

        /// <summary>
        ///     <para>
        ///         The minimum and maximum resource requirements cannot be computed for a job with exclusive
        ///         access to the nodes. You must specify the minimum and maximum resource values and resubmit the job.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004061E.</para>
        /// </remarks>
        public const int Validation_TaskExclusiveWhileJobAutoMinMaxEnabled = ValidationErrorStart + 54;

        /// <summary>
        ///     <para>The task can run exclusively on a node only if the job specifies that it must run exclusively on the node.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040619.</para>
        /// </remarks>
        public const int Validation_TaskExclusiveWhileJobNot = ValidationErrorStart + 49;

        /// <summary>
        ///     <para>
        ///         The task failed because the scheduler could not validate its parent
        ///         job.  Correct the validation error on the parent job and resubmit the job.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040620.</para>
        /// </remarks>
        public const int Validation_TaskFailedOnJobValidationFault = ValidationErrorStart + 56;

        /// <summary>
        ///     <para>
        ///         The task specifies a list of dependent tasks but the task does not specify a
        ///         name value for itself; each dependent task and the task specifying the dependency must include a name value.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040628.</para>
        /// </remarks>
        public const int Validation_TaskHasDependOnButNoName = ValidationErrorStart + 64;

        /// <summary>
        ///     <para>
        ///         The task cannot specify required nodes when the job has requested that the scheduler compute the required
        ///         resources.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004061F.</para>
        /// </remarks>
        public const int Validation_TaskHasRequiredNodesWhileJobAutoMinMaxEnabled = ValidationErrorStart + 55;

        // parametric sweep task
        /// <summary>
        ///     <para>The increment value for a parametric task must be larger than or equal to 1.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040645.</para>
        /// </remarks>
        public const int Validation_TaskIncrementValueLessThanZero = ValidationErrorStart + 93;

        /// <summary>
        ///     <para>The start value for a parametric task cannot be larger than end value.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040646.</para>
        /// </remarks>
        public const int Validation_TaskInvalidParametricSweep = ValidationErrorStart + 94;

        /// <summary>
        ///     <para>The maximum resource value for the task must be less than that of the job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040617.</para>
        /// </remarks>
        public const int Validation_TaskMaxGreaterThanJobMax = ValidationErrorStart + 47;

        /// <summary>
        ///     <para>The minimum resource value for the task must be less than that of the job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040616.</para>
        /// </remarks>
        public const int Validation_TaskMinGreaterThanJobMin = ValidationErrorStart + 46;

        /// <summary>
        ///     <para>
        ///         The minimum resource value for the task must be less than its maximum
        ///         The minimum resource value for the task must be less than that of the job value.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040618.</para>
        /// </remarks>
        public const int Validation_TaskMinGreaterThanTaskMax = ValidationErrorStart + 48;

        /// <summary>
        ///     <para>
        ///         The job contains a node release task, but this HPC cluster has support for node release turned off. Remove the
        ///         node release job
        ///         from the task, or contact a cluster administrator to request that the support
        ///         for node release tasks be turned on for the HPC cluster, then try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004064B.</para>
        /// </remarks>
        public const int Validation_TaskNodeReleaseDisabled = ValidationErrorStart + 99;

        /// <summary>
        ///     <para>
        ///         The value of the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" /> property for the node release task runtime exceeds
        ///         the globally configured maximum that the error message specifies. Change the value of the
        ///         <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" /> property so that it is lower than the value that
        ///         the error message specifies, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004064C.</para>
        /// </remarks>
        public const int Validation_TaskNodeReleaseExceedsMaxRunTime = ValidationErrorStart + 100;

        /// <summary>
        ///     <para>A specified nodegroup is requested in task, but not available or empty at the moment.</para>
        /// </summary>
        public const int Validation_TaskRequestedNodeGroupNotAvailable = ValidationErrorStart + 141;

        /// <summary>
        ///     <para>A specified nodegroup is requested in task, but the job also has requested node groups at the moment.</para>
        /// </summary>
        public const int Validation_TaskRequestedNodeGroupWhileJobNodeGroups = ValidationErrorStart + 143;

        /// <summary>
        ///     <para>A specified nodegroup is requested in task, but the job or task has also set requiredNode at the moment.</para>
        /// </summary>
        public const int Validation_TaskRequestedNodeGroupWhileJobTaskRequiredNodes = ValidationErrorStart + 142;

        /// <summary>
        ///     <para>
        ///         You have exceeded the number of times that a task can be queued again.
        ///         The TaskRetryCount cluster parameter specifies the maximum number of times that a task can be queued again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040611.</para>
        /// </remarks>
        public const int Validation_TaskRequeuedTooManyTimes = ValidationErrorStart + 41;

        // task required node 
        /// <summary>
        ///     <para>
        ///         The specified node is a required node for the task, but that node is not allocated to the job that contains
        ///         the task.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040653.</para>
        /// </remarks>
        public const int Validation_TaskRequiredNodeNotAllocatedToRunningJob = ValidationErrorStart + 107;

        /// <summary>
        ///     <para>The job requires a node that is not available.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004061C.</para>
        /// </remarks>
        public const int Validation_TaskRequiredNodeNotAvailable = ValidationErrorStart + 52;

        /// <summary>
        ///     <para>The list of nodes that the task requires must be included in the list of the nodes that the job requested.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004061A.</para>
        /// </remarks>
        public const int Validation_TaskRequiredNodeNotInJobAskedNodes = ValidationErrorStart + 50;

        /// <summary>
        ///     <para>The task specifies more required nodes than the job's specified maximum resource usage.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004061D.</para>
        /// </remarks>
        public const int Validation_TaskRequiredNodeOutOfJobMaximumResource = ValidationErrorStart + 53;

        /// <summary>
        ///     <para>
        ///         The run-time value for the task is larger than the length of time that the job is scheduled to remain
        ///         running.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004061B.</para>
        /// </remarks>
        public const int Validation_TaskRuntimeGreaterThanJob = ValidationErrorStart + 51;

        /// <summary>
        ///     <para>
        ///         An attempt was made to add a task of the specified type to a job in the Configuring state, but tasks of
        ///         that type cannot be added to a job in the Configuring state. Wait until the job is in a state other than
        ///         Configuration, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004064D.</para>
        /// </remarks>
        public const int Validation_TaskTypeAddedInWrongJobState = ValidationErrorStart + 101;

        /// <summary>
        ///     <para>The task failed validation.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040621.</para>
        /// </remarks>
        public const int Validation_TaskValidationFailed = ValidationErrorStart + 57;

        /// <summary>
        ///     <para>
        ///         The parametric task has current_number subtasks, which exceeds the
        ///         maximum_number of subtasks that a parametric task can have. Split the parametric
        ///         task into two or more parametric tasks that have fewer than the maximum_number of subtasks that a parametric
        ///         task can have, and try again.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x8004064E.</para>
        /// </remarks>
        public const int Validation_TooManyParametricInstances = ValidationErrorStart + 102;

        /// <summary>
        ///     <para>You cannot queue the task again because the task is marked to run only one time. </para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040612.</para>
        /// </remarks>
        public const int Validation_TryToRequeueNonRerunnableTask = ValidationErrorStart + 42;

        // task dependency and task group
        /// <summary>
        ///     <para>The depends on list is ambiguous because multiple tasks specify the same name.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x80040624.</para>
        /// </remarks>
        public const int Validation_TwoTasksWithSameNameDifferentGroup = ValidationErrorStart + 60;

        /// <summary>
        ///     <para>An unexpected exception occurred while validating the job.</para>
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        /// <remarks>
        ///     <para>For COM developers, the HRESULT value is 0x800405E9.</para>
        /// </remarks>
        public const int Validation_UnexpectedExceptionWhenValidating = ValidationErrorStart + 1;

        /// <summary>
        ///     <para />
        /// </summary>
        /// <returns>
        ///     <para />
        /// </returns>
        public const int Validation_Unknown = ValidationErrorStart + 2;

        // facility code: FACILITY_ITF; status code: start from 0x0200
        private const int ErrorCodeStart = unchecked((int)0x80040200);

        private const int ExceptionCodeEnd = ExceptionCodeStart + 1000;

        private const int ExceptionCodeStart = ExecutionErrorEnd;

        private const int ExecutionErrorEnd = ExecutionErrorStart + 1000;

        private const int ExecutionErrorStart = ValidationErrorEnd;

        private const int OperationErrorEnd = OperationErrorStart + 1000;

        private const int OperationErrorStart = ErrorCodeStart;

        private const int ResourceAssignmentErrorEnd = ResourceAssignmentErrorStart + 1000;

        private const int ResourceAssignmentErrorStart = CustomErrorEnd;

        // prefix of resource item name in resources
        private const string ResourcePrefix = "SchedulerError_";

        private const int ValidationErrorEnd = ValidationErrorStart + 1000;

        private const int ValidationErrorStart = OperationErrorEnd;

        private static readonly object initLock = new object();

        // error code to message table
        private static Dictionary<int, string> errorMessages;

        /// <summary>
        ///     <para>Defines the category of errors into which the <see cref="ErrorCode" /> codes are grouped.</para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         To use this enumeration in Visual Basic Scripting Edition (VBScript), you
        ///         need to use the numeric values for the enumeration members or create constants that
        ///         correspond to those members and set them equal to the numeric values. The
        ///         following code example shows how to create and set constants for this enumeration in VBScript.
        ///     </para>
        ///     <code language="vbs">const OperationError = 0
        /// const ValidationError = 1
        /// const ResourceAssignmentError = 2
        /// const ExecutionError = 3
        /// const Other = 4</code>
        /// </remarks>
        /// <example />
        public enum Category
        {
            /// <summary>
            ///     <para>
            ///         An error occurred while performing an operation (for example, the user tried to delete a
            ///         job template but they do not have permissions to delete templates). This enumeration member represents a value
            ///         of 0.
            ///     </para>
            /// </summary>
            OperationError,

            /// <summary>
            ///     <para>
            ///         The error occurred while validating the job or task before it ran. This enumeration member represents a value
            ///         of 1.
            ///     </para>
            /// </summary>
            ValidationError,

            /// <summary>
            ///     <para>
            ///         The error occurred while assigning resources to the job or task. This enumeration member represents a value
            ///         of 2.
            ///     </para>
            /// </summary>
            ResourceAssignmentError,

            /// <summary>
            ///     <para>The error occurred while executing the job or task. This enumeration member represents a value of 3.</para>
            /// </summary>
            ExecutionError,

            /// <summary>
            ///     <para>Includes errors related to starting service or broker jobs. This enumeration member represents a value of 4.</para>
            /// </summary>
            Other
        }

        /// <summary>
        ///     <para>Gets the error category to which the specified error code belongs.</para>
        /// </summary>
        /// <param name="code">
        ///     <para>The error code.</para>
        /// </param>
        /// <returns>
        ///     <para>The error category. For possible values, see the  <see cref="Category" /> enumeration.</para>
        /// </returns>
        public static Category ErrorCategory(int code)
        {
            if (code < OperationErrorEnd && code > OperationErrorStart)
            {
                return Category.OperationError;
            }

            if (code < ValidationErrorEnd && code > ValidationErrorStart)
            {
                return Category.ValidationError;
            }

            if (code < ResourceAssignmentErrorEnd && code > ResourceAssignmentErrorStart)
            {
                return Category.ResourceAssignmentError;
            }

            if (code < ExecutionErrorEnd && code > ExecutionErrorStart)
            {
                return Category.ExecutionError;
            }

            return Category.Other;
        }

        /// <summary>
        ///     <para>
        ///         Retrieves whether you can try the operation that generated the specified error value again without making
        ///         changes.
        ///     </para>
        /// </summary>
        /// <param name="errorCode">
        ///     <para>
        ///         An integer value for the error. This can be any of the fields in the
        ///         <see cref="ErrorCode" /> class.
        ///     </para>
        /// </param>
        /// <returns>
        ///     <para>
        ///         A
        ///         <see cref="System.Boolean" /> that indicates whether you can try the operation that generated the specified
        ///         error value again without making changes. True indicates that you can try the operation that generated the
        ///         specified error value again without making changes. False indicates that you cannot try the operation that
        ///         generated the specified error value again without making changes.
        ///     </para>
        /// </returns>
        public static bool IsOperationRetriable(int errorCode)
        {
            switch (errorCode)
            {
                case Operation_ServerIsBusy:
                case Operation_ObjectInUse:
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     <para>Creates the message text for the error using the specified insertion strings.</para>
        /// </summary>
        /// <param name="arguments">
        ///     <para>An array of insertion strings.</para>
        /// </param>
        /// <returns>
        ///     <para>The message text for the error.</para>
        /// </returns>
        public static string MakeErrorParams(params string[] arguments)
        {
            return string.Join(ArgumentSeparator, arguments);
        }

        /// <summary>
        ///     <para>Retrieves a formatted string that represents the message string for the specified error code.</para>
        /// </summary>
        /// <param name="errorCode">
        ///     <para>The error code.</para>
        /// </param>
        /// <param name="errorParams">
        ///     <para>An array of insertion strings.</para>
        /// </param>
        /// <returns>
        ///     <para>The message text for the error.</para>
        /// </returns>
        public static string ToString(int errorCode, string errorParams)
        {
            InitMessages();

            string errorMessageTemplate;
            if (!errorMessages.TryGetValue(errorCode, out errorMessageTemplate))
            {
                return SR.SchedulerError_Unknown;
            }

            string[] arguments = { };
            if (errorParams != null)
            {
                // An empty string is allowed
                arguments = errorParams.Split(new[] { ArgumentSeparator }, StringSplitOptions.None);
            }

            try
            {
                return string.Format(SR.Culture, errorMessageTemplate, arguments);
            }
            catch (FormatException)
            {
                // If cannot be formatted correctly, return an "unknown error" message
                return SR.SchedulerError_Unknown;
            }
        }

        private static void InitMessages()
        {
            if (errorMessages == null)
            {
                lock (initLock)
                {
                    if (errorMessages == null)
                    {
                        var messages = new Dictionary<int, string>();
                        foreach (var field in typeof(ErrorCode).GetFields(BindingFlags.Public | BindingFlags.Static))
                        {
                            var valueObj = field.GetValue(null);
                            if (valueObj == null || !(valueObj is int))
                            {
                                continue;
                            }

                            var value = (int)valueObj;

                            if (!messages.ContainsKey(value))
                            {
                                string message = null;

                                try
                                {
                                    message = SR.ResourceManager.GetString(ResourcePrefix + field.Name, SR.Culture);
                                }
                                catch
                                {
                                    message = null;
                                }

                                if (message == null)
                                {
                                    message = field.Name;
                                }

                                messages.Add(value, message);
                            }
                        }

                        errorMessages = messages;
                    }
                }
            }
        }
    }
}