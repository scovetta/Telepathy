namespace Microsoft.Hpc.ServiceBroker.Common.SchedulerAdapter
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.BackEnd;

    public class SchedulerAdapterClient : ClientBase<ISchedulerAdapter>, ISchedulerAdapter
    {
        /// <summary>
        /// Stores the unique id
        /// </summary>
        private static int uniqueIdx;

        private string[] predefinedSvcHost = new string[0];

        private DispatcherManager dispatcherManager = null;

        public SchedulerAdapterClient(Binding binding, EndpointAddress address) : this(binding, address, null, null)
        {
        }

        internal SchedulerAdapterClient(Binding binding, EndpointAddress address, string[] predefinedSvcHost, DispatcherManager dispatcherManager) : base(binding, address)
        {
            this.predefinedSvcHost = predefinedSvcHost;
            this.dispatcherManager = dispatcherManager;
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

        public async Task<(JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(int jobid)
        {
            // TODO: this is not proper place to put dispatcher creating logic. Remove this.

            int autoMax = int.MaxValue;
            int autoMin = 0;
            return (Scheduler.Session.Data.JobState.Running, autoMax, autoMin);
            return await this.Channel.RegisterJobAsync(jobid);
        }

        // TODO: remove globalTaskId
        public async Task<int?> GetTaskErrorCode(int jobId, int globalTaskId)
        {
            return await this.Channel.GetTaskErrorCode(jobId, globalTaskId);
        }
    }
}