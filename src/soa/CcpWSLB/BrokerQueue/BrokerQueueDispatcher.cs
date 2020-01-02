// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.Session.Common;

    /// <summary>
    /// the broker queue dispatcher.
    /// </summary>
    internal class BrokerQueueDispatcher
    {
        #region private fields

        /// <summary>
        /// the broker queue list that are waiting for dispatching requests.
        /// </summary>
        private List<BrokerQueue> waitBrokerQueuesForPeekRequests = new List<BrokerQueue>();

        /// <summary>
        /// the cache queue for the prefectched requests.
        /// </summary>
        private ConcurrentQueue<BrokerQueueItem> requestCacheQueue = new ConcurrentQueue<BrokerQueueItem>();

        /// <summary>
        /// pending requests callback queue.
        /// </summary>
        private ConcurrentQueue<BrokerQueueCallbackItem> requestCallbackQueue = new ConcurrentQueue<BrokerQueueCallbackItem>();

        /// <summary>
        /// a value indicates whether need prefetch the requests from the broker queues.
        /// </summary>
        private bool needPrefetch = true;

        /// <summary>
        /// the cache container size
        /// </summary>
        private int cacheContainerSize = 500;

        /// <summary>
        /// the low water marker to trigger prefetching requests.
        /// </summary>
        private int watermarkerForTriggerPrefetch = 200;

        /// <summary>
        /// the hash table for the requests that do not contain the AsyncToken.
        /// </summary>
        private Dictionary<Guid, DispatcherAsyncTokenItem> requestsAsyncTokenTable = new Dictionary<Guid, DispatcherAsyncTokenItem>();

        /// <summary>
        /// the hash table the responses that do not contain the async token.
        /// </summary>
        private Dictionary<Guid, DispatcherAsyncTokenItem> responsesWithoutAsyncTokenTable = new Dictionary<Guid, DispatcherAsyncTokenItem>();

        /// <summary>
        /// the client id table for the queues that need dispatch the requests.
        /// Client id is case insensitive
        /// </summary>
        private Dictionary<string, object> needDispatchQueueClientIdTable = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Stores the callback to call GetRequestAsync when triggering perfetch
        /// </summary>
        private WaitCallback waitCallbackToGetRequestAsync;

        /// <summary>
        /// Stores the shared data
        /// </summary>
        private SharedData sharedData;

        #endregion

        /// <summary>
        /// Initializes a new instance of the BrokerQueueDispatcher class.
        /// </summary>
        /// <param name="sessionId">the session id.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public BrokerQueueDispatcher(string sessionId, bool isDurable, SharedData sharedData)
        {
            this.sharedData = sharedData;

            // BUG FIX 3376 : Dont prefetch in non-durable queues. 1 - Unnecessary requests are processed after
            // brokerqueue is closed. 2 - Prefetch from memory queue adds unnecessary work
            if (!isDurable)
            {
                this.cacheContainerSize = 0;
                this.watermarkerForTriggerPrefetch = 0;
            }

            this.waitCallbackToGetRequestAsync = new ThreadHelper<object>(new WaitCallback(this.WaitCallbackToGetRequestAsync)).CallbackRoot;
        }

        /// <summary>
        /// Fetch the requests one by one from the storage but not remove the original message in the storage.
        /// if reach the end of the storage, empty exception raised.this is async call by BrokerQueueCallback.
        /// the async result will return the request message
        /// </summary>
        /// <param name="requestCallback">the call back to retrieve the request message</param>
        /// <param name="state">the async state object</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void GetRequestAsync(BrokerQueueCallback requestCallback, object state)
        {
            ParamCheckUtility.ThrowIfNull(requestCallback, "requestCallback");
            bool isCallbackHandled = false;
            if (this.requestCacheQueue.Count > 0)
            {
                BrokerQueueItem request = null;

                if (!this.requestCacheQueue.TryDequeue(out request))
                {
                    BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .GetRequestAsync: All cached requests are dispatched.");
                }

                if (request != null)
                {
                    try
                    {
#if DEBUG
                        BrokerTracing.TraceVerbose("[BrokerQueueDispatcher] .GetRequestAsync: handle request callback from cache queue. request id={0}", GetMessageId(request));
#endif

                        this.RegisterReemit(request);

                        isCallbackHandled = true;
                        requestCallback(request, state);
                    }
                    catch (Exception e)
                    {
                        this.requestCacheQueue.Enqueue(request);

                        // request callback raise exception.
                        BrokerTracing.TraceWarning("[BrokerQueueDispatcher] .GetRequestAsync: The request callback raise exception. Exception: {0}", e);
                    }
                }
            }

            if (this.requestCacheQueue.Count <= this.watermarkerForTriggerPrefetch)
            {
                BrokerTracing.TraceVerbose("[BrokerQueueDispatcher] .GetRequestAsync (perf): Trigger prefetch because the count of cached items is below threshold. state={0}", (int)state);
                this.TriggerPrefetch();
            }

            if (!isCallbackHandled)
            {
                this.requestCallbackQueue.Enqueue(new BrokerQueueCallbackItem(requestCallback, state));

                // in case the last request come in the cache queue right before the request callback enqueue.
                this.HandlePendingRequestCallback();
            }
        }

        private void RegisterReemit(BrokerQueueItem request)
        {
            // Only re-emit when message can be resent.
            if (this.sharedData.Config.LoadBalancing.MessageResendLimit > 0)
            {
                request.RegisterReemit(
                    TimeSpan.FromMilliseconds(this.sharedData.Config.LoadBalancing.MultiEmissionDelayTime),
                    this.sharedData.Config.LoadBalancing.MessageResendLimit,
                    clonedRequest => 
                    { 
                        this.PutResponseAsync(null, clonedRequest, true).GetAwaiter().GetResult();
                        this.sharedData.Observer.RequestReemit();
                    }); 
            }
        }

        /// <summary>
        /// Put the response into the storage, and delete corresponding request from the storage.
        /// the async result will return void.byt GetResult will throw exception if the response is not persisted into the persistence.
        /// </summary>
        /// <param name="responseMsg">the response message</param>
        /// <param name="requestItem">corresponding request item</param>
        /// <param name="ignoreAsyncToken">true to ignore the async token process from the table</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public async Task PutResponseAsync(Message responseMsg, BrokerQueueItem requestItem, bool ignoreAsyncToken = false)
        {
            try
            {
                // Null means the request has been put back.
                if (responseMsg != null && this.sharedData.Config.LoadBalancing.MessageResendLimit > 0)
                {
                    if (!requestItem.ReemitToken.Finish())
                    {
                        TraceUtils.TraceWarning(
                            "BrokerQueueDispatcher",
                            "PutResponseAsync",
                            "Drop the response {0} since no multi emission callback registered",
                            Utility.GetMessageIdFromMessage(requestItem.Message));

                        return;
                    }
                }

                BrokerQueueAsyncToken asyncToken = requestItem.PersistAsyncToken;
                if (asyncToken.AsyncToken != null || ignoreAsyncToken || responseMsg == null)
                {
                    await asyncToken.Queue.PutResponseAsync(responseMsg, requestItem);
                    return;
                }

                DispatcherAsyncTokenItem asyncTokenItem = null;
                lock (this.requestsAsyncTokenTable)
                {
                    this.requestsAsyncTokenTable.TryGetValue(asyncToken.PersistId, out asyncTokenItem);
                }

                if (asyncTokenItem != null)
                {
                    bool hasGetAsyncToken = false;
                    try
                    {
                        if (asyncTokenItem != null)
                        {
                            if (asyncTokenItem.AsyncToken != null)
                            {
                                asyncToken.AsyncToken = asyncTokenItem.AsyncToken;
                                hasGetAsyncToken = true;

                                lock (this.requestsAsyncTokenTable)
                                {
                                    this.requestsAsyncTokenTable.Remove(asyncToken.PersistId);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (hasGetAsyncToken)
                        {
                            BrokerTracing.TraceWarning("[BrokerQueueDispatcher] .PutResponseAsync: remove the async token from the table raised the exception, {0}", e);
                        }
                        else
                        {
                            // maybe there are duplicate responses, and the request in the async token table is removed by the previous request.
                            BrokerTracing.TraceWarning("[BrokerQueueDispatcher] .PutResponseAsync: try to get the async token from the table raised the exception, {0}", e);
                        }
                    }

                    if (hasGetAsyncToken)
                    {
                        await asyncToken.Queue.PutResponseAsync(responseMsg, requestItem);
                    }
                    else
                    {
                        BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .PutResponseAsync: Failed to get async token.");

                        lock (this.responsesWithoutAsyncTokenTable)
                        {
                            this.responsesWithoutAsyncTokenTable.Add(asyncToken.PersistId, new DispatcherAsyncTokenItem(responseMsg, asyncToken));
                        }

                        if (asyncTokenItem.AsyncToken != null)
                        {
                            bool needPutToTheQueue = false;
                            try
                            {
                                lock (this.responsesWithoutAsyncTokenTable)
                                {
                                    this.responsesWithoutAsyncTokenTable.Remove(asyncToken.PersistId);
                                }

                                needPutToTheQueue = true;

                                lock (this.requestsAsyncTokenTable)
                                {
                                    this.requestsAsyncTokenTable.Remove(asyncToken.PersistId);
                                }
                            }
                            catch (Exception e)
                            {
                                if (needPutToTheQueue)
                                {
                                    BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .PutResponseAsync: remove the request async token from the table failed, the exception: {0}", e);
                                }
                                else
                                {
                                    // in case the reponse message is persisted by another thread.
                                    BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .PutResponseAsync: remove the response from the responses without async table fail, the exception: {0}", e);
                                }
                            }

                            if (needPutToTheQueue)
                            {
                                asyncToken.AsyncToken = asyncTokenItem.AsyncToken;
                                await asyncToken.Queue.PutResponseAsync(responseMsg, requestItem);
                            }
                            else
                            {
                                BrokerTracing.TraceWarning(
                                    "[BrokerQueueDispatcher] .PutResponseAsync: Don't put response back because needPutToTheQueue=false. AsyncToken.PersistId={0}, requestItem id: {1}, response id:{2}",
                                    asyncToken.PersistId,
                                    requestItem?.Message?.Headers?.MessageId,
                                    responseMsg?.Headers?.MessageId);
                            }
                        }
                        else
                        {
                            // the request item is processed and its response is returned, but its async token is unknown yet.  At this point, as corresponding DispatcherAsyncToken item is 
                            // already put into responsesWithoutAsyncTokenTable, the request item itself is of no use now. so dispose it.
                            BrokerTracing.TraceInfo(
                                "[BrokerQueueDispatcher] .PutResponseAsync: Dispose requestItem id: {0}, response id:{1}",
                                requestItem?.Message?.Headers?.MessageId,
                                responseMsg?.Headers?.MessageId);

                            requestItem.Dispose();
                        }
                    }
                }
                else
                {
                    BrokerTracing.TraceError("[BrokerQueueDispatcher] .PutResponseAsync: can not find the async token.");
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[BrokerQueueDispatcher] .PutResponseAsync: unkown exception, {0}", e);
            }
        }

        /// <summary>
        /// add a broker queue to the dispatcher.
        /// </summary>
        /// <param name="queue">the broker queue.</param>
        /// <param name="requests">the requests for quick cache.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void AddBrokerQueue(BrokerQueue queue, IEnumerable<BrokerQueueItem> requests)
        {
            if (queue != null)
            {
                if (requests != null)
                {
                    foreach (BrokerQueueItem request in requests)
                    {
                        if (request != null)
                        {
                            if (request.PersistAsyncToken.AsyncToken == null)
                            {
                                lock (this.requestsAsyncTokenTable)
                                {
                                    if (!this.requestsAsyncTokenTable.ContainsKey(request.PersistId))
                                    {
                                        this.requestsAsyncTokenTable.Add(request.PersistId, new DispatcherAsyncTokenItem());
                                    }
                                    else
                                    {
                                        BrokerTracing.TraceError("[BrokerQueueDispatcher] .AddBrokerQueue: There are duplicate persist id, {0}", request.PersistId);
                                    }
                                }
                            }

                            this.requestCacheQueue.Enqueue(request);
                        }
                    }
                }

                bool containsQueue;
                lock (this.needDispatchQueueClientIdTable)
                {
                    containsQueue = this.needDispatchQueueClientIdTable.ContainsKey(queue.ClientId);
                }

                if (!containsQueue)
                {
                    bool needDispatch = true;
                    try
                    {
                        lock (this.needDispatchQueueClientIdTable)
                        {
                            this.needDispatchQueueClientIdTable.Add(queue.ClientId, null);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        needDispatch = false;

                        // the key already exists in the table.
                        BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .AddBrokerQueue: the broker queue with client id, {0}, already exists, Exception: {1}", queue.ClientId, e);
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceWarning("[BrokerQueueDispatcher] .AddBrokerQueue: add a broker queue with client id, {0}, to the client table raised exception, Exception: {1}", queue.ClientId, e);
                    }

                    if (needDispatch)
                    {
                        if (this.needPrefetch)
                        {
                            queue.GetRequestAsync(this.PrefetchRequestCallback, queue);
                        }
                        else
                        {
                            lock (this.waitBrokerQueuesForPeekRequests)
                            {
                                this.waitBrokerQueuesForPeekRequests.Add(queue);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// remove the broker queue from the dispatcher.
        /// </summary>
        /// <param name="queue">the broker queue.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void RemoveBrokerQueue(BrokerQueue queue)
        {
            if (queue != null)
            {
                bool containsQueue;
                lock (this.needDispatchQueueClientIdTable)
                {
                    containsQueue = this.needDispatchQueueClientIdTable.ContainsKey(queue.ClientId);
                }

                if (containsQueue)
                {
                    try
                    {
                        lock (this.needDispatchQueueClientIdTable)
                        {
                            this.needDispatchQueueClientIdTable.Remove(queue.ClientId);
                        }
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .RemoveBrokerQueue: Remove the broker queue with the client id, {0}, from the client table raised exception, {1}", queue.ClientId, e);
                    }
                }

                lock (this.waitBrokerQueuesForPeekRequests)
                {
                    this.waitBrokerQueuesForPeekRequests.Remove(queue);
                }
            }
        }

        /// <summary>
        /// Retrieve message id from a BrokerQueueItem instance
        /// </summary>
        /// <param name="messageItem">the BrokerQueueItem instance</param>
        /// <returns>message id of the message represented by the BrokerQueueItem instance</returns>
        private static string GetMessageId(BrokerQueueItem messageItem)
        {
            UniqueId messageId = SoaHelper.GetMessageId(messageItem.Message);

            return messageId == null ? string.Empty : messageId.ToString();
        }

        /// <summary>
        /// the callback for prefetch requests.
        /// </summary>
        /// <param name="queueItem">the request queue item</param>
        /// <param name="state">the state object.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void PrefetchRequestCallback(BrokerQueueItem queueItem, object state)
        {
            try
            {
#if DEBUG
                BrokerTracing.TraceVerbose("[BrokerQueueDispatcher] .PrefetchRequestCallback: received one request. request id={0}", GetMessageId(queueItem));
#endif
                bool isCached = false;
                bool shouldDisposeQueueItem = false;
                Guid persistId = Guid.Empty;
                object asyncToken = null;
                int dispatcherNumber = 0;
                if (queueItem != null)
                {
                    dispatcherNumber = queueItem.DispatchNumber;
                    persistId = queueItem.PersistId;
                    asyncToken = queueItem.PersistAsyncToken.AsyncToken;
                    if (this.requestsAsyncTokenTable.Count > 0 && dispatcherNumber == 0)
                    {
                        // if the request is already in the cache or dispatched.
                        lock (this.requestsAsyncTokenTable)
                        {
                            if (this.requestsAsyncTokenTable.ContainsKey(persistId))
                            {
                                try
                                {
                                    DispatcherAsyncTokenItem asyncTokenItem = this.requestsAsyncTokenTable[persistId];
                                    if (asyncTokenItem.AsyncToken == null)
                                    {
                                        asyncTokenItem.AsyncToken = asyncToken;
                                    }

                                    // the duplicated request item is of no use after retrieving out its async token.  so we should dispose it if possible
                                    shouldDisposeQueueItem = true;
                                }
                                catch (Exception e)
                                {
                                    // the request in the table is removed by another thread.
                                    BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .PrefetchRequestCallback: The request async token is removed from the table, Exception: {0}", e);
                                }

                                isCached = true;
                            }
                        }
                    }

                    if (!isCached)
                    {
                        bool needQueue = true;

                        // if there are pending requests callback, then dispatch the request through the pending requests callback.
                        if (this.requestCallbackQueue.Count > 0)
                        {
                            try
                            {
                                BrokerQueueCallbackItem requestCallback ;
                                if (this.requestCallbackQueue.TryDequeue(out requestCallback))
                                {
                                    this.RegisterReemit(queueItem);

                                    requestCallback.Callback(queueItem, requestCallback.CallbackState);
                                    needQueue = false;
#if DEBUG
                                    BrokerTracing.TraceVerbose("[BrokerQueueDispatcher] .PrefetchRequestCallback: Invoked callback for request.");
#endif
                                }
                                else
                                {
                                    BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .PrefetchRequestCallback: The requests callback queue is empty.");
                                }
                            }
                            catch (InvalidOperationException e)
                            {
                                // the request callback queue is drained by other threads.
                                BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .PrefetchRequestCallback: The requests callback queue is empty, Exception: {0}", e);
                            }
                            catch (Exception e)
                            {
                                // request callback raise exception.
                                BrokerTracing.TraceWarning("[BrokerQueueDispatcher] .PrefetchRequestCallback: The requests callback raise exception, Exception: {0}", e);
                            }
                        }

                        if (needQueue)
                        {
                            // if no pending requests callback, then append the request to the cache queue.
                            this.requestCacheQueue.Enqueue(queueItem);
                            if (this.requestCacheQueue.Count >= this.GetCacheContainerSize())
                            {
                                this.needPrefetch = false;
                            }
                            else
                            {
                                this.needPrefetch = true;
                            }
                        }
                    }
                }

                BrokerQueue brokerQueue = (BrokerQueue)state;

                // try to get the next request.
                if (this.needPrefetch)
                {
                    // hop to another thread to avoid recursive stack overflow for memory queue that will call the callback at the same thread.
                    ThreadPool.QueueUserWorkItem(this.GetRequestThreadProc, state);
                }
                else
                {
                    lock (this.waitBrokerQueuesForPeekRequests)
                    {
                        this.waitBrokerQueuesForPeekRequests.Add(brokerQueue);
                    }

                    if (this.requestCacheQueue.Count < this.sharedData.DispatcherCount + this.watermarkerForTriggerPrefetch)
                    {
                        this.TriggerPrefetch();
                    }
                }

                // drain the pending requests callback.
                this.HandlePendingRequestCallback();

                // check if the response already come back.
                if (!this.HandleResponseWithoutAsyncToken(brokerQueue, queueItem) && shouldDisposeQueueItem)
                {
                    // if its response is not back, and we've decided that the request queue item is of no use, dispose it.
                    queueItem.Dispose();
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .PrefetchRequestCallback: raised unknown exception: {0}", e);
            }
        }

        /// <summary>
        /// Gets the cache container size
        /// </summary>
        /// <returns>a number indicating the cache container size</returns>
        private int GetCacheContainerSize()
        {
            return this.sharedData.DispatcherCount * 2 + this.cacheContainerSize;
        }

        /// <summary>
        /// asign the async token to the corresponding response in the table without the async token and put it to the original queue.
        /// </summary>
        /// <param name="queue">the queue that own the response.</param>
        /// <param name="requestItem">the corresponding request item.</param>
        /// <returns> if the request item is handled with responsesWithoutAsyncTokenTable</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool HandleResponseWithoutAsyncToken(BrokerQueue queue, BrokerQueueItem requestItem)
        {
            bool isHandled = false;
            Guid persistId = requestItem.PersistId;
            object asyncToken = requestItem.PersistAsyncToken.AsyncToken;
            int dispatcherNumber = requestItem.DispatchNumber;
            if (asyncToken != null && this.responsesWithoutAsyncTokenTable.Count > 0)
            {
                bool needPutResponse = false;
                DispatcherAsyncTokenItem asyncTokenItem = null;
                try
                {
                    lock (this.responsesWithoutAsyncTokenTable)
                    {
                        this.responsesWithoutAsyncTokenTable.TryGetValue(persistId, out asyncTokenItem);
                    }

                    if (asyncTokenItem != null)
                    {
                        lock (this.responsesWithoutAsyncTokenTable)
                        {
                            this.responsesWithoutAsyncTokenTable.Remove(persistId);
                        }

                        needPutResponse = true;

                        lock (this.requestsAsyncTokenTable)
                        {
                            this.requestsAsyncTokenTable.Remove(persistId);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (needPutResponse)
                    {
                        BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .PrefetchRequestCallback: fail to remove the async token from the table, Exception: {0}", e);
                    }
                    else
                    {
                        BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .PrefetchRequestCallback: can not get the response async token, Exception: {0}", e);
                    }
                }

                if (needPutResponse && dispatcherNumber == 0)
                {
                    asyncTokenItem.PersistAsyncToken.AsyncToken = asyncToken;
                    queue.PutResponseAsync(asyncTokenItem.Message, requestItem).GetAwaiter().GetResult();
                    isHandled = true;
                }
            }
            return isHandled;
        }

        /// <summary>
        /// the thread proc for getting request from the queue.
        /// </summary>
        /// <param name="state">the state object.</param>
        private void GetRequestThreadProc(object state)
        {
            BrokerQueue queue = (BrokerQueue)state;
            queue.GetRequestAsync(this.PrefetchRequestCallback, queue);
        }

        /// <summary>
        /// handle the pending requests callback.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void HandlePendingRequestCallback()
        {
            while (this.requestCacheQueue.Count > 0 && this.requestCallbackQueue.Count > 0)
            {
                BrokerQueueItem request = null;
                try
                {
                    if (!this.requestCacheQueue.TryDequeue(out request))
                    {
                        BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .HandlePendingRequestCallback: All cached requests are dispatched.");
                    }
                }
                catch (InvalidOperationException e)
                {
                    // the queue is drained by others thread.
                    BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .HandlePendingRequestCallback: All cached requests are dispatched. Exception: {0}", e);
                }

                if (request != null)
                {
                    BrokerQueueCallbackItem requestCallback = null;
                    try
                    {
                        if (!this.requestCallbackQueue.TryDequeue(out requestCallback))
                        {
                            requestCallback = null;

                            // the requests callback queue is drained by other threads.
                            this.requestCacheQueue.Enqueue(request);

                            BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .HandlePendingRequestCallback: No available pending request callbacks in the queue.");
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        // the requests callback queue is drained by other threads.
                        this.requestCacheQueue.Enqueue(request);

                        BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .HandlePendingRequestCallback: No available pending request callbacks in the queue. Exception: {0}", e);
                    }

                    if (requestCallback != null)
                    {
                        try
                        {
#if DEBUG
                            BrokerTracing.TraceVerbose("[BrokerQueueDispatcher] .HandlePendingRequestCallback: invoke callback. request id={0}", GetMessageId(request));
#endif

                            this.RegisterReemit(request);

                            requestCallback.Callback(request, requestCallback.CallbackState);
                        }
                        catch (Exception e)
                        {
                            // the get request callback raise exception.
                            this.requestCacheQueue.Enqueue(request);

                            BrokerTracing.TraceWarning("[BrokerQueueDispatcher] .HandlePendingRequestCallback: The request callback raise exception. Exception: {0}", e);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// trigger the prefetch operation.
        /// </summary>
        private void TriggerPrefetch()
        {
            this.needPrefetch = true;
            while (this.needPrefetch && this.waitBrokerQueuesForPeekRequests.Count > 0)
            {
                BrokerQueue queue = null;
                lock (this.waitBrokerQueuesForPeekRequests)
                {
                    if (this.waitBrokerQueuesForPeekRequests.Count > 0)
                    {
                        queue = this.waitBrokerQueuesForPeekRequests[this.waitBrokerQueuesForPeekRequests.Count - 1];
                        this.waitBrokerQueuesForPeekRequests.Remove(queue);
                    }
                }

                if (queue != null)
                {
                    ThreadPool.QueueUserWorkItem(this.waitCallbackToGetRequestAsync, queue);
                }
            }
        }

        /// <summary>
        /// Callback to get request
        /// </summary>
        /// <param name="state">indicating the state</param>
        private void WaitCallbackToGetRequestAsync(object state)
        {
            BrokerQueue queue = (BrokerQueue)state;
            queue.GetRequestAsync(this.PrefetchRequestCallback, queue);
        }

        /// <summary>
        /// the async token item.
        /// </summary>
        private class DispatcherAsyncTokenItem
        {
            /// <summary>
            /// the async token.
            /// </summary>
            private object asyncTokenField;

            /// <summary>
            /// the message.
            /// </summary>
            private Message message;

            /// <summary>
            /// the persist async token.
            /// </summary>
            private BrokerQueueAsyncToken persistAsyncToken;

            /// <summary>
            /// Initializes a new instance of the DispatcherAsyncTokenItem class
            /// </summary>
            public DispatcherAsyncTokenItem()
            {
            }

            /// <summary>
            /// Initializes a new instance of the DispatcherAsyncTokenItem class
            /// </summary>
            /// <param name="msg">the message.</param>
            /// <param name="persistAsyncToken">the persist async token.</param>
            public DispatcherAsyncTokenItem(Message msg, BrokerQueueAsyncToken persistAsyncToken)
            {
                this.message = msg;
                this.persistAsyncToken = persistAsyncToken;
            }

            /// <summary>
            /// Gets or sets the async token.
            /// </summary>
            public object AsyncToken
            {
                get
                {
                    return this.asyncTokenField;
                }

                set
                {
                    this.asyncTokenField = value;
                }
            }

            /// <summary>
            /// Gets the message.
            /// </summary>
            public Message Message
            {
                get
                {
                    return this.message;
                }
            }

            /// <summary>
            /// Gets the persist async token.
            /// </summary>
            public BrokerQueueAsyncToken PersistAsyncToken
            {
                get
                {
                    return this.persistAsyncToken;
                }
            }
        }
    }
}
