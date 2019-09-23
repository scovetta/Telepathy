// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.HpcPack
{
    using System;

    /// <summary>
    /// the state definition for connecting to the scheduler.
    /// </summary>
    [Flags]
    internal enum SchedulerConnectState
    {
        /// <summary>
        /// means null.
        /// </summary>
        None = 0,

        /// <summary>
        /// indicating the sessiona launcher is tring to conect to the scheduler.
        /// </summary>
        Connecting = 1,

        /// <summary>
        /// indicating connected top the scheduler store.
        /// </summary>
        ConnectedToSchedulerStore = 2,

        /// <summary>
        /// indicating the job monitor started.
        /// </summary>
        StartedJobMonitor = 4,

        /// <summary>
        /// indicating the broker nodex manager started.
        /// </summary>
        StartedBrokerNodesManager = 8,

        /// <summary>
        /// indicating all the necessary connections to the shceduler are done.
        /// </summary>
        ConnectionComplete = ConnectedToSchedulerStore | StartedBrokerNodesManager | StartedJobMonitor,
    }
}