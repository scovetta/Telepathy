// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.ServiceModel;

    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Telepathy.Session.Internal;

    using RuntimeTraceHelper = Microsoft.Telepathy.RuntimeTrace.TraceHelper;

    /// <summary>
    /// Provide a trace listener for the soa diag trace.
    /// </summary>
    public class SoaDiagTraceListener : TraceListener
    {
        /// <summary>
        /// Stores the separator for the trace message.
        /// </summary>
        private const string TraceSeparator = ",";

        /// <summary>
        /// Stores the session Id.
        /// </summary>
        private string sessionId;

        /// <summary>
        /// Initializes a new instance of the SoaDiagTraceListener class.
        /// </summary>
        /// <param name="sessionId">soa session id</param>
        public SoaDiagTraceListener(string sessionId)
        {
            this.sessionId = sessionId;
        }

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">
        /// A message to write.
        /// </param>
        public override void Write(string message)
        {
            this.WriteLine(message);
        }

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">
        /// A message to write.
        /// </param>
        public override void WriteLine(string message)
        {
            this.InternalTraceMessage(TraceEventType.Information, message);
        }

        /// <summary>
        /// Writes a data object to the listener.
        /// </summary>
        /// <param name="eventCache">
        /// A System.Diagnostics.TraceEventCache object that contains the
        /// current process ID, thread ID, and stack trace information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the
        /// application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the System.Diagnostics.TraceEventType values specifying
        /// the type of event that has caused the trace.
        /// </param>
        /// <param name="id">
        /// A numeric identifier for the event.
        /// </param>
        /// <param name="data">
        /// The trace data to emit.
        /// </param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            this.TraceData(eventCache, source, eventType, id, new object[] { data });
        }

        /// <summary>
        /// Writes a data object to the listener.
        /// </summary>
        /// <param name="eventCache">
        /// A System.Diagnostics.TraceEventCache object that contains the
        /// current process ID, thread ID, and stack trace information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the
        /// application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the System.Diagnostics.TraceEventType values specifying
        /// the type of event that has caused the trace.
        /// </param>
        /// <param name="id">
        /// A numeric identifier for the event.
        /// </param>
        /// <param name="data">
        /// An array of objects to emit as data.
        /// </param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            string message = string.Empty;

            if (data != null)
            {
                // string.Empty is used as a place holder in the joined string
                // for null element in data array. it is exactly what we need.
                message = string.Join<object>(TraceSeparator, data);
            }

            this.InternalTraceMessage(eventType, message);
        }

        /// <summary>
        /// Writes trace and event information to the listener.
        /// </summary>
        /// <param name="eventCache">
        /// A System.Diagnostics.TraceEventCache object that contains the
        /// current process ID, thread ID, and stack trace information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the
        /// application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the System.Diagnostics.TraceEventType values specifying
        /// the type of event that has caused the trace.
        /// </param>
        /// <param name="id">
        /// A numeric identifier for the event.
        /// </param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            this.TraceEvent(eventCache, source, eventType, id, string.Empty);
        }

        /// <summary>
        /// Writes trace and event information to the listener.
        /// </summary>
        /// <param name="eventCache">
        /// A System.Diagnostics.TraceEventCache object that contains the
        /// current process ID, thread ID, and stack trace information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the
        /// application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the System.Diagnostics.TraceEventType values specifying
        /// the type of event that has caused the trace.
        /// </param>
        /// <param name="id">
        /// A numeric identifier for the event.
        /// </param>
        /// <param name="format">
        /// A format string that contains zero or more format items, which
        /// correspond to objects in the args array.
        /// </param>
        /// <param name="args">
        /// An object array containing zero or more objects to format.
        /// </param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (format == null)
            {
                throw new ArgumentNullException(StringTable.FormatCannotBeNull);
            }

            string message;

            if (args != null)
            {
                message = string.Format(CultureInfo.InvariantCulture, format, args);
            }
            else
            {
                message = format;
            }

            this.TraceEvent(eventCache, source, eventType, id, message);
        }

        /// <summary>
        /// Writes trace and event information to the listener.
        /// </summary>
        /// <param name="eventCache">
        /// A System.Diagnostics.TraceEventCache object that contains the
        /// current process ID, thread ID, and stack trace information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the
        /// application that generated the trace event.
        /// </param>
        /// <param name="eventType">
        /// One of the System.Diagnostics.TraceEventType values specifying
        /// the type of event that has caused the trace.
        /// </param>
        /// <param name="id">
        /// A numeric identifier for the event.
        /// </param>
        /// <param name="message">
        /// A message to write.
        /// </param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            this.InternalTraceMessage(eventType, message);
        }

        /// <summary>
        /// Writes trace and event information to the listener.
        /// </summary>
        /// <param name="eventCache">
        /// A System.Diagnostics.TraceEventCache object that contains the
        /// current process ID, thread ID, and stack trace information.
        /// </param>
        /// <param name="source">
        /// A name used to identify the output, typically the name of the
        /// application that generated the trace event.
        /// </param>
        /// <param name="id">
        /// A numeric identifier for the event.
        /// </param>
        /// <param name="message">
        /// A message to write.
        /// </param>
        /// <param name="relatedActivityId">
        /// A System.Guid object identifying a related activity.
        /// </param>
        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            this.InternalTraceMessage(TraceEventType.Transfer, message);
        }

        /// <summary>
        /// Emits an error message to the listener.
        /// </summary>
        /// <param name="message">
        /// A message to emit.
        /// </param>
        public override void Fail(string message)
        {
            this.Fail(message, null);
        }

        /// <summary>
        /// Emits an error message to the listener.
        /// </summary>
        /// <param name="message">
        /// A message to emit.
        /// </param>
        /// <param name="detailMessage">
        /// A detailed message to emit.
        /// </param>
        public override void Fail(string message, string detailMessage)
        {
            string result = string.Join(TraceSeparator, message, detailMessage);
            this.WriteLine(result);
        }

        /// <summary>
        /// It is a internal method. Writes trace and event information to
        /// the listener.
        /// </summary>
        /// <param name="eventType">
        /// One of the System.Diagnostics.TraceEventType values specifying
        /// the type of event that has caused the trace.
        /// </param>
        /// <param name="message">
        /// A message to write.
        /// </param>
        private void InternalTraceMessage(TraceEventType eventType, string message)
        {
            Guid messageId;

            if (OperationContext.Current == null ||
                !OperationContext.Current.IncomingMessageHeaders.MessageId.TryGetGuid(out messageId))
            {
                DiagTraceHelper.TraceWarning("[SoaDiagTraceListener] InternalTraceMessage: Skip the event trace without message Id.");
                return;
            }

            int dispatchIdIndex = OperationContext.Current.IncomingMessageHeaders.FindHeader(Constant.DispatchIdHeaderName, Constant.HpcHeaderNS);
            if (dispatchIdIndex < 0)
            {
                DiagTraceHelper.TraceWarning("[SoaDiagTraceListener] InternalTraceMessage: Skip the event trace without dispatch Id.");
                return;
            }

            Guid dispatchId = OperationContext.Current.IncomingMessageHeaders.GetHeader<Guid>(dispatchIdIndex);

            DiagTraceHelper.TraceVerbose(
                "[SoaDiagTraceListener] InternalTraceMessage: Receive a trace: {0}, level: {1}, messageId: {2}, dispatchId: {3}",
                message,
                eventType,
                messageId,
                dispatchId);

            bool result;

            switch (eventType)
            {
                case TraceEventType.Critical:
                    result = RuntimeTraceHelper.RuntimeTrace.LogUserTraceCritial(this.sessionId, messageId, dispatchId, message);
                    break;

                case TraceEventType.Error:
                    result = RuntimeTraceHelper.RuntimeTrace.LogUserTraceError(this.sessionId, messageId, dispatchId, message);
                    break;

                case TraceEventType.Warning:
                    result = RuntimeTraceHelper.RuntimeTrace.LogUserTraceWarning(this.sessionId, messageId, dispatchId, message);
                    break;

                case TraceEventType.Information:
                    result = RuntimeTraceHelper.RuntimeTrace.LogUserTraceInfo(this.sessionId, messageId, dispatchId, message);
                    break;

                case TraceEventType.Verbose:
                    result = RuntimeTraceHelper.RuntimeTrace.LogUserTraceVerbose(this.sessionId, messageId, dispatchId, message);
                    break;

                default:
                    result = RuntimeTraceHelper.RuntimeTrace.LogUserTraceInfo(this.sessionId, messageId, dispatchId, message);
                    break;
            }

            if (!result)
            {
                EventProvider.WriteEventErrorCode code = EventProvider.GetLastWriteEventError();

                if (code == EventProvider.WriteEventErrorCode.EventTooBig)
                {
                    DiagTraceHelper.TraceError(
                        "[SoaDiagTraceListener] InternalTraceMessage: LastWriteEventError is {0}, message size is {1}",
                        code.ToString(),
                        message.Length);
                }
                else
                {
                    DiagTraceHelper.TraceError(
                        "[SoaDiagTraceListener] InternalTraceMessage: LastWriteEventError is {0}",
                        code.ToString());
                }
            }
        }
    }
}
