using System.Runtime.InteropServices;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Contains information about a pool.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To retrieve the interface, call one of the following methods:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreatePool(System.String)" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenPool(System.String)" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISchedulerPool)]
    public interface ISchedulerPool
    {
        #region V3 SP2 methods
        /// <summary>
        ///   <para>Refreshes the properties of the scheduler pool object from the scheduler on the server. Any local changes will be lost.</para>
        /// </summary>
        void Refresh();

        /// <summary>
        ///   <para>Commits client side property changes to a pool object to the scheduler on the headnode.</para>
        /// </summary>
        void Commit();
        #endregion

        #region V3 SP2 properties

        /// <summary>
        ///   <para>Retrieves the name of the pool.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.String" /> object that contains the name of the pool.</para>
        /// </value>
        System.String Name { get; }

        /// <summary>
        ///   <para>Retrieves the Id number of the pool.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Int32" /> object that contains the Id of the pool.</para>
        /// </value>
        System.Int32 Id { get; }

        /// <summary>
        ///   <para>Sets and retrieves the weight of the pool.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns a <see cref="System.Int32" /> object that contains the weight of the pool..</para>
        /// </value>
        /// <remarks>
        ///   <para>The value range is an integer between 0 and 999999.</para>
        /// </remarks>
        System.Int32 Weight { get; set; }

        /// <summary>
        ///   <para>Retrieves the number of cores in the cluster guaranteed to 
        /// the pool according to its weight and weights of other pools on the cluster.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns a <see cref="System.Int32" /> object that contains the guaranteed number of cores..</para>
        /// </value>
        /// <remarks>
        ///   <para>This is a calculated property that is supplied by the scheduler store. </para>
        /// </remarks>
        System.Int32 Guarantee { get; }

        /// <summary>
        ///   <para>Retrieves the number of allocated cores that are being used by jobs that are running and belong to this pool.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns a <see cref="System.Int32" /> object that contains the number of allocated cores..</para>
        /// </value>
        /// <remarks>
        ///   <para>A pool with a weight of 0 has no guaranteed cores, but can have allocated cores if there are jobs that are submitted to the pool.</para>
        /// </remarks>
        System.Int32 CurrentAllocation { get; }

        #endregion

    }
}