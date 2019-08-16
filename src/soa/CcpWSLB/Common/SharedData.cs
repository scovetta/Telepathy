//-----------------------------------------------------------------------
// <copyright file="SharedData.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Wrap the shared data amoung components</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Threading;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Configuration;

    /// <summary>
    /// Wrap the shared data amoung components
    /// </summary>
    internal sealed class SharedData : DisposableObjectSlim
    {
        /// <summary>
        /// Stores the timeout to wait for job finish
        /// </summary>
        private const int TimeoutToWaitForJobFinish = 5 * 60 * 1000;

        /// <summary>
        /// Stores the dispatcher count
        /// </summary>
        private int dispatcherCount;

        /// <summary>
        /// Stores the job finish wait handle
        /// </summary>
        private ManualResetEvent jobFinishWaitHandle = new ManualResetEvent(false);

        /// <summary>
        /// Stores the wait handle wait for initialization complete
        /// </summary>
        private ManualResetEvent waitForInitializationComplete = new ManualResetEvent(false);

        /// <summary>
        /// Stores the broker info
        /// </summary>
        private BrokerStartInfo brokerInfo;

        /// <summary>
        /// Stores the session start info
        /// </summary>
        private SessionStartInfoContract startInfo;

        /// <summary>
        /// Stores the broker configurations
        /// </summary>
        private BrokerConfigurations config;

        /// <summary>
        /// Stores the service configuration
        /// </summary>
        private ServiceConfiguration serviceConfig;

        /// <summary>
        /// Stores a value indicating whether the session is failed
        /// </summary>
        private bool sessionFailed;

        /// <summary>
        /// Stores the initializing flag
        /// </summary>
        private bool initializing = true;

        /// <summary>
        /// Initializes a new instance of the SharedData class
        /// </summary>
        /// <param name="brokerInfo">indicating the broker info</param>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="config">indicating the broker configuration</param>
        /// <param name="serviceCnfig">indicating the service configuration</param>
        public SharedData(BrokerStartInfo brokerInfo, SessionStartInfoContract startInfo, BrokerConfigurations config, ServiceConfiguration serviceConfig)
        {
            this.brokerInfo = brokerInfo;
            this.startInfo = startInfo;
            this.config = config;
            this.serviceConfig = serviceConfig;
        }

        /// <summary>
        /// Gets the broker info
        /// </summary>
        public BrokerStartInfo BrokerInfo
        {
            get { return this.brokerInfo; }
        }

        /// <summary>
        /// Gets the session start info
        /// </summary>
        public SessionStartInfoContract StartInfo
        {
            get { return this.startInfo; }
        }

        /// <summary>
        /// Gets the broker configurations
        /// </summary>
        public BrokerConfigurations Config
        {
            get { return this.config; }
        }

        /// <summary>
        /// Gets the service configuration
        /// </summary>
        public ServiceConfiguration ServiceConfig
        {
            get { return this.serviceConfig; }
        }

        /// <summary>
        /// Gets or sets the broker observer
        /// </summary>
        public BrokerObserver Observer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the dispatcher count
        /// </summary>
        public int DispatcherCount
        {
            get { return this.dispatcherCount; }
            set { this.dispatcherCount = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the session is failed
        /// </summary>
        public bool SessionFailed
        {
            get { return this.sessionFailed; }
            set { this.sessionFailed = value; }
        }

        /// <summary>
        /// Gets a value indicating whether it is initializing
        /// </summary>
        public bool Initializing
        {
            get { return this.initializing; }
        }

        /// <summary>
        /// Informs that initialization is finished
        /// </summary>
        public void InitializationFinished()
        {
            this.initializing = false;
            this.waitForInitializationComplete.Set();
        }

        /// <summary>
        /// Wait for job finish
        /// </summary>
        public void WaitForJobFinish()
        {
            if (!this.jobFinishWaitHandle.WaitOne(TimeoutToWaitForJobFinish, false))
            {
                TraceHelper.TraceWarning(this.brokerInfo.SessionId, "[SharedData] Timeout to wait for job finish. Exit and let session launcher to fail the service job.");
            }
        }

        /// <summary>
        /// Wait for initialization complete
        /// </summary>
        public void WaitForInitializationComplete()
        {
            this.waitForInitializationComplete.WaitOne();
        }

        /// <summary>
        /// Informs that the job is finished
        /// </summary>
        public void JobFinished()
        {
            this.jobFinishWaitHandle.Set();
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        protected override void DisposeInternal()
        {
            base.DisposeInternal();

            try
            {
                this.jobFinishWaitHandle.Close();
                this.jobFinishWaitHandle = null;
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceWarning("[SharedData].DisposeInternal: Exception {0}", ex);
            }

            try
            {
                this.waitForInitializationComplete.Close();
                this.waitForInitializationComplete = null;
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceWarning("[SharedData].DisposeInternal: Exception {0}", ex);
            }
        }
    }
}
