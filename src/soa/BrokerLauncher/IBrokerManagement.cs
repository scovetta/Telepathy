//------------------------------------------------------------------------------
// <copyright file="IBrokerManagement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Interface for Broker Managment
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ServiceModel;

    /// <summary>
    /// Interface for BrokerManagement service
    /// </summary>
    [ServiceContract(Name = "IBrokerManagement", Namespace = "http://hpc.microsoft.com/brokerlauncher/")]
    internal interface IBrokerManagement
    {
        /// <summary>
        /// Takes broker offline
        /// </summary>
        /// <param name="forced">Force sessions to end</param>
        [OperationContract]
        void StartOffline(bool force);

        /// <summary>
        /// Is the broker offline?
        /// </summary>
        /// <returns>True if is offline; Otherwise false</returns>
        [OperationContract]
        bool IsOffline();

        /// <summary>
        /// Takes broker online
        /// </summary>
        [OperationContract]
        void Online();
    }
}
