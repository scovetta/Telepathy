// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.TelepathyContext
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Common.Registry;

    public interface IFabricContext
    {
        EndpointsConnectionString ConnectionString { get; }

        IRegistry Registry { get; }

        Task<string> ResolveSingletonServicePrimaryAsync(string serviceName, CancellationToken token);
    }
}