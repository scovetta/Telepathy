//------------------------------------------------------------------------------
// <copyright file="BlobTransferEntryStatus.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Status for BlobTransferFileTransferEntry.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    /// <summary>
    /// Status for BlobTransferFileTransferEntry.
    /// NotStarted -> Transfer -> [Monitor ->] [RemoveSource ->] Finished.
    /// </summary>
    public enum BlobTransferEntryStatus
    {
        /// <summary>
        /// Transfer is not started.
        /// </summary>
        NotStarted,

        /// <summary>
        /// Transfer file.
        /// </summary>
        Transfer,

        /// <summary>
        /// Monitor transfer process.
        /// </summary>
        Monitor,

        /// <summary>
        /// Remove source file.
        /// </summary>
        RemoveSource,

        /// <summary>
        /// Transfer is finished.
        /// </summary>
        Finished,
    }
}