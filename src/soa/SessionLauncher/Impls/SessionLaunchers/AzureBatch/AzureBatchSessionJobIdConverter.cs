// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.AzureBatch
{
    using System.Diagnostics;

    internal static class AzureBatchSessionJobIdConverter
    {
        private const string AzureBatchSessionJobIdPrefix = "Session_";

        public static string ConvertToAzureBatchJobId(string sessionId)
        {
            return AzureBatchSessionJobIdPrefix + sessionId;
        }

        public static string ConvertToSessionId(string jobId)
        {
            if (!jobId.StartsWith(AzureBatchSessionJobIdPrefix))
            {
                Trace.TraceWarning($"{jobId} is not valid Azure Batch Session Job Id. Treated as empty");
                return "-1";
            }

            return jobId.Substring(AzureBatchSessionJobIdPrefix.Length);
        }
    }
}
