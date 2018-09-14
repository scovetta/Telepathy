//------------------------------------------------------------------------------
// <copyright file="DataProxyAgent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Data proxy agent
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using Microsoft.Hpc.Azure.Common;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using SessionInternal = Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Data proxy agent
    /// </summary>
    internal class DataProxyAgent : DisposableObject ,IDataService
    {
        /// <summary>
        /// Max retry count
        /// </summary>
        private const int MaxRetryCount = 3;

        /// <summary>
        /// Azure proxy file reader that helps read proxy role instances inforation
        /// </summary>
        private static AzureProxyFileReader proxyFileReader = new AzureProxyFileReader();

        /// <summary>
        /// Internal DataServiceAgent instance for talking to individual data proxy instance
        /// </summary>
        private DataServiceAgent internalAgent;

        /// <summary>
        /// Currently selected proxy instance
        /// </summary>
        private string selectedProxyInstance;

        /// <summary>
        /// Excluded proxy instance list
        /// </summary>
        private List<string> excludedProxyInstances = new List<string>();

        /// <summary>
        /// Initializes a new instance of the DataProxyAgent class
        /// </summary>
        public DataProxyAgent()
        {
            this.RecreateInternalAgent();
        }

        /// <summary>
        /// The delegate for retriable operation
        /// </summary>
        /// <typeparam name="T">operation return type</typeparam>
        /// <returns>operation return value</returns>
        private delegate T RetriableOperation<T>();

        /// <summary>
        /// Create a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <returns>data store path of the data client</returns>
        public string CreateDataClient(string dataClientId, string[] allowedUsers)
        {
            return this.RetryHelper<string>(new RetriableOperation<string>(
                delegate
                {
                    return this.internalAgent.CreateDataClient(dataClientId, allowedUsers);
                }));
        }

        /// <summary>
        /// Create a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <param name="location">data location</param>
        /// <returns>information for accessing the data client</returns>
        public DataClientInfo CreateDataClientV4(string dataClientId, string[] allowedUsers, DataLocation location)
        {
            return this.RetryHelper<DataClientInfo>(new RetriableOperation<DataClientInfo>(
                delegate
                {
                    return this.internalAgent.CreateDataClientV4(dataClientId, allowedUsers, location);
                }));
        }

        /// <summary>
        /// Open a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <returns>information for accessing the data client</returns>
        public string OpenDataClient(string dataClientId)
        {
            return this.RetryHelper<string>(new RetriableOperation<string>(
                delegate
                {
                    return this.internalAgent.OpenDataClient(dataClientId);
                }));
        }

        /// <summary>
        /// Open a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <returns>information for accessing the data client</returns>
        public DataClientInfo OpenDataClientV4(string dataClientId, DataLocation location)
        {
            return this.RetryHelper<DataClientInfo>(new RetriableOperation<DataClientInfo>(
                delegate
                {
                    return this.internalAgent.OpenDataClientV4(dataClientId, location);
                }));
        }

        /// <summary>
        /// Delete a data client with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        public void DeleteDataClient(string dataClientId)
        {
            this.RetryHelper<bool>(new RetriableOperation<bool>(
               delegate
               {
                   this.internalAgent.DeleteDataClient(dataClientId);
                   return true;
               }));
        }

        /// <summary>
        /// Associate lifecycle of a DataClient with lifecycle of a session
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="sessionId">session id</param>
        public void AssociateDataClientWithSession(string dataClientId, int sessionId)
        {
            this.RetryHelper<bool>(new RetriableOperation<bool>(
              delegate
              {
                  this.internalAgent.AssociateDataClientWithSession(dataClientId, sessionId);
                  return true;
              }));
        }


        /// <summary>
        /// Mark a DataClient as write done
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        public void WriteDone(string dataClientId)
        {
            this.RetryHelper<bool>(new RetriableOperation<bool>(
              delegate
              {
                  this.internalAgent.WriteDone(dataClientId);
                  return true;
              }));
        }

        DataClientInfo IDataService.OpenDataClientBySecret(string dataClientId, int jobId, string jobSecret)
        {
            throw new ActionNotSupportedException();
        }

        /// <summary>
        /// dispose the object. Suppress the message cause use Utility.SafeCloseCommunicateObject to clean up the object
        /// </summary>
        /// <param name="disposing"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "internalAgent")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.internalAgent != null)
                {
                    SessionInternal.Utility.SafeCloseCommunicateObject(this.internalAgent);
                    this.internalAgent = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Recreate the internal DataServiceAgent instance
        /// </summary>
        private void RecreateInternalAgent()
        {
            // close previouus agent instance
            if (this.internalAgent != null)
            {
                this.excludedProxyInstances.Add(this.selectedProxyInstance);
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataProxyAgent] . Proxy instance {0} is excluded", this.selectedProxyInstance);

                SessionInternal.Utility.SafeCloseCommunicateObject(this.internalAgent);
                this.internalAgent = null;
            }

            List<string> proxyInstances = null;
            try
            {   
                proxyInstances = proxyFileReader.GetProxyInstances(true);

                // remove excludedProxyInstances from all instance list
                foreach (string excludedInstance in this.excludedProxyInstances)
                {
                    proxyInstances.Remove(excludedInstance);
                }

                if (proxyInstances == null || proxyInstances.Count == 0)
                {
                    throw new Exception("No healthy data proxy instance is available");
                }
            }
            catch (Exception ex)
            {
                throw new DataException(DataErrorCode.ConnectDataServiceFailure, ex);
            }

            // pick a proxy instance at random
            int index = (new Random()).Next(proxyInstances.Count);
            this.selectedProxyInstance = proxyInstances[index];

            string dataProxyAddress = string.Format(Constant.DataProxyEndpointFormat, this.selectedProxyInstance, DataProxyPorts.ProxyPort);
            this.internalAgent = new DataServiceAgent(new Uri(dataProxyAddress));
            this.internalAgent.Endpoint.Behaviors.Add(new DataMessageInspector());
        }

        /// <summary>
        /// Helper method that retry the specified operation on certain exceptions
        /// </summary>
        /// <typeparam name="T">operation return type</typeparam>
        /// <param name="operation">retriable operation</param>
        /// <returns>operation return value</returns>
        private T RetryHelper<T>(RetriableOperation<T> operation)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return operation();
                }
                catch (ActionNotSupportedException)
                {
                    throw;
                }
                catch (FaultException<DataFault>)
                {
                    throw;
                }
                catch (CommunicationException ex)
                {
                    TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataProxyAgent] . RetryHelper received exception: retryCount={0}, exception={1}", retryCount, ex);

                    // retry on communication exception
                    if (retryCount++ > MaxRetryCount)
                    {
                        throw;
                    }

                    this.RecreateInternalAgent();
                }
            }
        }
    }
}
