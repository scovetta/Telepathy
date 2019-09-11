//-----------------------------------------------------------------------
// <copyright file="AuthenticationFailure.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Fault code for authentication failure</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///   <para>Indicates that the credentials that were specified for the session did not have sufficient privileges for an operation.</para>
    /// </summary>
    /// <remarks>
    ///   <para>A client application that communicates with the service through the 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class specific to Microsoft HPC Pack 2008 R2 or later should catch this failure directly. </para> 
    ///   <para>A client application that communicates with the service by using the method 
    /// that was available in Windows HPC Server 2008 of directly using the client proxy should catch a  
    /// <see cref="System.ServiceModel.FaultException{T}" /> with a type parameter of 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.AuthenticationFailure" />. Also, you must define a fault contract in your client side service contract by using the following attribute:</para> 
    ///   <code>[FaultContract(typeof(AuthenticationFailure), Action = AuthenticationFailure.Action)]</code>
    /// </remarks>
    /// <seealso cref="System.ServiceModel.FaultException{T}" />
    [DataContract(Namespace = "http://hpc.microsoft.com/session/")]
    [Serializable]
    public sealed class AuthenticationFailure
    {
        /// <summary>
        ///   <para>Specifies the value of the SOAP action for the error message.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.AuthenticationFailure.Action" /> static field is used to write the fault contract needed for the service contract.</para> 
        /// </remarks>
        /// <seealso cref="System.ServiceModel.FaultException.Action" />
        public const string Action = "http://hpc.microsoft.com/session/AuthenticationFailure";

        /// <summary>
        /// Stores the user name of the rejected user
        /// </summary>
        [DataMember]
        private string userName;

        /// <summary>
        /// Initializes a new instance of the AuthenticationFailure class
        /// </summary>
        /// <param name="userName">indicating the user name</param>
        public AuthenticationFailure(string userName)
        {
            this.userName = userName;
        }

        /// <summary>
        ///   <para>Gets the user name of the account that did not have sufficient privileges for the operation.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.String" /> that contains the user name of the account that did not have sufficient privileges for the operation.</para>
        /// </value>
        public string UserName
        {
            get { return this.userName; }
        }
    }
}
