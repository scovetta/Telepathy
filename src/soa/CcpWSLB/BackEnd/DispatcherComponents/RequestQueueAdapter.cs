// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents
{
    using System.Diagnostics.Contracts;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.ServiceBroker.Common;

    /// <summary>
    /// Request queue adapter which is in charge of putting request
    /// back into broker queue
    /// </summary>
    internal class RequestQueueAdapter
    {
        /// <summary>
        /// Stores the instance of broker observer
        /// </summary>
        private IBrokerObserver observer;

        /// <summary>
        /// Stores the instance of broker queue factory
        /// </summary>
        private IBrokerQueueFactory queueFactory;

        /// <summary>
        /// Initializes a new instance of the RequestQueueAdapter class
        /// </summary>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="factory">indicating the broker queue factory</param>
        public RequestQueueAdapter(IBrokerObserver observer, IBrokerQueueFactory factory)
        {
            Contract.Requires(observer != null);
            Contract.Requires(factory != null);

            this.observer = observer;
            this.queueFactory = factory;
        }

        /// <summary>
        /// Put request back into broker queue
        /// </summary>
        /// <param name="data">indicating the instance of dispatch data</param>
        public void PutRequestBack(DispatchData data)
        {
            Contract.Requires(data.BrokerQueueItem != null);
            Contract.Ensures(data.BrokerQueueItem == null);

            BrokerTracing.EtwTrace.LogBackendRequestPutBack(data.SessionId, data.TaskId, data.MessageId);
            this.observer.RequestProcessingCompleted();
            this.queueFactory.PutResponseAsync(null, data.BrokerQueueItem).GetAwaiter().GetResult();

            // Set to null since we returned it back to queue
            data.BrokerQueueItem = null;
        }
    }
}
