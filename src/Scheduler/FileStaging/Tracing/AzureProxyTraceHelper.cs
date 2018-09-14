using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Hpc.Azure.Common;

namespace Microsoft.Hpc.Azure.FileStaging
{
    class AzureProxyTraceHelper : ProxyTraceBase
    {
        static AzureProxyTraceHelper _instance = new AzureProxyTraceHelper();

        internal override int TraceEvtId
        {
            get { return TracingEventId.AzureFileStagingProxy; }
        }

        internal override string FacilityString
        {
            get { return "AzureFileStagingProxy"; }
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
    }
}
