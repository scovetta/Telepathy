// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;
    using Microsoft.Telepathy.Session.QueueAdapter.Client.Impls;

    /// <summary>
    /// Broker factory for v3
    /// </summary>
    public class V3BrokerFactory : IBrokerFactory
    {
        /// <summary>
        /// Stores the durable flag
        /// </summary>
        private bool durable;

        /// <summary>
        /// Initializes a new instance of the V3BrokerFactory class
        /// </summary>
        /// <param name="durable">indicating the durable flag</param>
        public V3BrokerFactory(bool durable)
        {
            this.durable = durable;
        }

        /// <summary>
        /// Create broker
        /// </summary>
        /// <param name="startInfo">indicating the session start information</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="targetTimeout">indicating the target timeout</param>
        /// <param name="eprs">indicating the broker epr list</param>
        /// <param name="epr">output selected epr</param>
        /// <param name="binding">indicting the binding</param>
        /// <returns>returns the session information</returns>
        public async Task<SessionBase> CreateBroker(SessionStartInfo startInfo, string sessionId, DateTime targetTimeout, string[] eprs, Binding binding)
        {
            Exception innerException = null;
            IEnumerable<string> endpoints = eprs;
            if (startInfo.UseAzureQueue.GetValueOrDefault() && !endpoints.Contains(SessionInternalConstants.BrokerConnectionStringToken))
            {
                endpoints = endpoints.Concat(new[] { SessionInternalConstants.BrokerConnectionStringToken });
            }

            foreach (string epr in endpoints)
            {
                TimeSpan timeout = SessionBase.GetTimeout(targetTimeout);
                IBrokerLauncher brokerLauncher = null;
                try
                {
                    SessionBase.TraceSource.TraceInformation("[Session:{0}] Try to create broker... BrokerLauncherEpr = {1}", sessionId, epr);

                    void RenewBrokerLauncherClient()
                    {
                        if (epr == SessionInternalConstants.BrokerConnectionStringToken)
                        {
                            brokerLauncher = new BrokerLauncherCloudQueueClient(startInfo.BrokerLauncherStorageConnectionString);
                        }
                        else
                        {
                            var client = new BrokerLauncherClient(new Uri(epr), startInfo, binding);
                            client.InnerChannel.OperationTimeout = timeout;
                            brokerLauncher = client;
                        }
                    }

                    RenewBrokerLauncherClient();

                    BrokerInitializationResult result = null;

                    int retry = 20;
                    while (retry > 0)
                    {
                        try
                        {
                            if (this.durable)
                            {
                                result = brokerLauncher.CreateDurable(startInfo.Data, sessionId);
                            }
                            else
                            {
                                result = brokerLauncher.Create(startInfo.Data, sessionId);
                            }

                            break;
                        }
                        catch (Exception ex)
                        {
                            if (retry <= 0)
                            {
                                throw;
                            }

                            retry--;
                            Debug.WriteLine($"Waiting for Broker Launcher running. Detail: {ex.Message}");
                            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                            RenewBrokerLauncherClient();
                        }
                    }

                    Debug.Assert(result != null);

                    SessionBase.TraceSource.TraceInformation("[Session:{0}] Succesfully created broker.", sessionId);
                    SessionInfo info = SessionBase.BuildSessionInfo(result, this.durable, sessionId, epr, startInfo.Data.ServiceVersion, startInfo);

                    if (this.durable)
                    {
#if net40
                        return new DurableSession(info, startInfo.Headnode, binding);
#else
                        return new DurableSession(info, startInfo.Headnode, binding);
#endif
                    }
                    else
                    {

                        var session = new V3Session(info, startInfo.Headnode, startInfo.ShareSession, binding);
                        if (startInfo.UseAzureStorage)
                        {
                            session.BrokerLauncherClient = brokerLauncher;
                        }

                        return session;
                    }

                }
                catch (FaultException<SessionFault> e)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:{0}] Fault exception occured while creating broker: {1}. FaultCode = {2}", sessionId, e, e.Detail.Code);
                    switch (e.Detail.Code)
                    {
                        // Continue if current broker node is being taken offline
                        case SOAFaultCode.Broker_BrokerIsOffline:
                            continue;
                    }

                    throw Utility.TranslateFaultException(e);
                }
                catch (TimeoutException te)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}] TimeoutException occured while creating broker: {1}", sessionId, te);

                    // don't continue when we timeout
                    throw new TimeoutException(string.Format(SR.ConectBrokerLauncherTimeout, epr, Constant.DefaultCreateSessionTimeout), te);
                }
                catch (CommunicationException ex)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:{0}] Failed to create broker: {1}", sessionId, ex);
                    innerException = ex;
                    SessionBase.TraceSource.TraceInformation(ex.ToString());
                    continue;
                }
                finally
                {
                    var client = brokerLauncher as BrokerLauncherClient;
                    if (client != null)
                    {
                        Utility.SafeCloseCommunicateObject(client);
                    }
                }
            }

            SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}] Failed to create broker after trying all available eprs.", sessionId);
            throw new SessionException(SR.NoBrokerNodeFound, innerException);
        }

        /// <summary>
        /// Attach to a broker, returns session instance
        /// </summary>
        /// <param name="attachInfo">indicating the attach information</param>
        /// <param name="info">indicating the session info to be updated</param>
        /// <param name="timeout">indicating the timeout</param>
        /// <param name="binding">indicting the binding</param>
        /// <returns>returns the session instance</returns>
        public Task<SessionBase> AttachBroker(SessionAttachInfo attachInfo, SessionInfo info, TimeSpan timeout, Binding binding)
        {
            SessionBase.TraceSource.TraceInformation("[Session:{0}] Try to attach broker...", attachInfo.SessionId);
            BrokerLauncherClient broker = new BrokerLauncherClient(new Uri(info.BrokerLauncherEpr), attachInfo, binding);
            broker.InnerChannel.OperationTimeout = timeout;
            try
            {
                BrokerInitializationResult result = broker.Attach(info.Id);
                info.BrokerEpr = result.BrokerEpr;
                info.ControllerEpr = result.ControllerEpr;
                info.ResponseEpr = result.ResponseEpr;
                info.ServiceOperationTimeout = result.ServiceOperationTimeout;
                info.MaxMessageSize = result.MaxMessageSize;
                info.ClientBrokerHeartbeatInterval = result.ClientBrokerHeartbeatInterval;
                info.ClientBrokerHeartbeatRetryCount = result.ClientBrokerHeartbeatRetryCount;
                info.BrokerUniqueId = result.BrokerUniqueId;

                info.UseAzureQueue = result.UseAzureQueue;
                info.AzureRequestQueueUris = result.AzureRequestQueueUris;
                info.AzureRequestBlobUri = result.AzureRequestBlobUri;

                info.Username = attachInfo.Username;
                info.InternalPassword = attachInfo.InternalPassword;
                info.Headnode = attachInfo.Headnode;

                info.UseWindowsClientCredential = attachInfo.UseWindowsClientCredential;
            }
            catch (FaultException<SessionFault> e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}] Fault exception occured while attaching broker: {1}. FaultCode = {2}", attachInfo.SessionId, e, e.Detail.Code);
                throw Utility.TranslateFaultException(e);
            }
            catch (CommunicationException e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}] Failed to attach to broker: {1}", attachInfo.SessionId, e);
                throw new SessionException(SOAFaultCode.ConnectBrokerLauncherFailure, SR.ConnectBrokerLauncherFailure, e);
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}] Failed to attach to broker: {1}", attachInfo.SessionId, e);
                throw new SessionException(SOAFaultCode.UnknownError, e.ToString());
            }
            finally
            {
                Utility.SafeCloseCommunicateObject(broker);
            }

            if (SoaHelper.IsSchedulerOnIaaS(attachInfo.Headnode))
            {
                string suffix = SoaHelper.GetSuffixFromHeadNodeEpr(attachInfo.Headnode);
                if (info.BrokerEpr != null)
                {
                    SoaHelper.UpdateEprWithCloudServiceName(info.BrokerEpr, suffix);
                }

                if (info.ControllerEpr != null)
                {
                    SoaHelper.UpdateEprWithCloudServiceName(info.ControllerEpr, suffix);
                }

                if (info.ResponseEpr != null)
                {
                    SoaHelper.UpdateEprWithCloudServiceName(info.ResponseEpr, suffix);
                }
            }

            if (this.durable)
            {
                if (!info.Durable)
                {
                    throw new SessionException(SOAFaultCode.InvalidAttachInteractiveSession, SR.InvalidAttachInteractiveSession);
                }
#if net40
                return TaskEx.FromResult<SessionBase>(new DurableSession(info, attachInfo.Headnode, binding));
#else
                return Task.FromResult<SessionBase>(new DurableSession(info, attachInfo.Headnode, binding));
#endif
            }
            else
            {
                if (info.Durable)
                {
                    throw new SessionException(SOAFaultCode.InvalidAttachDurableSession, SR.InvalidAttachDurableSession);
                }
#if net40
                return TaskEx.FromResult<SessionBase>(new V3Session(info, attachInfo.Headnode, true, binding));
#else
                return Task.FromResult<SessionBase>(new V3Session(info, attachInfo.Headnode, true, binding));
#endif
            }
        }
    }
}
