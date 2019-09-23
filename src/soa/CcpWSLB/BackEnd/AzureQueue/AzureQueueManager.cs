// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.AzureQueue
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Xml;

    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;

    /// <summary>
    /// It is the manager to control both request and response queue.
    /// </summary>
    internal class AzureQueueManager : DisposableObject, IAzureQueueManager
    {
        /// <summary>
        /// It is the minimum count of the thread pool thread.
        /// </summary>
        private const int MinThreadsOfThreadpool = 64;

        /// <summary>
        /// Default retry policy for request storage operation.
        /// </summary>
        /// <remarks>
        /// Notice: Use NoRetry for request storage because of "at most once" semantic.
        /// </remarks>
        private static readonly IRetryPolicy RetryPolicyForRequestStorage = new NoRetry();

        /// <summary>
        /// A collection of ResponseQueueManager.
        /// Key: response queue name
        /// Value: ResponseQueueManager instance
        /// </summary>
        private ConcurrentDictionary<string, ResponseQueueManager> responseQueueManagers =
            new ConcurrentDictionary<string, ResponseQueueManager>();

        /// <summary>
        /// It stores the exception indicating that the response queue is not
        /// found. Response queue is per job per re-queue count. Broker doesn't
        /// recreate the response queue when it is deleted.
        /// </summary>
        private ConcurrentDictionary<string, ResponseStorageException> responseQueueNotFound =
            new ConcurrentDictionary<string, ResponseStorageException>();

        /// <summary>
        /// Cluster name.
        /// </summary>
        private string clusterName;

        /// <summary>
        /// Cluster Id.
        /// </summary>
        private Guid clusterId;

        /// <summary>
        /// Azure queue client instance.
        /// </summary>
        private CloudQueueClient queueClient;

        /// <summary>
        /// Azure blob client instance.
        /// </summary>
        private CloudBlobClient blobClient;

        /// <summary>
        /// Request queue/container is per cluster per Azure deployment, so
        /// broker should handle multiple request storages.
        /// </summary>
        /// <remarks>
        /// Key: Azure service name
        /// Value: tuple of cloud queue and blob container
        /// </remarks>
        private ConcurrentDictionary<string, Tuple<CloudQueue, CloudBlobContainer>> requestStorage
            = new ConcurrentDictionary<string, Tuple<CloudQueue, CloudBlobContainer>>();

        /// <summary>
        /// Hold the QueueAsyncResult for all the Azure side calculating
        /// messages. QueueAsyncResult contains callback information.
        /// </summary>
        private ConcurrentDictionary<UniqueId, QueueAsyncResult> callbacks
            = new ConcurrentDictionary<UniqueId, QueueAsyncResult>();

        /// <summary>
        /// Hold the message Ids for all the outstanding requests.
        /// Key: message Id
        /// Value: request queue name
        /// </summary>
        /// <remarks>
        /// Notice: Request queue is per cluster per Azure deployment. This
        /// dictionary is used to invoke callback for the outstanding requests
        /// if the request queue is deleted.
        /// </remarks>
        private ConcurrentDictionary<UniqueId, string> requestsMappingToRequestQueue
            = new ConcurrentDictionary<UniqueId, string>();

        /// <summary>
        /// Hold the message Ids for all the outstanding requests.
        /// Key: message Id
        /// Value: response storage name
        /// </summary>
        private ConcurrentDictionary<UniqueId, string> requestsMappingToResponseQueue
            = new ConcurrentDictionary<UniqueId, string>();

        /// <summary>
        /// Session Id.
        /// </summary>
        private string sessionId;

        /// <summary>
        /// This flag indicates if the request queue exists.
        /// </summary>
        private int requestQueueExist;

        /// <summary>
        /// Initializes a new instance of the AzureQueueManager class.
        /// </summary>
        /// <param name="sessionId">session Id</param>
        public AzureQueueManager(string sessionId, string clusterName, string clusterId)
        {
            ThreadPool.SetMinThreads(MinThreadsOfThreadpool, MinThreadsOfThreadpool);

            this.sessionId = sessionId;

            string schedulerName = null;

            try
            {
                schedulerName = SoaHelper.GetSchedulerName();
            }
            catch (InvalidOperationException e)
            {
                // It can happen in the in-proc broker scenario, because client
                // machine doesn't have registry key storing the headnode name.
                BrokerTracing.TraceError(
                    "[AzureQueueManager].AzureQueueManager: Failed to get scheduler name, {0}", e);
            }
            
            this.clusterName = clusterName;
            this.clusterId = new Guid(clusterId);
        }

        /// <summary>
        /// Gets or sets Azure storage connection string.
        /// </summary>
        /// <remarks>
        /// in order to delay load the storage connection string, it is not
        /// initialized in constructor.
        /// </remarks>
        public string StorageConnectionString
        {
            private get;
            set;
        }

        /// <summary>
        /// Gets the request queues.
        /// Key: Azure service name
        /// Value: Tuple of queue and blob container
        /// </summary>
        public ConcurrentDictionary<string, Tuple<CloudQueue, CloudBlobContainer>> RequestStorage
        {
            get
            {
                return this.requestStorage;
            }
        }

        /// <summary>
        /// Create storage queue
        /// </summary>
        /// <param name="queue">storage queue</param>
        /// <remarks>
        /// CreateIfNotExist method throws StorageClientException when queue is
        /// being deleted, so sleep for a while before retry.
        /// </remarks>
        public static void CreateQueueWithRetry(CloudQueue queue)
        {
            RetryHelper<object>.InvokeOperation(
            () =>
            {
                if (queue.CreateIfNotExists())
                {
                    BrokerTracing.TraceInfo(
                        "[AzureQueueManager].CreateQueueWithRetry: Create the queue {0}", queue.Name);
                }

                return null;
            },
            (e, count) =>
            {
                BrokerTracing.TraceError(
                    "Failed to create the queue {0}: {1}. Retry Count = {2}", queue.Name, e, count);

                StorageException se = e as StorageException;

                if (se != null)
                {
                    if (BurstUtility.GetStorageErrorCode(se) == QueueErrorCodeStrings.QueueAlreadyExists)
                    {
                        Thread.Sleep(TimeSpan.FromMinutes(1));
                    }
                }
            });
        }

        /// <summary>
        /// Create storage blob container.
        /// </summary>
        /// <param name="container">blob container</param>
        public static void CreateContainerWithRetry(CloudBlobContainer container)
        {
            RetryHelper<object>.InvokeOperation(
            () =>
            {
                if (container.CreateIfNotExists())
                {
                    BrokerTracing.TraceInfo(
                      "[AzureQueueManager].CreateContainerWithRetry: Create the container {0}", container.Name);
                }

                return null;
            },
            (e, count) =>
            {
                BrokerTracing.TraceError("Failed to create the container {0}: {1}. Retry Count = {2}", container.Name, e, count);

                StorageException se = e as StorageException;

                if (se != null)
                {
                    string errorCode = BurstUtility.GetStorageErrorCode(se);

                    // According to test, the error code is ResourceAlreadyExists.
                    // There is no doc about this, add ContainerAlreadyExists here.

                    // TODO: Azure storage SDK 2.0
                    if (//errorCode == StorageErrorCodeStrings.ResourceAlreadyExists ||
                          errorCode == StorageErrorCodeStrings.ContainerAlreadyExists)
                    {
                        Thread.Sleep(TimeSpan.FromMinutes(1));
                    }
                }
            });
        }

        /// <summary>
        /// Create message retriever to monitor the response queue and gets
        /// messages from it.
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <param name="jobRequeueCount">job re-queue count</param>
        /// <returns>response storage name</returns>
        public string Start(string jobId, int jobRequeueCount)
        {
            string responseStorageName = SoaHelper.GetResponseStorageName(this.clusterId.ToString(), jobId, jobRequeueCount);

            ResponseQueueManager manager =
                this.responseQueueManagers.GetOrAdd(
                    responseStorageName,
                    (key) =>
                    {
                        BrokerTracing.TraceVerbose(
                            "[AzureQueueManager].Start: Create the ResponseQueueManager, job requeue count {0}, {1}",
                            key,
                            responseStorageName);

                        return new ResponseQueueManager(this, this.sessionId, responseStorageName, this.StorageConnectionString);
                    });

            // Following method only start manager once.
            manager.Start();

            return responseStorageName;
        }

        /// <summary>
        /// Create request queue for specified Azure service.
        /// </summary>
        /// <param name="azureServiceName">azure service name</param>
        public void CreateRequestStorage(string azureServiceName)
        {
            this.requestStorage.GetOrAdd(
                azureServiceName,
                (key) =>
                {
                    string requestStorageName = SoaHelper.GetRequestStorageName(this.clusterId.ToString(), key);

                    BrokerTracing.TraceVerbose(
                        "[AzureQueueManager].CreateRequestStorage: Try to create the request storage {0} for Azure service {1}",
                        requestStorageName,
                        key);

                    this.CreateStorageClient(RetryPolicyForRequestStorage);

                    CloudQueue queue = this.queueClient.GetQueueReference(requestStorageName);

                    AzureQueueManager.CreateQueueWithRetry(queue);

                    CloudBlobContainer container = this.blobClient.GetContainerReference(requestStorageName);

                    AzureQueueManager.CreateContainerWithRetry(container);

                    if (Interlocked.CompareExchange(ref this.requestQueueExist, 1, 0) == 0)
                    {
                        BrokerTracing.EtwTrace.LogQueueCreatedOrExist(this.sessionId, requestStorageName);
                    }

                    return new Tuple<CloudQueue, CloudBlobContainer>(queue, container);
                });
        }

        /// <summary>
        /// Add QueueAsyncResult to the dictionary.
        /// </summary>
        /// <param name="result">async result</param>
        /// <param name="requestQueueName">request queue name</param>
        /// <param name="responseQueueName">response queue name</param>
        public void AddQueueAsyncResult(QueueAsyncResult result, string requestQueueName, string responseQueueName)
        {
            ResponseStorageException e;

            this.responseQueueNotFound.TryGetValue(responseQueueName, out e);

            if (e == null)
            {
                BrokerTracing.TraceVerbose(
                    "[AzureQueueManager].AddQueueAsyncResult: Add QueueAsyncResult {0} to the dictionary.",
                    result.MessageId);

                this.callbacks.AddOrUpdate(result.MessageId, result, (key, value) => result);

                this.requestsMappingToRequestQueue.AddOrUpdate(result.MessageId, requestQueueName, (key, value) => requestQueueName);

                this.requestsMappingToResponseQueue.AddOrUpdate(result.MessageId, responseQueueName, (key, value) => responseQueueName);
            }
            else
            {
                BrokerTracing.TraceError(
                    "[AzureQueueManager].AddQueueAsyncResult: Response queue is not found, {0}",
                    responseQueueName);

                throw e;
            }
        }

        /// <summary>
        /// Remove QueueAsyncResult from the dictionary.
        /// </summary>
        /// <param name="messageId">message Id</param>
        public void RemoveQueueAsyncResult(UniqueId messageId)
        {
            string value;

            if (!this.requestsMappingToRequestQueue.TryRemove(messageId, out value))
            {
                BrokerTracing.TraceError(
                    "[AzureQueueManager].CompleteCallback: Failed to remove message Id {0} from requestsMappingToRequestQueue.",
                    messageId);
            }

            if (!this.requestsMappingToResponseQueue.TryRemove(messageId, out value))
            {
                BrokerTracing.TraceError(
                    "[AzureQueueManager].CompleteCallback: Failed to remove message Id {0} from requestsMappingToResponseQueue.",
                    messageId);
            }

            QueueAsyncResult result;

            if (!this.callbacks.TryRemove(messageId, out result))
            {
                BrokerTracing.TraceError(
                    "[AzureQueueManager].CompleteCallback: Failed to remove QueueAsyncResult {0} from callbacks.",
                    messageId);
            }
            else
            {
                BrokerTracing.TraceVerbose(
                    "[AzureQueueManager].CompleteCallback: Remove QueueAsyncResult {0} from callbacks.",
                    messageId);
            }
        }

        /// <summary>
        /// Invoke the specified callback and delete it from the dictionary.
        /// </summary>
        /// <param name="asyncResult">it contains callback info</param>
        /// <param name="response">response message of the request</param>
        /// <param name="exception">exception occurred when process the request</param>
        public void CompleteCallback(QueueAsyncResult asyncResult, Message response, Exception exception)
        {
            try
            {
                using (asyncResult)
                {
                    asyncResult.ResponseMessage = response;

                    asyncResult.Exception = exception;

                    asyncResult.Complete();
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureQueueManager].CompleteCallback: Error occurs, {0}",
                    e);
            }
        }

        /// <summary>
        /// Handle the invalid request queue.
        /// </summary>
        /// <param name="e">
        /// exception occurred when access the response queue
        /// </param>
        /// <param name="requestQueueName">
        /// request queue name
        /// </param>
        public void HandleInvalidRequestQueue(StorageException e, string requestQueueName)
        {
            BrokerTracing.TraceWarning(
                "[AzureQueueManager].HandleInvalidRequestQueue: Exception occurs when access request queue, {0}, {1}",
                BurstUtility.GetStorageErrorCode(e),
                e);

            if (BurstUtility.IsQueueNotFound(e))
            {
                if (Interlocked.CompareExchange(ref this.requestQueueExist, 0, 1) == 1)
                {
                    BrokerTracing.EtwTrace.LogQueueNotExist(this.sessionId, requestQueueName);
                }

                this.TriggerCallbackForInvalidRequestQueue(requestQueueName, e);
            }
        }

        /// <summary>
        /// Call response message's callback method.
        /// </summary>
        /// <param name="message">cloud queue message</param>
        /// <param name="response">WCF response message</param>
        /// <returns>message Id</returns>
        public UniqueId ResponseCallback(CloudQueueMessage message, Message response)
        {
            UniqueId messageId = response.Headers.RelatesTo;
            Debug.Assert(messageId != null, "ResponseCallback: messageId can not be null.");

            BrokerTracing.TraceVerbose(
                "[AzureQueueManager].ResponseCallback: Get message {0} from queue, queue message Id {1}, dequeue count {2}, pop receipt {3}",
                messageId,
                message.Id,
                message.DequeueCount,
                message.PopReceipt);

            QueueAsyncResult result;

            if (this.callbacks.TryGetValue(messageId, out result))
            {
                BrokerTracing.TraceVerbose(
                    "[AzureQueueManager].ResponseCallback: Get callback for message {0} from dictionary.",
                    messageId);

                this.CompleteCallback(result, response, null);
            }
            else
            {
                // The response is already handled by others, which will also
                // delete the response from queue. But message deletion may
                // fail because message etag changes, so still need invoker of
                // this method to delete message.
                BrokerTracing.TraceWarning(
                    "[AzureQueueManager].ResponseCallback: Can not get callback for message {0} from dictionary, queue message Id {1}, dequeue count {2}, pop receipt {3}",
                    messageId,
                    message.Id,
                    message.DequeueCount,
                    message.PopReceipt);
            }

            return messageId;
        }

        /// <summary>
        /// Invoke the cached callback for specified response queue, which is
        /// deleted when session is running.
        /// </summary>
        /// <param name="e">
        /// exception occurred when access the queue
        /// </param>
        public void TriggerCallbackForInvalidResponseQueue(ResponseStorageException e)
        {
            BrokerTracing.TraceWarning(
                "[AzureQueueManager].TriggerCallbackForInvalidResponseQueue: Handle the invalid response queue {0}",
                e.ResponseStorageName);

            this.responseQueueNotFound.TryAdd(e.ResponseStorageName, e);

            this.TriggerCallback(this.requestsMappingToResponseQueue, e.ResponseStorageName, e);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                try
                {
                    this.responseQueueManagers.Values.AsParallel<ResponseQueueManager>().ForAll<ResponseQueueManager>(
                        (manager) =>
                        {
                            manager.Dispose();
                        });
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError("[AzureQueueManager].Dispose: Error occurs, {0}", e);
                }
            }
        }

        /// <summary>
        /// Create cloud queue/blob client. Don't create it in class
        /// constructor in order to delay retrieve the connection string.
        /// </summary>
        /// <param name="retryPolicy">
        /// It is storage retry policy. We have different policy for request
        /// storage and response storage.
        /// </param>
        private void CreateStorageClient(IRetryPolicy retryPolicy)
        {
            if (this.queueClient == null || this.blobClient == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.StorageConnectionString);

                if (this.queueClient == null)
                {
                    CloudQueueClient tmpQueueClient = storageAccount.CreateCloudQueueClient();

                    tmpQueueClient.DefaultRequestOptions.RetryPolicy = retryPolicy;

                    Interlocked.CompareExchange<CloudQueueClient>(ref this.queueClient, tmpQueueClient, null);
                }

                if (this.blobClient == null)
                {
                    CloudBlobClient tmpBlobClient = storageAccount.CreateCloudBlobClient();

                    tmpBlobClient.DefaultRequestOptions.RetryPolicy = retryPolicy;

                    Interlocked.CompareExchange<CloudBlobClient>(ref this.blobClient, tmpBlobClient, null);
                }
            }
        }

        /// <summary>
        /// Invoke the cached callback for specified request queue, which is
        /// deleted when session is running.
        /// </summary>
        /// <param name="requestQueueName">
        /// request queue name
        /// </param>
        /// <param name="e">
        /// exception occurred when access the request queue
        /// </param>
        private void TriggerCallbackForInvalidRequestQueue(string requestQueueName, StorageException e)
        {
            BrokerTracing.TraceWarning(
                "[AzureQueueManager].TriggerCallbackForInvalidRequestQueue: Handle the invalid request queue {0}",
                requestQueueName);

            this.TriggerCallback(this.requestsMappingToRequestQueue, requestQueueName, e);
        }

        /// <summary>
        /// Invoke the cached callback for specified target queue name.
        /// </summary>
        /// <param name="messageIdMapping">
        /// mapping from message Id to request/response name
        /// </param>
        /// <param name="queueName">
        /// target request/response queue name
        /// </param>
        /// <param name="e">
        /// exception occurred when access the queue
        /// </param>
        private void TriggerCallback(ConcurrentDictionary<UniqueId, string> messageIdMapping, string queueName, StorageException e)
        {
            BrokerTracing.TraceVerbose(
                "[AzureQueueManager].TriggerCallback: Handle the invalid queue {0}",
                queueName);

            while (messageIdMapping.Values.Contains<string>(queueName, StringComparer.InvariantCultureIgnoreCase))
            {
                List<UniqueId> messageIds = new List<UniqueId>();

                string tmpQueueName;

                foreach (UniqueId id in messageIdMapping.Keys)
                {
                    if (messageIdMapping.TryGetValue(id, out tmpQueueName))
                    {
                        if (string.Equals(queueName, tmpQueueName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            messageIds.Add(id);
                        }
                    }
                }

                BrokerTracing.TraceVerbose(
                    "[AzureQueueManager].TriggerCallback: Invoke callback for {0} messages.",
                    messageIds.Count);

                foreach (UniqueId messageId in messageIds)
                {
                    QueueAsyncResult result;

                    if (this.callbacks.TryGetValue(messageId, out result))
                    {
                        BrokerTracing.TraceVerbose(
                            "[AzureQueueManager].TriggerCallback: Get callback for message {0} from callbacks.",
                            messageId);

                        // following method deletes callback and deletes message from mapping
                        this.CompleteCallback(result, null, e);
                    }
                    else
                    {
                        BrokerTracing.TraceWarning(
                            "[AzureQueueManager].TriggerCallback: Can not get callback for message {0} from callbacks.",
                            messageId);
                    }

                    if (!messageIdMapping.TryRemove(messageId, out tmpQueueName))
                    {
                        BrokerTracing.TraceWarning(
                            "[AzureQueueManager].TriggerCallback: Can not remove message {0} from mapping.",
                            messageId);
                    }
                }
            }
        }
    }
}
