//------------------------------------------------------------------------------
// <copyright file="ServiceJobProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Service job resouce provide, provide service job as the resource for the session
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.Scheduler.Session.Common;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

#if !net40
    using Microsoft.Hpc.AADAuthUtil;
#endif

    /// <summary>
    /// Service job resouce provide, provide service job as the resource for the session
    /// </summary>
    internal class ServiceJobProvider : DisposableObject, IResourceProvider
    {
        /// <summary>
        /// Stores the head node
        /// </summary>
        private string headNode;

        /// <summary>
        /// Stores the endpoint prefix
        /// </summary>
        private string endpointPrefix;

        /// <summary>
        /// Stores the client proxy to session launcher
        /// </summary>
        private SessionLauncherClient client;

        private Binding binding;

#if net40
        private static readonly Task CompletedTask = TaskEx.FromResult(0);
#else
        private static readonly Task CompletedTask = Task.FromResult(0);
#endif


        /// <summary>
        /// Initializes a new instance of the ServiceJobProvider class
        /// </summary>
        /// <param name="headNode">indicating the head node</param>
        /// <param name="endpointPrefix">indicating the endpoint prefix</param>
        /// <param name="binding">indicting the binding</param>
        //public ServiceJobProvider(string headNode, string endpointPrefix, Binding binding)
        //{
        //    this.headNode = headNode;
        //    this.endpointPrefix = endpointPrefix;
        //    this.binding = binding;
        //    this.client = new SessionLauncherClient(Utility.GetSessionLauncher(headNode, endpointPrefix), binding);
        //}

        /// <summary>
        /// Initializes a new instance of the ServiceJobProvider class
        /// </summary>
        /// <param name="info">The session start info</param>
        /// <param name="binding">indicting the binding</param>
        public ServiceJobProvider(SessionStartInfo info, Binding binding)
        {
            this.headNode = info.Headnode;
            this.binding = binding;

            if ((info.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                this.endpointPrefix = SessionLauncherClient.EndpointPrefix;
            }
            else if ((info.TransportScheme & TransportScheme.Http) == TransportScheme.Http || (info.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                this.endpointPrefix = SessionLauncherClient.HttpsEndpointPrefix;
            }
            else if ((info.TransportScheme & TransportScheme.Custom) == TransportScheme.Custom)
            {
                this.endpointPrefix = SessionLauncherClient.EndpointPrefix;
            }

            this.client = new SessionLauncherClient(info, binding);
#if !net40
            if (info.UseAad)
            {
                this.client.Endpoint.Behaviors.UseAadClientBehaviors(info).GetAwaiter().GetResult();
            }
#endif
        }

        /// <summary>
        /// Initializes a new instance of the ServiceJobProvider class
        /// </summary>
        /// <param name="startInfo">The session attach info</param>
        /// <param name="binding">indicting the binding</param>
        public ServiceJobProvider(SessionAttachInfo info, Binding binding)
        {
            this.binding = binding;
            this.headNode = info.Headnode;
            if ((info.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                this.endpointPrefix = SessionLauncherClient.EndpointPrefix;
            }
            else if ((info.TransportScheme & TransportScheme.Http) == TransportScheme.Http || (info.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                this.endpointPrefix = SessionLauncherClient.HttpsEndpointPrefix;
            }
            else if ((info.TransportScheme & TransportScheme.Custom) == TransportScheme.Custom)
            {
                this.endpointPrefix = SessionLauncherClient.EndpointPrefix;
            }

            this.client = new SessionLauncherClient(info, binding);
#if !net40
            if (info.UseAad)
            {
                this.client.Endpoint.Behaviors.UseAadClientBehaviors(info).GetAwaiter().GetResult();
            }
#endif
        }

        /// <summary>
        /// Allocate resource for service job and provide broker epr
        /// </summary>
        /// <param name="startInfo">indicating session start information</param>
        /// <param name="durable">indicating whether it is a durable session</param>
        /// <param name="timeout">indicating the timeout</param>
        /// <param name="eprs">output string array of available broker epr list</param>
        /// <param name="sessionInfo">the session info</param>
        /// <returns>returns unique session id</returns>
        public async Task<SessionAllocateInfoContract> AllocateResource(SessionStartInfo startInfo, bool durable, TimeSpan timeout)
        {
            SessionAllocateInfoContract sessionAllocateInfo = new SessionAllocateInfoContract();
            this.client.InnerChannel.OperationTimeout = timeout;

            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:Unknown] Allocating resource... IsDurable = {0}, is LocalUser = {1}", durable, startInfo.LocalUser);
            DateTime startTime = DateTime.Now;

            if (durable)
            {
                sessionAllocateInfo = await RetryHelper<SessionAllocateInfoContract>.InvokeOperationAsync(
                    async () => await this.client.AllocateDurableV5Async(startInfo.Data, this.endpointPrefix).ConfigureAwait(false),
                    (e, r) =>
                    {
                        var remainingTime = GetRemainingTime(timeout, startTime);
                        if ((e is EndpointNotFoundException || (e is CommunicationException && !(e is FaultException<SessionFault>))) && remainingTime > TimeSpan.Zero)
                        {
                            Utility.SafeCloseCommunicateObject(this.client);
                            this.client = new SessionLauncherClient(startInfo, this.binding);
                            this.client.InnerChannel.OperationTimeout = remainingTime;
                        }
                        else
                        {
                            r.MaxRetryCount = 0;
                        }
                        return CompletedTask;
                    },
                    retry).ConfigureAwait(false);
            }
            else
            {
                sessionAllocateInfo = await RetryHelper<SessionAllocateInfoContract>.InvokeOperationAsync(
                    async () => await this.client.AllocateV5Async(startInfo.Data, this.endpointPrefix).ConfigureAwait(false),
                    (e, r) =>
                    {
                        var remainingTime = GetRemainingTime(timeout, startTime);
                        if ((e is EndpointNotFoundException || (e is CommunicationException && !(e is FaultException<SessionFault>))) && remainingTime > TimeSpan.Zero)
                        {
                            Utility.SafeCloseCommunicateObject(this.client);
                            this.client = new SessionLauncherClient(startInfo, this.binding);
                            this.client.InnerChannel.OperationTimeout = remainingTime;
                        }
                        else
                        {
                            r.MaxRetryCount = 0;
                        }
                        return CompletedTask;
                    },
                    retry).ConfigureAwait(false);
            }

            SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] Successfully allocated resource. Eprs = {1}", sessionAllocateInfo.Id, sessionAllocateInfo.BrokerLauncherEpr == null ? string.Empty : string.Join(",", sessionAllocateInfo.BrokerLauncherEpr));

            if (sessionAllocateInfo.ServiceVersion != null)
            {
                try
                {
                    startInfo.Data.ServiceVersion = sessionAllocateInfo.ServiceVersion;
                }
                catch
                {
                    throw new SessionException(SR.InvalidServiceVersionReturned);
                }
            }

            if (startInfo.UseSessionPool)
            {
                return sessionAllocateInfo;
            }
            else
            {
                if (!startInfo.UseInprocessBroker && (sessionAllocateInfo.BrokerLauncherEpr == null || sessionAllocateInfo.BrokerLauncherEpr.Length == 0))
                {
                    throw new SessionException(SR.NoBrokerNodeFound);
                }

                return sessionAllocateInfo;
            }
        }

        /// <summary>
        /// Get resource information
        /// </summary>
        /// <param name="attachInto">indicating session attach information</param>
        /// <param name="timeout">indicating the timeout</param>
        /// <returns>returns session information</returns>
        public async Task<SessionInfo> GetResourceInfo(SessionAttachInfo attachInfo, TimeSpan timeout)
        {
            this.client.InnerChannel.OperationTimeout = timeout;
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            SessionInfo info;
            DateTime startTime = DateTime.Now;
            if (attachInfo.TransportScheme == TransportScheme.Http)
            {
                info = Utility.BuildSessionInfoFromDataContract(await RetryHelper<SessionInfoContract>.InvokeOperationAsync(
                    async () => await this.client.GetInfoV5Sp1Async(SessionLauncherClient.HttpsEndpointPrefix, attachInfo.SessionId, attachInfo.UseAad).ConfigureAwait(false),
                    (e, r) =>
                    {
                        var remainingTime = GetRemainingTime(timeout, startTime);
                        if ((e is EndpointNotFoundException || (e is CommunicationException && !(e is FaultException<SessionFault>))) && remainingTime > TimeSpan.Zero)
                        {
                            Utility.SafeCloseCommunicateObject(this.client);
                            this.client = new SessionLauncherClient(attachInfo, this.binding);
                            this.client.InnerChannel.OperationTimeout = remainingTime;
                        }
                        else
                        {
                            r.MaxRetryCount = 0;
                        }
                        return CompletedTask;
                    },
                    retry).ConfigureAwait(false));
            }
            else
            {
                info = Utility.BuildSessionInfoFromDataContract(await RetryHelper<SessionInfoContract>.InvokeOperationAsync(
                    async () => await this.client.GetInfoV5Sp1Async(SessionLauncherClient.EndpointPrefix, attachInfo.SessionId, attachInfo.UseAad).ConfigureAwait(false),
                    (e, r) =>
                    {
                        var remainingTime = GetRemainingTime(timeout, startTime);

                        if ((e is EndpointNotFoundException || (e is CommunicationException && !(e is FaultException<SessionFault>))) && remainingTime > TimeSpan.Zero)
                        {
                            Utility.SafeCloseCommunicateObject(this.client);
                            this.client = new SessionLauncherClient(attachInfo, this.binding);
                            this.client.InnerChannel.OperationTimeout = remainingTime;
                        }
                        else
                        {
                            r.MaxRetryCount = 0;
                        }
                        return CompletedTask;
                    },
                    retry).ConfigureAwait(false));
            }
            return info;
        }

        private static TimeSpan GetRemainingTime(TimeSpan timeout, DateTime startTime)
        {
            TimeSpan remainingTime;
            if (timeout == TimeSpan.MaxValue)
            {
                remainingTime = timeout;
            }
            else
            {
                remainingTime = timeout - (DateTime.Now - startTime);
            }

            return remainingTime;
        }

        /// <summary>
        /// Free resource by cancel service job
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        public async Task FreeResource(SessionStartInfo startInfo, int sessionId)
        {
            try
            {
                if (sessionId != 0)
                {
                    RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

                    await RetryHelper<object>.InvokeOperationAsync(
                        async () =>
                        {
                            await this.client.TerminateV5Async(sessionId).ConfigureAwait(false);
                            return null;
                        },
                        (e, r) =>
                        {
                            if (e is EndpointNotFoundException)
                            {
                                Utility.SafeCloseCommunicateObject(this.client);
                                this.client = new SessionLauncherClient(startInfo, this.binding);
                            }
                            else
                            {
                                r.MaxRetryCount = 0;
                            }
                            return CompletedTask;
                        },
                        retry).ConfigureAwait(false);
                }
            }
            catch
            {
                // if terminate the session failed, then do nothing here.
            }
        }

        /// <summary>
        /// Dispose the ServiceJobProvider instance
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        [method: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Target = "ServiceJobProvider.client", Justification = "It's closed in Utility.SafeCloseCommunicateObject().")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.client != null)
                {
                    Utility.SafeCloseCommunicateObject(this.client);
                    this.client = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
