//------------------------------------------------------------------------------
// <copyright file="SchedulerAdapterInternalClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The client for the SchedulerAdapterInternal service.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.ServiceBroker;

    /// <summary>
    /// The client implementation for the scheduler adapter
    /// </summary>
    public class SchedulerAdapterInternalClient : ClientBase<ISchedulerAdapterInternal>, ISchedulerAdapterInternal
    {
        /// <summary>
        /// Stores the operation timeout
        /// </summary>
        private static readonly TimeSpan OperationTimeout = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Initializes a new instance of the SchedulerAdapterInternalClient class
        /// </summary>
        /// <param name="headNode">indicating the headnode</param>
        public SchedulerAdapterInternalClient(string headNode)
            : base(
                BindingHelper.HardCodedInternalSchedulerDelegationBinding,
                SoaHelper.CreateInternalCertEndpointAddress(
                    new Uri(SoaHelper.GetSchedulerDelegationInternalAddress(headNode)),
                    HpcContext.GetOrAdd(headNode, CancellationToken.None).GetSSLThumbprint().GetAwaiter().GetResult()))
        {
            // use certificate for cluster internal authentication
            string thunbprint = HpcContext.GetOrAdd(headNode, CancellationToken.None).GetSSLThumbprint().GetAwaiter().GetResult();
            this.ClientCredentials.UseInternalAuthentication(thunbprint);
            this.InnerChannel.OperationTimeout = OperationTimeout;
        }

        /// <summary>
        /// Update the job's properties
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="properties">the properties table</param>
        public async Task<bool> UpdateBrokerInfo(int jobid, Dictionary<string, object> properties)
        {
            return await base.Channel.UpdateBrokerInfo(jobid, properties).ConfigureAwait(false);
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
            //IAsyncResult result = base.Channel.BeginUpdateBrokerInfo(jobid, properties, null, null);
            //return base.Channel.EndUpdateBrokerInfo(result);
        }

        /// <summary>
        /// Get all the running and queued service jobs whose broker node is machinename.
        /// </summary>
        /// <param name="machineName">Node Name</param>
        /// <returns>sessionid, sessionstart info table</returns>
        public async Task<BrokerRecoverInfo[]> GetRecoverInfoFromJobs(string machineName)
        {
            return await base.Channel.GetRecoverInfoFromJobs(machineName).ConfigureAwait(false);
        }

        /// <summary>
        /// Create the sessionstartinfo from job properties.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>the sessionstart info</returns>
        public async Task<BrokerRecoverInfo> GetRecoverInfoFromJob(int jobid)
        {
            return await base.Channel.GetRecoverInfoFromJob(jobid).ConfigureAwait(false);
        }

        /// <summary>
        /// Check if the job id is valid
        /// </summary>
        /// <param name="jobid">jobid</param>
        /// <returns>true if the job id is valid</returns>
        public async Task<bool> IsValidJob(int jobid)
        {
            return await base.Channel.IsValidJob(jobid).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the job's owner SID
        /// </summary>
        /// <param name="jobid">indicating the job id</param>
        /// <returns>returns the job's owner SID</returns>
        public async Task<string> GetJobOwnerSID(int jobid)
        {
            return await base.Channel.GetJobOwnerSID(jobid).ConfigureAwait(false);
        }

        /// <summary>
        /// Get ACL string from a job template
        /// </summary>
        /// <param name="jobTemplate">the job template name</param>
        /// <returns>ACL string</returns>
        public async Task<string> GetJobTemlpateACL(string jobTemplate)
        {
            return await base.Channel.GetJobTemlpateACL(jobTemplate).ConfigureAwait(false);
        }

        /// <summary>
        /// Fail a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        public async Task FailJob(int jobid, string reason)
        {
            await base.Channel.FailJob(jobid, reason).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the job's allocated node name.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>list of the node name and location flag (on premise or not)</returns>
        public async Task<List<Tuple<string, bool>>> GetJobAllocatedNodeName(int jobid)
        {
            return await base.Channel.GetJobAllocatedNodeName(jobid).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the job's allocated node name.
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <param name="taskId">task id</param>
        /// <returns>list of the node name and location flag (on premise or not)</returns>
        public async Task<List<Tuple<string, bool>>> GetTaskAllocatedNodeName(int jobId, int taskId)
        {
            return await base.Channel.GetTaskAllocatedNodeName(jobId, taskId).ConfigureAwait(false);
        }

        /// <summary>
        /// Get all the exist session id list
        /// </summary>
        /// <returns>session id list</returns>
        public async Task<List<int>> GetAllSessionId()
        {
            return await base.Channel.GetAllSessionId().ConfigureAwait(false);
        }

        /// <summary>
        /// Get all the non terminated session id and requeue count.
        /// </summary>
        /// <returns>
        /// dictionary
        /// key: session Id
        /// value: requeue count
        /// </returns>
        public async Task<Dictionary<int, int>> GetNonTerminatedSession()
        {
            return await base.Channel.GetNonTerminatedSession().ConfigureAwait(false);
        }

        /// <summary>
        /// Get specified job requeue count
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>requeue count</returns>
        public async Task<int> GetJobRequeueCount(int jobid)
        {
            return await base.Channel.GetJobRequeueCount(jobid).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the session's broker node name.
        /// </summary>
        /// <param name="jobid">job id of the session</param>
        /// <returns>broker node name</returns>
        public async Task<string> GetSessionBrokerNodeName(int jobid)
        {
            return await base.Channel.GetSessionBrokerNodeName(jobid).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the broker node name list.
        /// </summary>
        /// <returns>broker node name list</returns>
        public async Task<List<string>> GetBrokerNodeName()
        {
            return await base.Channel.GetBrokerNodeName().ConfigureAwait(false);
        }

        /// <summary>
        /// Check if the soa diag trace enabled for the specified session.
        /// </summary>
        /// <param name="jobid">job id of the session</param>
        /// <returns>soa diag trace is enabled or disabled </returns>
        public bool IsDiagTraceEnabled(int jobid)
        {
            return base.Channel.IsDiagTraceEnabled(jobid);
        }

        /// <summary>
        /// Dump the event log onto a target file
        /// </summary>
        /// <param name="targetFolder">indicating the target folder to put the dumped file</param>
        /// <param name="logName">indicating the log name</param>
        /// <returns>returns the dumped file name</returns>
        public async Task<string> DumpEventLog(string targetFolder, string logName)
        {
            return await base.Channel.DumpEventLog(targetFolder, logName).ConfigureAwait(false);
        }

        /// <summary>
        /// Get specified job's customized properties.
        /// </summary>
        /// <param name="propNames">customized property names</param>
        /// <returns>customized properties</returns>
        public async Task<Dictionary<string, string>> GetJobCustomizedProperties(int jobid, string[] propNames)
        {
            return await base.Channel.GetJobCustomizedProperties(jobid, propNames).ConfigureAwait(false);
        }

        /// <summary>
        /// Set job's progress message.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <param name="message">progress message</param>
        public async Task SetJobProgressMessage(int jobid, string message)
        {
            await base.Channel.SetJobProgressMessage(jobid, message).ConfigureAwait(false);
        }

        /// <summary>
        /// Work around for WCF ServiceChannel leak bug
        /// Creates the channel with OperationContext=null to avoid creating the ServiceChannel
        /// with the InstanceContext for the singleton service.
        /// </summary>
        /// <returns>A new channel</returns>
        protected override ISchedulerAdapterInternal CreateChannel()
        {
            OperationContext oldContext = OperationContext.Current;
            OperationContext.Current = null;

            try
            {
                return base.CreateChannel();
            }
            finally
            {
                OperationContext.Current = oldContext;
            }
        }
    }
}
