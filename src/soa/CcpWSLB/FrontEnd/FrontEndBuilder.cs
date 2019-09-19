// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net.NetworkInformation;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Xml;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    using SR = Microsoft.Hpc.SvcBroker.SR;

    /// <summary>
    /// FrontEnd Builder
    /// </summary>
    internal static class FrontEndBuilder
    {
        /// <summary>
        /// Store the net tcp suffix
        /// </summary>
        private const string NetTcpScheme = "net.tcp";

        /// <summary>
        /// Store the http suffix
        /// </summary>
        private const string HttpScheme = "http";

        /// <summary>
        /// Store the https suffix
        /// </summary>
        private const string HttpsScheme = "https";

        /// <summary>
        /// Store the default net.tcp frontend uri
        /// </summary>
        private static readonly string DefaultNetTcpFrontEndServiceUri = SoaHelper.GetBrokerWorkerAddress("localhost");

        /// <summary>
        /// Store the internal net.tcp frontend uri
        /// </summary>
        private static readonly string InternalNetTcpFrontEndServiceUri = SoaHelper.GetBrokerWorkerInternalAddress("localhost");

        /// <summary>
        /// Store the internal net.tcp frontend uri
        /// </summary>
        private static readonly string AadNetTcpFrontEndServiceUri = SoaHelper.GetBrokerWorkerAadAddress("localhost");

        /// <summary>
        /// Store the default http frontend uri
        /// </summary>
        private const string DefaultHttpFrontEndServiceUri = "http://localhost/Broker";

        /// <summary>
        /// Store the default https frontend uri
        /// </summary>
        private const string DefaultHttpsFrontEndServiceUri = "https://localhost/Broker";

        /// <summary>
        /// Store the default controller postfix
        /// </summary>
        private const string DefaultControllerPostfix = "Controller";

        /// <summary>
        /// Store the default get response postfix
        /// </summary>
        private const string DefaultGetResponsePostfix = "GetResponse";

        /// <summary>
        /// Build the frontend
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="brokerAuth">indicating the broker authorization</param>
        /// <param name="bindings">indicating the bindings</param>
        /// <param name="azureQueueProxy">indicating the Azure storage proxy</param>
        /// <returns>frontend result</returns>
        public static FrontendResult BuildFrontEnd(SharedData sharedData, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, BindingsSection bindings, AzureQueueProxy azureQueueProxy)
        {
            FrontendResult result = new FrontendResult();

            // Bug 9514: Do not open frontend for inprocess broker
            if (sharedData.StartInfo.UseInprocessBroker)
            {
                return result;
            }

            bool flag = false;

            // Open frontend for different scheme

            // TODO: Separate HTTP frontend and Queue frontend
            if (azureQueueProxy != null)
            {
                flag = true;
                result.SetFrontendInfo(BuildHttpFrontend(sharedData, observer, clientManager, brokerAuth, bindings, azureQueueProxy), TransportScheme.Http);
            }
            else
            {
                if ((sharedData.StartInfo.TransportScheme & TransportScheme.Custom) == TransportScheme.Custom)
                {
                    flag = true;
                    result.SetFrontendInfo(BuildCustomFrontend(sharedData, observer, clientManager, brokerAuth, bindings), TransportScheme.Custom);
                }

                if ((sharedData.StartInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http)
                {
                    flag = true;
                    result.SetFrontendInfo(BuildHttpFrontend(sharedData, observer, clientManager, brokerAuth, bindings, azureQueueProxy), TransportScheme.Http);
                }

                if ((sharedData.StartInfo.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
                {
                    flag = true;
                    result.SetFrontendInfo(BuildNetTcpFrontend(sharedData, observer, clientManager, brokerAuth, bindings), TransportScheme.NetTcp);
                }

                if ((sharedData.StartInfo.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
                {
                    flag = true;
                    result.SetFrontendInfo(BuildNetHttpFrontend(sharedData, observer, clientManager, brokerAuth, bindings), TransportScheme.NetHttp);
                }
            }

            if (!flag)
            {
                BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[FrontEndBuilder] Invalid transport scheme: {0}", sharedData.StartInfo.TransportScheme);
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_NotSupportedTransportScheme, SR.NotSupportedTransportScheme, sharedData.StartInfo.TransportScheme.ToString());
            }

            return result;
        }

        /// <summary>
        /// Build the net.tcp frontend
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="brokerAuth">indicating the broker authorization</param>
        /// <param name="bindings">indicating the bindings</param>
        /// <returns>frontend info</returns>
        private static FrontendInfo BuildNetTcpFrontend(SharedData sharedData, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, BindingsSection bindings)
        {
            FrontendInfo result = new FrontendInfo();
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Start building net.tcp frontend...");

            // Get binding from configuration file
            Binding binding = BindingHelper.GetBinding(TransportScheme.NetTcp, sharedData.StartInfo.Secure, bindings, sharedData.StartInfo.LocalUser.GetValueOrDefault(), sharedData.StartInfo.UseAad);
            
            // Sync frontend binding.receiveTimeout with loadBalancing.ServiceOperationTimeout if its enabled (>0)
            int serviceOperationTimeout = sharedData.Config.LoadBalancing.ServiceOperationTimeout;
            if (serviceOperationTimeout > 0)
            {
                binding.SendTimeout = TimeSpan.FromMilliseconds(serviceOperationTimeout);
            }

            // Set frontend binding.ReceiveTimeout to max
            binding.ReceiveTimeout = TimeSpan.MaxValue;

            // Update backend binding's maxMessageSize settings with global maxMessageSize if its enabled (> 0)
            int maxMessageSize = sharedData.ServiceConfig.MaxMessageSize;
            if (maxMessageSize > 0)
            {
                BindingHelper.ApplyMaxMessageSize(binding, maxMessageSize);
                result.MaxMessageSize = maxMessageSize;
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 1: Apply global MaxMessageSize:{0}\n", result.MaxMessageSize);
            }
            else
            {
                // Get message size and reader quotas
                BindingElementCollection collection = binding.CreateBindingElements();
                result.MaxMessageSize = collection.Find<TransportBindingElement>().MaxReceivedMessageSize;
                result.ReaderQuotas = binding.GetProperty<XmlDictionaryReaderQuotas>(new BindingParameterCollection());

                StringBuilder sb = new StringBuilder();
                BrokerTracing.WriteProperties(sb, result.ReaderQuotas, 3, typeof(int));
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 1: Load binding data:\nMaxMessageSize = {0}\n[ReaderQuotas]\n{1}", result.MaxMessageSize, sb.ToString());
            }

            result.Binding = binding;
            BrokerTracing.EtwTrace.LogFrontendBindingLoaded(
                sharedData.BrokerInfo.SessionId,
                "NetTcp",
                result.MaxMessageSize,
                binding.ReceiveTimeout.Ticks,
                binding.SendTimeout.Ticks,
                binding.MessageVersion.ToString(),
                binding.Scheme);

            // Generate the net.tcp uri
            Uri baseNetTcpUri = sharedData.Config.Services.GetBrokerBaseAddress(NetTcpScheme);
            if (baseNetTcpUri == null)
            {
                string netTcpUriStr;
                if (sharedData.StartInfo.LocalUser.GetValueOrDefault())
                {
                    netTcpUriStr = InternalNetTcpFrontEndServiceUri;
                }
                else if (sharedData.StartInfo.UseAad)
                {
                    netTcpUriStr = AadNetTcpFrontEndServiceUri;
                }
                else
                {
                    netTcpUriStr = DefaultNetTcpFrontEndServiceUri;
                }

                baseNetTcpUri = new Uri(netTcpUriStr);
            }

            if (SoaHelper.IsOnAzure())
            {
                baseNetTcpUri = new Uri(SoaHelper.GetBrokerWorkerAddress("localhost"));
            }

            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 2: Load base address: {0}", baseNetTcpUri);

            Uri brokerNetTcpUri = ApplySessionId(baseNetTcpUri, sharedData.BrokerInfo.SessionId, "NetTcp", sharedData.BrokerInfo.EnableFQDN);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 3: Generate broker uri: {0}", brokerNetTcpUri);

            // Build the frontend
            result.FrontEnd = new DuplexFrontEnd(brokerNetTcpUri, binding, observer, clientManager, brokerAuth, sharedData);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 4: Build broker frontend succeeded.");
            BrokerTracing.EtwTrace.LogFrontendCreated(
                sharedData.BrokerInfo.SessionId,
                "NetTcp",
                result.FrontEnd.ListenUri);

            Uri controllerNetTcpUri = ApplySessionId(baseNetTcpUri, sharedData.BrokerInfo.SessionId, "NetTcp", sharedData.BrokerInfo.EnableFQDN);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 5: Generate controller base address: {0}", controllerNetTcpUri);

            if (SoaHelper.IsOnAzure())
            {
                result.ControllerFrontend = new ServiceHost(typeof(BrokerController));
                BindingHelper.ApplyDefaultThrottlingBehavior(result.ControllerFrontend);

                ServiceEndpoint controllerEndpoint = result.ControllerFrontend.AddServiceEndpoint(
                    typeof(IController),
                    binding,
                    new Uri(SoaHelper.GetBrokerControllerAddress(sharedData.BrokerInfo.SessionId)),
                    new Uri(Path.Combine(controllerNetTcpUri.ToString(), DefaultControllerPostfix)));
                controllerEndpoint.Behaviors.Add(new ControllerFrontendProvider(false, clientManager, brokerAuth, observer, null));
                result.ControllerUri = controllerEndpoint.ListenUri.AbsoluteUri;

                ServiceEndpoint getResponseEndpoint = result.ControllerFrontend.AddServiceEndpoint(
                    typeof(IResponseService),
                    binding,
                    new Uri(SoaHelper.GetBrokerGetResponseAddress(sharedData.BrokerInfo.SessionId)),
                    new Uri(Path.Combine(controllerNetTcpUri.ToString(), DefaultGetResponsePostfix)));
                getResponseEndpoint.Behaviors.Add(new ControllerFrontendProvider(false, clientManager, brokerAuth, observer, null));
                result.GetResponseUri = getResponseEndpoint.ListenUri.AbsoluteUri;
            }
            else
            {
                result.ControllerFrontend = new ServiceHost(typeof(BrokerController), controllerNetTcpUri);
                BindingHelper.ApplyDefaultThrottlingBehavior(result.ControllerFrontend);

                ServiceEndpoint controllerEndpoint = result.ControllerFrontend.AddServiceEndpoint(typeof(IController), binding, DefaultControllerPostfix);
                controllerEndpoint.Behaviors.Add(new ControllerFrontendProvider(false, clientManager, brokerAuth, observer, null));
                result.ControllerUri = controllerEndpoint.ListenUri.AbsoluteUri;

                ServiceEndpoint getResponseEndpoint = result.ControllerFrontend.AddServiceEndpoint(typeof(IResponseService), binding, DefaultGetResponsePostfix);
                getResponseEndpoint.Behaviors.Add(new ControllerFrontendProvider(false, clientManager, brokerAuth, observer, null));
                result.GetResponseUri = getResponseEndpoint.ListenUri.AbsoluteUri;
                if (sharedData.StartInfo.UseAad)
                {
                   throw new NotSupportedException();
                }
                else if (sharedData.StartInfo.LocalUser.GetValueOrDefault())
                {
                    BrokerTracing.TraceVerbose("[FrontEndBuilder] Building net.tcp frontend with internal authentication.");
                    result.ControllerFrontend.Credentials.UseInternalAuthenticationAsync(true).GetAwaiter().GetResult();
                }
            }

            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 6: Build controller frontend succeeded: ControllerUri = {0}, GetResponseUri = {1}", result.ControllerUri, result.GetResponseUri);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Building net.tcp frontend succeeded.");
            BrokerTracing.EtwTrace.LogFrontendControllerCreated(
                sharedData.BrokerInfo.SessionId,
                "NetTcp",
                result.ControllerUri,
                result.GetResponseUri);
            return result;
        }

        /// <summary>
        /// Build the NetHttp frontend
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="brokerAuth">indicating the broker authorization</param>
        /// <param name="bindings">indicating the bindings</param>
        /// <returns>frontend info</returns>
        private static FrontendInfo BuildNetHttpFrontend(SharedData sharedData, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, BindingsSection bindings)
        {
            FrontendInfo result = new FrontendInfo();
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Start building NetHttp frontend...");

            // Get binding from configuration file
            Binding binding = BindingHelper.GetBinding(TransportScheme.NetHttp, sharedData.StartInfo.Secure, bindings);

            // Sync frontend binding.receiveTimeout with loadBalancing.ServiceOperationTimeout if its enabled (>0)
            int serviceOperationTimeout = sharedData.Config.LoadBalancing.ServiceOperationTimeout;
            if (serviceOperationTimeout > 0)
            {
                binding.SendTimeout = TimeSpan.FromMilliseconds(serviceOperationTimeout);
            }

            // Set frontend binding.ReceiveTimeout to max
            binding.ReceiveTimeout = TimeSpan.MaxValue;

            // Update backend binding's maxMessageSize settings with global maxMessageSize if its enabled (> 0)
            int maxMessageSize = sharedData.ServiceConfig.MaxMessageSize;
            if (maxMessageSize > 0)
            {
                BindingHelper.ApplyMaxMessageSize(binding, maxMessageSize);
                result.MaxMessageSize = maxMessageSize;
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 1: Apply global MaxMessageSize:{0}\n", result.MaxMessageSize);
            }
            else
            {
                // Get message size and reader quotas
                BindingElementCollection collection = binding.CreateBindingElements();
                result.MaxMessageSize = collection.Find<TransportBindingElement>().MaxReceivedMessageSize;
                result.ReaderQuotas = binding.GetProperty<XmlDictionaryReaderQuotas>(new BindingParameterCollection());

                StringBuilder sb = new StringBuilder();
                BrokerTracing.WriteProperties(sb, result.ReaderQuotas, 3, typeof(int));
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 1: Load binding data:\nMaxMessageSize = {0}\n[ReaderQuotas]\n{1}", result.MaxMessageSize, sb.ToString());
            }

            result.Binding = binding;
            BrokerTracing.EtwTrace.LogFrontendBindingLoaded(
                sharedData.BrokerInfo.SessionId,
                "NetHttp",
                result.MaxMessageSize,
                binding.ReceiveTimeout.Ticks,
                binding.SendTimeout.Ticks,
                binding.MessageVersion.ToString(),
                binding.Scheme);

            // Generate the http uri
            Uri basehttpUri = sharedData.Config.Services.GetBrokerBaseAddress(sharedData.StartInfo.Secure ? HttpsScheme : HttpScheme);
            if (basehttpUri == null)
            {
                basehttpUri = new Uri(sharedData.StartInfo.Secure ? DefaultHttpsFrontEndServiceUri : DefaultHttpFrontEndServiceUri);
            }

            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 2: Load base address: {0}", basehttpUri);

            Uri brokerNetHttpUri = ApplySessionId(basehttpUri, sharedData.BrokerInfo.SessionId, "NetHttp", sharedData.BrokerInfo.EnableFQDN);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 3: Generate broker uri: {0}", brokerNetHttpUri);

            // Build the frontend
            result.FrontEnd = new DuplexFrontEnd(brokerNetHttpUri, binding, observer, clientManager, brokerAuth, sharedData);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 4: Build broker frontend succeeded.");
            BrokerTracing.EtwTrace.LogFrontendCreated(
                sharedData.BrokerInfo.SessionId,
                "NetHttp",
                result.FrontEnd.ListenUri);

            Uri controllerNetHttpUri = ApplySessionId(basehttpUri, sharedData.BrokerInfo.SessionId, "NetHttp", sharedData.BrokerInfo.EnableFQDN);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 5: Generate controller base address: {0}", controllerNetHttpUri);

            result.ControllerFrontend = new ServiceHost(typeof(BrokerController), controllerNetHttpUri);
            BindingHelper.ApplyDefaultThrottlingBehavior(result.ControllerFrontend);

            ServiceEndpoint controllerEndpoint = result.ControllerFrontend.AddServiceEndpoint(typeof(IController), binding, DefaultControllerPostfix);
            controllerEndpoint.Behaviors.Add(new ControllerFrontendProvider(false, clientManager, brokerAuth, observer, null));
            result.ControllerUri = controllerEndpoint.ListenUri.AbsoluteUri;

            ServiceEndpoint getResponseEndpoint = result.ControllerFrontend.AddServiceEndpoint(typeof(IResponseService), binding, DefaultGetResponsePostfix);
            getResponseEndpoint.Behaviors.Add(new ControllerFrontendProvider(false, clientManager, brokerAuth, observer, null));
            result.GetResponseUri = getResponseEndpoint.ListenUri.AbsoluteUri;

            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 6: Build controller frontend succeeded: ControllerUri = {0}, GetResponseUri = {1}", result.ControllerUri, result.GetResponseUri);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Building NetHttp frontend succeeded.");
            BrokerTracing.EtwTrace.LogFrontendControllerCreated(
                sharedData.BrokerInfo.SessionId,
                "NetHttp",
                result.ControllerUri,
                result.GetResponseUri);
            return result;
        }

        /// <summary>
        /// Build the http frontend
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="brokerAuth">indicating the broker authorization</param>
        /// <param name="bindings">indicating the bindings</param>
        /// <returns>frontend info</returns>
        private static FrontendInfo BuildHttpFrontend(SharedData sharedData, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, BindingsSection bindings, AzureQueueProxy azureQueueProxy)
        {
            FrontendInfo result = new FrontendInfo();
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Start building http frontend...");

            // Get binding from configuration file
            Binding binding = BindingHelper.GetBinding(TransportScheme.Http, sharedData.StartInfo.Secure, bindings);

            // Sync frontend binding.receiveTimeout with loadBalancing.ServiceOperationTimeout if its enabled (>0)
            int serviceOperationTimeout = sharedData.Config.LoadBalancing.ServiceOperationTimeout;
            if (serviceOperationTimeout > 0)
            {
                binding.SendTimeout = TimeSpan.FromMilliseconds(serviceOperationTimeout);
            }

            // Set frontend binding.ReceiveTimeout to max
            binding.ReceiveTimeout = TimeSpan.MaxValue;

            // Get message size and reader quotas
            int maxMessageSize = sharedData.ServiceConfig.MaxMessageSize;
            if (maxMessageSize > 0)
            {
                BindingHelper.ApplyMaxMessageSize(binding, maxMessageSize);
                result.MaxMessageSize = maxMessageSize;
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 1: Apply global MaxMessageSize:{0}\n", result.MaxMessageSize);
            }
            else
            {
                BindingElementCollection collection = binding.CreateBindingElements();
                result.MaxMessageSize = collection.Find<TransportBindingElement>().MaxReceivedMessageSize;
                result.ReaderQuotas = binding.GetProperty<XmlDictionaryReaderQuotas>(new BindingParameterCollection());

                StringBuilder sb = new StringBuilder();
                BrokerTracing.WriteProperties(sb, result.ReaderQuotas, 3, typeof(int));
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 1: Load binding data:\nMaxMessageSize = {0}\n[ReaderQuotas]\n{1}", result.MaxMessageSize, sb.ToString());
            }

            result.Binding = binding;
            BrokerTracing.EtwTrace.LogFrontendBindingLoaded(
                sharedData.BrokerInfo.SessionId,
                "Http",
                result.MaxMessageSize,
                binding.ReceiveTimeout.Ticks,
                binding.SendTimeout.Ticks,
                binding.MessageVersion.ToString(),
                binding.Scheme);

            // Generate the http uri
            Uri basehttpUri = sharedData.Config.Services.GetBrokerBaseAddress(sharedData.StartInfo.Secure ? HttpsScheme : HttpScheme);
            if (basehttpUri == null)
            {
                basehttpUri = new Uri(sharedData.StartInfo.Secure ? DefaultHttpsFrontEndServiceUri : DefaultHttpFrontEndServiceUri);
            }

            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 2: Load base address: {0}", basehttpUri);

            Uri brokerHttpUri = ApplySessionId(basehttpUri, sharedData.BrokerInfo.SessionId, "Http", sharedData.BrokerInfo.EnableFQDN);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 3: Generate broker uri: {0}", brokerHttpUri);

            // Build the frontend
            if (sharedData.StartInfo.UseAzureStorage == true)
            {
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: AzureQueueFrontEnd");
                result.FrontEnd = new AzureQueueFrontEnd(azureQueueProxy, brokerHttpUri, observer, clientManager, brokerAuth, sharedData);
            }
            else
            {
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: HttpFrontEnd");
                result.FrontEnd = new RequestReplyFrontEnd<IReplyChannel>(brokerHttpUri, binding, observer, clientManager, brokerAuth, sharedData);
                //result.FrontEnd = new OutputInputFrontEnd<IInputChannel>(brokerHttpUri, binding, observer, clientManager, brokerAuth, sharedData);
            }
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 4: Build broker frontend succeeded.");
            BrokerTracing.EtwTrace.LogFrontendCreated(
                sharedData.BrokerInfo.SessionId,
                "Http",
                result.FrontEnd.ListenUri);

            Uri controllerHttpUri = ApplySessionId(basehttpUri, sharedData.BrokerInfo.SessionId, "Http", sharedData.BrokerInfo.EnableFQDN);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 5: Generate controller base address: {0}", controllerHttpUri);

            // Bug 3012: Force the http frontend using the singleton instance
            result.ControllerFrontend = new ServiceHost(typeof(BrokerController), controllerHttpUri);
            BindingHelper.ApplyDefaultThrottlingBehavior(result.ControllerFrontend);
            ServiceBehaviorAttribute behavior = result.ControllerFrontend.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            Debug.Assert(behavior != null, "BrokerController must have a behavior.");
            behavior.InstanceContextMode = InstanceContextMode.Single;
            ServiceEndpoint endpoint = result.ControllerFrontend.AddServiceEndpoint(typeof(IController), binding, DefaultControllerPostfix);
            if (sharedData.StartInfo.UseAzureStorage == true)
            {
                endpoint.Behaviors.Add(new ControllerFrontendProvider(true, clientManager, brokerAuth, observer, azureQueueProxy));
            }
            else
            {
                endpoint.Behaviors.Add(new ControllerFrontendProvider(true, clientManager, brokerAuth, observer, null));
            }
            result.ControllerUri = endpoint.ListenUri.AbsoluteUri;
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 6: Build controller frontend succeeded: ControllerUri = {0}", result.ControllerUri);

            result.GetResponseUri = null;

            BrokerTracing.TraceVerbose("[FrontEndBuilder] Building http frontend succeeded.");
            BrokerTracing.EtwTrace.LogFrontendControllerCreated(
                sharedData.BrokerInfo.SessionId,
                "Http",
                result.ControllerUri,
                String.Empty);
            return result;
        }

        /// <summary>
        /// Build the custom frontend
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="brokerAuth">indicating the broker authorization</param>
        /// <param name="bindings">indicating the bindings</param>
        /// <returns>frontend info</returns>
        private static FrontendInfo BuildCustomFrontend(SharedData sharedData, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, BindingsSection bindings)
        {
            FrontendInfo result = new FrontendInfo();
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Start building custom frontend...");

            // Get binding from configuration file
            Binding binding = BindingHelper.GetBinding(TransportScheme.Custom, sharedData.StartInfo.Secure, bindings);

            // Get message size and reader quotas
            BindingElementCollection collection = binding.CreateBindingElements();

            // Sync frontend binding.receiveTimeout with loadBalancing.ServiceOperationTimeout if its enabled (>0)
            int serviceOperationTimeout = sharedData.Config.LoadBalancing.ServiceOperationTimeout;
            if (serviceOperationTimeout > 0)
            {
                binding.SendTimeout = TimeSpan.FromMilliseconds(serviceOperationTimeout);
            }

            // Set frontend binding.ReceiveTimeout to max
            binding.ReceiveTimeout = TimeSpan.MaxValue;

            TransportBindingElement transportBindingElement = collection.Find<TransportBindingElement>();
            XmlDictionaryReaderQuotas quotas = binding.GetProperty<XmlDictionaryReaderQuotas>(new BindingParameterCollection());
            if (sharedData.ServiceConfig.MaxMessageSize > 0)
            {
                transportBindingElement.MaxReceivedMessageSize = sharedData.Config.LoadBalancing.ServiceOperationTimeout;
                quotas.MaxBytesPerRead = XmlDictionaryReaderQuotas.Max.MaxBytesPerRead;
                quotas.MaxDepth = XmlDictionaryReaderQuotas.Max.MaxDepth;
                quotas.MaxNameTableCharCount = XmlDictionaryReaderQuotas.Max.MaxNameTableCharCount;
                quotas.MaxStringContentLength = Convert.ToInt32(sharedData.ServiceConfig.MaxMessageSize);
                quotas.MaxArrayLength = Convert.ToInt32(sharedData.ServiceConfig.MaxMessageSize);
            }

            result.MaxMessageSize = transportBindingElement.MaxReceivedMessageSize;
            result.ReaderQuotas = quotas;

            // Check if the custom binding supports duplex binding
            bool duplex = false;
            if (binding.CanBuildChannelListener<IDuplexSessionChannel>())
            {
                duplex = true;
            }

            StringBuilder sb = new StringBuilder();
            BrokerTracing.WriteProperties(sb, result.ReaderQuotas, 3, typeof(int));
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 1: Load binding data:\nMaxMessageSize = {0}\n[ReaderQuotas]\n{1}\nDuplex = {2}", result.MaxMessageSize, sb.ToString(), duplex);

            result.Binding = binding;
            BrokerTracing.EtwTrace.LogFrontendBindingLoaded(
                sharedData.BrokerInfo.SessionId,
                "Custom",
                result.MaxMessageSize,
                binding.ReceiveTimeout.Ticks,
                binding.SendTimeout.Ticks,
                binding.MessageVersion.ToString(),
                binding.Scheme);

            // Generate the net.tcp uri
            Uri baseUri = sharedData.Config.Services.GetBrokerBaseAddress(binding.Scheme);
            if (baseUri == null)
            {
                baseUri = GetDefaultUriByScheme(binding.Scheme);
            }

            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 2: Load base address: {0}", baseUri);

            Uri brokerUri = ApplySessionId(baseUri, sharedData.BrokerInfo.SessionId, "Custom", sharedData.BrokerInfo.EnableFQDN);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 3: Generate broker uri: {0}", brokerUri);

            // Build the frontend
            if (duplex)
            {
                result.FrontEnd = new DuplexFrontEnd(brokerUri, binding, observer, clientManager, brokerAuth, sharedData);
            }
            else
            {
                if (binding.CanBuildChannelListener<IReplySessionChannel>())
                {
                    result.FrontEnd = new RequestReplyFrontEnd<IReplySessionChannel>(brokerUri, binding, observer, clientManager, brokerAuth, sharedData);
                }
                else if (binding.CanBuildChannelListener<IReplyChannel>())
                {
                    result.FrontEnd = new RequestReplyFrontEnd<IReplyChannel>(brokerUri, binding, observer, clientManager, brokerAuth, sharedData);
                }
                else
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BindingNotSupported, SR.BindingNotSupported, binding.Name);
                }
            }

            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 4: Build broker frontend succeeded.");
            BrokerTracing.EtwTrace.LogFrontendCreated(
                sharedData.BrokerInfo.SessionId,
                "Custom",
                result.FrontEnd.ListenUri);

            Uri controllerUri = ApplySessionId(baseUri, sharedData.BrokerInfo.SessionId, "Custom", sharedData.BrokerInfo.EnableFQDN);
            BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 5: Generate controller base address: {0}", controllerUri);

            result.ControllerFrontend = new ServiceHost(typeof(BrokerController), controllerUri);
            BindingHelper.ApplyDefaultThrottlingBehavior(result.ControllerFrontend);
            ServiceEndpoint controllerEndpoint = result.ControllerFrontend.AddServiceEndpoint(typeof(IController), binding, DefaultControllerPostfix);
            controllerEndpoint.Behaviors.Add(new ControllerFrontendProvider(false, clientManager, brokerAuth, observer, null));
            result.ControllerUri = controllerEndpoint.ListenUri.AbsoluteUri;

            // Check if the binding supports duplex channel
            // If so, create GetResponse frontend
            if (binding.CanBuildChannelListener<IDuplexChannel>() || binding.CanBuildChannelListener<IDuplexSessionChannel>())
            {
                ServiceEndpoint getResponseEndpoint = result.ControllerFrontend.AddServiceEndpoint(typeof(IResponseService), binding, DefaultGetResponsePostfix);
                getResponseEndpoint.Behaviors.Add(new ControllerFrontendProvider(false, clientManager, brokerAuth, observer, null));
                result.GetResponseUri = getResponseEndpoint.ListenUri.AbsoluteUri;
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 6: Build controller frontend succeeded: ControllerUri = {0}, GetResponseUri = {1}", result.ControllerUri, result.GetResponseUri);
            }
            else
            {
                BrokerTracing.TraceVerbose("[FrontEndBuilder] Build frontend: Step 6: Build controller frontend succeeded: ControllerUri = {0}", result.ControllerUri);
            }

            BrokerTracing.TraceVerbose("[FrontEndBuilder] Building custom frontend succeeded.");
            BrokerTracing.EtwTrace.LogFrontendControllerCreated(
                sharedData.BrokerInfo.SessionId,
                "Custom",
                result.ControllerUri,
                result.GetResponseUri ?? String.Empty);
            return result;
        }

        /// <summary>
        /// Get default uri by scheme
        /// </summary>
        /// <param name="scheme">indicating the scheme</param>
        /// <returns>the default uri</returns>
        private static Uri GetDefaultUriByScheme(string scheme)
        {
            if (String.Compare(scheme, NetTcpScheme, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return new Uri(DefaultNetTcpFrontEndServiceUri);
            }
            else if (String.Compare(scheme, HttpScheme, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return new Uri(DefaultHttpFrontEndServiceUri);
            }
            else if (String.Compare(scheme, HttpsScheme, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return new Uri(DefaultHttpsFrontEndServiceUri);
            }
            else
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_NoDefaultUriForScheme, SR.NoDefaultUriForScheme, scheme);
                Debug.Fail("[ServiceJobMonitor] This line could not be reached.");
                return null;
            }
        }

        /// <summary>
        /// Apply session id to the uri
        /// </summary>
        /// <param name="baseUri">indicate the base uri</param>
        /// <param name="sessionId">indicate the session id</param>
        /// <param name="postfix">indicate the postfix</param>
        /// <param name="needFqdn">indicate if need FQDN</param>
        /// <returns>uri with session id</returns>
        private static Uri ApplySessionId(Uri baseUri, string sessionId, string postfix, bool needFqdn)
        {
            UriBuilder builder = new UriBuilder(baseUri);
            if (baseUri.IsLoopback)
            {
#if PaaS
                if (SoaHelper.IsOnAzure())
                {
                    builder.Host = AzureRoleHelper.GetLocalMachineAddress();
                }
                else
#endif
                {
                    if (needFqdn)
                    {
                        string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                        builder.Host = string.Format("{0}.{1}", Environment.MachineName, domainName);
                    }
                    else
                    {
                        builder.Host = Environment.MachineName;
                    }
                }
            }

            return new Uri(builder.Uri, String.Format(CultureInfo.InvariantCulture, "{0}/{1}", sessionId, postfix));
        }
    }
}
