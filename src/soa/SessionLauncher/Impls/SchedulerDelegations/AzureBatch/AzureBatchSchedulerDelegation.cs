namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.SchedulerDelegations.AzureBatch
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Common;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch;

    using JobState = Microsoft.Hpc.Scheduler.Session.Data.JobState;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, MaxItemsInObjectGraph = int.MaxValue)]
    internal class AzureBatchSchedulerDelegation : ISchedulerAdapter
    {
        internal AzureBatchSchedulerDelegation(AzureBatchSessionLauncher instance)
        {
            this.sessionLauncher = instance;
        }

        private AzureBatchSessionLauncher sessionLauncher;

        public async Task<bool> UpdateBrokerInfoAsync(int sessionId, Dictionary<string, object> properties)
        {
            try
            {
                using (var batchClient = AzureBatchConfiguration.GetBatchClient())
                {
                    TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[AzureBatchSchedulerDelegation] UpdateBrokerInfo...");
                    StringBuilder sb = new System.Text.StringBuilder();
                    foreach (KeyValuePair<string, object> property in properties)
                    {
                        sb.AppendLine($"Property = {property.Key}\tValue = {property.Value}");
                    }

                    TraceHelper.TraceEvent(sessionId, TraceEventType.Verbose, "[AzureBatchSchedulerDelegation] Properties detail:\n{0}", sb.ToString());

                    var sessionJob = await batchClient.JobOperations.GetJobAsync(AzureBatchSessionJobIdConverter.ConvertToAzureBatchJobId(sessionId)).ConfigureAwait(false);
                    await this.UpdateSoaRelatedPropertiesAsync(sessionJob, properties).ConfigureAwait(false);
                }

                return true;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, ex.ToString());
            }

            return false;
        }

        public async Task<(bool succeed, BalanceInfo balanceInfo, List<int> taskIds, List<int> runningTaskIds)> GetGracefulPreemptionInfoAsync(int sessionId)
        {
            Trace.TraceWarning($"Ignored call to {nameof(GetGracefulPreemptionInfoAsync)}");

            return (false, null, null, null);
        }

        public async Task<bool> FinishTaskAsync(int jobId, int taskUniqueId)
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

        public Task<int?> GetTaskErrorCode(int jobId, int globalTaskId)
        {
            throw new NotImplementedException();
        }

        private async Task UpdateSoaRelatedPropertiesAsync(CloudJob sessionJob, Dictionary<string, object> properties)
        {
            try
            {
                Dictionary<string, string> metaDic = new Dictionary<string, string>();
                var metadata = sessionJob.Metadata;
                if (metadata != null)
                {
                    foreach (var item in metadata)
                    {
                        metaDic[item.Name] = item.Value;
                    }
                }

                foreach (KeyValuePair<string, object> pair in properties)
                {
                    TraceHelper.TraceEvent(TraceEventType.Verbose, "[AzureBatchSchedulerDelegation] .UpdateSoaRelatedProperties: Job custom property {0}={1}", pair.Key, pair.Value);

                    if (pair.Value != null)
                    {
                        if (!SchedulerDelegationCommon.PropToEnvMapping.TryGetValue(pair.Key, out var envName))
                        {
                            envName = pair.Key;
                        }

                        if (pair.Value != null)
                        {
                            metaDic[envName] = pair.Value.ToString();

                            TraceHelper.TraceEvent(
                                TraceEventType.Verbose,
                                "[AzureBatchSchedulerDelegation] .UpdateSoaRelatedProperties: Set job custom property {0}={1}",
                                envName,
                                pair.Value);
                        }
                    }
                }

                sessionJob.Metadata = metaDic.Select(p => new MetadataItem(p.Key, p.Value)).ToList();

                int retry = 3;
                while (retry > 0)
                {
                    try
                    {
                        await sessionJob.CommitChangesAsync().ConfigureAwait(false);
                        TraceHelper.TraceEvent(TraceEventType.Information, "[AzureBatchSchedulerDelegation] .UpdateSoaRelatedProperties: Commit service job.");
                    }
                    catch (BatchException e)
                    {
                        var reqInfo = e.RequestInformation;

                        if (reqInfo != null && reqInfo.HttpStatusCode == HttpStatusCode.Conflict)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Warning, "[AzureBatchSchedulerDelegation] Conflict when commit service job property change: {0}", reqInfo.HttpStatusMessage);
                            return;
                        }
                        else
                        {
                            TraceHelper.TraceEvent(TraceEventType.Warning, "[AzureBatchSchedulerDelegation] Conflict when commit service job property change: {0}", e.ToString());
                            throw;
                        }
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[AzureBatchSchedulerDelegation] Failed to commit service job property change: {0}\nRetryCount = {1}", e, retry);
                        await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                    }
                    finally
                    {
                        retry--;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Warning, "[AzureBatchSchedulerDelegation] Failed to update soa properties, {0}", ex.ToString());
                throw;
            }
        }
    }
}