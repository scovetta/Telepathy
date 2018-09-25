//-----------------------------------------------------------------------
// <copyright file="MachineResourceInfo.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Resource information of a node</summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class is used when select candidate nodes for shrinking.
    /// </summary>
    internal class MachineResourceInfo : IComparable<MachineResourceInfo>
    {
        /// <summary>
        /// count of the occupied units
        /// </summary>
        private int occupiedUnitCount;

        /// <summary>
        /// Get/set the occupiedUnitCount
        /// </summary>
        public int OccupiedUnitCount
        {
            get
            {
                return this.occupiedUnitCount;
            }

            set
            {
                this.occupiedUnitCount = value;
            }
        }

        /// <summary>
        /// list of the idle dispatcher info
        /// </summary>
        private List<DispatcherInfo> idleDispatcherInfos = new List<DispatcherInfo>();

        /// <summary>
        /// Gets the list of the idle dispatcher info
        /// </summary>
        public List<DispatcherInfo> IdleDispatcherInfos
        {
            get
            {
                return this.idleDispatcherInfos;
            }
        }

        /// <summary>
        /// count of the idle units
        /// </summary>
        public int IdleUnitCount
        {
            get
            {
                return this.IdleDispatcherInfos.Count;
            }
        }

        /// <summary>
        /// count of the busy units
        /// </summary>
        public int BusyUnitCount
        {
            get
            {
                return this.OccupiedUnitCount - this.IdleUnitCount;
            }
        }

        /// <summary>
        /// Compare MachineResourceInfo objects by BusyUnitCount first and then by OccupiedUnitCount
        /// </summary>
        /// <param name="other">a MachineResourceInfo object to compare with this object</param>
        /// <returns>indicates the relative order</returns>
        public int CompareTo(MachineResourceInfo other)
        {
            int diff = this.BusyUnitCount - other.BusyUnitCount;
            if (diff != 0)
            {
                return diff;
            }
            else
            {
                return this.OccupiedUnitCount - other.OccupiedUnitCount;
            }
        }
    }
}
