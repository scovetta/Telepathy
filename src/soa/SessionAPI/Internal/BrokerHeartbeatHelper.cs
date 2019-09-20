// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;
    using System.ServiceModel;
    using System.Threading;

    using Microsoft.Telepathy.Session.Interface;

    /// <summary>
    /// Provides a heartbeat helper for monitoring heartbeat from session API to broker worker
    /// </summary>
    internal class BrokerHeartbeatHelper : DisposableObject
    {
        /// <summary>
        /// How many heartbeats have been missed
        /// </summary>
        private volatile int missedHeartbeats;

        /// <summary>
        /// Whether the heartbeat should be reset on next timer tick
        /// </summary>
        private volatile bool ignoreNextHeartbeatInterval;

        /// <summary>
        /// Heartbeat timer
        /// </summary>
        private Timer heartbeatTimer;

        /// <summary>
        /// Stores broker launcher client factory
        /// </summary>
        private IBrokerLauncherClientFactoryForHeartbeat factory;

        /// <summary>
        /// Stores the heartbeat interval
        /// </summary>
        private int clientBrokerHeartbeatInterval;

        /// <summary>
        /// Stores the heartbeat retry count
        /// </summary>
        private int clientBrokerHeartbeatRetryCount;

        /// <summary>
        /// Stores the session id
        /// </summary>
        private string sessionId;

        /// <summary>
        /// if the server supports PingBroker2
        /// </summary>
        private bool supportNewPing = true;

        /// <summary>
        /// the expect ping result (will be set after the first PingBroker returns)
        /// </summary>
        private string expectPingResult;

        /// <summary>
        /// A flag indicating if hearbeat timer is stopped or not. if heartbeatStopped == 0, 
        /// heartbeat timer is running; if heartbeatStopped == 1, heartbeat timer is stopped
        /// </summary>
        private int heartbeatStopped;

        /// <summary>
        /// Initializes a new instance of the BrokerHeartbeatHelper class
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="clientBrokerHeartbeatInterval">indicating the heartbeat interval</param>
        /// <param name="clientBrokerHeartbeatRetryCount">indicating the retry count</param>
        /// <param name="factory">indicating the broker launcher client factory</param>
        public BrokerHeartbeatHelper(string sessionId, int clientBrokerHeartbeatInterval, int clientBrokerHeartbeatRetryCount, IBrokerLauncherClientFactoryForHeartbeat factory)
        {
            this.sessionId = sessionId;
            this.clientBrokerHeartbeatInterval = clientBrokerHeartbeatInterval;
            this.clientBrokerHeartbeatRetryCount = clientBrokerHeartbeatRetryCount;
            this.factory = factory;
            this.heartbeatTimer = new Timer(this.RunHeartbeat, null, clientBrokerHeartbeatInterval, clientBrokerHeartbeatInterval);
            this.heartbeatStopped = 0;
#if API
            SessionBase.TraceSource.TraceInformation("[BrokerHeartbeatHelper] Start heartbeat for session {0}, Interval = {1}, RetryCount = {2}", sessionId, clientBrokerHeartbeatInterval, clientBrokerHeartbeatRetryCount);
#elif WebAPI
            Microsoft.Hpc.RuntimeTrace.TraceHelper.TraceInfo(this.sessionId, "[BrokerHeartbeatHelper] Start heartbeat for session {0}, Interval = {1}, RetryCount = {2}", sessionId, clientBrokerHeartbeatInterval, clientBrokerHeartbeatRetryCount);
#endif
        }

        /// <summary>
        /// Initializes a new instance of the BrokerHeartbeatHelper class
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="clientBrokerHeartbeatInterval">indicating the heartbeat interval</param>
        /// <param name="clientBrokerHeartbeatRetryCount">indicating the retry count</param>
        /// <param name="factory">indicating the broker launcher client factory</param>
        /// <param name="expectPingResult">indicating the expect ping result</param>
        public BrokerHeartbeatHelper(string sessionId, int clientBrokerHeartbeatInterval, int clientBrokerHeartbeatRetryCount, IBrokerLauncherClientFactoryForHeartbeat factory, string expectPingResult)
            : this(sessionId, clientBrokerHeartbeatInterval, clientBrokerHeartbeatRetryCount, factory)
        {
            this.expectPingResult = expectPingResult;
        }

        /// <summary>
        /// Gets the event handler triggered when heartbeat lost
        /// </summary>
        public event EventHandler<BrokerHeartbeatEventArgs> HeartbeatLost;

        /// <summary>
        /// Resets the heartbeat when sending requests or getting responses
        /// </summary>
        public void Reset()
        {
            // Reset the missed heartbeat counter and ignore next tick
            this.ignoreNextHeartbeatInterval = true;
            this.missedHeartbeats = 0;

            if (1 == Interlocked.CompareExchange(ref this.heartbeatStopped, 0, 1))
            {
                // set heartbeatStopped to 0, and restart heartbeatTimer
                this.heartbeatTimer.Change(this.clientBrokerHeartbeatInterval, this.clientBrokerHeartbeatInterval);
            }
        }

        /// <summary>
        /// Dispose the instance of BrokerHeartbeatHelper class
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
#if API
                SessionBase.TraceSource.TraceInformation("[BrokerHeartbeatHelper] Start disposing...");
#elif WebAPI
                Microsoft.Hpc.RuntimeTrace.TraceHelper.TraceInfo(this.sessionId, "[BrokerHeartbeatHelper] Start disposing...");
#endif
                if (this.heartbeatTimer != null)
                {
                    ManualResetEvent waitForComplete = new ManualResetEvent(false);
                    try
                    {
                        if (this.heartbeatTimer.Dispose(waitForComplete))
                        {
                            // Block until all queued callback are completed
                            waitForComplete.WaitOne();
                        }
                    }
                    finally
                    {
                        try
                        {
                            waitForComplete.Close();
                        }
                        catch
                        {
                            // Swallow exception that might be thrown when closing an wait handle
                        }
                    }

                    this.heartbeatTimer = null;
                }

                this.factory.CloseBrokerLauncherClientForHeartbeat();

#if API
                SessionBase.TraceSource.TraceInformation("[BrokerHeartbeatHelper] Timer disposed.");
#elif WebAPI
                Microsoft.Hpc.RuntimeTrace.TraceHelper.TraceInfo(this.sessionId, "[BrokerHeartbeatHelper] Timer disposed.");
#endif
            }
        }

        /// <summary>
        /// Stops the heartbeat and cleans up related resources
        /// </summary>
        private void Stop()
        {
            try
            {
                if (0 == Interlocked.CompareExchange(ref this.heartbeatStopped, 1, 0))
                {
                    // set heartbeatStopped to 1, and stop heartbeatTimer
                    this.heartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            catch (ObjectDisposedException)
            {
                // Swallow object disposed exception in case race
                // condition might happen when disposing the timer
                // and the callback instance is calling Stop
            }

            this.factory.CloseBrokerLauncherClientForHeartbeat();
        }

        /// <summary>
        /// Timer callback for heartbeat
        /// </summary>
        /// <param name="state">unused state param</param>
        private void RunHeartbeat(object state)
        {
            // If the heartbeat interval needs to be ignored, do so now and return
            if (this.ignoreNextHeartbeatInterval)
            {
                this.ignoreNextHeartbeatInterval = false;
                return;
            }

            // If the timer is closed, return
            if (this.heartbeatTimer == null)
            {
                return;
            }

            bool isBrokerLoaded = false;
            bool pingSucceeded = false;

            try
            {
                // Connect to the broker launcher
                IBrokerLauncher launcher = this.factory.GetBrokerLauncherClientForHeartbeat();

                if (this.supportNewPing)
                {
                    try
                    {
                        string result = launcher.PingBroker2(this.sessionId);

#if API
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[BrokerHeartbeatHelper] Ping succeeded. Result = {0}, ExpectedResult = {1}", result, this.expectPingResult);
#endif

                        pingSucceeded = true;
                        if (this.expectPingResult == null)
                        {
                            this.expectPingResult = result;
                        }

                        if (result == Constant.PingBroker2Result_BrokerNotExist ||
                            result != this.expectPingResult)
                        {
                            isBrokerLoaded = false;
                        }
                        else
                        {
                            isBrokerLoaded = true;
                        }
                    }
                    catch (ActionNotSupportedException)
                    {
                        if (this.expectPingResult == null)
                        {
                            this.supportNewPing = false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (!this.supportNewPing)
                {
                    // Ping it
                    isBrokerLoaded = launcher.PingBroker(this.sessionId);
                    pingSucceeded = true;
                }

            }
            catch (Exception e)
            {
#if API
                SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[BrokerHeartbeatHelper] Exception occured when pinging broker: {0}", e);
#elif WebAPI
                Microsoft.Hpc.RuntimeTrace.TraceHelper.TraceInfo(this.sessionId, "[BrokerHeartbeatHelper] Exception occured when pinging broker: {0}", e);
#endif

                // If the max missed heartbeats have been hit
                if (++this.missedHeartbeats == this.clientBrokerHeartbeatRetryCount)
                {
                    // Signal the broker is down
                    this.SendBrokerDownSignal(true);

                    // Shutdown the heartbeat
                    this.Stop();
                }
                else
                {
                    // Recreate the broker controller client so it can be used for next ping
                    this.factory.CloseBrokerLauncherClientForHeartbeat();
                }
            }

            // If ping call succeeded
            if (pingSucceeded)
            {
                // If broker isnt loaded
                if (!isBrokerLoaded)
                {
                    // Signal the broker is down
                    this.SendBrokerDownSignal(false);

                    // Shutdown the heartbeat
                    this.Stop();
                }
                else
                {
                    // Suceeded so reset missed heartbeat count
                    this.missedHeartbeats = 0;
                }
            }
        }

        /// <summary>
        /// Sends broker down signal
        /// </summary>
        /// <param name="isBrokerNodeDown">indicating whether broker node is down</param>
        private void SendBrokerDownSignal(bool isBrokerNodeDown)
        {
#if API
            SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[BrokerHeartbeatHelper] Send broker down signal: IsBrokerNodeDown = {0}", isBrokerNodeDown);
#elif WebAPI
            Microsoft.Hpc.RuntimeTrace.TraceHelper.TraceInfo(this.sessionId, "[BrokerHeartbeatHelper] Send broker down signal: IsBrokerNodeDown = {0}", isBrokerNodeDown);
#endif

            if (this.HeartbeatLost != null)
            {
                this.HeartbeatLost.BeginInvoke(this, new BrokerHeartbeatEventArgs(isBrokerNodeDown), this.CallbackToEndInvokeEvent, null);
            }
        }

        /// <summary>
        /// Callback to call EndInvoke method of the heartbeat lost event
        /// </summary>
        /// <param name="result">indicating the async result</param>
        private void CallbackToEndInvokeEvent(IAsyncResult result)
        {
            try
            {
                this.HeartbeatLost.EndInvoke(result);
            }
            catch
            {
            }
        }
    }
}
