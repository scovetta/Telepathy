//------------------------------------------------------------------------------
// <copyright file="AzureStorageCleaner.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Cleanup the Azure storage which stores request and response messages
//      for Azure burst.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    /// <summary>
    /// Cleanup the Azure storage which stores request and response messages
    /// for Azure burst.
    /// </summary>
    internal class AzureStorageCleaner : DisposableObject
    {
        /// <summary>
        /// Azure storage queue/container name's regular expression.
        /// </summary>
        /// <remarks>
        /// Format: hpcsoa-[hash]-response-[SessionId]-[RequeueCount]
        /// </remarks>
        private const string ResponseStorageNameRegexString = @"^{0}-(?<{1}>\d+)-(?<{2}>\d+)$";

        /// <summary>
        /// The name regex of the Azure storage queues and blob containers for the http clients
        /// </summary>
        /// <remarks>
        /// format: hpcsoa-[ClusterHash]-[SessionId]-<request>/<response-[SessionHash]>
        /// </remarks>
        private const string AzureQueueStorageNameRegexString = @"^{0}(?<{1}>\d+)-(request|response-\d+)$";

        /// <summary>
        /// Session Id group name in the regular expression.
        /// </summary>
        private const string SessionIdGroupName = "SessionId";

        /// <summary>
        /// Job requeue count group name in the regular expression.
        /// </summary>
        private const string RequeueCountGroupName = "RequeueCount";

        /// <summary>
        /// Timer period.
        /// </summary>
        private static readonly TimeSpan TimerPeriod = TimeSpan.FromHours(1);

        /// <summary>
        /// Default retry policy for the storage operation.
        /// </summary>
        private static readonly IRetryPolicy DefaultRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), 3);

        /// <summary>
        /// This timer triggers storage cleanup.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The scheduler helper
        /// </summary>
        private SchedulerHelper helper;

        /// <summary>
        /// Initializes a new instance of the AzureStorageCleaner class.
        /// </summary>
        public AzureStorageCleaner(SchedulerHelper helper)
        {
            this.helper = helper;
        }

        /// <summary>
        /// Start the cleaner.
        /// </summary>
        public void Start()
        {
            this.timer = new Timer(this.TimerCallback, null, TimeSpan.Zero, TimerPeriod);
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.timer != null)
            {
                try
                {
                    this.timer.Dispose();
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[AzureStorageCleaner].Dispose: Disposing timer failed, {0}",
                        e);
                }

                this.timer = null;
            }
        }

        /// <summary>
        /// Callback method of the timer.
        /// </summary>
        /// <param name="state">state object</param>
        private void TimerCallback(object state)
        {
            try
            {
                this.Cleanup().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                    "[AzureStorageCleaner].TimerCallback: Cleanup failed, {0}",
                    e);
            }
        }

        /// <summary>
        /// Cleanup the Azure storage including both queue and blob.
        /// </summary>
        private async Task Cleanup()
        {
            BrokerTracing.TraceVerbose(
                "[AzureStorageCleaner].Cleanup: Try to cleanup the Azure storage.");
            ClusterInfoContract clusterInfo = await this.helper.GetClusterInfoAsync();
            string clusterName = clusterInfo.ClusterName;
            Guid clusterId;
            if (!Guid.TryParse(clusterInfo.ClusterId, out clusterId))
            {
                BrokerTracing.TraceError(
                    "[AzureStorageCleaner].Cleanup: clusterInfo.ClusterId is not a valid GUID string.");
                throw new ArgumentException("clusterInfo.ClusterId", "clusterInfo.ClusterId is not a valid GUID string.");
            }
            var connectionString = clusterInfo.AzureStorageConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                BrokerTracing.TraceVerbose(
                    "[AzureStorageCleaner].Cleanup: Azure storage connection string is not set.");

                // no need to do anything if the connection string is not set
                return;
            }

            string prefix = SoaHelper.GetResponseStoragePrefix(clusterId.ToString());
            string prefixAQ = SoaHelper.GetAzureQueueStoragePrefix(clusterId.ToString().ToLower().GetHashCode());

            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            CloudQueueClient queueClient = account.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = DefaultRetryPolicy;

            var queues = queueClient.ListQueues(prefix);
            var queuesAQ = queueClient.ListQueues(prefixAQ);

            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = DefaultRetryPolicy;
            var containers = blobClient.ListContainers(prefix, ContainerListingDetails.None, null, null);
            var containersAQ = blobClient.ListContainers(prefixAQ, ContainerListingDetails.None, null, null);
            Dictionary<int, int> nonTerminatedSession;

            if (queues.Count<CloudQueue>() > 0 || containers.Count<CloudBlobContainer>() > 0 ||
                queuesAQ.Count<CloudQueue>() > 0 || containersAQ.Count<CloudBlobContainer>() > 0)
            {
                // if there are queue/container candidates for deleting, get
                // following info from session service
                nonTerminatedSession = await this.helper.GetNonTerminatedSession(); 
            }
            else
            {
                return;
            }

            // cleanup storage queue
            foreach (CloudQueue queue in queues)
            {
                BrokerTracing.TraceVerbose(
                    "[AzureStorageCleaner].Cleanup: Azure storage queue name is {0}",
                    queue.Name);

                if (this.IsSessionTerminated(nonTerminatedSession, prefix, queue.Name))
                {
                    try
                    {
                        queue.Delete();
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[AzureStorageCleaner].Cleanup: Deleting queue {0} failed, {1}",
                            queue.Name,
                            e);
                    }
                }
            }

            // cleanup storage blob container
            foreach (CloudBlobContainer container in containers)
            {
                BrokerTracing.TraceVerbose(
                    "[AzureStorageCleaner].Cleanup: Azure storage container name is {0}",
                    container.Name);

                if (this.IsSessionTerminated(nonTerminatedSession, prefix, container.Name))
                {
                    try
                    {
                        container.Delete();
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[AzureStorageCleaner].Cleanup: Deleting container {0} failed, {1}",
                            container.Name,
                            e);
                    }
                }
            }

            // cleanup storage queue for the http clients
            foreach (CloudQueue queue in queuesAQ)
            {
                BrokerTracing.TraceVerbose(
                    "[AzureStorageCleaner].Cleanup: Azure storage queue name is {0}",
                    queue.Name);

                if (this.IsSessionTerminatedAQ(nonTerminatedSession, prefixAQ, queue.Name))
                {
                    try
                    {
                        queue.Delete();
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[AzureStorageCleaner].Cleanup: Deleting queue {0} failed, {1}",
                            queue.Name,
                            e);
                    }
                }
            }

            // cleanup storage blob container for the http clients
            foreach (CloudBlobContainer container in containersAQ)
            {
                BrokerTracing.TraceVerbose(
                    "[AzureStorageCleaner].Cleanup: Azure storage container name is {0}",
                    container.Name);

                if (this.IsSessionTerminatedAQ(nonTerminatedSession, prefixAQ, container.Name))
                {
                    try
                    {
                        container.Delete();
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceError(
                            "[AzureStorageCleaner].Cleanup: Deleting container {0} failed, {1}",
                            container.Name,
                            e);
                    }
                }
            }

        }

        /// <summary>
        /// Check if the session terminated. Considering requeued job, if the
        /// requeue count is less than current value, it is also terminated.
        /// </summary>
        /// <param name="nonTerminatedSession">
        /// dictionary for the non-terminated session
        /// </param>
        /// <param name="prefix">queue/container name prefix</param>
        /// <param name="storageName">queue/container name</param>
        /// <returns>terminated or not</returns>
        private bool IsSessionTerminated(Dictionary<int, int> nonTerminatedSession, string prefix, string storageName)
        {
            Regex regex = new Regex(
                string.Format(ResponseStorageNameRegexString, prefix, SessionIdGroupName, RequeueCountGroupName),
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            Match match = regex.Match(storageName);

            if (!match.Success)
            {
                return false;
            }

            string sessionIdStr = match.Groups[SessionIdGroupName].Value;

            int sessionId;

            if (!int.TryParse(sessionIdStr, out sessionId))
            {
                return false;
            }

            string requeueCountStr = match.Groups[RequeueCountGroupName].Value;

            int requeueCount;

            if (!int.TryParse(requeueCountStr, out requeueCount))
            {
                return false;
            }

            BrokerTracing.TraceVerbose(
                "[AzureStorageCleaner].IsSessionTerminated: Get session Id and requeue count from storage name, session Id={0}, requeue count={1}",
                sessionId,
                requeueCount);

            int currentRequeueCount;

            if (nonTerminatedSession.TryGetValue(sessionId, out currentRequeueCount))
            {
                BrokerTracing.TraceVerbose(
                    "[AzureStorageCleaner].IsSessionTerminated: Find session info from dic, session Id={0}, current requeue count={1}",
                    sessionId,
                    currentRequeueCount);

                if (requeueCount < currentRequeueCount)
                {
                    // if requeue count is less than current value, the session
                    // is terminated
                    return true;
                }
            }
            else
            {
                BrokerTracing.TraceVerbose(
                    "[AzureStorageCleaner].IsSessionTerminated: Session {0} does not exist in the dic.",
                    sessionId);

                // if session Id doesn't exist in the dic, the session is
                // already terminated
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the session terminated. 
        /// </summary>
        /// <param name="nonTerminatedSession">
        /// dictionary for the non-terminated session
        /// </param>
        /// <param name="prefix">queue/container name prefix</param>
        /// <param name="storageName">queue/container name</param>
        /// <returns>terminated or not</returns>
        private bool IsSessionTerminatedAQ(Dictionary<int, int> nonTerminatedSession, string prefix, string storageName)
        {
            Regex regex = new Regex(
                string.Format(AzureQueueStorageNameRegexString, prefix, SessionIdGroupName),
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            Match match = regex.Match(storageName);

            if (!match.Success)
            {
                return false;
            }

            string sessionIdStr = match.Groups[SessionIdGroupName].Value;

            int sessionId;

            if (!int.TryParse(sessionIdStr, out sessionId))
            {
                return false;
            }
           
            BrokerTracing.TraceVerbose(
                "[AzureStorageCleaner].IsSessionTerminated: Get session Id from storage name, session Id={0}",
                sessionId);

            if (nonTerminatedSession.Keys.Contains(sessionId))
            {
                BrokerTracing.TraceVerbose(
                    "[AzureStorageCleaner].IsSessionTerminated: Find session info from dic, session Id={0}",
                    sessionId);

                return false;
            }
            else
            {
                BrokerTracing.TraceVerbose(
                    "[AzureStorageCleaner].IsSessionTerminated: Session {0} does not exist in the dic.",
                    sessionId);

                // if session Id doesn't exist in the dic, the session is
                // already terminated
                return true;
            }

        }
    }
}
