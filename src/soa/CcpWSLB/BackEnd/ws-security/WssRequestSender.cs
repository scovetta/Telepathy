// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents;
    using Microsoft.Telepathy.ServiceBroker.Common;

    /// <summary>
    /// This is an implementation of RequestSender for sending requests
    /// to Java WSS4J service hosts.
    /// </summary>
    internal class WssRequestSender : OnPremiseRequestSender
    {
        public WssRequestSender(EndpointAddress epr, Binding binding, int serviceOperationTimeout, IDispatcher dispatcher,
            int serviceInitializationTimeout, int initEndpointNotFoundWaitPeriod)
            : base(epr, binding, serviceOperationTimeout, dispatcher, serviceInitializationTimeout, initEndpointNotFoundWaitPeriod)
        {
        }

        /// <summary>
        /// Create a new ServiceClient instance
        /// </summary>
        protected override async Task CreateClientAsync()
        {
            await base.CreateClientAsync().ConfigureAwait(false);
            Utility.SetWssClientCertificate(this.Client.ChannelFactory.Credentials);
        }
    }
}
