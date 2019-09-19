// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using System;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Base class for all request context
    /// </summary>
    /// <remarks>
    /// Copied from WCF's source code System.ServiceModel.Channels.RequestContext
    /// </remarks>
    internal abstract class RequestContextBase : IDisposable
    {
        /// <summary>
        /// Store the message version
        /// </summary>
        private MessageVersion version;

        /// <summary>
        /// Stores the corresponding broker client
        /// </summary>
        private BrokerClient client;

        /// <summary>
        /// Initializes a new instance of the RequestContextBase class
        /// </summary>
        /// <param name="version">message version</param>
        /// <param name="client">indicating the corresponding broker client</param>
        protected RequestContextBase(MessageVersion version, BrokerClient client)
        {
            this.version = version;
            this.client = client;
        }

        /// <summary>
        /// Gets the message version
        /// </summary>
        public MessageVersion MessageVersion
        {
            get
            {
                return this.version;
            }
        }

        /// <summary>
        /// Gets the corresponding broker client
        /// </summary>
        public BrokerClient CorrespondingBrokerClient
        {
            get { return this.client; }
        }

        /// <summary>
        /// About the context
        /// </summary>
        public abstract void Abort();

        /// <summary>
        /// Async version to reply the message
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="callback">callback when reply ends</param>
        /// <param name="state">async state</param>
        /// <returns>async result</returns>
        public abstract IAsyncResult BeginReply(Message message, AsyncCallback callback, object state);

        /// <summary>
        /// Async version to reply the message indicating the timeout
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="timeout">timeout of the operation</param>
        /// <param name="callback">callback when reply ends</param>
        /// <param name="state">async state</param>
        /// <returns>async result</returns>
        public abstract IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state);

        /// <summary>
        /// Close the context
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// Close the context indicating the timeout
        /// </summary>
        /// <param name="timeout">timeout of the operation</param>
        public abstract void Close(TimeSpan timeout);

        /// <summary>
        /// Async version to end reply message
        /// </summary>
        /// <param name="result">async result</param>
        public abstract void EndReply(IAsyncResult result);

        /// <summary>
        /// Reply the message with timeout
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="timeout">timeout of the operation</param>
        public abstract void Reply(Message message, TimeSpan timeout);

        /// <summary>
        /// Dispose the context
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the context
        /// </summary>
        /// <param name="disposing">disposing operation</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
