// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.RuntimeTrace
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    /// <summary>
    /// the delegate for the IsDiagTraceEnabled method
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public delegate bool IsDiagTraceEnabledDelegate(string id);

    /// <summary>
    /// the helper functions for the trace.
    /// </summary>
    public static class TraceHelper
    {
        /// <summary>
        /// It is listener name for soa cosmos log. It is defined in the config file.
        /// </summary>
        private const string SoaListenerName = "SoaListener";

        static TraceHelper() { }

        /// <summary>
        /// Etw trace source
        /// </summary>
        private static RuntimeTraceWrapper trace = new RuntimeTraceWrapper();

        /// <summary>
        /// Sets the IsDiagTraceEnabled delegate.
        /// </summary>
        public static IsDiagTraceEnabledDelegate IsDiagTraceEnabled
        {
            set
            {
                trace.IsDiagTraceEnabled = value;
            }
        }

        /// <summary>
        /// Gets Etw trace source
        /// </summary>
        public static RuntimeTraceWrapper RuntimeTrace
        {
            get { return trace; }
        }

        /// <summary>
        /// trace event
        /// </summary>
        /// <param name="sessionId">the session id.</param>
        /// <param name="traceLevel">the trace level.</param>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        public static void TraceEvent(string sessionId, TraceEventType traceLevel, string format, params object[] objparams)
        {
            TraceHelper.TraceString(sessionId, traceLevel, TraceHelper.CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace event
        /// </summary>
        /// <param name="traceLevel">the trace level.</param>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        public static void TraceEvent(TraceEventType traceLevel, string format, params object[] objparams)
        {
            TraceHelper.TraceString("0", traceLevel, TraceHelper.CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace error
        /// </summary>
        /// <param name="traceSource">the trace source.</param>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void TraceError(string sessionId, string format, params object[] objparams)
        {
            TraceHelper.TraceString(sessionId, TraceEventType.Error, TraceHelper.CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace warning
        /// </summary>
        /// <param name="traceSource">the trace source.</param>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void TraceWarning(string sessionId, string format, params object[] objparams)
        {
            TraceHelper.TraceString(sessionId, TraceEventType.Warning, TraceHelper.CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace information
        /// </summary>
        /// <param name="traceSource">the trace source.</param>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void TraceInfo(string sessionId, string format, params object[] objparams)
        {
            TraceHelper.TraceString(sessionId, TraceEventType.Information, TraceHelper.CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace verbose.
        /// </summary>
        /// <param name="traceSource">the trace source.</param>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameteres.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void TraceVerbose(string sessionId, string format, params object[] objparams)
        {
            TraceHelper.TraceString(sessionId, TraceEventType.Verbose, TraceHelper.CombineTraceString(format, objparams));
        }

        /// <summary>
        /// trace the exception
        /// </summary>
        /// <param name="traceSource">the trace source.</param>
        /// <param name="exception">the exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void TraceException(string sessionId, Exception exception)
        {
            TraceHelper.TraceString(sessionId, TraceEventType.Error, TraceHelper.Exception2String(null, exception));
        }

        /// <summary>
        /// trace the exception
        /// </summary>
        /// <param name="traceSource">the trace source.</param>
        /// <param name="description">the description for the exception.</param>
        /// <param name="exception">the exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void TraceException(string sessionId, string description, Exception exception)
        {
            TraceHelper.TraceString(sessionId, TraceEventType.Error, TraceHelper.Exception2String(description, exception));
        }

        /// <summary>
        /// trace the exception
        /// </summary>
        /// <param name="traceSource">the trace source.</param>
        /// <param name="traceLevel">the trace level for the exception.</param>
        /// <param name="exception">the exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void TraceException(string sessionId, TraceEventType traceLevel, Exception exception)
        {
            TraceHelper.TraceString(sessionId, traceLevel, TraceHelper.Exception2String(null, exception));
        }

        /// <summary>
        /// trace the exception
        /// </summary>
        /// <param name="traceSource">the trace source.</param>
        /// <param name="traceLevel">the trace level for the exception.</param>
        /// <param name="description">the description for the exception.</param>
        /// <param name="exception">the exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static void TraceException(string sessionId, TraceEventType traceLevel, string description, Exception exception)
        {
            TraceHelper.TraceString(sessionId, traceLevel, TraceHelper.Exception2String(description, exception));
        }

        /// <summary>
        /// the helper function to write the trace.
        /// </summary>
        /// <param name="traceSource">the trace context.</param>
        /// <param name="traceLevel">the trace level.</param>
        /// <param name="content">the string content.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static void TraceString(string sessionId, TraceEventType traceLevel, string traceText)
        {
            try
            {
                switch (traceLevel)
                {
                    case TraceEventType.Critical:
                        if (sessionId.Equals("0"))
                        {
                            trace.LogEventTextCritial(traceText);
                        }
                        else
                        {
                            trace.LogTextCritial(sessionId, traceText);
                        }

                        break;

                    case TraceEventType.Error:
                        if (sessionId.Equals("0"))
                        {
                            trace.LogEventTextError(traceText);
                        }
                        else
                        {
                            trace.LogTextError(sessionId, traceText);
                        }

                        break;

                    case TraceEventType.Information:
                        if (sessionId.Equals("0"))
                        {
                            trace.LogEventTextInfo(traceText);
                        }
                        else
                        {
                            trace.LogTextInfo(sessionId, traceText);
                        }

                        break;

                    case TraceEventType.Warning:
                        if (sessionId.Equals("0"))
                        {
                            trace.LogEventTextWarning(traceText);
                        }
                        else
                        {
                            trace.LogTextWarning(sessionId, traceText);
                        }

                        break;

                    case TraceEventType.Verbose:
                        if (sessionId.Equals("0"))
                        {
                            trace.LogEventTextVerbose(traceText);
                        }
                        else
                        {
                            trace.LogTextVerbose(sessionId, traceText);
                        }

                        break;

                    case TraceEventType.Resume:
                    case TraceEventType.Start:
                    case TraceEventType.Stop:
                    case TraceEventType.Suspend:
                        if (sessionId.Equals("0"))
                        {
                            trace.LogEventTextInfo(traceText);
                        }
                        else
                        {
                            trace.LogTextInfo(sessionId, traceText);
                        }

                        break;
                }
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
            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, objparams);
            }
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder();
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

                return sb.ToString();
            }
        }

        /// <summary>
        /// the helper function to transfer the exception to string.
        /// </summary>
        /// <param name="description">the description for the trace.</param>
        /// <param name="e">the exception</param>
        /// <returns>the string that contain the session id, description and the excepton information.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static string Exception2String(string description, Exception e)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                if (!string.IsNullOrEmpty(description))
                {
                    sb.Append(", Description:");
                    sb.Append(description);
                }

                if (e != null)
                {
                    sb.Append(", Exception:");
                    sb.Append(e.ToString());
                }
            }
            catch (Exception)
            {
                // failed to transfer the exceptioin to string.
            }

            return sb.ToString();
        }
    }
}
