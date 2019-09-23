// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;

    /// <summary>
    /// The interface for session Launcher
    /// </summary>
    [ServiceContract(Name = "ISessionLauncher", Namespace = "http://hpc.microsoft.com/sessionlauncher/")]
    internal interface ISessionLauncher
    {
        /// <summary>
        /// Gets server version
        /// </summary>
        /// <returns>returns server version</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<Version> GetServerVersionAsync();

        /// <summary>
        /// Allocate a new session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix.</param>
        /// <param name="sessionid">the sessionid</param>
        /// <param name="serviceVersion">the service verison</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>the Broker Launcher EPR, sorted by the preference.</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<SessionAllocateInfoContract> AllocateAsync(SessionStartInfoContract info, string endpointPrefix);

        /// <summary>
        /// Allocate a new session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix.</param>
        /// <param name="sessionid">the sessionid</param>
        /// <param name="serviceVersion">the service verison</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>the Broker Launcher EPR, sorted by the preference.</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<SessionAllocateInfoContract> AllocateDurableAsync(SessionStartInfoContract info, string endpointPrefix);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="endpointPrefix">the endpoint prefix.</param>
        /// <param name="sessionId">the session id</param>
        /// <returns>the Broker Launcher EPR</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<SessionInfoContract> GetInfoAsync(string endpointPrefix, string sessionId);

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="sessionId">the session id</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task TerminateAsync(string sessionId);

        /// <summary>
        /// Returns the versions for a specific service
        /// </summary>
        /// <param name="headNode">headnode of cluster to conect to </param>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <returns>Available service versions</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<Version[]> GetServiceVersionsAsync(string serviceName);

        /// <summary>
        /// Gets SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <returns>returns the value</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<string> GetSOAConfigurationAsync(string key);

        /// <summary>
        /// Gets SOA configurations
        /// </summary>
        /// <param name="keys">indicating the keys</param>
        /// <returns>returns the values</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<Dictionary<string, string>> GetSOAConfigurationsAsync(List<string> keys);

        /// <summary>
        /// Get cluster configuration info
        /// </summary>
        /// <returns>cluster info contract</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<ClusterInfoContract> GetClusterInfoAsync();
    }
}