using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;


namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Contains counter data information for the node.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call the <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.GetCounters" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerCounters" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJobCounters" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerTaskCounters" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidINodeCounters)]
    public interface ISchedulerNodeCounters
    {
        /// <summary>
        ///   <para>Retrieves the number of cores on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores on the node.</para>
        /// </value>
        int NumberOfCores { get; }

        /// <summary>
        ///   <para>Retrieves the number of sockets on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of sockets on the node.</para>
        /// </value>
        int NumberOfSockets { get; }

        /// <summary>
        ///   <para>Retrieves the number of cores on the node that are offline.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are offline.</para>
        /// </value>
        int OfflineCoreCount { get; }

        /// <summary>
        ///   <para>Retrieves the number of cores on the node that are idle.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are idle.</para>
        /// </value>
        int IdleCoreCount { get; }

        /// <summary>
        ///   <para>Retrieves the number of cores on the node that are busy processing jobs.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are busy processing jobs.</para>
        /// </value>
        int BusyCoreCount { get; }
    }

    /// <summary>
    ///   <para>Contains counter data information for the node.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerNodeCounters" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidNodeCountersClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class SchedulerNodeCounters : ISchedulerNodeCounters
    {
        int _NumberOfCores = 0;
        int _NumberOfSockets = 0;
        int _OfflineCoreCount = 0;
        int _IdleCoreCount = 0;
        int _BusyCoreCount = 0;

        IClusterStoreObject _item = null;

        internal SchedulerNodeCounters(IClusterStoreObject item)
        {
            _item = item;
        }

        static PropertyId[] _pids =
        {
            NodePropertyIds.NumCores,
            NodePropertyIds.NumSockets,
            NodePropertyIds.OfflineResourceCount,
            NodePropertyIds.IdleResourceCount,
        };

        internal void Refresh()
        {
            PropertyRow row = _item.GetProps(_pids);

            _NumberOfCores = GetValueFromRow(row, NodePropertyIds.NumCores);
            _NumberOfSockets = GetValueFromRow(row, NodePropertyIds.NumSockets);
            _OfflineCoreCount = GetValueFromRow(row, NodePropertyIds.OfflineResourceCount);
            _IdleCoreCount = GetValueFromRow(row, NodePropertyIds.IdleResourceCount);

            _BusyCoreCount = _NumberOfCores - _IdleCoreCount - _OfflineCoreCount;
        }

        int GetValueFromRow(PropertyRow row, PropertyId pid)
        {
            StoreProperty prop = row[pid];

            if (prop != null)
            {
                return (int)prop.Value;
            }

            return 0;
        }

        /// <summary>
        ///   <para>Retrieves the number of cores on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores on the node.</para>
        /// </value>
        public int NumberOfCores
        {
            get { return _NumberOfCores; }
        }

        /// <summary>
        ///   <para>Retrieves the number of sockets on the node.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of sockets on the node.</para>
        /// </value>
        public int NumberOfSockets
        {
            get { return _NumberOfSockets; }
        }

        /// <summary>
        ///   <para>Retrieves the number of cores on the node that are offline.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are offline.</para>
        /// </value>
        public int OfflineCoreCount
        {
            get { return _OfflineCoreCount; }
        }

        /// <summary>
        ///   <para>Retrieves the number of cores on the node that are idle.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are idle.</para>
        /// </value>
        public int IdleCoreCount
        {
            get { return _IdleCoreCount; }
        }

        /// <summary>
        ///   <para>Retrieves the number of cores on the node that are busy processing jobs.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of cores that are busy processing jobs.</para>
        /// </value>
        public int BusyCoreCount
        {
            get { return _BusyCoreCount; }
        }

    }
}
