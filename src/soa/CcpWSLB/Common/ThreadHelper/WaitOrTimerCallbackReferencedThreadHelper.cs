// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common.ThreadHelper
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal class WaitOrTimerCallbackReferencedThreadHelper : ReferencedThreadHelper<object>
    {
        private WaitOrTimerCallback callbackDelegate;

        public WaitOrTimerCallbackReferencedThreadHelper(WaitOrTimerCallback callback, ReferenceObject refObj) : base(refObj)
        {
            this.callbackDelegate = callback;
        }

        /// <summary>
        /// WaitOrTimerCallback root method
        /// </summary>
        /// <param name="state">indicate the state</param>
        /// <param name="timedOut">indicate the timedOut</param>
        public void CallbackRoot(object state, bool timedOut)
        {
            if (!this.refObj.IncreaseCount())
            {
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ReferencedThreadHelper] Ref object is 0, return immediately.");
                return;
            }

            try
            {
                this.callbackDelegate(state, timedOut);
            }
            catch (ThreadAbortException)
            {
            }
            catch (AppDomainUnloadedException)
            {
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[ReferencedThreadHelper] Exception catched at root: {0}", e);
            }
            finally
            {
                try
                {
                    this.refObj.DecreaseCount();
                }
                catch (ThreadAbortException)
                {
                }
                catch (AppDomainUnloadedException)
                {
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceEvent(TraceEventType.Critical, 0, "[ReferencedThreadHelper] Exception catched while disposing the object: {0}", e);
                }
            }
        }
    }
}
