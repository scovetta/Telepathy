using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the states of a core.</para>
    /// </summary>
    [Flags]
    public enum ResourceState
    {
        /// <summary>
        ///   <para>The core is offline.</para>
        /// </summary>
        Offline = 0x0,
        /// <summary>
        ///   <para>The core is idle and ready to run a job.</para>
        /// </summary>
        Idle = 0x1,
        /// <summary>
        ///   <para>The core is reserved to run a job sometime in the future.</para>
        /// </summary>
        ScheduledReserve = 0x2,
        /// <summary>
        ///   <para>A job is scheduled on the core but is not yet running tasks.</para>
        /// </summary>
        JobScheduled = 0x4,
        /// <summary>
        ///   <para>The core ready and waiting for a task to run.</para>
        /// </summary>
        ReadyForTask = 0x8,
        /// <summary>
        ///   <para>Scheduled the core to run the task.</para>
        /// </summary>
        TaskScheduled = 0x10,
        /// <summary>
        ///   <para>Scheduled the core to run the job.</para>
        /// </summary>
        JobTaskScheduled = 0x20,
        /// <summary>
        ///   <para>Sent a request to the core to run the task.</para>
        /// </summary>
        TaskDispatched = 0x40,
        /// <summary>
        ///   <para>Sent a request to the core to run the job. </para>
        /// </summary>
        JobTaskDispatched = 0x80,
        /// <summary>
        ///   <para>The core is running the task.</para>
        /// </summary>
        TaskRunning = 0x100,
        /// <summary>
        ///   <para>The core is in the process of closing the task (the task finished running).</para>
        /// </summary>
        CloseTask = 0x200,
        /// <summary>
        ///   <para>Sent a request to the core to close the task.</para>
        /// </summary>
        CloseTaskDispatched = 0x400,
        /// <summary>
        ///   <para>The task is closed on the core.</para>
        /// </summary>
        TaskClosed = 0x800,
        /// <summary>
        ///   <para>The core is in the process of closing the job (all tasks have finished running).</para>
        /// </summary>
        CloseJob = 0x1000,
        /// <summary>
        ///   <para>A mask that represent all state values.</para>
        /// </summary>
        All = Offline | Idle | ScheduledReserve | JobScheduled | ReadyForTask | JobTaskScheduled
                                 | TaskScheduled | JobTaskDispatched | TaskDispatched | TaskRunning | CloseTask
                                 | CloseTaskDispatched | TaskClosed | CloseJob,
        /// <summary>
        ///   <para>Unknown state.</para>
        /// </summary>
        NA = 0x7FFFFFFF,
    }

    /// <summary>
    ///   <para />
    /// </summary>
    [Flags]
    public enum ResourceJobPhase
    {
        /// <summary>
        ///   <para />
        /// </summary>
        Unassigned = 0x0,
        /// <summary>
        ///   <para />
        /// </summary>
        NodePrepPending = 0x1,
        /// <summary>
        ///   <para />
        /// </summary>
        NodePrepRunning = 0x2,
        /// <summary>
        ///   <para />
        /// </summary>
        NodePrepFinished = 0x4,
        /// <summary>
        ///   <para />
        /// </summary>
        NodePrepCloseJob = 0x8,
        /// <summary>
        ///   <para />
        /// </summary>
        JobBody = 0x10,
        /// <summary>
        ///   <para />
        /// </summary>
        NodeReleaseRunning = 0x20,
        /// <summary>
        ///   <para />
        /// </summary>
        NodeReleaseFinished = 0x40,
        /// <summary>
        ///   <para />
        /// </summary>
        All = Unassigned | NodePrepPending | NodePrepRunning | NodePrepFinished | NodePrepCloseJob
                                  | JobBody | NodeReleaseRunning | NodeReleaseFinished,
        /// <summary>
        ///   <para />
        /// </summary>
        NA = 0x7FFFFFFF,
    }

    /// <summary>
    ///   <para>Defines the identifiers that uniquely identify the properties of a schedulable resource.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Currently, the only schedulable resource is a core.</para>
    /// </remarks>
    [Serializable]
    public class ResourcePropertyIds
    {
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.ResourcePropertyIds" /> class.</para>
        /// </summary>
        protected ResourcePropertyIds()
        {
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the core in the store.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para>The time when the job first starts running on the core.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId StartTime
        {
            get { return StorePropertyIds.StartTime; }
        }

        /// <summary>
        ///   <para>The last time that the server changed one of the property values of the resource.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ChangeTime
        {
            get { return StorePropertyIds.ChangeTime; }
        }

        /// <summary>
        ///   <para>The amount of time, in seconds, that the task should run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId RuntimeSeconds
        {
            get { return StorePropertyIds.RuntimeSeconds; }
        }

        /// <summary>
        ///   <para>Indicates whether the job that is running on the core is supposed to run until it is canceled.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId RunUntilCanceled
        {
            get { return StorePropertyIds.RunUntilCanceled; }
        }

        /// <summary>
        ///   <para>Identifies the types of jobs that the resource can run.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId JobType
        {
            get { return StorePropertyIds.JobType; }
        }

        /// <summary>
        ///   <para>The state of the resource.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId State
        {
            get { return _State; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public static PropertyId JobPhase
        {
            get { return _JobPhase; }
        }

        /// <summary>
        ///   <para>The previous state of the resource.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId PreviousState
        {
            get { return _PreviousState; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the core in the node.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId CoreId
        {
            get { return _CoreID; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the socket that contains the core.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId SocketId
        {
            get { return _SocketID; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the job that the resource is running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId JobId
        {
            get { return _JobID; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the task running on the core.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId TaskId
        {
            get { return _TaskID; }
        }

        /// <summary>
        ///   <para>The time by when the task must finish. </para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId LimitTime
        {
            get { return _LimitTime; }
        }

        /// <summary>
        ///   <para>The last time that the resource let the server know that is was alive.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId LastPingTime
        {
            get { return _LastPingTime; }
        }

        /// <summary>
        ///   <para>Indicates that the core is being taken offline.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId MoveToOffline
        {
            get { return _MoveToOffline; }
        }

        /// <summary>
        ///   <para>The name of the node that contains the core.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId NodeName
        {
            get { return _NodeName; }
        }

        /// <summary>
        ///   <para>The time by which the resource must be available to run another task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ReserveLimitTime
        {
            get { return _ReserveLimitTime; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies a job that is reserving the resource for future use.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId ReserveJobId
        {
            get { return _ReserveJobID; }
        }

        /// <summary>
        ///   <para>The time that the last task was dispatched to run on the core.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId DispatchTime
        {
            get { return _DispatchTime; }
        }

        /// <summary>
        ///   <para>Indicates that this allocation is the first resource allocation for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId FirstTaskAllocation
        {
            get { return _FirstTaskAllocation; }
        }

        /// <summary>
        ///   <para>Indicates if the resource is reachable.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Reachable
        {
            get { return _Reachable; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the node that contains the core.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId NodeId
        {
            get { return _NodeId; }
        }

        /// <summary>
        ///   <para>Indicates if the core is a virtual core.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Phantom
        {
            get { return _Phantom; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the task that the resource is running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId JobTaskId
        {
            get { return _TaskNiceId; }
        }

        /// <summary>
        ///   <para>The command line of the task that the core is running.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CommandLine
        {
            get { return _CommandLine; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId IsAvailable
        {
            get { return _IsAvailable; }
        }

        /// <summary>
        ///   <para>The internal resource object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ResourceObject
        {
            get { return StorePropertyIds.ResourceObject; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        static public PropertyId MemoryUsed
        {
            get { return _MemoryUsed; }
        }

        //
        // Private static values
        // 

        static PropertyId _State = new PropertyId(StorePropertyType.ResourceState, "State", PropertyIdConstants.ResourcePropertyIdStart + 1);
        static PropertyId _PreviousState = new PropertyId(StorePropertyType.ResourceState, "PreviousState", PropertyIdConstants.ResourcePropertyIdStart + 2);

        static PropertyId _JobPhase = new PropertyId(StorePropertyType.ResourceJobPhase, "JobPhase", PropertyIdConstants.ResourcePropertyIdStart + 3);

        static PropertyId _CoreID = new PropertyId(StorePropertyType.Int32, "CoreID", PropertyIdConstants.ResourcePropertyIdStart + 4);
        static PropertyId _SocketID = new PropertyId(StorePropertyType.Int64, "SocketID", PropertyIdConstants.ResourcePropertyIdStart + 5);

        static PropertyId _JobID = new PropertyId(StorePropertyType.Int32, "JobID", PropertyIdConstants.ResourcePropertyIdStart + 6);
        static PropertyId _TaskID = new PropertyId(StorePropertyType.Int32, "TaskID", PropertyIdConstants.ResourcePropertyIdStart + 7);

        static PropertyId _LimitTime = new PropertyId(StorePropertyType.DateTime, "LimitTime", PropertyIdConstants.ResourcePropertyIdStart + 11);
        static PropertyId _LastPingTime = new PropertyId(StorePropertyType.DateTime, "LastPingTime", PropertyIdConstants.ResourcePropertyIdStart + 13);
        static PropertyId _OfflineTime = new PropertyId(StorePropertyType.DateTime, "OfflineTime", PropertyIdConstants.ResourcePropertyIdStart + 15);
        static PropertyId _OnlineTime = new PropertyId(StorePropertyType.DateTime, "OnlineTime", PropertyIdConstants.ResourcePropertyIdStart + 16);

        static PropertyId _MoveToOffline = new PropertyId(StorePropertyType.Boolean, "MoveToOffline", PropertyIdConstants.ResourcePropertyIdStart + 17);

        static PropertyId _NodeName = new PropertyId(StorePropertyType.String, "NodeName", PropertyIdConstants.ResourcePropertyIdStart + 19);

        static PropertyId _ReserveLimitTime = new PropertyId(StorePropertyType.DateTime, "ReserveLimitTime", PropertyIdConstants.ResourcePropertyIdStart + 20);
        static PropertyId _ReserveJobID = new PropertyId(StorePropertyType.Int32, "ReserveJobID", PropertyIdConstants.ResourcePropertyIdStart + 21);

        static PropertyId _DispatchTime = new PropertyId(StorePropertyType.DateTime, "DispatchTime", PropertyIdConstants.ResourcePropertyIdStart + 24);

        static PropertyId _FirstTaskAllocation = new PropertyId(StorePropertyType.Boolean, "FirstTaskAllocation", PropertyIdConstants.ResourcePropertyIdStart + 25);

        static PropertyId _Reachable = new PropertyId(StorePropertyType.Boolean, "Reachable", PropertyIdConstants.ResourcePropertyIdStart + 32);

        static PropertyId _NodeId = new PropertyId(StorePropertyType.Int32, "NodeId", PropertyIdConstants.ResourcePropertyIdStart + 34);
        static PropertyId _Phantom = new PropertyId(StorePropertyType.Boolean, "Phantom", PropertyIdConstants.ResourcePropertyIdStart + 35);

        static PropertyId _TaskNiceId = new PropertyId(StorePropertyType.TaskId, "NiceTaskId", PropertyIdConstants.ResourcePropertyIdStart + 36);

        static PropertyId _CommandLine = new PropertyId(StorePropertyType.String, "CommandLine", PropertyIdConstants.ResourcePropertyIdStart + 37);

        static PropertyId _IsAvailable = new PropertyId(StorePropertyType.Boolean, "IsAvailable", PropertyIdConstants.ResourcePropertyIdStart + 38);

        static PropertyId _MemoryUsed = new PropertyId(StorePropertyType.Int32, "MemoryUsed", PropertyIdConstants.ResourcePropertyIdStart + 39);
    }

}
