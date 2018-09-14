//------------------------------------------------------------------------------
// <copyright file="BlobTransferRecursiveTransferOptions.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      BlobTransferRecursiveTransferOptions class.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Hpc.Azure.DataMovement.BlobTransferCallbacks;

    /// <summary>
    /// BlobTransferRecursiveTransferOptions class.
    /// </summary>
    public class BlobTransferRecursiveTransferOptions
    {
        /// <summary>
        /// The Azure Storage key for accessing source location.
        /// At most one parameter of SourceKey and SourceSAS
        /// could be provided.
        /// </summary>
        private string sourceKey;

        /// <summary>
        /// Shared Access Signature for source container.
        /// </summary>
        private string sourceSAS;

        /// <summary>
        /// The Azure Storage key for accessing destination 
        /// location. At most one parameter of DestinationKey and DestinationSAS 
        /// could be provided.
        /// </summary>
        private string destinationKey;
        
        /// <summary>
        /// Shared Access Signature for destination container.
        /// </summary>
        private string destinationSAS;

        /// <summary>
        /// The type of blob to use for uploads to Azure Storage.
        /// </summary>
        private BlobType uploadBlobType;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="BlobTransferRecursiveTransferOptions" /> class.
        /// </summary>
        public BlobTransferRecursiveTransferOptions()
        {
            // setup default values.
            this.SourceKey = string.Empty;
            this.SourceSAS = string.Empty;
            this.DestinationKey = string.Empty;
            this.DestinationSAS = string.Empty;
            this.Recursive = true;
            this.KeepLastWriteTime = false;
            this.FakeTransfer = false;
            this.ExcludeNewer = false;
            this.ExcludeOlder = false;
            this.OnlyFilesWithArchiveBit = false;
            this.UploadBlobType = BlobType.PageBlob;
        }

        /// <summary>
        /// Gets or sets the Azure Storage key for accessing source 
        /// location. At most one parameter of SourceKey and SourceSAS 
        /// could be provided.
        /// </summary>
        /// <value>Azure Storage key.</value>
        public string SourceKey
        {
            get
            { 
                return this.sourceKey;
            }

            set
            {
                try
                {
                    Convert.FromBase64String(value);
                }
                catch (FormatException ex)
                {
                    throw new ArgumentException(
                        string.Format(
                            Resources.StorageKeyInvalidFormatException,
                            "SourceKey"),
                        ex);
                }

                if (!string.IsNullOrEmpty(this.sourceSAS)
                    && !string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(
                        string.Format(
                            Resources.CanOnlySetOneCredentialException,
                            "SourceKey",
                            "SourceSAS"));
                }

                this.sourceKey = value;
            }
        }

        /// <summary>
        /// Gets or sets an Shared Access Signature for accessing source 
        /// container. At most one parameter of SourceKey and SourceSAS
        /// could be provided.
        /// </summary>
        /// <value>Shared Access Signature for source container.</value>
        public string SourceSAS
        {
            get
            {
                return this.sourceSAS;
            }

            set
            {
                if (!string.IsNullOrEmpty(this.sourceKey)
                    && !string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(
                        string.Format(
                            Resources.CanOnlySetOneCredentialException,
                            "SourceKey",
                            "SourceSAS"));
                }

                this.sourceSAS = value;
            }
        }

        /// <summary>
        /// Gets or sets the Azure Storage key for accessing destination 
        /// location. At most one parameter of DestinationKey and DestinationSAS 
        /// could be provided.
        /// </summary>
        /// <value>Azure Storage key.</value>
        public string DestinationKey
        {
            get
            {
                return this.destinationKey;
            }

            set
            {
                try
                {
                    Convert.FromBase64String(value);
                }
                catch (FormatException ex)
                {
                    throw new ArgumentException(
                        string.Format(
                            Resources.StorageKeyInvalidFormatException,
                            "DestinationKey"),
                        ex);
                }

                if (!string.IsNullOrEmpty(this.destinationSAS)
                    && !string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(
                        string.Format(
                            Resources.CanOnlySetOneCredentialException,
                            "DestinationKey",
                            "DestinationSAS"));
                }

                this.destinationKey = value;
            }
        }

        /// <summary>
        /// Gets or sets an Shared Access Signature for accessing destination 
        /// container. At most one parameter of DestinationKey and DestinationSAS
        /// could be provided.
        /// </summary>
        /// <value>Shared Access Signature for destination container.</value>
        public string DestinationSAS
        {
            get
            {
                return this.destinationSAS;
            }

            set
            {
                if (!string.IsNullOrEmpty(this.destinationKey)
                    && !string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(
                        string.Format(
                            Resources.CanOnlySetOneCredentialException,
                            "DestinationKey",
                            "DestinationSAS"));
                }

                this.destinationSAS = value;
            }
        }

        /// <summary>
        /// Gets or sets the patterns of files to copy from source location 
        /// to destination location.
        /// </summary>
        /// <value>List of file patterns to copy.</value>
        public IEnumerable<string> FilePatterns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to recursively copy files.
        /// </summary>
        /// <value>Indicates whether to recursively copy files.</value>
        public bool Recursive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only trigger transfer 
        /// events, and not actually transfer data.
        /// </summary>
        /// <value>Indicates whether we should transfer data or just show what would happen.</value>
        public bool FakeTransfer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to transfer blob snapshots.
        /// </summary>
        /// <value>Indicates whether we should transfer snapshots.</value>
        public bool TransferSnapshots
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to check MD5 hash after 
        /// finishing downloading blobs.
        /// </summary>
        /// <value>True if need to check MD5 after download; otherwise false.</value>
        public bool DownloadCheckMd5
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to delete source file
        /// after files transfer.
        /// </summary>
        /// <value>True if the source file will be deleted.</value>
        public bool MoveFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to keep local destination's
        /// last write time to be the same with source blob's. This value only can
        /// be set when downloading from Azure Storage.
        /// </summary>
        /// <value>True if to keep destination's last write time to be the same with source's.</value>
        public bool KeepLastWriteTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite destination 
        /// file if source file is newer.
        /// </summary>
        /// <value>Indicates whether we should skip copying files where the source is newer than the destination.</value>
        public bool ExcludeNewer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite destination 
        /// file if destination file is newer.
        /// </summary>
        /// <value>Indicates whether we should skip copying files where the destination file is newer than the source.</value>
        public bool ExcludeOlder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating attribute with which the file will be included in the transfer.
        /// </summary>
        /// <value>Indicates attribute with which the file will be included in the transfer.</value>
        public FileAttributes IncludedAttributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating attribute with which the file will be excluded in the transfer.
        /// </summary>
        /// <value>Indicates attribute with which the file will be excluded in the transfer.</value>
        public FileAttributes ExcludedAttributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to copy only files with 
        /// archive bit set.
        /// </summary>
        /// <value>Indicates to only copy files which have the archive bit set.</value>
        public bool OnlyFilesWithArchiveBit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the blob type to use for uploading to Azure Storage.
        /// </summary>
        /// <value>The type of blob to use for uploads to Azure Storage.</value>
        public BlobType UploadBlobType
        {
            get
            {
                return this.uploadBlobType;
            }

            set
            {
                if (Enum.IsDefined(typeof(BlobType), value))
                {
                    this.uploadBlobType = value;
                }
                else
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.UndefinedBlobTypeException,
                        value));
                }
            }
        }

        /// <summary>
        /// Gets or sets the callback method to be called before queueing the 
        /// actual file transfer. This callback allows callers to perform 
        /// various check and cancel queueing the specified file for transfer 
        /// if necessary.
        /// </summary>
        /// <value>Callback method to be called before the transfer is actually queued.</value>
        public BlobTransferBeforeQueueCallback BeforeQueueCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the restartable mode file transfers state.
        /// </summary>
        /// <value>Holds the file transfer status object used for recording restartable mode progress.</value>
        public BlobTransferFileTransferStatus FileTransferStatus
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether transfer is in restartable mode.
        /// </summary>
        /// <value>Indicates whether transfer is in restartable mode.</value>
        public bool RestartableMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the directory delimiter used in the blob names. 
        /// Downloaded blobs are organized into subdirectories based on the parts of their names. 
        /// Default: slash '/' .
        /// </summary>
        /// <value>Indicates the directory delimiter used in the blob names. </value>
        public char? Delimiter
        {
            get;

            set;
        }
    }
}
