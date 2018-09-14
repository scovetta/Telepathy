//------------------------------------------------------------------------------
// <copyright file="AzureRestSessionFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Session factory for Azure cluster (communicating using REST)
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Threading.Tasks;
    /// <summary>
    /// Session factory for Azure cluster (communicating using REST)
    /// </summary>
    internal class AzureRestSessionFactory : SessionFactory
    {
        /// <summary>
        /// Stores the serializer for WebSessionStartInfo
        /// </summary>
        private DataContractSerializer sessionStartInfoSerializer = new DataContractSerializer(typeof(WebSessionStartInfo));

        /// <summary>
        /// Stores the serializer for WebSessionInfoContract
        /// </summary>
        private DataContractSerializer webSessionInfoSerializer = new DataContractSerializer(typeof(WebSessionInfoContract));

        /// <summary>
        /// Create session
        /// </summary>
        /// <param name="startInfo">indicating session start information</param>
        /// <param name="durable">indicating a value whether a durable session is to be created</param>
        /// <param name="timeoutMilliseconds">indicating the timeout</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>returns session instance</returns>
        [Refactor("Make this method a real async method")]
        public override Task<SessionBase> CreateSession(SessionStartInfo startInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            SessionBase.CheckSessionStartInfo(startInfo);
            DateTime targetTimeout = DateTime.Now.AddMilliseconds(Constant.DefaultCreateSessionTimeout);

            // if following env variable is set, we will launch the service host in an admin job.
            string adminJobEnv = Environment.GetEnvironmentVariable(SessionBase.EnvVarNameForAdminJob, EnvironmentVariableTarget.Process);
            if (!string.IsNullOrEmpty(adminJobEnv) && adminJobEnv.Equals("1", StringComparison.InvariantCultureIgnoreCase))
            {
                startInfo.AdminJobForHostInDiag = true;
            }

            NetworkCredential credential = null;
            HttpWebRequest createSessionRequest = null;
            WebSessionInfo sessionInfo = null;

            int retry = 0;
            bool askForCredential = false;
            int askForCredentialTimes = 0;

            // Align the retry behavior of the RestSession with the original session.
            // User can try credential at most SessionBase.MaxRetryCount times.
            while (true)
            {
                retry++;

                SessionBase.TraceSource.TraceInformation("[Session:Unknown] Try to create session via REST API. TryCount = {0}, IsDurable = {1}", retry, durable);
                askForCredential = SessionBase.RetrieveCredentialOnAzure(startInfo);
                if (askForCredential)
                {
                    askForCredentialTimes++;
                }

                credential = Utility.BuildNetworkCredential(startInfo.Username, startInfo.InternalPassword);

                try
                {
                    // Following method needs to get cluster name, it may throw WebException because
                    // of invalid credential. Give chance to users to re-enter the credential.
                    createSessionRequest = SOAWebServiceRequestBuilder.GenerateCreateSessionWebRequest(startInfo.Headnode, durable, credential, WebMessageFormat.Xml);
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = (HttpWebResponse)e.Response;
                        if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            SessionBase.TraceSource.TraceInformation("[Session:Unknown] Authentication failed. Try to ask for credential again.");

                            // cleanup local cached invalid credential
                            startInfo.ClearCredential();
                            SessionBase.PurgeCredential(startInfo);

                            if (Utility.CanRetry(retry, askForCredential, askForCredentialTimes))
                            {
                                response.Close();
                                continue;
                            }
                        }
                    }

                    SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:Unknown] Failed to build CreateSession request: {0}", e);

                    Utility.HandleWebException(e);
                }

                try
                {
                    using (Stream stream = createSessionRequest.GetRequestStream())
                    {
                        WebSessionStartInfo info = WebAPIUtility.ToWebSessionStartInfo(startInfo.Data);
                        this.sessionStartInfoSerializer.WriteObject(stream, info);
                    }

                    using (HttpWebResponse response = (HttpWebResponse)createSessionRequest.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        sessionInfo = Utility.BuildWebSessionInfoFromDataContract((WebSessionInfoContract)this.webSessionInfoSerializer.ReadObject(stream));
                    }

                    break;
                }
                catch (WebException e)
                {
                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Error, 0, "[Session:Unknown] Failed to create session via REST API: {0}", e);

                    Utility.HandleWebException(e);
                }
            }

            SessionBase.SaveCrendential(startInfo, binding);
            sessionInfo.HeadNode = startInfo.Headnode;
            sessionInfo.Credential = credential;

            if (durable)
            {
#if net40
                return TaskEx.FromResult<SessionBase>(new DurableSession(sessionInfo, startInfo.Headnode, binding));
#else
                return Task.FromResult<SessionBase>(new DurableSession(sessionInfo, startInfo.Headnode, binding));
#endif
            }
            else
            {
#if net40
                return TaskEx.FromResult<SessionBase>(new V3Session(sessionInfo, startInfo.Headnode, startInfo.ShareSession, binding));
#else
                return Task.FromResult<SessionBase>(new V3Session(sessionInfo, startInfo.Headnode, startInfo.ShareSession, binding));
#endif
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
        [Refactor("Make this method a real async method")]
        public override Task<SessionBase> AttachSession(SessionAttachInfo attachInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            SessionAttachInfo info = attachInfo as SessionAttachInfo;

            NetworkCredential credential = null;
            WebSessionInfo sessionInfo = null;

            int retry = 0;
            bool askForCredential = false;
            int askForCredentialTimes = 0;

            // User can try credential at most SessionBase.MaxRetryCount times.
            while (true)
            {
                retry++;
                SessionBase.TraceSource.TraceInformation("[Session:Unknown] Try to attach session via REST API. TryCount = {0}, IsDurable = {1}", retry, durable);
                askForCredential = SessionBase.RetrieveCredentialOnAzure(info);

                if (askForCredential)
                {
                    askForCredentialTimes++;
                }

                credential = Utility.BuildNetworkCredential(info.Username, info.InternalPassword);

                sessionInfo = null;

                HttpWebRequest attachSessionRequest = null;

                try
                {
                    // Following method needs to get cluster name, it may throw WebException because
                    // of invalid credential. Give chance to users to re-enter the credential.
                    attachSessionRequest = SOAWebServiceRequestBuilder.GenerateAttachSessionWebRequest(info.Headnode, info.SessionId, durable, credential);
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = (HttpWebResponse)e.Response;
                        if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            // cleanup local cached invalid credential
                            info.ClearCredential();
                            SessionBase.PurgeCredential(info);

                            if (Utility.CanRetry(retry, askForCredential, askForCredentialTimes))
                            {
                                response.Close();
                                continue;
                            }
                        }
                    }

                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Error, 0, "[Session:Unknown] Failed to build AttachSession request: {0}", e);

                    Utility.HandleWebException(e);
                }

                try
                {
                    using (WebResponse response = attachSessionRequest.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    {
                        sessionInfo = Utility.BuildWebSessionInfoFromDataContract((WebSessionInfoContract)this.webSessionInfoSerializer.ReadObject(stream));
                    }

                    break;
                }
                catch (WebException e)
                {
                    SessionBase.TraceSource.TraceEvent(
                        TraceEventType.Error, 0, "[Session:Unknown] Failed to attach session via REST API: {0}", e);

                    Utility.HandleWebException(e);
                }
            }

            SessionBase.SaveCrendential(info);

            sessionInfo.HeadNode = info.Headnode;
            sessionInfo.Credential = credential;

            if (durable)
            {
#if net40
                return TaskEx.FromResult<SessionBase>(new DurableSession(sessionInfo, attachInfo.Headnode, binding));
#else
                return Task.FromResult<SessionBase>(new DurableSession(sessionInfo, attachInfo.Headnode, binding));
#endif
            }
            else
            {
#if net40
                return TaskEx.FromResult<SessionBase>(new V3Session(sessionInfo, attachInfo.Headnode, true, binding));
#else
                return Task.FromResult<SessionBase>(new V3Session(sessionInfo, attachInfo.Headnode, true, binding));
#endif
            }
        }

        public override Task<SessionBase> AttachBroker(SessionStartInfo startInfo, SessionInfoContract sessionInfo, bool durable, int timeoutMilliseconds, Binding binding)
        {
            throw new NotImplementedException();
        }
    }
}
