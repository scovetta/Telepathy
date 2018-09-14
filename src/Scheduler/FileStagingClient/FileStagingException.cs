//--------------------------------------------------------------------------
// <copyright file="FileStagingException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     Exceptions of this type are thrown by the API when there is a
//     a failure. An inner exception of any type derived from Exception
//     may provide more detail on the failure.
// </summary>
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace Microsoft.Hpc.Azure.FileStaging.Client
{
    /// <summary>
    /// Exceptions of this type are thrown by the API when there is a a failure. 
    /// An inner exception of any type derived from Exception may provide more 
    /// detail on the failure.
    /// </summary>
    public class FileStagingException : Exception
    {
        /// <summary>
        /// These member variables reflect those of InternalFaultDetail
        /// </summary>
        private string innerExceptionMessage;
        private FileStagingErrorCode errorCode;

        /// <summary>
        /// This constructor builds a FileStagingException based on the information within
        /// an InternalFaultDetail. Meant for internal use only.
        /// </summary>
        /// <param name="ex"></param>
        internal FileStagingException(InternalFaultDetail ex)
            : base(ex.Message)
        {
            this.innerExceptionMessage = ex.InnerExceptionMessage;
            this.errorCode = ex.ErrorCode;
        }

        /// <summary>
        /// This constructor builds a FileStagingException based on the information within
        /// an FaultException. Meant for internal use only.
        /// </summary>
        /// <param name="ex"></param>
        internal FileStagingException(FaultException ex)
            : base(ex.Message, ex.InnerException)
        {
            try
            {
                //
                // Try to get detail from the exception
                //
                MessageFault messageFault = ex.CreateMessageFault();
                if (messageFault.HasDetail)
                {
                    InternalFaultDetail detail = messageFault.GetDetail<InternalFaultDetail>();
                    this.innerExceptionMessage = detail.InnerExceptionMessage;
                    this.errorCode = detail.ErrorCode;
                }
                else
                {
                    this.errorCode = FileStagingErrorCode.UnknownFault;
                    this.innerExceptionMessage = string.Empty;
                }
            }
            catch
            {
                this.errorCode = FileStagingErrorCode.UnknownFault;
                this.innerExceptionMessage = string.Empty;
            }
        }

        /// <summary>
        /// This constructor builds a FileStagingException based on the information within
        /// a generic Exception. Meant for internal use only.
        /// </summary>
        /// <param name="ex"></param>
        internal FileStagingException(Exception ex)
            : base(ex.Message, ex.InnerException)
        {
            this.errorCode = FileStagingErrorCode.Unknown;
            this.innerExceptionMessage = string.Empty;
        }

        /// <summary>
        /// This constructor builds a FileStagingException based on the information within a generic 
        /// Exception, but where an error code has been provided. Meant for internal use only.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="errorCode"></param>
        internal FileStagingException(Exception ex, FileStagingErrorCode errorCode)
            : base(ex.Message, ex.InnerException)
        {
            this.errorCode = errorCode;
            this.innerExceptionMessage = string.Empty;
        }

        /// <summary>
        /// Retrieves the error code associated with the error
        /// </summary>
        public FileStagingErrorCode ErrorCode
        {
            get { return this.errorCode; }
        }

        /// <summary>
        /// Retrieves a message from the inner exception that caused the fault.
        /// </summary>
        public string InnerExceptionMessage
        {
            get { return this.innerExceptionMessage; }
        }
    }
}
