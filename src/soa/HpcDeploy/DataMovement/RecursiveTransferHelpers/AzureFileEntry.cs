//------------------------------------------------------------------------------
// <copyright file="AzureFileEntry.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      AzureFileEntry class to represent a single transfer file entry on azure.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// AzureFileEntry class to represent a single transfer file entry on azure.
    /// </summary>
    internal class AzureFileEntry : FileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFileEntry" /> class.
        /// </summary>
        /// <param name="relativePath">Relative path of the file indicated by this file entry.</param>
        /// <param name="cloudBlob">Corresponding ICloudBlob.</param>
        public AzureFileEntry(string relativePath, ICloudBlob cloudBlob)
            : base(relativePath, cloudBlob.Properties.LastModified, cloudBlob.SnapshotTime)
        {
            this.Blob = cloudBlob;
        }

        /// <summary>
        /// Gets the reference to the blob.
        /// </summary>
        public ICloudBlob Blob
        {
            get;
            private set;
        }
    }
}