// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    /// <summary>
    /// response event args.
    /// </summary>
    internal class ResponseEventArgs : BrokerQueueEventArgs
    {
        /// <summary>state object of the registered response calllback.</summary>
        private object stateField;

        /// <summary>
        /// Initializes a new instance of the ResponseEventArgs class.
        /// </summary>
        /// <param name="state">the registered response callback state object.</param>
        /// <param name="queue">the broker queue.</param>
        public ResponseEventArgs(object state, BrokerQueue queue)
            : base(queue)
        {
            this.stateField = state;
        }

        /// <summary>
        /// Gets the state object of the registered response callback.
        /// </summary>
        public object State
        {
            get
            {
                return this.stateField;
            }
        }
    }
}
