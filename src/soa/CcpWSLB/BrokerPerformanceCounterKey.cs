//------------------------------------------------------------------------------
// <copyright file="BrokerPerformanceCounterKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Represents broker's performance counter key
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Common
{
    using System;

    /// <summary>
    /// Represents broker's performance counter key
    /// </summary>
    [Serializable]
    public enum BrokerPerformanceCounterKey
    {
        /// <summary>
        /// Inidcate this should not be included in perf counter
        /// </summary>
        None = 0,

        /// <summary>
        /// Incoming Messages
        /// </summary>
        RequestMessages,

        /// <summary>
        /// Outgoing Messages
        /// </summary>
        ResponseMessages,

        /// <summary>
        /// Total calls
        /// </summary>
        Calculations,

        /// <summary>
        /// Total faulted calls
        /// </summary>
        Faults,

        /// <summary>
        /// Number of all requests in MSMQ
        /// </summary>
        DurableRequestsQueueLength,

        /// <summary>
        /// Number of all responses in MSMQ
        /// </summary>
        DurableResponsesQueueLength,
    }
}
