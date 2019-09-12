//------------------------------------------------------------------------------
// <copyright file="SessionLauncherClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Service client base to connect the session launcher in headnode
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;


    /// <summary>
    /// Service client base to connect the session launcher in headnode
    /// </summary>
    public class SessionLauncherClientBase : ClientBase<ISessionLauncher>, ISessionLauncher
    {
        /// <summary>
        /// the default endpoint prefix.
        /// </summary>
        protected const string defaultEndpointPrefix = "net.tcp://";

        /// <summary>
        /// the https endpoint prefix.
        /// </summary>
        protected const string httpsEndpointPrefix = "https://";

        /// <summary>
        /// Initializes a new instance of the SessionLauncherClient class.
        /// </summary>
        /// <param name="binding">indicating the binding</param>
        /// <param name="uri">the session launcher EPR</param>
        public SessionLauncherClientBase(Binding binding, EndpointAddress address)
            : base(binding, address)
        {
        }

        /// <summary>
        /// Gets server version
        /// </summary>
        /// <returns>returns server version</returns>
        public async Task<Version> GetServerVersionAsync()
        {
            return await this.Channel.GetServerVersionAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets server version
        /// </summary>
        /// <returns>returns server version</returns>
        public Version GetServerVersion()
        {
            return this.Channel.EndGetServerVersion(this.Channel.BeginGetServerVersion(null, null));
        }

        /// <summary>
        /// The async version of getting server version
        /// </summary>
        /// <param name="asyncState">indicating the callback</param>
        /// <param name="callback">indicating the async state</param>
        /// <returns>returns the async result</returns>
        public IAsyncResult BeginGetServerVersion(AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginGetServerVersion(callback, asyncState);
        }

        /// <summary>
        /// End the async version of getting server version
        /// </summary>
        /// <param name="result">indicating the async result</param>
        /// <returns>returns the server version</returns>
        public Version EndGetServerVersion(IAsyncResult result)
        {
            return this.Channel.EndGetServerVersion(result);
        }

        /// <summary>
        /// Allocate a session and get a list of brokerlauncher EPR
        /// </summary>
        /// <param name="info">Session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid returns</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>The EPRs of the broker launchers</returns>
        public async Task<SessionAllocateInfoContract> AllocateDurableAsync(SessionStartInfoContract info, string endpointPrefix)
        {
            return await this.Channel.AllocateDurableAsync(info, endpointPrefix).ConfigureAwait(false);
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            //IAsyncResult result = this.Channel.BeginAllocateDurable(info, endpointPrefix, null, null);
            //return this.Channel.EndAllocateDurable(result);
        }


        /// <summary>
        /// Allocate a session and get a list of brokerlauncher EPR
        /// </summary>
        /// <param name="info">Session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid returns</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>The EPRs of the broker launchers</returns>
        public string[] AllocateDurable(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginAllocateDurable(info, endpointPrefix, null, null);
            return this.Channel.EndAllocateDurable(out sessionid, out serviceVersion, out sessionInfo, result);
        }


        /// <summary>
        /// The async version of allocating a new session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="callback">The async callback</param>
        /// <param name="asyncState">async state object</param>
        /// <returns>The async result</returns>
        public IAsyncResult BeginAllocateDurable(SessionStartInfoContract info, string endpointPrefix, AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginAllocateDurable(info, endpointPrefix, callback, asyncState);
        }

        /// <summary>
        /// End the async opeartion of allocating
        /// </summary>
        /// <param name="sessionid">the session id</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <param name="result">the async result</param>
        /// <returns>the canidate broker launchers' eprs</returns>
        public string[] EndAllocateDurable(out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo, IAsyncResult result)
        {
            return this.Channel.EndAllocateDurable(out sessionid, out serviceVersion, out sessionInfo, result);
        }


        /// <summary>
        /// Allocate a session and get a list of brokerlauncher EPR
        /// </summary>
        /// <param name="info">Session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid returns</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>The EPRs of the broker launchers</returns>
        public async Task<SessionAllocateInfoContract> AllocateAsync(SessionStartInfoContract info, string endpointPrefix)
        {
            return await this.Channel.AllocateAsync(info, endpointPrefix).ConfigureAwait(false);
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            //IAsyncResult result = this.Channel.BeginAllocate(info, endpointPrefix, null, null);
            //return this.Channel.EndAllocate(result);
        }

        /// <summary>
        /// Allocate a session and get a list of brokerlauncher EPR
        /// </summary>
        /// <param name="info">Session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionid">the sessionid returns</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>The EPRs of the broker launchers</returns>
        public string[] Allocate(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginAllocate(info, endpointPrefix, null, null);
            return this.Channel.EndAllocate(out sessionid, out serviceVersion, out sessionInfo, result);
        }

        /// <summary>
        /// The async version of allocating a new session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="callback">The async callback</param>
        /// <param name="asyncState">async state object</param>
        /// <returns>The async result</returns>
        public IAsyncResult BeginAllocate(SessionStartInfoContract info, string endpointPrefix, AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginAllocate(info, endpointPrefix, callback, asyncState);
        }

        /// <summary>
        /// End the async opeartion of allocating
        /// </summary>
        /// <param name="sessionid">the session id</param>
        /// <param name="serviceVersion">the service version</param>
        /// <param name="sessionInfo">the session info</param>
        /// <param name="result">the async result</param>
        /// <returns>the canidate broker launchers' eprs</returns>
        public string[] EndAllocate(out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo, IAsyncResult result)
        {
            return this.Channel.EndAllocate(out sessionid, out serviceVersion, out sessionInfo, result);
        }

        /// <summary>
        /// Get the session informaiton for one session
        /// </summary>
        /// <param name="headnode">the headnode name</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionId">the session id</param>
        /// <returns>The Session Information</returns>
        public async Task<SessionInfoContract> GetInfoAsync(string endpointPrefix, int sessionId)
        {
            return await this.Channel.GetInfoAsync(endpointPrefix, sessionId).ConfigureAwait(false);
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            //IAsyncResult result = this.Channel.BeginGetInfo(headnode, endpointPrefix, sessionId, null, null);
            //return this.Channel.EndGetInfo(result);
        }

        /// <inheritdoc />
        public async Task<SessionInfoContract> GetInfoAadAsync(string endpointPrefix, int sessionId, bool useAad)
        {
            return await this.Channel.GetInfoAadAsync(endpointPrefix, sessionId, useAad).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the session informaiton for one session
        /// </summary>
        /// <param name="headnode">the headnode name</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionId">the session id</param>
        /// <returns>The Session Information</returns>
        public SessionInfoContract GetInfo(string headnode, string endpointPrefix, int sessionId)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginGetInfo(headnode, endpointPrefix, sessionId, null, null);
            return this.Channel.EndGetInfo(result);
        }

        /// <summary>
        /// Get the session informaiton for one session
        /// </summary>
        /// <param name="headnode">the headnode name</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionId">the session id</param>
        /// <returns>IAsyncResult instance</returns>
        public IAsyncResult BeginGetInfo(string headnode, string endpointPrefix, int sessionId, AsyncCallback callback, object state)
        {
            return this.Channel.BeginGetInfo(headnode, endpointPrefix, sessionId, callback, state);
        }

        /// <summary>
        /// Get the session informaiton for one session
        /// </summary>
        /// <returns>The Session Information</returns>
        public SessionInfoContract EndGetInfo(IAsyncResult result)
        {
            return this.Channel.EndGetInfo(result);
        }

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="sessionId">the session id</param>
        public async Task TerminateAsync(int sessionId)
        {
            await this.Channel.TerminateAsync(sessionId).ConfigureAwait(false);
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            //IAsyncResult result = this.Channel.BeginTerminate(headnode, sessionId, null, null);
            //this.Channel.EndTerminate(result);
        }

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="sessionId">the session id</param>
        public void Terminate(string headnode, int sessionId)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginTerminate(headnode, sessionId, null, null);
            this.Channel.EndTerminate(result);
        }

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="sessionId">the session id</param>
        public IAsyncResult BeginTerminate(string headnode, int sessionId, AsyncCallback callback, object state)
        {
            return this.Channel.BeginTerminate(headnode, sessionId, callback, state);
        }

        /// <summary>
        /// terminate a session.
        /// </summary>
        public void EndTerminate(IAsyncResult result)
        {
            this.Channel.EndTerminate(result);
        }
        /// <summary>
        /// Returns the versions for a specific service
        /// </summary>
        /// <param name="headNode">headnode of cluster to conect to </param>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <returns>Available service versions</returns>
        public async Task<Version[]> GetServiceVersionsAsync(string serviceName)
        {
            return await this.Channel.GetServiceVersionsAsync(serviceName).ConfigureAwait(false);
        }


        /// <summary>
        /// Returns the versions for a specific service
        /// </summary>
        /// <param name="headNode">headnode of cluster to conect to </param>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <returns>Available service versions</returns>
        public Version[] GetServiceVersions(string serviceName)
        {
            return this.Channel.GetServiceVersions(serviceName);
        }

#if HPCPACK
        /// <summary>
        /// Returns soa data server information
        /// </summary>
        /// <returns>Data server information</returns>
        public async Task<DataServerInfo> GetDataServerInfoAsync()
        {
            return await this.Channel.GetDataServerInfoAsync().ConfigureAwait(false);
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            //return this.Channel.EndGetDataServerInfo(this.Channel.BeginGetDataServerInfo(null, null));
        }

        /// <summary>
        /// Returns soa data server information
        /// </summary>
        /// <returns>Data server information</returns>
        public DataServerInfo GetDataServerInfo()
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            return this.Channel.EndGetDataServerInfo(this.Channel.BeginGetDataServerInfo(null, null));
        }

        /// <summary>
        /// The async version of getting data server information
        /// </summary>
        /// <param name="asyncState">indicating the callback</param>
        /// <param name="callback">indicating the async state</param>
        /// <returns>returns the async result</returns>
        public IAsyncResult BeginGetDataServerInfo(AsyncCallback callback, object asyncState)
        {
            return this.Channel.BeginGetDataServerInfo(callback, asyncState);
        }

        /// <summary>
        /// End the async version of getting data server information
        /// </summary>
        /// <param name="result">indicating the async result</param>
        /// <returns>returns the data server information</returns>
        public DataServerInfo EndGetDataServerInfo(IAsyncResult result)
        {
            return this.Channel.EndGetDataServerInfo(result);
        }
