using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using TelepathyCommon.Registry;

namespace TelepathyCommon.HpcContext
{
    public class TelepathyContext : ITelepathyContext, IDisposable
    {
        private TelepathyContext(EndpointsConnectionString connectionString, CancellationToken token)
        {
            this.connectionString = connectionString;
            this.CancellationToken = token;
        }

        private EndpointsConnectionString connectionString;

        private const string UriPathBaseString = @"/";

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

        private static ConcurrentDictionary<string, TelepathyContext> contexts = new ConcurrentDictionary<string, TelepathyContext>();

        /// <summary>
        /// Get the local fabric client context. This method should be called after the GetHpcContext(CancellationToken) overload.
        /// </summary>
        /// <returns>the hpc context instance.</returns>
        public static ITelepathyContext Get()
        {
            return Get(string.Empty);
        }

        /// <summary>
        /// Get the context connecting to local SF cluster.
        /// </summary>
        /// <returns>the hpc context instance.</returns>
        public static ITelepathyContext GetOrAdd(CancellationToken token)
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
        public static ITelepathyContext Get(string connectionString)
        {
            return Get(EndpointsConnectionString.ParseConnectionString(connectionString));
        }

        /// <summary>
        /// Get the hpc context which maps to the connection string.
        /// </summary>
        /// <param name="connectionString">the connection string instance</param>
        /// <returns></returns>
        public static ITelepathyContext Get(EndpointsConnectionString connectionString)
        {
#if HPCPACK
            TelepathyContext context;
            if (!contexts.TryGetValue(connectionString.ConnectionString, out context))
            {
                throw new InvalidOperationException(
                    @"Two reasons you got this exception:
1, There is no cached context, you should call GetOrAdd first.
2, The cancellation token associated with the TelepathyContext is cancelled, so no further access to the context and please cleanup as soon as possible.");
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
        /// <param name="isHpcService">Indicate if the TelepathyContext is inside an HpcService when instantiated outside of Service Fabric cluster.</param>
        /// <returns></returns>
        public static ITelepathyContext GetOrAdd(string connectionString, CancellationToken token, bool isHpcService = false)
        {
            return GetOrAdd(EndpointsConnectionString.ParseConnectionString(connectionString), token, isHpcService);
        }

        /// <summary>
        /// Get the hpc context which maps to the connection string.
        /// </summary>
        /// <param name="connectionString">the connection string instance</param>
        /// <param name="token">the cancellation token</param>
        /// <param name="isHpcService">Indicate if the TelepathyContext is inside an HpcService when instantiated outside of Service Fabric cluster.</param>
        /// <returns></returns>
        public static ITelepathyContext GetOrAdd(EndpointsConnectionString connectionString, CancellationToken token, bool isHpcService = false)
        {
#if HPCPACK
            HpcContextOwner hpcContextOwner = isHpcService ? HpcContextOwner.HpcServiceOutOfSFCluster : HpcContextOwner.Client;
            TelepathyContext context = contexts.GetOrAdd(connectionString.ConnectionString, s => new TelepathyContext(connectionString, token, hpcContextOwner));

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
