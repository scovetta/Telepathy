//-----------------------------------------------------------------------
// <copyright file="ThreadHelper.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Provide root entry for thread pool thread and async call back thread</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Provide root entry for thread pool thread and async call back thread
    /// </summary>
    /// <typeparam name="T">
    /// type of the callback state object
    /// object for WaitCallback
    /// IAsyncResult for AsyncCallback
    /// </typeparam>
    internal class ThreadHelper<T>
    {
        /// <summary>
        /// Stores the callback delegate
        /// </summary>
        private Delegate callbackDelegate;

        /// <summary>
        /// Initializes a new instance of the ThreadHelper class
        /// </summary>
        /// <param name="callbackDelegate">indicating the callback delegate</param>
        public ThreadHelper(Delegate callbackDelegate)
        {
            this.callbackDelegate = callbackDelegate;
        }

        /// <summary>
        /// Callback root method
        /// </summary>
        /// <param name="obj">indicating the state</param>
        public void CallbackRoot(T obj)
        {
            try
            {
                this.callbackDelegate.DynamicInvoke(obj);
            }
            catch (ThreadAbortException)
            {
            }
            catch (AppDomainUnloadedException)
            {
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[ThreadHelper] Exception catched at root: {0}", e);
            }
        }

        /// <summary>
        /// WaitOrTimerCallback root method
        /// </summary>
        /// <param name="state">indicate the state</param>
        /// <param name="timedOut">indicate the timedOut</param>
        public void WaitOrTimerCallbackRoot(object state, bool timedOut)
        {
            try
            {
                this.callbackDelegate.DynamicInvoke(state, timedOut);
            }
            catch (ThreadAbortException)
            {
            }
            catch (AppDomainUnloadedException)
            {
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[ThreadHelper] Exception catched at root: {0}", e);
            }
        }
    }
}
