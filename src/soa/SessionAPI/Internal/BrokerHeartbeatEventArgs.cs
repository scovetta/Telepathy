//------------------------------------------------------------------------------
// <copyright file="BrokerHeartbeatEventArgs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Provides an event argument for broker heartbeat event
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;

    /// <summary>
    /// Provides an event argument for broker heartbeat event
    /// </summary>
    public class BrokerHeartbeatEventArgs : EventArgs
    {
        /// <summary>
        /// Stores a value indicating whether it is broker node down
        /// </summary>
        private bool isBrokerNodeDown;

        /// <summary>
        /// Initializes a new instance of the BrokerHeartbeatEventArgs class
        /// </summary>
        /// <param name="isBrokerNodeDown">indicating whether it is broker node down</param>
        public BrokerHeartbeatEventArgs(bool isBrokerNodeDown)
        {
            this.isBrokerNodeDown = isBrokerNodeDown;
        }

        /// <summary>
        /// Gets a value indicating whether it is broker node down
        /// </summary>
        public bool IsBrokerNodeDown
        {
            get { return this.isBrokerNodeDown; }
        }
    }
}
