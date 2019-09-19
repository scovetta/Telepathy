// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System.Collections.Generic;

    /// <summary>
    /// The interface of dispatcher manager.
    /// </summary>
    internal interface IDispatcherManager
    {
        /// <summary>
        /// Gets the current active dispatcher count.
        /// </summary>
        int ActiveDispatcherCount
        {
            get;
        }

        /// <summary>
        /// Stop a batch of dispatchers.
        /// </summary>
        /// <param name="taskIds">the id of tasks for dispatchers.</param>
        void StopDispatchers(IEnumerable<string> taskIds, IEnumerable<string> tasksInInterest, bool resumeRemaining = true);

        /// <summary>
        /// Stop all of the dispatchers.
        /// </summary>
        void StopAllDispatchers();

        /// <summary>
        /// Resume all stopped dispatchers.
        /// </summary>
        void ResumeDispatchers();

        /// <summary>
        /// Get the current resource usage state of the dispatchers.
        /// </summary>
        /// <param name="averageCoresPerDispatcher">average cores per dispatcher</param>
        /// <param name="totalCores">total used cores count</param>
        /// <param name="dispatcherCount">the dispatcher count</param>
        /// <remarks>Consider calc when dispatchers are added\removed</remarks>
        void GetCoreResourceUsageInformation(
            IEnumerable<string> tasksInInterest,
            out int averageCoresPerDispatcher,
            out int totalCores);

        /// <summary>
        /// Retrieve the possible runaway tasks
        /// </summary>
        /// <param name="totalTaskIds">total task Ids</param>
        /// <returns></returns>
        IEnumerable<string> GetRunawayTasks(IEnumerable<string> totalTaskIds);

    }
}
