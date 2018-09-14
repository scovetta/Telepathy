using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    internal class LocalRowEnumerator : IRowEnumerator, IRemoteDisposable
    {
        PropertyId[]        _pids = null;
        SortProperty[]      _sort = null;
        FilterProperty[]    _filter = null;

        FilterProperty[]    _staticFilter = null;

        int                 _remoteId = -1;
    
        SchedulerStoreSvc   _owner;
        ObjectType          _type;
        PropertyId          _pidObject;
        PropertyId          _pidParent = null;

        int                 _parentId = 0;

        WeakReference       _enumPoolEntry = null;

        internal LocalRowEnumerator(SchedulerStoreSvc owner, ObjectType type, PropertyId pidObject)
        {
            _owner = owner;
            _type = type;
            _pidObject = pidObject;            
        }
        
        internal LocalRowEnumerator(SchedulerStoreSvc owner, ObjectType type, PropertyId pidObject, int parentId)
        {
            _owner = owner;
            _type = type;
            _pidObject = pidObject;
            
            _parentId = parentId;
            
            if (type == ObjectType.Task)
            {
                _staticFilter = new FilterProperty[] 
                    { 
                        new FilterProperty(FilterOperator.Equal, TaskPropertyIds.ParentJobId, _parentId),
                        new FilterProperty(FilterOperator.GreaterThanOrEqual, TaskPropertyIds.InstanceId, 0)
                    };
            }
            else if (type == ObjectType.Allocation)
            {
                _staticFilter = new FilterProperty[] 
                    { 
                        new FilterProperty(FilterOperator.Equal, AllocationProperties.JobId, _parentId),
                    };
            }
            else if (type == ObjectType.JobMessage)
            {
                _staticFilter = LocalJobMessageRowset.GetStaticFilter(_parentId);
            }
        }

        internal LocalRowEnumerator(SchedulerStoreSvc owner, ObjectType type, PropertyId pidObject, int parentId, TaskRowSetOptions options)
        {
            _owner = owner;
            _type = type;
            _pidObject = pidObject;

            _parentId = parentId;

            if (type != ObjectType.Task)
            {
                throw new InvalidOperationException("Cannot use this constructor to open object types other than Task");
            }

            if (options == TaskRowSetOptions.NoParametricExpansion)
            {
                _staticFilter = new FilterProperty[] 
                    { 
                        new FilterProperty(FilterOperator.Equal, TaskPropertyIds.ParentJobId, _parentId),
                        new FilterProperty(FilterOperator.LessThanOrEqual, TaskPropertyIds.InstanceId, 0)
                    };
            }
            else if(options == TaskRowSetOptions.None)
            {
                _staticFilter = new FilterProperty[] 
                    { 
                        new FilterProperty(FilterOperator.Equal, TaskPropertyIds.ParentJobId, _parentId),
                        new FilterProperty(FilterOperator.GreaterThanOrEqual, TaskPropertyIds.InstanceId, 0)
                    };
            }
            else if (options == TaskRowSetOptions.ParametricMasterOnly)
            {
                _staticFilter = new FilterProperty[] 
                    { 
                        new FilterProperty(FilterOperator.Equal, TaskPropertyIds.ParentJobId, _parentId),
                        new FilterProperty(FilterOperator.LessThan, TaskPropertyIds.InstanceId, 0)
                    };
            }
            else if (options == TaskRowSetOptions.FullParametric)
            {
                _staticFilter = new FilterProperty[] 
                    { 
                        new FilterProperty(FilterOperator.Equal, TaskPropertyIds.ParentJobId, _parentId),
                    };                
            }
            else
            {
                Debug.Assert(false);
            }
        }

        internal LocalRowEnumerator(SchedulerStoreSvc owner, ObjectType type, PropertyId pidObject, PropertyId pidParent, TaskRowSetOptions option)
        {
            _owner = owner;
            _type = type;
            _pidObject = pidObject;            

            _pidParent = pidParent;

            if (type == ObjectType.Task)
            {
                if (option == TaskRowSetOptions.NoParametricExpansion)
                {
                    _staticFilter = new FilterProperty[] 
                    { 
                        new FilterProperty(FilterOperator.LessThanOrEqual, TaskPropertyIds.InstanceId, 0)
                    };
                }
                else
                {
                    _staticFilter = new FilterProperty[] 
                    { 
                        new FilterProperty(FilterOperator.GreaterThanOrEqual, TaskPropertyIds.InstanceId, 0)
                    };
                }
            }
        }

        ~LocalRowEnumerator()
        {
            this.Dispose(false);
        }

        internal void Touch()
        {
            if (_remoteId != -1 && !_isDisposed)
            {
                _owner.ServerWrapper.RowEnum_Touch(_remoteId);
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
                if (_enumPoolEntry != null)
                {
                    try
                    {
                        _owner.UnRegisterRowEnum(_enumPoolEntry);
                    }
                    catch { }
                }

                // The helper could already be disconnected, and server wrapper could be null
                try
                {
                    _owner.InvokeRemoteDispose(this);
                }
                catch
                {
                }
            }

            _isDisposed = true;
        }

        internal bool Disposed
        {
            get { return _isDisposed; }
        }

        public void RemoteDispose()
        {
            CloseRemote();
        }

        void CloseRemote()
        {
            if (_remoteId != -1)
            {
                StoreServer server = _owner.ServerWrapper;
                if (server != null)
                {
                    try
                    {
                        server.RowEnum_Close(_remoteId);
                    }
                    catch
                    {
                        // Swallow it
                    }
                }
                _remoteId = -1;
            }
        }

        FilterProperty[] GetCombinedFilter()
        {
            FilterProperty[] filter = null;

            if (_staticFilter != null)
            {
                List<FilterProperty> temp = new List<FilterProperty>();

                temp.AddRange(_staticFilter);

                if (_filter != null)
                {
                    temp.AddRange(_filter);
                }

                filter = temp.ToArray();
            }
            else
            {
                filter = _filter;
            }

            FilterProperty[] remoteFilter = filter;
            if (_owner.RequiresPropertyConversion)
            {
                PropertyLookup.FilterProps_Convert(_owner.GetPropertyConverters(), filter, out remoteFilter);
            }

            return remoteFilter;
        }

        SortProperty[] GetSort()
        {
            SortProperty[] remoteSort = _sort;
            if (_owner.RequiresPropertyConversion)
            {
                PropertyLookup.SortProps_Convert(_owner.GetPropertyConverters(), _sort, out remoteSort);
            }
            return remoteSort;
        }

        void Init()
        {
            if (_remoteId == -1)
            {
                RowSetResult result = _owner.ServerWrapper.RowEnum_Open(_type, 0, _pids, GetCombinedFilter(), GetSort());
                
                _remoteId = result.RowSetId;
                
                _enumPoolEntry = _owner.RegisterRowEnum(this);
            }
        }

        int _indexId = -1;
        int _indexObject = -1;
        int _indexObjectParentId = -1;

        PropertyConversionMap _conversionMap = null;

        public void SetColumns(params PropertyId[] columnIds)
        {
            CloseRemote();

            _indexId = -1;
            _indexObject = -1;
            _indexObjectParentId = -1;
            _conversionMap = null;            

            List<PropertyId> pids = new List<PropertyId>();

            if (columnIds != null)
            {
                pids.AddRange(columnIds);

                for (int i = 0; i < columnIds.Length; i++)
                {
                    if (columnIds[i] == StorePropertyIds.Id)
                    {
                        _indexId = i;
                    }
                    else if (columnIds[i] == _pidObject)
                    {
                        _indexObject = i;
                    }
                    else if (columnIds[i] == _pidParent)
                    {
                        _indexObjectParentId = i;
                    }
                }
                
            }

            // Check to see if we need to add any additional 
            // properties to the enumeration.

            if (_indexObject != -1 && _indexId == -1)
            {
                pids.Add(StorePropertyIds.Id);
                
                _indexId = pids.Count - 1;
            }
            
            if (_pidParent != null && _indexObjectParentId == -1)
            {
                pids.Add(_pidParent);
                
                _indexObjectParentId = pids.Count - 1;
            }

            _pids = pids.ToArray();

            if (_owner.RequiresPropertyConversion)
            {
                PropertyId[] convertedPids;
                PropertyLookup.PreGetProps_Convert(_owner.GetPropertyConverters(), _pids, out convertedPids, out _conversionMap);
                _pids = convertedPids;
            }
        }

        public void SetFilter(params FilterProperty[] filter)
        {
            CloseRemote();

            _filter = filter;
        }

        public void SetSortOrder(params SortProperty[] sort)
        {
            CloseRemote();

            _sort = sort;
        }

        object ObjectForId(int idParent, int id)
        {
            switch (_type)
            {
                case ObjectType.Resource:
                    return new ResourceEx(_owner.RemoteServer, _owner.Token, id, _owner);
                    
                case ObjectType.Job:
                    return new JobEx(id, _owner.Token, _owner, null);
                
                case ObjectType.Task:
                    return new TaskEx(idParent, id, _owner);

                case ObjectType.Node:
                    return new NodeEx(id, _owner);

                case ObjectType.JobTemplate:
                    return new JobProfile(_owner, _owner.Token, id);

                case ObjectType.Allocation:
                    return new AllocationObject(_owner, id);

                case ObjectType.JobMessage:
                    return new JobMessageObject(_owner, id, idParent);
                
                default:
                    return null;
            }
        }

        public PropertyRowSet GetRows2(int numberOfRows)
        {
            Init();

            PropertyRowSet result = _owner.ServerWrapper.RowEnum_GetRows(_remoteId, numberOfRows);

            if (_indexObject != -1 || _conversionMap != null)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (_indexObject != -1)
                    {
                        int parentId = _parentId;
                        
                        if (_indexObjectParentId != -1)
                        {
                            parentId = (int) result[i][_indexObjectParentId].Value;
                        }
                        
                        result[i][_indexObject] = new StoreProperty(_pidObject, ObjectForId(parentId, (int)result[i][_indexId].Value));
                    }

                    if (_conversionMap != null)
                    {
                        PropertyLookup.PostGetProps_Deconvert(_conversionMap, result[i].Props);
                    }
                }
            }

            return result;
        }

        public void Reset()
        {
            CloseRemote();
        }

        public void SetProperties(params StoreProperty[] properties)
        {
            StoreProperty[] serverProps = properties;            
            
            if (_owner.RequiresPropertyConversion)
            {
                PropertyLookup.SetProps_Convert(_owner.GetPropertyConverters(), properties, out serverProps);
            }

            if (!_owner.ServerVersion.IsV2 && _owner.StoreInProc)
            {
                // This is a quick version for using internally inside the scheduler

                try
                {
                    _owner.ServerWrapper.RowEnum_SetProps(_type, serverProps, GetCombinedFilter());
                    return;
                }
                catch (SchedulerException e)
                {
                    // If it is permission denied, give it another chance to use the 
                    // slower way to set props
                    if (e.Code != ErrorCode.Operation_PermissionDenied)
                    {
                        throw;
                    }
                }
            }

            // Clients have to use this version, which is slower but security is ensured

            using (IClusterStoreTransaction trns =  _owner.BeginTransaction())
            {
                SetColumns(
                    StorePropertyIds.Id,
                    _pidObject
                    );
                    
                foreach (PropertyRow row in this)
                {
                    IClusterStoreObject item = (IClusterStoreObject) row[1].Value;
                    
                    item.SetProps(serverProps);
                }
                
                trns.Commit();
            }
        }

        public void SetOptions(int options)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IEnumerator<PropertyRow> GetEnumerator()
        {
            return new RowEnumeratorEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }


    public class RowEnumeratorEnumerator : IEnumerator<PropertyRow>
    {
        internal RowEnumeratorEnumerator(LocalRowEnumerator owner)
        {
            _owner = owner;
        }

        LocalRowEnumerator  _owner;
        PropertyRow[]       _cacheRows = null;
        int                 _index = 0;

        public void Dispose()
        {
            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public PropertyRow Current
        {
            get
            {
                Debug.Assert(_cacheRows != null);
                return _cacheRows[_index];
            }
        }

        public bool MoveNext()
        {
            if (_cacheRows != null)
            {
                ++_index;

                if (_index < _cacheRows.GetLength(0))
                {
                    return true;
                }
            }

            PropertyRowSet result = _owner.GetRows2(128);
            
            _cacheRows = result.Rows;

            if (_cacheRows == null || _cacheRows.GetLength(0) == 0)
            {
                return false;
            }

            _index = 0;

            return true;
        }

        public void Reset()
        {
            _owner.Reset();
            _cacheRows = null;
            _index = 0;
        }
    }

}
