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
    ///   <para>Defines a collection of filter property objects used to filter the results when retrieving jobs, task, and nodes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateFilterCollection" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISortCollection" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIFilterCollection)]
    public interface IFilterCollection : ISchedulerCollection, ICollection<FilterProperty>
    {
        void Add(FilterOperator operation, PropId propertyId, object value);

        [ComVisible(false)]
        void Add(FilterOperator operation, PropertyId propId, object value);

        /// <summary>
        ///   <para>Gets an array of the filters from the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An array of <see cref="Microsoft.Hpc.Scheduler.Properties.FilterProperty" /> objects that define the filters in the collection.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Because the filter objects are opaque objects you should not try to enumerate the filters in the collection.</para>
        /// </remarks>
        [ComVisible(false)]
        FilterProperty[] GetFilters();

        /// <summary>
        ///   <para>The number of items in the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items.</para>
        /// </value>
        new int Count { get; }

        /// <summary>
        ///   <para>An enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the filters in the collection.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Because the filter objects are opaque objects you should not try to enumerate the filters in the collection.</para>
        /// </remarks>
        new IEnumerator GetEnumerator();

        /// <summary>
        ///   <para>Removes all filters from the collection.</para>
        /// </summary>
        new void Clear();

        /// <summary>
        ///   <para>Retrieves the specified item from the collection.</para>
        /// </summary>
        /// <param name="index">
        ///   <para>The zero-based index of the item to retrieve from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.FilterProperty" /> object that defines the filter.
        /// </para>
        /// </returns>
        /// <remarks>
        ///   <para>Because the filter objects are opaque objects you should not try to get a filter from the collection.</para>
        /// </remarks>
        new FilterProperty this[int index] { get; }
    }

    /// <summary>
    ///   <para>Defines a collection of filter property objects used to filter the results when retrieving jobs, task, and nodes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.IFilterCollection" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidFilterCollectionClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class FilterCollection : SchedulerCollection<FilterProperty>, IFilterCollection
    {
        #region IFilterCollection Members

        [ComVisible(false)]
        public void Add(FilterOperator operation, PropertyId propertyId, object value)
        {
            if (value is ISchedulerCollection)
            {
                throw new SchedulerException(ErrorCode.Operation_InvalidFilterProperty, propertyId.Name);
            }
            Add(new FilterProperty(operation, propertyId, value));
        }

        public void Add(FilterOperator operation, PropId propId, object value)
        {
            Util.CheckArgumentNull(operation, "operation");

            PropertyId propertyId = PropertyUtil.PropertyIdFromPropIndex((int)propId);
            if (propertyId == null)
            {
                throw new ArgumentException("Unkown property Id", "propertyId");
            }

            Add(operation, propertyId, value);
        }

        /// <summary>
        ///   <para>Gets an array of the filters from the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An array of <see cref="Microsoft.Hpc.Scheduler.Properties.FilterProperty" /> objects that define the filters in the collection.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Because the filter objects are opaque objects you should not try to enumerate the filters in the collection.</para>
        /// </remarks>
        [ComVisible(false)]
        public FilterProperty[] GetFilters()
        {
            return List.ToArray();
        }

        /// <summary>
        ///   <para>An enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the filters in the collection.</para>
        /// </returns>
        IEnumerator IFilterCollection.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        // We should not use the base directly because it is a generic type,and has problem for 64 bit COM

        int IFilterCollection.Count
        {
            get
            {
                return base.Count;
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        void IFilterCollection.Clear()
        {
            base.Clear();
        }

        FilterProperty IFilterCollection.this[int index]
        {
            get
            {
                return base[index];
            }
        }
        #endregion
    }
}
