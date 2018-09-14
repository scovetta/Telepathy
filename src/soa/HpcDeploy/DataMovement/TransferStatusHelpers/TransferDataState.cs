//------------------------------------------------------------------------------
// <copyright file="TransferDataState.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Helper class for BlobTransfer to record state.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers
{
    using System;

    /// <summary>
    /// Calculate and show transfer speed.
    /// </summary>
    internal abstract class TransferDataState : IDisposable
    {
        /// <summary>
        /// Gets or sets a handle to the memory buffer to ensure the
        /// memory buffer remains in memory during the entire operation.
        /// </summary>
        public byte[] MemoryBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the sequence number of the data being processed.
        /// </summary>
        public int SequenceNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the starting offset of this part of data.
        /// </summary>
        public long StartOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the length of this part of data.
        /// </summary>
        public int Length
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets how many bytes have been read.
        /// </summary>
        public int BytesRead
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets callbackState for finishing work.
        /// </summary>
        public CallbackState CallbackState
        {
            get;
            set;
        }

        /// <summary>
        /// Public dispose method to release all resources owned.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Private dispose method to release managed/unmanaged objects.
        /// If disposing = true clean up managed resources as well as unmanaged resources.
        /// If disposing = false only clean up unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether or not to dispose managed resources.</param>
        protected abstract void Dispose(bool disposing);
    }
}