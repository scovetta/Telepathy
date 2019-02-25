//------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Connection info used to connect to a channel
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    /// <summary>
    /// Connection info used to connect to a channel
    /// </summary>
    public interface IConnectionInfo
    {
        /// <summary>
        /// The <see cref="TransportScheme"/>
        /// </summary>
        TransportScheme TransportScheme { get; set; }

        /// <summary>
        /// If the use is an AAD user
        /// </summary>
        bool UseAad { get; set; }

        /// <summary>
        /// If the user is a local user
        /// </summary>
        bool LocalUser { get; }

        /// <summary>
        /// This should typically be UseAad || LocalUser
        /// </summary>
        bool IsAadOrLocalUser { get; }

        /// <summary>
        /// Gets the headnode name
        /// </summary>
        string Headnode { get; }

        /// <summary>
        /// Storage connection string used in Azure Storage Transport Scheme
        /// </summary>
        string AzureStorageConnectionString { get; }

        /// <summary>
        /// Partition key when using Azure Table Storage for transporting
        /// </summary>
        string AzureTableStoragePartitionKey { get; }
    }
}
