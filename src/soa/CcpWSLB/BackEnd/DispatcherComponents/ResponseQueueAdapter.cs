// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.FrontEnd;
    using Microsoft.Telepathy.Session;

    /// <summary>
    /// Response queue adapter which is in charge of putting response
    /// back into broker queue
    /// </summary>
    internal class ResponseQueueAdapter
    {
        /// <summary>
        /// Stores the instance of broker observer
        /// </summary>
        private IBrokerObserver observer;

        /// <summary>
        /// Stores the instance of broker queue factory
        /// </summary>
        private IBrokerQueueFactory queueFactory;

        /// <summary>
        /// Stores the callback to reply response
        /// </summary>
        private AsyncCallback replyResponseCallback;

        /// <summary>
        /// The service request prefetch count.
        /// </summary>
        private int serviceRequestPrefetchCount;

        /// <summary>
        /// Initializes a new instance of the RequestQueueAdapter class
        /// </summary>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="factory">indicating the broker queue factory</param>
        public ResponseQueueAdapter(IBrokerObserver observer, IBrokerQueueFactory factory, int serviceRequestPrefetchCount)
        {
            this.observer = observer;
            this.queueFactory = factory;
            this.replyResponseCallback = new ThreadHelper<IAsyncResult>(new AsyncCallback(this.ReplyResponse)).CallbackRoot;
            this.serviceRequestPrefetchCount = serviceRequestPrefetchCount;
        }

        /// <summary>
        /// Put response back into broker queue
        /// </summary>
        /// <param name="data">indicating the instance of dispatch data</param>
        public async Task PutResponseBack(DispatchData data)
        {
            Contract.Requires(data.BrokerQueueItem != null);

            // Either the exception is null or it is a FaultException<RetryOperationError>;
            var faultException = data.Exception as FaultException<RetryOperationError>;
            Contract.Requires(data.Exception == null || faultException != null);

            // Either we will get a reply message or an exception.
            Contract.Requires(data.ReplyMessage != null || data.Exception != null);

            Contract.Requires(data.DispatchTime != null);

            Contract.Ensures(data.ReplyMessage == null);
            Contract.Ensures(data.BrokerQueueItem == null);
            Contract.Ensures(data.Exception == null);

            // Indicate that a call has completed
            try
            {
                long callDuration = DateTime.Now.Subtract(data.DispatchTime).Ticks / 10000 / (this.serviceRequestPrefetchCount + 1);
                this.observer.CallComplete(callDuration);
            }
            catch (Exception e)
            {
                // There may be some null reference exception while cleaning up
                // Ignore these exceptions
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[ResponseQueueAdapter] ID = {0} Exception throwed while updating the broker observer: {1}", data.TaskId, e);
            }

            var item = data.BrokerQueueItem;
            if (item.Context is DummyRequestContext)
            {
                Message reply = null;
                if (data.Exception != null)
                {
                    if (faultException != null)
                    {
                        reply = Message.CreateMessage(MessageVersion.Default, faultException.CreateMessageFault(), faultException.Action);

                        // Only add relatesTo header to WSAddressing messages
                        if (item.Message.Headers.MessageId != null && MessageVersion.Default.Addressing == AddressingVersion.WSAddressing10)
                        {
                            reply.Headers.RelatesTo = item.Message.Headers.MessageId;
                        }

                        BrokerTracing.EtwTrace.LogBackendGeneratedFaultReply(data.SessionId, data.TaskId, data.MessageId, reply.ToString());
                    }
                    else
                    {
                        Debug.Fail("The exception in DispatchData should be an instance of FaultException<RetryOperationError>");
                    }
                }
                else if (data.ReplyMessage != null)
                {
                    reply = data.ReplyMessage;

                    // Put the response to the broker queue
                    BrokerTracing.EtwTrace.LogBackendResponseStored(data.SessionId, data.TaskId, data.MessageId, reply.IsFault);
                }
                else
                {
                    Debug.Fail("Both ReplyMessage and Exception are null. This shouldn't happen.");
                }

                await this.queueFactory.PutResponseAsync(reply, item);
            }
            else
            {
                Message reply = null;
                if (data.Exception != null)
                {
                    if (faultException != null)
                    {
                        reply = FrontEndFaultMessage.GenerateFaultMessage(item.Message, item.Context.MessageVersion, faultException);
                        BrokerTracing.EtwTrace.LogBackendGeneratedFaultReply(data.SessionId, data.TaskId, data.MessageId, reply.ToString());
                    }
                    else
                    {
                        Debug.Fail("The exception in DispatchData should be an instance of FaultException<RetryOperationError>");
                    }
                }
                else if (data.ReplyMessage != null)
                {
                    reply = data.ReplyMessage;

                    // Reply the message
                    // Convert the message version
                    if (reply.Version != item.Context.MessageVersion)
                    {
                        if (reply.IsFault)
                        {
                            MessageFault fault = MessageFault.CreateFault(reply, int.MaxValue);
                            reply = Message.CreateMessage(item.Context.MessageVersion, fault, reply.Headers.Action);
                        }
                        else
                        {
                            reply = Message.CreateMessage(item.Context.MessageVersion, reply.Headers.Action, reply.GetReaderAtBodyContents());
                        }
                    }
                }
                else
                {
                    Debug.Fail("Both ReplyMessage and Exception are null. This shouldn't happen.");
                }

                this.TryReplyMessage(data.SessionId, item.Context, reply, data.MessageId);

                // dispose request item
                item.Dispose();
            }

            // Set to null since we returned it back to queue
            data.BrokerQueueItem = null;
            data.ReplyMessage = null;
            data.Exception = null;
        }

        /// <summary>
        /// Safe get IsFault property from a message instance
        /// </summary>
        /// <param name="message">indicating the message instance</param>
        /// <returns>returns the value of IsFault property, returns false if exception occured</returns>
        private static bool SafeGetIsFault(Message message)
        {
            try
            {
                return message.IsFault;
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceWarning("[ResponseQueueAdapter].SafeGetIsFault: Exception {0}", ex);

                // Swallow exception and return false
                return false;
            }
        }

        /// <summary>
        /// Try to reply message
        /// </summary>
        /// <param name="sessionId">indicate the session ID</param>
        /// <param name="context">indicate the request context</param>
        /// <param name="message">indicate the reply message</param>
        /// <param name="messageId">indicating the message id</param>
        private void TryReplyMessage(string sessionId, RequestContextBase context, Message message, Guid messageId)
        {
            try
            {
                this.observer.RequestProcessingCompleted();
                context.BeginReply(message, this.replyResponseCallback, new object[] { sessionId, context, message, messageId });
            }
            catch (Exception e)
            {
                bool isFault = SafeGetIsFault(message);

                // close mesage
                message.Close();

                // Bug 6288: Update counter regardless if reply is sent successfully or not
                this.ReplySent(context, isFault);
                BrokerTracing.EtwTrace.LogFrontEndResponseSentFailed(sessionId, context.CorrespondingBrokerClient.ClientId, messageId, e.ToString());
            }
        }

        /// <summary>
        /// Reply response
        /// </summary>
        /// <param name="ar">async result</param>
        private void ReplyResponse(IAsyncResult ar)
        {
            // No need to handle exceptions here
            object[] objArr = (object[])ar.AsyncState;
            string sessionId = (string)objArr[0];
            RequestContextBase context = (RequestContextBase)objArr[1];
            Message message = (Message)objArr[2];
            Guid messageId = (Guid)objArr[3];
            bool isFault = SafeGetIsFault(message);

            // close message
            message.Close();

            try
            {
                context.EndReply(ar);
                BrokerTracing.EtwTrace.LogFrontEndResponseSent(sessionId, context.CorrespondingBrokerClient.ClientId, messageId);
            }
            catch (Exception e)
            {
                BrokerTracing.EtwTrace.LogFrontEndResponseSentFailed(sessionId, context.CorrespondingBrokerClient.ClientId, messageId, e.ToString());

                // Should we swallow?
                // Swallow the exception
            }
            finally
            {
                // Why the ReplySent is called twice?
                this.ReplySent(context, isFault);
            }
        }

        /// <summary>
        /// Informs that a reply has been sent
        /// </summary>
        /// <param name="context">indicating the request context</param>
        /// <param name="isFault">indicating whether the reply is fault message</param>
        private void ReplySent(RequestContextBase context, bool isFault)
        {
            this.observer.ReplySent(isFault);
            context.CorrespondingBrokerClient.RegisterTimeoutIfNoPendingRequests();
        }
    }
}
