namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    internal static class SchedulerHelperFactory
    {
        public static ISchedulerHelper GetSchedulerHelper(IHpcContext context)
        {
            if (BrokerLauncherEnvironment.Standalone)
            {
                return new DummySchedulerHelper();
            }
            else
            {
                return new SchedulerHelper(context);
            }
        }
    }
}
