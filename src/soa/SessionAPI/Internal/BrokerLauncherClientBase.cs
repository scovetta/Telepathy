// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// The Broker Launcher Client Base
    /// </summary>
    public class BrokerLauncherClientBase : ClientBase<IBrokerLauncher>, IBrokerLauncher
    {

        /// <summary>
        /// https endpoint prefix
        /// </summary>
        protected const string HttpsScheme = "https";

        /// <summary>
        /// Initializes a new instance of the BrokerLauncherClient class.
        /// </summary>
        /// <param name="uri">The broker launcher EPR</param>
        /// <param name="binding">indicting the binding</param>
        public BrokerLauncherClientBase(Binding binding, EndpointAddress address)
            : base(binding, address)
        {
        }

        /// <summary>
        /// Create a session with the specific sessionid
        /// </summary>
        /// <param name="info">Session Start Info</param>
        /// <param name="sessionid">the session id</param>
        /// <returns>the session info</returns>
        public BrokerInitializationResult Create(SessionStartInfoContract info, string sessionid)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginCreate(info, sessionid, null, null);
            return this.Channel.EndCreate(result);
        }

        /// <summary>
        /// Create a session with the specific sessionid
        /// </summary>
        /// <param name="info">Session Start Info</param>
        /// <param name="sessionid">the session id</param>
        /// <param name="callback">The async callback</param>
        /// <param name="state">async state object</param>
        /// <returns>the async result</returns>
        public IAsyncResult BeginCreate(
            SessionStartInfoContract info,
            string sessionid,
            AsyncCallback callback,
            object state)
        {
            return this.Channel.BeginCreate(info, sessionid, callback, state);
        }

        /// <summary>
        /// End the async operation of creating.
        /// </summary>
        /// <param name="result">Async result</param>
        /// <returns>The session info</returns>
        public BrokerInitializationResult EndCreate(IAsyncResult result)
        {
            return this.Channel.EndCreate(result);
        }

        /// <summary>
        /// Attach to a existing session
        /// </summary>
        /// <param name="sessionId">the session id</param>
        /// <returns>the session info</returns>
        public BrokerInitializationResult Attach(string sessionId)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginAttach(sessionId, null, null);
            return this.Channel.EndAttach(result);
        }

        /// <summary>
        /// Attach to a existing session
        /// </summary>
        /// <param name="sessionId">the session id</param>
        /// <returns>IAsyncResult instance</returns>
        public IAsyncResult BeginAttach(string sessionId, AsyncCallback callback, object state)
        {
            return this.Channel.BeginAttach(sessionId, callback, state);
        }

        /// <summary>
        /// Attach to a existing session
        /// </summary>
        /// <returns>the session info</returns>
        public BrokerInitializationResult EndAttach(IAsyncResult result)
        {
            return this.Channel.EndAttach(result);
        }

        /// <summary>
        /// Close a session and cleanup all resource
        /// </summary>
        /// <param name="sessionId">the session id</param>
        public void Close(string sessionId)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginClose(sessionId, null, null);
            this.Channel.EndClose(result);
        }

        /// <summary>
        /// Close a session and cleanup all resource
        /// </summary>
        /// <param name="sessionId">the session id</param>
        /// <returns>IAsyncResult instance</returns>
        public IAsyncResult BeginClose(string sessionId, AsyncCallback callback, object state)
        {
            return this.Channel.BeginClose(sessionId, callback, state);
        }

        /// <summary>
        /// Close a session and cleanup all resource
        /// </summary>
        /// <param name="result">The IAsyncResult instance</param>
        public void EndClose(IAsyncResult result)
        {
            this.Channel.EndClose(result);
        }


        /// <summary>
        /// Create Durable session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionid">the session id</param>
        /// <returns>Session Info</returns>
        public BrokerInitializationResult CreateDurable(SessionStartInfoContract info, string sessionid)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginCreateDurable(info, sessionid, null, null);
            return this.Channel.EndCreateDurable(result);
        }

        /// <summary>
        /// Create Durable session
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionid">the session id</param>
        /// <param name="callback">The async callback</param>
        /// <param name="state">async state object</param>
        /// <returns>Session Info</returns>
        public IAsyncResult BeginCreateDurable(SessionStartInfoContract info, string sessionid, AsyncCallback callback, object state)
        {
            return this.Channel.BeginCreateDurable(info, sessionid, callback, state);
        }

        /// <summary>
        /// End the async operation of creating Durable session
        /// </summary>
        /// <param name="ar">Async Result</param>
        /// <returns>The session Info</returns>
        public BrokerInitializationResult EndCreateDurable(IAsyncResult ar)
        {
            return this.Channel.EndCreateDurable(ar);
        }

        /// <summary>
        /// Gets the active broker id list
        /// </summary>
        /// <returns>the list of active broker's session id</returns>
        public int[] GetActiveBrokerIdList()
        {
            return this.EndGetActiveBrokerIdList(this.BeginGetActiveBrokerIdList(null, null));
        }

        /// <summary>
        /// Async pattern to get the active broker id list
        /// </summary>
        public IAsyncResult BeginGetActiveBrokerIdList(AsyncCallback callback, object state)
        {
            return this.Channel.BeginGetActiveBrokerIdList(callback, state);
        }

        /// <summary>
        /// Gets the active broker id list
        /// </summary>
        /// <returns>the list of active broker's session id</returns>
        public int[] EndGetActiveBrokerIdList(IAsyncResult result)
        {
            return this.Channel.EndGetActiveBrokerIdList(result);
        }

        /// <summary>
        /// Ping session's broker
        /// </summary>
        /// <param name="sessionid">the session id</param>
        /// <returns>True if alive; else false or fault</returns>
        public bool PingBroker(string sessionid)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginPingBroker(sessionid, null, null);
            return this.Channel.EndPingBroker(result);
        }

        /// <summary>
        /// Ping session's broker
        /// </summary>
        /// <param name="sessionid">the session id</param>
        /// <returns>The unique identity</returns>
        public String PingBroker2(string sessionid)
        {
            // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
            IAsyncResult result = this.Channel.BeginPingBroker2(sessionid, null, null);
            return this.Channel.EndPingBroker2(result);
        }

        /// <summary>
        /// Ping session's broker
        /// </summary>
        /// <param name="sessionid">the session id</param>
        public IAsyncResult BeginPingBroker(string sessionID, AsyncCallback callback, object state)
        {
            return this.Channel.BeginPingBroker(sessionID, callback, state);
        }

        /// <summary>
        /// Ping session's broker
        /// </summary>
        /// <param name="sessionid">the session id</param>
        public IAsyncResult BeginPingBroker2(string sessionID, AsyncCallback callback, object state)
        {
            return this.Channel.BeginPingBroker2(sessionID, callback, state);
        }

        /// <summary>
        /// Ping session's broker
        /// </summary>
        /// <param name="result">IAsyncResult instance</param>
        /// <returns>True if alive; else false or fault</returns>
        public bool EndPingBroker(IAsyncResult result)
        {
            return this.Channel.EndPingBroker(result);
        }

        /// <summary>
        /// Ping session's broker
        /// </summary>
        /// <param name="result">IAsyncResult instance</param>
        /// <returns>The server identity, null if failed</returns>
        public String EndPingBroker2(IAsyncResult result)
        {
            return this.Channel.EndPingBroker2(result);
        }

    }
}
