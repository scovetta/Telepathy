//-----------------------------------------------------------------------
// <copyright file="RequestSender.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     This is an abstract class for sending requests.
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.Common;

    /// <summary>
    /// This is an abstract class for sending requests.
    /// </summary>
    internal abstract class RequestSender
    {
        private double totalSeconds = 0.0;
        private int totalCount = 0;
        private DateTime lastSentTime = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the RequestSender class.
        /// </summary>
        /// <param name="epr">endpoint address of the target proxy or host</param>
        /// <param name="binding">backend binding</param>
        /// <param name="serviceOperationTimeout">service operation timeout of backend connection</param>
        /// <param name="dispatcher">dispatcher instance</param>
        public RequestSender(EndpointAddress epr, Binding binding, int serviceOperationTimeout, IDispatcher dispatcher)
        {
            this.EndpointReferenceAddress = epr;

            this.BackendBinding = binding;

            this.IsBackendHttpConection = this.BackendBinding.CreateBindingElements().Find<HttpTransportBindingElement>() != null;

            this.BackendServiceOperationTimeout = serviceOperationTimeout;

            this.Dispatcher = dispatcher;

            this.TaskId = this.Dispatcher.TaskId;
        }

        /// <summary>
        /// Gets or sets the endpoint reference address of target service host.
        /// </summary>
        protected EndpointAddress EndpointReferenceAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the client sending requests.
        /// </summary>
        protected IService IServiceClient
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the task Id.
        /// </summary>
        protected int TaskId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the binding of backend connection between broker and service host.
        /// </summary>
        protected Binding BackendBinding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the operation timeout for backend connection.
        /// </summary>
        protected int BackendServiceOperationTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the backend uses http connection.
        /// </summary>
        protected bool IsBackendHttpConection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the dispatcher reference.
        /// </summary>
        /// <remarks>
        /// TODO: Now, we expose dispatcher to RequestSender, because
        /// DispatcherEngine is not ready yet. Finally, we will remove this
        /// after having a new engine.
        /// </remarks>
        protected IDispatcher Dispatcher
        {
            get;
            set;
        }

        /// <summary>
        /// Start client.
        /// </summary>
        /// <param name="state">GetNextRequestState instance</param>
        public abstract void StartClient(GetNextRequestState state);

        /// <summary>
        /// Refresh the client.
        /// </summary>
        /// <param name="clientIndex">client index</param>
        /// <param name="exceptionIndirect">
        ///     true: exception is from the connection between proxy and host,
        ///     need to increase message retry count
        ///     false: exception is from the connection between broker and proxy.
        /// </param>
        /// <param name="messageId">message Id</param>
        /// <returns>should increase the retry count or not</returns>
        public abstract Task<bool> RefreshClientAsync(int clientIndex, bool exceptionIndirect, Guid messageId);

        /// <summary>
        /// Release client. This applies to the client pool.
        /// </summary>
        public abstract void ReleaseClient();

        /// <summary>
        /// Create a IService instance with specified index.
        /// </summary>
        /// <param name="getNextRequest">
        /// trigger the client to retrieve next request
        /// </param>
        /// <param name="clientIndex">
        /// index of the client
        /// </param>
        public virtual async Task CreateClientAsync(bool getNextRequest, int clientIndex)
        {
            if (this.IServiceClient != null)
            {
                BrokerTracing.TraceWarning(
                    BrokerTracing.GenerateTraceString(
                        "RequestSender",
                        "CreateClientAsync",
                        this.TaskId,
                        clientIndex,
                        this.IServiceClient.ToString(),
                        string.Empty,
                        "Closed former client proxy."));

                this.CloseClient();
            }

            try
            {
                await this.CreateClientAsync().ConfigureAwait(false);

                BrokerTracing.TraceVerbose(
                    BrokerTracing.GenerateTraceString(
                        "RequestSender",
                        "CreateClientAsync",
                        this.TaskId,
                        clientIndex,
                        this.IServiceClient.ToString(),
                        string.Empty,
                        "Created a new client."));

                this.Dispatcher.PassBindingFlags[clientIndex] = true;

                GetNextRequestState state = null;

                if (getNextRequest)
                {
                    state = new GetNextRequestState(this.Dispatcher.GetNextRequest, clientIndex);
                }

                this.StartClient(state);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(
                    TraceEventType.Error,
                    0,
                    "[RequestSender] .CreateClientAsync: ID = {0}, init client failed: {1}",
                    this.TaskId,
                    e);

                if (this.IServiceClient != null)
                {
                    this.CloseClient();
                }

                this.Dispatcher.CloseThis();
            }
        }

        /// <summary>
        /// Check the client if it is valid.
        /// </summary>
        /// <param name="clientIndex">client index</param>
        /// <param name="messageId">message Id</param>
        /// <returns>valid or not</returns>
        public virtual Task<bool> ValidateClientAsync(int clientIndex, Guid messageId)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Send out request to the target proxy or host.
        /// </summary>
        /// <param name="data">dispatch data</param>
        /// <param name="dispatchId">dispatch Id</param>
        /// <param name="clientIndex">client index</param>
        public virtual void SendRequest(DispatchData data, Guid dispatchId, int clientIndex)
        {
            Message requestMessage = data.BrokerQueueItem.Message;

            Guid messageId = Utility.GetMessageIdFromMessage(requestMessage);

            // Bug 12045: Convert message before preparing it as some headers
            // might be added during preparation.
            // Check version
            if (requestMessage.Headers.MessageVersion != this.BackendBinding.MessageVersion)
            {
                BrokerTracing.TraceVerbose("[RequestSender].SendRequest: Convert message version from {0} to {1}", requestMessage.Headers.MessageVersion, this.BackendBinding.MessageVersion);

                requestMessage = Utility.ConvertMessage(requestMessage, this.BackendBinding.MessageVersion);
            }

            this.PrepareMessage(requestMessage, dispatchId, this.Dispatcher.PassBindingFlags[clientIndex]);

            data.Client = this.IServiceClient;

            // Bug #16197: reserve request message action in case it gets dropped when wcf processes the message
            data.RequestAction = requestMessage.Headers.Action;

            data.DispatchTime = DateTime.Now;

            this.IServiceClient.BeginProcessMessage(
                requestMessage,
                this.ProcessMessageCallback,
                data);

            this.lastSentTime = DateTime.UtcNow;

            // Notice: the request may complete fast, and it is disposed when response comes back before following code executes.
            // So don't access item.Message in following code.

            // Bug #13430:
            // (1) Define "try" as request sent by broker, so any request sent by broker successfully should
            //     have "current try count + 1" and not retried any more if retrylimit=0.
            // (2) Don't increase the try count if EndpointNotFoundException occurs.
            // (3) In "Burst to Azure" mode, connections are shared by dispatchers. The connection is possible to
            //     be killed because of failure in one dispatcher. Don't increase the try count in such case.

            // Increase the try count when BeginProcessMessage succeeds.
            data.BrokerQueueItem.TryCount++;

            BrokerTracing.TraceVerbose(
                BrokerTracing.GenerateTraceString(
                    "RequestSender",
                    "SendRequest",
                    this.TaskId,
                    clientIndex,
                    this.IServiceClient.ToString(),
                    messageId,
                    string.Format("Sent out message and increase the try count to {0}", data.BrokerQueueItem.TryCount)));
        }

        private void ProcessMessageCallback(IAsyncResult ar)
        {
            var data = (DispatchData)ar.AsyncState;

            double seconds = (DateTime.UtcNow - this.lastSentTime).TotalSeconds;
            this.totalSeconds += seconds;
            this.totalCount++;

            BrokerTracing.TraceVerbose(
                BrokerTracing.GenerateTraceString(
                    "RequestSender",
                    "SendRequest",
                    this.TaskId,
                    data.ClientIndex,
                    this.IServiceClient.ToString(),
                    data.MessageId,
                    $"Current process time {seconds}, total seconds {this.totalSeconds.ToString()}, total count {this.totalCount.ToString()}, average {(this.totalSeconds / this.totalCount).ToString()}"));

            this.Dispatcher.ProcessMessageCallback(ar); // Fire and forget here
        }

        /// <summary>
        /// Create an IService instance.
        /// </summary>
        protected abstract Task CreateClientAsync();

        /// <summary>
        /// Close client.
        /// </summary>
        protected abstract void CloseClient();

        /// <summary>
        /// Prepare the message for sending
        /// </summary>
        /// <param name="message">request message</param>
        /// <param name="dispatchId">indicating the dispatch id</param>
        /// <param name="needBinding">add binding data to the message header or not</param>
        protected virtual void PrepareMessage(Message message, Guid dispatchId, bool needBinding)
        {
            MessageHeader dispatchIdHeader = MessageHeader.CreateHeader(Constant.DispatchIdHeaderName, Constant.HpcHeaderNS, dispatchId);

            message.Headers.Add(dispatchIdHeader);

            // For Java service host with soap 1.1 and wsaddressing 1.0, we need to set the
            // ReplyTo as AnonymouseUri instead of None
            if (this.BackendBinding.MessageVersion == MessageVersion.Soap11WSAddressing10 && this.IsBackendHttpConection)
            {
                message.Headers.ReplyTo = new EndpointAddress(EndpointAddress.AnonymousUri);
                message.Headers.FaultTo = new EndpointAddress(EndpointAddress.AnonymousUri);
            }
        }
    }
}
