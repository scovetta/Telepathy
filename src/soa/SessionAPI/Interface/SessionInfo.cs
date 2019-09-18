//------------------------------------------------------------------------------
// <copyright file="SessionInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The information about a session
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;

    using Microsoft.Hpc.Scheduler.Session.Data;

    /// <summary>
    /// The structure contains all the information about a session
    /// </summary>
    public class SessionInfo : SessionInfoBase
    {
        /// <summary>
        /// Session Id, also the service job id
        /// </summary>
        private string id;

        /// <summary>
        /// The launcher EPR to open/close Broker
        /// </summary>
        private string brokerLauncherEpr;

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
        /// If the broker need secure binding
        /// </summary>
        private bool secure;

        /// <summary>
        /// If the session is durable
        /// </summary>
        private bool durable;

        /// <summary>
        /// The borker scheme for binding
        /// </summary>
        private TransportScheme scheme;

        /// <summary>
        /// The server version
        /// </summary>
        private Version version;

        /// <summary>
        /// the job state
        /// </summary>
        private JobState jobState;

        /// <summary>
        /// Service operation timeout
        /// </summary>
        private int serviceOperationTimeout;

        /// <summary>
        /// Version of the service created for the session
        /// </summary>
        private Version serviceVersion;

        /// <summary>
        /// Max message size
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
        /// Stores inprocess broker adapter
        /// </summary>
        private InprocBrokerAdapter inprocessBrokerAdapter;

        /// <summary>
        /// Stores a value indicating whether it is an inprocess session
        /// </summary>
        private bool useInprocessBroker;

        /// <summary>
        /// Stores a value indicating whether debug mode is enabled
        /// </summary>
        private bool debugModeEnabled;

        /// <summary>
        /// Session's owner
        /// </summary>
        private string sessionOwner;

        /// <summary>
        /// Session's ACL
        /// </summary>
        private string[] sessionACL;

        /// <summary>
        /// Azure request queue SAS Uri
        /// </summary>
        private string[] azureRequestQueueUris;

        /// <summary>
        /// Get or set Azure request queue SAS Uri
        /// </summary>
        public string[] AzureRequestQueueUris
        {
            get { return this.azureRequestQueueUris; }
            set { this.azureRequestQueueUris = value; }
        }

        /// <summary>
        /// Azure request blob container SAS Uri
        /// </summary>
        private string azureRequestBlobUri;

        /// <summary>
        /// Get or set Azure request blob container SAS Uri
        /// </summary>
        public string AzureRequestBlobUri
        {
            get { return azureRequestBlobUri; }
            set { azureRequestBlobUri = value; }
        }

        /// <summary>
        /// Whether the Azure storage queue/blob is used
        /// </summary>
        private bool? useAzureQueue;

        /// <summary>
        /// Get or set whether the Azure storage queue/blob is used
        /// </summary>
        public bool? UseAzureQueue
        {
            get { return useAzureQueue; }
            set { useAzureQueue = value; }
        }

        /// <summary>
        /// Gets or sets the session Id
        /// </summary>
        public override string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets The launcher EPR
        /// </summary>
        public string BrokerLauncherEpr
        {
            get { return this.brokerLauncherEpr; }
            set { this.brokerLauncherEpr = value; }
        }

        /// <summary>
        /// Gets or sets the EPR to send request in RR way
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public string[] BrokerEpr
        {
            get { return this.brokerEpr; }
            set { this.brokerEpr = value; }
        }

        /// <summary>
        /// Gets or sets the EPR to control the broker behavir
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public string[] ControllerEpr
        {
            get { return this.controllerEpr; }
            set { this.controllerEpr = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the broker need secure binding
        /// </summary>
        public override bool Secure
        {
            get { return this.secure; }
            set { this.secure = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the session is durable
        /// </summary>
        public bool Durable
        {
            get { return this.durable; }
            set { this.durable = value; }
        }

        /// <summary>
        /// Gets or sets the borker scheme for binding
        /// </summary>
        public override TransportScheme TransportScheme
        {
            get { return this.scheme; }
            set { this.scheme = value; }
        }

        /// <summary>
        /// Gets or sets the server version
        /// </summary>
        public Version ServerVersion
        {
            get { return this.version; }
            set { this.version = value; }
        }

        /// <summary>
        /// Gets or sets the epr to GetResponse
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public string[] ResponseEpr
        {
            get
            {
                return this.responseEpr;
            }
            set
            {
                this.responseEpr = value;
            }
        }

        /// <summary>
        /// Get or sets the job state.
        /// </summary>
        public JobState JobState
        {
            get
            {
                return this.jobState;
            }
            set
            {
                this.jobState = value;
            }
        }

        /// <summary>
        /// Gets or sets service operation timeout
        /// </summary>
        public override int ServiceOperationTimeout
        {
            get
            {
                return this.serviceOperationTimeout;
            }

            set
            {
                this.serviceOperationTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the service version
        /// </summary>
        public override Version ServiceVersion
        {
            get
            {
                return this.serviceVersion;
            }

            set
            {
                this.serviceVersion = value;
            }
        }

        /// <summary>
        /// The max message size allowed by settings
        /// </summary>
        public long MaxMessageSize
        {
            get
            {
                return this.maxMessageSize;
            }

            set
            {
                this.maxMessageSize = value;
            }
        }

        /// <summary>
        /// Get or set the client-broker heartbeat interval
        /// </summary>
        public int ClientBrokerHeartbeatInterval
        {
            get
            {
                return this.clientBrokerHeartbeatInterval;
            }

            set
            {
                this.clientBrokerHeartbeatInterval = value;
            }
        }

        /// <summary>
        /// Get or set the client-broker heartbeat retry count
        /// </summary>
        public int ClientBrokerHeartbeatRetryCount
        {
            get
            {
                return this.clientBrokerHeartbeatRetryCount;
            }

            set
            {
                this.clientBrokerHeartbeatRetryCount = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether it is an inprocess session
        /// </summary>
        public override bool UseInprocessBroker
        {
            get { return this.useInprocessBroker; }
            set { this.useInprocessBroker = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating owner of the session
        /// </summary>
        public string SessionOwner
        {
            get { return this.sessionOwner; }
            set { this.sessionOwner = value; }
        }

        /// <summary>
        /// Gets or sets a value indiciating which users can use the session
        /// </summary>
        public string[] SessionACL
        {
            get { return this.sessionACL; }
            set { this.sessionACL = value; }
        }

        /// <summary>
        /// Gets or sets the inprocess broker adapter
        /// </summary>
        internal InprocBrokerAdapter InprocessBrokerAdapter
        {
            get { return this.inprocessBrokerAdapter; }
            set { this.inprocessBrokerAdapter = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether debug mode is enabled
        /// </summary>
        internal bool DebugModeEnabled
        {
            get { return this.debugModeEnabled; }
            set { this.debugModeEnabled = value; }
        }

        /// <summary>
        /// Gets broker unique id (for heartbeat)
        /// </summary>
        internal string BrokerUniqueId { get; set; }

        /// <summary>
        /// Get or set whether the username and password windows client credential is used for the authentication
        /// </summary>
        public bool UseWindowsClientCredential { get; set; }

        internal string AzureControllerRequestQueueUri { get; set; }

        internal string AzureControllerResponseQueueUri { get; set; }

        public bool UseAzureStorage => this.TransportScheme == TransportScheme.AzureStorage || this.UseAzureQueue.GetValueOrDefault();
    }
}
