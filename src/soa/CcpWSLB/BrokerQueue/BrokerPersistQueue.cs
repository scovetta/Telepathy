// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.FrontEnd;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;

    /// <summary>
    /// the persist queue that will save the messages to the persistence
    /// TODO: BrokerPersistQueue should be inherited from the BrokerQueue directly.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "this is a queue implementation.")]
    internal class BrokerPersistQueue : BrokerQueue
    {
        #region private fields
        /// <summary>the default threshold value for the requests in the memory before persisting to the storage.</summary>
        private const int DefaultThresholdForRequestsPersist = 100;

        /// <summary>the default threshold value for the responses in the memory before persisting to the storage.</summary>
        private const int DefaultThresholdForResponsesPersist = 100;

        /// <summary>the default milliseconds that the responses keep in the cache.</summary>
        private const int DefaultResponsesCacheTimeout = 15000;

        /// <summary>the quick cache size.</summary>
        private int quickCacheSize
        {
            get
            {
                return this.SharedData.DispatcherCount * 2 + BrokerQueueConstants.DefaultQuickCacheSize;
            }
        }

        /// <summary>the storage provider.</summary>
        private ISessionPersist sessionPersistField;

        /// <summary>the name of the storage provider.</summary>
        private string peristenceNameField;

        /// <summary>if the requests number in the memory exceed this number, then the requests should be persisted to the storage provider.</summary>
        private int thresholdForRequestsPersistField;

        /// <summary>if the responses number in the memory exceed this number, then the responses should be persisted to the storage provider.</summary>
        private int thresholdForResponsesPersistField;

        /// <summary>the memory cache for the requests, but will be persisted to the storage provider once the requests count exceed the threshold value.</summary>
        private volatile List<BrokerQueueItem> requestsListField;

        /// <summary>the memory cache for the requests that will be used to fill the dispatcher's cache.</summary>
        private ConcurrentBag<BrokerQueueItem> quickCacheRequestsBagField;

        /// <summary>the memory cache for the responses, but will be persisted to the storage provider once the responses count exceed the threshold value.</summary>
        private volatile List<BrokerQueueItem> responsesListField;

        /// <summary>the requests number that are peristed to the storage provider.</summary>
        private long pendingPeristRequestsCountField;

        /// <summary>the total requests that are persisted but pending for commit.</summary>
        private long persistedRequestsCount;

        /// <summary>the requests number that are peristed and commited to the storage provider.</summary>
        private long commitedRequestsCountField;

        /// <summary>number of requests in the persistence that are available for feteching.</summary>
        private long availableRequestsCountField;

        /// <summary>all the requests number in the queue.</summary>
        private long allRequestsCountField;

        /// <summary>all the requests in the queue.</summary>
        private long dispatchedRequestsCountField;

        /// <summary>number of responses that ever received</summary>
        private long allResponsesCountField;

        /// <summary>the responses number that are persisting to the storage provider.</summary>
        private int responsesInCacheTimoutField;

        /// <summary>the latest exception raised in the queue.</summary>
        private Exception latestPeristExceptionField;

        /// <summary>the event will signaled once all the requests are persisted to the storage provider.</summary>
        private AutoResetEvent allRequestsPeristedEvent;

        /// <summary>the event will signaled once one request come in.</summary>
        private AutoResetEvent requestArriveEvent = new AutoResetEvent(false);

        /// <summary>the lock object for requests list.</summary>
        private object lockRequestsListField = new object();

        /// <summary>the lock object for responses list.</summary>
        private object lockResponsesListField = new object();

        /// <summary>the lock object for quick requests list.</summary>
        private object lockQuickCacheRequestsListField = new object();

        /// <summary>dummy request context.</summary>
        private DummyRequestContext dummyRequestContextField = DummyRequestContext.GetInstance(MessageVersion.Default);

        /// <summary>a value indicating whether this queue is closed.</summary>
        private volatile bool isClosedField;

        /// <summary>a value indicating whether this queue is disposed.</summary>
        private bool isDisposedField;

        /// <summary>the requests dispatcher.</summary>
        private BrokerQueueDispatcher dispatcherField;

        /// <summary>the session id of the queue.</summary>
        private string sessionIdField;

        /// <summary>the client id of the queue.</summary>
        private string clientIdField;

        /// <summary>the timer used to persist the pending responses.</summary>
        private Timer persistTimerField;

        /// <summary> A queue that maintins pending GetRequestAsync calls.  Each RequestCallbackItem contains context of a pending GetRequestAsync call.</summary>
        private Queue<RequestCallbackItem> getRequestCallbackQueue = new Queue<RequestCallbackItem>();

        /// <summary> lock object for getRequestCallbackQueue </summary>
        private object lockGetRequestCallbackQueue = new object();

        /// <summary> request callback item currently being processed </summary>
        private RequestCallbackItem currentRequestCallbackItem;

        /// <summary> Currrent number of get request call back. </summary>
        private long currentGetRequestCallbackCount = 0;

        /// <summary> Lock for request available action. </summary>
        private object requestAvailableActionLock = new object();

        /// <summary> The flag indicates request available. </summary>
        private bool requestAvailableActionFlag = false;

        /// <summary> Currrent number of get response call back. </summary>
        private long currentGetResponseCallbackCount = 0;

        /// <summary> Lock for response available action. </summary>
        private object responseAvailableActionLock = new object();

        /// <summary> The flag indicates response available. </summary>
        private bool responseAvailableActionFlag = false;

        /// <summary> number of responses that have been fetched from persistence. </summary>
        private long fetchedResponsesCountField;

        /// <summary> number of responses that have been fetched from persistence but filtered out and not dispatched. </summary>
        private long filteredResponsesCountField;

        /// <summary> number of responses that have been fetched from persistence and disptached. </summary>
        private long dispatchedResponsesCountField;

        /// <summary> response callback item currently being processed </summary>
        private ResponseCallbackItem currentResponseCallbackItem;

        /// <summary> lock object for registeredResponseCallbackQueue, and currentResponseCallbackItem. </summary>
        private object lockResponseCallbackQueue = new object();

        /// <summary> A queue that maintains pending RegisterResponseCallbacks.  Each ResponseCallbackItem contains context of a pending RegisterResponseCallback call. </summary>
        private Queue<ResponseCallbackItem> registeredResponseCallbackQueue = new Queue<ResponseCallbackItem>();

        /// <summary> a flag indicating whether AllResponseDispatched event has been sent.  false: not sent, true: sent. </summary>        
        private bool isResponseDispatchedEventNotified;

        /// <summary> lock object for isResponseDispatchedEventNotified.</summary>
        private object lockIsResponseDispatchedEventNotified = new object();

        /// <summary> A queue that maintains cached request items. </summary>
        private ConcurrentQueue<BrokerQueueItem> cachedRequestQueue = new ConcurrentQueue<BrokerQueueItem>();

        /// <summary> lock object for cachedRequestQueue </summary>
        private object lockCachedRequestQueue = new object();

        /// <summary> number of outstanding GetResponse calls</summary>
        private int outstandingGetResponseCount;

        #endregion

        /// <summary>
        /// Initializes a new instance of the BrokerPersistQueue class to support persist broker queue.
        /// </summary>
        /// <param name="dispatcher">the requests dispatcher.</param>
        /// <param name="quickCacheSize">the size of the quick cache.</param>
        /// <param name="persistenceName">the persistence name.</param>
        /// <param name="sessionId">the session id of the queue.</param>
        /// <param name="clientId">the client id of the queue.</param>
        /// <param name="sessionPersist">the session persistence that will store the messages</param>
        /// <param name="thresholdForRequestPersist">when the request message count in the memory is bigger than this value, 
        /// then the request messages in the memeory should be persisted to the persistence.</param>
        /// <param name="thresholdForResponsePersist">when the response message count in the memory is bigger than this value, 
        /// then the response messages in the memeory should be persisted to the persistence.</param>
        /// <param name="responseTimeout">the timeout miliseconds for the processing message that wait for the corresponding response, 
        /// the elapsed time it great than this value then the original request will be dispatched again.</param>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="factory">indicating the broker queue factory</param>
        public BrokerPersistQueue(BrokerQueueDispatcher dispatcher, string persistenceName, string sessionId, string clientId, ISessionPersist sessionPersist, int thresholdForRequestPersist, int thresholdForResponsePersist, int responseTimeout, bool needQuickCache, SharedData sharedData, BrokerQueueFactory factory)
            : base(sharedData, factory)
        {
            ParamCheckUtility.ThrowIfNull(sessionPersist, "sessionPersist");
            this.sessionPersistField = sessionPersist;
            this.dispatcherField = dispatcher;
            if (dispatcher != null)
            {
                if (needQuickCache)
                {
                    this.quickCacheRequestsBagField = new ConcurrentBag<BrokerQueueItem>();
                }
            }

            //this.quickCacheSizeField = quickCacheSize;
            this.peristenceNameField = persistenceName;
            if (clientId == null)
            {
                clientId = string.Empty;
            }

            this.sessionIdField = sessionId;
            this.clientIdField = clientId;
            if (thresholdForRequestPersist <= 0)
            {
                thresholdForRequestPersist = BrokerPersistQueue.DefaultThresholdForRequestsPersist;
            }

            if (thresholdForResponsePersist <= 0)
            {
                thresholdForResponsePersist = BrokerPersistQueue.DefaultThresholdForResponsesPersist;
            }

            this.thresholdForRequestsPersistField = thresholdForRequestPersist;
            this.thresholdForResponsesPersistField = thresholdForResponsePersist;
            this.requestsListField = new List<BrokerQueueItem>();
            this.responsesListField = new List<BrokerQueueItem>();
            this.allRequestsPeristedEvent = new AutoResetEvent(false);
            this.commitedRequestsCountField = this.allRequestsCountField = sessionPersist.AllRequestsCount;
            this.availableRequestsCountField = sessionPersist.RequestsCount;
            this.allResponsesCountField = this.sessionPersistField.ResponsesCount;

            this.responsesInCacheTimoutField = responseTimeout;
            if (this.responsesInCacheTimoutField > 0 && this.responsesInCacheTimoutField < BrokerPersistQueue.DefaultResponsesCacheTimeout)
            {
                this.responsesInCacheTimoutField = BrokerPersistQueue.DefaultResponsesCacheTimeout;
            }

            if (this.responsesInCacheTimoutField != 0)
            {
                this.persistTimerField = new Timer(new ThreadHelper<object>(new TimerCallback(this.PersistResponsesTimerCallback)).CallbackRoot, null, Timeout.Infinite, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Finalizes an instance of the BrokerPersistQueue class.
        /// </summary>
        ~BrokerPersistQueue()
        {
            this.Dispose(false);
        }

        #region public properties

        /// <summary>
        /// Gets a value indicating whether all requests are received.
        /// </summary>
        public override bool EOMReceived
        {
            get
            {
                return this.sessionPersistField.EOMReceived;
            }
        }

        /// <summary>
        /// Gets a value indicating whether all the requests in the storage are processed.
        /// </summary>
        public override bool IsAllRequestsProcessed
        {
            get
            {
                var eomReceived = this.EOMReceived;
                var commitedRequestsCount = Interlocked.Read(ref this.commitedRequestsCountField);
                var requestsCount = this.sessionPersistField.RequestsCount;
                var allRequestsCount = this.sessionPersistField.AllRequestsCount;

                var res = eomReceived && commitedRequestsCount > 0 && requestsCount == 0 && allRequestsCount > 0;

                Debug.WriteLineIf(
                    !res,
                    $"(DEBUG)[{nameof(BrokerPersistQueue)}].{nameof(this.IsAllRequestsProcessed)} return false. EOMReceived={eomReceived}(==true), commitedRequestsCountField={commitedRequestsCount}(>0), RequestsCount={requestsCount}(==0), AllRequestsCount={allRequestsCount}(>0)");

                return res;
            }
        }

        /// <summary>
        /// Gets the total number of the requests that be persisted to the broker queue
        /// </summary>
        public override long AllRequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.allRequestsCountField);
            }
        }

        /// <summary>
        /// Gets the number of the requests that get the corresponding responses.
        /// </summary>
        public override long ProcessedRequestsCount
        {
            get
            {
                return this.sessionPersistField.ResponsesCount;
            }
        }

        /// <summary>
        /// Gets the number of the requests that are dispatched but still not get corresponding responses.
        /// </summary>
        public override long ProcessingRequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.dispatchedRequestsCountField) - this.sessionPersistField.ResponsesCount;
            }
        }

        /// <summary>
        /// Gets the number of the requests that are flushed to the queue.
        /// </summary>
        public override long FlushedRequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.commitedRequestsCountField);
            }
        }

        /// <summary>
        /// Gets the number of the requests that are flushed to the queue.
        /// </summary>
        public override long FailedRequestsCount
        {
            get
            {
                return this.sessionPersistField.FailedRequestsCount;
            }
        }

        /// <summary>
        /// Gets the session id of the queue.
        /// </summary>
        public override string SessionId
        {
            get
            {
                return this.sessionIdField;
            }
        }

        /// <summary>
        /// Gets the client id of the queue
        /// </summary>
        public override string ClientId
        {
            get
            {
                return this.clientIdField;
            }
        }

        /// <summary>
        /// Gets the user name of the broker queue.
        /// </summary>
        public override string UserName
        {
            get
            {
                return this.sessionPersistField.UserName;
            }
        }

        /// <summary>
        /// Gets the persist name of the queue.
        /// </summary>
        public override string PersistName
        {
            get
            {
                return this.peristenceNameField;
            }
        }
        #endregion

        /// <summary>
        /// Put the request item into the storage. and the storage will cache the requests in the memory 
        /// until the front end call the flush method. the async result will return the BrokerQueueItem.
        /// </summary>
        /// <param name="context">the request context relate to the message</param>
        /// <param name="msg">the request message</param>
        /// <param name="asyncState">the asyncState relate to the message</param>
        public override async Task PutRequestAsync(RequestContextBase context, Message msg, object asyncState)
        {
            if (this.isDisposedField)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .PutRequestAsync: clientId={0}, the persist queue is closed.", this.clientIdField);
                return;
            }

            ParamCheckUtility.ThrowIfNull(msg, "msg");

            BrokerQueueItem request = new BrokerQueueItem(context, msg, asyncState);

            if (this.quickCacheRequestsBagField?.Count < this.quickCacheSize)
            {
                Message messageCopy = request.MessageBuffer.CreateMessage();
                BrokerQueueItem cacheRequest = new BrokerQueueItem(context, messageCopy, request.PersistId, asyncState);
                cacheRequest.PersistAsyncToken.Queue = this;
                this.quickCacheRequestsBagField?.Add(cacheRequest);
            }

            // "thresholdForRequestsPersistField <= 1" suggests not persisting requests in batching way.
            // So put the request item into persistence directly.
            if (this.thresholdForRequestsPersistField <= 1)
            {
                Interlocked.Increment(ref this.allRequestsCountField);
                await this.PersistRequest(request);
                this.requestArriveEvent.Set();
                return;
            }

            List<BrokerQueueItem> requests = null;
            lock (this.lockRequestsListField)
            {
                this.requestsListField.Add(request);
                if (this.requestsListField.Count >= this.thresholdForRequestsPersistField)
                {
                    requests = this.requestsListField;
                    this.requestsListField = new List<BrokerQueueItem>();
                }
            }
            Interlocked.Increment(ref this.allRequestsCountField);

            if (requests != null)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "[BrokerPersistQueue] .PutRequestAsync (perf): clientId={0}, try to persist request.", this.clientIdField);
                await this.PersistRequests(requests);
            }

            this.requestArriveEvent.Set();
        }

        /// <summary>
        /// Fetch the requests one by one from the storage but not remove the original message in the storage.
        /// if reach the end of the storage, empty exception raised.this is async call by BrokerQueueCallback.
        /// the async result will return the request message
        /// </summary>
        /// <param name="requestCallback">the call back to retrieve the request message</param>
        /// <param name="state">the async state object</param>
        public override void GetRequestAsync(BrokerQueueCallback requestCallback, object state)
        {
            if (this.isDisposedField)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .GetRequestAsync: clientId={0}, the persist queue is closed.", this.clientIdField);
                return;
            }

            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "[BrokerPersistQueue] .GetRequestAsync: clientId={0}, new get request callback come in.", this.clientIdField);

            // Note: 1) GetRequestAsync calls are handled in a sequenctial way (one-by-one).  At a given 
            // time, there are at most one GetRequestAsync served.  2) Context of a GetRequestAsync call
            // is encapsulated into an RequestCallbackItem object. It contains all info about a 
            // GetRequestAsync call.  3) If a GetRequestAsync call can not be served immediately, its 
            // context (RequestCallbackItem) will be queued into getRequestCallbackQueue.
            RequestCallbackItem requestCallbackItem = new RequestCallbackItem(requestCallback, state);

            lock (this.lockGetRequestCallbackQueue)
            {
                if (this.currentRequestCallbackItem != null)
                {
                    this.getRequestCallbackQueue.Enqueue(requestCallbackItem);
                    return;
                }
                this.currentRequestCallbackItem = requestCallbackItem;
            }

            this.HandleCurrentRequestCallback();
        }

        /// <summary>
        /// Get next un-peeked request from persistence
        /// </summary>
        private void GetRequestFromPersistence()
        {
            this.sessionPersistField.GetRequestAsync(this.GetRequestFromPersistenceCallback, null);
        }

        /// <summary>
        /// Put the response into the storage, and delete corresponding request from the storage.
        /// the async result will return void.byt GetResult will throw exception if the response is not persisted into the persistence.
        /// </summary>
        /// <param name="responseMsg">the response message</param>
        /// <param name="requestItem">corresponding request item</param>
        public override async Task PutResponseAsync(Message responseMsg, BrokerQueueItem requestItem)
        {
            ParamCheckUtility.ThrowIfNull(requestItem, "persistAsyncToken");
            if (this.isDisposedField)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .PutResponseAsync: clientId={0}, the persist queue is closed.", this.clientIdField);

                // Bug 18829: If the incoming response message is null, the request processing
                // counter has already been reduced. So don't need to decrease it again.
                if (responseMsg != null)
                {
                    // When a request has been processed and the queue has been
                    // disposed, need to decrease the processing counter.
                    Debug.Assert(this.SharedData.Observer != null, "[BrokerPersistQueue] When there's a response putting back, the Observer instance must have been set already into the shareData instance according to the initialization order.");
                    this.SharedData.Observer.RequestProcessingCompleted();
                }

                return;
            }

            // response is null, redispatch its corresponding request
            if (responseMsg == null)
            {
                this.RedispatchRequest(requestItem);
                return;
            }

            BrokerQueueAsyncToken persistAsyncToken = requestItem.PersistAsyncToken;
            BrokerQueueItem response = new BrokerQueueItem(null, responseMsg, persistAsyncToken.AsyncState);
            response.DispatchNumber = persistAsyncToken.DispatchNumber + 1;
            response.TryCount = persistAsyncToken.TryCount;
            response.PersistAsyncToken.AsyncToken = persistAsyncToken.AsyncToken;
            response.PeerItem = requestItem;

            // "thresholdForResponsePersistField <= 1" suggests not persisting responses in batching way.
            // So put the response item into persistence directly.
            if (this.thresholdForResponsesPersistField <= 1)
            {
                Interlocked.Increment(ref this.allResponsesCountField);
                await this.PersistResponse(response);
                return;
            }

            if (this.responsesListField.Count == 0)
            {
                if (this.persistTimerField != null)
                {
                    this.persistTimerField.Change(this.responsesInCacheTimoutField, Timeout.Infinite);
                }
            }

            lock (this.lockResponsesListField)
            {
                this.responsesListField.Add(response);
            }

            long allResponsesCount = Interlocked.Increment(ref this.allResponsesCountField);
            List<BrokerQueueItem> responses = null;
            long committedRequestCount = Interlocked.Read(ref this.commitedRequestsCountField);

            if (allResponsesCount >= committedRequestCount
                || (this.responsesListField.Count >= this.thresholdForResponsesPersistField))
            {
                if (this.persistTimerField != null)
                {
                    this.persistTimerField.Change(Timeout.Infinite, Timeout.Infinite);
                }

                lock (this.lockResponsesListField)
                {
                    if (this.responsesListField.Count > 0)
                    {
                        responses = this.responsesListField;
                        this.responsesListField = new List<BrokerQueueItem>();
                    }
                }
            }
            else
            {
                BrokerTracing.TraceEvent(
                    System.Diagnostics.TraceEventType.Verbose,
                    0,
                    "[BrokerPersistQueue] .PutResponseAsync: allResponsesCount={0}, commitedRequestCount={1}, responsesListField.Count={2}, thresholdForResponsesPersistField={3}",
                    allResponsesCount,
                    committedRequestCount,
                    this.responsesListField.Count,
                    this.thresholdForResponsesPersistField);
            }

            if (responses != null)
            {
                BrokerTracing.TraceEvent(
                    System.Diagnostics.TraceEventType.Verbose,
                    0,
                    "[BrokerPersistQueue] .PutResponseAsync: clientId={0}, persist responses, allResponsesCount={1}, commitedRequestCount={2}, thresholdForResponsesPersistField={3}, responses count={4}",
                    this.clientIdField,
                    allResponsesCount,
                    committedRequestCount,
                    this.thresholdForResponsesPersistField,
                    responses.Count);

                await this.PersistResponses(responses);
            }

            return;
        }

        /// <summary>
        /// register a response call back to get the response message.
        /// </summary>
        /// <param name="responseCallback">the response callback, the async result should be the BrokerQueueItem</param>
        /// <param name="messageVersion">the message version for the response message. if failed to pull the response from the storage, will use this version to create a fault message.</param>
        /// <param name="filter">the filter for the responses that the response callback expected.</param>
        /// <param name="responseCount">the responses count this registered response callback want to get.</param>
        /// <param name="state">the state object for the callback</param>
        /// <param name="needRemove">if need remove the response message when the response message is callback successfully.</param>
        /// <returns>a value indicating whether the response callback is registered sucessfully.</returns>
        public override bool RegisterResponsesCallback(BrokerQueueCallback responseCallback, MessageVersion messageVersion, ResponseActionFilter filter, int responseCount, object state)
        {
            ParamCheckUtility.ThrowIfNull(responseCallback, "response callback");
            if (this.isDisposedField)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .RegisterResponsesCallback: clientId={0}, the persist queue is closed.", this.clientIdField);
                return false;
            }

            BrokerTracing.TraceVerbose("[BrokerPersistQueue] .RegisterResponsesCallback: resgister response callback come in.");
            if (this.IsAllRequestsProcessed
                && Interlocked.Read(ref this.fetchedResponsesCountField) >= this.sessionPersistField.ResponsesCount)
            {
                return false;
            }

            if (responseCount <= 0)
            {
                responseCount = int.MaxValue;
            }

            ResponseCallbackItem responseCallbackItem = new ResponseCallbackItem(responseCallback, messageVersion, filter, responseCount, state);

            lock (this.lockResponseCallbackQueue)
            {
                if (this.currentResponseCallbackItem != null && this.currentResponseCallbackItem.ExpectedResponseCount > 0)
                {
                    this.registeredResponseCallbackQueue.Enqueue(responseCallbackItem);
                    return true;
                }
                this.currentResponseCallbackItem = responseCallbackItem;
            }

            this.HandleCurrentResponseCallback();

            return true;
        }

        /// <summary>
        /// reset the current response callback, after this method call, 
        /// the following RegisterResponseCallback will get the responses from the beginning.
        /// </summary>
        public override void ResetResponsesCallback()
        {
            if (this.isDisposedField)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .ResetResponsesCallback: clientId={0}, the persist queue is closed.", this.clientIdField);
                return;
            }

            BrokerTracing.TraceVerbose("[BrokerPersistQueue] .ResetResponsesCallback: clientId={0}, reset current response callback", this.clientIdField);
            this.sessionPersistField.ResetResponsesCallback();

            if (this.SharedData.BrokerInfo.Durable)
            {
                this.fetchedResponsesCountField = 0;
            }
            else
            {
                this.fetchedResponsesCountField = this.dispatchedResponsesCountField;
            }

            lock (this.lockResponseCallbackQueue)
            {
                this.currentResponseCallbackItem = null;
                this.registeredResponseCallbackQueue.Clear();
            }
            this.isResponseDispatchedEventNotified = false;
        }

        /// <summary>
        /// Acknowledge if a response has been sent back to client successfully or not
        /// </summary>
        /// <param name="response">response item</param>
        /// <param name="success">if the response item has been sent back to client successfully or not</param>
        public override void AckResponse(BrokerQueueItem response, bool success)
        {
            this.sessionPersistField.AckResponse(response, success);
        }

        /// <summary>
        /// Acknowledge if a list of responses have been sent back to client successfully or not
        /// </summary>
        /// <param name="responses">response item list</param>
        /// <param name="success">if the list of response items have been sent back to client successfully or not</param>
        public override void AckResponses(List<BrokerQueueItem> responses, bool success)
        {
            foreach (BrokerQueueItem response in responses)
            {
                this.AckResponse(response, success);
            }
        }

        /// <summary>
        /// flush all the requests in the cache to the persistence
        /// </summary>
        /// <param name="msgCount">the message count that will be persisted to the persistence.</param>
        /// <param name="timeoutMs">the millisecond that the flush operation can wait, otherwise, thro TimeoutException.</param>
        /// <param name="endOfMessage">a value indicating end of messages</param>
        public override void Flush(long msgCount, int timeoutMs, bool endOfMessage)
        {
            BrokerTracing.TraceVerbose("[BrokerPersistQueue] .Flush (perf): Begin");

            if (this.isDisposedField)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .Flush: clientId={0}, the persist queue is closed.", this.clientIdField);
                return;
            }

            // Bug 16062: Throw exception if user is trying to call EndRequests() on an
            // newly created empty broker client
            if (endOfMessage && msgCount == 0 && this.allRequestsCountField == 0 && this.commitedRequestsCountField == 0)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_EOMReject_NoRequest, SR.Broker_EOMReject_NoRequest);
            }

            try
            {
                while (true)
                {
                    if (msgCount != this.allRequestsCountField - this.commitedRequestsCountField)
                    {
                        BrokerTracing.TraceVerbose("[BrokerPersistQueue] .Flush: clientId={0}, wait for the late requests. msgCount {1}, allCount {2}, committedCount {3}", this.clientIdField, msgCount, this.allRequestsCountField, this.commitedRequestsCountField);
                        if (!this.requestArriveEvent.WaitOne(timeoutMs, false))
                        {
                            BrokerTracing.TraceWarning(
                                "[BrokerPersistQueue] .Flush: clientId={0}, wait for incoming requests timeout within {1} milliseconds.",
                                this.clientIdField,
                                timeoutMs);
                            throw new TimeoutException("BrokerPersistQueue flush timeout for waiting for the incoming requests within " + timeoutMs.ToString(CultureInfo.InvariantCulture) + " milliseconds for client[clientId=" + this.clientIdField + "].");
                        }

                        BrokerTracing.TraceVerbose("[BrokerPersistQueue] .Flush: clientId={0}, has new requests come in.", this.clientIdField);
                    }
                    else
                    {
                        BrokerTracing.TraceVerbose(
                            "[BrokerPersistQueue] .Flush: clientId={0}, all the requests come in, begin flush the requests.",
                            this.clientIdField);
                        break;
                    }
                }

                List<BrokerQueueItem> requests = null;
                lock (this.lockRequestsListField)
                {
                    if (this.requestsListField.Count != 0)
                    {
                        requests = this.requestsListField;
                        this.requestsListField = new List<BrokerQueueItem>();
                    }
                }

                this.PersistRequests(requests).GetAwaiter().GetResult();

                // wait for all the requests persisted to the storage.
                while (Interlocked.Read(ref this.persistedRequestsCount) != msgCount)
                {
                    BrokerTracing.TraceVerbose("[BrokerPersistQueue] .Flush (perf): In the while loop.");

                    if (this.latestPeristExceptionField != null)
                    {
                        throw this.latestPeristExceptionField;
                    }

                    if (!this.allRequestsPeristedEvent.WaitOne(timeoutMs, false))
                    {
                        BrokerTracing.TraceWarning(
                                "[BrokerPersistQueue] .Flush: clientId={0}, wait for persisting requests timeout within {1} milliseconds.",
                                this.clientIdField,
                                timeoutMs);
                        throw new TimeoutException("BrokerPersistQueue flush timeout for waiting for persisting requests within " + timeoutMs.ToString(CultureInfo.InvariantCulture) + " milliseconds.");
                    }
                }

                if (endOfMessage)
                {
                    this.sessionPersistField.EOMReceived = true;
                }

                // Reset in case Flush is called again
                long newRequestsCount = Interlocked.Exchange(ref this.persistedRequestsCount, 0);

                if (!this.sessionPersistField.IsInMemory())
                {
                    BrokerTracing.TraceVerbose("[BrokerPersistQueue] .Flush (perf): Begin to commit persist transaction.");
                    this.sessionPersistField.CommitRequest();
                    BrokerTracing.TraceVerbose("[BrokerPersistQueue] .Flush (perf): End of committing persist transaction.");

                    Interlocked.Add(ref this.availableRequestsCountField, newRequestsCount);

                    // Notify pending RequestCallbackItems that there are new requests available
                    this.NotifyRequestAvailable();
                }
            }
            catch (TimeoutException)
            {
                // rethrow the timeout exception.
                throw;
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[BrokerPersistQueue] .Flush: clientId={0}, flush the requests failed, Exception:{1}", this.clientIdField, e.ToString());
                this.AbortPendingTransaction();

                throw;
            }
            finally
            {
                if (this.latestPeristExceptionField != null)
                {
                    // as latestPersistException has been thrown out, reset it
                    this.latestPeristExceptionField = null;

                    // and dipose request items in quick cache.
                    this.ClearQuickCache();
                }
            }

            ConcurrentBag<BrokerQueueItem> quickCacheList = this.quickCacheRequestsBagField;
            // set quickCache to null.  Requests received after flush will not be quick cached.
            this.quickCacheRequestsBagField = null;
            if (quickCacheList != null)
            {
                this.dispatcherField.AddBrokerQueue(this, quickCacheList);
                BrokerTracing.TraceVerbose("[BrokerPersistQueue] .Flush: clientId={0}, msgCount={1}, added quick cache list for {2} requests.", this.clientIdField, msgCount, quickCacheList.Count);
            }

            Interlocked.Add(ref this.commitedRequestsCountField, (int)msgCount);

            // check if all requests are processed
            if (this.IsAllRequestsProcessed)
            {
                this.DispatchEvent(BrokerQueueEventId.AllRequestsProcessed, new BrokerQueueEventArgs(this));
            }

            // check if all responses are dispatched
            bool hasResponseCallback = false;
            object currentResponseCallbackState = null;
            lock (this.lockResponseCallbackQueue)
            {
                if (this.currentResponseCallbackItem != null)
                {
                    currentResponseCallbackState = this.currentResponseCallbackItem.CallbackState;
                    hasResponseCallback = true;
                }
            }
            if (hasResponseCallback)
            {
                this.NotifyResponseDispatchedEvent(currentResponseCallbackState);
            }

            BrokerTracing.TraceVerbose("[BrokerPersistQueue] .Flush (perf): End");
        }

        /// <summary>
        /// Used only for interactive session to update the flush counters for persisted and committed 
        /// </summary>
        public override void FlushCount()
        {
            BrokerTracing.TraceVerbose("[BrokerPersistQueue] .FlushCount: persistedRequestsCount={0}, commitedRequestsCountField={1}.", this.persistedRequestsCount, this.commitedRequestsCountField);
            long persistedRequestsCountTemp = Interlocked.Exchange(ref this.persistedRequestsCount, 0);
            Interlocked.Add(ref this.commitedRequestsCountField, persistedRequestsCountTemp);
        }

        /// <summary>
        /// Discard unflushed requests. It works for durable session only.
        /// Note: this method is not thread safe. Counters may be incorrect if
        /// it is called at the same time with BrokerQueue.PutRequestAsync or
        /// BrokerQueue.Flush.
        /// </summary>
        public override void DiscardUnflushedRequests()
        {
            BrokerTracing.TraceVerbose("[BrokerPersistQueue] .DiscardUnflushedRequests.");

            if (this.isDisposedField)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .DiscardUnflushedRequests: clientId={0}, the persist queue is closed.", this.clientIdField);
                return;
            }

            if (this.allRequestsCountField == this.commitedRequestsCountField)
            {
                //no unflushed requests, return
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .DiscardUnflushedRequests: clientId={0}, no unflushed request.", this.clientIdField);
                return;
            }

            // 1. clear requests in quick cache
            this.ClearQuickCache();

            // 2. discard requests that have not been persisted
            this.DiscardInMemoryRequests();

            // 3. discard requests that have been put into persistence but not committed
            this.AbortPendingTransaction();
        }

        /// <summary>
        /// remove all the data in this queue.
        /// </summary>
        public override SessionPersistCounter Close()
        {
            SessionPersistCounter counter = null;
            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "[BrokerPersistQueue] .Close: clientId={0}, close the persist queue.", this.clientIdField);
            if (!this.isClosedField)
            {
                this.isClosedField = true;
                this.Dispose();
                counter = this.sessionPersistField.Close();
                counter.FlushedRequestsCount = this.FlushedRequestsCount;
            }

            return counter;
        }

        /// <summary>
        /// dispose the resource
        /// </summary>
        /// <param name="disposing">indicate whether remove the resources.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213", MessageId = "requestAvailableEvent", Justification = "requestAvailableEvent has been disposed by SafeDisposeObject.")]
        [SuppressMessage("Microsoft.Usage", "CA2213", MessageId = "responseAvailableEvent", Justification = "responseAvailableEvent has been disposed by SafeDisposeObject.")]
        protected override void Dispose(bool disposing)
        {
            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "[BrokerPersistQueue] .Dispose: clientId={0}, Dispose the persist queue.", this.clientIdField);
            if (!this.isDisposedField)
            {
                this.isDisposedField = true;
                if (this.dispatcherField != null)
                {
                    this.dispatcherField.RemoveBrokerQueue(this);
                }

                this.allRequestsPeristedEvent.Close();
                this.requestArriveEvent.Close();
                if (disposing)
                {
                    this.sessionPersistField.Dispose();
                    this.dummyRequestContextField.Close();
                    if (this.persistTimerField != null)
                    {
                        this.persistTimerField.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// the timer calllback to persist responses in the cache.
        /// </summary>
        /// <param name="state">the stat object.</param>
        private void PersistResponsesTimerCallback(object state)
        {
            if (this.isDisposedField)
            {
                return;
            }

            List<BrokerQueueItem> responses = null;
            lock (this.lockResponsesListField)
            {
                if (this.responsesListField.Count > 0)
                {
                    // in some rare conditions, the persist timer callback is triggered, but before the callback run to the lock (this.lockResponsesListField) statement,
                    // the cache threshold value also hit, and the responses in the cache are persisting to the message queue and at the same time, another response come in before persit timeout callback run to the lock statement.
                    // then the last response will be persisted to the message queue immediatly, but this does not cause any problem except some perf penalty.
                    this.persistTimerField.Change(Timeout.Infinite, Timeout.Infinite);
                    responses = this.responsesListField;
                    this.responsesListField = new List<BrokerQueueItem>();
                }
            }

            if (responses != null)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "[BrokerPersistQueue] .PersistResponsesTimerCallback: clientId={0}, persist responses. Responses count:{1}", this.clientIdField, responses.Count);
                Task.Run(() => this.PersistResponses(responses));
            }
        }

        /// <summary>
        /// persist the requests in the memory into the persistence.
        /// </summary>
        /// <param name="requests">the requests that will be persisted to the storage.</param>
        private async Task PersistRequests(List<BrokerQueueItem> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                return;
            }

            Interlocked.Add(ref this.pendingPeristRequestsCountField, requests.Count);
            await this.sessionPersistField.PutRequestsAsync(requests, this.FinishPersistRequestsCallback, requests.Count);
        }

        /// <summary>
        /// put one request into the persistence.
        /// </summary>
        /// <param name="request">the request to be persisted.</param>
        private async Task PersistRequest(BrokerQueueItem request)
        {
            System.Diagnostics.Debug.Assert(request != null, "request item cannot be null");

            Interlocked.Increment(ref this.pendingPeristRequestsCountField);
            await this.sessionPersistField.PutRequestAsync(request, this.FinishPersistRequestsCallback, 1);
        }

        /// <summary>
        /// persist the responses to the persistence in batch.
        /// </summary>
        /// <param name="responses">the responses</param>
        private async Task PersistResponses(List<BrokerQueueItem> responses)
        {
            if (responses == null || responses.Count == 0)
            {
                return;
            }

            await this.sessionPersistField.PutResponsesAsync(responses, this.FinishPersistResponsesCallback, responses.Count);
        }

        /// <summary>
        /// put one response into the persistence
        /// </summary>
        /// <param name="response">the response to be persisted</param>
        private async Task PersistResponse(BrokerQueueItem response)
        {
            System.Diagnostics.Debug.Assert(response != null, "response item cannot be null");

            await this.sessionPersistField.PutResponseAsync(response, this.FinishPersistResponsesCallback, 1);
        }

        /// <summary>
        /// the callback function for PutRequests once the async operation finished or exception raised.
        /// </summary>
        /// <param name="exception">the exception when the async operation hit exception.</param>
        /// <param name="state">the callback state object.</param>
        private void FinishPersistRequestsCallback(Exception exception, object state)
        {
            if (exception != null)
            {
                BrokerTracing.TraceEvent(
                    System.Diagnostics.TraceEventType.Error,
                    0,
                    "[BrokerPersistQueue] .FinishPersistRequestsCallback: clientId={0}, persist requests raise exception, {1}.",
                    this.clientIdField,
                    exception);
                this.OnException(new ExceptionEventArgs(exception, this));
                this.latestPeristExceptionField = exception;
                this.allRequestsPeristedEvent.Set();
            }
            else
            {
                int peristedCount = (int)state;
                Interlocked.Add(ref this.pendingPeristRequestsCountField, -peristedCount);
                long persistedRequestsCountTemp = Interlocked.Add(ref this.persistedRequestsCount, peristedCount);
                BrokerTracing.TraceEvent(
                    System.Diagnostics.TraceEventType.Verbose,
                    0,
                    "[BrokerPersistQueue] .FinishPersistRequestsCallback: clientId={0}, persist requests finished, there are {1} peristed requests waiting for flush.",
                    this.clientIdField,
                    persistedRequestsCountTemp);
                if (persistedRequestsCountTemp + Interlocked.Read(ref this.commitedRequestsCountField) >= Interlocked.Read(ref this.allRequestsCountField))
                {
                    this.allRequestsPeristedEvent.Set();
                }

                // There are new requests available, so handle pending RequestCallbackItem
                // Note: for durable session, pending RequestCallbackItem will be handled after transaction commit
                if (!this.SharedData.BrokerInfo.Durable)
                {
                    Interlocked.Add(ref this.availableRequestsCountField, peristedCount);
                    this.NotifyRequestAvailable();
                }
            }
        }

        /// <summary>
        /// the callback function for PutResponses once the async operation finished or exception raised.
        /// </summary>
        /// <param name="exception">the exception when the async operation hit exception.</param>
        /// <param name="state">the callback state object.</param>
        private void FinishPersistResponsesCallback(Exception exception, int responseCount, int faultResponseCount, bool isLastResponse, List<BrokerQueueItem> failedResponses, object state)
        {
            // notify that a number of responses have been successfully put into persistence.
            if (responseCount > 0 || faultResponseCount > 0)
            {
                this.OnPutResponsesSuccess(responseCount, faultResponseCount);
            }

            if (exception != null)
            {
                BrokerTracing.TraceEvent(
                    System.Diagnostics.TraceEventType.Error,
                    0,
                    "[BrokerPersistQueue] .FinishPersistResponsesCallback: clientId={0}, persist responses raise exception, {1}.",
                    this.clientIdField,
                    exception);
                this.OnException(new ExceptionEventArgs(exception, this));

                // If response cannot be persisted due to insufficient response, retry 3 times, then report error.
                BrokerQueueException bqException = exception as BrokerQueueException;
                if (bqException != null && bqException.ErrorCode == (int)BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_INSUFFICIENT)
                {
                    if (failedResponses.Count > 0 && failedResponses[0].DispatchNumber > this.SharedData.Config.LoadBalancing.MessageResendLimit)
                    {
                        this.OnFatalException(bqException);
                        return;
                    }
                }
            }
            else
            {
                BrokerTracing.TraceEvent(
                    System.Diagnostics.TraceEventType.Verbose,
                    0,
                    "[BrokerPersistQueue] .FinishPersistResponsesCallback: clientId={0}, succeeded: {1} responses persisted",
                    this.clientIdField,
                    responseCount);
                if (isLastResponse && this.IsAllRequestsProcessed)
                {
                    this.DispatchEvent(BrokerQueueEventId.AllRequestsProcessed, new BrokerQueueEventArgs(this));
                }
            }


            // redispatch failed responses
            if (failedResponses != null && failedResponses.Count > 0)
            {
                foreach (BrokerQueueItem failedResponse in failedResponses)
                {
                    using (failedResponse)
                    {
                        this.RedispatchRequest(failedResponse.PeerItem);
                        failedResponse.PeerItem = null;
                    }
                }
            }

            // notify that there are more responses available
            if (responseCount > 0)
            {
                this.NotifyResponseAvailable();
            }
        }

        /// <summary>
        /// Get next response from persistence
        /// </summary>
        /// <param name="responseCallbackItem">Callback item that contains get responses context.</param>
        private void GetResponseFromPersistence(ResponseCallbackItem responseCallbackItem)
        {
            this.sessionPersistField.GetResponseAsync(this.GetResponseFromPersistenceCallback, responseCallbackItem);
        }

        /// <summary>
        /// the callback for getting response.
        /// </summary>
        /// <param name="responsePersistMessage">the response message from the storage.</param>
        /// <param name="isAllResponsesDispatched">a value indicates whether all the responses are dispatched.</param>
        /// <param name="state">the state object for the callback.</param>
        /// <param name="exception">the exception that the async operation raised.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void GetResponseFromPersistenceCallback(BrokerQueueItem responsePersistMessage, object state, Exception exception)
        {
            //ParamCheckUtility.ThrowIfOutofRange(responsePersistMessage != null && isAllResponsesDispatched, "isAllResponsesDispatched");
            ParamCheckUtility.ThrowIfOutofRange(responsePersistMessage == null && exception == null, "responsePersistMessage shall not be null on success");

            ResponseCallbackItem responseCallbackItem = (ResponseCallbackItem)state;
            ParamCheckUtility.ThrowIfNull(responseCallbackItem, "resposneCallbackItem");

            bool needDispatch = true;
            if (exception != null)
            {
                Message faultMessage = FrontEndFaultMessage.GenerateFaultMessage(null, responseCallbackItem.MessageVersion, SOAFaultCode.Broker_BrokerQueueFailure, SR.BrokerQueueFailure);
                responsePersistMessage = new BrokerQueueItem(faultMessage, Guid.Empty, responseCallbackItem.CallbackState);
            }
            else
            {
                if (responseCallbackItem.ResponseFilter != null &&
                   !responseCallbackItem.ResponseFilter.IsQualified(responsePersistMessage.Message))
                {
                    Debug.WriteLine("[BrokerPersistQueue].GetResponseFromPersistenceCallback: response message is filtered out.");
                    //increment fetched response count after response being filtered
                    Interlocked.Increment(ref this.fetchedResponsesCountField);
                    Interlocked.Increment(ref this.filteredResponsesCountField);

                    // check if all responses are dispatched
                    this.NotifyResponseDispatchedEvent(responseCallbackItem.CallbackState);

                    this.AckResponse(responsePersistMessage, false);

                    needDispatch = false;
                }
                else
                {
                    responseCallbackItem.DecrementResponseCount();
                }
            }

            if (needDispatch)
            {
                try
                {
                    responseCallbackItem.Callback(responsePersistMessage, responseCallbackItem.CallbackState);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceInfo(
                        "[BrokerPersistQueue] .GetResponseFromPersistenceCallback: clientId={0}, response callback raised exception, {1}.  Put response back into queue",
                        this.clientIdField,
                        e);

                    // No need to decrement this.fetchedResponsesCountField even if dispatching is failed, since ResetResponseCallback
                    // has already been called.
                    // Interlocked.Decrement(ref this.fetchedResponsesCountField);

                    this.OnException(new ExceptionEventArgs(e, this));
                }
                finally
                {
                    //increment fetched response count after response being dispatched, successfully or not.
                    Interlocked.Increment(ref this.fetchedResponsesCountField);
                    Interlocked.Increment(ref this.dispatchedResponsesCountField);

                    this.NotifyResponseDispatchedEvent(responseCallbackItem.CallbackState);
                }
            }

            // reset outstandingGetResponseCount
            this.outstandingGetResponseCount = 0;

            // continue to handle current response callback
            this.HandleCurrentResponseCallback();
        }

        private void HandleCurrentResponseCallback()
        {
            // if there is already outstanding GetResponse call, return
            if (0 != Interlocked.CompareExchange(ref this.outstandingGetResponseCount, 1, 0))
            {
                return;
            }

            // if all responses have been fetched, wait here            
            if (Interlocked.Read(ref this.fetchedResponsesCountField) >= this.sessionPersistField.ResponsesCount)
            {
                // reset outstandingGetResponseCount
                this.outstandingGetResponseCount = 0;

                try
                {
                    bool runCallback = false;
                    lock (this.responseAvailableActionLock)
                    {
                        if (this.responseAvailableActionFlag)
                        {
                            this.responseAvailableActionFlag = false;
                            runCallback = true;
                        }
                        else
                        {
                            this.currentGetResponseCallbackCount++;
                        }
                    }

                    if (runCallback)
                    {
                        Task.Run(() => this.HandleCurrentResponseCallback());
                    }
                }
                catch (ArgumentNullException e)
                {
                    //swallow the ArgumentNullException for responseAvailableEvent could be null if the BrokerPersistQueue is disposed.
                    BrokerTracing.TraceInfo(
                        "[BrokerPersistQueue] .HandleCurrentResponseCallback responseAvailableEvent is null: {0}", e);
                }
                return;
            }

            ResponseCallbackItem responseCallbackItem = null;
            lock (this.lockResponseCallbackQueue)
            {
                // skip ResponseCallbackItem with ExpectedResponseCount == 0
                while (this.currentResponseCallbackItem == null || this.currentResponseCallbackItem.ExpectedResponseCount <= 0)
                {
                    // if there is no ResponseCallback registered, return
                    if (this.registeredResponseCallbackQueue.Count == 0)
                    {
                        // reset outstandingGetResponseCount
                        this.outstandingGetResponseCount = 0;
                        return;
                    }

                    this.currentResponseCallbackItem = this.registeredResponseCallbackQueue.Dequeue();
                }
                responseCallbackItem = this.currentResponseCallbackItem;
            }

            // get response from persistence
            this.GetResponseFromPersistence(responseCallbackItem);
        }

        /// <summary>
        /// Handle current RequestCallbackItem
        /// </summary>
        private void HandleCurrentRequestCallback()
        {
            // first try to handle from cached request queue
            if (this.HandleFromCachedRequestQueue())
            {
                return;
            }

            // then, handle from persistence.
            if (this.HandleFromPersistence())
            {
                return;
            }

            bool runCallback = false;
            lock (this.requestAvailableActionLock)
            {
                if (this.requestAvailableActionFlag)
                {
                    this.requestAvailableActionFlag = false;
                    runCallback = true;
                    BrokerTracing.TraceVerbose("[BrokerPersistQueue] .HandleCurrentRequestCallback requestAvailableAction triggered.");
                }
                else
                {
                    this.currentGetRequestCallbackCount++;
                    BrokerTracing.TraceVerbose($"[BrokerPersistQueue] .HandleCurrentRequestCallback callback count is increased to {this.currentGetRequestCallbackCount}");
                }
            }

            if (runCallback)
            {
                Task.Run(() => this.HandleCurrentRequestCallback());
            }
        }

        /// <summary>
        /// Process pending RequestCallbackItem in GetRequestCallbackQueue
        /// </summary>
        private void HandlePendingRequestCallback()
        {
            bool hasPendingRequestCallback = false;
            lock (this.lockGetRequestCallbackQueue)
            {
                if (this.getRequestCallbackQueue.Count > 0)
                {
                    this.currentRequestCallbackItem = this.getRequestCallbackQueue.Dequeue();
                    hasPendingRequestCallback = true;
                }
                else
                {
                    this.currentRequestCallbackItem = null;
                }
            }

            if (hasPendingRequestCallback)
            {
                this.HandleCurrentRequestCallback();
            }
        }

        /// <summary>
        /// Notify that there are new requests available
        /// </summary>
        private void NotifyRequestAvailable()
        {
            try
            {
                bool runCallback = false;
                lock (this.requestAvailableActionLock)
                {
                    if (this.currentGetRequestCallbackCount > 0)
                    {
                        this.currentGetRequestCallbackCount--;
                        this.requestAvailableActionFlag = false;
                        runCallback = true;
                    }
                    else
                    {
                        this.requestAvailableActionFlag = true;
                    }
                }

                if (runCallback)
                {
                    Task.Run(() => this.HandleCurrentRequestCallback());
                }
            }
            catch (NullReferenceException e)
            {
                // Swallow null reference exception when it is invoked
                // while the instance has been disposed.
                BrokerTracing.TraceError("[BrokerPersistQueue] NullReferenceException occured while setting requestAvailableEvent: {0}", e);
            }
            catch (ObjectDisposedException e)
            {
                // Swallow object disposed exception when it is invoked
                // while the instance has been disposed.
                BrokerTracing.TraceError("[BrokerPersistQueue] ObjectDisposedException occured while setting requestAvailableEvent: {0}", e);
            }
        }

        /// <summary>
        /// Notify that there are new responss available
        /// </summary>
        private void NotifyResponseAvailable()
        {
            try
            {
                bool runCallback = false;
                lock (this.responseAvailableActionLock)
                {
                    if (this.currentGetResponseCallbackCount > 0)
                    {
                        this.currentGetResponseCallbackCount--;
                        this.responseAvailableActionFlag = false;
                        runCallback = true;
                    }
                    else
                    {
                        this.responseAvailableActionFlag = true;
                    }
                }

                if (runCallback)
                {
                    Task.Run(() => this.HandleCurrentResponseCallback());
                }
            }
            catch (NullReferenceException e)
            {
                // Swallow null reference exception when it is invoked
                // while the instance has been disposed.
                BrokerTracing.TraceError("[BrokerPersistQueue] NullReferenceException occured while setting responseAvailableEvent: {0}", e);
            }
            catch (ObjectDisposedException e)
            {
                // Swallow object disposed exception when it is invoked
                // while the instance has been disposed.
                BrokerTracing.TraceError("[BrokerPersistQueue] ObjectDisposedException occured while setting responseAvailableEvent: {0}", e);
            }
        }

        /// <summary>
        /// Dispatch a request that is cachedRequestQueue
        /// </summary>
        /// <returns>If there is request cached, return ture; or else, return false.</returns>
        private bool HandleFromCachedRequestQueue()
        {
            BrokerQueueItem cachedRequest = null;
            if (!this.cachedRequestQueue.TryDequeue(out cachedRequest))
            {
                return false;
            }

            if (cachedRequest == null)
            {
                return false;
            }

            this.GetRequestComplete(cachedRequest);
            return true;
        }

        /// <summary>
        /// Try to get request from persistence, and serve RequestCallbackItem.
        /// </summary>
        private bool HandleFromPersistence()
        {
            // if there is no more request available for fetching in the persistence, return
            if (Interlocked.Read(ref this.availableRequestsCountField) <= 0)
            {
                return false;
            }
            this.GetRequestFromPersistence();
            return true;
        }

        /// <summary>
        /// Redispatch a request item
        /// </summary>
        /// <param name="requestItem">the request item to be redispatched</param>
        private void RedispatchRequest(BrokerQueueItem requestItem)
        {
            BrokerTracing.TraceVerbose("[BrokerPersistQueue] .RedispatchRequest: redispatch request. clientid={0}, dispatchNumber = {1}.", this.clientIdField, requestItem.DispatchNumber);

            Interlocked.Decrement(ref this.dispatchedRequestsCountField);

            requestItem.PrepareMessage();
            requestItem.DispatchNumber++;

            this.CacheRequestForDispatching(requestItem);
        }

        /// <summary>
        /// Cache a request item for dispatching.
        /// </summary>
        /// <param name="requestItem">the request item to be cached</param>
        private void CacheRequestForDispatching(BrokerQueueItem requestItem)
        {
            this.cachedRequestQueue.Enqueue(requestItem);

            // Notify that there are more requests available.
            this.NotifyRequestAvailable();
        }

        /// <summary>
        /// the callback for getting request.
        /// </summary>
        /// <param name="requestPersistMessage">the request message from the storage.</param>
        /// <param name="isAllRequestsDispathed">a value indicates whether all the requests are dispatched.</param>
        /// <param name="state">the state object for the callback.</param>
        /// <param name="exception">the exception that the async operation raised.</param>
        private void GetRequestFromPersistenceCallback(BrokerQueueItem requestPersistMessage, object state, Exception exception)
        {
            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "[BrokerPersistQueue] .GetRequestFromPersistenceCallback: clientId={0}, get request from persistence callback.", this.clientIdField);

            // Consider 4 combinations of requestPersistMessage and exception:
            // 1) requestPersistMessage == null, exception == null:  there is no request available in the queue.  (when should this happen?)
            // 2) requestPersistMessage == null, exception != null:  GetRequest operation fails.
            // 3) requestPersistMessage != null, exception == null:  GetRequest operation succeeds.
            // 4) requestPersistMessage != null, exception != null:  invalid combination.
            ParamCheckUtility.ThrowIfOutofRange(requestPersistMessage != null && exception != null, "Both requestPersitMessage and exception returned");

            if (exception != null)
            {
                BrokerTracing.TraceEvent(
                    System.Diagnostics.TraceEventType.Error,
                    0,
                    "[BrokerPersistQueue] .GetRequestFromPersistenceCallback: clientId={0}, get persist request raise exception, {1}.",
                    this.clientIdField,
                    exception);
                this.OnException(new ExceptionEventArgs(exception, this));
            }
            else if (requestPersistMessage == null)
            {
                BrokerTracing.TraceEvent(
                    System.Diagnostics.TraceEventType.Warning,
                    0,
                    "[BrokerPersistQueue] .GetRequestFromPersistenceCallback: clientId={0}, requestPersistMessage is null.",
                    this.clientIdField);
            }

            Interlocked.Decrement(ref this.availableRequestsCountField);
            this.GetRequestComplete(requestPersistMessage);
        }

        /// <summary>
        /// Get request from persistence completed.  Invoke BrokerQueueCallback
        /// </summary>
        /// <param name="requestMessage">request message retrived back</param>
        private void GetRequestComplete(BrokerQueueItem requestMessage)
        {
            bool needRedispatch = false;
            try
            {
                if (requestMessage == null)
                {
                    return;
                }

                requestMessage.PersistAsyncToken.Queue = this;
                this.currentRequestCallbackItem.RequestCallback(requestMessage, this.currentRequestCallbackItem.CallbackState);

                long commitedRequestsCount = Interlocked.Read(ref this.commitedRequestsCountField);
                long dispatchedRequestsCount = Interlocked.Increment(ref this.dispatchedRequestsCountField);
                if (commitedRequestsCount > 0 && dispatchedRequestsCount >= commitedRequestsCount)
                {
                    BrokerTracing.TraceVerbose("[BrokerPersistQueue] .GetRequestComplete: all requests dispatched.");
                    this.DispatchEvent(BrokerQueueEventId.AllRequestsDispatched, new BrokerQueueEventArgs(this));
                }
            }
            catch (ObjectDisposedException e)
            {
                BrokerTracing.TraceError(
                    "[BrokerPersistQueue] .GetRequestComplete: clientId={0}, request callback raised exception, {0}.",
                    this.clientIdField,
                    e);
                needRedispatch = true;
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[BrokerPersistQueue] .GetRequestComplete: clientId={0}, request callback raised exception, {1}.",
                    this.clientIdField,
                    e);
                this.OnException(new ExceptionEventArgs(e, this));
                needRedispatch = true;
            }
            finally
            {
                // if failed to callback request, redispatch this request
                if (needRedispatch)
                {
                    this.CacheRequestForDispatching(requestMessage);
                }

                this.HandlePendingRequestCallback();
            }
        }

        /// <summary>
        /// A helper function for notifying response dispatching status
        /// </summary>
        /// <param name="responseCallbackState">the ResponseCallbackStat that trigger the notification</param>
        private void NotifyResponseDispatchedEvent(object responseCallbackState)
        {
            bool allRequestsProcessed = this.IsAllRequestsProcessed;
            long peekedResponseCount = Interlocked.Read(ref this.fetchedResponsesCountField);
            long responseCount = this.sessionPersistField.ResponsesCount;
            BrokerTracing.TraceEvent(
                System.Diagnostics.TraceEventType.Verbose,
                0,
                "[BrokerPersistQueue] .NotifyResponseDispatchedEvent: clientId={0}, peekedResponseCount={1}, responseCount={2}",
                this.clientIdField,
                peekedResponseCount,
                responseCount);

            if (peekedResponseCount >= responseCount)
            {
                // Note 1 - invoke "IsAllRequestsProcessed before "persistence.ResponsesCount".  Invoking them in the reverse order will introduce
                // race condition when persistence.ResponseCount and PersistResponses are invoked concurrently, and causes AllResponseDispatchedEvent
                // being sent out by mistake.
                // Scenario: persitence.ResponseCount == 10, persitence.RequestCount == 5, peekedResponseCount == 10.  At the same time, PersistResponses
                // is persiting the remaining 5 responses.  Expression "peekedResponseCount >= responseCount" is evaluated to true.  
                // Now PersistResponses is done, and persitence counters are updated: persitence.ResponseCount == 15, persistence.RequestCount == 0,
                // peekedResponseCount == 10.  At this time, "IsAllRequestsProcessed" return true.  AllResponseDispatched event is sent out. However, 
                // there are actually 5 responses not dispatched.
                // Note 2 - On completion of ISessionPersist.PersistResponses, update persitence.ResponseCount before persitence.RequestsCount.
                // Note 3 - FIXME!

                BrokerTracing.TraceEvent(
                    System.Diagnostics.TraceEventType.Verbose,
                    0,
                    "[BrokerPersistQueue] .NotifyResponseDispatchedEvent: clientId={0}, allRequestsProcessed={1}, isResponseDispatchedEventNotified={2}",
                    this.clientIdField,
                    allRequestsProcessed,
                    this.isResponseDispatchedEventNotified);

                if (allRequestsProcessed)
                {
                    if (!this.isResponseDispatchedEventNotified)
                    {
                        bool shouldDispatch = false;
                        lock (this.lockIsResponseDispatchedEventNotified)
                        {
                            if (!this.isResponseDispatchedEventNotified)
                            {
                                this.isResponseDispatchedEventNotified = true;
                                shouldDispatch = true;
                            }
                        }

                        BrokerTracing.TraceEvent(
                            System.Diagnostics.TraceEventType.Verbose,
                            0,
                            "[BrokerPersistQueue] .NotifyResponseDispatchedEvent: clientId={0}, isResponseDispatchedEventNotified={1}, shouldDispatch={2}",
                            this.clientIdField,
                            this.isResponseDispatchedEventNotified,
                            shouldDispatch);

                        if (shouldDispatch)
                        {
                            BrokerTracing.TraceVerbose("[BrokerPersistQueue] .NotifyResponseDispatchedEvent: all responses dispatched.");
                            this.DispatchEvent(BrokerQueueEventId.AllResponesDispatched, new ResponseEventArgs(responseCallbackState, this));
                        }
                    }
                }
                else
                {
                    // dispatch the AvailableResponsesDispatched event to let the outstanding Getresponses
                    // get a fault response once the broker failed and the requests are not processed completely.
                    BrokerTracing.TraceVerbose("[BrokerPersistQueue] .NotifyResponseDispatchedEvent: all available responses dispatched.");
                    this.DispatchEvent(BrokerQueueEventId.AvailableResponsesDispatched, new ResponseEventArgs(responseCallbackState, this));
                }
            }
        }

        /// <summary>
        /// Discard requests residing in memory
        /// </summary>
        private void DiscardInMemoryRequests()
        {
            int count = 0;
            lock (this.lockRequestsListField)
            {
                count = this.requestsListField.Count;
                if (count == 0)
                {
                    return;
                }

                foreach (BrokerQueueItem request in this.requestsListField)
                {
                    request.Dispose();
                }
                this.requestsListField.Clear();
            }

            // decrease "allRequestsCountField"
            Interlocked.Add(ref this.allRequestsCountField, -count);
            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .DiscardInMemoryRequests: clientId={0}, {1} requests discarded.", this.clientIdField, count);
        }

        /// <summary>
        /// Discard requests that have been put into peristence but not committed
        /// </summary>
        private void AbortPendingTransaction()
        {
            if (!this.sessionPersistField.IsInMemory())
            {
                // abort the transaction
                this.sessionPersistField.AbortRequest();

                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "[BrokerPersistQueue] .AbortPendingTransaction: clientId={0}, {1} requests discarded.", this.clientIdField, this.persistedRequestsCount);

                // decrease "allRequestsCountField"
                Interlocked.Add(ref this.allRequestsCountField, -this.persistedRequestsCount);

                // set persistedRequestsCount to 0
                Interlocked.Exchange(ref this.persistedRequestsCount, 0);
            }
        }

        /// <summary>
        /// Discards requests in quick cache
        /// </summary>
        private void ClearQuickCache()
        {
            ConcurrentBag<BrokerQueueItem> quickCacheList = this.quickCacheRequestsBagField;
            this.quickCacheRequestsBagField = new ConcurrentBag<BrokerQueueItem>();

            if (quickCacheList != null)
            {
                BrokerQueueItem requestItem = null;
                while (quickCacheList.TryTake(out requestItem))
                {
                    requestItem?.Dispose();
                }
            }
        }

        /// <summary>
        /// the request callback item
        /// </summary>
        private class RequestCallbackItem
        {
            #region private fields
            /// <summary>
            /// the request callback field.
            /// </summary>
            private BrokerQueueCallback requestCallbackField;

            /// <summary>
            /// the request callback state object.
            /// </summary>
            private object stateField;
            #endregion

            /// <summary>
            /// Initializes a new instance of the RequestCallbackItem class.
            /// </summary>
            /// <param name="requestCallback">the request callback.</param>
            /// <param name="state">the request callback state.</param>
            public RequestCallbackItem(BrokerQueueCallback requestCallback, object state)
            {
                if (requestCallback == null)
                {
                    throw new ArgumentNullException("requestCallback");
                }

                this.requestCallbackField = requestCallback;
                this.stateField = state;
            }

            /// <summary>
            /// Gets the request callback.
            /// </summary>
            public BrokerQueueCallback RequestCallback
            {
                get
                {
                    return this.requestCallbackField;
                }
            }

            /// <summary>
            /// Gets the request callback state object.
            /// </summary>
            public object CallbackState
            {
                get
                {
                    return this.stateField;
                }
            }
        }
    }
}
