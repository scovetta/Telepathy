// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using CommandLine;
    public interface ILogConfigOption
    {
        [Option('l', HelpText = "Set log configuration only")]
        bool ConfigureLogging { get; set; }

        [Option("Logging")]
        string Logging { get; set; }

        [Option("ConsoleLogging")]
        bool? ConsoleLogging { get; set; }

        [Option("ConsoleLoggingLevel")]
        string ConsoleLoggingLevel { get; set; }

        [Option("SeqLogging")]
        bool? SeqLogging { get; set; }

        [Option("SeqLoggingLevel")]
        string SeqLoggingLevel { get; set; }

        [Option("SeqServerUrl")]
        string SeqServerUrl { get; set; }

        [Option("AzureAnalyticsLogging")]
        bool? AzureAnalyticsLogging { get; set; }

        [Option("AzureAnalyticsLoggingLevel")]
        string AzureAnalyticsLoggingLevel { get; set; }

        [Option("AzureAnalyticsWorkspaceId")]
        string AzureAnalyticsWorkspaceId { get; set; }

        [Option("AzureAnalyticsAuthenticationId")]
        string AzureAnalyticsAuthenticationId { get; set; }

        [Option("LocalFileLogging")]
        bool? LocalFileLogging { get; set; }

        [Option("LocalFileLoggingLevel")]
        string LocalFileLoggingLevel { get; set; }

        [Option("LocalFilePath")]
        string LocalFilePath { get; set; }

        [Option("RollingInterval")]
        string RollingInterval { get; set; }
    }
}
