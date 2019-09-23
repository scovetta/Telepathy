// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Telepathy.ServiceBroker.Common;

    /// <summary>
    /// Talk to service host management endpoint to, exit service host.
    /// </summary>
    internal class ServiceHostController
    {
        protected const int MaxExitServiceRetryCount = 1;
        protected ICommunicationObject controllerClient;
        protected EndpointAddress controllerEndpoint;
        protected Binding binding;
        protected int retryCount;
        protected Action OnFailed { get; set; }

        public ServiceHostController(EndpointAddress serviceHostControllerEndpoint, Binding binding)
        {
            this.controllerEndpoint = serviceHostControllerEndpoint;
            this.binding = binding;
        }

        public virtual void BeginExit(Action onFailed)
        {
            try
            {
                this.OnFailed = onFailed;
                BrokerTracing.TraceVerbose("[SerivceHostController].BeginExit: construct the client. Binding {0}, Endpoint {1}", this.binding, this.controllerEndpoint);
                HpcServiceHostClient serviceHostClient = new HpcServiceHostClient(this.binding, this.controllerEndpoint, BrokerIdentity.IsHAMode);
                this.controllerClient = serviceHostClient;
                BrokerTracing.TraceVerbose("[SerivceHostController].BeginExit: BeginExit. Binding {0}, Endpoint {1}", this.binding, this.controllerEndpoint);
                serviceHostClient.BeginExit(this.EndExit, serviceHostClient);
                BrokerTracing.TraceVerbose("[SerivceHostController].BeginExit: Called. Binding {0}, Endpoint {1}", this.binding, this.controllerEndpoint);
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceWarning("[SerivceHostController].BeginExit: Exception : {0}", ex);

                this.OnFailed();

                // If BeginExit fails, dont retry any further. The task likely terminated already
                if (this.controllerClient != null)
                {
                    Utility.AsyncCloseICommunicationObject(this.controllerClient);
                }
            }
        }

        private void EndExit(IAsyncResult result)
        {
            var client = (HpcServiceHostClient)result.AsyncState;
            bool needRetry = false;
            try
            {
                BrokerTracing.TraceVerbose("[SerivceHostController].EndExit.");
                client.EndExit(result);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("[SerivceHostController] Exception thrown while exiting service host: {0}", e);

                if (++this.retryCount <= MaxExitServiceRetryCount)
                {
                    needRetry = true;
                }
                else
                {
                    this.OnFailed();
                }
            }
            finally
            {
                Utility.AsyncCloseICommunicationObject(client);
                this.controllerClient = null;
            }

            if (needRetry)
            {
                this.BeginExit(this.OnFailed);
            }
        }
    }
}
