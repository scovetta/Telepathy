using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Threading;
using System.Threading.Tasks;
using TelepathyCommon.HpcContext;
using TelepathyCommon.HpcContext.Extensions.RegistryExtension;

namespace TelepathyCommon.Service
{
#if !net40
    using System.Security.Claims;
#endif

    public static class WcfChannelModule
    {
        private static readonly TimeSpan ServiceHostOpenTimeout = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan ServiceHostCloseTimeout = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Use (Service local path + binding type name) as key
        /// </summary>
        private static readonly ConcurrentDictionary<string, ChannelFactory> ChannelFactoryCache = new ConcurrentDictionary<string, ChannelFactory>();

        public static bool IsX509Identity(IIdentity identity)
        {
            return identity != null && String.Equals(identity.AuthenticationType, "X509", StringComparison.OrdinalIgnoreCase);
        }

#if !net40
        // HeadNode only
        private static string AadClientAppId => TelepathyContext.GetOrAdd(CancellationToken.None).GetAADClientAppIdAsync().GetAwaiter().GetResult();

        private const string AppIdType = "appid";

        public static bool IsHpcAadPrincipal(this IPrincipal principal, ITelepathyContext context = null, string aadInfoNode = null)
        {
            var cp = principal as ClaimsPrincipal;
            if (cp == null)
            {
                return false;
            }

            return cp.Identity.IsHpcAadIdentity(context, aadInfoNode);
        }

        public static bool IsHpcAadIdentity(this IIdentity identity, ITelepathyContext context = null, string aadInfoNode = null)
        {
            var ci = identity as ClaimsIdentity;
            if (ci == null)
            {
                return false;
            }

            if (context == null)
            {
                return ci.Claims.Any(c => c.Type == AppIdType && c.Value == AadClientAppId);
            }

            var aadClientAppId = context.GetAADClientAppIdAsync(aadInfoNode).GetAwaiter().GetResult();
            return ci.Claims.Any(c => c.Type == AppIdType && c.Value == aadClientAppId);
        }
#endif

        public static NetTcpBinding DefaultNetTcpBindingFactory()
        {
            return new NetTcpBinding
            {
                MaxBufferPoolSize = 512 * 1024 * 1024, //512MB
                MaxReceivedMessageSize = 512 * 1024 * 1024, //512MB
                MaxBufferSize = 512 * 1024 * 1024, //512MB
                ReaderQuotas =
                {
                    MaxArrayLength = 64 * 1024 * 1024, //64MB
                    MaxStringContentLength = 64 * 1024 * 1024, //64MB
                    MaxBytesPerRead = 64 * 1024 * 1024 //64MB
                },
                SendTimeout = new TimeSpan(0, 10, 0)
            };
        }

        public static NetTcpBinding NetTcpBindingWithCertFactory()
        {
            var binding = DefaultNetTcpBindingFactory();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            return binding;
        }

        public static BasicHttpBinding DefaultBasicHttpBindingFactory()
        {
            BasicHttpBinding binding = new BasicHttpBinding
            {
                MaxBufferPoolSize = 512 * 1024 * 1024,
                MaxReceivedMessageSize = 512 * 1024 * 1024,
                MaxBufferSize = 512 * 1024 * 1024,
                SendTimeout = new TimeSpan(0, 10, 0)
            };

            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            return binding;
        }

        public static ServiceThrottlingBehavior DefaultServiceThrottlingBehavior()
        {
            return new ServiceThrottlingBehavior
            {
                MaxConcurrentCalls = 1000,
                MaxConcurrentInstances = 1000,
                MaxConcurrentSessions = 1000
            };
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

            ServiceAuthorizationBehavior myServiceBehavior = host.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
            myServiceBehavior.PrincipalPermissionMode = permissionMode;
            host.AddServiceEndpoint(typeof(T), binding, address);
            host.OpenTimeout = ServiceHostOpenTimeout;
            host.CloseTimeout = ServiceHostCloseTimeout;

            if (!String.IsNullOrEmpty(thumbPrint))
            {
                host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
                host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
                host.Credentials.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, thumbPrint);
            }

