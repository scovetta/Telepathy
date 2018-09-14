namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Properties;

    /// <summary>
    ///   <para>Provides information about a job template, including the identifier, name, description, and security descriptor for 
    /// the job template. Also provides dates and times for when the job template was created and last changed. </para>
    /// </summary>
    /// <remarks>
    ///   <para>To get the 
    /// <see cref="Microsoft.Hpc.Scheduler.JobTemplateInfo" /> object for a job template, use the 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateInfo(System.String)" /> method. To get the XML representation of the job template, use the  
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateXml(System.String)" /> method. To get a list of the job templates for the HPC cluster, use the  
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateList" /> method. </para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateInfo(System.String)" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidJobTemplateInfo)]
    public class JobTemplateInfo
    {
        int _id;
        string _name;
        string _securityDescriptor;
        string _description;
        DateTime _changeTime;
        DateTime _createTime;

        internal JobTemplateInfo(
            int id,
            string name,
            string securityDescriptor,
            string description,
            DateTime changeTime,
            DateTime createTime)
        {
            _id = id;
            _name = name;
            _securityDescriptor = securityDescriptor;
            _description = description;
            _changeTime = changeTime;
            _createTime = createTime;
        }

        /// <summary>
        ///   <para>Gets the numeric identifier for the job template.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Int32" /> that indicates the numeric identifier of the job template.</para>
        /// </value>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        ///   <para>Gets the name of the job template.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> that contains the name of the job template.</para>
        /// </value>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///   <para>Gets a text representation of the security descriptor that controls which users have access to the job template.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.String" /> that represents the security descriptor for the job template. For information about the format of this string, see 
        /// <see href="http://go.microsoft.com/fwlink/?LinkId=208939">Security 
        /// Descriptor String Format</see> (http://go.microsoft.com/fwlink/?LinkId=208939).</para> 
        /// </value>
        public string SecurityDescriptor
        {
            get { return _securityDescriptor; }
        }

        /// <summary>
        ///   <para>Gets the description for the job template.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> that contains the description for the job template.</para>
        /// </value>
        public string Description
        {
            get { return _description; }
        }

        /// <summary>
        ///   <para>Gets the date and time in Coordinated Universal Time for when the job template was last changed.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.DateTime" /> that indicates the date and time in Coordinated Universal Time for when the job template was last changed.</para>
        /// </value>
        public DateTime ChangeTime
        {
            get { return _changeTime; }
        }

        /// <summary>
        ///   <para>Gets the date and time in Coordinated Universal Time for when the job template was created.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.DateTime" /> that indicates the date and time in Coordinated Universal Time for when the job template was created.</para>
        /// </value>
        public DateTime CreateTime
        {
            get { return _createTime; }
        }
    }

    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISchedulerV3)]
    public interface ISchedulerV3 : IDisposable
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
    }

}
