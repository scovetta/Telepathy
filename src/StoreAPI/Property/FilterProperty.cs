using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>An opaque interface that defines a filter to use for filtering lists of jobs, tasks, or nodes.</para>
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidIFilterProperty)]
    public interface IFilterProperty
    {
        /// <summary>
        ///   <para>Retrieves the operator used to compare the filter value to the property value.</para>
        /// </summary>
        /// <value>
        ///   <para>The operator used to compare the filter value to the property value. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.FilterOperator" /> enumeration.</para>
        /// </value>
        FilterOperator Operator { get; }

        /// <summary>
        ///   <para>Retrieves the property and property value used to filter the objects.</para>
        /// </summary>
        /// <value>
        ///   <para>The property and property value used to filter the objects. For details, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> class.</para>
        /// </value>
        [ComVisible(false)]
        StoreProperty Property { get; }
    }

    /// <summary>
    ///   <para>Defines a filter to use for filtering lists of jobs, tasks, or nodes.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.Properties.IFilterProperty" /> interface.</para>
    /// </remarks>
    /// <example />
    [Serializable]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidFilterPropertyClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class FilterProperty : IFilterProperty
    {
        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.FilterProperty" /> class using the specified operator, property identifier, and property value.</para> 
        /// </summary>
        /// <param name="operation">
        ///   <para>The operator to use when comparing the filter value to the property value. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.FilterOperator" /> enumeration.</para>
        /// </param>
        /// <param name="propertyId">
        ///   <para>The identifier of the property on which to filter the objects.</para>
        /// </param>
        /// <param name="valueFilter">
        ///   <para>The filter value used to compare to the property value.</para>
        /// </param>
        public FilterProperty(FilterOperator operation, PropertyId propertyId, object valueFilter)
        {
            op = operation;

            prop = new StoreProperty(propertyId, valueFilter);
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.FilterProperty" /> class using the specified operator and property object.</para>
        /// </summary>
        /// <param name="operation">
        ///   <para>The operator to use when comparing the filter value to the property value. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.FilterOperator" /> enumeration.</para>
        /// </param>
        /// <param name="jobProperty">
        ///   <para>The property and property value used to filter the objects. For details, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> class.</para>
        /// </param>
        public FilterProperty(FilterOperator operation, StoreProperty jobProperty)
        {
            // Note that null is an acceptable filter value.

            op = operation;
            prop = jobProperty;
        }

        /// <summary>
        ///   <para>Initializes an empty instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.FilterProperty" /> class.</para>
        /// </summary>
        public FilterProperty()
        {
            op = 0;
        }

        /// <summary>
        ///   <para>Retrieves or sets the operator to use when comparing the filter value to the property value.</para>
        /// </summary>
        /// <value>
        ///   <para>The operator used when comparing the filter value to the property value. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.FilterOperator" /> enumeration.</para>
        /// </value>
        public FilterOperator Operator
        {
            get { return op; }
            set { op = value; }
        }

        /// <summary>
        ///   <para>Retrieves or sets the property and property value used to filter the jobs, tasks, or nodes.</para>
        /// </summary>
        /// <value>
        ///   <para>The property and property value used to filter the objects. For details, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> class.</para>
        /// </value>
        [ComVisible(false)]
        public StoreProperty Property
        {
            get { return prop; }
            set { prop = value; }
        }

        FilterOperator op;

        StoreProperty prop;

        /// <summary>
        ///   <para>Retrieves a formatted string that represents the object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A formatted string that represents the object.</para>
        /// </returns>
        public override string ToString()
        {
            return op.ToString() + ": " + prop.ToString();
        }

    }



}
