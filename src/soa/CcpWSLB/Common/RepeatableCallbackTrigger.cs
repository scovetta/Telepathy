//-----------------------------------------------------------------------
// <copyright file="RepeatableCallbackTrigger.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Trigger for repeatable callback</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Trigger for repeatable callback
    /// </summary>
    internal class RepeatableCallbackTrigger : IDisposable
    {
        /// <summary>
        /// Store the call back list
        /// </summary>
        private LinkedList<TimerInfo> timerList;

        /// <summary>
        /// Stores a value indicating whether the trigger is started
        /// </summary>
        private bool started;

        /// <summary>
        /// Stores a value indicating whether the trigger is stoped
        /// </summary>
        private bool stoped;

        /// <summary>
        /// Stores the timer callback
        /// </summary>
        private TimerCallback timerCallback;
        
        /// <summary>
        /// Stores the timer to trigger the callback
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the RepeatableCallbackTrigger class
        /// </summary>
        public RepeatableCallbackTrigger()
        {
            this.timerList = new LinkedList<TimerInfo>();
            this.timerCallback = new ThreadHelper<object>(new TimerCallback(this.TimerCallback)).CallbackRoot;
            this.timer = new Timer(this.timerCallback, null, -1, -1);
        }

        /// <summary>
        /// Finalizes an instance of the RepeatableCallbackTrigger class
        /// </summary>
        ~RepeatableCallbackTrigger()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Indicating that the init phace is finished
        /// </summary>
        public void Start()
        {
            if (this.timerList.Count == 0)
            {
                throw new InvalidOperationException("Must add some callback before you start the timer");
            }

            int timeoutInterval = this.timerList.First.Value.CallbackTime.Subtract(DateTime.Now).Milliseconds;
            if (timeoutInterval < 0)
            {
                timeoutInterval = 0;
            }

            BrokerTracing.TraceVerbose(
                "[RepeatableCallbackTrigger].Start: Register wait, timeoutInterval={0}",
                timeoutInterval);

            this.timer?.Change(timeoutInterval, -1);
            this.started = true;
        }

        /// <summary>
        /// Register callback
        /// </summary>
        /// <param name="span">indicate the time span</param>
        /// <param name="firstCallBack">indicate the time to wait before the first callback</param>
        /// <param name="callback">indicate the callback</param>
        /// <param name="state">indicate the async state</param>
        /// <returns>a token used to reset the callback</returns>
        public object RegisterCallback(TimeSpan span, TimeSpan firstCallBack, WaitCallback callback, object state)
        {
            // This operation is only allowed before init
            if (this.started)
            {
                throw new InvalidOperationException("Register callback is only allowed before started");
            }

            TimerInfo info = new TimerInfo(span, callback, state, firstCallBack);

            // Insert the timer info into the list
            // This could only happens before init, so no lock is needed here
            return this.InsertTimerInfo(info);
        }

        /// <summary>
        /// Register callback
        /// </summary>
        /// <param name="span">indicate the time span</param>
        /// <param name="callback">indicate the callback</param>
        /// <param name="state">indicate the async state</param>
        /// <returns>a token used to reset the callback</returns>
        public object RegisterCallback(TimeSpan span, WaitCallback callback, object state)
        {
            return this.RegisterCallback(span, span, callback, state);
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.stoped = true;

                try
                {
                    this.timer?.Dispose();
                    this.timer = null;
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[RepeatableCallbackTrigger] Exception thrown while disposing: {0}", e);
                }
            }
        }

        /// <summary>
        /// Callback when timer hits
        /// </summary>
        /// <param name="state">indicate the async state</param>
        /// <param name="timedOut">indicate that the callback is hit by timeout or wait handle set</param>
        private void TimerCallback(object state)
        {
            if (this.stoped)
            {
                return;
            }

            List<TimerInfo> list = new List<TimerInfo>();

            // Lock the timer list to retrieve the timer info that should be triggered before now
            lock (this.timerList)
            {
                while (this.timerList.First != null && this.timerList.First.Value.CallbackTime <= DateTime.Now)
                {
                    list.Add(this.timerList.First.Value);
                    this.timerList.RemoveFirst();
                }
            }

            foreach (TimerInfo info in list)
            {
                info.Callback(info.State);

                // Reregister the timer info
                info.Reset();

                this.InsertTimerInfo(info);
            }

            // Reregister the callback
            int timeoutInterval;
            if (this.timerList.Count == 0)
            {
                timeoutInterval = -1;
            }
            else
            {
                timeoutInterval = (int)this.timerList.First.Value.CallbackTime.Subtract(DateTime.Now).TotalMilliseconds;
                if (timeoutInterval < 0)
                {
                    timeoutInterval = 0;
                }
            }

            this.timer?.Change(timeoutInterval, -1);
        }

        /// <summary>
        /// Insert the timer info
        /// </summary>
        /// <param name="info">indicate the timer info</param>
        /// <returns>the node in the timer list</returns>
        private LinkedListNode<TimerInfo> InsertTimerInfo(TimerInfo info)
        {
            if (this.timerList.Count == 0)
            {
                this.timerList.AddFirst(info);
                return this.timerList.First;
            }

            LinkedListNode<TimerInfo> pointer = this.timerList.First;
            LinkedListNode<TimerInfo> node = null;
            while (pointer != null)
            {
                if (info.CallbackTime < pointer.Value.CallbackTime)
                {
                    node = this.timerList.AddBefore(pointer, info);
                    break;
                }

                pointer = pointer.Next;
            }

            if (node == null)
            {
                node = this.timerList.AddLast(info);
            }

            return node;
        }

        /// <summary>
        /// Stores the info for a timer
        /// </summary>
        private class TimerInfo
        {
            /// <summary>
            /// Stores the span
            /// </summary>
            private TimeSpan span;

            /// <summary>
            /// Stores the timer callback
            /// </summary>
            private WaitCallback callback;

            /// <summary>
            /// Stores the exact date time that the callback should be called
            /// </summary>
            private DateTime callbackTime;

            /// <summary>
            /// Stores the async state
            /// </summary>
            private object state;

            /// <summary>
            /// Initializes a new instance of the TimerInfo class
            /// </summary>
            /// <param name="span">indicating the time span</param>
            /// <param name="callback">indicating the callback</param>
            /// <param name="state">indicating the state</param>
            /// <param name="firstCallback">indicating the time to wait before the first callback</param>
            public TimerInfo(TimeSpan span, WaitCallback callback, object state, TimeSpan? firstCallback = null)
            {
                this.span = span;
                this.callbackTime = DateTime.Now.Add(firstCallback ?? this.span);
                this.callback = callback;
                this.state = state;
            }

            /// <summary>
            /// Gets or sets the async state
            /// </summary>
            public object State
            {
                get { return this.state; }
            }

            /// <summary>
            /// Gets the exact date time that the callback should be called
            /// </summary>
            public DateTime CallbackTime
            {
                get { return this.callbackTime; }
            }

            /// <summary>
            /// Gets or sets the timer callback
            /// </summary>
            public WaitCallback Callback
            {
                get { return this.callback; }
            }

            /// <summary>
            /// Reset the callback time
            /// </summary>
            public void Reset()
            {
                this.callbackTime = DateTime.Now.Add(this.span);
            }
        }
    }

    internal class WaitTaskInfo
    {
        public RegisteredWaitHandle Handle = null;
    }
}
