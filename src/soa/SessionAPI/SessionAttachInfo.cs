//------------------------------------------------------------------------------
// <copyright file="SessionAttachInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       The structure as the parameter to attach a session
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para>Defines a set of values that an SOA client should use to attach to an existing session.</para>
    /// </summary>
    /// <remarks>
    ///   <para>You can attach to an existing interactive session if the service job for the 
    /// session is in the queued or running state, or you can attach to an existing durable session.</para>
    /// </remarks>
    public class SessionAttachInfo : SessionInitInfoBase
    {
        /// <summary>
        /// Stores the session id
        /// </summary>
        private int sessionId;

        /// <summary>
        /// Gets the session id
        /// </summary>
        public int SessionId
        {
            get { return this.sessionId; }
        }

        /// <summary>
        /// Stores the epr list
        /// This reference is not null only when debug mode is enabled
        /// </summary>
        private string[] eprList;

        /// <summary>
        /// Stores the password
        /// </summary>
        private string password;

        /// <summary>
        /// Stores a value indicating whether the session is inprocess
        /// </summary>
        private bool useInprocessBroker;

        /// <summary>
        /// Get or set whether the username and password windows client credential is used for the authentication
        /// </summary>
        public bool UseWindowsClientCredential { get; set; }

        // BUG 9149 - Removed unused public heartbeat properties. RTM documentation to be updated stating properties are for internal use only

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Session.SessionAttachInfo" /> class.</para>
        /// </summary>
        /// <param name="headNode">
        ///   <para>String that specifies the name of the head node for the cluster that hosts the session to which you want to attach an SOA client.</para>
        /// </param>
        /// <param name="id">
        ///   <para>Integer that specifies the identifier of the session to which you want to attach an SOA client.</para>
        /// </param>
        public SessionAttachInfo(string headNode, int id)
            : base(headNode)
        {
            this.eprList = Utility.TryGetEprList();
            this.sessionId = id;
            this.UseWindowsClientCredential = false;
            // Set the default transport scheme to Http if the head node is on Azure IaaS
            if (SoaHelper.IsSchedulerOnIaaS(headNode))
            {
                this.transportScheme = TransportScheme.Http;
            }
        }

        /// <summary>
        /// The transport scheme. Default value is NetTcp.
        /// </summary>
        private TransportScheme transportScheme = TransportScheme.NetTcp;

        /// <summary>
        ///   <para>Gets or sets the transport binding schemes used for the existing session to which you want the client to connect.</para>
        /// </summary>
        /// <value>
        ///   <para>A value from the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme" /> enumeration that specifies the transport binding scheme. The values should be either  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.NetTcp" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.WebAPI" />. Use 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.WebAPI" /> to attach to a session that the Windows Azure HPC Scheduler hosts.</para>
        /// </value>
        public override TransportScheme TransportScheme
        {
            get { return this.transportScheme; }
            set { this.transportScheme = value; }
        }

        /// <summary>
        ///   <para>Gets or sets the user name to use to connect the client to the existing  SOA session.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> that indicates the user name, in the form domain\username. </para>
        /// </value>
        /// <remarks>
        ///   <para>The user name is limited to 80 characters. If this parameter is 
        /// NULL, empty, or not valid, HPC searches the credentials cache for the credentials to use.  
        /// If the cache contains the credentials for a single user, those credentials are used. 
        /// However, if multiple credentials exist in the cache, the user is prompted for the credentials. </para> 
        ///   <para>You only need to set this property when you use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.WebAPI" /> transport scheme, which you use to connect to the Windows Azure HPC Scheduler.</para> 
        ///   <para>If the user under whose credentials the job runs differs from the job owner, the user under whose credentials the 
        /// job runs must be an administrator. If that user is not an administrator, an exception occurs because that user does not have  
        /// permission to read the job. The job owner is the user who runs the SOA client application. If you set the user 
        /// under whose credentials the job runs to be the same as the job owner, that user does not need to be an administrator.</para> 
        ///   <para>In Microsoft HPC Pack, if a user creates an unshared secure session that runs under 
        /// the credentials of a second user, only the first user can send requests to the session. The second user  
        /// cannot send requests to the session. A session that is not secure accepts requests from all clients. A secure 
        /// and shared session accepts requests based on the permissions that the access control list (ACL) in the job template specifies.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Username" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionAttachInfo.Password" />
        public override string Username { get; set; }

        /// <summary>
        ///   <para>Sets the password to use to connect the client to the existing SOA session.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> that specifies the password.</para>
        /// </value>
        /// <remarks>
        ///   <para>The password is limited to 127 characters. If this parameter is null or empty, 
        /// this method uses the cached password if one exists; otherwise, the user is prompted for the password.</para>
        ///   <para>You only need to set this property when you use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.WebAPI" /> transport scheme, which you use to connect to the Windows Azure HPC Scheduler.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Password" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionAttachInfo.Username" />
        public string Password
        {
            set { this.password = value; }
        }

        /// <summary>
        /// Gets the password internal
        /// </summary>
        public override string InternalPassword
        {
            get { return this.password; }
            set { this.password = value; }
        }

        public bool SavePassword { get; set; }

        /// <summary>
        /// Remove the password info.
        /// </summary>
        public void ClearCredential()
        {
            this.Password = null;
            this.SavePassword = false;
        }

        /// <summary>
        /// Gets the epr list
        /// </summary>
        internal string[] EprList
        {
            get { return this.eprList; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the session is inprocess
        /// </summary>
        public bool UseInprocessBroker
        {
            get
            {
                if (this.DebugModeEnabled)
                {
                    // Always use inprocess broker if debug mode enabled
                    return true;
                }

                return this.useInprocessBroker;
            }

            set
            {
                this.useInprocessBroker = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether debug mode is enabled
        /// </summary>
        public bool DebugModeEnabled
        {
            get { return this.eprList != null; }
        }

        /// <summary>
        /// The certificate
        /// </summary>
        private byte[] certificate;
        public byte[] Certificate
        {
            set { this.certificate = value; }
        }

        /// <summary>
        /// The pfx password
        /// </summary>
        private string pfxPassword;
        public string PfxPassword
        {
            set { this.pfxPassword = value; }
        }


    }
}