//-----------------------------------------------------------------------
// <copyright file="MSMQPersist.cs" company="Microsoft">
//     Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>the implementation of MSMQ storage provider.</summary>
//-----------------------------------------------------------------------
#if MSMQ
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage.MSMQ
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Messaging;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.AADAuthUtil;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using Microsoft.Hpc.Scheduler.Session.Utility;

    /// <summary>
    /// the implementation of MSMQ storage provider.
    /// </summary>
    public class MSMQPersist : ISessionPersist
    {
        #region private fields
        /// <summary>the prefix of the queue path</summary>
        private const string QueuePathPrefix = "PRIVATE$\\HPC";

        /// <summary>the prefix for generating queue path on local computer</summary>
        private const string LocalQueuePathPrefix = ".\\";

        /// <summary>delimeter for generating queue name</summary>
        private const string QueueNameFieldDelimeter = "-";

        /// <summary>the prefix of the request queue name</summary>
        private const string RequestQueueSuffix = "REQUESTS";

        /// <summary>the prefix of the response queue name</summary>
        private const string ResponseQueueSuffix = "RESPONSES";

        /// <summary>queue owner name for "anonymous" user</summary>
        private const string AnonymousOwner = "Everyone";
        
        /// <summary>queue owner name for "anonymous" user</summary>
        private const string SystemOwner = "System";

        /// <summary>the prefix for persist version label</summary>
        private const string VersionLabel = "VERSION";

        /// <summary>the prefix for EOM flag label</summary>
        private const string EOMLabel = "EOM";

        /// <summary>
        /// the regex to match the queue name
        /// </summary>
        private static readonly Regex QueueNameRegex = new Regex(@"PRIVATE\$\\HPC(?<SessionId>-?\d+)-(?<ClientId>.*)-(?<Suffix>(REQUESTS)|(RESPONSES))$", RegexOptions.IgnoreCase);

        /// <summary>
        /// the regex to match the cert identity user name
        /// </summary>
        private static readonly Regex CertUserNameRegex = new Regex(@"CN=[\w\s]*;\ [0-9A-F]+", RegexOptions.IgnoreCase);

        /// <summary>the binary message formattoer</summary>
        private static BinaryMessageFormatter binFormatterField = new BinaryMessageFormatter();

        /// <summary>the broker node name.  For HA cluster, it is the cluster virtual name.</summary>
        private static string BrokerNodeName = BrokerIdentity.GetBrokerName();

        /// <summary>flag indicating if all requests have been received.</summary>
        private bool EOMFlag;

        /// <summary>a value indicating whether this is a new created MSMQ persistence.</summary>
        private bool isNewCreatePersistField;

        /// <summary>a value indicating whether the MSMQ persistence is closed.</summary>
        private volatile bool isClosedField;

        /// <summary>the message queue that store the request messages.</summary>
        private MessageQueue requestsQueueField;

        /// <summary>the native message queue api wrapper for request queue.</summary>
        private MessageQueueNative nativeRequestsQueueField;

        /// <summary>the message queue that store the response messages.</summary>
        private MessageQueue responsesQueueField;

        /// <summary>the native message queue api wrapper for response queue.</summary>
        private MessageQueueNative nativeResponsesQueueField;

        /// <summary>the transaction.</summary>
        private MessageQueueNativeTransaction persistRequestsTransactionField;

        /// <summary>the total requests count in the request queue.</summary>
        private long requestsCountField;

        /// <summary>the total request count.</summary>
        private long allRequestsCountField;

        /// <summary>number of requests that are sent to MSMQ but not committed yet. </summary>
        private long uncommittedRequestsCountField;

        /// <summary>the total responses count in the queue.</summary>
        private long responsesCountField;

        /// <summary>Gets the number of the requests that get the fault responses.</summary>
        private long failedRequestsCountField;

        /// <summary>the session id.</summary>
        private int sessionIdField;

        /// <summary>the client id.</summary>
        private string clientIdField;

        /// <summary>the user name.</summary>
        private string userNameField;

        /// <summary>persist version of this MSMQ queue</summary>
        private int persistVersion;

        /// <summary>a value indicating whether this persist is disposed.</summary>
        private bool isDisposedField;
        
        /// <summary>request message fetcher</summary>
        MSMQMessageFetcher requestFetcher;

        /// <summary>response message fetcher</summary>
        MSMQMessageFetcher responseFetcher;

        /// <summary>
        /// BrokerWorker TelepathyContext
        /// </summary>
        private static ITelepathyContext TelepathyContext => WinServiceHpcContextModule.GetOrAddWinServiceHpcContextFromEnv();

        /// <summary>
        /// Cache SID name mapping of AAD identity
        /// </summary>
        private static readonly Dictionary<string, string> AadSidDict = new Dictionary<string, string>();
        private static readonly object AadSidLock = new object();

        #endregion

        /// <summary>
        /// Initializes a new instance of the MSMQPersist class.
        /// </summary>
        /// <param name="sessionId">the session id.</param>
        /// <param name="clientId">the client id.</param>
        internal MSMQPersist(string userName, int sessionId, string clientId)
        {
            BrokerTracing.TraceVerbose("[MSMQPersist] .MSMQPersist: constructor. session id = {0}, client id = {1}, user name = {2}", sessionId, clientId, userName);
            Debug.Write($"[MSMQPersist].MSMQPersist: {Environment.NewLine}{Environment.StackTrace}");
            Debug.Write($"[MSMQPersist].MSMQPersist: Current principal: {Thread.CurrentPrincipal.Identity.Name}");

            this.sessionIdField = sessionId;
            this.clientIdField = clientId;

            // if the EnableConnectionCache is set to false, the message queue will cancel the pending IO when it is disposed.
            MessageQueue.EnableConnectionCache = false;
            string requestsQueueName = MakeRequestQueuePath(sessionId, clientId);
            string responseQueueName = MakeResponseQueuePath(sessionId, clientId);

            this.isNewCreatePersistField = true;
            try
            {
                bool requestQueueExist = MessageQueue.Exists(requestsQueueName);
                bool responseQueueExist = MessageQueue.Exists(responseQueueName);

                if (requestQueueExist && responseQueueExist)
                {
                    this.isNewCreatePersistField = false;
                }
                else if (requestQueueExist != responseQueueExist)
                {
                    // If there is only request queue but not response queue, it could be caused by:
                    //  a. Queue creation operation interrupted
                    //  b. Queue deletion operation interrupted
                    if (requestQueueExist)
                    {
                        BrokerTracing.TraceError("[MSMQPersist] .MSMQPersist: queue data not integrety.  Fix it by deleting queue = {0}", requestsQueueName);
                        MessageQueueNative.Delete(requestsQueueName);
                    }
                    else
                    {
                        // There is only response queue but no request queue - this should rarely happen, as we always 
                        // create request queue before response queue, and delete request queue after response queue.
                        BrokerTracing.TraceError("[MSMQPersist] .MSMQPersist: queue data not integrety.  Fix it by deleting queue = {0}", responseQueueName);
                        MessageQueueNative.Delete(responseQueueName);
                    }
                }
            }
            catch (MessageQueueException e)
            {
                BrokerTracing.TraceError("[MSMQPersist] .MSMQPersist: MessageQueue.Exists raised exception, exception: {0}", e);
                throw MessageQueueHelper.ConvertMessageQueueException(e);
            }
            catch (MessageQueueNativeException e)
            {
                BrokerTracing.TraceError("[MSMQPersist] .MSMQPersist: MessageQueueNative.Delete raised exception, exception: {0}", e);
                throw MessageQueueHelper.ConvertMessageQueueException(e);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[MSMQPersist] .MSMQPersist: MessageQueue.Exists raised exception, exception: {0}", e);
                throw new BrokerQueueException((int)BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_FAIL, e.ToString());
            }

            Action setUserName = () =>
                {
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var principal = Thread.CurrentPrincipal;
                        if (principal.IsHpcAadPrincipal(TelepathyContext))
                        {
                            this.userNameField = this.SetUserName(principal);
                        }
                        else
                        {
                            this.userNameField = this.SetUserName(userName);
                        }
                    }
                };
            
            if (this.isNewCreatePersistField)
            {
                try
                {
                    BrokerTracing.TraceInfo("[MSMQPersist] .MSMQPersist: creating native requests queue {0}", requestsQueueName);
                    this.nativeRequestsQueueField = MessageQueueNative.Create(requestsQueueName, true);
                    BrokerTracing.TraceInfo("[MSMQPersist] .MSMQPersist: creating message requests queue {0}", requestsQueueName);
                    this.requestsQueueField = new MessageQueue(requestsQueueName);
                    this.requestsQueueField.SetPermissions("Administrators", MessageQueueAccessRights.FullControl);
                    this.requestsQueueField.SetPermissions(BrokerNodeName, MessageQueueAccessRights.FullControl);
                    this.requestsQueueField.MessageReadPropertyFilter.AppSpecific = true;
                    this.requestsQueueField.MessageReadPropertyFilter.Label = true;

                    BrokerTracing.TraceInfo("[MSMQPersist] .MSMQPersist: creating native responses queue {0}", requestsQueueName);
                    this.nativeResponsesQueueField = MessageQueueNative.Create(responseQueueName, true);
                    BrokerTracing.TraceInfo("[MSMQPersist] .MSMQPersist: creating message responses queue {0}", requestsQueueName);
                    this.responsesQueueField = new MessageQueue(responseQueueName);
                    this.responsesQueueField.SetPermissions("Administrators", MessageQueueAccessRights.FullControl);
                    this.responsesQueueField.SetPermissions(BrokerNodeName, MessageQueueAccessRights.FullControl);
                    this.responsesQueueField.MessageReadPropertyFilter.AppSpecific = true;
                    this.responsesQueueField.MessageReadPropertyFilter.Label = true;

                    setUserName();

                    this.persistVersion = BrokerQueueItem.PersistVersion;
                    this.EOMFlag = false;
                    this.requestsQueueField.Label = this.FormatRequestQueueLabel();
                    BrokerTracing.TraceVerbose("[MSMQPersist] .MSMQPersist: set persist version = {0}", BrokerQueueItem.PersistVersion);

                    // On queue creation completion, mark response queue label with "0", which means failed response count = 0.
                    // Note: label response queue is used as a flag indicating queue creation is completed. So it must be the last step of queue creation.
                    this.responsesQueueField.Label = "0";
                }
                catch (MessageQueueNativeException e)
                {
                    BrokerTracing.TraceError(
                            "[MSMQPersist] .MSMQPersist: failed to create MSMQ queue, {0}, exception: {1}.",
                            responseQueueName,
                            e);

                    // delete request queue after response queue
                    if (this.responsesQueueField != null)
                    {
                        this.responsesQueueField.Close();
                        MessageQueueNative.Delete(responseQueueName);
                    }

                    if (this.requestsQueueField != null)
                    {
                        this.requestsQueueField.Close();
                        MessageQueueNative.Delete(requestsQueueName);
                    }

                    throw MessageQueueHelper.ConvertMessageQueueException(e);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                            "[MSMQPersist] .MSMQPersist: failed to create MSMQ queue, {0}, exception: {1}.",
                            responseQueueName,
                            e);

                    // delete request queue after response queue.
                    if (this.responsesQueueField != null)
                    {
                        this.responsesQueueField.Close();
                        MessageQueueNative.Delete(responseQueueName);
                    }

                    if (this.requestsQueueField != null)
                    {
                        this.requestsQueueField.Close();
                        MessageQueueNative.Delete(requestsQueueName);
                    }

                    throw new BrokerQueueException((int)BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_FAIL, e.ToString());
                }
            }
            else
            {
                this.requestsQueueField = new MessageQueue(requestsQueueName);
                this.requestsQueueField.MessageReadPropertyFilter.AppSpecific = true;
                this.requestsQueueField.MessageReadPropertyFilter.Label = true;

                this.responsesQueueField = new MessageQueue(responseQueueName);
                this.responsesQueueField.MessageReadPropertyFilter.AppSpecific = true;
                this.responsesQueueField.MessageReadPropertyFilter.Label = true;

                this.nativeRequestsQueueField = new MessageQueueNative(requestsQueueName);
                this.nativeResponsesQueueField = new MessageQueueNative(responseQueueName);

                try
                {
                    this.requestsCountField = MessageQueueNative.GetMessageCount(requestsQueueName);
                    this.responsesCountField = MessageQueueNative.GetMessageCount(responseQueueName);
                    this.allRequestsCountField = this.requestsCountField + this.responsesCountField;

                    this.persistVersion = BrokerVersion.DefaultPersistVersion;
                    // Default EOMFlag to true if there are requests in queue.
                    this.EOMFlag = (this.allRequestsCountField > 0);
                    MSMQPersist.ParseRequestQueueLabel(this.requestsQueueField.Label, ref this.persistVersion, ref this.EOMFlag);

                    BrokerTracing.TraceVerbose("[MSMQPersist] .MSMQPersist: retrieved persist version = {0}", this.persistVersion);
                    Debug.Assert(persistVersion == BrokerQueueItem.PersistVersion,
                        String.Format("Persist version mismatch: version from persist queue = {0}, version from job customized property = {1}", persistVersion, BrokerQueueItem.PersistVersion));

                    string responseQueueLabel = this.responsesQueueField.Label;
                    MSMQPersist.ParseResponseQueueLabel(responseQueueLabel, out this.failedRequestsCountField);

                    // Response queue label should never be null or empty.  If response queue label is null or empty, it is likely 
                    // due to previous queue creation is not fully completed. In this case, we will complete all creation steps and 
                    // take owner of the queue.
                    if (!string.IsNullOrEmpty(responseQueueLabel))
                    {
                        this.userNameField = GetUserName();
                    }
                    else
                    {
                        // if response queue label is null, there should be no request or response in queue.
                        if (this.allRequestsCountField != 0)
                        {
                            throw new BrokerQueueException((int)BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_FAIL, String.Format("broker queue data not integrity: uninitialzied queue contains message. queue name = {0}", responseQueueName));
                        }

                        this.requestsQueueField.SetPermissions("Administrators", MessageQueueAccessRights.FullControl);
                        this.requestsQueueField.SetPermissions(BrokerNodeName, MessageQueueAccessRights.FullControl);
                        this.requestsQueueField.MessageReadPropertyFilter.AppSpecific = true;
                        this.requestsQueueField.MessageReadPropertyFilter.Label = true;

                        this.responsesQueueField.SetPermissions("Administrators", MessageQueueAccessRights.FullControl);
                        this.responsesQueueField.SetPermissions(BrokerNodeName, MessageQueueAccessRights.FullControl);
                        this.responsesQueueField.MessageReadPropertyFilter.AppSpecific = true;
                        this.responsesQueueField.MessageReadPropertyFilter.Label = true;

                        setUserName();

                        this.persistVersion = BrokerQueueItem.PersistVersion;
                        this.EOMFlag = false;
                        this.requestsQueueField.Label = this.FormatRequestQueueLabel();
                        this.responsesQueueField.Label = "0";
                    }
                }
                catch (MessageQueueNativeException e)
                {
                    BrokerTracing.TraceError(
                            "[MSMQPersist] .MSMQPersist: failed to access MSMQ queue, {0}, exception: {1}.",
                            responseQueueName,
                            e);

                    throw MessageQueueHelper.ConvertMessageQueueException(e);
                }
                catch (MessageQueueException e)
                {
                    BrokerTracing.TraceError(
                         "[MSMQPersist] .MSMQPersist: failed to access MSMQ queue, {0}, exception: {1}.",
                         responseQueueName,
                         e);

                    throw MessageQueueHelper.ConvertMessageQueueException(e);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError(
                        "[MSMQPersist] .MSMQPersist: failed to access MSMQ queue, {0}, exception: {1}.",
                        responseQueueName,
                        e);

                    throw new BrokerQueueException((int)BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_FAIL, e.ToString());
                }
            }

            // Init transaction for persisting requests
            this.persistRequestsTransactionField = new MessageQueueNativeTransaction();
            this.persistRequestsTransactionField.Begin();
            BrokerTracing.TraceVerbose("[MSMQPersist] .MSMQPersist: MSMQ Transactions Enabled.");

            this.requestFetcher = new MSMQMessageFetcher(this.requestsQueueField, this.requestsCountField, binFormatterField);
            this.responseFetcher = new MSMQMessageFetcher(this.responsesQueueField, this.responsesCountField, binFormatterField);
        }

        /// <summary>
        /// Finalizes an instance of the MSMQPersist class.
        /// </summary>
        ~MSMQPersist()
        {
            this.Dispose(false);
        }

        #region public properties

        /// <summary>
        /// Gets the number of requests in the persistence
        /// </summary>
        public long RequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.requestsCountField);
            }
        }

        /// <summary>
        /// Gets the number of the current requests in the persistence
        /// </summary>
        public long AllRequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.allRequestsCountField);
            }
        }

        /// <summary>
        /// Gets the number of the current responses in the persistence
        /// </summary>
        public long ResponsesCount
        {
            get
            {
                return Interlocked.Read(ref this.responsesCountField);
            }
        }

        /// <summary>
        /// Gets the number of the requests that get the fault responses.
        /// </summary>
        public long FailedRequestsCount
        {
            get
            {
                return Interlocked.Read(ref this.failedRequestsCountField);
            }
        }

        /// <summary>
        /// Gets the user name of the broker queue.
        /// </summary>
        public string UserName
        {
            get
            {
                return this.userNameField;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is a new created storage.
        /// </summary>
        public bool IsNewCreated
        {
            get
            {
                return this.isNewCreatePersistField;
            }
        }

        /// <summary>
        /// Gets a value indicating whether EOM is received.
        /// </summary>
        public bool EOMReceived
        {
            get
            {
                return this.EOMFlag;
            }
            set
            {
                this.EOMFlag = true;
                this.requestsQueueField.Label = this.FormatRequestQueueLabel();
            }
        }
        #endregion

        /// <summary>
        /// Gets the status of the requests transaction.
        /// </summary>
        internal MessageQueueTransactionStatus RequestsTransactionStatus
        {
            get
            {
                MessageQueueTransactionStatus status = MessageQueueTransactionStatus.Aborted;
                if (this.persistRequestsTransactionField != null)
                {
                    status = this.persistRequestsTransactionField.Status;
                }

                return status;
            }
        }

        /// <summary>
        /// create a put request transaction
        /// </summary>
        /// <returns>return the persist transaction object.</returns>
        public IPersistTransaction GetPutRequestTransaction()
        {
            return new MSMQPersistTransaction(this);
        }

        /// <summary>
        /// Put the request item objects into the storage.
        /// </summary>
        /// <param name="requests">A list of request objects</param>
        /// <param name="putRequestCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        /// <remarks>This operation should aware the TransactionContext. 
        /// If the context is avariable, nothing should be changed if one of 
        /// operations failed.</remarks>
        public void PutRequestsAsync(IEnumerable<BrokerQueueItem> requests, PutRequestCallback putRequestCallback, object callbackState)
        {
            ParamCheckUtility.ThrowIfNull(requests, "requests");
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[MSMQPersist] .PutRequestsAsync: the persist queue is disposed.");
                return;
            }

            PutRequestState putRequestState = new PutRequestState(requests, putRequestCallback, callbackState);

            //TODO: make PutRequestsAsync an async call
            //ThreadPool.QueueUserWorkItem(this.PersistRequestsThreadProc, persistState);
            this.PersistRequestsThreadProc(putRequestState);
        }

        /// <summary>
        /// Put a single request item into the storage.
        /// </summary>
        /// <param name="request">the single request that need be stored to the persistenc</param>
        /// <param name="putRequestCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        /// <remarks>This operation should aware the TransactionContext. 
        /// If the context is avariable, nothing should be changed if one of 
        /// operations failed.</remarks>
        public void PutRequestAsync(BrokerQueueItem request, PutRequestCallback putRequestCallback, object callbackState)
        {
            ParamCheckUtility.ThrowIfNull(request, "request");
            BrokerQueueItem[] requests = new BrokerQueueItem[1];
            requests[0] = request;
            this.PutRequestsAsync(requests, putRequestCallback, callbackState);
        }

        /// <summary>
        /// Fetch the requests one by one from the storage but not remove the original message in the storage.
        /// </summary>
        /// <param name="callback">the callback to get the async result</param>
        /// <param name="state">the state object for the callback</param>
        public void GetRequestAsync(PersistCallback callback, object state)
        {
            ParamCheckUtility.ThrowIfNull(callback, "callback");
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[MSMQPersist] .GetRequestAsync: the persist queue is disposed.");
                return;
            }

            BrokerTracing.TraceVerbose("[MSMQPersist] .GetRequestAsync: Get request come in.");

            this.requestFetcher.GetMessageAsync(callback, state);
        }


        /// <summary>
        /// Put the response item objects into the storage.
        /// </summary>
        /// <param name="responses">A list of response objects</param>
        /// <param name="putResponseCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        public void PutResponsesAsync(IEnumerable<BrokerQueueItem> responses, PutResponseCallback putResponseCallback, object callbackState)
        {
            ParamCheckUtility.ThrowIfNull(responses, "responses");
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[MSMQPersist] .PutResponsesAsync: the persist queue is disposed.");
                return;
            }

            BrokerTracing.TraceVerbose("[MSMQPersist] .PutResponsesAsync: new responses come in. Response count: {0}", (int)callbackState);
            PutResponseState putResponseState = new PutResponseState(responses, putResponseCallback, callbackState);

            // Enter MSMQ concurrency control section
            MSMQConcurrencyController.Enter(this.PersistResponsesThreadProc, putResponseState);
        }

        /// <summary>
        /// Put on response item into persistence
        /// </summary>
        /// <param name="response">the response item to be persisted</param>
        /// <param name="putResponseCallback">the callback function that will be called once the async operation finish or exception raise.</param>
        /// <param name="callbackState">the state object for the callback.</param>
        public void PutResponseAsync(BrokerQueueItem response, PutResponseCallback putResponseCallback, object callbackState)
        {
            BrokerQueueItem[] responses = new BrokerQueueItem[1];
            responses[0] = response;
            this.PutResponsesAsync(responses, putResponseCallback, callbackState);
        }

        /// <summary>
        /// get a response from the storage.
        /// </summary>
        /// <param name="callback">the response callback, the async result should be the BrokerQueueItem</param>
        /// <param name="callbackState">the state object for the callback</param>
        public void GetResponseAsync(PersistCallback callback, object callbackState)
        {
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[MSMQPersist] .GetResponseAsync: the persist queue is disposed.");
                return;
            }

            BrokerTracing.TraceVerbose("[MSMQPersist] .GetResponseAsync: Get response come in.");

            this.responseFetcher.GetMessageAsync(callback, callbackState);
        }

        /// <summary>
        /// reset the current response callback, after this method call, 
        /// the registered RegisterResponseCallback will get the responses from the beginning.
        /// </summary>
        public void ResetResponsesCallback()
        {
            if (this.isDisposedField)
            {
                BrokerTracing.TraceWarning("[MSMQPersist] .ResetResponsesCallback: the persist queue is disposed.");
                return;
            }

            BrokerTracing.TraceVerbose("[MSMQPersist] .ResetResponsesCallback: begin.");

            // reset response MSMQMessageFetcher
            this.responseFetcher.SafeDispose();
            this.responseFetcher = new MSMQMessageFetcher(this.responsesQueueField, this.responsesCountField, binFormatterField);
        }

        /// <summary>
        /// acknowledge that a response message is dispatched, either successfully or not
        /// </summary>
        /// <param name="responseItem">response message being dispatched</param>
        /// <param name="success">if dispatching is success or not</param>
        public void AckResponse(BrokerQueueItem responseMessage, bool success)
        {
            // For durable session, need do nothing to the response message in msmq.  Just dispose the in-memeory copy of response message
            responseMessage.Dispose();
        }

        /// <summary>
        /// IDisposable method
        /// </summary>
        public void Dispose()
        {
            BrokerTracing.TraceVerbose("[MSMQPersist] .Dispose: Dispose the MSMQPersist, this.isDisposedField = {0}", this.isDisposedField);
            if (!this.isDisposedField)
            {
                this.isDisposedField = true;
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// remove the storage.
        /// </summary>
        public SessionPersistCounter Close()
        {
            BrokerTracing.TraceVerbose("[MSMQPersist] .Close: Close the MSMQPersist.");
            if (!this.isClosedField)
            {
                this.isClosedField = true;
                this.Dispose();

                // delete response queue before request queue.
                MessageQueueNative.Delete(MSMQPersist.MakeResponseQueuePath(this.sessionIdField, this.clientIdField));
                MessageQueueNative.Delete(MSMQPersist.MakeRequestQueuePath(this.sessionIdField, this.clientIdField));
            }

            SessionPersistCounter counter = new SessionPersistCounter();
            counter.ResponsesCountField = Interlocked.Read(ref this.responsesCountField);
            counter.FailedRequestsCountField = Interlocked.Read(ref this.failedRequestsCountField);
            return counter;
        }

        /// <summary>
        /// Gets the total requests queue length and the total responses queue length on this node
        /// </summary>
        /// <param name="totalRequestQueueLength">output the number of total requests queue length</param>
        /// <param name="totalResponseQueueLength">output the number of total responses queue length</param>
        public static void GetAllSessionCounts(out long totalRequestQueueLength, out long totalResponseQueueLength)
        {
            totalRequestQueueLength = 0;
            totalResponseQueueLength = 0;

            try
            {
                String[] privateQueueNames = MessageQueueNative.GetPrivateQueues();
                if (privateQueueNames != null && privateQueueNames.Length > 0)
                {
                    foreach (String queueName in privateQueueNames)
                    {
                        Match m = QueueNameRegex.Match(queueName);
                        if (!m.Success)
                        {
                            continue;
                        }

                        bool isRequestQueue = String.Compare(m.Groups["Suffix"].Value, RequestQueueSuffix, StringComparison.InvariantCultureIgnoreCase) == 0;
                        uint messageCount = (uint)MessageQueueNative.GetMessageCount(queueName);
                        if (isRequestQueue)
                        {
                            totalRequestQueueLength += messageCount;
                        }
                        else
                        {
                            totalResponseQueueLength += messageCount;
                        }
                    }
                }
            }
            catch (MessageQueueNativeException e)
            {
                BrokerTracing.TraceWarning("[MSMQPersist] .GetAllSessionCounts: fail to enumerate the message queues, Exception: {0}", e);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("[MSMQPersist] .GetAllSessionCounts: fail to enumerate the message queues, Exception: {0}", e);
            }
        }

        /// <summary>
        /// get all the clients id under the specific session.
        /// </summary>
        /// <param name="sessionId">the session id.</param>
        /// <returns>the clients id collection.</returns>
        internal static ClientInfo[] GetSessionClients(int sessionId, bool useAad)
        {
            // Client id is case insensitive
            Dictionary<string, ClientInfo> clientIdDic = new Dictionary<string, ClientInfo>(StringComparer.OrdinalIgnoreCase);
            try
            {
                String[] privateQueueNames = MessageQueueNative.GetPrivateQueues();
                if (privateQueueNames != null && privateQueueNames.Length > 0)
                {
                    foreach (String queueName in privateQueueNames)
                    {
                        int queueSessionId;
                        string queueClientId;
                        bool isRequestQueue;

                        if (!MSMQPersist.TryParseQueuePath(queueName, out queueSessionId, out queueClientId, out isRequestQueue))
                        {
                            continue;
                        }

                        if (queueSessionId != sessionId)
                        {
                            continue;
                        }

                        MessageQueue queue = new MessageQueue(queueName);
                        if (isRequestQueue)
                        {
                            int requestCount = MessageQueueNative.GetMessageCount(queueName);

                            ClientInfo info;
                            if (clientIdDic.TryGetValue(queueClientId, out info))
                            {
                                info.TotalRequestsCount += requestCount;
                            }
                            else
                            {
                                clientIdDic.Add(queueClientId, new ClientInfo(queueClientId, requestCount, 0));
                            }
                        }
                        else
                        {
                            string userName = GetUserName(queueName, useAad);
                            int responseCount = MessageQueueNative.GetMessageCount(queueName);

                            long failedCount;
                            ParseResponseQueueLabel(queue.Label, out failedCount);

                            ClientInfo info;
                            if (clientIdDic.TryGetValue(queueClientId, out info))
                            {
                                info.ProcessedRequestsCount = responseCount;
                                info.TotalRequestsCount += responseCount;
                                info.FailedRequestsCount = failedCount;
                                info.UserName = userName;
                            }
                            else
                            {
                                info = new ClientInfo(queueClientId, failedCount, userName);
                                info.ProcessedRequestsCount = responseCount;
                                info.TotalRequestsCount = responseCount;
                                clientIdDic.Add(queueClientId, info);
                            }
                        }
                    }
                }
            }
            catch (MessageQueueNativeException e)
            {
                BrokerTracing.TraceWarning("[MSMQPersist] .GetSessionClients: fail to enumerate the message queues, Exception: {0}", e);
                throw MessageQueueHelper.ConvertMessageQueueException(e);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("[MSMQPersist] .GetSessionClients: fail to enumerate the message queues, Exception: {0}", e);
                throw new BrokerQueueException((int)BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_FAIL, e.ToString());
            }

            ClientInfo[] clients = new ClientInfo[clientIdDic.Keys.Count];
            clientIdDic.Values.CopyTo(clients, 0);
            return clients;
        }

        /// <summary>
        /// cleanup the stale message queue.
        /// </summary>
        /// <param name="isStaleSessionCallback">the callback function to judge whether the session is stale.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static async Task CleanupStaleMessageQueue(IsStaleSessionCallback isStaleSessionCallback)
        {
            ParamCheckUtility.ThrowIfNull(isStaleSessionCallback, "isStaleSessionCallback");
            String[] privateQueueNames = MessageQueueNative.GetPrivateQueues();
            List<string> staleQueueNameList = new List<string>();
            Dictionary<int, bool> sessionIdStaleDic = new Dictionary<int, bool>();
            if (privateQueueNames != null && privateQueueNames.Length > 0)
            {
                foreach (String queueName in privateQueueNames)
                {
                    int queueSessionId;
                    string queueClientId;
                    bool isRequestQueue;

                    if (!MSMQPersist.TryParseQueuePath(queueName, out queueSessionId, out queueClientId, out isRequestQueue))
                    {
                        continue;
                    }

                    bool isStaleSession = false;
                    if (!sessionIdStaleDic.TryGetValue(queueSessionId, out isStaleSession))
                    {
                        isStaleSession = await isStaleSessionCallback(queueSessionId);
                        sessionIdStaleDic.Add(queueSessionId, isStaleSession);
                    }

                    if (isStaleSession)
                    {
                        BrokerTracing.TraceWarning("[MSMQPersist] .CleanupStaleMessageQueue: stale message queue detected. {0}", queueName);
                        staleQueueNameList.Add(queueName);
                    }
                }

                for (int i = 0; i < staleQueueNameList.Count; i++)
                {
                    try
                    {
                        MessageQueueNative.Delete(staleQueueNameList[i]);
                        BrokerTracing.TraceInfo("[MSMQPersist] .CleanupStaleMessageQueue: stale message queue '{0}' deleted", staleQueueNameList[i]);
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceWarning("[MSMQPersist] .CleanupStaleMessageQueue: fail to delete the message queue, {0}, Exception: {1}", staleQueueNameList[i], e);
                    }
                }
            }
        }

        internal static void SetAadSidEntry(string sid, string name)
        {
            lock (AadSidLock)
            {
                AadSidDict[sid] = name;
            }
        }

        /// <summary>
        /// commit the put requests transaction
        /// </summary>
        internal void CommitRequestTransaction()
        {
            this.ResetRequestsTransaction(true);
        }

        /// <summary>
        /// abort the requests transaction
        /// </summary>
        internal void AbortRequestTransaction()
        {
            this.ResetRequestsTransaction(false);
        }

        /// <summary>
        /// dispose the resources.
        /// </summary>
        /// <param name="disposing">a value indicating dispose the inner objects.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213", MessageId = "requestsQueueField", Justification = "requestsQueueField has been disposed by SafeDisposeMessageQueue.")]
        [SuppressMessage("Microsoft.Usage", "CA2213", MessageId = "responsesQueueField", Justification = "responsesQueueField has been disposed by SafeDisposeMessageQueue.")]
        [SuppressMessage("Microsoft.Usage", "CA2213", MessageId = "nativeRequestsQueueField", Justification = "nativeRequestsQueueField has been disposed by SafeDisposeObject.")]
        [SuppressMessage("Microsoft.Usage", "CA2213", MessageId = "nativeResponsesQueueField", Justification = "nativeResponsesQueueField has been disposed by SafeDisposeObject.")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.persistRequestsTransactionField != null)
                {
                    if (this.persistRequestsTransactionField.Status == MessageQueueTransactionStatus.Pending)
                    {
                        this.persistRequestsTransactionField.Abort();
                    }

                    this.persistRequestsTransactionField.Dispose();
                    this.persistRequestsTransactionField = null;
                }

                // dispose request/response fetcher before dispoing request/response queues
                // to stop fetching request/response from queues.
                if (this.requestFetcher != null)
                {
                    this.requestFetcher.SafeDispose();
                    this.requestFetcher = null;
                }

                if (this.responseFetcher != null)
                {
                    this.responseFetcher.SafeDispose();
                    this.responseFetcher = null;
                }

                if (this.requestsQueueField != null)
                {
                    MSMQPersist.SafeDisposeMessageQueue(this.requestsQueueField);
                    this.requestsQueueField = null;
                }

                if (this.responsesQueueField != null)
                {
                    MSMQPersist.SafeDisposeMessageQueue(this.responsesQueueField);
                    this.responsesQueueField = null;
                }

                CSharpUsageUtility.SafeDisposeObject(ref this.nativeRequestsQueueField);
                CSharpUsageUtility.SafeDisposeObject(ref this.nativeResponsesQueueField);
            }
        }

        /*
        /// <summary>
        /// helper function to get messasge queue.
        /// </summary>
        /// <param name="queueName">the queue name.</param>
        /// <param name="needLookupId">a value indicating whether need lookup id.</param>
        /// <returns>the message queue instance.</returns>
        private static MessageQueue GetMessageQueue(string queueName, bool needLookupId)
        {
            MessageQueue queue = null;
            try
            {
                queue = new MessageQueue(queueName);
                
                queue.DefaultPropertiesToSend.Recoverable = true;
                queue.MessageReadPropertyFilter.Recoverable = true;
                if (needLookupId)
                {
                    queue.MessageReadPropertyFilter.LookupId = true;
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError(
                        "[MSMQPersist] .MSMQPersist: failed to create MSMQ queue, {0}, exception: {1}.",
                        queueName,
                        e.ToString());

                throw;
            }

            return queue;
        }*/

        /// <summary>
        /// make request queue path
        /// </summary>
        /// <param name="sessionId">the session id.</param>
        /// <param name="clientId">the client id.</param>
        /// <returns>the request queue path.</returns>
        private static string MakeRequestQueuePath(int sessionId, string clientId)
        {
            return MSMQPersist.LocalQueuePathPrefix + MSMQPersist.QueuePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) +
                MSMQPersist.QueueNameFieldDelimeter + clientId + MSMQPersist.QueueNameFieldDelimeter + MSMQPersist.RequestQueueSuffix;
        }

        /// <summary>
        /// make reqponse queue path.
        /// </summary>
        /// <param name="sessionId">the session id.</param>
        /// <param name="clientId">the client id.</param>
        /// <returns>the response queue path.</returns>
        private static string MakeResponseQueuePath(int sessionId, string clientId)
        {
            return MSMQPersist.LocalQueuePathPrefix + MSMQPersist.QueuePathPrefix + sessionId.ToString(CultureInfo.InvariantCulture) +
                MSMQPersist.QueueNameFieldDelimeter + clientId + MSMQPersist.QueueNameFieldDelimeter + MSMQPersist.ResponseQueueSuffix;
        }

        /// <summary>
        /// Parse a queue path
        /// </summary>
        /// <param name="queuePath">the queue path.</param>
        /// <param name="sessionId">id of the session that the queue corresponds.</param>
        /// <param name="clientId">id of the session client that the queue corresponds.</param>
        /// <param name="isRequestQueue">a flat indicating if this is a request queue or response queue. </param>
        /// <returns>a flag indicating if the parsing succeeds. </returns>
        private static bool TryParseQueuePath(string queuePath, out int sessionId, out string clientId, out bool isRequestQueue)
        {
            Match m = QueueNameRegex.Match(queuePath);
            if (!m.Success)
            {
                sessionId = 0;
                clientId = String.Empty;
                isRequestQueue = false;
                return false;
            }

            sessionId = Convert.ToInt32(m.Groups["SessionId"].Value);
            isRequestQueue = String.Compare(m.Groups["Suffix"].Value, RequestQueueSuffix, StringComparison.InvariantCultureIgnoreCase) == 0;
            clientId = m.Groups["ClientId"].Value;
            return true;
        }

        /// <summary>
        /// safe dispose a message queue.
        /// </summary>
        /// <param name="pendingIOCount">the current pending IO count.</param>
        /// <param name="stalePendingIOCount">the stale pending IO count.</param>
        /// <param name="messageQueue">the message queue that need dispose safely.</param>
        private static void SafeDisposeMessageQueue(MessageQueue messageQueue)
        {
            messageQueue.Dispose();
        }

        /// <summary>
        /// Format request queue label
        /// Note - request queue label format:
        ///   before v3: no request queue label
        ///   v3: VERSION=x
        ///   v3.1: VERSION=x;EOM=y
        /// </summary>        
        /// <returns>formatted request queue label</returns>
        private string FormatRequestQueueLabel()
        {
            return String.Format("{0}={1};{2}={3}", VersionLabel, this.persistVersion, EOMLabel, this.EOMFlag ? "1" : "0");
        }

        /// <summary>
        /// the helper function to parse request queue label
        /// </summary>
        /// <param name="requestQueueLabel">the request queue label</param>
        /// <param name="persistVersion">the BrokerQueueItem version</param>
        /// <param name="EOMFlag">the EOM flag</param>
        private static void ParseRequestQueueLabel(string requestQueueLabel, ref int persistVersion, ref bool EOMFlag)
        {
            if (string.IsNullOrEmpty(requestQueueLabel))
            {
                return;
            }

            string[] keyValueFields = requestQueueLabel.Split(';');
            foreach (string keyValueField in keyValueFields)
            {
                string[] keyValuePair = keyValueField.Split('=');
                if (keyValuePair.Length != 2)
                {
                    // skip
                    continue;
                }

                if (0 == string.Compare(keyValuePair[0], VersionLabel, /*ignoreCase=*/true))
                {
                    //Parse version label, "VERSION=x"
                    int.TryParse(keyValuePair[1], out persistVersion);
                }
                else if (0 == string.Compare(keyValuePair[0], EOMLabel, /*ignoreCase=*/true))
                {
                    //Parse EOM label, "EOM=x"
                    int eom = 0;
                    if (int.TryParse(keyValuePair[1], out eom) && eom != 0)
                    {
                        EOMFlag = true;
                    }
                    else
                    {
                        EOMFlag = false;
                    }
                }
            }
        }

        /// <summary>
        /// the helper function to parse the label of response queue
        /// </summary>
        /// <param name="responseQueueLabel">indicating the response queue label</param>
        /// <param name="failedReqeustsCount">output the failed requests count</param>
        private static void ParseResponseQueueLabel(string responseQueueLabel, out long failedRequestsCount)
        {
            if (string.IsNullOrEmpty(responseQueueLabel) || !long.TryParse(responseQueueLabel, out failedRequestsCount))
            {
                failedRequestsCount = 0;
            }
        }

        /// <summary>
        /// reset the requests transaction
        /// </summary>
        /// <param name="needCommit">a value indicates whether need commit the transaction before renew it.</param>
        private void ResetRequestsTransaction(bool needCommit)
        {
            if (this.persistRequestsTransactionField != null)
            {
                try
                {
                    if (this.persistRequestsTransactionField.Status == MessageQueueTransactionStatus.Pending)
                    {
                        if (needCommit)
                        {
                            this.persistRequestsTransactionField.Commit();
                            long committed = Interlocked.Exchange(ref this.uncommittedRequestsCountField, 0);
                            this.requestFetcher.NotifyMoreMessages(committed);
                        }
                        else
                        {
                            this.persistRequestsTransactionField.Abort();

                            // reset uncommittedRequestsCountField
                            long committed = Interlocked.Exchange(ref this.uncommittedRequestsCountField, 0);
                            Interlocked.Add(ref this.requestsCountField, -committed);
                        }
                    }
                }
                catch (MessageQueueException e)
                {
                    throw MessageQueueHelper.ConvertMessageQueueException(e);
                }
                finally
                {
                    this.persistRequestsTransactionField.Dispose();
                    this.persistRequestsTransactionField = new MessageQueueNativeTransaction();
                    this.persistRequestsTransactionField.Begin();
                }
            }
        }

        /// <summary>
        /// Set user name of this MSMSPersist instance
        /// </summary>
        /// <param name="userName"></param>
        private string SetUserName(string userName)
        {
            string ownerName = UserNameToOwner(userName);

            //Change response queue owner to userName
            this.nativeResponsesQueueField.SetOwner(ownerName, false);
            return userName;
        }

        private string SetUserName(IPrincipal principal)
        {
            var sid = principal.GenerateSecurityIdentifierFromAadPrincipal(TelepathyContext);
            SetAadSidEntry(sid.Value, principal.Identity.Name);
            Trace.TraceInformation("SetUserName: TrySetAADUserIdentity. SID:{0}, Identity:{1}", sid.Value, principal.Identity.Name);
            this.nativeResponsesQueueField.SetOwner(sid.Value, true);
            Debug.Assert(
                this.nativeResponsesQueueField.GetOwner(true).Equals(sid.Value, StringComparison.CurrentCultureIgnoreCase),
                $"{nameof(SetUserName)} to {nameof(this.nativeResponsesQueueField)} failed. Expected {sid.Value}, actual {this.nativeResponsesQueueField.GetOwner(true)}");
            return principal.Identity.Name;
        }

        /// <summary>
        /// Get user name of this MSMQPersist instance
        /// </summary>
        /// <returns>user name of the MSMQPersist instance. </returns>
        private string GetUserName()
        {
            var principal = Thread.CurrentPrincipal;

            // If AAD SID Dictionary is not empty, it has big chance that this MSMQ is authenticated by AAd identity.
            if (principal.IsHpcAadPrincipal(TelepathyContext) || AadSidDict.Any())
            {
                string strSid = this.nativeResponsesQueueField.GetOwner(true);
                string userName;
                if (AadSidDict.TryGetValue(strSid, out userName))
                {
                    Trace.TraceInformation("GetUserName: Got AAD user name from SID SID: {0}, username: {1}", strSid, userName);
                    return userName;
                }
                else
                {
                    Trace.TraceWarning(
                        "GetUserName: Get AAD user name from SID failed. SID: {0}, current identity: {1}, current SID: {2}",
                        strSid,
                        principal.Identity.Name,
                        principal.GenerateSecurityIdentifierFromAadPrincipal(TelepathyContext));
                }
            }
            string ownerName = this.nativeResponsesQueueField.GetOwner(false);
            return OwnerToUserName(ownerName);
        }

        /// <summary>
        /// Helper function for retrieving user name of a MSMQPersist instance directly through its underlying response queue name
        /// </summary>
        /// <param name="responseQueueName">response queue path. </param>
        /// <param name="useAad">whether use AAD integration</param>
        /// <returns>user name of the MSMQPersist instance that the response queue corresponds. </returns>
        private static string GetUserName(string responseQueueName, bool useAad)
        {
            MessageQueueNative nativeResponseQueue = new MessageQueueNative(responseQueueName);
            var principal = Thread.CurrentPrincipal;
            if (principal.IsHpcAadPrincipal(TelepathyContext) || useAad)
            {
                string strSid = nativeResponseQueue.GetOwner(true);
                string userName;
                if (AadSidDict.TryGetValue(strSid, out userName))
                {
                    Trace.TraceInformation(
                        "GetUserName: Got AAD user name from SID SID: {0}, username: {1}, responseQueueName: {2}",
                        strSid,
                        userName,
                        responseQueueName);

                    return userName;
                }
                else
                {
                    Trace.TraceWarning(
                        "GetUserName: Get AAD user name from SID failed. SID: {0}, current identity: {1}, current SID: {2}, responseQueueName: {3}",
                        strSid,
                        principal.Identity.Name,
                        principal.GenerateSecurityIdentifierFromAadPrincipal(TelepathyContext),
                        responseQueueName);
                }
            }

            return OwnerToUserName(nativeResponseQueue.GetOwner(false));
        }

        /// <summary>
        /// Map user name of MSMQPerist instance to name that will be set as owner of underlying MSMQ (response) queue.
        /// </summary>
        /// <param name="userName">User name of a MSMQPersit instance.</param>
        /// <returns>Owner name of corresponding MSMQ (response) queue. </returns>
        private static string UserNameToOwner(string userName)
        {
            // User name to owner name mapping rule: 
            // - if user name is Constant.AnonymousUserName("Anonymous"), owner name is set to "Everyone"; 
            // - or else, owner name is set to user name.
            if (string.Equals(userName, Constant.AnonymousUserName, StringComparison.OrdinalIgnoreCase))
            {
                return AnonymousOwner;
            }

            if (CertUserNameRegex.IsMatch(userName))
            {
                // TODO: Check cert identity here
                Trace.TraceInformation("UserNameToOwner: Current identity is X509 identity {0}. Return System owner.", userName);
                return SystemOwner;
            }

            return userName;
        }

        /// <summary>
        /// Map owner name of MSMQ (response) queue back to user name of MSMQPerist instance.
        /// </summary>
        /// <param name="ownerName">Owner name of MSMQ (response) queue.</param>
        /// <returns>User name of corresponding MSMQPersit instance. </returns>
        private static String OwnerToUserName(String ownerName)
        {
            if (string.Equals(ownerName, MSMQPersist.AnonymousOwner, StringComparison.OrdinalIgnoreCase))
            {
                return Constant.AnonymousUserName;
            }
            return ownerName;
        }

        /// <summary>
        /// thread pool callback to save the requests.
        /// </summary>
        /// <param name="state">the state object, PersistState.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void PersistRequestsThreadProc(object state)
        {
            PutRequestState putRequestState = (PutRequestState)state;
            ParamCheckUtility.ThrowIfNull(state, "put request state");
            ParamCheckUtility.ThrowIfNull(putRequestState.Messages, "to-be-persisted requests");

            Exception exception = null;
            long requestsCount = 0;

            foreach (BrokerQueueItem request in putRequestState.Messages)
            {
                ParamCheckUtility.ThrowIfNull(request, "to-be-persisted request");

                using (request)
                {
                    // check if the request size > 4MB, if yes try to persist it into several partial messages
                    System.Messaging.Message sendMsg = MessageQueueNative.PrepareMessage(request);
                    try
                    {
                        exception = null;
                        List<System.Messaging.Message> sendMsgs = new List<Message>();
                        if (sendMsg.BodyStream.Length > Constant.MSMQChunkSize)
                        {
                            BrokerTracing.TraceVerbose("[MSMQPersist] .PersistRequestsThreadProc: request size {0} is larger than MSMQ chunk size.", sendMsg.BodyStream.Length);
                            string messageGroupId = request.PersistId.ToString();
                            byte[] buffer = new byte[Constant.MSMQChunkSize];
                            int msgNumber = (int)Math.Ceiling(((decimal)sendMsg.BodyStream.Length) / Constant.MSMQChunkSize);
                            int n = 0;
                            int msgCount = 0;
                            sendMsg.BodyStream.Position = 0;
                            while ((n = sendMsg.BodyStream.Read(buffer, 0, Constant.MSMQChunkSize)) != 0)
                            {
                                Message msg = new Message();
                                msg.BodyStream.Write(buffer, 0, n);
                                msg.AppSpecific = msgNumber;
                                msgCount++;
                                msg.Label = string.Format("{0}_{1}", messageGroupId, msgCount);
                                msg.BodyType = sendMsg.BodyType;
                                sendMsgs.Add(msg);
                            }
                        }
                        else
                        {
                            sendMsgs.Add(sendMsg);
                        }

                        foreach (Message msg in sendMsgs)
                        {
                            MessageQueueHelper.PerformRetriableOperation(
                            delegate(int retryNumber)
                            {
                                this.nativeRequestsQueueField.Send(msg, this.persistRequestsTransactionField);
                            },
                            "[MSMQPersist] .PersistRequestsThreadProc: persist request raised exception");
                        }

                        requestsCount++;

                        BrokerTracing.TraceVerbose("[MSMQPersist] .PersistRequestsThreadProc: send message(s) for persist id {0}.", request.PersistId);
                    }
                    catch (MessageQueueNativeException e)
                    {
                        if (this.isDisposedField)
                        {
                            // if the queue is closed, then quit the method.
                            return;
                        }

                        BrokerTracing.TraceError("[MSMQPersist] .PersistRequestsThreadProc: persist request raised exception, {0}", e);
                        exception = MessageQueueHelper.ConvertMessageQueueException(e);
                        break;
                    }
                    catch (Exception e)
                    {
                        if (this.isDisposedField)
                        {
                            // if the queue is closed, then quit the method.
                            return;
                        }

                        BrokerTracing.TraceError("[MSMQPersist] .PersistRequestsThreadProc: persist request raised exception, {0}", e);
                        exception = e;

                        break;
                    }
                }
            }

            if (exception == null)
            {
                Interlocked.Add(ref this.uncommittedRequestsCountField, requestsCount);
                Interlocked.Add(ref this.requestsCountField, requestsCount);
                Interlocked.Add(ref this.allRequestsCountField, requestsCount);
            }

            PutRequestCallback putRequestCallback = putRequestState.Callback;
            if (putRequestCallback != null)
            {
                try
                {
                    putRequestCallback(exception, putRequestState.CallbackState);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError("[MSMQPersist] .PersistRequestsThreadProc: Persist requests failed, Exception:{0}.", e);
                }
            }
        }


        /// <summary>
        /// thread pool callback to save the responses.
        /// </summary>
        /// <param name="state">the state object, PersistState.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void PersistResponsesThreadProc(object state)
        {
            PutResponseState putResponseState = (PutResponseState)state;

            ParamCheckUtility.ThrowIfNull(putResponseState, "putResponseState");
            ParamCheckUtility.ThrowIfNull(putResponseState.Messages, "putResponseState.Mesasges");

            bool isInConcurrencySection = true;
            MessageQueueNativeTransaction msmqTransaction = new MessageQueueNativeTransaction();
            msmqTransaction.Begin();

            try
            {
                // Save peer request items of response messages in case persist operation is failed.
                List<BrokerQueueItem> peerItems = new List<BrokerQueueItem>();
                foreach (BrokerQueueItem response in putResponseState.Messages)
                {
                    peerItems.Add(response.PeerItem);
                    response.PeerItem = null;
                }

                Exception exception = null;
                int responseCount = 0;
                int faultResponsesCount = 0;
                List<BrokerQueueItem> redispatchRequestsList = new List<BrokerQueueItem>();
                foreach (BrokerQueueItem response in putResponseState.Messages)
                {
                    ParamCheckUtility.ThrowIfNull(response, "to-be-persisted response");

                    // step 1, receive corresponding request from queue
                    long[] requestLookupIds = (long[])response.PersistAsyncToken.AsyncToken;
                    System.Messaging.Message requestMessage = null;

                    foreach (long requestLookupId in requestLookupIds)
                    {
                        try
                        {
                            MessageQueueHelper.PerformRetriableOperation(
                            delegate(int retryNumber)
                            {
                                requestMessage = this.nativeRequestsQueueField.ReceiveByLookupId((ulong)requestLookupId, msmqTransaction);

                                exception = null;
                            },
                            "[MSMQPersist] .PersistResponsesThreadProc: can not receive the corresponding request by lookup id[{0}] from the requests queue when persist the response",
                            requestLookupId);
                        }
                        catch (MessageQueueNativeException e)
                        {
                            if (e.ErrorCode == (int)MessageQueueErrorCode.MessageNotFound)
                            {
                                // not found the original request in the request queue by lookup id, 
                                // then the response should be a duplicate response for the request that is removed by the previous response. just skip the response.
                                BrokerTracing.TraceError(
                                        "[MSMQPersist] .PersistResponsesThreadProc: can not receive the corresponding request by lookup id[{0}] from the requests queue when persist the response with the exception,{1}.",
                                        requestLookupId,
                                        e);

                                continue;
                            }
                        }
                        catch (Exception e)
                        {
                            if (this.isDisposedField)
                            {
                                // if the queue is closed, then quit the method.
                                return;
                            }

                            BrokerTracing.TraceError(
                                    "[MSMQPersist] .PersistResponsesThreadProc: can not receive the corresponding request by lookup id[{0}] from the requests queue when persist the response with the exception,{1}.",
                                    requestLookupId,
                                    e);
                            exception = e;

                            break;
                        }
                    }

                    if (response.Message.IsFault)
                    {
                        faultResponsesCount++;
                    }

                    // step 2, put response into queue
                    System.Messaging.Message sendMsg = MessageQueueNative.PrepareMessage(response);

                    try
                    {
                        exception = null;

                        List<System.Messaging.Message> sendMsgs = new List<Message>();
                        if (sendMsg.BodyStream.Length > Constant.MSMQChunkSize)
                        {
                            BrokerTracing.TraceVerbose("[MSMQPersist] .PersistResponsesThreadProc: response size {0} is larger than MSMQ chunk size.", sendMsg.BodyStream.Length);
                            string messageGroupId = response.PersistId.ToString();
                            byte[] buffer = new byte[Constant.MSMQChunkSize];
                            int msgNumber = (int)Math.Ceiling(((decimal)sendMsg.BodyStream.Length) / Constant.MSMQChunkSize);
                            int n = 0;
                            int msgCount = 0;
                            sendMsg.BodyStream.Position = 0;
                            while ((n = sendMsg.BodyStream.Read(buffer, 0, Constant.MSMQChunkSize)) != 0)
                            {
                                Message msg = new Message();
                                msg.BodyStream.Write(buffer, 0, n);
                                msg.AppSpecific = msgNumber;
                                msgCount++;
                                msg.Label = string.Format("{0}_{1}", messageGroupId, msgCount);
                                msg.BodyType = sendMsg.BodyType;
                                sendMsgs.Add(msg);
                            }
                        }
                        else
                        {
                            sendMsgs.Add(sendMsg);
                        }

                        foreach (Message msg in sendMsgs)
                        {
                            MessageQueueHelper.PerformRetriableOperation(
                            delegate(int retryNumber)
                            {
                                this.nativeResponsesQueueField.Send(msg, msmqTransaction);
                            },
                            "[MSMQPersist] .PersistResponsesThreadProc: can not send the response to the responses queue");
                        }

                        BrokerTracing.TraceVerbose("[MSMQPersist] .PersistResponsesThreadProc: send message(s) for persist id {0}.", response.PersistId);
                    }
                    catch (MessageQueueNativeException e)
                    {
                        if (this.isDisposedField)
                        {
                            // if the queue is closed, then quit the method.
                            return;
                        }

                        BrokerTracing.TraceError("[MSMQPersist] .PersistResponsesThreadProc: persist response raised exception, {0}", e);
                        exception = MessageQueueHelper.ConvertMessageQueueException(e);
                        break;
                    }
                    catch (Exception e)
                    {
                        if (this.isDisposedField)
                        {
                            // if the queue is closed, then quit the method.
                            return;
                        }

                        BrokerTracing.TraceError("[MSMQPersist] .PersistResponsesThreadProc: can not send the response to the responses queue, Exception: {0}", e);
                        exception = e;
                        break;
                    }

                    // At this point, both step 1 & step 2 are performed succeesfully
                    responseCount++;
                }

                if (exception == null)
                {
                    try
                    {
                        msmqTransaction.Commit();
                        MSMQConcurrencyController.Exit();
                        isInConcurrencySection = false;
                    }
                    catch (InvalidOperationException e)
                    {
                        BrokerTracing.TraceError(
                                "[MSMQPersist] .PersistResponsesThreadProc: Commit the PutResponse transaction raised the InvalidOperationException, {0}.",
                                e);
                        exception = e;
                    }
                    catch (Exception e)
                    {
                        if (this.isDisposedField)
                        {
                            // if the queue is closed, then quit the method.
                            return;
                        }

                        BrokerTracing.TraceError(
                                "[MSMQPersist] .PersistResponsesThreadProc: Commit the PutResponse transaction raised the exception, {0}.",
                                e);
                        exception = e;
                    }
                }

                // persisting succeed
                if (exception == null)
                {
                    // dispose all response messages
                    foreach (BrokerQueueItem response in putResponseState.Messages)
                    {
                        response.Dispose();
                    }
                    // dispose all peer items
                    foreach (BrokerQueueItem request in peerItems)
                    {
                        request.Dispose();
                    }
                }

                if (exception != null)
                {
                    msmqTransaction.Abort();
                    MSMQConcurrencyController.Exit();
                    isInConcurrencySection = false;

                    responseCount = 0;
                    BrokerTracing.TraceError("[MSMQPersist] .PersistResponsesThreadProc: failed to persist the response to the responses queue, and the corrresponding requests will be redispatched soon.");

                    // lost the responses, the corresponding requests should can be redispatched soon.
                    int index = 0;
                    foreach (BrokerQueueItem response in putResponseState.Messages)
                    {
                        if (response != null && response.PersistAsyncToken != null)
                        {
                            response.PeerItem = peerItems[index];
                            redispatchRequestsList.Add(response);
                        }
                        else
                        {
                            BrokerTracing.TraceError("[MSMQPersist] .PersistResponsesThreadProc: invalide response, the response.AsyncToken is null.");
                        }
                        index++;
                    }
                }
                else if (faultResponsesCount > 0)
                {
                    try
                    {
                        this.responsesQueueField.Label = (Interlocked.Read(ref this.failedRequestsCountField) + faultResponsesCount).ToString();
                        Interlocked.Add(ref this.failedRequestsCountField, faultResponsesCount);
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceWarning("[MSMQPersist] .PersistResponsesThreadProc: Fail to update the fault message number in the response queue label, fault message count: {0}, Exception: {1}", faultResponsesCount, e);
                    }
                }

                // Note: Update responsesCountField before requestsCountField. - tricky part.
                // FIXME!
                Interlocked.Add(ref this.responsesCountField, responseCount);
                this.responseFetcher.NotifyMoreMessages(responseCount);
                long remainingRequestCount = Interlocked.Add(ref this.requestsCountField, -responseCount);
                bool isLastResponse = EOMReceived && (remainingRequestCount == 0);
                PutResponseCallback putResponseCallback = putResponseState.Callback;
                if (putResponseCallback != null)
                {
                    putResponseCallback(exception, responseCount, faultResponsesCount, isLastResponse, redispatchRequestsList, putResponseState.CallbackState);
                }
            }
            catch (Exception e)
            {
                if (!this.isDisposedField)
                {
                    BrokerTracing.TraceError("[MSMQPersist] .PersistResponsesThreadProc: persist responses failed, Exception: {0}", e);
                }
            }
            finally
            {
                try
                {
                    if (msmqTransaction != null && msmqTransaction.Status == MessageQueueTransactionStatus.Pending)
                    {
                        msmqTransaction.Dispose();
                    }
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceError("[MSMQPersist] .PersistResponsesThreadProc: Exception throwed while disposing the transaction: {0}", e);
                }

                if (isInConcurrencySection)
                {
                    MSMQConcurrencyController.Exit();
                }
            }
        }

        /// <summary>
        /// thread pool callback state for putting requests
        /// </summary>
        private class PutRequestState
        {
            #region private fields
            /// <summary>the messages.</summary>
            private IEnumerable<BrokerQueueItem> messagesField;

            /// <summary>the callback for putting request.</summary>
            private PutRequestCallback callbackField;

            /// <summary>the callback state object.</summary>
            private object callbackStateField;
            #endregion

            /// <summary>
            /// Initializes a new instance of the PutRequestState class.
            /// </summary>
            /// <param name="messages">the messages.</param>
            /// <param name="calback">the callback.</param>
            /// <param name="callbackState">the calllback state object.</param>
            public PutRequestState(IEnumerable<BrokerQueueItem> messages, PutRequestCallback callback, object callbackState)
            {
                ParamCheckUtility.ThrowIfNull(messages, "messages");
                this.messagesField = messages;
                this.callbackField = callback;
                this.callbackStateField = callbackState;
            }


            /// <summary>
            /// Gets the requests.
            /// </summary>
            public IEnumerable<BrokerQueueItem> Messages
            {
                get
                {
                    return this.messagesField;
                }
            }

            /// <summary>
            /// Gets the callback.
            /// </summary>
            public PutRequestCallback Callback
            {
                get
                {
                    return this.callbackField;
                }
            }

            /// <summary>
            /// Gets the callback state.
            /// </summary>
            public object CallbackState
            {
                get
                {
                    return this.callbackStateField;
                }
            }
        }

        /// <summary>
        /// thread pool callback stat for putting responses
        /// </summary>
        private class PutResponseState
        {
            #region private fields
            /// <summary>the messages.</summary>
            private IEnumerable<BrokerQueueItem> messagesField;

            /// <summary>the callback for putting response.</summary>
            private PutResponseCallback callbackField;

            /// <summary>the calllback state object.</summary>
            private object callbackStateField;
            #endregion

            /// <summary>
            /// Initializes a new instance of the PutRequestState class.
            /// </summary>
            /// <param name="messages">the messages.</param>
            /// <param name="callback">the callback.</param>
            /// <param name="callbackState">the calllback state object.</param>
            public PutResponseState(IEnumerable<BrokerQueueItem> messages, PutResponseCallback callback, object callbackState)
            {
                ParamCheckUtility.ThrowIfNull(messages, "messages");
                this.messagesField = messages;
                this.callbackField = callback;
                this.callbackStateField = callbackState;
            }


            /// <summary>
            /// Gets the requests.
            /// </summary>
            public IEnumerable<BrokerQueueItem> Messages
            {
                get
                {
                    return this.messagesField;
                }
            }

            /// <summary>
            /// Gets the callback.
            /// </summary>
            public PutResponseCallback Callback
            {
                get
                {
                    return this.callbackField;
                }
            }

            /// <summary>
            /// Gets the callback state.
            /// </summary>
            public object CallbackState
            {
                get
                {
                    return this.callbackStateField;
                }
            }
        }
    }
}
#endif