// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.Session.Common;

    /// <summary>
    /// the response callback item.
    /// </summary>
    internal class BrokerQueueCallbackItem
    {
        #region private fields
        /// <summary>the response callback.</summary>
        private BrokerQueueCallback brokerQueueCallbackField;

        /// <summary>the state object for the callback.</summary>
        private object callbackStateField;

        #endregion

        /// <summary>
        /// Initializes a new instance of the BrokerQueueCallbackItem class.
        /// </summary>
        /// <param name="brokerQueueCallback">the response/request callback.</param>
        /// <param name="state">the state object for the callback.</param>
        public BrokerQueueCallbackItem(BrokerQueueCallback brokerQueueCallback, object state)
        {
            ParamCheckUtility.ThrowIfNull(brokerQueueCallback, "brokerQueueCallback");
            this.brokerQueueCallbackField = brokerQueueCallback;
            this.callbackStateField = state;
        }

        /// <summary>
        /// Gets the response callback.
        /// </summary>
        public BrokerQueueCallback Callback
        {
            get
            {
                return this.brokerQueueCallbackField;
            }
        }

        /// <summary>
        /// Gets the callback state object.
        /// </summary>
        public object CallbackState
        {
            get
            {
                return this.callbackStateField;
            }
        }
    }
}
