namespace BrokerLauncher.UnitTest.Mock
{
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher;

    internal class MockBrokerLauncher : IBrokerLauncher
    {
        internal static BrokerInitializationResult Result => new BrokerInitializationResult() { BrokerUniqueId = "UnitTestBrokerId" };

        public BrokerInitializationResult Create(SessionStartInfoContract info, int sessionId)
        {
            return Result;
        }

        public BrokerInitializationResult CreateDurable(SessionStartInfoContract info, int sessionId)
        {
            return Result;
        }

        public BrokerInitializationResult Attach(int sessionId)
        {
            return Result;
        }

        public void Close(int sessionId)
        {
           // no act
        }

        public bool PingBroker(int sessionID)
        {
            return true;
        }

        public string PingBroker2(int sessionID)
        {
            return "Yes";
        }

        public int[] GetActiveBrokerIdList()
        {
            return new[] { 1 };
        }
    }
}