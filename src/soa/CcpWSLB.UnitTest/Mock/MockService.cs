using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.Hpc.ServiceBroker.UnitTest.Mock
{
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
