using System;
using System.AddIn.Pipeline;
using System.AddIn.Contract;

using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    [AddInContract]
    public interface IAddInFilterContract : IContract
    {
        void LoadFilter(string FilterFile, out byte[] loadData);

        void OnFilterLoad();
        void OnFilterUnload();

        int FilterActivation(byte[] JobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount);
        void RevertActivation(byte[] JobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount);

        int FilterSubmission(ref byte[] JobXml);
        void RevertSubmission(byte[] JobXml);
    }
}
