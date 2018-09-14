//------------------------------------------------------------------------------
// <copyright file="IController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       The interface for FireAndRecollect to send the EndOfMessage signal
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System.ServiceModel;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Represents a batch of response messages requests via PullResponses
    /// </summary>
    [XmlType]
    public class BrokerResponseMessages
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [XmlElement]
        public bool EOM;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        [XmlElement]
        public XmlElement[] SOAPMessage;
    }

    /// <summary>
    /// The interface for FireAndRecollect to send the EndRequests signal
    /// </summary>
    [ServiceContract(Name = "IBrokerController", Namespace = "http://hpc.microsoft.com/brokercontroller/")]
    public interface IController
    {
        /// <summary>
        /// Flush request messages
        /// </summary>
        /// <param name="count">the number of request messages that to be flushed</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Flush(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutFlushMs);

        /// <summary>
        /// Indicate the end of the request messages
        /// </summary>
        /// <param name="count">the number of request messages that haveno't been flushed</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void EndRequests(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutEOMMs);

        /// <summary>
        /// Indicate the end of the request messages
        /// </summary>
        /// <param name="count">Remove the response corresponding to this client</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Purge(string clientid);

        /// <summary>
        /// Get broker client status
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <returns>returns the broker client status</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerClientStatus GetBrokerClientStatus(string clientId);

        /// <summary>
        /// Get number of committed requests in specified client
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <returns>returns number of committed requests in the client with specified client id</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        int GetRequestsCount(string clientId);

        /// <summary>
        /// Pull the responses
        /// </summary>
        /// <param name="action"></param>
        /// <param name="position"></param>
        /// <param name="count"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [XmlSerializerFormat]
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerResponseMessages PullResponses(string action, GetResponsePosition position, int count, string clientId);

        /// <summary>
        /// Get the response Azure Storage Queue
        /// </summary>
        /// <param name="action"></param>
        /// <param name="clientData"></param>
        /// <param name="resetToBegin"></param>
        /// <param name="count"></param>
        /// <param name="clientId"></param>
        /// <param name="sessionHash"></param>
        /// <param name="azureResponseQueueUri"></param>
        /// <param name="azureResponseBlobUri"></param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void GetResponsesAQ(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId, int sessionHash, out string azureResponseQueueUri, out string azureResponseBlobUri);


        /// <summary>
        /// Perform Ping action
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Ping();
    }
}
