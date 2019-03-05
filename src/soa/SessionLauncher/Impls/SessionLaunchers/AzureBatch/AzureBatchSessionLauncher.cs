namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Security;
    using System.Threading.Tasks;

    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Common;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Utils;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    internal class AzureBatchSessionLauncher : SessionLauncher
    {
        private CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(SessionLauncherRuntimeConfiguration.SessionLauncherStorageConnectionString);

        private const string RuntimeContainer = "runtime";

        private const string CcpServiceHostFolder = "ccpservicehost";

        private const string BrokerFolder = "broker";

        private const string ServiceAssemblyContainer = "service-assembly";

        private const string ServiceRegistrationContainer = "service-registration";

        private const string AzureBatchTaskWorkingDirEnvVar = "%AZ_BATCH_TASK_WORKING_DIR%";

        // TODO: remove parameter less ctor and add specific parameters for the sake of test-ablity
        public AzureBatchSessionLauncher()
        {
        }

        public override async Task<SessionAllocateInfoContract> AllocateDurableV5Async(SessionStartInfoContract info, string endpointPrefix)
        {
            throw new NotSupportedException("Currently Session Launcher does not support durable session on Azure Batch.");
        }

        public override async Task<SessionInfoContract> GetInfoV5Sp1Async(string endpointPrefix, int sessionId, bool useAad)
        {
            throw new NotImplementedException();
        }

        public override async Task TerminateV5Async(int sessionId)
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
            throw new NotImplementedException();
        }

        public override async Task<Dictionary<string, string>> GetSOAConfigurationsAsync(List<string> keys)
        {
            throw new NotImplementedException();
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
            TraceHelper.TraceEvent(TraceEventType.Information, "[AzureBatchSessionLauncher] .CreateAndSubmitSessionJob: callId={0}, endpointPrefix={1}, durable={2}.", callId, endpointPrefix, durable);
            using (var batchClient = AzureBatchConfiguration.GetBatchClient())
            {
                var pool = await batchClient.PoolOperations.GetPoolAsync(AzureBatchConfiguration.BatchPoolName);
                ODATADetailLevel detailLevel = new ODATADetailLevel();
                detailLevel.SelectClause = "ipAddress";
                var nodes = await pool.ListComputeNodes(detailLevel).ToListAsync();
                if (nodes.Count < 2)
                {
                    // We don't expect the node running job manager task also performing computing
                    throw new InvalidOperationException("Compute node count in selected pool is less then 2.");
                }

                sessionAllocateInfo.Id = 0;
                sessionAllocateInfo.BrokerLauncherEpr = new[] { SessionInternalConstants.BrokerConnectionStringToken };

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
                    // var hashed = MD5.Create().ComputeHash(Guid.NewGuid().ToByteArray());
                    // string idSuffix = BitConverter.ToUInt16(hashed, 0).ToString().PadLeft(6, '0');
                    // string newJobId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + idSuffix;
                    string newJobId = AzureBatchSessionJobIdConverter.ConvertToAzureBatchJobId(AzureBatchSessionIdGenerator.GenerateSessionId());
                    Debug.Assert(batchClient != null, nameof(batchClient) + " != null");
                    var job = batchClient.JobOperations.CreateJob(newJobId, new PoolInformation() { PoolId = AzureBatchConfiguration.BatchPoolName });
                    job.JobPreparationTask = new JobPreparationTask(
                        @"cmd /c ""sc.exe config NetTcpPortSharing start= demand & reg ADD ^""HKLM\Software\Microsoft\StrongName\Verification\*,*^"" /f & reg ADD ^""HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\*,*^"" /f""");
                    job.JobPreparationTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));

                    // job.JobManagerTask = new JobManagerTask("Broker",
                    // $@"cmd /c broker\HpcBroker.exe -d --ServiceRegistrationPath %AZ_BATCH_APP_PACKAGE_{AzureBatchConstants.SoaAppPackageId.ToUpper()}#{AzureBatchConstants.SoaAppPackageVersion}%\Registration --AzureStorageConnectionString {AzureBatchConfiguration.SoaBrokerStorageConnectionString} --EnableAzureStorageQueueEndpoint True --ReadSvcHostFromEnv");
                    // job.JobManagerTask.ResourceFiles = new List<ResourceFile>(){ GetResourceFileReference(RuntimeContainer, BrokerFolder) };
                    // job.JobManagerTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
                    await job.CommitAsync();
                    return job.Id;
                }

                var jobId = await CreateJobAsync();
                int sessionId = AzureBatchSessionJobIdConverter.ConvertToSessionId(jobId);
                if (sessionId != -1)
                {
                    sessionAllocateInfo.Id = sessionId;
                }
                else
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[AzureBatchSessionLauncher] .CreateAndSubmitSessionJob: JobId was failed to parse. callId={0}, jobId={1}.", callId, jobId);
                }

                Task AddTasksAsync()
                {
                    int numTasks = nodes.Count - 1;
                    var comparer = new EnvironmentSettingComparer();

                    CloudTask CreateMultiInstanceTask(string taskId)
                    {
                        List<ResourceFile> resourceFiles = new List<ResourceFile>();
                        resourceFiles.Add(GetResourceFileReference(RuntimeContainer, CcpServiceHostFolder));
                        resourceFiles.Add(GetResourceFileReference(ServiceAssemblyContainer, "ccpechosvc"));
                        resourceFiles.Add(GetResourceFileReference(RuntimeContainer, BrokerFolder));
                        resourceFiles.Add(GetResourceFileReference(ServiceRegistrationContainer, null));

                        CloudTask multiInstanceTask = new CloudTask(
                            taskId,
                            $@"cmd /c {AzureBatchTaskWorkingDirEnvVar}\broker\HpcBroker.exe -d --ServiceRegistrationPath {AzureBatchTaskWorkingDirEnvVar} --AzureStorageConnectionString {AzureBatchConfiguration.SoaBrokerStorageConnectionString} --EnableAzureStorageQueueEndpoint True --ReadSvcHostFromEnv");
                        multiInstanceTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
                        multiInstanceTask.MultiInstanceSettings = new MultiInstanceSettings(
                            $@"cmd /c start cmd /c {AzureBatchTaskWorkingDirEnvVar}\ccpservicehost\CcpServiceHost.exe -standalone",
                            numTasks);
                        multiInstanceTask.EnvironmentSettings =
                            multiInstanceTask.EnvironmentSettings == null ? environment : environment.Union(multiInstanceTask.EnvironmentSettings, comparer).ToList();
                        multiInstanceTask.ResourceFiles = resourceFiles;
                        return multiInstanceTask;
                    }

                    CloudTask CreateTask(string taskId)
                    {
                        List<ResourceFile> resourceFiles = new List<ResourceFile>();
                        resourceFiles.Add(GetResourceFileReference(RuntimeContainer, CcpServiceHostFolder));
                        resourceFiles.Add(GetResourceFileReference(ServiceAssemblyContainer, "ccpechosvc"));

                        CloudTask cloudTask = new CloudTask(taskId, $@"cmd /c set & dir wd");
                        cloudTask.ResourceFiles = resourceFiles;
                        cloudTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
                        cloudTask.EnvironmentSettings = cloudTask.EnvironmentSettings == null ? environment : environment.Union(cloudTask.EnvironmentSettings, comparer).ToList();
                        return cloudTask;
                    }

                    var tasks = Enumerable.Range(0, numTasks).Select(_ => CreateMultiInstanceTask(Guid.NewGuid().ToString())).ToArray();
                    return batchClient.JobOperations.AddTaskAsync(jobId, tasks);
                }

                await AddTasksAsync();

                return sessionAllocateInfo;
            }
        }

        private class EnvironmentSettingComparer : IEqualityComparer<EnvironmentSetting>
        {
            public bool Equals(EnvironmentSetting x, EnvironmentSetting y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(EnvironmentSetting obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        protected override void AddSessionToPool(string serviceNameWithVersion, bool durable, int sessionId, int poolSize)
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
    }
}