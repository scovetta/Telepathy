//------------------------------------------------------------------------------
// <copyright file="SessionInfoContract.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The information about a cluster (in service fabric)
// </summary>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Hpc.Scheduler.Session
{
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
