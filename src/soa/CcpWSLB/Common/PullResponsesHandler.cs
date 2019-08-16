//------------------------------------------------------------------------------
// <copyright file="PullResponsesHandler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Handler for pull responses
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Xml;
    using System.Xml.XPath;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.ServiceBroker.FrontEnd;

    /// <summary>
    /// Handler for pull responses
    /// </summary>
    internal class PullResponsesHandler : BaseResponsesHandler
    {
        /// <summary>
        /// Stores the pull responses messages
        /// </summary>
        private PullResponseMessage result;

        /// <summary>
        /// Initializes a new instance of the PullResponsesHandler class
        /// </summary>
        /// <param name="queue">indicating the broker queue</param>
        /// <param name="action">indicating the action</param>
        /// <param name="timeoutManager">indicating the timeout manager</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="version">indicating the message version</param>
        public PullResponsesHandler(BrokerQueue queue, string action, TimeoutManager timeoutManager, BrokerObserver observer, SharedData sharedData, MessageVersion version)
            : base(queue, action, timeoutManager, observer, sharedData, version)
        {
        }

        /// <summary>
        /// Pull responses
        /// </summary>
        /// <param name="position">indicating the position</param>
        /// <param name="count">indicating the count</param>
        /// <returns>broker response messages</returns>
        public BrokerResponseMessages PullResponses(GetResponsePosition position, int count)
        {
            this.result = new PullResponseMessage(count, this.Queue, this.ConvertMessage, this.ResetTimeout, this.SharedData);
            if (position == GetResponsePosition.Begin)
            {
                this.Queue.ResetResponsesCallback();
                this.ResponsesCount = 0;
            }

            BrokerResponseMessages responseMessages;
            if (this.Queue.RegisterResponsesCallback(this.result.ReceiveResponse, OperationContext.Current.IncomingMessageVersion, GenerateResponseActionFilter(this.Action), count, null))
            {
                this.result.CompleteEvent.WaitOne();
                responseMessages = this.result.Message;
            }
            else
            {
                responseMessages = this.result.Message;
                responseMessages.EOM = true;
            }

            BrokerTracing.TraceInfo("[PullResponsesHandler] Pull responses finished, Count = {0}", responseMessages.SOAPMessage.Length);
            this.IncreaseResponsesCount(responseMessages.SOAPMessage.Length);
            return responseMessages;
        }

        /// <summary>
        /// Check if the action matches this handler
        /// </summary>
        /// <param name="action">indicating the action</param>
        /// <returns>whether the action matches the handler</returns>
        public bool Match(string action)
        {
            return this.Action == action;
        }

        /// <summary>
        /// Informs that EndOfResponses has reached
        /// </summary>
        /// <param name="eventId">indicating the event id</param>
        /// <param name="eventArgs">indicating the event args</param>
        public override void EndOfResponses(BrokerQueueEventId eventId, ResponseEventArgs eventArgs)
        {
            this.result.EndOfResponses(eventId);
        }

        /// <summary>
        /// Informs that the session has failed
        /// </summary>
        public override void SessionFailed()
        {
            this.result.SessionFailed();
        }

        /// <summary>
        /// Informs that the client is disconnected
        /// </summary>
        public override void ClientDisconnect(bool purged)
        {
            this.result.ClientDisposed(purged ? EndOfResponsesReason.ClientPurged : EndOfResponsesReason.ClientTimeout);
        }

        /// <summary>
        /// Dispose the pull responses handler
        /// </summary>
        protected override void DisposeInternal()
        {
            base.DisposeInternal();
            this.result.Dispose();
        }

        /// <summary>
        ///  Internal class to handle async read from broker queue
        /// </summary>
        private class PullResponseMessage : DisposableObjectSlim
        {
            /// <summary>
            /// end of message.
            /// </summary>
            private bool eom;

            /// <summary>
            /// The messages
            /// </summary>
            private List<XmlElement> messages = new List<XmlElement>();

            /// <summary>
            /// the count to read as expected
            /// </summary>
            private int count;

            /// <summary>
            /// Stores the broker queue
            /// </summary>
            private BrokerQueue queue;

            /// <summary>
            /// Stores the the event to wait until complete
            /// </summary>
            private AutoResetEvent completeEvent = new AutoResetEvent(false);

            /// <summary>
            /// Stores the convert message delegate
            /// </summary>
            private ConvertMessageDelegate convertMessage;

            /// <summary>
            /// Stores the reset timeout delegate
            /// </summary>
            private ResetTimeoutDelegate resetTimeout;

            /// <summary>
            /// Stores the shared data
            /// </summary>
            private SharedData sharedData;

            /// <summary>
            /// Initializes a new instance of the PullResponseMessage class
            /// </summary>
            /// <param name="count">indicating the count</param>
            /// <param name="queue">indicating the queue</param>
            /// <param name="convertMessage">indicating the convert message delegate</param>
            /// <param name="resetTimeout">indicating the reset timeout delegate</param>
            /// <param name="sharedData">indicating the shared data</param>
            public PullResponseMessage(int count, BrokerQueue queue, ConvertMessageDelegate convertMessage, ResetTimeoutDelegate resetTimeout, SharedData sharedData)
            {
                this.count = count;
                this.queue = queue;
                this.convertMessage = convertMessage;
                this.resetTimeout = resetTimeout;
                this.sharedData = sharedData;
            }

            /// <summary>
            /// Defines the convert message delegate
            /// </summary>
            /// <param name="message">indicating the message</param>
            /// <returns>returns the converted message</returns>
            public delegate Message ConvertMessageDelegate(Message message);

            /// <summary>
            /// Defines the reset timeout delegate
            /// </summary>
            public delegate void ResetTimeoutDelegate();

            /// <summary>
            /// Gets the BrokerResponseMessages
            /// </summary>
            public BrokerResponseMessages Message
            {
                get
                {
                    BrokerResponseMessages result = new BrokerResponseMessages();
                    result.SOAPMessage = this.messages.ToArray();
                    result.EOM = this.eom;
                    return result;
                }
            }

            /// <summary>
            /// Gets the event to wait until complete
            /// </summary>
            public AutoResetEvent CompleteEvent
            {
                get { return this.completeEvent; }
            }

            /// <summary>
            /// Indicates EndOfResponses event has been triggered
            /// </summary>
            /// <param name="eventId">indicating the event id</param>
            public void EndOfResponses(BrokerQueueEventId eventId)
            {
                if (eventId == BrokerQueueEventId.AllResponesDispatched
                    || (this.sharedData.SessionFailed && eventId == BrokerQueueEventId.AvailableResponsesDispatched))
                {
                    if (eventId == BrokerQueueEventId.AvailableResponsesDispatched)
                    {
                        this.GenerateFaultMessage(EndOfResponsesReason.ClientTimeout);
                    }

                    this.eom = true;
                    this.completeEvent.Set();
                }
            }

            /// <summary>
            /// the event when the session failed.
            /// </summary>
            public void SessionFailed()
            {
                if (this.queue.ProcessedRequestsCount <= 0)
                {
                    this.GenerateSessionFailureFaultMessage();
                    this.eom = true;
                    this.completeEvent.Set();
                }
            }

            /// <summary>
            /// Informs that the client is disposed
            /// </summary>
            /// <param name="reason">indicating the reason</param>
            public void ClientDisposed(EndOfResponsesReason reason)
            {
                if (this.queue.ProcessedRequestsCount <= 0)
                {
                    this.GenerateFaultMessage(reason);
                    this.eom = true;
                    this.completeEvent.Set();
                }
            }

            /// <summary>
            /// Receive Response
            /// </summary>
            /// <param name="item">indicating the broker queue item</param>
            /// <param name="asyncState">indicating the async state</param>
            public void ReceiveResponse(BrokerQueueItem item, object asyncState)
            {
                BrokerTracing.TraceVerbose("[PullResponsesHandler] Receive response.");
                XmlDocument doc = new XmlDocument() { XmlResolver = null };
                this.resetTimeout();

                try
                {
                    Message soap11Message = this.convertMessage(item.Message);
                    byte[] buffer;

                    MemoryStream ms = null;
                    try
                    {
                        ms = new MemoryStream();
                        using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(ms))
                        {
                            var msTemp = ms;
                            ms = null;
                            soap11Message.WriteMessage(writer);
                            writer.Flush();
                            buffer = msTemp.ToArray();
                        }
                    }
                    finally
                    {
                        if (ms != null)
                            ms.Dispose();
                    }

                    try
                    {
                        ms = new MemoryStream(buffer);
                        using (XmlReader reader = XmlReader.Create(ms))
                        {
                            ms = null;
                            doc.Load(reader);
                        }
                    }
                    finally
                    {
                        if (ms != null)
                            ms.Dispose();
                    }

                    this.messages.Add(doc.DocumentElement);
                    if (this.messages.Count >= this.count)
                    {
                        this.completeEvent.Set();
                    }
                }
                finally
                {
                    this.queue.AckResponse(item, true);
                }
            }

            /// <summary>
            /// Dispose the PullResponseMessage
            /// </summary>
            protected override void DisposeInternal()
            {
                base.DisposeInternal();

                try
                {
                    this.completeEvent.Close();
                }
                catch (Exception ex)
                {
                    BrokerTracing.TraceWarning("[PullResponsesHandler].DisposeInternal: Exception {0}", ex);
                }
            }

            /// <summary>
            /// generate the fault message when the session failed.
            /// </summary>
            private void GenerateFaultMessage(EndOfResponsesReason reason)
            {
                XmlDocument doc = new XmlDocument() { XmlResolver = null };
                Message soap11FaultMessage = null;
                switch (reason)
                {
                    case EndOfResponsesReason.ClientPurged:
                        soap11FaultMessage = FrontEndFaultMessage.GenerateFaultMessage(null, MessageVersion.Soap11, SOAFaultCode.ClientPurged, Microsoft.Hpc.SvcBroker.SR.ClientPurged);
                        break;
                    case EndOfResponsesReason.ClientTimeout:
                        soap11FaultMessage = FrontEndFaultMessage.GenerateFaultMessage(null, MessageVersion.Soap11, SOAFaultCode.ClientTimeout, Microsoft.Hpc.SvcBroker.SR.ClientTimeout);
                        break;
                }

                XPathNavigator nav = soap11FaultMessage.CreateBufferedCopy(BrokerEntry.MaxMessageSize).CreateNavigator();
                doc.Load(nav.ReadSubtree());
                this.messages.Add(doc.DocumentElement);
            }

            /// <summary>
            /// generate the fault message when the session failed.
            /// </summary>
            private void GenerateSessionFailureFaultMessage()
            {
                XmlDocument doc = new XmlDocument() { XmlResolver = null };
                Message soap11FaultMessage = FrontEndFaultMessage.GenerateFaultMessage(null, MessageVersion.Soap11, SOAFaultCode.Broker_SessionFailure, Microsoft.Hpc.SvcBroker.SR.SessionFailure);
                XPathNavigator nav = soap11FaultMessage.CreateBufferedCopy(BrokerEntry.MaxMessageSize).CreateNavigator();
                doc.Load(nav.ReadSubtree());
                this.messages.Add(doc.DocumentElement);
            }
        }
    }
}
