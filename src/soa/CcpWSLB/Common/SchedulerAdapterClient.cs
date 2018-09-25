//------------------------------------------------------------------------------
// <copyright file="SchedulerAdapterClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The client implementation for the scheduler adapter
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Scheduler.Session.Internal;
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading.Tasks;
    using SoaAmbientConfig;

    /// <summary>
    /// The client implementation for the scheduler adapter
    /// </summary>
    internal class SchedulerAdapterClient : DuplexClientBase<ISchedulerAdapter>, ISchedulerAdapter
    {
        /// <summary>
        /// Stores the timeout
        /// </summary>
        private static readonly TimeSpan SchedulerAdapterTimeout = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Initializes a new instance of the SchedulerAdapterClient class
        /// </summary>
        /// <param name="headnode">indicating the headnode</param>
        /// <param name="instanceContext">indicating the instance context</param>
        public SchedulerAdapterClient(string headnode, string certThrumbprint, InstanceContext instanceContext)
            : base(
                instanceContext,
                BindingHelper.HardCodedInternalSchedulerDelegationBinding,
                SoaHelper.CreateInternalCertEndpointAddress(new Uri(SoaHelper.GetSchedulerDelegationAddress(headnode)), certThrumbprint))
        {
            BrokerTracing.TraceVerbose("[SchedulerAdapterClient] In constructor");
            this.ClientCredentials.UseInternalAuthentication(certThrumbprint);
            if (BrokerIdentity.IsHAMode)
            {
                // Bug 10301 : Explicitly open channel when impersonating the resource group's account if running on failover cluster so identity flows correctly when
                //      calling HpcSession.
                //  NOTE: The patch we got from the WCF team (KB981001) only works when the caller is on a threadpool thread. 
                //  NOTE: Channel must be opened before setting OperationTimeout
                using (BrokerIdentity identity = new BrokerIdentity())
                {
                    identity.Impersonate();
                    this.Open();
                }
            }

            this.InnerChannel.OperationTimeout = SchedulerAdapterTimeout;

            foreach (OperationDescription op in this.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>() as DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
        }

        /// <summary>
        /// Start to subscribe the job and task event
        /// </summary>
        /// <param name="jobid">indicating the job id</param>
        /// <returns>tuple of jobstate, automax, automin of the job</returns>
        public async Task<Tuple<JobState, int, int>> RegisterJob(int jobid)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();

                return await this.Channel.RegisterJob(jobid);
                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
                //IAsyncResult result = base.Channel.BeginRegisterJob(jobid, null, null);
                //return base.Channel.EndRegisterJob(out autoMax, out autoMin, result);
            }
        }

        /// <summary>
        /// Update the job's properties
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="properties">the properties table</param>
        public async Task<bool> UpdateBrokerInfo(int jobid, Dictionary<string, object> properties)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await this.Channel.UpdateBrokerInfo(jobid, properties);
            }
        }

        /// <summary>
        /// Finish a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        public async Task FinishJob(int jobid, string reason)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                await this.Channel.FinishJob(jobid, reason);
            }
        }

        /// <summary>
        /// Fail a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        public async Task FailJob(int jobid, string reason)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                await this.Channel.FailJob(jobid, reason);
            }
        }

        /// <summary>
        /// Requeue or fail a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        public async Task RequeueOrFailJob(int jobid, string reason)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                await this.Channel.RequeueOrFailJob(jobid, reason);
            }
        }

        /// <summary>
        /// Add a node to job's exclude node list
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="nodeName">name of the node to be excluded</param>
        /// <returns>true if the node is successfully blacklisted, or the job is failed. false otherwise</returns>
        public async Task<bool> ExcludeNode(int jobid, string nodeName)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await this.Channel.ExcludeNode(jobid, nodeName);
            }
        }

        /// <summary>
        /// Get the error code property of the specified task.
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <param name="globalTaskId">unique task id</param>
        /// <returns>return error code value if it exists, otherwise return null</returns>
        public async Task<int?> GetTaskErrorCode(int jobId, int globalTaskId)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await this.Channel.GetTaskErrorCode(jobId, globalTaskId);
            }
        }

        /// <summary>
        /// Get the graceful preemption info.
        /// </summary>
        /// <param name="jobId">the job id</param>
        /// <returns>tuple of if the method succeeded, the number of plannedCoreCount and the taskIds.</returns>
        public async Task<(bool succeed, BalanceInfo balanceInfo, List<int> taskIds, List<int> runningTaskIds)> GetGracefulPreemptionInfo(int jobId)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await this.Channel.GetGracefulPreemptionInfo(jobId);
            }
        }
     
        /// <summary>
        /// Finish a task.
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="taskUniqueId">the task unique id</param>
        /// <returns>true if the method succeeded.</returns>
        public Task<bool> FinishTask(int jobid, int taskUniqueId)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();

                return this.Channel.FinishTask(jobid, taskUniqueId);
            }
        }
    }
}
