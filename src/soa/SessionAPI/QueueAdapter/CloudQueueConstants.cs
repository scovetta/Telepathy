namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    public static class CloudQueueConstants
    {
        public static string BrokerLauncherRequestQueueName => "brokerlaunchreq";

        public static string BrokerLauncherResponseQueueName => "brokerlaunchres";

        private const string BrokerWorkerControllerRequestQueueNamePrefix = "brokerworkerctrlreq";

        private const string BrokerWorkerControllerResponseQueueNamePrefix = "brokerworkerctrlres";

        public static string GetBrokerWorkerControllerRequestQueueName(int sessionId) => BrokerWorkerControllerRequestQueueNamePrefix + $"-{sessionId}";
        public static string GetBrokerWorkerControllerResponseQueueName(int sessionId) => BrokerWorkerControllerResponseQueueNamePrefix + $"-{sessionId}";
    }
}