//------------------------------------------------------------------------------
// <copyright file="GenericServiceResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Message contract for generic service
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.GenericService
{
    using System.ServiceModel;

    /// <summary>
    ///   <para>Represents a response from a generic service.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Specify the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.GenericService.GenericServiceResponse" /> type when calling the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses()" /> method.</para>
    /// </remarks>
    [MessageContract(WrapperName = "GenericOperationResponse", WrapperNamespace = "http://hpc.microsoft.com/GenericService", IsWrapped = true)]
    public class GenericServiceResponse
    {
        /// <summary>
        ///   <para>Indicates the SOAP action to use for responses with a generic SOA service.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.GetResponses{TMessage}(System.String,System.String,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.String,System.String,System.Int32)" 
        /// /> 
        public const string Action = "http://hpc.microsoft.com/GenericService/IGenericService/GenericOperationResponse";

        /// <summary>
        /// Stores the response data as a string
        /// </summary>
        private string data;

        /// <summary>
        ///   <para>Gets or sets a string that contains the response data.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns a <see cref="System.String" /> that contains the response data.</para>
        /// </value>
        [MessageBodyMember(Name = "GenericOperationResult", Namespace = "http://hpc.microsoft.com/GenericService", Order = 0)]
        public string Data
        {
            get { return this.data; }
            set { this.data = value; }
        }
    }
}
