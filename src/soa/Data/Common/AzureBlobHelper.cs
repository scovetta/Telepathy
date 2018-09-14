//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureBlobHelper.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Helper class for uploading data to blob or downloading data from blob
// </summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Helper class for uploading data to blob or downloading data from blob
    /// </summary>
    internal class AzureBlobHelper
    {
        /// <summary>
        /// Exception handler
        /// </summary>
        /// <param name="ex">exception being caught</param>
        public delegate void ExceptionHandler(Exception ex);

        /// <summary>
        /// Mark the blob as beging uploaded periodically
        /// </summary>
        /// <param name="blob">target Azure blob</param>
        /// <param name="exceptionHandler">exception handler</param>
        /// <returns>A timer that marks the blob as beging upload periodically</returns>
        public static Timer MarkBlobAsBeingUploaded(CloudBlockBlob blob, ExceptionHandler exceptionHandler)
        {
            Timer updateMetadataTimer = new Timer(
                delegate(object state)
                {
                    try
                    {
                        blob.Metadata[Constant.MetadataKeyLastUpdateTime] = DateTime.UtcNow.ToString();
                        blob.SetMetadata();
                    }
                    catch (Exception ex)
                    {
                        // besides StorageException, there is race condition that the timer callback
                        // is triggered after "blob" object is disposed, thus ObjectDisposedException
                        // is thrown. May just ignore the exception.
                        if (exceptionHandler != null)
                        {
                            exceptionHandler(ex);
                        }
                    }
                },
                blob,
                new TimeSpan(Constant.LastUpdateTimeUpdateIntervalInMilliseconds),
                new TimeSpan(Constant.LastUpdateTimeUpdateIntervalInMilliseconds));

            return updateMetadataTimer;
        }

        /// <summary>
        /// Mark a blob as completed(upload done), either success or failure
        /// </summary>
        /// <param name="blob">target Azure blob</param>
        /// <param name="errorCode">error code of the upload result</param>
        /// <param name="errorMessage">error message of the upload result</param>
        public static void MarkBlobAsCompleted(CloudBlockBlob blob, string errorCode, string errorMessage)
        {
            blob.Metadata[Constant.MetadataKeyErrorCode] = errorCode;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                blob.Metadata[Constant.MetadataKeyException] = errorMessage;
            }

            blob.SetMetadata();
        }

        /// <summary>
        /// Mark a blob as synced(synced to file share done)
        /// </summary>
        /// <param name="blob">target Azure blob</param>
        public static void MarkBlobAsSynced(CloudBlockBlob blob)
        {
            blob.FetchAttributes();
            blob.Metadata[Constant.MetadataKeySynced] = Boolean.TrueString;
            blob.SetMetadata();
        }

        /// <summary>
        /// Check if a blob is makred as completed(upload done)
        /// </summary>
        /// <param name="blob">target Azure blob</param>
        /// <param name="errorCode">error code of the upload result</param>
        /// <param name="errorMessage">error message of the upload result</param>
        /// <returns>true if blob is marked as completed, false otherwise</returns>
        public static bool IsBlobMarkedAsCompleted(CloudBlockBlob blob, out string errorCode, out string errorMessage)
        {
            errorCode = null;
            errorMessage = null;

            blob.FetchAttributes();

            blob.Metadata.TryGetValue(Constant.MetadataKeyErrorCode, out errorCode);
            blob.Metadata.TryGetValue(Constant.MetadataKeyException, out errorMessage);

            return !string.IsNullOrEmpty(errorCode);
        }

        /// <summary>
        /// Mark a blob as completed(upload done), either success or failure
        /// </summary>
        /// <param name="blob">target Azure blob</param>
        /// <param name="metadata">the meta data</param>
        public static void SetBlobMetadata(CloudBlockBlob blob, IDictionary<string, string> metadata)
        {
            blob.FetchAttributes();
            foreach (var key in metadata.Keys)
            {
                if (!string.IsNullOrEmpty(metadata[key]))
                {
                    blob.Metadata[key] = metadata[key];
                }
            }

            blob.SetMetadata();
        }

    }
}