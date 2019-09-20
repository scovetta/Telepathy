// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System.ServiceModel;

    [ServiceContract(Name = "IServiceControl", Namespace = "http://hpc.microsoft.com/hpcbrokerproxy/")]
    internal interface IProxyServiceManagement
    {
        [OperationContract(IsOneWay = true, Action = "http://hpc.microsoft.com/hpcbrokerproxy/exit")]
        void Exit(string machine, int jobId, int taskId, int port, BindingData data);
    }
}
