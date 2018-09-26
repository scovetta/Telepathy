//-----------------------------------------------------------------------
// <copyright file="MessageRetryException.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Represent retry limit exceeded and wrap the last exception</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    ///   <para>Indicates that the broker exceeded the limit for the number of times the broker should retry a request.</para>
    /// </summary>
    /// <remarks>
    ///   <para>The Microsoft HPC Pack API converts all SOAP faults that 
    /// relate to infrastructure rather than to the application to exceptions. The  
    /// <see cref="System.ServiceModel.FaultException{T}" /> object with a type parameter of 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.RetryOperationError" /> is converted to a 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.RetryOperationException" /> object. SOA clients for Microsoft HPC Pack should handle such exceptions.</para> 
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.RetryOperationError" />
    /// <seealso cref="System.ServiceModel.FaultException{T}" />
    [Serializable]
    public class RetryOperationException : Exception
    {
        /// <summary>
        /// Stores the reason
        /// </summary>
        private string reason;

        /// <summary>
        /// Initializes a new instance of the RetryOperationException class
        /// </summary>
        /// <param name="reason">indicating the retry reason</param>
        internal RetryOperationException(string reason)
            : base(SR.RetryOperationExceptionMessage)
        {
            this.reason = reason;
        }

        /// <summary>
        /// Initializes a new instance of the RetryOperationException class
        /// </summary>
        /// <param name="message">indicating the exception message</param>
        /// <param name="reason">indicating the retry reason</param>
        internal RetryOperationException(string message, string reason)
            : base(message)
        {
            this.reason = reason;
        }

        /// <summary>
        ///   <para>Gets the specific reason that the error in retrying a request occurred.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> that indicates the specific reason that the error in retrying a request occurred.</para>
        /// </value>
        public string Reason
        {
            get { return this.reason; }
        }
    }
}