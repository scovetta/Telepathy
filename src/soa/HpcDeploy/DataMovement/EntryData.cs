//------------------------------------------------------------------------------
// <copyright file="EntryData.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Entry data for recursive transfer callbacks.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Entry data for recursive transfer callbacks.
    /// </summary>
    public class EntryData
    {
        /// <summary>
        /// Gets source blob. Always null if source location is not an Azure Storage location.
        /// </summary>
        /// <value>Source blob. Null if source location is not an Azure Storage location.</value>
        public ICloudBlob SourceBlob
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets destination blob. Always null if destination location is not an Azure Storage location.
        /// </summary>
        /// <value>Destination blob. Null if destination location is not an Azure Storage location.</value>
        public ICloudBlob DestinationBlob
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets filename corresponding to this transfer entry.
        /// </summary>
        /// <value>File name of transfer entry.</value>
        public string FileName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets transfer entry <see cref="BlobTransferFileTransferEntry"/> information.
        /// </summary>
        /// <value>Object of transfer entry <see cref="BlobTransferFileTransferEntry"/>.</value>
        public BlobTransferFileTransferEntry TransferEntry
        {
            get;
            internal set;
        }
    }
}
