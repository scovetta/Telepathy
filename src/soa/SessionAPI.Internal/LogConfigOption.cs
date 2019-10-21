namespace Microsoft.Telepathy.Session.Internal
{
    using CommandLine;
    public class LogConfigOption
    {
        [Option('l', HelpText = "Set log configuration only")]
        public bool ConfigureLogging { get; set; }

        [Option("Logging", SetName = "Log")]
        public string Logging { get; set; }

        [Option("ConsoleLogging", SetName = "Log")]
        public bool ConsoleLogging { get; set; }

        [Option("ConsoleLoggingLevel", SetName = "Log")]
        public string ConsoleLoggingLevel { get; set; }

        [Option("SeqLogging", SetName = "Log")]
        public bool SeqLogging { get; set; }

        [Option("SeqLoggingLevel", SetName = "Log")]
        public string SeqLoggingLevel { get; set; }

        [Option("SeqServerUrl", SetName = "Log")]
        public string SeqServerUrl { get; set; }

        [Option("AzureAnalyticsLogging", SetName = "Log")]
        public bool AzureAnalyticsLogging { get; set; }

        [Option("AzureAnalyticsLoggingLevel", SetName = "Log")]
        public string AzureAnalyticsLoggingLevel { get; set; }

        [Option("AzureAnalyticsWorkspaceId", SetName = "Log")]
        public string AzureAnalyticsWorkspaceId { get; set; }

        [Option("AzureAnalyticsAuthenticationId", SetName = "Log")]
        public string AzureAnalyticsAuthenticationId { get; set; }
    }
}
