// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;

namespace Microsoft.Hpc.Scheduler.Session.Data
{
        #region For The Node State event: Its event arg, interface and handler
        /// <summary>
        ///   <para>Defines the parameters that are passed to the 
        /// <see cref="Microsoft.Hpc.Scheduler.NodeStateHandler" /> event handler when the state of a node changes.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For information about how to implement the event handler, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.NodeStateHandler" /> delegate. This event handler is called when the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OnNodeState" /> event occurs.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OnNodeState" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.NodeStateHandler" />
        public interface INodeStateEventArg
        {
            /// <summary>
            ///   <para>Gets the numeric identifier of the node for which the state changed.</para>
            /// </summary>
            /// <value>
            ///   <para>A <see cref="System.Int32" /> that indicates the numeric identifier of the node for which the state changed.</para>
            /// </value>
            /// <remarks>
            ///   <para>To get information about the node for which the state changed, use the 
            /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNode(System.Int32)" /> method with the nodeId parameter set to the 
            /// <see cref="Microsoft.Hpc.Scheduler.INodeStateEventArg.NodeId" /> property to get an 
            /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode" /> interface for the node.</para>
            /// </remarks>
            /// <seealso cref="Microsoft.Hpc.Scheduler.INodeStateEventArg.PreviousState" />
            /// <seealso cref="Microsoft.Hpc.Scheduler.INodeStateEventArg.NewState" />
            /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode" />
            /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNode(System.Int32)" />
            int NodeId { get; }
            /// <summary>
            ///   <para>Gets the new state of a node when the state of the node changes.</para>
            /// </summary>
            /// <value>
            ///   <para>A value from the <see cref="Microsoft.Hpc.Scheduler.Properties.NodeState" /> enumeration that indicates the new state of the node.</para>
            /// </value>
            NodeState NewState { get; }
            /// <summary>
            ///   <para>Gets the previous state of a node when the state of the node changes.</para>
            /// </summary>
            /// <value>
            ///   <para>A value from the 
            /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodeState" /> enumeration that indicates the previous state of the node.</para>
            /// </value>
            NodeState PreviousState { get; }
        }


        /// <summary>
        ///   <para />
        /// </summary>
        public class NodeStateEventArg : EventArgs, INodeStateEventArg
        {

            int _nodeId;
            NodeState _newState;
            NodeState _previousState;


            internal NodeStateEventArg(int nodeId, NodeState newState, NodeState previousState)
            {
                _nodeId = nodeId;
                _newState = newState;
                _previousState = previousState;

            }

            #region INodeStateEventArg Members

            /// <summary>
            ///   <para />
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            public int NodeId
            {
                get { return _nodeId; }
            }

            /// <summary>
            ///   <para />
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            public NodeState NewState
            {
                get { return _newState; }
            }

            /// <summary>
            ///   <para />
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            public NodeState PreviousState
            {
                get { return _previousState; }
            }

            #endregion

        }


        /// <summary>
        ///   <para>Defines the delegate that your application implements when you subscribe to the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OnNodeState" /> event.</para>
        /// </summary>
        /// <param name="sender">
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.SchedulerNode" /> object that represents the node for which the state changed. Cast the object to an 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode" /> interface.</para>
        /// </param>
        /// <param name="arg">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INodeStateEventArg" /> interface that provides information that is related to the change in the state of the node. </para> 
        /// </param>
        /// <remarks>
        ///   <para>To get the node, cast the object in the <paramref name="sender" /> parameter to an 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode" /> interface.</para>
        ///   <para>To determine the previous and new states of the node, cast the object in the args parameter to an 
        /// <see cref="Microsoft.Hpc.Scheduler.INodeStateEventArg" /> interface and get the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.INodeStateEventArg.PreviousState" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.INodeStateEventArg.NewState" /> properties.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OnNodeState" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.INodeStateEventArg" />
        public delegate void NodeStateHandler(object sender, NodeStateEventArg arg);

    
        public interface ISchedulerNodeEvents
        {
            /// <summary>
            ///   <para>Receives information when the state of the node changes.</para>
            /// </summary>
            /// <param name="sender">
            ///   <para>A SchedulerNode object for the node for which the state changed.</para>
            /// </param>
            /// <param name="arg">
            ///   <para>A NodeStateEventArg object that provides information that is related to the change in the state of the node. </para>
            /// </param>
            void OnNodeState(object sender, NodeStateEventArg arg);
        }

        #endregion

        #region The Node Reachable event: Its event arg, interface and handler

