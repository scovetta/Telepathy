//-----------------------------------------------------------------------
// <copyright file="DuplexRequestContext.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Request context for duplex channels</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Request context for duplex channels (NetTcp channel)
    /// </summary>
    /// <remarks>
    /// Copied from WCF source code: System.ServiceModel.Channels.ReplyOverDuplexChannelBase<![CDATA[<TInnerChannel>]]>+DuplexRequestContext<![CDATA[<TInnerChannel>]]>
    /// </remarks>
    internal class DuplexRequestContext : RequestContextBase
    {
        /// <summary>
        /// Default timeout for the request context
        /// </summary>
        private IDefaultCommunicationTimeouts defaultTimeouts;

        /// <summary>
        /// Store the inner channel
        /// </summary>
        private IDuplexChannel innerChannel;

        /// <summary>
        /// Store the reply to endpoint address
        /// </summary>
        private EndpointAddress replyTo;

        /// <summary>
        /// Stores the broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Stores the message id
        /// </summary>
        private UniqueId messageId;

        /// <summary>
        /// Initializes a new instance of the DuplexRequestContext class
        /// </summary>
        /// <param name="request">request message</param>
        /// <param name="defaultTimeouts">default timeouts</param>
        /// <param name="innerChannel">inner channel</param>
        /// <param name="observer">indicating the broker observer</param>
        public DuplexRequestContext(Message request, IDefaultCommunicationTimeouts defaultTimeouts, IDuplexChannel innerChannel, BrokerObserver observer)
            : this(request, defaultTimeouts, innerChannel, observer, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DuplexRequestContext class
        /// </summary>
        /// <param name="request">request message</param>
        /// <param name="defaultTimeouts">default timeouts</param>
        /// <param name="innerChannel">inner channel</param>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="client">indicating the broker client</param>
        public DuplexRequestContext(Message request, IDefaultCommunicationTimeouts defaultTimeouts, IDuplexChannel innerChannel, BrokerObserver observer, BrokerClient client)
            : base(request.Version, client)
        {
            this.defaultTimeouts = defaultTimeouts;
            this.innerChannel = innerChannel;
            this.observer = observer;
            if (request != null)
            {
                this.messageId = request.Headers.MessageId;
                this.replyTo = request.Headers.ReplyTo;
            }
        }

        /// <summary>
        /// About the request context
        /// </summary>
        public override void Abort()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Async version to reply the message
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="callback">callback when reply ends</param>
        /// <param name="state">async state</param>
        /// <returns>async result</returns>
        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            return this.BeginReply(message, this.defaultTimeouts.SendTimeout, callback, state);
        }

        /// <summary>
        /// Async version to reply the message indicating the timeout
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="timeout">timeout of the operation</param>
        /// <param name="callback">callback when reply ends</param>
        /// <param name="state">async state</param>
        /// <returns>async result</returns>
        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.observer.OutgoingResponse();
            this.PrepareReply(message);
            return this.innerChannel.BeginSend(message, timeout, callback, state);
        }

        /// <summary>
        /// Close the context
        /// </summary>
        public override void Close()
        {
            this.Close(this.defaultTimeouts.CloseTimeout);
        }

        /// <summary>
        /// Close the context indicating the timeout
        /// </summary>
        /// <param name="timeout">the time out</param>
        public override void Close(TimeSpan timeout)
        {
            this.Dispose(true);
        }

        /// <summary>
        /// End reply the result
        /// </summary>
        /// <param name="result">async result</param>
        public override void EndReply(IAsyncResult result)
        {
            this.innerChannel.EndSend(result);
        }

        /// <summary>
        /// Reply the message with timeout
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="timeout">timeout of the operation</param>
        public override void Reply(Message message, TimeSpan timeout)
        {
            this.observer.OutgoingResponse();
            this.PrepareReply(message);
            this.innerChannel.Send(message);
        }

        /// <summary>
        /// Prepare the reply
        /// </summary>
        /// <param name="message">reply message</param>
        private void PrepareReply(Message message)
        {
            if (this.replyTo != null)
            {
                this.replyTo.ApplyTo(message);
            }

            if (this.messageId != null)
            {
                message.Headers.RelatesTo = this.messageId;
            }
        }
    }
}
