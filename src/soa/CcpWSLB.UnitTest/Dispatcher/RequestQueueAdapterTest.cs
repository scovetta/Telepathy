using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;

using Microsoft.Hpc.ServiceBroker.UnitTest.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Hpc.ServiceBroker.UnitTest.Dispatcher
{
    using Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;

    [TestClass]
    public class RequestQueueAdapterTest
    {
        private static readonly Message SampleMessage = Message.CreateMessage(MessageVersion.Default, "SampleAction");

        [TestMethod]
        public void PutRequestTest()
        {
            var f = new MockBrokerQueueFactory();
            var ob = new MockBrokerObserver();
            var adapter = new RequestQueueAdapter(ob, f);
            var item = new BrokerQueueItem(SampleMessage, Guid.NewGuid(), null);

            DispatchData data = new DispatchData("1", 1, "1")
            {
                BrokerQueueItem = item,
                MessageId = Guid.NewGuid()
            };

            adapter.PutRequestBack(data);

            Assert.AreEqual(1, ob.RequestProcessingCompletedInvokedTimes, "RequestProcessingCompleted of BrokerObserver should be invoked once.");
            Assert.IsNull(data.BrokerQueueItem, "BrokerQueueItem property should be set to null after put back.");
            Assert.AreEqual(1, f.PutResponseAsyncInvokedTimes, "PutResponseAsync should be invoked once.");
            var messageList = f.PutMessageDic.ToList();
            Assert.AreEqual(1, messageList.Count, "There should be only one message put.");
            Assert.AreEqual(item, messageList[0].Key, "BrokerQueueItem should match.");
            Assert.AreEqual(1, messageList[0].Value.Count, "There should be only one response for the request queue item.");
            Assert.AreEqual(null, messageList[0].Value[0], "Response message should be null.");
        }
    }
}
