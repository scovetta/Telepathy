
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{

    /// <summary>
    ///   <para>Defines a task.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask" /> interface.</para>
    /// </remarks>
    /// <example />
    public partial class SchedulerTask : SchedulerObjectBase, ISchedulerTask, ISchedulerTaskV2, ISchedulerTaskV3, ISchedulerTaskV3SP1, ISchedulerTaskV4, ISchedulerTaskV4SP1, ISchedulerTaskV4SP3
    {

        #region SchedulerTask properties. Past it to the SchedulerTask class

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        internal protected override PropertyId[] GetPropertyIds()
        {
            PropertyId[] pids =
            {
                TaskPropertyIds.Name,
                TaskPropertyIds.State,
                TaskPropertyIds.PreviousState,
                TaskPropertyIds.MinCores,
                TaskPropertyIds.MaxCores,
                TaskPropertyIds.MinNodes,
                TaskPropertyIds.MaxNodes,
                TaskPropertyIds.MinSockets,
                TaskPropertyIds.MaxSockets,
                TaskPropertyIds.RuntimeSeconds,
                TaskPropertyIds.SubmitTime,
                TaskPropertyIds.CreateTime,
                TaskPropertyIds.EndTime,
                TaskPropertyIds.ChangeTime,
                TaskPropertyIds.StartTime,
                TaskPropertyIds.ParentJobId,
                TaskPropertyIds.TaskId,
                TaskPropertyIds.CommandLine,
                TaskPropertyIds.WorkDirectory,
                TaskPropertyIds.RequiredNodes,
                TaskPropertyIds.DependsOn,
                TaskPropertyIds.IsExclusive,
                TaskPropertyIds.IsRerunnable,
                TaskPropertyIds.StdOutFilePath,
                TaskPropertyIds.StdInFilePath,
                TaskPropertyIds.StdErrFilePath,
                TaskPropertyIds.ExitCode,
                TaskPropertyIds.TaskValidExitCodes,
                TaskPropertyIds.RequeueCount,
                TaskPropertyIds.StartValue,
                TaskPropertyIds.EndValue,
                TaskPropertyIds.IncrementValue,
                TaskPropertyIds.ErrorMessage,
                TaskPropertyIds.Output,
                TaskPropertyIds.HasRuntime,
                TaskPropertyIds.EncryptedUserBlob,
                TaskPropertyIds.UserBlob,
                TaskPropertyIds.Type,
                TaskPropertyIds.IsServiceConcluded,
                TaskPropertyIds.FailJobOnFailure,
                TaskPropertyIds.FailJobOnFailureCount,
                TaskPropertyIds.ExitIfPossible,
                TaskPropertyIds.ExecutionFailureRetryCount,
                TaskPropertyIds.RequestedNodeGroup,
            };
            return pids;
        }


        System.String _Name_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the display name of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The display name. The name is limited to 80 characters.</para>
        /// </value>
        public System.String Name
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.Name, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _Name_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.Name, value);
                _props[TaskPropertyIds.Name] = prop;
                _changeProps[TaskPropertyIds.Name] = prop;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.TaskState _State_Default = TaskState.Configuring;

        /// <summary>
        ///   <para>Retrieves the state of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the task. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState" /> enumeration.</para>
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.TaskState State
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.State, out prop))
                {
                    return (Microsoft.Hpc.Scheduler.Properties.TaskState)prop.Value;
                }

                return _State_Default;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.TaskState _PreviousState_Default = TaskState.Configuring;

        /// <summary>
        ///   <para>Retrieves the previous state of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The previous state of the job. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskState" /> enumeration.</para>
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.TaskState PreviousState
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.PreviousState, out prop))
                {
                    return (Microsoft.Hpc.Scheduler.Properties.TaskState)prop.Value;
                }

                return _PreviousState_Default;
            }
        }

        System.Int32 _MinCores_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of cores that the task requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of cores.</para>
        /// </value>
        public System.Int32 MinimumNumberOfCores
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.MinCores, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _MinCores_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.MinCores, value);
                _props[TaskPropertyIds.MinCores] = prop;
                _changeProps[TaskPropertyIds.MinCores] = prop;
            }
        }

        System.Int32 _MaxCores_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of cores that the scheduler may allocate for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of cores. </para>
        /// </value>
        public System.Int32 MaximumNumberOfCores
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.MaxCores, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _MaxCores_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.MaxCores, value);
                _props[TaskPropertyIds.MaxCores] = prop;
                _changeProps[TaskPropertyIds.MaxCores] = prop;
            }
        }

        System.Int32 _MinNodes_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of nodes that the task requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of nodes. The default is 1.</para>
        /// </value>
        public System.Int32 MinimumNumberOfNodes
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.MinNodes, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _MinNodes_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.MinNodes, value);
                _props[TaskPropertyIds.MinNodes] = prop;
                _changeProps[TaskPropertyIds.MinNodes] = prop;
            }
        }

        System.Int32 _MaxNodes_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of nodes that the scheduler may allocate for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of nodes.</para>
        /// </value>
        public System.Int32 MaximumNumberOfNodes
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.MaxNodes, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _MaxNodes_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.MaxNodes, value);
                _props[TaskPropertyIds.MaxNodes] = prop;
                _changeProps[TaskPropertyIds.MaxNodes] = prop;
            }
        }

        System.Int32 _MinSockets_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of sockets that the task requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of sockets. The default is 1.</para>
        /// </value>
        public System.Int32 MinimumNumberOfSockets
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.MinSockets, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _MinSockets_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.MinSockets, value);
                _props[TaskPropertyIds.MinSockets] = prop;
                _changeProps[TaskPropertyIds.MinSockets] = prop;
            }
        }

        System.Int32 _MaxSockets_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of sockets that the scheduler may allocate for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of sockets.</para>
        /// </value>
        public System.Int32 MaximumNumberOfSockets
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.MaxSockets, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _MaxSockets_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.MaxSockets, value);
                _props[TaskPropertyIds.MaxSockets] = prop;
                _changeProps[TaskPropertyIds.MaxSockets] = prop;
            }
        }

        System.Int32 _RuntimeSeconds_Default = 0;

        /// <summary>
        ///   <para>Retrieves or sets the run-time limit for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The run-time limit for the task, in seconds.</para>
        /// </value>
        public System.Int32 Runtime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.RuntimeSeconds, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _RuntimeSeconds_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.RuntimeSeconds, value);
                _props[TaskPropertyIds.RuntimeSeconds] = prop;
                _changeProps[TaskPropertyIds.RuntimeSeconds] = prop;
            }
        }

        System.DateTime _SubmitTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the time that the task was submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>The time the task was submitted.</para>
        /// </value>
        public System.DateTime SubmitTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.SubmitTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _SubmitTime_Default;
            }
        }

        System.DateTime _CreateTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the date and time when the task was created.</para>
        /// </summary>
        /// <value>
        ///   <para>The task creation time.</para>
        /// </value>
        public System.DateTime CreateTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.CreateTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _CreateTime_Default;
            }
        }

        System.DateTime _EndTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the date and time that the task ended.</para>
        /// </summary>
        /// <value>
        ///   <para>The time the task ended.</para>
        /// </value>
        public System.DateTime EndTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.EndTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _EndTime_Default;
            }
        }

        System.DateTime _ChangeTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>The last time that the user or server changed a property of the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The date and time that the task was last touched.</para>
        /// </value>
        public System.DateTime ChangeTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.ChangeTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _ChangeTime_Default;
            }
        }

        System.DateTime _StartTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the date and time that the task started running.</para>
        /// </summary>
        /// <value>
        ///   <para>The task start time. The value is in Coordinated Universal Time.</para>
        /// </value>
        public System.DateTime StartTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.StartTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _StartTime_Default;
            }
        }

        System.Int32 _ParentJobId_Default = 1;

        /// <summary>
        ///   <para>Retrieves the identifier of the parent job.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier of the parent job.</para>
        /// </value>
        public System.Int32 ParentJobId
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.ParentJobId, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _ParentJobId_Default;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.TaskId _TaskId_Default = new TaskId();

        /// <summary>
        ///   <para>Retrieves the identifiers that uniquely identify the task.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.TaskId" /> object that contains the identifiers that uniquely identify the task.</para>
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.TaskId TaskId
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.TaskId, out prop))
                {
                    return (Microsoft.Hpc.Scheduler.Properties.TaskId)prop.Value;
                }

                return _TaskId_Default;
            }
        }

        System.String _CommandLine_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the command line for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The command line. The command is limited to 480 characters.</para>
        /// </value>
        public System.String CommandLine
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.CommandLine, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _CommandLine_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.CommandLine, value);
                _props[TaskPropertyIds.CommandLine] = prop;
                _changeProps[TaskPropertyIds.CommandLine] = prop;
            }
        }

        System.String _WorkDirectory_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the directory in which to start the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The absolute path to the startup directory.</para>
        /// </value>
        public System.String WorkDirectory
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.WorkDirectory, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _WorkDirectory_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.WorkDirectory, value);
                _props[TaskPropertyIds.WorkDirectory] = prop;
                _changeProps[TaskPropertyIds.WorkDirectory] = prop;
            }
        }

        IStringCollection _RequiredNodes_List = null;

        /// <summary>
        ///   <para>Retrieves or sets the list of required nodes for the task.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of node names. </para>
        /// </value>
        public IStringCollection RequiredNodes
        {
            get
            {
                if (_RequiredNodes_List == null)
                {
                    StoreProperty prop;
                    if (_props.TryGetValue(TaskPropertyIds.RequiredNodes, out prop))
                    {
                        _RequiredNodes_List = Util.String2Collection((string)prop.Value);
                    }
                    else
                    {
                        _RequiredNodes_List = new StringCollection();
                    }
                }

                return _RequiredNodes_List;
            }

            set
            {
                _RequiredNodes_List = value;
                string RequiredNodesValue = string.Empty;
                if (_RequiredNodes_List != null)
                {
                    RequiredNodesValue = Util.Enumerable2String(_RequiredNodes_List);
                }

                StoreProperty prop;
                if (!_props.TryGetValue(TaskPropertyIds.RequiredNodes, out prop))
                {
                    prop = null;
                }

                if (prop != null && (string)prop.Value == RequiredNodesValue)
                {
                    return;
                }

                if (prop == null && string.IsNullOrEmpty(RequiredNodesValue))
                {
                    return;
                }

                StoreProperty newProp = new StoreProperty(TaskPropertyIds.RequiredNodes, RequiredNodesValue);
                _props[TaskPropertyIds.RequiredNodes] = newProp;
                _changeProps[TaskPropertyIds.RequiredNodes] = newProp;
            }
        }

        IStringCollection _DependsOn_List = null;

        /// <summary>
        ///   <para>Retrieves or sets the dependent tasks.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of task names.</para>
        /// </value>
        public IStringCollection DependsOn
        {
            get
            {
                if (_DependsOn_List == null)
                {
                    StoreProperty prop;
                    if (_props.TryGetValue(TaskPropertyIds.DependsOn, out prop))
                    {
                        _DependsOn_List = Util.String2Collection((string)prop.Value);
                    }
                    else
                    {
                        _DependsOn_List = new StringCollection();
                    }
                }

                return _DependsOn_List;
            }

            set
            {
                _DependsOn_List = value;

                // Task names should not contain commas
                CheckTaskNameFormat(_DependsOn_List);

                string DependsOnValue = string.Empty;
                if (_DependsOn_List != null)
                {
                    DependsOnValue = Util.Enumerable2String(_DependsOn_List);
                }

                StoreProperty prop;
                if (!_props.TryGetValue(TaskPropertyIds.DependsOn, out prop))
                {
                    prop = null;
                }

                if (prop != null && (string)prop.Value == DependsOnValue)
                {
                    return;
                }

                if (prop == null && string.IsNullOrEmpty(DependsOnValue))
                {
                    return;
                }

                StoreProperty newProp = new StoreProperty(TaskPropertyIds.DependsOn, DependsOnValue);
                _props[TaskPropertyIds.DependsOn] = newProp;
                _changeProps[TaskPropertyIds.DependsOn] = newProp;
            }
        }

        System.Boolean _IsExclusive_Default = false;

        /// <summary>
        ///   <para>Determines whether other tasks from the job can run on the node at the same time as this task.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if other tasks from the same job 
        /// cannot run on the node; otherwise, False. The default is False.</para>
        /// </value>
        public System.Boolean IsExclusive
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.IsExclusive, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                return _IsExclusive_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.IsExclusive, value);
                _props[TaskPropertyIds.IsExclusive] = prop;
                _changeProps[TaskPropertyIds.IsExclusive] = prop;
            }
        }

        System.Boolean _IsRerunnable_Default = true;

        /// <summary>
        ///   <para>Determines whether the task can run again after a failure.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the task can run again 
        /// after a failure; otherwise, False. The default is True.</para>
        /// </value>
        public System.Boolean IsRerunnable
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.IsRerunnable, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                return _IsRerunnable_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.IsRerunnable, value);
                _props[TaskPropertyIds.IsRerunnable] = prop;
                _changeProps[TaskPropertyIds.IsRerunnable] = prop;
            }
        }

        System.String _StdOutFilePath_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the path to which the server redirects standard output.</para>
        /// </summary>
        /// <value>
        ///   <para>The file to which standard output is redirected.</para>
        /// </value>
        public System.String StdOutFilePath
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.StdOutFilePath, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _StdOutFilePath_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.StdOutFilePath, value);
                _props[TaskPropertyIds.StdOutFilePath] = prop;
                _changeProps[TaskPropertyIds.StdOutFilePath] = prop;
            }
        }

        System.String _StdInFilePath_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the path from which the server redirects standard input.</para>
        /// </summary>
        /// <value>
        ///   <para>The file from which standard input is redirected.</para>
        /// </value>
        public System.String StdInFilePath
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.StdInFilePath, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _StdInFilePath_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.StdInFilePath, value);
                _props[TaskPropertyIds.StdInFilePath] = prop;
                _changeProps[TaskPropertyIds.StdInFilePath] = prop;
            }
        }

        System.String _StdErrFilePath_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the path to which the server redirects standard error.</para>
        /// </summary>
        /// <value>
        ///   <para>The file to which standard error is redirected.</para>
        /// </value>
        public System.String StdErrFilePath
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.StdErrFilePath, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _StdErrFilePath_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.StdErrFilePath, value);
                _props[TaskPropertyIds.StdErrFilePath] = prop;
                _changeProps[TaskPropertyIds.StdErrFilePath] = prop;
            }
        }

        System.Int32 _ExitCode_Default = 0;

        /// <summary>
        ///   <para>Retrieves the exit code that the task set.</para>
        /// </summary>
        /// <value>
        ///   <para>The exit code.</para>
        /// </value>
        public System.Int32 ExitCode
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.ExitCode, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _ExitCode_Default;
            }
        }

        System.String _TaskValidExitCodes_Default = "";

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="System.String" />.</para>
        /// </value>
        public System.String ValidExitCodes
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.TaskValidExitCodes, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _TaskValidExitCodes_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.TaskValidExitCodes, value);
                _props[TaskPropertyIds.TaskValidExitCodes] = prop;
                _changeProps[TaskPropertyIds.TaskValidExitCodes] = prop;
            }
        }

        System.Int32 _RequeueCount_Default = 0;

        /// <summary>
        ///   <para>Retrieves the number of times that the task has been queued again.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of times that the task has been queued again.</para>
        /// </value>
        public System.Int32 RequeueCount
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.RequeueCount, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _RequeueCount_Default;
            }
        }

        /// <summary>
        ///   <para>Determines whether the task is a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the task is a 
        /// parametric task; otherwise, False. The default is False.</para>
        /// </value>
        [System.ObsoleteAttribute("Please use the 'Type' property instead")]
        public System.Boolean IsParametric
        {
            get
            {
                return Microsoft.Hpc.Scheduler.Store.TaskTypeHelper.IsParametric(this.Type);
            }

            set
            {
                this.Type = value ? TaskType.ParametricSweep : TaskType.Basic;
            }
        }

        System.Int32 _StartValue_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the starting instance value for a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>The starting instance value.</para>
        /// </value>
        public System.Int32 StartValue
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.StartValue, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _StartValue_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.StartValue, value);
                _props[TaskPropertyIds.StartValue] = prop;
                _changeProps[TaskPropertyIds.StartValue] = prop;
            }
        }

        System.Int32 _EndValue_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the ending value for a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>The ending value.</para>
        /// </value>
        public System.Int32 EndValue
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.EndValue, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _EndValue_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.EndValue, value);
                _props[TaskPropertyIds.EndValue] = prop;
                _changeProps[TaskPropertyIds.EndValue] = prop;
            }
        }

        System.Int32 _IncrementValue_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the number by which to increment the instance value for a parametric task.</para>
        /// </summary>
        /// <value>
        ///   <para>The increment value used to calculate the next instance value.</para>
        /// </value>
        public System.Int32 IncrementValue
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.IncrementValue, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _IncrementValue_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.IncrementValue, value);
                _props[TaskPropertyIds.IncrementValue] = prop;
                _changeProps[TaskPropertyIds.IncrementValue] = prop;
            }
        }

        System.String _ErrorMessage_Default = "";

        /// <summary>
        ///   <para>Retrieves the task-related error message or task cancellation message.</para>
        /// </summary>
        /// <value>
        ///   <para>The message.</para>
        /// </value>
        public System.String ErrorMessage
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.ErrorMessage, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _ErrorMessage_Default;
            }
        }

        System.String _Output_Default = "";

        /// <summary>
        ///   <para>Retrieves the output generated by the command.</para>
        /// </summary>
        /// <value>
        ///   <para>The output from the command.</para>
        /// </value>
        public System.String Output
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.Output, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _Output_Default;
            }
        }

        System.Boolean _HasRuntime_Default = false;

        /// <summary>
        ///   <para>Determines whether the task has set the <see cref="Microsoft.Hpc.Scheduler.SchedulerTask.Runtime" /> task property.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the task specifies a runtime limit; otherwise, False.</para>
        /// </value>
        public System.Boolean HasRuntime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.HasRuntime, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                return _HasRuntime_Default;
            }
        }

        System.Byte[] _EncryptedUserBlob_Default = null;

        /// <summary>
        ///   <para>The encrypted user blob.</para>
        /// </summary>
        /// <value>
        ///   <para>A byte array that contains the encrypted blob. </para>
        /// </value>
        public System.Byte[] EncryptedUserBlob
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.EncryptedUserBlob, out prop))
                {
                    return (System.Byte[])prop.Value;
                }

                return _EncryptedUserBlob_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.EncryptedUserBlob, value);
                _props[TaskPropertyIds.EncryptedUserBlob] = prop;
                _changeProps[TaskPropertyIds.EncryptedUserBlob] = prop;
            }
        }

        System.String _UserBlob_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the user data associated with the task.</para>
        /// </summary>
        /// <value>
        ///   <para>The user data associated with the task. The resulting encrypted data is limited to 8,000 bytes.</para>
        /// </value>
        /// <remarks>
        ///   <para>HPC encrypts the user data when the job is added or submitted to the scheduler. Only the user that added or submitted the job can get 
        /// the user data. Note that if the user specifies a different RunAs user when 
        /// the job is submitted, the RunAs user will not be able to access the user data.</para> 
        /// </remarks>
        public System.String UserBlob
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.UserBlob, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _UserBlob_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.UserBlob, value);
                _props[TaskPropertyIds.UserBlob] = prop;
                _changeProps[TaskPropertyIds.UserBlob] = prop;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.TaskType _Type_Default = TaskType.Basic;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.TaskType Type
        {
            get
            {
                GetPropertyVersionCheck(TaskPropertyIds.Type);
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.Type, out prop))
                {
                    return (Microsoft.Hpc.Scheduler.Properties.TaskType)prop.Value;
                }

                return _Type_Default;
            }

            set
            {
                SetPropertyVersionCheck(TaskPropertyIds.Type, value);
                StoreProperty prop = new StoreProperty(TaskPropertyIds.Type, value);
                _props[TaskPropertyIds.Type] = prop;
                _changeProps[TaskPropertyIds.Type] = prop;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public bool IsServiceConcluded
        {
            get
            {
                GetPropertyVersionCheck(TaskPropertyIds.IsServiceConcluded);

                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.IsServiceConcluded, out prop))
                {
                    return (bool)prop.Value;
                }

                return false;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public bool FailJobOnFailure
        {
            get
            {
                GetPropertyVersionCheck(TaskPropertyIds.FailJobOnFailure);

                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.FailJobOnFailure, out prop))
                {
                    return (bool)prop.Value;
                }

                return false;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.FailJobOnFailure, value);
                _props[TaskPropertyIds.FailJobOnFailure] = prop;
                _changeProps[TaskPropertyIds.FailJobOnFailure] = prop;
            }
        }

        System.Int32 _FailJobOnFailureCount_Default = 1;
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public Int32 FailJobOnFailureCount
        {
            get
            {
                GetPropertyVersionCheck(TaskPropertyIds.FailJobOnFailureCount);

                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.FailJobOnFailureCount, out prop))
                {
                    return (Int32)prop.Value;
                }

                return _FailJobOnFailureCount_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.FailJobOnFailureCount, value);
                _props[TaskPropertyIds.FailJobOnFailureCount] = prop;
                _changeProps[TaskPropertyIds.FailJobOnFailureCount] = prop;
            }
        }


        bool _ExitIfPossbile_Default = false;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public bool ExitIfPossible
        {
            get
            {
                GetPropertyVersionCheck(TaskPropertyIds.ExitIfPossible);

                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.ExitIfPossible, out prop))
                {
                    return (bool)prop.Value;
                }

                return _ExitIfPossbile_Default;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int ExecutionFailureRetryCount
        {
            get
            {
                GetPropertyVersionCheck(TaskPropertyIds.ExecutionFailureRetryCount);

                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.ExecutionFailureRetryCount, out prop))
                {
                    return (int)prop.Value;
                }

                return 0;
            }
        }

        System.String _RequestedNodeGroup_Default = null;

        /// <summary>
        /// <para>the task requested node group</para>
        /// </summary>
        public System.String RequestedNodeGroup
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(TaskPropertyIds.RequestedNodeGroup, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _RequestedNodeGroup_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(TaskPropertyIds.RequestedNodeGroup, value);
                _props[TaskPropertyIds.RequestedNodeGroup] = prop;
                _changeProps[TaskPropertyIds.RequestedNodeGroup] = prop;
            }
        }

        #endregion
    }

}

