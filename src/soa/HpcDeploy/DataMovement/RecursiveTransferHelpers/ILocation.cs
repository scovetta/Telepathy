//------------------------------------------------------------------------------
// <copyright file="ILocation.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Location interface.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Location interface.
    /// </summary>
    internal interface ILocation
    {
        int GetMaxFileNameLength();

        /// <summary>
        /// Enumerates the files present in the storage location referenced by this object.
        /// </summary>
        /// <param name="filePatterns">Search pattern of files/prefix of blobs to return.</param>
        /// <param name="recursive">Indicates whether to recursively copy files.</param>
        /// <param name="getLastModifiedTime">Indicates whether we should retrieve the last modified time or not.</param>
        /// <param name="cancellationTokenSource">CancellationTokenSource for AzureStorageLocation to register cancellation handler to.</param>
        /// <returns>Enumerable list of FileEntry objects found in the storage location referenced by this object.</returns>
        IEnumerable<FileEntry> EnumerateLocation(IEnumerable<string> filePatterns, bool recursive, bool getLastModifiedTime, CancellationTokenSource cancellationTokenSource);

        /// <summary>
        /// Apply default file pattern to the passed in filePatterns list.
        /// </summary>
        /// <param name="filePatterns">File pattern to parse.</param>
        /// <returns>If filePatterns is null or empty return default file pattern. Otherwise return passed in file patterns.</returns>
        IEnumerable<string> GetFilePatternWithDefault(IEnumerable<string> filePatterns);
    }
}
