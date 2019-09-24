// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Persistences.MSMQPersist
{
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Implementation of MSMQ concurrency controller.  
    /// MSMQConcurrencyController defines concurrency controlled section.  A concurrency controlled section
    /// begins from MSMQConcurrencyController.Enter, and end at MSMQConcurrencyController.Exit.
    /// MSMQConcurrencyController allows at most MSMQConcurrencyController.MaxConcurrency calls into 
    /// concurrency controlled section at any time.
    /// </summary>
    static class MSMQConcurrencyController
    {
#region private fields
        /// <summary> max concurrency </summary>
        private const int maxConcurrency = 1;

        /// <summary> current concurrency </summary> 
        private static int concurrency = 0;

        /// <summary> callbacks waiting to be executed </summary>
        private static Queue<object> WaitingCallbacks = new Queue<object>();

        /// <summary> synchronization object </summary>
        private static object SyncObject = new object();
#endregion

        /// <summary>
        /// return current concurrency
        /// </summary>
        public static int Concurrency
        {
            get
            {
                return concurrency;
            }
        }

        /// <summary>
        /// return max concurrency
        /// </summary>
        public static int MaxConcurrency
        {
            get
            {
                return maxConcurrency;
            }
        }

        /// <summary>
        /// Try to enter concurrency controlled section.  If MaxConcurrency is reached, put the callback into a wait queue, which will be scheduled when Concurrency goes below MaxConcurrency.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static void Enter(WaitCallback callback, object state)
        {            
            bool enterSuccess = false;
            lock (SyncObject)
            {
                if (concurrency < maxConcurrency && WaitingCallbacks.Count == 0)
                {
                    BrokerTracing.TraceInfo("[MSMQConcurrencyController] enter concurrency controlled section");
                    concurrency++;
                    enterSuccess = true;
                }
                else
                {
                    WaitingCallbacks.Enqueue(new object[] { callback, state });
                    BrokerTracing.TraceInfo("[MSMQConcurrencyController] wait on concurrency controlled section");
                }
            }

            if (enterSuccess)
            {
                ThreadPool.QueueUserWorkItem(callback, state);
            }
        }

        /// <summary>
        /// Exit concurrency controlled section.
        /// </summary>
        public static void Exit()
        {
            BrokerTracing.TraceInfo("[MSMQConcurrencyController] exit conccurrency controlled section");
            object nextCallbackItem = null;
            lock (SyncObject)
            {
                if (concurrency <= 0)
                {
                    System.Diagnostics.Debug.Assert(false, "Concurrency is not positive");
                }                

                if (WaitingCallbacks.Count == 0)
                {
                    concurrency--;
                }
                else
                {
                    nextCallbackItem = WaitingCallbacks.Dequeue();
                }
            }

            if (nextCallbackItem != null)
            {
                BrokerTracing.TraceInfo("[MSMQConcurrencyController] schedule next waiting callback");
                object[] objs = (object[])nextCallbackItem;
                WaitCallback callback = (WaitCallback)objs[0];
                object callbackState = objs[1];

                ThreadPool.QueueUserWorkItem(callback, callbackState);
            }
        }
    }
}
