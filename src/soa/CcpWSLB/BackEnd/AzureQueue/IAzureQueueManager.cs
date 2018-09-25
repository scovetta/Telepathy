//-----------------------------------------------------------------------
// <copyright file="IAzureQueueManager.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     It is the interface of AzureQueueManager.
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.Collections.Concurrent;
    using System.ServiceModel.Channels;
    using System.Xml;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;

    /// <summary>
    /// It is the interface of AzureQueueManager.
    /// </summary>
    internal interface IAzureQueueManager
    {
        /// <summary>
        /// Gets or sets Azure storage connection string.
        /// </summary>
        string StorageConnectionString
        {
            set;
        }

        /// <summary>
        /// Gets the request queues.
        /// Key: Azure service name
        /// Value: Tuple of queue and blob container
        /// </summary>
        ConcurrentDictionary<string, Tuple<CloudQueue, CloudBlobContainer>> RequestStorage
        {
            get;
        }

        /// <summary>
        /// Create message retriever to monitor the response queue and gets
        /// messages from it.
        /// </summary>
        /// <param name="jobId">job Id</param>
        /// <param name="jobRequeueCount">job re-queue count</param>
        /// <returns>response storage name</returns>
        string Start(int jobId, int jobRequeueCount);

        /// <summary>
        /// Create request queue for specified Azure service.
        /// </summary>
        /// <param name="azureServiceName">azure service name</param>
        void CreateRequestStorage(string azureServiceName);

        /// <summary>
        /// Add QueueAsyncResult to the dictionary.
        /// </summary>
        /// <param name="result">async result</param>
        /// <param name="requestQueueName">request queue name</param>
        /// <param name="responseQueueName">response queue name</param>
        void AddQueueAsyncResult(QueueAsyncResult result, string requestQueueName, string responseQueueName);

        /// <summary>
        /// Remove QueueAsyncResult from the dictionary.
        /// </summary>
        /// <param name="messageId">message Id</param>
        void RemoveQueueAsyncResult(UniqueId messageId);

        /// <summary>
        /// Invoke the specified callback and delete it from the dictionary.
        /// </summary>
        /// <param name="asyncResult">it contains callback info</param>
        /// <param name="response">response message of the request</param>
        /// <param name="exception">exception occurred when process the request</param>
        void CompleteCallback(QueueAsyncResult asyncResult, Message response, Exception exception);

        /// <summary>
        /// Handle the invalid request queue.
        /// </summary>
        /// <param name="e">
        /// exception occurred when access the response queue
        /// </param>
        /// <param name="requestQueueName">
        /// request queue name
        /// </param>
        void HandleInvalidRequestQueue(StorageException e, string requestQueueName);

        /// <summary>
        /// Call response message's callback method.
        /// </summary>
        /// <param name="message">cloud queue message</param>
        /// <param name="response">WCF response message</param>
        /// <returns>message Id</returns>
        UniqueId ResponseCallback(CloudQueueMessage message, Message response);

        /// <summary>
        /// Invoke the cached callback for specified response queue, which is
        /// deleted when session is running.
        /// </summary>
        /// <param name="e">
        /// exception occurred when access the queue
        /// </param>
        void TriggerCallbackForInvalidResponseQueue(ResponseStorageException e);
    }
}
