using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines a collection of sort property objects used to sort the results when retrieving jobs, task, and nodes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateSortCollection" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.IFilterCollection" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISortCollection)]
    public interface ISortCollection : ISchedulerCollection, ICollection<SortProperty>
    {
        /// <summary>
        ///   <para>Adds a sort item to the collection using the specified operator and property object.</para>
        /// </summary>
        /// <param name="order">
        ///   <para>The order in which the results are returned (for example, ascending or descending). For possible values, see the <see cref="T:Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder" /> enumeration.</para>
        /// </param>
        /// <param name="propertyId">
        ///   <para>A <see cref="T:Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that uniquely identifies the property used to sort the results.</para>
        /// </param>
        /// <remarks>
        ///   <para>The order in which you add the properties to the collection determines the sort order of the results. For example, the first property is the primary sort key; the second property is the secondary sort key; and so on.</para>
        ///   <para>To specify the property identifiers, use the properties from the <see cref="T:Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />, <see cref="T:Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />, and <see cref="T:Microsoft.Hpc.Scheduler.Properties.NodePropertyIds" /> classes. However, use the <see cref="T:Microsoft.Hpc.Scheduler.PropId" /> enumeration to determine on which properties you can sort; the <see cref="T:Microsoft.Hpc.Scheduler.PropId" /> enumeration defines the only supported properties on which you can sort objects. </para>
        /// </remarks>
        [ComVisible(false)]
        void Add(SortProperty.SortOrder order, PropertyId propertyId);

        /// <summary>
        ///   <para>Adds a sort item to the collection using the specified operator and property identifier.</para>
        /// </summary>
        /// <param name="order">
        ///   <para>The order in which the results are returned (for example, ascending or descending). For possible values, see the <see cref="T:Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder" /> enumeration.</para>
        /// </param>
        /// <param name="propId">
        ///   <para>An identifier that uniquely identifies the property used to sort the results. For possible values, see <see cref="T:Microsoft.Hpc.Scheduler.PropId" /> enumeration.</para>
        /// </param>
        /// <remarks>
        ///   <para>The order in which you add the properties to the collection determines the sort order of the results. For example, the first property is the primary sort key; the second property is the secondary sort key; and so on.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/library/cc853433(v=vs.85).aspx">Filtering and Sorting Lists of Objects</see>.</para>
        /// </example>
        void Add(SortProperty.SortOrder order, PropId propId);

        /// <summary>
        ///   <para>Gets an array of the sort items from the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An array of <see cref="Microsoft.Hpc.Scheduler.Properties.SortProperty" /> objects that define the sort items in the collection.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Because the sort objects are opaque objects you should not try to enumerate the items in the collection.</para>
        /// </remarks>
        [ComVisible(false)]
        SortProperty[] GetSorts();

        /// <summary>
        ///   <para>Retrieves the number of items in the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items in the collection.</para>
        /// </value>
        new int Count { get; }

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the filters in the collection.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Because the sort objects are opaque objects you should not try to enumerate the items in the collection.</para>
        /// </remarks>
        new IEnumerator GetEnumerator();

        /// <summary>
        ///   <para>Removes all items from the collection.</para>
        /// </summary>
        /// <remarks>
        ///   <para />
        /// </remarks>
        new void Clear();

        /// <summary>
        ///   <para>Retrieves the specified sort item from the collection</para>
        /// </summary>
        /// <param name="index">
        ///   <para>The zero-based index of the sort item to retrieve from the collection</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.SortProperty" /> object that defines the sort item.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Because the sort objects are opaque objects you should not retrieve the sort objects from the collection.</para>
        /// </remarks>
        new SortProperty this[int index] { get; }
    }

    /// <summary>
    ///   <para>Defines a collection of sort property objects used to sort the results when retrieving jobs, task, and nodes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.ISortCollection" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidSortCollectionClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class SortCollection : SchedulerCollection<SortProperty>, ISortCollection
    {
        #region ISortCollection Members

        /// <summary>
        ///   <para>Adds a sort item to the collection using the specified operator and property object.</para>
        /// </summary>
        /// <param name="order">
        ///   <para>The sort order in which the results are returned (for example, ascending or descending). For possible values, see the <see cref="T:Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder" /> enumeration.</para>
        /// </param>
        /// <param name="propertyId">
        ///   <para>A <see cref="T:Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that uniquely identifies the property used to sort the results.</para>
        /// </param>
        /// <remarks />
        public void Add(SortProperty.SortOrder order, PropertyId propertyId)
        {
            Add(new SortProperty(order, propertyId));
        }

        /// <summary>
        ///   <para>Adds a sort item to the collection using the specified operator and property identifier.</para>
        /// </summary>
        /// <param name="order">
        ///   <para>The sort order in which the results are returned (for example, ascending or descending). For possible values, see the <see cref="T:Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder" /> enumeration.</para>
        /// </param>
        /// <param name="propId">
        ///   <para>An identifier that uniquely identifies the property used to sort the results. For possible values, see <see cref="T:Microsoft.Hpc.Scheduler.PropId" /> enumeration.</para>
        /// </param>
        /// <remarks />
        public void Add(SortProperty.SortOrder order, PropId propId)
        {
            Util.CheckArgumentNull(order, "order");

            PropertyId propertyId = PropertyUtil.PropertyIdFromPropIndex((int)propId);
            if (propertyId == null)
            {
                throw new ArgumentException("Unkown property Id", "propertyId");
            }

            Add(order, propertyId);
        }

        /// <summary>
        ///   <para>Gets an array of the sort items from the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An array of <see cref="Microsoft.Hpc.Scheduler.Properties.SortProperty" /> objects that define the sort items in the collection.</para>
        /// </returns>
        [ComVisible(false)]
        public SortProperty[] GetSorts()
        {
            return List.ToArray();
        }

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the filters in the collection.</para>
        /// </returns>
        IEnumerator ISortCollection.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        // We should not use the base directly because it is a generic type,and has problem for 64 bit COM

        int ISortCollection.Count
        {
            get
            {
                return base.Count;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        void ISortCollection.Clear()
        {
            base.Clear();
        }

        SortProperty ISortCollection.this[int index]
        {
            get
            {
                return base[index];
            }
        }
        #endregion
    }
}
