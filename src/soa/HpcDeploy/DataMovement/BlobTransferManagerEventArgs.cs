//------------------------------------------------------------------------------
// <copyright file="BlobTransferManagerEventArgs.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      EventArgs class for BlobTransferManager events.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;

    /// <summary>
    /// EventArgs class for BlobTransferManager events.
    /// </summary>
    public class BlobTransferManagerEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="BlobTransferManagerEventArgs" /> class.
        /// </summary>
        /// <param name="globalSpeed">Indicating the global transfer speed in bytes/sec.</param>
        internal BlobTransferManagerEventArgs(
            double globalSpeed)
        {
            if (globalSpeed < 0)
            {
                throw new ArgumentOutOfRangeException("globalSpeed");
            }

            this.GlobalSpeed = globalSpeed;
        }        
                
        /// <summary>
        /// Gets the global transfer speed in bytes/sec at the time of the event.
        /// </summary>
        /// <value>Global transfer speed in bytes/sec at the time of the event.</value>
        public double GlobalSpeed
        {
            get;
            private set;
        }
    }
}
