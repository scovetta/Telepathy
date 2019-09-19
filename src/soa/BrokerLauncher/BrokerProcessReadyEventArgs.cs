// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Provide broker process ready event arguments
    /// </summary>
    internal sealed class BrokerProcessReadyEventArgs : EventArgs
    {
        /// <summary>
        /// Stores a value indicating whether the broker process is timed out
        /// </summary>
        private bool timedOut;

        /// <summary>
        /// Initializes a new instance of the BrokerProcessReadyEventArgs class
        /// </summary>
        /// <param name="timedOut">indicating whether the broker process is timed out</param>
        public BrokerProcessReadyEventArgs(bool timedOut)
        {
            this.timedOut = timedOut;
        }

        /// <summary>
        /// Gets a value indicating whether the broker process is timed out
        /// </summary>
        public bool TimedOut
        {
            get { return this.timedOut; }
        }
    }
}
