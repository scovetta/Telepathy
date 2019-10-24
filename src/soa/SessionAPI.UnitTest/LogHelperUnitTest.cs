// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.UnitTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Telepathy.Session.Internal;
    using CommandLine;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Xml;

    [TestClass]
    public class LogHelperUnitTest
    {
        private class LogConfigOption : ILogConfigOption
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

            public string RollingInterval { get; set; }
        }

        private string tempXml = @"<?xml version='1.0' encoding='utf-8'?>
                <configuration>
                    <appSettings>
                        <add key = 'failoverClusterName' value='' />
                        <add key = 'serilog:using:File' value='Serilog.Sinks.File' />
                        <add key = 'serilog:write-to:File.path' value='C:\logs\test.txt' />
                        <add key = 'serilog:write-to:File.rollingInterval' value='Day' />
                        <add key = 'serilog:using:Seq' value='Serilog.Sinks.Seq' />
                        <add key = 'serilog:write-to:Seq.restrictedToMinimumLevel' value='Information' />
                        <add key = 'serilog:write-to:Seq.serverUrl' value='http://localhost:5341' />
                    </appSettings>
                </configuration>
        ";

        private static void WriteToXmlFile(string filePath, string contentToWrite, bool append = false)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(contentToWrite);
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        [TestMethod]
        public void DisableLoggingTest()
        {
            string testFilePath = "test-disable-all.config";
            try
            {
                //Write tempXml content to a new file named test.config 
                WriteToXmlFile(testFilePath, tempXml);
                LogConfigOption logConfigOption = new LogConfigOption();
                logConfigOption.Logging = "Disable";
                LogHelper.SetLoggingConfig(logConfigOption, testFilePath, "Test");
                XmlDocument doc = new XmlDocument();
                doc.Load(testFilePath);
                Console.WriteLine($"Current config file content is: ${doc.InnerXml}");

                var targetNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Contains("serilog:write-to")).ToList();

                Assert.AreEqual(0, targetNodes.Count);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                    Console.WriteLine($"{testFilePath} is deleted");
                }
            }
        }

        [TestMethod]
        public void SetConsoleLoggingTest()
        {
            string testFilePath = "test-console-logging.config";
            try
            {
                //Write tempXml content to a new file named test.config 
                WriteToXmlFile(testFilePath, tempXml);
                LogConfigOption logConfigOption = new LogConfigOption();
                logConfigOption.Logging = "Enable";
                logConfigOption.ConsoleLogging = true;
                logConfigOption.ConsoleLoggingLevel = "Verbose";
                LogHelper.SetLoggingConfig(logConfigOption, testFilePath, "Test");
                XmlDocument doc = new XmlDocument();
                doc.Load(testFilePath);
                Console.WriteLine($"Current config file content is: ${doc.InnerXml}");

                var usingConsoleNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Equals("serilog:using:Console")).ToList();
                var consoleLoggingLevelNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Equals("serilog:write-to:Console.restrictedToMinimumLevel")).ToList();

                Assert.AreEqual(1, usingConsoleNodes.Count);
                Assert.AreEqual("Serilog.Sinks.Console", usingConsoleNodes[0].Attributes["value"].Value);
                Assert.AreEqual(1, consoleLoggingLevelNodes.Count);
                Assert.AreEqual("Verbose", consoleLoggingLevelNodes[0].Attributes["value"].Value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                    Console.WriteLine($"{testFilePath} is deleted");
                }
            }
        }

        [TestMethod]
        public void SetAzureAnalyticsLoggingTest()
        {
            string testFilePath = "test-AzureAnalytics-logging.config";
            try
            {
                //Write tempXml content to a new file named test.config 
                WriteToXmlFile(testFilePath, tempXml);
                LogConfigOption logConfigOption = new LogConfigOption();
                logConfigOption.Logging = "Enable";
                logConfigOption.AzureAnalyticsLogging = true;
                logConfigOption.AzureAnalyticsWorkspaceId = "workspaceId";
                logConfigOption.AzureAnalyticsAuthenticationId = "authenticationId";
                logConfigOption.AzureAnalyticsLoggingLevel = "Verbose";
                LogHelper.SetLoggingConfig(logConfigOption, testFilePath, "Test");
                XmlDocument doc = new XmlDocument();
                doc.Load(testFilePath);
                Console.WriteLine($"Current config file content is: ${doc.InnerXml}");

                var usingAzureAnalyticsNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Equals("serilog:using:AzureLogAnalytics")).ToList();
                var loggingLevelNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Equals("serilog:write-to:AzureLogAnalytics.restrictedToMinimumLevel")).ToList();
                var workspaceIdNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Equals("serilog:write-to:AzureLogAnalytics.workspaceId")).ToList();
                var authenticationIdNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Equals("serilog:write-to:AzureLogAnalytics.authenticationId")).ToList();

                Assert.AreEqual(1, usingAzureAnalyticsNodes.Count);
                Assert.AreEqual("Serilog.Sinks.AzureAnalytics", usingAzureAnalyticsNodes[0].Attributes["value"].Value);
                Assert.AreEqual(1, loggingLevelNodes.Count);
                Assert.AreEqual("Verbose", loggingLevelNodes[0].Attributes["value"].Value);
                Assert.AreEqual(1, workspaceIdNodes.Count);
                Assert.AreEqual("workspaceId", workspaceIdNodes[0].Attributes["value"].Value);
                Assert.AreEqual(1, authenticationIdNodes.Count);
                Assert.AreEqual("authenticationId", authenticationIdNodes[0].Attributes["value"].Value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                    Console.WriteLine($"{testFilePath} is deleted");
                }
            }
        }

        [TestMethod]
        public void DisableFileLoggingTest()
        {
            string testFilePath = "test-disable-file-logging.config";
            try
            {
                //Write tempXml content to a new file named test.config 
                WriteToXmlFile(testFilePath, tempXml);
                XmlDocument doc = new XmlDocument();
                doc.Load(testFilePath);
                var existingNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Contains("serilog:write-to")).ToList();
                Assert.AreEqual(4, existingNodes.Count);
                LogConfigOption logConfigOption = new LogConfigOption();
                logConfigOption.ConfigureLogging = true;
                logConfigOption.Logging = "Enable";
                logConfigOption.LocalFileLogging = false;
                LogHelper.SetLoggingConfig(logConfigOption, testFilePath, "Test");
                doc.Load(testFilePath);
                Console.WriteLine($"Current config file content is: ${doc.InnerXml}");

                XmlNode usingFileNode =
                    doc.SelectSingleNode("//add[@key='serilog:using:File']");
                var targetNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Equals("serilog:write-to:File")).ToList();
                existingNodes = doc.GetElementsByTagName("add").Cast<XmlNode>()
                    .Where(item => item.Attributes["key"].Value.Contains("serilog:write-to")).ToList();

                Assert.AreEqual(0, targetNodes.Count);
                Assert.AreEqual(2, existingNodes.Count);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                    Console.WriteLine($"{testFilePath} is deleted");
                }
            }
        }
    }
}
