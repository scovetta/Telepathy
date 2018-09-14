using System;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the operators that you can use to compare a property value to a filter value.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code>const GreaterThan = 0
    /// const GreaterThanOrEqual = 1
    /// const LessThan = 2
    /// const LessThanOrEqual = 3
    /// const Equal = 4
    /// const NotEqual = 5
    /// const HasBitSet = 6
    /// const HasNoBitSet = 7
    /// const IsNull = 8
    /// const IsNotNull = 9
    /// const HasExclusiveBitSet = 10
    /// const In = 11
    /// const StartWith = 12</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.IFilterCollection.Add(Microsoft.Hpc.Scheduler.Properties.FilterOperator,Microsoft.Hpc.Scheduler.PropId,System.Object)" 
    /// /> 
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidFilterOperatorClass)]
    public enum FilterOperator
    {
        /// <summary>
        ///   <para>Include the object in the result if the property value 
        /// is greater than the filter value. This enumeration member represents a value of 0.</para>
        /// </summary>
        GreaterThan,
        /// <summary>
        ///   <para>Include the object in the result if the property value is greater 
        /// than or equal to the filter value. This enumeration member represents a value of 1.</para>
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        ///   <para>Include the object in the result if the property value 
        /// is less than the filter value. This enumeration member represents a value of 2.</para>
        /// </summary>
        LessThan,
        /// <summary>
        ///   <para>Include the object in the result if the property value is less 
        /// than or equal to the filter value. This enumeration member represents a value of 3.</para>
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        ///   <para>Include the object in the result if the property value equals the filter value. This enumeration member represents a value of 4.</para>
        /// </summary>
        Equal,
        /// <summary>
        ///   <para>Include the object in the result if the property value is 
        /// not equal to the filter value. This enumeration member represents a value of 5.</para>
        /// </summary>
        NotEqual,
        /// <summary>
        ///   <para>Include the object in the result if the property value has one or more bits set. For example, you can use this operator to test if the 
        /// state of the job is failed or canceled by using the bitwise OR to 
        /// set the filter value to both states. This enumeration member represents a value of 6.</para> 
        /// </summary>
        HasBitSet,
        /// <summary>
        ///   <para>Include the object in the result if the property value has no bits set. This enumeration member represents a value of 7.</para>
        /// </summary>
        HasNoBitSet,
        /// <summary>
        ///   <para>Include the object in the result if the property value is null. This enumeration member represents a value of 8.</para>
        /// </summary>
        IsNull,
        /// <summary>
        ///   <para>Include the object in the result if the property value is not null. This enumeration member represents a value of 9.</para>
        /// </summary>
        IsNotNull,
        /// <summary>
        ///   <para>Include the object in the result if the property value has a single bit set. This enumeration member represents a value of 10.</para>
        /// </summary>
        HasExclusiveBitSet,
        /// <summary>
        ///   <para>Include the object in the result if the property value equals one of the values in 
        /// an array of filter values. This enumeration member represents a value of 11. This value is only supported for Windows HPC Server 2008 R2.</para>
        /// </summary>
        In,     // To maintain the v3client -> v2HN compatibility, this is ONLY FOR INTERNAL USE!
                /// <summary>
                ///   <para>Include the object in the result if the property value starts with the 
                /// filter value. This enumeration member represents a value of 12. This value is only supported for Windows HPC Server 2008 R2.</para>
                /// </summary>
        StartWith,   // To maintain the v3client -> v2HN compatibility, this is ONLY FOR UI USE!
    }

}