//------------------------------------------------------------------------------
// <copyright file="BlobExtensions.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Extensions methods for CloudBlobs for use with BlobTransfer
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.Extensions
{
    using System;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Extension methods for CloudBlobs for use with BlobTransfer.
    /// </summary>
    internal static class BlobExtensions
    {
        /// <summary>
        /// Determines whether two blobs have the same Uri and SnapshotTime.
        /// </summary>
        /// <param name="blob">Blob to compare.</param>
        /// <param name="comparand">Comparand object.</param>
        /// <returns>True if the two blobs have the same Uri and SnapshotTime; otherwise, false.</returns>
        internal static bool Equals(
            ICloudBlob blob,
            ICloudBlob comparand)
        {
            if (blob == comparand)
            {
                return true;
            }

            if (null == blob || null == comparand)
            {
                return false;
            }

            return blob.Uri.Equals(comparand.Uri) &&
                blob.SnapshotTime.Equals(comparand.SnapshotTime);
        }

        /// <summary>
        /// Append an auto generated SAS to a blob uri.
        /// </summary>
        /// <param name="blob">Blob to append SAS.</param>
        /// <returns>Blob Uri with SAS appended.</returns>
        internal static ICloudBlob AppendSAS(
            this ICloudBlob blob)
        {
            if (null == blob)
            {
                throw new ArgumentNullException("blob");
            }

            if (blob.ServiceClient.Credentials.IsSAS)
            {
                return blob;
            }

            // SAS life time is at least 10 minutes.
            TimeSpan sasLifeTime = TimeSpan.FromMinutes(
                Math.Max(10, BlobTransferConstants.BlobCopySASLifeTimeInMinutes));

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.Now.Add(sasLifeTime),
                Permissions = SharedAccessBlobPermissions.Read,
            };

            // Blob property may not be retrieved yet.
            if (BlobType.PageBlob == blob.BlobType)
            {
                CloudPageBlob rootPageBlob = new CloudPageBlob(
                    blob.Uri,
                    blob.ServiceClient.Credentials);

                string sasToken = rootPageBlob.GetSharedAccessSignature(policy);

                return new CloudPageBlob(
                    blob.Uri,
                    blob.SnapshotTime,
                    new StorageCredentials(sasToken));
            }
            else if (BlobType.BlockBlob == blob.BlobType)
            {
                CloudBlockBlob rootBlockBlob = new CloudBlockBlob(
                    blob.Uri,
                    blob.ServiceClient.Credentials);

                string sasToken = rootBlockBlob.GetSharedAccessSignature(policy);

                return new CloudBlockBlob(
                    blob.Uri,
                    blob.SnapshotTime,
                    new StorageCredentials(sasToken));
            }
            else
            {
                throw new InvalidOperationException(
                    Resources.OnlySupportTwoBlobTypesException);
            }
        }
    }
}