//------------------------------------------------------------------------------
// <copyright file="CommonSchedulerHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Helper class for operation to scheduler
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal.Common
{
    using System;
    using System.Diagnostics;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.AADAuthUtil;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler;

    /// <summary>
    /// Helper class for operation to scheduler
    /// </summary>
    internal static class CommonSchedulerHelper
    {
        /// <summary>
        /// Initializes a new connection to the scheduler and return the Scheduler object
        /// </summary>
        /// <returns>connection to the scheduler</returns>
        public static async Task<IScheduler> GetScheduler(CancellationToken token)
        {
            Exception exp = null;
            RetryManager retryManager = SoaHelper.GetDefaultExponentialRetryManager();
            while (retryManager.HasAttemptsLeft)
            {
                try
                {
                    IScheduler scheduler = new ThreadSafeScheduler(new Scheduler());

                    // move the resolve/retry logic to the TheadSafeScheduler wrapper
                    //string headnodeMachine = await fabricClient.QueryManager.ResolveSchedulerService(token);

                    // If we are on Azure, specify delegate to specify caller's identity when calling into 
                    //  the scheduler. Only admins can do this
                    if (SoaHelper.IsOnAzure())
                    {
                        TraceHelper.RuntimeTrace.LogEventTextVerbose("Before connect service as client");

                        //scheduler.ConnectServiceAsClient(headnodeMachine, GetCallerIdentity);

                        //TODO: SF: scheduler store should have internal resolve/connect retry
                        //await scheduler.ConnectServiceAsClientAsync(fabricClient, GetCallerIdentity, token);

                        TraceHelper.RuntimeTrace.LogEventTextVerbose("After connect service as client");
                    }
                    else
                    {
                        TraceHelper.RuntimeTrace.LogEventTextVerbose("Before connect service");

                        //pass the resolve func to scheduler which has internal resolve/connect retry
                        await scheduler.ConnectAsync(new SchedulerConnectionContext(HpcContext.Get()) { ServiceAsClient = true, IdentityProvider = ServiceAsClientProvider.DefaultIdentityProvider, PrincipalProvider = AadPrincipalProvider.DefaultAadPrincipalProvider }, HpcContext.Get().CancellationToken);

                        //await scheduler.ConnectServiceAsClientAsync(HpcContext.GetOrAdd(CancellationToken.None), ServiceAsClientProvider.DefaultIdentityProvider, null, null);
                        TraceHelper.RuntimeTrace.LogEventTextVerbose("After connect service");
                    }

                    return scheduler;
                }
                catch (Exception e)
                {
                    // TODO: Trace the exception in some common trace source
                    exp = e;
                    TraceHelper.RuntimeTrace.LogEventTextError(string.Format("[CommonSchedulerHelper]. GetScheduler: {0}", e));
                    await retryManager.AwaitForNextAttempt(token);
                }
            }

            throw exp;
        }

        /// <summary>
        /// Caller for Scheduler.ConnectAsService
        /// </summary>
        /// <returns></returns>
        private static string GetCallerIdentity()
        {
            string originalCallerIdentity = null;

            try
            {
                // Get operation context
                OperationContext oc = OperationContext.Current;

                // If there is an operation context we are in a WCF call from REST service or another WCF client
                if (oc != null)
                {
                    // See if it is the SOA REST service by checking for original caller custom header
                    try
                    {
                        originalCallerIdentity = oc.IncomingMessageHeaders.GetHeader<string>(Constant.WFE_Role_Caller_HeaderName, Constant.WFE_Role_Caller_HeaderNameSpace);
                    }
                    catch (Exception ex)
                    {
                        // Swallow exception if header is missing
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[CommonSchedulerHelper].GetCallerIdentity: Exception {0}", ex);
                    }

                    // If the original caller header was specified (by REST service)
                    if (!String.IsNullOrEmpty(originalCallerIdentity))
                    {
                        // The immediate caller (the REST service) must be autenticated and an admin
                        WindowsPrincipal immediateCallerPrincipal = new WindowsPrincipal(oc.ServiceSecurityContext.WindowsIdentity);
                        if (!oc.ServiceSecurityContext.WindowsIdentity.IsAuthenticated || !immediateCallerPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
                        {
                            return null;
                        }
                    }

                    // If the immediate caller was not the REST service, it is a WCF client within Azure
                    else
                    {
                        // If its operation context has an authenticated WindowsIdentity, use it as the original caller
                        if (oc.ServiceSecurityContext.WindowsIdentity.IsAuthenticated)
                        {
                            originalCallerIdentity = ParseUserName(oc.ServiceSecurityContext.WindowsIdentity.Name);
                        }
                    }
                }

                // If the caller isnt the REST service or an authenticated user, use current thread identity which is the process
                //  identity
                if (String.IsNullOrEmpty(originalCallerIdentity))
                {
                    WindowsIdentity identity = WindowsIdentity.GetCurrent();

                    // Otherwise use thread's current identity
                    if (identity != null)
                    {
                        originalCallerIdentity = ParseUserName(identity.Name);
                    }
                }

                TraceHelper.RuntimeTrace.LogEventTextVerbose(string.Format("[CommonSchedulerHelper]. Caller idenity = {0}", originalCallerIdentity));
            }

            catch (Exception e)
            {
                TraceHelper.RuntimeTrace.LogEventTextError(string.Format("[CommonSchedulerHelper]. GetCallerIdentity: {0}", e));
            }

            return originalCallerIdentity;
        }

        private static string ParseUserName(string fullUserName)
        {
            string[] userNameParts = fullUserName.Split('\\');

            // If no machine name/doman is specified
            if (userNameParts.Length == 1)
            {
                // User the entire user name
                return fullUserName;
            }

            // Otherwise pull the user name out
            else if (userNameParts.Length == 2)
            {
                return userNameParts[1];
            }

            else
            {
                throw new Exception(String.Format("Invalidate user name format - {0}", fullUserName));
            }
        }
    }
}
