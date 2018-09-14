//------------------------------------------------------------------------------
// <copyright file="DataMessageInspector.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Message inspector that adds authentication info to message headers.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// Message inspector that adds authentication info to message headers
    /// </summary>
    internal class DataMessageInspector : IClientMessageInspector, IEndpointBehavior
    {
        #region IClientMessageInspector members

        /// <summary>
        /// Override IClientMessageInspector.AfterReceiveReply.  Do nothing.
        /// </summary>
        /// <param name="reply">the message to be transformed into types and handed back to the client application</param>
        /// <param name="correlationState">correlation state</param>
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
        }

        /// <summary>
        /// Add user name as a message header before a request message is sent to a service.
        /// </summary>
        /// <param name="request">the message to be sent to the service</param>
        /// <param name="channel">the WCF client object channel</param>
        /// <returns>null as no correlation state is used</returns>
        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            string jobIdEnvVar = Environment.GetEnvironmentVariable(Microsoft.Hpc.Scheduler.Session.Internal.Constant.JobIDEnvVar);
            if (string.IsNullOrEmpty(jobIdEnvVar))
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataMessageInspector] .BeforeSendRequest: job id not found");
            }

            MessageHeader jobIdHeader = MessageHeader.CreateHeader(Constant.JobIdHeaderName, Constant.DataMessageHeaderNamespace, jobIdEnvVar);
            request.Headers.Add(jobIdHeader);

            string jobSecretEnvVar = Environment.GetEnvironmentVariable(Microsoft.Hpc.Scheduler.Session.Internal.Constant.JobSecretEnvVar);
            if (string.IsNullOrEmpty(jobSecretEnvVar))
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataMessageInspector] .BeforeSendRequest: job secret not found");
            }

            MessageHeader jobSecretHeader = MessageHeader.CreateHeader(Constant.JobSecretHeaderName, Constant.DataMessageHeaderNamespace, jobSecretEnvVar);
            request.Headers.Add(jobSecretHeader);

            return null;
        }
        #endregion

        #region IEndpointBehavior members

        /// <summary>
        /// Pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }
  
        /// <summary>
        /// A modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }
   
        /// <summary>
        /// A modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }
 
        /// <summary>
        /// Confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }
        #endregion
    }
}
