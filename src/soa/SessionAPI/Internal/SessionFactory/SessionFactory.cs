namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionFactory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;

    public class SessionFactory : AbstractSessionFactory
    {
        public override async Task<SessionBase> CreateSession(SessionStartInfo startInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            SessionBase.CheckSessionStartInfo(startInfo);
            DateTime targetTimeout = DateTime.Now.AddMilliseconds(Constant.DefaultCreateSessionTimeout);
            SessionAllocateInfoContract sessionAllocateInfo = null;
            IResourceProvider resourceProvider = null;
            try
            {
                try
                {
                    resourceProvider = this.BuildResourceProvider(startInfo, binding);
                    // re-calculate the timeout to exclude the timespan for getting credential
                    sessionAllocateInfo = await resourceProvider.AllocateResource(startInfo, durable, SessionBase.GetTimeout(targetTimeout)).ConfigureAwait(false);
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] Successfully allocated resource.", sessionAllocateInfo.Id);

                    if (sessionAllocateInfo.BrokerLauncherEpr != null
                        && sessionAllocateInfo.BrokerLauncherEpr.Count() == 1
                        && sessionAllocateInfo.BrokerLauncherEpr[0] == SessionInternalConstants.BrokerConnectionStringToken)
                    {
                        if (!startInfo.UseAzureQueue.GetValueOrDefault())
                        {
                            SessionBase.TraceSource.TraceEvent(
                                TraceEventType.Error,
                                0,
                                "[Session:{0}] Server side only supports communication via AzureStorageQueue while client doesn't specify UseAzureQueue property.",
                                sessionAllocateInfo.Id);
                            throw new InvalidOperationException("Server side only supports communication via AzureStorageQueue while client doesn't specify UseAzureQueue property.");
                        }
                    }
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:Unknown] Exception occured while allocating resource: {0}", e);
                    if (sessionAllocateInfo != null)
                    {
                        await resourceProvider.FreeResource(startInfo, sessionAllocateInfo.Id).ConfigureAwait(false);
                    }

                    throw;
                }

                IBrokerFactory brokerFactory = BuildBrokerFactory(startInfo, durable);

                try
                {
                    return await brokerFactory.CreateBroker(startInfo, sessionAllocateInfo.Id, targetTimeout, sessionAllocateInfo.BrokerLauncherEpr, binding).ConfigureAwait(false);
                }
                catch
                {
                    // Free resource if failed to create broker or create session
                    await resourceProvider.FreeResource(startInfo, sessionAllocateInfo.Id).ConfigureAwait(false);
                    throw;
                }
                finally
                {
                    if (brokerFactory != null && brokerFactory is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
            finally
            {
                if (resourceProvider != null && resourceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private IResourceProvider BuildResourceProvider(SessionStartInfo startInfo, Binding binding)
        {
            return new GeneralResourceProvider(startInfo, binding);
        }

        public override async Task<SessionBase> AttachSession(SessionAttachInfo attachInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            throw new NotImplementedException();
        }

        public override async Task<SessionBase> AttachBroker(SessionStartInfo startInfo, SessionInfoContract sessionInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            throw new NotImplementedException();
        }
    }
}