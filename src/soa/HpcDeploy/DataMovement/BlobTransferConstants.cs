//------------------------------------------------------------------------------
// <copyright file="BlobTransferConstants.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Constants for use with the BlobTransfer class
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    /// <summary>
    /// Constants for use with the BlobTransfer class.
    /// </summary>
    public static class BlobTransferConstants
    {
        /// <summary>
        /// Stores the max block size, 4MB.
        /// </summary>
        public const int MaxBlockSize = 4 * 1024 * 1024;

        /// <summary>
        /// Default block size, 4MB.
        /// </summary>
        public const int DefaultBlockSize = 4 * 1024 * 1024;

        /// <summary>
        /// Default to root container name if none is specified.
        /// </summary>
        internal const string DefaultContainerName = "$root";

        /// <summary>
        /// Minimum block size, 256KB.
        /// </summary>
        internal const int MinBlockSize = 256 * 1024;

        /// <summary>
        /// Stores the max page blob file size, 1TB.
        /// </summary>
        internal const long MaxPageBlobFileSize = (long)1024 * 1024 * 1024 * 1024;

        /// <summary>
        /// Stores the max block blob file size, 50000 * 4M.
        /// </summary>
        internal const long MaxBlockBlobFileSize = (long)50000 * 4 * 1024 * 1024;

        /// <summary>
        /// Max upload window size.
        /// Upload window is used in uploading page blobs. 
        /// There can be multi threads to upload a page blob, 
        /// and we need to record upload window 
        /// and have constant length for a transfer entry record in restart journal,
        /// so set a limitation for upload window here.
        /// </summary>
        internal const int MaxUploadWindowSize = 128;

        /// <summary>
        /// Length to get page ranges in one request. 
        /// In blog http://blogs.msdn.com/b/windowsazurestorage/archive/2012/03/26/getting-the-page-ranges-of-a-large-page-blob-in-segments.aspx,
        /// it says that it's safe to get page ranges of 150M in one request.
        /// We use 148MB which is multiples of 4MB.
        /// </summary>
        internal const long PageRangesSpanSize = 148 * 1024 * 1024;

        /// <summary>
        /// If global memory status cannot be determined fallback to 1GB.
        /// </summary>
        internal const long DefaultMemoryCacheSize = (long)1 * 1024 * 1024 * 1024;

        /// <summary>
        /// Percentage of available we'll try to use for our memory cache.
        /// </summary>
        internal const double MemoryCacheMultiplier = 0.5;

        /// <summary>
        /// Maximum amount of memory to use for our memory cache.
        /// </summary>
        internal const long MemoryCacheMaximum = (long)2 * 1024 * 1024 * 1024;

        /// <summary>
        /// Maximum amount of cells in memory manager.
        /// </summary>
        internal const int MemoryManagerCellsMaximum = 8 * 1024;

        /// <summary>
        /// The life time in minutes of SAS auto generated for blob to blob copy.
        /// </summary>
        internal const int BlobCopySASLifeTimeInMinutes = 7 * 24 * 60;

        /// <summary>
        /// The time in milliseconds to wait to refresh copy status for blob to blob copy.
        /// </summary>
        internal const long BlobCopyStatusRefreshWaitTimeInMilliseconds = 100;
    }
}
