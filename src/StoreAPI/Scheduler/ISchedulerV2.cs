namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Properties;

    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISchedulerV2)]
    public interface ISchedulerV2 : IDisposable
    {
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
    }

}
