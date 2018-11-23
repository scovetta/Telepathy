using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.Hpc.SvcBroker.UnitTest.Mock
{
    [ServiceContract]
    interface IService
    {
        [OperationContract]
        int Calc(int a);
    }

    class MockWCFClient : ClientBase<IService>, IService
    {
        public MockWCFClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

        #region IService Members

        public int Calc(int a)
        {
            return base.Channel.Calc(a);
        }

        #endregion
    }
}
