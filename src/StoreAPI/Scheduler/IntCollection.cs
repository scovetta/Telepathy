using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines a collection of integer values. Typically, the collection contains job or node identifiers.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To create an empty collection, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateIntCollection" /> method.</para>
    ///   <para>To get this interface, call one of the following methods:</para>
    ///   <list type="nobullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobIdList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetNodeIdList(Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> 
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIIntCollection)]
    public interface IIntCollection : ISchedulerCollection, ICollection<int>
    {
        /// <summary>
        ///   <para>Retrieves the number of items in the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items.</para>
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
        new int this[int index] { get; }

        /// <summary>
        ///   <para>Removes all items from the collection.</para>
        /// </summary>
        new void Clear();

        /// <summary>
        ///   <para>Adds an item to the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The integer item to add to the collection.</para>
        /// </param>
        new void Add(int item);

        /// <summary>
        ///   <para>Removes the first occurrence of the specified item from the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The integer item to remove from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the item is found and removed from the collection; otherwise, false.</para>
        /// </returns>
        new bool Remove(int item);

        /// <summary>
        ///   <para>Determines whether the collection contains the specified item.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The integer item to find.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the item exists; otherwise, false.</para>
        /// </returns>
        new bool Contains(int item);
    }

    /// <summary>
    ///   <para>Defines a collection of integer values. Typically, the collection contains job or node identifiers.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.IIntCollection" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIntCollectionClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class IntCollection : SchedulerCollection<int>, IIntCollection
    {
        /// <summary>
        ///   <para>Initializes a new instance of this class.</para>
        /// </summary>
        public IntCollection()
        {
        }

        /// <summary>
        ///   <para>Initializes a new instance of this class using the specified collection.</para>
        /// </summary>
        /// <param name="list">
        ///   <para>An enumerable whose contents are used to initialize this collection.</para>
        /// </param>
        public IntCollection(IEnumerable<int> list)
            : base(list)
        {
        }

        /// <summary>
        ///   <para>Gets an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the items in the collection.</para>
        /// </returns>
        IEnumerator IIntCollection.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        // We should not use the base directly because it is a generic type,and has problem for 64 bit COM

        int IIntCollection.Count
        {
            get
            {
                return base.Count;
            }
        }

        int IIntCollection.this[int index]
        {
            get { return base[index]; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        void IIntCollection.Clear()
        {
            base.Clear();
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="item">
        ///   <para />
        /// </param>
        void IIntCollection.Add(int item)
        {
            base.Add(item);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="item">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        bool IIntCollection.Remove(int item)
        {
            return base.Remove(item);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="item">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        bool IIntCollection.Contains(int item)
        {
            return base.Contains(item);
        }
    }
}
