using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CcpWSLB.UnitTest")]

namespace Microsoft.Hpc.ServiceBroker.BrokerStorage.AzureQueuePersist
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.Scheduler.Session.Utility;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage.AzureStorageTool;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;

    public class AzureQueuePersist : ISessionPersist
    {
        /// <summary>queue owner name for "anonymous" user</summary>
        private const string AnonymousOwner = "Everyone";

        /// <summary>the prefix for EOM flag label</summary>
        private const string EOMLabel = "EOM";

        /// <summary>the prefix for generating queue path on local computer</summary>
        private const string LocalQueuePathPrefix = "-";

        /// <summary>the prefix of the queue path</summary>
        private const string PathPrefix = "HPC";

        private const string PrivatePathPrefix = "Private";

        /// <summary>delimeter for generating queue name</summary>
        private const string QueueNameFieldDelimeter = "-";

        /// <summary>the prefix of the request queue name</summary>
        private const string RequestQueueSuffix = "REQUESTS";

        /// <summary>the prefix of the response queue name</summary>
        private const string ResponseQueueSuffix = "RESPONSES";

        /// <summary>queue owner name for "anonymous" user</summary>
        private const string SystemOwner = "System";

        /// <summary>the prefix for persist version label</summary>
        private const string VersionLabel = "VERSION";

        /// <summary>
        /// the regex to match the cert identity user name
        /// </summary>
        private static readonly Regex CertUserNameRegex = new Regex(@"CN=[\w\s]*;\ [0-9A-F]+", RegexOptions.IgnoreCase);

        /// <summary>
        /// the regex to match the queue name
        /// </summary>
        private static readonly Regex QueueNameRegex = new Regex(
            @"PRIVATE\$\\HPC(?<SessionId>-?\d+)-(?<ClientId>.*)-(?<Suffix>(REQUESTS)|(RESPONSES))$",
            RegexOptions.IgnoreCase);

        /// <summary>the binary message formattoer</summary>
        private static IFormatter binFormatterField = new BinaryFormatter();

        /// <summary>the broker node name.  For HA cluster, it is the cluster virtual name.</summary>
        private static string BrokerNodeName = BrokerIdentity.GetBrokerName();

        /// <summary>the total request count.</summary>
        private long allRequestsCountField;

        private CloudBlobContainer blobContainer;

        /// <summary>the client id.</summary>
        private string clientIdField;

        /// <summary>flag indicating if all requests have been received.</summary>
        private bool EOMFlag;

        /// <summary>Gets the number of the requests that get the fault responses.</summary>
        private long failedRequestsCountField;

        /// <summary>a value indicating whether the MSMQ persistence is closed.</summary>
        private volatile bool isClosedField;

        private bool isCreatedResponseTask = false;

        private bool isCreateRequestTask = false;

        private bool isDisposedField;

        /// <summary>a value indicating whether this is a new created AzureStorage persistence.</summary>
        private bool isNewCreatePersistField;

        /// <summary>persist version of this AzureQueue queue</summary>
        private int persistVersion;

        private CloudQueue privateQueueField;

        private AzureQueueMessageFetcher requestFetcher;

        /// <summary>the message queue that store the request messages.</summary>
        private CloudQueue requestQueueField;

        /// <summary>the total requests count in the request queue.</summary>
        private long requestsCountField;

        private object requestTaskLock = new object();

        private AzureQueueMessageFetcher responseFetcher;

        private int responseIndex = int.MaxValue >> 1;

        private object responsePutLock = new object();

        private Queue<PutResponseState> responsePutQueue = new Queue<PutResponseState>();

        /// <summary>the total responses count in the queue.</summary>
        private long responsesCountField;

        private CloudTable responseTableField;

        /// <summary>the session id.</summary>
        private int sessionIdField;

        private int sleepTime = 500;

        private string storageConnectString;

        /// <summary>number of requests that are sent to MSMQ but not committed yet. </summary>
        private long uncommittedRequestsCountField;

        /// <summary>the user name.</summary>
        private string userNameField;

        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        private Dictionary<string, int> requestRetryDic = new Dictionary<string, int>();

        internal AzureQueuePersist(string userName, int sessionId, string clientId, string storageConnectString)
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

            string requestQueueName = MakeQueuePath(sessionId, clientId, true);
            string responseTableName = MakeTablePath(sessionId, clientId);
            string privateQueueName = MakeQueuePath(sessionId, clientId, false);

            this.isNewCreatePersistField = true;
            try
            {
                bool requestQueueExist = AzureStorageTool.ExistsQueue(storageConnectString, requestQueueName).GetAwaiter().GetResult();
                bool responseQueueExist = AzureStorageTool.ExistTable(storageConnectString, responseTableName).GetAwaiter().GetResult();

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
                        AzureStorageTool.DeleteQueueAsync(storageConnectString, sessionId.ToString(), requestQueueName)
                            .GetAwaiter().GetResult();
                    }
                    else
                    {
                        // There is only response queue but no request queue - this should rarely happen, as we always 
                        // create request queue before response queue, and delete request queue after response queue.
                        BrokerTracing.TraceError(
                            "[AzureQueuePersist] .AzureQueuePersist: queue data not integrety.  Fix it by deleting queue = {0}",
                            responseTableName);
                        AzureStorageTool.DeleteTableAsync(
                            storageConnectString,
                            sessionId.ToString(),
                            responseTableName).GetAwaiter().GetResult();
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
                    BrokerTracing.TraceInfo(
                        "[AzureQueuePersist] .AzureQueuePersist: creating message requests queue {0}",
                        requestQueueName);
                    this.EOMFlag = false;
                    this.requestQueueField = AzureStorageTool.CreateQueueAsync(
                        this.storageConnectString,
                        sessionId.ToString(),
                        requestQueueName,
                        clientId,
                        true,
                        this.FormatRequestQueueLabel()).GetAwaiter().GetResult();

                    this.privateQueueField = AzureStorageTool.CreateQueueAsync(
                        this.storageConnectString,
                        sessionId.ToString(),
                        privateQueueName,
                        clientId,
                        true,
                        this.FormatRequestQueueLabel()).GetAwaiter().GetResult();

                    BrokerTracing.TraceInfo(
                        "[AzureQueuePersist] .AzureQueuePersist: creating message responses queue {0}",
                        responseTableName);
                    this.responseTableField = AzureStorageTool.CreateTableAsync(
                        this.storageConnectString,
                        sessionId.ToString(),
                        responseTableName,
                        clientId,
                        false,
                        "0").GetAwaiter().GetResult();

                    this.blobContainer = AzureStorageTool
                        .CreateBlobContainerAsync(this.storageConnectString, requestQueueName).GetAwaiter().GetResult();
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
                //TODO go through the private queue and resend responses not received in response table.
                this.privateQueueField = AzureStorageTool.GetQueue(storageConnectString, privateQueueName).GetAwaiter()
                    .GetResult();
                this.blobContainer = AzureStorageTool
                    .CreateBlobContainerAsync(this.storageConnectString, requestQueueName).GetAwaiter().GetResult();
                try
                {
                    this.requestsCountField = this.requestQueueField.ApproximateMessageCount ?? 0;
                    this.responsesCountField = AzureStorageTool
                        .CountTableEntity(storageConnectString, responseTableName).GetAwaiter().GetResult();
                    this.allRequestsCountField = this.requestsCountField + this.responsesCountField;

                    this.persistVersion = BrokerVersion.DefaultPersistVersion;
                    this.EOMFlag = (this.allRequestsCountField > 0);
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

            // Init transaction for persisting requests
            BrokerTracing.TraceVerbose("[AzureQueuePersist] .AzureQueuePersist: AzureQueue Transactions Enabled.");
            this.requestFetcher = new AzureQueueRequestFetcher(
                this.requestQueueField,
                this.privateQueueField,
                this.requestsCountField,
                binFormatterField,
                this.blobContainer);
            this.responseFetcher = new AzureQueueResponseFetcher(
                this.responseTableField,
                this.responsesCountField,
                binFormatterField,
                this.blobContainer);
        }

        public long AllRequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.allRequestsCountField);
            }
        }

        public bool EOMReceived
        {
            get
            {
                return this.EOMFlag;
            }

            set
            {
                this.EOMFlag = true;
            }
        }

        public long FailedRequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.failedRequestsCountField);
            }
        }

        public bool IsNewCreated
        {
            get
            {
                return this.isNewCreatePersistField;
            }
        }

        public long RequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.requestsCountField);
            }
        }

        public long ResponsesCount
        {
            get
            {
                return Interlocked.Read(ref this.responsesCountField);
            }
        }

        public string UserName
        {
            get
            {
                return this.userNameField;
            }
        }

        public static async Task CleanupStaleMessageQueue(
            IsStaleSessionCallback isStaleSessionCallback,
            string connectString)
        {
            ParamCheckUtility.ThrowIfNull(isStaleSessionCallback, "isStaleSessionCallback");
            ParamCheckUtility.ThrowIfNull(connectString, "connectString");
            List<QueueInfo> queueinfos = AzureStorageTool.GetAllQueues(connectString).GetAwaiter().GetResult();
            List<QueueInfo> staleQueueNameList = new List<QueueInfo>();
            Dictionary<int, bool> sessionIdStaleDic = new Dictionary<int, bool>();
            if (queueinfos != null && queueinfos.Count > 0)
            {
                foreach (QueueInfo queueInfo in queueinfos)
                {
                    int queueSessionId = int.Parse(queueInfo.PartitionKey);

                    bool isStaleSession = false;
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

                for (int i = 0; i < staleQueueNameList.Count; i++)
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
                        this.sessionIdField.ToString(),
                        this.responseTableField.Name).GetAwaiter().GetResult();
                    this.responseTableField = null;
                }

                if (this.privateQueueField != null)
                {
                    AzureStorageTool.DeleteQueueAsync(
                        this.storageConnectString,
                        this.sessionIdField.ToString(),
                        this.privateQueueField.Name).GetAwaiter().GetResult();
                    this.privateQueueField = null;
                }

                if (this.requestQueueField != null)
                {
                    AzureStorageTool.DeleteQueueAsync(
                        this.storageConnectString,
                        this.sessionIdField.ToString(),
                        this.requestQueueField.Name).GetAwaiter().GetResult();
                    this.requestQueueField = null;
                }

                if (this.blobContainer != null)
                {
                    this.blobContainer.DeleteIfExistsAsync().GetAwaiter().GetResult();
                    this.blobContainer = null;
                }
            }

            SessionPersistCounter counter = new SessionPersistCounter();
            counter.ResponsesCountField = Interlocked.Read(ref this.responsesCountField);
            counter.FailedRequestsCountField = Interlocked.Read(ref this.failedRequestsCountField);
            return counter;
        }

        public bool IsInMemory()
        {
            return false;
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
            if (!this.isCreateRequestTask)
            {
                {
                    lock (this.requestTaskLock)
                    {
                        if (!this.isCreateRequestTask)
                        {
                            this.isCreateRequestTask = true;
                            Task.Run(
                                async () =>
                                    {
                                        while (true)
                                        {
                                            if (this.tokenSource.Token.IsCancellationRequested)
                                            {
                                                return;
                                            }

                                            int time;
                                            int uncommitted;
                                            IEnumerable<BrokerQueueItem> responses;
                                            (time, uncommitted, responses) = await AzureStorageTool.CheckRequestQueue(
                                                   this.requestQueueField,
                                                   this.privateQueueField,
                                                   this.responseTableField,
                                                   this.blobContainer,
                                                   this.requestRetryDic);
                                            if (uncommitted > 0)
                                            {
                                                Interlocked.Add(ref this.uncommittedRequestsCountField, uncommitted);
                                                this.CommitRequest();
                                            }

                                            if (responses.ToArray().Length > 0)
                                            {
                                                this.PutResponsesAsync(responses, null, null);
                                            }

                                            await Task.Delay(time);
                                        }
                                    });
                        }
                    }
                }
            }

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

        public void PutRequestAsync(
            BrokerQueueItem request,
            PutRequestCallback putRequestCallback,
            object callbackState)
        {
            ParamCheckUtility.ThrowIfNull(request, "request");
            BrokerQueueItem[] requests = new BrokerQueueItem[1];
            requests[0] = request;
            this.PutRequestsAsync(requests, putRequestCallback, callbackState);
        }

        public void PutRequestsAsync(
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

            PutRequestState putRequestState = new PutRequestState(requests, putRequestCallback, callbackState);

            // TODO: make PutRequestsAsync an async call
            this.PersistRequests(putRequestState);
        }

        public void PutResponseAsync(
            BrokerQueueItem response,
            PutResponseCallback putResponseCallback,
            object callbackState)
        {
            BrokerQueueItem[] responses = new BrokerQueueItem[1];
            responses[0] = response;
            this.PutResponsesAsync(responses, putResponseCallback, callbackState);
        }

        public void PutResponsesAsync(
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
            PutResponseState putResponseState = new PutResponseState(responses, putResponseCallback, callbackState);

            this.responsePutQueue.Enqueue(putResponseState);
            if (!this.isCreatedResponseTask)
            {
                lock (this.responsePutLock)
                {
                    if (!this.isCreatedResponseTask)
                    {
                        this.isCreatedResponseTask = true;
                        Task.Run(
                            async () =>
                                {
                                    while (true)
                                    {
                                        if (this.tokenSource.Token.IsCancellationRequested)
                                        {
                                            return;
                                        }

                                        this.PersistResponsesThreadProc();
                                        await Task.Delay(this.sleepTime);
                                    }
                                }, 
                            this.tokenSource.Token);
                    }
                }
            }

            // this.PersistResponsesThreadProc(putResponseState);
        }

        public void ResetResponsesCallback()
        {
            if (this.isDisposedField)
            {
                return;
            }

            this.responseFetcher.SafeDispose();
            this.responseFetcher = new AzureQueueResponseFetcher(
                this.responseTableField,
                this.responsesCountField,
                binFormatterField,
                this.blobContainer);
        }

        internal static ClientInfo[] GetSessionClients(string connectString, int sessionId, bool useAad)
        {
            ParamCheckUtility.ThrowIfNull(connectString, "connectString");

            // Client id is case insensitive
            Dictionary<string, ClientInfo> clientIdDic =
                new Dictionary<string, ClientInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                List<QueueInfo> queueinfos = AzureStorageTool.GetQueuesFromTable(connectString, sessionId.ToString())
                    .GetAwaiter().GetResult();
                if (queueinfos != null && queueinfos.Count > 0)
                {
                    foreach (QueueInfo queueInfo in queueinfos)
                    {
                        if (queueInfo.IsRequest)
                        {
                            CloudQueue queue = AzureStorageTool.GetQueue(connectString, queueInfo.RowKey).GetAwaiter()
                                .GetResult();
                            int requestCount = queue.ApproximateMessageCount == null
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
                            long responseCount = AzureStorageTool.CountTableEntity(connectString, queueInfo.RowKey)
                                .GetAwaiter().GetResult();
                            long failedCount = AzureStorageTool
                                .CountFailed(connectString, sessionId.ToString(), queueInfo.RowKey).GetAwaiter()
                                .GetResult();

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

            ClientInfo[] clients = new ClientInfo[clientIdDic.Keys.Count];
            clientIdDic.Values.CopyTo(clients, 0);
            return clients;
        }

        public void AbortRequest()
        {
            this.ResetRequestsTransaction(false);
        }

        internal void CloseFetchForTest()
        {
            this.requestFetcher.SafeDispose();
            this.requestFetcher = null;
            this.responseFetcher.SafeDispose();
            this.responseFetcher = null;
        }

        public void CommitRequest()
        {
            this.ResetRequestsTransaction(true);
        }

        private static string MakeQueuePath(int sessionId, string clientId, bool isRequest)
        {
            if (isRequest)
            {
                return (PathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + QueueNameFieldDelimeter
                        + clientId + QueueNameFieldDelimeter + RequestQueueSuffix).ToLower();
            }
            else
            {
                return (PrivatePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) + QueueNameFieldDelimeter
                        + clientId).ToLower();
            }
        }

        private static string MakeTablePath(int sessionId, string clientId)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string str in clientId.Split('-'))
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

                //Stop checkWaitQueue thread and persistResponseProc
                tokenSource.Cancel();
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

        private void PersistRequests(object state)
        {
            PutRequestState putRequestState = (PutRequestState)state;
            ParamCheckUtility.ThrowIfNull(state, "put request state");
            ParamCheckUtility.ThrowIfNull(putRequestState.Messages, "to-be-persisted requests");

            Exception exception = null;
            long requestsCount = 0;

            foreach (BrokerQueueItem request in putRequestState.Messages)
            {
                ParamCheckUtility.ThrowIfNull(request, "to-be-persisted request");
                using (request)
                {
                    // check if the request size > 64KB, if yes try to persist it into several partial messages
                    CloudQueueMessage sendMsg;
                    byte[] bytes = AzureStorageTool.PrepareMessage(request);
                    try
                    {
                        exception = null;
                        if (bytes.Length > Constant.AzureQueueMsgChunkSize)
                        {
                            sendMsg = new CloudQueueMessage(
                                AzureStorageTool.CreateBlobFromBytes(this.blobContainer, bytes).GetAwaiter()
                                    .GetResult());
                        }
                        else
                        {
                            sendMsg = new CloudQueueMessage(bytes);
                        }

                        this.requestQueueField.AddMessageAsync(sendMsg).GetAwaiter().GetResult();

                        requestsCount++;

                        BrokerTracing.TraceVerbose(
                            "[AzureQueuePersist] .PersistRequestsThreadProc: send message(s) for persist id {0}.",
                            request.PersistId);
                    }
                    catch (Exception e)
                    {
                        if (this.isDisposedField)
                        {
                            // if the queue is closed, then quit the method.
                            return;
                        }

                        BrokerTracing.TraceError(
                            "[AzureQueuePersist] .PersistRequestsThreadProc: persist request raised exception, {0}",
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

            PutRequestCallback putRequestCallback = putRequestState.Callback;
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
        }

        private void PersistResponsesThreadProc()
        {
            const int BatchSize = 50;
            List<BrokerQueueItem> failList = new List<BrokerQueueItem>();
            List<ResponseEntity> responseEntities = new List<ResponseEntity>();
            Dictionary<string, BrokerQueueItem> peerItems = new Dictionary<string, BrokerQueueItem>();
            List<BrokerQueueItem> responsesList = new List<BrokerQueueItem>();
            int responseCount = 0;
            int faultResponsesCount = 0;
            List<Exception> exceptions = new List<Exception>();
            PutResponseCallback putResponseCallback = null;

            while (this.responsePutQueue.Count > 0)
            {
                PutResponseState putResponseState = this.responsePutQueue.Dequeue();
                putResponseCallback = putResponseState.Callback;
                ParamCheckUtility.ThrowIfNull(putResponseState, "putResponseState");
                ParamCheckUtility.ThrowIfNull(putResponseState.Messages, "putResponseState.Mesasges");

                try
                {
                    foreach (BrokerQueueItem response in putResponseState.Messages)
                    {
                        ParamCheckUtility.ThrowIfNull(response, "to-be-persisted response");

                        // step 1, receive corresponding request from queue
                        // no duplicate response for one request
                        string requestToken = (string)response.PersistAsyncToken.AsyncToken;
                        bool isValid = false;
                        try
                        {
                            if (requestToken != null)
                            {
                                isValid = !(AzureStorageTool.IsExistedResponse(this.responseTableField, requestToken)
                                                .GetAwaiter().GetResult() || peerItems.ContainsKey(requestToken));
                            }
                        }
                        catch (Exception e)
                        {
                            BrokerTracing.TraceError(
                                "[AzureQueuePersist] .PersistResponsesThreadProc: can not receive the corresponding request by lookup id[{0}] from the requests queue when persist the response with the exception,{1}.",
                                requestToken,
                                e);
                        }

                        if (!isValid)
                        {
                            continue;
                        }

                        if (response.Message.IsFault)
                        {
                            faultResponsesCount++;
                        }

                        peerItems.Add(requestToken, response.PeerItem);
                        response.PeerItem = null;
                        responsesList.Add(response);

                        // step 2, put response into queue
                        Interlocked.Increment(ref this.responseIndex);
                        ResponseEntity sendMsg = new ResponseEntity(
                            this.clientIdField,
                            this.responseIndex.ToString(),
                            requestToken,
                            AzureStorageTool.PrepareMessage(response));

                        try
                        {
                            if (sendMsg.Message.Length > Constant.AzureQueueMsgChunkSize)
                            {
                                sendMsg.Message = AzureStorageTool
                                    .CreateBlobFromBytes(this.blobContainer, sendMsg.Message).GetAwaiter().GetResult();
                            }

                            responseEntities.Add(sendMsg);

                            // AzureStorageTool.AddMsgToTable(this.responseTableField, sendMsg).GetAwaiter().GetResult();
                        }
                        catch (Exception e)
                        {
                            if (this.isDisposedField)
                            {
                                // if the queue is closed, then quit the method.
                                return;
                            }

                            response.PeerItem = peerItems[requestToken];
                            failList.Add(response);
                            peerItems.Remove(requestToken);
                            BrokerTracing.TraceError(
                                "[AzureQueuePersisit] .PersistResponsesThreadProc: can not save large message into blob, Exception: {0}",
                                e);
                        }

                        // At this point, both step 1 & step 2 are performed successfully
                        responseCount++;
                        if (responseCount % BatchSize == 0)
                            insertTableAndUpdate(BatchSize);
                    }
                }
                catch (Exception e)
                {
                    if (!this.isDisposedField)
                    {
                        BrokerTracing.TraceError(
                            "[AzureQueuePersisit] .PersistResponsesThreadProc: persist responses failed, Exception: {0}",
                            e);
                    }
                }
            }

            void insertTableAndUpdate(int count)
            {
                Exception exception = null;
                try
                {
                    AzureStorageTool.AddBatchMsgToTable(this.responseTableField, responseEntities).GetAwaiter()
                        .GetResult();
                }
                catch (Exception e)
                {
                    exception = e;
                    exceptions.Add(e);
                    responseCount = 0;
                    if (this.isDisposedField)
                    {
                        // if the queue is closed, then quit the method.
                        return;
                    }

                    foreach (BrokerQueueItem responseTemp in responsesList)
                    {
                        string token = (string)responseTemp.PersistAsyncToken.AsyncToken;
                        responseTemp.PeerItem = peerItems[token];
                        failList.Add(responseTemp);
                        peerItems.Remove(token);
                    }

                    BrokerTracing.TraceError(
                        "[AzureQueuePersisit] .PersistResponsesThreadProc: can not send the response to the responses queue, Exception: {0}",
                        e);
                }

                // persisting succeed
                if (exception == null)
                {
                    // dispose all response messages
                    foreach (BrokerQueueItem responseTemp in responsesList)
                    {
                        responseTemp.Dispose();
                    }

                    // dispose all peer items
                    foreach (BrokerQueueItem request in peerItems.Values)
                    {
                        request.Dispose();
                    }
                }

                Interlocked.Add(ref this.responsesCountField, count);
                this.responseFetcher.NotifyMoreMessages(count);
                Interlocked.Add(ref this.requestsCountField, -count);
                peerItems.Clear();
                responseEntities.Clear();
                responsesList.Clear();
            }

            if (responseCount % BatchSize > 0)
                insertTableAndUpdate(responseCount % BatchSize);

            if (faultResponsesCount > 0)
            {
                try
                {
                    AzureStorageTool.UpdateInfo(
                            this.storageConnectString,
                            this.sessionIdField.ToString(),
                            this.responseTableField.Name,
                            (Interlocked.Read(ref this.failedRequestsCountField) + faultResponsesCount).ToString())
                        .GetAwaiter().GetResult();
                    Interlocked.Add(ref this.failedRequestsCountField, faultResponsesCount);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    BrokerTracing.TraceWarning(
                        "[AzureQueuePersisit] .PersistResponsesThreadProc: Fail to update the fault message number in the response queue label, fault message count: {0}, Exception: {1}",
                        faultResponsesCount,
                        e);
                }
            }

            if (failList.Count > 0)
                this.responsePutQueue.Enqueue(new PutResponseState(failList, putResponseCallback, failList.Count));

            bool isLastResponse = this.EOMReceived && (this.requestsCountField == 0);
            if (putResponseCallback != null)
            {
                putResponseCallback(
                    exceptions.Count > 0 ? exceptions[exceptions.Count - 1] : null,
                    responseCount,
                    faultResponsesCount,
                    isLastResponse,
                    null,
                    responseCount + faultResponsesCount);
            }
        }

        private void ResetRequestsTransaction(bool needCommit)
        {
            if (needCommit)
            {
                long committed = Interlocked.Exchange(ref this.uncommittedRequestsCountField, 0);
                this.requestFetcher.NotifyMoreMessages(committed);
            }
            else
            {
                // reset uncommittedRequestsCountField
                long committed = Interlocked.Exchange(ref this.uncommittedRequestsCountField, 0);
                Interlocked.Add(ref this.requestsCountField, -committed);
            }
        }

        /// <summary>
        /// thread pool callback state for putting requests
        /// </summary>
        private class PutRequestState
        {
            /// <summary>the callback for putting request.</summary>
            private PutRequestCallback callbackField;

            /// <summary>the callback state object.</summary>
            private object callbackStateField;

            /// <summary>the messages.</summary>
            private IEnumerable<BrokerQueueItem> messagesField;

            /// <summary>
            /// Initializes a new instance of the PutRequestState class.
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
            /// Gets the callback.
            /// </summary>
            public PutRequestCallback Callback
            {
                get
                {
                    return this.callbackField;
                }
            }

            /// <summary>
            /// Gets the callback state.
            /// </summary>
            public object CallbackState
            {
                get
                {
                    return this.callbackStateField;
                }
            }

            /// <summary>
            /// Gets the requests.
            /// </summary>
            public IEnumerable<BrokerQueueItem> Messages
            {
                get
                {
                    return this.messagesField;
                }
            }
        }

        /// <summary>
        /// thread pool callback stat for putting responses
        /// </summary>
        private class PutResponseState
        {
            /// <summary>the callback for putting response.</summary>
            private PutResponseCallback callbackField;

            /// <summary>the calllback state object.</summary>
            private object callbackStateField;

            /// <summary>the messages.</summary>
            private IEnumerable<BrokerQueueItem> messagesField;

            /// <summary>
            /// Initializes a new instance of the PutRequestState class.
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
            /// Gets the callback.
            /// </summary>
            public PutResponseCallback Callback
            {
                get
                {
                    return this.callbackField;
                }
            }

            /// <summary>
            /// Gets the callback state.
            /// </summary>
            public object CallbackState
            {
                get
                {
                    return this.callbackStateField;
                }
            }

            /// <summary>
            /// Gets the requests.
            /// </summary>
            public IEnumerable<BrokerQueueItem> Messages
            {
                get
                {
                    return this.messagesField;
                }
            }
        }
    }
}