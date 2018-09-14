using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines a collection of string values.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To create an empty collection, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateStringCollection" /> method.</para>
    ///   <para>To get this interface, call the following properties and method:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.IRemoteCommand.NodeNames" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.IScheduler.GetJobTemplateList" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.AllocatedNodes" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.NodeGroups" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.RequestedNodes" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerNode.NodeGroups" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.AllocatedNodes" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.DependsOn" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.RequiredNodes" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIStringCollection)]
    public interface IStringCollection : ISchedulerCollection, ICollection<string>
    {
        /// <summary>
        ///   <para>Retrieves the number of items in the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items.</para>
        /// </value>
        new int Count { get; }

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the items in the collection.</para>
        /// </returns>
        new IEnumerator GetEnumerator();

        /// <summary>
        ///   <para>Retrieves the specified item from the collection.</para>
        /// </summary>
        /// <param name="index">
        ///   <para>The zero-based index of the string to retrieve from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para>The specified string from the collection.</para>
        /// </returns>
        new string this[int index] { get; }

        /// <summary>
        ///   <para>Removes all items from the collection.</para>
        /// </summary>
        new void Clear();

        /// <summary>
        ///   <para>Adds a string to the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The string item to add to the collection.</para>
        /// </param>
        new void Add(string item);

        /// <summary>
        ///   <para>Removes the first occurrence of the specified string from the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The string to remove.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the item is found and removed from the collection; otherwise, false.</para>
        /// </returns>
        new bool Remove(string item);

        /// <summary>
        ///   <para>Determines whether the collection contains the specified item.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to find.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the item exists; otherwise, false.</para>
        /// </returns>
        new bool Contains(string item);
    }

    /// <summary>
    ///   <para>Defines a collection of string values.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidStringCollectionClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class StringCollection : SchedulerCollection<string>, IStringCollection
    {
        /// <summary>
        ///   <para>Initializes a new, empty instance of this class.</para>
        /// </summary>
        public StringCollection()
        {
        }

        /// <summary>
        ///   <para>Initializes a new instance of this class using the specified list of strings.</para>
        /// </summary>
        /// <param name="list">
        ///   <para>An enumerable that contains a list of strings used to initialize this instance.</para>
        /// </param>
        public StringCollection(IEnumerable<string> list)
            : base(list)
        {
        }

        internal StringCollection(bool readOnly)
            : base(readOnly)
        {
        }

        internal StringCollection(IEnumerable<string> list, bool readOnly)
            : base(list, readOnly)
        {
        }

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the items in the collection.</para>
        /// </returns>
        IEnumerator IStringCollection.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        // We should not use the base directly because it is a generic type,and has problem for 64 bit COM

        int IStringCollection.Count
        {
            get
            {
                return base.Count;
            }
        }

        string IStringCollection.this[int index]
        {
            get { return base[index]; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        void IStringCollection.Clear()
        {
            base.Clear();
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="item">
        ///   <para />
        /// </param>
        void IStringCollection.Add(string item)
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
        bool IStringCollection.Remove(string item)
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
        bool IStringCollection.Contains(string item)
        {
            return base.Contains(item);
        }
    }
}
