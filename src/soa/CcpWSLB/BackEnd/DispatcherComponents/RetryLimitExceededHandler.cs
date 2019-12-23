// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd.DispatcherComponents
{
    using System;
    using System.Diagnostics.Contracts;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// Handle retry limit exceeded 
    /// </summary>
    internal class RetryLimitExceededHandler
    {
        /// <summary>
        /// Stores shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// The response queue adapter.
        /// </summary>
        private ResponseQueueAdapter responseQueueAdapter;

        /// <summary>
        /// Construct the RetryLimitExceededHandler
        /// </summary>
        /// <param name="sharedData"></param>
        /// <param name="responseQueueAdapter"></param>
        public RetryLimitExceededHandler(SharedData sharedData, ResponseQueueAdapter responseQueueAdapter)
        {
            this.sharedData = sharedData;
            // TODO: we should remove the reference to ResponseQueueAdapter when dispatcher switch to new engine 
            this.responseQueueAdapter = responseQueueAdapter;
        }

        /// <summary>
        /// Handle retry limit exceeded
        /// </summary>
        /// <param name="data"></param>
        public void HandleRetryLimitExceeded(DispatchData data)
        {
            // handle retry limit only when we get exception or fault message
            Contract.Requires(data.Exception != null || 
                (data.ReplyMessage != null && data.ReplyMessage.IsFault == true), 
                "Retry Limit Exceeded when there is exception or fault message");

            int retryLimit = this.sharedData.Config.LoadBalancing.MessageResendLimit;
            BrokerTracing.EtwTrace.LogBackendResponseReceivedRetryLimitExceed(data.SessionId, data.TaskId, data.MessageId, retryLimit);

            // Initialize retry error and fault reason
            RetryOperationError retryError;
            FaultReason faultReason;

            if (data.Exception != null)
            {
                // generate fault exception from original exception
                retryError = new RetryOperationError(data.Exception.Message);

                string exceptionFaultReason = String.Format(SR.RetryLimitExceeded, retryLimit + 1, data.Exception.Message);
                faultReason = new FaultReason(exceptionFaultReason);
            }
            else
            {
                #region Debug Failure Test
                SimulateFailure.FailOperation(1);
                #endregion

                // generate fault exception from original reply
                MessageFault messageFault = MessageFault.CreateFault(data.ReplyMessage, Constant.MaxBufferSize);
                retryError = messageFault.GetDetail<RetryOperationError>();
                faultReason = messageFault.Reason;

                // close original reply message
                data.ReplyMessage.Close();
            }

            retryError.RetryCount = retryLimit + 1;
            retryError.LastFailedServiceId = data.TaskId;

            // Construct the new exception
            FaultException<RetryOperationError> faultException = new FaultException<RetryOperationError>(retryError, faultReason, Constant.RetryLimitExceedFaultCode, RetryOperationError.Action);

            data.Exception = faultException;
            this.responseQueueAdapter.PutResponseBack(data).GetAwaiter().GetResult();
        }
    }
}
