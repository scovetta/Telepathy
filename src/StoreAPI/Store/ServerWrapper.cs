//------------------------------------------------------------------------------
// <copyright file="ServerWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      StoreServer object.  Yes, in a file called "ServerWrapper.cs".
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Hpc.Scheduler.Properties;
    using System.Net.Http;
#if !net40
    using AADAuthUtil;
#endif

    internal class StoreServer : IRemoteDisposable
    {
        private const int InitialConnectionLimit = 2;

        private TcpClientChannel tcpChannel = null;
        private static uint priority = 1;

        private ISchedulerStoreInternal store = null;
        private ConnectionToken connToken = null;
        private EventListener eventListener = null;
        private ChannelFactory<ISchedulerStoreInternal> storeFactory = null;
        private Dictionary<string, object> serverProps = new Dictionary<string, object>();

        private SchedulerStoreSvc owner;
        int connectionId = -1;

        int remoteConnectionRetries = 0;
        int remoteConnectionLimit = 10;
        int clientEventSleepPeriod = 1;

        private static object connect_lock = new object();

        private StoreConnectionContext context;

        public bool UsingAAD { get; private set; } = false;

        private string aadJwtToken = null;

        private bool connectAsWcf = true;

        internal StoreServer(SchedulerStoreSvc owner)
        {
            this.owner = owner;
        }

        public bool IsHttp => this.context != null && this.context.IsHttp;

        internal ISchedulerStoreInternal Server => store;

        internal ConnectionToken Token => connToken;

        internal ConnectMethod ReConnectMethod = ConnectMethod.Undefined;

#pragma warning disable 618 // disable obsolete warnings (for UserPrivilege)
        /*
         *  On UserPrivilege:
         *  In v4sp1, CallerRoles was introduced and UserPrivilege was
         *  marked [Obsolete].  Also at that same time, UserPrivilege was extended
         *  to include the two new User Roles: JobOperator and JobAdministrator but NOT marked
         *  [flag].
         *  
         *  For back-compat, legacy remoted methods will not return either of the
         *  new enum members to clients that 'connect' with old clients or
         *  that connect and provide no client version information (back compat for v2 clients).
         *  
         * It turns out that several command-line tools and the occasional psh cmdlet call the
         * GetUserPrivilege() method to implement "client-side-authorization".  Client-side auth is not a good idea
         * as it tyically duplicates code that MUST be on the server for server security hardening.
         * Duplicate code in sepearate trees often leads to bugs as the server-side auth evolves or
         * bugs in the either tree as more lines of code need to be touched.
         * 
         * That having been said, tools call the getter for UserPrivilege and in order
         * for older tools to work agains the newer headnodes... the remoted methods that return
         * UserPrivilege must remain.
         * 
         * Because the legacy getter for UserPrivilege (GetUserTokenAndPrivilege) is/was actually
         * a token factory... and took no ClientVersion information... it can never return the new
         * enum members.
         * 
         * Thus the new remoted getter was introduced, also in v4sp1, in order to enable modern
         * client code to receive the modern values.
         * 
         * UserPrivilege and all its attendent methods/signatures should probably be removed
         * when .NetRemoting is removed from the product... this because both changes will probably
         * break the same client tools.
         * 
        */

        /// <summary>
        /// Queries the Scheduler Service and returns the UserPrivilege
        /// granted to the calling thread's current identity.
        /// </summary>
        /// <returns></returns>
        internal UserPrivilege GetUserPrivilege()
        {
            UserPrivilege userPrivilege = UserPrivilege.AccessDenied;
            this.CallServerFuncWithErrorHandling(() =>
            {
                CallResult cr = new CallResult(ErrorCode.Operation_PermissionDenied);
                if (owner.ServerVersion.Version < VersionControl.V4SP1)
                {
                    ConnectionToken userToken;
                    store.GetUserTokenAndPrivilege(out userToken, out userPrivilege);
                    cr = CallResult.Succeeded;
                }
                else
                {
                    cr = store.GetUserPrivilege(connToken, out userPrivilege);
                }

                return cr;
            });

            return userPrivilege;
        }

#pragma warning restore 618

        internal UserRoles GetUserRoles()
        {
            UserRoles roles = UserRoles.AccessDenied;
            this.CallServerFuncWithErrorHandling(() => store.GetUserRoles(connToken, out roles));
            return roles;
        }

        internal Dictionary<string, object> ServerProps
        {
            get
            {
                return serverProps;
            }
        }

        #region ServiceAsClient support, current Identity
        internal string GetServiceAsClientIdentity()
        {
            return this.context.IdentityProvider?.Invoke();
        }

        #endregion

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        internal async Task<string> ConnectAsync(StoreConnectionContext context, CancellationToken token, ConnectMethod method = ConnectMethod.Undefined)
        {
            this.context = context;
            this.context.Context.IgnoreCertNameMismatchValidation();
            if (this.IsHttp)
            {
                return await this.InternalConnectOverHttpAsync(token).ConfigureAwait(false);
            }
            else
            {
                return await this.InternalConnectAsync(token, method).ConfigureAwait(false);
            }
        }

        enum StoreHttpConnectionStatus
        {
            Basic,
            Ntlm,
            BasicAfterCredential,
            NtlmAfterCredential,
            FailedConnection
        };

        /// <summary>
        /// Try to connect to the server over https.
        /// The server might be using basic/ntlm and we cannot find that out
        /// without trying to connect.
        /// Moreover, if there are no saved credentials / saved credentials are invalid
        /// we will need to ask for credentials and try basic / ntlm again.
        /// 
        /// The steps in attempting to connect to the scheduler. If the conneciton succeeds at any
        /// step we return from the method.
        /// 1. If there are saved credentials try connecting with Basic
        /// 2. If there are no saved credentials try connecting with NTLM
        /// 3. Ask for credentials and try basic
        /// 4. Use the credentials asked for in the previous step and try ntlm
        /// 5. Connection has failed
        /// </summary>
        /// <returns></returns>
        private async Task<string> InternalConnectOverHttpAsync(CancellationToken token)
        {
            // If this number is changed by customer, we don't touch it.
            if (ServicePointManager.DefaultConnectionLimit == InitialConnectionLimit)
            {
                ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 2;
            }

            // Context won't be null because in memory connection doesn't use HTTP
            var schedulerNode = await this.context.ResolveSchedulerNodeAsync(token).ConfigureAwait(false);
            string httpsServerName = "https://" + schedulerNode;
            string credentialKey = httpsServerName + "/HpcScheduler";
            string usernameForHttps = null;
            SecureString passwordForHttps = null;
            StoreHttpConnectionStatus storeHttpStatus = StoreHttpConnectionStatus.Basic;

            //Try to ready any saved credentials
            CredentialHelper.ReadUnFormattedCred(credentialKey, out usernameForHttps, out passwordForHttps);

            //if there is a saved username and password try to connect as 
            //a basic connection first
            if (usernameForHttps != null && passwordForHttps != null)
            {
                storeHttpStatus = StoreHttpConnectionStatus.Basic;
            }
            else
            {
                storeHttpStatus = StoreHttpConnectionStatus.Ntlm;
            }

            while (storeHttpStatus != StoreHttpConnectionStatus.FailedConnection)
            {
                if (StoreHttpConnectionStatus.BasicAfterCredential == storeHttpStatus)
                {
                    bool fSave = true;
                    Credentials.PromptForCredentials(httpsServerName, ref usernameForHttps, ref passwordForHttps, ref fSave, SchedulerStore._fConsole, SchedulerStore._hWnd);
                    if (fSave)
                    {
                        CredentialHelper.WriteUnformattedCred(credentialKey, usernameForHttps, passwordForHttps);
                    }
                }

                // Register with the server
                try
                {
                    bool useBasic = false;
                    if (StoreHttpConnectionStatus.Basic == storeHttpStatus
                        || StoreHttpConnectionStatus.BasicAfterCredential == storeHttpStatus)
                    {
                        useBasic = true;
                    }
                    return TryConnectionOverHttp(schedulerNode, httpsServerName, usernameForHttps, passwordForHttps, useBasic);
                }
                catch (SecurityNegotiationException)
                {
                    throw new SchedulerException(ErrorCode.Operation_ServerCertNotTrusted, null);
                }
                catch (MessageSecurityException se)
                {
                    if (se.InnerException != null)
                    {
                        System.Net.WebException innerException = se.InnerException as System.Net.WebException;
                        if (WebExceptionStatus.ProtocolError == innerException.Status)
                        {
                            //try the next status now
                            storeHttpStatus++;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (FaultException e)
                {
#if net40
                    throw;
#else
                    if (e.IsAADAuthenticationException())
                    {
                        this.UsingAAD = true;
                        // try get JWT token
                        this.aadJwtToken = await AADAuthUtil.GetAADJwtTokenFromExAsync(e, this.context.UserName, this.context.Password).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
#endif
                }
            }

            throw new SchedulerException(ErrorCode.Operation_AuthenticationFailure, string.Empty);
        }

        private string TryConnectionOverHttp(string schedulerNode, string httpsServerName, string usernameForHttps, SecureString passwordForHttps, bool useBasic)
        {
            BasicHttpBinding httpBinding = WcfChannelModule.DefaultBasicHttpBindingFactory();
            if (!this.UsingAAD)
            {
                if (useBasic)
                {
                    httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                }
                else
                {
                    httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                }
            }

            // Context won't be null because in memory connection do connect
            string endPointStr = string.Format("{0}:{1}/{2}", httpsServerName, this.context.Port, WcfServiceConstants.SchedulerStoreServiceName);

            this.CloseChannelFactory();
            this.storeFactory = new ChannelFactory<ISchedulerStoreInternal>(httpBinding, endPointStr);

            foreach (OperationDescription op in this.storeFactory.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior = op.Behaviors[typeof(DataContractSerializerOperationBehavior)]
                    as DataContractSerializerOperationBehavior;

                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = 10 * 1024 * 1024;
                }
            }

#if net40
            if (useBasic)
            {
                this.storeFactory.Credentials.UserName.UserName = usernameForHttps;
                this.storeFactory.Credentials.UserName.Password = Credentials.UnsecureString(passwordForHttps);
            }
            else
            {
                this.storeFactory.Credentials.Windows.ClientCredential = new NetworkCredential(usernameForHttps, Credentials.UnsecureString(passwordForHttps));
            }
#else
            if (this.UsingAAD)
            {
                this.storeFactory.Endpoint.Behaviors.Add(new AADClientEndpointBehavior(this.aadJwtToken));
            }
            else if (usernameForHttps != null && passwordForHttps != null)
            {
                if (useBasic)
                {
                    this.storeFactory.Credentials.UserName.UserName = usernameForHttps;
                    this.storeFactory.Credentials.UserName.Password = Credentials.UnsecureString(passwordForHttps);
                }
                else
                {
                    this.storeFactory.Credentials.Windows.ClientCredential = new NetworkCredential(usernameForHttps, Credentials.UnsecureString(passwordForHttps));
                }
            }
#endif

            ISchedulerStoreInternal store = this.storeFactory.CreateChannel();
            this.store = store;

            // Register with the server
            try
            {
                RegisterWithServer();
            }
            catch (SecurityNegotiationException)
            {
                throw new SchedulerException(ErrorCode.Operation_ServerCertNotTrusted, null);
            }

            //start a new event listener
            eventListener = EventListener.StartListeningOverHttp(schedulerNode, owner, connectionId, clientEventSleepPeriod);
            //the listener over http has already been registered

            return this.store.Name;
        }

        private async Task<string> InternalConnectAsync(CancellationToken token, ConnectMethod method = ConnectMethod.Undefined)
        {
            this.ReConnectMethod = method;
            if (method == ConnectMethod.Undefined || method == (ConnectMethod.Remoting | ConnectMethod.WCF))
            {
                // always try connect through wcf first, even though the connectionString contains one head node
                try
                {
                    var schedulerName = await ConnectWcfAsync(token).ConfigureAwait(false);
                    this.ReConnectMethod = ConnectMethod.WCF;
                    return schedulerName;
                }
                catch (Exception e) when (e is HttpRequestException || (e is RetryCountExhaustException && e.InnerException is HttpRequestException))
                {
                    // for back compatibility, if connectionString only contains one head node and wcf connection failed, then will try connect through .net remoting
                    var endPoints = this.context.Context.GetConnectionString().EndPoints;
                    if (endPoints != null && endPoints.Count() == 1)
                    {
                        try
                        {
                            var schedulerName = ConnectWithRemoting(token);
                            this.ReConnectMethod = ConnectMethod.Remoting;
                            return schedulerName;
                        }
                        catch (Exception ex)
                        {
                            throw new AggregateException(e, ex);
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else if (method == ConnectMethod.WCF)
            {
                return await ConnectWcfAsync(token).ConfigureAwait(false);
            }
            else if(method == ConnectMethod.Remoting)
            {
                return ConnectWithRemoting(token);
            }
            else
            {
                Debug.Assert(false, "Unexpected connection method.");
                return null;
            }
        }

        private async Task<string> ConnectWcfAsync(CancellationToken token)
        {
            // Context won't be null because in memory connection doesn't do connect
            // Resolve the service node each time of connecting, because the server could be switched to another node.
            var schedulerNode = await this.context.ResolveSchedulerNodeAsync(token).ConfigureAwait(false);
            TraceHelper.TraceInfo("[StoreServer] Connect to scheduler node {0}", schedulerNode);
            string endPointStr;
            // Context won't be null because in memory connection doesn't do connect
            Debug.Assert(this.context != null, "StoreConnectionContext is null during ConnectWithRemoting");
            if (this.context.InternalConnection)
            {
                TraceHelper.TraceInfo("[StoreServer] Connect to scheduler through internal connection mode");
                endPointStr = string.Format(WcfServiceConstants.NetTcpUriFormat, schedulerNode, this.context.Port, WcfServiceConstants.SchedulerStoreInternalServiceName);
                this.store = await WcfChannelModule.CreateInternalWcfProxyAsync<ISchedulerStoreInternal>(
                    endPointStr,
                    this.context.Context,
                    this.context.ServiceAsClient ? new SchedulerClientEndpointBehavior(this.context.IdentityProvider, this.context.PrincipalProvider) : null).ConfigureAwait(false);
            }
            else
            {
                endPointStr = string.Format(WcfServiceConstants.NetTcpUriFormat, schedulerNode, this.context.Port, WcfServiceConstants.SchedulerStoreServiceName);
                EndpointIdentity id = EndpointIdentity.CreateSpnIdentity(string.Concat("HOST/", schedulerNode));
                EndpointAddress addr = new EndpointAddress(new Uri(endPointStr), id);
                this.store = WcfChannelModule.CreateWcfProxy<ISchedulerStoreInternal>(
                    addr,
                    this.context.ServiceAsClient ? new SchedulerClientEndpointBehavior(this.context.IdentityProvider, this.context.PrincipalProvider) : null);
            }

            RegisterEvent(schedulerNode);
            connectAsWcf = true;
            return this.store.Name;
        }

        private string ConnectWithRemoting(CancellationToken token)
        {
            var connectionString = this.context.Context.GetConnectionString().ConnectionString;
            var schedulerNode = connectionString.Contains(EndpointsConnectionString.Delimiter) ? this.context.ResolveSchedulerNodeAsync(token).ConfigureAwait(false).GetAwaiter().GetResult() : connectionString;
            IDictionary props;
            lock (connect_lock)
            {
                if (tcpChannel != null)
                {
                    ChannelServices.UnregisterChannel(tcpChannel);
                    tcpChannel = null;
                }

                props = new Hashtable
                {
                    ["tokenImpersonationLevel"] = "Impersonation",
                    ["secure"] = "true",
                    ["protectionLevel"] = "EncryptAndSign",
                    ["priority"] = priority++,
                    ["timeout"] = 300000,
                    ["name"] = ""
                };


                // Context won't be null because in memory connection doesn't do connect
                Debug.Assert(this.context != null, "StoreConnectionContext is null during ConnectWithRemoting");
                if (this.context.ServiceAsClient)
                {
                    if (null != this.context?.UserName)
                    {
                        if (!string.IsNullOrEmpty(this.context?.DomainName))
                        {
                            props["domain"] = this.context?.DomainName;
                        }

                        props["username"] = this.context?.RawUserName;
                        props["password"] = this.context?.Password;
                    }

                    BinaryClientFormatterSinkProvider provider = new BinaryClientFormatterSinkProvider();
                    provider.Next = new ServiceAsClientSinkProvider(this);

                    tcpChannel = new TcpClientChannel(props, provider);
                }
                else
                {
                    // to support Kerberos Constrained Delegation
                    props["serviceprincipalname"] = "HOST/" + schedulerNode;
                    tcpChannel = new TcpClientChannel(props, null);
                }

                ChannelServices.RegisterChannel(tcpChannel, true);

                try
                {
                    // Context won't be null because in memory connection doesn't do connect
                    this.store = (ISchedulerStoreInternal)Activator.GetObject(
                        typeof(ISchedulerStoreInternal),
                        $"tcp://{Resolve(schedulerNode)}:{this.context.RemotingPort}/SchedulerStoreService.remote");

                    IDictionary proxyProperties = ChannelServices.GetChannelSinkProperties(store);
                    proxyProperties["serviceprincipalname"] = "HOST/" + schedulerNode;

                    // Set timeout to 10 minutes to enable potentially long lived operations to complete.
                    proxyProperties["timeout"] = "600000";
                    RegisterEvent(schedulerNode);
                    connectAsWcf = false;
                    return this.store.Name;
                }
                catch
                {
                    // Need to unregister immediately. It won't be unregistered outside because the store object won't be set to any value.
                    ChannelServices.UnregisterChannel(tcpChannel);
                    tcpChannel = null;
                    throw;
                }
            }
        }

        private void RegisterEvent(string schedulerNode)
        {
            // Register with the server
            RegisterWithServer();
            TraceHelper.TraceInfo("[StoreServer] RegisterWithServer");
            // Check and make sure the server is newer than v3sp2
            if (this.context.ServiceAsClient)
            {
                CheckMinServerVersion(VersionControl.V3SP2);
            }

            eventListener?.Stop();
            eventListener = EventListener.StartListening(schedulerNode, owner);
            RetryManager retry = new RetryManager(new PeriodicRetryTimer(10), 3000);
            while (!eventListener.Registered && retry.HasAttemptsLeft)
            {
                retry.WaitForNextAttempt();
            }

            if (!eventListener.Registered)
            {
                throw new SchedulerException(ErrorCode.Operation_CouldNotRegisterWithServer, "");
            }
        }

        private static string Resolve(string name)
        {
            IPAddress ipAddress;
            // fix bug 29333, in some cusomter environment, Dns.GetHostEntry cannot query the specified IP address from DNS server
            if (IPAddress.TryParse(name, out ipAddress))
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipAddress.ToString();
                }

                if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    return string.Format("[{0}]", ipAddress.ToString()); // IPv6 address needs to have square-bracket
                }
            }
            else
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(name);
                // Get the IP address list that resolves to the host names contained in the Alias property.
                IPAddress[] addresses = hostInfo.AddressList;
                foreach (IPAddress address in addresses)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return address.ToString();
                    }

                    if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        return string.Format("[{0}]", address.ToString()); // IPv6 address needs to have square-bracket
                    }
                }
            }

            throw new SchedulerException(ErrorCode.Operation_CannotConnectWithScheduler, string.Empty);
        }

        internal void RegisterWithServer()
        {
            // Get the client source name
            string filename;
            try
            {
                filename = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            }
            catch
            {
                filename = "unknown";
            }

            // Sometimes there will be a debugger attached.  In 
            // this case we need to remove another extension.

            filename = Path.GetFileNameWithoutExtension(filename);

            Version clientVersion = owner.ClientVersion.Version;
            Version serverVersion = null;

#pragma warning disable 618 // warning for obsolete UserPrivilege

            UserPrivilege privilege = UserPrivilege.AccessDenied;

#pragma warning restore 618 // warning for obsolete UserPrivilege

            CallResult cr = null;

            if (!this.IsHttp)
            {
                cr = store.Register(filename, "", ConnectionRole.NormalClient, clientVersion, out connToken, out privilege, out serverVersion, out serverProps);
            }
            else
            {
                cr = store.RegisterOverHttp(filename, "", ConnectionRole.NormalClient, clientVersion, out connToken, out privilege, out serverVersion, out serverProps, out connectionId, out clientEventSleepPeriod);
            }

            if (cr.Code != ErrorCode.Success)
            {
                cr.Throw();
            }

            owner.ServerVersion = new VersionControl(serverVersion);
        }

        internal void LocalServer(ISchedulerStoreInternal localServer, ConnectionToken localToken)
        {
            store = localServer;
            connToken = localToken;
        }

        bool _fRequestReconnect = false;

        object _reconnectCompletionLock = new object();

        /// <summary>
        /// This method is for anyone who encounters a connection problem while talking with the remote server.
        /// It will send out a reconnect request to the store client and user can choose 
        /// 1. sync mode (block until reconnected or timeout), or
        /// 2. async mode (just send the reconnect and exit).
        /// The monitor thread will handle the reconnection.
        /// </summary>
        /// <param name="asyncReconnect"></param>
        internal void RequestReconnect(bool asyncReconnect)
        {
            lock (_reconnectCompletionLock)
            {
                if (this.connectAsWcf && WcfChannelModule.CheckWcfProxyHealth(this.store))
                {
                    TraceHelper.TraceInfo("[StoreServer] RequestReconnect store state is ok, don't need reconnect!");
                    return;
                }

                _fRequestReconnect = true;
                TraceHelper.TraceInfo("[StoreServer {0}] RequestReconnect to scheduler, asyncReconnect {1}", this.GetHashCode(), asyncReconnect);
                if (!asyncReconnect)
                {
                    if (!Monitor.Wait(_reconnectCompletionLock, ReconnectTimeout, true))
                    {
                        TraceHelper.TraceWarning("[StoreServer {0}] Exit RequestReconnect due to timeout", this.GetHashCode());
                        throw new SchedulerException(ErrorCode.Operation_ReconnectTimeout, (ReconnectTimeout / 1000).ToString());
                    }
                }
            }

            TraceHelper.TraceInfo("[StoreServer {0}] Exit RequestReconnect to scheduler, asyncReconnect {1}", this.GetHashCode(), asyncReconnect);
        }

        #region These are the functions that really do the reconnection. They will be called by the monitor thread only.

        const int ReconnectTimeout = 120000;

        internal bool NeedReconnect()
        {
            return _fRequestReconnect;
        }

        internal void SignalReconnectComplete()
        {
            lock (_reconnectCompletionLock)
            {
                TraceHelper.TraceInfo("[StoreServer {0}] SignalReconnectComplete", this.GetHashCode());
                _fRequestReconnect = false;
                Monitor.PulseAll(_reconnectCompletionLock);
            }
        }

        /// <summary>
        /// ReconnectInternal()
        /// This method will attempt to reconnect to the server in the event
        /// of certain network related exceptions.  If it can not reconnect,
        /// it will ultimately throw the exception up to the client that is
        /// making the call.
        /// </summary>
        /// 
        internal async Task ReconnectInternal(CancellationToken token)
        {
            RetryManager retry = new RetryManager(new ExponentialBackoffRetryTimer(2000, 30000), int.MaxValue);

            Exception e = null;

            while (retry.HasAttemptsLeft)
            {
                await retry.AwaitForNextAttempt().ConfigureAwait(false);

                e = null;
                try
                {
                    if (this.owner.Disposing)
                    {
                        return;
                    }

                    if (!this.IsHttp)
                    {
                        await this.InternalConnectAsync(token, this.ReConnectMethod).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.InternalConnectOverHttpAsync(token).ConfigureAwait(false);
                    }

                    break;
                }
                catch (IOException ie)
                {
                    e = ie;
                }
                catch (RemotingException re)
                {
                    e = re;
                }
                catch (SocketException se)
                {
                    e = se;
                }

                if (e != null)
                {
                    TraceHelper.TraceInfo("[StoreServer] Exception thrown in ReconnectInternal {0}", e);
                }
            }

            if (e != null)
            {
                // Throw the exception so the client call will 
                // fail, and the client can do the "right" thing.

                throw (e);
            }
        }

        internal void SendReconnectEvent(SchedulerConnectionEventArgs args)
        {
            lock (_lock)
            {
                TraceHelper.TraceInfo("[StoreServer{0}] SendReconnectEvent {1}", this.GetHashCode(), args.Code);
                _connectionEvent?.Invoke(owner, args);
            }
        }

        #endregion

        public void RemoteDispose()
        {
            Disconnect();
        }

        internal void Disconnect()
        {
            if (eventListener != null)
            {
                //Get the eventlistener to close its socket before closing the event channel on the server
                //This means that any sockets left in time_wait state will be on the client and not on the server.

                eventListener.Stop();
                if (eventListener.ConnectionId != -1)
                {
                    try
                    {
                        store.RemoteEvent_CloseClient(ref connToken, eventListener.ConnectionId);
                    }
                    catch
                    {
                        // It's ok to ignore the exception because anyway it will be deleted
                        // in the server side after 2 minutes.
                    }
                }
            }

            try
            {
                store.Unregister(connToken);
            }
            catch
            {
                // The server could be already offline. So ignore it.
            }

            this.CloseChannelFactory();
        }

        private void CloseChannelFactory()
        {
            if (this.storeFactory != null)
            {
                this.storeFactory.Close();
                this.storeFactory = null;
            }
        }

        object _lock = new object();
        event SchedulerConnectionHandler _connectionEvent;

        internal void AddConnectionHandler(SchedulerConnectionHandler handler)
        {
            lock (_lock)
            {
                _connectionEvent += handler;
            }
        }

        internal void RemovedConnectionHandler(SchedulerConnectionHandler handler)
        {
            lock (_lock)
            {
                _connectionEvent -= handler;
            }
        }

        private CallResult CallServerFuncWithErrorHandling(Func<CallResult> action, bool asyncReconnect = false)
        {
            CallResult result = null;
            PreRemoteCall();
            bool fRetry = true;
            while (fRetry)
            {
                try
                {
                    result = action();
                    break;
                }
                catch (FaultException<ExceptionWrapper> e)
                {
                    TraceHelper.TraceError("receive FaultException<ExceptionWrapper>");
                    fRetry = HandleException(e.Detail.DeserializeException(), asyncReconnect);
                }
                catch (FaultException)
                {
                    // FaultException must be caught before CommunicationException
                    throw;
                }
                catch (CommunicationException e)
                {
                    Debug.Assert(!(e is FaultException), "FaultException should not go here.");
                    fRetry = HandleException(e, asyncReconnect);
                }
                catch (Exception e) when (
                    e is IOException ||
                    e is RemotingException ||
                    e is SocketException ||
                    e is ObjectDisposedException ||
                    e is NotImplementedException)
                {
                    fRetry = HandleException(e, asyncReconnect);
                }
            }

            PostRemoteCall(result);
            return result;
        }

        private void CallServerActionWithErrorHandling(Action action, bool asyncReconnect = false)
        {
            PreRemoteCall();
            bool fRetry = true;
            while (fRetry)
            {
                try
                {
                    action();
                    break;
                }
                catch (FaultException<ExceptionWrapper> e)
                {
                    TraceHelper.TraceError("receive FaultException<ExceptionWrapper>");
                    fRetry = HandleException(e.Detail.DeserializeException(), asyncReconnect);
                }
                catch (FaultException)
                {
                    // FaultException must be caught before CommunicationException
                    throw;
                }
                catch (CommunicationException e)
                {
                    Debug.Assert(!(e is FaultException), "FaultException should not go here.");
                    fRetry = HandleException(e, asyncReconnect);
                }
                catch (Exception e) when (
                    e is IOException ||
                    e is RemotingException ||
                    e is SocketException ||
                    e is ObjectDisposedException ||
                    e is NotImplementedException)
                {
                    fRetry = HandleException(e, asyncReconnect);
                }
            }

            PostRemoteCall();
        }

        bool HandleException(Exception e, bool asyncReconnect = false)
        {
            TraceHelper.TraceError("HandleException {0}", e);
            bool reconnect = false;
            if (e is System.IO.FileNotFoundException) {
                throw e;
            }

            if (e is IOException)
            {
                reconnect = true;
            }
            else if (e is RemotingException ||
                e is SocketException ||
                e is CommunicationException ||
                e is ObjectDisposedException ||
                e is TaskCanceledException)
            {
                HandleCommunicationException(e);
                reconnect = true;
            }
            else if (e is NotImplementedException)
            {
                if (owner.ServerVersion.Version > owner.ClientVersion.Version)
                {
                    throw new SchedulerException(ErrorCode.Operation_FeatureDeprecated,
                        ErrorCode.MakeErrorParams(
                            owner.ServerVersion.Version.ToString(),
                            owner.ClientVersion.Version.ToString()));
                }
                else
                {
                    Debug.Assert(owner.ServerVersion.Version < owner.ClientVersion.Version);
                    throw new SchedulerException(ErrorCode.Operation_FeatureUnimplemented,
                        ErrorCode.MakeErrorParams(
                            owner.ServerVersion.Version.ToString(),
                            owner.ClientVersion.Version.ToString()));
                }
            }
            else if (e is System.Data.SqlClient.SqlException)
            {
                // Wrap SQL exceptions into the scheduler exception
                throw new SchedulerException(ErrorCode.Operation_DatabaseException, e.Message);
            }
            else
            {
                // Something else bad happened.
                throw e;
            }

            if (reconnect)
            {
                RequestReconnect(asyncReconnect);
            }

            // For now return 'true' always to indicate
            // that the calling method should retry the
            // remote call.  Note that we would not get
            // here if there was a failure on reconnect
            // since the Reconnect() method would throw
            // an exception after a number of retries.

            return true;
        }

        /// <summary>
        /// If this scheduler client has suffered a communication exception,
        /// it should retry until we get to the remoteconnectionlimit (10) and then throw.
        /// This will prevent a scheduler client from getting stuck in infinite retries while
        /// it has problems connecting to the server after the intial registration
        /// </summary>
        /// <param name="e"></param>
        private void HandleCommunicationException(Exception e)
        {
            remoteConnectionRetries++;
            if (remoteConnectionRetries > remoteConnectionLimit)
            {
                remoteConnectionRetries = 0;
                throw new SchedulerException(ErrorCode.Operation_CouldNotRegisterWithServer, e.Message);
            }
        }

        internal void CheckMinServerVersion(Version minVersion)
        {
            if (owner.ServerVersion.Version < minVersion)
            {
                throw new SchedulerException(ErrorCode.Operation_FeatureUnimplemented,
                        ErrorCode.MakeErrorParams(
                           owner.ServerVersion.Version.ToString(),
                           owner.ClientVersion.Version.ToString()));
            }
        }

        void PreRemoteCall()
        {
        }

        void PostRemoteCall()
        {
            remoteConnectionRetries = 0;
        }

        void PostRemoteCall(CallResult cr)
        {
            if (cr != null && cr.Code != ErrorCode.Success)
            {
                cr.Throw();
            }
            remoteConnectionRetries = 0;
        }

        public byte[] EncryptString(string value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RunTransaction(StoreTransaction transaction)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int GetEventPort()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int RegisterForEvent(Packets.EventObjectClass eventClass, int objectId, int parentObjectId, bool reRegister)
        {
            if (eventListener == null)
            {
                return 0;
            }

            int eventId = 0;
            this.CallServerFuncWithErrorHandling(() => store.RemoteEvent_RegisterForEvent(
                        ref connToken,
                        eventClass,
                        objectId,
                        parentObjectId,
                        eventListener.ConnectionId,
                        out eventId
                        ));
            return eventId;
        }

        internal int RegisterForEventWithoutRetry(Packets.EventObjectClass eventClass, int objectId, int parentObjectId, bool reRegister)
        {
            if (eventListener == null)
            {
                return 0;
            }

            int eventId;
            store.RemoteEvent_RegisterForEvent(
                        ref connToken,
                        eventClass,
                        objectId,
                        parentObjectId,
                        eventListener.ConnectionId,
                        out eventId
                        );
            return eventId;
        }

        public void UnRegisterForEvent(int eventId)
        {
            if (eventListener == null)
            {
                return;
            }

            this.CallServerFuncWithErrorHandling(() => store.RemoteEvent_UnRegisterForEvent(
                        ref connToken,
                        eventListener.ConnectionId,
                        eventId
                        ));
        }

        internal void RemoveEvent_TriggerTouch()
        {
            if (eventListener != null && eventListener.Registered)
            {
                store.RemoteEvent_TriggerTouch(ref connToken, eventListener.ConnectionId);
            }
        }

        internal void GetEventDataOverHttp(int connectionId, DateTime lastReadEvent, out List<byte[]> eventData)
        {
            eventData = null;
            if (eventListener == null)
            {
                return;
            }

            List<byte[]> tmpEventData = null;
            this.CallServerFuncWithErrorHandling(() => store.GetEventDataOverHttp(
                             connToken,
                             connectionId,
                             lastReadEvent,
                             out tmpEventData));
            eventData = tmpEventData;
        }

        internal void EnumeratePermissionCheck(
                ObjectType obType,
                int parentId
                )
        {
            this.CallServerFuncWithErrorHandling(() => store.Object_EnumeratorPermissionCheck(ref connToken, obType, parentId));
        }

        internal RowSetResult RowEnum_Open(
                ObjectType objectType,
                int options,
                PropertyId[] columns,
                FilterProperty[] filter,
                SortProperty[] sort
                )
        {
            RowSetResult data = null;
            this.CallServerActionWithErrorHandling(() =>
            {
                data = store.RowEnum_Open(
                      ref connToken,
                      objectType,
                      options,
                      columns,
                      filter,
                      sort
                      );
            });

            if (data.Code != ErrorCode.Success)
            {
                throw new SchedulerException(data.Code, null);
            }

            return data;
        }

        internal void RowEnum_Close(int id)
        {
            this.CallServerFuncWithErrorHandling(() => store.RowEnum_Close(ref connToken, id));
        }

        internal PropertyRowSet RowEnum_GetRows(int id, int numberOfRows)
        {
            PropertyRowSet data = null;
            this.CallServerActionWithErrorHandling(() =>
            {
                data = store.RowEnum_GetRows(ref connToken, id, numberOfRows);
            });

            return data;
        }

        internal void RowEnum_SetProps(ObjectType obType, StoreProperty[] props, FilterProperty[] filter)
        {
            this.CallServerFuncWithErrorHandling(() => store.RowEnum_SetProps(ref connToken, obType, props, filter));
        }

        internal void RowEnum_Touch(int id)
        {
            this.CallServerFuncWithErrorHandling(() => store.RowEnum_Touch(ref connToken, id));
        }

        internal RowSetResult RowSet_Freeze(int rowsetId)
        {
            RowSetResult data = null;
            this.CallServerActionWithErrorHandling(() =>
            {
                data = store.RowSet_Freeze(ref connToken, rowsetId);
            });

            return data;
        }

        internal RowSetResult RowSet_GetData(int rowsetId, int firstRow, int lastRow, bool defineBoundary)
        {
            RowSetResult data = null;
            this.CallServerActionWithErrorHandling(() =>
            {
                if (!defineBoundary || owner.ServerVersion.IsV2)
                {
                    data = store.RowSet_GetData(ref connToken, rowsetId, firstRow, lastRow);
                }
                else
                {
                    data = store.RowSet_GetDataWithWindowBoundary(ref connToken, rowsetId, firstRow, lastRow);
                }

                if (data.Rows != null)
                {
                    ConvertListPropsBack(data.Rows);
                }
            });

            return data;
        }

        internal RowSetResult RowSet_OpenRowSet(
                ObjectType objectType,
                RowSetType rowsetType,
                int flags,
                PropertyId[] columns,
                FilterProperty[] filter,
                SortProperty[] sort,
                AggregateColumn[] aggragate,
                PropertyId[] orderby,
                PropertyId[] frozenIds,
                int top
                )
        {
            RowSetResult data = null;
            this.CallServerActionWithErrorHandling(() =>
            {
                if (owner.ServerVersion.IsV2)
                {
                    data = store.RowSet_OpenRowSetV2(
                            ref connToken,
                            objectType,
                            rowsetType,
                            flags,
                            columns,
                            filter,
                            sort,
                            aggragate,
                            orderby
                            );
                }
                else
                {
                    data = store.RowSet_OpenRowSet(
                            ref connToken,
                            objectType,
                            rowsetType,
                            flags,
                            columns,
                            filter,
                            sort,
                            aggragate,
                            orderby,
                            frozenIds,
                            top
                            );
                }
            });

            return data;
        }

        internal void RowSet_CloseRowSet(int rowsetId)
        {
            this.CallServerFuncWithErrorHandling(() => store.RowSet_CloseRowSet(ref connToken, rowsetId));
        }

        internal void RowSet_TouchRowSet(int rowsetId)
        {
            this.CallServerFuncWithErrorHandling(() => store.RowSet_TouchRowSet(ref connToken, rowsetId));
        }

        internal int RowSet_GetObjectIndex(int rowsetId, Int32 objectId)
        {
            int index = 0;
            this.CallServerFuncWithErrorHandling(() => store.RowSet_GetObjectIndex(ref connToken, rowsetId, objectId, out index));
            return index;
        }

        public void Object_SetProps(ObjectType obType, int obId, StoreProperty[] props)
        {
            this.CallServerFuncWithErrorHandling(() => store.Object_SetProps(ref connToken, obType, obId, props));
        }

        public void Object_GetProps(ObjectType obType, int obId, PropertyId[] ids, out StoreProperty[] props)
        {
            props = null;
            StoreProperty[] tmpProps = null;
            this.CallServerFuncWithErrorHandling(() =>
            {
                CallResult result = store.Object_GetProps(ref connToken, obType, obId, ids, out tmpProps);
                ConvertListPropsBack(tmpProps);
                return result;
            });

            props = tmpProps;
        }

        private void ConvertListPropsBack(StoreProperty[] props)
        {
            if (props == null)
            {
                return;
            }
            foreach (StoreProperty prop in props)
            {
                if (StorePropertyIds.AllocatedCores == prop.Id || StorePropertyIds.AllocatedSockets == prop.Id || StorePropertyIds.AllocatedNodes == prop.Id)
                {
                    if (prop.Value is KeyValuePair<string, int>[])
                    {
                        prop.Value = new List<KeyValuePair<string, int>>((KeyValuePair<string, int>[])prop.Value);
                    }
                }
            }
        }

        private void ConvertListPropsBack(PropertyRow[] rows)
        {
            if (rows == null)
            {
                return;
            }
            foreach (PropertyRow row in rows)
            {
                if (row.Props != null)
                {
                    ConvertListPropsBack(row.Props);
                }
            }
        }

        public void Object_GetCustomProperties(ObjectType obType, Int32 obId, out StoreProperty[] props)
        {
            props = null;
            StoreProperty[] tmpProps = null;
            this.CallServerFuncWithErrorHandling(() => store.Object_GetCustomProperties(ref connToken, obType, obId, out tmpProps));
            props = tmpProps;
        }

        public void Job_VerifyId(int jobId, out StoreProperty[] existingProps)
        {
            existingProps = null;
            StoreProperty[] tmpProps = null;
            this.CallServerFuncWithErrorHandling(() => store.Job_VerifyId(connToken, jobId, out tmpProps));
            existingProps = tmpProps;
        }

        public void Job_AddJob(ref int jobId, StoreProperty[] jobProps)
        {
            StoreProperty[] newChangedProps = SchedulerStoreHelpers.UpdateCustomGpuPropertyIfNeeded(this.owner, jobProps);
            int tmpJobId = jobId;
            this.CallServerFuncWithErrorHandling(() => store.Job_AddJob(connToken, ref tmpJobId, newChangedProps));
            jobId = tmpJobId;
        }

        public CallResult Job_SubmitJob(int jobId, StoreProperty[] jobProps)
        {
            CallResult result = null;
            StoreProperty[] newChangedProps = SchedulerStoreHelpers.UpdateCustomGpuPropertyIfNeeded(this.owner, jobProps);
            string userName;
            this.CallServerActionWithErrorHandling(() => result = store.Job_SubmitJob(connToken, jobId, newChangedProps, out userName));

            return result;
        }

        public CallResult Job_SubmitJob(int jobId, StoreProperty[] jobProps, out string userName)
        {
            CallResult result = null;
            userName = null;
            CheckMinServerVersion(VersionControl.V3SP2);
            StoreProperty[] newChangedProps = SchedulerStoreHelpers.UpdateCustomGpuPropertyIfNeeded(this.owner, jobProps);
            string tmpUserName = null;
            this.CallServerActionWithErrorHandling(() => result = store.Job_SubmitJob(connToken, jobId, newChangedProps, out tmpUserName));
            userName = tmpUserName;
            return result;
        }

        public CallResult Job_CancelJob(int jobId, CancelRequest request, StoreProperty[] cancelProps)
        {
            CallResult result = null;
            this.CallServerActionWithErrorHandling(() => result = store.Job_CancelJob(connToken, jobId, request, cancelProps));
            return result;
        }

        public CallResult Job_FinishQueuedTasks(int jobId, string message)
        {
            CallResult result = null;
            this.CallServerActionWithErrorHandling(() => result = store.Job_FinishQueuedTasks(connToken, jobId, message));
            return result;
        }

        public CallResult Job_CancelQueuedTasks(int jobId, string message)
        {
            CallResult result = null;
            this.CallServerActionWithErrorHandling(() => result = store.Job_CancelQueuedTasks(connToken, jobId, message));
            return result;
        }

        public void Job_ConfigJob(int jobId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Job_ConfigJob(connToken, jobId));
        }

        public void Job_DeleteJob(int jobId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Job_DeleteJob(connToken, jobId));
        }

        public void Job_Clone(int jobIdOld, ref int jobIdNew)
        {
            int tmpJobIdNew = jobIdNew;
            this.CallServerFuncWithErrorHandling(() => store.Job_Clone(connToken, jobIdOld, ref tmpJobIdNew));
            jobIdNew = tmpJobIdNew;
        }

        public void Job_GetJobState(int jobId, out JobState state)
        {
            state = JobState.Configuring;
            JobState tmpState = JobState.Configuring;
            this.CallServerFuncWithErrorHandling(() => store.Job_GetJobState(connToken, jobId, out tmpState));
            state = tmpState;
        }

        public int Job_CreateChildJob(int parentJobId, StoreProperty[] jobProps)
        {
            StoreProperty[] newChangedProps = SchedulerStoreHelpers.UpdateCustomGpuPropertyIfNeeded(this.owner, jobProps);
            int childJobId = 0;
            this.CallServerFuncWithErrorHandling(() => store.Job_AddChildJob(connToken, parentJobId, ref childJobId, newChangedProps));
            return childJobId;
        }

        public void Job_GetShrinkRequests(int jobId, out Dictionary<string, Dictionary<int, ShrinkRequest>> shrinkRequestsByNode)
        {
            shrinkRequestsByNode = null;
            Dictionary<string, Dictionary<int, ShrinkRequest>> tmpShrinkRequestsByNode = null;
            this.CallServerFuncWithErrorHandling(() => store.Job_GetShrinkRequests(connToken, jobId, out tmpShrinkRequestsByNode));
            shrinkRequestsByNode = tmpShrinkRequestsByNode;
        }

        public void Job_AddShrinkRequest(int jobid, int resourceid, int nodeid, ShrinkRequest request)
        {
            this.CallServerFuncWithErrorHandling(() => store.Job_AddJobShrinkRequest(connToken, jobid, resourceid, nodeid, request));
        }

        public void Job_SetHoldUntil(int jobid, DateTime holdUntil)
        {
            CheckMinServerVersion(VersionControl.V3);
            this.CallServerFuncWithErrorHandling(() => store.Job_SetHoldUntil(connToken, jobid, holdUntil));
        }

        public void Job_GetAllTaskCustomProperties(int jobid, out PropertyRow[] resultRow)
        {
            CheckMinServerVersion(VersionControl.V3);
            resultRow = null;
            PropertyRow[] tmpResultRow = null;
            this.CallServerFuncWithErrorHandling(() => store.Job_GetAllTaskCustomProperties(ref connToken, jobid, out tmpResultRow));
            resultRow = tmpResultRow;
        }

        public void Job_AddExcludedNodes(int jobId, string[] excludedNodes)
        {
            CheckMinServerVersion(VersionControl.V3);
            this.CallServerFuncWithErrorHandling(() => store.Job_AddExcludedNodes(connToken, jobId, excludedNodes));
        }

        public void Job_RemoveExcludedNodes(int jobId, string[] excludedNodes)
        {
            CheckMinServerVersion(VersionControl.V3);
            this.CallServerFuncWithErrorHandling(() => store.Job_RemoveExcludedNodes(connToken, jobId, excludedNodes));
        }

        public void Job_ClearExcludedNodes(int jobId)
        {
            CheckMinServerVersion(VersionControl.V3);
            this.CallServerFuncWithErrorHandling(() => store.Job_ClearExcludedNodes(connToken, jobId));
        }

        public void Job_GetBalanceRequest(int jobId, out IList<BalanceRequest> request)
        {
            request = null;
            //   CheckMinServerVersion(VersionControl.V4SP6);
            CallResult result = null;

            PreRemoteCall();

            bool fRetry = true;

            while (fRetry)
            {
                try
                {
                    result = store.Job_GetBalanceRequest(this.Token, jobId, out request);
                    fRetry = false;
                }
                catch (IOException e)
                {
                    fRetry = HandleException(e);
                }
                catch (RemotingException e)
                {
                    fRetry = HandleException(e);
                }
                catch (SocketException e)
                {
                    fRetry = HandleException(e);
                }
                catch (NotImplementedException e)
                {
                    fRetry = HandleException(e);
                }
                catch (System.Data.SqlClient.SqlException e)
                {
                    fRetry = HandleException(e);
                }
            }

            PostRemoteCall(result);
        }


        public void Job_RequeueJob(int jobId)
        {
            this.CallServerFuncWithErrorHandling(() =>
            {
                CallResult result;
                // This is for back-compatility
                // If the version of server is older than V4SP1, we still use configure and submit for requeuing a job
                if (owner.ServerVersion.Version < VersionControl.V4SP1)
                {
                    object requeueLock = new object();

                    lock (requeueLock)
                    {
                        StoreProperty[] outProps;
                        Object_GetProps(ObjectType.Job, jobId, new PropertyId[] { JobPropertyIds.State }, out outProps);
                        JobState state = PropertyUtil.GetValueFromProp<JobState>(outProps[0], JobPropertyIds.State, JobState.Configuring);
                        if (state == JobState.Configuring)
                        {
                            result = new CallResult(ErrorCode.Operation_PermissionDenied);
                        }
                        else
                        {
                            result = store.Job_ConfigJob(connToken, jobId);
                            if (result.Code == ErrorCode.Success)
                            {
                                string userName;
                                result = store.Job_SubmitJob(connToken, jobId, new StoreProperty[] { }, out userName);
                            }
                        }
                    }
                }
                else
                {
                    result = store.Job_RequeueJob(connToken, jobId);
                }

                return result;
            });
        }

        public void Task_ValidateTaskId(int taskId, out int jobId)
        {
            jobId = 0;
            int tmpJobId = 0;
            this.CallServerFuncWithErrorHandling(() => store.Task_ValidateTaskId(connToken, taskId, out tmpJobId));
            jobId = tmpJobId;
        }

        public void Task_AddTaskToJob(int jobId, ref int taskId, StoreProperty[] taskProps)
        {
            int tmpTaskId = taskId;
            this.CallServerFuncWithErrorHandling(() => store.Task_AddTaskToJob(ref connToken, jobId, ref tmpTaskId, taskProps));
            taskId = tmpTaskId;
        }

        public void Task_AddTasksToJob(int jobId, ref List<int> taskIdList, List<StoreProperty[]> taskPropsList)
        {
            CallResult result = null;
            PreRemoteCall();
            bool fRetry = true;

            //There is a chance this operation might fail
            //In that case we will need to find out if the tasks were actually created
            //it is possible that an exception is thrown, but results in the tasks actually been added
            //(socket or remoting exception)

            //count the number of tasks that the job already has
            int startingTaskCount = CountTasksForJob(jobId, null);
            while (fRetry)
            {
                Exception ex = null;
                try
                {
                    result = store.Task_AddTasksToJob(ref connToken, jobId, ref taskIdList, taskPropsList);
                    fRetry = false;
                }
                catch (FaultException<ExceptionWrapper> e)
                {
                    TraceHelper.TraceError("[Task_AddTasksToJob] receive FaultException<ExceptionWrapper>");
                    ex = e.Detail.DeserializeException();
                    fRetry = HandleException(ex);
                }
                catch (Exception e)
                {
                    fRetry = HandleException(e);
                    ex = e;
                }

                if (fRetry)
                {
                    //check if the tasks have already been added
                    List<int> totalTaskIdList = new List<int>();
                    int currentTaskCount = CountTasksForJob(jobId, totalTaskIdList);
                    if (currentTaskCount - startingTaskCount != 0)
                    {
                        //all tasks were added
                        if (currentTaskCount - startingTaskCount == taskPropsList.Count)
                        {
                            //we are done .. the tasks were actually added
                            fRetry = false;
                            result = CallResult.Succeeded;
                            taskIdList = new List<int>();
                            taskIdList.AddRange(totalTaskIdList.GetRange(startingTaskCount, taskPropsList.Count));
                        }
                        else
                        {
                            //some tasks were added but not all.. This should never happen and if it does
                            //we cannot deal with this case
                            throw ex;
                        }
                    }
                }
            }

            PostRemoteCall(result);
        }

        private int CountTasksForJob(int jobId, List<int> taskIdList)
        {
            IClusterJob job = owner.OpenJob(jobId);
            int taskCount = 0;
            using (ITaskRowSet taskRowSet = job.OpenTaskRowSet(RowSetType.Snapshot))
            {
                taskRowSet.SetColumns(TaskPropertyIds.Id);
                taskCount = taskRowSet.GetCount();
                if (taskIdList != null)
                {
                    foreach (PropertyRow taskProps in taskRowSet)
                    {
                        if (taskProps[0].Id == TaskPropertyIds.Id)
                        {
                            taskIdList.Add((int)taskProps[0].Value);
                        }
                    }
                }
            }
            return taskCount;
        }

        public void Task_FindTaskIdByTaskId(int jobId, TaskId taskId, out int taskSystemId)
        {
            taskSystemId = 0;
            int tmpTaskSystemId = 0;
            this.CallServerFuncWithErrorHandling(() => store.Task_FindTaskByTaskId(ref connToken, jobId, taskId, out tmpTaskSystemId));
            taskSystemId = tmpTaskSystemId;
        }

        public void Task_FindTaskIdByFriendlyId(int jobId, int jobTaskId, ref int taskId)
        {
            int tmptaskId = taskId;
            this.CallServerFuncWithErrorHandling(() => store.Task_FindTaskIdByFriendlyId(ref connToken, jobId, jobTaskId, ref tmptaskId));
            taskId = tmptaskId;
        }

        public void Task_FindTaskIdByFriendlyId(int jobId, string niceId, ref int taskId)
        {
            int tmptaskId = taskId;
            this.CallServerFuncWithErrorHandling(() => store.Task_FindTaskIdByFriendlyId(ref connToken, jobId, niceId, ref tmptaskId));
            taskId = tmptaskId;
        }

        public void Task_CloneTask(int taskId, ref int taskIdNew, StoreProperty[] taskProps)
        {
            int tmptaskIdNew = taskIdNew;
            this.CallServerFuncWithErrorHandling(() => store.Task_CloneTask(ref connToken, taskId, ref tmptaskIdNew, taskProps));
            taskIdNew = tmptaskIdNew;
        }

        public void Task_SetEnvironmentVariable(int taskId, string name, string value)
        {
            this.CallServerFuncWithErrorHandling(() => store.Task_SetEnvironmentVariable(ref connToken, taskId, name, value));
        }

        public void Task_GetEnvironmentVariables(int taskId, out Dictionary<string, string> dict)
        {
            dict = null;
            Dictionary<string, string> tmpDict = null;
            this.CallServerFuncWithErrorHandling(() => store.Task_GetEnvironmentVariables(ref connToken, taskId, out tmpDict));
            dict = tmpDict;
        }

        public void Task_FindJobTasksWithEnvVars(int jobId, out int[] taskIds)
        {
            taskIds = null;
            int[] tmpTaskIds = null;
            this.CallServerFuncWithErrorHandling(() => store.Task_FindJobTasksWithEnvVars(ref connToken, jobId, out tmpTaskIds));
            taskIds = tmpTaskIds;
        }

        public void Task_SubmitTask(int jobId, int taskId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Task_SubmitTask(ref connToken, jobId, taskId));
        }

        public void Task_SubmitTasks(int jobId, int[] taskIds)
        {
            this.CallServerFuncWithErrorHandling(() => store.Task_SubmitTasks(ref connToken, jobId, taskIds));
        }

        public CallResult Task_CancelTask(int jobId, int taskId, CancelRequest request, int errorCode, string message)
        {
            CallResult result = null;
            this.CallServerActionWithErrorHandling(() => result = store.Task_CancelTask(ref connToken, jobId, taskId, request, errorCode, message));
            return result;
        }

        public void Task_DeleteTask(int jobId, int taskId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Task_DeleteTask(ref connToken, jobId, taskId));
        }

        public void Task_ConfigTask(int jobId, int taskId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Task_ConfigTask(ref connToken, jobId, taskId));
        }

        public void Task_ConcludeServiceTask(int taskId, bool fCancelSubTasks)
        {
            CheckMinServerVersion(VersionControl.V3);
            this.CallServerFuncWithErrorHandling(() => store.Task_ConcludeServiceTask(ref connToken, taskId, fCancelSubTasks));
        }

        public bool Node_ValidateNodeId(int id, out Guid nodeId)
        {
            nodeId = Guid.Empty;
            Guid tmpNodeId = Guid.Empty;
            CallResult result = null;
            this.CallServerActionWithErrorHandling(() => result = store.Node_ValidateNodeId(ref connToken, id, out tmpNodeId));
            nodeId = tmpNodeId;
            if (result.Code == ErrorCode.Success)
            {
                return true;
            }

            return false;
        }

        public void Node_InvalidNodeQueryCache()
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_InvalidNodeQueryCache(ref connToken));
        }

        public int Node_FindNodeIdByName(string name, out Guid nodeId)
        {
            int id = 0;
            nodeId = Guid.Empty;
            Guid tmpNodeId = Guid.Empty;
            CallResult result = this.CallServerFuncWithErrorHandling(() => store.Node_FindNodeIdByName(ref connToken, name, out id, out tmpNodeId));
            nodeId = tmpNodeId;
            return id;
        }

        public int Node_FindNodeIdBySID(string sid, out Guid nodeId)
        {
            int id = 0;
            nodeId = Guid.Empty;
            Guid tmpNodeId = Guid.Empty;
            this.CallServerFuncWithErrorHandling(() => store.Node_FindNodeIdBySID(ref connToken, sid, out id, out tmpNodeId));
            nodeId = tmpNodeId;
            return id;
        }

        public int Node_FindNodeIdByNodeId(Guid nodeId)
        {
            int id = 0;
            this.CallServerFuncWithErrorHandling(() => store.Node_FindNodeIdByNodeId(ref connToken, nodeId, out id));
            return id;
        }

        public int Node_AddNode(ConnectionToken token, StoreProperty[] props)
        {
            int newNodeId = 0;
            this.CallServerActionWithErrorHandling(() => newNodeId = store.Node_AddNode(token, props));
            return newNodeId;
        }

        public void Node_RemoveNode(ConnectionToken token, Guid nodeId)
        {
            this.CallServerActionWithErrorHandling(() => store.Node_RemoveNode(token, nodeId));
        }

        public void Node_TakeNodeOffline(Guid nodeId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_TakeNodeOffline(ref connToken, nodeId));
        }

        public void Node_TakeNodeOffline(Guid nodeId, bool force)
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_TakeNodeOffline(ref connToken, nodeId, force));
        }

        public void Node_TakeNodesOffline(Guid[] nodeIds, bool force)
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_TakeNodesOffline(ref connToken, nodeIds, force));
        }

        public void Node_PutNodeOnline(Guid nodeId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_PutNodeOnline(ref connToken, nodeId));
        }

        public void Node_PutNodesOnline(Guid[] nodeIds)
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_PutNodesOnline(ref connToken, nodeIds));
        }

        public void Node_TakeNodeOffline(int nodeId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_TakeNodeOffline(ref connToken, nodeId));
        }

        public void Node_SetDrainingNodesOffline()
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_SetDrainingNodesOffline(ref connToken));
        }

        public void Node_PutNodeOnline(int nodeId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_PutNodeOnline(ref connToken, nodeId));
        }

        public void Node_SetNodeReachable(ConnectionToken token, Guid nodeid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Node_SetNodeUnreachable(ConnectionToken toke, Guid nodeid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Node_SetNodeReachable(ConnectionToken token, int nodeid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Node_SetNodeUnreachable(ConnectionToken toke, int nodeid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string Node_GetActiveHeadNodeName()
        {
            string name = String.Empty;
            this.CallServerFuncWithErrorHandling(() => store.Node_GetActiveHeadNodeName(connToken, out name));
            return name;
        }

        public int Node_AddPhantomResource(int nodeId, JobType type)
        {
            int resourceId = 0;
            this.CallServerFuncWithErrorHandling(() => store.Node_AddPhantomResource(connToken, nodeId, type, out resourceId));
            return resourceId;
        }

        public void Node_RemovePhantomResource(int resourceId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Node_RemovePhantomResource(connToken, resourceId));
        }

        public void UpdateNodePingTime(ConnectionToken token, int nodeId, DateTime pingTime)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void ScheduleResource(ConnectionToken token, int resourceId, int jobId, StoreProperty[] jobProps)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void ReserveResourceForJob(ConnectionToken token, int resourceId, int jobId, DateTime limitTime, StoreProperty[] jobProperties)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IResourceRowSet OpenResourceEnum(ConnectionToken token)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal void Profile_CreateProfile(string profileName, out int profileId)
        {
            profileId = 0;
            int tmpProfileId = 0;
            this.CallServerFuncWithErrorHandling(() => store.Profile_CreateProfile(ref connToken, profileName, out tmpProfileId));
            profileId = tmpProfileId;
        }

        internal void Profile_CloneProfile(int profileId, string profileNameNew, out int newProfileId)
        {
            newProfileId = -1;
            int tmpNewProfileId = -1;
            this.CallServerFuncWithErrorHandling(() => store.Profile_CloneProfile(ref connToken, profileId, profileNameNew, out tmpNewProfileId));
            newProfileId = tmpNewProfileId;
        }

        internal void Profile_DeleteProfile(int profileId)
        {
            this.CallServerFuncWithErrorHandling(() => store.Profile_DeleteProfile(ref connToken, profileId));
        }

        public bool VerifyProfileId(ConnectionToken token, int profileId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetProfileIdByName(ConnectionToken token, string profileName, out int profileId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IJobProfileRowSet OpenProfileEnum(ConnectionToken token)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetProfileItems(ConnectionToken token, int profileId, out ClusterJobProfileItem[] items)
        {
            ClusterJobProfileItem[] tmpItems = null;
            this.CallServerActionWithErrorHandling(() => store.GetProfileItems(token, profileId, out tmpItems));
            items = tmpItems;
        }

        public void SetProfileItem(ConnectionToken token, int profileId, ClusterJobProfileItem item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        internal void Profile_ItemOperation(int profileId, ProfileItemOperator op, PropertyId pid, ClusterJobProfileItem item)
        {
            this.CallServerFuncWithErrorHandling(() => store.Profile_ItemOp(connToken, profileId, op, pid, item));
        }

        internal void Profile_UpdateItems(Int32 profileId, IEnumerable<StoreProperty> props, IEnumerable<ClusterJobProfileItem> items, bool merge)
        {
            this.CallServerFuncWithErrorHandling(() => store.Profile_UpdateItems(ref connToken, profileId, props, items, merge));
        }

        internal PropertyId GetPropertyId(ObjectType type, StorePropertyType propertyType, string propertyName)
        {
            PropertyId pid = null;
            this.CallServerFuncWithErrorHandling(() => store.Prop_GetPropertyId(ref connToken, type, propertyType, propertyName, out pid));
            return pid;
        }

        internal PropertyId CreatePropertyId(ObjectType type, StorePropertyType propertyType, string propertyName, string propertyDescription)
        {
            PropertyId pid = null;
            this.CallServerFuncWithErrorHandling(() => store.Prop_CreatePropertyId(ref connToken, type, propertyType, propertyName, propertyDescription, out pid));
            return pid;
        }

        internal byte[] EncryptCredential(string userName, string password, string ownerName)
        {
            byte[] encryptedPassword = null;
            if (string.IsNullOrEmpty(ownerName))
            {
                this.CallServerFuncWithErrorHandling(() => store.EncryptCredential(connToken, userName, password, out encryptedPassword));
            }
            else
            {
                CheckMinServerVersion(VersionControl.V4SP5);
                this.CallServerFuncWithErrorHandling(() => store.EncryptCredentialForSpecifiedOwner(connToken, userName, password, ownerName, out encryptedPassword));
            }

            return encryptedPassword;
        }

        internal void DisableCredentialReuse(string userName)
        {
            CheckMinServerVersion(VersionControl.V3SP1);
            this.CallServerFuncWithErrorHandling(() => store.DisableCredentialReuse(connToken, userName));
        }

        /// <summary>
        /// Method used to pass the encrypted certificate blob and its password to the scheduler store
        /// </summary>
        /// <param name="userName">The user to whom the certificate belongs</param>
        /// <param name="pfxPassword">The password used to encrypt the certificate</param>
        /// <param name="reusable">Is the certificate reusable for future job submits?</param>
        /// <param name="certificate">The encrypted blob containing the certificate</param>
        internal void SaveCertificate(string userName, SecureString pfxPassword, bool? reusable, byte[] certificate)
        {
            CheckMinServerVersion(VersionControl.V3SP2);
            this.CallServerFuncWithErrorHandling(() => store.SaveCertificate(connToken, userName, Credentials.UnsecureString(pfxPassword), reusable, certificate));
        }

        /// <summary>
        /// Method used to save the extended data
        /// </summary>
        /// <param name="userName">The user to whom the certificate belongs</param>
        /// <param name="extendedData">The extended data</param>
        internal void SaveExtendedData(string userName, string extendedData)
        {
            CheckMinServerVersion(VersionControl.V4SP4);
            this.CallServerFuncWithErrorHandling(() => store.SaveExtendedData(connToken, userName, extendedData));
        }

        internal void GetCertificateInfo(out SchedulerCertInfo certInfo)
        {
            CheckMinServerVersion(VersionControl.V3SP2);
            certInfo = null;
            SchedulerCertInfo tmpCertInfo = null;
            this.CallServerFuncWithErrorHandling(() => store.GetCertificateInfo(connToken, out tmpCertInfo));
            certInfo = tmpCertInfo;
        }

        internal Version GetServerVersion()
        {
            Version version = null;
            this.CallServerFuncWithErrorHandling(() => store.GetServerVersion(out version));
            return version;
        }

        internal int GetServerLinuxHttpsValue()
        {
            int linuxHttps = 0;
            this.CallServerFuncWithErrorHandling(() => store.GetServerLinuxHttpsValue(out linuxHttps));
            return linuxHttps;
        }

        internal UserCredential[] GetCredentialList(string ownerName, bool all)
        {
            CheckMinServerVersion(VersionControl.V5);
            UserCredential[] credentials = null;
            this.CallServerFuncWithErrorHandling(() => store.GetCredentialList(connToken, ownerName, all, out credentials));
            return credentials;
        }

        internal ServerPropertyDescriptor[] Prop_GetDescriptors(ObjectType obType, string[] names)
        {
            ServerPropertyDescriptor[] descs = null;
            this.CallServerFuncWithErrorHandling(() => store.Prop_GetDescriptors(ref connToken, obType, names, out descs));
            return descs;
        }

        internal Dictionary<string, string> Config_GetSettings()
        {
            Dictionary<string, string> configs = null;
            this.CallServerActionWithErrorHandling(() => configs = store.GetConfigurationSettings(ref connToken));
            return configs;
        }

        internal Dictionary<string, string> Config_GetDefaults()
        {
            Dictionary<string, string> configs = null;
            this.CallServerActionWithErrorHandling(() => configs = store.GetConfigurationSettingDefaults(ref connToken));
            return configs;
        }

        internal Dictionary<string, string[]> Config_GetLimits()
        {
            Dictionary<string, string[]> configs = null;
            this.CallServerActionWithErrorHandling(() => configs = store.GetConfigurationSettingLimits(ref connToken));
            return configs;
        }

        internal void Config_SetSetting(string name, string value)
        {
            this.CallServerFuncWithErrorHandling(() => store.SetConfigurationSetting(ref connToken, name, value));
        }

        internal void Config_SetEmailCredential(string username, string password)
        {
            this.CallServerFuncWithErrorHandling(() => store.SetEmailCredential(ref connToken, username, password));
        }

        internal string Config_GetEmailCredentialUser()
        {
            string config = null;
            this.CallServerActionWithErrorHandling(() => config = store.GetEmailCredentialUser(ref connToken));
            return config;
        }

        internal void GetTemplateCommonName(string friendlyTemplateName, out string templateCommonName)
        {
            templateCommonName = null;
            string tmpTemplateCommonName = null;
            this.CallServerFuncWithErrorHandling(() => store.GetTemplateCommonName(connToken, friendlyTemplateName, out tmpTemplateCommonName));
            templateCommonName = tmpTemplateCommonName;
        }

        public void SetClusterEnvironmentVariable(ConnectionToken token, string name, string value)
        {
            this.CallServerFuncWithErrorHandling(() => store.SetClusterEnvironmentVariable(this.connToken, name, value));
        }

        public Dictionary<string, string> GetClusterEnvironmentVariables(ConnectionToken token)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IEnumerable<NodeGroup> GetNodeGroups()
        {
            List<NodeGroup> groups = null;
            this.CallServerFuncWithErrorHandling(() => store.GetNodeGroups(connToken, out groups));
            return groups;
        }

        public string[] GetNodesFromGroup(string groupName)
        {
            string[] nodes = null;
            this.CallServerFuncWithErrorHandling(() => store.GetNodesFromGroup(connToken, groupName, out nodes));
            return nodes;
        }

        public void RegisterTaskStateChange(ConnectionToken token, TaskStateChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.RegisterTaskStateChange(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void UnRegisterTaskStateChange(ConnectionToken token, TaskStateChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.UnRegisterTaskStateChange(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void RegisterJobStateHandler(ConnectionToken token, JobStateChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.RegisterJobStateHandler(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void RegisterJobStateHandlerEx(ConnectionToken token, JobStateChangeDelegateEx handler)
        {
            if (this.owner.StoreInProc)
            {
                store.RegisterJobStateHandlerEx(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void UnRegisterJobStateHandler(ConnectionToken token, JobStateChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.UnRegisterJobStateHandler(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void UnRegisterJobStateHandlerEx(ConnectionToken token, JobStateChangeDelegateEx handler)
        {
            if (this.owner.StoreInProc)
            {
                store.UnRegisterJobStateHandlerEx(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void RegisterResourceStateHandler(ConnectionToken token, ResourceStateChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.RegisterResourceStateHandler(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void UnRegisterResourceStateHandler(ConnectionToken token, ResourceStateChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.UnRegisterResourceStateHandler(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void RegisterNodeStateHandler(ConnectionToken token, NodeStateChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.RegisterNodeStateHandler(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void UnRegisterNodeStateHandler(ConnectionToken token, NodeStateChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.UnRegisterNodeStateHandler(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void RegisterConfigChangeHandler(ConnectionToken token, ClusterConfigChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.RegisterConfigChangeHandler(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void UnRegisterConfigChangeHandler(ConnectionToken token, ClusterConfigChangeDelegate handler)
        {
            if (this.owner.StoreInProc)
            {
                store.UnRegisterConfigChangeHandler(token, handler);
            }
            else
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void TaskGroup_CreateChild(ConnectionToken token, int jobId, int parentId, StoreProperty[] props, out int childId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void TaskGroup_AddParent(ConnectionToken token, int jobId, int groupId, int parentId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void TaskGroup_FetchGroups(ConnectionToken token, int jobId, out List<KeyValuePair<int, int>> tree)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void TaskGroup_DeleteTaskGroup(int jobId, int groupId)
        {
            this.CallServerFuncWithErrorHandling(() => store.TaskGroup_DeleteGroup(ref connToken, jobId, groupId));
        }

        public void TaskGroup_CreateTaskGroupsAndDependencies(int jobId, List<string> newGroups, List<KeyValuePair<int, int>> newDependencies, int groupIdBase, out List<int> newGroupIds)
        {
            newGroupIds = null;
            List<int> tmpNewGroupIds = null;
            this.CallServerFuncWithErrorHandling(() => store.TaskGroup_CreateTaskGroupsAndDependencies(ref connToken, jobId, newGroups, newDependencies, groupIdBase, out tmpNewGroupIds));
            newGroupIds = tmpNewGroupIds;
        }

        public void TaskGroup_UpdateGroupMaxMin(ConnectionToken token, int jobId, out List<KeyValuePair<int, int>> tree)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        public IAllocationRowSet Allocation_OpenRowSet(ConnectionToken token, int jobId, int taskId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Allocation_VerifyId(ConnectionToken token, int allocationId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Allocation_FindIdByNodeAndTask(ConnectionToken toke, int nodeId, int taskId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetUserNamePassword(ConnectionToken token, string userName, byte[] password)
        {
            this.CallServerFuncWithErrorHandling(() => store.SetUserNamePassword(this.connToken, userName, password));
        }

        internal int ExpandParametricSweepTasksInBatch(int taskId, int maxExpand, TaskState expansionState)
        {
            CallResult result = this.CallServerFuncWithErrorHandling(() => store.Task_ExpandParametricSweepTasksInBatch(ref connToken, taskId, maxExpand, expansionState));
            return int.Parse(result.Params);
        }

        // This is newly added for V3
        internal Guid PingScheduler()
        {
            try
            {
                Guid guid = store.Ping();
                return guid;
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        internal void SetJobEnvVar(int jobId, string name, string value)
        {
            if (owner.ServerVersion.Version.Major < 3)
            {
                // TODO - FIX THIS
                throw new SchedulerException(string.Format("This function is not implemented for version {0}", owner.ServerVersion.Version.ToString()));
            }

            this.CallServerFuncWithErrorHandling(() => store.Job_SetEnvVar(ref connToken, jobId, name, value));
        }

        internal Dictionary<string, string> GetJobEnvVars(int jobId)
        {
            if (owner.ServerVersion.Version.Major < 3)
            {
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> vars = null;
            this.CallServerFuncWithErrorHandling(() => store.Job_GetEnvVars(ref connToken, jobId, out vars));
            return vars;
        }

        public int Pool_FindPoolIdByName(string poolName)
        {
            int id = 0;
            CheckMinServerVersion(VersionControl.V3SP2);
            this.CallServerFuncWithErrorHandling(() => store.Pool_FindPoolIdByName(connToken, poolName, out id));
            return id;
        }

        public int Pool_AddPool(string poolName)
        {
            int id = 0;
            CheckMinServerVersion(VersionControl.V3SP2);
            this.CallServerFuncWithErrorHandling(() => store.Pool_AddPool(connToken, poolName, out id));
            return id;
        }

        public int Pool_AddPool(string poolName, int poolWeight)
        {
            int id = 0;
            CheckMinServerVersion(VersionControl.V3SP2);
            this.CallServerFuncWithErrorHandling(() => store.Pool_AddPool(connToken, poolName, poolWeight, out id));
            return id;
        }

        public void Pool_DeletePool(string poolName)
        {
            CheckMinServerVersion(VersionControl.V3SP2);
            this.CallServerFuncWithErrorHandling(() => store.Pool_DeletePool(connToken, poolName));
        }

        public void Pool_DeletePool(string poolName, bool force)
        {
            CheckMinServerVersion(VersionControl.V3SP2);
            this.CallServerFuncWithErrorHandling(() => store.Pool_DeletePool(connToken, poolName, force));
        }

        public void SchedulerAzureBurst_CreateDeployment(string deploymentId, StoreProperty[] props)
        {
            CheckMinServerVersion(VersionControl.V4);
            this.CallServerFuncWithErrorHandling(() => store.SchedulerAzureBurst_CreateDeployment(connToken, deploymentId, props));
        }

        public void SchedulerAzureBurst_DeleteDeployment(string deploymentId)
        {
            CheckMinServerVersion(VersionControl.V4);
            this.CallServerFuncWithErrorHandling(() => store.SchedulerAzureBurst_DeleteDeployment(connToken, deploymentId));
        }

        #region Scheduler On Azure Account Management

        public void SchedulerOnAzure_AddUser(string username, string password, bool isAdmin)
        {
            CheckMinServerVersion(VersionControl.V3SP2);
            this.CallServerFuncWithErrorHandling(() => store.SchedulerOnAzure_AddUser(connToken, username, password, isAdmin));
        }

        public void SchedulerOnAzure_RemoveUser(string username)
        {
            CheckMinServerVersion(VersionControl.V3SP2);
            this.CallServerFuncWithErrorHandling(() => store.SchedulerOnAzure_RemoveUser(connToken, username));
        }

        public bool SchedulerOnAzure_ValidateUser(string username, string password)
        {
            CheckMinServerVersion(VersionControl.V3SP2);
            CallResult result = this.CallServerFuncWithErrorHandling(() => store.SchedulerOnAzure_ValidateUser(connToken, username, password));

            // Return true when it succeed
            // Return false when the error code is permission denied
            // Throw otherwise
            if (result.Code == ErrorCode.Success)
            {
                return true;
            }
            else if (result.Code == ErrorCode.Operation_PermissionDenied)
            {
                return false;
            }

            result.Throw();
            return false;
        }

        #endregion
    }
}
