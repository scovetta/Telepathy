// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal.AzureQueue
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Xml;

    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// It is a wrapper of CloudQueue class, and it handles BeginAddMessage's
    /// callback lost issue.
    /// </summary>
    internal class ReliableQueueClient : DisposableObject
    {
        /// <summary>
        /// Waiting time before callback is invoked.
        /// </summary>
        /// <remarks>
        /// Notice: (azure burst) in heavy concurreny, callback can be invoked
        /// after more than 2 minutes, so have 5 minutes here.
        /// </remarks>
        private static readonly TimeSpan CallbackTimeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Timer period.
        /// </summary>
        private static readonly TimeSpan TimerPeriod = TimeSpan.FromMinutes(3);

        /// <summary>
        /// Cloud queue.
        /// </summary>
        private CloudQueue cloudQueue;

        /// <summary>
        /// It is a timer to trigger retry when callback is lost.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// It stores all the IAsyncResult returned by BeginAddMessage. Throw
        /// TimeoutException if callback is not invoked after CallbackTimeout.
        /// </summary>
        /// <remarks>
        /// Key: wcf message Id
        /// Value: IAsyncResult returned by BeginAddMessage
        /// </remarks>
        private ConcurrentDictionary<UniqueId, IAsyncResult> asyncResultCache
            = new ConcurrentDictionary<UniqueId, IAsyncResult>();

        /// <summary>
        /// Client id, it is only used in trace.
        /// </summary>
        private Guid clientId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the ReliableQueueClient class.
        /// </summary>
        /// <param name="queue">cloud queue</param>
        public ReliableQueueClient(CloudQueue queue)
        {
            this.cloudQueue = queue;
            this.timer = new Timer(this.TimerCallback, null, TimerPeriod, TimerPeriod);
        }

        /// <summary>
        /// Async method for add message to queue.
        /// </summary>
        /// <param name="message">queue message</param>
        /// <param name="callback">callback method</param>
        /// <param name="state">state object</param>
        /// <returns>async result</returns>
        public IAsyncResult BeginAddMessage(CloudQueueMessage message, AsyncCallback callback, ReliableState state)
        {
            state.Callback = callback;
            state.TriggerTime = DateTime.UtcNow + CallbackTimeout;
            IAsyncResult result = this.cloudQueue.BeginAddMessage(message, callback, state);
            this.asyncResultCache.AddOrUpdate(state.MessageId, result, (key, value) => result);

            SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0,
                "Add message {0} to the local cache, client {1}",
                state.MessageId,
                this.clientId);

            return result;
        }

        /// <summary>
        /// Async method for add message.
        /// </summary>
        /// <param name="asyncResult">async result</param>
        public void EndAddMessage(IAsyncResult asyncResult)
        {
            var reliableState = asyncResult.AsyncState as ReliableState;

            if (reliableState.Timeout)
            {
                throw new TimeoutException(
                    string.Format(
                        "Callback is lost after {0} minutes, client {1}",
                        CallbackTimeout.TotalMinutes,
                        this.clientId));
            }

            if (!reliableState.CallbackInvoked())
            {
                SessionBase.TraceSource.TraceInformation(
                    "Message {0}, client {1}",
                    reliableState.MessageId,
                    this.clientId);

                try
                {
                    this.cloudQueue.EndAddMessage(asyncResult);
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0,
                            "EndAddMessage failed with exception {0}, client {1}",
                            e,
                            this.clientId);
                }
                finally
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0,
                        "Try remove the message {0} from cache, client {1}",
                        reliableState.MessageId,
                        this.clientId);

                    IAsyncResult tmp;

                    this.asyncResultCache.TryRemove(reliableState.MessageId, out tmp);
                }
            }
        }

        /// <summary>
        /// Dispose current object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (this.timer != null)
                {
                    try
                    {
                        this.timer.Dispose();
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0,
                            "Dispose timer failed, {0}, client {1}",
                            e,
                            this.clientId);
                    }

                    this.timer = null;
                }

                // Trigger TimeoutException on each item in asyncResultQueue.
                // Otherwise if callback lost, we don't have a way to recover
                // from that.
                int count = this.asyncResultCache.Count;

                if (count > 0)
                {
                    SessionBase.TraceSource.TraceInformation(
                        "Trigger TimeoutException on each item in asyncResultQueue, count {0}, client {1}",
                        count,
                        this.clientId);

                    this.InvokeCallback(this.asyncResultCache.Values);
                }
            }
        }

        /// <summary>
        /// Invoke callback.
        /// </summary>
        /// <param name="asyncResults">a collection of IAsyncResult</param>
        private void InvokeCallback(ICollection<IAsyncResult> asyncResults)
        {
            try
            {
                asyncResults.AsParallel<IAsyncResult>().ForAll<IAsyncResult>(
                (asyncResult) =>
                {
                    try
                    {
                        var reliableState = asyncResult.AsyncState as ReliableState;

                        if (!reliableState.CallbackInvoked())
                        {
                            SessionBase.TraceSource.TraceInformation(
                                "Timeout happens, expected trigger time of message {0} is {1}, client {2}",
                                reliableState.MessageId,
                                reliableState.TriggerTime.ToLocalTime(),
                                this.clientId);

                            reliableState.Timeout = true;

                            reliableState.Callback(asyncResult);
                        }
                        else
                        {
                            SessionBase.TraceSource.TraceInformation(
                                "Callback of message {0} is already invoked, client {1}",
                                reliableState.MessageId,
                                this.clientId);
                        }
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0,
                            "Invoking callback failed, {0}, client {1}",
                            e,
                            this.clientId);
                    }
                });
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0,
                    "Error occurs, {0}, client {1}",
                    e,
                    this.clientId);
            }
        }

        /// <summary>
        /// Callback method of the timer.
        /// </summary>
        /// <param name="state">state object</param>
        private void TimerCallback(object state)
        {
            SessionBase.TraceSource.TraceInformation(
                "Enter timer callback, client {0}",
                this.clientId);

            List<IAsyncResult> lostCallbacks = new List<IAsyncResult>();
            List<UniqueId> messageIds = new List<UniqueId>();

            try
            {
                IAsyncResult result;
                DateTime utcNow = DateTime.UtcNow;
                var keys = this.asyncResultCache.Keys;

                SessionBase.TraceSource.TraceInformation(
                    "There are {0} messages in the local cache, client {1}",
                    keys.Count,
                    this.clientId);

                foreach (UniqueId messageId in keys)
                {
                    if (this.asyncResultCache.TryGetValue(messageId, out result))
                    {
                        var reliableState = result.AsyncState as ReliableState;

                        if (reliableState.TriggerTime <= utcNow)
                        {
                            lostCallbacks.Add(result);
                            messageIds.Add(messageId);

                            SessionBase.TraceSource.TraceInformation(
                                "Expected trigger time of message {0} is {1}, get it, client {2}",
                                messageId,
                                reliableState.TriggerTime.ToLocalTime(),
                                this.clientId);
                        }
                        else
                        {
                            SessionBase.TraceSource.TraceInformation(
                                "Expected trigger time of message {0} is {1}, ignore it, client {2}",
                                messageId,
                                reliableState.TriggerTime.ToLocalTime(),
                                this.clientId);
                        }
                    }
                }

                // invoke the lost callbacks before deleting them from cache
                if (lostCallbacks.Count > 0)
                {
                    SessionBase.TraceSource.TraceInformation(
                        "Invoke callbacks, count {0}, client {1}",
                        lostCallbacks.Count,
                        this.clientId);

                    this.InvokeCallback(lostCallbacks);
                }

                // delete callbacks from cache
                if (messageIds.Count > 0)
                {
                    foreach (UniqueId messageId in messageIds)
                    {
                        IAsyncResult tmp;
                        this.asyncResultCache.TryRemove(messageId, out tmp);

                        SessionBase.TraceSource.TraceInformation(
                            "Remove message {0} from cache, client {1}",
                            messageId,
                            this.clientId);
                    }
                }
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0,
                    "Error occurs, {0}, client {1}",
                    e,
                    this.clientId);
            }
        }

        /// <summary>
        /// State object for ReliableQueueClient.BeginAddMessage.
        /// </summary>
        internal class ReliableState
        {
            /// <summary>
            /// it indicates if callback is invoked
            /// </summary>
            private int callbackInvoked;

            /// <summary>
            /// Initializes a new instance of the ReliableState class.
            /// </summary>
            /// <param name="state">state object</param>
            /// <param name="messageId">wcf message Id</param>
            public ReliableState(object state, UniqueId messageId)
            {
                this.State = state;
                this.MessageId = messageId;
            }

            /// <summary>
            /// Gets the wcf message Id.
            /// </summary>
            public UniqueId MessageId
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the state object.
            /// </summary>
            public object State
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets or sets the callback.
            /// </summary>
            public AsyncCallback Callback
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the expected trigger time.
            /// </summary>
            public DateTime TriggerTime
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether timeout happens.
            /// </summary>
            public bool Timeout
            {
                get;
                set;
            }

            /// <summary>
            /// Check and set the callbackInvoked flag.
            /// </summary>
            /// <returns>
            /// If callback has already been invoked, return true. Otherwise
            /// set the flag and return false.
            /// </returns>
            public bool CallbackInvoked()
            {
                if (Interlocked.CompareExchange(ref this.callbackInvoked, 1, 0) == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
