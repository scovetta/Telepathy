//------------------------------------------------------------------------------
// <copyright file="BlobTransferCallbackException.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Base exception class for exceptions to deal with exceptions thrown by callbacks.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.DataMovement.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base exception class for exceptions to wrap exceptions thrown by callbacks.
    /// The exception really thrown out by callbacks can be find in the InnerException of this class.
    /// </summary>
    [Serializable]
    public class BlobTransferCallbackException : Exception
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
        /// Initializes a new instance of the <see cref="BlobTransferCallbackException" /> class.
        /// </summary>
        /// <param name="message">Error message to indicate that some exception was thrown out from callback.</param>
        /// <param name="ex">Exception thrown by callback.</param>
        public BlobTransferCallbackException(string message, Exception ex)
            : base(message, ex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransferCallbackException" /> class.
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Streaming context.</param>
        private BlobTransferCallbackException(
            SerializationInfo info, 
            StreamingContext context)
            : base(info, context)
        {
            // We don't have anything to deserialize for now, so just don't check version here.
            // In the future, if we need to add some new field in this exception, we should first check the version
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

            base.GetObjectData(info, context);
        }
    }
}
