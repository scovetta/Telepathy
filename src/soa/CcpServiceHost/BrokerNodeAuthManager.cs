// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.CcpServiceHosting
{
    using System;
    using System.Diagnostics;
    using System.Security.Principal;
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using RuntimeTraceHelper = Microsoft.Hpc.RuntimeTrace.TraceHelper;

    /// <summary>
    /// An authorization manager which only allows access from broker node or resources group computer accounts
    /// </summary>
    internal class BrokerNodeAuthManager : ServiceAuthorizationManager
    {
        /// <summary>
        /// If enables BrokerNodeAuthManager
        /// </summary>
        private bool enable;

        /// <summary>
        /// Stores the allowed user identity
        /// </summary>
        private WindowsIdentity allowedUser;

        /// <summary>
        /// Stores the user name of job owner
        /// </summary>
        private string jobOwnerUserName;

        /// <summary>
        /// Stores the job id.
        /// </summary>
        private string jobId;

        /// <summary>
        /// Initializes a new instance of the BrokerNodeAuthManager class.
        /// </summary>
        /// <param name="jobId">job id of the soa session</param>
        internal BrokerNodeAuthManager(string jobId)
        {
            this.jobId = jobId;

            if (SoaHelper.IsCurrentUserLocal() || WindowsIdentity.GetCurrent().IsSystem)
            {
                this.enable = false;
                RuntimeTraceHelper.TraceEvent(this.jobId, TraceEventType.Information, "[HpcServiceHost]: BrokerNodeAuthManager is disabled in non-domain joined compute node.");
            }
            else
            {
                this.enable = true;
                this.allowedUser = WindowsIdentity.GetCurrent();

                // If this environment does not exist, the jobOwnerUserName will remain null.
                // Thus the comparision will always fail so that HpcServiceHost will not authenticate job owner.
                this.jobOwnerUserName = Environment.GetEnvironmentVariable(Constant.JobOwnerNameEnvVar, EnvironmentVariableTarget.Process);

                RuntimeTraceHelper.TraceEvent(
                    this.jobId,
                    TraceEventType.Information,
                    "[HpcServiceHost]: BrokerNodeAuthManager initialized. AllowerUser = {0}, JobOwner = {1}",
                    this.allowedUser.Name,
                    this.jobOwnerUserName);
            }
        }

        /// <summary>
        /// Called to auth each request
        /// </summary>
        /// <param name="operationContext">Operation's context</param>
        /// <returns>pass validation or not</returns>
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            if (this.enable == false)
            {
                RuntimeTraceHelper.TraceEvent(
                    this.jobId,
                    TraceEventType.Verbose,
                    "[HpcServiceHost]: BrokerNodeAuthManager is disabled.");
                return true;
            }

            WindowsIdentity callerIdentity = null;
            bool result = SoaHelper.CheckWindowsIdentity(operationContext, out callerIdentity);

            if (result && callerIdentity == null)
            {
                // this code path is for Azure.
                return true;
            }

            if (callerIdentity == null || !result || operationContext.ServiceSecurityContext.IsAnonymous)
            {
                RuntimeTraceHelper.TraceEvent(this.jobId, TraceEventType.Warning, "[HpcServiceHost]: Access denied by BrokerNodeAuthManager. WindowsIdeneity is not recognized.");
                return false;
            }

            RuntimeTraceHelper.TraceEvent(this.jobId, TraceEventType.Verbose, "[HpcServiceHost]: received request from {0}", callerIdentity.Name);

            // if this is calling from local
            if (callerIdentity.IsSystem)
            {
                return true;
            }

            // Bug 11378: Authenticate job owner also for inprocess broker
            if (callerIdentity.Name.Equals(this.jobOwnerUserName, StringComparison.InvariantCultureIgnoreCase))
            {
                RuntimeTraceHelper.TraceEvent(
                    this.jobId,
                    TraceEventType.Verbose,
                    "[HpcServiceHost]: Authenticate job owner {0} for inprocess broker.",
                    this.jobOwnerUserName);

                return true;
            }

            // is this call from a BN
            if (SessionBrokerNodes.IsSessionBrokerNode(callerIdentity, this.jobId))
            {
                return true;
            }

            RuntimeTraceHelper.TraceEvent(
                this.jobId,
                TraceEventType.Warning,
                "[HpcServiceHost]: {0}/SID={1} is not a broker node",
                callerIdentity.Name,
                callerIdentity.User.Value);

            // Last see if the caller is the 'run as' user for the process. This is mainly needed for diag tests
            if (callerIdentity.User == this.allowedUser.User)
            {
                return true;
            }
            else
            {
                RuntimeTraceHelper.TraceEvent(
                    this.jobId,
                    TraceEventType.Warning,
                    "[HpcServiceHost]: Access denied by BrokerNodeAuthManager. {0} is not allowed.",
                    callerIdentity.User);
                return false;
            }
        }
    }
}
