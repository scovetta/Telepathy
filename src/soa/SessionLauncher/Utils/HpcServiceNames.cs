// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Utils
{
    public class HpcServiceNames
    {
        public const string HpcSession = "HpcSession";
        public const string HpcScheduler = "HpcAzureScheduler";
        public const string HpcHAController = "HpcHAController";

        public const string HpcNode = "HpcAzureNodeManager";
        public const string HpcRest = "HpcAzureRestService";
        public const string HpcPortal = "HpcAzurePortalService";
        public const string HpcBroker = "HpcBroker";
        public const string HpcSoaRest = "HpcSoaRest";
        public const string HpcSoaDiagMon = "HpcSoaDiagMon";
        public const string Msmpi = "msmpi";

        public const string PortSharing = "NetTcpPortSharing";
    }

    public class HpcAzureServiceDescriptions
    {
        public const string HpcSession = "Hosts the session launcher service.";
        public const string HpcScheduler = "Manages jobs and tasks for Windows HPC Azure clusters.";
        public const string HpcHAController = "Manages the failover of the HPCScheduler service.";

        public const string HpcNode = "Manages processes for applications that run on a Windows HPC Azure cluster.";
        public const string HpcRest = "Manages requests by REST APIs.";
        public const string HpcPortal = "Hosts the Hpc Azure Portal Service.";
        public const string HpcBroker = "Hosts the broker for the sessions.";
        public const string HpcSoaRest = "Manages requests by REST APIs for SOA applications.";
        public const string HpcSoaDiagMon = "Starts the monitor process for the SOA diagnostic tracing.";
        public const string Msmpi = "Starts the manager process for MPI applications that run on a Windows HPC Server cluster.";

        public const string PortSharing = "NetTcpPortSharing.";
    }
}
