//------------------------------------------------------------------------------
// <copyright file="FileStagingErrorCode.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      This class defines file staging error code
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.FileStaging
{
    /// <summary>
    /// File staging error code definition
    /// </summary>
    public enum FileStagingErrorCode
    {
        /// <summary>
        /// Unknown error
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// Authentication failed
        /// </summary>
        AuthenticationFailed = 1,

        /// <summary>
        /// Caller is not authorizied
        /// </summary>
        NotAuthorized = 2,

        /// <summary>
        /// Endpoint is not found
        /// </summary>
        EndpointNotFound = 3,

        /// <summary>
        /// Request is timed out
        /// </summary>
        RequestTimedOut = 4,

        /// <summary>
        /// Communication error happens
        /// </summary>
        CommunicationFailure = 5,

        /// <summary>
        /// IO error happens
        /// </summary>
        TargetIOFailure = 6,

        /// <summary>
        /// Unknown InternalFaultDetail
        /// </summary>
        UnknownFault = 7,

        /// <summary>
        /// Intermediate blob storage account is not configured, or not configured properly
        /// </summary>
        IntermediateBlobStorageMisConfigured = 8,

        /// <summary>
        /// Target file or blob already exists
        /// </summary>
        TargetExists = 9,

        /// <summary>
        /// Internal server error happens
        /// </summary>
        InternalServerError = 10,
    }
}
