//------------------------------------------------------------------------------
// <copyright file="SessionAsyncResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       AsyncResult for async create session
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using System;
    using System.Diagnostics;
    using System.Security.Authentication;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Threading.Tasks;
    /// <summary>
    /// AsyncResult for async create session
    /// </summary>
    internal class SessionAsyncResult : SessionAsyncResultBase
    {
        /// <summary>
        /// Stores the hard coded retry limit
        /// </summary>
        private const int HardCodedRetryLimit = 3;

        /// <summary>
        /// The EPRs of the broker launcher canidates.
        /// </summary>
        private string[] eprCanidates;

        /// <summary>
        /// The current broker launcher we are connecting
        /// </summary>
        private int eprPtr;

        /// <summary>
        /// The session id 
        /// </summary>
        private int sessionid;

        /// <summary>
        /// Service version
        /// </summary>
        private string serviceVersion;

        /// <summary>
        /// retry when fail
        /// </summary>
        private int retry = HardCodedRetryLimit;

        /// <summary>
        /// Binding
        /// </summary>
        protected Binding binding = null;

        /// <summary>
        /// Initializes a new instance of the SessionAsyncResult class
        /// </summary>
        /// <param name="startInfo">the session start info</param>
        /// <param name="binding">indicating the binding</param>
        /// <param name="callback">the async callback</param>
        /// <param name="asyncState">the async state</param>
        public SessionAsyncResult(
            SessionStartInfo startInfo,
            Binding binding,
            AsyncCallback callback,
            object asyncState)
            : base(startInfo, callback, asyncState)
        {
            //SessionLauncherClient client = new SessionLauncherClient(startInfo);
            this.binding = binding;
            //TODO: remove blocking wait
            BeginAllocate(startInfo, CredType.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Allocate the session resource. The function will check password
        /// </summary>
        /// <param name="client"></param>
        /// <param name="startInfo"></param>
        private async Task BeginAllocate(SessionStartInfo startInfo, CredType credType)
        {
            bool durable = (GetType() == typeof(DurableSessionAsyncResult));
            SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:Unknown] Start to create session, IsDurable = {0}.", durable);
            while (retry > 0)
            {
                try
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:Unknown] Authenticating... Retry Count = {0}, CredType = {1}", HardCodedRetryLimit + 1 - retry, credType);
                    if (SoaHelper.IsSchedulerOnAzure(this.StartInfo.Headnode) || SoaHelper.IsSchedulerOnIaaS(this.StartInfo.Headnode))
                    {
                        SessionBase.RetrieveCredentialOnAzure(this.StartInfo);
                    }
                    else
                    {
                        await SessionBase.RetrieveCredentialOnPremise(this.StartInfo, credType, this.binding).ConfigureAwait(false);
                        SessionBase.CheckCredential(this.StartInfo);
                    }

                    break;
                }
                catch (AuthenticationException)
                {
                    if (credType == CredType.None)
                    {
                        credType = await CredUtil.GetCredTypeFromClusterAsync(this.StartInfo, this.binding).ConfigureAwait(false);
                    }

                    this.StartInfo.ClearCredential();
                    SessionBase.PurgeCredential(this.StartInfo);

                    retry--;
                    if (retry == 0)
                    {
                        throw;
                    }
                }
            }

            SessionLauncherClient client = new SessionLauncherClient(startInfo, this.binding);

            try
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:Unknown] Allocating resource...");
                if (durable)
                {
                    if ((StartInfo.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
                    {
                        client.BeginAllocateDurable(this.StartInfo.Data, SessionLauncherClient.EndpointPrefix, this.AllocateCallback, client);
                    }
                    else if ((StartInfo.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
                    {
                        client.BeginAllocateDurable(this.StartInfo.Data, SessionLauncherClient.HttpsEndpointPrefix, this.AllocateCallback, client);
                    }
                    else if ((StartInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http)
                    {
                        client.BeginAllocateDurable(this.StartInfo.Data, SessionLauncherClient.HttpsEndpointPrefix, this.AllocateCallback, client);
                    }
                    else if ((StartInfo.TransportScheme & TransportScheme.Custom) == TransportScheme.Custom)
                    {
                        client.BeginAllocateDurable(this.StartInfo.Data, SessionLauncherClient.EndpointPrefix, this.AllocateCallback, client);
                    }

                }
                else
                {
                    if ((StartInfo.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
                    {
                        client.BeginAllocate(this.StartInfo.Data, SessionLauncherClient.EndpointPrefix, this.AllocateCallback, client);
                    }
                    else if ((StartInfo.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
                    {
                        client.BeginAllocate(this.StartInfo.Data, SessionLauncherClient.HttpsEndpointPrefix, this.AllocateCallback, client);
                    }
                    else if ((StartInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http)
                    {
                        client.BeginAllocate(this.StartInfo.Data, SessionLauncherClient.HttpsEndpointPrefix, this.AllocateCallback, client);
                    }
                    else if ((StartInfo.TransportScheme & TransportScheme.Custom) == TransportScheme.Custom)
                    {
                        client.BeginAllocate(this.StartInfo.Data, SessionLauncherClient.EndpointPrefix, this.AllocateCallback, client);
                    }

                }
            }
            catch (EndpointNotFoundException e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:Unknown] EndpointNotFoundException occured while allocating resource: {0}", e);
                SessionBase.HandleEndpointNotFoundException(this.StartInfo.Headnode);
            }

            return;
        }

        /// <summary>
        /// Gets the session id
        /// </summary>
        protected int SessionId
        {
            get { return this.sessionid; }
        }

        /// <summary>
        /// Create the session from the broker launcher uri
        /// </summary>
        /// <param name="uri">The broker launcher uri</param>
        protected virtual void BeginCreateSession(string uri)
        {
            SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] Start create broker against uri: {1}", this.sessionid, uri);

            BrokerLauncherClient brokerLauncher = new BrokerLauncherClient(new Uri(uri), this.StartInfo, this.binding);

            brokerLauncher.BeginCreate(this.StartInfo.Data, this.sessionid, this.CreateCallback, brokerLauncher);
        }

        /// <summary>
        /// The callback for the create session operation 
        /// </summary>
        /// <param name="ar">the asnc result</param>
        protected void CreateCallback(IAsyncResult ar)
        {
            try
            {
                bool durable = (GetType() == typeof(DurableSessionAsyncResult));
                SessionInfo info = SessionBase.BuildSessionInfo(this.EndCreateSession(ar), false, this.sessionid, this.eprCanidates[this.eprPtr - 1], this.StartInfo.Data.ServiceVersion, this.StartInfo);
                SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] Successfully created broker.", this.sessionid);
                this.MarkFinish(
                    null,
                    durable ?
                        (SessionBase)new DurableSession(info, this.StartInfo.Headnode, this.binding) :
                        (SessionBase)new V3Session(info, this.StartInfo.Headnode, this.StartInfo.ShareSession, this.binding));
            }
            catch (FaultException<SessionFault> ex)
            {
                this.MarkFinish(Utility.TranslateFaultException(ex));
                return;
            }
            catch (CommunicationException e)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:{0}] Communication exception occured while creating broker: {1}. Will try next broker node candidate.", this.sessionid, e);
                if (!this.Canceled)
                {
                    this.ConnectToNextBroker();
                }

                return;
            }
            catch (Exception ex)
            {
                this.MarkFinish(ex);
            }
        }

        /// <summary>
        /// End the async operation.
        /// </summary>
        /// <param name="ar">the async result</param>
        /// <returns>the session info</returns>
        protected virtual BrokerInitializationResult EndCreateSession(IAsyncResult ar)
        {
            BrokerLauncherClient brokerLauncher = (BrokerLauncherClient)ar.AsyncState;
            try
            {
                return brokerLauncher.EndCreate(ar);
            }
            finally
            {
                Utility.SafeCloseCommunicateObject(brokerLauncher);
            }
        }

        /// <summary>
        /// When failure, terminate the session
        /// </summary>
        protected override async Task CleanupOnFailure()
        {
            if (this.sessionid != 0)
            {
                SessionLauncherClient client = new SessionLauncherClient(this.StartInfo, this.binding);
                try
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0, "[Session:{0}] Terminate session because create session failed.", this.sessionid);
                    await client.TerminateV5Async(this.sessionid).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:{0}] Failed to terminate session: {1}", this.sessionid, e);

                    // swallow
                }
                finally
                {
                    Utility.SafeCloseCommunicateObject(client);
                }
            }
        }

        /// <summary>
        /// The callback function for the allocate operation to session launcher
        /// </summary>
        /// <param name="ar">the async result</param>
        private void AllocateCallback(IAsyncResult ar)
        {
            SessionLauncherClient client = (SessionLauncherClient)ar.AsyncState;
            SessionInfoContract sessionInfo;

            try
            {
                try
                {
                    bool durable = (GetType() == typeof(DurableSessionAsyncResult));

                    if (durable)
                    {
                        this.eprCanidates = client.EndAllocateDurable(out this.sessionid, out serviceVersion, out sessionInfo, ar);
                    }
                    else
                    {
                        this.eprCanidates = client.EndAllocate(out this.sessionid, out serviceVersion, out sessionInfo, ar);
                    }

                    SessionBase.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[Session:{0}] Successfully allocated resource. ServiceVersion = {1}", this.sessionid, serviceVersion);
                    this.eprPtr = 0;

                    // If HpcSession returns a version, pass it to HpcBroker
                    if (!String.IsNullOrEmpty(serviceVersion))
                    {
                        try
                        {
                            this.StartInfo.Data.ServiceVersion = new Version(serviceVersion);
                        }

                        catch
                        {
                            throw new SessionException(SR.InvalidServiceVersionReturned);
                        }
                    }

                    SessionBase.SaveCrendential(this.StartInfo, this.binding);

                    if (this.eprCanidates == null || this.eprCanidates.Length == 0)
                        throw new SessionException(SR.NoBrokerNodeFound);

                    if (SoaHelper.IsSchedulerOnIaaS(this.StartInfo.Headnode))
                    {
                        string suffix = SoaHelper.GetSuffixFromHeadNodeEpr(this.StartInfo.Headnode);
                        for (int i = 0; i < this.eprCanidates.Length; i++)
                        {
                            this.eprCanidates[i] = SoaHelper.UpdateEprWithCloudServiceName(this.eprCanidates[i], suffix);
                        }
                    }

                    SessionBase.TraceSource.TraceInformation("Get the EPR list from headnode. number of eprCanidates={0}", this.eprCanidates.Length);



                    if (!this.Canceled)
                    {
                        this.ConnectToNextBroker();
                    }
                    else
                    {
                        // Bug 11765: If the operation is canceled, when allocating
                        // resource, do cleanup the resource
                        this.CleanupOnFailure().GetAwaiter().GetResult();
                    }
                }
                catch (AuthenticationException ex)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:Unknown] AuthenticationException occured while allocating resource: {0}", ex);

                    this.StartInfo.ClearCredential();
                    SessionBase.PurgeCredential(this.StartInfo);

                    if (retry > 0)
                    {
                        //TODO: remove sync wait
                        BeginAllocate(this.StartInfo, CredType.None).GetAwaiter().GetResult();
                    }
                    else
                    {
                        this.MarkFinish(new AuthenticationException(SR.AuthenticationFailed, ex));
                    }
                }
                catch (MessageSecurityException ex)
                {
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:Unknown] MessageSecurityException occured while allocating resource: {0}", ex);

                    this.StartInfo.ClearCredential();
                    SessionBase.PurgeCredential(this.StartInfo);

                    if (retry > 0)
                    {
                        //TODO: remove sync wait
                        BeginAllocate(this.StartInfo, CredType.None).GetAwaiter().GetResult();
                    }
                    else
                    {
                        this.MarkFinish(new AuthenticationException(SR.AuthenticationFailed, ex));
                    }
                }
                catch (FaultException<SessionFault> ex)
                {
                    int faultCode = (ex as FaultException<SessionFault>).Detail.Code;
                    CredType type = CredUtil.GetCredTypeFromFaultCode(faultCode);
                    SessionBase.TraceSource.TraceEvent(TraceEventType.Warning, 0, "[Session:Unknown] Fault exception occured while allocating resource. FaultCode = {0}, CredType = {1}", faultCode, type);

                    if (type != CredType.None)
                    {
                        this.StartInfo.ClearCredential();

                        if (retry > 0)
                        {
                            //TODO: remove sync wait
                            BeginAllocate(this.StartInfo, type).GetAwaiter().GetResult();
                        }
                        else
                        {
                            this.MarkFinish(new AuthenticationException(SR.AuthenticationFailed, ex));
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            catch (FaultException<SessionFault> ex)
            {
                this.MarkFinish(Utility.TranslateFaultException(ex));
            }
            catch (Exception ex)
            {
                this.MarkFinish(ex);
            }
            finally
            {
                Utility.SafeCloseCommunicateObject(client);
            }

        }

        /// <summary>
        /// Connect to the next broker launcher
        /// </summary>
        private void ConnectToNextBroker()
        {
            if (this.eprPtr >= this.eprCanidates.Length)
            {
                this.MarkFinish(new SessionException(SR.NoBrokerNodeFound));
            }
            else
            {
                this.BeginCreateSession(this.eprCanidates[this.eprPtr++]);
            }
        }
    }
}
