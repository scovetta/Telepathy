namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    public static class CloudQueueConstants
    {
        public static string BrokerLauncherRequestQueueName => "brokerlaunchreq";

        public static string BrokerLauncherResponseQueueName => "brokerlaunchres";

        public static string BrokerWorkerControllerRequestQueueName => "brokerworkerctrlreq";

        public static string BrokerWorkerControllerResponseQueueName => "brokerworkerctrlres";
    }
}