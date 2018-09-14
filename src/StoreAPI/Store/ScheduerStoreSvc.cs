//------------------------------------------------------------------------------
// <copyright file="AuthenticationUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      SchedulerStore object.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Properties;

    /// <summary>
    /// This interface specifies that when an object is being disposed, it will partially be disposed on the remote server.
    /// The store will call the remote disposal asynchronously, so that a Dispose() call can return in a timely fashion
    /// even if the server is down.
    /// </summary>
    internal interface IRemoteDisposable
    {
        void RemoteDispose();
    }

    static public class SchedulerStore
    {
        static public async Task<ISchedulerStore> ConnectAsync(StoreConnectionContext context, CancellationToken token)
        {
            return await ConnectAsync(context, token, ConnectMethod.Undefined).ConfigureAwait(false);
        }

        static public async Task<ISchedulerStore> ConnectAsync(StoreConnectionContext context, CancellationToken token, ConnectMethod method)
        {
            return await SchedulerStoreSvc.RemoteConnectAsync(context, token, method).ConfigureAwait(false);
        }

        static public async Task<ISchedulerStore> ConnectAsync(string server, CancellationToken token)
        {
            return await ConnectAsync(server, token, ConnectMethod.Undefined);
        }

        static public async Task<ISchedulerStore> ConnectAsync(string server, CancellationToken token, ConnectMethod method)
        {
            return await ConnectAsync(new StoreConnectionContext(server, token), token, method).ConfigureAwait(false);
        }

        [Obsolete("No use this sync method, instead use ConnectAsync")]
        static public ISchedulerStore Connect(string schedulerNode)
        {
            StoreConnectionContext context = new StoreConnectionContext(schedulerNode, CancellationToken.None);
            return ConnectAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        }

        [Obsolete("No use this sync method, instead use ConnectAsync")]
        static public ISchedulerStore Connect(string schedulerNode, int port)
        {
            StoreConnectionContext context = new StoreConnectionContext(schedulerNode, CancellationToken.None) { Port = port };
            return ConnectAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        }

        [Obsolete("No use this sync method, instead use ConnectAsync")]
        static public ISchedulerStore ServiceAsClient(string schedulerNode, ServiceAsClientIdentityProvider provider)
        {
            StoreConnectionContext context = new StoreConnectionContext(schedulerNode, CancellationToken.None) { IdentityProvider = provider, ServiceAsClient = true };
            return ConnectAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        }

        static public ISchedulerStore Server(ISchedulerStoreInternal store, ConnectionToken connectionToken, CancellationToken cancellationToken)
        {
            return new SchedulerStoreSvc(store, connectionToken, cancellationToken);
        }

        static public void SetInterfaceMode(bool fConsole, IntPtr hwnd)
        {
            _fConsole = fConsole;
            _hWnd = hwnd;
        }

        static public PropertyDescriptor[] GetStandardPropertyDescriptors(ObjectType typeMask, PropertyId[] propIds)
        {
            return PropertyLookup.GetPropertyDescriptors(typeMask, propIds);
        }

        public static bool IsGpuJob(ISchedulerStore store, int jobId)
        {
            bool isGpuJob = false;
            StorePropertyDescriptor[] descs = (StorePropertyDescriptor[])store.GetPropertyDescriptors(new string[] { JobPropertyIds.JobGpuCustomPropertyName }, ObjectType.Job);
            if (descs[0].PropId != StorePropertyIds.Error)
            {
                StoreProperty customProp = new StoreProperty(descs[0].PropId, true);
                StoreProperty[] customProps = new StoreProperty[] { customProp };
                store.GetCustomProperties(ObjectType.Job, jobId, out customProps);
                if (customProps.Length > 0 &&
                    customProps[0].PropName.Equals(JobPropertyIds.JobGpuCustomPropertyName) &&
                    customProps[0].Value != null)
                {
                    bool.TryParse(customProps[0].Value.ToString(), out isGpuJob);
                }
            }

            return isGpuJob;
        }

        static internal IntPtr _hWnd = IntPtr.Zero;
        static internal bool _fConsole = true;
    }

    /// <summary>
    /// ScheduleStoreSvc
    /// This is the implementation of the ISchedulerStore object that runs on
    /// the client computer.  This class manages the connection to the server
    /// and handles creation of any objects that are used by the client with
    /// the exception of enumerators which are instantiated on the server.
    /// 
    /// An important part of this object is to maintain the ConnectionToken
    /// that is used by the server to authenticate the client and determine
    /// what rights the client has to modify objects in the Schedule Store.
    /// This ConnectionToken object is hidden from the user of the the API.
    /// </summary>

    internal class SchedulerStoreSvc : ISchedulerStore, IEventControl
    {
        private ManualResetEvent _eventShutdown = new ManualResetEvent(false);
        private List<AsyncResult> _pendinglist = new List<AsyncResult>(10);
        private Thread monitorThread;
        private Thread reconnectThread;

        public static async Task<ISchedulerStore> RemoteConnectAsync(StoreConnectionContext context, CancellationToken token, ConnectMethod method = ConnectMethod.Undefined)
        {
            var storeSvc = new SchedulerStoreSvc(context);
            await storeSvc.InitializeAsync(token, method).ConfigureAwait(false);
            return storeSvc;
        }

        internal bool Disposing { get; private set; } = false;

        private bool _isDisposed = false;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.Disposing = true;
                if (null != _storeServer && !StoreInProc)
                {
                    try
                    {
                        InvokeRemoteDispose(_storeServer);
                    }
                    catch (Exception)
                    {
                        //Runtime is being shutdown when this call to disposing was made
                        //It probably came from the Scheduler.Dispose method while that object was being gc-ed
                        //If the runtime system is also being shutdown at the same time, disconnect may fail
                    }

                    _storeServer = null;
                }

                if (null != _pingTimer)
                {
                    _pingTimer.Dispose();
                    _pingTimer = null;
                }

                _eventShutdown?.Set();

                if (monitorThread != null)
                {
                    WaitForThreadToFinish(monitorThread, 500);
                    monitorThread = null;
                }

                if (reconnectThread != null)
                {
                    WaitForThreadToFinish(reconnectThread, 500);
                    reconnectThread = null;
                }
            }

#if !net40
            CommonEventSource.Current.Message("The store server on scheduler store svc is cleared.");
