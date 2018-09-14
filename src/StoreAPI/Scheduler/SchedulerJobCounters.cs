using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines the counter values related to the status of tasks in the job (for example, the number of tasks that have finished running).</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetCounters" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerCounters" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNodeCounters" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTaskCounters" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIJobCounters)]
    public interface ISchedulerJobCounters
    {
        /// <summary>
        ///   <para>Retrieves the number of tasks in the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks in the job.</para>
        /// </value>
        int TaskCount { get; }

        /// <summary>
        ///   <para>The number of tasks in the job that are being configured and have not yet been added to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks being configured.</para>
        /// </value>
        int ConfiguringTaskCount { get; }

        /// <summary>
        ///   <para>Gets the number of tasks in the job for which the scheduler is determining if the task is correctly configured and can run.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="System.Int32" /> that indicates the number of tasks in the job for which the scheduler is determining if the task is correctly configured and can run.</para> 
        /// </value>
        int ValidatingTaskCount { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have been submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have been submitted.</para>
        /// </value>
        int SubmittedTaskCount { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks in the job that are queued and ready to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are queued and ready to run.</para>
        /// </value>
        int QueuedTaskCount { get; }

        /// <summary>
        ///   <para>Gets the number of tasks in the job that the scheduler is sending to a node to run.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="System.Int32" /> that indicates the number of tasks in the job that the scheduler is sending to a node to run.</para>
        /// </value>
        int DispatchingTaskCount { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are running.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property has meaning only if the job is running.</para>
        /// </remarks>
        int RunningTaskCount { get; }

        /// <summary>
        ///   <para>The number of tasks in the job that failed the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have failed.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the job is running, this is the number of tasks that have failed since the job began running.</para>
        /// </remarks>
        int FailedTaskCount { get; }

        /// <summary>
        ///   <para>Gets the number of tasks in the job for which the node is cleaning up the resources that were allocated to the task.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="System.Int32" /> that indicates the number of tasks in the job for which the node is cleaning up the resources that were allocated to the task.</para> 
        /// </value>
        int FinishingTaskCount { get; }

        /// <summary>
        ///   <para>The number of tasks that are being canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks being canceled.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property is useful only if the job is running.</para>
        /// </remarks>
        int CancelingTaskCount { get; }

        /// <summary>
        ///   <para>The number of tasks that were canceled the last time the job ran.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have been canceled.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the job is running, the property reflects the number of tasks that have been canceled since the job began running.</para>
        /// </remarks>
        int CanceledTaskCount { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have finished running.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have finished.</para>
        /// </value>
        int FinishedTaskCount { get; }

        /// <summary>
        ///   <para>Retrieves the total CPU time used by all tasks in the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The total CPU time, in milliseconds.</para>
        /// </value>
        Int64 TotalCpuTime { get; }

        /// <summary>
        ///   <para>The elapsed execution time for user-mode instructions (the total time that all tasks in the job spent in user-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>The total user-mode time, in milliseconds.</para>
        /// </value>
        Int64 TotalUserTime { get; }

        /// <summary>
        ///   <para>The elapsed execution time for kernel-mode instructions (the total time that all tasks in the job spent in kernel-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>The total kernel-mode time, in milliseconds.</para>
        /// </value>
        Int64 TotalKernelTime { get; }

        /// <summary>
        ///   <para>Retrieves the total amount of memory used by the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The total amount of memory, in bytes, used by the job.</para>
        /// </value>
        Int64 TotalMemory { get; }
    }

    /// <summary>
    ///   <para>Defines the counter values related to the status of tasks in the job (for example, the number of tasks that have finished running).</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidJobCountersClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class SchedulerJobCounters : ISchedulerJobCounters
    {
        PropertyRow _row = null;

        IClusterStoreObject _item = null;

        internal SchedulerJobCounters(IClusterStoreObject item)
        {
            _item = item;
        }

        static PropertyId[] _pids =
        {
            JobPropertyIds.TaskCount,
            JobPropertyIds.ConfiguringTaskCount,
            JobPropertyIds.ValidatingTaskCount,
            JobPropertyIds.SubmittedTaskCount,
            JobPropertyIds.QueuedTaskCount,
            JobPropertyIds.DispatchingTaskCount,
            JobPropertyIds.RunningTaskCount,
            JobPropertyIds.FailedTaskCount,
            JobPropertyIds.CancelingTaskCount,
            JobPropertyIds.CanceledTaskCount,
            JobPropertyIds.FinishingTaskCount,
            JobPropertyIds.FinishedTaskCount,

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

        #region ISchedulerJobCounters Members

        /// <summary>
        ///   <para>Retrieves the number of tasks in the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks in the job.</para>
        /// </value>
        public int TaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.TaskCount); }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks being configured.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks being configured.</para>
        /// </value>
        public int ConfiguringTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.ConfiguringTaskCount); }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int ValidatingTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.ValidatingTaskCount); }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have been submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have been submitted.</para>
        /// </value>
        public int SubmittedTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.SubmittedTaskCount); }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are queued and ready to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are queued and ready to run.</para>
        /// </value>
        public int QueuedTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.QueuedTaskCount); }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int DispatchingTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.DispatchingTaskCount); }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are running.</para>
        /// </value>
        public int RunningTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.RunningTaskCount); }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have failed.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have failed.</para>
        /// </value>
        public int FailedTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.FailedTaskCount); }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int FinishingTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.FinishingTaskCount); }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks being canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks being canceled.</para>
        /// </value>
        public int CancelingTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.CancelingTaskCount); }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have been canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have been canceled.</para>
        /// </value>
        public int CanceledTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.CanceledTaskCount); }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have finished.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have finished.</para>
        /// </value>
        public int FinishedTaskCount
        {
            get { return GetValueFromRow(JobPropertyIds.FinishedTaskCount); }
        }

        /// <summary>
        ///   <para>Retrieves the total CPU time used by all tasks in the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The total CPU time, in milliseconds.</para>
        /// </value>
        public long TotalCpuTime
        {
            get { return GetValueFromRowInt64(JobPropertyIds.TotalCpuTime); }
        }

        /// <summary>
        ///   <para>The elapsed execution time for user-mode instructions (the total time that all tasks in the job spent in user-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>The total user-mode time, in milliseconds.</para>
        /// </value>
        public long TotalUserTime
        {
            get { return GetValueFromRowInt64(JobPropertyIds.TotalUserTime); }
        }

        /// <summary>
        ///   <para>The elapsed execution time for kernel-mode instructions (the total time that all tasks in the job spent in kernel-mode).</para>
        /// </summary>
        /// <value>
        ///   <para>The total kernel-mode time, in milliseconds.</para>
        /// </value>
        public long TotalKernelTime
        {
            get { return GetValueFromRowInt64(JobPropertyIds.TotalKernelTime); }
        }

        /// <summary>
        ///   <para>Retrieves the total amount of memory used by the job.</para>
        /// </summary>
        /// <value>
        ///   <para>Total amount of memory, in bytes, used by the job.</para>
        /// </value>
        public long TotalMemory
        {
            get { return GetValueFromRowInt64(StorePropertyIds.MemoryUsed); }
        }

        #endregion
    }
}
