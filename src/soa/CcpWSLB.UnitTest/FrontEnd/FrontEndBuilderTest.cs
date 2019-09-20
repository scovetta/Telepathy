// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.UnitTest.FrontEnd
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.FrontEnd;
    using Microsoft.Telepathy.ServiceBroker.UnitTest.Mock;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit Test for FrontEndBuilder
    /// </summary>
    [TestClass]
    public class FrontEndBuilderTest
    {
        /// <summary>
        /// Store the test context instance
        /// </summary>
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return this.testContextInstance;
            }
            set
            {
                this.testContextInstance = value;
            }
        }

        /// <summary>
        /// Unit test to create a controller front end
        /// </summary>
        [TestMethod]
        public void BuildFrontEndTest_NetTcp()
        {
            SessionStartInfoContract info = new SessionStartInfoContract();
            info.ServiceName = "CcpEchoSvc";
            BrokerStartInfo startInfo = new BrokerStartInfo();

            string filename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = filename;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            BrokerConfigurations brokerConfig = BrokerConfigurations.GetSectionGroup(config);

            startInfo.Durable = true;
            startInfo.SessionId = "100";
            startInfo.ConfigurationFile = "CcpEchoSvc.config";
            ConfigurationHelper.LoadConfiguration(info, startInfo, out brokerConfig, out var serviceConfig, out var bindings);

            var sharedData = new SharedData(startInfo, info, brokerConfig, serviceConfig);

            var result = FrontEndBuilder.BuildFrontEnd(sharedData, new BrokerObserver(sharedData, new ClientInfo[0]), null, null, bindings, null);
            Assert.IsTrue(Regex.IsMatch(result.ControllerUriList.FirstOrDefault(), @"net\.tcp://.+:9091/100/NetTcp/Controller"));
            Assert.IsTrue(Regex.IsMatch(result.GetResponseUriList.FirstOrDefault(), @"net\.tcp://.+:9091/100/NetTcp/GetResponse"));
            Assert.IsTrue(Regex.IsMatch(result.FrontendUriList.FirstOrDefault(), @"net\.tcp://.+:9091/100/NetTcp"));
        }

        
        /// <summary>
        /// Unit test to create a controller front end
        /// </summary>
        [TestMethod]
        public void BuildFrontEndTest_Http()
        {
            SessionStartInfoContract info = new SessionStartInfoContract();
            info.ServiceName = "CcpEchoSvc";
            info.TransportScheme = TransportScheme.Http;
            info.Secure = true;
            BrokerStartInfo startInfo = new BrokerStartInfo();
            MockBrokerAuthorization auth = new MockBrokerAuthorization();

            string filename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = filename;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            BrokerConfigurations brokerConfig = BrokerConfigurations.GetSectionGroup(config);

            startInfo.Durable = false;
            startInfo.SessionId = "100";
            startInfo.ConfigurationFile = "CcpEchoSvc.config";


            ConfigurationHelper.LoadConfiguration(info, startInfo, out brokerConfig, out var serviceConfig, out var bindings);


            var sharedData = new SharedData(startInfo, info, brokerConfig, serviceConfig);
            var result = FrontEndBuilder.BuildFrontEnd(sharedData, new BrokerObserver(sharedData, new ClientInfo[0]), null, null, bindings, null);


            Assert.IsTrue(Regex.IsMatch(result.ControllerUriList[1], @"https://.+/100/Http/Controller"));
            Assert.AreEqual(null, result.GetResponseUriList.FirstOrDefault());
            Assert.IsTrue(Regex.IsMatch(result.FrontendUriList[1], @"https://.+/100/Http"));
        }

        
        /// <summary>
        /// Unit test to create a controller front end
        /// </summary>
        [TestMethod]
        public void BuildFrontEndTest_Http_Unsecure()
        {
            SessionStartInfoContract info = new SessionStartInfoContract();
            info.ServiceName = "CcpEchoSvc";
            info.TransportScheme = TransportScheme.Http;
            info.Secure = false;
            BrokerStartInfo startInfo = new BrokerStartInfo();

            string filename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = filename;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            BrokerConfigurations brokerConfig = BrokerConfigurations.GetSectionGroup(config);

            startInfo.Durable = false;
            startInfo.SessionId = "100";
            startInfo.ConfigurationFile = "CcpEchoSvc.config";

            ConfigurationHelper.LoadConfiguration(info, startInfo, out brokerConfig, out var serviceConfig, out var bindings);

            var sharedData = new SharedData(startInfo, info, brokerConfig, serviceConfig);

            var result = FrontEndBuilder.BuildFrontEnd(sharedData, new BrokerObserver(sharedData, new ClientInfo[0]), null, null, bindings, null);

            Assert.IsTrue(Regex.IsMatch(result.ControllerUriList[1], @"http://.+/100/Http/Controller"));
            Assert.AreEqual(null, result.GetResponseUriList[1]);
            Assert.IsTrue(Regex.IsMatch(result.FrontendUriList[1], @"http://.+/100/Http"));

        }

    }
}
