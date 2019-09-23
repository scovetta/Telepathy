// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System.ServiceModel;

    /// <summary>
    /// Describes the message which tells client how many responses will be returned
    /// </summary>
    [MessageContract]
    public class EndOfResponses
    {
        /// <summary>
        /// Number of responses
        /// </summary>
        private int count;

        /// <summary>
        /// Stores the reason of EndOfResponses
        /// </summary>
        private EndOfResponsesReason reason;

        /// <summary>
        /// The number of messages that will be returned
        /// </summary>
        [MessageBodyMember]
        public int Count
        {
            get
            {
                return this.count;
            }

            set
            {
                this.count = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether client is purged
        /// </summary>
        [MessageBodyMember]
        public EndOfResponsesReason Reason
        {
            get { return this.reason; }
            set { this.reason = value; }
        }
    }
}
