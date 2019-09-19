// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#if HPCPACK
namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.HpcPack
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;
    using Microsoft.Hpc.Scheduler.Session.HpcPack.DataMapping;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.ServiceBroker;

    using Constant = Microsoft.Hpc.Scheduler.Session.Internal.Constant;
    using JobState = Microsoft.Hpc.Scheduler.Session.Data.JobState;

    internal class HpcPackSessionLauncher : SessionLauncher
    {
        /// <summary>
        /// If the user doesn't set any min/max value to the session, we will give an initial value
        /// of 16 tasks to the service job.
        /// </summary>
        private const int DefaultInitTaskNumber = 16;

        /// <summary>
        /// cancel job if failed task count is more than this boundary
        /// </summary>
        private const int MaxFailedTask = 10000;

        public HpcPackSessionLauncher(string headNode, bool runningLocal, BrokerNodesManager brokerNodesManager)
        {
            // set thread pool for scale
            int minThreads = Math.Min(Environment.ProcessorCount * 64, 1000);
            int maxThreads = Math.Min(Environment.ProcessorCount * 128, 2000);
            ThreadPool.SetMinThreads(minThreads, minThreads);
            ThreadPool.SetMaxThreads(maxThreads, maxThreads);

            ParamCheckUtility.ThrowIfEmpty(headNode, "headNode");

            TraceHelper.TraceEvent(TraceEventType.Information, "Header node:{0}", headNode);

            this.runningLocal = runningLocal;
            this.headNodeName = headNode;
            this.brokerNodesManager = brokerNodesManager;

            // retrive the cluster info from the reliable registry and keep a watcher to update the value
            clusterInfo = new ClusterInfo();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            do
            {
                try
                {
                    this.InitializeSchedulerConnect().GetAwaiter().GetResult();
                }
                catch (Exception)
                {
                    if (stopWatch.ElapsedMilliseconds > ConnectToSchedulerTimeout)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .SessionLauncher: Connecting to the scheduler, {0}, timeout.", headNode);
                        throw;
                    }

                    Thread.Sleep(100);
                }
            }
            while (this.schedulerConnectState != SchedulerConnectState.ConnectionComplete);

            // set the CCP_SERVICEREGISTRATION_PATH scheduler environment for scheduler only if the value is not already set
            try
            {
                string regPath = null;
                regPath = JobHelper.GetEnvironmentVariable(this.scheduler, Constant.RegistryPathEnv);
                if (regPath == null)
                {
                    var context = HpcContext.Get();
                    regPath = context.Registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.ServiceRegistrationSharePropertyName, context.CancellationToken)
                        .GetAwaiter()
                        .GetResult();

                    // v5sp1: Enable soa registration store by default
                    if (!regPath.Contains(HpcConstants.RegistrationStoreToken))
                    {
                        regPath = HpcConstants.RegistrationStoreToken + ";" + regPath;
                    }

                    this.scheduler.SetEnvironmentVariable(Constant.RegistryPathEnv, regPath);
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .SessionLauncher: Failed to set the RegistryPathEnv scheduler environment. {0}", e);
            }

            if (SessionLauncherSettings.Default.EnableDataService)
            {
                this.dataService = new DataService(clusterInfo, scheduler);
            }

            try
            {
                this.ConstructSessionPool();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .SessionLauncher: Failed to construct session pool. Exception:{0}", ex.ToString());
            }
        }

        /// <summary>
        /// the scheduler
        /// </summary>
        private IScheduler scheduler;

        /// <summary>
        /// If session launcher is running locally
        /// </summary>
        private readonly bool runningLocal;

        /// <summary>
        /// the head node.
        /// </summary>
        private readonly string headNodeName;

        /// <summary>
        /// a value indicating the current state of connection to scheduler.
        /// </summary>
        private SchedulerConnectState schedulerConnectState = SchedulerConnectState.None;

        /// <summary>
        /// data service instance
        /// </summary>
        private DataService dataService;

        /// <summary>
        /// the broker node manager.
        /// </summary>
        private BrokerNodesManager brokerNodesManager;

        /// <summary>
        /// the timeout setting for retring to conect to the scheduler.
        /// </summary>
        private static int ConnectToSchedulerTimeout => int.MaxValue; // 9 * 60 * 1000;

        /// <summary>
        /// Allocate a new session
        /// </summary>
        /// <param name="startInfo">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>the Broker Launcher EPRs, sorted by the preference.</returns>
        public override async Task<SessionAllocateInfoContract> AllocateAsync(SessionStartInfoContract startInfo, string endpointPrefix)
        {
            if (SoaHelper.IsOnAzure())
            {
                if (startInfo.UseInprocessBroker)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Azure_NotSupportInprocessBroker, SR.Azure_NotSupportInprocessBroker);
                }
            }

            return await this.AllocateInternalAsync(startInfo, endpointPrefix, false);
        }

        /// <summary>
        /// Allocate a new durable session
        /// </summary>
        /// <param name="startInfo">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>the Broker Launcher EPRs, sorted by the preference.</returns>
        public override async Task<SessionAllocateInfoContract> AllocateDurableAsync(SessionStartInfoContract startInfo, string endpointPrefix)
        {
            // TODO: on Azure, we don't support durable session in SP3 CTP.
            if (SoaHelper.IsOnAzure())
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Azure_NotSupportDurableSession, SR.Azure_NotSupportDurableSession);
            }

            return await this.AllocateInternalAsync(startInfo, endpointPrefix, true);
        }

        /// <summary>
        /// Attach to an exisiting session (create a session info by the specified service job)
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="endpointPrefix">the prefix of the endpoint epr.</param>
        /// <param name="sessionId">the session id</param>
        /// <param name="useAad">if getting info of an AAD session</param>
        /// <returns>the session information.</returns>
        public override async Task<SessionInfoContract> GetInfoAadAsync(string endpointPrefix, int sessionId, bool useAad)
        {
            using (new SessionIdentityImpersonation(useAad))
            {
                SessionInfoContract sessionInfo = null;
                CheckAccess();

                ParamCheckUtility.ThrowIfNullOrEmpty(endpointPrefix, "endpointPrefix");
                ParamCheckUtility.ThrowIfOutofRange(sessionId <= 0, "sessionId");

                TraceHelper.TraceEvent(
                    sessionId,
                    TraceEventType.Information,
                    "[SessionLauncher] .GetInfo: headnode={0}, endpointPrefix={1}, sessionId={2}",
                    Environment.MachineName,
                    endpointPrefix,
                    sessionId);

                if (!IsEndpointPrefixSupported(endpointPrefix))
                {
                    TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "[SessionLauncher] .GetInfo: {0} is not a supported endpoint prefix.", endpointPrefix);

                    ThrowHelper.ThrowSessionFault(SOAFaultCode.InvalidArgument, SR.SessionLauncher_EndpointNotSupported, endpointPrefix);
                }

                if (this.schedulerConnectState != SchedulerConnectState.ConnectionComplete)
                {
                    TraceHelper.TraceEvent(
                        sessionId,
                        TraceEventType.Information,
                        "[SessionLauncher] .GetInfo: session launcher is not conected to the scheduler, schedulerConnectState={0}",
                        this.schedulerConnectState);

                    ThrowHelper.ThrowSessionFault(SOAFaultCode.ConnectToSchedulerFailure, SR.SessionLauncher_NoConnectionToScheduler, null);
                }

                // TODO: SF: Utilze reliable collection instead of job properties.
                ISchedulerJob schedulerJob = this.OpenSessionJob(sessionId);

                if (schedulerJob != null)
                {
                    TraceHelper.TraceEvent(
                        sessionId,
                        TraceEventType.Information,
                        "[SessionLauncher] .GetInfo: try to get the job properties(Secure, TransportScheme, BrokerEpr, BrokerNode, ControllerEpr, ResponseEpr) for the job, jobid={0}.",
                        sessionId);

                    try
                    {
                        sessionInfo = new SessionInfoContract();
                        sessionInfo.Id = sessionId;
                        sessionInfo.JobState = JobStateConverter.FromHpcJobState(schedulerJob.State);
                        sessionInfo.UseAad = useAad;

                        // Return the owner of the session
                        sessionInfo.SessionOwner = schedulerJob.Owner;

                        string brokerNodeString = null;
                        bool localUser = false;
                        foreach (NameValue pair in schedulerJob.GetCustomProperties())
                        {
                            if (SoaHelper.IsOnAzure() && pair.Name.Equals(BrokerSettingsConstants.ShareSession, StringComparison.OrdinalIgnoreCase))
                            {
                                bool sharedSession = false;

                                if (bool.TryParse(pair.Value, out sharedSession))
                                {
                                    // if the session is shared and we are on Azure
                                    if (sharedSession)
                                    {
                                        // Get its job template
                                        string jobTemplateName = schedulerJob.JobTemplate;
                                        if (string.IsNullOrEmpty(jobTemplateName))
                                        {
                                            jobTemplateName = HpcSchedulerDelegation.DefaultJobTemplateName;
                                        }

                                        // Get its template's ACL
                                        JobTemplateInfo info = this.scheduler.GetJobTemplateInfo(jobTemplateName);

                                        // Return list of granted and denied users by name to REST service so it can 
                                        // authorize use of session
                                        sessionInfo.SessionACL = SoaHelper.GetGrantedUsers(info.SecurityDescriptor, (uint)JobTemplateRights.SubmitJob);
                                    }
                                }
                            }
                            else if (pair.Name.Equals(BrokerSettingsConstants.Secure, StringComparison.OrdinalIgnoreCase))
                            {
                                string secureString = pair.Value;
                                Debug.Assert(secureString != null, "BrokerSettingsConstants.Secure value should be a string.");
                                bool secure;
                                if (bool.TryParse(secureString, out secure))
                                {
                                    sessionInfo.Secure = secure;
                                    TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .GetInfo: get the job secure property, Secure={0}.", secure);
                                }
                                else
                                {
                                    TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "Illegal secure value[{0}] for job's " + BrokerSettingsConstants.Secure + " property", secureString);
                                }
                            }
                            else if (pair.Name.Equals(BrokerSettingsConstants.TransportScheme, StringComparison.OrdinalIgnoreCase))
                            {
                                string schemeString = pair.Value;
                                Debug.Assert(schemeString != null, "BrokerSettingsConstants.TransportScheme value should be a string.");

                                int scheme;
                                if (int.TryParse(schemeString, out scheme))
                                {
                                    sessionInfo.TransportScheme = (TransportScheme)scheme;
                                    TraceHelper.TraceEvent(
                                        sessionId,
                                        TraceEventType.Information,
                                        "[SessionLauncher] .GetInfo: get the job TransportScheme property, TransportScheme={0}.",
                                        sessionInfo.TransportScheme);
                                }
                                else
                                {
                                    TraceHelper.TraceEvent(
                                        sessionId,
                                        TraceEventType.Error,
                                        "Illegal transport scheme value[{0}] for job's " + BrokerSettingsConstants.TransportScheme + " property",
                                        schemeString);
                                }
                            }
                            else if (pair.Name.Equals(BrokerSettingsConstants.BrokerNode, StringComparison.OrdinalIgnoreCase))
                            {
                                brokerNodeString = pair.Value;
                                Debug.Assert(brokerNodeString != null, "BrokerSettingsConstants.BrokerNode value should be a string.");

                                TraceHelper.TraceEvent(
                                    sessionId,
                                    TraceEventType.Information,
                                    "[SessionLauncher] .GetInfo: get the job BrokerLauncherEpr property, BrokerLauncherEpr={0}.",
                                    sessionInfo.BrokerLauncherEpr);
                            }
                            else if (pair.Name.Equals(BrokerSettingsConstants.LocalUser, StringComparison.OrdinalIgnoreCase))
                            {
                                string localUserString = pair.Value;
                                Debug.Assert(localUserString != null, "BrokerSettingsConstants.LocalUser value should be a string.");
                                if (bool.TryParse(localUserString, out localUser))
                                {
                                    TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .GetInfo: get the job LocalUser property, LocalUser={0}.", localUser);
                                }
                                else
                                {
                                    TraceHelper.TraceEvent(
                                        sessionId,
                                        TraceEventType.Error,
                                        "Illegal LocalUser value[{0}] for job's " + BrokerSettingsConstants.LocalUser + " property",
                                        localUserString);
                                }
                            }
                            else if (pair.Name.Equals(BrokerSettingsConstants.Durable, StringComparison.OrdinalIgnoreCase))
                            {
                                string durableString = pair.Value;
                                Debug.Assert(durableString != null, "BrokerSettingsConstants.Durable value should be a string.");

                                bool durable;
                                if (bool.TryParse(durableString, out durable))
                                {
                                    sessionInfo.Durable = durable;
                                    TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .GetInfo: get the job Durable property, Durable={0}.", sessionInfo.Durable);
                                }
                                else
                                {
                                    TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "Illegal secure value[{0}] for job's " + BrokerSettingsConstants.Durable + " property", durableString);
                                }
                            }
                            else if (pair.Name.Equals(BrokerSettingsConstants.ServiceVersion, StringComparison.OrdinalIgnoreCase))
                            {
                                if (pair.Value != null)
                                {
                                    try
                                    {
                                        sessionInfo.ServiceVersion = new Version(pair.Value);
                                        TraceHelper.TraceEvent(
                                            sessionId,
                                            TraceEventType.Information,
                                            "[SessionLauncher] .GetInfo: get the job ServiceVersion property, ServiceVersion={0}.",
                                            (sessionInfo.ServiceVersion != null) ? sessionInfo.ServiceVersion.ToString() : string.Empty);
                                    }
                                    catch (Exception e)
                                    {
                                        TraceHelper.TraceEvent(
                                            sessionId,
                                            TraceEventType.Error,
                                            "Illegal secure value[{0}] for job's " + BrokerSettingsConstants.ServiceVersion + " property. Exception = {1}",
                                            pair.Value,
                                            e);
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(brokerNodeString))
                        {
                            if (brokerNodeString != Constant.InprocessBrokerNode)
                            {
                                // TODO: it's better to support Microsoft.Hpc.Scheduler.Session.ChannelTypes here instead of counting on boolean(s).
                                if (useAad)
                                {
                                    sessionInfo.BrokerLauncherEpr = BrokerNodesManager.GenerateBrokerLauncherAadEpr(endpointPrefix, brokerNodeString);
                                }
                                else if (!localUser)
                                {
                                    sessionInfo.BrokerLauncherEpr = BrokerNodesManager.GenerateBrokerLauncherEpr(endpointPrefix, brokerNodeString, sessionInfo.TransportScheme);
                                }
                                else
                                {
                                    sessionInfo.BrokerLauncherEpr = BrokerNodesManager.GenerateBrokerLauncherInternalEpr(endpointPrefix, brokerNodeString);
                                }
                            }
                        }
                        else
                        {
                            sessionInfo.UseInprocessBroker = true;
                        }

                        TraceHelper.TraceEvent(
                            sessionId,
                            TraceEventType.Information,
                            "[SessionLauncher] .GetInfo: get the job BrokerLauncherEpr property, BrokerLauncherEpr={0}.",
                            sessionInfo.BrokerLauncherEpr);
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "[SessionLauncher] .GetInfo: Failed to get all properties from job[{0}], Exception:{1}", sessionId, e);

                        ThrowHelper.ThrowSessionFault(SOAFaultCode.GetJobPropertyFailure, SR.SessionLauncher_FailToGetJobProperty, e.ToString());
                    }

                    SimulateFailure.FailOperation(1);
                }

                TraceHelper.TraceEvent(
                    sessionId,
                    TraceEventType.Information,
                    "[SessionLauncher] .GetInfo: return the sessionInfo, BrokerEpr={0}, BrokerLauncherEpr={1}, ControllerEpr={2}, Id={3}, JobState={4}, ResponseEpr={5}, Secure={6}, TransportScheme={7}.",
                    sessionInfo.BrokerEpr,
                    sessionInfo.BrokerLauncherEpr,
                    sessionInfo.ControllerEpr,
                    sessionInfo.Id,
                    sessionInfo.JobState,
                    sessionInfo.ResponseEpr,
                    sessionInfo.Secure,
                    sessionInfo.TransportScheme);
                return await Task.FromResult(sessionInfo);
            }
        }

        /// <summary>
        /// Allocate a new session
        /// </summary>
        /// <param name="startInfo">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>the Broker Launcher EPRs, sorted by the preference.</returns>
        public override string[] Allocate(SessionStartInfoContract startInfo, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo)
        {
            SessionAllocateInfoContract contract = this.AllocateInternalAsync(startInfo, endpointPrefix, false).GetAwaiter().GetResult();
            sessionid = contract.Id;
            serviceVersion = contract.ServiceVersion?.ToString();
            sessionInfo = contract.SessionInfo;
            return contract.BrokerLauncherEpr;
        }

        /// <summary>
        /// Allocate a new durable session
        /// </summary>
        /// <param name="startInfo">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>the Broker Launcher EPRs, sorted by the preference.</returns>
        public override string[] AllocateDurable(SessionStartInfoContract startInfo, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo)
        {
            SessionAllocateInfoContract contract = this.AllocateInternalAsync(startInfo, endpointPrefix, true).GetAwaiter().GetResult();
            sessionid = contract.Id;
            serviceVersion = contract.ServiceVersion?.ToString();
            sessionInfo = contract.SessionInfo;
            return contract.BrokerLauncherEpr;
        }

        /// <summary>
        /// To get the info for the existing session from the session job 
        /// </summary>
        /// <param name="headnode">the head node</param>
        /// <param name="endpointPrefix">the epr prefix for the broker launcher</param>
        /// <param name="sessionId">the session job id</param>
        /// <param name="schedulerJob">the scheduler job</param>
        /// <returns>the session info</returns>
        private SessionInfoContract GetInfo(string headnode, string endpointPrefix, int sessionId, ISchedulerJob schedulerJob)
        {
            SessionInfoContract sessionInfo = null;
            CheckAccess();

            ParamCheckUtility.ThrowIfNullOrEmpty(endpointPrefix, "endpointPrefix");
            ParamCheckUtility.ThrowIfOutofRange(sessionId <= 0, "sessionId");

            TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .GetInfo: headnode={0}, endpointPrefix={1}, sessionId={2}", headnode, endpointPrefix, sessionId);

            if (!IsEndpointPrefixSupported(endpointPrefix))
            {
                TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "[SessionLauncher] .GetInfo: {0} is not a supported endpoint prefix.", endpointPrefix);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.InvalidArgument, SR.SessionLauncher_EndpointNotSupported, endpointPrefix);
            }

            if (this.schedulerConnectState != SchedulerConnectState.ConnectionComplete)
            {
                TraceHelper.TraceEvent(
                    sessionId,
                    TraceEventType.Information,
                    "[SessionLauncher] .GetInfo: session launcher is not conected to the scheduler, schedulerConnectState={0}",
                    this.schedulerConnectState);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.ConnectToSchedulerFailure, SR.SessionLauncher_NoConnectionToScheduler, null);
            }

            // ISchedulerJob schedulerJob = this.OpenSessionJob(sessionId);
            if (schedulerJob != null)
            {
                TraceHelper.TraceEvent(
                    sessionId,
                    TraceEventType.Information,
                    "[SessionLauncher] .GetInfo: try to get the job properties(Secure, TransportScheme, BrokerEpr, BrokerNode, ControllerEpr, ResponseEpr) for the job, jobid={0}.",
                    sessionId);

                try
                {
                    sessionInfo = new SessionInfoContract();
                    sessionInfo.Id = sessionId;
                    sessionInfo.JobState = JobStateConverter.FromHpcJobState(schedulerJob.State);

                    // Return the owner of the session
                    sessionInfo.SessionOwner = schedulerJob.Owner;

                    string brokerNodeString = null;
                    bool localUser = false;
                    foreach (NameValue pair in schedulerJob.GetCustomProperties())
                    {
                        if (SoaHelper.IsOnAzure() && pair.Name.Equals(BrokerSettingsConstants.ShareSession, StringComparison.OrdinalIgnoreCase))
                        {
                            bool sharedSession = false;

                            if (bool.TryParse(pair.Value, out sharedSession))
                            {
                                // if the session is shared and we are on Azure
                                if (sharedSession)
                                {
                                    // Get its job template
                                    string jobTemplateName = schedulerJob.JobTemplate;
                                    if (string.IsNullOrEmpty(jobTemplateName))
                                    {
                                        jobTemplateName = HpcSchedulerDelegation.DefaultJobTemplateName;
                                    }

                                    // Get its template's ACL
                                    JobTemplateInfo info = this.scheduler.GetJobTemplateInfo(jobTemplateName);

                                    // Return list of granted and denied users by name to REST service so it can 
                                    // authorize use of session
                                    sessionInfo.SessionACL = SoaHelper.GetGrantedUsers(info.SecurityDescriptor, (uint)JobTemplateRights.SubmitJob);
                                }
                            }
                        }
                        else if (pair.Name.Equals(BrokerSettingsConstants.Secure, StringComparison.OrdinalIgnoreCase))
                        {
                            string secureString = pair.Value;
                            Debug.Assert(secureString != null, "BrokerSettingsConstants.Secure value should be a string.");
                            bool secure;
                            if (bool.TryParse(secureString, out secure))
                            {
                                sessionInfo.Secure = secure;
                                TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .GetInfo: get the job secure property, Secure={0}.", secure);
                            }
                            else
                            {
                                TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "Illegal secure value[{0}] for job's " + BrokerSettingsConstants.Secure + " property", secureString);
                            }
                        }
                        else if (pair.Name.Equals(BrokerSettingsConstants.TransportScheme, StringComparison.OrdinalIgnoreCase))
                        {
                            string schemeString = pair.Value;
                            Debug.Assert(schemeString != null, "BrokerSettingsConstants.TransportScheme value should be a string.");

                            int scheme;
                            if (int.TryParse(schemeString, out scheme))
                            {
                                sessionInfo.TransportScheme = (TransportScheme)scheme;
                                TraceHelper.TraceEvent(
                                    sessionId,
                                    TraceEventType.Information,
                                    "[SessionLauncher] .GetInfo: get the job TransportScheme property, TransportScheme={0}.",
                                    sessionInfo.TransportScheme);
                            }
                            else
                            {
                                TraceHelper.TraceEvent(
                                    sessionId,
                                    TraceEventType.Error,
                                    "Illegal transport scheme value[{0}] for job's " + BrokerSettingsConstants.TransportScheme + " property",
                                    schemeString);
                            }
                        }
                        else if (pair.Name.Equals(BrokerSettingsConstants.BrokerNode, StringComparison.OrdinalIgnoreCase))
                        {
                            brokerNodeString = pair.Value;
                            Debug.Assert(brokerNodeString != null, "BrokerSettingsConstants.BrokerNode value should be a string.");

                            TraceHelper.TraceEvent(
                                sessionId,
                                TraceEventType.Information,
                                "[SessionLauncher] .GetInfo: get the job BrokerLauncherEpr property, BrokerLauncherEpr={0}.",
                                sessionInfo.BrokerLauncherEpr);
                        }
                        else if (pair.Name.Equals(BrokerSettingsConstants.LocalUser, StringComparison.OrdinalIgnoreCase))
                        {
                            string localUserString = pair.Value;
                            Debug.Assert(localUserString != null, "BrokerSettingsConstants.LocalUser value should be a string.");
                            if (bool.TryParse(localUserString, out localUser))
                            {
                                TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .GetInfo: get the job LocalUser property, LocalUser={0}.", localUser);
                            }
                            else
                            {
                                TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "Illegal LocalUser value[{0}] for job's " + BrokerSettingsConstants.LocalUser + " property", localUserString);
                            }
                        }
                        else if (pair.Name.Equals(BrokerSettingsConstants.Durable, StringComparison.OrdinalIgnoreCase))
                        {
                            string durableString = pair.Value;
                            Debug.Assert(durableString != null, "BrokerSettingsConstants.Durable value should be a string.");

                            bool durable;
                            if (bool.TryParse(durableString, out durable))
                            {
                                sessionInfo.Durable = durable;
                                TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .GetInfo: get the job Durable property, Durable={0}.", sessionInfo.Durable);
                            }
                            else
                            {
                                TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "Illegal secure value[{0}] for job's " + BrokerSettingsConstants.Durable + " property", durableString);
                            }
                        }
                        else if (pair.Name.Equals(BrokerSettingsConstants.ServiceVersion, StringComparison.OrdinalIgnoreCase))
                        {
                            if (pair.Value != null)
                            {
                                // TODO: Use Parse in .Net 4.0
                                try
                                {
                                    sessionInfo.ServiceVersion = new Version(pair.Value);
                                    TraceHelper.TraceEvent(
                                        sessionId,
                                        TraceEventType.Information,
                                        "[SessionLauncher] .GetInfo: get the job ServiceVersion property, ServiceVersion={0}.",
                                        (sessionInfo.ServiceVersion != null) ? sessionInfo.ServiceVersion.ToString() : string.Empty);
                                }
                                catch (Exception e)
                                {
                                    TraceHelper.TraceEvent(
                                        sessionId,
                                        TraceEventType.Error,
                                        "Illegal secure value[{0}] for job's " + BrokerSettingsConstants.ServiceVersion + " property. Exception = {1}",
                                        pair.Value,
                                        e);
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(brokerNodeString))
                    {
                        if (brokerNodeString != Constant.InprocessBrokerNode)
                        {
                            sessionInfo.BrokerLauncherEpr =
                                localUser
                                    ? BrokerNodesManager.GenerateBrokerLauncherInternalEpr(endpointPrefix, brokerNodeString)
                                    : BrokerNodesManager.GenerateBrokerLauncherEpr(endpointPrefix, brokerNodeString, sessionInfo.TransportScheme);
                        }
                        else
                        {
                            sessionInfo.UseInprocessBroker = true;
                        }

                        TraceHelper.TraceEvent(
                            sessionId,
                            TraceEventType.Information,
                            "[SessionLauncher] .GetInfo: get the job BrokerLauncherEpr property, BrokerLauncherEpr={0}.",
                            sessionInfo.BrokerLauncherEpr);
                    }
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "[SessionLauncher] .GetInfo: Failed to get all properties from job[{0}], Exception:{1}", sessionId, e);

                    ThrowHelper.ThrowSessionFault(SOAFaultCode.GetJobPropertyFailure, SR.SessionLauncher_FailToGetJobProperty, e.ToString());
                }

                SimulateFailure.FailOperation(1);
            }

            TraceHelper.TraceEvent(
                sessionId,
                TraceEventType.Information,
                "[SessionLauncher] .GetInfo: return the sessionInfo, BrokerEpr={0}, BrokerLauncherEpr={1}, ControllerEpr={2}, Id={3}, JobState={4}, ResponseEpr={5}, Secure={6}, TransportScheme={7}.",
                sessionInfo.BrokerEpr,
                sessionInfo.BrokerLauncherEpr,
                sessionInfo.ControllerEpr,
                sessionInfo.Id,
                sessionInfo.JobState,
                sessionInfo.ResponseEpr,
                sessionInfo.Secure,
                sessionInfo.TransportScheme);
            return sessionInfo;
        }

        /// <summary>
        /// terminate a session (cancel the service job specified by the id)
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="sessionId">the session id</param>
        public override async Task TerminateAsync(int sessionId)
        {
            using (GetCallerWindowsIdentity().Impersonate())
            {
                CheckAccess();

                TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] .Terminate: try to terminate the session[id={0}].", sessionId);

                if (this.schedulerConnectState != SchedulerConnectState.ConnectionComplete)
                {
                    TraceHelper.TraceEvent(
                        TraceEventType.Information,
                        "[SessionLauncher] .Terminate: session launcher is not conected to the scheduler, schedulerConnectState={0}",
                        this.schedulerConnectState);

                    throw new SessionException(SR.SessionLauncher_NoConnectionToScheduler, null);
                }

                ISchedulerJob schedulerJob = this.OpenSessionJob(sessionId);

                TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .Terminate: Try to get the job state for the job[id={0}].", sessionId);

                JobState jobState = JobStateConverter.FromHpcJobState(schedulerJob.State);
                if ((jobState & JobState.Canceled) != 0 || (jobState & JobState.Failed) != 0 || (jobState & JobState.Finished) != 0 || (jobState & JobState.Finishing) != 0)
                {
                    return;
                }

                TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .Terminate: cancel the job[id={0}].", sessionId);

                this.scheduler.CancelJob(schedulerJob.Id, "The client tries to terminate the session");

                await Task.CompletedTask;

                SimulateFailure.FailOperation(1);
            }
        }

        /// <summary>
        /// Returns the versions for a specific service
        /// </summary>
        /// <param name="headNode">headnode of cluster to conect to </param>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <returns>Available service versions</returns>
        public override async Task<Version[]> GetServiceVersionsAsync(string serviceName)
        {
            using (GetCallerWindowsIdentity().Impersonate())
            {
#pragma warning disable 618 // disable obsolete warning for UserPrivilege
                UserPrivilege userPrivilege = this.scheduler.GetUserPrivilege();

                if (userPrivilege == UserPrivilege.AccessDenied)
                {
                    throw new SecurityException();
                }

#pragma warning restore 618 // disable obsolete warning for UserPrivilege

                // Get the versioned services
                return await Task.FromResult(this.GetServiceVersionsInternal(serviceName, true));
            }
        }

        /// <summary>
        /// Gets SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <returns>returns the value</returns>
        /// <remarks>
        /// This operation could be called by both client (with user's credential) and broker/broker worker
        /// (with broker node's machine account).
        /// If it is called by client (user), we need to impersonate the caller and let scheduler API to
        /// authenticate it.
        /// If it is called by broker/broker worker (broker node's machine account), we do not impersonate
        /// the caller since broker node's machine account is not HpcUser or HpcAdmin.
        /// </remarks>
        public override async Task<string> GetSOAConfigurationAsync(string key)
        {
            WindowsImpersonationContext impersonationContext = null;
            TraceHelper.TraceInfo(0, "[SessionLauncher] .GetSOAConfiguration: Get configuration for key {0}.", key);

            if (!SoaHelper.IsOnAzure())
            {
                if (OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.AuthByKerberosOrNtlmOrBasic())
                {
                    WindowsIdentity identity = OperationContext.Current.ServiceSecurityContext.WindowsIdentity;
                    if (!AuthenticationHelper.IsBrokerNode(identity, this.brokerNodesManager.IsBrokerNode))
                    {
                        impersonationContext = identity.Impersonate();
                    }
                }
            }

            try
            {
                string value;
                switch (key)
                {
                    case Constant.AutomaticShrinkEnabled:
                        string schedulingMode = JobHelper.GetClusterParameterValue(this.scheduler, Constant.SchedulingMode, Constant.SchedulingMode_Balanced);
                        value = true.ToString();

                        // If balanced scheduling isnt used, get AutomaticShrinkEnabled clusparam. Otherwise default AutomaticShrinkEnabled to true
                        if (0 != string.Compare(schedulingMode, Constant.SchedulingMode_Balanced, true))
                        {
                            value = JobHelper.GetClusterParameterValue(this.scheduler, Constant.AutomaticShrinkEnabled, true.ToString());
                        }

                        break;

                    case Constant.HpcSoftCardTemplateParam:
                    case Constant.HpcSoftCard:
                    case Constant.DisableCredentialReuse:
                    case Constant.NettcpOver443:
                        value = JobHelper.GetClusterParameterValue(this.scheduler, key, string.Empty);
                        break;

                    default:
                        // Get other configurations from ClusterEnvironments
                        value = JobHelper.GetEnvironmentVariable(this.scheduler, key);
                        break;
                }

                TraceHelper.TraceInfo(0, "[SessionLauncher] .GetSOAConfiguration: Value for key {0} is {1}.", key, value);
                return await Task.FromResult(value);
            }
            catch (Exception e)
            {
                TraceHelper.TraceError(0, "[SessionLauncher] .GetSOAConfiguartion: Failed to get configuration for key {0}. Exception = {1}", key, e);
                throw;
            }
            finally
            {
                if (impersonationContext != null)
                {
                    try
                    {
                        impersonationContext.Undo();
                    }
                    catch (Exception ex)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[SessionLauncher].GetSOAConfiguration: Exception {0}", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Gets SOA configuration
        /// </summary>
        /// <param name="keys">indicating the keys</param>
        /// <returns>returns the values</returns>
        public override async Task<Dictionary<string, string>> GetSOAConfigurationsAsync(List<string> keys)
        {
            WindowsImpersonationContext impersonationContext = null;
            TraceHelper.TraceInfo(0, "[SessionLauncher] .GetSOAConfiguration: Get configuration for keys {0}.", string.Join(",", keys));

            if (!SoaHelper.IsOnAzure())
            {
                if (OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.AuthByKerberosOrNtlmOrBasic())
                {
                    WindowsIdentity identity = OperationContext.Current.ServiceSecurityContext.WindowsIdentity;
                    if (!AuthenticationHelper.IsBrokerNode(identity, this.brokerNodesManager.IsBrokerNode))
                    {
                        impersonationContext = identity.Impersonate();
                    }
                }
            }

            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                foreach (string key in keys)
                {
                    string value;
                    switch (key)
                    {
                        case Constant.AutomaticShrinkEnabled:
                            string schedulingMode = JobHelper.GetClusterParameterValue(this.scheduler, Constant.SchedulingMode, Constant.SchedulingMode_Balanced);
                            value = true.ToString();

                            // If balanced scheduling isnt used, get AutomaticShrinkEnabled clusparam. Otherwise default AutomaticShrinkEnabled to true
                            if (0 != string.Compare(schedulingMode, Constant.SchedulingMode_Balanced, true))
                            {
                                value = JobHelper.GetClusterParameterValue(this.scheduler, Constant.AutomaticShrinkEnabled, true.ToString());
                            }

                            break;

                        case Constant.HpcSoftCardTemplateParam:
                        case Constant.HpcSoftCard:
                        case Constant.DisableCredentialReuse:
                        case Constant.NettcpOver443:
                            value = JobHelper.GetClusterParameterValue(this.scheduler, key, string.Empty);
                            break;

                        default:
                            // Get other configurations from ClusterEnvironments
                            value = JobHelper.GetEnvironmentVariable(this.scheduler, key);
                            break;
                    }

                    result.Add(key, value);
                    TraceHelper.TraceInfo(0, "[SessionLauncher] .GetSOAConfiguration: Value for key {0} is {1}.", key, value);
                }

                return await Task.FromResult(result);
            }
            catch (Exception e)
            {
                TraceHelper.TraceError(0, "[SessionLauncher] .GetSOAConfiguartion: Failed to get configuration for keys {0}. Exception = {1}", string.Join(",", keys), e);
                throw;
            }
            finally
            {
                if (impersonationContext != null)
                {
                    try
                    {
                        impersonationContext.Undo();
                    }
                    catch (Exception ex)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[SessionLauncher].GetSOAConfiguration: Exception {0}", ex);
                    }
                }
            }
        }

        protected override bool TryGetSessionAllocateInfoFromPooled(
            string endpointPrefix,
            bool durable,
            SessionAllocateInfoContract sessionAllocateInfo,
            string serviceConfigFile,
            ServiceRegistration registration,
            out SessionAllocateInfoContract allocateInternal)
        {
            allocateInternal = null;
            ISchedulerJob sessionJob = null;
            sessionAllocateInfo.Id = this.PickSessionIdFromPool(Path.GetFileNameWithoutExtension(serviceConfigFile), durable, registration.Service.MaxSessionPoolSize, out sessionJob);

            if (sessionAllocateInfo.Id > 0)
            {
                // better to getinfo here to eliminate the second call.
                sessionAllocateInfo.SessionInfo = this.GetInfo(string.Empty, endpointPrefix, sessionAllocateInfo.Id, sessionJob);
                {
                    allocateInternal = sessionAllocateInfo;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// release resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.dataService != null)
                {
                    try
                    {
                        this.dataService.Dispose();
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "Failed to dispose the data service - {0}", e);
                    }

                    this.dataService = null;
                }

                if (this.scheduler != null)
                {
                    try
                    {
                        this.scheduler.Close();
                        this.scheduler.Dispose();
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "Failed to dispose the scheduler - {0}", e);
                    }

                    this.scheduler = null;
                }

                if (this.brokerNodesManager != null)
                {
                    try
                    {
                        this.brokerNodesManager.Dispose();
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "Failed to dispose the broker node manager - {0}", e);
                    }

                    this.brokerNodesManager = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// connect to the scheduler to create a store
        /// create a jobMonitor and brokerNodeManager
        /// </summary>
        private async Task InitializeSchedulerConnect()
        {
            if (this.schedulerConnectState != SchedulerConnectState.ConnectionComplete)
            {
                if ((this.schedulerConnectState & SchedulerConnectState.ConnectedToSchedulerStore) != SchedulerConnectState.ConnectedToSchedulerStore)
                {
                    try
                    {
                        TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] .InitializeSchedulerConnect: Try to connect to the head node fabric connection string: null");

                        if (this.scheduler != null)
                        {
                            this.scheduler.Dispose();
                        }

                        this.scheduler = await CommonSchedulerHelper.GetScheduler(HpcContext.Get().CancellationToken);
                        this.schedulerConnectState |= SchedulerConnectState.ConnectedToSchedulerStore;
                        this.scheduler.SetInterfaceMode(false, new IntPtr(-1));
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .InitializeSchedulerConnect: Failed to connect to the scheduler store, Exception:{0}", e);

                        throw new SessionException(SR.CannotConnectToScheduler, e);
                    }
                }

                if ((this.schedulerConnectState & SchedulerConnectState.ConnectedToSchedulerStore) == SchedulerConnectState.ConnectedToSchedulerStore
                    && (this.schedulerConnectState & SchedulerConnectState.StartedJobMonitor) != SchedulerConnectState.StartedJobMonitor)
                {
                    try
                    {
                        TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] .InitializeSchedulerConnect: Try to create the job monitor.");

                        this.schedulerConnectState |= SchedulerConnectState.StartedJobMonitor;
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .InitializeSchedulerConnect: Failed to connect to the scheduler for job monitor, Exception:{0}", e);

                        throw new SessionException(SR.SessionLauncher_FailToConnectToScheduler, e);
                    }
                }

                if ((this.schedulerConnectState & SchedulerConnectState.ConnectedToSchedulerStore) == SchedulerConnectState.ConnectedToSchedulerStore
                    && (this.schedulerConnectState & SchedulerConnectState.StartedBrokerNodesManager) != SchedulerConnectState.StartedBrokerNodesManager)
                {
                    try
                    {
                        TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] .InitializeSchedulerConnect: Try to initialize the broker nodes manager.");

                        // this.brokerNodesManager = BrokerNodesManager.Instance;
                        this.schedulerConnectState |= SchedulerConnectState.StartedBrokerNodesManager;
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .InitializeSchedulerConnect: Failed to create broker nodes manager, Exception:{0}", e);

                        throw new SessionException(SR.SessionLauncher_FailToCreateBrokerManager, e);
                    }
                }

                if ((this.schedulerConnectState & ~SchedulerConnectState.Connecting) == SchedulerConnectState.ConnectionComplete)
                {
                    this.schedulerConnectState = SchedulerConnectState.ConnectionComplete;

                    TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] .InitializeSchedulerConnect: Finished connecting to the nead node: null");
                }
            }
        }

        /// <summary>
        /// if the RegistrationPath doesn't exist, throws an exception with proper error code
        /// </summary>
        protected override ServiceRegistrationRepo GetRegistrationRepo(string callId)
        {
            ServiceRegistrationRepo repo = null;
            if (SoaHelper.IsOnAzure())
            {
                string ccpPackageRoot = SoaHelper.GetCcpPackageRoot();

                TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .GetRegistrationRepo: CCP_PACKAGE_ROOT = {0}", ccpPackageRoot);
#if HPCPACK
                repo = new ServiceRegistrationRepo(ccpPackageRoot, HpcContext.Get().GetServiceRegistrationStore());
#else
                repo = new ServiceRegistrationRepo(ccpPackageRoot);
#endif
            }
            else
            {
                string regPath = null;

                try
                {
                    // TODO: set it when session service starts
                    regPath = JobHelper.GetEnvironmentVariable(this.scheduler, Constant.RegistryPathEnv);
                    if (!string.IsNullOrEmpty(SessionLauncherSettings.Default.ServiceRegistrationPath))
                    {
                        regPath = SessionLauncherSettings.Default.ServiceRegistrationPath + ";" + regPath;
                    }
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .GetRegistrationRepo: callId={0}, Get the scheduler environment failed. exception = {1}", callId, e);

                    ThrowHelper.ThrowSessionFault(SOAFaultCode.GetClusterPropertyFailure, SR.SessionLauncher_FailToGetClusterProperty, e.ToString());
                }

                if (!string.IsNullOrEmpty(regPath))
                {
#if HPCPACK
                    repo = new ServiceRegistrationRepo(regPath, HpcContext.Get().GetServiceRegistrationStore());
#else
                    repo = new ServiceRegistrationRepo(regPath);
#endif
                }
                else
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .GetRegistrationRepo: callId={0}, Get the scheduler environment for RegistryPathEnv is empty or null.", callId);
                }
            }

            if (repo != null && repo.GetServiceRegistrationDirectories() != null)
            {
                return repo;
            }

            TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .GetRegistrationRepo: No service registration directories are configured");

            ThrowHelper.ThrowSessionFault(SOAFaultCode.Service_RegistrationDirsMissing, SR.SessionLauncher_NoServiceRegistrationDirs);

            return null;
        }

        /// <summary>
        /// Open the session job.
        /// </summary>
        /// <param name="sessionId">session job id</param>
        /// <returns>scheduler job instance</returns>
        private ISchedulerJob OpenSessionJob(int sessionId)
        {
            ISchedulerJob schedulerJob = null;

            try
            {
                TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[SessionLauncher] .OpenSessionJob: Try to open the job[{0}].", sessionId);

                schedulerJob = this.scheduler.OpenJob(sessionId);
            }
            catch (SchedulerException e)
            {
                TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "[SessionLauncher] .OpenSessionJob: Failed to open job[{0}], Exception:{1}", sessionId, e.ToString());

                if (e.Code == ErrorCode.Operation_InvalidJobId)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.InvalidSessionId, SR.SessionLauncher_FailToOpenJob, e.ToString());
                }
                else
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.OpenJobFailure, SR.SessionLauncher_FailToOpenJob, e.ToString());
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "[SessionLauncher] .OpenSessionJob: Failed to open job[{0}], Exception:{1}", sessionId, e.ToString());

                ThrowHelper.ThrowSessionFault(SOAFaultCode.OpenJobFailure, SR.SessionLauncher_FailToOpenJob, e.ToString());
            }

            return schedulerJob;
        }

        /// <summary>
        /// Build the session pool by querying the running jobs.
        /// </summary>
        private void ConstructSessionPool()
        {
            PropertyIdCollection props = new PropertyIdCollection();
            props.Add(JobPropertyIds.Id);

            FilterCollection filters = new FilterCollection();
            filters.Add(FilterOperator.Equal, JobPropertyIds.State, JobState.Running);
            filters.Add(FilterOperator.IsNotNull, JobPropertyIds.ServiceName, null);

            SortCollection sorts = new SortCollection();
            sorts.Add(SortProperty.SortOrder.Ascending, JobPropertyIds.Id);

            using (ISchedulerRowEnumerator rows = this.scheduler.OpenJobEnumerator(props, filters, sorts))
            {
                foreach (PropertyRow row in rows)
                {
                    int id = (int)row[JobPropertyIds.Id].Value;

                    ISchedulerJob job = this.scheduler.OpenJob(id);

                    Dictionary<string, string> dic = JobHelper.GetEnvironmentVariables(job);
                    bool? pool = (bool?)JobHelper.GetValue<bool?>(dic, Constant.ServiceUseSessionPoolEnvVar);

                    string configFileName = (string)JobHelper.GetValue<string>(dic, Constant.ServiceConfigFileNameEnvVar);
                    string serviceNameWithVersion = null;
                    if (!string.IsNullOrEmpty(configFileName))
                    {
                        serviceNameWithVersion = Path.GetFileNameWithoutExtension(configFileName);
                    }

                    dic = JobHelper.GetCustomizedProperties(job, BrokerSettingsConstants.Durable);
                    bool? durable = (bool?)JobHelper.GetValue<bool?>(dic, BrokerSettingsConstants.Durable);

                    if (!string.IsNullOrEmpty(serviceNameWithVersion) && pool.HasValue && pool.Value && durable.HasValue)
                    {
                        Dictionary<string, SessionPool> dictionary = null;
                        if (durable.Value)
                        {
                            dictionary = this.durableSessionPool;
                        }
                        else
                        {
                            dictionary = this.nonDurableSessionPool;
                        }

                        SessionPool sp;
                        if (!dictionary.TryGetValue(serviceNameWithVersion, out sp))
                        {
                            sp = new SessionPool();
                            dictionary[serviceNameWithVersion] = sp;
                        }

                        sp.SessionIds.Add(id);
                    }
                }
            }
        }

        /// <summary>
        /// Get an id of a running session from the pool.
        /// If the session job is not running, remove it from the pool.
        /// </summary>
        /// <param name="serviceNameWithVersion">ServiceName_Version</param>
        /// <param name="durable">is the session durable</param>
        /// <param name="poolSize">the max number of sessions in the pool</param>
        /// <param name="sessionJob">the session job</param>
        /// <returns>session id, return 0 if it doesn't exist</returns>
        private int PickSessionIdFromPool(string serviceNameWithVersion, bool durable, int poolSize, out ISchedulerJob sessionJob)
        {
            Dictionary<string, SessionPool> dictionary = durable ? this.durableSessionPool : this.nonDurableSessionPool;
            Debug.Assert(dictionary != null, "the session pool is null");

            SessionPool sp = null;
            int jobid = 0;
            sessionJob = null;

            while (true)
            {
                lock (dictionary)
                {
                    if (sp == null && !dictionary.TryGetValue(serviceNameWithVersion, out sp))
                    {
                        sp = new SessionPool();
                        dictionary[serviceNameWithVersion] = sp;
                    }

                    if (sp.Length > 0 && jobid > 0)
                    {
                        // remove the jobid if the job is not in running + prerunning state.
                        // no exception happens if jobid doesn't exist any more.
                        int jobIndex = sp.SessionIds.IndexOf(jobid);
                        sp.RemoveAt(jobIndex);
                    }

                    if (sp.Length + sp.Preparing >= poolSize)
                    {
                        if (sp.Length > 0)
                        {
                            // DONE: (Session Pool, post v3sp2) Need a config setting to specify the pool size; Get a session in round robin, probably load balancing way next
                            // from v3sp2 to v4sp5rtm, only have one session in the pool
                            sp.Index %= Math.Min(sp.Length, poolSize);
                            jobid = sp[sp.Index++];
                            sp.Index %= Math.Min(sp.Length, poolSize);

                            TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .PickSessionIdFromPool: Pick the session {0} in the pool. Next index is {1}", jobid, sp.Index);
                        }
                        else
                        {
                            // need to wait for the sessions ready
                            jobid = -1;
                            sp.PoolChangeEvent.Reset();
                        }
                    }
                    else
                    {
                        jobid = 0;
                        sp.Preparing++;
                    }
                }

                if (jobid > 0)
                {
                    try
                    {
                        ISchedulerJob job = this.scheduler.OpenJob(jobid);
                        if ((job.State
                             & (Properties.JobState.Configuring
                                | Properties.JobState.Submitted
                                | Properties.JobState.Validating
                                | Properties.JobState.ExternalValidation
                                | Properties.JobState.Queued
                                | Properties.JobState.Running))
                            != 0)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .PickSessionIdFromPool: Find the session {0} in the pool.", jobid);
                            sessionJob = job;
                            return jobid;
                        }
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .PickSessionIdFromPool: Failed to get the state of job {0}. {1}", jobid, e.ToString());
                    }
                }
                else if (jobid == -1)
                {
                    TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .PickSessionIdFromPool: wait for the sessions ready for the session pool.");
                    sp.PoolChangeEvent.WaitOne();
                }
                else
                {
                    // jobid == 0
                    return 0;
                }
            }
        }

        /// <summary>
        /// Add a session to the session pool.
        /// </summary>
        ///<param name="serviceNameWithVersion">ServiceName_Version</param>
        /// <param name="durable">durable session or not</param>
        /// <param name="sessionId">session id</param>
        /// <param name="poolSize">the session pool size</param>
        protected override void AddSessionToPool(string serviceNameWithVersion, bool durable, int sessionId, int poolSize)
        {
            Dictionary<string, SessionPool> dictionary = durable ? this.durableSessionPool : this.nonDurableSessionPool;
            Debug.Assert(dictionary != null, "the session pool is null");

            lock (dictionary)
            {
                SessionPool sp;
                if (!dictionary.TryGetValue(serviceNameWithVersion, out sp))
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .AddSessionToPool: cannot find the session pool for service {0}", serviceNameWithVersion);
                }

                if (sessionId > 0 && sp.Length < poolSize)
                {
                    sp.SessionIds.Add(sessionId);
                }

                sp.Preparing--;
                sp.PoolChangeEvent.Set();

                TraceHelper.TraceEvent(
                    TraceEventType.Verbose,
                    "[SessionLauncher] .AddSessionToPool: Session Pool {0} statics Length/Size/Index/Preparing/Ids: {1}/{2}/{3}/{4}/{5}.",
                    serviceNameWithVersion,
                    sp.Length,
                    poolSize,
                    sp.Index,
                    sp.Preparing,
                    string.Join(",", sp.SessionIds));
            }
        }

        protected override async Task<SessionAllocateInfoContract> CreateAndSubmitSessionJob(
            SessionStartInfoContract startInfo,
            string endpointPrefix,
            bool durable,
            string callId,
            SecureString securePassword,
            ServiceRegistration registration,
            SessionAllocateInfoContract sessionAllocateInfo,
            string traceSwitchValue,
            string serviceName,
            BrokerConfigurations brokerConfigurations,
            string hostpath)
        {
            TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] .AllocateInternalAsync: callId={0}, endpointPrefix={1}, durable={2}.", callId, endpointPrefix, durable);

            if (!startInfo.UseAad && (string.IsNullOrEmpty(startInfo.Username) || securePassword == null))
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .AllocateInternalAsync: callId={0}, Username and password is necessary.", callId);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.AuthenticationFailure, SR.SessionLauncher_NeedUserNameAndPassword, null);
            }

            if (!IsEndpointPrefixSupported(endpointPrefix))
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .AllocateInternalAsync: callId={0}, enpoint prfix, {1}, is not support.", callId, endpointPrefix);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.InvalidArgument, SR.SessionLauncher_EndpointNotSupported, endpointPrefix);
            }

            ISchedulerJob schedulerJob = null;
            try
            {
                schedulerJob = this.scheduler.CreateJob();

                Debug.Assert(schedulerJob != null);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .AllocateInternalAsync: callId={0}, Create job failed, Exception:{1}", callId, e);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.CreateJobFailure, SR.SessionLauncher_FailToCreateJob, e.ToString());
            }

            try
            {
                JobHelper.MakeJobProperties(startInfo, schedulerJob, registration.Service.SoaDiagTraceLevel);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .AllocateInternalAsync: callId={0}, Make job properties failed, Exception:{1}", callId, e);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.CreateJobPropertiesFailure, SR.SessionLauncher_SchedulerException, e.ToString());
            }

            // Set JobRuntimeType for usage tracking
            schedulerJob.RuntimeType = JobRuntimeType.SOA;

            // Set the parent jobs if specified
            if (startInfo.ParentJobIds != null && startInfo.ParentJobIds.Count > 0)
            {
                schedulerJob.ParentJobIds = new IntCollection(startInfo.ParentJobIds);
            }

            sessionAllocateInfo.Id = 0;
            sessionAllocateInfo.BrokerLauncherEpr = null;
            List<NodeInfo> nodeInfos = new List<NodeInfo>();

            if (startInfo.UseInprocessBroker)
            {
                // Do not get broker eprs for inprocess broker session
                sessionAllocateInfo.BrokerLauncherEpr = new string[0];

                // Bug 11378: Set JobOwner's user name into job's environment
                schedulerJob.SetEnvironmentVariable(Constant.JobOwnerNameEnvVar, OperationContext.Current.ServiceSecurityContext.WindowsIdentity.Name);
            }

            // if this is a diagnostic case which specific the broker node
            else if (!string.IsNullOrEmpty(startInfo.DiagnosticBrokerNode))
            {
                // sessionAllocateInfo.BrokerLauncherEpr = new string[] { startInfo.IsAadOrLocalUser ? BrokerNodesManager.GenerateBrokerLauncherInternalEpr(endpointPrefix, startInfo.DiagnosticBrokerNode) : BrokerNodesManager.GenerateBrokerLauncherEpr(endpointPrefix, startInfo.DiagnosticBrokerNode, startInfo.TransportScheme) };
                sessionAllocateInfo.BrokerLauncherEpr = new[] { GenerateBrokerLauncherAddress(endpointPrefix, startInfo) };

                // TODO: RICHCI: GetSDDI cannot be called here. Need to look up SDDI from brokermanager
                nodeInfos.Add(BrokerNodesManager.GenerateNodeInfo(startInfo.DiagnosticBrokerNode));
            }
            else if (SoaHelper.IsOnAzure())
            {
                sessionAllocateInfo.BrokerLauncherEpr = this.brokerNodesManager.GetAvailableAzureBrokerEPRs(out nodeInfos);
                if (sessionAllocateInfo.BrokerLauncherEpr.Length <= 0)
                {
                    TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] .AllocateInternalAsync: callId={0}, no available Azure broker nodes", callId);
                    return sessionAllocateInfo;
                }
            }
            else if (!this.runningLocal)
            {
                bool enableFQDN = false;

                string enableFqdnStr = JobHelper.GetEnvironmentVariable(this.scheduler, Constant.EnableFqdnEnv);

                if (!string.IsNullOrEmpty(enableFqdnStr))
                {
                    if (bool.TryParse(enableFqdnStr, out enableFQDN))
                    {
                        TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .AllocateInternalAsync: The enableFQDN setting in cluster env var is {0}", enableFQDN);
                    }
                    else
                    {
                        TraceHelper.TraceEvent(
                            TraceEventType.Error,
                            "[SessionLauncher] .AllocateInternalAsync: The enableFQDN setting \"{0}\" in cluster env var is not a valid bool value.",
                            enableFqdnStr);
                    }
                }

                // get available broker eprs
                // GetAvailableBrokerEPRs is thread safe.
                sessionAllocateInfo.BrokerLauncherEpr = this.brokerNodesManager.GetAvailableBrokerEPRs(
                    durable,
                    endpointPrefix,
                    enableFQDN,
                    startInfo.ChannelType,
                    startInfo.TransportScheme,
                    out nodeInfos);

                if (sessionAllocateInfo.BrokerLauncherEpr.Length <= 0)
                {
                    TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] .AllocateInternalAsync: callId={0}, no available broker nodes", callId);
                    return sessionAllocateInfo;
                }
            }
            else
            {
                sessionAllocateInfo.BrokerLauncherEpr = new[] { "net.pipe://localhost/BrokerLauncher" };
            }

            // make job envs
            foreach (NameValueConfigurationElement entry in registration.Service.EnvironmentVariables)
            {
                schedulerJob.SetEnvironmentVariable(entry.Name, entry.Value);
            }

            // pass service serviceInitializationTimeout as job environment variables
            schedulerJob.SetEnvironmentVariable(Constant.ServiceInitializationTimeoutEnvVar, registration.Service.ServiceInitializationTimeout.ToString());

            if (startInfo.ServiceHostIdleTimeout == null)
            {
                schedulerJob.SetEnvironmentVariable(Constant.ServiceHostIdleTimeoutEnvVar, registration.Service.ServiceHostIdleTimeout.ToString());
            }
            else
            {
                schedulerJob.SetEnvironmentVariable(Constant.ServiceHostIdleTimeoutEnvVar, startInfo.ServiceHostIdleTimeout.ToString());
            }

            if (startInfo.ServiceHangTimeout == null)
            {
                schedulerJob.SetEnvironmentVariable(Constant.ServiceHangTimeoutEnvVar, registration.Service.ServiceHangTimeout.ToString());
            }
            else
            {
                schedulerJob.SetEnvironmentVariable(Constant.ServiceHangTimeoutEnvVar, startInfo.ServiceHangTimeout.ToString());
            }

            // pass MessageLevelPreemption switcher as job environment variables
            schedulerJob.SetEnvironmentVariable(Constant.EnableMessageLevelPreemptionEnvVar, registration.Service.EnableMessageLevelPreemption.ToString());

            // pass trace switcher to svchost
            if (!string.IsNullOrEmpty(traceSwitchValue))
            {
                schedulerJob.SetEnvironmentVariable(Constant.TraceSwitchValue, traceSwitchValue);
            }

            // pass taskcancelgraceperiod as environment variable to svchosts
            string taskCancelGracePeriod = JobHelper.GetClusterParameterValue(this.scheduler, Constant.TaskCancelGracePeriodClusParam, Constant.DefaultCancelTaskGracePeriod.ToString());
            schedulerJob.SetEnvironmentVariable(Constant.CancelTaskGracePeriodEnvVar, taskCancelGracePeriod);

            // pass service config file name to services
            schedulerJob.SetEnvironmentVariable(Constant.ServiceConfigFileNameEnvVar, serviceName);

            // pass maxMessageSize to service hosts
            int maxMessageSize = startInfo.MaxMessageSize.HasValue ? startInfo.MaxMessageSize.Value : registration.Service.MaxMessageSize;
            schedulerJob.SetEnvironmentVariable(Constant.ServiceConfigMaxMessageEnvVar, maxMessageSize.ToString());

            // pass service operation timeout to service hosts
            int? serviceOperationTimeout = null;
            if (startInfo.ServiceOperationTimeout.HasValue)
            {
                serviceOperationTimeout = startInfo.ServiceOperationTimeout;
            }
            else if (brokerConfigurations != null && brokerConfigurations.LoadBalancing != null)
            {
                serviceOperationTimeout = brokerConfigurations.LoadBalancing.ServiceOperationTimeout;
            }

            if (serviceOperationTimeout.HasValue)
            {
                schedulerJob.SetEnvironmentVariable(Constant.ServiceConfigServiceOperatonTimeoutEnvVar, serviceOperationTimeout.Value.ToString());
            }

            if (startInfo.Environments != null)
            {
                foreach (KeyValuePair<string, string> entry in startInfo.Environments)
                {
                    schedulerJob.SetEnvironmentVariable(entry.Key, entry.Value);
                }
            }

            // Pass DataServerInfo to service host via job environment variables
            DataServerInfo dsInfo = null;

            if (SessionLauncherSettings.Default.EnableDataService)
            {
                dsInfo = this.dataService.GetDataServerInfo();
            }

            if (dsInfo != null)
            {
                schedulerJob.SetEnvironmentVariable(Constant.SoaDataServerInfoEnvVar, dsInfo.AddressInfo);
            }

            // Each SOA job is assigned a GUID "secret", which is used
            // to identify soa job owner. When a job running in Azure 
            // tries to access common data, it sends this "secret" together
            // with a data request to data service.  Data service trusts
            // the data request only if the job id and job "secret" 
            // match. 
            schedulerJob.SetEnvironmentVariable(Constant.JobSecretEnvVar, Guid.NewGuid().ToString());

            // Set CCP_SERVICE_SESSIONPOOL env var of the job
            if (startInfo.UseSessionPool)
            {
                schedulerJob.SetEnvironmentVariable(Constant.ServiceUseSessionPoolEnvVar, bool.TrueString);
            }

            // Add the broker nodes available to the session as env variables so that services only
            // process requests from BNs
            List<string> nodeSSDLList = new List<string>(nodeInfos.Count);
            foreach (NodeInfo info in nodeInfos)
            {
                nodeSSDLList.Add(info.SSDL);
            }

            SessionBrokerNodes.SetSessionBrokerNodes(schedulerJob, nodeSSDLList);

            // Get the how many tasks to add
            // If there is user specified max units, then use it, otherwise a default 16
            int numTasks = (startInfo.MaxUnits != null && startInfo.MaxUnits.Value > 0) ? startInfo.MaxUnits.Value : DefaultInitTaskNumber;

            try
            {
                // Limit the task number within the cluster size
                int[] totalCountNumbers = new int[3];

                try
                {
                    ISchedulerCounters counters = this.scheduler.GetCounters();
                    totalCountNumbers[0] = counters.TotalCores;
                    totalCountNumbers[1] = counters.TotalSockets;
                    totalCountNumbers[2] = counters.TotalNodes;
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(
                        TraceEventType.Error,
                        "[SessionLauncher] .AllocateInternalAsync: callId={0}, Failed to get the property of TotalCoreCount, TotalSocketCount and TotalNodeCount, Exception:{1}",
                        callId,
                        e);

                    ThrowHelper.ThrowSessionFault(SOAFaultCode.GetClusterPropertyFailure, SR.SessionLauncher_FailToGetClusterProperty, e.ToString());
                }

                int maxSize = totalCountNumbers[0];
                if (startInfo.ResourceUnitType != null)
                {
                    switch ((JobUnitType)startInfo.ResourceUnitType)
                    {
                        case JobUnitType.Socket:
                            maxSize = totalCountNumbers[1];
                            break;

                        case JobUnitType.Node:
                            maxSize = totalCountNumbers[2];
                            break;
                    }
                }

                TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] .AllocateInternalAsync: callId={0}, numTasks={1}, maxSize={2}", callId, numTasks, maxSize);

                if (numTasks > maxSize)
                {
                    numTasks = maxSize;
                }

                try
                {
                    ISchedulerTask schedulerTask = schedulerJob.CreateTask();

                    // Add service tasks
                    schedulerTask.MinimumNumberOfCores = schedulerTask.MaximumNumberOfCores =
                                                             schedulerTask.MinimumNumberOfSockets =
                                                                 schedulerTask.MaximumNumberOfSockets = schedulerTask.MinimumNumberOfNodes = schedulerTask.MaximumNumberOfNodes = 1;

                    // Use service task to submit initial tasks
                    schedulerTask.Type = TaskType.Service;

                    schedulerTask.CommandLine = hostpath;

                    string stderrPath = registration.Service.StdError;
                    if (!string.IsNullOrEmpty(stderrPath))
                    {
                        schedulerTask.StdErrFilePath = stderrPath;
                    }

                    // Bug 8153: Use FailJobOnFailureCount task property to monitor for runaway service tasks (i.e. service tasks with a bad command line) instead of monitoring
                    // failed task instances from HpcSession which is too slow
                    schedulerTask.FailJobOnFailure = true;
                    schedulerTask.FailJobOnFailureCount = MaxFailedTask;

                    schedulerJob.SetEnvironmentVariable(BrokerSettingsConstants.Secure, startInfo.Secure.ToString());
                    schedulerJob.SetEnvironmentVariable(BrokerSettingsConstants.TransportScheme, startInfo.TransportScheme.ToString());

                    TraceHelper.TraceEvent(
                        TraceEventType.Information,
                        "[SessionLauncher] .AllocateInternalAsync: callId={0}, set job environment: {1}={2}, {3}={4}.",
                        callId,
                        BrokerSettingsConstants.Secure,
                        startInfo.Secure,
                        BrokerSettingsConstants.TransportScheme,
                        startInfo.TransportScheme);

                    schedulerJob.AddTask(schedulerTask);
                }
                catch (SchedulerException e)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .AllocateInternalAsync: callId={0}, Create task failed, Exception:{1}", callId, e);

                    ThrowHelper.ThrowSessionFault(SOAFaultCode.CreateJobTasksFailure, SR.SessionLauncher_CreateJobTasksFailure, e.ToString());
                }

                // Add job first then we can get a job id.
                using (new SessionIdentityImpersonation(startInfo.UseAad))
                {
                    this.scheduler.AddJob(schedulerJob);
                }

                if (!string.IsNullOrEmpty(startInfo.DependFiles))
                {
                    TraceHelper.TraceEvent(
                        TraceEventType.Information,
                        "[SessionLauncher] .AllocateInternalAsync: callId={0}, JobId={1}, DependFiles:{2}",
                        callId,
                        schedulerJob.Id,
                        startInfo.DependFiles);
                    string userRoot = Path.Combine(this.dataService.GetUserJobDataRoot(), startInfo.Username.Replace('\\', '.'));
                    string jobSharePath = Path.Combine(userRoot, schedulerJob.Id.ToString());
                    Directory.CreateDirectory(jobSharePath);
                    schedulerJob.SetEnvironmentVariable(Constant.SoaDataJobDirEnvVar, jobSharePath);
                    string[] filePairs = startInfo.DependFiles.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var filepair in filePairs)
                    {
                        int eq_index = filepair.IndexOf('=');
                        string clientId = filepair.Substring(0, eq_index);
                        string targetPath = filepair.Substring(eq_index + 1);
                        string localCachePath = Path.Combine(userRoot, clientId);
                        if (!File.Exists(localCachePath))
                        {
                            DataClient client = DataClient.Open("localhost", clientId, DataLocation.AzureBlob);
                            File.WriteAllBytes(localCachePath, client.ReadRawBytesAll());
                        }

                        targetPath = Path.Combine(jobSharePath, targetPath);
                        string targetDir = Path.GetDirectoryName(targetPath);
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }

                        File.Copy(localCachePath, targetPath);
                    }
                }

                // Add prepare and release tasks to server job if specified by the user
                AddPrepReleaseTasks(
                    schedulerJob,
                    GetServiceAssemblyDirectory(registration.Service.AssemblyPath),
                    registration.Service.PrepareNodeCommandLine,
                    registration.Service.ReleaseNodeCommandLine);

                SimulateFailure.FailOperation(1);

                try
                {
                    using (new SessionIdentityImpersonation(startInfo.UseAad))
                    {
                        if (startInfo.SavePassword.HasValue && startInfo.SavePassword.Value)
                        {
                            this.scheduler.SetCachedCredentials(startInfo.Username, UnsecureString(securePassword));
                        }

                        // upload the certificate to the scheduler if it exists in the startInfo
                        if (!string.IsNullOrEmpty(startInfo.PfxPassword) && startInfo.Certificate != null)
                        {
                            this.scheduler.SetCertificateCredentialsPfx(startInfo.Username, startInfo.PfxPassword, startInfo.Certificate);
                        }

                        TraceHelper.TraceEvent(
                            TraceEventType.Information,
                            "[SessionLauncher].AllocateInternalAsync: UseAad={0}, UserName={1}, CurrentPrincipal={2}",
                            startInfo.UseAad,
                            startInfo.Username,
                            Thread.CurrentPrincipal.Identity.Name);

                        if (startInfo.UseAad)
                        {
                            this.scheduler.SubmitJob(schedulerJob, null, null);
                        }
                        else if (startInfo.LocalUser.GetValueOrDefault())
                        {
                            this.scheduler.SubmitJob(schedulerJob, startInfo.Username.Split('\\').Last(), UnsecureString(securePassword));
                        }
                        else
                        {
                            this.scheduler.SubmitJob(schedulerJob, startInfo.Username, UnsecureString(securePassword));
                        }
                    }

                    sessionAllocateInfo.Id = schedulerJob.Id;
                }
                catch (InvalidCredentialException e)
                {
                    TraceHelper.TraceEvent(
                        sessionAllocateInfo.Id,
                        TraceEventType.Error,
                        "[SessionLauncher] .AllocateInternalAsync: callId={0}, Invalide credential to submit the job, Exception:{1}",
                        callId,
                        e);

                    try
                    {
                        this.scheduler.DeleteJob(schedulerJob.Id);
                    }
                    catch
                    {
                        TraceHelper.TraceEvent(
                            sessionAllocateInfo.Id,
                            TraceEventType.Warning,
                            $"[SessionLauncher] .AllocateInternalAsync: callId={callId}, Failed to delete the job {schedulerJob.Id}, Exception:{e}");
                    }

                    SchedulerException schedulerExcep = e.InnerException as SchedulerException;
                    if (schedulerExcep == null)
                    {
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.AuthenticationFailure, SR.SessionLauncher_CannotLogon, null);
                    }
                    else
                    {
                        int faultCode = SOAFaultCode.UnknownError;
                        if (schedulerExcep.Code == ErrorCode.Operation_AuthenticationFailure)
                        {
                            faultCode = ConvertToFaultCode(schedulerExcep.Params);
                        }

                        ThrowHelper.ThrowSessionFault(faultCode, schedulerExcep.Message, null);
                    }
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(sessionAllocateInfo.Id, TraceEventType.Error, "[SessionLauncher] .AllocateInternalAsync: callId={0}, Failed to submit job, Exception:{1}", callId, e);

                    ThrowHelper.ThrowSessionFault(SOAFaultCode.SubmitJobFailure, SR.SessionLauncher_FailToSubmitJob, e.Message);
                }

                StringBuilder brokerEprsString = new StringBuilder();
                foreach (string epr in sessionAllocateInfo.BrokerLauncherEpr)
                {
                    brokerEprsString.AppendLine(epr);
                }

                TraceHelper.TraceEvent(
                    sessionAllocateInfo.Id,
                    TraceEventType.Information,
                    "[SessionLauncher] .AllocateInternalAsync: callId={0}, Alloc returned Broker eprs:{1}",
                    callId,
                    brokerEprsString.ToString());

                return await Task.FromResult(sessionAllocateInfo);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .AllocateInternalAsync: callId={0}, Exception raised, Exception:{1}", callId, e);

                if (e is FaultException<SessionFault>)
                {
                    throw;
                }

                ThrowHelper.ThrowSessionFault(SOAFaultCode.UnknownError, SR.UnknownError, e.ToString());
            }

            return null;
        }


        /// <summary>
        /// Ensure the caller is a valid cluster user
        /// </summary>
        protected override void CheckAccess()
        {
            if (Thread.CurrentPrincipal.IsHpcAadPrincipal())
            {
                return;
            }

            if (SoaHelper.CheckX509Identity(OperationContext.Current))
            {
                return;
            }

            if (!SoaHelper.CheckWindowsIdentity(OperationContext.Current))
            {
                throw new SecurityException();
            }
        }

        protected override Task<SessionAllocateInfoContract> AllocateInternalAsync(SessionStartInfoContract startInfo, string endpointPrefix, bool durable)
        {
            if (this.schedulerConnectState != SchedulerConnectState.ConnectionComplete)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Information,
                    "[SessionLauncher] .AllocateInternalAsync: session launcher is not conected to the scheduler, schedulerConnectState={0}",
                    this.schedulerConnectState);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.ConnectToSchedulerFailure, SR.SessionLauncher_NoConnectionToScheduler, null);
            }

            return base.AllocateInternalAsync(startInfo, endpointPrefix, durable);
        }

        /// <summary>
        /// Returns the service assembly's directory
        /// </summary>
        /// <param name="serviceAssemblyPath">Path to service assembly</param>
        /// <returns></returns>
        private static string GetServiceAssemblyDirectory(string serviceAssemblyPath)
        {
            if (string.IsNullOrEmpty(serviceAssemblyPath))
            {
                return string.Empty;
            }

            return Path.GetDirectoryName(serviceAssemblyPath);
        }

        /// <summary>
        /// Convert a SecureString password back to a plain string
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static string UnsecureString(SecureString secureString)
        {
            if (secureString == null)
            {
                return null;
            }

            string unsecureString;
            IntPtr ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);

            try
            {
                unsecureString = Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }

            return unsecureString;
        }

        private static string GenerateBrokerLauncherAddress(string endpointPrefix, SessionStartInfoContract startInfo)
        {
            if (startInfo.UseAad)
            {
                return BrokerNodesManager.GenerateBrokerLauncherAadEpr(endpointPrefix, startInfo.DiagnosticBrokerNode);
            }

            if (startInfo.IsAadOrLocalUser)
            {
                // Local user goes here
                return BrokerNodesManager.GenerateBrokerLauncherInternalEpr(endpointPrefix, startInfo.DiagnosticBrokerNode);
            }

            return BrokerNodesManager.GenerateBrokerLauncherEpr(endpointPrefix, startInfo.DiagnosticBrokerNode, startInfo.TransportScheme);
        }

        /// <summary>
        /// Adds pre/post tasks to the service job (if configured)
        /// </summary>
        /// <param name="schedulerJob">Service job</param>
        /// <param name="workingDir">Working dir prep/release task working dir</param>
        /// <param name="prepareNodeCommandLine">Prepare node task command line</param>
        /// <param name="releaseNodeCommandLine">Release node task command line</param>
        /// <remarks>
        ///     Always use 32 bit HpcServiceHost since the pre/post command line is always a separate process. 32 bit version runs on 32 and 64 bit OSes.
        ///     Set working dir to service assembly's directory if it has one. 
        ///         On Azure the pre\post task is relative to the service assembly's local dir on the Azure node. Pre/post tasks can be absolute on azure.
        ///         On premise the pre\post task is relative to service assembly path's dir if any. If no assembly path dir its relative to user's profile dir. It can be absolute on premise.
        /// </remarks>
        private static void AddPrepReleaseTasks(ISchedulerJob schedulerJob, string workingDir, string prepareNodeCommandLine, string releaseNodeCommandLine)
        {
            if (!string.IsNullOrEmpty(prepareNodeCommandLine))
            {
                ISchedulerTask prepTask = schedulerJob.CreateTask();
                prepTask.Type = TaskType.NodePrep;
                prepTask.CommandLine = TaskCommandLine32;
                prepTask.SetEnvironmentVariable(Constant.PrePostTaskCommandLineEnvVar, prepareNodeCommandLine);
                prepTask.SetEnvironmentVariable(Constant.PrePostTaskOnPremiseWorkingDirEnvVar, workingDir);

                schedulerJob.AddTask(prepTask);
            }

            if (!string.IsNullOrEmpty(releaseNodeCommandLine))
            {
                ISchedulerTask releaseTask = schedulerJob.CreateTask();
                releaseTask.Type = TaskType.NodeRelease;
                releaseTask.CommandLine = TaskCommandLine32;
                releaseTask.SetEnvironmentVariable(Constant.PrePostTaskCommandLineEnvVar, releaseNodeCommandLine);
                releaseTask.SetEnvironmentVariable(Constant.PrePostTaskOnPremiseWorkingDirEnvVar, workingDir);

                schedulerJob.AddTask(releaseTask);
            }
        }

        /// <summary>
        /// Convert SchedulerException's Params property to SOA fault code.
        /// </summary>
        /// <param name="schedulerParams">
        /// When the SchedulerException is an inner exception of InvalidAuthernticationException,
        /// it's Params is expected to be null or empty or int.
        /// </param>
        /// <returns>SOA fault code</returns>
        private static int ConvertToFaultCode(string schedulerParams)
        {
            if (string.IsNullOrEmpty(schedulerParams))
            {
                // need password, keep back-compact with previous version client
                return SOAFaultCode.AuthenticationFailure;
            }

            int error;
            if (!int.TryParse(schedulerParams, out error))
            {
                return SOAFaultCode.UnknownError;
            }

            switch (error)
            {
                // need either password or cert when cluster param HpcSoftCard is Allowed
                case ErrorCode.AuthFailureAllowSoftCardDisableCredentialReuse:
                    return SOAFaultCode.AuthenticationFailure_NeedEitherTypeCred_UnReusable;

                // need either password or cert when cluster param HpcSoftCard is Allowed
                case ErrorCode.AuthFailureAllowSoftCardAboutToExpireSaved:
                case ErrorCode.AuthFailureAllowSoftCardNoValidSaved:
                    return SOAFaultCode.AuthenticationFailure_NeedEitherTypeCred;

                // need cert when cluster param HpcSoftCard is Required
                case ErrorCode.AuthFailureRequireSoftCardDisableCredentialReuse:
                case ErrorCode.AuthFailureRequireSoftCardAboutToExpireSaved:
                case ErrorCode.AuthFailureRequireSoftCardNoValidSaved:
                    return SOAFaultCode.AuthenticationFailure_NeedCertOnly;

                // need password when cluster param HpcSoftCard is Disabled
                case ErrorCode.AuthFailureDisableCredentialReuse:
                    return SOAFaultCode.AuthenticationFailure_NeedPasswordOnly_UnReusable;

                default:
                    return SOAFaultCode.UnknownError;
            }
        }

        /// <summary>
        /// the helper function to check whether the endpoint prefix is supported.
        /// </summary>
        /// <param name="endpointPrefix"></param>
        /// <returns>a value indicating if the endpoint is supported.</returns>
        private static bool IsEndpointPrefixSupported(string endpointPrefix)
        {
            if (string.IsNullOrEmpty(endpointPrefix))
            {
                return false;
            }

            switch (endpointPrefix.ToLowerInvariant().Trim())
            {
                case BrokerNodesManager.NettcpPrefix:
                case BrokerNodesManager.HttpsPrefix:
                    return true;
            }

            return false;
        }

        private static WindowsIdentity GetCallerWindowsIdentity()
        {
            WindowsIdentity callerWindowsIdentity = ServiceSecurityContext.Current.WindowsIdentity;
            if (callerWindowsIdentity == null)
            {
                throw new InvalidOperationException("The caller cannot be mapped to a WindowsIdentity");
            }

            return callerWindowsIdentity;
        }

        /// <summary>
        /// Gets data server information
        /// </summary>
        /// <returns>returns data server information</returns>
        internal DataService GetDataService()
        {
            return this.dataService;
        }
    }
}
#endif