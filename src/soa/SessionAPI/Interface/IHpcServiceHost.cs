//------------------------------------------------------------------------------
// <copyright file="IHpcServiceHost.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Interface for HpcServiceHost
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System.ServiceModel;

    /// <summary>
    /// Interface for HpcServiceHost
    /// </summary>
    [ServiceContract(Name = "IHpcServiceHost", Namespace = "http://hpc.microsoft.com/hpcservicehost/")]
    public interface IHpcServiceHost
    {
        /// <summary>
        /// Exit the service host
        /// </summary>
        [OperationContract(IsOneWay = true, Action = "http://hpc.microsoft.com/hpcservicehost/exit")]
        void Exit();
    }
}
