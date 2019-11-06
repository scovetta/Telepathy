// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Persistences.AzureQueuePersist
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;

    public class AzureStorageTool
    {
        private const string MsgPrefix = "MsgInBlob";

        /// <summary>
        ///     the regex to match the message body
        /// </summary>
        private static readonly Regex BlobMsgRegex = new Regex(MsgPrefix + "(?<BlobName>.*)$", RegexOptions.IgnoreCase);

        private static readonly int batchSize = 1000;

        private static readonly IFormatter formatter = new BinaryFormatter();

        private static readonly string queueTableName = "StorageInfoTable";

        public static async Task AddMsgToTable(CloudTable table, ResponseEntity entity)
        {
            try
            {
                var operation = TableOperation.Insert(entity);

                await table.ExecuteAsync(operation);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .AddMsgToTable: cannot insert the entity into table, index = {0}, guid = {1}, exception: {2}.", entity.PartitionKey, entity.RowKey, e);
                TableOperation retrieveOperation = TableOperation.Retrieve<ResponseEntity>(entity.PartitionKey, entity.RowKey);
                TableResult retrievedResult = table.Execute(retrieveOperation);
                if (retrievedResult.Result != null)
                {
                    BrokerTracing.TraceInfo("The entity is already exists in Table");
                }

                throw;
            }
        }


        public static async Task<long> CountFailed(string connectString, string sessionId, string tableName)
        {
            var table = GetTableClient(connectString).GetTableReference(queueTableName);
            var retrive = TableOperation.Retrieve<QueueInfo>(sessionId, tableName);
            TableResult result = await table.ExecuteAsync(retrive);

            if (result.Result != null  && long.TryParse(((QueueInfo)result.Result).Note, out var failedCount))
            {
                return failedCount;
            }

            return 0;
        }

        public static async Task<long> CountTableEntity(string connectString, string tableName)
        {
            var table = GetTableClient(connectString).GetTableReference(tableName);
            var query = new TableQuery<DynamicTableEntity>().Select(new[] { "PartitionKey" });
            var list = new List<DynamicTableEntity>();
            TableContinuationToken token = null;
            do
            {
                var seg = await table.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                list.AddRange(seg);
            }
            while (token != null);

            return list.Count;
        }

        public static async Task<CloudBlobContainer> CreateBlobContainerAsync(
            string connectString,
            string blobContainerName)
        {
            var container = GetBlobContainer(connectString, blobContainerName);
            await container.CreateIfNotExistsAsync();
            return container;
        }

        public static async Task<byte[]> CreateBlobFromBytes(CloudBlobContainer container, byte[] bytes)
        {
            await container.CreateIfNotExistsAsync();
            var blobName = Guid.NewGuid().ToString();
            var blob = container.GetBlockBlobReference(blobName);
            var content = new MemoryStream(bytes);
            await blob.UploadFromStreamAsync(content);
            return Encoding.ASCII.GetBytes(MsgPrefix + blobName);
        }

        public static async Task<CloudQueue> CreateQueueAsync(
            string connectString,
            string sessionId,
            string queueName,
            string clientId,
            bool isRequest,
            string note)
        {
            var info = new QueueInfo(sessionId, queueName, clientId, isRequest, note);
            try
            {
                var table = GetTableClient(connectString).GetTableReference(queueTableName);
                await table.CreateIfNotExistsAsync();
                var insertOperation = TableOperation.Insert(info);
                await table.ExecuteAsync(insertOperation);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .CreateQueueAsync: queueName={0} create info to table failed, the exception, {1}",
                    queueName,
                    e);
                throw;
            }

            try
            {
                var queue = GetQueueClient(connectString).GetQueueReference(queueName);
                if (await queue.CreateIfNotExistsAsync())
                {
                    return queue;
                }

                BrokerTracing.TraceWarning(
                    "[AzureStorageTool] .CreateQueueAsync: queueName={0} has existed.",
                    queueName);
                throw new Exception("Queue with the queueName has existed.");
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .CreateQueueAsync: queueName={0} create queue failed, the exception, {1}",
                    queueName,
                    e);
                throw;
            }
        }

        public static async Task<CloudTable> CreateTableAsync(
            string connectString,
            string sessionId,
            string tableName,
            string clientId,
            bool isRequest,
            string note)
        {
            var info = new QueueInfo(sessionId, tableName, clientId, isRequest, note);
            try
            {
                var table = GetTableClient(connectString).GetTableReference(queueTableName);
                await table.CreateIfNotExistsAsync();
                var insertOperation = TableOperation.Insert(info);
                await table.ExecuteAsync(insertOperation);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .CreateTableAsync: Table={0} insert info in storage table failed, the exception, {1}",
                    tableName,
                    e);
                throw;
            }

            try
            {
                var responseTable = GetTableClient(connectString).GetTableReference(tableName);
                if (await responseTable.CreateIfNotExistsAsync())
                {
                    return responseTable;
                }

                throw new Exception("Table with the tableName has existed.");
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .CreateTableAsync: Table={0} create error, the exception, {1}",
                    tableName,
                    e);
                throw;
            }
        }

        public static async Task DeleteQueueAsync(string connectString, string sessionId, string queueName)
        {
            var info = new QueueInfo(sessionId, queueName);
            info.ETag = "*"; // no etag cannot delete.
            try
            {
                var table = GetTableClient(connectString).GetTableReference(queueTableName);
                var deleteOperation = TableOperation.Delete(info);
                await table.ExecuteAsync(deleteOperation);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .DeleteQueueAsync: queueName={0} deleted from table failed, the exception, {1}",
                    queueName,
                    e);
            }

            try
            {
                var queue = GetQueueClient(connectString).GetQueueReference(queueName);
                await queue.DeleteIfExistsAsync();
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .DeleteQueueAsync: queueName={0} delete queue failed, the exception, {1}",
                    queueName,
                    e);
            }
        }

        public static async Task DeleteTableAsync(string connectString, string sessionId, string responseTableName)
        {
            var info = new QueueInfo(sessionId, responseTableName);
            info.ETag = "*"; // no etag cannot delete.
            try
            {
                var table = GetTableClient(connectString).GetTableReference(queueTableName);
                var deleteOperation = TableOperation.Delete(info);
                await table.ExecuteAsync(deleteOperation);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .DeleteTableAsync: Table={0} delete from storage table failed, the exception, {1}",
                    responseTableName,
                    e);
                throw;
            }

            try
            {
                var responseTable = GetTableClient(connectString).GetTableReference(responseTableName);
                await responseTable.DeleteIfExistsAsync();
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .DeleteTableAsync: Table={0} delete failed, the exception, {1}",
                    responseTableName,
                    e);
                throw;
            }
        }

        public static async Task<bool> ExistsQueue(string connectString, string queueName)
        {
            var queue = GetQueueClient(connectString).GetQueueReference(queueName.ToLower());
            return await queue.ExistsAsync();
        }

        public static async Task<bool> ExistTable(string connectString, string tableName)
        {
            var table = GetTableClient(connectString).GetTableReference(tableName.ToLower());
            return await table.ExistsAsync();
        }

        public static async Task<List<QueueInfo>> GetAllQueues(string connectString)
        {
            var tableClient = GetTableClient(connectString);
            var table = tableClient.GetTableReference(queueTableName);
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<QueueInfo>();
            var list = new List<QueueInfo>();
            TableContinuationToken token = null;
            do
            {
                var seg = await table.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                list.AddRange(seg);
            }
            while (token != null);

            return list;
        }

        public static async Task<List<ResponseEntity>> GetBatchEntityAsync(CloudTable tableField, long lastIndex, long ackIndex)
        {
            var query = new TableQuery<ResponseEntity>().Where("RowKey gt '" + lastIndex + "' and RowKey le '" + ackIndex + "'");
            var list = new List<ResponseEntity>();
            TableContinuationToken token = null;
            do
            {
                var seg = await tableField.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                list.AddRange(seg);
                if (list.Count >= batchSize)
                {
                    return list;
                }
            }
            while (token != null);

            return list;
        }

        public static async Task<Stream> GetMsgBody(CloudBlobContainer container, byte[] bytes)
        {
            string str;
            try
            {
                str = Encoding.ASCII.GetString(bytes);
            }
            catch
            {
                return new MemoryStream(bytes);
            }

            var m = BlobMsgRegex.Match(str);
            if (m.Success)
            {
                var blobName = m.Groups["BlobName"].Value;
                var content = new MemoryStream();
                try
                {
                    var blob = container.GetBlockBlobReference(blobName);
                    await blob.DownloadToStreamAsync(content);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                content.Position = 0;
                return content;
            }

            return new MemoryStream(bytes);
        }

        public static async Task<CloudQueue> GetQueue(string connectString, string queueName)
        {
            var queue = GetQueueClient(connectString).GetQueueReference(queueName.ToLower());
            await queue.FetchAttributesAsync();
            return queue;
        }

        public static async Task<List<QueueInfo>> GetQueuesFromTable(string connectString, string sessionId)
        {
            var tableClient = GetTableClient(connectString);
            var table = tableClient.GetTableReference(queueTableName);
            await table.CreateIfNotExistsAsync();

            var query = new TableQuery<QueueInfo>().Where("PartitionKey eq '" + sessionId + "'");
            var list = new List<QueueInfo>();
            TableContinuationToken token = null;
            do
            {
                var seg = await table.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                list.AddRange(seg);
            }
            while (token != null);

            return list;
        }

        public static CloudTable GetTable(string connectString, string tableName)
        {
            var table = GetTableClient(connectString).GetTableReference(tableName.ToLower());
            return table;
        }

        public static async Task<bool> IsExistedResponse(CloudTable table, string messageId)
        {
            var query = new TableQuery<DynamicTableEntity>().Where("MessageId eq '" + messageId + "'")
                .Select(new[] { "PartitionKey" });
            TableContinuationToken token = null;
            do
            {
                var seg = await table.ExecuteQuerySegmentedAsync(query, token);
                token = seg.ContinuationToken;
                if (seg.ToList().Count > 0)
                {
                    return true;
                }
            }
            while (token != null);

            return false;
        }

        public static byte[] PrepareMessage(object obj)
        {
            if (obj != null)
            {
                using (var memorySteam = new MemoryStream())
                {
                    formatter.Serialize(memorySteam, obj);
                    return memorySteam.ToArray();
                }
            }

            return null;
        }

        public static async Task RestoreRequest(
            CloudQueue requestQueue,
            CloudQueue pendingQueue,
            CloudTable responseTable,
            CloudBlobContainer container)
        {
            try
            {
                while (true)
                {
                    var message = await pendingQueue.PeekMessageAsync();
                    if (message == null)
                    {
                        break;
                    }

                    var messageId =
                        ((BrokerQueueItem)formatter.Deserialize(await GetMsgBody(container, message.AsBytes))).Message
                        .Headers.MessageId.ToString();
                    BrokerTracing.TraceVerbose(
                        "[AzureStorageTool] .CheckRequestQueue: queueName = {0}, cloudMessageId = {1}, messageId = {2}",
                        pendingQueue.Name,
                        message.Id,
                        messageId);
                    var query = new TableQuery<TableEntity>().Where("MessageId eq '" + messageId + "'");
                    var list = responseTable.ExecuteQuery(query).ToList();
                    if (list.Count > 0)
                    {
                        await pendingQueue.DeleteMessageAsync(await pendingQueue.GetMessageAsync());
                    }
                    else
                    {
                        // Add msg to request queue & delete it from pending queue.
                        message = await pendingQueue.GetMessageAsync();
                        await requestQueue.AddMessageAsync(new CloudQueueMessage(message.AsBytes));
                        await pendingQueue.DeleteMessageAsync(message);
                        BrokerTracing.TraceVerbose(
                            "[AzureStorageTool] .CheckRequestQueue: messageId = {0} is restored into request queue.",
                            messageId);
                    }
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .CheckRequestQueue: queueName={0}, responseTable={1}, the exception, {2}",
                    pendingQueue.Name,
                    responseTable.Name,
                    e);
                throw;
            }

            await requestQueue.FetchAttributesAsync();
        }

        public static async Task UpdateInfo(string connectString, string sessionId, string queueName, string note)
        {
            var tableClient = GetTableClient(connectString);
            try
            {
                var table = tableClient.GetTableReference(queueTableName);
                var updateOp = TableOperation.Merge(new QueueInfo(sessionId, queueName, note));
                await table.ExecuteAsync(updateOp);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageTool] .UpdateInfo: queueName={0} update queue info in table failed, the exception, {1}",
                    queueName,
                    e);
                throw;
            }
        }

        private static CloudBlobContainer GetBlobContainer(string storageConnectString, string containerName)
        {
            var blobAccount = CloudStorageAccount.Parse(storageConnectString);
            return blobAccount.CreateCloudBlobClient().GetContainerReference(containerName);
        }

        private static CloudQueueClient GetQueueClient(string storageConnectString)
        {
            var queueAccount = CloudStorageAccount.Parse(storageConnectString);
            return queueAccount.CreateCloudQueueClient();
        }

        private static CloudTableClient GetTableClient(string storageConnectString)
        {
            var tableAccount = CloudStorageAccount.Parse(storageConnectString);
            return tableAccount.CreateCloudTableClient();
        }
    }

    public class QueueInfo : TableEntity
    {
        public QueueInfo()
        {
        }

        public QueueInfo(string sessionId, string queueName)
            : base(sessionId, queueName)
        {
        }

        public QueueInfo(string sessionId, string queueName, string note)
            : base(sessionId, queueName)
        {
            this.Note = note;
        }

        public QueueInfo(string sessionId, string queueName, string clientId, bool isRequest, string note)
            : base(sessionId, queueName)
        {
            this.ClientId = clientId;
            this.IsRequest = isRequest;
            this.Note = note;
        }

        public string ClientId { get; set; }

        public bool IsRequest { get; set; }

        public string Note { get; set; }

        public string SessionId => this.PartitionKey;
    }

    public class ResponseEntity : TableEntity
    {
        public ResponseEntity()
        {
        }

        internal ResponseEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
        }

        internal ResponseEntity(string partitionKey, string rowKey, string messageId, byte[] message)
            : base(partitionKey, rowKey)
        {
            this.Message = message;
            this.MessageId = messageId;
        }

        public byte[] Message { get; set; }

        public string MessageId { get; set; }
    }
}