using Microsoft.Azure.Batch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.Impls.AzureBatch
{
    using Data = Microsoft.Hpc.Scheduler.Session.Data;
    public static class TaskStateConverter
    {
        public static Data.TaskState FromAzureBatchTaskState(TaskState state)
        {
            return TaskStateMapping[state];
        }

        private static Dictionary<TaskState, Data.TaskState> TaskStateMapping = new Dictionary<TaskState, Data.TaskState>
        {
            { TaskState.Active, Data.TaskState.Submitted },
            { TaskState.Completed, Data.TaskState.Finished },
            { TaskState.Preparing, Data.TaskState.Dispatching },
            { TaskState.Running, Data.TaskState.Running }
        };
    }
}
