//------------------------------------------------------------------------------
// <copyright file="ObserverItem.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Represents a item for observer
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;

    /// <summary>
    /// Represents a item for observer
    /// </summary>
    internal class ObserverItem
    {
        /// <summary>
        /// Stores the total value
        /// </summary>
        private long total;

        /// <summary>
        /// Stores the count
        /// </summary>
        private long count;

        /// <summary>
        /// Stores the pre total value
        /// </summary>
        private long preTotal;

        /// <summary>
        /// Stores the performance counter key
        /// </summary>
        private BrokerPerformanceCounterKey key;

        /// <summary>
        /// Initializes a new instance of the ObserverItem class
        /// </summary>
        /// <param name="key">indicate the performance counter key</param>
        public ObserverItem(BrokerPerformanceCounterKey key)
        {
            this.key = key;
        }

        /// <summary>
        /// Gets the total value
        /// </summary>
        public long Total
        {
            get { return Interlocked.Read(ref this.total); }
        }

        /// <summary>
        /// Gets the average value
        /// </summary>
        public long Average
        {
            get { return Interlocked.Read(ref this.count) == 0 ? 0 : Interlocked.Read(ref this.total) / Interlocked.Read(ref this.count); }
        }

        /// <summary>
        /// Gets the subtract value of total and preTotal
        /// </summary>
        public long Changed
        {
            get { return Interlocked.Read(ref this.total) - Interlocked.Read(ref this.preTotal); }
        }

        /// <summary>
        /// Increment the value
        /// </summary>
        /// <param name="value">indicate the value</param>
        public void Increment(long value)
        {
            Interlocked.Add(ref this.total, value);
        }

        /// <summary>
        /// Add new value
        /// </summary>
        /// <param name="value">indicate the value</param>
        public void AddValue(long value)
        {
            Interlocked.Add(ref this.total, value);
            Interlocked.Increment(ref this.count);
        }

        /// <summary>
        /// Update the performance counter
        /// </summary>
        public void UpdatePerformanceCounter()
        {
            BrokerPerformanceCounter.IncrementPerfCounterBy(this.key, Interlocked.Read(ref this.total) - Interlocked.Read(ref this.preTotal));
            this.preTotal = Interlocked.Read(ref this.total);
        }
    }
}
