// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Data
{
    // Important!: this class is only intended to be used in session API and subjected to change
    // TODO: Trim the enum down
    public enum TaskState
    {
        /// <summary>
        ///   <para>The state is not set. This enumeration member represents a value of 0.</para>
        /// </summary>
        NA = 0x0,
        /// <summary>
        ///   <para>The task is being configured. The application called the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CreateTask" /> method to create the task but has not called the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask)" /> method to add the task to the job. This enumeration member represents a value of 1.</para> 
        /// </summary>
        Configuring = 0x1,
        /// <summary>
        ///   <para>The task was added to the scheduling queue. This enumeration member represents a value of 2.</para>
        /// </summary>
        Submitted = 0x2,
        /// <summary>
        ///   <para>The scheduler is determining if the task is correctly configured and can run. This enumeration member represents a value of 4.</para>
        /// </summary>
        Validating = 0x4,
        /// <summary>
        ///   <para>The task was added to the scheduling queue. This enumeration member represents a value of 8.</para>
        /// </summary>
        Queued = 0x8,
        /// <summary>
        ///   <para>The scheduler is in the process of sending the task to the node to run. This enumeration member represents a value of 16.</para>
        /// </summary>
        Dispatching = 0x10,
        /// <summary>
        ///   <para>The task is running. This enumeration member represents a value of 32.</para>
        /// </summary>
        Running = 0x20,
        /// <summary>
        ///   <para>The node is cleaning up the resources that were allocated to the task. This enumeration member represents a value of 64.</para>
        /// </summary>
        Finishing = 0x40,
        /// <summary>
        ///   <para>The task successfully finished. This enumeration member represents a value of 128.</para>
        /// </summary>
        Finished = 0x80,
        /// <summary>
        ///   <para>The task failed, the job was canceled, or a system error occurred on the compute node. To get a description of the error, access the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ErrorMessage" /> property. This enumeration member represents a value of 256.</para>
        /// </summary>
        Failed = 0x100,
        /// <summary>
        ///   <para>The task was canceled (see 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.CancelTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" />). If the caller provided the reason for canceling the task, then the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.ErrorMessage" /> property will contain the reason. This enumeration member represents a value of 512.</para> 
        /// </summary>
        Canceled = 0x200,
        /// <summary>
        ///   <para>The task is in the process of being canceled. This enumeration member represents a value of 1024.</para>
        /// </summary>
        Canceling = 0x400,
        /// <summary>
        ///   <para>A mask used to indicate all states. This enumeration member represents a value of 2047.</para>
        /// </summary>
        All = Configuring | Submitted | Validating | Queued | Dispatching | Running | Finishing | Finished | Failed | Canceled | Canceling,
    }
}
