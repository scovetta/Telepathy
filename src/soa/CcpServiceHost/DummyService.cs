// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Dummy service
    /// </summary>
    [ServiceContract]
    internal class DummyService
    {
        /// <summary>
        /// Message handler
        /// </summary>
        /// <param name="request">incoming request message</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Contract class need at least on instance method")]
        [OperationContract(Action = "*", ReplyAction = "*")]
        public Message HandleMessage(Message requestMsg)
        {
            return GenerateFaultMessage(requestMsg);
        }

        /// <summary>
        /// Generate fault message for a request message.
        /// </summary>
        /// <param name="requestMsg">request message</param>
        /// <returns>generated fault message</returns>
        private static Message GenerateFaultMessage(Message requestMsg)
        {
            MessageVersion version = requestMsg.Headers.MessageVersion;
            SessionFault sessionFault = new SessionFault(SOAFaultCode.Service_InitializeFailed, StringTable.FailedToInitializeServiceHost);
            FaultReason faultReason = new FaultReason(StringTable.FailedToInitializeServiceHost);
            FaultCode faultCode = FaultCode.CreateReceiverFaultCode("ServiceHostInitializationFailed", Constant.HpcHeaderNS);
            FaultException faultException = new FaultException<SessionFault>(sessionFault, faultReason, faultCode, SessionFault.Action);
            MessageFault fault = faultException.CreateMessageFault();
            Message faultMessage = Message.CreateMessage(version, fault, faultException.Action);
            faultMessage.Headers.RelatesTo = requestMsg.Headers.MessageId;
            return faultMessage;
        }
    }
}
