// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.GenericService
{
    using System.ServiceModel;

    /// <summary>
    ///   <para>Represents a response from a generic service.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Specify the 
    /// <see cref="GenericServiceResponse" /> type when calling the 
    /// <see cref="BrokerClient{TContract}.GetResponses()" /> method.</para>
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
        /// <seealso cref="BrokerClient{TContract}.GetResponses{TMessage}(string,string,int)" />
        /// <seealso cref="BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Telepathy.Session.Internal.BrokerResponseHandler{TMessage},string,string,int)" 
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
