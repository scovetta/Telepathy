namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Dispatcher
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;
    using System.Xml;

    using Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.FrontEnd;
    using Microsoft.Telepathy.ServiceBroker.UnitTest.Mock;
    using Microsoft.Telepathy.Session;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResponseQueueAdapterTest
    {
        [TestMethod]
        public async Task PutResponseBackDummyPassTest()
        {
            var f = new MockBrokerQueueFactory();

            var ob = new MockBrokerObserver();

            var sampleMessage = Message.CreateMessage(MessageVersion.Default, "SampleAction");
            sampleMessage.Headers.MessageId = new UniqueId(Guid.NewGuid());
            var adapter = new ResponseQueueAdapter(ob, f, 4);
            var item = new BrokerQueueItem(DummyRequestContext.GetInstance(MessageVersion.Soap11), sampleMessage, Guid.NewGuid(), null);
            var message = Message.CreateMessage(MessageVersion.Default, "Default");

            DispatchData data = new DispatchData("1", 1, "1")
            {
                BrokerQueueItem = item,
                MessageId = Guid.NewGuid(),
                DispatchTime = new DateTime(2000, 1, 1),
                ReplyMessage = message,
            };

            await adapter.PutResponseBack(data);

            Assert.IsTrue(ob.Duration > 0, "The call duration should be greater than 0");
            Assert.AreEqual(f.PutMessageDic.Count, 1, "There must be 1 and only 1 instance");
            Assert.AreEqual(f.PutResponseAsyncInvokedTimes, 1, "There must be 1 and only 1 invoke");
            Assert.AreSame(f.PutMessageDic.First().Key, item, "The put back BrokerQueueItem should be the same as the original one.");
            Assert.AreEqual(f.PutMessageDic.First().Value.Count, 1, "The response message should only be one.");
            Assert.AreSame(f.PutMessageDic.First().Value[0], message, "The put back Message should be the same as the original one.");
            Assert.IsNull(data.BrokerQueueItem, "BrokerQueueItem property should be set to null after put back.");
            Assert.IsNull(data.ReplyMessage, "The reply message should be null after put back.");
            Assert.IsNull(data.Exception, "The Exception should be null after put back.");
        }

        [TestMethod]
        public async Task PutResponseBackDummyExceptionTest()
        {
            var f = new MockBrokerQueueFactory();

            var ob = new MockBrokerObserver();

            var sampleMessage = Message.CreateMessage(MessageVersion.Default, "SampleAction");
            sampleMessage.Headers.MessageId = new UniqueId(Guid.NewGuid());
            var adapter = new ResponseQueueAdapter(ob, f, 4);
            var item = new BrokerQueueItem(DummyRequestContext.GetInstance(MessageVersion.Soap11), sampleMessage, Guid.NewGuid(), null);

            DispatchData data = new DispatchData("1", 1, "1")
            {
                BrokerQueueItem = item,
                MessageId = Guid.NewGuid(),
                DispatchTime = new DateTime(2000, 1, 1),
                Exception = new FaultException<RetryOperationError>(new RetryOperationError("Reason")),
            };

            await adapter.PutResponseBack(data);

            Assert.IsTrue(ob.Duration > 0, "The call duration should be greater than 0");
            Assert.AreEqual(f.PutMessageDic.Count, 1, "There must be 1 and only 1 instance");
            Assert.AreEqual(f.PutResponseAsyncInvokedTimes, 1, "There must be 1 and only 1 invoke");
            Assert.AreSame(f.PutMessageDic.First().Key, item, "The put back BrokerQueueItem should be the same as the original one.");
            Assert.AreEqual(f.PutMessageDic.First().Value.Count, 1, "The response message should only be one.");
            Assert.AreSame(f.PutMessageDic.First().Value[0].Headers.RelatesTo, item.Message.Headers.MessageId, "The put back Message should be the same as the original one.");
            Assert.IsNull(data.BrokerQueueItem, "BrokerQueueItem property should be set to null after put back.");
            Assert.IsNull(data.ReplyMessage, "The reply message should be null after put back.");
            Assert.IsNull(data.Exception, "The Exception should be null after put back.");
        }

        [TestMethod]
        public async Task PutResponseBackPassTest()
        {
            var f = new MockBrokerQueueFactory();

            var ob = new MockBrokerObserver();

            var sampleMessage = Message.CreateMessage(MessageVersion.Default, "SampleAction");
            sampleMessage.Headers.MessageId = new UniqueId(Guid.NewGuid());
            var mockDuplexRequestContext = new MockDuplexRequestContext(sampleMessage);
            var adapter = new ResponseQueueAdapter(ob, f, 4);
            var item = new BrokerQueueItem(mockDuplexRequestContext, sampleMessage, Guid.NewGuid(), null);
            var message = Message.CreateMessage(MessageVersion.Default, "Default");

            DispatchData data = new DispatchData("1", 1, "1")
            {
                BrokerQueueItem = item,
                MessageId = Guid.NewGuid(),
                DispatchTime = new DateTime(2000, 1, 1),
                ReplyMessage = message,
            };

            await adapter.PutResponseBack(data);

            Assert.IsTrue(ob.Duration > 0, "The call duration should be greater than 0");
            Assert.AreSame(mockDuplexRequestContext.ReplyMessage, message, "The put back Message should be the same as the original one.");
            Assert.IsNull(data.BrokerQueueItem, "BrokerQueueItem property should be set to null after put back.");
            Assert.IsNull(data.ReplyMessage, "The reply message should be null after put back.");
            Assert.IsNull(data.Exception, "The Exception should be null after put back.");
        }

        [TestMethod]
        public async Task PutResponseBackExceptionTest()
        {
            var f = new MockBrokerQueueFactory();

            var ob = new MockBrokerObserver();

            var sampleMessage = Message.CreateMessage(MessageVersion.Default, "SampleAction");
            sampleMessage.Headers.MessageId = new UniqueId(Guid.NewGuid());
            UniqueId uniqueId = sampleMessage.Headers.MessageId;

            var mockDuplexRequestContext = new MockDuplexRequestContext(sampleMessage);

            var adapter = new ResponseQueueAdapter(ob, f, 4);
            var item = new BrokerQueueItem(mockDuplexRequestContext, sampleMessage, Guid.NewGuid(), null);

            DispatchData data = new DispatchData("1", 1, "1")
            {
                BrokerQueueItem = item,
                MessageId = Guid.NewGuid(),
                DispatchTime = new DateTime(2000, 1, 1),
                Exception = new FaultException<RetryOperationError>(new RetryOperationError("Reason")),
            };

            await adapter.PutResponseBack(data);

            Assert.IsTrue(ob.Duration > 0, "The call duration should be greater than 0");
            Assert.AreSame(mockDuplexRequestContext.ReplyMessage.Headers.RelatesTo, uniqueId, "The put back Message should be the same as the original one.");
            Assert.IsNull(data.BrokerQueueItem, "BrokerQueueItem property should be set to null after put back.");
            Assert.IsNull(data.ReplyMessage, "The reply message should be null after put back.");
            Assert.IsNull(data.Exception, "The Exception should be null after put back.");
        }
    }
}
