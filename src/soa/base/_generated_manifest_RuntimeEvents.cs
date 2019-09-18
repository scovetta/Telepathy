namespace Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics
{
    using System;
    using Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics;
    using System.Diagnostics;
    
    
    public sealed class RuntimeTraceTemplateFormatterProvider : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatterProvider
    {
        
        private static System.Collections.Generic.Dictionary<int, Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter> formatterDic = new System.Collections.Generic.Dictionary<int, Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter>();
        
        static RuntimeTraceTemplateFormatterProvider()
        {
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(10, new StringFormatter("[Session:{1}] {2}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(11, new StringFormatter("[Session:{1}] {2}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(12, new StringFormatter("[Session:{1}] {2}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(13, new StringFormatter("[Session:{1}] {2}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(14, new StringFormatter("[Session:{1}] {2}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(10014, new PureStringFormatter("{1}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(10010, new PureStringFormatter("{1}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(10011, new PureStringFormatter("{1}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(10012, new PureStringFormatter("{1}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(10013, new PureStringFormatter("{1}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(1000, new MessageFormatter("Servicehost Get the request."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(1001, new MessageWithFaultFormatter("Servicehost end Process Message."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(1002, new SessionFormatter("Servicehost is started."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(1003, new SessionFormatter("Servicehost is started."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(1004, new ServiceConfigCheckFormatter("The file {1} is found. The file is discarded in Microsoft HPC Pack 2012. All cont" +
                        "ent is moved to service registration file."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(1005, new SessionFormatter("The service assembly is loaded."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(1006, new SessionFormatter("Host got canceled event."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3000, new FrontendRequestMessageFormatter("[Session:{1}] ClientId = {2}, MessageId = {3}, Frontend received request."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3001, new FrontendRequestMessageFormatter("[Session:{1}] ClientId = {2}, MessageId = {3}, Response has been sent to the clie" +
                        "nt."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3002, new BackendRequestMessageWithEPRFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, DispatchId = {4}, TargetMachine = {5" +
                        "}, Request has been dispatched to the service host. EPR = {6}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3003, new BackendResponseMessageFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Fault = {4}, Received response from " +
                        "service host."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3004, new BackendMessageWithExceptionFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Request has been failed to dispatch " +
                        "to the service host. Exception: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3005, new BackendResponseMessageWithExceptionAndRetryCountFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Failed to receive response from serv" +
                        "ice host. RetryCount = {4}, Exception: {5}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3006, new FrontendRequestMessageGeneralErrorFormatter("[Session:{1}] ClientId = {2}, MessageId = {3}, Response has been failed to send t" +
                        "o the client. Exception: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3007, new FrontendRequestMessageFormatter("[Session:{1}] ClientId = {2}, MessageId = {3}, Frontend rejected request because " +
                        "client id does match."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3008, new FrontendRequestMessageFormatter("[Session:{1}] ClientId = {2}, MessageId = {3}, Frontend rejected request because " +
                        "client id is invalid or too long."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3009, new FrontendRequestMessageFormatter("[Session:{1}] ClientId = {2}, MessageId = {3}, Frontend silently rejected request" +
                        " because corresponding client is not accepting requests."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3010, new BackendResponseMessageRetryFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Retry operation error received from " +
                        "service host, RetryCount = {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3011, new BackendResponseMessageRetryFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Retry limit exceed, Limit = {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3012, new BackendRequestMessageFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Put request back into broker queue."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3013, new BackendGeneratedFaultReplyFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Generated fault reply message: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3014, new FrontendRequestMessageAuthenticationErrorFormatter("[Session:{1}] ClientId = {2}, MessageId = {3}, Frontend rejected request from use" +
                        "r {4}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3015, new FrontendRequestMessageGeneralErrorFormatter("[Session:{1}] ClientId = {2}, MessageId = {3}, Frontend rejected request. Excepti" +
                        "on: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3016, new BackendMessageWithExceptionFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Failed to handle response. Exception" +
                        ": {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3017, new BackendMessageWithExceptionFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Failed to handle EndpointNotFoundExc" +
                        "eption. Exception: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3018, new BackendMessageWithExceptionFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Failed to handle exception. Exceptio" +
                        "n: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3019, new BackendMessageWithExceptionFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, EndpointNotFoundException occured wh" +
                        "ile trying to process request: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3020, new BackendRequestMessageFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Dispatcher is closing. Need to put r" +
                        "equest back to broker queue."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3021, new BackendResponseMessageWithFaultCodeFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Response is a session fault, fault a" +
                        "ction = {4}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3022, new BackendRequestMessageFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, The underlying connection is invalid" +
                        ". Need to put request back to broker queue."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(3023, new BackendResponseMessageFormatter("[Session:{1}] TaskId = {2}, MessageId = {3}, Put response back into broker queue." +
                        ""));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4000, new SessionCreatingFormatter("Session {1} is being created by user {2}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4001, new SessionFormatter("Session {1} has been sucessfully created."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4002, new SessionFormatter("Failed to create session {1}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4003, new SessionFormatter("Session {1} is finished."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4004, new SessionFormatter("Session {1} is finished because corresponding service job is failed or canceled."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4005, new SessionFormatter("Session {1} is timed out and finished."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4006, new SessionFormatter("Session {1} is suspended."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4007, new SessionFormatter("Session {1} is suspended because corresponding service job is failed or canceled." +
                        ""));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4008, new SessionFormatter("Session {1} is timed out and suspended."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4009, new SessionCreatingFormatter("Session {1} has been raised up by user {2}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(4010, new SessionFormatter("Session {1} has been raised up because of failover."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(5000, new BrokerWorkerMessageFormatter("[Broker Worker {1}] BrokerWorker exists unexpectedly. {2}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(5001, new BrokerWorkerMessageFormatter("[Broker Worker {1}] {2}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(5002, new BrokerWorkerPIDFormatter("Broker worker process is ready. PID = {1}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(5003, new BrokerWorkerExitCodeFormatter("Broker worker process exited. PID = {1}, ExitCode = {2}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(5004, new BrokerWorkerPIDFormatter("Broker worker process failed to initialize. PID = {1}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(7000, new BrokerClientIdFormatter("[Session:{1}] ClientId = {2}, Client created."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(7001, new BrokerClientIdWithStateFormatter("[Session:{1}] ClientId = {2}, State ==> {3}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(7002, new BrokerClientIdWithStateFormatter("[Session:{1}] ClientId = {2}, Reject Flush because the state is not ClientConnect" +
                        "ed, state is {3}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(7003, new BrokerClientIdWithStateFormatter("[Session:{1}] ClientId = {2}, Reject EOM because the state is not ClientConnected" +
                        ", state is {3}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(7004, new BrokerClientIdFormatter("[Session:{1}] ClientId = {2}, Client created."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(7005, new BrokerClientIdFormatter("[Session:{1}] ClientId = {2}, All responses dispatched, send EndOfResponse messag" +
                        "e."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(7006, new BrokerClientIdFormatter("[Session:{1}] ClientId = {2}, All request done."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(7007, new BrokerClientIdWithStateFormatter("[Session:{1}] ClientId = {2}, Client disconnected because {3}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(9000, new StringWithMessageIdFormatter("[Session:{1}] MessageId = {2}, DispatchId = {3}: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(9001, new StringWithMessageIdFormatter("[Session:{1}] MessageId = {2}, DispatchId = {3}: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(9002, new StringWithMessageIdFormatter("[Session:{1}] MessageId = {2}, DispatchId = {3}: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(9003, new StringWithMessageIdFormatter("[Session:{1}] MessageId = {2}, DispatchId = {3}: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(9004, new StringWithMessageIdFormatter("[Session:{1}] MessageId = {2}, DispatchId = {3}: {4}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(11000, new SessionBindingInfoFormatter("[Session:{1}] {2} frontend binding loaded.\nMaxMessageSize = {3}%ReceiveTimeout = " +
                        "{4}\nSendTimeout = {5}\nMessageVersion = {6}\nScheme={7}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(11001, new SessionBindingInfoFormatter("[Session:{1}] {2} backend binding loaded.\nMaxMessageSize = {3}%ReceiveTimeout = {" +
                        "4}\nSendTimeout = {5}\nMessageVersion = {6}\nScheme={7}"));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(11002, new SessionFrontendUriFormatter("[Session:{1}] {2} frontend created, Uri = {3}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(11003, new SessionControllerUriFormatter("[Session:{1}] {2} frontend controller created, ControllerUri = {3}, GetResponseUr" +
                        "i = {4}."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(12000, new AzureStorageQueueFormatter("[Session:{1}] Azure storage queue {2} has been sucessfully created or already exi" +
                        "sts."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(12001, new AzureStorageQueueFormatter("[Session:{1}] Azure storage queue {2} has been sucessfully deleted."));
            RuntimeTraceTemplateFormatterProvider.formatterDic.Add(12002, new AzureStorageQueueFormatter("[Session:{1}] Azure storage queue {2} does not exist."));
        }
        
        protected override System.Collections.Generic.IDictionary<int, Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter> TemplateFormatterDic
        {
            get
            {
                return RuntimeTraceTemplateFormatterProvider.formatterDic;
            }
        }
        
        private sealed class BrokerClientIdFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BrokerClientIdFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("ClientId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class BrokerClientIdWithStateFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BrokerClientIdWithStateFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("ClientId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("State", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class SessionCreatingFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public SessionCreatingFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("UserName", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class MessageFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public MessageFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                return result;
            }
        }
        
        private sealed class MessageWithFaultFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public MessageWithFaultFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("Fault", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadBoolean(pointer, offset));
                offset = (offset + 1);
                return result;
            }
        }
        
        private sealed class ServiceConfigCheckFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public ServiceConfigCheckFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("FileName", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class StringWithMessageIdFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public StringWithMessageIdFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("DispatchId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("String", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class StringFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public StringFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("String", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class PureStringFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public PureStringFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("String", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class DispatcherFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public DispatcherFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("TaskId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                return result;
            }
        }
        
        private sealed class FrontendRequestMessageFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public FrontendRequestMessageFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("ClientId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                return result;
            }
        }
        
        private sealed class FrontendRequestMessageAuthenticationErrorFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public FrontendRequestMessageAuthenticationErrorFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("ClientId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("UserName", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class FrontendRequestMessageGeneralErrorFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public FrontendRequestMessageGeneralErrorFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("ClientId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("Exception", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class BackendRequestMessageFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BackendRequestMessageFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("TaskId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                return result;
            }
        }
        
        private sealed class BackendRequestMessageWithEPRFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BackendRequestMessageWithEPRFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("TaskId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("DispatchId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("TargetMachine", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("EndpointAddress", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class BackendGeneratedFaultReplyFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BackendGeneratedFaultReplyFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("TaskId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("FaultReply", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class BackendResponseMessageWithExceptionAndRetryCountFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BackendResponseMessageWithExceptionAndRetryCountFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("TaskId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("RetryCount", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("Exception", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class BackendResponseMessageFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BackendResponseMessageFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("TaskId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("Fault", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadBoolean(pointer, offset));
                offset = (offset + 1);
                return result;
            }
        }
        
        private sealed class BackendResponseMessageWithFaultCodeFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BackendResponseMessageWithFaultCodeFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("TaskId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("FaultCode", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class BackendResponseMessageRetryFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BackendResponseMessageRetryFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("TaskId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("RetryCount", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                return result;
            }
        }
        
        private sealed class BackendMessageWithExceptionFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BackendMessageWithExceptionFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("TaskId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("MessageId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadGuid(pointer, offset));
                offset = (offset + 16);
                result.Add("Exception", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class SessionFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public SessionFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                return result;
            }
        }
        
        private sealed class SessionHostUriFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public SessionHostUriFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("HostUri", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class SessionFrontendUriFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public SessionFrontendUriFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("Symbol", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("FrontendUri", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class SessionControllerUriFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public SessionControllerUriFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("Symbol", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("ControllerUri", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("GetResponseUri", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class SessionBindingInfoFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public SessionBindingInfoFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("Symbol", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("MaxMessageSize", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt64(pointer, offset));
                offset = (offset + 8);
                result.Add("ReceiveTimeout", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt64(pointer, offset));
                offset = (offset + 8);
                result.Add("SendTimeout", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt64(pointer, offset));
                offset = (offset + 8);
                result.Add("MessageVersion", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                result.Add("Scheme", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class BrokerWorkerMessageFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BrokerWorkerMessageFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("Id", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("Message", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class BrokerWorkerExitCodeFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BrokerWorkerExitCodeFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("PID", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("ExitCode", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                return result;
            }
        }
        
        private sealed class BrokerWorkerPIDFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BrokerWorkerPIDFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("PID", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                return result;
            }
        }
        
        private sealed class AzureStorageQueueFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public AzureStorageQueueFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("QueueName", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
    }
    
    public sealed class RuntimeTraceEventIdConst
    {
        
        public const ushort TextVerboseEventId = 10;
        
        public const ushort TextInfoEventId = 11;
        
        public const ushort TextWarningEventId = 12;
        
        public const ushort TextErrorEventId = 13;
        
        public const ushort TextCritialEventId = 14;
        
        public const ushort EventTextCritialEventId = 10014;
        
        public const ushort EventTextVerboseEventId = 10010;
        
        public const ushort EventTextInfoEventId = 10011;
        
        public const ushort EventTextWarningEventId = 10012;
        
        public const ushort EventTextErrorEventId = 10013;
        
        public const ushort HostRequestReceivedEventId = 1000;
        
        public const ushort HostResponseSentEventId = 1001;
        
        public const ushort HostStartEventId = 1002;
        
        public const ushort HostStopEventId = 1003;
        
        public const ushort HostServiceConfigCheckEventId = 1004;
        
        public const ushort HostAssemblyLoadedEventId = 1005;
        
        public const ushort HostCanceledEventId = 1006;
        
        public const ushort FrontEndRequestReceivedEventId = 3000;
        
        public const ushort FrontEndResponseSentEventId = 3001;
        
        public const ushort BackendRequestSentEventId = 3002;
        
        public const ushort BackendResponseReceivedEventId = 3003;
        
        public const ushort BackendRequestSentFailedEventId = 3004;
        
        public const ushort BackendResponseReceivedFailedEventId = 3005;
        
        public const ushort FrontEndResponseSentFailedEventId = 3006;
        
        public const ushort FrontEndRequestRejectedClientIdNotMatchEventId = 3007;
        
        public const ushort FrontEndRequestRejectedClientIdInvalidEventId = 3008;
        
        public const ushort FrontEndRequestRejectedClientStateInvalidEventId = 3009;
        
        public const ushort BackendResponseReceivedRetryOperationErrorEventId = 3010;
        
        public const ushort BackendResponseReceivedRetryLimitExceedEventId = 3011;
        
        public const ushort BackendRequestPutBackEventId = 3012;
        
        public const ushort BackendGeneratedFaultReplyEventId = 3013;
        
        public const ushort FrontEndRequestRejectedAuthenticationErrorEventId = 3014;
        
        public const ushort FrontEndRequestRejectedGeneralErrorEventId = 3015;
        
        public const ushort BackendHandleResponseFailedEventId = 3016;
        
        public const ushort BackendHandleEndpointNotFoundExceptionFailedEventId = 3017;
        
        public const ushort BackendHandleExceptionFailedEventId = 3018;
        
        public const ushort BackendEndpointNotFoundExceptionOccuredEventId = 3019;
        
        public const ushort BackendDispatcherClosedEventId = 3020;
        
        public const ushort BackendResponseReceivedSessionFaultEventId = 3021;
        
        public const ushort BackendValidateClientFailedEventId = 3022;
        
        public const ushort BackendResponseStoredEventId = 3023;
        
        public const ushort SessionCreatingEventId = 4000;
        
        public const ushort SessionCreatedEventId = 4001;
        
        public const ushort FailedToCreateSessionEventId = 4002;
        
        public const ushort SessionFinishedEventId = 4003;
        
        public const ushort SessionFinishedBecauseOfJobCanceledEventId = 4004;
        
        public const ushort SessionFinishedBecauseOfTimeoutEventId = 4005;
        
        public const ushort SessionSuspendedEventId = 4006;
        
        public const ushort SessionSuspendedBecauseOfJobCanceledEventId = 4007;
        
        public const ushort SessionSuspendedBecauseOfTimeoutEventId = 4008;
        
        public const ushort SessionRaisedUpEventId = 4009;
        
        public const ushort SessionRaisedUpFailoverEventId = 4010;
        
        public const ushort BrokerWorkerUnexpectedlyExitEventId = 5000;
        
        public const ushort BrokerWorkerMessageEventId = 5001;
        
        public const ushort BrokerWorkerProcessReadyEventId = 5002;
        
        public const ushort BrokerWorkerProcessExitedEventId = 5003;
        
        public const ushort BrokerWorkerProcessFailedToInitializeEventId = 5004;
        
        public const ushort BrokerClientCreatedEventId = 7000;
        
        public const ushort BrokerClientStateTransitionEventId = 7001;
        
        public const ushort BrokerClientRejectFlushEventId = 7002;
        
        public const ushort BrokerClientRejectEOMEventId = 7003;
        
        public const ushort BrokerClientTimedOutEventId = 7004;
        
        public const ushort BrokerClientAllResponseDispatchedEventId = 7005;
        
        public const ushort BrokerClientAllRequestDoneEventId = 7006;
        
        public const ushort BrokerClientDisconnectedEventId = 7007;
        
        public const ushort UserTraceVerboseEventId = 9000;
        
        public const ushort UserTraceInfoEventId = 9001;
        
        public const ushort UserTraceWarningEventId = 9002;
        
        public const ushort UserTraceErrorEventId = 9003;
        
        public const ushort UserTraceCritialEventId = 9004;
        
        public const ushort FrontendBindingLoadedEventId = 11000;
        
        public const ushort BackendBindingLoadedEventId = 11001;
        
        public const ushort FrontendCreatedEventId = 11002;
        
        public const ushort FrontendControllerCreatedEventId = 11003;
        
        public const ushort QueueCreatedOrExistEventId = 12000;
        
        public const ushort QueueDeletedEventId = 12001;
        
        public const ushort QueueNotExistEventId = 12002;
        
        private RuntimeTraceEventIdConst()
        {
        }
    }
    
    public sealed class RuntimeTraceTaskIdConst
    {
        
        public const ushort DebugTraceTaskId = 1;
        
        public const ushort SessionLifecycleTaskId = 2;
        
        public const ushort BrokerClientLifecycleTaskId = 3;
        
        public const ushort BrokerCreationInfoTaskId = 11;
        
        public const ushort ServiceHostTaskId = 6;
        
        public const ushort RequestProcessingTaskId = 7;
        
        public const ushort BrokerWorkerTaskId = 8;
        
        public const ushort UserTraceTaskId = 9;
        
        public const ushort HpcSessionTraceTaskId = 10;
        
        public const ushort QueueLifecycleTaskId = 12;
        
        private RuntimeTraceTaskIdConst()
        {
        }
    }
    
    public sealed class SOADiagTraceTemplateFormatterProvider : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatterProvider
    {
        
        private static System.Collections.Generic.Dictionary<int, Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter> formatterDic = new System.Collections.Generic.Dictionary<int, Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter>();
        
        static SOADiagTraceTemplateFormatterProvider()
        {
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(10, new SOADiagStringFormatter("{1}"));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(11, new SOADiagStringFormatter("{1}"));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(12, new SOADiagStringFormatter("{1}"));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(13, new SOADiagStringFormatter("{1}"));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(14, new SOADiagStringFormatter("{1}"));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(1000, new EventIdFormatter("Trace event (EventId = {1}) has been received. "));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(1001, new EventIdPlusBufferFormatter("Trace event (EventId = {1}) for session {2} has been buffered in buffer {3}. Numb" +
                        "erOfEventsInBuffer = {4}, BufferCreatedTime = {5}."));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(1002, new BufferFormatter("Buffer {1} has been written into storage. NumberOfEventsInBuffer = {2}, BufferCre" +
                        "atedTime = {3}."));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(1003, new BufferPlusExceptionFormatter("Buffer {1} has been discarded. NumberOfEventsInBuffer = {2}, BufferCreatedTime = " +
                        "{3}. Exception = {4}."));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(2000, new ThrottleFormatter("Start throttling. Current buffer size is {1} bytes."));
            SOADiagTraceTemplateFormatterProvider.formatterDic.Add(2001, new ThrottleFormatter("Stop throttling. Current buffer size is {1} bytes."));
        }
        
        protected override System.Collections.Generic.IDictionary<int, Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter> TemplateFormatterDic
        {
            get
            {
                return SOADiagTraceTemplateFormatterProvider.formatterDic;
            }
        }
        
        private sealed class SOADiagStringFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public SOADiagStringFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("String", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
        
        private sealed class EventIdFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public EventIdFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("EventId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                return result;
            }
        }
        
        private sealed class ThrottleFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public ThrottleFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("CurrentBufferSize", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                return result;
            }
        }
        
        private sealed class EventIdPlusBufferFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public EventIdPlusBufferFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("EventId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("SessionId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("BufferId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("Count", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("CreatedTime", System.DateTime.FromFileTimeUtc(Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt64(pointer, offset)));
                offset = (offset + 8);
                return result;
            }
        }
        
        private sealed class BufferFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BufferFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("BufferId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("Count", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("CreatedTime", System.DateTime.FromFileTimeUtc(Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt64(pointer, offset)));
                offset = (offset + 8);
                return result;
            }
        }
        
        private sealed class BufferPlusExceptionFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public BufferPlusExceptionFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("BufferId", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("Count", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt32(pointer, offset));
                offset = (offset + 4);
                result.Add("CreatedTime", System.DateTime.FromFileTimeUtc(Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadInt64(pointer, offset)));
                offset = (offset + 8);
                result.Add("Exception", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
    }
    
    public sealed class SOADiagTraceEventIdConst
    {
        
        public const ushort TextVerboseEventId = 10;
        
        public const ushort TextInfoEventId = 11;
        
        public const ushort TextWarningEventId = 12;
        
        public const ushort TextErrorEventId = 13;
        
        public const ushort TextCritialEventId = 14;
        
        public const ushort EventReceivedEventId = 1000;
        
        public const ushort EventBufferedEventId = 1001;
        
        public const ushort BufferWrittenEventId = 1002;
        
        public const ushort BufferDiscardedEventId = 1003;
        
        public const ushort StartThrottleEventId = 2000;
        
        public const ushort StopThrottleEventId = 2001;
        
        private SOADiagTraceEventIdConst()
        {
        }
    }
    
    public sealed class SOADiagTraceTaskIdConst
    {
        
        public const ushort DebugTraceTaskId = 1;
        
        public const ushort EventLifecycleTaskId = 2;
        
        public const ushort ThrottlingTaskId = 3;
        
        public const ushort DiagMonLifecycleTaskId = 4;
        
        private SOADiagTraceTaskIdConst()
        {
        }
    }
    
    public sealed class DataSvcTraceTemplateFormatterProvider : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatterProvider
    {
        
        private static System.Collections.Generic.Dictionary<int, Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter> formatterDic = new System.Collections.Generic.Dictionary<int, Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter>();
        
        static DataSvcTraceTemplateFormatterProvider()
        {
            DataSvcTraceTemplateFormatterProvider.formatterDic.Add(10, new PureStringFormatter("{1}"));
            DataSvcTraceTemplateFormatterProvider.formatterDic.Add(11, new PureStringFormatter("{1}"));
            DataSvcTraceTemplateFormatterProvider.formatterDic.Add(12, new PureStringFormatter("{1}"));
            DataSvcTraceTemplateFormatterProvider.formatterDic.Add(13, new PureStringFormatter("{1}"));
            DataSvcTraceTemplateFormatterProvider.formatterDic.Add(14, new PureStringFormatter("{1}"));
        }
        
        protected override System.Collections.Generic.IDictionary<int, Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter> TemplateFormatterDic
        {
            get
            {
                return DataSvcTraceTemplateFormatterProvider.formatterDic;
            }
        }
        
        private sealed class PureStringFormatter : Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TemplateFormatter
        {
            
            private string message;
            
            public PureStringFormatter(string message)
            {
                this.message = message;
            }
            
            protected override string Message
            {
                get
                {
                    return this.message;
                }
            }
            
            protected override System.Collections.Generic.Dictionary<string, object> ParseBinaryDataInternal(System.IntPtr pointer)
            {
                System.Collections.Generic.Dictionary<string, object> result = new System.Collections.Generic.Dictionary<string, object>();
                int offset = 0;
                result.Add("String", Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.ReadUnicodeString(pointer, offset));
                offset = Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics.TraceEventRawReaders.SkipUnicodeString(pointer, offset);
                return result;
            }
        }
    }
    
    public sealed class DataSvcTraceEventIdConst
    {
        
        public const ushort TextVerboseEventId = 10;
        
        public const ushort TextInfoEventId = 11;
        
        public const ushort TextWarningEventId = 12;
        
        public const ushort TextErrorEventId = 13;
        
        public const ushort TextCritialEventId = 14;
        
        private DataSvcTraceEventIdConst()
        {
        }
    }
    
    public sealed class DataSvcTraceTaskIdConst
    {
        
        public const ushort DebugTraceTaskId = 1;
        
        private DataSvcTraceTaskIdConst()
        {
        }
    }
}
namespace Microsoft.Hpc.RuntimeTrace
{
    using System;
    using System.Diagnostics;
    using Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics;
    
    
    public sealed class RuntimeTraceWrapper
    {
        
        private const string TraceSourceNameForCosmos = "HpcSoa";
        
        private System.Diagnostics.TraceSource cosmosTrace = new System.Diagnostics.TraceSource(TraceSourceNameForCosmos);
        
        private RuntimeTrace etwTrace = new RuntimeTrace();
        
        private IsDiagTraceEnabledDelegate isDiagTraceEnabled;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="We allow caller to change the trace level.")]
        public RuntimeTraceWrapper()
        {
            this.cosmosTrace.Switch.Level = SourceLevels.All;
        }
        
        public System.Diagnostics.TraceSource CosmosTrace
        {
            get
            {
                return this.cosmosTrace;
            }
        }
        
        public IsDiagTraceEnabledDelegate IsDiagTraceEnabled
        {
            get
            {
                return this.isDiagTraceEnabled;
            }
            set
            {
                this.isDiagTraceEnabled = value;
            }
        }
        
        private bool IsTraceEnabled(string sessionId)
        {
            if ((this.IsDiagTraceEnabled != null))
            {
                return this.IsDiagTraceEnabled(sessionId);
            }
            else
            {
                return false;
            }
        }
        
        public bool LogTextVerbose(string SessionId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.TextVerboseEventId, LogStringFormatTable.StringMessage, SessionId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogTextVerbose(SessionId, String);
        }
        
        public bool LogTextInfo(string SessionId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.TextInfoEventId, LogStringFormatTable.StringMessage, SessionId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogTextInfo(SessionId, String);
        }
        
        public bool LogTextWarning(string SessionId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.TextWarningEventId, LogStringFormatTable.StringMessage, SessionId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogTextWarning(SessionId, String);
        }
        
        public bool LogTextError(string SessionId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, RuntimeTraceEventIdConst.TextErrorEventId, LogStringFormatTable.StringMessage, SessionId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogTextError(SessionId, String);
        }
        
        public bool LogTextCritial(string SessionId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Critical, RuntimeTraceEventIdConst.TextCritialEventId, LogStringFormatTable.StringMessage, SessionId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogTextCritial(SessionId, String);
        }
        
        public bool LogEventTextCritial(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Critical, RuntimeTraceEventIdConst.EventTextCritialEventId, LogStringFormatTable.PureString, String);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogEventTextCritial(String);
        }
        
        public bool LogEventTextVerbose(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.EventTextVerboseEventId, LogStringFormatTable.PureString, String);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogEventTextVerbose(String);
        }
        
        public bool LogEventTextInfo(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.EventTextInfoEventId, LogStringFormatTable.PureString, String);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogEventTextInfo(String);
        }
        
        public bool LogEventTextWarning(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.EventTextWarningEventId, LogStringFormatTable.PureString, String);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogEventTextWarning(String);
        }
        
        public bool LogEventTextError(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, RuntimeTraceEventIdConst.EventTextErrorEventId, LogStringFormatTable.PureString, String);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogEventTextError(String);
        }
        
        public bool LogHostRequestReceived(string SessionId, System.Guid MessageId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.HostRequestReceivedEventId, LogStringFormatTable.HostRequestReceived, SessionId, MessageId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogHostRequestReceived(SessionId, MessageId);
        }
        
        public bool LogHostResponseSent(string SessionId, System.Guid MessageId, bool Fault)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.HostResponseSentEventId, LogStringFormatTable.HostResponseSent, SessionId, MessageId, Fault);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogHostResponseSent(SessionId, MessageId, Fault);
        }
        
        public bool LogHostStart(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.HostStartEventId, LogStringFormatTable.HostStart, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogHostStart(SessionId);
        }
        
        public bool LogHostStop(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.HostStopEventId, LogStringFormatTable.HostStart, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogHostStop(SessionId);
        }
        
        public bool LogHostServiceConfigCheck(string FileName)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.HostServiceConfigCheckEventId, LogStringFormatTable.HostServiceConfigCheck, FileName);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogHostServiceConfigCheck(FileName);
        }
        
        public bool LogHostAssemblyLoaded(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.HostAssemblyLoadedEventId, LogStringFormatTable.HostAssemblyLoaded, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogHostAssemblyLoaded(SessionId);
        }
        
        public bool LogHostCanceled(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.HostCanceledEventId, LogStringFormatTable.HostCanceled, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogHostCanceled(SessionId);
        }
        
        public bool LogFrontEndRequestReceived(string SessionId, string ClientId, System.Guid MessageId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.FrontEndRequestReceivedEventId, LogStringFormatTable.FrontEndRequestReceived, SessionId, ClientId, MessageId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontEndRequestReceived(SessionId, ClientId, MessageId);
        }
        
        public bool LogFrontEndResponseSent(string SessionId, string ClientId, System.Guid MessageId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.FrontEndResponseSentEventId, LogStringFormatTable.FrontEndResponseSent, SessionId, ClientId, MessageId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontEndResponseSent(SessionId, ClientId, MessageId);
        }
        
        public bool LogBackendRequestSent(string SessionId, string TaskId, System.Guid MessageId, System.Guid DispatchId, string TargetMachine, string EndpointAddress)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.BackendRequestSentEventId, LogStringFormatTable.BackendRequestSent, SessionId, TaskId, MessageId, DispatchId, TargetMachine, EndpointAddress);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendRequestSent(SessionId, TaskId, MessageId, DispatchId, TargetMachine, EndpointAddress);
        }
        
        public bool LogBackendResponseReceived(string SessionId, string TaskId, System.Guid MessageId, bool Fault)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.BackendResponseReceivedEventId, LogStringFormatTable.BackendResponseReceived, SessionId, TaskId, MessageId, Fault);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendResponseReceived(SessionId, TaskId, MessageId, Fault);
        }
        
        public bool LogBackendRequestSentFailed(string SessionId, string TaskId, System.Guid MessageId, string Exception)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BackendRequestSentFailedEventId, LogStringFormatTable.BackendRequestSentFailed, SessionId, TaskId, MessageId, Exception);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendRequestSentFailed(SessionId, TaskId, MessageId, Exception);
        }
        
        public bool LogBackendResponseReceivedFailed(string SessionId, string TaskId, System.Guid MessageId, int RetryCount, string Exception)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BackendResponseReceivedFailedEventId, LogStringFormatTable.BackendResponseReceivedFailed, SessionId, TaskId, MessageId, RetryCount, Exception);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendResponseReceivedFailed(SessionId, TaskId, MessageId, RetryCount, Exception);
        }
        
        public bool LogFrontEndResponseSentFailed(string SessionId, string ClientId, System.Guid MessageId, string Exception)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.FrontEndResponseSentFailedEventId, LogStringFormatTable.FrontEndResponseSentFailed, SessionId, ClientId, MessageId, Exception);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontEndResponseSentFailed(SessionId, ClientId, MessageId, Exception);
        }
        
        public bool LogFrontEndRequestRejectedClientIdNotMatch(string SessionId, string ClientId, System.Guid MessageId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.FrontEndRequestRejectedClientIdNotMatchEventId, LogStringFormatTable.FrontEndRequestRejectedClientIdNotMatch, SessionId, ClientId, MessageId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontEndRequestRejectedClientIdNotMatch(SessionId, ClientId, MessageId);
        }
        
        public bool LogFrontEndRequestRejectedClientIdInvalid(string SessionId, string ClientId, System.Guid MessageId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.FrontEndRequestRejectedClientIdInvalidEventId, LogStringFormatTable.FrontEndRequestRejectedClientIdInvalid, SessionId, ClientId, MessageId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontEndRequestRejectedClientIdInvalid(SessionId, ClientId, MessageId);
        }
        
        public bool LogFrontEndRequestRejectedClientStateInvalid(string SessionId, string ClientId, System.Guid MessageId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.FrontEndRequestRejectedClientStateInvalidEventId, LogStringFormatTable.FrontEndRequestRejectedClientStateInvalid, SessionId, ClientId, MessageId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontEndRequestRejectedClientStateInvalid(SessionId, ClientId, MessageId);
        }
        
        public bool LogBackendResponseReceivedRetryOperationError(string SessionId, string TaskId, System.Guid MessageId, int RetryCount)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BackendResponseReceivedRetryOperationErrorEventId, LogStringFormatTable.BackendResponseReceivedRetryOperationError, SessionId, TaskId, MessageId, RetryCount);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendResponseReceivedRetryOperationError(SessionId, TaskId, MessageId, RetryCount);
        }
        
        public bool LogBackendResponseReceivedRetryLimitExceed(string SessionId, string TaskId, System.Guid MessageId, int RetryCount)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BackendResponseReceivedRetryLimitExceedEventId, LogStringFormatTable.BackendResponseReceivedRetryLimitExceed, SessionId, TaskId, MessageId, RetryCount);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendResponseReceivedRetryLimitExceed(SessionId, TaskId, MessageId, RetryCount);
        }
        
        public bool LogBackendRequestPutBack(string SessionId, string TaskId, System.Guid MessageId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.BackendRequestPutBackEventId, LogStringFormatTable.BackendRequestPutBack, SessionId, TaskId, MessageId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendRequestPutBack(SessionId, TaskId, MessageId);
        }
        
        public bool LogBackendGeneratedFaultReply(string SessionId, string TaskId, System.Guid MessageId, string FaultReply)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.BackendGeneratedFaultReplyEventId, LogStringFormatTable.BackendGeneratedFaultReply, SessionId, TaskId, MessageId, FaultReply);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendGeneratedFaultReply(SessionId, TaskId, MessageId, FaultReply);
        }
        
        public bool LogFrontEndRequestRejectedAuthenticationError(string SessionId, string ClientId, System.Guid MessageId, string UserName)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.FrontEndRequestRejectedAuthenticationErrorEventId, LogStringFormatTable.FrontEndRequestRejectedAuthenticationError, SessionId, ClientId, MessageId, UserName);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontEndRequestRejectedAuthenticationError(SessionId, ClientId, MessageId, UserName);
        }
        
        public bool LogFrontEndRequestRejectedGeneralError(string SessionId, string ClientId, System.Guid MessageId, string Exception)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.FrontEndRequestRejectedGeneralErrorEventId, LogStringFormatTable.FrontEndRequestRejectedGeneralError, SessionId, ClientId, MessageId, Exception);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontEndRequestRejectedGeneralError(SessionId, ClientId, MessageId, Exception);
        }
        
        public bool LogBackendHandleResponseFailed(string SessionId, string TaskId, System.Guid MessageId, string Exception)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, RuntimeTraceEventIdConst.BackendHandleResponseFailedEventId, LogStringFormatTable.BackendHandleResponseFailed, SessionId, TaskId, MessageId, Exception);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendHandleResponseFailed(SessionId, TaskId, MessageId, Exception);
        }
        
        public bool LogBackendHandleEndpointNotFoundExceptionFailed(string SessionId, string TaskId, System.Guid MessageId, string Exception)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, RuntimeTraceEventIdConst.BackendHandleEndpointNotFoundExceptionFailedEventId, LogStringFormatTable.BackendHandleEndpointNotFoundExceptionFailed, SessionId, TaskId, MessageId, Exception);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendHandleEndpointNotFoundExceptionFailed(SessionId, TaskId, MessageId, Exception);
        }
        
        public bool LogBackendHandleExceptionFailed(string SessionId, string TaskId, System.Guid MessageId, string Exception)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, RuntimeTraceEventIdConst.BackendHandleExceptionFailedEventId, LogStringFormatTable.BackendHandleExceptionFailed, SessionId, TaskId, MessageId, Exception);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendHandleExceptionFailed(SessionId, TaskId, MessageId, Exception);
        }
        
        public bool LogBackendEndpointNotFoundExceptionOccured(string SessionId, string TaskId, System.Guid MessageId, string Exception)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BackendEndpointNotFoundExceptionOccuredEventId, LogStringFormatTable.BackendEndpointNotFoundExceptionOccured, SessionId, TaskId, MessageId, Exception);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendEndpointNotFoundExceptionOccured(SessionId, TaskId, MessageId, Exception);
        }
        
        public bool LogBackendDispatcherClosed(string SessionId, string TaskId, System.Guid MessageId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BackendDispatcherClosedEventId, LogStringFormatTable.BackendDispatcherClosed, SessionId, TaskId, MessageId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendDispatcherClosed(SessionId, TaskId, MessageId);
        }
        
        public bool LogBackendResponseReceivedSessionFault(string SessionId, string TaskId, System.Guid MessageId, string FaultCode)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BackendResponseReceivedSessionFaultEventId, LogStringFormatTable.BackendResponseReceivedSessionFault, SessionId, TaskId, MessageId, FaultCode);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendResponseReceivedSessionFault(SessionId, TaskId, MessageId, FaultCode);
        }
        
        public bool LogBackendValidateClientFailed(string SessionId, string TaskId, System.Guid MessageId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BackendValidateClientFailedEventId, LogStringFormatTable.BackendValidateClientFailed, SessionId, TaskId, MessageId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendValidateClientFailed(SessionId, TaskId, MessageId);
        }
        
        public bool LogBackendResponseStored(string SessionId, string TaskId, System.Guid MessageId, bool Fault)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.BackendResponseStoredEventId, LogStringFormatTable.BackendResponseStored, SessionId, TaskId, MessageId, Fault);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendResponseStored(SessionId, TaskId, MessageId, Fault);
        }
        
        public bool LogSessionCreating(string SessionId, string UserName)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionCreatingEventId, LogStringFormatTable.SessionCreating, SessionId, UserName);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionCreating(SessionId, UserName);
        }
        
        public bool LogSessionCreated(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionCreatedEventId, LogStringFormatTable.SessionCreated, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionCreated(SessionId);
        }
        
        public bool LogFailedToCreateSession(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.FailedToCreateSessionEventId, LogStringFormatTable.FailedToCreateSession, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFailedToCreateSession(SessionId);
        }
        
        public bool LogSessionFinished(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionFinishedEventId, LogStringFormatTable.SessionFinished, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionFinished(SessionId);
        }
        
        public bool LogSessionFinishedBecauseOfJobCanceled(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionFinishedBecauseOfJobCanceledEventId, LogStringFormatTable.SessionFinishedBecauseOfJobCanceled, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionFinishedBecauseOfJobCanceled(SessionId);
        }
        
        public bool LogSessionFinishedBecauseOfTimeout(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionFinishedBecauseOfTimeoutEventId, LogStringFormatTable.SessionTimeoutToFinished, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionFinishedBecauseOfTimeout(SessionId);
        }
        
        public bool LogSessionSuspended(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionSuspendedEventId, LogStringFormatTable.SessionSuspended, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionSuspended(SessionId);
        }
        
        public bool LogSessionSuspendedBecauseOfJobCanceled(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionSuspendedBecauseOfJobCanceledEventId, LogStringFormatTable.SessionSuspendedBecauseOfJobCanceled, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionSuspendedBecauseOfJobCanceled(SessionId);
        }
        
        public bool LogSessionSuspendedBecauseOfTimeout(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionSuspendedBecauseOfTimeoutEventId, LogStringFormatTable.SessionTimeoutToSuspended, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionSuspendedBecauseOfTimeout(SessionId);
        }
        
        public bool LogSessionRaisedUp(string SessionId, string UserName)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionRaisedUpEventId, LogStringFormatTable.SessionRaisedUp, SessionId, UserName);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionRaisedUp(SessionId, UserName);
        }
        
        public bool LogSessionRaisedUpFailover(string SessionId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.SessionRaisedUpFailoverEventId, LogStringFormatTable.SessionRaisedUpFailover, SessionId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogSessionRaisedUpFailover(SessionId);
        }
        
        public bool LogBrokerWorkerUnexpectedlyExit(int Id, string Message)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, RuntimeTraceEventIdConst.BrokerWorkerUnexpectedlyExitEventId, LogStringFormatTable.BrokerWorkerUnexpectedlyExitString, Id, Message);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogBrokerWorkerUnexpectedlyExit(Id, Message);
        }
        
        public bool LogBrokerWorkerMessage(int Id, string Message)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BrokerWorkerMessageEventId, LogStringFormatTable.BrokerWorkerMessageString, Id, Message);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogBrokerWorkerMessage(Id, Message);
        }
        
        public bool LogBrokerWorkerProcessReady(int PID)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BrokerWorkerProcessReadyEventId, LogStringFormatTable.BrokerWorkerProcessCreated, PID);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogBrokerWorkerProcessReady(PID);
        }
        
        public bool LogBrokerWorkerProcessExited(int PID, int ExitCode)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BrokerWorkerProcessExitedEventId, LogStringFormatTable.BrokerWorkerProcessExited, PID, ExitCode);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogBrokerWorkerProcessExited(PID, ExitCode);
        }
        
        public bool LogBrokerWorkerProcessFailedToInitialize(int PID)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BrokerWorkerProcessFailedToInitializeEventId, LogStringFormatTable.BrokerWorkerProcessFailedToInitialize, PID);
            if ((this.IsTraceEnabled("0") == false))
            {
                return false;
            }
            return etwTrace.LogBrokerWorkerProcessFailedToInitialize(PID);
        }
        
        public bool LogBrokerClientCreated(string SessionId, string ClientId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BrokerClientCreatedEventId, LogStringFormatTable.BrokerClientCreated, SessionId, ClientId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBrokerClientCreated(SessionId, ClientId);
        }
        
        public bool LogBrokerClientStateTransition(string SessionId, string ClientId, string State)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BrokerClientStateTransitionEventId, LogStringFormatTable.BrokerClientStateTransition, SessionId, ClientId, State);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBrokerClientStateTransition(SessionId, ClientId, State);
        }
        
        public bool LogBrokerClientRejectFlush(string SessionId, string ClientId, string State)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BrokerClientRejectFlushEventId, LogStringFormatTable.BrokerClientRejectFlush, SessionId, ClientId, State);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBrokerClientRejectFlush(SessionId, ClientId, State);
        }
        
        public bool LogBrokerClientRejectEOM(string SessionId, string ClientId, string State)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.BrokerClientRejectEOMEventId, LogStringFormatTable.BrokerClientRejectEOM, SessionId, ClientId, State);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBrokerClientRejectEOM(SessionId, ClientId, State);
        }
        
        public bool LogBrokerClientTimedOut(string SessionId, string ClientId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BrokerClientTimedOutEventId, LogStringFormatTable.BrokerClientTimedOut, SessionId, ClientId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBrokerClientTimedOut(SessionId, ClientId);
        }
        
        public bool LogBrokerClientAllResponseDispatched(string SessionId, string ClientId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BrokerClientAllResponseDispatchedEventId, LogStringFormatTable.BrokerClientAllResponseDispatched, SessionId, ClientId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBrokerClientAllResponseDispatched(SessionId, ClientId);
        }
        
        public bool LogBrokerClientAllRequestDone(string SessionId, string ClientId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BrokerClientAllRequestDoneEventId, LogStringFormatTable.BrokerClientAllRequestDone, SessionId, ClientId);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBrokerClientAllRequestDone(SessionId, ClientId);
        }
        
        public bool LogBrokerClientDisconnected(string SessionId, string ClientId, string State)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BrokerClientDisconnectedEventId, LogStringFormatTable.BrokerClientDisconnected, SessionId, ClientId, State);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBrokerClientDisconnected(SessionId, ClientId, State);
        }
        
        public bool LogUserTraceVerbose(string SessionId, System.Guid MessageId, System.Guid DispatchId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, RuntimeTraceEventIdConst.UserTraceVerboseEventId, LogStringFormatTable.UserTraceMessage, SessionId, MessageId, DispatchId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogUserTraceVerbose(SessionId, MessageId, DispatchId, String);
        }
        
        public bool LogUserTraceInfo(string SessionId, System.Guid MessageId, System.Guid DispatchId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.UserTraceInfoEventId, LogStringFormatTable.UserTraceMessage, SessionId, MessageId, DispatchId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogUserTraceInfo(SessionId, MessageId, DispatchId, String);
        }
        
        public bool LogUserTraceWarning(string SessionId, System.Guid MessageId, System.Guid DispatchId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, RuntimeTraceEventIdConst.UserTraceWarningEventId, LogStringFormatTable.UserTraceMessage, SessionId, MessageId, DispatchId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogUserTraceWarning(SessionId, MessageId, DispatchId, String);
        }
        
        public bool LogUserTraceError(string SessionId, System.Guid MessageId, System.Guid DispatchId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, RuntimeTraceEventIdConst.UserTraceErrorEventId, LogStringFormatTable.UserTraceMessage, SessionId, MessageId, DispatchId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogUserTraceError(SessionId, MessageId, DispatchId, String);
        }
        
        public bool LogUserTraceCritial(string SessionId, System.Guid MessageId, System.Guid DispatchId, string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Critical, RuntimeTraceEventIdConst.UserTraceCritialEventId, LogStringFormatTable.UserTraceMessage, SessionId, MessageId, DispatchId, String);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogUserTraceCritial(SessionId, MessageId, DispatchId, String);
        }
        
        public bool LogFrontendBindingLoaded(string SessionId, string Symbol, long MaxMessageSize, long ReceiveTimeout, long SendTimeout, string MessageVersion, string Scheme)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.FrontendBindingLoadedEventId, LogStringFormatTable.FrontendBindingLoaded, SessionId, Symbol, MaxMessageSize, ReceiveTimeout, SendTimeout, MessageVersion, Scheme);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontendBindingLoaded(SessionId, Symbol, MaxMessageSize, ReceiveTimeout, SendTimeout, MessageVersion, Scheme);
        }
        
        public bool LogBackendBindingLoaded(string SessionId, string Symbol, long MaxMessageSize, long ReceiveTimeout, long SendTimeout, string MessageVersion, string Scheme)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.BackendBindingLoadedEventId, LogStringFormatTable.BackendBindingLoaded, SessionId, Symbol, MaxMessageSize, ReceiveTimeout, SendTimeout, MessageVersion, Scheme);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogBackendBindingLoaded(SessionId, Symbol, MaxMessageSize, ReceiveTimeout, SendTimeout, MessageVersion, Scheme);
        }
        
        public bool LogFrontendCreated(string SessionId, string Symbol, string FrontendUri)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.FrontendCreatedEventId, LogStringFormatTable.FrontendCreated, SessionId, Symbol, FrontendUri);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontendCreated(SessionId, Symbol, FrontendUri);
        }
        
        public bool LogFrontendControllerCreated(string SessionId, string Symbol, string ControllerUri, string GetResponseUri)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.FrontendControllerCreatedEventId, LogStringFormatTable.FrontendControllerCreated, SessionId, Symbol, ControllerUri, GetResponseUri);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogFrontendControllerCreated(SessionId, Symbol, ControllerUri, GetResponseUri);
        }
        
        public bool LogQueueCreatedOrExist(string SessionId, string QueueName)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.QueueCreatedOrExistEventId, LogStringFormatTable.QueueCreatedOrExist, SessionId, QueueName);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogQueueCreatedOrExist(SessionId, QueueName);
        }
        
        public bool LogQueueDeleted(string SessionId, string QueueName)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, RuntimeTraceEventIdConst.QueueDeletedEventId, LogStringFormatTable.QueueDeleted, SessionId, QueueName);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogQueueDeleted(SessionId, QueueName);
        }
        
        public bool LogQueueNotExist(string SessionId, string QueueName)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, RuntimeTraceEventIdConst.QueueNotExistEventId, LogStringFormatTable.QueueNotExist, SessionId, QueueName);
            if ((this.IsTraceEnabled(SessionId) == false))
            {
                return false;
            }
            return etwTrace.LogQueueNotExist(SessionId, QueueName);
        }
    }
    
    public sealed class SOADiagTraceWrapper
    {
        
        private const string TraceSourceNameForCosmos = "HpcSoa";
        
        private System.Diagnostics.TraceSource cosmosTrace = new System.Diagnostics.TraceSource(TraceSourceNameForCosmos);
        
        private SOADiagTrace etwTrace = new SOADiagTrace();
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="We allow caller to change the trace level.")]
        public SOADiagTraceWrapper()
        {
            this.cosmosTrace.Switch.Level = SourceLevels.All;
        }
        
        public bool LogTextVerbose(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, SOADiagTraceEventIdConst.TextVerboseEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextVerbose(String);
        }
        
        public bool LogTextInfo(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, SOADiagTraceEventIdConst.TextInfoEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextInfo(String);
        }
        
        public bool LogTextWarning(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, SOADiagTraceEventIdConst.TextWarningEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextWarning(String);
        }
        
        public bool LogTextError(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, SOADiagTraceEventIdConst.TextErrorEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextError(String);
        }
        
        public bool LogTextCritial(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Critical, SOADiagTraceEventIdConst.TextCritialEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextCritial(String);
        }
        
        public bool LogEventReceived(int EventId)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, SOADiagTraceEventIdConst.EventReceivedEventId, LogStringFormatTable.EventReceived, EventId);
            return etwTrace.LogEventReceived(EventId);
        }
        
        public bool LogEventBuffered(int EventId, string SessionId, int BufferId, int Count, System.DateTime CreatedTime)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, SOADiagTraceEventIdConst.EventBufferedEventId, LogStringFormatTable.EventBuffered, EventId, SessionId, BufferId, Count, CreatedTime);
            return etwTrace.LogEventBuffered(EventId, SessionId, BufferId, Count, CreatedTime);
        }
        
        public bool LogBufferWritten(int BufferId, int Count, System.DateTime CreatedTime)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, SOADiagTraceEventIdConst.BufferWrittenEventId, LogStringFormatTable.BufferWritten, BufferId, Count, CreatedTime);
            return etwTrace.LogBufferWritten(BufferId, Count, CreatedTime);
        }
        
        public bool LogBufferDiscarded(int BufferId, int Count, System.DateTime CreatedTime, string Exception)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, SOADiagTraceEventIdConst.BufferDiscardedEventId, LogStringFormatTable.BufferDiscarded, BufferId, Count, CreatedTime, Exception);
            return etwTrace.LogBufferDiscarded(BufferId, Count, CreatedTime, Exception);
        }
        
        public bool LogStartThrottle(int CurrentBufferSize)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, SOADiagTraceEventIdConst.StartThrottleEventId, LogStringFormatTable.StartThrottle, CurrentBufferSize);
            return etwTrace.LogStartThrottle(CurrentBufferSize);
        }
        
        public bool LogStopThrottle(int CurrentBufferSize)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, SOADiagTraceEventIdConst.StopThrottleEventId, LogStringFormatTable.StopThrottle, CurrentBufferSize);
            return etwTrace.LogStopThrottle(CurrentBufferSize);
        }
    }
    
    public sealed class DataSvcTraceWrapper
    {
        
        private const string TraceSourceNameForCosmos = "HpcSoa";
        
        private System.Diagnostics.TraceSource cosmosTrace = new System.Diagnostics.TraceSource(TraceSourceNameForCosmos);
        
        private DataSvcTrace etwTrace = new DataSvcTrace();
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification="We allow caller to change the trace level.")]
        public DataSvcTraceWrapper()
        {
            this.cosmosTrace.Switch.Level = SourceLevels.All;
        }
        
        public bool LogTextVerbose(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, DataSvcTraceEventIdConst.TextVerboseEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextVerbose(String);
        }
        
        public bool LogTextInfo(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Information, DataSvcTraceEventIdConst.TextInfoEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextInfo(String);
        }
        
        public bool LogTextWarning(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Warning, DataSvcTraceEventIdConst.TextWarningEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextWarning(String);
        }
        
        public bool LogTextError(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Error, DataSvcTraceEventIdConst.TextErrorEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextError(String);
        }
        
        public bool LogTextCritial(string String)
        {
            cosmosTrace.TraceEvent(System.Diagnostics.TraceEventType.Critical, DataSvcTraceEventIdConst.TextCritialEventId, LogStringFormatTable.PureString, String);
            return etwTrace.LogTextCritial(String);
        }
    }
    
    public sealed class LogStringFormatTable
    {
        
        public static string PureString = "{0}";
        
        public static string StringMessage = "[Session:{0}] {1}";
        
        public static string UserTraceMessage = "[Session:{0}] MessageId = {1}, DispatchId = {2}: {3}";
        
        public static string TextTracing = "Text based tracing.";
        
        public static string Exception = "An unexpected exception occurred. For more information about this exception, see " +
            "the Details tab. \n\n Additional data:\n {0}";
        
        public static string HostRequestReceived = "Servicehost Get the request.";
        
        public static string HostResponseSent = "Servicehost end Process Message.";
        
        public static string HostStart = "Servicehost is started.";
        
        public static string HostLoadAssembly = "Assembly is loaded.";
        
        public static string HostCanceled = "Host got canceled event.";
        
        public static string HostServiceConfigCheck = "The file {0} is found. The file is discarded in Microsoft HPC Pack 2012. All cont" +
            "ent is moved to service registration file.";
        
        public static string HostAssemblyLoaded = "The service assembly is loaded.";
        
        public static string BackendRequestSent = "[Session:{0}] TaskId = {1}, MessageId = {2}, DispatchId = {3}, TargetMachine = {4" +
            "}, Request has been dispatched to the service host. EPR = {5}";
        
        public static string BackendRequestPutBack = "[Session:{0}] TaskId = {1}, MessageId = {2}, Put request back into broker queue.";
        
        public static string BackendDispatcherClosed = "[Session:{0}] TaskId = {1}, MessageId = {2}, Dispatcher is closing. Need to put r" +
            "equest back to broker queue.";
        
        public static string BackendValidateClientFailed = "[Session:{0}] TaskId = {1}, MessageId = {2}, The underlying connection is invalid" +
            ". Need to put request back to broker queue.";
        
        public static string BackendRequestSentFailed = "[Session:{0}] TaskId = {1}, MessageId = {2}, Request has been failed to dispatch " +
            "to the service host. Exception: {3}";
        
        public static string BackendGeneratedFaultReply = "[Session:{0}] TaskId = {1}, MessageId = {2}, Generated fault reply message: {3}";
        
        public static string BackendResponseReceived = "[Session:{0}] TaskId = {1}, MessageId = {2}, Fault = {3}, Received response from " +
            "service host.";
        
        public static string BackendResponseStored = "[Session:{0}] TaskId = {1}, MessageId = {2}, Put response back into broker queue." +
            "";
        
        public static string BackendResponseReceivedSessionFault = "[Session:{0}] TaskId = {1}, MessageId = {2}, Response is a session fault, fault a" +
            "ction = {3}.";
        
        public static string BackendResponseReceivedRetryOperationError = "[Session:{0}] TaskId = {1}, MessageId = {2}, Retry operation error received from " +
            "service host, RetryCount = {3}";
        
        public static string BackendResponseReceivedRetryLimitExceed = "[Session:{0}] TaskId = {1}, MessageId = {2}, Retry limit exceed, Limit = {3}";
        
        public static string BackendResponseReceivedFailed = "[Session:{0}] TaskId = {1}, MessageId = {2}, Failed to receive response from serv" +
            "ice host. RetryCount = {3}, Exception: {4}";
        
        public static string BackendHandleResponseFailed = "[Session:{0}] TaskId = {1}, MessageId = {2}, Failed to handle response. Exception" +
            ": {3}";
        
        public static string BackendHandleEndpointNotFoundExceptionFailed = "[Session:{0}] TaskId = {1}, MessageId = {2}, Failed to handle EndpointNotFoundExc" +
            "eption. Exception: {3}";
        
        public static string BackendHandleExceptionFailed = "[Session:{0}] TaskId = {1}, MessageId = {2}, Failed to handle exception. Exceptio" +
            "n: {3}";
        
        public static string BackendEndpointNotFoundExceptionOccured = "[Session:{0}] TaskId = {1}, MessageId = {2}, EndpointNotFoundException occured wh" +
            "ile trying to process request: {3}";
        
        public static string FrontEndRequestReceived = "[Session:{0}] ClientId = {1}, MessageId = {2}, Frontend received request.";
        
        public static string FrontEndRequestRejectedClientIdNotMatch = "[Session:{0}] ClientId = {1}, MessageId = {2}, Frontend rejected request because " +
            "client id does match.";
        
        public static string FrontEndRequestRejectedClientIdInvalid = "[Session:{0}] ClientId = {1}, MessageId = {2}, Frontend rejected request because " +
            "client id is invalid or too long.";
        
        public static string FrontEndRequestRejectedClientStateInvalid = "[Session:{0}] ClientId = {1}, MessageId = {2}, Frontend silently rejected request" +
            " because corresponding client is not accepting requests.";
        
        public static string FrontEndResponseSent = "[Session:{0}] ClientId = {1}, MessageId = {2}, Response has been sent to the clie" +
            "nt.";
        
        public static string FrontEndRequestRejectedAuthenticationError = "[Session:{0}] ClientId = {1}, MessageId = {2}, Frontend rejected request from use" +
            "r {3}.";
        
        public static string FrontEndRequestRejectedGeneralError = "[Session:{0}] ClientId = {1}, MessageId = {2}, Frontend rejected request. Excepti" +
            "on: {3}";
        
        public static string FrontEndResponseSentFailed = "[Session:{0}] ClientId = {1}, MessageId = {2}, Response has been failed to send t" +
            "o the client. Exception: {3}";
        
        public static string SessionCreating = "Session {0} is being created by user {1}.";
        
        public static string SessionCreated = "Session {0} has been sucessfully created.";
        
        public static string FailedToCreateSession = "Failed to create session {0}.";
        
        public static string SessionFinished = "Session {0} is finished.";
        
        public static string SessionFinishedBecauseOfJobCanceled = "Session {0} is finished because corresponding service job is failed or canceled.";
        
        public static string SessionTimeoutToFinished = "Session {0} is timed out and finished.";
        
        public static string SessionSuspended = "Session {0} is suspended.";
        
        public static string SessionSuspendedBecauseOfJobCanceled = "Session {0} is suspended because corresponding service job is failed or canceled." +
            "";
        
        public static string SessionTimeoutToSuspended = "Session {0} is timed out and suspended.";
        
        public static string SessionRaisedUp = "Session {0} has been raised up by user {1}.";
        
        public static string SessionRaisedUpFailover = "Session {0} has been raised up because of failover.";
        
        public static string BrokerClientCreated = "[Session:{0}] ClientId = {1}, Client created.";
        
        public static string BrokerClientStateTransition = "[Session:{0}] ClientId = {1}, State ==> {2}.";
        
        public static string BrokerClientRejectFlush = "[Session:{0}] ClientId = {1}, Reject Flush because the state is not ClientConnect" +
            "ed, state is {2}.";
        
        public static string BrokerClientRejectEOM = "[Session:{0}] ClientId = {1}, Reject EOM because the state is not ClientConnected" +
            ", state is {2}.";
        
        public static string BrokerClientTimedOut = "[Session:{0}] ClientId = {1}, Client created.";
        
        public static string BrokerClientAllResponseDispatched = "[Session:{0}] ClientId = {1}, All responses dispatched, send EndOfResponse messag" +
            "e.";
        
        public static string BrokerClientAllRequestDone = "[Session:{0}] ClientId = {1}, All request done.";
        
        public static string BrokerClientDisconnected = "[Session:{0}] ClientId = {1}, Client disconnected because {2}.";
        
        public static string BrokerWorkerUnexpectedlyExitString = "[Broker Worker {0}] BrokerWorker exists unexpectedly. {1}";
        
        public static string BrokerWorkerMessageString = "[Broker Worker {0}] {1}";
        
        public static string BrokerWorkerProcessCreated = "Broker worker process is ready. PID = {0}";
        
        public static string BrokerWorkerProcessExited = "Broker worker process exited. PID = {0}, ExitCode = {1}";
        
        public static string BrokerWorkerProcessFailedToInitialize = "Broker worker process failed to initialize. PID = {0}";
        
        public static string FrontendBindingLoaded = "[Session:{0}] {1} frontend binding loaded.\nMaxMessageSize = {2}%ReceiveTimeout = " +
            "{3}\nSendTimeout = {4}\nMessageVersion = {5}\nScheme={6}";
        
        public static string BackendBindingLoaded = "[Session:{0}] {1} backend binding loaded.\nMaxMessageSize = {2}%ReceiveTimeout = {" +
            "3}\nSendTimeout = {4}\nMessageVersion = {5}\nScheme={6}";
        
        public static string FrontendCreated = "[Session:{0}] {1} frontend created, Uri = {2}.";
        
        public static string FrontendControllerCreated = "[Session:{0}] {1} frontend controller created, ControllerUri = {2}, GetResponseUr" +
            "i = {3}.";
        
        public static string QueueCreatedOrExist = "[Session:{0}] Azure storage queue {1} has been sucessfully created or already exi" +
            "sts.";
        
        public static string QueueDeleted = "[Session:{0}] Azure storage queue {1} has been sucessfully deleted.";
        
        public static string QueueNotExist = "[Session:{0}] Azure storage queue {1} does not exist.";
        
        public static string EventReceived = "Trace event (EventId = {0}) has been received. ";
        
        public static string EventBuffered = "Trace event (EventId = {0}) for session {1} has been buffered in buffer {2}. Numb" +
            "erOfEventsInBuffer = {3}, BufferCreatedTime = {4}.";
        
        public static string BufferWritten = "Buffer {0} has been written into storage. NumberOfEventsInBuffer = {1}, BufferCre" +
            "atedTime = {2}.";
        
        public static string BufferDiscarded = "Buffer {0} has been discarded. NumberOfEventsInBuffer = {1}, BufferCreatedTime = " +
            "{2}. Exception = {3}.";
        
        public static string StartThrottle = "Start throttling. Current buffer size is {0} bytes.";
        
        public static string StopThrottle = "Stop throttling. Current buffer size is {0} bytes.";
        
        private LogStringFormatTable()
        {
        }
    }
}
