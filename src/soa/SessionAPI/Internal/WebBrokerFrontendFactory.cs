//------------------------------------------------------------------------------
// <copyright file="WebBrokerFrontendFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Provides a broker frontend factory to build proxy to communicate with
//      broker frontend through REST interface
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Web;
    using System.Threading;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Provides a broker frontend factory to build proxy to communicate with
    /// broker frontend through REST interface
    /// </summary>
    internal class WebBrokerFrontendFactory : BrokerFrontendFactory
    {
        /// <summary>
        /// Stores the web broker frontend for send request
        /// </summary>
        private WebBrokerFrontendForSendRequest frontendForSendRequest;

        /// <summary>
        /// Stores the web broker frontend for get response
        /// </summary>
        private WebBrokerFrontendForGetResponse frontendForGetResponse;

        /// <summary>
        /// Stores the lock object to protect code logic to create the instance of the
        /// WebBrokerFrontendForSendRequest class
        /// </summary>
        private object lockToCreateWebBrokerFrontendForSendRequest = new object();

        /// <summary>
        /// Stores the lock object to protect code logic to create the instance of the
        /// WebBrokerFrontendForGetResponse class
        /// </summary>
        private object lockToCreateWebBrokerFrontendForGetResponse = new object();

        // /// <summary>
        // /// Stores the web session info instance
        // /// </summary>
        // private WebSessionInfo info;

        /// <summary>
        /// Initializes a new instance of the WebBrokerFrontendFactory class
        /// </summary>
        /// <param name="info">indicating the web session info</param>
        public WebBrokerFrontendFactory(WebSessionInfo info, string clientId, IResponseServiceCallback callback)
            : base(clientId, callback)
        {
            this.info = info;
        }

        /// <summary>
        /// Get broker client proxy for send request
        /// </summary>
        /// <returns>returns broker client proxy for send request as IOutputChannel</returns>
        public override IChannel GetBrokerClient()
        {
            return this.GetWebBrokerFrontendForSendRequest();
        }

        /// <summary>
        /// Gets the controller client
        /// </summary>
        /// <returns>returns the controller client</returns>
        public override IController GetControllerClient()
        {
            return this.GetWebBrokerFrontendForSendRequest();
        }

        /// <summary>
        /// Gets controller client and set operation timeout if it is a WCF proxy
        /// </summary>
        /// <param name="operationTimeout">indicating the operation timeout</param>
        /// <returns>returns IController instance</returns>
        public override IController GetControllerClient(TimeSpan operationTimeout)
        {
            return this.GetControllerClient((int)operationTimeout.TotalMilliseconds);
        }

        /// <summary>
        /// Gets controller client and set operation timeout if it is a WCF proxy
        /// </summary>
        /// <param name="timeoutMilliseconds">indicating the operation timeout</param>
        /// <returns>returns IController instance</returns>
        public override IController GetControllerClient(int timeoutMilliseconds)
        {
            return this.GetControllerClient();
        }

        /// <summary>
        /// Gets the response service client
        /// </summary>
        /// <returns>returns the response service client</returns>
        public override IResponseService GetResponseServiceClient()
        {
            return this.GetWebBrokerFrontendForGetResponse();
        }

        /// <summary>
        /// Set close timeout (take no effect)
        /// </summary>
        /// <param name="timeoutMilliseconds">indicating timeout</param>
        public override void SetCloseTimeout(int timeoutMilliseconds)
        {
        }

        /// <summary>
        /// Close the frontend for send request
        /// </summary>
        /// <param name="setToNull">Whether to set client object to null after closing. THis should only be true if caller is within BrokerClient's objectLock to ensure
        /// another thread isnt using or about to use it</param>
        /// <param name="timeoutInMS">How long to wait for close. -1 means use binding's close timeout</param>
        public override void CloseBrokerClient(bool setToNull, int timeoutInMS)
        {
            if (this.frontendForSendRequest != null)
            {
                lock (this.lockToCreateWebBrokerFrontendForSendRequest)
                {
                    if (this.frontendForSendRequest != null)
                    {
                        this.frontendForSendRequest.Close();
                        if (setToNull)
                        {
                            this.frontendForSendRequest = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dispose the instance of the WebBrokerFrontendFactory class
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.frontendForSendRequest != null)
                {
                    this.frontendForSendRequest.Close();
                }

                if (this.frontendForGetResponse != null)
                {
                    this.frontendForGetResponse.Close();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the instance of the WebBrokerFrontendForSendRequest class
        /// Create one if it does not exist
        /// </summary>
        /// <returns>returns the instance of the WebBrokerFrontendForSendRequest class</returns>
        private WebBrokerFrontendForSendRequest GetWebBrokerFrontendForSendRequest()
        {
            if (this.frontendForSendRequest == null || this.frontendForSendRequest.IsFaulted)
            {
                lock (this.lockToCreateWebBrokerFrontendForSendRequest)
                {
                    if (this.frontendForSendRequest != null && this.frontendForSendRequest.IsFaulted)
                    {
                        SessionBase.TraceSource.TraceInformation("FrontendForSendRequest is in Faulted state. Recreate it.");

                        this.frontendForSendRequest.Close();
                        this.frontendForSendRequest = null;
                    }

                    if (this.frontendForSendRequest == null)
                    {
                        this.frontendForSendRequest = new WebBrokerFrontendForSendRequest(
                            info.BrokerNode, info.Id, this.ClientId, info.Credential);
                    }
                }
            }

            return this.frontendForSendRequest;
        }

        /// <summary>
        /// Gets the instance of the WebBrokerFrontendForGetResponse class
        /// Create one if it does not exist
        /// </summary>
        /// <returns>returns the instance of the WebBrokerFrontendForGetResponse class</returns>
        private WebBrokerFrontendForGetResponse GetWebBrokerFrontendForGetResponse()
        {
            if (this.frontendForGetResponse == null)
            {
                lock (this.lockToCreateWebBrokerFrontendForGetResponse)
                {
                    if (this.frontendForGetResponse == null)
                    {
                        this.frontendForGetResponse = new WebBrokerFrontendForGetResponse(this.info.Id, this.info.BrokerNode, this.info.Credential, this.ResponseCallback);
                    }
                }
            }

            return this.frontendForGetResponse;
        }

        /// <summary>
        /// Provides a broker frontend proxy communicating with SOA Web service
        /// This broker frontend proxy is in charge of sending request
        /// </summary>
        private class WebBrokerFrontendForSendRequest : DisposableObject, IOutputChannel, IController
        {
            /// <summary>
            /// Stores the web request to send request
            /// </summary>
            private HttpWebRequest webRequestToSendRequest;

            /// <summary>
            /// Stores the writer to write WCF message into the request content stream
            /// </summary>
            private XmlDictionaryWriter writer;

            /// <summary>
            /// Stores the request content stream
            /// </summary>
            private Stream requestContentStream;

            /// <summary>
            /// Stores a flag indicating whether EOM has been called
            /// </summary>
            private bool endRequestCalled;

            /// <summary>
            /// Stores a flag indicating whether the web request has been closed
            /// </summary>
            private bool webRequestClosed;

            /// <summary>
            /// Stores a flag indicating whether the web request to send request has been created
            /// </summary>
            private bool webRequestCreated;

            /// <summary>
            /// Stores the broker node
            /// </summary>
            private string brokerNode;

            /// <summary>
            /// Stores the session id
            /// </summary>
            private int sessionId;

            /// <summary>
            /// Stores the client id
            /// </summary>
            private string clientId;

            /// <summary>
            /// Stores the credential
            /// </summary>
            private NetworkCredential credential;

            /// <summary>
            /// Stores the data contract serializer to serialize instances of BrokerClientStatus
            /// </summary>
            private DataContractSerializer serializerForBrokerClientStatus = new DataContractSerializer(typeof(BrokerClientStatus));

            /// <summary>
            /// A flag tells if this WebBrokerFrontendForSendRequest is in faulted state and should not be used.
            /// </summary>
            private bool faultFlag;

            /// <summary>
            /// Return if this instance is in faulted state
            /// </summary>
            public bool IsFaulted
            {
                get { return this.faultFlag; }
            }

            /// <summary>
            /// Initializes a new instance of the WebBrokerFrontendForSendRequest class
            /// </summary>
            /// <param name="brokerNode">indicating the broker node</param>
            /// <param name="sessionId">indicating the session id</param>
            /// <param name="clientId">indicating the client id</param>
            /// <param name="credential">indicating the credential</param>
            public WebBrokerFrontendForSendRequest(string brokerNode, int sessionId, string clientId, NetworkCredential credential)
            {
                this.brokerNode = brokerNode;
                this.sessionId = sessionId;
                this.clientId = clientId;
                this.credential = credential;
            }

            /// <summary>
            /// Close the frontend proxy
            /// </summary>
            /// <param name="timeout">indicating the timeout (discarded)</param>
            void ICommunicationObject.Close(TimeSpan timeout)
            {
                ((ICommunicationObject)this).Close();
            }

            /// <summary>
            /// Close the frontend proxy
            /// </summary>
            void ICommunicationObject.Close()
            {
            }

            /// <summary>
            /// Send out message
            /// </summary>
            /// <param name="message">indicating the message to send</param>
            /// <param name="timeout">indicating the timeout (discarded)</param>
            void IOutputChannel.Send(Message message, TimeSpan timeout)
            {
                ((IOutputChannel)this).Send(message);
            }

            /// <summary>
            /// Send out message
            /// </summary>
            /// <param name="message">indicating the message to send</param>
            void IOutputChannel.Send(Message message)
            {
                if (this.endRequestCalled)
                {
                    return;
                }

                if (this.webRequestClosed)
                {
                    throw new ObjectDisposedException("WebBrokerFrontendForSendRequest");
                }

                try
                {
                    if (!this.webRequestCreated)
                    {
                        this.StartNewRequest();
                    }

                    message.WriteMessage(this.writer);
                }
                catch (Exception)
                {
                    this.faultFlag = true;
                    throw;
                }
            }

            /// <summary>
            /// Flush the request
            /// for web frontend, this operation will flush the request content stream only
            /// </summary>
            /// <param name="count">indicating the count</param>
            /// <param name="clientid">indicating the client id</param>
            /// <param name="timeoutThrottlingMs">indicating the timeout for throttling</param>
            /// <param name="timeoutFlushMs">indicating the timeout for flush</param>
            void IController.Flush(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutFlushMs)
            {
                if (this.endRequestCalled)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_EOMReject_EndRequests, SR.Broker_EOMReject_EndRequests);
                }

                if (this.webRequestClosed)
                {
                    throw new ObjectDisposedException("WebBrokerFrontendForSendRequest");
                }

                this.CloseCurrentWebRequest(false);
            }

            /// <summary>
            /// Gets broker client status
            /// </summary>
            /// <param name="clientId">indicating the client id</param>
            /// <returns>returns broker client status</returns>
            BrokerClientStatus IController.GetBrokerClientStatus(string clientId)
            {
                HttpWebRequest request = SOAWebServiceRequestBuilder.GenerateGetBatchStatusWebRequest(this.brokerNode, this.sessionId, clientId, this.credential, WebMessageFormat.Xml);
                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    return (BrokerClientStatus)serializerForBrokerClientStatus.ReadObject(stream);
                }
            }

            /// <summary>
            /// Send EndRequest signal to broker controller
            /// </summary>
            /// <param name="count">indicating the count</param>
            /// <param name="clientid">indicating the client id</param>
            /// <param name="timeoutThrottlingMs">indicating the timeout for throttling</param>
            /// <param name="timeoutFlushMs">indicating the timeout for flush</param>
            void IController.EndRequests(int count, string clientid, int batchId, int timeoutThrottlingMs, int timeoutEOMMs)
            {
                if (this.endRequestCalled)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_EOMReject_EndRequests, SR.Broker_EOMReject_EndRequests);
                }

                this.CloseCurrentWebRequest(true);

                HttpWebRequest request = SOAWebServiceRequestBuilder.GenerateCommitBatchWebRequest(this.brokerNode, this.sessionId, this.clientId, this.credential);
                using (request.GetResponse()) { }
                this.endRequestCalled = true;
            }

            /// <summary>
            /// Purge the client
            /// </summary>
            /// <param name="clientid">indicating the client id</param>
            void IController.Purge(string clientid)
            {
                HttpWebRequest request = SOAWebServiceRequestBuilder.GeneratePurgeBatchWebRequest(this.brokerNode, this.sessionId, this.clientId, this.credential);
                using (request.GetResponse()) { }
            }

            /// <summary>
            /// Starts a new HTTP request to send requests
            /// </summary>
            private void StartNewRequest()
            {
                this.webRequestToSendRequest = SOAWebServiceRequestBuilder.GenerateSendRequestWebRequest(this.brokerNode, this.sessionId, this.clientId, false, this.credential);
                this.webRequestToSendRequest.AllowWriteStreamBuffering = false;
                this.requestContentStream = this.webRequestToSendRequest.GetRequestStream();
                this.writer = XmlDictionaryWriter.CreateTextWriter(this.requestContentStream);
                this.writer.WriteStartElement(Constant.WSMessageArrayElementName);
                this.webRequestCreated = true;
            }

            /// <summary>
            /// Close current web request
            /// </summary>
            /// <param name="setAsClosed">indicating a value whether to set webRequestClosed to true</param>
            private void CloseCurrentWebRequest(bool setAsClosed)
            {
                if (this.webRequestClosed)
                {
                    return;
                }

                // As the connection is created lazy, if the web request
                // was not created at this time, just set closed flag and return
                if (!this.webRequestCreated)
                {
                    this.webRequestClosed = setAsClosed;
                    return;
                }

                Exception ex = null;
                try
                {
                    this.writer.WriteEndElement();
                    this.writer.Close();
                }
                catch (Exception e)
                {
                    ex = e;
                    SessionBase.TraceSource.TraceEvent(
                        System.Diagnostics.TraceEventType.Error,
                        0,
                        "[WebBrokerFrontendForSendRequest] Failed to close the instance of XmlDictionaryWriter class: {0}", e);
                }

                try
                {
                    this.requestContentStream.Close();
                }
                catch (Exception e)
                {
                    ex = e;
                    SessionBase.TraceSource.TraceEvent(
                        System.Diagnostics.TraceEventType.Error,
                        0,
                        "[WebBrokerFrontendForSendRequest] Failed to close the request content stream: {0}", e);
                }

                try
                {
                    using (this.webRequestToSendRequest.GetResponse()) { }
                }
                catch (WebException e)
                {
                    SessionBase.TraceSource.TraceEvent(
                        System.Diagnostics.TraceEventType.Error,
                        0,
                        "[WebBrokerFrontendForSendRequest] Failed to get http response for the web request to send requests: {0}", e);
                    Utility.HandleWebException(e);
                }
                finally
                {
                    this.webRequestClosed = setAsClosed;
                    this.webRequestCreated = false;
                }

                if (ex != null)
                {
                    this.faultFlag = true;
                    throw ex;
                }
            }

            /// <summary>
            /// Dispose the instance of the WebBrokerFrontendForSendRequest class
            /// </summary>
            /// <param name="disposing">indicating whether it is disposing</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (!this.webRequestClosed)
                    {
                        try
                        {
                            if (this.writer != null)
                            {
                                this.writer.Close();
                            }
                        }
                        catch { }

                        try
                        {
                            if (this.requestContentStream != null)
                            {
                                this.requestContentStream.Close();
                            }
                        }
                        catch { }
                    }

                    if (!this.endRequestCalled)
                    {
                        try
                        {
                            if (this.webRequestToSendRequest != null)
                            {
                                this.webRequestToSendRequest.Abort();
                            }
                        }
                        catch { }
                    }
                }

                base.Dispose(disposing);
            }

            #region Not supported operations
            IAsyncResult IOutputChannel.BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            IAsyncResult IOutputChannel.BeginSend(Message message, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            void IOutputChannel.EndSend(IAsyncResult result)
            {
                throw new NotSupportedException();
            }

            System.ServiceModel.EndpointAddress IOutputChannel.RemoteAddress
            {
                get { throw new NotSupportedException(); }
            }

            Uri IOutputChannel.Via
            {
                get { throw new NotSupportedException(); }
            }

            T IChannel.GetProperty<T>()
            {
                throw new NotSupportedException();
            }

            void System.ServiceModel.ICommunicationObject.Abort()
            {
                throw new NotSupportedException();
            }

            IAsyncResult System.ServiceModel.ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            IAsyncResult System.ServiceModel.ICommunicationObject.BeginClose(AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            IAsyncResult System.ServiceModel.ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            IAsyncResult System.ServiceModel.ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Closed
            {
                add { throw new NotSupportedException(); }
                remove { throw new NotSupportedException(); }
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Closing
            {
                add { throw new NotSupportedException(); }
                remove { throw new NotSupportedException(); }
            }

            void System.ServiceModel.ICommunicationObject.EndClose(IAsyncResult result)
            {
                throw new NotSupportedException();
            }

            void System.ServiceModel.ICommunicationObject.EndOpen(IAsyncResult result)
            {
                throw new NotSupportedException();
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Faulted
            {
                add { throw new NotSupportedException(); }
                remove { throw new NotSupportedException(); }
            }

            void System.ServiceModel.ICommunicationObject.Open(TimeSpan timeout)
            {
                throw new NotSupportedException();
            }

            void System.ServiceModel.ICommunicationObject.Open()
            {
                throw new NotSupportedException();
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Opened
            {
                add { throw new NotSupportedException(); }
                remove { throw new NotSupportedException(); }
            }

            event EventHandler System.ServiceModel.ICommunicationObject.Opening
            {
                add { throw new NotSupportedException(); }
                remove { throw new NotSupportedException(); }
            }

            System.ServiceModel.CommunicationState System.ServiceModel.ICommunicationObject.State
            {
                get { throw new NotSupportedException(); }
            }

            int IController.GetRequestsCount(string clientId)
            {
                throw new NotSupportedException();
            }

            BrokerResponseMessages IController.PullResponses(string action, GetResponsePosition position, int count, string clientId)
            {
                throw new NotSupportedException();
            }

            void IController.Ping()
            {
                throw new NotSupportedException();
            }

            public void GetResponsesAQ(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId, int sessinHash, out string azureResponseQueueUri, out string azureResponseBlobUri)
            {
                throw new NotImplementedException();
            }

            #endregion

        }

        /// <summary>
        /// Provides a broker frontend proxy communicating with SOA Web service
        /// This broker frontend proxy is in charge of getting response
        /// </summary>
        private class WebBrokerFrontendForGetResponse : DisposableObject, IResponseService
        {
            /// <summary>
            /// Stores the callback instance
            /// </summary>
            private IResponseServiceCallback callback;

            /// <summary>
            /// Stores the broker node
            /// </summary>
            private string brokerNode;

            /// <summary>
            /// Stores the session id
            /// </summary>
            private int sessionId;

            /// <summary>
            /// Stores the network credential
            /// </summary>
            private NetworkCredential credential;

            /// <summary>
            /// Stores the response handler list
            /// </summary>
            private List<WebResponseHandler> handlers = new List<WebResponseHandler>();

            /// <summary>
            /// Stores the lock object to protect the list of handlers
            /// </summary>
            private object lockObjectToProtectHandlers = new object();

            /// <summary>
            /// Initializes a new instance of the WebBrokerFrontendForGetResponse class
            /// </summary>
            /// <param name="sessionId">indicating the session id</param>
            /// <param name="brokerNode">indicating the broker node</param>
            /// <param name="credential">indicating the network credential</param>
            /// <param name="callback">indicating the response callback instance</param>
            public WebBrokerFrontendForGetResponse(int sessionId, string brokerNode, NetworkCredential credential, IResponseServiceCallback callback)
            {
                this.sessionId = sessionId;
                this.brokerNode = brokerNode;
                this.credential = credential;
                this.callback = callback;
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
                lock (this.lockObjectToProtectHandlers)
                {
                    if (this.handlers == null)
                    {
                        throw new ObjectDisposedException("WebBrokerFrontendForGetResponse");
                    }

                    HttpWebRequest request = SOAWebServiceRequestBuilder.GenerateGetResponseWebRequest(this.brokerNode, this.sessionId, clientId, this.credential, action, clientData, count, resetToBegin == GetResponsePosition.Begin);
                    WebResponseHandler handler = new WebResponseHandler(request, clientData, this.callback);
                    this.handlers.Add(handler);
                    handler.Completed += new EventHandler(Handler_Completed);
                }
            }

            /// <summary>
            /// Dispose the frontend
            /// </summary>
            /// <param name="disposing">indicating whether it is disposing</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    List<WebResponseHandler> list = new List<WebResponseHandler>();

                    lock (this.lockObjectToProtectHandlers)
                    {
                        if (this.handlers != null)
                        {
                            foreach (WebResponseHandler handler in this.handlers)
                            {
                                // Remove the handler event
                                handler.Completed -= new EventHandler(Handler_Completed);
                                list.Add(handler);
                            }

                            this.handlers = null;
                        }
                    }

                    // In order to avoid deadlock, should not call following Close method in above lock block.
                    foreach (WebResponseHandler handle in list)
                    {
                        handle.Close();
                    }
                }

                base.Dispose(disposing);
            }

            /// <summary>
            /// Handle handler complete event
            /// </summary>
            /// <param name="sender">indicating the sender</param>
            /// <param name="e">indicating the event args</param>
            private void Handler_Completed(object sender, EventArgs e)
            {
                lock (this.lockObjectToProtectHandlers)
                {
                    if (this.handlers != null)
                    {
                        this.handlers.Remove((WebResponseHandler)sender);
                    }
                }
            }

            /// <summary>
            /// Provides a response handler to read message from the web response
            /// </summary>
            private class WebResponseHandler : DisposableObject
            {
                /// <summary>
                /// Stores the max size of headers
                /// </summary>
                private const int MaxSizeOfHeaders = int.MaxValue;

                /// <summary>
                /// Stores the default reader quotas
                /// </summary>
                private static readonly XmlDictionaryReaderQuotas DefaultReaderQuotas = XmlDictionaryReaderQuotas.Max;

                /// <summary>
                /// Stores the supported message version
                /// </summary>
                private static readonly MessageVersion SupportedMessageVersion = MessageVersion.Soap12WSAddressing10;

                /// <summary>
                /// Stores the callback instance
                /// </summary>
                private IResponseServiceCallback callback;

                /// <summary>
                /// Stores the client data
                /// </summary>
                private string clientData;

                /// <summary>
                /// Stores the instance of HttpWebRequest class
                /// </summary>
                private HttpWebRequest request;

                /// <summary>
                /// Stores the typed message conveter for EndOfResponse
                /// </summary>
                private TypedMessageConverter endOfResponseMessageConverter = TypedMessageConverter.Create(typeof(EndOfResponses), Constant.EndOfMessageAction);

                /// <summary>
                /// Stores the thread to parse response
                /// </summary>
                private Thread parsingResponseThread;

                /// <summary>
                /// Stores the dispose flag indicating whether this object has been disposed
                /// </summary>
                private volatile bool disposeFlag;

                /// <summary>
                /// Initializes a new instance of the WebResponseHandler class
                /// </summary>
                /// <param name="response">indicating the web request</param>
                /// <param name="clientData">indicating the client data</param>
                /// <param name="callback">indicating the callback instance</param>
                public WebResponseHandler(HttpWebRequest request, string clientData, IResponseServiceCallback callback)
                {
                    this.request = request;
                    this.clientData = clientData;
                    this.callback = callback;

                    this.parsingResponseThread = new Thread(this.ParsingResponseThreadProc);
                    this.parsingResponseThread.IsBackground = true;
                    this.parsingResponseThread.Start();
                }

                /// <summary>
                /// Gets the event triggered when the handler completed parsing all responses
                /// </summary>
                public event EventHandler Completed;

                /// <summary>
                /// Dispose the response handler
                /// </summary>
                /// <param name="disposing">indicating whether it is disposing</param>
                protected override void Dispose(bool disposing)
                {
                    // Set the dispose flag so that the worker thread would be exited
                    this.disposeFlag = true;

                    base.Dispose(disposing);
                }

                /// <summary>
                /// Read message from the reader
                /// </summary>
                /// <returns>returns the message instance read from the stream</returns>
                private static Message ReadMessage(XmlDictionaryReader reader)
                {
                    while (reader.NodeType != XmlNodeType.Element)
                    {
                        if (reader.EOF)
                        {
                            return null;
                        }

                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            reader.ReadEndElement();
                        }
                        else
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }
                        }
                    }

                    Message m = Message.CreateMessage(reader.ReadSubtree(), MaxSizeOfHeaders, SupportedMessageVersion);
                    return m;
                }

                /// <summary>
                /// Parsing response stream and fetch response mesage from it
                /// </summary>
                private void ParsingResponseThreadProc()
                {
                    try
                    {
                        using (WebResponse response = this.request.GetResponse())
                        using (Stream responseStream = response.GetResponseStream())
                        using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(responseStream, DefaultReaderQuotas))
                        {
                            reader.ReadStartElement();

                            // this.disposeFlag is marked volatile so that the compiler
                            // won't cache the value here
                            while (!this.disposeFlag)
                            {
                                // In case of network failure and hang,
                                // this call should eventually timed out
                                Message m = ReadMessage(reader);
                                if (m == null)
                                {
                                    return;
                                }

                                if (m.Headers.Action.Equals(BrokerInstanceUnavailable.Action, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    TypedMessageConverter converter = TypedMessageConverter.Create(typeof(BrokerInstanceUnavailable), BrokerInstanceUnavailable.Action);
                                    BrokerInstanceUnavailable biu = (BrokerInstanceUnavailable)converter.FromMessage(m);
                                    SessionBase.TraceSource.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[WebResponseHandler] Received broker unavailable message, BrokerLauncherEpr = {0}, BrokerNodeDown= {1}", biu.BrokerLauncherEpr, biu.IsBrokerNodeDown);
                                    this.callback.SendBrokerDownSignal(biu.IsBrokerNodeDown);
                                    break;
                                }
                                else
                                {
                                    SessionBase.TraceSource.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "[WebResponseHandler] Received response message:  MessageId = {0}, Action = {1}", m.Headers.MessageId, m.Headers.Action);
                                    this.callback.SendResponse(m);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[WebBrokerFrontendForGetResponse] Failed to read response from the response stream: {0}", e);
                        if (this.disposeFlag)
                        {
                            // If it has already been disposed, do not
                            // pass the exception to the enumerator
                            return;
                        }

                        Exception exceptionToThrow = e;
                        WebException we = e as WebException;
                        if (we != null)
                        {
                            exceptionToThrow = WebAPIUtility.ConvertWebException(we);
                            FaultException<SessionFault> faultException = exceptionToThrow as FaultException<SessionFault>;
                            if (faultException != null)
                            {
                                exceptionToThrow = Utility.TranslateFaultException(faultException);
                            }
                        }

                        //Bug #15946: throw exception instead of signaling broker down.
                        //Note: To pass the exception to AsyncResponseCallback, wrap it as a response message with special action field.
                        Message exceptionMessage = Message.CreateMessage(MessageVersion.Default, Constant.WebAPI_ClientSideException);
                        exceptionMessage.Headers.Add(MessageHeader.CreateHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS, clientData));
                        exceptionMessage.Properties.Add(Constant.WebAPI_ClientSideException, exceptionToThrow);

                        this.callback.SendResponse(exceptionMessage);
                    }
                    finally
                    {
                        // Two threads are involved here:
                        // first thread: it is disposing BrokerClient, and now it is at WebBrokerFrontendForGetResponse.Dispose
                        // second thread: it is "ParsingResponseThreadProc" worker thread

                        // The first thread holds a lock "lockObjectToProtectHandlers" and calls WebResponseHandler.Dispose method,
                        // which attempts to abort second thread. But the second thread is in finally block here, it indefinitely
                        // delays the abort. So the first thread is blocked at "this.parsingResponseThread.Abort()".

                        // The second thread calls following "this.Completed" method, which waits for the same lock "lockObjectToProtectHandlers".
                        // So the second thread is blocked on that lock.

                        // In order to avoid deadlock, change the first thread. It should not hold a lock when aborts the second thread.

                        if (this.Completed != null)
                        {
                            this.Completed(this, EventArgs.Empty);
                        }
                    }
                }
            }
        }

        // not implemented
        public override AzureQueueProxy GetBrokerClientAQ()
        {
            throw new NotImplementedException();
        }
    }
}
