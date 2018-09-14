//------------------------------------------------------------------------------
// <copyright file="AzureStorageLocation.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Location class to represent azure storage source/destination location.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// Location class to represent azure storage source/destination location.
    /// </summary>
    internal class AzureStorageLocation : ILocation
    {
#if DEBUG
        // TODO: Here is just format for public azure. 
        // We'll need to add mooncake support in the future.

        /// <summary>
        /// Blob address pattern. We'll treat URI string with this pattern in it as a blob address, 
        /// or we'll treat it as an IP address/host name URI.
        /// </summary>
        private const string WindowsAzureBlobFormat = "blob.core.windows.net";
#endif

        /// <summary>
        /// Logs container name.
        /// </summary>
        private const string LogsContainerName = "$logs";

        /// <summary>
        /// Configures how many entries to request in each ListBlobsSegmented call from Azure Storage.
        /// Configuring a larger number will require fewer calls to Azure Storage, but each call will take longer to complete.
        /// Maximum supported by the Azure Storage API is 5000. Anything above this is rounded down to 5000.
        /// </summary>
        private const int ListBlobsSegmentSize = 250;

        /// <summary>
        /// A blob name (excluding container name) can be at most 1024 character long based on Windows Azure documentation.
        /// See http://msdn.microsoft.com/en-us/library/windowsazure/dd135715.aspx for details.
        /// </summary>
        private const int MaxBlobNameLength = 1024;

        /// <summary>
        /// Indicate whether location path uses endpoint path style or not.
        /// The endpoint path style: http://{IpAddress}:{Port}/{accountName}.
        /// </summary>
        private readonly bool EndPointPathStyle = false;

        /// <summary>
        /// Transfer options used in network connections.
        /// </summary>
        private BlobTransferOptions transferOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageLocation" /> class.
        /// </summary>
        /// <param name="location">Path to the azure storage location to parse.</param>
        /// <param name="storageKey">Optional storage key to access the specified Azure Storage location.</param>
        /// <param name="containerSAS">Optional Shared Access Signature to access the specified Azure Storage container.</param>
        /// <param name="transferOptions">Transfer options used in network connections.</param>
        /// <param name="isSourceLocation">Indicates whether this object represents a source location.</param>
        public AzureStorageLocation(
            string location, 
            string storageKey, 
            string containerSAS, 
            BlobTransferOptions transferOptions, 
            bool isSourceLocation)
        {
            // When destination is an Azure Storage location, one of storageKey or containerSAS is always required.
            if (!isSourceLocation && string.IsNullOrEmpty(storageKey) && string.IsNullOrEmpty(containerSAS))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ProvideExactlyOneParameterBothNullException,
                    "storageKey",
                    "containerSAS"));
            }

            // At most one of storageKey and containerSAS could be provided.
            if (!string.IsNullOrEmpty(storageKey) && !string.IsNullOrEmpty(containerSAS))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.ProvideAtMostOneParameterBothProvidedException,
                    "storageKey",
                    "containerSAS"));
            }

            Uri azureUri = new Uri(location);

#if DEBUG
            this.EndPointPathStyle = !azureUri.OriginalString.Contains(WindowsAzureBlobFormat);
#else
            IPAddress testParseIPAddress;
            this.EndPointPathStyle = IPAddress.TryParse(azureUri.Host, out testParseIPAddress);
