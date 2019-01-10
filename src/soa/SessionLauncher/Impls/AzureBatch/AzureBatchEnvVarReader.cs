namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch
{
    using System;

    public static class AzureBatchEnvVarReader
    {
        public static string GetJobId()
        {
            return Environment.GetEnvironmentVariable("AZ_BATCH_JOB_ID");
        }
    }
}
