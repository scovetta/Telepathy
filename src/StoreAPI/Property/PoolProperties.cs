using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the identifiers that uniquely identify the properties of a pool.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Use these identifiers when creating filters, specifying sort 
    /// orders, and using rowsets to retrieve specific properties from the database.</para>
    /// </remarks>
    [Serializable]
    public class PoolPropertyIds
    {
        // Make it so that no one can construct one of these.
        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.PoolPropertyIds" /> class.</para>
        /// </summary>
        protected PoolPropertyIds()
        {
        }

        /// <summary>
        ///   <para>The unique identifier that describes the Id number of the pool.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para>The unique identifier that describes the name of the pool.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Name
        {
            get { return StorePropertyIds.Name; }
        }

        /// <summary>
        ///   <para>The unique identifier that describes the weight of the pool.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        public static PropertyId Weight
        {
            get { return _Weight; }
        }

        /// <summary>
        ///   <para>The unique identifier that describes the internal pool object.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId PoolObject
        {
            get { return StorePropertyIds.PoolObject; }
        }

        /// <summary>
        ///   <para>The unique identifier that describes the number of cores in the cluster 
        /// guaranteed to the pool according to its weight and weights of other pools on the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Guarantee
        {
            get { return _Guarantee; }
        }

        /// <summary>
        ///   <para>The unique identifier that describes the number of allocated cores on the pool.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CurrentAllocation
        {
            get { return _CurrentAllocation; }
        }

        static PropertyId _Weight = new PropertyId(StorePropertyType.Int32, "Weight", PropertyIdConstants.PoolPropertyIdStart + 1);
        static PropertyId _Guarantee = new PropertyId(StorePropertyType.Int32, "Guarantee", PropertyIdConstants.PoolPropertyIdStart + 2, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
        static PropertyId _CurrentAllocation = new PropertyId(StorePropertyType.Int32, "CurrentAllocation", PropertyIdConstants.PoolPropertyIdStart + 3, PropFlags.Visible | PropFlags.Calculated | PropFlags.ReadOnly);
    }
}