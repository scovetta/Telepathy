//------------------------------------------------------------------------------
// <copyright file="BrokerManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Manager for all broker app domains
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Rest;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Hpc.ServiceBroker.Common;

    /// <summary>
    /// Manager for all broker app domains
    /// </summary>
    /// <remarks>
    /// Locking order:
    /// 1. lock the brokerDic first and get the entry and lock it within the lock
    /// 2. if entry is not disposed, do staff to the entry
    /// 3. lock the brokerDic and unlock the entry
    /// </remarks>
    internal sealed class BrokerManager : ISessionUserAuthenticator, IDisposable
    {
        /// <summary>
        /// Stores fail reason for failing interactive service job if broker worker is dead
        /// </summary>
        private const string FailInteractiveServiceJobBecauseBrokerWorkerDied = "Service job for an interactive session failed due to its correspnding broker worker process unexpectedly terminating.";

        /// <summary>
        /// Store the recover broker retry limit
        /// </summary>
        private const int RecoverBrokerRetryLimit = 3;

        /// <summary>
        /// Store the retry period
        /// </summary>
        private const int RetryPeriod = 10 * 1000; // 60 * 1000;

        /// <summary>
        /// Stores the retry period for attaching session
        /// </summary>
        private const int AttachSessionRetryPeriod = 3000;

        /// <summary>
        /// Gets the watch period
        /// </summary>
        private const int WatchPeriod = 60 * 1000;

        /// <summary>
        /// the default stale session data cleanup period is 24 hours.
        /// </summary>
        private const int StaleSessionCleanupPeriod = 24 * 60 * 60 * 1000;

        /// <summary>
        /// default update queue length period is 15 seconds.
        /// </summary>
        private const int UpdateQueueLengthPeriod = 15 * 1000;

        /// <summary>
        /// Stores the timeout for waiting process exit after BrokerUnloading is received
        /// </summary>
        private static readonly TimeSpan TimeoutForWaitingProcessExit = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Stores the dictionary of broker info, keyed by session id
        /// </summary>
        private volatile Dictionary<int, BrokerInfo> brokerDic;

        /// <summary>
        /// Stores the request queue length counter
        /// </summary>
        private PerformanceCounter requestQueueLengthCounter;

        /// <summary>
        /// Stores the response queue length counter
        /// </summary>
        private PerformanceCounter responseQueueLengthCounter;

        /// <summary>
        /// Connection status to the scheduler
        /// </summary>
        private volatile bool connected;

        /// <summary>
        /// Stores the scheduler helper
        /// </summary>
        private volatile ISchedulerHelper schedulerHelper;

        /// <summary>
        /// the time to cleanup the stale session data.
        /// </summary>
        private volatile Timer staleSessionCleanupTimer;

        /// <summary>
        /// timer to update queue length timer
        /// </summary>
        private volatile Timer updateQueueLengthTimer;

        /// <summary>
        /// the headnode name
        /// </summary>
        private volatile string headnode;

        /// <summary>
        /// recover thread
        /// </summary>
        //private Thread RecoverThread;

        /// <summary>
        /// recover task
        /// </summary>
        private Task RecoverTask;

        /// <summary>
        /// cancellation token source
        /// </summary>
        private CancellationTokenSource ts;

        /// <summary>
        /// Stores the broker process pool
        /// </summary>
        private volatile BrokerProcessPool pool;

        /// <summary>
        /// Stores the fabric cluster context;
        /// </summary>
        private IHpcContext context;

        /// <summary>
        /// Initializes a new instance of the BrokerManager class
        /// </summary>
        public BrokerManager(bool needRecover, IHpcContext context)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[BrokerManager] Constructor: needRecover={0}", needRecover);
            this.headnode = SoaHelper.GetSchedulerName(false);

            this.context = context;

            this.brokerDic = new Dictionary<int, BrokerInfo>();

            this.staleSessionCleanupTimer = new Timer(this.CleanStaleSessionData, null, Timeout.Infinite, Timeout.Infinite);
#if HPCPACK
            if (!SoaHelper.IsOnAzure())
            {
                // TODO: on azure, about the MSMQ. Don't use the MSMQ in the Azure cluster.
                this.updateQueueLengthTimer = new Timer(this.CallbackToUpdateMSMQLength, null, Timeout.Infinite, Timeout.Infinite);
            }
#endif

            this.pool = new BrokerProcessPool();

            if (needRecover && !BrokerLauncherEnvironment.Standalone)
            {
                this.ts = new CancellationTokenSource();
                CancellationToken ct = ts.Token;
                this.RecoverTask = Task.Run(async () => await this.RecoverThreadProc(ct), ct);
            }
        }

        /// <summary>
        /// Finalizes an instance of the BrokerManager class
        /// </summary>
        ~BrokerManager()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// the active broker number
        /// </summary>
        public int BrokerCount
        {
            get
            {
                lock (this.brokerDic)
                {
                    return this.brokerDic.Count;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the broker manager has connected to the scheduler
        /// </summary>
        public bool Connected
        {
            get { return this.connected; }
        }

        /// <summary>
        /// Create a new broker application domain
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionid">session id</param>
        /// <param name="durable">indicate if the session is durable</param>
        /// <returns>returns broker initialization result</returns>
        public async Task<BrokerInitializationResult> CreateNewBrokerDomain(SessionStartInfoContract info, int sessionid, bool durable)
        {
            string userName =
                (OperationContext.Current.ServiceSecurityContext != null && OperationContext.Current.ServiceSecurityContext.WindowsIdentity != null) ?
                OperationContext.Current.ServiceSecurityContext.WindowsIdentity.Name :
                String.Empty;
            TraceHelper.RuntimeTrace.LogSessionCreating(sessionid, userName);
            TraceHelper.TraceEvent(sessionid, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Create new broker domain: {0}", sessionid);

            BrokerRecoverInfo recoverInfo = new BrokerRecoverInfo();
            recoverInfo.StartInfo = info;
            recoverInfo.SessionId = sessionid;
            recoverInfo.Durable = durable;
            if (this.schedulerHelper == null)
            {
                this.schedulerHelper = SchedulerHelperFactory.GetSchedulerHelper(this.context);
            }

            ClusterInfoContract clusterInfo = await this.schedulerHelper.GetClusterInfoAsync();
            return await this.CreateBrokerAndRun(recoverInfo, false, clusterInfo);
        }

        /// <summary>
        /// Close the broker domain
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        public async Task CloseBrokerDomain(int sessionId)
        {
            TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Close broker {0}", sessionId);

            BrokerInfo info = null;
            lock (this.brokerDic)
            {
                this.brokerDic.TryGetValue(sessionId, out info);
            }
            
            if (info != null)
            {
                info.CheckAccess();
                await this.CleanupAsync(sessionId, false);
            }
            else
            {
                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Broker {0} didn't found. Maybe it has already been closed.", sessionId);
            }
        }

        /// <summary>
        /// Returns whether specified broker is loaded
        /// </summary>
        /// <returns></returns>
        public bool DoesBrokerExist(int sessionId, out string brokerWorkerUniqueId)
        {
            lock (this.brokerDic)
            {
                BrokerInfo info;
                if (this.brokerDic.TryGetValue(sessionId, out info))
                {
                    brokerWorkerUniqueId = info.UniqueId;
                    return true;
                }
                else
                {
                    brokerWorkerUniqueId = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Attach to a existing broker
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <returns>returns initialization result</returns>
        public async Task<BrokerInitializationResult> AttachBroker(int sessionId)
        {
            BrokerInfo info;
            BrokerInitializationResult result = null;
            Exception lastException = null;

            TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Client attached");

            for (int i = 0; i < RecoverBrokerRetryLimit; i++)
            {
                // Try to find broker that is still running
                bool success;
                lock (this.brokerDic)
                {
                    success = this.brokerDic.TryGetValue(sessionId, out info);
                    if (success)
                    {
                        Monitor.Enter(info);
                    }
                }

                if (success)
                {
                    try
                    {
                        info.CheckAccess();
                        TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Attaching exsiting broker: {0}", sessionId);
                        if (info.Disposed)
                        {
                            TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Broker is exiting...");
                            ThrowHelper.ThrowSessionFault(SOAFaultCode.Session_ValidateJobFailed_AlreadyFinished, SR.BrokerFinishing, sessionId.ToString());
                        }
                        else
                        {
                            bool needRestart = false;
                            try
                            {
                                info.Attach();
                            }
                            catch (EndpointNotFoundException e)
                            {
                                // Bug 8236: Need to catch EndpointNotFoundException and try to recover and retry attaching.
                                TraceHelper.TraceEvent(sessionId, TraceEventType.Warning, "[BrokerManager] Attach failed with EndpointNotFoundException, broker might be unloading. Will wait for broker exit and try raise it again. Exception: {0}", e);

                                // Wait until the process is exited and all event are finished
                                // This means that this broker info instance is removed from brokerDic so that we can start from create broker for attaching
                                info.WaitForProcessExit(TimeoutForWaitingProcessExit);
                                TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[BrokerManager] Broker process is exited and all events are finished, restart that broker for attaching");
                                needRestart = true;
                            }
                            catch (FaultException<SessionFault> e)
                            {
                                if (e.Detail.Code == (int)SOAFaultCode.Broker_BrokerSuspending)
                                {
                                    TraceHelper.TraceEvent(sessionId, TraceEventType.Warning, "[BrokerManager] Attach failed, broker is unloading to suspended state. Will wait for broker exit and try raise it again.");

                                    // Wait until the process is exited and all event are finished
                                    // This means that this broker info instance is removed from brokerDic so that we can start from create broker for attaching
                                    info.WaitForProcessExit(TimeoutForWaitingProcessExit);
                                    TraceHelper.TraceEvent(sessionId, TraceEventType.Information, "[BrokerManager] Broker process is exited and all events are finished, restart that broker for attaching");
                                    needRestart = true;
                                }
                                else
                                {
                                    TraceHelper.TraceEvent(sessionId, TraceEventType.Error, "[BrokerManager] Attach failed: {0}", e);
                                    throw;
                                }
                            }

                            if (!needRestart)
                            {

                                //TODO: check whether need to obtain the cluster id, hash and Azure storage SAS here.

                                return info.InitializationResult;
                            }
                        }
                    }
                    finally
                    {
                        Monitor.Exit(info);
                    }
                }

                // Try to find service job from finished jobs.
                // If no such service job is found, exception will throw by the scheduler helper and back to the client
                BrokerRecoverInfo recoverInfo = await this.schedulerHelper.TryGetSessionStartInfoFromFininshedJobs(sessionId);
                ClusterInfoContract clusterInfo = await this.schedulerHelper.GetClusterInfoAsync();
                try
                {
                    result = await this.CreateBrokerAndRun(recoverInfo, true, clusterInfo);
                }
                catch (FaultException<SessionFault> e)
                {
                    if (e.Detail.Code == SOAFaultCode.Broker_SessionIdAlreadyExists)
                    {
                        // Bug 9840: This exception means that someone already raised up the broker
                        // Should goto the very beginning to load initialization result
                        lastException = e;

                        // TODO: We don't know if this retry period is enough
                        // We need to investigate this more in SP2 and we might
                        // need an event wait handle to synchronize these rather
                        // than a retry period
                        await Task.Delay(AttachSessionRetryPeriod);
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (this.IsCallingFromHeadNode(OperationContext.Current.ServiceSecurityContext.WindowsIdentity))
                {
                    TraceHelper.RuntimeTrace.LogSessionRaisedUpFailover(sessionId);
                }
                else
                {
                    TraceHelper.RuntimeTrace.LogSessionRaisedUp(sessionId, OperationContext.Current.ServiceSecurityContext.WindowsIdentity.Name);
                }

                lastException = null;
                break;
            }

            if (lastException == null)
            {
                return result;
            }
            else
            {
                throw lastException;
            }
        }

        /// <summary>
        /// Close the broker manager
        /// </summary>
        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Gets the active broker id list
        /// </summary>
        /// <returns>the list of active broker's session id</returns>
        public int[] GetActiveBrokerIdList()
        {
            lock (this.brokerDic)
            {
                int[] result = new int[this.brokerDic.Count];
                this.brokerDic.Keys.CopyTo(result, 0);
                return result;
            }
        }

        /// <summary>
        /// Authenticate the incoming user as a valid HPC user for the session
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="caller">indicating the windows identity</param>
        /// <returns>
        /// returns a flag indicating whether the authentication succeeded
        /// </returns>
        public bool AuthenticateUser(int sessionId, WindowsIdentity caller)
        {
            try
            {
                if (this.IsCallingFromHeadNode(caller))
                {
                    return true;
                }
                else
                {
                    return caller.IsAuthenticated;
                }
            }
            catch (Exception e)
            {
                // print trace here and rethrow the exception.
                TraceHelper.TraceEvent(
                    sessionId,
                    TraceEventType.Error,
                    "[BrokerManager] AuthenticateUser: Failed to authenticate user, {0}", e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Dispose the broker manager
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Revert the change done by CreateDomainAndRun
        /// </summary>
        /// <param name="info">indicate the broker info</param>
        /// <param name="suspended">indicating whether revert to suspended or not</param>
        private static void RevertCreateDomainAndRun(BrokerInfo info, bool suspended)
        {
            if (info == null)
            {
                return;
            }

            try
            {
                info.CloseBroker(suspended);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(info.SessionId, System.Diagnostics.TraceEventType.Error, "[BrokerManager] RevertCreateDomainAndRun: Failed to close the entry: {0}", e);
            }
        }

        /// <summary>
        /// Gets the custom broker registration from service registration file
        /// </summary>
        /// <param name="serviceRegistrationPath">indicating the path of the service registration file</param>
        /// <returns>returns the instance of CustomBrokerRegistration class</returns>
        private static CustomBrokerRegistration GetCustomBroker(string serviceRegistrationPath)
        {
            try
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = serviceRegistrationPath;
                Configuration config = null;
                RetryManager.RetryOnceAsync(
                        () => config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None),
                        TimeSpan.FromSeconds(1),
                        ex => ex is ConfigurationErrorsException)
                    .GetAwaiter()
                    .GetResult();
                BrokerConfigurations brokerConfig = BrokerConfigurations.GetSectionGroup(config);
                if (brokerConfig == null)
                {
                    return null;
                }
                else
                {
                    return brokerConfig.CustomBroker;
                }
            }
            catch (ConfigurationErrorsException e)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.ConfigFile_Invalid,
                                              "{0}",
                                              e.ToString());

                return null;
            }
        }

        /// <summary>
        /// Do the clean up work
        /// </summary>
        /// <param name="sessionId">indicate the session Id</param>
        /// <param name="suspend">indicate whether the broker is suspended</param>
        private async Task CleanupAsync(int sessionId, bool suspend)
        {
            // lock the broker dic to remove the info
            // if no such broker info is found, return because somebody has done the cleanup.
            BrokerInfo info;
            lock (this.brokerDic)
            {
                if (this.brokerDic.TryGetValue(sessionId, out info))
                {
                    //TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Cleanup: Start clean up operation. Current Stack = {0}", Environment.StackTrace);
                    TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Cleanup: Start clean up operation");

                    // Hold the broker info lock within the broker dic lock
                    Monitor.Enter(info);

                    // If the broker has already been disposed, release the lock and quit
                    // This is possible if another client is calling CloseBroker at roughtly the same time and managed to dispose
                    // the BrokerInfo first
                    if (info.Disposed)
                    {
                        Monitor.Exit(info);
                        TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Cleanup: Broker {0} has already been closed.", sessionId);
                        return;
                    }

                    // Set the flag
                    info.Disposed = true;
                }
                else
                {
                    TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Cleanup: Broker {0} has already been closed.", sessionId);
                    return;
                }
            }

            int step = 0;

            try
            {
                // lock info to prevent somebody is attaching this broker at the same time
                if (info.Durable)
                {
                    try
                    {
                        //no await since there is lock
                        this.schedulerHelper.UpdateSuspended(sessionId, suspend).GetAwaiter().GetResult();
                        TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Cleanup: Step {0}: Update suspended succeeded.", ++step);
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Error, "[BrokerManager] Cleanup: Step {0}: Update suspended failed: {1}", ++step, e);
                    }
                }

                try
                {
                    info.CloseBroker(suspend);
                    TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Cleanup: Step {0}: Close broker succeeded", ++step);
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Error, "[BrokerManager] Cleanup: Step {0}: Close broker failed: {1}", ++step, e);
                }
            }
            finally
            {
                // Release the lock after closing procedure is over
                Monitor.Exit(info);
            }

            TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Clean up broker {0} finished, Suspended = {1}", sessionId, suspend);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Create a broker appdomain
        /// </summary>
        /// <param name="recoverInfo">broker recover info</param>
        /// <param name="sessionid">session id</param>
        /// <param name="durable">indicate if the session is durable</param>
        /// <param name="attached">indicate if it is attaching</param>
        /// <returns>returns the initialization result</returns>
        private async Task<BrokerInitializationResult> CreateBrokerAndRun(BrokerRecoverInfo recoverInfo, bool attached, ClusterInfoContract clusterInfo)
        {
            // Check the brokerDic to see if the session Id already exists
            lock (this.brokerDic)
            {
                if (this.brokerDic.ContainsKey(recoverInfo.SessionId))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_SessionIdAlreadyExists, SR.SessionIdAlreadyExists, recoverInfo.SessionId.ToString());
                }

                if (BrokerLauncherSettings.Default.MaxConcurrentSession > 0 && this.brokerDic.Count >= BrokerLauncherSettings.Default.MaxConcurrentSession)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_TooManyBrokerRunning, SR.TooManyBrokerRunning, BrokerLauncherSettings.Default.MaxConcurrentSession.ToString());
                }
            }

            //TODO: SF: make sure the clusterInfo.NetworkTopology string can be converted to ClusterTopology enum
            //ClusterTopology topo = ClusterTopology.Public;
            // ClusterTopology topo;
            // Enum.TryParse<ClusterTopology>(clusterInfo.NetworkTopology, out topo);

            //get soa configurations
            Dictionary<string, string> soaConfig = new Dictionary<string, string>();
            List<string> keys = new List<string>() { Constant.RegistryPathEnv, Constant.AutomaticShrinkEnabled, Constant.NettcpOver443, Constant.NetworkPrefixEnv, Constant.EnableFqdnEnv };
            soaConfig = await this.schedulerHelper.GetSOAConfigurations(keys);

            ServiceRegistrationRepo serviceRegistration = await this.GetRegistrationRepo(soaConfig[Constant.RegistryPathEnv]);
            string serviceRegistrationPath = serviceRegistration.GetServiceRegistrationPath(recoverInfo.StartInfo.ServiceName, recoverInfo.StartInfo.ServiceVersion);
            if (serviceRegistrationPath == null)
            {
                throw new FileNotFoundException("Registration file is not found", recoverInfo.StartInfo.ServiceName);
            }

            CustomBrokerRegistration customBroker = GetCustomBroker(serviceRegistrationPath);

            // Build the broker start info
            BrokerStartInfo brokerInfo = new BrokerStartInfo();
            brokerInfo.SessionId = recoverInfo.SessionId;
            brokerInfo.JobOwnerSID = await this.schedulerHelper.GetJobOwnerSID(brokerInfo.SessionId);
            brokerInfo.Durable = recoverInfo.Durable;
            brokerInfo.Attached = attached;
            //this is scheduler node or cluster connection string
            brokerInfo.Headnode = this.headnode;
            brokerInfo.PurgedFailed = recoverInfo.PurgedFailed;
            brokerInfo.PurgedProcessed = recoverInfo.PurgedProcessed;
            brokerInfo.PurgedTotal = recoverInfo.PurgedTotal;
            brokerInfo.ConfigurationFile = serviceRegistrationPath;
            brokerInfo.NetworkTopology = 0; // ClusterTopology.Public
            if (!BrokerLauncherEnvironment.Standalone)
            {
                brokerInfo.ClusterName = clusterInfo.ClusterName;
                brokerInfo.ClusterId = clusterInfo.ClusterId;
                brokerInfo.AzureStorageConnectionString = clusterInfo.AzureStorageConnectionString;
            }
            else
            {
                brokerInfo.Standalone = true;
            }

            brokerInfo.UseAad = recoverInfo.StartInfo.UseAad;
            brokerInfo.AadUserSid = recoverInfo.AadUserSid;
            brokerInfo.AadUserName = recoverInfo.AadUserName;

            if (soaConfig.TryGetValue(Constant.AutomaticShrinkEnabled, out var v))
            {
                brokerInfo.AutomaticShrinkEnabled = Convert.ToBoolean(v);
            }
            else
            {
                brokerInfo.AutomaticShrinkEnabled = false;
            }

            if (SoaHelper.IsOnAzure())
            {
                brokerInfo.EnableDiagTrace = true;
            }
            else
            {
                brokerInfo.EnableDiagTrace = SoaDiagTraceHelper.IsDiagTraceEnabled(recoverInfo.SessionId);
            }

            if (!SoaHelper.IsSchedulerOnAzure())
            {
                // default value is true
                bool nettcpOver443 = true;

                string value = soaConfig[Constant.NettcpOver443];

                if (!string.IsNullOrEmpty(value))
                {
                    if (!bool.TryParse(value, out nettcpOver443))
                    {
                        nettcpOver443 = true;
                    }
                }

                brokerInfo.HttpsBurst = !nettcpOver443;
            }

            if (SoaHelper.IsSchedulerOnAzure())
            {
                // do not need network prefix for the Azure nodes
                brokerInfo.NetworkPrefix = string.Empty;
            }
            else
            {
                brokerInfo.NetworkPrefix = soaConfig[Constant.NetworkPrefixEnv];
            }

            // get enableFQDN setting from the cluster env var
            bool enableFQDN = false;

            string enableFqdnStr = soaConfig[Constant.EnableFqdnEnv];

            if (!string.IsNullOrEmpty(enableFqdnStr))
            {
                if (bool.TryParse(enableFqdnStr, out enableFQDN))
                {
                    brokerInfo.EnableFQDN = enableFQDN;

                    BrokerTracing.TraceVerbose(
                        "[BrokerManager].CreateBrokerAndRun: The enableFQDN setting in cluster env var is {0}",
                        enableFQDN);
                }
                else
                {
                    BrokerTracing.TraceError(
                        "[BrokerManager].CreateBrokerAndRun: The enableFQDN setting \"{0}\" in cluster env var is not a valid bool value.",
                        enableFqdnStr);
                }
            }

            // set persist version.
            if (!brokerInfo.Attached)
            {
                //if creating a new session, set persist version to BrokerVersion.PersistVersion
                brokerInfo.PersistVersion = BrokerVersion.PersistVersion;
            }
            else
            {
                //if attaching an existing session, get PersistVersion from recoverInfo
                if (recoverInfo.PersistVersion.HasValue)
                {
                    brokerInfo.PersistVersion = recoverInfo.PersistVersion.Value;
                }
                else
                {
                    // if recover info doesn't have PersistVersion info, default to DefaultPersistVersion
                    brokerInfo.PersistVersion = BrokerVersion.DefaultPersistVersion;
                }

                // if version is not supported, throw UnsupportedVersion exception
                if (!BrokerVersion.IsSupportedPersistVersion(brokerInfo.PersistVersion))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_UnsupportedVersion, SR.UnsupportedVersion, brokerInfo.PersistVersion.ToString(), BrokerVersion.PersistVersion.ToString());
                }
            }
            BrokerAuthorization auth = null;
            if (recoverInfo.StartInfo.Secure)
            {
                if (recoverInfo.StartInfo.ShareSession)
                {
                    brokerInfo.JobTemplateACL = await this.schedulerHelper.GetJobTemplateACL(recoverInfo.StartInfo.JobTemplate);
                    auth = new BrokerAuthorization(brokerInfo.JobTemplateACL, (int)JobTemplateRights.SubmitJob, (int)JobTemplateRights.Generic_Read, (int)JobTemplateRights.Generic_Write, (int)JobTemplateRights.Generic_Execute, (int)JobTemplateRights.Generic_All);
                }
                else
                {
                    auth = new BrokerAuthorization(new SecurityIdentifier(brokerInfo.JobOwnerSID));
                }
            }

            BrokerInfo info = new BrokerInfo(recoverInfo, brokerInfo, auth, customBroker, this.pool);
            try
            {
                info.BrokerExited += new EventHandler(BrokerInfo_BrokerExited); // if the broker exit quickly due to short timeouts, the broker info could remain in the brokerDic, because it is added later.

                info.StartBroker();

                lock (this.brokerDic)
                {
                    if (BrokerLauncherSettings.Default.MaxConcurrentSession > 0 && this.brokerDic.Count >= BrokerLauncherSettings.Default.MaxConcurrentSession)
                    {
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_TooManyBrokerRunning, SR.TooManyBrokerRunning, BrokerLauncherSettings.Default.MaxConcurrentSession.ToString());
                    }

                    if (this.brokerDic.ContainsKey(recoverInfo.SessionId))
                    {
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_SessionIdAlreadyExists, SR.SessionIdAlreadyExists, recoverInfo.SessionId.ToString());
                    }

                    this.brokerDic.Add(recoverInfo.SessionId, info);
                }

                // Update broker info into job property
                await this.schedulerHelper.UpdateBrokerInfo(info);

            }
            catch (Exception e)
            {
                // Some exception happens during the call, do some clean up
                TraceHelper.TraceEvent(recoverInfo.SessionId, System.Diagnostics.TraceEventType.Error, "[BrokerManager] CreateBrokerDomainAndRun: Failed : {0}\nRevert change...", e);

                // Bug 5378: If the broker is raised because of attaching (failover), revert it to suspend but not finished state
                RevertCreateDomainAndRun(info, attached);
                throw;
            }

            return info.InitializationResult;
        }

        /// <summary>
        /// Create a ServiceRegistrationRepo instance.
        /// </summary>
        /// <returns>ServiceRegistrationRepo instance</returns>
        private Task<ServiceRegistrationRepo> GetRegistrationRepo(string centrialPath)
        {
            if (SoaHelper.IsOnAzure())
            {
                centrialPath = SoaHelper.GetCcpPackageRoot();
            }
            else
            {
                if (string.IsNullOrEmpty(centrialPath))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.ServiceRegistrationPathEnvironmentMissing, SR.ServiceRegistrationPathEnvironmentMissing);
                }
            }

            return Task.FromResult(new ServiceRegistrationRepo(centrialPath, this.context.GetServiceRegistrationRestClient()));
        }

        /// <summary>
        /// Handle broker exit event
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event args</param>
        private void BrokerInfo_BrokerExited(object sender, EventArgs e)
        {
            BrokerInfo info = (BrokerInfo)sender;
            int exitCode = info.GetExitCode();
            TraceHelper.TraceEvent(info.SessionId, exitCode == 0 ? TraceEventType.Information : TraceEventType.Error, "[BrokerManager] Broker process exited, ExitCode = {0}", exitCode);

            try
            {
                if (exitCode != 0 && info.RetryCount <= RecoverBrokerRetryLimit)
                {
                    // ExitCode != 0 means the broker encountered an unexpected error, retry
                    if (info.Durable)
                    {
                        // Bug 8285: Only retry if it is a durable session
                        TraceHelper.TraceEvent(info.SessionId, TraceEventType.Information, "[BrokerManager] Retry to create broker.");
                        info.StartBroker();
                        return;
                    }
                    else
                    {
                        // Bug 8285: Fail service job for interactive session if broker worker dies
                        this.schedulerHelper.FailJob(info.SessionId, FailInteractiveServiceJobBecauseBrokerWorkerDied).GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError(info.SessionId, "[BrokerManager] Exception thrown when handling broker exited event: {0}", ex);
            }

            lock (this.brokerDic)
            {
                if (!this.brokerDic.Remove(info.SessionId))
                {
                    TraceHelper.TraceError(info.SessionId,
                        "[BrokerManager] Failed to remove the session {0} from the brokerDic", info.SessionId);
                }
            }

            SoaDiagTraceHelper.RemoveDiagTraceEnabledFlag(info.SessionId);
        }

        /// <summary>
        /// Start broker init operations
        /// </summary>
        private async Task RecoverThreadProc(CancellationToken ct)
        {
            int retry = 0;
            BrokerRecoverInfo[] recoverInfoList;
            this.schedulerHelper = null;

            // TODO: on azure, perf counter
            if (!SoaHelper.IsOnAzure())
            {
                while (!ct.IsCancellationRequested)
                {
                    TraceHelper.TraceEvent(TraceEventType.Information, "[BrokerManager] Try to create the perf counters, Retry count = {0}", retry);
                    try
                    {
                        this.requestQueueLengthCounter = BrokerPerformanceCounterHelper.GetPerfCounter(BrokerPerformanceCounterKey.DurableRequestsQueueLength);
                        this.responseQueueLengthCounter = BrokerPerformanceCounterHelper.GetPerfCounter(BrokerPerformanceCounterKey.DurableResponsesQueueLength);
                        break;
                    }
                    catch (Exception e)
                    {
                        // Bug 8507 : Fix leak
                        if (this.requestQueueLengthCounter != null)
                        {
                            this.requestQueueLengthCounter.Close();
                            this.requestQueueLengthCounter = null;
                        }

                        TraceHelper.TraceEvent(TraceEventType.Error, "[BrokerManager] Failed to create the perf counters: {0}", e);
                        retry++;
                        await Task.Delay(RetryPeriod, ct);
                    }
                }
            }

            while (true)
            {
                TraceHelper.TraceEvent(
                    System.Diagnostics.TraceEventType.Information,
                    "[BrokerManager] Try to connect to the headnode, Retry count = {0}.",
                    retry);
                try
                {
                    lock (this.brokerDic)
                    {
                        this.brokerDic.Clear();
                    }

                    // Bug 8507 : Fix leak
                    if (this.schedulerHelper == null)
                    {
                        this.schedulerHelper = SchedulerHelperFactory.GetSchedulerHelper(this.context);
                    }

                    recoverInfoList = await this.schedulerHelper.LoadBrokerRecoverInfo();
                    break;
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(
                        TraceEventType.Error,
                        "[BrokerManager] Exception throwed while connecting to head node {0}: {1}", this.headnode, e);

                    retry++;
                    await Task.Delay(RetryPeriod, ct);
                }
            }

            this.staleSessionCleanupTimer.Change(0, BrokerManager.StaleSessionCleanupPeriod);

            if (this.updateQueueLengthTimer != null)
            {
                // TODO: on azure, about the MSMQ. Don't use the MSMQ in the Azure cluster.
                this.updateQueueLengthTimer.Change(0, BrokerManager.UpdateQueueLengthPeriod);
            }

            List<BrokerRecoverInfo> failedList = new List<BrokerRecoverInfo>();
            List<Exception> exceptionList = new List<Exception>();
            for (int i = 0; i < RecoverBrokerRetryLimit; i++)
            {
                List<BrokerRecoverInfo> retryList = new List<BrokerRecoverInfo>();
                foreach (BrokerRecoverInfo recoverInfo in recoverInfoList)
                {
                    try
                    {
                        // Only running broker will be recovered here
                        // Should start the broker immediately
                        ClusterInfoContract clusterInfo = await this.schedulerHelper.GetClusterInfoAsync();
                        await this.CreateBrokerAndRun(recoverInfo, true, clusterInfo);
                        TraceHelper.TraceEvent(recoverInfo.SessionId, System.Diagnostics.TraceEventType.Information, "[BrokerManager] Succeeded start broker {0} during initialization", recoverInfo.SessionId);
                        TraceHelper.RuntimeTrace.LogSessionRaisedUpFailover(recoverInfo.SessionId);
                    }
                    catch (Exception e)
                    {
                        TraceHelper.TraceEvent(recoverInfo.SessionId, System.Diagnostics.TraceEventType.Error, "[BrokerManager] Exception throwed while recovering broker {0} : {1}, Retry = {2}", recoverInfo.SessionId, e, ExceptionUtility.ShouldRetry(e));
                        lock (this.brokerDic)
                        {
                            if (this.brokerDic.ContainsKey(recoverInfo.SessionId))
                            {
                                this.brokerDic.Remove(recoverInfo.SessionId);
                            }
                        }

                        if (ExceptionUtility.ShouldRetry(e))
                        {
                            retryList.Add(recoverInfo);
                        }
                        else
                        {
                            failedList.Add(recoverInfo);
                            exceptionList.Add(e);
                        }
                    }
                }

                if (retryList.Count == 0)
                {
                    if (failedList.Count == 0)
                    {
                        this.connected = true;
                        TraceHelper.TraceEvent(
                            System.Diagnostics.TraceEventType.Information,
                            "[BrokerManager] Succeeded connecting to the headnode:{0}.",
                            this.schedulerHelper.HeadNode);
                        return;
                    }
                    else
                    {
                        break;
                    }
                }

                recoverInfoList = retryList.ToArray();
                await Task.Delay(RetryPeriod, ct);
            }

            TraceHelper.TraceEvent(System.Diagnostics.TraceEventType.Warning, "[BrokerManager] Connected to the headnode and recover broker info, Failed = {0}", recoverInfoList.Length);

            // fail jobs that cannot be recovered
            for (int i = 0; i < failedList.Count; i++)
            {
                BrokerRecoverInfo recoverInfo = failedList[i];
                Exception exception = exceptionList[i];

                // Log the exception
                TraceHelper.TraceEvent(System.Diagnostics.TraceEventType.Error, "[BrokerManager] Failed to recover broker.  Exception: {0}", exception);

                // We do not pass exception detail to FailJob call because of the 128 byte reason message limitation, which is likely not enough for exception detail.
                await this.schedulerHelper.FailJob(recoverInfo.SessionId, "Failed to recover broker.  Check broker log for detail.");
            }

            this.connected = true;
        }

        /// <summary>
        /// cleanup the stale session data.
        /// </summary>
        /// <param name="state">the state object.</param>
        private async void CleanStaleSessionData(object state)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[BrokerManager] CleanStaleSessionData: begin.");

            try
            {
                await BrokerEntry.CleanupStaleSessionData(
                    async delegate (int jobId)
                    {
                        return await this.schedulerHelper.IsJobPurged(jobId);
                    });
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[BrokerManager] Failed to cleanup stale session data: {0}", e.ToString());
            }

            TraceHelper.TraceEvent(TraceEventType.Verbose, "[BrokerManager] CleanStaleSessionData: end.");
        }

        public bool? IfSeesionCreatedByAadOrLocalUser(int sessionId)
        {
            BrokerInfo info = null;
            lock (this.brokerDic)
            {
                this.brokerDic.TryGetValue(sessionId, out info);
            }

            if (info == null)
            {
                // Proberbly already disposed.
                return null;
            }
            else
            {
                return info.IsAadOrLocalUser;
            }
        }

        private int disposed = 0;

        /// <summary>
        /// Dispose the broker manager
        /// </summary>
        /// <param name="disposing">indicating whether it's disposing</param>
        private void Dispose(bool disposing)
        {
            //dispose only once
            if (Interlocked.CompareExchange(ref disposed, 1, 0) == 1)
            {
                return;
            }

            List<int> sessionIdList;
            lock (this.brokerDic)
            {
                sessionIdList = new List<int>(this.brokerDic.Keys);
            }

            foreach (int sessionId in sessionIdList)
            {
                // Clean up will not throw exceptions
                this.CleanupAsync(sessionId, true).GetAwaiter().GetResult();
            }

            if (disposing)
            {
                if (this.RecoverTask != null && this.ts != null)
                {
                    // cancel the recover task
                    this.ts.Cancel();
                }

                this.ts?.Dispose();
                this.ts = null;

                // SchedulerHelper.Dispose() will not throw any exception
                if (this.schedulerHelper != null)
                {
                    this.schedulerHelper.Dispose();
                    this.schedulerHelper = null;
                }

                if (this.staleSessionCleanupTimer != null)
                {
                    this.staleSessionCleanupTimer.Dispose();
                    this.staleSessionCleanupTimer = null;
                }

                if (this.updateQueueLengthTimer != null)
                {
                    this.updateQueueLengthTimer.Dispose();
                    this.updateQueueLengthTimer = null;
                }

                if (this.pool != null)
                {
                    this.pool.Close();
                    this.pool = null;
                }
            }
        }

#if HPCPACK
        /// <summary>
        /// Callback to update MSMQ length counter
        /// </summary>
        /// <param name="state">null object</param>
        private void CallbackToUpdateMSMQLength(object state)
        {
            try
            {
                long requestQueueLength, responseQueueLength;
                MSMQPersist.GetAllSessionCounts(out requestQueueLength, out responseQueueLength);
                this.requestQueueLengthCounter.RawValue = requestQueueLength;
                this.responseQueueLengthCounter.RawValue = responseQueueLength;
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[BrokerManager] Failed update MSMQ length: {0}", e);
            }
        }
#endif

        /// <summary>
        /// Returns a value indicating whether the caller is headnode's machine account
        /// </summary>
        /// <param name="caller">indicating the caller</param>
        /// <returns>returns whether the caller is head node's machine account</returns>
        private bool IsCallingFromHeadNode(WindowsIdentity caller)
        {
            TraceHelper.TraceVerbose(0, "[BrokerManager] Check if user is calling from head node. UserName = {0}", caller.Name);
            //TODO: SF: this.headnode is cluster connection string. Fix it.
            return Utility.IsCallingFromHeadNode(caller, this.headnode);
        }
    }
}

