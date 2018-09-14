//--------------------------------------------------------------------------
// <copyright file="FileStagingClientUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     FileStaging client utility
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging.Client
{
    using System;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;

    /// <summary>
    /// File Staging Client Utility
    /// </summary>
    public static class FileStagingClientUtility
    {
        /// <summary>
        /// Create FileStagingException from StorageException
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static FileStagingException GenerateFileStagingException(StorageException e)
        {
            string errorCode = string.Empty;
            if (e.RequestInformation.ExtendedErrorInformation != null)
            {
                errorCode = e.RequestInformation.ExtendedErrorInformation.ErrorCode;
            }

            if (errorCode.Equals(StorageErrorCodeStrings.AuthenticationFailed, StringComparison.OrdinalIgnoreCase))
            {
                return new FileStagingException(e, FileStagingErrorCode.AuthenticationFailed);
            }
            else
            {
                return new FileStagingException(e, FileStagingErrorCode.TargetIOFailure);
            }
        }
    }
}
