using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Remoting;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler.Store
{
    internal enum SeekMethod
    {
        Begin,
        Current,
        End
    }

    internal class LocalJobRowSet : LocalRowSet, IJobRowSet
    {
        public LocalJobRowSet(SchedulerStoreSvc helper, RowSetType type)
            : base (ObjectType.Job, helper, type, JobPropertyIds.Id, StorePropertyIds.JobObject)
        {
        }


    }

    internal class LocalTaskRowSet : LocalRowSet, ITaskRowSet
    {
        int _jobId = 0;

        public LocalTaskRowSet(SchedulerStoreSvc helper, RowSetType type, Int32 jobId, TaskRowSetOptions options)
            : base (ObjectType.Task, helper, type, TaskPropertyIds.Id, TaskPropertyIds.TaskObject, TaskPropertyIds.ParentJobId, GetStaticFilter(jobId, options))
        {
            _jobId = jobId;            
        }

        internal override object GetObjectForId(int id, int idParent)
        {
            // In the case of job task rowset, we will not get the parent id back.

            if (idParent == 0)
            {
                idParent = _jobId;
            }
            
            return new TaskEx(idParent, id, _helper);
        }
        
        static FilterProperty[] GetStaticFilter(int jobId, TaskRowSetOptions options)
        {
            // Update the static filter.  Note 
            // that this can be done safely here 
            // since the options cannot be set
            // after any other operations on the
            // rowset (i.e. Read, Filter, Sort).

            List<FilterProperty> filterList = new List<FilterProperty>();
            if (jobId != 0)
            {
                filterList.Add(new FilterProperty(FilterOperator.Equal, TaskPropertyIds.ParentJobId, jobId));
            }
        
            if (options == TaskRowSetOptions.None)
            {
                filterList.Add(new FilterProperty(FilterOperator.GreaterThanOrEqual, TaskPropertyIds.InstanceId, 0));
            }
            else if (options == TaskRowSetOptions.NoParametricExpansion)
            {
                filterList.Add(new FilterProperty(FilterOperator.LessThanOrEqual, TaskPropertyIds.InstanceId, 0));
            }
            else if (options == TaskRowSetOptions.ParametricMasterOnly)
            {
                filterList.Add(new FilterProperty(FilterOperator.LessThan, TaskPropertyIds.InstanceId, 0));
            }
            else if (options == TaskRowSetOptions.FullParametric)
            {
                // No instanceId filter , display both master, normal and subtasks
            }
            else
            {
                Debug.Assert(false);
            }

            return filterList.ToArray();
        }
    }

    internal class LocalResourceRowSet : LocalRowSet, IResourceRowSet
    {
        public LocalResourceRowSet(SchedulerStoreSvc helper, RowSetType type)
            : base (ObjectType.Resource, helper, type, ResourcePropertyIds.Id, ResourcePropertyIds.ResourceObject)
        {
        }

    }

    internal class LocalNodeRowSet : LocalRowSet, INodeRowSet
    {
        public LocalNodeRowSet(SchedulerStoreSvc helper, RowSetType type)
            : base(ObjectType.Node, helper, type, NodePropertyIds.Id, NodePropertyIds.NodeObject)
        {
        }

    }

    internal class LocalProfileRowSet : LocalRowSet, IJobProfileRowSet
    {
        public LocalProfileRowSet(SchedulerStoreSvc helper, RowSetType type)
            : base(ObjectType.JobTemplate, helper, type, JobTemplatePropertyIds.Id, JobTemplatePropertyIds.TemplateObject)
        {
        }
    }

    internal class LocalPoolRowSet : LocalRowSet, IPoolRowSet
    {
        public LocalPoolRowSet(SchedulerStoreSvc helper, RowSetType type)
            : base(ObjectType.Pool, helper,type,PoolPropertyIds.Id,PoolPropertyIds.PoolObject)
        {
        }
    }

    internal class LocalTaskGroupRowSet : LocalRowSet, ITaskGroupRowSet
    {
        public LocalTaskGroupRowSet(SchedulerStoreSvc helper, RowSetType type, int jobId)
            : base(ObjectType.TaskGroup, helper, type, PoolPropertyIds.Id, null, TaskGroupPropertyIds.JobId, GetStaticFilter(jobId))
        {
        }

        static FilterProperty[] GetStaticFilter(int jobId)
        {
            // Update the static filter.  Note 
            // that this can be done safely here 
            // since the options cannot be set
            // after any other operations on the
            // rowset (i.e. Read, Filter, Sort).

            List<FilterProperty> filterList = new List<FilterProperty>();
            if (jobId != 0)
            {
                filterList.Add(new FilterProperty(FilterOperator.Equal, TaskGroupPropertyIds.JobId, jobId));
            }

            return filterList.ToArray();
        }
    }

    internal class LocalAllocationRowSet : LocalRowSet, IAllocationRowSet
    {
        public LocalAllocationRowSet(SchedulerStoreSvc helper, RowSetType type, Int32 jobId, Int32 taskId)
            : base(ObjectType.Allocation, helper, type, AllocationProperties.Id, AllocationProperties.AllocationObject, null, GetStaticFilter(jobId, taskId))
        {
        }

        internal static FilterProperty[] GetStaticFilter(int jobId, int taskId)
        {
            // Add a static filter to the rowset so that only
            // allocation records for the job or task are returned.
            List<FilterProperty> filter = new List<FilterProperty>();            
            if (jobId > 0)
            {
                filter.Add(new FilterProperty(FilterOperator.Equal, AllocationProperties.JobId, jobId));
            }            
            if (taskId > 0)
            {
                filter.Add(new FilterProperty(FilterOperator.Equal, AllocationProperties.TaskId, taskId));
            }
            if (filter.Count > 0)
            {
                return filter.ToArray();
            }
            return null;
        }
    }

    internal class LocalJobMessageRowset : LocalRowSet, IJobMessageRowSet
    {
        int _jobId = 0;

        public LocalJobMessageRowset(SchedulerStoreSvc helper, RowSetType type, Int32 jobId)
            : base(ObjectType.JobMessage, helper, type, JobMessagePropertyIds.Id, JobMessagePropertyIds.JobMessageObject,
                   JobMessagePropertyIds.JobId, GetStaticFilter(jobId))
        {
            _jobId = jobId;
        }

        internal override object GetObjectForId(int id, int idParent)
        {
            // This can happen when we are retrieving data from a paused rowset: 
            // parent IDs are not transmitted, and set to be zero.
            return new JobMessageObject(_helper, id, (idParent != 0 ? idParent : _jobId));
        }

        internal static FilterProperty[] GetStaticFilter(int jobId)
        {                        
            return new FilterProperty[]
            {
                new FilterProperty(FilterOperator.Equal, JobMessagePropertyIds.JobId, jobId),
                new FilterProperty(FilterOperator.GreaterThan, JobMessagePropertyIds.MessageCount, 0),
            };
        }
    }


    internal abstract class LocalRowSet : IRowSetEx
    {
        protected SchedulerStoreSvc         _helper;
        protected RowSetType                _type;
        
        int _indexObject = -1;
        int _indexObjectId = -1;
        int _indexObjectParentId = -1;

        PropertyConversionMap _conversionMap = null;

        protected RowSetWrapper _remoterowset;
        protected PropertyId    _pidObject;
        protected PropertyId    _pidId;
        protected PropertyId    _pidParentId;

        protected ObjectType    _objectType;

        DateTime _lastTouched = DateTime.UtcNow;

        ReaderWriterLockSlim _cs = new ReaderWriterLockSlim();
        
        internal DateTime LastTouched
        {
            get { return _lastTouched; }
        }
        
        internal void Touch()
        {
            if (!_isDisposed && _remoterowset != null)
            {
                _lastTouched = DateTime.UtcNow;
                _remoterowset.Touch();
            }
        }

        internal SchedulerStoreSvc Helper
        {
            get { return _helper; }
        }

        internal ObjectType ObjectType
        {
            get { return _objectType; }
        }

        internal RowSetType RowsetType
        {
            get { return _type; }
        }

        protected LocalRowSet(ObjectType obType, SchedulerStoreSvc helper, RowSetType type, PropertyId pidId, PropertyId pidObject)
            : this(obType, helper, type, pidId, pidObject, null, null)
        {
        }

        protected LocalRowSet(ObjectType obType, SchedulerStoreSvc helper, RowSetType type, PropertyId pidId,
                              PropertyId pidObject, PropertyId pidParentId, FilterProperty[] staticFilter)
        {
            _objectType = obType;

            _helper = helper;
            _type = type;

            _pidId = pidId;
            _pidObject = pidObject;
            _pidParentId = pidParentId;

            if (_type == RowSetType.Dynamic)
            {
                _helper.RegisterRowSet(this);
            }

            _remoterowset = new RowSetWrapper(this, staticFilter);

            _remoterowset.SetType(_type);
        }

        ~LocalRowSet()
        {
            if (!_isDisposed)
            {
                Dispose(false);
            }
        }


        bool _isDisposed = false;                

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                try
                {
                    _helper.UnRegisterRowSet(this);
                }
                catch { }

                // The helper could already be disconnected, and server wrapper could be null
                try
                {
                    _helper.InvokeRemoteDispose(_remoterowset);
                }
                catch
                {
                }
            }

            _remoterowset = null;
            _isDisposed = true;
        }


        internal bool Disposed
        {
            get
            {
                return _isDisposed;
            }
        }
        
        internal int GetGlobalId()
        {
            if (!_isDisposed && _remoterowset != null)
            {
                return _remoterowset.GetGlobalId();
            }
            return -1;
        }

        internal virtual object GetObjectForId(int id, int idParent)
        {
            switch (_objectType)
            {
                case ObjectType.Job:
                    return new JobEx(id, _helper.Token, _helper, null);
                
                case ObjectType.Resource:
                    return new ResourceEx(_helper.RemoteServer, _helper.Token, id, _helper);

                case ObjectType.Node:
                    return new NodeEx(id, _helper);
                    
                case ObjectType.Allocation:
                    return new AllocationObject(_helper, id);

                case ObjectType.JobTemplate:
                    return new JobProfile(_helper, _helper.Token, id);

                case ObjectType.Pool:
                    return new PoolEx(_helper, id);
                default:
                    break;
            }
            
            Debug.Assert(false);
            
            return null;
        }

        public int GetObjectIndex(int itemId)
        {
            using (new ReaderLockScope(_cs))
            {
                return _remoterowset.GetObjectIndex(itemId);
            }
        }


        internal void OnChangeNotifyFromServer(int rowCount, int objectId, int objectIndex, int objectPreviousIndex, EventType eventType, StoreProperty[] props)
        {
            using (new ReaderLockScope(_cs))
            {
                _remoterowset.UpdateRowCount(rowCount);

                // Translate the properties returned from the server back into client-side properties
                if (_conversionMap != null && props != null)
                {
                    // Since only a sub-set of the properties may have been modified, the existing map is not
                    // valid for the property array.  We need to construct a new map for the properties we received.
                    // Even for Create events, don't assume that the list of properties matches the existing conversion map.
                    PropertyConversionMap modifiedPropsMap;
                    StoreProperty[] serverProps = props;
                    PropertyLookup.PostGetProps_Deconvert(_conversionMap.GetMappedConverters(), serverProps, out props, out modifiedPropsMap);
                }

                if (_changehandlers != null)
                {
                    _changehandlers.Invoke(rowCount, objectId, objectIndex, objectPreviousIndex, eventType, props);
                }
            }
        }

        protected event OnRowSetChange _changehandlers;

        public void RegisterHandler(OnRowSetChange handler)
        {
            _changehandlers += handler;
        }

        public void UnRegisterHandler(OnRowSetChange handler)
        {
            _changehandlers -= handler;
        }

        public void SetColumns(params PropertyId[] columnIds)
        {
            using (new WriterLockScope(_cs))
            {
                // Check to see if a request is being made
                // for a Job Object

                _indexObject = -1;
                _indexObjectId = -1;
                _indexObjectParentId = -1;

                _conversionMap = null;

                for (int i = 0; i < columnIds.GetLength(0); i++)
                {
                    if (columnIds[i] == _pidObject)
                    {
                        _indexObject = i;
                    }
                    else if (columnIds[i] == _pidId)
                    {
                        _indexObjectId = i;
                    }
                    else if (_pidParentId != null && columnIds[i] == _pidParentId)
                    {
                        _indexObjectParentId = i;
                    }
                }

                // Make sure that we have the properties needed
                // to initialize the object coming back from the
                // server.

                List<PropertyId> ids = new List<PropertyId>(columnIds);

                // First make sure that the object Id is being requested.
                // If not add it.

                if (_indexObjectId == -1)
                {
                    if (_indexObject != -1)
                    {
                        _indexObjectId = _indexObject;
                        ids[_indexObject] = _pidId;
                    }
                    else
                    {
                        ids.Add(_pidId);
                        _indexObjectId = ids.Count - 1;
                    }
                }

                // If the object requires that a parent id be fetched
                // check to see if it is.  If not add it to the query.

                if (_pidParentId != null && _indexObjectParentId == -1)
                {
                    // Add the object parent id to the query

                    ids.Add(_pidParentId);

                    _indexObjectParentId = ids.Count - 1;
                }

                PropertyId[] remoteIds = ids.ToArray();
                if (_helper.RequiresPropertyConversion)
                {
                    PropertyId[] convertedIds;
                    PropertyLookup.PreGetProps_Convert(_helper.GetPropertyConverters(), remoteIds, out convertedIds, out _conversionMap);
                    remoteIds = convertedIds;
                }

                _remoterowset.SetColumns(remoteIds);
            }            
        }

        public void SetFrozenColumns(PropertyId[] columnIds)
        {
            using (new WriterLockScope(_cs))
            {
                PropertyId[] remoteIds = columnIds;
                if (_helper.RequiresPropertyConversion)
                {
                    PropertyId[] convertedIds;
                    PropertyConversionMap dummyMap;
                    PropertyLookup.PreGetProps_Convert(_helper.GetPropertyConverters(), remoteIds, out convertedIds, out dummyMap);
                    remoteIds = convertedIds;
                }

                _remoterowset.SetFrozenColumns(remoteIds);
            }            
        }

        public void SetFilter(FilterProperty[] filter)
        {
            // Validate the property types within the filter.
            
            PropertyLookup.ValidateFilterTypes(filter);

            using (new WriterLockScope(_cs))
            {
                FilterProperty[] remoteFilter = filter;
                if (_helper.RequiresPropertyConversion)
                {
                    PropertyLookup.FilterProps_Convert(_helper.GetPropertyConverters(), filter, out remoteFilter);
                }

                _remoterowset.SetFilter(remoteFilter);
            }
        }

        public void SetSortOrder(SortProperty[] sort)
        {
            using (new WriterLockScope(_cs))
            {
                SortProperty[] remoteSort = sort;
                if (_helper.RequiresPropertyConversion)
                {
                    PropertyLookup.SortProps_Convert(_helper.GetPropertyConverters(), sort, out remoteSort);
                }

                _remoterowset.SetSortOrder(remoteSort);
            }
        }

        public void SetTop(int top)
        {
            if (_type == RowSetType.Dynamic)
            {
                throw new InvalidOperationException("Can not specify top rows to a dynamic rowset.");
            }

            using (new WriterLockScope(_cs))
            {            
                _remoterowset.SetTop(top);
            }
        }

        public void Invalidate()
        {
            using (new WriterLockScope(_cs))
            {
                _remoterowset.Invalidate();
            }
        }

        public int GetCount()
        {
            using (new ReaderLockScope(_cs))
            {
                return _remoterowset.GetCount();
            }
        }

        public int Seek(SeekMethod method, int arg)
        {
            // Only one seek can be performed at a time, but other reads can be performed at the same time as the seek
            using (new UpgradeableLockScope(_cs))
            {
                return _remoterowset.Seek(method, arg);
            }
        }

        public int GetCurrentRow()
        {
            using (new ReaderLockScope(_cs))
            {
                return _remoterowset.GetCurrentRow();
            }
        }

        public PropertyRow[] GetRows(int numberOfRowsRequested)
        {
            // This is a seek-like operation, so only one of these may be performed at a time (but other reads can be performed simultaneously)
            using (new UpgradeableLockScope(_cs))
            {
                return ProcessRemoteRows(_remoterowset.GetRows(numberOfRowsRequested));                                
            }
        }

        PropertyRow[] GetPausedData(int firstRow, int lastRow, out int rowCount)
        {
            // Must be called from inside the critical section
            Debug.Assert(_cs.IsReadLockHeld || _cs.IsUpgradeableReadLockHeld);

            PropertyRow[] rows = _remoterowset.GetData(firstRow, lastRow, false, out rowCount);

            if (rows != null)
            {
                int objIdx = -1;
                for (int i = 0; i < rows.Length; i++)
                {
                    if (objIdx == -1)
                    {
                        // Find out the entry with pid = Error, fill the obj in this entry
                        for (int j = 0; j < rows[i].Props.Length; j++)
                        {
                            if (rows[i][j].Id == StorePropertyIds.Error)
                            {
                                objIdx = j;
                                break;
                            }
                        }
                        Debug.Assert(objIdx != -1);
                    }
                    rows[i][objIdx] = new StoreProperty(_pidObject, GetObjectForId((int)rows[i][0].Value, 0));
                }
            }

            return rows;
        }

        public PropertyRow[] GetRows(int firstRow, int lastRow)
        {
            int rowCount;            
            return GetRows(firstRow, lastRow, out rowCount);
        }        

        public PropertyRow[] GetRows(int firstRow, int lastRow, bool defineBoundary, out int rowCount)
        {
            using (new ReaderLockScope(_cs))
            {
                if (_fPaused)
                {
                    return GetPausedData(firstRow, lastRow, out rowCount);
                }
                return ProcessRemoteRows(_remoterowset.GetData(firstRow, lastRow, defineBoundary, out rowCount));
            }
        }

        public PropertyRow[] GetRows(int firstRow, int lastRow, out int rowCount)
        {
            return GetRows(firstRow, lastRow, false, out rowCount);
        }


        PropertyRow[] ProcessRemoteRows(PropertyRow[] rows)
        {
            // Must be called from inside the critical section
            Debug.Assert(_cs.IsReadLockHeld || _cs.IsUpgradeableReadLockHeld);

            if (_indexObject != -1 && rows != null)
            {
                foreach (PropertyRow row in rows)
                {
                    Debug.Assert(row[_indexObjectId].Id == _pidId);

                    int parentId = 0;

                    if (_indexObjectParentId != -1)
                    {
                        Debug.Assert(row[_indexObjectParentId].Id == _pidParentId);
                        parentId = (int)row[_indexObjectParentId].Value;
                    }

                    object storeObject = GetObjectForId((int)row[_indexObjectId].Value, parentId);
                    row[_indexObject].Id = _pidObject;
                    row[_indexObject].Value = storeObject;
                }
            }

            // Check if any properties need to be converted after being retrieved from the server
            if (_conversionMap != null && rows != null)
            {
                foreach (PropertyRow row in rows)
                {
                    PropertyLookup.PostGetProps_Deconvert(_conversionMap, row.Props);
                }
            }

            return rows;
        }


        public void SetAggregateColumns(params AggregateColumn[] columns)
        {
            using (new WriterLockScope(_cs))
            {
                _remoterowset.SetAggregateColumns(columns);
            }
        }

        public void SetGroupBy(PropertyId[] groupBy)
        {
            using (new WriterLockScope(_cs))
            {
                _remoterowset.SetGroupBy(groupBy);
            }
        }

        bool _fPaused = false;

        public void Pause()
        {
            if (_type != RowSetType.Dynamic)
            {
                throw new InvalidOperationException("Only dynamic rowsets may be paused");
            }

            using (new WriterLockScope(_cs))
            {
                if (!_fPaused)
                {
                    _remoterowset.Pause();
                    _fPaused = true;
                }
            }
        }
        
        public void Resume()
        {
            if (_type != RowSetType.Dynamic)
            {
                throw new InvalidOperationException("Only dynamic rowsets may be resumed");
            }

            using (new WriterLockScope(_cs))
            {
                if (_fPaused)
                {
                    _remoterowset.Resume();
                    _fPaused = false;
                }
            }
        }
       

        public IEnumerator<PropertyRow> GetEnumerator()
        {
            if (_type != RowSetType.Snapshot &&
                _type != RowSetType.SnapshotWithCustomProps)
            {
                throw new InvalidOperationException("Only Snapshot enumerations can use IEnumerable");
            }
                        
            return new RowSetEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
