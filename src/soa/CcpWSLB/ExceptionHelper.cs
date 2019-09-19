// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.Common
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceModel;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.SvcBroker;

    /// <summary>
    /// the exception helper class.
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>
        /// convert the exception to the fault exception that can be send back to the client.
        /// </summary>
        /// <param name="e">the exception.</param>
        /// <returns>the converted fault exception.</returns>
        public static FaultException<SessionFault> ConvertExceptionToFaultException(Exception e)
        {
            if (e == null)
            {
                return null;
            }

            FaultException<SessionFault> faultException = e as FaultException<SessionFault>;
            if (faultException != null)
            {
                return new FaultException<SessionFault>(faultException.Detail, faultException.Reason);
            }

            BrokerQueueException brokerQueueException = e as BrokerQueueException;
            if (brokerQueueException != null)
            {
                return ExceptionHelper.ConvertBrokerQueueExceptionToFaultException(brokerQueueException);
            }

            if (e is TimeoutException)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.OperationTimeout, FetchExceptionDetails(e));
            }

            ThrowHelper.ThrowSessionFault(SOAFaultCode.UnknownError, SR.UnknownError, FetchExceptionDetails(e));

            Debug.Fail("Should not reach this line.");
            return null;
        }

        /// <summary>
        /// convert the broker queue exceptioni to the fault exception.
        /// </summary>
        /// <param name="e">the broker queue exception.</param>
        /// <returns>the converted fault exception.</returns>
        public static FaultException<SessionFault> ConvertBrokerQueueExceptionToFaultException(BrokerQueueException e)
        {
            FaultException<SessionFault> faultException = null;
            if (e != null)
            {
                string reason = null;
                SessionFault fault = null;
                string exceptionDetail = FetchExceptionDetails(e);

                switch ((BrokerQueueErrorCode)e.ErrorCode)
                {
                    case BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_NOTAVAILABLE:
                        fault = new SessionFault(SOAFaultCode.StorageServiceNotAvailble, SR.StorageServiceNotAvailble);
                        reason = SR.StorageServiceNotAvailble;
                        break;
                    case BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_INSUFFICIENT:
                        fault = new SessionFault(SOAFaultCode.StorageSpaceNotSufficient, SR.StorageSpaceNotSufficient);
                        reason = SR.StorageSpaceNotSufficient;
                        break;
                    case BrokerQueueErrorCode.E_BQ_PERSIST_STORAGE_FAIL:
                        fault = new SessionFault(SOAFaultCode.StorageFailure, SR.StorageFailure, exceptionDetail);
                        reason = String.Format(CultureInfo.CurrentCulture, SR.StorageFailure, exceptionDetail);
                        break;
                    case BrokerQueueErrorCode.E_BQ_STATUS_CLOSED:
                        fault = new SessionFault(SOAFaultCode.StorageClosed, SR.StorageClosed);
                        reason = SR.StorageClosed;
                        break;
                    default:
                        fault = new SessionFault(SOAFaultCode.UnknownError, SR.UnknownError, exceptionDetail);
                        reason = String.Format(CultureInfo.CurrentCulture, SR.UnknownError, exceptionDetail);
                        break;
                }

                faultException = new FaultException<SessionFault>(fault, reason);
            }

            return faultException;
        }

        /// <summary>
        /// Fetch exception details. In debug build, it returns the error message
        /// including the callstack. For retail build, it returns only the error
        /// message
        /// </summary>
        /// <param name="e">indicating the exception</param>
        /// <returns>returns the exception details</returns>
        private static string FetchExceptionDetails(Exception e)
        {
#if DEBUG
            return e.ToString();
#else
            return e.Message;
#endif
        }
    }
}

