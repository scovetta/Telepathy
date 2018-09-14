//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="DataRequestListener.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Listen and process DataRequests from DataProxy
// </summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using TraceHelper = DataServiceTraceHelper;
    using Hpc.Scheduler.Session.Internal.SessionLauncher;
    /// <summary>
    /// This class listens to data request queue, reads data requests
    /// from it, handles them to data service, and puts data responses
    /// into corresponding data response queue
    /// </summary>
    internal class DataRequestListener
    {
        /// <summary>
        /// Wait interval to retry start AzureQueueListener: 5 seconds
        /// </summary>
        private const int StartAzureQueueListenerTimerIntervalInMilliseconds = 5000;

        /// <summary>
        /// Maximum number of data requests that can be processed concurrently
        /// </summary>
        private const int MaxConcurrency = 1024;

        /// <summary>
        /// DataRequest serializer
        /// </summary>
        private static DataContractSerializer requestSerializer = new DataContractSerializer(typeof(DataRequest));

        /// <summary>
        /// DataResponse serializer
        /// </summary>
        private static DataContractSerializer responseSerializer = new DataContractSerializer(typeof(DataResponse));
        
        /// <summary>
        /// The cluster info
        /// </summary>
        private ClusterInfo clusterInfo;

        /// <summary>
        /// Listener of the data request queue
        /// </summary>
        private AzureQueueListener<DataRequest> requestQueueListener;

        /// <summary>
        /// The data service instance
        /// </summary>
        private DataService dataService;

        /// <summary>
        /// Client for accessing the Azure queue service
        /// </summary>
        private CloudQueueClient queueClient;

        /// <summary>
        /// Timer to start AzureQueueListener
        /// </summary>
        private Timer timerStartAzureQueueListener;

        /// <summary>
        /// Lock object for current DataRequestListener instance
        /// </summary>
        private object lockInstance = new object();

        /// <summary>
        /// Initializes a new instance of the DataRequestListener class
        /// </summary>
        /// <param name="clusterInfo">the cluster info</param>
        /// <param name="dataService">data service instance</param>
        public DataRequestListener(ClusterInfo clusterInfo, DataService dataService)
        {
            this.clusterInfo = clusterInfo;
            this.dataService = dataService;
        }

        /// <summary>
        /// Start listening data requests
        /// </summary>
        public void Start()
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataRequestListener] .Start request queue listener");

            lock (this.lockInstance)
            {
                if (this.timerStartAzureQueueListener == null)
                {
                    this.timerStartAzureQueueListener = new Timer(this.StartAzureQueueListener, null, 0, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// Stop listening data requests
        /// </summary>
        public void Stop()
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataRequestListener] .Stop request queue listener");

            lock (this.lockInstance)
            {
                if (this.timerStartAzureQueueListener != null)
                {
                    this.timerStartAzureQueueListener.Dispose();
                    this.timerStartAzureQueueListener = null;
                }

                if (this.requestQueueListener != null)
                {
                    this.requestQueueListener.Dispose();
                    this.requestQueueListener = null;
                }
            }
        }

        /// <summary>
        /// Check if a DataRequest is timed out
        /// </summary>
        /// <param name="request">data request to be checked</param>
        /// <returns>true if timed out, false otherwise</returns>
        private static bool IsTimedOut(DataRequest request)
        {
            return DateTime.UtcNow.CompareTo(request.CreateTime.AddMinutes(Constant.DataProxyOperationTimeoutInMinutes)) > 0;
        }
        
        /// <summary>
        /// Timer callback to start AzureQueueListener
        /// </summary>
        /// <param name="state">timer callback state</param>
        private void StartAzureQueueListener(object state)
        {
            Guid clusterId;
            string storageConnectionString;
            try
            {
                string clusterName = this.clusterInfo.Contract.ClusterName;
                if(!Guid.TryParse(this.clusterInfo.Contract.ClusterId, out clusterId))
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .StartAzureQueueListener: this.clusterInfo.Contract.ClusterId is not valid {0}", this.clusterInfo.Contract.ClusterId);
                    throw new ArgumentException("this.clusterInfo.Contract.ClusterId", "this.clusterInfo.Contract.ClusterId is not valid.");
                }
                TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataRequestListener] .StartAzureQueueListener: cluster name={0}, unique cluster id={1}", clusterName, clusterId);

                storageConnectionString = this.clusterInfo.Contract.AzureStorageConnectionString;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Warning, "[DataRequestListener] .StartAzureQueueListener: failed to get clutser propertiets, exception = {0}", ex);
                this.timerStartAzureQueueListener.Change(StartAzureQueueListenerTimerIntervalInMilliseconds, Timeout.Infinite);
                return;
            }

            CloudStorageAccount account;
            try
            {
                if (string.IsNullOrWhiteSpace(storageConnectionString))
                {
                    return;
                }

                account = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .StartAzureQueueListener: failed to parse storage connection string. exception = {0}", ex);
                return;
            }

            try
            {
                this.queueClient = new CloudQueueClient(account.QueueEndpoint, account.Credentials);

                // initialize CloudQueue instance
                string requestQueueName = AzureQueueListener<DataRequest>.GetDataRequestQueueName(clusterId.ToString());
                TraceHelper.TraceEvent(TraceEventType.Information, "[DataRequestListener] .StartAzureQueueListener: request queue name={0}", requestQueueName);

                CloudQueue requestQueue = this.queueClient.GetQueueReference(requestQueueName);
                bool ret = requestQueue.CreateIfNotExists();
                if (ret)
                {
                    TraceHelper.TraceEvent(TraceEventType.Information, "[DataRequestListener] .StartAzureQueueListener: request queue {0} created", requestQueueName);
                }

                this.requestQueueListener = new AzureQueueListener<DataRequest>(requestQueue, this.ReceiveDataRequest, this.HandleRequestQueueListenerException, MaxConcurrency);

                TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataRequestListener] .Start request queue listener");
                this.requestQueueListener.Listen();
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .StartAzureQueueListener: received exception = {0}", ex);
                this.timerStartAzureQueueListener.Change(StartAzureQueueListenerTimerIntervalInMilliseconds, Timeout.Infinite);
            }
        }

        /// <summary>
        /// DataRequest handler
        /// </summary>
        /// <param name="request">received data request</param>
        private void ReceiveDataRequest(DataRequest request)
        {
            TraceHelper.TraceEvent(
                TraceEventType.Verbose,
                "[DataRequestListener] .ReceiveDataRequest: received request, id={0}, type={1}, data client id = {2}",
                request.Id,
                request.Type,
                request.DataClientId);

            if (IsTimedOut(request))
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Warning,
                    "[DataRequestListener] .ReceiveDataRequest: request is timed out. id={0}, create time={1}",
                    request.Id,
                    request.CreateTime);

                // if the request is timed out, discard it.
                return;
            }

            switch (request.Type)
            {
                case DataRequestType.Open:
                    this.OpenDataClient(request);
                    break;
                case DataRequestType.Delete:
                    this.DeleteDataClient(request);
                    break;
                default:
                    TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .ReceiveDataRequest: unsupported request type. id={0}", request.Id);
                    DataResponse response = new DataResponse(request);
                    response.ErrorCode = DataErrorCode.DataFeatureNotSupported;
                    string errorMessage = string.Format("Unsupported DataRequestType {0}", request.Type);
                    response.Exception = new FaultException<DataFault>(new DataFault(DataErrorCode.DataFeatureNotSupported, errorMessage, request.DataClientId, string.Empty), errorMessage);
                    this.SendDataResponse(request.ResponseQueue, response);
                    break;
            }
        }

        /// <summary>
        /// Data request queue listener exception handler
        /// </summary>
        /// <param name="exception">exception handler</param>
        private void HandleRequestQueueListenerException(Exception exception)
        {
            TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .HandleRequestQueueListenerException: exception encountered when receiving request, {0}", exception);
        }

        /// <summary>
        /// Open a data client
        /// </summary>
        /// <param name="request">data request with type DataRequestType.Open</param>
        private void OpenDataClient(DataRequest request)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataRequestListener] .OpenDataClient: request id = {0}, data client id = {1}", request.Id, request.DataClientId);
            DataResponse response = new DataResponse(request);
            try
            {
                string userName = this.GetUserName(request.JobId, request.JobSecret);
                using (WindowsIdentity identity = new WindowsIdentity(DataService.SamAccountNameToUserPrincipalName(userName)))
                {
                    DataClientInfo info = this.dataService.OpenDataClientInternal(identity, request.DataClientId, DataLocation.AzureBlob);
                    response.ErrorCode = DataErrorCode.Success;

                    // return only blob path
                    response.Results = new object[] { info.PrimaryDataPath };
                }
            }
            catch (SecurityException ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .OpenDataClient: failed to open data client {0}, exception = {1}", request.DataClientId, ex);
                response.ErrorCode = DataErrorCode.DataNoPermission;
                response.Exception = new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, ex.Message, request.DataClientId, string.Empty), ex.Message);
            }
            catch (FaultException<DataFault> ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .OpenDataClient: failed to open data client {0}, exception = {1}", request.DataClientId, ex);
                response.ErrorCode = ex.Detail.Code;
                response.Exception = ex;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .OpenDataClient: failed to open data client {0}, exception = {1}", request.DataClientId, ex);
                response.ErrorCode = DataErrorCode.Unknown;
                response.Exception = new FaultException<DataFault>(new DataFault(DataErrorCode.Unknown, ex.Message, request.DataClientId, string.Empty), ex.Message);
            }

            this.SendDataResponse(request.ResponseQueue, response);
        }

        /// <summary>
        /// Delete a data client
        /// </summary>
        /// <param name="request">data request with type DataRequestType.Delete</param>
        private void DeleteDataClient(DataRequest request)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataRequestListener] .DeleteDataClient: request id = {0}, data client id = {1}", request.Id, request.DataClientId);

            DataResponse response = new DataResponse(request);
            try
            {
                string userName = this.GetUserName(request.JobId, request.JobSecret);
                using (WindowsIdentity identity = new WindowsIdentity(DataService.SamAccountNameToUserPrincipalName(userName)))
                {
                    this.dataService.DeleteDataClientInternal(identity, request.DataClientId, DataLocation.FileShareAndAzureBlob);
                    response.ErrorCode = DataErrorCode.Success;
                }
            }
            catch (SecurityException ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .DeleteDataClient: failed to delete data client {0}, exception = {1}", request.DataClientId, ex);
                response.ErrorCode = DataErrorCode.DataNoPermission;
                response.Exception = new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, ex.Message, request.DataClientId, string.Empty), ex.Message);
            }
            catch (FaultException<DataFault> ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .DeleteDataClient: failed to delete data client {0}, exception = {1}", request.DataClientId, ex);
                response.ErrorCode = ex.Detail.Code;
                response.Exception = ex;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .DeleteDataClient: failed to delete data client {0}, exception = {1}", request.DataClientId, ex);
                response.ErrorCode = DataErrorCode.Unknown;
                response.Exception = new FaultException<DataFault>(new DataFault(DataErrorCode.Unknown, ex.Message, request.DataClientId, string.Empty), ex.Message);
            }

            this.SendDataResponse(request.ResponseQueue, response);
        }

        /// <summary>
        /// Send DataResponse to the specified response queue
        /// </summary>
        /// <param name="queueName">response queue name</param>
        /// <param name="response">DataResponse to be sent</param>
        private void SendDataResponse(string queueName, DataResponse response)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataRequestListener] .SendDataResponse: request id = {0}, response queue = {1}", response.RequestId, queueName);

            CloudQueue queue = this.queueClient.GetQueueReference(queueName);
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    responseSerializer.WriteObject(ms, response);
                    CloudQueueMessage message = new CloudQueueMessage(ms.ToArray());
                    queue.AddMessage(message);
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataRequestListener] .SendDataResponse: request id={0}, response queue={1}. exception={2}", response.RequestId, queueName, ex);
            }
        }

        /// <summary>
        /// Get job user name
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <param name="jobSecret">jobSecret</param>
        /// <returns>user name of the specified job</returns>
        private string GetUserName(int jobId, string jobSecret)
        {
            return this.dataService.GetJobUserName(jobId, jobSecret);
        }
    }
}