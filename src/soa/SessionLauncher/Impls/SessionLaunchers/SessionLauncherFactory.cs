// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers
{
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.AzureBatch;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.Local;
#if HPCPACK
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.HpcPack;
#endif

    internal static class SessionLauncherFactory
    {
#if HPCPACK
        public static HpcPackSessionLauncher CreateHpcPackSessionLauncher(string headNode, bool runningLocal, BrokerNodesManager brokerNodesManager) =>
            new HpcPackSessionLauncher(headNode, runningLocal, brokerNodesManager);
#endif

        public static AzureBatchSessionLauncher CreateAzureBatchSessionLauncher() => new AzureBatchSessionLauncher();

        public static LocalSessionLauncher CreateLocalSessionLauncher() => new LocalSessionLauncher();
    }
}
