// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Enum for reason of EndOfResponses
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com/BrokerLauncher")]
    public enum EndOfResponsesReason
    {
        /// <summary>
        /// Reason is succeeded
        /// </summary>
        [EnumMember]
        Success = 0,

        /// <summary>
        /// Reason is client purged
        /// </summary>
        [EnumMember]
        ClientPurged,

        /// <summary>
        /// Reason is client timeout
        /// </summary>
        [EnumMember]
        ClientTimeout,
    }
}
