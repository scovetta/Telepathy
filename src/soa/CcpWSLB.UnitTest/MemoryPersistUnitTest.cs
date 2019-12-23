// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.UnitTest
{
    using System;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Persistences;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MemoryPersistUnitTest
    {
        private static readonly string action = "This is a dummy action";

        private static readonly byte[] largeMsg = new byte[640000];

        private static readonly int millisecondsDelay = 1000;

        private static readonly string sessionId = "1";

        private static readonly string shortMsg = "This is short message!";

        private static readonly string wrongMsg = "This is wrong message!";

        private static readonly string username = "Any";

        private static string clientId;

        private bool CallbackIsCalled;

        private bool IsExpected;

        private MemoryPersist sessionPersist;

        [TestMethod]
        public async Task GetLargeRequestTest()
        {
            var request = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, largeMsg),
                null);
            await this.sessionPersist.PutRequestAsync(request, null, 0);
            this.sessionPersist.GetRequestAsync(this.GetLargeMessageTestCallback, null);
            while (!this.CallbackIsCalled)
            {
                await Task.Delay(millisecondsDelay);
            }

            Assert.AreEqual(true, this.IsExpected);
        }

        [TestMethod]
        public async Task GetLargeResponseTest()
        {
            var response = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, largeMsg),
                null);
            response.PersistAsyncToken.AsyncToken = Guid.NewGuid().ToString();
            response.PeerItem = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, largeMsg),
                null);
            await this.sessionPersist.PutResponseAsync(response, null, 0);
            this.sessionPersist.GetResponseAsync(this.GetLargeMessageTestCallback, null);
            while (!this.CallbackIsCalled)
            {
                await Task.Delay(millisecondsDelay);
            }

            Assert.AreEqual(true, this.IsExpected);
        }

        [TestMethod]
        public async Task GetRequestTest()
        {
            var request = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, shortMsg),
                null);
            await this.sessionPersist.PutRequestAsync(request, null, 0);
            this.sessionPersist.GetRequestAsync(this.GetMessageTestCallback, null);
            while (!this.CallbackIsCalled)
            {
                await Task.Delay(millisecondsDelay);
            }

            Assert.AreEqual(true, this.IsExpected);
        }

        [TestMethod]
        public async Task GetWrongRequestTest()
        {
            var request = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, shortMsg),
                null);
            await this.sessionPersist.PutRequestAsync(request, null, 0);
            this.sessionPersist.GetRequestAsync(this.GetWrongMessageTestCallback, null);
            while (!this.CallbackIsCalled)
            {
                await Task.Delay(millisecondsDelay);
            }

            Assert.AreEqual(false, this.IsExpected);
        }

        [TestMethod]
        public async Task GetResponseTest()
        {
            var response = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, shortMsg),
                null);
            response.PersistAsyncToken.AsyncToken = Guid.NewGuid().ToString();
            response.PeerItem = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, shortMsg),
                null);
            await this.sessionPersist.PutResponseAsync(response, null, 0);
            this.sessionPersist.GetResponseAsync(this.GetMessageTestCallback, null);
            while (!this.CallbackIsCalled)
            {
                await Task.Delay(millisecondsDelay);
            }

            Assert.AreEqual(true, this.IsExpected);
        }

        [TestInitialize]
        public void TestInit()
        {
            clientId = Guid.NewGuid().ToString();
            this.sessionPersist = new MemoryPersist(username, sessionId, clientId);
            this.CallbackIsCalled = false;
            this.IsExpected = false;
        }

        private void GetLargeMessageTestCallback(BrokerQueueItem persistMessage, object state, Exception exception)
        {
            this.IsExpected = persistMessage.Message.GetBody<byte[]>().SequenceEqual(largeMsg);
            this.CallbackIsCalled = true;
        }

        private void GetMessageTestCallback(BrokerQueueItem persistMessage, object state, Exception exception)
        {
            this.IsExpected = persistMessage.Message.GetBody<string>().Equals(shortMsg);
            this.CallbackIsCalled = true;
        }

        private void GetWrongMessageTestCallback(BrokerQueueItem persistMessage, object state, Exception exception)
        {
            this.IsExpected = persistMessage.Message.GetBody<string>().Equals(wrongMsg);
            this.CallbackIsCalled = true;
        }
    }
}