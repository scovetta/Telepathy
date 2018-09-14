//--------------------------------------------------------------------------
// <copyright file="InternalFaultDetail.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This class is used internally to add detail to FaultExceptions that 
//     explain the nature of the fault.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class is used internally to add detail to FaultExceptions that explain the nature of the fault.
    /// </summary>
    [DataContract]
    public class InternalFaultDetail
    {
        private string message, innerExceptionMessage;
        private FileStagingErrorCode errorCode;

        /// <summary>
        /// This default constructor is needed so that the object can be serialized.
        /// </summary>
        public InternalFaultDetail()
        {
            this.message = string.Empty;
            this.innerExceptionMessage = string.Empty;
            this.errorCode = FileStagingErrorCode.Unknown;
        }

        /// <summary>
        /// Constructs a InternalFaultException with a specific message and FileStaging-specific error code
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errorCode"></param>
        internal InternalFaultDetail(string message, FileStagingErrorCode errorCode)
        {
            this.message = message;
            this.innerExceptionMessage = string.Empty;
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Constructs a InternalFaultException with a specific message, FileStaging-specific error code, 
        /// and detail on the inner exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errorCode"></param>
        /// <param name="innerException"></param>
        internal InternalFaultDetail(string message, FileStagingErrorCode errorCode, Exception innerException)
        {
            this.message = message;
            this.errorCode = errorCode;

            if (innerException != null)
            {
                this.innerExceptionMessage = innerException.Message;
            }
            else
            {
                this.innerExceptionMessage = string.Empty;
            }
        }

        /// <summary>
        /// Returns the error message associated with the fault. A set method is provided
        /// so that the encompassing object can be deserialized.
        /// </summary>
        [DataMember]
        public string Message
        {
            get { return this.message; }
            set { this.message = value; }
        }

        /// <summary>
        /// Returns the error message associated with the fault's inner exception. A set method
        /// is provided so that the encompassing object can be deserialized.
        /// </summary>
        [DataMember]
        public string InnerExceptionMessage
        {
            get { return this.innerExceptionMessage; }
            set { this.innerExceptionMessage = value; }
        }

        /// <summary>
        /// Returns the FileStaging-specific error code associated with the fault. A set method is provided
        /// so that the encompassing object can be deserialized.
        /// </summary>
        [DataMember]
        public FileStagingErrorCode ErrorCode
        {
            get { return this.errorCode; }
            set { this.errorCode = value; }
        }
    }
}
