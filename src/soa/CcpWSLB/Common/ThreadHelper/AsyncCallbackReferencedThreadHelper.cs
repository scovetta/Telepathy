// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.Common.ThreadHelper
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal class AsyncCallbackReferencedThreadHelper<T> : ReferencedThreadHelper<T>
    {
        private Func<T, Task> callbackAct;

        public AsyncCallbackReferencedThreadHelper(Func<T, Task> callbackDelegate, ReferenceObject refObj) : base(refObj)
        {
            this.callbackAct = callbackDelegate;
        }

        /// <summary>
        /// Callback root method
        /// </summary>
        /// <param name="obj">indicating the state</param>
        public async Task CallbackRoot(T obj)
        {
            // Try to increase count at the very beginning, if count is already 0, return immediately
            if (!this.refObj.IncreaseCount())
            {
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ReferencedThreadHelper] Ref object is 0, return immediately.");
                return;
            }

            try
            {
                await this.callbackAct(obj).ConfigureAwait(false);
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
                // Decrease the ref count in the finally block
                // The decrease method may call the dispose method and catch and log the execptions here
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
