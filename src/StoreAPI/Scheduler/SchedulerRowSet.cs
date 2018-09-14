using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines a rowset object that you can use to retrieve data from the rowset.</para>
    /// </summary>
    /// <remarks>
    ///   <para>You must dispose of this instance when done.</para>
    ///   <para>The following methods return this interface:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJobRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTaskRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator" />
    public interface ISchedulerRowSet : IDisposable
    {
        /// <summary>
        ///   <para>Retrieves the number of rows in the rowset.</para>
        /// </summary>
        /// <returns>
        ///   <para>The number of rows in the rowset.</para>
        /// </returns>
        int GetCount();

        /// <summary>
        ///   <para>Retrieves the rowset index for the specified object.</para>
        /// </summary>
        /// <param name="itemId">
        ///   <para>An identifier that identifies an object in the rowset. For example, if the rowset contains job objects, specify a job identifier (see the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Id" /> property).</para>
        /// </param>
        /// <returns>
        ///   <para>The index for the specified object in the rowset.</para>
        /// </returns>
        /// <remarks>
        ///   <para>If the rowset contains nodes, specify a node identifier (see the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.Id" /> property). If the rowset contains tasks, specify the system identifier for the task (see 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.Id" />).</para>
        /// </remarks>
        int GetObjectIndex(int itemId);

        /// <summary>
        ///   <para>Retrieve the specified range of rows from the rowset.</para>
        /// </summary>
        /// <param name="firstRow">
        ///   <para>The zero-based index to the first row to retrieve.</para>
        /// </param>
        /// <param name="lastRow">
        ///   <para>The zero-based index to the last row to retrieve.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRowSet" /> object that contains the specified rows.</para>
        /// </returns>
        /// <remarks>
        ///   <para>If the value that you specify for <paramref name="lastRow" /> is 
        /// more than the number of rows in the rowset, the method returns all rows.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc907078(v=vs.85).aspx">Using a Rowset to Enumerate a List of Objects</see>.</para> 
        /// </example>
        PropertyRowSet GetRows(int firstRow, int lastRow);

        /// <summary>
        ///   <para>Retrieves the specified range of rows from the rowset and returns a count of the number of rows in the rowset.</para>
        /// </summary>
        /// <param name="firstRow">
        ///   <para>The zero-based index to the first row to retrieve.</para>
        /// </param>
        /// <param name="lastRow">
        ///   <para>The zero-based index to the last row to retrieve.</para>
        /// </param>
        /// <param name="rowCount">
        ///   <para>The number of rows in the rowset (the same as if you called the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowSet.GetCount" /> method).</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRowSet" /> object that contains the specified rows.</para>
        /// </returns>
        /// <remarks>
        ///   <para>If the value that you specify for <paramref name="lastRow" /> is 
        /// more than the number of rows in the rowset, the method returns all rows.</para>
        /// </remarks>
        PropertyRowSet GetRows(int firstRow, int lastRow, out int rowCount);
    }

    internal class SchedulerRowSet : ISchedulerRowSet
    {
        protected IRowSet _rowset;
        private bool _isDisposed = false;

        private PropertyId[] _pids = null;

        static PropertyId[] _defaultColumns =
        {
            StorePropertyIds.Id,
        };

        internal virtual PropertyId[] GetDefaultColumns()
        {
            return _defaultColumns;
        }

        internal void Init(IRowSet rowset, IPropertyIdCollection pids, IFilterCollection filter, ISortCollection sort)
        {
            _rowset = rowset;

            if (pids != null && pids.Count > 0)
            {
                _pids = pids.GetIds();
            }
            else
            {
                _pids = GetDefaultColumns();
            }

            _rowset.SetColumns(_pids);

            if (filter != null && filter.Count > 0)
            {
                _rowset.SetFilter(filter.GetFilters());
            }

            if (sort != null && sort.Count > 0)
            {
                _rowset.SetSortOrder(sort.GetSorts());
            }
        }


        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        // The pattern used here for disposal comes from the .NET Framework Design Guide
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_rowset != null)
                {
                    _rowset.Dispose();
                }
            }

            // In all cases, null out interesting/large objects.  If we had any unmanaged
            // objects, we would free them here as well.
            _rowset = null;

            // This object is disposed and no longer usable.
            _isDisposed = true;
        }

        public int GetCount()
        {
            return _rowset.GetCount();
        }

        public int GetObjectIndex(int itemId)
        {
            return _rowset.GetObjectIndex(itemId);
        }

        public PropertyRowSet GetRows(int firstRow, int lastRow)
        {
            return new PropertyRowSet(_pids, _rowset.GetRows(firstRow, lastRow));
        }

        public PropertyRowSet GetRows(int firstRow, int lastRow, out int rowCount)
        {
            return new PropertyRowSet(_pids, _rowset.GetRows(firstRow, lastRow, out rowCount));
        }
    }

    internal class JobRowSet : SchedulerRowSet
    {
        internal override PropertyId[] GetDefaultColumns()
        {
            return JobRowEnumerator._defaultColumns;
        }
    }

    internal class TaskRowSet : SchedulerRowSet
    {
        internal override PropertyId[] GetDefaultColumns()
        {
            return TaskEnumerator._defaultColumns;
        }
    }

    internal class PoolRowSet : SchedulerRowSet
    {
        internal static PropertyId[] _defaultColumns =
        {
            PoolPropertyIds.Id,
            PoolPropertyIds.Name,
        };
        internal override PropertyId[] GetDefaultColumns()
        {
            return _defaultColumns;
        }
    }

}
