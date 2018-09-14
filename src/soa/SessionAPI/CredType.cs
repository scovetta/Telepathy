//------------------------------------------------------------------------------
// <copyright file="CredType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The type of the credential.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    /// <summary>
    /// The type of the credential.
    /// </summary>
    enum CredType
    {
        None = 0,

        /// <summary>
        /// either password or certificate.
        /// HpcSoftCard: Allowed
        /// DisableCredentialReuse: False
        /// </summary>
        Either = 1,

        /// <summary>
        /// either password or certificate.
        /// HpcSoftCard: Allowed
        /// DisableCredentialReuse: True
        /// </summary>
        Either_CredUnreusable = 2,

        /// <summary>
        /// need password for credential.
        /// HpcSoftCard: Disabled
        /// DisableCredentialReuse: False
        /// </summary>
        Password = 3,

        /// <summary>
        /// need password for credential.
        /// HpcSoftCard: Disabled
        /// DisableCredentialReuse: True
        /// </summary>
        Password_CredUnreusable = 4,

        /// <summary>
        /// need cert for credential.
        /// HpcSoftCard: Required
        /// DisableCredentialReuse NA
        /// </summary>
        Certificate = 5,
    }
}
