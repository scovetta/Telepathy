using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Hpc.Scheduler.Properties;
using System.Threading;

namespace Microsoft.Hpc.Scheduler.Store
{
    class AsyncResultWrapper : IAsyncResult
    {
        public delegate void EndInvokeDelegate(IAsyncResult result);
        EndInvokeDelegate _dele;
        IAsyncResult _result;

        internal AsyncResultWrapper(IAsyncResult result, EndInvokeDelegate dele)
        {
            _dele = dele;
            _result = result;
        }

        public void EndInvoke()
        {
            _dele(_result);
        }

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return _result.AsyncState; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return _result.AsyncWaitHandle; }
        }

        public bool CompletedSynchronously
        {
            get { return _result.CompletedSynchronously; }
        }

        public bool IsCompleted
        {
            get { return _result.IsCompleted; }
        }

        #endregion
    }

    public class SchedulerStoreHelpers
    {        
        internal delegate void JobModifyInternalDelegate(IClusterJob job, StoreProperty[] changeProps, StoreProperty[] backupProps);

        // The delegate to asynchronously execute JobModifyInternal
        static JobModifyInternalDelegate queuedJobModifyDelegate = new JobModifyInternalDelegate(JobModifyInternal);
        static JobModifyInternalDelegate directlySetPropsDelegate = new JobModifyInternalDelegate(DirectlySetProps);
        static JobModifyInternalDelegate configureAndSetPropsDelegate = new JobModifyInternalDelegate(ConfigureAndSetProps);


        // Modify a queued job. This function will be executed in asynchronous way
        internal static void JobModifyInternal(IClusterJob job, StoreProperty[] changedProps, StoreProperty[] backupProps)
        {
            if (changedProps == null ||
                changedProps.Length == 0)
            {
                // Succeed it directly without doing anything
                return;
            }

            try
            {
                // Configure -> Change -> Submit

                job.Configure();
                job.SetProps(changedProps);
                job.Submit();
            }
            catch (Exception e)
            {
                // Start to revert

                try
                {
                    job.Configure();

                    if (backupProps != null && backupProps.Length > 0)
                    {
                        job.SetProps(backupProps);
                    } 
                    
                    job.Submit();
                }
                catch (Exception)
                {
                    // Failed to revert

                    throw new SchedulerException(
                        ErrorCode.Operation_JobRevertModifyFailed, e.Message);
                }

                throw new SchedulerException(ErrorCode.Operation_JobModifyFailed, e.Message);
            }
        }

        // Set props. This function will be executed in asynchronous way
        private static void DirectlySetProps(IClusterJob job, StoreProperty[] changedProps, StoreProperty[] noUse)
        {
            if (changedProps == null ||
                changedProps.Length == 0)
            {
                // Succeed it directly without doing anything
                return;
            }

            try
            {
                job.SetProps(changedProps);
            }
            catch (SchedulerException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SchedulerException(
                    ErrorCode.Operation_JobModifyFailed, e.Message);
            }
        }

        // Configure and set props. This function will be executed in asynchronous way
        private static void ConfigureAndSetProps(IClusterJob job, StoreProperty[] changedProps, StoreProperty[] noUse)
        {
            try
            {
                job.Configure();
                DirectlySetProps(job, changedProps, noUse);
            }
            catch (SchedulerException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SchedulerException(
                    ErrorCode.Operation_JobModifyFailed, e.Message);
            }
        }

        const JobState ConfigurableStateMask = JobState.Configuring | JobState.Queued | JobState.Failed | JobState.Canceled;

        public static bool IsConfigurableJobState(JobState state)
        {
            return ((state & ConfigurableStateMask) == state);
        }

        /// <summary>
        /// Begin operation of asynchronously modifying a job. The job has to be in Queued state.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="param"></param>
        /// <param name="store"></param>
        /// <param name="jobId"></param>
        /// <param name="changedProps"></param>
        /// <returns></returns>
        public static IAsyncResult BeginJobModify(AsyncCallback callback, object param, ISchedulerStore store, int jobId, StoreProperty[] changedProps)
        {
            if (changedProps == null)
            {
                changedProps = new StoreProperty[] { };
            }

            IClusterJob job = store.OpenJob(jobId);
            JobState state = (JobState)job.GetProps(JobPropertyIds.State)[0].Value;
            IAsyncResult result = null;

            StoreProperty[] newChangedProps = UpdateCustomGpuPropertyIfNeeded(store, changedProps);

            // First, check whether all properties can be modified in any state.
            bool allPropsModifiableInAnyState = true;
            foreach (StoreProperty prop in changedProps)
            {
                if (!PropsCanBeSetAnyTime.Contains(prop.Id))
                {
                    allPropsModifiableInAnyState = false;
                    break;
                }
            }
            if (allPropsModifiableInAnyState)
            {
                return new AsyncResultWrapper(
                        directlySetPropsDelegate.BeginInvoke(job, newChangedProps, null, callback, param),
                        directlySetPropsDelegate.EndInvoke);
            }

            switch (state)
            {
                case JobState.Queued:
                    bool needReconfig = false;
                    foreach (StoreProperty prop in newChangedProps)
                    {
                        if (PropsCanBeSetWhenQueuedAndRequireReconfigure.Contains(prop.Id))
                        {
                            needReconfig = true;
                        }
                    }
                    if (needReconfig)
                    {
                        result = new AsyncResultWrapper(
                            StartQueuedBranch(job, newChangedProps, callback, param),
                            queuedJobModifyDelegate.EndInvoke);
                    }
                    else
                    {
                        result = new AsyncResultWrapper(
                            directlySetPropsDelegate.BeginInvoke(job, newChangedProps, null, callback, param),
                            directlySetPropsDelegate.EndInvoke);
                    }
                    return result;

                case JobState.Running:
                case JobState.Configuring:
                    result = new AsyncResultWrapper(
                        directlySetPropsDelegate.BeginInvoke(job, newChangedProps, null, callback, param),
                        directlySetPropsDelegate.EndInvoke);
                    return result;

                case JobState.Canceled:
                case JobState.Failed:
                case JobState.Finished:
                    result = new AsyncResultWrapper(
                        configureAndSetPropsDelegate.BeginInvoke(job, newChangedProps, null, callback, param),
                        configureAndSetPropsDelegate.EndInvoke);
                    return result;

                default:
                    // Users are not allowed to change
                    // any properties while the job is not in
                    // any of the above states.
                    throw new SchedulerException(
                        ErrorCode.Operation_TryToModifyInvalidStateJob,
                        state.ToString());
            }
        }

        public static StoreProperty[] UpdateCustomGpuPropertyIfNeeded(ISchedulerStore store, StoreProperty[] changedProps)
        {
            StoreProperty customProp = null;
            foreach (StoreProperty prop in changedProps)
            {
                if (prop.Id == JobPropertyIds.UnitType)
                {
                    if ((JobUnitType)prop.Value == JobUnitType.Gpu)
                    {
                        StorePropertyDescriptor[] descs = (StorePropertyDescriptor[])store.GetPropertyDescriptors(new string[] { JobPropertyIds.JobGpuCustomPropertyName }, ObjectType.Job);

                        if (descs[0].PropId == StorePropertyIds.Error)
                        {
                            PropertyId pid = store.CreatePropertyId(ObjectType.Job, StorePropertyType.Boolean, JobPropertyIds.JobGpuCustomPropertyName, "");
                            customProp = new StoreProperty(pid, true);
                        }
                        else
                        {
                            customProp = new StoreProperty(descs[0].PropId, true);
                        }

                        prop.Value = JobUnitType.Socket;
                    }

                    break;
                }
            }

            if (customProp == null) return changedProps;
            else
            {
                List<StoreProperty> changedPropsList = new List<StoreProperty>(changedProps);
                changedPropsList.Add(customProp);
                return changedPropsList.ToArray();
            }  
        }

        private static IAsyncResult StartQueuedBranch(IClusterJob job, StoreProperty[] changedProps, AsyncCallback callback, object param)
        {
            // Check whether the job is in Queued state

            List<PropertyId> propIdsToBackup = new List<PropertyId>();
            foreach (PropertyId pid in PropsCanBeSetWhenQueuedAndRequireReconfigure)
            {
                if (!PropIdsToSkipBackup.Contains(pid))
                {
                    propIdsToBackup.Add(pid);
                }
            }
            PropertyRow row = job.GetProps(propIdsToBackup.ToArray());

            // Backup the original job attributes

            List<StoreProperty> backupProps = new List<StoreProperty>();
            foreach (StoreProperty prop in changedProps)
            {
                if (row[prop.Id] != null && row[prop.Id].Value != null)
                {
                    backupProps.Add(row[prop.Id]);
                }
                else
                {
                    backupProps.Add(new StoreProperty(prop.Id, null));
                }
            }

            // Go!

            return queuedJobModifyDelegate.BeginInvoke(
                job,
                changedProps,
                backupProps.ToArray(),
                callback,
                param);
        }

        /// <summary>
        /// End operation of asynchronously modifying a job.
        /// </summary>
        /// <param name="result"></param>
        public static void EndJobModify(IAsyncResult result)
        {
            if (!(result is AsyncResultWrapper))
            {
                throw new InvalidOperationException("The IAsyncResult object provided does not match this delegate.");
            }
            AsyncResultWrapper wrapper = result as AsyncResultWrapper;
            wrapper.EndInvoke();
        }

        /// <summary>
        /// Modify a job in a synchronous way. The job has to be in Queued state.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="jobId"></param>
        /// <param name="changedProps"></param>
        public static void JobModify(ISchedulerStore store, int jobId, params StoreProperty[] changedProps)
        {
            if(changedProps == null)
            {
                changedProps = new StoreProperty[]{};
            }
            EndJobModify(BeginJobModify(null, null, store, jobId, changedProps));
        }

        // Only the following items can be modified.  Most of these need to be backed
        // up before we attempt to reconfigure and re-queue the job.
        static List<PropertyId> PropsCanBeSetWhenQueuedAndRequireReconfigure =
            new List<PropertyId>(new PropertyId[]
                { 
                    JobPropertyIds.JobTemplate,
                    JobPropertyIds.Name,
                    JobPropertyIds.Project,                    
                    JobPropertyIds.RuntimeSeconds,
                    JobPropertyIds.RunUntilCanceled,
                    JobPropertyIds.FailOnTaskFailure,
                    JobPropertyIds.UnitType,
                    JobPropertyIds.MaxCores,
                    JobPropertyIds.MinCores,
                    JobPropertyIds.MaxSockets,
                    JobPropertyIds.MinSockets,
                    JobPropertyIds.MaxNodes,
                    JobPropertyIds.MinNodes,
                    JobPropertyIds.AutoCalculateMin,
                    JobPropertyIds.AutoCalculateMax,
                    JobPropertyIds.IsExclusive,
                    JobPropertyIds.SoftwareLicense,
                    JobPropertyIds.NodeGroups,
                    JobPropertyIds.MinMemory,
                    JobPropertyIds.MinCoresPerNode,
                    JobPropertyIds.RequestedNodes, 
                    JobPropertyIds.OrderBy,
                    JobPropertyIds.UserName,
                    JobPropertyIds.Password,
                    JobPropertyIds.EncryptedPassword,     
                    JobPropertyIds.NodeGroupOp,
                    JobPropertyIds.EstimatedProcessMemory,
                    JobPropertyIds.TaskExecutionFailureRetryLimit,
                }
            );

        //not every modifiable propertes could be backed up, 
        //e.g, passwords (even encrypted ones) cannot be read from the server,
        //and should not be backed up.
        static List<PropertyId> PropIdsToSkipBackup =
            new List<PropertyId>(new PropertyId[] 
                { 
                    JobPropertyIds.Password,
                    JobPropertyIds.EncryptedPassword,
                }
            );


        static List<PropertyId> PropsCanBeSetWhenRunning_WithProfileChecking = 
            new List<PropertyId>(
            new PropertyId[]
            {
                JobPropertyIds.Project,
                JobPropertyIds.RuntimeSeconds,
                JobPropertyIds.Name,                
                JobPropertyIds.RunUntilCanceled,                
                JobPropertyIds.Priority,    // We need this property because client code may be setting it instead of ExoandedPriority
                JobPropertyIds.ExpandedPriority,
            }
        );

        static List<PropertyId> PropsCanBeSetWhenRunning_WithoutProfileChecking = 
            new List<PropertyId>(
            new PropertyId[]
            {
            JobPropertyIds.ErrorCode,
            JobPropertyIds.ErrorParams,
            JobPropertyIds.RequestCancel,
            JobPropertyIds.NotifyOnCompletion,
            JobPropertyIds.TargetResourceCount,
            JobPropertyIds.EmailAddress,
            JobPropertyIds.HoldUntil,
            }
        );

        static List<PropertyId> PropsCanBeSetWhenQueued_WithProfileChecking =
            new List<PropertyId>(
            new PropertyId[]
            {
                JobPropertyIds.Priority,    // We need this property because client code may be setting it instead of ExoandedPriority
                JobPropertyIds.ExpandedPriority,
            }
        );

        static List<PropertyId> PropsCanBeSetWhenQueued_WithoutProfileChecking = 
            new List<PropertyId>(
            new PropertyId[]
            {
            JobPropertyIds.ErrorCode,
            JobPropertyIds.ErrorParams,
            JobPropertyIds.RequestCancel,
            JobPropertyIds.UserName,
            JobPropertyIds.Password,     
            JobPropertyIds.PendingReason,
            JobPropertyIds.HoldUntil,
            JobPropertyIds.NotifyOnStart,
            JobPropertyIds.NotifyOnCompletion,
            JobPropertyIds.TargetResourceCount,
            JobPropertyIds.EmailAddress,
            }
        );

        static List<PropertyId> PropsCanBeSetAnyTime = 
            new List<PropertyId>(
            new PropertyId[]
            {
            JobPropertyIds.Progress,
            JobPropertyIds.ProgressMessage,

            JobPropertyIds.EndpointReference,

            // For V3 SOA model
            JobPropertyIds.NumberOfCalls,
            JobPropertyIds.NumberOfOutstandingCalls,
            JobPropertyIds.CallDuration,
            JobPropertyIds.CallsPerSecond,
            }
        );


        /// <summary>
        /// Checks whether a job property can be modified in a given job state.  Also, will inform the caller
        /// whether setting the property will require further validation against the job template.
        /// </summary>
        /// <param name="state">The job state</param>
        /// <param name="pid">The job property ID</param>
        /// <param name="needsProfileChecking">Output: Will be true if setting the property should trigger
        /// a check against the job template</param>
        /// <returns>True if it is legal to modifiy the specified property in the given state</returns>
        public static bool CanPropBeSet(JobState state, PropertyId pid, out bool needsProfileChecking)
        {
            needsProfileChecking = false;
            if (PropsCanBeSetAnyTime.Contains(pid))
            {
                return true;
            }

            switch (state)
            {
                case JobState.Configuring:
                    return true;
    
                case JobState.Queued:
                    if (PropsCanBeSetWhenQueued_WithoutProfileChecking.Contains(pid))
                    {
                        return true;
                    }
                    if (PropsCanBeSetWhenQueued_WithProfileChecking.Contains(pid))
                    {
                        needsProfileChecking = true;
                        return true;
                    }
                    return false;

                case JobState.Running:            
                    if (PropsCanBeSetWhenRunning_WithoutProfileChecking.Contains(pid))
                    {
                        return true;
                    }
                    if (PropsCanBeSetWhenRunning_WithProfileChecking.Contains(pid))
                    {
                        needsProfileChecking = true;
                        return true;
                    }
                    return false;
            
                default:
                    return false;
            }
        }

        /// <summary>
        /// insert or update node prepare/release task
        /// </summary>
        /// <param name="clusterJob">cluster job</param>
        /// <param name="jobProfile">job template</param>
        public static void AddOrUpdateSpecialTask(IClusterJob clusterJob, string prepareTaskCmd, string releaseTaskCmd)
        {
            if (!string.IsNullOrEmpty(prepareTaskCmd) || !string.IsNullOrEmpty(releaseTaskCmd))
            {
                IClusterTask nodePrepTask = null;
                IClusterTask nodeReleaseTask = null;
                using (IRowEnumerator taskRowSet = clusterJob.OpenTaskRowEnumerator(TaskRowSetOptions.NoParametricExpansion))
                {
                    taskRowSet.SetFilter(new FilterProperty(FilterOperator.NotEqual, TaskPropertyIds.Type, TaskType.Basic),
                        new FilterProperty(FilterOperator.NotEqual, TaskPropertyIds.Type, TaskType.ParametricSweep),
                        new FilterProperty(FilterOperator.NotEqual, TaskPropertyIds.Type, TaskType.Service));
                    taskRowSet.SetColumns(TaskPropertyIds.Type, TaskPropertyIds.Id);

                    foreach (PropertyRow taskRow in taskRowSet)
                    {
                        if ((TaskType)taskRow.Props[0].Value == TaskType.NodePrep)
                        {
                            nodePrepTask = clusterJob.OpenTask((int)taskRow.Props[1].Value);
                        }
                        else if ((TaskType)taskRow.Props[0].Value == TaskType.NodeRelease)
                        {
                            nodeReleaseTask = clusterJob.OpenTask((int)taskRow.Props[1].Value);
                        }
                    }
                }

                SchedulerStoreHelpers.AddOrUpdateSpecialTask(clusterJob, nodePrepTask, prepareTaskCmd, TaskType.NodePrep);
                SchedulerStoreHelpers.AddOrUpdateSpecialTask(clusterJob, nodeReleaseTask, releaseTaskCmd, TaskType.NodeRelease);
            }
        }

        /// <summary>
        /// insert or update node prepare/release task
        /// </summary>
        /// <param name="clusterJob">cluster job</param>
        /// <param name="clusterTask">cluster task, if null, means no existing task for this type</param>
        /// <param name="profileItem">job template item</param>
        /// <param name="taskType">task type, could be NodePrep or NodeRelease</param>
        private static void AddOrUpdateSpecialTask(IClusterJob clusterJob, IClusterTask clusterTask, string command, TaskType taskType)
        {
            if (string.IsNullOrEmpty(command))
            {
                return;
            }

            if (clusterTask != null)
            {
                clusterTask.SetProps(new StoreProperty(TaskPropertyIds.CommandLine, command));
            }
            else
            {
                List<StoreProperty> taskProps = new List<StoreProperty>();
                taskProps.Add(new StoreProperty(TaskPropertyIds.Type, taskType));
                taskProps.Add(new StoreProperty(TaskPropertyIds.CommandLine, command));
                taskProps.Add(new StoreProperty(TaskPropertyIds.MaxCores, 1));
                taskProps.Add(new StoreProperty(TaskPropertyIds.MinCores, 1));
                taskProps.Add(new StoreProperty(TaskPropertyIds.MaxSockets, 1));
                taskProps.Add(new StoreProperty(TaskPropertyIds.MinSockets, 1));
                taskProps.Add(new StoreProperty(TaskPropertyIds.MaxNodes, 1));
                taskProps.Add(new StoreProperty(TaskPropertyIds.MinNodes, 1));
                clusterJob.CreateTask(taskProps.ToArray());
            }
        }
    }
}
