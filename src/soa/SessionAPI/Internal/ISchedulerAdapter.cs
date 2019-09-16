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
        Task<bool> UpdateBrokerInfoAsync(int sessionId, Dictionary<string, object> properties);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<(bool succeed, BalanceInfo balanceInfo, List<string> taskIds, List<string> runningTaskIds)> GetGracefulPreemptionInfoAsync(int sessionId);

        // TODO: this sig need to be changed, `taskUniqueId` should be removed
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> FinishTaskAsync(int jobId, string taskUniqueId);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> ExcludeNodeAsync(int jobid, string nodeName);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task RequeueOrFailJobAsync(int sessionId, string reason);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task FailJobAsync(int sessionId, string reason);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task FinishJobAsync(int sessionId, string reason);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<(JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(int jobid);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<int?> GetTaskErrorCode(int jobId, string globalTaskId);
    }
}
