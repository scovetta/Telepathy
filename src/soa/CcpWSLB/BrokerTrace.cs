//-----------------------------------------------------------------------
// <copyright file="BrokerTrace.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Broker tracing helper class</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using Microsoft.Hpc.RuntimeTrace;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Broker tracing helper class
    /// </summary>
    public static class BrokerTracing
    {
        /// <summary>
        /// the session Id
        /// </summary>
        private static int sessionId;

        /// <summary>
        /// Gets the instance of RuntimeTraceWrapper.
        /// </summary>
        public static RuntimeTraceWrapper EtwTrace
        {
            get
            {
                return TraceHelper.RuntimeTrace;
            }
        }

        /// <summary>
        /// Initializes the broker tracing object
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        public static void Initialize(int sessionId)
        {
            BrokerTracing.sessionId = sessionId;
        }

        /// <summary>
        /// Trace an event
        /// </summary>
        /// <param name="eventType">indicate the event type</param>
        /// <param name="id">indicate the id</param>
        /// <param name="format">indicate the message format</param>
        /// <param name="args">indicate the message args</param>
        public static void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceHelper.TraceEvent(sessionId, eventType, format, args);
        }

        /// <summary>
        /// trace the error information.
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        public static void TraceError(string format, params object[] objparams)
        {
            BrokerTracing.TraceEvent(TraceEventType.Error, 0, format, objparams);
        }

        /// <summary>
        /// trace the warning information.
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        public static void TraceWarning(string format, params object[] objparams)
        {
            BrokerTracing.TraceEvent(TraceEventType.Warning, 0, format, objparams);
        }

        /// <summary>
        /// trace the information.
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        public static void TraceInfo(string format, params object[] objparams)
        {
            BrokerTracing.TraceEvent(TraceEventType.Information, 0, format, objparams);
        }

        /// <summary>
        /// trace the verbose information.
        /// </summary>
        /// <param name="format">the format string.</param>
        /// <param name="objparams">the parameters.</param>
        public static void TraceVerbose(string format, params object[] objparams)
        {
            BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, format, objparams);
        }

        /// <summary>
        /// Write the properties of the object to the string builder
        /// </summary>
        /// <param name="sb">indicating the string builder</param>
        /// <param name="obj">indicating the object</param>
        /// <param name="indent">indicating the indent</param>
        /// <param name="filterTypes">indicating the filter types</param>
        internal static void WriteProperties(StringBuilder sb, object obj, int indent, params Type[] filterTypes)
        {
            Type t = obj.GetType();
            List<Type> filterList = new List<Type>(filterTypes);
            foreach (PropertyInfo info in t.GetProperties())
            {
                if (filterList.Contains(info.PropertyType))
                {
                    sb.Append(' ', indent);
                    sb.AppendFormat("{0} = {1}\n", info.Name, info.GetValue(obj, null));
                }
            }
        }

        /// <summary>
        /// Generate trace string for broker.
        /// </summary>
        internal static string GenerateTraceString(string className, string methodName, int taskId, int clientIndex, string clientInfo, Guid messageId, string rawTraceString)
        {
            return InternalGenerateTraceString(className, methodName, taskId, clientIndex, clientInfo, messageId.ToString(), rawTraceString, null);
        }

        /// <summary>
        /// Generate trace string for broker.
        /// </summary>
        internal static string GenerateTraceString(string className, string methodName, int taskId, int clientIndex, string clientInfo, string messageIdString, string rawTraceString)
        {
            return InternalGenerateTraceString(className, methodName, taskId, clientIndex, clientInfo, messageIdString, rawTraceString, null);
        }

        /// <summary>
        /// Generate trace string for broker.
        /// </summary>
        internal static string GenerateTraceString(string className, string methodName, int taskId, int clientIndex, string clientInfo, Guid messageId, string rawTraceString, Exception e)
        {
            return InternalGenerateTraceString(className, methodName, taskId, clientIndex, clientInfo, messageId.ToString(), rawTraceString, e);
        }

        /// <summary>
        /// Generate trace string for broker.
        /// </summary>
        private static string InternalGenerateTraceString(string className, string methodName, int taskId, int clientIndex, string clientInfo, string messageIdString, string rawTraceString, Exception e)
        {
            string exceptionString = string.Empty;
            if (e != null)
            {
                exceptionString = string.Format("Exception={0}", e);
            }

            return string.Format(
                "[{0}] .{1}: TaskId={2}, ClientIndex={3}, MessageId={4}, {5} Client={6} {7}",
                className,
                methodName,
                taskId,
                clientIndex,
                messageIdString,
                rawTraceString,
                clientInfo,
                exceptionString);
        }
    }
}