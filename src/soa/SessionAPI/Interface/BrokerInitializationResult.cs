//------------------------------------------------------------------------------
// <copyright file="BrokerInitializationResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Represents broker initilization result
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Represents broker initialization result
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com/BrokerLauncher")]
    public class BrokerInitializationResult
    {
        /// <summary>
        /// The EPR to send request in RR way
        /// </summary>
        private string[] brokerEpr;

        /// <summary>
        /// The EPR to control the broker behavir
        /// </summary>
        private string[] controllerEpr;

        /// <summary>
        /// The EPR to get response
        /// </summary>
        private string[] responseEpr;

        /// <summary>
        /// Service config's service operation timeout
        /// </summary>
        private int serviceOperationTimeoutMS;

        /// <summary>
        /// Service config's max message size
        /// </summary>
        private long maxMessageSize;

        /// <summary>
        /// The client-broker heartbeat interval
        /// </summary>
        private int clientBrokerHeartbeatInterval;

        /// <summary>
        /// The client-broker heartbeat retry count
        /// </summary>
        private int clientBrokerHeartbeatRetryCount;

        /// <summary>
        /// The request queue SAS Uri
        /// </summary>
        private string azureRequestQueueUri;

        /// <summary>
        /// The request blob container SAS Uri
        /// </summary>
        private string azureRequestBlobUri;

        /// <summary>
        /// Whether the Azure storage queue/blob is used
        /// </summary>
        private bool useAzureQueue;

        /// <summary>
        /// Get or set the broker EPRa array
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public string[] BrokerEpr
        {
            get { return this.brokerEpr; }
            set { this.brokerEpr = value; }
        }

        /// <summary>
        /// Get or set the controller EPRs array
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public string[] ControllerEpr
        {
            get { return this.controllerEpr; }
            set { this.controllerEpr = value; }
        }

        /// <summary>
        /// Get or set the reponese EPRs array
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public string[] ResponseEpr
        {
            get { return this.responseEpr; }
            set { this.responseEpr = value; }
        }

        /// <summary>
        /// Get or set the service operation time out
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public int ServiceOperationTimeout
        {
            get { return this.serviceOperationTimeoutMS; }
            set { this.serviceOperationTimeoutMS = value; }
        }

        /// <summary>
        /// Get or set the max message size
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public long MaxMessageSize
        {
            get { return this.maxMessageSize; }
            set { this.maxMessageSize = value; }
        }

        /// <summary>
        /// Get or set the client broker heart beat interval
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public int ClientBrokerHeartbeatInterval
        {
            get { return this.clientBrokerHeartbeatInterval; }
            set { this.clientBrokerHeartbeatInterval = value; }
        }

        /// <summary>
        /// Get or set the client broker heart beat retry count
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public int ClientBrokerHeartbeatRetryCount
        {
            get { return this.clientBrokerHeartbeatRetryCount; }
            set { this.clientBrokerHeartbeatRetryCount = value; }
        }

        /// <summary>
        /// Get or set the Azure request queue URI
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string AzureRequestQueueUri
        {
            get { return this.azureRequestQueueUri; }
            set { this.azureRequestQueueUri = value; }
        }

        /// <summary>
        /// Get or set the Azure request Blob URI
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string AzureRequestBlobUri
        {
            get { return this.azureRequestBlobUri; }
            set { this.azureRequestBlobUri = value; }
        }

        /// <summary>
        /// Get or set a value indicates if to use Azure Queue
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public bool UseAzureQueue
        {
            get { return this.useAzureQueue; }
            set { this.useAzureQueue = value; }
        }

        /// <summary>
        /// Get or set the broker unique ID
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember(IsRequired = false)]
        public string BrokerUniqueId { get; set; }

        /// <summary>
        /// Get or set a value indicates if supports message details
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember(IsRequired = false)]
        public bool SupportsMessageDetails { get; set; }

        /// <summary>
        /// Gets or sets the Azure controller request queue URI
        /// </summary>
        [DataMember(IsRequired = false)]
        public string AzureControllerRequestQueueUri { get; set; }

        /// <summary>
        /// Gets or sets the Azure controller response queue URI
        /// </summary>
        [DataMember(IsRequired = false)]
        public string AzureControllerResponseQueueUri { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("BrokerEpr = {0}\n", this.brokerEpr);
            sb.AppendFormat("ControllerEpr = {0}\n", this.controllerEpr);
            sb.AppendFormat("ResponseEpr = {0}\n", this.responseEpr);
            sb.AppendFormat("MaxMessageSize = {0}\n", this.maxMessageSize);
            sb.AppendFormat("ServiceOperationTimeout = {0}\n", this.serviceOperationTimeoutMS);
            sb.AppendFormat("ClientBrokerHeartbeatInterval = {0}\n", this.clientBrokerHeartbeatInterval);
            sb.AppendFormat("ClientBrokerHeartbeatRetryCount = {0}\n", this.clientBrokerHeartbeatRetryCount);
            sb.AppendFormat("SupportsMessageDetails = {0}\n", this.SupportsMessageDetails);
            sb.AppendFormat("UseAzureQueue = {0}\n", this.UseAzureQueue);
            return sb.ToString();
        }
    }
}