            return host;
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

        public static async Task<ServiceHost> SetupInternalWcfChannelAsync<T>(ITelepathyContext context, object singletonInstance, string serviceUri, ServiceAuthorizationManager manager = null)
        {
            string thumbPrint = await context.GetSSLThumbprint().ConfigureAwait(false);
            Trace.TraceInformation("[WcfHost] Begin to setup WCF channel on {0}, thumbprint is {1}", serviceUri, thumbPrint);
            NetTcpBinding tcpBinding = NetTcpBindingWithCertFactory();
            var internalWcfHost = CreateWcfChannel<T>(
                                    singletonInstance,
                                    tcpBinding,
                                    serviceUri, thumbPrint, manager, PrincipalPermissionMode.None, DefaultServiceThrottlingBehavior());
            await Task.Factory.FromAsync(internalWcfHost.BeginOpen(null, null), internalWcfHost.EndOpen).ConfigureAwait(false);
            Trace.TraceInformation("[WcfHost] End to setup internal WCF channel");
            return internalWcfHost;
        }

        public static async Task<ServiceHost> SetupInternalWcfChannelAsync<T>(ITelepathyContext context, object singletonInstance, int servicePort, string serviceName, ServiceAuthorizationManager manager = null)
        {
            string tcpAddr = String.Format(WcfServiceConstants.NetTcpUriFormat, "localhost", servicePort, serviceName);
            return await SetupInternalWcfChannelAsync<T>(context, singletonInstance, tcpAddr, manager).ConfigureAwait(false);
        }

        public static async Task<ServiceHost> SetupWcfChannelAsync<T>(object singletonInstance, int servicePort, string serviceName)
        {
            string tcpAddr = String.Format(WcfServiceConstants.NetTcpUriFormat, "localhost", servicePort, serviceName);
            return await SetupWcfChannelAsync<T>(singletonInstance, DefaultNetTcpBindingFactory(), null, PrincipalPermissionMode.UseWindowsGroups, null, tcpAddr).ConfigureAwait(false);
        }

        public static async Task<ServiceHost> SetupManagementWcfChannelAsync<T>(object singletonInstance, string serviceName)
        {
            return await SetupWcfChannelAsync<T>(singletonInstance, WcfServiceConstants.ManagementWcfChannelPort, serviceName).ConfigureAwait(false);
        }

        public static async Task<T> CreateInternalWcfProxyAsync<T>(string endPointStr, ITelepathyContext context, EndpointBehaviorBase behavior = null) where T : class
        {
            return await CreateInternalWcfProxyAsync<T>(new Uri(endPointStr), context, behavior).ConfigureAwait(false);
        }

        public static async Task<T> CreateInternalWcfProxyAsync<T>(Uri uri, ITelepathyContext context, EndpointBehaviorBase behavior = null) where T : class
        {
            NetTcpBinding tcpBinding = NetTcpBindingWithCertFactory();
            string thumbPrint = await context.GetSSLThumbprint().ConfigureAwait(false);
            ChannelFactory<T> channelFactory = GetWcfChannelFactory<T>(tcpBinding, uri, behavior, thumbPrint);
            string dnsIdentityName = GetCertDnsIdentityName(thumbPrint, StoreName.My, StoreLocation.LocalMachine);
            Trace.TraceInformation("[WcfProxy] Begin connect to endpointStr {0}, dnsIdentityName {1}", uri, dnsIdentityName);
            EndpointAddress endpointAddress = new EndpointAddress(uri, EndpointIdentity.CreateDnsIdentity(dnsIdentityName));

            foreach (OperationDescription op in channelFactory.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior = op.Behaviors[typeof(DataContractSerializerOperationBehavior)] as DataContractSerializerOperationBehavior;

                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = 10 * 1024 * 1024;
                }
            }

            T proxy = channelFactory.CreateChannel(endpointAddress);
            ((ICommunicationObject)proxy).Faulted += (sender, args) =>
                {
                    Trace.TraceWarning($"[WcfProxy] Channel to {endpointAddress.Uri} faulted.");
                    ((ICommunicationObject)sender).Abort();
                };
            Trace.TraceInformation("[WcfProxy] End to create internal wcf proxy");
            return proxy;
        }

