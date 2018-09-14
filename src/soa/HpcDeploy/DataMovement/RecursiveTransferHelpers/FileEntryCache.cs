//------------------------------------------------------------------------------
// <copyright file="FileEntryCache.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      FileEntryCache class to cache results from enumerating over a Location.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// FileEntryCache class to cache results from enumerating over a <see cref="Location" />.
    /// </summary>
    /// This class is mainly used to cache destination files entries.
    /// Local file system supports wildcard characters, while Azure supports only prefix match.
    /// In order to check whether overwrite prompt is needed correctly, we have to load all files
    /// from the destination.
    internal class FileEntryCache
    {
        private Dictionary<string, FileEntry> fileEntryCache;

        private bool azureSource;

        public FileEntryCache(ILocation cachedLocation, bool getLastModifiedTime, CancellationTokenSource cancellationTokenSource)
        {
            this.fileEntryCache = new Dictionary<string, FileEntry>();

            this.azureSource = cachedLocation is AzureStorageLocation;

            foreach (FileEntry entry in cachedLocation.EnumerateLocation(null, true, getLastModifiedTime, cancellationTokenSource))
            {
                ErrorFileEntry errorEntry = entry as ErrorFileEntry;

                if (null == errorEntry)
                {
                    // Ignore snapshots as we never treat it as a target file.
                    if (!entry.SnapshotTime.HasValue)
                    {
                        string fileName = this.UniformFileName(entry.RelativePath);

                        Debug.Assert(
                            !this.fileEntryCache.ContainsKey(fileName),
                            string.Format("We tried to add key {0} into dictionary, but found there was already one...", fileName));

                        this.fileEntryCache[fileName] = entry;
                    }
                }
                else
                {
                    this.Exception = errorEntry.Exception;
                    this.fileEntryCache = null;
                    break;
                }
            }
        }

        public Exception Exception
        {
            get;
            private set;
        }

        public FileEntry GetFileEntry(string fileName)
        {
            if (null != this.Exception)
            {
                return null;
            }

            fileName = this.UniformFileName(fileName);

            FileEntry entry;
            if (this.fileEntryCache.TryGetValue(fileName, out entry))
            {
                return entry;
            }

            return null;
        }

        private string UniformFileName(string fileName)
        {
            return this.azureSource ? fileName : fileName.ToLowerInvariant();
        }
    }
}
