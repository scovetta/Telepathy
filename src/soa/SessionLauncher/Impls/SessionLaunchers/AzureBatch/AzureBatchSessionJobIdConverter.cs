namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal static class AzureBatchSessionJobIdConverter
    {
        private const string AzureBatchSessionJobIdPrefix = "Session_";

        public static string ConvertToAzureBatchJobId(int sessionId)
        {
            return AzureBatchSessionJobIdPrefix + sessionId.ToString();
        }

        public static int ConvertToSessionId(string jobId)
        {
            if (!jobId.StartsWith(AzureBatchSessionJobIdPrefix))
            {
                Trace.TraceWarning($"{jobId} is not valid Azure Batch Session Job Id. Treated as empty");
                return -1;
            }

            return int.Parse(jobId.Substring(AzureBatchSessionJobIdPrefix.Length));
        }
    }
}
