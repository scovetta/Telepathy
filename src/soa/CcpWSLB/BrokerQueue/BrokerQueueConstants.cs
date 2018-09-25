//-----------------------------------------------------------------------------------
// <copyright file="BrokerQueueConstants.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>the definition of the constants for the broker queue.</summary>
//-----------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// the definition of the constants for the broker queue.
    /// </summary>
    internal static class BrokerQueueConstants
    {
        /// <summary>
        /// the flush waitone timeout
        /// </summary>
        public const int FlushWaitTimeout = 60 * 1000;

        /// <summary>
        /// the default timeout.
        /// </summary>
        public const int DefaultWaitTimeout = 30 * 1000;

        /// <summary>
        /// the default threshold value for requests that hold in the cache, if the number exceed this number, the the cached requests should be persisted to the storage.
        /// </summary>
        public const int DefaultThresholdForRequestPersist = 1000;

        /// <summary>
        /// the default threshold value for responses that hold in the cache, if the number exceed this number, the the cached responses should be persisted to the storage.
        /// </summary>
        public const int DefaultThresholdForResponsePersist = 1000;

        /// <summary>
        /// the default timeout for the messages in the cache, if no more messages come in, adn the timeout hit, then the cached messages should be persisted to the storage.
        /// </summary>
        public const int DefaultMessagesInCacheTimeout = 5000;

        /// <summary>
        /// the message copy buffer size, 4M size(the message size of the message queue cannot exceed 4 M)
        /// </summary>
        public const int DefaultMessageBufferSize = 4 * 1000 * 1000;

        /// <summary>
        /// the default quick cache size for a persist queue.
        /// </summary>
        public const int DefaultQuickCacheSize = 500;
    }
}
