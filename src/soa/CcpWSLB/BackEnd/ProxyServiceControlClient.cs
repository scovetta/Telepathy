// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.Hpc.BrokerProxy;

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    [ServiceContract(Name = "IHpcServiceHost", Namespace = "http://hpc.microsoft.com/hpcbrokerproxy/")]
    internal interface IProxyServiceControlClient : IProxyServiceManagement
    {
        [OperationContractAttribute(AsyncPattern = true, IsOneWay = true, Action = "http://hpc.microsoft.com/hpcbrokerproxy/exit")]
        IAsyncResult BeginExit(string machine, string jobId, string taskId, int port, BindingData data, AsyncCallback callback, object state);

        void EndExit(IAsyncResult result);
    }


    internal class ProxyServiceControlClient : ClientBase<IProxyServiceControlClient>, IProxyServiceControlClient
    {
        public ProxyServiceControlClient(Binding binding, EndpointAddress epr)
            : base(binding, epr)
        {
        }

        public IAsyncResult BeginExit(string machine, string jobId, string taskId, int port, BindingData data, AsyncCallback callback, object state)
        {
            return this.Channel.BeginExit(machine, jobId, taskId, port, data, callback, state);
        }

        public void EndExit(IAsyncResult result)
        {
            this.Channel.EndExit(result);
        }

        public void Exit(string machine, int jobId, int taskId, int port, BindingData data)
        {
            this.Channel.Exit(machine, jobId, taskId, port, data);
        }
    }
}
