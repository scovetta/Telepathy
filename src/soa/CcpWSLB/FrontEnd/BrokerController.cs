// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.FrontEnd
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.FrontEnd.AzureQueue;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Implementation the broker controller service
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true,
                    Name = "BrokerController", Namespace = "http://hpc.microsoft.com/brokercontroller/")]
    internal class BrokerController : IBrokerFrontend, IDisposable
    {
        /// <summary>
        /// Stores the broker client for this instance (which is available within a channel)
        /// </summary>
        private BrokerClient client;

        /// <summary>
        /// Stores the lock to create client
        /// </summary>
        private object lockToCreateClient = new object();

        /// <summary>
        /// Stores a value indicating whether the broker controller is a singleton
        /// </summary>
        private bool isSingleton;

        /// <summary>
        /// Stores a flag indicating whether the broker controller has been disposed
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Stores the client manager
        /// </summary>
        private BrokerClientManager clientManager;

        /// <summary>
        /// Stores the broker authorization
        /// </summary>
        private BrokerAuthorization brokerAuth;

        /// <summary>
        /// Stores the callback instance
        /// </summary>
        private IResponseServiceCallback callbackInstance;

        /// <summary>
        /// Stores broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Initializes a new instance of the BrokerController class
        /// </summary>
        /// <param name="isSingleton">indicating whether the BrokerController is a singleton</param>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="brokerAuth">indicating the broker authorization</param>
        /// <param name="observer">indicating broker observer</param>
        /// <param name="azureQueueProxy">the Azure storage proxy</param>
        public BrokerController(bool isSingleton, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, BrokerObserver observer, AzureQueueProxy azureQueueProxy)
        {
            this.isSingleton = isSingleton;
            this.clientManager = clientManager;
            this.brokerAuth = brokerAuth;
            this.observer = observer;
            this.callbackInstance = azureQueueProxy;
        }

        /// <summary>
        /// Initializes a new instance of the BrokerController class
        /// This constructor is for inprocess broker as IResponseServiceCallback is directly passed
        /// isSingleton is set to false and BrokerAuth is set to null for inprocess broker
        /// </summary>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="callbackInstance">indicating the callback instance</param>
        /// <param name="observer">indicating broker observer</param>
        public BrokerController(BrokerClientManager clientManager, IResponseServiceCallback callbackInstance, BrokerObserver observer)
            : this(false, clientManager, null, observer, null)
        {
            this.callbackInstance = callbackInstance;
        }

        /// <summary>
        /// Finalizes an instance of the BrokerController class
        /// </summary>
        ~BrokerController()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets a value indicating whether EndRequest has been received by the corresponding broker client
        /// </summary>
        bool IBrokerFrontend.EndRequestReceived(string clientId)
        {
            return (int)this.GetClient(clientId).State >= (int)BrokerClientState.EndRequests;
        }

        /// <summary>
        /// Flush request messages
        /// </summary>
        /// <param name="count">the number of request messages sent after last Flush operation</param>
        /// <param name="timeoutMs">indicating the timeout in MS</param>
        /// <param name="clientId">indicating the client id</param>
        public void Flush(int count, string clientId, int batchId, int timeoutThrottlingMs, int timeoutFlushMs)
        {
            ParamCheckUtility.ThrowIfOutofRange(count <= 0, "count");
            ParamCheckUtility.ThrowIfOutofRange(timeoutThrottlingMs <= 0 && timeoutThrottlingMs != Timeout.Infinite, "timeoutThrottlingMs");
            ParamCheckUtility.ThrowIfOutofRange(timeoutFlushMs <= 0 && timeoutFlushMs != Timeout.Infinite, "timeoutFlushMs");
            ParamCheckUtility.ThrowIfNull(clientId, "clientId");
            ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
            ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);

            this.ThrowIfDisposed();
            this.CheckAuth();

            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerController] Receive Flush for Client {0}, Count = {1}", clientId, count);
            try
            {
                #region Debug Failure Test
                SimulateFailure.FailOperation(1);
                #endregion

                BrokerClient brokerClient = this.GetClient(clientId);
                FrontEndBase frontendBase = brokerClient.GetDuplexFrontEnd();

                WaitOnThrottling(frontendBase, timeoutThrottlingMs);

                // Flush, which waits until all requests are stored.
                brokerClient.Flush(count, batchId, timeoutFlushMs);

                #region Debug Failure Test
                SimulateFailure.FailOperation(2);
                #endregion
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[BrokerController] Flush failed for client {0}: {1}", clientId, e);
                throw TranslateException(e);
            }
        }

        /// <summary>
        /// Indicate the end of requeusts
        /// </summary>
        /// <param name="count">indicating the number of the messages</param>
        /// <param name="timeoutMs">indicating the timeout in MS</param>
        /// <param name="clientId">indicating the client id</param>
        public void EndRequests(int count, string clientId, int batchId, int timeoutThrottlingMs, int timeoutEOMMs)
        {
            ParamCheckUtility.ThrowIfOutofRange(count < 0, "count");
            ParamCheckUtility.ThrowIfOutofRange(timeoutThrottlingMs <= 0 && timeoutThrottlingMs != Timeout.Infinite, "timeoutThrottlingMs");
            ParamCheckUtility.ThrowIfOutofRange(timeoutEOMMs <= 0 && timeoutEOMMs != Timeout.Infinite, "timeoutEOMMs");
            ParamCheckUtility.ThrowIfNull(clientId, "clientId");
            ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
            ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);

            this.ThrowIfDisposed();
            this.CheckAuth();

            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerController] Receive EOM for Client {0}, Count = {1}", clientId, count);
            try
            {
                #region Debug Failure Test
                SimulateFailure.FailOperation(1);
                #endregion

                BrokerClient brokerClient = this.GetClient(clientId);
                FrontEndBase frontendBase = brokerClient.GetDuplexFrontEnd();

                WaitOnThrottling(frontendBase, timeoutThrottlingMs);

                // Then handle the EOM which waits until all requests are stored. Use user specified EndRequests timeout for this
                brokerClient.EndOfMessage(count, batchId, timeoutEOMMs);

                #region Debug Failure Test
                SimulateFailure.FailOperation(2);
                #endregion
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[BrokerController] EOM failed for client {0}: {1}", clientId, e);
                throw TranslateException(e);
            }
        }

        /// <summary>
        /// Gets broker client status
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <returns>returns the broker client status</returns>
        public BrokerClientStatus GetBrokerClientStatus(string clientId)
        {
            try
            {
                ParamCheckUtility.ThrowIfNull(clientId, "clientId");
                ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
                ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);

                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerController] Get broker client status for Client {0}", clientId);

                this.ThrowIfDisposed();
                this.CheckAuth();

                BrokerClientState state = this.GetClient(clientId).State;
                return BrokerClient.MapToBrokerClientStatus(state);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerController] Get broker client status for Client {1} Failed: {0}", e, clientId);
                throw TranslateException(e);
            }
        }

        /// <summary>
        /// Gets number of requests that have been committed into specified client
        /// </summary>
        /// <param name="clientId">client id</param>
        /// <returns>number of committed requests in the client with specified client id</returns>
        public int GetRequestsCount(string clientId)
        {
            try
            {
                ParamCheckUtility.ThrowIfNull(clientId, "clientId");
                ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
                ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);

                BrokerTracing.TraceInfo("[BrokerController] Get requests count for Client {0}", clientId);

                this.ThrowIfDisposed();
                this.CheckAuth();

                return this.GetClient(clientId).RequestsCount;
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[BrokerController] Get requests for Client {1} Failed: {0}", e, clientId);
                throw TranslateException(e);
            }
        }

        /// <summary>
        /// Get responses from client
        /// </summary>
        /// <param name="action">indicating the action</param>
        /// <param name="clientData">indicating the client data</param>
        /// <param name="resetToBegin">indicating the position</param>
        /// <param name="count">indicating the count</param>
        /// <param name="clientId">indicating the client id</param>
        public void GetResponses(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId)
        {
            try
            {
                BrokerTracing.TraceVerbose("[BrokerController] GetResponses is called for Client {0}.", clientId);

                ParamCheckUtility.ThrowIfOutofRange(count <= 0 && count != -1, "count");
                ParamCheckUtility.ThrowIfNull(clientId, "clientId");
                ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
                ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);

                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerController] GetResponses for Client {2}, Count = {0}, Position = {1}", count, resetToBegin, clientId);

                this.ThrowIfDisposed();
                this.CheckAuth();

                // Try to get callback instance for inprocess broker
                IResponseServiceCallback callbackInstance = this.callbackInstance;

                // If callback instance is null, get callback instance from operation context
                if (callbackInstance == null)
                {
                    callbackInstance = OperationContext.Current.GetCallbackChannel<IResponseServiceCallback>();
                }

                this.GetClient(clientId).GetResponses(action, clientData, resetToBegin, count, callbackInstance, GetMessageVersion());

                #region Debug Failure Test
                SimulateFailure.FailOperation(1);
                #endregion

                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerController] GetResponses for Client {0} Succeeded.", clientId);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerController] GetResponses for Client {1} Failed: {0}", e, clientId);
                throw TranslateException(e);
            }
        }

        /// <summary>
        /// Get response from the Azure storage
        /// </summary>
        /// <param name="action">indicating the action</param>
        /// <param name="clientData">indicating the client data</param>
        /// <param name="resetToBegin">indicating the position</param>
        /// <param name="count">indicating the count</param>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="sessionHash">specifying the session object hash</param>
        /// <param name="azureResponseQueueUri">return the Azure storage queue SAS Uri</param>
        /// <param name="azureResponseBlobUri">return the Azure storage blob container SAS Uri</param>
        public void GetResponsesAQ(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId, int sessionHash, out string azureResponseQueueUri, out string azureResponseBlobUri)
        {
            //create the azure response queue if not exist
            AzureQueueProxy queueProxy = this.callbackInstance as AzureQueueProxy;
            if (queueProxy == null)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerController] The callback instance is not AzureQueueProxy");
                azureResponseQueueUri = null;
                azureResponseBlobUri = null;
                return;
            }

            queueProxy.AddResponseQueues(clientData, sessionHash);

            azureResponseQueueUri = queueProxy.ResponseClientUris[sessionHash].Item1;
            azureResponseBlobUri = queueProxy.ResponseClientUris[sessionHash].Item2;

            this.GetResponses(action, clientData, resetToBegin, count, clientId);
        }

        /// <summary>
        /// Gets incoming message version
        /// </summary>
        /// <returns>returns message version</returns>
        private static MessageVersion GetMessageVersion()
        {
            if (OperationContext.Current == null)
            {
                return MessageVersion.Default;
            }
            else
            {
                return OperationContext.Current.IncomingMessageVersion;
            }
        }

        /// <summary>
        /// Send requests
        /// </summary>
        /// <param name="message">indicating the message</param>
        void IBrokerFrontend.SendRequest(Message message)
        {
            string clientId = FrontEndBase.GetClientId(message, String.Empty);

            try
            {
                ParamCheckUtility.ThrowIfNull(clientId, "clientId");
                ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
                ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);

                this.observer.IncomingRequest();
                this.GetClient(clientId).RequestReceived(DummyRequestContext.GetInstance(MessageVersion.Default), message, null).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerController] SendRequest {1} Failed: {0}", e, clientId);
                throw TranslateException(e);
            }
        }

        /// <summary>
        /// Purge a client
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        public void Purge(string clientId)
        {
            try
            {
                ParamCheckUtility.ThrowIfNull(clientId, "clientId");
                ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
                ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);

                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerController] Purge Client {0}", clientId);
                this.ThrowIfDisposed();
                this.CheckAuth();
                this.clientManager.PurgeClient(clientId, GetUserName()).GetAwaiter().GetResult();
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerController] Purge Client {0} Succeeded.", clientId);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerController] Purge Client {1} Failed: {0}", e, clientId);
                throw TranslateException(e);
            }
        }

        public void Ping()
        {
            // Ignore this.
        }

        /// <summary>
        /// For Java to pull the responses
        /// </summary>
        /// <param name="action">indicating the action</param>
        /// <param name="position">indicating the position</param>
        /// <param name="count">indicating the count</param>
        /// <param name="clientId">indicating the client id</param>
        /// <returns>returns the responses messages</returns>
        public BrokerResponseMessages PullResponses(string action, GetResponsePosition position, int count, string clientId)
        {
            try
            {
                ParamCheckUtility.ThrowIfOutofRange(count <= 0, "count");
                ParamCheckUtility.ThrowIfNull(clientId, "clientId");
                ParamCheckUtility.ThrowIfTooLong(clientId.Length, "clientId", Constant.MaxClientIdLength, SR.ClientIdTooLong);
                ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, clientId, "clientId", SR.InvalidClientId);

                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerController] PullResponses: Action = {0}, Position = {1}, Count = {2}", action, position, count);
                this.ThrowIfDisposed();
                this.CheckAuth();

                #region Debug Failure Test
                SimulateFailure.FailOperation(1);
                #endregion

                return this.GetClient(clientId).PullResponses(action, position, count, GetMessageVersion());
            }
            catch (Exception e)
            {
                BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[BrokerController] PullResponses Failed: {0}", e);
                throw TranslateException(e);
            }
        }

        /// <summary>
        /// Dispose the broker controller
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Translate the exception
        /// </summary>
        /// <param name="e">indicating the exception</param>
        /// <returns>translated exception</returns>
        private static Exception TranslateException(Exception e)
        {
            return ExceptionHelper.ConvertExceptionToFaultException(e);
        }

        /// <summary>
        /// Gets the user name
        /// </summary>
        /// <returns>return the user name</returns>
        private static string GetUserName()
        {
#if HPCPACK
            // case 1. This handles aad integration.
            if (Thread.CurrentPrincipal.IsHpcAadPrincipal(WinServiceHpcContextModule.GetOrAddWinServiceHpcContextFromEnv()))
            {
                return Thread.CurrentPrincipal.Identity.Name;
            }

            // case 2. OperationContext.Current == null.  This handles inproc broker
            if (OperationContext.Current == null)
            {
                // Returns the log on user of the current thread if OperationContext.Current is null (apply to inprocess broker)
                return WindowsIdentity.GetCurrent().Name;
            }
#endif

            // Stand alone mode goes here
            if (OperationContext.Current == null)
            {
                return Constant.AnonymousUserName;
            }

            // case 3. OperationContext.Current.ServiceSecurityContext == null.  This handles insecure binding
            // Note: SOA REST service always use secure binding.
            if (OperationContext.Current.ServiceSecurityContext == null)
            {
                return Constant.AnonymousUserName;
            }

            // case 4.  OperationContext.Current.ServiceSecurityContext != null and caller is not SOA REST service.
            if (OperationContext.Current.ServiceSecurityContext.IsAnonymous)
            {
                return Constant.AnonymousUserName;
            }

            return OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
        }

        /// <summary>
        /// Throw FaultException if the broker controller instance has been disposed
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.ClientTimeout, SR.ClientTimeout);
            }
        }

        /// <summary>
        /// Dispose the broker controller
        /// </summary>
        /// <param name="disposing">indicating whether it is diposing</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            if (this.client != null)
            {
                try
                {
                    this.client.FrontendDisconnected(this);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[BrokerController] Exception thrown while disposing: {0}", e);
                }
            }
        }

        /// <summary>
        /// Check the authentication
        /// </summary>
        private void CheckAuth()
        {
            if (this.brokerAuth != null && !this.brokerAuth.CheckAccess())
            {
                string user = "Anonymous";
                if (ServiceSecurityContext.Current.WindowsIdentity != null)
                {
                    user = ServiceSecurityContext.Current.WindowsIdentity.Name;
                }

                ThrowHelper.ThrowSessionFault(SOAFaultCode.AuthenticationFailure, SR.AuthenticationFailure, user);
            }
        }

        /// <summary>
        /// Gets the client
        /// If the broker client is null, a broker client is created by the client id and returned
        /// If the broker client is not null, the client id must match the broker client's id if this is not a singleton frontend, otherwise an exception would be throwed
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <returns>the broker client</returns>
        private BrokerClient GetClient(string clientId)
        {
            // Bug 3062: For http client, the connection is always the same thus we should allow different client id in the same channel
            if (this.isSingleton)
            {
                BrokerClient client = this.clientManager.GetClient(clientId, GetUserName());
                client.SingletonInstanceConnected();
                return client;
            }
            else
            {
                if (this.client == null)
                {
                    lock (this.lockToCreateClient)
                    {
                        if (this.client == null)
                        {
                            //get domainUsername.
                            string domainUsername;
                            if (SoaCommonConfig.WithoutSessionLayer)
                            {
                                string domain = Environment.UserDomainName;
                                string username = Environment.UserName;
                                if (domain != System.Net.Dns.GetHostName())
                                    domainUsername = Path.Combine(domain, username);
                                else
                                    domainUsername = username;
                            }
                            else
                            { 
                                domainUsername = GetUserName();
                            }
                            this.client = this.clientManager.GetClient(clientId, domainUsername);
                            this.client.FrontendConnected(this, this);
                            return this.client;
                        }
                    }
                }

                if (String.Compare(this.client.ClientId, clientId, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return this.client;
                }
                else
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_ClientIdNotMatch, SR.ClientIdNotMatch);
                    Debug.Fail("[ServiceJobMonitor] This line could not be reached.");
                    return null;
                }
            }
        }

        /// <summary>
        /// Check a frontend if throttling is enganged.  If so, wait throttling to complete; If it doesn't complete in timeoutThrottlingMs, thow TimeoutException.
        /// </summary>
        /// <param name="frontEndBase">the frontend to be checked</param>
        /// <param name="timeoutThrottlingMs">how long to wait before throttling completes</param>
        private static void WaitOnThrottling(FrontEndBase frontendBase, int timeoutThrottlingMs)
        {
            if (frontendBase == null)
            {
                return;
            }

            // If throttling is engaged wait for it to complete
            if (frontendBase.IsThrottlingEngaged)
            {
                // First wait on any throttling if it is enabled. Wait for as long as client wants which from .Net clients
                //  is the timeout used when sending requests
                if (!frontendBase.ThrottlingWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeoutThrottlingMs), false))
                {
                    // If the throttle timeout expires, return a timeout exception
                    throw TranslateException(new TimeoutException(SR.ThrottlingTimeoutExceeded));
                }
            }
        }
    }
}
