// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System.ServiceModel;

    using Microsoft.Telepathy.Session.Exceptions;

    /// <summary>
    /// Interface for Broker Management Service
    /// </summary>
    [ServiceContract(Name = "IBrokerManagementService", Namespace = "http://hpc.microsoft.com/brokermanagement/")]
    public interface IBrokerManagementService
    {
        /// <summary>
        /// Ask broker to initialize
        /// </summary>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        /// <returns>returns broker initialization result</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerInitializationResult Initialize(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo);

        /// <summary>
        /// Attach to the broker
        /// broker would throw exception if it does not allow client to attach to it
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Attach();

        /// <summary>
        /// Ask to close the broker
        /// </summary>
        /// <param name="suspended">indicating whether the broker is asked to be suspended or closed</param>
        [OperationContract]
        void CloseBroker(bool suspended);
    }
}
