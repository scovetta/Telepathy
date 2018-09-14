using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Hpc.Scheduler.Store;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines the state of a core on a node in the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const offline = 0
    /// const idle = 1
    /// const busy = 2
    /// const draining = 3
    /// const reserved = 4</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerCore.State" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidSchedulerCoreState)]
    public enum SchedulerCoreState
    {
        /// <summary>
        ///   <para>The core is offline. This enumeration member represents a value of 0.</para>
        /// </summary>
        Offline = 0,

        /// <summary>
        ///   <para>The core is idle and ready to run a job. This enumeration member represents a value of 1.</para>
        /// </summary>
        Idle = 1,

        /// <summary>
        ///   <para>The core is running a job or task. This enumeration member represents a value of 2.</para>
        /// </summary>
        Busy = 2,

        /// <summary>
        ///   <para>The core is running a job but is marked to be 
        /// taken offline after the current job finishes. This enumeration member represents a value of 3.</para>
        /// </summary>
        Draining = 3,

        /// <summary>
        ///   <para>The core is reserved for a job. This enumeration member represents a value of 4.</para>
        /// </summary>
        Reserved = 4,
    }

    /// <summary>
    ///   <para>Provides information about a core on a node such as its state and the task currently running on it.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call the <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.GetCores" /> method.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISchedulerProcessor)]
    public interface ISchedulerCore
    {
        /// <summary>
        ///   <para>Retrieves the identifier of the job running on the core.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier of the job running on the core; otherwise, zero.</para>
        /// </value>
        /// <remarks>
        ///   <para>This job identifier and the ParentJobId property of 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCore.TaskId" />  will be the same value if a task is running. If the job is reserving resources (the job does not include tasks and the  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RunUntilCanceled" /> property is 
        /// True), this identifier will contain the job identifier and the ParentJobId property will be zero.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see 
        /// href="https://msdn.microsoft.com/library/cc853436(v=vs.85).aspx">Getting a List of Nodes in the Cluster</see>.</para> 
        /// </example>
        int JobId { get; }

        /// <summary>
        ///   <para>Retrieves the identifier of the task running on the core.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" /> object that contains the identifiers that uniquely identify the task running on the core. The identifiers are zero if there is no task running on the core.</para> 
        /// </value>
        ITaskId TaskId { get; }

        /// <summary>
        ///   <para>Retrieves the state of the core.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the core. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.SchedulerCoreState" /> enumeration.</para>
        /// </value>
        SchedulerCoreState State { get; }

        /// <summary>
        ///   <para>Retrieves the identifier that uniquely identifies the core.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier that uniquely identifies the core.</para>
        /// </value>
        int Id { get; }
    }

    /// <summary>
    ///   <para>Provides information about a core on a node such as its state and the task currently running on it.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerCore" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidSchedulerProcessor)]
    [ClassInterface(ClassInterfaceType.None)]
    public class SchedulerCore : ISchedulerCore
    {
        int _jobId;
        TaskId _taskId;
        SchedulerCoreState _state;
        int _coreId;

        internal SchedulerCore()
        {
            _jobId = 0;
            _coreId = 0;
            _taskId = new TaskId();

            _state = SchedulerCoreState.Offline;
        }

        /// <summary>
        ///   <para>Retrieves the identifier of the job running on the core.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier of the job running on the core; otherwise, zero.</para>
        /// </value>
        public int JobId
        {
            get { return _jobId; }
        }

        /// <summary>
        ///   <para>Retrieves the identifier of the task running on the core.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.ITaskId" /> object that contains the identifiers that uniquely identify the task running on the core. The identifiers are zero if there is no task running on the core.</para> 
        /// </value>
        public ITaskId TaskId
        {
            get { return _taskId; }
        }

        /// <summary>
        ///   <para>Retrieves the state of the core.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the core. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.SchedulerCoreState" /> enumeration.</para>
        /// </value>
        public SchedulerCoreState State
        {
            get { return _state; }
        }

        /// <summary>
        ///   <para>Retrieves the identifier that uniquely identifies the core.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier that uniquely identifies the core.</para>
        /// </value>
        public int Id
        {
            get { return _coreId; }
        }

        internal void InitFromRow(PropertyRow row)
        {
            StoreProperty prop;

            prop = row[ResourcePropertyIds.JobId];
            if (prop != null)
            {
                _jobId = (int)prop.Value;
            }
            else
            {
                _jobId = 0;
            }

            prop = row[ResourcePropertyIds.JobTaskId];
            if (prop != null)
            {
                _taskId = (TaskId)prop.Value;
            }
            else
            {
                _taskId = new TaskId(0, 0);
            }

            prop = row[ResourcePropertyIds.Phantom];
            if (prop != null && (bool)prop.Value == true)
            {
                _coreId = (int)row[ResourcePropertyIds.Id].Value;
            }
            else
            {
                _coreId = (int)row[ResourcePropertyIds.CoreId].Value;
            }

            prop = row[ResourcePropertyIds.MoveToOffline];

            StoreProperty propState = row[ResourcePropertyIds.State];

            if (propState != null)
            {
                if ((ResourceState)propState.Value == ResourceState.Offline)
                {
                    _state = SchedulerCoreState.Offline;
                }
                else if (prop != null && (bool)prop.Value == true)
                {
                    _state = SchedulerCoreState.Draining;
                }
                else if ((ResourceState)propState.Value == ResourceState.Idle)
                {
                    _state = SchedulerCoreState.Idle;
                }
                else
                {
                    _state = SchedulerCoreState.Busy;
                }
            }
        }
    }

}
