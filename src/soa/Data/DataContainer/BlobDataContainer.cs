//------------------------------------------------------------------------------
// <copyright file="BlobDataContainer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Azure blob based data container implementation
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using Microsoft.Hpc.Azure.DataMovement;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.Hpc.BrokerBurst;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Blob.Protocol;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;

    /// <summary>
    /// Azure blob based data container
    /// </summary>
    internal class BlobBasedDataContainer : IDataContainer
    {
        /// <summary>
        /// Check blob status interval: 5 seconds
        /// </summary>
        private const int CheckBlobStatusIntervalInMilliseconds = 5 * 1000;

        /// <summary>
        /// Default library directory path on on-premise installation and Azure VM role
        /// </summary>
        private const string HpcAssemblyDir = @"%CCP_HOME%bin";

        /// <summary>
        /// Default library directory path on Azure worker role
        /// </summary>
        private const string HpcAssemblyDir2 = @"%CCP_HOME%";

        /// <summary>
        /// Data movement library name
        /// </summary>
        private const string DataMovementAssemblyName = "Microsoft.Hpc.Azure.DataMovement.dll";

        /// <summary>
        /// Storage client library name
        /// </summary>
        private const string StorageClientAssemblyName = "Microsoft.WindowsAzure.Storage.dll";

        /// <summary>
        /// Default max concurrent connections allowed by a ServicePoint object
        /// </summary>
        private const int ServicePointManagerConnectionLimit = 48;

        /// <summary>
        /// Number of threads that concurrently download blobs from Azure storage
        /// </summary>
        private static int downloadBlobThreadCount;

        /// <summary>
        /// Mininum backoff interval in seconds when downloading blob for Azure Storage
        /// </summary>
        private static int downloadBlobMinBackoffInSeconds;

        /// <summary>
        /// Maximum backoff interval in seconds when downloading blob for Azure Storage
        /// </summary>
        private static int downloadBlobMaxBackoffInSeconds;

        /// <summary>
        /// Number of times to retry when downloading blob for Azure Storage
        /// </summary>
        private static int downloadBlobRetryCount;

        /// <summary>
        /// Timeout for downloading blob for Azure Storage
        /// </summary>
        private static int downloadBlobTimeoutInSeconds;

        /// <summary>
        /// container blob
        /// </summary>
        private CloudBlockBlob dataBlob;

        /// <summary>
        /// A flag indicating whether the container blob is being marked
        /// </summary>
        private int isDataBlobBeingMarked;

        /// <summary>
        /// Initializes static members of the BlobBasedDataContainer class
        /// </summary>
        static BlobBasedDataContainer()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;

            ServicePointManager.DefaultConnectionLimit = ServicePointManagerConnectionLimit;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            Configuration config = null;
            RetryManager.RetryOnceAsync(
                () => config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None),
                    TimeSpan.FromSeconds(1),
                    ex => ex is ConfigurationErrorsException)
                .GetAwaiter()
                .GetResult();
            Debug.Assert(config != null, "Configuration is not opened properly.");
            DataContainerConfiguration dataProviderConfig = DataContainerConfiguration.GetSection(config);
            downloadBlobThreadCount = dataProviderConfig.DownloadBlobThreadCount;
            downloadBlobMinBackoffInSeconds = dataProviderConfig.DownloadBlobMinBackOffInSeconds;
            downloadBlobMaxBackoffInSeconds = dataProviderConfig.DownloadBlobMaxBackOffInSeconds;
            downloadBlobRetryCount = dataProviderConfig.DownloadBlobRetryCount;
            downloadBlobTimeoutInSeconds = dataProviderConfig.DownloadBlobTimeoutInSeconds;

            TraceHelper.TraceSource.TraceEvent(
                TraceEventType.Information,
                0,
                "[BlobDataContainer] .static constructor: downloadBlobThreadCount={0}, downloadBlobMinBackoffInSeconds={1}, downloadBlobMaxBackoffInSeconds={2}, downloadBlobRetryCount={3}, downloadBlobTimeoutInSeconds={4}",
                downloadBlobThreadCount,
                downloadBlobMinBackoffInSeconds,
                downloadBlobMaxBackoffInSeconds,
                downloadBlobRetryCount,
                downloadBlobTimeoutInSeconds);
        }

        /// <summary>
        /// Initializes a new instance of the BlobBasedDataContainer class
        /// </summary>
        /// <param name="blobInfo">data blob information</param>
        public BlobBasedDataContainer(string blobInfo)
        {
            this.dataBlob = new CloudBlockBlob(new Uri(blobInfo));
        }

        /// <summary>
        /// Gets data container id
        /// </summary>
        public string Id
        {
            get
            {
                return this.dataBlob.Name;
            }
        }

        /// <summary>
        /// Returns a path that tells where the data is stored
        /// </summary>
        /// <returns>path telling where the data is stored</returns>
        public string GetStorePath()
        {
            return this.dataBlob.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Get the content Md5
        /// </summary>
        /// <returns>The base64 md5 string</returns>
        public string GetContentMd5()
        {
            WaitUntilTransferComplete(this.dataBlob);
            this.dataBlob.FetchAttributes();
            return this.dataBlob.Properties.ContentMD5;
        }

        /// <summary>
        /// Write a data item into data container and flush
        /// </summary>
        /// <param name="data">data content to be written</param>
        public void AddDataAndFlush(DataContent data)
        {
            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[BlobDataContainer] .AddDataAndFlush");

            using (MemoryStream ms = new MemoryStream())
            {
                // dump all data into a memory stream
                data.Dump(ms);

                // create timer that updates "CommonDataLastUpdateTime" metadata peoriodically
                Timer updateMetadataTimer = new Timer(
                    this.MarkBlobAsBeingUploaded,
                    null,
                    TimeSpan.FromMilliseconds(Constant.LastUpdateTimeUpdateIntervalInMilliseconds),
                    TimeSpan.FromMilliseconds(Constant.LastUpdateTimeUpdateIntervalInMilliseconds));

                // write data
                Exception transferException = null;
                try
                {
                    BlobTransferOptions transferOptions = new BlobTransferOptions
                    {
                        Concurrency = Environment.ProcessorCount * 8,
                    };

                    using (BlobTransferManager transferManager = new BlobTransferManager(transferOptions))
                    {
                        transferManager.QueueUpload(
                            this.dataBlob,
                            ms,
                            null,
                            delegate(object userData, double speed, double progress)
                            {
                                TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[BlobDataContainer] .AddDataAndFlush: progress={0}%", progress);
                            },
                            delegate(object userData, Exception ex)
                            {
                                if (ex != null)
                                {
                                    transferException = ex;
                                }
                            },
                            null);

                        transferManager.WaitForCompletion();
                    }
                }
                finally
                {
                    updateMetadataTimer.Dispose();
                }

                TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[BlobDataContainer] .AddDataAndFlush: data transfer done");

                DataException dataException = null;
                if (transferException != null)
                {
                    dataException = TranslateTransferExceptionToDataException(transferException);
                }

                try
                {
                    int errorCode = DataErrorCode.Success;
                    string errorMessage = string.Empty;
                    if (dataException != null)
                    {
                        errorCode = dataException.ErrorCode;
                        errorMessage = dataException.Message;
                    }

                    AzureBlobHelper.MarkBlobAsCompleted(this.dataBlob, errorCode.ToString(), errorMessage);
                }
                catch (StorageException ex)
                {
                    TraceHelper.TraceSource.TraceEvent(
                        TraceEventType.Error,
                        0,
                        "[BlobDataContainer] .AddDataAndFlush: failed to mark blob as completed. blob url={0}. error code={1}, exception={2}",
                        this.dataBlob.Uri.AbsoluteUri,
                        BurstUtility.GetStorageErrorCode(ex),
                        ex);
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceSource.TraceEvent(
                        TraceEventType.Error,
                        0,
                        "[BlobDataContainer] .AddDataAndFlush: failed to mark blob as completed. blob url={0}. Exception={1}",
                        this.dataBlob.Uri.AbsoluteUri,
                        ex);
                }

                if (dataException != null)
                {
                    throw dataException;
                }
            }
        }

        /// <summary>
        /// Gets data content from the data container
        /// </summary>
        /// <returns>data content in the data container</returns>
        public byte[] GetData()
        {
            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[BlobDataContainer] .GetData: blob url={0}", this.dataBlob.Uri.AbsoluteUri);

            WaitUntilTransferComplete(this.dataBlob);

            Exception transferException = null;
            byte[] data = new byte[this.dataBlob.Properties.Length];
            using (MemoryStream ms = new MemoryStream(data))
            {
                TraceHelper.TraceSource.TraceEvent(
                    TraceEventType.Verbose,
                    0,
                    "[BlobDataContainer] .GetData: download blob. threadCount={0} minBackoffInSeconds={1} maxBackoffInSeconds={2} retryCount={3} timeoutInSeconds={4}",
                    downloadBlobThreadCount,
                    downloadBlobMinBackoffInSeconds,
                    downloadBlobMaxBackoffInSeconds,
                    downloadBlobRetryCount,
                    downloadBlobTimeoutInSeconds);

                ExponentialRetry downloadRetryPolicy =
                    new ExponentialRetry(
                        TimeSpan.FromSeconds(downloadBlobMinBackoffInSeconds),
                        downloadBlobRetryCount);

                BlobRequestOptions blobRequestOptions = new BlobRequestOptions()
                {
                    RetryPolicy = downloadRetryPolicy,
                    MaximumExecutionTime = TimeSpan.FromSeconds(downloadBlobTimeoutInSeconds),
                };

                BlobTransferOptions options = new BlobTransferOptions();
                options.Concurrency = downloadBlobThreadCount;
                options.SetBlobRequestOptions(BlobRequestOperation.OpenRead, blobRequestOptions);

                using (BlobTransferManager transferManager = new BlobTransferManager(options))
                {
                    transferManager.QueueDownload(
                        this.dataBlob,
                        ms,
                        true,
                        null,
                        delegate(object userData, double speed, double progress)
                        {
                            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[BlobDataContainer] .GetData: progress={0}%", progress);
                        },
                        delegate(object userData, Exception ex)
                        {
                            if (ex != null)
                            {
                                transferException = ex;
                            }
                        },    
                        null);

                    transferManager.WaitForCompletion();
                }
            }

            TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[BlobDataContainer] .GetData: data transfer done");

            if (transferException == null)
            {
                return data;
            }

            TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[BlobDataContainer] .GetData: received exception={0}", transferException);
            throw TranslateTransferExceptionToDataException(transferException);
        }

        /// <summary>
        /// Delete the data container.
        /// </summary>
        public void DeleteIfExists()
        {
            try
            {
                this.dataBlob.DeleteIfExists();
            }
            catch (StorageException ex)
            {
                throw DataUtility.ConvertToDataException(ex);
            }
            catch (Exception ex)
            {
                throw new DataException(DataErrorCode.Unknown, ex);
            }
        }

        /// <summary>
        /// Check if the data container exists on data server or not
        /// </summary>
        /// <returns>true if the data container exists, false otherwise</returns>
        public bool Exists()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Load the assembly from some customized path, if it cannot be found automatically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A System.ResolveEventArgs that contains the event data.</param>
        /// <returns>targeted assembly</returns>
        private static Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
            {
                return null;
            }

            TraceHelper.TraceSource.TraceEvent(
                TraceEventType.Information,
                0,
                "[BlobDataContainer] .ResolveHandler: resolve assembly {0}.",
                args.Name);
            // Session API assembly may be installed in GAC, or %CCP_HOME%bin,
            // or "%CCP_HOME%"; while Microsoft.Hpc.Azure.DataMovement.dll
            // and Microsoft.WindowsAzure.StorageClient.dll
            // may be installed in %CCP_HOME%bin, or "%CCP_HOME%".  If they are
            // located at different places, we need load it from target folder
            // explicitly
            AssemblyName targetAssemblyName = new AssemblyName(args.Name);
            if (targetAssemblyName.Name.Equals(Path.GetFileNameWithoutExtension(DataMovementAssemblyName), StringComparison.OrdinalIgnoreCase))
            {
                return LoadAssembly(DataMovementAssemblyName);
            }
            else if (targetAssemblyName.Name.Equals(Path.GetFileNameWithoutExtension(StorageClientAssemblyName), StringComparison.OrdinalIgnoreCase))
            {
                return LoadAssembly(StorageClientAssemblyName);
            }

            return null;
        }

        /// <summary>
        /// Load assembly from default HPC installation directory
        /// </summary>
        /// <param name="assemblyName">name of the assembly to be loaded</param>
        /// <returns>targeted assembly</returns>
        private static Assembly LoadAssembly(string assemblyName)
        {
            TraceHelper.TraceSource.TraceEvent(
                TraceEventType.Information,
                0,
                "[BlobDataContainer] .LoadAssembly: will load assembly {0}.",
                assemblyName);

            string assemblyPath = Path.Combine(Environment.ExpandEnvironmentVariables(HpcAssemblyDir), assemblyName);
            if (!File.Exists(assemblyPath))
            {
                assemblyPath = Path.Combine(Environment.ExpandEnvironmentVariables(HpcAssemblyDir2), assemblyName);
            }

            TraceHelper.TraceSource.TraceEvent(
                TraceEventType.Information,
                0,
                "[BlobDataContainer] .LoadAssembly: try to load assembly {0}.",
                assemblyPath);
            try
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceSource.TraceEvent(
                    TraceEventType.Error,
                    0,
                    "[BlobDataContainer] .LoadAssembly: failed to load assembly {0}.  Exception={1}",
                    assemblyPath,
                    ex);
                return null;
            }
        }

        /// <summary>
        /// Translate transfer exception to data exception
        /// </summary>
        /// <param name="transferException">transfer exception</param>
        /// <returns>corresponding DataException</returns>
        private static DataException TranslateTransferExceptionToDataException(Exception transferException)
        {
            // if the TransferException is an AggregateException, translate only the first innerException
            AggregateException aggregateException = transferException as AggregateException;

            if (aggregateException != null)
            {
                if (aggregateException.InnerExceptions != null && aggregateException.InnerExceptions.Count > 0)
                {
                    transferException = aggregateException.InnerExceptions[0];
                }
                else if (aggregateException.InnerException != null)
                {
                    transferException = aggregateException.InnerException;
                }
            }

            TraceHelper.TraceSource.TraceEvent(
                TraceEventType.Error,
                0,
                "[BlobDataContainer] .TranslateTransferExceptionToDataException: exception={0}",
                transferException);

            BlobTransferException blobTransferException = transferException as BlobTransferException;
            StorageException storageException = null;
            if (blobTransferException != null)
            {
                storageException = blobTransferException.InnerException as StorageException;
            }

            if (storageException != null)
            {
                return DataUtility.ConvertToDataException(storageException);
            }

            WebException webException = transferException as WebException;
            if (webException != null)
            {
                return new DataException(GetDataErrorCode(webException), webException);
            }

            return new DataException(DataErrorCode.Unknown, transferException);
        }

        /// <summary>
        /// Map WebException to DataErrorCode
        /// </summary>
        /// <param name="e">the WebException to be interpreted</param>
        /// <returns>corresponding DataErrorCode</returns>
        private static int GetDataErrorCode(WebException e)
        {
            HttpWebResponse response = e.Response as HttpWebResponse;
            if (response == null)
            {
                return DataErrorCode.Unknown;
            }

            using (response)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return DataErrorCode.Success;
                    case HttpStatusCode.NotFound:
                        return DataErrorCode.DataClientDeleted;
                    case HttpStatusCode.Continue:
                        return DataErrorCode.DataRetry;
                    case HttpStatusCode.Unauthorized:
                        return DataErrorCode.DataNoPermission;
                    default:
                        return DataErrorCode.Unknown;
                }
            }
        }

        /// <summary>
        /// Wait until a blob is marked as completed, either succeeded or failed
        /// </summary>
        /// <param name="blob">data blob to be checked</param>
        private static void WaitUntilTransferComplete(CloudBlockBlob blob)
        {
            DateTime lastUpdateTime = DateTime.UtcNow;
            while (true)
            {
                if (RefreshBlobAttributes(blob))
                {
                    string strErrorCode;
                    if (!blob.Metadata.TryGetValue(Constant.MetadataKeyErrorCode, out strErrorCode))
                    {
                        strErrorCode = string.Empty;
                    }

                    string strException;
                    if (!blob.Metadata.TryGetValue(Constant.MetadataKeyException, out strException))
                    {
                        strException = string.Empty;
                    }

                    string strLastUpdateTime;
                    if (!blob.Metadata.TryGetValue(Constant.MetadataKeyLastUpdateTime, out strLastUpdateTime))
                    {
                        strLastUpdateTime = string.Empty;
                    }

                    if (!string.IsNullOrEmpty(strErrorCode))
                    {
                        TraceHelper.TraceSource.TraceEvent(
                            TraceEventType.Verbose,
                            0,
                            "[BlobDataContainer] .WaitUntilTransferComplete: success. blob name={0}, error code={1}, error message={2}",
                            blob.Name,
                            strErrorCode,
                            strException);

                        // error code is set. data transfer to blob is done
                        int errorCode = DataErrorCode.Success;
                        if (!int.TryParse(strErrorCode, out errorCode))
                        {
                            // error: "error code" metadata is malformed
                            TraceHelper.TraceSource.TraceEvent(
                                TraceEventType.Error,
                                0,
                                "[BlobDataContainer] .WaitUntilTransferComplete: blob name={0}, malformed error code={1}",
                                blob.Name,
                                strErrorCode);
                        }
                        else
                        {
                            if (errorCode != DataErrorCode.Success)
                            {
                                // throw DataException since error happens
                                if (errorCode == DataErrorCode.DataTransferToAzureFailed)
                                {
                                    throw new DataException(errorCode, string.Format(SR.DataTransferToAzureFailed, strException));
                                }
                                else
                                {
                                    throw new DataException(errorCode, strException);
                                }
                            }

                            break;
                        }
                    }
                    else
                    {
                        TraceHelper.TraceSource.TraceEvent(
                           TraceEventType.Verbose,
                           0,
                           "[BlobDataContainer] .WaitUntilTransferComplete: waiting... blob name={0}, last update time={1}",
                           blob.Name,
                           strLastUpdateTime);

                        // error code is not set yet. check if it is still in transfer
                        if (!string.IsNullOrEmpty(strLastUpdateTime))
                        {
                            DateTime newLastUpdateTime;
                            if (DateTime.TryParse(strLastUpdateTime, out newLastUpdateTime))
                            {
                                lastUpdateTime = newLastUpdateTime;
                            }
                            else
                            {
                                TraceHelper.TraceSource.TraceEvent(
                                    TraceEventType.Error,
                                    0,
                                    "[BlobDataContainer] .WaitUntilTransferComplete: blob name={0}, malformed last update time={1}",
                                    blob.Name,
                                    strLastUpdateTime);
                            }
                        }
                    }
                }

                if (DateTime.UtcNow.Subtract(lastUpdateTime).CompareTo(TimeSpan.FromSeconds(downloadBlobTimeoutInSeconds)) > 0)
                {
                    // "last update time" has not been updated for maxRetryCount times, throw timeout exception
                    throw new DataException(DataErrorCode.DataTransferToAzureFailed, new TimeoutException());
                }

                Thread.Sleep(CheckBlobStatusIntervalInMilliseconds);
            }
        }

        /// <summary>
        /// Refresh blob's attributes
        /// </summary>
        /// <param name="blob">target data blob</param>
        /// <returns>true if update blob's attributes successfully, false otherwise</returns>
        private static bool RefreshBlobAttributes(CloudBlockBlob blob)
        {
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (StorageException ex)
            {
                // Notice: Azure storage SDK 2.0 removes StorageServerException
                // and StorageClientException
                string errorCode = BurstUtility.GetStorageErrorCode(ex);

                TraceHelper.TraceSource.TraceEvent(
                    TraceEventType.Error,
                    0,
                    "[BlobDataContainer] .WaitUntilTransferComplete: failed to fetch blob attributes. blob name={0}, error code={1}, exception={2}",
                    blob.Name,
                    errorCode,
                    ex);

                if (errorCode.Equals(StorageErrorCodeStrings.ResourceNotFound, StringComparison.OrdinalIgnoreCase) ||
                    errorCode.Equals(BlobErrorCodeStrings.BlobNotFound, StringComparison.OrdinalIgnoreCase) ||
                    errorCode.Equals(BlobErrorCodeStrings.ContainerNotFound, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DataException(DataErrorCode.DataClientDeleted, ex);
                }

                throw DataUtility.ConvertToDataException(ex);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceSource.TraceEvent(
                    TraceEventType.Error,
                    0,
                    "[BlobDataContainer] .WaitUntilTransferComplete: failed to fetch blob attributes. blob name={0}, exception={1}",
                    blob.Name,
                    ex);

                throw new DataException(DataErrorCode.Unknown, ex);
            }
        }

        /// <summary>
        /// Timer callback to mark container data blob as being uploaded
        /// </summary>
        /// <param name="state">timer callback state</param>
        private void MarkBlobAsBeingUploaded(object state)
        {
            if (1 == Interlocked.CompareExchange(ref this.isDataBlobBeingMarked, 1, 0))
            {
                return;
            }

            try
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[BlobDataContainer] .MarkBlobAsBeingUploaded: uploading data");
                this.dataBlob.Metadata[Constant.MetadataKeyLastUpdateTime] = DateTime.UtcNow.ToString();
                this.dataBlob.SetMetadata();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceSource.TraceEvent(
                    TraceEventType.Error,
                    0,
                    "[BlobDataContainer] .MarkBlobAsBeingUploaded: failed to set lastUpdateTime for blob. Exception={0}",
                    ex);

                // besides StorageException, there is race condition that the timer callback
                // is triggered after "blob" object is disposed, thus ObjectDisposedException
                // is thrown. May just ignore the exception.
            }

            this.isDataBlobBeingMarked = 0;
        }
    }
}
