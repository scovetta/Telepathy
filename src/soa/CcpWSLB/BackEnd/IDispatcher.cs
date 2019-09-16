//-----------------------------------------------------------------------
// <copyright file="IDispatcher.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     It is interface of dispatcher.
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.WindowsAzure.Storage;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// It is interface of dispatcher.
    /// </summary>
    internal interface IDispatcher
    {
        /// <summary>
        /// Gets the dispatcher info
        /// </summary>
        DispatcherInfo Info
        {
            get;
        }

        /// <summary>
        /// Gets the value indicating if need to pass backend binding info.
        /// </summary>
        bool[] PassBindingFlags
        {
            get;
        }

        /// <summary>
        /// Gets the task id
        /// </summary>
        string TaskId
        {
            get;
        }

        /// <summary>
        /// Gets the callback of receive response.
        /// </summary>
        Func<IAsyncResult, Task> ProcessMessageCallback
        {
            get;
        }

        /// <summary>
        /// Stores the item array
        /// </summary>
        BrokerQueueItem[] items
        {
            get;
        }

        /// <summary>
        /// Gets the value indicating whether service initilization is completed.
        /// </summary>
        bool ServiceInitializationCompleted
        {
            get;
            set;
        }

        /// <summary>
        /// Close the dispatcher
        /// </summary>
        void Close();

        /// <summary>
        /// Close this dispatcher when dispatcher failed.
        /// </summary>
        /// <remarks>
        /// TODO: Should refactor this method name if we finally don't remove
        /// dispatcher class.
        /// </remarks>
        void CloseThis();

        /// <summary>
        /// This method handles general exception.
        /// </summary>
        Task HandleException(DateTime dispatchTime, int clientIndex, IService client, BrokerQueueItem item, Guid messageId, Exception e);

        /// <summary>
        /// This method handles StorageException. It occurs when add request
        /// message to the request queue in Azure storage.
        /// </summary>
        void HandleStorageException(DateTime dispatchTime, int clientIndex, IService client, BrokerQueueItem item, Guid messageId, StorageException e);

        /// <summary>
        /// This method handles EndpointNotFoundException. The exception can
        /// occur in on-premise soa, nettcp burst and https burst.
        /// </summary>
        void HandleEndpointNotFoundException(int clientIndex, IService client, BrokerQueueItem item, Guid messageId, EndpointNotFoundException e);

        /// <summary>
        /// Handle fault exception thrown by service code
        /// </summary>
        bool HandleFaultExceptionRetry(int clientIndex, BrokerQueueItem item, Message reply, DateTime dispatchTime, out bool preemption);

        /// <summary>
        /// Handle service instance failure
        /// </summary>
        void HandleServiceInstanceFailure(SessionFault sessionFault);

        /// <summary>
        /// Get next request from broker queue.
        /// </summary>
        /// <param name="serviceClientIndex">index of the service client through which the request will be sent</param>
        void GetNextRequest(int serviceClientIndex);

        /// <summary>
        /// Decrease processingRequests counter and check if all outstanding calls are completed.
        /// </summary>
        void DecreaseProcessingCount();

        /// <summary>
        /// Notify that this dispatcher is connected to service instance
        /// </summary>
        void OnServiceInstanceConnected(object state);

        bool CleanupClient(IService client);
    }
}
