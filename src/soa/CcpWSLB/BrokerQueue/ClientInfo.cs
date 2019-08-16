//------------------------------------------------------------------------------
// <copyright file="ClientInfo.cs" company="Microsoft">
//      Copyright (C)  Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      define the client info class for the persist queue to return the client infos for a session.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage
{
    /// <summary>
    /// define the client info.
    /// </summary>
    internal sealed class ClientInfo
    {
        #region private fields
        /// <summary>
        /// the client id.
        /// </summary>
        private string clientIdField;

        /// <summary>
        /// the total requests count.
        /// </summary>
        private long totalRequestsCountField;

        /// <summary>
        /// the total requests count that are processed.
        /// </summary>
        private long processedRequestsCountField;

        /// <summary>
        /// Stores the failed requests count
        /// </summary>
        private long failedRequestsCountField;

        /// <summary>
        /// Stores the user name of the client
        /// </summary>
        private string userName;
        #endregion

        /// <summary>
        /// Initializes a new instance of the ClientInfo class, 
        /// </summary>
        /// <param name="clientId">the client id.</param>
        /// <param name="totalRequestsCount">the request count of the total requests.</param>
        /// <param name="processedRequestsCount">the requests count of the processed requests.</param>
        public ClientInfo(string clientId, long totalRequestsCount, long processedRequestsCount)
        {
            this.clientIdField = clientId;
            this.totalRequestsCountField = totalRequestsCount;
            this.processedRequestsCountField = processedRequestsCount;
        }

        /// <summary>
        /// Initializes a new instance of the ClientInfo class, 
        /// </summary>
        /// <param name="clientId">the client id.</param>
        /// <param name="totalRequestsCount">the request count of the total requests.</param>
        /// <param name="processedRequestsCount">the requests count of the processed requests.</param>
        /// <param name="userName">the user name</param>
        public ClientInfo(string clientId, long totalRequestsCount, long processedRequestsCount, string userName)
        {
            this.clientIdField = clientId;
            this.totalRequestsCountField = totalRequestsCount;
            this.processedRequestsCountField = processedRequestsCount;
            this.userName = userName;
        }

        /// <summary>
        /// Initializes a new instance of the ClientInfo class, 
        /// </summary>
        /// <param name="clientId">the client id.</param>
        /// <param name="failed">indicating the failed requests count</param>
        /// <param name="userName">indicating the user name</param>
        public ClientInfo(string clientId, long failed, string userName)
        {
            this.clientIdField = clientId;
            this.failedRequestsCountField = failed;
            this.userName = userName;
        }

        /// <summary>
        /// Gets the client id.
        /// </summary>
        public string ClientId
        {
            get
            {
                return this.clientIdField;
            }
        }

        /// <summary>
        /// Gets or sets the total request count
        /// </summary>
        public long TotalRequestsCount
        {
            get { return this.totalRequestsCountField; }
            set { this.totalRequestsCountField = value; }
        }

        /// <summary>
        /// Gets or sets the number of the processed requests.
        /// </summary>
        public long ProcessedRequestsCount
        {
            get { return this.processedRequestsCountField; }
            set { this.processedRequestsCountField = value; }
        }

        /// <summary>
        /// Gets or sets the failed requests count
        /// </summary>
        public long FailedRequestsCount
        {
            get { return this.failedRequestsCountField; }
            set { this.failedRequestsCountField = value; }
        }

        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; }
        }
    }
}
