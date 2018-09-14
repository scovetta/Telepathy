namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Win32;

    /// <summary>
    /// Make this class for connection string format contract.
    /// Use this class for standard connection string processing.
    /// This is a struct, so we don't need handle null.
    /// </summary>
    public struct EndpointsConnectionString
    {
        /// <summary>
        /// indicate the hpc connection string.
        /// </summary>
        private const string ConnectionStringRegexString = @"^([A-Za-z0-9\-\.]+,)*([A-Za-z0-9\-\.]+)$|^$";

        private const string CommonRegistryPath = @"SOFTWARE\Microsoft\HPC";

        private static readonly Regex ConnectionStringRegex = new Regex(ConnectionStringRegexString);

        public const string Delimiter = ",";

        public static EndpointsConnectionString LoadFromEnvVarsOrWindowsRegistry()
        {
            EndpointsConnectionString endpointsConnectionString;
            string connectionString = Environment.GetEnvironmentVariable(HpcConstants.ConnectionStringEnvironmentVariableName);

            if (!string.IsNullOrEmpty(connectionString))
            {
                if (TryParseConnectionString(connectionString, out endpointsConnectionString))
                {
                    return endpointsConnectionString;
                }
            }

            var schedulerVar = Environment.GetEnvironmentVariable(HpcConstants.SchedulerEnvironmentVariableName);

            if (!string.IsNullOrEmpty(schedulerVar))
            {
                if (TryParseConnectionString(schedulerVar, out endpointsConnectionString))
                {
                    return endpointsConnectionString;
                }
            }

            string registryValue = null;

            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(CommonRegistryPath))
            {
                registryValue = regKey?.GetValue(HpcConstants.ClusterConnectionStringRegVal) as string;
            }

            if (!string.IsNullOrEmpty(registryValue) && TryParseConnectionString(registryValue, out endpointsConnectionString))
            {
                return endpointsConnectionString;
            }
            else
            {
                throw new InvalidOperationException(
                    $@"None of the following values contains a valid cluster connection string. 
Environment variable {HpcConstants.ConnectionStringEnvironmentVariableName}={connectionString},{HpcConstants.SchedulerEnvironmentVariableName}={schedulerVar},
RegistryKey {CommonRegistryPath}\{HpcConstants.ClusterConnectionStringRegVal}={registryValue}");
            }
        }

        public static bool TryParseConnectionString(string connectionString, out EndpointsConnectionString endpointsConnectionString)
        {
            connectionString = connectionString?.ToUpperInvariant();
            if (!ConnectionStringRegex.IsMatch(connectionString))
            {
                endpointsConnectionString = new EndpointsConnectionString();
                return false;
            }
            else
            {
                endpointsConnectionString = new EndpointsConnectionString(connectionString);
                return true;
            }
        }

        public static EndpointsConnectionString ParseConnectionString(string connectionString)
        {
            EndpointsConnectionString result;
            if (!TryParseConnectionString(connectionString, out result))
            {
                throw new ArgumentException("The supported format of connection string is: host1,host2,host3,...", nameof(connectionString));
            }

            return result;
        }

        public EndpointsConnectionString(IEnumerable<string> nodes)
        {
            this.ConnectionString = string.Join(Delimiter, nodes.Select(n => n?.ToUpperInvariant()));
        }

        private EndpointsConnectionString(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public string ConnectionString { get; private set; }

        public bool IsGateway => !string.IsNullOrEmpty(this.ConnectionString);

        public IEnumerable<string> EndPoints
        {
            get
            {
                var nodes = this.ConnectionString?.Split(Delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (nodes == null || nodes.Length == 0)
                {
                    return null;
                }
                else
                {
                    var localThis = this;
                    return nodes.Select(s => $"{s}:{(localThis.IsGateway ? HpcConstants.HpcNamingServicePort : HpcConstants.FabricClientConnectionPort)}");
                }
            }
        }
    }
}
