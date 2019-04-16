namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    public class HpcContext : IHpcContext, IDisposable
    {
        private HpcContext(EndpointsConnectionString connectionString, CancellationToken token, HpcContextOwner hpcContextOwner)
        {
            this.connectionString = connectionString;
            this.CancellationToken = token;

            if (!connectionString.IsGateway)
            {
                switch (ServerContextType)
                {
                    case HpcServerContextType.ServiceFabric:
                        {
                            var clientContextType = Assembly.Load("HpcCommonServer").GetType("Microsoft.Hpc.FabricClientContext");
                            this.FabricContext = (IFabricContext)Activator.CreateInstance(clientContextType, connectionString);
                            break;
                        }

                    case HpcServerContextType.NtService:
                        {
                            var clientContextType = Assembly.Load("HpcCommonServer").GetType("Microsoft.Hpc.NtServiceContext");
                            this.FabricContext = (IFabricContext)Activator.CreateInstance(clientContextType);
                            break;
                        }

                    case HpcServerContextType.Undefined:
                    default:
                        {
                            throw new InvalidOperationException($"{nameof(HpcServerContextType)} is not in a valid state. Current: {ServerContextType.ToString()}");
                        }
                }
            }
            else
            {
                this.FabricContext = new HpcFabricRestContext(connectionString, hpcContextOwner);
            }
        }

        private EndpointsConnectionString connectionString;

        private const string UriPathBaseString = @"/";

        public static bool CheckIfHttps(string oldMultiFormatName, out string serverName, out int port)
        {
            bool overHttp = false;

            serverName = null;
            port = 0;

            Uri serverUri;

            if (!oldMultiFormatName.Contains("://"))
            {
                return false;
            }

            if (!Uri.TryCreate(oldMultiFormatName, UriKind.Absolute, out serverUri))
            {
                return false;
            }

            if (string.Equals(serverUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                overHttp = true;

                //v3 bug 17539: If the user provided string has an additional path 
                //after the host name, we want that to be part of the servername.

                if (string.Equals(serverUri.AbsolutePath, UriPathBaseString))
                {
                    //If the server uri contains no additonal path, the URI class 
                    //still shows "/" as the absolute path. We do not want to add the trailing /
                    //if the user does not provide a path.
                    serverName = serverUri.Host;
                }
                else
                {
                    // Change to disallow the uri with path.
                    throw new ArgumentException("The uri specified shouldn't contain a path.", nameof(oldMultiFormatName));
                }

                port = serverUri.Port;
            }
            else
            {
                throw new ArgumentException("The uri's scheme should be https.", nameof(oldMultiFormatName));
            }

            return overHttp;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var fabricClientContext = this.FabricContext as IDisposable;
                if (fabricClientContext != null)
                {
                    fabricClientContext.Dispose();
                }
            }
        }

        private static ConcurrentDictionary<string, HpcContext> contexts = new ConcurrentDictionary<string, HpcContext>();

        /// <summary>
        /// Get the local fabric client context. This method should be called after the GetHpcContext(CancellationToken) overload.
        /// </summary>
        /// <returns>the hpc context instance.</returns>
        public static IHpcContext Get()
        {
            return Get(string.Empty);
        }

        /// <summary>
        /// Get the context connecting to local SF cluster.
        /// </summary>
        /// <returns>the hpc context instance.</returns>
        public static IHpcContext GetOrAdd(CancellationToken token)
        {
            return GetOrAdd(string.Empty, token);
        }

        /// <summary>
        /// Get the hpc context which maps to the connection string.
        /// Note: 
        ///     The connection string should be a gateway string, which is of format host1,host2,host3,...
        ///     in the service code: all you need to call is the GetHpcContext(token).
        ///     In the client code:
        ///         1. You can call EndpointsConnectionString.LoadFromWindowsRegistry to load the connection string from client registry key.
        ///         2. If you have a string input by user, just pass in here.
        ///         3. For single node scenario, we shouldn't by pass the node resolve
        /// </summary>
        /// <param name="connectionString">the connection string</param>
        /// <returns></returns>
        public static IHpcContext Get(string connectionString)
        {
            return Get(EndpointsConnectionString.ParseConnectionString(connectionString));
        }

        /// <summary>
        /// Get the hpc context which maps to the connection string.
        /// </summary>
        /// <param name="connectionString">the connection string instance</param>
        /// <returns></returns>
        public static IHpcContext Get(EndpointsConnectionString connectionString)
        {
#if HPCPACK
            HpcContext context;
            if (!contexts.TryGetValue(connectionString.ConnectionString, out context))
            {
                throw new InvalidOperationException(
                    @"Two reasons you got this exception:
1, There is no cached context, you should call GetOrAdd first.
2, The cancellation token associated with the HpcContext is cancelled, so no further access to the context and please cleanup as soon as possible.");
            }

            return context;
#endif
            if (!connectionString.IsGateway)
            {
                return SoaContext.SoaContext.Default;
            }
            else
            {
                return new SoaContext.SoaContext(connectionString);
            }
        }

        /// <summary>
        /// Get the hpc context which maps to the connection string.
        /// Note: 
        ///     The connection string should be a gateway string, which is of format host1,host2,host3,...
        ///     in the service code: all you need to call is the Get() or GetOrAdd(token).
        ///     In the client code:
        ///         1. You can call EndpointsConnectionString.LoadFromEnvVarsOrWindowsRegistry to load the connection string.
        ///         2. If you have a string input by user, just pass in here.
        /// </summary>
        /// <param name="connectionString">the connection string</param>
        /// <param name="token">the cancellation token</param>
        /// <param name="isHpcService">Indicate if the HpcContext is inside an HpcService when instantiated outside of Service Fabric cluster.</param>
        /// <returns></returns>
        public static IHpcContext GetOrAdd(string connectionString, CancellationToken token, bool isHpcService = false)
        {
            return GetOrAdd(EndpointsConnectionString.ParseConnectionString(connectionString), token, isHpcService);
        }

        /// <summary>
        /// Get the hpc context which maps to the connection string.
        /// </summary>
        /// <param name="connectionString">the connection string instance</param>
        /// <param name="token">the cancellation token</param>
        /// <param name="isHpcService">Indicate if the HpcContext is inside an HpcService when instantiated outside of Service Fabric cluster.</param>
        /// <returns></returns>
        public static IHpcContext GetOrAdd(EndpointsConnectionString connectionString, CancellationToken token, bool isHpcService = false)
        {
#if HPCPACK
            HpcContextOwner hpcContextOwner = isHpcService ? HpcContextOwner.HpcServiceOutOfSFCluster : HpcContextOwner.Client;
            HpcContext context = contexts.GetOrAdd(connectionString.ConnectionString, s => new HpcContext(connectionString, token, hpcContextOwner));

            if (context.connectionString.IsGateway != connectionString.IsGateway)
            {
                throw new InvalidOperationException(string.Format("The context is already initialized with IsHttp = {0}, and cannot be changed to IsHttp = {1}", context.connectionString.IsGateway, connectionString.IsGateway));
            }

            context.IgnoreCertNameMismatchValidation();
            return context;
#endif
            if (!connectionString.IsGateway)
            {
                return SoaContext.SoaContext.Default;
            }
            else
            {
                return new SoaContext.SoaContext(connectionString);
            }
        }

        public static void ClearAll()
        {
            lock (contexts)
            {
                foreach (var context in contexts.Values)
                {
                    context.Dispose();
                }

                contexts.Clear();
            }
        }

        public static bool IsNtService => ServerContextType == HpcServerContextType.NtService;

        internal static HpcServerContextType ServerContextType { get; private set; } = HpcServerContextType.Undefined;

        public static void AsServiceFabricContext() => SetServerContextType(HpcServerContextType.ServiceFabric);

        public static void AsNtServiceContext() => SetServerContextType(HpcServerContextType.NtService);

        private static void SetServerContextType(HpcServerContextType expected)
        {
            Debug.Assert(expected != HpcServerContextType.Undefined);
            if (ServerContextType == HpcServerContextType.Undefined)
            {
                ServerContextType = expected;
            }
            else if (ServerContextType == expected)
            {
                return;
            }
            else
            {
                throw new InvalidOperationException($"ServerContextType can only be set once. Expected: {expected}, Current: {ServerContextType}.");
            }
        }

        public static bool NotRetryPreviousRetryFailure { get; private set; } = false;

        public static void FailOnFirstRetryFailure()
        {
            NotRetryPreviousRetryFailure = true;
        }

        public CancellationToken CancellationToken { get; private set; }

        public IFabricContext FabricContext { get; private set; }

        public IRegistry Registry { get { return this.FabricContext.Registry; } }
    }
}
