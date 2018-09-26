//------------------------------------------------------------------------------
// <copyright file="Tracing.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Helper class that performs the various tracing to support ETW on vista+
//      text tracing on XP and SOA tracing when in a SOA job.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    using Microsoft.Hpc.Excel.Internal;
    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// References a method used by the TraceEvent class to make a call to a Crimson event writer method
    /// </summary>
    public delegate void EventWriterCallback();

    /// <summary>
    /// These levels are supported in tracing for Services for Excel
    /// </summary>
    public enum XlTraceLevel
    {
        /// <summary>
        /// Trace even if tracing is off
        /// </summary>
        Off = 0,

        /// <summary>
        /// Trace at critical level
        /// </summary>
        Critical = 1,

        /// <summary>
        /// Trace at error level
        /// </summary>
        Error = 2,

        /// <summary>
        /// Trace at warning level
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Trace at informational level
        /// </summary>
        Information = 4,

        /// <summary>
        /// Trace at verbose level
        /// </summary>
        Verbose = 5,
    }

    /// <summary>
    /// Helper class that performs the various tracing logic
    /// </summary>
    public static class Tracing
    {
        /// <summary>
        /// Event log for HPC Services for Excel
        /// </summary>
        private static EventLog eventLog = new EventLog("Windows HPC Services for Excel");

        /// <summary>
        /// Event provider for hpc excel events
        /// </summary>
        private static HPCExcelEventProvider trace;

        /// <summary>
        /// Flag to remember if Crimson events can be used (vista +)
        /// </summary>
        private static bool canUseCrimson = false;

        /// <summary>
        /// Flag to remember if SOA tracing should be enabled (if in a SOA job)
        /// </summary>
        private static bool soaTracingEnabled = false;

#if DEBUG
        /// <summary>
        /// Add a new trace source for debugging purpose
        /// </summary>
        private static readonly TraceSource _traceSource = new TraceSource("HPC SOA Excel");
#endif

        /// <summary>
        /// Initializes static members of the Tracing class. Determines what level
        /// of tracing is supported in the current context.
        /// </summary>
        static Tracing()
        {
            OperatingSystem hostOS = Environment.OSVersion;

            // Crimson can be used on Vista or later. If it can be used, get it ready.
            canUseCrimson = hostOS.Version.Major >= 6;
            if (canUseCrimson)
            {
                trace = new HPCExcelEventProvider();
            }

            // Check the environment to guess if SOA tracing can be used
            try
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CCP_JOBID")))
                {
                    soaTracingEnabled = true;
                }
            }
            catch (Exception)
            {
                soaTracingEnabled = false;
            }
        }

        /// <summary>
        /// The assemblies in the Microsoft.Hpc.Excel namespace supported by this class
        /// </summary>
        public enum ComponentId
        {
            /// <summary>
            /// Microsoft.Hpc.Excel.XllConnector component identifier
            /// </summary>
            XllConnector,

            /// <summary>
            /// Microsoft.Hpc.Excel.XllContainer component identifier
            /// </summary>
            XllContainer,

            /// <summary>
            /// Microsoft.Hpc.Excel.ExcelDriver component identifier
            /// </summary>
            ExcelDriver,

            /// <summary>
            /// Microsoft.Hpc.Excel.ExcelClient component identifier
            /// </summary>
            ExcelClient,

            /// <summary>
            /// Microsoft.Hpc.Excel.ExcelService component identifier
            /// </summary>
            ExcelService,
        }

        /// <summary>
        /// Gets a handle to the event provider helper
        /// </summary>
        public static HPCExcelEventProvider EventProvider
        {
            get { return trace; }
        }

        /// <summary>
        /// Writes a Critical level event to the debug channel
        /// </summary>
        /// <param name="componentName">Calling component ID</param>
        /// <param name="message">message to write to event</param>
        /// <param name="parameters">any parameters to place into the message</param>
        public static void WriteDebugTextCritical(ComponentId componentName, string message, params object[] parameters)
        {
            if (CanUseCrimson())
            {
                string traceString = CombineTraceString(componentName, message, parameters);

                // Trace using the event ID and channel appropriate for the calling assembly
                switch (componentName)
                {
                    case ComponentId.XllConnector:
                        trace.LogXllConnector_WriteDebugTextCritical(traceString);
                        break;

                    case ComponentId.XllContainer:
                        trace.LogXllContainer_WriteDebugTextCritical(traceString);
                        break;

                    case ComponentId.ExcelDriver:
                        trace.LogExcelDriver_WriteDebugTextCritical(traceString);
                        break;

                    case ComponentId.ExcelClient:
                        trace.LogExcelClient_WriteDebugTextCritical(traceString);
                        break;

                    case ComponentId.ExcelService:
                        trace.LogExcelService_WriteDebugTextCritical(traceString);
                        break;
                }
            }

            // Also use SOA tracing if SOA_TRACING is defined
            SoaTrace(XlTraceLevel.Critical, componentName, message, parameters);
        }

        /// <summary>
        /// Writes an Error level event to the debug channel
        /// </summary>
        /// <param name="componentName">Calling component ID</param>
        /// <param name="message">message to write to event</param>
        /// <param name="parameters">any parameters to place into the message</param>
        public static void WriteDebugTextError(ComponentId componentName, string message, params object[] parameters)
        {
            if (CanUseCrimson())
            {
                string traceString = CombineTraceString(componentName, message, parameters);
                // Trace using the event ID and channel appropriate for the calling assembly
                switch (componentName)
                {
                    case ComponentId.XllConnector:
                        trace.LogXllConnector_WriteDebugTextError(traceString);
                        break;

                    case ComponentId.XllContainer:
                        trace.LogXllContainer_WriteDebugTextError(traceString);
                        break;

                    case ComponentId.ExcelDriver:
                        trace.LogExcelDriver_WriteDebugTextError(traceString);
                        break;

                    case ComponentId.ExcelClient:
                        trace.LogExcelClient_WriteDebugTextError(traceString);
                        break;

                    case ComponentId.ExcelService:
                        trace.LogExcelService_WriteDebugTextError(traceString);
                        break;
                }
            }

            // Also use SOA tracing if SOA_TRACING is defined
            SoaTrace(XlTraceLevel.Error, componentName, message, parameters);
        }

        /// <summary>
        /// Writes a Warning level event to the debug channel
        /// </summary>
        /// <param name="componentName">Calling component ID</param>
        /// <param name="message">message to write to event</param>
        /// <param name="parameters">any parameters to place into the message</param>
        public static void WriteDebugTextWarning(ComponentId componentName, string message, params object[] parameters)
        {
            if (CanUseCrimson())
            {
                string traceString = CombineTraceString(componentName, message, parameters);

                // Trace using the event ID and channel appropriate for the calling assembly
                switch (componentName)
                {
                    case ComponentId.XllConnector:
                        trace.LogXllConnector_WriteDebugTextWarning(traceString);
                        break;

                    case ComponentId.XllContainer:
                        trace.LogXllContainer_WriteDebugTextWarning(traceString);
                        break;

                    case ComponentId.ExcelDriver:
                        trace.LogExcelDriver_WriteDebugTextWarning(traceString);
                        break;

                    case ComponentId.ExcelClient:
                        trace.LogExcelClient_WriteDebugTextWarning(traceString);
                        break;

                    case ComponentId.ExcelService:
                        trace.LogExcelService_WriteDebugTextWarning(traceString);
                        break;
                }
            }

            // Also use SOA tracing if SOA_TRACING is defined
            SoaTrace(XlTraceLevel.Warning, componentName, message, parameters);
        }

        /// <summary>
        /// Writes a Information level event to the debug channel
        /// </summary>
        /// <param name="componentName">Calling component ID</param>
        /// <param name="message">message to write to event</param>
        /// <param name="parameters">any parameters to place into the message</param>
        public static void WriteDebugTextInfo(ComponentId componentName, string message, params object[] parameters)
        {
            if (CanUseCrimson())
            {
                string traceString = CombineTraceString(componentName, message, parameters);

                // Trace using the event ID and channel appropriate for the calling assembly
                switch (componentName)
                {
                    case ComponentId.XllConnector:
                        trace.LogXllConnector_WriteDebugTextInfo(traceString);
                        break;

                    case ComponentId.XllContainer:
                        trace.LogXllContainer_WriteDebugTextInfo(traceString);
                        break;

                    case ComponentId.ExcelDriver:
                        trace.LogExcelDriver_WriteDebugTextInfo(traceString);
                        break;

                    case ComponentId.ExcelClient:
                        trace.LogExcelClient_WriteDebugTextInfo(traceString);
                        break;

                    case ComponentId.ExcelService:
                        trace.LogExcelService_WriteDebugTextInfo(traceString);
                        break;
                }
            }

            // Also use SOA tracing if SOA_TRACING is defined
            SoaTrace(XlTraceLevel.Information, componentName, message, parameters);
        }

        /// <summary>
        /// Writes a verbose level event to the debug channel
        /// </summary>
        /// <param name="componentName">Calling component ID</param>
        /// <param name="message">message to write to event</param>
        /// <param name="parameters">any parameters to place into the message</param>
        public static void WriteDebugTextVerbose(ComponentId componentName, string message, params object[] parameters)
        {
            if (CanUseCrimson())
            {
                string traceString = CombineTraceString(componentName, message, parameters);

                // Trace using the event ID and channel appropriate for the calling assembly
                switch (componentName)
                {
                    case ComponentId.XllConnector:
                        trace.LogXllConnector_WriteDebugTextVerbose(traceString);
                        break;

                    case ComponentId.XllContainer:
                        trace.LogXllContainer_WriteDebugTextVerbose(traceString);
                        break;

                    case ComponentId.ExcelDriver:
                        trace.LogExcelDriver_WriteDebugTextVerbose(traceString);
                        break;

                    case ComponentId.ExcelClient:
                        trace.LogExcelClient_WriteDebugTextVerbose(traceString);
                        break;

                    case ComponentId.ExcelService:
                        trace.LogExcelService_WriteDebugTextVerbose(traceString);
                        break;
                }
            }

            // Also use SOA tracing if SOA_TRACING is defined
            SoaTrace(XlTraceLevel.Verbose, componentName, message, parameters);
        }

        /// <summary>
        /// Traces using Crimson and an event from the event manifest, if possible, and traces using the event log if not possible.
        /// </summary>
        /// <param name="eventLevel">The level of the event (used for event log tracing only)</param>
        /// <param name="componentName">The name of the calling component </param>
        /// <param name="message">The text of the exception or error that is being logged (used for event log tracing only)</param>
        /// <param name="eventWriter">A method that will trace the event with Crimson if it can be used</param>
        public static void TraceEvent(XlTraceLevel eventLevel, ComponentId componentName, string message, EventWriterCallback eventWriter)
        {
            if (CanUseCrimson())
            {
                // This is running Vista or later. Use Crimson.
                eventWriter();
            }
            else
            {
                // Running XP. Use the event log.
                TraceEventLogEntry(componentName, eventLevel, message);
            }

            // Also use SOA tracing if SOA_TRACING is defined
            SoaTrace(eventLevel, componentName, message);
        }

        /// <summary>
        /// Determines whether or not Crimson can be used, based on the operating system version
        /// </summary>
        /// <returns>TRUE if Crimson can be used</returns>
        public static bool CanUseCrimson()
        {
            return canUseCrimson;
        }

        /// <summary>
        /// Traces a message to the event log with the specified level
        /// </summary>
        /// <param name="componentName">Calling component ID</param>
        /// <param name="eventLevel">severity of event</param>
        /// <param name="message">exception text</param>
        public static void TraceEventLogEntry(ComponentId componentName, XlTraceLevel eventLevel, string message)
        {
            // Update the event log source
            if (eventLog.Source.Length == 0)
            {
                switch (componentName)
                {
                    case ComponentId.XllConnector:
                        eventLog.Source = "XllConnector";
                        break;

                    case ComponentId.XllContainer:
                        eventLog.Source = "XllContainer";
                        break;

                    case ComponentId.ExcelDriver:
                        eventLog.Source = "ExcelDriver";
                        break;

                    case ComponentId.ExcelClient:
                        eventLog.Source = "ExcelClient";
                        break;

                    case ComponentId.ExcelService:
                        eventLog.Source = "ExcelService";
                        break;
                }
            }

            // Convert the XlTraceLevel to the event log level
            EventLogEntryType type = EventLogEntryType.Information;
            switch (eventLevel)
            {
                case XlTraceLevel.Critical:
                case XlTraceLevel.Error:
                    type = EventLogEntryType.Error;
                    break;

                case XlTraceLevel.Warning:
                    type = EventLogEntryType.Warning;
                    break;

                case XlTraceLevel.Information:
                case XlTraceLevel.Verbose:
                    type = EventLogEntryType.Information;
                    break;
            }

            // Write the event log entry
            string traceString = CombineTraceString(componentName, message);
            eventLog.WriteEntry(traceString, type);
        }

        /// <summary>
        /// Traces a message with SOA
        /// </summary>
        /// <param name="eventLevel">Severity of the event</param>
        /// <param name="message">error text to write to event</param>
        /// <param name="parameters">parameters for message string</param>
        public static void SoaTrace(XlTraceLevel eventLevel, string message, params object[] parameters)
        {
#if DEBUG
            _traceSource.TraceInformation(message, parameters);
#endif
            if (!soaTracingEnabled)
            {
                return;
            }

            string traceString = string.Format(message, parameters);
            try
            {
                switch (eventLevel)
                {
                    case XlTraceLevel.Critical:
                        if (ServiceContext.Logger.Switch.ShouldTrace(TraceEventType.Critical))
                        {
                            ServiceContext.Logger.TraceEvent(TraceEventType.Critical, 0, traceString);
                        }

                        break;

                    case XlTraceLevel.Error:
                        if (ServiceContext.Logger.Switch.ShouldTrace(TraceEventType.Error))
                        {
                            ServiceContext.Logger.TraceEvent(TraceEventType.Error, 0, traceString);
                        }

                        break;

                    case XlTraceLevel.Warning:
                        if (ServiceContext.Logger.Switch.ShouldTrace(TraceEventType.Warning))
                        {
                            ServiceContext.Logger.TraceEvent(TraceEventType.Warning, 0, traceString);
                        }

                        break;

                    case XlTraceLevel.Information:
                        if (ServiceContext.Logger.Switch.ShouldTrace(TraceEventType.Information))
                        {
                            ServiceContext.Logger.TraceEvent(TraceEventType.Information, 0, traceString);
                        }

                        break;
                    case XlTraceLevel.Verbose:
                        if (ServiceContext.Logger.Switch.ShouldTrace(TraceEventType.Verbose))
                        {
                            ServiceContext.Logger.TraceEvent(TraceEventType.Verbose, 0, traceString);
                        }

                        break;
                }
            }
            catch (Exception)
            {
                // Trace the exception?
            }
        }

        /// <summary>
        /// Traces a message with SOA
        /// </summary>
        /// <param name="eventLevel">Severity of the event</param>
        /// <param name="message">error text to write to event</param>
        /// <param name="parameters">parameters for message string</param>
        public static void SoaTrace(XlTraceLevel eventLevel, ComponentId componentName, string message, params object[] parameters)
        {
            string traceString = CombineTraceString(componentName, message, parameters);
            SoaTrace(eventLevel, traceString);
        }

        /// <summary>
        /// the helper function to combine the trace string.
        /// </summary>
        /// <param name="componentName">Calling component ID</param>
        /// <param name="message">message to write to event</param>
        /// <param name="parameters">any parameters to place into the message</param>
        /// <returns>the combined trace string.</returns>
        private static string CombineTraceString(ComponentId componentName, string message, params object[] parameters)
        {
            string traceString = String.Format(CultureInfo.CurrentCulture, message, parameters);

            switch (componentName)
            {
                case ComponentId.XllConnector:
                    return "[HpcXllConnector]: " + traceString;

                case ComponentId.XllContainer:
                    return "[HpcXllContainer]: " + traceString;

                case ComponentId.ExcelDriver:
                    return "[HpcExcelDriver]: " + traceString;

                case ComponentId.ExcelService:
                    return "[HpcExcelService]: " + traceString;

                case ComponentId.ExcelClient:
                    return "[HpcExcelClient]: " + traceString;

                default:
                    return traceString;
            }
        }
    }
}