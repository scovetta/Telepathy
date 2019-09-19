// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls
{
    public static class SessionLauncherRuntimeConfiguration
    {
        internal static bool AsConsole { get; set; } = false;

        internal static SchedulerType SchedulerType { get; set; } = SchedulerType.Unknown;

        internal static bool OpenAzureStorageListener => !string.IsNullOrEmpty(SessionLauncherStorageConnectionString);

        internal static string SessionLauncherStorageConnectionString { get; set; }
    }
}
