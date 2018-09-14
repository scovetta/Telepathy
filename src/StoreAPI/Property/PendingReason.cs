using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{

    /// <summary>
    ///   <para>Defines the possible reasons why the job or task has yet to start.</para>
    /// </summary>
    [Serializable]
    public class PendingReason
    {
        /// <summary>
        ///   <para>Defines the possible reasons why the job or task has yet to start.</para>
        /// </summary>
        [Serializable]
        public enum ReasonCode
        {
            /// <summary>
            ///   <para>The job is not pending.</para>
            /// </summary>
            None,
            /// <summary>
            ///   <para>There are not enough resources to run the job at this time.</para>
            /// </summary>
            NotEnoughResource,
            /// <summary>
            ///   <para>A higher priority job is running.</para>
            /// </summary>
            HigherPriorityJob,
            /// <summary>
            ///   <para>A job with the same priority is currently running.</para>
            /// </summary>
            JobSubmittedEarlier,
            /// <summary>
            ///   <para>A task on which this task is dependent is still running.</para>
            /// </summary>
            DependencyNotDone,
            /// <summary>
            ///   <para>The nodes required to run the job are not available.</para>
            /// </summary>
            RequiredNodesNotAvailable,
            /// <summary>
            ///   <para>The activation filter which determines if the job can run failed.</para>
            /// </summary>
            AcitivationFilter,
            /// <summary>
            ///   <para>The node is not configured to run broker jobs.</para>
            /// </summary>
            NoBrokerNode,
            /// <summary>
            ///   <para>An activation filter is delaying the job from running. Check again later to determine if the activation 
            /// filter stopped blocking the job from running. This value was 
            /// introduced in Windows® HPC Server 2008 R2 and is not supported in previous versions.</para> 
            /// </summary>
            HeldByActivationFilter,
            /// <summary>
            ///   <para>The job is delayed from running because an administrator used the 
            /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SetHoldUntil(System.DateTime)" /> method to set the 
            /// 
            /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.HoldUntil" /> property for the job. This value was introduced in Windows HPC Server 2008 R2 and is not available in previous versions.</para> 
            /// </summary>
            HeldByAdmin,
            /// <summary>
            ///   <para>The user held the job. This value was introduced in HPC Pack 2012 and is not supported in previous versions.</para>
            /// </summary>
            HeldByUser,
            /// <summary>
            ///   <para>The parent job was canceled, or the parent job failed. 
            /// This value was introduced in HPC Pack 2012 and is not supported in previous versions.</para>
            /// </summary>
            ParentJobsCanceledOrFailed,
            /// <summary>
            ///   <para>The parent job has not finished. This value was introduced in HPC Pack 2012 and is not supported in previous versions.</para>
            /// </summary>
            ParentJobsNotfinished,
            /// <summary>
            ///   <para>There is no available resource in requested node group. This value was introduced in HPC Pack 2016 and is not supported in previous versions.</para>
            /// </summary>
            NoAvailableResourceInRequestedNodeGroup,
            /// <summary>
            ///   <para>Only core type are allowed in the fast balanced mode for now, this constraint could be removed.</para>
            /// </summary>
            UnitTypeNotAllowedInFastBalance,
            /// <summary>
            ///   <para>Job min can only set to 1 in fast balanced mode for now, this constraint could be removed.</para>
            /// </summary>
            MinGreaterThanOneInFastBalance,
            /// <summary>
            ///   <para>Pool shouldn't be enabled in fast balanced mode for now, this constraint could be removed.</para>
            /// </summary>
            PoolEnabledInFastBalance,
            /// <summary>
            ///   <para>Job shouldn't have exclusive setting in fast balanced mode for now, this constraint could be removed.</para>
            /// </summary>
            JobExclusiveInFastBalance,
            /// <summary>
            ///   <para>Node operation shouldn't be in uniform in fast balanced mode for now, this constraint could be removed.</para>
            /// </summary>
            NodeUniformInFastBalance,
        }

        /// <summary>
        ///   <para>Writes a string that represents the pending reason.</para>
        /// </summary>
        /// <param name="code">
        ///   <para>The reason that the job or task is pending.</para>
        /// </param>
        /// <returns>
        ///   <para>A string that represents the pending reason.</para>
        /// </returns>
        public static string ToString(ReasonCode code)
        {
            switch (code)
            {
                case ReasonCode.None:
                    return SR.PendingReason_None;
                case ReasonCode.NotEnoughResource:
                    return SR.PendingReason_NotEnoughResource;
                case ReasonCode.HigherPriorityJob:
                    return SR.PendingReason_HigherPriorityJob;
                case ReasonCode.JobSubmittedEarlier:
                    return SR.PendingReason_JobSubmittedEarlier;
                case ReasonCode.DependencyNotDone:
                    return SR.PendingReason_DependencyNotDone;
                case ReasonCode.RequiredNodesNotAvailable:
                    return SR.PendingReason_RequiredNodesNotAvailable;
                case ReasonCode.AcitivationFilter:
                    return SR.PendingReason_ActivitionFilter;
                case ReasonCode.NoBrokerNode:
                    return SR.PendingReason_NoBrokerNode;
                case ReasonCode.HeldByActivationFilter:
                    return SR.PendingReason_HeldByActivationFilter;
                case ReasonCode.HeldByAdmin:
                    return SR.PendingReason_HeldByAdmin;
                case ReasonCode.HeldByUser:
                    return SR.PendingReason_HeldByUser;
                case ReasonCode.ParentJobsCanceledOrFailed:
                    return SR.PendingReason_ParentJobsCanceledOrFailed;
                case ReasonCode.ParentJobsNotfinished:
                    return SR.PendingReason_ParentJobsNotfinished;
                case ReasonCode.UnitTypeNotAllowedInFastBalance:
                    return SR.PendingReason_UnitTypeNotAllowedInFastBalance;
                case ReasonCode.MinGreaterThanOneInFastBalance:
                    return SR.PendingReason_MinGreaterThanOneInFastBalance;
                case ReasonCode.PoolEnabledInFastBalance:
                    return SR.PendingReason_PoolEnabledInFastBalance;
                case ReasonCode.JobExclusiveInFastBalance:
                    return SR.PendingReason_JobExclusiveInFastBalance;
                case ReasonCode.NodeUniformInFastBalance:
                    return SR.PendingReason_NodeUniformInFastBalance;
                // If the reason code is not recognized, then return SR.PendingReason_NotEnoughResource by default
                default:
                    return SR.PendingReason_NotEnoughResource;
            }
        }
    }
}
