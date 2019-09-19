// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#if HPCPACK
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.ServiceBroker;
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading.Tasks;
    /// <summary>
    /// The client implementation for the scheduler adapter
    /// </summary>
    internal class HpcSchedulerAdapterInternalClient : ClientBase<IHpcSchedulerAdapterInternal>, IHpcSchedulerAdapterInternal
    {
        /// <summary>
        /// Stores the operation timeout
        /// </summary>
        private static readonly TimeSpan OperationTimeout = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Initializes a new instance of the HpcSchedulerAdapterInternalClient class
        /// </summary>
        /// <param name="headNodeMachine">indicating the headnode</param>
        public HpcSchedulerAdapterInternalClient(string headNodeMachine, string certThumbprint)
            : base(
                BindingHelper.HardCodedInternalSchedulerDelegationBinding,
                SoaHelper.CreateInternalCertEndpointAddress(new Uri(SoaHelper.GetSchedulerDelegationInternalAddress(headNodeMachine)), certThumbprint))
        {
#if BrokerLauncher
            BrokerTracing.TraceVerbose("[HpcSchedulerAdapterInternalClient] In constructor");
#endif
            // use certificate for cluster internal authentication
            this.ClientCredentials.UseInternalAuthentication(certThumbprint);

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

            this.InnerChannel.OperationTimeout = OperationTimeout;
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
                return await base.Channel.UpdateBrokerInfo(jobid, properties);
                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
                // IAsyncResult result = base.Channel.BeginUpdateBrokerInfo(jobid, properties, null, null);
                // return base.Channel.EndUpdateBrokerInfo(result);
            }
        }

        /// <summary>
        /// Get all the running and queued service jobs whose broker node is machinename.
        /// </summary>
        /// <param name="machineName">Node Name</param>
        /// <returns>sessionid, sessionstart info table</returns>
        public async Task<BrokerRecoverInfo[]> GetRecoverInfoFromJobs(string machineName)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetRecoverInfoFromJobs(machineName);
            }
        }

        /// <summary>
        /// Create the sessionstartinfo from job properties.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>the sessionstart info</returns>
        public async Task<BrokerRecoverInfo> GetRecoverInfoFromJob(int jobid)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetRecoverInfoFromJob(jobid);
            }
        }

        /// <summary>
        /// Check if the job id is valid
        /// </summary>
        /// <param name="jobid">jobid</param>
        /// <returns>true if the job id is valid</returns>
        public async Task<bool> IsValidJob(int jobid)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.IsValidJob(jobid);
            }
        }

        /// <summary>
        /// Get the job's owner SID
        /// </summary>
        /// <param name="jobid"></param>
        /// <returns></returns>
        public async Task<string> GetJobOwnerSID(int jobid)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetJobOwnerSID(jobid);
            }
        }

        /// <summary>
        /// Get ACL string from a job template
        /// </summary>
        /// <param name="jobTemplate">the job template name</param>
        /// <returns>ACL string</returns>
        public async Task<string> GetJobTemlpateACL(string jobTemplate)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetJobTemlpateACL(jobTemplate);
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
                await base.Channel.FailJob(jobid, reason);
            }
        }

        /// <summary>
        /// Get the job's allocated node name.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>list of the node name and location flag (on premise or not)</returns>
        public async Task<List<Tuple<string, bool>>> GetJobAllocatedNodeName(int jobid)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetJobAllocatedNodeName(jobid);
            }
        }

        /// <summary>
        /// Get all the exist session id list
        /// </summary>
        /// <returns>session id list</returns>
        public async Task<List<int>> GetAllSessionId()
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetAllSessionId();
            }
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
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetNonTerminatedSession();
            }
        }

        /// <summary>
        /// Get specified job requeue count
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>requeue count</returns>
        public async Task<int> GetJobRequeueCount(int jobid)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetJobRequeueCount(jobid);
            }
        }

        /// <summary>
        /// Get the task's allocated node name.
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <param name="taskId">task id</param>
        /// <returns>list of the node name and location flag (on premise or not)</returns>
        public async Task<List<Tuple<string, bool>>> GetTaskAllocatedNodeName(int jobId, int taskId)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetTaskAllocatedNodeName(jobId, taskId);
            }
        }

        /// <summary>
        /// Get the session's broker node name.
        /// </summary>
        /// <param name="jobid">job id of the session</param>
        /// <returns>broker node name</returns>
        public async Task<string> GetSessionBrokerNodeName(int jobid)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetSessionBrokerNodeName(jobid);
            }
        }

        /// <summary>
        /// Get the broker node name list.
        /// </summary>
        /// <returns>name list</returns>
        public async Task<List<string>> GetBrokerNodeName()
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetBrokerNodeName();
            }
        }

        /// <summary>
        /// Get specified job's customized properties.
        /// </summary>
        /// <param name="propNames">customized property names</param>
        /// <returns>customized properties</returns>
        public async Task<Dictionary<string, string>> GetJobCustomizedProperties(int jobid, string[] propNames)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.GetJobCustomizedProperties(jobid, propNames);
            }
        }

        /// <summary>
        /// Check if the soa diag trace enabled for the specified session.
        /// </summary>
        /// <param name="jobid">job id of the session</param>
        /// <returns>soa diag trace is enabled or disabled </returns>
        public bool IsDiagTraceEnabled(int jobid)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return base.Channel.IsDiagTraceEnabled(jobid);
            }
        }

        /// <summary>
        /// Dump the event log onto a target file
        /// </summary>
        /// <param name="targetFolder">indicating the target folder to put the dumped file</param>
        /// <param name="logName">indicating the log name</param>
        /// <returns>returns the dumped file name</returns>
        public async Task<string> DumpEventLog(string targetFolder, string logName)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return await base.Channel.DumpEventLog(targetFolder, logName);
            }
        }

        /// <summary>
        /// Set job's progress message.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <param name="message">progress message</param>
        public async Task SetJobProgressMessage(int jobid, string message)
        {
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                await base.Channel.SetJobProgressMessage(jobid, message);
            }
        }

        /// <summary>
        /// Work around for WCF ServiceChannel leak bug
        /// Creates the channel with OperationContext=null to avoid creating the ServiceChannel
        /// with the InstanceContext for the singleton service.
        /// </summary>
        /// <returns>A new channel</returns>
        protected override IHpcSchedulerAdapterInternal CreateChannel()
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
#endif
