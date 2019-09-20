// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Mock
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;

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
