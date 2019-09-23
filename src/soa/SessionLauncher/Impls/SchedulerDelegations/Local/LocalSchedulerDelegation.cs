// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.SchedulerDelegations.Local
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.Local;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Data;
    using Microsoft.Telepathy.Session.Internal;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, MaxItemsInObjectGraph = int.MaxValue)]
    public class LocalSchedulerDelegation : ISchedulerAdapter
    {
        internal LocalSchedulerDelegation(LocalSessionLauncher instance)
        {
            this.sessionLauncher = instance;
        }

        private LocalSessionLauncher sessionLauncher;

        public async Task<bool> UpdateBrokerInfoAsync(string sessionId, Dictionary<string, object> properties)
        {
            Trace.TraceWarning($"Ignored call to {nameof(this.UpdateBrokerInfoAsync)}");
            return true;
        }

        public async Task<(bool succeed, BalanceInfo balanceInfo, List<string> taskIds, List<string> runningTaskIds)> GetGracefulPreemptionInfoAsync(string sessionId)
        {
            Trace.TraceWarning($"Ignored call to {nameof(this.GetGracefulPreemptionInfoAsync)}");

            return (false, null, null, null);
        }

        public async Task<bool> FinishTaskAsync(string jobId, string taskUniqueId)
        {
            Trace.TraceWarning($"Ignored call to {nameof(this.FinishTaskAsync)}");
            return true;
        }

        public async Task<bool> ExcludeNodeAsync(string jobid, string nodeName)
        {
            Trace.TraceWarning($"Ignored call to {nameof(this.ExcludeNodeAsync)}");

            return true;
        }

        public async Task RequeueOrFailJobAsync(string sessionId, string reason)
        {
            Trace.TraceWarning($"Ignored call to {nameof(this.RequeueOrFailJobAsync)}");
        }

        public async Task FailJobAsync(string sessionId, string reason) => await this.sessionLauncher.TerminateAsync(sessionId);

        public async Task FinishJobAsync(string sessionId, string reason) => await this.sessionLauncher.TerminateAsync(sessionId);

        public async Task<(JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(string jobid)
        {
            Trace.TraceWarning($"Ignored call to {nameof(this.RegisterJobAsync)}");
            return (JobState.Running, int.MaxValue, 0);
        }

        public Task<int?> GetTaskErrorCode(string jobId, string globalTaskId)
        {
            throw new System.NotImplementedException();
        }
    }
}