// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    // TODO: this class is subject to change
    [ServiceContract(CallbackContract = typeof(ISchedulerNotify), Namespace = "http://hpc.microsoft.com")]
    public interface ISchedulerAdapter
    {
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> UpdateBrokerInfoAsync(string sessionId, Dictionary<string, object> properties);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<(bool succeed, BalanceInfo balanceInfo, List<string> taskIds, List<string> runningTaskIds)> GetGracefulPreemptionInfoAsync(string sessionId);

        // TODO: this sig need to be changed, `taskUniqueId` should be removed
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> FinishTaskAsync(string jobId, string taskUniqueId);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> ExcludeNodeAsync(string jobid, string nodeName);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task RequeueOrFailJobAsync(string sessionId, string reason);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task FailJobAsync(string sessionId, string reason);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task FinishJobAsync(string sessionId, string reason);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<(JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(string jobid);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<int?> GetTaskErrorCode(string jobId, string globalTaskId);
    }
}
