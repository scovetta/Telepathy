// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.GenericService
{
    using System.ServiceModel;

    /// <summary>
    ///   <para>Represents a request for a generic service.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Specify the 
    /// <see cref="GenericServiceRequest" /> type when calling the 
    /// "Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T} method.</para>
    /// </remarks>
    [MessageContract(WrapperName = "GenericOperation", WrapperNamespace = "http://hpc.microsoft.com/GenericService", IsWrapped = true)]
    public class GenericServiceRequest
    {
        /// <summary>
        ///   <para>Indicates the SOAP action to use for requests with a generic SOA service.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="BrokerClient{TContract}.SendRequest{TMessage}(TMessage,object,string)" />
        /// <seealso cref="GenericServiceResponse.Data" />
        /// <seealso cref="GenericServiceResponse.Action" />
        /// <seealso cref="BrokerClient{TContract}.SendRequest{TMessage}(TMessage,object,string,int)" />
        /// <seealso cref="BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Telepathy.Session.Internal.BrokerResponseStateHandler{TMessage},string,string,object,int)" 
        /// /> 
        public const string Action = "http://hpc.microsoft.com/GenericService/IGenericService/GenericOperation";

        /// <summary>
        /// Stores the request data as a string
        /// </summary>
        private string data;

        /// <summary>
        ///   <para>Gets or sets the request data.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns a <see cref="System.String" /> that contains the request data.</para>
        /// </value>
        [MessageBodyMember(Name = "args", Namespace = "http://hpc.microsoft.com/GenericService", Order = 0)]
        public string Data
        {
            get { return this.data; }
            set { this.data = value; }
        }
    }
}