#endif

            string containerFolderPath = string.Empty;

            if (this.EndPointPathStyle)
            {
                int accountPathSeparator = azureUri.AbsolutePath.IndexOf('/', 1);

                if (-1 == accountPathSeparator)
                {
                    this.AccountName = azureUri.AbsolutePath.Substring(1);
                }
                else
                {
                    this.AccountName = azureUri.AbsolutePath.Substring(1, accountPathSeparator - 1);
                    containerFolderPath = azureUri.AbsolutePath.Substring(accountPathSeparator);
                }

                this.BaseAddress = string.Format("{0}/{1}", azureUri.GetLeftPart(UriPartial.Authority), this.AccountName);
            }
            else
            {
                this.AccountName = azureUri.Host.Split(new char[] { '.' }, 2)[0];
                this.BaseAddress = azureUri.GetLeftPart(UriPartial.Authority);

                containerFolderPath = azureUri.AbsolutePath;
            }

            if (string.IsNullOrEmpty(this.AccountName))
            {
                throw new ArgumentException(string.Format(Resources.CannotParseAccountFromUriException, location), "location");
            }

            string containerName = string.Empty;
            string folder = string.Empty;

            if (!string.IsNullOrEmpty(containerFolderPath))
            {
                int containerFolderSeparator = containerFolderPath.IndexOf('/', 1);

                if (-1 == containerFolderSeparator)
                {
                    containerName = containerFolderPath.Substring(1);
                }
                else
                {
                    containerName = containerFolderPath.Substring(1, containerFolderSeparator - 1);
                    folder = containerFolderPath.Substring(containerFolderSeparator + 1);
                }
            }

            if (string.IsNullOrEmpty(containerName))
            {
                containerName = BlobTransferConstants.DefaultContainerName;
            }

            if (containerName.Equals(LogsContainerName))
            {
                // Allow use of logs container as source location.
                if (!isSourceLocation)
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.ContainerOnlyValidForSourceException,
                        LogsContainerName));
                }
            }
            else
            {
                // The regular expression below is build up as follows:
                // Either "$root"
                //  - OR -
                // Start with either a letter or digit:                       ^[a-z0-9]
                // Followed by: 2-62 characters that comply to:               (...){2,62}$
                //   These inner characters can be either:
                //     A letter or digit:                                     ([a-z0-9]|
                //     Or a dash surround by a letter or digit on both sides: (?<=[a-z0-9])-(?=[a-z0-9])
                if (!Regex.IsMatch(containerName, @"^\$root$|^[a-z0-9]([a-z0-9]|(?<=[a-z0-9])-(?=[a-z0-9])){2,62}$"))
                {
                    throw new ArgumentException("Invalid container name", "containerName");
                }
            }

            // Normalize folder to end with slash.
            if (!string.IsNullOrEmpty(folder) && !folder.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                folder += '/';
            }

            if (containerName.Equals(BlobTransferConstants.DefaultContainerName) &&
                !string.IsNullOrEmpty(folder))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.SubfoldersNotAllowedUnderRootContainerException));
            }

            this.ContainerName = containerName;
            this.Folder = Uri.UnescapeDataString(folder);

            if (!string.IsNullOrEmpty(storageKey))
            {
                this.StorageCredential = new StorageCredentials(this.AccountName, storageKey);
            }
            else if (!string.IsNullOrEmpty(containerSAS))
            {
                this.StorageCredential = new StorageCredentials(containerSAS);
            }
            else
            {
                this.StorageCredential = new StorageCredentials();
            }

            this.BlobContainer = new CloudBlobContainer(
                new Uri(string.Format("{0}/{1}", this.BaseAddress, this.ContainerName)), 
                this.StorageCredential);

            this.transferOptions = transferOptions;
        }

        /// <summary>
        /// Gets the account name of this azure storage location.
        /// </summary>
        public string AccountName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the base DNS address of this azure storage location.
        /// </summary>
        public string BaseAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the container name of this azure storage location.
        /// </summary>
        public string ContainerName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the folder of this azure storage location.
        /// </summary>
        public string Folder
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the storage credential of this azure storage location.
        /// </summary>
        public StorageCredentials StorageCredential
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the CloudBlobContainer object of this azure storage container.
        /// </summary>
        public CloudBlobContainer BlobContainer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum file name length of any blob relative to this objects storage location. 
        /// </summary>
        /// <returns>Maximum file name length in bytes.</returns>
        public int GetMaxFileNameLength()
        {
            return MaxBlobNameLength - this.Folder.Length;
        }

        /// <summary>
        /// Enumerates the blobs present in the storage location referenced by this object.
        /// </summary>
        /// <param name="filePatterns">Prefix of blobs to return.</param>
        /// <param name="recursive">Indicates whether to recursively copy files.</param>
        /// <param name="getLastModifiedTime">Indicates whether we should retrieve the last modified time or not. 
        /// For AzureStorageLocation the last modified time is always retrieved regardless of this flag as there
        /// is no extra cost involved in retrieving this data.</param>
        /// <param name="cancellationTokenSource">CancellationTokenSource for AzureStorageLocation to register cancellation handler to.</param>
        /// <returns>Enumerable list of FileEntry objects found in the storage location referenced by this object.</returns>
        public IEnumerable<FileEntry> EnumerateLocation(IEnumerable<string> filePatterns, bool recursive, bool getLastModifiedTime, CancellationTokenSource cancellationTokenSource)
        {
            CancellationChecker cancellationChecker = new CancellationChecker();

            using (CancellationTokenRegistration tokenRegistration = 
                cancellationTokenSource.Token.Register(
                cancellationChecker.Cancel))
            {
                IEnumerable<string> filePatternsWithDefault = this.GetFilePatternWithDefault(filePatterns);

                string fullPrefix = string.Format(
                    "/{0}/{1}",
                    this.ContainerName,
                    this.Folder);

                if (this.EndPointPathStyle)
                {
                    fullPrefix = string.Concat("/", this.AccountName, fullPrefix);
                }

                // Analyze file patterns to support case-insensitive feature.
                HashSet<string> filePatternSet = new HashSet<string>();
                int maxFileNameLength = this.GetMaxFileNameLength();

                foreach (string filePattern in filePatternsWithDefault)
                {
                    // Exceed-limit-length patterns surely match no files.
                    if (filePattern.Length > maxFileNameLength)
                    {
                        continue;
                    }

                    char[] lowerPatternChars = filePattern.ToLowerInvariant().ToCharArray();
                    int filePatternLength = filePattern.Length;

                    int firstAlphaPlace = 0;
                    while (firstAlphaPlace < filePatternLength && !char.IsLower(lowerPatternChars[firstAlphaPlace]))
                    {
                        ++firstAlphaPlace;
                    }

                    string noAlphaPrefix = filePattern.Substring(0, firstAlphaPlace);
                    if (firstAlphaPlace == filePatternLength)
                    {
                        filePatternSet.Add(noAlphaPrefix);
                    }
                    else
                    {
                        char firstAlphaChar = lowerPatternChars[firstAlphaPlace];
                        filePatternSet.Add(noAlphaPrefix + firstAlphaChar);
                        filePatternSet.Add(noAlphaPrefix + char.ToUpperInvariant(firstAlphaChar));
                    }
                }

                BlobRequestOptions requestOptions = this.transferOptions.GetBlobRequestOptions(BlobRequestOperation.ListBlobs);
                OperationContext operationContext = new OperationContext()
                {
                    ClientRequestID = this.transferOptions.GetClientRequestId(),
                };

                // A HashSet to remove duplicate file entries because of multiple file patterns.
                HashSet<string> fileFound = new HashSet<string>();

                foreach (string filePattern in filePatternSet)
                {
                    BlobContinuationToken continuationToken = null;

                    do
                    {
                        BlobResultSegment resultSegment = null;

                        ErrorFileEntry errorEntry = null;

                        cancellationChecker.CheckCancellation();

                        try
                        {
                            // TODO: Currently keep it to be a sync call here. We may need to change this to be async and cancellable in the future.
                            resultSegment = this.BlobContainer.ListBlobsSegmented(
                                string.Format(
                                    "{0}{1}",
                                    this.Folder,
                                    filePattern),
                                true,
                                BlobListingDetails.Snapshots,
                                ListBlobsSegmentSize,
                                continuationToken,
                                requestOptions,
                                operationContext);
                        }
                        catch (Exception ex)
                        {
                            errorEntry = new ErrorFileEntry(ex);
                        }

                        if (null != errorEntry)
                        {
                            // Just return an error FileEntry if we cannot access
                            // the container; most likely the container doesn't
                            // exist.
                            yield return errorEntry;

                            // TODO: What should we do if some entries have been listed successfully?
                            yield break;
                        }

                        continuationToken = resultSegment.ContinuationToken;

                        foreach (IListBlobItem blobItem in resultSegment.Results)
                        {
                            cancellationChecker.CheckCancellation();

                            ICloudBlob blob = blobItem as ICloudBlob;

                            if (null != blob)
                            {
                                string blobFullPath = Uri.UnescapeDataString(blob.Uri.AbsolutePath);
                                string anchorBlobPath = Utils.AppendSnapShotToFileName(blobFullPath, blob.SnapshotTime);

                                // Distinguish snapshots from blobs.
                                anchorBlobPath += blob.SnapshotTime.HasValue ? "-S" : "-B";

                                if (!fileFound.Contains(anchorBlobPath))
                                {
                                    fileFound.Add(anchorBlobPath);

                                    foreach (string filePatternForCheck in filePatternsWithDefault)
                                    {
                                        string patternPrefix = fullPrefix + filePatternForCheck;

                                        // TODO: currrently not support search for files with prefix specified without considering sub-directory.
                                        bool returnItOrNot = recursive ?
                                            blobFullPath.StartsWith(patternPrefix, StringComparison.OrdinalIgnoreCase) :
                                            blobFullPath.Equals(patternPrefix, StringComparison.OrdinalIgnoreCase);

                                        if (returnItOrNot)
                                        {
                                            yield return new AzureFileEntry(blobFullPath.Remove(0, fullPrefix.Length), blob);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    while (continuationToken != null);
                }
            }
        }

        /// <summary>
        /// Apply default file pattern to the passed in filePatterns list.
        /// </summary>
        /// <param name="filePatterns">File pattern to parse.</param>
        /// <returns>If filePatterns is null or empty return default file pattern. Otherwise return passed in file patterns.</returns>
        public IEnumerable<string> GetFilePatternWithDefault(IEnumerable<string> filePatterns)
        {
            if (null != filePatterns && filePatterns.Any())
            {
                return filePatterns;
            }
            else
            {
                List<string> filePattern = new List<string>();
                filePattern.Add(string.Empty);

                return filePattern;
            }
        }

        /// <summary>
        /// Returns the path under container based on this objects storage location and the passed in relative path.
        /// </summary>
        /// <param name="relativePath">Relative path.</param>
        /// <returns>A string representing the target path.</returns>
        public string GetPathUnderContainer(string relativePath)
        {
            return string.Format("{0}{1}", this.Folder, relativePath);
        }

        /// <summary>
        /// Returns absolute uri based on this objects storage location and the passed in relative path.
        /// </summary>
        /// <param name="relativePath">Relative path.</param>
        /// <returns>A string representing absolute uri.</returns>
        public string GetAbsoluteUri(string relativePath)
        {
            return string.Format(
                "{0}/{1}",
                this.BlobContainer.Uri,
                Uri.EscapeDataString(this.GetPathUnderContainer(relativePath)));
        }

        /// <summary>
        /// Returns an ICloudBlob object based on this objects storage location and the passed in relative path.
        /// </summary>
        /// <param name="relativePath">Relative path of the blob to get an ICloudBlob object reference to.</param>
        /// <param name="blobType">Target blob type.</param>
        /// <returns>ICloudBlob object.</returns>
        public ICloudBlob GetBlobObject(string relativePath, BlobType blobType)
        {
            return this.GetBlobObject(relativePath, null, blobType);
        }

        /// <summary>
        /// Returns an ICloudBlob object based on this objects storage location and the passed in relative path.
        /// </summary>
        /// <param name="relativePath">Relative path of the blob to get an ICloudBlob object reference to.</param>
        /// <param name="snapshotTime">Snapshot time of the blob snapshot to return. Pass null to return a reference to a non-snapshot blob.</param>
        /// <param name="blobType">Target blob type.</param>
        /// <returns>ICloudBlob object.</returns>
        public ICloudBlob GetBlobObject(string relativePath, DateTimeOffset? snapshotTime, BlobType blobType)
        {
            string pathUnderContainer = this.GetPathUnderContainer(relativePath);

            if (BlobType.PageBlob == blobType)
            {
                return this.BlobContainer.GetPageBlobReference(pathUnderContainer, snapshotTime);
            }
            else
            {
                return this.BlobContainer.GetBlockBlobReference(pathUnderContainer, snapshotTime);
            }
        }
    }
}
