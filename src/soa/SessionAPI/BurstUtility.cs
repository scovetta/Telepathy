//-----------------------------------------------------------------------
// <copyright file="BurstUtility.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     It is an utility class for burst.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.BrokerBurst
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;

    /// <summary>
    /// It is an utility class for burst.
    /// </summary>
    public static class BurstUtility
    {
        /// <summary>
        /// Check if the queue is not found.
        /// </summary>
        /// <param name="e">exception occurred when access queue</param>
        /// <returns>
        /// return true if queue is not found
        /// </returns>
        public static bool IsQueueNotFound(StorageException e)
        {
            string errorCode = GetStorageErrorCode(e);

            return errorCode == QueueErrorCodeStrings.QueueNotFound;
        }


        /// <summary>
        /// Get error code from StorageException.
        /// </summary>
        /// <remarks>
        /// Error code is a string in Azure storage SDK 2.0.
        /// </remarks>
        /// <param name="e">Storage exception</param>
        /// <returns>Error code</returns>
        public static string GetStorageErrorCode(StorageException e)
        {
            string errorCode = string.Empty;

            if (e.RequestInformation != null &&
                e.RequestInformation.ExtendedErrorInformation != null)
            {
                errorCode = e.RequestInformation.ExtendedErrorInformation.ErrorCode;
            }

            return errorCode;
        }
    }
}
