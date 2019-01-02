namespace Microsoft.Hpc.Scheduler.Session.HpcPack.DataMapping
{
    public static class TaskStateConverter
    {
        public static Microsoft.Hpc.Scheduler.Session.Data.TaskState FromHpcTaskState(Microsoft.Hpc.Scheduler.Properties.TaskState taskState)
        {
            return (Data.TaskState)taskState;
        }
    }
}
