namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using TelepathyCommon.HpcContext;

    internal static class SchedulerHelperFactory
    {
        public static ISchedulerHelper GetSchedulerHelper(ITelepathyContext context)
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
