// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.UnitTest
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;

    using Microsoft.Azure;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Persistences.AzureQueuePersist;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;

    [TestClass]
    public class AzureQueuePersistUnitTest
    {
        private static readonly string action = "This is a dummy action";

        private static readonly IFormatter binFormatterField = new BinaryFormatter();

        private static readonly byte[] largeMsg = new byte[640000];

        private static readonly int millisecondsDelay = 1000;

        private static readonly string PendingPathPrefix = "Pending";

        private static readonly string QueueNameFieldDelimeter = "-";

        private static readonly string QueuePathPrefix = "HPC";

        private static readonly string RequestQueueSuffix = "REQUESTS";

        private static readonly string ResponseQueueSuffix = "RESPONSES";

        private static readonly string sessionId = "1";

        private static readonly string shortMsg = "This is short message!";

        private static readonly string wrongMsg = "This is wrong message!";

        private static readonly string storageConnectString =
            CloudConfigurationManager.GetSetting("StorageConnectionString");

        private static readonly string username = "Any";

        private static string clientId;

        private CloudBlobContainer blobContainer;

        private bool CallbackIsCalled;

        private bool IsExpected;

        private CloudQueue pendingQueue;

        private CloudQueue requestQueue;

        private CloudTable responseTable;

        private AzureQueuePersist sessionPersist;

        private CloudStorageAccount storageAccount;

        [TestMethod]
        public async Task GetLargeRequestTest()
        {
            var request = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, largeMsg),
                null);
            request.Message.Headers.MessageId = new UniqueId();
            await this.sessionPersist.PutRequestAsync(request, null, 0);
            this.sessionPersist.CommitRequest();
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
            await Task.Delay(millisecondsDelay);
            this.sessionPersist.GetResponseAsync(this.GetLargeMessageTestCallback, 1);
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
            request.Message.Headers.MessageId = new UniqueId();
            await this.sessionPersist.PutRequestAsync(request, null, 0);
            this.sessionPersist.CommitRequest();
            await Task.Delay(millisecondsDelay);
            this.sessionPersist.GetRequestAsync(this.GetMessageTestCallback, null);
            while (!this.CallbackIsCalled)
            {
                await Task.Delay(millisecondsDelay);
            }

            Assert.AreEqual(true, this.IsExpected);
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
            await Task.Delay(millisecondsDelay);
            this.sessionPersist.GetResponseAsync(this.GetMessageTestCallback, 1);
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
            request.Message.Headers.MessageId = new UniqueId();
            await this.sessionPersist.PutRequestAsync(request, null, 0);
            this.sessionPersist.CommitRequest();
            await Task.Delay(millisecondsDelay);
            this.sessionPersist.GetRequestAsync(this.GetWrongMessageTestCallback, null);
            while (!this.CallbackIsCalled)
            {
                await Task.Delay(millisecondsDelay);
            }

            Assert.AreEqual(false, this.IsExpected);
        }

        /*[TestMethod]
        public async Task PutLargeRequestTest()
        {
            var request = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, largeMsg),
                null);
            this.sessionPersist.CloseFetchForTest();
            await Task.Delay(millisecondsDelay);
            this.sessionPersist.PutRequestAsync(request, null, 0);
            var dequeItem = (BrokerQueueItem)binFormatterField.Deserialize(
                await AzureStorageTool.GetMsgBody(
                    this.blobContainer,
                    (await this.requestQueue.GetMessageAsync()).AsBytes));
            CollectionAssert.AreEqual(largeMsg, dequeItem.Message.GetBody<byte[]>());
        }

        [TestMethod]
        public async Task PutLargeResponseTest()
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
            this.sessionPersist.CloseFetchForTest();
            this.sessionPersist.PutResponseAsync(response, null, 0);
            await Task.Delay(millisecondsDelay);
            var list = await AzureStorageTool.GetBatchEntityAsync(this.responseTable, 0);
            BrokerQueueItem dequeItem = null;
            if (list.Count > 0)
            {
                dequeItem = (BrokerQueueItem)binFormatterField.Deserialize(
                    await AzureStorageTool.GetMsgBody(this.blobContainer, list[0].Message));
            }

            CollectionAssert.AreEqual(largeMsg, dequeItem.Message.GetBody<byte[]>());
        }

        [TestMethod]
        public async Task PutRequestTest()
        {
            var request = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, shortMsg),
                null);
            this.sessionPersist.CloseFetchForTest();
            await Task.Delay(millisecondsDelay);
            this.sessionPersist.PutRequestAsync(request, null, 0);
            var dequeItem = (BrokerQueueItem)binFormatterField.Deserialize(
                await AzureStorageTool.GetMsgBody(
                    this.blobContainer,
                    (await this.requestQueue.GetMessageAsync()).AsBytes));
            Assert.AreEqual(shortMsg, dequeItem.Message.GetBody<string>());
        }

        [TestMethod]
        public async Task PutResponseTest()
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
            this.sessionPersist.CloseFetchForTest();
            this.sessionPersist.PutResponseAsync(response, null, 0);
            await Task.Delay(millisecondsDelay);
            var list = await AzureStorageTool.GetBatchEntityAsync(this.responseTable, 0);
            BrokerQueueItem dequeItem = null;
            if (list.Count > 0)
            {
                dequeItem = (BrokerQueueItem)binFormatterField.Deserialize(
                    await AzureStorageTool.GetMsgBody(this.blobContainer, list[0].Message));
            }

            Assert.AreEqual(shortMsg, dequeItem.Message.GetBody<string>());
        }

        [TestMethod]
        public async Task RestoreRequestTest()
        {
            this.sessionPersist.Dispose();
            await Task.Delay(millisecondsDelay);
            var message = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, shortMsg),
                null);
            message.Message.Headers.MessageId = new UniqueId(Guid.NewGuid());
            var cloudMessage = new CloudQueueMessage(AzureStorageTool.PrepareMessage(message));
            await this.pendingQueue.AddMessageAsync(cloudMessage);
            this.sessionPersist = new AzureQueuePersist(username, sessionId, clientId, storageConnectString);
            this.sessionPersist.CloseFetchForTest();

            // Casually request fetched by pre-fetcher.
            var dequeItem = (BrokerQueueItem)binFormatterField.Deserialize(
                await AzureStorageTool.GetMsgBody(
                    this.blobContainer,
                    (await this.requestQueue.GetMessageAsync()).AsBytes));
            Assert.AreEqual(shortMsg, dequeItem.Message.GetBody<string>());
        }*/

        [TestInitialize]
        public void TestInit()
        {
            this.storageAccount = CloudStorageAccount.Parse(storageConnectString);
            Debug.Print("connect emulator successfully!");
            clientId = Guid.NewGuid().ToString();
            this.sessionPersist = new AzureQueuePersist(username, sessionId, clientId, storageConnectString);
            this.requestQueue = this.storageAccount.CreateCloudQueueClient()
                .GetQueueReference(MakeQueuePath(sessionId, clientId, true));
            this.responseTable = this.storageAccount.CreateCloudTableClient()
                .GetTableReference(MakeTablePath(sessionId, clientId));
            this.blobContainer = this.storageAccount.CreateCloudBlobClient()
                .GetContainerReference(MakeQueuePath(sessionId, clientId, true));
            this.pendingQueue = this.storageAccount.CreateCloudQueueClient()
                .GetQueueReference(MakePendingPath(sessionId, clientId));
            this.CallbackIsCalled = false;
            this.IsExpected = false;
        }

        private static string MakePendingPath(string sessionId, string clientId)
        {
            return (PendingPathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + QueueNameFieldDelimeter
                    + clientId).ToLower();
        }

        private static string MakeQueuePath(string sessionId, string clientId, bool isRequest)
        {
            if (isRequest)
            {
                return (QueuePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + QueueNameFieldDelimeter
                        + clientId + QueueNameFieldDelimeter + RequestQueueSuffix).ToLower();
            }

            return (QueuePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + QueueNameFieldDelimeter
                    + clientId + QueueNameFieldDelimeter + ResponseQueueSuffix).ToLower();
        }

        private static string MakeTablePath(string sessionId, string clientId)
        {
            var sb = new StringBuilder();
            foreach (var str in clientId.Split('-'))
            {
                sb.Append(str);
            }

            return (QueuePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + sb + ResponseQueueSuffix)
                .ToLower();
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