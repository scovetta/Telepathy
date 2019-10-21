// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{

    using Microsoft.Telepathy.Session.Common;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Utility class providing diag trace enabled/disabled flag.
    /// </summary>
    public static class LogHelper
    {
        private static XmlDocument doc;

        private static XmlNode appSettingNode;
        private static string ConfigPath { set; get; }
        private static XmlDocument GetConfigDoc()
        {
            doc = new XmlDocument();
            doc.Load(ConfigPath);
            appSettingNode = doc.GetElementsByTagName("appSettings")[0];
            return doc;
        }

        private static void RemoveLoggingConfig()
        { 
            var targetNodes = appSettingNode.ChildNodes.Cast<XmlNode>().Where(item => item.Attributes["key"].Value.Contains("serilog:")).ToList();
            if (targetNodes.Count > 0)
            {
                foreach (XmlNode node in targetNodes)
                {
                    appSettingNode.RemoveChild(node);
                }
                doc.Save(ConfigPath);
            }           
        }
        private static void DisableLogging()
        {
            RemoveLoggingConfig();          
        }

        private static void SetLoggingSource(string name)
        {
            XmlElement logSource = doc.CreateElement("add");
            logSource.SetAttribute("key", "serilog:enrich:with-property:Source");
            logSource.SetAttribute("value", $"{name}");
            appSettingNode.InsertAfter(logSource, appSettingNode.LastChild);
        }

        private static void SetBasicLoggingConfig(string sinkName, string sinkValue, string minimumLevel = "Information")
        {
            XmlElement logSink = doc.CreateElement("add");
            logSink.SetAttribute("key", $"serilog:using:{sinkName}") ;
            logSink.SetAttribute("value", $"Serilog.Sinks.{sinkValue}");
            appSettingNode.InsertAfter(logSink, appSettingNode.LastChild);
            XmlElement logLevel = doc.CreateElement("add");
            logLevel.SetAttribute("key", $"serilog:write-to:{sinkName}.restrictedToMinimumLevel");
            logLevel.SetAttribute("value", $"{minimumLevel}");
            appSettingNode.InsertAfter(logLevel, appSettingNode.LastChild);
        }

        private static void SetConsoleLoggingConfig(string minimumLevel = "Information")
        {
            SetBasicLoggingConfig("Console", "Console", minimumLevel);
        }

        private static void SetSeqLoggingConfig(string serverUrl, string minimumLevel = "Information")
        {
            SetBasicLoggingConfig("Seq", "Seq", minimumLevel);
            XmlElement seqServerUrl = doc.CreateElement("add");
            seqServerUrl.SetAttribute("key", "serilog:write-to:Seq.serverUrl");
            seqServerUrl.SetAttribute("value", $"{serverUrl}");
            appSettingNode.InsertAfter(seqServerUrl, appSettingNode.LastChild);
        }

        private static void SetAuzreAnalyticsLoggingConfig(string worksapceId, string authenticationId, string minimumLevel = "Information")
        {
            SetBasicLoggingConfig("AzureLogAnalytics", "AzureAnalytics", minimumLevel);
            XmlElement analyticsWorkspaceId = doc.CreateElement("add");
            analyticsWorkspaceId.SetAttribute("key", "serilog:write-to:AzureLogAnalytics.workspaceId");
            analyticsWorkspaceId.SetAttribute("value", $"{worksapceId}");
            appSettingNode.InsertAfter(analyticsWorkspaceId, appSettingNode.LastChild);
            XmlElement analyticsAuthenticationId = doc.CreateElement("add");
            analyticsAuthenticationId.SetAttribute("key", "serilog:write-to:AzureLogAnalytics.authenticationId");
            analyticsAuthenticationId.SetAttribute("value", $"{authenticationId}");
            appSettingNode.InsertAfter(analyticsAuthenticationId, appSettingNode.LastChild);
        }

        public static void SetLoggingConfig(LogConfigOption option, string logConfigFilePath, string source)
        {
            ConfigPath = logConfigFilePath;
            GetConfigDoc();
            if (option.Logging.Equals("Disable"))
            {            
                DisableLogging();
            }
            else if (option.Logging.Equals("Enable"))
            {
                RemoveLoggingConfig();
                SetLoggingSource(source);
                if (option.ConsoleLogging)
                {
                    if (!string.IsNullOrEmpty(option.ConsoleLoggingLevel))
                        SetConsoleLoggingConfig(option.ConsoleLoggingLevel);
                }
                if (option.SeqLogging)
                {
                    if (!string.IsNullOrEmpty(option.SeqServerUrl))
                    {
                        if (!string.IsNullOrEmpty(option.SeqLoggingLevel))
                        {
                            SetSeqLoggingConfig(option.SeqServerUrl, option.SeqLoggingLevel);
                        }
                        else
                        {
                            SetSeqLoggingConfig(option.SeqServerUrl);
                        }
                    }
                }
                if (option.AzureAnalyticsLogging)
                {
                    if (!string.IsNullOrEmpty(option.AzureAnalyticsWorkspaceId) && !string.IsNullOrEmpty(option.AzureAnalyticsAuthenticationId))
                    {
                        if (!string.IsNullOrEmpty(option.AzureAnalyticsLoggingLevel))
                        {
                            SetAuzreAnalyticsLoggingConfig(option.AzureAnalyticsWorkspaceId, option.AzureAnalyticsAuthenticationId, option.AzureAnalyticsLoggingLevel);
                        }
                        else
                        {
                            SetAuzreAnalyticsLoggingConfig(option.AzureAnalyticsWorkspaceId, option.AzureAnalyticsAuthenticationId);
                        }
                    }
                }

                doc.Save(ConfigPath);
            }
        }

     
    }
}
