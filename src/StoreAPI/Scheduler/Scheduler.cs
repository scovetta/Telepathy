namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Store;

    /// <summary>
    ///   <para>Defines the methods used to schedule and manage the jobs and tasks in a compute cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.IScheduler" /> interface. </para>
    /// </remarks>
    /// <example />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidSchedulerClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class Scheduler : IScheduler, ISchedulerV2, ISchedulerV3, ISchedulerV3SP2, ISchedulerV3SP3, ISchedulerV3SP4, ISchedulerV4, ISchedulerV4SP1, ISchedulerV4SP3, ISchedulerV5
    {
        bool _isDisposed;
        bool _isOwned;

        /// <summary>
        ///   <para>Specifies whether the calling application is a console or Windows application.</para>
        /// </summary>
        /// <param name="isConsole">
        ///   <para>Set to True if the calling application is a console application: otherwise, False.</para>
        /// </param>
        /// <param name="hwnd">
        ///   <para>The window handle to the parent window if the application is 
        /// a Windows application. The handled is ignored if <paramref name="isConsole" /> is True.</para>
        /// </param>
        public void SetInterfaceMode(bool isConsole, IntPtr hwnd)
        {
            SchedulerStore.SetInterfaceMode(isConsole, hwnd);
        }

        /// <summary>
        ///   <para>Gets the privilege level of the user.</para>
        /// </summary>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.UserPrivilege" /> enumeration value that specifies the privilege level of the user. For example, if the user is running as an administrator or a normal user.</para> 
        /// </returns>
        [Obsolete("Deprecated in HPC Pack 2012 with Service Pack 1 (SP1).  Use the GetUserRoles() method.")]
        public UserPrivilege GetUserPrivilege()
        {
            return Store.GetUserPrivilege();
        }

        ISchedulerStore _store = null;

        internal ISchedulerStore Store
        {
            get
            {
                if (_store == null)
                {
                    throw new SchedulerException(ErrorCode.Operation_NotConnectedToServer, "");
                }
                return _store;
            }
        }

        internal protected Scheduler(ISchedulerStore store)
        {
            _store = store;
        }

        /// <summary>
        ///   <para>Initializes of new instance of the <see cref="Microsoft.Hpc.Scheduler.Scheduler" /> class.</para>
        /// </summary>
        public Scheduler()
        {
        }

        /// <summary>
        ///   <para>Releases resources used by this instance.</para>
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        // The pattern used here for disposal comes from the .NET Framework Design Guide
        private void Dispose(bool disposing)
        {
            if ((null != _store) && (_isOwned == true))
            {
                _store.Dispose();
            }

            // Even it is being called from a finalizer, we still need 
            // to dispose the _store to shutdown threads

            // In all cases, null out interesting/large objects.  If we had any unmanaged
            // objects, we would free them here as well.
            _store = null;

            // This object is disposed and no longer usable.
            _isDisposed = true;
		}

        ~Scheduler()
        {
            this.Dispose(false);
        }

        /// <summary>
        ///   <para>Connects you to the specified cluster.</para>
        /// </summary>
        /// <param name="cluster">
        ///   <para>The computer name of the cluster's head node (the head node is the node on which Microsoft HPC 
        /// Service is installed). If your application is running on the head node, you can specify the node's name or use “localhost”.</para>
        /// </param>
        public void Connect(string cluster)
        {
            Connect(cluster, ConnectMethod.Undefined);
        }
        
        /// <summary>
        ///   <para>Retrieves the file version of the HPC server assembly.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IServerVersion" /> interface that contains the version information for the HPC server.</para>
        /// </returns>
        public IServerVersion GetServerVersion()
        {
            return new ServerVersion(Store.GetServerVersion());
        }

        /// <summary>
        ///   <para>Gets counter information for the cluster.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerCounters" /> interface that contains the counter data.</para>
        /// </returns>
        public ISchedulerCounters GetCounters()
        {
            SchedulerCounters counters = new SchedulerCounters();

            counters.Init(Store);

            return counters;
        }

        /// <summary>
        ///   <para>Retrieves a node object using the specified node name.</para>
        /// </summary>
        /// <param name="nodeName">
        ///   <para>The name of the node to open.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode" /> interface that you can use to retrieve information about the node.</para>
        /// </returns>
        public ISchedulerNode OpenNodeByName(string nodeName)
        {
            Util.CheckArgumentNullOrEmpty(nodeName, "nodeName");

            TraceHelper.TraceVerbose("Scheduler.OpenNodeByName: {0}", nodeName);

            IClusterNode node = Store.OpenNode(nodeName);

            SchedulerNode localNode = new SchedulerNode(Store);

            localNode.Init(node, null);

            return localNode;
        }

        /// <summary>
        ///   <para>Retrieves a node object using the specified node identifier.</para>
        /// </summary>
        /// <param name="nodeId">
        ///   <para>The identifier of the node to open.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode" /> interface that you can use to access the properties of the node.</para>
        /// </returns>
        public ISchedulerNode OpenNode(int nodeId)
        {
            TraceHelper.TraceVerbose("Scheduler.OpenNode: {0}", nodeId);

            IClusterNode node = Store.OpenNode(nodeId);

            SchedulerNode localNode = new SchedulerNode(Store);

            localNode.Init(node, null);

            return localNode;
        }

        /// <summary>
        ///   <para>Creates a task identifier that identifies a task.</para>
        /// </summary>
        /// <param name="jobTaskId">
        ///   <para>A sequential, numeric identifier that uniquely identifies the task within the job. Task identifiers begin with one.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" /> interface that identifies a task in a job.</para>
        /// </returns>
        public ITaskId CreateTaskId(Int32 jobTaskId)
        {
            TraceHelper.TraceInfo("Scheduler.CreateTaskId: {0}", jobTaskId);
            return new TaskId(jobTaskId);
        }

        /// <summary>
        ///   <para>Creates a task identifier that identifies an instance of a parametric task.</para>
        /// </summary>
        /// <param name="jobTaskId">
        ///   <para>A sequential, numeric identifier that uniquely identifies the parametric task within the job. Task identifiers begin with one.</para>
        /// </param>
        /// <param name="instanceId">
        ///   <para>A sequential, numeric identifier that uniquely identifies the instance of the parametric task.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" /> interface that identifies the parametric task instance.</para>
        /// </returns>
        public ITaskId CreateParametricTaskId(Int32 jobTaskId, Int32 instanceId)
        {
            TraceHelper.TraceInfo("Scheduler.CreateParametricTaskId, jobTaskId: {0}, instanceId: {1}", jobTaskId, instanceId);
            return new TaskId(jobTaskId, instanceId);
        }

        /// <summary>
        ///   <para>Creates a job.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface that defines the newly created job.</para>
        /// </returns>
        public ISchedulerJob CreateJob()
        {
            SchedulerJob job = new SchedulerJob(this);

            return job;
        }

        /// <summary>
        ///   <para>Clones the specified job.</para>
        /// </summary>
        /// <param name="jobId">
        ///   <para>The identifier of the job to clone.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface that defines the newly cloned job.</para>
        /// </returns>
        public ISchedulerJob CloneJob(int jobId)
        {
            TraceHelper.TraceInfo("Scheduler.CloneJob: jobId={0}", jobId);

            IClusterJob job = Store.OpenJob(jobId);
            IClusterJob jobClone = job.Clone();

            SchedulerJob resultJob = new SchedulerJob(this);
            resultJob.Init(jobClone);
            return resultJob;
        }

        /// <summary>
        ///   <para>Adds the specified job to the cluster.</para>
        /// </summary>
        /// <param name="jobNew">
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface of the job to add to the scheduler.</para>
        /// </param>
        public void AddJob(ISchedulerJob jobNew)
        {
            Util.CheckArgumentNull(jobNew, "jobNew");

            TraceHelper.TraceInfo("Scheduler.AddJob: jobId={0}", jobNew.Id);

            SchedulerJob job = (SchedulerJob)jobNew;

            job.CreateJob();
        }

        /// <summary>
        ///   <para>Retrieves the specified job from the scheduler.</para>
        /// </summary>
        /// <param name="id">
        ///   <para>Identifies the job to retrieve.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface that you can use to access the properties of the job.</para>
        /// </returns>
        public ISchedulerJob OpenJob(int id)
        {
            TraceHelper.TraceVerbose("Scheduler.OpenJob: jobId={0}", id);

            IClusterJob ijob = Store.OpenJob(id);

            SchedulerJob job = new SchedulerJob(this);

            job.Init(ijob);

            return job;
        }

        /// <summary>
        ///   <para>Adds a job to the scheduling queue using the job interface to identify the job.</para>
        /// </summary>
        /// <param name="job">
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface that identifies the job to submit to the scheduling queue.</para>
        /// </param>
        /// <param name="username">
        ///   <para>The name of the RunAs user, in the form domain\username. The user name is limited to 80 characters.</para>
        ///   <para>If this parameter is null, empty, or not valid, the service searches the credential cache for the credentials to use. If the cache 
        /// contains the credentials for a single user, those credentials are used. However, 
        /// if multiple credentials exist in the cache, the user is prompted for the credentials.</para> 
        /// </param>
        /// <param name="password">
        ///   <para>The password for the RunAs user. The password is limited to 127 characters.</para>
        ///   <para>If this parameter is null or empty, the method uses 
        /// the cached password if cached; otherwise, the user is prompted for the password.</para>
        /// </param>
        public void SubmitJob(ISchedulerJob job, string username, string password)
        {
            Util.CheckArgumentNull(job, "job");

            TraceHelper.TraceInfo("Scheduler.SubmitJob: jobId={0}", job.Id);

            SchedulerJob jobInt = (SchedulerJob)job;

            jobInt.Submit(Store, username, password);
        }

        /// <summary>
        ///   <para>Adds the job to the scheduling queue using the job identifier to identify the job.</para>
        /// </summary>
        /// <param name="jobId">
        ///   <para>An identifier that uniquely identifies the job in the cluster to add to the scheduling queue.</para>
        /// </param>
        /// <param name="username">
        ///   <para>The name of the RunAs user, in the form domain\username. The user name is limited to 80 characters.</para>
        ///   <para>If this parameter is null, empty, or not valid, the service searches the credential cache for the credentials to use. If the cache 
        /// contains the credentials for a single user, those credentials are used. However, 
        /// if multiple credentials exist in the cache, the user is prompted for the credentials.</para> 
        /// </param>
        /// <param name="password">
        ///   <para>The password for the RunAs user. The password is limited to 127 characters.</para>
        ///   <para>If this parameter is null or empty, the method uses 
        /// the cached password if cached; otherwise, the user is prompted for the password.</para>
        /// </param>
        public void SubmitJobById(int jobId, string username, string password)
        {
            TraceHelper.TraceInfo("Scheduler.SubmitJob: jobId={0}", jobId);

            IClusterJob job = Store.OpenJob(jobId);

            List<StoreProperty> props = new List<StoreProperty>();

            if (username != null)
            {
                props.Add(new StoreProperty(JobPropertyIds.UserName, username));
            }

            if (password != null)
            {
                props.Add(new StoreProperty(JobPropertyIds.Password, password));
            }

            job.Submit(props.ToArray());
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        /// <param name="message">
        ///   <para />
        /// </param>
        public void CancelJob(int jobId, string message)
        {
            CancelJob(jobId, message, false);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        /// <param name="message">
        ///   <para />
        /// </param>
        /// <param name="isForced">
        ///   <para />
        /// </param>
        public void CancelJob(int jobId, string message, bool isForced)
        {
            TraceHelper.TraceInfo("Scheduler.CancelJob: jobId={0}, message={1}, isForced={2}", jobId, message ?? string.Empty, isForced);

            Util.CheckArgumentNull(message, "message");

            IClusterJob job = Store.OpenJob(jobId);

            // We don't enable the graceful Cancel job for now.
            job.Cancel(message, isForced, false);
        }

        /// <summary>
        ///   <para>Moves the job to the Configuring state.</para>
        /// </summary>
        /// <param name="jobId">
        ///   <para>Identifies the job to move to the Configuring state. </para>
        /// </param>
        public void ConfigureJob(int jobId)
        {
            TraceHelper.TraceVerbose("Scheduler.ConfigureJob: jobId={0}", jobId);

            IClusterJob job = Store.OpenJob(jobId);
            job.Configure();
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        public void RequeueJob(int jobId)
        {
            TraceHelper.TraceInfo("Scheduler.RequeueJob: jobId={0}", jobId);

            IClusterJob job = Store.OpenJob(jobId);
            job.Requeue();
        }

        /// <summary>
        ///   <para>Sets a cluster-wide environment variable.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The name of the variable.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The value of the variable.</para>
        /// </param>
        public void SetEnvironmentVariable(string name, string value)
        {
            Util.CheckArgumentNull(name, "name");

            Store.SetClusterEnvironmentVariable(name, value);
        }

        /// <summary>
        ///   <para>Retrieves the cluster-wide environment variables.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains a collection of environment variables defined for the cluster.</para> 
        /// </value>
        public INameValueCollection EnvironmentVariables
        {
            get
            {
                return new NameValueCollection(Store.GetClusterEnvironmentVariables(), true);
            }
        }

        /// <summary>
        ///   <para>Sets a configuration parameter for the cluster.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The name of the parameter. The name is case-insensitive.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The value of the parameter.</para>
        /// </param>
        public void SetClusterParameter(string name, string value)
        {
            Util.CheckArgumentNullOrEmpty(name, "name");
            Util.CheckArgumentNull(value, "value");

            Store.OpenStoreManager().SetConfigurationSetting(name, value);
        }

        /// <summary>
        ///   <para>Retrieves the cluster's configuration parameters.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface that contains a collection of configuration parameters for the cluster.</para> 
        /// </value>
        public INameValueCollection ClusterParameters
        {
            get
            {
                return new NameValueCollection(Store.OpenStoreManager().GetConfigurationSettings(), true);
            }
        }

        /// <summary>
        ///   <para>Retrieves a list of job template names defined in the cluster.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of job template names.</para>
        /// </returns>
        public IStringCollection GetJobTemplateList()
        {
            List<string> templateNames = new List<string>();

            using (IRowEnumerator templates = Store.OpenProfileEnumerator())
            {
                templates.SetColumns(JobTemplatePropertyIds.Name);

                foreach (PropertyRow row in templates)
                {
                    StoreProperty prop = row[JobTemplatePropertyIds.Name];
                    if (prop != null)
                    {
                        templateNames.Add((string)prop.Value);
                    }
                }
            }

            return new StringCollection(templateNames);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobTemplateName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public XmlReader GetJobTemplateXml(string jobTemplateName)
        {
            Util.CheckArgumentNullOrEmpty(jobTemplateName, "jobTemplateName");

            MemoryStream stream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(stream);

            IClusterJobProfile jobTemplate = _store.OpenProfile(jobTemplateName);
            jobTemplate.PersistToXml(writer, XmlExportOptions.None);
            writer.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            return XmlReader.Create(stream, new XmlReaderSettings() { XmlResolver = null });
        }

        /// <summary>
        ///   <para>Retrieves a list of node group names defined in the cluster.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of node group names.</para>
        /// </returns>
        public IStringCollection GetNodeGroupList()
        {
            return new StringCollection(Store.GetNodeGroups().Select(n => n.Name));
        }

        /// <summary>
        ///   <para>Retrieves the list of nodes in the specified node group.</para>
        /// </summary>
        /// <param name="nodeGroup">
        ///   <para>A node group name.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains the collection of node names in the group.</para>
        /// </returns>
        /// <remarks>
        ///   <para>To retrieve the available node group names, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetNodeGroupList" /> method.</para>
        /// </remarks>
        public IStringCollection GetNodesInNodeGroup(string nodeGroup)
        {
            return new StringCollection(Store.GetNodesFromGroup(nodeGroup));
        }

        /// <summary>
        ///   <para>Retrieves a list of jobs based on the specified filters.</para>
        /// </summary>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of jobs. If null, the method returns all jobs.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of jobs. If null, the list is not sorted.</para> 
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerCollection" /> interface that contains a collection of jobs.</para>
        /// </returns>
        public ISchedulerCollection GetJobList(IFilterCollection filter, ISortCollection sort)
        {
            List<ISchedulerJob> result = new List<ISchedulerJob>();

            using (IRowEnumerator rowenum = Store.OpenJobEnumerator())
            {
                if (filter != null)
                {
                    rowenum.SetFilter(filter.GetFilters());
                }

                if (sort != null)
                {
                    rowenum.SetSortOrder(sort.GetSorts());
                }

                rowenum.SetColumns(SchedulerJob.EnumInitIds);

                foreach (PropertyRow row in rowenum)
                {
                    result.Add(new SchedulerJob(this, row));
                }
            }

            return new SchedulerCollection<ISchedulerJob>(result);
        }

        /// <summary>
        ///   <para>Retrieves a list of job identifiers based on the specified filters.</para>
        /// </summary>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of jobs. If null, the method returns all jobs.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of jobs. If null, the list is not sorted.</para> 
        /// </param>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IIntCollection" /> interface that contains one or more job identifiers that match the specified filter criteria.</para> 
        /// </returns>
        public IIntCollection GetJobIdList(IFilterCollection filter, ISortCollection sort)
        {
            using (IRowSet rowset = Store.OpenJobRowSet(RowSetType.Snapshot))
            {
                return _GetIdList(rowset, filter, sort);
            }
        }

        /// <summary>
        ///   <para>Retrieves a list of node identifiers based on the specified filters.</para>
        /// </summary>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of nodes. If null, the method returns all nodes.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of nodes. If null, the list is not sorted.</para> 
        /// </param>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IIntCollection" />  interface that contains one or more node identifiers that match the specified filter criteria.</para> 
        /// </returns>
        public IIntCollection GetNodeIdList(IFilterCollection filter, ISortCollection sort)
        {
            using (IRowSet rowset = Store.OpenNodeRowSet(RowSetType.Snapshot))
            {
                return _GetIdList(rowset, filter, sort);
            }
        }

        /// <summary>
        ///   <para>Retrieves a list of nodes based on the specified filters.</para>
        /// </summary>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of nodes. If null, the method returns all nodes.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of nodes. If null, the list is not sorted.</para> 
        /// </param>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCollection" /> interface that contains one or more node objects that match the specified filter criteria.</para> 
        /// </returns>
        public ISchedulerCollection GetNodeList(IFilterCollection filter, ISortCollection sort)
        {
            List<ISchedulerNode> result = new List<ISchedulerNode>();

            using (IRowEnumerator rowenum = Store.OpenNodeEnumerator())
            {
                if (filter != null)
                {
                    rowenum.SetFilter(filter.GetFilters());
                }

                if (sort != null)
                {
                    rowenum.SetSortOrder(sort.GetSorts());
                }

                rowenum.SetColumns(SchedulerNode.EnumInitIds);

                foreach (PropertyRow row in rowenum)
                {
                    result.Add(new SchedulerNode(Store, row));
                }
            }

            return new SchedulerCollection<ISchedulerNode>(result);
        }

        /// <summary>
        ///   <para>Creates an empty collection to which you add filter properties.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface to which you add filter properties.</para>
        /// </returns>
        public IFilterCollection CreateFilterCollection()
        {
            return new FilterCollection();
        }

        /// <summary>
        ///   <para>Creates an empty collection to which you add sort properties.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface to which you add sort properties.</para>
        /// </returns>
        public ISortCollection CreateSortCollection()
        {
            return new SortCollection();
        }

        /// <summary>
        ///   <para>Creates an empty collection to which you add string values.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface to which you add string values.</para>
        /// </returns>
        public IStringCollection CreateStringCollection()
        {
            return new StringCollection();
        }

        /// <summary>
        ///   <para>Creates an empty collection to which you add integer values.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IIntCollection" /> interface to which you add integer values.</para>
        /// </returns>
        public IIntCollection CreateIntCollection()
        {
            return new IntCollection();
        }

        /// <summary>
        ///   <para>Creates an empty collection to which you can add name/value pairs.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface to which you can add name/value pairs.</para>
        /// </returns>
        public INameValueCollection CreateNameValueCollection()
        {
            return new NameValueCollection();
        }

        /// <summary>
        ///   <para>Creates an object that you can use to provide additional property values to a command.</para>
        /// </summary>
        /// <param name="envs">
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> that contains a collection of environment variables used by the command.</para>
        /// </param>
        /// <param name="workDir">
        ///   <para>The full path to the startup directory for the command.</para>
        /// </param>
        /// <param name="stdIn">
        ///   <para>The full path to the standard input file used by the command.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ICommandInfo" /> interface that defines the property values used by the command.</para>
        /// </returns>
        public ICommandInfo CreateCommandInfo(INameValueCollection envs, string workDir, string stdIn)
        {
            return new CommandInfo(envs, workDir, stdIn);
        }

#pragma warning disable 618 // disable obsolete warnings (for UserPrivilege)

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="commandLine">
        ///   <para />
        /// </param>
        /// <param name="info">
        ///   <para />
        /// </param>
        /// <param name="nodes">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public IRemoteCommand CreateCommand(string commandLine, ICommandInfo info, IStringCollection nodes)
        {
            Util.CheckArgumentNullOrEmpty(commandLine, "commandLine");

            if (Store.GetUserPrivilege() != UserPrivilege.Admin)
            {
                throw new SchedulerException(ErrorCode.Operation_PermissionDenied, string.Empty);
            }

            return new RemoteCommand(this, commandLine, info, nodes);
        }


        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="commandLine">
        ///   <para />
        /// </param>
        /// <param name="info">
        ///   <para />
        /// </param>
        /// <param name="nodes">
        ///   <para />
        /// </param>
        /// <param name="redirectOutput">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public IRemoteCommand CreateCommand(string commandLine, ICommandInfo info, IStringCollection nodes, bool redirectOutput)
        {
            Util.CheckArgumentNullOrEmpty(commandLine, "commandLine");

            if (Store.GetUserPrivilege() != UserPrivilege.Admin)
            {
                throw new SchedulerException(ErrorCode.Operation_PermissionDenied, string.Empty);
            }

            return new RemoteCommand(this, commandLine, info, nodes, redirectOutput);
        }

#pragma warning restore 618 // disable obsolete warnings (for UserPrivilege)

        internal static IIntCollection _GetIdList(IRowSet rowset, IFilterCollection filter, ISortCollection sort)
        {
            rowset.SetColumns(JobPropertyIds.Id);

            if (filter != null)
            {
                rowset.SetFilter(filter.GetFilters());
            }

            if (sort != null)
            {
                rowset.SetSortOrder(sort.GetSorts());
            }

            int rowcount = rowset.GetCount();

            int[] result = new int[rowcount];

            int i = 0;

            foreach (PropertyRow row in rowset)
            {
                result[i] = (int)row[0].Value;
                ++i;
            }

            return new IntCollection(result);
        }

        /// <summary>
        ///   <para>Retrieves an enumerator that contains the jobs that match the filter criteria.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the job properties that you want to include for each job in the enumerator.</para> 
        /// </param>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of jobs. If null, the method returns all jobs.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of jobs. If null, the list is not sorted.</para> 
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator" /> interface that you use to enumerate the results.</para>
        /// </returns>
        [ComVisible(false)]
        public ISchedulerRowEnumerator OpenJobEnumerator(
                IPropertyIdCollection properties,
                IFilterCollection filter,
                ISortCollection sort
                )
        {
            IRowEnumerator rows = Store.OpenJobEnumerator();

            JobRowEnumerator result = new JobRowEnumerator();

            result.Init(rows, properties, filter, sort);

            return result;
        }

        /// <summary>
        ///   <para>Retrieves a rowset that contains the jobs that match the filter criteria.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the job properties that you want to include for each job in the enumerator.</para> 
        /// </param>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of jobs. If null, the method returns all jobs.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of jobs. If null, the list is not sorted.</para> 
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowSet" /> interface that you use to access the results.</para>
        /// </returns>
        [ComVisible(false)]
        public ISchedulerRowSet OpenJobRowSet(
                IPropertyIdCollection properties,
                IFilterCollection filter,
                ISortCollection sort
                )
        {
            JobRowSet result = new JobRowSet();

            result.Init(Store.OpenJobRowSet(RowSetType.Snapshot), properties, filter, sort);

            return result;
        }

        /// <summary>
        ///   <para>Retrieves an enumerator that contains the job history information for jobs that match the filter criteria.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the job history properties that you want to include for each history record in the enumerator.</para> 
        /// </param>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of job history records. If null, the method returns all job history.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of job history records. If null, the list is not sorted.</para> 
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator" /> interface that you use to enumerate the results.</para>
        /// </returns>
        [ComVisible(false)]
        public ISchedulerRowEnumerator OpenJobHistoryEnumerator(
                IPropertyIdCollection properties,
                IFilterCollection filter,
                ISortCollection sort
                )
        {
            IRowEnumerator rows = Store.OpenStoreManager().OpenJobHistoryEnumerator();

            JobHistoryEnumerator result = new JobHistoryEnumerator();

            result.Init(rows, properties, filter, sort);

            return result;
        }

        /// <summary>
        ///   <para>Retrieves an enumerator that contains the node history for the nodes in the cluster.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the node history properties that you want to include for each history record in the enumerator.</para> 
        /// </param>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of node history records. If null, the method returns all node history.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of node history records. If null, the list is not sorted.</para> 
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator" /> interface that you use to enumerate the results.</para>
        /// </returns>
        [ComVisible(false)]
        public ISchedulerRowEnumerator OpenNodeHistoryEnumerator(
                IPropertyIdCollection properties,
                IFilterCollection filter,
                ISortCollection sort
                )
        {
            IRowEnumerator rows = Store.OpenStoreManager().OpenNodeHistoryEnumerator();

            NodeHistoryEnumerator result = new NodeHistoryEnumerator();

            result.Init(rows, properties, filter, sort);

            return result;
        }

        /// <summary>
        ///   <para>Retrieves a rowset enumerator that contains the nodes in the cluster that match the filter criteria.</para>
        /// </summary>
        /// <param name="properties">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface that contains a collection of the node properties that you want to include for each node in the enumerator.</para> 
        /// </param>
        /// <param name="filter">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface that contains one or more filter properties used to filter the list of nodes. If null, the method returns all nodes.</para> 
        /// </param>
        /// <param name="sort">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface that contains one or more sort properties used to sort the list of nodes. If null, the list is not sorted.</para> 
        /// </param>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator" /> interface that contains an enumerator of the nodes that match the specified filter criteria.</para> 
        /// </returns>
        [ComVisible(false)]
        public ISchedulerRowEnumerator OpenNodeEnumerator(
                IPropertyIdCollection properties,
                IFilterCollection filter,
                ISortCollection sort
                )
        {
            IRowEnumerator rows = Store.OpenNodeEnumerator();

            NodeEnumerator result = new NodeEnumerator();

            result.Init(rows, properties, filter, sort);

            return result;
        }

        /// <summary>
        ///   <para>Deletes the credentials that were cached for the specified user.</para>
        /// </summary>
        /// <param name="userName">
        ///   <para>The name of the RunAs user, in the form domain\user. The user name is limited to 80 
        /// characters. If this parameter is NULL or empty, the method deletes all credentials that have been cached by the calling user.</para>
        /// </param>
        public void DeleteCachedCredentials(string userName)
        {
            Store.PurgeCredentials(userName);
        }

        /// <summary>
        ///   <para>Sets the credentials for the specified user in the credential 
        /// cache, so that the job scheduler can use the credentials for submitting jobs.</para>
        /// </summary>
        /// <param name="userName">
        ///   <para>A 
        /// 
        /// <see cref="System.String" /> object that specifies the name of the user for which you want the set the cached credentials, in the form domain\user. The user name is limited to 80 characters.</para> 
        /// </param>
        /// <param name="password">
        ///   <para>A <see cref="System.String" /> object that specifies the password for the user for which you want the set the cached credentials.</para>
        /// </param>
        public void SetCachedCredentials(string userName, string password)
        {
            Store.SetCachedCredentials(userName, password);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        public void Close()
        {
            TraceHelper.TraceVerbose("Scheduler.Close: cluster:{0}", ClusterName);

            this.Dispose();
        }

        #region ISchedulerV3 Members

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobTemplateName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public JobTemplateInfo GetJobTemplateInfo(string jobTemplateName)
        {
            int id = 0;
            string name = null;


            IClusterJobProfile template = Store.OpenProfile(jobTemplateName);
            id = template.Id;
            name = jobTemplateName;

            PropertyRow row = template.GetProps(
                JobTemplatePropertyIds.SecurityDescriptor,  //0
                JobTemplatePropertyIds.Description,         //1
                JobTemplatePropertyIds.ChangeTime,          //2
                JobTemplatePropertyIds.CreateTime);         //3

            string securityDescriptor = PropertyUtil.GetValueFromPropRow<string>(row, JobTemplatePropertyIds.SecurityDescriptor, null);
            string description = PropertyUtil.GetValueFromPropRow<string>(row, JobTemplatePropertyIds.Description, null);
            DateTime changeTime = PropertyUtil.GetValueFromPropRow<DateTime>(row, JobTemplatePropertyIds.ChangeTime, DateTime.MinValue);
            DateTime createTime = PropertyUtil.GetValueFromPropRow<DateTime>(row, JobTemplatePropertyIds.CreateTime, DateTime.MinValue);

            JobTemplateInfo info = new JobTemplateInfo(
                id, name, securityDescriptor, description, changeTime, createTime);

            return info;
        }

        ReconnectHandler _onReconnect;

        object _eventLock = new object();

        /// <summary>
        /// <para></para>
        /// </summary>
        public event EventHandler<ConnectionEventArg> OnSchedulerReconnect
        {
            add
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect += (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect -= (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        public event ReconnectHandler OnReconnect
        {
            add
            {
                lock (_eventLock)
                {
                    if (_onReconnect == null)
                    {
                        // This is the first event, need
                        // to register with the store to
                        // get notifications.

                        _store.ConnectionEvent += ReconnectEventHandler;
                    }

                    _onReconnect += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    _onReconnect -= value;

                    if (_onReconnect == null)
                    {
                        // No longer need notifications
                        // from the store.

                        _store.ConnectionEvent -= ReconnectEventHandler;
                    }
                }
            }
        }

        void ReconnectEventHandler(object sender, SchedulerConnectionEventArgs args)
        {
            ReconnectHandler handler = _onReconnect;
            if (handler != null)
            {
                TraceHelper.TraceVerbose("Scheduler.ReconnectEventHandler: cluster:{0}", ClusterName);
                handler(this, new ConnectionEventArg((ConnectionEventCode)args.Code, args.Exception));
            }
        }

        #endregion

        #region V3sp2

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="poolName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public ISchedulerPool OpenPool(string poolName)
        {
            IClusterPool ipool = Store.OpenPool(poolName);

            return GetSchedulerPool(ipool);
        }


        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="poolName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public ISchedulerPool CreatePool(string poolName)
        {
            TraceHelper.TraceInfo("Scheduler.CreatePool: poolName={0}", poolName ?? string.Empty);

            IClusterPool ipool = Store.AddPool(poolName);

            return GetSchedulerPool(ipool);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="poolName">
        ///   <para />
        /// </param>
        /// <param name="poolWeight">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public ISchedulerPool CreatePool(string poolName, int poolWeight)
        {
            TraceHelper.TraceInfo("Scheduler.CreatePool: poolName={0}, poolWeight", poolName ?? string.Empty, poolWeight);

            IClusterPool ipool = Store.AddPool(poolName, poolWeight);

            return GetSchedulerPool(ipool);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="poolName">
        ///   <para />
        /// </param>
        public void DeletePool(string poolName)
        {
            TraceHelper.TraceVerbose("Scheduler.DeletePool: poolName={0}", poolName ?? string.Empty);

            Store.DeletePool(poolName);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="poolName">
        ///   <para />
        /// </param>
        /// <param name="force">
        ///   <para />
        /// </param>
        public void DeletePool(string poolName, bool force)
        {
            TraceHelper.TraceInfo("Scheduler.DeletePool: poolName={0}, force={1}", poolName ?? string.Empty, force);

            Store.DeletePool(poolName, force);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public ISchedulerRowSet OpenPoolRowSet(IPropertyIdCollection properties)
        {
            PoolRowSet result = new PoolRowSet();
            result.Init(Store.OpenPoolRowset(), properties, null, null);

            return result;
        }

        PropertyId[] poolPropsForList = { PoolPropertyIds.Id, PoolPropertyIds.PoolObject };
        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public ISchedulerCollection GetPoolList()
        {
            List<ISchedulerPool> poolList = new List<ISchedulerPool>();
            List<IClusterPool> clusterPools = new List<IClusterPool>();


            using (IRowSet rowSet = Store.OpenPoolRowset())
            {
                rowSet.SetColumns(poolPropsForList);


                foreach (PropertyRow row in rowSet)
                {
                    IClusterPool clusterPool = PropertyUtil.GetValueFromPropRow<IClusterPool>(row, PoolPropertyIds.PoolObject, null);

                    if (clusterPool != null)
                    {
                        clusterPools.Add(clusterPool);
                    }
                }
            }

            foreach (IClusterPool clusterPool in clusterPools)
            {
                poolList.Add(GetSchedulerPool(clusterPool));
            }


            return new SchedulerCollection<ISchedulerPool>(poolList);
        }

        private ISchedulerPool GetSchedulerPool(IClusterPool ipool)
        {
            SchedulerPool pool = new SchedulerPool(this.Store);

            pool.Init(ipool);

            return pool;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="userName">
        ///   <para />
        /// </param>
        /// <param name="thumbprint">
        ///   <para />
        /// </param>
        public void SetCertificateCredentials(string userName, string thumbprint)
        {
            byte[] certBytes = null;
            SecureString pfxPassword;

            certBytes = GetCertificateFromStore(thumbprint, out pfxPassword);

            Store.SaveUserCertificate(userName, pfxPassword, true, certBytes);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="userName">
        ///   <para />
        /// </param>
        /// <param name="pfxPassword">
        ///   <para />
        /// </param>
        /// <param name="certBytes">
        ///   <para />
        /// </param>
        public void SetCertificateCredentialsPfx(string userName, string pfxPassword, byte[] certBytes)
        {
            SecureString secPfxPassword = new SecureString();
            foreach (char c in pfxPassword.ToCharArray())
            {
                secPfxPassword.AppendChar(c);
            }
            Store.SaveUserCertificate(userName, secPfxPassword, true, certBytes);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="thumbprint">
        ///   <para />
        /// </param>
        /// <param name="pfxPassword">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public byte[] GetCertificateFromStore(string thumbprint, out SecureString pfxPassword)
        {
            byte[] certBytes = null;

            string[] settingsToLoad = { "HpcSoftCardTemplate" };
            List<string> settingsValues = null;
            Store.GetConfigSettingsValues(settingsToLoad, out settingsValues);

            string certificateTemplateName = settingsValues[0];

            //Find a certificate with this template name and thumbprint from the user's local store
            certBytes = PropertyUtil.GetCertFromStore(thumbprint, certificateTemplateName, out pfxPassword);
            if (certBytes == null)
            {
                throw new SchedulerException(ErrorCode.Operation_NoCertificateFoundOnClient, "");
            }

            return certBytes;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="templateName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public string EnrollCertificate(string templateName)
        {
            return Store.EnrollCertificate(templateName);
        }

        #endregion

        #region V3SP3

        /// <summary>
        ///   <para>Validates a Windows Azure account username and its associated password.</para>
        /// </summary>
        /// <param name="username">
        ///   <para>The username of the Windows Azure account.</para>
        /// </param>
        /// <param name="password">
        ///   <para>The password of the Windows Azure account.</para>
        /// </param>
        /// <returns>
        ///   <para>Returns true if the user exists and the password is correct; false otherwise.</para>
        /// </returns>
        public bool ValidateAzureUser(string username, string password)
        {
            return Store.ValidateAzureUser(username, password);
        }

        /// <summary>
        /// <para></para>
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="identityProvider"></param>
        public void ConnectServiceAsClient(string cluster, ServiceAsClientIdentityProvider identityProvider)
        {
            Util.CheckArgumentNullOrEmpty(cluster, "cluster");
            Util.CheckArgumentNull(identityProvider, "identityProvider");
            this.ConnectServiceAsClientInternalAsync(HpcContext.GetOrAdd(cluster, CancellationToken.None), identityProvider, null, null, ConnectMethod.Undefined).GetAwaiter().GetResult();
        }

        /// <summary>
        /// <para></para>
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="identityProvider"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public void ConnectServiceAsClient(string cluster, ServiceAsClientIdentityProvider identityProvider, string userName, string password)
        {
            Util.CheckArgumentNullOrEmpty(cluster, "cluster");
            Util.CheckArgumentNull(identityProvider, "identityProvider");
            this.ConnectServiceAsClientInternalAsync(HpcContext.GetOrAdd(cluster, CancellationToken.None), identityProvider, userName, password, ConnectMethod.Undefined).GetAwaiter().GetResult();
        }
        
        event EventHandler<ConnectionEventArg> ISchedulerV3SP3.OnSchedulerReconnect
        {
            add
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect += (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect -= (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }
        }

        static Dictionary<string, bool> _clusterServiceAsClientMapping = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

        #endregion

        #region V3SP4

        event EventHandler<ConnectionEventArg> ISchedulerV3SP4.OnSchedulerReconnect
        {
            add
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect += (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect -= (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para>Returns <see cref="System.String" />.</para>
        /// </returns>
        public string GetActiveHeadNode()
        {
            string name = Store.GetActiveHeadNodeName();
            return name;
        }

        #endregion

        #region V4

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="userName">
        ///   <para />
        /// </param>
        /// <param name="password">
        ///   <para />
        /// </param>
        public void SetEmailCredentials(string userName, string password)
        {
            Util.CheckArgumentNullOrEmpty(userName, "userName");

            Store.OpenStoreManager().SetEmailCredential(userName, password);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        public void DeleteEmailCredentials()
        {
            Store.OpenStoreManager().SetEmailCredential(null, null);
        }

        event EventHandler<ConnectionEventArg> ISchedulerV4.OnSchedulerReconnect
        {
            add
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect += (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect -= (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }
        }

        #endregion

        #region V4SP1

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public UserRoles GetUserRoles()
        {
            UserRoles roles = Store.GetUserRoles();

            return roles;
        }

        private string ClusterName
        {
            get
            {
                // Return empty string if _store or _store.Name is null
                return _store != null ? (_store.Name ?? string.Empty) : string.Empty;
            }
        }

        event EventHandler<ConnectionEventArg> ISchedulerV4SP1.OnSchedulerReconnect
        {
            add
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect += (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((Scheduler)this).OnReconnect -= (ReconnectHandler)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method);
                }
            }
        }

        #endregion //v4sp1

        #region V4SP3

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        /// <param name="message">
        ///   <para />
        /// </param>
        /// <param name="isForced">
        ///   <para />
        /// </param>
        /// <param name="isGraceful">
        ///   <para />
        /// </param>
        public void CancelJob(int jobId, string message, bool isForced, bool isGraceful)
        {
            TraceHelper.TraceInfo("Scheduler.CancelJob: jobId={0}, message={1}, isForced={2}, isGraceful={3}", jobId, message ?? string.Empty, isForced, isGraceful);

            Util.CheckArgumentNull(message, "message");

            IClusterJob job = Store.OpenJob(jobId);

            // We don't enable the graceful Cancel job for now.
            job.Cancel(message, isForced, isGraceful);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        /// <param name="message">
        ///   <para />
        /// </param>
        /// <param name="isForced">
        ///   <para />
        /// </param>
        /// <param name="isGraceful">
        ///   <para />
        /// </param>
        public void FinishJob(int jobId, string message, bool isForced, bool isGraceful)
        {
            TraceHelper.TraceInfo("Scheduler.CancelJob: jobId={0}, message={1}, isForced={2}, isGraceful={3}", jobId, message ?? string.Empty, isForced, isGraceful);

            Util.CheckArgumentNull(message, "message");

            IClusterJob job = Store.OpenJob(jobId);

            // We don't enable the graceful Cancel job for now.
            job.Finish(message, isForced, isGraceful);
        }

        #endregion

        #region V5

        /// <summary>
        /// <para></para>
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="method"></param>
        public void Connect(string cluster, ConnectMethod method)
        {
            var context = new SchedulerConnectionContext(cluster, CancellationToken.None);
            this.ConnectAsync(context, CancellationToken.None, method).Wait();
        }

        /// <summary>
        /// Connect using the new connection string format.
        /// </summary>
        /// <param name="connectionString">the connection string to the service fabric cluster.
        /// The format is like 'servername1:port1;servername2:port2;...'
        /// The list should be all seed nodes in the cluster with its clientConnectionEndpoint</param>
        /// <param name="port">the server port when isOverHttp == false, https port otherwise.</param>
        /// <param name="isOverHttp">true to connect using https, .net remoting otherwise</param>
        /// <param name="token">the cancellation token</param>
        public async Task ConnectAsync(SchedulerConnectionContext context, CancellationToken token, ConnectMethod method)
        {
            Util.CheckArgumentNull(context, "context");

            if (_store != null)
            {
                _store.Dispose();
            }

            _store = await SchedulerStore.ConnectAsync(context.Context, token, method).ConfigureAwait(false);
            _isOwned = true;
        }

        /// <summary>
        /// <para></para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task ConnectAsync(SchedulerConnectionContext context, CancellationToken token)
        {
            await ConnectAsync(context, token, ConnectMethod.Undefined);
        }

        /// <summary>
        /// <para></para>
        /// </summary>
        /// <param name="hpcContext"></param>
        /// <param name="identityProvider"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public async Task ConnectServiceAsClientAsync(IHpcContext hpcContext, ServiceAsClientIdentityProvider identityProvider, string userName, string password, ConnectMethod method)
        {
            Util.CheckArgumentNull(hpcContext, "hpcContext");
            Util.CheckArgumentNull(identityProvider, "identityProvider");
            await this.ConnectServiceAsClientInternalAsync(hpcContext, identityProvider, userName, password, method).ConfigureAwait(false);
        }

        /// <summary>
        /// <para></para>
        /// </summary>
        /// <param name="hpcContext"></param>
        /// <param name="identityProvider"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task ConnectServiceAsClientAsync(IHpcContext hpcContext, ServiceAsClientIdentityProvider identityProvider, string userName, string password)
        {
            await ConnectServiceAsClientAsync(hpcContext, identityProvider, userName, password, ConnectMethod.Undefined);
        }

        private async Task ConnectServiceAsClientInternalAsync(IHpcContext hpcContext, ServiceAsClientIdentityProvider identityProvider, string userName, string password, ConnectMethod method = ConnectMethod.Undefined)
        {
            string connectionString = hpcContext.GetConnectionString().ConnectionString;
            TraceHelper.TraceVerbose("Scheduler.ConnectServiceAsClient: connectionString={0}, user={1}", connectionString ?? string.Empty, userName ?? string.Empty);

            bool existServiceAsClient = true;

            if (_clusterServiceAsClientMapping.TryGetValue(connectionString, out existServiceAsClient))
            {
                if (!existServiceAsClient)
                {
                    // If there has been a connection to that cluster and was not service as client, 
                    // then we don't allow to create a service as client channel here
                    throw new InvalidOperationException(string.Format("There has been a connection from this client to server {0} with ServiceAsClient model disabled. Another connection with ServiceAsClient enabled is not allowed.", connectionString));
                }
            }

            _clusterServiceAsClientMapping[connectionString] = true;

            if (_store != null)
            {
                _store.Dispose();
            }

            _store = await SchedulerStore.ConnectAsync(new StoreConnectionContext(hpcContext) { IdentityProvider = identityProvider, UserName = userName, Password = password, ServiceAsClient = true }, hpcContext.CancellationToken, method).ConfigureAwait(false);
            _isOwned = true;
        }

        #endregion


        #region V5SP2

        /// <summary>
        /// <para>This method is used to delete a job.</para>
        /// </summary>
        /// <param name="jobId">
        /// <para>The id of the job.</para>
        /// </param>
        public void DeleteJob(int jobId)
        {
            TraceHelper.TraceInfo($"Scheduler.DeleteJob: jobId={jobId}");

            Store.DeleteJob(jobId);
        }

        #endregion
    }
}
