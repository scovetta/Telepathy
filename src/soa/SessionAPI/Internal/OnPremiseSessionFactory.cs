//------------------------------------------------------------------------------
// <copyright file="OnPremiseSessionFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Session factory for on-premise cluster
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Data;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Authentication;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Threading.Tasks;
    /// <summary>
    /// Session factory for on-premise cluster
    /// </summary>
    internal class OnPremiseSessionFactory : SessionFactory
    {
        /// <summary>
        /// Create session
        /// </summary>
        /// <param name="startInfo">indicating session start information</param>
        /// <param name="durable">indicating a value whether a durable session is to be created</param>
        /// <param name="timeoutMilliseconds">indicating the timeout</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>returns session instance</returns>
        public override async Task<SessionBase> CreateSession(SessionStartInfo startInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            SessionBase.CheckSessionStartInfo(startInfo);
            DateTime targetTimeout = DateTime.Now.AddMilliseconds(Constant.DefaultCreateSessionTimeout);

            // if following env variable is set, we will launch the service host in an admin job.
            string adminJobEnv = Environment.GetEnvironmentVariable(SessionBase.EnvVarNameForAdminJob, EnvironmentVariableTarget.Process);
            if (!string.IsNullOrEmpty(adminJobEnv) && adminJobEnv.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                startInfo.AdminJobForHostInDiag = true;
            }

            SessionAllocateInfoContract sessionAllocateInfo = null;

            CredType typeOfExpectedCred = CredType.None;
            IResourceProvider resourceProvider = null;
            IList<string> newDataClients = null;

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
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:Unknown] Start to create session for on-premise cluster. IsDurable = {0}, RetryCount = {1}, AskForCredentialTimes = {2}, TimeCost = {3}", durable, retry, askForCredentialTimes, targetTimeout);

                        Stopwatch watch = new Stopwatch();
                        watch.Start();

                        if (startInfo.UseAad)
                        {
                            // Authentication handled by AADUtil
                        }
                        else if (SoaHelper.IsSchedulerOnAzure(startInfo.Headnode) || SoaHelper.IsSchedulerOnIaaS(startInfo.Headnode) || (startInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http || (startInfo.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
                        {
                            askForCredential = SessionBase.RetrieveCredentialOnAzure(startInfo);

                            if (askForCredential)
                            {
                                askForCredentialTimes++;
                            }
                        }
                        else
                        {
                            askForCredential = await SessionBase.RetrieveCredentialOnPremise(startInfo, typeOfExpectedCred, binding).ConfigureAwait(false);

                            if (askForCredential)
                            {
                                askForCredentialTimes++;
                            }

                            SessionBase.CheckCredential(startInfo);
                        }

                        resourceProvider = BuildResourceProvider(startInfo, durable, binding);
                        if (((newDataClients == null) || (newDataClients.Count == 0)) &&
                            (startInfo.DependFiles != null) && (startInfo.DependFiles.Count > 0))
                        {
                            // Upload the data files required for this session
                            newDataClients = this.UploadAzureFiles(startInfo);
                        }

                        watch.Stop();

                        // re-calculate the timeout to exclude the timespan for getting credential
                        targetTimeout = targetTimeout.AddMilliseconds(watch.ElapsedMilliseconds);
                        sessionAllocateInfo = await resourceProvider.AllocateResource(startInfo, durable, SessionBase.GetTimeout(targetTimeout)).ConfigureAwait(false);
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] Successfully allocated resource.", sessionAllocateInfo.Id);

                        if (sessionAllocateInfo.BrokerLauncherEpr != null)
                        {
                            if (SoaHelper.IsSchedulerOnIaaS(startInfo.Headnode))
                            {
                                string suffix = SoaHelper.GetSuffixFromHeadNodeEpr(startInfo.Headnode);
                                for (int i = 0; i < sessionAllocateInfo.BrokerLauncherEpr.Length; i++)
                                {
                                    sessionAllocateInfo.BrokerLauncherEpr[i] = SoaHelper.UpdateEprWithCloudServiceName(sessionAllocateInfo.BrokerLauncherEpr[i], suffix);
                                }
                            }
                            SessionBase.SaveCrendential(startInfo, binding);
                            SessionBase.TraceSource.TraceInformation("Get the EPR list from headnode. number of eprs={0}", sessionAllocateInfo.BrokerLauncherEpr.Length);
                            break;
                        }
                    }
                    catch (EndpointNotFoundException e)
                    {
                        this.CleanUpDataClients(startInfo, newDataClients);
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:Unknown] EndpointNotFoundException occured while allocating resource: {0}", e);
                        SessionBase.HandleEndpointNotFoundException(startInfo.Headnode);
                    }
                    catch (AuthenticationException e)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:Unknown] AuthenticationException occured while allocating resource: {0}", e);

                        startInfo.ClearCredential();
                        SessionBase.PurgeCredential(startInfo);
                        if (Utility.CanRetry(retry, askForCredential, askForCredentialTimes))
                        {
                            if (typeOfExpectedCred == CredType.None)
                            {
                                typeOfExpectedCred = await CredUtil.GetCredTypeFromClusterAsync(startInfo, binding).ConfigureAwait(false);
                            }

                            if (resourceProvider is IDisposable)
                            {
                                ((IDisposable)resourceProvider).Dispose();
                            }
                            continue;
                        }
                        else
                        {
                            if (sessionAllocateInfo != null)
                            {
                                await resourceProvider.FreeResource(startInfo, sessionAllocateInfo.Id).ConfigureAwait(false);
                            }
                            throw;
                        }
                    }
                    catch (MessageSecurityException e)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:Unknown] MessageSecurityException occured while allocating resource: {0}", e);

                        startInfo.ClearCredential();
                        SessionBase.PurgeCredential(startInfo);
                        if (Utility.CanRetry(retry, askForCredential, askForCredentialTimes))
                        {
                            if (typeOfExpectedCred == CredType.None)
                            {
                                typeOfExpectedCred = await CredUtil.GetCredTypeFromClusterAsync(startInfo, binding).ConfigureAwait(false);
                            }

                            if (resourceProvider is IDisposable)
                            {
                                ((IDisposable)resourceProvider).Dispose();
                            }
                            continue;
                        }
                        else
                        {
                            if (sessionAllocateInfo != null)
                            {
                                await resourceProvider.FreeResource(startInfo, sessionAllocateInfo.Id).ConfigureAwait(false);
                            }
                            throw;
                        }
                    }
                    catch (FaultException<SessionFault> ex)
                    {
                        typeOfExpectedCred = Utility.CanRetry(retry, askForCredential, askForCredentialTimes) ?
                            CredUtil.GetCredTypeFromFaultCode(ex.Detail.Code) : CredType.None;
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:Unknown] Fault exception occured while allocating resource. FaultCode = {0}. Exception = {1}", ex.Detail.Code, ex.ToString());

                        if (typeOfExpectedCred == CredType.None)
                        {
                            this.CleanUpDataClients(startInfo, newDataClients);
                            if (sessionAllocateInfo != null)
                            {
                                await resourceProvider.FreeResource(startInfo, sessionAllocateInfo.Id).ConfigureAwait(false);
                            }
                            throw Utility.TranslateFaultException(ex);
                        }
                        else
                        {
                            startInfo.ClearCredential();

                            if (resourceProvider is IDisposable)
                            {
                                ((IDisposable)resourceProvider).Dispose();
                            }
                            continue;
                        }
                    }
                    catch (CommunicationException e)
                    {
                        this.CleanUpDataClients(startInfo, newDataClients);
                        if (sessionAllocateInfo != null)
                        {
                            await resourceProvider.FreeResource(startInfo, sessionAllocateInfo.Id).ConfigureAwait(false);
                        }
                        throw new SessionException(SOAFaultCode.ConnectSessionLauncherFailure, SR.ConnectSessionLauncherFailure, e);
                    }
                    catch (TimeoutException e)
                    {
                        SessionBase.TraceSource.TraceInformation(e.ToString());
                        this.CleanUpDataClients(startInfo, newDataClients);
                        if (sessionAllocateInfo != null)
                        {
                            await resourceProvider.FreeResource(startInfo, sessionAllocateInfo.Id).ConfigureAwait(false);
                        }
                        throw new TimeoutException(string.Format(SR.ConnectSessionLauncherTimeout, Constant.DefaultCreateSessionTimeout), e);
                    }
                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:Unknown] Exception occured while allocating resource: {0}", e);

                        this.CleanUpDataClients(startInfo, newDataClients);
                        if (sessionAllocateInfo != null)
                        {
                            await resourceProvider.FreeResource(startInfo, sessionAllocateInfo.Id).ConfigureAwait(false);
                        }
                        throw;
                    }

                    if (sessionAllocateInfo.BrokerLauncherEpr == null && startInfo.UseSessionPool)
                    {
                        // If the session launcher picks a session from the pool, it returns the seesion id.
                        // eprs is null at this case. Try to attach to the session.

                        try
                        {
                            if (sessionAllocateInfo.SessionInfo == null)
                            {
                                SessionBase.TraceSource.TraceInformation("[Session:{0}] Attempt to attach to session {0} which is part of the session pool.", sessionAllocateInfo.Id);
                                return await AttachSession(new SessionAttachInfo(startInfo.Headnode, sessionAllocateInfo.Id), durable, timeoutMilliseconds, binding).ConfigureAwait(false);
                            }
                            else
                            {
                                SessionBase.TraceSource.TraceInformation("[Session:{0}] Attempt to attach to broker of the session {0} which is part of the session pool.", sessionAllocateInfo.Id);
                                return await AttachBroker(startInfo, sessionAllocateInfo.SessionInfo, durable, timeoutMilliseconds, binding).ConfigureAwait(false);
                            }
                        }
                        catch (Exception e)
                        {
                            SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "Failed to attach to the session {0}. {1}", sessionAllocateInfo.Id, e);
                            if (Utility.CanRetry(retry, askForCredential, askForCredentialTimes))
                            {
                                if (resourceProvider is IDisposable)
                                {
                                    ((IDisposable)resourceProvider).Dispose();
                                }
                                continue;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }

                IBrokerFactory brokerFactory = BuildBrokerFactory(startInfo, durable);

                try
                {
                    return await brokerFactory.CreateBroker(startInfo, sessionAllocateInfo.Id, targetTimeout, sessionAllocateInfo.BrokerLauncherEpr, binding).ConfigureAwait(false);
                }
                catch
                {
                    // Free resource if failed to create broker or create session
                    this.CleanUpDataClients(startInfo, newDataClients);
                    await resourceProvider.FreeResource(startInfo, sessionAllocateInfo.Id).ConfigureAwait(false);
                    throw;
                }
                finally
                {
                    if (brokerFactory is IDisposable)
                    {
                        ((IDisposable)brokerFactory).Dispose();
                    }
                }
            }
            finally
            {
                if (resourceProvider != null)
                {
                    if (resourceProvider is IDisposable)
                    {
                        ((IDisposable)resourceProvider).Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Attach to an existing session
        /// </summary>
        /// <param name="attachInfo">indicating the session attach information</param>
        /// <param name="durable">indicating a value whether a durable session is to be created</param>
        /// <param name="timeoutMilliseconds">indicating the timeout</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>returns session instance</returns>
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

                        if (attachInfo.UseAad)
                        {
                            // Authentication handled by AADUtil
                        }
                        else if (SoaHelper.IsSchedulerOnAzure(attachInfo.Headnode) || SoaHelper.IsSchedulerOnIaaS(attachInfo.Headnode) || (attachInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http)
                        {
                            askForCredential = SessionBase.RetrieveCredentialOnAzure(attachInfo);

                            if (askForCredential)
                            {
                                askForCredentialTimes++;
                            }
                        }
                        else
                        {
                            askForCredential = await SessionBase.RetrieveCredentialOnPremise(attachInfo, typeOfExpectedCred, binding).ConfigureAwait(false);

                            if (askForCredential)
                            {
                                askForCredentialTimes++;
                            }

                            SessionBase.CheckCredential(attachInfo);
                        }

                        resourceProvider = BuildResourceProvider(attachInfo, durable, binding);
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
                        SessionBase.SaveCrendential(attachInfo);
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

                        attachInfo.ClearCredential();
                        SessionBase.PurgeCredential(attachInfo);
                        if (Utility.CanRetry(retry, askForCredential, askForCredentialTimes))
                        {
                            if (typeOfExpectedCred == CredType.None)
                            {
                                typeOfExpectedCred = await CredUtil.GetCredTypeFromClusterAsync(attachInfo, binding).ConfigureAwait(false);
                            }

                            if (resourceProvider is IDisposable)
                            {
                                ((IDisposable)resourceProvider).Dispose();
                            }
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (MessageSecurityException e)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:{0}] MessageSecurityException occured while attaching session: {1}", attachInfo.SessionId, e);

                        attachInfo.ClearCredential();
                        SessionBase.PurgeCredential(attachInfo);
                        if (Utility.CanRetry(retry, askForCredential, askForCredentialTimes))
                        {
                            if (typeOfExpectedCred == CredType.None)
                            {
                                typeOfExpectedCred = await CredUtil.GetCredTypeFromClusterAsync(attachInfo, binding).ConfigureAwait(false);
                            }

                            if (resourceProvider is IDisposable)
                            {
                                ((IDisposable)resourceProvider).Dispose();
                            }
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (FaultException<SessionFault> ex)
                    {
                        typeOfExpectedCred = Utility.CanRetry(retry, askForCredential, askForCredentialTimes) ?
                            CredUtil.GetCredTypeFromFaultCode(ex.Detail.Code) : CredType.None;
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

            if (String.IsNullOrEmpty(info.BrokerLauncherEpr) && !info.UseInprocessBroker)
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

        /// <summary>
        /// Attach the broker
        /// </summary>
        /// <param name="startInfo">session start info</param>
        /// <param name="sessionInfo">session info</param>
        /// <param name="durable">whether durable session</param>
        /// <param name="timeoutMilliseconds">attach timeout</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>session object</returns>
        public override async Task<SessionBase> AttachBroker(SessionStartInfo startInfo, SessionInfoContract sessionInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            DateTime targetTimeout;

            Utility.ThrowIfNull(sessionInfo, "sessionInfo");
            Utility.ThrowIfInvalidTimeout(timeoutMilliseconds, "timeoutMilliseconds");

            if (timeoutMilliseconds == Timeout.Infinite)
            {
                targetTimeout = DateTime.MaxValue;
            }
            else
            {
                targetTimeout = DateTime.Now.AddMilliseconds(timeoutMilliseconds);
            }

            SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0, "[Session:{0}] Start to attach broker...", sessionInfo.Id);

            SessionInfo info = Utility.BuildSessionInfoFromDataContract(sessionInfo); // resourceProvider.GetResourceInfo(attachInfo, SessionBase.GetTimeout(targetTimeout));
            SessionAttachInfo attachInfo = new SessionAttachInfo(startInfo.Headnode, sessionInfo.Id);
            attachInfo.TransportScheme = startInfo.TransportScheme;
            attachInfo.Username = startInfo.Username;
            attachInfo.InternalPassword = startInfo.InternalPassword;

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


        /// <summary>
        /// Build resource provider by start info
        /// </summary>
        /// <param name="startInfo">indicating the session start information</param>
        /// <param name="durable">indicating whether the session is a durable session</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>returns the instance of IResourceProvider</returns>
        private static IResourceProvider BuildResourceProvider(SessionStartInfo startInfo, bool durable, Binding binding)
        {
            if (!startInfo.DebugModeEnabled)
            {
                Debug.Assert(startInfo.Headnode != null, "[SessionFactory] Head node should not be null if debug mode is enabled.");
                return new ServiceJobProvider(startInfo, binding);
            }
            else
            {
                return new DummyResourceProvider(durable);
            }
        }

        /// <summary>
        /// Build resource provider by attach info
        /// </summary>
        /// <param name="attachInfo">indicating the session attach information</param>
        /// <param name="durable">indicating whether the session is a durable session</param>
        /// <param name="binding">indicting the binding</param>
        /// <returns>returns the instance of IResourceProvider</returns>
        private static IResourceProvider BuildResourceProvider(SessionAttachInfo attachInfo, bool durable, Binding binding)
        {
            if (!attachInfo.DebugModeEnabled)
            {
                Debug.Assert(attachInfo.Headnode != null, "[SessionFactory] Head node should not be null if debug mode is enabled.");
                return new ServiceJobProvider(attachInfo, binding);
            }
            else
            {
                return new DummyResourceProvider(durable);
            }
        }

        /// <summary>
        /// Build broker factory
        /// </summary>
        private static IBrokerFactory BuildBrokerFactory(SessionStartInfo startInfo, bool durable)
        {
            if (startInfo.UseInprocessBroker)
            {
                return new InprocessBrokerFactory(startInfo.Headnode, durable);
            }
            else
            {
                return new V3BrokerFactory(durable);
            }
        }

        /// <summary>
        /// Build broker factory
        /// </summary>
        private static IBrokerFactory BuildBrokerFactory(SessionAttachInfo attachInfo, bool durable)
        {
            if (attachInfo.UseInprocessBroker)
            {
                return new InprocessBrokerFactory(attachInfo.Headnode, durable);
            }
            else
            {
                return new V3BrokerFactory(durable);
            }
        }

        /// <summary>
        /// Upload Azure files
        /// </summary>
        /// <param name="startInfo">The session start info</param>
        private IList<string> UploadAzureFiles(SessionStartInfo startInfo)
        {
            List<string> newClientIdList = null;
            if (startInfo.DependFiles != null && startInfo.DependFiles.Count > 0)
            {
                bool allUploaded = false;
                SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Start to upload data files to Azure blob");
                newClientIdList = new List<string>();
                List<string> depList = new List<string>();
                try
                {
                    foreach (string srcfile in startInfo.DependFiles.Keys)
                    {
                        if (!File.Exists(srcfile))
                        {
                            throw new IOException(string.Format("The file {0} doesn't exist", srcfile));
                        }

                        string md5ByteStr = string.Empty;
                        string md5base64Str = string.Empty;
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(srcfile))
                            {
                                var hashBytes = md5.ComputeHash(stream);
                                md5base64Str = Convert.ToBase64String(hashBytes);
                                md5ByteStr = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                                SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "Source file {0} md5: {1}", srcfile, md5ByteStr);
                            }
                        }

                        string clientId = string.Format("{0}_{1}", startInfo.Username.Replace(@"\", "-").ToLower(), md5ByteStr);
                        bool removeClient = false;
                        DataClient client = null;
                        try
                        {
                            client = DataClient.Open(startInfo, clientId);
                            string contentMd5 = client.GetContentMd5();
                            if (contentMd5 != md5base64Str)
                            {
                                SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0, "The content of data client {0} is not correct: expected md5 {1}, real md5 {2}", clientId, md5base64Str, contentMd5);
                                removeClient = true;
                            }
                        }
                        catch (DataException e)
                        {
                            if (e.ErrorCode == DataErrorCode.DataTransferToAzureFailed || e.ErrorCode == DataErrorCode.DataInconsistent)
                            {
                                // DataTransferToAzureFailed: Blob primary data container and the data transfer to blob failed.
                                // DataInconsistent: The data is inconsistent
                                SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "The data client {0} already exists but data corrupt: {1}", clientId, e);
                                removeClient = true;
                            }
                            else if (e.ErrorCode == DataErrorCode.ConnectDataServiceFailure)
                            {
                                SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "Open data client {0} failed due to communication failure: {1}", clientId, e);
                                if (e.InnerException is MessageSecurityException)
                                {
                                    // If caused by MessageSecurityException, we shall throw InnerException, so that prompt for username/password
                                    throw e.InnerException;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            else if (e.ErrorCode != DataErrorCode.DataClientNotFound)
                            {
                                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "Open data client {0} failed: {1}", clientId, e);
                                throw;
                            }
                        }

                        try
                        {
                            if (removeClient)
                            {
                                DataClient.Delete(startInfo, clientId);
                                client = null;
                            }

                            if (client == null)
                            {
                                SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0, "Create data client {0} for file {1}", clientId, srcfile);
                                client = DataClient.Create(startInfo, clientId, true);
                                newClientIdList.Add(clientId);
                                client.WriteRawBytesAll(File.ReadAllBytes(srcfile));
                            }

                            depList.Add(string.Format("{0}={1}", clientId, startInfo.DependFiles[srcfile]));
                        }
                        catch (Exception e)
                        {
                            SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "Failed to create data client {0}: {1}", clientId, e);
                            throw;
                        }
                    }

                    allUploaded = true;
                }
                finally
                {
                    if (!allUploaded && newClientIdList.Count > 0)
                    {
                        SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0, "Failed to upload some files, remove the files already uploaded");
                        CleanUpDataClients(startInfo, newClientIdList);
                        newClientIdList = null;
                    }
                }

                startInfo.Data.DependFiles = string.Join(";", depList.ToArray());
            }

            return newClientIdList;
        }

        private void CleanUpDataClients(SessionStartInfo startInfo, IList<string> dataClientIds)
        {
            if (dataClientIds == null)
            {
                return;
            }

            foreach (var removeId in dataClientIds)
            {
                try
                {
                    DataClient.Delete(startInfo, removeId);
                }
                catch (Exception ex)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "Failed to remove data client {0}: {1}", removeId, ex);
                }
            }
        }
    }
}
