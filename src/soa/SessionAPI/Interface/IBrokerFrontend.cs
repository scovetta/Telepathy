// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System.ServiceModel.Channels;

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
