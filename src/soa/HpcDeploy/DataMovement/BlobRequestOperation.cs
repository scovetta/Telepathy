//------------------------------------------------------------------------------
// <copyright file="BlobRequestOperation.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      BlobRequestOperation enumeration
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    /// <summary>
    /// BlobRequestOperation enumeration.
    /// </summary>
    public enum BlobRequestOperation
    {
        /// <summary>
        /// Create container operation.
        /// </summary>
        CreateContainer,

        /// <summary>
        /// List blobs operation.
        /// </summary>
        ListBlobs,

        /// <summary>
        /// CloudPageBlob.Create operation.
        /// </summary>
        CreatePageBlob,

        /// <summary>
        /// Delete operation.
        /// </summary>
        Delete,

        /// <summary>
        /// Get page ranges operation.
        /// </summary>
        GetPageRanges,

        /// <summary>
        /// Open read operation.
        /// </summary>
        OpenRead,

        /// <summary>
        /// Put block operation.
        /// </summary>
        PutBlock,

        /// <summary>
        /// Put block list operation.
        /// </summary>
        PutBlockList,

        /// <summary>
        /// Download block list operation.
        /// </summary>
        DownloadBlockList,

        /// <summary>
        /// Set metadata operation.
        /// </summary>
        SetMetadata,

        /// <summary>
        /// Fetch attributes operation.
        /// </summary>
        FetchAttributes,

        /// <summary>
        /// Write pages operation.
        /// </summary>
        WritePages,

        /// <summary>
        /// Clear pages operation.
        /// </summary>
        ClearPages,

        /// <summary>
        /// Get blob reference from server operation.
        /// </summary>
        GetBlobReferenceFromServer,

        /// <summary>
        /// Start copy from blob operation.
        /// </summary>
        StartCopyFromBlob,
    }
}
