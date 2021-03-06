﻿//-----------------------------------------------------------------------
// <copyright file="RequestReplyFrontEnd.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>FrontEnd for request/reply MEP</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.ServiceBroker.Common;
    using Microsoft.Hpc.SvcBroker;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// The FrontEnd for input/output MEP (works for customized binding)
    /// </summary>
    /// <typeparam name="T">indicate the input channel type</typeparam>
    internal class OutputInputFrontEnd<T> : FrontEndBase
        where T : class, IInputChannel
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
        public OutputInputFrontEnd(Uri listenUri, Binding binding, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, SharedData sharedData)
            : base(listenUri.AbsoluteUri, observer, clientManager, brokerAuth, sharedData)
        {

            List<BindingElement> elements = new List<BindingElement>();
            elements.Add(new OneWayBindingElement());
            elements.AddRange(binding.CreateBindingElements());
            CustomBinding shapedBinding = new CustomBinding(elements);

            this.listener = shapedBinding.BuildChannelListener<T>(listenUri);
            this.acceptChannel = new ReferencedThreadHelper<IAsyncResult>(new AsyncCallback(this.AcceptChannel), this).CallbackRoot;
            this.receiveRequest = new ReferencedThreadHelper<IAsyncResult>(new AsyncCallback(this.ReceiveRequest), this).CallbackRoot;
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
            if (typeof(T) == typeof(IReplySessionChannel) || typeof(T) == typeof(IInputChannel))
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

            if (typeof(T) == typeof(IReplySessionChannel) || typeof(T) == typeof(IInputChannel))
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
            Message request;
            try
            {
                request = channel.EndReceive(ar);
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
            Microsoft.Hpc.ServiceBroker.SimulateFailure.FailOperation(1);
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
            string userName = GetUserName(request, out callerSID);
            string clientId = GetClientId(request, callerSID);

            try
            {
                ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
                ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);
            }
            catch (ArgumentException)
            {
                BrokerTracing.EtwTrace.LogFrontEndRequestRejectedClientIdInvalid(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request));
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
                        BrokerTracing.EtwTrace.LogFrontEndRequestRejectedAuthenticationError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request), userName);
                        return;
                    }
                    else
                    {
                        BrokerTracing.EtwTrace.LogFrontEndRequestRejectedGeneralError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request), e.ToString());
                        throw;
                    }
                }
                catch (BrokerQueueException e)
                {
                    BrokerTracing.EtwTrace.LogFrontEndRequestRejectedGeneralError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request), e.ToString());
                    return;
                }
                catch (Exception e)
                {
                    BrokerTracing.EtwTrace.LogFrontEndRequestRejectedGeneralError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request), e.ToString());
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
            bool userDataFlag = GetUserInfoHeader(request);

            // Create request context
            RequestContextBase requestContext;
            
            //Message that needs not to reply
            requestContext = DummyRequestContext.GetInstance(request.Version);

            // Check auth
            if (!this.CheckAuth(request))
            {
                return;
            }

            // Bug 15195: Remove security header for https
            TryRemoveSecurityHeaderForHttps(request);

            // Send request to the broker client
            client.RequestReceived(requestContext, request, null);
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
                ar = channel.BeginReceive(this.receiveRequest, state);
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

    }
}
