//------------------------------------------------------------------------------
// <copyright file="IBrokerManagementService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Interface for Broker Management Service
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Interface for Broker Management Service
    /// </summary>
    [ServiceContract(Name = "IBrokerManagementService", Namespace = "http://hpc.microsoft.com/brokermanagement/")]
    internal interface IBrokerManagementService
    {
        /// <summary>
        /// Ask broker to initialize
        /// </summary>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        /// <param name="clusterEnvs">indicating the cluster envs</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerInitializationResult Initialize(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo);

        /// <summary>
        /// Async version to ask broker to initialize
        /// </summary>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        /// <param name="clusterEnvs">indicating the cluster envs</param>
        /// <param name="callback">indicating the callback</param>
        /// <param name="state">indicating the state</param>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginInitialize(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo, AsyncCallback callback, object state);

        /// <summary>
        /// Operation to receive session info for async version of Initialize
        /// </summary>
        /// <param name="result">indicating the async result</param>
        /// <returns>returns session info</returns>
        BrokerInitializationResult EndInitialize(IAsyncResult result);

        /// <summary>
        /// Attach to the broker
        /// broker would throw exception if it does not allow client to attach to it
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Attach();

        /// <summary>
        /// Async version to attach to the broker
        /// broker would throw exception if it does not allow client to attach to it
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginAttach(AsyncCallback callback, object state);

        /// <summary>
        /// Operation to finish attach
        /// </summary>
        /// <param name="result">indicating the async result</param>
        void EndAttach(IAsyncResult result);

        /// <summary>
        /// Ask to close the broker
        /// </summary>
        /// <param name="suspended">indicating whether the broker is asked to be suspended or closed</param>
        [OperationContract]
        void CloseBroker(bool suspended);


        /// <summary>
        /// Async version: Ask to close the broker
        /// </summary>
        /// <param name="suspended">indicating whether the broker is asked to be suspended or closed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State</param>
        /// <returns>The IAsyncResult instance</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginCloseBroker(bool suspended, AsyncCallback callback, object state);

        /// <summary>
        /// Finish CloseBroker operation
        /// </summary>
        /// <param name="result">The IAsyncResult</param>
        void EndCloseBroker(IAsyncResult result);
    }
}
