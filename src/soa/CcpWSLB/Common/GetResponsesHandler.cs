//------------------------------------------------------------------------------
// <copyright file="GetResponsesHandler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Handler for get responses
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.ServiceBroker.Common;
    using Microsoft.Hpc.ServiceBroker.FrontEnd;
    using Microsoft.Hpc.SvcBroker;

    using SR = Microsoft.Hpc.SvcBroker.SR;

    /// <summary>
    /// Handler for get responses
    /// </summary>
    internal class GetResponsesHandler : BaseResponsesHandler
    {
        /// <summary>
        /// the latest response service callback.
        /// </summary>
        private IResponseServiceCallback lastResponseServiceCallback;

        /// <summary>
        /// Stores the client id
        /// </summary>
        private string clientId;

        /// <summary>
        /// Stores the client data
        /// </summary>
        private string clientData;

        /// <summary>
        /// Stores a value indicating the handler has succeeded sent back the end of responses
        /// </summary>
        private bool eorReplied;

        /// <summary>
        /// Stores a value indicating whether the callback channel is disposed
        /// </summary>
        private bool callbackChannelDisposed;

        /// <summary>
        /// Stores a value indicating whether the GetResponseHandler
        /// should cache the broker queue item instead of ACK them immediately
        /// after sent them to the client
        /// </summary>
        /// <remarks>
        /// GetResponseHandler would only cache broker queue items sent back to
        /// client within this list when it is REST service as the client.
        /// It would ACK responses when the next GetResponse come in.
        /// </remarks>
        private bool cacheBrokerQueueItem;

        /// <summary>
        /// Stores the list of cached items
        /// </summary>
        private List<BrokerQueueItem> cachedItemList = new List<BrokerQueueItem>();

        /// <summary>
        /// Lock object for cachedItemList
        /// </summary>
        private object lockCacheItemList = new object();

        /// <summary>
        /// Initializes a new instance of the GetResponsesHandler class
        /// </summary>
        /// <param name="queue">indicating the broker queue</param>
        /// <param name="action">indicating the action</param>
        /// <param name="clientData">indicating the client data</param>
        /// <param name="clientId">indicating the client</param>
        /// <param name="timeoutManager">indicating the timeout manager</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="version">indicating the message version</param>
        public GetResponsesHandler(BrokerQueue queue, string action, string clientData, string clientId, TimeoutManager timeoutManager, BrokerObserver observer, SharedData sharedData, MessageVersion version)
            : base(queue, action, timeoutManager, observer, sharedData, version)
        {
            this.clientId = clientId;
            this.clientData = clientData;

            // For now, as REST service is the only frontend on Azure. Broker can know if it is
            // REST service connecting by checking if broker is running on Azure.
            // Need to fix this logic if we reopen net.tcp frontend on Azure.
            if (SoaHelper.IsOnAzure())
            {
                this.cacheBrokerQueueItem = true;
            }

#if DEBUG
            // For debug purpose, if the incoming message header contains a header which
            // indicates it is REST service calling, also set the flag to true
            // This is to enable on-premise test.
            if (OperationContext.Current != null && OperationContext.Current.IncomingMessageHeaders.FindHeader("HpcSOAWebSvc", Constant.HpcHeaderNS) >= 0)
            {
                this.cacheBrokerQueueItem = true;
            }
#endif
        }

        /// <summary>
        /// Check if the action and client data matches this handler
        /// </summary>
        /// <param name="action">indicating the action</param>
        /// <param name="clientData">indicating the client data</param>
        /// <returns>whether the action and client data matches the handler</returns>
        public bool Matches(string action, string clientData)
        {
            if (this.callbackChannelDisposed)
            {
                return false;
            }

            // If action is empty, return all responses regardless of action
            return (String.IsNullOrEmpty(this.Action) || action == this.Action) && clientData == this.clientData;
        }

        /// <summary>
        /// Get more responses
        /// </summary>
        /// <param name="position">indicating the position</param>
        /// <param name="count">indicating the count</param>
        /// <param name="callbackInstance">indicating the callback instance</param>
        public void GetResponses(GetResponsePosition position, int count, IResponseServiceCallback callbackInstance)
        {
            if (position == GetResponsePosition.Begin)
            {
                this.ResponsesCount = 0;
                if (this.IsSessionFailed())
                {
                    IResponseServiceCallback callback = callbackInstance;
                    this.ReplyFaultMessage(callback, FrontEndFaultMessage.GenerateFaultMessage(null, this.Version, SOAFaultCode.Broker_SessionFailure, SR.SessionFailure), this.clientData);
                    this.ReplyEndOfMessage(callback, this.clientData);
                    return;
                }

                if (this.cacheBrokerQueueItem)
                {
                    // ACK the items as they were failed to send back to client
                    lock (this.lockCacheItemList)
                    {
                        this.Queue.AckResponses(this.cachedItemList, false);
                        this.cachedItemList = new List<BrokerQueueItem>();
                    }
                }

                this.Queue.ResetResponsesCallback();
            }
            else
            {
                if (this.cacheBrokerQueueItem)
                {
                    // ACK the items as they were succeeded to send back to client
                    lock (this.lockCacheItemList)
                    {
                        this.Queue.AckResponses(this.cachedItemList, true);
                        this.cachedItemList = new List<BrokerQueueItem>();
                    }
                }
            }

            ResponseActionFilter filter = GenerateResponseActionFilter(this.Action);

            this.lastResponseServiceCallback = callbackInstance;
            if (!this.Queue.RegisterResponsesCallback(this.ReceiveResponse, this.Version, filter, count, new object[] { this.lastResponseServiceCallback, this.clientData }))
            {
                this.ReplyEndOfMessage(this.lastResponseServiceCallback, this.clientData);
            }
        }

        /// <summary>
        /// End of responses received
        /// </summary>
        /// <param name="eventId">indicating the event id</param>
        /// <param name="eventArgs">indicating the event args</param>
        public override void EndOfResponses(BrokerQueueEventId eventId, ResponseEventArgs eventArgs)
        {
            IResponseServiceCallback callback = (IResponseServiceCallback)(eventArgs.State as object[])[0];
            string clientData = (eventArgs.State as object[])[1].ToString();

            // if the broker fails and the last available response received, then append the session failure fault message let the client API to handle the failure gracefully.
            if (eventId == BrokerQueueEventId.AvailableResponsesDispatched)
            {
                this.ReplyFaultMessage(callback, FrontEndFaultMessage.GenerateFaultMessage(null, this.Version, SOAFaultCode.Broker_SessionFailure, SR.SessionFailure), clientData);
            }

            this.ReplyEndOfMessage(callback, clientData);
        }

        /// <summary>
        /// the event when the session failed.
        /// </summary>
        public override void SessionFailed()
        {
            if (this.Queue.ProcessedRequestsCount <= 0)
            {
                this.ReplyFaultMessage(this.lastResponseServiceCallback, FrontEndFaultMessage.GenerateFaultMessage(null, this.Version, SOAFaultCode.Broker_SessionFailure, SR.SessionFailure), this.clientData);
                this.ReplyEndOfMessage(this.lastResponseServiceCallback, this.clientData);
            }
        }

        /// <summary>
        /// Infomrs that the client is disconnected
        /// </summary>
        public override void ClientDisconnect(bool purged)
        {
            if (!this.eorReplied)
            {
                this.ReplyEndOfMessage(this.lastResponseServiceCallback, this.clientData, purged ? EndOfResponsesReason.ClientPurged : EndOfResponsesReason.ClientTimeout);
            }
        }

        /// <summary>
        /// Dispose the GetResponseHandler instance
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void DisposeInternal()
        {
            base.DisposeInternal();

            if (this.cacheBrokerQueueItem)
            {
                // Need to ack responses as they were successfully sent back
                try
                {
                    lock (this.lockCacheItemList)
                    {
                        this.Queue.AckResponses(this.cachedItemList, true);
                    }
                }
                catch (Exception ex)
                {
                    // Ignore exception as broker queue might be already disposed at this stage
                    BrokerTracing.TraceWarning("[GetResponsesHandler].DisposeInternal: Exception {0}", ex);
                }

                this.cachedItemList = null;
            }
        }

        /// <summary>
        /// Reply the fault message.
        /// </summary>
        /// <param name="callback">the response service callback.</param>
        /// <param name="faultMessage">indicating the fault message</param>
        /// <param name="clientData">the client data.</param>
        private void ReplyFaultMessage(IResponseServiceCallback callback, Message faultMessage, string clientData)
        {
            if (this.callbackChannelDisposed)
            {
                return;
            }

            this.ResetTimeout();

            try
            {
                faultMessage.Headers.Add(MessageHeader.CreateHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS, clientData));
                if (callback is AzureQueueProxy)
                {
                    callback.SendResponse(faultMessage, clientData);
                }
                else
                {
                    callback.SendResponse(faultMessage);
                }
                this.IncreaseResponsesCount();
            }
            catch (ObjectDisposedException)
            {
                this.callbackChannelDisposed = true;
                this.Queue.ResetResponsesCallback();
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerClient] Client {0}: Failed to send fault message, Exception, {1}", this.clientId, e);
            }
        }

        /// <summary>
        /// reply the end of message to the client.
        /// </summary>
        /// <param name="callback">the response service callback.</param>
        /// <param name="clientData">the client data.</param>
        private void ReplyEndOfMessage(IResponseServiceCallback callback, string clientData)
        {
            this.ReplyEndOfMessage(callback, clientData, EndOfResponsesReason.Success);
        }

        /// <summary>
        /// reply the end of message to the client.
        /// </summary>
        /// <param name="callback">the response service callback.</param>
        /// <param name="clientData">the client data.</param>
        /// <param name="clientPurged">indicating the client purged flag</param>
        private void ReplyEndOfMessage(IResponseServiceCallback callback, string clientData, EndOfResponsesReason reason)
        {
            if (this.callbackChannelDisposed)
            {
                return;
            }

            BrokerTracing.TraceInfo("[GetResponsesHandler] Client {0}: Send end of response, clientPurged = {1}", this.clientId, reason);
            this.ResetTimeout();
            TypedMessageConverter converter = TypedMessageConverter.Create(typeof(EndOfResponses), Constant.EndOfMessageAction);
            EndOfResponses endOfResponses = new EndOfResponses();
            endOfResponses.Count = this.ResponsesCount;
            endOfResponses.Reason = reason;
            Message eom = converter.ToMessage(endOfResponses, this.Version);
            eom.Headers.Add(MessageHeader.CreateHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS, clientData));
            this.eorReplied = true;
            try
            {
                if (callback is AzureQueueProxy)
                {
                    callback.SendResponse(eom, clientData);
                }
                else
                {
                    callback.SendResponse(eom);
                }
            }
            catch (ObjectDisposedException)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerClient] Client {0}: Send end of response error: communication object is disposed.", this.clientId);
                this.callbackChannelDisposed = true;
                this.Queue.ResetResponsesCallback();
            }
            catch (Exception ce)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerClient] Client {0}: Send end of response error: {1}", this.clientId, ce);

                // Swallow exception
            }
        }

        /// <summary>
        /// Receive response message
        /// </summary>
        /// <param name="item">broker queue item</param>
        /// <param name="asyncState">async state</param>
        private void ReceiveResponse(BrokerQueueItem item, object asyncState)
        {
            if (this.callbackChannelDisposed)
            {
                throw new Exception("Callback channel was disposed");
            }

            this.ResetTimeout();
            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "[BrokerClient] Client {0}: Receive Response from BrokerQueue", this.clientId);
            object[] objArray = asyncState as object[];
            IResponseServiceCallback callback = (IResponseServiceCallback)objArray[0];
            string clientData = objArray[1].ToString();
            Message response = this.ConvertMessage(item.Message);
            int index = response.Headers.FindHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS);
            if (index < 0)
            {
                response.Headers.Add(MessageHeader.CreateHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS, clientData));
            }

            Exception exception = null;
            try
            {
                if (callback is AzureQueueProxy)
                {
                    callback.SendResponse(response, clientData);
                }
                else
                {
                    callback.SendResponse(response);
                }
                BrokerTracing.EtwTrace.LogFrontEndResponseSent(this.SharedData.BrokerInfo.SessionId, this.clientId, Utility.GetMessageIdFromResponse(response));
                this.IncreaseResponsesCount();
            }
            catch (ObjectDisposedException e)
            {
                this.callbackChannelDisposed = true;
                exception = new Exception("Callback channel is disposed", e);
                throw exception;
            }
            catch (CommunicationObjectFaultedException e)
            {
                this.callbackChannelDisposed = true;
                exception = new Exception("Callback channel is faulted", e);
                throw exception;
            }
            catch (CommunicationObjectAbortedException e)
            {
                this.callbackChannelDisposed = true;
                exception = new Exception("Callback channel is abroted", e);
                throw exception;
            }
            catch (Exception ce)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerClient] Client {0}: Send back response error: {1}", this.clientId, ce);

                // Reply a fault message indicating failed to send back the response and reply EOR and finish.
                this.ReplyFaultMessage(callback, FrontEndFaultMessage.GenerateFaultMessage(null, MessageVersion.Default, SOAFaultCode.Broker_SendBackResponseFailed, SR.SendBackResponseFailed), clientData);
                this.ReplyEndOfMessage(callback, clientData);
            }
            finally
            {
                // We do not need to lock here because this callback is designed to be
                // triggered by BrokerQueue synchronizely
                if (this.cacheBrokerQueueItem)
                {
                    lock (this.lockCacheItemList)
                    {
                        this.cachedItemList.Add(item);
                    }
                }
                else
                {
                    this.Queue.AckResponse(item, (exception == null));
                }

                if (this.callbackChannelDisposed)
                {
                    this.Queue.ResetResponsesCallback();
                }
            }
        }
    }
}
