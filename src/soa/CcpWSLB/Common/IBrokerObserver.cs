// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker
{
    /// <summary>
    /// Provides an interface defining operations of BrokerObserver component
    /// </summary>
    internal interface IBrokerObserver
    {
        /// <summary>
        /// Informs that a request is completed (either finished correctly or failed with some exception)
        /// </summary>
        void RequestProcessingCompleted();

        /// <summary>
        /// Informs that a reply has been sent back to the client
        /// </summary>
        /// <param name="isFault">true if the reply is a fault.</param>
        void ReplySent(bool isFault);

        /// <summary>
        /// Indicates the observer that a call is completed
        /// </summary>
        /// <param name="duration">indicating the call duration</param>
        void CallComplete(long duration);
    }
}
