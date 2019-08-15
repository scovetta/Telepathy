//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="DataMoveTask.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Data move task
// </summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
#if HPCPACK
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Data.DataProvider;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Blob.Protocol;
    using Microsoft.Hpc.Azure.DataMovement;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;
    using TraceHelper = DataServiceTraceHelper;
    using System.IO;

    /// <summary>
    /// Data move task for moving data from source (local) to dest (Azure)
    /// </summary>
    internal class DataMoveTask : DisposableObject
    {
        /// <summary>
        /// The storage credentials for accessing the destination
        /// </summary>
        private StorageCredentials credentials;

        /// <summary>
        /// A flag indicating whether this data move task has been canceled
        /// </summary>
        private bool canceled;

        /// <summary>
        /// BlobTransferManager instance for uploading data from local to Azure Blob
        /// </summary>
        private BlobTransferManager transferManager;

        /// <summary>
        /// Synchronization object for this instance
        /// </summary>
        private object syncObj = new object();

        /// <summary>
        /// Initializes a new instance of the DataMoveTask class
        /// </summary>
        /// <param name="taskId">data move task id</param>
        /// <param name="source">local data source</param>
        /// <param name="dest">data destination on Azure</param>
        /// <param name="credentials">storage credentials for accessing the destination</param>
        public DataMoveTask(string taskId, string source, string dest, StorageCredentials credentials)
        {
            this.Id = taskId;
            this.Source = source;
            this.Destination = dest;
            this.credentials = credentials;
        }

        /// <summary>
        /// Gets id of the data move task
        /// </summary>
        public string Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the local data source
        /// </summary>
        public string Source
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the data destination on Azure
        /// </summary>
        public string Destination
        {
            get;
            private set;
        }

        /// <summary>
        /// Execute the data move task
        /// </summary>
        public void Run()
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataMoveTask].Run: source={0}, dest={1}", this.Source, this.Destination);

            bool isUpload = false;
            Exception exception = null;
            string blobPath = this.Source;
            if (BlobDataProvider.IsBlobDataContainerPath(this.Destination))
            {
                blobPath = this.Destination;
                isUpload = true;
            }

            CloudBlockBlob blob = new CloudBlockBlob(new Uri(blobPath), this.credentials);
            lock (this.syncObj)
            {
                if (this.canceled)
                {
                    // Do nothing if the task is canceled
                    TraceHelper.TraceEvent(
                        TraceEventType.Verbose,
                        "[DataMoveTask].Run: source={0}, dest={1}. skip canceled task",
                        this.Source,
                        this.Destination);

                    return;
                }

                BlobTransferOptions transferOptions = new BlobTransferOptions { Concurrency = Environment.ProcessorCount * 4, };
                this.transferManager = new BlobTransferManager(transferOptions);

                if (isUpload)
                {
                    this.transferManager.QueueUpload(
                        blob,
                        this.Source,
                        null,
                        null,
                        delegate(object userData, Exception ex)
                        {
                            if (ex != null)
                            {
                                exception = ex;
                            }
                        },
                        null);
                    this.transferManager.WaitForCompletion();
                }
                else
                {
                    try
                    {
                        using (FileStream fs = new FileStream(this.Destination, FileMode.Open, FileAccess.Write))
                        {
                            TraceHelper.TraceEvent(
                                TraceEventType.Verbose,
                                "[DataMoveTask].Run: source={0}, dest={1}. Start download",
                                this.Source,
                                this.Destination);

                            this.transferManager.QueueDownload(
                                blob,
                                fs,
                                true,
                                null,
                                null,
                                delegate(object userData, Exception ex)
                                {
                                    if (ex != null)
                                    {
                                        exception = ex;
                                    }
                                },
                                null);
                            this.transferManager.WaitForCompletion();
                        }
                    }
                    catch(Exception e)
                    {
                        TraceHelper.TraceEvent(
                            TraceEventType.Verbose,
                            "[DataMoveTask].Run: source={0}, dest={1}. get exception: {2}",
                            this.Source,
                            this.Destination, 
                            e);
                    }
                }
            }

            // transfer done
            int errorCode = DataErrorCode.Success;
            string errorMsg = string.Empty;
            if (exception != null)
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "[DataMoveTask].Run: source={0}, dest={1}, encounters exception={2}",
                    this.Source,
                    this.Destination,
                    exception);

                InterpretTransferException(exception, out errorCode, out errorMsg);
            }

            lock (this.syncObj)
            {
                if (this.canceled)
                {
                    TraceHelper.TraceEvent(
                        TraceEventType.Error,
                        "[DataMoveTask].Run: source={0}, dest={1}, tranfer is canceled",
                        this.Source,
                        this.Destination);

                    return;
                }

                try
                {
                    if (isUpload)
                    {
                        AzureBlobHelper.MarkBlobAsCompleted(blob, errorCode.ToString(), errorMsg);
                    }
                    else if(errorCode == DataErrorCode.Success)
                    {
                        AzureBlobHelper.MarkBlobAsSynced(blob);
                    }
                    TraceHelper.TraceEvent(
                        TraceEventType.Verbose,
                        "[DataMoveTask].Run: source={0}, dest={1}, transfer done. error code={2}, error message={3}",
                        this.Source,
                        this.Destination,
                        errorCode,
                        errorMsg);
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceEvent(
                        TraceEventType.Error,
                        "[DataMoveTask].Run: failed to set error code for blob {0}. Exception={1}",
                        this.Destination,
                        ex);
                }
            }
        }

        /// <summary>
        /// Cancel the data move task
        /// </summary>
        /// <param name="waitForCompletion">whether wait for cancel operation complete</param>
        public void Cancel(bool waitForCompletion)
        {
            lock (this.syncObj)
            {
                this.canceled = true;
                if (this.transferManager != null)
                {
                    this.transferManager.CancelWork();
                }
            }

            if (this.transferManager != null && waitForCompletion)
            {
                this.transferManager.WaitForCompletion();
            }
        }

        /// <summary>
        /// Refresh "lastUpdateTime" property of target blob
        /// </summary>
        public void Keepalive()
        {
            if (this.canceled)
            {
                // Do nothing if the task is canceled
                return;
            }

            TraceHelper.TraceEvent(
                TraceEventType.Verbose,
                "[DataMoveTask].Keepalive: source={0}, dest={1},",
                this.Source,
                this.Destination);

            if (BlobDataProvider.IsBlobDataContainerPath(this.Destination))
            {
                CloudBlockBlob destBlob = new CloudBlockBlob(new Uri(this.Destination), this.credentials);
                try
                {
                    BlobDataProvider.SetBlobAttributes(destBlob);
                }
                catch (Exception)
                {
                    // besides StorageException, there is race condition that the timer callback
                    // is triggered after "blob" object is disposed, thus ObjectDisposedException
                    // is thrown. May just ignore the exception.
                }
            }
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="disposing">indicating whether it is called directly or indirectly by user's code</param>
        protected override void Dispose(bool disposing)
        {
            if (this.transferManager != null)
            {
                this.transferManager.Dispose();
                this.transferManager = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Interpret exception returned by BlobTransferManager
        /// </summary>
        /// <param name="exception">exception to be interpreted</param>
        /// <param name="errorCode">result error code</param>
        /// <param name="errorMessage">result error message</param>
        private static void InterpretTransferException(Exception exception, out int errorCode, out string errorMessage)
        {
            AggregateException aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                if (aggregateException.InnerExceptions != null && aggregateException.InnerExceptions.Count > 0)
                {
                    exception = aggregateException.InnerExceptions[0];
                }
                else if (aggregateException.InnerException != null)
                {
                    exception = aggregateException.InnerException;
                }
            }

            BlobTransferException blobTransferException = exception as BlobTransferException;
            if (blobTransferException != null)
            {
                exception = blobTransferException.InnerException;
            }

            StorageException storageException = exception as StorageException;

            if (storageException != null)
            {
                errorCode = DataUtility.ConvertToDataServiceErrorCode(storageException);
            }
            else
            {
                errorCode = DataErrorCode.DataTransferToAzureFailed;
            }

            errorMessage = exception.ToString();
        }
    }
}
#endif