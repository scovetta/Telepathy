namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Mock
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;

    internal class MockBrokerQueueFactory : IBrokerQueueFactory
    {
        private int putResponseAsyncInvokedTimes;

        private ConcurrentDictionary<BrokerQueueItem, List<Message>> messageDic = new ConcurrentDictionary<BrokerQueueItem, List<Message>>();

        public int PutResponseAsyncInvokedTimes
        {
            get { return this.putResponseAsyncInvokedTimes; }
        }

        public ConcurrentDictionary<BrokerQueueItem, List<Message>> PutMessageDic
        {
            get { return this.messageDic; }
        }

        public async Task PutResponseAsync(Message responseMsg, BrokerQueueItem requestItem)
        {
            Interlocked.Increment(ref this.putResponseAsyncInvokedTimes);
            this.messageDic.AddOrUpdate(
                requestItem,
                x => new List<Message>() { responseMsg },
                (x, list) =>
                {
                    list.Add(responseMsg);
                    return list;
                });
        }
    }
}
