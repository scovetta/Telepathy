//------------------------------------------------------------------------------
// <copyright file="DataClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      DataClient interface
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session.Data.DataContainer;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    /// <summary>
    /// A class that provides interfaces to manage(create/open/delete) and access(write/read)
    /// DataClient on the data server.
    /// </summary>
    public class DataClient : IDisposable
    {
        #region private fields
        /// <summary>
        /// GZip compression flag.
        /// NOTE: this 32-char magic string is obtained from a GUID. It is used to identify compressed data.
        /// </summary>
        private const string GZipCompressionFlag = "E7FE6B887495BE4F93CD243D8C1E06AC";

        /// <summary>
        /// Original data length header length
        /// </summary>
        private const int OriginalDataLengthHeaderLength = 4;

        /// <summary>
        /// Compression data header length
        /// </summary>
        private const int CompressionHeaderLength = 36;

        /// <summary>
        /// Head node name
        /// </summary>
        private string headNode;

        /// <summary>
        /// DataClient id
        /// </summary>
        private string id;

        /// <summary>
        /// Data container for this DataClient instance
        /// </summary>
        private IDataContainer dataContainer;

        /// <summary>
        /// flag indicating whether this DataClient instance is read-only
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// flag indicating whether WriteXXXAll has been called on this DataClient instance.
        /// </summary>
        private bool isFlushed;

        /// <summary>
        /// flag indicating whether SetDataLifeCycle has been called on this DataClient instance.
        /// </summary>
        private bool hasLifeCycle;

        /// <summary>
        /// flag indicating whether this DataClient instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// the user name used to connect to data service
        /// </summary>
        private string userName;

        /// <summary>
        /// the password used to connect to data service
        /// </summary>
        private string password;

        /// <summary>
        /// the transport scheme for data client
        /// </summary>
        private TransportScheme scheme = TransportScheme.NetTcp;

        #endregion

        /// <summary>
        /// Initializes a new instance of the DataClient class
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="dataStorePath">data store path</param>
        /// <param name="readOnly">a flag indicating whether the DataClient instance is readonly or not</param>
        internal DataClient(string dataClientId, string dataStorePath, bool readOnly)
        {
            this.id = dataClientId;
            this.isReadOnly = readOnly;
            this.dataContainer = DataContainerHelper.GetDataContainer(dataStorePath);
        }

        /// <summary>
        /// Initializes a new instance of the DataClient class
        /// </summary>
        /// <param name="dataClientId">data client id</param>
        /// <param name="info">data client info</param>
        /// <param name="readOnly">a flag indicating whether the DataClient instance is readonly or not</param>
        internal DataClient(string dataClientId, DataClientInfo info, bool readOnly)
        {
            this.id = dataClientId;
            this.isReadOnly = readOnly;
            if (string.IsNullOrEmpty(info.SecondaryDataPath))
            {
                this.dataContainer = DataContainerHelper.GetDataContainer(info.PrimaryDataPath);
            }
            else
            {
                this.dataContainer = DataContainerHelper.GetDataContainer(info.PrimaryDataPath, info.SecondaryDataPath);
            }
        }

        /// <summary>
        /// Initializes a new instance of the DataClient class
        /// </summary>
        /// <param name="headNode">head node name</param>
        /// <param name="dataClientId">data client id</param>
        /// <param name="dataStorePath">data store path</param>
        /// <param name="readOnly">a flag indicating whether the DataClient instance is readonly or not</param>
        /// <param name="userName">user name</param>
        /// <param name="password">the password name</param>
        private DataClient(string headNode, string dataClientId, string dataStorePath, bool readOnly, TransportScheme scheme, string userName = null, string password = null)
            : this(dataClientId, dataStorePath, readOnly)
        {
            this.headNode = headNode;
            this.userName = userName;
            this.password = password;
            this.scheme = scheme;
        }

        /// <summary>
        /// Initializes a new instance of the DataClient class
        /// </summary>
        /// <param name="headNode">head node name</param>
        /// <param name="dataClientId">data client id</param>
        /// <param name="info">data client info</param>
        /// <param name="readOnly">a flag indicating whether the DataClient instance is readonly or not</param>
        /// <param name="userName">user name</param>
        /// <param name="password">the password name</param>
        private DataClient(string headNode, string dataClientId, DataClientInfo info, bool readOnly, TransportScheme scheme, string userName = null, string password = null)
            : this(dataClientId, info, readOnly)
        {
            this.headNode = headNode;
            this.userName = userName;
            this.password = password;
            this.scheme = scheme;
        }

        /// <summary>
        /// Gets DataClient Id
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Create a new instance of the DataClient class with the specified ID
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        /// <param name="dataClientId">string identifier of the DataClient</param>
        /// <returns>a new DataClient instance that provides read/write access to the DataClient</returns>
        public static DataClient Create(string headNode, string dataClientId)
        {
            Utility.ValidateHeadNode(headNode);
            Utility.ValidateDataClientId(dataClientId);

            return CreateInternal(headNode, dataClientId, null, TransportScheme.NetTcp, DataLocation.FileShare, null, null);
        }

        /// <summary>
        /// Create a new instance of the DataClient class with the specified ID and data security
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        /// <param name="dataClientId">string identifier of the DataClient</param>
        /// <param name="allowedUsers">users that are granted access the data</param>
        /// <returns>a new DataClient instance that provides read/write access to the DataClient</returns>
        public static DataClient Create(string headNode, string dataClientId, string[] allowedUsers)
        {
            Utility.ValidateHeadNode(headNode);
            Utility.ValidateDataClientId(dataClientId);

            return CreateInternal(headNode, dataClientId, allowedUsers, TransportScheme.NetTcp, DataLocation.FileShare, null, null);
        }

        /// <summary>
        /// Create a new instance of the DataClient class with the specified ID and data sercurity
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        /// <param name="dataClientId">string identifier of the DataClient</param>
        /// <param name="allowedUsers">users that are granted access the data</param>
        /// <param name="likelyAccessInAzure">a flag indicating whether this data client is likely to be accessed from within Azure</param>
        /// <returns>a new DataClient instance that provides read/write access to the DataClient</returns>
        public static DataClient Create(string headNode, string dataClientId, string[] allowedUsers, bool likelyAccessInAzure)
        {
            Utility.ValidateHeadNode(headNode);
            Utility.ValidateDataClientId(dataClientId);
            DataLocation location = likelyAccessInAzure ? DataLocation.FileShareAndAzureBlob : DataLocation.FileShare;
            return CreateInternal(headNode, dataClientId, allowedUsers, TransportScheme.NetTcp, location, null, null);
        }

        /// <summary>
        /// Create a DataClient instance with the specified data client id
        /// </summary>
        /// <param name="startInfo">the session start info</param>
        /// <param name="dataClientId">data client id</param>
        /// <param name="likelyAccessInAzure">a flag indicating whether this data client is likely to be accessed from within Azure</param>
        /// <returns>a new DataClient instance</returns>
        public static DataClient Create(SessionStartInfo startInfo, string dataClientId, bool likelyAccessInAzure)
        {
            Utility.ValidateHeadNode(startInfo.Headnode);
            Utility.ValidateDataClientId(dataClientId);
            DataLocation location = DataLocation.FileShare;
            if (Microsoft.Hpc.Scheduler.Session.Internal.SoaHelper.IsSchedulerOnIaaS(startInfo.Headnode))
            {
                location = DataLocation.AzureBlob;
            }
            else if (likelyAccessInAzure)
            {
                location = DataLocation.FileShareAndAzureBlob;
            }

            return CreateInternal(startInfo.Headnode, dataClientId, null, startInfo.TransportScheme, location, startInfo.Username, startInfo.InternalPassword);
        }

        /// <summary>
        /// Open an existing DataClient with the specified ID
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        /// <param name="dataClientId">ID of the DataClient to be opened</param>
        /// <returns>DataClient instance that provides read/write access to the specified DataClient</returns>
        public static DataClient Open(string headNode, string dataClientId)
        {
            return Open(headNode, dataClientId, DataLocation.FileShare);
        }

        /// <summary>
        /// Open an existing DataClient with the specified ID
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        /// <param name="dataClientId">ID of the DataClient to be opened</param>
        /// <param name="userName">user name</param>
        /// <param name="password">the password name</param>
        /// <returns>DataClient instance that provides read/write access to the specified DataClient</returns>
        public static DataClient Open(string headNode, string dataClientId, DataLocation location)
        {
            Utility.ValidateHeadNode(headNode);
            Utility.ValidateDataClientId(dataClientId);

            DataClientInfo info = OpenInternal(headNode, dataClientId, location, TransportScheme.NetTcp, null, null);
            return new DataClient(headNode, dataClientId, info.PrimaryDataPath, /*readOnly=*/true, TransportScheme.NetTcp, null, null);
        }

        /// <summary>
        /// Open an existing DataClient with the specified ID
        /// </summary>
        /// <param name="startInfo">the session start info</param>
        /// <param name="dataClientId">data client id</param>
        /// <returns>DataClient instance that provides read/write access to the specified DataClient</returns>
        public static DataClient Open(SessionStartInfo startInfo, string dataClientId)
        {
            Utility.ValidateHeadNode(startInfo.Headnode);
            Utility.ValidateDataClientId(dataClientId);
            DataLocation location = DataLocation.FileShare;
            if (Microsoft.Hpc.Scheduler.Session.Internal.SoaHelper.IsSchedulerOnIaaS(startInfo.Headnode))
            {
                location = DataLocation.AzureBlob;
            }

            DataClientInfo info = OpenInternal(startInfo.Headnode, dataClientId, location, startInfo.TransportScheme, startInfo.Username, startInfo.InternalPassword);
            return new DataClient(startInfo.Headnode, dataClientId, info.PrimaryDataPath, /*readOnly=*/true, startInfo.TransportScheme, startInfo.Username, startInfo.InternalPassword);
        }

        /// <summary>
        /// Open an existing DataClient with the specified ID, job ID and job secret
        /// </summary>
        /// <param name="hpcContext">The <see cref="IHpcContext"/> instance.</param>
        /// <param name="dataClientId">ID of the DataClient to be opened</param>
        /// <param name="jobId">Job ID</param>
        /// <param name="jobSecret">Job Secret</param>
        /// <returns>DataClient instance that provides read/write access to the specified DataClient</returns>
        internal static DataClient Open(string dataClientId, int jobId, string jobSecret)
        {
            var context = HpcContext.GetOrAdd(EndpointsConnectionString.LoadFromEnvVarsOrWindowsRegistry(), CancellationToken.None, true);

            Utility.ValidateDataClientId(dataClientId);
            DataClientInfo info = OpenBySecretInternal(context, dataClientId, jobId, jobSecret);
            return new DataClient(context.ResolveSessionLauncherNodeAsync().GetAwaiter().GetResult(), dataClientId, info.PrimaryDataPath, true, TransportScheme.NetTcp, null, null);
        }

        /// <summary>
        /// Delete the DataClient with the specified ID
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        /// <param name="dataClientId">ID of the DataClient to be deleted</param>
        public static void Delete(string headNode, string dataClientId)
        {
            DeleteInternal(headNode, dataClientId, TransportScheme.NetTcp, null, null);
        }

        /// <summary>
        /// Delete the DataClient with the specified ID
        /// </summary>
        /// <param name="startInfo">the session start info</param>
        /// <param name="dataClientId">data client id</param>
        public static void Delete(SessionStartInfo startInfo, string dataClientId)
        {
            DeleteInternal(startInfo.Headnode, dataClientId, startInfo.TransportScheme, startInfo.Username, startInfo.InternalPassword);
        }

        /// <summary>
        /// Associate DataClient with specific DataLifeCycle object
        /// </summary>
        /// <param name="lifeCycle">DataLifeCycle object</param>
        public void SetDataLifeCycle(DataLifeCycle lifeCycle)
        {
            Microsoft.Hpc.Scheduler.Session.Internal.ParamCheckUtility.ThrowIfNull(lifeCycle, "lifeCycle");
            this.CheckIfDisposed();
            this.CheckIfReadOnly();

            if (this.hasLifeCycle)
            {
                throw new DataException(DataErrorCode.DataClientLifeCycleSet, null, this.Id);
            }

            DataLifeCycleContext context = lifeCycle.Internal.Context;
            SessionBasedDataLifeCycleContext sessionContext = context as SessionBasedDataLifeCycleContext;

            IDataService dataAgent = GetDataServiceAgent(this.headNode, this.scheme, this.userName, this.password);
            InvokeDataOperation<bool>(
                dataAgent,
                delegate(IDataService agent)
                {
                    agent.AssociateDataClientWithSession(this.Id, sessionContext.SessionId);
                    return true;
                });

            this.hasLifeCycle = true;
        }

        /// <summary>
        /// Write data to the DataClient and mark the DataClient as ready for read
        /// </summary>
        /// <typeparam name="T">type of the data object</typeparam>
        /// <param name="data">data object to be written</param>
        public void WriteAll<T>(T data)
        {
            this.WriteAll<T>(data, false);
        }

        /// <summary>
        /// Write data to the DataClient and mark the DataClient as ready for read
        /// </summary>
        /// <typeparam name="T">type of the data object</typeparam>
        /// <param name="data">data object to be written</param>
        /// <param name="compressible">a flag indicating if the data is compressible</param>
        public void WriteAll<T>(T data, bool compressible)
        {
            Microsoft.Hpc.Scheduler.Session.Internal.ParamCheckUtility.ThrowIfNull(data, "data");
            this.CheckIfDisposed();
            this.CheckIfReadOnly();

            if (this.isFlushed)
            {
                throw new DataException(DataErrorCode.DataClientNotWritable, null, this.Id);
            }

            try
            {
                this.WriteAllInternal(data, compressible);
            }
            catch (DataException e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClient] .WriteAll: receives exception {0}", e);
                e.DataClientId = this.id;
                throw;
            }

            // All data has been written successfully, mark the data client as "write done"
            this.MarkWriteDone();

            this.isFlushed = true;
        }

        /// <summary>
        /// Write raw bytes to the DataClient and mark the DataClient as ready for read
        /// </summary>
        /// <param name="dataBytes">bytes to write</param>
        public void WriteRawBytesAll(byte[] dataBytes)
        {
            this.WriteRawBytesAll(dataBytes, false);
        }

        /// <summary>
        /// Write raw bytes to the DataClient and mark the DataClient as ready for read
        /// </summary>
        /// <param name="dataBytes">bytes to write</param>
        /// <param name="compressible">a flag indicating if the bytes are compressible</param>
        public void WriteRawBytesAll(byte[] dataBytes, bool compressible)
        {
            Microsoft.Hpc.Scheduler.Session.Internal.ParamCheckUtility.ThrowIfNull(dataBytes, "dataBytes");
            if (dataBytes.Length == 0)
            {
                throw new ArgumentException(SR.ArgumentEmpty, "dataBytes");
            }

            this.CheckIfDisposed();
            this.CheckIfReadOnly();

            if (this.isFlushed)
            {
                throw new DataException(DataErrorCode.DataClientNotWritable, null, this.Id);
            }

            try
            {
                this.WriteRawBytesAllInternal(dataBytes, compressible);
            }
            catch (DataException e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClient] .WriteRawBytesAll: receives exception {0}", e);
                e.DataClientId = this.id;
                throw;
            }

            // All data has been written successfully, mark the data client as "write done"
            this.MarkWriteDone();
            this.isFlushed = true;
        }

        /// <summary>
        /// Read back all data in the DataClient
        /// </summary>
        /// <typeparam name="T">type of the object that data represents</typeparam>
        /// <returns>data in the DataClient</returns>
        public T ReadAll<T>()
        {
            this.CheckIfDisposed();

            object data = this.ReadAllInternal();

            try
            {
                return (T)data;
            }
            catch (InvalidCastException)
            {
                throw new DataException(DataErrorCode.DataTypeMismatch, string.Format(Microsoft.Hpc.Scheduler.Session.SR.DataTypeMismatch, data.GetType().Name, typeof(T).Name));
            }
        }

        /// <summary>
        /// Read back all the data in the DataClient as raw bytes
        /// </summary>
        /// <returns>data in the DataClient as raw bytes</returns>
        public byte[] ReadRawBytesAll()
        {
            this.CheckIfDisposed();

            return this.ReadRawBytesAllInternal();
        }

        /// <summary>
        /// Get the data content md5
        /// </summary>
        /// <returns></returns>
        public string GetContentMd5()
        {
            return this.dataContainer.GetContentMd5();
        }
        /// <summary>
        /// Returns the path to the underlying store of data
        /// </summary>
        /// <returns>path to the underlying store of the data</returns>
        [Obsolete("GetStorePath is obsolete.")] // Obsolete added for V4 RTM.
        public string GetStorePath()
        {
            this.CheckIfDisposed();

            return this.dataContainer.GetStorePath();
        }

        /// <summary>
        /// Close the DataClient
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="dispose">a flag indicating whether release resources</param>
        protected virtual void Dispose(bool dispose)
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
            }
        }

        /// <summary>
        /// The delegate for invoking a remote DataService call
        /// </summary>
        /// <typeparam name="T">operation return type</typeparam>
        /// <returns>operation return value</returns>
        private delegate T DataOperation<T>(IDataService agent);

        /// <summary>
        /// Invoke a remote DataService call
        /// </summary>
        /// <typeparam name="T">operation return type</typeparam>
        /// <param name="headNode">head node name</param>
        /// <param name="operation">operation to be invoked</param>
        /// <returns>operation return value</returns>
        private static T InvokeDataOperation<T>(IDataService agent, DataOperation<T> operation)
        {
            try
            {
                return operation(agent);
            }
            catch (ActionNotSupportedException e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClient] .InvokeDataOperation: receives exception {0}", e);
                throw new DataException(DataErrorCode.DataFeatureNotSupported, SR.DataFeatureNotSupported);
            }
            catch (FaultException<DataFault> e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClient] .InvokeDataOperation: receives exception {0}", e);
                throw Utility.TranslateFaultException(e);
            }
            catch (CommunicationException e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClient] .InvokeDataOperation: receives exception {0}", e);
                throw new DataException(DataErrorCode.ConnectDataServiceFailure, e);
            }
            catch (TimeoutException e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClient] .InvokeDataOperation: receives exception {0}", e);
                throw new DataException(DataErrorCode.ConnectDataServiceTimeout, e);
            }
            finally
            {
                if (agent != null)
                {
                    CloseDataServiceAgent(agent);
                }
            }
        }

        /// <summary>
        /// Create a DataClient instance with the specified data client id
        /// </summary>
        /// <param name="headNode">head node name</param>
        /// <param name="dataClientId">data client id</param>
        /// <param name="allowedUsers">privileged users who has read permission to the data client</param>
        /// <param name="scheme">the transport scheme</param>
        /// <param name="location">the data location</param>
        /// <param name="userName">the user name</param>
        /// <param name="password">the password</param>
        /// <returns>a new DataClient instance</returns>
        private static DataClient CreateInternal(string headNode, string dataClientId, string[] allowedUsers, TransportScheme scheme, DataLocation location, string userName, string password)
        {
            if (allowedUsers != null)
            {
                foreach (string allowedUser in allowedUsers)
                {
                    Microsoft.Hpc.Scheduler.Session.Internal.Utility.ThrowIfNullOrEmpty(allowedUser, "allowed user");
                }
            }

            IDataService dataAgent = GetDataServiceAgent(headNode, scheme, userName, password);

            DataClientInfo info = InvokeDataOperation<DataClientInfo>(
                dataAgent,
                delegate(IDataService agent)
                {
                    return agent.CreateDataClientV4(dataClientId, allowedUsers, location);
                });

            Debug.Assert(!string.IsNullOrEmpty(info.PrimaryDataPath), "primaryDataPath");
            return new DataClient(headNode, dataClientId, info, /*readOnly = */false, scheme, userName, password);
        }

        /// <summary>
        /// Open a DataClient with the specified data client id
        /// </summary>
        /// <param name="headNode">head node name</param>
        /// <param name="dataClientId">data client id</param>
        /// <param name="scheme">the transport scheme</param>
        /// <param name="location">the data location</param>
        /// <param name="userName">the user name</param>
        /// <param name="password">the password</param>
        /// <returns>data store path</returns>
        private static DataClientInfo OpenInternal(string headNode, string dataClientId, DataLocation location, TransportScheme scheme, string userName, string password)
        {
            IDataService dataAgent = GetDataServiceAgent(headNode, scheme, userName, password);

            return InvokeDataOperation<DataClientInfo>(
                dataAgent,
                delegate(IDataService agent)
                {
                    return agent.OpenDataClientV4(dataClientId, location);
                });
        }

        private static DataClientInfo OpenBySecretInternal(IHpcContext hpcContext, string dataClientId, int jobId, string jobSecret)
        {
            return new DataServiceRestClient(hpcContext).OpenDataClientBySecret(dataClientId, jobId, jobSecret).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Delete the DataClient with the specified ID
        /// </summary>
        /// <param name="headNode">cluster head node name</param>
        /// <param name="dataClientId">ID of the DataClient to be deleted</param>
        /// <param name="scheme">the transport scheme</param>
        /// <param name="userName">user name</param>
        /// <param name="password">the password name</param>
        private static void DeleteInternal(string headNode, string dataClientId, TransportScheme scheme, string userName, string password)
        {
            Utility.ValidateHeadNode(headNode);
            Utility.ValidateDataClientId(dataClientId);

            IDataService dsagent = GetDataServiceAgent(headNode, scheme, userName, password);
            InvokeDataOperation<bool>(
                dsagent,
                delegate(IDataService agent)
                {
                    agent.DeleteDataClient(dataClientId);
                    return true;
                });
        }

        /// <summary>
        /// Mark the data written done for the client
        /// </summary>
        private void MarkWriteDone()
        {
            IDataService dataagent = GetDataServiceAgent(this.headNode, this.scheme, this.userName, this.password);
            try
            {
                InvokeDataOperation<bool>(
                   dataagent,
                   delegate(IDataService agent)
                   {
                       agent.WriteDone(this.Id);
                       return true;
                   });
            }
            catch (DataException ex)
            {
                // WriteDone method is added in V4. When a V4 client tries to talk to V3 cluster,
                // it will receive DataFeatureNotSupported error here. For back compatibility,
                // swallow this error. Related bug: 20474
                if (ex.ErrorCode != DataErrorCode.DataFeatureNotSupported)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Get agent for talking to data service or data proxy
        /// </summary>
        /// <param name="headNode">head node name</param>
        /// <param name="scheme">the transport scheme</param>
        /// <param name="userName">user name</param>
        /// <param name="password">the password name</param>
        /// <returns>agent to talk to data service or data proxy </returns>
        private static IDataService GetDataServiceAgent(string headNode, TransportScheme scheme = TransportScheme.NetTcp, string userName = null, string password = null)
        {
            if (Microsoft.Hpc.Scheduler.Session.Internal.SoaHelper.IsOnAzure() && !Microsoft.Hpc.Scheduler.Session.Internal.SoaHelper.IsSchedulerOnAzure())
            {
                // Azure burst scenario. should talk to data proxy
                return new DataProxyAgent();
            }
            else
            {
                DataServiceAgent agent = new DataServiceAgent(headNode, scheme);
                if(!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                {
                    agent.SetCredential(userName, password);
                }

                return agent;
            }
        }

        /// <summary>
        /// Close a DataServiceAgent instance or a DataProxyAgent service
        /// </summary>
        /// <param name="agent">agent instance to be closed</param>
        private static void CloseDataServiceAgent(IDataService agent)
        {
            DataServiceAgent dataServiceAgent = agent as DataServiceAgent;
            if (dataServiceAgent != null)
            {
                Microsoft.Hpc.Scheduler.Session.Internal.Utility.SafeCloseCommunicateObject(dataServiceAgent);
                return;
            }

            DataProxyAgent dataProxyAgent = agent as DataProxyAgent;
            if (dataProxyAgent != null)
            {
                dataProxyAgent.Close();
            }
        }

        /// <summary>
        /// Generate data compression header
        /// </summary>
        /// <param name="orginalDataLength">original data length</param>
        /// <returns>data compression header</returns>
        private static byte[] GenerateCompressionHeader(int orginalDataLength)
        {
            byte[] attributesArray = new byte[GZipCompressionFlag.Length + OriginalDataLengthHeaderLength];
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(GZipCompressionFlag), 0, attributesArray, 0, GZipCompressionFlag.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(orginalDataLength), 0, attributesArray, GZipCompressionFlag.Length, OriginalDataLengthHeaderLength);
            return attributesArray;
        }

        /// <summary>
        /// Check if data bytes are compressed or not
        /// </summary>
        /// <param name="dataBytes">data bytes to be checked</param>
        /// <param name="originalDataLength">original data length</param>
        /// <returns>true if dataBytes is compressed, false otherwise</returns>
        private static bool IsCompressed(byte[] dataBytes, out int originalDataLength)
        {
            originalDataLength = -1;

            // if data length is smaller than compression header size, consider this data as not compressed.
            // TODO: FIXME! or the data is corrupted.
            if (dataBytes.Length < CompressionHeaderLength)
            {
                return false;
            }

            string compressionFlag = Encoding.ASCII.GetString(dataBytes, 0, GZipCompressionFlag.Length);
            if (string.Equals(compressionFlag, GZipCompressionFlag, StringComparison.OrdinalIgnoreCase))
            {
                originalDataLength = BitConverter.ToInt32(dataBytes, GZipCompressionFlag.Length);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Write data object to current DataClient
        /// </summary>
        /// <param name="data">data object to be written</param>
        /// <param name="compressible">a flag indicating whether the data object is compressible or not</param>
        private void WriteAllInternal(object data, bool compressible)
        {
            ObjectDataContent content = new ObjectDataContent(data, compressible);
            this.WriteContent(content);
        }

        /// <summary>
        /// Write bytes to current DataClient
        /// </summary>
        /// <param name="data">data content to be written</param>
        private void WriteContent(DataContent data)
        {
            try
            {
                this.dataContainer.AddDataAndFlush(data);
            }
            catch (DataException e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClient] .WriteContent: receives exception {0}", e);
                e.DataClientId = this.id;
                throw;
            }
        }

        /// <summary>
        /// Read data from current DataClient as an object
        /// </summary>
        /// <returns>data in current DataClient as an object</returns>
        private object ReadAllInternal()
        {
            byte[] dataArray = this.ReadBytes();
            if (dataArray == null || dataArray.Length == 0)
            {
                throw new DataException(DataErrorCode.NoDataAvailable, null, this.Id);
            }

            try
            {
                ObjectDataContent content = ObjectDataContent.Parse(dataArray);
                return content.Object;
            }
            catch (InvalidDataException e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClient] .ReadAllInternal: failed to decompress data");
                throw new DataException(DataErrorCode.DataInconsistent, e, this.Id);
            }
        }

        /// <summary>
        /// Write a byte array to current DataClient
        /// </summary>
        /// <param name="dataBytes">byte array to be written</param>
        /// <param name="compressible">a flag indicating whether the byte array is compressible or not</param>
        private void WriteRawBytesAllInternal(byte[] dataBytes, bool compressible)
        {
            RawBytesDataContent content = new RawBytesDataContent(dataBytes, compressible);
            this.WriteContent(content);
        }

        /// <summary>
        /// Read data from current DataClient as a byte array
        /// </summary>
        /// <returns>data in current DataClient as a byte array</returns>
        private byte[] ReadRawBytesAllInternal()
        {
            byte[] dataArray = this.ReadBytes();
            if (dataArray == null || dataArray.Length == 0)
            {
                throw new DataException(DataErrorCode.NoDataAvailable, null, this.Id);
            }

            try
            {
                RawBytesDataContent content = RawBytesDataContent.Parse(dataArray);
                return content.RawBytes;
            }
            catch (Exception e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClient] .ReadRawBytesAll: failed to decompress data");
                throw new DataException(DataErrorCode.DataInconsistent, e, this.Id);
            }
        }

        /// <summary>
        /// Read data content from current DataClient
        /// </summary>
        /// <returns>data content in current DataClient</returns>
        private byte[] ReadBytes()
        {
            try
            {
                return this.dataContainer.GetData();
            }
            catch (DataException e)
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Error, 0, "[DataClientInternal] .ReadBytes: receives exception {0}", e);
                e.DataClientId = this.id;
                throw;
            }
        }

        /// <summary>
        /// Check if this DataClient instance is read-only
        /// </summary>
        private void CheckIfReadOnly()
        {
            if (this.isReadOnly)
            {
                throw new DataException(DataErrorCode.DataClientReadOnly, SR.DataClientReadOnly);
            }
        }

        /// <summary>
        /// Check if this DataClient instance is disposed
        /// </summary>
        private void CheckIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(null, Microsoft.Hpc.Scheduler.Session.SR.DataClientDisposed);
            }
        }
    }
}
