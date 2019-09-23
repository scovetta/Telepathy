// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System;

    /// <summary>
    /// the event args class.
    /// </summary>
    internal class BrokerQueueEventArgs : EventArgs
    {
        /// <summary>
        /// the broker queue.
        /// </summary>
        private BrokerQueue queueField;

        /// <summary>
        /// Initializes a new instance of the BrokerQueueEventArgs class.
        /// </summary>
        /// <param name="queue">the broker queue.</param>
        public BrokerQueueEventArgs(BrokerQueue queue)
        {
            this.queueField = queue;
        }

        /// <summary>
        /// Gets the broker queue.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "May need in the future")]
        public BrokerQueue BrokerQueue
        {
            get
            {
                return this.queueField;
            }
        }
    }
}
