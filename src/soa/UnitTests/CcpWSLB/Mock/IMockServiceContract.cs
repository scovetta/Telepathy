//-----------------------------------------------------------------------------------
// <copyright file="IMockServiceContract.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Mock object for service contract</summary>
//-----------------------------------------------------------------------------------
namespace Microsoft.Hpc.SvcBroker.UnitTest.Mock
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Text;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Mock object for service contract
    /// </summary>
    [ServiceContract]
    internal interface IMockServiceContract
    {
        /// <summary>
        /// Process Message
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        [OperationContract(Action = "*", ReplyAction = "*")]
        Message ProcessMessage(Message request);
    }
}
