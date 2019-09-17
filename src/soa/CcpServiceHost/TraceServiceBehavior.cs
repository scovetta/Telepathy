//------------------------------------------------------------------------------
// <copyright file="TraceServiceBehavior.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The message inspector to do the tracing
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.CcpServiceHosting
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using RuntimeTraceHelper = Microsoft.Hpc.RuntimeTrace.TraceHelper;
    using System.Threading;

    /// <summary>
    /// The message inspector
    /// </summary>
    internal class TraceServiceBehavior : IEndpointBehavior, IDispatchMessageInspector
    {
        private const string SoaDebuggerMachineName = "SoaDebuggerMachineName";

        private const string SoaDebuggerPort = "SoaDebuggerPort";

        private const string EnableSoaDebuggerEnvName = "EnableSoaDebugger";

        /// <summary>
        /// Setting if we want to propagate activityId
        /// </summary>
        private const string PropagateActivityValue = "propagateActivity";

        private string sessionId;

        private bool enableSoaDebugger;

        private bool propagateActivity;

        private CcpServiceHostWrapper hostWrapper;

        public TraceServiceBehavior(string sessionId, CcpServiceHostWrapper hostWrapper)
        {
            this.sessionId = sessionId;
            this.hostWrapper = hostWrapper;

            // determine the propagateActivity setting
            string setting = ServiceContext.Logger.Attributes[PropagateActivityValue];

            bool.TryParse(setting, out propagateActivity);

            bool.TryParse(
                Environment.GetEnvironmentVariable(EnableSoaDebuggerEnvName, EnvironmentVariableTarget.Process),
                out this.enableSoaDebugger);
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            Guid messageId;
            request.Headers.MessageId.TryGetGuid(out messageId);

            this.hostWrapper.AllMessageIds.Add(messageId);
            this.hostWrapper.SerivceHostIdleTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            this.hostWrapper.ServiceHangTimer?.Change(this.hostWrapper.ServiceHangTimeout, Timeout.Infinite);

            if (this.propagateActivity)
            {
                Trace.CorrelationManager.ActivityId = messageId;
            }

            // This trace is included in the user trace.
            ServiceContext.Logger.TraceEvent(
                TraceEventType.Verbose,
                0,
                "[HpcServiceHost]: Request is received.");

            RuntimeTraceHelper.TraceEvent(
                TraceEventType.Verbose,
                "[HpcServiceHost]: Request {0} is received.",
                messageId);

            if (this.enableSoaDebugger)
            {
                int machineNameIndex = request.Headers.FindHeader(SoaDebuggerMachineName, string.Empty);
                int portIndex = request.Headers.FindHeader(SoaDebuggerPort, string.Empty);

                if (0 <= machineNameIndex && machineNameIndex < request.Headers.Count &&
                    0 <= portIndex && portIndex < request.Headers.Count)
                {
                    string ideMachine = request.Headers.GetHeader<string>(machineNameIndex);
                    int port = request.Headers.GetHeader<int>(portIndex);

                    string localMachine = Environment.MachineName;
                    int pid = Process.GetCurrentProcess().Id;

                    // send back debug info "machinename|pid|jobid" to the VS IDE
                    SendByTcp(ideMachine, port, string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}", localMachine, pid, this.sessionId));
                    ServiceContext.Logger.TraceInformation(string.Format(CultureInfo.InvariantCulture, "[HpcServiceHost]: Send debug info to the VS IDE machine {0}:{1}.", ideMachine, port));
                }
            }

            return messageId;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            Guid guid = Guid.Empty;

            if (correlationState is Guid)
            {
                guid = (Guid)correlationState;
            }

            // This trace is included in the user trace.
            ServiceContext.Logger.TraceEvent(
                TraceEventType.Verbose,
                0,
                "[HpcServiceHost]: Response is sent back. IsFault = {0}",
                reply.IsFault);

            RuntimeTraceHelper.TraceEvent(
                TraceEventType.Verbose,
                "[HpcServiceHost]: Response {0} is sent back. IsFault = {1}",
                guid,
                reply.IsFault);

            if (this.propagateActivity)
            {
                System.Diagnostics.Trace.CorrelationManager.ActivityId = Guid.Empty;
            }

            if (this.hostWrapper.EnableMessageLevelPreemption)
            {
                // If the message is skipped, reply a fault message to the broker.
                if (this.hostWrapper.SkippedMessageIds.Contains(guid))
                {
                    this.hostWrapper.SkippedMessageIds.Remove(guid);

                    // For Service_Preempted error, reuse SessionFault.reason property to pass "processing message count" to the broker.
                    int messageCount = this.hostWrapper.ProcessingMessageIds.Count;
                    SessionFault fault = new SessionFault(SOAFaultCode.Service_Preempted, messageCount.ToString());

                    FaultException faultException = new FaultException<SessionFault>(fault, string.Empty, null, SessionFault.Action);
                    reply = GenerateFaultMessage(guid, reply.Headers.MessageVersion, faultException);
                }
                else if (this.hostWrapper.ProcessingMessageIds.Contains(guid))
                {
                    this.hostWrapper.ProcessingMessageIds.Remove(guid);

                    // The service host receives the cancel event when the request is being processed, so add a header to notice the broker.
                    if (this.hostWrapper.ReceivedCancelEvent)
                    {
                        // Use the header to pass "processing message count" to the broker.
                        int messageCount = this.hostWrapper.ProcessingMessageIds.Count;
                        reply.Headers.Add(MessageHeader.CreateHeader(Constant.MessageHeaderPreemption, Constant.HpcHeaderNS, messageCount));
                    }
                }
                else
                {
                    // If the message is not in above two lists, the message doesn't come into the invoker. No need to change its response.
                }
            }

            if (this.hostWrapper.AllMessageIds.Contains(guid))
            {
                this.hostWrapper.AllMessageIds.Remove(guid);
                lock (this.hostWrapper.AllMessageIds.SyncRoot)
                {
                    if (this.hostWrapper.AllMessageIds.Count == 0)
                    {
                        this.hostWrapper.SerivceHostIdleTimer?.Change(this.hostWrapper.ServiceHostIdleTimeout, Timeout.Infinite);
                        this.hostWrapper.ServiceHangTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    }
                    else
                    {
                        this.hostWrapper.ServiceHangTimer?.Change(this.hostWrapper.ServiceHangTimeout, Timeout.Infinite);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Build a fault message by the input info.
        /// </summary>
        /// <param name="messageId">message id</param>
        /// <param name="version">message version</param>
        /// <param name="faultException">fault exception</param>
        /// <returns>fault message</returns>
        private static Message GenerateFaultMessage(Guid messageId, MessageVersion version, FaultException faultException)
        {
            UniqueId relatesTo = null;
            if (messageId != null)
            {
                relatesTo = new UniqueId(messageId);
            }

            MessageFault fault = faultException.CreateMessageFault();
            Message faultMessage = Message.CreateMessage(version, fault, faultException.Action);
            if (relatesTo != null && version.Addressing == AddressingVersion.WSAddressing10)
            {
                // Only add relatesTo header to WSAddressing messages
                faultMessage.Headers.RelatesTo = relatesTo;
            }

            return faultMessage;
        }

        /// <summary>
        /// Send message to remote machine through socket.
        /// </summary>
        internal static void SendByTcp(string devMachine, int port, string data)
        {
            try
            {
                using (TcpClient client = new TcpClient(devMachine, port))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    string publicKey = reader.ReadLine();
                    writer.WriteLine(Encrypt(data, publicKey));
                    writer.Flush();

                    // sync with the server side. it receives data after VS
                    // attaches debugger to the service host process
                    client.Client.Receive(new byte[4]);
                }
            }
            catch (Exception e)
            {
                ServiceContext.Logger.TraceData(TraceEventType.Error, 0,
                    string.Format(CultureInfo.InvariantCulture, "[HpcServiceHost]: TraceServiceBehavior.SendByTcp: {0}.", e.ToString()));

                throw;
            }
        }

        /// <summary>
        /// Encrypt message by RSACryptoServiceProvider.
        /// </summary>
        /// <param name="data">resource plain text</param>
        /// <param name="publicKeyXml">public key, it comes from VS IDE</param>
        /// <returns>encrypted text</returns>
        internal static string Encrypt(string data, string publicKeyXml)
        {
            StringBuilder encrypt = new StringBuilder();

            byte[] source = Encoding.Unicode.GetBytes(data);

            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(publicKeyXml);

                //max length of the text for each encryption
                int maxLength = provider.KeySize / 8 - 42;

                int n = source.Length / maxLength;

                byte[] buffer = new byte[maxLength];
                for (int i = 0; i <= n; i++)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    int length = i < n ? maxLength : source.Length - n * maxLength;
                    Buffer.BlockCopy(source, i * maxLength, buffer, 0, length);
                    encrypt.Append(Convert.ToBase64String(provider.Encrypt(buffer, true)));
                }
            }

            return encrypt.ToString();
        }
    }
}
