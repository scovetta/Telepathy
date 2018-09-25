//-----------------------------------------------------------------------
// <copyright file="WssDispatcher.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Dispatch messages to broker proxy for Java WSS4J nodes in Windows Azure</summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using Microsoft.Hpc.BrokerProxy;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.ServiceBroker.Common;
    using System.Text;
    using System.Net;

    /// <summary>
    /// Dispatch messages to Java WSS4J node in Windows Azure
    /// </summary>
    internal class WssDispatcher : Dispatcher
    {
        /// <summary>
        /// Initializes a new instance of the WssDispatcher class
        /// </summary>
        /// <param name="info">indicating the dispatcher info</param>
        /// <param name="binding">binding information</param>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="observer">indicating the observer</param>
        /// <param name="queueFactory">indicating the queue factory</param>
        /// <param name="dispatcherIdle">set when the dispatcher enters idle status</param>
        public WssDispatcher(DispatcherInfo info, Binding binding, SharedData sharedData, BrokerObserver observer, BrokerQueueFactory queueFactory, SchedulerAdapterClientFactory schedulerAdapterClientFactory, AutoResetEvent dispatcherIdle)
            : base(info, binding, sharedData, observer, queueFactory, schedulerAdapterClientFactory, dispatcherIdle)
        {
            if (binding is BasicHttpBinding)
            {
                BasicHttpBinding httpBinding = binding as BasicHttpBinding;
                httpBinding.Security.Mode = BasicHttpSecurityMode.Message;
                httpBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.Certificate;
                httpBinding.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Basic128;
            }
            else
            {
                BrokerTracing.TraceWarning("[WssDispatcher]. The binding type is not HTTP {0}.", binding.GetType().ToString());
            }
        }

        /// <summary>
        /// Create OnPremiseRequestSender.
        /// </summary>
        /// <returns>OnPremiseRequestSender instance</returns>
        protected override RequestSender CreateRequestSender()
        {
            BrokerTracing.TraceVerbose("[WssDispatcher]. Create WssRequestSender for {0}.", this.Epr);

            return new WssRequestSender(
                this.Epr,
                this.BackendBinding,
                this.ServiceOperationTimeout,
                this,
                this.ServiceInitializationTimeout,
                InitEndpointNotFoundWaitPeriod);
        }

        protected override ResponseReceiver CreateResponseReceiver()
        {
            return new WssResponseReceiver(this);
        }

    }
}
