using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Contains counter information for the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetCounters" /> method.</para>
    ///   <para>The counters are a snapshot of the activity in the cluster at the time you retrieved the 
    /// data. The counter values in the cluster (not this object) will increase or decrease as activity in the cluster occurs.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNodeCounters" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTaskCounters" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISchedulerCounters)]
    public interface ISchedulerCounters
    {
        /// <summary>
        ///   <para>Retrieves the total number of nodes in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of nodes in the cluster.</para>
        /// </value>
        int TotalNodes { get; }

        /// <summary>
        ///   <para>Retrieves the number of nodes that are ready to run jobs.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of nodes that are ready to run jobs.</para>
        /// </value>
        int ReadyNodes { get; }

        /// <summary>
        ///   <para>Retrieves the number of nodes that are offline.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of nodes that are offline.</para>
        /// </value>
        int OfflineNodes { get; }

        /// <summary>
        ///   <para>Retrieves the number of nodes that are in the process of removing jobs from the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of nodes that are in the process of removing jobs from the node.</para>
        /// </value>
        int DrainingNodes { get; }

        /// <summary>
        ///   <para>Retrieves the total number of node in the cluster that are not reachable.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of node in the cluster that are not reachable.</para>
        /// </value>
        int UnreachableNodes { get; }

        /// <summary>
        ///   <para>Retrieves the total number of sockets in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of sockets in the cluster.</para>
        /// </value>
        int TotalSockets { get; }

        /// <summary>
        ///   <para>Retrieves the total number of cores in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of cores in the cluster.</para>
        /// </value>
        int TotalCores { get; }

        /// <summary>
        ///   <para>Retrieves the number of cores that are not allocated to a job.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are not allocated to a job.</para>
        /// </value>
        int IdleCores { get; }

        /// <summary>
        ///   <para>Retrieves the number of cores that are busy running tasks.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are busy running tasks.</para>
        /// </value>
        int BusyCores { get; }

        /// <summary>
        ///   <para>Retrieves the number of cores that are offline.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are offline.</para>
        /// </value>
        int OfflineCores { get; }

        /// <summary>
        ///   <para>Retrieves the total number of jobs in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of jobs in the cluster.</para>
        /// </value>
        int TotalJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number of jobs that are being configured.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that are being configured.</para>
        /// </value>
        int ConfiguringJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number of jobs that have been submitted to the scheduling queue and are awaiting validation.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that have been submitted to the scheduling queue and are awaiting validation.</para>
        /// </value>
        int SubmittedJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number of jobs being validated.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs being validated.</para>
        /// </value>
        int ValidatingJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number of jobs that are in the scheduling queue.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that are in the scheduling queue.</para>
        /// </value>
        int QueuedJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number of jobs that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that are running.</para>
        /// </value>
        int RunningJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number of jobs for which the server is cleaning up the resources that were allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs for which the server is cleaning up the resources that were allocated to the job.</para>
        /// </value>
        int FinishingJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number of jobs that have finished running successfully.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that have finished running successfully.</para>
        /// </value>
        int FinishedJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number of jobs that failed.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that failed.</para>
        /// </value>
        int FailedJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number of jobs that are in the process of being canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that are in the process of being canceled.</para>
        /// </value>
        int CancelingJobs { get; }

        /// <summary>
        ///   <para>Retrieves the number jobs that have been canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number jobs that have been canceled.</para>
        /// </value>
        int CanceledJobs { get; }

        /// <summary>
        ///   <para>Retrieves the total number of tasks in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of tasks in the cluster.</para>
        /// </value>
        int TotalTasks { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are being configured.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are being configured.</para>
        /// </value>
        int ConfiguringTasks { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have been submitted to the scheduling queue and are awaiting validation.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have been submitted to the scheduling queue and are awaiting validation.</para>
        /// </value>
        int SubmittedTasks { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are in the scheduling queue.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are in the scheduling queue.</para>
        /// </value>
        int QueuedTasks { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are running.</para>
        /// </value>
        int RunningTasks { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that failed.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that failed.</para>
        /// </value>
        int FailedTasks { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are in the process of being canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are in the process of being canceled.</para>
        /// </value>
        int CancelingTasks { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have been canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have been canceled.</para>
        /// </value>
        int CanceledTasks { get; }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have finished running successfully.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have finished running successfully.</para>
        /// </value>
        int FinishedTasks { get; }
    }

    /// <summary>
    ///   <para>Defines counter information for the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerCounters" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidSchedulerCounters)]
    [ClassInterface(ClassInterfaceType.None)]
    public class SchedulerCounters : ISchedulerCounters
    {
        int _TotalNodes;
        int _ReadyNodes;
        int _OfflineNodes;
        int _DrainingNodes;
        int _UnreachableNodes;

        int _TotalSockets;

        int _TotalCores;
        int _IdleCores;
        int _OfflineCores;
        int _BusyCores;

        int _TotalJobs;
        int _ConfiguringJobs;
        int _SubmittedJobs;
        int _ValidatingJobs;
        int _QueuedJobs;
        int _RunningJobs;
        int _FinishingJobs;
        int _FinishedJobs;
        int _FailedJobs;
        int _CancelingJobs;
        int _CanceledJobs;

        int _TotalTasks;
        int _ConfiguringTasks;
        int _SubmittedTasks;
        int _QueuedTasks;
        int _RunningTasks;
        int _FailedTasks;
        int _CancelingTasks;
        int _CanceledTasks;
        int _FinishedTasks;

        int GetValueFromRow(PropertyRow row, PropertyId pid)
        {
            StoreProperty prop = row[pid];

            if (prop != null)
            {
                return (int)prop.Value;
            }

            return 0;
        }

        internal void Init(ISchedulerStore store)
        {
            PropertyId[] _pids =
            {
                StorePropertyIds.TotalNodeCount,        // int _TotalNodes;
                StorePropertyIds.ReadyNodeCount,        // int _ReadyNodes;
                StorePropertyIds.OfflineNodeCount,      // int _OfflineNodes;
                StorePropertyIds.DrainingNodeCount,     // int _DraingNodes;
                StorePropertyIds.UnreachableNodeCount,  // int _UnreachableNodes;

                StorePropertyIds.TotalSocketCount,      // int _TotalSockets;

                StorePropertyIds.TotalCoreCount,    // int _TotalCores;
                StorePropertyIds.IdleResourceCount,     // int _IdleCores;
                StorePropertyIds.OfflineResourceCount,  // int _OfflineCores;
                //    int _BusyCores;

                StorePropertyIds.TotalJobCount,         // int _TotalJobs;
                StorePropertyIds.ConfigJobCount,        // int _ConfiguringJobs;
                StorePropertyIds.SubmittedJobCount,     // int _SubmittedJobs;
                StorePropertyIds.ValidatingJobCount,    // int _ValidatingJobs;
                StorePropertyIds.QueuedJobCount,        // int _QueuedJobs;
                StorePropertyIds.RunningJobCount,       // int _RunningJobs;
                StorePropertyIds.FinishingJobCount,     // int _FinishingJobs;
                StorePropertyIds.FinishedJobCount,      // int _FinishedJobs;
                StorePropertyIds.FailedJobCount,        // int _FailedJobs;
                StorePropertyIds.CancelingJobCount,     // int _CancelingJobs;
                StorePropertyIds.CanceledJobCount,      // int _CanceledJobs;

                StorePropertyIds.TotalTaskCount,        // int _TotalTasks;
                StorePropertyIds.ConfiguringTaskCount,  // int _ConfiguringTasks;
                StorePropertyIds.SubmittedTaskCount,    // int _SubmittedTasks;
                StorePropertyIds.QueuedTaskCount,       // int _QueuedTasks;
                StorePropertyIds.RunningTaskCount,      // int _RunningTasks;
                StorePropertyIds.FailedTaskCount,       // int _FailedTasks;
                StorePropertyIds.CancelingTaskCount,    // int _CancelingTasks;
                StorePropertyIds.CanceledTaskCount,     // int _CanceledTasks;
                StorePropertyIds.FinishedTaskCount,     // int _FinishedTasks;
            };

            PropertyRow row = store.GetProps(_pids);

            _TotalNodes = GetValueFromRow(row, StorePropertyIds.TotalNodeCount);
            _ReadyNodes = GetValueFromRow(row, StorePropertyIds.ReadyNodeCount);
            _OfflineNodes = GetValueFromRow(row, StorePropertyIds.OfflineNodeCount);
            _DrainingNodes = GetValueFromRow(row, StorePropertyIds.DrainingNodeCount);
            _UnreachableNodes = GetValueFromRow(row, StorePropertyIds.UnreachableNodeCount);

            _TotalSockets = GetValueFromRow(row, StorePropertyIds.TotalSocketCount);

            _TotalCores = GetValueFromRow(row, StorePropertyIds.TotalCoreCount);
            _IdleCores = GetValueFromRow(row, StorePropertyIds.IdleResourceCount);
            _OfflineCores = GetValueFromRow(row, StorePropertyIds.OfflineResourceCount);
            _BusyCores = _TotalCores - _IdleCores - _OfflineCores;

            _TotalJobs = GetValueFromRow(row, StorePropertyIds.TotalJobCount);
            _ConfiguringJobs = GetValueFromRow(row, StorePropertyIds.ConfigJobCount);
            _SubmittedJobs = GetValueFromRow(row, StorePropertyIds.SubmittedJobCount);
            _ValidatingJobs = GetValueFromRow(row, StorePropertyIds.ValidatingJobCount);
            _QueuedJobs = GetValueFromRow(row, StorePropertyIds.QueuedJobCount);
            _RunningJobs = GetValueFromRow(row, StorePropertyIds.RunningJobCount);
            _FinishingJobs = GetValueFromRow(row, StorePropertyIds.FinishingJobCount);
            _FinishedJobs = GetValueFromRow(row, StorePropertyIds.FinishedJobCount);
            _FailedJobs = GetValueFromRow(row, StorePropertyIds.FailedJobCount);
            _CancelingJobs = GetValueFromRow(row, StorePropertyIds.CancelingJobCount);
            _CanceledJobs = GetValueFromRow(row, StorePropertyIds.CanceledJobCount);

            _TotalTasks = GetValueFromRow(row, StorePropertyIds.TotalTaskCount);
            _ConfiguringTasks = GetValueFromRow(row, StorePropertyIds.ConfiguringTaskCount);
            _SubmittedTasks = GetValueFromRow(row, StorePropertyIds.SubmittedTaskCount);
            _QueuedTasks = GetValueFromRow(row, StorePropertyIds.QueuedTaskCount);
            _RunningTasks = GetValueFromRow(row, StorePropertyIds.RunningTaskCount);
            _FailedTasks = GetValueFromRow(row, StorePropertyIds.FailedTaskCount);
            _CancelingTasks = GetValueFromRow(row, StorePropertyIds.CancelingTaskCount);
            _CanceledTasks = GetValueFromRow(row, StorePropertyIds.CanceledTaskCount);
            _FinishedTasks = GetValueFromRow(row, StorePropertyIds.FinishedTaskCount);
        }


        /// <summary>
        ///   <para>Retrieves the total number of nodes in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of nodes in the cluster.</para>
        /// </value>
        public int TotalNodes
        {
            get { return _TotalNodes; }
        }

        /// <summary>
        ///   <para>Retrieves the number of nodes that are ready to run jobs.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of nodes that are ready to run jobs.</para>
        /// </value>
        public int ReadyNodes
        {
            get { return _ReadyNodes; }
        }

        /// <summary>
        ///   <para>Retrieves the number of nodes that are offline.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of nodes that are offline.</para>
        /// </value>
        public int OfflineNodes
        {
            get { return _OfflineNodes; }
        }

        /// <summary>
        ///   <para>Retrieves the number of nodes that are in the process of removing jobs from the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of nodes that are in the process of removing jobs from the node.</para>
        /// </value>
        public int DrainingNodes
        {
            get { return _DrainingNodes; }
        }

        /// <summary>
        ///   <para>Retrieves the total number of node in the cluster that are not reachable.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of node in the cluster that are not reachable.</para>
        /// </value>
        public int UnreachableNodes
        {
            get { return _UnreachableNodes; }
        }

        /// <summary>
        ///   <para>Retrieves the total number of sockets in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of sockets in the cluster.</para>
        /// </value>
        public int TotalSockets
        {
            get { return _TotalSockets; }
        }

        /// <summary>
        ///   <para>Retrieves the total number of cores in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of cores in the cluster.</para>
        /// </value>
        public int TotalCores
        {
            get { return _TotalCores; }
        }

        /// <summary>
        ///   <para>Retrieves the number of cores that are not allocated to a job.</para>
        /// </summary>
        /// <value>
        ///   <para>Retrieves the number of cores that are not allocated to a job.</para>
        /// </value>
        public int IdleCores
        {
            get { return _IdleCores; }
        }

        /// <summary>
        ///   <para>Retrieves the number of cores that are busy running tasks.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are busy running tasks.</para>
        /// </value>
        public int BusyCores
        {
            get { return _BusyCores; }
        }

        /// <summary>
        ///   <para>Retrieves the number of cores that are offline.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are offline.</para>
        /// </value>
        public int OfflineCores
        {
            get { return _OfflineCores; }
        }

        /// <summary>
        ///   <para>Retrieves the total number of jobs in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of jobs in the cluster.</para>
        /// </value>
        public int TotalJobs
        {
            get { return _TotalJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number of jobs that are being configured.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that are being configured.</para>
        /// </value>
        public int ConfiguringJobs
        {
            get { return _ConfiguringJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number of jobs that have been submitted to the scheduling queue and are awaiting validation.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that have been submitted to the scheduling queue and are awaiting validation.</para>
        /// </value>
        public int SubmittedJobs
        {
            get { return _SubmittedJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number of jobs being validated.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs being validated.</para>
        /// </value>
        public int ValidatingJobs
        {
            get { return _ValidatingJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number of jobs that are in the scheduling queue.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that are in the scheduling queue.</para>
        /// </value>
        public int QueuedJobs
        {
            get { return _QueuedJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number of jobs that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that are running.</para>
        /// </value>
        public int RunningJobs
        {
            get { return _RunningJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number of jobs for which the server is cleaning up the resources that were allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs for which the server is cleaning up the resources that were allocated to the job.</para>
        /// </value>
        public int FinishingJobs
        {
            get { return _FinishingJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number of jobs that have finished running successfully.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that have finished running successfully.</para>
        /// </value>
        public int FinishedJobs
        {
            get { return _FinishedJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number of jobs that failed.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that failed.</para>
        /// </value>
        public int FailedJobs
        {
            get { return _FailedJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number of jobs that are in the process of being canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of jobs that are in the process of being canceled.</para>
        /// </value>
        public int CancelingJobs
        {
            get { return _CancelingJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the number jobs that have been canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number jobs that have been canceled.</para>
        /// </value>
        public int CanceledJobs
        {
            get { return _CanceledJobs; }
        }

        /// <summary>
        ///   <para>Retrieves the total number of tasks in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The total number of tasks in the cluster.</para>
        /// </value>
        public int TotalTasks
        {
            get { return _TotalTasks; }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are being configured.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are being configured.</para>
        /// </value>
        public int ConfiguringTasks
        {
            get { return _ConfiguringTasks; }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have been submitted to the scheduling queue and are awaiting validation.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have been submitted to the scheduling queue and are awaiting validation.</para>
        /// </value>
        public int SubmittedTasks
        {
            get { return _SubmittedTasks; }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are in the scheduling queue.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are in the scheduling queue.</para>
        /// </value>
        public int QueuedTasks
        {
            get { return _QueuedTasks; }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are running.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are running.</para>
        /// </value>
        public int RunningTasks
        {
            get { return _RunningTasks; }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that failed.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that failed.</para>
        /// </value>
        public int FailedTasks
        {
            get { return _FailedTasks; }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that are in the process of being canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that are in the process of being canceled.</para>
        /// </value>
        public int CancelingTasks
        {
            get { return _CancelingTasks; }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have been canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have been canceled.</para>
        /// </value>
        public int CanceledTasks
        {
            get { return _CanceledTasks; }
        }

        /// <summary>
        ///   <para>Retrieves the number of tasks that have finished running successfully.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of tasks that have finished running successfully.</para>
        /// </value>
        public int FinishedTasks
        {
            get { return _FinishedTasks; }
        }
    }
}
