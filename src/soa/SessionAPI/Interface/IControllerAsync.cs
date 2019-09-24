// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System;
    using System.ServiceModel;

    using Microsoft.Telepathy.Session.Exceptions;

    /// <summary>
    /// The async version of IControllerAsync
    /// </summary>
    [ServiceContract(Name = "IBrokerController", Namespace = "http://hpc.microsoft.com/brokercontroller/")]
    public interface IControllerAsync
    {
        /// <summary>
        /// Flush messages
        /// </summary>
        /// <param name="count">the number of request messages that haven't been flushed</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Flush(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutFlushMs);

        /// <summary>
        /// Async version of Flush
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginFlush(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutFlushMs, System.AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of Flush
        /// </summary>
        void EndFlush(IAsyncResult result);

        /// <summary>
        /// Indicate the end of the request messages
        /// </summary>
        /// <param name="count">the number of request messages that haven't been flushed</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void EndRequests(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutEOMMs);

        /// <summary>
        /// Async version of EndRequests
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginEndRequests(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutEOMMs, System.AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of EndRequests
        /// </summary>
        void EndEndRequests(IAsyncResult result);

        /// <summary>
        /// Indicate the end of the request messages
        /// </summary>
        /// <param name="count">Remove the response corresponding to this client</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Purge(string clientid);

        /// <summary>
        /// Async version of Purge
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginPurge(string clientid, System.AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of Purge
        /// </summary>
        void EndPurge(IAsyncResult result);

        /// <summary>
        /// Get broker client status
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <returns>returns the broker client status</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerClientStatus GetBrokerClientStatus(string clientId);

        /// <summary>
        /// Async version of GetBrokerClientStatus
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetBrokerClientStatus(string clientId, System.AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of GetBrokerClientStatus
        /// </summary>
        BrokerClientStatus EndGetBrokerClientStatus(IAsyncResult result);

        /// <summary>
        /// Get number of committed requests in specified client
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <returns>returns number of committed requests in the client with specified client id</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        int GetRequestsCount(string clientId);

        /// <summary>
        /// Async version of GetRequestsCount
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetRequestsCount(string clientId, System.AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of GetRequestsCount
        /// </summary>
        int EndGetRequestsCount(IAsyncResult result);

        [XmlSerializerFormat]
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerResponseMessages PullResponses(string action, GetResponsePosition position, int count, string clientId);

        /// <summary>
        /// Async version of PullResponses
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginPullResponses(string action, GetResponsePosition position, int count, string clientId, System.AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of PullResponses
        /// </summary>
        BrokerResponseMessages EndPullResponses(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetResponsesAQ(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId, int sessionHash, System.AsyncCallback callback, object asyncState);

        void EndGetResponsesAQ(out string azureResponseQueueUri, out string azureResponseBlobUri, IAsyncResult result);

        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Ping();

        /// <summary>
        /// Async version of Purge
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginPing(AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of Purge
        /// </summary>
        void EndPing(IAsyncResult result);
    }
}
