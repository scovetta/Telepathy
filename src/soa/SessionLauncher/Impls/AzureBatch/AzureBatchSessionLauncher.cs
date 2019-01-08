namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security;
    using System.Threading.Tasks;

    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session.Configuration;

    internal class AzureBatchSessionLauncher : SessionLauncher
    {
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}