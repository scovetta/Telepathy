using System.Runtime.InteropServices;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{

    /// <summary>
    /// Defines an execution task within the context of Scheduler job.
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidITaskV4SP3)]
    public interface ISchedulerTaskV4SP3
    {
        #region V2 methods. Don't change

        /// <summary>
        /// Will refresh the properties in this object to the current values on the server.  
        /// Any local changes will be lost.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Commits any local changes to the server.
        /// </summary>
        void Commit();

        /// <summary>
        /// Adds an environment variable to the task
        /// </summary>
        /// <param name="name">Name of the variable to set</param>
        /// <param name="value">Value of the variable to set.  If 'null' it will be deleted.</param>
        void SetEnvironmentVariable(string name, string value);

        /// <summary>
        /// Returns resource counters for the task.
        /// </summary>
        /// <returns></returns>
        ISchedulerTaskCounters GetCounters();

        /// <summary>
        /// Returns a collection of the environment variables for the task.
        /// </summary>
        INameValueCollection EnvironmentVariables { get; }

        /// <summary>
        /// Collection of the nodes that have been allocated to the task while it is running and after it completes.
        /// </summary>
        IStringCollection AllocatedNodes { get; }

        /// <summary>
        /// Sets a custom property on the task.
        /// </summary>
        /// <param name="name">Name of the property to set</param>
        /// <param name="value">Value of the property</param>
        void SetCustomProperty(string name, string value);

        /// <summary>
        /// Returns all the custom properties defined for the task.
        /// </summary>
        /// <returns></returns>
        INameValueCollection GetCustomProperties();

        #endregion

        #region V2 properties. Don't change

        ///<summary>
        /// The name of this task.
        ///</summary>
        System.String Name { get; set; }

        ///<summary>
        /// The current state of the task.
        ///</summary>
        Microsoft.Hpc.Scheduler.Properties.TaskState State { get; }

        ///<summary>
        /// The previous state of the task.
        ///</summary>
        Microsoft.Hpc.Scheduler.Properties.TaskState PreviousState { get; }

        ///<summary>
        /// The minimum number of processors on which this task can run.
        ///</summary>
        System.Int32 MinimumNumberOfCores { get; set; }

        ///<summary>
        /// The maximum number of processors on which this task can run.
        ///</summary>
        System.Int32 MaximumNumberOfCores { get; set; }

        ///<summary>
        /// The minimum number of nodes on which this task can run.
        ///</summary>
        System.Int32 MinimumNumberOfNodes { get; set; }

        ///<summary>
        /// The maximum number of nodes on which this task can run.
        ///</summary>
        System.Int32 MaximumNumberOfNodes { get; set; }

        ///<summary>
        /// The minimum number of sockets on which this task can run.
        ///</summary>
        System.Int32 MinimumNumberOfSockets { get; set; }

        ///<summary>
        /// The maximum number of sockets on which this task can run.
        ///</summary>
        System.Int32 MaximumNumberOfSockets { get; set; }

        ///<summary>
        /// The hard limit on the amount of time this task will be allowed to run.
        ///</summary>
        System.Int32 Runtime { get; set; }

        ///<summary>
        /// The UTC time the task was submitted.
        ///</summary>
        System.DateTime SubmitTime { get; }

        ///<summary>
        /// The UTC time the task was created.
        ///</summary>
        System.DateTime CreateTime { get; }

        ///<summary>
        /// The UTC time the task stopped.
        ///</summary>
        System.DateTime EndTime { get; }

        ///<summary>
        /// The UTC time the task was last changed on the server.
        ///</summary>
        System.DateTime ChangeTime { get; }

        ///<summary>
        /// The UTC time the task started.
        ///</summary>
        System.DateTime StartTime { get; }

        ///<summary>
        /// The numeric ID of the job that this task belongs to.
        ///</summary>
        System.Int32 ParentJobId { get; }

        ///<summary>
        /// The ID for this task.
        ///</summary>
        Microsoft.Hpc.Scheduler.Properties.TaskId TaskId { get; }

        ///<summary>
        /// The command line that will be executed by this task.
        ///</summary>
        System.String CommandLine { get; set; }

        ///<summary>
        /// The working directory to be used during execution for this task.
        ///</summary>
        System.String WorkDirectory { get; set; }

        ///<summary>
        /// Lists the nodes that must be assigned to this task and its job in order to run.
        ///</summary>
        IStringCollection RequiredNodes { get; set; }

        ///<summary>
        /// The name of a task within the owning job that this task must wait to complete before it can start.
        ///</summary>
        IStringCollection DependsOn { get; set; }

        ///<summary>
        /// If True, no other tasks can be run on a compute node at the same time as this task.
        ///</summary>
        System.Boolean IsExclusive { get; set; }

        ///<summary>
        /// If the task runs and fails, setting Rerunnable to True means the scheduler will attempt to rerun the task. If Rerunnable is False, the task will fail after the first run attempt fails.
        ///</summary>
        System.Boolean IsRerunnable { get; set; }

        ///<summary>
        /// The path (relative to the working directory) to the file to which the stdout of this task should be written.
        ///</summary>
        System.String StdOutFilePath { get; set; }

        ///<summary>
        /// The path (relative to the working directory) to the file from which the stdin of this task should be read.
        ///</summary>
        System.String StdInFilePath { get; set; }

        ///<summary>
        /// The path (relative to the working directory) to the file to which the stderr of this task should be written.
        ///</summary>
        System.String StdErrFilePath { get; set; }

        ///<summary>
        /// The exit code for the process that this task ran.
        ///</summary>
        System.Int32 ExitCode { get; }

        ///<summary>
        /// The number of times the task has been requeued.
        ///</summary>
        System.Int32 RequeueCount { get; }

        ///<summary>
        /// If True, this task is parametric. This property is deprecated: please use the Type property instead.
        ///</summary>
        [System.ObsoleteAttribute("Please use the 'Type' property instead")]
        System.Boolean IsParametric { get; set; }

        ///<summary>
        /// The starting index for the sweep.
        ///</summary>
        System.Int32 StartValue { get; set; }

        ///<summary>
        /// The ending index for the sweep.
        ///</summary>
        System.Int32 EndValue { get; set; }

        ///<summary>
        /// The amount to increment the sweep index at each step of the sweep.
        ///</summary>
        System.Int32 IncrementValue { get; set; }

        ///<summary>
        /// Any error message that occured when the task was running.
        ///</summary>
        System.String ErrorMessage { get; }

        ///<summary>
        /// The output from the task.
        ///</summary>
        System.String Output { get; }

        ///<summary>
        /// If True this task has a runtime.
        ///</summary>
        System.Boolean HasRuntime { get; }


        ///<summary>
        /// EncryptedUserBlob
        ///</summary>
        System.Byte[] EncryptedUserBlob { get; set; }

        ///<summary>
        /// UserBlob
        ///</summary>
        System.String UserBlob { get; set; }

        #endregion

        #region V3 methods. Don't change

        /// <summary>
        /// Conclude a service task, by telling the scheduler to stop creating new service task
        /// instances after currently-running service task instances have completed.  This
        /// operation can only be applied to service tasks in the Running or Queued state.
        /// </summary>
        /// <param name="cancelSubTasks">If true, cancel all the running and queued sub-tasks
        /// of the service task.  If false, the scheduler will allow all currently running
        /// and queued service sub-tasks to complete.</param>
        void ServiceConclude(bool cancelSubTasks);

        #endregion

        #region V3 properties. Don't change.

        ///<summary>
        /// The task type. The type determine whether a task has any sub-tasks, and when it will run during the course of a job. See the TaskType enumeration for more details.
        ///</summary>
        Microsoft.Hpc.Scheduler.Properties.TaskType Type { get; set; }

        string AllocatedCoreIds { get; }

        bool IsServiceConcluded { get; }

        #endregion

        #region V3 SP1 Properties

        System.Boolean FailJobOnFailure { get; set; }

        System.Int32 FailJobOnFailureCount { get; set; }

        #endregion

        #region V4 properties. Don't change.

        System.String ValidExitCodes { get; set; }

        #endregion

        #region V4SP1 properties, Don't change

        bool ExitIfPossible { get; }

        #endregion

        #region V4SP3 properties, Don't change

        int ExecutionFailureRetryCount { get; }

        #endregion
    }

}
