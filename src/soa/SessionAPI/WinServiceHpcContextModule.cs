namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Threading;

    /// <summary>
    /// Helper class for NT Service to get HpcContext
    /// </summary>
    public static class WinServiceHpcContextModule
    {
        /// <summary>
        /// Get scheduler connection string from environment variable and create <see cref="HpcContext"/> from it
        /// </summary>
        /// <returns></returns>
        public static IHpcContext GetOrAddWinServiceHpcContextFromEnv()
            => HpcContext.GetOrAdd(Environment.GetEnvironmentVariable(HpcConstants.SchedulerEnvironmentVariableName), CancellationToken.None, true);
    }
}
