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
    ///   <para>Contains the methods used to retrieve rows from the enumerator.</para>
    /// </summary>
    /// <remarks>
    ///   <para>You must dispose of this instance when done.</para>
    ///   <para>The following methods return this interface:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJobEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJobHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNodeEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNodeHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTaskEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <example>
    ///   <para>For an example, see <see 
    /// href="https://msdn.microsoft.com/library/cc907078(v=vs.85).aspx">Using a Rowset to Enumerate a List of Objects</see>.</para> 
    /// </example>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerRowSet" />
    public interface ISchedulerRowEnumerator : IEnumerable<PropertyRow>, IDisposable
    {
        /// <summary>
        ///   <para>Retrieves one or more rows from the enumerator.</para>
        /// </summary>
        /// <param name="numberOfRows">
        ///   <para>The number of rows to retrieve from the enumerator.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRowSet" /> object that contains the rows from the enumerator.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Call this method in a loop until the <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRowSet.Length" /> property is zero.</para>
        ///   <para>Specify a reasonable number of rows to retrieve (for example, 128 or 256). Requesting too 
        /// many rows may stress memory and requesting too few rows will cost the same as requesting more rows.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc907078(v=vs.85).aspx">Using a Rowset to Enumerate a List of Objects</see>.</para> 
        /// </example>
        PropertyRowSet GetRows(int numberOfRows);
    }

    internal class SchedulerRowEnumerator : ISchedulerRowEnumerator
    {
        protected IRowEnumerator _enum = null;
        private bool _isDisposed = false;

        static PropertyId[] _defaultColumns =
        {
            StorePropertyIds.Id,
        };

        internal virtual PropertyId[] GetDefaultColumns()
        {
            return _defaultColumns;
        }

        internal void Init(IRowEnumerator rows, IPropertyIdCollection pids, IFilterCollection filter, ISortCollection sort)
        {
            _enum = rows;

            if (pids != null && pids.Count > 0)
            {
                _enum.SetColumns(pids.GetIds());
            }
            else
            {
                _enum.SetColumns(GetDefaultColumns());
            }

            if (filter != null && filter.Count > 0)
            {
                _enum.SetFilter(filter.GetFilters());
            }

            if (sort != null && sort.Count > 0)
            {
                _enum.SetSortOrder(sort.GetSorts());
            }
        }

        public PropertyRowSet GetRows(int numberOfRows)
        {
            return _enum.GetRows2(numberOfRows);
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
                if (_enum != null)
                {
                    _enum.Dispose();
                }
            }

            // In all cases, null out interesting/large objects.  If we had any unmanaged
            // objects, we would free them here as well.
            _enum = null;

            // This object is disposed and no longer usable.
            _isDisposed = true;
        }

        public IEnumerator<PropertyRow> GetEnumerator()
        {
            return _enum.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class JobRowEnumerator : SchedulerRowEnumerator
    {
        internal static PropertyId[] _defaultColumns =
        {
            JobPropertyIds.Id,
            JobPropertyIds.State,
            JobPropertyIds.Name,
            JobPropertyIds.Owner,
        };

        internal override PropertyId[] GetDefaultColumns()
        {
            return _defaultColumns;
        }
    }

    internal class JobHistoryEnumerator : SchedulerRowEnumerator
    {
        static PropertyId[] _defaultColumns =
        {
            JobHistoryPropertyIds.Id,
            JobHistoryPropertyIds.JobEvent,
            JobHistoryPropertyIds.EventTime,
            JobHistoryPropertyIds.JobId,
            JobHistoryPropertyIds.RequeueId,
            JobHistoryPropertyIds.SubmitTime,
            JobHistoryPropertyIds.StartTime,
            JobHistoryPropertyIds.EndTime,
            JobHistoryPropertyIds.Project,
            JobHistoryPropertyIds.JobTemplate,
            JobHistoryPropertyIds.CpuTime,
            JobHistoryPropertyIds.Runtime,
            JobHistoryPropertyIds.AverageMemoryUsed,
            JobHistoryPropertyIds.MemoryUsed,
            JobHistoryPropertyIds.HasGrown,
            JobHistoryPropertyIds.HasShrunk,
            JobHistoryPropertyIds.ServiceName,
            JobHistoryPropertyIds.NumberOfCalls,
            JobHistoryPropertyIds.CallDuration,
            JobHistoryPropertyIds.CallsPerSecond,
            JobHistoryPropertyIds.JobOwner,
        };

        internal override PropertyId[] GetDefaultColumns()
        {
            return _defaultColumns;
        }
    }

    internal class NodeHistoryEnumerator : SchedulerRowEnumerator
    {
        static PropertyId[] _defaultColumns =
        {
            NodeHistoryPropertyIds.Id,
            NodeHistoryPropertyIds.NodeGuid,
            NodeHistoryPropertyIds.NodeName,
            NodeHistoryPropertyIds.NodeId,
            NodeHistoryPropertyIds.NodeEvent,
            NodeHistoryPropertyIds.EventTime,
        };

        internal override PropertyId[] GetDefaultColumns()
        {
            return _defaultColumns;
        }
    }

    internal class NodeEnumerator : SchedulerRowEnumerator
    {
        static PropertyId[] _defaultColumns =
        {
            NodePropertyIds.Id,
            NodePropertyIds.Name,
            NodePropertyIds.State,
        };

        internal override PropertyId[] GetDefaultColumns()
        {
            return _defaultColumns;
        }
    }

    internal class TaskEnumerator : SchedulerRowEnumerator
    {
        internal static PropertyId[] _defaultColumns =
        {
            TaskPropertyIds.Id,
            TaskPropertyIds.State,
            TaskPropertyIds.StartTime,
            TaskPropertyIds.EndTime,
        };

        internal override PropertyId[] GetDefaultColumns()
        {
            return _defaultColumns;
        }
    }

    internal class AllocationHistoryEnumerator : SchedulerRowEnumerator
    {
        internal static PropertyId[] _defaultColumns =
        {
            AllocationProperties.JobId,
            AllocationProperties.TaskId,
            AllocationProperties.NodeId,
            AllocationProperties.CoreId,
            AllocationProperties.JobRequeueCount,
            AllocationProperties.TaskRequeueCount,
            AllocationProperties.StartTime,
            AllocationProperties.EndTime,
        };

        internal override PropertyId[] GetDefaultColumns()
        {
            return _defaultColumns;
        }

    }
}
