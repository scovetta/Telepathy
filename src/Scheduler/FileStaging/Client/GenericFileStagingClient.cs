//--------------------------------------------------------------------------
// <copyright file="GenericFileStagingClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     Generic FileStaging client.  This is for communication between
//     FileStagingProxys(SchedulerFileStagingProxy and AzureFileStagingProxy)
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// Generic file staging client
    /// </summary>
    public class GenericFileStagingClient : ClientBase<IFileStagingGenericRouter>, IFileStagingGenericRouter
    {
        /// <summary>
        /// Get logical name of Azure node, which this client connects to.
        /// </summary>
        public string LogicalNodeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the GenericFileStagingClient class
        /// </summary>
        /// <param name="binding">binding information</param>
        /// <param name="remoteAddress">address of target file staging proxy</param>
        /// <param name="logicalNodeName">logical name of Azure node</param>
        public GenericFileStagingClient(Binding binding, EndpointAddress remoteAddress, string logicalNodeName) :
            this(binding, remoteAddress)
        {
            this.LogicalNodeName = logicalNodeName;
        }

        /// <summary>
        /// Initializes a new instance of the GenericFileStagingClient class
        /// </summary>
        /// <param name="binding">binding information</param>
        /// <param name="remoteAddress">address of target file staging proxy</param>
        public GenericFileStagingClient(Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        /// <summary>
        /// Standard operation contract for request/reply
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        public Message ProcessMessage(Message request)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
            return this.EndProcessMessage(this.BeginProcessMessage(request, null, null));
        }

        /// <summary>
        /// Begin method for ProcessMessage
        /// </summary>
        /// <param name="request">request message</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        public IAsyncResult BeginProcessMessage(Message request, AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginProcessMessage(request, callback, asyncState);
        }

        /// <summary>
        /// End method for ProcessMessage
        /// </summary>
        /// <param name="ar">async result</param>
        /// <returns>reply message</returns>
        public Message EndProcessMessage(IAsyncResult ar)
        {
            return this.Channel.EndProcessMessage(ar);
        }
    }
}
