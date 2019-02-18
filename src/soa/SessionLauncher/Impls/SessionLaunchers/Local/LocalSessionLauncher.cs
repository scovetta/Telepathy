namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.Local
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Configuration;

    internal class LocalSessionLauncher : SessionLauncher
    {
        private Process brokerLauncherProcess;

        private Process svcHostProcess;

        public override async Task<SessionAllocateInfoContract> AllocateDurableV5Async(SessionStartInfoContract info, string endpointPrefix)
        {
            throw new NotSupportedException("Currently Session Launcher does not support durable session in Local Session mode.");
        }

        public override async Task<SessionInfoContract> GetInfoV5Sp1Async(string endpointPrefix, int sessionId, bool useAad)
        {
            throw new NotImplementedException();
        }

        public override async Task TerminateV5Async(int sessionId)
        {
            this.brokerLauncherProcess.Kill();
            this.svcHostProcess.Kill();
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
            // int sessionId = LocalSessionConfiguration.GetNextSessionId();
            int sessionId = SessionStartInfo.StandaloneSessionId;
            this.brokerLauncherProcess = Process.Start(
                LocalSessionConfiguration.BrokerLauncherExePath,
                $"-d --ServiceRegistrationPath {LocalSessionConfiguration.ServiceRegistrationPath} --AzureStorageConnectionString {LocalSessionConfiguration.BrokerStorageConnectionString} --EnableAzureStorageQueueEndpoint True");
            this.svcHostProcess = Process.Start(LocalSessionConfiguration.ServiceHostExePath, "-standalone");

            sessionAllocateInfo.Id = sessionId;
            sessionAllocateInfo.BrokerLauncherEpr = new[] { SessionInternalConstants.BrokerConnectionStringToken };

            return sessionAllocateInfo;
        }

        protected override void AddSessionToPool(string serviceNameWithVersion, bool durable, int sessionId, int poolSize)
        {
            throw new NotImplementedException();
        }

        protected override bool TryGetSessionAllocateInfoFromPooled(
            string endpointPrefix,
            bool durable,
            SessionAllocateInfoContract sessionAllocateInfo,
            string serviceConfigFile,
            ServiceRegistration registration,
            out SessionAllocateInfoContract allocateInternal)
        {
            throw new NotImplementedException();
        }

        protected override void CheckAccess()
        {
            // No authentication in Local mode
        }
    }
}