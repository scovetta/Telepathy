//------------------------------------------------------------------------------
// <copyright file="FileNameResolver.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      FileNameResolver factory methods.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// FileNameResolver base class and factory methods.
    /// </summary>
    internal static class FileNameResolver
    {
        /// <summary>
        /// Gets a file name resolver object based on whether the source and destination object are
        /// AzureStorageLocation or FileSystemLocation objects.
        /// If both locations are either AzureStorageLocation or FileSystemLocation objects a PassthroughFileNameResolver object is returned.
        /// If the source location is an AzureStorageLocation while the destination location is a FileSystemLocation an AzureToFileSystemFileNameResolver object is returned.
        /// If the source location is a FilesystemLocation while the destination location is an AzureStorageLocation a FileSystemToAzureFileNameResolver object is returned.
        /// </summary>
        /// <param name="sourceLocation">Source location object.</param>
        /// <param name="destinationLocation">Destination location object.</param>
        /// <param name="delimiter">Directory delimiter used in the blob names.</param>
        /// <returns>File name resolver to translate filenames from the source location to valid names for the destination location.</returns>
        public static IFileNameResolver GetFileNameResolver(
            ILocation sourceLocation, 
            ILocation destinationLocation,
            char? delimiter)
        {
            bool sourceIsOnAzure = sourceLocation is AzureStorageLocation;
            bool destinationIsOnAzure = destinationLocation is AzureStorageLocation;

            if (sourceIsOnAzure == destinationIsOnAzure)
            {
                if (sourceIsOnAzure)
                {
                    return new FileNameSnapshotAppender();
                }
                else
                {
                    Debug.Fail("We should never be here");
                    return null;
                }
            }
            else
            {
                if (sourceIsOnAzure)
                {
                    return new AzureToFileSystemFileNameResolver(
                        destinationLocation.GetMaxFileNameLength, 
                        delimiter);
                }
                else
                {
                    return new FileSystemToAzureFileNameResolver();
                }
            }
        }

        /// <summary>
        /// Turns baseFileName into a valid file name by calling Conflict and Construct.
        /// The procedures are enumerating numbers from 1 and trying to append the number to the base file name.
        /// Conflict is used to test whether current generated file name conflicts with others.
        /// Construct is supposed to generate a file name based on the three parameters, file name without extension, extension and the number to append.
        /// </summary>
        /// <param name="baseFileName">Original file name.</param>
        /// <param name="conflict">A delegate takes one file name as parameter and returns true when no confliction is found.</param>
        /// <param name="construct">A delegate takes three parameters, file name without extension, extension and the number to append, and returns
        /// a file name constructed by these three parameters.</param>
        /// <returns>Valid file name by calling Conflict and Construct.</returns>
        public static string ResolveFileNameConflict(string baseFileName, Func<string, bool> conflict, Func<string, string, int, string> construct)
        {
            if (!conflict(baseFileName))
            {
                return baseFileName;
            }

            string pathAndFilename = Path.ChangeExtension(baseFileName, null);
            string extension = Path.GetExtension(baseFileName);

            string resolvedName = string.Empty;
            int postfixCount = 1;

            do
            {
                resolvedName = construct(pathAndFilename, extension, postfixCount);
                postfixCount++;
            }
            while (conflict(resolvedName));

            return resolvedName;
        }
    }
}
