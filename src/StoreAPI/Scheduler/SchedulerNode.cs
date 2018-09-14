using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Hpc.Scheduler.Store;
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
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidNodeClass)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(ISchedulerNodeEvents), typeof(ISchedulerNodeReachableEvents))]
    public partial class SchedulerNode : ISchedulerNode, ISchedulerNodeV3SP2, ISchedulerNodeV3SP1, ISchedulerNodeV3, ISchedulerNodeV2
    {
        /// <summary>
        ///   <para>The internal node object.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected IClusterNode _node;
        ISchedulerStore _store;

        internal protected SchedulerNode(ISchedulerStore store)
        {
            _store = store;
        }

        internal protected SchedulerNode(ISchedulerStore store, PropertyRow row)
        {
            _store = store;

            _InitFromRow(row);

            StoreProperty prop = row[NodePropertyIds.NodeObject];

            if (prop != null)
            {
                _node = (IClusterNode)prop.Value;
            }
        }

        internal protected void Init(IClusterNode node, PropertyId[] pids)
        {
            _node = node;

            InitFromObject(_node, pids);
        }

        /// <summary>
        ///   <para>Refreshes this copy of the node object with the contents from the server.</para>
        /// </summary>
        public void Refresh()
        {
            InitFromObject(_node, GetCurrentPropsToReload());
            //reset the string lists
            _NodeGroups_List = null;
        }

        /// <summary>
        ///   <para>Retrieves the counter data for the node.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.ISchedulerNodeCounters" /> interface that contains the counter data.</para>
        /// </returns>
        public ISchedulerNodeCounters GetCounters()
        {
            SchedulerNodeCounters counters = new SchedulerNodeCounters(_node);

            counters.Refresh();

            return counters;
        }

        /// <summary>
        ///   <para>Retrieves the state information for each core on the node.</para>
        /// </summary>
        /// <returns>
        ///   <para>An 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCollection" /> interface that contains a collection of 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerCore" /> interfaces.</para>
        /// </returns>
        public ISchedulerCollection GetCores()
        {
            List<ISchedulerCore> result = new List<ISchedulerCore>();

            using (IRowEnumerator enm = _store.OpenResourceRowEnumerator())
            {
                enm.SetFilter(new FilterProperty(FilterOperator.Equal, ResourcePropertyIds.NodeId, Id));

                enm.SetColumns(
                        ResourcePropertyIds.Id,
                        ResourcePropertyIds.JobId,
                        ResourcePropertyIds.JobTaskId,
                        ResourcePropertyIds.State,
                        ResourcePropertyIds.MoveToOffline,
                        ResourcePropertyIds.CoreId
                        );

                foreach (PropertyRow row in enm)
                {
                    SchedulerCore proc = new SchedulerCore();

                    proc.InitFromRow(row);

                    result.Add(proc);
                }
            }

            return new SchedulerCollection<ISchedulerCore>(result);
        }

        object _eventLock = new object();
        NodeStateHandler _onNodeStateChange = null;

        event EventHandler<NodeStateEventArg> ISchedulerNodeV3SP1.OnNodeState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeState += (NodeStateHandler)Delegate.CreateDelegate(typeof(NodeStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeState -= (NodeStateHandler)Delegate.CreateDelegate(typeof(NodeStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<NodeStateEventArg> ISchedulerNodeV3.OnNodeState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeState += (NodeStateHandler)Delegate.CreateDelegate(typeof(NodeStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeState -= (NodeStateHandler)Delegate.CreateDelegate(typeof(NodeStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<NodeStateEventArg> ISchedulerNodeV3SP2.OnNodeState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeState += (NodeStateHandler)Delegate.CreateDelegate(typeof(NodeStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeState -= (NodeStateHandler)Delegate.CreateDelegate(typeof(NodeStateHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<NodeStateEventArg> ISchedulerNode.OnNodeState
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeState += (NodeStateHandler)Delegate.CreateDelegate(typeof(NodeStateHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeState -= (NodeStateHandler)Delegate.CreateDelegate(typeof(NodeStateHandler), value.Target, value.Method);
                }
            }
        }



        /// <summary>
        ///   <para />
        /// </summary>
        public event NodeStateHandler OnNodeState
        {
            add
            {
                //when the first event is registered with schedulernode, 
                // register an event with the store to get node
                // state change event handler
                lock (_eventLock)
                {
                    if (_onNodeStateChange == null)
                    {
                        _store.NodeEvent += StoreNodeEventHandler;
                    }
                    _onNodeStateChange += value;
                }
            }

            remove
            {
                lock (_eventLock)
                {
                    _onNodeStateChange -= value;

                    if (_onNodeStateChange == null)
                    {
                        _store.NodeEvent -= StoreNodeEventHandler;
                    }
                }
            }
        }


        void StoreNodeEventHandler(Int32 id, EventType eventType, StoreProperty[] props)
        {
            if (eventType != EventType.Modify || props == null)
            {
                return;
            }

            if (id == Id)
            {
                StoreProperty newState = null;
                StoreProperty previousState = null;
                foreach (StoreProperty prop in props)
                {
                    if (prop.Id == NodePropertyIds.State)
                    {
                        newState = prop;
                    }
                    else if (prop.Id == NodePropertyIds.PreviousState)
                    {
                        previousState = prop;
                    }
                }

                if (newState != null && previousState != null)
                {
                    NodeStateHandler handler = _onNodeStateChange;
                    if (handler != null)
                    {
                        handler.Invoke(this, new NodeStateEventArg(Id, (NodeState)newState.Value, (NodeState)previousState.Value));
                    }
                }

            }

        }


        NodeReachableHandler _onNodeReachable = null;

        event EventHandler<NodeReachableEventArg> ISchedulerNodeV3SP1.OnNodeReachable
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeReachable += (NodeReachableHandler)Delegate.CreateDelegate(typeof(NodeReachableHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeReachable -= (NodeReachableHandler)Delegate.CreateDelegate(typeof(NodeReachableHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<NodeReachableEventArg> ISchedulerNodeV3SP2.OnNodeReachable
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeReachable += (NodeReachableHandler)Delegate.CreateDelegate(typeof(NodeReachableHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeReachable -= (NodeReachableHandler)Delegate.CreateDelegate(typeof(NodeReachableHandler), value.Target, value.Method);
                }
            }
        }

        event EventHandler<NodeReachableEventArg> ISchedulerNode.OnNodeReachable
        {
            add
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeReachable += (NodeReachableHandler)Delegate.CreateDelegate(typeof(NodeReachableHandler), value.Target, value.Method);
                }
            }

            remove
            {
                if (value != null)
                {
                    ((SchedulerNode)this).OnNodeReachable -= (NodeReachableHandler)Delegate.CreateDelegate(typeof(NodeReachableHandler), value.Target, value.Method);
                }
            }
        }


        /// <summary>
        ///   <para />
        /// </summary>
        public event NodeReachableHandler OnNodeReachable
        {
            add
            {
                //when the first reachable event handler is registered with scheduler node 
                //register an event handler with the store that will give us all 
                //the changes in the node's reachability
                lock (_eventLock)
                {
                    if (_onNodeReachable == null)
                    {
                        _store.NodeEvent += StoreNodeReachableEventHandler;
                    }
                    _onNodeReachable += value;
                }
            }

            remove
            {
                //when the last user event handler has been removed, deregister our 
                //reachable handler from the store
                lock (_eventLock)
                {
                    _onNodeReachable -= value;

                    if (_onNodeReachable == null)
                    {
                        _store.NodeEvent -= StoreNodeReachableEventHandler;
                    }
                }
            }
        }

        /// <summary>
        /// The event handler registered with the store to find changes to the reachable status of the
        /// node and call the user node reachable event handlers
        /// </summary>
        /// <param name="id">node id</param>
        /// <param name="eventType">Type of event</param>
        /// <param name="props">The properties that changed</param>
        void StoreNodeReachableEventHandler(Int32 id, EventType eventType, StoreProperty[] props)
        {
            if (eventType != EventType.Modify || props == null)
            {
                return;
            }

            if (id == Id)
            {
                //if we find the reachable property among the properties that changed
                //we call the user node reachable event handlers
                StoreProperty reachableProp = null;
                foreach (StoreProperty prop in props)
                {
                    if (prop.Id == NodePropertyIds.Reachable)
                    {
                        reachableProp = prop;
                    }
                }

                if (reachableProp != null)
                {
                    NodeReachableHandler handler = _onNodeReachable;
                    if (handler != null)
                    {
                        handler.Invoke(this, new NodeReachableEventArg(Id, (bool)reachableProp.Value));
                    }
                }

            }

        }


        static PropertyId[] _enumInitIds = null;

        internal static PropertyId[] EnumInitIds
        {
            get
            {
                if (_enumInitIds == null)
                {
                    SchedulerNode sampleNode = new SchedulerNode((ISchedulerStore)null);

                    List<PropertyId> ids = new List<PropertyId>(sampleNode.GetPropertyIds());

                    ids.Add(NodePropertyIds.NodeObject);

                    _enumInitIds = ids.ToArray();
                }

                return _enumInitIds;
            }
        }

    }

}
