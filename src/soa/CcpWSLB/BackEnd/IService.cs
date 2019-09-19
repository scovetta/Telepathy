// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Standard service contract
    /// </summary>
    [ServiceContract]
    internal interface IService
    {
        /// <summary>
        /// Standard operation contract for request/reply
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        [OperationContract(Action = "*", ReplyAction = "*")]
        Message ProcessMessage(Message request);

        /// <summary>
        /// Async Pattern
        /// Begin method for ProcessMessage
        /// </summary>
        /// <param name="request">request message</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true, Action = "*", ReplyAction = "*")]
        IAsyncResult BeginProcessMessage(Message request, AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async Pattern
        /// End method for ProcessMessage
        /// </summary>
        /// <param name="ar">async result</param>
        /// <returns>reply message</returns>
        Message EndProcessMessage(IAsyncResult ar);
    }
}
