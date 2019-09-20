// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Persistences.MSMQPersist
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Messaging;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;

    /// <summary>
    /// A class that implements logic of peeking all messages from MSMQ queue.
    /// It walks through all messages in a MSMQ queue via MessageQueue.BeginPeek/EndPeek call, with a forwarding
    /// Cursor as parameter.  To increases peeking message throughput, it keeps multiple BeginPeek calls 
    /// outstanding at the same time (based on MSMQ perf whitepaper, MSMQ message receiving througput goes to 
    /// a peek when there aree 3~5 receiving threads);  meanwhile, it uses a small prefetching cache to reduce 
    /// message fetching latency.
    /// </summary>
    internal class MSMQMessageFetcher
    {
        #region private fields

        /// <summary> Default capacity of prefetch cache. </summary>
        private const int DefaultPrefetchCacheCapacity = 1024;

        /// <summary> Maximum number of concurrent outstanding BeginPeek operations allowed</summary>
        private const int DefaultMaxOutstandingFetchCount = 5;

        /// <summary> Prefetch cache that used to store messages retrieved from MSMQ</summary>
        private ConcurrentQueue<MessageResult> prefetchCache = new ConcurrentQueue<MessageResult>();

        /// <summary> Prefetch cache capacity</summary>
        private int prefetchCacheCapacity;

        /// <summary> Lock object for prefetchCache</summary>
        private ReaderWriterLockSlim lockPrefetchCache = new ReaderWriterLockSlim();

        /// <summary> Credit for prefetching. It indicates if there is space available in prefetchCache for keeping prefetching result.</summary>
        private int prefetchCredit;

        /// <summary> Number of currently outstanding BeginPeek operations</summary>
        private int outstandingFetchCount;

        /// <summary> Maximun number of outstanding BeginPeek operations</summary>
        private int maxOutstandingFetchCount;

        /// <summary> Lock object for synchronizing access to related counters, including outstandingFetchCount, prefetchCredit, and msmqMessageCount</summary>
        private object lockFetchCount = new object();

        /// <summary> The target MSMQ queue.</summary>
        private MessageQueue msmqQueueField;

        /// <summary> Number of messages in target MSMQ queue</summary>
        private long msmqMessageCount;

        /// <summary> Cursor used to walk through all messages in the target MSMQM queue, from head to tail</summary>
        private RefCountedCursor messagePeekCursorField;

        /// <summary> Lock object for messagePeekCursorField. </summary>
        private ReaderWriterLockSlim rwlockMessagePeekCursorField = new ReaderWriterLockSlim();

        /// <summary> The peek action used to locate and forward the message peek cursor.</summary>
        private PeekAction messagePeekActionField;

        /// <summary> The timespan for peek message from MSMQ.</summary>
        // Note: 2147483647 (int.MaxValue) * 100 nano-seconds = 214.748 seconds = 3 mins 34.748 seconds
        private TimeSpan messagePeekTimespanField = new TimeSpan(int.MaxValue);

        /// <summary> Current cursor position from queue head.  It also tells number of messages that have been peeked from MSMQ.</summary>
        private long peekCursorPosition;

        /// <summary> GetMessageState instance that stores context of a to-be-served GetMessageAsync call. </summary>
        private ConcurrentQueue<GetMessageState> currentGetMessageQueue = new ConcurrentQueue<GetMessageState>();

        /// <summary> The message formatter used to deserialize message peeked from MSMQ queue. </summary>
        private IMessageFormatter messageFormatter;

        /// <summary> A value indicating whether the target queue is disposed.</summary>
        private volatile bool isQueueDisposed;

        /// <summary> A value indicating whether this instance is disposed.</summary>
        private volatile bool isDisposedField;

        /// <summary>
        /// Concurrent cache for the partial messages
        /// </summary>
        private ConcurrentDictionary<string, ConcurrentDictionary<int, Message>> partialMessages = new ConcurrentDictionary<string, ConcurrentDictionary<int, Message>>();

        /// <summary>
        /// Stores the partial message counters
        /// </summary>
        private ConcurrentDictionary<string, int> partialMessageCounters = new ConcurrentDictionary<string, int>();

        private int pendingFetchCount = 0;

        private System.Timers.Timer prefetchTimer = new System.Timers.Timer();

#if DEBUG
        private int cacheHitCounter = 0;
        private int cacheMissCounter = 0;
#endif

#endregion

        /// <summary>
        /// Instantiate a new instance of MSMQMessageFetcher class.
        /// </summary>
        /// <param name="messageQueue">target MSMQ queue</param>
        /// <param name="messageCount">number of messages in queue</param>
        /// <param name="messageFormatter">message formatter for deserializing MSMQ message</param>
        public MSMQMessageFetcher(MessageQueue messageQueue, long messageCount, IMessageFormatter messageFormatter) :
            this(messageQueue, messageCount, messageFormatter, DefaultPrefetchCacheCapacity, Environment.ProcessorCount > DefaultMaxOutstandingFetchCount ? Environment.ProcessorCount : DefaultMaxOutstandingFetchCount)
        {
        }

        /// <summary>
        /// Instantiate a new instance of MSMQMessageFetcher class.
        /// </summary>
        /// <param name="messageQueue">target MSMQ queue</param>
        /// <param name="messageCount">number of messages in queue</param>
        /// <param name="messageFormatter">message formatter for deserializing MSMQ message</param>
        /// <param name="prefetchCacheCapacity">prefetch cache capacity</param>
        /// <param name="maxOutstandingFetchCount">maximun number of outstanding BeginPeek operations</param>
        public MSMQMessageFetcher(MessageQueue messageQueue, long messageCount, IMessageFormatter messageFormatter, int prefetchCacheCapacity, int maxOutstandingFetchCount)
        {
            this.msmqQueueField = messageQueue;
            this.msmqMessageCount = messageCount;
            this.messageFormatter = messageFormatter;
            this.prefetchCacheCapacity = prefetchCacheCapacity;
            this.maxOutstandingFetchCount = maxOutstandingFetchCount;
            this.prefetchCredit = this.prefetchCacheCapacity;

            this.messagePeekCursorField = new RefCountedCursor(this.msmqQueueField.CreateCursor());
            this.messagePeekActionField = PeekAction.Current;
            this.msmqQueueField.Disposed += this.OnQueueDisposed;

            this.prefetchTimer.AutoReset = false;
            this.prefetchTimer.Interval = 500;
            this.prefetchTimer.Elapsed += (sender, args) =>
                {
                    Debug.WriteLine("[MSMQMessageFetcher] .prefetchTimer raised.");
                    this.PeekMessage();
                    if (!this.isDisposedField)
                    {
                        this.prefetchTimer.Enabled = true;
                    }
                };
            this.prefetchTimer.Enabled = true;

            BrokerTracing.TraceVerbose("[MSMQMessageFetcher] .Create new instance: prefetchCacheCapacity={0}, maxOutstandingFetchCount={1}", this.prefetchCacheCapacity, this.maxOutstandingFetchCount);
        }

        /// <summary>
        /// Get next message from target MSMQ queue aysnchrously
        /// </summary>
        /// <param name="callback">the callback to be invoked once the next message is retrieved back from MSMQ queue</param>
        /// <param name="state">the state object of the callback</param>
        public void GetMessageAsync(PersistCallback callback, object state)
        {
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[MSMQMessageFetcher] .GetMessageAsync: the instance is disposed.");
                return;
            }

            BrokerTracing.TraceVerbose("[MSMQMessageFetcher] .GetMessageAsync: Get message come in.");

            //
            // GetMessageAsync:
            // step 1, try to get message from prefetch cache. If succeeded, invoke the callback directly; or else,
            // step 2, save context of this GetMessageAsync call, including callback and callback state into this.currentGetMessageQueue, which will be handled when a message is retrieved back from MSMQ.
            // step 3, check if need to get more messages.  If so, initiate more BeginPeek calls into MSMQ
            //
            GetMessageState getMessageState = new GetMessageState(callback, state);
            MessageResult result = null;
            Queue<MessageResult> messageQueue = new Queue<MessageResult>();

#if DEBUG
            bool getResponse = false;
#endif

            long getMessageCount = 1;
            ResponseCallbackItem responseCallbackItem = state as ResponseCallbackItem;
            if (responseCallbackItem != null)
            {
#if DEBUG
                getResponse = true;
#endif
                getMessageCount = responseCallbackItem.ExpectedResponseCount;
                Debug.WriteLine($"[MSMQMessageFetcher] .GetMessageAsync: working to get response. getMessageCount={getMessageCount}.");
            }
            else
            {
                Debug.WriteLine($"[MSMQMessageFetcher] .GetMessageAsync: working to get request. getMessageCount={getMessageCount}.");
            }

            this.lockPrefetchCache.EnterReadLock();
            try
            {
                while (getMessageCount > 0)
                {
                    if (this.prefetchCache == null)
                    {
                        // this instance is disposed
                        BrokerTracing.TraceWarning("[MSMQMessageFetcher] .GetMessageAsync: the instance is disposed.");
                        break;
                    }

                    if (!(this.prefetchCache.Count > 0 && this.prefetchCache.TryDequeue(out result)))
                    {
#if DEBUG
                        Interlocked.Increment(ref this.cacheMissCounter);
#endif
                        BrokerTracing.TraceVerbose("[MSMQMessageFetcher] .GetMessageAsync: cache miss");
                        result = null;
                        this.currentGetMessageQueue.Enqueue(getMessageState);
                        break;
                    }

#if DEBUG
                    Interlocked.Increment(ref this.cacheHitCounter);
#endif
                    BrokerTracing.TraceVerbose("[MSMQMessageFetcher] .GetMessageAsync: hit cache");
                    messageQueue.Enqueue(result);
                    getMessageCount--;
                }
            }
            finally
            {
                this.lockPrefetchCache.ExitReadLock();
            }

            Debug.WriteLine($"[MSMQMessageFetcher] .GetMessageAsync: getMessageCount={getMessageCount} after dequeue.");
#if DEBUG
            BrokerTracing.TraceVerbose("[MSMQMessageFetcher] .GetMessageAsync: cache hit rate: {0}, getting response: {1}", ((double)this.cacheHitCounter) / (this.cacheHitCounter + this.cacheMissCounter), getResponse);
#endif

            while (messageQueue.Any())
            {
                var message = messageQueue.Dequeue();
                if (message != null)
                {
                    // There is one more space available in prefetch cache
                    lock (this.lockFetchCount)
                    {
                        this.prefetchCredit++;
                    }

                    // GetMessageAsync is a light-weight call and is supposed to return very soon.  While in its callback,
                    // there may be time-consuming operations. So here we always invoke the callback in another thread.
                    ThreadPool.QueueUserWorkItem(this.GetMessageAsyncCallback, new object[] { getMessageState, message });
                }
                else
                {
                    BrokerTracing.TraceWarning("[MSMQMessageFetcher] .GetMessageAsync: encountered null message result.");
                }
            }

            this.CheckAndGetMoreMessages();
        }

        /// <summary>
        /// Notify that there are a number of messages put into the target MSMQ queue
        /// </summary>
        /// <param name="messageCount">number of messages</param>
        public void NotifyMoreMessages(long messageCount)
        {
            lock (this.lockFetchCount)
            {
                this.msmqMessageCount += messageCount;
            }
        }

        /// <summary>
        /// Dispose this MSMQMessageFetcher instance.
        /// </summary>
        public void SafeDispose()
        {
            BrokerTracing.TraceVerbose("[MSMQMessageFetcher] .SafeDispose.");

            //
            // SafeDispose:
            // 1. mark this instance as disposed
            // 2. wait for all outstanding BeginPeek operations back, and then dispose the cursor.
            //
            if (!this.isDisposedField)
            {
                this.isDisposedField = true;
                // dispose prefetched messages
                this.lockPrefetchCache.EnterWriteLock();
                try
                {
                    if (this.prefetchCache != null)
                    {
                        // below line of code is put inside of prefetchCache lock scope just to ensure it's executed only once
                        this.msmqQueueField.Disposed -= this.OnQueueDisposed;

                        if (!this.prefetchCache.IsEmpty)
                        {
                            BrokerTracing.TraceWarning("[MSMQMessageFetcher] .SafeDispose: PrefetchCache is not empty when disposing.");
                        }

                        foreach (MessageResult result in this.prefetchCache)
                        {
                            if (result.Message != null)
                            {
                                result.Message.Dispose();
                            }
                        }

                        this.prefetchCache = null;
                    }
                }
                finally
                {
                    this.lockPrefetchCache.ExitWriteLock();
                }

                this.rwlockMessagePeekCursorField.EnterWriteLock();
                try
                {
                    if (this.messagePeekCursorField != null)
                    {
                        this.messagePeekCursorField.Release();
                        this.messagePeekCursorField = null;
                    }
                }
                finally
                {
                    this.rwlockMessagePeekCursorField.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Threadpool callback that invokes callback of GetMessageAsync call
        /// </summary>
        /// <param name="state">callback state of the threadpool callback</param>
        private void GetMessageAsyncCallback(object state)
        {
            object[] objs = (object[])state;
            GetMessageState getMessageState = objs[0] as GetMessageState;
            MessageResult result = objs[1] as MessageResult;

            try
            {
                if (getMessageState.MessageCallback != null)
                {
                    getMessageState.MessageCallback(result.Message, getMessageState.CallbackState, result.Exception);
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                        "[MSMQMessageFetcher] .GetMessageAsyncCallback: perform the message callback failed, the exception, {0}",
                        e.ToString());
            }
        }

        /// <summary>
        /// Check if need to fetch more messages
        /// </summary>
        private void CheckAndGetMoreMessages()
        {
            // Fetch more messages from MSMQ to fill up the prefetch cache.
            while (!this.isDisposedField)
            {
                BrokerTracing.TraceVerbose(
                    "[MSMQMessageFetcher] .CheckAndGetMoreMessages: prefetchCredit={0}, outstandingFetchCount={1}, msmqMessageCount={2}, pendingFetchCount={3}",
                    this.prefetchCredit,
                    this.outstandingFetchCount,
                    this.msmqMessageCount,
                    this.pendingFetchCount);

                int increasedFetchCount = 0;
                while (this.AddFetchCount())
                {
                    ++increasedFetchCount;
                    Interlocked.Increment(ref this.pendingFetchCount);
                }

                BrokerTracing.TraceVerbose("[MSMQMessageFetcher] .CheckAndGetMoreMessages: increasedFetchCount={0}", increasedFetchCount);
                return;
            }
        }

        /// <summary>
        /// Peek next message from MSMQ by initiating one more BeginPeek call.
        /// </summary>
        private void PeekMessage()
        {
            BrokerTracing.TraceVerbose("[MSMQMessageFetcher] .PeekMessage: one more BeginPeek");

            if (this.isDisposedField)
            {
                BrokerTracing.TraceInfo("[MSMQMessageFetcher] .PeekMessage: the instance is disposed");
                this.RevertFetchCount();
                return;
            }

            List<Exception> exceptions = new List<Exception>();

#if DEBUG
            int peekCount = 0;
#endif
            while (true)
            {
                if (this.pendingFetchCount < 1)
                {
                    break;
                }

                this.rwlockMessagePeekCursorField.EnterUpgradeableReadLock();
                Debug.WriteLine($"[MSMQMessageFetcher] .PeekMessage: before execution pendingFetchCount={this.pendingFetchCount}");



                try
                {
                    while (this.pendingFetchCount > 0)
                    {
                        RefCountedCursor cursorRef;
                        if (this.messagePeekCursorField == null)
                        {
                            BrokerTracing.TraceInfo("[MSMQMessageFetcher] .PeekMessage: cursor for peek message is disposed");
                            this.RevertFetchCount();
                            return;
                        }
                        else
                        {
                            cursorRef = this.messagePeekCursorField.Acquire();
                        }

                        try
                        {
                            this.msmqQueueField.BeginPeek(this.messagePeekTimespanField, cursorRef.MSMQCursor, this.messagePeekActionField, cursorRef, this.PeekMessageComplete);
                            this.messagePeekActionField = PeekAction.Next;
                            Interlocked.Decrement(ref this.pendingFetchCount);
#if DEBUG
                            peekCount++;
#endif
                        }
                        catch (Exception e)
                        {
                            cursorRef.Release();
                            if (this.isDisposedField)
                            {
                                this.RevertFetchCount();
                                // if the queue is closed, do nothing.
                                return;
                            }

                            exceptions.Add(e);
                        }
                    }
                }
                finally
                {
                    this.rwlockMessagePeekCursorField.ExitUpgradeableReadLock();
                }

                // actively check if there is new message waiting to be fetch. If yes, don't wait for next timer triggering.
                this.CheckAndGetMoreMessages();
            }
#if DEBUG
            Debug.WriteLine($"[MSMQMessageFetcher] .PeekMessage: after execution pendingFetchCount={this.pendingFetchCount}, peekCount={peekCount}");
#endif

            foreach (var exception in exceptions)
            {
                BrokerTracing.TraceError(
                    "[MSMQMessageFetcher] .PeekMessage: BeginPeek message from the queue failed, the exception, {0}.",
                    exceptions.ToString());

                this.HandleMessageResult(new MessageResult(null, exception));
            }
        }

        /// <summary>
        /// Callback function invoked on BeginPeek completion. 
        /// </summary>
        /// <param name="ar"></param>
        private void PeekMessageComplete(IAsyncResult ar)
        {
            BrokerTracing.TraceVerbose("[MSMQMessageFetcher] .PeekMessageComplete: Peek message complete.");

            Exception exception = null;
            System.Messaging.Message message = null;
            bool needRetry = false;
            RefCountedCursor storedMessagePeekCursor = ar.AsyncState as RefCountedCursor;

            this.rwlockMessagePeekCursorField.EnterReadLock();
            try
            {
                try
                {
                    //
                    // Check if target queue is disposed. If so, don't call EndPeek.  However, the check doesn't guarantee
                    // that msmqQueueField is valid when EndPeek is invoked on it. So ObjectDisposedException shall be handled.
                    //
                    if (this.isQueueDisposed)
                    {
                        BrokerTracing.TraceWarning("[MSMQMessageFetcher]) .PeekMessageComplete: target queue is disposed");
                        return;
                    }

                    message = this.msmqQueueField.EndPeek(ar);
                    Interlocked.Increment(ref this.peekCursorPosition);
                }
                catch (MessageQueueException e)
                {
                    if (this.isDisposedField)
                    {
                        this.RevertFetchCount();
                        return;
                    }

                    needRetry = MessageQueueHelper.HandleAsyncCallbackError(
                        e,
                        ref this.messagePeekActionField,
                        "[MSMQMessageFetcher] .PeekMessageComplete: EndPeek message failed, outstandingPeekCount:{0}, messageCount={1}",
                        this.outstandingFetchCount,
                        this.msmqMessageCount);

                    if (!needRetry)
                    {
                        BrokerTracing.TraceError(
                            "[MSMQMessageFetcher] .PeekMessageComplete: end peek message failed, outstandingPeekCount:{0}, messageCount:{1} Exception:{2}",
                            this.outstandingFetchCount,
                            this.msmqMessageCount,
                            e);
                        exception = e;
                    }
                }
                catch (Exception e)
                {
                    if (this.isDisposedField)
                    {
                        this.RevertFetchCount();
                        return;
                    }

                    BrokerTracing.TraceError("[MSMQMessageFetcher] .PeekMessageComplete: end peek message failed, Exception:{0}", e.ToString());
                    exception = e;
                }
                finally
                {
                    storedMessagePeekCursor.Release();
                }
            }
            finally
            {
                this.rwlockMessagePeekCursorField.ExitReadLock();
            }

            if (needRetry)
            {
                Interlocked.Increment(ref this.pendingFetchCount);
                return;
            }

            if (message == null && exception == null)
            {
                BrokerTracing.TraceWarning("[MSMQMessageFetcher] .PeekMessageComplete: EndPeek return null.");
            }

            if (message != null)
            {
                // if the message is one of the partial messages
                if (message.AppSpecific > 0 && !string.IsNullOrEmpty(message.Label))
                {
                    try
                    {
                        BrokerTracing.TraceVerbose(
                               "[MSMQMessageFetcher] .PeekMessageComplete: peek partial message with AppSpecific {0} and Label {1}.", message.AppSpecific, message.Label);
                        string[] labels = message.Label.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                        string groupId = labels[0];
                        int msgNumber = int.Parse(labels[1]);
                        ConcurrentDictionary<int, Message> messages = this.partialMessages.GetOrAdd(groupId, (k) => new ConcurrentDictionary<int, Message>());
                        if (!messages.TryAdd(msgNumber, message))
                        {
                            BrokerTracing.TraceError(
                              "[MSMQMessageFetcher] .PeekMessageComplete: try add one of the composed messages failed. Message Label {0}.", message.Label);
                        }

                        // check if all partial messages are peeked and cached
                        if (this.partialMessageCounters.AddOrUpdate(groupId, 1, (k, v) => v + 1) == message.AppSpecific)
                        {
                            BrokerTracing.TraceVerbose(
                               "[MSMQMessageFetcher] .PeekMessageComplete: all partial messages are peeked and cached for group {0}.", groupId);
                            Message msg = new Message();
                            msg.BodyType = message.BodyType;
                            byte[] buffer = new byte[Constant.MSMQChunkSize];
                            List<long> lookUpIds = new List<long>(message.AppSpecific);
                            for (int i = 1; i <= message.AppSpecific; i++)
                            {
                                Message m = messages[i];
                                int count = m.BodyStream.Read(buffer, 0, Constant.MSMQChunkSize);
                                msg.BodyStream.Write(buffer, 0, count);
                                lookUpIds.Add(m.LookupId);
                            }
                            msg.BodyStream.Position = 0;
                            BrokerQueueItem brokerQueueItem = null;

                            // Deserialize message to BrokerQueueItem
                            try
                            {
                                brokerQueueItem = (BrokerQueueItem)this.messageFormatter.Read(msg);
                                brokerQueueItem.PersistAsyncToken.AsyncToken = lookUpIds.ToArray();
                            }
                            catch (Exception e)
                            {

                                BrokerTracing.TraceError(
                                    "[MSMQMessageFetcher] .PeekMessageComplete: deserialize message failed for composed messages with groupId {0}, Exception:{1}", groupId,
                                    e);
                                exception = e;
                            }

                            messages.Clear();
                            if (!this.partialMessages.TryRemove(groupId, out messages))
                            {
                                BrokerTracing.TraceWarning(
                               "[MSMQMessageFetcher] .PeekMessageComplete: try to remove partial messages with groupId {0} from cahce failed.", groupId);
                            }
                            int messageCount;
                            if (!this.partialMessageCounters.TryRemove(groupId, out messageCount))
                            {
                                BrokerTracing.TraceWarning(
                               "[MSMQMessageFetcher] .PeekMessageComplete: try to remove partial message counters with groupId {0} from cahce failed.", groupId);
                            }

                            this.HandleMessageResult(new MessageResult(brokerQueueItem, exception));
                        }
                        else
                        {
                            Interlocked.Increment(ref this.pendingFetchCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        BrokerTracing.TraceError(
                            "[MSMQMessageFetcher] .PeekMessageComplete: peek composed message failed, Exception:{0}", ex);
                    }
                }
                else
                {
                    BrokerQueueItem brokerQueueItem = null;

                    // Deserialize message to BrokerQueueItem
                    try
                    {
                        brokerQueueItem = (BrokerQueueItem)this.messageFormatter.Read(message);
                        brokerQueueItem.PersistAsyncToken.AsyncToken = new long[1] { message.LookupId };
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[MSMQMessageFetcher] .PeekMessageComplete: deserialize message failed, Exception:{0}",
                            e.ToString());
                        exception = e;
                    }

                    this.HandleMessageResult(new MessageResult(brokerQueueItem, exception));
                }
            }
        }

        /// <summary>
        /// Check and prepare prefetching related counters
        /// </summary>
        /// <returns></returns>
        private bool AddFetchCount()
        {
            lock (this.lockFetchCount)
            {
                // No prefetching if there are too many pending peek message operations
                if (this.outstandingFetchCount >= this.maxOutstandingFetchCount)
                {
                    return false;
                }

                // No prefetching if no message avaialble
                if (this.outstandingFetchCount >= this.msmqMessageCount)
                {
                    return false;
                }

                // No prefetching if prefecth credit is used up
                if (this.prefetchCredit <= 0)
                {
                    return false;
                }

                this.outstandingFetchCount++;

                // Decrease prefetch credit by 1 to reserve space in prefetch cache for prefetching result 
                this.prefetchCredit--;
                return true;
            }

        }

        /// <summary>
        /// Revert prefetch related counters.  This is to be called when a prefetch operation is cancelled.
        /// </summary>
        private void RevertFetchCount()
        {
            lock (this.lockFetchCount)
            {
                this.outstandingFetchCount--;
                this.prefetchCredit++;
            }
        }

        /// <summary>
        /// Handle MSMQ.BeginPeek/EndPeek result
        /// </summary>
        private void HandleMessageResult(MessageResult messageResult)
        {
            lock (this.lockFetchCount)
            {
                this.outstandingFetchCount--;
                if (messageResult.Message != null)
                {
                    this.msmqMessageCount--;
                }
            }

            GetMessageState getMessageState = null;

            this.lockPrefetchCache.EnterUpgradeableReadLock();

            bool TryGetMessageFromQueue(out GetMessageState state)
            {
                state = null;
                return this.currentGetMessageQueue != null && this.currentGetMessageQueue.TryDequeue(out state);
            }

            try
            {
                if (this.prefetchCache == null)
                {
                    // this instance is disposed
                    return;
                }

                if (!TryGetMessageFromQueue(out getMessageState))
                {
                    this.lockPrefetchCache.EnterWriteLock();
                    try
                    {
                        if (!TryGetMessageFromQueue(out getMessageState))
                        {
                            getMessageState = null;
                            this.prefetchCache.Enqueue(messageResult);
                        }
                    }
                    finally
                    {
                        this.lockPrefetchCache.ExitWriteLock();
                    }
                }
            }
            finally
            {
                this.lockPrefetchCache.ExitUpgradeableReadLock();
            }

            if (getMessageState != null)
            {
                lock (this.lockFetchCount)
                {
                    this.prefetchCredit++;
                }
                ThreadPool.QueueUserWorkItem(this.GetMessageAsyncCallback, new object[] { getMessageState, messageResult });
            }

            this.CheckAndGetMoreMessages();
        }

        /// <summary>
        /// Event handler that handles Disposed event for target queue
        /// </summary>
        private void OnQueueDisposed(object sender, EventArgs arg)
        {
            this.isQueueDisposed = true;
            BrokerTracing.TraceInfo("[MSMQMessageFetcher] .OnQueueDisposed: target queue is disposed.");
        }

        /// <summary>
        /// thread pool callback state for getting message
        /// </summary>
        private class GetMessageState
        {
            #region private fields

            /// <summary>the callback that get the message.</summary>
            private PersistCallback messageCallbackField;

            /// <summary>the callback state object.</summary>
            private object callbackStateField;

            #endregion

            /// <summary>
            /// Initializes a new instance of the GetMessageState class.
            /// </summary>
            /// <param name="messageCallback">the callback that get the message</param>
            /// <param name="callbackState">the callback state object.</param>
            public GetMessageState(PersistCallback messageCallback, object callbackState)
            {
                if (messageCallback == null)
                {
                    throw new ArgumentNullException("messageCallback");
                }

                this.messageCallbackField = messageCallback;
                this.callbackStateField = callbackState;
            }

            /// <summary>
            /// Gets the message callback.
            /// </summary>
            public PersistCallback MessageCallback
            {
                get
                {
                    return this.messageCallbackField;
                }
            }

            /// <summary>
            /// Gets the mesage callback state object.
            /// </summary>
            public object CallbackState
            {
                get
                {
                    return this.callbackStateField;
                }
            }
        }

        /// <summary>
        /// A simple wrapper that maintains result from MSMQ.BeginPeek/EndPeek
        /// </summary>
        private class MessageResult
        {
            private BrokerQueueItem message;
            private Exception exception;

            public MessageResult(BrokerQueueItem message, Exception e)
            {
                this.message = message;
                this.exception = e;
            }
            public BrokerQueueItem Message
            {
                get { return this.message; }
            }
            public Exception Exception
            {
                get { return this.exception; }
            }
        }

        /// <summary>
        /// Reference counted wrapper for MSMQ Cursor. 
        /// Note: this is not a thread-safe wrapper.
        /// </summary>
        private class RefCountedCursor
        {
            #region private fields

            /// <summary> the MSMQ Cursor field. </summary>
            Cursor cursorField;

            /// <summary> the reference count field. </summary>
            int refCountField;

            #endregion private fields

            /// <summary>
            /// Initializes a new instance of the RefCountedCursor class.  Reference count is initialized to 1.
            /// </summary>
            /// <param name="cursor">the MSMQ Cursor to be reference counted.</param>
            public RefCountedCursor(Cursor cursor)
            {
                this.cursorField = cursor;
                this.refCountField = 1;
            }

            /// <summary>
            /// Increases reference count by 1
            /// </summary>
            /// <returns>reference to this instance. </returns>
            public RefCountedCursor Acquire()
            {
                if (this.cursorField == null)
                {
                    throw new InvalidOperationException("this is disposed!");
                }

                Interlocked.Increment(ref this.refCountField);
                return this;
            }

            /// <summary>
            /// Decreases reference count by 1.  If ref count reaches 0, dispose underlying MSMQ cursor.
            /// </summary>
            public void Release()
            {
                Interlocked.Decrement(ref this.refCountField);
                if (this.refCountField == 0 && this.cursorField != null)
                {
                    this.cursorField?.Dispose();
                    this.cursorField = null;
                }
            }

            /// <summary>
            /// Gets wrapped MSMQ cursor.
            /// </summary>
            public Cursor MSMQCursor
            {
                get
                {
                    return this.cursorField;
                }
            }
        }
    }
}
