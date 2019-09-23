// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using System;

    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;

    /// <summary>
    /// Represents a collection of information for a node
    /// </summary>
    internal class NodeInfo
    {
        /// <summary>
        /// Stores the name of the node
        /// </summary>
        private string name;

        /// <summary>
        /// Stores the full qualified domain name of the node
        /// </summary>
        private string fqdn;

        /// <summary>
        /// Stores the ssdl of the node
        /// </summary>
        private string ssdl;

        /// <summary>
        /// Stores the exception when getting those information
        /// </summary>
        private Exception exception;

        /// <summary>
        /// Initializes a new instance of the NodeInfo class
        /// </summary>
        /// <param name="name">indicating the node name</param>
        /// <param name="domainName">indicating the node domain name</param>
        /// <param name="ssdl">indicating the node ssdl</param>
        /// <param name="exception">indicating the exception</param>
        public NodeInfo(string name, string domainName, string ssdl, Exception exception)
        {
            this.name = name;

            if (string.IsNullOrEmpty(domainName))
            {
                this.fqdn = name;
            }
            else
            {
                this.fqdn = string.Format("{0}.{1}", name, domainName);
            }

            this.ssdl = ssdl;
            this.exception = exception;
        }

        /// <summary>
        /// Gets the name of the node
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Gets the full qualified domain name of the node
        /// </summary>
        public string FQDN
        {
            get
            {
                return this.fqdn;
            }
        }

        /// <summary>
        /// Gets the SSDL of the node
        /// </summary>
        public string SSDL
        {
            get
            {
                this.TryThrowException();
                return this.ssdl;
            }
        }

        /// <summary>
        /// Throw exception if available
        /// </summary>
        private void TryThrowException()
        {
            if (this.exception != null)
            {
                ThrowHelper.ThrowSessionFault(
                    SOAFaultCode.SessionLauncher_FailedToGetBrokerNodeSSDL,
                    SR.SessionLauncher_FailedToGetBrokerNodeSSDL,
                    this.name,
                    this.exception.ToString());
            }
        }
    }
}
