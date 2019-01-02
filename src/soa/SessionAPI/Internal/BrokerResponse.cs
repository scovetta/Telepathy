//------------------------------------------------------------------------------
// <copyright file="BrokerResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Wraps response messages to provide access to data, faults and user data
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Authentication;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para>Represents a single response that a service-oriented architecture (SOA) service sent to the client to fulfill a request.</para>
    /// </summary>
    /// <typeparam name="TMessage">
    ///   <para>The type of the response message. You create a TMessage type by adding a 
    /// service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
    /// </typeparam>
    /// <remarks>
    ///   <para>You get a 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object by receiving it through your implementation of the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" /> delegate or from a 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" /> object that you retrieve with the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses" /> method.</para>
    ///   <para>Call the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.Dispose" /> method when you finish using the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseEnumerator{T}.Current" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses" />
    public class BrokerResponse<TMessage> : IDisposable
    {
        /// <summary>
        /// Stores the GetDetail method info base
        /// </summary>
        private static readonly MethodInfo GetDetailMethodInfoBase = typeof(MessageFault).GetMethod("GetDetail", Type.EmptyTypes);

        /// <summary>
        /// Response message's buffer
        /// </summary>
        private MessageBuffer messageBuffer;

        /// <summary>
        /// Converts response message to its type
        /// </summary>
        private TypedMessageConverter typedMessageConverter;

        /// <summary>
        /// Stores the fault collection
        /// </summary>
        private FaultDescriptionCollection faultCollection;

        /// <summary>
        /// Exception to throw when caller accesses Result property
        /// </summary>
        private Exception exception;

        /// <summary>
        /// Stores the message id of the corresponding request
        /// </summary>
        private Guid requestMessageId;

        /// <summary>
        /// Specifies that no more responses are available for this BrokerClient
        /// </summary>
        internal bool isLastResponse = false;

        /// <summary>
        /// Initializes a new instance of the BrokerResponse class
        /// </summary>
        /// <param name="requestMessageId">indicating the request message id</param>
        protected BrokerResponse(UniqueId requestMessageId)
        {
            if (requestMessageId != null)
            {
                requestMessageId.TryGetGuid(out this.requestMessageId);
            }
        }

        /// <summary>
        /// Initializes a new instance of the BrokerResponse class
        /// </summary>
        /// <param name="typedMessageConverter">Converts response message to its type</param>
        /// <param name="messageBuffer">Response message's buffer</param>
        /// <param name="faultCollection">indicating the fault collection</param>
        /// <param name="requestMessageId">indicating the request message id</param>
        internal BrokerResponse(TypedMessageConverter typedMessageConverter, MessageBuffer messageBuffer, FaultDescriptionCollection faultCollection, UniqueId requestMessageId)
            : this(requestMessageId)
        {
            this.typedMessageConverter = typedMessageConverter;
            this.messageBuffer = messageBuffer;
            this.faultCollection = faultCollection;
        }

        /// <summary>
        /// Creates BrokerResponse that hold an exception
        /// </summary>
        /// <param name="e">indicating the exception</param>
        /// <param name="requestMessageId">indicating the request message id</param>
        internal BrokerResponse(Exception e, UniqueId requestMessageId)
            : this(requestMessageId)
        {
            this.exception = e;
        }

        /// <summary>
        ///   <para>Gets or sets the message ID of the request.</para>
        /// </summary>
        /// <value>
        ///   <para>The message ID of the request.</para>
        /// </value>
        public Guid RequestMessageId
        {
            get { return this.requestMessageId; }
        }

        /// <summary>
        ///   <para>Gets whether the response is the last response for a set of requests.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Boolean" /> that indicates whether the response is the last response for a set of requests. 
        /// True indicates that the response is the last response for the set of requests. 
        /// False indicates that the response is not the last response.</para>
        /// </value>
        /// <remarks>
        ///   <para>When the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Behaviors" /> property is set to 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClientBehaviors.EnableIsLastResponseProperty" />, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.IsLastResponse" /> will return 
        /// True when the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object contains the last response. The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object will hold the last response until the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method is called.</para>
        ///   <para>When the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Behaviors" /> property is set to 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClientBehaviors.None" />, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object will return the response immediately without the need to call the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method. However, the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.IsLastResponse" /> property will always return 
        /// False, even when the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object contains the last response.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" 
        /// /> 
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T}(``0)" />
        public bool IsLastResponse
        {
            get
            {
                return this.isLastResponse;
            }
        }

        /// <summary>
        ///   <para>Gets the response message that is associated with the response.</para>
        /// </summary>
        /// <value>
        ///   <para>The response message, which has a type that you created by adding a 
        /// service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the response is a SOAP fault, an exception based on that fault occurs when you access this property.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.GetUserData{T}" />
        public TMessage Result
        {
            get
            {
                // If the BrokerResponse is a SOAP message, convert the SOAP message to an object or fault exception
                if (this.exception == null)
                {
                    Message message = this.messageBuffer.CreateMessage();

                    if (!message.IsFault)
                    {
                        return (TMessage)this.typedMessageConverter.FromMessage(message);
                    }
                    else
                    {
                        MessageFault messageFault = MessageFault.CreateFault(message, Constant.MaxBufferSize);
                        string action = message.Headers.Action;

                        if (messageFault.HasDetail)
                        {
                            Exception brokerException;
                            if (TryParseBrokerException(messageFault, action, out brokerException))
                            {
                                throw brokerException;
                            }

                            FaultDescription faultDescription = this.faultCollection.Find(action);
                            if (faultDescription != null)
                            {
                                object detail;
                                ConstructorInfo info;
                                try
                                {
                                    info = typeof(FaultException<>).MakeGenericType(faultDescription.DetailType).GetConstructor(new Type[] { faultDescription.DetailType, typeof(FaultReason) });
                                    MethodInfo getDetailMethodInfo = GetDetailMethodInfoBase.MakeGenericMethod(faultDescription.DetailType);
                                    detail = getDetailMethodInfo.Invoke(messageFault, null);
                                }
                                catch (Exception)
                                {
                                    // Swallow the exception and set the detail to null to throw a general fault exception
                                    info = null;
                                    detail = null;
                                }

                                if (info != null)
                                {
                                    FaultException exception = (FaultException)info.Invoke(new object[] { detail, messageFault.Reason });
                                    throw exception;
                                }
                            }
                        }

                        ThrowIfFaultUnderstood(message, messageFault, action, message.Version);

                        return default(TMessage);
                    }
                }

                // Otherwise the BrokerResponse is a client side exception, so throw when user accesses the result
                else
                {
                    throw this.exception;
                }
            }
        }

        /// <summary>
        /// Try to parse broker exception from the message
        /// </summary>
        /// <param name="messageFault">indicating the message fault</param>
        /// <param name="action">indicating the action</param>
        /// <param name="brokerException">output the broker exception</param>
        /// <returns>returns a value indicating whether successfully parsed out broker exception</returns>
        private static bool TryParseBrokerException(MessageFault messageFault, string action, out Exception brokerException)
        {
            switch (action)
            {
                case AuthenticationFailure.Action:
                    AuthenticationFailure af = messageFault.GetDetail<AuthenticationFailure>();
                    brokerException = new AuthenticationException(String.Format(SR.Broker_AuthenticationFailure, af.UserName));
                    return true;
                case RetryOperationError.Action:
                    RetryOperationError rle = messageFault.GetDetail<RetryOperationError>();
                    brokerException = new RetryOperationException(String.Format(SR.Broker_RetryLimitExceeded, rle.RetryCount, rle.Reason), rle.Reason);
                    return true;
                case SessionFault.Action:
                    SessionFault fault = messageFault.GetDetail<SessionFault>();
                    brokerException = Utility.TranslateFaultException(new FaultException<SessionFault>(fault, messageFault.Reason));
                    return true;
                default:
                    brokerException = null;
                    return false;
            }
        }

        /// <summary>
        ///   <para>Retrieves the data in the response that originates from the data that you passed to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T}(``0,System.Object)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T}(``0,System.Object,System.String)" /> method so that you could correlate requests and responses.</para> 
        /// </summary>
        /// <typeparam name="T">
        ///   <para>The type of the object that includes the data that you originally passed to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T}(``0,System.Object)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T}(``0,System.Object,System.String)" /> method.</para>
        /// </typeparam>
        /// <returns>
        ///   <para>The object that includes the data that you originally passed to the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T}(``0,System.Object)" /> or 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T}(``0,System.Object,System.String)" /> method.</para>
        /// </returns>
        public T GetUserData<T>()
        {
            // BrokerResponse is a client side exception, so throw it when user accesses the result.
            if (this.exception != null)
            {
                throw this.exception;
            }

            Message message = this.messageBuffer.CreateMessage();
            int index = message.Headers.FindHeader(Constant.UserDataHeaderName, Constant.HpcHeaderNS);

            if (index != -1)
            {
                try
                {
                    T value = message.Headers.GetHeader<T>(index);
                    return value;
                }
                catch (SerializationException)
                {
                    // If the user data cannot be deserialized it, return type's default value
                    return default(T);
                }
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Converts fault to exception and throws it
        /// </summary>
        /// <param name="response">Response message</param>
        /// <param name="fault">Fault message</param>
        /// <param name="action">SOAP Action</param>
        /// <param name="version">Message Version</param>
        private static void ThrowIfFaultUnderstood(
                            Message response,
                            MessageFault fault,
                            string action,
                            MessageVersion version)
        {
            Exception exception;
            bool isSenderFault;
            bool isReceiverFault;
            FaultCode subCode;
            FaultConverter faultConverter = FaultConverter.GetDefaultFaultConverter(version);

            if (faultConverter.TryCreateException(response, fault, out exception))
            {
                throw exception;
            }

            if (version.Envelope == EnvelopeVersion.Soap11)
            {
                isSenderFault = true;
                isReceiverFault = true;
                subCode = fault.Code;
            }
            else
            {
                isSenderFault = fault.Code.IsSenderFault;
                isReceiverFault = fault.Code.IsReceiverFault;
                subCode = fault.Code.SubCode;
            }

            if ((subCode != null) && (subCode.Namespace != null))
            {
                if (isSenderFault)
                {
                    if (string.Compare(subCode.Namespace, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher", StringComparison.Ordinal) == 0)
                    {
                        if (string.Compare(subCode.Name, "SessionTerminated", StringComparison.Ordinal) == 0)
                        {
                            throw new ChannelTerminatedException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                        }

                        if (string.Compare(subCode.Name, "TransactionAborted", StringComparison.Ordinal) == 0)
                        {
                            throw new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                        }
                    }
                }

                if (isReceiverFault && (string.Compare(subCode.Namespace, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher", StringComparison.Ordinal) == 0))
                {
                    if (string.Compare(subCode.Name, "InternalServiceFault", StringComparison.Ordinal) == 0)
                    {
                        if (fault.HasDetail)
                        {
                            ExceptionDetail detail = fault.GetDetail<ExceptionDetail>();
                            throw new FaultException<ExceptionDetail>(detail, fault.Reason, fault.Code, action);
                        }

                        throw new FaultException(fault, action);
                    }

                    if (string.Compare(subCode.Name, "DeserializationFailed", StringComparison.Ordinal) == 0)
                    {
                        throw new ProtocolException(fault.Reason.GetMatchingTranslation(CultureInfo.CurrentCulture).Text);
                    }
                }
            }

            throw new FaultException(fault);
        }

        /// <summary>
        ///   <para>Frees resources before the object is reclaimed by garbage collection.</para>
        /// </summary>
        ~BrokerResponse()
        {
            Dispose();
        }

        #region IDisposable Members

        /// <summary>
        ///   <para>Releases all of the resources that the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object used.</para>
        /// </summary>
        /// <remarks>
        ///   <para>Call the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.Dispose" /> method when you finish using the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object.</para>
        /// </remarks>
        public void Dispose()
        {
            if (this.messageBuffer != null)
            {
                this.messageBuffer.Close();
                this.messageBuffer = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
