// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common
{
    using System;
    using System.ServiceModel.Channels;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;

    /// <summary>
    /// Base class for GetResponsesHandler and PullResponsesHandler
    /// </summary>
    internal abstract class BaseResponsesHandler : DisposableObjectSlim
    {
        /// <summary>
        /// Stores the broker queue
        /// </summary>
        private BrokerQueue queue;

        /// <summary>
        /// Stores the action
        /// </summary>
        private string action;

        /// <summary>
        /// Stores the response count
        /// </summary>
        private int responsesCount;

        /// <summary>
        /// Stores the message version
        /// </summary>
        private MessageVersion version;

        /// <summary>
        /// Stores the timeout manager
        /// </summary>
        private TimeoutManager timeoutManager;

        /// <summary>
        /// Stores the shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// Stores the broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Initializes a new instance of the BaseResponsesHandler class
        /// </summary>
        /// <param name="queue">indicating the broker queue</param>
        /// <param name="action">indicating the action</param>
        /// <param name="timeoutManager">indicating the timeout manager</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="version">indicating the message version</param>
        protected BaseResponsesHandler(BrokerQueue queue, string action, TimeoutManager timeoutManager, BrokerObserver observer, SharedData sharedData, MessageVersion version)
        {
            this.queue = queue;
            this.action = action;
            this.timeoutManager = timeoutManager;
            this.observer = observer;
            this.sharedData = sharedData;
            this.version = version;
        }

        /// <summary>
        /// Gets or sets the responses count
        /// </summary>
        protected int ResponsesCount
        {
            get { return this.responsesCount; }
            set { this.responsesCount = value; }
        }

        /// <summary>
        /// Gets the shared data
        /// </summary>
        protected SharedData SharedData
        {
            get { return this.sharedData; }
        }

        /// <summary>
        /// Gets the message version
        /// </summary>
        protected MessageVersion Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// Gets the broker queue
        /// </summary>
        protected BrokerQueue Queue
        {
            get { return this.queue; }
        }

        /// <summary>
        /// Gets the action
        /// </summary>
        protected string Action
        {
            get { return this.action; }
        }

        /// <summary>
        /// Informs that EndOfResponses has reached
        /// </summary>
        /// <param name="eventId">indicating the event id</param>
        /// <param name="eventArgs">indicating the event args</param>
        public abstract void EndOfResponses(BrokerQueueEventId eventId, ResponseEventArgs eventArgs);

        /// <summary>
        /// Informs that the session has failed
        /// </summary>
        public abstract void SessionFailed();

        /// <summary>
        /// Informs that the client is disconnected
        /// </summary>
        public abstract void ClientDisconnect(bool purged);

        /// <summary>
        /// Generate response action filter
        /// </summary>
        /// <param name="action">indicating the action string</param>
        /// <returns>response action filter</returns>
        protected static ResponseActionFilter GenerateResponseActionFilter(string action)
        {
            ResponseActionFilter filter = null;
            if (!String.IsNullOrEmpty(action))
            {
                filter = new ResponseActionFilter(action);
            }

            return filter;
        }

        /// <summary>
        /// Increase the responses count
        /// </summary>
        protected void IncreaseResponsesCount()
        {
            Interlocked.Increment(ref this.responsesCount);
            this.observer.OutgoingResponse();
        }

        /// <summary>
        /// Batch version to increase the responses count
        /// </summary>
        /// <param name="count">indicating the number of responses</param>
        protected void IncreaseResponsesCount(int count)
        {
            Interlocked.Add(ref this.responsesCount, count);
            this.observer.OutgoingResponse(count);
        }

        /// <summary>
        /// Reset the timeout
        /// </summary>
        protected void ResetTimeout()
        {
            this.timeoutManager.ResetTimeout();
        }

        /// <summary>
        /// Gets a value indicating whether the session is failed
        /// </summary>
        protected bool IsSessionFailed()
        {
            return this.sharedData.SessionFailed;
        }

        /// <summary>
        /// Convert the message to the correct version
        /// </summary>
        /// <param name="message">indicating the message</param>
        /// <returns>returns the converted message</returns>
        protected Message ConvertMessage(Message message)
        {
            if (message.Version == this.version)
            {
                return message;
            }

            if (message.IsFault)
            {
                MessageFault fault = MessageFault.CreateFault(message, BrokerEntry.MaxMessageSize);
                Message faultMessage = Message.CreateMessage(this.version, fault, message.Headers.Action);
                Utility.PrepareAddressingHeaders(message, faultMessage);
                return faultMessage;
            }

            Message converted = Message.CreateMessage(this.version, message.Headers.Action, message.GetReaderAtBodyContents());

            // Add request message action to response message header
            Utility.CopyMessageHeader(Constant.ActionHeaderName, Constant.HpcHeaderNS, message.Headers, converted.Headers);

            // Add user data header
            Utility.CopyMessageHeader(Constant.UserDataHeaderName, Constant.HpcHeaderNS, message.Headers, converted.Headers);

            // Add message id header
            Utility.CopyMessageHeader(Constant.MessageIdHeaderName, Constant.HpcHeaderNS, message.Headers, converted.Headers);

            Utility.PrepareAddressingHeaders(message, converted);
                        
            return converted;
        }
    }
}
