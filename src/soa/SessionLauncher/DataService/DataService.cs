//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="DataService.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>the data serivce.</summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Data.DataProvider;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher;
    using TraceHelper = Microsoft.Hpc.Scheduler.Session.Data.Internal.DataServiceTraceHelper;

    /// <summary>
    /// the data service.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                     ConcurrencyMode = ConcurrencyMode.Multiple,
                     Name = "DataService",
                     Namespace = "http://hpc.microsoft.com/dataservice/")]
    internal class DataService : DisposableObject, IDataService
    {
        /// <summary>
        /// Local system account name
        /// </summary>
        private const string localSystemAccount = @"NT AUTHORITY\SYSTEM";

        /// <summary>
        /// Administrators account name
        /// </summary>
        private const string administratorsAccount = "Administrators";

        /// <summary>
        /// User principal name format
        /// </summary>
        private const string UpnFormat = "{0}@{1}";

        /// <summary>
        /// Sam account name delimeter of user name and domain name
        /// </summary>
        private const string SamAccountNameDelimeter = "\\";

        /// <summary>
        /// Scheduler instance
        /// </summary>
        private IScheduler scheduler;

        /// <summary>
        /// Data management component
        /// </summary>
        private DataManagement dataManagement;

        /// <summary>
        /// DataRequestListener is in charge of listening and processing DataRequests in DataRequests queue
        /// </summary>
        private DataRequestListener requestListener;

        /// <summary>
        /// Data client locks
        /// </summary>
        private Dictionary<string, RefCountedReaderWriterLock> dataClientLocks = new Dictionary<string, RefCountedReaderWriterLock>();

        /// <summary>
        /// Lock object for dataClientLocks
        /// </summary>
        private object lockDataClientLocks = new object();

        /// <summary>
        /// Job user name cache
        /// </summary>
        RRCache<int, Tuple<string, string>> jobUserNameCache = new RRCache<int, Tuple<string, string>>();

        /// <summary>
        /// Lock object for the job user name cache
        /// </summary>
        private object lockJobUserNameCache = new object();

        /// <summary>
        /// ClusterInfo instance
        /// </summary>
        private ClusterInfo clusterInfo;

        /// <summary>
        /// Initializes a new instance of the DataService class
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        /// <param name="scheduler">the scheduler instance</param>
        public DataService(ClusterInfo clusterInfo, IScheduler scheduler)
        {
            this.clusterInfo = clusterInfo;
            this.scheduler = scheduler;
            this.dataManagement = new DataManagement(clusterInfo, scheduler);

#if HPCPACK
            if (!SoaHelper.IsOnAzure())
            {
                // start data request listener for on-premise cluster only
                this.requestListener = new DataRequestListener(clusterInfo, this);
                this.requestListener.Start();
            }

            // register azure storage connection string updated event to reload data service
            this.clusterInfo.OnAzureStorageConnectionStringOrClusterIdUpdated += ClusterInfo_OnAzureStorageConnectionStringOrClusterIdUpdated;
#endif
        }

        /// <summary>
        /// Event handler wrapper to reload data service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClusterInfo_OnAzureStorageConnectionStringOrClusterIdUpdated(object sender, EventArgs e)
        {
            try
            {
                TraceHelper.TraceEvent(TraceEventType.Information, "[DataService] .ClusterInfo_OnAzureStorageConnectionStringOrClusterIdUpdated: ReloadDataServerInfo");
                ReloadDataServerInfo();
            }
            catch(Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .ClusterInfo_OnAzureStorageConnectionStringOrClusterIdUpdated: {0}", ex);
            }
        }

        /// <summary>
        /// Create a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <returns>information for further accessing the data client</returns>
        public string CreateDataClient(string dataClientId, string[] allowedUsers)
        {
            WindowsIdentity callerIdentity = ServiceSecurityContext.Current.WindowsIdentity;
            CheckUserAccess(callerIdentity);

            DataClientInfo info = this.CreateDataClientInternal(callerIdentity, dataClientId, allowedUsers, DataLocation.FileShare);
            return info.PrimaryDataPath;
        }

        /// <summary>
        /// Create a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <param name="location">data location</param>
        /// <returns>information for further accessing the data client</returns>
        public DataClientInfo CreateDataClientV4(string dataClientId, string[] allowedUsers, DataLocation location)
        {
            WindowsIdentity callerIdentity = ServiceSecurityContext.Current.WindowsIdentity;
            CheckUserAccess(callerIdentity);

            return this.CreateDataClientInternal(callerIdentity, dataClientId, allowedUsers, location);
        }

        /// <summary>
        /// Open a DataClient with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <returns>information for accessing the data client</returns>
        public string OpenDataClient(string dataClientId)
        {
            DataClientInfo info = this.OpenDataClientV4(dataClientId, DataLocation.FileShare);
            return info.PrimaryDataPath;
        }

        public DataClientInfo OpenDataClientBySecret(string dataClientId, int jobId, string jobSecret)
        {
            TraceHelper.TraceEvent(TraceEventType.Information, "[DataService] .OpenDataClientBySecret Entered. dataClientId: {0}, jobId: {1}", dataClientId, jobId);

            string jobUserName = this.GetJobUserName(jobId, jobSecret);
            using (WindowsIdentity identity = new WindowsIdentity(SamAccountNameToUserPrincipalName(jobUserName)))
            {
                DataClientInfo info = this.OpenDataClientInternal(identity, dataClientId, DataLocation.AzureBlob);
                return info;
            }
        }

        public DataClientInfo OpenDataClientV4(string dataClientId, DataLocation location)
        {
            WindowsIdentity callerIdentity = ServiceSecurityContext.Current.WindowsIdentity;
            CheckUserAccess(callerIdentity);
            DataClientInfo info = this.OpenDataClientInternal(callerIdentity, dataClientId, location);
            return info;
        }
        /// <summary>
        /// Delete a data client with the specified data client id
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        public void DeleteDataClient(string dataClientId)
        {
            WindowsIdentity callerIdentity = ServiceSecurityContext.Current.WindowsIdentity;
            CheckUserAccess(callerIdentity);

            this.DeleteDataClientInternal(callerIdentity, dataClientId, DataLocation.FileShareAndAzureBlob);
        }

        /// <summary>
        /// Associate lifecycle of a DataClient with lifecycle of a session
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="sessionId">session id</param>
        public void AssociateDataClientWithSession(string dataClientId, int sessionId)
        {
            WindowsIdentity callerIdentity = ServiceSecurityContext.Current.WindowsIdentity;
            CheckUserAccess(callerIdentity);

            this.AssociateDataClientWithSessionInternal(callerIdentity, dataClientId, sessionId, DataLocation.FileShareAndAzureBlob);
        }

        /// <summary>
        /// Mark a DataClient as write done
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        public void WriteDone(string dataClientId)
        {
            WindowsIdentity callerIdentity = ServiceSecurityContext.Current.WindowsIdentity;
            CheckUserAccess(callerIdentity);
            this.WriteDoneInternal(callerIdentity, dataClientId, DataLocation.FileShare);
        }

        /// <summary>
        /// Reload data server info
        /// </summary>
        internal void ReloadDataServerInfo()
        {
            this.dataManagement.LoadDataServerInfo();

            // restart data request listener
            this.requestListener.Stop();
            this.requestListener.Start();
        }

        /// <summary>
        /// Get data server info
        /// </summary>
        /// <returns>data server info</returns>
        internal DataServerInfo GetDataServerInfo()
        {
            return this.dataManagement.DataServer;
        }

        /// <summary>
        /// Get the root directory of soa user job data
        /// </summary>
        /// <returns>root directory of soa user job data</returns>
        internal string GetUserJobDataRoot()
        {
            return this.dataManagement.UserJobDataRoot;
        }

        /// <summary>
        /// Get job user name
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <param name="jobSecret">job secret</param>
        /// <returns>job user name</returns>
        internal string GetJobUserName(int jobId, string jobSecret)
        {
            try
            {
                Tuple<string, string> jobUserNameAndSecret;
                lock (this.lockJobUserNameCache)
                {
                    if (!jobUserNameCache.TryGetValue(jobId, out jobUserNameAndSecret))
                    {
                        // cache miss, get job user name and secret from scheduler
                        ISchedulerJob job = this.scheduler.OpenJob(jobId);
                        Dictionary<string, string> dic = JobHelper.GetEnvironmentVariables(job);

                        string secret = string.Empty;
                        if (!dic.TryGetValue(Microsoft.Hpc.Scheduler.Session.Internal.Constant.JobSecretEnvVar, out secret))
                        {
                            TraceHelper.TraceEvent(TraceEventType.Warning, "[DataService] .GetJobUserName: job secret not found. Job id={0}", jobId);
                        }

                        jobUserNameAndSecret = new Tuple<string, string>(job.UserName, secret);
                        jobUserNameCache.Add(jobId, jobUserNameAndSecret);
                    }
                }

                if (!string.Equals(jobUserNameAndSecret.Item2, jobSecret, StringComparison.OrdinalIgnoreCase))
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .GetJobUserName: secret doesn't match. Job id={0}", jobId);
                }
                else
                {
                    TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataService] .GetJobUserName: job id={0}, user name={1}", jobId, jobUserNameAndSecret.Item1);
                    return jobUserNameAndSecret.Item1;
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .GetJobUserName: failed to open job {0}.  Exception={1}", jobId, ex);
            }

            throw new SecurityException("Authentication failed");
        }

        /// <summary>
        /// Create a DataClient with the specified data client id on behalf of user
        /// </summary>
        /// <param name="userIdentity">identity of user who initiates the call</param>
        /// <param name="dataClientId">id that uniquely identifies a data client</param>
        /// <param name="allowedUsers">privileged users of the data client</param>
        /// <param name="location">data client location</param>
        /// <returns>information for further the data client</returns>
        internal DataClientInfo CreateDataClientInternal(WindowsIdentity userIdentity, string dataClientId, string[] allowedUsers, DataLocation location)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataService] .CreateDataClient: id = {0}", dataClientId);
            Utility.ValidateDataClientId(dataClientId);

            try
            {
                IDataProvider provider = this.GetDataProvider(location);
                DataClientInfo info;

                bool exclusiveFlag = true;
                RefCountedReaderWriterLock dataClientLock = this.AcquireDataClientLock(dataClientId, exclusiveFlag);
                try
                {

                    info = provider.CreateDataContainer(dataClientId);
                    this.CreateSoaUserShareFolderIfNotExists(userIdentity.Name);
                    try
                    {
                        provider.SetDataContainerPermissions(dataClientId, userIdentity.Name, allowedUsers);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            provider.DeleteDataContainer(dataClientId);
                        }
                        catch (Exception ex)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Warning, "[DataService].CreateDataClientInternal: Exception {0}", ex);
                        }

                        throw;
                    }
                }
                finally
                {
                    this.ReleaseDataClientLock(dataClientLock, exclusiveFlag, dataClientId);
                }

                if (location == DataLocation.AzureBlob)
                {
                    // Clear the second path
                    info.SecondaryDataPath = null;
                }

                return info;
            }
            catch (DataException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .CreateDataClient: id = {0} receives exception = {1}", dataClientId, e);
                string errorMessage = (e.InnerException == null) ? e.Message : e.InnerException.Message;
                throw new FaultException<DataFault>(new DataFault(e.ErrorCode, errorMessage, dataClientId, e.DataServer), errorMessage);
            }
            catch (UnauthorizedAccessException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .CreateDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (SecurityException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .CreateDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (IdentityNotMappedException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .CreateDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.InvalidAllowedUser, e.Message, e.UnmappedIdentities[0].Value), e.Message);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .CreateDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.Unknown, e.Message, dataClientId, string.Empty), e.Message);
            }
        }

        /// <summary>
        /// Open a a DataClient with the specified data client id on behalf of user
        /// </summary>
        /// <param name="userIdentity">identity of user who initiates the call</param>
        /// <param name="dataClientId">data client id</param>
        /// <param name="location">data client location</param>
        /// <returns>information for accessing the data client</returns>
        internal DataClientInfo OpenDataClientInternal(WindowsIdentity userIdentity, string dataClientId, DataLocation location)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataService] .OpenDataClient: id = {0}", dataClientId);

            Utility.ValidateDataClientId(dataClientId);

            try
            {
                IDataProvider provider = this.GetDataProvider(location);
                provider.CheckDataContainerPermissions(dataClientId, userIdentity, DataPermissions.Read);

                bool exclusiveFlag = true;
                RefCountedReaderWriterLock dataClientLock = this.AcquireDataClientLock(dataClientId, exclusiveFlag);
                try
                {
                    return provider.OpenDataContainer(dataClientId);
                }
                finally
                {
                    this.ReleaseDataClientLock(dataClientLock, exclusiveFlag, dataClientId);
                }
            }
            catch (DataException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .OpenDataClient: id = {0} receives exception = {1}", dataClientId, e);
                string errorMessage = (e.InnerException == null) ? e.Message : e.InnerException.Message;
                throw new FaultException<DataFault>(new DataFault(e.ErrorCode, errorMessage, dataClientId, e.DataServer), errorMessage);
            }
            catch (UnauthorizedAccessException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .OpenDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (SecurityException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .OpenDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .OpenDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.Unknown, e.Message, dataClientId, string.Empty), e.Message);
            }
        }

        /// <summary>
        /// Delete a data client with the specified data client id on behalf of user
        /// </summary>
        /// <param name="userIdentity">identity of user who initiates the call</param>
        /// <param name="dataClientId">data client id</param>
        /// <param name="location">data client location</param>
        internal void DeleteDataClientInternal(WindowsIdentity userIdentity, string dataClientId, DataLocation location)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataService] .DeleteDataClient: id = {0}", dataClientId);

            Utility.ValidateDataClientId(dataClientId);

            try
            {
                IDataProvider provider = this.GetDataProvider(location);
                provider.CheckDataContainerPermissions(dataClientId, userIdentity, DataPermissions.Delete);

                bool exclusiveFlag = true;
                RefCountedReaderWriterLock dataClientLock = this.AcquireDataClientLock(dataClientId, exclusiveFlag);
                try
                {
                    provider.DeleteDataContainer(dataClientId);
                }
                finally
                {
                    this.ReleaseDataClientLock(dataClientLock, exclusiveFlag, dataClientId);
                }

                this.dataManagement.RemoveDataClient(dataClientId);
            }
            catch (DataException e)
            {
                if (e.ErrorCode != DataErrorCode.DataClientNotFound)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .DeleteDataClient: id = {0} receives exception = {1}", dataClientId, e);
                    string errorMessage = (e.InnerException == null) ? e.Message : e.InnerException.Message;
                    throw new FaultException<DataFault>(new DataFault(e.ErrorCode, errorMessage, dataClientId, e.DataServer), errorMessage);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .DeleteDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (SecurityException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .DeleteDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .DeleteDataClient: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.Unknown, e.Message, dataClientId, string.Empty), e.Message);
            }
        }

        /// <summary>
        /// Associate lifecycle of a DataClient with lifecycle of a session on behalf of user
        /// </summary>
        /// <param name="userIdentity">identity of user who initiates the call</param>
        /// <param name="dataClientId">data client id</param>
        /// <param name="sessionId">session id</param>
        /// <param name="location">data client location</param>
        internal void AssociateDataClientWithSessionInternal(WindowsIdentity userIdentity, string dataClientId, int sessionId, DataLocation location)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataService] .AssociateDataClientWithSession: id = {0}, sessionId = {1}", dataClientId, sessionId);

            Utility.ValidateDataClientId(dataClientId);
            Utility.ValidateSessionId(sessionId);

            bool errorFlag = true;
            try
            {
                IDataProvider provider = this.GetDataProvider(location);
                provider.CheckDataContainerPermissions(dataClientId, userIdentity, DataPermissions.SetAttribute);

                bool exclusiveFlag = true;
                RefCountedReaderWriterLock dataClientLock = this.AcquireDataClientLock(dataClientId, exclusiveFlag);
                try
                {

                    Dictionary<string, string> containerAttributes = new Dictionary<string, string>();
                    containerAttributes.Add(Constant.DataAttributeSessionId, sessionId.ToString());
                    provider.SetDataContainerAttributes(dataClientId, containerAttributes);
                }
                finally
                {
                    this.ReleaseDataClientLock(dataClientLock, exclusiveFlag, dataClientId);
                }

                this.dataManagement.AssociateDataClientWithSession(dataClientId, sessionId);
                errorFlag = false;
            }
            catch (DataException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .AssociateDataClientWithSession: id = {0}, sessionId = {1} receives exception = {2}", dataClientId, sessionId, e);
                string errorMessage = (e.InnerException == null) ? e.Message : e.InnerException.Message;
                if (e.ErrorCode == DataErrorCode.DataClientNotFound)
                {
                    throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataClientDeleted, errorMessage, dataClientId, e.DataServer), errorMessage);
                }
                else
                {
                    throw new FaultException<DataFault>(new DataFault(e.ErrorCode, errorMessage, dataClientId, e.DataServer), errorMessage);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .AssociateDataClientWithSession: id = {0}, sessionId = {1} receives exception = {2}", dataClientId, sessionId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (SecurityException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .AssociateDataClientWithSession: id = {0}, sessionId = {1} receives exception = {2}", dataClientId, sessionId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .AssociateDataClientWithSession: id = {0}, sessionId = {1} receives exception = {2}", dataClientId, sessionId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.Unknown, e.Message, dataClientId, string.Empty), e.Message);
            }
            finally
            {
                if (errorFlag)
                {
                    // if failed to store lifecycle info with data container, remove it from lifecycle store
                    this.dataManagement.RemoveDataClient(dataClientId);
                }
            }
        }

        /// <summary>
        /// Mark a DataClient as write done on behalf of user
        /// </summary>
        /// <param name="userIdentity">identity of user who initiates the call</param>
        /// <param name="dataClientId">data client id</param>
        /// <param name="location">data client location</param>
        internal void WriteDoneInternal(WindowsIdentity userIdentity, string dataClientId, DataLocation location)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataService] .WriteDone: id = {0}", dataClientId);

            Utility.ValidateDataClientId(dataClientId);

            try
            {
                IDataProvider provider = this.GetDataProvider(location);
                provider.CheckDataContainerPermissions(dataClientId, userIdentity, DataPermissions.Write);

                bool exclusiveFlag = true;
                RefCountedReaderWriterLock dataClientLock = this.AcquireDataClientLock(dataClientId, exclusiveFlag);
                try
                {
                    Dictionary<string, string> containerAttributes = new Dictionary<string, string>();
                    containerAttributes.Add(Constant.DataAttributeWriteDone, bool.TrueString);
                    provider.SetDataContainerAttributes(dataClientId, containerAttributes);
                }
                finally
                {
                    this.ReleaseDataClientLock(dataClientLock, exclusiveFlag, dataClientId);
                }
            }
            catch (DataException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .WriteDone: id = {0} receives exception = {1}", dataClientId, e);
                string errorMessage = (e.InnerException == null) ? e.Message : e.InnerException.Message;
                if (e.ErrorCode == DataErrorCode.DataClientNotFound)
                {
                    throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataClientDeleted, errorMessage, dataClientId, e.DataServer), errorMessage);
                }
                else
                {
                    throw new FaultException<DataFault>(new DataFault(e.ErrorCode, errorMessage, dataClientId, e.DataServer), errorMessage);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .WriteDone: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (SecurityException e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .WriteDone: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, e.Message, dataClientId, string.Empty), e.Message);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] .WriteDone: id = {0} receives exception = {1}", dataClientId, e);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.Unknown, e.Message, dataClientId, string.Empty), e.Message);
            }
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if HPCPACK
                this.clusterInfo.OnAzureStorageConnectionStringOrClusterIdUpdated -= ClusterInfo_OnAzureStorageConnectionStringOrClusterIdUpdated;
