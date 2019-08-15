//-----------------------------------------------------------------------
// <copyright file="RequestReplyRequestContext.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Request context for request/reply channels</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using System;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Request context for request/reply channels (BasicHttp and WSHttp)
    /// </summary>
    /// <remarks>
    /// Wrapper of System.ServiceModel.Channels.RequestContext
    /// </remarks>
    internal class RequestReplyRequestContext : RequestContextBase
    {
        /// <summary>
        /// Store core request context
        /// </summary>
        private RequestContext requestContext;

        /// <summary>
        /// Stores the broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Initializes a new instance of the RequestReplyRequestContext class
        /// </summary>
        /// <param name="requestContext">core request context</param>
        /// <param name="observer">indicating the broker observer</param>
        public RequestReplyRequestContext(RequestContext requestContext, BrokerObserver observer)
            : this(requestContext, observer, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RequestReplyRequestContext class
        /// </summary>
        /// <param name="requestContext">core request context</param>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="client">indicating the broker client</param>
        public RequestReplyRequestContext(RequestContext requestContext, BrokerObserver observer, BrokerClient client)
            : base(requestContext.RequestMessage.Version, client)
        {
            this.requestContext = requestContext;
            this.observer = observer;
        }

        /// <summary>
        /// About the context
        /// </summary>
        public override void Abort()
        {
            this.requestContext.Close();
        }

        /// <summary>
        /// Async version to reply the message
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="callback">callback when reply ends</param>
        /// <param name="state">async state</param>
        /// <returns>async result</returns>
        public override IAsyncResult BeginReply(System.ServiceModel.Channels.Message message, AsyncCallback callback, object state)
        {
            this.observer.OutgoingResponse();
            return this.requestContext.BeginReply(message, callback, state);
        }

        /// <summary>
        /// Async version to reply the message indicating the timeout
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="timeout">timeout of the operation</param>
        /// <param name="callback">callback when reply ends</param>
        /// <param name="state">async state</param>
        /// <returns>async result</returns>
        public override IAsyncResult BeginReply(System.ServiceModel.Channels.Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.observer.OutgoingResponse();
            return this.requestContext.BeginReply(message, timeout, callback, state);
        }

        /// <summary>
        /// Close the context
        /// </summary>
        public override void Close()
        {
            this.requestContext.Close();
        }

        /// <summary>
        /// Close the context indicating the timeout
        /// </summary>
        /// <param name="timeout">timeout of the operation</param>
        public override void Close(TimeSpan timeout)
        {
            this.requestContext.Close(timeout);
        }

        /// <summary>
        /// Async version to end reply message
        /// </summary>
        /// <param name="result">async result</param>
        public override void EndReply(IAsyncResult result)
        {
            this.requestContext.EndReply(result);
        }


        /// <summary>
        /// Reply the message with timeout
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="timeout">timeout of the operation</param>
        public override void Reply(System.ServiceModel.Channels.Message message, TimeSpan timeout)
        {
            this.observer.OutgoingResponse();
            this.requestContext.Reply(message, timeout);
        }
    }
}
