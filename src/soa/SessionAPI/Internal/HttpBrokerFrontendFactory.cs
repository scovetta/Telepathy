//------------------------------------------------------------------------------
// <copyright file="WSBrokerFrontendFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Broker frontend factory to build proxy to communicate to broker
//      frontend using WS contract
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Xml;

    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Client;

    /// <summary>
    /// Broker frontend factory to build proxy to communicate to broker
    /// frontend using WS contract
    /// </summary>
    internal class HttpBrokerFrontendFactory : BrokerFrontendFactory
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

        // private IOutputChannel sendRequestClient;
        private IRequestChannel sendRequestClient;

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

        // private IChannelFactory<IOutputChannel> brokerClientFactory = null;
        private IChannelFactory<IRequestChannel> brokerClientFactory = null;

        /// <summary>
        /// Stores if Azure storage is used
        /// </summary>
        private bool useAzureQueue = false;

        /// <summary>
        /// Stores the Azure storage proxy
        /// </summary>
        private AzureQueueProxy azureQueueProxy = null;

        /// <summary>
        /// Initializes a new instance of the BrokerFrontendFactory class
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="binding">indicating the binding</param>
        /// <param name="info">indicating the session info</param>
        /// <param name="scheme">indicating the scheme</param>
        /// <param name="responseCallback">indicating the response callback</param>
        public HttpBrokerFrontendFactory(string clientId, Binding binding, SessionBase session, TransportScheme scheme, IResponseServiceCallback responseCallback) : base(clientId, responseCallback)
        {
            this.info = session.Info as SessionInfo;
            this.binding = binding;
            this.scheme = scheme;
            if (this.info.UseAzureQueue == true)
            {
                this.useAzureQueue = true;
                this.azureQueueProxy = session.AzureQueueProxy;
            }

            if (this.info.UseInprocessBroker)
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
                        // this.sendRequestClient = this.CreateClientWithRetry(ClientType.SendRequest) as IOutputChannel;
                        this.sendRequestClient = this.CreateClientWithRetry(ClientType.SendRequest) as IRequestChannel;
                    }
                }
            }

            return this.sendRequestClient;
        }

        /// <summary>
        /// Get the broker client for Azure storage proxy
        /// </summary>
        /// <returns></returns>
        public override AzureQueueProxy GetBrokerClientAQ()
        {
            return this.azureQueueProxy;
        }

        /// <summary>
        /// Gets the controller client
        /// </summary>
        /// <returns>returns the controller client</returns>
        public override IController GetControllerClient()
        {
            if (this.useAzureQueue && 
                !(string.IsNullOrEmpty(this.info.AzureControllerRequestQueueUri) || string.IsNullOrEmpty(this.info.AzureControllerResponseQueueUri)))
            {
                var controller = this.brokerControllerClient as BrokerControllerCloudQueueClient;
                if (controller == null)
                {
                    this.brokerControllerClient = new BrokerControllerCloudQueueClient(this.info.AzureControllerRequestQueueUri, this.info.AzureControllerResponseQueueUri);
                }
            }
            else
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
            if (this.responseServiceClient == null)
            {
                lock (this.lockCreateChannel)
                {
                    if (this.responseServiceClient == null)
                    {
                        this.responseServiceClient = new BrokerResponseServiceClient(this.GetControllerClient(), this.ResponseCallback, this.useAzureQueue, this.azureQueueProxy);
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

                if (this.responseServiceClient != null && this.responseServiceClient is BrokerResponseServiceClient)
                {
                    (this.responseServiceClient as BrokerResponseServiceClient).Close();
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
        /// Provides a broker frontend proxy communicating with SOA Web service
        /// This broker frontend proxy is in charge of getting response
        /// </summary>
        private class BrokerResponseServiceClient : DisposableObject, IResponseService
        {
            private IController controller;

            private IResponseServiceCallback callback;

            private bool useAzureQueue = false;

            private AzureQueueProxy azureQueueProxy = null;

            private List<IResponseHandler> handlers = new List<IResponseHandler>();

            private int totalResponseCount = 0;

            private int sessionHash = 0;

            public BrokerResponseServiceClient(IController controller, IResponseServiceCallback callback, bool useAzureQueue, AzureQueueProxy azureQueueProxy)
            {
                this.controller = controller;
                this.callback = callback;
                if (useAzureQueue)
                {
                    this.useAzureQueue = useAzureQueue;
                    this.azureQueueProxy = azureQueueProxy;
                    this.sessionHash = azureQueueProxy.SessionHash;
                }
            }

            /// <summary>
            /// Get responses
            /// </summary>
            /// <param name="action">indicating the action</param>
            /// <param name="clientData">indicating the client data</param>
            /// <param name="resetToBegin">indicating the get response position</param>
            /// <param name="count">indicating the count</param>
            /// <param name="clientId">indicating the client id</param>
            void IResponseService.GetResponses(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId)
            {
                if (this.useAzureQueue)
                {
                    string azureResponseQueueUri;
                    string azureResponseBlobUri;

                    this.controller.GetResponsesAQ(action, clientData, resetToBegin, count, clientId, this.sessionHash, out azureResponseQueueUri, out azureResponseBlobUri);

                    // then retrieve the responses async from the azure queue proxy
                    if (!this.azureQueueProxy.IsResponseClientInitialized && !string.IsNullOrEmpty(azureResponseQueueUri) && !string.IsNullOrEmpty(azureResponseBlobUri))
                    {
                        this.azureQueueProxy.InitResponseClient(azureResponseQueueUri, azureResponseBlobUri);
                    }

                    // check if there is already a handler with the same action and clientData
                    foreach (IResponseHandler h in handlers)
                    {
                        if (h.Action().Equals(action, StringComparison.InvariantCulture) && h.ClientData().Equals(clientData, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return;
                        }
                    }

                    AzureQueueResponseHandler handler = new AzureQueueResponseHandler(this.azureQueueProxy, action, clientData, resetToBegin, count, clientId, this.callback, this);
                    this.handlers.Add(handler);
                    handler.Completed += new EventHandler(HandlerCompleted);
                }
                else
                {
                    HttpResponseHandler handler = new HttpResponseHandler(this.controller, action, clientData, resetToBegin, count, clientId, this.callback, this);
                    this.handlers.Add(handler);
                    handler.Completed += new EventHandler(HandlerCompleted);
                }
            }

            private void HandlerCompleted(object sender, EventArgs e)
            {
                if (this.handlers != null)
                {
                    this.handlers.Remove((IResponseHandler)sender);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    foreach (DisposableObject handler in this.handlers)
                    {
                        handler.Close();
                    }
                }

                base.Dispose(disposing);
            }

            private class HttpResponseHandler : DisposableObject, IResponseHandler
            {
                private Thread HttpResponseThread;

                private volatile bool disposeFlag;

                private IController controller;

                private string action;

                public string Action()
                {
                    return action;
                }

                private string clientData;

                public string ClientData()
                {
                    return clientData;
                }

                private GetResponsePosition resetToBegin;

                private int count;

                private string clientId;

                private IResponseServiceCallback callback;

                public event EventHandler Completed;

                private BrokerResponseServiceClient responseClient;

                public HttpResponseHandler(
                    IController controller,
                    string action,
                    string clientData,
                    GetResponsePosition resetToBegin,
                    int count,
                    string clientId,
                    IResponseServiceCallback callback,
                    BrokerResponseServiceClient responseClient)
                {
                    this.controller = controller;
                    this.action = action;
                    this.clientData = clientData;
                    this.resetToBegin = resetToBegin;
                    this.count = count;
                    this.clientId = clientId;
                    this.callback = callback;
                    this.responseClient = responseClient;
                    this.HttpResponseThread = new Thread(this.HttpGetResponseThread);
                    this.HttpResponseThread.IsBackground = true;
                    this.HttpResponseThread.Start();
                }

                protected override void Dispose(bool disposing)
                {
                    this.disposeFlag = true;
                    base.Dispose(disposing);
                }

                private void HttpGetResponseThread()
                {
                    try
                    {
                        // List<IAsyncResult> results = new List<IAsyncResult>();
                        bool isEOM = false;

                        // ProcessResponseDelegate p = ProcessResponses;
                        int responseCount = 0;

                        // while (!isEOM)
                        // {
                        SessionBase.TraceSource.TraceInformation("Begin PullResponse : count {0} : clientId {1}", count, clientId);
                        BrokerResponseMessages responseMessages = this.controller.PullResponses(action, resetToBegin, count, clientId);
                        SessionBase.TraceSource.TraceInformation("End PullResponse : count {0} : isEOM {1}", responseMessages.SOAPMessage.Length, responseMessages.EOM);

                        // responseCount += responseMessages.SOAPMessage.Length;
                        responseCount = responseMessages.SOAPMessage.Length;

                        // results.Add(p.BeginInvoke(responseMessages, clientData, null, null));
                        ProcessResponses(responseMessages, clientData);
                        Interlocked.Add(ref this.responseClient.totalResponseCount, responseCount);
                        isEOM = responseMessages.EOM;

                        if (isEOM)
                        {
                            // construct endofreponses message
                            TypedMessageConverter converter = TypedMessageConverter.Create(typeof(EndOfResponses), Constant.EndOfMessageAction);
                            EndOfResponses endOfResponses = new EndOfResponses();
                            endOfResponses.Count = this.responseClient.totalResponseCount;
                            endOfResponses.Reason = EndOfResponsesReason.Success;
                            Message eom = converter.ToMessage(endOfResponses, MessageVersion.Soap11);
                            eom.Headers.Add(MessageHeader.CreateHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS, clientData));

                            this.callback.SendResponse(eom);
                        }
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceInformation("PullResponse Exception: {0}", e.ToString());

                        if (this.disposeFlag)
                        {
                            return;
                        }

                        Message exceptionMessage = Message.CreateMessage(MessageVersion.Soap11, @"http://hpc.microsoft.com/ClientSideExeption");
                        exceptionMessage.Headers.Add(MessageHeader.CreateHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS, clientData));
                        exceptionMessage.Properties.Add(@"HttpClientException", e);

                        this.callback.SendResponse(exceptionMessage);
                    }
                    finally
                    {
                        if (this.Completed != null)
                        {
                            this.Completed(this, EventArgs.Empty);
                        }
                    }
                }

                public delegate void ProcessResponseDelegate(BrokerResponseMessages responseMessages, string clientData);

                public void ProcessResponses(BrokerResponseMessages responseMessages, string clientData)
                {
                    foreach (XmlElement xe in responseMessages.SOAPMessage)
                    {
                        Message m = null;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            XmlWriter xw = XmlWriter.Create(ms);
                            xe.WriteTo(xw);
                            xw.Flush();
                            ms.Position = 0;
                            XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(XmlReader.Create(ms, new XmlReaderSettings() { XmlResolver = null }));
                            m = Message.CreateMessage(reader, int.MaxValue, MessageVersion.Soap11);
                            m.Headers.Add(MessageHeader.CreateHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS, clientData));
                            this.callback.SendResponse(m);
                        }
                    }
                }
            }

            private class AzureQueueResponseHandler : DisposableObject, IResponseHandler
            {
                private Thread AzureQueueResponseThread;

                private volatile bool disposeFlag;

                private string action;

                public string Action()
                {
                    return action;
                }

                private string clientData;

                public string ClientData()
                {
                    return clientData;
                }

                private GetResponsePosition resetToBegin;

                private int count;

                private string clientId;

                private IResponseServiceCallback callback;

                public event EventHandler Completed;

                private BrokerResponseServiceClient responseClient;

                private AzureQueueProxy azureQueueProxy = null;

                public AzureQueueResponseHandler(
                    AzureQueueProxy azureQueueProxy,
                    string action,
                    string clientData,
                    GetResponsePosition resetToBegin,
                    int count,
                    string clientId,
                    IResponseServiceCallback callback,
                    BrokerResponseServiceClient responseClient)
                {
                    this.action = action;
                    this.clientData = clientData;
                    this.resetToBegin = resetToBegin;
                    this.count = count;
                    this.clientId = clientId;
                    this.callback = callback;
                    this.responseClient = responseClient;
                    this.azureQueueProxy = azureQueueProxy;

                    this.AzureQueueResponseThread = new Thread(new ThreadStart(this.GetResponseThread));

                    this.AzureQueueResponseThread.IsBackground = true;
                    this.AzureQueueResponseThread.Start();
                }

                protected override void Dispose(bool disposing)
                {
                    if (disposing)
                    {
                        if (this.AzureQueueResponseThread != null)
                        {
                            this.AzureQueueResponseThread.Abort();
                        }
                    }

                    this.disposeFlag = true;
                    base.Dispose(disposing);
                }

                private void GetResponseThread()
                {
                    try
                    {
                        while (true)
                        {
                            // need to filter different client, action and deal with the end of responses
                            Message m = this.azureQueueProxy.ReceiveMessage(clientData);

                            this.callback.SendResponse(m);
                        }
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceInformation("GetResponseAQ Exception: {0}", e.ToString());

                        if (this.disposeFlag)
                        {
                            return;
                        }

                        Message exceptionMessage = Message.CreateMessage(MessageVersion.Soap11, @"http://hpc.microsoft.com/ClientSideExeption");
                        exceptionMessage.Headers.Add(MessageHeader.CreateHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS, clientData));
                        exceptionMessage.Properties.Add(@"AQClientException", e);

                        this.callback.SendResponse(exceptionMessage);
                    }
                    finally
                    {
                        if (this.Completed != null)
                        {
                            this.Completed(this, EventArgs.Empty);
                        }
                    }
                }
            }

            private interface IResponseHandler
            {
                string Action();

                string ClientData();

                event EventHandler Completed;

                void Close();
            }
        }

        /// <summary>
        /// Adapt IRequestChannel to IBrokerFrontend
        /// </summary>
        private class SendRequestAdapter : IRequestChannel, IDisposable
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

            #region IRequestChannel Members

            IAsyncResult IRequestChannel.BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            IAsyncResult IRequestChannel.BeginRequest(Message message, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            Message IRequestChannel.EndRequest(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            EndpointAddress IRequestChannel.RemoteAddress
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            Message IRequestChannel.Request(Message message, TimeSpan timeout)
            {
                this.frontend.SendRequest(message);
                return null;
            }

            Message IRequestChannel.Request(Message message)
            {
                this.frontend.SendRequest(message);
                return null;
            }

            Uri IRequestChannel.Via
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

            CommunicationState System.ServiceModel.ICommunicationObject.State
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
            public BrokerControllerClient(Binding binding, EndpointAddress epr) : base(binding, epr)
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

            public BrokerResponseMessages PullResponses(string action, GetResponsePosition position, int count, string clientId)
            {
                IAsyncResult result = this.Channel.BeginPullResponses(action, position, count, clientId, null, null);
                return this.Channel.EndPullResponses(result);
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
                IAsyncResult result = this.Channel.BeginGetResponsesAQ(action, clientData, resetToBegin, count, clientId, sessionHash, null, null);
                this.Channel.EndGetResponsesAQ(out azureResponseQueueUri, out azureResponseBlobUri, result);
            }

            public IAsyncResult BeginGetResponsesAQ(
                string action,
                string clientData,
                GetResponsePosition resetToBegin,
                int count,
                string clientId,
                int sessionHash,
                AsyncCallback callback,
                object asyncState)
            {
                return this.Channel.BeginGetResponsesAQ(action, clientData, resetToBegin, count, clientId, sessionHash, callback, asyncState);
            }

            public void EndGetResponsesAQ(out string azureResponseQueueUri, out string azureResponseBlobUri, IAsyncResult result)
            {
                this.Channel.EndGetResponsesAQ(out azureResponseQueueUri, out azureResponseBlobUri, result);
            }

            #region NotImplemented

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
                            ClientCredentials clientCredentials = new ClientCredentials();
                            clientCredentials.UserName.UserName = this.info.Username;
                            clientCredentials.UserName.Password = this.info.InternalPassword;
                            BindingParameterCollection bindingParams = new BindingParameterCollection();
                            bindingParams.Add(clientCredentials);
                            this.brokerClientFactory = binding.BuildChannelFactory<IRequestChannel>(bindingParams);
                            this.brokerClientFactory.Open();
                        }

                        client = this.brokerClientFactory.CreateChannel(GenerateEndpointAddress(info.BrokerEpr, this.scheme, this.info.Secure, this.info.IsAadOrLocalUser));
                        break;

                    case ClientType.Controller:
                        BrokerControllerClient controllerClient = new BrokerControllerClient(
                            this.binding,
                            GenerateEndpointAddress(this.info.ControllerEpr, this.scheme, this.info.Secure, this.info.IsAadOrLocalUser));
                        controllerClient.ClientCredentials.UserName.UserName = this.info.Username;
                        controllerClient.ClientCredentials.UserName.Password = this.info.InternalPassword;
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
    }
}