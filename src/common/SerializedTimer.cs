//------------------------------------------------------------------------------
// <copyright file="SerializedTimer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#region Using directives

using System;
using System.Diagnostics;
using System.Threading;

#endregion

namespace Microsoft.Hpc
{
    /// <summary>
    /// A timer which won't fire new timer event until the previous one is finished
    /// </summary>
    public class SerializedTimer : IDisposable
    {        
        /// <summary>
        /// the timer
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The callback for timer
        /// </summary>
        private TimerCallback callback;

        /// <summary>
        /// A flag indicating if one timer callback is being executed.
        /// 0 means not ticking, 1 means ticking
        /// </summary>
        private int ticking;

        /// <summary>
        /// If the object is disposed
        /// </summary>
        bool isDisposed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public SerializedTimer(TimerCallback callback, object state)
        {
            this.callback = callback;
            timer = new Timer(new TimerCallback(Tick), state, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// IDispose.Dispose
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Dispose worker
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            // Dispose / cleanup managed objects.  
            if (disposing)
            {
                timer?.Dispose();
                timer = null;
            }

            // In all cases, null out interesting/large objects
            isDisposed = true;
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        public void Start(int due, int interval)
        {
            timer.Change(due, interval);
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite); //check back in 30 seconds
        }

        /// <summary>
        /// The tick function, it's guaranteed that only one instance is running
        /// </summary>
        /// <param name="state"></param>
        private void Tick(object state)
        {
            //try to set the flag, if someone is already in progress, we yield
            if (Interlocked.Exchange(ref ticking, 1) == 1)
            {
                return;
            }

            try
            {
                callback(state);
            }
            catch (Exception)
            {
                // The timer should not be crashed by any exception

                // Previously we set it to swallow only catchable exceptions, then we found a case
                // that at the time the scheduler service stops, it will stop the timer then dispose 
                // the store object. However even the timer is stopped, the last tick can still be
                // running. In this way, the tick can throw a null reference exception and crash
                // the process.
            }
            finally
            {
                //always set the flag back
                Interlocked.Exchange(ref ticking, 0);
            }
        }

    }

}

