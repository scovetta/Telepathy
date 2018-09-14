using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIJobV4SP2)]
    public interface ISchedulerJobV4SP2
    {
        #region V2 methods. Don't change
        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerTask CreateTask();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="task">
        ///   <para />
        /// </param>
        void AddTask(ISchedulerTask task);


        /// <summary>
        ///   <para />
        /// </summary>
        void Refresh();

        /// <summary>
        ///   <para />
        /// </summary>
        void Commit();

        /// <summary>
        /// Opens an existing task on the server.
        /// </summary>
        /// <param name="taskId">ID of the task to open on the server.</param>
        /// <returns></returns>
        ISchedulerTask OpenTask(ITaskId taskId);

        /// <summary>
        /// Cancels the task.
        /// </summary>
        /// <param name="taskId">ID of the task to Cancel</param>
        void CancelTask(ITaskId taskId);

        /// <summary>
        /// Requeues a failed or canceled Task.
        /// </summary>
        /// <param name="taskId">ID of the task to Requeue</param>
        void RequeueTask(ITaskId taskId);

        /// <summary>
        /// Submits a task to a running job, where the Task has already been added to the cluster.
        /// </summary>
        /// <param name="taskId"></param>
        void SubmitTaskById(ITaskId taskId);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="task">
        ///   <para />
        /// </param>
        void SubmitTask(ISchedulerTask task);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <param name="expandParametric">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerCollection GetTaskList(IFilterCollection filter, ISortCollection sort, bool expandParametric);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="filter">
        ///   <para />
        /// </param>
        /// <param name="sort">
        ///   <para />
        /// </param>
        /// <param name="expandParametric">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerCollection GetTaskIdList(IFilterCollection filter, ISortCollection sort, bool expandParametric);

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
        /// <param name="expandParametric">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        ISchedulerRowEnumerator OpenTaskEnumerator(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort, bool expandParametric);

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
        /// <param name="expandParametric">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [ComVisible(false)]
        ISchedulerRowSet OpenTaskRowSet(IPropertyIdCollection properties, IFilterCollection filter, ISortCollection sort, bool expandParametric);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="templateName">
        ///   <para />
        /// </param>
        void SetJobTemplate(string templateName);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String JobTemplate { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection AllocatedNodes { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection EndpointAddresses { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        string OrderBy { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="url">
        ///   <para />
        /// </param>
        void RestoreFromXml(string url);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="reader">
        ///   <para />
        /// </param>
        [ComVisible(false)]
        void RestoreFromXml(XmlReader reader);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="value">
        ///   <para />
        /// </param>
        void SetCustomProperty(string name, string value);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        INameValueCollection GetCustomProperties();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerJobCounters GetCounters();

        /// <summary>
        ///   <para />
        /// </summary>
        event EventHandler<JobStateEventArg> OnJobState;

        /// <summary>
        ///   <para />
        /// </summary>
        event EventHandler<TaskStateEventArg> OnTaskState;

        #endregion

        #region V2 properties. Don't change
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 Id { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String Name { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String Owner { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String UserName { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.JobPriority Priority { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String Project { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 Runtime { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime SubmitTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime CreateTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime EndTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime StartTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime ChangeTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.JobState State { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.JobState PreviousState { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MinimumNumberOfCores { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MaximumNumberOfCores { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MinimumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MaximumNumberOfNodes { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MinimumNumberOfSockets { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MaximumNumberOfSockets { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.JobUnitType UnitType { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection RequestedNodes { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean IsExclusive { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean RunUntilCanceled { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection NodeGroups { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean FailOnTaskFailure { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean AutoCalculateMax { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean AutoCalculateMin { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean CanGrow { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean CanShrink { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean CanPreempt { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String ErrorMessage { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean HasRuntime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 RequeueCount { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MinMemory { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MaxMemory { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MinCoresPerNode { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 MaxCoresPerNode { get; set; }



        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection SoftwareLicense { get; set; }


        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String ClientSource { get; set; }

        #endregion

        #region V3 methods Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        void Finish();

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
        /// <param name="taskList">
        ///   <para />
        /// </param>
        [ComVisible(false)]
        void AddTasks(ISchedulerTask[] taskList);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="taskList">
        ///   <para />
        /// </param>
        [ComVisible(false)]
        void SubmitTasks(ISchedulerTask[] taskList);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="holdUntil">
        ///   <para />
        /// </param>

        void SetHoldUntil(DateTime holdUntil);

        /// <summary>
        ///   <para />
        /// </summary>

        void ClearHold();

        /// <summary>
        /// Cancels the task.
        /// </summary>
        /// <param name="taskId">ID of the task to Cancel</param>
        /// <param name="message">Message to be saved on the task object once the task has been canceled.  Can be null</param>
        [ComVisible(true)]
        void CancelTask(ITaskId taskId, string message);

        /// <summary>
        /// Cancels the task.
        /// </summary>
        /// <param name="taskId">ID of the task to Cancel</param>
        /// <param name="message">Message to be saved on the task object once the task has been canceled.  Can be null</param>
        /// <param name="isForce">Should the task be force cancelled ?</param>
        void CancelTask(ITaskId taskId, string message, bool isForce);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="nodeNames">
        ///   <para />
        /// </param>
        void AddExcludedNodes(IStringCollection nodeNames);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="nodeNames">
        ///   <para />
        /// </param>
        void RemoveExcludedNodes(IStringCollection nodeNames);

        /// <summary>
        ///   <para />
        /// </summary>
        void ClearExcludedNodes();

        #endregion

        #region V3 properties Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 Progress { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String ProgressMessage { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 TargetResourceCount { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 ExpandedPriority { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String ServiceName { get; set; }


        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime HoldUntil { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean NotifyOnStart { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean NotifyOnCompletion { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection ExcludedNodes { get; }

        #endregion

        #region V3 SP1 Methods Don't change

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
        ISchedulerRowEnumerator OpenJobAllocationHistoryEnumerator(IPropertyIdCollection properties);

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
        ISchedulerRowEnumerator OpenTaskAllocationHistoryEnumerator(IPropertyIdCollection properties);

        #endregion

        #region V3 SP1 Properties Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String EmailAddress { get; set; }

        #endregion

        #region V3 SP2 Properties Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String Pool { get; }

        #endregion

        #region V3 SP3 Properties

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [ComVisible(false)]
        Microsoft.Hpc.Scheduler.Properties.JobRuntimeType RuntimeType { get; set; }

        #endregion

        #region V4 properties. Don't change.

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String ValidExitCodes { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        bool SingleNode { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        JobNodeGroupOp NodeGroupOp { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IIntCollection ParentJobIds { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IIntCollection ChildJobIds { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        bool FailDependentTasks { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        int EstimatedProcessMemory { get; set; }

        #endregion

        #region V4SP1 properties, Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        int PlannedCoreCount { get; }

        #endregion

        #region V4SP1 methods, Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        void Requeue();

        #endregion

        #region V4SP2 methods, Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="url">
        ///   <para />
        /// </param>
        /// <param name="includeTaskGroup">
        ///   <para />
        /// </param>
        void RestoreFromXmlEx(string url, bool includeTaskGroup);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="reader">
        ///   <para />
        /// </param>
        /// <param name="includeTaskGroup">
        ///   <para />
        /// </param>
        [ComVisible(false)]
        void RestoreFromXmlEx(XmlReader reader, bool includeTaskGroup);

        #endregion
    }
}
