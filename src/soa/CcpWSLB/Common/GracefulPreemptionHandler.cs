//------------------------------------------------------------------------------
// <copyright file="GracefulPreemptionHandler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
///     This class handles the graceful preemption requests from scheduler.
///     It reads requests from scheduler and co-works with the ServiceJobMonitor and DispatcherManager
///     to release resources or cancel job.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.ServiceBroker.BackEnd;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// This class handles the graceful preemption requests from scheduler.
    /// It reads requests from scheduler and co-works with the ServiceJobMonitor and DispatcherManager
    /// to release resources or cancel job.
    /// </summary>
    internal class GracefulPreemptionHandler
    {
        /// <summary>
        /// The planned core count which means no preemption.
        /// </summary>
        private const int NoPreemptionCores = -1;

        /// <summary>
        private const int MaxRunawayTolerance = 6;
        /// The tasks that marked as exit if possible
        /// </summary>
        private HashSet<int> exitingTaskIds;

        /// <summary>
        /// The tasks that not marked as exit if possible
        /// </summary>
        private HashSet<int> runningTaskIds;

        /// <summary>
        /// The maximum core count that the scheduler want the job to have.
        /// </summary>
        public BalanceInfo BalanceInfo { get; private set; }

        /// <summary>
        /// the instance of the dispatcher Manager.
        /// </summary>
        private IDispatcherManager dispatcherManager;

        /// <summary>
        /// The sessionId.
        /// </summary>
        private int sessionId;

        /// <summary>
        /// Possible runaway tasks and how many times it is seen
        /// </summary>
        private readonly Dictionary<int, int> possibleRunaways = new Dictionary<int, int>();
        /// <summary>
        /// A set field of all recognized task IDs
        /// </summary>
        private readonly HashSet<int> recognizedTaskIds = new HashSet<int>();

        /// <summary>
        /// Task ID of already removed dispatchers
        /// </summary>
        private readonly HashSet<int> removedDispatcherIds = new HashSet<int>();

        /// <summary>
        /// Callback to finish *only* runaway tasks
        /// </summary>
        private readonly Action<int> finishRunawayTaskCallback;
        public GracefulPreemptionHandler(IDispatcherManager dispatcherManager, int sessionId, Action<int> finishRunawayTaskCallback)
        {
            // Initialize it to the max value to not preempt until get an actual number from scheduler.
            this.BalanceInfo = new BalanceInfo(int.MaxValue);

            this.dispatcherManager = dispatcherManager;
            this.sessionId = sessionId;
            this.finishRunawayTaskCallback = finishRunawayTaskCallback;
        }

        /// <summary>
        /// Get the dispatchers to shutdown to meet the preemption requirement.
        /// If the requirement cannot be met before the dispatcher count is less than min units,
        /// it will set the cancel job to true.
        /// If the requirement is met before all idle dispatchers are shutdown
        /// it will set the resume flag to resume the stopped dispatchers.
        /// </summary>
        /// <param name="idleDispatchers">the idle dispatcher list</param>
        /// <param name="schedulerAdapter">the adapter to the scheduler</param>
        /// <param name="minDispatchers">the minimum dispatchers count</param>
        /// <param name="shouldCancelJob">true if we need cancel the job</param>
        /// <param name="shouldResumeRemaining">true if we need resume the dispatchers.</param>
        /// <returns>the dispatchers to shutdown</returns>
        public BalanceResultInfo GetDispatchersToShutdown(
            List<DispatcherInfo> idleDispatchers,
            int minDispatchers,
            bool balanceRequestRefreshed)
        {
            idleDispatchers.Sort((d1, d2) => d2.CoreCount.CompareTo(d1.CoreCount));

            bool shouldCancelJob = false;
            int activeDispatcherCount = this.dispatcherManager.ActiveDispatcherCount;          
            BalanceInfo info = this.BalanceInfo;
            BrokerTracing.TraceVerbose(
                "[GracefulPreemptionHandler].GetDispatchersToShutdown: UseFastBalance = {0}, Request count = {1}, dispatcherCount = {2}, maxAllowedCoreCount = {3}, minDispatcherCount = {4}, balanceRequestRefreshed = {5}",
                info.UseFastBalance,
                info.BalanceRequests.Count,
                activeDispatcherCount,
                info.AllowedCoreCount,
                minDispatchers,
                balanceRequestRefreshed);
            var resultInfo = new BalanceResultInfo(info.UseFastBalance);
            if (info.UseFastBalance)
            {
                List<int> emptyRunningList = new List<int>();
                foreach (var request in info.BalanceRequests)
                {
                    resultInfo.GracefulPreemptionResults.Add(
                        this.GetDispatchersToShutdownAux(
                            idleDispatchers,
                            request.AllowedCoreCount,
                            true,
                            balanceRequestRefreshed,
                            minDispatchers,
                            request.TaskIds,
                            emptyRunningList,
                            ref activeDispatcherCount,
                            out shouldCancelJob));
                    if (shouldCancelJob)
                    {
                        break;
                    }
                }
            }
            else
            {
                Debug.Assert(info.BalanceRequests.Count == 1);
                Debug.Assert(info.BalanceRequests.First().TaskIds == null);
                var request = info.BalanceRequests.First();
                resultInfo.GracefulPreemptionResults.Add(
                    this.GetDispatchersToShutdownAux(
                        idleDispatchers,
                        request.AllowedCoreCount,
                        false,
                        balanceRequestRefreshed,
                        minDispatchers,
                        this.exitingTaskIds,
                        this.runningTaskIds,
                        ref activeDispatcherCount,
                        out shouldCancelJob));
            }
            if (shouldCancelJob)
            {
                resultInfo.ShouldCancelJob = true;
                this.StopAllDispatchers();
            }
            Debug.Assert(
                resultInfo.ShouldCancelJob || resultInfo.GracefulPreemptionResults.Count == info.BalanceRequests.Count,
                string.Format(
                    "[GracefulPreemptionHandler](Assert) GracefulPreemptionResults number {0} does not equal with BalanceRequests number {1}",
                    resultInfo.GracefulPreemptionResults.Count,
                    info.BalanceRequests.Count));
            BrokerTracing.TraceVerbose(
                "[GracefulPreemptionHandler].GetDispatchersToShutdown: return dispatchers Count: {0}, results count: {1}",
                resultInfo.GracefulPreemptionResults.Sum(r => r.DispatchersToShutdown.Count),
                resultInfo.GracefulPreemptionResults.Count);
            return resultInfo;
        }

        /// <summary>
        /// Get the dispatchers to shutdown to meet the preemption requirement.
        /// If the requirement cannot be met before the dispatcher count is less than min units,
        /// it will set the cancel job to true.
        /// If the requirement is met before all idle dispatchers are shutdown
        /// it will set the resume flag to resume the stopped dispatchers.
        /// </summary>
        /// <param name="idleDispatchers">the idle dispatcher list</param>
        /// <param name="schedulerAdapter">the adapter to the scheduler</param>
        /// <param name="minDispatchers">the minimum dispatchers count</param>
        /// <param name="shouldCancelJob">true if we need cancel the job</param>
        /// <param name="shouldResumeRemaining">true if we need resume the dispatchers.</param>
        /// <returns>the dispatchers to shutdown</returns>
        private GracefulPreemptionResult GetDispatchersToShutdownAux(
            List<DispatcherInfo> idleDispatchers,
            int allowedCoreCount,
            bool fastBalance,
            bool balanceRequestRefreshed,
            int minDispatchers,
            IEnumerable<int> exitingTasks,
            IEnumerable<int> runningTasks,
            ref int activeDispatcherCount,
            out bool shouldCancelJob)
        {
            BrokerTracing.TraceVerbose("[GracefulPreemptionHandler].GetDispatchersToShutdownAux: fastBalance = {0}, balanceRequestRefreshed = {1}", fastBalance, balanceRequestRefreshed);
            shouldCancelJob = false;
            bool shouldResumeRemaining = false;

            int releaseIndex = 0;

            int totalDispatcherCores;
            int averageCoresPerDispatcher;
            IEnumerable<int> tasksInInterest = null;
            if (fastBalance)
            {
                tasksInInterest = exitingTasks;
            }
            this.dispatcherManager.GetCoreResourceUsageInformation(tasksInInterest, out averageCoresPerDispatcher, out totalDispatcherCores);

            int totalTaskCores;
            if (fastBalance)
            {
                //TODO: make it async
                totalTaskCores = totalDispatcherCores;
            }
            else
            {
                totalTaskCores = (exitingTasks.Count() + runningTasks.Count()) * averageCoresPerDispatcher;
                BrokerTracing.TraceVerbose(
                    "[GracefulPreemptionHandler].GetDispatchersToShutdownAux: totalTaskCores = {0}, totalDispatcherCores = {1}, dispatcherCount = {2}, maxAllowedCoreCount = {3}, minDispatcherCount = {4}",
                    totalTaskCores,
                    totalDispatcherCores,
                    activeDispatcherCount,
                    allowedCoreCount,
                    minDispatchers);
            }

            if (fastBalance && balanceRequestRefreshed)
            {
                int finishedCount = this.CheckAndFinishRunawayTasks(exitingTasks);
                BrokerTracing.TraceVerbose(
                    "[GracefulPreemptionHandler].GetDispatchersToShutdownAux: finished runaway tasks = {0}, totalTaskCores = {1}",
                    finishedCount,
                    totalTaskCores);
            }
            int coresToRelease = totalDispatcherCores > allowedCoreCount ? totalDispatcherCores - allowedCoreCount : totalTaskCores - allowedCoreCount;

            List<DispatcherInfo> plannedToShutdown = new List<DispatcherInfo>();

            for (releaseIndex = 0;
                releaseIndex < idleDispatchers.Count &&
                coresToRelease > 0;
                releaseIndex++)
            {
                var dispatcherInfo = idleDispatchers[releaseIndex];
                if (!exitingTasks.Contains(dispatcherInfo.UniqueId))
                {
                    // skip the tasks without ExitIfPossible flag.
                    continue;
                }

                coresToRelease -= dispatcherInfo.CoreCount;
                plannedToShutdown.Add(dispatcherInfo);

                if (dispatcherInfo.BlockRetryCount <= 0)
                {
                    // This is a young blocked dispatcher or active dispatcher.
                    activeDispatcherCount--;
                    // plannedAllowedCoreCount is 0 when the job is hold on
                    if (allowedCoreCount != 0 && activeDispatcherCount < minDispatchers)
                    {
                        BrokerTracing.TraceVerbose(
                            "[GracefulPreemptionHandler].GetDispatchersToShutdownAux: dispatcher count is less than the minimum, should cancel the job");

                        shouldCancelJob = true;

                        break;
                    }
                }

                BrokerTracing.TraceVerbose(
                    "[GracefulPreemptionHandler].GetDispatchersToShutdownAux: release dispatcher with Id = {0}, CoreCount = {1}. Remaining cores to release = {2}",
                    dispatcherInfo.UniqueId,
                    dispatcherInfo.CoreCount,
                    coresToRelease);
            }

            // plannedAllowedCoreCount is 0 when the job is hold on
            if (allowedCoreCount != 0 && coresToRelease <= 0)
            {
                BrokerTracing.TraceVerbose(
                    "[GracefulPreemptionHandler].GetDispatchersToShutdownAux: enough resources released, should resume the remaining dispatchers");

                shouldResumeRemaining = true;
            }

            bool noNewRequestInFastBalance = fastBalance && !balanceRequestRefreshed;
            // cancel means dispatcher count drops below minimum, but not enough cores released.
            // resume means enough cores released meets before the dispatcher count drops to below minimum.
            // otherwise, the preemption is on-going, which means all idle dispatchers released,
            // but not enough cores released, so we need continue push the dispatcher manager to
            // stop dispatchers to have more idle dispatchers in the next adjust pass.
            if (!shouldCancelJob && !shouldResumeRemaining && !noNewRequestInFastBalance)
            {
                // This mark won't shutdown the dispatcher, so it won't lead to cancel the job.
                this.MarkStoppingFlags(plannedToShutdown.Select(info => info.UniqueId), exitingTasks);
            }

            BrokerTracing.TraceVerbose(
                "[GracefulPreemptionHandler].GetDispatchersToShutdownAux: return dispatchers Count = {0}",
                plannedToShutdown.Count);

            var res = new GracefulPreemptionResult(plannedToShutdown, shouldResumeRemaining);
            if (fastBalance)
            {
                res.TaskIdsInInterest = exitingTasks;
            }

            return res;
        }

        /// <summary>
        /// Get possible runaway tasks to shutdown
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> GetTasksToShutdown()
        {
            BrokerTracing.TraceVerbose("[GracefulPreemptionHandler].GetTasksToShutdown: exitingTaskIds = {0}, runningTaskIds = {1}, BalanceInfo = {2}",
                this.exitingTaskIds.Count,
                this.runningTaskIds.Count,
                this.BalanceInfo.AllowedCoreCount);
            if (exitingTaskIds.Count + runningTaskIds.Count >= this.BalanceInfo.AllowedCoreCount)
            {
                return this.dispatcherManager.GetRunawayTasks(this.exitingTaskIds.Union(this.runningTaskIds));
            }
            return null;
        }

        /// <summary>
        /// Mark the dispatchers being preempted to stop 
        /// </summary>
        /// <param name="coresToRelease">the cores to release</param>
        /// <param name="plannedToShutdownTaskIds">the task ids planned to be shutdown</param>
        private void MarkStoppingFlags(IEnumerable<int> plannedToShutdownTaskIds, IEnumerable<int> exitingTaskIds)
        {
            BrokerTracing.TraceVerbose(
                "[GracefulPreemptionHandler].MarkStoppingFlags: stop the ExitIfPossible dispatchers, total count = {0}",
                exitingTaskIds.Count());

            // Occupied too much resource, stop the prefetch to release.
            this.dispatcherManager.StopDispatchers(exitingTaskIds.Union(plannedToShutdownTaskIds), null);
        }

        /// <summary>
        /// Mark all the dispatchers to stop 
        /// </summary>
        private void StopAllDispatchers()
        {
            BrokerTracing.TraceVerbose("[GracefulPreemptionHandler].StopAllDispatchers: stop all dispatchers");

            // Occupied too much resource, stop the prefetch to release.
            this.dispatcherManager.StopAllDispatchers();
        }

        /// <summary>
        /// Poll the scheduler for the newly published graceful preemption info.
        /// </summary>
        /// <param name="schedulerAdapter">the scheduler adapter</param>
        public async Task<bool> RefreshGracefulPreemptionInfo(ISchedulerAdapter schedulerAdapter)
        {
            BrokerTracing.TraceVerbose(
                "[GracefulPreemptionHandler].RefreshPreemptionRequest: query scheduler adapter");

            bool result;
            List<int> taskIds;
            List<int> runningTaskIds;
            BalanceInfo balanceInfo;
            (result, balanceInfo, taskIds, runningTaskIds) = await schedulerAdapter.GetGracefulPreemptionInfo(this.sessionId);
            
            if (result)
            {
                BrokerTracing.TraceVerbose(
                    "[GracefulPreemptionHandler].RefreshPreemptionRequest: get new request taskIds count = {0}, running taskIds count = {1}, maxAllowedCoreCount = {2}, fastBalance = {3}",
                    taskIds.Count,
                    runningTaskIds.Count,
                    balanceInfo.AllowedCoreCount,
                    balanceInfo.UseFastBalance);

                BrokerTracing.TraceVerbose(
                    "[GracefulPreemptionHandler].RefreshPreemptionRequest: replace old request taskIds count = {0}, maxAllowedCoreCount = {1}, fastBalance = {2}",
                    this.exitingTaskIds == null ? 0 : this.exitingTaskIds.Count,
                    this.BalanceInfo.AllowedCoreCount,
                    this.BalanceInfo.UseFastBalance);

                foreach (var request in balanceInfo.BalanceRequests)
                {
                    if (request.AllowedCoreCount <= NoPreemptionCores)
                    {
                        request.AllowedCoreCount = int.MaxValue;
                    }
                }

                this.BalanceInfo = balanceInfo;
                this.exitingTaskIds = new HashSet<int>(taskIds);
                this.runningTaskIds = new HashSet<int>(runningTaskIds);
                return true;
            }
            else
            {
                BrokerTracing.TraceVerbose(
                    "[GracefulPreemptionHandler].RefreshPreemptionRequest: Refresh failed.");
                return false;
            }
        }
        private int CheckAndFinishRunawayTasks(IEnumerable<int> taskIds)
        {
            int finishedTaskNumber = 0;

            // Mark a task reported by scheduler as a possible runaway task if one of the following conditions is met 
            // 1. Broker worker has never received its task event
            // 2. Corresponding dispatcher was already removed by broker worker
            // Do not use except and intersect for performance reason.
            var newPossible = taskIds.Where(id => !this.IsRecognizedTaskId(id)).Union(taskIds.Where(this.IsRemovedDispatcher)).ToList();
            foreach (var taskId in newPossible)
            {
                if (this.possibleRunaways.ContainsKey(taskId))
                {
                    this.possibleRunaways[taskId]++;
                }
                else
                {
                    this.possibleRunaways[taskId] = 1;
                }

                if (this.possibleRunaways[taskId] > MaxRunawayTolerance)
                {
                    ThreadPool.QueueUserWorkItem(
                        state =>
                            {
                                int id = (int)state;
                                BrokerTracing.TraceVerbose("[GracefulPreemptionHandler].CheckAndFinishRunawayTasks: finish runaway task {0}", id);
                                try
                                {
                                    this.finishRunawayTaskCallback(id);
                                }
                                catch (Exception e)
                                {
                                    BrokerTracing.TraceVerbose("[GracefulPreemptionHandler].CheckAndFinishRunawayTasks: finish runaway task {0} : exception {1}", id, e);
                                }
                            },
                        taskId);
                    finishedTaskNumber++;
                }
            }

            foreach (var stabled in this.possibleRunaways.Keys.Except(newPossible).ToArray())
            {
                this.possibleRunaways.Remove(stabled);
            }

            return finishedTaskNumber;
        }

        internal void AddToRecognizedTaskIds(IEnumerable<int> tasksIds)
        {
            this.recognizedTaskIds.UnionWith(tasksIds);
        }

        internal void AddToRemovedDispatcherIds(int taskId)
        {
            if (this.removedDispatcherIds.Add(taskId))
            {
                BrokerTracing.TraceInfo("Added task {0} to removedDispatcherIds list.", taskId);
            }
        }

        internal bool IsRemovedDispatcher(int taskId)
        {
            return this.removedDispatcherIds.Contains(taskId);
        }

        private bool IsRecognizedTaskId(int taskId)
        {
            return this.recognizedTaskIds.Contains(taskId);
        }
    }
}
