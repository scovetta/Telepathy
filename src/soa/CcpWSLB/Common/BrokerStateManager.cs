// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common
{
    using System;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.RuntimeTrace;

    /// <summary>
    /// Broker's state manager
    /// </summary>
    internal sealed class BrokerStateManager : IDisposable
    {
        /// <summary>
        /// Stores the broker state
        /// </summary>
        private BrokerState state;

        /// <summary>
        /// Stores the lock object for state
        /// </summary>
        private object lockState = new object();

        /// <summary>
        /// Stores the lock object for unloading broker
        /// </summary>
        private object lockUnloadBroker = new object();

        /// <summary>
        /// Stores the flag for unload broker
        /// </summary>
        private bool unloadBroker;

        /// <summary>
        /// Stores a flag indicating whether the broker is unload to suspended or not
        /// </summary>
        private bool unloadToSuspend;

        /// <summary>
        /// Stores the timeout manager
        /// </summary>
        private TimeoutManager timeoutManager;

        /// <summary>
        /// Stores the shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// Initializes a new instance of the BrokerStateManager class
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="clientAvaliable">indicating whether there is client avaliable when starting up</param>
        public BrokerStateManager(SharedData sharedData, bool clientAvaliable)
        {
            this.sharedData = sharedData;
            this.timeoutManager = new TimeoutManager("BrokerStateManager");
            this.state = BrokerState.Started;
            this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.ClientConnectionTimeout, clientAvaliable ? new WaitCallback(this.TimeoutToSuspended) : new WaitCallback(this.TimeoutToFinish), null);
            BrokerTracing.TraceInfo("[BrokerStateManager] Successfully initialized BrokerStateManager: State = {0}", this.state);
        }

        /// <summary>
        /// Finalizes an instance of the BrokerStateManager class
        /// </summary>
        ~BrokerStateManager()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Define the delegate for UnloadBorker event
        /// </summary>
        /// <param name="suspend">indicating the unload event is to suspend or to finish</param>
        public delegate void UnloadBrokerEventHandler(bool suspend);

        /// <summary>
        /// Define the delegate for SessionFailed event
        /// </summary>
        public delegate void SessionFailedEventHandler();

        /// <summary>
        /// Gets the unload broker event
        /// </summary>
        public event UnloadBrokerEventHandler UnloadBroker;

        /// <summary>
        /// Gets the session failed event
        /// </summary>
        public event SessionFailedEventHandler OnFailed;

        /// <summary>
        /// Close the broker state manager
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Informs that a client has connectted
        /// </summary>
        public void ClientConnected()
        {
            BrokerTracing.TraceInfo("[BrokerStateManager] A client has connectted.");
            lock (this.lockState)
            {
                if (this.state == BrokerState.Started || this.state == BrokerState.Idle)
                {
                    BrokerTracing.TraceInfo("[BrokerStateManager] State: {0} ==> Running", this.state);
                    this.state = BrokerState.Running;
                    this.timeoutManager.Stop();
                }
            }
        }

        /// <summary>
        /// Informs that all clients have disconnectted
        /// </summary>
        public void AllClientsDisconnected()
        {
            BrokerTracing.TraceInfo("[BrokerStateManager] All clients have disconnectted.");
            lock (this.lockState)
            {
                if (this.state == BrokerState.Running)
                {
                    BrokerTracing.TraceInfo("[BrokerStateManager] State: Running ==> Idle");
                    this.state = BrokerState.Idle;
                    this.timeoutManager.RegisterTimeout(this.sharedData.Config.Monitor.SessionIdleTimeout, this.TimeoutToSuspended, null);
                }
            }
        }

        /// <summary>
        /// Indicate that the service failed
        /// </summary>
        public void ServiceFailed()
        {
            BrokerTracing.TraceInfo("[BrokerStateManager] Service failed.");

            // Do not need lock here
            if (this.sharedData.SessionFailed)
            {
                return;
            }

            this.sharedData.SessionFailed = true;
            if (this.OnFailed != null)
            {
                this.OnFailed();
            }

            this.SyncUnloadBroker(this.sharedData.BrokerInfo.Durable);
        }

        /// <summary>
        /// Informs that a client is attaching
        /// Throw exception if broker does not allow it
        /// </summary>
        public void Attach()
        {
            lock (this.lockUnloadBroker)
            {
                if (this.unloadBroker)
                {
                    ThrowBrokerUnloadingException(this.unloadToSuspend, this.sharedData.BrokerInfo.SessionId);
                }
                else
                {
                    if (!this.timeoutManager.ResetTimeout())
                    {
                        // Callback is already triggering
                        ThrowBrokerUnloadingException(this.unloadToSuspend, this.sharedData.BrokerInfo.SessionId);
                    }
                }
            }
        }

        /// <summary>
        /// Dispose the BrokerStateManager
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Throws an exception indicating that broker is unloading
        /// </summary>
        /// <param name="unloadToSuspend">indicating whether the broker is unloading to suspended or not</param>
        /// <param name="sessionId">indicating the session id</param>
        private static void ThrowBrokerUnloadingException(bool unloadToSuspend, string sessionId)
        {
            if (unloadToSuspend)
            {
                ThrowHelper.ThrowSessionFault(Microsoft.Hpc.Scheduler.Session.SOAFaultCode.Broker_BrokerSuspending, SR.BrokerSuspending);
            }
            else
            {
                ThrowHelper.ThrowSessionFault(Microsoft.Hpc.Scheduler.Session.SOAFaultCode.Session_ValidateJobFailed_AlreadyFinished, SR.Session_ValidateJobFailed_AlreadyFninshed, sessionId.ToString());
            }
        }

        /// <summary>
        /// Dispose the BrokerStateManager
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.timeoutManager.Dispose();
            }
        }

        /// <summary>
        /// Sync unload broker
        /// </summary>
        /// <param name="suspended">indicate whether suspend</param>
        private void SyncUnloadBroker(bool suspended)
        {
            lock (this.lockUnloadBroker)
            {
                if (!this.unloadBroker)
                {
                    this.unloadBroker = true;
                    this.unloadToSuspend = suspended;
                    this.timeoutManager.Stop();

                    // Make sure the initialization is completed before broker unload itself
                    this.sharedData.WaitForInitializationComplete();

                    // Make sure the job is finished before the broker unload itself
                    this.sharedData.WaitForJobFinish();

                    if (this.sharedData.SessionFailed)
                    {
                        if (suspended)
                        {
                            TraceHelper.RuntimeTrace.LogSessionSuspendedBecauseOfJobCanceled(this.sharedData.BrokerInfo.SessionId);
                        }
                        else
                        {
                            TraceHelper.RuntimeTrace.LogSessionFinishedBecauseOfJobCanceled(this.sharedData.BrokerInfo.SessionId);
                        }
                    }
                    else
                    {
                        if (suspended)
                        {
                            TraceHelper.RuntimeTrace.LogSessionSuspendedBecauseOfTimeout(this.sharedData.BrokerInfo.SessionId);
                        }
                        else
                        {
                            TraceHelper.RuntimeTrace.LogSessionFinishedBecauseOfTimeout(this.sharedData.BrokerInfo.SessionId);
                        }
                    }

                    this.UnloadBroker(suspended);
                }
            }
        }

        /// <summary>
        /// Timeout to suspended state
        /// </summary>
        /// <param name="state">null object</param>
        private void TimeoutToSuspended(object state)
        {
            BrokerTracing.TraceInfo("[BrokerStateManager] Timeout to suspended triggered.");
            bool flag = false;
            lock (this.lockState)
            {
                if (this.state == BrokerState.Idle || this.state == BrokerState.Started)
                {
                    BrokerTracing.TraceInfo("[BrokerStateManager] State: {0} ==> Suspend", this.state);
                    this.state = BrokerState.Suspend;
                    flag = true;
                }
            }

            if (flag)
            {
                this.SyncUnloadBroker(this.sharedData.BrokerInfo.Durable);
            }
        }

        /// <summary>
        /// Timeout to finish state
        /// </summary>
        /// <param name="state">null object</param>
        private void TimeoutToFinish(object state)
        {
            BrokerTracing.TraceInfo("[BrokerStateManager] Timeout to finish triggered.");
            bool flag = false;
            lock (this.lockState)
            {
                if (this.state == BrokerState.Started)
                {
                    BrokerTracing.TraceInfo("[BrokerStateManager] State: Started ==> Finish");
                    this.state = BrokerState.Finished;
                    flag = true;
                }
            }

            if (flag)
            {
                this.SyncUnloadBroker(false);
            }
        }
    }
}
