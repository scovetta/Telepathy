// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.FrontEnd
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.Common.ThreadHelper;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// The FrontEnd for request/reply MEP (works for both BasicHttpBinding and WSHttpBinding)
    /// </summary>
    /// <typeparam name="T">indicate the reply channel type</typeparam>
    internal class RequestReplyFrontEnd<T> : FrontEndBase
        where T : class, IReplyChannel
    {
        /// <summary>
        /// Get the magic number
        /// </summary>
        private static readonly int magicNumber = Environment.ProcessorCount + 2;

        /// <summary>
        /// Store the channel listener
        /// </summary>
        private IChannelListener<T> listener;

        /// <summary>
        /// Store the async callback for accepting channel
        /// </summary>
        private AsyncCallback acceptChannel;

        /// <summary>
        /// Store the async callback for receiving reqeuests
        /// </summary>
        private AsyncCallback receiveRequest;

        /// <summary>
        /// Initializes a new instance of the RequestReplyFrontEnd class
        /// </summary>
        /// <param name="listenUri">uri the frontend listen to</param>
        /// <param name="binding">binding that the frontend uses</param>
        public RequestReplyFrontEnd(Uri listenUri, Binding binding, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, SharedData sharedData)
            : base(listenUri.AbsoluteUri, observer, clientManager, brokerAuth, sharedData)
        {
            this.listener = binding.BuildChannelListener<T>(listenUri);
            this.acceptChannel = new BasicCallbackReferencedThreadHelper<IAsyncResult>(this.AcceptChannel, this).CallbackRoot;
            this.receiveRequest = new BasicCallbackReferencedThreadHelper<IAsyncResult>(this.ReceiveRequest, this).CallbackRoot;
        }

        /// <summary>
        /// Open the frontend
        /// </summary>
        public override void Open()
        {
            try
            {
                this.listener.Open();
            }
            catch (CommunicationException ce)
            {
                BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[RequestReplyFrontEnd] Exception throwed while opening the listener: {0}", ce);
                this.listener.Abort();
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_OpenFrontEndFailed, SR.OpenFrontEndFailed, ce.Message);
            }

            // For session channel, we will try to accept multi channels
            // For nonsession channel, we will only accept one channel
            if (typeof(T) == typeof(IReplySessionChannel))
            {
                for (int i = 0; i < magicNumber; i++)
                {
                    this.listener.BeginAcceptChannel(this.acceptChannel, null);
                }
            }
            else
            {
                // T is IReplyChannel
                this.listener.BeginAcceptChannel(this.acceptChannel, null);
            }
        }

        /// <summary>
        /// Close the frontend
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            this.CloseAllChannel();

            try
            {
                this.listener.Close();
            }
            catch (Exception ce)
            {
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[RequestReplyFrontEnd] Exception throwed while closing the listener: {0}", ce);
                this.listener.Abort();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// AsyncCallback for BeginAcceptChannel
        /// </summary>
        /// <param name="ar">async result</param>
        private void AcceptChannel(IAsyncResult ar)
        {
            T channel;
            try
            {
                // Accept the channel
                channel = this.listener.EndAcceptChannel(ar);
            }
            catch (TimeoutException)
            {
                // The channel will timeout, At this time, try to accpet channel again
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[RequestReplyFrontEnd] Channel timedout");
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginAcceptChannel(this.listener, this.acceptChannel);
                }

                return;
            }
            catch (CommunicationException ce)
            {
                // Error while accepting channel
                // Also try to accept the cahnnel again
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[RequestReplyFrontEnd] Exception throwed while accepting channel: {0}", ce);
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginAcceptChannel(this.listener, this.acceptChannel);
                }

                return;
            }

            if (channel == null)
            {
                // Indicate that the listener should be closed
                // Close the listener
                lock (this.listener)
                {
                    if (this.listener.State == CommunicationState.Opened)
                    {
                        this.Close();
                    }
                }

                return;
            }

            if (typeof(T) == typeof(IReplySessionChannel))
            {
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginAcceptChannel(this.listener, this.acceptChannel);
                }
            }

            try
            {
                channel.Open();
            }
            catch (TimeoutException)
            {
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[RequestReplyFrontEnd] Open channel timed out");
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginAcceptChannel(this.listener, this.acceptChannel);
                }

                return;
            }
            catch (CommunicationException ce)
            {
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[RequestReplyFrontEnd] Exception throwed while opening channel: {0}", ce);
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginAcceptChannel(this.listener, this.acceptChannel);
                }

                return;
            }

            BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[RequestReplyFrontEnd] Channel Opened");

            // Try to receive messages
            for (int i = 0; i < magicNumber; i++)
            {
                this.TryToBeginReceiveMessagesWithThrottling(new ChannelClientState(channel, null));
            }
        }

        /// <summary>
        /// AsyncCallback for ReceiveRequest
        /// </summary>
        /// <param name="ar">async result</param>
        private void ReceiveRequest(IAsyncResult ar)
        {
            ChannelClientState state = (ChannelClientState)ar.AsyncState;
            T channel = (T)state.Channel;
            BrokerClient client = state.Client;
            RequestContext request;
            try
            {
                request = channel.EndReceiveRequest(ar);
            }
            catch (TimeoutException)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[RequestReplyFrontEnd] Receive Request timed out");
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginReceiveMessagesWithThrottling(state);
                }

                return;
            }
            catch (CommunicationException ce)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[RequestReplyFrontEnd] Exception while receiving requests: {0}", ce.Message);

                // Retry receiving requests
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginReceiveMessagesWithThrottling(state);
                }

                return;
            }

            #region Debug Failure Test
            SimulateFailure.FailOperation(1);
            #endregion

            // After channel timed out, the request will be null if you call channel.ReceiveRequest()
            // Need to end the channel at this time
            if (request == null)
            {
                // Indicate that the channel should be closed
                // Close the channel
                lock (channel)
                {
                    if (channel.State == CommunicationState.Opened)
                    {
                        try
                        {
                            channel.Close();
                        }
                        catch (CommunicationException ce)
                        {
                            BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[RequestReplyFrontEnd] Exception throwed while close the channel: {0}", ce);
                            channel.Abort();
                        }
                    }
                }

                return;
            }

            // Try to get the client id and the user name
            string callerSID;
            string userName = this.GetUserName(request.RequestMessage, out callerSID);
            string clientId = GetClientId(request.RequestMessage, callerSID);

            try
            {
                ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
                ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);
            }
            catch (ArgumentException)
            {
                BrokerTracing.EtwTrace.LogFrontEndRequestRejectedClientIdInvalid(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request.RequestMessage));
                RequestContextBase requestContextToReject = new RequestReplyRequestContext(request, this.Observer);
                requestContextToReject.BeginReply(FrontEndFaultMessage.GenerateFaultMessage(request.RequestMessage, request.RequestMessage.Headers.MessageVersion, SOAFaultCode.Broker_InvalidClientIdOrTooLong, SR.InvalidClientIdOrTooLong), this.ReplySentCallback, requestContextToReject);
                return;
            }

            if (client == null || client.State == BrokerClientState.Disconnected || String.Compare(clientId, client.ClientId, StringComparison.OrdinalIgnoreCase) != 0)
            {
                try
                {
                    client = this.ClientManager.GetClient(clientId, userName);
                    client.SingletonInstanceConnected();
                }
                catch (FaultException<SessionFault> e)
                {
                    if (e.Detail.Code == (int)SOAFaultCode.AccessDenied_BrokerQueue)
                    {
                        BrokerTracing.EtwTrace.LogFrontEndRequestRejectedAuthenticationError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request.RequestMessage), userName);
                        RequestContextBase requestContextToReject = new RequestReplyRequestContext(request, this.Observer);
                        requestContextToReject.BeginReply(FrontEndFaultMessage.GenerateFaultMessage(request.RequestMessage, request.RequestMessage.Headers.MessageVersion, SOAFaultCode.Broker_UserNameNotMatch, SR.UserNameNotMatch, userName, clientId), this.ReplySentCallback, requestContextToReject);
                        return;
                    }
                    else
                    {
                        BrokerTracing.EtwTrace.LogFrontEndRequestRejectedGeneralError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request.RequestMessage), e.ToString());
                        throw;
                    }
                }
                catch (BrokerQueueException e)
                {
                    BrokerTracing.EtwTrace.LogFrontEndRequestRejectedGeneralError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request.RequestMessage), e.ToString());
                    RequestContextBase requestContextToReject = new RequestReplyRequestContext(request, this.Observer);
                    requestContextToReject.BeginReply(FrontEndFaultMessage.TranslateBrokerQueueExceptionToFaultMessage(e, request.RequestMessage), this.ReplySentCallback, requestContextToReject);
                    return;
                }
                catch (Exception e)
                {
                    BrokerTracing.EtwTrace.LogFrontEndRequestRejectedGeneralError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request.RequestMessage), e.ToString());
                    throw;
                }

                state.Client = client;
            }

            // Receive new requests
            if (!ar.CompletedSynchronously)
            {
                this.TryToBeginReceiveMessagesWithThrottling(state);
            }

            // Try to get the user info header
            bool userDataFlag = GetUserInfoHeader(request.RequestMessage);

            // Create request context
            RequestContextBase requestContext;
            if (userDataFlag)
            {
                // Message that needs not to reply
                requestContext = DummyRequestContext.GetInstance(request.RequestMessage.Version);

                // Reply the message to the client immediately if user data is found for request/reply MEP (basic http binding)
                try
                {
                    request.BeginReply(Message.CreateMessage(request.RequestMessage.Headers.MessageVersion, request.RequestMessage.Headers.Action + "Response"), this.ReplySent, request);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceEvent(TraceEventType.Error, 0, "[RequestReplyFrontEnd] Exception throwed while trying to reply dummy message to client: {0}", e);
                }
            }
            else
            {
                requestContext = new RequestReplyRequestContext(request, this.Observer, client);
            }

            // Check auth
            if (!this.CheckAuth(requestContext, request.RequestMessage))
            {
                return;
            }

            // Bug 15195: Remove security header for https
            TryRemoveSecurityHeaderForHttps(request.RequestMessage);

            // Send request to the broker client
            client.RequestReceived(requestContext, request.RequestMessage, null).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Try to begin receive messages
        /// </summary>
        /// <param name="ccs">indicating the channel and the client</param>
        /// <returns>if the operation completed synchronously</returns>
        protected override bool TryToBeginReceive(ChannelClientState state)
        {
            T channel = (T)((ChannelClientState)state).Channel;
            IAsyncResult ar = null;

            try
            {
                ar = channel.BeginReceiveRequest(this.receiveRequest, state);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[RequestReplyFrontEnd] Exception throwed while begin receive messages: {0}", e);

                // Channel must be in falted state
                lock (channel)
                {
                    if (channel.State == CommunicationState.Faulted)
                    {
                        BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[RequestReplyFrontEnd] About the channel.");

                        // About the falted channel
                        channel.Abort();
                        return false;
                    }
                }
            }

            return ar.CompletedSynchronously && channel.State == CommunicationState.Opened;
        }

        /// <summary>
        /// Try to remove security header for https, this is for java sevice host
        /// </summary>
        /// <param name="message">indicating the message</param>
        private static void TryRemoveSecurityHeaderForHttps(Message message)
        {
            int index = message.Headers.FindHeader(Constant.SecurityHeaderName, Constant.SecurityHeaderNamespace);
            if (index >= 0)
            {
                message.Headers.RemoveAt(index);
            }
        }

        /// <summary>
        /// Call EndReply when reply sent
        /// </summary>
        /// <param name="ar">async result</param>
        private void ReplySent(IAsyncResult ar)
        {
            RequestContext context = (RequestContext)ar.AsyncState;
            try
            {
                context.EndReply(ar);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Error, 0, "[RequestReplyFrontEnd] Exception throwed while sending reply: {0}", e);
            }
        }
    }
}
