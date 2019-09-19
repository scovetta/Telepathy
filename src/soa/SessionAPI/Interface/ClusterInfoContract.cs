// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The structure contains the information about a cluster
    /// </summary>
    [Serializable]
    [DataContract(Name = "ClusterInfoContract", Namespace = "http://hpc.microsoft.com/SessionLauncher")]
    public class ClusterInfoContract
    {
        /// <summary>
        /// Gets or sets the cluster name
        /// </summary>
        [DataMember]
        public string ClusterName { get; set; }

        /// <summary>
        /// Gets or sets the cluster ID
        /// </summary>
        [DataMember]
        public string ClusterId { get; set; }

        /// <summary>
        /// Gets or sets the network topology
        /// </summary>
        [DataMember]
        public string NetworkTopology { get; set; }

        /// <summary>
        /// Gets or sets the Azure storage connection string
        /// </summary>
        [DataMember]
        public string AzureStorageConnectionString { get; set; }
    }
}
