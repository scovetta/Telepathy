
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Manages the tasks and resources that are associated with a job.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob" /> interface.</para>
    /// </remarks>
    /// <example />
    public partial class SchedulerJob : SchedulerObjectBase, ISchedulerJob, ISchedulerJobV3, ISchedulerJobV2, ISchedulerJobV3SP1, ISchedulerJobV3SP2, ISchedulerJobV3SP3, ISchedulerJobV4
    {
        /// <summary>
        ///   <para>An array of properties to query from the store when refreshing the job object.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        internal protected override PropertyId[] GetPropertyIds()
        {
            PropertyId[] pids =
            {
                JobPropertyIds.Id,
                JobPropertyIds.Name,
                JobPropertyIds.Owner,
                JobPropertyIds.UserName,
                JobPropertyIds.Project,
                JobPropertyIds.RuntimeSeconds,
                JobPropertyIds.SubmitTime,
                JobPropertyIds.CreateTime,
                JobPropertyIds.EndTime,
                JobPropertyIds.StartTime,
                JobPropertyIds.ChangeTime,
                JobPropertyIds.State,
                JobPropertyIds.PreviousState,
                JobPropertyIds.MinCores,
                JobPropertyIds.MaxCores,
                JobPropertyIds.MinNodes,
                JobPropertyIds.MaxNodes,
                JobPropertyIds.MinSockets,
                JobPropertyIds.MaxSockets,
                JobPropertyIds.UnitType,
                JobPropertyIds.RequestedNodes,
                JobPropertyIds.IsExclusive,
                JobPropertyIds.RunUntilCanceled,
                JobPropertyIds.NodeGroups,
                JobPropertyIds.FailOnTaskFailure,
                JobPropertyIds.AutoCalculateMax,
                JobPropertyIds.AutoCalculateMin,
                JobPropertyIds.CanGrow,
                JobPropertyIds.CanShrink,
                JobPropertyIds.Preemptable,
                JobPropertyIds.ErrorMessage,
                JobPropertyIds.HasRuntime,
                JobPropertyIds.RequeueCount,
                JobPropertyIds.MinMemory,
                JobPropertyIds.MaxMemory,
                JobPropertyIds.MinCoresPerNode,
                JobPropertyIds.MaxCoresPerNode,
                JobPropertyIds.EndpointReference,
                JobPropertyIds.SoftwareLicense,
                JobPropertyIds.OrderBy,
                JobPropertyIds.ClientSource,
                JobPropertyIds.Progress,
                JobPropertyIds.ProgressMessage,
                JobPropertyIds.TargetResourceCount,
                JobPropertyIds.ExpandedPriority,
                JobPropertyIds.ServiceName,
                JobPropertyIds.JobTemplate,
                JobPropertyIds.HoldUntil,
                JobPropertyIds.NotifyOnStart,
                JobPropertyIds.NotifyOnCompletion,
                JobPropertyIds.ExcludedNodes,
                JobPropertyIds.EmailAddress,
                JobPropertyIds.Pool,
                JobPropertyIds.RuntimeType,
                JobPropertyIds.JobValidExitCodes,
                JobPropertyIds.SingleNode,
                JobPropertyIds.NodeGroupOp,
                JobPropertyIds.ParentJobIds,
                JobPropertyIds.FailDependentTasks,
                JobPropertyIds.EstimatedProcessMemory,
                JobPropertyIds.PlannedCoreCount,
                JobPropertyIds.TaskExecutionFailureRetryLimit,
            };

            return pids;
        }


        System.Int32 _Id_Default = -1;

        /// <summary>
        ///   <para>Retrieves the job identifier.</para>
        /// </summary>
        /// <value>
        ///   <para>The job identifier.</para>
        /// </value>
        public System.Int32 Id
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.Id, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _Id_Default;
            }
        }

        System.String _Name_ProfileDefault = null;
        System.String _Name_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the display name of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The display name. The name is limited to 80 characters.</para>
        /// </value>
        public System.String Name
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.Name, out prop))
                {
                    return (System.String)prop.Value;
                }

                if (_Name_ProfileDefault != null)
                {
                    return (System.String)_Name_ProfileDefault;
                }

                return _Name_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.Name, value);
                _props[JobPropertyIds.Name] = prop;
                _changeProps[JobPropertyIds.Name] = prop;
            }
        }

        System.String _Owner_Default = "";

        /// <summary>
        ///   <para>Retrieves the name of the user who created, submitted, or queued the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The user name.</para>
        /// </value>
        public System.String Owner
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.Owner, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _Owner_Default;
            }
        }

        System.String _UserName_ProfileDefault = null;
        System.String _UserName_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the RunAs user for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The user name in the form, domain\username.</para>
        /// </value>
        public System.String UserName
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.UserName, out prop))
                {
                    return (System.String)prop.Value;
                }

                if (_UserName_ProfileDefault != null)
                {
                    return (System.String)_UserName_ProfileDefault;
                }

                return _UserName_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.UserName, value);
                _props[JobPropertyIds.UserName] = prop;
                _changeProps[JobPropertyIds.UserName] = prop;
            }
        }

        /// <summary>
        ///   <para>Retrieves or sets the job priority.</para>
        /// </summary>
        /// <value>
        ///   <para>The job priority. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobPriority" /> enumeration.</para>
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.JobPriority Priority
        {
            get
            {
                return Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.ExpandedPriorityToJobPriority(this.ExpandedPriority);
            }

            set
            {
                this.ExpandedPriority = Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.JobPriorityToExpandedPriority((int)value);
            }
        }

        System.String _Project_ProfileDefault = null;
        System.String _Project_Default = "";

        /// <summary>
        ///   <para>Retrieves or sets the project name to associate with the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The project name. The name is limited to 80 characters.</para>
        /// </value>
        public System.String Project
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.Project, out prop))
                {
                    return (System.String)prop.Value;
                }

                if (_Project_ProfileDefault != null)
                {
                    return (System.String)_Project_ProfileDefault;
                }

                return _Project_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.Project, value);
                _props[JobPropertyIds.Project] = prop;
                _changeProps[JobPropertyIds.Project] = prop;
            }
        }

        System.Int32? _RuntimeSeconds_ProfileDefault = null;
        System.Int32 _RuntimeSeconds_Default = 0;

        /// <summary>
        ///   <para>Retrieves or sets the run-time limit for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The run-time limit for the job, in seconds.</para>
        /// </value>
        public System.Int32 Runtime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.RuntimeSeconds, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_RuntimeSeconds_ProfileDefault != null)
                {
                    return (System.Int32)_RuntimeSeconds_ProfileDefault;
                }

                return _RuntimeSeconds_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.RuntimeSeconds, value);
                _props[JobPropertyIds.RuntimeSeconds] = prop;
                _changeProps[JobPropertyIds.RuntimeSeconds] = prop;
            }
        }

        System.DateTime _SubmitTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the time that the job was submitted.</para>
        /// </summary>
        /// <value>
        ///   <para>The job submit time. The value is 
        /// <see cref="System.DateTime.MinValue" /> if the job has not been submitted (see 
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob,System.String,System.String)" />).</para>
        /// </value>
        public System.DateTime SubmitTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.SubmitTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _SubmitTime_Default;
            }
        }

        System.DateTime _CreateTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the date and time that the job was created.</para>
        /// </summary>
        /// <value>
        ///   <para>The job creation time.</para>
        /// </value>
        public System.DateTime CreateTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.CreateTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _CreateTime_Default;
            }
        }

        System.DateTime _EndTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the date and time that job ended.</para>
        /// </summary>
        /// <value>
        ///   <para>The job end time.</para>
        /// </value>
        public System.DateTime EndTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.EndTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _EndTime_Default;
            }
        }

        System.DateTime _StartTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>The date and time that the job started running.</para>
        /// </summary>
        /// <value>
        ///   <para>The job start time.</para>
        /// </value>
        public System.DateTime StartTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.StartTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _StartTime_Default;
            }
        }

        System.DateTime _ChangeTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the last time that the user or server changed a property of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The date and time that the job was last touched.</para>
        /// </value>
        public System.DateTime ChangeTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.ChangeTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _ChangeTime_Default;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.JobState _State_Default = JobState.Configuring;

        /// <summary>
        ///   <para>Retrieves the state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the job. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> enumeration.</para>
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.JobState State
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.State, out prop))
                {
                    return (Microsoft.Hpc.Scheduler.Properties.JobState)prop.Value;
                }

                return _State_Default;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.JobState _PreviousState_Default = JobState.Configuring;

        /// <summary>
        ///   <para>Retrieves the previous state of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The previous state of the job. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobState" /> enumeration.</para>
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.JobState PreviousState
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.PreviousState, out prop))
                {
                    return (Microsoft.Hpc.Scheduler.Properties.JobState)prop.Value;
                }

                return _PreviousState_Default;
            }
        }

        System.Int32? _MinCores_ProfileDefault = null;
        System.Int32 _MinCores_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of cores that the job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of cores.</para>
        /// </value>
        public System.Int32 MinimumNumberOfCores
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MinCores, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MinCores_ProfileDefault != null)
                {
                    return (System.Int32)_MinCores_ProfileDefault;
                }

                return _MinCores_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MinCores, value);
                _props[JobPropertyIds.MinCores] = prop;
                _changeProps[JobPropertyIds.MinCores] = prop;

                UnitType = JobUnitType.Core;
                AutoCalculateMin = false;
            }
        }

        System.Int32? _MaxCores_ProfileDefault = null;
        System.Int32 _MaxCores_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of cores that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of cores.</para>
        /// </value>
        public System.Int32 MaximumNumberOfCores
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MaxCores, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MaxCores_ProfileDefault != null)
                {
                    return (System.Int32)_MaxCores_ProfileDefault;
                }

                return _MaxCores_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MaxCores, value);
                _props[JobPropertyIds.MaxCores] = prop;
                _changeProps[JobPropertyIds.MaxCores] = prop;

                UnitType = JobUnitType.Core;
                AutoCalculateMax = false;
            }
        }

        System.Int32? _MinNodes_ProfileDefault = null;
        System.Int32 _MinNodes_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of nodes that the job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of nodes. </para>
        /// </value>
        public System.Int32 MinimumNumberOfNodes
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MinNodes, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MinNodes_ProfileDefault != null)
                {
                    return (System.Int32)_MinNodes_ProfileDefault;
                }

                return _MinNodes_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MinNodes, value);
                _props[JobPropertyIds.MinNodes] = prop;
                _changeProps[JobPropertyIds.MinNodes] = prop;

                UnitType = JobUnitType.Node;
                AutoCalculateMin = false;
            }
        }

        System.Int32? _MaxNodes_ProfileDefault = null;
        System.Int32 _MaxNodes_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of nodes that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of nodes.</para>
        /// </value>
        public System.Int32 MaximumNumberOfNodes
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MaxNodes, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MaxNodes_ProfileDefault != null)
                {
                    return (System.Int32)_MaxNodes_ProfileDefault;
                }

                return _MaxNodes_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MaxNodes, value);
                _props[JobPropertyIds.MaxNodes] = prop;
                _changeProps[JobPropertyIds.MaxNodes] = prop;

                UnitType = JobUnitType.Node;
                AutoCalculateMax = false;
            }
        }

        System.Int32? _MinSockets_ProfileDefault = null;
        System.Int32 _MinSockets_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of sockets that the job requires to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of sockets.</para>
        /// </value>
        public System.Int32 MinimumNumberOfSockets
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MinSockets, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MinSockets_ProfileDefault != null)
                {
                    return (System.Int32)_MinSockets_ProfileDefault;
                }

                return _MinSockets_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MinSockets, value);
                _props[JobPropertyIds.MinSockets] = prop;
                _changeProps[JobPropertyIds.MinSockets] = prop;

                UnitType = JobUnitType.Socket;
                AutoCalculateMin = false;
            }
        }

        System.Int32? _MaxSockets_ProfileDefault = null;
        System.Int32 _MaxSockets_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of sockets that the scheduler may allocate for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of sockets. </para>
        /// </value>
        public System.Int32 MaximumNumberOfSockets
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MaxSockets, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MaxSockets_ProfileDefault != null)
                {
                    return (System.Int32)_MaxSockets_ProfileDefault;
                }

                return _MaxSockets_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MaxSockets, value);
                _props[JobPropertyIds.MaxSockets] = prop;
                _changeProps[JobPropertyIds.MaxSockets] = prop;

                UnitType = JobUnitType.Socket;
                AutoCalculateMax = false;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.JobUnitType? _UnitType_ProfileDefault = null;
        Microsoft.Hpc.Scheduler.Properties.JobUnitType _UnitType_Default = JobUnitType.Core;

        /// <summary>
        ///   <para>Determines whether cores, nodes, or sockets are used to allocate resources for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The unit type. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType" /> enumeration.</para>
        /// </value>
        public virtual Microsoft.Hpc.Scheduler.Properties.JobUnitType UnitType
        {
            get
            {
                StoreProperty prop;
                JobUnitType? unitType = null;
                if (_props.TryGetValue(JobPropertyIds.UnitType, out prop))
                {
                    unitType = (JobUnitType)prop.Value;
                }
                else if (_UnitType_ProfileDefault != null)
                {
                    unitType = (JobUnitType)_UnitType_ProfileDefault;
                }
                else
                {
                    unitType = _UnitType_Default;
                }

                if (unitType == JobUnitType.Socket)
                {
                    if (SchedulerStore.IsGpuJob(this._store, this.Id))
                    {
                        unitType = JobUnitType.Gpu;
                    }
                }

                return unitType.Value;
            }
            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.UnitType, value);
                _props[JobPropertyIds.UnitType] = prop;
                StoreProperty[] changedProps = SchedulerStoreHelpers.UpdateCustomGpuPropertyIfNeeded(this._store, new StoreProperty[] { prop });
                foreach (StoreProperty newProp in changedProps)
                {
                    if (newProp.PropName.Equals(JobPropertyIds.JobGpuCustomPropertyName))
                    {
                        _customprops.SetCustomProperty(newProp.PropName, newProp.Value.ToString());
                    }
                    else
                    {
                        _changeProps[newProp.Id] = prop;
                    }
                }
            }
        }

        string _RequestedNodes_ProfileDefault = null;
        IStringCollection _RequestedNodes_List = null;

        /// <summary>
        ///   <para>Retrieves or sets the list of nodes that are requested for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of node names. The nodes must exist in the cluster.</para> 
        /// </value>
        public IStringCollection RequestedNodes
        {
            get
            {
                if (_RequestedNodes_List == null)
                {
                    StoreProperty prop;
                    if (_props.TryGetValue(JobPropertyIds.RequestedNodes, out prop))
                    {
                        _RequestedNodes_List = Util.String2Collection((string)prop.Value);
                    }
                    else if (_RequestedNodes_ProfileDefault != null)
                    {
                        _RequestedNodes_List = Util.String2Collection(_RequestedNodes_ProfileDefault);
                    }
                    else
                    {
                        _RequestedNodes_List = new StringCollection();
                    }
                }

                return _RequestedNodes_List;
            }

            set
            {
                _RequestedNodes_List = value;
                string RequestedNodesValue = string.Empty;
                if (_RequestedNodes_List != null)
                {
                    RequestedNodesValue = Util.Enumerable2String(_RequestedNodes_List);
                }

                StoreProperty prop;
                if (!_props.TryGetValue(JobPropertyIds.RequestedNodes, out prop))
                {
                    prop = null;
                }

                if (prop != null && (string)prop.Value == RequestedNodesValue)
                {
                    return;
                }

                if (prop == null && string.IsNullOrEmpty(RequestedNodesValue))
                {
                    return;
                }

                StoreProperty newProp = new StoreProperty(JobPropertyIds.RequestedNodes, RequestedNodesValue);
                _props[JobPropertyIds.RequestedNodes] = newProp;
                _changeProps[JobPropertyIds.RequestedNodes] = newProp;
            }
        }

        System.Boolean? _IsExclusive_ProfileDefault = null;
        System.Boolean _IsExclusive_Default = false;

        /// <summary>
        ///   <para>Determines whether nodes should be exclusively allocated to the job.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the nodes are exclusively allocated 
        /// to the job; otherwise, False. The default is True.</para>
        /// </value>
        public System.Boolean IsExclusive
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.IsExclusive, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_IsExclusive_ProfileDefault != null)
                {
                    return (System.Boolean)_IsExclusive_ProfileDefault;
                }

                return _IsExclusive_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.IsExclusive, value);
                _props[JobPropertyIds.IsExclusive] = prop;
                _changeProps[JobPropertyIds.IsExclusive] = prop;
            }
        }

        System.Boolean? _RunUntilCanceled_ProfileDefault = null;
        System.Boolean _RunUntilCanceled_Default = false;

        /// <summary>
        ///   <para>Determines whether the server reserves resources for the job until the job is canceled (even if the job has no active tasks).</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the server reserves resources 
        /// for the job; otherwise, False. The default is False.</para>
        /// </value>
        public System.Boolean RunUntilCanceled
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.RunUntilCanceled, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_RunUntilCanceled_ProfileDefault != null)
                {
                    return (System.Boolean)_RunUntilCanceled_ProfileDefault;
                }

                return _RunUntilCanceled_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.RunUntilCanceled, value);
                _props[JobPropertyIds.RunUntilCanceled] = prop;
                _changeProps[JobPropertyIds.RunUntilCanceled] = prop;
            }
        }

        string _NodeGroups_ProfileDefault = null;
        IStringCollection _NodeGroups_List = null;

        /// <summary>
        ///   <para>Retrieves or sets the names of the node groups that define the nodes on which the job can run.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains a collection of node group names.</para>
        /// </value>
        public IStringCollection NodeGroups
        {
            get
            {
                if (_NodeGroups_List == null)
                {
                    StoreProperty prop;
                    if (_props.TryGetValue(JobPropertyIds.NodeGroups, out prop))
                    {
                        _NodeGroups_List = Util.String2Collection((string)prop.Value);
                    }
                    else if (_NodeGroups_ProfileDefault != null)
                    {
                        _NodeGroups_List = Util.String2Collection(_NodeGroups_ProfileDefault);
                    }
                    else
                    {
                        _NodeGroups_List = new StringCollection();
                    }
                }

                return _NodeGroups_List;
            }

            set
            {
                _NodeGroups_List = value;
                string NodeGroupsValue = string.Empty;
                if (_NodeGroups_List != null)
                {
                    NodeGroupsValue = Util.Enumerable2String(_NodeGroups_List);
                }

                StoreProperty prop;
                if (!_props.TryGetValue(JobPropertyIds.NodeGroups, out prop))
                {
                    prop = null;
                }

                if (prop != null && (string)prop.Value == NodeGroupsValue)
                {
                    return;
                }

                if (prop == null && string.IsNullOrEmpty(NodeGroupsValue))
                {
                    return;
                }

                StoreProperty newProp = new StoreProperty(JobPropertyIds.NodeGroups, NodeGroupsValue);
                _props[JobPropertyIds.NodeGroups] = newProp;
                _changeProps[JobPropertyIds.NodeGroups] = newProp;
            }
        }

        System.Boolean? _FailOnTaskFailure_ProfileDefault = null;
        System.Boolean _FailOnTaskFailure_Default = false;

        /// <summary>
        ///   <para>Determines whether the job fails when one of the tasks in the job fails.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the job fails when 
        /// a task fails; otherwise, False. The default is True.</para>
        /// </value>
        public System.Boolean FailOnTaskFailure
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.FailOnTaskFailure, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_FailOnTaskFailure_ProfileDefault != null)
                {
                    return (System.Boolean)_FailOnTaskFailure_ProfileDefault;
                }

                return _FailOnTaskFailure_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.FailOnTaskFailure, value);
                _props[JobPropertyIds.FailOnTaskFailure] = prop;
                _changeProps[JobPropertyIds.FailOnTaskFailure] = prop;
            }
        }

        System.Boolean? _AutoCalculateMax_ProfileDefault = null;
        System.Boolean _AutoCalculateMax_Default = false;

        /// <summary>
        ///   <para>Determines whether the scheduler automatically calculates the maximum resource value.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the scheduler calculates the 
        /// maximum value; otherwise, False if the application specifies the value.</para>
        /// </value>
        public System.Boolean AutoCalculateMax
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.AutoCalculateMax, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_AutoCalculateMax_ProfileDefault != null)
                {
                    return (System.Boolean)_AutoCalculateMax_ProfileDefault;
                }

                return _AutoCalculateMax_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.AutoCalculateMax, value);
                _props[JobPropertyIds.AutoCalculateMax] = prop;
                _changeProps[JobPropertyIds.AutoCalculateMax] = prop;
            }
        }

        System.Boolean? _AutoCalculateMin_ProfileDefault = null;
        System.Boolean _AutoCalculateMin_Default = false;

        /// <summary>
        ///   <para>Determines whether the scheduler automatically calculates the minimum resource value.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the scheduler calculates the 
        /// minimum value; otherwise, False if the application specifies the value.</para>
        /// </value>
        public System.Boolean AutoCalculateMin
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.AutoCalculateMin, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_AutoCalculateMin_ProfileDefault != null)
                {
                    return (System.Boolean)_AutoCalculateMin_ProfileDefault;
                }

                return _AutoCalculateMin_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.AutoCalculateMin, value);
                _props[JobPropertyIds.AutoCalculateMin] = prop;
                _changeProps[JobPropertyIds.AutoCalculateMin] = prop;
            }
        }

        System.Boolean? _CanGrow_ProfileDefault = null;
        System.Boolean _CanGrow_Default = true;

        /// <summary>
        ///   <para>Determines whether the job resources can grow.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the job resources can grow as more resources become available; otherwise, False.</para>
        /// </value>
        public System.Boolean CanGrow
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.CanGrow, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_CanGrow_ProfileDefault != null)
                {
                    return (System.Boolean)_CanGrow_ProfileDefault;
                }

                return _CanGrow_Default;
            }
        }

        System.Boolean? _CanShrink_ProfileDefault = null;
        System.Boolean _CanShrink_Default = true;

        /// <summary>
        ///   <para>Determines whether the job resources can shrink.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the server releases extra resources 
        /// when they are no longer needed by the job; otherwise, False.</para>
        /// </value>
        public System.Boolean CanShrink
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.CanShrink, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_CanShrink_ProfileDefault != null)
                {
                    return (System.Boolean)_CanShrink_ProfileDefault;
                }

                return _CanShrink_Default;
            }
        }

        System.Boolean? _Preemptable_ProfileDefault = null;
        System.Boolean _Preemptable_Default = false;

        /// <summary>
        ///   <para>Determines whether another job can preempt this job.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if another job can preempt this job; otherwise, False.</para>
        /// </value>
        public System.Boolean CanPreempt
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.Preemptable, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_Preemptable_ProfileDefault != null)
                {
                    return (System.Boolean)_Preemptable_ProfileDefault;
                }

                return _Preemptable_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.Preemptable, value);
                _props[JobPropertyIds.Preemptable] = prop;
                _changeProps[JobPropertyIds.Preemptable] = prop;
            }
        }

        System.String _ErrorMessage_Default = "";

        /// <summary>
        ///   <para>Retrieves the job-related error message or job cancellation message.</para>
        /// </summary>
        /// <value>
        ///   <para>The message.</para>
        /// </value>
        public System.String ErrorMessage
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.ErrorMessage, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _ErrorMessage_Default;
            }
        }

        System.Boolean _HasRuntime_Default = false;

        /// <summary>
        ///   <para>Determines whether the job has set the <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.Runtime" /> job property.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the job specifies a runtime limit; otherwise, False.</para>
        /// </value>
        public System.Boolean HasRuntime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.HasRuntime, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                return _HasRuntime_Default;
            }
        }

        System.Int32 _RequeueCount_Default = 0;

        /// <summary>
        ///   <para>Retrieves the number of times that the job has been queued again.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of times that the job has been queued again.</para>
        /// </value>
        public System.Int32 RequeueCount
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.RequeueCount, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _RequeueCount_Default;
            }
        }

        System.Int32? _MinMemory_ProfileDefault = null;
        System.Int32 _MinMemory_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the minimum amount of memory that a node must have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum amount of memory, in megabytes, that a node must have for the job to run on it.</para>
        /// </value>
        public System.Int32 MinMemory
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MinMemory, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MinMemory_ProfileDefault != null)
                {
                    return (System.Int32)_MinMemory_ProfileDefault;
                }

                return _MinMemory_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MinMemory, value);
                _props[JobPropertyIds.MinMemory] = prop;
                _changeProps[JobPropertyIds.MinMemory] = prop;
            }
        }

        System.Int32? _MaxMemory_ProfileDefault = null;
        System.Int32 _MaxMemory_Default = Int32.MaxValue;

        /// <summary>
        ///   <para>Retrieves or sets the maximum amount of memory that a node may have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum amount of memory, in megabytes, that a node may have for the job to run on it.</para>
        /// </value>
        public System.Int32 MaxMemory
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MaxMemory, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MaxMemory_ProfileDefault != null)
                {
                    return (System.Int32)_MaxMemory_ProfileDefault;
                }

                return _MaxMemory_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MaxMemory, value);
                _props[JobPropertyIds.MaxMemory] = prop;
                _changeProps[JobPropertyIds.MaxMemory] = prop;
            }
        }

        System.Int32? _MinCoresPerNode_ProfileDefault = null;
        System.Int32 _MinCoresPerNode_Default = 1;

        /// <summary>
        ///   <para>Retrieves or sets the minimum number of cores that a node must have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>The minimum number of processors on a node that the job requires. The default is 1.</para>
        /// </value>
        public System.Int32 MinCoresPerNode
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MinCoresPerNode, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MinCoresPerNode_ProfileDefault != null)
                {
                    return (System.Int32)_MinCoresPerNode_ProfileDefault;
                }

                return _MinCoresPerNode_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MinCoresPerNode, value);
                _props[JobPropertyIds.MinCoresPerNode] = prop;
                _changeProps[JobPropertyIds.MinCoresPerNode] = prop;
            }
        }

        System.Int32? _MaxCoresPerNode_ProfileDefault = null;
        System.Int32 _MaxCoresPerNode_Default = Int32.MaxValue;

        /// <summary>
        ///   <para>Retrieves or sets the maximum number of cores that a node can have for the job to run on it.</para>
        /// </summary>
        /// <value>
        ///   <para>The maximum number of cores.</para>
        /// </value>
        public System.Int32 MaxCoresPerNode
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.MaxCoresPerNode, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_MaxCoresPerNode_ProfileDefault != null)
                {
                    return (System.Int32)_MaxCoresPerNode_ProfileDefault;
                }

                return _MaxCoresPerNode_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.MaxCoresPerNode, value);
                _props[JobPropertyIds.MaxCoresPerNode] = prop;
                _changeProps[JobPropertyIds.MaxCoresPerNode] = prop;
            }
        }

        string _SoftwareLicense_ProfileDefault = null;
        IStringCollection _SoftwareLicense_List = null;

        /// <summary>
        ///   <para>Retrieves or sets the software licensing requirements for the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The licenses that the job requires. The format is string:integer{,string: integer}, where each 
        /// string is the name of an application and each integer represents how many licenses are required.</para>
        /// </value>
        public IStringCollection SoftwareLicense
        {
            get
            {
                if (_SoftwareLicense_List == null)
                {
                    StoreProperty prop;
                    if (_props.TryGetValue(JobPropertyIds.SoftwareLicense, out prop))
                    {
                        _SoftwareLicense_List = Util.String2Collection((string)prop.Value);
                    }
                    else if (_SoftwareLicense_ProfileDefault != null)
                    {
                        _SoftwareLicense_List = Util.String2Collection(_SoftwareLicense_ProfileDefault);
                    }
                    else
                    {
                        _SoftwareLicense_List = new StringCollection();
                    }
                }

                return _SoftwareLicense_List;
            }

            set
            {
                _SoftwareLicense_List = value;
                string SoftwareLicenseValue = string.Empty;
                if (_SoftwareLicense_List != null)
                {
                    SoftwareLicenseValue = Util.Enumerable2String(_SoftwareLicense_List);
                }

                StoreProperty prop;
                if (!_props.TryGetValue(JobPropertyIds.SoftwareLicense, out prop))
                {
                    prop = null;
                }

                if (prop != null && (string)prop.Value == SoftwareLicenseValue)
                {
                    return;
                }

                if (prop == null && string.IsNullOrEmpty(SoftwareLicenseValue))
                {
                    return;
                }

                StoreProperty newProp = new StoreProperty(JobPropertyIds.SoftwareLicense, SoftwareLicenseValue);
                _props[JobPropertyIds.SoftwareLicense] = newProp;
                _changeProps[JobPropertyIds.SoftwareLicense] = newProp;
            }
        }

        System.String _ClientSource_ProfileDefault = null;
        System.String _ClientSource_Default = "";

        /// <summary>
        ///   <para>Retrieves the name of the process that created the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the process that created the job.</para>
        /// </value>
        public System.String ClientSource
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.ClientSource, out prop))
                {
                    return (System.String)prop.Value;
                }

                if (_ClientSource_ProfileDefault != null)
                {
                    return (System.String)_ClientSource_ProfileDefault;
                }

                return _ClientSource_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.ClientSource, value);
                _props[JobPropertyIds.ClientSource] = prop;
                _changeProps[JobPropertyIds.ClientSource] = prop;
            }
        }

        System.Int32? _Progress_ProfileDefault = null;
        System.Int32 _Progress_Default = 0;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.Int32 Progress
        {
            get
            {
                GetPropertyVersionCheck(JobPropertyIds.Progress);
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.Progress, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_Progress_ProfileDefault != null)
                {
                    return (System.Int32)_Progress_ProfileDefault;
                }

                return _Progress_Default;
            }

            set
            {
                SetPropertyVersionCheck(JobPropertyIds.Progress, value);
                StoreProperty prop = new StoreProperty(JobPropertyIds.Progress, value);
                _props[JobPropertyIds.Progress] = prop;
                _changeProps[JobPropertyIds.Progress] = prop;
            }
        }

        System.String _ProgressMessage_ProfileDefault = null;
        System.String _ProgressMessage_Default = "";

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.String ProgressMessage
        {
            get
            {
                GetPropertyVersionCheck(JobPropertyIds.ProgressMessage);
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.ProgressMessage, out prop))
                {
                    return (System.String)prop.Value;
                }

                if (_ProgressMessage_ProfileDefault != null)
                {
                    return (System.String)_ProgressMessage_ProfileDefault;
                }

                return _ProgressMessage_Default;
            }

            set
            {
                SetPropertyVersionCheck(JobPropertyIds.ProgressMessage, value);
                StoreProperty prop = new StoreProperty(JobPropertyIds.ProgressMessage, value);
                _props[JobPropertyIds.ProgressMessage] = prop;
                _changeProps[JobPropertyIds.ProgressMessage] = prop;
            }
        }

        System.Int32? _TargetResourceCount_ProfileDefault = null;
        System.Int32 _TargetResourceCount_Default = 0;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.Int32 TargetResourceCount
        {
            get
            {
                GetPropertyVersionCheck(JobPropertyIds.TargetResourceCount);
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.TargetResourceCount, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_TargetResourceCount_ProfileDefault != null)
                {
                    return (System.Int32)_TargetResourceCount_ProfileDefault;
                }

                return _TargetResourceCount_Default;
            }

            set
            {
                SetPropertyVersionCheck(JobPropertyIds.TargetResourceCount, value);
                StoreProperty prop = new StoreProperty(JobPropertyIds.TargetResourceCount, value);
                _props[JobPropertyIds.TargetResourceCount] = prop;
                _changeProps[JobPropertyIds.TargetResourceCount] = prop;
            }
        }

        System.Int32? _ExpandedPriority_ProfileDefault = null;
        System.Int32 _ExpandedPriority_Default = Microsoft.Hpc.Scheduler.Properties.ExpandedPriority.JobPriorityToExpandedPriority((int)JobPriority.Normal);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.Int32 ExpandedPriority
        {
            get
            {
                GetPropertyVersionCheck(JobPropertyIds.ExpandedPriority);
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.ExpandedPriority, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                if (_ExpandedPriority_ProfileDefault != null)
                {
                    return (System.Int32)_ExpandedPriority_ProfileDefault;
                }

                return _ExpandedPriority_Default;
            }

            set
            {
                SetPropertyVersionCheck(JobPropertyIds.ExpandedPriority, value);
                StoreProperty prop = new StoreProperty(JobPropertyIds.ExpandedPriority, value);
                _props[JobPropertyIds.ExpandedPriority] = prop;
                _changeProps[JobPropertyIds.ExpandedPriority] = prop;
            }
        }

        System.String _ServiceName_ProfileDefault = null;
        System.String _ServiceName_Default = "";

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.String ServiceName
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.ServiceName, out prop))
                {
                    return (System.String)prop.Value;
                }

                if (_ServiceName_ProfileDefault != null)
                {
                    return (System.String)_ServiceName_ProfileDefault;
                }

                return _ServiceName_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.ServiceName, value);
                _props[JobPropertyIds.ServiceName] = prop;
                _changeProps[JobPropertyIds.ServiceName] = prop;
            }
        }

        System.String _JobTemplate_Default = "Default";

        /// <summary>
        ///   <para>Retrieves the name of the job template used to initialize the properties of the job.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the job template.</para>
        /// </value>
        public System.String JobTemplate
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.JobTemplate, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _JobTemplate_Default;
            }
        }

        System.DateTime _HoldUntil_Default = DateTime.MinValue;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.DateTime HoldUntil
        {
            get
            {
                GetPropertyVersionCheck(JobPropertyIds.HoldUntil);
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.HoldUntil, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _HoldUntil_Default;
            }
        }

        System.Boolean? _NotifyOnStart_ProfileDefault = null;
        System.Boolean _NotifyOnStart_Default = false;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.Boolean NotifyOnStart
        {
            get
            {
                GetPropertyVersionCheck(JobPropertyIds.NotifyOnStart);
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.NotifyOnStart, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_NotifyOnStart_ProfileDefault != null)
                {
                    return (System.Boolean)_NotifyOnStart_ProfileDefault;
                }

                return _NotifyOnStart_Default;
            }

            set
            {
                SetPropertyVersionCheck(JobPropertyIds.NotifyOnStart, value);
                StoreProperty prop = new StoreProperty(JobPropertyIds.NotifyOnStart, value);
                _props[JobPropertyIds.NotifyOnStart] = prop;
                _changeProps[JobPropertyIds.NotifyOnStart] = prop;
            }
        }

        System.Boolean? _NotifyOnCompletion_ProfileDefault = null;
        System.Boolean _NotifyOnCompletion_Default = false;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.Boolean NotifyOnCompletion
        {
            get
            {
                GetPropertyVersionCheck(JobPropertyIds.NotifyOnCompletion);
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.NotifyOnCompletion, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                if (_NotifyOnCompletion_ProfileDefault != null)
                {
                    return (System.Boolean)_NotifyOnCompletion_ProfileDefault;
                }

                return _NotifyOnCompletion_Default;
            }

            set
            {
                SetPropertyVersionCheck(JobPropertyIds.NotifyOnCompletion, value);
                StoreProperty prop = new StoreProperty(JobPropertyIds.NotifyOnCompletion, value);
                _props[JobPropertyIds.NotifyOnCompletion] = prop;
                _changeProps[JobPropertyIds.NotifyOnCompletion] = prop;
            }
        }

        IStringCollection _ExcludedNodes_List = null;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public IStringCollection ExcludedNodes
        {
            get
            {
                GetPropertyVersionCheck(JobPropertyIds.ExcludedNodes);
                if (_ExcludedNodes_List == null)
                {
                    StoreProperty prop;
                    if (_props.TryGetValue(JobPropertyIds.ExcludedNodes, out prop))
                    {
                        _ExcludedNodes_List = Util.String2Collection((string)prop.Value, true);
                    }
                    else
                    {
                        _ExcludedNodes_List = new StringCollection(true);
                    }
                }

                return _ExcludedNodes_List;
            }
        }

        System.String _EmailAddress_Default = null;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.String EmailAddress
        {
            get
            {
                GetPropertyVersionCheck(JobPropertyIds.EmailAddress);
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.EmailAddress, out prop))
                {
                    return (System.String)prop.Value;
                }


                return _EmailAddress_Default;
            }

            set
            {
                SetPropertyVersionCheck(JobPropertyIds.EmailAddress, value);
                StoreProperty prop = new StoreProperty(JobPropertyIds.EmailAddress, value);
                _props[JobPropertyIds.EmailAddress] = prop;
                _changeProps[JobPropertyIds.EmailAddress] = prop;
            }
        }

        System.String _Pool_ProfileDefault = null;
        System.String _Pool_Default = "Default";

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.String Pool
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.Pool, out prop))
                {
                    return (System.String)prop.Value;
                }

                if (_Pool_ProfileDefault != null)
                {
                    return (System.String)_Pool_ProfileDefault;
                }

                return _Pool_Default;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.JobRuntimeType _RuntimeType_Default = 0;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.JobRuntimeType RuntimeType
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.RuntimeType, out prop))
                {
                    return (Microsoft.Hpc.Scheduler.Properties.JobRuntimeType)prop.Value;
                }

                return _RuntimeType_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.RuntimeType, value);
                _props[JobPropertyIds.RuntimeType] = prop;
                _changeProps[JobPropertyIds.RuntimeType] = prop;
            }
        }

        System.String _JobValidExitCodes_Default = "";

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
                if (_props.TryGetValue(JobPropertyIds.JobValidExitCodes, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _JobValidExitCodes_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.JobValidExitCodes, value);
                _props[JobPropertyIds.JobValidExitCodes] = prop;
                _changeProps[JobPropertyIds.JobValidExitCodes] = prop;
            }
        }

        bool _SingleNode_Default = false;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="System.Boolean" />.</para>
        /// </value>
        public bool SingleNode
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.SingleNode, out prop))
                {
                    return (bool)prop.Value;
                }

                return _SingleNode_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.SingleNode, value);
                _props[JobPropertyIds.SingleNode] = prop;
                _changeProps[JobPropertyIds.SingleNode] = prop;
            }
        }


        JobNodeGroupOp _NodeGroupOp_Default = JobNodeGroupOp.Intersect;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.Properties.JobNodeGroupOp" />.</para>
        /// </value>
        public JobNodeGroupOp NodeGroupOp
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.NodeGroupOp, out prop))
                {
                    if (prop.Value != null)
                    {
                        JobNodeGroupOp nodeGroupOp = (JobNodeGroupOp)prop.Value;
                        return nodeGroupOp;
                    }
                }
                return _NodeGroupOp_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.NodeGroupOp, value);
                _props[JobPropertyIds.NodeGroupOp] = prop;
                _changeProps[JobPropertyIds.NodeGroupOp] = prop;
            }
        }


        IIntCollection _ParentJobIds_List = null;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.IIntCollection" />.</para>
        /// </value>
        public IIntCollection ParentJobIds
        {
            get
            {
                if (_ParentJobIds_List == null)
                {
                    StoreProperty prop;
                    if (_props.TryGetValue(JobPropertyIds.ParentJobIds, out prop))
                    {
                        _ParentJobIds_List = Util.String2IntCollection((string)prop.Value);
                    }
                    else
                    {
                        _ParentJobIds_List = new IntCollection();
                    }
                }

                return _ParentJobIds_List;
            }

            set
            {
                _ParentJobIds_List = value;
                string DependsOnValue = string.Empty;
                if (_ParentJobIds_List != null)
                {
                    DependsOnValue = Util.EnumerableInt2String(_ParentJobIds_List);
                }

                StoreProperty prop;
                if (!_props.TryGetValue(JobPropertyIds.ParentJobIds, out prop))
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

                StoreProperty newProp = new StoreProperty(JobPropertyIds.ParentJobIds, DependsOnValue);
                _props[JobPropertyIds.ParentJobIds] = newProp;
                _changeProps[JobPropertyIds.ParentJobIds] = newProp;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.IIntCollection" />.</para>
        /// </value>
        public IIntCollection ChildJobIds
        {
            get
            {
                // When a scheduler job object is created, it cannot get the value for JobPropertyIds.ChildJobIds
                // the value for JobPropertyIds.ChildJobIds needs to be updated after dependency information is
                // submitted to server. Thus, the accurate value must be read from server each time when users request it

                PropertyRow row = _job.GetProps(new PropertyId[] { JobPropertyIds.ChildJobIds });
                IIntCollection _ChildJobIds_List = Util.String2IntCollection((string)(row.Props[0].Value));

                return _ChildJobIds_List;
            }
        }


        bool _FailDependentTasks_Default = false;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="System.Boolean" />.</para>
        /// </value>
        public bool FailDependentTasks
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.FailDependentTasks, out prop))
                {
                    return (bool)prop.Value;
                }

                return _FailDependentTasks_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.FailDependentTasks, value);
                _props[JobPropertyIds.FailDependentTasks] = prop;
                _changeProps[JobPropertyIds.FailDependentTasks] = prop;
            }
        }


        int _EstimatedProcessMemory_Default = 0;
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="System.Int32" />.</para>
        /// </value>
        public int EstimatedProcessMemory
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.EstimatedProcessMemory, out prop))
                {
                    if (prop.Value != null)
                    {
                        return (int)prop.Value;
                    }
                }

                return _EstimatedProcessMemory_Default;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.EstimatedProcessMemory, value);
                _props[JobPropertyIds.EstimatedProcessMemory] = prop;
                _changeProps[JobPropertyIds.EstimatedProcessMemory] = prop;
            }

        }


        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int PlannedCoreCount
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.PlannedCoreCount, out prop))
                {
                    if (prop.Value != null)
                    {
                        return (int)prop.Value;
                    }
                }

                return PropertyUtil.DefaultPlannedCoreCount;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int TaskExecutionFailureRetryLimit
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(JobPropertyIds.TaskExecutionFailureRetryLimit, out prop))
                {
                    if (prop.Value != null)
                    {
                        return (int)prop.Value;
                    }
                }

                return PropertyUtil.DefaultTaskExecutionFailureRetryLimit;
            }

            set
            {
                StoreProperty prop = new StoreProperty(JobPropertyIds.TaskExecutionFailureRetryLimit, value);
                _props[JobPropertyIds.TaskExecutionFailureRetryLimit] = prop;
                _changeProps[JobPropertyIds.TaskExecutionFailureRetryLimit] = prop;
            }
        }
    }
}

