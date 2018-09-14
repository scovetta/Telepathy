using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Contains the identifiers that uniquely identify the task.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To create this interface, call the 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateTaskId(System.Int32)" /> or 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateParametricTaskId(System.Int32,System.Int32)" /> method.</para>
    ///   <para>If the job has been added to the scheduler, the task identifier is assigned to the task when you call the 
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> method. Otherwise, the identifier is assigned when the job is added to the scheduler.</para> 
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTask.TaskId" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidITaskId)]
    public interface ITaskId
    {
        /// <summary>
        ///   <para>Retrieves the identifier that identifies the parent job.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier for the parent job.</para>
        /// </value>
        [ComVisible(true)]
        int ParentJobId { get; }

        /// <summary>
        ///   <para>Retrieves the sequential identifier that identifies the task in the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The display task identifier.</para>
        /// </value>
        /// <remarks>
        ///   <para>Typically, you use this identifier for display. If the task is a parametric task, you use this value with 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId.InstanceId" /> to create the display identifier for the task. For example, JobTaskId.InstanceId.</para> 
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853421(v=vs.85).aspx">Cloning a Job</see>.</para>
        /// </example>
        [ComVisible(true)]
        int JobTaskId { get; }

        /// <summary>
        ///   <para>Retrieves the instance identifier of a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>The instance identifier.</para>
        /// </value>
        /// <remarks>
        ///   <para>Typically, you use the instance identifier with the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId.JobTaskId" /> identifier to create a display identifier for the task. For example, JobTaskId.InstanceId.</para> 
        ///   <para>Use this property only if the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.IsParametric" /> property is 
        /// True.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853421(v=vs.85).aspx">Cloning a Job</see>.</para>
        /// </example>
        [ComVisible(true)]
        int InstanceId { get; }
    }

    /// <summary>
    ///   <para>Contains the identifiers that uniquely identify the task.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" /> interface.</para>
    /// </remarks>
    /// <example />
    [Serializable]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidTaskId)]
    [ClassInterface(ClassInterfaceType.None)]
    public class TaskId : ITaskId
    {
        Int32 _parentJobId;
        Int32 _jobTaskId;
        Int32 _instanceId;

        /// <summary>
        ///   <para>Initializes an empty instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId" /> class.</para>
        /// </summary>
        public TaskId()
        {
            _parentJobId = 0;
            _jobTaskId = 0;
            _instanceId = 0;
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId" /> class using the identifier that identifies the task within the job.</para>
        /// </summary>
        /// <param name="jobTaskId">
        ///   <para>An identifier that identifies the task within the job.</para>
        /// </param>
        public TaskId(Int32 jobTaskId)
        {
            _parentJobId = 0;
            _jobTaskId = jobTaskId;

            _instanceId = 0;
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId" /> class using the identifiers for a parametric task.</para>
        /// </summary>
        /// <param name="jobTaskId">
        ///   <para>An identifier that identifies the task within the job.</para>
        /// </param>
        /// <param name="taskInstanceId">
        ///   <para>An identifier that identifies the instance of a parametric task.</para>
        /// </param>
        public TaskId(Int32 jobTaskId, Int32 taskInstanceId)
        {
            _jobTaskId = jobTaskId;
            _instanceId = taskInstanceId;

            _parentJobId = 0;
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId" /> class using the job, task, and instance identifiers. </para>
        /// </summary>
        /// <param name="parentJobId">
        ///   <para>The identifier of the job to which the task belongs.</para>
        /// </param>
        /// <param name="jobTaskId">
        ///   <para>The identifier that identifies the task within the job.</para>
        /// </param>
        /// <param name="taskInstanceId">
        ///   <para>The instance identifier for a parametric task.</para>
        /// </param>
        public TaskId(Int32 parentJobId, Int32 jobTaskId, Int32 taskInstanceId)
        {
            _jobTaskId = jobTaskId;
            _instanceId = taskInstanceId;

            _parentJobId = parentJobId;
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId" /> class using a string to identify the task identifiers.</para>
        /// </summary>
        /// <param name="text">
        ///   <para>A string that contains the parts of the 
        /// task identifier in the form JobId.TaskId.SubTaskId.</para>
        /// </param>
        public TaskId(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            //
            // Split dot separated input into individual items
            //
            string[] items = text.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            // We expect 2 or 3 items (job ID, task ID, optionally subtask ID)
            if (items.Length < 2 || items.Length > 3)
                throw new SchedulerException(ErrorCode.Operation_InvalidTaskId, string.Empty);

            //
            // Parse all items to int
            //
            int[] parsed = new int[items.Length];

            for (int i = 0; i < items.Length; ++i)
            {
                // Check if parsing was successful and if result is in expected range
                if (!int.TryParse(items[i], out parsed[i]) || parsed[i] <= 0)
                    throw new SchedulerException(ErrorCode.Operation_InvalidTaskId, string.Empty);
            }

            //
            // Store parsed values
            //
            _parentJobId = parsed[0];
            _jobTaskId = parsed[1];

            // Assign instance id only if provided
            if (items.Length == 3)
                _instanceId = parsed[2];
        }


        /// <summary>
        ///   <para>Retrieves the identifier of the job to which the task belongs.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier of the parent job.</para>
        /// </value>
        public Int32 ParentJobId
        {
            get { return _parentJobId; }
            set { _parentJobId = value; }
        }

        /// <summary>
        ///   <para>Retrieves the sequential identifier of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The display task identifier.</para>
        /// </value>
        public Int32 JobTaskId
        {
            get { return _jobTaskId; }
            set { _jobTaskId = value; }
        }

        /// <summary>
        ///   <para>Retrieves the instance identifier of a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>The instance identifier.</para>
        /// </value>
        public Int32 InstanceId
        {
            get { return _instanceId; }
            set { _instanceId = value; }
        }

        /// <summary>
        ///   <para>Retrieves a string that represents the object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A string that represents the object. The string is in the form, ParentJobId.JobTaskId.InstanceId.</para>
        /// </returns>
        public override string ToString()
        {
            if (_instanceId < 1)
            {
                return _parentJobId + "." + _jobTaskId;
            }
            else
            {
                return _parentJobId + "." + _jobTaskId + "." + _instanceId;
            }
        }

        /// <summary>
        ///   <para>Determines if this object is equal to the specified task identifier object.</para>
        /// </summary>
        /// <param name="obj">
        ///   <para>The <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId" /> object to compare with this object.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the two <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId" /> objects are equal; otherwise, false.</para>
        /// </returns>
        public override bool Equals(object obj)
        {
            TaskId that = obj as TaskId;

            return (this._parentJobId == that._parentJobId && this._jobTaskId == that._jobTaskId && this._instanceId == that._instanceId);
        }

        /// <summary>
        ///   <para>Retrieves the hash code for this object.</para>
        /// </summary>
        /// <returns>
        ///   <para>The hash code for this object.</para>
        /// </returns>
        public override int GetHashCode()
        {
            return _parentJobId + _jobTaskId + _instanceId;
        }
    }
}
