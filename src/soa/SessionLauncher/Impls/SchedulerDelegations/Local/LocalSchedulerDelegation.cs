namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.SchedulerDelegations.Local
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.Local;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, MaxItemsInObjectGraph = int.MaxValue)]
    public class LocalSchedulerDelegation : ISchedulerAdapter
    {
        internal LocalSchedulerDelegation(LocalSessionLauncher instance)
        {
            this.sessionLauncher = instance;
        }

        private LocalSessionLauncher sessionLauncher;

        public async Task<bool> UpdateBrokerInfoAsync(int sessionId, Dictionary<string, object> properties)
        {
            Trace.TraceWarning($"Ignored call to {nameof(UpdateBrokerInfoAsync)}");
            return true;
        }

        public async Task<(bool succeed, BalanceInfo balanceInfo, List<string> taskIds, List<string> runningTaskIds)> GetGracefulPreemptionInfoAsync(int sessionId)
        {
            Trace.TraceWarning($"Ignored call to {nameof(GetGracefulPreemptionInfoAsync)}");

            return (false, null, null, null);
        }

        public async Task<bool> FinishTaskAsync(int jobId, string taskUniqueId)
        {
            Trace.TraceWarning($"Ignored call to {nameof(FinishTaskAsync)}");
            return true;
        }

        public async Task<bool> ExcludeNodeAsync(int jobid, string nodeName)
        {
            Trace.TraceWarning($"Ignored call to {nameof(ExcludeNodeAsync)}");

            return true;
        }

        public async Task RequeueOrFailJobAsync(int sessionId, string reason)
        {
            Trace.TraceWarning($"Ignored call to {nameof(RequeueOrFailJobAsync)}");
        }

        public async Task FailJobAsync(int sessionId, string reason) => await this.sessionLauncher.TerminateV5Async(sessionId);

        public async Task FinishJobAsync(int sessionId, string reason) => await this.sessionLauncher.TerminateV5Async(sessionId);

        public async Task<(JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(int jobid)
        {
            Trace.TraceWarning($"Ignored call to {nameof(RegisterJobAsync)}");
            return (JobState.Running, int.MaxValue, 0);
        }

        public Task<int?> GetTaskErrorCode(int jobId, string globalTaskId)
        {
            throw new System.NotImplementedException();
        }
    }
}