// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para />
    /// </summary>
    public abstract class BrokerClientBase : IDisposable
    {
        /// <summary>
        /// Stores the default client id for Hpc SOA web service
        /// </summary>
        private const string DefaultClientIdForRestService = "default";

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected string clientId;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected TransportScheme transportScheme = TransportScheme.None;

        /// <summary>
        /// Gets the <see cref="TransportScheme"/>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public TransportScheme TransportScheme
        {
            get { return this.transportScheme; }
        }

        /// <summary>
        /// Shutdown lock
        /// </summary>
        protected object objectLock = new object();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected OperationDescriptionCollection operations;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected SessionBase session;

        /// <summary>
        /// All BrokerClients need to know when broker goes down
        /// </summary>
        public abstract void SendBrokerDownSignal(bool isBrokerNodeDown);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="disposing">
        ///   <para />
        /// </param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="clientId">
        ///   <para />
        /// </param>
        /// <param name="session">
        ///   <para />
        /// </param>
        protected BrokerClientBase(string clientId, SessionBase session)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                if (session.Info is SessionInfo)
                {
                    this.clientId = String.Empty;
                }
                else
                {
                    this.clientId = DefaultClientIdForRestService;
                }
            }
            else
            {
                this.clientId = clientId;
            }

            ParamCheckUtility.ThrowIfNotMatchRegex(ParamCheckUtility.ClientIdValid, this.clientId, "clientId", SR.InvalidClientId);

            // Associate BrokerClient with session
            session.AddBrokerClient(this.clientId, this);

            this.session = session;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void Dispose()
        {
            lock (this.objectLock)
            {
                DetachFromSession();

                Dispose(true);
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Detachs BrokerClient from Session stopping any hearbeat signals
        /// </summary>
        public void DetachFromSession()
        {
            if (this.session != null)
            {
                try
                {
                    this.session.RemoveBrokerClient(this.clientId);
                }

                catch
                {
                    // Forced to eat potential NullReferenceException because detach needs to be called as soon as the BrokerClient is closed to 
                    // stop heartbeats. However if throttling is engaged, objectLock cannot be acquired to protected this.session. 
                }
            }
        }

        /// <summary>
        /// Returns the TypedMessageConverter and operation faults for the specified action and reply action
        /// </summary>
        /// <returns></returns>
        internal bool GetResponseMessageInfo(ref string action, ref string replyAction, ref Type responseType,
                            out TypedMessageConverter typedMessageConverter, out FaultDescriptionCollection faultCollection, out string errorMessage)
        {
            int operationIndex = 0;
            int operationMatchIndex = -1;
            int actionMatchIndex = -1;
            int replyActionMatchIndex = -1;
            int operationQueryID = -1;

            typedMessageConverter = null;
            faultCollection = null;
            errorMessage = "Unexpected error";

            // If caller explicitly passes action and replyAction to GetResponses or AddResponseHandler, look for both
            if (!String.IsNullOrEmpty(action) && !String.IsNullOrEmpty(replyAction))
            {
                operationQueryID = 0;
            }

            // If the caller passes just a response message type to GetResponses or AddResponseHandler, look for response type
            else if (responseType != null)
            {
                operationQueryID = 1;
            }

            // If the caller is using a typeless GetResponses or AddResponseHander, look for action
            else if (!String.IsNullOrEmpty(action))
            {
                operationQueryID = 2;
            }

            // Otherwise we have unexecpted input
            else
            {
                return false;
            }

            // Loop through all the message descriptions of all the operations and look for matches
            foreach (OperationDescription operationDescription in this.operations)
            {
                bool actionMatch = false;
                bool replyActionMatch = false;
                int actionIndex = -1;
                int replyActionIndex = -1;
                int messageDescriptonIndex = 0;

                foreach (MessageDescription messageDescription in operationDescription.Messages)
                {
                    if (messageDescription.Direction == MessageDirection.Input)
                    {
                        if (messageDescription.Action == action)
                        {
                            actionMatch = true;
                        }

                        actionIndex = messageDescriptonIndex;
                    }
                    else if (messageDescription.Direction == MessageDirection.Output)
                    {
                        if (messageDescription.Action == replyAction || messageDescription.MessageType == responseType)
                        {
                            replyActionMatch = true;
                        }

                        replyActionIndex = messageDescriptonIndex;
                    }

                    messageDescriptonIndex++;
                }

                // If we are looking for an exact match AND found matching action and reply actions
                //      OR we are looking for a replyAction match AND found a matching reply action
                //      OR we are looking for a action match AND found a matching action
                if ((operationQueryID == 0 && actionMatch && replyActionMatch) ||
                    (operationQueryID == 1 && replyActionMatch) ||
                    (operationQueryID == 2 && actionMatch))
                {
                    // If a previous operation was already matched, return ambiguous error
                    if (operationMatchIndex != -1)
                    {
                        errorMessage = SR.AmbiguousOperation;
                        return false;
                    }

                    // Else save the operation and its reply message description
                    operationMatchIndex = operationIndex;
                    actionMatchIndex = actionIndex;
                    replyActionMatchIndex = replyActionIndex;
                }

                operationIndex++;
            }

            // If no matching operation was found, return operation not found
            if (operationMatchIndex == -1)
            {
                errorMessage = "Operation not found for specified actions";
                return false;
            }

            // Create typedMessageCoverter and get fault collection 
            OperationDescription operation = this.operations[operationMatchIndex];
            MessageDescription requestMessageDescription = operation.Messages[actionMatchIndex];
            MessageDescription replyMessageDescription = operation.Messages[replyActionMatchIndex];

            typedMessageConverter = TypedMessageConverter.Create(replyMessageDescription.MessageType, replyMessageDescription.Action);
            action = requestMessageDescription.Action;
            replyAction = replyMessageDescription.Action;
            responseType = replyMessageDescription.MessageType;
            faultCollection = operation.Faults;

            Debug.Assert(responseType != typeof(object) ? responseType == replyMessageDescription.MessageType : true, "Unexpected action/response type match!");

            return true;
        }

        /// <summary>
        /// Pull request action header from response message. Broker will set this
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        internal static string GetActionFromResponseMessage(Message response)
        {
            int index = response.Headers.FindHeader(Constant.ActionHeaderName, Constant.HpcHeaderNS);
            string ret = null;

            if (index >= 0)
            {
                ret = response.Headers.GetHeader<string>(index);
            }

            return ret;
        }
    }
}
