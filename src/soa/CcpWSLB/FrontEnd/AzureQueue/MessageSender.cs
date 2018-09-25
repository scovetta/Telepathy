//-----------------------------------------------------------------------
// <copyright file="MessageSender.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     It sends response messages to response queue
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Xml;

    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// It sends response messages to response queue
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
        private Dictionary<int, Tuple<ConcurrentQueue<Message>, AzureStorageClient>> responseMessageClients;

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
            Dictionary<int, Tuple<ConcurrentQueue<Message>, AzureStorageClient>> responseMessageClients,
            int concurrency)
        {
            this.responseMessageClients = responseMessageClients;

            this.workers = new Worker[concurrency];

            for (int i = 0; i < this.workers.Length; i++)
            {
                this.workers[i] = new Worker(responseMessageClients);
            }

            this.timer = new Timer(this.CheckWorker, null, 0, Timeout.Infinite);
        }

        private void CheckWorker(object obj)
        {
            try
            {
                bool anyResponses = false;
                foreach (Tuple<ConcurrentQueue<Message>, AzureStorageClient> responseMessageClient in this.responseMessageClients.Values)
                {
                    if (responseMessageClient.Item1.Count > 0)
                    {
                        anyResponses = true;
                        break;
                    }
                }

                if (anyResponses)
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
                TraceUtils.TraceError("MessageSender.Worker", "CheckWorker", "Error occurs, {0}", e);
            }
        }

        /// <summary>
        /// Start the sender workers.
        /// </summary>
        public void Start()
        {
            TraceUtils.TraceInfo("MessageSender", "Start", "Start the message sender.");

            this.workers.AsParallel<Worker>().ForAll<Worker>(
            (w) =>
            {
                w.Start();
            });
        }


        public void Pause()
        {
            TraceUtils.TraceInfo("MessageSender", "Pause", "Pause the message sender.");

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
            /// The dictionary for message queues and storage clients
            /// Key: the session hash
            /// Value:local response queue and the Azure storage client
            /// </summary>
            private Dictionary<int, Tuple<ConcurrentQueue<Message>, AzureStorageClient>> responseMessageClients;

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
            /// <param name="responseMessages">local cache for the response messages</param>
            /// <param name="requestStorageClient">client for request storage</param>
            public Worker(
                Dictionary<int, Tuple<ConcurrentQueue<Message>, AzureStorageClient>> responseMessageClients)
            {
                this.responseMessageClients = responseMessageClients;

                this.timer = new Timer(this.InternalBeginAddResponse, null, Timeout.Infinite, Timeout.Infinite);
            }

            /// <summary>
            /// Start the worker.
            /// </summary>
            public void Start()
            {
                TraceUtils.TraceInfo("MessageSender.Worker", "Start", "Start the message sender worker.");

                this.stop = false;
                this.sleepPeriod = 0;

                // start the run if timer stopped
                lock(this.timerStopLock)
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

            public void Pause()
            {
                TraceUtils.TraceInfo("MessageSender.Worker", "Pause", "Pause the message sender worker.");

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
                                TraceUtils.TraceWarning("MessageSender.Worker", "Dispose", "Exception while disposing timer {0}", ex);
                            }

                            this.timer = null;
                        }

                        // wait for callback to be called in case there is
                        // on-the-fly async call
                        if (!this.waitHandler.Wait(TimeSpan.FromSeconds(5)))
                        {
                            TraceUtils.TraceWarning("MessageSender.Worker", "Dispose", "WaitHandler.Wait timeout happens.");
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
                        TraceUtils.TraceError("MessageSender.Worker", "Dispose", "Error occurs, {0}", e);
                    }
                }
            }

            /// <summary>
            /// Async Pattern. Callback of BeginProcessMessage method.
            /// </summary>
            /// <param name="state">state object</param>
            private void InternalBeginAddResponse(object state)
            {
                Message responseMessage = null;
                Tuple<ConcurrentQueue<Message>, AzureStorageClient> messageClient = null;
                try
                {
                    foreach (Tuple<ConcurrentQueue<Message>, AzureStorageClient> responseMessageClient in this.responseMessageClients.Values)
                    {
                        if (!responseMessageClient.Item1.TryDequeue(out responseMessage))
                        {
                            BrokerTracing.TraceInfo(
                                SoaHelper.CreateTraceMessage(
                                    "MessageSender.Worker",
                                    "InternalBeginAddResponse",
                                    "Local response cache is empty."));

                        }
                        else
                        {
                            messageClient = responseMessageClient;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "MessageSender.Worker",
                            "InternalBeginAddResponse",
                            string.Format("Failed to get response from local cache, {0}", e)));
                }

                if (responseMessage == null)
                {
                    this.TriggerTimer();

                    return;
                }

                BrokerTracing.TraceInfo(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "InternalBeginAddResponse",
                                "Retrieved message from local response cache."));

                try
                {

                    UniqueId messageId = SoaHelper.GetMessageId(responseMessage);
                    
                    if (messageId == null)
                    {
                        messageId = new UniqueId();
                    }

                    ReliableQueueClient.ReliableState reliableState = new ReliableQueueClient.ReliableState(responseMessage, messageClient, messageId);

                    this.waitHandler.Reset();

                    byte[] messageData;

                    using (responseMessage)
                    {
                        messageData = AzureQueueMessageItem.Serialize(responseMessage);
                    }

                    messageClient.Item2.BeginAddMessage(messageData, messageId, this.AddMessageCallback, reliableState);
                }
                catch (Exception e)
                {
                    this.waitHandler.Set();

                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "MessageSender.Worker",
                            "InternalBeginAddResponse",
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

                Message responseMessage = null;

                Tuple<ConcurrentQueue<Message>, AzureStorageClient> messageClient = null;

                try
                {
                    var reliableState = ar.AsyncState as ReliableQueueClient.ReliableState;

                    responseMessage = reliableState.State as Message;

                    messageId = reliableState.MessageId;

                    messageClient = reliableState.MessageClient;

                    try
                    {
                        BrokerTracing.TraceInfo(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "AddMessageCallback",
                                "Call EndAddMessage to complete adding response message to queue."));

                        messageClient.Item2.EndAddMessage(ar);
                    }
                    catch (TimeoutException e)
                    {
                        BrokerTracing.TraceWarning(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "AddMessageCallback",
                                "TimeoutException occurs, {0}", e));

                        // if the callback is lost, EndAddMessage method above
                        // throws TimeoutException, so have a retry here.
                        this.RetryAddResponse(responseMessage, messageClient);

                        return;
                    }
                    finally
                    {
                        this.waitHandler.Set();
                    }

                    this.InternalBeginAddResponse(null);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        SoaHelper.CreateTraceMessage(
                            "MessageSender.Worker",
                            "AddMessageCallback",
                            "Error occurs, {0}", e));

                    this.RetryAddResponse(responseMessage, messageClient);

                    this.TriggerTimer();
                }
            }

            /// <summary>
            /// Add response info back to the local cache for retry next time.
            /// </summary>
            /// <param name="result">response info</param>
            /// <param name="messageId">WCF message id of response</param>
            /// <param name="eprString">endpoint string of the message</param>
            private void RetryAddResponse(Message responseMessage, Tuple<ConcurrentQueue<Message>, AzureStorageClient> messageClient)
            {
                if (responseMessage != null)
                {
                    try
                    {
                        BrokerTracing.TraceError(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "RetryAddResponse",
                                "Add response back to the local cache."));

                        messageClient.Item1.Enqueue(responseMessage);
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
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
                            BrokerTracing.TraceInfo(
                                SoaHelper.CreateTraceMessage(
                                    "MessageSender.Worker",
                                    "TriggerTimer",
                                    "Worker stops."));
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
                    BrokerTracing.TraceInfo(
                            SoaHelper.CreateTraceMessage(
                                "MessageSender.Worker",
                                "TriggerTimer",
                                "Timer triggered with sleep time {0}.", this.sleepPeriod));

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
                    TraceUtils.TraceWarning(
                        "MessageSender.Worker",
                        "TriggerTimer",
                        "NullReferenceException occurs when timer is being disposed.");
                }
                catch (Exception e)
                {
                    TraceUtils.TraceError("MessageSender.Worker", "TriggerTimer", "Error occurs, {0}", e);
                }
            }
        }
    }
}
