// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using CommandLine;
    public interface ILogConfigOption
    {
        [Option('l', HelpText = "Set log configuration only")]
        bool ConfigureLogging { get; set; }

        [Option("Logging", HelpText = "Accept 'Enable' or 'Disable' to enable or disable logging feature")]
        string Logging { get; set; }

        [Option("ConsoleLogging", HelpText = "Set true to enable Console logging or false to disable Console logging")]
        bool? ConsoleLogging { get; set; }

        [Option("ConsoleLoggingLevel", HelpText = "The minimum logging level for console, accept 'Verbose'/'Debug'/'Information'/'Warning'/'Error'/'Fatal'")]
        string ConsoleLoggingLevel { get; set; }

        [Option("SeqLogging", HelpText = "Set true to enable Seq logging or false to disable Seq logging") ]
        bool? SeqLogging { get; set; }

        [Option("SeqLoggingLevel", HelpText = "The minimum logging level for Seq, accept 'Verbose'/'Debug'/'Information'/'Warning'/'Error'/'Fatal'")]
        string SeqLoggingLevel { get; set; }

        [Option("SeqServerUrl", HelpText = "The Seq Server Url")]
        string SeqServerUrl { get; set; }

        [Option("AzureAnalyticsLogging", HelpText = "Set true to enable AzureAnalytics logging or false to disable AzureAnalytics logging")]
        bool? AzureAnalyticsLogging { get; set; }

        [Option("AzureAnalyticsLoggingLevel", HelpText = "The minimum logging level for AzureAnalytics, accept 'Verbose'/'Debug'/'Information'/'Warning'/'Error'/'Fatal'")]
        string AzureAnalyticsLoggingLevel { get; set; }

        [Option("AzureAnalyticsWorkspaceId", HelpText = "The WorkSpaceId of AzureAnalytics")]
        string AzureAnalyticsWorkspaceId { get; set; }

        [Option("AzureAnalyticsAuthenticationId", HelpText = "The AuthenticationId of AzureAnalytics")]
        string AzureAnalyticsAuthenticationId { get; set; }

        [Option("LocalFileLogging", HelpText = "Set true to enable File logging or false to disable File logging")]
        bool? LocalFileLogging { get; set; }

        [Option("LocalFileLoggingLevel", HelpText = "The minimum logging level for File, accept 'Verbose'/'Debug'/'Information'/'Warning'/'Error'/'Fatal'")]
        string LocalFileLoggingLevel { get; set; }

        [Option("LocalFilePath", HelpText = "The file path for writing log")]
        string LocalFilePath { get; set; }

        [Option("LocalFileFormatter", HelpText = "The content format for File logging")]
        string LocalFileFormatter { get; set; }

        [Option("RollingInterval", HelpText = "The rolling interval for File logging")]
        string RollingInterval { get; set; }
    }
}
