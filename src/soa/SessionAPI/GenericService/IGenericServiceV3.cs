// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.GenericService
{
    using System.ServiceModel;

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
