// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.RuntimeTrace
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Provider util methods to write trace for SOA diag system
    /// </summary>
    public static class DiagTraceHelper
    {
        /// <summary>
        /// Stores the diag trace instance
        /// </summary>
        private static SOADiagTraceWrapper diagTrace = new SOADiagTraceWrapper();

        /// <summary>
        /// Gets the diag trace instance
        /// </summary>
        public static SOADiagTraceWrapper SOADiagTrace
        {
            get { return diagTrace; }
        }

        /// <summary>
        /// trace event
        /// </summary>
        /// <param name="traceLevel">the trace level.</param>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        [Conditional("TRACE")]
        public static void TraceEvent(TraceEventType traceLevel, string format, params object[] objparams)
        {
            TraceString(traceLevel, CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace error
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [Conditional("TRACE")]
        public static void TraceError(string format, params object[] objparams)
        {
            TraceString(TraceEventType.Error, CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace warning
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [Conditional("TRACE")]
        public static void TraceWarning(string format, params object[] objparams)
        {
            TraceString(TraceEventType.Warning, CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace information
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [Conditional("TRACE")]
        public static void TraceInfo(string format, params object[] objparams)
        {
            TraceString(TraceEventType.Information, CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace verbose.
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [Conditional("TRACE")]
        public static void TraceVerbose(string format, params object[] objparams)
        {
            TraceString(TraceEventType.Verbose, CombineTraceString(format, objparams));
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
        /// the helper function to write the trace.
        /// </summary>
        /// <param name="traceLevel">the trace level.</param>
        /// <param name="traceText">the string content.</param>
        [Conditional("TRACE")]
        private static void TraceString(TraceEventType traceLevel, string traceText)
        {
            try
            {
#if UNITTEST
                // If the code is running in unit test, also write the trace
                // into Console so that it would be visible in test result
                Console.Out.WriteLine(traceText);
#endif

                switch (traceLevel)
                {
                    case TraceEventType.Critical:
                        diagTrace.LogTextCritial(traceText);
                        break;
                    case TraceEventType.Error:
                        diagTrace.LogTextError(traceText);
                        break;
                    case TraceEventType.Information:
                        diagTrace.LogTextInfo(traceText);
                        break;
                    case TraceEventType.Warning:
                        diagTrace.LogTextWarning(traceText);
                        break;
                    case TraceEventType.Verbose:
                        diagTrace.LogTextVerbose(traceText);
                        break;
                    case TraceEventType.Resume:
                    case TraceEventType.Start:
                    case TraceEventType.Stop:
                    case TraceEventType.Suspend:
                        diagTrace.LogTextInfo(traceText);
                        break;
                }

#if !BrokerLauncher && PAAS
                if (SoaHelper.IsOnAzure())
                {
                    switch (traceLevel)
                    {
                        case TraceEventType.Critical:
                        case TraceEventType.Error:
                            DiagAzureTraceHelper.TraceError(traceText);
                            break;

                        case TraceEventType.Warning:
                            DiagAzureTraceHelper.TraceWarning(traceText);
                            break;

                        case TraceEventType.Information:
                            DiagAzureTraceHelper.TraceInformation(traceText);
                            break;

                        case TraceEventType.Verbose:
                            DiagAzureTraceHelper.WriteLine(traceText);
                            break;

                        default:
                            DiagAzureTraceHelper.WriteLine(traceText);
                            break;
                    }
                }
#endif

            }
            catch (Exception)
            {
                // failed to write the trace.
            }
        }

        /// <summary>
        /// the helper function to combine the trace string.
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        /// <returns>the combined trace string.</returns>
        private static string CombineTraceString(string format, params object[] objparams)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, format, objparams);
            }
            catch (Exception e)
            {
                sb.Clear();
                sb.Append("Fail to combine the trace string, format:");
                sb.Append(format);
                if (objparams == null || objparams.Length <= 0)
                {
                    sb.Append(", objparams is null");
                }
                else
                {
                    sb.Append(", objparams.Length = ");
                    sb.Append(objparams.Length.ToString(CultureInfo.InvariantCulture));
                }

                sb.Append(", Exception:");
                sb.Append(e.ToString());
            }

            return sb.ToString();
        }
    }
}
