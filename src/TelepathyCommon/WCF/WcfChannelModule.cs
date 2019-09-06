namespace TelepathyCommon.Service
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Threading.Tasks;

    using TelepathyCommon.HpcContext;
#if !net40
    using TelepathyCommon.HpcContext.Extensions.RegistryExtension;

#endif

    public static class WcfChannelModule
    {
        /// <summary>
        ///     Use (Service local path + binding type name) as key
        /// </summary>
        private static readonly ConcurrentDictionary<string, ChannelFactory> ChannelFactoryCache = new ConcurrentDictionary<string, ChannelFactory>();

        private static readonly TimeSpan ServiceHostCloseTimeout = TimeSpan.FromSeconds(3);

        private static readonly TimeSpan ServiceHostOpenTimeout = TimeSpan.FromMinutes(1);

        public static bool CheckWcfProxyHealth(object wcfProxyObj)
        {
            var ret = false;
            var clientChannel = wcfProxyObj as IClientChannel;
            if (clientChannel?.State == CommunicationState.Created || clientChannel?.State == CommunicationState.Opening || clientChannel?.State == CommunicationState.Opened)
            {
                ret = true;
            }

            return ret;
        }

        /// <summary>
        ///     Recommend calling this when clearing service resources
        /// </summary>
        public static void ClearWcfChannelFactoryCache()
        {
            foreach (var channelFactoryPair in ChannelFactoryCache)
            {
                ChannelFactory _;
                DisposeWcfChannelFactory(channelFactoryPair.Value);
                ChannelFactoryCache.TryRemove(channelFactoryPair.Key, out _);
            }
        }

        public static ServiceHost CreateWcfChannel<T>(
            object singletonInstance,
            Binding binding,
            string address,
            string thumbPrint = null,
            ServiceAuthorizationManager authorizationManager = null,
            PrincipalPermissionMode permissionMode = PrincipalPermissionMode.UseWindowsGroups,
            ServiceThrottlingBehavior throttlingBehavior = null)
        {
            return CreateWcfChannel<T>(singletonInstance, binding, new Uri(address), thumbPrint, authorizationManager, permissionMode, throttlingBehavior);
        }

        public static ServiceHost CreateWcfChannel<T>(
            object singletonInstance,
            Binding binding,
            Uri address,
            string thumbPrint = null,
            ServiceAuthorizationManager authorizationManager = null,
            PrincipalPermissionMode permissionMode = PrincipalPermissionMode.UseWindowsGroups,
            ServiceThrottlingBehavior throttlingBehavior = null)
        {
            var host = new ServiceHost(singletonInstance);
            if (authorizationManager != null)
            {
                host.Authorization.ServiceAuthorizationManager = authorizationManager;
            }

            if (throttlingBehavior != null)
            {
                host.Description.Behaviors.Add(throttlingBehavior);
            }

            var myServiceBehavior = host.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
            myServiceBehavior.PrincipalPermissionMode = permissionMode;
            host.AddServiceEndpoint(typeof(T), binding, address);
            host.OpenTimeout = ServiceHostOpenTimeout;
            host.CloseTimeout = ServiceHostCloseTimeout;

            if (!string.IsNullOrEmpty(thumbPrint))
            {
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
                host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
                host.Credentials.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, thumbPrint);
            }

            return host;
        }

        public static BasicHttpBinding DefaultBasicHttpBindingFactory()
        {
            var binding = new BasicHttpBinding
                              {
                                  MaxBufferPoolSize = 512 * 1024 * 1024, MaxReceivedMessageSize = 512 * 1024 * 1024, MaxBufferSize = 512 * 1024 * 1024, SendTimeout = new TimeSpan(0, 10, 0)
                              };

            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            return binding;
        }

        public static NetTcpBinding DefaultNetTcpBindingFactory()
        {
            return new NetTcpBinding
                       {
                           MaxBufferPoolSize = 512 * 1024 * 1024, // 512MB
                           MaxReceivedMessageSize = 512 * 1024 * 1024, // 512MB
                           MaxBufferSize = 512 * 1024 * 1024, // 512MB
                           ReaderQuotas =
                               {
                                   MaxArrayLength = 64 * 1024 * 1024, // 64MB
                                   MaxStringContentLength = 64 * 1024 * 1024, // 64MB
                                   MaxBytesPerRead = 64 * 1024 * 1024 // 64MB
                               },
                           SendTimeout = new TimeSpan(0, 10, 0)
                       };
        }

        public static ServiceThrottlingBehavior DefaultServiceThrottlingBehavior()
        {
            return new ServiceThrottlingBehavior { MaxConcurrentCalls = 1000, MaxConcurrentInstances = 1000, MaxConcurrentSessions = 1000 };
        }

        public static void DisposeWcfProxy(object wcfProxyObj)
        {
            var wcfProxy = wcfProxyObj as ICommunicationObject;
            if (wcfProxy != null)
            {
                DisposeCommunicationObject(wcfProxy);
            }
        }

        public static string GetCertDnsIdentityName(string thumbPrint, StoreName storeName, StoreLocation storeLocation)
        {
            var store1 = new X509Store(storeName, storeLocation);
            try
            {
                store1.Open(OpenFlags.ReadOnly);
                var certs = store1.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, true);
                foreach (var cert in certs)
                {
                    return cert.GetNameInfo(X509NameType.DnsName, false);
                }
            }
            finally
            {
                store1.Close();
            }

            Trace.TraceError("[{0}] Can not find cert with thumbprint {1} in store {2}, {3}", nameof(WcfChannelModule), thumbPrint, storeName, storeLocation);
            throw new InvalidOperationException($"Can not find cert with thumbprint {thumbPrint} in store {storeName}, {storeLocation}");
        }

        public static string GetWcfServiceUriString(string host, int port, string serviceName)
        {
            return string.Format(WcfServiceConstants.NetTcpUriFormat, host, port, serviceName);
        }

        public static bool IsX509Identity(IIdentity identity)
        {
            return identity != null && string.Equals(identity.AuthenticationType, "X509", StringComparison.OrdinalIgnoreCase);
        }

        public static NetTcpBinding NetTcpBindingWithCertFactory()
        {
            var binding = DefaultNetTcpBindingFactory();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            return binding;
        }

        public static async Task<ServiceHost> SetupInternalWcfChannelAsync<T>(ITelepathyContext context, object singletonInstance, string serviceUri, ServiceAuthorizationManager manager = null)
        {
            var thumbPrint = await context.GetSSLThumbprint().ConfigureAwait(false);
            Trace.TraceInformation("[WcfHost] Begin to setup WCF channel on {0}, thumbprint is {1}", serviceUri, thumbPrint);
            var tcpBinding = NetTcpBindingWithCertFactory();
            var internalWcfHost = CreateWcfChannel<T>(singletonInstance, tcpBinding, serviceUri, thumbPrint, manager, PrincipalPermissionMode.None, DefaultServiceThrottlingBehavior());
            await Task.Factory.FromAsync(internalWcfHost.BeginOpen(null, null), internalWcfHost.EndOpen).ConfigureAwait(false);
            Trace.TraceInformation("[WcfHost] End to setup internal WCF channel");
            return internalWcfHost;
        }

        public static async Task<ServiceHost> SetupInternalWcfChannelAsync<T>(
            ITelepathyContext context,
            object singletonInstance,
            int servicePort,
            string serviceName,
            ServiceAuthorizationManager manager = null)
        {
            var tcpAddr = string.Format(WcfServiceConstants.NetTcpUriFormat, "localhost", servicePort, serviceName);
            return await SetupInternalWcfChannelAsync<T>(context, singletonInstance, tcpAddr, manager).ConfigureAwait(false);
        }

        public static async Task<ServiceHost> SetupManagementWcfChannelAsync<T>(object singletonInstance, string serviceName)
        {
            return await SetupWcfChannelAsync<T>(singletonInstance, WcfServiceConstants.ManagementWcfChannelPort, serviceName).ConfigureAwait(false);
        }

        public static async Task<ServiceHost> SetupWcfChannelAsync<T>(
            object singletonInstance,
            Binding binding,
            ServiceAuthorizationManager authorizationManager,
            PrincipalPermissionMode permissionMode,
            ServiceThrottlingBehavior throttlingBehavior,
            string address)
        {
            Trace.TraceInformation("[WcfHost] Start WCF endpoint on {0}", address);
            var host = CreateWcfChannel<T>(singletonInstance, binding, address, null, authorizationManager, permissionMode, throttlingBehavior);
            await Task.Factory.FromAsync(host.BeginOpen(null, null), host.EndOpen).ConfigureAwait(false);
            Trace.TraceInformation("[WcfHost] End to setup WCF channel");
            return host;
        }

        public static async Task<ServiceHost> SetupWcfChannelAsync<T>(object singletonInstance, int servicePort, string serviceName)
        {
            var tcpAddr = string.Format(WcfServiceConstants.NetTcpUriFormat, "localhost", servicePort, serviceName);
            return await SetupWcfChannelAsync<T>(singletonInstance, DefaultNetTcpBindingFactory(), null, PrincipalPermissionMode.UseWindowsGroups, null, tcpAddr).ConfigureAwait(false);
        }

        private static void DisposeCommunicationObject(ICommunicationObject communicationObj)
        {
            try
            {
                communicationObj.Close();
            }
            catch (TimeoutException)
            {
                communicationObj.Abort();
            }
            catch (CommunicationException)
            {
                communicationObj.Abort();
            }
            catch (Exception)
            {
                communicationObj.Abort();
                throw;
            }
        }

        private static void DisposeWcfChannelFactory(ChannelFactory channelFactory)
        {
            DisposeCommunicationObject(channelFactory);
        }
    }
}