// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using Microsoft.Telepathy.Session.Interface;

    /// <summary>
    /// Provides an interface for broker launcher client factories who provide
    /// broker launcher client for heartbeat
    /// </summary>
    public interface IBrokerLauncherClientFactoryForHeartbeat
    {
        /// <summary>
        /// Close the broker launcher client for heartbeat
        /// </summary>
        void CloseBrokerLauncherClientForHeartbeat();

        /// <summary>
        /// Gets broker launcher client for heartbeat
        /// </summary>
        /// <returns>returns the broker launcher client</returns>
        IBrokerLauncher GetBrokerLauncherClientForHeartbeat();
    }
}