        public interface INodeReachableEventArg
        {
            /// <summary>
            ///   <para>Gets the numeric identifier of the node for which the HPC Node Manager Service became reachable or unreachable.</para>
            /// </summary>
            /// <value>
            ///   <para>An 
            /// 
            /// <see cref="System.Int32" /> that indicates the numeric identifier of the node for which the HPC Node Manager Service became reachable or unreachable.</para> 
            /// </value>
            /// <remarks>
            ///   <para>To get information about the node for which the HPC Node Manager Service became reachable or unreachable, use the 
            /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNode(System.Int32)" /> method with the nodeId parameter set to the 
            /// <see cref="Microsoft.Hpc.Scheduler.INodeReachableEventArg.NodeId" /> property to get an 
            /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode" /> interface for the node.</para>
            /// </remarks>
            /// <seealso cref="Microsoft.Hpc.Scheduler.INodeReachableEventArg.Reachable" />
            /// <seealso cref="Microsoft.Hpc.Scheduler.IScheduler.OpenNode(System.Int32)" />
            /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode" />
            int NodeId { get; }
            /// <summary>
            ///   <para>Gets whether the HPC Node Manager Service on the node that the 
            /// <see cref="Microsoft.Hpc.Scheduler.INodeReachableEventArg.NodeId" /> property identifies became reachable or unreachable.</para>
            /// </summary>
            /// <value>
            ///   <para>A 
            /// <see cref="System.Boolean" /> that indicates whether the HPC Node Manager Service on the node became reachable or unreachable. 
            /// True indicates that the HPC Node Manager Service on the node became reachable. 
            /// False indicates that HPC Node Manager Service on the node became unreachable.</para>
            /// </value>
            bool Reachable { get; }
        }

  
        public class NodeReachableEventArg : EventArgs, INodeReachableEventArg
        {
            int _nodeId;
            bool _reachable = false;

            internal NodeReachableEventArg(int nodeId, bool reachability)
            {
                _nodeId = nodeId;
                _reachable = reachability;
            }

            #region INodeReachableEventArg Members

            /// <summary>
            ///   <para />
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            public int NodeId
            {
                get { return _nodeId; }
            }

            /// <summary>
            ///   <para />
            /// </summary>
            /// <value>
            ///   <para />
            /// </value>
            public bool Reachable
            {
                get { return _reachable; }
            }

            #endregion
        }

        /// <summary>
        ///   <para>Defines the delegate that your application implements when you subscribe to the 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OnNodeReachable" /> event.</para>
        /// </summary>
        /// <param name="sender">
        ///   <para>A 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.SchedulerNode" /> object that represents the node for which the HPC Node Manager Service on the node became reachable or unreachable. Cast the object to an  
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode" /> interface.</para>
        /// </param>
        /// <param name="arg">
        ///   <para>An 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.INodeReachableEventArg" /> interface that provides information that is related to the HPC Node Manager Service on the node becoming reachable or unreachable.</para> 
        /// </param>
        /// <remarks>
        ///   <para>To get the node, cast the object in the <paramref name="sender" /> parameter to an 
        /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode" /> interface. </para>
        ///   <para>To determine whether the HPC Node Manager Service on the node 
        /// became reachable or unreachable, cast the object in the args parameter to an  
        /// <see cref="Microsoft.Hpc.Scheduler.INodeReachableEventArg" /> interface and get the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.INodeReachableEventArg.Reachable" /> property.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode.OnNodeReachable" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.INodeReachableEventArg" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.INodeReachableEventArg.Reachable" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerNode" />
        public delegate void NodeReachableHandler(object sender, NodeReachableEventArg arg);

        public interface ISchedulerNodeReachableEvents
        {
            /// <summary>
            ///   <para>Receives information when the HPC Node Manager Service on a node becomes reachable or unreachable.</para>
            /// </summary>
            /// <param name="sender">
            ///   <para>A 
            /// 
            /// <see cref="Microsoft.Hpc.Scheduler.SchedulerNode" /> object that represents the node for which the HPC Node Manager Service on the node became reachable or unreachable.</para> 
            /// </param>
            /// <param name="arg">
            ///   <para>A 
            /// 
            /// <see cref="Microsoft.Hpc.Scheduler.NodeReachableEventArg" /> object that provides information that is related to the HPC Node Manager Service on the node becoming reachable or unreachable.</para> 
            /// </param>
            void OnNodeReachable(object sender, NodeReachableEventArg arg);
        }

        #endregion
}
