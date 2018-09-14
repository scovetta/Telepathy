//-----------------------------------------------------------------------
// <copyright file="AzureCountersTrace.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Azure node counter uses this to trace messages</summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.AzureRuntime.Worker
{
    using Microsoft.Hpc.Azure.Common;
    using Microsoft.Hpc.Scheduler;

    internal class AzureCountersTrace : WorkerTraceBase
    {
        static AzureCountersTrace _instance = new AzureCountersTrace();

        internal override int TraceEvtId
        {
            get { return TracingEventId.AzureNodeCounters; }
        }

        internal override string FacilityString
        {
            get { return "AzureNodeCounters"; }
        }

        internal static void TraceError(string format, params object[] args)
        {
            try
            {
                _instance.TraceErrorInternal(format, args);
            }
            catch
            {
            }
        }

        internal static void TraceError(int jobId, int taskId, int[] resourceId, string nodeName, string format, params object[] args)
        {
            string newMessage;
            object[] newArgs;
            SchedulerTracingUtil.GenMessageFormat(jobId, taskId, resourceId, nodeName, format, args, out newMessage, out newArgs);
            AzureCountersTrace.TraceError(newMessage, newArgs);
        }

        internal static void TraceInformation(string format, params object[] args)
        {
            try
            {
                _instance.TraceInformationInternal(format, args);
            }
            catch
            {
            }
        }

        internal static void TraceInformation(int jobId, int taskId, int[] resourceId, string nodeName, string format, params object[] args)
        {
            string newMessage;
            object[] newArgs;
            SchedulerTracingUtil.GenMessageFormat(jobId, taskId, resourceId, nodeName, format, args, out newMessage, out newArgs);
            AzureCountersTrace.TraceInformation(newMessage, newArgs);
        }

        internal static void TraceWarning(string format, params object[] args)
        {
            try
            {
                _instance.TraceWarningInternal(format, args);
            }
            catch
            {
            }
        }

        internal static void TraceWarning(int jobId, int taskId, int[] resourceId, string nodeName, string format, params object[] args)
        {
            string newMessage;
            object[] newArgs;
            SchedulerTracingUtil.GenMessageFormat(jobId, taskId, resourceId, nodeName, format, args, out newMessage, out newArgs);
            AzureCountersTrace.TraceWarning(newMessage, newArgs);
        }

        internal static void WriteLine(string format, params object[] args)
        {
            try
            {
                _instance.TraceVerboseInternal(format, args);
            }
            catch
            {
            }
        }

        internal static void WriteLine(int jobId, int taskId, int[] resourceId, string nodeName, string format, params object[] args)
        {
            string newMessage;
            object[] newArgs;
            SchedulerTracingUtil.GenMessageFormat(jobId, taskId, resourceId, nodeName, format, args, out newMessage, out newArgs);
            AzureCountersTrace.WriteLine(newMessage, newArgs);
        }
    }
}
