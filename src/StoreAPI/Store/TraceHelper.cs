using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Store
{
    /// <summary>
    /// Trace helper
    /// </summary>
    public static class TraceHelper
    {
        /// <summary>
        /// Source name for .net tracing
        /// </summary>
        private const string TraceSource = "HpcStoreApi";

        /// <summary>
        /// Instance to TraceSource for writing to .net tracing
        /// </summary>
        private static TraceSource tracing = new TraceSource(TraceSource);

        /// <summary>
        /// writes a message to the tracing facility as an verbose message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void TraceVerbose(string message, params object[] args)
        {
            TraceEvent(TraceEventType.Verbose, message, args);
        }

        /// <summary>
        /// writes a message to the tracing facility as an informational message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void TraceInfo(string message, params object[] args)
        {
            TraceEvent(TraceEventType.Information, message, args);
        }

        /// <summary>
        /// writes a message to the tracing facility as an warning message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void TraceWarning(string message, params object[] args)
        {
            TraceEvent(TraceEventType.Warning, message, args);
        }

        /// <summary>
        /// writes a message to the tracing facility as an error message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void TraceError(string message, params object[] args)
        {
            TraceEvent(TraceEventType.Error, message, args);
        }

        /// <summary>
        /// Sends a general event to tracing.
        /// This is a version without facility information
        /// </summary>
        /// <param name="traceLevel">Trace level</param>
        /// <param name="message">format string</param>
        /// <param name="args">additional data</param>
        private static void TraceEvent(TraceEventType traceLevel, string message, params object[] args)
        {
            // message id is ignored here
            try
            {
                // message id is ignored here
                if (tracing.Switch.ShouldTrace(traceLevel))
                {
                    tracing.TraceEvent(traceLevel, 0, message, args);
                }
            }
            catch (Exception e)
            {
                tracing.TraceEvent(TraceEventType.Error, 0, "TraceSource.TraceEvent Exception: {0}", e);

                // We will not pass on the exception, which might be caused by tracing system. Failure in tracing system should not block the system.
                // If caller has any defect in passing right parameters, we have the log in place telling the problem.
            }
        }
    }
}
