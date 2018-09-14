using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Hpc.Scheduler.Properties;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the state of the job changes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To implement your event handler, see the <see cref="Microsoft.Hpc.Scheduler.JobStateHandler" /> delegate.</para>
    /// </remarks>
    [ComVisible(true)]
    [Guid(ComGuids.GuidIJobStateChangeEventArgs)]
    public interface IJobStateEventArg
    {
        /// <summary>
        ///   <para>Retrieves the identifier of the job whose state has changed.</para>
        /// </summary>
        /// <value>
        ///   <para>The job identifier.</para>
        /// </value>
        /// <remarks>
        ///   <para>Use the job identifier when calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJob(System.Int32)" /> method to get an interface to the job whose state has changed.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853482(v=vs.85).aspx">Implementing the Event Handlers for Job Events in C#</see>.</para> 
        /// </example>
        int JobId { get; }
        /// <summary>
        ///   <para>Retrieves the state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the job. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> enumeration.</para>
        /// </value>
        JobState NewState { get; }
        /// <summary>
        ///   <para>Retrieves the previous state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The previous state of the job. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> enumeration.</para>
        /// </value>
        JobState PreviousState { get; }
    }

    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the state of the job changes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To implement your event handler, see the <see cref="Microsoft.Hpc.Scheduler.JobStateHandler" /> delegate.</para>
    /// </remarks>
    [ComVisible(true)]
    [Guid(ComGuids.GuidJobStateChangeEventArgsClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class JobStateEventArg : EventArgs, IJobStateEventArg
    {
        int _jobId;
        JobState _newState;
        JobState _previousState;

        internal JobStateEventArg(int jobId, JobState newState, JobState previousState)
        {
            _jobId = jobId;
            _newState = newState;
            _previousState = previousState;
        }

        #region IJobStateChangeEventArgs Members

        /// <summary>
        ///   <para>Retrieves the identifier of the job whose state has changed.</para>
        /// </summary>
        /// <value>
        ///   <para>The job identifier.</para>
        /// </value>
        /// <remarks>
        ///   <para>Use the job identifier when calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJob(System.Int32)" /> method to get an interface to the job whose state has changed.</para>
        /// </remarks>
        public int JobId
        {
            get { return _jobId; }
        }

        /// <summary>
        ///   <para>Retrieves the state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the job. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> enumeration.</para>
        /// </value>
        public JobState NewState
        {
            get { return _newState; }
        }

        /// <summary>
        ///   <para>Retrieves the previous state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The previous state of the job. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> enumeration.</para>
        /// </value>
        public JobState PreviousState
        {
            get { return _previousState; }
        }

        #endregion
    }

    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the state of a task in the job changes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To implement your event handler, see the <see cref="Microsoft.Hpc.Scheduler.TaskStateHandler" /> delegate.</para>
    ///   <para>This interface is also used with the <see cref="Microsoft.Hpc.Scheduler.CommandTaskStateHandler" /> delegate.</para>
    /// </remarks>
    [ComVisible(true)]
    [Guid(ComGuids.GuidITaskStateChangeEventArgs)]
    public interface ITaskStateEventArg
    {
        /// <summary>
        ///   <para>Retrieves the identifier of the job that contains the task whose state has changed.</para>
        /// </summary>
        /// <value>
        ///   <para>The job identifier.</para>
        /// </value>
        /// <remarks>
        ///   <para>Use the job identifier when calling the 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJob(System.Int32)" /> method to get an interface to the job.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853482(v=vs.85).aspx">Implementing the Event Handlers for Job Events in C#</see>.</para> 
        /// </example>
        int JobId { get; }
        /// <summary>
        ///   <para>Retrieves the identifier that uniquely identifies the task in a job.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" /> object that uniquely identifies the task.</para>
        /// </value>
        /// <remarks>
        ///   <para>Use the task identifier when calling the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method to get an interface to the task.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853482(v=vs.85).aspx">Implementing the Event Handlers for Job Events in C#</see>.</para> 
        /// </example>
        ITaskId TaskId { get; }
        /// <summary>
        ///   <para>Retrieves the state of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the task. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState" /> enumeration.</para>
        /// </value>
        TaskState NewState { get; }
        /// <summary>
        ///   <para>Retrieves the previous state of the task.
        /// </para>
        /// </summary>
        /// <value>
        ///   <para>The previous state of the task. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState" /> enumeration.</para>
        /// </value>
        TaskState PreviousState { get; }
    }

    /// <summary>
    ///   <para>Defines the arguments that are passed to your event handler when the state of a task in the job changes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.ITaskStateEventArg" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [Guid(ComGuids.GuidTaskStateChangeEventArgsClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class TaskStateEventArg : EventArgs, ITaskStateEventArg
    {
        int _jobId;
        TaskId _taskId;
        TaskState _newState;
        TaskState _previousState;

        internal TaskStateEventArg(int jobId, TaskId taskId, TaskState newState, TaskState previousState)
        {
            _jobId = jobId;
            _taskId = taskId;
            _newState = newState;
            _previousState = previousState;
        }

        #region IJobStateChangeEventArgs Members

        /// <summary>
        ///   <para>Retrieves the identifier of the job that contains the task whose state has changed.</para>
        /// </summary>
        /// <value>
        ///   <para>The job identifier.</para>
        /// </value>
        public int JobId
        {
            get { return _jobId; }
        }

        /// <summary>
        ///   <para>Retrieves the identifier that uniquely identifies the task in a job.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" /> interface that uniquely identifies the task.</para>
        /// </value>
        public ITaskId TaskId
        {
            get { return _taskId; }
        }

        /// <summary>
        ///   <para>Retrieves the state of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the task. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState" /> enumeration.</para>
        /// </value>
        public TaskState NewState
        {
            get { return _newState; }
        }

        /// <summary>
        ///   <para>Retrieves the previous state of the task.
        /// </para>
        /// </summary>
        /// <value>
        ///   <para>The previous state of the task. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState" /> enumeration.</para>
        /// </value>
        public TaskState PreviousState
        {
            get { return _previousState; }
        }

        #endregion
    }

    /// <summary>
    ///   <para>Defines the delegate to implement when you subscribe to the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OnJobState" /> event.</para>
    /// </summary>
    /// <param name="sender">
    ///   <para>A scheduler object. Cast the object to an <see cref="Microsoft.Hpc.Scheduler.IScheduler" /> interface.</para>
    /// </param>
    /// <param name="arg">
    ///   <para>A <see cref="Microsoft.Hpc.Scheduler.JobStateEventArg" /> object that provides information related to the state of the job.</para>
    /// </param>
    /// <remarks>
    ///   <para>To get the job, cast the <paramref name="sender" /> object to an 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler" /> interface. Then, pass the 
    /// <see cref="Microsoft.Hpc.Scheduler.JobStateEventArg.JobId" /> property to the 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJob(System.Int32)" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.TaskStateHandler" />
    public delegate void JobStateHandler(object sender, JobStateEventArg arg);

    /// <summary>
    ///   <para>Defines the delegate to implement when you subscribe to the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OnTaskState" /> event.</para>
    /// </summary>
    /// <param name="sender">
    ///   <para>A scheduler object. Cast the object to an <see cref="Microsoft.Hpc.Scheduler.IScheduler" /> interface.</para>
    /// </param>
    /// <param name="arg">
    ///   <para>An <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg" /> object that provides information related to the state of the task.</para>
    /// </param>
    /// <remarks>
    ///   <para>To get the job, cast the <paramref name="sender" /> object to an 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler" /> interface. Then, pass the 
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg.JobId" /> property to the 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJob(System.Int32)" /> method. To get the task, pass the 
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg.TaskId" /> property to the 
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.JobStateHandler" />
    public delegate void TaskStateHandler(object sender, TaskStateEventArg arg);

    /// <summary>
    ///   <para>Defines the interface that COM applications implement to handle events raised by the 
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> object. </para>
    /// </summary>
    [ComVisible(true)]
    [Guid(ComGuids.GuidISchedulerJobEvents)]
    [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface ISchedulerJobEvents
    {
        /// <summary>
        ///   <para>Is called when the state of the job changes.</para>
        /// </summary>
        /// <param name="sender">
        ///   <para>The job object that sent the event.</para>
        /// </param>
        /// <param name="arg">
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.JobStateEventArg" /> object that contains the event properties.</para>
        /// </param>
        void OnJobState(object sender, JobStateEventArg arg);
        /// <summary>
        ///   <para>Is called when the state of a task in the job changes.</para>
        /// </summary>
        /// <param name="sender">
        ///   <para>The job object that sent the event.</para>
        /// </param>
        /// <param name="arg">
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg" /> object that contains the event properties.</para>
        /// </param>
        void OnTaskState(object sender, TaskStateEventArg arg);
    }
}
