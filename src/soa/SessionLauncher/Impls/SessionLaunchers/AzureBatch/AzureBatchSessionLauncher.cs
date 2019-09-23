// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.AzureBatch
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security;
    using System.Threading.Tasks;

    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Common;
    using Microsoft.Telepathy.Common;
    using Microsoft.Telepathy.Common.ServiceRegistrationStore;
    using Microsoft.Telepathy.Internal.SessionLauncher.Utils;
    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Configuration;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    using JobState = Microsoft.Telepathy.Session.Data.JobState;

    internal class AzureBatchSessionLauncher : SessionLauncher
    {
        private CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(SessionLauncherRuntimeConfiguration.SessionLauncherStorageConnectionString);

        private const string RuntimeContainer = "runtime";

        private const string CcpServiceHostFolder = "ccpservicehost";

        private const string BrokerFolder = "broker";

        private const string ServiceAssemblyContainer = "service-assembly";

        private const string ServiceRegistrationContainer = "service-registration";

        private const string AzureBatchTaskWorkingDirEnvVar = "%AZ_BATCH_TASK_WORKING_DIR%";

        private const string AzureBatchJobPrepTaskWorkingDirEnvVar = "AZ_BATCH_JOB_PREP_WORKING_DIR";

        private const string OpenNetTcpPortSharingAndDisableStrongNameValidationCmdLine =
            @"cmd /c ""sc.exe config NetTcpPortSharing start= demand & reg ADD ^""HKLM\Software\Microsoft\StrongName\Verification\*,*^"" /f & reg ADD ^""HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\*,*^"" /f""";

        private static TimeSpan SchedulingTimeout = TimeSpan.FromMinutes(5);

        private Process brokerLauncherProcess; // TODO: Design - change it to a process has same level with session launcher

        private readonly Dictionary<string, string> defaultSoaConfigurations = new Dictionary<string, string>()
                                                                                   {
                                                                                       { Constant.RegistryPathEnv, $"%{AzureBatchJobPrepTaskWorkingDirEnvVar}%" },
                                                                                       { Constant.AutomaticShrinkEnabled, "False" },
                                                                                       { Constant.NettcpOver443, "True" },
                                                                                       { Constant.NetworkPrefixEnv, string.Empty },
                                                                                       { Constant.EnableFqdnEnv, string.Empty }
                                                                                   };

        // TODO: remove parameter less ctor and add specific parameters for the sake of test-ablity
        public AzureBatchSessionLauncher()
        {
            this.clusterInfo = new ClusterInfo();
            this.clusterInfo.Contract.ClusterName = AzureBatchConfiguration.BatchPoolName;
            this.clusterInfo.Contract.NetworkTopology = "Public";
            this.clusterInfo.Contract.AzureStorageConnectionString =
                AzureBatchConfiguration.SoaBrokerStorageConnectionString;
        }

        public override async Task<SessionInfoContract> GetInfoAsync(string endpointPrefix, string sessionId)
        {
            SessionInfoContract sessionInfo = null;
            this.CheckAccess();

            ParamCheckUtility.ThrowIfNullOrEmpty(endpointPrefix, "endpointPrefix");

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

            try
            {
                using (var batchClient = AzureBatchConfiguration.GetBatchClient())
                {
                    var jobId = AzureBatchSessionJobIdConverter.ConvertToAzureBatchJobId(sessionId);

                    var sessionJob = await batchClient.JobOperations.GetJobAsync(jobId).ConfigureAwait(false);
                    if (sessionJob == null)
                    {
                        throw new InvalidOperationException($"[{nameof(AzureBatchSessionLauncher)}] .{nameof(this.GetInfoAsync)} Failed to get batch job for session {sessionId}");
                    }

                    TraceHelper.TraceEvent(
                        sessionId,
                        TraceEventType.Information,
                        $"[{nameof(AzureBatchSessionLauncher)}] .{nameof(this.GetInfoAsync)}: try to get the job properties(Secure, TransportScheme, BrokerEpr, BrokerNode, ControllerEpr, ResponseEpr) for the job, jobid={sessionId}.");

                    sessionInfo = new SessionInfoContract();
                    sessionInfo.Id = AzureBatchSessionJobIdConverter.ConvertToSessionId(sessionJob.Id);

                    // TODO: sessionInfo.JobState
                    var metadata = sessionJob.Metadata;
                    if (metadata != null)
                    {
                        string brokerNodeString = null;

                        foreach (var pair in metadata)
                        {
                            if (pair.Name.Equals(BrokerSettingsConstants.Secure, StringComparison.OrdinalIgnoreCase))
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
                                sessionInfo.BrokerLauncherEpr = BrokerNodesManager.GenerateBrokerLauncherEpr(endpointPrefix, brokerNodeString, sessionInfo.TransportScheme);
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
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "[SessionLauncher] .GetInfo: Failed to get all properties from job[{0}], Exception:{1}", sessionId, e);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.GetJobPropertyFailure, SR.SessionLauncher_FailToGetJobProperty, e.ToString());
            }

            if (sessionInfo.SessionOwner == null)
            {
                sessionInfo.SessionOwner = "Everyone";
            }

            if (sessionInfo.SessionACL == null)
            {
                sessionInfo.SessionACL = new string[0];
            }

            if (sessionInfo.JobState == 0)
            {
                // TODO: apply job state converting
                sessionInfo.JobState = JobState.Running;
            }

            TraceHelper.TraceEvent(
                sessionId,
                TraceEventType.Information,
                "[SessionLauncher] .GetInfo: return the sessionInfo, BrokerEpr={0}, BrokerLauncherEpr={1}, ControllerEpr={2}, Id={3}, JobState={4}, ResponseEpr={5}, Secure={6}, TransportScheme={7}, sessionOwner={8}, sessionACL={9}",
                sessionInfo.BrokerEpr,
                sessionInfo.BrokerLauncherEpr,
                sessionInfo.ControllerEpr,
                sessionInfo.Id,
                sessionInfo.JobState,
                sessionInfo.ResponseEpr,
                sessionInfo.Secure,
                sessionInfo.TransportScheme,
                sessionInfo.SessionOwner ?? "null",
                sessionInfo.SessionACL?.Length.ToString() ?? "null");
            return sessionInfo;
        }


        public override async Task TerminateAsync(string sessionId)
        {
            using (var batchClient = AzureBatchConfiguration.GetBatchClient())
            {
                var batchJob = await batchClient.JobOperations.GetJobAsync(AzureBatchSessionJobIdConverter.ConvertToAzureBatchJobId(sessionId));
                await batchJob.TerminateAsync();
            }
        }

        public override async Task<Version[]> GetServiceVersionsAsync(string serviceName)
        {
            throw new NotImplementedException();
        }

        public override async Task<string> GetSOAConfigurationAsync(string key)
        {
            // TODO: Retrieve configuration from env vars
            if (this.defaultSoaConfigurations.TryGetValue(key, out var res))
            {
                return res;
            }
            else
            {
                return string.Empty;
            }
        }

        public override async Task<Dictionary<string, string>> GetSOAConfigurationsAsync(List<string> keys)
        {
            // TODO: Retrieve configuration from env vars
            var res = new Dictionary<string, string>();
            foreach (var key in keys)
            {
                if (this.defaultSoaConfigurations.ContainsKey(key))
                {
                    res[key] = this.defaultSoaConfigurations[key];
                }
            }

            return res;
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
            try
            {
                bool brokerPerfMode = true; // TODO: implement separated broker mode
                if (brokerPerfMode)
                {
                    TraceHelper.TraceEvent(TraceEventType.Information, "[AzureBatchSessionLauncher] .CreateAndSubmitSessionJob: broker perf mode");
                }

                TraceHelper.TraceEvent(
                    TraceEventType.Information,
                    "[AzureBatchSessionLauncher] .CreateAndSubmitSessionJob: callId={0}, endpointPrefix={1}, durable={2}.",
                    callId,
                    endpointPrefix,
                    durable);
                using (var batchClient = AzureBatchConfiguration.GetBatchClient())
                {
                    var pool = await batchClient.PoolOperations.GetPoolAsync(AzureBatchConfiguration.BatchPoolName);
                    ODATADetailLevel detailLevel = new ODATADetailLevel();
                    detailLevel.SelectClause = "affinityId, ipAddress";
                    //detailLevel.FilterClause = @"state eq 'idle'";
                    var nodes = await pool.ListComputeNodes(detailLevel).ToListAsync();
                    if (nodes.Count < 1)
                    {
                        throw new InvalidOperationException("Compute node count in selected pool is less then 1.");
                    }

                    sessionAllocateInfo.Id = string.Empty;

                    // sessionAllocateInfo.BrokerLauncherEpr = new[] { SessionInternalConstants.BrokerConnectionStringToken };
                    IList<EnvironmentSetting> ConstructEnvironmentVariable()
                    {
                        List<EnvironmentSetting> env = new List<EnvironmentSetting>(); // Can change to set to ensure no unintended overwrite
                        foreach (NameValueConfigurationElement entry in registration.Service.EnvironmentVariables)
                        {
                            env.Add(new EnvironmentSetting(entry.Name, entry.Value));
                        }

                        // pass service serviceInitializationTimeout as job environment variables
                        env.Add(new EnvironmentSetting(Constant.ServiceInitializationTimeoutEnvVar, registration.Service.ServiceInitializationTimeout.ToString()));

                        if (startInfo.ServiceHostIdleTimeout == null)
                        {
                            env.Add(new EnvironmentSetting(Constant.ServiceHostIdleTimeoutEnvVar, registration.Service.ServiceHostIdleTimeout.ToString()));
                        }
                        else
                        {
                            env.Add(new EnvironmentSetting(Constant.ServiceHostIdleTimeoutEnvVar, startInfo.ServiceHostIdleTimeout.ToString()));
                        }

                        if (startInfo.ServiceHangTimeout == null)
                        {
                            env.Add(new EnvironmentSetting(Constant.ServiceHangTimeoutEnvVar, registration.Service.ServiceHangTimeout.ToString()));
                        }
                        else
                        {
                            env.Add(new EnvironmentSetting(Constant.ServiceHangTimeoutEnvVar, startInfo.ServiceHangTimeout.ToString()));
                        }

                        // pass MessageLevelPreemption switcher as job environment variables
                        env.Add(new EnvironmentSetting(Constant.EnableMessageLevelPreemptionEnvVar, registration.Service.EnableMessageLevelPreemption.ToString()));

                        // pass trace switcher to svchost
                        if (!string.IsNullOrEmpty(traceSwitchValue))
                        {
                            env.Add(new EnvironmentSetting(Constant.TraceSwitchValue, traceSwitchValue));
                        }

                        // pass taskcancelgraceperiod as environment variable to svchosts
                        env.Add(new EnvironmentSetting(Constant.CancelTaskGracePeriodEnvVar, Constant.DefaultCancelTaskGracePeriod.ToString()));

                        // pass service config file name to services
                        env.Add(new EnvironmentSetting(Constant.ServiceConfigFileNameEnvVar, serviceName));

                        // pass maxMessageSize to service hosts
                        int maxMessageSize = startInfo.MaxMessageSize.HasValue ? startInfo.MaxMessageSize.Value : registration.Service.MaxMessageSize;
                        env.Add(new EnvironmentSetting(Constant.ServiceConfigMaxMessageEnvVar, maxMessageSize.ToString()));

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
                            env.Add(new EnvironmentSetting(Constant.ServiceConfigServiceOperatonTimeoutEnvVar, serviceOperationTimeout.Value.ToString()));
                        }

                        if (startInfo.Environments != null)
                        {
                            foreach (KeyValuePair<string, string> entry in startInfo.Environments)
                            {
                                env.Add(new EnvironmentSetting(entry.Key, entry.Value));
                            }
                        }

                        // Each SOA job is assigned a GUID "secret", which is used
                        // to identify soa job owner. When a job running in Azure 
                        // tries to access common data, it sends this "secret" together
                        // with a data request to data service.  Data service trusts
                        // the data request only if the job id and job "secret" 
                        // match. 
                        env.Add(new EnvironmentSetting(Constant.JobSecretEnvVar, Guid.NewGuid().ToString()));

                        // Set CCP_SERVICE_SESSIONPOOL env var of the job
                        if (startInfo.UseSessionPool)
                        {
                            env.Add(new EnvironmentSetting(Constant.ServiceUseSessionPoolEnvVar, bool.TrueString));
                        }

                        void SetBrokerNodeAuthenticationInfo()
                        {
                            // TODO: set the information needed by compute node to authenticate broker node
                            return;
                        }

                        SetBrokerNodeAuthenticationInfo();

                        env.Add(new EnvironmentSetting(BrokerSettingsConstants.Secure, startInfo.Secure.ToString()));
                        env.Add(new EnvironmentSetting(BrokerSettingsConstants.TransportScheme, startInfo.TransportScheme.ToString()));

                        TraceHelper.TraceEvent(
                            TraceEventType.Information,
                            "[AzureBatchSessionLauncher] .CreateAndSubmitSessionJob: callId={0}, set job environment: {1}={2}, {3}={4}.",
                            callId,
                            BrokerSettingsConstants.Secure,
                            startInfo.Secure,
                            BrokerSettingsConstants.TransportScheme,
                            startInfo.TransportScheme);

                        env.Add(new EnvironmentSetting(TelepathyConstants.SchedulerEnvironmentVariableName, Dns.GetHostName()));
                        env.Add(new EnvironmentSetting(Constant.OverrideProcNumEnvVar, "TRUE"));
                        return env;
                    }
                    var environment = ConstructEnvironmentVariable();

                    ResourceFile GetResourceFileReference(string containerName, string blobPrefix)
                    {
                        var sasToken = AzureStorageUtil.ConstructContainerSas(this.cloudStorageAccount, containerName, SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read);
                        ResourceFile rf;
                        if (string.IsNullOrEmpty(blobPrefix))
                        {
                            rf = ResourceFile.FromStorageContainerUrl(sasToken);
                        }
                        else
                        {
                            rf = ResourceFile.FromStorageContainerUrl(sasToken, blobPrefix: blobPrefix);
                        }

                        return rf;
                    }

                    async Task<string> CreateJobAsync()
                    {
                        //TODO: need a function to test if all parameters are legal.
                        if (startInfo.MaxUnits != null && startInfo.MaxUnits <= 0)
                        {
                            throw new ArgumentException("Maxunit value is invalid.");
                        }
                        string newJobId = AzureBatchSessionJobIdConverter.ConvertToAzureBatchJobId(AzureBatchSessionIdGenerator.GenerateSessionId());
                        Debug.Assert(batchClient != null, nameof(batchClient) + " != null");
                        var job = batchClient.JobOperations.CreateJob(newJobId, new PoolInformation() { PoolId = AzureBatchConfiguration.BatchPoolName });
                        job.JobPreparationTask = new JobPreparationTask(OpenNetTcpPortSharingAndDisableStrongNameValidationCmdLine);
                        job.JobPreparationTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
                        job.JobPreparationTask.ResourceFiles = new List<ResourceFile>() { GetResourceFileReference(ServiceRegistrationContainer, null) };

                        // List<ResourceFile> resourceFiles = new List<ResourceFile>();
                        // resourceFiles.Add(GetResourceFileReference(RuntimeContainer, BrokerFolder));
                        // resourceFiles.Add(GetResourceFileReference(ServiceRegistrationContainer, null));

                        // // job.JobManagerTask = new JobManagerTask("Broker",
                        // // $@"cmd /c {AzureBatchTaskWorkingDirEnvVar}\broker\HpcBroker.exe -d --ServiceRegistrationPath {AzureBatchTaskWorkingDirEnvVar} --AzureStorageConnectionString {AzureBatchConfiguration.SoaBrokerStorageConnectionString} --EnableAzureStorageQueueEndpoint True --SvcHostList {string.Join(",", nodes.Select(n => n.IPAddress))}");
                        // job.JobManagerTask = new JobManagerTask("List",
                        // $@"cmd /c dir & set");
                        // job.JobManagerTask.ResourceFiles = resourceFiles;
                        // job.JobManagerTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));

                        // Set Meta Data
                        if (job.Metadata == null)
                        {
                            job.Metadata = new List<MetadataItem>();
                        }

                        Dictionary<string, string> jobMetadata = new Dictionary<string, string>()
                                                                     {
                                                                         { BrokerSettingsConstants.ShareSession, startInfo.ShareSession.ToString() },
                                                                         { BrokerSettingsConstants.Secure, startInfo.Secure.ToString() },
                                                                         { BrokerSettingsConstants.TransportScheme, ((int)startInfo.TransportScheme).ToString() },
                                                                         { BrokerSettingsConstants.UseAzureQueue, (startInfo.UseAzureQueue == true).ToString() },
                                                                     };
                        if (startInfo.ServiceVersion != null)
                        {
                            jobMetadata.Add(BrokerSettingsConstants.ServiceVersion, startInfo.ServiceVersion?.ToString());
                        }

                        if (startInfo.MaxUnits != null)
                        {
                            jobMetadata.Add("MaxUnits", startInfo.MaxUnits.ToString());
                        }

                        Dictionary<string, int?> jobOptionalMetadata = new Dictionary<string, int?>()
                                                                           {
                                                                               { BrokerSettingsConstants.ClientIdleTimeout, startInfo.ClientIdleTimeout },
                                                                               { BrokerSettingsConstants.SessionIdleTimeout, startInfo.SessionIdleTimeout },
                                                                               { BrokerSettingsConstants.MessagesThrottleStartThreshold, startInfo.MessagesThrottleStartThreshold },
                                                                               { BrokerSettingsConstants.MessagesThrottleStopThreshold, startInfo.MessagesThrottleStopThreshold },
                                                                               { BrokerSettingsConstants.ClientConnectionTimeout, startInfo.ClientConnectionTimeout },
                                                                               { BrokerSettingsConstants.ServiceConfigMaxMessageSize, startInfo.MaxMessageSize },
                                                                               { BrokerSettingsConstants.ServiceConfigOperationTimeout, startInfo.ServiceOperationTimeout },
                                                                               { BrokerSettingsConstants.DispatcherCapacityInGrowShrink, startInfo.DispatcherCapacityInGrowShrink }
                                                                           };

                        job.Metadata = job.Metadata.Concat(jobMetadata.Select(p => new MetadataItem(p.Key, p.Value)))
                            .Concat(jobOptionalMetadata.Where(p => p.Value.HasValue).Select(p => new MetadataItem(p.Key, p.Value.ToString()))).ToList();

                        job.DisplayName = $"{job.Id} - {startInfo.ServiceName} - WCF Service";
                        await job.CommitAsync();
                        return job.Id;
                    }

                    var jobId = await CreateJobAsync();
                    string sessionId = AzureBatchSessionJobIdConverter.ConvertToSessionId(jobId);
                    if (!sessionId.Equals("-1"))
                    {
                        sessionAllocateInfo.Id = sessionId;
                    }
                    else
                    {
                        TraceHelper.TraceEvent(TraceEventType.Error, "[AzureBatchSessionLauncher] .CreateAndSubmitSessionJob: JobId was failed to parse. callId={0}, jobId={1}.", callId, jobId);
                    }

                    Task AddTasksAsync()
                    {
                        int numTasks = startInfo.MaxUnits != null ? (int)startInfo.MaxUnits : nodes.Count;
                       
                        var comparer = new EnvironmentSettingComparer();

                        CloudTask CreateTask(string taskId)
                        {
                            List<ResourceFile> resourceFiles = new List<ResourceFile>();
                            resourceFiles.Add(GetResourceFileReference(RuntimeContainer, CcpServiceHostFolder));
                            resourceFiles.Add(GetResourceFileReference(ServiceAssemblyContainer, startInfo.ServiceName.ToLower()));

                            CloudTask cloudTask = new CloudTask(taskId, $@"cmd /c {AzureBatchTaskWorkingDirEnvVar}\ccpservicehost\CcpServiceHost.exe -standalone");
                            cloudTask.ResourceFiles = resourceFiles;
                            cloudTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Pool));
                            cloudTask.EnvironmentSettings = cloudTask.EnvironmentSettings == null ? environment : environment.Union(cloudTask.EnvironmentSettings, comparer).ToList();
                            return cloudTask;
                        }

                        CloudTask CreateBrokerTask(bool direct)
                        {
                            List<ResourceFile> resourceFiles = new List<ResourceFile>();
                            resourceFiles.Add(GetResourceFileReference(RuntimeContainer, BrokerFolder));

                            string cmd;
                            if (direct)
                            {
                                cmd =
                                    $@"cmd /c {AzureBatchTaskWorkingDirEnvVar}\broker\HpcBroker.exe -d --ServiceRegistrationPath %{AzureBatchJobPrepTaskWorkingDirEnvVar}% --SvcHostList {string.Join(",", nodes.Select(n => n.IPAddress))}";
                            }
                            else
                            {
                                cmd =
                                    $@"cmd /c {AzureBatchTaskWorkingDirEnvVar}\broker\HpcBroker.exe -d --ServiceRegistrationPath %{AzureBatchJobPrepTaskWorkingDirEnvVar}% --AzureStorageConnectionString {AzureBatchConfiguration.SoaBrokerStorageConnectionString} --EnableAzureStorageQueueEndpoint True --SvcHostList {string.Join(",", nodes.Select(n => n.IPAddress))}";
                            }

                            CloudTask cloudTask = new CloudTask("Broker", cmd);
                            cloudTask.ResourceFiles = resourceFiles;
                            cloudTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Pool));
                            cloudTask.EnvironmentSettings = cloudTask.EnvironmentSettings == null ? environment : environment.Union(cloudTask.EnvironmentSettings, comparer).ToList();
                            return cloudTask;
                        }

                        //TODO: task id type should be changed from int to string
                        var tasks = Enumerable.Range(0, numTasks - 1).Select(_ => CreateTask(Guid.NewGuid().ToString())).ToArray();
                        if (!brokerPerfMode)
                        {
                            tasks = tasks.Union(new[] { CreateBrokerTask(true) }).ToArray();
                        }
                        else
                        {
                            tasks = tasks.Union(new[] { CreateTask(Guid.NewGuid().ToString()) }).ToArray();
                        }

                        return batchClient.JobOperations.AddTaskAsync(jobId, tasks);
                    }

                    await AddTasksAsync();

                    async Task StartAndWaitLocalBrokerLauncher()
                    {
                        if (this.brokerLauncherProcess == null || this.brokerLauncherProcess.HasExited)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Information, "[AzureBatchSessionLauncher] .StartAndWaitLocalBrokerLauncher: Waiting Batch Job Starting");                           
                            string cmd = $@"-d --ServiceRegistrationPath %{AzureBatchJobPrepTaskWorkingDirEnvVar}%  --SessionAddress {Environment.MachineName}";
                            string brokerPath = AzureBatchConfiguration.BrokerLauncherPath;

                            if (string.IsNullOrWhiteSpace(brokerPath) || !File.Exists(brokerPath))
                            {
                                throw new InvalidOperationException($"{brokerPath} doesn't exist.");
                            }

                            var brokerStartInfo = new ProcessStartInfo();
                            brokerStartInfo.FileName = brokerPath;
                            brokerStartInfo.Arguments = cmd;
                            brokerStartInfo.EnvironmentVariables[AzureBatchJobPrepTaskWorkingDirEnvVar] = SessionLauncherSettings.Default.ServiceRegistrationStoreFacadeFolder;
                            brokerStartInfo.UseShellExecute = false;

                            // var brokerLauncherProcess = Process.Start(brokerPath, cmd);
                            this.brokerLauncherProcess = Process.Start(brokerStartInfo);
                        }

                        // await Task.Delay(TimeSpan.FromSeconds(60));
                        sessionAllocateInfo.BrokerLauncherEpr = new[] { SoaHelper.GetBrokerLauncherAddress(Environment.MachineName) };
                    }

                    async Task WaitBatchBrokerLauncher()
                    {
                        var brokerTask = await batchClient.JobOperations.GetTaskAsync(jobId, "Broker");
                        TaskStateMonitor monitor = batchClient.Utilities.CreateTaskStateMonitor();
                        await monitor.WhenAll(new[] { brokerTask }, TaskState.Running, SchedulingTimeout);

                        await brokerTask.RefreshAsync();

                        var brokerNodeIp = nodes.First(n => n.AffinityId == brokerTask.ComputeNodeInformation.AffinityId).IPAddress;
                        sessionAllocateInfo.BrokerLauncherEpr = new[] { SoaHelper.GetBrokerLauncherAddress(brokerNodeIp) };
                    }

                    if (brokerPerfMode)
                    {
                        await StartAndWaitLocalBrokerLauncher();
                    }
                    else
                    {
                        await WaitBatchBrokerLauncher();
                    }

                    return sessionAllocateInfo;
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, $"[{nameof(AzureBatchSessionLauncher)}] .{nameof(this.CreateAndSubmitSessionJob)}: Exception happens: {ex.ToString()}");
                throw;
            }
        }

        private class EnvironmentSettingComparer : IEqualityComparer<EnvironmentSetting>
        {
            public bool Equals(EnvironmentSetting x, EnvironmentSetting y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.Name == y.Name;
            }

            public int GetHashCode(EnvironmentSetting obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        protected override void AddSessionToPool(string serviceNameWithVersion, bool durable, string sessionId, int poolSize)
        {
            throw new NotSupportedException("Currently Session Launcher does not support session pool on Azure Batch.");
        }

        protected override bool TryGetSessionAllocateInfoFromPooled(
            string endpointPrefix,
            bool durable,
            SessionAllocateInfoContract sessionAllocateInfo,
            string serviceConfigFile,
            ServiceRegistration registration,
            out SessionAllocateInfoContract allocateInternal)
        {
            throw new NotSupportedException("Currently Session Launcher does not support session pool on Azure Batch.");
        }

        protected override void CheckAccess()
        {
            // No authentication on Azure Batch for now
        }

        protected internal override ServiceRegistrationRepo CreateServiceRegistrationRepo(string regPath) =>
            new ServiceRegistrationRepo(
                regPath,
                new AzureBlobServiceRegistrationStore(SessionLauncherRuntimeConfiguration.SessionLauncherStorageConnectionString),
                SessionLauncherSettings.Default.ServiceRegistrationStoreFacadeFolder);

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
    }
}