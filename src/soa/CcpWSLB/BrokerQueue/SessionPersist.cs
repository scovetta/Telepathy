// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// the delegate for the get request callback.
    /// </summary>
    /// <param name="persistMessage">the message from the queue.</param>
    /// <param name="state">the state object for the callback.</param>
    /// <param name="exception">the exception that the async operation raised.</param>
    public delegate void PersistCallback(BrokerQueueItem persistMessage, object state, Exception exception);


    /// <summary>
    /// the delegate callback for putting request
    /// </summary>
    /// <param name="exception">the raised exception on putting request failure</param>
    /// <param name="state">the state object for the callback</param>
    public delegate void PutRequestCallback(Exception exception, object state);

    /// <summary>
    /// the delegate callback for putting response
    /// </summary>
    /// <param name="exception">the raised exception on putting response failure</param>
    /// <param name="responseCount">number of responses that are successfully put into persistence</param>
    /// <param name="faultResponseCount">number of fault responses</param>
    /// <param name="isLastResponse">a flag indicating if the last response for available requests is persisted</param>
    /// <param name="failedItems">response items failed to be put</param>
    /// <param name="state">the state object for the callback </param>
    public delegate void PutResponseCallback(Exception exception, int responseCount, int faultResponseCount, bool isLastResponse, List<BrokerQueueItem> failedItems, object state);

    /// <summary>
    /// The main interface to communicate between broker and the storage system
    /// </summary>
    public interface ISessionPersist : IDisposable
    {
        /// <summary>
        /// Gets the total number of the requests in the persistence
        /// </summary>
        long AllRequestsCount { get; } 

        /// <summary>
        /// Gets the number of requests in the persistence
        /// </summary>
        long RequestsCount { get; }

        /// <summary>
        /// Gets the number of the current responses in the persistence
        /// </summary>
        long ResponsesCount { get; }

        /// <summary>
        /// Gets the number of the requests that get the fault responses.
        /// </summary>
        long FailedRequestsCount { get; }

        /// <summary>
        /// Gets the user name of the broker queue.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Gets a value indicating whether this is a new created storage.
        /// </summary>
        bool IsNewCreated { get; }

        /// <summary>
        /// Gets a value indicating whether EOM is received.
        /// </summary>
        bool EOMReceived { get; set; }

        /// <summary>
        /// Put the request item objects into the storage.
        /// </summary>
        /// <param name="requests">A list of request objects</param>
        /// <param name="putRequestCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for callback.</param>
        /// <remarks>This operation should aware the TransactionContext. 
        /// If the context is avariable, nothing should be changed if one of 
        /// operations failed.</remarks>
        Task PutRequestsAsync(IEnumerable<BrokerQueueItem> requests, PutRequestCallback putRequestCallback, object callbackState);

        /// <summary>
        /// Put a single request item into the storage.
        /// </summary>
        /// <param name="request">the single request that need be stored to the persistenc</param>
        /// <param name="putRequestCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for callback.</param>
        /// <remarks>This operation should aware the TransactionContext. 
        /// If the context is avariable, nothing should be changed if one of 
        /// operations failed.</remarks>
        Task PutRequestAsync(BrokerQueueItem request, PutRequestCallback putRequestCallback, object callbackState);

        /// <summary>
        /// Fetch the requests one by one from the storage but not remove the original message in the storage.
        /// if reach the end of the storage, empty exception raised.
        /// </summary>
        /// <param name="callback">the callback to get the async result</param>
        /// <param name="state">the state object for the callback</param>
        void GetRequestAsync(PersistCallback callback, object state);       

        /// <summary>
        /// Put the response item objects into the storage.
        /// </summary>
        /// <param name="responeses">A list of response objects</param>
        /// <param name="putResponseCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        Task PutResponsesAsync(IEnumerable<BrokerQueueItem> responses, PutResponseCallback putResponseCallback, object callbackState);

        /// <summary>
        /// Put on response item into persistence
        /// </summary>
        /// <param name="response">the response item to be persisted</param>
        /// <param name="putResponseCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        Task PutResponseAsync(BrokerQueueItem response, PutResponseCallback putResponseCallback, object callbackState);

        /// <summary>
        /// get a response message from the storage.
        /// </summary>
        /// <param name="callback">the response callback, the async result should be the BrokerQueueItem</param>
        /// <param name="callbackState">the state object for the callback</param>
        /// <returns>a value indicating whether alll the responses are dispatched.</returns>
        void GetResponseAsync(PersistCallback callback, object callbackState);
        
        /// <summary>
        /// reset the current response callback, after this method call, 
        /// the registered RegisterResponseCallback will get the responses from the beginning.
        /// </summary>
        void ResetResponsesCallback();

        /// <summary>
        /// acknowledge that a response message is dispatched, either successfully or not
        /// </summary>
        /// <param name="responseItem">response message being dispatched</param>
        /// <param name="success">if dispatching is success or not</param>
        void AckResponse(BrokerQueueItem responseItem, bool success);

        /// <summary>
        /// remove the storage.
        /// </summary>
        SessionPersistCounter Close();

        bool IsInMemory();

        void CommitRequest();

        void AbortRequest();
    }


    /// <summary>
    /// This counter is returned when close the queue.
    /// </summary>
    public class SessionPersistCounter
    {
        /// <summary>
        /// the total responses count in the queue
        /// </summary>
        private long responsesCountField;

        public long ResponsesCountField
        {
            get { return this.responsesCountField; }
            set { this.responsesCountField = value; }
        }


        /// <summary>
        /// the number of the requests that get the fault responses
        /// </summary>
        private long failedRequestsCountField;

        public long FailedRequestsCountField
        {
            get { return this.failedRequestsCountField; }
            set { this.failedRequestsCountField = value; }
        }


        /// <summary>
        /// the number of the requests that are flushed to the queue
        /// </summary>
        private long flushedRequestsCount;

        public long FlushedRequestsCount
        {
            get { return this.flushedRequestsCount; }
            set { this.flushedRequestsCount = value; }
        }
    }
}
