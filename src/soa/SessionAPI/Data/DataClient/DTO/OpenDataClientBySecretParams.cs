namespace Microsoft.Hpc.Scheduler.Session.Data.DTO
{
    public class OpenDataClientBySecretParams
    {
        public OpenDataClientBySecretParams()
        {
        }

        public OpenDataClientBySecretParams(string dataClientId, int jobId, string jobSecret)
        {
            this.DataClientId = dataClientId;
            this.JobId = jobId;
            this.JobSecret = jobSecret;
        }


        public string DataClientId { get; set; }

        public int JobId { get; set; }

        public string JobSecret { get; set; }
    }
}
