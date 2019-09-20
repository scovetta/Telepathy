// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using Microsoft.WindowsAzure.ServiceRuntime;

    internal static class AzureRoleHelper
    {
        /// <summary>
        /// Get the IP address of local machine.
        /// Notice: Don't use method Dns.GetHostAddresses, which returns multi ip addresses.
        /// We can't differentiate which one is correct for the endpoint.
        /// </summary>
        /// <returns>broker node address</returns>
        internal static string GetLocalMachineAddress()
        {
            return GetRoleInstanceAddress(RoleEnvironment.CurrentRoleInstance);
        }

        /// <summary>
        /// Get IP address of specified role instance.
        /// </summary>
        /// <param name="instance">role instance</param>
        /// <returns>
        /// Return IP address if AllPorts endpoint is declared. Otherwise, return empty (not expected).
        /// </returns>
        private static string GetRoleInstanceAddress(RoleInstance instance)
        {
            RoleInstanceEndpoint endpoint;

            if (instance.InstanceEndpoints.TryGetValue(SchedulerEndpointNames.AllPorts, out endpoint))
            {
                return endpoint.IPEndpoint.Address.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private class SchedulerEndpointNames
        {
            public const string NodeManagerService = "Microsoft.Hpc.Azure.Endpoint.Manager";
            public const string ApplicationI = "Microsoft.Hpc.Azure.Endpoint.ApplicationI";
            public const string ApplicationII = "Microsoft.Hpc.Azure.Endpoint.ApplicationII";
            public const string SOADataService = "Microsoft.Hpc.Azure.Endpoint.ApplicationI";  // SOA reuses the application ports
            public const string SOAControlService = "Microsoft.Hpc.Azure.Endpoint.ApplicationII";
            public const string NodeMappingService = "Microsoft.Hpc.Azure.Endpoint.NodeMapping";

            ////public const string SchedulerListenerService = "SchedulerListener";
            // Temporarily all ports hack for Scheduler On Azure
            public const string SchedulerListenerService = "Microsoft.Hpc.Azure.Endpoint.AllPorts";

            public const int NumApplicationPorts = 8; // Change this value also have to change the gap between SOAData and SOAControl in the Module enum in NodeMapping.cs

            public const string HostsDistribution = "Microsoft.Hpc.Azure.Endpoint.HostsDistribution";
            public const string FileTransfer = "Microsoft.Hpc.Azure.Endpoint.FileTransfer";

            public const string HPCWebServiceHttps = "Microsoft.Hpc.Azure.Endpoint.HPCWebServiceHttps";

            public const string AllPorts = "Microsoft.Hpc.Azure.Endpoint.AllPorts";

            public const string ProxyService = "Microsoft.Hpc.Azure.Endpoint.JobManager";

            public const string FileStagingService = "Microsoft.Hpc.Azure.Endpoint.FileStaging";

            public static string ProxyServiceEndpoint = "Microsoft.Hpc.Azure.Endpoint.HpcComponents";

            public static string FileStagingServiceEndpoint = "Microsoft.Hpc.Azure.Endpoint.HpcComponents";
        }
    }
}
