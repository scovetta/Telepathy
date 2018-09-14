//------------------------------------------------------------------------------
// <copyright file="TransferSpeedTracker.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Helper class for BlobTransfer to track transfer speed.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers
{
    using System;

    /// <summary>
    /// Class to track transfer speed.
    /// </summary>
    internal class TransferSpeedTracker : TransferTracker
    {
        /// <summary>
        /// Represents a call back which is called when transfer 
        /// gets any progress to notify about the transfer speed.
        /// </summary>
        private Action<double> speedCallback;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="TransferSpeedTracker" /> class.
        /// </summary>
        /// <param name="speedCallback">Call back which is called when transfer 
        /// gets any progress to notify about the transfer speed.</param>
        /// <param name="concurrency">How many work items to process 
        /// concurrently in BlobTransferManager.</param>
        public TransferSpeedTracker(Action<double> speedCallback, int concurrency)
            : base(concurrency)
        {
            this.speedCallback = speedCallback;
        }

        /// <summary>
        /// Trigger callback.
        /// </summary>
        /// <param name="bytesTransferred">Indicating by how much the bytes transferred.</param>
        protected override void TriggerCallback(long bytesTransferred)
        {
            if (null != this.speedCallback)
            {
                double speed = this.TransferSpeedCalculator.CalculateSpeed(bytesTransferred);
                this.speedCallback(speed);
            }
        }
    }
}
