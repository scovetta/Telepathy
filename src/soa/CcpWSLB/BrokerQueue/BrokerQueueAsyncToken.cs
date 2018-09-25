//-----------------------------------------------------------------------------------
// <copyright file="BrokerQueueAsyncToken.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>the async token for the brokerqueue.</summary>
//-----------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage
{
    using System;

    /// <summary>
    /// the async token class.
    /// </summary>
    internal class BrokerQueueAsyncToken
    {
        #region private fields
        /// <summary>
        /// the broker queue.
        /// </summary>
        private BrokerQueue queueField;

        /// <summary>
        /// the async state related to the request.
        /// </summary>
        private object asyncStateField;

        /// <summary>
        /// the async token related with the persisted request.
        /// </summary>
        private object asyncTokenField;

        /// <summary>the disapathed number of the related request.</summary>
        private int dispatchNumberField;

        /// <summary>
        /// Stores the total try count
        /// </summary>
        private int tryCount;

        /// <summary>
        /// the persist id.
        /// </summary>
        private Guid persistIdField;
        #endregion

        /// <summary>
        /// Initializes a new instance of the BrokerQueueAsyncToken class, 
        /// </summary>
        /// <param name="persistId">the persist id.</param>
        /// <param name="asyncState">the async state.</param>
        /// <param name="asyncToken">the async toekn.</param>
        /// <param name="dispatchNumber">the dispatch number of the request.</param>
        public BrokerQueueAsyncToken(Guid persistId, object asyncState, object asyncToken, int dispatchNumber)
        {
            this.persistIdField = persistId;
            this.asyncStateField = asyncState;
            this.asyncTokenField = asyncToken;
            this.dispatchNumberField = dispatchNumber;
        }

        /// <summary>
        /// Gets the persist id.
        /// </summary>
        public Guid PersistId
        {
            get
            {
                return this.persistIdField;
            }
        }

        /// <summary>
        /// Gets or sets the broker queue.
        /// </summary>
        public BrokerQueue Queue
        {
            get
            {
                return this.queueField;
            }

            set
            {
                this.queueField = value;
            }
        }

        /// <summary>
        /// Gets or sets the async token object
        /// </summary>
        public object AsyncToken
        {
            get
            {
                return this.asyncTokenField;
            }

            set
            {
                this.asyncTokenField = value;
            }
        }

        /// <summary>
        /// Gets the async state object.
        /// </summary>
        public object AsyncState
        {
            get
            {
                return this.asyncStateField;
            }
        }

        /// <summary>
        /// Gets or sets the dispatched number of the related request message.
        /// </summary>
        internal int DispatchNumber
        {
            get
            {
                return this.dispatchNumberField;
            }

            set
            {
                this.dispatchNumberField = value;
            }
        }

        /// <summary>
        /// Gets or sets the total try count of this broker queue item
        /// </summary>
        internal int TryCount
        {
            get { return this.tryCount; }
            set { this.tryCount = value; }
        }
    }
}
