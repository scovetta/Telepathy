// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Represents information for broker recovery
    /// </summary>
    [DataContract(Namespace = "http://hpc.microsoft.com")]
    public class BrokerRecoverInfo
    {
        /// <summary>
        /// Stores the session start info
        /// </summary>
        private SessionStartInfoContract startInfo;

        /// <summary>
        /// Stores a value indicating whether the session is durable
        /// </summary>
        private bool durable;

        /// <summary>
        /// Stores session id
        /// </summary>
        private string sessionId;

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
        /// Stores the persist version
        /// </summary>
        private int? persistVersion;

        /// <summary>
        /// Stores the AAD user sid
        /// </summary>
        private string aadUserSid;

        /// <summary>
        /// Stores the AAD user name
        /// </summary>
        private string aadUserName;

        /// <summary>
        /// Get or set the <see cref="SessionStartInfoContract"/>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public SessionStartInfoContract StartInfo
        {
            get { return this.startInfo; }
            set { this.startInfo = value; }
        }

        /// <summary>
        /// Get or set a value indicates if use durable session
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public bool Durable
        {
            get { return this.durable; }
            set { this.durable = value; }
        }

        /// <summary>
        /// Get or set the session ID
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string SessionId
        {
            get { return this.sessionId; }
            set { this.sessionId = value; }
        }

        /// <summary>
        /// Get or set the purged failed number
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public long PurgedFailed
        {
            get { return this.purgedFailed; }
            set { this.purgedFailed = value; }
        }

        /// <summary>
        /// Get or set the purged processed number
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public long PurgedProcessed
        {
            get { return this.purgedProcessed; }
            set { this.purgedProcessed = value; }
        }

        /// <summary>
        /// Get or set the total purged number
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public long PurgedTotal
        {
            get { return this.purgedTotal; }
            set { this.purgedTotal = value; }
        }

        /// <summary>
        /// Get or set the persist version
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public int? PersistVersion
        {
            get { return this.persistVersion; }
            set { this.persistVersion = value; }
        }

        /// <summary>
        /// Gets or sets the AAD user SID
        /// </summary>
        [DataMember]
        public string AadUserSid
        {
            get { return this.aadUserSid; }
            set { this.aadUserSid = value; }
        }

        /// <summary>
        /// Gets or sets the AAD user name
        /// </summary>
        [DataMember]
        public string AadUserName
        {
            get { return this.aadUserName; }
            set { this.aadUserName = value; }
        }
    }
}
