// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common.ThreadHelper
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;

    internal class BrokerQueueCallbackReferencedThreadHelper : ReferencedThreadHelper<object>
    {
        private BrokerQueueCallback brokerQueueCallback;

        public BrokerQueueCallbackReferencedThreadHelper(BrokerQueueCallback callback, ReferenceObject refObj) : base(refObj)
        {
            this.brokerQueueCallback = callback;
        }

        /// <summary>
        /// BrokerQueueCallback root method
        /// </summary>
        /// <param name="item">indicating the broker queue item</param>
        /// <param name="state">indicating the state</param>
        public void CallbackRoot(BrokerQueueItem item, object state)
        {
            if (!this.refObj.IncreaseCount())
            {
                throw new ObjectDisposedException("dispatcher");
            }

            try
            {
                this.brokerQueueCallback(item, state);
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
