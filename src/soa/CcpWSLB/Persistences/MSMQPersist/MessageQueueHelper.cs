//-----------------------------------------------------------------------
// <copyright file="MessageQueueHelper.cs" company="Microsoft">
//     Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>the MSMQ helper class.</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BrokerStorage.MSMQ
{
    using System;
    using System.Globalization;
    using System.Messaging;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;

    /// <summary>
    /// the delegate for the retriable operations.
    /// </summary>
    internal delegate void RetriableOperation(int retryNumber);

    /// <summary>
    /// the helper class for messasge queue.
    /// </summary>
    internal static class MessageQueueHelper
    {
        /// <summary>
        /// the retry count.
        /// </summary>
        public const int RetryCount = 3;

        /// <summary>
        /// the helper function to perform the retriable operations.
        /// </summary>
        /// <param name="retriableOperation">the delegation method.</param>
        /// <param name="format">the error format string when exception raised.</param>
        /// <param name="objParams">the params for the format string.</param>
        public static void PerformRetriableOperation(RetriableOperation retriableOperation, string format, params object[] objParams)
        {
            int retryNumber = 0;
            do
            {
                try
                {
                    retriableOperation(retryNumber);
                    return;
                }
                catch (MessageQueueException e)
                {
                    // the msmq queue cannot access, then retry 3 times.
                    if (MessageQueueHelper.CanRetry(e))
                    {
                        retryNumber++;
                        if (retryNumber < MessageQueueHelper.RetryCount)
                        {
                            string errorMessage = string.Empty;
                            if (!string.IsNullOrEmpty(format))
                            {
                                errorMessage = string.Format(CultureInfo.InvariantCulture, format, objParams) + ", and";
                            }

                            errorMessage += "Will perform retry[" + retryNumber.ToString(CultureInfo.InvariantCulture) + "], the exception:" + e.ToString();
                            BrokerTracing.TraceWarning(errorMessage);
                        }
                        else
                        {
                            throw MessageQueueHelper.ConvertMessageQueueException(e);
                        }
                    }
                    else
                    {
                        throw MessageQueueHelper.ConvertMessageQueueException(e);
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                System.Threading.Thread.Sleep(100);
            }
            while (retryNumber > 0);
        }

        /// <summary>
        /// handle the message async callback error for begin peek/receive.
        /// </summary>
        /// <param name="e">the exception.</param>
        /// <param name="peekAction">the peek action that maybe need reset for the error.</param>
        /// <param name="format">the format string for tracing.</param>
        /// <param name="objParams">the parameteres for tracing.</param>
        /// <returns>a value indicate whether need retry the async IO.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public static bool HandleAsyncCallbackError(MessageQueueException e, ref PeekAction peekAction, string format, params object[] objParams)
        {
            bool needRetry = false;
            if (e.MessageQueueErrorCode == MessageQueueErrorCode.IllegalCursorAction
                    || e.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
            {
                // the cursor is moved to the end of the queue, then the need set the peek action to peek the new come in messages.
                needRetry = true;
                peekAction = PeekAction.Current;
            }
            else if (e.MessageQueueErrorCode == MessageQueueErrorCode.OperationCanceled)
            {
                // Question: Can a thread performs an asynchronous Receive call and then exits?
                //    No. This can lead to the cancellation of the Receive call. A thread which makes an asynchronous Receive call (either receive with callback or receive with overlap) must be alive as long as the Receive call is pending in the MSMQ driver. If the thread terminates and exits before the Receive call completes then the Receive operation is cancelled and the application gets back the error code 0xc0000120 (STATUS_CANCELLED).
                // For VB/COM code, this means that a thread which calls EnableNotification must be alive until the Arrived (or ArrivedError) events are called.
                // For .NET framework, this means that a thread which calls BeginPeek or BeginReceive must be alive until the completion delegate is called.
                // It is legitimate to terminate the calling thread before the Receive operation is completed if the callback (or notification) code is designed to accept and handle the STATUS_CANCELLED error.
                // the MSMQPersist does not use the dedicated thread to peek the responses or requests from the message queue. but the threadpool maybe shrink and some threads in the pool that have pending peek operation exit will cancel the pending peek/receive operation.
                // so, need retry the peek/receive operation when OperationCanceled exception raised.
                needRetry = true;
            }

            if (needRetry)
            {
                string errorMessage = string.Empty;
                if (!string.IsNullOrEmpty(format))
                {
                    errorMessage = string.Format(format, objParams) + ", and";
                }

                errorMessage += " the exception: " + e.ToString();
                BrokerTracing.TraceInfo(errorMessage);
            }

            return needRetry;
        }

        /// <summary>
        /// convert the message queue exception to the broker queue exception.
        /// </summary>
        /// <param name="e">the message queue exception.</param>
        /// <returns>the broker queue exception.</returns>
        public static BrokerQueueException ConvertMessageQueueException(MessageQueueException e)
        {
            BrokerQueueException exception = null;

            if (e != null)
            {
                BrokerQueueErrorCode errorCode = MapToBrokerQueueErrorCode(e.MessageQueueErrorCode);
                exception = new BrokerQueueException((int)errorCode, e.ToString());
            }
            return exception;
        }

#if MSMQ
        /// <summary>
        /// convert the message queue native exception to the broker queue exception.
        /// </summary>
        /// <param name="e">the message queue native exception.</param>
        /// <returns>the broker queue exception.</returns>
        public static BrokerQueueException ConvertMessageQueueException(MessageQueueNativeException e)
        {  
            BrokerQueueException exception = null;

            if (e != null)
            {
                BrokerQueueErrorCode errorCode = MapToBrokerQueueErrorCode((MessageQueueErrorCode)e.ErrorCode);
                exception = new BrokerQueueException((int)errorCode, e.ErrorMessage);
            }
            return exception;
        }
#endif

        /// <summary>
        /// map MessageQueueErrorCode to BrokerQueueErrorCode.
        /// </summary>
        /// <param name="e">the MessageQueueErrorCode.</param>
        /// <returns>the BrokerQueueErrorCode.</returns>
        public static BrokerQueueErrorCode MapToBrokerQueueErrorCode(MessageQueueErrorCode mqError)
        {           
            BrokerQueueErrorCode errorCode = BrokerQueueErrorCode.E_BQ_UNKOWN_ERROR;
            switch (mqError)
            {
                case MessageQueueErrorCode.InsufficientResources:
                    errorCode = BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_INSUFFICIENT;
                    break;
                case MessageQueueErrorCode.MessageStorageFailed:
                    errorCode = BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_FAIL;
                    break;
                case MessageQueueErrorCode.IOTimeout:
                    errorCode = BrokerQueueErrorCode.E_BQ_PERSIST_OPERATION_TIMEOUT;
                    break;
                case MessageQueueErrorCode.ServiceNotAvailable:
                    errorCode = BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_NOTAVAILABLE;
                    break;
            }

            return errorCode;
        }

        /// <summary>
        /// to judge whether the specified exception is retriable.
        /// </summary>
        /// <param name="e">the specfic exception</param>
        /// <returns>a value indicating whether the exception is retriable.</returns>
        private static bool CanRetry(MessageQueueException e)
        {
            if (e == null)
            {
                return true;
            }

            switch (e.MessageQueueErrorCode)
            {
                case MessageQueueErrorCode.DeleteConnectedNetworkInUse:
                case MessageQueueErrorCode.DsIsFull:
                case MessageQueueErrorCode.DtcConnect:
                case MessageQueueErrorCode.IOTimeout:
                case MessageQueueErrorCode.SharingViolation:
                    return true;
            }

            return false;
        }
    }
}
