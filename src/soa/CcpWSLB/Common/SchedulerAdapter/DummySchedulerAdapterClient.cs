namespace Microsoft.Hpc.ServiceBroker.Common
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.BackEnd;

    internal partial class SchedulerAdapterClientFactory
    {
        /// <summary>
        /// Dummy scheduler adapter client
        /// </summary>
        private class DummySchedulerAdapterClient : ISchedulerAdapter
        {
            /// <summary>
            /// Stores the unique id
            /// </summary>
            private static int uniqueId;

            /// <summary>
            /// Stores the callback instance
            /// </summary>
            private DispatcherManager dispatcherManager;

            /// <summary>
            /// Stores the epr list
            /// </summary>
            private string[] eprList;

            /// <summary>
            /// Initializes a new instance of the DummySchedulerAdapterClient class
            /// </summary>
            /// <param name="eprList">indicating the epr list</param>
            /// <param name="dispatcherManager">indicating the dispatcher manager instance</param>
            public DummySchedulerAdapterClient(string[] eprList, DispatcherManager dispatcherManager)
            {
                this.dispatcherManager = dispatcherManager;
                this.eprList = eprList;
            }

            public bool IsDiagTraceEnabled(int sessionId)
            {
                return false;
            }

            /*
            /// <summary>
            /// Start to subscribe the job and task event
            /// </summary>
            /// <param name="jobid">indicating the job id</param>
            /// <param name="autoMax">indicating the auto max property of the job</param>
            /// <param name="autoMin">indicating the auto min property of the job</param>
            public JobState RegisterJob(int jobid, out int autoMax, out int autoMin)
            {
                autoMax = int.MaxValue;
                autoMin = 0;
                foreach (string epr in this.eprList)
                {
                    DispatcherInfo info = new EprDispatcherInfo(epr, 1, Interlocked.Increment(ref uniqueId));
                    this.dispatcherManager.NewDispatcherAsync(info).GetAwaiter().GetResult();
                }

                return JobState.Running;
            }

            /// <summary>
            /// Finish a service job with the reason
            /// </summary>
            /// <param name="jobid">the job id</param>
            /// <param name="reason">the reason string</param>
            public void FinishJob(int jobid, string reason)
            {
                // Do nothing
            }

            /// <summary>
            /// Fail a service job with the reason
            /// </summary>
            /// <param name="jobid">the job id</param>
            /// <param name="reason">the reason string</param>
            public void FailJob(int jobid, string reason)
            {
                // Do nothing
            }

            /// <summary>
            /// Requeue or fail a service job with the reason
            /// </summary>
            /// <param name="jobid">the job id</param>
            /// <param name="reason">the reason string</param>
            public void RequeueOrFailJob(int jobid, string reason)
            {
                // Do nothing
            }

            /// <summary>
            /// Add a node to job's exclude node list
            /// </summary>
            /// <param name="jobid">the job id</param>
            /// <param name="nodeName">name of the node to be excluded</param>
            /// <returns>true if the node is successfully blacklisted, or the job is failed. false otherwise</returns>
            public bool ExcludeNode(int jobid, string nodeName)
            {
                // Do nothing
                return true;
            }
            

            /// <summary>
            /// Update the job's properties
            /// </summary>
            /// <param name="jobid">the job id</param>
            /// <param name="properties">the properties table</param>
            /// <returns>returns a value indicating whether the operation succeeded</returns>
            public bool UpdateBrokerInfo(int jobid, System.Collections.Generic.Dictionary<string, object> properties)
            {
                // Do nothing
                return true;
            }
            */

            /// <summary>
            /// Update the job's properties
            /// </summary>
            /// <param name="jobid">the job id</param>
            /// <param name="properties">the properties table</param>
            /// <returns>returns a value indicating whether the operation succeeded</returns>
            public Task<bool> UpdateBrokerInfoAsync(int jobid, System.Collections.Generic.Dictionary<string, object> properties)
            {
                // Do nothing
                return Task<bool>.FromResult(true);
            }

            /*
            /// <summary>
            /// Get the error code property of the specified task.
            /// </summary>
            /// <param name="jobId">job id</param>
            /// <param name="globalTaskId">unique task id</param>
            /// <returns>return error code value if it exists, otherwise return null</returns>
            public int? GetTaskErrorCode(int jobId, int globalTaskId)
            {
                return null;
            }

            /// <summary>
            /// Begin-method for the async mode of the GetTaskErrorCode.
            /// It is a dummy method here just for interface implementation.
            /// </summary>
            public IAsyncResult BeginGetTaskErrorCode(int jobId, int globalTaskId, AsyncCallback callback, object state)
            {
                return null;
            }

            /// <summary>
            /// End-method for the async mode of the GetTaskErrorCode.
            /// It is a dummy method here just for interface implementation.
            /// </summary>
            public int? EndGetTaskErrorCode(IAsyncResult result)
            {
                return null;
            }

            public bool GetGracefulPreemptionInfo(int jobId, out BalanceInfo balanceInfo, out List<int> taskIds, out List<int> runningTaskIds)
            {
                taskIds = null;
                runningTaskIds = null;
                balanceInfo = new BalanceInfo(int.MaxValue);
                return true;
            }
            */

            async Task<(Microsoft.Hpc.Scheduler.Session.Data.JobState jobState, int autoMax, int autoMin)> ISchedulerAdapter.RegisterJobAsync(int jobid)
            {
                int autoMax = int.MaxValue;
                int autoMin = 0;
                // foreach (string epr in this.eprList)
                // {
                //     BrokerTracing.TraceInfo($"Creating Dispatcher for predefined Service Host {epr}");
                //     DispatcherInfo info = new EprDispatcherInfo(epr, 1, Interlocked.Increment(ref uniqueId));
                //     await this.dispatcherManager.NewDispatcherAsync(info).ConfigureAwait(false);
                // }
                // 
                return (Scheduler.Session.Data.JobState.Running, autoMax, autoMin);
            }

            async Task ISchedulerAdapter.FinishJobAsync(int jobid, string reason)
            {
                await Task.CompletedTask;
            }

            async Task ISchedulerAdapter.FailJobAsync(int jobid, string reason)
            {
                await Task.CompletedTask;
            }

            async Task ISchedulerAdapter.RequeueOrFailJobAsync(int jobid, string reason)
            {
                await Task.CompletedTask;
            }

            async Task<bool> ISchedulerAdapter.ExcludeNodeAsync(int jobid, string nodeName)
            {
                return await Task.FromResult(true);
            }

            async Task<bool> ISchedulerAdapter.UpdateBrokerInfoAsync(int jobid, Dictionary<string, object> properties)
            {
                return await Task.FromResult(true);
            }
            
            /*
            async Task<int?> ISchedulerAdapter.GetTaskErrorCode(int jobId, int globalTaskId)
            {
                return await Task.FromResult<int?>(null);
            }
            */


            async Task<(bool succeed, BalanceInfo balanceInfo, List<int> taskIds, List<int> runningTaskIds)> ISchedulerAdapter.GetGracefulPreemptionInfoAsync(int jobId)
            {
                return (true, new BalanceInfo(int.MaxValue), null, null); 
            }

            public Task<bool> FinishTaskAsync(int jobId, int taskUniqueId)
            {
                return Task.FromResult(true);
            }
        }
    }
}