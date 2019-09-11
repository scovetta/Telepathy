//------------------------------------------------------------------------------
// <copyright file="NonTerminatingError.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Represents a non-terminating error
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///   <para>Indicates that an error connected to the retrying requests by the broker occurred, when the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.RetryOperationError" /> class  is specified as the type parameter for a 
    /// <see cref="System.ServiceModel.FaultException{T}" /> object.</para>
    /// </summary>
    /// <remarks>
    ///   <para>A SOA service should generate a 
    /// <see cref="System.ServiceModel.FaultException{T}" /> object with type parameter of 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.RetryOperationError" /> to indicate that the broker should retry the request.</para>
    ///   <para>When a SOA client that communicates with the service using the Microsoft HPC Pack model receives a 
    /// <see cref="System.ServiceModel.FaultException{T}" /> object with type parameter of 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.RetryOperationError" />, the exception indicates that the broker exceeded the limit for the number of times the broker should retry a request.</para> 
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.RetryOperationException" />
    /// <seealso cref="System.ServiceModel.FaultException{T}" />
    [DataContract(Namespace = "http://hpc.microsoft.com/session/")]
    [Serializable]
    public sealed class RetryOperationError
    {
        /// <summary>
        /// Stores the reason
        /// </summary>
        [DataMember]
        private string reason;

        /// <summary>
        /// Stores the retry count
        /// </summary>
        [DataMember]
        private int retryCount;

        /// <summary>
        /// Stores the task id that associate with the last failure
        /// </summary>
        [DataMember(IsRequired = false)]
        private int lastFailedServiceId;

        /// <summary>
        ///   <para>Specifies the value of the SOAP action for the error message.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="System.ServiceModel.FaultException.Action" />
        public const string Action = "http://hpc.microsoft.com/session/RetryOperationError";

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.RetryOperationError" /> class with the specified reason.</para>
        /// </summary>
        /// <param name="reason">
        ///   <para>A string that indicates the specific reason that the error in retrying a request occurred.</para>
        /// </param>
        public RetryOperationError(string reason)
        {
            this.reason = reason;
        }

        /// <summary>
        /// Initializes a new instance of the RetryOperationError class
        /// </summary>
        /// <param name="reason">the detail information, the exception must be Serializable</param>
        /// <param name="retryCount">the retry count</param>
        /// <param name="lastFailedServiceId">indicating the task id that associate with the last failure</param>
        internal RetryOperationError(string reason, int retryCount, int lastFailedServiceId)
        {
            this.reason = reason;
            this.retryCount = retryCount;
            this.lastFailedServiceId = lastFailedServiceId;
        }

        /// <summary>
        ///   <para>Gets the specific reason that the error in retrying a request occurred.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> that indicates the specific reason that the error in retrying a request occurred.</para>
        /// </value>
        public string Reason
        {
            get
            {
                return this.reason;
            }
        }

        /// <summary>
        ///   <para>Gets or set the number of times the broker should retry a request.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Int32" /> that indicates the number of times the broker should retry a request.</para>
        /// </value>
        public int RetryCount
        {
            get
            {
                return this.retryCount;
            }
            set
            {
                this.retryCount = value;
            }
        }

        /// <summary>
        ///   <para>Gets the task identifier for the service that is associated with the last error.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="System.Int32" /> that indicates the task identifier of the service that is associated with the last error.</para>
        /// </value>
        public int LastFailedServiceId
        {
            get { return this.lastFailedServiceId; }

            // Broker will set this parameter if it is thrown by user
            set { this.lastFailedServiceId = value; }
        }
    }
}
