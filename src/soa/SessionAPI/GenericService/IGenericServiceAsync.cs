//------------------------------------------------------------------------------
// <copyright file="IGenericServiceAsync.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Async version service contract for generic service
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.GenericService
{
    using System;
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para>Represents a contract for an asynchronous generic service.</para>
    /// </summary>
    /// <remarks>
    ///   <para>This interface is provided so that you can control serialization and deserialization of 
    /// your types if, for example, you write your client application in a language other than C#.</para>
    /// </remarks>
    [ServiceContract(Name = "IGenericService", Namespace = "http://hpc.microsoft.com/GenericService")]
    public interface IGenericServiceAsync
    {
        /// <summary>
        ///   <para>When implemented in a derived class, performs a complete generic operation.</para>
        /// </summary>
        /// <param name="args">
        ///   <para>The arguments for the operation.</para>
        /// </param>
        /// <returns>
        ///   <para>Returns a <see cref="System.String" /> that representtt the results of the operation.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Implement this method so that it begins and ends the operation.</para>
        /// </remarks>
        [OperationContract(Action = GenericServiceRequest.Action, ReplyAction = GenericServiceResponse.Action)]
        [FaultContract(typeof(RetryOperationError), Action = RetryOperationError.Action)]
        [FaultContract(typeof(AuthenticationFailure), Action = AuthenticationFailure.Action)]
        string GenericOperation(string args);

        /// <summary>
        ///   <para>When implemented in a derived class, begins an asynchronous generic operation.</para>
        /// </summary>
        /// <param name="args">
        ///   <para>The arguments for the generic operation.</para>
        /// </param>
        /// <param name="callback">
        ///   <para>The method that receives the callback on completion.</para>
        /// </param>
        /// <param name="state">
        ///   <para>The current state of the client object.</para>
        /// </param>
        /// <returns>
        ///   <para>Returns an <see cref="System.IAsyncResult" /> object that contains the result of the operation.</para>
        /// </returns>
        [OperationContract(AsyncPattern = true, Action = GenericServiceRequest.Action, ReplyAction = GenericServiceResponse.Action)]
        IAsyncResult BeginGenericOperation(string args, AsyncCallback callback, object state);

        /// <summary>
        ///   <para>When implemented in a derived class, ends an asynchronous generic operation.</para>
        /// </summary>
        /// <param name="result">
        ///   <para>The result of the operation.</para>
        /// </param>
        /// <returns>
        ///   <para>Returns a <see cref="System.String" /> that represents the result of the operation.</para>
        /// </returns>
        string EndGenericOperation(IAsyncResult result);
    }
}
