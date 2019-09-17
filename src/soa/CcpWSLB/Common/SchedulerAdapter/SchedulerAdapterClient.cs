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

    public class SchedulerAdapterClient : DuplexClientBase<ISchedulerAdapter>, ISchedulerAdapter
    {
        /// <summary>
        /// Stores the unique id
        /// </summary>
        private static int uniqueIdx;

        private string[] predefinedSvcHost = new string[0];

        private DispatcherManager dispatcherManager = null;

        public SchedulerAdapterClient(Binding binding, EndpointAddress address, InstanceContext context) : this(binding, address, null, null, context)
        {
        }

        internal SchedulerAdapterClient(Binding binding, EndpointAddress address, string[] predefinedSvcHost, DispatcherManager dispatcherManager, InstanceContext instanceContext) : base(instanceContext, binding, address)
        {
            this.predefinedSvcHost = predefinedSvcHost;
            this.dispatcherManager = dispatcherManager;
        }

        public async Task<bool> UpdateBrokerInfoAsync(string sessionId, Dictionary<string, object> properties) => await this.Channel.UpdateBrokerInfoAsync(sessionId, properties);

        public async Task<(bool succeed, BalanceInfo balanceInfo, List<string> taskIds, List<string> runningTaskIds)> GetGracefulPreemptionInfoAsync(string sessionId) =>
            await this.Channel.GetGracefulPreemptionInfoAsync(sessionId);

        public async Task<bool> FinishTaskAsync(string jobId, string taskUniqueId) => await this.Channel.FinishTaskAsync(jobId, taskUniqueId);

        public async Task<bool> ExcludeNodeAsync(string jobid, string nodeName) => await this.Channel.ExcludeNodeAsync(jobid, nodeName);

        public async Task RequeueOrFailJobAsync(string sessionId, string reason)
        {
            await this.Channel.RequeueOrFailJobAsync(sessionId, reason);
        }

        public async Task FailJobAsync(string sessionId, string reason)
        {
            await this.Channel.FailJobAsync(sessionId, reason);
        }

        public async Task FinishJobAsync(string sessionId, string reason)
        {
            await this.Channel.FinishJobAsync(sessionId, reason);
        }

        public async Task<(JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(string jobid)
        {
            // TODO: this is not proper place to put dispatcher creating logic. Remove this.
            return await this.Channel.RegisterJobAsync(jobid);
        }

        // TODO: remove globalTaskId
        public async Task<int?> GetTaskErrorCode(string jobId, string globalTaskId)
        {
            return await this.Channel.GetTaskErrorCode(jobId, globalTaskId);
        }
    }
}