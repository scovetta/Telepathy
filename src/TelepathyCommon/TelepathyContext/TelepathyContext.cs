// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace TelepathyCommon.HpcContext
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using TelepathyCommon.Registry;

    public class TelepathyContext : ITelepathyContext, IDisposable
    {
        private const string UriPathBaseString = @"/";

        private static ConcurrentDictionary<string, TelepathyContext> contexts = new ConcurrentDictionary<string, TelepathyContext>();

        public CancellationToken CancellationToken { get; private set; }

        public IFabricContext FabricContext { get; private set; }

        public IRegistry Registry => this.FabricContext.Registry;

        /// <summary>
        ///     Get the local fabric client context. This method should be called after the GetHpcContext(CancellationToken)
        ///     overload.
        /// </summary>
        /// <returns>the hpc context instance.</returns>
        public static ITelepathyContext Get()
        {
            return Get(string.Empty);
        }

        public static ITelepathyContext Get(string connectionString)
        {
            return Get(EndpointsConnectionString.ParseConnectionString(connectionString));
        }

        /// <summary>
        ///     Get the hpc context which maps to the connection string.
        /// </summary>
        /// <param name="connectionString">the connection string instance</param>
        /// <returns></returns>
        public static ITelepathyContext Get(EndpointsConnectionString connectionString)
        {
            return GetOrAdd(connectionString);
        }

        /// <summary>
        ///     Get the hpc context which maps to the connection string.
        /// </summary>
        /// <param name="connectionString">the connection string instance</param>
        /// <returns></returns>
        public static ITelepathyContext GetOrAdd(EndpointsConnectionString connectionString)
        {
            if (!connectionString.IsGateway)
            {
                return SoaContext.SoaContext.Default;
            }

            return new SoaContext.SoaContext(connectionString);
        }

        public static ITelepathyContext GetOrAdd(string connectionString)
        {
            return GetOrAdd(EndpointsConnectionString.ParseConnectionString(connectionString));
        }

        public static ITelepathyContext GetOrAdd()
        {
            return GetOrAdd(string.Empty);
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
    }
}