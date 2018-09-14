using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Hpc.Scheduler.Store;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler
{


    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidINodeV3)]
    public interface ISchedulerNodeV3
    {
        #region V2 node methods Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        void Refresh();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerCollection GetCores();

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        ISchedulerNodeCounters GetCounters();

        #endregion

        #region V2 Node properties Don't Change

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 Id { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.String Name { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.JobType JobType { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        Microsoft.Hpc.Scheduler.Properties.NodeState State { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean Reachable { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 NumberOfCores { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 NumberOfSockets { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime OfflineTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.DateTime OnlineTime { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Boolean MoveToOffline { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Guid Guid { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int64 MemorySize { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        System.Int32 CpuSpeed { get; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        IStringCollection NodeGroups { get; }

        #endregion

        #region v3 methods Don't change

        /// <summary>
        ///   <para />
        /// </summary>
        event EventHandler<NodeStateEventArg> OnNodeState;

        #endregion

    }

}
