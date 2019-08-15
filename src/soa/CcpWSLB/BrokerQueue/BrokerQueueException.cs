//-----------------------------------------------------------------------
// <copyright file="BrokerQueueException.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>define the broker queue exception.</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    /// <summary>
    /// the error code definition for the broker queue exception.
    /// </summary>
    public enum BrokerQueueErrorCode
    {
        /// <summary>
        /// no exception.
        /// </summary>
        S_SUCCEED = 0,

        /// <summary>
        /// the broker queue hit an unknown exception.
        /// </summary>
        E_BQ_UNKOWN_ERROR = 1000,

        /// <summary>
        /// the persist name does not exist in the app.config
        /// </summary>
        E_BQ_CONFIG_PERSIST_NOT_EXIST = 1100,

        /// <summary>
        /// can not find the assemply specified by the app.config
        /// </summary>
        E_BQ_CONFIG_PERSIST_ASSEMBLY_NOT_FOUND = 1101,

        /// <summary>
        /// can not create the persist factory instance.
        /// </summary>
        E_BQ_CONFIG_PERSIST_FACTORY_ERROR = 1102,

        /// <summary>
        /// can not initialize the persist instance
        /// </summary>
        E_BQ_PERSIST_INIT_ERROR = 1200,

        /// <summary>
        /// the total messasge number or message size exceed the storage limitation.
        /// </summary>
        E_BQ_PERSIST_EXCEEDLIMIT = 1201,

        /// <summary>
        /// the transaction can not finish with the timeout period.
        /// </summary>
        E_BQ_PERSIST_TRANSACTION_TIMEOUT = 1202,

        /// <summary>
        /// writing the message to the persistence timeout.
        /// </summary>
        E_BQ_PERSIST_OPERATION_TIMEOUT = 1203,

        /// <summary>
        /// fail to write the message to the persistence
        /// </summary>
        E_BQ_PERSIST_WRITING_FAIL = 1204,

        /// <summary>
        /// fail to serialize the message before writing it to the persistence.
        /// </summary>
        E_BQ_PERSIST_SERIALIZE_FAIL = 1205,

        /// <summary>
        /// can not deserialize the message from the stream.
        /// </summary>
        E_BQ_PERSIST_DESERIALIZE_FAIL = 1206,

        /// <summary>
        /// fail to read the message from the persistence.
        /// </summary>
        E_BQ_PERSIST_READING_FAIL = 1207,

        /// <summary>
        /// can not find the original request.
        /// </summary>
        E_BQ_PERSIST_ORIGINALREQUEST_NOT_FOUND = 1210,

        /// <summary>
        /// can not remove the original request.
        /// </summary>
        E_BQ_PERSIST_REMOVE_ORIGINALREQUEST_FAIL = 1211,

        /// <summary>
        /// insufficient storage to persist the messages.
        /// </summary>
        E_BQ_PERSIST_STORAGE_INSUFFICIENT = 1212,

        /// <summary>
        /// the storage failure.
        /// </summary>
        E_BQ_PERSIST_STORAGE_FAIL = 1213,

        /// <summary>
        /// the storage service is not available.
        /// </summary>
        E_BQ_PERSIST_STORAGE_NOTAVAILABLE = 1214,

        /// <summary>
        /// the callback id is invalide.
        /// </summary>
        E_BQ_INVALID_CALLBACKID = 1300,

        /// <summary>
        /// the flush error for that the message count is not correct.
        /// </summary>
        E_BQ_FLUSH_MESSAGECOUNT_MISMATCH = 1301,

        /// <summary>
        /// the original request message is not in processing when receive a response.
        /// </summary>
        E_BQ_ORIGINALREQUEST_NOT_INPROCESSING = 1302,

        /// <summary>
        /// registered too many 
        /// </summary>
        E_BQ_CALLBACK_EXEEDLIMIT = 1303,

        /// <summary>
        /// there are pending registered response callback.
        /// </summary>
        E_BQ_CALLBACK_HAVE_PENDING_CALLBACK = 1304,

        /// <summary>
        /// the requests not flushed.
        /// </summary>
        E_BQ_CALLBACK_NO_FLUSHED_REQUEST = 1305,

        /// <summary>
        /// the user name doesnot match.
        /// </summary>
        E_BQ_USER_NOT_MATCH = 1306,

        /// <summary>
        /// for V2 compatible model, if the memory is exhausted.
        /// </summary>
        E_BQ_OUT_OF_MEMORY = 1400,

        /// <summary>
        /// the broker queue is closed.
        /// </summary>
        E_BQ_STATUS_CLOSED = 1500,

        /// <summary>
        /// attach to a session without a broker queue is not allowed
        /// </summary>
        E_BQ_ATTACH_FAIL_NO_BROKER_QUEUE = 1600,

        /// <summary>
        /// create a broker with an existing broker queue is not allowed
        /// </summary>
        E_BQ_CREATE_BROKER_FAIL_EXISTING_BROKER_QUEUE = 1700,
    }

    /// <summary>
    /// the broker queue exception definition.
    /// </summary>
    [Serializable]
    public class BrokerQueueException : Exception
    {
        /// <summary>
        /// the error code tag name for SerializationInfo.
        /// </summary>
        private const string BrokerQueueError = "BQErrorCode";

        /// <summary>
        /// the error code related with this exception.
        /// </summary>
        private int errorCode;

        /// <summary>
        /// Initializes a new instance of the BrokerQueueException class.
        /// </summary>
        public BrokerQueueException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BrokerQueueException class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public BrokerQueueException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BrokerQueueException class with a specified
        /// error message.
        /// </summary>
        /// <param name="errorCode">the error code for the error</param>
        /// <param name="message">The message that describes the error</param>
        public BrokerQueueException(int errorCode, string message)
            : base(message)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the BrokerQueueException class with a specified
        /// error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public BrokerQueueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BrokerQueueException class with a specified
        /// error message and a reference to the inner exception that is the cause of
        /// this exception.
        /// </summary>
        /// <param name="errorCode">the error code for the exception.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public BrokerQueueException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the BrokerQueueException class with serialized
        /// data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected BrokerQueueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }

            this.errorCode = (int)info.GetValue(BrokerQueueError, typeof(int));
        }

        /// <summary>
        /// Gets the error code for this exception
        /// </summary>
        public int ErrorCode
        {
            get
            {
                if (0 == this.errorCode)
                {
                    this.errorCode = (int)BrokerQueueErrorCode.E_BQ_UNKOWN_ERROR;
                }

                return this.errorCode;
            }
        }

        /// <summary>
        /// Populates a System.Runtime.Serialization.SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo to populate with data.</param>
        /// <param name="context">The destination (see System.Runtime.Serialization.StreamingContext) for this serialization.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
            info.AddValue(BrokerQueueError, this.errorCode);
        }
    }
}
