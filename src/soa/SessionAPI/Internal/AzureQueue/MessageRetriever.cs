// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal

{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// Delegate of the message handler.
    /// </summary>
    /// <param name="messages">message collection</param>
    internal delegate void MessageHandler(IEnumerable<CloudQueueMessage> messages);

    /// <summary>
    /// Delegate of the handler for invalid queue.
    /// </summary>
    /// <param name="e">
    /// exception occurred when access the queue
    /// </param>
    internal delegate void InvalidQueueHandler(StorageException e);

    /// <summary>
    /// It monitors the specified queue, and gets messages from it.
    /// </summary>
    internal class MessageRetriever : DisposableObject
    {
        /// <summary>
        /// A collection of retriever workers.
        /// </summary>
        private Worker[] workers;

        /// <summary>
        /// It is called when StorageException happens.
        /// </summary>
        private InvalidQueueHandler invalidQueueHandler;

        /// <summary>
        /// It indicates if InvalidQueueHandler is called. InvalidQueueHandler
        /// is expected to be called at most once.
        /// </summary>
        private int invalidQueueHandlerInvoked;

        /// <summary>
        /// Storage queue name.
        /// </summary>
        private string queueName;

        /// <summary>
        /// Initializes a new instance of the MessageRetriever class.
        /// </summary>
        /// <param name="queue">target Azure storage queue</param>
        /// <param name="concurrency">concurrency for getting messages</param>
        /// <param name="visibleTimeout">message visible timeout</param>
        /// <param name="messageHandler">message handler</param>
        /// <param name="invalidQueueHandler">handle invalid queue</param>
        public MessageRetriever(CloudQueue queue, int concurrency, TimeSpan visibleTimeout, MessageHandler messageHandler, InvalidQueueHandler invalidQueueHandler)
        {
            this.queueName = queue.Name;
            this.invalidQueueHandler = invalidQueueHandler;
            this.workers = new Worker[concurrency];

            for (int i = 0; i < this.workers.Length; i++)
            {
                this.workers[i] = new Worker(queue, visibleTimeout, messageHandler, this.HandleInvalidQueue);
            }
        }

        /// <summary>
        /// Start the message retriever.
        /// </summary>
        public void Start()
        {
            SessionBase.TraceSource.TraceInformation(
                "MessageRetriever, Start, Start {0} workers, queue {1}", this.workers.Length, this.queueName);

            this.workers.AsParallel<Worker>().ForAll<Worker>(
            (w) =>
            {
                w.Start();
            });
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            SessionBase.TraceSource.TraceInformation(
                "MessageRetriever, Dispose, Queue {0}, disposing is {1}", this.queueName, disposing);

            if (disposing)
            {
                this.workers.AsParallel<Worker>().ForAll<Worker>(
                (w) =>
                {
                    w.Close();
                });
            }
        }

        /// <summary>
        /// Invoke the invalidQueueHandler.
        /// </summary>
        /// <param name="e">exception occurred when access the queue</param>
        private void HandleInvalidQueue(StorageException e)
        {
            if (this.invalidQueueHandler != null)
            {
                if (Interlocked.CompareExchange(ref this.invalidQueueHandlerInvoked, 1, 0) == 0)
                {
                    try
                    {
                        // invoke the handler in threadpool thread rather than
                        // in IO completion thread
                        ThreadPool.QueueUserWorkItem((s) => { this.invalidQueueHandler(e); });
                    }
                    catch (Exception ex)
                    {
                        SessionBase.TraceSource.TraceEvent(
                            TraceEventType.Error,
                            0,
                            "MessageRetriever, HandleInvalidQueue, Error occurs, queue {0}, {1}",
                            this.queueName,
                            ex);
                    }
                }
            }
        }

        /// <summary>
        /// It gets messages from a specified storage queue.
        /// </summary>
        private class Worker : DisposableObject
        {
            /// <summary>
            /// The min period is 100ms for polling messages from specified queue.
            /// </summary>
            private const int MinSleepTime = 100;

            /// <summary>
            /// The max period is 5sec for polling messages from specified queue.
            /// </summary>
            private const int MaxSleepTime = 5 * 1000;

            /// <summary>
            /// It is the next sleep time used by back-off logic.
            /// </summary>
            private int sleepPeriod = MinSleepTime;

            /// <summary>
            /// It is the target queue for polling messages.
            /// </summary>
            private CloudQueue queue;

            /// <summary>
            /// It is called when get messages from queue.
            /// </summary>
            private MessageHandler messageHandler;

            /// <summary>
            /// It is the timer for polling messages.
            /// </summary>
            private Timer timer;

            /// <summary>
            /// It is the visible timeout when get message from the queue.
            /// </summary>
            private TimeSpan visibleTimeout;

            /// <summary>
            /// It is called when exception happens when access queue.
            /// </summary>
            private InvalidQueueHandler invalidQueueHandler;

            /// <summary>
            /// It indicates if stops polling messages.
            /// </summary>
            private bool stop;

            /// <summary>
            /// It makes sure that callback is invoked when dispose current object.
            /// </summary>
            private ManualResetEventSlim waitHandler = new ManualResetEventSlim(false);

            /// <summary>
            /// This Id is only used in trace.
            /// </summary>
            private Guid workerId = Guid.NewGuid();

            /// <summary>
            /// Initializes a new instance of the Worker class.
            /// </summary>
            /// <param name="queue">target Azure storage queue</param>
            /// <param name="visibleTimeout">message visible timeout</param>
            /// <param name="messageHandler">message handler</param>
            /// <param name="invalidQueueHandler">handle invalid queue</param>
            public Worker(CloudQueue queue, TimeSpan visibleTimeout, MessageHandler messageHandler, InvalidQueueHandler invalidQueueHandler)
            {
                this.queue = queue;
                this.visibleTimeout = visibleTimeout;
                this.messageHandler = messageHandler;
                this.invalidQueueHandler = invalidQueueHandler;

                // do not start the timer here
                this.timer = new Timer(this.InternalBeginGetMessages, null, Timeout.Infinite, Timeout.Infinite);
            }

            /// <summary>
            /// Start the retriever.
            /// </summary>
            public void Start()
            {
                this.stop = false;

                this.timer.Change(0, Timeout.Infinite);
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
                    try
                    {
                        // set stop flag
                        this.stop = true;

                        if (this.timer != null)
                        {
                            try
                            {
                                this.timer.Dispose();
                            }
                            catch (Exception ex)
                            {
                                SessionBase.TraceSource.TraceEvent(
                                    TraceEventType.Warning,
                                    0,
                                    "MessageRetriever.Worker, Dispose, Disposing timer exception {0}",
                                    ex);
                            }

                            this.timer = null;
                        }

                        // wait for callback to be called in case there is
                        // on-the-fly async call
                        if (!this.waitHandler.Wait(TimeSpan.FromSeconds(5)))
                        {
                            SessionBase.TraceSource.TraceEvent(
                                TraceEventType.Warning,
                                0,
                                "WaitHandler.Wait timeout happens, worker {0}, queue {1}",
                                this.workerId,
                                this.queue.Name);
                        }

                        try
                        {
                            this.waitHandler.Dispose();
                        }
                        catch (Exception ex)
                        {
                            SessionBase.TraceSource.TraceEvent(
                                TraceEventType.Warning,
                                0,
                                "Disposing waitHandler exception {0}",
                                ex);
                        }

                        this.waitHandler = null;
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceEvent(
                            TraceEventType.Error,
                            0,
                            "Error occurs, worker {0}, queue {1}, {2}",
                            this.workerId,
                            this.queue.Name,
                            e);
                    }
                }
            }

            /// <summary>
            /// Call cloud queue's async method to get messages.
            /// </summary>
            /// <param name="state">state object</param>
            private void InternalBeginGetMessages(object state)
            {
                if (this.stop)
                {
                    // if current worker already stops, just return
                    return;
                }

                // this Id is only used in trace log to track latency for each
                // BeginGetMessages method call
                Guid callId = Guid.NewGuid();

                try
                {
                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Verbose,
                        0,
                        "Call BeginGetMessages, worker {0}, call Id {1}, queue {2}",
                        this.workerId,
                        callId,
                        this.queue.Name);

                    this.waitHandler.Reset();

                    this.queue.BeginGetMessages(Constant.GetQueueMessageBatchSize, this.visibleTimeout, null, null, this.GetMessagesCallback, callId);

                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Verbose,
                        0,
                        "BeginGetMessages returns, worker {0}, call Id {1}, queue {2}",
                        this.workerId,
                        callId,
                        this.queue.Name);
                }
                catch (StorageException e)
                {
                    this.waitHandler.Set();

                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Error,
                        0,
                        "BeginGetMessages failed, worker {0}, call Id {1}, queue {2}, error code {3}, {4}",
                        this.workerId,
                        callId,
                        this.queue.Name,
                        BurstUtility.GetStorageErrorCode(e),
                        e);

                    this.HandleInvalidQueue(e);

                    this.TriggerTimer();
                }
                catch (Exception e)
                {
                    this.waitHandler.Set();

                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Error,
                        0,
                        "Error occurs, worker {0}, call Id {1}, queue {2}, {3}",
                        this.workerId,
                        callId,
                        this.queue.Name,
                        e);

                    this.TriggerTimer();
                }
            }

            /// <summary>
            /// Callback of the BeginGetMessages method.
            /// </summary>
            /// <param name="ar">async result</param>
            private void GetMessagesCallback(IAsyncResult ar)
            {
                Guid callId = (Guid)ar.AsyncState;

                try
                {
                    IEnumerable<CloudQueueMessage> messages = null;

                    try
                    {
                        SessionBase.TraceSource.TraceEvent(
                            TraceEventType.Verbose,
                            0,
                            "Call EndGetMessages, worker {0}, call Id {1}, queue {2}",
                            this.workerId,
                            callId,
                            this.queue.Name);

                        messages = this.queue.EndGetMessages(ar);

                        SessionBase.TraceSource.TraceEvent(
                            TraceEventType.Verbose,
                            0,
                            "EndGetMessages returns, worker {0}, call Id {1}, queue {2}",
                            this.workerId,
                            callId,
                            this.queue.Name);
                    }
                    catch (StorageException e)
                    {
                        SessionBase.TraceSource.TraceEvent(
                            TraceEventType.Error,
                            0,
                            "EndGetMessages failed, worker {0}, call Id {1}, queue {2}, error code {3}, {4}",
                            this.workerId,
                            callId,
                            this.queue.Name,
                            BurstUtility.GetStorageErrorCode(e),
                            e);

                        this.HandleInvalidQueue(e);
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceEvent(
                            TraceEventType.Error,
                            0,
                            "EndGetMessages failed, worker {0}, call Id {1}, queue {2}, {3}",
                            this.workerId,
                            callId,
                            this.queue.Name,
                            e);
                    }
                    finally
                    {
                        this.waitHandler.Set();
                    }

                    int count = 0;

                    if (messages != null)
                    {
                        count = messages.Count<CloudQueueMessage>();
                    }

                    if (count > 0)
                    {
                        SessionBase.TraceSource.TraceEvent(
                            TraceEventType.Verbose,
                            0,
                            "Get {0} messages from the queue, worker {1}, call Id {2}, queue {3}",
                            count,
                            this.workerId,
                            callId,
                            this.queue.Name);

                        this.sleepPeriod = 0;

                        // Make sure messageHandler is a fast operation, call
                        // it before getting messages next time, in case
                        // current thread doesn't get chance on time to call
                        // messageHandler if BeginGetMessages's callback is
                        // invoked on current thread again.
                        if (this.messageHandler != null)
                        {
                            try
                            {
                                this.messageHandler(messages);
                            }
                            catch (Exception e)
                            {
                                SessionBase.TraceSource.TraceEvent(
                                    TraceEventType.Error,
                                    0,
                                    "Message handler throws exception, worker {0}, call Id {1}, queue {2}, {3}",
                                    this.workerId,
                                    callId,
                                    this.queue.Name,
                                    e);
                            }
                        }

                        this.InternalBeginGetMessages(null);
                    }
                    else
                    {
                        SessionBase.TraceSource.TraceEvent(
                            TraceEventType.Verbose,
                            0,
                            "Get 0 message from the queue, worker {0}, call Id {1}, queue {2}",
                            this.workerId,
                            callId,
                            this.queue.Name);

                        this.TriggerTimer();
                    }
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Error,
                        0,
                        "Error occurs, worker {0}, call Id {1}, queue {2}, {3}",
                        this.workerId,
                        callId,
                        this.queue.Name,
                        e);

                    this.TriggerTimer();
                }
            }

            /// <summary>
            /// Start the timer.
            /// </summary>
            private void TriggerTimer()
            {
                if (this.stop)
                {
                    SessionBase.TraceSource.TraceInformation(
                        "Worker stops, worker {0}, queue {1}",
                        this.workerId,
                        this.queue.Name);

                    return;
                }

                try
                {
                    this.timer.Change(this.sleepPeriod, Timeout.Infinite);

                    if (this.sleepPeriod == 0)
                    {
                        this.sleepPeriod = MinSleepTime;
                    }
                    else
                    {
                        this.sleepPeriod *= 2;

                        if (this.sleepPeriod > MaxSleepTime)
                        {
                            this.sleepPeriod = MaxSleepTime;
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Warning,
                        0,
                        "NullReferenceException occurs when timer is being disposed, worker {0}, queue {1}",
                        this.workerId,
                        this.queue.Name);
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Error,
                        0,
                        "Error occurs, worker {0}, queue {1}, {2}",
                        this.workerId,
                        this.queue.Name,
                        e);
                }
            }

            /// <summary>
            /// Invoke invalidQueueHandler if failed to access the queue.
            /// </summary>
            /// <remarks>
            /// Notice: Only handle the case that queue is not found now. May
            /// add more error handlings for specific queue issues if necessary.
            /// </remarks>
            /// <param name="e">exception happens when access the queue</param>
            private void HandleInvalidQueue(StorageException e)
            {
                SessionBase.TraceSource.TraceEvent(
                    TraceEventType.Error,
                    0,
                    "StorageException, worker {0}, queue {1}, error code {2}, {3}",
                    this.workerId,
                    this.queue.Name,
                    BurstUtility.GetStorageErrorCode(e),
                    e);

                if (BurstUtility.IsQueueNotFound(e))
                {
                    // Invoke invalidQueueHandler if the exception indicates
                    // that the queue is not found.
                    if (this.invalidQueueHandler != null)
                    {
                        this.invalidQueueHandler(e);
                    }
                }
            }
        }
    }
}
