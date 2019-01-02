//------------------------------------------------------------------------------
// <copyright file="BrokerClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The implementation of the BrokerClient class
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Xml;

#if !net40
    using Microsoft.Hpc.AADAuthUtil;
#endif
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker;

    /// <summary>
    ///   <para>Provides methods that enable clients to connect to a session and 
    /// send requests, then disconnect from the session and reconnect later to retrieve the responses.</para>
    /// </summary>
    /// <typeparam name="TContract">
    ///   <para>An interface to the Windows Communication Foundation (WCF) service 
    /// that is hosted by the session to which the client should connect.</para>
    /// </typeparam>
    /// <remarks>
    ///   <para>You can use the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class to connect to a regular session or a durable session.</para>
    ///   <para>You can create multiple 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> objects with different client identifiers per session. You can use each instance to send a batch of requests and receive the corresponding responses.</para> 
    ///   <para>You must dispose of an 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object when you finish using it by calling the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close" /> method or by creating the object by calling the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(Microsoft.Hpc.Scheduler.Session.SessionBase)" /> constructor within a 
    /// <see href="http://go.microsoft.com/fwlink/?LinkId=177731">using Statement</see> (http://go.microsoft.com/fwlink/?LinkId=177731) in C#. The 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close" /> method in turn calls the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Dispose(System.Boolean)" /> method.</para>
    /// </remarks>
    public class BrokerClient<TContract> : BrokerClientBase
    {
        /// <summary>
        /// Stores the cache of typed message converter
        /// </summary>
        private Dictionary<string, TypedMessageConverter> typedMessageConverterCache = new Dictionary<string, TypedMessageConverter>();

        /// <summary>
        /// Stores the lock to protect typedMessageConverterCache
        /// </summary>
        private object lockTypedMessageConverterCache = new object();

        /// <summary>
        /// Stores the instance of broker frontend factory
        /// </summary>
        private BrokerFrontendFactory frontendFactory;

        /// <summary>
        /// Store the count of the sent messages
        /// </summary>
        private int sendCount;

        /// <summary>
        /// Store the count of uncommitted messages
        /// </summary>
        private int uncommittedCount;

        /// <summary>
        /// Implements IResponseServiceCallback and distributes responses 
        /// </summary>
        private CallbackManager callbackManager;

        /// <summary>
        /// Default timeout when waiting for new responses
        /// </summary>
        private int defaultResponsesTimeout = Timeout.Infinite; // Default to Infinite

        /// <summary>
        /// Default timeout when waiting for new requests to reach broker
        /// </summary>
        private int defaultSendTimeout = 60 * 1000; // Default to 1 min

        /// <summary>
        /// Client Idenitity
        /// </summary>
        private MessageHeader clientIdHeader;

        /// <summary>
        /// User name header
        /// </summary>
        private MessageHeader userNameHeader;

        /// <summary>
        /// Client instance identity
        /// </summary>
        private MessageHeader instanceIdHeader;

        /// <summary>
        /// The last sendtime explicitly specified by the user
        /// </summary>
        private int? lastSendTimeoutMS;

        /// <summary>
        /// Whether EndRequests was called
        /// </summary>
        private bool endRequests = false;

        /// <summary>
        /// A flag indicating that there is ever failure happened when calling SendRequest/Flush/EndRequests
        /// </summary>
        private bool sendRequestFailedFlag;

        /// <summary>
        /// Lock object for sendRequestFailedFlag
        /// </summary>
        private object lockSendRequestFailedFlag = new object();

        /// <summary>
        /// Stores the broker client behaviors
        /// </summary>
        private BrokerClientBehaviors behaviors = BrokerClientBehaviors.EnableIsLastResponseProperty;

        /// <summary>
        /// Response callback that serves registered response handler
        /// </summary>
        private IResponseServiceCallback asyncResponseCallback;

        /// <summary>
        /// The instance id of the client. The later instance id will discard all unflushed requests of the previous instance id.
        /// </summary>
        private int batchId = Interlocked.Increment(ref SessionBase.BatchId);

        /// <summary>
        /// Cache of AAD Jwt Token
        /// </summary>
        private string jwtTokenCache;

        /// <summary>
        ///   <para>Gets or sets the behavior of the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object.</para>
        /// </summary>
        /// <value>
        ///   <para>The current behavior.</para>
        /// </value>
        /// <remarks>
        ///   <para>When the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Behaviors" /> property is set to 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClientBehaviors.EnableIsLastResponseProperty" />, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.IsLastResponse" /> will return 
        /// True when the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object contains the last response. The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object will hold the last response until the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method is called.</para>
        ///   <para>When the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Behaviors" /> property is set to 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClientBehaviors.None" />, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object will return the response immediately without the need to call the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method. However, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.IsLastResponse" /> property will always return 
        /// False, even when the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object contains the last response.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClientBehaviors.EnableIsLastResponseProperty" />
        public BrokerClientBehaviors Behaviors
        {
            get
            {
                return this.behaviors;
            }
            set
            {
                this.behaviors = value;
            }
        }

        /// <summary>
        /// Gets the flag of EnableIsLastResponseProperty.
        /// </summary>
        private bool EnableIsLastResponseProperty
        {
            get
            {
                return ((this.behaviors & BrokerClientBehaviors.EnableIsLastResponseProperty) != 0);
            }
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class that connects to the specified session.</para>
        /// </summary>
        /// <param name="session">
        ///   <para>An object derived from the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase" /> class that represents the session or durable session that hosts the Windows Communication Foundation (WCF) service to which the client should connect.</para> 
        /// </param>
        /// <remarks>
        ///   <para>The session uses the WCF 
        /// <see cref="System.ServiceModel.NetTcpBinding" /> type of binding by default. To specify a custom type of 
        /// <see cref="System.ServiceModel.NetTcpBinding" /> binding, use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(Microsoft.Hpc.Scheduler.Session.SessionBase,System.ServiceModel.Channels.Binding)" />,  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(Microsoft.Hpc.Scheduler.Session.SessionBase,System.String)" />, 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(System.String,Microsoft.Hpc.Scheduler.Session.SessionBase,System.ServiceModel.Channels.Binding)" />, or  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(System.String,Microsoft.Hpc.Scheduler.Session.SessionBase,System.String)" /> form of the constructor instead</para> 
        ///   <para>Because 
        /// <see cref="System.ServiceModel.NetTcpBinding" /> is used, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> must contain 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.NetTcp" /></para>
        /// </remarks>
        public BrokerClient(SessionBase session)
            : base(null, session)
        {
            Utility.ThrowIfNull(session, "session");

            Init(null, null);
        }

        /// <summary>
        ///   <para>Frees resources before the object is reclaimed by garbage collection.</para>
        /// </summary>
        ~BrokerClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// Creates instances of BrokerClient
        /// </summary>
        /// <param name="session">Session or DurableSession</param>
        /// <param name="binding">Binding to use to connect to broker</param>
        public BrokerClient(SessionBase session, Binding binding)
            : base(null, session)
        {
            Utility.ThrowIfNull(session, "session");

            Init(binding, null);
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class that connects to the specified session, and assigns the specified client identifier to that instance.</para> 
        /// </summary>
        /// <param name="clientid">
        ///   <para>Unicode string that specifies an identifier to use for the client. The maximum length of the string is 128 Unicode 
        /// characters. The client identifier can only contain lowercase and uppercase letters, 
        /// digits, underscores (_), opening or closing braces ({ or }), and spaces.</para> 
        /// </param>
        /// <param name="session">
        ///   <para>An object derived from the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase" /> class that represents the session or durable session that hosts the Windows Communication Foundation (WCF) service to which the client should connect.</para> 
        /// </param>
        /// <remarks>
        ///   <para>The session uses the WCF 
        /// <see cref="System.ServiceModel.NetTcpBinding" /> type of binding by default. To specify a custom type of 
        /// <see cref="System.ServiceModel.NetTcpBinding" /> binding, use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(Microsoft.Hpc.Scheduler.Session.SessionBase,System.ServiceModel.Channels.Binding)" />,  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(Microsoft.Hpc.Scheduler.Session.SessionBase,System.String)" />, 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(System.String,Microsoft.Hpc.Scheduler.Session.SessionBase,System.ServiceModel.Channels.Binding)" />, or  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(System.String,Microsoft.Hpc.Scheduler.Session.SessionBase,System.String)" /> form of the constructor instead</para> 
        ///   <para>Because 
        /// <see cref="System.ServiceModel.NetTcpBinding" /> is used, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> must contain 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.NetTcp" /></para>
        /// </remarks>
        public BrokerClient(string clientid, SessionBase session)
            : base(clientid, session)
        {
            Utility.ThrowIfNull(session, "session");
            Utility.ThrowIfNullOrEmpty(clientid, "clientid");
            Utility.ThrowIfTooLong(clientid.Length, "clientid", 128, SR.ClientIdTooLong);

            Init(null, null);
        }
        

        /// <summary>
        /// Creates instances of BrokerClient
        /// </summary>
        /// <param name="clientid">String identity of the client</param>
        /// <param name="session">Session or DurableSession</param>
        /// <param name="binding">Binding to use to connect to broker</param>
        public BrokerClient(string clientid, SessionBase session, Binding binding)
            : base(clientid, session)
        {
            Utility.ThrowIfNull(session, "session");
            Utility.ThrowIfNull(binding, "binding");
            Utility.ThrowIfNullOrEmpty(clientid, "clientid");
            Utility.ThrowIfTooLong(clientid.Length, "clientid", 128, SR.ClientIdTooLong);

            Init(binding, null);
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class that connects to the specified session by using the specified configuration of  
        /// <see cref="System.ServiceModel.NetTcpBinding" />.</para>
        /// </summary>
        /// <param name="session">
        ///   <para>An object derived from the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase" /> class that represents the session or durable session that hosts the Windows Communication Foundation (WCF) service to which the client should connect.</para> 
        /// </param>
        /// <param name="bindingConfigName">
        ///   <para>String that specifies the name of the binding setting that you want to 
        /// use to connect the client to the session, as defined in the configuration file for the application.</para>
        /// </param>
        /// <remarks>
        ///   <para>The Microsoft.Hpc.Scheduler.Session.BrokerClient{T} class supports two types of binding: 
        /// <see cref="System.ServiceModel.NetTcpBinding" />, which is the default, and CustomBinding.</para>
        ///   <para>If 
        /// <see cref="System.ServiceModel.NetTcpBinding" /> is used, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> must contain 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.NetTcp" />.</para>
        ///   <para>If CustomBinding is used, 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> must contain 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.Custom" />.</para>
        ///   <para>If you choose CustomBinding and you want to use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class, the binding must support DuplexSessionChannel (for example, Binding.CanBuildChannelFactory&lt;IDuplexSessionChannel&gt;() must return  
        /// True).</para>
        ///   <para>If you use 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.WebAPI" />, binding is ignored because the REST API is used for communication.</para>
        /// </remarks>
        public BrokerClient(SessionBase session, string bindingConfigName)
            : base(null, session)
        {
            Utility.ThrowIfNull(session, "session");
            Utility.ThrowIfNullOrEmpty(bindingConfigName, "bindingConfigName");

            Init(null, bindingConfigName);
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class that connects to the specified session by using a specified type of binding from the configuration file for the application, and assigns the specified client identifier to that instance.</para> 
        /// </summary>
        /// <param name="clientid">
        ///   <para>Unicode string that specifies an identifier to use for the client. The maximum length of the string is 128 Unicode 
        /// characters. The client identifier can only contain lowercase and uppercase letters, 
        /// digits, underscores (_), opening or closing braces ({ or }), and spaces.</para> 
        /// </param>
        /// <param name="session">
        ///   <para>An object derived from the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase" /> class that represents the session or durable session that hosts the Windows Communication Foundation (WCF) service to which the client should connect.</para> 
        /// </param>
        /// <param name="bindingConfigName">
        ///   <para>String that specifies the name of the binding setting that you want to 
        /// use to connect the client to the session, as defined in the configuration file for the application.</para>
        /// </param>
        /// <remarks>
        ///   <para>The Microsoft.Hpc.Scheduler.Session.BrokerClient{T} class supports two types of binding: 
        /// <see cref="System.ServiceModel.NetTcpBinding" />, which is the default, and CustomBinding.</para>
        ///   <para>If 
        /// <see cref="System.ServiceModel.NetTcpBinding" /> is used, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> must contain 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.NetTcp" />.</para>
        ///   <para>If CustomBinding is used, 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.TransportScheme" /> must contain 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.Custom" />.</para>
        ///   <para>If you choose CustomBinding and you want to use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class, the binding must support DuplexSessionChannel (for example, Binding.CanBuildChannelFactory&lt;IDuplexSessionChannel&gt;() must return  
        /// True).</para>
        ///   <para>If you use 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.WebAPI" />, binding is ignored because the REST API is used for communication.</para>
        /// </remarks>
        public BrokerClient(string clientid, SessionBase session, string bindingConfigName)
            : base(clientid, session)
        {
            Utility.ThrowIfNull(session, "session");
            Utility.ThrowIfNullOrEmpty(clientid, "clientid");
            Utility.ThrowIfTooLong(clientid.Length, "clientid", 128, SR.ClientIdTooLong);

            Init(null, bindingConfigName);
        }

        /// <summary>
        /// Initializes BrokerClient session
        /// </summary>
        /// <param name="session">Session or DurableSession</param>
        /// <param name="binding">Binding configuration to use to connect to broker</param>
        /// <param name="bindingConfigName">Binding configuration to use to connect to broker</param>
        private void Init(Binding binding, string bindingConfigName)
        {
            bool schedulerOnAzure = this.session.Info.UseInprocessBroker ?
                SoaHelper.IsSchedulerOnAzure() :
                SoaHelper.IsSchedulerOnAzure(this.session.HeadNode);
            this.callbackManager = new CallbackManager(schedulerOnAzure);
            this.clientIdHeader = MessageHeader.CreateHeader(Constant.ClientIdHeaderName, Constant.HpcHeaderNS, this.clientId);
            this.userNameHeader = MessageHeader.CreateHeader(Constant.UserNameHeaderName, Constant.HpcHeaderNS, this.session.UserName);

            if (IsDurableSession(this.session))
            {
                this.instanceIdHeader = MessageHeader.CreateHeader(Constant.ClientInstanceIdHeaderName, Constant.HpcHeaderNS, this.batchId);
            }

            // Create a fake channel to the service to get contract description
            ChannelFactory<TContract> channelFactory = new ChannelFactory<TContract>();
            this.operations = channelFactory.Endpoint.Contract.Operations;
            Utility.SafeCloseCommunicateObject(channelFactory);

            // If serviceOperationTimeout is specified, derive default timeouts for sending requests and receiving responses from serviceOperationTimeouts. 
            if (this.session.Info.ServiceOperationTimeout > 0)
            {
                // Set GetResponses timeout to service operation timeout and sync with SendRequests timeout
                // this.defaultResponsesTimeout should be default Timeout.Infinite
                this.defaultSendTimeout = this.session.Info.ServiceOperationTimeout;
            }
            else
            {
                // Otherwise default to infinite for getresponses timeout and the binding specified Sendtimeout. Infinite for GetResponses
                //   to avoid user having to grapple with timeouts to start and because there is no WCF timeout to fallback to for responses
                this.defaultSendTimeout = binding == null ? Timeout.Infinite : (int)binding.SendTimeout.TotalMilliseconds;
            }

            Debug.Assert(this.session.Info is SessionInfo);

            // Set the binding
            if (!String.IsNullOrEmpty(bindingConfigName))
            {
                binding = GetConfiguredClientBinding(bindingConfigName, this.session.Info.Secure);
            }
            else if (binding == null)
            {
                binding = GetDefaultClientBinding();
            }

            // Apply the max message size if specified
            BindingHelper.ApplyMaxMessageSize(binding, ((SessionInfo)session.Info).MaxMessageSize);

            // Set the scheme and make sure it matches the binding
            TransportScheme scheme = TransportScheme.None;
            if (binding is NetTcpBinding)
            {
                Utility.ThrowIfInvalid((this.session.Info.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp, "binding", SR.BindingTransportSchemeMismatch);
                scheme = TransportScheme.NetTcp;
            }
#if !net40
            else if (binding is NetHttpsBinding || binding is NetHttpBinding)
            {
                Utility.ThrowIfInvalid((this.session.Info.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp, "binding", SR.BindingTransportSchemeMismatch);
                scheme = TransportScheme.NetHttp;
            }
#endif
            else if (binding is BasicHttpBinding)
            {
                Utility.ThrowIfInvalid((this.session.Info.TransportScheme & TransportScheme.Http) == TransportScheme.Http, "binding", SR.BindingTransportSchemeMismatch);
                scheme = TransportScheme.Http;
            }
            else if (binding is CustomBinding)
            {
                Utility.ThrowIfInvalid((this.session.Info.TransportScheme & TransportScheme.Custom) == TransportScheme.Custom, "binding", SR.BindingTransportSchemeMismatch);
                scheme = TransportScheme.Custom;
            }
            else if (this.session.Info.UseInprocessBroker)
            {
                // Do not need to check binding
            }
            else
            {
                throw new ArgumentException(SR.BrokerClientTransportSchemeNotSupport, "session");
            }

            //save the transport scheme for the client
            this.transportScheme = scheme;

            if (((SessionInfo)this.session.Info).UseAzureQueue.GetValueOrDefault() || scheme == TransportScheme.Http)
            {
                this.frontendFactory = new HttpBrokerFrontendFactory(this.clientId, binding, this.session, scheme, this.callbackManager);
            }
            else
            {
                this.frontendFactory = new WSBrokerFrontendFactory(this.clientId, binding, (SessionInfo)this.session.Info, scheme, this.callbackManager);
            }

            SessionBase.TraceSource.TraceInformation(
                "[Session:{0}] BrokerClient instance created. ClientId = {1}, Scheme = {2}, DefaultSendTimeout = {3}, DefaultResponsesTimeout = {4}",
                this.session.Id,
                this.clientId,
                scheme,
                this.defaultSendTimeout,
                this.defaultResponsesTimeout);

            // else
            // {
            //     this.frontendFactory = new WebBrokerFrontendFactory((WebSessionInfo)this.session.Info, this.clientId, this.callbackManager);
            //     SessionBase.TraceSource.TraceInformation("[Session:{0}] BrokerClient instance created. ClientId = {1}, Scheme = {2}, DefaultSendTimeout = {3}, DefaultResponsesTimeout = {4}", this.session.Id, this.clientId, TransportScheme.WebAPI, this.defaultSendTimeout, this.defaultResponsesTimeout);
            // }
#if !net40 && HPCPACK
            if (this.session.Info.UseAad)
            {
                this.jwtTokenCache = CredUtil.GetSoaAadJwtToken(this.session.Info.Headnode, this.session.UserName, this.session.InternalPassword).GetAwaiter().GetResult();
            }
#endif
        }

        /// <summary>
        ///   <para>Sends the specified request message to the broker.</para>
        /// </summary>
        /// <param name="request">
        ///   <para>An object of the type specified by TMessage 
        /// that represents the request message that you want to send to the broker.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the message to send. You create a TMessage type by adding 
        /// a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <remarks>
        ///   <para>To include data with the request that the service should return with the response to the request 
        /// so that the client can correlate requests with responses, use any of the other forms of the SendRequest method instead.</para>
        ///   <para>The default length of time that the method should wait for the broker to accept the request before generating an exception is 
        /// the value specified by the serviceOperationTimeout setting in the configuration file used to register the service. To specify the length of this time-out, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object, System.Int32)"/> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object, System.String, System.Int32)"/> method instead. </para>
        ///   <para>To send the request along with the SOAP action for the request, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object, System.String, System.Int32)"/> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" />
        public void SendRequest<TMessage>(TMessage request)
        {
            SendRequest<TMessage>(request, messageId: null);
        }

        /// <summary>
        /// Send typed message to broker
        /// </summary>
        /// <typeparam name="TMessage">Typed message type</typeparam>
        /// <param name="request">Typed message to send</param>
        /// <param name="messageId">Specify the message Id</param>
        public void SendRequest<TMessage>(TMessage request, UniqueId messageId)
        {
            Utility.ThrowIfNull(request, "request");

            MessageDescription messageDescription = GetMessageDescription(typeof(TMessage), MessageDirection.Input);
            Utility.ThrowIfInvalid((messageDescription != null), "TMessage");
            SendRequest<TMessage>(request, String.Empty, messageDescription.Action, defaultSendTimeout, messageId);
        }

        /// <summary>
        ///   <para>Sends the specified request message to the broker, along with data that the service 
        /// should return with the response to the request so that the client can correlate requests with responses.</para>
        /// </summary>
        /// <param name="request">
        ///   <para>An object of the type specified by TMessage 
        /// that represents the request message that you want to send to the broker.</para>
        /// </param>
        /// <param name="userData">
        ///   <para>An object that contains data that the service should return with the response to the request so 
        /// that the client can correlate requests with responses. The client can get this data from the response by calling the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{TMessage}.GetUserData{T}" /> method.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the message to send. You create a TMessage type by adding 
        /// a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <remarks>
        ///   <para>To send a request without sending data that the service should return with 
        /// the response to the request so that the client can correlate requests with responses, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage"/> method instead.</para>
        ///   <para>The default length of time that the method should wait for the broker to accept the request before generating an exception is 
        /// the value specified by the serviceOperationTimeout setting in the configuration file used to register the service. To specify the length of this time-out, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object, System.Int32)"/> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object, System.String, System.Int32)"/>  </para>
        ///   <para>To send the request along with the SOAP action for the request, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object, System.String, System.Int32)"/> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{TMessage}.GetUserData{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" />
        public void SendRequest<TMessage>(TMessage request, object userData)
        {
            SendRequest<TMessage>(request, userData, messageId: null);
        }

        /// <summary>
        /// Send typed messages with user data to broker
        /// </summary>
        /// <typeparam name="TMessage">Typed message type</typeparam>
        /// <param name="userData">User supplied request data</param> 
        /// <param name="request">Typed message to send</param>
        /// <param name="messageId">Specify the message Id</param>
        public void SendRequest<TMessage>(TMessage request, object userData, UniqueId messageId)
        {
            Utility.ThrowIfNull(request, "request");
            Utility.ThrowIfNull(userData, "userData");

            MessageDescription messageDescription = GetMessageDescription(typeof(TMessage), MessageDirection.Input);
            Utility.ThrowIfInvalid((messageDescription != null), "TMessage");
            SendRequestInternal<TMessage>(request, userData, messageDescription.Action, defaultSendTimeout, messageId);
        }

        /// <summary>
        ///   <para>Sends the specified request message to the broker with the specified timeout period, along with data 
        /// that the service should return with the response to the request so that the client can correlate requests with responses.</para>
        /// </summary>
        /// <param name="request">
        ///   <para>An object of the type specified by TMessage 
        /// that represents the request message that you want to send to the broker.</para>
        /// </param>
        /// <param name="userData">
        ///   <para>An object that contains data that the service should return with the response to the request so 
        /// that the client can correlate requests with responses. The client can get this data from the response by calling the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{TMessage}.GetUserData{T}" /> method.</para>
        /// </param>
        /// <param name="sendTimeoutMilliseconds">
        ///   <para>An integer that specifies the length of time in milliseconds that 
        /// the method should wait for the broker to accept the request before generating an exception.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the message to send. You create a TMessage type by adding 
        /// a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <exception cref="System.TimeoutException">
        ///   <para>The broker did not accept the request before the specified timeout period elapsed.</para>
        /// </exception>
        /// <remarks>
        ///   <para>To use the default length of time that the method should wait for the broker to accept the 
        /// request before generating an exception specified by the serviceOperationTimeout setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage)" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object)" />, or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" /> method instead.</para>
        ///   <para>To send the request along with the SOAP action for the request, use the
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String,System.Int32)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{TMessage}.GetUserData{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" />
        public void SendRequest<TMessage>(TMessage request, object userData, int sendTimeoutMilliseconds)
        {
            SendRequest<TMessage>(request, userData, sendTimeoutMilliseconds, null);
        }

        /// <summary>
        /// Send typed messages with user data to broker
        /// </summary>
        /// <typeparam name="TMessage">Typed message type</typeparam>
        /// <param name="userData">User supplied request data</param> 
        /// <param name="request">Typed message to send</param>
        /// <param name="sendTimeoutMS">Timeout for send requests</param>
        /// <param name="messageId">Specify the message Id</param>
        public void SendRequest<TMessage>(TMessage request, object userData, int sendTimeoutMilliseconds, UniqueId messageId)
        {
            Utility.ThrowIfNull(request, "request");
            Utility.ThrowIfNull(userData, "userData");
            Utility.ThrowIfInvalidTimeout(sendTimeoutMilliseconds, "sendTimeoutMilliseconds");

            MessageDescription messageDescription = GetMessageDescription(typeof(TMessage), MessageDirection.Input);
            Utility.ThrowIfInvalid((messageDescription != null), "TMessage");
            SendRequestInternal<TMessage>(request, userData, messageDescription.Action, sendTimeoutMilliseconds, messageId);
        }

        /// <summary>
        ///   <para>Sends the specified request message and SOAP action to the broker, along with data that 
        /// the service should return with the response to the request so that the client can correlate requests with responses.</para>
        /// </summary>
        /// <param name="request">
        ///   <para>An object of the type specified by TMessage 
        /// that represents the request message that you want to send to the broker.</para>
        /// </param>
        /// <param name="userData">
        ///   <para>An object that contains data that the service should return with the response to the request so 
        /// that the client can correlate requests with responses. The client can get this data from the response by calling the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{TMessage}.GetUserData{T}" /> method.</para>
        /// </param>
        /// <param name="action">
        ///   <para>String that specifies a SOAP action for the message if the appropriate 
        /// action cannot be derived from the type of the message. SOAP actions are defined  
        /// in the .wdsl file that is created when you add a service reference 
        /// to the Visual Studio project for the client application or by running the svcutil tool.</para> 
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the message to send. You create a TMessage type by adding 
        /// a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <remarks>
        ///   <para>The default length of time that the method should wait for the broker to accept the request before generating an exception is 
        /// the value specified by the serviceOperationTimeout setting in the configuration file used to register the service. To specify the length of this time-out, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object, System.Int32)"/> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object, System.String, System.Int32)"/> method instead.</para>
        ///   <para>To send the request without the SOAP action for the request, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage)"/>, 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object)"/>" />, or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage, System.Object, System.Int32)"/> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{TMessage}.GetUserData{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.Int32)" />
        public void SendRequest<TMessage>(TMessage request, object userData, string action)
        {
            SendRequest<TMessage>(request, userData, action, null);
        }

        /// <summary>
        /// Send typed messages with user data to broker
        /// </summary>
        /// <typeparam name="TMessage">Typed message type</typeparam>
        /// <param name="request">Typed message to send</param>
        /// <param name="userData">User supplied request data</param> 
        /// <param name="action">Action of typed message if type is ambiguous</param>
        /// <param name="messageId">Specify the message Id</param>
        public void SendRequest<TMessage>(TMessage request, object userData, string action, UniqueId messageId)
        {
            Utility.ThrowIfNull(request, "request");
            Utility.ThrowIfNull(userData, "userData");
            Utility.ThrowIfNullOrEmpty(action, "action");

            MessageDescription messageDescription = GetMessageDescription(typeof(TMessage), MessageDirection.Input, action);
            Utility.ThrowIfInvalid((messageDescription != null), "TMessage/action");
            SendRequestInternal<TMessage>(request, userData, action, defaultSendTimeout, messageId);
        }

        /// <summary>
        ///   <para>Sends the specified request message and SOAP action to the broker with the specified timeout period, along with 
        /// data that the service should return with the response to the request so that the client can correlate requests with responses.</para>
        /// </summary>
        /// <param name="request">
        ///   <para>An object of the type specified by TMessage 
        /// that represents the request message that you want to send to the broker.</para>
        /// </param>
        /// <param name="userData">
        ///   <para>An object that contains data that the service should return with the response to the request so 
        /// that the client can correlate requests with responses. The client can get this data from the response by calling the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{TMessage}.GetUserData{T}" /> method.</para>
        /// </param>
        /// <param name="action">
        ///   <para>String that specifies a SOAP action for the message if the appropriate 
        /// action cannot be derived from the type of the message. SOAP actions are defined  
        /// in the .wsdl file that is created when you add a service reference 
        /// to the Visual Studio project for the client application or by running the svcutil tool.</para> 
        /// </param>
        /// <param name="sendTimeoutMilliseconds">
        ///   <para>An integer that specifies the length of time in milliseconds that 
        /// the method should wait for the broker to accept the request before generating an exception.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the message to send. You create a TMessage type by adding 
        /// a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <exception cref="System.TimeoutException">
        ///   <para>The broker did not accept the request before the specified timeout period elapsed.</para>
        /// </exception>
        /// <remarks>
        ///   <para>To use the default length of time that the method should wait for the broker to accept the 
        /// request before generating an exception specified by the serviceOperationTimeout setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage)" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object)" />, or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" /> method instead.</para>
        ///   <para>To send the request without the SOAP action for the request, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage)" />,
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object)" />, or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.Int32)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{TMessage}.GetUserData{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.String)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SendRequest{TMessage}(TMessage,System.Object,System.Int32)" />
        public void SendRequest<TMessage>(TMessage request, object userData, string action, int sendTimeoutMilliseconds)
        {
            SendRequest<TMessage>(request, userData, action, sendTimeoutMilliseconds, null);
        }

        /// <summary>
        /// Send typed messages with user data to broker
        /// </summary>
        /// <typeparam name="TMessage">Typed message type</typeparam>
        /// <param name="request">Typed message to send</param>
        /// <param name="userData">User supplied request data</param> 
        /// <param name="action">Action of typed message if type is ambiguous</param>
        /// <param name="sendTimeoutMS">Timeout for send requests</param>
        /// <param name="messageId">Specify the message Id</param>
        public void SendRequest<TMessage>(TMessage request, object userData, string action, int sendTimeoutMilliseconds, UniqueId messageId)
        {
            Utility.ThrowIfNull(request, "request");
            Utility.ThrowIfNull(userData, "userData");
            Utility.ThrowIfNullOrEmpty(action, "action");
            Utility.ThrowIfInvalidTimeout(sendTimeoutMilliseconds, "sendTimeoutMilliseconds");

            MessageDescription messageDescription = GetMessageDescription(typeof(TMessage), MessageDirection.Input, action);
            Utility.ThrowIfInvalid((messageDescription != null), "TMessage/action");
            SendRequestInternal<TMessage>(request, userData, action, defaultSendTimeout, messageId);
        }

        /// <summary>
        /// Send typed messages with user data to broker
        /// </summary>
        /// <typeparam name="TMessage">Typed message type</typeparam>
        /// <param name="request">Typed message to send</param>
        /// <param name="userData">User supplied request data</param> 
        /// <param name="action">Action of typed message if type is ambiguous</param>
        /// <param name="sendTimeoutMS">Timeout for send requests</param>
        private void SendRequestInternal<TMessage>(TMessage request, object userData, string action, int sendTimeoutMilliseconds)
        {
            SendRequestInternal<TMessage>(request, userData, action, defaultSendTimeout, null);
        }

        /// <summary>
        /// Send typed messages with user data to broker
        /// </summary>
        /// <typeparam name="TMessage">Typed message type</typeparam>
        /// <param name="request">Typed message to send</param>
        /// <param name="userData">User supplied request data</param> 
        /// <param name="action">Action of typed message if type is ambiguous</param>
        /// <param name="sendTimeoutMS">Timeout for send requests</param>
        /// <param name="messageId">Specify the message Id</param>
        private void SendRequestInternal<TMessage>(TMessage request, object userData, string action, int sendTimeoutMilliseconds, UniqueId messageId)
        {
            // Prepare the request message
            TypedMessageConverter typedMessageConverter = null;
            lock (this.lockTypedMessageConverterCache)
            {
                if (!this.typedMessageConverterCache.TryGetValue(action, out typedMessageConverter))
                {
                    typedMessageConverter = TypedMessageConverter.Create(typeof(TMessage), action);
                    this.typedMessageConverterCache.Add(action, typedMessageConverter);
                }
            }

            Message message = null;
            bool isFailed = false;
            lock (typedMessageConverter)
            {
                if (this.transportScheme == TransportScheme.Http)
                {
                    message = typedMessageConverter.ToMessage(request, MessageVersion.Soap11);
                }
                else
                {
                    message = typedMessageConverter.ToMessage(request);
                }
            }

            //if soap11 message, add the message header for message id
            if (message.Version == MessageVersion.Soap11)
            {
                if (messageId == null)
                {
                    message.Headers.Add(GenerateMessageIdHeader(new UniqueId()));
                }
                else
                {
                    message.Headers.Add(GenerateMessageIdHeader(messageId));
                }
            }
            else
            {

                // Exception is possibly thrown if the message id
                // was not set properly
                bool isGuid = false;
                try
                {
                    // Bug 13622: Set message id here for trace consistency
                    // If user didn't set the message id, we will set one for
                    // him.
                    if (message.Headers.MessageId == null)
                    {
                        if (messageId == null)
                        {
                            message.Headers.MessageId = new UniqueId();
                        }
                        else
                        {
                            message.Headers.MessageId = messageId;
                        }
                        isGuid = true;
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] Incoming request does not contain a message Id, generated GUID {1} is assigned as the message id.", this.session.Id, message.Headers.MessageId);
                    }
                    else
                    {
                        isGuid = message.Headers.MessageId.IsGuid;
                    }
                }
                catch
                {
                    // Swallow the exception here and leave isGuid
                    // to false. ArgumentException would be throw
                    // if it is not a GUID.
                }

                // Bug 18110: If user set a message id by himself and the
                // message id was not a GUID, throw argument exception.
                if (!isGuid)
                {
                    throw new ArgumentException(SR.InvalidMessageId);
                }
            }

            try
            {
                message.Headers.Add(GenerateUserDataHeader(userData));
                message.Headers.Add(this.clientIdHeader);

#if !net40
                if (this.session.Info.UseAad)
                {
                    message.Headers.Add(new AADAuthMessageHeader(this.jwtTokenCache));
                }
#endif

                if (IsDurableSession(this.session))
                {
                    this.CheckAndClearSendRequestFailedFlag();
                    message.Headers.Add(this.instanceIdHeader);
                }

                lock (this.objectLock)
                {
                    this.CheckDisposed();
                    this.CheckBrokerAvailability();

                    if ((this.session.Info as SessionInfo).UseAzureQueue == true)
                    {
                        // add username in the message header if secure
                        if ((this.session.Info as SessionInfo).Secure)
                        {
                            message.Headers.Add(this.userNameHeader);
                        }

                        AzureQueueProxy azureQueueProxy = this.frontendFactory.GetBrokerClientAQ();
                        azureQueueProxy.SendMessage(message);
                    }
                    else
                    {
                        //IOutputChannel brokerClient = this.frontendFactory.GetBrokerClient();
                        IChannel brokerClient = this.frontendFactory.GetBrokerClient();

                        // Send the request. We dont need to do this async to hook up to heartbeats because they are one-way calls. If the broker goes down, it will throw
                        //  a CommunicationException
                        if (brokerClient is IRequestChannel)
                        {
                            Message httpResponseMessage = null;
                            if (sendTimeoutMilliseconds == Timeout.Infinite)
                                httpResponseMessage = (brokerClient as IRequestChannel).Request(message, TimeSpan.MaxValue);
                            else
                                httpResponseMessage = (brokerClient as IRequestChannel).Request(message, new TimeSpan(0, 0, 0, 0, sendTimeoutMilliseconds));
                        }
                        else // IOutputChannel
                        {
                            if (sendTimeoutMilliseconds == Timeout.Infinite)
                                (brokerClient as IOutputChannel).Send(message, TimeSpan.MaxValue);
                            else
                                (brokerClient as IOutputChannel).Send(message, new TimeSpan(0, 0, 0, 0, sendTimeoutMilliseconds));
                        }
                    }
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] MessageId = {1}, Request has been successfully sent to broker.", this.session.Id, messageId);

                    // Reset the heartbeat since operation just succeeded
                    this.session.ResetHeartbeat();

                    // Remember the last SendRequest timeout used and use it for throttling timeout in EndRequests
                    this.lastSendTimeoutMS = sendTimeoutMilliseconds;

                    Interlocked.Increment(ref this.sendCount);
                    Interlocked.Increment(ref this.uncommittedCount);
                }
            }
            catch (FaultException<SessionFault> e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}] MessageId = {1}, Fault exception occured while sending request: {2}. FaultCode = {3}", this.session.Id, messageId, e, e.Detail.Code);
                isFailed = true;
                throw Utility.TranslateFaultException(e);
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}] MessageId = {1}, Failed to send request: {2}", this.session.Id, messageId, e);
                isFailed = true;

                // Cleanup up message object if something fails
                message.Close();
                throw;
            }
            finally
            {
                if (isFailed && IsDurableSession(this.session))
                {
                    this.DiscardUnflushedRequests();
                    this.SetSendRequestFailedFlag();
                }
            }
        }

        /// <summary>
        /// Generates SOAP header for user data
        /// </summary>
        /// <param name="userData">Serializable user data object</param>
        /// <returns></returns>
        private static MessageHeader GenerateUserDataHeader(object userData)
        {
            MessageHeader messageHeader = MessageHeader.CreateHeader(Constant.UserDataHeaderName, Constant.HpcHeaderNS, userData);
            int messageHeaderLen = messageHeader.ToString().Length;
            Utility.ThrowIfTooLong(messageHeaderLen, "userData", Constant.MaxUserDataLen, SR.UserDataTooLong, Constant.MaxUserDataLen, messageHeaderLen);
            return messageHeader;
        }

        /// <summary>
        /// Generate SOAP header for message id
        /// </summary>
        /// <param name="messageId">Message id</param>
        /// <returns>Message header</returns>
        private static MessageHeader GenerateMessageIdHeader(UniqueId messageId)
        {
            MessageHeader messageHeader = MessageHeader.CreateHeader(Constant.MessageIdHeaderName, Constant.HpcHeaderNS, messageId.ToString());
            return messageHeader;
        }

        /// <summary>
        ///   <para>Retrieves an enumerator of objects of the specified type that represent the 
        /// response messages that the service-oriented architecture (SOA) service returned, subject to the default timeout period.</para>
        /// </summary>
        /// <typeparam name="TMessage">
        ///   <para>The type of the message for which you want to retrieve an enumerator. You create a TMessage 
        /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <returns>
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" /> object that is an enumerator of 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> objects that represent the responses from the SOA service.</para>
        /// </returns>
        /// <exception cref="System.TimeoutException">
        ///   <para>The enumerator reached the end of the default timeout period before all of the responses were received.</para>
        /// </exception>
        /// <remarks>
        ///   <para>The default length of time that the enumeration waits for responses is the value specified by the serviceOperationTimeout setting in the 
        /// configuration file used to register the service. If the sessions for your HPC cluster are queued for long periods of time, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.Int32)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.String,System.String,System.Int32)" /> to specify the timeout value instead. You may want to increase the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para> 
        ///   <para>To get responses for all called SOA operations, regardless of the type of the object that represents the response messages, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses()" /> method instead.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.String,System.String,System.Int32)" />
        public BrokerResponseEnumerator<TMessage> GetResponses<TMessage>()
        {
            return GetResponses<TMessage>(defaultResponsesTimeout);
        }

        /// <summary>
        ///   <para>Retrieves an enumerator of objects of the specified type that represent the 
        /// response messages that the service-oriented architecture (SOA) service returned, subject to the specified timeout period.</para>
        /// </summary>
        /// <param name="waitTimeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds that the enumerator should wait between 
        /// successive responses or for an indicator that no more responses will be sent before the enumerator stops enumerating responses.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the message for which you want to retrieve an enumerator. You create a TMessage 
        /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <returns>
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" /> object that is an enumerator of 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> objects that represent the responses from the SOA service.</para>
        /// </returns>
        /// <exception cref="System.TimeoutException">
        ///   <para>The enumerator reached the end of the specified timeout period before 
        /// receiving a new response or an indicator that no more responses will be sent.</para>
        /// </exception>
        /// <remarks>
        ///   <para>To use the default timeout period specified by the serviceOperationTimeout 
        /// setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}()" /> method instead.</para>
        ///   <para>If the sessions for your HPC cluster are queued for long periods of time, you may want to increase 
        /// the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para>
        ///   <para>To get responses for all called SOA operations, regardless of the type of the object that represents the response messages, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses(System.Int32)" /> method instead.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.String,System.String,System.Int32)" />
        public BrokerResponseEnumerator<TMessage> GetResponses<TMessage>(int waitTimeoutMilliseconds)
        {
            Utility.ThrowIfInvalidTimeout(waitTimeoutMilliseconds, "waitTimeoutMilliseconds");
            return this.GetResponsesInternal<TMessage>(String.Empty, String.Empty, waitTimeoutMilliseconds);
        }

        /// <summary>
        ///   <para>Retrieves an enumerator of objects of the specified type that represent the response messages that the service-oriented 
        /// architecture (SOA) service returned, subject to the specified timeout period, 
        /// and specifies the SOAP actions for the request and response messages.</para> 
        /// </summary>
        /// <param name="action">
        ///   <para>String that specifies a SOAP action for the request message if the 
        /// appropriate action cannot be derived from the type of the request message. SOAP actions are  
        /// defined in the .wdsl file that is created when you add a service reference 
        /// to the Visual Studio project for the client application or by running the svcutil tool.</para> 
        /// </param>
        /// <param name="replyAction">
        ///   <para>String that specifies a SOAP action for the response message if the 
        /// appropriate action cannot be derived from the type of the response message. SOAP actions are  
        /// defined in the .wdsl file that is created when you add a service reference 
        /// to the Visual Studio project for the client application or by running the svcutil tool.</para> 
        /// </param>
        /// <param name="waitTimeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds that the enumerator should wait between 
        /// successive responses or for an indicator that no more responses will be sent before the enumerator stops enumerating responses.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the message for which you want to retrieve an enumerator. You create a TMessage 
        /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <returns>
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" /> object that is an enumerator of 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> objects that represent the responses from the SOA service.</para>
        /// </returns>
        /// <exception cref="System.TimeoutException">
        ///   <para>The enumerator reached the end of the specified timeout period before 
        /// receiving a new response or an indicator that no more responses will be sent.</para>
        /// </exception>
        /// <remarks>
        ///   <para>To use the default timeout period specified by the serviceOperationTimeout 
        /// setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}()" /> method instead.</para>
        ///   <para>If the sessions for your HPC cluster are queued for long periods of time, you may want to increase 
        /// the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para>
        ///   <para>To get responses for all called SOA operations, regardless of the type of the object that represents the response messages, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses(System.Int32)" /> method instead.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.Int32)" />
        public BrokerResponseEnumerator<TMessage> GetResponses<TMessage>(string action, string replyAction, int waitTimeoutMilliseconds)
        {
            Utility.ThrowIfNullOrEmpty(action, "action");
            Utility.ThrowIfNullOrEmpty(replyAction, "replyAction");
            Utility.ThrowIfInvalidTimeout(waitTimeoutMilliseconds, "waitTimeoutMilliseconds");
            return this.GetResponsesInternal<TMessage>(action, replyAction, waitTimeoutMilliseconds);
        }

        /// <summary>
        ///   <para>Retrieves an enumerator of objects that represent the response messages 
        /// that the service-oriented architecture (SOA) service returned, subject to the default timeout period.</para>
        /// </summary>
        /// <returns>
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" /> object that is an enumerator of 
        /// <see cref="System.Object" /> objects that represent the responses from the SOA service.</para>
        /// </returns>
        /// <exception cref="System.TimeoutException">
        ///   <para>The enumerator reached the end of the default timeout period before all of the responses were received.</para>
        /// </exception>
        /// <remarks>
        ///   <para>The default length of time that the enumeration waits for responses is the value specified by the serviceOperationTimeout setting in the 
        /// configuration file used to register the service. If the sessions for your HPC cluster are queued for long periods of time, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses(System.Int32)" /> method to specify the timeout value instead. You may want to increase the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para> 
        ///   <para>When you use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses()" /> method, check the type of the object that you get with the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.Result" /> property of the object to identify the type of the response, and cast the object to that type to access its members.</para> 
        ///   <para>To specify the type of the object that represents the response messages, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}()" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}()" />
        public BrokerResponseEnumerator<Object> GetResponses()
        {
            return GetResponses(defaultResponsesTimeout);
        }

        /// <summary>
        ///   <para>Retrieves an enumerator of objects that represent the response messages 
        /// that the service-oriented architecture (SOA) service returned, subject to the specified timeout period.</para>
        /// </summary>
        /// <param name="waitTimeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds that the enumerator should wait between 
        /// successive responses or for an indicator that no more responses will be sent before the enumerator stops enumerating responses.</para>
        /// </param>
        /// <returns>
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" /> object that is an enumerator of 
        /// <see cref="System.Object" /> objects that represent the responses from the SOA service.</para>
        /// </returns>
        /// <exception cref="System.TimeoutException">
        ///   <para>The enumerator reached the end of the specified timeout period before 
        /// receiving a new response or an indicator that no more responses will be sent.</para>
        /// </exception>
        /// <remarks>
        ///   <para>To use the default timeout period specified by the serviceOperationTimeout 
        /// setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}()" /> method instead. If the sessions for your HPC cluster are queued for long periods of time, you may want to increase the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para> 
        ///   <para>When you use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses(System.Int32)" /> method, check the type of the object that you get with the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.Result" /> property of the object to identify the type of the response, and cast the object to that type to access its members.</para> 
        ///   <para>To specify the type of the object that represents the response messages, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.Int32)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.Int32)" />
        public BrokerResponseEnumerator<Object> GetResponses(int waitTimeoutMilliseconds)
        {
            return GetResponses<Object>(waitTimeoutMilliseconds);
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the 
        /// form of objects of the specified type from the service-oriented architecture (SOA) service.</para>
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the response message that you want the callback function to receive. You create a TMessage 
        /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <remarks>
        ///   <para>The default length of time that the callback function waits for responses is the value specified by the serviceOperationTimeout setting in 
        /// the configuration file used to register the service. If the sessions for your HPC cluster are queued for long periods of time, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage} ,System.Int32)" />,  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage}, System.String, System.String ,System.Int32)" />,
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage}, System.Object ,System.Int32)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage}, System.String, System.String, System.Object, System.Int32)" />
        ///  method to specify the timeout value instead. You may want to increase the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para> 
        ///   <para>To designate a callback function that includes a parameter for a state object 
        /// that you want to pass to the callback function each time it is called, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" />,  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,System.Int32)" />, or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.String, System.String,System.Object,System.Int32)" /> method.</para> 
        ///   <para>To designate a callback function that receives responses for all called SOA 
        /// operations, regardless of the type of the object that represents the response messages, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" /> method instead.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.GetResponses{TMessage}()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.String,System.String,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.String,System.String,System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.Int32)" 
        /// /> 
        public void SetResponseHandler<TMessage>(BrokerResponseHandler<TMessage> callback)
        {
            Utility.ThrowIfNull(callback, "callback");

            this.SetResponseHandler<TMessage>(callback, defaultResponsesTimeout, !this.EnableIsLastResponseProperty);
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the form of objects of the specified type from the 
        /// service-oriented architecture (SOA) service, along with a state object that you 
        /// want to pass to the callback function each time it is called.</para> 
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <param name="state">
        ///   <para>A state object that you want to pass to the callback function each time it is called. The function that the 
        /// <paramref name="callback" /> parameter specifies must include a parameter for this object. You can use this object to pass the instance of the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class or other state information to the callback function.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the response message that you want the callback function to receive. You create a TMessage 
        /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <remarks>
        ///   <para>The default length of time that the callback function waits for responses is the value specified by the serviceOperationTimeout setting in 
        /// the configuration file used to register the service. If the sessions for your HPC cluster are queued for long periods of time, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.Int32)" />,   
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},String,String,System.Int32)" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},Object,System.Int32)" /> , or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},String,String,Object,System.Int32)" /> method to specify the timeout value instead. You may want to increase the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para> 
        ///   <para>To designate a callback function that receives responses for all called SOA 
        /// operations, regardless of the type of the object that represents the response messages, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object)" /> method instead.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.String,System.String,System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.String,System.String,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" 
        /// /> 
        public void SetResponseHandler<TMessage>(BrokerResponseStateHandler<TMessage> callback, object state)
        {
            Utility.ThrowIfNull(callback, "callback");

            this.SetResponseHandler<TMessage>(callback, state, defaultResponsesTimeout, !this.EnableIsLastResponseProperty);
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the form of objects 
        /// of the specified type from the service-oriented architecture (SOA) service, subject to the specified timeout period.</para>
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds that the callback function should 
        /// wait between successive responses or for an indicator that no more responses will be sent generating an exception.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the response message that you want the callback function to receive. You create a TMessage 
        /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <exception cref="System.TimeoutException">
        ///   <para>The callback function reached the end of the specified timeout period 
        /// before receiving a new response or an indicator that no more responses will be sent.</para>
        /// </exception>
        /// <remarks>
        ///   <para>If the sessions for your HPC cluster are queued for long periods of time, you may want to increase 
        /// the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para>
        ///   <para>To use the default timeout period specified by the serviceOperationTimeout 
        /// setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" /> method instead.</para> 
        ///   <para>To designate a callback function that includes a parameter for a state object that you want 
        /// to pass to the callback function each time it is called, subject to a timeout period, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,System.Int32)" />   or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},String,String,System.Object,System.Int32)" /> method.</para> 
        ///   <para>To designate a callback function along with the SOAP actions for requests and responses, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},String,String,System.Int32)" /> or
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},String,String,System.Object,System.Int32)" /> method.</para> 
        ///   <para>To designate a callback function that receives responses for all called SOA 
        /// operations, regardless of the type of the object that represents the response messages, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" /> method instead.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.String,System.String,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.String,System.String,System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" 
        /// /> 
        public void SetResponseHandler<TMessage>(BrokerResponseHandler<TMessage> callback, int timeoutMilliseconds)
        {
            this.SetResponseHandler<TMessage>(callback, timeoutMilliseconds, !this.EnableIsLastResponseProperty);
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the 
        /// form of objects of the specified type from the service-oriented architecture (SOA)  
        /// service, along with a state object that you want to pass to 
        /// the callback function each time it is called, subject to the specified timeout period.</para> 
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <param name="state">
        ///   <para>A state object that you want to pass to the callback function each time it is called. The function that the 
        /// <paramref name="callback" /> parameter specifies must include a parameter for this object. You can use this object to pass the instance of the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class or other state information to the callback function.</para>
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds that the callback function should 
        /// wait between successive responses or for an indicator that no more responses will be sent generating an exception.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the response message that you want the callback function to receive. You create a TMessage 
        /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <exception cref="System.TimeoutException">
        ///   <para>The callback function reached the end of the specified timeout period 
        /// before receiving a new response or an indicator that no more responses will be sent.</para>
        /// </exception>
        /// <remarks>
        ///   <para>If the sessions for your HPC cluster are queued for long periods of time, you may want to increase 
        /// the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para>
        ///   <para>To use the default timeout period specified by the serviceOperationTimeout 
        /// setting in the configuration file used to register the service, use the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" /> or  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" /> method instead.</para> 
        ///   <para>To designate a callback function that receives responses for all called SOA 
        /// operations, regardless of the type of the object that represents the response messages, use the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object,System.Int32)" /> method instead.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.String,System.String,System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.String,System.String,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" 
        /// /> 
        public void SetResponseHandler<TMessage>(BrokerResponseStateHandler<TMessage> callback, object state, int timeoutMilliseconds)
        {
            this.SetResponseHandler<TMessage>(callback, state, timeoutMilliseconds, !this.EnableIsLastResponseProperty);
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the form of objects of the specified type from 
        /// the service-oriented architecture (SOA) service, along with the SOAP actions 
        /// for the request and responses, subject to the specified timeout period.</para> 
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <param name="action">
        ///   <para>String that specifies a SOAP action for the request message if the 
        /// appropriate action cannot be derived from the type of the request message. SOAP actions are  
        /// defined in the .wdsl file that is created when you add a service reference 
        /// to the Visual Studio project for the client application or by running the svcutil tool.</para> 
        /// </param>
        /// <param name="replyAction">
        ///   <para>String that specifies a SOAP action for the response message if the 
        /// appropriate action cannot be derived from the type of the response message. SOAP actions are  
        /// defined in the .wdsl file that is created when you add a service reference 
        /// to the Visual Studio project for the client application or by running the svcutil tool.</para> 
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds that the callback function should 
        /// wait between successive responses or for an indicator that no more responses will be sent generating an exception.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the response message that you want the callback function to receive. You create a TMessage 
        /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <exception cref="System.TimeoutException">
        ///   <para>The callback function reached the end of the specified timeout period 
        /// before receiving a new response or an indicator that no more responses will be sent.</para>
        /// </exception>
        /// <remarks>
        ///   <para>If the sessions for your HPC cluster are queued for long periods of time, you may want to increase 
        /// the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para>
        ///   <para>To use the default timeout period specified by the serviceOperationTimeout 
        /// setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" /> method instead.</para> 
        ///   <para>To designate a callback function that includes a parameter for a state object that you want 
        /// to pass to the callback function each time it is called, subject to a timeout period, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,int)" /> or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},string,string,System.Object,int)" /> method.</para> 
        ///   <para>To designate a callback without specifying the SOAP actions for requests and responses, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage}, int)" />,   
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" /> or
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object, int)" /> method.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.String,System.String,System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.String,System.String,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" 
        /// /> 
        public void SetResponseHandler<TMessage>(BrokerResponseHandler<TMessage> callback, string action, string replyAction, int timeoutMilliseconds)
        {
            Utility.ThrowIfNull(callback, "callback");
            Utility.ThrowIfNullOrEmpty(action, "action");
            Utility.ThrowIfNullOrEmpty(replyAction, "replyAction");
            Utility.ThrowIfInvalidTimeout(timeoutMilliseconds, "timeoutMilliseconds");

            lock (this.objectLock)
            {
                this.CheckDisposed();
                this.CheckBrokerAvailability();
                this.CheckResponseCallback();

                AsyncResponseCallback<TMessage> asyncCallback = new AsyncResponseCallback<TMessage>(this.session,
                                this.frontendFactory.GetResponseServiceClient(), this.callbackManager,
                                this, this.clientId, action, replyAction, callback, null, null, !this.EnableIsLastResponseProperty);

                asyncCallback.Listen(timeoutMilliseconds);
                this.asyncResponseCallback = asyncCallback;
            }
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the form of 
        /// objects of the specified type from the service-oriented architecture (SOA) service, along with a state  
        /// object that you want to pass to the callback function each time it is 
        /// called and the SOAP actions for the request and responses, subject to the specified timeout period.</para> 
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <param name="action">
        ///   <para>String that specifies a SOAP action for the request message if the 
        /// appropriate action cannot be derived from the type of the request message. SOAP actions are  
        /// defined in the .wdsl file that is created when you add a service reference 
        /// to the Visual Studio project for the client application or by running the svcutil tool.</para> 
        /// </param>
        /// <param name="replyAction">
        ///   <para>String that specifies a SOAP action for the response message if the 
        /// appropriate action cannot be derived from the type of the response message. SOAP actions are  
        /// defined in the .wdsl file that is created when you add a service reference 
        /// to the Visual Studio project for the client application or by running the svcutil tool.</para> 
        /// </param>
        /// <param name="state">
        ///   <para>A state object that you want to pass to the callback function each time it is called. The function that the 
        /// <paramref name="callback" /> parameter specifies must include a parameter for this object. You can use this object to pass the instance of the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class or other state information to the callback function.</para>
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds that the callback function should 
        /// wait between successive responses or for an indicator that no more responses will be sent generating an exception.</para>
        /// </param>
        /// <typeparam name="TMessage">
        ///   <para>The type of the response message that you want the callback function to receive. You create a TMessage 
        /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </typeparam>
        /// <exception cref="System.TimeoutException">
        ///   <para>The callback function reached the end of the specified timeout period 
        /// before receiving a new response or an indicator that no more responses will be sent.</para>
        /// </exception>
        /// <remarks>
        ///   <para>If the sessions for your HPC cluster are queued for long periods of time, you may want to increase 
        /// the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para>
        ///   <para>To use the default timeout period specified by the serviceOperationTimeout 
        /// setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" /> or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" /> method instead.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}(System.String,System.String,System.Int32)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.String,System.String,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" 
        /// /> 
        public void SetResponseHandler<TMessage>(BrokerResponseStateHandler<TMessage> callback, string action, string replyAction, object state, int timeoutMilliseconds)
        {
            Utility.ThrowIfNull(callback, "callback");
            Utility.ThrowIfNullOrEmpty(action, "action");
            Utility.ThrowIfNullOrEmpty(replyAction, "replyAction");
            Utility.ThrowIfInvalid(timeoutMilliseconds > 0 || timeoutMilliseconds == Timeout.Infinite, "waitTimeoutMilliseconds");

            lock (this.objectLock)
            {
                this.CheckDisposed();
                this.CheckBrokerAvailability();
                this.CheckResponseCallback();

                AsyncResponseCallback<TMessage> asyncCallback = new AsyncResponseCallback<TMessage>(this.session,
                                this.frontendFactory.GetResponseServiceClient(), this.callbackManager,
                                this, this.clientId, action, replyAction, null, callback, state, !this.EnableIsLastResponseProperty);

                asyncCallback.Listen(timeoutMilliseconds);
                this.asyncResponseCallback = asyncCallback;
            }
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the form of 
        /// <see cref="System.Object" /> objects from the service-oriented architecture (SOA) service.</para>
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <remarks>
        ///   <para>The default length of time that the callback function waits for responses is the value specified by the serviceOperationTimeout setting in 
        /// the configuration file used to register the service. If the sessions for your HPC cluster are queued for long periods of time, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" /> or  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object,System.Int32)" /> method to specify the timeout value instead. You may want to increase the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para> 
        ///   <para>To designate a callback function that includes a parameter for a state object 
        /// that you want to pass to the callback function each time it is called, use the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object)" /> or  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object,System.Int32)" /> method.</para> 
        ///   <para>To designate a callback function that receives responses for the SOA operation that corresponds to 
        /// a specified type parameter, regardless of the type of the object that represents the response messages, use the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" /> method instead.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage})" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object,System.Int32)" 
        /// /> 
        public void SetResponseHandler(BrokerResponseHandler<Object> callback)
        {
            this.SetResponseHandler(callback, defaultResponsesTimeout, !this.EnableIsLastResponseProperty);
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the form 
        /// <see cref="System.Object" /> objects from the service-oriented architecture (SOA) service, along with a state object that you want to pass to the callback function each time it is called.</para> 
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <param name="state">
        ///   <para>A state object that you want to pass to the callback function each time it is called. The function that the 
        /// <paramref name="callback" /> parameter specifies must include a parameter for this object. You can use this object to pass the instance of the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class or other state information to the callback function.</para>
        /// </param>
        /// <remarks>
        ///   <para>The default length of time that the callback function waits for responses is the value specified by the serviceOperationTimeout setting in 
        /// the configuration file used to register the service. If the sessions for your HPC cluster are queued for long periods of time, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" /> or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object,System.Int32)" /> method to specify the timeout value instead. You may want to increase the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para> 
        ///   <para>To designate a callback function that does not include a parameter for a state 
        /// object that you want to pass to the callback function each time it is called, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" /> or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" /> method.</para> 
        ///   <para>To designate a callback function that receives responses for the SOA operation that corresponds to 
        /// a specified type parameter, regardless of the type of the object that represents the response messages, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" /> method instead.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" 
        /// /> 
        public void SetResponseHandler(BrokerResponseStateHandler<Object> callback, object state)
        {
            this.SetResponseHandler(callback, state, defaultResponsesTimeout, !this.EnableIsLastResponseProperty);
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the form of 
        /// <see cref="System.Object" /> objects from the service-oriented architecture (SOA) service, subject to the specified timeout period.</para>
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds that the callback function should 
        /// wait between successive responses or for an indicator that no more responses will be sent generating an exception.</para>
        /// </param>
        /// <exception cref="System.TimeoutException">
        ///   <para>The callback function reached the end of the specified timeout period 
        /// before receiving a new response or an indicator that no more responses will be sent.</para>
        /// </exception>
        /// <remarks>
        ///   <para>If the sessions for your HPC cluster are queued for long periods of time, you may want to increase 
        /// the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para>
        ///   <para>To use the default timeout period specified by the serviceOperationTimeout 
        /// setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" /> or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object)" /> method instead.</para> 
        ///   <para>To designate a callback function that includes a parameter for a state object 
        /// that you want to pass to the callback function each time it is called, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object)" /> or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object,System.Int32)" /> method.</para> 
        ///   <para>To designate a callback function that receives responses for the SOA operation that corresponds to 
        /// a specified type parameter, regardless of the type of the object that represents the response messages, use the  
        /// "Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage} method instead.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{TMessage},System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object,System.Int32)" 
        /// /> 
        public void SetResponseHandler(BrokerResponseHandler<Object> callback, int timeoutMilliseconds)
        {
            this.SetResponseHandler<Object>(callback, timeoutMilliseconds, !this.EnableIsLastResponseProperty);
        }

        /// <summary>
        ///   <para>Designates the callback function that should receive responses in the form 
        /// <see cref="System.Object" /> objects from the service-oriented architecture (SOA) service, along with a state object that you want to pass to the callback function each time it is called, subject to the specified timeout period.</para> 
        /// </summary>
        /// <param name="callback">
        ///   <para>A function that implements the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{T}" /> delegate that you want to designate as the callback function to receive responses from the SOA service.</para> 
        /// </param>
        /// <param name="state">
        ///   <para>A state object that you want to pass to the callback function each time it is called. The function that the 
        /// <paramref name="callback" /> parameter specifies must include a parameter for this object. You can use this object to pass the instance of the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class or other state information to the callback function.</para>
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds that the callback function should 
        /// wait between successive responses or for an indicator that no more responses will be sent generating an exception.</para>
        /// </param>
        /// <exception cref="System.TimeoutException">
        ///   <para>The callback function reached the end of the specified timeout period 
        /// before receiving a new response or an indicator that no more responses will be sent.</para>
        /// </exception>
        /// <remarks>
        ///   <para>If the sessions for your HPC cluster are queued for long periods of time, you may want to increase 
        /// the value to the sum of the current value of the serviceOperationTimeout setting and the estimated amount of time that sessions are queued.</para>
        ///   <para>To use the default timeout period specified by the serviceOperationTimeout 
        /// setting in the configuration file used to register the service, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" /> or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object)" /> method instead.</para> 
        ///   <para>To designate a callback function that does not include a parameter for a state 
        /// object that you want to pass to the callback function each time it is called, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" /> or  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" /> method.</para> 
        ///   <para>To designate a callback function that receives responses for the SOA operation that corresponds to 
        /// a specified type parameter, regardless of the type of the object that represents the response messages, use the  
        /// "Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage} method instead.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object},System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{TContract}.SetResponseHandler{TMessage}(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{TMessage},System.Object,System.Int32)" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseStateHandler{System.Object},System.Object)" 
        /// /> 
        public void SetResponseHandler(BrokerResponseStateHandler<Object> callback, object state, int timeoutMilliseconds)
        {
            this.SetResponseHandler<Object>(callback, state, timeoutMilliseconds, !this.EnableIsLastResponseProperty);
        }

        /// <summary>
        /// Asynchronously gets responses
        /// </summary>
        /// <typeparam name="TMessage">Response's type</typeparam>
        /// <param name="callback">Callback that receives responses</param>
        /// <param name="timeoutMilliseconds">How long to wait for next batch of responses before timing out</param>
        /// <param name="ignoreIsLastResponse">Ignore the IsLastResponse property</param>
        private void SetResponseHandler<TMessage>(BrokerResponseHandler<TMessage> callback, int timeoutMilliseconds, bool ignoreIsLastResponse)
        {
            Utility.ThrowIfNull(callback, "callback");
            Utility.ThrowIfInvalidTimeout(timeoutMilliseconds, "timeoutMilliseconds");

            lock (this.objectLock)
            {
                this.CheckDisposed();
                this.CheckBrokerAvailability();
                this.CheckResponseCallback();

                AsyncResponseCallback<TMessage> asyncCallback = new AsyncResponseCallback<TMessage>(this.session,
                                this.frontendFactory.GetResponseServiceClient(), this.callbackManager,
                                this, this.clientId, null, null, callback, null, null, ignoreIsLastResponse);

                asyncCallback.Listen(timeoutMilliseconds);
                this.asyncResponseCallback = asyncCallback;
                SessionBase.TraceSource.TraceInformation("[Session:{0}] SetResponseHandler called for client {1}.", this.session.Id, this.clientId);
            }
        }

        /// <summary>
        /// Asynchronously gets responses
        /// </summary>
        /// <typeparam name="TMessage">Response's type</typeparam>
        /// <param name="callback">Callback that receives responses</param>
        /// <param name="state">response handler state</param>
        /// <param name="timeoutMilliseconds">How long to wait for next batch of responses before timing out</param>
        /// <param name="ignoreIsLastResponse">Ignore the IsLastResponse property</param>
        private void SetResponseHandler<TMessage>(BrokerResponseStateHandler<TMessage> callback, object state, int timeoutMilliseconds, bool ignoreIsLastResponse)
        {
            Utility.ThrowIfNull(callback, "callback");
            Utility.ThrowIfInvalidTimeout(timeoutMilliseconds, "TimeoutMilliseconds");

            lock (this.objectLock)
            {
                this.CheckDisposed();
                this.CheckBrokerAvailability();
                this.CheckResponseCallback();

                AsyncResponseCallback<TMessage> asyncCallback = new AsyncResponseCallback<TMessage>(this.session,
                                this.frontendFactory.GetResponseServiceClient(), this.callbackManager,
                                this, this.clientId, null, null, null, callback, state, ignoreIsLastResponse);

                asyncCallback.Listen(timeoutMilliseconds);
                this.asyncResponseCallback = asyncCallback;
                SessionBase.TraceSource.TraceInformation("[Session:{0}] SetResponseHandler called for client {1}.", this.session.Id, this.clientId);
            }
        }

        /// <summary>
        ///   <para>Gets the status of the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.BrokerClientStatus" /> that indicates the status of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object.</para>
        /// </returns>
        public BrokerClientStatus GetStatus()
        {
            lock (this.objectLock)
            {
                this.CheckDisposed();

                IController controller = this.frontendFactory.GetControllerClient();
                try
                {
                    return controller.GetBrokerClientStatus(this.clientId);
                }
                catch (FaultException<SessionFault> e)
                {
                    throw Utility.TranslateFaultException(e);
                }
            }
        }

        /// <summary>
        ///   <para>Gets the number of requests that have been sent to this <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" />.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of requests that have been sent to this <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /></para>
        /// </value>
        public int RequestsCount
        {
            get
            {
                lock (this.objectLock)
                {
                    this.CheckDisposed();

                    IController controller = this.frontendFactory.GetControllerClient();
                    try
                    {
                        return controller.GetRequestsCount(this.clientId);
                    }
                    catch (ActionNotSupportedException)
                    {
                        //
                        // Catch ActionNotSupportedException in case broker node is not upgraded to latest version (>=v3.1).
                        //
                        throw new SessionException(SOAFaultCode.Broker_UnsupportedOperation, String.Format(CultureInfo.CurrentCulture, SR.Broker_UnsupportedOperation, "GetRequestsCount"));
                    }
                    catch (FaultException<SessionFault> e)
                    {
                        throw Utility.TranslateFaultException(e);
                    }
                }
            }
        }

        /// <summary>
        /// GetResponses for the current session (internal version, no param check)
        /// </summary>
        /// <typeparam name="TMessage">Response's type</typeparam>
        /// <param name="action">Action of typed message if type is ambiguous</param>
        /// <param name="replyAction">Reply action of typed message if type is ambiguous</param>
        /// <param name="waitForResponsesTimeout">How long to wait for responses (in millseonds)</param>
        /// <returns>returns an instance of the BrokerResponseEnumerator class</returns>
        private BrokerResponseEnumerator<TMessage> GetResponsesInternal<TMessage>(string action, string replyAction, int waitTimeoutMilliseconds)
        {
            TimeSpan timeout;
            if (waitTimeoutMilliseconds != Timeout.Infinite)
            {
                timeout = TimeSpan.FromMilliseconds(waitTimeoutMilliseconds);
            }
            else
            {
                // Cannot set to TimeSpan.MaxValue as WaitHandle.WaitAny would throw
                // ArgumentException as it is in fact long.MaxValue
                timeout = TimeSpan.FromMilliseconds(int.MaxValue - 1);
            }

            lock (this.objectLock)
            {
                this.CheckDisposed();
                this.CheckBrokerAvailability();

                return new BrokerResponseEnumerator<TMessage>(this.session, this.frontendFactory.GetResponseServiceClient(), this.callbackManager, timeout, this, this.clientId, action, replyAction);
            }
        }

        /// <summary>
        /// Returns the message description of the specified type
        /// </summary>
        /// <param name="messageType">Message Type</param>
        /// <param name="messageDirection">Whether its input or output</param>
        /// <returns></returns>
        private MessageDescription GetMessageDescription(Type messageType, MessageDirection messageDirection)
        {
            MessageDescription ret = null;

            foreach (OperationDescription operationDescription in this.operations)
            {
                foreach (MessageDescription messageDescription in operationDescription.Messages)
                {
                    if (messageDescription.Direction == messageDirection &&
                        messageDescription.MessageType == messageType)
                    {
                        if (ret != null)
                        {
                            throw new InvalidOperationException(SR.AmbiguousOperation);
                        }

                        ret = messageDescription;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Returns the message description of the specified type
        /// </summary>
        /// <param name="messageType">Message Type</param>
        /// <param name="messageDirection">Whether its input or output</param>
        /// <param name="action">Message action that identifies intent of the message</param>
        /// <returns></returns>
        private MessageDescription GetMessageDescription(Type messageType, MessageDirection messageDirection, string action)
        {
            foreach (OperationDescription operationDescription in this.operations)
            {
                foreach (MessageDescription messageDescription in operationDescription.Messages)
                {
                    if (messageDescription.Direction == messageDirection &&
                        messageDescription.MessageType == messageType &&
                        string.Equals(messageDescription.Action, action, StringComparison.OrdinalIgnoreCase))
                    {
                        return messageDescription;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns default binding to use for the specified transport scheme
        /// </summary>
        /// <returns></returns>
        private Binding GetDefaultClientBinding()
        {
            Binding binding = null;

            if ((this.session.Info.TransportScheme == TransportScheme.WebAPI))
            {
                return null;
            }

            if ((this.session.Info.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                binding = new NetTcpBinding(this.session.Info.Secure ? SecurityMode.Transport : SecurityMode.None);
                if (this.session.Info.Secure && this.session.Info.IsAadOrLocalUser)
                {
                    if (this.session.Info.UseAad)
                    {
                        ((NetTcpBinding)binding).Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
                    }
                    else if (this.session.Info.LocalUser)
                    {
                        ((NetTcpBinding)binding).Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
                    }

                    ((NetTcpBinding)binding).Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
                }

                binding.ReceiveTimeout = this.defaultResponsesTimeout == Timeout.Infinite ? TimeSpan.MaxValue : new TimeSpan(0, 0, 0, 0, this.defaultResponsesTimeout);
                binding.SendTimeout = new TimeSpan(0, 0, 0, 0, this.defaultSendTimeout);
            }
            else if ((this.session.Info.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                binding = BindingHelper.GetBinding(TransportScheme.NetHttp, this.session.Info.Secure);
                binding.ReceiveTimeout = this.defaultResponsesTimeout == Timeout.Infinite ? TimeSpan.MaxValue : new TimeSpan(0, 0, 0, 0, this.defaultResponsesTimeout);
                binding.SendTimeout = new TimeSpan(0, 0, 0, 0, this.defaultSendTimeout);
            }
            else if ((this.session.Info.TransportScheme & TransportScheme.Http) == TransportScheme.Http)
            {
                binding = BindingHelper.GetBinding(TransportScheme.Http, this.session.Info.Secure);
                binding.ReceiveTimeout = this.defaultResponsesTimeout == Timeout.Infinite ? TimeSpan.MaxValue : new TimeSpan(0, 0, 0, 0, this.defaultResponsesTimeout);
                binding.SendTimeout = binding.ReceiveTimeout; // send timeout for http should be the same as receive timeout to pull the response
            }
            else if ((this.session.Info.TransportScheme & TransportScheme.Custom) == TransportScheme.Custom)
            {
                throw new SessionException(SOAFaultCode.MustIndicateBindingForCustomTransportScheme, SR.MustIndicateBindingForCustomTransportScheme);
            }
            else
            {
                throw new InvalidOperationException();
            }

            return binding;
        }

        /// <summary>
        /// Returns the binding based on specified configuration name
        /// </summary>
        /// <param name="configurationName">Name of the binding configuration to load</param>
        /// <returns></returns>
        private Binding GetConfiguredClientBinding(string configurationName, bool secure)
        {
            Binding binding = null;

            switch (this.session.Info.TransportScheme)
            {
                case (TransportScheme)0x8:
                    binding = new NetNamedPipeBinding(configurationName);
                    break;
                case TransportScheme.NetTcp:
                    binding = new NetTcpBinding(configurationName);
                    break;
#if !net40
                case TransportScheme.NetHttp:
                    if (secure)
                    {
                        binding = new NetHttpsBinding(configurationName);
                    }
                    else
                    {
                        binding = new NetHttpBinding(configurationName);
                    }
                    break;
#endif
                case TransportScheme.Http:
                    binding = new BasicHttpBinding(configurationName);
                    break;
            }

            return binding;
        }

        /// <summary>
        ///   <para>Releases all of the resources that the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object used.</para>
        /// </summary>
        /// <param name="disposing">
        ///   <para>
        ///     <see cref="System.Boolean" /> that specifies whether to release managed resources in addition to unmanaged resources. 
        /// True indicates that the managed and unmanaged resources should be released because the code is calling the method directly. 
        /// False indicates that only the unmanaged resources can be disposed of because the method is being called by the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Finalize" /> method.</para>
        /// </param>
        /// <remarks>
        ///   <para>You must dispose of an 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object when you finish using it by calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close" /> method or by creating the object by calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(Microsoft.Hpc.Scheduler.Session.SessionBase)" /> constructor within a 
        /// <see href="http://go.microsoft.com/fwlink/?LinkId=177731">using Statement</see> (http://go.microsoft.com/fwlink/?LinkId=177731) in C#. The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close" /> method in turn calls the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Dispose(System.Boolean)" /> method.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close" />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.objectLock)
                {
                    // first close frontendFactory sot that it stops sending responses to
                    // callbackManager.
                    try
                    {
                        if (this.frontendFactory != null)
                        {
                            this.frontendFactory.Close();
                            this.frontendFactory = null;
                        }
                    }
                    catch (Exception e)
                    {
                        // Swallow the exception
                        SessionBase.TraceSource.TraceData(System.Diagnostics.TraceEventType.Warning, 0, e.ToString());
                    }

                    if (this.asyncResponseCallback != null)
                    {
                        this.asyncResponseCallback.Close();
                        this.asyncResponseCallback = null;
                    }

                    // then close callbackManager
                    try
                    {

                        if (this.callbackManager != null)
                        {
                            this.callbackManager.Close();
                            this.callbackManager = null;
                        }
                    }
                    catch (Exception e)
                    {
                        // Swallow the exception
                        SessionBase.TraceSource.TraceData(System.Diagnostics.TraceEventType.Warning, 0, e.ToString());
                    }

                    this.session = null;
                }
            }
        }

        /// <summary>
        ///   <para>Informs the broker that it should commit all request messages, subject to the default timeout period.</para>
        /// </summary>
        /// <exception cref="System.TimeoutException">
        ///   <para>The broker did not commit the request messages within the default timeout period.</para>
        /// </exception>
        /// <remarks>
        ///   <para>The default length of time that the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush()" /> method waits for the broker to commit the request messages is 60,000 milliseconds. To specify a different length of time, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush(System.Int32)" /> method instead.</para>
        ///   <para>The difference between the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush()" /> method and the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" /> method: The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush()" /> method commits all pending request messages, and allows further request messages to be sent after the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush()" /> method was called. The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" /> method commits all pending request messages, and does not allow further request messages to be sent.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush(System.Int32)" />
        public void Flush()
        {
            Flush(Constant.EOMTimeoutMS);
        }

        /// <summary>
        ///   <para>Informs the broker that it should commit all request messages, subject to the specified timeout period.</para>
        /// </summary>
        /// <param name="timeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds 
        /// that the method should wait for the broker to commit the request messages.</para>
        /// </param>
        /// <exception cref="System.TimeoutException">
        ///   <para>The broker did not commit the request messages within the specified timeout period.</para>
        /// </exception>
        /// <remarks>
        ///   <para>To use the default timeout period of 60,000 milliseconds, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush()" /> method instead.</para>
        ///   <para>The difference between the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush(System.Int32)" /> method and the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" /> method: The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush(System.Int32)" /> method commits all pending request messages, and allows further request messages to be sent after the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush(System.Int32)" /> method was called. The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" /> method commits all pending request messages, and does not allow further request messages to be sent.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush()" />
        public void Flush(int timeoutMilliseconds)
        {
            Flush(timeoutMilliseconds, /*endOfMessage =*/false);
        }

        /// <summary>
        ///   <para>Informs the broker that this instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class is finished sending request messages and that the broker should commit all request messages, subject to the default timeout period.</para> 
        /// </summary>
        /// <exception cref="System.TimeoutException">
        ///   <para>The broker did not commit the request messages within the default timeout period.</para>
        /// </exception>
        /// <remarks>
        ///   <para>The default length of time that the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" /> method waits for the broker to commit the request messages is 60,000 milliseconds. To specify a different length of time, use the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" /> method instead.</para>
        ///   <para>The difference between the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush()" /> method and the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" /> method: The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush()" /> method commits all pending request messages, and allows further request messages to be sent after the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush()" /> method was called. The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" /> method commits all pending request messages, and does not allow further request messages to be sent.</para> 
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> object and a 
        /// <see cref="HpcDurableSession" /> object handle requests differently. A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> object may begin processing requests before the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(0" /> method is called. However, a 
        /// <see cref="HpcDurableSession" /> object will not process requests until the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" /> method is called. After the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" /> method is called, the 
        /// <see cref="HpcDurableSession" /> object will commit the requests to the message queue.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" />
        public void EndRequests()
        {
            EndRequests(Constant.EOMTimeoutMS);
        }

        /// <summary>
        ///   <para>Informs the broker that this instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class is finished sending request messages and that the broker should commit all request messages, subject to the specified timeout period.</para> 
        /// </summary>
        /// <param name="timeoutMilliseconds">
        ///   <para>Integer that specifies the length of time in milliseconds 
        /// that the method should wait for the broker to commit the request messages.</para>
        /// </param>
        /// <exception cref="System.TimeoutException">
        ///   <para>The broker did not commit the request messages within the specified timeout period.</para>
        /// </exception>
        /// <remarks>
        ///   <para>To use the default timeout period of 60,000 milliseconds, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method instead.</para>
        ///   <para>The difference between the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush(System.Int32)" /> method and the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" /> method: The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush(System.Int32)" /> method commits all pending request messages, and allows further request messages to be sent after the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush(System.Int32)" /> method was called. The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" /> method commits all pending request messages, and does not allow further request messages to be sent.</para> 
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> object and a 
        /// <see cref="HpcDurableSession" /> object handle requests differently. A 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session" /> object may begin processing requests before the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" /> method is called. However, a 
        /// <see cref="HpcDurableSession" /> object will not process requests until the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" /> method is called. After the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" /> method is called, the 
        /// <see cref="HpcDurableSession" /> object will commit the requests to the message queue.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" />
        public void EndRequests(int timeoutMilliseconds)
        {
            Flush(timeoutMilliseconds, /*endOfMessage =*/true);
        }

        /// <summary>
        /// Commit uncommitted requetss to broker's store.
        /// </summary>
        /// <param name="timeoutMilliseconds"></param>
        /// <param name="endOfMessage"></param>
        private void Flush(int timeoutMilliseconds, bool endOfMessage)
        {
            TimeSpan operationTimeout;
            Utility.ThrowIfInvalidTimeout(timeoutMilliseconds, "timeoutMilliseconds");

            bool isFailed = false;
            try
            {
                lock (this.objectLock)
                {
                    this.CheckDisposed();
                    this.CheckBrokerAvailability();

                    // If user explicitly specified SendTimeout use it. Otherwise use default sendtimeout
                    int sendTimeout = this.lastSendTimeoutMS.HasValue ? this.lastSendTimeoutMS.Value : this.defaultSendTimeout;

                    // If the sendTimeout is positive, use it for the throttling timeout. Else use what caller supplied for EOM timeout
                    int timeoutThrottlingMs = ((int)sendTimeout > 0) ? (int)sendTimeout : timeoutMilliseconds;

                    if (timeoutMilliseconds == Timeout.Infinite || timeoutThrottlingMs == Timeout.Infinite)
                    {
                        operationTimeout = TimeSpan.MaxValue;
                    }
                    else
                    {
                        // Pick the larger of the two timeouts to ensure operationTimeout is long enough
                        int largerTimeout = Math.Max(timeoutMilliseconds, timeoutThrottlingMs);
                        double dTimeoutMilliseconds = Math.Max(largerTimeout * 1.10, Constant.MinOperationTimeout);

                        if (dTimeoutMilliseconds >= Int32.MaxValue)
                        {
                            operationTimeout = TimeSpan.MaxValue;
                        }
                        else
                        {
                            operationTimeout = TimeSpan.FromMilliseconds(dTimeoutMilliseconds);
                        }
                    }

                    SessionBase.TraceSource.TraceInformation("[Session:{0}] Flush client {1}. EndOfMessage = {2}, Timeout = {3}.", this.session.Id, this.clientId, endOfMessage, operationTimeout.TotalMilliseconds);
                    if (!endOfMessage)
                    {
                        // for Flush, do nothing if there is no request waiting for commit
                        if (this.uncommittedCount == 0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        // for EndRequests, there must be at least one request in the client before calling EndRequests.
                        if (this.sendCount == 0)
                        {
                            throw new InvalidOperationException(SR.InvalidEndRequestsCount);
                        }
                    }

                    // Get a connection to the broker controller
                    IController controllerProxy = this.frontendFactory.GetControllerClient(operationTimeout);

                    // If controller proxy implemented IControllerAsync, use the async version to call EndRequests() so client can fail if broker goes down immmediately.
                    // This is mainly for normal v3 broker.
                    // If controller proxy didn't implemented IControllerAsync, use the sync version. This is for inprocess broker since async is not needed.
                    if (controllerProxy is IControllerAsync)
                    {
                        IControllerAsync controller = (IControllerAsync)controllerProxy;

                        // Start an async call to Flush/EndRequests so client can fail if broker goes down w/o waiting for timeout. It also 
                        //  works around the System.Net .Net 3.5 RTM race condition.
                        IAsyncResult ar;
                        if (!endOfMessage)
                        {
                            ar = controller.BeginFlush(this.uncommittedCount, clientId, this.batchId, timeoutThrottlingMs, timeoutMilliseconds, null, null);
                        }
                        else
                        {
                            ar = controller.BeginEndRequests(this.uncommittedCount, clientId, this.batchId, timeoutThrottlingMs, timeoutMilliseconds, null, null);
                        }

                        // Wait for the EndRequests call to complete OR heartbeat to signal
                        int index = WaitHandle.WaitAny(new WaitHandle[] { ar.AsyncWaitHandle, this.session.HeartbeatSignaledEvent });

                        // If EndRequests completed
                        if (index == 0)
                        {
                            // Reset the heartbeat since operation just succeeded
                            this.session.ResetHeartbeat();

                            // Get the result
                            if (!endOfMessage)
                            {
                                try
                                {
                                    controller.EndFlush(ar);
                                }
                                catch (ActionNotSupportedException)
                                {
                                    //
                                    // Catch ActionNotSupportedException in case broker node is not upgraded to latest version (>=v3.1).
                                    //
                                    throw new SessionException(SOAFaultCode.Broker_UnsupportedOperation, String.Format(CultureInfo.CurrentCulture, SR.Broker_UnsupportedOperation, "Flush"));
                                }

                                this.uncommittedCount = 0;
                                SessionBase.TraceSource.TraceInformation("[Session:{0}] Flush succeeded for client {1}.", this.session.Id, this.clientId);
                            }
                            else
                            {
                                controller.EndEndRequests(ar);
                                this.uncommittedCount = 0;

                                // Close the connection used to send requests since it is no longer needed
                                this.frontendFactory.CloseBrokerClient(true, -1);

                                this.endRequests = true;
                                SessionBase.TraceSource.TraceInformation("[Session:{0}] EndRequests succeeded for client {1}.", this.session.Id, this.clientId);
                            }

                        }

                        // Else if the heartbeat signaled
                        else if (index == 1)
                        {
                            SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, String.Format("[Session:{0}] Heartbeat lost when flushing request for client {1}.", this.session.Id, this.clientId));
                            throw SessionBase.GetHeartbeatException(this.session.IsBrokerNodeUnavailable);
                        }
                    }
                    else
                    {
                        if (!endOfMessage)
                        {
                            try
                            {
                                controllerProxy.Flush(this.uncommittedCount, clientId, this.batchId, timeoutMilliseconds, timeoutMilliseconds);
                            }
                            catch (ActionNotSupportedException)
                            {
                                //
                                // Catch ActionNotSupportedException in case broker node is not upgraded to latest version (>=v3.1).
                                //
                                throw new SessionException(SOAFaultCode.Broker_UnsupportedOperation, String.Format(CultureInfo.CurrentCulture, SR.Broker_UnsupportedOperation, "Flush"));
                            }

                            this.uncommittedCount = 0;
                            SessionBase.TraceSource.TraceInformation("[Session:{0}] Flush succeeded for client {1}.", this.session.Id, this.clientId);
                        }
                        else
                        {
                            controllerProxy.EndRequests(this.uncommittedCount, clientId, this.batchId, timeoutThrottlingMs, timeoutMilliseconds);
                            this.uncommittedCount = 0;
                            this.frontendFactory.CloseBrokerClient(true, -1);
                            this.endRequests = true;
                            SessionBase.TraceSource.TraceInformation("[Session:{0}] EndRequests succeeded for client {1}.", this.session.Id, this.clientId);
                        }
                    }
                }
            }
            catch (FaultException<SessionFault> e)
            {
                isFailed = true;
                throw Utility.TranslateFaultException(e);
            }
            catch (WebException e)
            {
                isFailed = true;
                Utility.HandleWebException(e);
            }
            catch (Exception ex)
            {
                SessionBase.TraceSource.TraceInformation("[Session:{0}] Unknow Exception in Flush for client{1}: {2}.", this.session.Id, this.clientId, ex.ToString());
                isFailed = true;
                throw;
            }
            finally
            {
                if (isFailed && IsDurableSession(this.session))
                {
                    this.DiscardUnflushedRequests();
                    this.SetSendRequestFailedFlag();
                }
            }
        }

        /// <summary>
        /// Discard unflushed requests at client side.
        /// </summary>
        private void DiscardUnflushedRequests()
        {
            // roll back counters for requests that have been sent out but not flushed (via Flush/EndRequests)
            Interlocked.Add(ref this.sendCount, -this.uncommittedCount);
            this.uncommittedCount = 0;
        }

        /// <summary>
        /// Set "sendRequestFailedFlag" to true
        /// </summary>
        private void SetSendRequestFailedFlag()
        {
            lock (this.lockSendRequestFailedFlag)
            {
                this.sendRequestFailedFlag = true;
            }
        }

        /// <summary>
        /// Check sendRequestFailedFlag
        /// </summary>
        /// <returns>return true if sendRequestFailedFlag was true and set to false; false otherwise</returns>
        private bool CheckAndClearSendRequestFailedFlag()
        {
            if (this.sendRequestFailedFlag)
            {
                lock (this.lockSendRequestFailedFlag)
                {
                    if (this.sendRequestFailedFlag)
                    {
                        this.batchId = Interlocked.Increment(ref SessionBase.BatchId);

                        // update instanceIdHeader
                        this.instanceIdHeader = MessageHeader.CreateHeader(Constant.ClientInstanceIdHeaderName, Constant.HpcHeaderNS, this.batchId);

                        // clear sendRequestFailedFlag
                        this.sendRequestFailedFlag = false;

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check response callback for registered response handler
        /// </summary>
        private void CheckResponseCallback()
        {
            if (this.CheckAndClearSendRequestFailedFlag())
            {
                if (this.asyncResponseCallback != null)
                {
                    this.asyncResponseCallback.Close();
                    this.asyncResponseCallback = null;
                }
            }
        }

        /// <summary>
        /// Check if a session is durable or not
        /// </summary>
        private static bool IsDurableSession(SessionBase session)
        {
            return session is DurableSession;
        }

        /// <summary>
        ///   <para>Closes the connection to the broker without emptying or deleting the message queues that correspond to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object.</para>
        /// </summary>
        /// <remarks>
        ///   <para>This method is equivalent to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close(System.Boolean)" /> method with the purge parameter set to 
        /// False. To empty and delete the message queues that correspond to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object when you close the connection to the broker, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close(System.Boolean)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close(System.Boolean,System.Int32)" /> method instead. </para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close(System.Boolean)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close(System.Boolean,System.Int32)" />
        public void Close()
        {
            Close(false, Constant.PurgeTimeoutMS);
        }

        /// <summary>
        ///   <para>Closes the connection to the broker, and optionally empties and deletes the message queues that correspond to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object.</para>
        /// </summary>
        /// <param name="purge">
        ///   <para>Boolean that specifies whether to empty and delete the message queues that correspond to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object. 
        /// True indicates that the method should empty and delete the message queues. 
        /// False indicates that the method should not empty and delete the message queues.</para>
        /// </param>
        /// <remarks>
        ///   <para>Calling this method with the <paramref name="purge" /> parameter set to 
        /// False is equivalent to calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close" /> method.</para>
        ///   <para>The default timeout period for the emptying and deleting the message 
        /// queues is 60,000 milliseconds. To specify the length of the timeout period, use  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close(System.Boolean,System.Int32)" /> instead.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close()" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Close(System.Boolean,System.Int32)" />
        public void Close(bool purge)
        {
            Close(purge, Constant.PurgeTimeoutMS);
        }

        /// <summary>
        ///   <para>Closes the connection to the broker, and optionally empties and deletes the message queues that correspond to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object that is subject to the specified timeout period.</para>
        /// </summary>
        /// <param name="purge">
        ///   <para>A 
        /// <see cref="System.Boolean" /> object that specifies whether to empty and delete the message queues that correspond to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object. 
        /// True indicates that the method should empty and delete the message queues. 
        /// False indicates that the method should not empty and delete the message queues.</para>
        /// </param>
        /// <param name="timeoutMilliseconds">
        ///   <para>Specifies the length of time in milliseconds that the method should wait for the message queues to be emptied and deleted.</para>
        /// </param>
        public void Close(bool purge, int timeoutMilliseconds)
        {
            Utility.ThrowIfInvalidTimeout(timeoutMilliseconds, "timeoutMilliseconds");

            // BUG 9244 : Detach session and close broker client outside objectLock in case throttling is engaged

            try
            {
                // BUG 4968 : Detach from session immediately so this client doesnt get any more session heartbeat signals while closing
                DetachFromSession();
            }

            catch
            {
                // Since DetachFromSession is called outside objectLock, their references may be null. If so do not throw potential exception to the caller. Just continue.
            }

            try
            {
                // BUG 7344, 9774 : If EndRequests wasnt called, close the connection used to send requests to ensure throttling isnt enabled. This must be done outside
                //  the lock because SendRequest may have the lock and is blocked due to throttling. 
                if (!this.endRequests)
                {
                    if (this.frontendFactory != null)
                    {
                        // BUG 10196 : Need a shorter timeout to meet Close\Cancel CTQ. User may want to cancel a batch while throttling is engaged. Try closing the connection with
                        //  a shorter timeout (3s) and abort if the timeout expires. We dont want to just abort the connection since it leaves connections open on the broker side
                        //  until receiveTimeout expires.
                        this.frontendFactory.CloseBrokerClient(false, 3000);
                    }
                }
            }

            catch
            {
                // Since Close is called outside objectLock, their references may be null. If so do not throw potential exception to the caller. Just continue.
            }

            lock (this.objectLock)
            {
                if (purge)
                {
                    if (this.frontendFactory != null)
                    {
                        try
                        {
                            IController controller = this.frontendFactory.GetControllerClient(timeoutMilliseconds);
                            controller.Purge(clientId);
                        }
                        catch (FaultException<SessionFault> e)
                        {
                            throw Utility.TranslateFaultException(e);
                        }
                    }
                }

                // Remember user supplied close timeout (if any) and use in Dispose when colosing connections
                if (this.frontendFactory != null)
                {
                    this.frontendFactory.SetCloseTimeout(timeoutMilliseconds);
                }

                this.Dispose();
            }
        }

        /// <summary>
        /// Check if the object is disposed/closed
        /// </summary>
        private void CheckDisposed()
        {
            if (this.session == null)
                throw new ObjectDisposedException("BrokerClient");
        }

        /// <summary>
        /// Check if broker is available or not
        /// </summary>
        private void CheckBrokerAvailability()
        {
            // for interactive session, check if broker is available. if not, throw exception directly.
            // for durable session, BrokerClient is reliable, so always try to connect broker
            if (!IsDurableSession(this.session) && !this.session.IsBrokerAvailable)
            {
                throw SessionBase.GetHeartbeatException(this.session.IsBrokerNodeUnavailable);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Called by SessionBase when broker heartbeat signals
        /// </summary>
        public override void SendBrokerDownSignal(bool isBrokerNodeDown)
        {
            lock (this.objectLock)
            {
                // If there are callbacks, tell them to end
                if (this.callbackManager != null)
                {
                    this.callbackManager.SendBrokerDownSignal(isBrokerNodeDown);
                }
            }
        }

        #endregion
    }
}
