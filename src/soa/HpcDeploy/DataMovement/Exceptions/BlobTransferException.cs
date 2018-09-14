//------------------------------------------------------------------------------
// <copyright file="BlobTransferException.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Base exception class for exceptions thrown by BlobTransfer.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.DataMovement.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base exception class for exceptions thrown by BlobTransfer.
    /// </summary>
    [Serializable]
    public class BlobTransferException : Exception
    {
        /// <summary>
        /// Version of current BlobTransferException serialization format.
        /// </summary>
        private const int ExceptionVersion = 1;

        /// <summary>
        /// Serialization field name for Version.
        /// </summary>
        private const string VersionFieldName = "Version";

        /// <summary>
        /// Serialization field name for ErrorCode.
        /// </summary>
        private const string ErrorCodeFieldName = "ErrorCode";

        /// <summary>
        /// BlobTransfer error code.
        /// </summary>
        private BlobTransferErrorCode errorCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferException" /> class.
        /// </summary>
        /// <param name="errorCode">BlobTransfer error code.</param>
        public BlobTransferException(BlobTransferErrorCode errorCode)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferException" /> class.
        /// </summary>
        /// <param name="errorCode">BlobTransfer error code.</param>
        /// <param name="message">Exception message.</param>
        public BlobTransferException(
            BlobTransferErrorCode errorCode, 
            string message)
            : base(message)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferException" /> class.
        /// </summary>
        /// <param name="errorCode">BlobTransfer error code.</param>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public BlobTransferException(
            BlobTransferErrorCode errorCode, 
            string message, 
            Exception innerException)
            : base(message, innerException)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferException" /> class.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        private BlobTransferException(
            SerializationInfo info, 
            StreamingContext context)
            : base(info, context)
        {
            int exceptionVersion = info.GetInt32(VersionFieldName);

            if (exceptionVersion >= 1)
            {
                this.errorCode = (BlobTransferErrorCode)info.GetInt32(ErrorCodeFieldName);
            }
        }

        /// <summary>
        /// Gets the detailed error code.
        /// </summary>
        /// <value>The error code of the exception.</value>
        public BlobTransferErrorCode ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }

        /// <summary>
        /// Serializes the exception.
        /// </summary>
        /// <param name="info">Serialization info object.</param>
        /// <param name="context">Streaming context.</param>
        public override void GetObjectData(
            SerializationInfo info, 
            StreamingContext context)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue(VersionFieldName, ExceptionVersion);
            info.AddValue(ErrorCodeFieldName, this.errorCode);

            base.GetObjectData(info, context);
        }
    }
}
