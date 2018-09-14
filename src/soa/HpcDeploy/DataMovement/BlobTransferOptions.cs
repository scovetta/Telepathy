//------------------------------------------------------------------------------
// <copyright file="BlobTransferOptions.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      BlobTransferOptions class.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Hpc.Azure.DataMovement.BlobTransferCallbacks;

    /// <summary>
    /// BlobTransferOptions class.
    /// </summary>
    public class BlobTransferOptions
    {
        /// <summary>
        /// Stores the BlockSize to use for Windows Azure Storage transfers.
        /// </summary>
        private int blockSize;

        /// <summary>
        /// Stores customized BlobRequestOptions.
        /// </summary>
        private Dictionary<BlobRequestOperation, BlobRequestOptions> allBlobRequestOptions = new Dictionary<BlobRequestOperation, BlobRequestOptions>();

        /// <summary>
        /// Stores the base ClientRequestID.
        /// </summary>
        private string baseClientRequestID;

        /// <summary>
        /// Stores ClientRequestID.
        /// </summary>
        private string clientRequestId;

        /// <summary>
        /// How many work items to process concurrently.
        /// </summary>
        private int concurrency;

        /// <summary>
        /// Maximum amount of cache memory to use in bytes.
        /// </summary>
        private long maximumCacheSize;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="BlobTransferOptions" /> class.
        /// </summary>
        public BlobTransferOptions()
        {
            // setup default values.
            this.Concurrency = Environment.ProcessorCount * 8;
            this.BlockSize = BlobTransferConstants.DefaultBlockSize;

            GlobalMemoryStatusNativeMethods memStatus = new GlobalMemoryStatusNativeMethods();

            if (0 == memStatus.AvailablePhysicalMemory)
            {
                this.MaximumCacheSize = BlobTransferConstants.DefaultMemoryCacheSize;
            }
            else
            {
                this.MaximumCacheSize =
                    Math.Min(
                        (long)(memStatus.AvailablePhysicalMemory * BlobTransferConstants.MemoryCacheMultiplier),
                        BlobTransferConstants.MemoryCacheMaximum);
            }

            this.baseClientRequestID = BlobTransferOptions.GetDataMovementClientRequestID();
            this.clientRequestId = this.baseClientRequestID;
        }

        /// <summary>
        /// Gets or sets a value indicating how many work items to process 
        /// concurrently. Downloading or uploading a single blob can consist 
        /// of a large number of work items.
        /// </summary>
        /// <value>How many work items to process concurrently.</value>
        public int Concurrency
        {
            get
            {
                return this.concurrency;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.ConcurrentCountNotPositiveException));
                }

                this.concurrency = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how much memory we can cache
        /// during upload/download.
        /// </summary>
        /// <value>Maximum amount of cache memory to use in bytes.</value>
        public long MaximumCacheSize
        {
            get
            {
                return this.maximumCacheSize;
            }

            set
            {
                if (value < BlobTransferConstants.MaxBlockSize)
                {
                    throw new ArgumentException(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.SmallMemoryCacheSizeLimitationException,
                        Utils.BytesToHumanReadableSize(BlobTransferConstants.MaxBlockSize)));
                }

                this.maximumCacheSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the callback method to be called if the destination 
        /// file is already present to confirm whether the file should be 
        /// overwritten or not.
        /// If no callback is specified then all existing target files will 
        /// be overwritten.
        /// </summary>
        /// <value>Callback method to be called if destination file is already present.</value>
        public BlobTransferOverwritePromptCallback OverwritePromptCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the callback method to be called if the source 
        /// file has been changed since the last transfer to confirm 
        /// whether to retransfer the whole file or just fail it.
        /// If no callback is specified then all of transfers whose source file
        /// has been changed will be failed.
        /// </summary>
        /// <value>Callback method to be called if the source 
        /// file has been changed since the last transfer.</value>
        public BlobTransferRetransferModifiedFileCallback RetransferModifiedCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the BlockSize to use for Windows Azure Storage transfers.
        /// </summary>
        /// <value>BlockSize to use for Windows Azure Storage transfers.</value>
        public int BlockSize
        {
            get
            {
                return this.blockSize;
            }

            set
            {
                if (BlobTransferConstants.MinBlockSize > value || value > BlobTransferConstants.MaxBlockSize)
                {
                    string errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.BlockSizeOutOfRangeException,
                        Utils.BytesToHumanReadableSize(BlobTransferConstants.MinBlockSize),
                        Utils.BytesToHumanReadableSize(BlobTransferConstants.MaxBlockSize));

                    throw new ArgumentOutOfRangeException("value", value, errorMessage);
                }

                this.blockSize = value;
            }
        }

        /// <summary>
        /// Appends string to ClientRequestID string of DataMovement. The
        /// result string will be sent as ClientRequestID to server in 
        /// every connection.
        /// </summary>
        /// <param name="postfix">String to append.</param>
        public void AppendToClientRequestId(string postfix)
        {
            this.clientRequestId = this.baseClientRequestID + " " + postfix;
        }

        /// <summary>
        /// Gets the ClientRequestID to be sent to server.
        /// </summary>
        /// <returns>The ClientRequestID.</returns>
        public string GetClientRequestId()
        {
            return this.clientRequestId;
        }

        /// <summary>
        /// Sets the BlobRequestOptions to use when performing the specified operation.
        /// </summary>
        /// <param name="operation">Operation to perform.</param>
        /// <param name="blobRequestOptions">Blob request options to use.</param>
        public void SetBlobRequestOptions(BlobRequestOperation operation, BlobRequestOptions blobRequestOptions)
        {
            this.allBlobRequestOptions[operation] = blobRequestOptions;
        }

        /// <summary>
        /// Gets the BlobRequestOptions to use when performing the specified operation.
        /// </summary>
        /// <param name="operation">Blob request operation.</param>
        /// <returns>Blob request options to use.</returns>
        public BlobRequestOptions GetBlobRequestOptions(BlobRequestOperation operation)
        {
            BlobRequestOptions blobRequestOptions;
            if (this.allBlobRequestOptions.TryGetValue(operation, out blobRequestOptions))
            {
                return blobRequestOptions;
            }

            // return default BlobRequestOptions if user doesn't specify one.
            switch (operation)
            {
                case BlobRequestOperation.CreateContainer:
                    return BlobTransfer_BlobRequestOptionsFactory.CreateContainerRequestOptions;

                case BlobRequestOperation.ListBlobs:
                    return BlobTransfer_BlobRequestOptionsFactory.ListBlobsRequestOptions;

                case BlobRequestOperation.CreatePageBlob:
                    return BlobTransfer_BlobRequestOptionsFactory.CreatePageBlobRequestOptions;

                case BlobRequestOperation.Delete:
                    return BlobTransfer_BlobRequestOptionsFactory.DeleteRequestOptions;

                case BlobRequestOperation.GetPageRanges:
                    return BlobTransfer_BlobRequestOptionsFactory.GetPageRangesRequestOptions;

                case BlobRequestOperation.OpenRead:
                    return BlobTransfer_BlobRequestOptionsFactory.OpenReadRequestOptions;

                case BlobRequestOperation.PutBlock:
                    return BlobTransfer_BlobRequestOptionsFactory.PutBlockRequestOptions;

                case BlobRequestOperation.PutBlockList:
                    return BlobTransfer_BlobRequestOptionsFactory.PutBlockListRequestOptions;

                case BlobRequestOperation.DownloadBlockList:
                    return BlobTransfer_BlobRequestOptionsFactory.DownloadBlockListRequestOptions;

                case BlobRequestOperation.SetMetadata:
                    return BlobTransfer_BlobRequestOptionsFactory.SetMetadataRequestOptions;

                case BlobRequestOperation.FetchAttributes:
                    return BlobTransfer_BlobRequestOptionsFactory.FetchAttributesRequestOptions;

                case BlobRequestOperation.WritePages:
                    return BlobTransfer_BlobRequestOptionsFactory.WritePagesRequestOptions;

                case BlobRequestOperation.ClearPages:
                    return BlobTransfer_BlobRequestOptionsFactory.ClearPagesRequestOptions;

                case BlobRequestOperation.GetBlobReferenceFromServer:
                    return BlobTransfer_BlobRequestOptionsFactory.GetBlobReferenceFromServerRequestOptions;

                case BlobRequestOperation.StartCopyFromBlob:
                    return BlobTransfer_BlobRequestOptionsFactory.StartCopyFromBlobRequestOptions;

                default:
                    throw new ArgumentOutOfRangeException("operation");
            }
        }

        /// <summary>
        /// Gets the string to add into ClientRequestId.
        /// </summary>
        /// <returns>String to add into ClientRequestId.</returns>
        private static string GetDataMovementClientRequestID()
        {
            AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
            return assemblyName.Name + "/" + assemblyName.Version.ToString();
        }
    }
}
