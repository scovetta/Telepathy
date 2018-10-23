//------------------------------------------------------------------------------
// <copyright file="BrokerLauncher.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Implementation of broker launcher
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;

    using Microsoft.Hpc.AADAuthUtil;

    /// <summary>
    /// Implementation of broker launcher
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true,
         Name = "BrokerLauncher", Namespace = "http://hpc.microsoft.com/brokerlauncher/")]
    internal class BrokerLauncher : IBrokerLauncher, IDisposable
    {
        /// <summary>
        /// Stores the broker manager
        /// </summary>
        private BrokerManager brokerManager;

        /// <summary>
        /// the flag to handle online/offline
        /// </summary>
        private bool AllowNewSession = true;

        /// <summary>
        /// How often to check whether graceful offline is complete
        /// </summary>
        const int gracefulOfflineCheckInterval = 5000;

        /// <summary>
        /// Lock for instance member vars
        /// </summary>
        private object instanceLock = new object();

        /// <summary>
        /// Stores the job object
        /// </summary>
        private JobObject job;

        /// <summary>
        /// Stores the fabric client;
        /// </summary>
        private IHpcContext context;

        /// <summary>
        /// Initializes a new instance of the BrokerLauncher class
        /// </summary>
        [SecurityPermission(SecurityAction.Demand)]
        public BrokerLauncher(bool managementOperationsOnly, IHpcContext context)
        {
            this.context = context;

            // Initializes the job object
            this.job = new JobObject();

            // Assign broker launcher process to the job object
            this.job.Assign(Process.GetCurrentProcess());

            // If this instance of HpcBroker service should handle mgmt and app operations
            if (!managementOperationsOnly)
            {
                // Force the broker mananger to intialize when the service is online
                this.brokerManager = new BrokerManager(this.IsOnline, context);
            }
        }

        /// <summary>
        /// Finalizes an instance of the BrokerLauncher class
        /// </summary>
        ~BrokerLauncher()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the instance of broker manager
        /// </summary>
        internal BrokerManager BrokerManager
        {
            get { return this.brokerManager; }
        }

        /// <summary>
        /// Create a session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionId">session id</param>
        /// <returns>returns broker initialization result</returns>
        public BrokerInitializationResult Create(SessionStartInfoContract info, int sessionId)
        {
            if ((!this.AllowNewSession) || (!this.IsOnline && String.IsNullOrEmpty(info.DiagnosticBrokerNode)))
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerIsOffline, SR.BrokerIsOffline);
            }

            // Handle invalid input parameters
            try
            {
                ParamCheckUtility.ThrowIfOutofRange(sessionId < -1, "sessionId");
                ParamCheckUtility.ThrowIfNull(info, "info");
                if (!BrokerLauncherEnvironment.Standalone)
                {
                    this.CheckAccess(sessionId);
                }

                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerLauncher] Create: SessionId = {0}", sessionId);
                //TODO: make it async
                BrokerInitializationResult returnValue = this.brokerManager.CreateNewBrokerDomain(info, sessionId, false).GetAwaiter().GetResult();

                #region Debug Failure Test
                Microsoft.Hpc.ServiceBroker.SimulateFailure.FailOperation(1);
                #endregion

                TraceHelper.RuntimeTrace.LogSessionCreated(sessionId);
                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerLauncher] Create Broker {0} Succeeded.", sessionId);
                return returnValue;
            }
            catch (Exception e)
            {
                TraceHelper.RuntimeTrace.LogFailedToCreateSession(sessionId);

                // Bug 10614: Throw a proper exception when the broker node is being taken offline
                if ((!this.AllowNewSession) || (!this.IsOnline && String.IsNullOrEmpty(info.DiagnosticBrokerNode)))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerIsOffline, SR.BrokerIsOffline);
                }

                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Error, "[BrokerLauncher] Create Broker {0} failed: {1}", sessionId, e.ToString());
                throw ExceptionHelper.ConvertExceptionToFaultException(e);
            }
        }

        /// <summary>
        /// Create a durable session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionId">session id</param>
        /// <returns>returns broker initialization result</returns>
        public BrokerInitializationResult CreateDurable(SessionStartInfoContract info, int sessionId)
        {
            if ((!this.AllowNewSession) || (!this.IsOnline && String.IsNullOrEmpty(info.DiagnosticBrokerNode)))
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerIsOffline, SR.BrokerIsOffline);
            }

            try
            {
                ParamCheckUtility.ThrowIfOutofRange(sessionId <= 0, "sessionId");
                ParamCheckUtility.ThrowIfNull(info, "info");

                this.CheckAccess(sessionId);

                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerLauncher] CreateDurable: SessionId = {0}", sessionId);

                // following method will write LogSessionCreating trace
                //TODO: make it async
                BrokerInitializationResult returnValue = this.brokerManager.CreateNewBrokerDomain(info, sessionId, true).GetAwaiter().GetResult();

                #region Debug Failure Test
                Microsoft.Hpc.ServiceBroker.SimulateFailure.FailOperation(1);
                #endregion

                TraceHelper.RuntimeTrace.LogSessionCreated(sessionId);
                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerLauncher] Create Durable Broker {0} Succeeded.", sessionId);
                return returnValue;
            }
            catch (Exception e)
            {
                TraceHelper.RuntimeTrace.LogFailedToCreateSession(sessionId);

                // Bug 10614: Throw a proper exception when the broker node is being taken offline
                if ((!this.AllowNewSession) || (!this.IsOnline && String.IsNullOrEmpty(info.DiagnosticBrokerNode)))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerIsOffline, SR.BrokerIsOffline);
                }

                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Error, "[BrokerLauncher] Create Durable Broker {0} failed: {1}", sessionId, e);
                throw ExceptionHelper.ConvertExceptionToFaultException(e);
            }
        }

        /// <summary>
        /// Attach to a session
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <returns>returns broker initialization result</returns>
        public BrokerInitializationResult Attach(int sessionId)
        {
            if (!this.AllowNewSession)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerIsOffline, SR.BrokerIsOffline);
            }

            try
            {
                ParamCheckUtility.ThrowIfOutofRange(sessionId <= 0, "sessionId");
                this.CheckAccess(sessionId);

                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerLauncher] Attach: SessionId = {0}", sessionId);
                BrokerInitializationResult returnValue = this.brokerManager.AttachBroker(sessionId).GetAwaiter().GetResult();

                #region Debug Failure Test
                Microsoft.Hpc.ServiceBroker.SimulateFailure.FailOperation(1);
                #endregion

                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerLauncher] Attach Broker {0} Succeeded.", sessionId);
                return returnValue;
            }
            catch (Exception e)
            {
                // Bug 10614: Throw a proper exception when the broker node is being taken offline
                if (!this.AllowNewSession)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerIsOffline, SR.BrokerIsOffline);
                }

                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Error, "[BrokerLauncher] Attach Broker {0} failed: {1}", sessionId, e);
                throw ExceptionHelper.ConvertExceptionToFaultException(e);
            }
        }

        /// <summary>
        /// Close a session
        /// </summary>
        /// <param name="sessionId">session id</param>
        public void Close(int sessionId)
        {
            try
            {
                ParamCheckUtility.ThrowIfOutofRange(sessionId < -1, "sessionId");

                bool? isAadOrLocalUser = this.BrokerManager.IfSeesionCreatedByAadOrLocalUser(sessionId);
                if (!isAadOrLocalUser.HasValue)
                {
                    TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Warning, "[BrokerLauncher] Info not found: SessionId = {0}", sessionId);
                    return;
                }
                this.CheckAccess(sessionId);
                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerLauncher] Close: SessionId = {0}", sessionId);
                this.brokerManager.CloseBrokerDomain(sessionId).GetAwaiter().GetResult();

                #region Debug Failure Test
                Microsoft.Hpc.ServiceBroker.SimulateFailure.FailOperation(1);
                #endregion

                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Information, "[BrokerLauncher] Close Broker {0} Succeeded.", sessionId);
            }
            catch (Exception e)
            {
                // Bug 14019: Swallow the exception for close as the broker node is already taken offline
                if (!this.AllowNewSession)
                {
                    return;
                }

                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Error, "[BrokerLauncher] Close Broker {0} failed: {1}", sessionId, e);
                throw ExceptionHelper.ConvertExceptionToFaultException(e);
            }
        }

        /// <summary>
        /// Pings specified broker
        /// </summary>
        /// <param name="sessionID">sessionID of broker to ping</param>
        public bool PingBroker(int sessionId)
        {
            try
            {
                ParamCheckUtility.ThrowIfOutofRange(sessionId <= 0, "sessionId");
                this.CheckAccess(sessionId);

                if (!SoaHelper.CheckWindowsIdentity(OperationContext.Current))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.AccessDenied_BrokerLauncher, SR.AccessDenied_BrokerLauncher);
                }

                string uniqueId;
                return this.brokerManager.DoesBrokerExist(sessionId, out uniqueId);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Error, "[BrokerLauncher] Ping Broker {0} failed: {1}", sessionId, e);
                throw ExceptionHelper.ConvertExceptionToFaultException(e);
            }
        }

        /// <summary>
        /// Ping broker new version
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public string PingBroker2(int sessionId)
        {
            try
            {
                ParamCheckUtility.ThrowIfOutofRange(sessionId <= 0, "sessionId");
                this.CheckAccess(sessionId);

                string uniqueId;
                if (this.brokerManager.DoesBrokerExist(sessionId, out uniqueId))
                {
                    return uniqueId;
                }
                else
                {
                    return Constant.PingBroker2Result_BrokerNotExist;
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(sessionId, System.Diagnostics.TraceEventType.Error, "[BrokerLauncher] Ping Broker {0} failed: {1}", sessionId, e);
                throw ExceptionHelper.ConvertExceptionToFaultException(e);
            }
        }

        /// <summary>
        /// Get active broker id list
        /// </summary>
        /// <returns>return active broker id lists</returns>
        public int[] GetActiveBrokerIdList()
        {
            try
            {
                // Set sessionId to 0 to check access for head node machine account
                this.CheckAccess(0);

                return this.brokerManager.GetActiveBrokerIdList();
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(0, System.Diagnostics.TraceEventType.Error, "[BrokerLauncher] Get active broker id list failed: {0}", e);
                throw ExceptionHelper.ConvertExceptionToFaultException(e);
            }
        }

        /// <summary>
        /// Close the broker launcher
        /// </summary>
        public void Close()
        {
            try
            {
                this.Dispose();
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Critical, "[BrokerLauncher] Dispose failed: {0}", e);
            }
        }

        /// <summary>
        /// Dispse the broker launcher
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Ensure the caller is a valid cluster user
        /// </summary>
        private void CheckAccess(int sessionId)
        {
            if (BrokerLauncherEnvironment.Standalone)
            {
                return;
            }

            if (Thread.CurrentPrincipal.IsHpcAadPrincipal(this.context))
            {
                return;
            }

            if (SoaHelper.CheckX509Identity(OperationContext.Current))
            {
                return;
            }

            WindowsIdentity identity = null;
            if (SoaHelper.CheckWindowsIdentity(OperationContext.Current, out identity))
            {
                if (identity == null)
                {
                    return;
                }
                else
                {
                    if (this.brokerManager.AuthenticateUser(sessionId, identity))
                    {
                        return;
                    }
                }
            }

            ThrowHelper.ThrowSessionFault(SOAFaultCode.AccessDenied_BrokerLauncher, SR.AccessDenied_BrokerLauncher);
        }

        /// <summary>
        /// Dispose the broker launcher
        /// </summary>
        /// <param name="disposing">indicating whether it's disposing</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.brokerManager != null)
                {
                    // BrokerManager.Dispose() will not throw exceptions
                    this.brokerManager.Close();
                    this.brokerManager = null;
                }

                if (this.CheckOfflineState != null)
                {
                    this.CheckOfflineState.Dispose();
                    this.CheckOfflineState = null;
                }
            }
        }

        #region Online/Offline

        /// <summary>
        /// Timer to monitor graceful offlone
        /// </summary>
        private Timer CheckOfflineState;

        /// <summary>
        /// Whether the broker is online or offline
        /// </summary>
        /// TODO: Should this be init from reg?
        private bool isOnline = true;

        /// <summary>
        /// Whether the broker is online or offline
        /// </summary>
        internal bool IsOnline
        {
            get
            {
                return this.isOnline;
            }
        }

        /// <summary>
        /// Bring broker online
        /// </summary>
        internal void Online()
        {
            // Check if management command should be redirected to another HpcBroker on the same node (needed
            // for failover BN)
            if (!ShouldRedirectManagementCommand())
            {
                if (!this.IsOnline)
                {
                    this.brokerManager.Close();
                    // Force the broker mananger to intialize when the service is online
                    this.brokerManager = new BrokerManager(true, this.context);
                }

                // If not handle command in this instance
                // Allow new sessions 
                this.AllowNewSession = true;

                // Cleanup up previous timer. Need lock in case user cancels offline timer callback is in progress
                lock (this.instanceLock)
                {
                    if (this.CheckOfflineState != null)
                    {
                        this.CheckOfflineState.Dispose();
                        this.CheckOfflineState = null;
                    }
                }
            }
            else
            {
                // If command should be redirected, redirect online synchronously
                RedirectManagementCommand(delegate(BrokerManagementClient client) { client.Online(); });
            }

            this.isOnline = true;
        }

        /// <summary>
        /// Bring broker offline gracefully
        /// </summary>
        /// <returns>WaitHandle signalled when offline completes</returns>
        internal EventWaitHandle StartOffline(bool force)
        {
            AutoResetEvent finish = new AutoResetEvent(false);

            // Check if management command should be redirected to another HpcBroker on the same node (needed
            // for failover BN)
            if (!ShouldRedirectManagementCommand())
            {
                // If not handle command in this instance
                // Prevent new sessions
                this.AllowNewSession = false;

                if (force)
                {
                    // Dispose all the broker node
                    this.brokerManager.Close();

                    // Signal event now since we are now offline
                    this.isOnline = false;

                    finish.Set();
                }
                else
                {
                    // Watch for existing sessions to complete
                    this.CheckOfflineState = new Timer(new TimerCallback(this.CheckOfflineStateCallback), finish, 0, gracefulOfflineCheckInterval);
                }
            }
            else
            {
                // If command should be redirected, redirect online 
                if (!RedirectManagementCommand(delegate(BrokerManagementClient client) { client.StartOffline(force); }))
                {
                    // If connection to another local HpcBroker instance failed, assume its offline on this node
                    this.isOnline = false;
                    finish.Set();
                }
                else
                {
                    // wait for offline to complete
                    this.CheckOfflineState = new Timer(new TimerCallback(this.CheckOfflineStateCallback), finish, 0, gracefulOfflineCheckInterval);
                }
            }

            return finish;
        }

        /// <summary>
        /// Timer callback for local graceful offline check
        /// </summary>
        /// <param name="finishEvent"></param>
        private void CheckOfflineStateCallback(object finishEvent)
        {
            lock (this.instanceLock)
            {
                if (this.CheckOfflineState != null)
                {
                    bool isOffline = false;

                    if (!ShouldRedirectManagementCommand())
                    {
                        isOffline = this.brokerManager.BrokerCount == 0;

                        if (isOffline)
                        {
                            this.brokerManager.Close();
                            this.brokerManager = new BrokerManager(false, this.context);
                        }
                    }
                    else
                    {
                        if (!RedirectManagementCommand(delegate(BrokerManagementClient client) { isOffline = client.IsOffline(); }))
                        {
                            // If connection to another local HpcBroker instance failed, assume its offline on this node
                            isOffline = true;
                        }
                    }

                    if (isOffline)
                    {
                        // Close timer once all brokers ended
                        this.CheckOfflineState.Dispose();
                        this.CheckOfflineState = null;

                        // Signal that offline complete
                        this.isOnline = false;

                        ((AutoResetEvent)finishEvent).Set();
                    }
                }
            }
        }

        /// <summary>
        /// Whether to redirect the management command to an instance of HpcBroker running in a FC resource group
        /// TODO: Consider caching 
        /// </summary>
        /// <returns></returns>
        private static bool ShouldRedirectManagementCommand()
        {
            // If this HpcBroker instance is running as a Windows service and on a failover BN, redirect
            return !LauncherHostService.LauncherHostService.IsConsoleApplication
                        && Win32API.IsFailoverBrokerNode();
        }

        /// <summary>
        /// Redirects management commands t broker management service
        /// </summary>
        /// <param name="managementCommand">management command to redirect</param>
        /// <param name="managementCommandParam">param to management command</param>
        private static bool RedirectManagementCommand(Action<BrokerManagementClient> managementCommand)
        {
            Debug.Assert(OperationContext.Current == null, "BrokerManagement service should not redirect commands");

            bool clientConnectionFailed = false;

            try
            {
                BrokerManagementClient client = null;

                try
                {
                    client = new BrokerManagementClient();

                    // If connection to local BrokerManager service fails, there is very likely no broker
                    // resource group running on this machine (i.e. this machine is a standby). Eat
                    // the exception and allow pause to succeed
                    try
                    {
                        client.Open();
                    }
                    catch (Exception ex)
                    {
                        clientConnectionFailed = true;
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerLauncher].RedirectManagementCommand: Exception {0}", ex);
                    }

                    if (!clientConnectionFailed)
                    {
                        managementCommand(client);
                    }
                }

                finally
                {
                    if (client != null && client.State != CommunicationState.Faulted)
                    {
                        try
                        {
                            client.Close();
                        }
                        catch (Exception ex)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerLauncher].RedirectManagementCommand: Exception {0}", ex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // For any other exception, log it and rethrow
                TraceHelper.TraceEvent(TraceEventType.Error, "Redirecting mgmt command to local broker resource group failed  - {0}", e);
                throw;
            }

            // Return whether client connected to another local HpcBroker instance
            return !clientConnectionFailed;
        }

        #endregion
    }
}
