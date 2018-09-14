//------------------------------------------------------------------------------
// <copyright file="DataServiceTraceHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data service trace helper
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Hpc.RuntimeTrace;

    /// <summary>
    /// Data service trace helper
    /// </summary>
    internal static class DataServiceTraceHelper
    {
        /// <summary>
        /// Stores the data service trace instance
        /// </summary>
        private static DataSvcTraceWrapper diagTrace = new DataSvcTraceWrapper();

        /// <summary>
        /// trace event
        /// </summary>
        /// <param name="traceLevel">the trace level.</param>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        [Conditional("TRACE")]
        public static void TraceEvent(TraceEventType traceLevel, string format, params object[] objparams)
        {
            string traceString = string.Format(CultureInfo.InvariantCulture, format, objparams);
            try
            {
                switch (traceLevel)
                {
                    case TraceEventType.Critical:
                        diagTrace.LogTextCritial(traceString);
                        break;

                    case TraceEventType.Error:
                        diagTrace.LogTextError(traceString);
                        break;

                    case TraceEventType.Information:
                        diagTrace.LogTextInfo(traceString);
                        break;

                    case TraceEventType.Verbose:
                        diagTrace.LogTextVerbose(traceString);
                        break;

                    case TraceEventType.Warning:
                        diagTrace.LogTextWarning(traceString);
                        break;

                    case TraceEventType.Resume:
                    case TraceEventType.Start:
                    case TraceEventType.Stop:
                    case TraceEventType.Suspend:
                        diagTrace.LogTextInfo(traceString);
                        break;
                }
            }
            catch (Exception)
            {
                // failed to write the trace
            }
        }
    }
}
