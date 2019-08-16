//------------------------------------------------------------------------------
// <copyright file="IDataService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The interface for data service
// </summary>
//------------------------------------------------------------------------------
#if HPCPACK
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System.ServiceModel;

    /// <summary>
    /// The interface for data service
    /// </summary>
    [ServiceContract(Name = "IDataService", Namespace = "http://hpc.microsoft.com/dataservice/")]
    internal interface IDataService
    {
        /// <summary>
        /// Create a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <returns>data store path of the data client</returns>
        [OperationContract]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        string CreateDataClient(string dataClientId, string[] allowedUsers);

        /// <summary>
        /// Create a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <param name="location">data location</param>
        /// <returns>information for accessing the data client</returns>
        [OperationContract]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        DataClientInfo CreateDataClientV4(string dataClientId, string[] allowedUsers, DataLocation location);

        /// <summary>
        /// Open a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <returns>information for accessing the data client</returns>
        [OperationContract]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        string OpenDataClient(string dataClientId);

        /// <summary>
        /// Open a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <returns>information for accessing the data client</returns>
        [OperationContract]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        DataClientInfo OpenDataClientV4(string dataClientId, DataLocation location);

        /// <summary>
        /// Delete a data client with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        [OperationContract]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        void DeleteDataClient(string dataClientId);

        /// <summary>
        /// Associate lifecycle of a DataClient with lifecycle of a session
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="sessionId">session id</param>
        [OperationContract]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        void AssociateDataClientWithSession(string dataClientId, int sessionId);

        /// <summary>
        /// Mark a DataClient as write done
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        [OperationContract]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        void WriteDone(string dataClientId);

        /// <summary>
        /// Create a DataClient with the specified data client id, job id and job secret
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(DataFault), Action = DataFault.Action)]
        DataClientInfo OpenDataClientBySecret(string dataClientId, int jobId, string jobSecret);
    }
}
#endif