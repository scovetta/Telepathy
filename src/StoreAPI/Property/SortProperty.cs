using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>An opaque interface that defines a property on which the lists of jobs, tasks, or nodes are sorted.</para>
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidISortProperty)]
    public interface ISortProperty
    {
        /// <summary>
        ///   <para>Retrieves the sort order in which the objects are returned.</para>
        /// </summary>
        /// <value>
        ///   <para>The sort order in which the objects are returned (for example, ascending or descending). For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder" /> enumeration.</para>
        /// </value>
        SortProperty.SortOrder Order { get; }

        /// <summary>
        ///   <para>Retrieves a property object that identifies the property used to sort the returned list.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that uniquely identifies the property used to sort the returned list. </para>
        /// </value>
        [ComVisible(false)]
        PropertyId Id { get; }
    }

    /// <summary>
    ///   <para>Defines a property on which the lists of jobs, tasks, or nodes are sorted.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.Properties.ISortProperty" /> interface.</para>
    /// </remarks>
    /// <example />
    [Serializable]
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidSortPropertyClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class SortProperty : ISortProperty
    {
        /// <summary>
        ///   <para>Defines the sort orders in which results can be sorted. </para>
        /// </summary>
        /// <remarks>
        ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
        /// need to use the numeric values for the enumeration members or create constants that  
        /// correspond to those members and set them equal to the numeric values. The 
        /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
        ///   <code language="vbs">const Ascending = 0
        /// const Descending = 1</code>
        /// </remarks>
        /// <example />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISortCollection.Add(Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder,Microsoft.Hpc.Scheduler.PropId)" 
        /// /> 
        [ComVisible(true)]
        [GuidAttribute(ComGuids.GuidSortOrderClass)]
        public enum SortOrder
        {
            /// <summary>
            ///   <para>Sort in ascending order. This enumeration member represents a value of 0.</para>
            /// </summary>
            Ascending,
            /// <summary>
            ///   <para>Sort in descending order. This enumeration member represents a value of 1.</para>
            /// </summary>
            Descending,
        }

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.SortProperty" /> class. </para>
        /// </summary>
        /// <param name="order">
        ///   <para>The order in which the results are sorted (for example, ascending or descending). For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder" /> enumeration.</para>
        /// </param>
        /// <param name="propId">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies the property used to sort the results.</para>
        /// </param>
        public SortProperty(SortOrder order, PropertyId propId)
        {
            _ord = order;
            _pid = propId;
        }

        /// <summary>
        ///   <para>Retrieves or sets the order in which the results are sorted.</para>
        /// </summary>
        /// <value>
        ///   <para>The order in which the results are sorted (for example, ascending or descending). For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.SortProperty.SortOrder" /> enumeration.</para>
        /// </value>
        public SortOrder Order
        {
            get { return _ord; }
            set { _ord = value; }
        }

        /// <summary>
        ///   <para>Retrieves or sets the identifier of the property used to sort the results.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies the property used to sort the results.</para>
        /// </value>
        [ComVisible(false)]
        public PropertyId Id
        {
            get { return _pid; }
            set { _pid = value; }
        }

        PropertyId _pid;
        SortOrder _ord;

        /// <summary>
        ///   <para>Retrieves a formatted string that represents the object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A formatted string that represents the object.</para>
        /// </returns>
        public override string ToString()
        {
            return _ord.ToString() + ": " + _pid.ToString();
        }
    }


}
