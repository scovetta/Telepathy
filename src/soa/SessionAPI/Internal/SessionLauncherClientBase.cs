// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.Interface;

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
        public async Task<SessionAllocateInfoContract> AllocateAsync(SessionStartInfoContract info, string endpointPrefix)
        {
            return await this.Channel.AllocateAsync(info, endpointPrefix).ConfigureAwait(false);
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            //IAsyncResult result = this.Channel.BeginAllocate(info, endpointPrefix, null, null);
            //return this.Channel.EndAllocate(result);
        }

        /// <summary>
        /// Get the session informaiton for one session
        /// </summary>
        /// <param name="headnode">the headnode name</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <param name="sessionId">the session id</param>
        /// <returns>The Session Information</returns>
        public async Task<SessionInfoContract> GetInfoAsync(string endpointPrefix, string sessionId)
        {
            return await this.Channel.GetInfoAsync(endpointPrefix, sessionId).ConfigureAwait(false);
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            //IAsyncResult result = this.Channel.BeginGetInfo(headnode, endpointPrefix, sessionId, null, null);
            //return this.Channel.EndGetInfo(result);
        }

        /// <summary>
        /// terminate a session.
        /// </summary>
        /// <param name="sessionId">the session id</param>
        public async Task TerminateAsync(string sessionId)
        {
            await this.Channel.TerminateAsync(sessionId).ConfigureAwait(false);
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            //IAsyncResult result = this.Channel.BeginTerminate(headnode, sessionId, null, null);
            //this.Channel.EndTerminate(result);
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
