//--------------------------------------------------------------------------
// <copyright file="ClusterFileSystemInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This data contract replaces System.IO.FileSystemInfo so that we can 
//     better control our versioning, and so that we can provide the same
//     data as the members of FileSystemInfo provide locally.
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
    /// This class provides information on a file system entry stored on a node in a cluster
    /// </summary>
    [DataContract]
    public abstract class ClusterFileSystemInfo
    {
        private FileAttributes attributes;
        private DateTime creationTime;
        private DateTime creationTimeUtc;
        private string extension;
        private string fullName;
        private DateTime lastAccessTime;
        private DateTime lastAccessTimeUtc;
        private DateTime lastWriteTime;
        private DateTime lastWriteTimeUtc;

        public ClusterFileSystemInfo()
            : base()
        {
        }

        public ClusterFileSystemInfo(FileSystemInfo info)
        {
            this.attributes = info.Attributes;
            this.creationTime = info.CreationTime;
            this.creationTimeUtc = info.CreationTimeUtc;
            this.extension = info.Extension;
            this.fullName = info.FullName;
            this.lastAccessTime = info.LastAccessTime;
            this.lastAccessTimeUtc = info.LastAccessTimeUtc;
            this.lastWriteTime = info.LastWriteTime;
            this.lastWriteTimeUtc = info.LastWriteTimeUtc;
        }

        // Summary:
        //     Gets or sets the System.IO.FileAttributes of the current System.IO.FileSystemInfo.
        //
        // Returns:
        //     System.IO.FileAttributes of the current System.IO.FileSystemInfo.
        //
        // Exceptions:
        //   System.IO.FileNotFoundException:
        //     The specified file does not exist.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid, such as being on an unmapped drive.
        //
        //   System.Security.SecurityException:
        //     The caller does not have the required permission.
        //
        //   System.ArgumentException:
        //     The caller attempts to set an invalid file attribute.
        //
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        [DataMember]
        public FileAttributes Attributes
        {
            get { return this.attributes; }
            set { this.attributes = value; }
        }
        
        //
        // Summary:
        //     Gets or sets the creation time of the current System.IO.FileSystemInfo object.
        //
        // Returns:
        //     The creation date and time of the current System.IO.FileSystemInfo object.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid, such as being on an unmapped drive.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Microsoft Windows NT or later.
        [DataMember]
        public DateTime CreationTime
        {
            get { return this.creationTime; }
            set { this.creationTime = value; }
        }
        
        //
        // Summary:
        //     Gets or sets the creation time, in coordinated universal time (UTC), of the
        //     current System.IO.FileSystemInfo object.
        //
        // Returns:
        //     The creation date and time in UTC format of the current System.IO.FileSystemInfo
        //     object.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.IO.DirectoryNotFoundException:
        //     The specified path is invalid, such as being on an unmapped drive.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Microsoft Windows NT or later.
        [DataMember]
        public DateTime CreationTimeUtc
        {
            get { return this.creationTimeUtc; }
            set { this.creationTimeUtc = value; }
        }
        
        //
        // Summary:
        //     Gets a value indicating whether the file or directory exists.
        //
        // Returns:
        //     true if the file or directory exists; otherwise, false.
        [DataMember]
        public abstract bool Exists { get; set; }
        
        //
        // Summary:
        //     Gets the string representing the extension part of the file.
        //
        // Returns:
        //     A string containing the System.IO.FileSystemInfo extension.
        [DataMember]
        public string Extension
        {
            get { return this.extension; }
            set { this.extension = value; }
        }
        
        //
        // Summary:
        //     Gets the full path of the directory or file.
        //
        // Returns:
        //     A string containing the full path.
        //
        // Exceptions:
        //   System.Security.SecurityException:
        //     The caller does not have the required permission.
        [DataMember]
        public virtual string FullName
        {
            get { return this.fullName; }
            set { this.fullName = value; }
        }
        
        //
        // Summary:
        //     Gets or sets the time the current file or directory was last accessed.
        //
        // Returns:
        //     The time that the current file or directory was last accessed.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Microsoft Windows NT or later.
        [DataMember]
        public DateTime LastAccessTime
        {
            get { return this.lastAccessTime; }
            set { this.lastAccessTime = value; }
        }
        
        //
        // Summary:
        //     Gets or sets the time, in coordinated universal time (UTC), that the current
        //     file or directory was last accessed.
        //
        // Returns:
        //     The UTC time that the current file or directory was last accessed.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Microsoft Windows NT or later.
        [DataMember]
        public DateTime LastAccessTimeUtc
        {
            get { return this.lastAccessTimeUtc; }
            set { this.lastAccessTimeUtc = value; }
        }
        
        //
        // Summary:
        //     Gets or sets the time when the current file or directory was last written
        //     to.
        //
        // Returns:
        //     The time the current file was last written.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Microsoft Windows NT or later.
        [DataMember]
        public DateTime LastWriteTime
        {
            get { return this.lastWriteTime; }
            set { this.lastWriteTime = value; }
        }
        
        //
        // Summary:
        //     Gets or sets the time, in coordinated universal time (UTC), when the current
        //     file or directory was last written to.
        //
        // Returns:
        //     The UTC time when the current file was last written to.
        //
        // Exceptions:
        //   System.IO.IOException:
        //     System.IO.FileSystemInfo.Refresh() cannot initialize the data.
        //
        //   System.PlatformNotSupportedException:
        //     The current operating system is not Microsoft Windows NT or later.
        [DataMember]
        public DateTime LastWriteTimeUtc
        {
            get { return this.lastWriteTimeUtc; }
            set { this.lastWriteTimeUtc = value; }
        }
        
        //
        // Summary:
        //     For files, gets the name of the file. For directories, gets the name of the
        //     last directory in the hierarchy if a hierarchy exists. Otherwise, the Name
        //     property gets the name of the directory.
        //
        // Returns:
        //     A string that is the name of the parent directory, the name of the last directory
        //     in the hierarchy, or the name of a file, including the file name extension.
        [DataMember]
        public abstract string Name { get; set;  }
    }
}
