//------------------------------------------------------------------------------
// <copyright file="DummyRequestContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Represents dummy request context
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Represents dummy request context
    /// </summary>
    internal class DummyRequestContext : RequestContextBase
    {
        /// <summary>
        /// Stores the dummy request context dictionary
        /// </summary>
        private static Dictionary<MessageVersion, DummyRequestContext> dummyRequestContextDic = new Dictionary<MessageVersion, DummyRequestContext>();

        /// <summary>
        /// Stores the dummy request context for Soap11 message (for BasicHttp and Java client)
        /// </summary>
        private static DummyRequestContext Soap11RequestContext = new DummyRequestContext(MessageVersion.Soap11);

        /// <summary>
        /// Stores the dummy request context for Soap12WSAddressing10 message (for NetTcp)
        /// </summary>
        private static DummyRequestContext Soap12WSAddressing10RequestContext = new DummyRequestContext(MessageVersion.Soap12WSAddressing10);

        /// <summary>
        /// Stores the locker
        /// </summary>
        private static object lockThis = new object();

        /// <summary>
        /// Initializes a new instance of the DummyRequestContext class
        /// </summary>
        /// <param name="version">indicate the message version</param>
        private DummyRequestContext(MessageVersion version)
            : base(version, null)
        {
        }

        /// <summary>
        /// Gets a dummy request context instance
        /// </summary>
        /// <param name="version">indicating the message version</param>
        /// <returns>returns the message version</returns>
        public static DummyRequestContext GetInstance(MessageVersion version)
        {
            if (version == MessageVersion.Soap11)
            {
                return Soap11RequestContext;
            }
            else if (version == MessageVersion.Soap12WSAddressing10)
            {
                return Soap12WSAddressing10RequestContext;
            }
            else
            {
                lock (lockThis)
                {
                    DummyRequestContext instance;
                    if (!dummyRequestContextDic.TryGetValue(version, out instance))
                    {
                        instance = new DummyRequestContext(version);
                        dummyRequestContextDic.Add(version, instance);
                    }

                    return instance;
                }
            }

        }

        /// <summary>
        /// About the context
        /// </summary>
        public override void Abort()
        {
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
            return null;
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
            return null;
        }

        /// <summary>
        /// Close the context
        /// </summary>
        public override void Close()
        {
        }

        /// <summary>
        /// Close the context indicating the timeout
        /// </summary>
        /// <param name="timeout">timeout of the operation</param>
        public override void Close(TimeSpan timeout)
        {
        }

        /// <summary>
        /// Async version to end reply message
        /// </summary>
        /// <param name="result">async result</param>
        public override void EndReply(IAsyncResult result)
        {
        }

        /// <summary>
        /// Reply the message with timeout
        /// </summary>
        /// <param name="message">reply message</param>
        /// <param name="timeout">timeout of the operation</param>
        public override void Reply(Message message, TimeSpan timeout)
        {
        }
    }
}
