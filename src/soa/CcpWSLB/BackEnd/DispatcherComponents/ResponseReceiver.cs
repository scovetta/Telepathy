//-----------------------------------------------------------------------
// <copyright file="ResponseReceiver.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     This is an abstract class for receiving response from service host.
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.Diagnostics.Contracts;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.ServiceBroker.Common;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// This is an abstract class for receiving response from service host.
    /// </summary>
    internal abstract class ResponseReceiver
    {
        /// <summary>
        /// It is contract message format. Make sure there is no other code
        /// before invoking Contract's method.
        /// </summary>
        private const string ContractMessageFormat =
            "{0} can not be null when attempt to receive response.";

        /// <summary>
        /// Gets or sets the dispatcher reference.
        /// </summary>
        /// <remarks>
        /// TODO: Now, we expose dispatcher to RequestSender, because other
        /// components (e.g. ExceptionHandler) are not ready yet. Finally, we
        /// will remove this after having a new engine.
        /// </remarks>
        private IDispatcher dispatcher;

        /// <summary>
        /// Initializes a new instance of the ResponseReceiver class.
        /// </summary>
        public ResponseReceiver(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        /// <summary>
        /// Receive response message from host.
        /// </summary>
        /// <param name="data">DispatchData instance</param>
        public void ReceiveResponse(DispatchData data)
        {
            Contract.Requires(data.AsyncResult != null, string.Format(ContractMessageFormat, "DispatchData.AsyncResult"));

            Contract.Requires(data.Client != null, string.Format(ContractMessageFormat, "DispatchData.Client"));

            Contract.Requires(data.BrokerQueueItem != null, string.Format(ContractMessageFormat, "DispatchData.BrokerQueueItem"));

            Contract.Ensures(
                data.ReplyMessage != null || data.Exception != null,
                "DispatchData should have either a reply message either an exception.");

            Message reply = null;

            int taskId = data.TaskId;

            DateTime dispatchTime = data.DispatchTime;

            int clientIndex = data.ClientIndex;

            IService client = data.Client;

            BrokerQueueItem item = data.BrokerQueueItem;

            // Bug #16197: if request action field is dropped by wcf, restore it back
            if (string.IsNullOrEmpty(item.Message.Headers.Action))
            {
                item.Message.Headers.Action = data.RequestAction;
            }

            this.dispatcher.items[clientIndex] = null;

            this.dispatcher.DecreaseProcessingCount();

            Guid messageId = Utility.GetMessageIdFromMessage(item.Message);

            try
            {
                // Get the response message
                reply = client.EndProcessMessage(data.AsyncResult);

                data.ReplyMessage = reply;

                // following method needs to be called before setting bServiceInitalizationCompleted flag
                this.PostProcessMessage(reply);

                // mark bServiceInitalizationCompleted flag to true;
                if (!this.dispatcher.ServiceInitializationCompleted)
                {
                    this.dispatcher.ServiceInitializationCompleted = true;

                    // nofity that it is connected to service host
                    this.dispatcher.OnServiceInstanceConnected(null);
                }

                BrokerTracing.EtwTrace.LogBackendResponseReceived(data.SessionId, taskId, messageId, reply.IsFault);
            }
            catch (EndpointNotFoundException e)
            {
                data.Exception = e;

                // TODO: need ExceptionHandler
                this.dispatcher.HandleEndpointNotFoundException(clientIndex, client, item, messageId, e);

                return;
            }
            catch (StorageException e)
            {
                data.Exception = e;

                // TODO: need ExceptionHandler
                this.dispatcher.HandleStorageException(dispatchTime, clientIndex, client, item, messageId, e);

                return;
            }
            catch (Exception e)
            {
                data.Exception = e;

                // TODO: need ExceptionHandler
                this.dispatcher.HandleException(dispatchTime, clientIndex, client, item, messageId, e);

                return;
            }

            if (reply.IsFault)
            {
                // If any fault message is received, consider passing binding data again.
                // TODO: pass binding data only on special error code
                this.dispatcher.PassBindingFlags[clientIndex] = true;
            }

            bool servicePreempted = false;

            if (reply.IsFault)
            {
                BrokerTracing.TraceVerbose(
                    BrokerTracing.GenerateTraceString(
                        "ResponseReceiver",
                        "ReceiveResponse",
                        taskId,
                        clientIndex,
                        client.ToString(),
                        messageId,
                        "The response is a fault message."));

                // TODO: need ExceptionHandler
                data.ExceptionHandled = this.dispatcher.HandleFaultExceptionRetry(clientIndex, item, reply, dispatchTime, out servicePreempted);
            }
            else
            {
                // if it is a normal response, check if the CancelEvent happens when request is being processed.
                int index = reply.Headers.FindHeader(Constant.MessageHeaderPreemption, Constant.HpcHeaderNS);

                if (index >= 0 && index < reply.Headers.Count)
                {
                    servicePreempted = true;

                    int messageCount = reply.Headers.GetHeader<int>(index);

                    BrokerTracing.TraceVerbose(
                        BrokerTracing.GenerateTraceString(
                            "ResponseReceiver",
                            "ReceiveResponse",
                            taskId,
                            clientIndex,
                            client.ToString(),
                            messageId,
                            string.Format("The count of processing message in the counterpart host is {0}.", messageCount)));

                    // will remove the dispatcher if no processing message left on the host
                    if (messageCount == 0)
                    {
                        // Simulate a SessionFault and reuse the method HandleServiceInstanceFailure,
                        // and the message will be processed later as normal.
                        BrokerTracing.TraceVerbose(
                            BrokerTracing.GenerateTraceString(
                                "ResponseReceiver",
                                "ReceiveResponse",
                                taskId,
                                clientIndex,
                                client.ToString(),
                                messageId,
                                "(Preemption) Call HandleServiceInstanceFailure."));

                        // TODO: need ServiceInstanceFailureHandler
                        this.dispatcher.HandleServiceInstanceFailure(new SessionFault(SOAFaultCode.Service_Preempted, string.Empty));
                    }
                }
            }

            data.ServicePreempted = servicePreempted;

            // Debug Failure Test
            Microsoft.Hpc.ServiceBroker.SimulateFailure.FailOperation(1);
        }

        /// <summary>
        /// Post process response message
        /// </summary>
        /// <param name="message">response message</param>
        protected abstract void PostProcessMessage(Message message);
    }
}
