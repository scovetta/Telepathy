//------------------------------------------------------------------------------
// <copyright file="Location.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Location class to represent source or destination location.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System;

    /// <summary>
    /// Location class to represent source or destination location.
    /// </summary>
    internal static class Location
    {
        /// <summary>
        /// Prefix for a destination located on Azure Storage (http).
        /// </summary>
        private const string OnAzurePrefixHttp = "http://";

        /// <summary>
        /// Prefix for a destination location on Azure Storage (https).
        /// </summary>
        private const string OnAzurePrefixHttps = "https://";

        public static ILocation CreateLocation(string location, string storageKey, string containerSAS, BlobTransferOptions transferOptions, bool isSourceLocation)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new ArgumentNullException("location");
            }

            if (IsOnAzure(location))
            {
                return new AzureStorageLocation(location, storageKey, containerSAS, transferOptions, isSourceLocation);
            }
            else
            {
                return new FileSystemLocation(location);
            }
        }

        internal static bool Equals(ILocation locationA, ILocation locationB)
        {
            if (locationA == locationB)
            {
                return true;
            }

            if (null == locationA || null == locationB)
            {
                return false;
            }

            AzureStorageLocation azureLocationA = locationA as AzureStorageLocation;
            AzureStorageLocation azureLocationB = locationB as AzureStorageLocation;

            if ((null == azureLocationA) != (null == azureLocationB))
            {
                return false;
            }

            if (null == azureLocationA)
            {
                // Both are FileSystemLocation.
                FileSystemLocation fileSystemLocationA = locationA as FileSystemLocation;
                FileSystemLocation fileSystemLocationB = locationB as FileSystemLocation;

                return fileSystemLocationA.FullPath.Equals(fileSystemLocationB.FullPath);
            }
            else
            {
                // Both are AzureStorageLocation.
                string pathA = azureLocationA.GetAbsoluteUri(string.Empty);
                string pathB = azureLocationB.GetAbsoluteUri(string.Empty);

                return pathA.Equals(pathB);
            }
        }

        private static bool IsOnAzure(string location)
        {
            return location.StartsWith(OnAzurePrefixHttp, StringComparison.OrdinalIgnoreCase) || location.StartsWith(OnAzurePrefixHttps, StringComparison.OrdinalIgnoreCase);
        }
    }
}
