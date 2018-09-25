//-----------------------------------------------------------------------
// <copyright file="AzureQueueManager.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     It is the manager to control both request and response queue.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// It is the manager to control both request and response queue.
    /// </summary>
    internal class AzureQueueProxy : DisposableObject, IResponseServiceCallback
    {
        /// <summary>
        /// The blocking wait time interval for new requests to receive
        /// </summary>
        private const int RequestWaitIntervalMs = 5 * 60 * 1000;

        /// <summary>
        /// It is wait time for messageProcessorTask to exit.
        /// </summary>
        private const int WaitTimeForTaskInMillisecond = 3 * 1000;

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
        /// Cluster Hash from the cluster Id
        /// </summary>
        private int clusterHash;

        /// <summary>
        /// Cluster Hash from the cluster Id
        /// </summary>
        public int ClusterHash
        {
            get { return clusterHash; }
            set { clusterHash = value; }
        }

        /// <summary>
        /// Azure queue client instance.
        /// </summary>
        private CloudQueueClient queueClient;

        /// <summary>
        /// Azure blob client instance.
        /// </summary>
        private CloudBlobClient blobClient;

        /// <summary>
        /// Session Id.
        /// </summary>
        private int sessionId;

        /// <summary>
        /// Azure storage connection string
        /// </summary>
        private string azureStorageConnectionString;

        /// <summary>
        /// the request queue/blob container name
        /// </summary>
        private string requestStorageName;
        
        /// <summary>
        /// the request Azure queue
        /// </summary>
        private CloudQueue requestQueue;

        /// <summary>
        /// the request Azure blob container;
        /// </summary>
        private CloudBlobContainer requestBlobContainer;

        /// <summary>
        /// the local request queue
        /// </summary>
        //ConcurrentQueue<Message> requestMessageQueue = new ConcurrentQueue<Message>();
        BlockingCollection<Message> requestMessageQueue = new BlockingCollection<Message>();

        /// <summary>
        /// the local response queue
        /// </summary>
        ConcurrentQueue<Message> responseMessageQueue = new ConcurrentQueue<Message>();

        /// <summary>
        /// the dictionary for response clients
        /// key:session hash
        /// value:local response queue and the Azure storage client
        /// </summary>
        Dictionary<int, Tuple<ConcurrentQueue<Message>, AzureStorageClient>> responseMessageClients = new Dictionary<int,Tuple<ConcurrentQueue<Message>,AzureStorageClient>>();
        
        /// <summary>
        /// the dictionary for callback id
        /// key:callback id
        /// value:session hash
        /// </summary>
        Dictionary<string, int> sessionClientData = new Dictionary<string, int>();

        /// <summary>
        /// lock for syncing dictionaries
        /// </summary>
        private object dicSyncLock = new object();
        
        /// <summary>
        /// the request storage client
        /// </summary>
        private AzureStorageClient requestStorageClient;

        /// <summary>
        /// the response storage client
        /// </summary>
        private AzureStorageClient responseStorageClient;

        /// <summary>
        /// the message sender
        /// </summary>
        private MessageSender messageSender;

        /// <summary>
        /// the message retriever;
        /// </summary>
        private MessageRetriever messageRetriever;

        /// <summary>
        /// the request queue SAS Uri
        /// </summary>
        private string requestQueueUri;

        /// <summary>
        /// the request queue SAS Uri
        /// </summary>
        public string RequestQueueUri
        {
            get { return requestQueueUri; }
            set { requestQueueUri = value; }
        }

        /// <summary>
        /// the request blob container SAS Uri
        /// </summary>
        private string requestBlobUri;

        /// <summary>
        /// the request blob container SAS Uri
        /// </summary>
        public string RequestBlobUri
        {
            get { return requestBlobUri; }
            set { requestBlobUri = value; }
        }

        /// <summary>
        /// the dictionary for response storage SAS Uris
        /// key:session hash
        /// value:queue SAS Uri and blob container SAS Uri
        /// </summary>
        private Dictionary<int, Tuple<string, string>> responseClientUris = new Dictionary<int, Tuple<string, string>>();

        /// <summary>
        /// the dictionary for response storage SAS Uris
        /// key:session hash
        /// value:queue SAS Uri and blob container SAS Uri
        /// </summary>
        public Dictionary<int, Tuple<string, string>> ResponseClientUris
        {
            get { return responseClientUris; }
            set { responseClientUris = value; }
        }

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
        private ConcurrentQueue<IEnumerable<CloudQueueMessage>> requestCache
            = new ConcurrentQueue<IEnumerable<CloudQueueMessage>>();

        /// <summary>
        /// It controls the request messages in local cache.
        /// </summary>
        private Semaphore semaphoreForRequest;

        /// <summary>
        /// It controls the worker for processing requests.
        /// </summary>
        private Semaphore semaphoreForWorker;

        /// <summary>
        /// Cancel tasks TODO: check if it works 
        /// </summary>
        private CancellationTokenSource taskCancellation = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the AzureQueueManager 
        /// </summary>
        /// <param name="clusterName">the cluster name</param>
        /// <param name="clusterHash">the cluster id hash</param>
        /// <param name="sessionId">the session id</param>
        /// <param name="azureStorageConnectionString">the Azure storage connection string</param>
        public AzureQueueProxy(string clusterName, int clusterHash, int sessionId, string azureStorageConnectionString)
        {
            // this works for broker
            ThreadPool.SetMinThreads(MinThreadsOfThreadpool, MinThreadsOfThreadpool);

            // improve http performance for Azure storage queue traffic
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            this.clusterName = clusterName;
            this.sessionId = sessionId;
            this.clusterHash = clusterHash;

            this.azureStorageConnectionString = azureStorageConnectionString;

            // build the request and response queue
            this.requestStorageName = SoaHelper.GetRequestStorageName(clusterHash, sessionId);
            // this.responseStorageNamePrefix = SoaHelper.GetResponseStorageName(clusterId, sessionId);

            // exponential retry
            this.CreateStorageClient(DefaultRetryPolicy);

            this.requestQueue = this.queueClient.GetQueueReference(requestStorageName);
            CreateQueueWithRetry(this.requestQueue);
            this.requestBlobContainer = this.blobClient.GetContainerReference(requestStorageName);
            CreateContainerWithRetry(this.requestBlobContainer);

            //generate the SAS token for the queue and blob container

            SharedAccessQueuePolicy queuePolicy = new SharedAccessQueuePolicy() { Permissions = SharedAccessQueuePermissions.Add, SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7) };
            this.requestQueueUri = string.Join(string.Empty, this.requestQueue.Uri, this.requestQueue.GetSharedAccessSignature(queuePolicy));

            SharedAccessBlobPolicy blobPolicy = new SharedAccessBlobPolicy() { Permissions = SharedAccessBlobPermissions.Write, SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7) };
            this.requestBlobUri = string.Join(string.Empty, this.requestBlobContainer.Uri, this.requestBlobContainer.GetSharedAccessSignature(blobPolicy));

            this.requestStorageClient = new AzureStorageClient(this.requestQueue, this.requestBlobContainer);
            //this.responseStorageClient = new AzureStorageClient(this.responseQueue, this.responseBlobContainer);

            // initialize sender and retriever
            this.messageSender = new MessageSender(this.responseMessageClients, SenderConcurrency);
            this.messageRetriever = new MessageRetriever(this.requestStorageClient.Queue, RetrieverConcurrency, DefaultVisibleTimeout, this.HandleMessages, null);

            this.receiveRequest = this.ReceiveRequest;
        }

        /// <summary>
        /// open the proxy
        /// </summary>
        public void Open()
        {
            ThreadPool.QueueUserWorkItem(this.InternalStart);
        }
        
        /// <summary>
        /// Start the proxy. Proxy waits for the request queue to be created,
        /// and processes request messages.
        /// </summary>
        /// <param name="state">state object</param>
        private void InternalStart(object state)
        {
            //ThreadPool.SetMinThreads(MinThreadsOfThreadpool, MinThreadsOfThreadpool);

            this.semaphoreForRequest = new Semaphore(0, int.MaxValue);
            this.semaphoreForWorker = new Semaphore(MessageProcessWorker, MessageProcessWorker);
            
            this.messageRetriever.Start();

            CancellationToken token = this.taskCancellation.Token;

            this.messageProcessorTask = new Task(this.MessageProcessor, token, token);
            this.messageProcessorTask.Start();

            return;
        }


        /// <summary>
        /// Cleanup current object.
        /// </summary>
        private void Cleanup()
        {
            if (this.messageSender != null)
            {
                try
                {
                    this.messageSender.Close();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Closing message retriever failed, {0}", e)));
                }

                this.messageSender = null;
            }

            if (this.messageRetriever != null)
            {
                try
                {
                    this.messageRetriever.Close();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Closing message retrier failed, {0}", e)));
                }

                this.messageRetriever = null;
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

                            // no need to wait here for the task could be waiting for the semaphore
                            //this.messageProcessorTask.Wait(WaitTimeForTaskInMillisecond, this.taskCancellation.Token);
                        }
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Cancelling messageProcessorTask failed, {0}", e)));
                    }
                }

                try
                {
                    // the Task cannot be disposed when it is not in a cmpletion state
                    // this.messageProcessorTask.Dispose();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Disposing message processor task failed, {0}", e)));
                }

                this.messageProcessorTask = null;
            }

            if (this.semaphoreForRequest != null)
            {
                try
                {
                    this.semaphoreForRequest.Dispose();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Disposing semaphoreForRequest failed, {0}", e)));
                }

                this.semaphoreForRequest = null;
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
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Disposing semaphoreForWorker failed, {0}", e)));
                }

                this.semaphoreForWorker = null;
            }

            if (this.requestStorageClient != null)
            {
                try
                {
                    this.requestStorageClient.Close();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Disposing requestStorageClient failed, {0}", e)));
                }

                this.requestStorageClient = null;
            }

            if (this.responseStorageClient != null)
            {
                try
                {
                    this.responseStorageClient.Close();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Cleanup",
                            string.Format("Disposing responseStorageClient failed, {0}", e)));
                }

                this.responseStorageClient = null;
            }

            this.requestMessageQueue?.Dispose();

        }

        /// <summary>
        /// Callback method of the message retriever.
        /// </summary>
        /// <param name="messages">a collection of messages</param>
        private void HandleMessages(IEnumerable<CloudQueueMessage> messages)
        {
            try
            {
                this.requestCache.Enqueue(messages);
                this.semaphoreForRequest.Release();
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
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
        private void MessageProcessor(object t)
        {
            try
            {
                CancellationToken token = (CancellationToken)t;
                while (!token.IsCancellationRequested)
                {
                    this.semaphoreForRequest.WaitOne();

                    IEnumerable<CloudQueueMessage> messages;

                    if (this.requestCache.TryDequeue(out messages))
                    {
                        this.semaphoreForWorker.WaitOne();

                        ThreadPool.QueueUserWorkItem((s) => { this.ProcessMessages(messages); });
                    }
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
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
            BrokerTracing.TraceInfo(
                SoaHelper.CreateTraceMessage(
                    "Proxy",
                    "ProcessMessages",
                    string.Format("Process {0} messages.", messages.Count<CloudQueueMessage>())));

            messages.AsParallel<CloudQueueMessage>().ForAll<CloudQueueMessage>(
            (requestQueueMessage) =>
            {
                
                Message request = null;

                try
                {
                    BrokerTracing.TraceInfo(
                        string.Format("SOA broker proxy perf1 - {0}", DateTime.UtcNow.TimeOfDay.TotalSeconds));

                    request = this.requestStorageClient.GetWcfMessageFromQueueMessage(requestQueueMessage);

                    this.requestMessageQueue.Add(request);
                    //this.requestMessageQueue.Enqueue(request);

                    UniqueId messageId = SoaHelper.GetMessageId(request);

                    BrokerTracing.TraceInfo(
                        SoaHelper.CreateTraceMessage(
                            "Proxy",
                            "Request received inqueue",
                            string.Empty,
                            messageId,
                            "Request message in queue"));

                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "Proxy", "ProcessMessages", string.Format("Error occurs {0}", e)));

                }
            });

            this.semaphoreForWorker.Release();
        }

        public Message ReceiveRequest()
        {
            BrokerTracing.TraceInfo("[AzureQueueProxy] ReceiveRequest is called");
            Message request = null;
            try
            {
                while (!requestMessageQueue.TryTake(out request, RequestWaitIntervalMs))
                {
                    BrokerTracing.TraceInfo("[AzureQueueProxy] No message in the request message queue. Block wait.");
                }
            }
            catch (ObjectDisposedException)
            {
                BrokerTracing.TraceWarning("[AzureQueueProxy] requestMessageQueue is disposed.");
                return null;
            }
            //while (!requestMessageQueue.TryDequeue(out request))
            //{
            //    BrokerTracing.TraceInfo("[AzureQueueProxy] No message in the request message queue. Sleep wait.");
            //    Thread.Sleep(RequestWaitIntervalMs);
            //}
            BrokerTracing.TraceInfo("[AzureQueueProxy] Request is dequeued {0}", request.Version.ToString());
            return request;
        }

        public delegate Message ReceiveRequestDelegate();

        private ReceiveRequestDelegate receiveRequest;
        
        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            IAsyncResult ar = receiveRequest.BeginInvoke(callback, state);
            //BrokerTracing.TraceInfo("[AzureQueueProxy] BeginReceiveRequest ar.CompletedSynchronously {0}", ar.CompletedSynchronously);
            return ar;
        }

        public Message EndReceiveRequest(IAsyncResult result)
        {
            return receiveRequest.EndInvoke(result);
        }

        public void SendResponse(Message response)
        {
            BrokerTracing.TraceInfo("[AzureQueueProxy] Response is inqueued {0}", response.Version.ToString());

            // for azure queue need to copy a new message
            MessageBuffer messageBuffer = null;
            messageBuffer = response.CreateBufferedCopy(int.MaxValue);
            Message m = messageBuffer.CreateMessage();
            
            this.responseMessageQueue.Enqueue(m);
        }

        public void SendResponse(Message response, string clientData)
        {
            BrokerTracing.TraceInfo("[AzureQueueProxy] Response is inqueued with clientData {0}", response.Version.ToString());

            // for azure queue need to copy a new message
            MessageBuffer messageBuffer = null;
            messageBuffer = response.CreateBufferedCopy(int.MaxValue);
            Message m = messageBuffer.CreateMessage();

            this.responseMessageClients[this.sessionClientData[clientData]].Item1.Enqueue(m);
        }


        public void AddResponseQueues(string clientData, int sessionHash)
        {
            lock (this.dicSyncLock)
            {
                if (!this.responseMessageClients.Keys.Contains(sessionHash))
                {
                    string responseStorageName = SoaHelper.GetResponseStorageName(this.clusterHash, this.sessionId, sessionHash);
                    CloudQueue responseQueue = this.queueClient.GetQueueReference(responseStorageName);
                    CreateQueueWithRetry(responseQueue);
                    // use default retry option for the cloud queue
                    CloudBlobContainer responseBlobContainer = this.blobClient.GetContainerReference(responseStorageName);
                    CreateContainerWithRetry(responseBlobContainer);
                    // use default retry option for the cloud blob
                    SharedAccessQueuePolicy queuePolicy = new SharedAccessQueuePolicy() { Permissions = SharedAccessQueuePermissions.ProcessMessages, SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7) };
                    string responseQueueUri = string.Join(string.Empty, responseQueue.Uri, responseQueue.GetSharedAccessSignature(queuePolicy));

                    SharedAccessBlobPolicy blobPolicy = new SharedAccessBlobPolicy() { Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Delete, SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7) };
                    string responseBlobUri = string.Join(string.Empty, responseBlobContainer.Uri, responseBlobContainer.GetSharedAccessSignature(blobPolicy));


                    this.responseMessageClients.Add(sessionHash, Tuple.Create(new ConcurrentQueue<Message>(), new AzureStorageClient(responseQueue, responseBlobContainer)));
                    this.responseClientUris.Add(sessionHash, Tuple.Create(responseQueueUri, responseBlobUri));
                }

                if (!this.sessionClientData.Keys.Contains(clientData))
                {
                    this.sessionClientData.Add(clientData, sessionHash);
                }
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
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.Cleanup();
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
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.azureStorageConnectionString);

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
                
        void IResponseServiceCallback.Close()
        {
            throw new NotImplementedException();
        }

        // to be impplemented.
        void IResponseServiceCallback.SendBrokerDownSignal(bool isBrokerNodeDown)
        {
            throw new NotImplementedException();
        }

        
    }
}
