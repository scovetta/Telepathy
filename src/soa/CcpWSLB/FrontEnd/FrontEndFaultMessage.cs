// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.FrontEnd
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Provide frontend fault messagesx
    /// </summary>
    internal static class FrontEndFaultMessage
    {
        /// <summary>
        /// Generates the fault message from fault code
        /// </summary>
        /// <param name="requestMessage">indicating the request message</param>
        /// <param name="version">message version</param>
        /// <param name="code">indicating the soa fault code</param>
        /// <param name="reason">indicating the reason</param>
        /// <param name="context">indicating the context</param>
        /// <returns>fault message</returns>
        public static Message GenerateFaultMessage(Message requestMessage, MessageVersion version, int code, string reason, params string[] context)
        {
            string errorMessage = context == null || context.Length == 0 ? reason : String.Format(CultureInfo.CurrentCulture, reason, context);
            FaultException<SessionFault> faultException = new FaultException<SessionFault>(new SessionFault(code, reason, context), errorMessage, FaultCode.CreateReceiverFaultCode(SOAFaultCode.GetFaultCodeName(code), Constant.HpcHeaderNS), SessionFault.Action);
            return GenerateFaultMessage(requestMessage, version, faultException);
        }

        /// <summary>
        /// Translates broker queue exception to fault message
        /// </summary>
        /// <param name="e">indicating the broker queue exception</param>
        /// <param name="relatesTo">indicating the relatesTo id</param>
        /// <param name="version">message version</param>
        /// <returns>fault message</returns>
        public static Message TranslateBrokerQueueExceptionToFaultMessage(BrokerQueueException e, Message request)
        {
            MessageVersion version = request.Headers.MessageVersion;
            FaultException<SessionFault> faultException = ExceptionHelper.ConvertBrokerQueueExceptionToFaultException(e);

            return GenerateFaultMessage(request, version, faultException);
        }

        /// <summary>
        /// Generates the fault message from fault code
        /// </summary>
        /// <param name="requestMessage">indicating the request message</param>
        /// <param name="version">message version</param>
        /// <param name="faultException">indicating the fault exception</param>
        /// <returns>fault message</returns>
        public static Message GenerateFaultMessage(Message requestMessage, MessageVersion version, FaultException faultException)
        {
            MessageFault fault = faultException.CreateMessageFault();
            UniqueId relatesTo = null;
            Message faultMessage = Message.CreateMessage(version, fault, faultException.Action);

            if (requestMessage != null)
            {
                // add action
                faultMessage.Headers.Add(MessageHeader.CreateHeader(Constant.ActionHeaderName, Constant.HpcHeaderNS, requestMessage.Headers.Action));

                // add user data
                int index = requestMessage.Headers.FindHeader(Constant.UserDataHeaderName, Constant.HpcHeaderNS);
                if (index >= 0)
                {
                    faultMessage.Headers.CopyHeaderFrom(requestMessage, index);
                }

                relatesTo = requestMessage.Headers.MessageId;
            }

            // Only add relatesTo header to WSAddressing messages
            if (relatesTo != null && version.Addressing == AddressingVersion.WSAddressing10)
            {
                faultMessage.Headers.RelatesTo = relatesTo;
            }

            return faultMessage;
        }
    }
}
