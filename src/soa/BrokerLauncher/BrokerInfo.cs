//------------------------------------------------------------------------------
// <copyright file="BrokerInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Stores broker info for broker manager
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Hpc.ServiceBroker.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.RuntimeTrace;
    using System.Threading;

    /// <summary>
    /// Stores broker info for broker manager
    /// </summary>
    internal class BrokerInfo
    {
        /// <summary>
        /// Stores the operation timeout for broker management service
        /// </summary>
        private readonly static TimeSpan operationTimeoutForBrokerManagementService = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Stores the retry limit for operation to broker management service
        /// </summary>
        private const int RetryLimit = 3;

        /// <summary>
        /// Stores the session id
        /// </summary>
        private int sessionId;

        /// <summary>
        /// Stores the initialization result
        /// </summary>
        private BrokerInitializationResult result;

        /// <summary>
        /// Stores a value indicating whether the broker is durable
        /// </summary>
        private bool durable;

        /// <summary>
        /// Stores the session start info
        /// </summary>
        private SessionStartInfoContract sessionStartInfo;

        /// <summary>
        /// Stores the broker process
        /// </summary>
        private BrokerProcess brokerProcess;

        /// <summary>
        /// Stores the disposed flag
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Stores the broker info
        /// </summary>
        private BrokerStartInfo brokerInfo;

        /// <summary>
        /// Stores the retry count
        /// </summary>
        private int retryCount;

        /// <summary>
        /// Stores the broker auth
        /// </summary>
        private BrokerAuthorization auth;

        /// <summary>
        /// Stores the custom broker registration configuration
        /// </summary>
        private CustomBrokerRegistration customBroker;

        /// <summary>
        /// Stores the broker process pool
        /// </summary>
        private BrokerProcessPool pool;

        /// <summary>
        /// Stores the callback to close broker
        /// </summary>
        private AsyncCallback callbackToCloseBroker;

        /// <summary>
        /// Initializes a new instance of the BrokerInfo class from broker recover info
        /// </summary>
        /// <param name="recoverInfo">indicating the broker recover info</param>
        /// <param name="brokerInfo">indicating the broker start info</param>
        /// <param name="auth">indicating the broker auth</param>
        /// <param name="customBroker">indicating the custom broker configuration</param>
        /// <param name="pool">indicating the broker process pool</param>
        public BrokerInfo(BrokerRecoverInfo recoverInfo, BrokerStartInfo brokerInfo, BrokerAuthorization auth, CustomBrokerRegistration customBroker, BrokerProcessPool pool)
        {
            this.callbackToCloseBroker = new ThreadHelper<IAsyncResult>(new AsyncCallback(this.OnCloseBroker)).CallbackRoot;
            this.durable = recoverInfo.Durable;
            this.brokerInfo = brokerInfo;
            this.sessionId = recoverInfo.SessionId;
            this.sessionStartInfo = recoverInfo.StartInfo;
            this.auth = auth;
            this.customBroker = customBroker;
            this.pool = pool;

            this.sessionStartInfo.IpAddress = BrokerLauncherSettings.Default.SvcHostList.Cast<string>().ToArray();
            this.sessionStartInfo.RegPath = BrokerLauncherSettings.Default.CCP_SERVICEREGISTRATION_PATH;
        }

        /// <summary>
        /// Gets the broker exit event
        /// </summary>
        public event EventHandler BrokerExited;

        /// <summary>
        /// Gets or sets a value indicating whether the broker has been disposed
        /// </summary>
        public bool Disposed
        {
            get { return this.disposed; }
            set { this.disposed = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the broker is durable
        /// </summary>
        public bool Durable
        {
            get { return this.durable; }
        }

        /// <summary>
        /// Gets the retry count
        /// </summary>
        public int RetryCount
        {
            get { return this.retryCount; }
        }

        /// <summary>
        /// Gets the session info
        /// </summary>
        public BrokerInitializationResult InitializationResult
        {
            get { return this.result; }
        }

        /// <summary>
        /// Gets the session id
        /// </summary>
        public int SessionId
        {
            get { return this.sessionId; }
        }

        public bool IsAadOrLocalUser => this.sessionStartInfo.IsAadOrLocalUser;

        /// <summary>
        /// Gets the unique id for the broker worker process
        /// This id is associate with the process so that if the
        /// process exits and recreated by broker launcher, the id
        /// should be expected to be different
        /// </summary>
        public string UniqueId
        {
            get
            {
                return this.brokerProcess == null ?
                    Constant.PingBroker2Result_BrokerNotExist :
                    this.brokerProcess.UniqueId.ToString();
            }
        }

        /// <summary>
        /// gets the owner SID
        /// </summary>
        public string JobOwnerSID
        {
            get { return this.brokerInfo.JobOwnerSID; }
        }

        /// <summary>
        /// Gets or sets the persist version
        /// </summary>
        public int PersistVersion
        {
            get { return this.brokerInfo.PersistVersion; }
            set { this.brokerInfo.PersistVersion = value; }
        }

        /// <summary>
        /// Check if the incoming user is valid to attach to the session
        /// </summary>
        public void CheckAccess()
        {
            if (this.auth != null && !this.auth.CheckAccess())
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.AccessDenied_Broker, SR.AccessDenied_Broker);
            }
        }

        /// <summary>
        /// Gets the exit code
        /// </summary>
        /// <returns>returns the exit code</returns>
        public int GetExitCode()
        {
            return this.brokerProcess.ExitCode;
        }

        /// <summary>
        /// Start broker process
        /// </summary>
        public void StartBroker()
        {
            bool failoverMode = false;
            if (Interlocked.Increment(ref this.retryCount) > 1)
            {
                // Bug 7150: Need to set attach to true when retrying
                failoverMode = true;
                this.brokerInfo.Attached = true;
            }

            if (this.customBroker == null || String.IsNullOrEmpty(this.customBroker.Executive))
            {
                this.brokerProcess = this.pool.GetBrokerProcess();
            }
            else
            {
                this.brokerProcess = CreateCustomBrokerProcess(this.customBroker);
            }

            // Log a trace mapping broker worker pid to session id.
            TraceHelper.TraceEvent(
                this.sessionId,
                TraceEventType.Information,
                "[BrokerInfo].StartBroker: Init broker worker {0} for session {1}.",
                this.brokerProcess.Id,
                this.sessionId);

            BrokerManagementServiceClient client = this.CreateClient();
            try
            {
                this.result = client.Initialize(this.sessionStartInfo, this.brokerInfo);

                // Set broker's unique id to the initialization result when the process
                // is (re)started.
                this.result.BrokerUniqueId = this.UniqueId;
                this.brokerProcess.Exited += new EventHandler(BrokerProcess_Exited);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(this.sessionId, TraceEventType.Error, "[BrokerInfo] Failed to initialize broker: {0}", e.ToString());

                // If in failover mode, close this broker and do not retry anymore
                if (failoverMode)
                {
                    try
                    {
                        this.CloseBroker(true);
                    }
                    catch (Exception ex)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Warning, "[BrokerInfo].StartBroker: Exception {0}", ex);
                    }
                }

                throw;
            }
            finally
            {
                Utility.AsyncCloseICommunicationObject(client);
            }
        }

        /// <summary>
        /// Send a ctrl event to the broker process to close the broker
        /// </summary>
        /// <param name="suspended">indicating the suspended flag</param>
        public void CloseBroker(bool suspended)
        {
            BrokerManagementServiceClient client = this.CreateClient();
            try
            {
                // Call CloseBroker asynchronously and don't care about result
                client.BeginCloseBroker(suspended, new AsyncCallback(this.callbackToCloseBroker), new object[] { suspended, RetryLimit, client });
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(this.sessionId, TraceEventType.Warning, "[BrokerInfo] Failed to close broker: {0}", e);
                Utility.AsyncCloseICommunicationObject(client);
                throw;
            }
        }

        /// <summary>
        /// Attach to the broker
        /// </summary>
        public void Attach()
        {
            BrokerManagementServiceClient client = this.CreateClient();
            try
            {
                client.Attach();
                TraceHelper.TraceEvent(this.sessionId, TraceEventType.Information, "[BrokerInfo] Successfully attached to the broker.");
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(this.sessionId, TraceEventType.Error, "[BrokerInfo] Attach failed: {0}", e);
                throw;
            }
            finally
            {
                Utility.AsyncCloseICommunicationObject(client);
            }
        }

        /// <summary>
        /// Wait until process is exited and all process exit callback is finished
        /// </summary>
        /// <remarks>
        /// This method is only be called when holding the lock of "this" instance
        /// Thus this method won't be called concurrently
        /// </remarks>
        public void WaitForProcessExit(TimeSpan timeoutToKillProcess)
        {
            this.brokerProcess.WaitForExit(timeoutToKillProcess);
        }

        /// <summary>
        /// Create custom broker process
        /// </summary>
        /// <param name="customBrokerRegistration">indicating the custom broker registration</param>
        /// <returns>returns the broker process object</returns>
        private static BrokerProcess CreateCustomBrokerProcess(CustomBrokerRegistration customBroker)
        {
            BrokerProcess process = new BrokerProcess(customBroker.Executive, customBroker.EnvironmentVariables);
            process.Start();
            process.WaitForReady();
            return process;
        }

        /// <summary>
        /// Finish close broker operation
        /// </summary>
        /// <param name="result">IAsyncResult instance</param>
        private void OnCloseBroker(IAsyncResult result)
        {
            object[] objArr = (object[])result.AsyncState;
            bool suspended = (bool)objArr[0];
            int retry = (int)objArr[1];
            BrokerManagementServiceClient client = (BrokerManagementServiceClient)objArr[2];

            try
            {
                client.EndCloseBroker(result);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(this.sessionId, TraceEventType.Error, "[BrokerInfo] Failed to close broker: {0}\nRetryCount = {1}", e, retry);

                // Perform retry if retry>0
                if (retry > 0)
                {
                    retry--;
                    BrokerManagementServiceClient newClient = this.CreateClient();
                    try
                    {
                        newClient.BeginCloseBroker(suspended, new AsyncCallback(this.callbackToCloseBroker), new object[] { suspended, retry, newClient });
                    }
                    catch (Exception ex)
                    {
                        // Do not retry if BeginCloseBroker failed
                        TraceHelper.TraceEvent(this.sessionId, TraceEventType.Error, "[BrokerInfo] Retry failed when calling BeginCloseBroker: {0}", ex);
                        Utility.AsyncCloseICommunicationObject(newClient);
                    }
                }
                else
                {
                    // Kill broker worker process if it failed to close broker
                    try
                    {
                        this.brokerProcess.KillBrokerProcess();
                    }
                    catch (Exception ex)
                    {
                        TraceHelper.TraceError(this.sessionId, "[BrokerInfo] Failed to kill broker worker process: {0}", ex);
                    }
                }
            }
            finally
            {
                Utility.AsyncCloseICommunicationObject(client);
            }
        }

        /// <summary>
        /// Handles broker process exit event
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event args</param>
        private void BrokerProcess_Exited(object sender, EventArgs e)
        {
            TraceHelper.TraceEvent(this.sessionId, TraceEventType.Information, "[BrokerInfo] Broker process exited, Exit code = {0}", this.brokerProcess.ExitCode);
            TraceHelper.RuntimeTrace.LogBrokerWorkerProcessExited(this.brokerProcess.Id, this.brokerProcess.ExitCode);
            if (this.BrokerExited != null)
            {
                this.BrokerExited(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Creates a new client
        /// </summary>
        private BrokerManagementServiceClient CreateClient()
        {
            BrokerManagementServiceClient client =
                new BrokerManagementServiceClient(
                    BindingHelper.HardCodedBrokerManagementServiceBinding,
                    new EndpointAddress(SoaHelper.GetBrokerManagementServiceAddress(this.brokerProcess.Id)));
            client.InnerChannel.OperationTimeout = operationTimeoutForBrokerManagementService;
            return client;
        }
    }
}
