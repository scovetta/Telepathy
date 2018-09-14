//------------------------------------------------------------------------------
// <copyright file="IDataServiceAsync.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Async version of the data service interface
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    /// <summary>
    /// Async version of the data service interface
    /// </summary>
    [ServiceContract(Name = "IDataService", Namespace = "http://hpc.microsoft.com/dataservice/")]
    internal interface IDataServiceAsync
    {
        /// <summary>
        /// Begin method of CreateDataClient
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        IAsyncResult BeginCreateDataClient(string dataClientId, string[] allowedUsers, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of CreateDataClient
        /// </summary>
        /// <param name="result">async result</param>
        /// <returns>data store path of the data client</returns>
        string EndCreateDataClient(IAsyncResult result);

        /// <summary>
        /// Begin method of CreateDataClientV4
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <param name="location">data location</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        IAsyncResult BeginCreateDataClientV4(string dataClientId, string[] allowedUsers, DataLocation location, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of CreateDataClientV4
        /// </summary>
        /// <param name="result">async result</param>
        /// <returns>information for accessing the data client</returns>
        DataClientInfo EndCreateDataClientV4(IAsyncResult result);

        /// <summary>
        /// Begin method of OpenDataClient
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        IAsyncResult BeginOpenDataClient(string dataClientId, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of OpenDataClient
        /// </summary>
        /// <param name="result">async result</param>
        /// <returns>information for accessing the data client</returns>
        string EndOpenDataClient(IAsyncResult result);

        /// <summary>
        /// Begin method of OpenDataClient
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        IAsyncResult BeginOpenDataClientV4(string dataClientId, DataLocation location, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of OpenDataClient
        /// </summary>
        /// <param name="result">async result</param>
        /// <returns>information for accessing the data client</returns>
        DataClientInfo EndOpenDataClientV4(IAsyncResult result);

        /// <summary>
        /// Begin method of BeginOpenDataClientBySecret
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        IAsyncResult BeginOpenDataClientBySecret(string dataClientId, int jobId, string jobSecret, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of OpenDataClient
        /// </summary>
        /// <param name="result">async result</param>
        /// <returns>information for accessing the data client</returns>
        DataClientInfo EndOpenDataClientBySecret(IAsyncResult result);

        /// <summary>
        /// Begin method of DeleteDataClient
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        IAsyncResult BeginDeleteDataClient(string dataClientId, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of DeleteDataClient
        /// </summary>
        /// <param name="result">async result</param>
        void EndDeleteDataClient(IAsyncResult result);

        /// <summary>
        /// Begin method of AssociateDataClientWithSession
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="sessionId">session id</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        IAsyncResult BeginAssociateDataClientWithSession(string dataClientId, int sessionId, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of AssociateDataClientWithSession
        /// </summary>
        /// <param name="result">async result</param>
        void EndAssociateDataClientWithSession(IAsyncResult result);

        /// <summary>
        /// Begin method of WriteDone
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="callback">async callback</param>
        /// <param name="asyncState">async state</param>
        /// <returns>async result</returns>
        [OperationContract(AsyncPattern = true)]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        IAsyncResult BeginWriteDone(string dataClientId, AsyncCallback callback, object asyncState);

        /// <summary>
        /// End method of WriteDone
        /// </summary>
        /// <param name="result">async result</param>
        void EndWriteDone(IAsyncResult result);
    }
}