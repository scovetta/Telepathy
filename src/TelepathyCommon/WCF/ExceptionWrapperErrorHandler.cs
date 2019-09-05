using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace TelepathyCommon.Service
{
    public class ExceptionWrapperErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Provide a fault. The Message fault parameter can be replaced, or set to null to suppress reporting a fault.
        /// </summary>
        /// <param name="error">The <see cref="Exception"/> object thrown in the course of the service operation.</param>
        /// <param name="version">The SOAP version of the message.</param>
        /// <param name="fault">The <see cref="System.ServiceModel.Channels.Message"/> object that is returned to the client, or service, in the duplex case.</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            //If it's a FaultException already, then we have nothing to do
            if (error is FaultException)
                return;

            Trace.TraceWarning("[ExceptionWrapperErrorHandler] The exception is: {0}", error);
            var faultException = new FaultException<ExceptionWrapper>(new ExceptionWrapper(error), error.Message);
            fault = Message.CreateMessage(version, faultException.CreateMessageFault(), faultException.Action);
        }

        /// <summary>
        /// Enables error-related processing and returns a value that indicates whether the dispatcher aborts the session and the instance context in certain cases.
        /// </summary>
        /// <param name="error">The exception thrown during processing.</param>
        /// <returns>true if Windows Communication Foundation (WCF) should not abort the session (if there is one) and instance context if the instance context is not Single; otherwise, false. The default is false.</returns>
        public bool HandleError(Exception error)
        {
            return true;
        }
    }
}
