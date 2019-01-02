//------------------------------------------------------------------------------
// <copyright file="SessionInfoContract.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The information about a session
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Runtime.Serialization;

    using Microsoft.Hpc.Scheduler.Session.Data;

    /// <summary>
    /// The structure contains all the information about a session
    /// </summary>
    [Serializable]
    [DataContract(Name = "SessionInfo", Namespace = "http://hpc.microsoft.com/SessionLauncher")]
    public class SessionInfoContract
    {
        /// <summary>
        /// Session Id, also the service job id
        /// </summary>
        private int id;

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
        /// The client-broker heartbeat interval
        /// </summary>
        private int clientBrokerHeartbeatInterval;

        /// <summary>
        /// The client-broker heartbeat retry count
        /// </summary>
        private int clientBrokerHeartbeatRetryCount;

        /// <summary>
        /// Stores a value indicating whether it is an inprocess session
        /// </summary>
        private bool useInprocessBroker;

        /// <summary>
        /// Session's owner
        /// </summary>
        private string sessionOwner;

        /// <summary>
        /// Session's ACL
        /// </summary>
        private string[] sessionACL;

        /// <summary>
        /// Indicate if use AAD integrated authentication
        /// </summary>
        private bool useAad;

        /// <summary>
        /// Gets or sets the session Id
        /// </summary>
        [DataMember]
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets The launcher EPR
        /// </summary>
        [DataMember]
        public string BrokerLauncherEpr
        {
            get { return this.brokerLauncherEpr; }
            set { this.brokerLauncherEpr = value; }
        }

        /// <summary>
        /// Gets or sets the EPR to send request in RR way
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public string[] BrokerEpr
        {
            get { return this.brokerEpr; }
            set { this.brokerEpr = value; }
        }

        /// <summary>
        /// Gets or sets the EPR to control the broker behavir
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public string[] ControllerEpr
        {
            get { return this.controllerEpr; }
            set { this.controllerEpr = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the broker need secure binding
        /// </summary>
        [DataMember]
        public bool Secure
        {
            get { return this.secure; }
            set { this.secure = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the session is durable
        /// </summary>
        [DataMember]
        public bool Durable
        {
            get { return this.durable; }
            set { this.durable = value; }
        }

        /// <summary>
        /// Gets or sets the borker scheme for binding
        /// </summary>
        [DataMember]
        public TransportScheme TransportScheme
        {
            get { return this.scheme; }
            set { this.scheme = value; }
        }

        /// <summary>
        /// Gets or sets the server version
        /// </summary>
        [DataMember]
        public Version ServerVersion
        {
            get { return this.version; }
            set { this.version = value; }
        }

        /// <summary>
        /// Gets or sets the epr to GetResponse
        /// </summary>
        [DataMember]
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
        [DataMember]
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
        [DataMember]
        public int ServiceOperationTimeout
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
        [DataMember]
        public Version ServiceVersion
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
        /// Get or set the client-broker heartbeat interval
        /// </summary>
        [DataMember]
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
        [DataMember]
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
        [DataMember(IsRequired = false)]
        public bool UseInprocessBroker
        {
            get { return this.useInprocessBroker; }
            set { this.useInprocessBroker = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating owner of the session
        /// </summary>
        [DataMember(IsRequired = false)]
        public string SessionOwner
        {
            get { return this.sessionOwner; }
            set { this.sessionOwner = value; }
        }

        /// <summary>
        /// Gets or sets a value indiciating which users can use the session
        /// </summary>
        [DataMember(IsRequired = false)]
        public string[] SessionACL
        {
            get { return this.sessionACL; }
            set { this.sessionACL = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether use AAD integrated authentication
        /// </summary>
        [DataMember]
        public bool UseAad
        {
            get { return this.useAad; }
            set { this.useAad = value; }
        }
    }
}
