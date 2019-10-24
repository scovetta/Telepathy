// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Persistences.AzureQueuePersist
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.WindowsAzure.Storage.Blob;

    using Timer = System.Timers.Timer;

    internal class AzureQueueMessageFetcher
    {
        /// <summary> Maximum number of concurrent outstanding BeginPeek operations allowed</summary>
        protected const int DefaultMaxOutstandingFetchCount = 10;

        /// <summary> Default capacity of prefetch cache. </summary>
        protected const int DefaultPrefetchCacheCapacity = 1024;

        protected CloudBlobContainer blobContainer;

        protected IFormatter formatter;

        /// <summary> A value indicating whether this instance is disposed.</summary>
        protected volatile bool isDisposedField;

        protected long messageCount;

        protected int pendingFetchCount = 0;

        protected ConcurrentQueue<MessageResult> prefetchCache = new ConcurrentQueue<MessageResult>();

        protected Timer prefetchTimer = new System.Timers.Timer();

        private ConcurrentQueue<GetMessageState> currentGetMessageQueue = new ConcurrentQueue<GetMessageState>();

        private Guid fetcherId = Guid.NewGuid();

        private bool isRequest;

        private object lockFetchCount = new object();

        private ReaderWriterLockSlim lockPrefetchCache = new ReaderWriterLockSlim();

        /// <summary> Maximun number of outstanding BeginPeek operations</summary>
        private int maxOutstandingFetchCount;

        private int outstandingFetchCount;

        /// <summary> Prefetch cache capacity</summary>
        private int prefetchCacheCapacity;

        /// <summary> Credit for prefetching. It indicates if there is space available in prefetchCache for keeping prefetching result.</summary>
        private int prefetchCredit;

        public AzureQueueMessageFetcher(
            long messageCount,
            IFormatter messageFormatter,
            int prefetchCacheCapacity,
            int maxOutstandingFetchCount,
            CloudBlobContainer blobContainer)
        {
            this.messageCount = messageCount;
            this.formatter = messageFormatter;
            this.prefetchCacheCapacity = prefetchCacheCapacity;
            this.maxOutstandingFetchCount = maxOutstandingFetchCount;
            this.prefetchCredit = this.prefetchCacheCapacity;
            this.blobContainer = blobContainer;

            this.prefetchTimer.AutoReset = false;
            this.prefetchTimer.Interval = 500;
        }

        public void GetMessageAsync(PersistCallback callback, object state)
        {
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning(
                    "[AzureQueueMessageFetcher] .GetMessageAsync: fetcherId={0} the instance is disposed.",
                    this.fetcherId);
                return;
            }

            BrokerTracing.TraceVerbose(
                "[AzureQueueMessageFetcher] .GetMessageAsync: fetcherId={0} Get message come in.",
                this.fetcherId);

            // GetMessageAsync:
            // step 1, try to get message from prefetch cache. If succeeded, invoke the callback directly; or else,
            // step 2, save context of this GetMessageAsync call, including callback and callback state into this.currentGetMessageState, which will be handled when a message is retrieved back from MSMQ.
            // step 3, check if need to get more messages.  If so, initiate more BeginPeek calls into MSMQ
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
                Debug.WriteLine(
                    $"[AzureQueueMessageFetcher] .GetMessageAsync: working to get response. getMessageCount={getMessageCount}.");
            }
            else
            {
                Debug.WriteLine(
                    $"[AzureQueueMessageFetcher] .GetMessageAsync: working to get request. getMessageCount={getMessageCount}.");
            }

            this.lockPrefetchCache.EnterReadLock();
            try
            {
                if (this.prefetchCache == null)
                {
                    // this instance is disposed
                    BrokerTracing.TraceWarning(
                        "[AzureQueueMessageFetcher] .GetMessageAsync: the instance is disposed.");
                    return;
                }

                while (getMessageCount > 0)
                {
                    if (!(this.prefetchCache.Count > 0 && this.prefetchCache.TryDequeue(out result)) )
                    {
                        BrokerTracing.TraceVerbose("[AzureQueueMessageFetcher] .GetMessageAsync: cache miss");
                        result = null;
                        this.currentGetMessageQueue.Enqueue(getMessageState);
                        break;
                    }

                    BrokerTracing.TraceVerbose("[AzureQueueMessageFetcher] .GetMessageAsync: hit cache");
                    messageQueue.Enqueue(result);
                    getMessageCount--;
                }
            }
            finally
            {
                this.lockPrefetchCache.ExitReadLock();
            }

            Debug.WriteLine(
                $"[AzureQueueMessageFetcher] .GetMessageAsync: getMessageCount={getMessageCount} after dequeue.");

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
                    ThreadPool.QueueUserWorkItem(
                        this.GetMessageAsyncCallback,
                        new object[] { getMessageState, message });
                }
                else
                {
                    BrokerTracing.TraceWarning(
                        "[AzureQueueMessageFetcher] .GetMessageAsync: fetcherId={0} encountered null message result.",
                        this.fetcherId);
                }
            }

            this.CheckAndGetMoreMessages();
        }

        public void NotifyMoreMessages(long messageCount)
        {
            lock (this.lockFetchCount)
            {
                this.messageCount += messageCount;
            }

            // fill the cache with messages
            this.CheckAndGetMoreMessages();
        }

        public void SafeDispose()
        {
            BrokerTracing.TraceVerbose("[AzureQueueMessageFetcher] .SafeDispose fetcherId={0}.", this.fetcherId);

            // SafeDispose:
            // 1. mark this instance as disposed
            // 2. wait for all outstanding BeginPeek operations back, and then dispose the cursor.
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
                        if (!this.prefetchCache.IsEmpty)
                        {
                            BrokerTracing.TraceWarning(
                                "[AzureQueueMessageFetcher] .SafeDispose: fetcherId={0} PrefetchCache is not empty when disposing.",
                                this.fetcherId);
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
            }
        }

        protected void CheckAndGetMoreMessages()
        {
            while (!this.isDisposedField)
            {
                BrokerTracing.TraceVerbose(
                    "[AzureQueueMessageFetcher] .CheckAndGetMoreMessages: fetcherId={0} prefetchCredit={1}, outstandingFetchCount={2}, msmqMessageCount={3}, pendingFetchCount={4}",
                    this.fetcherId,
                    this.prefetchCredit,
                    this.outstandingFetchCount,
                    this.messageCount,
                    this.pendingFetchCount);

                int increasedFetchCount = 0;
                while (this.AddFetchCount())
                {
                    ++increasedFetchCount;
                    Interlocked.Increment(ref this.pendingFetchCount);
                }

                BrokerTracing.TraceVerbose(
                    "[AzureQueueMessageFetcher] .CheckAndGetMoreMessages: fetcherId={0} increasedFetchCount={1}",
                    this.fetcherId,
                    increasedFetchCount);
                return;
            }
        }

        protected void HandleMessageResult(MessageResult messageResult)
        {
            lock (this.lockFetchCount)
            {
                this.outstandingFetchCount--;
                if (messageResult.Message != null)
                {
                    this.messageCount--;
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

                ThreadPool.QueueUserWorkItem(
                    this.GetMessageAsyncCallback,
                    new object[] { getMessageState, messageResult });
            }

            this.CheckAndGetMoreMessages();
        }

        protected void RevertFetchCount()
        {
            lock (this.lockFetchCount)
            {
                this.outstandingFetchCount--;
                this.prefetchCredit++;
            }
        }

        private bool AddFetchCount()
        {
            lock (this.lockFetchCount)
            {
                if (this.outstandingFetchCount >= this.maxOutstandingFetchCount)
                {
                    return false;
                }

                if (this.outstandingFetchCount >= this.messageCount)
                {
                    return false;
                }

                if (this.prefetchCredit <= 0)
                {
                    return false;
                }

                this.outstandingFetchCount++;
                this.prefetchCredit--;

                return true;
            }
        }

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
                    "[AzureQueueMessageFetcher] .GetMessageAsyncCallbck: fetcherId={0} perform the message callback failed, the exception, {1}",
                    this.fetcherId,
                    e);
            }
        }

        protected class MessageResult
        {
            private Exception exception;

            private BrokerQueueItem message;

            public MessageResult(BrokerQueueItem message, Exception e)
            {
                this.message = message;
                this.exception = e;
            }

            public Exception Exception
            {
                get
                {
                    return this.exception;
                }
            }

            public BrokerQueueItem Message
            {
                get
                {
                    return this.message;
                }
            }
        }

        private class GetMessageState
        {
            /// <summary>the callback state object.</summary>
            private object callbackStateField;

            /// <summary>the callback that get the message.</summary>
            private PersistCallback messageCallbackField;

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
            /// Gets the mesage callback state object.
            /// </summary>
            public object CallbackState
            {
                get
                {
                    return this.callbackStateField;
                }
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
        }
    }
}