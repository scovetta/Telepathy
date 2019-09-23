// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.AzureQueue
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;

    /// <summary>
    /// It is the manager to control response queue.
    /// </summary>
    internal class ResponseQueueManager : DisposableObjectSlim
    {
        /// <summary>
        /// The upper limit of the thread pool threads processing responses.
        /// </summary>
        private const int MessageProcessWorker = 32;

        /// <summary>
        /// It is concurrency value used by message retriever.
        /// </summary>
        /// <remarks>
        /// Notice: According to test, should not use large concurrency value.
        /// </remarks>
        private const int RetrieverConcurrency = 8;

        /// <summary>
        /// It is wait time for messageProcessorTask to exit.
        /// </summary>
        private const int WaitTimeForTaskInMillisecond = 3 * 1000;

        /// <summary>
        /// Retry limit for MessageProcessor.
        /// </summary>
        private const int MessageProcessorRetryLimit = 3;

        /// <summary>
        /// It is default visible timeout for the response messages. Response
        /// message is deleted from the queue after its corresponding callback
        /// is invoked.
        /// </summary>
        /// <remarks>
        /// Notice: (azure burst) According to test, some responses are
        /// invisible, but Begin/EndGetMessages doesn't get them because of the
        /// GetMessages transaction timed out. So use a small timeout here,
        /// broker can get the response message next time.
        /// </remarks>
        private static readonly TimeSpan ResponseVisibleTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Default retry policy for response storage operation.
        /// </summary>
        private static readonly IRetryPolicy RetryPolicyForResponseStorage
            = new ExponentialRetry(TimeSpan.FromSeconds(3), 3);

        /// <summary>
        /// It is a flag indicating if the current manager starts.
        /// </summary>
        private int start;

        /// <summary>
        /// Instance of the MessageRetriever.
        /// </summary>
        private MessageRetriever retriever;

        /// <summary>
        /// Response storage name
        /// </summary>
        private string responseStorageName;

        /// <summary>
        /// Azure queue client instance.
        /// </summary>
        private CloudQueueClient queueClient;

        /// <summary>
        /// Azure blob client instance.
        /// </summary>
        private CloudBlobClient blobClient;

        /// <summary>
        /// Response queue is per cluster per session, so broker only needs to
        /// handle one response queue.
        /// </summary>
        private CloudQueue responseQueue;

        /// <summary>
        /// Response container is counterpart of the response queue, it is also
        /// per cluster per session.
        /// </summary>
        private CloudBlobContainer responseContainer;

        /// <summary>
        /// Storage client assesses the response queue or blob if necessary.
        /// </summary>
        private AzureStorageClient responseStorageClient;

        /// <summary>
        /// Local cache for the response message. Message retriever gets
        /// responses from queue and put them in this cache.
        /// </summary>
        private ConcurrentQueue<IEnumerable<CloudQueueMessage>> responseCache
            = new ConcurrentQueue<IEnumerable<CloudQueueMessage>>();

        /// <summary>
        /// It controls the response messages in local cache.
        /// </summary>
        private Semaphore semaphoreForResponse;

        /// <summary>
        /// It controls the worker for processing response messages.
        /// </summary>
        private Semaphore semaphoreForWorker;

        /// <summary>
        /// It is a worker task to get response messages from local cache and
        /// then queue work item in thread pool to process response messages.
        /// </summary>
        private Task messageProcessorTask;

        /// <summary>
        /// It is a token indicating if want to stop the task.
        /// </summary>
        private CancellationTokenSource taskCancellation = new CancellationTokenSource();

        /// <summary>
        /// Instance of AzureQueueManager.
        /// </summary>
        private AzureQueueManager azureQueueManager;

        /// <summary>
        /// Storage connection string.
        /// </summary>
        private string storageConnectionString;

        /// <summary>
        /// Session Id.
        /// </summary>
        private string sessionId;

        /// <summary>
        /// Initializes a new instance of the ResponseQueueManager class.
        /// </summary>
        /// <param name="azureQueueManager">AzureQueueManager instance</param>
        /// <param name="sessionId">session Id</param>
        /// <param name="responseStorageName">response storage name</param>
        /// <param name="storageConnectionString">storage connection string</param>
        public ResponseQueueManager(AzureQueueManager azureQueueManager, string sessionId, string responseStorageName, string storageConnectionString)
        {
            this.azureQueueManager = azureQueueManager;

            this.sessionId = sessionId;

            this.responseStorageName = responseStorageName;

            this.storageConnectionString = storageConnectionString;
        }

        /// <summary>
        /// Create message retriever to monitor the response queue and gets
        /// messages from it.
        /// </summary>
        public void Start()
        {
            BrokerTracing.TraceVerbose("[ResponseQueueManager].Start: Try to create response storage.");

            // Make sure response storage is created, the invoker will send
            // request messages when this method returns. Following method
            // is thread safe.
            this.CreateResponseStorage();

            if (Interlocked.CompareExchange(ref this.start, 1, 0) == 0)
            {
                BrokerTracing.TraceVerbose(
                    "[ResponseQueueManager].Start, {0}", this.responseStorageName);

                this.semaphoreForResponse = new Semaphore(0, int.MaxValue);

                this.semaphoreForWorker = new Semaphore(MessageProcessWorker, MessageProcessWorker);

                this.messageProcessorTask =
                    Task.Factory.StartNew(
                        () => this.MessageProcessor(this.taskCancellation.Token),
                        this.taskCancellation.Token);

                this.responseStorageClient = new AzureStorageClient(this.responseQueue, this.responseContainer);

                this.retriever =
                    new MessageRetriever(
                        this.responseQueue,
                        RetrieverConcurrency,
                        ResponseVisibleTimeout,
                        this.HandleMessages,
                        this.HandleInvalidResponseQueue);

                this.retriever.Start();
            }
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void DisposeInternal()
        {
            base.DisposeInternal();

            // this only happens when unload broker worker.
            if (this.retriever != null)
            {
                try
                {
                    this.retriever.Close();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[ResponseQueueManager].Dispose: Closing message retriever queue failed, {0}, {1}",
                        e,
                        this.responseStorageName);
                }

                this.retriever = null;
            }

            if (this.responseQueue != null)
            {
                try
                {
                    this.responseQueue.Delete();

                    BrokerTracing.EtwTrace.LogQueueDeleted(this.sessionId, this.responseQueue.Name);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[ResponseQueueManager].Dispose: Deleting response queue failed, {0}, {1}",
                        e,
                        this.responseStorageName);
                }

                this.responseQueue = null;
            }

            if (this.responseContainer != null)
            {
                try
                {
                    this.responseContainer.Delete();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[ResponseQueueManager].Dispose: Deleting response container failed, {0}, {1}",
                        e,
                        this.responseStorageName);
                }

                this.responseContainer = null;
            }

            if (this.messageProcessorTask != null)
            {
                if (this.taskCancellation != null)
                {
                    try
                    {
                        using (this.taskCancellation)
                        {
                            this.taskCancellation.Cancel();

                            this.messageProcessorTask.Wait(WaitTimeForTaskInMillisecond, this.taskCancellation.Token);
                        }
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceWarning(
                            "[ResponseQueueManager].Dispose: Cancelling messageProcessorTask failed, {0}, {1}",
                            e,
                            this.responseStorageName);
                    }
                }

                try
                {
                    this.messageProcessorTask.Dispose();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[ResponseQueueManager].Dispose: Disposing messageProcessorTask failed, {0}, {1}",
                        e,
                        this.responseStorageName);
                }

                this.messageProcessorTask = null;
            }

            if (this.semaphoreForResponse != null)
            {
                try
                {
                    this.semaphoreForResponse.Dispose();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[ResponseQueueManager].Dispose: Disposing semaphoreForResponse failed, {0}, {1}",
                        e,
                        this.responseStorageName);
                }

                this.semaphoreForResponse = null;
            }

            if (this.semaphoreForWorker != null)
            {
                try
                {
                    this.semaphoreForWorker.Dispose();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[ResponseQueueManager].Dispose: Disposing semaphoreForWorker failed, {0}, {1}",
                        e,
                        this.responseStorageName);
                }

                this.semaphoreForWorker = null;
            }

            if (this.responseStorageClient != null)
            {
                try
                {
                    this.responseStorageClient.Dispose();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[ResponseQueueManager].Dispose: Disposing responseStorageClient failed, {0}, {1}",
                        e,
                        this.responseStorageName);
                }

                this.responseStorageClient = null;
            }
        }

        /// <summary>
        /// Create response queue/blob container for the specified session.
        /// </summary>
        private void CreateResponseStorage()
        {
            this.CreateStorageClient(RetryPolicyForResponseStorage);

            if (this.responseQueue == null)
            {
                BrokerTracing.TraceVerbose(
                    "[ResponseQueueManager].CreateResponseStorage: Try to create the response queue {0}",
                    this.responseStorageName);

                CloudQueue queue = this.queueClient.GetQueueReference(this.responseStorageName);

                AzureQueueManager.CreateQueueWithRetry(queue);

                if (Interlocked.CompareExchange<CloudQueue>(ref this.responseQueue, queue, null) == null)
                {
                    BrokerTracing.EtwTrace.LogQueueCreatedOrExist(this.sessionId, this.responseStorageName);
                }
            }

            if (this.responseContainer == null)
            {
                BrokerTracing.TraceVerbose(
                    "[ResponseQueueManager].CreateResponseStorage: Try to create the response container {0}",
                    this.responseStorageName);

                CloudBlobContainer container = this.blobClient.GetContainerReference(this.responseStorageName);

                AzureQueueManager.CreateContainerWithRetry(container);

                Interlocked.CompareExchange<CloudBlobContainer>(ref this.responseContainer, container, null);
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
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.storageConnectionString);

                if (this.queueClient == null)
                {
                    BrokerTracing.TraceVerbose(
                        "[ResponseQueueManager].CreateStorageClient: Try to create the response queue client {0}",
                        this.responseStorageName);

                    CloudQueueClient tmpQueueClient = storageAccount.CreateCloudQueueClient();

                    tmpQueueClient.DefaultRequestOptions.RetryPolicy = retryPolicy;

                    Interlocked.CompareExchange<CloudQueueClient>(ref this.queueClient, tmpQueueClient, null);
                }

                if (this.blobClient == null)
                {
                    BrokerTracing.TraceVerbose(
                        "[ResponseQueueManager].CreateStorageClient: Try to create the response container client {0}",
                        this.responseStorageName);

                    CloudBlobClient tmpBlobClient = storageAccount.CreateCloudBlobClient();

                    tmpBlobClient.DefaultRequestOptions.RetryPolicy = retryPolicy;

                    Interlocked.CompareExchange<CloudBlobClient>(ref this.blobClient, tmpBlobClient, null);
                }
            }
        }

        /// <summary>
        /// Get requests from the local cache and spawn a thread to process
        /// them.
        /// </summary>
        /// <param name="token">task cancellation token</param>
        private void MessageProcessor(CancellationToken token)
        {
            try
            {
                int retry = 0;

                while (!token.IsCancellationRequested && retry < MessageProcessorRetryLimit)
                {
                    try
                    {
                        BrokerTracing.TraceVerbose(
                            "[ResponseQueueManager].MessageProcessor: Wait for response, {0}",
                            this.responseStorageName);

                        this.semaphoreForResponse.WaitOne();

                        IEnumerable<CloudQueueMessage> messages;

                        BrokerTracing.TraceVerbose(
                            "[ResponseQueueManager].MessageProcessor: Try to get response, {0}",
                            this.responseStorageName);

                        if (this.responseCache.TryDequeue(out messages))
                        {
                            BrokerTracing.TraceVerbose(
                                "[ResponseQueueManager].MessageProcessor: Get {0} responses from responseCache and wait for worker, {1}",
                                messages.Count<CloudQueueMessage>(),
                                this.responseStorageName);

                            this.semaphoreForWorker.WaitOne();

                            BrokerTracing.TraceVerbose(
                                "[ResponseQueueManager].MessageProcessor: Queue user work item to process responses, {0}",
                                this.responseStorageName);

                            ThreadPool.QueueUserWorkItem((s) => { this.ProcessMessages(messages); });
                        }
                        else
                        {
                            BrokerTracing.TraceWarning(
                                "[ResponseQueueManager].MessageProcessor: Can not get response from responseCache, {0}",
                                this.responseStorageName);
                        }
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[ResponseQueueManager].MessageProcessor: Error occurs, {0}, {1}, retry count {2}",
                            e,
                            this.responseStorageName,
                            retry);

                        retry++;
                    }
                }
            }
            catch (Exception e)
            {
                // Error here is not expected, in case it happens, log the
                // trace. Current ResponseQueueManager stops.
                BrokerTracing.TraceError(
                    "[ResponseQueueManager].MessageProcessor: Error occurs, {0}, {1}", e, this.responseStorageName);
            }
        }

        /// <summary>
        /// Callback method of the message retriever.
        /// </summary>
        /// <param name="messages">a collection of messages</param>
        private void HandleMessages(IEnumerable<CloudQueueMessage> messages)
        {
            try
            {
                BrokerTracing.TraceVerbose(
                    "[ResponseQueueManager].HandleMessages: Get {0} messages from the queue, {1}",
                    messages.Count<CloudQueueMessage>(),
                    this.responseStorageName);

                this.responseCache.Enqueue(messages);

                this.semaphoreForResponse.Release();
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[ResponseQueueManager].HandleMessages: Error occurs, {0}, {1}", e, this.responseStorageName);
            }
        }

        /// <summary>
        /// Callback method of the message retriever.
        /// </summary>
        /// <param name="messages">a collection of messages</param>
        private void ProcessMessages(IEnumerable<CloudQueueMessage> messages)
        {
            try
            {
                BrokerTracing.TraceVerbose(
                    "[ResponseQueueManager].ProcessMessages: Process {0} messages, {1}",
                    messages.Count<CloudQueueMessage>(),
                    this.responseStorageName);

                messages.AsParallel<CloudQueueMessage>().ForAll<CloudQueueMessage>(
                (message) =>
                {
                    try
                    {
                        Message response = this.responseStorageClient.GetWcfMessageFromQueueMessage(message);

                        UniqueId messageId = this.azureQueueManager.ResponseCallback(message, response);

                        this.responseStorageClient.DeleteMessageAsync(message, messageId);
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[ResponseQueueManager].ProcessMessages: Error occurs for queue message {0}, {1}, {2}",
                            message.Id,
                            e,
                            this.responseStorageName);
                    }
                });
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceError(
                    "[ResponseQueueManager].ProcessMessages: Error occurs, {0}, {1}",
                    ex,
                    this.responseStorageName);
            }
            finally
            {
                this.semaphoreForWorker.Release();

                BrokerTracing.TraceVerbose(
                    "[ResponseQueueManager].ProcessMessages: Release semaphoreForWorker");
            }
        }

        /// <summary>
        /// Handle the invalid response queue.
        /// </summary>
        /// <param name="e">
        /// exception occurred when access the response queue
        /// </param>
        private void HandleInvalidResponseQueue(StorageException e)
        {
            BrokerTracing.TraceWarning(
                "[ResponseQueueManager].HandleInvalidResponseQueue: Exception occurs when access response queue, {0}, {1}, {2}",
                BurstUtility.GetStorageErrorCode(e),
                e,
                this.responseStorageName);

            if (BurstUtility.IsQueueNotFound(e))
            {
                // Current method is called once, so only have following trace once.
                BrokerTracing.EtwTrace.LogQueueNotExist(this.sessionId, this.responseStorageName);

                this.azureQueueManager.TriggerCallbackForInvalidResponseQueue(
                    new ResponseStorageException(e, this.responseStorageName));
            }
        }
    }
}
