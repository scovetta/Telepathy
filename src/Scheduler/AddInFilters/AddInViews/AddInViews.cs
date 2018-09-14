using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    [System.AddIn.Pipeline.AddInBaseAttribute()]
    public abstract class AddInViewBase
    {
        public abstract void LoadFilter(string filterFile, out byte[] loadData);

        public abstract void OnFilterLoad();
        public abstract void UnloadFilter();

        public abstract int FilterActivation(byte[] jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount);
        public abstract void RevertActivation(byte[] jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount);

        public abstract int FilterSubmission(ref byte[] jobXml);
        public abstract void RevertSubmission(byte[] jobXml);
    }
}
