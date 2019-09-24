namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Mock
{
    using System.ServiceModel;

    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    internal class MockService : IEchoSvc
    {
        public string Echo(string input)
        {
            return input;
        }
    }

    [ServiceContract]
    public interface IEchoSvc
    {
        [OperationContract]
        string Echo(string input);
    }
}
