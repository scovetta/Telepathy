// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;

    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.Internal.AzureQueue;

    /// <summary>
    /// Broker frontend factory to build proxy to communicate to broker
    /// frontend using WS contract
    /// </summary>
    internal class WSBrokerFrontendFactory : BrokerFrontendFactory
    {
        /// <summary>
        /// Stores the session info
        /// </summary>
        private SessionInfo info;

        /// <summary>
        /// Stores the binding object
        /// </summary>
        private Binding binding;

        /// <summary>
        /// Stores the broker frontend for inprocess broker
        /// </summary>
        private IBrokerFrontend brokerFrontend;

        /// <summary>
        /// Stores the scheme
        /// </summary>
        private TransportScheme scheme;

        /// <summary>
        /// Stores the client proxy for send request
        /// </summary>
        private IOutputChannel sendRequestClient;

        /// <summary>
        /// Stores the client proxy for broker controller
        /// </summary>
        private IController brokerControllerClient;

        /// <summary>
        /// Stores the response service client proxy
        /// </summary>
        private IResponseService responseServiceClient;

        /// <summary>
        /// Stores the lock to create channel
        /// </summary>
        private object lockCreateChannel = new object();

        /// <summary>
        /// Stores the current close timeout
        /// </summary>
        private int currentCloseTimeout = Constant.PurgeTimeoutMS;

        /// <summary>
        /// ChannelFactory for broker client
        /// </summary>
        private IChannelFactory<IDuplexSessionChannel> brokerClientFactory = null;

        /// <summary>
        /// It indicates whether the scheduler is on Azure or on-premise.
        /// </summary>
        private bool schedulerOnAzure = false;

        /// <summary>
        /// Indicates whether the scheduler is on IaaS VM
        /// </summary>
        private bool schedulerOnIaaS = false;

        /// <summary>
        /// Initializes a new instance of the BrokerFrontendFactory class
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="binding">indicating the binding</param>
        /// <param name="info">indicating the session info</param>
        /// <param name="scheme">indicating the scheme</param>
        /// <param name="responseCallback">indicating the response callback</param>
        public WSBrokerFrontendFactory(string clientId, Binding binding, SessionInfo info, TransportScheme scheme, IResponseServiceCallback responseCallback)
            : base(clientId, responseCallback)
        {
            this.info = info;
            this.schedulerOnAzure = SoaHelper.IsSchedulerOnAzure(this.info.BrokerLauncherEpr, this.info.UseInprocessBroker);
            this.schedulerOnIaaS = SoaHelper.IsSchedulerOnIaaS(this.info.Headnode);

            this.binding = binding;
            this.scheme = scheme;
            if (info.UseInprocessBroker)
            {
                this.brokerFrontend = this.info.InprocessBrokerAdapter.GetBrokerFrontend(responseCallback);
                this.sendRequestClient = new SendRequestAdapter(this.brokerFrontend);
                this.brokerControllerClient = this.brokerFrontend;
                this.responseServiceClient = this.brokerFrontend;
            }
        }

        /// <summary>
        /// Get broker client proxy for send request
        /// </summary>
        /// <returns>returns broker client proxy for send request as IOutputChannel</returns>
        public override IChannel GetBrokerClient()
        {
            if (this.sendRequestClient == null || this.sendRequestClient.State == CommunicationState.Faulted)
            {
                lock (this.lockCreateChannel)
                {
                    // if sendRequestClient is in Faulted state, recreate it
                    if (this.sendRequestClient != null && this.sendRequestClient.State == CommunicationState.Faulted)
                    {
                        SessionBase.TraceSource.TraceInformation("SendRequestClient is in Faulted state. Recreate it.");
                        Utility.SafeCloseCommunicateObject(this.sendRequestClient);
                        this.sendRequestClient = null;
                    }

                    if (this.sendRequestClient == null)
                    {
                        this.sendRequestClient = this.CreateClientWithRetry(ClientType.SendRequest) as IOutputChannel;
                    }
                }
            }

            return this.sendRequestClient;
        }

        /// <summary>
        /// Gets the controller client
        /// </summary>
        /// <returns>returns the controller client</returns>
        public override IController GetControllerClient()
        {
            ICommunicationObject brokerControllerChannel = this.brokerControllerClient as ICommunicationObject;
            if (this.brokerControllerClient == null || (brokerControllerChannel != null && brokerControllerChannel.State == CommunicationState.Faulted))
            {
                lock (this.lockCreateChannel)
                {
                    // if brokerControllerClient is in Faulted state, recreate it
                    if (brokerControllerChannel != null && brokerControllerChannel.State == CommunicationState.Faulted)
                    {
                        Utility.SafeCloseCommunicateObject(brokerControllerChannel);
                        this.brokerControllerClient = null;
                    }

                    if (this.brokerControllerClient == null)
                    {
                        this.brokerControllerClient = this.CreateClientWithRetry(ClientType.Controller) as BrokerControllerClient;
                    }
                }
            }

            return this.brokerControllerClient;
        }

        /// <summary>
        /// Gets controller client and set operation timeout if it is a WCF proxy
        /// </summary>
        /// <param name="operationTimeout">indicating the operation timeout</param>
        /// <returns>returns IController instance</returns>
        public override IController GetControllerClient(TimeSpan operationTimeout)
        {
            IController controller = this.GetControllerClient();
            if (controller is ClientBase<IControllerAsync>)
            {
                ClientBase<IControllerAsync> proxy = (ClientBase<IControllerAsync>)controller;
                proxy.InnerChannel.OperationTimeout = operationTimeout;
            }

            return controller;
        }

        /// <summary>
        /// Gets controller client and set operation timeout if it is a WCF proxy
        /// </summary>
        /// <param name="timeoutMilliseconds">indicating the operation timeout</param>
        /// <returns>returns IController instance</returns>
        public override IController GetControllerClient(int timeoutMilliseconds)
        {
            IController controller = this.GetControllerClient();
            if (controller is ClientBase<IControllerAsync>)
            {
                ClientBase<IControllerAsync> proxy = (ClientBase<IControllerAsync>)controller;

                double operationTimeoutMS = Math.Max(timeoutMilliseconds, Constant.MinOperationTimeout);

                if (timeoutMilliseconds == Timeout.Infinite)
                {
                    proxy.InnerChannel.OperationTimeout = TimeSpan.MaxValue;
                }
                else
                {
                    proxy.InnerChannel.OperationTimeout = TimeSpan.FromMilliseconds(operationTimeoutMS);
                }
            }

            return controller;
        }

        /// <summary>
        /// Gets the response service client
        /// </summary>
        /// <returns>returns the response service client</returns>
        public override IResponseService GetResponseServiceClient()
        {
            ICommunicationObject getResponsesChannel = this.responseServiceClient as ICommunicationObject;
            if (this.responseServiceClient == null || (getResponsesChannel != null && getResponsesChannel.State == CommunicationState.Faulted))
            {
                lock (this.lockCreateChannel)
                {
                    // if responseServiceClient is in Faulted state, recreate it
                    if (getResponsesChannel != null && getResponsesChannel.State == CommunicationState.Faulted)
                    {
                        SessionBase.TraceSource.TraceInformation("ResponseServiceClient is in Faulted state. Recreate it.");
                        Utility.SafeCloseCommunicateObject(getResponsesChannel);
                        this.responseServiceClient = null;
                    }

                    if (this.responseServiceClient == null)
                    {
                        // For Azure connection, the Azure broker sends back heartbeat message for this client.
                        this.responseServiceClient = this.CreateClientWithRetry(ClientType.GetResponse) as BrokerResponseServiceClient;
                    }
                }
            }

            return this.responseServiceClient;
        }

        /// <summary>
        /// Set close timeout
        /// </summary>
        /// <param name="timeoutMilliseconds">indicating timeout</param>
        public override void SetCloseTimeout(int timeoutMilliseconds)
        {
            this.currentCloseTimeout = timeoutMilliseconds;
        }

        /// <summary>
        /// Close broker client proxy
        /// </summary>
        /// <param name="setToNull">Whether to set client object to null after closing. THis should only be true if caller is within BrokerClient's objectLock to ensure
        /// another thread isnt using or about to use it</param>
        /// <param name="timeoutInMS">How long to wait for close. -1 means use binding's close timeout</param>
        public override void CloseBrokerClient(bool setToNull, int timeoutInMS)
        {
            if (this.sendRequestClient != null)
            {
                lock (this.lockCreateChannel)
                {
                    if (this.sendRequestClient != null && !(this.sendRequestClient is SendRequestAdapter))
                    {
                        Utility.SafeCloseCommunicateObject(this.sendRequestClient, timeoutInMS);

                        if (setToNull)
                        {
                            // Only set to null for WCF proxy
                            this.sendRequestClient = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dispose the BrokerFrontendFactory instance
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                #region Close WCF proxies

                List<IAsyncResult> asyncResults = new List<IAsyncResult>();
                IAsyncResult asyncResult = null;

                // If sendRequestClient is SendRequestAdapter, close is not needed
                if (this.sendRequestClient != null && this.sendRequestClient is ICommunicationObject && !(this.sendRequestClient is SendRequestAdapter))
                {
                    asyncResult = ((ICommunicationObject)this.sendRequestClient).BeginClose(null, this.sendRequestClient);
                    asyncResults.Add(asyncResult);
                }

                if (this.responseServiceClient != null && this.responseServiceClient is ICommunicationObject)
                {
                    asyncResult = ((ICommunicationObject)this.responseServiceClient).BeginClose(null, this.responseServiceClient);
                    asyncResults.Add(asyncResult);
                }

                if (this.brokerControllerClient != null && this.brokerControllerClient is ICommunicationObject)
                {
                    asyncResult = ((ICommunicationObject)this.brokerControllerClient).BeginClose(null, this.brokerControllerClient);
                    asyncResults.Add(asyncResult);
                }

                // Cant WaitAll on STA thread so wait on handles individually
                foreach (IAsyncResult ar in asyncResults)
                {
                    ICommunicationObject currentChannel = (ICommunicationObject)ar.AsyncState;

                    if (!ar.IsCompleted)
                    {
                        if (!ar.AsyncWaitHandle.WaitOne(this.currentCloseTimeout, false))
                        {
                            // If wait times out, trace, abort and move on to next
                            SessionBase.TraceSource.TraceInformation("Timeout waiting for broker client to close");
                            currentChannel.Abort();
                            continue;
                        }
                    }

                    // Check to ensure the Close completed successfully. If not abort
                    try
                    {
                        currentChannel.EndClose(ar);
                    }

                    catch (Exception e)
                    {
                        // If close fails, trace, abort and move on to next
                        SessionBase.TraceSource.TraceInformation("Broker client to close failed. Aborting - {0}", e);
                        currentChannel.Abort();
                    }
                }

                if (this.brokerClientFactory != null)
                {
                    try
                    {
                        this.brokerClientFactory.Abort();
                        this.brokerClientFactory = null;
                    }
                    catch
                    {
                        // abort the factory and swallow the exception
                    }
                }

                #endregion

                #region Close inprocess broker adapters

                DisposeObject(this.sendRequestClient);
                DisposeObject(this.responseServiceClient);
                DisposeObject(this.brokerControllerClient);

                #endregion
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Dispose a object if it implements IDisposable
        /// </summary>
        /// <param name="obj">indicating the object to be disposed</param>
        private static void DisposeObject(object obj)
        {
            IDisposable instance = obj as IDisposable;
            if (instance != null)
            {
                instance.Dispose();
            }
        }

        /// <summary>
        /// Generate endpoint address by scheme from epr list
        /// </summary>
        /// <param name="eprList">indicating the epr list</param>
        /// <param name="scheme">indicating the scheme</param>
        /// <returns>endpoint address</returns>
        private static EndpointAddress GenerateEndpointAddress(string[] eprList, TransportScheme scheme, bool secure, bool internalChannel)
        {
            int index = (int)Math.Log((int)scheme, 2);
            string epr = eprList[index];
            return SoaHelper.CreateEndpointAddress(new Uri(epr), secure, internalChannel);
        }

        /// <summary>
        /// Broker response service client proxy
        /// </summary>
        private class BrokerResponseServiceClient : DuplexClientBase<IResponseServiceAsync>, IResponseService, IResponseServiceAsync
        {
            public BrokerResponseServiceClient(Binding binding, EndpointAddress remoteAddress, InstanceContext context)
                : base(context, binding, remoteAddress)
            {
            }

            public void GetResponses(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId)
            {
                this.EndGetResponses(this.BeginGetResponses(action, clientData, resetToBegin, count, clientId, null, null));
            }

            public IAsyncResult BeginGetResponses(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId, AsyncCallback callback, object state)
            {
                return this.Channel.BeginGetResponses(action, clientData, resetToBegin, count, clientId, callback, state);
            }

            public void EndGetResponses(IAsyncResult result)
            {
                this.Channel.EndGetResponses(result);
            }
        }

        /// <summary>
        /// Adapt IOutputChannel to IBrokerFrontend
        /// </summary>
        private class SendRequestAdapter : IOutputChannel, IDisposable
        {
            /// <summary>
            /// Stores the broker frontend instance
            /// </summary>
            private IBrokerFrontend frontend;

            /// <summary>
            /// Initializes a new instance of the SendRequestAdapter class
            /// </summary>
            /// <param name="frontend">indicating the broker frontend instance</param>
            public SendRequestAdapter(IBrokerFrontend frontend)
            {
                this.frontend = frontend;
            }

            #region IOutputChannel Members

            IAsyncResult IOutputChannel.BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            IAsyncResult IOutputChannel.BeginSend(Message message, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            void IOutputChannel.EndSend(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            System.ServiceModel.EndpointAddress IOutputChannel.RemoteAddress
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            void IOutputChannel.Send(Message message, TimeSpan timeout)
            {
                this.frontend.SendRequest(message);
            }

            void IOutputChannel.Send(Message message)
            {
                this.frontend.SendRequest(message);
            }

            Uri IOutputChannel.Via
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            T IChannel.GetProperty<T>()
            {
                throw new NotImplementedException();
            }

            void System.ServiceModel.ICommunicationObject.Abort()
            {
                throw new NotImplementedException();
            }

            IAsyncResult System.ServiceModel.ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            IAsyncResult System.ServiceModel.ICommunicationObject.BeginClose(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            IAsyncResult System.ServiceModel.ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            IAsyncResult System.ServiceModel.ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            void System.ServiceModel.ICommunicationObject.Close(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            void System.ServiceModel.ICommunicationObject.Close()
            {
                throw new NotImplementedException();
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Closed
            {
                add
                {
                    throw new NotImplementedException();
                }
                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Closing
            {
                add
                {
                    throw new NotImplementedException();
                }
                remove
                {
                    throw new NotImplementedException();
                }
            }

            void System.ServiceModel.ICommunicationObject.EndClose(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            void System.ServiceModel.ICommunicationObject.EndOpen(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Faulted
            {
                add
                {
                    throw new NotImplementedException();
                }
                remove
                {
                    throw new NotImplementedException();
                }
            }

            void System.ServiceModel.ICommunicationObject.Open(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            void System.ServiceModel.ICommunicationObject.Open()
            {
                throw new NotImplementedException();
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Opened
            {
                add
                {
                    throw new NotImplementedException();
                }
                remove
                {
                    throw new NotImplementedException();
                }
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Opening
            {
                add
                {
                    throw new NotImplementedException();
                }
                remove
                {
                    throw new NotImplementedException();
                }
            }

            System.ServiceModel.CommunicationState System.ServiceModel.ICommunicationObject.State
            {
                get
                {
                    return CommunicationState.Opened;
                }
            }

            #endregion

            public void Dispose()
            {
                ((IDisposable)this.frontend).Dispose();
            }
        }

        /// <summary>
        /// Client proxy for broker controller
        /// </summary>
        private class BrokerControllerClient : ClientBase<IControllerAsync>, IControllerAsync, IController
        {
            public BrokerControllerClient(Binding binding, EndpointAddress epr)
                : base(binding, epr)
            {
            }

            /// <summary>
            /// Send the Flush command
            /// </summary>
            /// <param name="count">message count</param>
            public void Flush(int count, string clientId, int batchId, int timeoutThrottlingMs, int timeoutFlushMs)
            {
                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
                IAsyncResult result = this.Channel.BeginFlush(count, clientId, batchId, timeoutThrottlingMs, timeoutFlushMs, null, null);
                this.Channel.EndFlush(result);
            }

            /// <summary>
            /// Send the Flush command async
            /// </summary>
            /// <param name="count">message count</param>
            public IAsyncResult BeginFlush(int count, string clientId, int batchId, int timeoutThrottlingMs, int timeoutFlushMs, AsyncCallback callback, object state)
            {
                return this.Channel.BeginFlush(count, clientId, batchId, timeoutThrottlingMs, timeoutFlushMs, callback, state);
            }

            /// <summary>
            /// Finish async Flush
            /// </summary>
            public void EndFlush(IAsyncResult ar)
            {
                this.Channel.EndFlush(ar);
            }

            /// <summary>
            /// Send the end of message
            /// </summary>
            /// <param name="count">message count</param>
            public void EndRequests(int count, string clientId, int batchId, int timeoutThrottlingMs, int timeoutEOMMs)
            {
                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
                IAsyncResult result = this.Channel.BeginEndRequests(count, clientId, batchId, timeoutThrottlingMs, timeoutEOMMs, null, null);
                this.Channel.EndEndRequests(result);
            }

            /// <summary>
            /// Send the end of message async
            /// </summary>
            /// <param name="count">message count</param>
            public IAsyncResult BeginEndRequests(int count, string clientId, int batchId, int timeoutThrottlingMs, int timeoutEOMMs, AsyncCallback callback, object state)
            {
                return this.Channel.BeginEndRequests(count, clientId, batchId, timeoutThrottlingMs, timeoutEOMMs, callback, state);
            }

            /// <summary>
            /// Finish async end of message
            /// </summary>
            /// <param name="count">message count</param>
            public void EndEndRequests(IAsyncResult ar)
            {
                this.Channel.EndEndRequests(ar);
            }

            /// <summary>
            /// Get broker client status
            /// </summary>
            /// <param name="clientId">indicating the client id</param>
            /// <returns>returns the broker client status</returns>
            public BrokerClientStatus GetBrokerClientStatus(string clientId)
            {
                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
                IAsyncResult result = this.Channel.BeginGetBrokerClientStatus(clientId, null, null);
                return this.Channel.EndGetBrokerClientStatus(result);
            }

            /// <summary>
            /// Get number of committed requests in specified client
            /// </summary>
            /// <param name="clientId">indicating the client id</param>
            /// <returns>returns number of committed requests in the client with specified client id</returns>
            public int GetRequestsCount(string clientId)
            {
                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
                IAsyncResult result = this.Channel.BeginGetRequestsCount(clientId, null, null);
                return this.Channel.EndGetRequestsCount(result);
            }

            public void Purge(string clientId)
            {
                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028
                IAsyncResult result = this.Channel.BeginPurge(clientId, null, null);
                this.Channel.EndPurge(result);
            }

            public IAsyncResult BeginPing(AsyncCallback callback, object asyncState)
            {
                return this.Channel.BeginPing(callback, asyncState);
            }

            public void EndPing(IAsyncResult result)
            {
                this.Channel.EndPing(result);
            }

            public void Ping()
            {
                this.Channel.EndPing(this.Channel.BeginPing(null, null));
            }

            #region NotImplemented

            public BrokerResponseMessages PullResponses(string action, GetResponsePosition position, int count, string clientId)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginPurge(string clientid, AsyncCallback callback, object asyncState)
            {
                throw new NotImplementedException();
            }

            public void EndPurge(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginGetBrokerClientStatus(string clientId, AsyncCallback callback, object asyncState)
            {
                throw new NotImplementedException();
            }

            public BrokerClientStatus EndGetBrokerClientStatus(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginGetRequestsCount(string clientId, AsyncCallback callback, object asyncState)
            {
                throw new NotImplementedException();
            }

            public int EndGetRequestsCount(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginPullResponses(string action, GetResponsePosition position, int count, string clientId, AsyncCallback callback, object asyncState)
            {
                throw new NotImplementedException();
            }

            public BrokerResponseMessages EndPullResponses(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public void GetResponsesAQ(
                string action,
                string clientData,
                GetResponsePosition resetToBegin,
                int count,
                string clientId,
                int sessionHash,
                out string azureResponseQueueUri,
                out string azureResponseBlobUri)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginGetResponsesAQ(
                string action,
                string clientData,
                GetResponsePosition resetToBegin,
                int count,
                string clientId,
                int sessionHash,
                System.AsyncCallback callback,
                object asyncState)
            {
                throw new NotImplementedException();
            }

            public void EndGetResponsesAQ(out string azureResponseQueueUri, out string azureResponseBlobUri, IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            #endregion

        }

        /// <summary>
        /// This factory class creates three different kinds of client. This method gathers the code of client creation
        /// and adds the retry logic. The retry can avoid transient connection error especially when connect to Azure.
        /// </summary>
        /// <param name="clientType">the type of the client</param>
        /// <returns>the client</returns>
        private ICommunicationObject CreateClientWithRetry(ClientType clientType)
        {
            ICommunicationObject client = null;
            Exception exception = null;

            try
            {
                exception = null;
                switch (clientType)
                {
                    case ClientType.SendRequest:

                        if (this.brokerClientFactory == null)
                        {
                            BindingParameterCollection bindingParms = new BindingParameterCollection();
                            ClientCredentials clientCredentials = new ClientCredentials();
                            if (this.info.UseAad)
                            {
                                // Authentication will be taken care of by adding message header to request
                            }
                            else if (this.SetClientCredential(clientCredentials))
                            {
                                bindingParms.Add(clientCredentials);
                            }

                            this.brokerClientFactory = this.binding.BuildChannelFactory<IDuplexSessionChannel>(bindingParms);
                            this.brokerClientFactory.Open();
                        }

                        client = this.brokerClientFactory.CreateChannel(GenerateEndpointAddress(this.info.BrokerEpr, this.scheme, this.info.Secure, this.info.IsAadOrLocalUser));
                        break;

                    case ClientType.GetResponse:

                        BrokerResponseServiceClient responseClient = new BrokerResponseServiceClient(
                            this.binding,
                            GenerateEndpointAddress(this.info.ResponseEpr, this.scheme, this.info.Secure, this.info.IsAadOrLocalUser),
                            new InstanceContext(this.ResponseCallback));
                            this.SetClientCredential(responseClient);


                        client = responseClient;
                        break;

                    case ClientType.Controller:

                        BrokerControllerClient controllerClient = new BrokerControllerClient(
                            this.binding,
                            GenerateEndpointAddress(this.info.ControllerEpr, this.scheme, this.info.Secure, this.info.IsAadOrLocalUser));
                            this.SetClientCredential(controllerClient);

                        client = controllerClient;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                client.Open();
            }
            catch (Exception e)
            {
                if (this.brokerClientFactory != null)
                {
                    try
                    {
                        this.brokerClientFactory.Abort();
                        this.brokerClientFactory = null;
                    }
                    catch
                    {
                        // abandon the factory, swallow the exception
                    }
                }

                if (client != null)
                {
                    try
                    {
                        client.Abort();
                        client = null;
                    }
                    catch
                    {
                        // abandon the channel, swallow the exception
                    }
                }

                exception = e;
            }


            if (exception != null)
            {
                throw exception;
            }

            return client;
        }

        private void SetClientCredential<T>(ClientBase<T> controller) where T : class
        {
            Debug.Assert(controller != null);
            Debug.Assert(controller.ClientCredentials != null);
            this.SetClientCredential(controller.ClientCredentials);
        }

        private bool SetClientCredential(ClientCredentials clientCredentials)
        {
            if (this.schedulerOnIaaS || this.info.UseWindowsClientCredential)
            {
                string domainName;
                string userName;
                SoaHelper.ParseDomainUser(this.info.Username, out domainName, out userName);
                clientCredentials.Windows.ClientCredential.Domain = domainName;
                clientCredentials.Windows.ClientCredential.UserName = userName;
                clientCredentials.Windows.ClientCredential.Password = this.info.InternalPassword;
                return true;
            }
            else if ((this.scheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                clientCredentials.UserName.UserName = this.info.Username;
                clientCredentials.UserName.Password = this.info.InternalPassword;
                return true;
            }
            else if (this.info.LocalUser)
            {
                clientCredentials.UseInternalAuthenticationAsync().GetAwaiter().GetResult();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This is a internal enum for the types of the client created the BrokerFrontedFactory.
        /// </summary>
        private enum ClientType
        {
            /// <summary>
            /// Broker client
            /// </summary>
            SendRequest,

            /// <summary>
            /// GetResponse client
            /// </summary>
            GetResponse,

            /// <summary>
            /// Broker controller client
            /// </summary>
            Controller
        }

        // not implemented
        public override AzureQueueProxy GetBrokerClientAQ()
        {
            throw new NotImplementedException();
        }
    }
}
