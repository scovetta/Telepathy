
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler
{

    /// <summary>
    ///   <para>Contains information about a compute node.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode" /> interface.</para>
    /// </remarks>
    /// <example />
    public partial class SchedulerNode : SchedulerObjectBase, ISchedulerNode, ISchedulerNodeV3SP1, ISchedulerNodeV3, ISchedulerNodeV2
    {


        /// <summary>
        ///   <para>An array or properties to get from the store when refreshing the node object.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        internal protected override PropertyId[] GetPropertyIds()
        {
            PropertyId[] pids =
            {
                NodePropertyIds.Id,
                NodePropertyIds.Name,
                NodePropertyIds.JobType,
                NodePropertyIds.State,
                NodePropertyIds.Reachable,
                NodePropertyIds.NumCores,
                NodePropertyIds.NumSockets,
                NodePropertyIds.OfflineTime,
                NodePropertyIds.OnlineTime,
                NodePropertyIds.MoveToOffline,
                NodePropertyIds.Guid,
                NodePropertyIds.MemorySize,
                NodePropertyIds.CpuSpeed,
                NodePropertyIds.Availability,
                NodePropertyIds.Location,
                NodePropertyIds.AzureServiceName,
                NodePropertyIds.DnsSuffix,
                NodePropertyIds.Affinity,
                NodePropertyIds.AzureLoadBalancerAddress
            };
            return pids;
        }


        System.Int32 _Id_Default = -1;

        /// <summary>
        ///   <para>Retrieves the identifier that uniquely identifies the node in the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>The sequential, numeric identifier given to the node when it was added to the cluster.</para>
        /// </value>
        public System.Int32 Id
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.Id, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _Id_Default;
            }
        }

        System.String _Name_Default = "";

        /// <summary>
        ///   <para>Retrieves the computer name of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the node.</para>
        /// </value>
        public System.String Name
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.Name, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _Name_Default;
            }
        }

        System.String _DnsSuffix_Default = "";

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public System.String DnsSuffix
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.DnsSuffix, out prop))
                {
                    return (System.String)prop.Value;
                }

                return _DnsSuffix_Default;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.JobType _JobType_Default = JobType.Batch;

        /// <summary>
        ///   <para>Retrieves the types of jobs that this node is configured to run.</para>
        /// </summary>
        /// <value>
        ///   <para>The types of jobs that this node can run. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobType" /> enumeration.</para>
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.JobType JobType
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.JobType, out prop))
                {
                    return (Microsoft.Hpc.Scheduler.Properties.JobType)prop.Value;
                }

                return _JobType_Default;
            }
        }

        Microsoft.Hpc.Scheduler.Properties.NodeState _State_Default = NodeState.Offline;

        /// <summary>
        ///   <para>Retrieves the current state of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The state of the node. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.NodeState" /> enumeration.</para>
        /// </value>
        public Microsoft.Hpc.Scheduler.Properties.NodeState State
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.State, out prop))
                {
                    return (Microsoft.Hpc.Scheduler.Properties.NodeState)prop.Value;
                }

                return _State_Default;
            }
        }

        System.Boolean _Reachable_Default = false;

        /// <summary>
        ///   <para>Determines whether the server thinks the node is reachable.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the node is reachable; otherwise, False.</para>
        /// </value>
        public System.Boolean Reachable
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.Reachable, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                return _Reachable_Default;
            }
        }

        System.Int32 _NumCores_Default = 1;

        /// <summary>
        ///   <para>Retrieves the number of cores on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores on the node.</para>
        /// </value>
        public System.Int32 NumberOfCores
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.NumCores, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _NumCores_Default;
            }
        }

        System.Int32 _NumSockets_Default = 1;

        /// <summary>
        ///   <para>Retrieves the number of sockets on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of sockets.</para>
        /// </value>
        public System.Int32 NumberOfSockets
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.NumSockets, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _NumSockets_Default;
            }
        }

        System.DateTime _OfflineTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the date and time that the node last went offline.</para>
        /// </summary>
        /// <value>
        ///   <para>The last time that the node went offline. </para>
        /// </value>
        public System.DateTime OfflineTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.OfflineTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _OfflineTime_Default;
            }
        }

        System.DateTime _OnlineTime_Default = DateTime.MinValue;

        /// <summary>
        ///   <para>Retrieves the date and time that the node last came online.</para>
        /// </summary>
        /// <value>
        ///   <para>The last time that the node came online.</para>
        /// </value>
        public System.DateTime OnlineTime
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.OnlineTime, out prop))
                {
                    return (System.DateTime)prop.Value;
                }

                return _OnlineTime_Default;
            }
        }

        System.Boolean _MoveToOffline_Default = false;

        /// <summary>
        ///   <para>Determines if a user requested that the node go offline.</para>
        /// </summary>
        /// <value>
        ///   <para>Is True if the user has requested that the node go offline; otherwise False.</para>
        /// </value>
        public System.Boolean MoveToOffline
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.MoveToOffline, out prop))
                {
                    return (System.Boolean)prop.Value;
                }

                return _MoveToOffline_Default;
            }
        }

        System.Guid _Guid_Default = Guid.Empty;

        /// <summary>
        ///   <para>Retrieves the globally unique identifier that uniquely identifies the node in the system.</para>
        /// </summary>
        /// <value>
        ///   <para>The node's system identifier.</para>
        /// </value>
        public System.Guid Guid
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.Guid, out prop))
                {
                    return (System.Guid)prop.Value;
                }

                return _Guid_Default;
            }
        }

        System.Int64 _MemorySize_Default = 0;

        /// <summary>
        ///   <para>Retrieves the size of memory.</para>
        /// </summary>
        /// <value>
        ///   <para>The size of memory, in bytes.</para>
        /// </value>
        public System.Int64 MemorySize
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.MemorySize, out prop))
                {
                    return (System.Int64)prop.Value;
                }

                return _MemorySize_Default;
            }
        }

        NodeAvailability _Availability_Default = NodeAvailability.AlwaysOn;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public NodeAvailability Availability
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.Availability, out prop))
                {
                    return (NodeAvailability)prop.Value;
                }

                return _Availability_Default;
            }
        }

        NodeLocation _Location_Default = NodeLocation.OnPremise;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public NodeLocation Location
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.Location, out prop))
                {
                    return (NodeLocation)prop.Value;
                }

                return _Location_Default;
            }
        }

        string _AzureServiceName_Default = null;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string AzureServiceName
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.AzureServiceName, out prop))
                {
                    return prop.Value as string;
                }

                return _AzureServiceName_Default;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string AzureLoadBalancerAddress
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.AzureLoadBalancerAddress, out prop))
                {
                    return prop.Value as string;
                }

                return null;
            }
        }

        System.Int32 _CpuSpeed_Default = 1;

        /// <summary>
        ///   <para>Retrieves the processor speed of the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The processor speed, in MHz.</para>
        /// </value>
        public System.Int32 CpuSpeed
        {
            get
            {
                StoreProperty prop;
                if (_props.TryGetValue(NodePropertyIds.CpuSpeed, out prop))
                {
                    return (System.Int32)prop.Value;
                }

                return _CpuSpeed_Default;
            }
        }

        IStringCollection _NodeGroups_List = null;

        /// <summary>
        ///   <para>Retrieves the list of node groups to which this node belongs.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains the collection of node group names.</para>
        /// </value>
        public IStringCollection NodeGroups
        {
            get
            {
                if (_NodeGroups_List == null)
                {
                    LoadDeferredProp(_node, NodePropertyIds.NodeGroups);

                    StoreProperty prop;
                    if (_props.TryGetValue(NodePropertyIds.NodeGroups, out prop))
                    {
                        _NodeGroups_List = Util.String2Collection((string)prop.Value, true);
                    }
                    else
                    {
                        _NodeGroups_List = new StringCollection(true);
                    }
                }

                return _NodeGroups_List;
            }
        }
    }

}

