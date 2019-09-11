//------------------------------------------------------------------------------
// <copyright file="ProxyFault.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Wrapper class that encapsulates exception that BrokerProxy encounters when communicating with service hosts
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.BrokerProxy
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// Wrapper class for exceptions that BrokerProxy encounters when communicating with service hosts.
    /// Note: as proxy of broker, BrokerProxy need send back to broker every non-trivial exception it encounters
    /// when talking to servce hosts.  The exception will be encapsualted into this ProxyFault class and carried
    /// back within a FaultException.
    /// </summary>
    [DataContract(Namespace = "http://hpc.microsoft.com/session/")]
    [Serializable]
    internal class ProxyFault
    {
        /// <summary>
        /// Gets the action for ProxyFault
        /// </summary>
        public const string Action = "http://hpc.microsoft.com/session/BrokerProxyFault";

        /// <summary>
        /// Proxy can't find the endpoint of the hpcservicehost
        /// </summary>
        public const int ProxyEndpointNotFound = (int)SOAFaultCodeCategory.BrokerProxyError | 0x0001;

        /// <summary>
        /// CommunicationException happens in the connection between proxy and the hpcservicehost
        /// </summary>
        public const int ProxyCommunicationException = (int)SOAFaultCodeCategory.BrokerProxyError | 0x0002;

        /// <summary>
        /// the fault code
        /// </summary>
        [DataMember]
        private int faultCode;

        /// <summary>
        /// the error message
        /// </summary>
        [DataMember]
        private string message;

        /// <summary>
        /// Initializes a new instance of the ProxyFault class
        /// </summary>
        /// <param name="faultCode">fault code</param>
        /// <param name="message">error message</param>
        public ProxyFault(int faultCode, string message)
        {
            this.faultCode = faultCode;
            this.message = message;
        }

        /// <summary>
        /// Gets the fault code
        /// </summary>
        public int FaultCode
        {
            get
            {
                return this.faultCode;
            }
        }

        /// <summary>
        /// Gets the message
        /// </summary>
        public string Message
        {
            get
            {
                return this.message;
            }
        }
    }
}
