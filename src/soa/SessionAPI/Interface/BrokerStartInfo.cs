//------------------------------------------------------------------------------
// <copyright file="BrokerStartInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Broker start info
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Broker start info
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com")]
    public class BrokerStartInfo
    {
        /// <summary>
        /// Stores the value indicating whether the broker is durable
        /// </summary>
        private bool durable;

        /// <summary>
        /// Stores the session id
        /// </summary>
        private int sessionId;

        /// <summary>
        /// Stores a value indicating whether the broker is started by attach
        /// </summary>
        private bool attached;

        /// <summary>
        /// headnode
        /// </summary>
        private string headnode;

        /// <summary>
        /// Stores the purged failed count
        /// </summary>
        private long purgedFailed;

        /// <summary>
        /// Stores the purged processed count
        /// </summary>
        private long purgedProcessed;

        /// <summary>
        /// Stores the purged total
        /// </summary>
        private long purgedTotal;

        /// <summary>
        /// Stores the job owner's SID
        /// </summary>
        private string jobOwnerSID;

        /// <summary>
        /// Stores the ACL string of the job template
        /// </summary>
        private string jobTemplateACL;

        /// <summary>
        /// Stores the path of the configuration file
        /// </summary>
        private string configurationFile;

        /// <summary>
        /// Stores the persist version
        /// </summary>
        private int persistVersion;

        /// <summary>
        /// Stores the network topology, default value is 0
        /// </summary>
        private int networkTopology = 0;

        /// <summary>
        /// Stores the value indicating whether automatic shrink is enabled, default value is Constant.AutomaticShrinkEnabledDefault (true)
        /// </summary>
        private bool automaticShrinkEnabled = Constant.AutomaticShrinkEnabledDefault;

        /// <summary>
        /// Stores the network prefix
        /// </summary>
        private string networkPrefix = String.Empty;

        /// <summary>
        /// Stores the flag indicating diag trace is enabled/disabled
        /// </summary>
        private bool enableDiagTrace;

        /// <summary>
        /// Stores the flag indicating FQDN is enabled/disabled
        /// </summary>
        private bool enableFQDN;

        /// <summary>
        /// Stores the flag indicating if use the soa burst based on https protocol.
        /// </summary>
        private bool httpsBurst;

        private string clusterName;

        private string clusterId;

        private string azureStorageConnectionString;

        /// <summary>
        /// Gets or sets a value indicating whether the broker is started by attach
        /// </summary>
        [DataMember]
        public bool Attached
        {
            get { return this.attached; }
            set { this.attached = value; }
        }

        /// <summary>
        /// Gets or sets the session id
        /// </summary>
        [DataMember]
        public int SessionId
        {
            get { return this.sessionId; }
            set { this.sessionId = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the broker is durable
        /// </summary>
        [DataMember]
        public bool Durable
        {
            get { return this.durable; }
            set { this.durable = value; }
        }

        /// <summary>
        /// Gets or sets the head node
        /// </summary>
        [DataMember]
        public string Headnode
        {
            get { return this.headnode; }
            set { this.headnode = value; }
        }

        /// <summary>
        /// Gets or sets the ACL string of the job template
        /// </summary>
        [DataMember]
        public string JobTemplateACL
        {
            get { return this.jobTemplateACL; }
            set { this.jobTemplateACL = value; }
        }

        /// <summary>
        /// Gets or sets the purged failed count
        /// </summary>
        [DataMember]
        public long PurgedFailed
        {
            get { return Interlocked.Read(ref this.purgedFailed); }
            set { this.purgedFailed = value; }
        }

        /// <summary>
        /// Gets or sets the purged processed count
        /// </summary>
        [DataMember]
        public long PurgedProcessed
        {
            get { return Interlocked.Read(ref this.purgedProcessed); }
            set { this.purgedProcessed = value; }
        }

        /// <summary>
        /// Gets or sets the purged total
        /// </summary>
        [DataMember]
        public long PurgedTotal
        {
            get { return Interlocked.Read(ref this.purgedTotal); }
            set { this.purgedTotal = value; }
        }

        /// <summary>
        /// Gets or sets the configuration file
        /// </summary>
        [DataMember]
        public string ConfigurationFile
        {
            get { return this.configurationFile; }
            set { this.configurationFile = value; }
        }

        /// <summary>
        /// Gets or sets the job owner's SID
        /// </summary>
        [DataMember]
        public string JobOwnerSID
        {
            get { return this.jobOwnerSID; }
            set { this.jobOwnerSID = value; }
        }

        /// <summary>
        /// Gets or sets the persist version
        /// </summary>
        [DataMember]
        public int PersistVersion
        {
            get { return this.persistVersion; }
            set { this.persistVersion = value; }
        }

        /// <summary>
        /// Gets or sets the network topology
        /// </summary>
        [DataMember]
        public int NetworkTopology
        {
            get { return this.networkTopology; }
            set { this.networkTopology = value; }
        }

        /// <summary>
        /// Gets or sets the value indicating whether automatic shrink is enabled
        /// </summary>
        [DataMember]
        public bool AutomaticShrinkEnabled
        {
            get { return this.automaticShrinkEnabled; }
            set { this.automaticShrinkEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the network prefix
        /// </summary>
        [DataMember]
        public string NetworkPrefix
        {
            get { return this.networkPrefix; }
            set { this.networkPrefix = value; }
        }

        /// <summary>
        /// Gets or sets the flag indicating diag trace is enabled/disabled
        /// </summary>
        [DataMember]
        public bool EnableDiagTrace
        {
            get { return this.enableDiagTrace; }
            set { this.enableDiagTrace = value; }
        }

        /// <summary>
        /// Gets or sets the flag indicating FQDN is enabled/disabled
        /// </summary>
        [DataMember]
        public bool EnableFQDN
        {
            get { return this.enableFQDN; }
            set { this.enableFQDN = value; }
        }

        /// <summary>
        /// Gets or sets the flag indicating if use the soa burst based on https protocol.
        /// </summary>
        [DataMember]
        public bool HttpsBurst
        {
            get { return this.httpsBurst; }
            set { this.httpsBurst = value; }
        }

        /// <summary>
        /// Get or set the cluster name
        /// </summary>
        [DataMember]
        public string ClusterName
        {
            get
            {
                return this.clusterName;
            }

            set
            {
                this.clusterName = value;
            }
        }

        /// <summary>
        /// Get or set the cluster ID
        /// </summary>
        [DataMember]
        public string ClusterId
        {
            get
            {
                return this.clusterId;
            }

            set
            {
                this.clusterId = value;
            }
        }

        /// <summary>
        /// Get or set the Azure storage connection string
        /// </summary>
        [DataMember]
        public string AzureStorageConnectionString
        {
            get
            {
                return this.azureStorageConnectionString;
            }

            set
            {
                this.azureStorageConnectionString = value;
            }
        }

        /// <summary>
        /// Get or set a value indicate if use AAD principle
        /// </summary>
        [DataMember]
        public bool UseAad { get; set; }

        /// <summary>
        /// Gets or sets the AAD user SID
        /// </summary>
        [DataMember]
        public string AadUserSid { get; set; }

        /// <summary>
        /// Gets or sets the AAD user name
        /// </summary>
        [DataMember]
        public string AadUserName { get; set; }

        [DataMember]
        public bool Standalone { get; set; }
    }
}
