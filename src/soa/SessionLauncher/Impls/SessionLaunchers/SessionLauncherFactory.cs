// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls
{
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch;
#if HPCPACK
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.HpcPack;
#endif
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.Local;

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
