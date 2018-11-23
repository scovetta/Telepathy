//-----------------------------------------------------------------------------------
// <copyright file="RequestReplyFrontEndTest.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Unit test for request reply frontend</summary>
//-----------------------------------------------------------------------------------
namespace Microsoft.Hpc.SvcBroker.UnitTest
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using Microsoft.Hpc.ServiceBroker.FrontEnd;
    using Microsoft.Hpc.SvcBroker;
    using Microsoft.Hpc.SvcBroker.UnitTest.Mock;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This is a test class for RequestReplyFrontEndTest and is intended
    /// to contain all RequestReplyFrontEndTest Unit Tests
    /// </summary>
    [TestClass]
    public class RequestReplyFrontEndTest
    {
        /// <summary>
        /// Open FrontEnd and send messages
        /// </summary>
        [TestMethod]
        public void SendRequestWithoutSecurity()
        {
            string uri = "http://localhost:8888";

            // Open frontend
            Uri listenUri = new Uri(uri);
            Binding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            MockBrokerQueue queue = new MockBrokerQueue();
            queue.DirectReply = true;
            RequestReplyFrontEnd<IReplyChannel> target = new RequestReplyFrontEnd<IReplyChannel>(listenUri, binding, null, queue);
            target.Open();

            // Build channel to send request
            IChannelFactory<IRequestChannel> factory = binding.BuildChannelFactory<IRequestChannel>();
            factory.Open();
            IRequestChannel channel = factory.CreateChannel(new EndpointAddress(uri));
            channel.Open();

            Message request = Message.CreateMessage(MessageVersion.Soap11, "UnitTest", "Test");
            try
            {
                Message reply = channel.Request(request);
            }
            catch (FaultException fe)
            {
                Assert.AreEqual(fe.Code.Name, "DummyReply");
            }

            channel.Close();
            factory.Close();
            target.Close();
        }

        /// <summary>
        /// Send message with security
        /// </summary>
        //[TestMethod]
        public void SendMessageWithSecurity()
        {
            string uri = String.Format("https://{0}:8080", Environment.MachineName);

            // Open frontend
            Uri listenUri = new Uri(uri);
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportWithMessageCredential);
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            MockBrokerAuthorization auth = new MockBrokerAuthorization();
            MockBrokerQueue queue = new MockBrokerQueue();
            queue.DirectReply = true;
            RequestReplyFrontEnd<IReplyChannel> target = new RequestReplyFrontEnd<IReplyChannel>(listenUri, binding, auth, queue);
            target.Open();

            // Build channel to send request
            MockWCFClient client = new MockWCFClient(binding, new EndpointAddress(uri));
            client.ClientCredentials.UserName.UserName = "fareast\\wsdcta";
            client.ClientCredentials.UserName.Password = "Pa55word00)";

            auth.Allow = false;
            try
            {
                client.Calc(3);
            }
            catch (FaultException fe)
            {
                Assert.AreEqual(fe.Code.Name, "AuthenticationFailure");
            }

            // Send request which is allowed
            auth.Allow = true;
            try
            {
                client.Calc(4);
            }
            catch (FaultException fe)
            {
                Assert.AreEqual(fe.Code.Name, "DummyReply");
            }

            client.Close();
            target.Close();
        }
    }
}
