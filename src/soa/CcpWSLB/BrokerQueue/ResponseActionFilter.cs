// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System;
    using System.ServiceModel.Channels;

    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// the response action filter.
    /// </summary>
    public class ResponseActionFilter
    {
        /// <summary>
        /// the action should be included in the qualified messasge header.
        /// </summary>
        private string actionField;

        /// <summary>
        /// Initializes a new instance of the ResponseActionFilter class.
        /// </summary>
        /// <param name="action">the action should be included in the qualified messasge header.</param>
        public ResponseActionFilter(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentNullException("action");
            }

            this.actionField = action;
        }

        /// <summary>
        /// a method to judge whether the passed in response is qualified.
        /// </summary>
        /// <param name="response">the response message.</param>
        /// <returns>a value indicating whether the input response message is qualified.</returns>
        public bool IsQualified(Message response)
        {
            if (response == null)
            {
                return false;
            }

            bool isQualified = false;
            int index = response.Headers.FindHeader(Constant.ActionHeaderName, Constant.HpcHeaderNS);
            if (index >= 0)
            {
                isQualified = this.actionField.Equals(response.Headers.GetHeader<string>(index), StringComparison.Ordinal);
            }

            return isQualified;
        }
    }
}
