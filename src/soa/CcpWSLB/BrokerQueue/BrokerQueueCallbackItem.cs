//-----------------------------------------------------------------------------------
// <copyright file="BrokerQueueCallbackItem.cs" company="Microsoft">
//     Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>the response callback item definition.</summary>
//-----------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage
{
    using Microsoft.Hpc.Scheduler.Session.Internal;

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
