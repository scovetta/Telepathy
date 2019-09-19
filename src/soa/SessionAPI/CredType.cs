// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    /// <summary>
    /// The type of the credential.
    /// </summary>
    public enum CredType
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
