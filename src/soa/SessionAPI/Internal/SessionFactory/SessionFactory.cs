// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionFactory
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Authentication;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Data;

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
                        if (!startInfo.UseAzureStorage)
                        {
                            SessionBase.TraceSource.TraceEvent(
                                TraceEventType.Error,
                                0,
                                "[Session:{0}] Server side only supports communication via AzureStorageQueue while client doesn't specify UseAzureQueue property and not using AzureStorage scheme.",
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

        private IResourceProvider BuildResourceProvider(SessionAttachInfo attachInfo, Binding binding)
        {
            return new GeneralResourceProvider(attachInfo, binding);
        }

        public override async Task<SessionBase> AttachSession(SessionAttachInfo attachInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            Debug.Assert(attachInfo is SessionAttachInfo, "[OnPremiseSessionFactory].AttachSession: attachInfo's type must be SessionAttachInfo.");
            DateTime targetTimeout;

            Utility.ThrowIfNull(attachInfo, "attachInfo");
            Utility.ThrowIfInvalidTimeout(timeoutMilliseconds, "timeoutMilliseconds");

            if (timeoutMilliseconds == Timeout.Infinite)
            {
                targetTimeout = DateTime.MaxValue;
            }
            else
            {
                targetTimeout = DateTime.Now.AddMilliseconds(timeoutMilliseconds);
            }

            SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0, "[Session:{0}] Start to attach session...", attachInfo.SessionId);

            IResourceProvider resourceProvider = null;
            SessionInfo info = null;
            CredType typeOfExpectedCred = CredType.None;
            try
            {
                int retry = 0;
                bool askForCredential = false;

                // allow users to try credential at most MaxRetryCount times
                int askForCredentialTimes = 0;
                while (Utility.CanRetry(retry, askForCredential, askForCredentialTimes))
                {
                    retry++;

                    try
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] Start to attach session.", attachInfo.SessionId);
                        Stopwatch watch = new Stopwatch();
                        watch.Start();

                        

                        resourceProvider = this.BuildResourceProvider(attachInfo, binding);
                        watch.Stop();

                        // re-calculate the timeout to exclude the timespan for getting credential
                        try
                        {
                            targetTimeout = targetTimeout.AddMilliseconds(watch.ElapsedMilliseconds);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                        }

                        info = await resourceProvider.GetResourceInfo(attachInfo, SessionBase.GetTimeout(targetTimeout)).ConfigureAwait(false);
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] Successfully got resource info. BrokerLauncherEpr = {1}", attachInfo.SessionId, info.BrokerLauncherEpr);

                        // If the session is an inprocess broker session, info.UseInprocessBroker will be set
                        // to true by HpcSession service. Need to set attachInfo.UseInprocessBroker to true
                        // in order to build correct broker factory instance.
                        if (info.UseInprocessBroker)
                        {
                            attachInfo.UseInprocessBroker = true;
                        }

                        // If debug mode is enabled, need to set info.UseInprocessBroker to true in order to
                        // build correct broker factory instance.
                        if (attachInfo.DebugModeEnabled)
                        {
                            info.UseInprocessBroker = true;
                        }

                        if (SoaHelper.IsSchedulerOnIaaS(attachInfo.Headnode))
                        {
                            string suffix = SoaHelper.GetSuffixFromHeadNodeEpr(attachInfo.Headnode);
                            info.BrokerLauncherEpr = SoaHelper.UpdateEprWithCloudServiceName(info.BrokerLauncherEpr, suffix);
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
                        break;

                    }
                    catch (SessionException)
                    {
                        throw;
                    }
                    catch (EndpointNotFoundException)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}] EndpointNotFoundException occured while getting resource info.", attachInfo.SessionId);
                        SessionBase.HandleEndpointNotFoundException(attachInfo.Headnode);
                    }
                    catch (AuthenticationException e)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:{0}] AuthenticationException occured while attaching session: {1}", attachInfo.SessionId, e);

                        throw;
                    }
                    catch (MessageSecurityException e)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:{0}] MessageSecurityException occured while attaching session: {1}", attachInfo.SessionId, e);

                        attachInfo.ClearCredential();
                        SessionBase.PurgeCredential(attachInfo);
                        throw;
                    }
                    catch (FaultException<SessionFault> ex)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:{0}] Fault exception occured while allocating resource. FaultCode = {1}", attachInfo.SessionId, ex.Detail.Code);

                        if (typeOfExpectedCred == CredType.None)
                        {
                            throw Utility.TranslateFaultException(ex);
                        }
                        else
                        {
                            attachInfo.ClearCredential();

                            if (resourceProvider is IDisposable)
                            {
                                ((IDisposable)resourceProvider).Dispose();
                            }
                            continue;
                        }
                    }
                    catch (CommunicationException e)
                    {
                        throw new SessionException(SOAFaultCode.ConnectSessionLauncherFailure, SR.ConnectSessionLauncherFailure, e);
                    }
                    catch (TimeoutException e)
                    {
                        throw new TimeoutException(string.Format(SR.ConnectSessionLauncherTimeout, Constant.DefaultCreateSessionTimeout), e);
                    }
                    catch (Exception e)
                    {
                        throw new SessionException(SOAFaultCode.UnknownError, e.ToString());
                    }
                } // while
            }
            finally
            {
                IDisposable disposableObject = resourceProvider as IDisposable;
                if (disposableObject != null)
                {
                    disposableObject.Dispose();
                }
            }

            if (string.IsNullOrEmpty(info.BrokerLauncherEpr) && !info.UseInprocessBroker)
            {
                if ((info.JobState &
                    (JobState.Configuring
                      | JobState.ExternalValidation
                      | JobState.Queued
                      | JobState.Running
                      | JobState.Submitted
                      | JobState.Validating)) != 0)
                {
                    throw new SessionException(string.Format(SR.AttachConfiguringSession, attachInfo.SessionId));
                }
                else
                {
                    throw new SessionException(string.Format(SR.AttachNoBrokerSession, attachInfo.SessionId));
                }
            }

            IBrokerFactory brokerFactory = BuildBrokerFactory(attachInfo, durable);
            try
            {
                return await brokerFactory.AttachBroker(attachInfo, info, SessionBase.GetTimeout(targetTimeout), binding).ConfigureAwait(false);
            }
            finally
            {
                IDisposable disposableObject = brokerFactory as IDisposable;
                if (disposableObject != null)
                {
                    disposableObject.Dispose();
                }
            }
        }

        public override async Task<SessionBase> AttachBroker(SessionStartInfo startInfo, SessionInfoContract sessionInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            throw new NotImplementedException();
        }
    }
}