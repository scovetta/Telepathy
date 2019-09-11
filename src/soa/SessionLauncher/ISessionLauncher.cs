//------------------------------------------------------------------------------
// <copyright file="ISessionLauncher.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The interface for session Launcher
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    using System;
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Session;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    
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
        /// Gets server version
        /// </summary>
        /// <returns>returns server version</returns>
        //[OperationContract]
        //[FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        //Version GetServerVersion();

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
        Task<SessionAllocateInfoContract> AllocateV5Async(SessionStartInfoContract info, string endpointPrefix);

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
        string[] Allocate(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo);

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
        Task<SessionAllocateInfoContract> AllocateDurableV5Async(SessionStartInfoContract info, string endpointPrefix);

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
        string[] AllocateDurable(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="endpointPrefix">the endpoint prefix.</param>
        /// <param name="sessionId">the session id</param>
        /// <returns>the Broker Launcher EPR</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<SessionInfoContract> GetInfoV5Async(string endpointPrefix, int sessionId);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="endpointPrefix">the endpoint prefix.</param>
        /// <param name="sessionId">the session id</param>
        /// <param name="useAad">if getting info of an AAD session</param>
        /// <returns>the Broker Launcher EPR</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<SessionInfoContract> GetInfoV5Sp1Async(string endpointPrefix, int sessionId, bool useAad);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="endpointPrefix">the endpoint prefix.</param>
        /// <param name="sessionId">the session id</param>
        /// <returns>the Broker Launcher EPR</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        SessionInfoContract GetInfo(string headnode, string endpointPrefix, int sessionId);

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="sessionId">the session id</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task TerminateV5Async(int sessionId);

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="sessionId">the session id</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Terminate(string headnode, int sessionId);

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
        /// Returns the versions for a specific service
        /// </summary>
        /// <param name="headNode">headnode of cluster to conect to </param>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <returns>Available service versions</returns>
        //[OperationContract]
        //[FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        //Version[] GetServiceVersions(string serviceName);

        /// <summary>
        /// Gets SOA configurations
        /// </summary>
        /// <param name="keys">indicating the keys</param>
        /// <returns>returns the values</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<Dictionary<string, string>> GetSOAConfigurationsAsync(List<string> keys);

        /// <summary>
        /// Gets SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <returns>returns the value</returns>
        //[OperationContract]
        //[FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        //string GetSOAConfiguration(string key);

        /// <summary>
        /// Get cluster configuration info
        /// </summary>
        /// <returns>cluster info contract</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<ClusterInfoContract> GetClusterInfoAsync();
    }
}