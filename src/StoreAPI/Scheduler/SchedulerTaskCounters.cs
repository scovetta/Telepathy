using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Contains counter data for the tasks that are running in the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.GetCounters" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerCounters" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNodeCounters" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidITaskCounters)]
    public interface ISchedulerTaskCounters
    {
        /// <summary>
        ///   <para>Retrieves the total CPU time used by all processes that are running in the cluster for this task.</para>
        /// </summary>
        /// <value>
        ///   <para>The total CPU time, in milliseconds.</para>
        /// </value>
        Int64 TotalCpuTime { get; }

        /// <summary>
        ///   <para>The elapsed execution time for user-mode instructions (the total time that 
        /// all processes that are running in the cluster have spent in user-mode for this task).</para>
        /// </summary>
        /// <value>
        ///   <para>The total user-mode time, in milliseconds.</para>
        /// </value>
        Int64 TotalUserTime { get; }

        /// <summary>
        ///   <para>The elapsed execution time for kernel-mode instructions (the total time that 
        /// all processes that are running in the cluster spent in kernel-mode for this task).</para>
        /// </summary>
        /// <value>
        ///   <para>The total kernel-mode time, in milliseconds.</para>
        /// </value>
        Int64 TotalKernelTime { get; }

        /// <summary>
        ///   <para>Retrieves the total memory used by all processes that are running in the cluster for this task.</para>
        /// </summary>
        /// <value>
        ///   <para>The total memory, in bytes, used by all tasks.</para>
        /// </value>
        Int64 TotalMemory { get; }
    }

    /// <summary>
    ///   <para>Contains counter data for the tasks that are running in the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTaskCounters" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidTaskCountersClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class SchedulerTaskCounters : ISchedulerTaskCounters
    {
        PropertyRow _row = null;

        IClusterStoreObject _item = null;

        internal SchedulerTaskCounters(IClusterStoreObject item)
        {
            _item = item;
        }

        static PropertyId[] _pids =
        {

            StorePropertyIds.TotalCpuTime,
            StorePropertyIds.TotalUserTime,
            StorePropertyIds.TotalKernelTime,

            StorePropertyIds.MemoryUsed,
        };

        internal void Refresh()
        {
            _row = _item.GetProps(_pids);
        }


        int GetValueFromRow(PropertyId pid)
        {
            StoreProperty prop = _row[pid];

            if (prop != null)
            {
                return (int)prop.Value;
            }

            return 0;
        }

        Int64 GetValueFromRowInt64(PropertyId pid)
        {
            StoreProperty prop = _row[pid];

            if (prop != null)
            {
                return (Int64)prop.Value;
            }

            return 0;
        }


        /// <summary>
        ///   <para>Retrieves the total CPU time used by all tasks that are running in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total CPU time, in milliseconds.</para>
        /// </value>
        public long TotalCpuTime
        {
            get { return GetValueFromRowInt64(JobPropertyIds.TotalCpuTime); }
        }

        /// <summary>
        ///   <para>The elapsed execution time for user-mode instructions (the total time 
        /// that all tasks that are running in the cluster have spent in user-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>The total user-mode time, in milliseconds.</para>
        /// </value>
        public long TotalUserTime
        {
            get { return GetValueFromRowInt64(JobPropertyIds.TotalUserTime); }
        }

        /// <summary>
        ///   <para>The elapsed execution time for kernel-mode instructions (the total time 
        /// that all tasks that are running in the cluster have spent in kernel-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>The total kernel-mode time, in milliseconds.</para>
        /// </value>
        public long TotalKernelTime
        {
            get { return GetValueFromRowInt64(JobPropertyIds.TotalKernelTime); }
        }

        /// <summary>
        ///   <para>Retrieves the total memory used by all tasks that are running in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total memory, in bytes, used by all tasks.</para>
        /// </value>
        public long TotalMemory
        {
            get { return GetValueFromRowInt64(StorePropertyIds.MemoryUsed); }
        }
    }
}
