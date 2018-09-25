//------------------------------------------------------------------------------
// <copyright file="IBrokerQueueFactory.cs" company="Microsoft">
//      Copyright (C)  Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Provides an interface defining opeartions of BrokerQueueFactory
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage
{
    using System.ServiceModel.Channels;

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
        void PutResponseAsync(Message responseMsg, BrokerQueueItem requestItem);
    }
}
