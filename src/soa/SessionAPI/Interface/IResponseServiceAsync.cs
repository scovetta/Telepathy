// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System;
    using System.ServiceModel;

    using Microsoft.Telepathy.Session.Exceptions;

    /// <summary>
    /// Async version of IResponseService
    /// </summary>
    [ServiceContract(Name = "ResponseService", CallbackContract = typeof(IResponseServiceCallback), Namespace = "http://hpc.microsoft.com")]
    public interface IResponseServiceAsync
    {
        /// <summary>
        /// Get specifies response messages
        /// </summary>
        /// <param name="action">Which resonses to return</param>
        /// <param name="clientData">Client data to return in response message headers</param>
        /// <param name="position">Position in the enum to start (start or current)</param>
        /// <param name="count">Number of messages to return</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void GetResponses(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId);

        /// <summary>
        /// Get specifies response messages
        /// </summary>
        /// <param name="action">Which resonses to return</param>
        /// <param name="clientData">Client data to return in response message headers</param>
        /// <param name="position">Position in the enum to start (start or current)</param>
        /// <param name="count">Number of messages to return</param>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetResponses(string action, string clientData, GetResponsePosition resetToBegin, int count, string clientId, AsyncCallback callback, object state);

        /// <summary>
        /// Get specifies response messages
        /// </summary>
        void EndGetResponses(IAsyncResult result);
    }
}
