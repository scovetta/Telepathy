// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Test.E2E.Bvt.Helper.Trace
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Provider util methods to write trace for SOA diag system in UI
    /// </summary>
    /// <remarks>
    /// This util has the same interface with the DiagTraceHelper in broker
    /// launcher and SoaDiagMon service but write the traces into .Net trace
    /// source.
    /// </remarks>
    internal static class DiagTraceHelper
    {
        /// <summary>
        /// trace event
        /// </summary>
        /// <param name="traceLevel">the trace level.</param>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        [Conditional("TRACE")]
        public static void TraceEvent(TraceEventType traceLevel, string format, params object[] objparams)
        {
        }

        /// <summary>
        /// trace error
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [Conditional("TRACE")]
        public static void TraceError(string format, params object[] objparams)
        {
        }

        /// <summary>
        /// trace warning
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [Conditional("TRACE")]
        public static void TraceWarning(string format, params object[] objparams)
        {
        }

        /// <summary>
        /// trace information
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [Conditional("TRACE")]
        public static void TraceInfo(string format, params object[] objparams)
        {
        }

        /// <summary>
        /// trace verbose.
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [Conditional("TRACE")]
        public static void TraceVerbose(string format, params object[] objparams)
        {
        }

        /// <summary>
        /// Converts the given file time into a string representing
        /// the file time
        /// </summary>
        /// <param name="fileTime">indicating the file time</param>
        /// <returns>returns the formatted string</returns>
        public static string FormatFileTime(long fileTime)
        {
            // Option "o" will formate the datetime instance like this:
            // 2008-06-15T21:15:07.0000000
            // http://msdn.microsoft.com/en-us/library/zdtaw1bw.aspx
            return DateTime.FromFileTimeUtc(fileTime).ToString("o", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// the helper function to combine the trace string.
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        /// <returns>the combined trace string.</returns>
        private static string CombineTraceString(string format, params object[] objparams)
        {
            string traceString = null;
            try
            {
                traceString = string.Format(CultureInfo.InvariantCulture, format, objparams);
            }
            catch (Exception e)
            {
                traceString = "Fail to combine the trace string, format:" + format;
                if (objparams == null || objparams.Length <= 0)
                {
                    traceString += ", objparams is null";
                }
                else
                {
                    traceString += ", objparams.Length = " + objparams.Length.ToString(CultureInfo.InvariantCulture);
                }

                traceString += ", Exception:" + e.ToString();
            }

            return traceString;
        }
    }
}