        public static T CreateWcfProxy<T>(string endPointStr) where T : class
        {
            return CreateWcfProxy<T>(new EndpointAddress(new Uri(endPointStr)));
        }

        public static T CreateWcfProxy<T>(Uri uri) where T : class
        {
            return CreateWcfProxy<T>(new EndpointAddress(uri));
        }

        public static T CreateWcfProxy<T>(EndpointAddress endpointAddress, EndpointBehaviorBase behavior = null) where T : class
        {
            Binding binding = DefaultNetTcpBindingFactory();
            Trace.TraceInformation("[WcfProxy] Begin to create wcf proxy to {0}", endpointAddress.Uri);
            ChannelFactory<T> channelFactory = GetWcfChannelFactory<T>(binding, endpointAddress.Uri, behavior);
            T proxy = channelFactory.CreateChannel(endpointAddress);
            ((ICommunicationObject)proxy).Faulted += (sender, args) =>
                {
                    Trace.TraceWarning($"[WcfProxy] Channel to {endpointAddress.Uri} faulted.");
                    ((ICommunicationObject)sender).Abort();
                };
            Trace.TraceInformation("[WcfProxy] End to create wcf proxy");
            return proxy;
        }

        public static void DisposeWcfProxy(object wcfProxyObj)
        {
            var wcfProxy = wcfProxyObj as ICommunicationObject;
            if (wcfProxy != null)
            {
                DisposeCommunicationObject(wcfProxy);
            }
        }

        public static bool CheckWcfProxyHealth(object wcfProxyObj)
        {
            bool ret = false;
            var clientChannel = wcfProxyObj as IClientChannel;
            if (clientChannel?.State == CommunicationState.Created || clientChannel?.State == CommunicationState.Opening || clientChannel?.State == CommunicationState.Opened)
            {
                ret = true;
            }
            return ret;
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

        /// <summary>
        /// Recommend calling this when clearing service resources
        /// </summary>
        public static void ClearWcfChannelFactoryCache()
        {
            foreach (KeyValuePair<string, ChannelFactory> channelFactoryPair in ChannelFactoryCache)
            {
                ChannelFactory _;
                DisposeWcfChannelFactory(channelFactoryPair.Value);
                ChannelFactoryCache.TryRemove(channelFactoryPair.Key, out _);
            }
        }

        private static ChannelFactory<T> GetWcfChannelFactory<T>(Binding binding, Uri serviceUri, EndpointBehaviorBase behavior, string thumbPrint = null)
        {
            ChannelFactory ret;
            string key = $"{binding.GetType().Name}-{serviceUri.LocalPath}-{behavior?.Name}";
            if (ChannelFactoryCache.TryGetValue(key, out ret))
            {
                if (ret.State == CommunicationState.Faulted || ret.State == CommunicationState.Closing || ret.State == CommunicationState.Closed)
                {
                    ChannelFactoryCache.TryRemove(key, out ret);
                    ret.Abort();
                }
                else
                {
                    return (ChannelFactory<T>)ret;
                }
            }

            return (ChannelFactory<T>)ChannelFactoryCache.GetOrAdd(key, k => {
                ret = new ChannelFactory<T>(binding);
                if (!string.IsNullOrEmpty(thumbPrint))
                {
                    ret.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
                    ret.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
                    ret.Credentials.ClientCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindByThumbprint, thumbPrint);
                }

                if (behavior != null)
                {
                    ret.Endpoint.Behaviors.Add(behavior);
                }

                return ret;
            });
        }

        public static string GetWcfServiceUriString(string host, int port, string serviceName)
        {
            return string.Format(WcfServiceConstants.NetTcpUriFormat, host, port, serviceName);
        }

        public static string GetCertDnsIdentityName(string thumbPrint, StoreName storeName, StoreLocation storeLocation)
        {
            X509Store store1 = new X509Store(storeName, storeLocation);
            try
            {
                store1.Open(OpenFlags.ReadOnly);
                var certs = store1.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint,
                    true);
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
    }
}