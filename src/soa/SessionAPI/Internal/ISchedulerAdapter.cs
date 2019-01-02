namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Data;

    // TODO: this class is subject to change
    public interface ISchedulerAdapter
    {
        Task<bool> UpdateBrokerInfoAsync(int sessionId, Dictionary<string, object> properties);

        Task<(bool succeed, BalanceInfo balanceInfo, List<int> taskIds, List<int> runningTaskIds)> GetGracefulPreemptionInfo(int sessionId);

        // TODO: this sig need to be changed
        Task<bool> FinishTask(int jobId, int taskUniqueId);

        Task<bool> ExcludeNodeAsync(int jobid, string nodeName);

        Task RequeueOrFailJobAsync(int sessionId, string reason);

        Task FailJobAsync(int sessionId, string reason);

        Task FinishJobAsync(int sessionId, string reason);

        Task<(JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(int jobid);
    }
}
