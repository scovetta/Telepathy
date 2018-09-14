//------------------------------------------------------------------------------
// <copyright file="TransferSpeedCalculator.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Helper class for BlobTransfer to calculate transfer speed.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Class used to calculate transfer speed.
    /// </summary>
    internal class TransferSpeedCalculator
    {
        /// <summary>
        /// Capacity of queue to calculate speed.
        /// </summary>
        private int queueSize;

        /// <summary>
        /// Stores the total bytes.
        /// </summary>
        private long totalBytes;

        /// <summary>
        /// Stores the update time as ticks.
        /// </summary>
        private Queue<long> timeQueue;

        /// <summary>
        /// Stores the bytes transferred.
        /// </summary>
        private Queue<long> bytesQueue;

        /// <summary>
        /// Stores the lock to calculate speed.
        /// </summary>
        private object lockCalculateSpeed = new object();
        
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="TransferSpeedCalculator" /> class.
        /// </summary>
        /// <param name="concurrency">How many work items to process 
        /// concurrently in BlobTransferManager.</param>
        public TransferSpeedCalculator(int concurrency)
        {
            this.queueSize = concurrency + 1;
            this.timeQueue = new Queue<long>(this.queueSize);
            this.bytesQueue = new Queue<long>(this.queueSize);

            this.CalculateSpeed(0L);
        }

        /// <summary>
        /// Gets the the total transferred bytes count.
        /// </summary>
        internal long TotalBytes
        {
            get
            {
                return Interlocked.Read(ref this.totalBytes);
            }
        }

        /// <summary>
        /// Add new transferred bytes count to current total value.
        /// </summary>
        /// <param name="bytesTransfered">Bytes count to be added.</param>
        /// <returns>The new totally bytes count.</returns>
        public long AddBytesTransferred(long bytesTransfered)
        {
            return Interlocked.Add(ref this.totalBytes, bytesTransfered);
        }

        /// <summary>
        /// Updates the current status by indicating the bytes transferred.
        /// </summary>
        /// <param name="total">Indicating by how much the bytes transferred.</param>
        /// <returns>Indicating previous recorded totally transferred bytes.</returns>
        public long UpdateStatus(long total)
        {
            return Interlocked.Exchange(ref this.totalBytes, total);
        }

        /// <summary>
        /// Append current transferred bytes into state queues and calculate speed.
        /// </summary>
        /// <param name="total">Indicating current transferred bytes.</param>
        /// <returns>Returns the speed.</returns>
        public double CalculateSpeed(long total)
        {
            lock (this.lockCalculateSpeed)
            {
                if (!this.bytesQueue.Any()
                    || (this.bytesQueue.Last() != total))
                {
                    if (this.timeQueue.Count == this.queueSize)
                    {
                        this.timeQueue.Dequeue();
                        this.bytesQueue.Dequeue();
                    }

                    long ticksNow = DateTime.Now.Ticks;
                    this.timeQueue.Enqueue(ticksNow);
                    this.bytesQueue.Enqueue(total);
                }

                double speed = 0.0;

                if (this.timeQueue.Count > 1)
                {
                    speed = (this.bytesQueue.Last() - this.bytesQueue.First())
                        / TimeSpan.FromTicks(this.timeQueue.Last() - this.timeQueue.First()).TotalSeconds;
                }
                
                return speed;
            }
        }
    }
}
