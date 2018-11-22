//------------------------------------------------------------------------------
// <copyright file="FrontEndBuilderTest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Unit Test for FrontEndBuilder
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.SvcBroker.UnitTest.FrontEnd
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Hpc.ServiceBroker.FrontEnd;
    using Microsoft.Hpc.Scheduler.Session;
    using System.ServiceModel;
    using System.IO;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Hpc.SvcBroker.UnitTest.Mock;
    using System.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Configuration;

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
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        /// Unit test to create a controller front end
        /// </summary>
        [TestMethod]
        public void BuildFrontEndTest_NetTcp()
        {
            SessionStartInfo info = new SessionStartInfo("localhost", "CcpEchoSvc");
            //info.TransportScheme = TransportScheme.WsHttp;
            BrokerStartInfo startInfo = new BrokerStartInfo();
            MockBrokerAuthorization auth = new MockBrokerAuthorization();

            string filename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = filename;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            BrokerConfigurations brokerConfig = BrokerConfigurations.GetSectionGroup(config);

            startInfo.Durable = true;
            startInfo.BrokerPort = 9999;
            startInfo.ControllerPort = 10000;
            startInfo.SessionId = 100;
            ServiceHost host;
            string controllerEpr, getResponseEpr;
            FrontEndBase frontEnd;
            FrontEndBuilder.BuildFrontEnd(info, startInfo, auth, brokerConfig, out frontEnd, out host, out controllerEpr, out getResponseEpr);
            Assert.AreEqual("net.tcp://yanh-notebook:10000/Broker/Controller", controllerEpr);
            Assert.AreEqual("net.tcp://yanh-notebook:10000/Broker/GetResponse", getResponseEpr);
            Assert.AreEqual("net.tcp://yanh-notebook:9999/Broker", frontEnd.ListenUri);
        }

        /// <summary>
        /// Unit test to create a controller front end
        /// </summary>
        [TestMethod]
        public void BuildFrontEndTest_Http()
        {
            SessionStartInfo info = new SessionStartInfo("localhost", "CcpEchoSvc");
            info.TransportScheme = TransportScheme.Http;
            BrokerStartInfo startInfo = new BrokerStartInfo();
            MockBrokerAuthorization auth = new MockBrokerAuthorization();

            string filename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = filename;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            BrokerConfigurations brokerConfig = BrokerConfigurations.GetSectionGroup(config);

            startInfo.Durable = false;
            startInfo.BrokerPort = 9999;
            startInfo.ControllerPort = 10000;
            startInfo.SessionId = 100;
            ServiceHost host;
            string controllerEpr, getResponseEpr;
            FrontEndBase frontEnd;
            FrontEndBuilder.BuildFrontEnd(info, startInfo, auth, brokerConfig, out frontEnd, out host, out controllerEpr, out getResponseEpr);
            Assert.AreEqual(null, controllerEpr);
            Assert.AreEqual(null, getResponseEpr);
            Assert.AreEqual("https://yanh-notebook:9999/Broker", frontEnd.ListenUri);
        }

        /// <summary>
        /// Unit test to create a controller front end
        /// </summary>
        [TestMethod]
        public void BuildFrontEndTest_Http_Unsecure()
        {
            SessionStartInfo info = new SessionStartInfo("localhost", "CcpEchoSvc");
            info.TransportScheme = TransportScheme.Http;
            info.Secure = false;
            BrokerStartInfo startInfo = new BrokerStartInfo();
            MockBrokerAuthorization auth = new MockBrokerAuthorization();

            string filename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = filename;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            BrokerConfigurations brokerConfig = BrokerConfigurations.GetSectionGroup(config);

            startInfo.Durable = false;
            startInfo.BrokerPort = 9999;
            startInfo.ControllerPort = 10000;
            startInfo.SessionId = 100;
            ServiceHost host;
            string controllerEpr, getResponseEpr;
            FrontEndBase frontEnd;
            FrontEndBuilder.BuildFrontEnd(info, startInfo, auth, brokerConfig, out frontEnd, out host, out controllerEpr, out getResponseEpr);
            Assert.AreEqual(null, controllerEpr);
            Assert.AreEqual(null, getResponseEpr);
            Assert.AreEqual("http://yanh-notebook:9999/Broker", frontEnd.ListenUri);
        }
    }
}
