using System;
using System.Globalization;
using Microsoft.Hpc.Azure.FileStaging.Events;

namespace Microsoft.Hpc.Azure.FileStaging
{
    class LocalWorkerTraceHelper
    {
        private static FileStagingEventsWrapper eventWriter;

        static LocalWorkerTraceHelper()
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
                eventWriter.EventWriteWorker_CriticalTrace(message, string.Empty, string.Empty, string.Empty);
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
                eventWriter.EventWriteWorker_CriticalTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
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
                eventWriter.EventWriteWorker_ErrorTrace(message, string.Empty, string.Empty, string.Empty);
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
                eventWriter.EventWriteWorker_ErrorTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
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
                eventWriter.EventWriteWorker_WarningTrace(message, string.Empty, string.Empty, string.Empty);
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
                eventWriter.EventWriteWorker_WarningTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
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
                eventWriter.EventWriteWorker_InformationTrace(message, string.Empty, string.Empty, string.Empty);
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
                eventWriter.EventWriteWorker_InformationTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
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
                eventWriter.EventWriteWorker_VerboseTrace(message, string.Empty, string.Empty, string.Empty);
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
                eventWriter.EventWriteWorker_VerboseTrace(message, ex.GetType().ToString(), ex.Message, ex.StackTrace);
            }
            catch
            {
            }
        }
    }
}
