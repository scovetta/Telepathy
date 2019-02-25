namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls
{
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch;
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.HpcPack;
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.Local;

    internal static class SessionLauncherFactory
    {
        public static HpcPackSessionLauncher CreateHpcPackSessionLauncher(string headNode, bool runningLocal, BrokerNodesManager brokerNodesManager) =>
            new HpcPackSessionLauncher(headNode, runningLocal, brokerNodesManager);

        public static AzureBatchSessionLauncher CreateAzureBatchSessionLauncher() => new AzureBatchSessionLauncher();

        public static LocalSessionLauncher CreateLocalSessionLauncher() => new LocalSessionLauncher();
    }
}
