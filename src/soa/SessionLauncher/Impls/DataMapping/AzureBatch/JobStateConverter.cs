// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Common;

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch
{
    public static class AzureBatchJobStateConverter
    {
        public async static Task<Telepathy.Session.Data.JobState> FromAzureBatchJobAsync(CloudJob job)
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
                        return Telepathy.Session.Data.JobState.Running;
                    }
                    return Telepathy.Session.Data.JobState.Queued;
                }
            }
            return JobStateMapping[(JobState)job.State];
        }

        private static Dictionary<JobState, Telepathy.Session.Data.JobState> JobStateMapping = new Dictionary<JobState, Telepathy.Session.Data.JobState>
        {
            { JobState.Completed, Telepathy.Session.Data.JobState.Finished},
            { JobState.Deleting, Telepathy.Session.Data.JobState.Finishing},
            { JobState.Disabled, Telepathy.Session.Data.JobState.Failed},
            { JobState.Disabling, Telepathy.Session.Data.JobState.Canceling},
            { JobState.Enabling, Telepathy.Session.Data.JobState.Configuring},
            { JobState.Terminating, Telepathy.Session.Data.JobState.Finishing}
        };
    }
}
