// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BackEnd.AzureQueue;
    using Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.Common.SchedulerAdapter;
    using Microsoft.Telepathy.ServiceBroker.Common.ThreadHelper;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Internal;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// Dispatch messages to service hosts
    /// </summary>
    internal class Dispatcher : ReferenceObject, IDispatcher
    {
        /// <summary>
        /// Endpoint address of host.
        /// </summary>
        protected EndpointAddress Epr
        {
            get;
            private set;
        }

        /// <summary>
        /// Stores the backend binding.
        /// </summary>
        protected Binding BackendBinding
        {
            get;
            private set;
        }

        /// <summary>
        /// Stores the item array
        /// </summary>
        public BrokerQueueItem[] items
        {
            get;
            private set;
        }

#if DEBUG
        /// <summary>
        /// Stores the message count array
        /// </summary>
        private int[] messageCount;
#endif

        /// <summary>
        /// Stores the callback to receive response
        /// </summary>
        public Func<IAsyncResult, Task> ProcessMessageCallback
        {
            get;
            private set;
        }

        /// <summary>
        /// Stores the callback to get task error code
        /// </summary>
        private Func<IAsyncResult, Task> getTaskErrorCodeCallback;

        /// <summary>
        /// Stores the broker queue callback to receive request
        /// </summary>
        private BrokerQueueCallback receiveRequestCallback;

        /// <summary>
        /// Initial endpoint not found wait period = 1000 millisecond.
        /// </summary>
        protected const int InitEndpointNotFoundWaitPeriod = 1000;

        /// <summary>
        /// Stores how long to wait before retry if endpointNotFound exception is encountered.
        /// </summary>
        private int endpointNotFoundWaitPeriod = -1;

        /// <summary>
        /// Stores the timers for each service client to retry on endpointNotFound exception.
        /// </summary>
        private Timer[] endpointNotFoundRetryTimer;

        /// <summary>
        /// Stores the service initialization timeout value, in millisecond
        /// </summary>
        private int serviceInitializationTimeout;

        /// <summary>
        /// A flag indicating whether service initilization is completed
        /// </summary>
        private volatile bool bServiceInitializationCompleted = false;

        public bool ServiceInitializationCompleted
        {
            get
            {
                return this.bServiceInitializationCompleted;
            }
            set
            {
                this.bServiceInitializationCompleted = value;
            }
        }

        /// <summary>
        /// Stores the current client number
        /// </summary>
        private int currentClientNumber;

        /// <summary>
        /// Stores the max capacity;
        /// </summary>
        private int maxCapacity;

        /// <summary>
        /// Stores the dispatcher info
        /// </summary>
        private DispatcherInfo info;

        /// <summary>
        /// Stores the number of the processing requests
        /// </summary>
        private int processingRequests;

        /// <summary>
        /// Stores shared data
        /// </summary>
        public SharedData SharedData
        {
            get;
            private set;
        }

        /// <summary>
        /// Stores the broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Stores the broker queue factory
        /// </summary>
        private BrokerQueueFactory queueFactory;

        /// <summary>
        /// Stores the callback to call RetryOnEndpointNotFoundException function
        /// </summary>
        private TimerCallback retryOnEndpointNotFoundExceptionCallback;

        /// <summary>
        /// A flag indicating if this dispatcher is dispatching requests
        /// </summary>
        private volatile bool isDispatching;

        /// <summary>
        /// Stores the event that get signaled when the dispatcher is stopped and all responses are returned
        /// </summary>
        private ManualResetEvent allResponseReturnedEvent = new ManualResetEvent(false);

        /// <summary>
        /// The scheduler adapter factory
        /// </summary>
        private SchedulerAdapterClientFactory schedulerAdapterClientFactory;

        /// <summary>
        /// Stores the task error code.
        /// </summary>
        private int? taskErrorCode;

        #region Inner Components

        /// <summary>
        /// Stores the request queue adapter
        /// </summary>
        private RequestQueueAdapter requestQueueAdapter;

        /// <summary>
        /// Stores the request senders. Each request has a sender.
        /// </summary>
        private RequestSender[] requestSenders;

        /// <summary>
        /// Stores the response receiver.
        /// </summary>
        private ResponseReceiver responseReceiver;

        /// <summary>
        /// The response queue adapter.
        /// </summary>
        private ResponseQueueAdapter responseQueueAdapter;

        /// <summary>
        /// Retry Limit Exceeded Handler
        /// </summary>
        private RetryLimitExceededHandler retryLimitExceededHandler;

        /// <summary>
        /// Set when the dispatcher enters idle status.
        /// </summary>
        private AutoResetEvent dispatcherIdle;

        /// <summary>
        /// The client indexes that already stoped.
        /// </summary>
        private List<int> stoppedClientIndex;

        #endregion

        /// <summary>
        /// Gets the operation timeout for backend connection.
        /// </summary>
        protected int ServiceOperationTimeout
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service initialization timeout value, in millisecond
        /// </summary>
        protected int ServiceInitializationTimeout
        {
            get { return this.serviceInitializationTimeout; }
            private set { this.serviceInitializationTimeout = value; }
        }

        /// <summary>
        /// Initializes a new instance of the Dispatcher class
        /// </summary>
        /// <param name="info">indicating the dispatcher info</param>
        /// <param name="binding">binding information</param>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="queueFactory">indicating the queue factory</param>
        /// <param name="dispatcherIdle">set when the dispatcher enters idle status</param>
        public Dispatcher(DispatcherInfo info, Binding binding, SharedData sharedData, BrokerObserver observer, BrokerQueueFactory queueFactory, SchedulerAdapterClientFactory schedulerAdapterClientFactory, AutoResetEvent dispatcherIdle)
        {
            this.info = info;
            this.TaskId = info.UniqueId;

            this.Epr = info.ServiceHostAddress;
            this.BackendBinding = binding;
            this.SharedData = sharedData;
            this.dispatcherIdle = dispatcherIdle;

            this.stoppedClientIndex = new List<int>();

            // Will update backend receive timeout with global serviceOperationTimeout if it is enabled (> 0).
            this.ServiceOperationTimeout = this.SharedData.Config.LoadBalancing.ServiceOperationTimeout;

            this.observer = observer;
            this.queueFactory = queueFactory;
            this.schedulerAdapterClientFactory = schedulerAdapterClientFactory;

            this.maxCapacity = this.Capacity * (1 + sharedData.Config.LoadBalancing.ServiceRequestPrefetchCount);
            this.serviceInitializationTimeout = this.SharedData.ServiceConfig.ServiceInitializationTimeout * 11 / 10;  // plus 10% serviceInitializationTimeout
            this.ProcessMessageCallback = new AsyncCallbackReferencedThreadHelper<IAsyncResult>(this.ResponseReceivedAsync, this).CallbackRoot;
            this.receiveRequestCallback = new BrokerQueueCallbackReferencedThreadHelper((item, state) => this.ReceiveRequestAsync(item, state).GetAwaiter().GetResult(), this).CallbackRoot; // TODO: Change this signature
            this.retryOnEndpointNotFoundExceptionCallback = new BasicCallbackReferencedThreadHelper<object>(this.RetryOnEndpointNotFoundException, this).CallbackRoot;
            this.getTaskErrorCodeCallback = new AsyncCallbackReferencedThreadHelper<IAsyncResult>(this.TaskErrorCodeReceivedAsync, this).CallbackRoot;

            this.PassBindingFlags = new bool[this.maxCapacity];
            this.items = new BrokerQueueItem[this.maxCapacity];
            this.endpointNotFoundRetryTimer = new Timer[this.maxCapacity];

#if DEBUG
            this.messageCount = new int[this.maxCapacity];
#endif

            this.PrepareComponents();
        }

        public static ServiceHostController CreateController(DispatcherInfo dispatcherInfo, Binding binding, bool httpsBurst)
        {
            ServiceHostController controller = null;

            try
            {
                EndpointAddress controllerEndpoint = dispatcherInfo.ServiceHostControllerAddress;
                if (controllerEndpoint != null)
                {
                    BrokerTracing.TraceInfo("[Dispatcher] .CreateController: Constructing the ServiceHostController for {0}", dispatcherInfo.ServiceHostControllerAddress);

                    if (dispatcherInfo.GetType().Equals(typeof(ServiceTaskDispatcherInfo)))
                    {
                        controller = new ServiceHostController(controllerEndpoint, binding);
                    }
                    if (dispatcherInfo.GetType().Equals(typeof(AzureDispatcherInfo)))
                    {
                        controller = new AzureServiceHostController(controllerEndpoint, binding, dispatcherInfo as AzureDispatcherInfo, httpsBurst);
                    }
                    if (dispatcherInfo.GetType().Equals(typeof(WssDispatcherInfo)))
                    {
                        controller = new ServiceHostController(controllerEndpoint, binding);
                    }
                }
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceError("[Dispatcher] .CreateController: Error while Constructing the ServiceHostController for {0}. {1}", dispatcherInfo.ServiceHostControllerAddress, ex);
            }

            return controller;
        }

        /// <summary>
        /// Prepare inner components
        /// </summary>
        private void PrepareComponents()
        {
            this.requestQueueAdapter = new RequestQueueAdapter(this.observer, this.queueFactory);

            this.responseQueueAdapter = new ResponseQueueAdapter(this.observer, this.queueFactory, this.SharedData.Config.LoadBalancing.ServiceRequestPrefetchCount);

            this.requestSenders = new RequestSender[this.maxCapacity];

            this.responseReceiver = this.CreateResponseReceiver();

            this.retryLimitExceededHandler = new RetryLimitExceededHandler(this.SharedData, this.responseQueueAdapter);
        }

        /// <summary>
        /// Gets or sets the flags indicating if need to pass binding info for clients.
        /// </summary>
        public bool[] PassBindingFlags
        {
            get;
            private set;
        }

        /// <summary>
        /// Create OnPremiseRequestSender.
        /// </summary>
        /// <returns>OnPremiseRequestSender instance</returns>
        protected virtual RequestSender CreateRequestSender()
        {
            return new OnPremiseRequestSender(
                this.Epr,
                this.BackendBinding,
                this.ServiceOperationTimeout,
                this,
                this.serviceInitializationTimeout,
                InitEndpointNotFoundWaitPeriod);
        }

        protected virtual ResponseReceiver CreateResponseReceiver()
        {
            return new OnPremiseResponseReceiver(this);
        }

        /// <summary>
        /// Start dispatching
        /// </summary>
        internal virtual async Task StartAsync()
        {
            this.isDispatching = true;
            this.currentClientNumber = this.SharedData.DispatcherCount * 2 < this.observer.GetQueuedRequestsCount() ? this.maxCapacity : this.Capacity;
            for (int i = 0; i < this.currentClientNumber; i++)
            {
                this.requestSenders[i] = this.CreateRequestSender();
                await this.requestSenders[i].CreateClientAsync(true, i).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Resume the dispatcher from stopping state.
        /// </summary>
        internal void Resume()
        {
            if (!this.isDispatching)
            {
                BrokerTracing.TraceInfo("[Dispatcher].Resume ID = {0}", this.TaskId);

                this.isDispatching = true;
                this.allResponseReturnedEvent.Reset();

                int[] stoppedIndexes;

                lock (((ICollection)this.stoppedClientIndex).SyncRoot)
                {
                    stoppedIndexes = new int[this.stoppedClientIndex.Count];
                    this.stoppedClientIndex.CopyTo(stoppedIndexes);
                    this.stoppedClientIndex.Clear();
                }

                foreach (int index in stoppedIndexes)
                {
                    this.requestSenders[index].StartClient(new GetNextRequestState(this.GetNextRequest, index));
                }
            }
        }

        /// <summary>
        /// Stop dispatching
        /// </summary>
        internal void Stop()
        {
            if (this.isDispatching)
            {
                this.isDispatching = false;
            }
        }

        /// <summary>
        /// Public access to max capacity
        /// </summary>
        public int MaxCapacity
        {
            get
            {
                return this.maxCapacity;
            }
        }

        /// <summary>
        /// Gets the event triggered when the dispatcher failed
        /// </summary>
        public event EventHandler Failed;

        /// <summary>
        /// Gets the event triggered when the dispatcher is connected to service hsot
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Gets the event triggered when service host failure is detected
        /// </summary>
        public event EventHandler<ServiceInstanceFailedEventArgs> OnServiceInstanceFailedEvent;

        /// <summary>
        /// Gets the task id
        /// </summary>
        public string TaskId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of the processing requests
        /// </summary>
        public int ProcessingRequests
        {
            get { return this.processingRequests; }
        }

        /// <summary>
        /// Gets the dispatcher info
        /// </summary>
        public DispatcherInfo Info
        {
            get { return this.info; }
        }

        /// <summary>
        /// Gets the capacity
        /// </summary>
        public int Capacity
        {
            get { return this.info.Capacity; }
        }

        /// <summary>
        /// Gets the target machine name of current dispatcher
        /// </summary>
        public string MachineName
        {
            get { return this.info.MachineName; }
        }

        /// <summary>
        /// Batch close a list of dispatchers
        /// </summary>
        /// <param name="closeInstances">indicating the instances to be closed</param>
        public static void BatchCloseDispatcher(IList<Dispatcher> closeInstances)
        {
            BrokerTracing.TraceInfo("[Dispatcher] Begin to batch close dispatcher instances, Count = {0}", closeInstances.Count);
            foreach (Dispatcher dispatcher in closeInstances)
            {
                dispatcher.Close();
            }

            BrokerTracing.TraceInfo("[Dispatcher] Succeeded batch closing dispatcher instances.");
        }

        /// <summary>
        /// Close the dispatcher
        /// </summary>
        public void Close()
        {
            BrokerTracing.TraceVerbose("[Dispatcher] .Close: ID = {0}, Stop dispatching.", this.TaskId);

            // stop dispatching
            this.Stop();

            // and wait until all outstanding calls are back
            int processingRequestsCount = this.processingRequests;
            if (processingRequestsCount != 0)
            {
                BrokerTracing.TraceVerbose(
                    "[Dispatcher] .Close: ID = {0}, There are {1} outstanding calls. Waiting them back...",
                    this.TaskId,
                    processingRequestsCount);

                // wait for at most 5 seconds.
                if (this.allResponseReturnedEvent != null &&
                    !this.allResponseReturnedEvent.WaitOne(TimeSpan.FromSeconds(5), true))
                {
                    // We should rarely reach this point...
                    BrokerTracing.TraceWarning("[Dispatcher] .Close: ID = {0}, Not all responses return in 5 seconds.", this.TaskId);
                }
            }

            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Dispose the Dispatcher
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            BrokerTracing.TraceVerbose(
                "[Dispatcher] .Dispose: ID = {0}, disposing = {1}, maxCapacity = {2}",
                this.TaskId,
                disposing,
                this.maxCapacity);

            for (int i = 0; i < this.maxCapacity; i++)
            {
                if (this.requestSenders[i] != null)
                {
                    BrokerTracing.TraceVerbose("[Dispatcher] .Dispose: ID = {0}.{1}, release client", this.TaskId, i);

                    this.requestSenders[i].ReleaseClient();
                }
            }

            // for requests that don't receive their responses when Dispose method is called, just put them back into queue
            if (this.items != null)
            {
                for (int i = 0; i < this.maxCapacity; i++)
                {
                    // BrokerQueueFactory may be null during cleaning up
                    if (this.queueFactory != null && this.items[i] != null)
                    {
                        // Put the calculating requests back to the broker queue

                        Guid messageId = Guid.Empty;

                        try
                        {
                            messageId = Utility.GetMessageIdFromMessage(this.items[i].Message);
                        }
                        catch (ObjectDisposedException)
                        {
                            // Message is closed, so can't get its id.
                            // ObjectDisposedDexception happens in Finaizer
                        }
                        catch (NullReferenceException)
                        {
                            // BrokerQueueItem.buffer is closed and set to
                            // null. This exception can happen in Finaizer.
                            // Swallow it to avoid process crash.

                            BrokerTracing.TraceWarning(
                            "[Dispatcher] .Dispose: ID = {0}.{1}, MessageId = {2}, NullReferenceException when GetMessageIdFromMessage.",
                            this.TaskId,
                            i,
                            messageId);

                            // continue in this rare case
                            continue;
                        }

                        BrokerTracing.TraceWarning(
                            "[Dispatcher] .Dispose: ID = {0}.{1}, MessageId = {2}, Put request back to the queue because the dispatcher is closed.",
                            this.TaskId,
                            i,
                            messageId);

                        // Bug 13430: Need to increase try count if a request is being put back
                        // to broker queue because of task failure. Call HandleExceptionRetry
                        // to do so and mock an exception with SR.TaskFailed as only the message
                        // of the exception will be used.
                        // Indicate dispatchTime to DateTime.Now to let the calculation time to be 0.
                        // This is to make it eaiser as we don't know the calculation time yet and it
                        // is meaningless here because the task is failed and the calculation is
                        // still on air.
                        // Indicate preempted to true to avoid call to GetNextRequest().
                        // Only when the MessageResendLimit = 0, we treat this error as real retry by setting the decreaseTryCount = false.
                        // When the messageResendLimit > 0, it means the message CAN be calculated for more than once, so we decrease the try count to not count the current exception
                        // as retry.
                        // NOTE: the MessageResendLimit = 0 has a special meaning that the message cannot be calculated for more than once, it will have data corruption or other type of 
                        // severe issue in the cluster if a message is calculated twice, so, only in this special case, we won't decrease so that we strictly guarantee the only once
                        // semantics, but this will also bring in more chance for message failure.

                        if (this.SharedData.Config.LoadBalancing.MessageResendLimit == 0)
                        {
                            this.HandleExceptionRetry(i, string.Empty, this.items[i], new Exception(SR.TaskFailed), DateTime.Now, false /* decreaseTryCount */, true);
                        }
                        else
                        {
                            this.HandleExceptionRetry(i, string.Empty, this.items[i], new Exception(SR.TaskFailed), DateTime.Now, true /* decreaseTryCount */, true);
                        }
                    }
                    else
                    {
                        if (this.queueFactory == null)
                        {
                            BrokerTracing.TraceVerbose("[Dispatcher] .Dispose: ID = {0}.{1}, queueFactory is null.", this.TaskId, i);
                        }

                        if (this.items[i] == null)
                        {
                            BrokerTracing.TraceVerbose("[Dispatcher] .Dispose: ID = {0}.{1}, item is null.", this.TaskId, i);
                        }
                    }
                }
            }

            if (disposing)
            {
                try
                {
                    if (this.allResponseReturnedEvent != null)
                    {
                        this.allResponseReturnedEvent.Close();
                        this.allResponseReturnedEvent = null;
                    }
                }
                catch (Exception ex)
                {
                    BrokerTracing.TraceWarning("[Dispatcher].Dispose: ID = {0}, Exception while close allResponseReturnedEvent {1}", this.TaskId, ex);
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Callback to receive request
        /// </summary>
        /// <param name="item">broker queue item</param>
        /// <param name="state">async state</param>
        private async Task ReceiveRequestAsync(BrokerQueueItem item, object state)
        {
            int index = (int)state;
#if DEBUG
            this.messageCount[index]++;
            BrokerTracing.TraceVerbose("[Dispatcher] .ReceiveRequestAsync: ID = {0}.{1}, Received {2} requests.", this.TaskId, index, this.messageCount[index]);
#endif

            BrokerTracing.TraceVerbose(
                "[Dispatcher] .ReceiveRequestAsync: ID = {0}.{1}, Received request, currentClientNumber = {2}",
                this.TaskId,
                index,
                this.currentClientNumber);

            Guid messageId = Utility.GetMessageIdFromMessage(item.Message);

            // Create a dispatch id for this dispatch. This id is used for SOA diag
            // service to analyze user traces.
            Guid dispatchId = Guid.NewGuid();

            BrokerTracing.EtwTrace.LogBackendRequestSent(
                this.SharedData.BrokerInfo.SessionId,
                this.TaskId,
                messageId,
                dispatchId,
                this.MachineName,
                this.Epr.ToString());

            // Move this ahead because when putting back, the counter
            // would be reduced.
            this.observer.RequestProcessing();

            DispatchData data = new DispatchData(this.SharedData.BrokerInfo.SessionId, index, this.TaskId)
            {
                BrokerQueueItem = item,
                MessageId = messageId
            };

            string clientInfo = string.Empty;

            try
            {
                Interlocked.Increment(ref this.processingRequests);

                // if the dispatcher is stopped, put the request back to queue
                if (!this.isDispatching)
                {
                    lock (((ICollection)this.stoppedClientIndex).SyncRoot)
                    {
                        if (!this.isDispatching)
                        {
                            BrokerTracing.EtwTrace.LogBackendDispatcherClosed(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId);

                            this.requestQueueAdapter.PutRequestBack(data);

                            this.DecreaseProcessingCount();

                            this.stoppedClientIndex.Add(index);

                            return;
                        }
                    }
                }

                #region Debug Failure Test
                SimulateFailure.FailOperation(2);
                #endregion

                Debug.Assert(this.ProcessMessageCallback != null);

                // validate the client before processing the request
                if (!await this.requestSenders[index].ValidateClientAsync(index, messageId).ConfigureAwait(false))
                {
                    // Bug 19506: If failed to validate client and put the request
                    // back into the broker queue. Need to log the trace so that
                    // soa diag service could identify the issue.
                    BrokerTracing.EtwTrace.LogBackendValidateClientFailed(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId);

                    try
                    {
                        // If previous client is invalid, ValidateClient creates a new
                        // client; otherwise, go on using it. Following StartClient method
                        // hooks GetNextRequest call back. If the client already changes
                        // to valid state, the call back method is called immediately.
                        // this.clients[index] is a new one if the previous is invalid.

                        //BrokerTracing.TraceVerbose("[Dispatcher] .ReceiveRequestAsync: Hook GetNextRequest call back to the client {0}", client);

                        this.requestSenders[index].StartClient(new GetNextRequestState(this.GetNextRequest, index));
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError("[Dispatcher] .ReceiveRequestAsync: Failed to hook GetNextRequest call back to the client, {0}", e);

                        // excpetion will be caught by following code and goto HandleExceptionRetry method.
                        throw;
                    }

                    this.requestQueueAdapter.PutRequestBack(data);

                    this.DecreaseProcessingCount();

                    return;
                }

                this.items[index] = item;

                this.requestSenders[index].SendRequest(data, dispatchId, index);

                clientInfo = data.Client.ToString();

                if (this.PassBindingFlags[index])
                {
                    // set passBindingFlags to false to indicate that BeginProcessMessage has been invoked on the client.
                    this.PassBindingFlags[index] = false;
                }
            }
            catch (EndpointNotFoundException e)
            {
                BrokerTracing.TraceError("[Dispatcher] .ReceiveRequestAsync: EndpointNotFoundException happens in client {0}, message id = {1}", clientInfo, messageId);

                this.items[index] = null;

                await this.requestSenders[index].CreateClientAsync(false, index).ConfigureAwait(false);

                this.DecreaseProcessingCount();

                this.HandleEndpointNotFoundException(index, item, e, false /* decreaseTryCount */);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[Dispatcher] .ReceiveRequestAsync: exception happens in client {0}, message id = {1}", clientInfo, messageId);

                BrokerTracing.EtwTrace.LogBackendRequestSentFailed(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, e.ToString());

                this.items[index] = null;

                await this.requestSenders[index].CreateClientAsync(false, index).ConfigureAwait(false);

                this.DecreaseProcessingCount();

                this.HandleExceptionRetry(index, clientInfo, item, e, DateTime.Now, false /* decreaseTryCount */, false);
            }
        }

        /// <summary>
        /// Receive response from the service host.
        /// </summary>
        /// <param name="ar">async result</param>
        private async Task ResponseReceivedAsync(IAsyncResult ar)
        {
            Contract.Requires(ar.AsyncState is DispatchData);

            DispatchData data = ar.AsyncState as DispatchData;

            // TODO: this is a workaround. after we have engine, we will save
            // the AsyncResult to DispatchData after BeginProcessMessage returns.
            data.AsyncResult = ar;

            string taskId = data.TaskId;

            DateTime dispatchTime = data.DispatchTime;

            int clientIndex = data.ClientIndex;

            IService client = data.Client;

            BrokerQueueItem item = data.BrokerQueueItem;

            bool servicePreempted = data.ServicePreempted;

            Guid messageId = data.MessageId;

            BrokerTracing.TraceVerbose(
                BrokerTracing.GenerateTraceString(
                    "Dispatcher",
                    "ResponseReceived",
                    this.TaskId,
                    clientIndex,
                    client.ToString(),
                    messageId,
                    "Enter method."));

            try
            {
                this.responseReceiver.ReceiveResponse(data);

                // Exception should already be handled, so just return. Check
                // the code and comment in ResponseReceiver.ReceiveResponse.
                if (data.Exception != null)
                {
                    return;
                }

                // TODO: Need PreemptionHandler
                if (data.ServicePreempted)
                {
                    BrokerTracing.TraceVerbose(
                        BrokerTracing.GenerateTraceString(
                            "Dispatcher",
                            "ResponseReceived",
                            this.TaskId,
                            clientIndex,
                            client.ToString(),
                            messageId,
                            "(Preemption) Stop the dispatcher because of preemption."));

                    this.Stop();
                }
                else
                {
                    // Ask for another request
                    this.GetNextRequest(clientIndex);
                }

                if (data.ExceptionHandled)
                {
                    return;
                }

                #region Debug Failure Test
                SimulateFailure.FailOperation(1);
                #endregion

                // Don't create client and dispatch next message if the task is preempted.
                if (!servicePreempted)
                {
                    if (this.currentClientNumber < this.maxCapacity)
                    {
                        int number = Interlocked.Increment(ref this.currentClientNumber) - 1;
                        if (number < this.maxCapacity)
                        {
                            if (this.requestSenders[number] == null)
                            {
                                this.requestSenders[number] = this.CreateRequestSender();
                            }

                            await this.requestSenders[number].CreateClientAsync(true, number).ConfigureAwait(false);
                        }
                    }
                }

                await this.responseQueueAdapter.PutResponseBack(new DispatchData(this.SharedData.BrokerInfo.SessionId, clientIndex, this.TaskId)
                {
                    BrokerQueueItem = item,
                    MessageId = messageId,
                    ReplyMessage = data.ReplyMessage,
                    DispatchTime = dispatchTime,
                });
            }
            catch (Exception e)
            {
                if (this.SharedData == null)
                {
                    BrokerTracing.TraceError("[Dispatcher] .ResponseReceived : TaskId = {0}.{1}, The sharedData is null unexpectedly.", this.TaskId, clientIndex);
                }

                if (this.SharedData.BrokerInfo == null)
                {
                    BrokerTracing.TraceError("[Dispatcher] .ResponseReceived : TaskId = {0}.{1}, The sharedData.BrokerInfo is null unexpectedly.", this.TaskId, clientIndex);
                }

                BrokerTracing.EtwTrace.LogBackendHandleResponseFailed(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, e.ToString());

                throw;
            }
        }

        /// <summary>
        /// This method handles general exception.
        /// </summary>
        public async Task HandleException(DateTime dispatchTime, int clientIndex, IService client, BrokerQueueItem item, Guid messageId, Exception e)
        {
            string clientInfo = client.ToString();

            BrokerTracing.TraceError(
                BrokerTracing.GenerateTraceString(
                    "Dispatcher",
                    "HandleException",
                    this.TaskId,
                    clientIndex,
                    clientInfo,
                    messageId,
                    "Exception happens.",
                    e));

            if (this.ShouldCheckTaskErrorCode(e))
            {
                await this.HandleMessageLevelPreemptionAsync(dispatchTime, clientIndex, client, item, messageId, e).ConfigureAwait(false);
            }
            else
            {
                // If other exception is catched, ths dispatcher will do the following things:
                // 1. Increase the counter of failure on this request
                // 2. If this counter reaches the limit, a fault message will be generated using the last exception and returned to the client
                // 3. Otherwise, the request will be put back into the broker queue.
                // 4. The dispatcher will ask the broker queue for another request.
                try
                {
                    // Renew channel if exception is thrown
                    // Note: recreate client before retriving next request
                    bool increaseTryCount = await this.requestSenders[clientIndex].RefreshClientAsync(clientIndex, this.IsExceptionIndirect(e), messageId).ConfigureAwait(false);

                    BrokerTracing.TraceError(
                        BrokerTracing.GenerateTraceString(
                            "Dispatcher",
                            "HandleException",
                            this.TaskId,
                            clientIndex,
                            clientInfo,
                            messageId,
                            string.Format("Call HandleExceptionRetry, increaseTryCount = {0}", increaseTryCount)));

                    this.HandleExceptionRetry(clientIndex, clientInfo, item, e, dispatchTime, !increaseTryCount /* decreaseTryCount */, false);
                }
                catch (Exception ex)
                {
                    BrokerTracing.EtwTrace.LogBackendHandleExceptionFailed(
                        this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, ex.ToString());

                    // Cannot handle this exception because we do not know whether the request is put back
                    // Throw this exception to kill the entire process
                    throw;
                }
            }
        }

        /// <summary>
        /// This method handles genernal exception when MessageLevelPreemption
        /// is enabled.
        /// </summary>
        private async Task HandleMessageLevelPreemptionAsync(DateTime dispatchTime, int clientIndex, IService client, BrokerQueueItem item, Guid messageId, Exception e)
        {
            BrokerTracing.TraceVerbose(
                BrokerTracing.GenerateTraceString(
                    "Dispatcher",
                    "HandleMessageLevelPreemptionAsync",
                    this.TaskId,
                    clientIndex,
                    client.ToString(),
                    messageId,
                    "(Preemption) Attempt to get the SchedulerAdapterClient to retrieve the task error code."));
            SchedulerAdapterClient adapterClient =
                await this.schedulerAdapterClientFactory.GetSchedulerAdapterClientAsync().ConfigureAwait(false) as SchedulerAdapterClient;

            if (adapterClient == null)
            {
                // It is for in-proc broker debugging purpose. It runs on one
                // machine, and does not need hpc cluster, so there is not
                // logic of task preemption.
            }
            else
            {
                TaskErrorCodeState state =
                    new TaskErrorCodeState()
                    {
                        AdapterClient = adapterClient,
                        Exception = e,
                        ClientIndex = clientIndex,
                        ServiceClient = client,
                        QueueItem = item,
                        DispatchTime = dispatchTime,
                    };

                if (this.taskErrorCode.HasValue && this.taskErrorCode.Value != 0)
                {
                    BrokerTracing.TraceVerbose(
                        BrokerTracing.GenerateTraceString(
                            "Dispatcher",
                            "HandleMessageLevelPreemptionAsync",
                            this.TaskId,
                            clientIndex,
                            client.ToString(),
                            messageId,
                            string.Format("(Preemption) cached taskErrorCode is {0}.", this.taskErrorCode)));

                    // Call the callback method directly if we already cache the error code.
                    DummyAsyncResult aresult = new DummyAsyncResult();
                    aresult.AsyncState = state;
                    await this.HandleTaskErrorCodeAsync(aresult).ConfigureAwait(false);
                }
                else
                {
                    BrokerTracing.TraceVerbose(
                        BrokerTracing.GenerateTraceString(
                            "Dispatcher",
                            "HandleMessageLevelPreemptionAsync",
                            this.TaskId,
                            clientIndex,
                            client.ToString(),
                            messageId,
                            "(Preemption) Call BeginGetTaskErrorCode."));

                    // There is no need to do this. Caching the item without cleaning it would cause the Null reference exception in disposing
                    /* 
                    // Put the item back to the cache in case exception happens or the call back is not triggered
                    // before dispatcher is disposed. The cache will be cleared in the callback method.
                    this.items[clientIndex] = item;
                    Interlocked.Increment(ref this.processingRequests);
                    */

                    try
                    {
                        this.taskErrorCode = await adapterClient.GetTaskErrorCode(this.SharedData.BrokerInfo.SessionId, this.TaskId).ConfigureAwait(false);
                        DummyAsyncResult aresult = new DummyAsyncResult();
                        aresult.AsyncState = state;
                        await this.HandleTaskErrorCodeAsync(aresult).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        BrokerTracing.TraceError(
                            BrokerTracing.GenerateTraceString(
                                "Dispatcher",
                                "HandleMessageLevelPreemptionAsync",
                                this.TaskId,
                                clientIndex,
                                client.ToString(),
                                messageId,
                                "(Preemption) Exception happens in BeginGetTaskErrorCode.",
                                ex));

                        // There is no need to do this.
                        /*
                        // Clear the cache and decrease the count if BeginGetTaskErrorCode fails.
                        this.items[clientIndex] = null;
                        this.DecreaseProcessingCount();
                        */

// Call the callback method directly if BeginGetTaskErrorCode fails.
#if HPCPACK
                        this.taskErrorCode = ErrorCode.UnknownError;
#endif
                        DummyAsyncResult aresult = new DummyAsyncResult();
                        aresult.AsyncState = state;
                        await this.HandleTaskErrorCodeAsync(aresult).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// This method handles StorageException. It occurs when add request
        /// message to the request queue in Azure storage.
        /// </summary>
        public void HandleStorageException(DateTime dispatchTime, int clientIndex, IService client, BrokerQueueItem item, Guid messageId, StorageException e)
        {
            string xStoreErrorCode = BurstUtility.GetStorageErrorCode(e);

            string errorMessageIncludeRequestId = string.Empty;

            string clientInfo = client.ToString();

            if (e.RequestInformation.ExtendedErrorInformation != null)
            {
                errorMessageIncludeRequestId = e.RequestInformation.ExtendedErrorInformation.ErrorMessage ?? string.Empty;
            }

            string trace = string.Format(
                "status code: {0}, xstore error code: {1}, request id: {2}", e.RequestInformation.HttpStatusCode, xStoreErrorCode, errorMessageIncludeRequestId);

            BrokerTracing.TraceError(
                BrokerTracing.GenerateTraceString(
                    "Dispatcher",
                    "HandleStorageException",
                    this.TaskId,
                    clientIndex,
                    clientInfo,
                    messageId,
                    trace,
                    e));

            // By default, we should increse retry count for "at most once" semantic.
            bool increaseTryCount = true;

            // By default, get next request to process, unless request queue or
            // response queue is deleted.
            bool getNextRequest = true;

            if (BurstUtility.IsQueueNotFound(e))
            {
                // If queue is not found, this dispatcher does not need
                // to get next request.
                getNextRequest = false;

                if (e is RequestStorageException)
                {
                    // If the request queue is not found, it is sure
                    // that the messages is not processed at service
                    // host.
                    increaseTryCount = false;
                }
                else
                {
                    // If the response queue is not found, message may
                    // be already processed at service host, so should
                    // increase try count.
                    increaseTryCount = true;
                }
            }
            else
            {
                WebException we = e.InnerException as WebException;

                if (we != null)
                {
                    var httpWebResponse = we.Response as HttpWebResponse;

                    if (httpWebResponse != null)
                    {
                        if (httpWebResponse.StatusCode == HttpStatusCode.ServiceUnavailable ||
                            httpWebResponse.StatusCode == HttpStatusCode.NotImplemented ||
                            httpWebResponse.StatusCode == HttpStatusCode.BadGateway ||
                            httpWebResponse.StatusCode == HttpStatusCode.HttpVersionNotSupported)
                        {
                            // these error codes indicate that message does not reach server
                            increaseTryCount = false;
                        }
                    }
                }
            }

            try
            {
                this.HandleExceptionRetry(clientIndex, clientInfo, item, e, dispatchTime, !increaseTryCount /* decreaseTryCount */, !getNextRequest /* preempted */);
            }
            catch (Exception ex)
            {
                BrokerTracing.EtwTrace.LogBackendHandleExceptionFailed(
                    this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, ex.ToString());

                // Cannot handle this exception because we do not know whether the request is put back
                // Throw this exception to kill the entire process
                throw;
            }
        }

        /// <summary>
        /// This method handles EndpointNotFoundException. The exception can
        /// occur in on-premise soa, nettcp burst and https burst.
        /// </summary>
        public void HandleEndpointNotFoundException(int clientIndex, IService client, BrokerQueueItem item, Guid messageId, EndpointNotFoundException e)
        {
            BrokerTracing.TraceWarning(
                BrokerTracing.GenerateTraceString(
                    "Dispatcher",
                    "HandleEndpointNotFoundException",
                    this.TaskId,
                    clientIndex,
                    client.ToString(),
                    messageId,
                    "EndpointNotFoundException happens.",
                    e));

            this.requestSenders[clientIndex].RefreshClientAsync(clientIndex, this.IsExceptionIndirect(e), messageId);

            // If EndpointNotFoundException is catched, the dispatcher will do the following things:
            // 1. Put the request back into the broker queue
            // 2. Wait and retry
            try
            {
                // The try count gets increased when BeginProcessMessage succeeds,
                // decrease it when EndpointNotFoundException happens in EndProcessMessage,
                this.HandleEndpointNotFoundException(clientIndex, item, e, true /* decreaseTryCount */);
            }
            catch (Exception ex)
            {
                BrokerTracing.EtwTrace.LogBackendHandleEndpointNotFoundExceptionFailed(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, ex.ToString());

                // Cannot handle this exception because we do not know whether the request is put back
                // Throw this exception to kill the entire process
                throw;
            }
        }

        /// <summary>
        /// If the exception is from the connection between dispatcher and host,
        /// it might be caused by the job preemption or cancellation
        /// </summary>
        /// <param name="e">exception received by the dispatcher</param>
        /// <returns>should check the error code or not</returns>
        protected virtual bool ShouldCheckTaskErrorCode(Exception e)
        {
            return (e is CommunicationException && this.SharedData.ServiceConfig.EnableMessageLevelPreemption);
        }

        /// <summary>
        /// Call back method of the gettaskErrorCode.
        /// </summary>
        /// <param name="asyncResult">async result</param>
        private async Task TaskErrorCodeReceivedAsync(IAsyncResult asyncResult)
        {
            TaskErrorCodeState state = asyncResult.AsyncState as TaskErrorCodeState;
            Debug.Assert(state != null, "TaskErrorCodeReceived: asyncResult.AsyncState must be TaskErrorCodeState");

            SchedulerAdapterClient adapterClient = state.AdapterClient;
            int clientIndex = state.ClientIndex;
            IService client = state.ServiceClient;
            BrokerQueueItem item = state.QueueItem;
            Guid messageId = Utility.GetMessageIdFromMessage(item.Message);

            try
            {
                BrokerTracing.TraceVerbose(
                BrokerTracing.GenerateTraceString(
                    "Dispatcher",
                    "TaskErrorCodeReceived",
                    this.TaskId,
                    clientIndex,
                    client.ToString(),
                    messageId,
                    "(Preemption) Call EndGetTaskErrorCode."));

                //this.taskErrorCode = adapterClient.EndGetTaskErrorCode(asyncResult);
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceError(
                    BrokerTracing.GenerateTraceString(
                        "Dispatcher",
                        "TaskErrorCodeReceived",
                        this.TaskId,
                        clientIndex,
                        client.ToString(),
                        messageId,
                        "(Preemption) Exception happens in EndGetTaskErrorCode.",
                        ex));
            }

            await this.HandleTaskErrorCodeAsync(asyncResult).ConfigureAwait(false);
        }

        /// <summary>
        /// Handle the exception according to the task error code
        /// </summary>
        private async Task HandleTaskErrorCodeAsync(IAsyncResult asyncResult)
        {
            TaskErrorCodeState state = asyncResult.AsyncState as TaskErrorCodeState;
            Debug.Assert(state != null, "HandleTaskErrorCode: asyncResult.AsyncState must be TaskErrorCodeState");

            SchedulerAdapterClient adapterClient = state.AdapterClient;
            Exception exception = state.Exception;
            bool exceptionIndirect = this.IsExceptionIndirect(exception);
            int clientIndex = state.ClientIndex;

            string clientInfo = state.ServiceClient.ToString();

            BrokerQueueItem item = state.QueueItem;
            Guid messageId = Utility.GetMessageIdFromMessage(item.Message);
            DateTime dispatchTime = state.DispatchTime;

            bool taskPreempted = false;
            if (this.taskErrorCode.HasValue)
            {
#if HPCPACK
                if (this.taskErrorCode.Value == ErrorCode.Execution_TasksPreempted ||
                    this.taskErrorCode.Value == ErrorCode.Execution_TaskCanceledBeforeAssignment ||
                    this.taskErrorCode.Value == ErrorCode.Execution_TaskCanceledDuringExecution ||
                    this.taskErrorCode.Value == ErrorCode.Execution_TaskCanceledOnJobRequeue ||
                    this.taskErrorCode.Value == ErrorCode.Execution_TasksJobCanceled ||
                    this.taskErrorCode.Value == ErrorCode.Operation_CanceledByUser)
                {
                    taskPreempted = true;
                }
#endif

                BrokerTracing.TraceVerbose(
                    BrokerTracing.GenerateTraceString(
                        "Dispatcher",
                        "HandleTaskErrorCode",
                        this.TaskId,
                        clientIndex,
                        clientInfo,
                        messageId,
                        string.Format("(Preemption) taskErrorCode is {0}, taskPreempted is {1}.", this.taskErrorCode.Value, taskPreempted)));
            }
            else
            {
                BrokerTracing.TraceVerbose(
                    BrokerTracing.GenerateTraceString(
                        "Dispatcher",
                        "HandleTaskErrorCode",
                        this.TaskId,
                        clientIndex,
                        clientInfo,
                        messageId,
                        "(Preemption) taskErrorCode has no value."));
            }

            // By default increase try count for "at most once" semantic.
            bool increaseTryCount = true;

            if (taskPreempted)
            {
                BrokerTracing.TraceVerbose(
                    BrokerTracing.GenerateTraceString(
                        "Dispatcher",
                        "HandleTaskErrorCode",
                        this.TaskId,
                        clientIndex,
                        clientInfo,
                        messageId,
                        "(Preemption) Stop the dispatcher because of preemption."));

                this.Stop();
            }
            else
            {
                increaseTryCount = await this.requestSenders[clientIndex].RefreshClientAsync(clientIndex, exceptionIndirect, messageId).ConfigureAwait(false);

                BrokerTracing.TraceVerbose(
                    BrokerTracing.GenerateTraceString(
                        "Dispatcher",
                        "HandleTaskErrorCode",
                        this.TaskId,
                        clientIndex,
                        clientInfo,
                        messageId,
                        string.Format("(Preemption) increaseTryCount is {0}.", increaseTryCount)));
            }

            try
            {
                if (!(asyncResult is DummyAsyncResult))
                {
                    // no need to call following method if asyncResult is DummyAsyncResult,
                    // because they are already called in the ResponseReceived method.
                    this.items[clientIndex] = null;
                    this.DecreaseProcessingCount();
                }

                BrokerTracing.TraceVerbose(
                    BrokerTracing.GenerateTraceString(
                        "Dispatcher",
                        "HandleTaskErrorCode",
                        this.TaskId,
                        clientIndex,
                        clientInfo,
                        messageId,
                        "(Preemption) Call HandleExceptionRetry."));

                this.HandleExceptionRetry(clientIndex, clientInfo, item, exception, dispatchTime, !increaseTryCount /* decreaseTryCount */, taskPreempted);
            }
            catch (Exception ex)
            {
                BrokerTracing.EtwTrace.LogBackendHandleExceptionFailed(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, ex.ToString());

                // Cannot handle this exception because we do not know whether the request is put back
                // Throw this exception to kill the entire process
                throw;
            }
        }

        /// <summary>
        /// Get next request from queue
        /// </summary>
        /// <param name="serviceClientIndex">index of the service client through which the request will be sent</param>
        public void GetNextRequest(int serviceClientIndex)
        {
            BrokerTracing.TraceVerbose(
                "[Dispatcher] .GetNextRequest: ID = {0}.{1}, isDispatching is {2}",
                this.TaskId,
                serviceClientIndex,
                this.isDispatching);

            bool shouldDispatch = true;
            if (!this.isDispatching)
            {
                lock (((ICollection)this.stoppedClientIndex).SyncRoot)
                {
                    if (!this.isDispatching)
                    {
                        this.stoppedClientIndex.Add(serviceClientIndex);
                        shouldDispatch = false;
                    }
                }
            }

            if (shouldDispatch)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate(object state)
                    {
                        this.queueFactory.Dispatcher.GetRequestAsync(this.receiveRequestCallback, serviceClientIndex);
                    });
            }
        }

        /// <summary>
        /// Decrease processingRequests counter, and check if all outstanding calls are completed.
        /// </summary>
        public void DecreaseProcessingCount()
        {
            int processingRequestsCount = Interlocked.Decrement(ref this.processingRequests);

            // notify that all outstanding calls are back
            if (!this.isDispatching && processingRequestsCount == 0 && this.allResponseReturnedEvent != null)
            {
                try
                {
                    this.dispatcherIdle.Set();
                }
                catch (ObjectDisposedException)
                {
                    BrokerTracing.TraceWarning("[Dispatcher] .DecreaseProcessingCount: the dispatcherIdle event is disposed already, the service job is finished.");
                }

                this.allResponseReturnedEvent.Set();
            }
        }

        /// <summary>
        /// Handle fault exception thrown by service code
        /// </summary>
        /// <param name="clientIndex">indicating the client index</param>
        /// <param name="item">indicating the broker item</param>
        /// <param name="reply">indicating the fault message</param>
        /// <param name="dispatchTime">indicating the dispatch time</param>
        /// <returns>returns a value indicating whether the fault exception is handled by broker</returns>
        public bool HandleFaultExceptionRetry(int clientIndex, BrokerQueueItem item, Message reply, DateTime dispatchTime, out bool preemption)
        {
            preemption = false;
            this.PassBindingFlags[clientIndex] = true;

            string action = reply.Headers.Action;

            // NOTICE: This is a workround for Java version service host. We have to dig into the headers to find out
            // "action" header, because the BasicHttpBinding uses SOAP11 that can't support for Action property.
            if (string.IsNullOrEmpty(action))
            {
                string targetHeaderName = "Action";
                try
                {
                    for (int i = 0; i < reply.Headers.Count; i++)
                    {
                        MessageHeaderInfo info = reply.Headers[i];
                        if (targetHeaderName.Equals(info.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            action = reply.Headers.GetHeader<string>(i);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError("[Dispatcher] .HandleFaultExceptionRetry: Failed to get the Action header. {0}", e);
                }
            }

            Guid messageId = Utility.GetMessageIdFromMessage(item.Message);

            if (action == RetryOperationError.Action)
            {
                BrokerTracing.EtwTrace.LogBackendResponseReceivedRetryOperationError(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, item.TryCount);

                if (item.TryCount - 1 >= this.SharedData.Config.LoadBalancing.MessageResendLimit)
                {
                    // TODO: FaultResponseHandler should pass the DispatchData to RetryLimitExceededHandler here
                    var dispatchData = new DispatchData(this.SharedData.BrokerInfo.SessionId, clientIndex, this.TaskId)
                    {
                        BrokerQueueItem = item,
                        MessageId = messageId,
                        DispatchTime = dispatchTime,
                        ReplyMessage = reply,
                    };

                    this.retryLimitExceededHandler.HandleRetryLimitExceeded(dispatchData);
                }
                else
                {

#region Debug Failure Test
                    SimulateFailure.FailOperation(2);
#endregion

                    // Currently since data instance is not widely used here (inside the
                    // exception handler logic), we will need to build the instance where
                    // ever it would be invoked into a "decoupled" components.
                    // This is only temporary since once we have done decoupling the caller
                    // (exception handler in this case), the data instace would naturally
                    // be here.
                    DispatchData data = new DispatchData(this.SharedData.BrokerInfo.SessionId, clientIndex, this.TaskId)
                    {
                        BrokerQueueItem = item,
                        MessageId = messageId
                    };

                    this.requestQueueAdapter.PutRequestBack(data);
                }

                return true;
            }
            else if (action == SessionFault.Action)
            {
                MessageFault fault = MessageFault.CreateFault(reply, int.MaxValue);
                SessionFault sessionFault = fault.GetDetail<SessionFault>();
                BrokerTracing.TraceVerbose("[Dispatcher] .HandleFaultExceptionRetry: sessionFault.Code: {0}", sessionFault.Code);
                BrokerTracing.EtwTrace.LogBackendResponseReceivedSessionFault(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, SOAFaultCode.GetFaultCodeName(sessionFault.Code));

                // Message inspector at host side sets Service_Preempted fault
                // code to indicate that message is not processed by service
                // host because preemption already happens.
                preemption = (sessionFault.Code == SOAFaultCode.Service_Preempted);

                if (preemption)
                {
                    // If the message is skipped by service host because of
                    // preemption, should not increase its try count.
                    item.TryCount--;

                    BrokerTracing.TraceVerbose(
                        "[Dispatcher] .HandleFaultExceptionRetry: Decrease try count to {0} because message is skipped by service host when preemption happens.",
                        item.TryCount);
                }

                DispatchData data = new DispatchData(this.SharedData.BrokerInfo.SessionId, clientIndex, this.TaskId)
                {
                    BrokerQueueItem = item,
                    MessageId = messageId
                };

                this.requestQueueAdapter.PutRequestBack(data);

                if (preemption)
                {
                    int messageCount;
                    int.TryParse(sessionFault.Reason, out messageCount);
                    BrokerTracing.TraceVerbose("[Dispatcher] .HandleFaultExceptionRetry: (Preemption) The count of processing message on host is {0}", messageCount);

                    // will remove the dispatcher if no processing message left on the host
                    if (messageCount == 0)
                    {
                        BrokerTracing.TraceVerbose("[Dispatcher] .HandleFaultExceptionRetry: (Preemption) Call HandleServiceInstanceFailure, task id {0}.", this.info.UniqueId);
                        this.HandleServiceInstanceFailure(sessionFault);
                    }
                }
                else
                {
                    this.HandleServiceInstanceFailure(sessionFault);
                }

                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// Handle normal exception while receiving reply
        /// </summary>
        /// <param name="index">indicating the client index</param>
        /// <param name="item">indicating the broker item</param>
        /// <param name="ex">indicating the exception</param>
        /// <param name="dispatchTime">indicating the dispatch time</param>
        /// <param name="decreaseTryCount">indicating if should decrease the try count</param>
        /// <param name="preempted">indicating if the task is preempted or not</param>
        private void HandleExceptionRetry(int index, string clientInfo, BrokerQueueItem item, Exception ex, DateTime dispatchTime, bool decreaseTryCount, bool preempted)
        {
            Guid messageId = Utility.GetMessageIdFromMessage(item.Message);

            BrokerTracing.EtwTrace.LogBackendResponseReceivedFailed(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, item.TryCount, ex.ToString());

            if (decreaseTryCount)
            {
                // item.TryCount increased when BeginProcessMessage method succeeds.
                // Decrease it here if we don't want to increase it.
                item.TryCount--;

                BrokerTracing.TraceVerbose(
                    BrokerTracing.GenerateTraceString(
                        "Dispatcher",
                        "HandleExceptionRetry",
                        this.TaskId,
                        index,
                        clientInfo,
                        messageId,
                        string.Format("Decrease the message try count to {0}.", item.TryCount)));
            }

            if (item.TryCount - 1 >= this.SharedData.Config.LoadBalancing.MessageResendLimit)
            {
                // TODO: ExceptionHandler should pass the DispatchData to RetryLimitExceededHandler here
                var dispatchData = new DispatchData(this.SharedData.BrokerInfo.SessionId, index, this.TaskId)
                {
                    BrokerQueueItem = item,
                    MessageId = messageId,
                    DispatchTime = dispatchTime,
                    Exception = ex,
                };

                this.retryLimitExceededHandler.HandleRetryLimitExceeded(dispatchData);
            }
            else
            {
                DispatchData data = new DispatchData(this.SharedData.BrokerInfo.SessionId, index, this.TaskId)
                {
                    BrokerQueueItem = item,
                    MessageId = messageId
                };

                this.requestQueueAdapter.PutRequestBack(data);
            }

            if (!preempted)
            {
                BrokerTracing.TraceVerbose(
                    BrokerTracing.GenerateTraceString(
                        "Dispatcher",
                        "HandleExceptionRetry",
                        this.TaskId,
                        index,
                        clientInfo,
                        messageId,
                        "Call GetNextRequest."));

                this.GetNextRequest(index);
            }
        }

        /// <summary>
        /// Handle service instance unreachable
        /// </summary>
        private void HandleServiceInstanceUnreachable()
        {
            SessionFault sessionFault = new SessionFault(SOAFaultCode.Service_Unreachable, "EndpointNotFoundException caught");
            ThreadPool.QueueUserWorkItem(new ThreadHelper<object>(new WaitCallback(this.OnServiceInstanceFailed)).CallbackRoot, sessionFault);
        }

        /// <summary>
        /// Handle service instance failure
        /// </summary>
        /// <param name="sessionFault">failure reason</param>
        public void HandleServiceInstanceFailure(SessionFault sessionFault)
        {
            ThreadPool.QueueUserWorkItem(new ThreadHelper<object>(new WaitCallback(this.OnServiceInstanceFailed)).CallbackRoot, sessionFault);
        }

        /// <summary>
        /// Notify that this dispatcher is connected to service instance
        /// </summary>
        /// <param name="state"></param>
        public void OnServiceInstanceConnected(object state)
        {
            if (this.Connected != null)
            {
                try
                {
                    this.Connected(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError("[[Dispatcher] .OnServiceInstanceConnected: Connected event handler throws exception: {0}", e);
                }
            }
        }

        /// <summary>
        /// Thread pool callback to handle service instance failure
        /// </summary>
        /// <param name="obj">callback state</param>
        private void OnServiceInstanceFailed(object state)
        {
            if (this.OnServiceInstanceFailedEvent != null)
            {
                SessionFault sessionFault = state as SessionFault;
                ServiceInstanceFailedEventArgs args = new ServiceInstanceFailedEventArgs(sessionFault);
                this.OnServiceInstanceFailedEvent(this, args);
            }
        }

        /// <summary>
        /// Check if the specified exception is from broker proxy.
        /// On-premise disptacher doesn't need to handle this
        /// </summary>
        protected virtual bool IsExceptionIndirect(Exception e)
        {
            return false;
        }

        /// <summary>
        /// Close this dispatcher
        /// </summary>
        public void CloseThis()
        {
            ThreadPool.QueueUserWorkItem(new ThreadHelper<object>(new WaitCallback(this.CallbackToCloseThis)).CallbackRoot);
        }

        /// <summary>
        /// Callback to close the dispatcher
        /// </summary>
        /// <param name="obj">null object</param>
        private void CallbackToCloseThis(object obj)
        {
            if (this.Failed != null)
            {
                this.Failed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handle the endpoint not found exception
        /// </summary>
        /// <param name="index">indicate the client index</param>
        /// <param name="item">indicate the broker queue item</param>
        /// <param name="ex">indicate the exception</param>
        /// <param name="decreaseTryCount">should decrease the try count or not</param>
        private void HandleEndpointNotFoundException(int index, BrokerQueueItem item, EndpointNotFoundException ex, bool decreaseTryCount)
        {
            if (decreaseTryCount)
            {
                item.TryCount--;
            }

            // There are 2 cases that could throw EndpointNotFoundException:
            // 1. target service initializaion is not completed yet.
            // 2. target service is unreachable.
            // For case 1, we do back-off retry;  for case 2, we do periodical retry.

            Guid messageId = Utility.GetMessageIdFromMessage(item.Message);

            BrokerTracing.EtwTrace.LogBackendEndpointNotFoundExceptionOccured(this.SharedData.BrokerInfo.SessionId, this.TaskId, messageId, ex.ToString());

            // Put the request back into the broker queue
            DispatchData data = new DispatchData(this.SharedData.BrokerInfo.SessionId, index, this.TaskId)
            {
                BrokerQueueItem = item,
                MessageId = messageId
            };

            this.requestQueueAdapter.PutRequestBack(data);

            // We need to pass binding data again if the first message doesn't get to proxy due to EndpointNotFoundException.
            this.PassBindingFlags[index] = true;

            if (!this.bServiceInitializationCompleted)
            {
                int elapsedTimeSinceTaskStart = (int)(DateTime.Now - this.info.TaskStartTime).TotalMilliseconds;

                // if service initialization timeout is used up, set bServiceInitilizationCompleted to true, and consider target service as unreachable;
                if (elapsedTimeSinceTaskStart >= this.serviceInitializationTimeout)
                {
                    this.bServiceInitializationCompleted = true;
                    this.HandleServiceInstanceUnreachable();
                    return;
                }

                if (this.endpointNotFoundWaitPeriod == -1)
                {
                    this.endpointNotFoundWaitPeriod = InitEndpointNotFoundWaitPeriod;
                }
                else
                {
                    if (index == 0)
                    {
                        // back off
                        // Note: every service client can possibly reach this point.  However, we don't want to back-off wait period for each client.
                        // So here, client with index == 0 is the one we care.
                        this.endpointNotFoundWaitPeriod *= 2;
                    }
                }

                int timeToWait = this.endpointNotFoundWaitPeriod;
                if (elapsedTimeSinceTaskStart + timeToWait >= this.serviceInitializationTimeout)
                {
                    timeToWait = this.serviceInitializationTimeout - elapsedTimeSinceTaskStart;
                }

#region Debug Failure Test
                SimulateFailure.FailOperation(1);
#endregion

                BrokerTracing.TraceEvent(
                    TraceEventType.Information,
                    0,
                    "[Dispatcher] .HandleEndpointNotFoundException: ID = {0}.{1}, Wait {2} millisecond. Will retry then.",
                    this.TaskId,
                    index,
                    timeToWait);

                this.endpointNotFoundRetryTimer[index] = new Timer(this.retryOnEndpointNotFoundExceptionCallback, index, timeToWait, Timeout.Infinite);
            }
            else
            {
                BrokerTracing.TraceEvent(
                    TraceEventType.Error,
                    0,
                    "[Dispatcher] .HandleEndpointNotFoundException: ID = {0}.{1} EndpointNotFound exception retry limit exceeded, put the request back to the broker queue.",
                    this.TaskId,
                    index);

                this.HandleServiceInstanceUnreachable();
            }
        }

        /// <summary>
        /// Thread pool callback for retrying a ServiceClient on EndpointNotFoundException
        /// </summary>
        /// <param name="state">call back state</param>
        private void RetryOnEndpointNotFoundException(object state)
        {
            int clientIndex = (int)state;

            if (this.endpointNotFoundRetryTimer[clientIndex] != null)
            {
                this.endpointNotFoundRetryTimer[clientIndex].Dispose();
                this.endpointNotFoundRetryTimer[clientIndex] = null;
            }

            this.GetNextRequest(clientIndex);
        }

        /// <summary>
        /// Cleanup the specified client.
        /// </summary>
        /// <remarks>
        /// On premise dispatcher does not need to implement this method.
        /// </remarks>
        public virtual bool CleanupClient(IService client)
        {
            return true;
        }

        /// <summary>
        /// It is used by dispatcher only for TaskErrorCodeReceived and HandleTaskErrorCode methods.
        /// </summary>
        private class DummyAsyncResult : IAsyncResult
        {
            public object AsyncState
            {
                get;
                set;
            }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
            }
        }

        /// <summary>
        /// It is used by dispatcher only for TaskErrorCodeReceived and HandleTaskErrorCode methods.
        /// </summary>
        private class TaskErrorCodeState
        {
            public SchedulerAdapterClient AdapterClient
            {
                get;
                set;
            }

            public Exception Exception
            {
                get;
                set;
            }

            public int ClientIndex
            {
                get;
                set;
            }

            public IService ServiceClient
            {
                get;
                set;
            }

            public BrokerQueueItem QueueItem
            {
                get;
                set;
            }

            public Guid MessageId
            {
                get;
                set;
            }

            public DateTime DispatchTime
            {
                get;
                set;
            }
        }
    }
}
