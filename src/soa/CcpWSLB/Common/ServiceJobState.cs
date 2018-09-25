//-----------------------------------------------------------------------
// <copyright file="ServiceJobState.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Service job state</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Service job state
    /// </summary>
    internal enum ServiceJobState
    {
        /// <summary>
        /// Not started
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// Service job is Started
        /// </summary>
        Started = 1,

        /// <summary>
        /// Service job is idle
        /// </summary>
        Idle = 2,

        /// <summary>
        /// Service job is busy
        /// </summary>
        Busy = 3,

        /// <summary>
        /// Service job is finished
        /// </summary>
        Finished = 4,
    }
}
