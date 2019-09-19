// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Xml;

    /// <summary>
    /// It sends response messages to response queue and delete request
    /// messages from request queue.
    /// </summary>
    internal class MessageSender : DisposableObject
    {

        /// <summary>
        /// A collection of sender workers.
        /// </summary>
        private Worker[] workers;

        /// <summary>
        /// The timer to check the queue and worker status
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Time check internal when all workers are running
        /// </summary>
        private const int SleepTimeWhenRunning = 1000;

        /// <summary>
        /// Time check internal when all workers are stopped
        /// </summary>
        private const int SleepTimeWhenStopped = 100;

        /// <summary>
        /// Store the request message queue
        /// </summary>
        private ConcurrentQueue<Message> requestMessages;

        /// <summary>
        /// Indicate if the sender is running or not
        /// </summary>
        private bool IsRunning = false;

        /// <summary>
        /// Initializes a new instance of the MessageSender class.
        /// </summary>
        /// <param name="calculatingMessages">outstanding request messages</param>
        /// <param name="requestStorageClient">client for request storage</param>
        /// <param name="concurrency">count of the sender workers</param>
        public MessageSender(
            ConcurrentQueue<Message> requestMessages,
            AzureStorageClient requestStorageClient,
            int concurrency)
        {
            this.requestMessages = requestMessages;

            this.workers = new Worker[concurrency];

            for (int i = 0; i < this.workers.Length; i++)
            {
                this.workers[i] = new Worker(requestMessages, requestStorageClient);
            }

            this.timer = new Timer(this.CheckWorker, null, 0, Timeout.Infinite);

        }

        private void CheckWorker(object obj)
        {
            try
            {
                if (this.requestMessages.Count > 0)
                {
                    if (!this.IsRunning)
                    {
                        this.Start();
                        this.IsRunning = true;
                    }
                }
                else
                {
                    if (this.IsRunning)
                    {
                        this.Pause();
                        this.IsRunning = false;
                    }

                }
                if (this.IsRunning)
                {
                    this.timer.Change(SleepTimeWhenRunning, Timeout.Infinite);
                }
                else
                {
                    this.timer.Change(SleepTimeWhenStopped, Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceInformation("MessageSender, CheckWorker, exception is thrown: {0}.", e);
            }
        }


        /// <summary>
        /// Start the sender workers.
        /// </summary>
        public void Start()
        {
            SessionBase.TraceSource.TraceInformation("MessageSender, Start, Start the message sender.");

            this.workers.AsParallel<Worker>().ForAll<Worker>(
            (w) =>
            {
                w.Start();
            });
        }


        /// <summary>
        /// Pause the sender workers.
        /// </summary>
        public void Pause()
        {
            SessionBase.TraceSource.TraceInformation("MessageSender, Pause, Pause the message sender.");

            this.workers.AsParallel<Worker>().ForAll<Worker>(
            (w) =>
            {
                w.Pause();
            });

        }

        /// <summary>
        /// Dispose current object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                this.workers.AsParallel<Worker>().ForAll<Worker>(
                (w) =>
                {
                    w.Close();
                });

                this.timer.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// It sends response message to response queue and deletes request
        /// message from request queue.
        /// </summary>
        private class Worker : DisposableObject
        {
            /// <summary>
            /// The min period is 100ms for timer.
            /// </summary>
            private const int MinSleepTime = 100;

            /// <summary>
            /// The max period is 5sec for timer.
            /// </summary>
            private const int MaxSleepTime = 5 * 1000;

            /// <summary>
            /// It is the next sleep time used by back-off logic.
            /// </summary>
            private int sleepPeriod = MinSleepTime;

            /// <summary>
            /// It is a local cache for the response messages.
            /// </summary>
            private ConcurrentQueue<Message> requestMessages;

            /// <summary>
            /// It is client for response storage.
            /// </summary>
            private AzureStorageClient requestStorageClient;

            /// <summary>
            /// It is the timer for sending messages.
            /// </summary>
            private Timer timer;

            /// <summary>
            /// It indicates if stops sending messages.
            /// </summary>
            private bool stop;

            /// <summary>
            /// Indicate if the timer is stopped
            /// </summary>
            private bool timerStopped = true;

            /// <summary>
            /// The timer stopped locker to sync with the stop flag
            /// </summary>
            private object timerStopLock = new object();

            /// <summary>
            /// It makes sure that callback is invoked when dispose current object.
            /// </summary>
            private ManualResetEventSlim waitHandler = new ManualResetEventSlim(true);

            /// <summary>
            /// Initializes a new instance of the Worker class.
            /// </summary>
            /// <param name="calculatingMessages">outstanding request messages</param>
            /// <param name="requestMessages">local cache for the response messages</param>
            /// <param name="requestStorageClient">client for request storage</param>
            public Worker(
                ConcurrentQueue<Message> requestMessages,
                AzureStorageClient requestStorageClient)
            {
                this.requestMessages = requestMessages;
                this.requestStorageClient = requestStorageClient;
                this.timer = new Timer(this.InternalBeginAddRequest, null, Timeout.Infinite, Timeout.Infinite);
            }

            /// <summary>
            /// Start the worker.
            /// </summary>
            public void Start()
            {
                SessionBase.TraceSource.TraceInformation("MessageSender.Worker, Start, Start the message sender worker.");

                this.stop = false;
                this.sleepPeriod = 0;

                // start the run if timer stopped
                lock (this.timerStopLock)
                {
                    if (this.timerStopped)
                    {
                        this.timerStopped = false;
                        // trigger the timer
                        this.timer.Change(0, Timeout.Infinite);
                        this.sleepPeriod = MinSleepTime;

                    }
                }
            }

            /// <summary>
            /// Pause the worker
            /// </summary>
            public void Pause()
            {
                SessionBase.TraceSource.TraceInformation("MessageSender.Worker, Pause, Pause the message sender worker.");

                this.stop = true;
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
                                // ignore the error here
                                SessionBase.TraceSource.TraceInformation("MessageSender.Worker, Dispose, Exception while disposing timer {0}", ex);
                            }

                            this.timer = null;
                        }

                        // wait for callback to be called in case there is
                        // on-the-fly async call
                        if (!this.waitHandler.Wait(TimeSpan.FromSeconds(5)))
                        {
                            SessionBase.TraceSource.TraceInformation("MessageSender.Worker, Dispose, WaitHandler.Wait timeout happens.");
                        }

                        try
                        {
                            this.waitHandler.Dispose();
                        }
                        catch
                        {
                        }

                        this.waitHandler = null;
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceInformation("MessageSender.Worker, Dispose, Error occurs, {0}", e);
                    }
                }
            }

            /// <summary>
            /// Async Pattern. Callback of BeginProcessMessage method.
            /// </summary>
            /// <param name="state">state object</param>
            private void InternalBeginAddRequest(object state)
            {
                Message requestMessage = null;

                SessionBase.TraceSource.TraceInformation(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "InternalBeginAddRequest",
                                "Request queue length is {0}.", requestMessages.Count));

                try
                {
                    if (!this.requestMessages.TryDequeue(out requestMessage))
                    {
                        SessionBase.TraceSource.TraceInformation(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "InternalBeginAddRequest",
                                "Local response cache is empty."));
                    }
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "MessageSender.Worker",
                            "InternalBeginAddRequest",
                            string.Format("Failed to get response from local cache, {0}", e)));
                }

                if (requestMessage == null)
                {
                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "MessageSender.Worker",
                            "InternalBeginAddRequest",
                            string.Format("Get null request from local cache, trigger the time to wait")));

                    this.TriggerTimer();

                    return;
                }

                try
                {

                    UniqueId messageId = SoaHelper.GetMessageId(requestMessage);

                    if (messageId == null)
                    {
                        messageId = new UniqueId();
                    }

                    ReliableQueueClient.ReliableState reliableState = new ReliableQueueClient.ReliableState(requestMessage, messageId);

                    byte[] messageData;

                    using (requestMessage)
                    {
                        messageData = AzureQueueMessageItem.Serialize(requestMessage);
                    }

                    this.waitHandler.Reset();

                    this.requestStorageClient.BeginAddMessage(messageData, messageId, this.AddMessageCallback, reliableState);

                }
                catch (Exception e)
                {
                    this.waitHandler.Set();

                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "MessageSender.Worker",
                            "InternalBeginAddRequest",
                            "Failed to add response message, {0}", e));

                    this.TriggerTimer();
                }
            }

            /// <summary>
            /// Callback method of CloudQueue.BeginAddMessage. Proxy attempts to
            /// add the message to response queue.
            /// </summary>
            /// <param name="ar">async result object</param>
            private void AddMessageCallback(IAsyncResult ar)
            {

                UniqueId messageId;

                Message requestMessage = null;

                try
                {
                    var reliableState = ar.AsyncState as ReliableQueueClient.ReliableState;

                    requestMessage = reliableState.State as Message;

                    messageId = reliableState.MessageId;

                    try
                    {
                        SessionBase.TraceSource.TraceInformation(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "AddMessageCallback",
                                "Call EndAddMessage to complete adding response message to queue."));

                        this.requestStorageClient.EndAddMessage(ar);

                        // reset the time sleep time to MinSleepTime
                        this.sleepPeriod = MinSleepTime;

                    }
                    catch (TimeoutException e)
                    {
                        SessionBase.TraceSource.TraceInformation(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "AddMessageCallback",
                                "TimeoutException occurs, {0}", e));

                        // if the callback is lost, EndAddMessage method above
                        // throws TimeoutException, so have a retry here.
                        this.RetryAddRequest(requestMessage);

                        // not sure why return here, suppose need to TriggerTimer
                        return;
                    }
                    finally
                    {
                        this.waitHandler.Set();
                    }

                    this.InternalBeginAddRequest(null);
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceInformation(
                        SoaHelper.CreateTraceMessage(
                            "MessageSender.Worker",
                            "AddMessageCallback",
                            "Error occurs, {0}", e));

                    this.RetryAddRequest(requestMessage);

                    this.TriggerTimer();
                }
            }

            /// <summary>
            /// Add response info back to the local cache for retry next time.
            /// </summary>
            /// <param name="result">response info</param>
            /// <param name="messageId">WCF message id of response</param>
            /// <param name="eprString">endpoint string of the message</param>
            private void RetryAddRequest(Message requestMessage)
            {
                if (requestMessage != null)
                {
                    try
                    {
                        SessionBase.TraceSource.TraceInformation(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "RetryAddResponse",
                                "Add response back to the local cache."));

                        this.requestMessages.Enqueue(requestMessage);
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceInformation(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "RetryAddResponse",
                                "Error occurs, {0}", e));
                    }
                }
            }

            /// <summary>
            /// Start the timer.
            /// </summary>
            private void TriggerTimer()
            {
                if (this.stop)
                {
                    lock (this.timerStopLock)
                    {
                        if (this.stop)
                        {
                            this.timerStopped = true;
                            SessionBase.TraceSource.TraceInformation("MessageSender.Worker, TriggerTimer, Worker stops.");
                            return;
                        }
                        else
                        {
                            this.timerStopped = false;
                        }
                    }
                }

                try
                {
                    SessionBase.TraceSource.TraceInformation("MessageSender.Worker, TriggerTimer, sleepPeriod = {0}.", this.sleepPeriod);

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
                    SessionBase.TraceSource.TraceInformation(
                        "MessageSender.Worker, TriggerTimer, NullReferenceException occurs when timer is being disposed.");
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceInformation("MessageSender.Worker, TriggerTimer, Error occurs, {0}", e);
                }
            }
        }
    }
}
