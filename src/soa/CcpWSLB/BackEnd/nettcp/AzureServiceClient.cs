// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.nettcp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Telepathy.ServiceBroker.Common;

    /// <summary>
    /// Service Client
    /// </summary>
    internal class AzureServiceClient : DisposableObjectSlim, IService
    {
        /// <summary>
        /// binding information
        /// </summary>
        private Binding binding;

        /// <summary>
        /// remote address
        /// </summary>
        private EndpointAddress remoteAddress;

        /// <summary>
        /// service operation timeout for the serviceClient
        /// </summary>
        private int serviceOperationTimeout;

        /// <summary>
        /// service client
        /// </summary>
        private ServiceClient serviceClient;

        /// <summary>
        /// Block the BeginProcessMessage until client.BeginOpen()'s call back is triggered.
        /// </summary>
        private ManualResetEvent waitHandleForOpen = new ManualResetEvent(false);

        /// <summary>
        /// It records if we already attempt to open the client.
        /// </summary>
        private int clientOpenAsync = 0;

        /// <summary>
        /// It is a list for the GetNextRequestState.
        /// When the client is opened, should trigger the GetNextRequest method.
        /// </summary>
        private List<GetNextRequestState> stateList = new List<GetNextRequestState>();

        /// <summary>
        /// It is a sync object to protect the stateList.
        /// </summary>
        private object SyncObjForStateList = new object();

        /// <summary>
        /// Get the instance of the service client.
        /// </summary>
        public ServiceClient ServiceClient
        {
            get { return this.serviceClient; }
        }

        /// <summary>
        /// Initializes a new instance of the AzureServiceClient class.
        /// </summary>
        /// <param name="binding">binding information</param>
        /// <param name="remoteAddress">remote address</param>
        public AzureServiceClient(Binding binding, EndpointAddress remoteAddress)
        {
            this.binding = binding;
            this.remoteAddress = remoteAddress;

            this.serviceClient = new ServiceClient(this.binding, this.remoteAddress);
            BrokerTracing.TraceVerbose("[AzureServiceClient]. AzureServiceClient: Create a new client {0}", this.serviceClient.ToString());
            this.serviceClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
            Utility.SetAzureClientCertificate(this.serviceClient.ChannelFactory.Credentials);
        }

        /// <summary>
        /// Recreate the service client.
        /// </summary>
        private void RecreateClient()
        {
            if (this.ServiceClient != null)
            {
                BrokerTracing.TraceVerbose("[AzureServiceClient]. ReCreateClient: Close the previous client {0}", this.ServiceClient.ToString());
                this.ServiceClient.AsyncClose();
            }

            this.serviceClient = new ServiceClient(this.binding, this.remoteAddress);
            BrokerTracing.TraceVerbose("[AzureServiceClient]. ReCreateClient: Create a new client {0}", this.serviceClient.ToString());
            this.serviceClient.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
            Utility.SetAzureClientCertificate(this.serviceClient.ChannelFactory.Credentials);

            this.clientOpenAsync = 0;

            this.AsyncStart(null, this.serviceOperationTimeout);
        }

        /// <summary>
        /// Opent the client and trigger GetNextRequest method.
        /// </summary>
        private static void AsyncStartWorker(object state)
        {
            BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStartWorker is called.");

            object[] objs = state as object[];
            Debug.Assert(objs != null && objs.Length == 2);

            AzureServiceClient client = objs[0] as AzureServiceClient;

            int serviceOperationTimeout = (int)objs[1];

            bool validConnection = false;

            string clientInfo = string.Empty;

            try
            {
                clientInfo = client.ToString();

                // open the channel explicitly when it is shared by multi threads
                client.ServiceClient.Open();

                BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStartWorker: Open the client succeeded, {0}", clientInfo);

                // setting client.InnerChannel.OperationTimeout causes the channel factory to open, and then causes client.Open to fail,
                // becasue we can't call Open when channel factory is already opened.
                // it is fine to set OperationTimeout after client.Open
                if (serviceOperationTimeout > 0)
                {
                    BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStartWorker: Set OperationTimeout {0} to the client {1}", serviceOperationTimeout, clientInfo);

                    client.ServiceClient.InnerChannel.OperationTimeout = TimeSpan.FromMilliseconds(serviceOperationTimeout);
                }

                validConnection = true;
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[AzureServiceClient]. AsyncStartWorker: Open the client failed, {0}, {1}", clientInfo, e);
            }

            try
            {
                if (validConnection)
                {
                    try
                    {
                        if (client.waitHandleForOpen != null)
                        {
                            BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStartWorker: Release the wait handle, {0}", clientInfo);
                            client.waitHandleForOpen.Set();
                        }
                    }
                    catch (Exception ex)
                    {
                        BrokerTracing.TraceWarning("[AzureServiceClient].AsyncStartWorker: Exception {0}", ex);

                        // Exception may happen if the client is already closed. Ignore this
                        // exception because the client released that WaitHandle when closed.
                    }

                    BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStartWorker: Call TriggerGetNextRequest, {0}", clientInfo);

                    // the client.WaitHandleForOpen.Set() is already called above before this.
                    client.TriggerGetNextRequest();
                }
                else
                {
                    client.RecreateClient();
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[AzureServiceClient]. AsyncStartWorker: Execption happens, {0}, {1}", clientInfo, e);
            }
        }

        /// <summary>
        /// Check if the client has the connection ready.
        /// </summary>
        /// <returns>the connection is ready or not</returns>
        public bool IsConnectionReady()
        {
            BrokerTracing.TraceVerbose("[AzureServiceClient]. IsConnectionReady is called.");

            try
            {
                if (this.waitHandleForOpen != null && this.waitHandleForOpen.WaitOne(0, false))
                {
                    return (this.ServiceClient.State == CommunicationState.Opened);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceWarning("[AzureServiceClient].IsConnectionReady: Exception {0}.", ex);

                // PingProxy thread calls this method before sending ping message.
                // Catch exception here, in case waitHandleForOpen is just closed or set null.
                return false;
            }
        }

        /// <summary>
        /// Async open the connection and start to process requests.
        /// </summary>
        public void AsyncStart(GetNextRequestState state, int serviceOperationTimeout)
        {
            BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStart is called.");

            this.serviceOperationTimeout = serviceOperationTimeout;

            Guid clientGuid = this.ServiceClient.ClientGuid;

            if (Interlocked.CompareExchange(ref this.clientOpenAsync, 1, 0) == 0)
            {
                if (state != null)
                {
                    lock (this.SyncObjForStateList)
                    {
                        // this.stateList cannot be null before BeginOpen is called.
                        BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStart: Add async state to the list before BeginOpen is called, client {0}", clientGuid);
                        this.stateList.Add(state);
                    }
                }

                BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStart: BeginOpen the client {0}", clientGuid);

                ThreadPool.QueueUserWorkItem(
                    new ThreadHelper<object>(new WaitCallback(AzureServiceClient.AsyncStartWorker)).CallbackRoot,
                    new object[] { this, this.serviceOperationTimeout });
            }
            else
            {
                BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStart: BeginOpen is already called, client {0}", clientGuid);

                if (state != null)
                {
                    lock (this.SyncObjForStateList)
                    {
                        if (this.stateList != null)
                        {
                            BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStart: Add async state to the list after BeginOpen is called, client {0}", clientGuid);
                            this.stateList.Add(state);
                        }
                        else
                        {
                            // if this.stateList is null, TriggerGetNextRequest is already called.
                            BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncStart: Call GetNextRequest method, client {0}", clientGuid);
                            state.Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Trigger the GetNextRequest delegates stored in the this.stateList.
        /// </summary>
        private void TriggerGetNextRequest()
        {
            BrokerTracing.TraceVerbose("[AzureServiceClient]. TriggerGetNextRequest is called.");

            List<GetNextRequestState> list = null;

            lock (this.SyncObjForStateList)
            {
                list = this.stateList;

                // setting it to null means the callback is already triggered.
                this.stateList = null;
            }

            if (list != null)
            {
                BrokerTracing.TraceVerbose("[AzureServiceClient]. TriggerGetNextRequest: Call GetNextRequest method, list count is {0}, {1}", list.Count, this);
                foreach (GetNextRequestState state in list)
                {
                    state.Invoke();
                }
            }
        }

        /// <summary>
        /// Close the instance, primarily the underlying communication object.
        /// Close the waitHandleForOpen handle.
        /// </summary>
        public void AsyncClose()
        {
            BrokerTracing.TraceVerbose("[AzureServiceClient]. AsyncClose: Close client {0}.", this);

            if (this.waitHandleForOpen != null)
            {
                try
                {
                    this.waitHandleForOpen.Close();
                    this.waitHandleForOpen = null;
                }
                catch (Exception ex)
                {
                    BrokerTracing.TraceWarning("[AzureServiceClient].AsyncClose: Exception {0}", ex);
                }
            }

            this.ServiceClient.AsyncClose();
        }

        /// <summary>
        /// Include client state and id in the return value.
        /// </summary>
        public override string ToString()
        {
            return this.ServiceClient.ToString();
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        protected override void DisposeInternal()
        {
            BrokerTracing.TraceVerbose("[AzureServiceClient]. DisposeInternal: Dispose client {0}.", this);

            base.DisposeInternal();

            this.AsyncClose();
        }

        public Message ProcessMessage(Message request)
        {
            // Impersonate the broker's identity. If this is a non-failover BN, BrokerIdentity.Impersonate
            // does nothing and the computer account is used. If this is a failover BN, Impersonate will use
            // resource group's network name
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();

                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
                IAsyncResult result = this.BeginProcessMessage(request, null, null);
                return this.EndProcessMessage(result);
            }
        }

        /// <summary>
        /// Async Pattern
        /// Begin method for ProcessMessage
        /// </summary>
        /// <param name="request">request message</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        public IAsyncResult BeginProcessMessage(Message request, AsyncCallback callback, object asyncState)
        {
            //
            // if the connection is not ready (waitone timeout), let following BeginProcessMessage method throw exception.
            //

            BrokerTracing.TraceVerbose("[AzureServiceClient]. BeginProcessMessage: Send message at client {0}.", this);
            return this.ServiceClient.BeginProcessMessage(request, callback, asyncState);
        }

        /// <summary>
        /// Async Pattern
        /// End method for ProcessMessage
        /// </summary>
        /// <param name="ar">async result</param>
        /// <returns>reply message</returns>
        public Message EndProcessMessage(IAsyncResult ar)
        {
            BrokerTracing.TraceVerbose("[AzureServiceClient]. EndProcessMessage: Send message at client {0}.", this);
            return this.ServiceClient.EndProcessMessage(ar);
        }
    }
}
