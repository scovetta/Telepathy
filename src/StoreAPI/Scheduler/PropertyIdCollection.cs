using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines a collection of property identifiers.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Use this interface when specifying the properties that you want 
    /// to retrieve using a rowset or rowset enumerator. For example, when calling  
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJobRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" /> or  
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJobEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection,Microsoft.Hpc.Scheduler.IFilterCollection,Microsoft.Hpc.Scheduler.ISortCollection)" />.</para> 
    /// </remarks>
    /// <example>
    ///   <para>For an example, see <see 
    /// href="https://msdn.microsoft.com/library/cc907078(v=vs.85).aspx">Using a Rowset to Enumerate a List of Objects</see>.</para> 
    /// </example>
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.NodePropertyIds" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIPropertyIdCollection)]
    public interface IPropertyIdCollection : ISchedulerCollection, ICollection<PropertyId>
    {
        //void Add(PropId propertyId);

        [ComVisible(false)]
        void AddPropertyId(PropertyId propId);

        /// <summary>
        ///   <para>Retrieves and array of property identifiers.</para>
        /// </summary>
        /// <returns>
        ///   <para>An array of <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> objects that identify the properties in the collection.</para>
        /// </returns>
        [ComVisible(false)]
        PropertyId[] GetIds();

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
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you can use to enumerate the items in the collection.</para>
        /// </returns>
        new IEnumerator GetEnumerator();

        /// <summary>
        ///   <para>Removes all property identifiers from the collection.</para>
        /// </summary>
        new void Clear();

        /// <summary>
        ///   <para>Retrieves the specified item from the collection.</para>
        /// </summary>
        /// <param name="index">
        ///   <para>The zero-based index of the item to retrieve from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies the property.</para>
        /// </returns>
        [ComVisible(false)]
        new PropertyId this[int index] { get; }
    }

    /// <summary>
    ///   <para>Defines a collection of property identifiers.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.IPropertyIdCollection" /> interface.</para>
    /// </remarks>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidPropertyIdCollectionClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class PropertyIdCollection : SchedulerCollection<PropertyId>, IPropertyIdCollection
    {
        [ComVisible(false)]
        public void AddPropertyId(PropertyId propertyId)
        {
            Add(propertyId);
        }

        /*
         public void Add(FilterOperator operation, PropId propId, object value)
         {
             Util.CheckArgumentNull(operation, "operation");

             PropertyId propertyId = PropertyLookup.PropertyIdFromPropIndex((int)propId);
             if (propertyId == null)
             {
                 throw new ArgumentException("Unkown property Id", "propertyId");
             }

             Add(operation, propertyId, value);
         }
         */

        /// <summary>
        ///   <para>Retrieves and array of property identifiers.</para>
        /// </summary>
        /// <returns>
        ///   <para>An array of <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> objects that identify the properties in the collection.</para>
        /// </returns>
        [ComVisible(false)]
        public PropertyId[] GetIds()
        {
            return List.ToArray();
        }

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you can use to enumerate the items in the collection.</para>
        /// </returns>
        IEnumerator IPropertyIdCollection.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }
    }
}
