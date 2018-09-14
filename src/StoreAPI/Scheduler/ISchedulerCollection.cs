using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines a generic collection of objects that the scheduler returns.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call one of the following methods:</para>
    ///   <list type="nobullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetNodeList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskIdList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetTaskList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection,System.Boolean)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.GetCores" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.IFilterCollection" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.INameValueCollection" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.IStringCollection" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.IIntCollection" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISortCollection" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISchedulerCollection)]
    public interface ISchedulerCollection : ICollection
    {
        /// <summary>
        ///   <para>Retrieves a count of the number of items in the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items in the collection.</para>
        /// </value>
        new int Count { get; }

        /// <summary>
        ///   <para>Gets an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the items in the collection.</para>
        /// </returns>
        new IEnumerator GetEnumerator();

        /// <summary>
        ///   <para>Retrieves the specified item from the collection.</para>
        /// </summary>
        /// <param name="index">
        ///   <para>The zero-based index of the item to retrieve from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para>The specified item from the collection.</para>
        /// </returns>
        object this[int index] { get; }

        /// <summary>
        ///   <para>Adds an item to the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to add to the collection. The type for all items in the collection must be the same.</para>
        /// </param>
        void Add(object item);

        /// <summary>
        ///   <para>Removes the first occurrence of the specified item from the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to remove from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para> Is true if the item is found and removed from the collection; otherwise, false.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The method uses <see cref="System.Collections.Generic.Comparer{T}.Default" /> to determine equality of the objects.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerCollection.Add(System.Object)" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerCollection.Clear" />
        bool Remove(object item);

        /// <summary>
        ///   <para>Determines whether the collection contains the specified item.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to find. The type of the item to find depends on the contents of the collection.</para>
        /// </param>
        /// <returns>
        ///   <para> Is true if the item is found in the collection; otherwise, false.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The method uses <see cref="System.Collections.Generic.Comparer{T}.Default" /> to determine equality of the objects.</para>
        /// </remarks>
        bool Contains(object item);

        /// <summary>
        ///   <para>Removes all items from the collection.</para>
        /// </summary>
        void Clear();
    }
}
