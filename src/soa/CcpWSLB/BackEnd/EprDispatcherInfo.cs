// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System.ServiceModel;

    using Microsoft.Telepathy.Session.Data;

    /// <summary>
    /// Dispatcher info contains only epr
    /// </summary>
    internal class EprDispatcherInfo : DispatcherInfo
    {
        /// <summary>
        /// Stores the epr
        /// </summary>
        private string epr;

        public EprDispatcherInfo(string epr, int capacity, string unqiueId)
            : base(unqiueId, capacity, null, null, NodeLocation.OnPremise)
        {
            this.epr = epr;

            // TODO: support service host on Azure
        }

        /// <summary>
        /// Gets the enpoint address for a service host
        /// </summary>
        /// <param name="isController">indicating whether controller address is required</param>
        /// <returns>endpoint address</returns>
        protected override EndpointAddress GetEndpointAddress(bool isController)
        {
            if (isController)
            {
                return null;
            }
            else
            {
                return new EndpointAddress(this.epr);
            }
        }
    }
}