#endif

        /// <summary>
        /// Gets SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <returns>returns the value</returns>
        public async Task<string> GetSOAConfigurationAsync(string key)
        {
            return await this.Channel.GetSOAConfigurationAsync(key).ConfigureAwait(false);
            //return this.EndGetSOAConfiguration(this.BeginGetSOAConfiguration(key, null, null));
        }

        /// <summary>
        /// Gets SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <returns>returns the value</returns>
        public string GetSOAConfiguration(string key)
        {
            return this.EndGetSOAConfiguration(this.BeginGetSOAConfiguration(key, null, null));
        }

        /// <summary>
        /// Begin method to get SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <param name="callback">indicating the callback</param>
        /// <param name="state">indicating the async state</param>
        /// <returns>returns the async result</returns>
        public IAsyncResult BeginGetSOAConfiguration(string key, AsyncCallback callback, object state)
        {
            return this.Channel.BeginGetSOAConfiguration(key, callback, state);
        }

        /// <summary>
        /// End method to get SOA configuration
        /// </summary>
        /// <param name="result">indicating the async result</param>
        /// <returns>returns the configuration value</returns>
        public string EndGetSOAConfiguration(IAsyncResult result)
        {
            return this.Channel.EndGetSOAConfiguration(result);
        }

        /// <summary>
        /// Gets SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <returns>returns the value</returns>
        public async Task<Dictionary<string, string>> GetSOAConfigurationsAsync(List<string> keys)
        {
            return await this.Channel.GetSOAConfigurationsAsync(keys).ConfigureAwait(false);
        }

        /// <summary>
        /// Get cluster info
        /// </summary>
        /// <returns></returns>
        public async Task<ClusterInfoContract> GetClusterInfoAsync()
        {
            return await this.Channel.GetClusterInfoAsync().ConfigureAwait(false);
        }
    }
}
