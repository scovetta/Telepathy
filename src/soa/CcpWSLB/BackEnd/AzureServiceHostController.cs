// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using Microsoft.Hpc.BrokerProxy;
    using Microsoft.Hpc.ServiceBroker.Common;

    /// <summary>
    /// Encapsualte logic that controls service hosts on Azure.
    /// </summary>
    internal class AzureServiceHostController : ServiceHostController
    {
        private AzureDispatcherInfo dispatcherInfo;

        private bool https;

        internal AzureServiceHostController(EndpointAddress controllerEndpoint, Binding binding, AzureDispatcherInfo info, bool https)
            : base(controllerEndpoint, ProxyBinding.BrokerProxyControllerBinding)
        {
            this.dispatcherInfo = info;

            this.https = https;
        }

        public override void BeginExit(Action onFailed)
        {
            try
            {
                this.OnFailed = onFailed;
                Binding controllerBinding = null;

                if (this.https)
                {
                    controllerBinding = ProxyBinding.BrokerProxyControllerHttpBinding;
                }
                else
                {
                    controllerBinding = ProxyBinding.BrokerProxyControllerBinding;
                }

                BrokerTracing.TraceVerbose("[AzureServiceHostController].BeginExit: construct the client. Binding {0}, Endpoint {1} , https = {2}", controllerBinding, this.controllerEndpoint, this.https);

                ProxyServiceControlClient proxyServiceControlClient
                    = new ProxyServiceControlClient(controllerBinding, this.controllerEndpoint);
                proxyServiceControlClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
                Utility.SetAzureClientCertificate(proxyServiceControlClient.ChannelFactory.Credentials);
                BrokerTracing.TraceVerbose("[AzureServiceHostController].BeginExit: BeginExit. Binding {0}, Endpoint {1}", controllerBinding, this.controllerEndpoint);

                this.controllerClient = proxyServiceControlClient;

                BindingData bindingData = new BindingData(this.binding);

                proxyServiceControlClient.BeginExit(
                    dispatcherInfo.MachineName,
                    dispatcherInfo.JobId,
                    dispatcherInfo.TaskId,
                    dispatcherInfo.FirstCoreId,
                    bindingData,
                    this.EndExit,
                    proxyServiceControlClient);

                BrokerTracing.TraceVerbose("[AzureServiceHostController].BeginExit: Called. Binding {0}, Endpoint {1}", controllerBinding, this.controllerEndpoint);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("[AzureServiceHostController].BeginExit: Error occurs, {0}", e);
                
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
            var client = (ProxyServiceControlClient)result.AsyncState;
            bool needRetry = false;
            try
            {
                BrokerTracing.TraceVerbose("[AzureServiceHostController].EndExit: Try to call EndExit method, https = {0}", this.https);

                client.EndExit(result);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("[AzureServiceHostController].EndExit: Exception thrown while exiting service host: {0}", e);

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
