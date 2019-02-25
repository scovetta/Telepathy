//-----------------------------------------------------------------------
// <copyright file="AzureQueueManager.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     It is the manager to control both request and response queue.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    /// <summary>
    /// It is the manager to control both request and response queue.
    /// </summary>
    internal class AzureQueueProxy : DisposableObject
    {
        private int sendConcurrencyLevel;

        private int queueAssignIndex = -1;

        private readonly ConcurrentDictionary<string, ConcurrentQueue<Message>> ClientIdToQueueMapping = new ConcurrentDictionary<string, ConcurrentQueue<Message>>();

        /// <summary>
        /// The wait time interval for new requests
        /// </summary>
        private const int RequestWaitIntervalMs = 100;

        /// <summary>
        /// Default retry policy for request storage operation.
        /// </summary>
        /// <remarks>
        /// Notice: Use NoRetry for request storage because of "at most once" semantic.
        /// </remarks>
        private static readonly IRetryPolicy RetryPolicyForRequestStorage = new NoRetry();

        /// <summary>
        /// Cluster name.
        /// </summary>
        private string clusterName;

        /// <summary>
        /// Session Id.
        /// </summary>
        private int sessionId;

        /// <summary>
        /// Session hash code
        /// </summary>
        private int sessionHash;

        /// <summary>
        /// Get or set session hash code
        /// </summary>
        public int SessionHash
        {
            get { return sessionHash; }
            set { sessionHash = value; }
        }

        private CloudQueue[] requestQueues;
        private CloudBlobContainer requestBlobContainer;

        private CloudQueue responseQueue;
        private CloudBlobContainer responseBlobContainer;

        // request message queue
        private ConcurrentQueue<Message>[] requestMessageQueues = null; //new ConcurrentQueue<Message>();

        // response message queue
        ConcurrentQueue<Message> responseMessageQueue = new ConcurrentQueue<Message>();

        // multiple concurrent queue for clientData and action
        Dictionary<Tuple<string>, ConcurrentQueue<Message>> responseMessageQueues = new Dictionary<Tuple<string>, ConcurrentQueue<Message>>();

        private AzureStorageClient[] requestStorageClients;
        private AzureStorageClient responseStorageClient;

        private MessageSender[] messageSenders;
        private MessageRetriever messageRetriever;

        /// <summary>
        /// It is the minimum count of the threadpool thread.
        /// </summary>
        private const int MinThreadsOfThreadpool = 64;

        /// <summary>
        /// The upper limit concurrency of CloudQueue.BeginGetMessages.
        /// </summary>
        /// <remarks>
        /// According to test, should not use large concurrency value. Given
        /// that there are several proxy roles, use a smaller value than the
        /// value used by on-premise AzureQueueManager.
        /// </remarks>
        private const int RetrieverConcurrency = 16;

        /// <summary>
        /// The upper limit concurrency of adding response messages to the
        /// response queue.
        /// </summary>
        private const int SenderConcurrency = 1000;

        /// <summary>
        /// The upper limit of the thread pool threads processing messages.
        /// </summary>
        private const int MessageProcessWorker = 64;

        /// <summary>
        /// Default visible timeout of the calculating messages.
        /// </summary>
        private static readonly TimeSpan DefaultVisibleTimeout = TimeSpan.FromDays(7);

        /// <summary>
        /// Default retry policy for the storage operation.
        /// </summary>
        private static readonly IRetryPolicy DefaultRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), 3);

        /// <summary>
        /// It is a worker thread to get messages from local cache and process
        /// them.
        /// </summary>
        private Task messageProcessorTask;

        /// <summary>
        /// Local cache for the request message. Message retriever gets
        /// requests from queue and put them in this cache.
        /// </summary>
        private ConcurrentQueue<IEnumerable<CloudQueueMessage>> responseCache
            = new ConcurrentQueue<IEnumerable<CloudQueueMessage>>();

        /// <summary>
        /// It controls the request messages in local cache.
        /// </summary>
        private Semaphore semaphoreForResponse;

        /// <summary>
        /// It controls the worker for processing requests.
        /// </summary>
        private Semaphore semaphoreForWorker;

        private bool responseClientInitialized = false;

        public bool IsResponseClientInitialized
        {
            get { return responseClientInitialized; }
        }

        private object responseClientLock = new object();

        /// <summary>
        /// Initializes a new instance of the AzureQueueManager class.
        /// </summary>
        /// <param name="sessionId">session Id</param>
        public AzureQueueProxy(string clusterName, int sessionId, int sessionHash, string[] azureRequestQueueUris, string azureRequestBlobUri)
        {
            this.sendConcurrencyLevel = azureRequestQueueUris.Length;
            this.requestMessageQueues = Enumerable.Range(0, this.sendConcurrencyLevel).Select(_ => new ConcurrentQueue<Message>()).ToArray();

            // this doesn't work for client.
            // ThreadPool.SetMinThreads(MinThreadsOfThreadpool, MinThreadsOfThreadpool);

            // improve http performance for Azure storage queue traffic
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            this.clusterName = clusterName;
            this.sessionId = sessionId;
            this.sessionHash = sessionHash;

            this.requestQueues = azureRequestQueueUris.Select(u => new CloudQueue(new Uri(u))).ToArray();

            // use default retry option
            this.requestBlobContainer = new CloudBlobContainer(new Uri(azureRequestBlobUri));

            // use default retry option
            this.requestStorageClients = this.requestQueues.Select(q => new AzureStorageClient(q, this.requestBlobContainer)).ToArray();

            // initialize the sender
            this.messageSenders = new MessageSender[this.sendConcurrencyLevel];
            for (int i = 0; i != this.sendConcurrencyLevel; i++)
            {
                this.messageSenders[i] = new MessageSender(this.requestMessageQueues[i], this.requestStorageClients[i], SenderConcurrency/this.sendConcurrencyLevel);
            }
        }

        /// <summary>
        /// Initial the response client 
        /// </summary>
        /// <param name="azureResponseQueueUri">The response queue SAS Uri</param>
        /// <param name="azureResponseBlobUri">The response blob container SAS Uri</param>
        public void InitResponseClient(string azureResponseQueueUri, string azureResponseBlobUri)
        {
            if (!this.responseClientInitialized)
            {
                lock (this.responseClientLock)
                {
                    if (!this.responseClientInitialized)
                    {
                        this.responseQueue = new CloudQueue(new Uri(azureResponseQueueUri));
                        // exponential retry
                        this.responseQueue.ServiceClient.DefaultRequestOptions.RetryPolicy = DefaultRetryPolicy;

                        this.responseBlobContainer = new CloudBlobContainer(new Uri(azureResponseBlobUri));
                        // exponential retry
                        this.responseBlobContainer.ServiceClient.DefaultRequestOptions.RetryPolicy = DefaultRetryPolicy;

                        this.responseStorageClient = new AzureStorageClient(this.responseQueue, this.responseBlobContainer);
                        this.messageRetriever = new MessageRetriever(this.responseStorageClient.Queue, RetrieverConcurrency, DefaultVisibleTimeout, this.HandleMessages, null);
                        this.receiveResponse = this.ReceiveMessage;

                        this.semaphoreForResponse = new Semaphore(0, int.MaxValue);
                        this.semaphoreForWorker = new Semaphore(MessageProcessWorker, MessageProcessWorker);

                        this.messageRetriever.Start();
                        this.messageProcessorTask = new Task(this.MessageProcessor);
                        this.messageProcessorTask.Start();

                        this.responseClientInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Cleanup current object.
        /// </summary>
        private void Cleanup()
        {
            foreach (var messageSender in this.messageSenders)
            {
                if (messageSender != null)
                {
                    try
                    {
                        messageSender.Close();
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceInformation(
                            SoaHelper.CreateTraceMessage(
                                "Proxy",
                                "Cleanup",
                                string.Format("Closing message retriever failed, {0}", e)));
                    }

                    // this.messageSenders = null;
                }
            }
            

            if (this.messageRetriever != null)
            {
                try
                {
                    this.messageRetriever.Close();
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Closing message retrier failed, {0}", e)));
                }

                this.messageRetriever = null;
            }

            if (this.messageProcessorTask != null)
            {
                try
                {
                    this.messageProcessorTask.Dispose();
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Disposing message processor task failed, {0}", e)));
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
                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Disposing semaphoreForRequest failed, {0}", e)));
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
                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Disposing semaphoreForWorker failed, {0}", e)));
                }

                this.semaphoreForWorker = null;
            }

            foreach (var requestStorageClient in this.requestStorageClients)
            {
                if (requestStorageClient != null)
                {
                    try
                    {
                        requestStorageClient.Close();
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceInformation(
                            SoaHelper.CreateTraceMessage(
                                "Proxy",
                                "Cleanup",
                                string.Format("Disposing semaphoreForWorker failed, {0}", e)));
                    }

                    //requestStorageClients = null;
                }
            }
            

            if (this.responseStorageClient != null)
            {
                try
                {
                    this.responseStorageClient.Close();
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Disposing responseStorageClient failed, {0}", e)));
                }

                this.responseStorageClient = null;
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
                this.responseCache.Enqueue(messages);
                this.semaphoreForResponse.Release();
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceInformation(
                    SoaHelper.CreateTraceMessage(
                        "Proxy",
                        "HandleMessages",
                        string.Format("Error occurs, {0}", e)));
            }
        }

        /// <summary>
        /// Get requests from the local cache and spawn a thread to process
        /// them.
        /// </summary>
        /// <remarks>
        /// Can add throttling logic in the method, but now no need, because
        /// broker proxy doesn't hit limit of CPU, Memory or Network.
        /// </remarks>
        private void MessageProcessor()
        {
            try
            {
                while (true)
                {
                    this.semaphoreForResponse.WaitOne();

                    IEnumerable<CloudQueueMessage> messages;

                    if (this.responseCache.TryDequeue(out messages))
                    {
                        this.semaphoreForWorker.WaitOne();

                        ThreadPool.QueueUserWorkItem((s) => { this.ProcessMessages(messages); });
                    }
                }
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceInformation(
                    SoaHelper.CreateTraceMessage(
                        "Proxy",
                        "MessageProcessor",
                        string.Format("Error occurs, {0}", e)));
            }
        }


        /// <summary>
        /// Process a collection of queue messages.
        /// </summary>
        /// <param name="messages">collection of the queue messages</param>
        private void ProcessMessages(IEnumerable<CloudQueueMessage> messages)
        {
            SessionBase.TraceSource.TraceInformation(
                SoaHelper.CreateTraceMessage(
                    "Proxy",
                    "ProcessMessages",
                    string.Format("Process {0} messages.", messages.Count<CloudQueueMessage>())));

            messages.AsParallel<CloudQueueMessage>().ForAll<CloudQueueMessage>(
            (responseQueueMessage) =>
            {

                Message response = null;

                try
                {
                    SessionBase.TraceSource.TraceInformation(
                        string.Format("SOA broker proxy perf1 - {0}", DateTime.UtcNow.TimeOfDay.TotalSeconds));

                    response = this.responseStorageClient.GetWcfMessageFromQueueMessage(responseQueueMessage);

                    var qkey = Tuple.Create(SoaHelper.GetClientDataHeaderFromMessage(response));

                    if (!this.responseMessageQueues.ContainsKey(qkey))
                    {
                        lock (this.responseMessageQueues)
                        {
                            if (!this.responseMessageQueues.ContainsKey(qkey))
                            {
                                this.responseMessageQueues.Add(qkey, new ConcurrentQueue<Message>());
                            }
                        }
                    }

                    this.responseMessageQueues[qkey].Enqueue(response);

                    UniqueId messageId = SoaHelper.GetMessageId(response);

                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Response received inqueue",
                            string.Empty,
                            messageId,
                            "Response message in queue"));

                    // do not delete the message from the queue here, depends on the long invisable timeout and hourly cleanup
                    // this.responseStorageClient.DeleteMessageAsync(responseQueueMessage, messageId);

                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "Proxy", "ProcessMessages", string.Format("Error occurs {0}", e)));

                }
            });

            this.semaphoreForWorker.Release();
        }

        public Message ReceiveMessage(string clientData)
        {
            var qKey = Tuple.Create(clientData);
            Message message = null;

            while (!this.responseMessageQueues.ContainsKey(qKey) || !this.responseMessageQueues[qKey].TryDequeue(out message))
            {
                Thread.Sleep(RequestWaitIntervalMs);
            }
            return message;
        }

        public delegate Message ReceiveRequestDelegate(string clientData);

        private ReceiveRequestDelegate receiveResponse;

        public IAsyncResult BeginReceiveRequest(string clientData, AsyncCallback callback, object state)
        {
            IAsyncResult ar = receiveResponse.BeginInvoke(clientData, callback, state);
            return ar;
        }

        public Message EndReceiveRequest(IAsyncResult result)
        {
            return receiveResponse.EndInvoke(result);
        }

        public void SendMessage(Message message)
        {
            this.SendMessage(string.Empty, message);
        }

        public void SendMessage(string clientId, Message message)
        {
            var queue = this.ClientIdToQueueMapping.GetOrAdd(
                clientId,
                id =>
                    {
                        int queueIdx = Interlocked.Increment(ref this.queueAssignIndex) % this.sendConcurrencyLevel;
                        return this.requestMessageQueues[queueIdx];
                    });

            queue.Enqueue(message);
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
                    SessionBase.TraceSource.TraceInformation(
                        "[AzureQueueManager].CreateQueueWithRetry: Create the queue {0}", queue.Name);
                }

                return null;
            },
            (e, count) =>
            {
                SessionBase.TraceSource.TraceInformation(
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
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Cleanup();
            }

            base.Dispose(disposing);
        }
    }
}
