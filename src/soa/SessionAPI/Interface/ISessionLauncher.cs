// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.Exceptions;

    /// <summary>
    /// The interface for session Launcher
    /// </summary>
    [ServiceContract(Name = "ISessionLauncher", Namespace = "http://hpc.microsoft.com/sessionlauncher/")]
    public interface ISessionLauncher
    {
        /// <summary>
        /// Gets server version
        /// </summary>
        /// <returns>returns server version</returns>
        [OperationContract]
        Task<Version> GetServerVersionAsync();

        /// <summary>
        /// Allocate a new session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>the Broker Launcher EPR, sorted by the preference.</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<SessionAllocateInfoContract> AllocateAsync(SessionStartInfoContract info, string endpointPrefix);

        /// <summary>
        /// Allocate a new session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>the Broker Launcher EPR, sorted by the preference.</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<SessionAllocateInfoContract> AllocateDurableAsync(SessionStartInfoContract info, string endpointPrefix);
            
        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
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
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <returns>Available service versions</returns>
        [OperationContract]
        Task<Version[]> GetServiceVersionsAsync(string serviceName);

        /// <summary>
        /// Returns the versions for a specific service
        /// </summary>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <returns>Available service versions</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Version[] GetServiceVersions(string serviceName);

#if HPCPACK
        /// <summary>
        /// Returns soa data server information
        /// </summary>
        /// <returns>Data server information</returns>
        [OperationContract]
        Task<DataServerInfo> GetDataServerInfoAsync();

        /// <summary>
        /// Returns soa data server information
        /// </summary>
        /// <returns>Data server information</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        DataServerInfo GetDataServerInfo();

        /// <summary>
        /// The async version of getting data server information
        /// </summary>
        /// <param name="asyncState">indicating the callback</param>
        /// <param name="callback">indicating the async state</param>
        /// <returns>returns the async result</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetDataServerInfo(AsyncCallback callback, object asyncState);

        /// <summary>
        /// End the async version of getting data server information
        /// </summary>
        /// <param name="result">indicating the async result</param>
        /// <returns>returns the data server information</returns>
        DataServerInfo EndGetDataServerInfo(IAsyncResult result);
#endif

        /// <summary>
        /// Gets SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <returns>returns the value</returns>
        [OperationContract]
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