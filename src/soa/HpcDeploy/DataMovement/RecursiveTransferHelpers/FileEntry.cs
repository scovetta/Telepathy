//------------------------------------------------------------------------------
// <copyright file="FileEntry.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      FileEntry class to represent a single transfer file entry.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System;

    /// <summary>
    /// FileEntry class to represent a single transfer file entry.
    /// </summary>
    internal class FileEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileEntry" /> class.
        /// </summary>
        /// <param name="entry">File entry to copy.</param>
        public FileEntry(FileEntry entry)
            : this(entry.RelativePath, entry.LastModified, entry.SnapshotTime)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEntry" /> class.
        /// </summary>
        /// <param name="relativePath">Relative path of the file indicated by this file entry.</param>
        public FileEntry(string relativePath)
            : this(relativePath, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEntry" /> class.
        /// </summary>
        /// <param name="relativePath">Relative path of the file indicated by this file entry.</param>
        /// <param name="lastModified">Indicates the last updated time of the file entry in UTC time.</param>
        public FileEntry(string relativePath, DateTimeOffset? lastModified)
            : this(relativePath, lastModified, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileEntry" /> class.
        /// </summary>
        /// <param name="relativePath">Relative path of the file indicated by this file entry.</param>
        /// <param name="lastModified">Indicates the last updated time of the file entry in UTC time.</param>
        /// <param name="snapshotTime">Indicates the snapshot time of a blob snapshot.</param>
        public FileEntry(string relativePath, DateTimeOffset? lastModified, DateTimeOffset? snapshotTime)
        {
            this.RelativePath = relativePath;
            this.LastModified = lastModified;
            this.SnapshotTime = snapshotTime;
        }

        /// <summary>
        /// Gets the relative path of the file indicated by this file entry.
        /// </summary>
        public string RelativePath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the last updated time of the file entry in UTC time.
        /// </summary>
        public DateTimeOffset? LastModified
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the snap shot time of the blob snapshot.
        /// </summary>
        public DateTimeOffset? SnapshotTime
        {
            get;
            private set;
        }
    }
}
