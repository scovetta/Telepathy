using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Threading;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    internal class RowSetWrapper : IRemoteDisposable
    {
        LocalRowSet     _owner  = null;
        int             _globalId = -1;

        FilterProperty[]    _staticFilter = null;
        
        PropertyId[]        _columnIds = null;
        FilterProperty[]    _filter = null;
        SortProperty[]      _sort = null;
        AggregateColumn[]   _aggregate = null;
        PropertyId[]        _groupby = null;
        RowSetType          _type = RowSetType.Snapshot;
        int                 _options = 0;
        int                 _top = -1;
        PropertyId[]        _frozenColumnIds = null;

        int                 _eventId = -1;

        object _openclose_lock = new object();

        internal RowSetWrapper(LocalRowSet owner, FilterProperty[] staticFilter)
        {
            _owner = owner;
            _staticFilter = staticFilter;
            _filter = _staticFilter;
        }

        public void RemoteDispose()
        {
            CloseRemoteRowSet();
        }
        
        void CloseRemoteRowSet()
        {
            lock (_openclose_lock)
            {
                int currentGlobalId = Interlocked.Exchange(ref _globalId, -1);
                if (currentGlobalId != -1)
                {
                    try
                    {
                        StoreServer server = _owner.Helper.ServerWrapper;
                        if (server != null)
                        {
                            if (_eventId != -1)
                            {
                                try
                                {
                                    server.UnRegisterForEvent(_eventId);
                                }
                                catch
                                {
                                    // Swallow
                                }
                            }
                            try
                            {
                                server.RowSet_CloseRowSet(currentGlobalId);
                            }
                            catch
                            {
                                // Swallow
                            }
                        }                        
                    }
                    catch
                    {
                        // Ignore network exceptions in this case.
                    }
                    _eventId = -1;
                }
            }
        }
        
        void OpenRemoteRowSet()
        {
            lock (_openclose_lock)
            {
                if (_globalId == -1)
                {
                    RowSetResult result = _owner.Helper.ServerWrapper.RowSet_OpenRowSet(
                            _owner.ObjectType,
                            _type,
                            _options,
                            _columnIds,
                            _filter,
                            _sort,
                            _aggregate,
                            _groupby,
                            _frozenColumnIds,
                            _top);

                    if (result.Code == ErrorCode.Operation_PermissionDenied)
                    {
                        throw new SchedulerException(ErrorCode.Operation_PermissionDenied, "");
                    }

                    _globalId = result.RowSetId;
                    _rowCount = result.TotalRowCount;

                    if (_type == RowSetType.Dynamic)
                    {
                        try
                        {
                            _eventId = _owner.Helper.ServerWrapper.RegisterForEvent(
                                 Packets.EventObjectClass.Rowset,
                                 _globalId,
                                 0,
                                 false);
                        }
                        catch { }
                    }
                }
            }
        }

        internal void UpdateRowCount(int rowCount)
        {
            _rowCount = rowCount;
        }

        internal void Touch()
        {
            if (_globalId != -1)
            {
                _owner.Helper.ServerWrapper.RowSet_TouchRowSet(_globalId);
            }
        }

        internal void Invalidate()
        {
            CloseRemoteRowSet();
        }

        public void SetType(RowSetType type)
        {
            CloseRemoteRowSet();
            
            _type = type;
        }

        public void SetColumns(params PropertyId[] columnIds)
        {
            CloseRemoteRowSet();

            _columnIds = columnIds;
            
            _propIndexMap.Clear();

            if (_columnIds != null)
            {
                for (int i = 0; i < columnIds.Length; i++)
                {
                    _propIndexMap[columnIds[i]] = i;
                }
            }
        }

        public void SetFrozenColumns(PropertyId[] ids)
        {
            CloseRemoteRowSet();

            _frozenColumnIds = ids;
        }


        Dictionary<PropertyId, int> _propIndexMap = new Dictionary<PropertyId,int>();

        public void SetAggregateColumns(params AggregateColumn[] columns)
        {
            CloseRemoteRowSet();

            _aggregate = columns;
        }

        public void SetGroupBy(params PropertyId[] groupBy)
        {
            CloseRemoteRowSet();

            _groupby = groupBy;
        }

        public int SetFilter(params FilterProperty[] filter)
        {
            CloseRemoteRowSet();

            if (_staticFilter != null)
            {
                if (filter != null)
                {
                    List<FilterProperty> newFilter = new List<FilterProperty>(_staticFilter);

                    newFilter.AddRange(filter);

                    _filter = newFilter.ToArray();
                }
                else
                {
                    _filter = _staticFilter;
                }
            }
            else
            {
                _filter = filter;
            }

            return 0;
        }

        public int SetSortOrder(params SortProperty[] sort)
        {
            CloseRemoteRowSet();

            _sort = sort;

            return 0;        
        }

        public int SetTop(int top)
        {
            CloseRemoteRowSet();

            _top = top;

            return 0;
        }

        int _nCurrentRow = 0;
        int _rowCount = 0;

        public int GetCount()
        {
            OpenRemoteRowSet();

            return _rowCount;
        }

        public int GetGlobalId()
        {
            // This should be a local operation.  Global ID -1 means that no valid counter-part for this rowset
            // has so far been created on the server.
            return _globalId;
        }

        public int Seek(SeekMethod method, int arg)
        {
            OpenRemoteRowSet();
            
            switch (method)
            {
                case SeekMethod.Begin:
                    _nCurrentRow = arg;
                    break;
                   
                case SeekMethod.End:
                    _nCurrentRow = _rowCount - (arg + 1);
                    break;
                
                case SeekMethod.Current:
                    _nCurrentRow += arg;
                    break;
            }

            return _nCurrentRow;
        }

        public int GetCurrentRow()
        {
            return _nCurrentRow;
        }

        public int GetObjectIndex(int itemId)
        {
            OpenRemoteRowSet();
            
            return _owner.Helper.ServerWrapper.RowSet_GetObjectIndex(_globalId, itemId);
        }


        public PropertyRow[] GetRows(int numberOfRowsRequested)
        {
            int rowCount = 0;
            
            PropertyRow[] rows = GetData(_nCurrentRow, _nCurrentRow + numberOfRowsRequested - 1, false, out rowCount);

            if (rows != null)
            {
                _nCurrentRow += rows.Length;

                if (rows.Length == 0)
                {
                    return null;
                }
            }            

            return rows;
        }

        public PropertyRow[] GetData(int firstRow, int lastRow, bool defineBoundary, out int rowCount)
        {
            OpenRemoteRowSet();
            
            RowSetResult result;
            
            while (true)
            {
                result = _owner.Helper.ServerWrapper.RowSet_GetData(_globalId, firstRow, lastRow, defineBoundary);
                
                if (result.Code == 0)
                {
                    break;
                }
                
                CloseRemoteRowSet();
                OpenRemoteRowSet();
            }
            
            _rowCount = rowCount = result.TotalRowCount;

            if (result.Rows != null && _propIndexMap.Count > 0)
            {
                foreach (PropertyRow row in result.Rows)
                {
                    row.SetPropToIndexMap(_propIndexMap);
                }
            }
            
            return result.Rows;
        }

        bool _fPaused = false;

        internal void Pause()
        {
            RowSetResult result = _owner.Helper.ServerWrapper.RowSet_Freeze(_globalId);

            if (result.Code != 0)
            {
                throw new SchedulerException(SR.FailedFreeze);
            }

            // While we are paused, close the dynamic rowset on the server side, since we will be re-opening when we resume
            CloseRemoteRowSet();

            _propIndexMap.Clear();

            _globalId = result.RowSetId;
            
            _fPaused = true;
        }
        
        internal void Resume()
        {
            if (_fPaused)
            {
                // Close the "frozen" rowset on the remote side
                CloseRemoteRowSet();
                                
                _propIndexMap.Clear();

                if (_columnIds != null)
                {
                    for (int i = 0; i < _columnIds.Length; i++)
                    {
                        _propIndexMap[_columnIds[i]] = i;
                    }
                }               
            
                _fPaused = false;
            }
        }

    }
}
