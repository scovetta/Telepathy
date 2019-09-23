// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// Client proxy to BrokerManagement service
    /// </summary>
    internal class BrokerManagementClient : ClientBase<IBrokerManagementAsync>, IBrokerManagement
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BrokerManagementClient()
            : base(BrokerManagement.Binding, new EndpointAddress(BrokerManagement.Address))
        {
        }

        /// <summary>
        /// Brings HpcBroker online
        /// </summary>
        public void Online()
        {
            this.InnerChannel.OperationTimeout = TimeSpan.MaxValue;

            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
            IAsyncResult result = this.Channel.BeginOnline(null, null);
            this.Channel.EndOnline(result);
        }

        /// <summary>
        /// Brings HpcBroker offline
        /// </summary>
        /// <param name="force"></param>
        public void StartOffline(bool force)
        {
            this.InnerChannel.OperationTimeout = TimeSpan.MaxValue;

            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
            IAsyncResult result = this.Channel.BeginStartOffline(force, null, null);
            this.Channel.EndStartOffline(result);
        }

        /// <summary>
        /// Tells whether HpcBroker is offline
        /// </summary>
        /// <returns>True if it is offline; Otherwise false</returns>
        public bool IsOffline()
        {
            this.InnerChannel.OperationTimeout = TimeSpan.MaxValue;

            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
            IAsyncResult result = this.Channel.BeginIsOffline(null, null);
            return this.Channel.EndIsOffline(result);
        }

        /// <summary>
        /// Work around for WCF ServiceChannel leak bug
        /// Creates the channel with OperationContext=null to avoid creating the ServiceChannel
        /// with the InstanceContext for the singleton service.
        /// </summary>
        /// <returns>A new channel</returns>
        protected override IBrokerManagementAsync CreateChannel()
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
