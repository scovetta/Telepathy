using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public delegate void OnRowSetChange(int rowCount, Int32 objectId, int objectIndex, int objectPreviousIndex, EventType eventType, StoreProperty[] props);

    public interface IRowSet : IEnumerable<PropertyRow>, IDisposable
    {
        void SetColumns(params PropertyId[] columnIds);

        void SetAggregateColumns(params AggregateColumn[] columns);

        void SetGroupBy(params PropertyId[] groupBy);

        void SetFilter(params FilterProperty[] filter);

        void SetSortOrder(params SortProperty[] sort);

        int GetCount();

        int GetCurrentRow();

        int GetObjectIndex(int itemId);
        
        PropertyRow[] GetRows(int firstRow, int lastRow);

        PropertyRow[] GetRows(int firstRow, int lastRow, out int rowCount);
        
        void Pause();
        
        void Resume();

//        event OnRowSetChange OnChange;

        // To be deprecated
        
        void RegisterHandler(OnRowSetChange handler);
        
        void UnRegisterHandler(OnRowSetChange handler);
    }

    // For methods newly added in V3 
    public interface IRowSetEx : IRowSet
    {
        void SetTop(int top);
        PropertyRow[] GetRows(int firstRow, int lastRow, bool defineBoundary, out int rowCount);        
        void Invalidate();
        void SetFrozenColumns(PropertyId[] columnIds);
    }

}
