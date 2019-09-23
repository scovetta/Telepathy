// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Common.ServiceJobMonitor;
    using Microsoft.Telepathy.ServiceBroker.FrontEnd;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Represent a broker client
    /// </summary>
    internal class BrokerClient : IDisposable
    {
        /// <summary>
        /// Stores the state
        /// </summary>
        private BrokerClientState state = BrokerClientState.NotStarted;

        /// <summary>
        /// Stores the client id
        /// </summary>
        private string clientId;

        /// <summary>
        /// Stores the client instance id
        /// </summary>
        private int currentBatchId = -1;

        private AutoResetEvent batchIdChangedEvent = new AutoResetEvent(true);

        /// <summary>
        /// Lock object for discarding unflushed requests.  It prevents new-coming requests from being discarded
        /// </summary>
        private ReaderWriterLockSlim lockForDiscardRequests = new ReaderWriterLockSlim();

        /// <summary>
        /// Stores the broker queue
        /// </summary>
        private BrokerQueue queue;

        /// <summary>
        /// Stores the timeout manager
        /// </summary>
        private TimeoutManager timeoutManager;

        /// <summary>
        /// lock for the state transition
        /// </summary>
        private object lockState = new object();

        /// <summary>
        /// Stores the responses client
        /// </summary>
        private BaseResponsesHandler responsesClient;

        /// <summary>
        /// Stores the connected instance
        /// </summary>
        private Dictionary<object, object> connectedInstance = new Dictionary<object, object>();

        /// <summary>
        /// Stores a flag indicating whether endOfMessage has called.
        /// </summary>
        private bool endOfMessageCalled;

        /// <summary>
        /// Stores shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// Stores the broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Stores the ServiceJob monitor
        /// </summary>
        private ServiceJobMonitorBase monitor;

        /// <summary>
        /// Stores a value indicating whether a singleton instance has connected or not
        /// </summary>
        private bool singletonInstanceConnected;

        /// <summary>
        /// Stores the broker state manager
        /// </summary>
        private BrokerStateManager stateManager;

        /// <summary>
        /// Stores the number of directly replied responses
        /// </summary>
        private long directlyReplied;

        /// <summary>
        /// This dictionary retains the message Ids in a same batch to help check if the coming request is a duplicated message
        /// </summary>
        private ConcurrentDictionary<Guid, int> batchMessageIds = new ConcurrentDictionary<Guid, int>();

        /// <summary>
        /// Indicate if the brokerclient is disposing
        /// </summary>
        private bool disposing = false;

        /// <summary>
        /// The v2 proxy client with the default client id prefix
        /// </summary>
        private bool v2ProxyClient = false;

        /// <summary>
        /// Initializes a new instance of the BrokerClient class
        /// </summary>
        /// <param name="clientId">indicating the client Id</param>
        /// <param name="userName">indicating the user name</param>
        /// <param name="queueFactory">indicating the queue factory</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="stateManager">indicating the state manager</param>
        /// <param name="monitor">indicating the monitor</param>
        /// <param name="sharedData">indicating the shared data</param>
        public BrokerClient(string clientId, string userName, BrokerQueueFactory queueFactory, BrokerObserver observer, BrokerStateManager stateManager, ServiceJobMonitorBase monitor, SharedData sharedData)
        {
            bool isNewCreated;

            // Set the "signletonInstanceConnected" property if
            // SessionStartInfo.AutoDisposeBrokerClient is set. This property is only possibly to
            // be set to true after if HTTP transport scheme is specified. And it is by design so.
            if (sharedData.StartInfo.AutoDisposeBrokerClient.HasValue)
            {
                this.singletonInstanceConnected = !sharedData.StartInfo.AutoDisposeBrokerClient.Value;
            }

            this.clientId = clientId;
            this.v2ProxyClient = clientId.StartsWith(FrontEndBase.DefaultClientPrefix);
            this.sharedData = sharedData;
            this.observer = observer;
            this.monitor = monitor;
            this.stateManager = stateManager;
            this.stateManager.OnFailed += new BrokerStateManager.SessionFailedEventHandler(this.StateManager_OnFailed);
            
            try
            {
                this.queue = queueFactory.GetPersistQueueByClient(clientId, userName, out isNewCreated);
            }
            catch (BrokerQueueException e)
            {
                // Catch the exception about the username not match and translte it to session fault
                if (e.ErrorCode == (int)BrokerQueueErrorCode.E_BQ_USER_NOT_MATCH)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.AccessDenied_BrokerQueue, SR.AccessDenied_BrokerQueue, clientId, userName);
                }
                else
                {
                    throw;
                }
            }

            this.queue.OnEvent += new BrokerQueueEventDelegate(this.Queue_OnEvent);
            this.queue.OnPutResponsesSuccessEvent += new EventHandler<PutResponsesSuccessEventArgs>(this.Queue_OnPutResponsesSuccessEvent);
            this.queue.OnFatalExceptionEvent += new EventHandler<ExceptionEventArgs>(this.Queue_OnFatalExceptionEvent);
            this.timeoutManager = new TimeoutManager("BrokerClient " + clientId);
            BrokerTracing.EtwTrace.LogBrokerClientCreated(this.sharedData.BrokerInfo.SessionId, clientId);

            if (this.queue.IsAllRequestsProcessed || monitor.ServiceJobState == ServiceJobState.Finished)
            {
                // If the queue has processed all the request or the service job is finished, the broker client can only get responses
                this.state = BrokerClientState.GetResponse;
                this.endOfMessageCalled = true;
                BrokerTracing.EtwTrace.LogBrokerClientStateTransition(this.sharedData.BrokerInfo.SessionId, this.clientId, "GetResponse");
                this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientIdleTimeout, this.TimeoutToDisconnected, this.state);
            }
            else
            {
                if (!this.queue.EOMReceived)
                {
                    // If EndOfMessage is not received, the client is in the ClientConnected state and is ready to accept messages
                    this.state = BrokerClientState.ClientConnected;
                    BrokerTracing.EtwTrace.LogBrokerClientStateTransition(this.sharedData.BrokerInfo.SessionId, this.clientId, "ClientConnected");
                    this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientIdleTimeout, this.TimeoutToDisconnected, this.state);
                }
                else
                {
                    // If EndOfMessage has been received, the client is in the EndOfMessage state and does not accept any more requests.
                    this.state = BrokerClientState.EndRequests;
                    this.endOfMessageCalled = true;
                    BrokerTracing.EtwTrace.LogBrokerClientStateTransition(this.sharedData.BrokerInfo.SessionId, this.clientId, "EndOfMessage");
                }
            }
        }

        /// <summary>
        /// Finalizes an instance of the BrokerClient class
        /// </summary>
        ~BrokerClient()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event triggered when the client is disconnected
        /// </summary>
        public event EventHandler ClientDisconnected;

        /// <summary>
        /// Event triggered when the client is in "AllRequestDone" state
        /// </summary>
        public event EventHandler AllRequestDone;

        /// <summary>
        /// Gets the client id
        /// </summary>
        public string ClientId
        {
            get { return this.clientId; }
        }

        /// <summary>
        /// Gets the client state
        /// </summary>
        public BrokerClientState State
        {
            get { return this.state; }
        }

        /// <summary>
        /// Gets number of committed requests in this client
        /// </summary>
        public int RequestsCount
        {
            get { return (int)this.queue.FlushedRequestsCount; }
        }

        /// <summary>
        /// Finalizes the broker client
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Delete the correspoding queue
        /// </summary>
        public SessionPersistCounter DeleteQueue()
        {
            SessionPersistCounter counter = null;
            if (this.queue != null)
            {
                this.queue.OnEvent -= this.Queue_OnEvent;
                this.queue.OnPutResponsesSuccessEvent -= this.Queue_OnPutResponsesSuccessEvent;
                this.queue.OnFatalExceptionEvent -= this.Queue_OnFatalExceptionEvent;

                counter = this.queue.Close();
                this.queue = null;
            }

            // Send client purged when delete queue
            if (this.responsesClient != null)
            {
                this.responsesClient.ClientDisconnect(true);
            }

            return counter;
        }

        /// <summary>
        /// Informs the client that a singleton instance has connected to this client
        /// </summary>
        public void SingletonInstanceConnected()
        {
            this.singletonInstanceConnected = true;
        }

        /// <summary>
        /// Flush arrives
        /// </summary>
        /// <param name="msgCount">indicating the msg count</param>
        /// <param name="timeout">indicating the timeout</param>
        public void Flush(long msgCount, int batchId, int timeout, bool endOfMessage = false)
        {
            lock (this.lockState)
            {
                while (true)
                {
                    // Read lock this to wait 
                    this.lockForDiscardRequests.EnterReadLock();
                    bool needWait = false;

                    try
                    {
                        if (this.state != BrokerClientState.ClientConnected)
                        {
                            BrokerTracing.EtwTrace.LogBrokerClientRejectFlush(this.sharedData.BrokerInfo.SessionId, this.clientId, this.state.ToString());
                            ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_FlushRejected, SR.FlushRejected, BrokerClient.MapToBrokerClientStatus(this.state).ToString());
                        }

                        // Reset the timer
                        this.timeoutManager.ResetTimeout();

                        if (!this.sharedData.BrokerInfo.Durable)
                        {
                            // For non-durable session, just flush the requests.
                            this.queue.Flush(msgCount, timeout, endOfMessage);
                            break;
                        }

                        if (batchId > this.currentBatchId)
                        {
                            needWait = true;
                        }
                        else if (batchId < this.currentBatchId)
                        {
                            BrokerTracing.TraceWarning(
                                "[BrokerClient] .Flush: clientId={0}, drop the current flush. batchId {1}, currentBatchId {2}.",
                                this.clientId,
                                batchId,
                                this.currentBatchId);

                            break;
                        }
                    }
                    finally
                    {
                        this.lockForDiscardRequests.ExitReadLock();
                    }

                    if (needWait)
                    {
                        BrokerTracing.TraceVerbose("[BrokerClient] .Flush: clientId={0}, batchId {1} currentBatchId {2} wait for new batch.", this.clientId, batchId, this.currentBatchId);
                        if (!this.batchIdChangedEvent.WaitOne(timeout, false))
                        {
                            BrokerTracing.TraceWarning(
                                "[BrokerClient] .Flush: clientId={0}, wait for new batch timeout within {1} milliseconds.",
                                this.clientId,
                                timeout);

                            throw new TimeoutException(
                                string.Format(
                                CultureInfo.InvariantCulture,
                                "[BrokerClient] .Flush timeout for waiting for new batch within {0} milliseconds for client {1}.",
                                    timeout,
                                    this.clientId));
                        }

                        BrokerTracing.TraceVerbose("[BrokerClient] .Flush: clientId={0}, has new batch come in.", this.clientId);
                        continue;
                    }
                    else
                    {
                        this.queue.Flush(msgCount, timeout, endOfMessage);
                        if (msgCount > 0)
                        {
                            // Stop timeout for durable session if requests are flushed
                            this.timeoutManager.Stop();
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// EndOfMessage arrives
        /// </summary>
        /// <param name="msgCount">indicating the msg count</param>
        /// <param name="timeout">indicating the timeout</param>
        public void EndOfMessage(long msgCount, int batchId, int timeout)
        {
            lock (this.lockState)
            {
                if (this.state != BrokerClientState.ClientConnected)
                {
                    BrokerTracing.EtwTrace.LogBrokerClientRejectEOM(this.sharedData.BrokerInfo.SessionId, this.clientId, this.state.ToString());
                    switch (this.state)
                    {
                        case BrokerClientState.GetResponse:
                            ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_EOMReject_GetResponse, SR.EOMReject_GetResponse);
                            break;
                        case BrokerClientState.EndRequests:
                            ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_EOMReject_EndRequests, SR.EOMReject_EndRequests);
                            break;
                        default:
                            ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_EOMRejected, SR.EOMRejected, this.state.ToString());
                            break;
                    }
                }

                // Stop the timer
                this.timeoutManager.Stop();

                try
                {
                    this.Flush(msgCount, batchId, timeout, true);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError("[BrokerClient] Failed to flush messages: {0}", e);

                    // If EndRequests throws exception from broker queue, the
                    // timer is closed and never reactivated without user action.
                    // Thus, the broker client would leak forever and the broker
                    // process would run away if user does not purge the client
                    // or close the session later.
                    this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientIdleTimeout, this.TimeoutToDisconnected, this.state);
                    throw;
                }

                // State will change if flush succeeded
                // If flush failed, the state won't change
                BrokerTracing.TraceInfo("[BrokerClient] Client {0}: State: ClientConnected ==> EndOfMessage", this.clientId);
                this.state = BrokerClientState.EndRequests;
                this.endOfMessageCalled = true;
            }
        }

        /// <summary>
        /// Request received for this client
        /// </summary>
        /// <param name="requestContext">indicating the request context</param>
        /// <param name="requestMessage">indicating the request message</param>
        /// <param name="state">indicating the state</param>
        /// <remarks>this method is thread safe</remarks>
        public void RequestReceived(RequestContextBase requestContext, Message requestMessage, object state)
        {
            // in case the broker client itself is disposed
            try
            {
                // Only allow request come in in client connected state
                if (this.state == BrokerClientState.ClientConnected)
                {
                    BrokerTracing.EtwTrace.LogFrontEndRequestReceived(this.sharedData.BrokerInfo.SessionId, this.clientId, Utility.GetMessageIdFromMessage(requestMessage));

                    // Make sure requests are from the same client instance.  If not, discard messages from previous client instance
                    int batchId = GetBatchId(requestMessage);

                    BrokerTracing.TraceInfo("[BrokerClient] RequestReceived Client {0}: Batch {1}: Message {2}.", this.clientId, batchId, Utility.GetMessageIdFromMessage(requestMessage));

                    if (batchId > this.currentBatchId)
                    {
                        this.lockForDiscardRequests.EnterWriteLock();
                        try
                        {
                            if (batchId > this.currentBatchId)
                            {
                                BrokerTracing.TraceWarning("[BrokerClient] New client instance id encountered: {0}. Will discard unflushed requests", batchId);
                                this.observer.ReduceUncommittedCounter(this.queue.AllRequestsCount - this.queue.FlushedRequestsCount);
                                this.queue.DiscardUnflushedRequests();
                                this.currentBatchId = batchId;
                                this.batchIdChangedEvent.Set();
                                this.batchMessageIds.Clear();
                            }
                        }
                        finally
                        {
                            this.lockForDiscardRequests.ExitWriteLock();
                        }
                    }

                    if (batchId == this.currentBatchId)
                    {
                        this.lockForDiscardRequests.EnterReadLock();
                        try
                        {
                            if (batchId == this.currentBatchId)
                            {
                                Guid messageId = Utility.GetMessageIdFromMessage(requestMessage);
                                if (this.batchMessageIds.TryAdd(messageId, batchId))
                                {
                                    this.queue.PutRequestAsync(requestContext, requestMessage, state);
                                    this.observer.IncomingRequest();
                                }
                                else
                                {
                                    BrokerTracing.TraceWarning("[BrokerClient] Client {0}: discarded one duplicate request with instanceId: {1}, messageId {2}.", this.clientId, batchId, messageId);
                                }
                            }
                            else
                            {
                                Debug.Assert(
                                    batchId < this.currentBatchId,
                                    string.Format("Unexpected clientInstanceId {0} and currentClientInstanceId {1}", batchId, this.currentBatchId));

                                Guid messageId = Utility.GetMessageIdFromMessage(requestMessage);
                                BrokerTracing.TraceWarning("[BrokerClient] Client {0}: discarded one request with instanceId: {1}, messageId {2}.", this.clientId, batchId, messageId);
                            }
                        }
                        finally
                        {
                            this.lockForDiscardRequests.ExitReadLock();
                        }
                    }
                    else
                    {
                        Debug.Assert(
                            batchId < this.currentBatchId,
                            string.Format("Unexpected clientInstanceId {0} and currentClientInstanceId {1}", batchId, this.currentBatchId));

                        Guid messageId = Utility.GetMessageIdFromMessage(requestMessage);
                        BrokerTracing.TraceWarning("[BrokerClient] Client {0}: discarded one request with instanceId: {1}, messageId {2}.", this.clientId, batchId, messageId);
                    }

                    if (this.sharedData.BrokerInfo.Durable)
                    {
                        // Reset timeout for durable session, stop it when Flush succeeds which means there's outstanding reqeusts.
                        this.timeoutManager.ResetTimeout();
                    }
                    else
                    {
                        // Bug 4842: Stop timeout when there's outstanding requests for interactive session
                        this.timeoutManager.Stop();
                    }
                }
                else
                {
                    BrokerTracing.EtwTrace.LogFrontEndRequestRejectedClientStateInvalid(this.sharedData.BrokerInfo.SessionId, this.clientId, Utility.GetMessageIdFromMessage(requestMessage));
                }
            }
            catch (NullReferenceException en)
            {
                BrokerTracing.TraceWarning("[BrokerClient] NullReferenceException is thrown for the broker client may be disposing: {0}", en);
            }
            catch (ObjectDisposedException eo)
            {
                BrokerTracing.TraceWarning("[BrokerClient] ObjectDisposedException is thrown for the broker client may be disposing: {0}", eo);
            }

        }

        /// <summary>
        /// Get responses
        /// </summary>
        /// <param name="action">indicating the action</param>
        /// <param name="clientData">indicating the client data</param>
        /// <param name="resetToBegin">indicating the position</param>
        /// <param name="count">indicating the count</param>
        /// <param name="callbackInstance">indicating the callback instance</param>
        /// <param name="version">indicating the message version</param>
        public void GetResponses(string action, string clientData, GetResponsePosition resetToBegin, int count, IResponseServiceCallback callbackInstance, MessageVersion version)
        {
            this.CheckDisconnected();

            this.timeoutManager.ResetTimeout();

            if (this.responsesClient is GetResponsesHandler)
            {
                GetResponsesHandler handler = this.responsesClient as GetResponsesHandler;
                if (handler.Matches(action, clientData))
                {
                    handler.GetResponses(resetToBegin, count, callbackInstance);
                    return;
                }
            }

            if (this.responsesClient != null)
            {
                this.responsesClient.Dispose();
            }

            this.responsesClient = new GetResponsesHandler(this.queue, action, clientData, this.clientId, this.timeoutManager, this.observer, this.sharedData, version);
            (this.responsesClient as GetResponsesHandler).GetResponses(resetToBegin, count, callbackInstance);
        }

        /// <summary>
        /// Pull responses for Java clients
        /// </summary>
        /// <param name="action">indicating the action</param>
        /// <param name="position">indicating the position</param>
        /// <param name="count">indicating the count</param>
        /// <returns>returns the responses messages</returns>
        /// <param name="version">indicating the message version</param>
        public BrokerResponseMessages PullResponses(string action, GetResponsePosition position, int count, MessageVersion version)
        {
            this.CheckDisconnected();

            this.timeoutManager.ResetTimeout();

            if (this.responsesClient is PullResponsesHandler)
            {
                PullResponsesHandler handler = this.responsesClient as PullResponsesHandler;
                if (handler.Match(action))
                {
                    return handler.PullResponses(position, count);
                }
            }

            if (this.responsesClient != null)
            {
                this.responsesClient.Dispose();
            }

            this.responsesClient = new PullResponsesHandler(this.queue, action, this.timeoutManager, this.observer, this.sharedData, version);
            return (this.responsesClient as PullResponsesHandler).PullResponses(position, count);
        }

        /// <summary>
        /// Informs that a frontend has connected
        /// </summary>
        /// <param name="instance">indicating the frontend instance</param>
        public void FrontendConnected(object instance, object frontend)
        {
            BrokerTracing.TraceInfo("[BrokerClient] Client {0}: Frontend connected: {1} ({2}).", this.clientId, instance.ToString(), instance.GetHashCode());
            lock (this.connectedInstance)
            {
                this.connectedInstance.Add(instance, frontend);
            }
        }

        /// <summary>
        /// Informs that the frontend has been disconnected
        /// </summary>
        /// <param name="instance">indicating the frontend instance</param>
        public void FrontendDisconnected(object instance)
        {
            BrokerTracing.TraceInfo("[BrokerClient] Client {0}: Frontend disconnected: {1} ({2}).", this.clientId, instance.ToString(), instance.GetHashCode());

            bool flag;
            lock (this.connectedInstance)
            {
                flag = this.connectedInstance.Remove(instance);
                flag = flag && this.connectedInstance.Count == 0 && !this.disposing;
            }

            // If singleton instance is already connected, we don't dispose the broker client.
            // This logic originally didn't exist because HTTP frontend cannot detect disconnection.
            // We add this logic here for REST service as it requires broker client to not dispose
            // itself if no connection is attached. And REST service is using net.tcp connection
            // which can detect disconnection.
            if (flag && !this.singletonInstanceConnected)
            {
                BrokerTracing.EtwTrace.LogBrokerClientDisconnected(this.sharedData.BrokerInfo.SessionId, this.clientId, "frontend disonnected");
                BrokerTracing.TraceInfo("[BrokerClient] Client {0}: Client SyncDisconnect when frontend disconnected: {1} ({2}).", this.clientId, instance.ToString(), instance.GetHashCode());
                this.SyncDisconnect();
            }
        }

        /// <summary>
        /// Check access for the user
        /// </summary>
        /// <param name="userName">indicating the user name</param>
        public void CheckAccess(string userName)
        {
            if (!String.Equals(userName, this.queue.UserName, StringComparison.OrdinalIgnoreCase) && this.queue.UserName != Constant.AnonymousUserName)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.AccessDenied_BrokerQueue, SR.AccessDenied_BrokerQueue, userName, this.clientId);
            }
        }


        /// <summary>
        /// Returns the duplex front end for the BrokerClient
        /// </summary>
        /// <returns></returns>
        public FrontEndBase GetDuplexFrontEnd()
        {
            FrontEndBase duplexFrontEnd = null;

            lock (this.connectedInstance)
            {
                foreach (object connectedInstance in this.connectedInstance.Values)
                {
                    if (connectedInstance is DuplexFrontEnd)
                    {
                        duplexFrontEnd = (DuplexFrontEnd)connectedInstance;
                        break;
                    }
                }
            }

            return duplexFrontEnd;
        }

        /// <summary>
        /// Check if the client is disconnected and throw exception if so
        /// </summary>
        private void CheckDisconnected()
        {
            if (this.state == BrokerClientState.Disconnected)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.ClientTimeout, SR.ClientTimeout);
            }
        }

        /// <summary>
        /// Dispose the broker client
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        private void Dispose(bool disposing)
        {
            this.disposing = disposing;
            try
            {
                // Set state
                this.state = BrokerClientState.Disconnected;

                // Copy all the connected instances out of lock
                List<object> connectedList = null;
                lock (this.connectedInstance)
                {
                    connectedList = new List<object>(this.connectedInstance.Keys);
                }

                foreach (object instance in connectedList)
                {
                    try
                    {
                        BrokerTracing.TraceInfo("[BrokerClient] Try to close the connected instance: {0} ({1})", instance.ToString(), instance.GetHashCode());
                        if (instance is IDisposable)
                        {
                            // BrokerController
                            ((IDisposable)instance).Dispose();
                        }
                        else if (instance is IChannel)
                        {
                            Utility.AsyncCloseICommunicationObject((ICommunicationObject)instance);
                        }
                        else
                        {
                            Debug.Fail("[BrokerClient] Connected instance must be IDisposable or IChannel.");
                        }
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceWarning("[BrokerClient] Failed to close the connected instance: {0}", e);
                    }
                }

                if (disposing)
                {
                    if (this.queue != null)
                    {
                        this.queue.OnEvent -= this.Queue_OnEvent;
                        this.queue.OnPutResponsesSuccessEvent -= this.Queue_OnPutResponsesSuccessEvent;
                        this.queue.OnFatalExceptionEvent -= this.Queue_OnFatalExceptionEvent;

                        //for durable session, the queue is closed unless there were flushed requests, dispose if EOM was called.
                        if (this.sharedData.BrokerInfo.Durable)
                        {
                            if (this.queue.FlushedRequestsCount == 0)
                            {
                                BrokerTracing.TraceInfo("[BrokerClient] durable session broker client {0} close the queue.", this.clientId);
                                //if not ever flushed, reduce the count of all requests in the queue
                                this.observer.ReduceUncommittedCounter(this.queue.AllRequestsCount);
                                this.queue.Close();
                            }
                            else if (this.endOfMessageCalled)
                            {
                                // Only dispose the broker queue if it is a durable session. Non-durable broker queue for 
                                // interactive session should be kept/reused and closed when closing the broker domain.
                                //
                                // Note (related bug #14224): logic in BrokerClient.SyncDisconnect() ensures that BrokerClient 
                                // instance will not be disposed if EndRequests is called (this.state = BrokerClientState.EndRequests).
                                // So below code snippet could only be reached on broker entry exiting.
                                this.observer.ReduceUncommittedCounter(this.queue.AllRequestsCount - this.queue.FlushedRequestsCount);
                                this.queue.Dispose();
                            }

                        }
                        else //for interactive session, close the queue if EOM is not called.
                        {
                            if (!this.endOfMessageCalled)
                            {
                                BrokerTracing.TraceInfo("[BrokerClient] interactive session broker client {0} close the queue.", this.clientId);
                                if (!this.v2ProxyClient)
                                {
                                    //reduce the count of all unflushed requests in the queue
                                    this.observer.ReduceUncommittedCounter(this.queue.AllRequestsCount - this.queue.FlushedRequestsCount);
                                }
                                this.queue.Close();
                            }
                        }

                        this.queue = null;

                        this.batchMessageIds.Clear();
                    }
                }

                if (this.responsesClient != null)
                {
                    this.responsesClient.Dispose();
                    this.responsesClient = null;
                }

                if (this.timeoutManager != null)
                {
                    this.timeoutManager.Dispose();
                    this.timeoutManager = null;
                }

                // do not dispose this.lockForDiscardRequests for this may block the threads in the write queue, which may lead to the hang when broker entry exits.
                // do not dispose this.batchIdChangedEvent for any wait for the event would hang
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("[BrokerClient] Exception thrown while disposing: {0}", e);
            }
        }

        /// <summary>
        /// the event when the session failed.
        /// </summary>
        private void StateManager_OnFailed()
        {
            if (this.responsesClient != null)
            {
                this.responsesClient.SessionFailed();
            }
        }

        /// <summary>
        /// Check for pending requests and register client idle timeout if no pending requests
        /// </summary>
        public void RegisterTimeoutIfNoPendingRequests()
        {
            Debug.Assert(!this.sharedData.BrokerInfo.Durable, "[BrokerClient] RegisterTimeoutIfNoPendingRequests should only be called in interactive session.");
            Interlocked.Increment(ref this.directlyReplied);
            lock (this.lockState)
            {
                if (this.state == BrokerClientState.ClientConnected)
                {
                    if (this.queue.AllRequestsCount == this.directlyReplied)
                    {
                        BrokerTracing.TraceInfo("[BrokerClient] Client {0}: Restart counting ClientIdleTimeout because all requests are processed.", this.clientId);
                        this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientIdleTimeout, this.TimeoutToDisconnected, this.state);
                    }
                }
            }
        }

        /// <summary>
        /// Triggers for queue events
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event args</param>
        private void Queue_OnPutResponsesSuccessEvent(object sender, PutResponsesSuccessEventArgs e)
        {
            // For both durable session, enable ClientIdleTimeout when all flushed requests are processed
            if (this.sharedData.BrokerInfo.Durable)
            {
                if (this.queue.FlushedRequestsCount == this.queue.ProcessedRequestsCount)
                {
                    BrokerTracing.TraceInfo(
                        "[BrokerClient] Client {0}: Queue_OnPutResponsesSuccessEvent FlushedRequestsCount {1} ProcessedRequestsCount {2}.",
                        this.clientId, this.queue.FlushedRequestsCount, this.queue.ProcessedRequestsCount);
                    lock (this.lockState)
                    {
                        if (this.state == BrokerClientState.AllRequestDone ||
                            this.state == BrokerClientState.ClientConnected ||
                            this.state == BrokerClientState.GetResponse)
                        {
                            if (this.queue.FlushedRequestsCount == this.queue.ProcessedRequestsCount)
                            {
                                BrokerTracing.TraceInfo(
                                    "[BrokerClient] Client {0}: Restart counting ClientIdleTimeout because all flushed requests are processed.",
                                    this.clientId);
                                this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientIdleTimeout,
                                    this.TimeoutToDisconnected, this.state);
                            }
                        }
                    }
                }
            }
            else // For interactive session, enable ClientIdleTimeout when all requests are processed
            {
                if (this.queue.AllRequestsCount == this.queue.ProcessedRequestsCount)
                {
                    BrokerTracing.TraceInfo(
                        "[BrokerClient] Client {0}: Queue_OnPutResponsesSuccessEvent AllRequestsCount {1} ProcessedRequestsCount {2}.",
                        this.clientId, this.queue.AllRequestsCount, this.queue.ProcessedRequestsCount);
                    lock (this.lockState)
                    {
                        if (this.state == BrokerClientState.AllRequestDone ||
                            this.state == BrokerClientState.ClientConnected ||
                            this.state == BrokerClientState.GetResponse)
                        {
                            if (this.queue.AllRequestsCount == this.queue.ProcessedRequestsCount)
                            {
                                BrokerTracing.TraceInfo(
                                    "[BrokerClient] Client {0}: Restart counting ClientIdleTimeout because all requests are processed.",
                                    this.clientId);
                                this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientIdleTimeout,
                                    this.TimeoutToDisconnected, this.state);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle fatal broker queue exception
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="fatalExceptionEventArgs">indicating the exception</param>
        private void Queue_OnFatalExceptionEvent(object sender, ExceptionEventArgs fatalExceptionEventArgs)
        {
            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Critical, 0, "Fatal exception encountered. Exception = {0}", fatalExceptionEventArgs.Exception);
            BrokerQueueException bqException = fatalExceptionEventArgs.Exception as BrokerQueueException;
            if (bqException != null && bqException.ErrorCode == (int)BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_INSUFFICIENT)
            {
                this.monitor.FailServiceJob("Insufficient broker queue storage").GetAwaiter().GetResult();
            }
            else
            {
                BrokerTracing.TraceError("Unknown exception: {0}", fatalExceptionEventArgs);
            }
        }

        /// <summary>
        /// Triggers for queue events
        /// </summary>
        /// <param name="eventId">indicating the event id</param>
        /// <param name="eventArgs">indicating the event args</param>
        private void Queue_OnEvent(BrokerQueueEventId eventId, EventArgs eventArgs)
        {
            if (eventId == BrokerQueueEventId.AllRequestsProcessed)
            {
                // Handle the event in a seperate thread to avoid potential deadlock
                ThreadPool.QueueUserWorkItem(
                    new ThreadHelper<object>(new WaitCallback(this.OnAllRequestsProcessed)).CallbackRoot
                    );
            }
            else if (eventId == BrokerQueueEventId.AllResponesDispatched
                || (this.sharedData.SessionFailed && eventId == BrokerQueueEventId.AvailableResponsesDispatched))
            {
                if (eventId == BrokerQueueEventId.AvailableResponsesDispatched)
                {
                    BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerClient] Client {0}: All available responses dispatched, send EndOfResponse message", this.clientId);
                }
                else
                {
                    BrokerTracing.EtwTrace.LogBrokerClientAllResponseDispatched(this.sharedData.BrokerInfo.SessionId, this.clientId);
                }

                if (this.responsesClient != null)
                {
                    this.responsesClient.EndOfResponses(eventId, (ResponseEventArgs)eventArgs);
                }
            }
        }

        private void OnAllRequestsProcessed(object state)
        {
            lock (this.lockState)
            {
                BrokerTracing.EtwTrace.LogBrokerClientAllRequestDone(this.sharedData.BrokerInfo.SessionId, this.clientId);
                this.state = BrokerClientState.AllRequestDone;
            }

            // Instead of invoking AllRequestDone event handler directly, we fist make a copy of it and
            // then check for null. This ensures that we'll not fire NullReferenceException in case all
            // AllRequestDone subscribers are removed.
            EventHandler allRequestDoneCopy = this.AllRequestDone;
            if (allRequestDoneCopy != null)
            {
                allRequestDoneCopy(this, EventArgs.Empty);
            }

            bool flag = false;
            lock (this.connectedInstance)
            {
                if (this.connectedInstance.Count != 0)
                {
                    this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientIdleTimeout, this.TimeoutToDisconnected, this.state);
                    flag = true;
                }
            }

            if (!flag && !this.singletonInstanceConnected)
            {
                // Bug 5193: If there was no channel connected to this client while all requests are done, close the client immediately
                BrokerTracing.EtwTrace.LogBrokerClientDisconnected(this.sharedData.BrokerInfo.SessionId, this.clientId, "all request done and there's no frontend connected");
                this.SyncDisconnect();
            }
            else if (!flag && this.singletonInstanceConnected)
            {
                // Bug 17241: If there's no connected instance and this.singletonInstanceConnceted
                // is true (this situation could happen when on azure, since it is the REST
                // instance connecting, singletonInstanceConnected is always set to true so that
                // the broker client would not dispose itself as the real client might be still
                // exist), we need to register timeout so that it could be timed out. Otherwise,
                // the client might be leaked forever so that the entire broker would runaway.
                this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientIdleTimeout, this.TimeoutToDisconnected, this.state);
            }
        }

        /// <summary>
        /// Timeout to diconnect the client
        /// </summary>
        /// <param name="state">indicating the state</param>
        private void TimeoutToDisconnected(object state)
        {
            BrokerTracing.EtwTrace.LogBrokerClientDisconnected(this.sharedData.BrokerInfo.SessionId, this.clientId, "of client idle timeout");
            this.SyncDisconnect();
        }

        /// <summary>
        /// Sync disconnect the broker client
        /// </summary>
        private void SyncDisconnect()
        {
            bool flag = false;
            lock (this.lockState)
            {
                BrokerTracing.TraceInfo("[BrokerClient] Client {0}: SyncDisconnect the state is {1} and this.queue.ProcessingRequestsCount/ProcessedRequestsCount/FlushedRequestsCount are {2}/{3}/{4}.", this.clientId, this.state.ToString(), this.queue.ProcessingRequestsCount, this.queue.ProcessedRequestsCount, this.queue.FlushedRequestsCount);

                if (this.state == BrokerClientState.GetResponse
                    || this.state == BrokerClientState.AllRequestDone
                    || (this.state == BrokerClientState.ClientConnected
                    && ((this.queue.ProcessingRequestsCount == 0 && this.queue.ProcessedRequestsCount >= this.queue.FlushedRequestsCount)
                    || (this.v2ProxyClient && this.observer.AllRequestProcessed())))) // check if the broker client is idle for v2 proxy client, in which case responses are directly sent back without persist in the queue.
                {
                    flag = true;
                    this.state = BrokerClientState.Disconnected;

                    if (this.timeoutManager != null)
                        this.timeoutManager.Stop();
                }
            }

            if (flag)
            {
                BrokerTracing.TraceInfo("[BrokerClient] Client {0}: Disconnect client.", this.clientId);

                if (this.responsesClient != null)
                {
                    this.responsesClient.ClientDisconnect(false);
                }

                if (this.ClientDisconnected != null)
                {
                    this.ClientDisconnected(this, EventArgs.Empty);
                }
            }
            else if (!this.sharedData.BrokerInfo.Durable)
            {
                //update the flush counters for persisted and committed for interactive session when client disconnected without removal.
                this.queue.FlushCount();
            }
        }

        /// <summary>
        /// Helper function that maps BrokerClientState to BrokerClientStatus
        /// </summary>
        /// <param name="state"></param>
        internal static BrokerClientStatus MapToBrokerClientStatus(BrokerClientState state)
        {
            switch (state)
            {
                case BrokerClientState.ClientConnected:
                    return BrokerClientStatus.Ready;
                case BrokerClientState.EndRequests:
                    return BrokerClientStatus.Processing;
                case BrokerClientState.AllRequestDone:
                case BrokerClientState.GetResponse:
                    return BrokerClientStatus.Finished;
                default:
                    Debug.Fail("[BrokerClient] BrokerClient should not be in other states.");
                    return default(BrokerClientStatus);
            }
        }

        /// <summary>
        /// Get id of the client instance that sends the message
        /// </summary>
        /// <returns>client instance id as string if it is found in message header, or -1</returns>
        private static int GetBatchId(Message message)
        {
            int index = message.Headers.FindHeader(Constant.ClientInstanceIdHeaderName, Constant.HpcHeaderNS);
            if (index < 0)
            {
                return -1;
            }

            return message.Headers.GetHeader<int>(index);
        }
    }
}
