//-----------------------------------------------------------------------
// <copyright file="IProxyServiceManagement.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Contract of service controller</summary>
//-----------------------------------------------------------------------

using System.ServiceModel;
using Microsoft.Hpc.ServiceBroker;

namespace Microsoft.Hpc.BrokerProxy
{
    [ServiceContract(Name = "IServiceControl", Namespace = "http://hpc.microsoft.com/hpcbrokerproxy/")]
    internal interface IProxyServiceManagement
    {
        [OperationContract(IsOneWay = true, Action = "http://hpc.microsoft.com/hpcbrokerproxy/exit")]
        void Exit(string machine, int jobId, int taskId, int port, BindingData data);
    }
}
