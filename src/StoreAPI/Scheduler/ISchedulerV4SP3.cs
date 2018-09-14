//------------------------------------------------------------------------------
// <copyright file="AuthenticationUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      IScheduler as extended for v4sp1.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Properties;

    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISchedulerV4SP3)]
    public interface ISchedulerV4SP3 : IDisposable
    {
        #region V2 scheduler methods/props. Don't modify this

        /// <summary>
        /// This method is used to connect this instance to a cluster.
        /// </summary>
        /// <param name="cluster">Network name of the cluster to connect to.</param>
        void Connect(string cluster);

        /// <summary>
        /// Returns the privilege level of the user that is connected under this instance.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Deprecated in HPC Pack 2012 with Service Pack 1 (SP1).  Use the GetUserRoles() method.")]
        UserPrivilege GetUserPrivilege();

        /// <summary>
        /// Set the mode that this instance of the scheduler is using. 
        /// </summary>
        /// <param name="isConsole">
        /// If True the scheduler should use the console to prompt for credentials, and the <paramref name="hWnd"/> parameter will be ignored.
        /// </param>
        /// <param name="hwnd">
        /// Win32 Handle of the window that the credential dialog should be parented to.  If set to -1 and <paramref name="isConsole"/> is false,
        /// this method will attempt to authenticate the user via cached credentials, and will throw an Exception if the authentication fails.        
        /// </param>
        void SetInterfaceMode(bool isConsole, IntPtr hwnd);

        /// <summary>
        /// Returns the version information of the server.
        /// </summary>
        /// <returns></returns>
        IServerVersion GetServerVersion();

        /// <summary>
        /// Create a new empty job that uses the default profile.
        /// </summary>
        /// <returns></returns>
        ISchedulerJob CreateJob();

        /// <summary>
        /// Returns an existing ISchedulerJob object from the server.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ISchedulerJob OpenJob(int id);

        /// <summary>
        /// Creates a new job that uses the specified job object to populate
        /// the initial settings.
        /// </summary>
        /// <param name="jobId">ID of the job to clone</param>
        /// <returns>New job object</returns>
        ISchedulerJob CloneJob(int jobId);

        /// <summary>
        /// Adds a job object to the scheduler store without submitting the job
        /// to the scheduler.
        /// </summary>
        /// <param name="job">Job to add to the server</param>
        void AddJob(ISchedulerJob job);

        /// <summary>
        /// Submits a job to the scheduler. 
        /// </summary>
        /// <param name="job">ISchedulerJob object that contains the job to be submitted</param>
        /// <param name="username">Account that the job is to run as.  Can be null</param>
        /// <param name="password">Password that the job is to use.  Can be null</param>
        void SubmitJob(ISchedulerJob job, string username, string password);

        /// <summary>
        /// Submits a job to the scheduler. 
        /// </summary>
        /// <param name="jobId">Id of the job to be submitted</param>
        /// <param name="username">Account that the job is to run as.  Can be null</param>
        /// <param name="password">Password that the job is to use.  Can be null</param>
        void SubmitJobById(int jobId, string username, string password);

        /// <summary>
        /// Cancels an existing job on the cluster.
        /// </summary>
        /// <param name="jobId">ID of the job to be canceled</param>
        /// <param name="message">Message to be saved on the job object once the job has been canceled.  Can be null</param>
        void CancelJob(int jobId, string message);

        /// <summary>
        /// Will attempt to move a job that is not in the configuring state back to configuring.
        /// This enables changes to be made to the job.
        /// </summary>
        /// <param name="jobId">ID of the job to move back to configuring</param>
        void ConfigureJob(int jobId);

        /// <summary>
        /// Returns a filtered and sorted collection of ISchedulerJob objects
        /// </summary>
        /// <param name="filter">Filter to use when enumerating. Can be null if no filter is required.</param>
        /// <param name="sort">Sort to use when enumerating. Can be null if no sorting is required.</param>
        /// <returns></returns>
        ISchedulerCollection GetJobList(IFilterCollection filter, ISortCollection sort);

        /// <summary>
        /// Returns a filtered and sorted collection of Job ID's.
        /// </summary>
        /// <param name="filter">Filter to use when enumerating. Can be null if no filter is required.</param>
        /// <param name="sort">Sort to use when enumerating. Can be null if no sorting is required.</param>
        /// <returns></returns>
        IIntCollection GetJobIdList(IFilterCollection filter, ISortCollection sort);

        /// <summary>
        /// Returns a filtered and sorted collection of ISchedulerNode objects
        /// </summary>
        /// <param name="filter">Filter to use when enumerating. Can be null if no filter is required.</param>
        /// <param name="sort">Sort to use when enumerating. Can be null if no sorting is required.</param>
        /// <returns></returns>
        ISchedulerCollection GetNodeList(IFilterCollection filter, ISortCollection sort);

        /// <summary>
        /// Returns a filtered and sorted collection of ISchedulerNode objects
        /// </summary>
        /// <param name="filter">Filter to use when enumerating. Can be null if no filter is required.</param>
        /// <param name="sort">Sort to use when enumerating. Can be null if no sorting is required.</param>
        /// <returns></returns>
        IIntCollection GetNodeIdList(IFilterCollection filter, ISortCollection sort);

        /// <summary>
        /// Opens an ISchedulerNode object for the specified node ID.
        /// </summary>
        /// <param name="nodeId">Scheduler NodeID</param>
        /// <returns></returns>
        ISchedulerNode OpenNode(int nodeId);

        /// <summary>
        /// Opens an ISchedulerNode object for the specified node name.
        /// </summary>
        /// <param name="nodeName">NETBIOS name of the node to open</param>
        /// <returns></returns>
        ISchedulerNode OpenNodeByName(string nodeName);

        /// <summary>
        /// Creates an ITaskId object with the specified Job Task Id
        /// </summary>
        /// <param name="jobTaskId">Numeric ID for the Task</param>
        /// <returns></returns>
        ITaskId CreateTaskId(Int32 jobTaskId);

        /// <summary>
        /// Creates a parametric ITaskId object.
        /// </summary>
        /// <param name="jobTaskId">Numeric ID for the Task</param>
        /// <param name="instanceId">Instance ID of the Task</param>
        /// <returns></returns>
        ITaskId CreateParametricTaskId(Int32 jobTaskId, Int32 instanceId);

        /// <summary>
        /// Add or update an cluster wide environment variable.  If the value is set to 'null'
        /// the variable will be removed from the cluster.
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Value for the variable</param>
        void SetEnvironmentVariable(string name, string value);

        /// <summary>
        /// Collection of the environment variables set for the cluster.
        /// </summary>
        INameValueCollection EnvironmentVariables { get; }

        /// <summary>
        /// Updates a cluster parameter.
        /// </summary>
        /// <param name="name">Name of the parameter to update.</param>
        /// <param name="value">Value of the parameter to set.</param>
        void SetClusterParameter(string name, string value);

        /// <summary>
        /// Collection of cluster parameters.
        /// </summary>
        INameValueCollection ClusterParameters { get; }

        /// <summary>
        /// Returns a collection of all the defined Job Templates names on the cluster.
        /// </summary>
        /// <returns></returns>
        IStringCollection GetJobTemplateList();

        /// <summary>
        /// Returns a collection of all the defined Node Group names on the cluster.
        /// </summary>
        /// <returns></returns>
        IStringCollection GetNodeGroupList();

        /// <summary>
        /// Returns the names of Nodes that are contained within a Node Group.
        /// </summary>
        /// <param name="nodeGroup">Name of the Node Group to enumerate</param>
        /// <returns></returns>
        IStringCollection GetNodesInNodeGroup(string nodeGroup);

        /// <summary>
        /// Creates a INameValueCollection object. The collection is initially empty.
        /// </summary>
        /// <returns></returns>
        INameValueCollection CreateNameValueCollection();

        /// <summary>
        /// Creates a IFilterCollection object. The collection is initially empty.
        /// </summary>
        /// <returns></returns>
        IFilterCollection CreateFilterCollection();

        /// <summary>
        /// Creates a ISortCollection object. The collection is initially empty.
        /// </summary>
        /// <returns></returns>
        ISortCollection CreateSortCollection();

        /// <summary>
        /// Creates a IStringCollection object. The collection is initially empty.
        /// </summary>
        /// <returns></returns>
        IStringCollection CreateStringCollection();

        /// <summary>
        /// Creates a IIntCollection object. The collection is initially empty.
        /// </summary>
        /// <returns></returns>
        IIntCollection CreateIntCollection();

        /// <summary>
        /// Creates a ICommandInfo object.
        /// </summary>
        /// <param name="envs"></param>
        /// <param name="workDir"></param>
        /// <param name="stdIn"></param>
        /// <returns></returns>
        ICommandInfo CreateCommandInfo(INameValueCollection envs, string workDir, string stdIn);

        /// <summary>
        /// Create a remote command
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="info"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        IRemoteCommand CreateCommand(string commandLine, ICommandInfo info, IStringCollection nodes);

        /// <summary>
        /// Returns global counters for the cluster.
        /// </summary>
        /// <returns></returns>
        ISchedulerCounters GetCounters();

        /// <summary>
        /// Opens a Row Enumerator of Jobs on the cluster.
        /// </summary>
        /// <param name="properties">Collection of Job Properties to return</param>
        /// <param name="filter">Filter to use when enumerating. Can be null if no filter is required.</param>
        /// <param name="sort">Sort to use when enumerating. Can be null if no sorting is required.</param>
        /// <returns></returns>
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenJobEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        /// Opens a Rowset of Jobs on the cluster.
        /// </summary>
        /// <param name="properties">Collection of Job Properties to return</param>
        /// <param name="filter">Filter to use when enumerating. Can be null if no filter is required.</param>
        /// <param name="sort">Sort to use when enumerating. Can be null if no sorting is required.</param>
        /// <returns></returns>
        [ComVisible(false)]
        ISchedulerRowSet OpenJobRowSet(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        /// Opens a Row Enumerator of Job History records on the cluster.
        /// </summary>
        /// <param name="properties">Collection of Job History Properties to return</param>
        /// <param name="filter">Filter to use when enumerating. Can be null if no filter is required.</param>
        /// <param name="sort">Sort to use when enumerating. Can be null if no sorting is required.</param>
        /// <returns></returns>
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenJobHistoryEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        /// Opens a Row Enumerator of Node History records on the cluster.
        /// </summary>
        /// <param name="properties">Collection of Node History Properties to return</param>
        /// <param name="filter">Filter to use when enumerating. Can be null if no filter is required.</param>
        /// <param name="sort">Sort to use when enumerating. Can be null if no sorting is required.</param>
        /// <returns></returns>
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenNodeHistoryEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        /// Opens a Row Enumerator of Nodes on the cluster.
        /// </summary>
        /// <param name="properties">Collection of Node Properties to return</param>
        /// <param name="filter">Filter to use when enumerating. Can be null if no filter is required.</param>
        /// <param name="sort">Sort to use when enumerating. Can be null if no sorting is required.</param>
        /// <returns></returns>
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenNodeEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        /// Deletes all cluster credentials cached for the specified user 
        /// </summary>
        /// <param name="userName">Supplies the userName whose credentials should be
        /// deleted. If null or empty, all users credentials will be deleted.
        /// </param>        
        void DeleteCachedCredentials(string userName);

        /// <summary>
        /// Sets cluster credentials cache for the specified user. SetConsole controls prompting for credentials
        /// if either userName or password is null or empty or if there is a problem setting the credentials
        /// </summary>
        /// <param name="userName">Supplies the userName whose credentials should be set.</param>
        /// <param name="password">Supplies the required credentials</param>
        void SetCachedCredentials(string userName, string password);

        /// <summary>
        /// Close the scheduler connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Returns an XmlReader for the XML exported from the designated job template.
        /// </summary>
        /// <param name="jobTemplateName">The name of the job template to be read.</param>
        /// <returns></returns>
        [ComVisible(false)]
        XmlReader GetJobTemplateXml(string jobTemplateName);

        #endregion

        #region V3 scheduler methods/props. Don't modify this
        /// <summary>
        /// Get the job template information
        /// </summary>
        /// <param name="jobTemplateName">The name of the job template</param>
        /// <returns></returns>
        JobTemplateInfo GetJobTemplateInfo(string jobTemplateName);

        /// <summary>
        /// Cancel a job with the specified id, save the message as the cancel message
        /// If the isForced parameter is set to true, cancel the job without running the 
        /// release tasks or allowing the tasks any grace period
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="message"></param>
        /// <param name="isForced"></param>
        void CancelJob(int jobId, string message, bool isForced);

        /// <summary>
        /// Event handler that is called when there is a reconnection on the scheduler.
        /// </summary>
        event EventHandler<ConnectionEventArg> OnSchedulerReconnect;
        #endregion

        #region V3 SP2 scheduler methods / props. Don't modify this

        /// <summary>
        /// Create a pool with this name if one does not exist
        /// If one already exists throw an exception
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        ISchedulerPool CreatePool(string poolName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="poolWeight"></param>
        /// <returns></returns>
        ISchedulerPool CreatePool(string poolName, int poolWeight);

        /// <summary>
        /// Open a pool object for a pool of this name
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        ISchedulerPool OpenPool(string poolName);

        /// <summary>
        /// Delete a pool with the given name if one exists
        /// If it doesn't throw an exception
        /// </summary>
        /// <param name="poolName"></param>
        void DeletePool(string poolName);

        /// <summary>
        /// Delete a pool with the given name if one exists
        /// If it doesn't throw an exception
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="force"></param>
        void DeletePool(string poolName, bool force);

        [ComVisible(false)]
        ISchedulerRowSet OpenPoolRowSet(IPropertyIdCollection properties);

        /// <summary>
        /// Get a list of all pools on the cluster
        /// </summary>
        /// <returns></returns>
        ISchedulerCollection GetPoolList();

        /// <summary>
        /// Upload a certificate in the client's certificate store to the scheduler to use for running jobs as this user
        /// </summary>
        /// <param name="userName">if it is null or empty, the owner is treated as user. Only SYSTEM on headnode is allowed to submit credentials
        /// as another user. So all other users should submit with username=null or empty or set to the same user as the owner</param>
        /// <param name="thumbprint">The thumbprint of the certificate to upload. If it is null, 
        /// the store will be searched for relevant certificates and if there are multiple the user will be prompted to choose a password depending on interface mode.</param>
        void SetCertificateCredentials(string userName, string thumbprint);

        /// <summary>
        /// Set this certificate encoded with this password as this user's cred
        /// </summary>
        /// <param name="userName">If this is null, the current owner is treated as the user</param>
        /// <param name="pfxPassword"></param>
        /// <param name="certBytes"></param>
        void SetCertificateCredentialsPfx(string userName, string pfxPassword, byte[] certBytes);

        /// <summary>
        /// Get a certificate matching the thumbprint from the local store encoded as a stream of bytes 
        /// and also set the randomly generated password used to encrypt the password
        /// </summary>
        /// <param name="thumbprint">if null, we will look for any certificate matching the requirements. If there is one, we return it.
        /// If there are multiple, the user will be prompted to choose depending on interface mode</param>
        /// <param name="pfxPassword"></param>
        /// <returns></returns>
        byte[] GetCertificateFromStore(string thumbprint, out SecureString pfxPassword);

        /// <summary>
        /// Enroll the user in a certificate of this template
        /// </summary>
        /// <param name="templateName">As long as the scheduler specifies a hpcsoftcardtemplate, this argument is ignored. If however, the scheduler
        /// does not specify a template, we use this argument as the template name</param>
        /// <returns></returns>
        string EnrollCertificate(string templateName);


        /// <summary>
        /// Create a remote command which will not perform output redirection
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="info"></param>
        /// <param name="nodes"></param>
        /// <param name="redirectOutput"></param>
        /// <returns></returns>
        IRemoteCommand CreateCommand(string commandLine, ICommandInfo info, IStringCollection nodes, bool redirectOutput);

        #endregion

        #region V3 SP3 scheduler methods /props

        /// <summary>
        /// Validate an Azure user and its password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>True if the user exists and the password is correct, false otherwise.</returns>
        [ComVisible(false)]
        bool ValidateAzureUser(string username, string password);

        /// <summary>
        /// Connect to the scheduler with ServiceAsClient mode.
        /// </summary>
        /// <param name="cluster">The name of the cluster headnode.</param>
        /// <param name="identityProvider">A function that can provide the caller's identity.</param>
        [ComVisible(false)]
        void ConnectServiceAsClient(string cluster, ServiceAsClientIdentityProvider identityProvider);

        /// <summary>
        /// Connect to the scheduler with ServiceAsClient mode. Connect as the specified identity.
        /// </summary>
        /// <param name="cluster">The name of the cluster headnode.</param>
        /// <param name="identityProvider">A function that can provide the caller's identity.</param>
        /// <param name="userName">The name of the user to connect as.</param>
        /// <param name="password">The password of the user to connect as.</param>
        [ComVisible(false)]
        void ConnectServiceAsClient(string cluster, ServiceAsClientIdentityProvider identityProvider, string userName, string password);
        #endregion

        #region V3 SP4 scheduler methods /props

        /// <summary>
        /// Return machine name of the active head node if on-premise, or else return the role instance name if in Azure like HeadNode_IN_3
        /// </summary>
        /// <returns></returns>
        string GetActiveHeadNode();

        #endregion

        #region V4 scheduler methods /props

        /// <summary>
        /// Set email credential 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        void SetEmailCredentials(string userName, string password);

        /// <summary>
        /// Delete email credential 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        void DeleteEmailCredentials();

        #endregion

        #region V4SP1 scheduler methods / properties

        /// <summary>
        /// Returns the union of all Roles for the current identity.
        /// </summary>
        /// <returns></returns>
        UserRoles GetUserRoles();

        /// <summary>
        /// Requeue job
        /// </summary>
        /// <param name="jobId"></param>
        void RequeueJob(int jobId);

        #endregion

        #region V4SP3 scheduler methods / properties

        /// <summary>
        /// Cancel a job with the specified id
        /// </summary>
        /// <param name="jobId">the id of the job.</param>
        /// <param name="message">the cancel message.</param>
        /// <param name="isForced">true to cancel the job without running the release tasks or allowing the tasks any grace period</param>
        /// <param name="isGraceful">true to cancel the job and wait for running tasks end.</param>
        void CancelJob(int jobId, string message, bool isForced, bool isGraceful);

        /// <summary>
        /// Finish a job with the specified id
        /// </summary>
        /// <param name="jobId">the id of the job.</param>
        /// <param name="message">the finish message.</param>
        /// <param name="isForced">true to finish the job without running the release tasks or allowing the tasks any grace period</param>
        /// <param name="isGraceful">true to finish the job and wait for running tasks end.</param>
        void FinishJob(int jobId, string message, bool isForced, bool isGraceful);

        #endregion
    }
}


