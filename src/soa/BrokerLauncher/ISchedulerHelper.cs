// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal interface ISchedulerHelper : IDisposable
    {
        bool Enable { get; }

        /// <summary>
        /// Gets the head node
        /// </summary>
        string HeadNode { get; }

        /// <summary>
        /// a function help to judge whether a job purged.
        /// </summary>
        /// <param name="jobId">the job id.</param>
        /// <returns>a value indicating whether the specified job purged or not.</returns>
        Task<bool> IsJobPurged(string jobId);

        /// <summary>
        /// Update the broker info
        /// </summary>
        /// <param name="info">broker info</param>
        Task UpdateBrokerInfo(BrokerInfo info);

        /// <summary>
        /// Update the broker's suspended property
        /// </summary>
        /// <param name="sessionId">indicating the session ID</param>
        /// <param name="suspended">indicating the broker is suspended or not</param>
        Task UpdateSuspended(string sessionId, bool suspended);

        /// <summary>
        /// Restore broker recover info from scheduler
        /// </summary>
        /// <returns>list of broker recover info</returns>
        Task<BrokerRecoverInfo[]> LoadBrokerRecoverInfo();

        /// <summary>
        /// Try to get session start info from finished jobs
        /// </summary>
        /// <param name="sessionId">indicating the session Id</param>
        /// <returns>session start info</returns>
        Task<BrokerRecoverInfo> TryGetSessionStartInfoFromFininshedJobs(string sessionId);

        /// <summary>
        /// Gets ACL string for certain job template
        /// </summary>
        /// <param name="jobTemplate">indicating the job template</param>
        /// <returns>returns ACL string</returns>
        Task<string> GetJobTemplateACL(string jobTemplate);

        /// <summary>
        /// Gets job owner's sid
        /// </summary>
        /// <param name="jobId">indicating the job id</param>
        /// <returns>returns job owner's sid</returns>
        Task<string> GetJobOwnerSID(string jobId);

        /// <summary>
        /// Fail a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        Task FailJob(string jobid, string reason);

        /// <summary>
        /// Check if the soa diag trace enabled for the specified session.
        /// </summary>
        /// <param name="jobId">job id of the session</param>
        /// <returns>soa diag trace is enabled or disabled </returns>
        Task<bool> IsDiagTraceEnabled(string jobId);

        /// <summary>
        /// Dispose the scheduler helper
        /// Clean up the connection to scheduler
        /// </summary>
        void Dispose();

        /// <summary>
        /// Gets SOA configurations
        /// </summary>
        /// <param name="keys">indicating the keys</param>
        /// <returns>returns the values</returns>
        Task<Dictionary<string, string>> GetSOAConfigurations(List<string> keys);

        /// <summary>
        /// Get non terminated session.
        /// </summary>
        /// <returns>non terminated session dic</returns>
        Task<Dictionary<int, int>> GetNonTerminatedSession();

        /// <summary>
        /// Gets cluster info
        /// </summary>
        /// <param name="keys">indicating the keys</param>
        /// <returns>returns the values</returns>
        Task<ClusterInfoContract> GetClusterInfoAsync();
    }
}