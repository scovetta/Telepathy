//------------------------------------------------------------------------------
// <copyright file="BrokerObserver.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Observer of the broker
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;

    /// <summary>
    /// Observer of the broker
    /// </summary>
    internal sealed class BrokerObserver : IBrokerObserver
    {
        /// <summary>
        /// Stores the incoming request observer item
        /// </summary>
        private ObserverItem incomingRequestObserver;

        /// <summary>
        /// Stores the calls observer item
        /// </summary>
        private ObserverItem callsObserver;

        /// <summary>
        /// Stores the outgoing response observer item
        /// </summary>
        private ObserverItem outgoingResponseObserver;

        /// <summary>
        /// Stores the faulted calls observer item
        /// </summary>
        private ObserverItem faultedCallsObserver;

        /// <summary>
        /// Stores the call duration observer
        /// </summary>
        private ObserverItem callDurationObserver;

        /// <summary>
        /// Stores the total count
        /// </summary>
        private long total;

        /// <summary>
        /// Stores the processed count
        /// </summary>
        private long processed;

        /// <summary>
        /// Stores the processing count
        /// </summary>
        private long processing;

        /// <summary>
        /// Stores the number of failed responses that have been purged
        /// </summary>
        private long purgedFailed;

        /// <summary>
        /// Stores the number of responses that have been purged
        /// </summary>
        private long purgedProcessed;

        /// <summary>
        /// Stores the total number of requests/responses that have been purged
        /// </summary>
        private long purgedTotal;

        /// <summary>
        /// Stores the directly replied count
        /// </summary>
        private long directlyReplied;

        /// <summary>
        /// Stores the reply fetched count
        /// </summary>
        private long replyFetched;

        /// <summary>
        /// lock object that protects purge counters, including purgedTotal, purgedProcessed, and purgedFailed
        /// </summary>
        private object lockPurgedCounters = new object();

        /// <summary>
        /// a flag indicating whether the throttling is enabled
        /// </summary>
        private bool enableThrottling;

        /// <summary>
        /// throttling start threshold
        /// </summary>
        private int messageThrottleStartThreshold;

        /// <summary>
        /// throttling stop threshold
        /// </summary>
        private int messageThrottleStopThreshold;

        /// <summary>
        /// number of queued messages (request/response) 
        /// </summary>
        private long queuedMessageCount;

        /// <summary>
        /// a flag indicating if broker is in throttling state
        /// </summary>
        private int isThrottling;

        /// <summary>
        /// The reemitted count.
        /// </summary>
        private long reemitted;

        /// <summary>
        /// Initializes a new instance of the BrokerObserver class
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="clientInfoList">indicating the client info list</param>
        public BrokerObserver(SharedData sharedData, ClientInfo[] clientInfoList)
        {
            this.callsObserver = new ObserverItem(BrokerPerformanceCounterKey.Calculations);
            this.faultedCallsObserver = new ObserverItem(BrokerPerformanceCounterKey.Faults);
            this.incomingRequestObserver = new ObserverItem(BrokerPerformanceCounterKey.RequestMessages);
            this.outgoingResponseObserver = new ObserverItem(BrokerPerformanceCounterKey.ResponseMessages);
            this.callDurationObserver = new ObserverItem(BrokerPerformanceCounterKey.None);

            this.purgedTotal = sharedData.BrokerInfo.PurgedTotal;
            this.purgedProcessed = sharedData.BrokerInfo.PurgedProcessed;
            this.purgedFailed = sharedData.BrokerInfo.PurgedFailed;
            this.total = this.purgedTotal;
            this.processed = this.purgedProcessed;
            this.faultedCallsObserver.Increment(this.purgedFailed);

            foreach (ClientInfo info in clientInfoList)
            {
                this.total += info.TotalRequestsCount;
                this.processed += info.ProcessedRequestsCount;
                this.faultedCallsObserver.Increment(info.FailedRequestsCount);
            }

            this.enableThrottling = !sharedData.BrokerInfo.Durable;
            this.messageThrottleStartThreshold = sharedData.Config.Monitor.MessageThrottleStartThreshold;
            this.messageThrottleStopThreshold = sharedData.Config.Monitor.MessageThrottleStopThreshold;

            BrokerTracing.TraceInfo("[BrokerObserver] Initial counter: Total = {0}, Processed = {1}, Failed = {2}, PurgedTotal = {3}, PurgedProcessed = {4}, PurgedFailed = {5}",
                                    this.total,
                                    this.processed,
                                    this.faultedCallsObserver.Total,
                                    this.purgedTotal,
                                    this.purgedProcessed,
                                    this.purgedFailed);
        }

        /// <summary>
        /// Gets the total calls number
        /// </summary>
        public long TotalCalls
        {
            get { return Interlocked.Read(ref this.total); }
        }

        /// <summary>
        /// Gets the purged total number
        /// </summary>
        public long PurgedTotal
        {
            get { return Interlocked.Read(ref this.purgedTotal); }
        }

        /// <summary>
        /// Gets the purged processed number
        /// </summary>
        public long PurgedProcessed
        {
            get { return Interlocked.Read(ref this.purgedProcessed); }
        }

        /// <summary>
        /// Gets the purged failed number
        /// </summary>
        public long PurgedFailed
        {
            get { return Interlocked.Read(ref this.purgedFailed); }
        }

        /// <summary>
        /// Gets the total queue length
        /// </summary>
        /// <returns>returns the total queue length</returns>
        public long GetQueuedRequestsCount()
        {
            return Interlocked.Read(ref this.total) - Interlocked.Read(ref this.processing) - Interlocked.Read(ref this.processed);
        }

        /// <summary>
        /// Gets the stored response count
        /// </summary>
        /// <returns>returns the number of stored responses</returns>
        public long GetStoredResponseCount()
        {
            return Interlocked.Read(ref this.processed) - Interlocked.Read(ref this.replyFetched);
        }

        /// <summary>
        /// Whether all requests are processed. Used for v2 proxy client to sync disconnect the client when it is idle.
        /// </summary>
        /// <returns></returns>
        public bool AllRequestProcessed()
        {
            return Interlocked.Read(ref this.processing) == 0
                && Interlocked.Read(ref this.processed) == Interlocked.Read(ref this.total);
        }

        /// <summary>
        /// Throttling start event handler
        /// </summary>
        public event EventHandler OnStartThrottling;

        /// <summary>
        /// Throttling stop event handler
        /// </summary>
        public event EventHandler OnStopThrottling;

        /// <summary>
        /// Indicates the observer that there is an incoming request
        /// </summary>
        public void IncomingRequest()
        {
            this.incomingRequestObserver.Increment(1);
            Interlocked.Increment(ref this.total);

            // check if should start throttling
            if (this.enableThrottling)
            {
                if (Interlocked.Increment(ref this.queuedMessageCount) > this.messageThrottleStartThreshold)
                {
                    if (Interlocked.CompareExchange(ref this.isThrottling, 1, 0) == 0)
                    {
                        if (this.OnStartThrottling != null)
                        {
                            this.OnStartThrottling(this, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Indicates the observer that there is an outgoing response
        /// </summary>
        public void OutgoingResponse()
        {
            OutgoingResponse(1);
        }

        /// <summary>
        /// Batch version to indicate the observer that there is an outgoing response
        /// </summary>
        public void OutgoingResponse(int count)
        {
            this.outgoingResponseObserver.Increment(count);
            Interlocked.Add(ref this.replyFetched, count);

            // check if should stop throttling
            if (enableThrottling)
            {
                if (Interlocked.Add(ref this.queuedMessageCount, -count) < this.messageThrottleStopThreshold)
                {
                    if (Interlocked.CompareExchange(ref this.isThrottling, 0, 1) == 1)
                    {
                        if (this.OnStopThrottling != null)
                        {
                            this.OnStopThrottling(this, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Indicates the observer that a call is completed
        /// </summary>
        /// <param name="duration">indicating the call duration</param>
        public void CallComplete(long duration)
        {
            this.callDurationObserver.AddValue(duration);
        }

        /// <summary>
        /// A request is reemitted.
        /// </summary>
        public void RequestReemit()
        {
            Interlocked.Increment(ref this.reemitted);
        }

        /// <summary>
        /// Informs that a request is being processed
        /// </summary>
        public void RequestProcessing()
        {
            Interlocked.Increment(ref this.processing);
        }

        /// <summary>
        /// Informs that a request is completed (either finished correctly or failed with some exception)
        /// </summary>
        public void RequestProcessingCompleted()
        {
            Interlocked.Decrement(ref this.processing);
        }

        /// <summary>
        /// Reduce the uncommitted message count
        /// </summary>
        /// <param name="count">indicating the count of the uncommitted message</param>
        public void ReduceUncommittedCounter(long count)
        {
            Interlocked.Add(ref this.total, -count);
        }

        /// <summary>
        /// Informs that a number of responses are persisted
        /// </summary>
        /// <param name="responseNumber">indicating the response number</param>
        /// <param name="faultedResponseNumber">indicating the faulted response number</param>
        public void ResponsePersisted(long responseNumber, long faultedResponseNumber)
        {
            // [Bug 9713]: first decrease "processing", and then increase "processed".
            Interlocked.Add(ref this.processing, -responseNumber);
            Interlocked.Add(ref this.processed, responseNumber);

            this.faultedCallsObserver.Increment(faultedResponseNumber);
            this.callsObserver.Increment(responseNumber);
        }

        /// <summary>
        /// Informs that a client is purged
        /// </summary>
        /// <param name="total">indicating the total number</param>
        /// <param name="failed">indicating the failed number</param>
        /// <param name="processed">indicating the processed number</param>
        public void ClientPurged(long total, long failed, long processed)
        {
            lock (this.lockPurgedCounters)
            {
                Interlocked.Add(ref this.purgedTotal, total);
                Interlocked.Add(ref this.purgedFailed, failed);
                Interlocked.Add(ref this.purgedProcessed, processed);
            }
        }

        /// <summary>
        /// Informs that a reply has been sent back to the client
        /// </summary>
        public void ReplySent(bool isFault)
        {
            // [Bug 9713]: first increase "processed", and then increase "directlyReplied"
            Interlocked.Increment(ref this.processed);
            Interlocked.Increment(ref this.directlyReplied);
            this.callsObserver.Increment(1);
            if (isFault)
            {
                this.faultedCallsObserver.Increment(1);
            }
        }

        /// <summary>
        /// Gets the counters
        /// </summary>
        /// <param name="totalCalls">output the total calls</param>
        /// <param name="totalFaulted">output the total faulted</param>
        /// <param name="callDuration">output call duration</param>
        /// <param name="outstands">output the outstands</param>
        /// <param name="incomingRate">output incoming rate</param>
        /// <param name="processed">output the processed count</param>
        /// <param name="processing">output the processing count</param>
        /// <param name="purgedProcessed">output the purged processed count</param>
        /// <param name="reemitted">the reemitted request count</param>
        public void GetCounters(out long totalCalls, out long totalFaulted, out long callDuration, out long outstands, out long incomingRate, out long processed, out long processing, out long purgedProcessed, out long reemitted)
        {
            //
            // Math of counters:
            //  totalCalls = processed + outstands,
            //  outstands = processing + incoming
            // So:
            //  totalCalls = processed + processing + incoming
            //
            // Update logic for a group of counters: processed, processing, directlyReplied
            // - on dispatching a request, processing++
            // - on receiving a response, processing--, processed++, and
            // - if the response is sent to client directly instead of put into broker queue, directlyReplied++
            //
            // [Bug 9731] update above group of counters without using a lock
            // To acheive this, below operation sequences should be followed:
            // 1. on receiving a resonse, first decrease "processing", and then increase "processed".  This ensures "processed + processing <= totalCalls"
            // 2. when to increase "directlyReplied", first increase "processed", and then increase "directlyReplied".
            // 3. when to read "directlyReplied", first read "directlyReplied", and then read "processed".  2 & 3 ensures that "directlyReplied <= processed".
            //

            // read "directlyReplied" before "processed"
            long tmpDirectlyReplied = Interlocked.Read(ref this.directlyReplied);

            totalCalls = Interlocked.Read(ref this.total);
            processing = Interlocked.Read(ref this.processing);
            processed = Interlocked.Read(ref this.processed);
            reemitted = Interlocked.Read(ref this.reemitted);
            totalFaulted = this.faultedCallsObserver.Total;
            incomingRate = this.callsObserver.Changed;
            callDuration = this.callDurationObserver.Average;

            long tmpPurgedTotal = 0;
            long tmpPurgedProcessed = 0;
            lock (this.lockPurgedCounters)
            {
                tmpPurgedTotal = Interlocked.Read(ref this.purgedTotal);
                tmpPurgedProcessed = Interlocked.Read(ref this.purgedProcessed);
            }

            purgedProcessed = tmpPurgedProcessed + tmpDirectlyReplied;
            outstands = totalCalls - processed - tmpPurgedTotal + tmpPurgedProcessed;

            BrokerTracing.TraceVerbose(
                "[BrokerObserver] GetCounters: totalCalls = {0}, processed = {1}, processing = {2}, purgedTotal = {3}, tmpPurgedProcessed = {4}, purgedProcessed = {5}, reemitted = {6}",
                totalCalls,
                processed,
                processing,
                this.purgedTotal,
                tmpPurgedProcessed,
                purgedProcessed,
                reemitted);
        }

        /// <summary>
        /// Update perf counter
        /// </summary>
        public void UpdatePerformanceCounter()
        {
            // Update performance counter
            this.callsObserver.UpdatePerformanceCounter();
            this.faultedCallsObserver.UpdatePerformanceCounter();
            this.incomingRequestObserver.UpdatePerformanceCounter();
            this.outgoingResponseObserver.UpdatePerformanceCounter();
        }
    }
}
