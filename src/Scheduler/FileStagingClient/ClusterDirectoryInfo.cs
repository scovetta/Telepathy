//--------------------------------------------------------------------------
// <copyright file="ClusterDirectoryInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This data contract replaces System.IO.DirectoryInfo so that we can 
//     better control our versioning, and so that we can provide the same 
//     data as the members of DirectoryInfo provide locally.
// </summary>
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Hpc.Azure.FileStaging.Client
{
    /// <summary>
    /// This class provides information on a directory stored on a node in a cluster
    /// </summary>
    [DataContract]
    public class ClusterDirectoryInfo : ClusterFileSystemInfo
    {
        private bool exists;
        private string name;

        public ClusterDirectoryInfo()
            : base()
        {
        }

        public ClusterDirectoryInfo(DirectoryInfo info) 
            : base(info)
        {
            this.exists = info.Exists;
            this.name = info.Name;
        }
        
        // Summary:
        //     Gets a value indicating whether the directory exists.
        //
        // Returns:
        //     true if the directory exists; otherwise, false.
        [DataMember]
        public override bool Exists
        {
            get { return this.exists; }
            set { this.exists = value; }
        }
        
        //
        // Summary:
        //     Gets the name of this System.IO.DirectoryInfo instance.
        //
        // Returns:
        //     The directory name.
        [DataMember]
        public override string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
}
