// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using System;
    using System.Messaging;
    using System.Runtime.Remoting;
    using System.ServiceModel;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Telepathy.Session.Exceptions;

    /// <summary>
    /// Exception utilities
    /// </summary>
    internal static class ExceptionUtility
    {
        /// <summary>
        /// Gets a value indicating whether we should retry the exception
        /// </summary>
        /// <param name="e">indicate the exception</param>
        /// <returns>a value indicating whether we should retry the exception</returns>
        public static bool ShouldRetry(Exception e)
        {
            FaultException<SessionFault> ex = e as FaultException<SessionFault>;
            if (ex != null)
            {
                return ex.Detail.Code == (int)SOAFaultCode.Broker_RegisterJobFailed;
            }

            return e is RemotingException || e is TimeoutException || e is MessageQueueException;
        }
    }
}
