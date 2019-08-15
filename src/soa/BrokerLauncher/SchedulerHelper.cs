//------------------------------------------------------------------------------
// <copyright file="SchedulerHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Helper class for operation to scheduler
// </summary>
//------------------------------------------------------------------------------

// TODO: remove not supported interfaces
// TODO: change signature to reveal async nature

namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Hpc.ServiceBroker.Common;
    using Microsoft.Hpc.ServiceBroker.Common.SchedulerAdapter;

    using TelepathyCommon;
    using TelepathyCommon.HpcContext;
    using TelepathyCommon.HpcContext.Extensions;
    using TelepathyCommon.HpcContext.Extensions.RegistryExtension;

    /// <summary>
    /// Helper class for operation to scheduler
    /// </summary>
    internal class SchedulerHelper : ISchedulerHelper
    {
        /// <summary>
        /// The client for the scheduler proxy in HN
        /// </summary>
        private Lazy<SchedulerAdapterClient> schedulerClient;

        /// <summary>
        /// Stores the client proxy for SessionLauncher service
        /// </summary>
        private Lazy<SessionLauncherClient> sessionLauncherClient;

        /// <summary>
        /// Stores the semaphore object for scheduler client
        /// </summary>
        private SemaphoreSlim schedulerClientSS = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Stores the semaphore object for session launcher client
        /// </summary>
        private SemaphoreSlim sessionLauncherClientSS = new SemaphoreSlim(1, 1);

        /// <summary>
        /// the head node machine name or ip resolved from service fabric
        /// </summary>
        private Lazy<string> sessionNode;

        /// <summary>
        /// the thumbprint of the certificate used for internal communication 
        /// </summary>
        private Lazy<string> certThumbprint;

        /// <summary>
        /// Stores the fabric cluster context
        /// </summary>
        private ITelepathyContext context;

        /// <summary>
        /// Initializes a new instance of the SchedulerHelper class
        /// </summary>
        public SchedulerHelper(ITelepathyContext context)
        {
            this.context = context;
            this.sessionNode = new Lazy<string>(() => ResolveSessionNodeWithRetries().GetAwaiter().GetResult(), LazyThreadSafetyMode.ExecutionAndPublication);
            this.certThumbprint = new Lazy<string>(() => this.context.GetSSLThumbprint().GetAwaiter().GetResult(), LazyThreadSafetyMode.ExecutionAndPublication);
            this.schedulerClient = new Lazy<SchedulerAdapterClient>(
                () => new SchedulerAdapterClient(BindingHelper.HardCodedUnSecureNetTcpBinding, new EndpointAddress(SoaHelper.GetSchedulerDelegationAddress(this.sessionNode.Value))),
                LazyThreadSafetyMode.ExecutionAndPublication);
            this.sessionLauncherClient = new Lazy<SessionLauncherClient>(
                () => new SessionLauncherClient(this.sessionNode.Value, this.certThumbprint.Value),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Finalizes an instance of the SchedulerHelper class
        /// </summary>
        ~SchedulerHelper()
        {
            this.Dispose(false);
        }

        public bool Enable => true;

        /// <summary>
        /// Gets the head node
        /// </summary>
        public string HeadNode
        {
            get
            {
                return this.sessionNode.Value;
            }
        }

        /// <summary>
        /// a function help to judge whether a job purged.
        /// </summary>
        /// <param name="jobId">the job id.</param>
        /// <returns>a value indicating whether the specified job purged or not.</returns>
        public async Task<bool> IsJobPurged(int jobId)
        {
            /*
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            return await RetryHelper<bool>.InvokeOperationAsync(
                async () =>
                {
                    return !await this.schedulerClient.Value.IsValidJob(jobId);
                },
                async (e, r) =>
                {
                    TraceHelper.TraceError(jobId,
                        "[SchedulerHelper] Failed to check if the service job is purged: {0}\nRetryCount = {1}", e,
                        r.RetryCount);
                    await this.RenewSchedulerAdapterClientAsync();
                },
                retry);
                */
            return false;
        }

        /// <summary>
        /// Update the broker info
        /// </summary>
        /// <param name="info">broker info</param>
        public async Task UpdateBrokerInfo(BrokerInfo info)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("EndpointReference", string.Join(";", info.InitializationResult.BrokerEpr));

            // use the broker role IP address for the broker node on Azure
            properties.Add(BrokerSettingsConstants.BrokerNode, SoaHelper.IsOnAzure() ? AzureRoleHelper.GetLocalMachineAddress() : Environment.MachineName);

            properties.Add(BrokerSettingsConstants.Suspended, info.Durable);
            properties.Add(BrokerSettingsConstants.Durable, info.Durable);
            properties.Add(BrokerSettingsConstants.PersistVersion, info.PersistVersion);
            properties.Add(BrokerSettingsConstants.MessageDetailsAvailable, info.InitializationResult.SupportsMessageDetails);

            await this.UpdateBrokerInfoInternalAsync(info.SessionId, properties);
        }

        /// <summary>
        /// Update the broker's suspended property
        /// </summary>
        /// <param name="sessionId">indicating the session ID</param>
        /// <param name="suspended">indicating the broker is suspended or not</param>
        public async Task UpdateSuspended(int sessionId, bool suspended)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add(BrokerSettingsConstants.Suspended, suspended);
            await this.UpdateBrokerInfoInternalAsync(sessionId, p);
        }

        /// <summary>
        /// Restore broker recover info from scheduler
        /// </summary>
        /// <returns>list of broker recover info</returns>
        public async Task<BrokerRecoverInfo[]> LoadBrokerRecoverInfo()
        {
            // RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
            // return await RetryHelper<BrokerRecoverInfo[]>.InvokeOperationAsync(
            // async () => await this.schedulerClient.Value.GetRecoverInfoFromJobs(Environment.MachineName),
            // async (e, r) =>
            // {
            // TraceHelper.TraceError(0,
            // "[SchedulerHelper] Failed to load broker recover info: {0}\nRetryCount = {1}", e, r.RetryCount);
            // await this.RenewSchedulerAdapterClientAsync();
            // }, retry);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Try to get session start info from finished jobs
        /// </summary>
        /// <param name="sessionId">indicating the session Id</param>
        /// <returns>session start info</returns>
        public async Task<BrokerRecoverInfo> TryGetSessionStartInfoFromFininshedJobs(int sessionId)
        {
            /*
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
            return await RetryHelper<BrokerRecoverInfo>.InvokeOperationAsync(
                async () => await this.schedulerClient.Value.GetRecoverInfoFromJob(sessionId),
                async (e, r) =>
                {
                    TraceHelper.TraceError(0,
                        "[SchedulerHelper] Failed to load broker recover info: {0}\nRetryCount = {1}", e, r.RetryCount);
                    await this.RenewSchedulerAdapterClientAsync();
                }, retry);
                */
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets ACL string for certain job template
        /// </summary>
        /// <param name="jobTemplate">indicating the job template</param>
        /// <returns>returns ACL string</returns>
        public async Task<string> GetJobTemplateACL(string jobTemplate)
        {
            /*
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            string acl = null;
            try
            {
                acl = await RetryHelper<string>.InvokeOperationAsync(
                    async () => await this.schedulerClient.Value.GetJobTemlpateACL(jobTemplate),
                    async (e, r) =>
                    {
                        TraceHelper.TraceEvent(System.Diagnostics.TraceEventType.Error,
                            "[SchedulerHelper] Exception throwed while fetch ACL from template: {0}\nRetryCount = {1}",
                            e, r.RetryCount);
                        await this.RenewSchedulerAdapterClientAsync();
                    }, retry);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Warning, "[SchedulerHelper].GetJobTemplateACL: Exception {0}", ex);
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_FailedToGetSecurityDescriptor,
                    SR.FailedToGetSecurityDescriptor);
            }

            if (acl == null)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_FailedToGetSecurityDescriptor,
                    SR.FailedToGetSecurityDescriptor);
            }

            return acl;
            */
            throw new NotSupportedException();
        }

        // /// <summary>
        // /// Get specified job requeue count.
        // /// </summary>
        // /// <param name="jobid">job id</param>
        // /// <returns>requeue count</returns>
        // private async Task<int> GetJobRequeueCount(int jobid)
        // {
        // RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
        // return await RetryHelper<int>.InvokeOperationAsync(
        // async () => await this.schedulerClient.Value.GetJobRequeueCount(jobid),
        // async (e, r) =>
        // {
        // TraceHelper.TraceError(0, "[SchedulerHelper] Failed to get job requeue count: {0}\nRetryCount = {1}",
        // e, r.RetryCount);
        // await this.RenewSchedulerAdapterClientAsync();
        // }, retry);
        // }

        /// <summary>
        /// Gets job owner's sid
        /// </summary>
        /// <param name="jobId">indicating the job id</param>
        /// <returns>returns job owner's sid</returns>
        public async Task<string> GetJobOwnerSID(int jobId)
        {
            /*
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            string sid = null;
            try
            {
                sid = await RetryHelper<string>.InvokeOperationAsync(
                    async () => await this.schedulerClient.Value.GetJobOwnerSID(jobId),
                    async (e, r) =>
                    {
                        TraceHelper.TraceEvent(jobId, System.Diagnostics.TraceEventType.Error,
                            "[SchedulerHelper] Exception throwed while fetch job owner's SID: {0}\nRetryCount = {1}", e,
                            r.RetryCount);
                        await this.RenewSchedulerAdapterClientAsync();
                    }, retry);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Warning, "[SchedulerHelper].GetJobTemplateACL: Exception {0}", ex);
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_CannotGetUserSID, SR.CannotGetUserSID);
            }

            if (sid == null)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_CannotGetUserSID, SR.CannotGetUserSID);
            }

            return sid;
            */
            throw new NotSupportedException();
        }

        /// <summary>
        /// Fail a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        public async Task FailJob(int jobid, string reason)
        {
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            await RetryHelper<object>.InvokeOperationAsync(
                async () =>
                    {
                        await this.schedulerClient.Value.FailJobAsync(jobid, reason);
                        return null;
                    },
                async (e, r) =>
                    {
                        TraceHelper.TraceEvent(jobid, System.Diagnostics.TraceEventType.Error, "[SchedulerHelper] Exception throwed while failing job: {0}\nRetryCount = {1}", e, r.RetryCount);
                        await this.RenewSchedulerAdapterClientAsync();
                    },
                retry);
        }

        /// <summary>
        /// Check if the soa diag trace enabled for the specified session.
        /// </summary>
        /// <param name="jobId">job id of the session</param>
        /// <returns>soa diag trace is enabled or disabled </returns>
        public async Task<bool> IsDiagTraceEnabled(int jobId)
        {
            /*
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            return await RetryHelper<bool>.InvokeOperationAsync(
                async () =>
                {
                    return await Task.Run(() => this.schedulerClient.Value.IsDiagTraceEnabled(jobId));
                },
                async (e, r) =>
                {
                    // Bug 19256: If failed to check IsDiagTraceEnabled, do not
                    // write session specific trace since it might be disabled
                    TraceHelper.TraceEvent(TraceEventType.Error,
                        "[SchedulerHelper] Failed to check if diag trace is enabled: {0}\nRetryCount = {1}", e,
                        r.RetryCount);
                    await this.RenewSchedulerAdapterClientAsync();
                }, retry);
                */
            return false;
        }

        /// <summary>
        /// Dispose the scheduler helper
        /// Clean up the connection to scheduler
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets SOA configurations
        /// </summary>
        /// <param name="keys">indicating the keys</param>
        /// <returns>returns the values</returns>
        public async Task<Dictionary<string, string>> GetSOAConfigurations(List<string> keys)
        {
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            return await RetryHelper<Dictionary<string, string>>.InvokeOperationAsync(
                       async () =>
                           {
                               using (BrokerIdentity identity = new BrokerIdentity())
                               {
                                   identity.Impersonate();
                                   return await this.sessionLauncherClient.Value.GetSOAConfigurationsAsync(keys);
                               }
                           },
                       async (e, r) =>
                           {
                               TraceHelper.TraceEvent(
                                   TraceEventType.Warning,
                                   "[SchedulerHelper] Failed to get SOA configuration, Key:{0}, Retry:{1}, Error:{2}",
                                   string.Join(",", keys),
                                   r.RetryCount,
                                   e);
                               await this.RenewSessionLauncherClientAsync();
                           },
                       retry);
        }

        /// <summary>
        /// Get non terminated session.
        /// </summary>
        /// <returns>non terminated session dic</returns>
        public async Task<Dictionary<int, int>> GetNonTerminatedSession()
        {
            /*
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
            return await RetryHelper<Dictionary<int, int>>.InvokeOperationAsync(
                async () => await this.schedulerClient.Value.GetNonTerminatedSession(),
                async (e, r) =>
                {
                    TraceHelper.TraceError(0,
                        "[SchedulerHelper] Failed to get non terminated session: {0}\nRetryCount = {1}", e, r.RetryCount);
                    await this.RenewSchedulerAdapterClientAsync();
                }, retry);
                */
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets cluster info
        /// </summary>
        /// <param name="keys">indicating the keys</param>
        /// <returns>returns the values</returns>
        public async Task<ClusterInfoContract> GetClusterInfoAsync()
        {
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            return await RetryHelper<ClusterInfoContract>.InvokeOperationAsync(
                       async () =>
                           {
                               using (BrokerIdentity identity = new BrokerIdentity())
                               {
                                   identity.Impersonate();
                                   return await this.sessionLauncherClient.Value.GetClusterInfoAsync();
                               }
                           },
                       async (e, r) =>
                           {
                               TraceHelper.TraceEvent(TraceEventType.Warning, "[SchedulerHelper] Failed to get cluster info, Retry:{0}, Error:{1}", r.RetryCount, e);
                               await this.RenewSessionLauncherClientAsync();
                           },
                       retry);
        }

        /// <summary>
        /// Dispose the scheduler helper
        /// </summary>
        /// <param name="disposing">indicating if it's disposing</param>
        [method:
            System.Diagnostics.CodeAnalysis.SuppressMessage(
                "Microsoft.Usage",
                "CA2213:DisposableFieldsShouldBeDisposed",
                Target = "SchedulerHelper.client",
                Justification = "It's closed in Microsoft.Hpc.ServiceBroker.Common.Utility.AsyncCloseICommunicationObject(this.client).")]
        [method:
            System.Diagnostics.CodeAnalysis.SuppressMessage(
                "Microsoft.Usage",
                "CA2213:DisposableFieldsShouldBeDisposed",
                Target = "SchedulerHelper.sessionLauncherClient",
                Justification = "It's closed in Microsoft.Hpc.ServiceBroker.Common.Utility.AsyncCloseICommunicationObject(this.sessionLauncherClient).")]
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.schedulerClient.IsValueCreated)
                {
                    Utility.AsyncCloseICommunicationObject(this.schedulerClient.Value);
                }

                if (this.sessionLauncherClient.IsValueCreated)
                {
                    Utility.AsyncCloseICommunicationObject(this.sessionLauncherClient.Value);
                }

                this.sessionLauncherClientSS.Dispose();
            }
        }

        /// <summary>
        /// Renew client proxy to scheduler adapter internal service
        /// </summary>
        private async Task RenewSchedulerAdapterClientAsync()
        {
            if (this.schedulerClient.IsValueCreated)
            {
                await this.schedulerClientSS.WaitAsync();
                try
                {
                    if (this.schedulerClient.IsValueCreated)
                    {
                        if (this.schedulerClient.Value != null)
                        {
                            Utility.AsyncCloseICommunicationObject(this.schedulerClient.Value);
                        }

                        this.sessionNode = new Lazy<string>(() => this.ResolveSessionNodeWithRetries().GetAwaiter().GetResult(), LazyThreadSafetyMode.ExecutionAndPublication);
                        this.certThumbprint = new Lazy<string>(() => this.context.GetSSLThumbprint().GetAwaiter().GetResult(), LazyThreadSafetyMode.ExecutionAndPublication);
                        this.schedulerClient = new Lazy<SchedulerAdapterClient>(
                            () => new SchedulerAdapterClient(BindingHelper.HardCodedUnSecureNetTcpBinding, new EndpointAddress(SoaHelper.GetSchedulerDelegationAddress(this.sessionNode.Value))),
                            LazyThreadSafetyMode.ExecutionAndPublication);
                    }
                }
                finally
                {
                    this.schedulerClientSS.Release();
                }
            }
        }

        /// <summary>
        /// Renew client proxy to scheduler adapter internal service
        /// </summary>
        private async Task RenewSessionLauncherClientAsync()
        {
            if (this.sessionLauncherClient.IsValueCreated)
            {
                await this.sessionLauncherClientSS.WaitAsync();
                try
                {
                    if (this.sessionLauncherClient.IsValueCreated)
                    {
                        if (this.sessionLauncherClient.Value != null)
                        {
                            Utility.AsyncCloseICommunicationObject(this.sessionLauncherClient.Value);
                        }

                        this.sessionNode = new Lazy<string>(() => ResolveSessionNodeWithRetries().GetAwaiter().GetResult(), LazyThreadSafetyMode.ExecutionAndPublication);
                        this.certThumbprint = new Lazy<string>(() => this.context.GetSSLThumbprint().GetAwaiter().GetResult(), LazyThreadSafetyMode.ExecutionAndPublication);
                        this.sessionLauncherClient = new Lazy<SessionLauncherClient>(
                            () => new SessionLauncherClient(this.sessionNode.Value, this.certThumbprint.Value),
                            LazyThreadSafetyMode.ExecutionAndPublication);
                    }
                }
                finally
                {
                    this.sessionLauncherClientSS.Release();
                }
            }
        }

        /// <summary>
        /// Update broker info
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="properties">indicating the key value pairs to be updated</param>
        private async Task UpdateBrokerInfoInternalAsync(int sessionId, Dictionary<string, object> properties)
        {
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            await RetryHelper<object>.InvokeOperationAsync(
                async () =>
                    {
                        if (await this.schedulerClient.Value.UpdateBrokerInfoAsync(sessionId, properties).ConfigureAwait(false))
                        {
                            return null;
                        }
                        else
                        {
                            throw new InvalidOperationException("Can not update the properties in the scheduler database for EPRs");
                        }
                    },
                async (e, r) =>
                    {
                        TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "[BrokerLauncher.SchedulerHelper] UpdateBrokerInfo failed: Exception = {0}\nRetryCount = {1}", e, r.RetryCount);
                        await this.RenewSchedulerAdapterClientAsync().ConfigureAwait(false);
                    },
                retry).ConfigureAwait(false);
        }

        /// <summary>
        /// Resolve the session node from the context with infinite retries
        /// </summary>
        /// <returns>The resolved session node name</returns>
        private async Task<string> ResolveSessionNodeWithRetries()
        {
            RetryManager retry = SoaHelper.GetDefaultInfinitePeriodRertyManager();
            return await RetryHelper<string>.InvokeOperationAsync(
                       async () => await this.context.ResolveSessionLauncherNodeAsync(),
                       async (e, r) =>
                           {
                               TraceHelper.TraceWarning(0, "[SchedulerHelper] Failed to ResolveSessionLauncherNodeAsync: {0}\nRetryCount = {1}", e, r.RetryCount);
                               await Task.CompletedTask;
                           },
                       retry);
        }

        // /// <summary>
        // /// Create the SchedulerAdapterInternalClient with the session node
        // /// </summary>
        // /// <returns>The SchedulerAdapterInternalClient, null if exception happens</returns>
        // private SchedulerAdapterInternalClient CreateSchedulerAdapterInternalClient()
        // {
        // try
        // {
        // return new SchedulerAdapterInternalClient(this.sessionNode.Value, this.certThumbprint.Value);
        // }
        // catch (Exception e)
        // {
        // TraceHelper.TraceError(0, "[SchedulerHelper] Failed to CreateSchedulerAdapterInternalClient: {0}", e);
        // return null;
        // }
        // }
        // /// <summary>
        // /// Create the SessionLauncherClient with the session node
        // /// </summary>
        // /// <returns>The SessionLauncherClient, null if exception happens</returns>
        // private SessionLauncherClient CreateSessionLauncherClient()
        // {
        // try
        // {
        // return new SessionLauncherClient(this.sessionNode.Value, this.certThumbprint.Value);
        // }
        // catch (Exception e)
        // {
        // TraceHelper.TraceError(0, "[SchedulerHelper] Failed to CreateSessionLauncherClient: {0}", e);
        // return null;
        // }
        // }
    }
}