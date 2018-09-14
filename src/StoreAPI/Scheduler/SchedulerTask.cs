using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{

    /// <summary>
    ///   <para>Defines a task.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interface.</para>
    /// </remarks>
    /// <example />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidTaskClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public partial class SchedulerTask : ISchedulerTask, ISchedulerTaskV2, ISchedulerTaskV3, ISchedulerTaskV3SP1, ISchedulerTaskV4, ISchedulerTaskV4SP1, ISchedulerTaskV4SP3
    {
        // Tasks can only be created by the API.        
        /// <summary>
        ///   <para>Initializes of new instance of the <see cref="Microsoft.Hpc.Scheduler.SchedulerTask" /> class.</para>
        /// </summary>
        internal protected SchedulerTask()
        {
        }

        ISchedulerStore _store = null;

        internal SchedulerTask(ISchedulerStore store)
        {
            _store = store;
        }

        internal void LocalInitFromRow(PropertyRow row)
        {
            _InitFromRow(row);

            StoreProperty prop = row[TaskPropertyIds.TaskObject];

            if (prop != null)
            {
                _task = (IClusterTask)prop.Value;
            }
        }

        CustomPropContainer _customProps = new CustomPropContainer(ObjectType.Task);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected Dictionary<PropertyId, StoreProperty> _changeProps = new Dictionary<PropertyId, StoreProperty>();

        /// <summary>
        ///   <para>The internal task object.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected IClusterTask _task = null;

        Dictionary<string, string> _localVars = new Dictionary<string, string>();

        internal protected void InitFromTask(IClusterTask task, PropertyId[] propIds)
        {
            _task = task;

            InitFromObject(task, propIds);
        }

        internal override void GetPropertyVersionCheck(PropertyId propId)
        {
            GetPropertyVersionCheck(_store, ObjectType.Task, this.GetType().Name, propId);
        }

        internal override void SetPropertyVersionCheck(PropertyId propId, object propValue)
        {
            SetPropertyVersionCheck(_store, ObjectType.Task, this.GetType().Name, propId, propValue);
        }

        /// <summary>
        ///   <para>Refreshes this copy of the task with the contents from the server.</para>
        /// </summary>
        public void Refresh()
        {
            if (_task == null)
            {
                throw new SchedulerException(ErrorCode.Operation_TaskNotCreatedOnServer, "");
            }

            _changeProps.Clear();

            InitFromObject(_task, GetCurrentPropsToReload());

            _customProps.Refresh(_store, _task);

            //reset the string lists
            _RequiredNodes_List = null;
            _DependsOn_List = null;
        }

        /// <summary>
        ///   <para>Commits the local task changes to the server.</para>
        /// </summary>
        public void Commit()
        {
            if (_task == null)
            {
                throw new SchedulerException(ErrorCode.Operation_TaskNotCreatedOnServer, "");
            }

            CommitStringLists();

            if (_changeProps.Keys.Count > 0 || _customProps.ChangeCount > 0)
            {
                _task.SetProps(new List<StoreProperty>(_changeProps.Values).ToArray());

                _customProps.Commit(_store, _task);

                _changeProps.Clear();
            }
        }

        /// <summary>
        ///   <para>Retrieves the application-defined properties that were added to the task.</para>
        /// </summary>
        /// <returns>
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> object that contains the collection of properties. Each item in the collection is a 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValue" /> object.</para>
        /// </returns>
        public INameValueCollection GetCustomProperties()
        {
            return _customProps.GetCustomProperties(_store, _task);
        }

        /// <summary>
        ///   <para>Sets an application-defined property on the task.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The property name. The name is limited to 80 characters.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The property value as a string. The name is limited to 1,024 characters.</para>
        /// </param>
        public void SetCustomProperty(string name, string value)
        {
            _customProps.SetCustomProperty(name, value);
        }

        void CommitStringLists()
        {
            //make sure changes to the string list are added to prop table
            if (_RequiredNodes_List != null)
            {
                RequiredNodes = _RequiredNodes_List;
            }

            if (_DependsOn_List != null)
            {
                DependsOn = _DependsOn_List;
            }
        }

        /// <summary>
        ///   <para>Retrieves counter data for the task.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerTaskCounters" /> interface that contains the counter data.</para>
        /// </returns>
        public ISchedulerTaskCounters GetCounters()
        {
            if (_task != null)
            {
                SchedulerTaskCounters counters = new SchedulerTaskCounters(_task);

                counters.Refresh();

                return counters;
            }

            return null;
        }

        /// <summary>
        ///   <para>Sets a task-specific environment variable.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The name of the variable.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The string value of the variable. If null or an empty string, the variable is deleted.</para>
        /// </param>
        public void SetEnvironmentVariable(string name, string value)
        {
            if (_task == null)
            {
                _localVars[name] = value;
            }
            else
            {
                _task.SetEnvironmentVariable(name, value);
            }
        }

        /// <summary>
        ///   <para>Retrieves the environment variables that were set for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains the collection of variables. Each item in the collection is an  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValue" /> interface that contains the variable name and value. The collection is empty if no variables have been set.</para> 
        /// </value>
        public INameValueCollection EnvironmentVariables
        {
            get
            {
                if (_task == null)
                {
                    return new NameValueCollection(_localVars, true);
                }
                else
                {
                    Dictionary<string, string> vars = _task.GetEnvironmentVariables();

                    return new NameValueCollection(vars, true);
                }
            }
        }

        internal bool BackedByStore
        {
            get
            {
                if (_task != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal IClusterTask ClusterTask
        {
            get { return _task; }
            set { _task = value; }
        }

        internal void CreateTask(IClusterJob job)
        {
            CreateTask(job, 0, null);
        }

        internal void CreateTask(IClusterJob job, int rootGroupId, Dictionary<int, int> taskGroupIdMapping)
        {

            CollectProps(rootGroupId, taskGroupIdMapping);

            _task = job.CreateTask(new List<StoreProperty>(_changeProps.Values).ToArray());

            SetEnvVars();
            //save the custom properties
            _customProps.Commit(_store, _task);

            _localVars.Clear();
            _changeProps.Clear();

            Refresh();
        }

        private void CollectProps(int rootGroupId, Dictionary<int, int> taskGroupIdMapping)
        {
            if (_task != null)
            {
                throw new InvalidOperationException("Cannot use an existing task object to create a new task");
            }

            CommitStringLists();


            // Remap the task groupId, if any
            if (taskGroupIdMapping != null)
            {
                StoreProperty groupIdProp = null;
                if (_changeProps.TryGetValue(TaskPropertyIds.GroupId, out groupIdProp))
                {
                    TaskType type = TaskType.Basic;
                    if (_props.ContainsKey(TaskPropertyIds.Type))
                    {
                        type = (TaskType)(_props[TaskPropertyIds.Type].Value);
                    }

                    if (TaskTypeHelper.IsGroupable(type))
                    {
                        int oldId = 0;
                        bool validValue = false;
                        if (groupIdProp.Value is int)
                        {
                            oldId = (int)groupIdProp.Value;
                            validValue = true;
                        }
                        if (groupIdProp.Value is string)
                        {
                            validValue = int.TryParse((string)groupIdProp.Value, out oldId);
                        }

                        int newId = rootGroupId;
                        if (validValue)
                        {
                            if (taskGroupIdMapping.ContainsKey(oldId))
                            {
                                newId = taskGroupIdMapping[oldId];
                            }

                        }
                        groupIdProp.Value = newId;
                    }
                    else
                    {
                        _changeProps.Remove(TaskPropertyIds.GroupId);
                    }
                }
            }
        }

        private void SetEnvVars()
        {
            foreach (KeyValuePair<string, string> item in _localVars)
            {
                _task.SetEnvironmentVariable(item.Key, item.Value);
            }
        }

        internal void AddPropsToList(IClusterJob job, int rootGroupId, Dictionary<int, int> taskGroupIdMapping, List<StoreProperty[]> propsListForTasks)
        {
            CollectProps(rootGroupId, taskGroupIdMapping);
            List<StoreProperty> propsList = new List<StoreProperty>(_changeProps.Values);
            _customProps.AddPropsToList(_store, propsList);
            propsListForTasks.Add(propsList.ToArray());

        }

        internal void PostCreateTask()
        {
            SetEnvVars();

            _localVars.Clear();
            _changeProps.Clear();

        }

        // Loading this property will be deferred until the property is explicitly requested
        /// <summary>
        ///   <para>Retrieves the names of the nodes that have been allocated to run the task or have run the task.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains the names of the nodes that have been allocated to run the task or have run the task.</para> 
        /// </value>
        public IStringCollection AllocatedNodes
        {
            get
            {
                LoadDeferredProp(_task, TaskPropertyIds.AllocatedNodes);

                StringCollection allocatedNodes = new StringCollection();
                StoreProperty allocatedNodesProp;
                if (!_props.TryGetValue(TaskPropertyIds.AllocatedNodes, out allocatedNodesProp))
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

        // Loading this property will be deferred until the property is explicitly requested
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string AllocatedCoreIds
        {
            get
            {
                GetPropertyVersionCheck(TaskPropertyIds.AllocatedCoreIds);

                LoadDeferredProp(_task, TaskPropertyIds.AllocatedCoreIds);

                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.AllocatedCoreIds, out prop))
                {
                    Dictionary<string, string> dict = prop.Value as Dictionary<string, string>;
                    if (dict == null)
                    {
                        return string.Empty;
                    }

                    StringBuilder bldr = new StringBuilder();
                    bool first = true;
                    foreach (KeyValuePair<string, string> item in dict)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            bldr.Append(" ");
                        }
                        bldr.Append(item.Key + " " + item.Value);
                    }

                    return bldr.ToString();
                }

                return string.Empty;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="cancelSubTasks">
        ///   <para />
        /// </param>
        public void ServiceConclude(bool cancelSubTasks)
        {
            if (_task == null)
            {
                throw new InvalidOperationException("The service task has not yet been created on the server");
            }
            _task.ServiceConclude(cancelSubTasks);
        }


        static PropertyId[] _enumInitIds = null;

        internal static PropertyId[] EnumInitIds
        {
            get
            {
                if (_enumInitIds == null)
                {
                    SchedulerTask sampleTask = new SchedulerTask();

                    List<PropertyId> ids = new List<PropertyId>(sampleTask.GetPropertyIds());

                    ids.Add(TaskPropertyIds.TaskObject);

                    _enumInitIds = ids.ToArray();
                }

                return _enumInitIds;
            }
        }

        internal void InitFromTaskPropertyBag(TaskPropertyBag taskBag)
        {
            foreach (StoreProperty prop in taskBag.FetchProperties(_store))
            {
                if ((prop.Id.Flags & PropFlags.Custom) != 0)
                {
                    _customProps.SetCustomProperty(prop.Id.Name, prop.Value.ToString());
                }
                else
                {
                    _changeProps[prop.Id] = prop;
                    _props[prop.Id] = prop;
                }
            }

            foreach (KeyValuePair<string, string> var in taskBag.FetchEnvironmentVariables())
            {
                SetEnvironmentVariable(var.Key, var.Value);
            }
        }

        /// <summary>
        /// Check whether task names do not contain invalid chars like comma
        /// comma is used as separator of list of task names, thus comma should not appear in task name
        /// </summary>
        /// <param name="input"></param>
        private static void CheckTaskNameFormat(IStringCollection input)
        {
            if (input == null)
            {
                return;
            }

            foreach (string taskName in input)
            {
                if (String.IsNullOrEmpty(taskName) || taskName.Trim() == String.Empty)
                {
                    continue;
                }

                // Find comma
                if (taskName.IndexOf(',') != -1)
                {
                    throw new SchedulerException(ErrorCode.Operation_TaskNameContainInvalidChars, taskName);
                }
            }
        }
    }
}
