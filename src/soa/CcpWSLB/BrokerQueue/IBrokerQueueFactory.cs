// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an interface defining opeartions of BrokerQueueFactory
    /// </summary>
    internal interface IBrokerQueueFactory
    {
        /// <summary>
        /// Put the response into the storage, and delete corresponding request from the storage.
        /// the async result will return void.byt GetResult will throw exception if the response is not persisted into the persistence.
        /// </summary>
        /// <param name="responseMsg">the response message</param>
        /// <param name="requestItem">corresponding request item</param>
        Task PutResponseAsync(Message responseMsg, BrokerQueueItem requestItem);
    }
}
