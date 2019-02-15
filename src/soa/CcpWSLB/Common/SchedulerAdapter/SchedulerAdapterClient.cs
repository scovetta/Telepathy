namespace Microsoft.Hpc.ServiceBroker.Common.SchedulerAdapter
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    internal class SchedulerAdapterClient : ClientBase<ISchedulerAdapter>, ISchedulerAdapter
    {
        public SchedulerAdapterClient(Binding binding, EndpointAddress address) : base(binding, address)
        {
        }

        public async Task<bool> UpdateBrokerInfoAsync(int sessionId, Dictionary<string, object> properties) => await this.Channel.UpdateBrokerInfoAsync(sessionId, properties);

        public async Task<(bool succeed, BalanceInfo balanceInfo, List<int> taskIds, List<int> runningTaskIds)> GetGracefulPreemptionInfoAsync(int sessionId) =>
            await this.Channel.GetGracefulPreemptionInfoAsync(sessionId);

        public async Task<bool> FinishTaskAsync(int jobId, int taskUniqueId) => await this.Channel.FinishTaskAsync(jobId, taskUniqueId);

        public async Task<bool> ExcludeNodeAsync(int jobid, string nodeName) => await this.Channel.ExcludeNodeAsync(jobid, nodeName);

        public async Task RequeueOrFailJobAsync(int sessionId, string reason)
        {
            await this.Channel.RequeueOrFailJobAsync(sessionId, reason);
        }

        public async Task FailJobAsync(int sessionId, string reason)
        {
            await this.Channel.FailJobAsync(sessionId, reason);
        }

        public async Task FinishJobAsync(int sessionId, string reason)
        {
            await this.Channel.FinishJobAsync(sessionId, reason);
        }

        public async Task<(JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(int jobid) => await this.Channel.RegisterJobAsync(jobid);
    }
}