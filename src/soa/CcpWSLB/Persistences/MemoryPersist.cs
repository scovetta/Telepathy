// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Persistences
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;

    /// <summary>
    /// the implementation of in-memory storage provider
    /// </summary>
    internal class MemoryPersist : ISessionPersist
    {
        #region private fields

        /// <summary>the queue that keeps request messages </summary>
        private ConcurrentQueue<BrokerQueueItem> requestQueueField = new ConcurrentQueue<BrokerQueueItem>();

        /// <summary>token object. No significant for MemoryPersist. </summary>
        private readonly object PersistToken = new object();

        /// <summary>the queue that keeps response messages </summary>
        private Queue<BrokerQueueItem> responseQueueField = new Queue<BrokerQueueItem>();

        /// <summary>the queue that keeps response messages that are filtered out</summary>
        private Queue<BrokerQueueItem> filteredOutResponseQueueField = new Queue<BrokerQueueItem>();

        /// <summary>the lock object for responseQueueField and filteredResponseQueueField</summary>
        private object lockResponseQueueField = new object();

        /// <summary>total number of requests ever in request queue.  Equals requestQueueField.Count. </summary>
        private long allRequestCountField;

        /// <summary>number of requests in request queue. </summary>
        private long requestCountField;

        /// <summary>number of responses ever in response queue. </summary>
        private long responseCountField;

        /// <summary>number of requests that failed to be processed. </summary>
        private long failedRequestCountField;

        /// <summary>flag indicating if all requests have been received</summary>
        private bool EOMFlag;

        /// <summary>the user name </summary>
        private string userNameField;

        /// <summary>the session id</summary>
        private string sessionIdField;

        /// <summary>the client id</summary>
        private string clientIdField;

        /// <summary>a value indicating whether this instance is closed.</summary>
        private bool isClosedField;

        #endregion

        /// <summary>        
        /// Initializes a new instance of the MemoryPersist class.
        /// </summary>
        /// <param name="userName">the user name</param>
        /// <param name="sessionId">the session id.</param>
        /// <param name="clientId">the client id.</param>
        internal MemoryPersist(string userName, string sessionId, string clientId)
        {
            this.EOMFlag = false;
            this.userNameField = userName;
            this.sessionIdField = sessionId;
            this.clientIdField = clientId;
        }

        /// <summary>
        /// Gets the total number of the requests in the persistence
        /// </summary>
        public long AllRequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.allRequestCountField);
            }
        }

        /// <summary>
        /// Gets the number of requests in the persistence
        /// </summary>
        public long RequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.requestCountField);
            }
        }

        /// <summary>
        /// Gets the number of the responses ever in the persistence
        /// </summary>
        public long ResponsesCount
        {
            get
            {
                return Interlocked.Read(ref this.responseCountField);
            }
        }

        /// <summary>
        /// Gets the number of the requests that get the fault responses.
        /// </summary>
        public long FailedRequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.failedRequestCountField);
            }
        }

        /// <summary>
        /// Gets the user name of the broker queue.
        /// </summary>
        public string UserName
        {
            get
            {
                return this.userNameField;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is a new created storage.
        /// </summary>
        public bool IsNewCreated
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether EOM is received.
        /// </summary>
        public bool EOMReceived
        {
            get
            {
                return this.EOMFlag;
            }
            set
            {
                this.EOMFlag = value;
            }
        }

        /// <summary>
        /// Put the request item objects into the storage.
        /// </summary>
        /// <param name="requests">A list of request objects</param>
        /// <param name="putRequestCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        public async Task PutRequestsAsync(IEnumerable<BrokerQueueItem> requests, PutRequestCallback putRequestCallback, object callbackState)
        {
            if (this.isClosedField)
            {
                BrokerTracing.TraceWarning("[MemoryPersist] .PutRequestsAsync: the queue is closed.");
                return;
            }

            int requestsCount = 0;
            foreach (BrokerQueueItem request in requests)
            {
                request.PersistAsyncToken.AsyncToken = this.PersistToken;
                this.requestQueueField.Enqueue(request);
                requestsCount++;
            }

            Interlocked.Add(ref this.allRequestCountField, requestsCount);
            Interlocked.Add(ref this.requestCountField, requestsCount);

            PutRequestComplete(putRequestCallback, callbackState);
        }

        /// <summary>
        /// Put a single request item into the storage.
        /// </summary>
        /// <param name="request">the single request that need be stored to the persistenc</param>
        /// <param name="putRequestCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        public async Task PutRequestAsync(BrokerQueueItem request, PutRequestCallback putRequestCallback, object callbackState)
        {
            if (this.isClosedField)
            {
                BrokerTracing.TraceWarning("[MemoryPersist] .GetRequestAsync: the queue is closed.");
                return;
            }

            request.PersistAsyncToken.AsyncToken = this.PersistToken;
            this.requestQueueField.Enqueue(request);

            Interlocked.Increment(ref this.allRequestCountField);
            Interlocked.Increment(ref this.requestCountField);

            PutRequestComplete(putRequestCallback, callbackState);
        }

        /// <summary>
        /// Fetch the requests one by one from the storage but not remove the original message in the storage.
        /// </summary>
        /// <param name="callback">the callback to get the async result</param>
        /// <param name="state">the state object for the callback</param>
        public void GetRequestAsync(PersistCallback callback, object state)
        {
            if (this.isClosedField)
            {
                BrokerTracing.TraceWarning("[MemoryPersist] .GetRequestAsync: the queue is closed.");
                return;
            }

            BrokerQueueItem requestItem = null;
            if (!this.requestQueueField.TryDequeue(out requestItem))
            {
                BrokerTracing.TraceError("[MemoryPersist] .GetRequestAsync: no request available. sessionId={0}, clientId={1}", this.sessionIdField, this.clientIdField);
            }

            if (callback == null)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(this.GetOperationComplete, new GetCallbackContext(callback, state, requestItem));
        }

        /// <summary>
        /// Put the response item objects into the storage.
        /// </summary>
        /// <param name="responses">A list of response objects</param>
        /// <param name="putResponseCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        public async Task PutResponsesAsync(IEnumerable<BrokerQueueItem> responses, PutResponseCallback putResponseCallback, object callbackState)
        {
            int responseCount = 0;
            int faultResponseCount = 0;

            foreach (BrokerQueueItem response in responses)
            {
                lock (this.lockResponseQueueField)
                {
                    this.responseQueueField.Enqueue(response);
                    Interlocked.Increment(ref this.responseCountField);
                }
                response.PeerItem.Dispose();
                response.PeerItem = null;
                if (response.Message.IsFault)
                {
                    faultResponseCount++;
                }
                responseCount++;
            }

            Interlocked.Add(ref this.failedRequestCountField, faultResponseCount);
            long remainingRequestCount = Interlocked.Add(ref this.requestCountField, -(responseCount));
            bool isLastResponse = this.EOMReceived && (remainingRequestCount == 0);

            PutResponseComplete(responseCount, faultResponseCount, isLastResponse, putResponseCallback, callbackState);
        }

        /// <summary>
        /// Put on response item into persistence
        /// </summary>
        /// <param name="response">the response item to be persisted</param>
        /// <param name="putResponseCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        public async Task PutResponseAsync(BrokerQueueItem response, PutResponseCallback putResponseCallback, object callbackState)
        {
            bool isFaultResponse = response.Message.IsFault;
            lock (this.lockResponseQueueField)
            {
                this.responseQueueField.Enqueue(response);
                Interlocked.Increment(ref this.responseCountField);
            }

            response.PeerItem.Dispose();
            response.PeerItem = null;

            if (isFaultResponse)
            {
                Interlocked.Increment(ref this.failedRequestCountField);
            }
            bool isLastResponse = (Interlocked.Decrement(ref this.requestCountField) == 0);

            PutResponseComplete(1, isFaultResponse ? 1 : 0, isLastResponse, putResponseCallback, callbackState);
        }

        /// <summary>
        /// Get a response from the storage
        /// </summary>
        /// <param name="callback">the response callback, the async result should be the BrokerQueueItem</param>
        /// <param name="callbackState">the state object for the callback</param>
        public void GetResponseAsync(PersistCallback callback, object callbackState)
        {
            if (this.isClosedField)
            {
                BrokerTracing.TraceWarning("[MemoryPersist] .GetResponseAsync: the queue is closed.");
                return;
            }

            BrokerQueueItem responseItem = null;
            try
            {
                lock (this.lockResponseQueueField)
                {
                    responseItem = this.responseQueueField.Dequeue();
                }
            }
            catch (InvalidOperationException)
            {
                BrokerTracing.TraceError("[MemoryPersist] .GetResponseAsync: no response available.  sessionId = {0}, clientId = {1}", this.sessionIdField, this.clientIdField);
            }

            if (callback == null)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(this.GetOperationComplete, new GetCallbackContext(callback, callbackState, responseItem));
        }

        /// <summary>
        /// reset the current response callback, after this method call, 
        /// the registered RegisterResponseCallback will get the responses from the beginning.
        /// </summary>
        public void ResetResponsesCallback()
        {
            if (this.isClosedField)
            {
                BrokerTracing.TraceWarning("[MemoryPersist] .ResetResponsesCallback: the queue is closed.");
                return;
            }

            lock (this.lockResponseQueueField)
            {
                // merge responseQueueField and filteredOutResponseQueueFiled
                Queue<BrokerQueueItem> fromQueue = this.filteredOutResponseQueueField;
                Queue<BrokerQueueItem> toQueue = this.responseQueueField;
                if (this.responseQueueField.Count < this.filteredOutResponseQueueField.Count)
                {
                    fromQueue = this.responseQueueField;
                    toQueue = this.filteredOutResponseQueueField;
                }
                while(fromQueue.Count > 0)
                {
                    toQueue.Enqueue(fromQueue.Dequeue());
                }
                
                if (this.filteredOutResponseQueueField.Count > 0)
                {
                    Queue<BrokerQueueItem> tempQueue = this.responseQueueField;
                    this.responseQueueField = this.filteredOutResponseQueueField;
                    this.filteredOutResponseQueueField = tempQueue;
                }
            }
        }

        /// <summary>
        /// acknowledge that a response message is dispatched, either successfully or not
        /// </summary>
        /// <param name="responseItem">response message being dispatched</param>
        /// <param name="success">if dispatching is success or not</param>
        public void AckResponse(BrokerQueueItem responseMessage, bool success)
        {
            if (success)
            {
                responseMessage.Dispose();
                return;
            }

            // if failed to send response, response message is put back 
            responseMessage.PrepareMessage();

            // put response back                
            lock (this.lockResponseQueueField)
            {
                this.filteredOutResponseQueueField.Enqueue(responseMessage);
            }
        }

        /// <summary>
        /// IDisposable method
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// remove the storage.
        /// </summary>
        public SessionPersistCounter Close()
        {
            BrokerTracing.TraceVerbose("[MemoryPersist] .Close: Close the MemoryPersist.");
            if (!this.isClosedField)
            {
                this.isClosedField = true;
            }

            SessionPersistCounter counter = new SessionPersistCounter();
            counter.ResponsesCountField = Interlocked.Read(ref this.responseCountField);
            counter.FailedRequestsCountField = Interlocked.Read(ref this.failedRequestCountField);
            return counter;
        }

        public bool IsInMemory()
        {
            return true;
        }

        public void CommitRequest()
        {
        }

        public void AbortRequest()
        {
        }

        /// <summary>
        /// get request/resposne operation is completed
        /// </summary>
        /// <param name="state">the operation context</param>
        private void GetOperationComplete(object state)
        {
            if (this.isClosedField)
            {
                BrokerTracing.TraceWarning("[MemoryPersist] .GetOperationComplete: the queue is closed.");
                return;
            }

            GetCallbackContext callbackContext = (GetCallbackContext)state;

            try
            {
                callbackContext.Callback(callbackContext.CallbackMessage, callbackContext.CallbackState, null);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[MemoryPersist] .GetOperationComplete: callback failed, Exception:{0}.", e.ToString());
            }
        }

        /// <summary>
        /// the putting request complete thread proc
        /// </summary>
        /// <param name="state">the thread pool callback state</param>
        private static void PutRequestComplete(PutRequestCallback callback, object state)
        {
            if (callback == null)
            {
                return;
            }

            try
            {
                callback(null, state);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[MemoryPersist] .PutRequestComplete: callback failed, Exception:{0}.", e.ToString());
            }
        }

        /// <summary>
        /// the putting response complete thread proc
        /// </summary>
        /// <param name="state">the thread pool callback state</param>
        private static void PutResponseComplete(int responseCount, int faultResponseCount, bool isLastResponse, PutResponseCallback putResponseCallback, object callbackState)
        {
            if (putResponseCallback == null)
            {
                return;
            }

            try
            {
                putResponseCallback(null, responseCount, faultResponseCount, isLastResponse, null, callbackState);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[MemoryPersist] .PutResponseComplete: callback failed, Exception:{0}.", e.ToString());
            }
        }

        /// <summary>
        /// context for getting request/response operations
        /// </summary>
        class GetCallbackContext
        {
            #region private fields
            /// <summary>
            /// the callback field
            /// </summary>
            private PersistCallback callbackField;

            /// <summary>
            /// the callback state field
            /// </summary>
            private object callbackStateField;

            /// <summary>
            /// the message retrieved from this storage
            /// </summary>
            private BrokerQueueItem callbackMsgField;

            #endregion

            /// <summary>
            /// Instantiate a new instance of class GetCallbackContext
            /// </summary>
            /// <param name="callback">the callback</param>
            /// <param name="callbackState">the callback context</param>
            /// <param name="callbackMsg">the retrieved back message</param>
            public GetCallbackContext(PersistCallback callback, object callbackState, BrokerQueueItem callbackMsg)
            {
                this.callbackField = callback;
                this.callbackStateField = callbackState;
                this.callbackMsgField = callbackMsg;
            }

            /// <summary>
            /// Gets the callback
            /// </summary>
            public PersistCallback Callback
            {
                get
                {
                    return this.callbackField;
                }
            }

            /// <summary>
            /// Gets the callback state
            /// </summary>
            public object CallbackState
            {
                get
                {
                    return this.callbackStateField;
                }
            }

            /// <summary>
            /// Gets the retrieved message
            /// </summary>
            public BrokerQueueItem CallbackMessage
            {
                get
                {
                    return this.callbackMsgField;
                }
            }
        }
    }
}
