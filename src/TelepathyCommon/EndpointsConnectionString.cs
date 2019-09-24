// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Make this class for connection string format contract.
    ///     Use this class for standard connection string processing.
    ///     This is a struct, so we don't need handle null.
    /// </summary>
    public struct EndpointsConnectionString
    {
        /// <summary>
        ///     indicate the hpc connection string.
        /// </summary>
        private const string ConnectionStringRegexString = @"^([A-Za-z0-9\-\.]+,)*([A-Za-z0-9\-\.]+)$|^$";

        private const string CommonRegistryPath = @"SOFTWARE\Microsoft\HPC";

        private static readonly Regex ConnectionStringRegex = new Regex(ConnectionStringRegexString);

        public const string Delimiter = ",";

        public static EndpointsConnectionString LoadFromEnvVarsOrWindowsRegistry()
        {
            EndpointsConnectionString endpointsConnectionString;
            var connectionString = Environment.GetEnvironmentVariable(TelepathyConstants.ConnectionStringEnvironmentVariableName);

            if (!string.IsNullOrEmpty(connectionString))
            {
                if (TryParseConnectionString(connectionString, out endpointsConnectionString))
                {
                    return endpointsConnectionString;
                }
            }

            var schedulerVar = Environment.GetEnvironmentVariable(TelepathyConstants.SchedulerEnvironmentVariableName);

            if (!string.IsNullOrEmpty(schedulerVar))
            {
                if (TryParseConnectionString(schedulerVar, out endpointsConnectionString))
                {
                    return endpointsConnectionString;
                }
            }

            string registryValue = null;

            using (var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(CommonRegistryPath))
            {
                registryValue = regKey?.GetValue(TelepathyConstants.ClusterConnectionStringRegVal) as string;
            }

            if (!string.IsNullOrEmpty(registryValue) && TryParseConnectionString(registryValue, out endpointsConnectionString))
            {
                return endpointsConnectionString;
            }

            throw new InvalidOperationException(
                $@"None of the following values contains a valid cluster connection string. 
Environment variable {TelepathyConstants.ConnectionStringEnvironmentVariableName}={connectionString},{TelepathyConstants.SchedulerEnvironmentVariableName}={schedulerVar},
RegistryKey {CommonRegistryPath}\{TelepathyConstants.ClusterConnectionStringRegVal}={registryValue}");
        }

        public static bool TryParseConnectionString(string connectionString, out EndpointsConnectionString endpointsConnectionString)
        {
            connectionString = connectionString?.ToUpperInvariant();
            if (connectionString == null)
            {
                Trace.TraceWarning($"{nameof(connectionString)} is null. Default to localhost.");
                connectionString = "localhost";
            }

            if (!ConnectionStringRegex.IsMatch(connectionString))
            {
                endpointsConnectionString = new EndpointsConnectionString();
                return false;
            }

            endpointsConnectionString = new EndpointsConnectionString(connectionString);
            return true;
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

        public string ConnectionString { get; }

        public bool IsGateway => !string.IsNullOrEmpty(this.ConnectionString);
    }
}