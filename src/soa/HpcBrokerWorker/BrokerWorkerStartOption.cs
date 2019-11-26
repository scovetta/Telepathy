// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerShim
{
    using CommandLine;
    using Microsoft.Telepathy.Session.Internal;
    internal class BrokerWorkerStartOption : ILogConfigOption
    {
        [Option('f', "JsonFilePath", SetName = "ConfigurationFile")]
        public string JsonFilePath { get; set; }

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
