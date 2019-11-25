// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using CommandLine;
    using Microsoft.Telepathy.Session.Internal;

    internal class SessionLauncherStartOption : ILogConfigOption
    {
        [Option('s', "AzureBatchServiceUrl", SetName = "AzureBatch")]
        public string AzureBatchServiceUrl { get; set; }

        [Option('n', "AzureBatchAccountName", SetName = "AzureBatch")]
        public string AzureBatchAccountName { get; set; }

        [Option('k', "AzureBatchAccountKey", SetName = "AzureBatch")]
        public string AzureBatchAccountKey { get; set; }

        [Option('j', "AzureBatchJobId", SetName = "AzureBatch")]
        public string AzureBatchJobId { get; set; }

        [Option('p', "AzureBatchPoolName", SetName = "AzureBatch")]
        public string AzureBatchPoolName { get; set; }

        [Option('c', "AzureBatchBrokerStorageConnectionString", SetName = "AzureBatch")]
        public string AzureBatchBrokerStorageConnectionString { get; set; }

        [Option('h', "HpcPackSchedulerAddress", SetName = "HpcPack")]
        public string HpcPackSchedulerAddress { get; set; }

        [Option("BrokerLauncherExePath")]
        public string BrokerLauncherExePath { get; set; }

        [Option("ServiceHostExePath", SetName = "Local")]
        public string ServiceHostExePath { get; set; }

        [Option("ServiceRegistrationPath", SetName = "Local")]
        public string ServiceRegistrationPath { get; set; }

        [Option("LocalBrokerStorageConnectionString", SetName = "Local")]
        public string LocalBrokerStorageConnectionString { get; set; }

        [Option('f', "JsonFilePath", SetName = "ConfigurationFile")]
        public string JsonFilePath { get; set; }

        [Option('d', HelpText = "Start as console application")]
        public bool AsConsole { get; set; }

        public bool ConfigureLogging { get; set; }

        public string Logging { get; set; }

        public bool? ConsoleLogging { get; set; }

        public string ConsoleLoggingLevel { get; set; }

        public bool? SeqLogging { get; set; }

        public string SeqLoggingLevel { get; set; }

        public string SeqServerUrl { get; set; }

        public bool? AzureAnalyticsLogging { get; set; }

        public string AzureAnalyticsLoggingLevel { get; set; }

        public string AzureAnalyticsWorkspaceId { get; set; }

        public string AzureAnalyticsAuthenticationId { get; set; }

        public bool? LocalFileLogging { get; set; }

        public string LocalFileLoggingLevel { get; set; }

        public string LocalFilePath { get; set; }

        public string LocalFileFormatter { get; set; }

        public string RollingInterval { get; set; }
    }
}