//------------------------------------------------------------------------------
// <copyright file="SchedulerAdapterClientFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Factory class for scheduler adapter client
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.Common
{
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.ServiceBroker.BackEnd;
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;
    using SoaAmbientConfig;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Factory class for scheduler adapter client
    /// </summary>
    internal class SchedulerAdapterClientFactory : IDisposable
    {
        /// <summary>
        /// Stores the shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// Stores the instance of the scheduler adapter client
        /// </summary>
        private ISchedulerAdapter schedulerAdapterClient;

        /// <summary>
        /// Stores the semaphore to create client
        /// </summary>
        private SemaphoreSlim createClientSS = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Stores the service job monitor instance
        /// </summary>
        private InternalServiceJobMonitor monitor;

        /// <summary>
        /// Stores the dispatcher manager
        /// </summary>
        private DispatcherManager dispatcherManager;

        /// <summary>
        /// Stores the fabric cluster context;
        /// </summary>
        private IHpcContext context;

        /// <summary>
        /// Initializes a new instance of the SchedulerAdapterClientFactory class
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="monitor">indicating the monitor</param>
        /// <param name="dispatcherManager">indicating the dispatcher manager</param>
        public SchedulerAdapterClientFactory(SharedData sharedData, InternalServiceJobMonitor monitor, DispatcherManager dispatcherManager, IHpcContext context)
        {
            this.sharedData = sharedData;
            this.monitor = monitor;
            this.dispatcherManager = dispatcherManager;
            this.context = context;
        }

        /// <summary>
        /// Finalizes an instance of the SchedulerAdapterClientFactory class
        /// </summary>
        ~SchedulerAdapterClientFactory()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Dispose the shceduler adapter client factory
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the instance of scheduler adapter client
        /// </summary>
        /// <returns>returns the instance of the scheduler adapter client</returns>
        public async Task<ISchedulerAdapter> GetSchedulerAdapterClientAsync()
        {
            //this.CheckClient();
            bool newClientCreated = false;
            SchedulerAdapterClient client = this.schedulerAdapterClient as SchedulerAdapterClient;
            if (this.schedulerAdapterClient == null || client == null || client.State == CommunicationState.Faulted || client.InnerChannel.State == CommunicationState.Faulted)
            {
                await this.createClientSS.WaitAsync();
                try
                {
                    if (this.schedulerAdapterClient == null)
                    {
                        await this.CreateClient();
                        if (!SoaAmbientConfig.StandAlone)
                        {
                            newClientCreated = true;
                        }
                    }
                    else
                    {
                        client = this.schedulerAdapterClient as SchedulerAdapterClient;
                        if (client == null || client.State == CommunicationState.Faulted || client.InnerChannel.State == CommunicationState.Faulted)
                        {
                            try
                            {
                                Utility.AsyncCloseICommunicationObject(client);
                                await this.CreateClient();
                                if (!SoaAmbientConfig.StandAlone)
                                {
                                    newClientCreated = true;
                                }
                            }
                            catch (Exception e)
                            {
                                // Swallow exception when creating client
                                BrokerTracing.TraceError("[SchedulerAdapterClientFactory] Exception thrown when creating client: {0}", e);
                            }
                        }
                    }
                }
                finally
                {
                    this.createClientSS.Release();
                }

                if (newClientCreated)
                {
                    await this.monitor.RegisterJob();
                }
            }

            return this.schedulerAdapterClient;
        }

        /// <summary>
        /// Create client
        /// </summary>
        private async Task CreateClient()
        {
            if (this.sharedData.StartInfo.EprList != null)
            {
                this.schedulerAdapterClient = new DummySchedulerAdapterClient(this.sharedData.StartInfo.EprList, this.dispatcherManager);
            }
            else
            {
                //TODO
                string headnodeMachine = System.Net.Dns.GetHostName();
                string certThrumbprint = string.Empty;
                if (!SoaAmbientConfig.StandAlone)
                {
                    headnodeMachine = await this.context.ResolveSessionLauncherNodeAsync();
                    certThrumbprint = await this.context.GetSSLThumbprint();
                }

                this.schedulerAdapterClient = new SchedulerAdapterClient(headnodeMachine, certThrumbprint, new System.ServiceModel.InstanceContext(this.monitor));
            }
        }

        /// <summary>
        /// Check client state, recreate new client if the client is faulted
        /// </summary>
        private void CheckClient()
        {
            SchedulerAdapterClient client = this.schedulerAdapterClient as SchedulerAdapterClient;
            if (client != null)
            {
                if (client.State == CommunicationState.Faulted || client.InnerChannel.State == CommunicationState.Faulted)
                {
                    try
                    {
                        Utility.AsyncCloseICommunicationObject(client);
                        this.CreateClient().GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        // Swallow exception when creating client
                        BrokerTracing.TraceError("[SchedulerAdapterClientFactory] Exception thrown when creating client: {0}", e);
                    }
                }
            }
        }

        /// <summary>
        /// Close the factory
        /// </summary>
        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Dispose the scheduler adapter client factory
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.schedulerAdapterClient is ICommunicationObject)
                {
                    Utility.AsyncCloseICommunicationObject((ICommunicationObject)this.schedulerAdapterClient);
                }
            }
        }

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
                foreach (string epr in this.eprList)
                {
                    DispatcherInfo info = new EprDispatcherInfo(epr, 1, Interlocked.Increment(ref uniqueId));
                    await this.dispatcherManager.NewDispatcherAsync(info).ConfigureAwait(false);
                }

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
