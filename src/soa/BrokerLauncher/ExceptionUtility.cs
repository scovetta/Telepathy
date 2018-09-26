﻿//------------------------------------------------------------------------------
// <copyright file="ExceptionUtility.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Exception utilities
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Messaging;
    using System.Runtime.Remoting;
    using System.ServiceModel;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using System.Text;

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