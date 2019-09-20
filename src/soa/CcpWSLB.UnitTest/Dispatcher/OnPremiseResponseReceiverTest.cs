namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Dispatcher
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.UnitTest.Mock;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OnPremiseResponseReceiverTest
    {
        [TestMethod]
        public void OnPremiseResponseReceiver_ReceiveResponse_Exception()
        {
            MockDispatcher dispatcher = new MockDispatcher();

            AzureResponseReceiver receiver = new AzureResponseReceiver(dispatcher);

            Message emptyMessage = Message.CreateMessage(MessageVersion.Default, string.Empty);

            DispatchData data = new DispatchData("1", 0, "1")
            {
                BrokerQueueItem = new BrokerQueueItem(null, emptyMessage, null),

                Client = new MockClient()
                {
                    Action = delegate()
                    {
                        throw new EndpointNotFoundException();
                    }
                }
            };

            receiver.ReceiveResponse(data);

            Assert.AreEqual(
                data.Exception.GetType(),
                typeof(EndpointNotFoundException),
                "EndpointNotFoundException is expected to happen in ReceiveResponse method.");
        }

        [TestMethod]
        public void OnPremiseResponseReceiver_ReceiveResponse_FaultMessage()
        {
            MockDispatcher dispatcher = new MockDispatcher();

            AzureResponseReceiver receiver = new AzureResponseReceiver(dispatcher);

            Message emptyMessage = Message.CreateMessage(MessageVersion.Default, string.Empty);

            MessageFault fault = MessageFault.CreateFault(FaultCode.CreateReceiverFaultCode("Error", Constant.HpcHeaderNS), string.Empty);

            DispatchData data = new DispatchData("1", 0, "1")
            {
                BrokerQueueItem = new BrokerQueueItem(null, emptyMessage, null),

                Client = new MockClient()
                {

                    Response = Message.CreateMessage(MessageVersion.Default, fault, string.Empty)
                }
            };

            receiver.ReceiveResponse(data);

            Assert.AreEqual(data.Exception, null, "Exception is not expected to happen in ReceiveResponse method.");

            Assert.AreEqual(data.ReplyMessage.IsFault, true, "Expected to receive fault message ReceiveResponse method.");
        }

        [TestMethod]
        public void OnPremiseResponseReceiver_ReceiveResponse_PreemptionMessage()
        {
            MockDispatcher dispatcher = new MockDispatcher();

            AzureResponseReceiver receiver = new AzureResponseReceiver(dispatcher);

            Message emptyMessage = Message.CreateMessage(MessageVersion.Default, string.Empty);

            Message preemption = Message.CreateMessage(MessageVersion.Default, Constant.HpcHeaderNS);

            preemption.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderPreemption, Constant.HpcHeaderNS, 1));

            DispatchData data = new DispatchData("1", 0, "1")
            {
                BrokerQueueItem = new BrokerQueueItem(null, emptyMessage, null),

                Client = new MockClient()
                {
                    Response = preemption
                }
            };

            receiver.ReceiveResponse(data);

            Assert.AreEqual(data.Exception, null, "Exception is not expected to happen in ReceiveResponse method.");

            Assert.AreEqual(data.ReplyMessage.IsFault, false, "Expected to receive non-fault message ReceiveResponse method.");

            Assert.AreEqual(data.ServicePreempted, true, "Expected to get ServicePreemption notice.");
        }
    }
}
