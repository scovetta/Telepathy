// -----------------------------------------------------------------------
// <copyright file="MockDuplexRequestContext.cs" company="MSIT">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.UnitTest.Mock
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.ServiceBroker.FrontEnd;

    /// <summary>
    /// the Mock Duplex Request context
    /// </summary>
    internal class MockDuplexRequestContext : DuplexRequestContext
    {
        public MockDuplexRequestContext(Message message)
            : base(message, null, null, null)
        {
        }

        public Message ReplyMessage
        {
            get;
            private set;
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
            this.ReplyMessage = message;
            return null;
        }
    }
}
