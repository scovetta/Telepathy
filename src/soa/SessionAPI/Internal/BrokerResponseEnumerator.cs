// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Xml;

    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;

    /// <summary>
    /// Holds messages in message windows
    /// </summary>
    class MessageWindowItem
    {
        public MessageWindowItem(MessageBuffer messageBuffer, string actionFromResponse, string replyAction, UniqueId relatesTo)
        {
            this.MessageBuffer = messageBuffer;
            this.ActionFromResponse = actionFromResponse;
            this.ReplyAction = replyAction;
            this.RelatesTo = relatesTo;
        }

        public MessageBuffer MessageBuffer;
        public string ActionFromResponse;
        public string ReplyAction;
        public Exception CarriedException;
        public UniqueId RelatesTo;
    }

    /// <summary>
    ///   <para>Represents a set of responses from a service-oriented architecture (SOA) service to a set of requests sent by an SOA client.</para>
    /// </summary>
    /// <typeparam name="TMessage">
    ///   <para>The type of the response message. You create a TMessage type by adding a 
    /// service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
    /// </typeparam>
    /// <remarks>
    ///   <para>This class allows you to enumerate the responses through the 
    /// <see cref="MoveNext" /> method.</para>
    /// </remarks>
    public class BrokerResponseEnumerator<TMessage> : IEnumerator<BrokerResponse<TMessage>>, IResponseServiceCallback, IEnumerable<BrokerResponse<TMessage>>
    {
        /// <summary>
        /// Max number of messages in a window
        /// Never use Constant.GetResponse_All with the enumerator
        /// </summary>
        internal const int ResponseWindowSize = 256;

        /// <summary>
        /// Number of windows that should be cached locally. Must be 2 or greater
        /// </summary>
        private const int WindowCount = 2;

        /// <summary>
        /// Used to convert message objects to typed messages
        /// </summary>
        private TypedMessageConverter typedMessageConverter = null;

        /// <summary>
        /// Used to convert message to type EndOfResponses
        /// </summary>
        private TypedMessageConverter endOfResponsesConverter = null;

        /// <summary>
        /// How long to wait for responses before ending enumeration
        /// </summary>
        private TimeSpan newResponsesTimeout;

        /// <summary>
        /// Reference to broker's response service
        /// </summary>
        private IResponseService responseServiceClient = null;

        /// <summary>
        /// Stores the reference to the response service client where
        /// async operation is supported
        /// </summary>
        /// <remarks>
        /// This field would remain null if the given response service client
        /// does not support async operation
        /// </remarks>
        private IResponseServiceAsync responseServiceAsyncClient;

        /// <summary>
        /// Lock to synchronize multiple callbacks
        /// </summary>
        private object responsesLock = new object();

        /// <summary>
        /// Queue of response windows. Response windows are filled in callback and added to 
        ///  this queue so enumerator can use them 
        /// </summary>
        private Queue<Queue<MessageWindowItem>> responsesWindows = new Queue<Queue<MessageWindowItem>>();

        /// <summary>
        /// The current window used to receive responses
        /// </summary>
        private Queue<MessageWindowItem> currentReceiveWindow = new Queue<MessageWindowItem>(ResponseWindowSize);

        /// <summary>
        /// The current window used to enumerator responses
        /// </summary>
        private Queue<MessageWindowItem> currentEnumWindow = null;

        /// <summary>
        /// The current response the enumerator is at
        /// </summary>
        private BrokerResponse<TMessage> currentResponse = null;

        /// <summary>
        /// Signaled by callback when a new response window is ready for the enumerator
        /// </summary>
        private ManualResetEvent newResponseWindowOrEOM = new ManualResetEvent(false);

        /// <summary>
        /// Indicates total responses expected from response service. -1 means total isnt known yet.
        /// </summary>
        private int totalResponseCount = -1;

        /// <summary>
        /// Indicates number of responses currently received
        /// </summary>
        private int currentResponseCount = 0;

        /// <summary>
        /// Indicates whether the client is purged
        /// </summary>
        private EndOfResponsesReason endOfResponsesReason = EndOfResponsesReason.Success;

        /// <summary>
        /// Indicates number of outstanding requests for responses
        /// </summary>
        private int outstandingRequestCount = 0;

        /// <summary>
        /// Reference to callback registration
        /// </summary>
        private CallbackManager callbackManager = null;

        /// <summary>
        /// Id to callback registration
        /// </summary>
        private string callbackManagerId = String.Empty;

        /// <summary>
        /// Action of the operation to return
        /// </summary>
        private string action = null;

        /// <summary>
        /// Reply action of the operation to return
        /// </summary>
        private string replyAction = null;

        /// <summary>
        /// Whether the BrokerClient is disposing or closing
        /// </summary>
        private bool close;

        /// <summary>
        /// Stores the client id
        /// </summary>
        private string clientId;

        /// <summary>
        /// Stores the fault collection
        /// </summary>
        private FaultDescriptionCollection faultCollection;

        /// <summary>
        /// Signals that the broker is down
        /// </summary>
        private ManualResetEvent signalBrokerDown = new ManualResetEvent(false);

        /// <summary>
        /// Store the exception, which leads to broker down (signalBrokerDown.Set).
        /// </summary>
        private Exception exceptionCauseBrokerDown;

        /// <summary>
        /// Reference to BrokerClient that created this enumerator
        /// </summary>
        private BrokerClientBase brokerClient = null;

        /// <summary>
        /// Whether heartbeat signalled the broker or broker node is down
        /// </summary>
        private bool isBrokerNodeDown = false;

        /// <summary>
        /// Response message type. Should always be typeof(TMessage) unless TMessage is Object
        /// </summary>
        private Type responseType = null;

        /// <summary>
        /// Timer used to flush current responses window if not empty and no new responses arrive
        /// </summary>
        private Timer flushResponsesTimer = null;

        /// <summary>
        /// How long before a windows with responses waits for more resposnes before its flushed to the enumerator
        /// </summary>
        private int flushResponsesTimerInterval = 3000;

        /// <summary>
        /// Whether Dispose was called
        /// </summary>
        private bool isDisposeCalled = false;

        /// <summary>
        /// Reference to session
        /// </summary>
        private SessionBase session = null;

        /// <summary>
        /// Initializes a new instance of the BrokerResponseEnumerator class
        /// </summary>
        /// <param name="responseService">Client the broker's response service</param>
        /// <param name="callbackManager">Manages all callbacks for the session</param>
        /// <param name="typedMessageConverter">Typed message convertor</param>
        /// <param name="newResponsesTimeout">Timeout waiting for responses</param>
        /// <param name="action">Action of the response messages</param>
        /// <param name="clientId">client id</param>
        internal BrokerResponseEnumerator(
            SessionBase session,
            IResponseService responseServiceClient,
            CallbackManager callbackManager,
            TimeSpan newResponsesTimeout,
            BrokerClientBase brokerClient,
            string clientId,
            string action,
            string replyAction)
        {
            Type responseType = typeof(TMessage);

            this.newResponsesTimeout = newResponsesTimeout;
            this.callbackManager = callbackManager;
            this.callbackManagerId = this.callbackManager.Register(this);
            this.endOfResponsesConverter = TypedMessageConverter.Create(typeof(EndOfResponses), Constant.EndOfMessageAction);
            this.brokerClient = brokerClient;
            this.clientId = clientId;
            this.session = session;

            // If caller specified a responseType or both actions, make sure the type or the actions are valid before starting the enum by getting 
            //  the deserialization objects now. Else the deseralization objects are retrieved dynamically based on response messages
            if ((typeof(TMessage) != typeof(object)) || (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(replyAction)))
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
            this.responseServiceClient = responseServiceClient;
            this.responseServiceAsyncClient = responseServiceClient as IResponseServiceAsync;
            this.flushResponsesTimer = new Timer(this.FlushResponseTimerCallback, null, Timeout.Infinite, Timeout.Infinite);

            if (this.session.Info is SessionInfo && (this.session.Info as SessionInfo).UseAzureQueue == true)
            {
                this.GetMoreResponses(true, Constant.GetResponse_All);
            }
            else
            {
                this.GetMoreResponses(true, ResponseWindowSize * 2);
            }
        }

        /// <summary>
        ///   <para>Frees resources before the object is reclaimed by garbage collection.</para>
        /// </summary>
        ~BrokerResponseEnumerator()
        {
            this.InternalDispose();
        }

        /// <summary>
        /// Throw session exception according to the reason if needed
        /// </summary>
        /// <param name="reason">indicating the reason</param>
        private static void ThrowIfNeed(EndOfResponsesReason reason)
        {
            switch (reason)
            {
                case EndOfResponsesReason.ClientPurged:
                    throw SessionBase.ClientPurgedException;
                case EndOfResponsesReason.ClientTimeout:
                    throw SessionBase.ClientTimeoutException;
            }
        }

        #region IEnumerator<BrokerResponse<T>> Members

        /// <summary>
        ///   <para>Gets the 
        /// <see cref="BrokerResponse{TMessage}" /> object at the current position in the 
        /// <see cref="BrokerResponseEnumerator{TMessage}" /> enumerator.</para>
        /// </summary>
        /// <value>
        ///   <para>The 
        /// <see cref="BrokerResponse{TMessage}" /> object at the current position in the 
        /// <see cref="BrokerResponseEnumerator{TMessage}" /> enumerator.</para>
        /// </value>
        /// <remarks>
        ///   <para>The 
        /// 
        /// <see cref="Current" /> property is undefined under any of the following conditions:</para> 
        ///   <list type="number">
        ///     <item>
        ///       <description>
        ///         <para>The enumerator is positioned before the first 
        /// element in the collection, which occurs immediately after you create the enumerator or call the  
        /// <see cref="Reset" /> method. You must call the 
        /// 
        /// <see cref="MoveNext" /> method to advance the enumerator to the first element of the collection before you read the value of  
        /// <see cref="Current" />.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>The last call to 
        /// <see cref="MoveNext" /> returned 
        /// False, which indicates the end of the collection.</para>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <para>The enumerator becomes invalid because of changes to the collection, such as adding, modifying, or deleting elements.</para>
        ///       </description>
        ///     </item>
        ///   </list>
        ///   <para>
        ///     <see cref="Current" /> returns the same object until you call 
        /// <see cref="MoveNext" />. 
        /// <see cref="MoveNext" /> sets 
        /// <see cref="Current" /> to the next element.</para>
        /// </remarks>
        /// <seealso cref="MoveNext" />
        /// <seealso cref="BrokerResponse{TMessage}" />
        /// <seealso cref="System.Collections.Generic.IEnumerator{T}.Current" />
        /// <seealso cref="Reset" />
        /// <seealso cref="Current" />
        public BrokerResponse<TMessage> Current
        {
            get
            {
                if (this.currentResponse == null)
                {
                    throw new InvalidOperationException("The enumerator is positioned before the first element of the collection");
                }

                return this.currentResponse;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        ///   <para>Frees resources before the object is reclaimed by garbage collection.<see cref="BrokerResponseEnumerator{TMessage}" /> object used.</para> 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.InternalDispose();
        }

        #endregion

        /// <summary>
        /// Unregisters enumerator's callback with manager
        /// </summary>
        private void InternalDispose()
        {
            try
            {
                lock (this.responsesLock)
                {
                    this.isDisposeCalled = true;

                    if (this.flushResponsesTimer != null)
                    {
                        this.flushResponsesTimer.Dispose();
                        this.flushResponsesTimer = null;
                    }
                }

                if (this.callbackManager != null && !String.IsNullOrEmpty(this.callbackManagerId))
                {
                    this.callbackManager.Unregister(this.callbackManagerId);
                    this.callbackManager = null;
                    this.callbackManagerId = String.Empty;
                }

                if (this.newResponseWindowOrEOM != null)
                {
                    this.newResponseWindowOrEOM.Close();
                    this.newResponseWindowOrEOM = null;
                }

                if (this.signalBrokerDown != null)
                {
                    this.signalBrokerDown.Close();
                    this.signalBrokerDown = null;
                }
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceData(TraceEventType.Error, 0, "Response enumerator dispose failed - {0}", e);
            }
        }

        #region IEnumerator Members

        /// <summary>
        /// Gets current BrokerResponse which wraps response message
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                if (this.currentResponse == null)
                {
                    throw new InvalidOperationException("The enumerator is positioned before the first element of the collection");
                }

                return this.Current;
            }
        }

        /// <summary>
        ///   <para>Advances the enumerator to the next element of the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>
        ///     <see cref="System.Boolean" /> object that specifies whether the enumerator successfully advanced to the next element. 
        /// True indicates that the enumerator successfully advanced to the next element. 
        /// False indicates that the enumerator passed the end of the collection.</para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        ///   <para>The collection was modified after the enumerator was created. </para>
        /// </exception>
        /// <remarks>
        ///   <para>After you create the enumerator or call the 
        /// 
        /// <see cref="Reset" /> method, the enumerator is positioned before the first element of the collection, and the first call to the  
        /// 
        /// <see cref="MoveNext" /> method moves the enumerator over the first element of the collection.</para> 
        ///   <para>If 
        /// 
        /// <see cref="MoveNext" /> passes the end of the collection, the enumerator is positioned after the last element in the collection, and  
        /// <see cref="MoveNext" /> returns 
        /// False. When the enumerator is at this position, subsequent calls to 
        /// <see cref="MoveNext" /> also returns 
        /// False until you call 
        /// <see cref="Reset" />.</para>
        ///   <para>An enumerator remains valid as long as the collection remains unchanged. If you make changes to 
        /// the collection, such as adding, modifying, or deleting elements, the enumerator is irrecoverably invalidated and the next call to  
        /// <see cref="MoveNext" /> or 
        /// <see cref="Reset" /> results in an 
        /// <see cref="System.InvalidOperationException" />.</para>
        /// </remarks>
        /// <seealso cref="Reset" />
        /// <seealso cref="System.Collections.IEnumerator.MoveNext" />
        public bool MoveNext()
        {
            // If the corresponding BrokerClient is closed or disposed, return no more items
            if (this.close)
            {
                if (!this.session.IsBrokerAvailable)
                {
                    throw SessionBase.GetHeartbeatException(this.isBrokerNodeDown);
                }
                else
                {
                    return false;
                }
            }

            ThrowIfNeed(this.endOfResponsesReason);

            bool flag1, flag2, flag3;

            lock (this.responsesLock)
            {
                flag1 = this.currentEnumWindow == null || this.currentEnumWindow.Count == 0;
                flag2 = this.responsesWindows.Count == 0;
                flag3 = this.totalResponseCount != this.currentResponseCount;
            }

            // If there is no current window for enumeration or the current one is empty
            if (flag1)
            {
                // If there are no other windows ready for enumeration
                if (flag2)
                {
                    // If all responses have not been returned
                    if (flag3)
                    {
                        // Wait for more response windows to be ready for enumeration
                        int ret = WaitHandle.WaitAny(new WaitHandle[] { this.newResponseWindowOrEOM, this.signalBrokerDown }, this.newResponsesTimeout, false);

                        // If the timeout expires,
                        if (ret == WaitHandle.WaitTimeout)
                        {
                            // Check to see if the receive window has messages
                            if (this.currentReceiveWindow.Count == 0)
                            {
                                // If not return that there are no more messages
                                // RICHCI: 6/7/9 : Changed from returning false to throwing timeout exception
                                //  so user can distinguish between timeout and no more responses
                                throw new TimeoutException(String.Format(SR.TimeoutGetResponse, this.newResponsesTimeout.TotalMilliseconds));
                            }
                            else
                            {
                                // If there are response messages, move the partially filled receive window to
                                // the "ready to read" response windows queue
                                lock (this.responsesLock)
                                {
                                    this.responsesWindows.Enqueue(this.currentReceiveWindow);
                                    this.currentReceiveWindow = new Queue<MessageWindowItem>();
                                }

                                // Fall through to pulling a new window for the current enum window
                            }
                        }

                        // If the broker down event is signaled, throw exception
                        if (ret == 1)
                        {
                            var faultException = this.exceptionCauseBrokerDown as FaultException<SessionFault>;

                            if (faultException != null)
                            {
                                throw Utility.TranslateFaultException(faultException);
                            }
                            else
                            {
                                throw SessionBase.GetHeartbeatException(this.isBrokerNodeDown);
                            }
                        }

                        ThrowIfNeed(this.endOfResponsesReason);

                        // If the BrokerClient is disposed or closed, return there are no more responses
                        if (this.close)
                        {
                            return false;
                        }

                        // If all we were waiting for was EOM, return there are no more responses
                        if (this.responsesWindows.Count == 0)
                        {
                            return false;
                        }

                        // Else fall through to pulling a new window for the current enum window
                    }
                    else
                    {
                        // If there are no responses pending, return that there are no more responses
                        return false;
                    }
                }

                int responseWindowCount = 0;

                // Get a new current enum window
                lock (this.responsesLock)
                {
                    this.currentEnumWindow = this.responsesWindows.Dequeue();
                    responseWindowCount = this.responsesWindows.Count;
                }

                // If this was the last window
                if (responseWindowCount == 0)
                {
                    // Reset the event so subsequent calls to Move wait
                    this.newResponseWindowOrEOM.Reset();

                    // Do not ask for more responses. The enumerator would ask for more
                    // responses only when outstanding responses is 0.
                }
            }

            // Dequeue the current message
            MessageWindowItem messageWindowItem = this.currentEnumWindow.Dequeue();

            // Bug #15946: handling client side exception thrown from WebResponseHandler(when talking to rest service)
            if (messageWindowItem.CarriedException != null)
            {
                throw messageWindowItem.CarriedException;
            }

            // Create a BrokerResponse object from Message
            this.currentResponse = this.CreateResponse(messageWindowItem.ActionFromResponse, messageWindowItem.ReplyAction, messageWindowItem.MessageBuffer, messageWindowItem.RelatesTo);

            // Return true since Current will have a new element
            return true;
        }

        /// <summary>
        /// Dynamically creates objects needed to deserialize a response message. Keeps last deserializer since its likely it will be reused
        /// </summary>
        /// <param name="actionFromResponse">Request's action pulled from response message</param>
        /// <param name="replyAction">Response's replyaction</param>
        /// <param name="messageBuffer">Response message buffer</param>
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

            return new BrokerResponse<TMessage>(typedMessageConverter, messageBuffer, faultCollection, relatesTo);
        }

        /// <summary>
        ///   <para>Sets the enumerator to its initial position, which is before the first element in the collection.</para>
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///   <para>The collection was modified after the enumerator was created. </para>
        /// </exception>
        /// <remarks>
        ///   <para>An enumerator remains valid as long as the collection remains unchanged. If you make changes to the 
        /// collection, such as adding, modifying, or deleting elements, the enumerator is irrecoverably invalidated and the next call to the  
        /// <see cref="MoveNext" /> or 
        /// <see cref="Reset" /> method results in an 
        /// <see cref="System.InvalidOperationException" />.</para>
        /// </remarks>
        /// <seealso cref="MoveNext" />
        /// <seealso cref="System.Collections.IEnumerator.Reset" />
        public void Reset()
        {
            // If the corresponding BrokerClient is closed or disposed, return
            if (this.close)
            {
                return;
            }

            int ret = 0;

            // Wait for outstanding requests for responses to complete
            if (this.outstandingRequestCount != 0 && this.currentResponseCount != this.totalResponseCount)
            {
                ret = WaitHandle.WaitAny(new WaitHandle[] { this.newResponseWindowOrEOM, this.signalBrokerDown }, this.newResponsesTimeout, false);
                // If this wait times out, go ahead and reset. There is a chance for duplicates
            }

            // If the BrokerClient is disposed or closed or the broker down event is signaled, just return
            if (this.close || ret == 1)
            {
                return;
            }

            // Clear the window currently used by enumerator
            this.currentEnumWindow.Clear();

            // Clear all filled response windows
            // Clear window currently being filled
            lock (this.responsesLock)
            {
                this.responsesWindows.Clear();
                this.currentReceiveWindow.Clear();
            }

            this.totalResponseCount = -1;
            this.currentResponseCount = 0;

            // Refill the response windows from the start
            if (this.session.Info is SessionInfo && (this.session.Info as SessionInfo).UseAzureQueue == true)
            {
                this.GetMoreResponses(true, Constant.GetResponse_All);
            }
            else
            {
                this.GetMoreResponses(true, ResponseWindowSize * WindowCount);
            }
        }

        #endregion

        #region IResponseServiceCallback Members

        /// <summary>
        /// Receives new response messages from broker
        /// </summary>
        /// <param name="response">Response message</param>
        void IResponseServiceCallback.SendResponse(Message response)
        {
            MessageBuffer messageBuffer = null;
            EndOfResponses endOfResponses = null;
            Exception clientSideException = null;
            bool isEOM = false;

            // Reset the heartbeat since operation just succeeded
            this.session.ResetHeartbeat();

            try
            {
                // Check whether this is an EOM message
                isEOM = Utility.IsEOM(response);

                if (!isEOM)
                {
                    // If not EOM, create a copy of the message's buffer so it can be deserialized
                    // A copy is needed because WCF will dispose this message when the callback returns
                    // Alternatively we could deserialize the entire message but we want
                    //  callback to be quick and we may wind up deserializing messages the user
                    //  never looks at (i.e. just checks user data or exits enum early)
                    try
                    {
                        messageBuffer = response.CreateBufferedCopy(Constant.MaxBufferSize);
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}][BrokerResponseEnumerator] Failed to create message buffer from the response message: {1}", this.session.Id, e);
                        clientSideException = e;
                    }
                }
                else
                {
                    // If EOM, deserialize EndOfResponses message
                    endOfResponses = (EndOfResponses)this.endOfResponsesConverter.FromMessage(response);
                }

                string messageId = String.Empty;
                try
                {
                    messageId = SoaHelper.GetMessageId(response).ToString();
                }
                catch
                {
                    // Swallow possible ObjectDisposedException
                }

                SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}][BrokerResponseEnumerator] Response received from broker. IsEOM = {1}, MessageId = {2}", this.session.Id, isEOM, messageId);

                lock (this.responsesLock)
                {
                    // If the enumerator is disposing or disposed, return
                    if (this.isDisposeCalled)
                        return;

                    // If this is not an EOM message
                    if (!isEOM)
                    {
                        string actionFromResponse = BrokerClientBase.GetActionFromResponseMessage(response);

                        // Save the response in the current window
                        MessageWindowItem messageItem = new MessageWindowItem(
                            messageBuffer,
                            actionFromResponse,
                            !response.IsFault ? response.Headers.Action : String.Empty,
                            response.Headers.RelatesTo == null ? SoaHelper.GetMessageId(response) : response.Headers.RelatesTo);

                        // Bug #15946: handling client side exception thrown from WebResponseHandler(when talking to rest service)
                        // To propagate the client side exception to BrokerResponseEnumerator api, store the exception in MessageWindowItem
                        if (string.Equals(response.Headers.Action, Constant.WebAPI_ClientSideException, StringComparison.Ordinal))
                        {
                            clientSideException = response.Properties[Constant.WebAPI_ClientSideException] as Exception;
                        }
                        messageItem.CarriedException = clientSideException;

                        this.currentReceiveWindow.Enqueue(messageItem);

                        // Increment the current number of responses
                        this.currentResponseCount++;

                        // Lower number of outstanding requests
                        if (Interlocked.Decrement(ref this.outstandingRequestCount) == 0)
                        {
                            try
                            {
                                this.GetMoreResponses(false, ResponseWindowSize);
                            }
                            catch (Exception e)
                            {
                                SessionBase.TraceSource.TraceEvent(TraceEventType.Error,
                                    0, "[Session:{0}][BrokerResponseEnumerator] Failed to get more responses in SendResponse context: {1}", this.session.Id, e);

                                // Send broker down signal since connection to broker might be bad
                                ((IResponseServiceCallback)this).SendBrokerDownSignal(false);
                            }
                        }

                        // If the current window is full or there are no more responses
                        if (this.currentReceiveWindow.Count == ResponseWindowSize ||
                                this.currentResponseCount == this.totalResponseCount)
                        {
                            // Disable the flush timer
                            this.flushResponsesTimer.Change(Timeout.Infinite, Timeout.Infinite);

                            // Immediately flush the receive window
                            this.FlushCurrentReceiveWindow();
                        }

                        // Otherwise reset the flushtimer to the full interval
                        else
                        {
                            this.flushResponsesTimer.Change(this.flushResponsesTimerInterval, Timeout.Infinite);
                        }
                    }
                    else
                    {
                        // If we already received an EOM, ignore any others. We may get multiple EOM since 
                        // more responses are requested until we get EOM. This can lead to overrequesting.
                        if (this.totalResponseCount != -1)
                        {
                            return;
                        }

                        // Save total response count
                        this.totalResponseCount = endOfResponses.Count;
                        this.endOfResponsesReason = endOfResponses.Reason;

                        // If there are no more responses
                        if (this.currentResponseCount == this.totalResponseCount)
                        {
                            // Make the current receive window available to enumerator if it has responses
                            if (this.currentReceiveWindow.Count != 0)
                            {
                                this.responsesWindows.Enqueue(this.currentReceiveWindow);
                                this.currentReceiveWindow = new Queue<MessageWindowItem>();
                            }

                            // Notify enumerator that a new window is available. Notify even if
                            // its empty since it allows MoveNext to exit and return there are no 
                            // more items
                            this.newResponseWindowOrEOM.Set();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // If we get an exception here log and dispose the enum
                this.Dispose();

                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}][BrokerResponseEnumerator] Unhandled exception in response enumerator callback :{1}", this.session.Id, e);
            }
        }

        // not implemented
        public void SendResponse(Message m, string clientData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Timer used to flush current responses window if not empty and no new responses arrive
        /// </summary>
        /// <param name="state"></param>
        private void FlushResponseTimerCallback(object state)
        {
            lock (this.responsesLock)
            {
                // If Dispose wasnt called, flush the responses window
                if (!this.isDisposeCalled)
                {
                    this.FlushCurrentReceiveWindow();
                }
            }
        }

        /// <summary>
        /// Flushes current window to enumerator
        /// </summary>
        /// <remarks>Must be called in responsesLock lock</remarks>
        private void FlushCurrentReceiveWindow()
        {
            int responseWindowCount = 0;

            // If there are no responses in the current window, dont flush
            if (this.currentReceiveWindow.Count == 0)
                return;

            // Make the current window available to enumerator
            responseWindowCount = this.responsesWindows.Count;
            this.responsesWindows.Enqueue(this.currentReceiveWindow);

            // If this window was a first
            if (responseWindowCount == 0)
            {
                // Notify enumerator that a new window is available
                this.newResponseWindowOrEOM.Set();
            }

            // Create a new receive window
            this.currentReceiveWindow = new Queue<MessageWindowItem>(ResponseWindowSize);
        }

        /// <summary>
        /// Called by BrokerClient and Sessiobn objects when user closed them
        /// </summary>
        void IResponseServiceCallback.Close()
        {
            this.close = true;

            this.newResponseWindowOrEOM.Set();
        }

        /// <summary>
        /// Called by session object when broker is unavailable
        /// </summary>
        void IResponseServiceCallback.SendBrokerDownSignal(bool isBrokerNodeDown)
        {
            this.isBrokerNodeDown = isBrokerNodeDown;

            try
            {
                SessionBase.TraceSource.TraceData(TraceEventType.Warning, 0, "SendBrokerDownSignal isBrokerNodeDown = {0}, session Id = {1}", isBrokerNodeDown, this.session.Id);
                // Signal that broker is down so enumerators dont block
                this.signalBrokerDown.Set();
            }
            catch (ObjectDisposedException e)
            {
                // Swallow the exception
                SessionBase.TraceSource.TraceData(TraceEventType.Warning, 0, e.ToString());
            }
            catch (NullReferenceException e)
            {
                // Swallow the exception
                SessionBase.TraceSource.TraceData(TraceEventType.Warning, 0, e.ToString());
            }
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        ///   <para>Gets an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.Generic.IEnumerator{T}" /> that you can use to iterate through the collection.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The foreach statement of the C# language (for each in C++, for each 
        /// in Visual Basic) hides the complexity of the enumerators. Therefore, 
        /// using foreach is recommended instead of directly manipulating the enumerator. </para> 
        ///   <para>You can use enumerators to read the data in the collection, but not to modify the underlying collection.</para>
        ///   <para>Initially, the enumerator is positioned before the first element in the collection. At this position, 
        /// <see cref="Current" /> is undefined. Therefore, you must call 
        /// 
        /// <see cref="MoveNext" /> to advance the enumerator to the first element of the collection before reading the value of  
        /// <see cref="Current" />.</para>
        ///   <para>
        ///     <see cref="Current" /> returns the same object until you call 
        /// <see cref="MoveNext" />. 
        /// <see cref="MoveNext" /> sets 
        /// <see cref="Current" /> to the next element.</para>
        ///   <para>If 
        /// 
        /// <see cref="MoveNext" /> passes the end of the collection, the enumerator is positioned after the last element in the collection, and  
        /// <see cref="MoveNext" /> returns 
        /// False. When the enumerator is at this position, subsequent calls to 
        /// <see cref="MoveNext" /> also return 
        /// False. If the last call to 
        /// <see cref="MoveNext" /> returned 
        /// False, 
        /// <see cref="Current" /> is undefined. You cannot set 
        /// 
        /// <see cref="Current" /> to the first element of the collection again; you must create a new enumerator instance instead.</para> 
        ///   <para>An enumerator remains valid as long as the collection remains unchanged. If changes are made to 
        /// the collection, such as adding, modifying, or deleting elements, the enumerator is irrecoverably invalidated and its behavior is undefined.</para>
        ///   <para>The enumerator does not have exclusive access to the collection; therefore, enumerating 
        /// through a collection is intrinsically not a thread-safe procedure. To guarantee thread safety during  
        /// enumeration, you can lock the collection during the entire enumeration. To allow the collection 
        /// to be accessed by multiple threads for reading and writing, you must implement your own synchronization.</para> 
        /// </remarks>
        /// <seealso cref="Current" />
        /// <seealso cref="MoveNext" />
        /// <seealso cref="GetEnumerator" />
        public IEnumerator<BrokerResponse<TMessage>> GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///   <para>Gets an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.Generic.IEnumerator{T}" /> that you can use to iterate through the collection.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The foreach statement of the C# language (for each in C++, for each 
        /// in Visual Basic) hides the complexity of the enumerators. Therefore, 
        /// using foreach is recommended instead of directly manipulating the enumerator. </para> 
        ///   <para>You can use enumerators to read the data in the collection, but not to modify the underlying collection.</para>
        ///   <para>Initially, the enumerator is positioned before the first element in the collection. At this position, 
        /// <see cref="Current" /> is undefined. Therefore, you must call 
        /// 
        /// <see cref="MoveNext" /> to advance the enumerator to the first element of the collection before reading the value of  
        /// <see cref="Current" />.</para>
        ///   <para>
        ///     <see cref="Current" /> returns the same object until you call 
        /// <see cref="MoveNext" />. 
        /// <see cref="MoveNext" /> sets 
        /// <see cref="Current" /> to the next element.</para>
        ///   <para>If 
        /// 
        /// <see cref="MoveNext" /> passes the end of the collection, the enumerator is positioned after the last element in the collection, and  
        /// <see cref="MoveNext" /> returns 
        /// False. When the enumerator is at this position, subsequent calls to 
        /// <see cref="MoveNext" /> also return 
        /// False. If the last call to 
        /// <see cref="MoveNext" /> returned 
        /// False, 
        /// <see cref="Current" /> is undefined. You cannot set 
        /// 
        /// <see cref="Current" /> to the first element of the collection again; you must create a new enumerator instance instead.</para> 
        ///   <para>An enumerator remains valid as long as the collection remains unchanged. If changes are made to 
        /// the collection, such as adding, modifying, or deleting elements, the enumerator is irrecoverably invalidated and its behavior is undefined.</para>
        ///   <para>The enumerator does not have exclusive access to the collection; therefore, enumerating 
        /// through a collection is intrinsically not a thread-safe procedure. To guarantee thread safety during  
        /// enumeration, you can lock the collection during the entire enumeration. To allow the collection 
        /// to be accessed by multiple threads for reading and writing, you must implement your own synchronization.</para> 
        /// </remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Retrieves another window of response messages
        /// </summary>
        /// <param name="reset">Whether to reset from the start</param>
        /// <param name="messageCount">Number of messages to return</param>
        private void GetMoreResponses(bool reset, int messageCount)
        {
            GetResponsePosition position = reset ? GetResponsePosition.Begin : GetResponsePosition.Current;

            try
            {
                // Bug 22175: We must avoid to use the sync version of GetResponses
                // here because it is within a lock on an I/O completion thread
                if (this.responseServiceAsyncClient != null)
                {
                    this.responseServiceAsyncClient.BeginGetResponses(
                        this.action,
                        this.callbackManagerId,
                        position,
                        messageCount,
                        this.clientId,
                        this.GetResponseCallback,
                        null);
                }
                else
                {
                    // In certain cases like inproc broker, we will still need
                    // the sync version but it won't trigger the bug because there
                    // is no pending I/O here.
                    this.responseServiceClient.GetResponses(
                                this.action,
                                this.callbackManagerId,
                                position,
                                messageCount,
                                this.clientId);

                    // Reset the heartbeat since operation just succeeded
                    this.session.ResetHeartbeat();
                }
            }
            catch (FaultException<SessionFault> e)
            {
                throw Utility.TranslateFaultException(e);
            }

            if (messageCount == Constant.GetResponse_All)
            {
                Interlocked.Exchange(ref messageCount, int.MaxValue);
            }
            else
            {
                Interlocked.Add(ref this.outstandingRequestCount, messageCount);
            }
        }

        /// <summary>
        /// Callback for get responses
        /// </summary>
        /// <param name="ar">indicating the async result</param>
        private void GetResponseCallback(IAsyncResult ar)
        {
            Debug.Assert(this.responseServiceAsyncClient != null, "Callback should not be triggered if async operation is not supported.");
            try
            {
                this.responseServiceAsyncClient.EndGetResponses(ar);

                // Reset the heartbeat since operation just succeeded
                this.session.ResetHeartbeat();
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error,
                    0, "[Session:{0}][BrokerResponseEnumerator] Failed to get more responses in SendResponse context: {1}", this.session.Id, e);

                this.exceptionCauseBrokerDown = e;

                // Send broker down signal since connection to broker might be bad
                ((IResponseServiceCallback)this).SendBrokerDownSignal(false);
            }
        }
    }
}
