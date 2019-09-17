//------------------------------------------------------------------------------
// <copyright file="ISessionLauncher.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The interface for session Launcher
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using System.Collections.Generic;

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
        /// Gets server version
        /// </summary>
        /// <returns>returns server version</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Version GetServerVersion();

        /// <summary>
        /// The async version of getting server version
        /// </summary>
        /// <param name="asyncState">indicating the callback</param>
        /// <param name="callback">indicating the async state</param>
        /// <returns>returns the async result</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetServerVersion(AsyncCallback callback, object asyncState);

        /// <summary>
        /// End the async version of getting server version
        /// </summary>
        /// <param name="result">indicating the async result</param>
        /// <returns>returns the server version</returns>
        Version EndGetServerVersion(IAsyncResult result);

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
        Task<SessionAllocateInfoContract> AllocateV5Async(SessionStartInfoContract info, string endpointPrefix);

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
        string[] Allocate(SessionStartInfoContract info, string endpointPrefix, out string sessionid, out string serviceVersion, out SessionInfoContract sessionInfo);

        /// <summary>
        /// The async version of allocating a new session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid</param>
        /// <returns>The async result</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAllocate(SessionStartInfoContract info, string endpointPrefix, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End the asyn operation of allocating
        /// </summary>
        /// <param name="sessionid">the session id</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <param name="result">async result</param>
        /// <returns>The results</returns>
        string[] EndAllocate(out string sessionid, out string serviceVersion, out SessionInfoContract sessionInfo, IAsyncResult result);

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
        Task<SessionAllocateInfoContract> AllocateDurableV5Async(SessionStartInfoContract info, string endpointPrefix);

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
        string[] AllocateDurable(SessionStartInfoContract info, string endpointPrefix, out string sessionid, out string serviceVersion, out SessionInfoContract sessionInfo);

        /// <summary>
        /// The async version of allocating a new session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid</param>
        /// <returns>The async result</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAllocateDurable(SessionStartInfoContract info, string endpointPrefix, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End the asyn operation of allocating
        /// </summary>
        /// <param name="sessionid">the session id</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <param name="result">async result</param>
        /// <returns>The results</returns>
        string[] EndAllocateDurable(out string sessionid, out string serviceVersion, out SessionInfoContract sessionInfo, IAsyncResult result);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionId">the session id</param>
        /// <returns>the Broker Launcher EPR</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<SessionInfoContract> GetInfoV5Async(string endpointPrefix, string sessionId);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="endpointPrefix">the endpoint prefix.</param>
        /// <param name="sessionId">the session id</param>
        /// <param name="useAad">if getting info of an AAD session</param>
        /// <returns>the Broker Launcher EPR</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<SessionInfoContract> GetInfoV5Sp1Async(string endpointPrefix, string sessionId, bool useAad);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="headnode">the headnode</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionId">the session id</param>
        /// <returns>the Broker Launcher EPR</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        SessionInfoContract GetInfo(string headnode, string endpointPrefix, string sessionId);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="headnode">the headnode</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionId">the session id</param>
        /// <returns>IAsyncResult instance</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetInfo(string headnode, string endpointPrefix, string sessionId, AsyncCallback callback, object state);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <returns>the Broker Launcher EPR</returns>
        SessionInfoContract EndGetInfo(IAsyncResult result);

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="sessionId">the session id</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task TerminateV5Async(string sessionId);

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="sessionId">the session id</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Terminate(string headnode, string sessionId);

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="sessionId">the session id</param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginTerminate(string headnode, string sessionId, AsyncCallback callback, object state);

        /// <summary>
        /// terminate a session.
        /// </summary>
        void EndTerminate(IAsyncResult result);
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
        /// Gets SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <returns>returns the value</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        string GetSOAConfiguration(string key);

        /// <summary>
        /// Begin method to get SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <param name="callback">indicating the callback</param>
        /// <param name="state">indicating the async state</param>
        /// <returns>returns the async result</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetSOAConfiguration(string key, AsyncCallback callback, object state);

        /// <summary>
        /// End method to get SOA configuration
        /// </summary>
        /// <param name="result">indicating the async result</param>
        /// <returns>returns the configuration value</returns>
        string EndGetSOAConfiguration(IAsyncResult result);

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