// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.FrontEnd
{
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    using Microsoft.Telepathy.Common.Registry;
    using Microsoft.Telepathy.Common.TelepathyContext.Extensions.RegistryExtension;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.Common.ThreadHelper;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// The FrontEnd for duplex MEP (works for NetTcpBinding)
    /// </summary>
    internal class DuplexFrontEnd : FrontEndBase
    {
        /// <summary>
        /// Get the magic number
        /// </summary>
        private static readonly int magicNumber = Environment.ProcessorCount + 2;

        /// <summary>
        /// Store the channel listener
        /// </summary>
        private IChannelListener<IDuplexSessionChannel> listener;

        /// <summary>
        /// Store the async callback for accepting channel
        /// </summary>
        private AsyncCallback acceptChannel;

        /// <summary>
        /// Store the async callback for receiving reqeuests
        /// </summary>
        private AsyncCallback receiveRequest;

        /// <summary>
        /// Store the binding information
        /// </summary>
        private Binding binding;

        /// <summary>
        /// Initializes a new instance of the DuplexFrontEnd class
        /// </summary>
        /// <param name="listenUri">uri the frontend listen to</param>
        /// <param name="binding">binding that the frontend uses</param>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="brokerAuth">indicating the broker authorization</param>
        /// <param name="sharedData">indicating the shared data</param>
        public DuplexFrontEnd(Uri listenUri, Binding binding, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, SharedData sharedData)
            : base(listenUri.AbsoluteUri, observer, clientManager, brokerAuth, sharedData)
        {
            this.binding = binding;
            if (sharedData.StartInfo.UseAad)
            {
                BindingParameterCollection bindingParms = new BindingParameterCollection();
                var serviceCred = new ServiceCredentials();
                serviceCred.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, new NonHARegistry().GetSSLThumbprint().GetAwaiter().GetResult());

                bindingParms.Add(serviceCred);

                this.listener = binding.BuildChannelListener<IDuplexSessionChannel>(listenUri, bindingParms);
            }
            else if (sharedData.StartInfo.LocalUser.GetValueOrDefault())
            {
                this.listener = binding.BuildChannelListener<IDuplexSessionChannel>(listenUri, new ServiceCredentials().UseInternalAuthenticationAsync(true).GetAwaiter().GetResult());
            }
            else
            {
                this.listener = binding.BuildChannelListener<IDuplexSessionChannel>(listenUri);
            }

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
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[DuplexFrontEnd] Exception throwed while opening the listener: {0}", e);
                this.listener.Abort();
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_OpenFrontEndFailed, SR.OpenFrontEndFailed, e.Message);
            }

            for (int i = 0; i < magicNumber; i++)
            {
                this.listener.BeginAcceptChannel(this.acceptChannel, null);
            }
        }

        /// <summary>
        /// Close the frontend
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            this.CloseAllChannel();

            try
            {
                this.listener.Close();
            }
            catch (Exception ce)
            {
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[DuplexFrontEnd] Exception throwed while closing the listener: {0}", ce);
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
            IDuplexSessionChannel channel;
            try
            {
                // Accept the channel
                channel = this.listener.EndAcceptChannel(ar);
            }
            catch (TimeoutException)
            {
                // The channel will timeout. At this time, try to accpet channel again
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[DuplexFrontEnd] Channel timedout");

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
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[DuplexFrontEnd] Exception throwed while accepting channel: {0}", ce);
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

            if (!ar.CompletedSynchronously)
            {
                this.TryToBeginAcceptChannel(this.listener, this.acceptChannel);
            }

            try
            {
                channel.Open();
            }
            catch (TimeoutException)
            {
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[DuplexFrontEnd] Open channel timed out");
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginAcceptChannel(this.listener, this.acceptChannel);
                }

                return;
            }
            catch (CommunicationException ce)
            {
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[DuplexFrontEnd] Exception throwed while opening channel: {0}", ce);
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginAcceptChannel(this.listener, this.acceptChannel);
                }

                return;
            }

            BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[DuplexFrontEnd] Channel Opened");

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
            IDuplexSessionChannel channel = (IDuplexSessionChannel)state.Channel;
            BrokerClient client = state.Client;
            Message requestMessage;
            try
            {
                requestMessage = channel.EndReceive(ar);
            }
            catch (Exception ce)
            {
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[DuplexFrontEnd] Exception while receiving requests: {0}", ce);
                this.FrontendDisconnect(channel, client);

                lock (channel)
                {
                    if (channel.State == CommunicationState.Faulted)
                    {
                        BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[DuplexFrontEnd] Abort faulted channel.");
                        channel.Abort();
                        return;
                    }
                }

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

            // After channel timeout, the request will be null if you call channel.Receive()
            // Need to end the channel at this time
            if (requestMessage == null)
            {
                // Indicate that the channel should be closed
                // Close the channel
                lock (channel)
                {
                    if (channel.State == CommunicationState.Opened)
                    {
                        this.FrontendDisconnect(channel, client);
                        try
                        {
                            channel.Close();
                            BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[DuplexFrontEnd] Channel closed");
                        }
                        catch (Exception ce)
                        {
                            BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[DuplexFrontEnd] Exception throwed while close the channel: {0}", ce);
                            channel.Abort();
                        }
                    }
                }

                return;
            }

            // Try to get the client id
            string callerSID;
            string userName = this.GetUserName(requestMessage, out callerSID);
            string clientId = GetClientId(requestMessage, callerSID);

            try
            {
                ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
                ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);
            }
            catch (ArgumentException)
            {
                BrokerTracing.EtwTrace.LogFrontEndRequestRejectedClientIdInvalid(this.SessionId, clientId, Utility.GetMessageIdFromMessage(requestMessage));
                RequestContextBase requestContextToReject = new DuplexRequestContext(requestMessage, this.binding, channel, this.Observer);
                requestContextToReject.BeginReply(FrontEndFaultMessage.GenerateFaultMessage(requestMessage, requestMessage.Headers.MessageVersion, SOAFaultCode.Broker_InvalidClientIdOrTooLong, SR.InvalidClientIdOrTooLong), this.ReplySentCallback, requestContextToReject);
                return;
            }

            if (client == null)
            {
                if (!this.TryGetClientByChannel(channel, clientId, userName, requestMessage, out client))
                {
                    return;
                }

                state.Client = client;
            }

            // Receive new requests
            if (!ar.CompletedSynchronously)
            {
                this.TryToBeginReceiveMessagesWithThrottling(state);
            }

            // Reject if client id does not match
            if (String.Compare(clientId, client.ClientId, StringComparison.OrdinalIgnoreCase) != 0)
            {
                BrokerTracing.EtwTrace.LogFrontEndRequestRejectedClientIdNotMatch(this.SessionId, clientId, Utility.GetMessageIdFromMessage(requestMessage));
                RequestContextBase requestContextToReject = new DuplexRequestContext(requestMessage, this.binding, channel, this.Observer);
                requestContextToReject.BeginReply(FrontEndFaultMessage.GenerateFaultMessage(requestMessage, requestMessage.Headers.MessageVersion, SOAFaultCode.Broker_ClientIdNotMatch, SR.ClientIdNotMatch), this.ReplySentCallback, requestContextToReject);
                return;
            }

            // Try to get the user info header
            bool userDataFlag = GetUserInfoHeader(requestMessage);

            // Create request context
            RequestContextBase requestContext;
            if (userDataFlag)
            {
                // Message that needs not to reply
                requestContext = DummyRequestContext.GetInstance(requestMessage.Version);
            }
            else
            {
                requestContext = new DuplexRequestContext(requestMessage, this.binding, channel, this.Observer, client);
            }

            // Check auth
            if (!this.CheckAuth(requestContext, requestMessage))
            {
                return;
            }

            // remove security header for websocket
            TryRemoveSecurityHeaderForHttps(requestMessage);

            // Send the request to the broker client
            client.RequestReceived(requestContext, requestMessage, null);
        }

        /// <summary>
        /// Try to get client by channel
        /// If authentication failed, fault message is replied and false is returned
        /// </summary>
        /// <param name="channel">indicating the channel</param>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="userName">indicating the user name</param>
        /// <param name="request">indicating the request</param>
        /// <param name="client">output the client</param>
        /// <returns>returns whether succeeded get client</returns>
        private bool TryGetClientByChannel(IDuplexSessionChannel channel, string clientId, string userName, Message request, out BrokerClient client)
        {
            try
            {
                client = this.GetClientByChannel(channel, clientId, userName);
                return true;
            }
            catch (FaultException<SessionFault> e)
            {
                if (e.Detail.Code == (int)SOAFaultCode.AccessDenied_BrokerQueue)
                {
                    BrokerTracing.EtwTrace.LogFrontEndRequestRejectedAuthenticationError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request), userName);
                    RequestContextBase requestContextToReject = new DuplexRequestContext(request, this.binding, channel, this.Observer);
                    requestContextToReject.BeginReply(FrontEndFaultMessage.GenerateFaultMessage(request, request.Headers.MessageVersion, SOAFaultCode.Broker_UserNameNotMatch, SR.UserNameNotMatch, userName, clientId), this.ReplySentCallback, requestContextToReject);
                    client = null;
                    return false;
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
                RequestContextBase requestContextToReject = new DuplexRequestContext(request, this.binding, channel, this.Observer);
                requestContextToReject.BeginReply(FrontEndFaultMessage.TranslateBrokerQueueExceptionToFaultMessage(e, request), this.ReplySentCallback, requestContextToReject);
                client = null;
                return false;
            }
            catch (Exception e)
            {
                BrokerTracing.EtwTrace.LogFrontEndRequestRejectedGeneralError(this.SessionId, clientId, Utility.GetMessageIdFromMessage(request), e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Try to begin receive messages
        /// </summary>
        /// <param name="ccs">indicating the channel and the client</param>
        /// <returns>if the operation completed synchronously</returns>
        protected override bool TryToBeginReceive(ChannelClientState ccs)
        {
            IDuplexSessionChannel channel = (IDuplexSessionChannel)ccs.Channel;
            BrokerClient client = ccs.Client;
            IAsyncResult ar = null;

            try
            {
                ar = channel.BeginReceive(this.receiveRequest, ccs);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[DuplexFrontEnd] Exception throwed while begin receive messages: {0}", e);

                // Channel must be in falted state
                lock (channel)
                {
                    if (channel.State == CommunicationState.Faulted)
                    {
                        BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[DuplexFrontEnd] About the channel.");
                        this.FrontendDisconnect(channel, client);

                        // About the falted channel
                        channel.Abort();
                    }
                }

                return false;
            }

            return ar.CompletedSynchronously && channel.State == CommunicationState.Opened;
        }

        /// <summary>
        /// Indicating the client that frontend disconnected
        /// </summary>
        /// <param name="channel">indicating the channel</param>
        /// <param name="client">indicating the client</param>
        private void FrontendDisconnect(IDuplexSessionChannel channel, BrokerClient client)
        {
            if (client == null)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[DuplexFrontEnd] FrontendDisconnect client == null");
                client = this.GetClientByChannel(channel, null, String.Empty);
            }

            if (client != null)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[DuplexFrontEnd] FrontendDisconnect client != null");
                client.FrontendDisconnected(channel);
            }
        }

        /// <summary>
        /// Try to remove security header for https, this is for websocket
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
