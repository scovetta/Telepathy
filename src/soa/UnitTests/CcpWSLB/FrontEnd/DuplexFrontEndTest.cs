//-----------------------------------------------------------------------------------
// <copyright file="DuplexFrontEndTest.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Unit test for duplex frontend</summary>
//-----------------------------------------------------------------------------------
namespace Microsoft.Hpc.SvcBroker.UnitTest.FrontEnd
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using Microsoft.Hpc.SvcBroker.UnitTest.Mock;
    using Microsoft.Hpc.ServiceBroker.FrontEnd;

    /// <summary>
    /// Unit test for duplex frontend
    /// </summary>
    [TestClass]
    public class DuplexFrontEndTest
    {
        /// <summary>
        /// Unit test to send a request using NetTcp
        /// </summary>
        [TestMethod]
        public void SendRequestTest()
        {
            string uri = "net.tcp://localhost:8889";

            // Open frontend
            Uri listenUri = new Uri(uri);
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            MockBrokerQueue queue = new MockBrokerQueue();
            queue.DirectReply = true;
            DuplexFrontEnd target = new DuplexFrontEnd(listenUri, binding, null, queue, null, null );
            target.Open();

            MockWCFClient client = new MockWCFClient(binding, new EndpointAddress(uri));

            try
            {
                client.Calc(4);
            }
            catch (FaultException fe)
            {
                Assert.AreEqual(fe.Code.SubCode.Name, "DummyReply");
            }

            target.Close();
        }

        /// <summary>
        /// Unit test to send a request using NetTcp with security
        /// </summary>
        [TestMethod]
        public void SendRequestTestWithSecurity()
        {
            string uri = "net.tcp://localhost:8889";

            // Open frontend
            Uri listenUri = new Uri(uri);
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);
            MockBrokerQueue queue = new MockBrokerQueue();
            MockBrokerAuthorization auth = new MockBrokerAuthorization();
            queue.DirectReply = true;
            DuplexFrontEnd target = new DuplexFrontEnd(listenUri, binding, auth, queue);
            target.Open();

            MockWCFClient client = new MockWCFClient(binding, new EndpointAddress(uri));

            auth.Allow = true;
            try
            {
                client.Calc(4);
            }
            catch (FaultException fe)
            {
                Assert.AreEqual(fe.Code.SubCode.Name, "DummyReply");
            }

            auth.Allow = false;
            try
            {
                client.Calc(4);
            }
            catch (FaultException fe)
            {
                Assert.AreEqual(fe.Code.SubCode.Name, "AuthenticationFailure");
            }

            target.Close();
        }
    }
}
