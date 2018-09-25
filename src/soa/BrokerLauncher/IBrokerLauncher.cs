//------------------------------------------------------------------------------
// <copyright file="IBrokerLauncher.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Interface for Broker Launcher
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Interface for Broker Launcher
    /// </summary>
    [ServiceContract(Name = "IBrokerLauncher", Namespace = "http://hpc.microsoft.com/brokerlauncher/")]
    interface IBrokerLauncher
    {
        /// <summary>
        /// Create a new broker
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionId">the session id which is also service job id</param>
        /// <returns>The brokerLauncher's EPRs, client should connect to them by order</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerInitializationResult Create(SessionStartInfoContract info, int sessionId);

        /// <summary>
        /// Create a new broker which working under reliable manner.
        /// </summary>
        /// <param name="info">session start info</param>
        /// <param name="sessionId">the session id which is also service job id</param>
        /// <returns>The brokerLauncher's EPRs, client should connect to them by order</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerInitializationResult CreateDurable(SessionStartInfoContract info, int sessionId);

        /// <summary>
        /// Attach to an exisiting session
        /// </summary>
        /// <param name="sessionId">The session Identity</param>
        /// <returns>the Broker Launcher EPR</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerInitializationResult Attach(int sessionId);

        /// <summary>
        /// Clean up all the resource related to this session.
        /// Finish the session Job
        /// </summary>
        /// <param name="sessionId">The session id</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Close(int sessionId);

        /// <summary>
        /// Pings specified broker
        /// </summary>
        /// <param name="sessionID">indicating the session id</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        bool PingBroker(int sessionID);

        /// <summary>
        /// Pings specified broker. New Version
        /// </summary>
        /// <param name="sessionID">indicating the session id</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        string PingBroker2(int sessionID);

        /// <summary>
        /// Gets the active broker id list
        /// </summary>
        /// <returns>the list of active broker's session id</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        int[] GetActiveBrokerIdList();
    }
}