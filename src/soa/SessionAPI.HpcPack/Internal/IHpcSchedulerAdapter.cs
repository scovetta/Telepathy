//------------------------------------------------------------------------------
// <copyright file="IHpcSchedulerAdapter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Service contract of scheduler adapter
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Properties;

    /// <summary>
    /// Service contract of scheduler adapter
    /// </summary>
    [ServiceContract(CallbackContract = typeof(ISchedulerNotify), Namespace = "http://hpc.microsoft.com")]
    public interface IHpcSchedulerAdapter
    {
        /// <summary>
        /// Start to subscribe the job and task event
        /// </summary>
        /// <param name="jobid">indicating the job id</param>
        /// <param name="autoMax">indicating the auto max property of the job</param>
        /// <param name="autoMin">indicating the auto min property of the job</param>
        /// <returns>returns the current job state</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<Tuple<JobState, int, int>> RegisterJob(int jobid);

        /// <summary>
        /// Finish a job
        /// </summary>
        /// <param name="jobid">
        ///   <para />
        /// </param>
        /// <param name="reason">
        ///   <para />
        /// </param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task FinishJob(int jobid, string reason);

        /// <summary>
        /// Fail a job
        /// </summary>
        /// <param name="jobid">
        ///   <para />
        /// </param>
        /// <param name="reason">
        ///   <para />
        /// </param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task FailJob(int jobid, string reason);

        /// <summary>
        /// Re-queue or fail a job
        /// </summary>
        /// <param name="jobid">
        ///   <para />
        /// </param>
        /// <param name="reason">
        ///   <para />
        /// </param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task RequeueOrFailJob(int jobid, string reason);

        /// <summary>
        /// Exclude a node from a job
        /// </summary>
        /// <param name="jobid">
        ///   <para />
        /// </param>
        /// <param name="nodeName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> ExcludeNode(int jobid, string nodeName);

        /// <summary>
        /// Update properties of broker info
        /// </summary>
        /// <param name="jobid">
        ///   <para />
        /// </param>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> UpdateBrokerInfo(int jobid, Dictionary<string, object> properties);

        /// <summary>
        /// Get the task error code
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        /// <param name="globalTaskId">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<int?> GetTaskErrorCode(int jobId, int globalTaskId);

        /// <summary>
        /// Get the graceful preemption information.
        /// </summary>
        /// <param name="jobId">the job id</param>
        /// <returns>returns a value indicating whether the operation succeeded</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<(bool succeed, BalanceInfo balanceInfo, List<int> taskIds, List<int> runningTaskIds)> GetGracefulPreemptionInfo(int jobId); // TODO: Change to value tuple here

        /// <summary>
        /// Finish a task
        /// </summary>
        /// <param name="jobid">
        ///   <para />
        /// </param>
        /// <param name="taskUniqueId">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> FinishTask(int jobId, int taskUniqueId);
    }
}
