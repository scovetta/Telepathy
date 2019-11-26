// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;

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

        private static void RemoveLoggingConfig(string name)
        {
            string sinkName = name == "All" ? "serilog:write-to" : $"serilog:write-to:{name}";
            var targetNodes = appSettingNode.ChildNodes.Cast<XmlNode>().Where(item => item.Attributes["key"].Value.Contains(sinkName)).ToList();
            if (targetNodes.Count > 0)
            {
                foreach (XmlNode node in targetNodes)
                {
                    appSettingNode.RemoveChild(node);
                }
                doc.Save(ConfigPath);
            }           
        }
        private static void DisableLogging(string name)
        {
            RemoveLoggingConfig(name);          
        }

        private static void SetLoggingSource(string name)
        {
            SetLoggingConfigItem("serilog:enrich:with-property:Source", name);
        }

        private static void SetLoggingConfigItem(string itemKey, string itemValue)
        {
            if (string.IsNullOrEmpty(itemValue))
            {
                return;    
            }

            List<XmlNode> configNodes = appSettingNode.ChildNodes.Cast<XmlNode>().Where(item => item.Attributes["key"].Value.Contains(itemKey)).ToList();
           
            if (configNodes.Count == 0)
            {
                Trace.TraceInformation($"New item key is {itemKey} , value is " + itemValue);
                XmlElement configEle = doc.CreateElement("add");
                configEle.SetAttribute("key", itemKey);
                configEle.SetAttribute("value", itemValue);
                appSettingNode.InsertAfter(configEle, appSettingNode.LastChild);
            }
            else 
            {
                Trace.TraceInformation($"Update item key is {itemKey} , value is " + itemValue);
                configNodes[configNodes.Count-1].Attributes["value"].Value = itemValue;
            }
        }

        private static void SetBasicLoggingConfig(string sinkName, string sinkValue, string minimumLevel)
        {
            SetLoggingConfigItem($"serilog:using:{sinkName}", $"Serilog.Sinks.{sinkValue}");
            SetLoggingConfigItem($"serilog:write-to:{sinkName}.restrictedToMinimumLevel", minimumLevel);
        }

        private static void SetConsoleLoggingConfig(string minimumLevel)
        {
            //Enable console logging should has an item which key contains serilog:write-to:Console
            minimumLevel = string.IsNullOrEmpty(minimumLevel) ? "Information" : minimumLevel;
            SetBasicLoggingConfig("Console", "Console", minimumLevel);
        }

        private static void SetSeqLoggingConfig(string serverUrl, string minimumLevel)
        {
            SetBasicLoggingConfig("Seq", "Seq", minimumLevel);
            SetLoggingConfigItem("serilog:write-to:Seq.serverUrl", serverUrl);
        }

        private static void SetAzureAnalyticsLoggingConfig(string workspaceId, string authenticationId, string minimumLevel)
        {
            SetBasicLoggingConfig("AzureLogAnalytics", "AzureAnalytics", minimumLevel);
            SetLoggingConfigItem("serilog:write-to:AzureLogAnalytics.workspaceId", workspaceId);
            SetLoggingConfigItem("serilog:write-to:AzureLogAnalytics.authenticationId", authenticationId);
        }

        private static void SetFileLoggingConfig(string logFilePath, string minimumLevel, string rollingInterval, string formatter)
        {           
            SetBasicLoggingConfig("File", "File", minimumLevel);
            SetLoggingConfigItem("serilog:write-to:File.path", logFilePath);
            SetLoggingConfigItem("serilog:write-to:File.rollingInterval", rollingInterval);
            SetLoggingConfigItem("serilog:write-to:File.formatter", formatter);
        }

        public static void SetLoggingConfig(ILogConfigOption option, string logConfigFilePath, string source)
        {
            ConfigPath = logConfigFilePath;
            try
            {
                GetConfigDoc();
            }
            catch (Exception e)
            {
                Trace.TraceError("Exception occurs when get config file - " + e);
            }
            if (option.Logging.Equals("Disable"))
            {    
                Trace.TraceInformation("Logging is set as Disable.");
                DisableLogging("All");
            }
            else if (option.Logging.Equals("Enable"))
            {
                Trace.TraceInformation("Logging is set as Enable.");
                SetLoggingSource(source);
                if (option.ConsoleLogging.HasValue)
                {
                    if (option.ConsoleLogging.Value)
                    {
                        Trace.TraceInformation("Set console logging configuration.");
                        SetConsoleLoggingConfig(option.ConsoleLoggingLevel);
                    }
                    else
                    {
                        Trace.TraceInformation("Disable Console logging");
                        DisableLogging("Console");
                    }
                }

                if (option.SeqLogging.HasValue)
                {
                    if (option.SeqLogging.Value)
                    {
                        Trace.TraceInformation("Set Seq logging configuration.");
                        SetSeqLoggingConfig(option.SeqServerUrl, option.SeqLoggingLevel);
                    }
                    else
                    {
                        Trace.TraceInformation("Disable Seq logging");
                        DisableLogging("Seq");
                    }
                }

                if (option.AzureAnalyticsLogging.HasValue)
                {
                    if (option.AzureAnalyticsLogging.Value)
                    {
                        Trace.TraceInformation("Set AzureAnalytics logging configuration.");
                        SetAzureAnalyticsLoggingConfig(option.AzureAnalyticsWorkspaceId,
                            option.AzureAnalyticsAuthenticationId, option.AzureAnalyticsLoggingLevel);
                    }
                    else
                    {
                        Trace.TraceInformation("Disable AzureAnalytics logging");
                        DisableLogging("AzureLogAnalytics");
                    }
                }

                if (option.LocalFileLogging.HasValue)
                {
                    if (option.LocalFileLogging.Value)
                    {
                        SetFileLoggingConfig(option.LocalFilePath, option.LocalFileLoggingLevel,
                            option.RollingInterval, option.LocalFileFormatter);
                    }
                    else
                    {
                        Trace.TraceInformation("Disable file logging");
                        DisableLogging("File");
                    }
                }

                doc.Save(ConfigPath);
            }
            else
            {
                throw new Exception("Please set valid Logging value, \"Disable/Enable\"");
            }
        }   
    }
}
