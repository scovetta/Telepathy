namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Security;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Common;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;

    internal class AzureBatchSessionLauncher : SessionLauncher
    {
        // TODO: remove parameter less ctor and add specific parameters for the sake of test-ablity
        public AzureBatchSessionLauncher()
        {
        }

        public override async Task<SessionAllocateInfoContract> AllocateV5Async(SessionStartInfoContract info, string endpointPrefix)
        {
            return await this.AllocateInternalAsync(info, endpointPrefix, false);
        }

        public override string[] Allocate(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo)
        {
            var contract = this.AllocateV5Async(info, endpointPrefix).GetAwaiter().GetResult();
            sessionid = contract.Id;
            serviceVersion = contract.ServiceVersion?.ToString();
            sessionInfo = contract.SessionInfo;
            return contract.BrokerLauncherEpr;
        }

        public override async Task<SessionAllocateInfoContract> AllocateDurableV5Async(SessionStartInfoContract info, string endpointPrefix)
        {
            throw new NotSupportedException("Currently Session Launcher does not support durable session on Azure Batch.");
        }

        public override string[] AllocateDurable(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo)
        {
            SessionAllocateInfoContract contract = this.AllocateDurableV5Async(info, endpointPrefix).GetAwaiter().GetResult();
            sessionid = contract.Id;
            serviceVersion = contract.ServiceVersion?.ToString();
            sessionInfo = contract.SessionInfo;
            return contract.BrokerLauncherEpr;
        }

        public override async Task<SessionInfoContract> GetInfoV5Sp1Async(string endpointPrefix, int sessionId, bool useAad)
        {
            throw new NotImplementedException();
        }

        public override async Task TerminateV5Async(int sessionId)
        {
            throw new NotImplementedException();
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

                async Task<string> CreateJobAsync()
                {
                    var hashed = MD5.Create().ComputeHash(Guid.NewGuid().ToByteArray());
                    string idSuffix = BitConverter.ToUInt16(hashed, 0).ToString().PadLeft(6, '0');
                    // string newJobId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + idSuffix;
                    string newJobId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    Debug.Assert(batchClient != null, nameof(batchClient) + " != null");
                    var job = batchClient.JobOperations.CreateJob(newJobId, new PoolInformation() { PoolId = AzureBatchConfiguration.BatchPoolName });
                    await job.CommitAsync();
                    return job.Id;
                }

                var jobId = await CreateJobAsync();
                if (int.TryParse(jobId, out int jobIdL))
                {
                    sessionAllocateInfo.Id = jobIdL;
                }
                else
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[AzureBatchSessionLauncher] .CreateAndSubmitSessionJob: JobId was failed to parse. callId={0}, jobId={1}.", callId, jobId);
                }

                Task AddTasksAsync()
                {
                    int numTasks = nodes.Count - 1;
                    var comparer = new EnvironmentSettingComparer();

                    CloudTask CreateTask(string taskId)
                    {
                        CloudTask multiInstanceTask = new CloudTask(
                            taskId,
                            $@"cmd /c %AZ_BATCH_APP_PACKAGE_{AzureBatchConstants.SoaAppPackageId.ToUpper()}#{AzureBatchConstants.SoaAppPackageVersion}%\BrokerOutput\HpcBroker.exe -d --ServiceRegistrationPath %AZ_BATCH_APP_PACKAGE_{AzureBatchConstants.SoaAppPackageId.ToUpper()}#{AzureBatchConstants.SoaAppPackageVersion}%\Registration --AzureStorageConnectionString {AzureBatchConfiguration.SoaBrokerStorageConnectionString} --EnableAzureStorageQueueEndpoint True --ReadSvcHostFromEnv");
                        multiInstanceTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
                        multiInstanceTask.MultiInstanceSettings = new MultiInstanceSettings(
                            $@"cmd /c start cmd /c %AZ_BATCH_APP_PACKAGE_{AzureBatchConstants.SoaAppPackageId.ToUpper()}#{AzureBatchConstants.SoaAppPackageVersion}%\CcpServiceHost\CcpServiceHost.exe -standalone",
                            numTasks);
                        multiInstanceTask.EnvironmentSettings =
                            multiInstanceTask.EnvironmentSettings == null ? environment : environment.Union(multiInstanceTask.EnvironmentSettings, comparer).ToList();
                        return multiInstanceTask;
                    }

                    var tasks = Enumerable.Range(0, numTasks).Select(_ => CreateTask(Guid.NewGuid().ToString())).ToArray();
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