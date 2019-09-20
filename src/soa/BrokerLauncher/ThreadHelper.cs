// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    using Microsoft.Telepathy.RuntimeTrace;

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
        /// Stores the session id
        /// </summary>
        private string sessionId;

        /// <summary>
        /// Initializes a new instance of the ThreadHelper class
        /// </summary>
        /// <param name="callbackDelegate">indicating the callback delegate</param>
        public ThreadHelper(Delegate callbackDelegate)
        {
            this.callbackDelegate = callbackDelegate;
        }

        /// <summary>
        /// Initializes a new instance of the ThreadHelper class
        /// </summary>
        /// <param name="callbackDelegate">indicating the callback delegate</param>
        /// <param name="sessionId">indicating the session id</param>
        public ThreadHelper(Delegate callbackDelegate, string sessionId)
            : this(callbackDelegate)
        {
            this.sessionId = sessionId;
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
                if (!sessionId.Equals("0"))
                {
                    TraceHelper.TraceEvent(this.sessionId, TraceEventType.Critical, "[ThreadHelper] Exception catched at root: {0}", e);
                }
                else
                {
                    TraceHelper.TraceEvent(TraceEventType.Critical, "[ThreadHelper] Exception catched at root: {0}", e);
                }
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
                if (!sessionId.Equals("0"))
                {
                    TraceHelper.TraceEvent(this.sessionId, TraceEventType.Critical, "[ThreadHelper] Exception catched at root: {0}", e);
                }
                else
                {
                    TraceHelper.TraceEvent(TraceEventType.Critical, "[ThreadHelper] Exception catched at root: {0}", e);
                }
            }
        }
    }
}
