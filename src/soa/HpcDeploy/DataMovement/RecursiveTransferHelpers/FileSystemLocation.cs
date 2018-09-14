//------------------------------------------------------------------------------
// <copyright file="FileSystemLocation.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Location class to represent file system location.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;

    /// <summary>
    /// Location class to represent file system location.
    /// </summary>
    internal class FileSystemLocation : ILocation
    {
        /// <summary>
        /// Maximum windows file path is 260 characters, including a terminating NULL characters.
        /// This leaves 259 useable characters.
        /// </summary>
        ////private const int MaxPathLength = 259;
        // TODO - Windows file path has 2 limits. 
        //   1) Full file name can not be longer than 259 characters. 
        //   2) Folder path can not be longer than 247 characters excluding the file name. 
        // We currently handle the full file name only, which breaks if the path contains a folder name which exceeds the 247 characters limit.
        // Needs to be fixed, but use 247 as max path for now as a temporary work around.
        private const int MaxPathLength = 247;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemLocation" /> class.
        /// </summary>
        /// <param name="location">Path to the file system storage location to parse.</param>
        public FileSystemLocation(string location)
        {            
            string directory = Path.GetFullPath(location);

            // Normalize directory to end with back slash.
            if (!directory.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                directory += Path.DirectorySeparatorChar;
            }

            this.FullPath = directory;
        }

        public string FullPath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum file name length of any file name relative to this file system source location. 
        /// </summary>
        /// <returns>Maximum file name length in bytes.</returns>
        public int GetMaxFileNameLength()
        {
            return MaxPathLength - this.FullPath.Length;
        }

        /// <summary>
        /// Enumerates the files present in the storage location referenced by this object.
        /// </summary>
        /// <param name="filePatterns">Search pattern of files to return.</param>
        /// <param name="recursive">Indicates whether to recursively copy files.</param>
        /// <param name="getLastModifiedTime">Indicates whether we should retrieve the last modified time or not.</param>
        /// <param name="cancellationTokenSource">CancellationTokenSource for AzureStorageLocation to register cancellation handler to.</param>
        /// <returns>Enumerable list of FileEntry objects found in the storage location referenced by this object.</returns>
        public IEnumerable<FileEntry> EnumerateLocation(IEnumerable<string> filePatterns, bool recursive, bool getLastModifiedTime, CancellationTokenSource cancellationTokenSource)
        {
            CancellationChecker cancellationChecker = new CancellationChecker();
            using (CancellationTokenRegistration tokenRegistration =
                cancellationTokenSource.Token.Register(cancellationChecker.Cancel))
            {
                IEnumerable<string> filePatternsWithDefault = this.GetFilePatternWithDefault(filePatterns);
                HashSet<string> returnedEntries = new HashSet<string>();

                int maxFileNameLength = this.GetMaxFileNameLength();

                foreach (string filePattern in filePatternsWithDefault)
                {
                    // Exceed-limit-length patterns surely match no files.
                    if (filePattern.Length > maxFileNameLength)
                    {
                        continue;
                    }

                    SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                    IEnumerable<string> directoryEnumerator = null;

                    ErrorFileEntry errorEntry = null;

                    cancellationChecker.CheckCancellation();

                    try
                    {
                        // Directory.GetFiles/EnumerateFiles will be broken when encounted special items, such as
                        // files in recycle bins or the folder "System Volume Information". Rewrite this function
                        // because our listing should not be stopped by these unexpected files. 
                        directoryEnumerator = EnumerateDirectoryHelper.EnumerateFiles(this.FullPath, filePattern, searchOption, cancellationTokenSource);
                    }
                    catch (Exception ex)
                    {
                        errorEntry = new ErrorFileEntry(ex);
                    }

                    if (null != errorEntry)
                    {
                        // We any exception we might get from Directory.GetFiles/
                        // Directory.EnumerateFiles. Just return an error FileEntry
                        // to indicate error occured in this case. 
                        yield return errorEntry;

                        // TODO: What should we do if some entries have been listed successfully?
                        yield break;
                    }

                    if (null != directoryEnumerator)
                    {
                        foreach (string entry in directoryEnumerator)
                        {
                            cancellationChecker.CheckCancellation();

                            string relativeEntry = entry;

                            if (relativeEntry.StartsWith(this.FullPath, StringComparison.OrdinalIgnoreCase))
                            {
                                relativeEntry = relativeEntry.Remove(0, this.FullPath.Length);
                            }

                            if (!returnedEntries.Contains(relativeEntry))
                            {
                                returnedEntries.Add(relativeEntry);

                                DateTime? lastModifiedUtc = null;

                                if (getLastModifiedTime)
                                {
                                    lastModifiedUtc = File.GetLastWriteTimeUtc(entry);
                                }

                                yield return new FileEntry(relativeEntry, lastModifiedUtc);
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<string> GetFilePatternWithDefault(IEnumerable<string> filePatterns)
        {
            if (null != filePatterns && filePatterns.Any())
            {
                return filePatterns;
            }
            else
            {
                List<string> filePattern = new List<string>();
                filePattern.Add("*");

                return filePattern;
            }
        }

        public string GetAbsolutePath(string fileName)
        {
            return Path.Combine(this.FullPath, fileName);
        }
    }
}
