// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.RuntimeTrace
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Runtime.InteropServices;

    public class RuntimeTrace : IDisposable
    {
        //
        // Provider Microsoft-HPC-Runtime Event Count 77
        //

        internal EventProviderVersionTwo m_provider = new EventProviderVersionTwo(new Guid("8979efb0-97da-4729-8296-f118f3562a53"));
        //
        // Task :  eventGUIDs
        //

        //
        // Event Descriptors
        //
        protected EventDescriptor TextVerbose;
        protected EventDescriptor TextInfo;
        protected EventDescriptor TextWarning;
        protected EventDescriptor TextError;
        protected EventDescriptor TextCritial;
        protected EventDescriptor EventTextCritial;
        protected EventDescriptor EventTextVerbose;
        protected EventDescriptor EventTextInfo;
        protected EventDescriptor EventTextWarning;
        protected EventDescriptor EventTextError;
        protected EventDescriptor HostRequestReceived;
        protected EventDescriptor HostResponseSent;
        protected EventDescriptor HostStart;
        protected EventDescriptor HostStop;
        protected EventDescriptor HostServiceConfigCheck;
        protected EventDescriptor HostAssemblyLoaded;
        protected EventDescriptor HostCanceled;
        protected EventDescriptor FrontEndRequestReceived;
        protected EventDescriptor FrontEndResponseSent;
        protected EventDescriptor BackendRequestSent;
        protected EventDescriptor BackendResponseReceived;
        protected EventDescriptor BackendRequestSentFailed;
        protected EventDescriptor BackendResponseReceivedFailed;
        protected EventDescriptor FrontEndResponseSentFailed;
        protected EventDescriptor FrontEndRequestRejectedClientIdNotMatch;
        protected EventDescriptor FrontEndRequestRejectedClientIdInvalid;
        protected EventDescriptor FrontEndRequestRejectedClientStateInvalid;
        protected EventDescriptor BackendResponseReceivedRetryOperationError;
        protected EventDescriptor BackendResponseReceivedRetryLimitExceed;
        protected EventDescriptor BackendRequestPutBack;
        protected EventDescriptor BackendGeneratedFaultReply;
        protected EventDescriptor FrontEndRequestRejectedAuthenticationError;
        protected EventDescriptor FrontEndRequestRejectedGeneralError;
        protected EventDescriptor BackendHandleResponseFailed;
        protected EventDescriptor BackendHandleEndpointNotFoundExceptionFailed;
        protected EventDescriptor BackendHandleExceptionFailed;
        protected EventDescriptor BackendEndpointNotFoundExceptionOccured;
        protected EventDescriptor BackendDispatcherClosed;
        protected EventDescriptor BackendResponseReceivedSessionFault;
        protected EventDescriptor BackendValidateClientFailed;
        protected EventDescriptor BackendResponseStored;
        protected EventDescriptor SessionCreating;
        protected EventDescriptor SessionCreated;
        protected EventDescriptor FailedToCreateSession;
        protected EventDescriptor SessionFinished;
        protected EventDescriptor SessionFinishedBecauseOfJobCanceled;
        protected EventDescriptor SessionFinishedBecauseOfTimeout;
        protected EventDescriptor SessionSuspended;
        protected EventDescriptor SessionSuspendedBecauseOfJobCanceled;
        protected EventDescriptor SessionSuspendedBecauseOfTimeout;
        protected EventDescriptor SessionRaisedUp;
        protected EventDescriptor SessionRaisedUpFailover;
        protected EventDescriptor BrokerWorkerUnexpectedlyExit;
        protected EventDescriptor BrokerWorkerMessage;
        protected EventDescriptor BrokerWorkerProcessReady;
        protected EventDescriptor BrokerWorkerProcessExited;
        protected EventDescriptor BrokerWorkerProcessFailedToInitialize;
        protected EventDescriptor BrokerClientCreated;
        protected EventDescriptor BrokerClientStateTransition;
        protected EventDescriptor BrokerClientRejectFlush;
        protected EventDescriptor BrokerClientRejectEOM;
        protected EventDescriptor BrokerClientTimedOut;
        protected EventDescriptor BrokerClientAllResponseDispatched;
        protected EventDescriptor BrokerClientAllRequestDone;
        protected EventDescriptor BrokerClientDisconnected;
        protected EventDescriptor UserTraceVerbose;
        protected EventDescriptor UserTraceInfo;
        protected EventDescriptor UserTraceWarning;
        protected EventDescriptor UserTraceError;
        protected EventDescriptor UserTraceCritial;
        protected EventDescriptor FrontendBindingLoaded;
        protected EventDescriptor BackendBindingLoaded;
        protected EventDescriptor FrontendCreated;
        protected EventDescriptor FrontendControllerCreated;
        protected EventDescriptor QueueCreatedOrExist;
        protected EventDescriptor QueueDeleted;
        protected EventDescriptor QueueNotExist;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_provider.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }


        public RuntimeTrace()
        {
            unchecked
            {
                this.TextVerbose = new EventDescriptor(0xa, 0x0, 0x13, 0x5, 0x0, 0x1, (long)0x1000000000000001);
                this.TextInfo = new EventDescriptor(0xb, 0x0, 0x13, 0x4, 0x0, 0x1, (long)0x1000000000000001);
                this.TextWarning = new EventDescriptor(0xc, 0x0, 0x13, 0x3, 0x0, 0x1, (long)0x1000000000000001);
                this.TextError = new EventDescriptor(0xd, 0x0, 0x13, 0x2, 0x0, 0x1, (long)0x1000000000000001);
                this.TextCritial = new EventDescriptor(0xe, 0x0, 0x13, 0x1, 0x0, 0x1, (long)0x1000000000000001);
                this.EventTextCritial = new EventDescriptor(0x271e, 0x0, 0x10, 0x1, 0x0, 0xa, (long)0x8000000000000001);
                this.EventTextVerbose = new EventDescriptor(0x271a, 0x0, 0x12, 0x5, 0x0, 0xa, (long)0x2000000000000001);
                this.EventTextInfo = new EventDescriptor(0x271b, 0x0, 0x12, 0x4, 0x0, 0xa, (long)0x2000000000000001);
                this.EventTextWarning = new EventDescriptor(0x271c, 0x0, 0x12, 0x3, 0x0, 0xa, (long)0x2000000000000001);
                this.EventTextError = new EventDescriptor(0x271d, 0x0, 0x11, 0x2, 0x0, 0xa, (long)0x4000000000000001);
                this.HostRequestReceived = new EventDescriptor(0x3e8, 0x0, 0x13, 0x5, 0xf0, 0x6, (long)0x1000000000000000);
                this.HostResponseSent = new EventDescriptor(0x3e9, 0x0, 0x13, 0x5, 0x6, 0x6, (long)0x1000000000000000);
                this.HostStart = new EventDescriptor(0x3ea, 0x0, 0x13, 0x4, 0x1, 0x6, (long)0x1000000000000000);
                this.HostStop = new EventDescriptor(0x3eb, 0x0, 0x13, 0x4, 0x2, 0x6, (long)0x1000000000000000);
                this.HostServiceConfigCheck = new EventDescriptor(0x3ec, 0x0, 0x10, 0x3, 0x0, 0xa, (long)0x8000000000000000);
                this.HostAssemblyLoaded = new EventDescriptor(0x3ed, 0x0, 0x13, 0x4, 0x0, 0x6, (long)0x1000000000000000);
                this.HostCanceled = new EventDescriptor(0x3ee, 0x0, 0x13, 0x4, 0x0, 0x6, (long)0x1000000000000000);
                this.FrontEndRequestReceived = new EventDescriptor(0xbb8, 0x0, 0x13, 0x5, 0xf0, 0x7, (long)0x1000000000000000);
                this.FrontEndResponseSent = new EventDescriptor(0xbb9, 0x0, 0x13, 0x5, 0x6, 0x7, (long)0x1000000000000000);
                this.BackendRequestSent = new EventDescriptor(0xbba, 0x0, 0x13, 0x5, 0x9, 0x7, (long)0x1000000000000000);
                this.BackendResponseReceived = new EventDescriptor(0xbbb, 0x0, 0x13, 0x5, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendRequestSentFailed = new EventDescriptor(0xbbc, 0x0, 0x13, 0x3, 0x9, 0x7, (long)0x1000000000000000);
                this.BackendResponseReceivedFailed = new EventDescriptor(0xbbd, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.FrontEndResponseSentFailed = new EventDescriptor(0xbbe, 0x0, 0x13, 0x3, 0x9, 0x7, (long)0x1000000000000000);
                this.FrontEndRequestRejectedClientIdNotMatch = new EventDescriptor(0xbbf, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.FrontEndRequestRejectedClientIdInvalid = new EventDescriptor(0xbc0, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.FrontEndRequestRejectedClientStateInvalid = new EventDescriptor(0xbc1, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendResponseReceivedRetryOperationError = new EventDescriptor(0xbc2, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendResponseReceivedRetryLimitExceed = new EventDescriptor(0xbc3, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendRequestPutBack = new EventDescriptor(0xbc4, 0x0, 0x13, 0x5, 0x9, 0x7, (long)0x1000000000000000);
                this.BackendGeneratedFaultReply = new EventDescriptor(0xbc5, 0x0, 0x13, 0x5, 0xf0, 0x7, (long)0x1000000000000000);
                this.FrontEndRequestRejectedAuthenticationError = new EventDescriptor(0xbc6, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.FrontEndRequestRejectedGeneralError = new EventDescriptor(0xbc7, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendHandleResponseFailed = new EventDescriptor(0xbc8, 0x0, 0x13, 0x2, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendHandleEndpointNotFoundExceptionFailed = new EventDescriptor(0xbc9, 0x0, 0x13, 0x2, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendHandleExceptionFailed = new EventDescriptor(0xbca, 0x0, 0x13, 0x2, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendEndpointNotFoundExceptionOccured = new EventDescriptor(0xbcb, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendDispatcherClosed = new EventDescriptor(0xbcc, 0x0, 0x13, 0x3, 0x9, 0x7, (long)0x1000000000000000);
                this.BackendResponseReceivedSessionFault = new EventDescriptor(0xbcd, 0x0, 0x13, 0x3, 0xf0, 0x7, (long)0x1000000000000000);
                this.BackendValidateClientFailed = new EventDescriptor(0xbce, 0x0, 0x13, 0x3, 0x9, 0x7, (long)0x1000000000000000);
                this.BackendResponseStored = new EventDescriptor(0xbcf, 0x0, 0x13, 0x5, 0x9, 0x7, (long)0x1000000000000000);
                this.SessionCreating = new EventDescriptor(0xfa0, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.SessionCreated = new EventDescriptor(0xfa1, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.FailedToCreateSession = new EventDescriptor(0xfa2, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.SessionFinished = new EventDescriptor(0xfa3, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.SessionFinishedBecauseOfJobCanceled = new EventDescriptor(0xfa4, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.SessionFinishedBecauseOfTimeout = new EventDescriptor(0xfa5, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.SessionSuspended = new EventDescriptor(0xfa6, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.SessionSuspendedBecauseOfJobCanceled = new EventDescriptor(0xfa7, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.SessionSuspendedBecauseOfTimeout = new EventDescriptor(0xfa8, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.SessionRaisedUp = new EventDescriptor(0xfa9, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.SessionRaisedUpFailover = new EventDescriptor(0xfaa, 0x0, 0x13, 0x4, 0x0, 0x2, (long)0x1000000000000000);
                this.BrokerWorkerUnexpectedlyExit = new EventDescriptor(0x1388, 0x0, 0x11, 0x2, 0x0, 0x8, (long)0x4000000000000000);
                this.BrokerWorkerMessage = new EventDescriptor(0x1389, 0x0, 0x11, 0x4, 0x0, 0x8, (long)0x4000000000000000);
                this.BrokerWorkerProcessReady = new EventDescriptor(0x138a, 0x0, 0x11, 0x4, 0x0, 0x8, (long)0x4000000000000000);
                this.BrokerWorkerProcessExited = new EventDescriptor(0x138b, 0x0, 0x11, 0x4, 0x0, 0x8, (long)0x4000000000000000);
                this.BrokerWorkerProcessFailedToInitialize = new EventDescriptor(0x138c, 0x0, 0x11, 0x3, 0x0, 0x8, (long)0x4000000000000000);
                this.BrokerClientCreated = new EventDescriptor(0x1b58, 0x0, 0x13, 0x4, 0x0, 0x3, (long)0x1000000000000000);
                this.BrokerClientStateTransition = new EventDescriptor(0x1b59, 0x0, 0x13, 0x4, 0x0, 0x3, (long)0x1000000000000000);
                this.BrokerClientRejectFlush = new EventDescriptor(0x1b5a, 0x0, 0x13, 0x3, 0x0, 0x3, (long)0x1000000000000000);
                this.BrokerClientRejectEOM = new EventDescriptor(0x1b5b, 0x0, 0x13, 0x3, 0x0, 0x3, (long)0x1000000000000000);
                this.BrokerClientTimedOut = new EventDescriptor(0x1b5c, 0x0, 0x13, 0x4, 0x0, 0x3, (long)0x1000000000000000);
                this.BrokerClientAllResponseDispatched = new EventDescriptor(0x1b5d, 0x0, 0x13, 0x4, 0x0, 0x3, (long)0x1000000000000000);
                this.BrokerClientAllRequestDone = new EventDescriptor(0x1b5e, 0x0, 0x13, 0x4, 0x0, 0x3, (long)0x1000000000000000);
                this.BrokerClientDisconnected = new EventDescriptor(0x1b5f, 0x0, 0x13, 0x4, 0x0, 0x3, (long)0x1000000000000000);
                this.UserTraceVerbose = new EventDescriptor(0x2328, 0x0, 0x13, 0x5, 0x0, 0x9, (long)0x1000000000000001);
                this.UserTraceInfo = new EventDescriptor(0x2329, 0x0, 0x13, 0x4, 0x0, 0x9, (long)0x1000000000000001);
                this.UserTraceWarning = new EventDescriptor(0x232a, 0x0, 0x13, 0x3, 0x0, 0x9, (long)0x1000000000000001);
                this.UserTraceError = new EventDescriptor(0x232b, 0x0, 0x13, 0x2, 0x0, 0x9, (long)0x1000000000000001);
                this.UserTraceCritial = new EventDescriptor(0x232c, 0x0, 0x10, 0x1, 0x0, 0x9, (long)0x8000000000000001);
                this.FrontendBindingLoaded = new EventDescriptor(0x2af8, 0x0, 0x13, 0x4, 0x0, 0xb, (long)0x1000000000000000);
                this.BackendBindingLoaded = new EventDescriptor(0x2af9, 0x0, 0x13, 0x4, 0x0, 0xb, (long)0x1000000000000000);
                this.FrontendCreated = new EventDescriptor(0x2afa, 0x0, 0x13, 0x4, 0x0, 0xb, (long)0x1000000000000000);
                this.FrontendControllerCreated = new EventDescriptor(0x2afb, 0x0, 0x13, 0x4, 0x0, 0xb, (long)0x1000000000000000);
                this.QueueCreatedOrExist = new EventDescriptor(0x2ee0, 0x0, 0x13, 0x4, 0x0, 0xc, (long)0x1000000000000000);
                this.QueueDeleted = new EventDescriptor(0x2ee1, 0x0, 0x13, 0x4, 0x0, 0xc, (long)0x1000000000000000);
                this.QueueNotExist = new EventDescriptor(0x2ee2, 0x0, 0x13, 0x2, 0x0, 0xc, (long)0x1000000000000000);
            }
        }


        //
        // Event method for TextVerbose
        //
        public bool LogTextVerbose(string SessionId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateString(ref this.TextVerbose, SessionId, String);
        }

        //
        // Event method for TextInfo
        //
        public bool LogTextInfo(string SessionId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateString(ref this.TextInfo, SessionId, String);
        }

        //
        // Event method for TextWarning
        //
        public bool LogTextWarning(string SessionId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateString(ref this.TextWarning, SessionId, String);
        }

        //
        // Event method for TextError
        //
        public bool LogTextError(string SessionId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateString(ref this.TextError, SessionId, String);
        }

        //
        // Event method for TextCritial
        //
        public bool LogTextCritial(string SessionId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateString(ref this.TextCritial, SessionId, String);
        }

        //
        // Event method for EventTextCritial
        //
        public bool LogEventTextCritial(string String)
        {

            return this.m_provider.WriteEvent(ref this.EventTextCritial, String);

        }

        //
        // Event method for EventTextVerbose
        //
        public bool LogEventTextVerbose(string String)
        {

            return this.m_provider.WriteEvent(ref this.EventTextVerbose, String);

        }

        //
        // Event method for EventTextInfo
        //
        public bool LogEventTextInfo(string String)
        {

            return this.m_provider.WriteEvent(ref this.EventTextInfo, String);

        }

        //
        // Event method for EventTextWarning
        //
        public bool LogEventTextWarning(string String)
        {

            return this.m_provider.WriteEvent(ref this.EventTextWarning, String);

        }

        //
        // Event method for EventTextError
        //
        public bool LogEventTextError(string String)
        {

            return this.m_provider.WriteEvent(ref this.EventTextError, String);

        }

        //
        // Event method for HostRequestReceived
        //
        public bool LogHostRequestReceived(string SessionId, Guid MessageId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateMessage(ref this.HostRequestReceived, SessionId, MessageId);
        }

        //
        // Event method for HostResponseSent
        //
        public bool LogHostResponseSent(string SessionId, Guid MessageId, bool Fault)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateMessageWithFault(ref this.HostResponseSent, SessionId, MessageId, Fault);
        }

        //
        // Event method for HostStart
        //
        public bool LogHostStart(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.HostStart, SessionId);
        }

        //
        // Event method for HostStop
        //
        public bool LogHostStop(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.HostStop, SessionId);
        }

        //
        // Event method for HostServiceConfigCheck
        //
        public bool LogHostServiceConfigCheck(string FileName)
        {

            return this.m_provider.WriteEvent(ref this.HostServiceConfigCheck, FileName);

        }

        //
        // Event method for HostAssemblyLoaded
        //
        public bool LogHostAssemblyLoaded(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.HostAssemblyLoaded, SessionId);
        }

        //
        // Event method for HostCanceled
        //
        public bool LogHostCanceled(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.HostCanceled, SessionId);
        }

        //
        // Event method for FrontEndRequestReceived
        //
        public bool LogFrontEndRequestReceived(string SessionId, string ClientId, Guid MessageId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateFrontendRequestMessage(ref this.FrontEndRequestReceived, SessionId, ClientId, MessageId);
        }

        //
        // Event method for FrontEndResponseSent
        //
        public bool LogFrontEndResponseSent(string SessionId, string ClientId, Guid MessageId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateFrontendRequestMessage(ref this.FrontEndResponseSent, SessionId, ClientId, MessageId);
        }

        //
        // Event method for BackendRequestSent
        //
        public bool LogBackendRequestSent(string SessionId, string TaskId, Guid MessageId, Guid DispatchId, string TargetMachine, string EndpointAddress)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendRequestMessageWithEPR(ref this.BackendRequestSent, SessionId, TaskId, MessageId, DispatchId, TargetMachine, EndpointAddress);
        }

        //
        // Event method for BackendResponseReceived
        //
        public bool LogBackendResponseReceived(string SessionId, string TaskId, Guid MessageId, bool Fault)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendResponseMessage(ref this.BackendResponseReceived, SessionId, TaskId, MessageId, Fault);
        }

        //
        // Event method for BackendRequestSentFailed
        //
        public bool LogBackendRequestSentFailed(string SessionId, string TaskId, Guid MessageId, string Exception)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendMessageWithException(ref this.BackendRequestSentFailed, SessionId, TaskId, MessageId, Exception);
        }

        //
        // Event method for BackendResponseReceivedFailed
        //
        public bool LogBackendResponseReceivedFailed(string SessionId, string TaskId, Guid MessageId, int RetryCount, string Exception)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendResponseMessageWithExceptionAndRetryCount(ref this.BackendResponseReceivedFailed, SessionId, TaskId, MessageId, RetryCount, Exception);
        }

        //
        // Event method for FrontEndResponseSentFailed
        //
        public bool LogFrontEndResponseSentFailed(string SessionId, string ClientId, Guid MessageId, string Exception)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateFrontendRequestMessageGeneralError(ref this.FrontEndResponseSentFailed, SessionId, ClientId, MessageId, Exception);
        }

        //
        // Event method for FrontEndRequestRejectedClientIdNotMatch
        //
        public bool LogFrontEndRequestRejectedClientIdNotMatch(string SessionId, string ClientId, Guid MessageId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateFrontendRequestMessage(ref this.FrontEndRequestRejectedClientIdNotMatch, SessionId, ClientId, MessageId);
        }

        //
        // Event method for FrontEndRequestRejectedClientIdInvalid
        //
        public bool LogFrontEndRequestRejectedClientIdInvalid(string SessionId, string ClientId, Guid MessageId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateFrontendRequestMessage(ref this.FrontEndRequestRejectedClientIdInvalid, SessionId, ClientId, MessageId);
        }

        //
        // Event method for FrontEndRequestRejectedClientStateInvalid
        //
        public bool LogFrontEndRequestRejectedClientStateInvalid(string SessionId, string ClientId, Guid MessageId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateFrontendRequestMessage(ref this.FrontEndRequestRejectedClientStateInvalid, SessionId, ClientId, MessageId);
        }

        //
        // Event method for BackendResponseReceivedRetryOperationError
        //
        public bool LogBackendResponseReceivedRetryOperationError(string SessionId, string TaskId, Guid MessageId, int RetryCount)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendResponseMessageRetry(ref this.BackendResponseReceivedRetryOperationError, SessionId, TaskId, MessageId, RetryCount);
        }

        //
        // Event method for BackendResponseReceivedRetryLimitExceed
        //
        public bool LogBackendResponseReceivedRetryLimitExceed(string SessionId, string TaskId, Guid MessageId, int RetryCount)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendResponseMessageRetry(ref this.BackendResponseReceivedRetryLimitExceed, SessionId, TaskId, MessageId, RetryCount);
        }

        //
        // Event method for BackendRequestPutBack
        //
        public bool LogBackendRequestPutBack(string SessionId, string TaskId, Guid MessageId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendRequestMessage(ref this.BackendRequestPutBack, SessionId, TaskId, MessageId);
        }

        //
        // Event method for BackendGeneratedFaultReply
        //
        public bool LogBackendGeneratedFaultReply(string SessionId, string TaskId, Guid MessageId, string FaultReply)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendGeneratedFaultReply(ref this.BackendGeneratedFaultReply, SessionId, TaskId, MessageId, FaultReply);
        }

        //
        // Event method for FrontEndRequestRejectedAuthenticationError
        //
        public bool LogFrontEndRequestRejectedAuthenticationError(string SessionId, string ClientId, Guid MessageId, string UserName)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateFrontendRequestMessageAuthenticationError(ref this.FrontEndRequestRejectedAuthenticationError, SessionId, ClientId, MessageId, UserName);
        }

        //
        // Event method for FrontEndRequestRejectedGeneralError
        //
        public bool LogFrontEndRequestRejectedGeneralError(string SessionId, string ClientId, Guid MessageId, string Exception)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateFrontendRequestMessageGeneralError(ref this.FrontEndRequestRejectedGeneralError, SessionId, ClientId, MessageId, Exception);
        }

        //
        // Event method for BackendHandleResponseFailed
        //
        public bool LogBackendHandleResponseFailed(string SessionId, string TaskId, Guid MessageId, string Exception)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendMessageWithException(ref this.BackendHandleResponseFailed, SessionId, TaskId, MessageId, Exception);
        }

        //
        // Event method for BackendHandleEndpointNotFoundExceptionFailed
        //
        public bool LogBackendHandleEndpointNotFoundExceptionFailed(string SessionId, string TaskId, Guid MessageId, string Exception)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendMessageWithException(ref this.BackendHandleEndpointNotFoundExceptionFailed, SessionId, TaskId, MessageId, Exception);
        }

        //
        // Event method for BackendHandleExceptionFailed
        //
        public bool LogBackendHandleExceptionFailed(string SessionId, string TaskId, Guid MessageId, string Exception)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendMessageWithException(ref this.BackendHandleExceptionFailed, SessionId, TaskId, MessageId, Exception);
        }

        //
        // Event method for BackendEndpointNotFoundExceptionOccured
        //
        public bool LogBackendEndpointNotFoundExceptionOccured(string SessionId, string TaskId, Guid MessageId, string Exception)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendMessageWithException(ref this.BackendEndpointNotFoundExceptionOccured, SessionId, TaskId, MessageId, Exception);
        }

        //
        // Event method for BackendDispatcherClosed
        //
        public bool LogBackendDispatcherClosed(string SessionId, string TaskId, Guid MessageId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendRequestMessage(ref this.BackendDispatcherClosed, SessionId, TaskId, MessageId);
        }

        //
        // Event method for BackendResponseReceivedSessionFault
        //
        public bool LogBackendResponseReceivedSessionFault(string SessionId, string TaskId, Guid MessageId, string FaultCode)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendResponseMessageWithFaultCode(ref this.BackendResponseReceivedSessionFault, SessionId, TaskId, MessageId, FaultCode);
        }

        //
        // Event method for BackendValidateClientFailed
        //
        public bool LogBackendValidateClientFailed(string SessionId, string TaskId, Guid MessageId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendRequestMessage(ref this.BackendValidateClientFailed, SessionId, TaskId, MessageId);
        }

        //
        // Event method for BackendResponseStored
        //
        public bool LogBackendResponseStored(string SessionId, string TaskId, Guid MessageId, bool Fault)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBackendResponseMessage(ref this.BackendResponseStored, SessionId, TaskId, MessageId, Fault);
        }

        //
        // Event method for SessionCreating
        //
        public bool LogSessionCreating(string SessionId, string UserName)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSessionCreating(ref this.SessionCreating, SessionId, UserName);
        }

        //
        // Event method for SessionCreated
        //
        public bool LogSessionCreated(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.SessionCreated, SessionId);
        }

        //
        // Event method for FailedToCreateSession
        //
        public bool LogFailedToCreateSession(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.FailedToCreateSession, SessionId);
        }

        //
        // Event method for SessionFinished
        //
        public bool LogSessionFinished(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.SessionFinished, SessionId);
        }

        //
        // Event method for SessionFinishedBecauseOfJobCanceled
        //
        public bool LogSessionFinishedBecauseOfJobCanceled(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.SessionFinishedBecauseOfJobCanceled, SessionId);
        }

        //
        // Event method for SessionFinishedBecauseOfTimeout
        //
        public bool LogSessionFinishedBecauseOfTimeout(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.SessionFinishedBecauseOfTimeout, SessionId);
        }

        //
        // Event method for SessionSuspended
        //
        public bool LogSessionSuspended(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.SessionSuspended, SessionId);
        }

        //
        // Event method for SessionSuspendedBecauseOfJobCanceled
        //
        public bool LogSessionSuspendedBecauseOfJobCanceled(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.SessionSuspendedBecauseOfJobCanceled, SessionId);
        }

        //
        // Event method for SessionSuspendedBecauseOfTimeout
        //
        public bool LogSessionSuspendedBecauseOfTimeout(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.SessionSuspendedBecauseOfTimeout, SessionId);
        }

        //
        // Event method for SessionRaisedUp
        //
        public bool LogSessionRaisedUp(string SessionId, string UserName)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSessionCreating(ref this.SessionRaisedUp, SessionId, UserName);
        }

        //
        // Event method for SessionRaisedUpFailover
        //
        public bool LogSessionRaisedUpFailover(string SessionId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSession(ref this.SessionRaisedUpFailover, SessionId);
        }

        //
        // Event method for BrokerWorkerUnexpectedlyExit
        //
        public bool LogBrokerWorkerUnexpectedlyExit(int Id, string Message)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerWorkerMessage(ref this.BrokerWorkerUnexpectedlyExit, Id, Message);
        }

        //
        // Event method for BrokerWorkerMessage
        //
        public bool LogBrokerWorkerMessage(int Id, string Message)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerWorkerMessage(ref this.BrokerWorkerMessage, Id, Message);
        }

        //
        // Event method for BrokerWorkerProcessReady
        //
        public bool LogBrokerWorkerProcessReady(int PID)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerWorkerPID(ref this.BrokerWorkerProcessReady, PID);
        }

        //
        // Event method for BrokerWorkerProcessExited
        //
        public bool LogBrokerWorkerProcessExited(int PID, int ExitCode)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerWorkerExitCode(ref this.BrokerWorkerProcessExited, PID, ExitCode);
        }

        //
        // Event method for BrokerWorkerProcessFailedToInitialize
        //
        public bool LogBrokerWorkerProcessFailedToInitialize(int PID)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerWorkerPID(ref this.BrokerWorkerProcessFailedToInitialize, PID);
        }

        //
        // Event method for BrokerClientCreated
        //
        public bool LogBrokerClientCreated(string SessionId, string ClientId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerClientId(ref this.BrokerClientCreated, SessionId, ClientId);
        }

        //
        // Event method for BrokerClientStateTransition
        //
        public bool LogBrokerClientStateTransition(string SessionId, string ClientId, string State)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerClientIdWithState(ref this.BrokerClientStateTransition, SessionId, ClientId, State);
        }

        //
        // Event method for BrokerClientRejectFlush
        //
        public bool LogBrokerClientRejectFlush(string SessionId, string ClientId, string State)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerClientIdWithState(ref this.BrokerClientRejectFlush, SessionId, ClientId, State);
        }

        //
        // Event method for BrokerClientRejectEOM
        //
        public bool LogBrokerClientRejectEOM(string SessionId, string ClientId, string State)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerClientIdWithState(ref this.BrokerClientRejectEOM, SessionId, ClientId, State);
        }

        //
        // Event method for BrokerClientTimedOut
        //
        public bool LogBrokerClientTimedOut(string SessionId, string ClientId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerClientId(ref this.BrokerClientTimedOut, SessionId, ClientId);
        }

        //
        // Event method for BrokerClientAllResponseDispatched
        //
        public bool LogBrokerClientAllResponseDispatched(string SessionId, string ClientId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerClientId(ref this.BrokerClientAllResponseDispatched, SessionId, ClientId);
        }

        //
        // Event method for BrokerClientAllRequestDone
        //
        public bool LogBrokerClientAllRequestDone(string SessionId, string ClientId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerClientId(ref this.BrokerClientAllRequestDone, SessionId, ClientId);
        }

        //
        // Event method for BrokerClientDisconnected
        //
        public bool LogBrokerClientDisconnected(string SessionId, string ClientId, string State)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBrokerClientIdWithState(ref this.BrokerClientDisconnected, SessionId, ClientId, State);
        }

        //
        // Event method for UserTraceVerbose
        //
        public bool LogUserTraceVerbose(string SessionId, Guid MessageId, Guid DispatchId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateStringWithMessageId(ref this.UserTraceVerbose, SessionId, MessageId, DispatchId, String);
        }

        //
        // Event method for UserTraceInfo
        //
        public bool LogUserTraceInfo(string SessionId, Guid MessageId, Guid DispatchId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateStringWithMessageId(ref this.UserTraceInfo, SessionId, MessageId, DispatchId, String);
        }

        //
        // Event method for UserTraceWarning
        //
        public bool LogUserTraceWarning(string SessionId, Guid MessageId, Guid DispatchId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateStringWithMessageId(ref this.UserTraceWarning, SessionId, MessageId, DispatchId, String);
        }

        //
        // Event method for UserTraceError
        //
        public bool LogUserTraceError(string SessionId, Guid MessageId, Guid DispatchId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateStringWithMessageId(ref this.UserTraceError, SessionId, MessageId, DispatchId, String);
        }

        //
        // Event method for UserTraceCritial
        //
        public bool LogUserTraceCritial(string SessionId, Guid MessageId, Guid DispatchId, string String)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateStringWithMessageId(ref this.UserTraceCritial, SessionId, MessageId, DispatchId, String);
        }

        //
        // Event method for FrontendBindingLoaded
        //
        public bool LogFrontendBindingLoaded(string SessionId, string Symbol, long MaxMessageSize, long ReceiveTimeout, long SendTimeout, string MessageVersion, string Scheme)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSessionBindingInfo(ref this.FrontendBindingLoaded, SessionId, Symbol, MaxMessageSize, ReceiveTimeout, SendTimeout, MessageVersion, Scheme);
        }

        //
        // Event method for BackendBindingLoaded
        //
        public bool LogBackendBindingLoaded(string SessionId, string Symbol, long MaxMessageSize, long ReceiveTimeout, long SendTimeout, string MessageVersion, string Scheme)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSessionBindingInfo(ref this.BackendBindingLoaded, SessionId, Symbol, MaxMessageSize, ReceiveTimeout, SendTimeout, MessageVersion, Scheme);
        }

        //
        // Event method for FrontendCreated
        //
        public bool LogFrontendCreated(string SessionId, string Symbol, string FrontendUri)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSessionFrontendUri(ref this.FrontendCreated, SessionId, Symbol, FrontendUri);
        }

        //
        // Event method for FrontendControllerCreated
        //
        public bool LogFrontendControllerCreated(string SessionId, string Symbol, string ControllerUri, string GetResponseUri)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateSessionControllerUri(ref this.FrontendControllerCreated, SessionId, Symbol, ControllerUri, GetResponseUri);
        }

        //
        // Event method for QueueCreatedOrExist
        //
        public bool LogQueueCreatedOrExist(string SessionId, string QueueName)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateAzureStorageQueue(ref this.QueueCreatedOrExist, SessionId, QueueName);
        }

        //
        // Event method for QueueDeleted
        //
        public bool LogQueueDeleted(string SessionId, string QueueName)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateAzureStorageQueue(ref this.QueueDeleted, SessionId, QueueName);
        }

        //
        // Event method for QueueNotExist
        //
        public bool LogQueueNotExist(string SessionId, string QueueName)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateAzureStorageQueue(ref this.QueueNotExist, SessionId, QueueName);
        }
    }

    public class SOADiagTrace : IDisposable
    {
        //
        // Provider Microsoft-HPC-SOADiag Event Count 11
        //

        internal EventProviderVersionTwo m_provider = new EventProviderVersionTwo(new Guid("f120775c-db24-4504-b1c8-7f120d8008a0"));
        //
        // Task :  eventGUIDs
        //

        //
        // Event Descriptors
        //
        protected EventDescriptor TextVerbose;
        protected EventDescriptor TextInfo;
        protected EventDescriptor TextWarning;
        protected EventDescriptor TextError;
        protected EventDescriptor TextCritial;
        protected EventDescriptor EventReceived;
        protected EventDescriptor EventBuffered;
        protected EventDescriptor BufferWritten;
        protected EventDescriptor BufferDiscarded;
        protected EventDescriptor StartThrottle;
        protected EventDescriptor StopThrottle;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_provider.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }


        public SOADiagTrace()
        {
            unchecked
            {
                this.TextVerbose = new EventDescriptor(0xa, 0x0, 0x12, 0x5, 0x0, 0x1, (long)0x2000000000000001);
                this.TextInfo = new EventDescriptor(0xb, 0x0, 0x12, 0x4, 0x0, 0x1, (long)0x2000000000000001);
                this.TextWarning = new EventDescriptor(0xc, 0x0, 0x12, 0x3, 0x0, 0x1, (long)0x2000000000000001);
                this.TextError = new EventDescriptor(0xd, 0x0, 0x11, 0x2, 0x0, 0x1, (long)0x4000000000000001);
                this.TextCritial = new EventDescriptor(0xe, 0x0, 0x10, 0x1, 0x0, 0x1, (long)0x8000000000000001);
                this.EventReceived = new EventDescriptor(0x3e8, 0x0, 0x12, 0x5, 0x0, 0x0, (long)0x2000000000000000);
                this.EventBuffered = new EventDescriptor(0x3e9, 0x0, 0x12, 0x5, 0x0, 0x0, (long)0x2000000000000000);
                this.BufferWritten = new EventDescriptor(0x3ea, 0x0, 0x12, 0x5, 0x0, 0x0, (long)0x2000000000000000);
                this.BufferDiscarded = new EventDescriptor(0x3eb, 0x0, 0x11, 0x3, 0x0, 0x0, (long)0x4000000000000000);
                this.StartThrottle = new EventDescriptor(0x7d0, 0x0, 0x11, 0x4, 0x0, 0x0, (long)0x4000000000000000);
                this.StopThrottle = new EventDescriptor(0x7d1, 0x0, 0x11, 0x4, 0x0, 0x0, (long)0x4000000000000000);
            }
        }


        //
        // Event method for TextVerbose
        //
        public bool LogTextVerbose(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextVerbose, String);

        }

        //
        // Event method for TextInfo
        //
        public bool LogTextInfo(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextInfo, String);

        }

        //
        // Event method for TextWarning
        //
        public bool LogTextWarning(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextWarning, String);

        }

        //
        // Event method for TextError
        //
        public bool LogTextError(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextError, String);

        }

        //
        // Event method for TextCritial
        //
        public bool LogTextCritial(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextCritial, String);

        }

        //
        // Event method for EventReceived
        //
        public bool LogEventReceived(int EventId)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateEventId(ref this.EventReceived, EventId);
        }

        //
        // Event method for EventBuffered
        //
        public bool LogEventBuffered(int EventId, string SessionId, int BufferId, int Count, DateTime CreatedTime)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateEventIdPlusBuffer(ref this.EventBuffered, EventId, SessionId, BufferId, Count, CreatedTime);
        }

        //
        // Event method for BufferWritten
        //
        public bool LogBufferWritten(int BufferId, int Count, DateTime CreatedTime)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBuffer(ref this.BufferWritten, BufferId, Count, CreatedTime);
        }

        //
        // Event method for BufferDiscarded
        //
        public bool LogBufferDiscarded(int BufferId, int Count, DateTime CreatedTime, string Exception)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateBufferPlusException(ref this.BufferDiscarded, BufferId, Count, CreatedTime, Exception);
        }

        //
        // Event method for StartThrottle
        //
        public bool LogStartThrottle(int CurrentBufferSize)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateThrottle(ref this.StartThrottle, CurrentBufferSize);
        }

        //
        // Event method for StopThrottle
        //
        public bool LogStopThrottle(int CurrentBufferSize)
        {

            if (!this.m_provider.IsEnabled())
            {
                return true;
            }

            return this.m_provider.TemplateThrottle(ref this.StopThrottle, CurrentBufferSize);
        }
    }

    public class DataSvcTrace : IDisposable
    {
        //
        // Provider Microsoft-HPC-DataService Event Count 5
        //

        internal EventProviderVersionTwo m_provider = new EventProviderVersionTwo(new Guid("176564f1-d1f1-4e2d-938d-b04eb87dc2b7"));
        //
        // Task :  eventGUIDs
        //

        //
        // Event Descriptors
        //
        protected EventDescriptor TextVerbose;
        protected EventDescriptor TextInfo;
        protected EventDescriptor TextWarning;
        protected EventDescriptor TextError;
        protected EventDescriptor TextCritial;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_provider.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }


        public DataSvcTrace()
        {
            unchecked
            {
                this.TextVerbose = new EventDescriptor(0xa, 0x0, 0x12, 0x5, 0x0, 0x1, (long)0x2000000000000001);
                this.TextInfo = new EventDescriptor(0xb, 0x0, 0x12, 0x4, 0x0, 0x1, (long)0x2000000000000001);
                this.TextWarning = new EventDescriptor(0xc, 0x0, 0x12, 0x3, 0x0, 0x1, (long)0x2000000000000001);
                this.TextError = new EventDescriptor(0xd, 0x0, 0x11, 0x2, 0x0, 0x1, (long)0x4000000000000001);
                this.TextCritial = new EventDescriptor(0xe, 0x0, 0x10, 0x1, 0x0, 0x1, (long)0x8000000000000001);
            }
        }


        //
        // Event method for TextVerbose
        //
        public bool LogTextVerbose(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextVerbose, String);

        }

        //
        // Event method for TextInfo
        //
        public bool LogTextInfo(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextInfo, String);

        }

        //
        // Event method for TextWarning
        //
        public bool LogTextWarning(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextWarning, String);

        }

        //
        // Event method for TextError
        //
        public bool LogTextError(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextError, String);

        }

        //
        // Event method for TextCritial
        //
        public bool LogTextCritial(string String)
        {

            return this.m_provider.WriteEvent(ref this.TextCritial, String);

        }
    }

    internal class EventProviderVersionTwo : EventProvider
    {
         internal EventProviderVersionTwo(Guid id)
                : base(id)
         {}


        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private struct EventData
        {
            [FieldOffset(0)]
            internal UInt64 DataPointer;
            [FieldOffset(8)]
            internal uint Size;
            [FieldOffset(12)]
            internal int Reserved;
        }



        internal unsafe bool TemplateString(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string String
            )
        {
            int argumentCount = 2;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(String.Length + 1)*sizeof(char);

                fixed (char* a0 = SessionId, a1 = String)
                {
                    userDataPtr[0].DataPointer = (ulong)a0;
                    userDataPtr[1].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateMessage(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            Guid MessageId
            )
        {
            int argumentCount = 2;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].DataPointer = (UInt64)(&MessageId);
                userDataPtr[1].Size = (uint)(sizeof(Guid)  );

                fixed (char* a0 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateMessageWithFault(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            Guid MessageId,
            bool Fault
            )
        {
            int argumentCount = 3;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].DataPointer = (UInt64)(&MessageId);
                userDataPtr[1].Size = (uint)(sizeof(Guid)  );

                int FaultInt = Fault ? 1 : 0;
                userDataPtr[2].DataPointer = (UInt64)(&FaultInt);
                userDataPtr[2].Size = (uint)(sizeof(int));

                fixed (char* a0 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateSession(
            ref EventDescriptor eventDescriptor,
            string SessionId
            )
        {
            int argumentCount = 1;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                fixed (char* a0 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateFrontendRequestMessage(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string ClientId,
            Guid MessageId
            )
        {
            int argumentCount = 3;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(ClientId.Length + 1)*sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                fixed (char* a0 = ClientId, a1 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a1;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBackendRequestMessageWithEPR(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string TaskId,
            Guid MessageId,
            Guid DispatchId,
            string TargetMachine,
            string EndpointAddress
            )
        {
            int argumentCount = 6;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(TaskId.Length + 1) * sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                userDataPtr[3].DataPointer = (UInt64)(&DispatchId);
                userDataPtr[3].Size = (uint)(sizeof(Guid)  );

                userDataPtr[4].Size = (uint)(TargetMachine.Length + 1)*sizeof(char);

                userDataPtr[5].Size = (uint)(EndpointAddress.Length + 1)*sizeof(char);

                fixed (char* a0 = TaskId, a1 = TargetMachine, a2 = EndpointAddress, a3 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a3;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[4].DataPointer = (ulong)a1;
                    userDataPtr[5].DataPointer = (ulong)a2;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBackendResponseMessage(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string TaskId,
            Guid MessageId,
            bool Fault
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(TaskId.Length + 1) * sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                int FaultInt = Fault ? 1 : 0;
                userDataPtr[3].DataPointer = (UInt64)(&FaultInt);
                userDataPtr[3].Size = (uint)(sizeof(int));

                fixed (char* a0 = TaskId, a1 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a1;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBackendMessageWithException(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string TaskId,
            Guid MessageId,
            string Exception
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(TaskId.Length + 1) * sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                userDataPtr[3].Size = (uint)(Exception.Length + 1)*sizeof(char);

                fixed (char* a0 = TaskId, a1 = Exception, a2 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a2;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[3].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBackendResponseMessageWithExceptionAndRetryCount(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string TaskId,
            Guid MessageId,
            int RetryCount,
            string Exception
            )
        {
            int argumentCount = 5;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(TaskId.Length + 1) * sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                userDataPtr[3].DataPointer = (UInt64)(&RetryCount);
                userDataPtr[3].Size = (uint)(sizeof(int)  );

                userDataPtr[4].Size = (uint)(Exception.Length + 1)*sizeof(char);

                fixed (char* a0 = TaskId, a1 = Exception, a2 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a2;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[4].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateFrontendRequestMessageGeneralError(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string ClientId,
            Guid MessageId,
            string Exception
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(ClientId.Length + 1)*sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                userDataPtr[3].Size = (uint)(Exception.Length + 1)*sizeof(char);

                fixed (char* a0 = ClientId, a1 = Exception, a2 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a2;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[3].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBackendResponseMessageRetry(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string TaskId,
            Guid MessageId,
            int RetryCount
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(TaskId.Length + 1) * sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                userDataPtr[3].DataPointer = (UInt64)(&RetryCount);
                userDataPtr[3].Size = (uint)(sizeof(int)  );

                fixed (char* a1 = TaskId, a0 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a0;
                    userDataPtr[1].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBackendRequestMessage(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string TaskId,
            Guid MessageId
            )
        {
            int argumentCount = 3;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(TaskId.Length + 1) * sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                fixed (char* a1 = TaskId, a0 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a0;
                    userDataPtr[1].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }                
            }

            return status;

        }



        internal unsafe bool TemplateBackendGeneratedFaultReply(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string TaskId,
            Guid MessageId,
            string FaultReply
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(TaskId.Length + 1) * sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                userDataPtr[3].Size = (uint)(FaultReply.Length + 1)*sizeof(char);

                fixed (char* a0 = TaskId, a1 = FaultReply, a2 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a2;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[3].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateFrontendRequestMessageAuthenticationError(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string ClientId,
            Guid MessageId,
            string UserName
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(ClientId.Length + 1)*sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                userDataPtr[3].Size = (uint)(UserName.Length + 1)*sizeof(char);

                fixed (char* a0 = ClientId, a1 = UserName, a2 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a2;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[3].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBackendResponseMessageWithFaultCode(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string TaskId,
            Guid MessageId,
            string FaultCode
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(TaskId.Length + 1) * sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MessageId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                userDataPtr[3].Size = (uint)(FaultCode.Length + 1)*sizeof(char);

                fixed (char* a0 = TaskId, a1 = FaultCode, a2 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a2;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[3].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateSessionCreating(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string UserName
            )
        {
            int argumentCount = 2;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(UserName.Length + 1)*sizeof(char);

                fixed (char* a0 = UserName, a1 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a1;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBrokerWorkerMessage(
            ref EventDescriptor eventDescriptor,
            int Id,
            string Message
            )
        {
            int argumentCount = 2;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].DataPointer = (UInt64)(&Id);
                userDataPtr[0].Size = (uint)(sizeof(int));

                userDataPtr[1].Size = (uint)(Message.Length + 1)*sizeof(char);

                fixed (char* a0 = Message)
                {
                    userDataPtr[1].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBrokerWorkerPID(
            ref EventDescriptor eventDescriptor,
            int PID
            )
        {
            int argumentCount = 1;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].DataPointer = (UInt64)(&PID);
                userDataPtr[0].Size = (uint)(sizeof(int));

                status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
            }

            return status;

        }



        internal unsafe bool TemplateBrokerWorkerExitCode(
            ref EventDescriptor eventDescriptor,
            int PID,
            int ExitCode
            )
        {
            int argumentCount = 2;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].DataPointer = (UInt64)(&PID);
                userDataPtr[0].Size = (uint)(sizeof(int));

                userDataPtr[1].DataPointer = (UInt64)(&ExitCode);
                userDataPtr[1].Size = (uint)(sizeof(int)  );

                status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
            }

            return status;

        }



        internal unsafe bool TemplateBrokerClientId(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string ClientId
            )
        {
            int argumentCount = 2;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(ClientId.Length + 1)*sizeof(char);

                fixed (char* a0 = ClientId, a1 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a1;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBrokerClientIdWithState(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string ClientId,
            string State
            )
        {
            int argumentCount = 3;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(ClientId.Length + 1)*sizeof(char);

                userDataPtr[2].Size = (uint)(State.Length + 1)*sizeof(char);

                fixed (char* a0 = ClientId, a1 = State, a2 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a2;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[2].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateStringWithMessageId(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            Guid MessageId,
            Guid DispatchId,
            string String
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].DataPointer = (UInt64)(&MessageId);
                userDataPtr[1].Size = (uint)(sizeof(Guid)  );

                userDataPtr[2].DataPointer = (UInt64)(&DispatchId);
                userDataPtr[2].Size = (uint)(sizeof(Guid)  );

                userDataPtr[3].Size = (uint)(String.Length + 1)*sizeof(char);

                fixed (char* a0 = String, a1 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a1;
                    userDataPtr[3].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateSessionBindingInfo(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string Symbol,
            long MaxMessageSize,
            long ReceiveTimeout,
            long SendTimeout,
            string MessageVersion,
            string Scheme
            )
        {
            int argumentCount = 7;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(Symbol.Length + 1)*sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&MaxMessageSize);
                userDataPtr[2].Size = (uint)(sizeof(long)  );

                userDataPtr[3].DataPointer = (UInt64)(&ReceiveTimeout);
                userDataPtr[3].Size = (uint)(sizeof(long)  );

                userDataPtr[4].DataPointer = (UInt64)(&SendTimeout);
                userDataPtr[4].Size = (uint)(sizeof(long)  );

                userDataPtr[5].Size = (uint)(MessageVersion.Length + 1)*sizeof(char);

                userDataPtr[6].Size = (uint)(Scheme.Length + 1)*sizeof(char);

                fixed (char* a0 = Symbol, a1 = MessageVersion, a2 = Scheme, a3 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a3;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[5].DataPointer = (ulong)a1;
                    userDataPtr[6].DataPointer = (ulong)a2;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateSessionFrontendUri(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string Symbol,
            string FrontendUri
            )
        {
            int argumentCount = 3;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(Symbol.Length + 1)*sizeof(char);

                userDataPtr[2].Size = (uint)(FrontendUri.Length + 1)*sizeof(char);

                fixed (char* a0 = Symbol, a1 = FrontendUri, a2 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a2;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[2].DataPointer = (ulong)a1;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateSessionControllerUri(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string Symbol,
            string ControllerUri,
            string GetResponseUri
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(Symbol.Length + 1)*sizeof(char);

                userDataPtr[2].Size = (uint)(ControllerUri.Length + 1)*sizeof(char);

                userDataPtr[3].Size = (uint)(GetResponseUri.Length + 1)*sizeof(char);

                fixed (char* a0 = Symbol, a1 = ControllerUri, a2 = GetResponseUri, a3 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a3;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    userDataPtr[2].DataPointer = (ulong)a1;
                    userDataPtr[3].DataPointer = (ulong)a2;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateAzureStorageQueue(
            ref EventDescriptor eventDescriptor,
            string SessionId,
            string QueueName
            )
        {
            int argumentCount = 2;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[1].Size = (uint)(QueueName.Length + 1)*sizeof(char);

                fixed (char* a0 = QueueName, a1 = SessionId)
                {
                    userDataPtr[0].DataPointer = (ulong)a1;
                    userDataPtr[1].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateEventId(
            ref EventDescriptor eventDescriptor,
            int EventId
            )
        {
            int argumentCount = 1;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].DataPointer = (UInt64)(&EventId);
                userDataPtr[0].Size = (uint)(sizeof(int));

                status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
            }

            return status;

        }



        internal unsafe bool TemplateEventIdPlusBuffer(
            ref EventDescriptor eventDescriptor,
            int EventId,
            string SessionId,
            int BufferId,
            int Count,
            DateTime CreatedTime
            )
        {
            int argumentCount = 5;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].DataPointer = (UInt64)(&EventId);
                userDataPtr[0].Size = (uint)(sizeof(int));

                userDataPtr[1].Size = (uint)(SessionId.Length + 1) * sizeof(char);

                userDataPtr[2].DataPointer = (UInt64)(&BufferId);
                userDataPtr[2].Size = (uint)(sizeof(int)  );

                userDataPtr[3].DataPointer = (UInt64)(&Count);
                userDataPtr[3].Size = (uint)(sizeof(int)  );

                long CreatedTimeFileTime = CreatedTime.ToFileTime();
                userDataPtr[4].DataPointer = (UInt64)(&CreatedTimeFileTime);
                userDataPtr[4].Size = (uint)(sizeof(long));

                fixed (char* a0 = SessionId)
                {
                    userDataPtr[1].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateBuffer(
            ref EventDescriptor eventDescriptor,
            int BufferId,
            int Count,
            DateTime CreatedTime
            )
        {
            int argumentCount = 3;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].DataPointer = (UInt64)(&BufferId);
                userDataPtr[0].Size = (uint)(sizeof(int));

                userDataPtr[1].DataPointer = (UInt64)(&Count);
                userDataPtr[1].Size = (uint)(sizeof(int)  );

                long CreatedTimeFileTime = CreatedTime.ToFileTime();
                userDataPtr[2].DataPointer = (UInt64)(&CreatedTimeFileTime);
                userDataPtr[2].Size = (uint)(sizeof(long));

                status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
            }

            return status;

        }



        internal unsafe bool TemplateBufferPlusException(
            ref EventDescriptor eventDescriptor,
            int BufferId,
            int Count,
            DateTime CreatedTime,
            string Exception
            )
        {
            int argumentCount = 4;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].DataPointer = (UInt64)(&BufferId);
                userDataPtr[0].Size = (uint)(sizeof(int));

                userDataPtr[1].DataPointer = (UInt64)(&Count);
                userDataPtr[1].Size = (uint)(sizeof(int)  );

                long CreatedTimeFileTime = CreatedTime.ToFileTime();
                userDataPtr[2].DataPointer = (UInt64)(&CreatedTimeFileTime);
                userDataPtr[2].Size = (uint)(sizeof(long));

                userDataPtr[3].Size = (uint)(Exception.Length + 1)*sizeof(char);

                fixed (char* a0 = Exception)
                {
                    userDataPtr[3].DataPointer = (ulong)a0;
                    status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
                }
            }

            return status;

        }



        internal unsafe bool TemplateThrottle(
            ref EventDescriptor eventDescriptor,
            int CurrentBufferSize
            )
        {
            int argumentCount = 1;
            bool status = true;

            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                byte* userData = stackalloc byte[sizeof(EventData) * argumentCount];
                EventData* userDataPtr = (EventData*)userData;

                userDataPtr[0].DataPointer = (UInt64)(&CurrentBufferSize);
                userDataPtr[0].Size = (uint)(sizeof(int));

                status = this.WriteEvent(ref eventDescriptor, argumentCount, (IntPtr)(userData));
            }

            return status;

        }

    }

}
