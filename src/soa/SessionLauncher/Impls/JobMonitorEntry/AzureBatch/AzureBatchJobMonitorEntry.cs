// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.JobMonitorEntry.AzureBatch
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Azure.Batch;
    using Microsoft.Telepathy.Common;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.AzureBatch;
    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;

    using JobState = Microsoft.Azure.Batch.Common.JobState;

    internal class AzureBatchJobMonitorEntry : IDisposable
    {
        /// <summary>
        /// Stores the dispose flag
        /// </summary>
        private int disposeFlag;

        /// <summary>
        /// The session id
        /// </summary>
        private readonly string sessionid;

        /// <summary>
        /// Stores the context
        /// </summary>
        private ISchedulerNotify context;

        /// <summary>
        /// Stores the previous state
        /// </summary>
        private Telepathy.Session.Data.JobState currentState;

        /// <summary>
        /// Stores the min units of resource
        /// </summary>
        private int minUnits;

        /// <summary>
        /// Stores the max units of resource
        /// </summary>
        private int maxUnits;

        /// <summary>
        /// Service job
        /// </summary>f
        private CloudJob cloudJob;

        /// <summary>
        /// Stores how many times corresponding job has been requeued because of broker request
        /// </summary>
        private int requeueCount;

        /// <summary>
        /// Stores the date time that Start() opertaion was last called
        /// </summary>
        private DateTime lastStartTime = DateTime.MinValue;

        /// <summary>
        /// Bacth Client configuration
        /// </summary>
        private BatchClient batchClient;

        /// <summary>
        /// monitor Azure Batch job state
        /// </summary>
        private AzureBatchJobMonitor batchJobMonitor;

        /// <summary>
        /// Gets or sets the exit event
        /// </summary>
        public event EventHandler Exit;

        /// <summary>
        /// Lock for check job state
        /// </summary>
        private object changeJobStateLock = new object();

        /// <summary>
        /// Initializes a new instance of the JobMonitorEntry class
        /// </summary>
        /// <param name="sessionid">indicating the session id</param>
        public AzureBatchJobMonitorEntry(string sessionid)
        {
            this.sessionid = sessionid;
            this.batchClient = AzureBatchConfiguration.GetBatchClient();
        }

        /// <summary>
        /// Gets the previous state
        /// </summary>
        public Telepathy.Session.Data.JobState CurrentState
        {
            get { return this.currentState; }
        }

        /// <summary>
        /// Gets the session id
        /// </summary>
        public string SessionId
        {
            get { return this.sessionid; }
        }

        /// <summary>
        /// Gets the min units
        /// Calculated at startup
        /// </summary>
        public int MinUnits
        {
            get { return this.minUnits; }
        }

        /// <summary>
        /// Gets the max units
        /// Calculated at startup
        /// </summary>
        public int MaxUnits
        {
            get { return this.maxUnits; }
        }


        /// <summary>
        /// Gets the service job
        /// </summary>
        internal CloudJob CloudJob
        {
            get { return this.cloudJob; }
        }

        /// <summary>
        /// Dispose the job monitor entry
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start the monitor
        /// </summary>
        public async Task<CloudJob> StartAsync(System.ServiceModel.OperationContext context)
        {
            TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitorEntry] Start monitor Entry.");
            this.currentState = Telepathy.Session.Data.JobState.Queued;
            this.context = context.GetCallbackChannel<ISchedulerNotify>();
            this.cloudJob = await this.batchClient.JobOperations.GetJobAsync(AzureBatchSessionJobIdConverter.ConvertToAzureBatchJobId(this.sessionid));

            if (this.cloudJob.State == JobState.Disabled)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Session_ValidateJobFailed_JobCanceled, SR.SessionLauncher_ValidateJobFailed_JobCanceled, this.sessionid.ToString());
            }

            if (this.cloudJob.Metadata != null)
            {
                MetadataItem maxUnitsItem = this.cloudJob.Metadata.FirstOrDefault(item => item.Name == "MaxUnits");
                if (maxUnitsItem != null)
                {
                    if (Int32.TryParse(maxUnitsItem.Value, out int result))
                    {
                        this.maxUnits = result;
                    }
                }
            }

            // monitor batch job state
            this.batchJobMonitor = new AzureBatchJobMonitor(this.sessionid, this.JobMonitor_OnReportJobState);
            try
            {
                Task.Run(() => this.StartMonitorAsync());
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[AzureBatchJobMonitorEntry] Exception thrown when start Azure Batch Job Monitor: {0}", e);
            }

            return this.cloudJob;
        }

        /// <summary>
        /// Close the job monitor entry
        /// </summary>
        public void Close()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Query task info
        /// </summary>
        private async Task StartMonitorAsync()
        {
            try
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Verbose, "[AzureBatchJobMonitorEntry] Start Azure Batch Job Monitor.");
                // RetryManager mgr = new RetryManager(new ExponentialRandomBackoffRetryTimer(1 * 1000, 10 * 1000));
                // await mgr.InvokeWithRetryAsync(() => await batchJobMonitor.StartAsync(), ex => true);
                RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                await RetryHelper<Task>.InvokeOperationAsync(
                        async () =>
                        {
                            await this.batchJobMonitor.StartAsync();
                            return null;
                        },
                        async (e, r) => await Task.FromResult<object>(new Func<object>(() => { TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[AzureBatchJobMonitorEntry] Exception thrown when trigger start Azure Batch Job Monitor: {0} ", e, r.RetryCount); return null; }).Invoke()),
                        retry);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Warning, "[AzureBatchJobMonitorEntry] Exception thrown when trigger start Azure Batch Job Monitor: {0}", e);
            }
        }

        /// <summary>
        /// Callback when Azure Batch Monitor report jon state
        /// </summary>
        private async void JobMonitor_OnReportJobState(Telepathy.Session.Data.JobState state, List<TaskInfo> stateChangedTaskList, bool shouldExit)
        {
            if (state != this.currentState)
            {
                lock (this.changeJobStateLock)
                {
                    if (state != this.currentState)
                    {
                        this.currentState = state;
                        if (this.context != null)
                        {
                            try
                            {
                                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitorEntry] Job state change event triggered, new state received from AzureBatchJobMonitor: {0}", state);
                                ISchedulerNotify proxy = this.context;
                                proxy.JobStateChanged(state);
                            }
                            catch (System.ObjectDisposedException e)
                            {
                                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[AzureBatchJobMonitorEntry] Callback channel is disposed: {0}, lose connection to broker.", e);
                                this.context = null;
                            }
                            catch (CommunicationException e)
                            {
                                // Channel is aborted, set the context to null
                                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[AzureBatchJobMonitorEntry] Callback channel is aborted: {0}", e);
                                this.context = null;
                            }
                            catch (Exception e)
                            {
                                TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[AzureBatchJobMonitorEntry] Failed to trigger job state change event: {0}", e);
                            }
                        }
                    }                   
                }
            }

            if (stateChangedTaskList != null)
            {
                if (this.context != null)
                {
                    try
                    {
                        ISchedulerNotify proxy = this.context;
                        await proxy.TaskStateChanged(stateChangedTaskList);
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitorEntry] Task state change event triggered.");
                    }
                    catch (System.ObjectDisposedException e)
                    {
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[AzureBatchJobMonitorEntry] Callback channel is disposed: {0}, lose connection to broker", e);
                        this.context = null;
                    }
                    catch (CommunicationException e)
                    {
                        // Channel is aborted, set the context to null
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[AzureBatchJobMonitorEntry] Callback channel is aborted: {0}", e);
                        this.context = null;
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(this.sessionid, TraceEventType.Error, "[AzureBatchJobMonitorEntry] Failed to trigger task state change event: {0}", e);
                    }
                }            
            }

            if (shouldExit)
            {
                if (this.Exit != null)
                {
                    TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitorEntry] Exit AzureBatchJobMonitor Entry");
                    this.Exit(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Dispose the job monitor entry
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
                        this.batchClient.Dispose();
                    }
                }
            }
        }
    }
}
