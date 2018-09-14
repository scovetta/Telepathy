using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public static class StoreCounterHelper
    {
        static Dictionary<JobState, PropertyId> _jobCounterPropMap;
        static Dictionary<TaskState, PropertyId> _taskCounterPropMap;
        static Dictionary<TaskState, PropertyId> _parametricTaskCounterPropMap;

        static StoreCounterHelper()
        {
            _jobCounterPropMap = new Dictionary<JobState, PropertyId>();
            _jobCounterPropMap.Add(JobState.Configuring, StorePropertyIds.ConfigJobCount);
            _jobCounterPropMap.Add(JobState.Submitted, StorePropertyIds.SubmittedJobCount);
            _jobCounterPropMap.Add(JobState.Validating, StorePropertyIds.ValidatingJobCount);            
            _jobCounterPropMap.Add(JobState.ExternalValidation, StorePropertyIds.ExternalValidationJobCount);
            _jobCounterPropMap.Add(JobState.Queued, StorePropertyIds.QueuedJobCount);
            _jobCounterPropMap.Add(JobState.Running, StorePropertyIds.RunningJobCount);
            _jobCounterPropMap.Add(JobState.Finishing, StorePropertyIds.FinishingJobCount);
            _jobCounterPropMap.Add(JobState.Finished, StorePropertyIds.FinishedJobCount);            
            _jobCounterPropMap.Add(JobState.Failed, StorePropertyIds.FailedJobCount);
            _jobCounterPropMap.Add(JobState.Canceled, StorePropertyIds.CanceledJobCount);
            _jobCounterPropMap.Add(JobState.Canceling, StorePropertyIds.CancelingJobCount);
            _jobCounterPropMap.Add(JobState.All, StorePropertyIds.TotalJobCount);
            
            _taskCounterPropMap = new Dictionary<TaskState, PropertyId>();
            _taskCounterPropMap.Add(TaskState.Configuring, StorePropertyIds.ConfiguringTaskCount);
            _taskCounterPropMap.Add(TaskState.Submitted, StorePropertyIds.SubmittedTaskCount);
            _taskCounterPropMap.Add(TaskState.Validating, StorePropertyIds.ValidatingTaskCount);
            _taskCounterPropMap.Add(TaskState.Queued, StorePropertyIds.QueuedTaskCount);
            _taskCounterPropMap.Add(TaskState.Dispatching, StorePropertyIds.DispatchingTaskCount);
            _taskCounterPropMap.Add(TaskState.Running, StorePropertyIds.RunningTaskCount);
            _taskCounterPropMap.Add(TaskState.Finishing, StorePropertyIds.FinishingTaskCount);
            _taskCounterPropMap.Add(TaskState.Finished, StorePropertyIds.FinishedTaskCount);
            _taskCounterPropMap.Add(TaskState.Failed, StorePropertyIds.FailedTaskCount);
            _taskCounterPropMap.Add(TaskState.Canceled, StorePropertyIds.CanceledTaskCount);
            _taskCounterPropMap.Add(TaskState.Canceling, StorePropertyIds.CancelingTaskCount);
            _taskCounterPropMap.Add(TaskState.All, StorePropertyIds.TotalTaskCount);

            _parametricTaskCounterPropMap = new Dictionary<TaskState, PropertyId>();
            _parametricTaskCounterPropMap.Add(TaskState.Configuring, ProtectedTaskPropertyIds.ParametricConfiguringCount);
            _parametricTaskCounterPropMap.Add(TaskState.Submitted, ProtectedTaskPropertyIds.ParametricSubmittedCount);
            _parametricTaskCounterPropMap.Add(TaskState.Validating, ProtectedTaskPropertyIds.ParametricValidatingCount);            
            _parametricTaskCounterPropMap.Add(TaskState.Queued, ProtectedTaskPropertyIds.ParametricQueuedCount);
            _parametricTaskCounterPropMap.Add(TaskState.Dispatching, ProtectedTaskPropertyIds.ParametricDispatchingCount);
            _parametricTaskCounterPropMap.Add(TaskState.Running, ProtectedTaskPropertyIds.ParametricRunningCount);
            _parametricTaskCounterPropMap.Add(TaskState.Finishing, ProtectedTaskPropertyIds.ParametricFinishingCount);
            _parametricTaskCounterPropMap.Add(TaskState.Finished, ProtectedTaskPropertyIds.ParametricFinishedCount);
            _parametricTaskCounterPropMap.Add(TaskState.Failed, ProtectedTaskPropertyIds.ParametricFailedCount);
            _parametricTaskCounterPropMap.Add(TaskState.Canceled, ProtectedTaskPropertyIds.ParametricCanceledCount);
            _parametricTaskCounterPropMap.Add(TaskState.Canceling, ProtectedTaskPropertyIds.ParametricCancelingCount);
            _parametricTaskCounterPropMap.Add(TaskState.All, ProtectedTaskPropertyIds.ParametricTotalCount);
        }

        public static PropertyId GetJobCounterPropertyId(JobState state)
        {
            PropertyId pid = null;
            _jobCounterPropMap.TryGetValue(state, out pid);
            return pid;
        }

        public static PropertyId GetTaskCounterPropertyId(TaskState state)
        {
            PropertyId pid = null;
            _taskCounterPropMap.TryGetValue(state, out pid);
            return pid;
        }

        public static PropertyId GetParametricTaskCounterPropertyId(TaskState state)
        {
            PropertyId pid = null;
            _parametricTaskCounterPropMap.TryGetValue(state, out pid);
            return pid;
        }

    }
}
