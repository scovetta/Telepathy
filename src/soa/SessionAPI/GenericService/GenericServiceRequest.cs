// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.GenericService
{
    using System.ServiceModel;

    /// <summary>
    ///   <para>Represents a request for a generic service.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Specify the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.GenericService.GenericServiceRequest" /> type when calling the 
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
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.GenericService.GenericServiceResponse.Data" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.GenericService.GenericServiceResponse.Action" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.String,System.String,System.Object,System.Int32)" 
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
