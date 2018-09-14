using System;
using System.Globalization;
using Microsoft.Hpc.Azure.FileStaging.Events;

namespace Microsoft.Hpc.Azure.FileStaging
{
    class LocalProxyTraceHelper
    {
        private static FileStagingEventsWrapper eventWriter;

        static LocalProxyTraceHelper()
        {
            try
            {
                eventWriter = new FileStagingEventsWrapper();
            }
            catch
            {
            }
        }

        public static void TraceCritical(string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_CriticalTrace(message, string.Empty, string.Empty, string.Empty);
            }
            catch
            {
            }
        }

        public static void TraceCritical(Exception ex, string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_CriticalTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
            }
            catch
            {
            }
        }

        public static void TraceError(string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_ErrorTrace(message, string.Empty, string.Empty, string.Empty);
            }
            catch
            {
            }
        }

        public static void TraceError(Exception ex, string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_ErrorTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
            }
            catch
            {
            }
        }

        public static void TraceWarning(string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_WarningTrace(message, string.Empty, string.Empty, string.Empty);
            }
            catch
            {
            }
        }

        public static void TraceWarning(Exception ex, string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_WarningTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
            }
            catch
            {
            }
        }

        public static void TraceInformation(string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_InformationTrace(message, string.Empty, string.Empty, string.Empty);
            }
            catch
            {
            }
        }

        public static void TraceInformation(Exception ex, string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_InformationTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
            }
            catch
            {
            }
        }

        public static void TraceVerbose(string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_VerboseTrace(message, string.Empty, string.Empty, string.Empty);
            }
            catch
            {
            }
        }

        public static void TraceVerbose(Exception ex, string format, params object[] args)
        {
            try
            {
                string message = string.Format(CultureInfo.CurrentCulture, format, args);
                eventWriter.EventWriteProxy_VerboseTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
            }
            catch
            {
            }
        }
    }
}
