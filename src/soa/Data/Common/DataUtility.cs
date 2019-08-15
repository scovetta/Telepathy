//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="DataUtility.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>It is a helper class.</summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
#if HPCPACK

namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Net;
    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob.Protocol;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;

    /// <summary>
    /// It is a helper class.
    /// </summary>
    internal static class DataUtility
    {
        /// <summary>
        /// Convert StorageException to DataException
        /// </summary>
        /// <param name="e">Storage exception</param>
        /// <returns>Data exception</returns>
        public static DataException ConvertToDataException(StorageException e)
        {
            return new DataException(ConvertToDataServiceErrorCode(e), e);
        }

        /// <summary>
        /// Convert StorageException to DataErrorCode
        /// </summary>
        /// <param name="e">Storage exception</param>
        /// <returns>Data error code</returns>
        public static int ConvertToDataServiceErrorCode(StorageException e)
        {
            if (e.RequestInformation == null)
            {
                return DataErrorCode.Unknown;
            }
            else if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Forbidden)
            {
                return DataErrorCode.DataNoPermission;
            }
            else if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.BadGateway ||
                     e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.BadRequest ||
                     e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.RequestedRangeNotSatisfiable)
            {
                return DataErrorCode.DataServerUnreachable;
            }
            else if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                return DataErrorCode.DataClientAlreadyExists;
            }
            else if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
            {
                return DataErrorCode.DataClientNotFound;
            }
            else
            {
                string errorCode = BurstUtility.GetStorageErrorCode(e);

                if (errorCode.Equals(StorageErrorCodeStrings.AuthenticationFailed, StringComparison.OrdinalIgnoreCase))
                {
                    return DataErrorCode.DataNoPermission;
                }
                else if (errorCode.Equals(BlobErrorCodeStrings.BlobAlreadyExists, StringComparison.OrdinalIgnoreCase))
                {
                    return DataErrorCode.DataClientAlreadyExists;
                }
                else if (errorCode.Equals(BlobErrorCodeStrings.BlobNotFound, StringComparison.OrdinalIgnoreCase) ||
                    errorCode.Equals(StorageErrorCodeStrings.ResourceNotFound, StringComparison.OrdinalIgnoreCase))
                {
                    return DataErrorCode.DataClientNotFound;
                }
                
                return DataErrorCode.Unknown;
            }
        }
    }
}
#endif