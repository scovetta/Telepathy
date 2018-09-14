//------------------------------------------------------------------------------
// <copyright file="IBrokerFrontend.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Interface for broker frontend
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Interface for broker frontend
    /// </summary>
    public interface IBrokerFrontend : IController, IResponseService
    {
        /// <summary>
        /// Send requests
        /// </summary>
        /// <param name="message">indicating the message</param>
        void SendRequest(Message message);

        /// <summary>
        /// End request received operation
        /// </summary>
        /// <param name="clientId">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        bool EndRequestReceived(string clientId);
    }
}
