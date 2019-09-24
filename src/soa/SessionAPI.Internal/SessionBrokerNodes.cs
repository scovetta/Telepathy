// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Principal;

    using RuntimeTraceHelper = Microsoft.Telepathy.RuntimeTrace.TraceHelper;

    /// <summary>
    /// Saves the broker node(s) associated to a sessions in job env vars
    /// </summary>
    public static class SessionBrokerNodes
    {
        private const string BrokerCountEnvVarName = "WCF_BROKERNODE_COUNT";

        private const string BrokerNameEnvVarNameTemplate = "WCF_BROKERNODE_{0}";

        private static List<SecurityIdentifier> brokerNodeIdentityCache = null;

        private static object brokerNodeIdentityCacheLock = new object();

#if HPCPACK
        /// <summary>
        /// Set the broker nodes for the session
        /// </summary>
        /// <param name="job">Session's job</param>
        /// <param name="nodeNames">Broker node names</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared by multiple projects")]
        internal static void SetSessionBrokerNodes(ISchedulerJob schedulerJob, ICollection<string> nodeSSDLs)
        {
            // Add the broker nodes available to the session as env variables so that services only
            // process requests from BNs
            int i = 0;
            foreach (string ssdl in nodeSSDLs)
            {
                schedulerJob.SetEnvironmentVariable(string.Format(BrokerNameEnvVarNameTemplate, ++i), ssdl);
            }

            schedulerJob.SetEnvironmentVariable(BrokerCountEnvVarName, nodeSSDLs.Count.ToString());
        }
#endif


        /// <summary>
        /// Returns whether specified broker node is associated with the session
        /// </summary>
        /// <param name="identity">The broker node as a computer account</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared by multiple projects")]
        public static bool IsSessionBrokerNode(WindowsIdentity identity, string jobId)
        {
            // BUG 8419 : Cache BN SecurityIdentiy objects to improve latency
            if (!CacheBrokerNodeIdentities(jobId))
            {
                return false;
            }

            if (!brokerNodeIdentityCache.Contains(identity.User))
            {
                // dump SID list
                string strSIDList = string.Empty;
                foreach (SecurityIdentifier sid in brokerNodeIdentityCache)
                {
                    strSIDList += sid.Value;
                    strSIDList += ",";
                }

                RuntimeTraceHelper.TraceEvent(
                    jobId,
                    TraceEventType.Error,
                    "[HpcServiceHost]: SessionBrokerNodes.IsSessionBrokerNode: {0} is not in cached node list: {1}",
                    identity.User.Value,
                    strSIDList);

                return false;
            }
            return true;
        }

        /// <summary>
        /// Returned broker node identity cache
        /// </summary>
        /// <returns></returns>
        private static bool CacheBrokerNodeIdentities(string jobId)
        {
            bool ret = true;

            if (brokerNodeIdentityCache == null)
            {
                lock (brokerNodeIdentityCacheLock)
                {
                    if (brokerNodeIdentityCache == null)
                    {
                        try
                        {
                            IDictionary envVars = Environment.GetEnvironmentVariables();

                            // Extract the number of BNs
                            string nodeCountStr = (string)envVars[BrokerCountEnvVarName];

                            int nodeCount = 0;
                            if (string.IsNullOrEmpty(nodeCountStr) || !int.TryParse(nodeCountStr, out nodeCount))
                            {
                                RuntimeTraceHelper.TraceEvent(
                                    jobId,
                                    TraceEventType.Error,
                                    "[HpcServiceHost]: SessionBrokerNodes.CacheBrokerNodeIdentities: return false because nodeCountStr={0}",
                                    nodeCountStr);

                                return false;
                            }

                            if (nodeCount <= 0)
                            {
                                RuntimeTraceHelper.TraceEvent(
                                    jobId,
                                    TraceEventType.Error,
                                    "[HpcServiceHost]: SessionBrokerNodes.CacheBrokerNodeIdentities: return false because nodeCount={0}",
                                    nodeCount);

                                return false;
                            }

                            // Enum each BN and create a SecurityIdentifier for it
                            brokerNodeIdentityCache = new List<SecurityIdentifier>();

                            for (int i = 0; i < nodeCount; i++)
                            {
                                string nodeSSDL = (string)envVars[string.Format(BrokerNameEnvVarNameTemplate, i + 1)];

                                // Create BN identity using SecurityIdentifier. This ensures correct comparison (including same domain - Bug 6787). 
                                SecurityIdentifier brokerNodeIdentity = new SecurityIdentifier(nodeSSDL);
                                brokerNodeIdentityCache.Add(brokerNodeIdentity);

                                RuntimeTraceHelper.TraceEvent(
                                    jobId,
                                    TraceEventType.Verbose,
                                    "[HpcServiceHost]: SessionBrokerNodes.CacheBrokerNodeIdentities: cached node: node SSDL={0}, SID={1}",
                                    nodeSSDL,
                                    brokerNodeIdentity.Value);
                            }
                        }

                        catch (Exception e)
                        {
                            RuntimeTraceHelper.TraceEvent(
                                jobId,
                                TraceEventType.Error,
                                "[HpcServiceHost]: Fail to cache broker node identities for broker node authentication. - {0}",
                                e);

                            ret = false;
                        }
                    }
                }
            }

            return ret;
        }
    }
}
