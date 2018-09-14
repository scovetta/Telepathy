namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Hpc.Scheduler.Properties;
    using IntPair = System.Collections.Generic.KeyValuePair<int, int>;

    internal class TaskGroupHost
    {
        private SchedulerStoreSvc _helper;
        private int _jobId;
        private ConnectionToken _token;

        private int _rootId = 0;

        Dictionary<int, TaskGroup> _items;

        internal TaskGroupHost(SchedulerStoreSvc helper, int jobId, ConnectionToken token)
        {
            _helper = helper;
            _token = token;
            _jobId = jobId;
        }

        class ListSorter : IComparer<IntPair>
        {
            public int Compare(KeyValuePair<int, int> x, KeyValuePair<int, int> y)
            {
                return x.Value - y.Value;
            }
        }

        internal void Init()
        {
            List<IntPair> list;

            _helper.RemoteServer.TaskGroup_FetchGroups(_token, _jobId, out list);
            this._rootId = list[0].Key;

            // evanc: Note that we should always try to add the first value of each key group to children, because the sql doesn't guarantee the return order of the records.
            // So it will be a random bug when sometimes the sql decide to return a non-zero child value first for a parent id.
            this._items = list.GroupBy(p => p.Key).ToDictionary(g => g.Key, g => new TaskGroup(g.Key, this._token, g.Where(p => p.Value != 0).Select(p => p.Value).ToArray(), this));

            // evanc: Parent won't be zero, but needs skip zero child.
            foreach(var group in list.GroupBy(p => p.Value).Where(g => g.Key != 0))
            {
                var parents = group.Select(p => p.Key).ToArray();
                this._items[group.Key].SetParents(parents);

            }

            if ((_helper.ServerVersion.IsV4 && !_helper.ServerVersion.IsOlderThanV4SP5QFE)
                || (_helper.ServerVersion.IsV5 && _helper.ServerVersion.IsNewerThanV5RTM))
            {
                // fetch active
                List<KeyValuePair<int, KeyValuePair<int, int>>> groupStat;
                _helper.RemoteServer.TaskGroup_FetchStatistics(_token, _jobId, out groupStat);

                foreach (var tg in groupStat)
                {
                    // completed is when active == 0b
                    this._items[tg.Key].IsCompleted = tg.Value.Key == 0;
                    this._items[tg.Key].TasksFailedOrCancelled = tg.Value.Value > 0;
                }

                foreach (var failedTg in this._items.Values.Where(i => i.TasksFailedOrCancelled).Cast<IClusterTaskGroup>())
                {
                    foreach (var tg in failedTg.ExpandSubTree(g =>
                    {
                        var children = g.GetChildren();
                        return children == null ? null : children.Where(c => !c.ParentTasksFailedOrCancelled);
                    }, g => g))
                    {
                        tg.ParentTasksFailedOrCancelled = true;
                    }
                }
            }
        }

        internal TaskGroup GetRootGroup()
        {
            return _items[_rootId];
        }

        internal TaskGroup GetGroup(int groupId)
        {
            return _items[groupId];
        }

        internal TaskGroup CreateChild(int parentId, string name)
        {
            StoreProperty[] props = { new StoreProperty(TaskGroupPropertyIds.Name, name) };

            int childId;

            _helper.RemoteServer.TaskGroup_CreateChild(_token, _jobId, parentId, props, out childId);

            TaskGroup result = new TaskGroup(childId, _token, null, this);

            _items.Add(childId, result);

            return result;
        }

        internal void AddChild(TaskGroup parent, TaskGroup child)
        {
            _helper.RemoteServer.TaskGroup_AddParent(_token, _jobId, child.Id, parent.Id);
        }

        internal void SetProps(Int32 id, StoreProperty[] props)
        {
            _helper.SetPropsOnServer(ObjectType.TaskGroup, id, props);
        }

        internal PropertyRow GetProps(Int32 id, PropertyId[] pids)
        {
            return _helper.GetPropsFromServer(ObjectType.TaskGroup, id, pids);
        }
    }


    internal class TaskGroup : IClusterTaskGroup
    {
        private int _groupId;
        private ConnectionToken _token;
        private int[] _children;
        private int[] _parents = null;
        private TaskGroupHost _owner;

        internal TaskGroup(int groupId, ConnectionToken token, int[] children, TaskGroupHost owner)
        {
            _groupId = groupId;
            _token = token;
            _children = children;
            _owner = owner;
        }

        public bool IsCompleted { get; set; }

        internal void SetParents(int[] parents)
        {
            _parents = parents;
        }

        void AddParentToArray(int id)
        {
            int newLen = 0;

            if (_parents != null)
            {
                newLen = _parents.Length;
            }

            int[] newArray = new int[newLen + 1];

            for (int i = 0; i < newLen; i++)
            {
                newArray[i] = _parents[i];
            }

            newArray[newLen] = id;

            _parents = newArray;
        }

        void AddChildIdToArray(int id)
        {
            int newLen = 0;

            if (_children != null)
            {
                newLen = _children.Length;
            }

            int[] newArray = new int[newLen + 1];

            for (int i = 0; i < newLen; i++)
            {
                newArray[i] = _children[i];
            }

            newArray[newLen] = id;

            _children = newArray;
        }

        public IClusterTaskGroup CreateChild(string name)
        {
            TaskGroup item = _owner.CreateChild(_groupId, name);

            AddChildIdToArray(item.Id);

            return item;
        }

        void AddChild(TaskGroup child)
        {
            _owner.AddChild(this, child);

            AddChildIdToArray(child.Id);
        }

        public void AddParent(IClusterTaskGroup groupParent)
        {
            // Add this child to the parent group.

            TaskGroup parent = groupParent as TaskGroup;

            parent.AddChild(this);

            AddParentToArray(groupParent.Id);
        }

        IClusterTaskGroup[] GetGroupsForArray(int[] array)
        {
            if (array == null || array.Length == 0)
            {
                return null;
            }

            IClusterTaskGroup[] result = new IClusterTaskGroup[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                result[i] = _owner.GetGroup(array[i]);
            }

            return result;
        }

        public IClusterTaskGroup[] GetChildren()
        {
            return GetGroupsForArray(_children);
        }

        public IClusterTaskGroup[] GetParents()
        {
            return GetGroupsForArray(_parents);
        }

        /// <summary>
        /// This method is added to help to improve the performance of the CalkTaskGroupLevel method in JobResource 
        /// It replaces the GetParents() method.
        /// There is no worry about the backward compatibility, since this is a local function.
        /// </summary>
        /// <returns></returns>
        public int[] GetParentIds()
        {
            return _parents;
        }

        public int Id
        {
            get { return _groupId; }
        }

        public PropertyRow GetProps(params PropertyId[] propertyIds)
        {
            return _owner.GetProps(_groupId, propertyIds);
        }

        public PropertyRow GetPropsByName(params string[] propertyNames)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public PropertyRow GetAllProps()
        {
            return GetProps();
        }

        public void SetProps(params StoreProperty[] properties)
        {
            _owner.SetProps(_groupId, properties);
        }

        public void PersistToXml(System.Xml.XmlWriter writer, XmlExportOptions flags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RestoreFromXml(System.Xml.XmlReader reader, XmlImportOptions flags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool ParentTasksFailedOrCancelled { get; set; }
        public bool TasksFailedOrCancelled { get; set; }
    }
}
