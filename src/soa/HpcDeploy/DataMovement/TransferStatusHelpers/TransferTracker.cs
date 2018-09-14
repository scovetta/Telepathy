//------------------------------------------------------------------------------
// <copyright file="TransferTracker.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Base class to define common transfer tracker functionalities.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers
{
    /// <summary>
    /// Base class to define common transfer tracker functionalities.
    /// </summary>
    internal abstract class TransferTracker
    {
        /// <summary>
        /// Object to calculate transfer speed.
        /// </summary>
        private TransferSpeedCalculator transferSpeedCalculator;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="TransferTracker" /> class.
        /// </summary>
        /// <param name="concurrency">How many work items to process 
        /// concurrently in BlobTransferManager.</param>
        protected TransferTracker(int concurrency)
        {
            this.transferSpeedCalculator = new TransferSpeedCalculator(concurrency);
        }

        /// <summary>
        /// Gets object to calculate transfer speed.
        /// </summary>
        protected TransferSpeedCalculator TransferSpeedCalculator
        {
            get
            {
                return this.transferSpeedCalculator;
            }
        }
        
        /// <summary>
        /// Updates the current status by indicating the bytes transferred.
        /// </summary>
        /// <param name="total">Indicating by how much the bytes transferred.</param>
        /// <returns>Indicating previous recorded totally transferred bytes.</returns>
        public long UpdateStatus(long total)
        {
            long previousTotalBytes = this.transferSpeedCalculator.UpdateStatus(total);

            this.TriggerCallback(total);

            return previousTotalBytes;
        }

        /// <summary>
        /// Updates the current status by indicating the bytes transferred.
        /// </summary>
        /// <param name="bytesToIncrease">Indicating by how much the bytes 
        /// transferred increased.</param>
        public void AddBytesTransferred(long bytesToIncrease)
        {
            long total = this.transferSpeedCalculator.AddBytesTransferred(bytesToIncrease);

            this.TriggerCallback(total);
        }
        
        /// <summary>
        /// Abstract function to declare method to trigger callback.
        /// </summary>
        /// <param name="bytesTransferred">Indicating by how much the bytes transferred.</param>
        protected abstract void TriggerCallback(long bytesTransferred);
    }
}
