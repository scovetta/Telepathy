// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    /// <summary>
    /// Put responses success event args
    /// </summary>
    class PutResponsesSuccessEventArgs : BrokerQueueEventArgs
    {
        /// <summary> number of responses successfully put. </summary>
        private int responsesCountField;

        /// <summary> number of fault responses. </summary>
        private int faultResponsesCountField;

        /// <summary>
        /// Initializes a new instance of the PutResponsesSuccessEventArgs class.
        /// </summary>
        /// <param name="successResponsesCount"> number of responses successfully put</param>
        /// <param name="faultResponsesCount"> number of fault responses</param>
        /// <param name="queue">the broker queue.</param>
        public PutResponsesSuccessEventArgs(int responsesCount, int faultResponsesCount, BrokerQueue queue)
            : base(queue)
        {
            this.responsesCountField = responsesCount;
            this.faultResponsesCountField = faultResponsesCount;
        }

        /// <summary>
        /// Gets the number of responses that are successfully put.
        /// </summary>
        public int ResponsesCount
        {
            get
            {
                return this.responsesCountField;
            }
        }

        /// <summary>
        /// Gets the number of fault responses
        /// </summary>
        public int FaultResponsesCount
        {
            get
            {
                return this.faultResponsesCountField;
            }
        }
    }
}