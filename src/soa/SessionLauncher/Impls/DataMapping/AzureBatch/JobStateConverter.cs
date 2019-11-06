// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.DataMapping.AzureBatch
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Common;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.AzureBatch;

    public static class AzureBatchJobStateConverter
    {
        public async static Task<Telepathy.Session.Data.JobState> FromAzureBatchJobAsync(CloudJob job)
        {
            //Handle job state Active independantly in case of batch tasks are not running
            using (var batchClient = AzureBatchConfiguration.GetBatchClient())
            {
                if (job.State == JobState.Active)
                {

                    ODATADetailLevel detail = new ODATADetailLevel(selectClause: "id,state");
                    List<CloudTask> allTasks = await batchClient.JobOperations.ListTasks(job.Id, detail).ToListAsync();
                    if (allTasks.Exists(task => task.State == TaskState.Running))
                    {
                        return Telepathy.Session.Data.JobState.Running;
                    }
                    return Telepathy.Session.Data.JobState.Queued;
                }
                else if (job.State == JobState.Terminating)
                {
                    ODATADetailLevel detail = new ODATADetailLevel(selectClause: "id,state");
                    List<CloudTask> allTasks = await batchClient.JobOperations.ListTasks(job.Id, detail).ToListAsync();
                    if (allTasks.Exists(task => task.State != TaskState.Completed))
                    {
                        return Telepathy.Session.Data.JobState.Canceling;
                    }
                    return Telepathy.Session.Data.JobState.Finishing;
                }
            }
            return JobStateMapping[(JobState)job.State];
        }

        private static Dictionary<JobState, Telepathy.Session.Data.JobState> JobStateMapping = new Dictionary<JobState, Telepathy.Session.Data.JobState>
        {
            { JobState.Completed, Telepathy.Session.Data.JobState.Finished},
            { JobState.Deleting, Telepathy.Session.Data.JobState.Canceling},
            //TODO: Disabled->Queued, Disabling->Running
            { JobState.Disabled, Telepathy.Session.Data.JobState.Canceled},
            { JobState.Disabling, Telepathy.Session.Data.JobState.Canceling},
            { JobState.Enabling, Telepathy.Session.Data.JobState.Configuring}
        };
    }
}
