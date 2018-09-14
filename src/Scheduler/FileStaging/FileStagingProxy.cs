//--------------------------------------------------------------------------
// <copyright file="FileStagingProxy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This is the generic definitition of the file staging proxy. Its
//     implementation may run in Azure or on-premise. This is an abstract
//     class and it cannot be instantiated.
// </summary>
//--------------------------------------------------------------------------

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("hpcazurelog")]

namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Hpc.Azure.Common;
    using Microsoft.Hpc.Management.FileTransfer;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    ///  This is the generic definitition of the file staging proxy. Its
    ///  implementation may run in Azure or on-premise. This is an abstract
    ///  class and it cannot be instantiated.
    /// </summary>
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public abstract class FileStagingProxy : IFileStagingRouter
    {
        /// <summary>
        /// Keep a dictionary to look up open channels based on the logical name of the destination
        /// </summary>
        protected Dictionary<string, GenericFileStagingClient> channels =
            new Dictionary<string, GenericFileStagingClient>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Lock on this object when changing the channels lookup table
        /// </summary>
        protected object channelLock = new object();

        /// <summary>
        /// Need a service host to run the WCF service used as a proxy
        /// </summary>
        protected ServiceHost serviceHost;

        /// <summary>
        /// Interval between two consequtive checks of CloudStorageAccount: 5 seconds
        /// </summary>
        private const int CheckCloudStorageAccountIntervalInMilliseconds = 5000;

        /// <summary>
        /// FileStagingBlobManager instance
        /// </summary>
        private FileStagingBlobManager azureBlobManager;

        /// <summary>
        /// Lock object for azureBlobManager
        /// </summary>
        private object lockAzureBlobManager = new object();

        /// <summary>
        /// Timer that checks CloudStorageAccount periodically
        /// </summary>
        private Timer checkCloudStorageAccountTimer;

        /// <summary>
        /// A flag indicating whether CloudStorageAccount is being checked
        /// </summary>
        private int isCheckingCloudStorageAccount;

        public FileStagingProxy()
        {
        }

        /// <summary>
        /// Implementations must start the service with this method call
        /// </summary>
        public virtual void Start()
        {
            if (this.checkCloudStorageAccountTimer == null)
            {
                this.checkCloudStorageAccountTimer = new Timer(this.CheckCloudStorageAccountTimerCallback, null, 0, CheckCloudStorageAccountIntervalInMilliseconds);
            }
        }

        /// <summary>
        /// Implementations must stop the service with this method call
        /// </summary>
        public virtual void Stop()
        {
            if (this.checkCloudStorageAccountTimer != null)
            {
                this.checkCloudStorageAccountTimer.Dispose();
                this.checkCloudStorageAccountTimer = null;
            }

            this.serviceHost.Close();
        }

        /// <summary>
        /// This service operation simply forwards a request to the targeted node
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        public abstract Message ProcessMessage(Message request);

        /// <summary>
        /// Check that user is an authenticated user.
        /// </summary>
        /// <returns>user name of caller</returns>
        public abstract string CheckUserAccess();

        public Message ProcessMessage1(Message request)
        {
            Trace.WriteLine("Processing ReadFile request.");
            return this.ProcessMessage(request);
        }

        public Message ProcessMessage2(Message request)
        {
            Trace.WriteLine("Processing WriteFile request.");
            return this.ProcessMessage(request);
        }

        public Message ProcessMessage3(Message request)
        {
            Trace.WriteLine("Processing DeleteFile request.");
            return this.ProcessMessage(request);
        }

        public Message ProcessMessage4(Message request)
        {
            Trace.WriteLine("Processing GetDirectories request.");
            return this.ProcessMessage(request);
        }

        public Message ProcessMessage5(Message request)
        {
            Trace.WriteLine("Processing DeleteDirectory request.");
            return this.ProcessMessage(request);
        }

        public Message ProcessMessage6(Message request)
        {
            Trace.WriteLine("Processing GetFiles request.");
            return this.ProcessMessage(request);
        }

        /// <summary>
        /// Copy a file from local to Azure blob
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        public Message CopyFileToBlob(Message request)
        {
            Trace.WriteLine("Processing CopyFileToBlob request.");
            return this.ProcessMessage(request);
        }

        /// <summary>
        /// Copy a file from Azure blob to local
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        public Message CopyFileFromBlob(Message request)
        {
            Trace.WriteLine("Processing CopyFileFromBlob request.");
            return this.ProcessMessage(request);
        }

        /// <summary>
        /// Copy a directory from local to Azure blob
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        public Message CopyDirectoryToBlob(Message request)
        {
            Trace.WriteLine("Processing CopyDirectoryToBlob request.");
            return this.ProcessMessage(request);
        }

        /// <summary>
        /// Copy a directory from Azure blob to local
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        public Message CopyDirectoryFromBlob(Message request)
        {
            Trace.WriteLine("Processing CopyDirectoryFromBlob request.");
            return this.ProcessMessage(request);
        }

        /// <summary>
        /// Returns an URL pointing to user's container on the intermediate blob
        /// storage, and an Azure SAS for accessing the container
        /// </summary>
        /// <param name="sas">Azure SAS for accessing user's container</param>
        /// <returns>an URL pointing to user's container on the intermediate blob storage</returns>
        public string GetContainerUrl(out string sas)
        {
            FileStagingBlobManager blobManager = this.GetFileStagingBlobManager();
            if (blobManager == null)
            {
                throw new FaultException<InternalFaultDetail>(
                        new InternalFaultDetail(Resources.Common_IntermediateBlobStorageMisConfigured, FileStagingErrorCode.IntermediateBlobStorageMisConfigured));
            }

            string userName = this.CheckUserAccess();
            return blobManager.GetContainerUrlForUser(userName, HpcContext.Get().GetClusterNameAsync().GetAwaiter().GetResult(), out sas);
        }

        /// <summary>
        /// Returns an Azure SAS that grants specified permissions to a blob under user's container
        /// </summary>
        /// <param name="blobName">target blob name</param>
        /// <param name="permissions">permissions to be granted by the SAS</param>
        /// <returns>an Azure SAS that grants specified permissions to the target blob</returns>
        public string GenerateSASForBlob(string blobName, SharedAccessBlobPermissions permissions)
        {
            return this.GenerateSASForBlobAsync(blobName, permissions).GetAwaiter().GetResult();
        }

        public async Task<string> GenerateSASForBlobAsync(string blobName, SharedAccessBlobPermissions permissions)
        {
            FileStagingBlobManager blobManager = this.GetFileStagingBlobManager();
            if (blobManager == null)
            {
                throw new FaultException<InternalFaultDetail>(
                        new InternalFaultDetail(Resources.Common_IntermediateBlobStorageMisConfigured, FileStagingErrorCode.IntermediateBlobStorageMisConfigured));
            }

            string userName = this.CheckUserAccess();
            var clusterName = await HpcContext.Get().GetClusterNameAsync();
            return blobManager.GenerateSASForBlob(userName, blobName, clusterName, permissions);
        }

        /// <summary>
        /// Keep the underlying channel alive
        /// </summary>
        public void KeepAlive()
        {
            return;
        }

        /// <summary>
        /// Implementations must return a channel to a file staging client based on its logical name
        /// </summary>
        /// <param name="logicalName">logical name of target node</param>
        /// <returns>a channel to the specified node</returns>
        protected abstract GenericFileStagingClient GetChannel(string logicalName);

        /// <summary>
        /// Gets the Azure storage account for accessing the Azure storage service
        /// </summary>
        /// <returns>Azure storage account for accessing the Azure storage service</returns>
        protected abstract CloudStorageAccount GetStorageAccount();

        /// <summary>
        /// Gets a value from the message header with the specified name
        /// </summary>
        /// <typeparam name="T">input header type</typeparam>
        /// <param name="headerName">header name</param>
        /// <returns>value of type T from the message header with the specified header name</returns>
        protected T GetInputFromHeader<T>(string headerName)
        {
            MessageHeaders incomingHeaders = System.ServiceModel.OperationContext.Current.IncomingMessageHeaders;
            T input = incomingHeaders.GetHeader<T>(headerName, FileStagingCommon.WcfHeaderNamespace);

            return input;
        }

        /// <summary>
        /// Get the FileStagingBlobManager instance
        /// </summary>
        /// <returns>the FileStagingBlobManager instance</returns>
        private FileStagingBlobManager GetFileStagingBlobManager()
        {
            if (this.azureBlobManager == null)
            {
                lock (this.lockAzureBlobManager)
                {
                    if (this.azureBlobManager == null)
                    {
                        CloudStorageAccount account = this.GetStorageAccount();
                        if (account != null)
                        {
                            this.azureBlobManager = new FileStagingBlobManager(account);
                        }
                    }
                }
            }

            return this.azureBlobManager;
        }

        /// <summary>
        /// Timer callback to check CloudStorageAccount update
        /// </summary>
        /// <param name="state">timer callback state</param>
        private void CheckCloudStorageAccountTimerCallback(object state)
        {
            if (1 == Interlocked.CompareExchange(ref this.isCheckingCloudStorageAccount, 1, 0))
            {
                return;
            }

            if (this.azureBlobManager != null)
            {
                lock (this.lockAzureBlobManager)
                {
                    if (this.azureBlobManager != null)
                    {
                        CloudStorageAccount account = this.GetStorageAccount();
                        if (account == null)
                        {
                            this.azureBlobManager = null;
                        }
                        else
                        {
                            // update storage account
                            this.azureBlobManager.StorageAccount = account;
                        }
                    }
                }
            }

            this.isCheckingCloudStorageAccount = 0;
        }

        public abstract byte[] GetAzureLocalLogFile(string instanceName, string fileName);
        public abstract HpcFileInfo[] GetAzureLocalLogFileList(string instanceName, DateTime startTime, DateTime endTime);
    }
}
