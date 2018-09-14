using System;
using System.IO;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    public abstract class AddInFilterBase
    {
        public abstract void LoadFilter(string filterFile, out byte[] loadData);

        public abstract void OnFilterLoad();
        public abstract void OnFilterUnload();

        public abstract int FilterActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount);
        public abstract void RevertActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount);

        public abstract int FilterSubmission(Stream jobXmlIn, out Stream jobXmlModified);
        public abstract void RevertSubmission(Stream jobXml);
    }
}
