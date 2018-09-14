//--------------------------------------------------------------------------
// <copyright file="ClusterFileInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This data contract replaces System.IO.FileInfo so that we can better
//     control our versioning, and so that we can provide the same data
//     as the members of FileInfo provide locally.
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
    /// This class provides information on a file stored on a node in a cluster
    /// </summary>
    [DataContract]
    public class ClusterFileInfo : ClusterFileSystemInfo
    {
        private string directoryName;
        private bool exists;
        private bool isReadOnly;
        private long length;
        private string name;

        public ClusterFileInfo()
            : base()
        {
        }

        public ClusterFileInfo(FileInfo info) 
            : base(info)
        {
            this.directoryName = info.DirectoryName;
            this.exists = info.Exists;
            this.isReadOnly = info.IsReadOnly;
            this.length = info.Length;
            this.name = info.Name;
        }

        //
        // Summary:
        //     Gets a string representing the directory's full path.
        //
        // Returns:
        //     A string representing the directory's full path.
        //
        // Exceptions:
        //   System.Security.SecurityException:
        //     The caller does not have the required permission.
        //
        //   System.ArgumentNullException:
        //     null was passed in for the directory name.
        [DataMember]
        public string DirectoryName
        {
            get { return this.directoryName; }
            set { this.directoryName = value; }
        }
        
        //
        // Summary:
        //     Gets a value indicating whether a file exists.
        //
        // Returns:
        //     true if the file exists; false if the file does not exist or if the file
        //     is a directory.
        [DataMember]
        public override bool Exists
        {
            get { return this.exists; }
            set { this.exists = value; }
        }
        
        //
        // Summary:
        //     Gets or sets a value that determines if the current file is read only.
        //
        // Returns:
        //     true if the current file is read only; otherwise, false.
        //
        // Exceptions:
        //   System.IO.FileNotFoundException:
        //     The file described by the current System.IO.FileInfo object could not be
        //     found.
        //
        //   System.IO.IOException:
        //     An I/O error occurred while opening the file.
        //
        //   System.UnauthorizedAccessException:
        //     The file described by the current System.IO.FileInfo object is read-only.
        //      -or- This operation is not supported on the current platform.  -or- The
        //     caller does not have the required permission.
        [DataMember]
        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
            set { this.isReadOnly = value; }
        }
        
        //
        // Summary:
        //     Gets the size, in bytes, of the current file.
        //
        // Returns:
        //     The size of the current file in bytes.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot update the state of the file or
        //     directory.
        //
        //   System.IO.FileNotFoundException:
        //     The file does not exist.  -or- The Length property is called for a directory.
        [DataMember]
        public long Length
        {
            get { return this.length; }
            set { this.length = value; }
        }
        
        //
        // Summary:
        //     Gets the name of the file.
        //
        // Returns:
        //     The name of the file.
        [DataMember]
        public override string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
    }
}
