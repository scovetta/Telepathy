// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.BrokerProxy;

    /// <summary>
    /// This is a class for receiving response from Azure side service host.
    /// </summary>
    internal class AzureResponseReceiver : ResponseReceiver
    {
        /// <summary>
        /// A string that used to mark an exception as indirect.
        /// Note: "indirect" exceptions are exceptions that carried in ProxyFault
        /// and passed back by broker proxy.
        /// </summary>
        private const string IndirectExceptionMark = "IndirectServiceHostException";

        /// <summary>
        /// Initializes a new instance of the AzureResponseReceiver class.
        /// </summary>
        public AzureResponseReceiver(IDispatcher dispatcher)
            : base(dispatcher)
        {
        }

        /// <summary>
        /// Exceptions thrown during broker proxy and service host communication
        /// are wrapped into FaultException(ProxyFault) and passed back to
        /// AzureDispatcher. Here we check if the message is a fault message that
        /// carries ProxyFault. If so, extract the inner exception from ProxyFault
        /// and throw it out. Dispatcher will handle the exception as if it is
        /// talking to service host directly.
        /// </summary>
        /// <param name="message">response message</param>
        protected override void PostProcessMessage(Message message)
        {
            if (message.IsFault)
            {
                if (message.Headers.Action == ProxyFault.Action)
                {
                    MessageFault fault = MessageFault.CreateFault(message, int.MaxValue);

                    ProxyFault proxyFault = fault.GetDetail<ProxyFault>();

                    if (proxyFault != null)
                    {
                        // Create exception according to the FaultCode, and mark it as "indirect"
                        // by setting its source to IndirectExceptionMark. This mark will later
                        // be checked by dispatcher to determine if it need to recreate the
                        // underlying WCF channel. For indirect exception, we don't need to
                        // recreate the channel(between dispatcher and broker proxy).
                        Exception e;

                        if (proxyFault.FaultCode == ProxyFault.ProxyEndpointNotFound)
                        {
                            // will throw EndpointNotFoundException for ProxyFault.ProxyEndpointNotFound
                            e = new EndpointNotFoundException(proxyFault.Message);
                        }
                        else if (proxyFault.FaultCode == ProxyFault.ProxyCommunicationException)
                        {
                            // will throw CommunicationException for ProxyFault.ProxyCommunicationException
                            e = new CommunicationException(proxyFault.Message);
                        }
                        else
                        {
                            // will throw general exception for other FaultCode
                            e = new Exception(proxyFault.Message);
                        }

                        e.Source = IndirectExceptionMark;

                        throw e;
                    }
                }
            }
        }
    }
}
