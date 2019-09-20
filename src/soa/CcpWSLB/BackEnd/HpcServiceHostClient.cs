// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Hpc.Scheduler.Session.Interface;

#if Broker
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
#endif

    [ServiceContract(Name = "IHpcServiceHost", Namespace = "http://hpc.microsoft.com/hpcservicehost/")]
    internal interface IHpcServiceHostClient : IHpcServiceHost
    {
        [OperationContract(AsyncPattern = true, IsOneWay = true, Action = "http://hpc.microsoft.com/hpcservicehost/exit")]
        IAsyncResult BeginExit(AsyncCallback callback, object state);

        void EndExit(IAsyncResult result);
    }

    internal class HpcServiceHostClient : ClientBase<IHpcServiceHostClient>, IHpcServiceHostClient
    {
        public HpcServiceHostClient(Binding binding, EndpointAddress epr, bool shouldOpenExplicitly = false)
            : base(binding, epr)
        {
            // only open explicitly in HA mode.
            if (shouldOpenExplicitly)
            {
#if Broker
                // Bug 10301 : Explicitly open channel when impersonating the resource group's account if running on failover cluster so identity flows correctly when
                //      calling HpcServiceHost.
                //  NOTE: The patch we got from the WCF team (KB981001) only works when the caller is on a threadpool thread. 
                //  NOTE: Channel must be opened before setting OperationTimeout
                using (BrokerIdentity identity = new BrokerIdentity())
                {
                    identity.Impersonate();
#endif
                    this.Open();
#if Broker
                }
#endif
            }
        }

        /// <summary>
        /// Send exit command to service host
        /// </summary>
        /// <remarks>
        /// When this method is called by Azure broker proxy, impersonation is not needed.
        /// </remarks>
        public void Exit()
        {
#if Broker
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
#endif
                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
                IAsyncResult result = this.Channel.BeginExit(null, null);
                this.Channel.EndExit(result);
#if Broker
            }
#endif
        }

        /// <summary>
        /// Send exit command to service host async
        /// </summary>
        /// <remarks>
        /// When this method is called by Azure broker proxy, impersonation is not needed.
        /// </remarks>
        public IAsyncResult BeginExit(AsyncCallback callback, object state)
        {
#if Broker
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
#endif
                return this.Channel.BeginExit(callback, state);
#if Broker
            }
#endif
        }

        /// <summary>
        /// Complete send exit command to service host
        /// </summary>
        public void EndExit(IAsyncResult result)
        {
            this.Channel.EndExit(result);
        }
    }
}
