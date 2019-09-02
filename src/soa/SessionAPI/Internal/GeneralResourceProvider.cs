using TelepathyCommon;

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    public class GeneralResourceProvider : IResourceProvider, IDisposable
    {
        private string sessionNode;

        private string endpointPrefix;

        private SessionLauncherClient client;

        private Binding binding;

        // TODO: consolidate ctors
        public GeneralResourceProvider(SessionStartInfo info, Binding binding)
        {
            this.sessionNode = info.Headnode;
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
            else if ((info.TransportScheme & TransportScheme.AzureStorage) == TransportScheme.AzureStorage)
            {
                this.endpointPrefix = "az.table";
            }

            this.client = new SessionLauncherClient(info, binding);
        }

        public GeneralResourceProvider(SessionAttachInfo info, Binding binding)
        {
            this.sessionNode = info.Headnode;
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
            else if ((info.TransportScheme & TransportScheme.AzureStorage) == TransportScheme.AzureStorage)
            {
                this.endpointPrefix = "az.table";
            }

            this.client = new SessionLauncherClient(info, binding);
        }

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
                        return Task.CompletedTask;
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
                        return Task.CompletedTask;
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

        public async Task<SessionInfo> GetResourceInfo(SessionAttachInfo attachInfo, TimeSpan timeout)
        {
            this.client.InnerChannel.OperationTimeout = timeout;
            RetryManager retry = SoaHelper.GetDefaultExponentialRetryManager();

            SessionInfo info;
            DateTime startTime = DateTime.Now;
            if (attachInfo.TransportScheme == TransportScheme.Http)
            {
                info = Utility.BuildSessionInfoFromDataContract(
                    await RetryHelper<SessionInfoContract>.InvokeOperationAsync(
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

                                    return Task.CompletedTask;
                                },
                            retry)
                        .ConfigureAwait(false));
            }
            else
            {
                info = Utility.BuildSessionInfoFromDataContract(
                    await RetryHelper<SessionInfoContract>.InvokeOperationAsync(
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

                                    return Task.CompletedTask;
                                },
                            retry)
                        .ConfigureAwait(false));
            }

            return info;
        }

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
                                return Task.CompletedTask;
                            },
                        retry).ConfigureAwait(false);
                }
            }
            catch
            {
                // if terminate the session failed, then do nothing here.
            }
        }

        public void Dispose()
        {
            if (this.client != null)
            {
                Utility.SafeCloseCommunicateObject(this.client);
                this.client = null;
            }
        }
    }
}
