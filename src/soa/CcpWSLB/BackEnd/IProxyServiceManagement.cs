// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.ServiceModel;

namespace Microsoft.Hpc.BrokerProxy
{
    [ServiceContract(Name = "IServiceControl", Namespace = "http://hpc.microsoft.com/hpcbrokerproxy/")]
    internal interface IProxyServiceManagement
    {
        [OperationContract(IsOneWay = true, Action = "http://hpc.microsoft.com/hpcbrokerproxy/exit")]
        void Exit(string machine, int jobId, int taskId, int port, BindingData data);
    }
}
