//------------------------------------------------------------------------------
// <copyright file="DownloadDataState.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Keep the state of downloading a single block from web.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers
{
    using System.IO;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Keep the state of downloading a single block from web.
    /// </summary>
    internal class DownloadDataState : TransferDataState
    {
        /// <summary>
        /// Gets or sets the memory stream used to encapsulate the memory
        /// buffer for passing the methods such as PutBlock, WritePages, 
        /// DownloadToStream and DownloadRangeToStream, as these methods
        /// requires a stream and doesn't allow for a byte array as input.
        /// </summary>
        public MemoryStream MemoryStream
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the OperationContext for DataMovement needs to get 
        /// changed OperationContext after some async calls 
        /// like BeginDownloadRangeToStream.
        /// </summary>
        public OperationContext OperationContext
        {
            get;
            set;
        }

        /// <summary>
        /// Private dispose method to release managed/unmanaged objects.
        /// If disposing = true clean up managed resources as well as unmanaged resources.
        /// If disposing = false only clean up unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether or not to dispose managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != this.MemoryStream)
                {
                    this.MemoryStream.Dispose();
                    this.MemoryStream = null;
                }
            }
        }
    }
}
