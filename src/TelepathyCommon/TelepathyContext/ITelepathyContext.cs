// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.TelepathyContext
{
    using System.Threading;

    using Microsoft.Telepathy.Common.Registry;

    public interface ITelepathyContext
    {
        CancellationToken CancellationToken { get; }

        IFabricContext FabricContext { get; }

        IRegistry Registry { get; }
    }
}