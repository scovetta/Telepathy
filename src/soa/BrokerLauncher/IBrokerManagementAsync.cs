//------------------------------------------------------------------------------
// <copyright file="IBrokerManagementAsync.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Async version of IBrokerManagement interface
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ServiceModel;

    /// <summary>
    /// Async version of IBrokerMangement interface
    /// </summary>
    [ServiceContract(Name = "IBrokerManagement", Namespace = "http://hpc.microsoft.com/brokerlauncher/")]
    internal interface IBrokerManagementAsync
    {
        /// <summary>
        /// Takes broker offline
        /// </summary>
        /// <param name="forced">Force sessions to end</param>
        [OperationContract]
        void StartOffline(bool force);

        /// <summary>
        /// Async version of StartOffline
        /// </summary>
        /// <param name="force">Force sessions to end</param>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginStartOffline(bool force, System.AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of StartOffline
        /// </summary>
        /// <param name="result">IAsyncResult instance</param>
        void EndStartOffline(IAsyncResult result);

        /// <summary>
        /// Is the broker offline?
        /// </summary>
        /// <returns>True if is offline; Otherwise false</returns>
        [OperationContract]
        bool IsOffline();

        /// <summary>
        /// Async version of IsOffline
        /// </summary>
        /// <returns>IAsyncResult instance</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginIsOffline(System.AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of IsOffline
        /// </summary>
        /// <returns>True if it is offline; otherwise false</returns>
        bool EndIsOffline(IAsyncResult result);

        /// <summary>
        /// Takes broker online
        /// </summary>
        [OperationContract]
        void Online();

        /// <summary>
        /// Async version of Online
        /// </summary>
        /// <returns>IAsyncResult instance</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginOnline(System.AsyncCallback callback, object asyncState);

        /// <summary>
        /// Async version of Online
        /// </summary>
        /// <param name="result">IAsyncResult instance</param>
        void EndOnline(IAsyncResult result);
    }
}
