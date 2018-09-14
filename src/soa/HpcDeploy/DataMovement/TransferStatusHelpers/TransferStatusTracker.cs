//------------------------------------------------------------------------------
// <copyright file="TransferStatusTracker.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Helper class for BlobTransfer to track progress.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers
{
    using System;
    using System.Threading;

    /// <summary>
    /// Calculate and show transfer speed and progress.
    /// </summary>
    internal class TransferStatusTracker : TransferTracker
    {
        /// <summary>
        /// Stores the total size.
        /// </summary>
        private long totalSize;

        /// <summary>
        /// Callback to refresh status whenever there is news.
        /// </summary>
        private Action<double, double> progressCallback;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="TransferStatusTracker" /> class.
        /// </summary>
        /// <param name="totalSize">Total size of the object to be 
        /// transferred.</param>
        /// <param name="concurrency">How many work items to process 
        /// concurrently in BlobTransferManager.</param>
        /// <param name="progressCallback">Callback to be invoked upon 
        /// transfer progress.</param>
        public TransferStatusTracker(
            long totalSize, 
            int concurrency,
            Action<double, double> progressCallback)
            : base(concurrency)
        {
            this.totalSize = totalSize;
            this.progressCallback = progressCallback;
        }

        /// <summary>
        /// Gets the total size.
        /// </summary>
        /// <value>The total size of the file being tracked.</value>
        protected long TotalSize
        {
            get
            {
                return Interlocked.Read(ref this.totalSize);
            }
        }

        /// <summary>
        /// Trigger callback.
        /// </summary>
        /// <param name="bytesTransferred">Indicating by how much the bytes transferred.</param>
        protected override void TriggerCallback(long bytesTransferred)
        {
            if (null != this.progressCallback)
            {
                double speed = this.TransferSpeedCalculator.CalculateSpeed(bytesTransferred);
                double progress = 0 == this.totalSize ? 100 : ((double)bytesTransferred) / this.TotalSize * 100;

                this.progressCallback(speed, progress);
            }
        }
    }
}