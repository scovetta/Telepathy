//------------------------------------------------------------------------------
// <copyright file="BlobTransferErrorCode.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Error codes for BlobTransferException.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.DataMovement.Exceptions
{
    /// <summary>
    /// Error codes for BlobTransferException.
    /// </summary>
    public enum BlobTransferErrorCode
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// Invalid source location specified.
        /// </summary>
        InvalidSourceLocation = 1,

        /// <summary>
        /// Invalid destination location specified.
        /// </summary>
        InvalidDestinationLocation = 2,

        /// <summary>
        /// Failed to open file for upload or download.
        /// </summary>
        OpenFileFailed = 3,

        /// <summary>
        /// The file to transfer is too large for the specified blob type.
        /// </summary>
        UploadBlobSourceFileSizeTooLarge = 4,

        /// <summary>
        /// The file size is invalid for the specified blob type.
        /// </summary>
        UploadBlobSourceFileSizeInvalid = 5,

        /// <summary>
        /// User canceled.
        /// </summary>
        OperationCanceled = 6,

        /// <summary>
        /// Both Source and Destination are locally accessible locations.
        /// At least one of source and destination should be an Azure Storage location.
        /// </summary>
        LocalToLocalTransfersUnsupported = 7,

        /// <summary>
        /// Failed to do copying from blob to blob.
        /// </summary>
        CopyFromBlobToBlobFailed = 8,

        /// <summary>
        /// Source and destination are the same.
        /// </summary>
        SameSourceAndDestination = 9,

        /// <summary>
        /// BlobCopyMonitor detects mismatch between copy id stored in transfer entry and 
        /// that retrieved from server.
        /// </summary>
        MismatchCopyId = 10,

        /// <summary>
        /// BlobCopyMonitor fails to retrieve CopyState for the blob which we are to monitor.
        /// </summary>
        FailToRetrieveCopyStateForBlobToMonitor = 11,

        /// <summary>
        /// Fails to allocate memory in MemoryManager.
        /// </summary>
        FailToAllocateMemory = 12,

        /// <summary>
        /// Fails to get source's last write time.
        /// </summary>
        FailToGetSourceLastWriteTime = 13,
    }
}
