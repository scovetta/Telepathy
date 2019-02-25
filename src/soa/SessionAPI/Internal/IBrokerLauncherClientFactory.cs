//------------------------------------------------------------------------------
// <copyright file="IBrokerLauncherClientFactoryForHeartbeat.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Provides an interface for broker launcher client factories who provide
//      broker launcher client for heartbeat
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
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
