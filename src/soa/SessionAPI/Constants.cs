namespace Microsoft.Hpc.Scheduler.Session
{
    public static class TelepathyConstants
    {
        public static int StandaloneSessionId => 0;

        public static string AzureTableBindingSchemePrefix => @"az.table://";

        public static string SessionLauncherAzureTableBindingAddress => $"{AzureTableBindingSchemePrefix}SessionLauncher";

        public static string SessionSchedulerDelegationAzureTableBindingAddress => $"{AzureTableBindingSchemePrefix}SchedulerDelegation";
    }
}
