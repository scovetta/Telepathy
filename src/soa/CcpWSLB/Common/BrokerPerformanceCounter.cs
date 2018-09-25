//------------------------------------------------------------------------------
// <copyright file="BrokerPerformanceCounter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Broker's performance counter
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;

    /// <summary>
    /// Broker's performance counter
    /// </summary>
    internal class BrokerPerformanceCounter
    {
        /// <summary>
        /// Stores the lock
        /// </summary>
        private static object lockThis = new object();

        /// <summary>
        /// Stores the singleton instance
        /// </summary>
        private static BrokerPerformanceCounter instance;

        /// <summary>
        /// Stores the counters
        /// </summary>
        private PerformanceCounter[] counters;

        /// <summary>
        /// Prevents a default instance of the BrokerPerformanceCounter class from being created
        /// </summary>
        private BrokerPerformanceCounter()
        {
            this.counters = new PerformanceCounter[5];
            this.counters[(int)BrokerPerformanceCounterKey.RequestMessages] = BrokerPerformanceCounterHelper.GetPerfCounter(BrokerPerformanceCounterKey.RequestMessages);
            this.counters[(int)BrokerPerformanceCounterKey.ResponseMessages] = BrokerPerformanceCounterHelper.GetPerfCounter(BrokerPerformanceCounterKey.ResponseMessages);
            this.counters[(int)BrokerPerformanceCounterKey.Calculations] = BrokerPerformanceCounterHelper.GetPerfCounter(BrokerPerformanceCounterKey.Calculations);
            this.counters[(int)BrokerPerformanceCounterKey.Faults] = BrokerPerformanceCounterHelper.GetPerfCounter(BrokerPerformanceCounterKey.Faults);
        }

        /// <summary>
        /// Increase the perf counter
        /// </summary>
        /// <param name="key">indicate the counter key</param>
        /// <param name="delta">indicate the delta</param>
        public static void IncrementPerfCounterBy(BrokerPerformanceCounterKey key, long delta)
        {
            try
            {
                if (delta != 0)
                {
                    GetInstance().counters[(int)key].IncrementBy(delta);
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "Exception throwed while updating perf counter: {0}", e);
            }
        }

        /// <summary>
        /// Gets a singleton instance
        /// </summary>
        /// <returns>singleton instance</returns>
        private static BrokerPerformanceCounter GetInstance()
        {
            lock (lockThis)
            {
                if (instance == null)
                {
                    try
                    {
                        instance = new BrokerPerformanceCounter();
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "Exception throwed while initializing broker performance counter: {0}", e);
                        throw;
                    }
                }

                return instance;
            }
        }
    }
}
