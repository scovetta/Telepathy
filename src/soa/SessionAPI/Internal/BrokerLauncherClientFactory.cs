//------------------------------------------------------------------------------
// <copyright file="BrokerLauncherClientFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Factory for broker launcher client
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    /// <summary>
    /// Factory for broker launcher client
    /// </summary>
    public class BrokerLauncherClientFactory : DisposableObject, IBrokerLauncherClientFactoryForHeartbeat
    {
        /// <summary>
        /// Stores the broker launcher client
        /// </summary>
        private IBrokerLauncher brokerLauncher;

        /// <summary>
        /// Stores the broker launcher client for heart beat
        /// </summary>
        private IBrokerLauncher brokerLauncherForHeartbeat;

        /// <summary>
        /// Stores the broker launcher uri
        /// </summary>
        private Uri uri;

        /// <summary>
        /// Stores the heart beat interval
        /// </summary>
        private int heartbeatInterval;

        /// <summary>
        /// Stores the lock when create channel
        /// </summary>
        private object lockCreateChannel = new object();

        /// <summary>
        /// The session info
        /// </summary>
        private SessionInfoBase info;

        /// <summary>
        /// Binding
        /// </summary>
        private Binding binding;


#if DEBUG
        /// <summary>
        /// Stores a flag indicating whether heartbeat is allowed
        /// </summary>
        /// <remarks>
        /// This flag is targted to let session API throw exception
        /// when GetBrokerLauncherClientForHeartbeat() operation is
        /// called in web API mode (which does not support heartbeat).
        /// </remarks>
        private bool allowsHeartbeat;
#endif

        /// <summary>
        /// Initializes a new instance of the BrokerLauncherClientFactory class
        /// </summary>
        /// <param name="info">indicating the session info</param>
        /// <param name="binding">indicating the binding</param>
        public BrokerLauncherClientFactory(SessionInfoBase info, Binding binding)
        {
            this.info = info;
            this.binding = binding;
            Debug.Assert(info is SessionInfo);

#if DEBUG
            this.allowsHeartbeat = true;
#endif
            SessionInfo sessionInfo = (SessionInfo)info;
            if (info.UseInprocessBroker)
            {
                this.brokerLauncher = sessionInfo.InprocessBrokerAdapter;
                this.brokerLauncherForHeartbeat = sessionInfo.InprocessBrokerAdapter;
            }
            else if (sessionInfo.BrokerLauncherEpr == SessionInternalConstants.ConnectionStringToken)
            {
                Trace.TraceInformation($"[{nameof(BrokerLauncherClientFactory)}] will not connect to frontend as EPR is {nameof(SessionInternalConstants.ConnectionStringToken)}.");
            }
            else
            {
                this.uri = new Uri(sessionInfo.BrokerLauncherEpr);
                this.heartbeatInterval = sessionInfo.ClientBrokerHeartbeatInterval;
            }

            //             else
            //             {
            //                 // info is WebSessionInfo
            //                 WebSessionInfo webSessionInfo = (WebSessionInfo)info;
            //                 this.brokerLauncher = new WebBrokerLauncherClient(webSessionInfo.HeadNode, webSessionInfo.Credential);
            // #if DEBUG
            //                 this.allowsHeartbeat = false;
            // #endif
            //             }
        }

        /// <summary>
        /// Gets broker launcher client with timeout specific
        /// </summary>
        /// <param name="timeout">indicating the timeout</param>
        /// <returns>returns broker launcher client</returns>
        public IBrokerLauncher GetBrokerLauncherClient(int timeoutMilliseconds)
        {
            this.PrepareBrokerLauncherClient(ref this.brokerLauncher, timeoutMilliseconds);
            return this.brokerLauncher;
        }

        /// <summary>
        /// Gets broker launcher client for heart beat
        /// </summary>
        /// <returns>returns broker launcher client</returns>
        public IBrokerLauncher GetBrokerLauncherClientForHeartbeat()
        {
#if DEBUG
            if (!this.allowsHeartbeat)
            {
                // This line should not be reached in current code
                // Throw an exception for further notification
                throw new InvalidOperationException("Heartbeat not allowed");
            }
#endif

            this.PrepareBrokerLauncherClient(ref this.brokerLauncherForHeartbeat, this.heartbeatInterval);
            return this.brokerLauncherForHeartbeat;
        }

        /// <summary>
        /// Prepares broker launcher client for heart beat with timeout specific
        /// </summary>
        /// <param name="brokerLauncherRef">indicating the reference to broker launcher</param>
        /// <param name="timeout">indicating the timeout</param>
        private void PrepareBrokerLauncherClient(ref IBrokerLauncher brokerLauncherRef, int timeoutMilliseconds)
        {
            if (brokerLauncherRef == null)
            {
                lock (this.lockCreateChannel)
                {
                    if (brokerLauncherRef == null)
                    {
                        brokerLauncherRef = new BrokerLauncherClient(this.uri, this.info, this.binding);
                    }
                }
            }

            if (brokerLauncherRef is ClientBase<IBrokerLauncher>)
            {
                ClientBase<IBrokerLauncher> broker = (ClientBase<IBrokerLauncher>)brokerLauncherRef;
                if (timeoutMilliseconds == Timeout.Infinite)
                {
                    broker.InnerChannel.OperationTimeout = TimeSpan.MaxValue;
                }
                else
                {
                    broker.InnerChannel.OperationTimeout = SessionBase.GetTimeout(DateTime.Now.AddMilliseconds(timeoutMilliseconds));
                }
            }
        }

        /// <summary>
        /// Close the broker controller client so it can be recreated for next ping
        /// </summary>
        public void CloseBrokerLauncherClientForHeartbeat()
        {
            lock (this.lockCreateChannel)
            {
                if (this.brokerLauncherForHeartbeat != null && this.brokerLauncherForHeartbeat is ICommunicationObject)
                {
                    Utility.SafeCloseCommunicateObject((ICommunicationObject)this.brokerLauncherForHeartbeat);
                    this.brokerLauncherForHeartbeat = null;
                }
            }
        }

        /// <summary>
        /// Dispose the BrokerLauncherClientFactory instance
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.brokerLauncher != null && this.brokerLauncher is ICommunicationObject)
                {
                    Utility.SafeCloseCommunicateObject((ICommunicationObject)this.brokerLauncher);
                    this.brokerLauncher = null;
                }

                if (this.brokerLauncherForHeartbeat != null && this.brokerLauncherForHeartbeat is ICommunicationObject)
                {
                    Utility.SafeCloseCommunicateObject((ICommunicationObject)this.brokerLauncherForHeartbeat);
                    this.brokerLauncherForHeartbeat = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Broker launcher client communicating with SOA web service
        /// </summary>
        /// <remarks>
        /// This client only implemented IBrokerLauncher.Close() operation as
        /// it is the only operation need to SOA web service. And unlike other
        /// proxy, this client communicates directly to the head node, instead
        /// of broker node.
        /// </remarks>
        private class WebBrokerLauncherClient : IBrokerLauncher
        {
            /// <summary>
            /// Stores the head node
            /// </summary>
            private string headNode;

            /// <summary>
            /// Stores the network credential
            /// </summary>
            private NetworkCredential credential;

            /// <summary>
            /// Initializes a new instance of the WebBrokerLauncherClient class
            /// </summary>
            /// <param name="headNode">indicating the head node</param>
            /// <param name="credential">indicating the network credential</param>
            public WebBrokerLauncherClient(string headNode, NetworkCredential credential)
            {
                this.headNode = headNode;
                this.credential = credential;
            }

            /// <summary>
            /// Close session
            /// </summary>
            /// <param name="sessionId">indicating the session id</param>
            void IBrokerLauncher.Close(int sessionId)
            {
                try
                {
                    HttpWebRequest request = SOAWebServiceRequestBuilder.GenerateCloseSessionWebRequest(this.headNode, sessionId, this.credential);
                    using (request.GetResponse()) { }
                }
                catch (WebException e)
                {
                    SessionBase.TraceSource.TraceEvent(System.Diagnostics.TraceEventType.Error, 0, "[WebBrokerLauncherClient] Exception thrown while closing broker: {0}", e);
                    Exception ex = WebAPIUtility.ConvertWebException(e);
                    if (ex is FaultException<SessionFault>)
                    {
                        // Bug 16075: Ignore session fault when closing broker
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            #region Not supported operations
            BrokerInitializationResult IBrokerLauncher.Create(SessionStartInfoContract info, int sessionId)
            {
                throw new NotSupportedException();
            }

            IAsyncResult IBrokerLauncher.BeginCreate(SessionStartInfoContract info, int sessionId, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            Interface.BrokerInitializationResult IBrokerLauncher.EndCreate(IAsyncResult ar)
            {
                throw new NotSupportedException();
            }

            bool IBrokerLauncher.PingBroker(int sessionID)
            {
                throw new NotSupportedException();
            }

            String IBrokerLauncher.PingBroker2(int sessionID)
            {
                throw new NotSupportedException();
            }

            IAsyncResult IBrokerLauncher.BeginPingBroker(int sessionID, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            IAsyncResult IBrokerLauncher.BeginPingBroker2(int sessionID, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            bool IBrokerLauncher.EndPingBroker(IAsyncResult result)
            {
                throw new NotSupportedException();
            }

            String IBrokerLauncher.EndPingBroker2(IAsyncResult result)
            {
                throw new NotSupportedException();
            }

            BrokerInitializationResult IBrokerLauncher.CreateDurable(SessionStartInfoContract info, int sessionId)
            {
                throw new NotSupportedException();
            }

            IAsyncResult IBrokerLauncher.BeginCreateDurable(SessionStartInfoContract info, int sessionId, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            BrokerInitializationResult IBrokerLauncher.EndCreateDurable(IAsyncResult ar)
            {
                throw new NotSupportedException();
            }

            BrokerInitializationResult IBrokerLauncher.Attach(int sessionId)
            {
                throw new NotSupportedException();
            }

            IAsyncResult IBrokerLauncher.BeginAttach(int sessionId, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            BrokerInitializationResult IBrokerLauncher.EndAttach(IAsyncResult result)
            {
                throw new NotSupportedException();
            }

            IAsyncResult IBrokerLauncher.BeginClose(int sessionId, AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            void IBrokerLauncher.EndClose(IAsyncResult result)
            {
                throw new NotSupportedException();
            }

            int[] IBrokerLauncher.GetActiveBrokerIdList()
            {
                throw new NotSupportedException();
            }

            IAsyncResult IBrokerLauncher.BeginGetActiveBrokerIdList(AsyncCallback callback, object state)
            {
                throw new NotSupportedException();
            }

            int[] IBrokerLauncher.EndGetActiveBrokerIdList(IAsyncResult result)
            {
                throw new NotSupportedException();
            }
            #endregion
        }
    }
}
