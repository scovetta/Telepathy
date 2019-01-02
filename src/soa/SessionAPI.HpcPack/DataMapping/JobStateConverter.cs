namespace Microsoft.Hpc.Scheduler.Session.HpcPack.DataMapping
{
    public static class JobStateConverter
    {
        public static Microsoft.Hpc.Scheduler.Session.Data.JobState FromHpcJobState(Microsoft.Hpc.Scheduler.Properties.JobState jobstate)
        {
            return (Data.JobState)jobstate;
        }
    }
}
