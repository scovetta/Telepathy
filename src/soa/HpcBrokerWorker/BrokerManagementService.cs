// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerShim
{
    using System;
    using System.ServiceModel;
    using System.Threading;

    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// The Broker Management Service
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    internal class BrokerManagementService : IBrokerManagementService
    {
        /// <summary>
        /// Stores the env name for session id
        /// </summary>
        private const string SessionIdEnv = "HPC_SESSIONID";

        /// <summary>
        /// Stores the broker entry instance
        /// </summary>
        private BrokerEntry entry;

        /// <summary>
        /// Stores the exit wait handle
        /// </summary>
        private ManualResetEvent exitWaitHandle;

        /// <summary>
        /// Stores a value indicating whether the broker has been closed
        /// </summary>
        private bool closed;

        /// <summary>
        /// Initializes a new instance of the BrokerManagementService class
        /// </summary>
        /// <param name="exitWaitHandle">indicating the exit wait handle</param>
        public BrokerManagementService(ManualResetEvent exitWaitHandle)
        {
            this.exitWaitHandle = exitWaitHandle;
        }

        /// <summary>
        /// Ask broker to initialize
        /// </summary>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        /// <returns>returns broker initialization result</returns>
        public BrokerInitializationResult Initialize(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo)
        {
            ParamCheckUtility.ThrowIfNull(startInfo, "startInfo");
            ParamCheckUtility.ThrowIfNull(brokerInfo, "brokerInfo");

            // Concurrent mode is set to single so no need to lock here
            if (this.entry != null)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_AlreadyInitialized, SR.AlreadyInitialized);
            }

            // Notice: We get the session Id here and expect to change the
            // cosmos log file name to include session id. But cosmos log has a
            // limitation that it is a static class without close method, and
            // it can't be initialized twice. HpcTraceListener is just a wrapper
            // for that static class. So creating a new HpcTraceListener can not
            // workaround this issue.
            TraceHelper.TraceInfo(
                brokerInfo.SessionId,
                "[BrokerManagementService].Initialize: Broker worker initializes for session {0}",
                brokerInfo.SessionId);

            // Set the configuration indicating enable/disable diag trace
            SoaDiagTraceHelper.SetDiagTraceEnabledFlag(brokerInfo.SessionId, brokerInfo.EnableDiagTrace);
            TraceHelper.IsDiagTraceEnabled = SoaDiagTraceHelper.IsDiagTraceEnabled;

            Environment.SetEnvironmentVariable(SessionIdEnv, brokerInfo.SessionId.ToString(), EnvironmentVariableTarget.Process);

#if HPCPACK
            // create the session id mapping file
            foreach (TraceListener listener in TraceHelper.RuntimeTrace.CosmosTrace.Listeners)
            {
                if (listener is HpcTraceListener)
                {
                    HpcTraceListener hpcListener = listener as HpcTraceListener;
                    if (hpcListener.IsPerSessionLogging)
                    {
                        string logIdFileName = string.Format("{0}_{1}", HpcTraceListener.LogFileBaseName, brokerInfo.SessionId);
                        string logIdFilePath = Path.Combine(HpcTraceListener.LogDir, logIdFileName);
                        try
                        {
                            using (FileStream file = File.Open(logIdFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
                            {
                            }

                        }
                        catch (Exception e)
                        {
                            TraceHelper.TraceError(brokerInfo.SessionId,
                                "[BrokerManagementService].Initialize: Create log session Id match file failed with Exception {0}",
                                e.ToString());
                        }
                    }
                }
            }
#endif

            this.entry = new BrokerEntry(brokerInfo.SessionId);
            this.entry.BrokerFinished += new EventHandler(this.Entry_BrokerFinished);
            return this.entry.Run(startInfo, brokerInfo);
        }

        /// <summary>
        /// Attach to the broker
        /// broker would throw exception if it does not allow client to attach to it
        /// </summary>
        public void Attach()
        {
            this.CheckInitialization();
            this.entry.Attach();
        }

        /// <summary>
        /// Ask to close the broker
        /// </summary>
        /// <param name="suspended">indicating whether the broker is asked to be suspended or closed</param>
        public void CloseBroker(bool suspended)
        {
            this.CheckInitialization();
            this.CheckClose();

            if (suspended)
            {
                TraceHelper.RuntimeTrace.LogSessionSuspended(this.entry.SessionId);
            }
            else
            {
                TraceHelper.RuntimeTrace.LogSessionFinished(this.entry.SessionId);
            }

            this.entry.Close(!suspended).GetAwaiter().GetResult();
            this.closed = true;
        }

        private void CheckClose()
        {
            if (this.closed)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_AlreadyClosed, SR.AlreadyClosed);
            }
        }

        /// <summary>
        /// Check if the broker has been initialized and throw exception if not so
        /// </summary>
        private void CheckInitialization()
        {
            if (this.entry == null)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_NotInitialized, SR.NotInitialized);
            }
        }

        /// <summary>
        /// Event triggered when broker finished
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event args</param>
        private void Entry_BrokerFinished(object sender, EventArgs e)
        {
            // Set the exit wait handle to allow process exit
            this.exitWaitHandle.Set();
        }
    }
}
