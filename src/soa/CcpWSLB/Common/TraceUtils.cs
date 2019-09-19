// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker
{
    using System.Diagnostics;

    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// It is helper class enabling writting trace on both on-premise side and
    /// proxy side.
    /// </summary>
    internal static class TraceUtils
    {
        /// <summary>
        /// Trace error level log.
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="methodName">method name</param>
        /// <param name="format">message format</param>
        /// <param name="args">arguments of message format</param>
        public static void TraceError(string className, string methodName, string format, params object[] args)
        {
            string traceMessage = SoaHelper.CreateTraceMessage(className, methodName, format, args);
#if PROXY
            Microsoft.Hpc.BrokerProxy.AzureBrokerProxyTrace.TraceError(traceMessage);
#elif DATAPROXY
            Microsoft.Hpc.Scheduler.Session.Data.Internal.DataProxyTrace.TraceError(traceMessage);
#elif DATASVC
            Microsoft.Hpc.Scheduler.Session.Data.Internal.DataServiceTraceHelper.TraceEvent(TraceEventType.Error, traceMessage);
#elif CONSOLE
            Console.Error.WriteLine(string.Format("{0} - {1}", DateTime.Now, traceMessage));
#else
            BrokerTracing.TraceError(traceMessage);
#endif
        }

        /// <summary>
        /// Trace warning level log.
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="methodName">method name</param>
        /// <param name="format">message format</param>
        /// <param name="args">arguments of message format</param>
        public static void TraceWarning(string className, string methodName, string format, params object[] args)
        {
            string traceMessage = SoaHelper.CreateTraceMessage(className, methodName, format, args);
#if PROXY
            Microsoft.Hpc.BrokerProxy.AzureBrokerProxyTrace.TraceWarning(traceMessage);
#elif DATAPROXY
            Microsoft.Hpc.Scheduler.Session.Data.Internal.DataProxyTrace.TraceWarning(traceMessage);
#elif DATASVC
            Microsoft.Hpc.Scheduler.Session.Data.Internal.DataServiceTraceHelper.TraceEvent(TraceEventType.Warning, traceMessage);
#elif CONSOLE
            Console.Out.WriteLine(string.Format("{0} - {1}", DateTime.Now, traceMessage));
#else
            BrokerTracing.TraceWarning(traceMessage);
#endif
        }

        /// <summary>
        /// Trace info level log.
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="methodName">method name</param>
        /// <param name="format">message format</param>
        /// <param name="args">arguments of message format</param>
        public static void TraceInfo(string className, string methodName, string format, params object[] args)
        {
            string traceMessage = SoaHelper.CreateTraceMessage(className, methodName, format, args);
#if PROXY
            Microsoft.Hpc.BrokerProxy.AzureBrokerProxyTrace.TraceInformation(traceMessage);
#elif DATAPROXY
            Microsoft.Hpc.Scheduler.Session.Data.Internal.DataProxyTrace.TraceInformation(traceMessage);
#elif DATASVC
            Microsoft.Hpc.Scheduler.Session.Data.Internal.DataServiceTraceHelper.TraceEvent(TraceEventType.Information, traceMessage);
#elif CONSOLE
            Console.Out.WriteLine(string.Format("{0} - {1}", DateTime.Now, traceMessage));
#else
            BrokerTracing.TraceInfo(traceMessage);
#endif
        }

        /// <summary>
        /// Trace verbose level log.
        /// </summary>
        /// <param name="className">class name</param>
        /// <param name="methodName">method name</param>
        /// <param name="format">message format</param>
        /// <param name="args">arguments of message format</param>
#if DATASVC
        [Conditional("DEBUG")]
#endif
        public static void TraceVerbose(string className, string methodName, string format, params object[] args)
        {
            string traceMessage = SoaHelper.CreateTraceMessage(className, methodName, format, args);
#if PROXY
            Microsoft.Hpc.BrokerProxy.AzureBrokerProxyTrace.WriteLine(traceMessage);
#elif DATAPROXY
            Microsoft.Hpc.Scheduler.Session.Data.Internal.DataProxyTrace.TraceVerbose(traceMessage);
#elif DATASVC
            Microsoft.Hpc.Scheduler.Session.Data.Internal.DataServiceTraceHelper.TraceEvent(TraceEventType.Verbose, traceMessage);
#elif CONSOLE
            Console.Out.WriteLine(string.Format("{0} - {1}", DateTime.Now, traceMessage));
#else
            BrokerTracing.TraceVerbose(traceMessage);
#endif
        }
    }
}
