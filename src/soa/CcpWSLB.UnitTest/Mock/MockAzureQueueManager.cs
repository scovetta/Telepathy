using System;
using System.Collections.Concurrent;
using System.ServiceModel.Channels;
using System.Xml;
using Microsoft.Hpc.Scheduler.Session.Common;
using Microsoft.Hpc.ServiceBroker.BackEnd;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Hpc.ServiceBroker.UnitTest.Mock
{
    internal class MockAzureQueueManager : IAzureQueueManager
    {
        public MockAzureQueueManager()
        {
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=soa;AccountKey=xxx==");

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // just get reference instead of creating it
            CloudQueue queue = queueClient.GetQueueReference("queue");

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // just get reference instead of creating it
            CloudBlobContainer container = blobClient.GetContainerReference("container");

            Tuple<CloudQueue, CloudBlobContainer> tuple = new Tuple<CloudQueue, CloudBlobContainer>(queue, container);

            requestStorage.AddOrUpdate("svc", tuple, (key, value) => tuple);
        }

        public string StorageConnectionString
        {
            set
            {
            }
        }

        private ConcurrentDictionary<string, Tuple<CloudQueue, CloudBlobContainer>> requestStorage
            = new ConcurrentDictionary<string, Tuple<CloudQueue, CloudBlobContainer>>();

        public ConcurrentDictionary<string, Tuple<CloudQueue, CloudBlobContainer>> RequestStorage
        {
            get
            {
                return this.requestStorage;
            }
        }

        private ConcurrentDictionary<UniqueId, QueueAsyncResult> callbacks
            = new ConcurrentDictionary<UniqueId, QueueAsyncResult>();

        public string Start(int jobId, int jobRequeueCount)
        {
            throw new NotImplementedException();
        }

        public string Start(string jobId, int jobRequeueCount) => throw new NotImplementedException();

        public void CreateRequestStorage(string azureServiceName)
        {
            throw new NotImplementedException();
        }

        public void AddQueueAsyncResult(QueueAsyncResult result, string requestQueueName, string responseQueueName)
        {
            this.callbacks.AddOrUpdate(result.MessageId, result, (key, value) => result);
        }

        public void RemoveQueueAsyncResult(UniqueId messageId)
        {
            QueueAsyncResult result;

            this.callbacks.TryRemove(messageId, out result);
        }

        public void CompleteCallback(QueueAsyncResult asyncResult, Message response, Exception exception)
        {
            using (asyncResult)
            {
                asyncResult.ResponseMessage = response;

                asyncResult.Exception = exception;

                asyncResult.Complete();
            }
        }

        public void HandleInvalidRequestQueue(StorageException e, string requestQueueName)
        {
        }

        public UniqueId ResponseCallback(CloudQueueMessage message, Message response)
        {
            throw new NotImplementedException();
        }

        public void TriggerCallbackForInvalidResponseQueue(ResponseStorageException e)
        {
            throw new NotImplementedException();
        }
    }
}
