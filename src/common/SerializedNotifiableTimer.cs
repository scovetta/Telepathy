using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Microsoft.Hpc
{
    /// <summary>
    /// A timer class. The difference than normal timer are:
    /// 1. the timer could be wake up while it's waiting for next tick.
    /// 2. only one thread could execute the callback at a time. See summary of OnEvent
    ///    its constrain on the callback
    /// </summary>
    internal class SerializedNotifiableTimer : IDisposable
    {
        AutoResetEvent myEvent;
        RegisteredWaitHandle waitHandle;
        int handling;
        int interval;
        TimerCallback callback;
        object state;

        internal SerializedNotifiableTimer(TimerCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            myEvent = new AutoResetEvent(false);
        }

        ~SerializedNotifiableTimer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            AutoResetEvent eventToDispose = Interlocked.Exchange(ref myEvent, null);
            RegisteredWaitHandle waitHandleToDispose = Interlocked.Exchange(ref waitHandle, null);

            if (disposing)
            {
                if (waitHandleToDispose != null)
                {
                    waitHandleToDispose.Unregister(myEvent);
                }


                if (eventToDispose != null)
                {
                    eventToDispose.Close();
                }
            }
        }

        internal void Start(int interval)
        {
            Debug.Assert(waitHandle == null);
            this.interval = interval;
            waitHandle = ThreadPool.RegisterWaitForSingleObject(myEvent, OnEvent, state, interval, true);
        }

        internal void Stop()
        {
            Debug.Assert(waitHandle != null);
            waitHandle.Unregister(myEvent);
            waitHandle = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void Notify()
        {
            myEvent.Set();
        }

        /// <summary>
        /// Method to handle the event or timeout.
        /// Attention: Only one thread could run this function at a time. 
        /// If previous request hasn't be handled, we give up this one. In this case,
        /// the event could already been reset so the wakeup request is "lost". 
        /// The callback has to be able to handle this case in next tick.
        /// </summary>
        /// <param name="tickState"></param>
        /// <param name="timeout"></param>
        void OnEvent(object tickState, bool timeout)
        {
            //the timer may already be disposed
            if (myEvent == null)
            {
                return;
            }

            Exception ex = null;    
            try
            {
                if (Interlocked.CompareExchange(ref handling, 1, 0) == 1)
                {   
                    return;
                }
                try
                {
                    callback(tickState);
                }
                finally
                {
                    Interlocked.Exchange(ref handling, 0);
                }
            }
            finally
            {
                try
                {
                    //make a local copy to avoid race with Dispose in aother thread
                    AutoResetEvent eventToWait = myEvent;
                    if (eventToWait != null)
                    {
                        waitHandle = ThreadPool.RegisterWaitForSingleObject(eventToWait, OnEvent, state, interval, true);
                    }
                }
                catch (Exception e)
                {
                    //swallow exception here because the event may be closed by other threads
                    if (!ExceptionHelper.IsCatchableException(e))
                    {
                        ex = e;
                    }
                }
            }
            if (ex != null)
            {
                throw ex;
            }
        }
    }
}