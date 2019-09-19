// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.ServiceBroker.Common;

    /// <summary>
    /// Service Client
    /// </summary>
    internal class ServiceClient : ClientBase<IService>, IService
    {
        /// <summary>
        /// client id. only use this in trace.
        /// </summary>
        private Guid guid = Guid.NewGuid();

        private string guidCache;
        private string endpointAddressCache;
        private string toStringCache = null;
        private bool stateChanged = true;
        private object stateChangeLock = new object();
        private string stateCache = "Opened";

        public Guid ClientGuid => guid;

        /// <summary>
        /// Create a new instance of the ServiceClient class.
        /// </summary>
        /// <param name="binding">binding information</param>
        /// <param name="remoteAddress">remote address</param>
        public ServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
            BrokerTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "[ServiceClient] In constructor, client id {0}, IsHA = {1}",
                this.guid,
                BrokerIdentity.IsHAMode);
        }

        public async Task InitAsync()
        {
            this.guidCache = this.guid.ToString();

#if DEBUG
            DateTime start = DateTime.Now;
            Debug.WriteLine($"[{nameof(ServiceClient)}] .{nameof(this.InitAsync)} (perf) Start initialization of client {this.guidCache}");
#endif

            if (BrokerIdentity.IsHAMode)
            {
                // Bug 10301 : Explicitly open channel when impersonating the resource group's account if running on failover cluster so identity flows correctly when
                //      calling HpcServiceHost.
                //  NOTE: The patch we got from the WCF team (KB981001) only works when the caller is on a threadpool thread. 
                //  NOTE: Channel must be opened before setting OperationTimeout
                using (BrokerIdentity identity = new BrokerIdentity())
                {
                    identity.Impersonate();
                    this.Open();
                }
            }
            else
            {
                var commu = (ICommunicationObject)this;
                await Task.Factory.FromAsync(
                        commu.BeginOpen,
                        result =>
                            {
#if DEBUG
                                Debug.WriteLine($"[{nameof(ServiceClient)}] .{nameof(this.InitAsync)} (perf) Start EndOpen of client {this.guidCache}. Millisecond used:{(DateTime.Now - start).TotalMilliseconds}");
#endif
                                commu.EndOpen(result);
                            },
                        null)
                    .ConfigureAwait(false);
            }

            this.RegisterStateChangedNotification();
            this.endpointAddressCache = this.Endpoint.Address.ToString();
#if DEBUG
            Debug.WriteLine($"[{nameof(ServiceClient)}] .{nameof(this.InitAsync)} (perf) Ended initialization of client {this.guidCache} to {this.endpointAddressCache}. Millisecond used:{(DateTime.Now - start).TotalMilliseconds}");
#endif
        }

        private void RegisterStateChangedNotification()
        {
            var commu = (ICommunicationObject)this;
            commu.Closed += (sender, args) => this.NotifyStateChanged(CommunicationState.Closed);
            commu.Closing += (sender, args) => this.NotifyStateChanged(CommunicationState.Closing);
            commu.Faulted += (sender, args) => this.NotifyStateChanged(CommunicationState.Faulted);
            commu.Opened += (sender, args) => this.NotifyStateChanged(CommunicationState.Opened);
            commu.Opening += (sender, args) => this.NotifyStateChanged(CommunicationState.Opening);
        }

        private void NotifyStateChanged(CommunicationState newState)
        {
            lock (this.stateChangeLock)
            {
                this.stateChanged = true;
                this.stateCache = newState.ToString();
                Debug.WriteLine($"[{nameof(ServiceClient)}] .{nameof(this.NotifyStateChanged)} state changed to {newState.ToString()}");
            }
        }

        /// <summary>
        /// Standard operation contract for request/reply
        /// Any instance members of ClientBase(TChannel) are not guaranteed to be thread safe.
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        public System.ServiceModel.Channels.Message ProcessMessage(System.ServiceModel.Channels.Message request)
        {
            // Impersonate the broker's identity. If this is a non-failover BN, BrokerIdentity.Impersonate
            // does nothing and the computer account is used. If this is a failover BN, Impersonate will use
            // resource group's network name
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();

                // Call async version and block on completion in order to workaround System.Net.Socket bug #750028 
                IAsyncResult result = Channel.BeginProcessMessage(request, null, null);
                return Channel.EndProcessMessage(result);
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
        public IAsyncResult BeginProcessMessage(System.ServiceModel.Channels.Message request, AsyncCallback callback, object asyncState)
        {
            // Impersonate the broker's identity. If this is a non-failover BN, BrokerIdentity.Impersonate
            // does nothing and the computer account is used. If this is a failover BN, Impersonate will use
            // resource group's network name
            using (BrokerIdentity identity = new BrokerIdentity())
            {
                identity.Impersonate();
                return Channel.BeginProcessMessage(request, callback, asyncState);
            }
        }

        /// <summary>
        /// Async Pattern
        /// End method for ProcessMessage
        /// </summary>
        /// <param name="ar">async result</param>
        /// <returns>reply message</returns>
        public Message EndProcessMessage(IAsyncResult ar)
        {
            return Channel.EndProcessMessage(ar);
        }

        /// <summary>
        /// Start the client to process requests.
        /// </summary>
        public void Start(GetNextRequestState state, int serviceOperationTimeout)
        {
            if (serviceOperationTimeout > 0)
            {
                this.InnerChannel.OperationTimeout = TimeSpan.FromMilliseconds(serviceOperationTimeout);
            }

            if (state != null)
            {
                state.Invoke();
            }
        }

        /// <summary>
        /// Close the instance, primarily the underlying communictaion object.
        /// </summary>
        public void AsyncClose()
        {
            BrokerTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "[ServiceClient].AsyncClose: Will close the client, client id {0}",
                this.guid);

            Utility.AsyncCloseICommunicationObject(this);
        }

        /// <summary>
        /// Include client state and id in the return value.
        /// </summary>
        public override string ToString()
        {
            if (Volatile.Read(ref this.stateChanged))
            {
                lock (this.stateChangeLock)
                {
                    if (this.stateChanged)
                    {
                        Debug.WriteLine($"[{nameof(ServiceClient)}].{nameof(this.ToString)}: detected state change. new state: {this.stateCache}.");
                        this.toStringCache = $"Service Client ({this.stateCache}) {this.guidCache}, {this.endpointAddressCache}";
                        this.stateChanged = false;
                    }
                }
            }

            Debug.Assert(!string.IsNullOrEmpty(this.toStringCache));
            return this.toStringCache;
        }
    }
}
