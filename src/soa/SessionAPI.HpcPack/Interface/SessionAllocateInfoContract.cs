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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Properties;

    /// <summary>
    /// The structure contains all the information about a session
    /// </summary>
    [Serializable]
    [DataContract(Name = "SessionAllocateInfo", Namespace = "http://hpc.microsoft.com/SessionLauncher")]
    public class SessionAllocateInfoContract
    {

        /// <summary>
        /// Broker launcher eprs
        /// </summary>
        private string[] brokerlauncherEpr = null;

        /// <summary>
        /// Session Id, also the service job id
        /// </summary>
        private int id = 0;

        /// <summary>
        /// Version of the service created for the session
        /// </summary>
        private Version serviceVersion = null;

        /// <summary>
        /// Session info contract
        /// </summary>
        private SessionInfoContract sessionInfo = null;

        /// <summary>
        /// Gets or sets the EPR to send request in RR way
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public string[] BrokerLauncherEpr
        {
            get { return this.brokerlauncherEpr; }
            set { this.brokerlauncherEpr = value; }
        }

        /// <summary>
        /// Gets or sets the session Id
        /// </summary>
        [DataMember(IsRequired = false)]
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        /// <summary>
        /// Gets or sets the service version
        /// </summary>
        [DataMember(IsRequired = false)]
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
        /// Gets or sets the session info
        /// </summary>
        [DataMember(IsRequired = false)]
        public SessionInfoContract SessionInfo
        {
            get
            {
                return this.sessionInfo;
            }

            set
            {
                this.sessionInfo = value;
            }
        }
    }
}
