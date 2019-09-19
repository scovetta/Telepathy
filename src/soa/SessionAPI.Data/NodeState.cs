// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Data
{
    public enum NodeState
    {
        /// <summary>
        ///   <para>The node is offline. This enumeration member represents a value of 1.</para>
        /// </summary>
        Offline = 0x1,
        /// <summary>
        ///   <para>The scheduler is in the process of removing jobs from the node. This enumeration member represents a value of 2.</para>
        /// </summary>
        Draining = 0x2,
        /// <summary>
        ///   <para>The node is online and ready to run jobs. This enumeration member represents a value of 4.</para>
        /// </summary>
        Online = 0x4,
        /// <summary>
        ///   <para>All states are set. This enumeration member represents a value of 7.</para>
        /// </summary>
        All = Offline | Draining | Online,
    }
}