#endif

                if (this.dataManagement != null)
                {
                    this.dataManagement.Dispose();
                    this.dataManagement = null;
                }

                if (this.requestListener != null)
                {
                    this.requestListener.Stop();
                    this.requestListener = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Check user access
        /// </summary>
        /// <param name="userIdentity">identity of user who initiates the call</param>
        private static void CheckUserAccess(WindowsIdentity userIdentity)
        {
            bool isAdminOrUser = false;
            try
            {
                isAdminOrUser = AuthenticationUtil.IsHpcAdminOrUser(userIdentity);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] . CheckUserAccess encountered exception. user={0}, exception={1}.", userIdentity.Name, ex);
            }

            if (!isAdminOrUser)
            {
                string errorMessage = string.Format(SR.DataService_UnauthorizedUser, userIdentity.Name);
                throw new FaultException<DataFault>(new DataFault(DataErrorCode.DataNoPermission, errorMessage), errorMessage);
            }
        }

        /// <summary>
        /// Get current DataProvider
        /// </summary>
        /// <param name="location">data client location</param>
        /// <returns>return current DataProvider</returns>
        private IDataProvider GetDataProvider(DataLocation location)
        {
            DataServerInfo dataServerInfo = this.dataManagement.DataServer;
            if (dataServerInfo == null)
            {
                throw new DataException(DataErrorCode.NoDataServerConfigured, string.Empty);
            }

            return DataProviderHelper.GetDataProvider(location, dataServerInfo);
        }

        /// <summary>
        /// Acquire lock for the specified data client
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="exclusive">if this lock is exclusive or not</param>
        /// <returns>lock object for the specified data client</returns>
        private RefCountedReaderWriterLock AcquireDataClientLock(string dataClientId, bool exclusive)
        {
            RefCountedReaderWriterLock dataClientLock;
            lock (this.lockDataClientLocks)
            {
                if (!this.dataClientLocks.TryGetValue(dataClientId, out dataClientLock))
                {
                    dataClientLock = new RefCountedReaderWriterLock();
                    this.dataClientLocks.Add(dataClientId, dataClientLock);
                }

                dataClientLock.IncRef();
            }

            if (!dataClientLock.TryLock(exclusive))
            {
                // if failed to acquire the lock
                int refCount = dataClientLock.DecRef();
                if (refCount == 0)
                {
                    bool removeFlag = false;
                    lock (this.lockDataClientLocks)
                    {
                        if (0 == dataClientLock.RefCount)
                        {
                            this.dataClientLocks.Remove(dataClientId);
                            removeFlag = true;
                        }
                    }

                    if (removeFlag)
                    {
                        dataClientLock.Close();
                    }
                }

                throw new DataException(DataErrorCode.DataClientBusy, string.Empty);
            }

            return dataClientLock;
        }

        /// <summary>
        /// Release a data client lock, and close it if it is used by no body.
        /// </summary>
        /// <param name="dataClientLock">data client lock</param>
        /// <param name="exclusive">if unlocking an exclusive lock or not</param>
        /// <param name="dataClientId">data client id</param>
        private void ReleaseDataClientLock(RefCountedReaderWriterLock dataClientLock, bool exclusive, string dataClientId)
        {
            Debug.Assert(dataClientLock != null, "dataClientLock");
            dataClientLock.Unlock(exclusive);

            int refCount = dataClientLock.DecRef();
            if (refCount == 0)
            {
                bool removeFlag = false;
                lock (this.lockDataClientLocks)
                {
                    // this data client lock is hold by nobody, just remove it.
                    if (0 == dataClientLock.RefCount)
                    {
                        this.dataClientLocks.Remove(dataClientId);
                        removeFlag = true;
                    }
                }

                if (removeFlag)
                {
                    dataClientLock.Close();
                }
            }
        }

        private string CreateSoaUserShareFolderIfNotExists(string userName)
        {
            string soaShareRoot = this.GetDataServerInfo().AddressInfo;
            string userDir = userName.Replace('\\', '.');
            userDir = Path.Combine(this.GetUserJobDataRoot(), userDir);
            if (!Directory.Exists(userDir))
            {
                try
                {
                    IEnumerable<string> hnAccounts = HpcContext.Get().GetNodesAsync().GetAwaiter().GetResult().Select(h => string.Format(@"{0}\{1}$", Environment.UserDomainName, h));
                    List<string> accounts = new List<string>(hnAccounts);
                    accounts.Add(userName);
                    accounts.Add(localSystemAccount);
                    accounts.Add(administratorsAccount);
                    // prepare ACL setting for the directory
                    // grant the owner, "system", "administrators" and head nodes account (for remote file share) full control access. This rule applies to this folder, subfolders, and files.
                    DirectorySecurity ds = new DirectorySecurity();
                    ds.SetAccessRuleProtection(true, false);
                    foreach (var acc in accounts)
                    {
                        ds.AddAccessRule(new FileSystemAccessRule(acc, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    }

                    // create the UserJobs dir for soa user jobs and set ACL
                    Directory.CreateDirectory(userDir);
                    Directory.SetAccessControl(userDir, ds);
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "[DataService] CreateSoaUserShareFolderIfNotExists: Failed to create folder {0}: {1}", userDir, e);
                    throw;
                }
            }

            return userDir;
        }

        /// <summary>
        /// Ref counted reader write lock
        /// </summary>
        private class RefCountedReaderWriterLock : DisposableObject
        {
            /// <summary>
            /// Reader writer lock
            /// </summary>
            private ReaderWriterLockSlim innerLock = new ReaderWriterLockSlim();

            /// <summary>
            /// Reference count
            /// </summary>
            private int refCount;

            /// <summary>
            /// Gets reference count
            /// </summary>
            public int RefCount
            {
                get
                {
                    return this.refCount;
                }
            }

            /// <summary>
            /// Increase reference count
            /// </summary>
            /// <returns>new reference count</returns>
            public int IncRef()
            {
                return Interlocked.Increment(ref this.refCount);
            }

            /// <summary>
            /// Decrease reference count
            /// </summary>
            /// <returns>new reference count</returns>
            public int DecRef()
            {
                return Interlocked.Decrement(ref this.refCount);
            }

            /// <summary>
            /// Try to enter lock
            /// </summary>
            /// <param name="exclusive">if enter the lock exclusively or not</param>
            /// <returns>true if aquire the lock successfully, false otherwise</returns>
            public bool TryLock(bool exclusive)
            {
                Debug.Assert(this.refCount > 0, "refCount");

                if (exclusive)
                {
                    return this.innerLock.TryEnterWriteLock(TimeSpan.FromMinutes(Constant.DataOperationTimeoutInMinutes));
                }
                else
                {
                    return this.innerLock.TryEnterReadLock(TimeSpan.FromMinutes(Constant.DataOperationTimeoutInMinutes));
                }
            }

            /// <summary>
            /// Exit the lock
            /// </summary>
            /// <param name="exclusive">if exiting an exclusive lock</param>
            public void Unlock(bool exclusive)
            {
                Debug.Assert(this.refCount > 0, "refCount");

                if (exclusive)
                {
                    this.innerLock.ExitWriteLock();
                }
                else
                {
                    this.innerLock.ExitReadLock();
                }
            }

            /// <summary>
            /// Dispose the instance
            /// </summary>
            /// <param name="disposing">indicating whether it is called directly or indirectly by user's code</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.innerLock.Dispose();
                }
            }
        }

        /// <summary>
        /// A random replacement cache (not thread safe)
        /// </summary>
        private class RRCache<TKey, TValue>
        {
            /// <summary>
            /// Default capacity: 1024
            /// </summary>
            private const int DefaultCapacity = 1024;

            /// <summary>
            /// Cache capacity
            /// </summary>
            private int capacity;

            /// <summary>
            /// Cache entries
            /// </summary>
            private Dictionary<TKey, TValue> entries = new Dictionary<TKey, TValue>();

            /// <summary>
            /// Initializes a new instance of the RRCache class
            /// </summary>
            public RRCache()
                : this(DefaultCapacity)
            {
            }

            /// <summary>
            /// Initializes a new instance of the RRCache class
            /// </summary>
            /// <param name="capacity">cache capacity</param>
            public RRCache(int capacity)
            {
                this.capacity = capacity;
            }

            /// <summary>
            ///  Adds the specified key and value to the cache.
            /// </summary>
            /// <param name="key">The key of the entry to add</param>
            /// <param name="value">The value of the entry to add</param>
            public void Add(TKey key, TValue value)
            {
                if (this.entries.Count >= this.capacity)
                {
                    List<TKey> keys = new List<TKey>(this.entries.Keys);
                    int index = new Random().Next(0, this.entries.Count);
                    this.entries.Remove(keys[index]);
                }

                this.entries.Add(key, value);
            }

            /// <summary>
            /// Gets the value associated with the specified key.
            /// </summary>
            /// <param name="key">The key of the value to get</param>
            /// <param name="value">The value associated with the specified key</param>
            /// <returns>true if cache contains an entry with the specified key, false otherwise</returns>
            public bool TryGetValue(TKey key, out TValue value)
            {
                return this.entries.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Convert a sam account name to UPN
        /// </summary>
        /// <param name="userName">sam account name</param>
        /// <returns>corresponding user principal name</returns>
        internal static string SamAccountNameToUserPrincipalName(string userName)
        {
            if (!userName.Contains(SamAccountNameDelimeter))
            {
                return userName;
            }

            string domain, user;
            try
            {
                string[] strs = userName.Split(new string[] { SamAccountNameDelimeter }, StringSplitOptions.None);
                domain = strs[0];
                user = strs[1];
                return string.Format(UpnFormat, user, domain);
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Warning, "[DataRequestListener].SamAccountNameToUserPrincipalName: Exception {0}", ex);

                throw new ArgumentException("user name");
            }
        }
    }
}
