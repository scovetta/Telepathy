using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    [System.AddIn.Pipeline.AddInAdapterAttribute()]
    public class ViewToContractAddInAdapter : System.AddIn.Pipeline.ContractBase, IAddInFilterContract
    {
        private AddInViewBase _view;

        public ViewToContractAddInAdapter(AddInViewBase view)
        {
            _view = view;
        }

        public void LoadFilter(string FilterFile, out byte[]loadData)
        {
            _view.LoadFilter(FilterFile, out loadData);
        }

        public void OnFilterLoad()
        {
            _view.OnFilterLoad();
        }

        public void OnFilterUnload()
        {
            _view.UnloadFilter();
        }

        public int FilterActivation(byte[] jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            int retVal = _view.FilterActivation(jobXml, schedulerPass, jobIndex, backfill, resourceCount);

            return retVal;
        }

        public void RevertActivation(byte[] jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            _view.RevertActivation(jobXml, schedulerPass, jobIndex, backfill, resourceCount);
        }

        public int FilterSubmission(ref byte[] jobXml)
        {
            int retVal = _view.FilterSubmission(ref jobXml);

            return retVal;
        }

        public void RevertSubmission(byte[] jobXml)
        {
            _view.RevertSubmission(jobXml);
        }
    }
}
