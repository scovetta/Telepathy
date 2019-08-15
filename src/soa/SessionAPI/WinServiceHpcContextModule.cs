using TelepathyCommon.HpcContext;

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Threading;

    using TelepathyCommon;

    /// <summary>
    /// Helper class for NT Service to get TelepathyContext
    /// </summary>
    public static class WinServiceHpcContextModule
    {
        /// <summary>
        /// Get scheduler connection string from environment variable and create <see cref="TelepathyContext"/> from it
        /// </summary>
        /// <returns></returns>
        public static ITelepathyContext GetOrAddWinServiceHpcContextFromEnv()
            => TelepathyContext.GetOrAdd(Environment.GetEnvironmentVariable(HpcConstants.SchedulerEnvironmentVariableName), CancellationToken.None, true);
    }
}
