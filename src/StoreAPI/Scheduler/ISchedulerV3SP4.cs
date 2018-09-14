namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Properties;

    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISchedulerV3SP4)]
    public interface ISchedulerV3SP4 : IDisposable
    {
        #region V2 scheduler methods/props. Don't modify this

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="cluster">
        ///   <para />
        /// </param>
        void Connect(string cluster);

#pragma warning disable 618 // disable obsolete warnings (for UserPrivilege)

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        UserPrivilege GetUserPrivilege();

#pragma warning restore 618 // disable obsolete warnings (for UserPrivilege)

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="isConsole">
        ///   <para />
        /// </param>
        /// <param name="hwnd">
        ///   <para />
        /// </param>
        void SetInterfaceMode(bool isConsole, IntPtr hwnd);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        IServerVersion GetServerVersion();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerJob CreateJob();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="id">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerJob OpenJob(int id);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerJob CloneJob(int jobId);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="job">
        ///   <para />
        /// </param>
        void AddJob(ISchedulerJob job);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="job">
        ///   <para />
        /// </param>
        /// <param name="username">
        ///   <para />
        /// </param>
        /// <param name="password">
        ///   <para />
        /// </param>
        void SubmitJob(ISchedulerJob job, string username, string password);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        /// <param name="username">
        ///   <para />
        /// </param>
        /// <param name="password">
        ///   <para />
        /// </param>
        void SubmitJobById(int jobId, string username, string password);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        /// <param name="message">
        ///   <para />
        /// </param>
        void CancelJob(int jobId, string message);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobId">
        ///   <para />
        /// </param>
        void ConfigureJob(int jobId);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerCollection GetJobList(IFilterCollection filter, ISortCollection sort);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        IIntCollection GetJobIdList(IFilterCollection filter, ISortCollection sort);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerCollection GetNodeList(IFilterCollection filter, ISortCollection sort);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        IIntCollection GetNodeIdList(IFilterCollection filter, ISortCollection sort);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="nodeId">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerNode OpenNode(int nodeId);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="nodeName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerNode OpenNodeByName(string nodeName);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobTaskId">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ITaskId CreateTaskId(Int32 jobTaskId);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobTaskId">
        ///   <para />
        /// </param>
        /// <param name="instanceId">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ITaskId CreateParametricTaskId(Int32 jobTaskId, Int32 instanceId);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="value">
        ///   <para />
        /// </param>
        void SetEnvironmentVariable(string name, string value);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        INameValueCollection EnvironmentVariables { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="value">
        ///   <para />
        /// </param>
        void SetClusterParameter(string name, string value);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        INameValueCollection ClusterParameters { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        IStringCollection GetJobTemplateList();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        IStringCollection GetNodeGroupList();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="nodeGroup">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        IStringCollection GetNodesInNodeGroup(string nodeGroup);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        INameValueCollection CreateNameValueCollection();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        IFilterCollection CreateFilterCollection();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        ISortCollection CreateSortCollection();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        IStringCollection CreateStringCollection();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        IIntCollection CreateIntCollection();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="envs">
        ///   <para />
        /// </param>
        /// <param name="workDir">
        ///   <para />
        /// </param>
        /// <param name="stdIn">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ICommandInfo CreateCommandInfo(INameValueCollection envs, string workDir, string stdIn);

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
        IRemoteCommand CreateCommand(string commandLine, ICommandInfo info, IStringCollection nodes);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerCounters GetCounters();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenJobEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        ISchedulerRowSet OpenJobRowSet(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenJobHistoryEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenNodeHistoryEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenNodeEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="userName">
        ///   <para />
        /// </param>
        void DeleteCachedCredentials(string userName);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="userName">
        ///   <para />
        /// </param>
        /// <param name="password">
        ///   <para />
        /// </param>
        void SetCachedCredentials(string userName, string password);

        /// <summary>
        ///   <para />
        /// </summary>
        void Close();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobTemplateName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        XmlReader GetJobTemplateXml(string jobTemplateName);

        #endregion

        #region V3 scheduler methods/props. Don't modify this
        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="jobTemplateName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        JobTemplateInfo GetJobTemplateInfo(string jobTemplateName);

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
        void CancelJob(int jobId, string message, bool isForced);

        /// <summary>
        ///   <para />
        /// </summary>
        event EventHandler<ConnectionEventArg> OnSchedulerReconnect;
        #endregion

        #region V3 SP2 scheduler methods / props. Don't modify this

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="poolName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerPool CreatePool(string poolName);

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
        ISchedulerPool CreatePool(string poolName, int poolWeight);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="poolName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerPool OpenPool(string poolName);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="poolName">
        ///   <para />
        /// </param>
        void DeletePool(string poolName);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="poolName">
        ///   <para />
        /// </param>
        /// <param name="force">
        ///   <para />
        /// </param>
        void DeletePool(string poolName, bool force);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="properties">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        ISchedulerRowSet OpenPoolRowSet(IPropertyIdCollection properties);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerCollection GetPoolList();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="userName">
        ///   <para />
        /// </param>
        /// <param name="thumbprint">
        ///   <para />
        /// </param>
        void SetCertificateCredentials(string userName, string thumbprint);

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
        void SetCertificateCredentialsPfx(string userName, string pfxPassword, byte[] certBytes);

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
        byte[] GetCertificateFromStore(string thumbprint, out SecureString pfxPassword);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="templateName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        string EnrollCertificate(string templateName);


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
        IRemoteCommand CreateCommand(string commandLine, ICommandInfo info, IStringCollection nodes, bool redirectOutput);

        #endregion

        #region V3 SP3 scheduler methods /props

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="username">
        ///   <para />
        /// </param>
        /// <param name="password">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
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
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        string GetActiveHeadNode();

        #endregion
    }
}
