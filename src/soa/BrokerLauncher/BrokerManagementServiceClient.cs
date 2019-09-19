// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    /// <summary>
    /// WCF client for broker management service
    /// </summary>
    internal class BrokerManagementServiceClient : ClientBase<IBrokerManagementService>, IBrokerManagementService
    {
        /// <summary>
        /// Initializes a new instance of the BrokerManagementServiceClient class
        /// </summary>
        /// <param name="binding">indicating the binding</param>
        /// <param name="remoteAddress">indicating the remote address</param>
        public BrokerManagementServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

        /// <summary>
        /// Ask to close the broker
        /// </summary>
        /// <param name="suspended">indicating whether the broker is asked to be suspended or closed</param>
        public void CloseBroker(bool suspended)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
            IAsyncResult result = this.Channel.BeginCloseBroker(suspended, null, null);
            this.Channel.EndCloseBroker(result);
        }

        /// <summary>
        /// Async version: Ask to close the broker
        /// </summary>
        /// <param name="suspended">indicating whether the broker is asked to be suspended or closed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State</param>
        /// <returns>The IAsyncResult instance</returns>
        public IAsyncResult BeginCloseBroker(bool suspended, AsyncCallback callback, object state)
        {
            return this.Channel.BeginCloseBroker(suspended, callback, state);
        }

        /// <summary>
        /// Finish CloseBroker operation
        /// </summary>
        /// <param name="result">The IAsyncResult</param>
        public void EndCloseBroker(IAsyncResult result)
        {
            this.Channel.EndCloseBroker(result);
        }

        /// <summary>
        /// Ask broker to initialize
        /// </summary>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        public BrokerInitializationResult Initialize(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
            IAsyncResult result = this.Channel.BeginInitialize(startInfo, brokerInfo, null, null);
            return this.Channel.EndInitialize(result);
        }

        /// <summary>
        /// Async version to ask broker to initialize
        /// </summary>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        /// <param name="clusterEnvs">indicating the cluster envs</param>
        /// <param name="callback">indicating the callback</param>
        /// <param name="state">indicating the state</param>
        public IAsyncResult BeginInitialize(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo, AsyncCallback callback, object state)
        {
            return this.Channel.BeginInitialize(startInfo, brokerInfo, callback, state);
        }

        /// <summary>
        /// Operation to receive session info for async version of Initialize
        /// </summary>
        /// <param name="result">indicating the async result</param>
        /// <returns>returns session info</returns>
        public BrokerInitializationResult EndInitialize(IAsyncResult result)
        {
            return this.Channel.EndInitialize(result);
        }

        /// <summary>
        /// Attach to the broker
        /// broker would throw exception if it does not allow client to attach to it
        /// </summary>
        public void Attach()
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
            this.EndAttach(this.BeginAttach(null, null));
        }

        /// <summary>
        /// Async version to attach to the broker
        /// broker would throw exception if it does not allow client to attach to it
        /// </summary>
        public IAsyncResult BeginAttach(AsyncCallback callback, object state)
        {
            return this.Channel.BeginAttach(callback, state);
        }

        /// <summary>
        /// Operation to finish attach
        /// </summary>
        /// <param name="result">indicating the async result</param>
        public void EndAttach(IAsyncResult result)
        {
            this.Channel.EndAttach(result);
        }

        /// <summary>
        /// Work around for WCF ServiceChannel leak bug
        /// Creates the channel with OperationContext=null to avoid creating the ServiceChannel
        /// with the InstanceContext for the singleton service.
        /// </summary>
        /// <returns>A new channel</returns>
        protected override IBrokerManagementService CreateChannel()
        {
            OperationContext oldContext = OperationContext.Current;
            OperationContext.Current = null;

            try
            {
                return base.CreateChannel();
            }
            finally
            {
                OperationContext.Current = oldContext;
            }
        }
    }
}
