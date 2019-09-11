//-----------------------------------------------------------------------
// <copyright file="SoaHelper.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Utilities for both on-premise and azure environment.</summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.AccessControl;
    using System.Security.Authentication;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;

    using Microsoft.Win32;

    using TelepathyCommon;
    using TelepathyCommon.HpcContext;
    using TelepathyCommon.HpcContext.Extensions;
    using TelepathyCommon.Registry;
    using TelepathyCommon.Service;
    /// <summary>
    /// It is a helper class, shared by broker and proxy.
    /// </summary>
    public static class SoaHelper
    {
        /// <summary>
        /// Environment Variable to pass the localtion of the process.
        /// </summary>
        private const string OnAzureEnvVar = "CCP_ONAZURE";

        /// <summary>
        /// Environment Variable to pass the localtion of the scheduler.
        /// </summary>
        private const string SchedulerOnAzureEnvVar = "CCP_SCHEDULERONAZURE";

        /// <summary>
        /// Stores the name of local host.
        /// </summary>
        private const string LocalHost = "localhost";
        
        /// <summary>
        /// The on-premise port for the session launcher (HN).
        /// </summary>
        private const int SessionLauncherPortOnPremise = 9090;

        /// <summary>
        /// The on-premise port for the scheduler delegation (HN).
        /// </summary>
        private const int SchedulerDelegationPortOnPremise = 9092;

        /// <summary>
        /// The on-premise port for the data service (HN).
        /// </summary>
        private const int DataServicePortOnPremise = 9094;

        /// <summary>
        /// The on-premise port for the broker launcher (BN).
        /// </summary>
        private const int BrokerLauncherPortOnPremise = 9087;

        /// <summary>
        /// The on-premise port for the broker worker (BN).
        /// </summary>
        private const int BrokerWorkerPortOnPremise = 9091;

        /// <summary>
        /// The on-premise port for the broker management (BN).
        /// </summary>
        private const int BrokerManagementPortOnPremise = 9093;

        /// <summary>
        /// The on-premise port for the soa diag service (BN).
        /// </summary>
        private const int DiagServicePortOnPremise = 9095;

        /// <summary>
        /// The on-premise port for the soa diag cleanup service (CN).
        /// </summary>
        private const int DiagCleanupServicePortOnPremise = 9096;

        internal const string AllowPrivilegePrefix = "A";

        internal const string DenyPrivilegePrefix = "D";

        /// <summary>
        /// Use following continuous ports on Azure, so we can use FixedPortRange to declare the
        /// internal endpoint in order to avoid hit limit (maximum 5 internal endpoints per role).
        /// SessionService: 4090-4091
        /// BrokerService:  4092-4094
        /// </summary>
        private const int SessionLauncherPortAzure = 4090;

        private const int SchedulerDelegationPortAzure = 4091;

        private const int BrokerLauncherPortAzure = 4092;

        private const int BrokerWorkerPortAzure = 4093;

        private const int BrokerManagementPortAzure = 4094;

        internal static int HttpsDefaultPort => 443;

        private const int HttpDefaultPort = 80;

        private const int ClusterIdentityHashLength = 13;

        internal static int SessionLauncherPort(bool isSchedulerOnAzure)
        {
            return isSchedulerOnAzure ? SessionLauncherPortAzure : SessionLauncherPortOnPremise;
        }

        private static int SessionLauncherPort(bool isSchedulerOnAzure, Binding binding)
        {
            if (binding == null)
            {
                return SessionLauncherPort(isSchedulerOnAzure);
            }

            int port = 0;
            switch (binding.Scheme.ToLowerInvariant())
            {
                case "net.tcp":
                    port = isSchedulerOnAzure ? SessionLauncherPortAzure : SessionLauncherPortOnPremise;
                    break;
                case "http":
                    port = HttpDefaultPort;
                    break;
                case "https":
                    port = HttpsDefaultPort;
                    break;
                default:
                    break;
            }
            return port;
        }

        private static int SchedulerDelegationPort(bool isSchedulerOnAzure)
        {
            return isSchedulerOnAzure ? SchedulerDelegationPortAzure : SchedulerDelegationPortOnPremise;
        }

        private static int BrokerLauncherPort(bool isSchedulerOnAzure)
        {
            return isSchedulerOnAzure ? BrokerLauncherPortAzure : BrokerLauncherPortOnPremise;
        }

        private static int BrokerLauncherPort(bool isSchedulerOnAzure, Binding binding)
        {
            if (binding == null)
            {
                return BrokerLauncherPort(isSchedulerOnAzure);
            }

            int port = 0;
            switch (binding.Scheme.ToLowerInvariant())
            {
                case "net.tcp":
                    port = isSchedulerOnAzure ? BrokerLauncherPortAzure : BrokerLauncherPortOnPremise;
                    break;
                case "http":
                    port = HttpDefaultPort;
                    break;
                case "https":
                    port = HttpsDefaultPort;
                    break;
                default:
                    break;
            }
            return port;
        }

        private static int BrokerWorkerPort(bool isSchedulerOnAzure)
        {
            return isSchedulerOnAzure ? BrokerWorkerPortAzure : BrokerWorkerPortOnPremise;
        }

        private static int BrokerManagementPort(bool isSchedulerOnAzure)
        {
            return isSchedulerOnAzure ? BrokerManagementPortAzure : BrokerManagementPortOnPremise;
        }

        /// <summary>
        /// This prefix indicates the Azure cluster headnode.
        /// We are using "net.tcp://" for net.tcp connection,
        /// and will use "http://" for http connection in future.
        /// </summary>
        private const string AzureHeadNodePrefix = "net.tcp://";

        /// <summary>
        /// The regular expression of the Azure service epr.
        /// </summary>
        private const string AzureEprRegex = @"^.+\.cloudapp\.net:(\d)+/.+$";

        /// <summary>
        /// The uri prefix for net tcp binding
        /// </summary>
        internal static string NetTcpPrefix => "net.tcp://";

        /// <summary>
        /// The uri prefix for https binding
        /// </summary>
        internal static string HttpsPrefix => "https://";

        //private const string IaaSHeadnodeRegex = @"^.+\.cloudapp\.net$";

        /// <summary>
        /// Stores the registry path to find the scheduler header
        /// </summary>
        private const string CommonRegistryPath = @"SOFTWARE\Microsoft\HPC";

        /// <summary>
        /// Stores the head node name string to find the scheduler header
        /// </summary>
        private const string HeadNodeNameString = @"ClusterName";

        private const string ClusterConnectionString = @"ClusterConnectionString";

        /// <summary>
        /// Stores the registry value name indicating ActiveRole. The registry
        /// key InstalledRole indicates the role capability of the node during
        /// installation time, but a node's role can be changed after
        /// installation, the current role of the node is tracked by a new
        /// registry key ActiveRole. Any code which used to use InstalledRole
        /// to detect a node's current role should be fixed to use the
        /// ActiveRole key. This also applies to HA HN and HA BN. (#22237)
        /// </summary>
        private const string ActiveRoleString = "ActiveRole";

        /// <summary>
        /// Stores the address format of the session launcher service.
        /// </summary>
        internal static string SessionLauncherAddressFormat => "{0}{1}:{2}/SessionLauncher";

        /// <summary>
        /// Stores the NetHttp address format of the session launcher service.
        /// </summary>
        internal static string SessionLauncherNetHttpAddressFormat => "{0}{1}:{2}/SessionLauncher/NetHttp";

        /// <summary>
        /// Stores the address format of the session launcher internal service.
        /// </summary>
        internal static string SessionLauncherInternalAddressFormat => "{0}{1}:{2}/SessionLauncher/Internal";
        
        /// <summary>
        /// Stores the address format of the session launcher AAD service.
        /// </summary>
        internal static string SessionLauncherAadAddressFormat => "{0}{1}:{2}/SessionLauncher/AAD";

        /// <summary>
        /// Stores the address format of the scheduler delegation service.
        /// </summary>
        private const string SchedulerDelegationAddressFormat = "net.tcp://{0}:{1}/SchedulerDelegation";

        /// <summary>
        /// Stores the address format of the internal scheduler delegation service.
        /// </summary>
        private const string SchedulerDelegationInternalAddressFormat = "net.tcp://{0}:{1}/SchedulerDelegation/Internal";

        /// <summary>
        /// Stores the address format of the broker launcher service.
        /// </summary>
        internal static string BrokerLauncherAddressFormat => "{0}{1}:{2}/BrokerLauncher";

        /// <summary>
        /// Stores the NetHttp address format of the broker launcher service.
        /// </summary>
        internal static string BrokerLauncherNetHttpAddressFormat => "{0}{1}:{2}/BrokerLauncher/NetHttp";

        /// <summary>
        /// Stores the address format of the internal broker launcher service.
        /// </summary>
        internal static string BrokerLauncherInternalAddressFormat => "{0}{1}:{2}/BrokerLauncher/Internal";

        /// <summary>
        /// Stores the address format of the AAD broker launcher service.
        /// </summary>
        public static string BrokerLauncherAadAddressFormat => "{0}{1}:{2}/BrokerLauncher/AAD";

        /// <summary>
        /// Stores the address format of the broker worker service.
        /// </summary>
        private const string BrokerWorkerAddressFormat = "{0}{1}:{2}/Broker";

        /// <summary>
        /// Stores the address format of the broker worker internal service.
        /// </summary>
        private const string BrokerWorkerInternalAddressFormat = "{0}{1}:{2}/Broker/Internal";
        
        /// <summary>
        /// Stores the address format of the broker worker internal service.
        /// </summary>
        private const string BrokerWorkerAadAddressFormat = "{0}{1}:{2}/Broker/AAD";

        /// <summary>
        /// Stores the address format of the broker controller service.
        /// </summary>
        private const string BrokerControllerAddressFormat = "net.tcp://{0}:{1}/{2}/NetTcp/Controller";

        /// <summary>
        /// Stores the address format of the broker response service.
        /// </summary>
        private const string BrokerGetResponseAddressFormat = "net.tcp://{0}:{1}/{2}/NetTcp/GetResponse";

        /// <summary>
        /// Stores the address format of the broker management service.
        /// </summary>
        private const string BrokerManagementAddressFormat = "http://localhost:{0}/BrokerManagementService/{1}";

        /// <summary>
        /// Stores the address format of the data service.
        /// </summary>
        private const string DataServiceAddressFormat = "{0}{1}:{2}/DataService";

        /// <summary>
        /// Stores the NetHttp address format of the data service.
        /// </summary>
        private const string DataServiceNetHttpAddressFormat = "{0}{1}:{2}/DataService/NetHttp";

        /// <summary>
        /// Stores the address format of the soa diag service.
        /// </summary>
        private const string DiagServiceAddressFormat = "net.tcp://{0}:{1}/SoaDiagService";

        /// <summary>
        /// Stores the address format of the soa cleanup diag service.
        /// </summary>
        private const string DiagCleanupServiceAddressFormat = "net.tcp://{0}:{1}/SoaDiagCleanupService";

        /// <summary>
        /// It is only used at service side. It indicates if current process is running on Azure. Check Env Var "CCP_ONAZURE".
        /// </summary>
        /// <returns>on azure or not</returns>
        public static bool IsOnAzure()
        {
            return (Environment.GetEnvironmentVariable(OnAzureEnvVar) == "1");
        }

        public static bool IsSchedulerOnAzure()
        {
            return (Environment.GetEnvironmentVariable(SchedulerOnAzureEnvVar) == "1");
        }

        /// <summary>
        /// Get the info of on Azure or not by the uri.
        /// </summary>
        /// <returns>on Azure or not</returns>
        public static bool IsSchedulerOnAzure(Uri uri)
        {
            if (IsSchedulerOnAzure())
            {
                return true;
            }
            else
            {
                return (new Regex(AzureEprRegex, RegexOptions.IgnoreCase)).IsMatch(uri.ToString());
            }
        }

        /// <summary>
        /// Get the info of on Azure or not.
        /// </summary>
        /// <param name="uriString">Uri string</param>
        /// <param name="inprocBroker">is using inprocess broker</param>
        /// <returns>on Azure or not</returns>
        public static bool IsSchedulerOnAzure(string uriString, bool inprocBroker)
        {
            if (inprocBroker)
            {
                // We support Azure inproc broker only when client is also on Azure,
                // so just call following method to check env var.
                return IsSchedulerOnAzure();
            }
            else
            {
                if (string.IsNullOrEmpty(uriString))
                {
                    return false;
                }
                else
                {
                    return IsSchedulerOnAzure(new Uri(uriString));
                }
            }
        }

        /// <summary>
        /// Get the info of on Azure or not by the scheduler name.
        /// </summary>
        /// <returns>on Azure or not</returns>
        public static bool IsSchedulerOnAzure(string schedulerName)
        {
            if (IsSchedulerOnAzure())
            {
                return true;
            }
            else
            {
                return schedulerName.StartsWith(AzureHeadNodePrefix, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Get if the scheduler is on Azure IaaS
        /// </summary>
        /// <param name="schedulerName"></param>
        /// <returns></returns>
        public static bool IsSchedulerOnIaaS(string schedulerName)
        {
            return (new Regex(GetIaaSHeadnodeRegex(), RegexOptions.IgnoreCase)).IsMatch(schedulerName);
        }

        private static string GetAzureDomain(string name, string defaultValue)
        {
            string result = defaultValue;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\HPC"))
            {
                if (key != null)
                {
                    result = (string)key.GetValue(name, result);
                }
            }

            return result;
        }

        private class AzureDnsSuffixes
        {
            public const string ServiceDomain = "cloudapp.net";
            public const string QueuePostfix = "queue.core.windows.net";
            public const string TablePostfix = "table.core.windows.net";
            public const string FilePostfix = "file.core.windows.net";
            public const string BlobPostfix = "blob.core.windows.net";
            public const string ManagementPostfix = "management.core.windows.net";
            public const string SQLAzurePostfix = "database.windows.net";
            public const string SQLAzureManagementPostfix = "management.database.windows.net";
            public const string AzureIaaSDomains = "cloudapp.net|cloudapp.azure.com|chinacloudapp.cn";
            public const string AzureADAuthority = "https://login.windows.net/";
            public const string AzureADResource = "https://management.azure.com/";
        }

        /// <summary>
        /// Get IaaS head node regular expression
        /// </summary>
        /// <returns>IaaS head node regular expression</returns>
        public static string GetIaaSHeadnodeRegex()
        {
            string[] domainNames = GetAzureDomain("IaaSDomainNames", AzureDnsSuffixes.AzureIaaSDomains).Replace(".", @"\.").Split(new char[] { '|', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
            return string.Format(@"^.+\.({0})$", string.Join("|", domainNames));
        }

        /// <summary>
        /// Update the IaaS returned EPR with the cloud service name
        /// </summary>
        /// <param name="epr">IaaS returned EPR</param>
        /// <param name="suffix">Indicates the epr suffix</param>
        /// <returns>Updated EPR string</returns>
        public static string UpdateEprWithCloudServiceName(string epr, string suffix)
        {
            if (string.IsNullOrEmpty(epr))
            {
                return epr;
            }
            string pattern = @"(?<=://).+?(?=:|/)";
            string updatedEpr = epr;
            Match m = Regex.Match(epr, pattern, RegexOptions.IgnoreCase);
            if (m.Success)
            {
                string serverName = m.ToString();
                string serviceName = serverName + suffix;
                updatedEpr = updatedEpr.Replace(serverName, serviceName);
            }
            return updatedEpr;
        }

        /// <summary>
        /// Update the IaaS returned EPR with the cloud service name
        /// </summary>
        /// <param name="eprs">IaaS returned EPRs</param>
        /// <param name="suffix">IaaS domain suffix</param>
        public static void UpdateEprWithCloudServiceName(string[] eprs, string suffix)
        {
            for (int i = 0; i < eprs.Length; i++)
            {
                if (!String.IsNullOrEmpty(eprs[i]))
                {
                    eprs[i] = SoaHelper.UpdateEprWithCloudServiceName(eprs[i], suffix);
                }
            }
        }

        public static EndpointAddress CreateEndpointAddress(Uri uri, bool secure, bool certIdentity)
        {
            if (certIdentity)
            {
                string dnsIdentityName =
                    WcfChannelModule.GetCertDnsIdentityName(
                        new NonHARegistry().GetValueAsync<string>(TelepathyConstants.HpcFullKeyName, TelepathyConstants.SslThumbprint, CancellationToken.None, null).GetAwaiter().GetResult(),
                        StoreName.My,
                        StoreLocation.LocalMachine);
                return new EndpointAddress(uri, EndpointIdentity.CreateDnsIdentity(dnsIdentityName));
            }
            if (secure)
            {
                return new EndpointAddress(uri, EndpointIdentity.CreateSpnIdentity("HOST/" + uri.Host));
            }
            else
            {
                return new EndpointAddress(uri);
            }
        }

        public static EndpointAddress CreateInternalCertEndpointAddress(Uri uri, string certThumbprint)
        {
            string dnsIdentityName = WcfChannelModule.GetCertDnsIdentityName(certThumbprint, StoreName.My, StoreLocation.LocalMachine);
            return new EndpointAddress(uri, EndpointIdentity.CreateDnsIdentity(dnsIdentityName));
        }

        #region address
        public static string GetSessionLauncherAddress()
        {
            return GetSessionLauncherAddress(AzureAddress);
        }

        public static string GetSessionLauncherAddress(string hostname)
        {
            return string.Format(SessionLauncherAddressFormat, NetTcpPrefix, hostname, SessionLauncherPort(IsSchedulerOnAzure(hostname)));
        }
        public static string GetSessionLauncherInternalAddress(string hostname)
        {
            return string.Format(SessionLauncherInternalAddressFormat, NetTcpPrefix, hostname, SessionLauncherPort(IsSchedulerOnAzure(hostname)));
        }

        public static string GetSessionLauncherAddress(string hostname, string endpointPrefix)
        {
            return string.Format(SessionLauncherAddressFormat, endpointPrefix, hostname, SessionLauncherPort(IsSchedulerOnAzure(hostname)));
        }

        [Obsolete("This is not aad ready.")]
        public static string GetSessionLauncherAddress(string hostname, TransportScheme scheme)
        {
            Debug.WriteLine($"[{nameof(SoaHelper)}.{nameof(GetSessionLauncherAddress)}] Entered with hostname {hostname}, scheme {scheme}, callstack{Environment.NewLine} {Environment.StackTrace}");
            if ((scheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                return string.Format(SoaHelper.IsCurrentUserLocal() ? SessionLauncherInternalAddressFormat : SessionLauncherAddressFormat, NetTcpPrefix, hostname, SessionLauncherPort(IsSchedulerOnAzure(hostname)));
            }
            else if ((scheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                return string.Format(SessionLauncherNetHttpAddressFormat, HttpsPrefix, hostname, HttpsDefaultPort);
            }
            else if ((scheme & TransportScheme.Http) == TransportScheme.Http)
            {
                return string.Format(SessionLauncherAddressFormat, HttpsPrefix, hostname, HttpsDefaultPort);
            }
            else if ((scheme & TransportScheme.Custom) == TransportScheme.Custom)
            {
                return string.Format(SessionLauncherAddressFormat, NetTcpPrefix, hostname, SessionLauncherPort(IsSchedulerOnAzure(hostname)));
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetSessionLauncherAddress(string hostname, Binding binding)
        {
            if (binding == null)
            {
                return GetSessionLauncherAddress(hostname);
            }

            return string.Format(SessionLauncherAddressFormat, binding.Scheme + @"://", hostname, SessionLauncherPort(IsSchedulerOnAzure(hostname), binding));
        }
        public static string GetSessionLauncherInternalAddress(string hostname, Binding binding)
        {
            if (binding == null)
            {
                return GetSessionLauncherInternalAddress(hostname);
            }

            return string.Format(SessionLauncherInternalAddressFormat, binding.Scheme + @"://", hostname, SessionLauncherPort(IsSchedulerOnAzure(hostname), binding));
        }

        public static string GetSchedulerDelegationAddress(string hostname)
        {
            return string.Format(SchedulerDelegationAddressFormat, hostname, SchedulerDelegationPort(IsSchedulerOnAzure(hostname)));
        }

        public static string GetSchedulerDelegationInternalAddress(string hostname)
        {
            return string.Format(SchedulerDelegationInternalAddressFormat, hostname, SchedulerDelegationPort(IsSchedulerOnAzure(hostname)));
        }

        public static string GetBrokerLauncherAddress()
        {
            return GetBrokerLauncherAddress(AzureAddress);
        }

        public static string GetBrokerLauncherAddress(string hostname)
        {
            return string.Format(BrokerLauncherAddressFormat, NetTcpPrefix, hostname, BrokerLauncherPort(IsSchedulerOnAzure(hostname)));
        }

        public static string GetBrokerLauncherInternalAddress(string hostname)
        {
            return string.Format(BrokerLauncherInternalAddressFormat, NetTcpPrefix, hostname, BrokerLauncherPort(IsSchedulerOnAzure(hostname)));
        }

        public static string GetBrokerLauncherAadAddress(string hostname)
        {
            return string.Format(BrokerLauncherAadAddressFormat, NetTcpPrefix, hostname, BrokerLauncherPort(IsSchedulerOnAzure(hostname)));
        }

        public static string GetBrokerLauncherAddress(string hostname, TransportScheme scheme)
        {
            if ((scheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                return string.Format(BrokerLauncherAddressFormat, NetTcpPrefix, hostname, BrokerLauncherPort(IsSchedulerOnAzure(hostname)));
            }
            else if ((scheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                return string.Format(BrokerLauncherNetHttpAddressFormat, HttpsPrefix, hostname, HttpsDefaultPort);
            }
            else if ((scheme & TransportScheme.Http) == TransportScheme.Http)
            {
                return string.Format(BrokerLauncherAddressFormat, HttpsPrefix, hostname, HttpsDefaultPort);
            }
            else if ((scheme & TransportScheme.Custom) == TransportScheme.Custom)
            {
                return string.Format(BrokerLauncherAddressFormat, NetTcpPrefix, hostname, BrokerLauncherPort(IsSchedulerOnAzure(hostname)));
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetBrokerLauncherAddress(string hostname, Binding binding)
        {
            if (binding == null)
            {
                return GetBrokerLauncherAddress(hostname);
            }

            return string.Format(BrokerLauncherAddressFormat, binding.Scheme + @"://", hostname, BrokerLauncherPort(IsSchedulerOnAzure(hostname), binding));

        }

        public static string GetBrokerWorkerAddress(string hostname)
        {
            return string.Format(BrokerWorkerAddressFormat, NetTcpPrefix, hostname, BrokerWorkerPort(IsSchedulerOnAzure(hostname)));
        }

        public static string GetBrokerWorkerInternalAddress(string hostname)
        {
            return string.Format(BrokerWorkerInternalAddressFormat, NetTcpPrefix, hostname, BrokerWorkerPort(IsSchedulerOnAzure(hostname)));
        }

        public static string GetBrokerWorkerAadAddress(string hostname)
        {
            return string.Format(BrokerWorkerAadAddressFormat, NetTcpPrefix, hostname, BrokerWorkerPort(IsSchedulerOnAzure(hostname)));
        }

        public static string GetBrokerControllerAddress(int sessionId)
        {
            return GetBrokerControllerAddress(LocalHost, sessionId);
        }

        public static string GetBrokerControllerAddress(string hostname, int sessionId)
        {
            return string.Format(BrokerControllerAddressFormat, hostname, BrokerWorkerPort(IsSchedulerOnAzure(hostname)), sessionId);
        }

        public static string GetBrokerGetResponseAddress(int sessionId)
        {
            return GetBrokerGetResponseAddress(LocalHost, sessionId);
        }

        public static string GetBrokerGetResponseAddress(string hostname, int sessionId)
        {
            return string.Format(BrokerGetResponseAddressFormat, hostname, BrokerWorkerPort(IsSchedulerOnAzure(hostname)), sessionId);
        }

        public static string GetBrokerManagementServiceAddress(int pid)
        {
            return string.Format(BrokerManagementAddressFormat, BrokerManagementPort(IsSchedulerOnAzure()), pid);
        }

        /// <summary>
        /// Generate the address of data service.
        /// </summary>
        /// <returns>data service address</returns>
        public static string GetDataServiceAddress()
        {
            return GetDataServiceAddress(string.Empty, TransportScheme.NetTcp);
        }

        /// <summary>
        /// Generate the address of data service.
        /// </summary>
        /// <param name="clusterConnectionString">the cluster connection string</param>
        /// <param name="scheme">Indicates the trasport scheme</param>
        /// <returns>data service address</returns>
        public static string GetDataServiceAddress(string clusterConnectionString, TransportScheme scheme)
        {
            string hostname = TelepathyContext.GetOrAdd(clusterConnectionString).ResolveSessionLauncherNodeAsync().GetAwaiter().GetResult();
            hostname += TryGetIaaSSuffix(clusterConnectionString);

            if ((scheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                return string.Format(DataServiceAddressFormat, NetTcpPrefix, hostname, DataServicePortOnPremise);
            }
            else if ((scheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                return string.Format(DataServiceNetHttpAddressFormat, HttpsPrefix, hostname, HttpsDefaultPort);
            }
            else if ((scheme & TransportScheme.Http) == TransportScheme.Http)
            {
                return string.Format(DataServiceAddressFormat, HttpsPrefix, hostname, HttpsDefaultPort);
            }
            else if ((scheme & TransportScheme.Custom) == TransportScheme.Custom)
            {
                return string.Format(DataServiceAddressFormat, NetTcpPrefix, hostname, DataServicePortOnPremise);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Try get IaaS endpoint suffix from head node epr
        /// </summary>
        /// <param name="headnode"></param>
        /// <returns></returns>
        public static string TryGetIaaSSuffix(string headnode)
        {
            if (IsSchedulerOnIaaS(headnode))
            {
                return GetSuffixFromHeadNodeEpr(headnode);
            }

            return string.Empty;
        }

        /// <summary>
        /// Generate the address of soa diag service.
        /// </summary>
        /// <param name="hostname">machine name</param>
        /// <returns>soa diag service address</returns>
        public static string GetDiagServiceAddress(string hostname)
        {
            return string.Format(DiagServiceAddressFormat, hostname, DiagServicePortOnPremise);
        }

        /// <summary>
        /// Generate the address of soa diag cleanup service.
        /// </summary>
        /// <param name="hostname">machine name</param>
        /// <returns>soa diag service address</returns>
        public static string GetDiagCleanupServiceAddress(string hostname)
        {
            return string.Format(DiagCleanupServiceAddressFormat, hostname, DiagCleanupServicePortOnPremise);
        }

        #endregion

        public static string AzureAddress
        {
            get
            {
                return Environment.GetEnvironmentVariable("CCP_AZURESERVICE");
            }
        }

        /// <summary>
        /// Get the scheduler connection string . In the Azure cluster, it gets the scheduler virtual name.
        /// Multi scheduler instance can exist in the Azure cluster, but only one active scheduler.
        /// </summary>
        /// <param name="isSessionLauncher">
        /// It indicates if the invoker of this method is session launcher.
        /// </param>
        /// <returns></returns>
        public static string GetSchedulerName()
        {
            // Broker gets Azure scheduler virtual name from env var.
            return Environment.GetEnvironmentVariable(TelepathyConstants.SchedulerEnvironmentVariableName);
        }

        /// <summary>
        /// Get cluster name from local physical registry
        /// </summary>
        /// <returns>the cluster name</returns>
        public static string GetClusterName()
        {
            // Broker gets on-premise scheduler name from regitry key.
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(CommonRegistryPath))
            {
                if (regKey != null)
                {
                    string registryValue = regKey.GetValue(HeadNodeNameString) as string;
                    if (String.IsNullOrEmpty(registryValue))
                    {
                        throw new InvalidOperationException("Headnode name in registry is null.");
                    }

                    return registryValue.ToUpper(CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new InvalidOperationException("Cannot find registry key of headnode.");
                }
            }
        }

        /// <summary>
        /// Get the machine name considering the HA HN an HA BN.
        /// (1) For HA HN, %ccp_scheduler% is the virtual network name (set by setup), so can use it.
        /// (2) For HA BN, read Dns.GetHostName to get the virtual network name.
        /// </summary>
        /// <returns>machine name</returns>
        public static string GetMachineName()
        {
            if (IsHeadnode())
            {
                //TODO: SF:
                return Environment.GetEnvironmentVariable(TelepathyConstants.SchedulerEnvironmentVariableName);
            }
            else if (IsBrokernode())
            {
                return Dns.GetHostName();
            }
            else
            {
                return Environment.MachineName;
            }
        }

        /// <summary>
        /// Check if current machine is a WnRole or WnSvRole.
        /// </summary>
        /// <returns>is WnRole or WnSvRole node or not</returns>
        public static bool IsWorkstationNode()
        {
            foreach (string value in GetActiveRoles())
            {
                if (string.Equals(value, "WN", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(value, "SV", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether the current node is head node
        /// </summary>
        /// <returns>returns the value</returns>
        public static bool IsHeadnode()
        {
            return GetActiveRoles().Contains("HN", StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets a value indicating whether the current node is broker node
        /// </summary>
        /// <returns>is broker node or not</returns>
        public static bool IsBrokernode()
        {
            return GetActiveRoles().Contains("BN", StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Calculate md5 hash of a string
        /// </summary>
        /// <param name="value">string to be calculated</param>
        /// <returns>md5 string of the specified string</returns>
        public static string Md5Hash(string value)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                StringBuilder strBuilder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strBuilder.Append(bytes[i].ToString("x2"));
                }

                return strBuilder.ToString();
            }
        }

        /// <summary>
        /// Get Azure request storage name.
        /// </summary>
        /// <remarks>
        /// Request storage is per cluster per azure deployment.
        /// Format: hpcsoa-[md5hash]-request
        /// </remarks>
        /// <param name="clusterId">cluster Id</param>
        /// <param name="azureServiceName">azure service name</param>
        /// <returns>request storage name</returns>
        public static string GetRequestStorageName(string clusterId, string azureServiceName)
        {
            // make sure to change the original string to lower case before
            // generating md5 hash
            string hash = Md5Hash(string.Format("{0}{1}", clusterId, azureServiceName).ToLowerInvariant());

            return string.Format("hpcsoa-{0}-request", hash).ToLowerInvariant();
        }

        /// <summary>
        /// Get Azure response storage name.
        /// </summary>
        /// <remarks>
        /// Response storage is per cluster per session.
        /// Format: hpcsoa-[md5hash]-response-[SessionId]-[RequeueCount]
        /// </remarks>
        /// <param name="clusterId">cluster Id</param>
        /// <param name="sessionId">session Id</param>
        /// <param name="requeueCount">job requeue count</param>
        /// <returns>response storage name</returns>
        public static string GetResponseStorageName(string clusterId, int sessionId, int requeueCount)
        {
            if (sessionId < TelepathyConstants.StandaloneSessionId)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionId));
            }
            else if (sessionId == TelepathyConstants.StandaloneSessionId)
            {
                sessionId = 0;
            }

            return string.Format("{0}-{1}-{2}", GetResponseStoragePrefix(clusterId), sessionId, requeueCount).ToLowerInvariant();
        }

        /// <summary>
        /// Get Azure request queue/blob container name
        /// </summary>
        /// <param name="clusterHash">the hash code from cluster id</param>
        /// <param name="sessionId">the session id</param>
        /// <returns></returns>
        public static string GetRequestStorageName(int clusterHash, int sessionId)
        {
            if (sessionId < TelepathyConstants.StandaloneSessionId)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionId));
            }
            else if (sessionId == TelepathyConstants.StandaloneSessionId)
            {
                sessionId = 0;
            }

            uint uClusterHash = Convert.ToUInt32((long)clusterHash - int.MinValue);
            return string.Format("hpcsoa-{0}-{1}-request", uClusterHash, sessionId);
        }

        /// <summary>
        /// Get Azure response queue/blob container name
        /// </summary>
        /// <param name="clusterHash">the hash code from cluster id</param>
        /// <param name="sessionId">the session id</param>
        /// <returns></returns>
        public static string GetResponseStorageName(int clusterHash, int sessionId)
        {
            uint uClusterHash = Convert.ToUInt32((long)clusterHash - int.MinValue);
            return string.Format("hpcsoa-{0}-{1}-response", uClusterHash, sessionId);
        }

        public static string GetControllerRequestStorageName(int clusterHash, int sessionId)
        {
            if (sessionId < TelepathyConstants.StandaloneSessionId)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionId));
            }
            else if (sessionId == TelepathyConstants.StandaloneSessionId)
            {
                sessionId = 0;
            }

            uint uClusterHash = Convert.ToUInt32((long)clusterHash - int.MinValue);
            return $"hpcsoa-{uClusterHash}-{sessionId}-controller-request";
        }

        public static string GetControllerResponseStorageName(int clusterHash, int sessionId)
        {
            if (sessionId < TelepathyConstants.StandaloneSessionId)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionId));
            }
            else if (sessionId == TelepathyConstants.StandaloneSessionId)
            {
                sessionId = 0;
            }

            uint uClusterHash = Convert.ToUInt32((long)clusterHash - int.MinValue);
            return $"hpcsoa-{uClusterHash}-{sessionId}-controller-response";
        }

        /// <summary>
        /// Get Azure response queue/blob container name
        /// </summary>
        /// <param name="clusterHash">the hash code from cluster id</param>
        /// <param name="sessionId">the session id</param>
        /// <param name="sessionHash">the hash code from session object</param>
        /// <returns></returns>
        public static string GetResponseStorageName(int clusterHash, int sessionId, int sessionHash)
        {
            if (sessionId < TelepathyConstants.StandaloneSessionId)
            {
                throw new ArgumentOutOfRangeException(nameof(sessionId));
            }
            else if (sessionId == TelepathyConstants.StandaloneSessionId)
            {
                sessionId = 0;
            }

            uint uClusterHash = Convert.ToUInt32((long)clusterHash - int.MinValue);
            uint uSessionHash = Convert.ToUInt32((long)sessionHash - int.MinValue);
            return string.Format("hpcsoa-{0}-{1}-response-{2}", uClusterHash, sessionId, uSessionHash);
        }

        /// <summary>
        /// Get messge id from the WCF message
        /// If the message headers does not have the MessgeId, try find it from the customized header.
        /// </summary>
        /// <param name="message">WCF message</param>
        /// <returns>The message Id if found</returns>
        public static UniqueId GetMessageId(Message message)
        {
            return message == null ? null :
                (message.Headers == null ? null :
                (message.Headers.MessageId == null ? GetMessageIdHeaderFromMessage(message) : message.Headers.MessageId));
        }

        /// <summary>
        /// Get the prefix name of the response storage.
        /// </summary>
        /// <remarks>
        /// Format: hpcsoa-[md5hash]-response
        /// </remarks>
        /// <param name="clusterId">cluster Id</param>
        /// <returns>prefix name</returns>
        public static string GetResponseStoragePrefix(string clusterId)
        {
            // make sure to change the original string to lower case before
            // generating md5 hash
            string hash = Md5Hash(clusterId.ToLowerInvariant());

            return string.Format("hpcsoa-{0}-response", hash).ToLowerInvariant();
        }

        /// <summary>
        /// Get Azure storage prefix
        /// </summary>
        /// <param name="clusterHash">the hash code from the cluster id</param>
        /// <returns>The storage prefix</returns>
        public static string GetAzureQueueStoragePrefix(int clusterHash)
        {
            uint uClusterHash = Convert.ToUInt32((long)clusterHash - int.MinValue);
            return string.Format("hpcsoa-{0}-", uClusterHash);
        }

        /// <summary>
        /// Get HeadNode EPR Suffix
        /// </summary>
        /// <param name="headnode"></param>
        /// <returns></returns>
        public static string GetSuffixFromHeadNodeEpr(string headnode) => headnode.Substring(headnode.IndexOf('.') - ClusterIdentityHashLength);

        /// <summary>
        /// Check the windows identity authenticated.
        /// </summary>
        /// <param name="context">WCF operation context </param>
        /// <returns>authenticated or not</returns>
        public static bool CheckWindowsIdentity(OperationContext context)
        {
            WindowsIdentity identity = null;
            return CheckWindowsIdentity(context, out identity);
        }
        
        /// <summary>
        /// Check the x509 identity authenticated.
        /// </summary>
        /// <param name="context">WCF operation context </param>
        /// <returns>authenticated or not</returns>
        public static bool CheckX509Identity(OperationContext context)
        {
            if (context.ServiceSecurityContext.PrimaryIdentity.AuthenticationType.Equals("X509", StringComparison.OrdinalIgnoreCase))
            {
                IIdentity id = null;
                if (CheckCertIdentity(OperationContext.Current, out id))
                {
                    return true;
                }
                else
                {
                    throw new AuthenticationException(String.Format(CultureInfo.InvariantCulture, "Unauthorized certificate: {0}", id?.Name));
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check the windows identity authenticated.
        /// </summary>
        /// <param name="context">WCF operation context </param>
        /// <param name="identity">return the identity</param>
        /// <returns>authenticated or not</returns>
        public static bool CheckWindowsIdentity(OperationContext context, out WindowsIdentity identity)
        {
            if (IsOnAzure())
            {
                // Skip the Windows identity check on Azure.
                identity = null;
                return true;
            }
            else
            {
                if (context.ServiceSecurityContext == null)
                {
                    Debug.WriteLine("[CheckWindowsIdentity] ServiceSecurityContext is null. Return false.");
                    identity = null;
                    return false;
                }
                else
                {
                    identity = context.ServiceSecurityContext.WindowsIdentity;
                    if (identity == null)
                    {
                        Debug.WriteLine("[CheckWindowsIdentity] WindowsIdentity is null. Return false.");
                        return false;
                    }
                    else
                    {
                        Debug.WriteLine("[CheckWindowsIdentity] WindowsIdentity={0}, IsAuthenticated={1}.", identity.Name, identity.IsAuthenticated);
                        Debug.WriteLine("[CheckWindowsIdentity] PrimaryIdentity={0}, IsAuthenticated={1}.", context.ServiceSecurityContext.PrimaryIdentity.Name, context.ServiceSecurityContext.PrimaryIdentity.IsAuthenticated);
                        return identity.IsAuthenticated;
                    }
                }
            }
        }

        internal static bool CheckCertIdentity(OperationContext context, out IIdentity identity)
        {
            if (context.ServiceSecurityContext == null)
            {
                identity = null;
                return false;
            }
            else
            {
                identity = context.ServiceSecurityContext.PrimaryIdentity;
                if (identity == null)
                {
                    return false;
                }
                else
                {
                    return identity.IsAuthenticated;
                }
            }
        }

        /// <summary>
        /// Create a message in certain format
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="methodName">method name</param>
        /// <param name="format">message format</param>
        /// <param name="args">message arguments</param>
        /// <returns>formatted message</returns>
        public static string CreateTraceMessage(string className, string methodName, string format, params object[] args)
        {
            return CreateTraceMessage(className, methodName, string.Format(format, args));
        }

        /// <summary>
        /// Create a message in certain format
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="methodName">method name</param>
        /// <param name="message">raw message</param>
        /// <returns>formatted message</returns>
        public static string CreateTraceMessage(string className, string methodName, string message)
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0}].{1}: {2}", className, methodName, message);
        }

        /// <summary>
        /// Create a message in certain format
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="methodName">method name</param>
        /// <param name="endpointString">endpoint address of targeted host</param>
        /// <param name="messageId">message id</param>
        /// <param name="message">raw message</param>
        /// <returns>formatted message</returns>
        public static string CreateTraceMessage(string className, string methodName, string endpointString, UniqueId messageId, string message)
        {
            string newMessage = string.Format(CultureInfo.InvariantCulture, "epr = {0}, message id = {1}, {2}", endpointString, messageId == null ? string.Empty : messageId.ToString(), message);
            return CreateTraceMessage(className, methodName, newMessage);
        }

        /// <summary>
        /// Get CCP_PACKAGE_ROOT from the env var.
        /// Check both process wide and machine wide env var.
        /// </summary>
        public static string GetCcpPackageRoot()
        {
            string path = Environment.GetEnvironmentVariable(Constant.PackageRootEnvVar);

            if (string.IsNullOrEmpty(path))
            {
                path = Environment.GetEnvironmentVariable(Constant.PackageRootEnvVar, EnvironmentVariableTarget.Machine);
            }

            return path;
        }

        /// <summary>
        /// Parse domain\user to domain and user
        /// </summary>
        /// <param name="domainUser">domain\user</param>
        /// <param name="domain">domain</param>
        /// <param name="user">user</param>
        public static void ParseDomainUser(string domainUser, out string domain, out string user)
        {
            if (domainUser.Contains("\\"))
            {
                string[] nameParts = domainUser.Split(new char[] { '\\' }, StringSplitOptions.None);
                domain = nameParts[0];
                user = nameParts[1];
            }
            else
            {
                domain = string.Empty;
                user = domainUser;
            }
        }

        /// <summary>
        /// Get Azure file share name
        /// </summary>
        /// <param name="clusterId">the cluster id</param>
        /// <param name="userName">the user name</param>
        /// <returns>the azure file share name</returns>
        public static string GetAzureFileShareName(string clusterId, string userName)
        {
            return string.Join("-", clusterId, userName.Replace(@"\", "-").ToLower());
        }

        /// <summary>
        /// Get Action header
        /// </summary>
        /// <param name="message">WCF message</param>
        /// <returns>Action header</returns>
        public static string GetActionHeaderFromMessage(Message message)
        {
            int index = message.Headers.FindHeader(Constant.ActionHeaderName, Constant.HpcHeaderNS);
            string ret = null;

            if (index >= 0)
            {
                ret = message.Headers.GetHeader<string>(index);
            }

            return ret;
        }

        /// <summary>
        /// Get client id from the WCF message
        /// </summary>
        /// <param name="message">WCF message</param>
        /// <returns>the client id</returns>
        public static string GetClientIdHeaderFromMessage(Message message)
        {
            int index = message.Headers.FindHeader(Constant.ClientIdHeaderName, Constant.HpcHeaderNS);
            string ret = null;

            if (index >= 0)
            {
                ret = message.Headers.GetHeader<string>(index);
            }

            return ret;
        }

        /// <summary>
        /// Get client data/the call back id from WCF message
        /// </summary>
        /// <param name="message">WCF message</param>
        /// <returns>client data</returns>
        public static string GetClientDataHeaderFromMessage(Message message)
        {
            int index = message.Headers.FindHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS);
            string ret = null;

            if (index >= 0)
            {
                ret = message.Headers.GetHeader<string>(index);
            }

            return ret;
        }

        /// <summary>
        /// Get message id from WCF messsage header
        /// </summary>
        /// <param name="message">WCF message</param>
        /// <returns>message id</returns>
        public static UniqueId GetMessageIdHeaderFromMessage(Message message)
        {
            int index = message.Headers.FindHeader(Constant.MessageIdHeaderName, Constant.HpcHeaderNS);
            UniqueId ret = null;

            if (index >= 0)
            {
                ret = new UniqueId(message.Headers.GetHeader<string>(index));
            }

            return ret;
        }

        /// <summary>
        /// Get the default exponential backoff retry manager
        /// </summary>
        /// <returns>the default exponential backoff retry manager</returns>
        public static RetryManager GetDefaultExponentialRetryManager()
        {
            return new RetryManager(new ExponentialBackoffRetryTimer(2000, 30000), 5);
        }

        /// <summary>
        /// Get the default infinite period (3 seconds) retry manager
        /// </summary>
        /// <returns></returns>
        public static RetryManager GetDefaultInfinitePeriodRertyManager()
        {
            return new RetryManager(new PeriodicRetryTimer(3000), RetryManager.InfiniteRetries);
        }

        /// <summary>
        /// Check if current user is a local user
        /// </summary>
        /// <returns>true if current user is local, not in domain</returns>
        public static bool IsCurrentUserLocal()
        {
            return Environment.UserDomainName == Environment.MachineName;
        }

        public static bool IsCallingFromInternalWithCert()
        {
            return ServiceSecurityContext.Current.PrimaryIdentity.AuthenticationType == "X509";
        }

        /// <summary>
        /// Returns a list of users granted and denied the specified privilege in specified ACL
        /// </summary>
        /// <param name="sddl">SDDL that represents ACL</param>
        /// <param name="privilege">Inidates the user privilege</param>
        /// <returns>Specified users</returns>
        internal static string[] GetGrantedUsers(string sddl, uint privilege)
        {
            List<string> users = new List<string>();

            // Create security descriptor from SDDL
            CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor(false, false, sddl);

            // Loop through ACL in SecurityDescriptor
            foreach (CommonAce ace in descriptor.DiscretionaryAcl)
            {
                // If this ACE is for the specified privilege
                if (((uint)ace.AccessMask & (uint)privilege) != 0)
                {
                    // If the ACE is explicitly granting or denying the privilege
                    if (ace.AceQualifier == AceQualifier.AccessAllowed || ace.AceQualifier == AceQualifier.AccessDenied)
                    {
                        NTAccount ntAccount = (NTAccount)ace.SecurityIdentifier.Translate(typeof(NTAccount));
                        string username = ntAccount.Value;

                        // Check for domain\username form and extract username only. Otherwise, assume username is not machine dependent.
                        string[] nameParts = username.Split('\\');
                        if (nameParts.Length == 2)
                        {
                            username = String.Format("{0}:{1}", (ace.AceQualifier == AceQualifier.AccessAllowed) ? AllowPrivilegePrefix : DenyPrivilegePrefix, nameParts[1]);
                        }
                        else
                        {
                            username = String.Format("{0}:{1}", (ace.AceQualifier == AceQualifier.AccessAllowed) ? AllowPrivilegePrefix : DenyPrivilegePrefix, username);
                        }

                        users.Add(username);
                    }
                }
            }

            return users.ToArray();
        }

        /// <summary>
        /// Detects whether caller is SOA REST service and on azure and if so returns caller's name. The 
        /// caller's name will always be username only - no machine name or domain.
        /// </summary>
        /// <remarks>The caller can actually be any admin but a product-wide assumption is a admin wont hack the system</remarks>
        /// <returns></returns>
        internal static bool IsCallerSOARESTOnAzure(WindowsIdentity identity, MessageHeaders headers, out string callerUserName)
        {
            bool ret = false;
            string originalCallerIdentity = null;

            if (IsOnAzure())
            {
                // See if it is the SOA REST service by checking for original caller custom header
                int index = headers.FindHeader(Constant.WFE_Role_Caller_HeaderName, Constant.WFE_Role_Caller_HeaderNameSpace);

                if (index >= 0)
                {
                    originalCallerIdentity = headers.GetHeader<string>(index);

                    // As Bug 16728 has been won't fixed. We do not need to check access
                    // here anymore. Just return true is fine.
                    ret = true;
                }
            }

            if (ret)
            {
                callerUserName = originalCallerIdentity;
            }
            else
            {
                callerUserName = String.Empty;
            }

            return ret;
        }

        /// <summary>
        /// Gets the active roles
        /// </summary>
        /// <returns>returns a string array indicating the active roles</returns>
        private static string[] GetActiveRoles()
        {
            //Only used to check if workstation node in ccp service host.
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(CommonRegistryPath))
            {
                if (regKey != null)
                {
                    string[] result = regKey.GetValue(ActiveRoleString) as string[];

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return new string[0];
        }

        #region Service Fabric


        #endregion
    }
}
