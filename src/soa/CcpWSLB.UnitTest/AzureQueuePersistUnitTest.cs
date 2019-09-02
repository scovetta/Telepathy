namespace CcpWSLB.UnitTest
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Azure;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage.AzureQueuePersist;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage.AzureStorageTool;
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

        private static readonly Guid guid = Guid.NewGuid();

        private static readonly string clientId = guid.ToString();

        private static readonly byte[] largeMsg = new byte[64000];

        private static readonly string PrivatePathPrefix = "Private";

        private static readonly string QueueNameFieldDelimeter = "-";

        private static readonly string QueuePathPrefix = "HPC";

        private static readonly string RequestQueueSuffix = "REQUESTS";

        private static readonly string ResponseQueueSuffix = "RESPONSES";

        private static readonly int sessionId = 1;

        private static readonly string shortMsg = "This is short message!";

        private static readonly string storageConnectString =
            CloudConfigurationManager.GetSetting("StorageConnectionString");

        private static readonly string username = "Any";

        private CloudBlobContainer blobContainer;

        private CloudQueue privateQueue;

        private CloudQueue requestQueue;

        private CloudTable responseTable;

        private AzureQueuePersist sessionPersist;

        private CloudStorageAccount storageAccount;

        [TestMethod]
        public void GetLargeRequestTest()
        {
            var request = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, shortMsg),
                null);
            this.sessionPersist.PutRequestAsync(request, null, 0);
            this.sessionPersist.CommitRequest();
            this.sessionPersist.GetRequestAsync(GetLargeMessageTestCallback, null);
        }

        [TestMethod]
        public void GetLargeResponseTest()
        {
            var response = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, largeMsg),
                null);
            response.PersistAsyncToken.AsyncToken = Guid.NewGuid().ToString();
            this.sessionPersist.PutResponseAsync(response, null, 0);
            this.sessionPersist.GetResponseAsync(GetLargeMessageTestCallback, null);
        }

        [TestMethod]
        public void GetRequestTest()
        {
            var request = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, shortMsg),
                null);
            this.sessionPersist.PutRequestAsync(request, null, 0);
            this.sessionPersist.CommitRequest();
            this.sessionPersist.GetRequestAsync(GetMessageTestCallback, null);
        }

        [TestMethod]
        public void GetResponseTest()
        {
            var response = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, shortMsg),
                null);
            response.PersistAsyncToken.AsyncToken = Guid.NewGuid().ToString();
            this.sessionPersist.PutResponseAsync(response, null, 0);
            this.sessionPersist.GetResponseAsync(GetMessageTestCallback, null);
        }

        [TestMethod]
        public async Task PutLargeRequestTest()
        {
            var request = new BrokerQueueItem(
                null,
                Message.CreateMessage(MessageVersion.Soap12WSAddressing10, action, largeMsg),
                null);
            this.sessionPersist.CloseFetchForTest();
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
            await Task.Delay(500);
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
            await Task.Delay(500);
            var list = await AzureStorageTool.GetBatchEntityAsync(this.responseTable, 0);
            BrokerQueueItem dequeItem = null;
            if (list.Count > 0)
            {
                dequeItem = (BrokerQueueItem)binFormatterField.Deserialize(
                    await AzureStorageTool.GetMsgBody(this.blobContainer, list[0].Message));
            }

            Assert.AreEqual(shortMsg, dequeItem.Message.GetBody<string>());
        }

        [TestInitialize]
        public void TestInit()
        {
            this.storageAccount = CloudStorageAccount.Parse(storageConnectString);
            Debug.Print("connect emulator successfully!");
            this.sessionPersist = new AzureQueuePersist(username, sessionId, clientId, storageConnectString);
            this.requestQueue = this.storageAccount.CreateCloudQueueClient()
                .GetQueueReference(MakeQueuePath(sessionId, clientId, true));
            this.responseTable = this.storageAccount.CreateCloudTableClient()
                .GetTableReference(MakeTablePath(sessionId, clientId));
            this.responseTable.DeleteIfExists();
            this.responseTable.CreateIfNotExists();
            this.blobContainer = this.storageAccount.CreateCloudBlobClient()
                .GetContainerReference(MakeQueuePath(sessionId, clientId, true));

            this.privateQueue = this.storageAccount.CreateCloudQueueClient()
                .GetQueueReference(MakePrivatePath(sessionId, clientId));
            this.requestQueue.Clear();
            this.privateQueue.Clear();
        }

        private static void GetLargeMessageTestCallback(
            BrokerQueueItem persistMessage,
            object state,
            Exception exception)
        {
            CollectionAssert.AreEqual(largeMsg, persistMessage.Message.GetBody<byte[]>());
        }

        private static void GetMessageTestCallback(BrokerQueueItem persistMessage, object state, Exception exception)
        {
            Assert.AreEqual(shortMsg, persistMessage.Message.GetBody<string>());
        }

        private static string MakePrivatePath(int sessionId, string clientId)
        {
            return (PrivatePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + QueueNameFieldDelimeter
                    + clientId).ToLower();
        }

        private static string MakeQueuePath(int sessionId, string clientId, bool isRequest)
        {
            if (isRequest)
            {
                return (QueuePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + QueueNameFieldDelimeter
                        + clientId + QueueNameFieldDelimeter + RequestQueueSuffix).ToLower();
            }

            return (QueuePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + QueueNameFieldDelimeter
                    + clientId + QueueNameFieldDelimeter + ResponseQueueSuffix).ToLower();
        }

        private static string MakeTablePath(int sessionId, string clientId)
        {
            var sb = new StringBuilder();
            foreach (var str in clientId.Split('-'))
            {
                sb.Append(str);
            }

            return (QueuePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + sb + ResponseQueueSuffix)
                .ToLower();
        }
    }
}