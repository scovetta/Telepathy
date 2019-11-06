// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.SchedulerDelegations.AzureBatch
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
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.DataMapping.AzureBatch;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.JobMonitorEntry.AzureBatch;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.AzureBatch;
    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Internal;

    using JobState = Microsoft.Telepathy.Session.Data.JobState;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, MaxItemsInObjectGraph = int.MaxValue)]
    internal class AzureBatchSchedulerDelegation : ISchedulerAdapter
    {
        internal AzureBatchSchedulerDelegation(AzureBatchSessionLauncher instance)
        {
            this.sessionLauncher = instance;
            Trace.TraceInformation("[AzureBatchSchedulerDelegation] Successfully initialized scheduler adapter.");
        }

        private AzureBatchSessionLauncher sessionLauncher;

        /// <summary>
        /// The dictionary to store the monitors: (JobId, JobMonitorEntry)
        /// </summary>
        private Dictionary<string, AzureBatchJobMonitorEntry> JobMonitors = new Dictionary<string, AzureBatchJobMonitorEntry>();

        public async Task<bool> UpdateBrokerInfoAsync(string sessionId, Dictionary<string, object> properties)
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
            catch (BatchException ex)
            {
                if (ex.RequestInformation != null && ex.RequestInformation.HttpStatusCode != null)
                {
                    if (ex.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
                    {
                        TraceHelper.TraceEvent(sessionId, TraceEventType.Warning, "[AzureBatchSchedulerDelegation] Can't update job properties because it can't be found in AzureBatch.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, ex.ToString());
            }

            return false;
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

        /// <summary>
        /// Start to subscribe the job and task event
        /// </summary>
        /// <param name="jobid">indicating the job id</param>
        /// <param name="autoMax">indicating the auto max property of the job</param>
        /// <param name="autoMin">indicating the auto min property of the job</param>
        public async Task<(JobState jobState, int autoMax, int autoMin)> RegisterJobAsync(string jobid)
        {
            Trace.TraceInformation($"[AzureBatchSchedulerDelegation] Begin: RegisterJob, job id is {jobid}...");
            //CheckBrokerAccess(jobid);

            int autoMax = 0, autoMin = 0;
            CloudJob batchJob;
            try
            {
                AzureBatchJobMonitorEntry jobMonitorEntry;
                lock (this.JobMonitors)
                {
                    if (!this.JobMonitors.TryGetValue(jobid, out jobMonitorEntry))
                    {
                        jobMonitorEntry = new AzureBatchJobMonitorEntry(jobid);
                        jobMonitorEntry.Exit += new EventHandler(this.JobMonitorEntry_Exit);
                    }
                }

                batchJob = await jobMonitorEntry.StartAsync(System.ServiceModel.OperationContext.Current);

                // Bug 18050: Only add/update the instance if it succeeded to
                // open the job.
                lock (this.JobMonitors)
                {
                    this.JobMonitors[jobid] = jobMonitorEntry;
                }

                autoMin = jobMonitorEntry.MinUnits;
                autoMax = jobMonitorEntry.MaxUnits;
            }
            catch (Exception e)
            {
                Trace.TraceError($"[AzureBatchSchedulerDelegation] Exception thrown while registering job: {jobid}", e);
                throw;
            }

            Trace.TraceInformation($"[AzureBatchSchedulerDelegation] End: RegisterJob. Current job state = {batchJob.State}.");
            return (await AzureBatchJobStateConverter.FromAzureBatchJobAsync(batchJob), autoMax, autoMin);
        }

        /// <summary>
        /// Job finished event handler
        /// </summary>
        public static event EventHandler OnJobFinished;

        /// <summary>
        /// Job failed/canceled event handler
        /// </summary>
        public static event EventHandler OnJobFailedOrCanceled;

      
        /// <summary>
        /// Event triggered when job monitor entry's instance is exited
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event args</param>
        private void JobMonitorEntry_Exit(object sender, EventArgs e)
        {
            AzureBatchJobMonitorEntry entry = (AzureBatchJobMonitorEntry)sender;
            Debug.Assert(entry != null, "[AzureBatchSchedulerDelegation] Sender should be an instance of JobMonitorEntry class.");
            lock (this.JobMonitors)
            {
                this.JobMonitors.Remove(entry.SessionId);
            }

            entry.Exit -= new EventHandler(this.JobMonitorEntry_Exit);
            entry.Close();
            Trace.TraceInformation($"[AzureBatchSchedulerDelegation] End: JobMonitorEntry Exit.");
        }

        public Task<int?> GetTaskErrorCode(string jobId, string globalTaskId)
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