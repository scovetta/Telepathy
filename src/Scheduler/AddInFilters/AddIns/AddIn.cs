using System;
using System.Collections.Generic;
using System.Text;
using System.AddIn;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    [AddIn("Microsoft HPC Scheduler AddIn Filter", Version = "1.0.0.0")]
    public class HpcSchedulerAddInFilter : AddInViewBase
    {
        private static readonly FilterInternal _internalFilter = new FilterInternal();

        public override void LoadFilter(string filterFile, out byte[] loadData)
        {
            _internalFilter.LoadFilter(filterFile, out loadData);
        }

        public override void OnFilterLoad()
        {
            _internalFilter.OnFilterLoad();
        }

        public override void UnloadFilter()
        {
            _internalFilter.OnFilterUnload();
        }

        public override int FilterActivation(byte[] jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            int retVal = _internalFilter.FilterActivation(jobXml, schedulerPass, jobIndex, backfill, resourceCount);

            return retVal;
        }

        public override void RevertActivation(byte[] jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            _internalFilter.RevertActivation(jobXml, schedulerPass, jobIndex, backfill, resourceCount);
        }

        public override int FilterSubmission(ref byte[] jobXml)
        {
            int retVal = _internalFilter.FilterSubmission(ref jobXml);

            return retVal;
        }

        public override void RevertSubmission(byte[] jobXml)
        {
            _internalFilter.RevertSubmission(jobXml);
        }
    }
}
