// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CcpWSLB.UnitTest")]

namespace Microsoft.Telepathy.ServiceBroker.Persistences.AzureQueuePersist
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;
    using System.Collections.Concurrent;

    using Microsoft.Telepathy.Common;

    public class AzureQueuePersist : ISessionPersist
    {
        /// <summary>the prefix for EOM flag label</summary>
        private const string EOMLabel = "EOM";

        /// <summary>the prefix of the queue path</summary>
        private const string PathPrefix = "HPC";

        private const string PendingPathPrefix = "Pending";

        /// <summary>delimeter for generating queue name</summary>
        private const string QueueNameFieldDelimeter = "-";

        /// <summary>the prefix of the request queue name</summary>
        private const string RequestQueueSuffix = "REQUESTS";

        /// <summary>the prefix of the response queue name</summary>
        private const string ResponseQueueSuffix = "RESPONSES";

        /// <summary>the prefix for persist version label</summary>
        private const string VersionLabel = "VERSION";

        /// <summary>the binary message formattoer</summary>
        private static readonly IFormatter binFormatterField = new BinaryFormatter();

        /// <summary>the broker node name.  For HA cluster, it is the cluster virtual name.</summary>
        private static string BrokerNodeName = BrokerIdentity.GetBrokerName();

        /// <summary>the client id.</summary>
        private readonly string clientIdField;

        /// <summary>a value indicating whether this is a new created AzureStorage persistence.</summary>
        private readonly bool isNewCreatePersistField;

        /// <summary>persist version of this AzureQueue queue</summary>
        private readonly int persistVersion;

        private readonly object responsePutLock = new object();

        private readonly Queue<PutResponseState> responsePutQueue = new Queue<PutResponseState>();

        /// <summary>the session id.</summary>
        private readonly string sessionIdField;

        private readonly int sleepTime = 500;

        private readonly string storageConnectString;

        /// <summary>the user name.</summary>
        private readonly string userNameField;

        /// <summary>the total request count.</summary>
        private long allRequestsCountField;

        private CloudBlobContainer blobContainer;

        /// <summary>flag indicating if all requests have been received.</summary>
        private bool EOMFlag;

        /// <summary>Gets the number of the requests that get the fault responses.</summary>
        private long failedRequestsCountField;

        /// <summary>a value indicating whether the MSMQ persistence is closed.</summary>
        private volatile bool isClosedField;

        private bool isCreatedResponseTask;

        private bool isCreateRequestTask = false;

        private bool isDisposedField;

        private CloudQueue pendingQueueField;

        private AzureQueueRequestFetcher requestFetcher;

        /// <summary>the message queue that store the request messages.</summary>
        private CloudQueue requestQueueField;

        /// <summary>the total requests count in the request queue.</summary>
        private long requestsCountField;

        private AzureQueueResponseFetcher responseFetcher;

        private long responseIndex = int.MaxValue >> 1;

        /// <summary>the total responses count in the queue.</summary>
        private long responsesCountField;

        private CloudTable responseTableField;

        /// <summary>number of requests that are sent to AzureQueue but not committed yet. </summary>
        private long uncommittedRequestsCountField;

        private ConcurrentDictionary<long, bool> storedResponseDic = new ConcurrentDictionary<long, bool>();

        private C5.IntervalHeap<long> priorityQueue = new C5.IntervalHeap<long>();

        private long responseLock = 0;

        private ReaderWriterLockSlim rwlockPriorityQueue = new ReaderWriterLockSlim();

        private long lastReponseIndex = Int32.MaxValue >> 1;

        private ConcurrentDictionary<string, bool> uniqueResponseDic = new ConcurrentDictionary<string, bool>();

#if DEBUG
        private int procCount = 0;
#endif

        internal AzureQueuePersist(string userName, string sessionId, string clientId, string storageConnectString)
        {
            BrokerTracing.TraceVerbose(
                "[AzureQueuePersist] .AzureQueuePersist: constructor. session id = {0}, client id = {1}, user name = {2}",
                sessionId,
                clientId,
                userName);
            Debug.Write($"[AzureQueuePersist].AzureQueuePersist: {Environment.NewLine}{Environment.StackTrace}");
            Debug.Write(
                $"[AzureQueuePersist].AzureQueuePersist: Current principal: {Thread.CurrentPrincipal.Identity.Name}");

            this.sessionIdField = sessionId;
            this.clientIdField = clientId;
            this.userNameField = userName;

            this.storageConnectString = storageConnectString;

            var requestQueueName = MakeQueuePath(sessionId, clientId, true);
            var responseTableName = MakeTablePath(sessionId, clientId);
            var pendingQueueName = MakeQueuePath(sessionId, clientId, false);

            this.isNewCreatePersistField = true;
            try
            {
                var requestQueueExist = AzureStorageTool.ExistsQueue(storageConnectString, requestQueueName)
                    .GetAwaiter().GetResult();
                var responseQueueExist = AzureStorageTool.ExistTable(storageConnectString, responseTableName)
                    .GetAwaiter().GetResult();

                if (requestQueueExist && responseQueueExist)
                {
                    this.isNewCreatePersistField = false;
                }
                else if (requestQueueExist != responseQueueExist)
                {
                    // If there is only request queue but not response queue, it could be caused by:
                    // a. Queue creation operation interrupted
                    // b. Queue deletion operation interrupted
                    if (requestQueueExist)
                    {
                        BrokerTracing.TraceError(
                            "[AzureQueuePersist] .AzureQueuePersist: queue data not integrety.  Fix it by deleting queue = {0}",
                            requestQueueName);
                        AzureStorageTool.DeleteQueueAsync(storageConnectString, sessionId, requestQueueName)
                            .GetAwaiter().GetResult();
                    }
                    else
                    {
                        // There is only response queue but no request queue - this should rarely happen, as we always 
                        // create request queue before response queue, and delete request queue after response queue.
                        BrokerTracing.TraceError(
                            "[AzureQueuePersist] .AzureQueuePersist: queue data not integrety.  Fix it by deleting queue = {0}",
                            responseTableName);
                        AzureStorageTool.DeleteTableAsync(storageConnectString, sessionId, responseTableName)
                            .GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureQueuePersist] .AzureQueuePersist: MessageQueue.Exists raised exception, exception: {0}",
                    e);
                throw new BrokerQueueException((int)BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_FAIL, e.ToString());
            }

            if (this.isNewCreatePersistField)
            {
                try
                {
                    this.pendingQueueField = AzureStorageTool.CreateQueueAsync(
                        this.storageConnectString,
                        sessionId,
                        pendingQueueName,
                        clientId,
                        true,
                        this.FormatRequestQueueLabel()).GetAwaiter().GetResult();

                    this.blobContainer = AzureStorageTool
                        .CreateBlobContainerAsync(this.storageConnectString, requestQueueName).GetAwaiter().GetResult();

                    BrokerTracing.TraceInfo(
                        "[AzureQueuePersist] .AzureQueuePersist: creating message requests queue {0}",
                        requestQueueName);
                    this.EOMFlag = false;
                    this.requestQueueField = AzureStorageTool.CreateQueueAsync(
                        this.storageConnectString,
                        sessionId,
                        requestQueueName,
                        clientId,
                        true,
                        this.FormatRequestQueueLabel()).GetAwaiter().GetResult();

                    BrokerTracing.TraceInfo(
                        "[AzureQueuePersist] .AzureQueuePersist: creating message responses queue {0}",
                        responseTableName);
                    this.responseTableField = AzureStorageTool.CreateTableAsync(
                        this.storageConnectString,
                        sessionId,
                        responseTableName,
                        clientId,
                        false,
                        "0").GetAwaiter().GetResult();

                    this.persistVersion = BrokerQueueItem.PersistVersion;

                    BrokerTracing.TraceVerbose(
                        "[AzureQueuePersist] .AzureQueuePersist: set persist version = {0}",
                        BrokerQueueItem.PersistVersion);

                    // On queue creation completion, mark response queue label with "0", which means failed response count = 0.
                    // Note: label response queue is used as a flag indicating queue creation is completed. So it must be the last step of queue creation.
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureQueuePersist] .AzureQueuePersist: failed to create AzureQueue queue, {0}, exception: {1}.",
                        responseTableName,
                        e);

                    // delete request queue after response queue
                    if (this.responseTableField != null)
                    {
                        this.responseTableField.DeleteAsync().GetAwaiter().GetResult();
                    }

                    if (this.requestQueueField != null)
                    {
                        this.requestQueueField.DeleteAsync().GetAwaiter().GetResult();
                    }

                    throw;
                }
            }
            else
            {
                this.requestQueueField = AzureStorageTool.GetQueue(storageConnectString, requestQueueName).GetAwaiter()
                    .GetResult();
                this.responseTableField = AzureStorageTool.GetTable(storageConnectString, responseTableName);
                this.pendingQueueField = AzureStorageTool.GetQueue(storageConnectString, pendingQueueName).GetAwaiter()
                    .GetResult();
                this.blobContainer = AzureStorageTool
                    .CreateBlobContainerAsync(this.storageConnectString, requestQueueName).GetAwaiter().GetResult();
                try
                {
                    /*AzureStorageTool.RestoreRequest(
                        this.requestQueueField,
                        this.pendingQueueField,
                        this.responseTableField,
                        this.blobContainer).GetAwaiter().GetResult();*/
                    this.requestsCountField = this.requestQueueField.ApproximateMessageCount ?? 0;
                    (this.responsesCountField, this.lastReponseIndex) = AzureStorageTool
                        .CountTableEntity(storageConnectString, responseTableName).GetAwaiter().GetResult();
                    this.allRequestsCountField = this.requestsCountField + this.responsesCountField;
                    if (this.lastReponseIndex > this.responseIndex)
                    {
                        this.responseIndex = this.lastReponseIndex;
                    }

                    this.persistVersion = BrokerVersion.DefaultPersistVersion;
                    this.EOMFlag = this.allRequestsCountField > 0;
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureQueuePersist] .AzureQueuePersist: failed to access azure queue, {0}, exception: {1}.",
                        responseTableName,
                        e);
                    throw;
                }
            }

            // Init fetchers
            BrokerTracing.TraceVerbose("[AzureQueuePersist] .AzureQueuePersist: AzureQueue Transactions Enabled.");
            this.requestFetcher = new AzureQueueRequestFetcher(
                this.requestQueueField,
                this.pendingQueueField,
                this.requestsCountField,
                binFormatterField,
                this.blobContainer);
            this.responseFetcher = new AzureQueueResponseFetcher(
                this.responseTableField,
                this.responsesCountField,
                binFormatterField,
                this.blobContainer,
                this.lastReponseIndex);
        }

        public long AllRequestsCount => Interlocked.Read(ref this.allRequestsCountField);

        public bool EOMReceived
        {
            get => this.EOMFlag;

            set => this.EOMFlag = true;
        }

        public long FailedRequestsCount => Interlocked.Read(ref this.failedRequestsCountField);

        public bool IsNewCreated => this.isNewCreatePersistField;

        public long RequestsCount => Interlocked.Read(ref this.requestsCountField);

        public long ResponsesCount => Interlocked.Read(ref this.responsesCountField);

        public string UserName => this.userNameField;

        public static async Task CleanupStaleMessageQueue(
            IsStaleSessionCallback isStaleSessionCallback,
            string connectString)
        {
            ParamCheckUtility.ThrowIfNull(isStaleSessionCallback, "isStaleSessionCallback");
            ParamCheckUtility.ThrowIfNull(connectString, "connectString");
            var queueinfos = AzureStorageTool.GetAllQueues(connectString).GetAwaiter().GetResult();
            var staleQueueNameList = new List<QueueInfo>();
            var sessionIdStaleDic = new Dictionary<string, bool>();
            if (queueinfos != null && queueinfos.Count > 0)
            {
                foreach (var queueInfo in queueinfos)
                {
                    var queueSessionId = queueInfo.PartitionKey;

                    var isStaleSession = false;
                    if (!sessionIdStaleDic.TryGetValue(queueSessionId, out isStaleSession))
                    {
                        isStaleSession = await isStaleSessionCallback(queueSessionId);
                        sessionIdStaleDic.Add(queueSessionId, isStaleSession);
                    }

                    if (isStaleSession)
                    {
                        BrokerTracing.TraceWarning(
                            "[AzureQueuePersist] .CleanupStaleMessageQueue: stale message queue detected. {0}",
                            queueInfo.RowKey);
                        staleQueueNameList.Add(queueInfo);
                    }
                }

                for (var i = 0; i < staleQueueNameList.Count; i++)
                {
                    try
                    {
                        AzureStorageTool.DeleteQueueAsync(
                            connectString,
                            staleQueueNameList[i].PartitionKey,
                            staleQueueNameList[i].RowKey).GetAwaiter().GetResult();
                        BrokerTracing.TraceInfo(
                            "[AzureQueuePersist] .CleanupStaleMessageQueue: stale message queue '{0}' deleted",
                            staleQueueNameList[i]);
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceWarning(
                            "[AzureQueuePersist] .CleanupStaleMessageQueue: fail to delete the message queue, {0}, Exception: {1}",
                            staleQueueNameList[i],
                            e);
                    }
                }
            }
        }

        public void AbortRequest()
        {
            this.ResetRequestsTransaction(false);
        }

        public void AckResponse(BrokerQueueItem responseItem, bool success)
        {
            responseItem.Dispose();
        }

        public SessionPersistCounter Close()
        {
            BrokerTracing.TraceVerbose("[AzureQueuePersist] .Close: Close the AzureQueuePersist.");
            if (!this.isClosedField)
            {
                this.isClosedField = true;
                this.Dispose();

                // delete response queue before request queue.
                if (this.responseTableField != null)
                {
                    AzureStorageTool.DeleteTableAsync(
                        this.storageConnectString,
                        this.sessionIdField,
                        this.responseTableField.Name).GetAwaiter().GetResult();
                    this.responseTableField = null;
                }

                if (this.pendingQueueField != null)
                {
                    AzureStorageTool.DeleteQueueAsync(
                        this.storageConnectString,
                        this.sessionIdField,
                        this.pendingQueueField.Name).GetAwaiter().GetResult();
                    this.pendingQueueField = null;
                }

                if (this.requestQueueField != null)
                {
                    AzureStorageTool.DeleteQueueAsync(
                        this.storageConnectString,
                        this.sessionIdField,
                        this.requestQueueField.Name).GetAwaiter().GetResult();
                    this.requestQueueField = null;
                }

                if (this.blobContainer != null)
                {
                    this.blobContainer.DeleteIfExistsAsync().GetAwaiter().GetResult();
                    this.blobContainer = null;
                }
            }

            var counter = new SessionPersistCounter();
            counter.ResponsesCountField = Interlocked.Read(ref this.responsesCountField);
            counter.FailedRequestsCountField = Interlocked.Read(ref this.failedRequestsCountField);
            return counter;
        }

        public void CommitRequest()
        {
            this.ResetRequestsTransaction(true);
        }

        public void Dispose()
        {
            BrokerTracing.TraceVerbose(
                "[AzureQueuePersist] .Dispose: Dispose the AzureQueuePersist, this.isDisposedField = {0}",
                this.isDisposedField);
            if (!this.isDisposedField)
            {
                this.isDisposedField = true;
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        public void GetRequestAsync(PersistCallback callback, object state)
        {
            ParamCheckUtility.ThrowIfNull(callback, "callback");
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[AzureQueuePersisit] .GetRequestAsync: the persist queue is disposed.");
                return;
            }

            BrokerTracing.TraceVerbose("[AzureQueuePersisit] .GetRequestAsync: Get request come in.");

            this.requestFetcher.GetMessageAsync(callback, state);
        }

        public void GetResponseAsync(PersistCallback callback, object callbackState)
        {
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[AzureQueuePersisit] .GetResponseAsync: the persist queue is disposed.");
                return;
            }

            if (callbackState == null)
            {
                BrokerTracing.TraceWarning("[AzureQueuePersisit] .GetResponseAsync: the callbackState is null.");
                return;
            }

            this.responseFetcher.GetMessageAsync(callback, callbackState);
        }

        public bool IsInMemory()
        {
            return false;
        }

        public async Task PutRequestAsync(
            BrokerQueueItem request,
            PutRequestCallback putRequestCallback,
            object callbackState)
        {
            ParamCheckUtility.ThrowIfNull(request, "request");
            var requests = new BrokerQueueItem[1];
            requests[0] = request;
            await this.PutRequestsAsync(requests, putRequestCallback, callbackState);
        }

        public async Task PutRequestsAsync(
            IEnumerable<BrokerQueueItem> requests,
            PutRequestCallback putRequestCallback,
            object callbackState)
        {
            ParamCheckUtility.ThrowIfNull(requests, "requests");
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[AzureQueuePersist] .PutRequestsAsync: the persist queue is disposed.");
                return;
            }

            var putRequestState = new PutRequestState(requests, putRequestCallback, callbackState);

            // TODO: make PutRequestsAsync an async call
            await this.PersistRequests(putRequestState);
        }

        public async Task PutResponseAsync(
            BrokerQueueItem response,
            PutResponseCallback putResponseCallback,
            object callbackState)
        {
            var responses = new BrokerQueueItem[1];
            responses[0] = response;
            await this.PutResponsesAsync(responses, putResponseCallback, callbackState);
        }

        public async Task PutResponsesAsync(
            IEnumerable<BrokerQueueItem> responses,
            PutResponseCallback putResponseCallback,
            object callbackState)
        {
            ParamCheckUtility.ThrowIfNull(responses, "responses");
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[AzureQueuePersisit] .PutResponsesAsync: the persist queue is disposed.");
                return;
            }

            BrokerTracing.TraceVerbose(
                "[AzureQueuePersisit] .PutResponsesAsync: new responses come in. Response count: {0}",
                (int)callbackState);
            var putResponseState = new PutResponseState(responses, putResponseCallback, callbackState);

            await this.PersistResponses(putResponseState);
        }

        public void ResetResponsesCallback()
        {
            if (this.isDisposedField)
            {
                return;
            }

            this.lastReponseIndex = this.responseFetcher.AckIndex;
            this.responseFetcher.SafeDispose();
            this.responseFetcher = new AzureQueueResponseFetcher(
                this.responseTableField,
                this.responsesCountField,
                binFormatterField,
                this.blobContainer,
                this.lastReponseIndex);
        }

        internal static ClientInfo[] GetSessionClients(string connectString, string sessionId, bool useAad)
        {
            ParamCheckUtility.ThrowIfNull(connectString, "connectString");

            // Client id is case insensitive
            var clientIdDic = new Dictionary<string, ClientInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var queueinfos = AzureStorageTool.GetQueuesFromTable(connectString, sessionId).GetAwaiter().GetResult();
                if (queueinfos != null && queueinfos.Count > 0)
                {
                    foreach (var queueInfo in queueinfos)
                    {
                        if (queueInfo.IsRequest)
                        {
                            var queue = AzureStorageTool.GetQueue(connectString, queueInfo.RowKey).GetAwaiter()
                                .GetResult();
                            var requestCount = queue.ApproximateMessageCount == null
                                                   ? 0
                                                   : queue.ApproximateMessageCount.Value;
                            ClientInfo info;
                            if (clientIdDic.TryGetValue(queueInfo.ClientId, out info))
                            {
                                info.TotalRequestsCount += requestCount;
                            }
                            else
                            {
                                clientIdDic.Add(
                                    queueInfo.ClientId,
                                    new ClientInfo(queueInfo.ClientId, requestCount, 0));
                            }
                        }
                        else
                        {
                            var (responseCount, _) = AzureStorageTool.CountTableEntity(connectString, queueInfo.RowKey)
                                .GetAwaiter().GetResult();
                            var failedCount = AzureStorageTool.CountFailed(connectString, sessionId, queueInfo.RowKey)
                                .GetAwaiter().GetResult();

                            ClientInfo info;
                            if (clientIdDic.TryGetValue(queueInfo.ClientId, out info))
                            {
                                info.ProcessedRequestsCount = responseCount;
                                info.TotalRequestsCount += responseCount;
                                info.FailedRequestsCount = failedCount;
                            }
                            else
                            {
                                info = new ClientInfo(queueInfo.ClientId, responseCount, responseCount);
                                clientIdDic.Add(queueInfo.ClientId, info);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning(
                    "[AzureQueuePersist] .GetSessionClients: fail to enumerate the message queues, Exception: {0}",
                    e);
                throw new BrokerQueueException((int)BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_FAIL, e.ToString());
            }

            var clients = new ClientInfo[clientIdDic.Keys.Count];
            clientIdDic.Values.CopyTo(clients, 0);
            return clients;
        }

        internal void CloseFetchForTest()
        {
            this.requestFetcher.SafeDispose();
            this.requestFetcher = null;
            this.responseFetcher.SafeDispose();
            this.responseFetcher = null;
        }

        private static string MakeQueuePath(string sessionId, string clientId, bool isRequest)
        {
            if (isRequest)
            {
                return (PathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + QueueNameFieldDelimeter
                        + clientId + RequestQueueSuffix).ToLower();
            }

            return (PendingPathPrefix + sessionId.ToString(CultureInfo.InvariantCulture)
                    + clientId).ToLower();
        }

        private static string MakeTablePath(string sessionId, string clientId)
        {
            var sb = new StringBuilder();
            foreach (var str in clientId.Split('-'))
            {
                sb.Append(str);
            }

            return (PathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + sb + ResponseQueueSuffix).ToLower();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose request/response fetcher before dispoing request/response queues
                // to stop fetching request/response from queues.
                if (this.requestFetcher != null)
                {
                    this.requestFetcher.SafeDispose();
                    this.requestFetcher = null;
                }

                if (this.responseFetcher != null)
                {
                    this.responseFetcher.SafeDispose();
                    this.responseFetcher = null;
                }
            }
        }

        private string FormatRequestQueueLabel()
        {
            return string.Format(
                "{0}={1};{2}={3}",
                VersionLabel,
                this.persistVersion,
                EOMLabel,
                this.EOMFlag ? "1" : "0");
        }

        private async Task PersistRequests(object state)
        {
            var putRequestState = (PutRequestState)state;
            ParamCheckUtility.ThrowIfNull(state, "put request state");
            ParamCheckUtility.ThrowIfNull(putRequestState.Messages, "to-be-persisted requests");

            Exception exception = null;
            long requestsCount = 0;
            BrokerTracing.TraceVerbose(
                "[AzureQueuePersist] .PersistRequests: persist requests start.");
            foreach (var request in putRequestState.Messages)
            {
                ParamCheckUtility.ThrowIfNull(request, "to-be-persisted request");
                using (request)
                {
                    // check if the request size > 64KB, if yes try to persist it into several partial messages
                    CloudQueueMessage sendMsg;
                    var bytes = AzureStorageTool.PrepareMessage(request);
                    
                    try
                    {
                        RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                        await RetryHelper<object>.InvokeOperationAsync(
                            async () =>
                                {
                                    if (bytes.Length > Constant.AzureQueueMsgChunkSize)
                                    {
                                        sendMsg = new CloudQueueMessage(
                                            await AzureStorageTool.CreateBlobFromBytes(this.blobContainer, bytes));
                                    }
                                    else
                                    {
                                        sendMsg = new CloudQueueMessage(bytes);
                                    }

                                    await this.requestQueueField.AddMessageAsync(sendMsg);

                                    requestsCount++;

                                    BrokerTracing.TraceVerbose(
                                        "[AzureQueuePersist] .PersistRequests: send message(s) for persist id {0}.",
                                        request.PersistId);
                                    return null;
                                }, 
                            (e, r) =>
                                {
                                    BrokerTracing.TraceEvent(
                                                        System.Diagnostics.TraceEventType.Error,
                                                        0,
                                                        "[AzureQueuePersist] .PersistRequests: Exception thrown while add message in queue: {0} with retry: {1}",
                                                        e,
                                                        r.RetryCount);
                                    return Task.CompletedTask;
                                },
                            retry);
                    }
                    catch (Exception e)
                    {
                        if (this.isDisposedField)
                        {
                            // if the queue is closed, then quit the method.
                            return;
                        }

                        BrokerTracing.TraceError(
                            "[AzureQueuePersist] .PersistRequests: persist request raised exception, {0}",
                            e);
                        exception = e;

                        break;
                    }
                }
            }

            if (exception == null)
            {
                Interlocked.Add(ref this.uncommittedRequestsCountField, requestsCount);
                Interlocked.Add(ref this.requestsCountField, requestsCount);
                Interlocked.Add(ref this.allRequestsCountField, requestsCount);
            }

            var putRequestCallback = putRequestState.Callback;
            if (putRequestCallback != null)
            {
                try
                {
                    putRequestCallback(exception, putRequestState.CallbackState);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureQueuePersist] .PersistRequestsThreadProc: Persist requests failed, Exception:{0}.",
                        e);
                }
            }

            BrokerTracing.TraceVerbose(
                "[AzureQueuePersist] .PersistRequests: persist requests end.");
        }

        private async Task PersistResponses(object state)
        {
            PutResponseState putResponseState = (PutResponseState)state;
            ParamCheckUtility.ThrowIfNull(putResponseState, "putResponseState");
            ParamCheckUtility.ThrowIfNull(putResponseState.Messages, "putResponseState.Mesasges");
#if DEBUG
            int num = Interlocked.Increment(ref this.procCount);
            BrokerTracing.TraceVerbose(
                "[AzureQueuePersist] .PersistResponses: persist responses start, number = {0}.", num);
#endif
            try
            {
                // Save peer request items of response messages in case persist operation is failed.
                List<BrokerQueueItem> peerItems = new List<BrokerQueueItem>();
                foreach (BrokerQueueItem response in putResponseState.Messages)
                {
                    peerItems.Add(response.PeerItem);
                    response.PeerItem = null;
                }

                Exception exception = null;
                int responseCount = 0;
                int faultResponsesCount = 0;
                List<BrokerQueueItem> redispatchRequestsList = new List<BrokerQueueItem>();
                foreach (BrokerQueueItem response in putResponseState.Messages)
                {
                    ParamCheckUtility.ThrowIfNull(response, "to-be-persisted response");

                    // step 1, check the response whether persisted before.
                    var requestToken = (string)response.PersistAsyncToken.AsyncToken;

                    if (requestToken == null)
                    {
                        continue;
                    }

                    if (this.uniqueResponseDic.ContainsKey(requestToken))
                    {
                        if (this.uniqueResponseDic.TryGetValue(requestToken, out bool isStored))
                        {
                            if (isStored)
                            {
                                continue;
                            }
                        }
                    }

                    if (response.Message.IsFault)
                    {
                        faultResponsesCount++;
                    }

                    // step 2, put response into queue
                    this.rwlockPriorityQueue.EnterWriteLock();
                    long index = Interlocked.Increment(ref this.responseIndex);
                    priorityQueue.Add(index);
                    this.rwlockPriorityQueue.ExitWriteLock();

                    var sendMsg = new ResponseEntity(
                        this.clientIdField,
                        index.ToString(),
                        requestToken,
                        AzureStorageTool.PrepareMessage(response));


                    try
                    {
                        RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();
                        await RetryHelper<object>.InvokeOperationAsync(
                           async () =>
                           {
                               if (sendMsg.Message.Length > Constant.AzureQueueMsgChunkSize)
                               {
                                   sendMsg.Message = await AzureStorageTool
                                       .CreateBlobFromBytes(this.blobContainer, sendMsg.Message);
                               }

                               await AzureStorageTool.AddMsgToTable(this.responseTableField, sendMsg);
                               return null;
                           },
                           (e, r) =>
                           {
                               BrokerTracing.TraceEvent(
                                                   System.Diagnostics.TraceEventType.Error,
                                                   0,
                                                   "[AzureQueuePersist] .PersistResponses: Exception thrown while add entity in table: {0} with retry: {1}",
                                                   e,
                                                   r.RetryCount);
                               return Task.CompletedTask;
                           },
                           retry);
                    }
                    catch (Exception e)
                    {
                        if (this.isDisposedField)
                        {
                            // if the queue is closed, then quit the method.
                            return;
                        }

                        BrokerTracing.TraceWarning(
                            "[AzureQueuePersist] .PersistResponses: Operate error with azure storage, Exception: {0}",
                            e);
                        exception = e;
                        storedResponseDic.AddOrUpdate(index, false, (k, v) => false);
                        this.uniqueResponseDic.AddOrUpdate(requestToken, false, (k, v) => false);
                        await Task.Run(() => ReleaseTask());
                        break;
                    }

                    // At this point, both step 1 & step 2 are performed succeesfully
                    responseCount++;
                    storedResponseDic.AddOrUpdate(index, true, (k, v) => true);
                    this.uniqueResponseDic.AddOrUpdate(requestToken, true, (k, v) => true);
                    await Task.Run(() => this.ReleaseTask());
                }
                
                // persisting succeed
                if (exception == null)
                {
                    // dispose all response messages
                    foreach (BrokerQueueItem response in putResponseState.Messages)
                    {
                        response.Dispose();
                    }
                    // dispose all peer items
                    foreach (BrokerQueueItem request in peerItems)
                    {
                        request.Dispose();
                    }
                }

                if (exception != null)
                {

                    responseCount = 0;
                    BrokerTracing.TraceError("[AzureQueuePersist] .PersistResponses failed to persist the response to the responses queue, and the corrresponding requests will be redispatched soon.");

                    // lost the responses, the corresponding requests should can be redispatched soon.
                    int index = 0;
                    foreach (BrokerQueueItem response in putResponseState.Messages)
                    {
                        if (response != null && response.PersistAsyncToken != null)
                        {
                            response.PeerItem = peerItems[index];
                            redispatchRequestsList.Add(response);
                        }
                        else
                        {
                            BrokerTracing.TraceError("[AzureQueuePersist] .PersistResponses: invalid response, the response.AsyncToken is null.");
                        }

                        index++;
                    }
                }
                else if (faultResponsesCount > 0)
                {
                    try
                    {
                        await AzureStorageTool.UpdateInfo(
                                this.storageConnectString,
                                this.sessionIdField,
                                this.responseTableField.Name,
                                (Interlocked.Read(ref this.failedRequestsCountField) + faultResponsesCount).ToString());
                        Interlocked.Add(ref this.failedRequestsCountField, faultResponsesCount);
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceWarning("[AzureQueuePersist] .PersistResponses: Fail to update the fault message number in the response queue label, fault message count: {0}, Exception: {1}", faultResponsesCount, e);
                    }
                }

                // Note: Update responsesCountField before requestsCountField. - tricky part.
                // FIXME!
                Interlocked.Add(ref this.responsesCountField, responseCount);

                long remainingRequestCount = Interlocked.Add(ref this.requestsCountField, -responseCount);
                bool isLastResponse = EOMReceived && (remainingRequestCount == 0);
                PutResponseCallback putResponseCallback = putResponseState.Callback;
                if (putResponseCallback != null)
                {
                    putResponseCallback(exception, responseCount, faultResponsesCount, isLastResponse, redispatchRequestsList, putResponseState.CallbackState);
                }
#if DEBUG
                BrokerTracing.TraceVerbose(
                    "[AzureQueuePersist] .PersistResponses: persist responses end, num = {0}.", num);
#endif
            }
            catch (Exception e)
            {
                if (!this.isDisposedField)
                {
                    BrokerTracing.TraceError("[AzureQueuePersist] .PersistResponses: persist responses failed, Exception: {0}", e);
                }
            }
        }

        private void ReleaseTask()
        {
            long p = Interlocked.Exchange(ref responseLock, 2);
            if (p == 0)
            {
                do
                {
                    p = Interlocked.Exchange(ref responseLock, 1);
                    if (p == 1)
                        return;
                    try
                    {
                        BrokerTracing.TraceVerbose(
                            "[AzureQueuePersist] .ReleaseTask: count: {0}, min: {1}",
                            priorityQueue.Count, priorityQueue.Count == 0 ? -1 : priorityQueue.FindMin());
                        long max = 0;
                        long responseCount = 0;

                        while (true)
                        {
                            this.rwlockPriorityQueue.EnterUpgradeableReadLock();
                            try
                            {
                                if (priorityQueue.Count > 0)
                                {
                                    if (storedResponseDic.ContainsKey(priorityQueue.FindMin()))
                                    {
                                        if (storedResponseDic.TryGetValue(
                                            this.priorityQueue.FindMin(),
                                            out bool isStored))
                                        {
                                            if (isStored)
                                            {
                                                max = priorityQueue.FindMin();
                                                responseCount++;
                                            }
                                        }

                                        this.storedResponseDic.TryRemove(this.priorityQueue.FindMin(), out bool temp);
                                        this.rwlockPriorityQueue.EnterWriteLock();
                                        priorityQueue.DeleteMin();
                                        this.rwlockPriorityQueue.ExitWriteLock();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            finally
                            {
                                this.rwlockPriorityQueue.ExitUpgradeableReadLock();
                            }
                        }

                        BrokerTracing.TraceVerbose(
                            "[AzureQueuePersist] .ReleaseTask: Ack update: {0}, {1}",
                            max, responseCount);
                        this.responseFetcher.ChangeAck(max);
                        this.responseFetcher.NotifyMoreMessages(responseCount);
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError("[AzureQueuePersist].ReleaseTask error: {0}", e);
                        throw;
                    }
                    finally
                    {
                        p = Interlocked.Exchange(ref responseLock, 0);
                    }

                } while (p == 2);
            }
        }

        private void ResetRequestsTransaction(bool needCommit)
        {
            if (needCommit)
            {
                var committed = Interlocked.Exchange(ref this.uncommittedRequestsCountField, 0);
                this.requestFetcher.NotifyMoreMessages(committed);
            }
            else
            {
                // reset uncommittedRequestsCountField
                var committed = Interlocked.Exchange(ref this.uncommittedRequestsCountField, 0);
                Interlocked.Add(ref this.requestsCountField, -committed);
            }
        }

        /// <summary>
        ///     thread pool callback state for putting requests
        /// </summary>
        private class PutRequestState
        {
            /// <summary>the callback for putting request.</summary>
            private readonly PutRequestCallback callbackField;

            /// <summary>the callback state object.</summary>
            private readonly object callbackStateField;

            /// <summary>the messages.</summary>
            private readonly IEnumerable<BrokerQueueItem> messagesField;

            /// <summary>
            ///     Initializes a new instance of the PutRequestState class.
            /// </summary>
            /// <param name="messages">the messages.</param>
            /// <param name="calback">the callback.</param>
            /// <param name="callbackState">the calllback state object.</param>
            public PutRequestState(
                IEnumerable<BrokerQueueItem> messages,
                PutRequestCallback callback,
                object callbackState)
            {
                ParamCheckUtility.ThrowIfNull(messages, "messages");
                this.messagesField = messages;
                this.callbackField = callback;
                this.callbackStateField = callbackState;
            }

            /// <summary>
            ///     Gets the callback.
            /// </summary>
            public PutRequestCallback Callback => this.callbackField;

            /// <summary>
            ///     Gets the callback state.
            /// </summary>
            public object CallbackState => this.callbackStateField;

            /// <summary>
            ///     Gets the requests.
            /// </summary>
            public IEnumerable<BrokerQueueItem> Messages => this.messagesField;
        }

        /// <summary>
        ///     thread pool callback stat for putting responses
        /// </summary>
        private class PutResponseState
        {
            /// <summary>the callback for putting response.</summary>
            private readonly PutResponseCallback callbackField;

            /// <summary>the calllback state object.</summary>
            private readonly object callbackStateField;

            /// <summary>the messages.</summary>
            private readonly IEnumerable<BrokerQueueItem> messagesField;

            /// <summary>
            ///     Initializes a new instance of the PutRequestState class.
            /// </summary>
            /// <param name="messages">the messages.</param>
            /// <param name="callback">the callback.</param>
            /// <param name="callbackState">the calllback state object.</param>
            public PutResponseState(
                IEnumerable<BrokerQueueItem> messages,
                PutResponseCallback callback,
                object callbackState)
            {
                ParamCheckUtility.ThrowIfNull(messages, "messages");
                this.messagesField = messages;
                this.callbackField = callback;
                this.callbackStateField = callbackState;
            }

            /// <summary>
            ///     Gets the callback.
            /// </summary>
            public PutResponseCallback Callback => this.callbackField;

            /// <summary>
            ///     Gets the callback state.
            /// </summary>
            public object CallbackState => this.callbackStateField;

            /// <summary>
            ///     Gets the requests.
            /// </summary>
            public IEnumerable<BrokerQueueItem> Messages => this.messagesField;
        }
    }
}