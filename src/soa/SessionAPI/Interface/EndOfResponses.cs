//------------------------------------------------------------------------------
// <copyright file="EndOfResponses.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Message contract that represents the end of response messages
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Session.Interface;

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
