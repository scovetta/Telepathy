//------------------------------------------------------------------------------
// <copyright file="TraceHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Tracing utilities for soa data components.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System.Diagnostics;

    /// <summary>
    /// Tracing utilities for data components.
    /// </summary>
    internal static class TraceHelper
    {
        /// <summary>
        /// Name of the default event source
        /// </summary>
        private const string DefaultEventSource = "SOA Data";

        /// <summary>
        /// Default trace source
        /// </summary>
        private static TraceSource defaultTraceSourceInternal =  new TraceSource(DefaultEventSource);

        /// <summary>
        /// Trace source
        /// </summary>
        private static TraceSource traceSourceInternal;

        /// <summary>
        /// Get trace source
        /// </summary>
        public static TraceSource TraceSource
        {
            get { return traceSourceInternal ?? defaultTraceSourceInternal; }
            set { traceSourceInternal = value; }
        }
    }
}
