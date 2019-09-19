// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Provide management to a timeout
    /// </summary>
    internal sealed class TimeoutManager : IDisposable
    {
        /// <summary>
        /// Stores the reset time span
        /// </summary>
        private static readonly TimeSpan ResetTimeSpan = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets the Timeout inaccuracy limit
        /// </summary>
        private const int TimeoutInaccuracyLimit = 1100;

        /// <summary>
        /// Stores the name
        /// </summary>
        private string name;

        /// <summary>
        /// Stores the lock this object
        /// </summary>
        private object lockThis = new object();

        /// <summary>
        /// Stores the timeout info
        /// </summary>
        private TimeoutInfo info;

        /// <summary>
        /// Stores the timer
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Stores the last reset time
        /// </summary>
        private DateTime lastResetTime;

        /// <summary>
        /// Stores a value indicating whether callback is triggering
        /// </summary>
        private bool callbackTriggering;

        /// <summary>
        /// Initializes a new instance of the TimeoutManager class
        /// </summary>
        /// <param name="name">indicating the name of this timeout manager</param>
        public TimeoutManager(string name)
        {
            this.name = name;
            TimerCallback timerCallback = new ThreadHelper<object>(new TimerCallback(this.Callback)).CallbackRoot;
            this.timer = new Timer(timerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Finalizes an instance of the TimeoutManager class
        /// </summary>
        ~TimeoutManager()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Register the timeout
        /// If there's another timeout registered before, the timeout will be replaced
        /// </summary>
        /// <param name="span">time span to trigger the timeout</param>
        /// <param name="callback">callback for the timeout</param>
        /// <param name="state">indicate the async state</param>
        public void RegisterTimeout(int span, WaitCallback callback, object state)
        {
            BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[TimeoutManager] {2} Timeout registered: Span = {0}, State = {1}", span, state, this.name);

            // Change the timer to prevent the timer trigger when updating info
            lock (this.lockThis)
            {
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.info = new TimeoutInfo();
                this.info.Span = span;
                this.info.Callback = callback;
                this.info.State = state;

                this.lastResetTime = DateTime.Now;
                this.timer.Change(span, span);
            }
        }

        /// <summary>
        /// Stop the current timeout
        /// </summary>
        public void Stop()
        {
            if (this.info != null)
            {
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[TimeoutManager] {0} Timeout stopped", this.name);
                lock (this.lockThis)
                {
                    if (this.info != null)
                    {
                        this.info = null;
                        this.lastResetTime = DateTime.Now;
                        this.timer.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                }
            }
        }

        /// <summary>
        /// Reset the timeout
        /// </summary>
        /// <returns>
        /// returns false if the callback is already triggering
        /// </returns>
        public bool ResetTimeout()
        {
            // DateTime struct is thread safe
            if (DateTime.Now.Subtract(this.lastResetTime) > ResetTimeSpan)
            {
                lock (this.lockThis)
                {
                    if (this.info != null)
                    {
                        this.lastResetTime = DateTime.Now;
                        this.timer.Change(this.info.Span, this.info.Span);
                    }

                    if (this.callbackTriggering)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Dispose the timeout manager
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the timeout manager
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.timer != null)
                {
                    this.timer.Dispose();
                    this.timer = null;
                }
            }
        }

        /// <summary>
        /// Callback when timeout triggers
        /// </summary>
        /// <param name="state">indicating the state</param>
        private void Callback(object state)
        {
            BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[TimeoutManager] {0} Timeout triggered: State = {1}", this.name, state);
            TimeoutInfo info;
            lock (this.lockThis)
            {
                info = this.info;
                if (info == null)
                {
                    return;
                }

                if (DateTime.Now.Subtract(this.lastResetTime) < TimeSpan.FromMilliseconds(info.Span * 0.9))
                {
                    // If the time last from last reset is shorter than the real timeout (info.Span), it means that the callback is raised before the reset is called and should be returned with no action
                    BrokerTracing.TraceWarning("[TimeoutManager] The timer is already reset. Do no action and return. LastResetTime = {0}", this.lastResetTime);
                    return;
                }

                // Set the callback triggering flag
                this.callbackTriggering = true;
            }

            try
            {
                info.Callback(info.State);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[TimeoutManager] Callback rasied exception: {0}", e);
            }
            finally
            {
                // reset the callback triggering flag
                this.callbackTriggering = false;
            }
        }

        /// <summary>
        /// Timeout info
        /// </summary>
        private class TimeoutInfo
        {
            /// <summary>
            /// Stores the span
            /// </summary>
            private int span;

            /// <summary>
            /// Stores the async state
            /// </summary>
            private object state;

            /// <summary>
            /// Stores the callback
            /// </summary>
            private WaitCallback callback;

            /// <summary>
            /// Gets or sets the span
            /// </summary>
            public int Span
            {
                get { return this.span; }
                set { this.span = value; }
            }

            /// <summary>
            /// Gets or sets the callback
            /// </summary>
            public WaitCallback Callback
            {
                get { return this.callback; }
                set { this.callback = value; }
            }

            /// <summary>
            /// Gets or sets the async state
            /// </summary>
            public object State
            {
                get { return this.state; }
                set { this.state = value; }
            }
        }
    }
}
