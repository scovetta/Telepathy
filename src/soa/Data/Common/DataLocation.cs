//------------------------------------------------------------------------------
// <copyright file="DataLocation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data location
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Enumeration of supported data locations
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com/session/data")]
    public enum DataLocation
    {
        /// <summary>
        /// File share
        /// </summary>
        [EnumMember]
        FileShare = 0,

        /// <summary>
        /// File share and Azure blob
        /// </summary>
        [EnumMember]
        FileShareAndAzureBlob = 1,

        /// <summary>
        /// Azure blob
        /// </summary>
        [EnumMember]
        AzureBlob = 2,

        /// <summary>
        /// Default location
        /// </summary>
        [EnumMember]
        Default = FileShare,
    }
}
