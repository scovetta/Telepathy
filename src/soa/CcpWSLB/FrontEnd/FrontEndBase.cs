// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.Common;
    using Microsoft.Hpc.ServiceBroker.Common.ThreadHelper;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    using SR = Microsoft.Hpc.SvcBroker.SR;

    /// <summary>
    /// Base class for all frontends
    /// </summary>
    internal abstract class FrontEndBase : ReferenceObject
    {
        /// <summary>
        /// Prefix for the default clients
        /// </summary>
        public static readonly string DefaultClientPrefix = "076EC2DC-BFE3-4865-9092-EDE051B794CE--";

        /// <summary>
        /// Store the async callback for reply sent
        /// </summary>
        private AsyncCallback replySentCallback;

        /// <summary>
        /// Store the uri that the frontend listen on
        /// </summary>
        private string listenUri;

        /// <summary>
        /// Stores a value indicating whether the broker is in thottling
        /// </summary>
        private int throttling = 0; // 1 - true, 0 - false

        /// <summary>
        /// Stores the throttling wait handle
        /// </summary>
        private ManualResetEvent throttlingWaitHandle;

        /// <summary>
        /// Stores the callback states
        /// </summary>
        private Queue<ChannelClientState> callbackStateQueue;

        /// <summary>
        /// Lock for the callback state queue
        /// </summary>
        private object callbackStateQueueLock;

        /// <summary>
        /// Stores the tryToReceiveRequest callback
        /// </summary>
        private WaitCallback tryToReceiveRequestCallback;

        /// <summary>
        /// Stores the broker client dictionary, keyed by the channel object
        /// </summary>
        private Dictionary<IChannel, BrokerClient> brokerClientDic = new Dictionary<IChannel, BrokerClient>();

        /// <summary>
        /// Stores the broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Stores the broker client manager
        /// </summary>
        private BrokerClientManager clientManager;

        /// <summary>
        /// Stores the broker authorization
        /// </summary>
        private BrokerAuthorization brokerAuth;

        /// <summary>
        /// Stores the shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// Initializes a new instance of the FrontEndBase class
        /// </summary>
        /// <param name="listenUri">indicate the listen uri</param>
        /// <param name="observer">indicating the broker observer</param>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="brokerAuth">indicating the broker authorization</param>
        /// <param name="sharedData">indicating the shared data</param>
        public FrontEndBase(string listenUri, BrokerObserver observer, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, SharedData sharedData)
        {
            this.replySentCallback = new BasicCallbackReferencedThreadHelper<IAsyncResult>(this.ReplySent, this).CallbackRoot;
            this.tryToReceiveRequestCallback = new BasicCallbackReferencedThreadHelper<object>(this.TryToReceiveRequestCallback, this).CallbackRoot;
            this.listenUri = listenUri;
            this.throttlingWaitHandle = new ManualResetEvent(false);
            this.callbackStateQueue = new Queue<ChannelClientState>();
            this.callbackStateQueueLock = new object();
            this.observer = observer;
            this.clientManager = clientManager;
            this.brokerAuth = brokerAuth;
            this.sharedData = sharedData;
            this.observer.OnStartThrottling += this.StartThrottling;
            this.observer.OnStopThrottling += this.StopThrottling;
        }

        /// <summary>
        /// Gets the uri that the frontend listen on
        /// </summary>
        public string ListenUri
        {
            get { return this.listenUri; }
        }

        /// <summary>
        /// Gets the reply sent callback
        /// </summary>
        protected AsyncCallback ReplySentCallback
        {
            get { return this.replySentCallback; }
        }

        /// <summary>
        /// Gets the broker observer
        /// </summary>
        protected BrokerObserver Observer
        {
            get { return this.observer; }
        }

        /// <summary>
        /// Gets the client manager
        /// </summary>
        protected BrokerClientManager ClientManager
        {
            get { return this.clientManager; }
        }

        protected string SessionId
        {
            get { return this.sharedData.BrokerInfo.SessionId; }
        }

        /// <summary>
        /// Open the frontend
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Close the frontend
        /// </summary>
        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Returns the wait handle that is signal
        /// </summary>
        public WaitHandle ThrottlingWaitHandle
        {
            get
            {
                return this.throttlingWaitHandle;
            }
        }

        /// <summary>
        /// Returns whether throttling is engaged
        /// </summary>
        public bool IsThrottlingEngaged
        {
            get
            {
                return this.throttling == 1;
            }
        }

        /// <summary>
        /// Gets the user name from the message
        /// </summary>
        /// <param name="message">indicating the message</param>
        /// <param name="callerSID">output caller's SID if exists</param>
        /// <returns>username as a string</returns>
        protected string GetUserName(Message message, out string callerSID)
        {
            callerSID = string.Empty;

            if (message.Properties.Security == null ||
                message.Properties.Security.ServiceSecurityContext == null || 
                message.Properties.Security.ServiceSecurityContext.IsAnonymous)
            {
                return Constant.AnonymousUserName;
            }

            if (message.Properties.Security.ServiceSecurityContext.WindowsIdentity.User != null)
            {
                callerSID = message.Properties.Security.ServiceSecurityContext.WindowsIdentity.User.Value;
            }

            return message.Properties.Security.ServiceSecurityContext.PrimaryIdentity.Name;
        }


        /// <summary>
        /// Gets the user name from the message
        /// </summary>
        /// <param name="message">indicating the message</param>
        /// <returns>username as a string</returns>
        protected static string GetUserName(Message message)
        {

            int index = message.Headers.FindHeader(Constant.UserNameHeaderName, Constant.HpcHeaderNS);
            if (index < 0)
            {
                return Constant.AnonymousUserName;
            }
            else
            {
                return message.Headers.GetHeader<string>(index);
            }
            
        }



        /// <summary>
        /// Gets the corresponding broker client for the channel
        /// </summary>
        /// <param name="channel">indicating the channel instance</param>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="userName">indicating the user name</param>
        /// <returns>corresponding broker client</returns>
        protected BrokerClient GetClientByChannel(IChannel channel, string clientId, string userName)
        {
            BrokerClient client;

            lock (this.brokerClientDic)
            {
                if (this.brokerClientDic.TryGetValue(channel, out client))
                {
                    // Do not need to check the user name because the user name is bound with the channel
                    return client;
                }
            }

            // Return null if client id is null
            if (clientId == null)
            {
                return null;
            }

            // Gets a client for this channel
            // We do not need a lock here because GetClient is a threadsafe method and there's only one broker client instance for a certain client id
            // If the user name is not match, exception would throw
            client = this.clientManager.GetClient(clientId, userName);

            // lock the broker client dic to add the channel/client pair, need to check if the pair exists because another thread may do it earlier
            lock (this.brokerClientDic)
            {
                if (!this.brokerClientDic.ContainsKey(channel))
                {
                    this.brokerClientDic.Add(channel, client);
                    client.FrontendConnected(channel, this);
                }
            }

            return client;
        }

        /// <summary>
        /// Check the auth
        /// </summary>
        /// <param name="request">request context</param>
        /// <param name="message">message contains the security context</param>
        /// <returns>the message passes the check or not</returns>
        protected bool CheckAuth(RequestContextBase request, Message message)
        {
            ServiceSecurityContext context = GetSecurityContextFromRequest(message);
            if (this.brokerAuth != null && !this.brokerAuth.CheckAccess(context))
            {
                BrokerTracing.EtwTrace.LogFrontEndRequestRejectedAuthenticationError(this.SessionId, string.Empty, Utility.GetMessageIdFromMessage(message), context.WindowsIdentity.Name);
                FaultException<AuthenticationFailure> faultException = new FaultException<AuthenticationFailure>(new AuthenticationFailure(context.WindowsIdentity.Name), string.Format(SR.AuthenticationFailure, context.WindowsIdentity.Name), Constant.AuthenticationFailureFaultCode, AuthenticationFailure.Action);
                request.BeginReply(FrontEndFaultMessage.GenerateFaultMessage(message, message.Headers.MessageVersion, faultException), this.replySentCallback, request);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check the auth
        /// </summary>
        /// <param name="message">message contains the security context</param>
        /// <returns>the message passes the check or not</returns>
        protected bool CheckAuth(Message message)
        {
            ServiceSecurityContext context = GetSecurityContextFromRequest(message);
            if (this.brokerAuth != null && !this.brokerAuth.CheckAccess(context))
            {
                BrokerTracing.EtwTrace.LogFrontEndRequestRejectedAuthenticationError(this.SessionId, string.Empty, Utility.GetMessageIdFromMessage(message), context.WindowsIdentity.Name);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Gets the user info from the message headers
        /// </summary>
        /// <param name="message">request message</param>
        /// <returns>user data object</returns>
        protected static bool GetUserInfoHeader(Message message)
        {
            int index = message.Headers.FindHeader(Constant.UserDataHeaderName, Constant.HpcHeaderNS);

            if (index >= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to begin accept channel
        /// </summary>
        /// <typeparam name="T">indicating the channel type</typeparam>
        /// <param name="listener">indicating the listener</param>
        /// <param name="callback">indicating the callback</param>
        protected void TryToBeginAcceptChannel<T>(IChannelListener<T> listener, AsyncCallback callback) where T : class, IChannel
        {
            IAsyncResult ar = null;

            do
            {
                try
                {
                    ar = listener.BeginAcceptChannel(callback, null);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[FrontEndBase] Exception throwed while begin accept channel: {0}", e);

                    // If it failed to begin accept the channel, it means the listener is failed and we should dispose the frontend
                    ((IDisposable)this).Dispose();
                }
            }
            while (ar.CompletedSynchronously && listener.State == CommunicationState.Opened);
        }

        /// <summary>
        /// Try to get the client id from the message header
        /// </summary>
        /// <param name="message">indicating the message header</param>
        /// <param name="callerSID">indicating the caller's SID</param>
        /// <returns>return the client id as a string, if no client id found, return caller's SID plus a prefix</returns>
        internal static string GetClientId(Message message, string callerSID)
        {
            int index = message.Headers.FindHeader(Constant.ClientIdHeaderName, Constant.HpcHeaderNS);
            if (index < 0)
            {
                return DefaultClientPrefix + callerSID;
            }

            return message.Headers.GetHeader<string>(index);
        }

        /// <summary>
        /// Dispose the FrontEndBase
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.throttlingWaitHandle != null)
                {
                    this.throttlingWaitHandle.Close();
                }
            }

            this.observer.OnStartThrottling -= this.StartThrottling;
            this.observer.OnStopThrottling -= this.StopThrottling;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Close all the channels
        /// </summary>
        protected void CloseAllChannel()
        {
            // Close all the channels
            lock (this.brokerClientDic)
            {
                foreach (IChannel channel in this.brokerClientDic.Keys)
                {
                    channel.Abort();
                }

                this.brokerClientDic.Clear();
                this.brokerClientDic = null;
            }
        }

        /// <summary>
        /// Wrapper to call TryToBeginReceiveMessagesWithThrottling
        /// Repeat to call this method if it has been completed synchronously
        /// </summary>
        /// <param name="state">indicating the channel client state</param>
        protected void TryToBeginReceiveMessagesWithThrottling(ChannelClientState state)
        {
            bool completedSynchronously = false;
            do
            {
                completedSynchronously = this.TryToBeginReceiveMessagesWithThrottlingInternal(state);
            }
            while (completedSynchronously);
        }

        /// <summary>
        /// Override this method to implement real begin receive logic
        /// </summary>
        /// <param name="state">indicating the channel and the client</param>
        /// <returns>if the operation completed synchronously</returns>
        protected abstract bool TryToBeginReceive(ChannelClientState state);

        /// <summary>
        /// Start throttling
        /// </summary>
        private void StartThrottling(object sender, EventArgs args)
        {
            if(Interlocked.CompareExchange(ref this.throttling, 1, 0) == 0)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[FrontEndBase] Start throttling");
                this.throttlingWaitHandle.Reset();
            }
        }

        /// <summary>
        /// Stop throttling
        /// </summary>
        private void StopThrottling(object sender, EventArgs args)
        {
            if (Interlocked.CompareExchange(ref this.throttling, 0, 1) == 1)
            {
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[FrontEndBase] Stop throttling");
                this.throttlingWaitHandle.Set();

                Queue<ChannelClientState> callbackQueue = null;

                if (this.callbackStateQueue.Count > 0)
                {
                    lock (this.callbackStateQueueLock)
                    {
                        if (this.callbackStateQueue.Count > 0)
                        {
                            callbackQueue = this.callbackStateQueue;
                            this.callbackStateQueue = new Queue<ChannelClientState>();
                        }
                    }
                }

                if (callbackQueue != null)
                {
                    while (callbackQueue.Count > 0)
                    {
                        Task.Run(() => this.tryToReceiveRequestCallback(callbackQueue.Dequeue()));
                    }
                }
            }
        }

        /// <summary>
        /// Try to receive messages with throttling
        /// </summary>
        /// <param name="state">indicating the state</param>
        /// <returns>indicating whether the operation completed synchronously</returns>
        private bool TryToBeginReceiveMessagesWithThrottlingInternal(ChannelClientState state)
        {
            if (this.throttling == 0)
            {
                return this.TryToBeginReceive(state);
            }
            else
            {
                lock (this.callbackStateQueueLock)
                {
                    this.callbackStateQueue.Enqueue(state);
                }

                return false;
            }
        }

        /// <summary>
        /// Callback to call the TryToBeginReceive method
        /// </summary>
        /// <param name="state">indicating the client channel state</param>
        /// <param name="timedOut">indicating whether it is called because of timeout</param>
        private void TryToReceiveRequestCallback(object state)
        {
            do
            {
            }
            while (this.TryToBeginReceive((ChannelClientState)state));
        }

        /// <summary>
        /// Get the security context from a request
        /// </summary>
        /// <param name="message">the message contains security context</param>
        /// <returns>service sercurity context instance</returns>
        private static ServiceSecurityContext GetSecurityContextFromRequest(Message message)
        {
            if (message.Properties.Security == null)
            {
                return null;
            }

            return message.Properties.Security.ServiceSecurityContext;
        }

        /// <summary>
        /// Call EndReply when reply sent
        /// </summary>
        /// <param name="ar">async result</param>
        private void ReplySent(IAsyncResult ar)
        {
            RequestContextBase context = (RequestContextBase)ar.AsyncState;
            try
            {
                context.EndReply(ar);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(TraceEventType.Error, 0, "[FrontEndBase] Exception throwed while sending reply: {0}", e);
            }
        }

        /// <summary>
        /// Represents a state of channel and client pair
        /// </summary>
        protected class ChannelClientState
        {
            /// <summary>
            /// Stores the channel
            /// </summary>
            private IChannel channel;

            /// <summary>
            /// Stores the client
            /// </summary>
            private BrokerClient client;

            /// <summary>
            /// Initializes a new instance of the ChannelClientState class
            /// </summary>
            /// <param name="channel">indicating the channel</param>
            /// <param name="client">indicating the client</param>
            public ChannelClientState(IChannel channel, BrokerClient client)
            {
                this.channel = channel;
                this.client = client;
            }

            /// <summary>
            /// Gets or sets the channel
            /// </summary>
            public IChannel Channel
            {
                get { return this.channel; }
            }

            /// <summary>
            /// Gets or sets the client
            /// </summary>
            public BrokerClient Client
            {
                get { return this.client; }
                set { this.client = value; }
            }
        }
    }
}
