// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.DataMapping.AzureBatch
{
    using System.Collections.Generic;

    using Microsoft.Azure.Batch.Common;

    public static class TaskStateConverter
    {
        public static Telepathy.Session.Data.TaskState FromAzureBatchTaskState(TaskState state)
        {
            return TaskStateMapping[state];
        }

        private static Dictionary<TaskState, Telepathy.Session.Data.TaskState> TaskStateMapping = new Dictionary<TaskState, Telepathy.Session.Data.TaskState>
        {
            { TaskState.Active, Telepathy.Session.Data.TaskState.Submitted },
            { TaskState.Completed, Telepathy.Session.Data.TaskState.Finished },
            { TaskState.Preparing, Telepathy.Session.Data.TaskState.Dispatching },
            { TaskState.Running, Telepathy.Session.Data.TaskState.Running }
        };
    }
}
