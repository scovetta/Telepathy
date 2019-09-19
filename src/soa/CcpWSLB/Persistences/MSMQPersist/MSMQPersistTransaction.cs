// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#if  MSMQ
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage.MSMQ
{
    using System;
    using System.Collections.Generic;
    using System.Messaging;
    using System.Text;

    /// <summary>
    /// the MSMQ transaction wrapper.
    /// </summary>
    internal class MSMQPersistTransaction : IPersistTransaction
    {
        /// <summary>the parent persist object.</summary>
        private MSMQPersist parentPersistField;

        /// <summary>
        /// Initializes a new instance of the MSMQPersistTransaction class.
        /// </summary>
        /// <param name="persist">the MSMQ queue persistence.</param>
        public MSMQPersistTransaction(MSMQPersist persist)
        {
            if (persist == null)
            {
                throw new ArgumentNullException("persist");
            }

            this.parentPersistField = persist;
        }

        /// <summary>
        /// Gets the state of the persist transaction.
        /// </summary>
        public PersistTransactionStatus Status
        {
            get
            {
                PersistTransactionStatus status = PersistTransactionStatus.Initialized;
                switch (this.parentPersistField.RequestsTransactionStatus)
                {
                    case MessageQueueTransactionStatus.Aborted:
                        status = PersistTransactionStatus.Aborted;
                        break;
                    case MessageQueueTransactionStatus.Committed:
                        status = PersistTransactionStatus.Committed;
                        break;
                    case MessageQueueTransactionStatus.Pending:
                        status = PersistTransactionStatus.Pending;
                        break;
                }

                return status;
            }
        }

        /// <summary>
        /// commit the persist transaction
        /// </summary>
        public void Commit()
        {
            this.parentPersistField.CommitRequestTransaction();
        }

        /// <summary>
        /// Abort the persist transaction
        /// </summary>
        public void Abort()
        {
            this.parentPersistField.AbortRequestTransaction();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
#endif