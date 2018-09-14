//------------------------------------------------------------------------------
// <copyright file="IGenericService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Service contract for generic service
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.GenericService
{
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para>Defines a contract for a generic service.</para>
    /// </summary>
    /// <remarks>
    ///   <para>This interface is provided so that you can control serialization and deserialization 
    /// of your types if, for example, you write your client applications in languages other than C#.</para>
    /// </remarks>
    [ServiceContract(Name = "IGenericService", Namespace = "http://hpc.microsoft.com/GenericService")]
    public interface IGenericService
    {
        /// <summary>
        ///   <para>When overridden in a derived class, provides a generic interface that accepts a 
        /// <see cref="System.String" /> and returns a 
        /// <see cref="System.String" />.</para>
        /// </summary>
        /// <param name="args">
        ///   <para>The arguments for the service.</para>
        /// </param>
        /// <returns>
        ///   <para>When overridden in a derived class, returns a 
        /// <see cref="System.String" /> that contains the serialized result that the service computes.</para>
        /// </returns>
        [OperationContract(Action = GenericServiceRequest.Action, ReplyAction = GenericServiceResponse.Action)]
        [FaultContract(typeof(RetryOperationError), Action = RetryOperationError.Action)]
        [FaultContract(typeof(AuthenticationFailure), Action = AuthenticationFailure.Action)]
        string GenericOperation(string args);
    }
}
