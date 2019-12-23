// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.FrontEnd;

    /// <summary>
    /// the delegate for the get request callback.
    /// </summary>
    /// <param name="queueItem">the message from the queue.</param>
    /// <param name="state">the state object for the callback.</param>
    internal delegate void BrokerQueueCallback(BrokerQueueItem queueItem, object state);

    /// <summary>
    /// the broker queue event delegation.
    /// </summary>
    /// <param name="eventId">the event id.</param>
    /// <param name="eventArgs">the event args.</param>
    internal delegate void BrokerQueueEventDelegate(BrokerQueueEventId eventId, EventArgs eventArgs);

    /// <summary>
    /// define the broker envent id.
    /// </summary>
    internal enum BrokerQueueEventId
    {
        /// <summary>
        /// all the requests in the queue are dispatched through the get request callback.
        /// </summary>
        AllRequestsDispatched,

        /// <summary>
        /// all the requests in the queue already gets the corresponding responses.
        /// </summary>
        AllRequestsProcessed,

        /// <summary>
        /// all the responses in the broker queue are dispatched throught the registered responses callback.
        /// </summary>
        AllResponesDispatched,

        /// <summary>
        /// current available responses dispatched.
        /// </summary>
        AvailableResponsesDispatched,
    }

    /// <summary>
    /// the broker queue abstract interface.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "this is a queue, so the queue suffix is proper.")]
    internal abstract class BrokerQueue : IDisposable
    {
        /// <summary>
        /// Stores the shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// Stores the reference to broker queue factory
        /// </summary>
        private BrokerQueueFactory brokerQueueFactory;

        /// <summary>
        /// Initializes a new instance of the BrokerQueue class
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="factory">indicating the broker queue factory</param>
        protected BrokerQueue(SharedData sharedData, BrokerQueueFactory factory)
        {
            this.sharedData = sharedData;
            this.brokerQueueFactory = factory;
        }

        protected private BrokerQueue()
        {
        }

        /// <summary>
        /// the exception receive event.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> OnExceptionEvent;

        /// the putting responses success event.
        /// </summary>
        public event EventHandler<PutResponsesSuccessEventArgs> OnPutResponsesSuccessEvent;

        /// <summary>
        /// the fatal exception event
        /// </summary>
        public event EventHandler<ExceptionEventArgs> OnFatalExceptionEvent;

        /// <summary>
        /// the broker queue event.
        /// </summary>
        public event BrokerQueueEventDelegate OnEvent;

        /// <summary>
        /// Gets a value indicating whether all requests are received.
        /// </summary>
        public abstract bool EOMReceived { get; }

        /// <summary>
        /// Gets a value indicating whether all the requests in the storage are processed.
        /// </summary>
        public abstract bool IsAllRequestsProcessed { get; }

        /// <summary>
        /// Gets the total number of the requests that be persisted to the broker queue
        /// </summary>
        public abstract long AllRequestsCount { get; }

        /// <summary>
        /// Gets the number of the requests that get the corresponding responses.
        /// </summary>
        public abstract long ProcessedRequestsCount { get; }

        /// <summary>
        /// Gets the number of the requests that are dispatched but still not get corresponding responses.
        /// </summary>
        public abstract long ProcessingRequestsCount { get; }

        /// <summary>
        /// Gets the number of the requests that are flushed to the queue.
        /// </summary>
        public abstract long FlushedRequestsCount { get; }

        /// <summary>
        /// Gets the number of the requests that get the fault responses.
        /// </summary>
        public abstract long FailedRequestsCount { get; }

        /// <summary>
        /// Gets the session id of the broker queue.
        /// </summary>
        public abstract string SessionId { get; }

        /// <summary>
        /// Gets the client id of the queue.
        /// </summary>
        public abstract string ClientId { get; }

        /// <summary>
        /// Gets the persist name of the queue.
        /// </summary>
        public abstract string PersistName { get; }

        /// <summary>
        /// Gets the user name of the broker queue.
        /// </summary>
        public abstract string UserName { get; }

        /// <summary>
        /// Gets the shared data
        /// </summary>
        protected SharedData SharedData
        {
            get { return this.sharedData; }
        }

        /// <summary>
        /// Put the request item into the storage. and the storage will cache the requests in the memory 
        /// until the front end call the flush method. the async result will return the BrokerQueueItem.
        /// </summary>
        /// <param name="context">the request context relate to the message</param>
        /// <param name="msg">the request message</param>
        /// <param name="asyncState">the asyncState relate to the message</param>
        public abstract Task PutRequestAsync(RequestContextBase context, Message msg, object asyncState);

        /// <summary>
        /// Fetch the requests one by one from the storage but not remove the original message in the storage.
        /// if reach the end of the storage, empty exception raised.this is async call by BrokerQueueCallback.
        /// the async result will return the request message
        /// </summary>
        /// <param name="requestCallback">the call back to retrieve the request message</param>
        /// <param name="state">the async state object</param>
        public abstract void GetRequestAsync(BrokerQueueCallback requestCallback, object state);

        /// <summary>
        /// Put the response into the storage, and delete corresponding request from the storage.
        /// the async result will return void.byt GetResult will throw exception if the response is not persisted into the persistence.
        /// </summary>
        /// <param name="responseMsg">the response message</param>
        /// <param name="requestItem">corresponding request item</param>
        public abstract Task PutResponseAsync(Message responseMsg, BrokerQueueItem requestItem);

        /// <summary>
        /// register a response call back to get the response message.
        /// </summary>
        /// <param name="responseCallback">the response callback, the async result should be the BrokerQueueItem</param>
        /// <param name="messageVersion">the message version for the response message. if failed to pull the response from the storage, will use this version to create a fault message.</param>
        /// <param name="filter">the filter for the responses that the response callback expected.</param>
        /// <param name="responseCount">the responses count this registered response callback want to get.</param>
        /// <param name="state">the state object for the callback</param>
        /// <returns>a value indicating whether the response callback is registered sucessfully.</returns>
        public abstract bool RegisterResponsesCallback(BrokerQueueCallback responseCallback, MessageVersion messageVersion, ResponseActionFilter filter, int responseCount, object state);

        /// <summary>
        /// reset the current response callback, after this method call, the following RegisterResponseCallback will get the respoonses from the beginning.
        /// </summary>
        public abstract void ResetResponsesCallback();

        /// <summary>
        /// Acknowledge if a response has been sent back to client successfully or not
        /// </summary>
        /// <param name="response">response item</param>
        /// <param name="success">if the response item has been sent back to client successfully or not</param>
        public abstract void AckResponse(BrokerQueueItem response, bool success);

        /// <summary>
        /// Acknowledge if a list of responses have been sent back to client successfully or not
        /// </summary>
        /// <param name="responses">response item list</param>
        /// <param name="success">if the list of response items have been sent back to client successfully or not</param>
        public abstract void AckResponses(List<BrokerQueueItem> responses, bool success);

        /// <summary>
        /// flush all the requests in the cache to the persistence
        /// </summary>
        /// <param name="msgCount">the message count that will be persisted to the persistence.</param>
        /// <param name="timeoutMs">the millisecond that the flush operation can wait, otherwise, thro TimeoutException.</param>
        /// <param name="endOfMessage">a value indicating end of messages</param>
        public abstract void Flush(long msgCount, int timeoutMs, bool endOfMessage);

        /// <summary>
        /// Used only for interactive session to update the flush counters for persisted and committed 
        /// </summary>
        public abstract void FlushCount();
        
        /// <summary>
        /// Discard unflushed requests
        /// </summary>
        public abstract void DiscardUnflushedRequests();

        /// <summary>
        /// remove all the data in this queue.
        /// </summary>
        public abstract SessionPersistCounter Close();

        /// <summary>
        /// IDisposable method
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Dispose()
        {
            try
            {
                this.brokerQueueFactory.RemovePersistQueueByClient(this.ClientId);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("[BrokerQueue] .Dispose: remove the broker queue from the broker queue factory cached raised exception, {0}", e);
            }

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// send the exception through the exception event.
        /// </summary>
        /// <param name="exceptionEventArgs">the exception event args.</param>
        internal void OnException(ExceptionEventArgs exceptionEventArgs)
        {
            if (exceptionEventArgs == null)
            {
                return;
            }

            if (this.OnExceptionEvent != null)
            {
                try
                {
                    this.OnExceptionEvent(this, exceptionEventArgs);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError("[BrokerQueue]. OnException: recevied exception. exception={0}", e);
                }
            }
            else
            {
                // TODO: need log here.
            }
        }

        /// notify that a number of responses have been successfully put into queue
        /// </summary>
        /// <param name="numSuccessResponses">number of successfully put responses.</param>
        /// <param name="numFaultResponses">number of fault responses.</param>
        internal void OnPutResponsesSuccess(int numSuccessResponses, int numFaultResponses)
        {
            //bug 28856: the broker queue is responsible to update the counters when responses are received in case the broker client is already closed.
            this.SharedData.Observer.ResponsePersisted(numSuccessResponses, numFaultResponses);
            if (this.OnPutResponsesSuccessEvent != null)
            {
                PutResponsesSuccessEventArgs putResponsesSuccessEventArgs = new PutResponsesSuccessEventArgs(numSuccessResponses, numFaultResponses, this);
                this.OnPutResponsesSuccessEvent(this, putResponsesSuccessEventArgs);
            }
        }

        /// <summary>
        /// notify that a fatal exception is generated.
        /// </summary>
        /// <param name="exceptionEventArgs">the ExceptionEventArgs that contains the fatal exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void OnFatalException(Exception fatalException)
        {
            if (this.OnFatalExceptionEvent != null)
            {
                try
                {
                    ExceptionEventArgs fatalExceptionEventArgs = new ExceptionEventArgs(fatalException, this);
                    this.OnFatalExceptionEvent(this, fatalExceptionEventArgs);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[BrokerQueue] .OnFatalException: OnFatalExceptionEvent raised exception, {0}", e);
                }
            }
        }

        /// <summary>
        /// dispatch the broker queue event.
        /// </summary>
        /// <param name="eventId">the event id.</param>
        /// <param name="eventArgs">the event args.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void DispatchEvent(BrokerQueueEventId eventId, EventArgs eventArgs)
        {
            if (this.OnEvent != null)
            {
                try
                {
                    this.OnEvent(eventId, eventArgs);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[BrokerQueue] .DispatchEvent: the event handler raised exception, {0}", e);
                }
            }
        }

        /// <summary>
        /// dispose the resources.
        /// </summary>
        /// <param name="disposing">indicate whether need dispose the resource.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
