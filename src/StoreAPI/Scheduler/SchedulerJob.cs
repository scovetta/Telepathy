using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Manages the tasks and resources that are associated with a job.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface.</para>
    /// </remarks>
    /// <example />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidJobClass)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(ISchedulerJobEvents))]
    public partial class SchedulerJob : ISchedulerJob, ISchedulerJobV3, ISchedulerJobV2, ISchedulerJobV3SP1, ISchedulerJobV3SP2, ISchedulerJobV3SP3, ISchedulerJobV4, ISchedulerJobV4SP1, ISchedulerJobV4SP2, ISchedulerJobV4SP3, ISchedulerJobV4SP5, ISchedulerJobV4SP6, ISchedulerJobV5
    {
        /// <summary>
        ///   <para>A dictionary of the properties that have been changed but not yet committed.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected Dictionary<PropertyId, StoreProperty> _changeProps = new Dictionary<PropertyId, StoreProperty>();

        /// <summary>
        ///   <para>The internal job object.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected IClusterJob _job = null;

        // Useful only if the job is restored from an xml file
        List<KeyValuePair<int, int>> _taskDependencies = new List<KeyValuePair<int, int>>();

        IScheduler _scheduler = null;

        /// <summary>
        ///   <para>The internal store object.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected ISchedulerStore _store = null;

        /// <summary>
        ///   <para>Initializes a new instance of this class using the specified scheduler object.</para>
        /// </summary>
        /// <param name="scheduler">
        ///   <para>A scheduler object used to initialize this instance.</para>
        /// </param>
        internal protected SchedulerJob(Scheduler scheduler)
        {
            _scheduler = scheduler;
            if (scheduler != null)
            {
                _store = scheduler.Store;
            }
        }

        internal protected SchedulerJob(ISchedulerStore store)
        {
            _store = store;
        }

        internal SchedulerJob(Scheduler scheduler, PropertyRow row)
        {
            _scheduler = scheduler;
            _store = scheduler.Store;

            _InitFromRow(row);

            StoreProperty propJob = row[JobPropertyIds.JobObject];

            if (propJob != null)
            {
                _job = (IClusterJob)propJob.Value;
            }
        }

        internal void Init(IClusterJob job)
        {
            _job = job;

            InitFromObject(job, null);
        }

        internal IClusterJob JobEx
        {
            get { return _job; }
        }

        internal void InitProfile(string profileName)
        {
            IClusterJobProfile profile = _store.OpenProfile(profileName);

            IClusterJobProfile defaultProfile = null;
            if (profileName == _defaultProfileName)
            {
                defaultProfile = profile;
            }
            else
            {
                defaultProfile = _store.OpenProfile(_defaultProfileName);
            }

            ClusterJobProfileItem[] items = profile.GetProfileItems();

            Dictionary<PropertyId, ClusterJobProfileItem> map = new Dictionary<PropertyId, ClusterJobProfileItem>();

            foreach (ClusterJobProfileItem item in items)
            {
                map.Add(item.PropId, item);
            }

            ClusterJobProfileItem[] defaultItems = defaultProfile.GetProfileItems();

            Dictionary<PropertyId, ClusterJobProfileItem> defaultMap = new Dictionary<PropertyId, ClusterJobProfileItem>();

            foreach (ClusterJobProfileItem item in defaultItems)
            {
                bool found = false;

                foreach (ClusterJobProfileItem tmpitem in items)
                {
                    if (item.PropId == tmpitem.PropId)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    defaultMap.Add(item.PropId, item);
                }
            }

            InitProfileVals(map);
            InitProfileVals(defaultMap);
        }

        internal void CreateJob()
        {
            if (Id != -1)
            {
                throw new SchedulerException(ErrorCode.Operation_JobAlreadyCreatedOnServer, "");
            }

            TraceHelper.TraceVerbose("SchedulerJob.CreateJob: Begin to create job");

            CommitStringLists();

            StoreProperty[] props = null;

            if (_changeProps.Values.Count > 0)
            {
                props = new List<StoreProperty>(_changeProps.Values).ToArray();
            }

            _job = _store.CreateJob(props);

            SetEnvVars();
            _customprops.Commit(_store, _job);
            PropagateExcludedNodes();

            _localVars.Clear();
            _localExcludedNodes.Clear();
            _changeProps.Clear();

            InitFromObject(_job, null);

            TraceHelper.TraceInfo("SchedulerJob.CreateJob: ID={0}", _job.Id);

            // Reconstruct the task groups
            IClusterTaskGroup rootGrp = _job.GetRootTaskGroup();
            int rootGrpId = rootGrp.Id;
            Dictionary<int, int> taskGroupIdMapping = new Dictionary<int, int>();

            if (_store.ServerVersion.Version < VersionControl.V3SP4)
            {
                // This is for backward compatibility

                Dictionary<int, IClusterTaskGroup> taskGroupIdMappingOld =
                    new Dictionary<int, IClusterTaskGroup>();

                JobPropertyBag.ReconstructTaskGroups(rootGrp, _taskDependencies, taskGroupIdMappingOld);

                foreach (KeyValuePair<int, IClusterTaskGroup> item in taskGroupIdMappingOld)
                {
                    taskGroupIdMapping[item.Key] = item.Value.Id;
                }
            }
            else
            {
                JobPropertyBag.ReconstructTaskGroupsEx(_job, rootGrp, _taskDependencies, taskGroupIdMapping);
            }

            CreateAddedTasks(rootGrpId, taskGroupIdMapping);

            TraceHelper.TraceInfo("SchedulerJob.CreateJob: Successfully created job {0}", _job.Id);
        }

        private const int TaskAdditionBatchSize = 1000;

        private void CreateAddedTasks(int rootGrpId, Dictionary<int, int> taskGroupIdMapping)
        {
            if (_tasks.Count != 0)
            {
                int taskIndex = 0;
                List<StoreProperty[]> propsListForTasks = new List<StoreProperty[]>();

                //Create the tasks in batches

                while (taskIndex < _tasks.Count)
                {
                    //the start index for this batch
                    int startBatchIndex = taskIndex;

                    //Add the properties for the tasks in this batch
                    //The batch size is limited by the TaskAdditionBatchSize or the number of remaining
                    //tasks whichever is lower

                    int endBatchIndex = Math.Min(_tasks.Count, (startBatchIndex + TaskAdditionBatchSize));

                    for (; taskIndex < endBatchIndex; taskIndex++)
                    {
                        SchedulerTask task = _tasks[taskIndex];
                        task.AddPropsToList(_job, rootGrpId, taskGroupIdMapping, propsListForTasks);
                    }



                    List<IClusterTask> clusterTaskList = _job.CreateTasks(propsListForTasks);

                    //assign the cluster objects of the created tasks to the right SchedulerTasks
                    for (int i = startBatchIndex; i < endBatchIndex; i++)
                    {
                        _tasks[i].ClusterTask = clusterTaskList[i - startBatchIndex];
                        _tasks[i].PostCreateTask();
                    }

                    propsListForTasks.Clear();
                }
                _tasks.Clear();
            }
        }

        internal void Submit(ISchedulerStore store, string username, string password)
        {
            List<StoreProperty> props;

            if (Id == -1)
            {
                // Need to create a new job
                CreateJob();
                props = new List<StoreProperty>();
            }
            // Else this is a job created by clone, and the values of the edited properties done after cloning should be submitted
            else
            {
                props = new List<StoreProperty>(_changeProps.Values);
            }

            if (username != null)
            {
                props.Add(new StoreProperty(JobPropertyIds.UserName, username));
            }

            if (password != null)
            {
                props.Add(new StoreProperty(JobPropertyIds.Password, password));
            }

            _job.Submit(props.ToArray());
        }

        /// <summary>
        ///   <para>Retrieves the counter data for the job.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters" /> interface that contains the counter data.</para>
        /// </returns>
        public ISchedulerJobCounters GetCounters()
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            SchedulerJobCounters counters = new SchedulerJobCounters(_job);
            counters.Refresh();

            return counters;
        }

        /// <summary>
        ///   <para>Creates a task.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interface that you use to define the task.</para>
        /// </returns>
        public ISchedulerTask CreateTask()
        {
            SchedulerTask task = new SchedulerTask(_store);

            // Set the task defaults.  The below are
            // needed to make sure that the task will
            // start when only a command line is specified.

            task.MinimumNumberOfCores = 1;
            task.MaximumNumberOfCores = 1;

            task.MinimumNumberOfNodes = 1;
            task.MaximumNumberOfNodes = 1;

            task.MinimumNumberOfSockets = 1;
            task.MaximumNumberOfSockets = 1;

            return task;
        }

        List<SchedulerTask> _tasks = new List<SchedulerTask>();

        /// <summary>
        ///   <para>Adds the task to the job.</para>
        /// </summary>
        /// <param name="task">
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interface of the task to add to the job. To create the task, call the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTask" /> method.</para>
        /// </param>
        public void AddTask(ISchedulerTask task)
        {
            SchedulerTask taskReal = (SchedulerTask)task;

            TraceHelper.TraceVerbose("SchedulerJob.AddTask: JobID={0}, taskID={0}", Id, task.TaskId);

            if (_job != null)
            {
                taskReal.CreateTask(_job);
            }
            else
            {
                _tasks.Add(taskReal);
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="taskList">
        ///   <para />
        /// </param>
        public void AddTasks(ISchedulerTask[] taskList)
        {
            foreach (ISchedulerTask task in taskList)
            {
                SchedulerTask taskReal = (SchedulerTask)task;

                TraceHelper.TraceVerbose("SchedulerJob.AddTasks: JobID={0}, taskID={0}", Id, task.TaskId);

                if (!taskReal.BackedByStore)
                {
                    _tasks.Add(taskReal);
                }
            }
            if (_job != null)
            {
                PropertyId[] loadProps = { JobPropertyIds.State };

                PropertyRow propRow = _job.GetProps(loadProps);

                LoadProps(propRow.Props);

                CreateAddedTasks(0, null);
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="taskList">
        ///   <para />
        /// </param>
        public void SubmitTasks(ISchedulerTask[] taskList)
        {
            AddTasks(taskList);

            if (_job != null)
            {
                PropertyId[] loadProps = { JobPropertyIds.State };

                PropertyRow propRow = _job.GetProps(loadProps);

                LoadProps(propRow.Props);

                CreateAddedTasks(0, null);


                if (State == JobState.Queued || State == JobState.Running)
                {
                    int[] taskIds = new int[taskList.Length];
                    for (int i = 0; i < taskList.Length; i++)
                    {
                        SchedulerTask taskReal = (SchedulerTask)taskList[i];
                        taskIds[i] = taskReal.ClusterTask.TaskId;
                    }
                    _job.SubmitTasks(taskIds);

                }
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected override PropertyId[] GetWriteOnlyPropertyIds()
        {
            PropertyId[] pids = { JobPropertyIds.Password };
            return pids;
        }


        internal override void GetPropertyVersionCheck(PropertyId propId)
        {
            GetPropertyVersionCheck(_store, ObjectType.Job, this.GetType().Name, propId);
        }

        internal override void SetPropertyVersionCheck(PropertyId propId, object propValue)
        {
            SetPropertyVersionCheck(_store, ObjectType.Job, this.GetType().Name, propId, propValue);
        }

        /// <summary>
        ///   <para>Refreshes this copy of the job with the contents from the server.</para>
        /// </summary>
        public void Refresh()
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            _changeProps.Clear();

            InitFromObject(_job, GetCurrentPropsToReload());

            _customprops.Refresh(_store, _job);

            //reset the string lists
            _RequestedNodes_List = null;
            _NodeGroups_List = null;
            _ExcludedNodes_List = null;
            _ParentJobIds_List = null;
        }

        /// <summary>
        ///   <para>Commits to the server any local changes to the job.</para>
        /// </summary>
        public void Commit()
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            CommitStringLists();

            if (_changeProps.Keys.Count > 0 || _customprops.ChangeCount > 0)
            {
                _job.SetProps(new List<StoreProperty>(_changeProps.Values).ToArray());

                _customprops.Commit(_store, _job);

                _changeProps.Clear();
            }

            CreateAddedTasks(0, null);
        }

        void CommitStringLists()
        {
            //make sure change to the string lists are added to the prop table
            if (_RequestedNodes_List != null)
            {
                RequestedNodes = _RequestedNodes_List;
            }

            if (_NodeGroups_List != null)
            {
                NodeGroups = _NodeGroups_List;
            }

            if (_SoftwareLicense_List != null)
            {
                SoftwareLicense = _SoftwareLicense_List;
            }

            if (_ParentJobIds_List != null)
            {
                ParentJobIds = _ParentJobIds_List;
            }
        }

        static TaskId TaskIdFromITaskId(ITaskId taskId)
        {
            return new TaskId(taskId.ParentJobId, taskId.JobTaskId, taskId.InstanceId);
        }

        /// <summary>
        ///   <para>Opens the task using the specified task identifier.</para>
        /// </summary>
        /// <param name="taskId">
        ///   <para>An <see cref="T:Microsoft.Hpc.Scheduler.Properties.ITaskId" /> interface that identifies the task to open.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="T:Microsoft.Hpc.Scheduler.ISchedulerTask" /> interface to the opened task.</para>
        /// </returns>
        /// <remarks />
        public ISchedulerTask OpenTask(ITaskId taskId)
        {
            IClusterTask itask = _job.OpenTask(TaskIdFromITaskId(taskId));

            SchedulerTask task = new SchedulerTask(_store);

            task.InitFromTask(itask, null);

            return task;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="taskSystemId">
        ///   <para />
        /// </param>
        /// <param name="callback">
        ///   <para />
        /// </param>
        /// <param name="message">
        ///   <para />
        /// </param>
        /// <param name="isForced">
        ///   <para />
        /// </param>
        public IAsyncResult BeginFinishTask(int taskSystemId, string message, bool isForced, AsyncCallback callback, object state)
        {
            TraceHelper.TraceInfo("SchedulerJob.CancelTask: jobId={0}, taskSystemId={1}, message={2}, isForced={3}", Id, taskSystemId, message ?? string.Empty, isForced);

            return this._job.BeginCancelTask(taskSystemId, isForced ? CancelRequest.Finish : CancelRequest.FinishGraceful, callback, state, message);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="ar">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public TaskState EndFinishTask(IAsyncResult ar)
        {
            TraceHelper.TraceInfo("SchedulerJob.EndFinishTask");

            return this._job.EndCancelTask(ar);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="taskId">
        ///   <para />
        /// </param>
        public void CancelTask(ITaskId taskId)
        {
            CancelTask(taskId, string.Empty);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="taskId">
        ///   <para />
        /// </param>
        /// <param name="message">
        ///   <para />
        /// </param>
        public void CancelTask(ITaskId taskId, string message)
        {
            CancelTask(taskId, message, false);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="taskId">
        ///   <para />
        /// </param>
        /// <param name="message">
        ///   <para />
        /// </param>
        /// <param name="isForced">
        ///   <para />
        /// </param>
        public void CancelTask(ITaskId taskId, string message, bool isForced)
        {
            TraceHelper.TraceInfo("SchedulerJob.CancelTask: jobId={0}, taskId={1}, message={2}, isForced={3}", Id, taskId, message ?? string.Empty, isForced);

            IClusterTask task = _job.OpenTask(TaskIdFromITaskId(taskId));
            _job.CancelTask(task.Id, message, isForced);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="taskId">
        ///   <para />
        /// </param>
        /// <param name="message">
        ///   <para />
        /// </param>
        public void FinishTask(ITaskId taskId, string message)
        {
            TraceHelper.TraceInfo("SchedulerJob.FinishTask: jobId={0}, taskId={1}", Id, taskId);

            this._job.FinishTaskByNiceId(taskId.JobTaskId, message);
        }

        /// <summary>
        ///   <para>Submits a task to the job using the specified task interface.</para>
        /// </summary>
        /// <param name="task">
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interface of the task to add to the job</para>
        /// </param>
        public void SubmitTask(ISchedulerTask task)
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            if (task.State != TaskState.Configuring)
            {
                throw new SchedulerException(ErrorCode.Operation_OnlyConfiguringTasksCanBeSubmitted, "");
            }

            TraceHelper.TraceInfo("SchedulerJob.SubmitTask: jobId={0}, taskId={1}", Id, task.TaskId);

            SchedulerTask realTask = (SchedulerTask)task;

            if (realTask.BackedByStore == false)
            {
                realTask.CreateTask(_job);
            }

            realTask.ClusterTask.SubmitTask();
        }

        /// <summary>
        ///   <para>Submits a task to the job using the task identifier to identify the task.</para>
        /// </summary>
        /// <param name="taskId">
        ///   <para>An <see cref="T:Microsoft.Hpc.Scheduler.Properties.ITaskId" /> interface that identifies the task to add to the job.</para>
        /// </param>
        /// <remarks />
        public void SubmitTaskById(ITaskId taskId)
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            TraceHelper.TraceInfo("SchedulerJob.SubmitTaskById: jobId={0}, taskId={1}", Id, taskId);

            IClusterTask task = _job.OpenTask(TaskIdFromITaskId(taskId));

            task.SubmitTask();
        }

        /// <summary>
        ///   <para>Queues a task again.</para>
        /// </summary>
        /// <param name="taskId">
        ///   <para>A <see cref="T:Microsoft.Hpc.Scheduler.Properties.ITaskId" /> objects that identifies the task to queue again.</para>
        /// </param>
        /// <remarks />
        public void RequeueTask(ITaskId taskId)
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            TraceHelper.TraceInfo("SchedulerJob.RequeueTask: jobId={0}, taskId={1}", Id, taskId);

            IClusterTask task = _job.OpenTask(TaskIdFromITaskId(taskId));

            task.Configure();
            task.SubmitTask();
        }

        /// <summary>
        ///   <para>Retrieves a list of task objects based on the specified filters.</para>
        /// </summary>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains a collection of one or more filter properties used to filter the list of tasks. If null, the method returns all tasks.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An array of 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains a collection of one or more sort properties used to sort the list of tasks. If null, the list is not sorted.</para> 
        /// </param>
        /// <param name="expandParametric">
        ///   <para>Set to true to include parametric instances in the results; otherwise, false.</para>
        /// </param>
        /// <returns>
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCollection" /> interface that contains a collection of task identifiers (see 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" />) that meet the filter criteria.</para>
        /// </returns>
        /// <remarks>
        ///   <para>If you specify more than one filter, a logical AND is applied to 
        /// the filters. For example, return tasks that are running and have exclusive access to the nodes.</para>
        ///   <para>Only the job owner or administrator can list the tasks in 
        /// a job. The job must have been added to the scheduler before calling this method.</para>
        /// </remarks>
        public ISchedulerCollection GetTaskList(IFilterCollection filter, ISortCollection sort, bool expandParametric)
        {
            if (_job == null)
            {
                return new SchedulerCollection<SchedulerTask>(_tasks);
            }
            else
            {
                TaskRowSetOptions options = TaskRowSetOptions.NoParametricExpansion;

                if (expandParametric)
                {
                    options = TaskRowSetOptions.None;
                }

                List<SchedulerTask> result = new List<SchedulerTask>();

                using (IRowEnumerator rowenum = _job.OpenTaskRowEnumerator(options))
                {
                    if (filter != null)
                    {
                        rowenum.SetFilter(filter.GetFilters());
                    }

                    if (sort != null)
                    {
                        rowenum.SetSortOrder(sort.GetSorts());
                    }

                    rowenum.SetColumns(SchedulerTask.EnumInitIds);

                    foreach (PropertyRow row in rowenum)
                    {
                        SchedulerTask task = new SchedulerTask(_store);

                        task.LocalInitFromRow(row);

                        result.Add(task);
                    }
                }

                return new SchedulerCollection<SchedulerTask>(result);
            }
        }

        /// <summary>
        ///   <para>Retrieves a list of task identifiers based on the specified filters.</para>
        /// </summary>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains a collection of one or more filter properties used to filter the list of tasks. If null, the method returns all tasks.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An array of 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains a collection of one or more sort properties used to sort the list of tasks. If null, the list is not sorted.</para> 
        /// </param>
        /// <param name="expandParametric">
        ///   <para>Set to true to include parametric instances in the results; otherwise, false.</para>
        /// </param>
        /// <returns>
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCollection" /> interface that contains a collection of task identifiers (see 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" />) that meet the filter criteria.</para>
        /// </returns>
        /// <remarks>
        ///   <para>If you specify more than one filter, a logical AND is applied to 
        /// the filters. For example, return tasks that are running and have exclusive access to the nodes.</para>
        ///   <para>Only the job owner or administrator can list the tasks in 
        /// a job. The job must have been added to the scheduler before calling this method.</para>
        /// </remarks>
        public ISchedulerCollection GetTaskIdList(IFilterCollection filter, ISortCollection sort, bool expandParametric)
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            TaskRowSetOptions options = TaskRowSetOptions.NoParametricExpansion;

            if (expandParametric)
            {
                options = TaskRowSetOptions.None;
            }

            List<TaskId> result = new List<TaskId>();

            using (IRowEnumerator rowenum = _job.OpenTaskRowEnumerator(options))
            {
                if (filter != null)
                {
                    rowenum.SetFilter(filter.GetFilters());
                }

                if (sort != null)
                {
                    rowenum.SetSortOrder(sort.GetSorts());
                }

                rowenum.SetColumns(TaskPropertyIds.TaskId);

                foreach (PropertyRow row in rowenum)
                {
                    if (row[0].Id == TaskPropertyIds.TaskId)
                    {
                        result.Add((TaskId)row[0].Value);
                    }
                }
            }

            return new SchedulerCollection<TaskId>(result);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        public ISchedulerRowEnumerator OpenJobAllocationHistoryEnumerator(
            IPropertyIdCollection properties)
        {
            IFilterCollection filter = new FilterCollection();
            filter.Add(new FilterProperty(FilterOperator.Equal, AllocationProperties.JobId, _job.Id));
            filter.Add(new FilterProperty(FilterOperator.Equal, AllocationProperties.TaskId, 0));

            return OpenAllocationHistoryEnumeratorInternal(properties, filter);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        public ISchedulerRowEnumerator OpenTaskAllocationHistoryEnumerator(
            IPropertyIdCollection properties)
        {
            IFilterCollection filter = new FilterCollection();
            filter.Add(new FilterProperty(FilterOperator.Equal, AllocationProperties.JobId, _job.Id));
            filter.Add(new FilterProperty(FilterOperator.NotEqual, AllocationProperties.TaskId, 0));

            return OpenAllocationHistoryEnumeratorInternal(properties, filter);
        }

        internal ISchedulerRowEnumerator OpenAllocationHistoryEnumeratorInternal(
            IPropertyIdCollection properties,
            IFilterCollection filter)
        {
            if (_job == null || _store == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            IRowEnumerator rows = _store.OpenAllocationEnumerator();

            AllocationHistoryEnumerator result = new AllocationHistoryEnumerator();

            result.Init(rows, properties, filter, null);

            return result;
        }

        /// <summary>
        ///   <para>Retrieves a rowset enumerator that contains the tasks that match the filter criteria.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the task properties that you want to include for each task in the enumerator.</para> 
        /// </param>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of tasks. If null, the method returns all tasks.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of tasks. If null, the list is not sorted.</para> 
        /// </param>
        /// <param name="expandParametric">
        ///   <para>Set to true to include parametric instances in the results; otherwise, false.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator" /> interface that you can use to enumerate the results.</para>
        /// </returns>
        [ComVisible(false)]
        public ISchedulerRowEnumerator OpenTaskEnumerator(
                IPropertyIdCollection properties,
                IFilterCollection filter,
                ISortCollection sort,
                bool expandParametric
                )
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            IRowEnumerator rows = _job.OpenTaskRowEnumerator(expandParametric ? TaskRowSetOptions.None : TaskRowSetOptions.NoParametricExpansion);

            TaskEnumerator result = new TaskEnumerator();

            result.Init(rows, properties, filter, sort);

            return result;
        }

        /// <summary>
        ///   <para>Retrieves a rowset that contains the tasks that match the filter criteria.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the task properties that you want to include for each task in the rowset.</para> 
        /// </param>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of tasks. If null, the method returns all tasks.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of tasks. If null, the list is not sorted.</para> 
        /// </param>
        /// <param name="expandParametric">
        ///   <para>Set to true to include parametric instances in the results; otherwise, false.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowSet" /> interface that you use to access the results.</para>
        /// </returns>
        [ComVisible(false)]
        public ISchedulerRowSet OpenTaskRowSet(
                IPropertyIdCollection properties,
                IFilterCollection filter,
                ISortCollection sort,
                bool expandParametric
                )
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_JobNotCreatedOnServer, "");
            }

            IRowSet rows = _job.OpenTaskRowSet(RowSetType.Snapshot, expandParametric ? TaskRowSetOptions.None : TaskRowSetOptions.NoParametricExpansion);

            TaskRowSet result = new TaskRowSet();

            result.Init(rows, properties, filter, sort);

            return result;
        }


        static string _defaultProfileName = "Default";


        /// <summary>
        ///   <para>Sets the job template to use for the job.</para>
        /// </summary>
        /// <param name="templateName">
        ///   <para>The name of the template to use for this job.</para>
        /// </param>
        public void SetJobTemplate(string templateName)
        {
            TraceHelper.TraceVerbose("SchedulerJob.RequeueTask: jobId={0}, templateName={1}", Id, templateName ?? string.Empty);

            _changeProps[JobPropertyIds.JobTemplate] = new StoreProperty(JobPropertyIds.JobTemplate, templateName);
            _props[JobPropertyIds.JobTemplate] = _changeProps[JobPropertyIds.JobTemplate];

            InitProfile(templateName);
        }


        /// <summary>
        ///   <para />
        /// </summary>
        public void Finish()
        {
            this.Finish(false, false);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="isForce">
        ///   <para />
        /// </param>
        /// <param name="isGraceful">
        ///   <para />
        /// </param>
        public void Finish(bool isForce, bool isGraceful)
        {
            TraceHelper.TraceInfo("SchedulerJob.Finish: jobId={0}, isForce={1}, isGraceful={2}", this.Id, isForce, isGraceful);

            _job.Finish("Finished by user", isForce, isGraceful);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="isForce">
        ///   <para />
        /// </param>
        /// <param name="isGraceful">
        ///   <para />
        /// </param>
        public void Cancel(bool isForce, bool isGraceful)
        {
            TraceHelper.TraceInfo("SchedulerJob.Cancel: jobId={0}, isForce={1}, isGraceful={2}", this.Id, isForce, isGraceful);

            _job.Cancel("Canceled by user", isForce, isGraceful);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        public void Requeue()
        {
            TraceHelper.TraceInfo("SchedulerJob.Requeue: jobId={0}", Id);

            _job.Requeue();
        }

        Dictionary<string, string> _localVars = new Dictionary<string, string>();

        private void SetEnvVars()
        {
            foreach (KeyValuePair<string, string> item in _localVars)
            {
                _job.SetEnvironmentVariable(item.Key, item.Value);
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="value">
        ///   <para />
        /// </param>
        public void SetEnvironmentVariable(string name, string value)
        {
            if (_job == null)
            {
                _localVars[name] = value;
            }
            else
            {
                _job.SetEnvironmentVariable(name, value);
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public INameValueCollection EnvironmentVariables
        {
            get
            {
                if (_job == null)
                {
                    return new NameValueCollection(_localVars, true);
                }
                else
                {
                    Dictionary<string, string> vars = _job.GetEnvironmentVariables();

                    return new NameValueCollection(vars, true);
                }
            }
        }

        /// <summary>
        ///   <para>Overwrites the properties and tasks of the job using the XML at the specified URL.</para>
        /// </summary>
        /// <param name="url">
        ///   <para>The URL that identifies the XML to use to overwrite the contents of the job.</para>
        /// </param>
        public void RestoreFromXml(string url)
        {
            using (XmlTextReader reader = new XmlTextReader(url) { DtdProcessing = DtdProcessing.Prohibit })
            {
                RestoreFromXml(reader);
            }
        }

        /// <summary>
        ///   <para>Overwrites the properties and tasks of the job using the contents from the XML reader.</para>
        /// </summary>
        /// <param name="reader">
        ///   <para>An <see cref="System.Xml.XmlReader" /> that contains the XML used to overwrite the content of the job.</para>
        /// </param>
        [ComVisible(false)]
        public void RestoreFromXml(XmlReader reader)
        {
            _RestoreFromXml(reader, XmlImportOptions.NoTaskGroups);
        }

        internal protected void ProtectedRestoreFromXml(Stream xmlStream, XmlImportOptions options)
        {
            xmlStream.Position = 0;

            using (XmlTextReader reader = new XmlTextReader(xmlStream) { DtdProcessing = DtdProcessing.Prohibit })
            {
                _RestoreFromXml(reader, options);
            }
        }

        internal void _RestoreFromXml(XmlReader reader, XmlImportOptions options)
        {
            JobPropertyBag jobBag = new JobPropertyBag();

            jobBag.ReadXML(reader, options);

            foreach (StoreProperty prop in jobBag.FetchProperties(_store))
            {
                if ((prop.Id.Flags & PropFlags.Custom) != 0)
                {
                    _customprops.SetCustomProperty(prop.Id.Name, prop.Value.ToString());
                }
                else if (prop.Id == JobPropertyIds.JobTemplate)
                {
                    SetJobTemplate(prop.Value.ToString());
                }
                else
                {
                    _changeProps[prop.Id] = prop;
                    _props[prop.Id] = prop;
                }
            }

            IDictionary<string, string> jobEnvVars = jobBag.FetchEnvironmentVariables();
            if (jobEnvVars != null)
            {
                foreach (KeyValuePair<string, string> var in jobEnvVars)
                {
                    SetEnvironmentVariable(var.Key, var.Value);
                }
            }

            foreach (TaskPropertyBag taskBag in jobBag.GetTasks())
            {
                SchedulerTask task = new SchedulerTask(_store);

                task.InitFromTaskPropertyBag(taskBag);

                _tasks.Add(task);
            }

            _taskDependencies = jobBag.GetTaskDependencies();
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="holdUntil">
        ///   <para />
        /// </param>
        public void SetHoldUntil(DateTime holdUntil)
        {
            if (holdUntil < DateTime.Now)
            {
                throw new SchedulerException(ErrorCode.Operation_JobInvalidHoldUntil, "");
            }
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_InvalidJobId, "");
            }

            TraceHelper.TraceVerbose("SchedulerJob.SetHoldUntil: jobId={0}, holdUntil={1}", Id, HoldUntil);

            _job.SetHoldUntil(holdUntil.ToUniversalTime());
        }

        /// <summary>
        ///   <para />
        /// </summary>
        public void ClearHold()
        {
            if (_job == null)
            {
                throw new SchedulerException(ErrorCode.Operation_InvalidJobId, "");
            }

            TraceHelper.TraceVerbose("SchedulerJob.ClearHold: jobId={0}", Id);

            _job.SetHoldUntil(DateTime.MinValue);
        }


        // If the job is not yet created on the server, this dictionary will contains the names of the nodes that will be
        // propagated to the server once the job is created.
        Dictionary<string, object> _localExcludedNodes = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="nodeNames">
        ///   <para />
        /// </param>
        public void AddExcludedNodes(IStringCollection nodeNames)
        {
            Util.CheckCollectionForNullOrEmptyStrings(nodeNames);
            if (nodeNames.Count == 0)
            {
                return;
            }

            TraceHelper.TraceVerbose("SchedulerJob.AddExcludedNodes: jobId={0}, nodeNames={1}", Id, string.Join(",", nodeNames));

            if (_job == null)
            {
                foreach (string nodeName in nodeNames)
                {
                    _localExcludedNodes[nodeName] = null;
                }
                _ExcludedNodes_List = new StringCollection(_localExcludedNodes.Keys, true);
            }
            else
            {
                _job.AddExcludedNodes(Util.Collection2Array<string>(nodeNames));
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="nodeNames">
        ///   <para />
        /// </param>
        public void RemoveExcludedNodes(IStringCollection nodeNames)
        {
            Util.CheckCollectionForNullOrEmptyStrings(nodeNames);
            if (nodeNames.Count == 0)
            {
                return;
            }

            TraceHelper.TraceVerbose("SchedulerJob.RemoveExcludedNodes: jobId={0}, nodeNames={1}", Id, string.Join(",", nodeNames));

            if (_job == null)
            {
                foreach (string nodeName in nodeNames)
                {
                    _localExcludedNodes.Remove(nodeName);
                }
                _ExcludedNodes_List = new StringCollection(_localExcludedNodes.Keys, true);
            }
            else
            {
                _job.RemoveExcludedNodes(Util.Collection2Array<string>(nodeNames));
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        public void ClearExcludedNodes()
        {
            TraceHelper.TraceVerbose("SchedulerJob.ClearExcludedNodes: jobId={0}", Id);

            if (_job == null)
            {
                _localExcludedNodes.Clear();
                _ExcludedNodes_List = new StringCollection(true);
            }
            else
            {
                _job.ClearExcludedNodes();
            }
        }

        private void PropagateExcludedNodes()
        {
            if (_localExcludedNodes.Count > 0)
            {
                _job.AddExcludedNodes(Util.Collection2Array<string>(_localExcludedNodes.Keys));
                _localExcludedNodes.Clear();
            }
        }

        CustomPropContainer _customprops = new CustomPropContainer(ObjectType.Job);

        /// <summary>
        ///   <para>Sets an application-defined property on the job.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The property name. The name is limited to 80 characters.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The property value as a string. The name is limited to 1,024 characters.</para>
        /// </param>
        public void SetCustomProperty(string name, string value)
        {
            TraceHelper.TraceVerbose("SchedulerJob.SetCustomProperty: jobId={0}, name={1}, value={2}", Id, name ?? string.Empty, value ?? string.Empty);

            _customprops.SetCustomProperty(name, value);
        }

        /// <summary>
        ///   <para>Retrieves the application-defined properties for the job.</para>
        /// </summary>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains the collection of properties. Each item in the collection is an  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValue" /> interface that contains the property name and value. The collection is empty if no properties have been set.</para> 
        /// </returns>
        public INameValueCollection GetCustomProperties()
        {
            return _customprops.GetCustomProperties(_store, _job);
        }

        void StoreJobEventHandler(Int32 id, EventType eventType, StoreProperty[] props)
        {
            if (eventType != EventType.Modify || props == null)
            {
                return;
            }

            if (id == Id)
            {
                // Find the states

                StoreProperty newState = null;
                StoreProperty oldState = null;

                foreach (StoreProperty prop in props)
                {
                    if (prop.Id == JobPropertyIds.State)
                    {
                        newState = prop;
                    }
                    else if (prop.Id == JobPropertyIds.PreviousState)
                    {
                        oldState = prop;
                    }
                }

                if (newState != null && oldState != null)
                {
                    JobStateHandler handler = _onJobStateChange;

                    if (handler != null)
                    {
                        handler(_scheduler, new JobStateEventArg(Id, (JobState)newState.Value, (JobState)oldState.Value));
                    }
                }
            }
        }

        void StoreTaskEventHandler(Int32 jobId, Int32 taskSystemId, TaskId taskId, EventType eventType, StoreProperty[] props)
        {
            if (eventType != EventType.Modify || props == null)
            {
                return;
            }

            if (jobId == Id)
            {
                // Find the states

                StoreProperty newState = null;
                StoreProperty oldState = null;

                foreach (StoreProperty prop in props)
                {
                    if (prop.Id == TaskPropertyIds.State)
                    {
                        newState = prop;
                    }
                    else if (prop.Id == TaskPropertyIds.PreviousState)
                    {
                        oldState = prop;
                    }
                }

                if (newState != null && oldState != null)
                {
                    if (taskId.InstanceId < 0)
                    {
                        // Per HPCv3 bug 1140, master task state changes should not be reported to public-API clients.  This is done
                        // in order to maintain back-compatibility with programs written for V2 clients, which may not expect negative
                        // task instance IDs.
                        return;
                    }

                    TaskStateHandler handler = _onTaskStateChange;
                    if (handler != null)
                    {
                        handler(_scheduler, new TaskStateEventArg(Id, taskId, (TaskState)newState.Value, (TaskState)oldState.Value));
                    }
                }
            }
        }

        JobStateHandler _onJobStateChange;
        TaskStateHandler _onTaskStateChange;

        object _eventLock = new object();

        event EventHandler<JobStateEventArg> ISchedulerJobV2.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV3.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV3SP1.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV3SP2.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV3SP3.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV4.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV4SP1.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV4SP2.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV4SP3.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV4SP5.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV4SP6.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJobV5.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<JobStateEventArg> ISchedulerJob.OnJobState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState += (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnJobState -= (JobStateHandler)Delegate.CreateDelegate(typeof(JobStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV2.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV3.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV3SP1.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV3SP2.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV3SP3.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV4.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV4SP1.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV4SP2.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV4SP3.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV4SP5.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }


        event EventHandler<TaskStateEventArg> ISchedulerJobV4SP6.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<TaskStateEventArg> ISchedulerJobV5.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }


        event EventHandler<TaskStateEventArg> ISchedulerJob.OnTaskState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState += (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerJob)this).OnTaskState -= (TaskStateHandler)Delegate.CreateDelegate(typeof(TaskStateHandler), value.Target, value.Method);
                }
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        public event JobStateHandler OnJobState
        {
            add
            {
                lock (_eventLock)
                {
                    if (_onJobStateChange == null)
                    {
                        // This is the first event, need
                        // to register with the store to
                        // get notifications.

                        _store.JobEvent += StoreJobEventHandler;
                    }

                    _onJobStateChange += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    _onJobStateChange -= value;

                    if (_onJobStateChange == null)
                    {
                        // No longer need notifications
                        // from the store.

                        _store.JobEvent -= StoreJobEventHandler;
                    }
                }
            }
        }

        bool _useJobTaskEvt = false;
        /// <summary>
        ///   <para />
        /// </summary>
        public event TaskStateHandler OnTaskState
        {
            add
            {
                lock (_eventLock)
                {
                    if (_onTaskStateChange == null)
                    {
                        if (_job == null)
                        {
                            // Register to the store
                            _store.TaskEvent += StoreTaskEventHandler;
                        }
                        else
                        {
                            // Register only the job's task events
                            _job.TaskEvent += StoreTaskEventHandler;
                            _useJobTaskEvt = true;
                        }
                    }

                    _onTaskStateChange += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    _onTaskStateChange -= value;

                    if (_onTaskStateChange == null)
                    {
                        if (_useJobTaskEvt)
                        {
                            if (_job != null)
                            {
                                _job.TaskEvent -= StoreTaskEventHandler;
                            }
                        }
                        else
                        {
                            _store.TaskEvent -= StoreTaskEventHandler;
                        }
                    }
                }
            }
        }

        static PropertyId[] _enumInitIds = null;

        internal static PropertyId[] EnumInitIds
        {
            get
            {
                if (_enumInitIds == null)
                {
                    SchedulerJob sampleJob = new SchedulerJob(null as Scheduler);

                    List<PropertyId> ids = new List<PropertyId>(sampleJob.GetPropertyIds());

                    ids.Add(JobPropertyIds.JobObject);

                    _enumInitIds = ids.ToArray();
                }

                return _enumInitIds;
            }
        }

        // Loading this property will be deferred until the property is explicitly requested
        /// <summary>
        ///   <para>Retrieves the names of the nodes that have been allocated to run the tasks in the job or have run the tasks.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains the names of the nodes that have been allocated to run the tasks in the job or have run the tasks.</para> 
        /// </value>
        public IStringCollection AllocatedNodes
        {
            get
            {
                LoadDeferredProp(_job, JobPropertyIds.AllocatedNodes);

                StringCollection allocatedNodes = new StringCollection();
                StoreProperty allocatedNodesProp;
                if (!_props.TryGetValue(JobPropertyIds.AllocatedNodes, out allocatedNodesProp))
                {
                    return allocatedNodes;
                }

                ICollection<KeyValuePair<string, int>> allocationList = allocatedNodesProp.Value as ICollection<KeyValuePair<string, int>>;
                if (allocationList == null || allocationList.Count == 0)
                {
                    return allocatedNodes;
                }

                foreach (KeyValuePair<string, int> allocatedNode in allocationList)
                {
                    allocatedNodes.Add(allocatedNode.Key);
                }

                return allocatedNodes;
            }
        }

        /// <summary>
        ///   <para>Retrieves the unique network addresses that a client uses to communicate with a service endpoint.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of endpoint addresses to which the session is connected.</para> 
        /// </value>
        public IStringCollection EndpointAddresses
        {
            get
            {
                StoreProperty endPointProp;
                if (!_props.TryGetValue(JobPropertyIds.EndpointReference, out endPointProp))
                {
                    return new StringCollection();
                }

                return Util.String2Collection(endPointProp.Value as string, false, ';');
            }
        }

        string _Orderby_ProfileDefault = null;
        string _Orderby_Default = "";

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string OrderBy
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.OrderBy, out prop))
                {
                    return ((JobOrderByList)prop.Value).ToString();
                }

                if (_Orderby_ProfileDefault != null)
                {
                    return _Orderby_ProfileDefault;
                }

                return _Orderby_Default;
            }

            set
            {
                JobOrderByList orderby = JobOrderByList.Parse(value);
                StoreProperty prop = new StoreProperty(JobPropertyIds.OrderBy, orderby);
                _props[JobPropertyIds.OrderBy] = prop;
                _changeProps[JobPropertyIds.OrderBy] = prop;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="url">
        ///   <para />
        /// </param>
        /// <param name="includeTaskGroup">
        ///   <para />
        /// </param>
        public void RestoreFromXmlEx(string url, bool includeTaskGroup)
        {
            using (XmlTextReader reader = new XmlTextReader(url) { DtdProcessing = DtdProcessing.Prohibit })
            {
                this.RestoreFromXmlEx(reader, includeTaskGroup);
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="reader">
        ///   <para />
        /// </param>
        /// <param name="includeTaskGroup">
        ///   <para />
        /// </param>
        [ComVisible(false)]
        public void RestoreFromXmlEx(XmlReader reader, bool includeTaskGroup)
        {
            if (includeTaskGroup)
            {
                this._RestoreFromXml(reader, XmlImportOptions.None);
            }
            else
            {
                this._RestoreFromXml(reader, XmlImportOptions.NoTaskGroups);
            }
        }

        #region V4SP6 and V5SP2 methods / properties

        public bool GetBalanceRequest(out IList<BalanceRequest> request)
        {
            return null != (request = this.JobEx.GetBalanceRequest());
        }

        #endregion
    }

    /// <summary>
    ///   <para>Manages the tasks and resources that are associated with a job.</para>
    /// </summary>
    /// <summary>
    ///   <para>Manages the tasks and resources that are associated with a job.</para>
    /// </summary>
    /// <summary>
    ///   <para>Manages the tasks and resources that are associated with a job.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface.</para>
    /// </remarks>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface.</para>
    /// </remarks>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface.</para>
    /// </remarks>
    /// <example />
    /// <example />
    /// <example />
    public partial class SchedulerJob : ISchedulerJob, ISchedulerJobV3, ISchedulerJobV2, ISchedulerJobV3SP1
    {
        void InitProfileVals(Dictionary<PropertyId, ClusterJobProfileItem> map)
        {
            ClusterJobProfileItem prop;

            if (map.TryGetValue(JobPropertyIds.Name, out prop))
            {
                _Name_ProfileDefault = prop.StringDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.UserName, out prop))
            {
                _UserName_ProfileDefault = prop.StringDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.ExpandedPriority, out prop))
            {
                _ExpandedPriority_Default = prop.IntDefaultValue;
            }
            else if (map.TryGetValue(JobPropertyIds.Priority, out prop))
            {
                // Back-compat with V2 job templates
                _ExpandedPriority_Default = Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.JobPriorityToExpandedPriority((int)prop.PriorityDefaultValue);
            }

            if (map.TryGetValue(JobPropertyIds.Project, out prop))
            {
                _Project_ProfileDefault = prop.StringDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.RuntimeSeconds, out prop))
            {
                _RuntimeSeconds_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MinCores, out prop))
            {
                _MinCores_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MaxCores, out prop))
            {
                _MaxCores_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MinNodes, out prop))
            {
                _MinNodes_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MaxNodes, out prop))
            {
                _MaxNodes_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MinSockets, out prop))
            {
                _MinSockets_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MaxSockets, out prop))
            {
                _MaxSockets_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.UnitType, out prop))
            {
                _UnitType_ProfileDefault = prop.UnitTypeDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.RequestedNodes, out prop))
            {
                _RequestedNodes_ProfileDefault = prop.StringDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.IsExclusive, out prop))
            {
                _IsExclusive_ProfileDefault = prop.BoolDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.RunUntilCanceled, out prop))
            {
                _RunUntilCanceled_ProfileDefault = prop.BoolDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.NodeGroups, out prop))
            {
                _NodeGroups_ProfileDefault = prop.StringDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.FailOnTaskFailure, out prop))
            {
                _FailOnTaskFailure_ProfileDefault = prop.BoolDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.AutoCalculateMax, out prop))
            {
                _AutoCalculateMax_ProfileDefault = prop.BoolDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.AutoCalculateMin, out prop))
            {
                _AutoCalculateMin_ProfileDefault = prop.BoolDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.CanGrow, out prop))
            {
                _CanGrow_ProfileDefault = prop.BoolDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.CanShrink, out prop))
            {
                _CanShrink_ProfileDefault = prop.BoolDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.Preemptable, out prop))
            {
                _Preemptable_ProfileDefault = prop.BoolDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MinMemory, out prop))
            {
                _MinMemory_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MaxMemory, out prop))
            {
                _MaxMemory_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MinCoresPerNode, out prop))
            {
                _MinCoresPerNode_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.MaxCoresPerNode, out prop))
            {
                _MaxCoresPerNode_ProfileDefault = prop.IntDefaultValue;
            }

            if (map.TryGetValue(JobPropertyIds.SoftwareLicense, out prop))
            {
                _SoftwareLicense_ProfileDefault = prop.StringDefaultValue;
            }
            if (map.TryGetValue(JobPropertyIds.Pool, out prop))
            {
                _Pool_ProfileDefault = prop.StringDefaultValue;
            }
        }
    }
}
