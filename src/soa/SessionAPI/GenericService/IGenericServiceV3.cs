//------------------------------------------------------------------------------
// <copyright file="IGenericService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Service contract for generic service for v3 client
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.GenericService
{
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Service contract for generic service for v3 client
    /// </summary>
    [ServiceContract(Name = "IGenericService", Namespace = "http://hpc.microsoft.com/GenericService")]
    public interface IGenericServiceV3
    {
        /// <summary>
        /// Perform generic operation
        /// </summary>
        /// <param name="request">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [OperationContract(Action = GenericServiceRequest.Action, ReplyAction = GenericServiceResponse.Action)]
        [FaultContract(typeof(RetryOperationError), Action = RetryOperationError.Action)]
        [FaultContract(typeof(AuthenticationFailure), Action = AuthenticationFailure.Action)]
        GenericServiceResponse GenericOperation(GenericServiceRequest request);
    }
}
