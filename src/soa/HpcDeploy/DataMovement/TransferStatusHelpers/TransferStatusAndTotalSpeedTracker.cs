//------------------------------------------------------------------------------
// <copyright file="TransferStatusAndTotalSpeedTracker.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Helper class for BlobTransfer to track progress and global speed.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers
{
    using System;

    /// <summary>
    /// Class used to track one transfer's speed and progress 
    /// and total transfer speed in the whole BlobTransferManager.
    /// </summary>
    internal class TransferStatusAndTotalSpeedTracker
    {
        /// <summary>
        /// Object to track transfer speed and progress.
        /// </summary>
        private TransferStatusTracker statusTracker;

        /// <summary>
        /// Object reference to BlobTransferManager total transfer speed tracker.
        /// </summary>
        private TransferSpeedTracker globalSpeedTracker;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="TransferStatusAndTotalSpeedTracker" /> class.
        /// </summary>
        /// <param name="totalSize">Total size of the object to be 
        /// transferred.</param>
        /// <param name="concurrency">How many work items to process 
        /// concurrently in BlobTransferManager.</param>
        /// <param name="progressCallback">Callback to be invoked upon 
        /// transfer progress.</param>
        /// <param name="globalSpeedTracker">Reference to BlobTransferManager 
        /// total transfer speed tracker.</param>
        public TransferStatusAndTotalSpeedTracker(
            long totalSize,
            int concurrency,
            Action<double, double> progressCallback,
            TransferSpeedTracker globalSpeedTracker)
        {
            this.statusTracker = new TransferStatusTracker(totalSize, concurrency, progressCallback);

            this.globalSpeedTracker = globalSpeedTracker;
        }

        /// <summary>
        /// Updates the current status by indicating the bytes transferred.
        /// </summary>
        /// <param name="total">Indicating by how much the bytes transferred.</param>
        public void UpdateStatus(long total)
        {
            long previousTotalBytes = 0;
            if (null != this.statusTracker)
            {
                previousTotalBytes = this.statusTracker.UpdateStatus(total);
            }

            if (null != this.globalSpeedTracker)
            {
                this.globalSpeedTracker.AddBytesTransferred(total - previousTotalBytes);
            }
        }

        /// <summary>
        /// Updates the current status by indicating the bytes transferred.
        /// </summary>
        /// <param name="bytesToIncrease">Indicating by how much the bytes 
        /// transferred increased.</param>
        public void AddBytesTransferred(long bytesToIncrease)
        {
            if (null != this.statusTracker)
            {
                this.statusTracker.AddBytesTransferred(bytesToIncrease);
            }

            if (null != this.globalSpeedTracker)
            {
                this.globalSpeedTracker.AddBytesTransferred(bytesToIncrease);
            }
        }        
    }
}
