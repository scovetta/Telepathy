using System.IO;

using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    [System.AddIn.Pipeline.HostAdapterAttribute()]
    public class AddInFilterContractToViewHostAdapter : AddInFilterBase
    {
        private IAddInFilterContract _contract;
        private System.AddIn.Pipeline.ContractHandle _handle;

        public AddInFilterContractToViewHostAdapter(IAddInFilterContract contract)
        {
            _contract = contract;
            _handle = new System.AddIn.Pipeline.ContractHandle(contract);
        }

        public override void LoadFilter(string FilterFile, out byte[] loadData)
        {
            _contract.LoadFilter(FilterFile, out loadData);
        }

        public override void OnFilterLoad()
        {
            _contract.OnFilterLoad();
        }

        public override void OnFilterUnload()
        {
            _contract.OnFilterUnload();
        }

        private byte[] StreamToBytes(Stream inStream)
        {
            inStream.Flush();
            inStream.Position = 0;  // start from beginning of stream

            byte[] streamBytes = new byte[inStream.Length];

            // convert to byte array
            inStream.Read(streamBytes, 0, (int)inStream.Length);

            return streamBytes;
        }

        public override int FilterActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            byte[] jobXmlBytes = StreamToBytes(jobXml);

            int retVal = _contract.FilterActivation(jobXmlBytes, schedulerPass, jobIndex, backfill, resourceCount);

            return retVal;
        }

        public override void RevertActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            byte[] jobXmlBytes = StreamToBytes(jobXml);

            _contract.RevertActivation(jobXmlBytes, schedulerPass, jobIndex, backfill, resourceCount);
        }

        public override int FilterSubmission(Stream jobXmlIn, out Stream jobXmlModified)
        {
            byte[] jobXmlBytes = StreamToBytes(jobXmlIn);

            jobXmlModified = null;

            int retVal = _contract.FilterSubmission(ref jobXmlBytes);  // call the addin to filter the submission

                // if the filter signals it changed the xml we must push changes to output stream
            if ((int)SubmissionFilterResponse.SuccessJobChanged == retVal)
            {
                jobXmlModified = new MemoryStream();

                jobXmlModified.Write(jobXmlBytes, 0, (int)jobXmlBytes.Length);

                jobXmlModified.Position = 0;
            }

            return retVal;
        }

        public override void RevertSubmission(Stream jobXml)
        {
            byte[] jobXmlBytes = StreamToBytes(jobXml);

            _contract.RevertSubmission(jobXmlBytes);
        }
    }
}
