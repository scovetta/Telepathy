// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace TelepathyCommon.HpcContext.SoaContext
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using TelepathyCommon.Registry;

    // TODO: remove me
    public class SoaFabricContext : IFabricContext
    {
        private const string LocalHost = "localhost";

        public static readonly IFabricContext Default = new SoaFabricContext();

        public SoaFabricContext(string connectionString)
            : this(EndpointsConnectionString.ParseConnectionString(connectionString))
        {
        }

        public SoaFabricContext()
        {
            this.ConnectionString = EndpointsConnectionString.ParseConnectionString(LocalHost);
        }

        public SoaFabricContext(EndpointsConnectionString connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public EndpointsConnectionString ConnectionString { get; }

        public IRegistry Registry => throw new NotImplementedException();

        public async Task<string> ResolveSingletonServicePrimaryAsync(string serviceName, CancellationToken token)
        {
            return this.ConnectionString.ConnectionString;
        }
    }
}