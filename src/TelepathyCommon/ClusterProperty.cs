//------------------------------------------------------------------------------
// <copyright file="ClusterProperty.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Cluster properties definition file
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc
{
    /// <summary>
    /// Cluster properties definition file
    /// </summary>
    public static class ClusterProperty
    {
        public const string SpoolDirEntry = "SpoolDir";

        public const string AllowNewUserConnectionsEntry = "AllowNewUserConnections";

        public const string InactivityCountEntry = "InactivityCount";

        public const string InactivityCountAzureEntry = "InactivityCountAzure";

        public const string HeartbeatIntervalEntry = "HeartbeatInterval";

        public const string PreemptionEntry = "PreemptionType";

        public const string AffinityModeEntry = "AffinityType";

        // Maximum allowed run time for node release tasks
        public const string NodeReleaseTaskTimeoutEntry = @"NodeReleaseTaskTimeout";

        // Submission Filter Executable file name
        public const string SubmissionFilterExeEntry = @"SubmissionFilterProgram";

        // Timeout value (in seconds) for the Submission Filter, default is 15s
        public const string SubmissionFilterWaitTimeEntry = @"SubmissionFilterTimeout";

        // Activation Filter Executable file name
        public const string ActivationFilterExeEntry = @"ActivationFilterProgram";

        // Timeout value (in seconds) for the Submission Filter, default is 15s
        public const string ActivationFilterWaitTimeEntry = @"ActivationFilterTimeout";

        // Time to live for completed job records in the db. Unit: days, default is 5 days
        public const string JobRemoveTimeEntry = @"TtlCompletedJobs";

        // maximum time the system reruns a job, default is 3
        public const string JobNumberOfRequeuesEntry = @"JobRetryCount";

        // Maximum time the system reruns a task, default is 3
        public const string TaskNumberOfRequeuesEntry = @"TaskRetryCount";

        // look ahead value for backfill
        public const string BackfillLookaheadEntry = @"BackfillLookAhead";

        public const string BackfillLoadPeriodEntry = @"BackfillLoadPeriod";

        // automatic grow is enabled
        public const string GrowEnabledEntry = @"AutomaticGrowthEnabled";

        // automatic grow is enabled
        public const string ShrinkEnabledEntry = @"AutomaticShrinkEnabled";

        // time interval (in seconds) to update perf counters
        public const string PerfCounterUpdateIntervalEntry = @"PerfCounterUpdateInterval";

        // Hour to run JobCleanUp (0-23), default is 2am everyday.
        public const string JobCleanUpHourEntry = @"JobCleanUpHour";
        public const string JobCleanDayOfWeekEntry = @"JobCleanUpDayOfWeek";

        public const string SchedulingModeEntry = @"SchedulingMode";

        public const string PreemptionBalancedModeEntry = @"PreemptionBalancedMode";

        public const string TaskCancelGracePeriodEntry = @"TaskCancelGracePeriod";

        public const string PriorityBiasEntry = @"PriorityBias";

        /// <summary>
        /// Once PriorityBiasLevel > 0, PriorityBias is disabled.
        /// </summary>
        public const string PriorityBiasLevelEntry = @"PriorityBiasLevel";

        public const string ReBalancingIntervalEntry = @"ReBalancingInterval";

        public const string DefaultHoldDurationEntry = @"DefaultHoldDuration";

        public const string TransactionSetPropsBatchSizeEntry = @"TransactionSetPropsBatchSize";

        public const string TransactionSetPropsBatchStartEntry = @"TransactionSetPropsBatchStart";

        public const string TransactionExtraActionBatchSizeEntry = @"TransactionExtraActionBatchSize";

        public const string TaskMonitorCommitBatchSizeEntry = @"TaskMonitorCommitBatchSize";

        public const string DeleteOldJobBatchSizeDefaultEntry = @"DeleteOldJobBatchSizeDefault";

        public const string ExcludedNodesLimitEntry = @"ExcludedNodesLimit";

        // Email notification settings
        public const string EmailNotificationEnabledEntry = @"EmailNotificationEnabled";
        public const string EmailCredentialUserNameEntry = @"EmailCredentialUsername";
        public const string EmailCredentialPasswordEntry = @"EmailCredentialPassword";
        public const string EmailSmtpServerEntry = @"EmailSmtpServer";
        public const string EmailFromAddressEntry = @"EmailFromAddress";
        public const string EmailUseSslEntry = @"EmailUseSsl";
        public const string EmailBurstFrequencyEntry = @"EmailBurstFrequency";
        public const string EmailBurstSizeEntry = @"EmailBurstSize";
        public const string EmailMaxQueueSizeEntry = @"EmailMaxQueueSize";

        public const string DisableCredentialReuseEntry = @"DisableCredentialReuse";

        public const string DisableResourceValidationEntry = @"DisableResourceValidation";

#if DEBUG
        public const string SqlTransactionFailurePercentageEntry = @"SqlTransactionFailurePercentage";
        public const string SqlReadFailurePercentageEntry = @"SqlReadFailurePercentageEntry";
        public const string EventListenerClosePercentageEntry = @"EventListenerClosePercentage";
        public const string ADFailurePercentageEntry = @"AdFailurePercentageEntry";
        public const string AzureBurstFailurePercentageEntry = @"AzureBurstFailurePercentageEntry";
#endif

        public const string HpcUsersSidsEntry = @"HpcUsersSids";

        public const string HpcAdminMirrorSidsEntry = @"HpcAdminMirrorSids";

        public const string HpcJobAdministratorsSidsEntry = @"HpcJobAdministratorsSids";

        public const string HpcSoftCardEntry = @"HpcSoftCard";

        public const string HpcSoftCardTemplateEntry = @"HpcSoftCardTemplate";

        public const string HpcSoftCardExpirationWarningEntry = @"SoftCardExpirationWarning";

        public const string SchedulerWebServicePortEntry = @"SchedulerWebServicePort";

        public const string SchedulerWebServiceEnabledEntry = @"SchedulerWebServiceEnabled";

        public const string SchedulerWebServiceThumbprintEntry = @"SchedulerWebServiceThumbprint";

        public const string SchedulerWebServiceAuthEntry = @"SchedulerWebServiceAuth";

        public const string EnablePoolsEntry = @"EnablePools";

        public const string SchedulerOnAzureEntry = @"SchedulerOnAzure";

        public const string GrowByPreemptionEntry = @"GrowByPreemptionEnabled";

        public const string TaskImmediatePreemptionEntry = @"TaskImmediatePreemptionEnabled";

        public const string NettcpOver443Entry = @"NettcpOver443";

        public const string AzureLogsToBlobPolicyEntry = "AzureLogsToBlob";

        public const string AzureLogsToBlobThrottleEntry = "AzureLogsToBlobThrottling";

        public const string AzureLogsToBlobIntervalEntry = "AzureLogsToBlobInterval";

        public const string SchedulerDeleteOldJobRetryIntervalEntry = @"SchedulerDeleteOldJobRetryInterval";
        public const string SchedulerDeleteOldJobsDefaultCommandTimeoutEntry = @"SchedulerDeleteOldJobsDefaultCommandTimeout";
        public const string SchedulerDeleteOldJobsTotalTimeoutEntry = @"SchedulerDeleteOldJobsTotalTimeout";
        public const string SchedulerDeleteOldJobsMaxBatchSizeEntry = @"SchedulerDeleteOldJobsMaxBatchSize";
        public const string SchedulerDeleteOldJobsMaxTimeoutEntry = @"SchedulerDeleteOldJobsMaxTimeout";

        public const string EnableGrowShrink = "EnableGrowShrink";

        // this is for HPC 2012 R2 Update 3 (V4SP5), it is replaced by TasksPerResourceUnit after HPC 2012 R2 Update 3
        public const string ParamSweepTasksPerCore = "ParamSweepTasksPerCore";

        // this is added after HPC 2012 R2 Update 3, it means how many tasks will grow one resource unit(based on job resource unit type, core, socket or node)
        public const string TasksPerResourceUnit = "TasksPerResourceUnit";

        public const string GrowThreshold = "GrowThreshold";

        public const string GrowInterval = "GrowInterval";

        public const string GrowTimeout = "GrowTimeout";

        public const string ShrinkInterval = "ShrinkInterval";

        public const string ShrinkIdleTimes = "ShrinkIdleTimes";

        public const string ExtraNodesGrowRatio = "ExtraNodesGrowRatio";

        public const string GrowByMin = "GrowByMin";

        public const string SoaJobGrowThreshold = "SoaJobGrowThreshold";

        public const string SoaRequestsPerCore = "SoaRequestsPerCore";

        public const string ExcludeNodeGroups = "ExcludeNodeGroups";

        public const string GetAzureBatchTaskOutputEntry = @"GetAzureBatchTaskOutput";

        public const string ScanAzureBatchTaskWithoutFilterIntervalEntry = @"ScanAzureBatchTaskWithoutFilterInterval";

        public const char SeparatorForCloudServiceAndiLBIp = '@';
    }    
}