//-----------------------------------------------------------------------------------
// <copyright file="MockServiceHost.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Mock object for service host</summary>
//-----------------------------------------------------------------------------------
namespace Microsoft.Hpc.SvcBroker.UnitTest.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    /// <summary>
    /// Mock object for service host
    /// </summary>
    internal class MockServiceHost : IMockServiceContract
    {
        /// <summary>
        /// Process Message
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        public Message ProcessMessage(Message request)
        {
            return Message.CreateMessage(request.Version, request.Headers.Action, request.GetBody<string>() + "Reply");
        }
    }
}
