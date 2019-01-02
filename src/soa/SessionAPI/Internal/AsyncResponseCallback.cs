//------------------------------------------------------------------------------
// <copyright file="AsyncResponseCallback.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Implements response message callback that routes responses to app's 
//      delegate
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Implements response message callback that routes responses to app's delegate
    /// </summary>
    /// <typeparam name="TMessage">Response message type</typeparam>
    internal class AsyncResponseCallback<TMessage> : DisposableObject, IResponseServiceCallback
    {
        /// <summary>
        /// Max number of messages in a window
        /// </summary>
        private int responseWindowSize;

        /// <summary>
        /// Used to convert message objects to typed messages
        /// </summary>
        private TypedMessageConverter typedMessageConverter = null;

        /// <summary>
        /// Stores the fault collection
        /// </summary>
        private FaultDescriptionCollection faultCollection;

        /// <summary>
        /// Response message type. Should always be typeof(TMessage) unless TMessage is Object
        /// </summary>
        private Type responseType = null;

        /// <summary>
        /// Reference to broker's response service
        /// </summary>
        private IResponseService responseService = null;

        /// <summary>
        /// User's response callback
        /// </summary>
        private BrokerResponseHandler<TMessage> callback = null;

        /// <summary>
        /// User's response callback
        /// </summary>
        private BrokerResponseStateHandler<TMessage> callbackState = null;

        /// <summary>
        /// Action of the operation whose responses to retrieve
        /// </summary>
        private string action = null;

        /// <summary>
        /// Reply action of the operation whose response to retrieve
        /// </summary>
        private string replyAction = null;

        /// <summary>
        /// Current response count
        /// </summary>
        private int currentResponseCount = 0;

        /// <summary>
        /// Callback Registration Id
        /// </summary>
        private string callbackManagerId = String.Empty;

        /// <summary>
        /// Client id
        /// </summary>
        private string clientId;

        /// <summary>
        /// Timer for response handler timeout if any
        /// </summary>
        private Timer responseHandlerTimeoutTimer = null;

        /// <summary>
        /// Response handler timeout value in milliseconds
        /// </summary>
        private int responseHanderTimeout = 0;

        /// <summary>
        /// Whether to forward further responses to user code
        /// </summary>
        private bool shuttingDown = false;

        /// <summary>
        /// Lock to coordinate shutdown
        /// </summary>
        private object shutdownLock = new object();

        /// <summary>
        /// Reference to broker client
        /// </summary>
        private BrokerClientBase brokerClient = null;

        /// <summary>
        /// Callbackmanager
        /// </summary>
        private CallbackManager callbackManager = null;

        /// <summary>
        /// Response handler state
        /// </summary>
        private object state = null;

        /// <summary>
        /// The expected number of responses
        /// </summary>
        private int expectedResponseCount = -1;

        /// <summary>
        /// Last broker response received from the broker.
        /// </summary>
        private BrokerResponse<TMessage> lastBrokerResponse = null;

        /// <summary>
        /// Lock needed to check for last response
        /// </summary>
        private object lastResponseLock = new object();

        /// <summary>
        /// Reference to session
        /// </summary>
        private SessionBase session = null;

        /// <summary>
        /// Flag of ignoring the LastResponse property
        /// </summary>
        private bool ignoreIsLastResponseProperty;

        /// <summary>
        /// Initializes a new instance of the AsyncResponseCallback class
        /// </summary>
        /// <param name="session">indicating the session instance</param>
        /// <param name="responseService">Broker's response service</param>
        /// <param name="callbackManager">Reference to internal callback manager</param>
        /// <param name="brokerClient">indicating the broker client instance</param>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="action">Action of responses to retrieve</param>
        /// <param name="replyAction">indicating the reply action</param>
        /// <param name="callback">User's callback delegate</param>
        /// <param name="callbackState">indicating the callback state</param>
        /// <param name="state">indicating the async state</param>
        /// <param name="ignoreIsLastResponseProperty">indicating if ignore the LastResponse property</param>
        public AsyncResponseCallback(
                SessionBase session,
                IResponseService responseService,
                CallbackManager callbackManager,
                BrokerClientBase brokerClient,
                string clientId,
                string action,
                string replyAction,
                BrokerResponseHandler<TMessage> callback,
                BrokerResponseStateHandler<TMessage> callbackState,
                object state,
                bool ignoreIsLastResponseProperty)
        {
            Type responseType = typeof(TMessage);

            // If session is communicating to the Web service, we cannot get all responses
            // at one time by indicating responseWindowSize to -1.
            // This is because broker would not delete responses immediately if it is web
            // service connecting, it only delete responses when the next GetResponse come
            // in. If session API asks for ALL responses, it might lead to hang because of
            // throttling.

            if (brokerClient.TransportScheme == TransportScheme.WebAPI)
            {
                this.responseWindowSize = BrokerResponseEnumerator<TMessage>.ResponseWindowSize;
            }
            else if (brokerClient.TransportScheme == TransportScheme.Http)
            {
                if (session.Info is SessionInfo && (session.Info as SessionInfo).UseAzureQueue == true)
                {
                    this.responseWindowSize = Constant.GetResponse_All;
                }
                else
                {
                    this.responseWindowSize = BrokerResponseEnumerator<TMessage>.ResponseWindowSize;
                }
            }
            else
            {
                this.responseWindowSize = Constant.GetResponse_All;
            }

            this.responseService = responseService;
            this.brokerClient = brokerClient;
            this.clientId = clientId;
            this.callback = callback;
            this.callbackState = callbackState;
            this.callbackManager = callbackManager;
            this.state = state;
            this.session = session;
            this.ignoreIsLastResponseProperty = ignoreIsLastResponseProperty;

            // If caller specified a responseType or both actions, make sure the type or the actions are valid before starting the enum by getting 
            //  the deserialization objects now. Else the deseralization objects are retrieved dynamically based on response messages
            if ((typeof(TMessage) != typeof(Object)) || (!String.IsNullOrEmpty(action) && !String.IsNullOrEmpty(replyAction)))
            {
                string errorMessage;

                // If replyAction isnt known, GetResponseMessageInfo will return it. This is used to track which response deserialization is currently cached in the enumerator
                if (!this.brokerClient.GetResponseMessageInfo(ref action, ref replyAction, ref responseType, out this.typedMessageConverter, out this.faultCollection, out errorMessage))
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }

            this.action = action;
            this.replyAction = replyAction;
            this.responseType = responseType;
        }

        /// <summary>
        /// Initiates listening for responses
        /// </summary>
        /// <param name="callbackManagerId">Callback instance ID</param>
        /// <param name="timeoutMilliseconds">How long to wait for responses (in milliseconds)</param>
        public void Listen(int timeoutMilliseconds)
        {
            // Save callback ID for subsequent calls to GetResponses
            this.callbackManagerId = this.callbackManager.Register(this);

            try
            {
                // Start requesting responses.
                // Note: before SOA Rest service was introduced, we retrived ResponseWindowSize * 2 message
                // at the first time, expecting to have more messages coming down the pipe. To support
                // SOA Rest service, we changed it to retrieve ResponseWindowSize, instead of ResponseWindowSize *2,
                // messages at the first time, to ensure that the next GetResponses call is made after
                // the previous one is completed.
                this.responseService.GetResponses(
                        this.action,
                        this.callbackManagerId,
                        GetResponsePosition.Begin,
                        (this.responseWindowSize != Constant.GetResponse_All) ?
                            this.responseWindowSize : Constant.GetResponse_All,
                        this.clientId);
            }
            catch (FaultException<SessionFault> e)
            {
                throw Utility.TranslateFaultException(e);
            }

            if (timeoutMilliseconds != Timeout.Infinite)
            {
                this.responseHanderTimeout = timeoutMilliseconds;
                this.responseHandlerTimeoutTimer = new Timer(ResponseHandlerTimeoutTimer, null, timeoutMilliseconds, 0);
            }
        }

        #region IResponseServiceCallback Members

        /// <summary>
        /// Receives response messages from broker's response service
        /// </summary>
        /// <param name="message">Response message</param>
        public void SendResponse(Message message)
        {
            try
            {
                int currentResponseCount = 0;

                // Reset the heartbeat since operation just succeeded
                this.session.ResetHeartbeat();

                // Reset timeout timer since a response was received
                if (this.responseHandlerTimeoutTimer != null)
                {
                    this.responseHandlerTimeoutTimer.Change(this.responseHanderTimeout, 0);
                }

                // Bug #15946: handling client side exception thrown from WebResponseHandler(when talking to rest service)
                if (string.Equals(message.Headers.Action, Constant.WebAPI_ClientSideException, StringComparison.Ordinal))
                {
                    Exception e = message.Properties[Constant.WebAPI_ClientSideException] as Exception;
                    InvokeCallback(new BrokerResponse<TMessage>(e, message.Headers.RelatesTo));
                    return;
                }

                // TODO: Consider whether user callback should get an EOM message
                if (!Utility.IsEOM(message))
                {
                    // If the handler is closed, timed out, closed or broker heartbeat signaled, dont forward any more requests to the user.
                    // NOTE: If some responses already slipped through its OK because we prefer that over adding a lock here
                    if (this.shuttingDown)
                        return;

                    // Create a BrokerResponse object wrapper for the message object. A copy of the message must be created
                    MessageBuffer messageBuffer = null;
                    try
                    {
                        messageBuffer = message.CreateBufferedCopy(Constant.MaxBufferSize);
                    }
                    catch (Exception e)
                    {
                        Utility.LogError("AsyncResponseCallback.SendResponse received exception - {0}", e);
                        InvokeCallback(new BrokerResponse<TMessage>(e, message.Headers.RelatesTo));
                        return;
                    }

                    BrokerResponse<TMessage> brokerResponse = CreateResponse(BrokerClientBase.GetActionFromResponseMessage(message),
                                        !message.IsFault ? message.Headers.Action : String.Empty,
                                        messageBuffer,
                                        message.Headers.RelatesTo == null ? SoaHelper.GetMessageId(message) : message.Headers.RelatesTo);

                    BrokerResponse<TMessage> lastResponse = null;

                    if (this.ignoreIsLastResponseProperty)
                    {
                        lastResponse = brokerResponse;
                    }
                    else
                    {
                        // Atomically swap out the last response.
                        lastResponse = Interlocked.Exchange<BrokerResponse<TMessage>>(ref this.lastBrokerResponse, brokerResponse);
                    }

                    // If there was a previous last response
                    if (lastResponse != null)
                    {
                        // Send it to the callback
                        InvokeCallback(lastResponse);

                        // Increment response count
                        currentResponseCount = Interlocked.Increment(ref this.currentResponseCount);
                    }

                    // If the caller wants all messages at once, do so.
                    // Else we always request ResponseWindowSize * 2 messages. If we get to
                    // the end of a window, request another
                    if (this.responseWindowSize != Constant.GetResponse_All && currentResponseCount != 0 &&
                        (currentResponseCount + 1) % this.responseWindowSize == 0)
                    {
                        // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
                        try
                        {

                            SessionBase.TraceSource.TraceInformation("GetResponse : currentResponseCount {0} : clientId {1}", currentResponseCount, clientId);
                            this.responseService.GetResponses(
                                    this.action,
                                    this.callbackManagerId,
                                    GetResponsePosition.Current,
                                    this.responseWindowSize,
                                    this.clientId);
                        }
                        catch (FaultException<SessionFault> e)
                        {
                            throw Utility.TranslateFaultException(e);
                        }
                    }
                }

                // If this is a client purged fault, return that exception in a BrokerResponse
                else
                {
                    TypedMessageConverter endOfResponsesConverter = TypedMessageConverter.Create(typeof(EndOfResponses), Constant.EndOfMessageAction);
                    EndOfResponses endOfResponses = (EndOfResponses)endOfResponsesConverter.FromMessage(message);

                    switch (endOfResponses.Reason)
                    {
                        case EndOfResponsesReason.ClientPurged:
                            InvokeCallback(new BrokerResponse<TMessage>(SessionBase.ClientPurgedException, new UniqueId(Guid.Empty)));
                            break;
                        case EndOfResponsesReason.ClientTimeout:
                            InvokeCallback(new BrokerResponse<TMessage>(SessionBase.ClientTimeoutException, new UniqueId(Guid.Empty)));
                            break;
                        default:
                            // Save how many responses are expected (minus 1 for the last response)
                            System.Diagnostics.Debug.Assert(endOfResponses.Count > 0, "Response count should also be positive number");
                            this.expectedResponseCount = endOfResponses.Count - 1;
                            break;
                    }
                }

                if (!this.ignoreIsLastResponseProperty)
                {
                    // Check to see if we receive the EOM message and all the responses.
                    // If so pass last response to callback marked as such. Also make
                    // sure this isnt called again with a lock.
                    if (this.expectedResponseCount == this.currentResponseCount)
                    {
                        lock (this.lastResponseLock)
                        {
                            if (this.expectedResponseCount == this.currentResponseCount)
                            {
                                // Mark the response as the last one
                                this.lastBrokerResponse.isLastResponse = true;

                                // Send last response to callback. Note since currentResponseCount
                                // is incremented after responses are sent to the callback, the
                                // callback will not be processing any other responses at this time.
                                InvokeCallback(this.lastBrokerResponse);

                                this.expectedResponseCount = -1;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Log and eat unhandled user exceptions from their calback
                Utility.LogError("Unhandled exception processing response - {0}", e);
            }
        }

        /// <summary>
        /// Callback called when broker is down
        /// </summary>
        public void SendBrokerDownSignal(bool isBrokerNodeDown)
        {
            lock (shutdownLock)
            {
                // If the handler is already shutting down, just return. We dont want user to get unecessary exception responses.
                if (this.shuttingDown)
                    return;

                try
                {
                    InvokeCallback(new BrokerResponse<TMessage>(SessionBase.GetHeartbeatException(isBrokerNodeDown), new UniqueId(Guid.Empty)));
                }
                catch (Exception e)
                {
                    // Log and eat unhandled user exceptions from their calback
                    Utility.LogError("Unhandled exception from user's response callback - {0}", e);
                }

                // close this instance
                this.Close();
            }
        }


        /// <summary>
        /// Dynamically creates objects needed to deserialize a response message. Keeps last deserializer since its likely it will be reused
        /// </summary>
        /// <param name="actionFromResponse">indicating the action from the response header</param>
        /// <param name="replyAction">indicating the reply action</param>
        /// <param name="messageBuffer">indicating the message buffer</param>
        /// <param name="relatesTo">indicating the relatesTo header</param>
        /// <returns>returns the created instance of BrokerResponse class</returns>
        private BrokerResponse<TMessage> CreateResponse(string actionFromResponse, string replyAction, MessageBuffer messageBuffer, UniqueId relatesTo)
        {
            TypedMessageConverter typedMessageConverter = null;
            FaultDescriptionCollection faultCollection = null;
            Type responseType = null;

            // If the actionFromResponse is empty, this is a broker fault. BrokerResponse can handle this with no app service specific information
            if (!String.IsNullOrEmpty(actionFromResponse))
            {
                // Check if we already have a response message converter & fault collection for this action. If not get it from the BrokerClient
                if (this.typedMessageConverter != null && this.replyAction == replyAction)
                {
                    typedMessageConverter = this.typedMessageConverter;
                    faultCollection = this.faultCollection;
                    responseType = this.responseType;
                }
                else
                {
                    string errorMessage = null;

                    // Get the deserialization objects from the BrokerClient
                    if (!this.brokerClient.GetResponseMessageInfo(ref actionFromResponse, ref replyAction, ref responseType,
                                                                    out typedMessageConverter, out faultCollection, out errorMessage))
                    {
                        throw new InvalidOperationException(errorMessage);
                    }

                    this.action = actionFromResponse;
                    this.replyAction = replyAction;
                    this.typedMessageConverter = typedMessageConverter;
                    this.faultCollection = faultCollection;
                    this.responseType = responseType;
                }
            }

            return new BrokerResponse<TMessage>(this.typedMessageConverter, messageBuffer, this.faultCollection, relatesTo);
        }

        /// <summary>
        /// Callback for response handler timeout timer
        /// </summary>
        /// <param name="state"></param>
        private void ResponseHandlerTimeoutTimer(object state)
        {
            lock (shutdownLock)
            {
                // If the handler is already shutting down, just return. We dont want user to get unecessary exception responses.
                if (this.shuttingDown)
                    return;

                if (!this.ignoreIsLastResponseProperty)
                {
                    // Atomically swap out the last response.
                    BrokerResponse<TMessage> lastResponse = Interlocked.Exchange<BrokerResponse<TMessage>>(ref this.lastBrokerResponse, null);

                    // If there was last response
                    if (lastResponse != null)
                    {
                        // Send it to the callback
                        InvokeCallback(lastResponse);
                    }
                }

                // If the response handler times out, give the handler a timeout exception
                InvokeCallback(new BrokerResponse<TMessage>(new TimeoutException(), new UniqueId(Guid.Empty)));

                // close this instance
                this.Close();
            }
        }

        void IResponseServiceCallback.Close()
        {
            this.Close();
        }

        protected override void Dispose(bool disposing)
        {
            this.shuttingDown = true;

            this.callbackManager.Unregister(this.callbackManagerId);

            if (disposing)
            {
                // Callback manager calls Close on all callback objects
                if (this.responseHandlerTimeoutTimer != null)
                {
                    this.responseHandlerTimeoutTimer.Dispose();
                    this.responseHandlerTimeoutTimer = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Invokes correct user callback
        /// </summary>
        /// <param name="brokerResponse">BrokerResponse to pass to callback</param>
        /// <param name="state">State to pass to callback</param>
        private void InvokeCallback(BrokerResponse<TMessage> brokerResponse)
        {
            try
            {
                if (this.callback != null)
                {
                    this.callback.Invoke(brokerResponse);
                }
                else if (this.callbackState != null)
                {
                    this.callbackState.Invoke(brokerResponse, this.state);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false, "No callback specifed");
                }
            }

            catch (Exception e)
            {
                Utility.LogError("Unhandled exception invoking response handler - {0}", e);
            }
        }

        #endregion

        public void SendResponse(Message m, string clientData)
        {
            throw new NotImplementedException();
        }
    }
}