#endif
            _isDisposed = true;
        }

        void WaitForThreadToFinish(Thread t, int timeout)
        {
            try
            {
                bool ret = t.Join(timeout);
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        delegate void RemoteDisposalDelegate();

        internal bool InvokeRemoteDispose(IRemoteDisposable target)
        {
            return InvokeRemoteDispose(target, 500);
        }

        // Returns true if the asynchronous dispose call completed in the specified time
        internal bool InvokeRemoteDispose(IRemoteDisposable target, int timeout)
        {
            // If we are part of the scheduler process, invoke the method directly
            if (StoreInProc)
            {
                target.RemoteDispose();
                return true;
            }

            RemoteDisposalDelegate del = delegate () { target.RemoteDispose(); };
            IAsyncResult result = del.BeginInvoke(null, null);
            if (result.AsyncWaitHandle.WaitOne(timeout, true))
            {
                del.EndInvoke(result);
                return true;
            }
            return false;
        }

        private StoreServer _storeServer = null;

        internal ISchedulerStoreInternal RemoteServer => _storeServer.Server;

        internal StoreServer ServerWrapper => _storeServer;

        internal ConnectionToken Token => _storeServer.Token;

        public bool StoreInProc { get; } = false;

        /// <summary>
        /// This name is actually the cluster name.
        /// </summary>
        internal string clusterName = null;

        private StoreConnectionContext context;

        public bool OverHttp => this.context != null && this.context.IsHttp;

        public string Owner => _storeServer.Token?.UserName;

        public string OwnerSid => _storeServer.Token?.UserSid;

        const string SchedulerGuid = "SchedulerGuid";

        internal ServiceAsClientIdentityProvider IdentityProvider { get; private set; }

        public SchedulerStoreSvc(StoreConnectionContext context)
        {
            this.context = context;
            this._storeServer = new StoreServer(this);
            this.IdentityProvider = context.IdentityProvider;
        }

        CancellationToken token;

        internal async Task InitializeAsync(CancellationToken token, ConnectMethod method = ConnectMethod.Undefined)
        {
            this.token = token;

            this.clusterName = await this._storeServer.ConnectAsync(this.context, token, method).ConfigureAwait(false);

            object guidObj;
            if (this._storeServer.ServerProps.TryGetValue(SchedulerGuid, out guidObj))
            {
                _schedulerGuid = (Guid)guidObj;
            }

            this._pingTimer = new Timer(PingScheduler, _schedulerGuid, PingInterval, PingInterval);

            if (ServerVersion.Version < ClientVersion.Version)
            {
                IEnumerable<PropertyConverter> compatConverters = PropertyVersioning.GetBackCompatPropConverters(ServerVersion.Version);
                foreach (PropertyConverter converter in compatConverters)
                {
                    this.AddPropertyConverter(converter);
                }
            }

            // Converter to build client-side error messages
            this.AddPropertyConverter(new ErrorMessageConverter());
            this.CreateThreads();
        }

        internal SchedulerStoreSvc(ISchedulerStoreInternal store, ConnectionToken connectionToken, CancellationToken cancellationToken)
        {
            this.token = cancellationToken;

            _storeServer = new StoreServer(this);

            clusterName = store.Name;

            _storeServer.LocalServer(store, connectionToken);

            StoreInProc = true;

            store.RegisterEventPacketHandler(connectionToken, InProcEventHandler);

            _pingTimer = new Timer(PingScheduler, _schedulerGuid, PingInterval, PingInterval);

        }

        void InProcEventHandler(Packets.EventPacket packet)
        {
            EventListener.ProcessEventPacket(this, packet);
        }

        public string Name
        {
            get { return clusterName; }
        }

        public Version GetServerVersion()
        {
            return this.ServerVersion.Version;
        }

        public int GetServerLinuxHttpsValue()
        {
            return _storeServer.GetServerLinuxHttpsValue();
        }

        public Dictionary<string, object> GetServerProperties()
        {
            return _storeServer.ServerProps;
        }

        public void PurgeCredentials(string username)
        {
            try
            {
                CredentialCache.PurgeCredential(clusterName, username);
            }
            catch (Exception)
            {
                //if the server version is older than v3sp1 then
                //it reflects a problem with caching, for later server
                //this command failing is not really a problem  
                if (ServerVersion.IsOlderThanV3SP1)
                {
                    throw;
                }
            }

            //if the server version is equal to v3 sp1 or later, disable credential reuse on the server
            if (!ServerVersion.IsOlderThanV3SP1)
            {
                _storeServer.DisableCredentialReuse(username);
            }
        }

#pragma warning disable 618 // disable obsolete warnings (for UserPrivilege)

        public UserPrivilege GetUserPrivilege()
        {
            return _storeServer.GetUserPrivilege();
        }

#pragma warning restore 618

        public UserRoles GetUserRoles()
        {
            UserRoles roles = _storeServer.GetUserRoles();

            return roles;
        }

        public void GetCustomProperties(ObjectType obType, int objId, out StoreProperty[] props)
        {
            if (obType != ObjectType.Job && obType != ObjectType.Task)
            {
                throw new SchedulerException(ErrorCode.Operation_InvalidOperation, "Only job and task can get custom properties.");
            }
            ServerWrapper.Object_GetCustomProperties(obType, objId, out props);
        }

        internal PropertyRow GetPropsFromServer(ObjectType obType, Int32 itemId, PropertyId[] propertyIds)
        {
            // Check to see if the caller specified GetProps(null).  
            // If so get all props.
            if (propertyIds != null && (propertyIds.Length == 0 || (propertyIds.Length == 1 && propertyIds[0] == null)))
            {
                propertyIds = null;
            }

            List<PropertyConverter> converters = null;
            PropertyConversionMap conversionMap = null;
            bool needDeconversion = false;
            if (RequiresPropertyConversion)
            {
                converters = GetPropertyConverters();
                if (propertyIds != null)
                {
                    PropertyId[] convertedIds;
                    needDeconversion = PropertyLookup.PreGetProps_Convert(converters, propertyIds, out convertedIds, out conversionMap);
                    propertyIds = convertedIds;
                }
                else
                {
                    // Since we don't know which properties we'll get yet, always try to deconvert them when they return from the server
                    needDeconversion = true;
                }
            }

            StoreProperty[] props;

            _storeServer.Object_GetProps(obType, itemId, propertyIds, out props);

            if (needDeconversion)
            {
                if (conversionMap != null)
                {
                    // Call the faster variant, since we already have the conversion map
                    PropertyLookup.PostGetProps_Deconvert(conversionMap, props);
                }
                else
                {
                    StoreProperty[] deconvertedProps;
                    PropertyLookup.PostGetProps_Deconvert(converters, props, out deconvertedProps, out conversionMap);
                    props = deconvertedProps;
                }
            }

            PropertyRow row = new PropertyRow(props);

            return row;
        }

        internal List<PropertyConverter> GetPropertyConverters()
        {
            return new List<PropertyConverter>(_propConverters.Values);
        }

        public void AddPropertyConverter(PropertyConverter converter)
        {
            if (StoreInProc)
            {
                throw new InvalidOperationException("Property conveters should not be used on the server side");
            }

            lock (_propConverters)
            {
                PropertyId id = converter.GetPropId();
                if (_propConverters.ContainsKey(id))
                {
                    throw new InvalidOperationException(String.Format("Converter for property {0} already exists in the store helper", id));
                }
                _propConverters.Add(id, converter);
            }
        }

        internal bool RequiresPropertyConversion
        {
            get { return _propConverters.Count > 0; }
        }

        internal void SetPropsOnServer(ObjectType obType, Int32 itemId, StoreProperty[] props)
        {
            if (props != null && props.Length == 1 && props[0] == null)
            {
                return;
            }

            props = PropertyLookup.ProcessSetProps(this, obType, props);

            StoreTransaction transaction = GetTransaction();

            if (transaction != null)
            {
                transaction.SetObjectProps(obType, itemId, props);
            }
            else
            {
                Debug.Assert(_storeServer != null, "The store server field is null");
                _storeServer.Object_SetProps(obType, itemId, props);
            }
        }

        public PropertyRow GetProps(PropertyId[] pids)
        {
            return GetPropsFromServer(ObjectType.Store, 1, pids);
        }

        public IClusterJob OpenJob(int jobid)
        {
            StoreProperty[] existingProps;

            ServerWrapper.Job_VerifyId(jobid, out existingProps);

            return new JobEx(jobid, Token, this, existingProps);
        }

        internal static StoreProperty[] VerifyJobSource(StoreProperty[] createProps)
        {
            bool sourceFound = false;

            if (createProps != null)
            {
                foreach (StoreProperty prop in createProps)
                {
                    if (prop.Id == JobPropertyIds.ClientSource)
                    {
                        sourceFound = true;
                        break;
                    }
                }
            }

            if (sourceFound == false)
            {
                List<StoreProperty> props = new List<StoreProperty>();

                if (createProps != null)
                {
                    props.AddRange(createProps);
                }

                string filename = null;

                try
                {
                    filename = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
                }
                catch
                {
                    filename = "unknown";
                }

                // Sometimes there will be a debugger attached.  In 
                // this case we need to remove another extension.

                filename = Path.GetFileNameWithoutExtension(filename);

                if (filename.Length > _kSourceSizeLimit)
                {
                    filename = filename.Substring(0, _kSourceSizeLimit);
                }

                props.Add(new StoreProperty(JobPropertyIds.ClientSource, filename));

                createProps = props.ToArray();
            }

            return createProps;
        }

        const int _kSourceSizeLimit = 63;

        public IClusterJob CreateJob(StoreProperty[] jobProperties)
        {
            jobProperties = PropertyLookup.ProcessSetProps(this, ObjectType.Job, jobProperties);
            jobProperties = VerifyJobSource(jobProperties);
            Int32 jobid = 0;
            _storeServer.Job_AddJob(ref jobid, jobProperties);
            return new JobEx(jobid, Token, this, jobProperties);
        }

        public IClusterJob CreateJobFromXml(XmlReader reader)
        {
            return CreateJobFromXml(reader, null);
        }

        public IClusterJob CreateJobFromXml(XmlReader reader, StoreProperty[] existingProps)
        {
            JobPropertyBag jobBag = new JobPropertyBag();
            jobBag.ReadXML(reader, XmlImportOptions.None);
            StoreProperty[] createProps = VerifyJobSource(existingProps);
            Int32 jobId = 0;
            _storeServer.Job_AddJob(ref jobId, createProps);
            JobEx job = new JobEx(jobId, Token, this, null);
            jobBag.CommitToJob(this, job);
            return job;
        }

        public int DeleteJob(int jobid)
        {
            _storeServer.Job_DeleteJob(jobid);
            return 0;
        }

        public IJobRowSet OpenJobRowSet()
        {
            LocalJobRowSet rowset = new LocalJobRowSet(this, RowSetType.Snapshot);
            return rowset;
        }

        public IJobRowSet OpenJobRowSet(RowSetType type)
        {
            return new LocalJobRowSet(this, type);
        }

        public IRowEnumerator OpenJobEnumerator()
        {
            return new LocalRowEnumerator(this, ObjectType.Job, JobPropertyIds.JobObject);
        }

        public IRowEnumerator OpenJobHistoryEnumerator()
        {
            return new LocalRowEnumerator(this, ObjectType.JobHistory, null);
        }

        public void GetJobShrinkRequests(int jobid, out Dictionary<string, Dictionary<int, ShrinkRequest>> shrinkRequestsByNode)
        {
            shrinkRequestsByNode = null;
            ServerWrapper.Job_GetShrinkRequests(jobid, out shrinkRequestsByNode);
        }

        public void AddJobShrinkRequest(int jobid, int resourceId, int nodeid, ShrinkRequest request)
        {
            ServerWrapper.Job_AddShrinkRequest(jobid, resourceId, nodeid, request);
        }


        public IStoreManager OpenStoreManager()
        {
            var storeServer = this._storeServer;

            if (storeServer != null)
            {
                return new StoreManager(storeServer.Token, this);
            }

            throw new ObjectDisposedException("The SchedulerStoreSvc instance has already been disposed");
        }

        public IClusterResource OpenResource(int resourceid)
        {
            return new ResourceEx(RemoteServer, Token, resourceid, this);
        }

        public IResourceRowSet OpenResourceRowSet()
        {
            return (IResourceRowSet)new LocalResourceRowSet(this, RowSetType.Snapshot);
        }

        public IResourceRowSet OpenResourceRowSet(RowSetType type)
        {
            return (IResourceRowSet)new LocalResourceRowSet(this, type);
        }

        public IRowEnumerator OpenResourceRowEnumerator()
        {
            return new LocalRowEnumerator(this, ObjectType.Resource, ResourcePropertyIds.ResourceObject);
        }

        public INodeRowSet OpenNodeRowSet()
        {
            return OpenNodeRowSet(RowSetType.Snapshot);
        }

        public INodeRowSet OpenNodeRowSet(RowSetType type)
        {
            return (INodeRowSet)new LocalNodeRowSet(this, type);
        }

        public IRowEnumerator OpenNodeEnumerator()
        {
            return new LocalRowEnumerator(this, ObjectType.Node, NodePropertyIds.NodeObject);
        }

        public IClusterNode OpenNode(Guid nodeId)
        {
            int id = ServerWrapper.Node_FindNodeIdByNodeId(nodeId);
            return new NodeEx(id, this);
        }

        public IClusterNode OpenNode(int id)
        {
            Guid nodeId = Guid.Empty;

            if (!ServerWrapper.Node_ValidateNodeId(id, out nodeId))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Node id {0} does not exist", id));
            }

            return new NodeEx(id, this);
        }

        public IClusterNode OpenNode(string name)
        {
            Guid nodeId = Guid.Empty;
            int id = ServerWrapper.Node_FindNodeIdByName(name, out nodeId);
            return new NodeEx(id, this);
        }

        public IClusterNode OpenNode(System.Security.Principal.SecurityIdentifier sid)
        {
            Guid nodeId = Guid.Empty;
            int id = ServerWrapper.Node_FindNodeIdBySID(sid.ToString(), out nodeId);
            return new NodeEx(id, this);
        }

        public void InvalidNodeQueryCache()
        {
            ServerWrapper.Node_InvalidNodeQueryCache();
        }

        public IClusterAllocation OpenAllocationObject(int allocationId)
        {
            if (!RemoteServer.Allocation_VerifyId(Token, allocationId))
            {
                throw new ArgumentException("Bad id");
            }

            return new AllocationObject(this, allocationId);
        }

        public IClusterAllocation OpenAllocationObject(int nodeId, int taskId)
        {
            int id = RemoteServer.Allocation_FindIdByNodeAndTask(Token, nodeId, taskId);
            if (id == 0)
            {
                throw new ArgumentException("Bad node id or taskid");
            }

            return new AllocationObject(this, id);
        }

        public IClusterAllocation OpenAllocationObject(int nodeId, int jobId, int taskNiceId)
        {
            int id = RemoteServer.Allocation_FindIdByNodeJobAndTask(Token, nodeId, jobId, taskNiceId);
            if (id == 0)
            {
                throw new ArgumentException("Bad node id, jobid or task nice id");
            }

            return new AllocationObject(this, id);
        }

        public IClusterPool OpenPool(string poolName)
        {
            int id;

            //verify pool name first
            id = ServerWrapper.Pool_FindPoolIdByName(poolName);
            if (id == 0)
            {
                throw new SchedulerException(ErrorCode.Operation_PoolNonExistent, ErrorCode.MakeErrorParams(poolName));
            }

            return new PoolEx(this, id);
        }

        public IClusterPool AddPool(string poolName)
        {
            var id = ServerWrapper.Pool_AddPool(poolName);
            if (id < 0)
            {
                throw new ArgumentException("Pool creation failed");
            }

            return new PoolEx(this, id);
        }

        public IClusterPool AddPool(string poolName, int poolWeight)
        {
            var id = ServerWrapper.Pool_AddPool(poolName, poolWeight);
            if (id < 0)
            {
                throw new ArgumentException("Pool creation failed");
            }

            return new PoolEx(this, id);
        }

        public void DeletePool(string poolName)
        {
            ServerWrapper.Pool_DeletePool(poolName);
        }

        public void DeletePool(string poolName, bool force)
        {
            ServerWrapper.Pool_DeletePool(poolName, force);
        }

        public IPoolRowSet OpenPoolRowset()
        {
            ServerWrapper.EnumeratePermissionCheck(ObjectType.Pool, 0);
            return new LocalPoolRowSet(this, RowSetType.Snapshot);
        }

        public IPoolRowSet OpenPoolRowSet(RowSetType rowSetType)
        {
            ServerWrapper.EnumeratePermissionCheck(ObjectType.Pool, 0);
            return new LocalPoolRowSet(this, rowSetType);
        }

        #region Scheduler On Azure User Management

        public void AddAzureUser(string username, string passeword, bool isAdmin)
        {
            ServerWrapper.SchedulerOnAzure_AddUser(username, passeword, isAdmin);
        }

        public void RemoveAzureUser(string username)
        {
            ServerWrapper.SchedulerOnAzure_RemoveUser(username);
        }

        public bool ValidateAzureUser(string username, string password)
        {
            return ServerWrapper.SchedulerOnAzure_ValidateUser(username, password);
        }

        #endregion


        public void UpdateTaskNodeStats(int nodeId, int jobId, int taskId, StoreProperty[] props)
        {
            StoreTransaction transaction = GetTransaction();

            if (transaction != null)
            {
                transaction.UpdateTaskNodeStats(nodeId, jobId, taskId, props);
            }
            else
            {
                ConnectionToken token = _storeServer.Token;
                _storeServer.Server.Allocation_UpdateTaskNodeStats(ref token, nodeId, jobId, taskId, props);
            }
        }

        internal void CreateThreads()
        {
            if (this.reconnectThread == null)
            {
                this.reconnectThread = new Thread(this.ReconnectThread) { IsBackground = true };
                this.reconnectThread.Start();
            }

            if (this.monitorThread == null)
            {
                this.monitorThread = new Thread(this.MonitorThread) { IsBackground = true };
                this.monitorThread.Start();
            }
        }

        // StateWatcher()
        // In the near future this will be done by watching
        // change events that come in from the server.  For
        // now we will just poll for the items within a loop.

        DateTime _lastTouched = DateTime.UtcNow;

        private void MonitorThread()
        {
            IStoreManager mgr;
            try
            {
                mgr = OpenStoreManager();
            }
            catch (ObjectDisposedException)
            {
                // exit the thread gracefully when the object is disposed.
                return;
            }

            while (true)
            {
                try
                {
                    if (_eventShutdown.WaitOne(500, false))
                    {
                        break;
                    }
                    if (_storeServer == null)
                    {
                        break;
                    }

                    // Immediately return if SchedulerStoreSvc is being disposed
                    if (this.Disposing)
                    {
                        return;
                    }

                    // Check to see if any of the jobs have changed to the desired state.
                    lock (_pendinglist)
                    {
                        // Clear out any old results that need to be closed from the list.
                        int i = 0;
                        while (i < _pendinglist.Count)
                        {
                            if (_pendinglist[i].IsExpired)
                            {
                                _pendinglist.RemoveAt(i);
                            }
                            else
                            {
                                ++i;
                            }
                        }

                        List<AsyncResult> resultsToRemove = new List<AsyncResult>();
                        foreach (AsyncResult result in _pendinglist)
                        {
                            if (result.IsExpired)
                            {
                                // Skip this one.
                                continue;
                            }

                            try
                            {
                                if (result.objectType == ObjectType.Job)
                                {
                                    // Open the job and see if the state has gone to where we want it to.
                                    IClusterJob job = OpenJob(result.JobId);
                                    PropertyRow props = job.GetProps(JobPropertyIds.State);
                                    if (props[0].Id == JobPropertyIds.State)
                                    {
                                        if (result.DoesStateMatch((JobState)props[0].Value))
                                        {
                                            result.ResultState = (int)props[0].Value;
                                            ManualResetEvent handle = result.AsyncWaitHandle as ManualResetEvent;
                                            handle?.Set();
                                            result.Invoke();
                                            resultsToRemove.Add(result);
                                        }
                                    }
                                }
                                else if (result.objectType == ObjectType.Task)
                                {
                                    IClusterTask task = mgr.OpenGlobalTask(result.JobId);
                                    PropertyId[] ids = { TaskPropertyIds.State };
                                    PropertyRow props = task.GetProps(ids);
                                    if (props[0].Id == TaskPropertyIds.State)
                                    {
                                        if (result.DoesStateMatch((TaskState)props[0].Value))
                                        {
                                            result.ResultState = (int)props[0].Value;

                                            ManualResetEvent handle = result.AsyncWaitHandle as ManualResetEvent;
                                            handle?.Set();
                                            result.Invoke();
                                            resultsToRemove.Add(result);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Swallow up the exception.
                            }
                        }
                        // Remove all the results that have been invoked in this turn
                        foreach (AsyncResult result in resultsToRemove)
                        {
                            _pendinglist.Remove(result);
                        }
                    }

                    // Once a minute we need to go through and touch all the
                    // remote rowsets that we have open and also have the 
                    // server send a keep alive packet to us.
                    if ((DateTime.UtcNow - _lastTouched) > TimeSpan.FromSeconds(59))
                    {
                        _storeServer.RemoveEvent_TriggerTouch();
                        _lastTouched = DateTime.UtcNow;
                        List<LocalRowSet> rowsetsToTouch = new List<LocalRowSet>();
                        lock (_openrowsets)
                        {
                            rowsetsToTouch.AddRange(_openrowsets);
                        }

                        // Perform the touch outside the rowset lock, since it involves a remote call.
                        foreach (LocalRowSet rowset in rowsetsToTouch)
                        {
                            if (!rowset.Disposed)
                            {
                                try { rowset.Touch(); }
                                catch { }
                            }
                        }

                        List<WeakReference> toRemove = new List<WeakReference>();
                        List<LocalRowEnumerator> toTouch = new List<LocalRowEnumerator>();
                        lock (_openrowenums)
                        {
                            foreach (WeakReference rowenumref in _openrowenums)
                            {
                                LocalRowEnumerator rowenum = rowenumref.Target as LocalRowEnumerator;
                                if (rowenum == null)
                                {
                                    toRemove.Add(rowenumref);
                                }
                                else
                                {
                                    toTouch.Add(rowenum);
                                }
                            }

                            foreach (WeakReference rowenumref in toRemove)
                            {
                                _openrowenums.Remove(rowenumref);
                            }
                        }

                        // Perform the touch outside the rowset lock, since it involves a remote call.
                        foreach (LocalRowEnumerator rowenum in toTouch)
                        {
                            if (!rowenum.Disposed)
                            {
                                try { rowenum.Touch(); }
                                catch { }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceWarning("Exception occurred in Monitor thread of Store, ex {0}", ex);
                }
            }
        }

        private void ReconnectThread()
        {
            try
            {
                while (true)
                {
                    if (_eventShutdown.WaitOne(500, false))
                    {
                        break;
                    }
                    if (_storeServer == null)
                    {
                        break;
                    }
                    try
                    {
                        // Check whether there is any reconnect request
                        if (!this.OverHttp)
                        {
                            if (_storeServer.NeedReconnect())
                            {
                                _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.StoreDisconnect));
                                try
                                {
                                    // Immediately return if SchedulerStoreSvc is being disposed
                                    if (this.Disposing)
                                    {
                                        return;
                                    }

                                    // Only wait in the explicitly newed thread.
                                    // Tobe refactored: change the thread to async.
                                    _storeServer.ReconnectInternal(token).GetAwaiter().GetResult();
                                    _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.StoreReconnect));
                                }
                                catch (Exception e)
                                {
                                    TraceHelper.TraceWarning("[SchedulerStoreSvc] ReconnectThread-ReconnectInternal {0}", e);
                                    // On any exception, go directly to the next cycle
                                    // This is because we can't do anything if the store isnt' connected
                                    _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.Exception, e));
                                    continue;
                                }

                                // Tell everybody that is waiting on this reconnect: the store is reconnected!
                                _storeServer.SignalReconnectComplete();
                            }

                            // Check whether there is any register-events request
                            if (Interlocked.Exchange(ref _needRegisterEvt, 0) == 1)
                            {
                                _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.EventDisconnect));
                                try
                                {
                                    ReRegisterForEvents();
                                    _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.EventReconnect));
                                }
                                catch (Exception e)
                                {
                                    TraceHelper.TraceWarning("[SchedulerStoreSvc] ReconnectThread-ReRegisterForEvents {0}", e);
                                    // Try again next cycle.                             
                                    _needRegisterEvt = 1;
                                    _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.Exception, e));
                                }
                            }
                        }
                        else
                        {
                            //over http there is only 1 connection really
                            int eventNeedsRegister = Interlocked.Exchange(ref _needRegisterEvt, 0);
                            if (_storeServer.NeedReconnect() || eventNeedsRegister == 1)
                            {
                                if (!_disconnectedHttp)
                                {
                                    _disconnectedHttp = true;
                                    _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.EventDisconnect));
                                    _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.StoreDisconnect));
                                }
                                try
                                {
                                    // Immediately return if SchedulerStoreSvc is being disposed
                                    if (this.Disposing)
                                    {
                                        return;
                                    }

                                    // Only wait in the explicitly newed thread.
                                    // Tobe refactored: change the thread to async.
                                    _storeServer.ReconnectInternal(token).GetAwaiter().GetResult();
                                    //the store api can talk to the scheduler again                                        
                                    ReRegisterForEvents();
                                    _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.EventReconnect));
                                    _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.StoreReconnect));
                                    _storeServer.SignalReconnectComplete();
                                    _disconnectedHttp = false;
                                }
                                catch (Exception e)
                                {
                                    TraceHelper.TraceWarning("[SchedulerStoreSvc] ReconnectThread-ReRegisterForEvents {0}", e);
                                    // On any exception, go directly to the next cycle This is because we can't do anything if the store isnt' connected
                                    _storeServer.SendReconnectEvent(new SchedulerConnectionEventArgs(SchedulerConnectionEventCode.Exception, e));
                                }
                            }
                            else
                            {
                                _disconnectedHttp = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceHelper.TraceWarning("Exception occurred in ReconnectThread of Store, ex {0}", ex);
                        // Swallow any exceptions that we get here. Since this is a worker thread, there is
                        // not an easy way to indicate an error has occured.
                    }
                }
            }
            catch (ThreadAbortException) { }
        }

        public AsyncResult RegisterForJobStateChange(int jobId, int state, AsyncCallback callback, object param)
        {
            AsyncResult result = new AsyncResult(ObjectType.Job, jobId, state, callback, param);

            result.RegisterForEvent(this);

            lock (_pendinglist)
            {
                _pendinglist.Add(result);
            }

            return result;
        }

        public AsyncResult RegisterForTaskStateChange(int taskId, int state, AsyncCallback callback, object param)
        {
            AsyncResult result = new AsyncResult(ObjectType.Task, taskId, state, callback, param);

            result.RegisterForEvent(this);

            lock (_pendinglist)
            {
                _pendinglist.Add(result);
            }

            return result;
        }

        public void CloseAsyncResult(AsyncResult result)
        {
            //result could be null, if the operation to create the result failed
            if (result != null)
            {
                result.Close();
            }
        }

        public IClusterJobProfile OpenProfile(int profileId)
        {
            if (RemoteServer.VerifyProfileId(Token, profileId) == false)
            {
                throw new SchedulerException(ErrorCode.Operation_InvalidProfileId, "");
            }

            return new JobProfile(this, Token, profileId);
        }

        public IClusterJobProfile OpenProfile(string profileName)
        {
            Int32 profileId;

            RemoteServer.GetProfileIdByName(Token, profileName, out profileId);

            if (profileId == -1)
            {
                throw new SchedulerException(ErrorCode.Operation_ProfileNotFound, profileName);
            }

            return new JobProfile(this, Token, profileId);
        }

        public IJobProfileRowSet OpenProfileRowSet()
        {
            return (IJobProfileRowSet)new LocalProfileRowSet(this, RowSetType.Snapshot);
        }

        public IJobProfileRowSet OpenProfileRowSet(RowSetType type)
        {
            return (IJobProfileRowSet)new LocalProfileRowSet(this, type);
        }

        public IRowEnumerator OpenProfileEnumerator()
        {
            return new LocalRowEnumerator(this, ObjectType.JobTemplate, JobTemplatePropertyIds.TemplateObject);
        }

        // This is only for reporting in V3 or above
        public IRowEnumerator OpenAllocationEnumerator()
        {
            if (this.ServerVersion.IsV2)
            {
                throw new NotImplementedException("This method only works in version 3.0 and above");
            }
            return new LocalRowEnumerator(this, ObjectType.Allocation, AllocationProperties.AllocationObject);
        }

        public void SetClusterEnvironmentVariable(string name, string value)
        {
            ServerWrapper.SetClusterEnvironmentVariable(Token, name, value);
        }

        public Dictionary<string, string> GetClusterEnvironmentVariables()
        {
            return RemoteServer.GetClusterEnvironmentVariables(Token);
        }

        public IEnumerable<NodeGroup> GetNodeGroups()
        {
            return ServerWrapper.GetNodeGroups();
        }

        public string[] GetNodesFromGroup(string nodeGroupName)
        {
            return ServerWrapper.GetNodesFromGroup(nodeGroupName);
        }

        public void CancelAsyncWait(IAsyncResult result)
        {
            CloseAsyncResult((AsyncResult)result);
        }

        Dictionary<int, Stack<StoreTransactionWrapper>> _transactions = new Dictionary<int, Stack<StoreTransactionWrapper>>(10);

        public IClusterStoreTransaction BeginTransaction()
        {
            // First see if we have a transaction already for this thread.

            int currThreadId = Thread.CurrentThread.ManagedThreadId;

            StoreTransactionWrapper wrapper = new StoreTransactionWrapper(currThreadId, this);
            AddTransaction(currThreadId, wrapper);

            return wrapper;
        }

        private void AddTransaction(int currThreadId, StoreTransactionWrapper wrapper)
        {
            lock (_transactions)
            {
                Stack<StoreTransactionWrapper> stack = null;
                if (!_transactions.TryGetValue(currThreadId, out stack))
                {
                    stack = new Stack<StoreTransactionWrapper>();
                    _transactions[currThreadId] = stack;
                }
                //we should check that this transaction has not been already added 
                foreach (StoreTransactionWrapper item in stack)
                {
                    if (item == wrapper)
                    {
                        throw new InvalidProgramException("Should not add a transaction wrapper that has already been added");
                    }
                }
                stack.Push(wrapper);
            }
        }

        public StoreTransaction GetTransaction()
        {
            int currThreadId = Thread.CurrentThread.ManagedThreadId;

            lock (_transactions)
            {
                Stack<StoreTransactionWrapper> stack = null;
                if (_transactions.TryGetValue(currThreadId, out stack) && stack.Count > 0)
                {
                    return stack.Peek().Transaction;
                }
                else
                {
                    return null;
                }
            }
        }

        internal void RunTransaction(StoreTransactionWrapper wrapper)
        {
            RemoveTransaction(wrapper);

            if (wrapper.Transaction.Items.Count > 0)
            {
                ConnectionToken token = Token;

                CallResult result = RemoteServer.RunTransaction(ref token, wrapper.Transaction);

                if (result != CallResult.Succeeded)
                {
                    result.Throw();
                }
            }
        }

        private void RemoveTransaction(StoreTransactionWrapper wrapper)
        {
            // Remove it from the list first, then run it.
            int currThreadId = Thread.CurrentThread.ManagedThreadId;

            lock (_transactions)
            {
                Stack<StoreTransactionWrapper> stack = null;
                if (_transactions.TryGetValue(currThreadId, out stack))
                {
                    if (stack.Count > 0 && stack.Peek() == wrapper)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        foreach (StoreTransactionWrapper item in stack)
                        {
                            if (item == wrapper)
                            {
                                throw new InvalidProgramException("Cannot submit/cancel a transaction context when it has sub contexts");
                            }
                        }
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        internal void CancelTransaction(StoreTransactionWrapper wrapper)
        {
            RemoveTransaction(wrapper);
        }

        internal void DetachTransaction(StoreTransactionWrapper wrapper)
        {
            RemoveTransaction(wrapper);
        }

        internal void AttachTransaction(StoreTransactionWrapper wrapper)
        {
            int currThreadId = Thread.CurrentThread.ManagedThreadId;
            if (currThreadId != wrapper.Transaction.ThreadId)
            {
                throw new InvalidProgramException("StoreTransaction has to be attached back to the same thread from which it was detached");
            }
            AddTransaction(currThreadId, wrapper);

        }


        /// <summary>
        /// This method is used to signal any pending requests for completed job state changes
        /// This method should be called only when the monitor thread is not present since
        /// the monitor thread also signals completed pending requests
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="props"></param>
        void CheckJobPendingWaits(int jobId, StoreProperty[] props)
        {
            // See if we have a state property change.

            StoreProperty prop = null;

            for (int i = 0; i < props.GetLength(0); i++)
            {
                prop = props[i];

                if (prop.Id == JobPropertyIds.State)
                {
                    break;
                }

                prop = null;
            }

            if (prop == null)
            {
                return;
            }

            // Check to see if we are waiting for any jobs
            // to finish being submitted.

            //make a copy of the pending list under lock
            lock (_pendinglist)
            {
                List<AsyncResult> resultsToRemove = new List<AsyncResult>();

                foreach (AsyncResult result in _pendinglist)
                {
                    if (result.IsExpired)
                    {
                        // Skip this one.
                        continue;
                    }

                    if (result.objectType == ObjectType.Job)
                    {
                        if (result.JobId == jobId)
                        {
                            if (result.DoesStateMatch((JobState)prop.Value))
                            {
                                Debug.WriteLine("Async notify of job reaching state");

                                result.ResultState = (int)prop.Value;
                                ManualResetEvent wait = result.AsyncWaitHandle as ManualResetEvent;
                                if (wait != null)
                                {
                                    wait.Set();
                                }
                                result.Invoke();
                                resultsToRemove.Add(result);
                            }
                        }
                    }
                }
                // Remove all the results that have been invoked in this turn
                foreach (AsyncResult result in resultsToRemove)
                {
                    _pendinglist.Remove(result);
                }

            }
        }


        object _eventLock = new object();


        void IEventControl.OnJobEvent(Int32 jobId, EventType eventType, StoreProperty[] props)
        {
            // Check to see if we have any clients waiting for a state change
            // on this job object only if the monitor thread does not exist
            // Monitor thread does not exist on the server

            if (eventType == EventType.Modify && !this.OverHttp)
            {
                CheckJobPendingWaits(jobId, props);
            }

            // Notify any registered clients of the job.

            lock (_eventLock)
            {
                if (_jobEvent != null)
                {
                    _jobEvent(jobId, eventType, props);
                }
            }
        }

        void IEventControl.OnTaskEvent(Int32 jobId, Int32 taskSystemId, Int32 taskNiceId, Int32 taskInstanceId, EventType eventType, StoreProperty[] props)
        {
            // Check to see if we have any clients waiting for a state change on this job object.
            if (eventType == EventType.Modify)
            {
                //CheckTaskPendingWaits(jobId, props);
            }

            // Last, notify any register clients of the job.
            lock (_eventLock)
            {
                _taskEvent?.Invoke(jobId, taskSystemId, new TaskId(jobId, taskNiceId, taskInstanceId), eventType, props);
                SendJobTaskEvent(jobId, taskSystemId, new TaskId(jobId, taskNiceId, taskInstanceId), eventType, props);
            }
        }

        void IEventControl.OnResourceEvent(Int32 id, EventType eventType, StoreProperty[] props)
        {
            lock (_eventLock)
            {
                _resourceEvent?.Invoke(id, eventType, props);
            }
        }


        void IEventControl.OnNodeEvent(Int32 id, EventType eventType, StoreProperty[] props)
        {
            lock (_eventLock)
            {
                _nodeEvent?.Invoke(id, eventType, props);
            }
        }

        void IEventControl.OnProfileEvent(Int32 id, EventType eventType, StoreProperty[] props)
        {
            lock (_eventLock)
            {
                _profileEvent?.Invoke(id, eventType, props);
            }
        }

        void IEventControl.OnRowsetChange(int rowsetId, int rowCount, int objectIndex, int objectPreviousIndex, int objectId, EventType eventType, StoreProperty[] props)
        {
            List<LocalRowSet> list = new List<LocalRowSet>();
            lock (_openrowsets)
            {
                list.AddRange(_openrowsets);
            }

            foreach (LocalRowSet rowset in list)
            {
                try
                {
                    if (!rowset.Disposed && rowset.GetGlobalId() == rowsetId)
                    {
                        rowset.OnChangeNotifyFromServer(rowCount, objectId, objectIndex, objectPreviousIndex, eventType, props);
                    }
                }
                catch (NullReferenceException)
                {
                }
            }
        }

        void IEventControl.GetEventData(int connectionId, DateTime lastReadEvent, out List<byte[]> eventData)
        {
            ServerWrapper.GetEventDataOverHttp(
                             connectionId,
                             lastReadEvent,
                             out eventData);
        }

        int _needRegisterEvt = 0;

        void IEventControl.RequestRegisterEvt()
        {
            _needRegisterEvt = 1;
        }

        List<LocalRowSet> _openrowsets = new List<LocalRowSet>();

        public void RegisterRowSet(LocalRowSet rowset)
        {
            lock (_openrowsets)
            {
                _openrowsets.Add(rowset);
            }
        }

        public void UnRegisterRowSet(LocalRowSet rowset)
        {
            lock (_openrowsets)
            {
                _openrowsets.Remove(rowset);
            }
        }

        List<WeakReference> _openrowenums = new List<WeakReference>();

        internal WeakReference RegisterRowEnum(LocalRowEnumerator rowenum)
        {
            WeakReference entry = new WeakReference(rowenum);
            lock (_openrowenums)
            {
                _openrowenums.Add(entry);
            }

            return entry;
        }

        internal void UnRegisterRowEnum(WeakReference rowenumref)
        {
            lock (_openrowenums)
            {
                if (_openrowenums.Contains(rowenumref))
                {
                    _openrowenums.Remove(rowenumref);
                }
            }
        }

        public PropertyDescriptor[] GetPropertyDescriptors(ObjectType typeMask, PropertyId[] propIds)
        {
            return PropertyLookup.GetPropertyDescriptors(typeMask, propIds);
        }

        public PropertyDescriptor[] GetPropertyDescriptors(ObjectType typeMask, PropFlags flagMask)
        {
            return PropertyLookup.GetPropertyDescriptors(this, typeMask, flagMask);
        }

        public PropertyDescriptor[] GetPropertyDescriptors(string[] names, ObjectType typeMask)
        {
            return PropertyLookup.GetPropertyDescriptors(this, names, typeMask);
        }

        public PropertyId GetPropertyId(ObjectType type, StorePropertyType propertyType, string propertyName)
        {
            return ServerWrapper.GetPropertyId(type, propertyType, propertyName);
        }

        public PropertyId CreatePropertyId(ObjectType type, StorePropertyType propertyType, string propertyName, string propertyDescription)
        {
            return ServerWrapper.CreatePropertyId(type, propertyType, propertyName, propertyDescription);
        }

        public byte[] EncryptCredential(string username, string password)
        {
            return ServerWrapper.EncryptCredential(username, password, null);
        }

        public void SetCachedCredentials(string userName, string password)
        {
            SetCachedCredentials(userName, password, null);
        }

        public void SetCachedCredentials(string userName, string password, string ownerName)
        {
            if (this.ServerWrapper.UsingAAD)
            {
                if (string.IsNullOrEmpty(userName)) throw new ArgumentNullException(nameof(userName));
            }
            else if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                // We don't have a complete credential set, go ahead and prompt the user for credentials.
                SecureString securePwd = new SecureString();
                password = null;

                Credentials.PromptForCredentials(this.Name, ref userName, ref securePwd, SchedulerStore._fConsole, SchedulerStore._hWnd);

                // Now we need to materialize the cleartext password so we can pass it to the server
                password = Credentials.UnsecureString(securePwd);
            }

            byte[] cachedCreds = ServerWrapper.EncryptCredential(userName, password, ownerName);

            if (cachedCreds != null && !this.ServerWrapper.UsingAAD)
            {
                //
                // The credentials are OK, go ahead and cache them.
                //
                try
                {
                    CredentialCache.CacheCredential(this.Name, userName, cachedCreds);
                }
                catch (Exception)
                {
                    //if the server version is older than v3sp1 then
                    //it reflects a problem with caching, for later server
                    //this command failing is not really a problem  
                    if (ServerVersion.IsOlderThanV3SP1)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Call through to the server wrapper with the encrypted certificate blob and the password used to encrypt the 
        /// certificate blob
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pfxPassword"></param>
        /// <param name="reusable"></param>
        /// <param name="certificate"></param>
        public void SaveUserCertificate(string userName, SecureString pfxPassword, bool? reusable, byte[] certificate)
        {
            CheckOSVersionForSoftcard();
            ServerWrapper.SaveCertificate(userName, pfxPassword, reusable, certificate);
        }

        /// <summary>
        /// Call through to the server wrapper with the extended data for the user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="extendedData"></param>
        public void SaveUserExtendedData(string userName, string extendedData)
        {
            this.ServerWrapper.SaveExtendedData(userName, extendedData);
        }

        public UserCredential[] GetCredentialList(string ownerName, bool all)
        {
            return this.ServerWrapper.GetCredentialList(ownerName, all);
        }

        public string EnrollCertificate(string templateName)
        {

            CheckOSVersionForSoftcard();

            ServerWrapper.CheckMinServerVersion(VersionControl.V3SP2);

            string[] settingsToLoad = { "HpcSoftCardTemplate" };
            List<string> settingsValues = null;
            GetConfigSettingsValues(settingsToLoad, out settingsValues);

            string serverTemplateName = settingsValues[0];
            string templateToEnroll = null;
            if (string.IsNullOrEmpty(serverTemplateName))
            {
                templateToEnroll = templateName;
            }
            else
            {
                if (!string.IsNullOrEmpty(templateName))
                {
                    if (string.Compare(templateName, serverTemplateName, true) != 0)
                    {
                        //if the user specifies a templatename that is different from the one specified by the server
                        //throw an error
                        throw new SchedulerException(ErrorCode.Operation_ServerTemplatePresent, ErrorCode.MakeErrorParams(templateName, serverTemplateName));
                    }
                }
                templateToEnroll = serverTemplateName;
            }

            if (string.IsNullOrEmpty(templateToEnroll))
            {
                throw new SchedulerException(ErrorCode.Operation_NoTemplateName, "");
            }

            //convert to common template name since the enroll certificate commands only recognize common template names
            //and not the user friendly ones
            string templateCommonName = null;

            ServerWrapper.GetTemplateCommonName(templateToEnroll, out templateCommonName);

            if (string.IsNullOrEmpty(templateCommonName))
            {
                throw new SchedulerException(ErrorCode.Operation_NoTemplateName, "");
            }

            string thumbprint = null;
            SecureString password = CertificateHelper.GetRandomPassword();

            byte[] certBytes = null;
            try
            {
                certBytes = Enroll.EnrollToPfxAsn1UserContext(templateCommonName, Credentials.UnsecureString(password));
            }
            catch (Exception e)
            {
                if (e is Win32Exception)
                {
                    Win32Exception winEx = e as Win32Exception;
                    switch ((uint)winEx.NativeErrorCode)
                    {
                        case 0x8009000B:
                            throw new SchedulerException(ErrorCode.Operation_CertificateNoPrivateKeysExport, "");

                        case 0x80090027:
                            throw new SchedulerException(ErrorCode.Operation_CertificateFailRequest, "");

                        default:
                            throw new SchedulerException(ErrorCode.Operation_CertificateEnrollFailureWithCause, winEx.Message);
                    }
                }
                else
                {
                    throw new SchedulerException(ErrorCode.Operation_CertificateEnrollFailureWithCause, e.Message);
                }
            }


            if (certBytes == null)
            {
                throw new SchedulerException(ErrorCode.Operation_CertificateEnrollFailure, "");
            }

            X509Certificate2 cert = new X509Certificate2(certBytes, password);

            thumbprint = cert.Thumbprint;

            return thumbprint;
        }

        private static void CheckOSVersionForSoftcard()
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                throw new SchedulerException(ErrorCode.Operation_SoftCardNotSupported, "");
            }
        }



        public void GetCertificateInfo(out SchedulerCertInfo certInfo)
        {
            certInfo = null;
            ServerWrapper.GetCertificateInfo(out certInfo);
        }

        /// <summary>
        /// Get the values for certain cluster config settings
        /// </summary>
        /// <param name="configSettings"></param>
        /// <param name="configValues"></param>
        public void GetConfigSettingsValues(IEnumerable<string> configSettings, out List<string> configValues)
        {
            configValues = new List<string>();
            Dictionary<string, string> serverProps = ServerWrapper.Config_GetSettings();

            foreach (string setting in configSettings)
            {
                string value = string.Empty;
                serverProps.TryGetValue(setting, out value);
                if (value == null)
                {
                    value = string.Empty;
                }
                configValues.Add(value);
            }
        }


        const int PingInterval = 30000;
        //when scheduler is disconnected, we need ping more frequently
        private const int ShortPingInterval = 2000;
        private int currentPingInterval = PingInterval;

        Guid _schedulerGuid = Guid.Empty;
        object _schedulerGuidLock = new object();
        Timer _pingTimer = null;
        bool _fInPing = false;
        bool _disconnectedHttp = false;

        Dictionary<PropertyId, PropertyConverter> _propConverters = new Dictionary<PropertyId, PropertyConverter>();

        internal void ReRegisterForEvents()
        {
            TraceHelper.TraceInfo($"Register for events, _jobEventId={_jobEventId}, _taskEventId={_taskEventId}, _nodeEventId={_nodeEventId}, _resourceEventId={_resourceEventId}, _profileEventId={_profileEventId}, _fStoreInProc={StoreInProc}");
            if (_jobEventId != -1 && !StoreInProc)
            {
                _jobEventId = _storeServer.RegisterForEventWithoutRetry(Packets.EventObjectClass.AllJobs, 0, 0, true);
            }
            if (_taskEventId != -1 && !StoreInProc)
            {
                _taskEventId = _storeServer.RegisterForEventWithoutRetry(Packets.EventObjectClass.AllTasks, 0, 0, true);
            }
            if (_nodeEventId != -1 && !StoreInProc)
            {
                _nodeEventId = _storeServer.RegisterForEventWithoutRetry(Packets.EventObjectClass.AllNodes, 0, 0, true);
            }
            if (_resourceEventId != -1 && !StoreInProc)
            {
                _resourceEventId = _storeServer.RegisterForEventWithoutRetry(Packets.EventObjectClass.AllResources, 0, 0, true);
            }
            if (_profileEventId != -1 && !StoreInProc)
            {
                _profileEventId = _storeServer.RegisterForEventWithoutRetry(Packets.EventObjectClass.AllProfiles, 0, 0, true);
            }

            if (!StoreInProc)
            {
                foreach (KeyValuePair<int, JobTaskEventClient> item in _jobTaskEventClnts)
                {
                    item.Value.EvtId = _storeServer.RegisterForEventWithoutRetry(Packets.EventObjectClass.AllTasks, 0, item.Key, true);
                }
            }

            // No need to re-register dynamic rowsets for events, since we can assume that they are out of date.
            // Client code will need to explicitly invaliate these rowsets, and open new ones on the server.
        }

        /// <summary>
        /// This is ping without block. 
        /// For client, Many remote call to scheduler maybe hang up for a while if scheduler disconnected, 
        /// it may ping first, then client can get scheduler status, and determine whether send remote call to scheduler
        /// </summary>
        public bool PingScheduler()
        {
            try
            {
                if (_storeServer.ReConnectMethod == ConnectMethod.WCF && !WcfChannelModule.CheckWcfProxyHealth(this._storeServer.Server))
                {
                    return false;
                }

                Guid newGuid = _storeServer.PingScheduler();
                if (newGuid != Guid.Empty) return true;
                //trigger ping more frequently than usual, so transient disconnect can be solved quickly
                this._pingTimer.Change(ShortPingInterval, ShortPingInterval);
                this.currentPingInterval = ShortPingInterval;
            }
            catch
            {
                // Swallow any exception. They are ignorable.
            }

            return false;
        }

        private void PingScheduler(object guidObj)
        {
            if (_fInPing)
            {
                // Yield
                return;
            }

            _fInPing = true;

            try
            {
                lock (_schedulerGuidLock)
                {
                    if (_schedulerGuid == Guid.Empty)
                    {
                        // Normally for V2 server, the guid will be Empty always
                        return;
                    }

                    // This is newly added in V3
                    Guid newGuid = _storeServer.PingScheduler();

                    if (newGuid == Guid.Empty)
                    {
                        _storeServer.RequestReconnect(false);
                        if (this.currentPingInterval < PingInterval)
                        {
                            //To avoid more traffic to scheduler, we increase the ping interval time by time
                            int interval = this.currentPingInterval * 2;
                            if (interval > PingInterval)
                            {
                                interval = PingInterval;
                            }

                            this._pingTimer.Change(interval, interval);
                            this.currentPingInterval = interval;
                        }
                    }
                    else if (_schedulerGuid != newGuid) //already reconnect
                    {
                        this._pingTimer.Change(PingInterval, PingInterval);
                        this.currentPingInterval = PingInterval;
                        _schedulerGuid = newGuid;
                    }
                }
            }
            catch
            {
                // Swallow any exception. They are ignorable.
            }
            finally
            {
                _fInPing = false;
            }
        }

        SchedulerJobEventDelegate _jobEvent;

        int _jobEventId = -1;

        public event SchedulerJobEventDelegate JobEvent
        {
            add
            {
                lock (_eventLock)
                {
                    if (_jobEvent == null && !StoreInProc)
                    {
                        _jobEventId = _storeServer.RegisterForEvent(Packets.EventObjectClass.AllJobs, 0, 0, false);
                    }

                    _jobEvent += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    _jobEvent -= value;

                    if (_jobEvent == null && !StoreInProc)
                    {
                        _storeServer.UnRegisterForEvent(_jobEventId);
                        _jobEventId = -1;
                    }
                }
            }
        }

        SchedulerTaskEventDelegate _taskEvent;

        int _taskEventId = -1;

        public event SchedulerTaskEventDelegate TaskEvent
        {
            add
            {
                lock (_eventLock)
                {
                    if (_taskEvent == null && !StoreInProc)
                    {
                        _taskEventId = _storeServer.RegisterForEvent(Packets.EventObjectClass.AllTasks, 0, 0, false);
                    }

                    _taskEvent += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    _taskEvent -= value;

                    if (_taskEvent == null && !StoreInProc)
                    {
                        _storeServer.UnRegisterForEvent(_taskEventId);
                        _taskEventId = -1;
                    }
                }
            }
        }

        class JobTaskEventClient
        {
            int _evtId = -1;
            SchedulerTaskEventDelegate _evtClients;

            public event SchedulerTaskEventDelegate EvtClients
            {
                add
                {
                    _evtClients += value;
                }
                remove
                {
                    _evtClients -= value;
                }
            }

            public bool IsEmpty
            {
                get
                {
                    return _evtClients == null;
                }
            }

            public int EvtId
            {
                get { return _evtId; }
                set { _evtId = value; }
            }

            public JobTaskEventClient(int evtId)
            {
                _evtId = evtId;
            }

            public void SendEvent(int jobId, int taskId, TaskId niceId, EventType evtType, StoreProperty[] props)
            {
                _evtClients(jobId, taskId, niceId, evtType, props);
            }
        }

        Dictionary<int, JobTaskEventClient> _jobTaskEventClnts = new Dictionary<int, JobTaskEventClient>();

        internal void RegisterJobTaskEvent(int jobId, SchedulerTaskEventDelegate evtClient)
        {
            lock (_eventLock)
            {
                JobTaskEventClient item = null;
                if (!_jobTaskEventClnts.TryGetValue(jobId, out item))
                {
                    int evtId = -1;
                    if (!StoreInProc)
                    {
                        evtId = _storeServer.RegisterForEvent(Packets.EventObjectClass.AllTasks, 0, jobId, false);
                    }
                    item = new JobTaskEventClient(evtId);
                    _jobTaskEventClnts[jobId] = item;
                }

                item.EvtClients += evtClient;
            }
        }

        internal void UnregisterJobTaskEvent(int jobId, SchedulerTaskEventDelegate evtClient)
        {
            lock (_eventLock)
            {
                JobTaskEventClient item = null;
                if (_jobTaskEventClnts.TryGetValue(jobId, out item))
                {
                    item.EvtClients -= evtClient;

                    if (item.IsEmpty)
                    {
                        if (!StoreInProc)
                        {
                            _storeServer.UnRegisterForEvent(_jobTaskEventClnts[jobId].EvtId);
                        }
                        _jobTaskEventClnts.Remove(jobId);
                    }
                }
            }
        }

        internal void SendJobTaskEvent(int jobId, int taskId, TaskId niceId, EventType evtType, StoreProperty[] props)
        {
            lock (_eventLock)
            {
                JobTaskEventClient item = null;
                if (_jobTaskEventClnts.TryGetValue(jobId, out item))
                {
                    item.SendEvent(jobId, taskId, niceId, evtType, props);
                }
            }
        }

        SchedulerObjectEventDelegate _profileEvent;

        int _profileEventId = -1;

        public event SchedulerObjectEventDelegate ProfileEvent
        {
            add
            {
                lock (_eventLock)
                {
                    if (_profileEvent == null && !StoreInProc)
                    {
                        _profileEventId = _storeServer.RegisterForEvent(Packets.EventObjectClass.AllProfiles, 0, 0, false);
                    }

                    _profileEvent += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    _profileEvent -= value;

                    if (_profileEvent == null && !StoreInProc)
                    {
                        _storeServer.UnRegisterForEvent(_profileEventId);
                        _profileEventId = -1;
                    }
                }
            }
        }

        public event SchedulerConnectionHandler ConnectionEvent
        {
            add
            {
                _storeServer.AddConnectionHandler(value);
            }

            remove
            {
                _storeServer.RemovedConnectionHandler(value);
            }
        }

        SchedulerObjectEventDelegate _nodeEvent;

        int _nodeEventId = -1;

        public event SchedulerObjectEventDelegate NodeEvent
        {
            add
            {
                lock (_eventLock)
                {
                    if (_nodeEvent == null && !StoreInProc)
                    {
                        _nodeEventId = _storeServer.RegisterForEvent(Packets.EventObjectClass.AllNodes, 0, 0, false);
                    }

                    _nodeEvent += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    _nodeEvent -= value;

                    if (_nodeEvent == null && !StoreInProc)
                    {
                        _storeServer.UnRegisterForEvent(_nodeEventId);
                        _nodeEventId = -1;
                    }
                }
            }
        }

        SchedulerObjectEventDelegate _resourceEvent;

        int _resourceEventId = -1;

        public event SchedulerObjectEventDelegate ResourceEvent
        {
            add
            {
                lock (_eventLock)
                {
                    if (_resourceEvent == null && !StoreInProc)
                    {
                        _resourceEventId = _storeServer.RegisterForEvent(Packets.EventObjectClass.AllResources, 0, 0, false);
                    }

                    _resourceEvent += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    _resourceEvent -= value;

                    if (_resourceEvent == null && !StoreInProc)
                    {
                        _storeServer.UnRegisterForEvent(_resourceEventId);
                        _resourceEventId = -1;
                    }
                }
            }
        }

        public void SetJobModifyFilter(JobModifyFilter filter)
        {
            if (!StoreInProc)
            {
                throw new InvalidOperationException("JobModifyFilter could only be set at server side");
            }

            RemoteServer.SetJobModifyFilter(filter);
        }

        public void SetNodeQueryCacheInvalidNotification(NodeQueryCacheInvalidNotification handler)
        {
            if (!StoreInProc)
            {
                throw new InvalidOperationException("NodeQueryCacheInvalidNotification could only be set at server side");
            }

            RemoteServer.SetNodeQueryCacheInvalidNotification(handler);
        }

        public void SetUserNamePassword(string userName, byte[] password)
        {
            RemoteServer.SetUserNamePassword(this.Token, userName, password);
        }

        public int ExpandParametricSweepTasksInBatch(int taskId, int maxExpand, TaskState expansionState)
        {
            return _storeServer.ExpandParametricSweepTasksInBatch(taskId, maxExpand, expansionState);
        }

        public string GetActiveHeadNodeName()
        {
            return _storeServer.Node_GetActiveHeadNodeName();
        }

        public async Task<string> GetSchedulerNodeNameAsync()
        {
            return await this.context.ResolveSchedulerNodeAsync(this.token).ConfigureAwait(false);
        }

        public void CreateDeployment(string deploymentId, StoreProperty[] props)
        {
            ServerWrapper.SchedulerAzureBurst_CreateDeployment(deploymentId, props);
        }

        public void DeleteDeployment(string deploymentId)
        {
            ServerWrapper.SchedulerAzureBurst_DeleteDeployment(deploymentId);
        }

        public async Task<string> PeekTaskOutputAsync(int jobId, int taskId)
        {
            return await RemoteServer.PeekTaskOutput(this.Token, jobId, taskId).ConfigureAwait(false);
        }

        #region Version control

        VersionControl _clientVersion = null;
        VersionControl _serverVersion = null;

        public VersionControl ClientVersion
        {
            [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
            get
            {
                if (_clientVersion == null)
                {
                    Assembly myAss = Assembly.GetExecutingAssembly();

                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(myAss.Location);

                    _clientVersion = new VersionControl(new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart));
                }

                return _clientVersion;
            }
        }

        public VersionControl ServerVersion
        {
            get
            {
                // If nobody sets the server version, that means the client is 
                // running inside the scheduler, so the server version is the same
                // with client version.
                if (_serverVersion == null)
                {
                    _serverVersion = ClientVersion;
                }

                return _serverVersion;
            }
            set { _serverVersion = value; }
        }

        #endregion
    }
}
