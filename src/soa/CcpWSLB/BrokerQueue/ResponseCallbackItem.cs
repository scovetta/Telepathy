// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BrokerStorage
{
    using System;
    using System.Threading;
    using System.ServiceModel.Channels;

    /// <summary>
    /// the responses callback item.
    /// </summary>
    internal class ResponseCallbackItem
    {
        #region private fields
        /// <summary>the responses callback.</summary>
        private BrokerQueueCallback responsesCallbackField;

        /// <summary>the message version.</summary>
        private MessageVersion messageVersionField;

        /// <summary>the response callback state.</summary>
        private object callbackStateField;

        /// <summary>the expected responses count that should return by this registered response callback.</summary>
        private long expectedResponsesCountField;

        /// <summary>the filter for the responses.</summary>
        private ResponseActionFilter responseFilterField;
        #endregion

        /// <summary>
        /// Initializes a new instance of the ResponseCallbackItem class.
        /// </summary>
        /// <param name="callback">the response callback.</param>
        /// <param name="filter">the filter for the responses.</param>
        /// <param name="expectResponseCount">the expected responses count that should returned through this callback.</param>
        /// <param name="callbackState">the callback state object.</param>
        /// <param name="needRemove">a value indicating whether need remove the fetched responses.</param>
        public ResponseCallbackItem(BrokerQueueCallback callback, MessageVersion messageVersion, ResponseActionFilter filter, long expectResponseCount, object callbackState)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            this.responsesCallbackField = callback;
            this.messageVersionField = messageVersion;
            this.responseFilterField = filter;
            this.expectedResponsesCountField = expectResponseCount;
            this.callbackStateField = callbackState;
        }

        /// <summary>
        /// Gets the response callback method.
        /// </summary>
        public BrokerQueueCallback Callback
        {
            get
            {
                return this.responsesCallbackField;
            }
        }

        public MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersionField;
            }
        }

        /// <summary>
        /// Gets the response callback state.
        /// </summary>
        public object CallbackState
        {
            get
            {
                return this.callbackStateField;
            }
        }

        /// <summary>
        /// Gets the expected response count of this callback.
        /// </summary>
        public long ExpectedResponseCount
        {
            get
            {
                return this.expectedResponsesCountField;
            }
        }

        /// <summary>
        /// Gets the response filter.
        /// </summary>
        public ResponseActionFilter ResponseFilter
        {
            get
            {
                return this.responseFilterField;
            }
        }

        /// <summary>
        /// Decrement the expected response count.
        /// </summary>
        /// <returns>return the reminding response count that need return through this resonse callback.</returns>
        public long DecrementResponseCount()
        {
            return Interlocked.Decrement(ref this.expectedResponsesCountField);
        }
    }
}
