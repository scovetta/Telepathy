// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.GenericService
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    ///   <para>Implements a client proxy for a generic service.</para>
    /// </summary>
    /// <remarks>
    ///   <para>A client application that communicates with the service by using the method that 
    /// was available in Windows HPC Server 2008 of directly using the client proxy should use this class.</para>
    /// </remarks>
    public class GenericServiceClient : ClientBase<IGenericServiceAsync>, IGenericServiceAsync
    {
        /// <summary>
        /// Initializes a new instance of the GenericServiceClient class
        /// </summary>
        /// <param name="binding">indicating the binding</param>
        /// <param name="remoteAddress">indicating the remote address</param>
        public GenericServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        /// <summary>
        ///   <para>A complete generic operation.</para>
        /// </summary>
        /// <param name="args">
        ///   <para>The arguments for the operation.</para>
        /// </param>
        /// <returns>
        ///   <para>Returns a <see cref="System.String" /> that representtt the results of the operation.</para>
        /// </returns>
        /// <remarks>
        ///   <para>This method begins and ends the operation.</para>
        /// </remarks>
        public string GenericOperation(string args)
        {
            return this.Channel.EndGenericOperation(this.Channel.BeginGenericOperation(args, null, null));
        }

        /// <summary>
        ///   <para>Begins a generic operation.</para>
        /// </summary>
        /// <param name="args">
        ///   <para>The arguments for the generic operation.</para>
        /// </param>
        /// <param name="callback">
        ///   <para>The method that receives the callback on completion.</para>
        /// </param>
        /// <param name="state">
        ///   <para>The current state of the client object.</para>
        /// </param>
        /// <returns>
        ///   <para>Returns an <see cref="System.IAsyncResult" /> object that contains the result of the operation.</para>
        /// </returns>
        public IAsyncResult BeginGenericOperation(string args, AsyncCallback callback, object state)
        {
            return this.Channel.BeginGenericOperation(args, callback, state);
        }

        /// <summary>
        ///   <para>Ends a generic operation.</para>
        /// </summary>
        /// <param name="result">
        ///   <para>The result of the operation.</para>
        /// </param>
        /// <returns>
        ///   <para>Returns a <see cref="System.String" /> that represents the result of the operation.</para>
        /// </returns>
        public string EndGenericOperation(IAsyncResult result)
        {
            return this.Channel.EndGenericOperation(result);
        }
    }
}
