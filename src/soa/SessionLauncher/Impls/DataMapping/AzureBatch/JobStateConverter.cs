using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch
{
    public static class AzureBatchJobStateConverter
    {
        public async static Task<Data.JobState> FromAzureBatchJobAsync(CloudJob job)
        {
            //Handle job state Active independantly in case of batch tasks are not running
            if (job.State == JobState.Active)
            {
                using (var batchClient = AzureBatchConfiguration.GetBatchClient())
                {
                    ODATADetailLevel detail = new ODATADetailLevel(selectClause: "id,state");
                    List<CloudTask> allTasks = await batchClient.JobOperations.ListTasks(job.Id, detail).ToListAsync();
                    if (allTasks.Exists(task => task.State == TaskState.Running))
                    {
                        return Data.JobState.Running;
                    }
                    return Data.JobState.Queued;
                }
            }
            return JobStateMapping[(JobState)job.State];
        }

        private static Dictionary<JobState, Data.JobState> JobStateMapping = new Dictionary<JobState, Data.JobState>
        {
            { JobState.Completed, Data.JobState.Finished},
            { JobState.Deleting, Data.JobState.Finishing},
            { JobState.Disabled, Data.JobState.Failed},
            { JobState.Disabling, Data.JobState.Canceling},
            { JobState.Enabling, Data.JobState.Configuring},
            { JobState.Terminating, Data.JobState.Finishing}
        };
    }
}
