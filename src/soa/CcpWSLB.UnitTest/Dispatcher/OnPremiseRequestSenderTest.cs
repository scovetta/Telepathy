using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Hpc.ServiceBroker.BackEnd;
using Microsoft.Hpc.ServiceBroker.BrokerStorage;
using Microsoft.Hpc.ServiceBroker.UnitTest.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Hpc.ServiceBroker.UnitTest.Dispatcher
{
    [TestClass]
    public class OnPremiseRequestSenderTest
    {
        private const string addressString = "net.tcp://localhost:9100/1/1/_defaultEndpoint";

        private EndpointAddress address = new EndpointAddress(addressString);

        private Binding binding = new NetTcpBinding();

        private int serviceOperationTimeout = 1000 * 100;
        private int serviceInitializationTimeout = 1000 * 60;
        private int initEndpointNotFoundWaitPeriod = 1000;

        [TestMethod]
        public void OnPremiseRequestSender_CreateClient()
        {
            MockDispatcher dispatcher = new MockDispatcher();

            OnPremiseRequestSender sender = new OnPremiseRequestSender(this.address, this.binding, this.serviceOperationTimeout, dispatcher, serviceInitializationTimeout, initEndpointNotFoundWaitPeriod);

            sender.CreateClientAsync(false, 0);

            ServiceClient client = Utility.GetServiceClient(sender);

            Assert.AreNotEqual(client, null, "ServiceClient.Client should not be null.");

            Assert.AreEqual(client.Endpoint.Address.ToString(), addressString, "ServiceClient.Client epr address should be the same as expected one.");
        }

        [TestMethod]
        public void OnPremiseRequestSender_ReleaseClient()
        {
            MockDispatcher dispatcher = new MockDispatcher();

            OnPremiseRequestSender sender = new OnPremiseRequestSender(this.address, this.binding, this.serviceOperationTimeout, dispatcher, serviceInitializationTimeout, initEndpointNotFoundWaitPeriod);

            sender.CreateClientAsync(false, 0);

            sender.ReleaseClient();

            // client is closed by async call, so wait for a while here.
            Thread.Sleep(1000);

            ServiceClient client = Utility.GetServiceClient(sender);

            Assert.AreNotEqual(client.State, CommunicationState.Created, "ServiceClient.Client should not be at created state.");
        }

        [TestMethod]
        public void OnPremiseRequestSender_RefreshClient()
        {
            MockDispatcher dispatcher = new MockDispatcher();

            OnPremiseRequestSender sender = new OnPremiseRequestSender(this.address, this.binding, this.serviceOperationTimeout, dispatcher, serviceInitializationTimeout, initEndpointNotFoundWaitPeriod);

            sender.CreateClientAsync(false, 0);

            ServiceClient oldClient = Utility.GetServiceClient(sender);

            sender.RefreshClientAsync(0, false, Guid.NewGuid());

            // client is closed by async call, so wait for a while here.
            Thread.Sleep(1000);

            ServiceClient newClient = Utility.GetServiceClient(sender);

            Assert.AreNotEqual(newClient, oldClient, "ServiceClient.Client should be changed.");

            Assert.AreNotEqual(oldClient.State, CommunicationState.Created, "Previous ServiceClient.Client should not be at created state.");

            Assert.AreEqual(newClient.State, CommunicationState.Opening, "New ServiceClient.Client should be at created state.");
        }

        [TestMethod]
        public void OnPremiseRequestSender_StartClient()
        {
            MockDispatcher dispatcher = new MockDispatcher();

            OnPremiseRequestSender sender = new OnPremiseRequestSender(this.address, this.binding, this.serviceOperationTimeout, dispatcher, serviceInitializationTimeout, initEndpointNotFoundWaitPeriod);

            sender.CreateClientAsync(false, 0);

            bool clientStart = false;

            GetNextRequestState state =
                new GetNextRequestState(
                    (index) =>
                    {
                        clientStart = true;
                    },
                    0);

            sender.StartClient(state);

            Assert.AreEqual(clientStart, true, "The local variable clientStart should be set when start client.");
        }
    }
}
