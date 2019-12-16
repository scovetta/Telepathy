// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.FrontEnd
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.Common.ThreadHelper;
    using Microsoft.Telepathy.ServiceBroker.FrontEnd.AzureQueue;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// The FrontEnd for using Azure Storage Queue
    /// </summary>
    internal class AzureQueueFrontEnd : FrontEndBase
    {
        /// <summary>
        /// Get the magic number
        /// </summary>
        private static readonly int magicNumber = Environment.ProcessorCount + 2;

        /// <summary>
        /// Store the async callback for receiving reqeuests
        /// </summary>
        private AsyncCallback receiveRequest;

        /// <summary>
        /// Store the Azure storage proxy
        /// </summary>
        private AzureQueueProxy azureQueueProxy;

        /// <summary>
        /// Initializes a new instance of the AzureQueueFrontEnd class
        /// </summary>
        /// <param name="listenUri">uri the frontend listen to</param>
        /// <param name="binding">binding that the frontend uses</param>
        public AzureQueueFrontEnd(AzureQueueProxy proxy, Uri listenUri, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, SharedData sharedData)
            : base(listenUri.AbsoluteUri, observer, clientManager, brokerAuth, sharedData)
        {
            this.azureQueueProxy = proxy;
            this.receiveRequest = new BasicCallbackReferencedThreadHelper<IAsyncResult>(this.ReceiveRequest, this).CallbackRoot;
        }

        /// <summary>
        /// Open the frontend
        /// </summary>
        public override void Open()
        {
            try
            {
                this.azureQueueProxy.Open();

                // Try to receive messages
                for (int i = 0; i < magicNumber; i++)
                {
                    this.TryToBeginReceiveMessagesWithThrottling(new ChannelClientState(null, null));
                }

            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[AzureQueueFrontEnd] Exception throwed while opening the azure queue proxy : {0}", e);
                throw;
            }
        }

        /// <summary>
        /// Close the frontend
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            this.CloseAllChannel();

            this.azureQueueProxy.Close();

            base.Dispose(disposing);
        }

        /// <summary>
        /// AsyncCallback for ReceiveRequest
        /// </summary>
        /// <param name="ar">async result</param>
        private void ReceiveRequest(IAsyncResult ar)
        {
            ChannelClientState state = (ChannelClientState)ar.AsyncState;
            BrokerClient client = state.Client;
            Message request;
            try
            {
                request = this.azureQueueProxy.EndReceiveRequest(ar);
            }
            catch (TimeoutException)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[AzureQueueFrontEnd] Receive Request timed out");
                if (!ar.CompletedSynchronously)
                {
                    this.TryToBeginReceiveMessagesWithThrottling(state);
                }

                return;
            }
            catch (CommunicationException ce)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[AzureQueueFrontEnd] Exception while receiving requests: {0}", ce.Message);

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


            // Try to get the client id and the user name
            string userName = GetUserName(request);
            string clientId = GetClientId(request, string.Empty);

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

            if (client == null || client.State == BrokerClientState.Disconnected || !client.ClientId.Equals(clientId, StringComparison.InvariantCultureIgnoreCase))
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

            // Bug 15195: Remove security header for https
            TryRemoveSecurityHeaderForHttps(request);
                        
            // Send request to the broker client
            client.RequestReceived(requestContext, request, null).GetAwaiter().GetResult();
        }

        
        /// <summary>
        /// Try to begin receive messages
        /// </summary>
        /// <param name="ccs">indicating the channel and the client</param>
        /// <returns>if the operation completed synchronously</returns>
        protected override bool TryToBeginReceive(ChannelClientState state)
        {
            // IAsyncResult ar = new AsyncResult(state);
            IAsyncResult ar = null;
            try
            {
                ar = this.azureQueueProxy.BeginReceiveRequest(this.receiveRequest, state);

            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[AzureQueueFrontEnd] Exception throwed while begin receive messages: {0}", e);
            }

            return ar.CompletedSynchronously;
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


        private class AsyncResult : IAsyncResult
        {
            /// <summary>
            /// The async state
            /// </summary>
            private object asyncState;

            /// <summary>
            /// The operation is completed.
            /// </summary>
            private bool completed;

            /// <summary>
            /// The operation is complete synchoronously
            /// </summary>
            private bool completedSync;

            /// <summary>
            /// the event to wait for the operation finish
            /// </summary>
            private ManualResetEvent evt = new ManualResetEvent(false);


            public AsyncResult(object asyncState)
            {
                this.asyncState = asyncState;
                this.completed = true;
                this.completedSync = false;
            }

            public object AsyncState
            {
                get { return this.asyncState; }
            }

            public System.Threading.WaitHandle AsyncWaitHandle
            {
                get { return this.evt; }
            }

            public bool CompletedSynchronously
            {
                get { return this.completedSync; }
            }

            public bool IsCompleted
            {
                get { return this.completed; }
            }
        }

    }
}
