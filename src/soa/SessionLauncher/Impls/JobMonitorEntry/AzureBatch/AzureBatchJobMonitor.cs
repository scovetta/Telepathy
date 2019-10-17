// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.JobMonitorEntry.AzureBatch
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Common;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.DataMapping.AzureBatch;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.AzureBatch;
    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;

    using TelepathyConstants = Microsoft.Telepathy.Common.TelepathyConstants;

    internal class AzureBatchJobMonitor : IDisposable
    {
        /// <summary>
        /// The session id
        /// </summary>
        private readonly string sessionid;

        /// <summary>
        /// Service job
        /// </summary>
        private CloudJob cloudJob;

        /// <summary>
        /// Stores the dispose flag
        /// </summary>
        private int disposeFlag;

        /// <summary>
        /// Stores the min time gap between two pull task operations
        /// </summary>
        private const int PullJobMinGap = 1000;

        /// <summary>
        /// Stores the max time gap between two pull task operations (if required)
        /// </summary>
        private const int PullJobMaxGap = 10000;

        /// <summary>
        /// Stores the current pull task gap for two continuously request
        /// </summary>
        private int pullJobGap = PullJobMinGap;

        private BatchClient batchClient;

        /// <summary>
        /// Gets or sets the exit event
        /// </summary>
        public event EventHandler Exit;

        /// <summary>
        /// Gets or sets the report state event
        /// </summary>
        public Action<Telepathy.Session.Data.JobState, List<TaskInfo>, bool> ReportJobStateAction;

        /// <summary>
        /// Stores the last change time
        /// </summary>
        private DateTime lastChangeTime;

        private Telepathy.Session.Data.JobState previousJobState = Session.Data.JobState.Configuring;

        /// <summary>
        /// Initializes a new instance of the JobMonitorEntry class
        /// </summary>
        /// <param name="sessionid">indicating the session id</param>
        public AzureBatchJobMonitor(string sessionid, Action<Telepathy.Session.Data.JobState, List<TaskInfo>, bool> reportJobStateAction)
        {
            this.sessionid = sessionid;
            this.batchClient = AzureBatchConfiguration.GetBatchClient();
            this.ReportJobStateAction = reportJobStateAction;
            this.lastChangeTime = SqlDateTime.MinValue.Value;
        }

        /// <summary>
        /// Start the monitor
        /// </summary>
        public async Task StartAsync()
        {
            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] Start Azure Batch job monitor.");
            this.cloudJob = await this.batchClient.JobOperations.GetJobAsync(AzureBatchSessionJobIdConverter.ConvertToAzureBatchJobId(this.sessionid));
            if (this.cloudJob.State == JobState.Disabled)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Session_ValidateJobFailed_JobCanceled, SR.SessionLauncher_ValidateJobFailed_JobCanceled, this.sessionid.ToString());
            }

            try
            {
                await Task.Run(async () => await this.QueryJobChangeAsync());
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[AzureBatchJobMonitor] Exception thrown when trigger querying job info task: {0}", e);
            }
        }

        /// <summary>
        /// Query job info
        /// </summary>
        private async Task QueryJobChangeAsync()
        {
            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[AzureBatchJobMonitorEntry] Enters QueryTaskInfo method.");
            bool shouldExit = false;
            this.pullJobGap = PullJobMinGap;
            JobState state = JobState.Active;
            Session.Data.JobState currentJobState = Session.Data.JobState.Configuring;
            var pool = this.batchClient.PoolOperations.GetPool(AzureBatchConfiguration.BatchPoolName);
            ODATADetailLevel detailLevel = new ODATADetailLevel();
            detailLevel.SelectClause = "affinityId, ipAddress";
            var nodes = await pool.ListComputeNodes(detailLevel).ToListAsync();
            while (true)
            {
                if (shouldExit)
                {
                    break;
                }
                List<TaskInfo> stateChangedTaskList = new List<TaskInfo>();

                try
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[AzureBatchJobMonitor] Starting get job state.");
                    ODATADetailLevel detail = new ODATADetailLevel(selectClause: "state");
                    this.cloudJob = await this.batchClient.JobOperations.GetJobAsync(this.cloudJob.Id);
                    state = this.cloudJob.State.HasValue ? this.cloudJob.State.Value : state;
                    currentJobState = await AzureBatchJobStateConverter.FromAzureBatchJobAsync(this.cloudJob);
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] Current job state in AzureBatch: JobState = {0}\n", state);
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] Current job state in Telepathy: JobState = {0}\n", currentJobState);
                    stateChangedTaskList = await this.GetTaskStateChangeAsync(nodes);
                }
                catch (BatchException e)
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[AzureBatchJobMonitor] BatchException thrown when querying job info: {0}", e);
                    //If the previous job state is canceling and current job is not found, then the job is deleted.
                    if (e.RequestInformation != null && e.RequestInformation.HttpStatusCode != null)
                    {
                        if (previousJobState == Session.Data.JobState.Canceling && e.RequestInformation.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[AzureBatchJobMonitor] The queried job has been deleted.");
                            shouldExit = true;
                            currentJobState = Session.Data.JobState.Canceled;
                        }
                    }
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[AzureBatchJobMonitor] Exception thrown when querying job info: {0}", e);
                }
                finally
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] Previous job state repoert to AzureBatchJobMonitorEntry: JobState = {0}\n", previousJobState);

                    if (state == JobState.Completed || state == JobState.Disabled)
                    {
                        if (this.previousJobState == Session.Data.JobState.Canceling)
                        {
                            currentJobState = Session.Data.JobState.Canceled;
                        }
                        shouldExit = true;
                    }
                    else if (this.previousJobState == Session.Data.JobState.Canceling && !shouldExit)
                    {
                        //Override current job state as Canceling, because when all tasks turn to be completed, the job state converter will make job state finishing.
                        //If one job is cancelling in previous state and now is not in one terminated state, keep to reporting cancelling state to job monitor entry.
                        currentJobState = Session.Data.JobState.Canceling;
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] Overwrite current job state as {0} in Telepathy accoding to previous job state {1}\n", currentJobState, previousJobState);
                    }
                    if (this.ReportJobStateAction != null)
                    {
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] Current job state repoert to AzureBatchJobMonitorEntry: JobState = {0}\n", currentJobState);
                        this.ReportJobStateAction(currentJobState, stateChangedTaskList, shouldExit);
                    }
                    this.previousJobState = currentJobState;
                }

                if (!shouldExit)
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] Waiting {0} miliseconds and start another round of getting job state info.", this.pullJobGap);

                    // Sleep and pull job again, clear the register pull job flag
                    await Task.Delay(this.pullJobGap);
                    if (this.pullJobGap < PullJobMaxGap)
                    {
                        this.pullJobGap *= 2;
                        if (this.pullJobGap > PullJobMaxGap)
                        {
                            this.pullJobGap = PullJobMaxGap;
                        }
                    }
                }            
            }
        }

        /// <summary>
        /// Gets the task info
        /// </summary>
        /// <returns>returns the task info as a dictionary, keyed by task id</returns>
        /// <remarks>
        /// This method returns a list of task info which ChangeTime property is in this rank: [this.lastChangeTime, DateTime.Now].
        /// This method does not change this.lastChangeTime to DateTime.Now after getting tasks because it may fail when sending those information to broker
        /// So changeTime is outputed and this.lastChangeTime should be modified to this time after suceeded sending back task info
        /// </remarks>
        private async Task<List<TaskInfo>> GetTaskStateChangeAsync(List<ComputeNode> nodes)
        {
            DateTime changeTime = DateTime.UtcNow;
            try
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[AzureBatchgJobMonitor] Query task info...");
                ODATADetailLevel detail = new ODATADetailLevel(filterClause: $"(stateTransitionTime ge datetime'{this.lastChangeTime:O}')", selectClause: "id,nodeInfo,state,stateTransitionTime");
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] Query task info filter clause = {0}\n", detail.FilterClause);
                List<CloudTask> stateChangedTasks = await this.batchClient.JobOperations.ListTasks(this.cloudJob.Id, detail).ToListAsync();
                if (stateChangedTasks.Count == 0)
                {
                    // no service task dispathed yet.
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning,
                        "[AzureBatchJobMonitorEntry] Failed to get tasks or no task state change.");

                    return null;
                }

                List<TaskInfo> results = new List<TaskInfo>(stateChangedTasks.Count);
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] The number of changed state tasks is {0}", stateChangedTasks.Count);

                foreach (CloudTask task in stateChangedTasks)
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] task {0} state changed to {1}, at date time = {2}\n", task.Id, task.State, task.StateTransitionTime);
                    TaskState state = task.State.Value;
                    if (state == TaskState.Running)
                    {
                        TaskInfo info = new TaskInfo();
                        info.Id = task.Id;
                        info.State = TaskStateConverter.FromAzureBatchTaskState(task.State.Value);
                        info.MachineName = nodes.First(n => n.AffinityId == task.ComputeNodeInformation.AffinityId)
                            .IPAddress;
                        info.Capacity = Int32.Parse(TelepathyConstants.NodeCapacity);
                        info.FirstCoreIndex = Int32.Parse(TelepathyConstants.FirstCoreIndex);
                        results.Add(info);
                    }
                    else if (state == TaskState.Completed)
                    {
                        TaskInfo info = new TaskInfo();
                        info.Id = task.Id;
                        info.State = TaskStateConverter.FromAzureBatchTaskState(task.State.Value);
                        results.Add(info);
                    }
                }
                this.cloudJob.Refresh();
                this.lastChangeTime = changeTime;
                return results;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[AzureBatchJobMonitor] Fail when get task info: {0}", ex);
                return null;
            }
            finally
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[AzureBatchJobMonitor] Query task info finished.");
            }
        }

        /// <summary>
        /// Dispose the job monitor
        /// </summary>
        /// <param name="disposing">indicating the disposing flag</param>
        private void Dispose(bool disposing)
        {
            if (Interlocked.Increment(ref this.disposeFlag) == 1)
            {
                if (disposing)
                {
                    if (this.batchClient != null)
                    {
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitor] Start to dispose batch client in AzureBatchJobMonitor.");
                        this.batchClient.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Dispose the job monitor entry
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
