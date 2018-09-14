//-----------------------------------------------------------------------
// <copyright file="DiagAzureTraceHelper.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     It is used by soa diag mon on Azure to write trace.
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics
{
    using Microsoft.Hpc.Azure.Common;

    /// <summary>
    /// Azure soa diag mon service uses this to trace messages
    /// </summary>
    internal class DiagAzureTraceHelper : WorkerTraceBase
    {
        /// <summary>
        /// Stores the instance of DiagAzureTraceHelper.
        /// </summary>
        private static DiagAzureTraceHelper instance = new DiagAzureTraceHelper();

        /// <summary>
        /// Get the trace event Id.
        /// </summary>
        internal override int TraceEvtId
        {
            get { return TracingEventId.AzureSoaDiagMon; }
        }

        /// <summary>
        /// Get the facility string.
        /// </summary>
        internal override string FacilityString
        {
            get { return "AzureSoaDiagMon"; }
        }

        /// <summary>
        /// Trace the error level message.
        /// </summary>
        /// <param name="format">message format</param>
        /// <param name="args">message arguments</param>
        internal static void TraceError(string format, params object[] args)
        {
            instance.TraceErrorInternal(format, args);
        }

        /// <summary>
        /// Trace the warning level message.
        /// </summary>
        /// <param name="format">message format</param>
        /// <param name="args">message arguments</param>
        internal static void TraceWarning(string format, params object[] args)
        {
            instance.TraceWarningInternal(format, args);
        }

        /// <summary>
        /// Trace the information level message.
        /// </summary>
        /// <param name="format">message format</param>
        /// <param name="args">message arguments</param>
        internal static void TraceInformation(string format, params object[] args)
        {
            instance.TraceInformationInternal(format, args);
        }

        /// <summary>
        /// Trace the verbose level message.
        /// </summary>
        /// <param name="format">message format</param>
        /// <param name="args">message arguments</param>
        internal static void WriteLine(string format, params object[] args)
        {
            instance.TraceVerboseInternal(format, args);
        }
    }
}
